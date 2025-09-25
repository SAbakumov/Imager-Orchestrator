using DagOrchestrator.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StackExchange.Redis;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Channels;
using static System.Net.Mime.MediaTypeNames;

namespace DagOrchestrator.Services
{
    public class ImageSubmissionService
    {
        private TcpClient _client;
        private NetworkStream _stream;



        public ImageSubmissionService(PythonComService pythonComService)
        {
            _client = new TcpClient("127.0.0.1", 8401);
            _stream = _client.GetStream();

        
        }






        public void SendData(JobImageData data)
        {
            try
            {
                var job_bytes = Encoding.UTF8.GetBytes(data.JobID);
                _stream.Write(job_bytes, 0, job_bytes.Length);

                byte[] ack2 = new byte[32];
                int ackLen2 = _stream.Read(ack2, 0, ack2.Length);


                int size = data.Image.Length;

                byte[] sizeBytes = BitConverter.GetBytes(System.Net.IPAddress.HostToNetworkOrder(size));
                _stream.Write(sizeBytes, 0, 4);


                byte[] ack = new byte[32];
                int ackLen = _stream.Read(ack, 0, ack.Length);


                _stream.Write(data.Image, 0, data.Image.Length);

                byte[] ack3 = new byte[32];
                int ackLen3 = _stream.Read(ack2, 0, ack2.Length);
            }
            catch (Exception e)  {

                Console.WriteLine(e.Message);
            }


        }

        internal void Enqueue(JobImageData receivedData)
        {
            if (!IsSocketAlive(_client))
            {
                _client = new TcpClient("127.0.0.1", 8401);
                _stream = _client.GetStream();
            }
           SendData(receivedData);


        }

        private bool IsSocketAlive(TcpClient client)
        {
            try
            {
                Socket s = client.Client;
                return !(s.Poll(1, SelectMode.SelectRead) && s.Available == 0);
            }
            catch
            {
                return false;
            }
        }
    }
}
