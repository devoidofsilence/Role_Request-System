using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using RoleRequest.Models;
using System.Data.SqlClient;
using RoleRequest.Helpers;
using System.Data;
using System.Net.Mail;
using System.Text;
using System.IO;
using RoleRequest.Models.Utils;

namespace RoleRequest.Services
{
    public class IDManagementService
    {
        String connectionString = String.Empty;
        public IDManagementService()
        {
            connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        }

        public int SaveEnrollUser(IDMUserModel eum)
        {
            SqlConnection db = new SqlConnection(connectionString);
            SqlCommand com = new SqlCommand();
            SqlTransaction tran;
            db.Open();
            tran = db.BeginTransaction();
            int i = 0;
            try
            {
                //Run all your insert statements here here
                //Role Request Table Insertion
                com.CommandType = CommandType.StoredProcedure;
                com.CommandText = "IUD_SP_USER_ENROLL";
                com.Connection = db;
                com.Transaction = tran;
                com.Parameters.AddWithValue("@USER_ID", eum.EnrollUserID);
                com.Parameters.AddWithValue("@EMAIL", eum.Email);
                com.Parameters.AddWithValue("@EMAIL_PWD", eum.EmailIDPwd);
                com.Parameters.AddWithValue("@AD_USERNAME", eum.ADUsername);
                com.Parameters.AddWithValue("@AD_USERNAME_PWD", eum.ADUsernamePwd);
                com.Parameters.AddWithValue("@ASSIGNED_IDM_ADMIN", eum.AssignedIDMAdmin);
                com.Parameters.AddWithValue("@STATUS", "DONE");
                i = com.ExecuteNonQuery();
                tran.Commit();

                //send email to the supervisor here
                EmailToSupervisor ets = new EmailToSupervisor();
                using (MailMessage msg = new MailMessage())
                {
                    try
                    {
                        msg.From = new MailAddress(ConfigurationManager.AppSettings["emailFromIDM"]);
                        //msg.From = new MailAddress(_emailFrom);
                        msg.Subject = String.Format(ets.Subject, eum.FullName);
                        msg.Body = String.Format(ets.Body(), eum.FullName, eum.EmployeeID, eum.EmployeeIDPwd, eum.Email, eum.EmailIDPwd, eum.ADUsername, eum.ADUsernamePwd);
                        msg.IsBodyHtml = true;
                        msg.To.Add(new MailAddress(eum.SupervisorEmailID));

                        ///
                        /// Modified for Multiple CC
                        ///


                        //if (!string.IsNullOrEmpty(em.CC))
                        //{
                        //    foreach (var address in em.CC.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
                        //    {
                        //        string res = CheckEmail(address);
                        //        if (!string.IsNullOrEmpty(res))
                        //            msg.CC.Add(res);
                        //    }
                        //}


                        ///END OF MODIFICATION CC


                        SmtpClient smtp = new SmtpClient();
                        smtp.Host = ConfigurationManager.AppSettings["Host"];
                        smtp.EnableSsl = Convert.ToBoolean(ConfigurationManager.AppSettings["EnableSsl"]);
                        System.Net.NetworkCredential NetworkCred = new System.Net.NetworkCredential();
                        NetworkCred.UserName = ConfigurationManager.AppSettings["emailFromIDM"];
                        NetworkCred.Password = ConfigurationManager.AppSettings["PasswordIDM"];
                        smtp.UseDefaultCredentials = false;
                        smtp.Credentials = NetworkCred;
                        smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                        smtp.Port = int.Parse(ConfigurationManager.AppSettings["Port"]);
                        smtp.Timeout = 20000;

                        smtp.Send(msg);
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback();
                    }
                }
            }
            catch (SqlException sqlex)
            {
                tran.Rollback();
            }
            finally
            {
                db.Close();
            }
            return i;
        }

