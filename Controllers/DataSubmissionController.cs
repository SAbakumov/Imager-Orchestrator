using DagOrchestrator.Models;
using DagOrchestrator.Services;
using MessagePack;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Metadata;
using static System.Net.Mime.MediaTypeNames;

namespace DagOrchestrator.Controllers
{

    [ApiController]
    public class DataSubmissionController : Controller
    {
        private readonly DagRegisterService _dagRegister;

        private readonly PythonComService _pythonComService;
        private readonly JobSubmissionService _jobSubmissionService;
        private readonly ImageSubmissionService _imageSubmissionService;
        private readonly Uri jobIdSetter;



        public DataSubmissionController( DagRegisterService dagRegister,PythonComService pythonComService, JobSubmissionService jobService, 
            ImageSubmissionService imService)
        {

            _dagRegister = dagRegister;
            _pythonComService = pythonComService;
            _jobSubmissionService = jobService;
            _imageSubmissionService = imService;

            string serviceUrl = _pythonComService.GetPythonAdress().ToString();

            UriBuilder builder = new UriBuilder(serviceUrl)
            {
                Path = "set_jobid"
            };

            jobIdSetter = builder.Uri;
        }
        private List<MessagePackData> StartMessagePackReader(byte[] receivedData, int bufferlength)
        {
            MessagePackReader reader = new MessagePackReader(new ReadOnlySequence<byte>(receivedData));
            List<MessagePackData> messagePackDatas = new List<MessagePackData>();

            while (reader.Consumed <= bufferlength - 5)
            {
                messagePackDatas.Add(MessagePackSerializer.Deserialize<MessagePackData>(ref reader));
            }


            return messagePackDatas;
        }



        /// <summary>
        /// Data submission endpoint. Expects data in valid MessagePack format
        /// </summary> 
        [HttpPost("submit_data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SubmitData([FromQuery] string DagId)
        {
            var stopwatch = Stopwatch.StartNew();

            using var memoryStream = new MemoryStream();
            await Request.Body.CopyToAsync(memoryStream, 2*2048*2048);
            byte[] receivedData = memoryStream.GetBuffer();
            int validLength = (int)memoryStream.Length;


            List<MessagePackData> deserializedMessages = StartMessagePackReader(receivedData, validLength);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
            };


            var tasks = new List<Task>();
            //var batch = _db.CreateBatch();
            var jobid = Guid.NewGuid().ToString();

            foreach (var image in deserializedMessages)
            {

                if (_jobSubmissionService.HasPendingPipeline(DagId, image.message.metadata.detectionindex) is JobDefinition pending_job)
                {
                    jobid = pending_job.JobID;
                }

                _imageSubmissionService.Enqueue(new JobImageData(receivedData, jobid));

            }





            foreach (var image in deserializedMessages)
            {
                if (_jobSubmissionService.HasPendingPipeline(DagId, image.message.metadata.detectionindex) is JobDefinition pending_job)
                {
                    pending_job.ReceivedImages++;
                    if (pending_job.ReceivedImages == pending_job.MaxImages)
                    {

                        _jobSubmissionService.SubmitJob(pending_job);
                        _jobSubmissionService.RemovePendingJob(pending_job);
                    }
                }
                else
                {
                    var dag = _dagRegister.RetrieveProcessingPipeline(DagId);

                    if (dag != null)
                    {
                        var copied_dag = dag.Select(x => x.DeepCopy()).ToList();

                        foreach (var dagnode in copied_dag)
                        {
                            dagnode.JobID = jobid;
                            dagnode.InputParameters.JobId = jobid;

                        }


                        var new_pending_job = new JobDefinition(jobid, DagId, image.message.metadata.detectionindex,
                            image.message.metadata.nimageswithdetectionindex, copied_dag);

                        new_pending_job.ReceivedImages++;
                        if (new_pending_job.ReceivedImages == new_pending_job.MaxImages)
                        {

                            _jobSubmissionService.SubmitJob(new_pending_job);
                        }
                        else
                        {
                            _jobSubmissionService.AddPendingJob(new_pending_job);
                        }
                    }
                }

            }
            return Ok(new { type = "status", status = "success" });
        }
    }



}
