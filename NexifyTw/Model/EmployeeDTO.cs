using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NexifyTw.Model
{
    public class EmployeeDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime DateOfBirth { get; set; }
        public double Salary { get; set; }
        public string Address { get; set; }
    }
}
