using DagOrchestrator.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Security.Principal;

namespace DagOrchestrator.Services
{
    public class DagRegisterService
    {
        private ConcurrentDictionary<string, List<DagNode>> ProcessingPipelines = new();

        public DagRegisterService()
        {
 
        }

        public void CacheProcessingPipeline(string id, List<DagNode> dag_nodes)
        {
            ProcessingPipelines.TryAdd(id, dag_nodes);
        }

        public List<DagNode> RetrieveProcessingPipeline(string id)
        {
            ProcessingPipelines.TryGetValue(id, out var result);
            return result;
        }
    }
}
