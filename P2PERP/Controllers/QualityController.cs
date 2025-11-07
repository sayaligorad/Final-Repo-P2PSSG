using P2PLibray.Account;
using P2PLibray.GRN;
using P2PLibray.Quality;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using static P2PLibray.Quality.GRNShowItemPR;
using static P2PLibray.Quality.Quality;
using System.IO;

namespace P2PERP.Controllers
{
    public class QualityController : Controller
    {
        BALQuality bal = new BALQuality();

        #region Pranav Mane
        public async Task<ActionResult> UserProfile()
        {
            var loginCheck = CheckLogin();
            if (loginCheck != null)
                return loginCheck;

            BALAccount account = new BALAccount();
            var acc = await account.UserProfileDetails(Session["StaffCode"].ToString());
            return View(acc);
        }

        public ActionResult CheckLogin()
        {
            if (Session["StaffCode"] == null || string.IsNullOrWhiteSpace(Session["StaffCode"].ToString()))
            {
                return RedirectToAction("MainLogin", "Account");
            }
            return null;
        }

        public ActionResult Calender()
        {
            if (Session["StaffCode"] == null || string.IsNullOrWhiteSpace(Session["StaffCode"].ToString()))
            {
                return RedirectToAction("MainLogin", "Account");
            }
            return View();
        }


        [Route("Quality/SendMail")]
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
        #endregion

        #region Prashant

        // Loads the Quality Check Reports page (View only)
        public ActionResult QalityCheackReports()
        {
            return View();
        }

        // ✅ Quality Item Check Count (Status-wise QC Report)
        [HttpGet]
        public async Task<JsonResult> QualityCheakPR(string startDate = null, string endDate = null)
        {
            // Fetch data from BAL method (status-wise QC counts)
            var dr = await bal.GetStatusWiseQualityAsyncPR(startDate, endDate);

            List<QualityPR> qualityList = new List<QualityPR>();

            // Read data and populate list
            if (dr.HasRows)
            {
                while (await dr.ReadAsync())
                {
                    qualityList.Add(new QualityPR
                    {
                        StatusName = dr["StatusName"].ToString(),
                        TotalQc = Convert.ToInt32(dr["TotalQc"])
                    });
                }
            }
            dr.Close();

            // Return JSON result (for charts/tables on frontend)
            return Json(new { data = qualityList }, JsonRequestBehavior.AllowGet);
        }


        // ✅ GRN Pie Chart Data (Status-wise GRN count)
        [HttpGet]
        public async Task<JsonResult> GRNPieChartPR(string startDate = null, string endDate = null)
        {
            var dr = await bal.GRNAllPR(startDate, endDate);

            var result = new List<object>();
            while (await dr.ReadAsync())
            {
                result.Add(new
                {
                    StatusName = dr["StatusName"].ToString(),
                    TotalGRN = Convert.ToInt32(dr["TotalGRN"])
                });
            }
            dr.Close();

            return Json(result, JsonRequestBehavior.AllowGet);
        }


        // ========================== Confirm GRN Items ==========================

        // Loads the Confirm GRN Items page (View only)
        public ActionResult ConfirmItem()
        {
            return View();
        }

        // ✅ Get Confirm GRN List
        [HttpGet]
        public async Task<JsonResult> ConfirmItemsPSR()
        {
           
            var confirmList = await bal.ConfirmItemGrnPSR();

            return Json(new { data = confirmList }, JsonRequestBehavior.AllowGet);
        }

