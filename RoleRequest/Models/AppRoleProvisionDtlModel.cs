using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RoleRequest.Models
{
    public class AppRoleProvisionDtlModel
    {
        public int RrfId { get; set; }

        public int ProvisionId { get; set; }

        public int RoleId { get; set; }

        public string RoleName { get; set; }

        public string RoleRemark { get; set; }
    }
}