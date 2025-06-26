using DagOrchestrator.Services;
using Microsoft.AspNetCore.Mvc;

namespace DagOrchestrator.Controllers
{
    [ApiController]
    [Route("dagnodes/[controller]")]
    public class NodeInfoController : ControllerBase
    {

        private readonly PythonComService _pythonComService;

        public NodeInfoController(PythonComService pythonComService)
        {
            _pythonComService = pythonComService;
        }

        /// <summary>
        /// Retrieves a list of all available processing nodes from python backend
        /// </summary>        
        [HttpGet("get_nodes")]
        public async  Task<IActionResult> GetAvailableNodes()
        {
            string available_nodes = await _pythonComService.GetAvailableNodes();
            return Ok(available_nodes);
        }

        /// <summary>
        /// Retrieves processing function input/output parameters for requested Python API path.
        /// </summary>    
        [HttpPost("get_node_params")]
        public async  Task<IActionResult> GetNodeFunctionParameters([FromBody] FunctionRoute route)
        {
            string function_info = await _pythonComService.GetFunctionInfo(route.Route);
            return Ok(function_info);
        }
      
    }

    public class FunctionRoute
    {
        public string Route { get; set; }
    }
}
