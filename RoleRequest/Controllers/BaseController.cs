using RoleRequest.Models;
using RoleRequest.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RoleRequest.Controllers
{
    public class BaseController : Controller
    {
        //
        // GET: /Base/
        EmployeeModel emp = null;
        List<UserModel> uModelList = null;
        CommonService cs = null;
        int totalCountOfRoleRequest;
        int totalCountForRecommendation;
        int totalCountForApproval;
        int totalCountForAccessAction;
        int totalCountForEnrolledUsers;
        int totalCountForHRAdmin;
        int totalCountForEmailAndAD;
        int totalEmployeesListedForRevocation;
        int totalEmployeesAvailableForRevocation;
        public BaseController()
        {
            uModelList = new List<UserModel>();
            cs = new CommonService();
            totalCountOfRoleRequest = 0;
            totalCountForRecommendation = 0;
            totalCountForApproval = 0;
            totalCountForAccessAction = 0;
            totalCountForEnrolledUsers = 0;
            totalCountForHRAdmin = 0;
            totalCountForEmailAndAD = 0;
            totalEmployeesListedForRevocation = 0;
            totalEmployeesAvailableForRevocation = 0;
            emp = cs.GetCurrentUserFromAD();
            if (emp != null)
            {
                totalCountOfRoleRequest = cs.GetAllRoleRequestRecordsCount(emp.SamAccountName);
                totalCountForRecommendation = cs.GetRoleRequestRecordsCountForRecommendation(emp.SamAccountName);
                totalCountForApproval = cs.GetRoleRequestRecordsCountForApproval(emp.SamAccountName);
                totalCountForEnrolledUsers = cs.EnrolledUsersCountBySupervisorADName(emp.SamAccountName);

                ViewBag.TotalCountForRecommendation = totalCountForRecommendation;
                ViewBag.TotalCountForApproval = totalCountForApproval;
                ViewBag.TotalCountRoleRequestList = totalCountOfRoleRequest;
                ViewBag.TotalCountEnrolledUsers = totalCountForEnrolledUsers;
                uModelList = cs.GetUserFromUserTableByADUserName(emp.SamAccountName);
                if (uModelList != null && uModelList.Count > 0)
                {
                    emp.UserRole = new List<string>();
                    emp.UserRole.AddRange(uModelList.Select(x => x.UserRole));
                    if (emp.UserRole.Contains("ADMIN"))
                    {
                        totalCountForAccessAction = cs.GetRoleRequestRecordsCountForProvidingAccess();
                        ViewBag.TotalCountForAccessAction = totalCountForAccessAction;

                        totalCountForEmailAndAD = cs.GetUsersCountForADAndEmail(emp.SamAccountName);
                        ViewBag.TotalCountForEmailAndAD = totalCountForEmailAndAD;

                        totalEmployeesAvailableForRevocation = cs.GetRevokeListAvailableCountForRevocation(emp.SamAccountName);
                        ViewBag.TotalEmployeesAvailableForRevocation = totalEmployeesAvailableForRevocation;
                    }
                    else if (emp.UserRole.Contains("HR_ADMIN"))
                    {
                        totalCountForHRAdmin = cs.HRApprovalUsersCount(emp.SamAccountName);
                        ViewBag.TotalCountHRAdmin = totalCountForHRAdmin;

                        totalEmployeesListedForRevocation = cs.GetEmployeeCountListedForRevocation();
                        ViewBag.TotalEmployeesListedForRevocation = totalEmployeesListedForRevocation;
                    }
                    ViewBag.UserRole = emp.UserRole;
                }
            }
        }
    }
}
