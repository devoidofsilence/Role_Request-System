using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using RoleRequest.Models;
using System.Data.SqlClient;
using System.Data;
using RoleRequest.Helpers;
using RoleRequest.Models.Utils;
using System.Net.Mail;

namespace RoleRequest.Services
{
    public class EnrollUserService
    {
        String connectionString = String.Empty;
        EmployeeModel emp = null;
        CommonService cs = null;
        public EnrollUserService()
        {
            connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            emp = new EmployeeModel();
            cs = new CommonService();
        }

        public int SaveEnrollUser(EnrollUserModel eum)
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
                com.Parameters.AddWithValue("@FULL_NAME", eum.FullName);
                com.Parameters.AddWithValue("@DEPARTMENT_ID", eum.DepartmentId);
                com.Parameters.AddWithValue("@BRANCH_LOCATION_ID", eum.BranchLocationId);
                com.Parameters.AddWithValue("@CORPORATE_TITLE_ID", eum.CorpTitleId);
                com.Parameters.AddWithValue("@FUNCTIONAL_TITLE", eum.FunctionalTitle);
                com.Parameters.AddWithValue("@MOBILE_NUMBER", eum.MobileNumber);
                com.Parameters.AddWithValue("@JOIN_DATE", eum.JoinDate);
                com.Parameters.AddWithValue("@SUPERVISION_BY", eum.SupervisionBy);
                com.Parameters.AddWithValue("@STATUS", "INIT");
                i = com.ExecuteNonQuery();

                //send email to the HR resources here
                EmailToHR etr = new EmailToHR();
                using (MailMessage msg = new MailMessage())
                {
                    try
                    {
                        msg.From = new MailAddress(ConfigurationManager.AppSettings["emailFromIDM"]);
                        msg.Subject = String.Format(etr.Subject);
                        msg.Body = String.Format(etr.Body(eum.FullName));
                        msg.IsBodyHtml = true;

                        List<UserModel> umList = new List<UserModel>();
                        umList = cs.GetUsersFromUserTableByRoleName("HR_ADMIN");

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

        public List<EnrollUserModel> EnrolledUsersBySupervisorADName(string supervisorADName)
        {
            List<EnrollUserModel> enrollUsersList = new List<EnrollUserModel>();

            String commandText = @"SELECT UE.*, CT.TITLE_NAME CORP_TITLE_NAME, DEP.DEPARTMENT_NAME DEPARTMENT_NAME, BRN.BRANCH_NAME BRANCH_NAME FROM DBO.USER_ENROLL UE
                                    INNER JOIN DBO.DEPARTMENT DEP ON UE.DEPARTMENT_ID = DEP.DEPARTMENT_ID
                                    INNER JOIN DBO.BRANCH BRN ON UE.BRANCH_LOCATION_ID = BRN.BRANCH_ID
                                    INNER JOIN DBO.CORPORATE_TITLE CT ON UE.CORPORATE_TITLE_ID = CT.TITLE_ID
                                    WHERE UE.SUPERVISION_BY='" + supervisorADName + "' ORDER BY CASE WHEN UE.STATUS = 'INIT' THEN 1 WHEN UE.STATUS = 'FFAE' THEN 2 ELSE 3 END ASC";

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

        public EnrollUserModel EnrolledUserDetailByEnrollUserID(int enrollUserID)
        {
            EnrollUserModel enrollUser = new EnrollUserModel();

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
                        enrollUser = new EnrollUserModel()
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
                            Email = reader["EMAIL"].ToString(),
                            EmailPwd = reader["EMAIL_PWD"].ToString(),
                            ADUsername = reader["AD_USERNAME"].ToString(),
                            ADPassword = reader["AD_USERNAME_PWD"].ToString(),
                            EmployeeID = reader["EMPLOYEE_ID"].ToString(),
                            EmployeeIDPwd = reader["EMPLOYEE_ID_PWD"].ToString(),
                            Remarks = reader["STATUS"].ToString() == "RJCT" ? reader["REMARKS"].ToString() : string.Empty
                        };
                    }
                }
            }
            return enrollUser;
        }
    }
}