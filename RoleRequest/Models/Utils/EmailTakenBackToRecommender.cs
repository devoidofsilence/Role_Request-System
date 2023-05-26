using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RoleRequest.Models.Utils
{
    public class EmailTakenBackToRecommender
    {
        public string Subject = "Details for Role Request";

        public string Body(string RequesterFullname, string remarks)
        {
            String tmpMessage;

            tmpMessage = "<html><meta charset=\"UTF-8\"><p><body dir=\"ltr\">";
            tmpMessage += "A role request assigned to you as recommender has been taken back by: <b>" + RequesterFullname + "</b> keeping remarks as: <b>" + remarks + "</b>. Please check the details in the Role Request Application.<br><br>";
            tmpMessage += "</body></p></meta></html>";

            return tmpMessage;
        }
    }
}