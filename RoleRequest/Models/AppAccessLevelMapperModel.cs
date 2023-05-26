using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RoleRequest.Models
{
    public class AppAccessLevelMapperModel
    {
        public int AppId { get; set; }

        public string AppName { get; set; }

        public int AccessLevelId { get; set; }

        public string AccessLevelName { get; set; }

        public bool IsSelected { get; set; }
    }
}