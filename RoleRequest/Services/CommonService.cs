using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RoleRequest.Models;
using System.Data.SqlClient;
using System.Data;
using RoleRequest.Helpers;
using System.Configuration;
using System.Reflection;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices;
using System.Web.Mvc;

namespace RoleRequest.Services
{
    public class CommonService
    {
        String connectionString = String.Empty;
        public CommonService()
        {
            connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        }

        //[OutputCache(Duration = int.MaxValue)]
        //get primary roles
        public List<PrimaryRoleModel> GetAllPrimaryRoles()
        {
            var primaryRoles = new List<PrimaryRoleModel>();
            String commandText = "SELECT * FROM DBO.PRIMARY_ROLE";

            using (SqlDataReader reader = SqlHelper.ExecuteReader(connectionString, commandText,
                CommandType.Text))
            {
                while (reader.Read())
                {
                    if (reader.HasRows)
                    {
                        PrimaryRoleModel prm = new PrimaryRoleModel()
                        {
                            PrimaryRoleId = (int)reader["PRIMARY_ROLE_ID"],
                            PrimaryRoleName = reader["ROLE_NAME"].ToString()
                        };
                        primaryRoles.Add(prm);
                    }
                }
            }
            return primaryRoles;
        }

        public List<PrimaryRoleModel> GetAllPrimaryRolesFromLatestSavedRequest(string employeeUsername)
        {
            var primaryRoles = new List<PrimaryRoleModel>();
            String commandText = @"WITH CTE AS
                                    (SELECT RR.*, PRD.PRIMARY_ROLE_ID, PR.ROLE_NAME, PRD.IS_CHECKED, PRD.CHANGED_FLAG 
                                    FROM ROLE_REQUEST RR 
                                    INNER JOIN PRIMARY_ROLE_DTL PRD ON RR.RRF_ID = PRD.RRF_ID
                                    INNER JOIN PRIMARY_ROLE PR ON PR.PRIMARY_ROLE_ID = PRD.PRIMARY_ROLE_ID
                                    WHERE EMPLOYEE_USERNAME = '" + employeeUsername.Trim() + @"' AND IS_RECOMMENDED = 1 AND IS_APPROVED = 1 AND REQUEST_STATUS = 'ASGN'),
                                    CTE2 AS
                                    (
                                    SELECT RRF_ID, PRIMARY_ROLE_ID, ROLE_NAME, IS_CHECKED, CHANGED_FLAG FROM CTE WHERE RRF_ID IN (SELECT MAX(RRF_ID) FROM CTE)
                                    )
                                    SELECT * FROM CTE2 WHERE IS_CHECKED = 1";

            using (SqlDataReader reader = SqlHelper.ExecuteReader(connectionString, commandText,
                CommandType.Text))
            {
                while (reader.Read())
                {
                    if (reader.HasRows)
                    {
                        PrimaryRoleModel prm = new PrimaryRoleModel()
                        {
                            PrimaryRoleId = (int)reader["PRIMARY_ROLE_ID"],
                            PrimaryRoleName = reader["ROLE_NAME"].ToString(),
                            IsSelected = Convert.ToBoolean(reader["IS_CHECKED"]),
                            ChangedFlag = !string.IsNullOrEmpty(reader["CHANGED_FLAG"].ToString()) ? Convert.ToBoolean(reader["CHANGED_FLAG"]) : false,
                            LatestSavedRequestFlag = true
                        };
                        primaryRoles.Add(prm);
                    }
                }
            }
            return primaryRoles;
        }

        public List<AppRolesModel> GetAllAppRolesFromLatestSavedRequest(string employeeUsername)
        {
            var appRoles = new List<AppRolesModel>();
            AppRolesModel arm = null;
            string commandText = @"WITH CTE AS
                                    (SELECT RR.*, AD.DOMAIN_ID, AD.DOMAIN_NAME, A.APP_ID, A.[APP_NAME], ARMD.ROLE_ID, ARMD.CHECKED_FLG, ARMD.CHANGED_FLAG, ARH.ROLE_HEAD_ID, ARH.ROLE_HEAD_NAME, AR.ROLE_NAME 
                                    FROM ROLE_REQUEST RR 
                                    INNER JOIN APP_ROLES_MAPPER_DTL ARMD ON RR.RRF_ID = ARMD.RRF_ID
                                    INNER JOIN APP_ROLES AR ON AR.ROLE_ID = ARMD.ROLE_ID 
                                    INNER JOIN APP_ROLE_HEAD ARH ON ARH.ROLE_HEAD_ID = AR.ROLE_HEAD_ID
                                    INNER JOIN APP A ON A.APP_ID = ARH.APP_ID
                                    INNER JOIN APP_DOMAIN AD ON AD.DOMAIN_ID = A.DOMAIN_ID
                                    WHERE EMPLOYEE_USERNAME = '" + employeeUsername.Trim() + @"' AND IS_RECOMMENDED = 1 AND IS_APPROVED = 1 AND REQUEST_STATUS = 'ASGN'),
                                    CTE2 AS 
                                    (
                                    SELECT RRF_ID, DOMAIN_ID, DOMAIN_NAME, APP_ID, [APP_NAME],ROLE_HEAD_ID, ROLE_HEAD_NAME, ROLE_ID, ROLE_NAME, CHECKED_FLG, CHANGED_FLAG FROM CTE WHERE RRF_ID IN (SELECT MAX(RRF_ID) FROM CTE)
                                    )
                                    SELECT * FROM CTE2 WHERE CHECKED_FLG =1";

            using (SqlDataReader reader = SqlHelper.ExecuteReader(connectionString, commandText,
                CommandType.Text))
            {
                while (reader.Read())
                {
                    if (reader.HasRows)
                    {
                        arm = new AppRolesModel()
                        {
                            RoleId = (int)reader["ROLE_ID"],
                            RoleName = reader["ROLE_NAME"].ToString(),
                            IsSelected = Convert.ToBoolean(reader["CHECKED_FLG"]),
                            ChangedFlag = !string.IsNullOrEmpty(reader["CHANGED_FLAG"].ToString()) ? Convert.ToBoolean(reader["CHANGED_FLAG"]) : false,
                            LatestSavedRequestFlag = true
                        };
                        appRoles.Add(arm);
                    }
                }
            }
            return appRoles;
        }