        public List<EnrollUserModel> GetUsersForADAndEmail(string idmAdminADName)
        {
            List<EnrollUserModel> enrollUsersList = new List<EnrollUserModel>();

            //String commandText = @"SELECT UE.*, CT.TITLE_NAME CORP_TITLE_NAME, DEP.DEPARTMENT_NAME DEPARTMENT_NAME, BRN.BRANCH_NAME BRANCH_NAME FROM DBO.USER_ENROLL UE
            //                        INNER JOIN DBO.DEPARTMENT DEP ON UE.DEPARTMENT_ID = DEP.DEPARTMENT_ID
            //                        INNER JOIN DBO.BRANCH BRN ON UE.BRANCH_LOCATION_ID = BRN.BRANCH_ID
            //                        INNER JOIN DBO.CORPORATE_TITLE CT ON UE.CORPORATE_TITLE_ID = CT.TITLE_ID
            //                        WHERE UE.STATUS='FFAE' OR UE.ASSIGNED_IDM_ADMIN='" + idmAdminADName + "' ORDER BY CASE WHEN UE.STATUS = 'INIT' THEN 1 WHEN UE.STATUS = 'FFAE' THEN 2 ELSE 3 END ASC";
            String commandText = @"SELECT UE.*, CT.TITLE_NAME CORP_TITLE_NAME, DEP.DEPARTMENT_NAME DEPARTMENT_NAME, BRN.BRANCH_NAME BRANCH_NAME FROM DBO.USER_ENROLL UE
                                    INNER JOIN DBO.DEPARTMENT DEP ON UE.DEPARTMENT_ID = DEP.DEPARTMENT_ID
                                    INNER JOIN DBO.BRANCH BRN ON UE.BRANCH_LOCATION_ID = BRN.BRANCH_ID
                                    INNER JOIN DBO.CORPORATE_TITLE CT ON UE.CORPORATE_TITLE_ID = CT.TITLE_ID
                                    WHERE UE.STATUS IN ('FFAE', 'DONE') OR UE.ASSIGNED_IDM_ADMIN='" + idmAdminADName + "' ORDER BY CASE WHEN UE.STATUS = 'INIT' THEN 1 WHEN UE.STATUS = 'FFAE' THEN 2 ELSE 3 END ASC";


            using (SqlDataReader reader = SqlHelper.ExecuteReader(connectionString, commandText,
                CommandType.Text))
            {
                while (reader.Read())
                {
                    if (reader.HasRows)
                    {
                        EnrollUserModel eum = new EnrollUserModel()
                        {
                            EnrollUserID = (int)reader["USER_ID"],
                            FullName = reader["FULL_NAME"].ToString(),
                            DepartmentId = (int)reader["DEPARTMENT_ID"],
                            DepartmentName = reader["DEPARTMENT_NAME"].ToString(),
                            BranchLocationId = (int)reader["BRANCH_LOCATION_ID"],
                            BranchLocationName = reader["BRANCH_NAME"].ToString(),
                            CorpTitleId = (int)reader["CORPORATE_TITLE_ID"],
                            CorpTitleName = reader["CORP_TITLE_NAME"].ToString(),
                            FunctionalTitle = reader["FUNCTIONAL_TITLE"].ToString(),
                            MobileNumber = reader["MOBILE_NUMBER"].ToString(),
                            JoinDate = reader["JOIN_DATE"].ToString(),
                            Status = reader["STATUS"].ToString(),
                            SupervisionBy = reader["SUPERVISION_BY"].ToString(),
                            EmployeeID = reader["EMPLOYEE_ID"].ToString()
                        };
                        enrollUsersList.Add(eum);
                    }
                }
            }
            return enrollUsersList;
        }

