using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using RoleRequest.Models;
using System.Data.SqlClient;
using RoleRequest.Helpers;
using System.Data;
using RoleRequest.Models.Utils;
using System.Net.Mail;

namespace RoleRequest.Services
{
    public class HRService
    {
        String connectionString = String.Empty;
        EmployeeModel emp = null;
        CommonService cs = null;
        public HRService()
        {
            connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            emp = new EmployeeModel();
            cs = new CommonService();
        }

        public int SaveEnrollUser(HRUserModel eum)
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
                com.Parameters.AddWithValue("@EMPLOYEE_ID", eum.EmployeeID);
                com.Parameters.AddWithValue("@EMPLOYEE_ID_PWD", eum.EmployeeIDPwd);
                com.Parameters.AddWithValue("@ASSIGNED_HR_ADMIN", eum.AssignedHRAdmin);
                com.Parameters.AddWithValue("@REMARKS", eum.Remarks);
                com.Parameters.AddWithValue("@STATUS", eum.Status);
                i = com.ExecuteNonQuery();

                if (eum.Status == "FFAE")
                {
                    //send email to the IAM resources here
                    EmailToIAM etr = new EmailToIAM();
                    using (MailMessage msg = new MailMessage())
                    {
                        try
                        {
                            msg.From = new MailAddress(ConfigurationManager.AppSettings["emailFromIDM"]);
                            msg.Subject = String.Format(etr.Subject);
                            string fullName = GetUserFullNameFromUserID(eum.EnrollUserID);
                            msg.Body = String.Format(etr.Body(fullName));
                            msg.IsBodyHtml = true;

                            List<UserModel> umList = new List<UserModel>();
                            umList = cs.GetUsersFromUserTableByRoleName("ADMIN");

                            foreach (var item in umList)
                            {
                                emp = new EmployeeModel();
                                emp = cs.GetUserFromADBySamName(item.ADUsername);
                                msg.To.Add(new MailAddress(emp.EmailId.Trim()));
                            }

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

        public int RejectEnrollFromHR(HRUserRejectModel eum)
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
                com.Parameters.AddWithValue("@ASSIGNED_HR_ADMIN", eum.AssignedHRAdmin);
                com.Parameters.AddWithValue("@REMARKS", eum.Remarks);
                com.Parameters.AddWithValue("@STATUS", eum.Status);
                i = com.ExecuteNonQuery();

                if (eum.Status == "RJCT")
                {
                    //send email to the Supervisor resources here
                    EmailRejectToSupervisor etr = new EmailRejectToSupervisor();
                    using (MailMessage msg = new MailMessage())
                    {
                        try
                        {
                            msg.From = new MailAddress(ConfigurationManager.AppSettings["emailFromIDM"]);
                            msg.Subject = String.Format(etr.Subject);
                            string fullName = GetUserFullNameFromUserID(eum.EnrollUserID);
                            msg.Body = String.Format(etr.Body(fullName, eum.Remarks));
                            msg.IsBodyHtml = true;

                            emp = new EmployeeModel();
                            emp = cs.GetUserFromADBySamName(eum.SupervisionBy);
                            if (!string.IsNullOrEmpty(emp.EmailId))
                            {
                                msg.To.Add(new MailAddress(emp.EmailId.Trim()));

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
                        }
                        catch (Exception ex)
                        {
                            tran.Rollback();
                        }
                    }
                }

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

        public List<EnrollUserModel> HRApprovalUsers(string hrAdminADName)
        {
            List<EnrollUserModel> enrollUsersList = new List<EnrollUserModel>();

            String commandText = @"SELECT UE.*, CT.TITLE_NAME CORP_TITLE_NAME, DEP.DEPARTMENT_NAME DEPARTMENT_NAME, BRN.BRANCH_NAME BRANCH_NAME FROM DBO.USER_ENROLL UE
                                    INNER JOIN DBO.DEPARTMENT DEP ON UE.DEPARTMENT_ID = DEP.DEPARTMENT_ID
                                    INNER JOIN DBO.BRANCH BRN ON UE.BRANCH_LOCATION_ID = BRN.BRANCH_ID
                                    INNER JOIN DBO.CORPORATE_TITLE CT ON UE.CORPORATE_TITLE_ID = CT.TITLE_ID
                                    WHERE UE.STATUS='INIT' OR UE.ASSIGNED_HR_ADMIN='" + hrAdminADName + "' ORDER BY CASE WHEN UE.STATUS = 'INIT' THEN 1 WHEN UE.STATUS = 'FFAE' THEN 2 ELSE 3 END ASC";


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
                            SupervisionBy = reader["SUPERVISION_BY"].ToString()
                        };
                        enrollUsersList.Add(eum);
                    }
                }
            }
            return enrollUsersList;
        }

