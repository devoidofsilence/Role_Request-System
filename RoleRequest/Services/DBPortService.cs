using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RoleRequest.Models;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using RoleRequest.Helpers;

namespace RoleRequest.Services
{
    public class DBPortService
    {
        String connectionString = String.Empty;
        EmployeeModel emp = null;
        CommonService cs = null;
        public DBPortService()
        {
            connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            emp = new EmployeeModel();
            cs = new CommonService();
        }

        public List<RoleRequestModel> GetAllRoleRequests()
        {
            List<RoleRequestModel> roleRequestList = new List<RoleRequestModel>();

            String commandText = @"SELECT * FROM ROLE_REQUEST ORDER BY RRF_ID ASC";


            using (SqlDataReader reader = SqlHelper.ExecuteReader(connectionString, commandText,
                CommandType.Text))
            {
                while (reader.Read())
                {
                    if (reader.HasRows)
                    {
                        RoleRequestModel rrm = new RoleRequestModel
                        {
                            RoleRequestId = (int)reader["RRF_ID"],
                            EmpSamAccountName = reader["EMPLOYEE_USERNAME"].ConvertToString(),
                            RequestStatus = reader["REQUEST_STATUS"].ConvertToString()
                        };
                        roleRequestList.Add(rrm);
                    }
                }
            }
            return roleRequestList;
        }

        public List<PrimaryRoleModel> GetAllPrimaryRoleDetailForEmpUsername(List<int> roleRequestIds)
        {
            List<PrimaryRoleModel> prmList = new List<PrimaryRoleModel>();

            string roleRequestIdsListString = string.Empty;

            roleRequestIdsListString = string.Concat("(", String.Join(",", roleRequestIds), ")");

            string commandText = @"SELECT * FROM PRIMARY_ROLE_DTL WHERE RRF_ID IN " + roleRequestIdsListString + " ORDER BY RRF_ID";


            using (SqlDataReader reader = SqlHelper.ExecuteReader(connectionString, commandText,
                CommandType.Text))
            {
                while (reader.Read())
                {
                    if (reader.HasRows)
                    {
                        PrimaryRoleModel prm = new PrimaryRoleModel
                        {
                            RoleRequestId = (int)reader["RRF_ID"],
                            PrimaryRoleId = (int)reader["PRIMARY_ROLE_ID"],
                            IsSelected = (bool)reader["IS_CHECKED"]
                        };
                        prmList.Add(prm);
                    }
                }
            }
            return prmList;
        }

        public List<AppRolesModel> GetAllAppRoleDetailForEmpUsername(List<int> roleRequestIds)
        {
            List<AppRolesModel> arList = new List<AppRolesModel>();

            string roleRequestIdsListString = string.Empty;

            roleRequestIdsListString = string.Concat("(", String.Join(",", roleRequestIds), ")");

            string commandText = @"SELECT * FROM APP_ROLES_MAPPER_DTL WHERE RRF_ID IN " + roleRequestIdsListString + " ORDER BY RRF_ID";


            using (SqlDataReader reader = SqlHelper.ExecuteReader(connectionString, commandText,
                CommandType.Text))
            {
                while (reader.Read())
                {
                    if (reader.HasRows)
                    {
                        AppRolesModel ar = new AppRolesModel
                        {
                            RoleRequestId = (int)reader["RRF_ID"],
                            RoleId = (int)reader["ROLE_ID"],
                            IsSelected = (bool)reader["CHECKED_FLG"]
                        };
                        arList.Add(ar);
                    }
                }
            }
            return arList;
        }

        public List<AppAccessLevelModel> GetAllAppAccessLevelDetailForEmpUsername(List<int> roleRequestIds)
        {
            List<AppAccessLevelModel> aalList = new List<AppAccessLevelModel>();

            string roleRequestIdsListString = string.Empty;

            roleRequestIdsListString = string.Concat("(", String.Join(",", roleRequestIds), ")");

            string commandText = @"SELECT * FROM [ACCESS_LEVEL_MAPPER_DTL] WHERE RRF_ID IN " + roleRequestIdsListString + " ORDER BY RRF_ID";


            using (SqlDataReader reader = SqlHelper.ExecuteReader(connectionString, commandText,
                CommandType.Text))
            {
                while (reader.Read())
                {
                    if (reader.HasRows)
                    {
                        AppAccessLevelModel ar = new AppAccessLevelModel
                        {
                            RoleRequestId = (int)reader["RRF_ID"],
                            AccessLevelId = (int)reader["ACCESS_LEVEL_ID"],
                            AppId = (int)reader["APP_ID"],
                            IsSelected = (bool)reader["CHECKED_FLG"]
                        };
                        aalList.Add(ar);
                    }
                }
            }
            return aalList;
        }

        public List<string> GetAllRoleRequestEmpUsernames()
        {
            List<string> roleRequestEmpUsernamesList = new List<string>();

            String commandText = @"SELECT DISTINCT EMPLOYEE_USERNAME FROM ROLE_REQUEST ORDER BY EMPLOYEE_USERNAME ASC";


            using (SqlDataReader reader = SqlHelper.ExecuteReader(connectionString, commandText,
                CommandType.Text))
            {
                while (reader.Read())
                {
                    if (reader.HasRows)
                    {

                        roleRequestEmpUsernamesList.Add(reader["EMPLOYEE_USERNAME"].ConvertToString());
                    }
                }
            }
            return roleRequestEmpUsernamesList;
        }

        public int UpdateSelectedFlagForPrimRoles(int highestRRF, List<int> primRoleIds)
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
                string primRoleIdsListString = string.Empty;

                primRoleIdsListString = string.Concat("(", String.Join(",", primRoleIds), ")");
                com.CommandText = @"UPDATE PRIMARY_ROLE_DTL SET IS_CHECKED=1 WHERE RRF_ID='" + highestRRF + "' AND PRIMARY_ROLE_ID IN " + primRoleIdsListString;
                com.Connection = db;
                com.Transaction = tran;
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

        public int UpdateSelectedFlagForAppRole(int highestRRF, List<int> appRoleIds)
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
                string appRoleIdsListString = string.Empty;

                appRoleIdsListString = string.Concat("(", String.Join(",", appRoleIds), ")");
                com.CommandText = @"UPDATE APP_ROLES_MAPPER_DTL SET CHECKED_FLG=1 WHERE RRF_ID='" + highestRRF + "' AND ROLE_ID IN " + appRoleIdsListString;
                com.Connection = db;
                com.Transaction = tran;
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

        public int UpdateSelectedFlagForAccessLevelForAnApp(int highestRRF, int appId, int accessLevelId)
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
                com.CommandText = @"UPDATE ACCESS_LEVEL_MAPPER_DTL SET CHECKED_FLG=1 WHERE RRF_ID='" + highestRRF + "' AND APP_ID = " + appId + " AND ACCESS_LEVEL_ID=" + accessLevelId;
                com.Connection = db;
                com.Transaction = tran;
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