        public IDMUserModel EnrolledUserDetailByEnrollUserID(int enrollUserID)
        {
            IDMUserModel enrollUser = new IDMUserModel();

            String commandText = @"SELECT UE.*, CT.TITLE_NAME CORP_TITLE_NAME, DEP.DEPARTMENT_NAME DEPARTMENT_NAME, BRN.BRANCH_NAME BRANCH_NAME FROM DBO.USER_ENROLL UE
                                    INNER JOIN DBO.DEPARTMENT DEP ON UE.DEPARTMENT_ID = DEP.DEPARTMENT_ID
                                    INNER JOIN DBO.BRANCH BRN ON UE.BRANCH_LOCATION_ID = BRN.BRANCH_ID
                                    INNER JOIN DBO.CORPORATE_TITLE CT ON UE.CORPORATE_TITLE_ID = CT.TITLE_ID
                                    WHERE UE.USER_ID='" + enrollUserID + "'";

            using (SqlDataReader reader = SqlHelper.ExecuteReader(connectionString, commandText,
                CommandType.Text))
            {
                while (reader.Read())
                {
                    if (reader.HasRows)
                    {
                        enrollUser = new IDMUserModel()
                        {
                            EnrollUserID = (int)reader["USER_ID"],
                            FullName = reader["FULL_NAME"].ToString(),
                            DepartmentId = (int)reader["DEPARTMENT_ID"],
                            DepartmentName = reader["DEPARTMENT_NAME"].ToString(),
                            BranchLocationId = (int)reader["BRANCH_LOCATION_ID"],
                            BranchLocationName = reader["BRANCH_NAME"].ToString(),
                            CorpTitleId = (int)reader["CORPORATE_TITLE_ID"],
                            CorpTitleName = reader["CORP_TITLE_NAME"].ToString(),
                            FunctionalTitle = reader["FUNCTIONAL_TITLE"].ToString(),
                            MobileNumber = reader["MOBILE_NUMBER"].ToString(),
                            JoinDate = reader["JOIN_DATE"].ConvertToString() != "" ? Convert.ToDateTime(reader["JOIN_DATE"]).ToString("dd/MM/yyyy") : "",
                            Status = reader["STATUS"].ToString(),
                            SupervisionBy = reader["SUPERVISION_BY"].ToString(),
                            EmployeeID = reader["EMPLOYEE_ID"].ToString(),
                            EmployeeIDPwd = reader["EMPLOYEE_ID_PWD"].ToString()
                        };
                    }
                }
            }
            return enrollUser;
        }

        public bool CheckDuplicateEmailID(string emailID)
        {
            bool hasDuplicateEmailID = false;
            String commandText = @"SELECT * FROM USER_ENROLL WHERE EMAIL = '" + emailID.Trim() + "'";

            using (SqlDataReader reader = SqlHelper.ExecuteReader(connectionString, commandText,
                CommandType.Text))
            {
                while (reader.Read())
                {
                    if (reader.HasRows)
                    {
                        hasDuplicateEmailID = true;
                    }
                }
            }
            return hasDuplicateEmailID;
        }

        public bool CheckDuplicateADName(string adUsername)
        {
            bool hasDuplicateADUsername = false;
            String commandText = @"SELECT * FROM USER_ENROLL WHERE AD_USERNAME = '" + adUsername.Trim() + "'";

            using (SqlDataReader reader = SqlHelper.ExecuteReader(connectionString, commandText,
                CommandType.Text))
            {
                while (reader.Read())
                {
                    if (reader.HasRows)
                    {
                        hasDuplicateADUsername = true;
                    }
                }
            }
            return hasDuplicateADUsername;
        }

        public List<RevokeRequestModel> GetRevokeListAvailableForRevocation(string idmUserName)
        {
            List<RevokeRequestModel> revokeList = new List<RevokeRequestModel>();
            List<EmployeeModel> allEmployeesFromAD = new List<EmployeeModel>();
            if (HttpContext.Current.Cache["Employees"] != null)
            {
                allEmployeesFromAD = (List<EmployeeModel>)HttpContext.Current.Cache["Employees"];
            }

            //string commandText = @"SELECT * FROM REVOKE_REQUEST_MAIN WHERE TAKEN_BY IS NULL OR UPPER(RTRIM(LTRIM(TAKEN_BY))) = UPPER(RTRIM(LTRIM('" + idmUserName + "'))) ORDER BY CASE WHEN REQUEST_STATUS='INIT' THEN 1 ELSE 2 END ASC";
            string commandText = @"SELECT * FROM REVOKE_REQUEST_MAIN ORDER BY CASE WHEN REQUEST_STATUS='INIT' THEN 1 ELSE 2 END ASC";


            using (SqlDataReader reader = SqlHelper.ExecuteReader(connectionString, commandText,
                CommandType.Text))
            {
                while (reader.Read())
                {
                    if (reader.HasRows)
                    {
                        RevokeRequestModel rrm = new RevokeRequestModel()
                        {
                            RevokeId = (int)reader["REVOKE_ID"],
                            RevokeEmpUsername = reader["REVOKE_EMP_USERNAME"].ConvertToString(),
                            RequestStatus = reader["REQUEST_STATUS"].ConvertToString(),
                            RequestedBy = reader["REQUESTED_BY"].ConvertToString(),
                            TakenBy = reader["TAKEN_BY"].ConvertToString(),
                            ResignationDate = Convert.ToDateTime(reader["RESIGNATION_DATE"]),
                            RevokeDate = Convert.ToDateTime(reader["REVOKE_REQUEST_DATE"])
                        };
                        dynamic matchedEmployee = allEmployeesFromAD.Where(x => x.SamAccountName == rrm.RevokeEmpUsername).FirstOrDefault();
                        if (matchedEmployee != null)
                        {
                            rrm.RevokeEmpFullName = string.Concat(matchedEmployee.EmployeeFirstName, " ", matchedEmployee.EmployeeLastName);
                            revokeList.Add(rrm);
                        }
                    }
                }
            }
            return revokeList;
        }

