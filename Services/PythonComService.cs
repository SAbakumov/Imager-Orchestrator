using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
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

        public async Task<string> SubmitImagerAPICall(string route, string nodeinfo)
        {

            var payload = new StringContent(nodeinfo, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(route, payload);

            return await response.Content.ReadAsStringAsync();
        }


    }
}
