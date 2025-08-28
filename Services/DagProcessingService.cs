using DagOrchestrator.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Net;
using System.Security.Principal;

namespace DagOrchestrator.Services
{
    public class DagProcessingService : BackgroundService
    {
        private readonly IDagScheduler _dagScheduler;
        private readonly PythonComService _pythonComService;
        private readonly JobSubmissionService _jobSubmissionService;

        public DagProcessingService(IDagScheduler dagScheduler, PythonComService pythonComService, JobSubmissionService jobSubmissionService)
        {
            _jobSubmissionService = jobSubmissionService;
            _dagScheduler = dagScheduler;
            _pythonComService  = pythonComService;
        }

        public void SubmitDag(List<DagNode> dagNodes)
        {
            _dagScheduler.AppendDagNodes(dagNodes); 
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

                    var python_response = await  _pythonComService.SubmitImagerAPICall(node.ApiPath, json);
                    var image_response =  await  python_response.Content.ReadAsStringAsync();

                    if (python_response.StatusCode == HttpStatusCode.InternalServerError)
                    {
                        _dagScheduler.RemoveNodesWithJobId(node.JobID);
                        _jobSubmissionService.SetJobStatusFailed(node.JobID, image_response);
                        node = null;
                    }


                    if (node!=null && node.IsOutputNode != null && !(bool)node.IsOutputNode)
                    {

                        JArray python_output_array = JArray.Parse(image_response);

                        var image_output_params = new List<PythonOutput?>();
                        foreach (var output in python_output_array)
                        {
                            switch (output.SelectToken("datatype")?.ToString())
                            {
                                case "Image2D":
                                    image_output_params.Add(output.ToObject<ImageResultOutput>());
                                    break;
                                case "MeasurementElement":
                                    image_output_params.Add(output.ToObject<ElementResultOutput>());
                                    break;
                            }
                        }

                        if (image_output_params != null)
                        {

                            for (int i = 0; i < image_output_params.Count; i++)
                            {
                                foreach (var output_node_id in node.OutputNodes[i])
                                {

                                    var output_node = _dagScheduler.RetrieveNodeByNodeID(output_node_id);

                                    foreach (var item in output_node.InputParameters.Input.Where(x => x.ImageDir == node.NodeId.ToString()))
                                    {
                                        item.ImageDir = image_output_params[i] switch
                                        {
                                            ImageResultOutput imageOutput => imageOutput.image_dir,
                                            ElementResultOutput elementOutput => elementOutput.elementproperties.ToString(Formatting.None),
                                            _ => item.ImageDir
                                        };

                                        item.IsAssigned = true;
                                    }
                                }
                            }
                        }
                        _dagScheduler.RemoveNode(node);
                    }
                    else
                    {
                        if (node != null && (bool)node.IsOutputNode!)
                        {
                            _jobSubmissionService.SetJobResult(image_response, node.JobID);
                            _jobSubmissionService.SetJobStatusCompleted(node.JobID);
                            _dagScheduler.RemoveNodesWithJobId(node.JobID);
                        }
                    }
                }
                else
                {
                    await Task.Delay(500, stoppingToken); // wait before next check
                }
            }
        }
    }

    public abstract class PythonOutput
    {
        public abstract string datatype { get; set; }
    }

    public class ImageResultOutput : PythonOutput
    {
        public override string datatype { get; set; } 
        public string image_dir { get; set; }   
    }

    public class ElementResultOutput : PythonOutput
    {
        public override string datatype { get; set; }
        public JObject elementproperties { get; set; }
    }
}
