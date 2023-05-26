using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RoleRequest.Models
{
    public class PrimRoleRevocationDtlModel
    {
        public int RevokeId { get; set; }

        public int PrimRoleId { get; set; }

        public string Status { get; set; }

        public string PrimaryRoleName { get; set; }
    }
}