        public List<AppAccessLevelModel> GetAllAccessLevelsFromLatestSavedRequest(string employeeUsername, int appId)
        {
            var appAccessLevels = new List<AppAccessLevelModel>();
            AppAccessLevelModel arm = null;
            string commandText = @"WITH CTE AS
                                    (SELECT RR.*, ALMD.APP_ID, ALMD.ACCESS_LEVEL_ID, ALMD.CHECKED_FLG, ALMD.CHANGED_FLAG, A.[APP_NAME], AAL.ACCESS_LEVEL_NAME
                                    FROM ROLE_REQUEST RR 
                                    INNER JOIN ACCESS_LEVEL_MAPPER_DTL ALMD ON RR.RRF_ID = ALMD.RRF_ID 
                                    INNER JOIN APP A ON A.APP_ID = ALMD.APP_ID 
                                    INNER JOIN APP_ACCESS_LEVEL AAL ON AAL.ACCESS_LEVEL_ID = ALMD.ACCESS_LEVEL_ID 
                                    WHERE EMPLOYEE_USERNAME = '" + employeeUsername.Trim() + @"' AND IS_RECOMMENDED = 1 AND IS_APPROVED = 1 AND REQUEST_STATUS = 'ASGN' AND ALMD.APP_ID='" + appId.ToString() + @"'),
                                    CTE2 AS 
                                    (
                                    SELECT RRF_ID, APP_ID, [APP_NAME], ACCESS_LEVEL_ID, ACCESS_LEVEL_NAME, CHECKED_FLG, CHANGED_FLAG FROM CTE
WHERE RRF_ID IN (SELECT MAX(RRF_ID) FROM CTE)
                                    )
                                    SELECT * FROM CTE2 WHERE CHECKED_FLG =1";
            /*WITH CTE AS
                                    (SELECT RR.*, ALMD.APP_ID, ALMD.ACCESS_LEVEL_ID, ALMD.CHECKED_FLG, ALMD.CHANGED_FLAG, A.[APP_NAME], AAL.ACCESS_LEVEL_NAME
                                    FROM ROLE_REQUEST RR 
                                    INNER JOIN ACCESS_LEVEL_MAPPER_DTL ALMD ON RR.RRF_ID = ALMD.RRF_ID 
                                    INNER JOIN APP A ON A.APP_ID = ALMD.APP_ID 
                                    INNER JOIN APP_ACCESS_LEVEL AAL ON AAL.ACCESS_LEVEL_ID = ALMD.ACCESS_LEVEL_ID 
                                    WHERE EMPLOYEE_USERNAME = '" + employeeUsername.Trim() + @"' AND IS_RECOMMENDED = 1 AND IS_APPROVED = 1 AND REQUEST_STATUS = 'ASGN' AND ALMD.APP_ID='" + appId.ToString() + @"'),
                                    CTE2 AS 
                                    (
                                    SELECT RRF_ID, APP_ID, [APP_NAME], ACCESS_LEVEL_ID, ACCESS_LEVEL_NAME, CHECKED_FLG, CHANGED_FLAG FROM CTE
WHERE RRF_ID IN (SELECT MAX(RRF_ID) FROM CTE)
                                    )
                                    SELECT * FROM CTE2 WHERE CHECKED_FLG =1*/

            using (SqlDataReader reader = SqlHelper.ExecuteReader(connectionString, commandText,
                CommandType.Text))
            {
                while (reader.Read())
                {
                    if (reader.HasRows)
                    {
                        arm = new AppAccessLevelModel()
                        {
                            AccessLevelId = (int)reader["ACCESS_LEVEL_ID"],
                            AccessLevelName = reader["ACCESS_LEVEL_NAME"].ToString(),
                            IsSelected = Convert.ToBoolean(reader["CHECKED_FLG"]),
                            ChangedFlag = !string.IsNullOrEmpty(reader["CHANGED_FLAG"].ToString()) ? Convert.ToBoolean(reader["CHANGED_FLAG"]) : false,
                            LatestSavedRequestFlag = true
                        };
                        appAccessLevels.Add(arm);
                    }
                }
            }
            return appAccessLevels;
        }

        public List<PrimaryRoleModel> GetPrimaryRoleDetails(int rrfId)
        {
            var primaryRoles = new List<PrimaryRoleModel>();
            PrimaryRoleModel prm = null;
            string commandText = @"SELECT PRD.RRF_ID, PRD.PRIMARY_ROLE_ID, PR.ROLE_NAME, PRD.IS_CHECKED, PRD.CHANGED_FLAG FROM PRIMARY_ROLE_DTL PRD WITH (NOLOCK) INNER JOIN [PRIMARY_ROLE] PR WITH (NOLOCK) ON PRD.PRIMARY_ROLE_ID = PR.PRIMARY_ROLE_ID
                                    WHERE PRD.RRF_ID='" + rrfId + "'";

            using (SqlDataReader reader = SqlHelper.ExecuteReader(connectionString, commandText,
                CommandType.Text))
            {
                while (reader.Read())
                {
                    if (reader.HasRows)
                    {
                        prm = new PrimaryRoleModel()
                        {
                            PrimaryRoleId = (int)reader["PRIMARY_ROLE_ID"],
                            PrimaryRoleName = reader["ROLE_NAME"].ToString(),
                            IsSelected = (bool)reader["IS_CHECKED"],
                            ChangedFlag = !string.IsNullOrEmpty(reader["CHANGED_FLAG"].ToString()) ? (bool)reader["CHANGED_FLAG"] : false
                        };
                        primaryRoles.Add(prm);
                    }
                }
            }
            return primaryRoles;
        }

        public List<PrimaryRoleModel> GetPrimaryRoleDetailsOfLastApprovedRRF(string requestingEmployeeUsername)
        {
            var primaryRoles = new List<PrimaryRoleModel>();
            PrimaryRoleModel prm = null;
            string commandText = @"WITH CTE AS
                                    (SELECT RR.*, PRD.PRIMARY_ROLE_ID, PR.ROLE_NAME, PRD.IS_CHECKED 
                                    FROM ROLE_REQUEST RR 
                                    INNER JOIN PRIMARY_ROLE_DTL PRD ON RR.RRF_ID = PRD.RRF_ID
                                    INNER JOIN PRIMARY_ROLE PR ON PR.PRIMARY_ROLE_ID = PRD.PRIMARY_ROLE_ID
                                    WHERE EMPLOYEE_USERNAME = '" + requestingEmployeeUsername + @"' AND IS_RECOMMENDED = 1 AND IS_APPROVED = 1 AND REQUEST_STATUS = 'ASGN'),
                                    CTE2 AS
                                    (
                                    SELECT RRF_ID, PRIMARY_ROLE_ID, ROLE_NAME, IS_CHECKED FROM CTE WHERE RRF_ID IN (SELECT MAX(RRF_ID) FROM CTE)
                                    )
                                    SELECT * FROM CTE2 WHERE IS_CHECKED = 1";

            using (SqlDataReader reader = SqlHelper.ExecuteReader(connectionString, commandText,
                CommandType.Text))
            {
                while (reader.Read())
                {
                    if (reader.HasRows)
                    {
                        prm = new PrimaryRoleModel()
                        {
                            PrimaryRoleId = (int)reader["PRIMARY_ROLE_ID"],
                            PrimaryRoleName = reader["ROLE_NAME"].ToString(),
                            IsSelected = (Boolean)reader["IS_CHECKED"]
                        };
                        primaryRoles.Add(prm);
                    }
                }
            }
            return primaryRoles;
        }

        //get roles from Domain
        //[OutputCache(Duration = int.MaxValue)]
        public List<CoreModel> GetRoles()
        {
            var roles = new List<CoreModel>();
            CoreModel srm = null;
            string commandText = @"SELECT * FROM APP A
                                    INNER JOIN APP_DOMAIN AD
                                    ON A.DOMAIN_ID=AD.DOMAIN_ID
                                    LEFT OUTER JOIN ACCESS_LEVEL_MAPPER ALM
                                    ON A.APP_ID=ALM.APP_ID
                                    LEFT OUTER JOIN APP_ACCESS_LEVEL AAL
                                    ON AAL.ACCESS_LEVEL_ID = ALM.ACCESS_LEVEL_ID
                                    LEFT OUTER JOIN APP_ROLE_HEAD ARH
                                    ON ARH.APP_ID=ALM.APP_ID
                                    LEFT OUTER JOIN APP_ROLES AR
                                    ON AR.ROLE_HEAD_ID=ARH.ROLE_HEAD_ID
                                    ORDER BY AD.DOMAIN_ID";

            using (SqlDataReader reader = SqlHelper.ExecuteReader(connectionString, commandText,
                CommandType.Text))
            {
                while (reader.Read())
                {
                    if (reader.HasRows)
                    {
                        int rhId;
                        srm = new CoreModel()
                        {
                            DomainId = (int)reader["DOMAIN_ID"],
                            DomainName = reader["DOMAIN_NAME"].ToString(),
                            AppId = (int)reader["APP_ID"],
                            AppName = reader["APP_NAME"].ToString(),
                            AccessLevelId = (int)reader["ACCESS_LEVEL_ID"],
                            AccessLevelName = reader["ACCESS_LEVEL_NAME"].ToString(),
                            RoleHeadId = Int32.TryParse(reader["ROLE_HEAD_ID"].ToString(), out rhId) ? (int)reader["ROLE_HEAD_ID"] : 0,
                            RoleHeadName = reader["ROLE_HEAD_NAME"].ToString(),
                            RoleId = Int32.TryParse(reader["ROLE_ID"].ToString(), out rhId) ? (int)reader["ROLE_ID"] : 0,
                            RoleName = reader["ROLE_NAME"].ToString()
                        };
                        roles.Add(srm);
                    }
                }
            }
            return roles;
        }

