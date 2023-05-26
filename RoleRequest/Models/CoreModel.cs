using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RoleRequest.Models
{
    public class CoreModel
    {
        public int DomainId { get; set; }

        public string DomainName { get; set; }

        public int AppId { get; set; }

        public string AppName { get; set; }

        public int AccessLevelId { get; set; }

        public string AccessLevelName { get; set; }

        public int RoleHeadId { get; set; }

        public string RoleHeadName { get; set; }

        public int RoleId { get; set; }

        public string RoleName { get; set; }

        public bool IsSelectedAccessLevel { get; set; }

        public bool IsSelectedRole { get; set; }
    }
}