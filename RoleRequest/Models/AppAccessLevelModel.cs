using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RoleRequest.Models
{
    public class AppAccessLevelModel
    {
        public int AccessLevelId { get; set; }

        public string AccessLevelName { get; set; }

        public bool IsSelected { get; set; }

        public bool LatestSavedRequestFlag { get; set; }

        public bool ChangedFlag { get; set; }

        public int RoleRequestId { get; set; }

        public int AppId { get; set; }
    }
}