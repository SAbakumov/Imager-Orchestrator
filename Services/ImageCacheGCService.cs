using DagOrchestrator.Models;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using StackExchange.Redis;
using System.Xml.Linq;

namespace DagOrchestrator.Services
{
    public class ImageCacheGCService
    {
        public Dictionary<Guid, List<string>> CoupledNodeIDs { get; set; } = new();
        public Dictionary<Guid, List<string>> CoupledImagePaths { get; set; } = new();
        private PythonComService _pythonComService;

        public ImageCacheGCService(PythonComService pythonComService)
        {
            _pythonComService = pythonComService;
        }

        public void TryAddNodeID(DagNode node)
        {
            CoupledNodeIDs.TryAdd(node.NodeId, new List<string>());
            CoupledImagePaths.TryAdd(node.NodeId, new List<string>());

            foreach (var output_node in node.OutputNodes)
            {
                foreach(string output_node_id in output_node)
                {
                    CoupledNodeIDs[node.NodeId].Add(output_node_id);
                }
            }
        }

        public void AddReferencePath(DagNode node, string referencePath)
        {
            CoupledImagePaths[node.NodeId].Add(referencePath);
        }

        public void RemoveReferenceNodeID(DagNode node)
        {
            foreach(var node_refnode in CoupledNodeIDs)
            {
                CoupledNodeIDs[node_refnode.Key].Remove(node.NodeId.ToString());
            }
        }

        public async Task InvokeNodeGC()
        {
            var to_remove_nodes = new List<Guid>();
            foreach (var node_refnode in CoupledNodeIDs)
            {


                if (CoupledNodeIDs[node_refnode.Key].Count == 0)
                {

                    var to_delete_Keys = CoupledImagePaths[node_refnode.Key].Select(x =>
                    {
                        if (x.Contains("fromprovider"))
                        {
                            string result = x.StartsWith("fromprovider::")
                            ? x.Substring("fromprovider::".Length)
                            : x;
                            return result;
                        }
                        return x;
                    }).ToArray();
                    foreach( var key in to_delete_Keys)
                    {
                        await _pythonComService.SubmitPythonAPIDeleteCall("/clear_key", key);
                    }
                    to_remove_nodes.Add(node_refnode.Key);
                }
            }
            foreach(var key in to_remove_nodes)
            {
                CoupledNodeIDs.Remove(key);
                CoupledImagePaths.Remove(key);
            }
        }

        public async Task CleanMemoryForNodeID(DagNode node)
        {
            foreach(var image_inputs in node.InputParameters.Input)
            {

                string to_clear_keys = image_inputs.ImageDir;
                string result;
                if (to_clear_keys.Contains("fromprovider"))
                { 
                    result = to_clear_keys.StartsWith("fromprovider::")
                       ? to_clear_keys.Substring("fromprovider::".Length)
                       : to_clear_keys;
                }
                else
                {
                    result = to_clear_keys;
                }

                await _pythonComService.SubmitPythonAPIDeleteCall("/clear_key", result);
                //db.KeyDelete(rds_key);
            }
            CoupledNodeIDs.Remove(node.NodeId);
            CoupledImagePaths.Remove(node.NodeId);

        }
    }
}
