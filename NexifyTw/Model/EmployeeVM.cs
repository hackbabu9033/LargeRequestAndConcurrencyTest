using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NexifyTw.Model
{
    public class EmployeeVM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string DateOfBirth { get; set; }
        public double Salary { get; set; }
        public string Address { get; set; }
    }
}
