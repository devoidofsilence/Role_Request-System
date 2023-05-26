using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace RoleRequest.Models
{
    public class RevokeRequestModel
    {
        public int RoleRequestId { get; set; }

        public int RevokeId { get; set; }

        public string RevokeEmpID { get; set; }

        public string RevokeEmpUsername { get; set; }

        public string RevokeEmpFullName { get; set; }

        public string RequestStatus { get; set; }

        public string RequestedBy { get; set; }

        public string TakenBy { get; set; }

        public DateTime RevokeDate { get; set; }

        [Required(ErrorMessage = "Please provide resignation date.")]
        public DateTime ResignationDate { get; set; }

        public bool AllRevoked { get; set; }

        [Required(ErrorMessage = "Please provide remark.")]
        public string Remarks { get; set; }

        public List<AppAccessRevocationDtlModel> aardmList { get; set; }

        public List<PrimRoleRevocationDtlModel> prrdmList { get; set; }
    }
}