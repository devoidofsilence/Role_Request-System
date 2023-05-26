using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RoleRequest.Services;
using RoleRequest.Models;
using System.DirectoryServices.AccountManagement;

namespace RoleRequest.Controllers
{
    public class CommonController : Controller
    {
        CommonService cs = null;
        public CommonController()
        {
            cs = new CommonService();
        }
        //
        // GET: /Common/
        //[OutputCache(Duration = int.MaxValue)]
        public JsonResult GetEmployeesJSON()
        {
            List<EmployeeModel> lstEmployees = new List<EmployeeModel>();
            lstEmployees = cs.GetEmployeesFromAD();

            return Json(lstEmployees, JsonRequestBehavior.AllowGet);
        }
    }
}
