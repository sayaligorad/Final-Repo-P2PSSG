using Newtonsoft.Json;
using P2PLibray.Account;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Security;

namespace P2PERP.Controllers
{
    public class AccountController : Controller
    {
        BALAccount bal = new BALAccount();

        /// <summary>
        /// Displays the login page and clears any existing session.
        /// </summary>
        /// <returns>The login view.</returns>
        [Route("Account/Login")]
        [HttpGet]
        public ActionResult MainLogin()
        {
            Session.Clear();
            FormsAuthentication.SignOut();

            return View();
        }

        /// <summary>
        /// Logs out the current user, clears the session, and prevents cached access.
        /// </summary>
        /// <returns>Redirects to the login page.</returns>
        public ActionResult Logout()
        {
            Session.Clear();
            FormsAuthentication.SignOut();

            Response.Cache.SetExpires(DateTime.UtcNow.AddSeconds(-1));
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();

            return RedirectToAction("MainLogin", "Account");
        }

        /// <summary>
        /// Authenticates a user with the provided account credentials.
        /// </summary>
        /// <param name="acc">The account object containing email and password.</param>
        /// <returns>
        /// A JSON result indicating success or failure. 
        /// On success, includes the department ID.
        /// </returns>
        [Route("Account/Login")]
        [HttpPost]
        public async Task<ActionResult> MainLogin(Account acc)
        {
            Session["StaffCode"] = null;
            Account acc1 = await bal.Login(acc);

            if (acc1?.StaffCode == string.Empty || acc1?.StaffCode == null)
            {
                return Json(new { success = false, message = "Invalid login credentials" });
            }

            Session["StaffCode"] = acc1.StaffCode;
            Session["DepartmentId"] = acc1.DepartmentId;
            Session["RoleId"] = acc1.RoleId;

            await bal.AfterLogin();

            return Json(new { success = true, departmentId = acc1.DepartmentId });
        }

        /// <summary>
        /// Displays the forgot password page.
        /// </summary>
        /// <returns>The forgot password view.</returns>
        [HttpGet]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        /// <summary>
        /// Handles a user's request to reset their password.
        /// Verifies if the provided email exists in the system, 
        /// and if valid, stores relevant session information for the password reset process.
        /// </summary>
        /// <param name="acc">An <see cref="Account"/> object containing the user's email address.</param>
        /// <returns>
        /// Returns a JSON result:
        /// - If the email is valid: { success = true } and stores the staff code and email in session for further steps.
        /// - If the email is invalid: { success = false, message = "Invalid Email" }.
        /// </returns>
        [HttpPost]
        public async Task<ActionResult> ForgotPassword(Account acc)
        {
            string str = await bal.CheckEmail(acc);

            if (string.IsNullOrEmpty(str))
            {
                return Json(new { success = false, message = "Invalid Email" });
            }

            Session["StaffCodeForForgotPassword"] = str;
            Session["ForgetPasswordEmail"] = acc.EmailAddress;
            Session["VerificationCode"] = acc.Code;

            return Json(new { success = true });
        }

