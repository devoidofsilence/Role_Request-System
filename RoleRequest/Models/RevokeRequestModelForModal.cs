using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace RoleRequest.Models
{
    public class RevokeRequestModelForModal
    {
        public string RevokeEmpID { get; set; }

        public string RevokeEmpUsername { get; set; }

        public string RevokeEmpFullName { get; set; }

        public string RequestedBy { get; set; }

        public DateTime RevokeDate { get; set; }

        [Required(ErrorMessage = "Please provide resignation date.")]
        public string ResignationDate { get; set; }
    }
}