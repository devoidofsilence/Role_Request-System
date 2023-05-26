using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RoleRequest.Services;
using RoleRequest.Models;

namespace RoleRequest.Controllers
{
    public class DBPortController : Controller
    {
        //
        // GET: /DBPort/

        public ActionResult Index()
        {
            return View("IndexPost");
        }

        [HttpPost]
        public ActionResult IndexPost()
        {
            DBPortService dbps = new DBPortService();
            List<string> roleRequestEmpUsernames = dbps.GetAllRoleRequestEmpUsernames();
            List<RoleRequestModel> roleRequests = dbps.GetAllRoleRequests();
            foreach (var item in roleRequestEmpUsernames)
            {
                //if (item.ToLower() == "binam.sapkota" || item.ToLower() == "ramesh.raut")
                //{
                    List<int> roleRequestIDsForUsername = new List<int>();
                roleRequestIDsForUsername = roleRequests.Where(x => x.EmpSamAccountName == item).Select(c => c.RoleRequestId).ToList();
                int highestRoleRequestId = roleRequests.Where(x => x.EmpSamAccountName == item).Select(c => c.RoleRequestId).ToList().Max();
                int actingHighestRoleRequestId = 0;
                int hasIncompleteRequest = roleRequests.Where(x => (x.EmpSamAccountName == item && x.RequestStatus != "ASGN")).ToList().Count();
                if (hasIncompleteRequest > 0 && roleRequestIDsForUsername.Count > 1)
                {
                    actingHighestRoleRequestId = roleRequests.Where(x => (x.EmpSamAccountName == item && x.RequestStatus == "ASGN")).Select(c => c.RoleRequestId).ToList().Max();
                }
                List<PrimaryRoleModel> prmList = new List<PrimaryRoleModel>();
                prmList = dbps.GetAllPrimaryRoleDetailForEmpUsername(roleRequestIDsForUsername);
                List<int> primRoleIdsThatNeedToBeMadeCheckedInMaxRRFId = new List<int>();
                primRoleIdsThatNeedToBeMadeCheckedInMaxRRFId = prmList.Where(x => (x.IsSelected == true && x.RoleRequestId != highestRoleRequestId)).Select(c => c.PrimaryRoleId).Distinct().ToList();
                dbps.UpdateSelectedFlagForPrimRoles(highestRoleRequestId, primRoleIdsThatNeedToBeMadeCheckedInMaxRRFId);
                if (actingHighestRoleRequestId != 0)
                {
                    dbps.UpdateSelectedFlagForPrimRoles(actingHighestRoleRequestId, primRoleIdsThatNeedToBeMadeCheckedInMaxRRFId);
                }
                //For App Roles
                List<AppRolesModel> arList = new List<AppRolesModel>();
                arList = dbps.GetAllAppRoleDetailForEmpUsername(roleRequestIDsForUsername);
                List<int> appRoleIdsThatNeedToBeMadeCheckedInMaxRRFId = new List<int>();
                appRoleIdsThatNeedToBeMadeCheckedInMaxRRFId = arList.Where(x => (x.IsSelected == true && x.RoleRequestId != highestRoleRequestId)).Select(c => c.RoleId).Distinct().ToList();
                dbps.UpdateSelectedFlagForAppRole(highestRoleRequestId, appRoleIdsThatNeedToBeMadeCheckedInMaxRRFId);
                if (actingHighestRoleRequestId != 0)
                {
                    dbps.UpdateSelectedFlagForAppRole(actingHighestRoleRequestId, appRoleIdsThatNeedToBeMadeCheckedInMaxRRFId);
                }
                //For App Access Levels
                List<AppAccessLevelModel> aalList = new List<AppAccessLevelModel>();
                aalList = dbps.GetAllAppAccessLevelDetailForEmpUsername(roleRequestIDsForUsername);
                List<TempClassForDBPort> appAccessLevelIdsWithAppIdThatNeedToBeMadeCheckedInMaxRRFId = new List<TempClassForDBPort>();
                appAccessLevelIdsWithAppIdThatNeedToBeMadeCheckedInMaxRRFId = aalList.Where(x => (x.IsSelected == true && x.RoleRequestId != highestRoleRequestId)).Select(c => new TempClassForDBPort { AppId = c.AppId, AccessLevelId = c.AccessLevelId, RrfId = c.RoleRequestId }).Distinct().ToList();
                foreach (var accessLevelIdAppIdColl in appAccessLevelIdsWithAppIdThatNeedToBeMadeCheckedInMaxRRFId)
                {
                    if (accessLevelIdAppIdColl.AppId != 15)
                    {
                        if (appAccessLevelIdsWithAppIdThatNeedToBeMadeCheckedInMaxRRFId.Where(x => x.AppId == accessLevelIdAppIdColl.AppId).Count() > 1)
                        {
                            if (appAccessLevelIdsWithAppIdThatNeedToBeMadeCheckedInMaxRRFId.Where(x => x.AppId == accessLevelIdAppIdColl.AppId).Select(x => x.RrfId).Max() == accessLevelIdAppIdColl.RrfId)
                            {
                                dbps.UpdateSelectedFlagForAccessLevelForAnApp(highestRoleRequestId, accessLevelIdAppIdColl.AppId, accessLevelIdAppIdColl.AccessLevelId);
                                if (actingHighestRoleRequestId != 0)
                                {
                                    dbps.UpdateSelectedFlagForAccessLevelForAnApp(actingHighestRoleRequestId, accessLevelIdAppIdColl.AppId, accessLevelIdAppIdColl.AccessLevelId);
                                }
                            }
                        }
                        else
                        {
                            dbps.UpdateSelectedFlagForAccessLevelForAnApp(highestRoleRequestId, accessLevelIdAppIdColl.AppId, accessLevelIdAppIdColl.AccessLevelId);
                            if (actingHighestRoleRequestId != 0)
                            {
                                dbps.UpdateSelectedFlagForAccessLevelForAnApp(actingHighestRoleRequestId, accessLevelIdAppIdColl.AppId, accessLevelIdAppIdColl.AccessLevelId);
                            }
                        }
                    }
                    else
                    {
                        dbps.UpdateSelectedFlagForAccessLevelForAnApp(highestRoleRequestId, accessLevelIdAppIdColl.AppId, accessLevelIdAppIdColl.AccessLevelId);
                        if (actingHighestRoleRequestId != 0)
                        {
                            dbps.UpdateSelectedFlagForAccessLevelForAnApp(actingHighestRoleRequestId, accessLevelIdAppIdColl.AppId, accessLevelIdAppIdColl.AccessLevelId);
                        }
                    }
                }
                //}
            }
            return View();
        }

    }
}