        public RevokeRequestModel GetRevocationDtlFromRevokeId(int revokeId)
        {
            RevokeRequestModel rrm = new RevokeRequestModel();
            List<EmployeeModel> allEmployeesFromAD = new List<EmployeeModel>();
            if (HttpContext.Current.Cache["Employees"] != null)
            {
                allEmployeesFromAD = (List<EmployeeModel>)HttpContext.Current.Cache["Employees"];
            }

            String commandText = @"SELECT * FROM REVOKE_REQUEST_MAIN WHERE REVOKE_ID = '" + revokeId + "'";


            using (SqlDataReader reader = SqlHelper.ExecuteReader(connectionString, commandText,
                CommandType.Text))
            {
                while (reader.Read())
                {
                    if (reader.HasRows)
                    {
                        rrm = new RevokeRequestModel()
                        {
                            RevokeId = (int)reader["REVOKE_ID"],
                            RevokeEmpUsername = reader["REVOKE_EMP_USERNAME"].ConvertToString(),
                            RequestStatus = reader["REQUEST_STATUS"].ConvertToString(),
                            RequestedBy = reader["REQUESTED_BY"].ConvertToString(),
                            TakenBy = reader["TAKEN_BY"].ConvertToString(),
                            Remarks = reader["REMARKS"].ConvertToString(),
                            ResignationDate = Convert.ToDateTime(reader["RESIGNATION_DATE"]),
                            RevokeDate = Convert.ToDateTime(reader["REVOKE_REQUEST_DATE"])
                        };
                        dynamic matchedEmployee = allEmployeesFromAD.Where(x => x.SamAccountName == rrm.RevokeEmpUsername).FirstOrDefault();
                        if (matchedEmployee != null)
                        {
                            rrm.RevokeEmpFullName = string.Concat(matchedEmployee.EmployeeFirstName, " ", matchedEmployee.EmployeeLastName);
                        }
                    }
                }
            }
            return rrm;
        }

        public List<AppAccessRevocationDtlModel> GetAccessRevocationDtlFromRevokeId(int revokeId)
        {
            List<AppAccessRevocationDtlModel> appAccessList = new List<AppAccessRevocationDtlModel>();

            String commandText = @"SELECT A.APP_ID, A.APP_NAME, AARD.REVOKE_ID, AARD.STATUS FROM APP_ACCESS_REVOCATION_DTL AARD INNER JOIN APP A ON AARD.APP_ID = A.APP_ID WHERE AARD.REVOKE_ID = '" + revokeId + "'";


            using (SqlDataReader reader = SqlHelper.ExecuteReader(connectionString, commandText,
                CommandType.Text))
            {
                while (reader.Read())
                {
                    if (reader.HasRows)
                    {
                        AppAccessRevocationDtlModel aardm = new AppAccessRevocationDtlModel()
                        {
                            RevokeId = (int)reader["REVOKE_ID"],
                            AppId = (int)reader["APP_ID"],
                            Status = reader["STATUS"].ConvertToString(),
                            AppName = reader["APP_NAME"].ConvertToString()
                        };
                        appAccessList.Add(aardm);
                    }
                }
            }
            return appAccessList;
        }

