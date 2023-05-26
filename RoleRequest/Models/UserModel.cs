using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RoleRequest.Models
{
    public class UserModel
    {
        public int UserID { get; set; }

        public string ADUsername{ get; set; }

        public string UserRole { get; set; }
    }
}