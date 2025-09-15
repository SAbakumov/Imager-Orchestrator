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

        public List<DagNode> RetrieveProcessingPipeline(string dagid)
        {

            ProcessingPipelines.TryGetValue(dagid, out var result);
            return result;
        }

        internal void ClearCachedProcessingPipelines()
        {
            ProcessingPipelines.Clear();
        }
    }
}



    //    public static string DefaultJob = @"[
    //  {
    //    ""node_id"": ""7bb7b739-b71e-4733-bce6-ee9a7a9977ce"",
    //    ""input_nodes"": [],
    //    ""input_parameters"": {
    //      ""node_id"": ""7bb7b739-b71e-4733-bce6-ee9a7a9977ce"",
    //      ""input"": [
    //        {
    //          ""input_type"": ""AcquisitionName"",
    //          ""input_params"": {
    //            ""input_json_params"": {
    //              ""value"": ""NewAcq"",
    //              ""name"": ""Input Acquisition""
    //            }
    //          }
    //        },
    //        {
    //          ""input_type"": ""DetectorName"",
    //          ""input_params"": {
    //            ""input_json_params"": {
    //              ""value"": ""DummyCam1"",
    //              ""name"": ""Input Detector""
    //            }
    //          }
    //        }
    //      ]
    //    },
    //    ""output_nodes"": [
    //      [
    //        ""fe2bda93-35f9-487e-a1b3-dc09456cc28d""
    //      ]
    //    ],
    //    ""isoutputnode"": false,
    //    ""isinputnode"": true,
    //    ""api_path"": ""/api/io/live_input""
    //  },
    //  {
    //    ""node_id"": ""fe2bda93-35f9-487e-a1b3-dc09456cc28d"",
    //    ""input_nodes"": [
    //      ""7bb7b739-b71e-4733-bce6-ee9a7a9977ce""
    //    ],
    //    ""input_parameters"": {
    //      ""node_id"": ""fe2bda93-35f9-487e-a1b3-dc09456cc28d"",
    //      ""input"": [
    //        {
    //          ""input_type"": ""Image2D"",
    //          ""isinputnode"": true,
    //          ""input_params"": {
    //            ""input_json_params"": {
    //              ""image_dir"": ""7bb7b739-b71e-4733-bce6-ee9a7a9977ce""
    //            }
    //          }
    //        }
    //      ]
    //    },
    //    ""output_nodes"": [
    //      [
    //        ""7143241c-96e8-4c5d-ac53-ac86be2ad25e""
    //      ]
    //    ],
    //    ""isoutputnode"": false,
    //    ""isinputnode"": false,
    //    ""api_path"": ""/api/processing/calculate_mean""
    //  },
    //  {
    //    ""node_id"": ""7143241c-96e8-4c5d-ac53-ac86be2ad25e"",
    //    ""input_nodes"": [
    //      ""fe2bda93-35f9-487e-a1b3-dc09456cc28d""
    //    ],
    //    ""input_parameters"": {
    //      ""node_id"": ""7143241c-96e8-4c5d-ac53-ac86be2ad25e"",
    //      ""input"": [
    //        {
    //          ""input_type"": ""Image2D"",
    //          ""isinputnode"": true,
    //          ""input_params"": {
    //            ""input_json_params"": {
    //              ""image_dir"": ""fe2bda93-35f9-487e-a1b3-dc09456cc28d""
    //            }
    //          }
    //        }
    //      ]
    //    },
    //    ""output_nodes"": [
    //      [
    //        ""e5f70b1a-ab04-4b6f-83ac-815231c7b547""
    //      ]
    //    ],
    //    ""isoutputnode"": false,
    //    ""isinputnode"": false,
    //    ""api_path"": ""/api/processing/set_wait_time""
    //  },
    //  {
    //    ""node_id"": ""e5f70b1a-ab04-4b6f-83ac-815231c7b547"",
    //    ""input_nodes"": [
    //      ""7143241c-96e8-4c5d-ac53-ac86be2ad25e""
    //    ],
    //    ""input_parameters"": {
    //      ""node_id"": ""e5f70b1a-ab04-4b6f-83ac-815231c7b547"",
    //      ""input"": [
    //        {
    //          ""input_type"": ""MeasurementElement"",
    //          ""isinputnode"": true,
    //          ""input_params"": {
    //            ""input_json_params"": {
    //              ""image_dir"": ""7143241c-96e8-4c5d-ac53-ac86be2ad25e""
    //            }
    //          }
    //        }
    //      ]
    //    },
    //    ""output_nodes"": [],
    //    ""isoutputnode"": true,
    //    ""isinputnode"": false,
    //    ""api_path"": ""/api/io/measurement_output""
    //  }
    //]";
