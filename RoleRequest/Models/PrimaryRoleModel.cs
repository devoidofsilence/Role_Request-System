using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RoleRequest.Models
{
    public class PrimaryRoleModel
    {
        public int PrimaryRoleId { get; set; }

        public int RoleRequestId { get; set; }

        public string PrimaryRoleName { get; set; }

        public bool IsSelected { get; set; }

        public bool LatestSavedRequestFlag { get; set; }

        public bool ChangedFlag { get; set; }
    }
}