using DagOrchestrator.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DagOrchestrator.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DagOrchestrator.Controllers
{
    [Route("nodesubmission")]
    [ApiController]
    public class NodeSubmissionController : ControllerBase
    {
        /// <summary>
        /// Submit a DAG containing various nodes and deserialize them into the DagNode object.
        /// Starts the DAG processing
        /// </summary>    
        [HttpPost("submit_dag")]
        public async Task<IActionResult> SubmitDagFromRequest([FromBody] JArray dag_nodes)
        {
            var dagList = dag_nodes.ToObject<List<DagNode>>();
            return Ok(); 
        } 
    }
}
