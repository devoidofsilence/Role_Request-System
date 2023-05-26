using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RoleRequest.Services;
using System.Data;
using RoleRequest.Models;
using RoleRequest.Models.Utils;
using System.Text;

namespace RoleRequest.Controllers
{
    [Authorize]
    public class RoleRequestController : BaseController
    {
        CommonService cs = null;
        RoleRequestService rrs = null;
        RoleRequestModel rrm = null;
        EmployeeModel emp = null;
        List<AppDomainModel> lstAppDomain = null;
        List<AppsModel> lstApps = null;
        List<AppRoleHeadModel> lstRoleHead = null;
        List<AppRolesModel> lstRoles = null;
        List<AppAccessLevelModel> lstAccessLevel = null;
        List<CoreModel> lstCoreModel = null;
        List<EmployeeModel> lstEmp = null;
        List<SelectListItem> departments = null;
        List<SelectListItem> branchLocations = null;
        List<SelectListItem> corporateTitles = null;
        List<SelectListItem> functionalTitles = null;

        //For Edit Mode 
        List<AppAccessLevelModel> aalmLst = null;
        List<AppRolesModel> armLst = null;

        public RoleRequestController()
        {
            cs = new CommonService();
            rrm = new RoleRequestModel();
            emp = cs.GetCurrentUserFromAD();
            rrs = new RoleRequestService();
            lstCoreModel = new List<CoreModel>();
            lstEmp = cs.GetEmployeesFromAD();

            //Start DDLs
            departments = cs.GetDepartmentsForDDL();
            branchLocations = cs.GetBranchLocationsForDDL();
            corporateTitles = cs.GetCorporateTitlesForDDL();
            functionalTitles = cs.GetFunctionalTitlesForDDL();
            //End DDLS

            //Start Lists
            lstAppDomain = new List<AppDomainModel>();
            lstApps = new List<AppsModel>();
            lstRoleHead = new List<AppRoleHeadModel>();
            lstRoles = new List<AppRolesModel>();
            lstAccessLevel = new List<AppAccessLevelModel>();
            //End Lists

            //For Edit Mode
            aalmLst = new List<AppAccessLevelModel>();
            armLst = new List<AppRolesModel>();
        }
        //
        // GET: /RoleRequest/

