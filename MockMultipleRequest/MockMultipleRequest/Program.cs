using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace MockMultipleRequest
{
    class Program
    {
        private static int _taskCount = 10000;
        private static int _eachTaskReqeustCount = 10000;
        static void Main(string[] args)
        {
            SendMultipleReqeustWithNoWait(1000);
        }


        private static void SendMultipleReqeustWithNoWait(int portNumber)
        {
            var sendRequestTasks = new List<Task>();
            for (int i = 0; i < _taskCount; i++)
            {
                sendRequestTasks.Add(SendRequestWithNoWait(portNumber));
            }

            Parallel.ForEach(sendRequestTasks, task => task.Start());
            return;
        }

        private static Task SendRequestWithNoWait(int portNumber)
        {
            var endPoint = $"http://localhost:{portNumber}/api/employee";
            var httpclinet = new HttpClient();
            for (int i = 0; i < _eachTaskReqeustCount; i++)
            {
                httpclinet.GetAsync(endPoint);
            }
            return Task.CompletedTask;
        }
    }
}
