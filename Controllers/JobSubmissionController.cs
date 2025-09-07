using DagOrchestrator.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace DagOrchestrator.Controllers
{

    [ApiController]
    [Route("jobsubmission")]
    public class JobSubmissionController : Controller
    {
        private readonly JobSubmissionService _jobService;
        private readonly DagRegisterService _dagRegister;
        private readonly DagProcessingService _dagProcessor;


        public JobSubmissionController(JobSubmissionService jobService, DagRegisterService dagRegister, DagProcessingService dagProcessor)
        {
            _jobService = jobService;  
            _dagRegister = dagRegister; 
            _dagProcessor = dagProcessor;
        }



        [HttpGet("get_job_result")]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetJobOutput([FromQuery] string dagid)
        {
            var cts = new CancellationTokenSource(TimeSpan.FromMinutes(20));

            try
            {
                while (_jobService.HasJobsInQueue())
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
                return NotFound(new { status = "NotFound", output = "" });

            if (job.LazyNode != null)
            {
                job.JobOutput = await Task.Run(() => _dagProcessor.ExecuteSingleNode(job.LazyNode));
                job.Status = JobStatus.Completed;
            }


            switch (job.Status)
            {
                case JobStatus.Completed:
                    return Ok(new
                    {
                        status = job.Status.ToString(),
                        output = job.JobOutput
                    });

                case JobStatus.Running:
                case JobStatus.Pending:
                    return Accepted(new
                    {
                        status = job.Status.ToString(),
                        output = ""
                    });

                case JobStatus.Failed:
                    return StatusCode(StatusCodes.Status500InternalServerError, new
                    {
                        status = job.Status.ToString(),
                        output = ""
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
