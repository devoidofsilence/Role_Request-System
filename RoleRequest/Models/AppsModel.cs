using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RoleRequest.Models
{
    public class AppsModel
    {
        public int AppId { get; set; }

        public string AppName { get; set; }

        public List<AppAccessLevelModel> lstAccessLevel { get; set; }

        public List<AppRoleHeadModel> lstRoleHeads { get; set; }
    }
}