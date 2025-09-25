using DagOrchestrator.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using StackExchange.Redis;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DagOrchestrator.Controllers
{

    [ApiController]
    [Route("decisions")]
    public class JobSubmissionController : Controller
    {
        private readonly JobSubmissionService _jobService;
        private readonly DagRegisterService _dagRegister;
        private readonly DagProcessingService _dagProcessor;
        private readonly PythonComService _pythonService;
        private readonly ImageCacheGCService _imageCacheGCService;


        public JobSubmissionController(JobSubmissionService jobService, DagRegisterService dagRegister, DagProcessingService dagProcessor, 
            PythonComService pythonService, ImageCacheGCService imageCacheGCService)
        {
            _jobService = jobService;  
            _dagRegister = dagRegister; 
            _dagProcessor = dagProcessor;
            _pythonService = pythonService;
            _imageCacheGCService = imageCacheGCService;

        }



        [HttpGet("get_decision/{decisiontype}")]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetJobOutput(
            string decisiontype,        
            [FromQuery] string dagid)
        {
            var cts = new CancellationTokenSource(TimeSpan.FromMinutes(20));

            try
            {
                while (_jobService.HasJobsInQueue(dagid))
                {
                    await Task.Delay(500, cts.Token);
                }

            }
            catch (TaskCanceledException)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    status = "Failed to produce valid output on 20 minute timeout",
                    output = ""
                });
            }
            var job = _jobService.GetJobResult(dagid);


            if (job == null)
                return NotFound();

            if (job.LazyNode != null)
            {
                var response = await Task.Run(() => _dagProcessor.ExecuteSingleNode(job.LazyNode, dagid));
                job.JobOutput = JObject.Parse(response);
                job.Status = JobStatus.Completed;
                await _imageCacheGCService.CleanMemoryForNodeID(job.LazyNode);
            }


            if (job.JobOutput.TryGetValue("type", out var jobdecisiontype))
            {
                var string_jobdecisiontype = jobdecisiontype.ToString(Newtonsoft.Json.Formatting.None).Trim('"');
                if (string_jobdecisiontype != decisiontype)
                {
                    return BadRequest(new { type = "status", status = "error", 
                        what = $"requested decision type {decisiontype} does not match job return type {jobdecisiontype}" });
                }
            }
            else
            {
                return StatusCode(500, new { type = "status", status = "error",
                    what = "decision type could not be inferred from job output" });
            }
            await _pythonService.SubmitPythonAPIGetCall("/purge_state", dagid);

            switch (job.Status)
            {
                case JobStatus.Completed:
                    return Ok(job.JobOutput);

                case JobStatus.Running:
                case JobStatus.Pending:
                    return Accepted(new
                    {
                        status = job.Status.ToString(),
                        output = ""
                    });

                case JobStatus.Failed:
                    return StatusCode(500, new
                    {
                        type = "status",
                        status = "error",
                        what = $"job with DagID {dagid} failed."
                    });

                default:
                    return Ok(new
                    {
                        status = job.Status.ToString(),
                        output = ""
                    });
            }
        }
    }
}
