using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RoleRequest.Models;
using RoleRequest.Services;
using RoleRequest.Models.Utils;

namespace RoleRequest.Controllers
{
    [Authorize]
    public class ProvisionController : BaseController
    {
        ProvisionModel pm = null;
        ProvisionService ps = null;
        CommonService cs = null;
        EmployeeModel emp = null;
        public ProvisionController()
        {
            pm = new ProvisionModel();
            ps = new ProvisionService();
            cs = new CommonService();
            emp = cs.GetCurrentUserFromAD();
        }

        //
        // GET: /Provision/

        public ActionResult ProvisionByRRFAndProvisionId(int rrfID, int provisionID)
        {
            pm = ps.GetProvisionFromRRFIdAndProvisionId(rrfID);
            if (pm != null)
            {
                if (string.IsNullOrEmpty(pm.ProvidedBy))
                {
                    if (emp != null)
                    {
                        pm.ProvidedBy = emp.SamAccountName;
                    }
                }
                pm.apdmList = ps.GetAccessProvisionDtlFromRRFIdAndProvisionID(rrfID, provisionID);
                pm.prpdmList = ps.GetPrimaryRoleProvisionDtlFromRRFIdAndProvisionID(rrfID, provisionID);
                pm.additionalAccess = ps.GetAdditionalAccessProvisionDtlFromRRFIdAndProvisionID(rrfID, provisionID);
                ViewData["AccessProvisionDtl"] = pm.apdmList;
                ViewData["PrimaryRoleProvisionDtl"] = pm.prpdmList;
                ViewData["AdditionalAccessProvisionDtl"] = pm.additionalAccess;
            }
            return View("Provision", pm);
        }

        [ChildActionOnly]
        public PartialViewResult ProvisionByRRFAndProvisionIdChild(int rrfID, string mode)
        {
            pm = ps.GetProvisionFromRRFIdAndProvisionId(rrfID);
            if (pm != null)
            {
                if (String.IsNullOrEmpty(pm.ProvidedBy))
                {
                    emp = cs.GetCurrentUserFromAD();
                    if (emp != null)
                    {
                        pm.ProvidedBy = emp.SamAccountName;
                    }
                }
                if (mode == "viewOnly")
                {
                    pm.viewOnlyMode = true;
                }
                pm.apdmList = ps.GetAccessProvisionDtlFromRRFIdAndProvisionID(rrfID, pm.ProvisionId);
                pm.prpdmList = ps.GetPrimaryRoleProvisionDtlFromRRFIdAndProvisionID(rrfID, pm.ProvisionId);
                pm.additionalAccess = ps.GetAdditionalAccessProvisionDtlFromRRFIdAndProvisionID(rrfID, pm.ProvisionId);
                ViewData["AccessProvisionDtl"] = pm.apdmList;
                ViewData["PrimaryRoleProvisionDtl"] = pm.prpdmList;
                ViewData["AdditionalAccessProvisionDtl"] = pm.additionalAccess;
            }
            return PartialView("_Provision", pm);
        }

        public ActionResult RoleRequestAssignedViewOnly(int rrfID)
        {
            ViewData["rrfID"] = rrfID;
            return View();
        }

        [HttpPost]
        public int TakeRequest(int rrfId, int provisionId)
        {
            string providedBy = String.Empty;
            emp = cs.GetCurrentUserFromAD();
            if (emp != null)
            {
                providedBy = emp.SamAccountName;
            }
            ProvisionModel pm = new ProvisionModel()
            {
                ProvisionId = provisionId,
                RoleRequestId = rrfId,
                ProvidedBy = providedBy,
                ProvisionStatus = "INPR",
                saveRequest = false
            };
            int i = ps.SaveProvisionRequest(pm);
            return i;
        }

        [HttpPost]
        public ActionResult SaveRequest(ProvisionModel saveProvisionModel, string Command)
        {
            try
            {
                if (Command == "Return")
                {
                    saveProvisionModel.rejectionFlag = true;
                    ModelState.Remove("additionalAccess.AccessRemarks");
                    List<string> tempModel = ModelState.Keys.ToList();
                    foreach (var item in tempModel)
                    {
                        if (item.Contains("apdmList") || item.Contains("prpdmList"))
                        {
                            ModelState.Remove(item);
                        }
                    }
                }
                if (ModelState.IsValid)
                {
                    if (saveProvisionModel.rejectionFlag != true)
                    {
                        saveProvisionModel.ProvisionStatus = "DONE";
                    }
                    saveProvisionModel.saveRequest = true;

                    if (!String.IsNullOrEmpty(saveProvisionModel.RequestedByUsername))
                    {
                        emp = cs.GetUserFromADBySamName(saveProvisionModel.RequestedByUsername.Trim());
                        if (emp != null && !String.IsNullOrEmpty(emp.EmailId))
                        {
                            saveProvisionModel.RequestedByEmailID = emp.EmailId;
                        }
                    }

                    int i = ps.SaveProvisionRequest(saveProvisionModel);
                    TempData["UserMessage"] = new MessageVM() { CssClassName = "toast alert alert-success", Title = "Success!", Message = "Operation Successful." };
                    return RedirectToAction("AccessAdmin", "Dashboard");
                }
                else
                {
                    saveProvisionModel.ProvisionStatus = "INPR"; //set Provision Status to In Progress
                    ViewData["AccessProvisionDtl"] = saveProvisionModel.apdmList;
                    ViewData["PrimaryRoleProvisionDtl"] = saveProvisionModel.prpdmList;
                    ViewData["AdditionalAccessProvisionDtl"] = saveProvisionModel.additionalAccess;
                }
            }
            catch (Exception)
            {
                TempData["UserMessage"] = new MessageVM() { CssClassName = "toast alert alert-error", Title = "Error!", Message = "Operation Failed." };
                throw;
            }
            return View("Provision", saveProvisionModel);
        }
    }
}
