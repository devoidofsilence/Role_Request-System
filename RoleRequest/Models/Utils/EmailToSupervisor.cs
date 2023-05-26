using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RoleRequest.Models.Utils
{
    public class EmailToSupervisor
    {
        public string Subject = "Details for {0}";

        public string Body()
        {
            String tmpMessage;

            tmpMessage = "<html><meta charset=\"UTF-8\"><p><body dir=\"ltr\">";
            tmpMessage += "<b>Details for {0}:  </b><br><br>";
            tmpMessage += "<b>HRIS ID: {1} </b><br>";
            tmpMessage += "<b>HRIS Password: {2} </b><br>";
            tmpMessage += "<b>Email ID: {3} </b><br>";
            tmpMessage += "<b>Email Password: {4} </b><br>";
            tmpMessage += "<b>Active Dir. Username ID: {5} </b><br>";
            tmpMessage += "<b>Active Dir. Password: {6} </b><br>";
            tmpMessage += "</body></p></meta></html>";

            return tmpMessage;
        }
    }
}