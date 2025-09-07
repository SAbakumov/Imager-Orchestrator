using DagOrchestrator.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StackExchange.Redis;
using System.Net;

namespace DagOrchestrator.Services
{
    public class DagProcessingService : BackgroundService
    {

        private bool no_job_running = true;
        private readonly IDagScheduler _dagScheduler;
        private readonly PythonComService _pythonComService;
        private readonly JobSubmissionService _jobSubmissionService;

        private readonly IConnectionMultiplexer _imageCache;
        private IDatabase _db;

        private ImageCacheGCService _imageCacheGCService;

        public DagProcessingService(IDagScheduler dagScheduler, IConnectionMultiplexer cache, PythonComService pythonComService,
            JobSubmissionService jobSubmissionService)
        {
            _jobSubmissionService = jobSubmissionService;
            _dagScheduler = dagScheduler;
            _pythonComService  = pythonComService;
            _imageCache = cache;
            _db = cache.GetDatabase();

        }


        public void SubmitDag(List<DagNode> dagNodes)
        {
            _dagScheduler.AppendDagNodes(dagNodes); 
        }


        internal void HandleNode(DagNode node, string image_response)
        {

            if (node is null)
                return;

            bool isOutputNode = node.IsOutputNode ?? false;
            bool isLazyNode = node.IsLazyNode ?? false;

            if (isOutputNode && !isLazyNode)
            {
                HandleCompletedOutputNode(node, image_response);
            }
            else if (!isOutputNode)
            {
                HandleIntermediateNode(node, image_response);
            }
            else if (isLazyNode)
            {
                HandleLazyNode(node, image_response);
            }

        }

        public async Task<string> ExecuteSingleNode(DagNode node)
        {

            var input_params = JObject.FromObject(node.InputParameters);
            string json = JsonConvert.SerializeObject(input_params, Formatting.None);

            var python_response = await _pythonComService.SubmitImagerAPICall(node.ApiPath, json);
            var image_response = await python_response.Content.ReadAsStringAsync();

            return image_response;  
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (no_job_running)
                {
                    var queued_job = _jobSubmissionService.DequeueJob();
                    if (queued_job != null)
                    {
                        _dagScheduler.SetCurrentJob(queued_job.Nodes);
                        _imageCacheGCService = new ImageCacheGCService(_db);
                        no_job_running = false;
                    }
                }

                var node = _dagScheduler.RetrieveSubmissionReadyNode();
                
                if (node != null && node.ApiPath!=null && node.InputParameters!=null)
                {
                    _imageCacheGCService.TryAddNodeID(node);

                    var input_params = JObject.FromObject(node.InputParameters);
                    string json = JsonConvert.SerializeObject(input_params, Formatting.None);

                    var python_response = await  _pythonComService.SubmitImagerAPICall(node.ApiPath, json);
                    var image_response =  await  python_response.Content.ReadAsStringAsync();

                    if (python_response.StatusCode == HttpStatusCode.InternalServerError)
                    {
                        _dagScheduler.RemoveNodesWithJobId(node.JobID);
                        _jobSubmissionService.SetJobStatusFailed(node.JobID, image_response);
                        no_job_running = true;
                        node = null;
                    }


                    HandleNode(node, image_response);
    
      
                }
                else
                {
                    await Task.Delay(500, stoppingToken); // wait before next check
                }
            }
        }

        private void HandleCompletedOutputNode(DagNode node, string imageResponse)
        {
            _jobSubmissionService.SetJobResult(node.JobID, imageResponse);
            _jobSubmissionService.SetJobStatusCompleted(node.JobID);
            _dagScheduler.RemoveNodesWithJobId(node.JobID);
            no_job_running = true;
        }

        private void HandleIntermediateNode(DagNode node, string imageResponse)
        {
            JArray pythonOutputArray = JArray.Parse(imageResponse);

            var imageOutputParams = new List<PythonOutput?>();
            foreach (var output in pythonOutputArray)
            {
                switch (output.SelectToken("datatype")?.ToString())
                {
                    case "Image2D":
                        var imageResult = output.ToObject<ImageResultOutput>();
                        imageOutputParams.Add(imageResult);
                        _imageCacheGCService.AddReferencePath(node, imageResult.image_dir);
                        break;

                    case "MeasurementElement":
                        imageOutputParams.Add(output.ToObject<ElementResultOutput>());
                        break;
                }
            }

            if (imageOutputParams.Count > 0)
            {
                for (int i = 0; i < imageOutputParams.Count; i++)
                {
                    foreach (var outputNodeId in node.OutputNodes[i])
                    {
                        var outputNode = _dagScheduler.RetrieveNodeByNodeID(outputNodeId);

                        foreach (var item in outputNode.InputParameters.Input
                            .Where(x => x.ImageDir == node.NodeId.ToString()))
                        {
                            item.ImageDir = imageOutputParams[i] switch
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

            _imageCacheGCService.RemoveReferenceNodeID(node);
            _imageCacheGCService.InvokeNodeGC();
            _dagScheduler.RemoveNode(node);
        }

        private void HandleLazyNode(DagNode node, string imageResponse)
        {
            _jobSubmissionService.SetJobResult(node.JobID, imageResponse);
            _jobSubmissionService.SetLazyNodes(node);
            _dagScheduler.RemoveNodesWithJobId(node.JobID);
            no_job_running = true;
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
