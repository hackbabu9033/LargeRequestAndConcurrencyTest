using NexifyTw.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;

namespace NexifyTw.Repo
{
    public class EmployeeRepo
    {
        private string _conn = "";
        private IConfiguration _config;
        private List<string> _status;
        private static Random _rnd = new Random();
        private int count { get { return _status.Count; } }

        public EmployeeRepo(IConfiguration config)
        {
            _config = config;
            _conn = _config.GetConnectionString("Nexify");
            _status = new List<string>() { "hot","cold","aaa","bbb","ccc","ddd" };
        }
        public List<EmployeeDTO> GetAll()
        {
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
            var rndIndex = _rnd.Next(0, count);
            return _status[rndIndex];
        }
    }
}