        // ✅ Get Confirmed Item Details by GRN Code
        [HttpGet]
        public async Task<JsonResult> ConfirmedItemsDetailsPR(string grnCode)
        {
            if (string.IsNullOrEmpty(grnCode))
            {
                return Json(new { success = false, message = "GRNCode is required" }, JsonRequestBehavior.AllowGet);
            }

            try
            {
                var confirmedItems = await bal.ConfirmItemPR(grnCode);
                return Json(new { success = true, data = confirmedItems }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // Return error message if exception occurs
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        // ========================== Non-Confirm GRN Items ==========================

        // Loads the Non-Confirm GRN Items page (View only)
        public ActionResult NonConfirmItem()
        {
            return View();
        }

        // ✅ Get Non-Confirm GRN List
        [HttpGet]
        public async Task<JsonResult> nonConfirmItemsPR()
        {
           
            var nonconfirmList = await bal.NonConfirmItemGrnPR();

            return Json(new { data = nonconfirmList }, JsonRequestBehavior.AllowGet);
        }

        // ✅ Get Failed/Rejected Item Details by GRN Code
        [HttpGet]
        public async Task<JsonResult> FailedItemsDetailsPR(string grnCode)
        {
            if (string.IsNullOrEmpty(grnCode))
            {
                return Json(new { success = false, message = "GRNCode is required" }, JsonRequestBehavior.AllowGet);
            }

            try
            {
                var failedItems = await bal.NonConfirmItemListPR(grnCode);
                return Json(new { success = true, data = failedItems }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // Return error message if exception occurs
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        // ========================== GRN Master List ==========================

        // Loads the GRN List page (View only)
        public ActionResult GRN()
        {
            return View();
        }

        // ✅ Get GRN List
        [HttpGet]
        public async Task<JsonResult> GRNShowListPSR()
        {
           
            var grnList = await bal.GRNShowListAsyncPR();

            return Json(new { data = grnList }, JsonRequestBehavior.AllowGet);
        }



		// ========================== Graph Reports ==========================
		// Confirmed Items Controller
		[HttpGet]
		public async Task<JsonResult> ConfirmedItemDetailsPSR(DateTime? startDate = null, DateTime? endDate = null)
		{
			try
			{
				var confirmedItems = await bal.ConfirmItemDetailsPSR(startDate, endDate);
				return Json(new { success = true, data = confirmedItems }, JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
			}
		}

		// Failed Items Controller
		[HttpGet]
		public async Task<JsonResult> FailedItemsGraphPR(DateTime? startDate = null, DateTime? endDate = null)
		{
			try
			{
				var failedItems = await bal.GetFailedItemsPR(startDate, endDate);
				return Json(new { success = true, data = failedItems }, JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
			}
		}

		// Pending Items Controller
		public async Task<JsonResult> PendingItemsGraphPR(string startDate = null, string endDate = null)
		{
			try
			{
				// Fetch data from BAL method
				var dr = await bal.GetPendingItemsAsyncPR(startDate, endDate);

				List<PendingItemPR> pendingList = new List<PendingItemPR>();

				// Read data and populate list
				if (dr.HasRows)
				{
					while (await dr.ReadAsync())
					{
						pendingList.Add(new PendingItemPR
						{
							GRNCode = dr["GRNCode"]?.ToString() ?? "",
							ItemCode = dr["ItemCode"]?.ToString() ?? "",
							ItemName = dr["ItemName"]?.ToString() ?? "",
							AddedDate = dr["ItemAddedDate"]?.ToString() ?? ""
						});
					}
				}
				dr.Close();

				// Return JSON result
				return Json(new { data = pendingList }, JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				// Log error
				System.Diagnostics.Debug.WriteLine($"Error in PendingItemsGraphPR: {ex.Message}");
				return Json(new { data = new List<PendingItemPR>() }, JsonRequestBehavior.AllowGet);
			}
		}



        #endregion Prashant

        #region Rajlaxmi

        // GET: QualityP2P
        //  Default Index action
        public ActionResult IndexRG()
        {
            return View();
        }

        //  Returns all Quality Check grid data (JSON) for rendering in client-side tables

        public async Task<ActionResult> AllgriddataRG()
        {
            var data = await bal.AllItemCheckGridRG();
            return Json(data, JsonRequestBehavior.AllowGet);
        }


        //  Loads New Task list (as PartialView) for a given GRN Code
        public async Task<ActionResult> NewTaskListRG(string id)
        {
            // If id is not passed, pick a default or just show blank
            if (!string.IsNullOrEmpty(id))
            {
                var data = await bal.AllItemCheckGridRG();
                var newdata = data.FirstOrDefault(i => i.GRNCode == id);

                if (newdata != null)
                    ViewBag.GRN = newdata.GRNCode;
                else
                    ViewBag.GRN = id; // fallback: still show the id
            }
            else
            {
                // fallback if no id passed in query string
                ViewBag.GRN = "";
            }

            return PartialView("_NewTaskListRG"); //  always return same partial
        }


        //  Fetches item details by GRN code and returns as JSON
        public async Task<ActionResult> ItemByGRNRG()
        {
            var data = await bal.ItemByGRNCodeRG();
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        // Loads inspection form details for a given ItemCode
        public async Task<ActionResult> InspecFormRG(string id)
        {
            var data = await bal.InspecdetailsFormRG(id);
            var newdata = data.FirstOrDefault(i => i.GrnItemCode == id);

            if (newdata != null)
            {
                ViewBag.ItemCode = newdata.ItemCode;
                ViewBag.ItemName = newdata.ItemName;
                ViewBag.ItemType = newdata.ItemType;
                ViewBag.InspectionType = newdata.InspectionType;
                ViewBag.PlanName = newdata.PlanName;
                ViewBag.AddedDate = newdata.strAddedDate;
                ViewBag.Parameters = newdata.Parameters;
                ViewBag.GRN = newdata.GRNCode;
                ViewBag.Quantity = newdata.Quantity;
                ViewBag.GRNItemCode = newdata.GrnItemCode;
                return PartialView("InspecFormRG");
            }
            else
            {
                return PartialView("InspecFormRG");
            }

        }

        //  Load the parameters in table
        public async Task<JsonResult> ParameterTableRG(string id)
        {
            var data = await bal.ParametertableRG(id);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]

        // change the status in quality check table
        public async Task<ActionResult> qualitycheckitemRG(string id, string sqc, string Inf, string GRNCode)
        {

            var staffcode = Session["StaffCode"].ToString();
            await bal.ConfirmBtnInsStatusRG(id, sqc, Inf, staffcode);

            return Json(new { success = true });
        }




        //This is PartialView for nonconfirmform
        public async Task<ActionResult> NonconfirmRG(string GRIcode, string sqc, string Inf, string itemcode, string grnnumber)
        {
            if (!string.IsNullOrEmpty(itemcode))
            {
                var data = await bal.NonConfirmformRG(itemcode);
                var newdata = data.FirstOrDefault(i => i.ItemCode == itemcode);

                if (newdata != null)
                {
                    string nccode = await bal.GenerateNextNCcode();
                    string QCCode = await bal.GenerateNextQCCode();

                    ViewBag.GRN = grnnumber;
                    ViewBag.InspType = newdata.InspectionType;
                    ViewBag.planname = newdata.PlanName;
                    ViewBag.itemname = newdata.ItemName;
                    ViewBag.itemcode = newdata.ItemCode;
                    ViewBag.NcCode = nccode;
                    ViewBag.sqc = sqc;
                    ViewBag.Inf = Inf;
                    ViewBag.GRICode = GRIcode;
                    ViewBag.QCCode = QCCode;


                    return PartialView("_NonconfirmPartialRG");
                }

                ViewBag.Message = "No record found for ItemCode: " + itemcode;
                return PartialView("_NonconfirmPartialRG");
            }

            return PartialView("_NonconfirmPartialRG");
        }

        //This is Function for getting data for update and also insert data into  tables
        public async Task<ActionResult> finalNCRG(string GRNICode, string SQC, string INF, string FQC, string ROR, string QC, string STF, string GRNCode)
        {
            var staffcode = Session["StaffCode"].ToString();
            String QCCODE = await bal.GetQcCodeRG(GRNICode);
            await bal.InitiatebtnstatusRG(GRNICode, INF, SQC, staffcode);
            await bal.insertQFitemsRG(FQC, QCCODE, STF, ROR, staffcode);

            return Json("");

        }

        //This is View for completed task
        public PartialViewResult CompletedTaskListRG(string id)
        {

            ViewBag.GRN = id ?? "";
            return PartialView("_CompletedTaskListRG"); // ✅ empty model if no
        }


        //This is Function Object for completed task grid load
        public async Task<ActionResult> CompletedTaskgridRRG(string id)
        {
            var data = await bal.CompletedTaskRG(id);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        #endregion Rajlaxmi

        #region Nurjha
        // GET: Quality Index Page
        public ActionResult Index()
        {
            // Return default index view
            return View();
        }

        // GET: Quality Dashboard for NAM
        public ActionResult QCheckDashboardNAM()
        {
            // Return dashboard view
            return View();
        }




        // GET: Quality Dashboard for NAM
        public ActionResult QualityDashBoardNAM()
        {
            // Return dashboard view
            return View();
        }

        // GET: Quality View for NAM
        public ActionResult QualityViewNAM()
        {
            // Return quality view page
            return View();
        }



        // Fetch Confirmed and Non-Confirmed counts
        [HttpGet]
        public async Task<JsonResult> GetConfirmCountNAM(DateTime? startDate, DateTime? endDate)
        {
            // Call BAL to get confirm/non-confirm counts
            var result = await bal.GetConfirmCountNAM(startDate, endDate);
            // Return JSON result
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        // Fetch Confirmed items List
        [HttpGet]
        public async Task<JsonResult> GetConfirmedListNAM(DateTime? startDate, DateTime? endDate)
        {
            // Call BAL to get confirmed GRN list
            var list = await bal.GetConfirmedListNAM(startDate, endDate);
            // Return JSON result
            return Json(list, JsonRequestBehavior.AllowGet);
        }
        // Fetch NonConfirmed GRN List
        [HttpGet]
        public async Task<JsonResult> GetNonConfirmedListNAM(DateTime? startDate, DateTime? endDate)
        {
            // Call BAL to get Nonconfirmed items  list
            var list = await bal.GetNonConfirmedListNAM(startDate, endDate);
            // Return JSON result
            return Json(list, JsonRequestBehavior.AllowGet);
        }



        public async Task<JsonResult> GetPendingListNAM(DateTime? startDate, DateTime? endDate)
        {
            var result = await bal.GetPendingListNAM(startDate, endDate);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}