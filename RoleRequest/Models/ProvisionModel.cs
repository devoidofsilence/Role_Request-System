using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace RoleRequest.Models
{
    public class ProvisionModel
    {
        public int ProvisionId { get; set; }

        public int RoleRequestId { get; set; }

        public bool rejectionFlag { get; set; }

        public string RequestedByUsername { get; set; }

        public string RequestedByEmailID { get; set; }

        public string RequestedByFullName { get; set; }

        public string ProvidedBy { get; set; }

        public bool saveRequest { get; set; }

        public string ProvisionStatus { get; set; }
        [Required(ErrorMessage = "Please provide remarks.")]
        [DataType(DataType.MultilineText)]
        public string Remarks { get; set; }

        public string BranchName { get; set; }
        public string ApprovedDate { get; set; }
        public string InitiatedDate { get; set; }

        public bool viewOnlyMode { get; set; }

        public List<AccessProvisionDtlModel> apdmList { get; set; }

        public List<PrimRoleProvisionDtlModel> prpdmList { get; set; }

        public AdditionalAccessProvisionDtlModel additionalAccess { get; set; }

        public RoleRequestModel RoleRequestModel { get; set; }
    }
}