        public List<EmployeeModel> GetEmployees()
        {
            var employees = new List<EmployeeModel>();
            EmployeeModel em = null;
            string commandText = @"SELECT EMP.*, BR.BRANCH_NAME, DEP.DEPARTMENT_NAME, CT.TITLE_NAME AS CORPORATE_TITLE, FT.TITLE_NAME AS FUNCTIONAL_TITLE
                                    FROM DBO.EMPLOYEE EMP INNER JOIN BRANCH BR ON EMP.BRANCH_ID = BR.BRANCH_ID
                                    INNER JOIN DEPARTMENT DEP ON EMP.DEPARTMENT_ID = DEP.DEPARTMENT_ID 
                                    INNER JOIN CORPORATE_TITLE CT ON EMP.CORPORATE_TITLE_ID = CT.TITLE_ID
                                    INNER JOIN FUNCTIONAL_TITLE FT ON EMP.FUNCTIONAL_TITLE_ID = FT.TITLE_ID";

            using (SqlDataReader reader = SqlHelper.ExecuteReader(connectionString, commandText,
                CommandType.Text))
            {
                while (reader.Read())
                {
                    if (reader.HasRows)
                    {
                        em = new EmployeeModel()
                        {
                            EmployeeId = reader["EMPLOYEE_ID"].ToString(),
                            EmployeeFirstName = reader["EMPLOYEE_FIRST_NAME"].ToString(),
                            EmployeeLastName = reader["EMPLOYEE_LAST_NAME"].ToString(),
                            EmployeeFullName = !String.IsNullOrEmpty(reader["EMPLOYEE_FIRST_NAME"].ToString()) && !String.IsNullOrEmpty(reader["EMPLOYEE_LAST_NAME"].ToString()) ? String.Concat(reader["EMPLOYEE_FIRST_NAME"].ToString(), " ", reader["EMPLOYEE_LAST_NAME"].ToString()) : String.Empty,
                            BranchId = (int)reader["BRANCH_ID"],
                            BranchName = reader["BRANCH_NAME"].ToString(),
                            DepartmentId = (int)reader["DEPARTMENT_ID"],
                            DepartmentName = reader["DEPARTMENT_NAME"].ToString(),
                            CorporateTitleId = (int)reader["CORPORATE_TITLE_ID"],
                            CorporateTitle = reader["CORPORATE_TITLE"].ToString(),
                            FunctionalTitleId = (int)reader["FUNCTIONAL_TITLE_ID"],
                            FunctionalTitle = reader["FUNCTIONAL_TITLE"].ToString()
                        };
                        employees.Add(em);
                    }
                }
            }
            return employees;
        }

        //[OutputCache(Duration = int.MaxValue)]
        public List<EmployeeModel> GetEmployeesFromAD()
        {
            if (HttpContext.Current.Cache["Employees"] != null)
            {
                return (List<EmployeeModel>)HttpContext.Current.Cache["Employees"];
            }

            // Creating a directory entry object by passing LDAP address
            List<EmployeeModel> lstEmployees = new List<EmployeeModel>();
            PrincipalContext context = new PrincipalContext(ContextType.Domain, "sbl.com", "tp_app", "$bl@!23$");
            PrincipalSearcher search = new PrincipalSearcher(new UserPrincipal(context));
            EmployeeModel em = null;
            foreach (UserPrincipal user in search.FindAll())
            {
                if (null != user && null != user.DisplayName)
                {
                    em = new EmployeeModel()
                    {
                        EmployeeId = user.EmployeeId,
                        SamAccountName = user.SamAccountName,
                        EmailId = user.EmailAddress,
                        EmployeeFirstName = user.GivenName,
                        EmployeeLastName = user.Surname,
                        EmployeeFullName = user.DisplayName,
                        BranchId = 0,
                        BranchName = "",
                        DepartmentId = 0,
                        DepartmentName = "",
                        CorporateTitleId = 0,
                        CorporateTitle = "",
                        FunctionalTitleId = 0,
                        FunctionalTitle = ""
                    };
                    lstEmployees.Add(em);
                }
                //if (user.SamAccountName.Trim().ToLower().Contains("deepika"))
                //{

                //}
            }
            HttpContext.Current.Cache["Employees"] = lstEmployees;
            return lstEmployees;
        }

        public List<UserModel> GetUsersFromUserTable()
        {
            var users = new List<UserModel>();
            UserModel u = null;
            string commandText = @"SELECT * FROM USERS";

            using (SqlDataReader reader = SqlHelper.ExecuteReader(connectionString, commandText,
                CommandType.Text))
            {
                while (reader.Read())
                {
                    if (reader.HasRows)
                    {
                        u = new UserModel()
                        {
                            UserID = (int)reader["USER_ID"],
                            ADUsername = reader["AD_USERNAME"].ToString().Trim().ToLower(),
                            UserRole = reader["USER_ROLE"].ToString(),
                        };
                        users.Add(u);
                    }
                }
            }
            return users;
        }

        public List<UserModel> GetUsersFromUserTableByRoleName(string RoleName)
        {
            var users = new List<UserModel>();
            UserModel u = null;
            string commandText = @"SELECT * FROM USERS WHERE USER_ROLE='" + RoleName + "'";

            using (SqlDataReader reader = SqlHelper.ExecuteReader(connectionString, commandText,
                CommandType.Text))
            {
                while (reader.Read())
                {
                    if (reader.HasRows)
                    {
                        u = new UserModel()
                        {
                            UserID = (int)reader["USER_ID"],
                            ADUsername = reader["AD_USERNAME"].ToString().Trim().ToLower(),
                            UserRole = reader["USER_ROLE"].ToString(),
                        };
                        users.Add(u);
                    }
                }
            }
            return users;
        }

        public List<UserModel> GetUserFromUserTableByADUserName(string ADUsername)
        {
            List<UserModel> uModelList = new List<UserModel>();
            try
            {
                string userName = ADUsername.ToLower().Trim();
                uModelList.AddRange(GetUsersFromUserTable().Where(x =>
                  x.ADUsername.ToLower().Trim() == userName));
            }
            catch (Exception ex)
            { //Exception logic here 
            }
            return uModelList;
        }

        ////[OutputCache(Duration = int.MaxValue, VaryByParam = "ADUsername")]
        public EmployeeModel GetCurrentUserFromAD()
        {
            List<EmployeeModel> emplList = GetEmployeesFromAD();
            EmployeeModel emp = new EmployeeModel();
            try
            {
                string userName = (HttpContext.Current.User.Identity.Name.ToString()).ToLower().Trim();
                emp = emplList.Where(x =>
                  x.SamAccountName.ToLower().Trim() == userName).FirstOrDefault();
            }
            catch (Exception ex)
            { //Exception logic here 
            }
            return emp;
        }

