using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RoleRequest.Models
{
    public class RemarksReport
    {
        public int RequestId { get; set; }

        public string RemarksBy { get; set; }

        public string AssignDate { get; set; }

        public string CompleteDate { get; set; }

        public string AdditionalRequest { get; set; }

        public string Remarks { get; set; }

        public string Action { get; set; }
    }
}