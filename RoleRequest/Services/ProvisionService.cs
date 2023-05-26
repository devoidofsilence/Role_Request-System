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
    public class ProvisionService
    {
        String connectionString = String.Empty;
        public ProvisionService()
        {
            connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        }

        public ProvisionModel GetProvisionFromRRFIdAndProvisionId(int rrfId)
        {
            var provision = new ProvisionModel();
            String commandText = "SELECT * FROM DBO.PROVISION PRV INNER JOIN ROLE_REQUEST RR ON PRV.RRF_ID = RR.RRF_ID WHERE PRV.RRF_ID='" + rrfId + "'";

            using (SqlDataReader reader = SqlHelper.ExecuteReader(connectionString, commandText,
                CommandType.Text))
            {
                while (reader.Read())
                {
                    if (reader.HasRows)
                    {
                        provision.ProvisionId = (int)reader["PROVISION_ID"];
                        provision.RoleRequestId = rrfId;
                        provision.ProvidedBy = reader["PROVIDED_BY"].ToString();
                        provision.ProvisionStatus = reader["PROVISION_STATUS"].ToString();
                        provision.RequestedByUsername = reader["EMPLOYEE_USERNAME"].ToString();
                    }
                }
            }
            return provision;
        }

        public List<AccessProvisionDtlModel> GetAccessProvisionDtlFromRRFIdAndProvisionID(int rrfId, int provisionId)
        {
            var accessProvisionDtls = new List<AccessProvisionDtlModel>();
            AccessProvisionDtlModel apdm = null;
            string commandText = "SELECT * FROM DBO.ACCESS_PROVISION_DTL APD INNER JOIN APP A ON APD.APP_ID = A.APP_ID WHERE APD.ACCESS_PROVISION_ID = '" + provisionId + "' AND APD.RRF_ID = '" + rrfId + "'";

            using (SqlDataReader reader = SqlHelper.ExecuteReader(connectionString, commandText,
                CommandType.Text))
            {
                while (reader.Read())
                {
                    if (reader.HasRows)
                    {
                        apdm = new AccessProvisionDtlModel()
                        {
                            AccessProvisionId = provisionId,
                            RrfId = rrfId,
                            AppId = (int)reader["APP_ID"],
                            AppName = reader["APP_NAME"].ToString(),
                            AppRemark = reader["APP_REMARK"].ToString()
                        };
                        accessProvisionDtls.Add(apdm);
                    }
                }
            }
            return accessProvisionDtls;
        }

        public List<PrimRoleProvisionDtlModel> GetPrimaryRoleProvisionDtlFromRRFIdAndProvisionID(int rrfId, int provisionId)
        {
            var primRoleProvisionDtls = new List<PrimRoleProvisionDtlModel>();
            PrimRoleProvisionDtlModel prpdm = new PrimRoleProvisionDtlModel();
            string commandText = "SELECT * FROM DBO.PRIM_ROLE_PROVISION_DTL PRPD INNER JOIN PRIMARY_ROLE PR ON PRPD.PRIM_ROLE_ID = PR.PRIMARY_ROLE_ID WHERE PRPD.PROVISION_ID = '" + provisionId + "' AND PRPD.RRF_ID = '" + rrfId + "'";

            using (SqlDataReader reader = SqlHelper.ExecuteReader(connectionString, commandText,
                CommandType.Text))
            {
                while (reader.Read())
                {
                    if (reader.HasRows)
                    {
                        prpdm = new PrimRoleProvisionDtlModel()
                        {
                            ProvisionId = provisionId,
                            RrfId = rrfId,
                            PrimaryRoleId = (int)reader["PRIM_ROLE_ID"],
                            PrimaryRoleName = reader["ROLE_NAME"].ToString(),
                            PrimaryRoleRemark = reader["ROLE_REMARK"].ToString()
                        };
                        primRoleProvisionDtls.Add(prpdm);
                    }
                }
            }
            return primRoleProvisionDtls;
        }

        public AdditionalAccessProvisionDtlModel GetAdditionalAccessProvisionDtlFromRRFIdAndProvisionID(int rrfId, int provisionId)
        {
            var accessProvisionDtl = new AdditionalAccessProvisionDtlModel();
            String commandText = "SELECT * FROM DBO.ADDITIONAL_ACCESS_PROVISION_DTL WHERE PROVISION_ID = '" + provisionId + "' AND RRF_ID = '" + rrfId + "'";

            using (SqlDataReader reader = SqlHelper.ExecuteReader(connectionString, commandText,
                CommandType.Text))
            {
                while (reader.Read())
                {
                    if (reader.HasRows)
                    {
                        accessProvisionDtl.ProvisionId = provisionId;
                        accessProvisionDtl.RrfId = rrfId;
                        accessProvisionDtl.AccessRemarks = reader["ACCESS_REMARK"].ToString();
                    }
                }
            }
            return accessProvisionDtl;
        }

        public int SaveProvisionRequest(ProvisionModel pm)
        {
            SqlConnection db = new SqlConnection(connectionString);
            SqlCommand com = new SqlCommand();
            SqlCommand com2 = new SqlCommand();
            SqlCommand com3 = new SqlCommand();
            SqlTransaction tran;
            db.Open();
            tran = db.BeginTransaction();
            int i = 0;
            try
            {
                //Run all your insert statements here here
                //Role Request Table Insertion
                com.CommandType = CommandType.StoredProcedure;
                com.CommandText = "IUD_SP_PROVISION";
                com.Connection = db;
                com.Transaction = tran;
                com.Parameters.AddWithValue("@PROVIDED_BY", pm.ProvidedBy);
                com.Parameters.AddWithValue("@PROVISION_STATUS", pm.ProvisionStatus);
                com.Parameters.AddWithValue("@PROVISION_ID", pm.ProvisionId);
                com.Parameters.AddWithValue("@REJECTION_FLAG", pm.rejectionFlag);
                com.Parameters.AddWithValue("@RRF_ID", pm.RoleRequestId);
                com.Parameters.AddWithValue("@REMARKS", pm.Remarks);
                com.Parameters.AddWithValue("@SAVEMODE", pm.saveRequest);
                i = com.ExecuteNonQuery();
                if (!pm.rejectionFlag && pm.saveRequest && i == 3)
                {
                    if (pm.prpdmList != null)
                    {
                        foreach (var item in pm.prpdmList)
                        {
                            com2.CommandText = "IUD_SP_PRIM_ROLE_PROVISION_DTL";
                            com2.CommandType = CommandType.StoredProcedure;
                            com2.Connection = db;
                            com2.Transaction = tran;
                            com2.Parameters.AddWithValue("@RRF_ID", item.RrfId);
                            com2.Parameters.AddWithValue("@PROVISION_ID", item.ProvisionId);
                            com2.Parameters.AddWithValue("@PRIM_ROLE_ID", item.PrimaryRoleId);
                            com2.Parameters.AddWithValue("@ROLE_REMARK", item.PrimaryRoleRemark);
                            i = com2.ExecuteNonQuery();
                            com2.Parameters.Clear();
                        }
                    }
                    if (pm.apdmList!=null)
                    {
                        foreach (var item in pm.apdmList)
                        {
                            com3.CommandText = "IUD_SP_ACCESS_PROVISION_DTL";
                            com3.CommandType = CommandType.StoredProcedure;
                            com3.Connection = db;
                            com3.Transaction = tran;
                            com3.Parameters.AddWithValue("@RRF_ID", item.RrfId);
                            com3.Parameters.AddWithValue("@ACCESS_PROVISION_ID", item.AccessProvisionId);
                            com3.Parameters.AddWithValue("@APP_ID", item.AppId);
                            com3.Parameters.AddWithValue("@APP_REMARK", item.AppRemark);
                            i = com3.ExecuteNonQuery();
                            com3.Parameters.Clear();
                        }
                    }
                    if (pm.additionalAccess != null)
                    {
                            com3.CommandText = "IUD_SP_ADDITIONAL_ACCESS_PROVISION_DTL";
                            com3.CommandType = CommandType.StoredProcedure;
                            com3.Connection = db;
                            com3.Transaction = tran;
                            com3.Parameters.AddWithValue("@RRF_ID", pm.additionalAccess.RrfId);
                            com3.Parameters.AddWithValue("@PROVISION_ID", pm.additionalAccess.ProvisionId);
                            com3.Parameters.AddWithValue("@ACCESS_REMARK", pm.additionalAccess.AccessRemarks);
                            i = com3.ExecuteNonQuery();
                            com3.Parameters.Clear();
                    }
                    //send email to the requester here
                    EmailToRequester etr = new EmailToRequester();
                    using (MailMessage msg = new MailMessage())
                    {
                        try
                        {
                            msg.From = new MailAddress(ConfigurationManager.AppSettings["emailFromIDM"]);
                            //msg.From = new MailAddress(_emailFrom);
                            msg.Subject = String.Format(etr.Subject);
                            msg.Body = String.Format(etr.Body(pm.prpdmList, pm.apdmList, pm.additionalAccess));
                            msg.IsBodyHtml = true;
                            msg.To.Add(new MailAddress(pm.RequestedByEmailID));


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
                else if (pm.rejectionFlag)
                {
                    //send email to the requester here
                    EmailRejectionToRequester etr = new EmailRejectionToRequester();
                    using (MailMessage msg = new MailMessage())
                    {
                        try
                        {
                            msg.From = new MailAddress(ConfigurationManager.AppSettings["emailFromIDM"]);
                            //msg.From = new MailAddress(_emailFrom);
                            msg.Subject = String.Format(etr.Subject);
                            msg.Body = String.Format(etr.Body());
                            msg.IsBodyHtml = true;
                            msg.To.Add(new MailAddress(pm.RequestedByEmailID));


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