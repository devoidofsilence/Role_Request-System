using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RoleRequest.Models.Utils
{
    public class EmailRejectionToRequester
    {
        public string Subject = "Details for Role Request";

        public string Body()
        {
            String tmpMessage;

            tmpMessage = "<html><meta charset=\"UTF-8\"><p><body dir=\"ltr\">";
            tmpMessage += "Your role request has been returned. Please check the details in the Role Request Application.<br><br>";
            tmpMessage += "</body></p></meta></html>";

            return tmpMessage;
        }
    }
}