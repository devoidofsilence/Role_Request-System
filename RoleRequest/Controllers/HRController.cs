using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RoleRequest.Models;
using RoleRequest.Services;
using RoleRequest.Filters;
using System.Text;
using System.Net;

namespace RoleRequest.Controllers
{
    public class HRController : BaseController
    {
        EnrollUserModel eum = null;
        HRUserModel hum = null;
        HRUserRejectModel hurm = null;
        CommonService cs = null;
        EnrollUserService eus = null;
        HRService hrs = null;
        EmployeeModel emp = null;
        RevokeRequestModelForModal rrmfm = null;

        public HRController()
        {
            eum = new EnrollUserModel();
            hum = new HRUserModel();
            hurm = new HRUserRejectModel();
            cs = new CommonService();
            eus = new EnrollUserService();
            hrs = new HRService();
            emp = new EmployeeModel();
            rrmfm = new RevokeRequestModelForModal();
        }

        public ActionResult UserHRDetailForm(int EnrollUserID)
        {
            hum = hrs.EnrolledUserDetailByEnrollUserID(EnrollUserID);
            return PartialView("_HRForm", hum);
        }

        public ActionResult UserHRRejectForm(int EnrollUserID)
        {
            hurm = hrs.EnrolledUserDetailByEnrollUserIDForRejection(EnrollUserID);
            return PartialView("_HRRejectForm", hurm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        //[ValidateAjaxAttribute]
        public ActionResult UserHRDetailForm(HRUserModel model)
        {
            try
            {
                if (!String.IsNullOrEmpty(model.EmployeeID) && hrs.CheckDuplicateEmployeeID(model.EmployeeID))
                {
                    return this.Json(new
                    {
                        EnableError = true,
                        ErrorTitle = "Error",
                        ErrorMsg = "Employee ID already exists"
                    });
                }

                // Verification  
                if (ModelState.IsValid)
                {
                    model.Status = "FFAE";
                    model.AssignedHRAdmin = User.Identity.Name.ToLower();
                    hrs.SaveEnrollUser(model);
                    // Info.  
                    return this.Json(new
                    {
                        EnableSuccess = true,
                        SuccessTitle = "Success",
                        SuccessMsg = "Operation Successful"
                    });
                }
            }
            catch (Exception ex)
            {
                // Info  
                Console.Write(ex);
            }
            // Info  
            //return this.Json(new
            //{
            //    EnableError = true,
            //    ErrorTitle = "Error",
            //    ErrorMsg = "Something goes wrong, please try again later"
            //});
            return Json(new
            {
                EnableError = true,
                Errors = from x in ModelState.Keys
                         where ModelState[x].Errors.Count > 0
                         select new
                         {
                             key = x,
                             errors = ModelState[x].Errors.
                                                    Select(y => y.ErrorMessage).
                                                    ToArray()
                         }
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        //[ValidateAjaxAttribute]
        public ActionResult UserHRRejectForm(HRUserRejectModel model)
        {
            try
            {
                // Verification  
                if (ModelState.IsValid)
                {
                    model.Status = "RJCT";
                    model.AssignedHRAdmin = User.Identity.Name.ToLower();
                    hrs.RejectEnrollFromHR(model);
                    // Info.  
                    return this.Json(new
                    {
                        EnableSuccess = true,
                        SuccessTitle = "Success",
                        SuccessMsg = "Operation Successful"
                    });
                }
            }
            catch (Exception ex)
            {
                // Info  
                Console.Write(ex);
            }
            // Info  
            //return this.Json(new
            //{
            //    EnableError = true,
            //    ErrorTitle = "Error",
            //    ErrorMsg = "Something goes wrong, please try again later"
            //});
            return Json(new
            {
                EnableError = true,
                Errors = from x in ModelState.Keys
                         where ModelState[x].Errors.Count > 0
                         select new
                         {
                             key = x,
                             errors = ModelState[x].Errors.
                                                    Select(y => y.ErrorMessage).
                                                    ToArray()
                         }
            }, JsonRequestBehavior.AllowGet);
        }

        [ChildActionOnly]
        public PartialViewResult HRUsersList(string hrAdminADName)
        {
            try
            {
                ViewData["HRUsersList"] = hrs.HRApprovalUsers(hrAdminADName);
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(Environment.NewLine);
                sb.Append("4. " + ex.Message);
                // flush every 20 seconds as you do it
                System.IO.File.AppendAllText(Server.MapPath("/log.txt"), sb.ToString());
                sb.Clear();
            }
            return PartialView("_UsersListInHR");
        }

        public ActionResult HRUsersListAJAX(string hrAdminADName)
        {
            try
            {
                ViewData["HRUsersList"] = hrs.HRApprovalUsers(hrAdminADName);
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(Environment.NewLine);
                sb.Append("4. " + ex.Message);
                // flush every 20 seconds as you do it
                System.IO.File.AppendAllText(Server.MapPath("/log.txt"), sb.ToString());
                sb.Clear();
            }
            return PartialView("_UsersListInHR");
        }

        [ChildActionOnly]
        public PartialViewResult HRUsersListForRevocation()
        {
            try
            {
                ViewData["HRUsersListForRevocation"] = hrs.GetEmployeeListAvailableForRevocation();
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(Environment.NewLine);
                sb.Append("4. " + ex.Message);
                // flush every 20 seconds as you do it
                System.IO.File.AppendAllText(Server.MapPath("/log.txt"), sb.ToString());
                sb.Clear();
            }
            return PartialView("_UsersListInHRForRevocation");
        }

        public ActionResult HRUsersListForRevocationAJAX()
        {
            try
            {
                ViewData["HRUsersListForRevocation"] = hrs.GetEmployeeListAvailableForRevocation();
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(Environment.NewLine);
                sb.Append("4. " + ex.Message);
                // flush every 20 seconds as you do it
                System.IO.File.AppendAllText(Server.MapPath("/log.txt"), sb.ToString());
                sb.Clear();
            }
            return PartialView("_UsersListInHRForRevocation");
        }

        public ActionResult OpenModalFormForRevoke(string EmpUsernameForRevoke)
        {
            if (HttpContext.Cache["Employees"] != null)
            {
                List<EmployeeModel> allEmplList = (List<EmployeeModel>)HttpContext.Cache["Employees"];
                rrmfm = new RevokeRequestModelForModal()
                {
                    RevokeEmpFullName = allEmplList.Where(x => x.SamAccountName == EmpUsernameForRevoke).FirstOrDefault().EmployeeFullName,
                    RevokeEmpUsername = EmpUsernameForRevoke,
                    ResignationDate = (DateTime.Today).ToString("MM/dd/yyyy"),
                    RequestedBy = User.Identity.Name
                };
            }
            return PartialView("_RevokeRequest", rrmfm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RevokeRequest(RevokeRequestModelForModal revokeRequestModel)
        {
            try
            {
                // Verification  
                if (ModelState.IsValid)
                {
                    string requestedBy = string.Empty;
                    emp = cs.GetCurrentUserFromAD();
                    if (emp != null)
                    {
                        requestedBy = emp.SamAccountName;
                    }
                    RevokeRequestModel rrm = new RevokeRequestModel()
                    {
                        RevokeEmpUsername = revokeRequestModel.RevokeEmpUsername,
                        RequestStatus = "INIT",
                        RequestedBy = requestedBy,
                        RevokeDate = DateTime.Now,
                        ResignationDate = Convert.ToDateTime(revokeRequestModel.ResignationDate)
                    };

                    int i = hrs.SaveRevokeRequest(rrm);
                    // Info.  
                    return this.Json(new
                    {
                        EnableSuccess = true,
                        SuccessTitle = "Success",
                        SuccessMsg = "Operation Successful"
                    });
                }
            }
            catch (Exception ex)
            {
                // Info  
                Console.Write(ex);
            }
            return Json(new
            {
                EnableError = true,
                Errors = from x in ModelState.Keys
                         where ModelState[x].Errors.Count > 0
                         select new
                         {
                             key = x,
                             errors = ModelState[x].Errors.
                                                    Select(y => y.ErrorMessage).
                                                    ToArray()
                         }
            }, JsonRequestBehavior.AllowGet);
        }
    }
}
