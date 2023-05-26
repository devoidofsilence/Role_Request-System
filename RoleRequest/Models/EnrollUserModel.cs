using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace RoleRequest.Models
{
    public class EnrollUserModel
    {
        public int EnrollUserID { get; set; }

        [Required(ErrorMessage = "Please provide full name")]
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

        [Required(ErrorMessage = "Please provide mobile number")]
        [StringLength(10, ErrorMessage = "Mobile number should not be more than 10 in length")]
        public string MobileNumber { get; set; }

        [Required(ErrorMessage = "Please provide joining date")]
        public string JoinDate { get; set; }

        public string EmployeeID { get; set; }

        public string EmployeeIDPwd { get; set; }

        public string ADUsername { get; set; }

        public string ADPassword { get; set; }

        public string Email { get; set; }

        public string EmailPwd { get; set; }

        public string Status { get; set; }

        public string SupervisionBy { get; set; }

        public string Remarks { get; set; }
    }
}