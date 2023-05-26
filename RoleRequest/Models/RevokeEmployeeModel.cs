using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RoleRequest.Models
{
    public class RevokeEmployeeModel
    {
        public string SamAccountName { get; set; }

        public string EmployeeFullName { get; set; }

        public string RequestStatus { get; set; }

        public string BranchName { get; set; }

        public string HRISId { get; set; }

        public string EmailId { get; set; }
    }
}