        //[OutputCache(Duration = int.MaxValue, VaryByParam = "SamName")]
        public EmployeeModel GetUserFromADBySamName(string SamName)
        {
            List<EmployeeModel> emplList = GetEmployeesFromAD();
            EmployeeModel emp = new EmployeeModel();
            try
            {
                string userName = SamName.ToLower().Trim();
                emp = emplList.Where(x =>
                  x.SamAccountName.ToLower().Trim() == userName).FirstOrDefault();
            }
            catch (Exception ex)
            { //Exception logic here 
            }
            return emp;
        }

        public RoleRequestModel GetOngoingRoleRequestRecord(string employeeUserName)
        {
            RoleRequestModel rrm = new RoleRequestModel();
            string commandText = @"SELECT TOP 1 * FROM ROLE_REQUEST WHERE EMPLOYEE_USERNAME='" + employeeUserName + "' AND REQUEST_STATUS<>'ASGN' ORDER BY RRF_ID DESC";

            using (SqlDataReader reader = SqlHelper.ExecuteReader(connectionString, commandText,
                CommandType.Text))
            {
                while (reader.Read())
                {
                    if (reader.HasRows)
                    {
                        rrm.RoleRequestId = (int)reader["RRF_ID"];
                        rrm.RoleRequestDate = reader["REQUEST_DATE"].ToString();
                        rrm.EmpSamAccountName = reader["EMPLOYEE_USERNAME"].ToString();
                        rrm.InputLimitAmt = (decimal)reader["INPUT_LIMIT_AMT"];
                        rrm.AuthorizationLimitAmt = (decimal)reader["AUTHORIZATION_LIMIT_AMT"];
                        rrm.AdditionalRequest = reader["ADDITIONAL_REQUEST"].ToString();
                        rrm.IsRecommended = (bool)reader["IS_RECOMMENDED"];
                        rrm.RecommendationBySAM = reader["RECOMMENDATION_BY"].ToString();
                        rrm.IsApproved = (bool)reader["IS_APPROVED"];
                        rrm.ApprovalBySAM = reader["APPROVAL_BY"].ToString();
                        rrm.Remarks = reader["REMARKS"].ToString();
                        rrm.BranchLocationId = reader["BRANCH_LOCATION_ID"].ToString();
                        rrm.DepartmentId = reader["DEPARTMENT_ID"].ToString();
                        rrm.FunctionalTitle = reader["FUNCTIONAL_TITLE"].ToString();
                        rrm.CorporateTitleId = reader["CORPORATE_TITLE_ID"].ToString();
                        rrm.RequestStatus = reader["REQUEST_STATUS"].ToString();
                        rrm.HRIS_ID = reader["HRIS_ID"].ToString();
                        rrm.FLEX_ID = reader["FLEX_ID"].ToString();
                        rrm.EMAIL_ID = reader["EMAIL_ID"].ToString();
                        rrm.MOBILE_NUMBER = reader["MOBILE_NUMBER"].ToString();
                    }
                }
            }
            return rrm;
        }

        public List<RoleRequestModel> GetAllRoleRequestRecords(EmployeeModel employee)
        {
            List<RoleRequestModel> rrmList = new List<RoleRequestModel>();
            RoleRequestModel rrm = null;
            string commandText = @"SELECT RR.*, BRN.BRANCH_NAME, DPT.DEPARTMENT_NAME, PRV.PROVIDED_BY FROM ROLE_REQUEST RR INNER JOIN BRANCH BRN ON RR.BRANCH_LOCATION_ID = BRN.BRANCH_ID INNER JOIN DEPARTMENT DPT ON RR.DEPARTMENT_ID = DPT.DEPARTMENT_ID LEFT OUTER JOIN PROVISION PRV ON RR.RRF_ID = PRV.RRF_ID WHERE RR.EMPLOYEE_USERNAME='" + employee.SamAccountName + "' ORDER BY CASE WHEN RR.REQUEST_STATUS = 'INIT' THEN 1 WHEN RR.REQUEST_STATUS = 'CORR' THEN 2 WHEN RR.REQUEST_STATUS = 'RECC' THEN 3 WHEN RR.REQUEST_STATUS = 'CORA' THEN 4 WHEN RR.REQUEST_STATUS = 'APPR' THEN 5 ELSE 6 END ASC";

            using (SqlDataReader reader = SqlHelper.ExecuteReader(connectionString, commandText,
                CommandType.Text))
            {
                while (reader.Read())
                {
                    if (reader.HasRows)
                    {
                        rrm = new RoleRequestModel()
                        {
                            RoleRequestId = (int)reader["RRF_ID"],
                            RoleRequestDate = reader["REQUEST_DATE"].ToString(),
                            EmpSamAccountName = reader["EMPLOYEE_USERNAME"].ToString(),
                            InputLimitAmt = (decimal)reader["INPUT_LIMIT_AMT"],
                            AuthorizationLimitAmt = (decimal)reader["AUTHORIZATION_LIMIT_AMT"],
                            AdditionalRequest = reader["ADDITIONAL_REQUEST"].ToString(),
                            IsRecommended = (bool)reader["IS_RECOMMENDED"],
                            RecommendationBySAM = reader["RECOMMENDATION_BY"].ToString(),
                            IsApproved = (bool)reader["IS_APPROVED"],
                            ApprovalBySAM = reader["APPROVAL_BY"].ToString(),
                            Remarks = reader["REMARKS"].ToString(),
                            BranchLocationId = reader["BRANCH_LOCATION_ID"].ToString(),
                            BranchName = reader["BRANCH_NAME"].ToString(),
                            DepartmentId = reader["DEPARTMENT_ID"].ToString(),
                            DepartmentName = reader["DEPARTMENT_NAME"].ToString(),
                            FunctionalTitle = reader["FUNCTIONAL_TITLE"].ToString(),
                            CorporateTitleId = reader["CORPORATE_TITLE_ID"].ToString(),
                            RequestStatus = reader["REQUEST_STATUS"].ToString(),
                            HRIS_ID = reader["HRIS_ID"].ToString(),
                            FLEX_ID = reader["FLEX_ID"].ToString(),
                            EMAIL_ID = reader["EMAIL_ID"].ToString(),
                            MOBILE_NUMBER = reader["MOBILE_NUMBER"].ToString(),
                            AccessGiver = reader["PROVIDED_BY"].ToString(),
                            EmployeeFullName = employee.EmployeeFullName
                        };
                        rrmList.Add(rrm);
                    }
                }
            }
            return rrmList;
        }
        public int GetAllRoleRequestRecordsCount(string employeeUsername)
        {
            int totalRequestCount = 0;
            string commandText = @"SELECT COUNT(*) REQUEST_COUNT FROM ROLE_REQUEST WHERE EMPLOYEE_USERNAME='" + employeeUsername + "'";

            using (SqlDataReader reader = SqlHelper.ExecuteReader(connectionString, commandText,
                CommandType.Text))
            {
                while (reader.Read())
                {
                    if (reader.HasRows)
                    {
                        totalRequestCount = (int)reader["REQUEST_COUNT"];
                    }
                }
            }
            return totalRequestCount;
        }

