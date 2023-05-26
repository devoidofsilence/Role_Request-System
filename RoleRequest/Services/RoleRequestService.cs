using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using RoleRequest.Helpers;
using System.Data.SqlClient;
using System.Data;
using RoleRequest.Models;
using System.Net.Mail;
using RoleRequest.Models.Utils;

namespace RoleRequest.Services
{
    public class RoleRequestService
    {
        String connectionString = String.Empty;
        EmployeeModel emp = null;
        CommonService cs = null;
        public RoleRequestService()
        {
            connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            emp = new EmployeeModel();
            cs = new CommonService();
        }

        public void SaveRoleRequest(RoleRequestModel rrm)
        {
            List<AppAccessLevelMapperModel> almm = new List<AppAccessLevelMapperModel>();
            List<AppDomainModel> aggRoles = new List<AppDomainModel>();
            SqlConnection db = new SqlConnection(connectionString);
            SqlCommand com = new SqlCommand();
            SqlCommand com2 = new SqlCommand();
            SqlCommand com3 = new SqlCommand();
            SqlCommand com4 = new SqlCommand();
            SqlTransaction tran;
            db.Open();
            tran = db.BeginTransaction();
            try
            {
                //Run all your insert statements here here
                //Role Request Table Insertion
                com.CommandType = CommandType.StoredProcedure;
                com.CommandText = "IUD_SP_ROLE_REQUEST";
                com.Connection = db;
                com.Transaction = tran;
                com.Parameters.AddWithValue("@EDIT_MODE", rrm.editMode);
                com.Parameters.AddWithValue("@RECOMMEND_MODE", rrm.recommendMode);
                com.Parameters.AddWithValue("@APPROVE_MODE", rrm.approveMode);
                com.Parameters.AddWithValue("@REJECTION_FLAG", rrm.rejectionFlag);
                com.Parameters.AddWithValue("@RRF_ID", rrm.RoleRequestId);
                com.Parameters.AddWithValue("@REQUEST_DATE", rrm.RoleRequestDate != null ? Convert.ToDateTime(rrm.RoleRequestDate) : DateTime.Now.Date);
                com.Parameters.AddWithValue("@EMPLOYEE_USERNAME", rrm.EmpSamAccountName);
                com.Parameters.AddWithValue("@INPUT_LIMIT_AMT", rrm.InputLimitAmt);
                com.Parameters.AddWithValue("@AUTHORIZATION_LIMIT_AMT", rrm.AuthorizationLimitAmt);
                com.Parameters.AddWithValue("@REMARKS", ((object)rrm.Remarks) ?? DBNull.Value);
                com.Parameters.AddWithValue("@ADDITIONAL_REQUEST", ((object)rrm.AdditionalRequest) ?? DBNull.Value);
                com.Parameters.AddWithValue("@RECOMMENDATION_BY", rrm.RecommendationBySAM);
                com.Parameters.AddWithValue("@APPROVAL_BY", rrm.ApprovalBySAM);
                com.Parameters.AddWithValue("@BRANCH_LOCATION_ID", ((object)rrm.BranchLocationId) ?? DBNull.Value);
                com.Parameters.AddWithValue("@DEPARTMENT_ID", ((object)rrm.DepartmentId) ?? DBNull.Value);
                com.Parameters.AddWithValue("@FUNCTIONAL_TITLE", ((object)rrm.FunctionalTitle) ?? DBNull.Value);
                com.Parameters.AddWithValue("@CORPORATE_TITLE_ID", ((object)rrm.CorporateTitleId) ?? DBNull.Value);
                com.Parameters.AddWithValue("@REQUEST_STATUS", "INIT");
                com.Parameters.AddWithValue("@HRIS_ID", rrm.HRIS_ID);
                com.Parameters.AddWithValue("@FLEX_ID", rrm.FLEX_ID);
                com.Parameters.AddWithValue("@EMAIL_ID", rrm.EMAIL_ID);
                com.Parameters.AddWithValue("@MOBILE_NUMBER", rrm.MOBILE_NUMBER);
                com.Parameters.AddWithValue("@TAKEN_BACK", rrm.takenBackFlag);
                com.Parameters.Add("@INSERTED_RRF_ID", SqlDbType.Int).Direction = ParameterDirection.Output;
                com.Parameters.Add("@INSERTED_HIST_RRF_ID", SqlDbType.Int).Direction = ParameterDirection.Output;
                com.ExecuteNonQuery();
                if (!rrm.recommendMode && !rrm.approveMode && !rrm.takenBackFlag)
                {
                    Int32 operatedRRFId;
                    Int32 operatedHistoryRRFId;
                    if (Int32.TryParse((com.Parameters["@INSERTED_HIST_RRF_ID"].Value).ToString(), out operatedHistoryRRFId))
                    {
                        operatedRRFId = rrm.editMode ? rrm.RoleRequestId : Int32.Parse((com.Parameters["@INSERTED_RRF_ID"].Value).ToString());
                        //Primary Role Detail Table Insertion
                        foreach (var item in rrm.lstPrimaryRoleModel)
                        {
                            //if ((item.LatestSavedRequestFlag && !item.IsSelected) || (!item.LatestSavedRequestFlag && item.IsSelected))
                            //{
                            com2.CommandText = "IUD_SP_PRIMARY_ROLE_DTL";
                            com2.CommandType = CommandType.StoredProcedure;
                            com2.Connection = db;
                            com2.Transaction = tran;
                            com2.Parameters.AddWithValue("@RRF_ID", operatedRRFId);
                            com2.Parameters.AddWithValue("@HIST_RRF_ID", operatedHistoryRRFId);
                            com2.Parameters.AddWithValue("@PRIMARY_ROLE_ID", item.PrimaryRoleId);
                            com2.Parameters.AddWithValue("@IS_CHECKED", item.IsSelected);
                            com2.Parameters.AddWithValue("@CHANGED_FLAG", (item.LatestSavedRequestFlag && !item.IsSelected) || (!item.LatestSavedRequestFlag && item.IsSelected) ? true : item.ChangedFlag);
                            com2.Parameters.AddWithValue("@EDIT_MODE", rrm.editMode);
                            com2.ExecuteNonQuery();
                            com2.Parameters.Clear();
                            //}
                            //else
                            //{
                            //    com2.CommandText = "IUD_SP_PRIMARY_ROLE_DTL";
                            //    com2.CommandType = CommandType.StoredProcedure;
                            //    com2.Connection = db;
                            //    com2.Transaction = tran;
                            //    com2.Parameters.AddWithValue("@RRF_ID", operatedRRFId);
                            //    com2.Parameters.AddWithValue("@HIST_RRF_ID", operatedHistoryRRFId);
                            //    com2.Parameters.AddWithValue("@PRIMARY_ROLE_ID", item.PrimaryRoleId);
                            //    com2.Parameters.AddWithValue("@IS_CHECKED", false);
                            //    com2.Parameters.AddWithValue("@CHANGED_FLAG", false);
                            //    com2.Parameters.AddWithValue("@EDIT_MODE", rrm.editMode);
                            //    com2.ExecuteNonQuery();
                            //    com2.Parameters.Clear();
                            //}
                        }

                        //Access Level Mapper Detail Table Insertion
                        if (rrm.secondaryRoleDomain != null && rrm.secondaryRoleDomain.lstApps != null)
                        {
                            foreach (var apps in rrm.secondaryRoleDomain.lstApps)
                            {
                                foreach (var accesslevel in apps.lstAccessLevel)
                                {
                                    com3.CommandText = "IUD_SP_ACCESS_LEVEL_MAPPER_DTL";
                                    com3.CommandType = CommandType.StoredProcedure;
                                    com3.Connection = db;
                                    com3.Transaction = tran;
                                    com3.Parameters.AddWithValue("@RRF_ID", operatedRRFId);
                                    com3.Parameters.AddWithValue("@HIST_RRF_ID", operatedHistoryRRFId);
                                    com3.Parameters.AddWithValue("@APP_ID", apps.AppId);
                                    com3.Parameters.AddWithValue("@ACCESS_LEVEL_ID", accesslevel.AccessLevelId);
                                    com3.Parameters.AddWithValue("@CHECKED_FLG", accesslevel.IsSelected);
                                    com3.Parameters.AddWithValue("@CHANGED_FLAG", (accesslevel.LatestSavedRequestFlag && !accesslevel.IsSelected) || (!accesslevel.LatestSavedRequestFlag && accesslevel.IsSelected) ? true : accesslevel.ChangedFlag);
                                    com3.Parameters.AddWithValue("@EDIT_MODE", rrm.editMode);
                                    com3.ExecuteNonQuery();
                                    com3.Parameters.Clear();
                                }
                            }
                        }

                        if (rrm.flexcubeRoleDomain != null && rrm.flexcubeRoleDomain.lstApps != null)
                        {
                            foreach (var apps in rrm.flexcubeRoleDomain.lstApps)
                            {
                                foreach (var accesslevel in apps.lstAccessLevel)
                                {
                                    com3.CommandText = "IUD_SP_ACCESS_LEVEL_MAPPER_DTL";
                                    com3.CommandType = CommandType.StoredProcedure;
                                    com3.Connection = db;
                                    com3.Transaction = tran;
                                    com3.Parameters.AddWithValue("@RRF_ID", operatedRRFId);
                                    com3.Parameters.AddWithValue("@HIST_RRF_ID", operatedHistoryRRFId);
                                    com3.Parameters.AddWithValue("@APP_ID", apps.AppId);
                                    com3.Parameters.AddWithValue("@ACCESS_LEVEL_ID", accesslevel.AccessLevelId);
                                    com3.Parameters.AddWithValue("@CHECKED_FLG", accesslevel.IsSelected);
                                    com3.Parameters.AddWithValue("@CHANGED_FLAG", (accesslevel.LatestSavedRequestFlag && !accesslevel.IsSelected) || (!accesslevel.LatestSavedRequestFlag && accesslevel.IsSelected) ? true : accesslevel.ChangedFlag);
                                    com3.Parameters.AddWithValue("@EDIT_MODE", rrm.editMode);
                                    com3.ExecuteNonQuery();
                                    com3.Parameters.Clear();
                                }
                            }
                        }

                        if (rrm.edmsRoleDomain != null && rrm.edmsRoleDomain.lstApps != null)
                        {
                            foreach (var apps in rrm.edmsRoleDomain.lstApps)
                            {
                                foreach (var accesslevel in apps.lstAccessLevel)
                                {
                                    com3.CommandText = "IUD_SP_ACCESS_LEVEL_MAPPER_DTL";
                                    com3.CommandType = CommandType.StoredProcedure;
                                    com3.Connection = db;
                                    com3.Transaction = tran;
                                    com3.Parameters.AddWithValue("@RRF_ID", operatedRRFId);
                                    com3.Parameters.AddWithValue("@HIST_RRF_ID", operatedHistoryRRFId);
                                    com3.Parameters.AddWithValue("@APP_ID", apps.AppId);
                                    com3.Parameters.AddWithValue("@ACCESS_LEVEL_ID", accesslevel.AccessLevelId);
                                    com3.Parameters.AddWithValue("@CHECKED_FLG", accesslevel.IsSelected);
                                    com3.Parameters.AddWithValue("@CHANGED_FLAG", (accesslevel.LatestSavedRequestFlag && !accesslevel.IsSelected) || (!accesslevel.LatestSavedRequestFlag && accesslevel.IsSelected) ? true : accesslevel.ChangedFlag);
                                    com3.Parameters.AddWithValue("@EDIT_MODE", rrm.editMode);
                                    com3.ExecuteNonQuery();
                                    com3.Parameters.Clear();
                                }
                            }
                        }


                        //APP ROLES MAPPER DETAIL table insertion
                        if (rrm.flexcubeRoleDomain != null && rrm.flexcubeRoleDomain.lstApps != null)
                        {
                            foreach (var apps in rrm.flexcubeRoleDomain.lstApps)
                            {
                                foreach (var rolehead in apps.lstRoleHeads)
                                {
                                    foreach (var role in rolehead.lstRolesModel)
                                    {
                                        com4.CommandText = "IUD_SP_APP_ROLES_MAPPER_DTL";
                                        com4.CommandType = CommandType.StoredProcedure;
                                        com4.Connection = db;
                                        com4.Transaction = tran;
                                        com4.Parameters.AddWithValue("@RRF_ID", operatedRRFId);
                                        com4.Parameters.AddWithValue("@HIST_RRF_ID", operatedHistoryRRFId);
                                        com4.Parameters.AddWithValue("@ROLE_ID", role.RoleId);
                                        com4.Parameters.AddWithValue("@CHECKED_FLG", role.IsSelected);
                                        com4.Parameters.AddWithValue("@CHANGED_FLAG", (role.LatestSavedRequestFlag && !role.IsSelected) || (!role.LatestSavedRequestFlag && role.IsSelected) ? true : role.ChangedFlag);
                                        com4.Parameters.AddWithValue("@EDIT_MODE", rrm.editMode);
                                        com4.ExecuteNonQuery();
                                        com4.Parameters.Clear();
                                    }
                                }
                            }
                        }

                        if (rrm.edmsRoleDomain != null && rrm.edmsRoleDomain.lstApps != null)
                        {
                            foreach (var apps in rrm.edmsRoleDomain.lstApps)
                            {
                                foreach (var rolehead in apps.lstRoleHeads)
                                {
                                    foreach (var role in rolehead.lstRolesModel)
                                    {
                                        com4.CommandText = "IUD_SP_APP_ROLES_MAPPER_DTL";
                                        com4.CommandType = CommandType.StoredProcedure;
                                        com4.Connection = db;
                                        com4.Transaction = tran;
                                        com4.Parameters.AddWithValue("@RRF_ID", operatedRRFId);
                                        com4.Parameters.AddWithValue("@HIST_RRF_ID", operatedHistoryRRFId);
                                        com4.Parameters.AddWithValue("@ROLE_ID", role.RoleId);
                                        com4.Parameters.AddWithValue("@CHECKED_FLG", role.IsSelected);
                                        com4.Parameters.AddWithValue("@CHANGED_FLAG", (role.LatestSavedRequestFlag && !role.IsSelected) || (!role.LatestSavedRequestFlag && role.IsSelected) ? true : role.ChangedFlag);
                                        com4.Parameters.AddWithValue("@EDIT_MODE", rrm.editMode);
                                        com4.ExecuteNonQuery();
                                        com4.Parameters.Clear();
                                    }
                                }
                            }
                        }

                        //send email to the Recommender here
                        EmailToRecommender etr = new EmailToRecommender();
                        using (MailMessage msg = new MailMessage())
                        {
                            try
                            {
                                msg.From = new MailAddress(ConfigurationManager.AppSettings["emailFromIDM"]);
                                msg.Subject = String.Format(etr.Subject);
                                msg.Body = String.Format(etr.Body(rrm.EmployeeFullName));
                                msg.IsBodyHtml = true;
                                emp = new EmployeeModel();
                                emp = cs.GetUserFromADBySamName(rrm.RecommendationBySAM);
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
                            catch (Exception ex)
                            {
                                tran.Rollback();
                            }
                        }
                    }
                }
                else if (rrm.recommendMode && !rrm.approveMode && !rrm.rejectionFlag)
                {
                    //send email to the Approver here
                    EmailToApprover etr = new EmailToApprover();
                    using (MailMessage msg = new MailMessage())
                    {
                        try
                        {
                            msg.From = new MailAddress(ConfigurationManager.AppSettings["emailFromIDM"]);
                            msg.Subject = String.Format(etr.Subject);
                            msg.Body = String.Format(etr.Body(rrm.EmployeeFullName));
                            msg.IsBodyHtml = true;
                            emp = new EmployeeModel();
                            emp = cs.GetUserFromADBySamName(rrm.ApprovalBySAM);
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
                        catch (Exception ex)
                        {
                            tran.Rollback();
                        }
                    }
                }
                else if (!rrm.recommendMode && rrm.approveMode && !rrm.rejectionFlag)
                {
                    //send email to the IT Admins here
                    EmailToAdmin etr = new EmailToAdmin();
                    using (MailMessage msg = new MailMessage())
                    {
                        try
                        {
                            msg.From = new MailAddress(ConfigurationManager.AppSettings["emailFromIDM"]);
                            msg.Subject = String.Format(etr.Subject);
                            msg.Body = String.Format(etr.Body(rrm.EmployeeFullName));
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
                else if (rrm.rejectionFlag)
                {
                    //send email to the Requester
                    EmailRejectionToRequester etr = new EmailRejectionToRequester();
                    using (MailMessage msg = new MailMessage())
                    {
                        try
                        {
                            msg.From = new MailAddress(ConfigurationManager.AppSettings["emailFromIDM"]);
                            msg.Subject = String.Format(etr.Subject);
                            msg.Body = String.Format(etr.Body());
                            msg.IsBodyHtml = true;
                            emp = new EmployeeModel();
                            emp = cs.GetUserFromADBySamName(rrm.EmpSamAccountName);
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
                        catch (Exception ex)
                        {
                            tran.Rollback();
                        }
                    }
                }
                else if (rrm.takenBackFlag)
                {
                    if (rrm.RequestStatus == "INIT")
                    {
                        //send email to the Recommender
                        EmailTakenBackToRecommender etr = new EmailTakenBackToRecommender();
                        using (MailMessage msg = new MailMessage())
                        {
                            try
                            {
                                msg.From = new MailAddress(ConfigurationManager.AppSettings["emailFromIDM"]);
                                msg.Subject = String.Format(etr.Subject);
                                emp = new EmployeeModel();
                                emp = cs.GetUserFromADBySamName(rrm.EmpSamAccountName);
                                msg.Body = String.Format(etr.Body(emp.EmployeeFirstName + " " + emp.EmployeeLastName, rrm.Remarks));
                                msg.IsBodyHtml = true;
                                emp = new EmployeeModel();
                                emp = cs.GetUserFromADBySamName(rrm.RecommendationBySAM);
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
                            catch (Exception ex)
                            {
                                tran.Rollback();
                            }
                        }
                    }
                    else if (rrm.RequestStatus == "RECC")
                    {
                        //send email to the Approver
                        EmailTakenBackToApprover etr = new EmailTakenBackToApprover();
                        using (MailMessage msg = new MailMessage())
                        {
                            try
                            {
                                msg.From = new MailAddress(ConfigurationManager.AppSettings["emailFromIDM"]);
                                msg.Subject = String.Format(etr.Subject);
                                emp = new EmployeeModel();
                                emp = cs.GetUserFromADBySamName(rrm.EmpSamAccountName);
                                msg.Body = String.Format(etr.Body(emp.EmployeeFirstName + " " + emp.EmployeeLastName, rrm.Remarks));
                                msg.IsBodyHtml = true;
                                emp = new EmployeeModel();
                                emp = cs.GetUserFromADBySamName(rrm.ApprovalBySAM);
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
                            catch (Exception ex)
                            {
                                tran.Rollback();
                            }
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
        }

        public List<AppAccessLevelModel> GetAccessLevelsForAppId(int appID)
        {
            var appAccessLevels = new List<AppAccessLevelModel>();
            AppAccessLevelModel aal = null;
            string commandText = "SELECT * FROM DBO.APP_ACCESS_LEVEL AAL WITH (NOLOCK) INNER JOIN ACCESS_LEVEL_MAPPER ALM WITH (NOLOCK) ON AAL.ACCESS_LEVEL_ID=ALM.ACCESS_LEVEL_ID WHERE ALM.APP_ID='" + appID + "'";

            using (SqlDataReader reader = SqlHelper.ExecuteReader(connectionString, commandText,
                CommandType.Text))
            {
                while (reader.Read())
                {
                    if (reader.HasRows)
                    {
                        aal = new AppAccessLevelModel()
                        {
                            AccessLevelId = (int)reader["ACCESS_LEVEL_ID"],
                            AccessLevelName = reader["ACCESS_LEVEL_NAME"].ToString(),
                            IsSelected = false
                        };
                        appAccessLevels.Add(aal);
                    }
                }
            }
            return appAccessLevels;
        }

        public List<AppAccessLevelModel> GetSavedAccessLevelFromRRFIDAndAppID(int rrfID, int appID)
        {
            var appAccessLevels = new List<AppAccessLevelModel>();
            AppAccessLevelModel aal = null;
            string commandText = "SELECT * FROM DBO.ACCESS_LEVEL_MAPPER_DTL ALMD WITH (NOLOCK) INNER JOIN APP_ACCESS_LEVEL AAL WITH (NOLOCK) ON ALMD.ACCESS_LEVEL_ID=AAL.ACCESS_LEVEL_ID WHERE ALMD.RRF_ID='" + rrfID + "' AND ALMD.APP_ID='" + appID + "'";

            using (SqlDataReader reader = SqlHelper.ExecuteReader(connectionString, commandText,
                CommandType.Text))
            {
                while (reader.Read())
                {
                    if (reader.HasRows)
                    {
                        aal = new AppAccessLevelModel()
                        {
                            AccessLevelId = (int)reader["ACCESS_LEVEL_ID"],
                            AccessLevelName = reader["ACCESS_LEVEL_NAME"].ToString(),
                            IsSelected = Convert.ToBoolean(reader["CHECKED_FLG"]),
                            ChangedFlag = !string.IsNullOrEmpty(reader["CHANGED_FLAG"].ToString()) ? (bool)reader["CHANGED_FLAG"] : false
                        };
                        appAccessLevels.Add(aal);
                    }
                }
            }
            return appAccessLevels;
        }

        public List<AppRolesModel> GetRoles()
        {
            var appRoles = new List<AppRolesModel>();
            AppRolesModel ar = null;
            string commandText = "SELECT * FROM DBO.APP_ROLES";

            using (SqlDataReader reader = SqlHelper.ExecuteReader(connectionString, commandText,
                CommandType.Text))
            {
                while (reader.Read())
                {
                    if (reader.HasRows)
                    {
                        ar = new AppRolesModel()
                        {
                            RoleId = (int)reader["ROLE_ID"],
                            RoleName = reader["ROLE_NAME"].ToString(),
                            IsSelected = false
                        };
                        appRoles.Add(ar);
                    }
                }
            }
            return appRoles;
        }

        public List<AppRolesModel> GetSavedRolesFromRRFIDAndAppID(int rrfID, int roleID)
        {
            var appRoles = new List<AppRolesModel>();
            AppRolesModel ar = null;
            string commandText = "SELECT * FROM DBO.APP_ROLES_MAPPER_DTL ARMD WITH (NOLOCK) INNER JOIN APP_ROLES AR WITH (NOLOCK) ON ARMD.ROLE_ID=AR.ROLE_ID WHERE ARMD.RRF_ID='" + rrfID + "' AND ARMD.ROLE_ID='" + roleID + "'";

            using (SqlDataReader reader = SqlHelper.ExecuteReader(connectionString, commandText,
                CommandType.Text))
            {
                while (reader.Read())
                {
                    if (reader.HasRows)
                    {
                        ar = new AppRolesModel()
                        {
                            RoleId = (int)reader["ROLE_ID"],
                            RoleName = reader["ROLE_NAME"].ToString(),
                            IsSelected = Convert.ToBoolean(reader["CHECKED_FLG"]),
                            ChangedFlag = !string.IsNullOrEmpty(reader["CHANGED_FLAG"].ToString()) ? (bool)reader["CHANGED_FLAG"] : false
                        };
                        appRoles.Add(ar);
                    }
                }
            }
            return appRoles;
        }
    }
}