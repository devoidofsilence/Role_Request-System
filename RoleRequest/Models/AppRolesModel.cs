using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RoleRequest.Models
{
    public class AppRolesModel
    {
        public int RoleId { get; set; }

        public int RoleRequestId { get; set; }

        public string RoleName { get; set; }

        public bool IsSelected { get; set; }

        public bool LatestSavedRequestFlag { get; set; }
        
        public bool ChangedFlag { get; set; }
    }
}