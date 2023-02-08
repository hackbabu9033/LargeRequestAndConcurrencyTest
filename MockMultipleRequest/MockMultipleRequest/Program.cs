using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace MockMultipleRequest
{
    class Program
    {
        private static int _threadTaskCount = 100;
        private static int _eachTaskReqeustCount = 100000;
        private static int _portNumber = 44304;
        private static int _test = 0;
        static void Main(string[] args)
        {
            SendMultipleReqeustWithNoWait(_portNumber);
        }


        private static void SendMultipleReqeustWithNoWait(int portNumber)
        {
            var sendRequestTasks = new List<Task>();
            var sendRequestActions = new List<Action>();
            var threads = new Thread[_threadTaskCount];
            // 建立多個task，每個Task都不停發送request
            for (int i = 0; i < _threadTaskCount; i++)
            {
                //threads[i] = new Thread(()=>SendRequestWithNoWait());
                sendRequestTasks.Add(new Task(() => SendRequestWithNoWait()));
                sendRequestActions.Add(SendRequestWithNoWait);
            }

            #region 這裡以兩種方式來做打大量api的測試->1. 建立多條執行緒打api  2. 利用Parallel.foreach(threadpool)方式打api

            // 理論差異：因為建立執行緒本身要花費不少時間(且內部執行sub函式時間遠小於建立thread所花時間)，所以第一個方式理論上會比第二個慢很多

            //for (int i = 0; i < _threadTaskCount; i++)
            //{
            //    threads[i].Start();
            //}
            // 以Parallel的方式模擬同時間發出大量request
            //Parallel.ForEach(sendRequestTasks, task => task.Start());
            Parallel.ForEach(sendRequestActions, action => action.Invoke());


            #endregion
        }
        
        private static void SendRequestWithNoWait()
        {
            var endPoint = $"https://localhost:44304/api/Employee/status";
            var httpclinet = new HttpClient();
            for (int i = 0; i < _eachTaskReqeustCount; i++)
            {
                //Console.WriteLine(_test++);
                httpclinet.GetAsync(endPoint);
            }
        }
    }
}
