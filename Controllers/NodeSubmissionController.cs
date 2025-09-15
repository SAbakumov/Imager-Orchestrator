using DagOrchestrator.Models;
using DagOrchestrator.Services;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace DagOrchestrator.Controllers
{
    [Route("nodesubmission")]
    [ApiController]
    public class NodeSubmissionController : ControllerBase
    {
        private readonly DagProcessingService _dagProcessor;
        private readonly DagRegisterService _dagRegister;

        public NodeSubmissionController(DagProcessingService dagProcessor, IConnectionMultiplexer rediscache, DagRegisterService dagRegister)
        {
            _dagProcessor = dagProcessor;
            _dagRegister = dagRegister;

            var server = rediscache.GetServer("dagorchestrator-redis-1", 6379);
            // Whenever new experiment run starts, we flush the DB to prevent uncleared data from interfering.
            server.FlushDatabase();
        }

        /// <summary>
        /// Submit multiple DAG definitions and store them in a register for further use.
        /// </summary>
        [HttpPost("set_dags")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult SetDags([FromBody] List<DagSubmissionRequest> dagDefinitions)
        {
            _dagRegister.ClearCachedProcessingPipelines();
            if (dagDefinitions == null || dagDefinitions.Count == 0)
                return BadRequest("No DAG definitions provided.");

            var errors = new List<string>();

            foreach (var dag in dagDefinitions)
            {
                if (string.IsNullOrWhiteSpace(dag.DagID) || dag.DagNodes == null || dag.DagNodes.Count == 0)
                {
                    errors.Add($"Invalid DAG: missing DagID or DagNodes.");
                    continue;
                }

                _dagRegister.CacheProcessingPipeline(dag.DagID, dag.DagNodes);
            }

            if (errors.Count > 0)
                return BadRequest(new { type = "status", status = "error", what =  errors });

            return Ok(new { type = "status", status = "success" });
        }

        /// <summary>
        /// Submit a single DAG for processing.
        /// </summary>
        [HttpPost("submit_dag")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult SubmitDag([FromBody] List<DagNode> dagNodes)
        {
            if (dagNodes == null || dagNodes.Count == 0)
                return BadRequest("No DAG nodes provided or invalid format.");

            _dagProcessor.SubmitDag(dagNodes);
            return Ok(new { Message = "DAG submitted successfully." });
        }
    }

    public class DagSubmissionRequest
    {
        public string DagID { get; set; }
        public List<DagNode> DagNodes { get; set; }
    }
}
