using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RoleRequest.Models
{
    public class EmployeeModel
    {
        public string EmployeeId { get; set; }

        public string SamAccountName { get; set; }

        public string EmailId { get; set; }

        public string EmployeeFirstName { get; set; }

        public string EmployeeLastName { get; set; }

        public string EmployeeFullName;

        public int BranchId { get; set; }

        public string BranchName { get; set; }

        public int DepartmentId { get; set; }

        public string DepartmentName { get; set; }

        public int CorporateTitleId { get; set; }

        public string CorporateTitle { get; set; }

        public int FunctionalTitleId { get; set; }

        public string FunctionalTitle { get; set; }

        public List<string> UserRole { get; set; }
    }
}