        public RoleRequestModel GetRoleRequestRecordByRRFId(int rrfId)
        {
            RoleRequestModel rrm = new RoleRequestModel();
            string commandText = @"SELECT TOP 1 * FROM ROLE_REQUEST WHERE RRF_ID='" + rrfId + "'";

            using (SqlDataReader reader = SqlHelper.ExecuteReader(connectionString, commandText,
                CommandType.Text))
            {
                while (reader.Read())
                {
                    if (reader.HasRows)
                    {
                        rrm.RoleRequestId = (int)reader["RRF_ID"];
                        rrm.RoleRequestDate = reader["REQUEST_DATE"].ToString();
                        rrm.EmpSamAccountName = reader["EMPLOYEE_USERNAME"].ToString();
                        rrm.InputLimitAmt = (decimal)reader["INPUT_LIMIT_AMT"];
                        rrm.AuthorizationLimitAmt = (decimal)reader["AUTHORIZATION_LIMIT_AMT"];
                        rrm.AdditionalRequest = reader["ADDITIONAL_REQUEST"].ToString();
                        rrm.IsRecommended = (bool)reader["IS_RECOMMENDED"];
                        rrm.RecommendationBySAM = reader["RECOMMENDATION_BY"].ToString();
                        rrm.IsApproved = (bool)reader["IS_APPROVED"];
                        rrm.ApprovalBySAM = reader["APPROVAL_BY"].ToString();
                        rrm.Remarks = reader["REMARKS"].ToString();
                        rrm.BranchLocationId = reader["BRANCH_LOCATION_ID"].ToString();
                        rrm.DepartmentId = reader["DEPARTMENT_ID"].ToString();
                        rrm.FunctionalTitle = reader["FUNCTIONAL_TITLE"].ToString();
                        rrm.CorporateTitleId = reader["CORPORATE_TITLE_ID"].ToString();
                        rrm.RequestStatus = reader["REQUEST_STATUS"].ToString();
                        rrm.HRIS_ID = reader["HRIS_ID"].ToString();
                        rrm.FLEX_ID = reader["FLEX_ID"].ToString();
                        rrm.EMAIL_ID = reader["EMAIL_ID"].ToString();
                        rrm.MOBILE_NUMBER = reader["MOBILE_NUMBER"].ToString();
                    }
                }
            }
            return rrm;
        }

        public List<RoleRequestModel> GetRoleRequestRecordsForRecommendation(string employeeUserName, List<EmployeeModel> lstEmp)
        {
            RoleRequestModel rrm = null;
            List<RoleRequestModel> rrmList = new List<RoleRequestModel>();
            string empSamAccountName = string.Empty;
            //String commandText = @"SELECT RR.*, BRN.BRANCH_NAME FROM ROLE_REQUEST RR INNER JOIN BRANCH BRN ON RR.BRANCH_LOCATION_ID = BRN.BRANCH_ID WHERE RR.RECOMMENDATION_BY='" + employeeUserName + "' AND RR.REQUEST_STATUS = 'INIT' ORDER BY CASE WHEN RR.REQUEST_STATUS = 'INIT' THEN 1 WHEN RR.REQUEST_STATUS = 'CORR' THEN 2 WHEN RR.REQUEST_STATUS = 'RECC' THEN 3 WHEN RR.REQUEST_STATUS = 'CORA' THEN 4 WHEN RR.REQUEST_STATUS = 'APPR' THEN 5 ELSE 6 END ASC";
            string commandText = @"SELECT RR.*, BRN.BRANCH_NAME, DPT.DEPARTMENT_NAME, PRV.PROVIDED_BY FROM ROLE_REQUEST RR INNER JOIN BRANCH BRN ON RR.BRANCH_LOCATION_ID = BRN.BRANCH_ID INNER JOIN DEPARTMENT DPT ON RR.DEPARTMENT_ID = DPT.DEPARTMENT_ID LEFT OUTER JOIN PROVISION PRV ON RR.RRF_ID = PRV.RRF_ID WHERE RR.RECOMMENDATION_BY='" + employeeUserName + "' ORDER BY CASE WHEN RR.REQUEST_STATUS = 'INIT' THEN 1 WHEN RR.REQUEST_STATUS = 'CORR' THEN 2 WHEN RR.REQUEST_STATUS = 'RECC' THEN 3 WHEN RR.REQUEST_STATUS = 'CORA' THEN 4 WHEN RR.REQUEST_STATUS = 'APPR' THEN 5 ELSE 6 END ASC";

            using (SqlDataReader reader = SqlHelper.ExecuteReader(connectionString, commandText,
                CommandType.Text))
            {
                while (reader.Read())
                {
                    if (reader.HasRows)
                    {
                        rrm = new RoleRequestModel()
                        {
                            RoleRequestId = (int)reader["RRF_ID"],
                            RoleRequestDate = reader["REQUEST_DATE"].ToString(),
                            EmpSamAccountName = reader["EMPLOYEE_USERNAME"].ToString(),
                            InputLimitAmt = (decimal)reader["INPUT_LIMIT_AMT"],
                            AuthorizationLimitAmt = (decimal)reader["AUTHORIZATION_LIMIT_AMT"],
                            AdditionalRequest = reader["ADDITIONAL_REQUEST"].ToString(),
                            IsRecommended = (bool)reader["IS_RECOMMENDED"],
                            RecommendationBySAM = reader["RECOMMENDATION_BY"].ToString(),
                            IsApproved = (bool)reader["IS_APPROVED"],
                            ApprovalBySAM = reader["APPROVAL_BY"].ToString(),
                            Remarks = reader["REMARKS"].ToString(),
                            BranchLocationId = reader["BRANCH_LOCATION_ID"].ToString(),
                            BranchName = reader["BRANCH_NAME"].ToString(),
                            DepartmentId = reader["DEPARTMENT_ID"].ToString(),
                            DepartmentName = reader["DEPARTMENT_NAME"].ToString(),
                            FunctionalTitle = reader["FUNCTIONAL_TITLE"].ToString(),
                            CorporateTitleId = reader["CORPORATE_TITLE_ID"].ToString(),
                            RequestStatus = reader["REQUEST_STATUS"].ToString(),
                            HRIS_ID = reader["HRIS_ID"].ToString(),
                            FLEX_ID = reader["FLEX_ID"].ToString(),
                            EMAIL_ID = reader["EMAIL_ID"].ToString(),
                            MOBILE_NUMBER = reader["MOBILE_NUMBER"].ToString(),
                            AccessGiver = reader["PROVIDED_BY"].ToString()
                        };
                        empSamAccountName = rrm.EmpSamAccountName.ToLower().Trim();
                        rrm.EmployeeFullName = lstEmp.Where(x => x.SamAccountName.ToLower().Trim() == empSamAccountName).Select(m => (m.EmployeeFirstName + " " + m.EmployeeLastName)).FirstOrDefault();
                        rrmList.Add(rrm);
                    }
                }
            }
            return rrmList;
        }

        public int GetRoleRequestRecordsCountForRecommendation(string employeeUserName)
        {
            int totalRequestCount = 0;
            string commandText = @"SELECT COUNT(*) REQUEST_COUNT FROM ROLE_REQUEST WHERE RECOMMENDATION_BY='" + employeeUserName + "'";

            using (SqlDataReader reader = SqlHelper.ExecuteReader(connectionString, commandText,
                CommandType.Text))
            {
                while (reader.Read())
                {
                    if (reader.HasRows)
                    {
                        totalRequestCount = (int)reader["REQUEST_COUNT"];
                    }
                }
            }
            return totalRequestCount;
        }

