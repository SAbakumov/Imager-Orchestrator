using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace DagOrchestrator.Services
{
    public class PythonComService
    {

        private string _port = "http://localhost:8443";
        private HttpClient _httpClient;

        public PythonComService(HttpClient httpClient) 
        { 
            _httpClient = httpClient;
        }

        public async Task<string> GetAvailableNodes()
        {
            string data = await _httpClient.GetStringAsync($"{_port}/api/get_nodes");
            return data;    
        }

        public async Task<string> GetFunctionInfo(string route)
        {
            string func_info = await _httpClient.GetStringAsync($"{_port}{route}/get_info");
            return func_info;
        }


    }
}