        /// <summary>
        /// Checks whether the current user session contains a valid StaffCode.
        /// </summary>
        /// <returns>
        /// Returns a JSON object with:
        /// - success = true, if Session["StaffCode"] exists.
        /// - success = false, if Session["StaffCode"] is null.
        /// </returns>
        [HttpGet]
        public JsonResult CheckSession()
        {
            bool isValid = Session["StaffCode"] != null;
            return Json(new { success = isValid }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Displays the verification code entry page.
        /// </summary>
        /// <returns>The verification code view.</returns>
        [HttpGet]
        public ActionResult VerifyCode()
        {
            return View();
        }

        /// <summary>
        /// Verifies the reset code entered by the user.
        /// </summary>
        /// <param name="acc">The account object containing the entered code.</param>
        /// <returns>
        /// A JSON result indicating whether the verification code matches.
        /// </returns>
        [HttpPost]
        public ActionResult VerifyCode(Account acc)
        {
            if (acc.Code != Session["VerificationCode"].ToString())
                return Json(new { success = false, message = "Code Dose Not Match" });
            return Json(new { success = true });
        }

        /// <summary>
        /// Displays the change password page.
        /// </summary>
        /// <returns>The change password view.</returns>
        [HttpGet]
        public ActionResult ChangePassword()
        {
            return View();
        }

        /// <summary>
        /// Updates the password for the staff account after verification.
        /// </summary>
        /// <param name="acc">The account object containing new and confirm password.</param>
        /// <returns>A JSON result indicating success or failure.</returns>
        [HttpPost]
        public async Task<ActionResult> ChangePassword(Account acc)
        {
            if (acc.Password != acc.ConfirmPassword)
                return Json(new { success = false, message = "Passwords Dose Not Match" });

            acc.StaffCode = Session["StaffCodeForForgotPassword"].ToString();

            await bal.ChangePassword(acc);

            return Json(new { success = true });
        }

        /// <summary>
        /// Retrieves user information for the currently logged-in staff.
        /// </summary>
        /// <returns>
        /// A JSON result containing user name, department, role, and profile photo,
        /// or an error if the session has expired.
        /// </returns>
        [HttpGet]
        public async Task<ActionResult> GetUserInfo()
        {
            var staffCode = Session["StaffCode"]?.ToString();
            if (string.IsNullOrEmpty(staffCode))
                return Json(new { success = false, message = "Session expired" }, JsonRequestBehavior.AllowGet);

            Account acc = await bal.UserProfileData(staffCode);

            return Json(new
            {
                success = true,
                userName = acc.UserName,
                department = acc.Department,
                role = acc.RoleName,
                profilePhoto = acc.ProfilePhoto
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Updates the user profile with the provided account information.
        /// </summary>
        /// <param name="acc">The account object containing updated profile info.</param>
        /// <returns>A JSON result indicating success or failure.</returns>
        [HttpPost]
        public async Task<ActionResult> UpdateUserProfile(Account acc)
        {
            if (String.IsNullOrEmpty(acc.AlternamteNumber))
                return Json(new { success = false, message = "Alternate Number Is Null" });

            await bal.UpdateUserProfile(acc);

            return Json(new { success = true });
        }

        /// <summary>
        /// Sends an email with optional CC, BCC, and attachments to the specified recipients.
        /// </summary>
        /// <param name="email">The Email object containing recipients, subject, body, and attachments.</param>
        /// <returns>
        /// A JSON result indicating the outcome:
        /// - success = true, message = "Email sent successfully." if the email was sent successfully.
        /// - success = false, message = error details if sending failed.
        /// </returns>
        [Route("Account/SendEmail")]
        [HttpPost]
        [ValidateInput(false)]
        public JsonResult SendEmail()
        {
            try
            {
                var request = HttpContext.Request;

                // Read and deserialize email JSON manually
                var emailJson = request.Form["email"];
                var email = JsonConvert.DeserializeObject<Email>(emailJson);

                using (var smtpClient = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtpClient.Credentials = new NetworkCredential(
                        WebConfigurationManager.AppSettings["MainEmail"],
                        WebConfigurationManager.AppSettings["AppPassword"]
                    );
                    smtpClient.EnableSsl = true;

                    using (var mail = new MailMessage())
                    {
                        mail.From = new MailAddress(WebConfigurationManager.AppSettings["MainEmail"]);
                        mail.Subject = email.Subject;
                        mail.Body = email.Body;
                        mail.IsBodyHtml = email.IsBodyHtml;

                        email.ToEmails?.ForEach(x => mail.To.Add(x));
                        email.CcEmails?.ForEach(x => mail.CC.Add(x));
                        email.BccEmails?.ForEach(x => mail.Bcc.Add(x));

                        // Attach uploaded files directly (no saving)
                        for (int i = 0; i < request.Files.Count; i++)
                        {
                            var file = request.Files[i];
                            if (file != null && file.ContentLength > 0)
                            {
                                var attachment = new Attachment(file.InputStream, file.FileName);
                                mail.Attachments.Add(attachment);
                            }
                        }

                        smtpClient.Send(mail);
                    }
                }

                return Json(new { success = true, message = "Email sent successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Retrieves all permissions assigned to the currently logged-in staff.
        /// </summary>
        /// <returns>
        /// A JSON result containing permission type and name,
        /// or an error if the session has expired.
        /// </returns>
        public async Task<ActionResult> GetReadPermissions()
        {
            var staffCode = Session["StaffCode"]?.ToString();
            if (string.IsNullOrEmpty(staffCode))
                return Json(new { success = false, message = "Session expired" }, JsonRequestBehavior.AllowGet);

            var permissions = await bal.GetAllPermissions(staffCode);

            var allPermission = permissions.Select(p => new { p.PermissionType, p.PermissionName }).ToList();

            return Json(new { success = true, permissions = allPermission }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Retrieves country details from the CountryStateCity API.
        /// </summary>
        /// <param name="id">
        /// The ISO2 country code.  
        /// If null or empty, all countries are returned;  
        /// otherwise, details of the specified country are returned.
        /// </param>
        /// <returns>
        /// A JSON result containing either a list of <see cref="CountryDto"/> objects (all countries)  
        /// or a single <see cref="CountryDto"/> object (one country).
        /// </returns>
        [HttpGet]
        public async Task<JsonResult> GetCountries(string id = null)
        {
            var code = id;
            var apiKey = WebConfigurationManager.AppSettings["X-CSCAPI-KEY"];
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new Exception("API Key not found in Web.config!");
            }

            using (var client = new HttpClient())
            {
                string url = string.IsNullOrEmpty(code)
                    ? "https://api.countrystatecity.in/v1/countries"
                    : $"https://api.countrystatecity.in/v1/countries/{code}";

                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(url)
                };
                request.Headers.Add("X-CSCAPI-KEY", apiKey);

                using (var response = await client.SendAsync(request))
                {
                    response.EnsureSuccessStatusCode();
                    var body = await response.Content.ReadAsStringAsync();

                    if (string.IsNullOrEmpty(code))
                    {
                        // All countries → array
                        var allCountries = JsonConvert.DeserializeObject<List<CountryDto>>(body);
                        return Json(allCountries, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        // Single country → object
                        var country = JsonConvert.DeserializeObject<CountryDto>(body);
                        return Json(country, JsonRequestBehavior.AllowGet);
                    }
                }
            }
        }

        /// <summary>
        /// Retrieves state details for a given country from the CountryStateCity API.
        /// </summary>
        /// <param name="CountryCode">The ISO2 country code.</param>
        /// <param name="StateCode">
        /// (Optional) The ISO2 state code.  
        /// If provided, the result is filtered to the matching state only.
        /// </param>
        /// <returns>
        /// A JSON result containing a list of <see cref="StateDto"/> objects,  
        /// filtered by state code if specified.
        /// </returns>
        [HttpGet]
        public async Task<JsonResult> GetStates(string CountryCode, string StateCode = null)
        {
            var apiKey = WebConfigurationManager.AppSettings["X-CSCAPI-KEY"];
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new Exception("API Key not found in Web.config!");
            }

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("X-CSCAPI-KEY", apiKey);

                // Correct API endpoint per country
                string url = $"https://api.countrystatecity.in/v1/countries/{CountryCode}/states";

                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var body = await response.Content.ReadAsStringAsync();
                var states = JsonConvert.DeserializeObject<List<StateDto>>(body);

                // If StateCode provided, filter
                if (!string.IsNullOrEmpty(StateCode))
                {
                    states = states
                        .Where(s => string.Equals(s.Iso2, StateCode, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }

                return Json(states, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Retrieves city details for a given country and state from the CountryStateCity API.
        /// </summary>
        /// <param name="countryCode">The ISO2 country code.</param>
        /// <param name="stateCode">The ISO2 state code.</param>
        /// <param name="CityId">
        /// (Optional) The numeric city ID.  
        /// If greater than 0, returns details of a single city including mapped country and state names;  
        /// otherwise, returns all cities in the state.
        /// </param>
        /// <returns>
        /// A JSON result containing either a list of <see cref="CityDto"/> objects (all cities in the state)  
        /// or a single <see cref="CityDto"/> object enriched with country and state names (one city).
        /// </returns>
        [HttpGet]
        public async Task<JsonResult> GetCities(string countryCode, string stateCode, int CityId = 0)
        {
            var apiKey = WebConfigurationManager.AppSettings["X-CSCAPI-KEY"];
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new Exception("API Key not found in Web.config!");
            }

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("X-CSCAPI-KEY", apiKey);

                // Cities endpoint
                var url = $"https://api.countrystatecity.in/v1/countries/{countryCode}/states/{stateCode}/cities";
                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var body = await response.Content.ReadAsStringAsync();
                var cities = JsonConvert.DeserializeObject<List<CityDto>>(body);

                if (CityId > 0)
                {
                    var city = cities.FirstOrDefault(c => c.Id == CityId);
                    if (city != null)
                    {
                        // still use ISO2 code here
                        var countryUrl = $"https://api.countrystatecity.in/v1/countries/{countryCode}";
                        var countryRes = await client.GetStringAsync(countryUrl);
                        var country = JsonConvert.DeserializeObject<CountryDto>(countryRes);

                        var stateUrl = $"https://api.countrystatecity.in/v1/countries/{countryCode}/states/{stateCode}";
                        var stateRes = await client.GetStringAsync(stateUrl);
                        var state = JsonConvert.DeserializeObject<StateDto>(stateRes);

                        // Map codes → names
                        city.CountryName = country?.Name;
                        city.StateName = state?.Name;
                    }

                    return Json(city, JsonRequestBehavior.AllowGet);
                }

                return Json(cities, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Retrieves calendar events for the logged-in staff member based on their permissions.
        /// </summary>
        /// <remarks>
        /// - Verifies if the session has a valid staff code.  
        /// - Gets user permissions from the business layer.  
        /// - For each permission, loads the corresponding event data (PR, RFQ, Quotation, PO, GRN, Goods Return, QC).  
        /// - Events are returned in a JSON format ready to bind with calendar components like FullCalendar.
        /// </remarks>
        /// <returns>
        /// A JSON result containing a list of calendar events.  
        /// Each event includes:
        /// <list type="bullet">
        ///   <item><description><c>id</c> – Unique event identifier</description></item>
        ///   <item><description><c>title</c> – Event title</description></item>
        ///   <item><description><c>start</c>/<c>end</c> – Event dates</description></item>
        ///   <item><description><c>className</c>, <c>color</c>, <c>textColor</c> – Event styling</description></item>
        ///   <item><description><c>extendedProps</c> – Extra details depending on the module</description></item>
        /// </list>
        /// If the session has expired, returns <c>{ success = false, message = "Session expired" }</c>.
        /// </returns>
        public async Task<JsonResult> GetEvents()
        {
            var events = new List<object>();

            // Get logged-in staff code from session
            var staffCode = Session["StaffCode"]?.ToString();
            if (string.IsNullOrEmpty(staffCode))
                return Json(new { success = false, message = "Session expired" }, JsonRequestBehavior.AllowGet);

            // Fetch user permissions
            var permissions = await bal.GetReadPermissions(staffCode);
            var permissionNames = permissions.Select(p => p.PermissionName).ToList();

            bool hasPR = permissionNames.Contains("PurchaseRequisition");
            bool hasPO = permissionNames.Contains("PurchaseOrder");
            bool hasSP = permissionNames.Contains("StockPlanning");

            // --- Combined logic ---
            if (hasSP)
            {
                // Always load these if user has StockPlanning
                var isrEvents = await bal.GetItemStockRefillEventsAsync();
                var mrpEvents = await bal.GetMaterialReqPlanningEventsAsync();
                var jitEvents = await bal.GetJustInTimeEventsAsync();

                // Filter based on PR/PO presence
                if (hasPR && hasPO)
                {
                    // All three → keep all
                    events.AddRange(isrEvents);
                    events.AddRange(mrpEvents);
                    events.AddRange(jitEvents);
                }
                else if (hasPR && !hasPO)
                {
                    // PR + SP → Refile + Planning
                    events.AddRange(isrEvents);
                    events.AddRange(mrpEvents);
                }
                else if (hasPO && !hasPR)
                {
                    // PO + SP → JIT only
                    events.AddRange(jitEvents);
                }
                else
                {
                    // Only SP (no PR, no PO) → All three
                    events.AddRange(isrEvents);
                    events.AddRange(mrpEvents);
                    events.AddRange(jitEvents);
                }
            }

            // --- Main permission-based events ---
            foreach (var perm in permissions)
            {
                switch (perm.PermissionName)
                {
                    case "PurchaseRequisition":
                        events.AddRange(await bal.GetPurchaseRequisitionEventsAsync());
                        break;

                    case "RequestForQuotation":
                        events.AddRange(await bal.GetRFQEventsAsync());
                        break;

                    case "RegisterQuotation":
                        events.AddRange(await bal.GetRegisterQuotationEventsAsync());
                        break;

                    case "PurchaseOrder":
                        events.AddRange(await bal.GetPurchaseOrderEventsAsync());
                        break;

                    case "GRNInfo":
                        events.AddRange(await bal.GetGRNEventsAsync());
                        break;

                    case "GoodsReturnInfo":
                        events.AddRange(await bal.GetGoodsReturnEventsAsync());
                        break;

                    case "QualityCheckInfo":
                        events.AddRange(await bal.GetQualityCheckEventsAsync());
                        break;
                }
            }

            // Return final events as JSON
            return Json(events, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Retrieves the list of unread notifications for the currently logged-in staff member.
        /// </summary>
        /// <returns>
        /// A JSON result containing a list of unread notifications for the user.
        /// </returns>
        [HttpGet]
        public async Task<JsonResult> GetNotifications()
        {
            string staffCode = Session["StaffCode"]?.ToString();
            var data = await bal.GetUnreadNotifications(staffCode);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Retrieves the complete list of notifications (read and unread) 
        /// for the currently logged-in staff member.
        /// </summary>
        /// <returns>
        /// A JSON result containing all notifications for the user.
        /// </returns>
        [HttpGet]
        public async Task<JsonResult> GetAllNotifications()
        {
            string staffCode = Session["StaffCode"]?.ToString();
            var data = await bal.GetAllNotifications(staffCode);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Marks a specific notification as read for the currently logged-in staff member.
        /// </summary>
        /// <param name="id">The unique identifier of the notification to mark as read.</param>
        /// <returns>
        /// A JSON result indicating success.
        /// </returns>
        [HttpPost]
        public async Task<JsonResult> MarkAsRead(int id)
        {
            string staffCode = Session["StaffCode"]?.ToString();
            await bal.MarkAsRead(id, staffCode);
            return Json(new { success = true });
        }

        /// <summary>
        /// Marks all unread notifications as read for the currently logged-in staff member.
        /// </summary>
        /// <returns>
        /// A JSON result indicating success.
        /// </returns>
        [HttpPost]
        public async Task<JsonResult> MarkAllAsRead()
        {
            string staffCode = Session["StaffCode"]?.ToString();
            await bal.MarkAllAsRead(staffCode);
            return Json(new { success = true });
        }


        [HttpGet]
        public ActionResult SendMailHSB()
        {
            return View();
        }

        [HttpPost]
        public ActionResult SendMailHSB(HttpPostedFileBase attachment, string toEmail, string subject, string messageBody)
        {
            try
            {
                string fromEmail = System.Configuration.ConfigurationManager.AppSettings["SenderEmail"];
                string password = System.Configuration.ConfigurationManager.AppSettings["SenderPassword"];

                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(fromEmail);
                mail.To.Add(toEmail);
                mail.Subject = subject;
                mail.Body = messageBody;
                mail.IsBodyHtml = true;

                // Add attachment if provided
                if (attachment != null && attachment.ContentLength > 0)
                {
                    string fileName = Path.GetFileName(attachment.FileName);
                    mail.Attachments.Add(new Attachment(attachment.InputStream, fileName));
                }

                SmtpClient smtp = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential(fromEmail, password),
                    EnableSsl = true
                };

                smtp.Send(mail);
                ViewBag.Status = "Email sent successfully!";
            }
            catch (Exception ex)
            {
                ViewBag.Status = "Error: " + ex.Message;
            }

            return View();
        }


    }
}