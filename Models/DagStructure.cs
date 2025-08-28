using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DagOrchestrator.Models
{


    public class InputParameter
    {
        [JsonIgnore]
        private JObject? _jsonObject;
        [JsonIgnore]
        public bool IsAssigned = false;


        [JsonProperty("input_type")]
        public string? InputType { get; set; }

        private JObject? _inputParams;

        [JsonProperty("input_params")]
        public JObject? InputParams
        {
            get => _inputParams;
            set
            {
                _inputParams = value;
                _jsonObject = new JObject();
                if (_inputParams != null)
                    _jsonObject["input_params"] = _inputParams;
            }
        }
        private JValue? _isInputNode;

        [JsonProperty("isinputnode")]
        private JValue? IsInputNodeSetter
        {
            set => _isInputNode = value; 
        }

        [JsonIgnore]
        public JValue? IsInputNode => _isInputNode; 

        [JsonIgnore]
        public string? ImageDir
        {
            get
            {
                return _jsonObject?
                    .SelectToken("input_params.input_json_params.image_dir")
                    ?.ToString();
            }
            set
            {
                if (_jsonObject == null)
                    _jsonObject = new JObject();

                if (_jsonObject["input_params"] is not JObject inputParams)
                {
                    inputParams = new JObject();
                    _jsonObject["input_params"] = inputParams;
                }

                if (inputParams["input_json_params"] is not JObject inputJsonParams)
                {
                    inputJsonParams = new JObject();
                    inputParams["input_json_params"] = inputJsonParams;
                }

                if (value is null)
                    inputJsonParams.Remove("image_dir");
                else
                    inputJsonParams["image_dir"] = value;
            }
        }

        public void SubstituteImageDir(string newImageDir)
        {
            ImageDir = newImageDir;
        }
    }


    public class DagNode
    {
        [JsonProperty("node_id")]
        public Guid NodeId { get; set; }

        [JsonProperty("input_nodes")]
        public List<Guid>? InputNodes { get; set; }

        [JsonProperty("input_parameters")]
        public InputParametersContainer? InputParameters { get; set; }

        [JsonProperty("api_path")]
        public string? ApiPath { get; set; }

        [JsonProperty("output_nodes")]
        public List<List<string>>? OutputNodes { get; set; }

        [JsonIgnore]
        public string? JobID { get; set; }

        [JsonProperty("isinputnode")]
        public bool? IsInputNode { get; set; }

        [JsonProperty("isoutputnode")]
        public bool? IsOutputNode { get; set; }
    }

    public class InputParametersContainer
    {
        [JsonProperty("node_id")]
        public Guid NodeId { get; set; }

        [JsonProperty("job_id")]
        public string? JobId { get; set; }



        [JsonProperty("input")]
        public List<InputParameter>? Input { get; set; }
    }
}