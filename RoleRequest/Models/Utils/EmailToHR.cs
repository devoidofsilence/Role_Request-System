using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RoleRequest.Models.Utils
{
    public class EmailToHR
    {
        public string Subject = "Details for User Enroll";

        public string Body(string UserName)
        {
            String tmpMessage;

            tmpMessage = "<html><meta charset=\"UTF-8\"><p><body dir=\"ltr\">";
            tmpMessage += "A User Enrollment has been posted for: <b>" + UserName + "</b>. Please check the details in the Role Request Application.<br><br>";
            tmpMessage += "</body></p></meta></html>";

            return tmpMessage;
        }
    }
}