        public HRUserModel EnrolledUserDetailByEnrollUserID(int enrollUserID)
        {
            HRUserModel enrollUser = new HRUserModel();

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
                        enrollUser = new HRUserModel()
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
                            SupervisionBy = reader["SUPERVISION_BY"].ToString()
                        };
                    }
                }
            }
            return enrollUser;
        }

        public HRUserRejectModel EnrolledUserDetailByEnrollUserIDForRejection(int enrollUserID)
        {
            HRUserRejectModel enrollUser = new HRUserRejectModel();

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
                        enrollUser = new HRUserRejectModel()
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
                            SupervisionBy = reader["SUPERVISION_BY"].ToString()
                        };
                    }
                }
            }
            return enrollUser;
        }

        public bool CheckDuplicateEmployeeID(string employeeID)
        {
            bool hasDuplicateEmployeeID = false;
            String commandText = @"SELECT * FROM USER_ENROLL WHERE EMPLOYEE_ID = '" + employeeID.Trim() + "'";

            using (SqlDataReader reader = SqlHelper.ExecuteReader(connectionString, commandText,
                CommandType.Text))
            {
                while (reader.Read())
                {
                    if (reader.HasRows)
                    {
                        hasDuplicateEmployeeID = true;
                    }
                }
            }
            return hasDuplicateEmployeeID;
        }

        public string GetUserFullNameFromUserID(int userID)
        {
            string FullName = string.Empty;
            String commandText = "SELECT * FROM USER_ENROLL WITH (NOLOCK) WHERE USER_ID = '" + userID + "'";

            using (SqlDataReader reader = SqlHelper.ExecuteReader(connectionString, commandText,
                CommandType.Text))
            {
                while (reader.Read())
                {
                    if (reader.HasRows)
                    {
                        FullName = reader["FULL_NAME"].ToString();
                    }
                }
            }
            return FullName;
        }

        public List<RevokeEmployeeModel> GetEmployeeListAvailableForRevocation()
        {
            List<RevokeEmployeeModel> employeeList = new List<RevokeEmployeeModel>();
            List<EmployeeModel> allEmployeesFromAD = new List<EmployeeModel>();
            if (HttpContext.Current.Cache["Employees"] != null)
            {
                allEmployeesFromAD = (List<EmployeeModel>)HttpContext.Current.Cache["Employees"];
            }

            String commandText = @"WITH CTE AS 
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
            return employeeList;
        }

        public int SaveRevokeRequest(RevokeRequestModel rrm)
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
                com.CommandText = "IUD_SP_REVOKE_REQUEST";
                com.Connection = db;
                com.Transaction = tran;
                com.Parameters.AddWithValue("@REVOKE_ID", rrm.RevokeId);
                com.Parameters.AddWithValue("@REVOKE_EMP_USERNAME", rrm.RevokeEmpUsername);
                com.Parameters.AddWithValue("@REQUEST_STATUS", rrm.RequestStatus);
                com.Parameters.AddWithValue("@REQUESTED_BY", rrm.RequestedBy);
                com.Parameters.AddWithValue("@REVOKE_REQUEST_DATE", rrm.RevokeDate);
                com.Parameters.AddWithValue("@RESIGNATION_DATE", rrm.ResignationDate);
                com.Parameters.AddWithValue("@TAKEN_BY", rrm.TakenBy);
                com.Parameters.AddWithValue("@REMARKS", rrm.Remarks);
                i = com.ExecuteNonQuery();

                if (rrm.RequestStatus == "INIT")
                {
                    List<EmployeeModel> allEmplList = new List<EmployeeModel>();
                    //send email to the IAM resources here
                    if (HttpContext.Current.Cache["Employees"] != null)
                    {
                        allEmplList = (List<EmployeeModel>)HttpContext.Current.Cache["Employees"];
                    }
                    else
                    {
                        allEmplList = cs.GetEmployeesFromAD();
                    }
                    EmailToIAMRevoke etr = new EmailToIAMRevoke();
                    using (MailMessage msg = new MailMessage())
                    {
                        try
                        {
                            msg.From = new MailAddress(ConfigurationManager.AppSettings["emailFromIDM"]);
                            msg.Subject = String.Format(etr.Subject);
                            string fullName = allEmplList.Where(x => x.SamAccountName == rrm.RevokeEmpUsername).FirstOrDefault().EmployeeFullName;
                            msg.Body = String.Format(etr.Body(fullName));
                            msg.IsBodyHtml = true;

                            List<UserModel> umList = new List<UserModel>();
                            umList = cs.GetUsersFromUserTableByRoleName("ADMIN");

                            foreach (var item in umList)
                            {
                                emp = new EmployeeModel();
                                emp = cs.GetUserFromADBySamName(item.ADUsername);
                                msg.To.Add(new MailAddress(emp.EmailId.Trim()));
                            }

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

                else if (rrm.RequestStatus == "DONE")
                {
                    List<EmployeeModel> allEmplList = new List<EmployeeModel>();
                    //send email to the IAM resources here
                    if (HttpContext.Current.Cache["Employees"] != null)
                    {
                        allEmplList = (List<EmployeeModel>)HttpContext.Current.Cache["Employees"];
                    }
                    else
                    {
                        allEmplList = cs.GetEmployeesFromAD();
                    }
                    EmailToRevokeRequester etrr = new EmailToRevokeRequester();
                    using (MailMessage msg = new MailMessage())
                    {
                        try
                        {
                            msg.From = new MailAddress(ConfigurationManager.AppSettings["emailFromIDM"]);
                            msg.Subject = String.Format(etrr.Subject);
                            string fullName = allEmplList.Where(x => x.SamAccountName == rrm.RevokeEmpUsername).FirstOrDefault().EmployeeFullName;
                            msg.Body = String.Format(etrr.Body(fullName));
                            msg.IsBodyHtml = true;

                            string requesterEmail = allEmplList.Where(x => x.SamAccountName == rrm.RequestedBy).FirstOrDefault().EmailId;
                            msg.To.Add(new MailAddress(requesterEmail));

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