        public List<RoleRequestModel> GetRoleRequestRecordsForApproval(string employeeUserName, List<EmployeeModel> lstEmp)
        {
            List<RoleRequestModel> rrmList = new List<RoleRequestModel>();
            RoleRequestModel rrm = null;
            string empSamAccountName = string.Empty;
            //String commandText = @"SELECT RR.*, BRN.BRANCH_NAME FROM ROLE_REQUEST RR INNER JOIN BRANCH BRN ON RR.BRANCH_LOCATION_ID = BRN.BRANCH_ID WHERE RR.APPROVAL_BY='" + employeeUserName + "' AND RR.REQUEST_STATUS = 'RECC' ORDER BY CASE WHEN RR.REQUEST_STATUS = 'INIT' THEN 1 WHEN RR.REQUEST_STATUS = 'CORR' THEN 2 WHEN RR.REQUEST_STATUS = 'RECC' THEN 3 WHEN RR.REQUEST_STATUS = 'CORA' THEN 4 WHEN RR.REQUEST_STATUS = 'APPR' THEN 5 ELSE 6 END ASC";
            string commandText = @"SELECT RR.*, BRN.BRANCH_NAME, DPT.DEPARTMENT_NAME, PRV.PROVIDED_BY FROM ROLE_REQUEST RR INNER JOIN BRANCH BRN ON RR.BRANCH_LOCATION_ID = BRN.BRANCH_ID INNER JOIN DEPARTMENT DPT ON RR.DEPARTMENT_ID = DPT.DEPARTMENT_ID LEFT OUTER JOIN PROVISION PRV on RR.RRF_ID = PRV.RRF_ID WHERE RR.APPROVAL_BY='" + employeeUserName + "' ORDER BY CASE WHEN RR.REQUEST_STATUS = 'INIT' THEN 1 WHEN RR.REQUEST_STATUS = 'CORR' THEN 2 WHEN RR.REQUEST_STATUS = 'RECC' THEN 3 WHEN RR.REQUEST_STATUS = 'CORA' THEN 4 WHEN RR.REQUEST_STATUS = 'APPR' THEN 5 ELSE 6 END ASC";

            using (SqlDataReader reader = SqlHelper.ExecuteReader(connectionString, commandText,
                CommandType.Text))
            {
                while (reader.Read())
                {
                    if (reader.HasRows)
                    {
                        rrm = new RoleRequestModel()
                        {
                            RoleRequestId = (int)reader["RRF_ID"],
                            RoleRequestDate = reader["REQUEST_DATE"].ToString(),
                            EmpSamAccountName = reader["EMPLOYEE_USERNAME"].ToString(),
                            InputLimitAmt = (decimal)reader["INPUT_LIMIT_AMT"],
                            AuthorizationLimitAmt = (decimal)reader["AUTHORIZATION_LIMIT_AMT"],
                            AdditionalRequest = reader["ADDITIONAL_REQUEST"].ToString(),
                            IsRecommended = (bool)reader["IS_RECOMMENDED"],
                            RecommendationBySAM = reader["RECOMMENDATION_BY"].ToString(),
                            IsApproved = (bool)reader["IS_APPROVED"],
                            ApprovalBySAM = reader["APPROVAL_BY"].ToString(),
                            Remarks = reader["REMARKS"].ToString(),
                            BranchLocationId = reader["BRANCH_LOCATION_ID"].ToString(),
                            BranchName = reader["BRANCH_NAME"].ToString(),
                            DepartmentId = reader["DEPARTMENT_ID"].ToString(),
                            DepartmentName = reader["DEPARTMENT_NAME"].ToString(),
                            FunctionalTitle = reader["FUNCTIONAL_TITLE"].ToString(),
                            CorporateTitleId = reader["CORPORATE_TITLE_ID"].ToString(),
                            RequestStatus = reader["REQUEST_STATUS"].ToString(),
                            HRIS_ID = reader["HRIS_ID"].ToString(),
                            FLEX_ID = reader["FLEX_ID"].ToString(),
                            EMAIL_ID = reader["EMAIL_ID"].ToString(),
                            MOBILE_NUMBER = reader["MOBILE_NUMBER"].ToString(),
                            AccessGiver = reader["PROVIDED_BY"].ToString()
                        };
                        empSamAccountName = rrm.EmpSamAccountName.ToLower().Trim();
                        rrm.EmployeeFullName = lstEmp.Where(x =>
                  x.SamAccountName.ToLower().Trim() == empSamAccountName).Select(m => (m.EmployeeFirstName + " " + m.EmployeeLastName)).FirstOrDefault();
                        rrmList.Add(rrm);
                    }
                }
            }
            return rrmList;
        }
        public int GetRoleRequestRecordsCountForApproval(string employeeUserName)
        {
            int totalRequestCount = 0;
            string commandText = @"SELECT COUNT(*) REQUEST_COUNT FROM ROLE_REQUEST WHERE APPROVAL_BY='" + employeeUserName + "'";

            using (SqlDataReader reader = SqlHelper.ExecuteReader(connectionString, commandText,
                CommandType.Text))
            {
                while (reader.Read())
                {
                    if (reader.HasRows)
                    {
                        totalRequestCount = (int)reader["REQUEST_COUNT"];
                    }
                }
            }
            return totalRequestCount;
        }

