using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RoleRequest.Models.Utils
{
    public class EmailToRequester
    {
        public string Subject = "Details for Role Request";

        public string Body(List<PrimRoleProvisionDtlModel> prpdmList, List<AccessProvisionDtlModel> apdmList, AdditionalAccessProvisionDtlModel additionalAccess)
        {
            String tmpMessage;

            tmpMessage = "<html><meta charset=\"UTF-8\"><p><body dir=\"ltr\">";
            tmpMessage += "<b>Your role request has been completed. Details for Role Request:  </b><br><br>";
            if (prpdmList != null)
            {
                foreach (var item in prpdmList)
                {
                    tmpMessage += item.PrimaryRoleName + ":" + "<b>" + item.PrimaryRoleRemark + " </b><br>";
                }
            }
            if (apdmList != null)
            {
                foreach (var item in apdmList)
                {
                    tmpMessage += item.AppName + ":" + "<b>" + item.AppRemark + " </b><br>";
                }
            }
            if (additionalAccess != null)
            {
                tmpMessage += "Remarks :" + "<b>" + additionalAccess.AccessRemarks + " </b><br>";
            }
            tmpMessage += "</body></p></meta></html>";

            return tmpMessage;
        }
    }
}