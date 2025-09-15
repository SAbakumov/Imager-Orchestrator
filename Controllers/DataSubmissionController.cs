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

namespace DagOrchestrator.Controllers
{

    [ApiController]
    public class DataSubmissionController : Controller
    {
        private readonly DagRegisterService _dagRegister;
        private readonly IConnectionMultiplexer _redisImageCache;
        private readonly IDatabase _db;
        private readonly PythonComService _pythonComService;
        private readonly JobSubmissionService _jobSubmissionService;



        public DataSubmissionController( DagRegisterService dagRegister, IConnectionMultiplexer cache,PythonComService pythonComService, JobSubmissionService jobService)
        {
            _redisImageCache = cache;
            _db = _redisImageCache.GetDatabase();    
            _dagRegister = dagRegister;
            _pythonComService = pythonComService;
            _jobSubmissionService = jobService;
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


        [HttpGet("submit_data_warmup")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SubmitData()
        {

            return Ok(new { Message = "{status : connected}" });
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
            await Request.Body.CopyToAsync(memoryStream);
            byte[] receivedData = memoryStream.GetBuffer();
            int validLength = (int)memoryStream.Length;

            List<MessagePackData> deserializedMessages = StartMessagePackReader(receivedData, validLength);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
            };


            var tasks = new List<Task>();
            var batch = _db.CreateBatch();
            var jobid = Guid.NewGuid().ToString();

            foreach (var image in deserializedMessages)
            {

                ReadOnlyMemory<byte> mem = image.message.data.imagedata;

                tasks.Add(batch.StringSetAsync($"{jobid}_{image.message.metadata.acquisitiontype}_{image.message.data.detectorname}", mem));

                var hashKeySize = $"{jobid}_{image.message.metadata.acquisitiontype}_{image.message.data.detectorname}_size";
                var entriesSize = new HashEntry[]
                {
                new("nrows", image.message.data.nrows),
                new("ncols", image.message.data.ncols)
                };
                tasks.Add(batch.HashSetAsync(hashKeySize, entriesSize));

                var hashKeyPosition = $"{jobid}_{image.message.metadata.acquisitiontype}_{image.message.data.detectorname}_position";
                var entriesPosition = new HashEntry[]
                {
                new("x", image.message.metadata.stageposition.x),
                new("y", image.message.metadata.stageposition.y),
                new("z", image.message.metadata.stageposition.z),
                new("usinghardwareautofocus", image.message.metadata.stageposition.usinghardwareautofocus),
                new("offset", image.message.metadata.stageposition.hardwareautofocusoffset)

                };
                tasks.Add(batch.HashSetAsync(hashKeyPosition, entriesPosition));

            }

            batch.Execute();

            Task.WhenAll(tasks).Wait();



            foreach (var image in deserializedMessages)
            {
                if(_jobSubmissionService.HasPendingPipeline(DagId, image.message.metadata.detectionindex) is JobDefinition pending_job)
                {
                    pending_job.ReceivedImages++;
                    if(pending_job.ReceivedImages == pending_job.MaxImages)
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
