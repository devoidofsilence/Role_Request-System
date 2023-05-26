using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RoleRequest.Models.Utils
{
    public class EmailToIAMRevoke
    {
        public string Subject = "Details for Revoke Request";

        public string Body(string UserName)
        {
            String tmpMessage;

            tmpMessage = "<html><meta charset=\"UTF-8\"><p><body dir=\"ltr\">";
            tmpMessage += "A Revoke Request has been made for: <b>" + UserName + "</b>. Please check the details in the Role Request Application.<br><br>";
            tmpMessage += "</body></p></meta></html>";

            return tmpMessage;
        }
    }
}