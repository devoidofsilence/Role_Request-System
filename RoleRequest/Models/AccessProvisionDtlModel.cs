using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace RoleRequest.Models
{
    public class AccessProvisionDtlModel
    {
        public int RrfId { get; set; }

        public int AccessProvisionId { get; set; }

        public int AppId { get; set; }

        public string AppName { get; set; }

        [Required(ErrorMessage="Please provide remark.")]
        public string AppRemark { get; set; }
    }
}