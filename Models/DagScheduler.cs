using DagOrchestrator.Services;
using Newtonsoft.Json.Linq;
using System.Xml.Linq;

namespace DagOrchestrator.Models
{
    public interface IDagScheduler
    {
        public string DagId { get; set; }
        public void StartSubmission();
        public void AppendDagNodes(List<DagNode> dagNodes);
        public DagNode? RetrieveSubmissionReadyNode();
        public void RemoveNode(DagNode node);
        public DagNode? RetrieveNodeByNodeID(string nodeid);

        void RemoveNodesWithJobId(string? jobID);
        void SetCurrentJob(List<DagNode> dagNodes, string dagId);
    }

    public class DagScheduler : IDagScheduler
    {
        private readonly object _lock = new();
        public string DagId { get; set; }
        private readonly PythonComService _pythonComService;

        List<DagNode> DagNodes = new();

        public DagScheduler(PythonComService pythonComService)
        {
            _pythonComService = pythonComService;   
        }

        public void AppendDagNodes(List<DagNode> dagNodes)
        {
            DagNodes.AddRange(dagNodes);
        }
        public void StartSubmission()
        {

            var dagnode = RetrieveSubmissionReadyNode();

        }
        public void RemoveNode(DagNode node)
        {
            DagNodes.Remove(node);
        }

        public DagNode? RetrieveSubmissionReadyNode()
        {
            lock (_lock)
            {
                foreach (var node in DagNodes)
                {
                    if(node.InputNodes.Count==0)
                    {
                        return node;
                    }
                    var parameters = node.InputParameters?.Input ?? new List<InputParameter>();
                    var input_paths = parameters.Where(x => x.IsInputNode?.Value<bool>() ?? false).ToList();
                    bool are_all_paths_occupied = input_paths.Select(x => x.IsAssigned).All(x => x);
                    if (are_all_paths_occupied)
                    {
                        return node;
                    }
                }
            }
            return null;
        }
        public DagNode? RetrieveNodeByNodeID(string id)
        {
            return DagNodes.First(x => x.NodeId.ToString() == id);
        }

        public void RemoveNodesWithJobId(string? jobID)
        {
            DagNodes.RemoveAll(x => x.JobID==jobID);
        }


        public void SetCurrentJob(List<DagNode> dagNodes, string dagId)
        {
            DagNodes = dagNodes;
            DagId = dagId;
        }
    }
}
