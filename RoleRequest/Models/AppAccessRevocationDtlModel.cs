using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RoleRequest.Models
{
    public class AppAccessRevocationDtlModel
    {
        public int RevokeId { get; set; }

        public int AppId { get; set; }

        public string Status { get; set; }

        public string AppName { get; set; }
    }
}