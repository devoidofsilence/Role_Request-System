using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RoleRequest.Models;
using RoleRequest.Services;

namespace RoleRequest.Controllers
{
    [Authorize]
    public class DashboardController : BaseController
    {
        EmployeeModel emp = null;
        List<UserModel> uModelList = null;
        CommonService cs = null;
        EnrollUserService eus = null;
        HRService hrs = null;
        IDManagementService idms = null;
        //RoleRequestModel rrm = null;
        List<RoleRequestModel> rrmList = null;
        List<RoleRequestModel> forRecommendationList = null;
        List<RoleRequestModel> forApprovalList = null;
        List<ProvisionModel> forAccessActionList = null;
        List<EmployeeModel> lstEmp = null;

        public DashboardController()
        {
            uModelList = new List<UserModel>();
            cs = new CommonService();
            eus = new EnrollUserService();
            hrs = new HRService();
            idms = new IDManagementService();
            rrmList = new List<RoleRequestModel>();
            forRecommendationList = new List<RoleRequestModel>();
            forApprovalList = new List<RoleRequestModel>();
            forAccessActionList = new List<ProvisionModel>();
            lstEmp = cs.GetEmployeesFromAD();
            emp = cs.GetCurrentUserFromAD();
        }

        public void Initializer()
        {
            if (emp != null)
            {
                rrmList = cs.GetAllRoleRequestRecords(emp);
                uModelList = cs.GetUserFromUserTableByADUserName(emp.SamAccountName);
                if (uModelList != null && uModelList.Count > 0)
                {
                    emp.UserRole = new List<string>();
                    emp.UserRole.AddRange(uModelList.Select(x => x.UserRole));
                    if (emp.UserRole.Contains("ADMIN"))
                    {
                        forAccessActionList = cs.GetRoleRequestRecordsForProvidingAccess(emp.SamAccountName, lstEmp);
                    }
                }
                forRecommendationList = cs.GetRoleRequestRecordsForRecommendation(emp.SamAccountName, lstEmp);
                forApprovalList = cs.GetRoleRequestRecordsForApproval(emp.SamAccountName, lstEmp);
            }
        }

        //
        // GET: /Dashboard/
        public ActionResult Dashboard()
        {
            Initializer();
            ViewData["ForRecommendation"] = forRecommendationList;
            ViewData["ForApproval"] = forApprovalList;
            ViewData["ForAccessAction"] = forAccessActionList;
            ViewData["RoleRequestList"] = rrmList;

            if (uModelList != null && uModelList.Count > 0)
            {
                ViewData["UserRole"] = emp.UserRole;
            }

            return View();
        }

        public ActionResult Requested()
        {
            if (emp != null)
            {
                rrmList = cs.GetAllRoleRequestRecords(emp);
            }
            ViewData["RoleRequestList"] = rrmList;
            return View();
        }

        public ActionResult Recommendation()
        {
            if (emp != null)
            {
                forRecommendationList = cs.GetRoleRequestRecordsForRecommendation(emp.SamAccountName, lstEmp);
            }
            ViewData["ForRecommendation"] = forRecommendationList;

            return View();
        }

        public ActionResult Approval()
        {
            if (emp != null)
            {
                forApprovalList = cs.GetRoleRequestRecordsForApproval(emp.SamAccountName, lstEmp);
            }
            ViewData["ForApproval"] = forApprovalList;

            return View();
        }

        public ActionResult AccessAdmin()
        {
            if (emp != null)
            {
                forAccessActionList = cs.GetRoleRequestRecordsForProvidingAccess(emp.SamAccountName, lstEmp);
                //uModelList = cs.GetUserFromUserTableByADUserName(emp.SamAccountName);
                //if (uModelList != null && uModelList.Count > 0)
                //{
                //    emp.UserRole = new List<string>();
                //    emp.UserRole.AddRange(uModelList.Select(x => x.UserRole));
                //    if (emp.UserRole.Contains("ADMIN"))
                //    {
                //        forAccessActionList = cs.GetRoleRequestRecordsForProvidingAccess(emp.SamAccountName, lstEmp);
                //        ViewData["UserRole"] = emp.UserRole;
                //    }
                //}
            }
            ViewData["ForAccessAction"] = forAccessActionList;
            return View();
        }

        public ActionResult EnrollUser()
        {
            return View();
        }

        public ActionResult IDManagement()
        {
            return View();
        }

        public ActionResult RevokeUserList()
        {
            return View();
        }

        public ActionResult Revoke()
        {
            return View();
        }

        public ActionResult HRAdmin()
        {
            return View();
        }
    }
}
