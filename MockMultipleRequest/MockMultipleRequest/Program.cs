using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
        private static string _apiEndPoint = "https://localhost:44304/api/Employee/status";
        static void Main(string[] args)
        {
            //// 這個跑法會將單核心cpu運算占滿
            //while (true)
            //{

            //}

            // 但若在之中稍微thread sleep一下就不會
            // 因為cpu本身運算頻率相當高，即使休息單位時間的tick，仍然是很長的時間
            //var test = true;
            //while (true)
            //{
            //    Thread.Sleep(new TimeSpan(10000));
            //}

            #region foreground / background thread
            //// foreground thread vs background thread
            //// Foreground threads keep the application alive for as long as any one of them is running, whereas background threads do not. Once all foreground threads finish, the application ends, and any background threads still running abruptly terminate.
            //// in the following condition,if worker is foreground thread,then application will wait user input enter
            //// if worker is background thread,then application end very soon since there are no other foregrounds
            //var a = new List<string>() {"a" };
            //Thread worker = new Thread(() => Console.ReadLine());
            //if (a.Any()) worker.IsBackground = true;
            //worker.Start();
            #endregion

            #region test use hashSet data performance diff
            // 說明：如果針對單一collection的「多次查詢」，要先建立好hashset後再查詢可以大幅度的提升效能
            // 因為hashset的查詢是O(1)
            // hash的原理可以參考這裡https://ithelp.ithome.com.tw/articles/10208884
            var testData = new int[10000000];
            var rnd = new Random();
            var rndSearchIndex = rnd.Next(0, 10000000);
            for (int i = 0; i < 10000000; i++)
            {
                testData[i] = rnd.Next(0, 10000000);
            }
            var rndSearchIndexValue = testData[rndSearchIndex];


            var stopWatch = new Stopwatch();
            var queryTime = 3000;

            //case1: use containes directly to find specific element
            stopWatch.Start();
            for (int i = 0; i < queryTime; i++)
            {
                testData.Contains(rndSearchIndexValue);
            }
            Console.WriteLine($"directly call array contains:{stopWatch.ElapsedMilliseconds}");
            stopWatch.Stop();
            stopWatch.Reset();
            //case2: use hash then find specific element
            stopWatch.Start();
            var hashSet = new HashSet<int>(testData);
            //hashSet.Contains(rndSearchIndexValue);
            for (int i = 0; i < queryTime; i++)
            {
                hashSet.Contains(rndSearchIndexValue);
            }
            stopWatch.Stop();
            Console.WriteLine($"create hashset call array contains:{stopWatch.ElapsedMilliseconds}");
            #endregion

            //SendMultipleReqeustWithNoWait(_portNumber);
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

            var webClinet = new WebClient();
            webClinet.DownloadDataCompleted += QueryCompleteHandler;
            webClinet.DownloadDataAsync(new Uri(_apiEndPoint));

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
                // A single tick represents one hundred nanoseconds or one ten-millionth of a second. There are 10,000 ticks in a millisecond (see TicksPerMillisecond) and 10 million ticks in a second.
                Thread.Sleep(new TimeSpan(100000));
            }
        }

        private static void QueryCompleteHandler(object sender, DownloadDataCompletedEventArgs e)
        {
            try
            {
                // If the request was not canceled and did not throw
                // an exception, display the resource.
                if (!e.Cancelled && e.Error == null)
                {
                    byte[] data = (byte[])e.Result;
                    string textData = System.Text.Encoding.UTF8.GetString(data);

                    Console.WriteLine($"request success, result:{textData}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