        public List<PrimRoleRevocationDtlModel> GetPrimaryRoleRevocationDtlFromRevokeId(int revokeId)
        {
            List<PrimRoleRevocationDtlModel> primRoleList = new List<PrimRoleRevocationDtlModel>();

            String commandText = @"SELECT * FROM PRIM_ROLE_REVOCATION_DTL PRRD INNER JOIN PRIMARY_ROLE PR ON PRRD.PRIM_ROLE_ID = PR.PRIMARY_ROLE_ID WHERE PRRD.REVOKE_ID = '" + revokeId + "'";


            using (SqlDataReader reader = SqlHelper.ExecuteReader(connectionString, commandText,
                CommandType.Text))
            {
                while (reader.Read())
                {
                    if (reader.HasRows)
                    {
                        PrimRoleRevocationDtlModel prrdm = new PrimRoleRevocationDtlModel()
                        {
                            RevokeId = (int)reader["REVOKE_ID"],
                            PrimRoleId = (int)reader["PRIM_ROLE_ID"],
                            Status = reader["STATUS"].ConvertToString(),
                            PrimaryRoleName = reader["ROLE_NAME"].ConvertToString(),
                        };
                        primRoleList.Add(prrdm);
                    }
                }
            }
            return primRoleList;
        }

        public int GetLatestRoleRequestIdForEmpUsername(string empUsername)
        {
            int rrfId = 0;
            String commandText = @"SELECT MAX(RRF_ID) RRFID FROM ROLE_REQUEST WHERE UPPER(RTRIM(LTRIM(EMPLOYEE_USERNAME))) = UPPER(RTRIM(LTRIM('" + empUsername + "')))";

            using (SqlDataReader reader = SqlHelper.ExecuteReader(connectionString, commandText,
                CommandType.Text))
            {
                while (reader.Read())
                {
                    if (reader.HasRows)
                    {
                        rrfId = (int)reader["RRFID"];
                    }
                }
            }
            return rrfId;
        }

        public bool CheckIfRevokeIsTaken(int revokeId)
        {
            bool revokeIsTaken = false;
            String commandText = @"SELECT TAKEN_BY FROM REVOKE_REQUEST_MAIN WHERE REVOKE_ID='" + revokeId + "'";

            using (SqlDataReader reader = SqlHelper.ExecuteReader(connectionString, commandText,
                CommandType.Text))
            {
                while (reader.Read())
                {
                    if (reader.HasRows)
                    {
                        if (!string.IsNullOrEmpty(reader["TAKEN_BY"].ConvertToString()))
                        {
                            revokeIsTaken = true;
                        }
                    }
                }
            }
            return revokeIsTaken;
        }

        public int SetTakenBy(int revokeId, string takenBy)
        {
            SqlConnection db = new SqlConnection(connectionString);
            SqlCommand com = new SqlCommand();
            SqlTransaction tran;
            db.Open();
            tran = db.BeginTransaction();
            int i = 0;
            try
            {
                //Run all your insert statements here here
                //Role Request Table Insertion
                com.CommandType = CommandType.Text;
                com.CommandText = "UPDATE REVOKE_REQUEST_MAIN SET TAKEN_BY=@TAKEN_BY WHERE REVOKE_ID=@REVOKE_ID";
                com.Connection = db;
                com.Transaction = tran;
                com.Parameters.AddWithValue("@REVOKE_ID", revokeId);
                com.Parameters.AddWithValue("@TAKEN_BY", takenBy);
                i = com.ExecuteNonQuery();

                tran.Commit();
            }
            catch (SqlException sqlex)
            {
                tran.Rollback();
            }
            finally
            {
                db.Close();
            }
            return i;
        }

        public int UpdateRevokeRequestAppAccess(int revokeId, int appId)
        {
            SqlConnection db = new SqlConnection(connectionString);
            SqlCommand com = new SqlCommand();
            SqlTransaction tran;
            db.Open();
            tran = db.BeginTransaction();
            int i = 0;
            try
            {
                //Run all your insert statements here here
                //Role Request Table Insertion
                com.CommandType = CommandType.Text;
                com.CommandText = "UPDATE APP_ACCESS_REVOCATION_DTL SET STATUS='REV' WHERE APP_ID=@APP_ID AND REVOKE_ID=@REVOKE_ID";
                com.Connection = db;
                com.Transaction = tran;
                com.Parameters.AddWithValue("@REVOKE_ID", revokeId);
                com.Parameters.AddWithValue("@APP_ID", appId);
                i = com.ExecuteNonQuery();

                tran.Commit();
            }
            catch (SqlException sqlex)
            {
                tran.Rollback();
            }
            finally
            {
                db.Close();
            }
            return i;
        }

