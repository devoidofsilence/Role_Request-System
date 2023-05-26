using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RoleRequest.Models
{
    public class AppDomainModel
    {
        public int DomainId { get; set; }

        public string DomainName { get; set; }

        public List<AppsModel> lstApps { get; set; }
    }
}