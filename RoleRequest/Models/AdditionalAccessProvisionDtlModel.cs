using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace RoleRequest.Models
{
    public class AdditionalAccessProvisionDtlModel
    {
        public int RrfId { get; set; }

        public int ProvisionId { get; set; }

        [Required(ErrorMessage = "Please provide remark.")]
        public string AccessRemarks { get; set; }
    }
}