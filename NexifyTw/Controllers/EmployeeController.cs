using Microsoft.AspNetCore.Mvc;
using NexifyTw.Model;
using NexifyTw.Repo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NexifyTw
{
    [ApiController]
    [Route("api/Employee")]
    public class EmployeeController : ControllerBase
    {
        private EmployeeRepo _employeeRepo { get; set; }

        public EmployeeController(EmployeeRepo employeeRepo)
        {
            _employeeRepo = employeeRepo;
        }

        [HttpGet]
        public IActionResult GetEmployee()
        {
            var result = _employeeRepo.GetAll().Select(x => new EmployeeVM()
            {
                Id = x.Id,
                DateOfBirth = x.DateOfBirth.Date.ToString("yyyy/MM/dd"),
                Name = x.Name,
                Salary = x.Salary,
                Address = x.Address
            });
            return Ok(new { data = result });
        }

        [HttpPut]
        public IActionResult Update([FromBody] List<PutEmployeeModel> putEmployees)
        {
            _employeeRepo.PutEmployees(putEmployees);
            return Ok();
        }
    }
}
