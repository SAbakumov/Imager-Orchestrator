using DagOrchestrator.Models;
using StackExchange.Redis;
using System.Xml.Linq;

namespace DagOrchestrator.Services
{
    public class ImageCacheGCService
    {
        public Dictionary<Guid, List<string>> CoupledNodeIDs { get; set; } = new();
        public Dictionary<Guid, List<string>> CoupledImagePaths { get; set; } = new();

        private readonly IDatabase _db;

        public ImageCacheGCService(IDatabase db)
        {
            _db = db;
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

        public void InvokeNodeGC()
        {
            foreach (var node_refnode in CoupledNodeIDs)
            {
                if(CoupledNodeIDs[node_refnode.Key].Count==0)
                {
                    _db.KeyDelete(CoupledImagePaths[node_refnode.Key].Select( x=>
                    {
                        if(x.Contains("fromprovider"))
                        {
                            string result = x.StartsWith("fromprovider::")
                            ? x.Substring("fromprovider::".Length)
                            : x;
                            return new RedisKey(result);
                        }
                        return new RedisKey(x);
                    }).ToArray());
                }
            }
        }

        public static void CleanMemoryForNodeID(DagNode node, IDatabase db)
        {
            foreach(var image_inputs in node.InputParameters.Input)
            {

                string to_clear_keys = image_inputs.ImageDir;
                RedisKey rds_key;
                if (to_clear_keys.Contains("fromprovider"))
                {
                    string result = to_clear_keys.StartsWith("fromprovider::")
                       ? to_clear_keys.Substring("fromprovider::".Length)
                       : to_clear_keys;
                    rds_key = new RedisKey(result);
                }
                else
                {
                    rds_key = new RedisKey(to_clear_keys);
                }

                db.KeyDelete(rds_key);
            }
        }
    }
}