        private void Initializer()
        {
            try
            {
                if (emp != null)
                {
                    rrm.EmployeeFullName = emp.EmployeeFullName;
                    rrm.EmpSamAccountName = emp.SamAccountName;
                    if (rrm.RoleRequestId != 0)
                    {
                        string approvalBySAM = rrm.ApprovalBySAM.ToLower().Trim();
                        EmployeeModel empAppr = lstEmp.Where(m => m.SamAccountName.ToLower().Trim() == approvalBySAM).First();
                        if (empAppr != null)
                        {
                            rrm.ApprovalByEmpName = string.Concat(empAppr.EmployeeFullName, "(", empAppr.SamAccountName, ")");
                        }
                        string recommendationBySAM = rrm.RecommendationBySAM.ToLower().Trim();
                        EmployeeModel empRecomm = lstEmp.Where(m => m.SamAccountName.ToLower().Trim() == recommendationBySAM).First();
                        if (empRecomm != null)
                        {
                            rrm.RecommendationByEmpName = string.Concat(empRecomm.EmployeeFullName, "(", empRecomm.SamAccountName, ")");
                        }
                        rrm.editMode = true;
                    }
                }

                if (rrm.editMode)
                {
                    rrm.lstPrimaryRoleModel = cs.GetPrimaryRoleDetails(rrm.RoleRequestId);
                    //get differential role request history here - primary role detail
                    if (!string.IsNullOrEmpty(rrm.EmpSamAccountName))
                    {
                        rrm.lstPrimaryRoleModelLatestSavedRequest = cs.GetAllPrimaryRolesFromLatestSavedRequest(rrm.EmpSamAccountName);
                    }
                    foreach (var item in rrm.lstPrimaryRoleModelLatestSavedRequest)
                    {
                        rrm.lstPrimaryRoleModel.Where(x => (x.PrimaryRoleId == item.PrimaryRoleId)).Select(c => { c.LatestSavedRequestFlag = item.LatestSavedRequestFlag; return c; }).ToList();
                        rrm.lstPrimaryRoleModel.Where(x => (x.PrimaryRoleId == item.PrimaryRoleId && !x.ChangedFlag)).Select(c => { c.IsSelected = item.IsSelected; return c; }).ToList();
                    }
                    rrm.remarksList = cs.GetAllRemarksInThisRequest(rrm.RoleRequestId);
                }
                else
                {
                    rrm.lstPrimaryRoleModel = cs.GetAllPrimaryRoles();
                    //get differential role request history here - primary role detail
                    if (!string.IsNullOrEmpty(rrm.EmpSamAccountName))
                    {
                        rrm.lstPrimaryRoleModelLatestSavedRequest = cs.GetAllPrimaryRolesFromLatestSavedRequest(rrm.EmpSamAccountName);
                    }
                    foreach (var item in rrm.lstPrimaryRoleModelLatestSavedRequest)
                    {
                        rrm.lstPrimaryRoleModel.Where(x => x.PrimaryRoleId == item.PrimaryRoleId).Select(c => { c.LatestSavedRequestFlag = item.LatestSavedRequestFlag; c.IsSelected = item.IsSelected; c.ChangedFlag = item.ChangedFlag; return c; }).ToList();
                    }
                }

                lstCoreModel = cs.GetRoles();
                var queryDomain =
                        from domain in lstCoreModel
                        group domain by new { domain.DomainId, domain.DomainName } into newGroup
                        orderby newGroup.Key.DomainId
                        select newGroup;
                AppDomainModel rdm = null;
                AppsModel am = null;
                AppAccessLevelModel alm = null;
                AppRoleHeadModel arhm = null;
                AppRolesModel arm = null;
                foreach (var domainGroup in queryDomain)
                {
                    rdm = new AppDomainModel();
                    rdm.DomainId = Convert.ToInt32(domainGroup.Key.DomainId);
                    rdm.DomainName = domainGroup.Key.DomainName;
                    var filteredApps = (domainGroup.Where(m => m.DomainId == domainGroup.Key.DomainId)).GroupBy(m => new { m.AppId, m.AppName }).AsEnumerable();
                    foreach (var app in filteredApps)
                    {
                        am = new AppsModel();
                        am.AppId = Convert.ToInt32(app.Key.AppId);
                        am.AppName = app.Key.AppName;
                        if (rrm.editMode)
                        {
                            aalmLst = rrs.GetSavedAccessLevelFromRRFIDAndAppID(rrm.RoleRequestId, am.AppId);
                        }
                        else
                        {
                            aalmLst = rrs.GetAccessLevelsForAppId(am.AppId);
                        }
                        var filteredAccessLevels = (app.Where(m => m.AppId == app.Key.AppId)).GroupBy(m => new { m.AccessLevelId, m.AccessLevelName }).AsEnumerable();
                        foreach (var accesslevel in filteredAccessLevels)
                        {
                            alm = new AppAccessLevelModel();
                            alm.AccessLevelId = Convert.ToInt32(accesslevel.Key.AccessLevelId);
                            alm.AccessLevelName = accesslevel.Key.AccessLevelName.ToString();
                            if (rrm.editMode)
                            {
                                //
                                if (!string.IsNullOrEmpty(rrm.EmpSamAccountName))
                                {
                                    rrm.lstAccessLevelModelLatestSavedRequest = cs.GetAllAccessLevelsFromLatestSavedRequest(rrm.EmpSamAccountName, am.AppId);
                                }
                                foreach (var item in rrm.lstAccessLevelModelLatestSavedRequest)
                                {
                                    aalmLst.Where(x => (x.AccessLevelId == item.AccessLevelId)).Select(c => { c.LatestSavedRequestFlag = item.LatestSavedRequestFlag; return c; }).ToList();
                                    aalmLst.Where(x => (x.AccessLevelId == item.AccessLevelId && !x.ChangedFlag)).Select(c => { c.IsSelected = item.IsSelected; return c; }).ToList();
                                }
                                //

                                if (aalmLst.Count > 0)
                                {
                                    alm.IsSelected = aalmLst.Where(m => m.AccessLevelId == alm.AccessLevelId).First().IsSelected;
                                    alm.LatestSavedRequestFlag = aalmLst.Where(m => m.AccessLevelId == alm.AccessLevelId).First().LatestSavedRequestFlag;
                                }
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(rrm.EmpSamAccountName))
                                {
                                    rrm.lstAccessLevelModelLatestSavedRequest = cs.GetAllAccessLevelsFromLatestSavedRequest(rrm.EmpSamAccountName, am.AppId);
                                }
                                foreach (var item in rrm.lstAccessLevelModelLatestSavedRequest)
                                {
                                    aalmLst.Where(x => x.AccessLevelId == item.AccessLevelId).Select(c => { c.LatestSavedRequestFlag = item.LatestSavedRequestFlag; c.IsSelected = item.IsSelected; c.ChangedFlag = item.ChangedFlag; return c; }).ToList();
                                }

                                if (aalmLst.Count > 0)
                                {
                                    alm.IsSelected = aalmLst.Where(m => m.AccessLevelId == alm.AccessLevelId).First().IsSelected;
                                    alm.LatestSavedRequestFlag = aalmLst.Where(m => m.AccessLevelId == alm.AccessLevelId).First().LatestSavedRequestFlag;
                                    alm.ChangedFlag = aalmLst.Where(m => m.AccessLevelId == alm.AccessLevelId).First().ChangedFlag;
                                }
                            }
                            if (am.lstAccessLevel == null)
                            {
                                am.lstAccessLevel = new List<AppAccessLevelModel>();
                                //get differential role request history here - App Access Level detail
                            }
                            am.lstAccessLevel.Add(alm);
                        }

                        var filteredAppHeads = (app.Where(m => m.AppId == app.Key.AppId)).GroupBy(m => new { m.RoleHeadId, m.RoleHeadName }).AsEnumerable();
                        foreach (var appHeadGroup in filteredAppHeads)
                        {
                            arhm = new AppRoleHeadModel();
                            arhm.RoleHeadId = Convert.ToInt32(appHeadGroup.Key.RoleHeadId);
                            arhm.RoleHeadName = appHeadGroup.Key.RoleHeadName.ToString();
                            var filteredRoles = (appHeadGroup.Where(m => m.RoleHeadId == appHeadGroup.Key.RoleHeadId)).GroupBy(m => new { m.RoleId, m.RoleName }).AsEnumerable();
                            foreach (var roleGroup in filteredRoles)
                            {
                                arm = new AppRolesModel();
                                arm.RoleId = Convert.ToInt32(roleGroup.Key.RoleId);
                                arm.RoleName = roleGroup.Key.RoleName;
                                if (rrm.editMode && arm.RoleId != 0)
                                {
                                    armLst = rrs.GetSavedRolesFromRRFIDAndAppID(rrm.RoleRequestId, arm.RoleId);

                                    //
                                    if (!string.IsNullOrEmpty(rrm.EmpSamAccountName))
                                    {
                                        rrm.lstAppRolesModelLatestSavedRequest = cs.GetAllAppRolesFromLatestSavedRequest(rrm.EmpSamAccountName);
                                    }
                                    foreach (var item in rrm.lstAppRolesModelLatestSavedRequest)
                                    {
                                        armLst.Where(x => (x.RoleId == item.RoleId)).Select(c => { c.LatestSavedRequestFlag = item.LatestSavedRequestFlag; return c; }).ToList();
                                        armLst.Where(x => (x.RoleId == item.RoleId && !x.ChangedFlag)).Select(c => { c.IsSelected = item.IsSelected; return c; }).ToList();
                                    }
                                    //

                                    if (armLst.Count > 0)
                                    {
                                        arm.IsSelected = armLst.Where(m => m.RoleId == arm.RoleId).First().IsSelected;
                                        arm.LatestSavedRequestFlag = armLst.Where(m => m.RoleId == arm.RoleId).First().LatestSavedRequestFlag;
                                    }
                                }
                                else if (arm.RoleId != 0)
                                {
                                    armLst = rrs.GetRoles();

                                    if (!string.IsNullOrEmpty(rrm.EmpSamAccountName))
                                    {
                                        rrm.lstAppRolesModelLatestSavedRequest = cs.GetAllAppRolesFromLatestSavedRequest(rrm.EmpSamAccountName);
                                    }
                                    foreach (var item in rrm.lstAppRolesModelLatestSavedRequest)
                                    {
                                        armLst.Where(x => x.RoleId == item.RoleId).Select(c => { c.LatestSavedRequestFlag = item.LatestSavedRequestFlag; c.IsSelected = item.IsSelected; c.ChangedFlag = item.ChangedFlag; return c; }).ToList();
                                    }

                                    if (armLst.Count > 0)
                                    {
                                        arm.IsSelected = armLst.Where(m => m.RoleId == arm.RoleId).First().IsSelected;
                                        arm.LatestSavedRequestFlag = armLst.Where(m => m.RoleId == arm.RoleId).First().LatestSavedRequestFlag;
                                        arm.ChangedFlag = armLst.Where(m => m.RoleId == arm.RoleId).First().ChangedFlag;
                                    }
                                }
                                if (arhm.lstRolesModel == null)
                                {
                                    arhm.lstRolesModel = new List<AppRolesModel>();
                                    //get differential role request history here - App role head detail
                                }
                                arhm.lstRolesModel.Add(arm);
                            }
                            if (am.lstRoleHeads == null)
                            {
                                am.lstRoleHeads = new List<AppRoleHeadModel>();
                            }
                            am.lstRoleHeads.Add(arhm);
                        }
                        if (rdm.lstApps == null)
                        {
                            rdm.lstApps = new List<AppsModel>();
                        }
                        rdm.lstApps.Add(am);
                    }
                    lstAppDomain.Add(rdm);
                }
                rrm.secondaryRoleDomain = lstAppDomain.Where(m => m.DomainName == "SECONDARY").FirstOrDefault();
                rrm.flexcubeRoleDomain = lstAppDomain.Where(m => m.DomainName == "FLEXCUBE").FirstOrDefault();
                rrm.edmsRoleDomain = lstAppDomain.Where(m => m.DomainName == "EDMS").FirstOrDefault();
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
        }