        public int UpdateRevokeRequestPrimRole(int revokeId, int primRoleId)
        {
            SqlConnection db = new SqlConnection(connectionString);
            SqlCommand com = new SqlCommand();
            SqlTransaction tran;
            db.Open();
            tran = db.BeginTransaction();
            int i = 0;
            try
            {
                //Run all your insert statements here here
                //Role Request Table Insertion
                com.CommandType = CommandType.Text;
                com.CommandText = "UPDATE PRIM_ROLE_REVOCATION_DTL SET STATUS='REV' WHERE PRIM_ROLE_ID=@PRIM_ROLE_ID AND REVOKE_ID=@REVOKE_ID";
                com.Connection = db;
                com.Transaction = tran;
                com.Parameters.AddWithValue("@REVOKE_ID", revokeId);
                com.Parameters.AddWithValue("@PRIM_ROLE_ID", primRoleId);
                i = com.ExecuteNonQuery();

                tran.Commit();
            }
            catch (SqlException sqlex)
            {
                tran.Rollback();
            }
            finally
            {
                db.Close();
            }
            return i;
        }

        public bool CheckIfAllAppAccessesWereRevoked(int revokeId)
        {
            bool allRevoked = true;
            String commandText = @"SELECT * FROM APP_ACCESS_REVOCATION_DTL WHERE REVOKE_ID='" + revokeId + "' AND STATUS IS NULL";

            using (SqlDataReader reader = SqlHelper.ExecuteReader(connectionString, commandText,
                CommandType.Text))
            {
                while (reader.Read())
                {
                    if (reader.HasRows)
                    {
                        allRevoked = false;
                    }
                }
            }
            return allRevoked;
        }

        public bool CheckIfAllPrimRolesWereRevoked(int revokeId)
        {
            bool allRevoked = true;
            String commandText = @"SELECT * FROM PRIM_ROLE_REVOCATION_DTL WHERE REVOKE_ID='" + revokeId + "' AND STATUS IS NULL";

            using (SqlDataReader reader = SqlHelper.ExecuteReader(connectionString, commandText,
                CommandType.Text))
            {
                while (reader.Read())
                {
                    if (reader.HasRows)
                    {
                        allRevoked = false;
                    }
                }
            }
            return allRevoked;
        }

        public int UpdateAllRevokeRequestAppAccess(int revokeId)
        {
            SqlConnection db = new SqlConnection(connectionString);
            SqlCommand com = new SqlCommand();
            SqlTransaction tran;
            db.Open();
            tran = db.BeginTransaction();
            int i = 0;
            try
            {
                //Run all your insert statements here here
                //Role Request Table Insertion
                com.CommandType = CommandType.Text;
                com.CommandText = "UPDATE APP_ACCESS_REVOCATION_DTL SET STATUS='REV' WHERE REVOKE_ID=@REVOKE_ID";
                com.Connection = db;
                com.Transaction = tran;
                com.Parameters.AddWithValue("@REVOKE_ID", revokeId);
                i = com.ExecuteNonQuery();

                tran.Commit();
            }
            catch (SqlException sqlex)
            {
                tran.Rollback();
            }
            finally
            {
                db.Close();
            }
            return i;
        }

        public int UpdateAllRevokeRequestPrimRole(int revokeId)
        {
            SqlConnection db = new SqlConnection(connectionString);
            SqlCommand com = new SqlCommand();
            SqlTransaction tran;
            db.Open();
            tran = db.BeginTransaction();
            int i = 0;
            try
            {
                //Run all your insert statements here here
                //Role Request Table Insertion
                com.CommandType = CommandType.Text;
                com.CommandText = "UPDATE PRIM_ROLE_REVOCATION_DTL SET STATUS='REV' WHERE REVOKE_ID=@REVOKE_ID";
                com.Connection = db;
                com.Transaction = tran;
                com.Parameters.AddWithValue("@REVOKE_ID", revokeId);
                i = com.ExecuteNonQuery();

                tran.Commit();
            }
            catch (SqlException sqlex)
            {
                tran.Rollback();
            }
            finally
            {
                db.Close();
            }
            return i;
        }
    }
}