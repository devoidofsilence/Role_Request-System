using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RoleRequest.Models.Utils
{
    public class EmailToRevokeRequester
    {
        public string Subject = "Details for Revoke Request";

        public string Body(string UserName)
        {
            String tmpMessage;

            tmpMessage = "<html><meta charset=\"UTF-8\"><p><body dir=\"ltr\">";
            tmpMessage += "The Revoke Request made for: <b>" + UserName + "</b> is completed. Please check the details in the Role Request Application.<br><br>";
            tmpMessage += "</body></p></meta></html>";

            return tmpMessage;
        }
    }
}