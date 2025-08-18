using DagOrchestrator.Models;
using DagOrchestrator.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace DagOrchestrator.Controllers
{
    [Route("nodesubmission")]
    [ApiController]
    public class NodeSubmissionController : ControllerBase
    {
        private readonly DagProcessingService _dagProcessor;

        public NodeSubmissionController(DagProcessingService dagProcessor) 
        {
            _dagProcessor = dagProcessor;
        }

        /// <summary>
        /// Submit a DAG containing various nodes and deserialize them into the DagNode object.
        /// Starts the DAG processing
        /// </summary>    
        /// 
        [HttpPost("submit_dag")]
        public async Task<IActionResult> SubmitDagFromRequest([FromBody] JArray dag_nodes)
        {

            var sw = Stopwatch.StartNew();

            // System.Text.Json replacement for JObject.ToObject<List<T>>
            var dagList = dag_nodes.ToObject<List<DagNode>>();


            if (dagList != null)
            {
                _dagProcessor.SubmitDag(dagList);
                sw.Stop();

                //_dagScheduler.StartSubmission();
                return Ok();
            }
            return NotFound("Could not find a list of DAG nodes");
        } 
    }
}
