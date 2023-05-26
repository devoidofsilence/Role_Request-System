using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace RoleRequest.Models
{
    public class PrimRoleProvisionDtlModel
    {
        public int RrfId { get; set; }

        public int ProvisionId { get; set; }

        public int PrimaryRoleId { get; set; }

        public string PrimaryRoleName { get; set; }

        [Required(ErrorMessage = "Please provide remark.")]
        public string PrimaryRoleRemark { get; set; }
    }
}