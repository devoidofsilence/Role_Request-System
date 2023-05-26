using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RoleRequest.Models;
using RoleRequest.Services;
using RoleRequest.Filters;
using System.Text;
using RoleRequest.Models.Utils;

namespace RoleRequest.Controllers
{
    public class IDManagementController : BaseController
    {
        EnrollUserModel eum = null;
        IDMUserModel ium = null;
        CommonService cs = null;
        EnrollUserService eus = null;
        IDManagementService idms = null;
        HRService hrs = null;
        EmployeeModel em = null;
        RevokeRequestModel rrm = null;

        public IDManagementController()
        {
            eum = new EnrollUserModel();
            ium = new IDMUserModel();
            cs = new CommonService();
            eus = new EnrollUserService();
            idms = new IDManagementService();
            hrs = new HRService();
            em = new EmployeeModel();
            rrm = new RevokeRequestModel();
        }

        public ActionResult UserIDMDetailForm(int EnrollUserID)
        {
            ium = idms.EnrolledUserDetailByEnrollUserID(EnrollUserID);
            if (!String.IsNullOrEmpty(ium.SupervisionBy))
            {
                em = cs.GetUserFromADBySamName(ium.SupervisionBy.Trim());
                if (em != null && !String.IsNullOrEmpty(em.EmailId))
                {
                    ium.SupervisorEmailID = em.EmailId;
                }
            }
            return PartialView("_IDMForm", ium);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        //[ValidateAjaxAttribute]
        public ActionResult UserIDMDetailForm(IDMUserModel model)
        {
            try
            {
                if (!String.IsNullOrEmpty(model.ADUsername) && idms.CheckDuplicateADName(model.ADUsername))
                {
                    return this.Json(new
                    {
                        EnableError = true,
                        ErrorTitle = "Error",
                        ErrorMsg = "Active directory username already exists"
                    });
                }

                if (!String.IsNullOrEmpty(model.Email) && idms.CheckDuplicateEmailID(model.Email))
                {
                    return this.Json(new
                    {
                        EnableError = true,
                        ErrorTitle = "Error",
                        ErrorMsg = "Email ID already exists"
                    });
                }
                // Verification  
                if (ModelState.IsValid)
                {
                    model.AssignedIDMAdmin = User.Identity.Name.ToLower();
                    idms.SaveEnrollUser(model);
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
        public PartialViewResult IDMUsersList(string idmAdminADName)
        {
            try
            {
                ViewData["IDMUsersList"] = idms.GetUsersForADAndEmail(idmAdminADName);
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
            return PartialView("_UsersListInIDM");
        }

        public ActionResult IDMUsersListAJAX(string idmAdminADName)
        {
            try
            {
                ViewData["IDMUsersList"] = idms.GetUsersForADAndEmail(idmAdminADName);
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
            return PartialView("_UsersListInIDM");
        }

        [ChildActionOnly]
        public PartialViewResult IDMUsersListForRevocation(string idmADName)
        {
            try
            {
                ViewData["IDMUsersListForRevocation"] = idms.GetRevokeListAvailableForRevocation(idmADName);
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
            return PartialView("_UsersListInIDMForRevocation");
        }

        public ActionResult RevocationByRevokeId(int revokeId)
        {
            rrm = idms.GetRevocationDtlFromRevokeId(revokeId);
            if (rrm != null)
            {
                //if (String.IsNullOrEmpty(rrm.TakenBy))
                //{
                //    em = cs.GetCurrentUserFromAD();
                //    if (em != null)
                //    {
                //        rrm.TakenBy = em.SamAccountName;
                //    }
                //}
                rrm.aardmList = idms.GetAccessRevocationDtlFromRevokeId(revokeId);
                rrm.prrdmList = idms.GetPrimaryRoleRevocationDtlFromRevokeId(revokeId);
                int nonRevokeCount = 0;
                foreach (var item in rrm.aardmList)
                {
                    if (item.Status != "REV")
                    {
                        nonRevokeCount++;
                    }
                }
                foreach (var item in rrm.prrdmList)
                {
                    if (item.Status != "REV")
                    {
                        nonRevokeCount++;
                    }
                }
                rrm.AllRevoked = nonRevokeCount > 0 ? false : true;
                rrm.RoleRequestId = idms.GetLatestRoleRequestIdForEmpUsername(rrm.RevokeEmpUsername);
                ViewData["AccessRevocationDtl"] = rrm.aardmList;
                ViewData["PrimaryRoleRevocationDtl"] = rrm.prrdmList;
            }
            return View("Revocation", rrm);
        }

        [HttpPost]
        public int RevokeRequestAppAccess(int revokeId, int appId)
        {
            int i = 0;
            if (!idms.CheckIfRevokeIsTaken(revokeId))
            {
                i = idms.SetTakenBy(revokeId, User.Identity.Name);
            }
            else
            {
                i = 1;
            }
            int j = 0;
            if (i != 0)
            {
                j = idms.UpdateRevokeRequestAppAccess(revokeId, appId);
            }
            return j;
        }

        [HttpPost]
        public int RevokeRequestPrimRole(int revokeId, int primRoleId)
        {
            int i = 0;
            if (!idms.CheckIfRevokeIsTaken(revokeId))
            {
                i = idms.SetTakenBy(revokeId, User.Identity.Name);
            }
            else
            {
                i = 1;
            }
            int j = 0;
            if (i != 0)
            {
                j = idms.UpdateRevokeRequestPrimRole(revokeId, primRoleId);
            }
            return j;
        }

        [HttpPost]
        public int RevokeRequestAll(int revokeId)
        {
            int i = 0;
            if (!idms.CheckIfRevokeIsTaken(revokeId))
            {
                i = idms.SetTakenBy(revokeId, User.Identity.Name);
            }
            else
            {
                i = 1;
            }
            int j = 0;
            if (i != 0)
            {
                j = idms.UpdateAllRevokeRequestAppAccess(revokeId);
            }
            int k = 0;
            if (i + j != 0)
            {
                k = idms.UpdateAllRevokeRequestPrimRole(revokeId);
            }
            return i + j + k;
        }

        [HttpPost]
        public ActionResult SaveRevokeRequest(RevokeRequestModel rrm)
        {
            if (ModelState.IsValid)
            {
                //check if all items were revoked before saving here
                if (idms.CheckIfAllAppAccessesWereRevoked(rrm.RevokeId) && idms.CheckIfAllPrimRolesWereRevoked(rrm.RevokeId))
                {
                    rrm.RequestStatus = "DONE";
                    hrs.SaveRevokeRequest(rrm);
                    TempData["UserMessage"] = new MessageVM() { CssClassName = "toast alert alert-success", Title = "Success!", Message = "Operation Successful." };
                    return RedirectToAction("RevokeUserList", "Dashboard");
                }
                else
                {
                    ModelState.AddModelError("Error", "Please revoke all accesses before saving.");
                }
            }
            rrm.aardmList = idms.GetAccessRevocationDtlFromRevokeId(rrm.RevokeId);
            rrm.prrdmList = idms.GetPrimaryRoleRevocationDtlFromRevokeId(rrm.RevokeId);
            rrm.RoleRequestId = idms.GetLatestRoleRequestIdForEmpUsername(rrm.RevokeEmpUsername);
            ViewData["AccessRevocationDtl"] = rrm.aardmList;
            ViewData["PrimaryRoleRevocationDtl"] = rrm.prrdmList;
            return View("Revocation", rrm);
        }
    }
}
