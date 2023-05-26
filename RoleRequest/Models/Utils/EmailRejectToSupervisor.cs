using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RoleRequest.Models.Utils
{
    public class EmailRejectToSupervisor
    {
        public string Subject = "Details for User Enroll";

        public string Body(string UserName, string remarks)
        {
            String tmpMessage;

            tmpMessage = "<html><meta charset=\"UTF-8\"><p><body dir=\"ltr\">";
            tmpMessage += "The User Enrollment request made for: <b>" + UserName + "</b> has been rejected with remarks as: <b>"+ remarks +"</b>. Please check the details in the Role Request Application.<br><br>";
            tmpMessage += "</body></p></meta></html>";

            return tmpMessage;
        }
    }
}