        public List<ProvisionModel> GetRoleRequestRecordsForProvidingAccess(string accessGiver, List<EmployeeModel> lstEmp)
        {
            ProvisionModel rrm = null;
            string requesterUserNameTrimmed = string.Empty;
            List<ProvisionModel> rrmList = new List<ProvisionModel>();

            //            String commandText = @"SELECT * FROM ROLE_REQUEST RR INNER JOIN PROVISION P ON RR.RRF_ID = P.RRF_ID
            //                                    WHERE (PROVIDED_BY IS NULL OR PROVIDED_BY IN ('" + accessGiver + "')) AND RR.EMPLOYEE_USERNAME NOT IN ('" + accessGiver + "') ORDER BY CASE WHEN PROVISION_STATUS = 'IDLE' THEN 1 WHEN PROVISION_STATUS = 'INPR' THEN 2 ELSE 3 END ASC";
            //            String commandText = @"SELECT * FROM ROLE_REQUEST RR INNER JOIN PROVISION P ON RR.RRF_ID = P.RRF_ID
            //                                   ORDER BY CASE WHEN PROVISION_STATUS = 'IDLE' THEN 1 WHEN PROVISION_STATUS = 'INPR' THEN 2 ELSE 3 END ASC";
            string commandText = @"SELECT RR.RRF_ID, RR.EMPLOYEE_USERNAME, HRR2.REQUEST_DATE,B.BRANCH_NAME, D.DEPARTMENT_NAME, HRR.MODIFIED_DATE, P.PROVISION_ID, P.PROVIDED_BY, P.PROVISION_STATUS, P.REMARKS FROM ROLE_REQUEST RR WITH (NOLOCK)
                                    INNER JOIN PROVISION P WITH (NOLOCK) ON RR.RRF_ID = P.RRF_ID
                                    INNER JOIN BRANCH B WITH (NOLOCK) ON RR.BRANCH_LOCATION_ID = B.BRANCH_ID
									INNER JOIN DEPARTMENT D WITH (NOLOCK) ON RR.DEPARTMENT_ID = D.DEPARTMENT_ID
                                    LEFT OUTER JOIN (SELECT REF_RRF_ID, Max(MODIFIED_DATE) AS MODIFIED_DATE
                                    FROM HIST_ROLE_REQUEST WITH (NOLOCK) 
                                    GROUP BY REF_RRF_ID) HRR
                                    ON RR.RRF_ID = HRR.REF_RRF_ID
                                    LEFT OUTER JOIN (SELECT REF_RRF_ID, MIN(REQUEST_DATE) AS REQUEST_DATE
                                    FROM HIST_ROLE_REQUEST WITH (NOLOCK) 
                                    GROUP BY REF_RRF_ID) HRR2
                                    ON RR.RRF_ID = HRR2.REF_RRF_ID
                                    ORDER BY CASE WHEN PROVISION_STATUS = 'IDLE' THEN 1 WHEN PROVISION_STATUS = 'INPR' THEN 2 ELSE 3 END ASC, MODIFIED_DATE ASC";

            using (SqlDataReader reader = SqlHelper.ExecuteReader(connectionString, commandText,
                CommandType.Text))
            {
                while (reader.Read())
                {
                    if (reader.HasRows)
                    {
                        rrm = new ProvisionModel()
                        {
                            RoleRequestId = (int)reader["RRF_ID"],
                            RequestedByUsername = reader["EMPLOYEE_USERNAME"].ToString(),
                            ProvisionId = (int)reader["PROVISION_ID"],
                            ProvidedBy = reader["PROVIDED_BY"].ToString(),
                            ProvisionStatus = reader["PROVISION_STATUS"].ToString(),
                            BranchName = (reader["BRANCH_NAME"].ToString() + "-" + reader["DEPARTMENT_NAME"].ToString()),
                            ApprovedDate = reader["MODIFIED_DATE"].ToString(),
                            InitiatedDate = reader["REQUEST_DATE"].ToString(),
                        };
                        requesterUserNameTrimmed = rrm.RequestedByUsername.ToLower().Trim();
                        //if (rrm.RequestedByUsername == "krisna.bista")
                        //{

                        //}
                        rrm.RequestedByFullName = lstEmp.Where(x => x.SamAccountName.ToLower().Trim() == requesterUserNameTrimmed).Select(m => (m.EmployeeFirstName + " " + m.EmployeeLastName)).FirstOrDefault();
                        rrmList.Add(rrm);
                    }
                }
            }
            return rrmList;
        }
        public int GetRoleRequestRecordsCountForProvidingAccess()
        {
            int totalRequestCount = 0;

            string commandText = @"SELECT COUNT(*) REQUEST_COUNT FROM ROLE_REQUEST RR WITH (NOLOCK)
                                    INNER JOIN PROVISION P WITH (NOLOCK) ON RR.RRF_ID = P.RRF_ID
                                    INNER JOIN BRANCH B WITH (NOLOCK) ON RR.BRANCH_LOCATION_ID = B.BRANCH_ID
									INNER JOIN DEPARTMENT D WITH (NOLOCK) ON RR.DEPARTMENT_ID = D.DEPARTMENT_ID
                                    LEFT OUTER JOIN (SELECT REF_RRF_ID, Max(MODIFIED_DATE) AS MODIFIED_DATE
                                    FROM HIST_ROLE_REQUEST WITH (NOLOCK) 
                                    GROUP BY REF_RRF_ID) HRR
                                    ON RR.RRF_ID = HRR.REF_RRF_ID
                                    LEFT OUTER JOIN (SELECT REF_RRF_ID, MIN(REQUEST_DATE) AS REQUEST_DATE
                                    FROM HIST_ROLE_REQUEST WITH (NOLOCK) 
                                    GROUP BY REF_RRF_ID) HRR2
                                    ON RR.RRF_ID = HRR2.REF_RRF_ID";

            using (SqlDataReader reader = SqlHelper.ExecuteReader(connectionString, commandText,
                CommandType.Text))
            {
                while (reader.Read())
                {
                    if (reader.HasRows)
                    {
                        totalRequestCount = (int)reader["REQUEST_COUNT"];
                    }
                }
            }
            return totalRequestCount;
        }

        public int EnrolledUsersCountBySupervisorADName(string supervisorADName)
        {
            int totalRequestCount = 0;

            string commandText = @"SELECT COUNT(*) REQUEST_COUNT FROM DBO.USER_ENROLL WHERE SUPERVISION_BY='" + supervisorADName + "'";

            using (SqlDataReader reader = SqlHelper.ExecuteReader(connectionString, commandText,
                CommandType.Text))
            {
                while (reader.Read())
                {
                    if (reader.HasRows)
                    {
                        totalRequestCount = (int)reader["REQUEST_COUNT"];
                    }
                }
            }
            return totalRequestCount;
        }

        public int HRApprovalUsersCount(string hrAdminADName)
        {
            int totalRequestCount = 0;

            string commandText = @"SELECT COUNT(*) REQUEST_COUNT FROM DBO.USER_ENROLL WHERE STATUS='INIT' OR ASSIGNED_HR_ADMIN='" + hrAdminADName + "'";

            using (SqlDataReader reader = SqlHelper.ExecuteReader(connectionString, commandText,
                CommandType.Text))
            {
                while (reader.Read())
                {
                    if (reader.HasRows)
                    {
                        totalRequestCount = (int)reader["REQUEST_COUNT"];
                    }
                }
            }
            return totalRequestCount;
        }

        public int GetUsersCountForADAndEmail(string idmAdminADName)
        {
            int totalRequestCount = 0;
            string commandText = @"SELECT COUNT(*) REQUEST_COUNT FROM DBO.USER_ENROLL WHERE STATUS IN ('FFAE', 'DONE') OR ASSIGNED_IDM_ADMIN='" + idmAdminADName + "'";

            using (SqlDataReader reader = SqlHelper.ExecuteReader(connectionString, commandText,
                CommandType.Text))
            {
                while (reader.Read())
                {
                    if (reader.HasRows)
                    {
                        totalRequestCount = (int)reader["REQUEST_COUNT"];
                    }
                }
            }
            return totalRequestCount;
        }

        public int GetEmployeeCountListedForRevocation()
        {
            List<RevokeEmployeeModel> employeeList = new List<RevokeEmployeeModel>();
            List<EmployeeModel> allEmployeesFromAD = new List<EmployeeModel>();
            if (HttpContext.Current.Cache["Employees"] != null)
            {
                allEmployeesFromAD = (List<EmployeeModel>)HttpContext.Current.Cache["Employees"];
            }

            string commandText = @"WITH CTE AS 
                                    (
                                    SELECT DISTINCT RR.EMPLOYEE_USERNAME, RRM.REQUEST_STATUS, RR.HRIS_ID, RR.EMAIL_ID, CASE WHEN (BR.branch_code IS NOT NULL AND BR.location_code IS NOT NULL) THEN
                                    (CONCAT(BR.BRANCH_NAME, ' / ' ,BR.branch_code,'-',BR.location_code)) 
                                    WHEN (BR.branch_code IS NULL AND BR.location_code IS NULL) THEN BR.BRANCH_NAME
                                    WHEN  (BR.branch_code IS NOT NULL AND BR.location_code IS NULL) THEN (CONCAT(BR.BRANCH_NAME, ' / ' ,BR.branch_code)) 
                                    WHEN (BR.branch_code IS NULL AND BR.location_code IS NOT NULL) THEN (CONCAT(BR.BRANCH_NAME, ' - ' ,BR.location_code)) 
                                    END BRANCH_NAME FROM ROLE_REQUEST RR
                                    LEFT OUTER JOIN REVOKE_REQUEST_MAIN RRM ON RR.EMPLOYEE_USERNAME = RRM.REVOKE_EMP_USERNAME
                                    LEFT OUTER JOIN BRANCH BR ON RR.BRANCH_LOCATION_ID = BR.BRANCH_ID
                                    WHERE RR.REQUEST_STATUS = 'ASGN'
                                    )
                                    SELECT * FROM CTE ORDER BY CASE WHEN REQUEST_STATUS IS NULL THEN 1 WHEN REQUEST_STATUS = 'INIT' THEN 2 ELSE 3 END ASC";


            using (SqlDataReader reader = SqlHelper.ExecuteReader(connectionString, commandText,
                CommandType.Text))
            {
                while (reader.Read())
                {
                    if (reader.HasRows)
                    {
                        RevokeEmployeeModel em = new RevokeEmployeeModel()
                        {
                            SamAccountName = reader["EMPLOYEE_USERNAME"].ConvertToString(),
                            RequestStatus = reader["REQUEST_STATUS"].ConvertToString(),
                            BranchName = reader["BRANCH_NAME"].ConvertToString(),
                            HRISId = reader["HRIS_ID"].ConvertToString(),
                            EmailId = reader["EMAIL_ID"].ConvertToString()
                        };
                        dynamic matchedEmployee = allEmployeesFromAD.Where(x => x.SamAccountName == em.SamAccountName).FirstOrDefault();
                        if (matchedEmployee != null)
                        {
                            em.EmployeeFullName = string.Concat(matchedEmployee.EmployeeFirstName, " ", matchedEmployee.EmployeeLastName);
                            employeeList.Add(em);
                        }
                    }
                }
            }
            return employeeList.Count;
        }

