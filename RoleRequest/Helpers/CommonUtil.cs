using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RoleRequest.Helpers
{
    public static class CommonUtil
    {
        public static string ConvertToString(this object nonstring)
        {
            if (nonstring == null)
            {
                return String.Empty;
            }
            return nonstring.ToString();
        }
    }
}