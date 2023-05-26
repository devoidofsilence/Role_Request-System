using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RoleRequest.Models;
using RoleRequest.Services;
using RoleRequest.Filters;
using System.Text;

namespace RoleRequest.Controllers
{
    public class EnrollUserController : BaseController
    {
        EnrollUserModel eum = null;
        CommonService cs = null;
        EnrollUserService eus = null;

        public EnrollUserController()
        {
            eum = new EnrollUserModel();
            cs = new CommonService();
            eus = new EnrollUserService();
        }

        //
        // GET: /EnrollUser/
        [ChildActionOnly]
        public PartialViewResult EnrollUser()
        {
            List<SelectListItem> departments = cs.GetDepartmentsForDDL();
            List<SelectListItem> branchLocations = cs.GetBranchLocationsForDDL();
            List<SelectListItem> corporateTitles = cs.GetCorporateTitlesForDDL();
            //List<SelectListItem> functionalTitles = cs.GetFunctionalTitlesForDDL();

            ViewData["Department"] = departments;
            ViewData["BranchLocation"] = branchLocations;
            ViewData["CorporateTitle"] = corporateTitles;
            //ViewData["FunctionalTitle"] = functionalTitles;
            return PartialView("_EnrollUser", eum);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        //[ValidateAjaxAttribute]
        public ActionResult EnrollUser(EnrollUserModel model)
        {
            try
            {
                // Verification  
                if (ModelState.IsValid)
                {
                    model.SupervisionBy = User.Identity.Name.ToLower();
                    eus.SaveEnrollUser(model);
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

            return Json(new { EnableError = true, Errors = from x in ModelState.Keys
                            where ModelState[x].Errors.Count > 0
                            select new
                            {
                                key = x,
                                errors = ModelState[x].Errors.
                                                       Select(y => y.ErrorMessage).
                                                       ToArray()
                            } }, JsonRequestBehavior.AllowGet);
        }

        [ChildActionOnly]
        public PartialViewResult EnrollUsersListBySupervisorADName(string supervisorADName)
        {
            try
            {
                ViewData["EnrollUserList"] = eus.EnrolledUsersBySupervisorADName(supervisorADName);
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
            return PartialView("_EnrollUserList");
        }

        public ActionResult EnrollUsersListBySupervisorADNameAJAX(string supervisorADName)
        {
            try
            {
                ViewData["EnrollUserList"] = eus.EnrolledUsersBySupervisorADName(supervisorADName);
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
            return PartialView("_EnrollUserList");
        }

        public ActionResult EnrollUserDetail(int EnrollUserID)
        {
            eum = eus.EnrolledUserDetailByEnrollUserID(EnrollUserID);
            return PartialView("_EnrollUserDetail", eum);
        }
    }
}
