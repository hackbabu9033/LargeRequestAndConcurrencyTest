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
        private static int _totalTaskCount = 10;
        private static int _eachTaskReqeustCount = 10000;
        private static int _portNumber = 44304;
        private static int _test = 0;
        //private static string _apiEndPoint = "https://localhost:44304/api/Employee/status";
        private static string _apiEndPoint = "http://localhost:1000/NexifyTw/api/Employee/status";
        private static int _canceledCount = 0;
        private static object _canceledCountLock = new object();
        private static object _successCountLock = new object();
        private static int _successCount = 0;
        private static object _failCountLock = new object();
        private static int _failConut = 0;
        private static int _serviceUnAvaliableCount = 0;
        private static object _serviceCountLock = new object();
        private static HashSet<string> _allReqeustContents = new HashSet<string>();


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
            //// 說明：如果針對單一collection的「多次查詢」，要先建立好hashset後再查詢可以大幅度的提升效能
            //// 因為hashset的查詢是O(1)
            //// hash的原理可以參考這裡https://ithelp.ithome.com.tw/articles/10208884
            //var testData = new int[10000000];
            //var rnd = new Random();
            //var rndSearchIndex = rnd.Next(0, 10000000);
            //for (int i = 0; i < 10000000; i++)
            //{
            //    testData[i] = rnd.Next(0, 10000000);
            //}
            //var rndSearchIndexValue = testData[rndSearchIndex];


            //var stopWatch = new Stopwatch();
            //var queryTime = 3000;

            ////case1: use containes directly to find specific element
            //stopWatch.Start();
            //for (int i = 0; i < queryTime; i++)
            //{
            //    testData.Contains(rndSearchIndexValue);
            //}
            //Console.WriteLine($"directly call array contains:{stopWatch.ElapsedMilliseconds}");
            //stopWatch.Stop();
            //stopWatch.Reset();
            ////case2: use hash then find specific element
            //stopWatch.Start();
            //var hashSet = new HashSet<int>(testData);
            ////hashSet.Contains(rndSearchIndexValue);
            //for (int i = 0; i < queryTime; i++)
            //{
            //    hashSet.Contains(rndSearchIndexValue);
            //}
            //stopWatch.Stop();
            //Console.WriteLine($"create hashset call array contains:{stopWatch.ElapsedMilliseconds}");
            #endregion

            //var httpClinet = new HttpClient();
            //var response = httpClinet.GetAsync(_apiEndPoint).GetAwaiter().GetResult();

            //var webclinet = new WebClient();
            //webclinet.DownloadStringCompleted += GetStringResultHandler;
            //webclinet.DownloadStringAsync(new Uri(_apiEndPoint));
            //Thread.Sleep(1000);
            SendMultipleReqeustWithNoWait().GetAwaiter().GetResult();
        }



        private static async Task SendMultipleReqeustWithNoWait()
        {
            var sendRequestTasks = new List<Task>();
            // 建立多個task，每個Task都不停發送request
            var threads = new Thread[_totalTaskCount];
            for (int i = 0; i < _totalTaskCount; i++)
            {
                sendRequestTasks.Add(new Task(() => SendWebRequest()));
                threads[i] = new Thread(() => SendWebRequest());
                //sendRequestTasks.Add(new Task(() => SendRequestWithNoWait()));
            }



            #region 這裡以兩種方式來做打大量api的測試->1. 建立多條執行緒打api  2. 利用Parallel.foreach(threadpool)方式打api

            // 理論差異：因為建立執行緒本身要花費不少時間(且內部執行sub函式時間遠小於建立thread所花時間)，所以第一個方式理論上會比第二個慢很多

            //for (int i = 0; i < _threadTaskCount; i++)
            //{
            //    threads[i].Start();
            //}
            // 以Parallel的方式模擬同時間發出大量request
            //Parallel.ForEach(sendRequestTasks, task => task.Start());
            for (int i = 0; i < _totalTaskCount; i++)
            {
                threads[i].Start();
            }
            //Parallel.ForEach(sendRequestTasks, task => task.Start());

            //await Task.WhenAll(sendRequestTasks);
            string allContents = string.Empty;
            while (true)
            {
                Thread.Sleep(1000);
                Console.WriteLine($"cur total static -> success:{_successCount},fail:{_failConut},service unavaliable:{_serviceUnAvaliableCount}");

                if (_allReqeustContents.Any())
                {
                    allContents = string.Join(',', _allReqeustContents);
                }
                Console.WriteLine($"cur request Contents : {allContents}");
            }
            
            #endregion
        }

        private static void SendWebRequest()
        {
            for (int i = 0; i < _eachTaskReqeustCount; i++)
            {
                var webClinet = new WebClient();
                webClinet.DownloadStringCompleted += GetStringResultHandler;
                webClinet.DownloadStringAsync(new Uri(_apiEndPoint));
            }
        }
        
        private static void SendRequestWithNoWait()
        {
            var httpclinet = new HttpClient();
            for (int i = 0; i < _eachTaskReqeustCount; i++)
            {
                //Console.WriteLine(_test++);
                httpclinet.GetAsync(_apiEndPoint);
                // A single tick represents one hundred nanoseconds or one ten-millionth of a second. There are 10,000 ticks in a millisecond (see TicksPerMillisecond) and 10 million ticks in a second.
            }
        }

        private static void GetStringResultHandler(object sender, DownloadStringCompletedEventArgs e)
        {
            try
            {
                // If the request was not canceled and did not throw
                // an exception, display the resource.
                if (e.Cancelled)
                {
                    lock (_canceledCountLock)
                    {
                        _canceledCount++;
                        _allReqeustContents.Add("request Cancelled");
                    }
                    return;
                }
                if (e.Error != null)
                {
                    if (e.Error.Message.Contains("503"))
                    {
                        lock (_serviceCountLock)
                        {
                            _serviceUnAvaliableCount++;
                            _allReqeustContents.Add("server unavaliable");
                        }
                        return;
                    }
                    else
                    {
                        lock (_failCountLock)
                        {
                            _failConut++;
                            _allReqeustContents.Add(e.Error.Message);
                        }
                        return;
                    }
                }
                lock (_successCountLock)
                {
                    _successCount++;
                    return;
                }
            }
            catch (Exception ex)
            {
                lock (_failCountLock)
                {
                    _failConut++;
                }
            }
        }

        
    }
    public enum ErrorType
    {
        canceled,
        serveUnavailable,
        other
    }
}
