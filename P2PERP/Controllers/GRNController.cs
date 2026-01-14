using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.draw;
using P2PLibray.Account;
using P2PLibray.GRN;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace P2PERP.Controllers
{
    public class GRNController : Controller
    {
        BALGRN bal = new BALGRN();

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
        #endregion

        #region Rutik
        // ---- Loads the main GRN index page
        public ActionResult Index()
        {
            return View();
        }

        // ---- Loads the Rejected Goods page
        public ActionResult RejectedGoods()
        {
            return View();
        }

        // ---- Loads the Return Goods page
        public ActionResult ReturnGoods()
        {
            return View();
        }

        // ---- Loads the partial view for rejected goods list
        public ActionResult RejectedGoodsList()
        {
            return PartialView("_RejectedGRNHSB");
        }

        // ---- Loads the partial view for goods return list
        public ActionResult GoodsReturnList()
        {
            return PartialView("_GoodReturnListHSB");
        }

        // ---- Loads the print view for goods return details by GRN Code
        public async Task<ActionResult> PrintReturnListHSB(string grCode)
        {
            PrintDetailsClass model = await bal.PrintDetailsHSB(grCode);
            return PartialView("_PrintHSB", model);
        }

        // ---- Returns JSON data for rejected goods
        [HttpGet]
        public async Task<JsonResult> getRejectedGoods()
        {
            SqlDataReader dr = await bal.GetRejectedGoods();
            List<ReturnGoodsClass> ReturnGoodsList = new List<ReturnGoodsClass>();

            if (dr.HasRows)
            {
                while (await dr.ReadAsync())
                {
                    ReturnGoodsClass ReturnGoods = new ReturnGoodsClass
                    {
                        GRNCode = dr["GRNCode"].ToString(),
                        AddedDate = Convert.ToDateTime(dr["AddedDate"]).ToString("yyyy-MM-dd"),
                        FullName = dr["FullName"].ToString()
                    };

                    ReturnGoodsList.Add(ReturnGoods);
                }
            }

            dr.Close();
            return Json(new { data = ReturnGoodsList }, JsonRequestBehavior.AllowGet);
        }

        // ---- Returns JSON data for goods return list
        [HttpGet]
        public async Task<JsonResult> getReturnGoodsList()
        {
            SqlDataReader dr = await bal.GetReturnGoods();
            List<ReturnGood> ReturnGoodsList = new List<ReturnGood>();

            if (dr.HasRows)
            {
                while (await dr.ReadAsync())
                {
                    ReturnGood ReturnGoods = new ReturnGood
                    {
                        GRNCode = dr["GRNCode"].ToString(),
                        StatusName = dr["StatusName"].ToString(),
                        AddedDate = Convert.ToDateTime(dr["AddedDate"]).ToString("yyyy-MM-dd"),
                        GoodReturnCode = dr["GoodReturnCode"].ToString(),
                        VenderName = dr["VenderName"].ToString(),
                        InvoiceNo = dr["InvoiceNo"].ToString(),
                        ReasonOfRejection = dr["ReasonOfRejection"].ToString()
                    };

                    ReturnGoodsList.Add(ReturnGoods);
                }
            }

            dr.Close();
            return Json(new { data = ReturnGoodsList }, JsonRequestBehavior.AllowGet);
        }

        // ---- Loads partial form for Goods Return creation based on GRNCode
        [HttpGet]
        public async Task<ActionResult> CreateGRForm(string grnCode)
        {
            GoodsReturnViewModel model = await bal.GSTInstockGRNHSB(grnCode);
            return PartialView("_CreateGR", model);
        }

        // ---- Saves Goods Return details
        [HttpPost]
        public async Task<JsonResult> SaveGRHSB(GoodsReturnViewModel model)
        {
            try
            {
                model.AddedBy = Session["StaffCode"].ToString();
                await bal.SaveGRHSB(model);
                return Json(new { success = true, message = "Goods Return saved successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        // ---- Loads partial form for Dispatch details based on GRNCode
        [HttpGet]
        public async Task<ActionResult> DispatchGRForm(string grCode)
        {
            GoodsReturnViewModel model = await bal.DispatchDetailsHSB(grCode);
            return PartialView("_DispatchHSB", model);
        }

        // ---- Returns list of available vehicle types
        [HttpGet]
        public async Task<JsonResult> getVehicleTypeHSB()
        {
            List<vehicleTypeModel> model = await bal.GetVehicleTypesHSB();
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        // ---- Returns company address based on staff code
        [HttpGet]
        public async Task<JsonResult> getCompanyAddressHSB()
        {
            string staffCode = "STF001";
            string model = await bal.GetCompanyAddressHSB(staffCode);
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        // ---- Saves dispatch details for goods return
        [HttpPost]
        public async Task<JsonResult> SaveDispatchHSB(GoodDispatchModel model)
        {
            try
            {
                await bal.SaveDispachHSB(model);
                return Json(new { success = true, message = "Items dispatched successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        [Route("GRN/SendMail")]
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

        #region Pravin

        //GRN Summary Report Action Methods
        public ActionResult GRNSummaryReportPSM()
        {
            return View();
        }


        //Total GRN Count
        [HttpGet]
        public async Task<JsonResult> GRNPieChartPSM(string fromDate, string toDate)
        {
            BALGRN obj = new BALGRN();
            DataTable dt = await obj.GRNSummaryPSM();

            int totalGRN = 0;
            DateTime? from = null, to = null;

            if (DateTime.TryParse(fromDate, out DateTime fd)) from = fd;
            if (DateTime.TryParse(toDate, out DateTime td)) to = td;

            if (dt != null && dt.Rows.Count > 0)
            {
                var filteredRows = dt.AsEnumerable();

                if (from.HasValue && to.HasValue)
                {
                    filteredRows = filteredRows.Where(r =>
                    {
                        var date = r.Field<DateTime>("AddedDate").Date;
                        return date >= from.Value.Date && date <= to.Value.Date;
                    });
                }

                totalGRN = filteredRows.Sum(r => r.Field<int>("TotalGRN"));
            }

            return Json(new { TotalGRN = totalGRN }, JsonRequestBehavior.AllowGet);
        }



        //Retrieves GRN item details by GRN code for display.
        [HttpGet]
        public async Task<JsonResult> GRNItemsPSM(string GRNCode)
        {
            BALGRN obj = new BALGRN();
            DataTable dt = await obj.GRNItemsPSM(GRNCode);
            var items = dt.AsEnumerable().Select(row => new
            {
                ItemName = row["ItemName"].ToString(),
                CostPerUnit = row["CostPerUnit"].ToString(),
                UnitQuantity = row["UnitQuantity"].ToString(),
                Discount = row["Discount"].ToString(),
                TaxRate = row["TaxRate"].ToString(),
                FinalAmount = row["FinalAmount"].ToString(),
                IsQuality = row["IsQuality"].ToString()
            }).ToList();

            return Json(new { data = items }, JsonRequestBehavior.AllowGet);
        }

        // Fetch GRN Reoprt list in Datatable
        [HttpGet]
        public async Task<JsonResult> AllGRNSummaryListPSM(string fromDate, string toDate)
        {
            BALGRN obj = new BALGRN();
            List<object> materials = new List<object>();

            DateTime? from = null, to = null;
            if (DateTime.TryParse(fromDate, out DateTime fd)) from = fd.Date;
            if (DateTime.TryParse(toDate, out DateTime td)) to = td.Date;

            SqlDataReader dr = await obj.GRNSummaryListPSM();

            while (await dr.ReadAsync())
            {
                if (!DateTime.TryParse(dr["AddedDate"].ToString(), out DateTime addedDate))
                    continue;

                // ✅ Compare only the Date part
                DateTime addedDateOnly = addedDate.Date;

                if ((!from.HasValue || addedDateOnly >= from.Value) &&
                    (!to.HasValue || addedDateOnly <= to.Value))
                {
                    materials.Add(new
                    {
                        GRNCode = dr["GRNCode"].ToString(),
                        POCode = dr["POCode"].ToString(),
                        VendorName = dr["VendorName"]?.ToString() ?? "",
                        CompanyName = dr["CompanyName"]?.ToString() ?? "",
                        AddedBy = dr["AddedBy"]?.ToString() ?? "",
                        AddedDate = addedDateOnly.ToString("yyyy-MM-dd"),
                        TotalAmount = dr["TotalAmount"]?.ToString() ?? ""
                    });
                }
            }
            dr.Close();

            return Json(new { data = materials }, JsonRequestBehavior.AllowGet);
        }




        //Return Good Report Action Methods
        public ActionResult GoodsReturnSummaryReportPSM()
        {
            return View();
        }

        //Alternative - Use same approach as table method
        public async Task<JsonResult> GoodsReturnChartsPSM(string fromDate, string toDate)
        {
            try
            {
                BALGRN obj = new BALGRN();
                List<object> goodsReturns = new List<object>();

                SqlDataReader dr = await obj.GoodsReturnSummaryListPSM();

                DateTime? fDate = null;
                DateTime? tDate = null;

                // Parse dates safely (same as table method)
                if (!string.IsNullOrEmpty(fromDate))
                {
                    if (DateTime.TryParseExact(fromDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedFromDate))
                        fDate = parsedFromDate;
                }

                if (!string.IsNullOrEmpty(toDate))
                {
                    if (DateTime.TryParseExact(toDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedToDate))
                        tDate = parsedToDate;
                }

                // Count statuses directly from the data reader
                int assignCount = 0;
                int dispatchCount = 0;

                while (await dr.ReadAsync())
                {
                    if (!DateTime.TryParse(dr["AddedDate"]?.ToString(), out DateTime addedDate))
                        continue;

                    // Apply date filtering (same as table method)
                    bool include = true;
                    if (fDate.HasValue)
                        include = include && (addedDate.Date >= fDate.Value.Date);
                    if (tDate.HasValue)
                        include = include && (addedDate.Date <= tDate.Value.Date);

                    if (include)
                    {
                        var statusName = dr["StatusName"]?.ToString() ?? "";
                        if (statusName.Equals("Assign", StringComparison.OrdinalIgnoreCase))
                            assignCount++;
                        else if (statusName.Equals("Dispatch", StringComparison.OrdinalIgnoreCase))
                            dispatchCount++;
                    }
                }

                dr.Close();

                var result = new List<object>
        {
            new { StatusName = "Assign", Count = assignCount },
            new { StatusName = "Dispatch", Count = dispatchCount }
        };

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GoodsReturnChartsPSM: {ex.Message}");
                return Json(new[] {
            new { StatusName = "Assign", Count = 0 },
            new { StatusName = "Dispatch", Count = 0 }
           }, JsonRequestBehavior.AllowGet);
            }
        }

        // Goods return summary report show in Datatable with Date Filtering
        [HttpGet]
        public async Task<JsonResult> GetGoodsReturnSummaryPSM(string fromDate, string toDate)
        {
            try
            {
                BALGRN obj = new BALGRN();
                List<object> goodsReturns = new List<object>();

                SqlDataReader dr = await obj.GoodsReturnSummaryListPSM();

                DateTime? fDate = null;
                DateTime? tDate = null;

                // Parse dates safely
                if (!string.IsNullOrEmpty(fromDate))
                {
                    if (DateTime.TryParseExact(fromDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedFromDate))
                        fDate = parsedFromDate;
                }

                if (!string.IsNullOrEmpty(toDate))
                {
                    if (DateTime.TryParseExact(toDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedToDate))
                        tDate = parsedToDate;
                }

                while (await dr.ReadAsync())
                {
                    if (!DateTime.TryParse(dr["AddedDate"]?.ToString(), out DateTime addedDate))
                        continue;

                    // Apply date filtering
                    bool include = true;
                    if (fDate.HasValue)
                        include = include && (addedDate.Date >= fDate.Value.Date);
                    if (tDate.HasValue)
                        include = include && (addedDate.Date <= tDate.Value.Date);

                    if (include)
                    {
                        goodsReturns.Add(new
                        {
                            GoodsReturnCode = dr["GoodsReturnCode"]?.ToString() ?? "",
                            TransporterName = dr["TransporterName"]?.ToString() ?? "",
                            TransportContactNo = dr["TransportContactNo"]?.ToString() ?? "",
                            VehicleNo = dr["VehicleNo"]?.ToString() ?? "",
                            VehicleTypeName = dr["VehicleTypeName"]?.ToString() ?? "",
                            Reason = dr["Reason"]?.ToString() ?? "",
                            StatusName = dr["StatusName"]?.ToString() ?? "",
                            AddedDate = addedDate.ToString("yyyy-MM-dd")
                        });
                    }
                }

                dr.Close();

                return Json(new { data = goodsReturns }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetGoodsReturnSummaryPSM: {ex.Message}");
                return Json(new { data = new List<object>() }, JsonRequestBehavior.AllowGet);
            }
        }


        //Quality check Report Action Method 
        public ActionResult GRNQualityCheckPSM()
        {
            return View();
        }


        //Total Quality Doughnut chart Action Method
        [HttpGet]
        public async Task<JsonResult> GRNQualityCheckChartPSM(string startDate = "", string endDate = "")
        {
            BALGRN obj = new BALGRN();
            DataTable dt = await obj.GRNQualityCheckPSM(); // original SP call, no date params
            if (dt == null || dt.Rows.Count == 0)
                return Json(new List<object>(), JsonRequestBehavior.AllowGet);
            DateTime? from = null, to = null;
            if (!string.IsNullOrEmpty(startDate))
                from = DateTime.ParseExact(startDate, "yyyy-MM-dd", null);
            if (!string.IsNullOrEmpty(endDate))
                to = DateTime.ParseExact(endDate, "yyyy-MM-dd", null);
            var query = dt.AsEnumerable();
            if (from.HasValue && to.HasValue && dt.Columns.Contains("AddedDate"))
            {
                query = query.Where(row =>
                {
                    DateTime added = Convert.ToDateTime(row["AddedDate"]);
                    return added >= from.Value && added <= to.Value;
                });
            }
            var result = query.Select(row => new
            {
                Category = row["ItemCategoryName"].ToString(),
                Count = Convert.ToInt32(row["QualityCount"])
            }).ToList();

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        // Total Assigned Quality check item list
        [HttpGet]
        public async Task<JsonResult> GetGRNAssignQCListPSM(string startDate = "", string endDate = "")
        {
            BALGRN obj = new BALGRN();
            DataTable dt = await obj.GRNAssignQCListPSM();
            DateTime sDate, eDate;
            bool hasStart = DateTime.TryParse(startDate, out sDate);
            bool hasEnd = DateTime.TryParse(endDate, out eDate);
            var rows = dt.AsEnumerable().Where(row =>
            {
                if (!dt.Columns.Contains("AddedDate") || row["AddedDate"] == DBNull.Value)
                    return false;

                var addedDate = Convert.ToDateTime(row["AddedDate"]);
                if (hasStart && addedDate < sDate) return false;
                if (hasEnd && addedDate > eDate) return false;
                return true;
            });

            var list = rows.Select(r => new
            {
                ItemName = r["ItemName"].ToString(),
                Description = r["Description"].ToString(),
                StatusName = r["StatusName"].ToString(),
                ItemCategoryName = r["ItemCategoryName"].ToString(),
                Name = r["Name"].ToString(),
                AddedDate = (dt.Columns.Contains("AddedDate") && r["AddedDate"] != DBNull.Value)
                                ? Convert.ToDateTime(r["AddedDate"]).ToString("yyyy-MM-dd")
                                : ""
            }).ToList();

            return Json(new { data = list }, JsonRequestBehavior.AllowGet);
        }


        // Approved Purchase Order Item  Action Method
        public ActionResult ApprovedPOReportPSM()
        {
            return View();
        }
        // Approved Purchase Order Report Action Methods Chart
        [HttpGet]
        public async Task<JsonResult> ApprovedPOChartPSM(string startDate = "", string endDate = "")
        {
            BALGRN obj = new BALGRN();
            DataTable dt = await obj.ApprovedPOTotalCountChartPSM();

            DateTime? sDate = string.IsNullOrEmpty(startDate) ? (DateTime?)null : DateTime.Parse(startDate);
            DateTime? eDate = string.IsNullOrEmpty(endDate) ? (DateTime?)null : DateTime.Parse(endDate).AddDays(1).AddSeconds(-1);

            var rows = dt.AsEnumerable()
                .Where(r =>
                {
                    if (r["ApprovedRejectedDate"] == DBNull.Value) return false;
                    DateTime approveDate = Convert.ToDateTime(r["ApprovedRejectedDate"]);
                    if (sDate.HasValue && approveDate < sDate.Value) return false;
                    if (eDate.HasValue && approveDate > eDate.Value) return false;
                    return true;
                });

            var result = rows
                .GroupBy(r => r["VendorName"].ToString())
                .Select(g => new
                {
                    VendorName = g.Key,
                    Count = g.Select(r => r["POCode"].ToString()).Distinct().Count()
                })
                .ToList();

            return Json(new { data = result }, JsonRequestBehavior.AllowGet);
        }


        // Approved Purchase Order Report Action Methods open list
        [HttpGet]
        public async Task<JsonResult> ApprovedPOListsPSM(string startDate = "", string endDate = "")
        {
            BALGRN obj = new BALGRN();
            DataTable dt = await obj.ApprovedPOListPSM();

            DateTime? sDate = string.IsNullOrEmpty(startDate) ? (DateTime?)null : DateTime.Parse(startDate);
            DateTime? eDate = string.IsNullOrEmpty(endDate) ? (DateTime?)null : DateTime.Parse(endDate).AddDays(1).AddSeconds(-1);

            var poList = dt.AsEnumerable()
                .Where(row =>
                {
                    if (row["ApprovedRejectedDate"] == DBNull.Value) return true;
                    DateTime approvedDate = Convert.ToDateTime(row["ApprovedRejectedDate"]);
                    if (sDate.HasValue && approvedDate < sDate.Value) return false;
                    if (eDate.HasValue && approvedDate > eDate.Value) return false;
                    return true;
                })
                .Select(row => new
                {
                    POCode = row["POCode"].ToString(),
                    RQNO = row["RQNO"].ToString(),
                    VendorName = row["VendorName"].ToString(),
                    VendorCompanyName = row["VendorCompanyName"].ToString(),
                    ApprovedRejectedByName = row["ApprovedRejectedByName"].ToString(),
                    ApprovedRejectedDate = row["ApprovedRejectedDate"] == DBNull.Value
                        ? ""
                        : Convert.ToDateTime(row["ApprovedRejectedDate"]).ToString("yyyy-MM-dd"),
                    TotalAmount = row["TotalAmount"] != DBNull.Value ? Convert.ToDecimal(row["TotalAmount"]).ToString("N2") : "0.00",
                    StatusName = row["StatusName"].ToString()
                }).ToList();

            return Json(new { data = poList }, JsonRequestBehavior.AllowGet);
        }


        // Approved Purchase Order Report Action Methods Approved PO Items
        [HttpGet]
        public async Task<JsonResult> ApprovedPOItemsPSM(string poCode)
        {
            BALGRN obj = new BALGRN();
            DataTable dt = await obj.ApprovedPOItemsPSM(poCode);

            var result = dt.AsEnumerable()
                .Select(row => new
                {
                    ItemName = row["ItemName"].ToString(),
                    CostPerUnit = row["CostPerUnit"].ToString(),
                    UnitQuantity = row["UnitQuantity"].ToString(),
                    Discount = row["Discount"].ToString(),
                    TaxRate = row["TaxRate"].ToString(),
                    FinalAmount = row["FinalAmount"].ToString(),
                    IsQuality = row["IsQuality"].ToString(),
                })
                .ToList();

            return Json(new { data = result }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Rushikesh

        public ActionResult GRNDashboardRHK()
        {
            return View();
        }

        /// <summary>
        /// Returns total GRN count between startDate and endDate.
        /// </summary>
        public async Task<JsonResult> GetTotalGRNRHK(DateTime? startDate, DateTime? endDate)
        {
            try
            {
                DataTable dt = await bal.TotalGRNRHK(startDate, endDate);

                int count = (dt.Rows.Count > 0) ? Convert.ToInt32(dt.Rows[0]["TotalGRN"]) : 0;
                return Json(new { count = count }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new { count = 0 }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Returns total number of GRN items between startDate and endDate.
        /// </summary>
        public async Task<JsonResult> GetTotalGRNItemRHK(DateTime? startDate, DateTime? endDate)
        {
            try
            {
                DataTable dt = await bal.TotalGRNItemRHK(startDate, endDate);
                int count = (dt.Rows.Count > 0) ? Convert.ToInt32(dt.Rows[0]["TotalItem"]) : 0;
                return Json(new { count = count }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new { count = 0 }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Returns count of Approved QC items between startDate and endDate.
        /// </summary>
        public async Task<JsonResult> GetApproveCountRHK(DateTime? startDate, DateTime? endDate)
        {
            try
            {
                DataTable dt = await bal.ApproveCountRHK(startDate, endDate);
                int count = (dt.Rows.Count > 0) ? Convert.ToInt32(dt.Rows[0]["ApproveCount"]) : 0;
                return Json(new { count = count }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new { count = 0 }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Returns count of Rejected GRNs within given date range.
        /// </summary>
        public async Task<JsonResult> GetRejectedGRNCountRHK(DateTime? startDate, DateTime? endDate)
        {
            try
            {
                DataTable dt = await bal.RejectedGRNCountRHK(startDate, endDate);
                int count = (dt.Rows.Count > 0) ? Convert.ToInt32(dt.Rows[0]["RejectedCount"]) : 0;
                return Json(new { count = count }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new { count = 0 }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Returns count of QC assigned items between startDate and endDate.
        /// </summary>
        public async Task<JsonResult> GetQCAssignedCountRHK(DateTime? startDate, DateTime? endDate)
        {
            try
            {
                DataTable dt = await bal.QCAssignedCountRHK(startDate, endDate);
                int count = (dt.Rows.Count > 0) ? Convert.ToInt32(dt.Rows[0]["QCAssignedCount"]) : 0;
                return Json(new { count = count }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new { count = 0 }, JsonRequestBehavior.AllowGet);
            }
        }
        /// <summary>
        /// Returns count of QC pending items between startDate and endDate.
        /// </summary>
        public async Task<JsonResult> GetPendingCountRHK(DateTime? startDate, DateTime? endDate)
        {
            try
            {
                DataTable dt = await bal.PendingCountRHK(startDate, endDate);
                int count = (dt.Rows.Count > 0) ? Convert.ToInt32(dt.Rows[0]["PendingCount"]) : 0;
                return Json(new { count = count }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new { count = 0 }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Returns count of upcoming items (in open POs not yet GRN’d).
        /// </summary>
        public async Task<JsonResult> GetUpcomingItemCountRHK(DateTime? startDate, DateTime? endDate)
        {
            try
            {
                DataTable dt = await bal.UpcomingItemCountRHK(startDate, endDate);
                int count = (dt.Rows.Count > 0) ? Convert.ToInt32(dt.Rows[0]["ItemCount"]) : 0;
                return Json(new { count = count }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new { count = 0 }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Returns GRN Trends (date-wise GRN count for chart).
        /// </summary>
        public async Task<JsonResult> GetGRNTrendsRHK(DateTime? startDate, DateTime? endDate)
        {
            try
            {
                DataSet ds = await bal.GRNTrendsRHK(startDate, endDate);
                List<string> dates = new List<string>();
                List<int> counts = new List<int>();

                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        dates.Add(Convert.ToDateTime(dr["Date"]).ToString("MMM dd"));
                        counts.Add(Convert.ToInt32(dr["GRNCount"]));
                    }
                }
                else
                {
                    // No records → Return all dates with 0
                    DateTime currentDate = (DateTime)startDate;
                    while (currentDate <= endDate)
                    {
                        dates.Add(currentDate.ToString("MMM dd"));
                        counts.Add(0);
                        currentDate = currentDate.AddDays(1);
                    }
                }

                return Json(new { dates = dates, counts = counts }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                // Return default 0 values in case of error
                List<string> dates = new List<string>();
                List<int> counts = new List<int>();

                DateTime currentDate = (DateTime)startDate;
                while (currentDate <= endDate)
                {
                    dates.Add(currentDate.ToString("MMM dd"));
                    counts.Add(0);
                    currentDate = currentDate.AddDays(1);
                }

                return Json(new { dates = dates, counts = counts }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Returns last 10 recent GRNs with vendor, PO, invoice, and status.
        /// </summary>
        public async Task<JsonResult> RecentGRNListRHK()
        {
            try
            {
                DataSet ds = await bal.RecentGRNListRHK();
                List<GRN> grnlist = new List<GRN>();

                if (ds != null && ds.Tables.Count > 0)
                {
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        grnlist.Add(new GRN
                        {
                            GRNCode = dr["GRNCode"].ToString(),
                            Vendor = dr["VenderName"].ToString(),
                            POCode = dr["POCode"].ToString(),
                            InvoiceNo = dr["InvoiceNo"].ToString(),

                            AddedBy = dr["FullName"].ToString(),
                            AddedDate = Convert.ToDateTime(dr["AddedDate"])
                        });
                    }
                }
                return Json(new { data = grnlist }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new { data = new List<GRN>() }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Returns list of Approved QC Items in given date range.
        /// </summary>
        public async Task<JsonResult> GetApprovedItemsRHK(DateTime? startDate, DateTime? endDate)
        {
            try
            {
                DataTable dt = await bal.ApprovedItemsRHK(startDate, endDate);
                List<GRN> approvedItems = new List<GRN>();

                foreach (DataRow dr in dt.Rows)
                {
                    approvedItems.Add(new GRN
                    {
                        QualityCheckCode = dr["QualityCheckCode"].ToString(),
                        GRNCode = dr["GRNCode"].ToString(),
                        ItemName = dr["ItemName"].ToString(),
                        Quantity = Convert.ToInt32(dr["Quantity"]),
                        Status = dr["StatusName"].ToString(),
                        AddedBy = dr["AddedBy"].ToString(),
                        AddedDate = Convert.ToDateTime(dr["AddedDate"])
                    });
                }

                return Json(new { data = approvedItems }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new { data = new List<GRN>() }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Returns list of Rejected QC Items in given date range.
        /// </summary>
        public async Task<JsonResult> GetRejectedItemsRHK(DateTime? startDate, DateTime? endDate)
        {
            try
            {
                DataTable dt = await bal.RejectedItemsRHK(startDate, endDate);
                List<GRN> rejectedItems = new List<GRN>();

                foreach (DataRow dr in dt.Rows)
                {
                    rejectedItems.Add(new GRN
                    {
                        QualityCheckCode = dr["QualityCheckCode"].ToString(),
                        GRNCode = dr["GRNCode"].ToString(),
                        ItemName = dr["ItemName"].ToString(),
                        Quantity = Convert.ToInt32(dr["Quantity"]),
                        Status = dr["StatusName"].ToString(),
                        AddedBy = dr["AddedBy"].ToString(),
                        AddedDate = Convert.ToDateTime(dr["AddedDate"])
                    });
                }

                return Json(new { data = rejectedItems }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new { data = new List<GRN>() }, JsonRequestBehavior.AllowGet);
            }
        }

        // ================== Partial Views ===================

        /// <summary>
        /// Returns GRN List (for partial view) with vendor, PO, status, etc.
        /// </summary>
        public async Task<ActionResult> GetGRNListPartialRHK(DateTime? startDate, DateTime? endDate)
        {
            DataSet ds = await bal.GRNListRHK(startDate, endDate);
            List<GRN> grnlist = new List<GRN>();

            if (ds != null && ds.Tables.Count > 0)
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    grnlist.Add(new GRN
                    {
                        GRNCode = dr["GRNCode"].ToString(),
                        POCode = dr["POCode"].ToString(),
                        InvoiceNo = dr["InvoiceNo"].ToString(),
                        Vendor = dr["VenderName"].ToString(),

                        AddedBy = dr["FullName"].ToString(),
                        AddedDate = Convert.ToDateTime(dr["AddedDate"])
                    });
                }
            }

            return PartialView("_GRNListRHK", grnlist);
        }

        /// <summary>
        /// Returns all GRN Items (for partial view).
        /// </summary>
        public async Task<ActionResult> GetGRNItemsPartialRHK(DateTime? startDate, DateTime? endDate)
        {
            DataTable dt = await bal.TotalGRNItemListRHK(startDate, endDate);
            List<GRN> items = new List<GRN>();

            foreach (DataRow dr in dt.Rows)
            {
                items.Add(new GRN
                {
                    GRNCode = dr["GRNCode"].ToString(),
                    ItemName = dr["ItemName"].ToString(),
                    Quantity = Convert.ToInt32(dr["Quantity"]),
                    IsQuality = dr["ISQuality"].ToString(),
                });
            }

            return PartialView("_GRNItemsRHK", items);
        }

        /// <summary>
        /// Returns QC Assigned Items (for partial view).
        /// </summary>
        public async Task<ActionResult> GetQCListPartialRHK(DateTime? startDate, DateTime? endDate)
        {
            DataTable dt = await bal.QCAssignedItemsRHK(startDate, endDate);
            List<GRN> qcItems = new List<GRN>();

            foreach (DataRow dr in dt.Rows)
            {
                qcItems.Add(new GRN
                {
                    QualityCheckCode = dr["QualityCheckCode"].ToString(),
                    GRNCode = dr["GRNCode"].ToString(),
                    ItemName = dr["ItemName"].ToString(),
                    Quantity = Convert.ToInt32(dr["Quantity"]),
                    Status = dr["StatusName"].ToString(),
                    AddedBy = dr["AddedBy"].ToString(),
                    AddedDate = Convert.ToDateTime(dr["AddedDate"])
                });
            }

            return PartialView("_QCListRHK", qcItems);
        }

        /// <summary>
        /// Returns upcoming items (from pending POs).
        /// </summary>
        public async Task<ActionResult> GetUpcomingItemPartialRHK(DateTime? startDate, DateTime? endDate)
        {
            DataTable dt = await bal.UpcomingItemListRHK(startDate, endDate);
            List<GRN> Items = new List<GRN>();

            foreach (DataRow dr in dt.Rows)
            {
                Items.Add(new GRN
                {
                    POCode = dr["POCode"].ToString(),
                    ItemCode = dr["ItemCode"].ToString(),
                    ItemName = dr["ItemName"].ToString(),
                    OrderQuantity = Convert.ToInt32(dr["OrderedQty"]),
                    RecievedQuantity = Convert.ToInt32(dr["ReceivedQty"]),
                    PendingQuantity = Convert.ToInt32(dr["PendingQty"]),
                    ExpectedDate = Convert.ToDateTime(dr["VendorDeliveryDate"]),
                    OrderedBy = dr["AddedBy"].ToString(),
                });
            }

            return PartialView("_UpcomingItemListRHK", Items);
        }

        /// <summary>
        /// Returns Approved QC Items (for partial view).
        /// </summary>
        public async Task<ActionResult> GetApprovedItemsPartialRHK(DateTime? startDate, DateTime? endDate)
        {
            DataTable dt = await bal.ApprovedItemsRHK(startDate, endDate);
            List<GRN> approvedItems = new List<GRN>();

            foreach (DataRow dr in dt.Rows)
            {
                approvedItems.Add(new GRN
                {
                    QualityCheckCode = dr["QualityCheckCode"].ToString(),
                    GRNCode = dr["GRNCode"].ToString(),
                    ItemName = dr["ItemName"].ToString(),
                    Quantity = Convert.ToInt32(dr["Quantity"]),
                    Status = dr["StatusName"].ToString(),
                    AddedBy = dr["AddedBy"].ToString(),
                    AddedDate = Convert.ToDateTime(dr["AddedDate"])
                });
            }

            return PartialView("_ApprovedItemsRHK", approvedItems);
        }

        /// <summary>
        /// Returns Rejected QC Items (for partial view).
        /// </summary>
        public async Task<ActionResult> GetRejectedItemsPartialRHK(DateTime? startDate, DateTime? endDate)
        {
            DataTable dt = await bal.RejectedItemsRHK(startDate, endDate);
            List<GRN> rejectedItems = new List<GRN>();

            foreach (DataRow dr in dt.Rows)
            {
                rejectedItems.Add(new GRN
                {
                    QualityCheckCode = dr["QualityCheckCode"].ToString(),
                    GRNCode = dr["GRNCode"].ToString(),
                    ItemName = dr["ItemName"].ToString(),
                    Quantity = Convert.ToInt32(dr["Quantity"]),
                    Status = dr["StatusName"].ToString(),
                    AddedBy = dr["AddedBy"].ToString(),
                    AddedDate = Convert.ToDateTime(dr["AddedDate"])
                });
            }

            return PartialView("_RejectedItemsRHK", rejectedItems);
        }

        /// <summary>
        /// Returns Pending QC Items (for partial view).
        /// </summary>
        public async Task<ActionResult> GetPendingItemsPartialRHK(DateTime? startDate, DateTime? endDate)
        {
            DataTable dt = await bal.PendingItemsRHK(startDate, endDate);
            List<GRN> pendingItems = new List<GRN>();

            foreach (DataRow dr in dt.Rows)
            {
                pendingItems.Add(new GRN
                {
                    QualityCheckCode = dr["QualityCheckCode"].ToString(),
                    GRNCode = dr["GRNCode"].ToString(),
                    ItemName = dr["ItemName"].ToString(),
                    Quantity = Convert.ToInt32(dr["Quantity"]),
                    Status = dr["StatusName"].ToString(),
                    AddedBy = dr["AddedBy"].ToString(),
                    AddedDate = Convert.ToDateTime(dr["AddedDate"])
                });
            }

            return PartialView("_PendingItemsRHK", pendingItems);
        }
        #endregion Rushikesh

        #region Sayali


        // Returns the main GRN page view
        public ActionResult GRNSSG()
        {
            return View();
        }

        // Returns the partial view of approved PO list
        public ActionResult ApprovedPoSSG()
        {
            return PartialView("_ApprovedPOListSSG");
        }

        // Returns the partial view of GRN list
        public ActionResult GRNShowSSG()
        {
            return PartialView("_GRNListSSG");
        }



        // Fetches the list of approved POs asynchronously and returns JSON for datatable
        public async Task<JsonResult> ApprovedpoListSSG()
        {
            try
            {
                // Get approved PO dataset from BAL
                DataSet ds = await bal.ShowApprovedPOSSG();
                List<GRN> poList = new List<GRN>();

                if (ds?.Tables.Count > 0)
                {
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        DateTime? poDate = row["PODATE"] != DBNull.Value
                            ? Convert.ToDateTime(row["PODATE"])
                            : (DateTime?)null;

                        poList.Add(new GRN
                        {
                            POCode = row["POCode"].ToString(),
                            PODate = poDate.HasValue ? poDate.Value.ToString("yyyy-MM-dd") : null,
                            TotalAmount = row["TotalAmount"] != DBNull.Value ? Convert.ToDecimal(row["TotalAmount"]) : 0,
                            POStatus = row["POStatus"].ToString(),
                            VendorName = row["VenderName"].ToString()
                        });
                    }
                }

                return Json(new { data = poList }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { data = new List<GRN>(), error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }



        // Fetches the GRN list asynchronously with optional date filters
        public async Task<JsonResult> GRNListSSG(string fromDate, string toDate)
        {
            try
            {
                DateTime? from = null;
                DateTime? to = null;

                if (!string.IsNullOrEmpty(fromDate))
                    from = DateTime.ParseExact(fromDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);

                if (!string.IsNullOrEmpty(toDate))
                    to = DateTime.ParseExact(toDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);

                DataSet ds = await bal.ShowGRNListSSG();
                List<GRN> grnList = new List<GRN>();

                if (ds?.Tables.Count > 0)
                {
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        DateTime? grnDate = row["GRNDate"] != DBNull.Value
                            ? Convert.ToDateTime(row["GRNDate"])
                            : (DateTime?)null;

                        // ✅ Compare only date part, ignore time
                        if (from.HasValue && grnDate.HasValue && grnDate.Value.Date < from.Value.Date) continue;
                        if (to.HasValue && grnDate.HasValue && grnDate.Value.Date > to.Value.Date) continue;

                        grnList.Add(new GRN
                        {
                            GRNCode = row["GRNCode"].ToString(),
                            POCode = row["POCode"].ToString(),
                            InvoiceNo = row["InvoiceNo"].ToString(),
                            VendorName = row["VenderName"].ToString(),
                            GRNDate = grnDate.HasValue ? grnDate.Value.ToString("dd/MM/yyyy") : "",
                            QCStatus = row["QCStatus"].ToString(),
                            ShowAssignQCButton = row["QCStatus"].ToString() == "Pending"
                        });
                    }
                }

                return Json(new { data = grnList }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { data = new List<GRN>(), error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }




        // Fetches PO items for a given POCode asynchronously
        [HttpGet]
        public async Task<JsonResult> POItemsSSG(string POCode)
        {
            try
            {
                GRN objGRN = new GRN { POCode = POCode };
                DataSet ds = await bal.GetPOItemsSSG(objGRN);

                List<GRN> items = new List<GRN>();

                if (ds?.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        GRN item = new GRN
                        {
                            ItemCode = row["ItemCode"].ToString(),
                            ItemName = row["ItemName"].ToString(),
                            Quantity = row["PendingQuantity"] != DBNull.Value ? Convert.ToInt32(row["PendingQuantity"]) : 0,
                            CostPerUnit = row["CostPerUnit"] != DBNull.Value ? Convert.ToDecimal(row["CostPerUnit"]) : 0,
                            Discount = row["DiscountPercent"] != DBNull.Value ? Convert.ToDecimal(row["DiscountPercent"]) : 0,
                            GST = row["GST"].ToString(),
                            UOMName = row["UOMName"].ToString(),
                            Description = row["Description"].ToString(),
                            IsQuality = row.Table.Columns.Contains("ISQuality") ? row["ISQuality"].ToString() : string.Empty,
                            POCode = POCode,
                            DiscountPercent = row.Table.Columns.Contains("DiscountPercent") && row["DiscountPercent"] != DBNull.Value
                                              ? Convert.ToDecimal(row["DiscountPercent"]) : 0,
                            TotalAmount = row.Table.Columns.Contains("Amount") && row["Amount"] != DBNull.Value
                                              ? Convert.ToDecimal(row["Amount"]) : 0
                        };

                        items.Add(item);
                    }
                }

                return Json(new { data = items }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { data = new List<GRN>(), error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }



        // Loads the Create GRN modal view with PO and invoice info
        public async Task<ActionResult> CreateGRNSSG(string POCode)
        {
            try
            {
                GRN objGRN = new GRN { POCode = POCode };
                if (!string.IsNullOrEmpty(POCode))
                {
                    var ds = await bal.POInfoSSG(objGRN);
                    if (ds?.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                    {
                        DataRow row = ds.Tables[0].Rows[0];

                        ViewBag.POCode = row["POCode"].ToString();
                        ViewBag.PODate = row["PODATE"] != DBNull.Value
                            ? Convert.ToDateTime(row["PODATE"]).ToString("yyyy-MM-dd")
                            : string.Empty;
                        ViewBag.VendorName = row["VenderName"].ToString();
                        ViewBag.CompanyAddress = row["CompanyAddress"].ToString();
                        ViewBag.BillingAddress = row["BillingAddress"].ToString();
                        ViewBag.GRNCode = row["NewGRNCode"].ToString();
                        ViewBag.WarehouseName = row["WarehouseName"].ToString();
                        ViewBag.WareHouseId = row["WareHouseId"].ToString();

                    }
                }

                return PartialView("_CreateGRNSSG");
            }
            catch (Exception ex)
            {
                return Content("Error in CreateGRNSSG: " + ex.Message);
            }
        }

        // Saves GRN header and items,Update po and poItem status
        [HttpPost]
        public async Task<JsonResult> CreateSSG(GRN objGRN, List<GRN> Items)
        {
            var staffcode = Session["StaffCode"] as string;

            if (string.IsNullOrEmpty(staffcode))
                return Json(new { success = false, message = "Staff code not found in session. Please login again." });

            if (objGRN == null || Items == null || Items.Count == 0)
                return Json(new { success = false, message = "Invalid GRN data." });

            try
            {
                await bal.SaveGRNHeaderSSG(objGRN, staffcode);
                foreach (var item in Items)
                {
                    item.GRNCode = objGRN.GRNCode;
                    await bal.SaveGRNItemSSG(item);
                }
                await bal.UpdatePOItemStatusSSG(objGRN.POCode);
                await bal.UpdatePOStatusSSG(objGRN.POCode);

                return Json(new { success = true, message = "GRN created & PO statuses updated successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error creating GRN: " + ex.Message });
            }
        }



       


        // Fetches GRN items for display in view
        [HttpGet]
        public async Task<JsonResult> GetGRNItemsSSG(string GRNCode)
        {
            try
            {
                if (string.IsNullOrEmpty(GRNCode))
                    return Json(new { success = false, message = "GRNCode is required", items = new List<object>() }, JsonRequestBehavior.AllowGet);

                GRN objGRN = new GRN { GRNCode = GRNCode };
                var dsItems = await bal.ViewGRNItemSSG(objGRN);

                List<GRN> items = new List<GRN>();

                if (dsItems?.Tables.Count > 0 && dsItems.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow row in dsItems.Tables[0].Rows)
                    {
                        items.Add(new GRN
                        {
                            ItemName = row["ItemName"]?.ToString(),
                            UOMName = row["UOMName"]?.ToString(),
                            Description = row["Description"]?.ToString(),
                            qc = row["QC"]?.ToString(),
                            POQuantity = Convert.ToDecimal(row["POQuantity"] == DBNull.Value ? 0 : row["POQuantity"]),
                            GRNQuantity = Convert.ToDecimal(row["GRNQuantity"] == DBNull.Value ? 0 : row["GRNQuantity"]),
                            RemainingQuantity = Convert.ToDecimal(row["RemainingQuantity"] == DBNull.Value ? 0 : row["RemainingQuantity"]),
                            UnitRate = Convert.ToDecimal(row["UnitRate"] == DBNull.Value ? 0 : row["UnitRate"]),
                            Discount = Convert.ToDecimal(row["Discount"] == DBNull.Value ? 0 : row["Discount"]),
                            GST = row["GST"]?.ToString(),
                            TotalAmount = Convert.ToDecimal(row["Amount"] == DBNull.Value ? 0 : row["Amount"])
                        });
                    }
                }

                return Json(new { success = true, items }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message, items = new List<object>() }, JsonRequestBehavior.AllowGet);
            }
        }

        // Loads the view GRN modal with header details
        [HttpGet]
        public async Task<ActionResult> ViewGRNSSG(string GRNCode)
        {
            try
            {
                GRN objGRN = new GRN { GRNCode = GRNCode };
                var dsHeader = await bal.ViewGRNSSG(objGRN);

                if (dsHeader?.Tables.Count > 0 && dsHeader.Tables[0].Rows.Count > 0)
                {
                    var row = dsHeader.Tables[0].Rows[0];

                    ViewBag.GRNCode = row["GRNCode"].ToString();
                    ViewBag.POCode = row["POCode"].ToString();

                    ViewBag.PODate = row["PODate"] != DBNull.Value
                        ? Convert.ToDateTime(row["PODate"]).ToString("dd-MM-yyyy") : "";
                    ViewBag.GRNDate = row["GRNDate"] != DBNull.Value
                        ? Convert.ToDateTime(row["GRNDate"]).ToString("dd-MM-yyyy") : "";
                    ViewBag.InvoiceNo = row["InvoiceNo"].ToString();
                    ViewBag.InvoiceDate = row["InvoiceDate"] != DBNull.Value
                        ? Convert.ToDateTime(row["InvoiceDate"]).ToString("dd-MM-yyyy") : "";
                    ViewBag.VendorName = row["VenderName"].ToString();
                    ViewBag.CompanyAddress = row["CompanyAddress"].ToString();
                    ViewBag.BillingAddress = row["BillingAddress"].ToString();
                    ViewBag.ReceivedBy = row.Table.Columns.Contains("ReceivedBy") && row["ReceivedBy"] != DBNull.Value
                        ? row["ReceivedBy"].ToString()
                        : "";
                    ViewBag.DiscountPercent = row.Table.Columns.Contains("DiscountPercent") && row["DiscountPercent"] != DBNull.Value
                        ? Convert.ToDecimal(row["DiscountPercent"]) : 0;
                    ViewBag.TotalAmount = row.Table.Columns.Contains("Amount") && row["Amount"] != DBNull.Value
                        ? Convert.ToDecimal(row["Amount"]) : 0;
                    ViewBag.ShippingCharges = row.Table.Columns.Contains("ShippingCharges") && row["ShippingCharges"] != DBNull.Value
                        ? Convert.ToDecimal(row["ShippingCharges"]) : 0;
                }

                ViewBag.Mode = "View";
                return PartialView("_ViewGRNSSG");
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching GRN header: " + ex.Message, ex);
            }
        }
        [HttpGet]
        public async Task<ActionResult> GenerateGRNPDF(string GRNCode)
        {
            try
            {
                if (string.IsNullOrEmpty(GRNCode))
                    return new HttpStatusCodeResult(400, "GRNCode is required");

                // === Fetch Data ===
                GRN objGRN = new GRN { GRNCode = GRNCode };
                var dsHeader = await bal.ViewGRNSSG(objGRN);
                var dsItems = await bal.ViewGRNItemSSG(objGRN);

                if (dsHeader?.Tables.Count == 0 || dsHeader.Tables[0].Rows.Count == 0)
                    return new HttpStatusCodeResult(404, "GRN not found");

                var row = dsHeader.Tables[0].Rows[0];

                // === Header Data ===
                string poNo = row["POCode"].ToString();
                string grnNo = row["GRNCode"].ToString();
                string vendor = row.Table.Columns.Contains("VenderName") ? row["VenderName"].ToString() : "";
                string invoiceNo = row["InvoiceNo"].ToString();
                string invoiceDate = row["InvoiceDate"] != DBNull.Value ? Convert.ToDateTime(row["InvoiceDate"]).ToString("dd-MM-yyyy") : "";
                string grnDate = row["GRNDate"] != DBNull.Value ? Convert.ToDateTime(row["GRNDate"]).ToString("dd-MM-yyyy") : "";
                string poDate = row["PODate"] != DBNull.Value ? Convert.ToDateTime(row["PODate"]).ToString("dd-MM-yyyy") : "";
                string companyAddr = row["CompanyAddress"].ToString();
                string billingAddr = row["BillingAddress"].ToString();
                string receivedBy = row["ReceivedBy"].ToString();
                string warehouseName = row.Table.Columns.Contains("WarehouseName") ? row["WarehouseName"].ToString() : "Main Warehouse";

                // === Totals ===
                decimal subtotal = 0, shipping = 0, grandTotal = 0;
                if (dsItems?.Tables.Count > 0 && dsItems.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow dr in dsItems.Tables[0].Rows)
                        subtotal += dr["Amount"] != DBNull.Value ? Convert.ToDecimal(dr["Amount"]) : 0;
                }

                shipping = row.Table.Columns.Contains("ShippingCharges") && row["ShippingCharges"] != DBNull.Value
                           ? Convert.ToDecimal(row["ShippingCharges"]) : 0;

                grandTotal = subtotal + shipping;

                // === PDF Generation ===
                using (MemoryStream ms = new MemoryStream())
                {
                    Document doc = new Document(PageSize.A4, 36f, 36f, 20f, 36f);
                    PdfWriter.GetInstance(doc, ms);
                    doc.Open();

                    // === Colors ===
                    BaseColor softBlue = new BaseColor(232, 240, 254);
                    BaseColor headerBlue = new BaseColor(44, 88, 180);
                    BaseColor sectionBlue = new BaseColor(35, 76, 150);
                    BaseColor borderGray = new BaseColor(210, 210, 210);
                    BaseColor tableHeader = new BaseColor(52, 73, 94);

                    // === Fonts ===
                    Font titleFont = FontFactory.GetFont("Segoe UI", 15, Font.BOLD, headerBlue);
                    Font labelFont = FontFactory.GetFont("Segoe UI", 9, Font.BOLD, BaseColor.BLACK);
                    Font textFont = FontFactory.GetFont("Segoe UI", 9, BaseColor.BLACK);
                    Font sectionFont = FontFactory.GetFont("Segoe UI", 10, Font.BOLD, sectionBlue);
                    Font tableHeaderFont = FontFactory.GetFont("Segoe UI", 9, Font.BOLD, BaseColor.WHITE);

                    // === Title ===
                    Paragraph title = new Paragraph("GOODS RECEIPT NOTE (GRN)", titleFont)
                    {
                        Alignment = Element.ALIGN_CENTER,
                        SpacingBefore = 0f,
                        SpacingAfter = 2f
                    };
                    doc.Add(title);

                    Paragraph grnNum = new Paragraph("GRN No: " + grnNo, FontFactory.GetFont("Segoe UI", 10, Font.BOLD, headerBlue))
                    {
                        Alignment = Element.ALIGN_CENTER,
                        SpacingAfter = 10f
                    };
                    doc.Add(grnNum);

                    // === PO DETAILS ===
                    Paragraph poHeader = new Paragraph("PO DETAILS", sectionFont)
                    {
                        SpacingAfter = 3f
                    };
                    doc.Add(poHeader);

                    PdfPTable poTbl = new PdfPTable(4) { WidthPercentage = 100, SpacingAfter = 10f };
                    poTbl.SetWidths(new float[] { 1.2f, 2f, 1.2f, 2f });

                    void AddPOCell(string label, string value)
                    {
                        poTbl.AddCell(new PdfPCell(new Phrase(label, labelFont))
                        {
                            BackgroundColor = softBlue,
                            Padding = 4,
                            BorderColor = borderGray
                        });
                        poTbl.AddCell(new PdfPCell(new Phrase(value, textFont))
                        {
                            Padding = 4,
                            BorderColor = borderGray
                        });
                    }

                    AddPOCell("PO No", poNo);
                    AddPOCell("PO Date", poDate);
                    AddPOCell("Company Address", companyAddr);
                    AddPOCell("Billing Address", billingAddr);
                    AddPOCell("Vendor Name", vendor);
                    AddPOCell("", ""); // keeps table aligned
                    doc.Add(poTbl);

                    // === GRN DETAILS ===
                    Paragraph grnHeader = new Paragraph("GRN DETAILS", sectionFont)
                    {
                        SpacingAfter = 3f
                    };
                    doc.Add(grnHeader);

                    PdfPTable grnTbl = new PdfPTable(4) { WidthPercentage = 100, SpacingAfter = 10f };
                    grnTbl.SetWidths(new float[] { 1.2f, 2f, 1.2f, 2f });

                    void AddGRNCell(string label, string value)
                    {
                        grnTbl.AddCell(new PdfPCell(new Phrase(label, labelFont))
                        {
                            BackgroundColor = softBlue,
                            Padding = 4,
                            BorderColor = borderGray
                        });
                        grnTbl.AddCell(new PdfPCell(new Phrase(value, textFont))
                        {
                            Padding = 4,
                            BorderColor = borderGray
                        });
                    }

                    AddGRNCell("GRN No", grnNo);
                    AddGRNCell("GRN Date", grnDate);
                    AddGRNCell("Invoice No", invoiceNo);
                    AddGRNCell("Invoice Date", invoiceDate);
                    AddGRNCell("Received By", receivedBy);
                    AddGRNCell("Warehouse", warehouseName);
                    doc.Add(grnTbl);

                    // === ITEM DETAILS ===
                    Paragraph itemHeader = new Paragraph("GRN ITEM DETAILS", sectionFont)
                    {
                        SpacingAfter = 3f
                    };
                    doc.Add(itemHeader);

                    //PdfPTable itemTbl = new PdfPTable(8) { WidthPercentage = 100, SpacingAfter = 10f };
                    //itemTbl.SetWidths(new float[] { 0.7f, 2f, 2f, 1f, 1f, 1f, 1f, 1.2f });

                    PdfPTable itemTbl = new PdfPTable(10) { WidthPercentage = 100, SpacingAfter = 10f };
                    itemTbl.SetWidths(new float[] { 0.6f, 2f, 1.2f, 2f, 1f, 1f, 1f, 1f, 1f, 1.2f });


                    // string[] headers = { "Sr.No", "Item Name", "Description", "PO Qty", "GRN Qty", "Rate", "Discount", "Amount" };
                    string[] headers = {
                        "Sr.No", "Item Name", "UOM", "Description",
                        "PO Qty", "GRN Qty", "Rate", "GST (%)", "Discount", "Amount"
                    };

                    foreach (string h in headers)
                    {
                        itemTbl.AddCell(new PdfPCell(new Phrase(h, tableHeaderFont))
                        {
                            BackgroundColor = tableHeader,
                            HorizontalAlignment = Element.ALIGN_CENTER,
                            Padding = 4,
                            BorderColor = borderGray
                        });
                    }

                    //if (dsItems != null && dsItems.Tables[0].Rows.Count > 0)
                    //{
                    //    int i = 1;
                    //    foreach (DataRow dr in dsItems.Tables[0].Rows)
                    //    {
                    //        BaseColor bg = (i % 2 == 0) ? new BaseColor(248, 248, 248) : BaseColor.WHITE;
                    //        string rate = dr["UnitRate"] != DBNull.Value ? "₹ " + Convert.ToDecimal(dr["UnitRate"]).ToString("N2") : "₹ 0.00";
                    //        string discount = dr["Discount"] != DBNull.Value ? dr["Discount"].ToString() + " %" : "0 %";
                    //        string amount = dr["Amount"] != DBNull.Value ? "₹ " + Convert.ToDecimal(dr["Amount"]).ToString("N2") : "₹ 0.00";

                    //        itemTbl.AddCell(new PdfPCell(new Phrase(i.ToString(), textFont)) { BackgroundColor = bg, HorizontalAlignment = Element.ALIGN_CENTER, Padding = 4 });
                    //        itemTbl.AddCell(new PdfPCell(new Phrase(dr["ItemName"].ToString(), textFont)) { BackgroundColor = bg, Padding = 4 });
                    //        itemTbl.AddCell(new PdfPCell(new Phrase(dr["Description"].ToString(), textFont)) { BackgroundColor = bg, Padding = 4 });
                    //        itemTbl.AddCell(new PdfPCell(new Phrase(dr["POQuantity"].ToString(), textFont)) { BackgroundColor = bg, HorizontalAlignment = Element.ALIGN_CENTER, Padding = 4 });
                    //        itemTbl.AddCell(new PdfPCell(new Phrase(dr["GRNQuantity"].ToString(), textFont)) { BackgroundColor = bg, HorizontalAlignment = Element.ALIGN_CENTER, Padding = 4 });
                    //        itemTbl.AddCell(new PdfPCell(new Phrase(rate, textFont)) { BackgroundColor = bg, HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 4 });
                    //        itemTbl.AddCell(new PdfPCell(new Phrase(discount, textFont)) { BackgroundColor = bg, HorizontalAlignment = Element.ALIGN_CENTER, Padding = 4 });
                    //        itemTbl.AddCell(new PdfPCell(new Phrase(amount, textFont)) { BackgroundColor = bg, HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 4 });
                    //        i++;
                    //    }
                    //}
                    //doc.Add(itemTbl);

                    if (dsItems != null && dsItems.Tables[0].Rows.Count > 0)
                    {
                        int i = 1;
                        foreach (DataRow dr in dsItems.Tables[0].Rows)
                        {
                            BaseColor bg = (i % 2 == 0) ? new BaseColor(248, 248, 248) : BaseColor.WHITE;

                            string rate = dr["UnitRate"] != DBNull.Value ? "₹ " + Convert.ToDecimal(dr["UnitRate"]).ToString("N2") : "₹ 0.00";
                            string discount = dr["Discount"] != DBNull.Value ? dr["Discount"].ToString() + " %" : "0 %";
                            string amount = dr["Amount"] != DBNull.Value ? "₹ " + Convert.ToDecimal(dr["Amount"]).ToString("N2") : "₹ 0.00";

                            // New fields
                            string uom = dr.Table.Columns.Contains("UOMName") ? dr["UOMName"].ToString() : "";
                            string gst = dr.Table.Columns.Contains("GST") && dr["GST"] != DBNull.Value
                              ? dr["GST"].ToString() // no extra %
                              : "0%";



                            // Add cells to table
                            itemTbl.AddCell(new PdfPCell(new Phrase(i.ToString(), textFont)) { BackgroundColor = bg, HorizontalAlignment = Element.ALIGN_CENTER, Padding = 4 });
                            itemTbl.AddCell(new PdfPCell(new Phrase(dr["ItemName"].ToString(), textFont)) { BackgroundColor = bg, Padding = 4 });
                            itemTbl.AddCell(new PdfPCell(new Phrase(uom, textFont)) { BackgroundColor = bg, HorizontalAlignment = Element.ALIGN_CENTER, Padding = 4 });
                            itemTbl.AddCell(new PdfPCell(new Phrase(dr["Description"].ToString(), textFont)) { BackgroundColor = bg, Padding = 4 });
                            itemTbl.AddCell(new PdfPCell(new Phrase(dr["POQuantity"].ToString(), textFont)) { BackgroundColor = bg, HorizontalAlignment = Element.ALIGN_CENTER, Padding = 4 });
                            itemTbl.AddCell(new PdfPCell(new Phrase(dr["GRNQuantity"].ToString(), textFont)) { BackgroundColor = bg, HorizontalAlignment = Element.ALIGN_CENTER, Padding = 4 });
                            itemTbl.AddCell(new PdfPCell(new Phrase(rate, textFont)) { BackgroundColor = bg, HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 4 });
                            itemTbl.AddCell(new PdfPCell(new Phrase(gst, textFont)) { BackgroundColor = bg, HorizontalAlignment = Element.ALIGN_CENTER, Padding = 4 });
                            itemTbl.AddCell(new PdfPCell(new Phrase(discount, textFont)) { BackgroundColor = bg, HorizontalAlignment = Element.ALIGN_CENTER, Padding = 4 });
                            itemTbl.AddCell(new PdfPCell(new Phrase(amount, textFont)) { BackgroundColor = bg, HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 4 });

                            i++;
                        }
                    }
                    doc.Add(itemTbl);


                    // === TOTALS ===
                    PdfPTable totalTbl = new PdfPTable(2) { WidthPercentage = 40, HorizontalAlignment = Element.ALIGN_RIGHT };
                    totalTbl.SetWidths(new float[] { 1f, 1f });

                    void AddTotalRow(string label, decimal value, bool highlight = false)
                    {
                        totalTbl.AddCell(new PdfPCell(new Phrase(label, highlight ? labelFont : textFont))
                        {
                            BackgroundColor = highlight ? softBlue : BaseColor.WHITE,
                            Padding = 5,
                            BorderColor = borderGray
                        });
                        totalTbl.AddCell(new PdfPCell(new Phrase("₹ " + value.ToString("N2"), highlight ? labelFont : textFont))
                        {
                            BackgroundColor = highlight ? softBlue : BaseColor.WHITE,
                            Padding = 5,
                            BorderColor = borderGray,
                            HorizontalAlignment = Element.ALIGN_RIGHT
                        });
                    }

                    AddTotalRow("Subtotal", subtotal);
                    AddTotalRow("Shipping Charges", shipping);
                    AddTotalRow("Grand Total", grandTotal, highlight: true);

                    doc.Add(totalTbl);
                    doc.Close();

                    return File(ms.ToArray(), "application/pdf");
                }
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(500, "Error generating GRN PDF: " + ex.Message);
            }
        }


        // Fetch list of warehouses asynchronously
        [HttpGet]
        public async Task<JsonResult> GetWarehousesSSG()
        {
            try
            {
                DataSet ds = await bal.GetWarehouseSSG();
                List<GRN> warehouseList = new List<GRN>();

                if (ds?.Tables.Count > 0)
                {
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        warehouseList.Add(new GRN
                        {
                            WareHouseId = row["WareHouseId"] != DBNull.Value ? Convert.ToInt32(row["WareHouseId"]) : 0,
                            WarehouseName = row["WarehouseName"]?.ToString()
                        });
                    }
                }

                return Json(new { data = warehouseList }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { data = new List<GRN>(), error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        // Fetches items pending QC assignment for a GRN
        [HttpGet]
        public async Task<ActionResult> FetchQCSSG(string GRNCode)
        {
            try
            {
                if (string.IsNullOrEmpty(GRNCode))
                    return PartialView("_FetchQCSSG", new List<GRN>());

                GRN objGRN = new GRN { GRNCode = GRNCode };
                var ds = await bal.FetchQCItemsSSG(objGRN);

                var items = new List<GRN>();
                if (ds?.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        items.Add(new GRN
                        {
                            GRNItemCode = row["GRNItemCode"]?.ToString(),
                            ItemName = row["ItemName"]?.ToString(),
                            UOMName = row["UOMName"]?.ToString(),
                            Description = row["Description"]?.ToString(),
                            IsQuality = row["ISQuality"]?.ToString() ?? "0",
                            Quantity = row["Quantity"] != DBNull.Value ? Convert.ToInt32(row["Quantity"]) : 0
                        });
                    }
                }

                return PartialView("_FetchQCSSG", items);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "❌ Error in FetchQCSSG: " + ex.Message;
                return PartialView("_FetchQCSSG", new List<GRN>());
            }
        }

        // Assign selected GRN items to QC
        [HttpPost]
        public async Task<JsonResult> AssignQCSSG(string[] GRNItemCodes, string GRNCode)
        {
            try
            {
                // Get staff code from session
                var staffcode = Session["StaffCode"] as string;
                if (string.IsNullOrEmpty(staffcode))
                    return Json(new { success = false, message = "Staff code not found in session. Please login again." });

                if (string.IsNullOrEmpty(GRNCode) || GRNItemCodes == null || !GRNItemCodes.Any())
                    return Json(new { success = false, message = "GRNCode and items are required." });

                int insertedCount = 0;

                foreach (var itemCode in GRNItemCodes)
                {
                    GRN objGRN = new GRN
                    {
                        GRNCode = GRNCode,
                        GRNItemCode = itemCode
                    };

                    // Pass staffcode to BAL method
                    insertedCount += await bal.AssignQCSSG(objGRN, staffcode);
                }

                return Json(new
                {
                    success = insertedCount > 0,
                    message = insertedCount > 0
                        ? $"{insertedCount} items assigned to QC successfully."
                        : "No new items assigned. Items may already be in QC."
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error assigning QC: " + ex.Message });
            }
        }




        #endregion sayali

    }
}