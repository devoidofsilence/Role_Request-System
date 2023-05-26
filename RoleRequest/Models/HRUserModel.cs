using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace RoleRequest.Models
{
    public class HRUserModel
    {
        public int EnrollUserID { get; set; }

        public string FullName { get; set; }

        public int DepartmentId { get; set; }

        public string DepartmentName { get; set; }

        public int BranchLocationId { get; set; }

        public string BranchLocationName { get; set; }

        public int CorpTitleId { get; set; }

        public string CorpTitleName { get; set; }

        //public int FuncTitleId { get; set; }

        //public string FuncTitleName { get; set; }

        public string FunctionalTitle { get; set; }

        public string MobileNumber { get; set; }

        public string JoinDate { get; set; }

        [Required(ErrorMessage = "Please provide employee ID")]
        public string EmployeeID { get; set; }

        [Required(ErrorMessage = "Please provide password")]
        public string EmployeeIDPwd { get; set; }

        public string ADUsername { get; set; }

        public string AssignedHRAdmin { get; set; }

        public string Email { get; set; }

        public string Status { get; set; }

        public string SupervisionBy { get; set; }

        public string Remarks { get; set; }
    }
}