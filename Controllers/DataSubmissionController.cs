using DagOrchestrator.Models;
using DagOrchestrator.Services;
using MessagePack;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Metadata;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DagOrchestrator.Controllers
{

    [ApiController]
    public class DataSubmissionController : Controller
    {
        private readonly DagRegisterService _dagRegister;
        private readonly IConnectionMultiplexer _imageCache;
        private readonly IDatabase _db;
        private readonly PythonComService _pythonComService;
        private readonly JobSubmissionService _jobSubmissionService;



        public DataSubmissionController( DagRegisterService dagRegister, IConnectionMultiplexer cache,PythonComService pythonComService, JobSubmissionService jobService)
        {
            _imageCache = cache;
            _db = _imageCache.GetDatabase();    
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
                ReadOnlyMemory<byte> mem = image.data.imagedata;

                tasks.Add(batch.StringSetAsync($"{jobid}_{image.metadata.acquisitiontype}_{image.data.detectorname}", mem));

                var hashKey = $"{jobid}_{image.metadata.acquisitiontype}_{image.data.detectorname}_size";
                var entries = new HashEntry[]
                {
                new("nrows", image.data.nrows),
                new("ncols", image.data.ncols)
                };
                tasks.Add(batch.HashSetAsync(hashKey, entries));
            }

            batch.Execute();

            Task.WhenAll(tasks).Wait();

            var dag = _dagRegister.RetrieveProcessingPipeline(DagId);

            if (dag != null)
            {
                var copied_dag = dag.Select(x => x.DeepCopy()).ToList();

                foreach (var dagnode in dag)
                {
                    dagnode.JobID = jobid;
                    dagnode.InputParameters.JobId = jobid;

                }
            }


            _jobSubmissionService.SubmitJob(new JobDefinition(jobid,DagId, dag));
            return Ok(new { Message = $"{{received : {validLength} }}" });
        }
    }
}
