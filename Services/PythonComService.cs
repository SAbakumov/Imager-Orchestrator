using DagOrchestrator.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System.Diagnostics;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DagOrchestrator.Services
{
    public class PythonComService
    {

        private string _port = "http://localhost:8400";
        private HttpClient _httpClient;

        public PythonComService(HttpClient httpClient) 
        { 
            _httpClient = httpClient;
        }

        public async Task<string> GetAvailableNodes()
        {
            string data = await _httpClient.GetStringAsync($"/api/get_nodes");
            return data;    
        }

        public async Task<string> GetFunctionInfo(string route)
        {
            string func_info = await _httpClient.GetStringAsync($"{route}/get_info");
            return func_info;
        }

        public async Task<HttpResponseMessage> SubmitImagerAPICall(string route, string nodeinfo)
        {

            var payload = new StringContent(nodeinfo, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(route, payload);

            return response;
        }

        internal async Task<string> ClearData(string? jobID)
        {
            string response = await _httpClient.GetStringAsync($"/imagedata/clear_all_data?process_id={jobID}");
            return response;
        }

        internal async Task<string> SendData(MessagePackData data)
        {
            var tiffMeta = new TiffPlaneMetadata
            {
                PositionX = data.message.metadata.stageposition.x,
                PositionY = data.message.metadata.stageposition.y,
                PositionZ = data.message.metadata.stageposition.z,
                AcquisitionName = data.message.metadata.acquisitiontype,
                DetectorName = data.message.data.detectorname,
                Width = (uint)data.message.data.ncols,
                Height = (uint)data.message.data.nrows,
                TimePoint = data.message.data.timestamp
            };

            using var byteContent = new ByteArrayContent(data.message.data.imagedata);
            byteContent.Headers.ContentType =
                new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");



            //string response = await _httpClient.GetStringAsync($"/imagedata/clear_all_data?process_id={jobID}");
            var response = _httpClient.PostAsync(
                $"imagedata/set_data?" +
                $"process_id=abc&" +
                $"acqname={tiffMeta.AcquisitionName}&" +
                $"detname={tiffMeta.DetectorName}&" +
                $"detindex={data.index}&" +
                $"width={tiffMeta.Width}&" +
                $"height={tiffMeta.Height}",
                byteContent
                ).GetAwaiter().GetResult();



            return response.ToString();
        }
    }

    internal class TiffPlaneMetadata
    {
        public float PositionX { get; set; }
        public float PositionY { get; set; }
        public float PositionZ { get; set; }
        public string AcquisitionName { get; set; }
        public string DetectorName { get; set; }
        public uint Width { get; set; }
        public uint Height { get; set; }
        public float TimePoint { get; set; }
    }
}
