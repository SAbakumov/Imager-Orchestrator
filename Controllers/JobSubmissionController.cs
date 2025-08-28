using DagOrchestrator.Services;
using Microsoft.AspNetCore.Mvc;

namespace DagOrchestrator.Controllers
{

    [ApiController]
    [Route("jobsumission/{dag_id}")]
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


        [HttpPost("submit_job")]
        public IActionResult SetData(string dag_id)
        {
            if(_jobService.IsJobForDagRunning(dag_id)) 
            {
                return BadRequest($"Job for ${dag_id} already running");
            } 
            else
            {
                _jobService.SubmitJob(dag_id);
                return Ok();
            }
        }

        [HttpPost("start")]
        public IActionResult StartProcessing(string dag_id)
        {
            var dag = _dagRegister.RetrieveProcessingPipeline(dag_id);
            foreach(var dagnode in dag)
            {
                dagnode.JobID = dag_id;
                dagnode.InputParameters.JobId = dag_id;

            }
            _jobService.SetJobStatusRunning(dag_id);    
            _dagProcessor.SubmitDag(dag);
            return Ok();
        }

        [HttpGet("get_job_result")]
        public IActionResult GetJobOutput(string dag_id)
        {
            var job = _jobService.GetJobResult(dag_id);

            if (job == null)
                return NotFound(new { status = "NotFound", output = "" });

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
