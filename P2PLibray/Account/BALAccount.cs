using P2PHelper;
using P2PLibray.Quality;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PLibray.Account
{
    public class BALAccount
    {
        MSSQL sql = new MSSQL();

        #region Login
        /// <summary>
        /// Attempts to log in a user by validating the provided email and password
        /// against the database.
        /// </summary>
        /// <param name="acc">The account object containing the email address and password for login.</param>
        /// <returns>
        /// An <see cref="Account"/> object with StaffCode and DepartmentId populated
        /// if the login is successful; otherwise, returns the same object with
        /// empty/zero values.
        /// </returns>
        public async Task<Account> Login(Account acc)
        {
            Dictionary<string,string> param = new Dictionary<string,string>();
            param.Add("@Flag","Login");
            param.Add("@Email",acc.EmailAddress);
            param.Add("@Password",acc.Password);

            SqlDataReader dr = await sql.ExecuteStoredProcedureReturnDataReader("AccountProcedure", param);
            if (dr.Read())
            {
                acc.StaffCode = dr.IsDBNull(dr.GetOrdinal("StaffCode")) ? string.Empty : dr.GetString(dr.GetOrdinal("StaffCode"));
                acc.DepartmentId = dr.IsDBNull(dr.GetOrdinal("DepartmentId")) ? 0 : dr.GetInt32(dr.GetOrdinal("DepartmentId"));
                acc.RoleId = dr.IsDBNull(dr.GetOrdinal("RoleId")) ? 0 : dr.GetInt32(dr.GetOrdinal("RoleId"));

            }

            return acc;
        }

        /// <summary>
        /// Checks whether an email exists in the system and retrieves the corresponding staff code.
        /// </summary>
        /// <param name="acc">The account object containing the email address to check.</param>
        /// <returns>The staff code associated with the email if found; otherwise, an empty string.</returns>
        public async Task<string> CheckEmail(Account acc)
        {
            string code = string.Empty;
            
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("@Flag", "CheckEmail");
            param.Add("@Email",acc.EmailAddress);

            SqlDataReader dr = await sql.ExecuteStoredProcedureReturnDataReader("AccountProcedure", param);
            if (dr.Read())
            {
                code = dr.IsDBNull(dr.GetOrdinal("StaffCode")) ? string.Empty : dr.GetString(dr.GetOrdinal("StaffCode"));
            }

            return code;
        }

        /// <summary>
        /// Changes the password of a staff member.
        /// </summary>
        /// <param name="acc">The account object containing StaffCode and new Password.</param>
        public async Task ChangePassword(Account acc)
        {
            Dictionary<string,string> param = new Dictionary<string, string>();
            param.Add("@Flag", "ChangePassword");
            param.Add("@Password", acc.Password);
            param.Add("@StaffCode", acc.StaffCode);
            await sql.ExecuteStoredProcedure("AccountProcedure", param);
        }

        /// <summary>
        /// Executes post-login operations by calling the "AccountProcedure" stored procedure.
        /// Passes the current date (in yyyy-MM-dd format) and the "AfterLogin" flag as parameters.
        /// This method is asynchronous and does not return a value.
        /// </summary>
        public async Task AfterLogin()
        {
            Dictionary<string, string> param = new Dictionary<string, string>
            {
                {"@Flag","AfterLogin" }, {"@Date",DateTime.Now.ToString("yyyy-MM-dd")}
            };
            await sql.ExecuteStoredProcedure("AccountProcedure", param);
        }
        #endregion

        #region User Profile
        /// <summary>
        /// Retrieves complete user profile details for a given staff member,
        /// including personal information, contact details, and assigned permissions.
        /// </summary>
        /// <param name="StaffCode">The unique staff code used to identify the user.</param>
        /// <returns>
        /// An <see cref="Account"/> object containing profile information such as
        /// name, department, role, contact details, addresses, and permissions.
        /// </returns>
        /// 
        public async Task<Account> UserProfileDetails(string StaffCode)
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("@Flag", "UserProfilePCM");
            param.Add("@StaffCode", StaffCode);

            var acc = new Account();

            using (SqlDataReader dr = await sql.ExecuteStoredProcedureReturnDataReader("AccountProcedure", param))
            {
                if (await dr.ReadAsync())
                {
                    acc.UserName = dr.IsDBNull(dr.GetOrdinal("FullName")) ? string.Empty : dr.GetString(dr.GetOrdinal("FullName"));
                    acc.StaffCode = dr.IsDBNull(dr.GetOrdinal("StaffCode")) ? string.Empty : dr.GetString(dr.GetOrdinal("StaffCode"));
                    acc.Department = dr.IsDBNull(dr.GetOrdinal("DepartmentName")) ? string.Empty : dr.GetString(dr.GetOrdinal("DepartmentName"));
                    acc.RoleName = dr.IsDBNull(dr.GetOrdinal("RoleName")) ? string.Empty : dr.GetString(dr.GetOrdinal("RoleName"));
                    acc.ProfilePhoto = dr.IsDBNull(dr.GetOrdinal("Location")) ? string.Empty : dr.GetString(dr.GetOrdinal("Location"));
                    acc.JoiningDate = dr.IsDBNull(dr.GetOrdinal("JoiningDate")) ? DateTime.MinValue : dr.GetDateTime(dr.GetOrdinal("JoiningDate"));
                    acc.Gender = dr.IsDBNull(dr.GetOrdinal("Gender")) ? string.Empty : dr.GetString(dr.GetOrdinal("Gender"));
                    acc.DateOfBirth = dr.IsDBNull(dr.GetOrdinal("DOB")) ? DateTime.MinValue : dr.GetDateTime(dr.GetOrdinal("DOB"));
                    acc.BloodGroup = dr.IsDBNull(dr.GetOrdinal("BloodGroup")) ? string.Empty : dr.GetString(dr.GetOrdinal("BloodGroup"));
                    acc.PhoneNumber = dr.IsDBNull(dr.GetOrdinal("ContactNo")) ? string.Empty : dr.GetInt64(dr.GetOrdinal("ContactNo")).ToString();
                    acc.AlternamteNumber = dr.IsDBNull(dr.GetOrdinal("AlternameNumber")) ? string.Empty : dr.GetInt64(dr.GetOrdinal("AlternameNumber")).ToString();
                    acc.EmailAddress = dr.IsDBNull(dr.GetOrdinal("EmailAddress")) ? string.Empty : dr.GetString(dr.GetOrdinal("EmailAddress"));
                    acc.MotherName = dr.IsDBNull(dr.GetOrdinal("MotherName")) ? string.Empty : dr.GetString(dr.GetOrdinal("MotherName"));
                    acc.SameLocation = dr.IsDBNull(dr.GetOrdinal("SameLocation")) ? false : dr.GetBoolean(dr.GetOrdinal("SameLocation"));
                    acc.LocalLocation = dr.IsDBNull(dr.GetOrdinal("LocalLocation")) ? string.Empty : dr.GetString(dr.GetOrdinal("LocalLocation"));
                    acc.LocalLandmark = dr.IsDBNull(dr.GetOrdinal("LocalLandmark")) ? string.Empty : dr.GetString(dr.GetOrdinal("LocalLandmark"));
                    acc.LocalPincode = dr.IsDBNull(dr.GetOrdinal("LocalPincode")) ? 0 : dr.GetInt32(dr.GetOrdinal("LocalPincode"));
                    acc.ParmanentLocation = dr.IsDBNull(dr.GetOrdinal("ParmanentLocation")) ? string.Empty : dr.GetString(dr.GetOrdinal("ParmanentLocation"));
                    acc.ParmanentLandmark = dr.IsDBNull(dr.GetOrdinal("ParmanentLandmark")) ? string.Empty : dr.GetString(dr.GetOrdinal("ParmanentLandmark"));
                    acc.ParmanentPincode = dr.IsDBNull(dr.GetOrdinal("ParmanentPincode")) ? 0 : dr.GetInt32(dr.GetOrdinal("ParmanentPincode"));
                    acc.CountryCode = dr.IsDBNull(dr.GetOrdinal("LocalCountryCode")) ? string.Empty : dr.GetString(dr.GetOrdinal("LocalCountryCode"));
                    acc.StateCode = dr.IsDBNull(dr.GetOrdinal("LocalStateCode")) ? string.Empty : dr.GetString(dr.GetOrdinal("LocalStateCode"));
                    acc.CityId = dr.IsDBNull(dr.GetOrdinal("LocalCityId")) ? 0 : dr.GetInt32(dr.GetOrdinal("LocalCityId"));
                    acc.ExtraCountryCode = dr.IsDBNull(dr.GetOrdinal("ParmanentCountryCode")) ? string.Empty : dr.GetString(dr.GetOrdinal("ParmanentCountryCode"));
                    acc.ExtraStateCode = dr.IsDBNull(dr.GetOrdinal("ParmanentStateCode")) ? string.Empty : dr.GetString(dr.GetOrdinal("ParmanentStateCode"));
                    acc.ExtraCityId = dr.IsDBNull(dr.GetOrdinal("ParmanentCityId")) ? 0 : dr.GetInt32(dr.GetOrdinal("ParmanentCityId"));
                }
                if (await dr.NextResultAsync())
                {
                    while (await dr.ReadAsync())
                    {
                        acc.PermissionsData.Add(new Permissions
                        {
                            PermissionName = dr.IsDBNull(dr.GetOrdinal("PermissionName")) ? string.Empty : dr.GetString(dr.GetOrdinal("PermissionName")),
                            HasPermission = dr.IsDBNull(dr.GetOrdinal("HasPermission")) ? 0 : dr.GetInt32(dr.GetOrdinal("HasPermission"))
                        });
                    }
                }
            }
            return acc;
        }

        /// <summary>
        /// Retrieves basic user profile data (summary) for a staff member.
        /// </summary>
        /// <param name="StaffCode">The staff code of the user.</param>
        /// <returns>An Account object with basic user profile data.</returns>
        public async Task<Account> UserProfileData(string StaffCode)
        {
            Account acc = new Account();

            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("@Flag", "UserDataPCM");
            param.Add("@StaffCode", StaffCode);

            SqlDataReader dr = await sql.ExecuteStoredProcedureReturnDataReader("AccountProcedure", param);
            if (dr.Read())
            {
                acc.UserName = dr.IsDBNull(dr.GetOrdinal("FullName")) ? string.Empty : dr.GetString(dr.GetOrdinal("FullName"));
                acc.Department = dr.IsDBNull(dr.GetOrdinal("DepartmentName")) ? string.Empty : dr.GetString(dr.GetOrdinal("DepartmentName"));
                acc.RoleName = dr.IsDBNull(dr.GetOrdinal("RoleName")) ? string.Empty : dr.GetString(dr.GetOrdinal("RoleName"));
                acc.ProfilePhoto = dr.IsDBNull(dr.GetOrdinal("Location")) ? string.Empty : dr.GetString(dr.GetOrdinal("Location"));
            }

            return acc;
        }

        /// <summary>
        /// Updates the profile information of a staff member.
        /// </summary>
        /// <param name="acc">The account object containing updated profile information.</param>
        public async Task UpdateUserProfile(Account acc)
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("@Flag", "UpdateUserProfilePCM");
            param.Add("@AlternateNumber", acc.AlternamteNumber);
            param.Add("@StaffCode", acc.StaffCode);
            await sql.ExecuteStoredProcedure("AccountProcedure", param);
        }

        /// <summary>
        /// Retrieves the complete list of permissions assigned to a staff member,
        /// including both permission types and their names.
        /// </summary>
        /// <param name="StaffCode">The unique staff code of the user.</param>
        /// <returns>
        /// A list of <see cref="Permissions"/> objects containing both the type
        /// of permission and its corresponding name.
        /// </returns>
        public async Task<List<Permissions>> GetAllPermissions(string StaffCode)
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("@Flag", "GetAllPermissionsPcm");
            param.Add("@StaffCode", StaffCode);

            var acc = new List<Permissions>();

            using (SqlDataReader dr = await sql.ExecuteStoredProcedureReturnDataReader("AccountProcedure", param))
            {
                while (await dr.ReadAsync())
                {
                    acc.Add(new Permissions
                    {
                        PermissionType = dr.IsDBNull(dr.GetOrdinal("Permissions")) ? string.Empty : dr.GetString(dr.GetOrdinal("Permissions")),
                        PermissionName = dr.IsDBNull(dr.GetOrdinal("PermissionName")) ? string.Empty : dr.GetString(dr.GetOrdinal("PermissionName"))
                    });
                }
            }
            return acc;
        }
        #endregion

        #region Notification
        /// <summary>
        /// Retrieves all notifications (both read and unread) for the specified staff member.
        /// </summary>
        /// <param name="staffCode">The unique staff code of the user.</param>
        /// <returns>
        /// A list of <see cref="NotificationProperty"/> objects containing notification details.
        /// </returns>
        public async Task<List<NotificationProperty>> GetAllNotifications(string staffCode)
        {
            var result = new List<NotificationProperty>();

            var parameters = new Dictionary<string, string>
            {
                { "@Flag","GetAllNotifications" },
                { "@StaffCode", staffCode }
            };

            DataSet ds = await sql.ExecuteStoredProcedureReturnDS("AccountProcedure", parameters);

            if (ds != null && ds.Tables.Count > 0)
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    result.Add(new NotificationProperty
                    {
                        NotificationId = Convert.ToInt32(row["NotificationId"]),
                        StaffCode = row["StaffCode"].ToString(),
                        NotificationMessage = row["NotificationMessage"].ToString(),
                        IsRead = Convert.ToBoolean(row["IsRead"])
                    });
                }
            }

            return result;
        }

        /// <summary>
        /// Retrieves only unread notifications for the specified staff member.
        /// </summary>
        /// <param name="staffCode">The unique staff code of the user.</param>
        /// <returns>
        /// A list of <see cref="NotificationProperty"/> objects containing unread notification details.
        /// </returns>
        public async Task<List<NotificationProperty>> GetUnreadNotifications(string staffCode)
        {
            var result = new List<NotificationProperty>();

            var parameters = new Dictionary<string, string>
            {
                { "@Flag","GetUnreadNotifications" },
                { "@StaffCode", staffCode }
            };

            DataSet ds = await sql.ExecuteStoredProcedureReturnDS("AccountProcedure", parameters);

            if (ds != null && ds.Tables.Count > 0)
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    result.Add(new NotificationProperty
                    {
                        NotificationId = Convert.ToInt32(row["NotificationId"]),
                        StaffCode = row["StaffCode"].ToString(),
                        NotificationMessage = row["NotificationMessage"].ToString(),
                        IsRead = Convert.ToBoolean(row["IsRead"])
                    });
                }
            }

            return result;
        }

        /// <summary>
        /// Marks a single notification as read for the specified staff member.
        /// </summary>
        /// <param name="id">The unique identifier of the notification to mark as read.</param>
        /// <param name="staffCode">The staff code of the user.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task MarkAsRead(int id, string staffCode)
        {
            var parameters = new Dictionary<string, string>
            {
                { "@Flag","MarkSingleRead" },
                { "@NotificationId", id.ToString() },
                { "@StaffCode", staffCode }
            };

            await sql.ExecuteStoredProcedure("AccountProcedure", parameters);
        }

        /// <summary>
        /// Marks all notifications as read for the specified staff member.
        /// </summary>
        /// <param name="staffCode">The staff code of the user.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task MarkAllAsRead(string staffCode)
        {
            var parameters = new Dictionary<string, string>
            {
                { "@Flag","MarkAllRead" },
                { "@StaffCode", staffCode }
            };

            await sql.ExecuteStoredProcedure("AccountProcedure", parameters);
        }
        #endregion

        #region Calendar
        /// <summary>
        /// Retrieves the list of read-only permissions assigned to a staff member.
        /// </summary>
        /// <param name="StaffCode">The unique staff code of the user.</param>
        /// <returns>
        /// A list of <see cref="Permissions"/> objects containing the names of
        /// read permissions granted to the user.
        /// </returns>
        public async Task<List<Permissions>> GetReadPermissions(string StaffCode)
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("@Flag", "GetReadPermissionsPCM");
            param.Add("@StaffCode", StaffCode);

            var acc = new List<Permissions>();

            using (SqlDataReader dr = await sql.ExecuteStoredProcedureReturnDataReader("AccountProcedure", param))
            {
                while (await dr.ReadAsync())
                {
                    acc.Add(new Permissions
                    {
                        PermissionName = dr.IsDBNull(dr.GetOrdinal("Permissions")) ? string.Empty : dr.GetString(dr.GetOrdinal("Permissions"))
                    });
                }
            }
            return acc;
        }

        #region PR
        /// <summary>
        /// Gets a list of all Purchase Requisitions (PRs) for calendar display.
        /// </summary>
        /// <returns>List of <see cref="Account"/> objects with PR code, added by, and added date.</returns>
        public async Task<List<Account>> PRListPCM()
        {
            List<Account> acc = new List<Account>();

            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("@Flag", "ShowPRsOnCalendar");
            using (SqlDataReader dr = await sql.ExecuteStoredProcedureReturnDataReader("AccountProcedure", param))
            {
                while (await dr.ReadAsync())
                {
                    acc.Add(new Account
                    {
                        IdCode = dr.IsDBNull(dr.GetOrdinal("PRCode")) ? string.Empty : dr.GetString(dr.GetOrdinal("PRCode")),
                        AddedBy = dr.IsDBNull(dr.GetOrdinal("EmployeeName")) ? string.Empty : dr.GetString(dr.GetOrdinal("EmployeeName")),
                        AddedDate = dr.IsDBNull(dr.GetOrdinal("AddedDate")) ? DateTime.MinValue : dr.GetDateTime(dr.GetOrdinal("AddedDate"))
                    });
                }
            }

            return acc;
        }

        /// <summary>
        /// Gets detailed information for a specific Purchase Requisition (PR),
        /// including metadata and associated items.
        /// </summary>
        /// <param name="code">The PR code.</param>
        /// <returns><see cref="CalendarEventData"/> with full PR details.</returns>
        public async Task<CalendarEventData> PRDetails(string code)
        {
            var prDetails = new CalendarEventData();

            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("@Flag", "PRDetailsPCM");
            param.Add("@Code",code);
            using (SqlDataReader dr = await sql.ExecuteStoredProcedureReturnDataReader("AccountProcedure", param))
            {
                if (await dr.ReadAsync())
                {
                    prDetails.PRCode = dr["PRCode"].ToString(); 
                    prDetails.RequiredDate = dr["RequiredDate"] != DBNull.Value ? Convert.ToDateTime(dr["RequiredDate"]) : (DateTime?)null; 
                    prDetails.StatusName = dr["StatusName"].ToString(); 
                    prDetails.Description = dr["Description"].ToString(); 
                    prDetails.AddedBy = dr["AddedBy"].ToString(); 
                    prDetails.AddedDate = dr["AddedDate"] != DBNull.Value ? Convert.ToDateTime(dr["AddedDate"]) : (DateTime?)null; 
                    prDetails.ApprovedBy = dr["ApprovedBy"]?.ToString(); 
                    prDetails.ApprovedDate = dr["ApprovedDate"] != DBNull.Value ? Convert.ToDateTime(dr["ApprovedDate"]) : (DateTime?)null; 
                    prDetails.PriorityName = dr["PriorityName"]?.ToString();
                }

                if (await dr.NextResultAsync())
                {
                    while (await dr.ReadAsync())
                    {
                        prDetails.Items.Add(new ItemData
                        {
                            PRCode = dr["PRCode"].ToString(),
                            PRItemCode = dr["PRItemCode"]?.ToString(),
                            ItemCode = dr["ItemCode"]?.ToString(),
                            ItemName = dr["ItemName"]?.ToString(),
                            RequiredQuantity = dr["RequiredQuantity"] != DBNull.Value ? Convert.ToInt32(dr["RequiredQuantity"]) : 0
                        });
                    }
                }
            }

            return prDetails;
        }

        /// <summary>
        /// Builds calendar events for all Purchase Requisitions (PRs).
        /// </summary>
        /// <returns>List of event objects containing PR info for calendar display.</returns>
        public async Task<List<object>> GetPurchaseRequisitionEventsAsync()
        {
            var events = new List<object>();

            var PRList = await PRListPCM();

            foreach (var pr in PRList)
            {
                var prDetails = await PRDetails(pr.IdCode);
                events.Add(new
                {
                    id = pr.IdCode,
                    title = $"Purchase Requisition Is Added By {pr.AddedBy}",
                    start = pr.AddedDate.ToString("yyyy/MM/ddTHH:mm:ss"),
                    color = "#007bff",
                    extendedProps = new
                    {
                        module = "PurchaseRequisition",
                        PRCode = prDetails.PRCode,
                        RequiredDate = prDetails.RequiredDate?.ToString("dd-MM-yyyy").Replace("-", "/"),
                        StatusName = prDetails.StatusName,
                        Description = prDetails.Description,
                        AddedBy = prDetails.AddedBy,
                        AddedDate = prDetails.AddedDate?.ToString("dd-MM-yyyy").Replace("-", "/"),
                        ApprovedBy = prDetails.ApprovedBy,
                        ApprovedDate = prDetails.ApprovedDate?.ToString("dd-MM-yyyy").Replace("-", "/"),
                        PriorityName = prDetails.PriorityName,
                        Items = prDetails.Items
                    }
                });
            }

            return events;
        }
        #endregion

        #region RFQ
        /// <summary>
        /// Gets a list of all RFQs (Request for Quotation) for calendar display.
        /// </summary>
        /// <returns>List of <see cref="Account"/> objects with RFQ code, added by, and expected date.</returns>
        public async Task<List<Account>> RFQListPCM()
        {
            List<Account> acc = new List<Account>();

            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("@Flag", "ShowRFQsOnCalendar");
            using (SqlDataReader dr = await sql.ExecuteStoredProcedureReturnDataReader("AccountProcedure", param))
            {
                while (await dr.ReadAsync())
                {
                    acc.Add(new Account
                    {
                        IdCode = dr.IsDBNull(dr.GetOrdinal("RFQCode")) ? string.Empty : dr.GetString(dr.GetOrdinal("RFQCode")),
                        AddedBy = dr.IsDBNull(dr.GetOrdinal("EmployeeName")) ? string.Empty : dr.GetString(dr.GetOrdinal("EmployeeName")),
                        AddedDate = dr.IsDBNull(dr.GetOrdinal("AddedDate")) ? DateTime.MinValue : dr.GetDateTime(dr.GetOrdinal("AddedDate")),
                        EndDate = dr.IsDBNull(dr.GetOrdinal("ExpectedDate")) ? DateTime.Today : dr.GetDateTime(dr.GetOrdinal("ExpectedDate"))
                    });
                }
            }

            return acc;
        }

        /// <summary>
        /// Gets detailed information for a specific RFQ, including metadata and related items.
        /// </summary>
        /// <param name="code">The RFQ code.</param>
        /// <returns><see cref="CalendarEventData"/> containing RFQ details.</returns>
        public async Task<CalendarEventData> RFQDetails(string code)
        {
            var rfqDetails = new CalendarEventData();

            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("@Flag", "RFQDetailsPCM");
            param.Add("@Code", code);
            using (SqlDataReader dr = await sql.ExecuteStoredProcedureReturnDataReader("AccountProcedure", param))
            {
                if (await dr.ReadAsync())
                {
                    rfqDetails.RFQCode = dr["RFQCode"].ToString();
                    rfqDetails.PRCode = dr["PRCode"].ToString();
                    rfqDetails.AddedBy = dr["AddedBy"].ToString();
                    rfqDetails.AddedDate = dr["AddedDate"] != DBNull.Value ? Convert.ToDateTime(dr["AddedDate"]) : (DateTime?)null;
                    rfqDetails.ExpectedDate = dr["ExpectedDate"] != DBNull.Value ? Convert.ToDateTime(dr["ExpectedDate"]) : (DateTime?)null;
                    rfqDetails.Description = dr["Description"].ToString();
                    rfqDetails.AccountantName = dr["AccountantName"].ToString();
                    rfqDetails.AccountantEmail = dr["EmailAddress"].ToString();
                    rfqDetails.DeliveryAddress = dr["DeliveryAddress"].ToString();
                }

                if (await dr.NextResultAsync())
                {
                    while (await dr.ReadAsync())
                    {
                        rfqDetails.Items.Add(new ItemData
                        {
                            RFQCode = dr["RFQCode"]?.ToString(),
                            PRItemCode = dr["PRItemCode"]?.ToString(),
                            ItemCode = dr["ItemCode"]?.ToString(),
                            ItemName = dr["ItemName"]?.ToString(),
                            RequiredQuantity = dr["RequiredQuantity"] != DBNull.Value ? Convert.ToInt32(dr["RequiredQuantity"]) : 0
                        });
                    }
                }
            }

            return rfqDetails;
        }

        public async Task<List<object>> GetRFQEventsAsync()
        {
            var events = new List<object>();
            var RFQList = await RFQListPCM();

            foreach (var rfq in RFQList)
            {
                var rfqDetails = await RFQDetails(rfq.IdCode);
                events.Add(new
                {
                    id = rfq.IdCode,
                    title = $"Request For Quotation Is Added By {rfq.AddedBy}",
                    start = rfq.AddedDate.ToString("yyyy-MM-dd"),
                    end = rfq.EndDate.ToString("yyyy-MM-dd"),
                    color = "#17a2b8",
                    extendedProps = new
                    {
                        module = "RequestForQuotation",
                        rfqDetails.RFQCode,
                        rfqDetails.PRCode,
                        ExpectedDate = rfqDetails.ExpectedDate?.ToString("dd-MM-yyyy").Replace("-", "/"),
                        rfqDetails.Description,
                        rfqDetails.AddedBy,
                        AddedDate = rfqDetails.AddedDate?.ToString("dd-MM-yyyy").Replace("-", "/"),
                        rfqDetails.AccountantName,
                        rfqDetails.AccountantEmail,
                        rfqDetails.DeliveryAddress,
                        rfqDetails.Items
                    }
                });
            }
            return events;
        }
        #endregion

        #region RQ
        /// <summary>
        /// Gets a list of all Register Quotation (RQ) entries for calendar display.
        /// </summary>
        /// <returns>List of <see cref="Account"/> objects with entry count, date, and creator.</returns>
        public async Task<List<Account>> RQListPCM()
        {
            List<Account> acc = new List<Account>();

            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("@Flag", "ShowRQsOnCalendar");
            using (SqlDataReader dr = await sql.ExecuteStoredProcedureReturnDataReader("AccountProcedure", param))
            {
                while (await dr.ReadAsync())
                {
                    acc.Add(new Account
                    {
                        Count = dr.IsDBNull(dr.GetOrdinal("EntryCount")) ? 0 : dr.GetInt32(dr.GetOrdinal("EntryCount")),
                        AddedDate = dr.IsDBNull(dr.GetOrdinal("AddedDate")) ? DateTime.MinValue : dr.GetDateTime(dr.GetOrdinal("AddedDate")),
                        AddedBy = dr.IsDBNull(dr.GetOrdinal("EmployeeName")) ? string.Empty : dr.GetString(dr.GetOrdinal("EmployeeName"))
                    });
                }
            }

            return acc;
        }

        /// <summary>
        /// Gets detailed RQ (Register Quotation) data for a specific date.
        /// </summary>
        /// <param name="date">The date for which RQ details are fetched.</param>
        /// <returns>List of <see cref="CalendarEventData"/> objects containing RQ details.</returns>
        public async Task<List<CalendarEventData>> RQDetails(string date)
        {
            var RqDetails = new List<CalendarEventData>();

            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("@Flag", "RQDetailsPCM");
            param.Add("@Date", date);

            using (SqlDataReader dr = await sql.ExecuteStoredProcedureReturnDataReader("AccountProcedure", param))
            {
                while (await dr.ReadAsync())
                {
                    RqDetails.Add(new CalendarEventData
                    {
                        RegisterQuotationCode = dr["RegisterQuotationCode"].ToString(),
                        RFQCode = dr["RFQCode"].ToString(),
                        VendorName = dr["VenderName"].ToString(),
                        DeliveryDate = dr.IsDBNull(dr.GetOrdinal("DeliveryDate")) ? DateTime.MinValue : dr.GetDateTime(dr.GetOrdinal("DeliveryDate")),
                        StatusName = dr["StatusName"].ToString(),
                        AddedBy = dr["AddedBy"].ToString(),
                        ApprovedBy = dr["ApprovedBy"].ToString(),
                        AddedDate = dr.IsDBNull(dr.GetOrdinal("AddedDate")) ? DateTime.MinValue : dr.GetDateTime(dr.GetOrdinal("AddedDate")),
                        ApprovedDate = dr.IsDBNull(dr.GetOrdinal("ApprovedDate")) ? DateTime.MinValue : dr.GetDateTime(dr.GetOrdinal("ApprovedDate")),
                        ShippingCharges = dr.IsDBNull(dr.GetOrdinal("ShippingCharges")) ? 0 : dr.GetDecimal(dr.GetOrdinal("ShippingCharges")),
                    });
                }
            }

            return RqDetails;
        }

        public async Task<List<object>> GetRegisterQuotationEventsAsync()
        {
            var events = new List<object>();
            var RQList = await RQListPCM();
            foreach (var rq in RQList)
            {
                var rqDetails = await RQDetails(rq.AddedDate.ToString("yyyy-MM-dd"));

                var items = rqDetails.Select(i => new {
                    i.RegisterQuotationCode,
                    i.RFQCode,
                    i.VendorName,
                    i.StatusName,
                    i.AddedBy,
                    DeliveryDate = i.DeliveryDate.HasValue ? i.DeliveryDate.Value.ToString("dd-MM-yyyy").Replace("-", "/") : "",
                    AddedDate = i.AddedDate.HasValue ? i.AddedDate.Value.ToString("dd-MM-yyyy").Replace("-", "/") : "",
                    i.ApprovedBy,
                    ApprovedDate = i.ApprovedDate.HasValue ? i.ApprovedDate.Value.ToString("dd-MM-yyyy").Replace("-", "/") : "",
                    i.ShippingCharges
                });

                events.Add(new
                {
                    id = $"RQ-{rq.AddedDate:yyyyMMdd}",
                    title = $"{rq.Count} Quotation{(rq.Count != 1 ? "s" : "")} {(rq.Count != 1 ? "Are" : "Is")} Registerd By {rq.AddedBy}",
                    start = rq.AddedDate.ToString("yyyy-MM-dd"),
                    color = "#6f42c1",

                    extendedProps = new
                    {
                        module = "RegisterQuotation",

                        Items = items
                    }
                });
            }
            return events;
        }
        #endregion

        #region PO
        /// <summary>
        /// Retrieves a list of all Purchase Orders (POs) to display on the calendar.
        /// </summary>
        /// <returns>List of <see cref="Account"/> objects with PO code, creator, and date.</returns>
        public async Task<List<Account>> POListPCM()
        {
            List<Account> acc = new List<Account>();

            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("@Flag", "ShowPOsOnCalendar");
            using (SqlDataReader dr = await sql.ExecuteStoredProcedureReturnDataReader("AccountProcedure", param))
            {
                while (await dr.ReadAsync())
                {
                    acc.Add(new Account
                    {
                        IdCode = dr.IsDBNull(dr.GetOrdinal("POCode")) ? string.Empty : dr.GetString(dr.GetOrdinal("POCode")),
                        AddedBy = dr.IsDBNull(dr.GetOrdinal("EmployeeName")) ? string.Empty : dr.GetString(dr.GetOrdinal("EmployeeName")),
                        AddedDate = dr.IsDBNull(dr.GetOrdinal("AddedDate")) ? DateTime.MinValue : dr.GetDateTime(dr.GetOrdinal("AddedDate"))
                    });
                }
            }

            return acc;
        }

        /// <summary>
        /// Retrieves detailed information about a specific Purchase Order (PO),
        /// including items and terms/conditions.
        /// </summary>
        /// <param name="code">The PO code.</param>
        /// <returns>A <see cref="CalendarEventData"/> object with PO details.</returns>
        public async Task<CalendarEventData> GetPODetails(string code)
        {
            var podetails = new CalendarEventData();

            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("@Flag", "PODetailsPCM");
            param.Add("@Code", code);

            using (SqlDataReader dr = await sql.ExecuteStoredProcedureReturnDataReader("AccountProcedure", param))
            {
                if (await dr.ReadAsync())
                {
                    podetails.POCode = dr["POCode"]?.ToString();
                    podetails.StatusName = dr["StatusName"]?.ToString();
                    podetails.AddedDate = dr["AddedDate"] != DBNull.Value ? Convert.ToDateTime(dr["AddedDate"]) : (DateTime?)null;
                    podetails.ApprovedDate = dr["ApprovedRejectedDate"] != DBNull.Value ? Convert.ToDateTime(dr["ApprovedRejectedDate"]) : (DateTime?)null;
                    podetails.TotalAmount = dr["TotalAmount"] != DBNull.Value ? Convert.ToDecimal(dr["TotalAmount"]) : 0;
                    podetails.BillingAddress = dr["BillingAddress"]?.ToString();
                    podetails.VendorName = dr["VenderName"]?.ToString();
                    podetails.AddedBy = dr["AddedBy"]?.ToString();
                    podetails.ApprovedBy = dr["ApprovedBy"]?.ToString();
                    podetails.ShippingCharges = dr["ShippingCharges"] != DBNull.Value ? Convert.ToDecimal(dr["ShippingCharges"]) : 0;
                    podetails.AccountantName = dr["AccountantName"]?.ToString();
                }

                if (await dr.NextResultAsync())
                {
                    while (await dr.ReadAsync())
                    {
                        podetails.Items.Add(new ItemData
                        {
                            POCode = dr["POCode"]?.ToString(),
                            POItemCode = dr["POItemCode"]?.ToString(),
                            RQItemCode = dr["RQItemCode"]?.ToString(),
                            ItemCode = dr["ItemCode"]?.ToString(),
                            ItemName = dr["ItemName"]?.ToString(),
                            CostPerUnit = dr["CostPerUnit"] != DBNull.Value ? Convert.ToDecimal(dr["CostPerUnit"]) : (decimal?)null,
                            Discount = dr.IsDBNull(dr.GetOrdinal("Discount")) ? 0 : dr.GetInt32(dr.GetOrdinal("Discount")),
                            Quantity = dr["Quantity"] != DBNull.Value ? Convert.ToInt64(dr["Quantity"]) : (long?)null,
                            StatusName = dr["StatusName"].ToString()
                        });
                    }
                }

                if (await dr.NextResultAsync())
                {
                    while (await dr.ReadAsync())
                    {
                        var term = dr["TermConditionName"]?.ToString();
                        if (!string.IsNullOrEmpty(term))
                            podetails.TermConditions.Add(term);
                    }
                }
            }

            return podetails;
        }

        /// <summary>
        /// Builds calendar events for all Purchase Orders (POs) for visualization on the calendar.
        /// </summary>
        /// <returns>List of event objects representing PO entries.</returns>
        public async Task<List<object>> GetPurchaseOrderEventsAsync()
        {
            var events = new List<object>();
            var POList = await POListPCM();

            foreach (var po in POList)
            {
                var PODetails = await GetPODetails(po.IdCode);
                events.Add(new
                {
                    id = po.IdCode,
                    title = $"Purchase Order Is Added By {po.AddedBy}",
                    start = po.AddedDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                    color = "#fd7e14",
                    extendedProps = new
                    {
                        module = "PurchaseOrder",
                        PODetails.POCode,
                        PODetails.StatusName,
                        AddedDate = PODetails.AddedDate?.ToString("dd-MM-yyyy").Replace("-", "/"),
                        ApprovedDate = PODetails.ApprovedDate?.ToString("dd-MM-yyyy").Replace("-", "/"),
                        PODetails.TotalAmount,
                        PODetails.BillingAddress,
                        PODetails.VendorName,
                        PODetails.AddedBy,
                        PODetails.ApprovedBy,
                        PODetails.AccountantName,
                        PODetails.ShippingCharges,
                        PODetails.Items,
                        TermConditions = PODetails.TermConditions ?? new List<string>()
                    }
                });
            }
            return events;
        }
        #endregion

        #region GRN
        /// <summary>
        /// Retrieves a list of all GRNs (Goods Receipt Notes) for calendar display.
        /// </summary>
        /// <returns>List of <see cref="Account"/> objects containing GRN codes, creator, and dates.</returns>
        public async Task<List<Account>> GRNListPCM()
        {
            List<Account> acc = new List<Account>();

            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("@Flag", "ShowGRNsOnCalendar");
            using (SqlDataReader dr = await sql.ExecuteStoredProcedureReturnDataReader("AccountProcedure", param))
            {
                while (await dr.ReadAsync())
                {
                    acc.Add(new Account
                    {
                        IdCode = dr.IsDBNull(dr.GetOrdinal("GRNCode")) ? string.Empty : dr.GetString(dr.GetOrdinal("GRNCode")),
                        AddedBy = dr.IsDBNull(dr.GetOrdinal("EmployeeName")) ? string.Empty : dr.GetString(dr.GetOrdinal("EmployeeName")),
                        AddedDate = dr.IsDBNull(dr.GetOrdinal("AddedDate")) ? DateTime.MinValue : dr.GetDateTime(dr.GetOrdinal("AddedDate"))
                    });
                }
            }

            return acc;
        }

        /// <summary>
        /// Retrieves detailed information about a specific GRN, including received items.
        /// </summary>
        /// <param name="code">The GRN code.</param>
        /// <returns>A <see cref="CalendarEventData"/> object with GRN details and items.</returns>
        public async Task<CalendarEventData> GRNDetails(string code)
        {
            var grnDetails = new CalendarEventData();

            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("@Flag", "GRNDetailsPCM");
            param.Add("@Code", code);

            using (SqlDataReader dr = await sql.ExecuteStoredProcedureReturnDataReader("AccountProcedure", param))
            {
                if (await dr.ReadAsync())
                {
                    grnDetails.POCode = dr["POCode"].ToString();
                    grnDetails.GRNCode = dr["GRNCode"].ToString();
                    grnDetails.PODate = dr.IsDBNull(dr.GetOrdinal("PODate")) ? DateTime.MinValue : dr.GetDateTime(dr.GetOrdinal("PODate"));
                    grnDetails.GRNDate = dr.IsDBNull(dr.GetOrdinal("GRNDate")) ? DateTime.MinValue : dr.GetDateTime(dr.GetOrdinal("GRNDate"));
                    grnDetails.InvoiceDate = dr.IsDBNull(dr.GetOrdinal("InvoiceDate")) ? DateTime.MinValue : dr.GetDateTime(dr.GetOrdinal("InvoiceDate"));
                    grnDetails.VendorName = dr["VenderName"].ToString();
                    grnDetails.InvoiceCode = dr["InvoiceNo"].ToString();
                    grnDetails.CompanyAddress = dr["CompanyAddress"].ToString();
                    grnDetails.BillingAddress = dr["BillingAddress"].ToString();
                    grnDetails.TotalAmount = dr["TotalAmount"] != DBNull.Value ? Convert.ToDecimal(dr["TotalAmount"]) : 0;
                    grnDetails.ShippingCharges = dr["ShippingCharges"] != DBNull.Value ? Convert.ToDecimal(dr["ShippingCharges"]) : 0;
                }

                if (await dr.NextResultAsync())
                {
                    while (await dr.ReadAsync())
                    {
                        grnDetails.Items.Add(new ItemData
                        {
                            GRNCode = dr.IsDBNull(dr.GetOrdinal("GRNCode")) ? null : dr.GetString(dr.GetOrdinal("GRNCode")),
                            GRNItemCode = dr.IsDBNull(dr.GetOrdinal("GRNItemCode")) ? null : dr.GetString(dr.GetOrdinal("GRNItemCode")),
                            ItemCode = dr.IsDBNull(dr.GetOrdinal("ItemCode")) ? null : dr.GetString(dr.GetOrdinal("ItemCode")),
                            ItemName = dr.IsDBNull(dr.GetOrdinal("ItemName")) ? "-" : dr.GetString(dr.GetOrdinal("ItemName")),
                            Quantity = dr.IsDBNull(dr.GetOrdinal("UnitQuantity")) ? 0 : dr.GetInt64(dr.GetOrdinal("UnitQuantity")),
                            CostPerUnit = dr.IsDBNull(dr.GetOrdinal("CostPerUnit")) ? 0 : dr.GetDecimal(dr.GetOrdinal("CostPerUnit")),
                            Discount = dr.IsDBNull(dr.GetOrdinal("Discount")) ? 0 : dr.GetInt32(dr.GetOrdinal("Discount")),
                            TaxRate = dr.IsDBNull(dr.GetOrdinal("TaxRate")) ? "-" : dr.GetString(dr.GetOrdinal("TaxRate")),
                            FinalAmount = dr.IsDBNull(dr.GetOrdinal("FinalAmount")) ? 0 : dr.GetDecimal(dr.GetOrdinal("FinalAmount"))
                        });
                    }
                }
            }

            return grnDetails;
        }

        /// <summary>
        /// Builds calendar events for all GRNs for visualization on the calendar.
        /// </summary>
        /// <returns>List of event objects representing GRN entries.</returns>
        public async Task<List<object>> GetGRNEventsAsync()
        {
            var events = new List<object>();
            var GRNList = await GRNListPCM();

            foreach (var grn in GRNList)
            {
                var grnDetails = await GRNDetails(grn.IdCode);
                var items = grnDetails.Items.Select(g => new
                {
                    g.GRNCode,
                    g.GRNItemCode,
                    g.ItemCode,
                    g.ItemName,
                    g.Quantity,
                    g.CostPerUnit,
                    g.Discount,
                    g.TaxRate,
                    g.FinalAmount
                });

                events.Add(new
                {
                    id = grn.IdCode,
                    title = $"GRN Is Added By {grn.AddedBy}",
                    start = grn.AddedDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                    color = "#28a745",
                    extendedProps = new
                    {
                        module = "GRNInfo",
                        grnDetails.POCode,
                        grnDetails.GRNCode,
                        PODate = grnDetails.PODate?.ToString("dd/MM/yyyy"),
                        GRNDate = grnDetails.GRNDate?.ToString("dd-MM-yyyy").Replace("-", "/"),
                        InvoiceDate = grnDetails.InvoiceDate?.ToString("dd-MM-yyyy").Replace("-", "/"),
                        grnDetails.VendorName,
                        grnDetails.InvoiceCode,
                        grnDetails.CompanyAddress,
                        grnDetails.BillingAddress,
                        grnDetails.StatusName,
                        grnDetails.TotalAmount,
                        grnDetails.ShippingCharges,
                        Items = items
                    }
                });
            }
            return events;
        }
        #endregion

        #region GR
        /// <summary>
        /// Retrieves a list of all Goods Returns (GRs) for calendar display.
        /// </summary>
        /// <returns>List of <see cref="Account"/> objects containing GR codes, creator, and dates.</returns>
        public async Task<List<Account>> GRListPCM()
        {
            List<Account> acc = new List<Account>();

            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("@Flag", "ShowGRsOnCalendar");
            using (SqlDataReader dr = await sql.ExecuteStoredProcedureReturnDataReader("AccountProcedure", param))
            {
                while (await dr.ReadAsync())
                {
                    acc.Add(new Account
                    {
                        IdCode = dr.IsDBNull(dr.GetOrdinal("GoodReturnCode")) ? string.Empty : dr.GetString(dr.GetOrdinal("GoodReturnCode")),
                        AddedBy = dr.IsDBNull(dr.GetOrdinal("EmployeeName")) ? string.Empty : dr.GetString(dr.GetOrdinal("EmployeeName")),
                        AddedDate = dr.IsDBNull(dr.GetOrdinal("AddedDate")) ? DateTime.MinValue : dr.GetDateTime(dr.GetOrdinal("AddedDate"))
                    });
                }
            }

            return acc;
        }

        /// <summary>
        /// Retrieves detailed information for a specific Goods Return, including returned items.
        /// </summary>
        /// <param name="code">The Goods Return code.</param>
        /// <returns>A <see cref="CalendarEventData"/> object with GR details and items.</returns>
        public async Task<CalendarEventData> GRDetails(string code)
        {
            var grDetails = new CalendarEventData();

            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("@Flag", "GRDetailsPCM");
            param.Add("@Code", code);

            using (SqlDataReader dr = await sql.ExecuteStoredProcedureReturnDataReader("AccountProcedure", param))
            {
                if (await dr.ReadAsync())
                {
                    grDetails.GoodsReturnCode = dr["GoodReturnCode"].ToString();
                    grDetails.GRNCode = dr["GRNCode"].ToString();
                    grDetails.TransporterName = dr["TransporterName"].ToString();
                    grDetails.TransportContactNo = dr["TransportContactNo"].ToString();
                    grDetails.VehicleNo = dr["VehicleNo"].ToString();
                    grDetails.VehicleType = dr["VehicleType"].ToString();
                    grDetails.Reason = dr["ReasonOfRejection"].ToString();
                    grDetails.AddedBy = dr["AddedBy"].ToString();
                    grDetails.AddedDate = dr.IsDBNull(dr.GetOrdinal("AddedDate")) ? DateTime.MinValue : dr.GetDateTime(dr.GetOrdinal("AddedDate"));
                    grDetails.StatusName = dr["StatusName"].ToString();

                    if (await dr.NextResultAsync())
                    {
                        while (await dr.ReadAsync())
                        {
                            grDetails.Items.Add(new ItemData
                            {
                                GRItemCode = dr["GRItemCode"].ToString(),
                                ItemCode = dr["ItemCode"].ToString(),
                                ItemName = dr["ItemName"].ToString(),
                                Reason = dr["Reason"].ToString()
                            });
                        }
                    }
                }
            }

            return grDetails;
        }

        /// <summary>
        /// Builds calendar events for all Goods Returns (GRs) for visualization on the calendar.
        /// </summary>
        /// <returns>List of event objects representing GR entries.</returns>
        public async Task<List<object>> GetGoodsReturnEventsAsync()
        {
            var events = new List<object>();
            var goodsReturnList = await GRListPCM();

            foreach (var gr in goodsReturnList)
            {
                var grDetails = await GRDetails(gr.IdCode);
                events.Add(new
                {
                    id = gr.IdCode,
                    title = $"Goods Return Entry Is Added By {gr.AddedBy}",
                    start = gr.AddedDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                    color = "#ffc107",
                    extendedProps = new
                    {
                        module = "GoodsReturnInfo",
                        grDetails.GoodsReturnCode,
                        grDetails.GRNCode,
                        grDetails.TransporterName,
                        grDetails.TransportContactNo,
                        grDetails.VehicleNo,
                        grDetails.VehicleType,
                        grDetails.Reason,
                        grDetails.AddedBy,
                        AddedDate = grDetails.AddedDate?.ToString("dd-MM-yyyy").Replace("-", "/"),
                        grDetails.StatusName,
                        grDetails.Items
                    }
                });
            }
            return events;
        }
        #endregion

        #region QC
        /// <summary>
        /// Retrieves a list of Quality Checks (QCs) aggregated by date and status for calendar display.
        /// </summary>
        /// <returns>List of <see cref="Account"/> objects containing QC counts, date, status, and creator.</returns>
        public async Task<List<Account>> QCListPCM()
        {
            List<Account> acc = new List<Account>();

            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("@Flag", "ShowQCsOnCalendar");
            using (SqlDataReader dr = await sql.ExecuteStoredProcedureReturnDataReader("AccountProcedure", param))
            {
                while (await dr.ReadAsync())
                {
                    acc.Add(new Account
                    {
                        Count = dr.IsDBNull(dr.GetOrdinal("EntryCount")) ? 0 : dr.GetInt32(dr.GetOrdinal("EntryCount")),
                        AddedDate = dr.IsDBNull(dr.GetOrdinal("AddedDate")) ? DateTime.MinValue : dr.GetDateTime(dr.GetOrdinal("AddedDate")),
                        Status = dr.IsDBNull(dr.GetOrdinal("StatusName")) ? string.Empty : dr.GetString(dr.GetOrdinal("StatusName")),
                        AddedBy = dr.IsDBNull(dr.GetOrdinal("EmployeeName")) ? string.Empty : dr.GetString(dr.GetOrdinal("EmployeeName"))
                    });
                }
            }

            return acc;
        }

        /// <summary>
        /// Retrieves detailed QC (Quality Check) information for a specific date and status.
        /// </summary>
        /// <param name="date">The date for which QC details are retrieved.</param>
        /// <param name="status">The QC status (e.g., Passed, Failed).</param>
        /// <returns>List of <see cref="CalendarEventData"/> objects containing QC details and related items.</returns>
        public async Task<List<CalendarEventData>> QCDetails(string date,string status)
        {
            var qcdetails = new List<CalendarEventData>();

            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("@Flag", "QCDetailsPCM");
            param.Add("@Date", date);
            param.Add("@StatusName", status);

            using (SqlDataReader dr = await sql.ExecuteStoredProcedureReturnDataReader("AccountProcedure", param))
            {
                while (await dr.ReadAsync())
                {
                    try
                    {
                        qcdetails.Add(new CalendarEventData
                        {
                            QualityCheckCode = dr["QualityCheckCode"]?.ToString(),
                            StatusName = dr["StatusName"]?.ToString(),
                            GRNItemsCode = dr["GRNItemCode"]?.ToString(),
                            ItemCode = dr["ItemCode"]?.ToString(),
                            ItemName = dr["ItemName"]?.ToString(),
                            QCAddedBy = dr["QCAddedBy"]?.ToString(),
                            QCFailedAddedBy = dr["QCFailedAddedBy"]?.ToString(),
                            Reason = dr["Reason"]?.ToString(),
                            InspectionFrequency = dr["InspectionFrequency"] == DBNull.Value ? 0 : Convert.ToInt32(dr["InspectionFrequency"]),
                            SampleQualityChecked = dr["SampleQualityChecked"] == DBNull.Value ? 0 : Convert.ToInt64(dr["SampleQualityChecked"]),
                            SampleTestFailed = dr["SampleTestFailed"] == DBNull.Value ? 0 : Convert.ToInt64(dr["SampleTestFailed"]),
                            QCAddedDate = dr["QCAddedDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["QCAddedDate"]),
                            QCFailedDate = dr["QCFailedDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["QCFailedDate"]),
                            Quantity = dr["Quantity"] == DBNull.Value ? 0 : Convert.ToInt64(dr["Quantity"]),
                        });

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error reading QC record: " + ex.Message);
                        throw;
                    }

                }
            }

            return qcdetails;
        }

        /// <summary>
        /// Builds calendar events for all Quality Checks (QCs) for visualization on the calendar.
        /// </summary>
        /// <returns>List of event objects representing QC entries with items.</returns>
        public async Task<List<object>> GetQualityCheckEventsAsync()
        {
            var events = new List<object>();
            var QCList = await QCListPCM();

            foreach (var qc in QCList)
            {
                var qcDetails = await QCDetails(qc.AddedDate.ToString("yyyy-MM-dd"), qc.Status);
                var items = qcDetails.Select(i => new
                {
                    i.QualityCheckCode,
                    i.StatusName,
                    i.GRNItemsCode,
                    i.ItemCode,
                    i.ItemName,
                    i.Quantity,
                    i.InspectionFrequency,
                    i.SampleQualityChecked,
                    i.SampleTestFailed,
                    i.QCAddedBy,
                    QCAddedDate = i.QCAddedDate?.ToString("dd-MM-yyyy").Replace("-", "/"),
                    i.QCFailedAddedBy,
                    QCFailedDate = i.QCFailedDate?.ToString("dd-MM-yyyy").Replace("-", "/"),
                    i.Reason
                });

                events.Add(new
                {
                    id = $"QC-{qc.AddedDate:yyyyMMdd}",
                    title = $"{qc.Count} Items Has {(qc.Status == "Confirmed" ? "Passed" : "Failed")} Quality Check",
                    start = qc.AddedDate.ToString("yyyy-MM-dd"),
                    color = "#dc3545",
                    extendedProps = new
                    {
                        module = "QualityCheckInfo",
                        Items = items
                    }
                });
            }
            return events;
        }
        #endregion

        #region ISR
        /// <summary>
        /// Retrieves a list of Item Stock Refill (ISR) requests aggregated by date and staff for calendar display.
        /// </summary>
        /// <returns>List of <see cref="Account"/> objects containing ISR counts, date, and requester.</returns>
        public async Task<List<Account>> ISRListPCM()
        {
            List<Account> acc = new List<Account>();

            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("@Flag", "ShowISROnCalendar");
            using (SqlDataReader dr = await sql.ExecuteStoredProcedureReturnDataReader("AccountProcedure", param))
            {
                while (await dr.ReadAsync())
                {
                    acc.Add(new Account
                    {
                        Count = dr.IsDBNull(dr.GetOrdinal("ItemCount")) ? 0 : dr.GetInt32(dr.GetOrdinal("ItemCount")),
                        AddedDate = dr.IsDBNull(dr.GetOrdinal("AddedDate")) ? DateTime.MinValue : dr.GetDateTime(dr.GetOrdinal("AddedDate")),
                        AddedBy = dr.IsDBNull(dr.GetOrdinal("EmployeeName")) ? string.Empty : dr.GetString(dr.GetOrdinal("EmployeeName"))
                    });
                }
            }

            return acc;
        }

        /// <summary>
        /// Retrieves detailed ISR information for a specific date and staff member.
        /// </summary>
        /// <param name="date">The date for which ISR details are retrieved.</param>
        /// <param name="code">The staff code associated with the ISR.</param>
        /// <returns>List of <see cref="CalendarEventData"/> objects containing ISR items and metadata.</returns>
        public async Task<List<CalendarEventData>> ISRDetails(string date,string code)
        {
            var isrDetails = new List<CalendarEventData>();

            Dictionary<string,string> param = new Dictionary<string, string>()
            {
                { "@Flag","ISRDetailsPCM"},
                { "@Date",date },
                { "@StaffCode", code }
            };

            using (SqlDataReader dr = await sql.ExecuteStoredProcedureReturnDataReader("AccountProcedure", param))
            {
                while (await dr.ReadAsync())
                {
                    try
                    {
                        isrDetails.Add(new CalendarEventData
                        {
                            ItemCode = dr["ItemCode"].ToString(),
                            ItemName = dr["ItemName"].ToString(),
                            Quantity = dr["Quantity"] == DBNull.Value ? 0 : Convert.ToInt64(dr["Quantity"]),
                            RequiredDate = dr["RequiredDate"] != DBNull.Value ? Convert.ToDateTime(dr["RequiredDate"]) : (DateTime?)null,
                            StatusName = dr["Status"].ToString(),
                            AddedBy = dr["EmployeeName"].ToString(),
                            AddedDate = dr["AddedDate"] != DBNull.Value ? Convert.ToDateTime(dr["AddedDate"]) : (DateTime?)null,
                        });
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error reading ISR record: " + ex.Message);
                        throw;
                    }

                }
            }

            return isrDetails;
        }

        /// <summary>
        /// Builds calendar events for all Item Stock Refill (ISR) requests for visualization on the calendar.
        /// </summary>
        /// <returns>List of event objects representing ISR entries with items.</returns>
        public async Task<List<object>> GetItemStockRefillEventsAsync()
        {
            var events = new List<object>();

            var ISRList = await ISRListPCM();

            foreach (var isr in ISRList)
            {
                var isrDetails = await ISRDetails(isr.AddedDate.ToString("yyyy-MM-dd"), isr.Code);
                var items = isrDetails.Select(i => new
                {
                    i.ItemCode,
                    i.ItemName,
                    i.Quantity,
                    RequiredDate = i.RequiredDate?.ToString("dd-MM-yyyy").Replace("-", "/"),
                    i.StatusName,
                    i.AddedBy,
                    AddedDate = i.AddedDate?.ToString("dd-MM-yyyy").Replace("-", "/")
                });
                events.Add(new
                {
                    id = $"RQ-{isr.AddedDate:yyyyMMdd}",
                    title = $"{isr.Count} Item Stock Refill Request{(isr.Count != 1 ? "s":"")} {(isr.Count != 1 ? "Are" : "Is")} Registerd By {isr.AddedBy}",
                    start = isr.AddedDate.ToString("yyyy-MM-dd"),
                    color = "#6610f2",
                    extendedProps = new
                    {
                        module = "ItemStockRefill",
                        Items = items
                    }
                });
            }

            return events;
        }
        #endregion

        #region JIT
        /// <summary>
        /// Retrieves a list of Just-In-Time (JIT) item requests aggregated by date and staff for calendar display.
        /// </summary>
        /// <returns>List of <see cref="Account"/> objects containing JIT counts, date, and requester.</returns>
        public async Task<List<Account>> JITListPCM()
        {
            List<Account> acc = new List<Account>();

            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("@Flag", "ShowJITOnCalendar");
            using (SqlDataReader dr = await sql.ExecuteStoredProcedureReturnDataReader("AccountProcedure", param))
            {
                while (await dr.ReadAsync())
                {
                    acc.Add(new Account
                    {
                        Count = dr.IsDBNull(dr.GetOrdinal("ItemCount")) ? 0 : dr.GetInt32(dr.GetOrdinal("ItemCount")),
                        AddedDate = dr.IsDBNull(dr.GetOrdinal("AddedDate")) ? DateTime.MinValue : dr.GetDateTime(dr.GetOrdinal("AddedDate")),
                        AddedBy = dr.IsDBNull(dr.GetOrdinal("EmployeeName")) ? string.Empty : dr.GetString(dr.GetOrdinal("EmployeeName")),
                        Code = dr.IsDBNull(dr.GetOrdinal("AddedBy")) ? string.Empty : dr.GetString(dr.GetOrdinal("AddedBy")),
                    });
                }
            }

            return acc;
        }

        /// <summary>
        /// Retrieves detailed JIT information for a specific date and staff member.
        /// </summary>
        /// <param name="date">The date for which JIT details are retrieved.</param>
        /// <param name="code">The staff code associated with the JIT request.</param>
        /// <returns>List of <see cref="CalendarEventData"/> objects containing JIT items and metadata.</returns>
        public async Task<List<CalendarEventData>> JITDetails(string date, string code)
        {
            var isrDetails = new List<CalendarEventData>();

            Dictionary<string, string> param = new Dictionary<string, string>()
            {
                { "@Flag","JITDetailsPCM"},
                { "@Date",date },
                { "@StaffCode", code }
            };

            using (SqlDataReader dr = await sql.ExecuteStoredProcedureReturnDataReader("AccountProcedure", param))
            {
                while (await dr.ReadAsync())
                {
                    try
                    {
                        isrDetails.Add(new CalendarEventData
                        {
                            ItemCode = dr["ItemCode"].ToString(),
                            ItemName = dr["ItemName"].ToString(),
                            Quantity = dr["Quantity"] == DBNull.Value ? 0 : Convert.ToInt64(dr["Quantity"]),
                            RequiredDate = dr["RequiredDate"] != DBNull.Value ? Convert.ToDateTime(dr["RequiredDate"]) : (DateTime?)null,
                            StatusName = dr["Status"].ToString(),
                            AddedBy = dr["EmployeeName"].ToString(),
                            AddedDate = dr["AddedDate"] != DBNull.Value ? Convert.ToDateTime(dr["AddedDate"]) : (DateTime?)null,
                        });
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error reading JIT record: " + ex.Message);
                        throw;
                    }

                }
            }

            return isrDetails;
        }

        /// <summary>
        /// Builds calendar events for all Just-In-Time (JIT) requests for visualization.
        /// </summary>
        /// <returns>List of event objects representing JIT entries with items.</returns>
        public async Task<List<object>> GetJustInTimeEventsAsync()
        {
            var events = new List<object>();

            var JITList = await JITListPCM();

            foreach (var jit in JITList)
            {
                var jitDetails = await JITDetails(jit.AddedDate.ToString("yyyy-MM-dd"), jit.Code);
                var items = jitDetails.Select(i => new
                {
                    i.ItemCode,
                    i.ItemName,
                    i.Quantity,
                    RequiredDate = i.RequiredDate?.ToString("dd-MM-yyyy").Replace("-", "/"),
                    i.StatusName,
                    i.AddedBy,
                    AddedDate = i.AddedDate?.ToString("dd-MM-yyyy").Replace("-", "/")
                });
                events.Add(new
                {
                    id = $"RQ-{jit.AddedDate:yyyyMMdd}",
                    title = $"{jit.Count} Just In Time Request{(jit.Count != 1 ? "s" : "")} {(jit.Count != 1 ? "Are" : "Is")} Registerd By {jit.AddedBy}",
                    start = jit.AddedDate.ToString("yyyy-MM-dd"),
                    color = "#0d6efd",
                    extendedProps = new
                    {
                        module = "JustInTime",
                        Items = items
                    }
                });
            }

            return events;
        }
        #endregion

        #region MRP
        /// <summary>Fetches all MRP records for calendar display.</summary>
        /// <returns>List of <see cref="Account"/> representing MRP entries.</returns>
        public async Task<List<Account>> MRPListPCM()
        {
            List<Account> acc = new List<Account>();

            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("@Flag", "ShowMRPOnCalendar");
            using (SqlDataReader dr = await sql.ExecuteStoredProcedureReturnDataReader("AccountProcedure", param))
            {
                while (await dr.ReadAsync())
                {
                    acc.Add(new Account
                    {
                        IdCode = dr.IsDBNull(dr.GetOrdinal("MaterialReqPlanningCode")) ? string.Empty : dr.GetString(dr.GetOrdinal("MaterialReqPlanningCode")),
                        AddedBy = dr.IsDBNull(dr.GetOrdinal("EmployeeName")) ? string.Empty : dr.GetString(dr.GetOrdinal("EmployeeName")),
                        AddedDate = dr.IsDBNull(dr.GetOrdinal("AddedDate")) ? DateTime.MinValue : dr.GetDateTime(dr.GetOrdinal("AddedDate"))
                    });
                }
            }

            return acc;
        }

        /// <summary>Fetches detailed info for a specific MRP record, including items.</summary>
        /// <param name="code">MRP Code.</param>
        /// <returns><see cref="CalendarEventData"/> with header and item-level details.</returns>
        public async Task<CalendarEventData> MRPDetails(string code)
        {
            var mrpDetails = new CalendarEventData();

            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("@Flag", "MRPDetailsPCM");
            param.Add("@Code", code);

            using (SqlDataReader dr = await sql.ExecuteStoredProcedureReturnDataReader("AccountProcedure", param))
            {
                if (await dr.ReadAsync())
                {
                    try
                    {
                        mrpDetails.MaterialReqPlanningCode = dr["MaterialReqPlanningCode"].ToString();
                        mrpDetails.PlanName = dr["PlanName"].ToString();
                        mrpDetails.PlanYear = dr["Year"] != DBNull.Value ? Convert.ToInt32(dr["Year"]) : 0;
                        mrpDetails.FromDate = dr["FromDate"] != DBNull.Value ? Convert.ToDateTime(dr["FromDate"]) : (DateTime?)null;
                        mrpDetails.ToDate = dr["ToDate"] != DBNull.Value ? Convert.ToDateTime(dr["ToDate"]) : (DateTime?)null;
                        mrpDetails.StatusName = dr["StatusName"].ToString();
                        mrpDetails.AddedBy = dr["AddedBy"].ToString();
                        mrpDetails.AddedDate = dr["AddedDate"] != DBNull.Value ? Convert.ToDateTime(dr["AddedDate"]) : (DateTime?)null;
                        mrpDetails.ApprovedBy = dr["ApprovedBy"].ToString();
                        mrpDetails.ApprovedDate = dr["ApprovedDate"] != DBNull.Value ? Convert.ToDateTime(dr["ApprovedDate"]) : (DateTime?)null;
                        mrpDetails.Reason = dr["Reason"].ToString();

                        if (await dr.NextResultAsync())
                        {
                            while (await dr.ReadAsync())
                            {
                                mrpDetails.Items.Add(new ItemData
                                {
                                    IssueItemsId = dr["IssueItemsId"] != DBNull.Value ? Convert.ToInt32(dr["IssueItemsId"]) : 0,
                                    ItemCode = dr["ItemCode"].ToString(),
                                    ItemName = dr["ItemName"].ToString(),
                                    Quantity = dr["Quantity"] != DBNull.Value ? Convert.ToInt64(dr["Quantity"]) : (long?)null,
                                });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error reading MRP record: " + ex.Message);
                        throw;
                    }
                }
            }

            return mrpDetails;
        }

        /// <summary>Builds calendar events for all MRP entries.</summary>
        /// <returns>List of anonymous objects representing MRP events for calendar UI.</returns>
        public async Task<List<object>> GetMaterialReqPlanningEventsAsync()
        {
            var events = new List<object>();
            var mrpList = await MRPListPCM();

            foreach (var mrp in mrpList)
            {
                var mrpDetails = await MRPDetails(mrp.IdCode);
                var items = mrpDetails.Items.Select(i => new
                {
                    i.IssueItemsId,
                    i.ItemCode,
                    i.ItemName,
                    Quantity = i.Quantity ?? 0
                });

                events.Add(new
                {
                    id = mrp.IdCode,
                    title = $"Material Requirenment Planning Entry Is Added By {mrp.AddedBy}",
                    start = mrp.AddedDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                    color = "#20c997",
                    extendedProps = new
                    {
                        module = "MaterialReqPlanningInfo",
                        mrpDetails.MaterialReqPlanningCode,
                        mrpDetails.PlanName,
                        mrpDetails.PlanYear,
                        FromDate = mrpDetails.FromDate?.ToString("dd-MM-yyyy").Replace("-", "/"),
                        ToDate = mrpDetails.ToDate?.ToString("dd-MM-yyyy").Replace("-", "/"),
                        mrpDetails.StatusName,
                        mrpDetails.AddedBy,
                        AddedDate = mrpDetails.AddedDate?.ToString("dd-MM-yyyy").Replace("-", "/"),
                        mrpDetails.ApprovedBy,
                        ApprovedDate = mrpDetails.ApprovedDate?.ToString("dd-MM-yyyy").Replace("-", "/"),
                        mrpDetails.Reason,
                        Items = items
                    }
                });
            }
            return events;
        }
        #endregion

        #endregion
    }
}
