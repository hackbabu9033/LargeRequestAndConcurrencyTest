using NexifyTw.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;
using System.Threading;
using System.Collections.Concurrent;

namespace NexifyTw.Repo
{
    public class EmployeeRepo
    {
        private string _conn = "";
        private IConfiguration _config;
        private List<string> _status;
        private static Random _rnd = new Random();
        private static int test = 0;
        private static ConcurrentBag<int> _allThreadIds = new ConcurrentBag<int>();
        private static ConcurrentBag<int> requestLogCounts = new ConcurrentBag<int>();
        private static object _objLock = new object();
        private int count { get { return _status.Count; } }

        public EmployeeRepo(IConfiguration config)
        {
            _config = config;
            _conn = _config.GetConnectionString("Nexify");
            _status = new List<string>() { "hot","cold","aaa","bbb","ccc","ddd" };
        }
        public List<EmployeeDTO> GetAll()
        {
            System.Diagnostics.Debug.WriteLine("get all used thread Ids");
            foreach (var threadId in _allThreadIds)
            {
                System.Diagnostics.Debug.WriteLine(threadId);
            }
            using (var conn = new SQLiteConnection(_conn))
            {
                var sql = "select * from Employee order by Id DESC";
                var result = conn.Query<EmployeeDTO>(sql).ToList();
                return result;
            }
        }

        public void PutEmployees(List<PutEmployeeModel> employees)
        {
            var insertDatas = employees.Where(x => !x.Id.HasValue);
            var updateDatas = employees.Except(insertDatas);
            var updateSql = @"UPDATE Employee SET Name=@Name,
DateOfBirth=@DateOfBirth,
Salary=@Salary,
Address=@Address 
WHERE Id=@Id";
            var insertSql = @"Insert into Employee (Name,DateOfBirth,Salary,Address)
VALUES (@Name,@DateOfBirth,@Salary,@Address)";
            using (var conn = new SQLiteConnection(_conn))
            {
                conn.Open();
                using (var transcation = conn.BeginTransaction())
                {
                    try
                    {
                        if (updateDatas.Any()) 
                            conn.Execute(updateSql, updateDatas);
                        if (insertDatas.Any())
                            conn.Execute(insertSql, insertDatas);
                        transcation.Commit();
                    }
                    catch(Exception ex)
                    {
                        transcation.Rollback();
                    }
                }
                conn.Close();
            }
        }

        public string GetRandomeStatus()
        {
            //// display get get request time and thread id which process this request
            //// 這裡可以看到即使有非常大量的reqeust情況下，仍然只有很少數的work thread
            //var curThreadId = Environment.CurrentManagedThreadId;
            //if (!_allThreadIds.Contains(curThreadId))
            //{
            //    _allThreadIds.Add(curThreadId);
            //}


            //// 如果此站台的佇列長度與最大併發連線數沒有很大時，如有大量request，就會發生503的問題
            //// mock some operation consume large amount of time
            //Thread.Sleep(300);

            AddTest();
            System.Diagnostics.Debug.WriteLine(test);
            var rndIndex = _rnd.Next(0, count);
            return _status[rndIndex];
        }

        private async void AddTest()
        {
            Monitor.Enter(_objLock);
            test++;
            Monitor.Exit(_objLock);
            System.Diagnostics.Debug.WriteLine(test);
        }
    }
}