        public ActionResult RoleRequest()
        {
            try
            {
                ViewData["Department"] = departments;
                ViewData["BranchLocation"] = branchLocations;
                ViewData["CorporateTitle"] = corporateTitles;
                ViewData["FunctionalTitle"] = functionalTitles;
                if (emp != null)
                {
                    rrm = cs.GetOngoingRoleRequestRecord(emp.SamAccountName);
                }
                Initializer();
                if ((rrm.RoleRequestId == 0 || rrm.RequestStatus == null || rrm.RequestStatus == "ASGN") || (rrm.RoleRequestId != 0 && (rrm.RequestStatus == "CORR" || rrm.RequestStatus == "CORA" || rrm.RequestStatus == "CORI" || rrm.RequestStatus == "TBCK")))
                {
                    ViewData["EnableFormSubmission"] = true;
                }
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
            return View(rrm);
        }

        public ActionResult RoleRequestByRRFId(int rrfID, string mode)
        {
            try
            {
                ViewData["Department"] = departments;
                ViewData["BranchLocation"] = branchLocations;
                ViewData["CorporateTitle"] = corporateTitles;

                rrm = cs.GetRoleRequestRecordByRRFId(rrfID);
                if (rrm != null && rrm.RoleRequestId != 0)
                {
                    emp = cs.GetUserFromADBySamName(rrm.EmpSamAccountName);
                }
                if (mode == "viewOnly")
                {
                    rrm.viewOnlyMode = true;
                    rrm.recommendMode = false;
                    rrm.approveMode = false;
                }
                Initializer();
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
            return View("RoleRequest", rrm);
        }

        [ChildActionOnly]
        public PartialViewResult RoleRequestByRRFIdChild(int rrfID, string mode)
        {
            try
            {
                ViewData["Department"] = departments;
                ViewData["BranchLocation"] = branchLocations;
                ViewData["CorporateTitle"] = corporateTitles;
                ViewData["FunctionalTitle"] = functionalTitles;

                rrm = cs.GetRoleRequestRecordByRRFId(rrfID);
                if (rrm != null && rrm.RoleRequestId != 0)
                {
                    emp = cs.GetUserFromADBySamName(rrm.EmpSamAccountName);
                }
                if (mode == "viewOnly")
                {
                    rrm.viewOnlyMode = true;
                    rrm.recommendMode = false;
                    rrm.approveMode = false;
                }
                if (mode == "viewOnlyRevoke")
                {
                    rrm.viewOnlyMode = true;
                    rrm.recommendMode = false;
                    rrm.approveMode = false;
                    rrm.revokeMode = true;
                }
                else if (mode == "recommend")
                {
                    rrm.recommendMode = true;
                    rrm.approveMode = false;
                    rrm.viewOnlyMode = false;
                    //rrm.RecommendationRemarks = null;
                    ViewData["EnableRecommendation"] = true;
                }
                else if (mode == "approve")
                {
                    rrm.recommendMode = false;
                    rrm.approveMode = true;
                    rrm.viewOnlyMode = false;
                    //rrm.ApprovalRemarks = null;
                    ViewData["EnableApproval"] = true;
                }
                Initializer();
                //get differential role request history here - primary role detail
                //get differential role request history here - App Access Level detail
                //get differential role request history here - App Role Head detail
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
            return PartialView("_RoleRequest", rrm);
        }

        [ChildActionOnly]
        public PartialViewResult RoleRequestByRRFIdChildForBMs(int sno, int rrfID, string mode)
        {
            try
            {
                ViewData["Department"] = departments;
                ViewData["BranchLocation"] = branchLocations;
                ViewData["CorporateTitle"] = corporateTitles;
                ViewData["FunctionalTitle"] = functionalTitles;

                rrm = cs.GetRoleRequestRecordByRRFId(rrfID);
                if (rrm != null && rrm.RoleRequestId != 0)
                {
                    emp = cs.GetUserFromADBySamName(rrm.EmpSamAccountName);
                }
                if (mode == "viewOnly")
                {
                    rrm.viewOnlyMode = true;
                    rrm.recommendMode = false;
                    rrm.approveMode = false;
                }
                if (mode == "viewOnlyRevoke")
                {
                    rrm.viewOnlyMode = true;
                    rrm.recommendMode = false;
                    rrm.approveMode = false;
                    rrm.revokeMode = true;
                }
                else if (mode == "recommend")
                {
                    rrm.recommendMode = true;
                    rrm.approveMode = false;
                    rrm.viewOnlyMode = false;
                    //rrm.RecommendationRemarks = null;
                    ViewData["EnableRecommendation"] = true;
                }
                else if (mode == "approve")
                {
                    rrm.recommendMode = false;
                    rrm.approveMode = true;
                    rrm.viewOnlyMode = false;
                    //rrm.ApprovalRemarks = null;
                    ViewData["EnableApproval"] = true;
                }
                Initializer();
                //get differential role request history here - primary role detail
                //get differential role request history here - App Access Level detail
                //get differential role request history here - App Role Head detail
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
            rrm.RoleRequestId = sno;
            return PartialView("_RoleRequestForBMs", rrm);
        }

        public ActionResult RoleRequestViewOnlyForBMs()
        {
            string primaryRoleNames = string.Empty;
            string secondaryAppDetails = string.Empty;
            List<int> rrfsListOfOOAndBMs = new List<int>()
                //{7565,7307,9493,7532};
            {7565,7307,9493,7532,7431,7011,6971,8112,6863,6576,4024,8248,8223,7176,6880,9474,3184,9562,3346,7926,7940,3138,8315,3850,8251,7103,4087,6041,7915,9591,8204,8322,8376,2480,8311,7339,6915,
            9448,6324,6990,9560,9544,8336,7979,1910,4373,6984,8301,8385,7089,7312,4218,3894,9479,8406,6682,9470,6288,9558,9567,7262,9563,5984,4128,2192,7948,7508,7739,7337,7791,6501,8138,7729,7961,
            6318,2699,3983,3203,8137,8308,6215,7310,7821,7308,9519,1894,6282,9457,7163,6027,4573,7182,7790,7687,6307,9569,8353,9535,4256,6193,7434,4539,7711,6242,6670,7061,7536,8193,6852,3170,8168,
            6368,4508,8228,8195,7693,6749,7005,4765,9453,2706,5964,9449,7129,4768,6113,8404,8360,7125,8316,7352,7670,7131,4507,9556,8187,4466,8210,6892,7418,4555,6898,8004,8386,442 ,8171,7612,2916,
            4460,3045,6492,7723,8007,6884,8032,7208,6912,6082,9540,6154,7045,3513,6908,8188,8077,7981,7776,6720,9574,8334,7450,7848,7488,7705,2634,3474,8365,9440,8393,8337,8217,6871,7549,4319,4369,
            8326,9442,8164,7142,4742,8099,8067,4386,3378,7370,7828,7245,2122,7982,7158,4622,7499,7634,8178,7265,7883,4368,9512,7133,7817,8358,6700,4417,6563,7172,7843,8374,7420,7692,1906,2485,3348,
            6932,6428,8263,8359,8190,767 ,7837,7029,7202,9500,7168,6655,8294,8153,9437,7400,8314,4671,8389,4247,8108,4709,7478,6520,7994,8216,6843,7349,3084,4496,2316,9498,8387,7751,6944,8384,6000,
            3558,7364,4377,8214,4660,6860,5975,7247,7819,9487,7718,9466,7222,9433,6213,8033,7920,6683,9420,2842,8392,6945,7610,8319,3048,6870,9469,7927,9461,4316,6373,8017,4281,8402,9579,6867,6897,
            7556,6824,7496,8050,7526,5938,3788,7858,6960,4215,7210,8218,7160,6899,7765,5970,5841,7105,8051,6869,6895,9584,7681,7870,6845,9514,7150,8179,7361,6964,668 ,8381,1832,8373,3529,7081,8169,
            8368,8046,8291,9570,4451,6921,6933};
            try
            {
                ViewData["rrfIDs"] = rrfsListOfOOAndBMs;
                return View();
            }
            catch (Exception ex)
            {

            }
            return View();
        }

        public ActionResult RoleRequestViewOnly(int rrfID)
        {
            ViewData["rrfID"] = rrfID;
            return View();
        }

        public ActionResult RoleRequestRecAppr(int rrfID, string mode)
        {
            try
            {
                rrm = cs.GetRoleRequestRecordByRRFId(rrfID);
                if (rrm != null && rrm.RoleRequestId != 0)
                {
                    emp = cs.GetUserFromADBySamName(rrm.EmpSamAccountName);
                }
                if (mode == "recommend")
                {
                    rrm.recommendMode = true;
                    rrm.approveMode = false;
                    rrm.viewOnlyMode = false;
                    //rrm.RecommendationRemarks = null;
                    ViewData["EnableRecommendation"] = true;
                }
                else if (mode == "approve")
                {
                    rrm.recommendMode = false;
                    rrm.approveMode = true;
                    rrm.viewOnlyMode = false;
                    //rrm.ApprovalRemarks = null;
                    ViewData["EnableApproval"] = true;
                }
                Initializer();
                //Build your Model (assuming from a database etc)
                RoleRequestModel model = rrm;

                //Store the model within the Session
                Session["Model"] = model;
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
            ViewData["rrfID"] = rrfID;
            return View(rrm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RoleRequest(RoleRequestModel rr, string Command)
        {
            try
            {
                if (rr.recommendMode)
                {
                    if (Session["Model"] != null)
                    {
                        RoleRequestModel olderModel = Session["Model"] as RoleRequestModel;

                        //Your logic here
                        olderModel.Remarks = rr.Remarks;
                        rr = olderModel;
                    }
                    if (Command == "Return")
                    {
                        rr.rejectionFlag = true;
                    }
                    ModelState.Remove("EmpSamAccountName");
                    ModelState.Remove("EmployeeFullName");
                    ModelState.Remove("RecommendationByEmpName");
                    ModelState.Remove("RecommendationBySAM");
                    ModelState.Remove("ApprovalByEmpName");
                    ModelState.Remove("ApprovalBySAM");
                    ModelState.Remove("ApprovalRemarks");
                    ModelState.Remove("HRIS_ID");
                    ModelState.Remove("EMAIL_ID");
                    ModelState.Remove("MOBILE_NUMBER");
                    ModelState.Remove("DepartmentId");
                    ModelState.Remove("BranchLocationId");
                }
                else if (rr.approveMode)
                {
                    if (Session["Model"] != null)
                    {
                        RoleRequestModel olderModel = Session["Model"] as RoleRequestModel;

                        //Your logic here
                        olderModel.Remarks = rr.Remarks;
                        rr = olderModel;
                    }
                    if (Command == "Return")
                    {
                        rr.rejectionFlag = true;
                    }
                    ModelState.Remove("EmpSamAccountName");
                    ModelState.Remove("EmployeeFullName");
                    ModelState.Remove("RecommendationByEmpName");
                    ModelState.Remove("RecommendationBySAM");
                    ModelState.Remove("ApprovalByEmpName");
                    ModelState.Remove("ApprovalBySAM");
                    ModelState.Remove("RecommendationRemarks");
                    ModelState.Remove("HRIS_ID");
                    ModelState.Remove("EMAIL_ID");
                    ModelState.Remove("MOBILE_NUMBER");
                    ModelState.Remove("DepartmentId");
                    ModelState.Remove("BranchLocationId");
                }
                else if (!rr.recommendMode && !rr.approveMode)
                {
                    ModelState.Remove("RecommendationRemarks");
                    ModelState.Remove("ApprovalRemarks");
                }
                if (ModelState.IsValid)
                {
                    //Dispose of the existing model in the Session
                    Session.Remove("Model");

                    rrs.SaveRoleRequest(rr);
                    TempData["UserMessage"] = new MessageVM() { CssClassName = "toast alert alert-success", Title = "Success!", Message = "Operation Successful." };
                    if (rr.recommendMode && !rr.approveMode)
                    {
                        return RedirectToAction("Recommendation", "Dashboard");
                    }
                    else if (!rr.recommendMode && rr.approveMode)
                    {
                        return RedirectToAction("Approval", "Dashboard");
                    }
                    return RedirectToAction("Requested", "Dashboard");
                }
                else if (rr.recommendMode && !rr.approveMode)
                {
                    ViewData["EnableRecommendation"] = true;
                    ViewData["rrfID"] = rr.RoleRequestId;
                    return View("RoleRequestRecAppr", rr);
                }
                else if (!rr.recommendMode && rr.approveMode)
                {
                    ViewData["EnableApproval"] = true;
                    ViewData["rrfID"] = rr.RoleRequestId;
                    return View("RoleRequestRecAppr", rr);
                }
                else
                {
                    ViewData["EnableFormSubmission"] = true;

                    ViewData["Department"] = departments;
                    ViewData["BranchLocation"] = branchLocations;
                    ViewData["CorporateTitle"] = corporateTitles;
                    //ViewData["FunctionalTitle"] = functionalTitles;
                }
            }
            catch (Exception)
            {
                TempData["UserMessage"] = new MessageVM() { CssClassName = "toast alert alert-error", Title = "Error!", Message = "Operation Failed." };
                throw;
            }
            return View(rr);
        }

        public JsonResult GetAllRemarksInThisRequest(int rrfId)
        {
            List<RemarksReport> lstRemarksReport = new List<RemarksReport>();
            lstRemarksReport = cs.GetAllRemarksInThisRequest(rrfId);
            return Json(lstRemarksReport, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public int TakeBackRequest(int rrfId, string remarks)
        {
            try
            {
                RoleRequestModel rrm = new RoleRequestModel();
                rrm = cs.GetRoleRequestRecordByRRFId(rrfId);
                rrm.takenBackFlag = true;
                rrm.editMode = true;
                rrm.Remarks = remarks;
                rrs.SaveRoleRequest(rrm);

                return 1;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
    }
}
