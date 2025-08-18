using DagOrchestrator.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Security.Principal;

namespace DagOrchestrator.Services
{
    public class DagProcessingService : BackgroundService
    {
        private readonly IDagScheduler _dagScheduler;
        private readonly PythonComService _pythonComService;

        public DagProcessingService(IDagScheduler dagScheduler, PythonComService pythonComService)
        {
            _dagScheduler = dagScheduler;
            _pythonComService  = pythonComService;
        }

        public void SubmitDag(List<DagNode> dagNodes)
        {
            _dagScheduler.SetDagNodes(dagNodes); 
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var node = _dagScheduler.RetrieveSubmissionReadyNode();
                if (node != null && node.ApiPath!=null && node.InputParameters!=null)
                {
                    var input_params = JObject.FromObject(node.InputParameters);
                    string json = JsonConvert.SerializeObject(input_params, Formatting.None);

                    string image_response = await  _pythonComService.SubmitImagerAPICall(node.ApiPath, json);

                    List<ResultOutput> image_output_params = JsonConvert.DeserializeObject<List<ResultOutput>>(image_response);
                    if (image_output_params != null)
                    {

                        for (int i = 0; i < image_output_params.Count; i++)
                        {
                            foreach (var output_node_id in node.OutputNodes[i])
                            {
                                try
                                {
                                    var output_node = _dagScheduler.RetrieveNodeByNodeID(output_node_id);

                                    foreach (var item in output_node.InputParameters.Input.Where(x => x.ImageDir == node.NodeId.ToString()))
                                    {
                                        item.ImageDir = image_output_params[i].image_dir;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex.Message);
                                }

                            }
                        }
                    }
                    _dagScheduler.RemoveNode(node);
                    //_dagScheduler.ProcessNode(node);
                }
                else
                {
                    await Task.Delay(500, stoppingToken); // wait before next check
                }
            }
        }
    }

    public class ResultOutput
    {
        public string datatype { get; set; } 
        public string image_dir { get; set; }   
    }
}
