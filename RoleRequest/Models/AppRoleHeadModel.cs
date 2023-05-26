using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RoleRequest.Models
{
    public class AppRoleHeadModel
    {
        public int RoleHeadId { get; set; }

        public string RoleHeadName { get; set; }

        public List<AppRolesModel> lstRolesModel { get; set; }
    }
}