        public int GetRevokeListAvailableCountForRevocation(string idmUserName)
        {
            int totalRequestCount = 0;

            string commandText = @"SELECT COUNT(*) REQUEST_COUNT FROM REVOKE_REQUEST_MAIN WHERE REVOKE_EMP_USERNAME = '" + idmUserName + "'";

            using (SqlDataReader reader = SqlHelper.ExecuteReader(connectionString, commandText,
                CommandType.Text))
            {
                while (reader.Read())
                {
                    if (reader.HasRows)
                    {
                        totalRequestCount = (int)reader["REQUEST_COUNT"];
                    }
                }
            }
            return totalRequestCount;
        }

        public List<RemarksReport> GetAllRemarksInThisRequest(int roleRequestId)
        {
            List<RemarksReport> remarksList = new List<RemarksReport>();
            String commandText = "SP_GET_REMARKS_IN_THIS_REQUEST";
            SqlParameter parameterReqId = new SqlParameter("@RRF_ID", SqlDbType.Int);
            parameterReqId.Value = roleRequestId;
            using (SqlDataReader reader = SqlHelper.ExecuteReader(connectionString, commandText,
                CommandType.StoredProcedure, parameterReqId))
            {
                while (reader.Read())
                {
                    if (reader.HasRows)
                    {
                        var action = String.Empty;
                        switch (reader["REQUEST_STATUS"].ToString())
                        {
                            case "INIT":
                                action = "INITIATED";
                                break;
                            case "CORR":
                                action = "RETURNED";
                                break;
                            case "RECC":
                                action = "RECOMMENDED";
                                break;
                            case "CORA":
                                action = "RETURNED";
                                break;
                            case "UPDT":
                                action = "UPDATED";
                                break;
                            case "TBCK":
                                action = "TAKEN BACK";
                                break;
                            case "APPR":
                                action = "APPROVED";
                                break;
                            case "CORI":
                                action = "RETURNED";
                                break;
                            default:
                                action = "COMPLETED";
                                break;
                        }
                        var remarksByFullName = String.Empty;
                        RemarksReport rr = new RemarksReport()
                        {
                            RequestId = (int)reader["REF_RRF_ID"],
                            AdditionalRequest = reader["ADDITIONAL_REQUEST"].ToString(),
                            RemarksBy = reader["MODIFIED_BY"].ToString(),
                            AssignDate = reader["REQUEST_DATE"].ToString(),
                            CompleteDate = reader["MODIFIED_DATE"].ToString(),
                            Remarks = reader["REMARKS"].ToString(),
                            Action = action
                        };
                        remarksList.Add(rr);
                    }
                }
            }
            return remarksList;
        }

        //[OutputCache(Duration = int.MaxValue)]
        public List<SelectListItem> GetDepartmentsForDDL()
        {
            List<SelectListItem> departments = new List<SelectListItem>();
            SelectListItem sl = null;
            string commandText = "SELECT * FROM DEPARTMENT ORDER BY DEPARTMENT_NAME ASC";
            using (SqlDataReader reader = SqlHelper.ExecuteReader(connectionString, commandText,
                CommandType.Text))
            {
                while (reader.Read())
                {
                    if (reader.HasRows)
                    {
                        sl = new SelectListItem()
                        {
                            Value = reader["DEPARTMENT_ID"].ToString(),
                            Text = reader["DEPARTMENT_NAME"].ToString(),
                        };
                        departments.Add(sl);
                    }
                }
            }
            return departments;
        }

        //[OutputCache(Duration = int.MaxValue)]
        public List<SelectListItem> GetBranchLocationsForDDL()
        {
            List<SelectListItem> branches = new List<SelectListItem>();
            SelectListItem sl = null;
            string commandText = @"select
                                    CASE WHEN (branch_code IS NOT NULL AND location_code IS NOT NULL) THEN
                                    (CONCAT(BRANCH_NAME, ' / ' ,branch_code,'-',location_code)) 
                                    WHEN (branch_code IS NULL AND location_code IS NULL) THEN BRANCH_NAME
                                    WHEN  (branch_code IS NOT NULL AND location_code IS NULL) THEN (CONCAT(BRANCH_NAME, ' / ' ,branch_code)) 
                                    WHEN (branch_code IS NULL AND location_code IS NOT NULL) THEN (CONCAT(BRANCH_NAME, ' - ' ,location_code)) 
                                    END BRANCH_NAME,
                                    BRANCH_ID
                                    from branch ORDER BY BRANCH_NAME ASC";
            using (SqlDataReader reader = SqlHelper.ExecuteReader(connectionString, commandText,
                CommandType.Text))
            {
                while (reader.Read())
                {
                    if (reader.HasRows)
                    {
                        sl = new SelectListItem()
                        {
                            Value = reader["BRANCH_ID"].ToString(),
                            Text = reader["BRANCH_NAME"].ToString(),
                        };
                        branches.Add(sl);
                    }
                }
            }
            return branches;
        }

        //[OutputCache(Duration = int.MaxValue)]
        public List<SelectListItem> GetFunctionalTitlesForDDL()
        {
            List<SelectListItem> functionalTitles = new List<SelectListItem>();
            SelectListItem sl = null;
            string commandText = "SELECT * FROM FUNCTIONAL_TITLE";
            using (SqlDataReader reader = SqlHelper.ExecuteReader(connectionString, commandText,
                CommandType.Text))
            {
                while (reader.Read())
                {
                    if (reader.HasRows)
                    {
                        sl = new SelectListItem()
                        {
                            Value = reader["TITLE_ID"].ToString(),
                            Text = reader["TITLE_NAME"].ToString(),
                        };
                        functionalTitles.Add(sl);
                    }
                }
            }
            return functionalTitles;
        }

        //[OutputCache(Duration = int.MaxValue)]
        public List<SelectListItem> GetCorporateTitlesForDDL()
        {
            List<SelectListItem> corporateTitles = new List<SelectListItem>();
            SelectListItem sl = null;
            string commandText = "SELECT * FROM CORPORATE_TITLE";
            using (SqlDataReader reader = SqlHelper.ExecuteReader(connectionString, commandText,
                CommandType.Text))
            {
                while (reader.Read())
                {
                    if (reader.HasRows)
                    {
                        sl = new SelectListItem()
                        {
                            Value = reader["TITLE_ID"].ToString(),
                            Text = reader["TITLE_NAME"].ToString(),
                        };
                        corporateTitles.Add(sl);
                    }
                }
            }
            return corporateTitles;
        }
    }
}