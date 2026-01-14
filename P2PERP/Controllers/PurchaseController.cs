using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.draw;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;
using P2PLibray.Account;
using P2PLibray.Purchase;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Printing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Services.Description;
using System.Web.UI.WebControls;
using System.Xml.Linq;
using static P2PLibray.Purchase.Purchase;
using iTextColor = iTextSharp.text.BaseColor;
using iTextFont = iTextSharp.text.Font;
using iTextRectangle = iTextSharp.text.Rectangle;

namespace P2PERP.Controllers
{
    public class PurchaseController : Controller
    {
        BALPurchase bal = new BALPurchase();

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


        [Route("Purchase/SendMail")]
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
        // GET: Create PR page
        [HttpGet]
        public ActionResult CreatePRADDItemPSM()
        {
            return View();
        }

        // Generate PR code
        [HttpGet]
        public async Task<JsonResult> GeneratePRCodePSM()
        {
            BALPurchase bal = new BALPurchase();
            string newCode = await bal.GeneratePRCodePSM();
            return Json(new { success = true, prCode = newCode }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<JsonResult> CreatePRADDItemPSM(CreatePRPSM purchase)
        {
            if (purchase == null || string.IsNullOrEmpty(purchase.PRCode) || purchase.Items == null || purchase.Items.Count == 0)
            {
                return Json(new { success = false, message = "Invalid purchase requisition data." });
            }

            BALPurchase bal = new BALPurchase();
            purchase.AddedBy = Session["StaffCode"]?.ToString();
            purchase.AddedDate = DateTime.Now;

            await bal.CreatePRADDItemPSM(purchase);

            return Json(new { success = true, message = "Purchase Requisition saved successfully!" });
        }

        // Priority list
        [HttpGet]
        public async Task<JsonResult> PriorityPSM()
        {
            BALPurchase bal = new BALPurchase();
            SqlDataReader dr = await bal.PriorityPSM();
            var statusList = new List<SelectListItem>();

            if (dr.HasRows)
            {
                while (await dr.ReadAsync())
                {
                    statusList.Add(new SelectListItem
                    {
                        Value = dr["StatusId"].ToString(),
                        Text = dr["StatusName"].ToString()
                    });
                }
            }
            dr.Close();
            return Json(statusList, JsonRequestBehavior.AllowGet);
        }

        // Item Request Status
        [HttpGet]
        public async Task<JsonResult> SelectItemReqStatusPSM()
        {
            BALPurchase bal = new BALPurchase();
            SqlDataReader dr = await bal.ItemReqStatusPSM();
            var statusList = new List<SelectListItem>();

            if (dr.HasRows)
            {
                while (await dr.ReadAsync())
                {
                    statusList.Add(new SelectListItem
                    {
                        Value = dr["SubTypeId"].ToString(),
                        Text = dr["SubTypeName"].ToString()
                    });
                }
            }
            dr.Close();
            return Json(statusList, JsonRequestBehavior.AllowGet);
        }
        // Plan Names for Dropdown
        [HttpGet]
        public async Task<JsonResult> SelectPlanNamesPSM()
        {
            BALPurchase bal = new BALPurchase();
            SqlDataReader dr = await bal.AddPlanNamePSM();
            var planNameList = new List<SelectListItem>();

            if (dr.HasRows)
            {
                while (await dr.ReadAsync())
                {
                    planNameList.Add(new SelectListItem
                    {
                        Value = dr["MaterialReqPlanningCode"].ToString(),
                        Text = dr["PlanName"].ToString()
                    });
                }
            }

            dr.Close();
            return Json(planNameList, JsonRequestBehavior.AllowGet);
        }

        // 🔹 MRP Items by PlanCode (with optional date filter)
        [HttpGet]
        public async Task<JsonResult> MRPItemsListPSM(string planCode, string from = null, string to = null)
        {
            List<object> items = new List<object>();
            BALPurchase bal = new BALPurchase();

            using (SqlDataReader dr = await bal.GetMRPItemsPSM(planCode))
            {
                while (await dr.ReadAsync())
                {
                    DateTime reqDate = Convert.ToDateTime(dr["RequiredDate"]);
                    // Apply date filter if provided
                    if (!string.IsNullOrEmpty(from) && !string.IsNullOrEmpty(to))
                    {
                        DateTime fromDate = DateTime.Parse(from);
                        DateTime toDate = DateTime.Parse(to);
                        if (reqDate < fromDate || reqDate > toDate)
                            continue;
                    }

                    items.Add(new
                    {
                        ItemCode = dr["ItemCode"].ToString(),
                        ItemName = dr["ItemName"].ToString(),
                        Description = dr["Description"].ToString(),
                        UOMName = dr["UOMName"].ToString(),
                        UnitRates = dr["UnitRates"].ToString(),
                        Quantity = dr["Quantity"].ToString(),
                        RequiredDate = reqDate.ToString("yyyy-MM-dd")
                    });
                }
            }

            return Json(items, JsonRequestBehavior.AllowGet);
        }
        public async Task<JsonResult> GenerateStockRequirementPR(string from, string to)
        {
            BALPurchase bal = new BALPurchase();
            DataSet ds = await bal.ItemNamePSM();
            var itemlist = new List<object>();

            DateTime? fromDate = null;
            DateTime? toDate = null;

            if (!string.IsNullOrEmpty(from) && DateTime.TryParse(from, out DateTime fd))
                fromDate = fd;

            if (!string.IsNullOrEmpty(to) && DateTime.TryParse(to, out DateTime td))
                toDate = td;

            foreach (DataRow row in ds.Tables[0].Rows)
            {
                var requiredDate = Convert.ToDateTime(row["RequiredDate"]);

                // ✅ Apply Date Filter
                if ((fromDate == null || requiredDate >= fromDate) &&
                    (toDate == null || requiredDate <= toDate))
                {
                    itemlist.Add(new
                    {
                        ItemId = row["ItemId"].ToString(),
                        ItemCode = row["ItemCode"].ToString(),
                        ItemName = row["ItemName"].ToString(),
                        Description = row["Description"].ToString(),
                        UOMName = row["UOMName"].ToString(),
                        UnitRates = row["UnitRates"].ToString(),
                        Quantity = row["Quantity"].ToString(),
                        RequiredDate = requiredDate.ToString("yyyy-MM-dd")
                    });
                }
            }

            return Json(itemlist, JsonRequestBehavior.AllowGet);
        }

        // Delete PR Item
        [HttpPost]
        public async Task<JsonResult> DeletePRItemPSM(string ItemCode)
        {
            if (string.IsNullOrEmpty(ItemCode))
                return Json(new { success = false, message = "Invalid ItemCode" });

            BALPurchase bal = new BALPurchase();
            await bal.DeletePRItemPSM(ItemCode);
            return Json(new { success = true, message = "Item deleted successfully" });
        }
        #endregion


        #region Ashutosh

        List<Purchase> user = new List<Purchase>();
        public ActionResult Index()
        {
            return View();
        }


        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        // Controller Optional
        public async Task<ActionResult> PurchaseRequestTableAT()
        {
            var lstUserDtl = await bal.ShowDataAT();
            return View(lstUserDtl);
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Displays the Purchase Order Report page with a list of purchase orders.
        /// </summary>
        public async Task<ActionResult> PurchaseOrderReportAT()
        {
            var purchaseOrders = await bal.GetPurchaseOrdersAT();
            return View(purchaseOrders);
        }


        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Displays the RFQ report page with a list of RFQs.
        /// </summary>
        public async Task<ActionResult> RFQReportAT()
        {
            var rfqList = await bal.GetRFQReportAT();
            return View(rfqList);
        }




        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        //Controller for POItems
        [HttpGet]
        public async Task<ActionResult> GetPOItems(string poCode)
        {
            if (string.IsNullOrEmpty(poCode))
                return Json(new { success = false, message = "POCode required" }, JsonRequestBehavior.AllowGet);

            var items = await bal.GetPOItemsAT(poCode);
            return Json(new { success = true, data = items }, JsonRequestBehavior.AllowGet);
        }


        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        //Controller for PRItems
        [HttpGet]
        public async Task<JsonResult> GetPRItems(string prCode)
        {
            var bal = new BALPurchase();
            var items = await bal.GetPRItemsAT(prCode);
            return Json(items, JsonRequestBehavior.AllowGet);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        // RFQ Registered Quotation List
        //[HttpGet]
        //public async Task<ActionResult> GetRFQVendorResponses(string rfqCode)
        //{
        //    if (string.IsNullOrEmpty(rfqCode))
        //        return Json(new { success = false, message = "RFQCode is required" }, JsonRequestBehavior.AllowGet);

        //    var items = await bal.GetRFQVendorResponsesAT(rfqCode);
        //    return Json(new { success = true, data = items }, JsonRequestBehavior.AllowGet);
        //}
        [HttpGet]
        public async Task<ActionResult> GetRFQVendorResponses(string rfqCode)
        {
            if (string.IsNullOrEmpty(rfqCode))
                return Json(new { success = false, message = "RFQCode is required" }, JsonRequestBehavior.AllowGet);

            var items = await bal.GetRFQVendorResponsesAT(rfqCode);
            return Json(new { success = true, data = items }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Vaibhavi
        //  Show all RFQs
        public ActionResult ShowAllRFQsVNK()
        {
            return View();
        }

        //  Fetch all RFQs
        public async Task<ActionResult> AllRFQVNK()
        {
            var data = await bal.ShowAllRFQVNK();
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        //  View all RFQs
        public ActionResult ViewAllRFQsVNK()
        {
            return View();
        }

        //  View RFQ details by code
        public async Task<ActionResult> ViewRFQVNK(string rfqCode)
        {
            var data = await bal.ViewRFQVNK(rfqCode);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        //  Register Quotation 
        //[HttpGet]
        //public ActionResult RegisterQuotationVNK(string rfqCode, string prCode)
        //{
        //    ViewBag.RFQCode = rfqCode;
        //    ViewBag.PRCode = prCode;
        //    ViewBag.RequiredDate = "";

        //    return View();
        //}

       [HttpGet]
public async Task<ActionResult> RegisterQuotationVNK(string rfqCode, string prCode)
{
    ViewBag.RFQCode = rfqCode;
    ViewBag.PRCode = prCode;
    ViewBag.RequiredDate = "";

    // ✅ Await the BAL call
    var result = await bal.GetRFQHeaderVNK(rfqCode);

    // ✅ Cast to DataTable since BAL returns ds.Tables[0]
    var dt = result as DataTable;

    if (dt != null && dt.Rows.Count > 0)
    {
        var row = dt.Rows[0];

        if (row["RequiredDate"] != DBNull.Value)
        {
            DateTime reqDate;
            if (DateTime.TryParse(row["RequiredDate"].ToString(), out reqDate))
            {
                ViewBag.RequiredDate = reqDate.ToString("yyyy-MM-dd");
            }
        }
    }

    return View();
}



        //  Get RFQ header by RFQ code
        [HttpGet]
        public async Task<ActionResult> GetRFQHeaderVNK(string rfqCode)
        {
            var data = await bal.GetRFQHeaderVNK(rfqCode);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        //  Get vendors linked to an RFQ
        public async Task<JsonResult> GetVendorsByRFQVNK(string rfqCode)
        {
            var vendors = await bal.GetVendorsByRFQVNK(rfqCode);
            return Json(vendors, JsonRequestBehavior.AllowGet);
        }

        //  Get vendor details by vendor code
        [HttpGet]
        public async Task<JsonResult> GetVendorDetailsVNK(string vendorCode)
        {
            var result = await bal.GetVendorDetailsVNK(vendorCode);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        //  Get items for RFQ by PR code
        [HttpGet]
        public async Task<JsonResult> GetItemsForRFQVNK(string prCode)
        {
            var items = await bal.GetItemsForRFQVNK(prCode);

            var result = items.Select(x => new
            {
                ItemCode = x.ItemCode,
                ItemName = x.ItemName,
                UOMName = x.UOMName,
                Description = x.Description,
                quantity = x.Quantity
            });

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        // Get GST details of an item by item code
        public async Task<JsonResult> GetItemGSTVNK(string itemCode)
        {
            var gst = await bal.GetItemGSTVNK(itemCode);
            return Json(gst, JsonRequestBehavior.AllowGet);
        }

        // Save Register Quotation
        [HttpPost]
        public async Task<JsonResult> SaveRegisterQuotationVNK(RegisterQuotation rq)
        {

            var code = await bal.SaveRegisterQuotationVNK(rq, Session["StaffCode"].ToString());
            return Json(new { success = !string.IsNullOrEmpty(code), code });
        }

        // View Quotation 
        [HttpGet]
        public ActionResult ViewQuotationVNK(string rfqCode, string prCode)
        {
            ViewBag.RFQCode = rfqCode;
            ViewBag.PRCode = prCode;

            if (Request.IsAjaxRequest())
                return PartialView("_ViewQuotationVNK");

            return View("_ViewQuotationVNK");
        }

        // View Quotation detail 
        [HttpGet]
        public ActionResult ViewQuotationDetailVNK(string rqCode)
        {
            ViewBag.RegisterQuotationCode = rqCode;

            if (Request.IsAjaxRequest())
                return PartialView("_ViewQuotationDetailVNK");

            return View("_ViewQuotationDetailVNK");
        }

        // Get header of one quotation
        [HttpGet]
        public async Task<ActionResult> GetRQHeaderByCodeVNK(string rqCode)
        {
            var data = await bal.GetRQHeaderByCodeVNK(rqCode);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        //  Get items of one quotation
        [HttpGet]
        public async Task<ActionResult> GetRQItemsByCodeVNK(string rqCode)
        {
            var data = await bal.GetRQItemsByCodeVNK(rqCode);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        //  Purchase Orders (Approved)
        [HttpGet]
        public ActionResult PurchaseOrdersVNK()
        {
            return View();
        }

        //  Get approved purchase orders
        [HttpGet]
        public async Task<ActionResult> GetApprovedPOsVNK()
        {
            var data = await bal.GetApprovedPOsVNK();
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        //  View Purchase Order detail
        [HttpGet]
        public ActionResult ViewPurchaseOrderVNK(string poCode)
        {
            ViewBag.POCode = poCode;
            if (Request.IsAjaxRequest())
                return PartialView("_ViewPurchaseOrderVNK");
            return View("_ViewPurchaseOrderVNK");
        }

        // Get Purchase Order header
        [HttpGet]
        public async Task<ActionResult> GetPOHeaderByCodeVNK(string poCode)
        {
            var data = await bal.GetPOHeaderByCodeVNK(poCode);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        // Get Purchase Order items
        [HttpGet]
        public async Task<ActionResult> GetPOItemsByCodeVNK(string poCode)
        {
            var data = await bal.GetPOItemsByCodeVNK(poCode);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ApprovedPOsPartialVNK()
        {
            return PartialView("_ApprovedPOsPartialVNK"); // returns the partial view
        }

      

        [HttpGet]
        public async Task<FileResult> ViewApprovedPOPDF(string poCode)
        {
            // 1️⃣ Fetch PO Header from your BAL
            POHeader poHeader = await bal.GetPOHeaderByCodeVNK(poCode);

            // 2️⃣ Fetch PO Items from your BAL
            List<POItem> poItems = await bal.GetPOItemsByCodeVNK(poCode);

            if (poHeader == null || poItems == null || poItems.Count == 0)
                throw new Exception("No data found for this PO.");

            // 3️⃣ Generate PDF using your own method
            byte[] fileBytes = GenerateApprovedPOPDF(poHeader, poItems);

            // 4️⃣ Return PDF inline in browser
            Response.AppendHeader("Content-Disposition", "inline; filename=PurchaseOrder.pdf");
            return File(fileBytes, "application/pdf");
        }


        //public byte[] GenerateApprovedPOPDF(POHeader po, List<POItem> items)
        //{
        //    using (MemoryStream ms = new MemoryStream())
        //    {
        //        Document doc = new Document(PageSize.A4, 36f, 36f, 20f, 36f);
        //        PdfWriter.GetInstance(doc, ms);
        //        doc.Open();

        //        // === COLORS ===
        //        BaseColor softBlue = new BaseColor(232, 240, 254);
        //        BaseColor headerBlue = new BaseColor(44, 88, 180);
        //        BaseColor sectionBlue = new BaseColor(35, 76, 150);
        //        BaseColor borderGray = new BaseColor(210, 210, 210);
        //        BaseColor tableHeader = new BaseColor(52, 73, 94);

        //        // === FONTS ===
        //        iTextSharp.text.Font titleFont = FontFactory.GetFont("Segoe UI", 15, iTextSharp.text.Font.BOLD, headerBlue);
        //        iTextSharp.text.Font labelFont = FontFactory.GetFont("Segoe UI", 9, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
        //        iTextSharp.text.Font textFont = FontFactory.GetFont("Segoe UI", 9, BaseColor.BLACK);
        //        iTextSharp.text.Font sectionFont = FontFactory.GetFont("Segoe UI", 10, iTextSharp.text.Font.BOLD, sectionBlue);
        //        iTextSharp.text.Font tableHeaderFont = FontFactory.GetFont("Segoe UI", 9, iTextSharp.text.Font.BOLD, BaseColor.WHITE);

        //        // === TITLE ===
        //        Paragraph title = new Paragraph("PURCHASE ORDER", titleFont)
        //        {
        //            Alignment = Element.ALIGN_CENTER,
        //            SpacingBefore = 0f,
        //            SpacingAfter = 2f
        //        };
        //        doc.Add(title);

        //        Paragraph poNum = new Paragraph("PO Code: " + po?.POCode, FontFactory.GetFont("Segoe UI", 10, iTextSharp.text.Font.BOLD, headerBlue))
        //        {
        //            Alignment = Element.ALIGN_CENTER,
        //            SpacingAfter = 10f
        //        };
        //        doc.Add(poNum);

        //        // === SECTION 1: ORDER DETAILS ===
        //        Paragraph poHeader = new Paragraph("ORDER DETAILS", sectionFont)
        //        {
        //            SpacingAfter = 5f
        //        };
        //        doc.Add(poHeader);

        //        PdfPTable orderTbl = new PdfPTable(4) { WidthPercentage = 100, SpacingAfter = 10f };
        //        orderTbl.SetWidths(new float[] { 1.2f, 2f, 1.2f, 2f });

        //        void AddOrderCell(string label, string value)
        //        {
        //            orderTbl.AddCell(new PdfPCell(new Phrase(label, labelFont))
        //            {
        //                BackgroundColor = softBlue,
        //                Padding = 4,
        //                BorderColor = borderGray
        //            });
        //            orderTbl.AddCell(new PdfPCell(new Phrase(value ?? "", textFont))
        //            {
        //                Padding = 4,
        //                BorderColor = borderGray
        //            });
        //        }

        //        AddOrderCell("PO Date", po?.AddedDateVK != null ? Convert.ToDateTime(po.AddedDateVK).ToString("dd-MM-yyyy") : "");
        //        AddOrderCell("Approved/Rejected Date", po?.ApprovedRejectedDateVK != null ? Convert.ToDateTime(po.ApprovedRejectedDateVK).ToString("dd-MM-yyyy") : "");
        //        AddOrderCell("Company", po?.InvoiceToCompanyName ?? "");
        //        AddOrderCell("Delivery Address", po?.DeliveryAddress ?? "");
        //        AddOrderCell("Terms & Conditions", po?.TermConditionName ?? "");
        //        AddOrderCell("", "");
        //        doc.Add(orderTbl);

        //        // === SECTION 2: VENDOR DETAILS ===
        //        Paragraph vendorHeader = new Paragraph("VENDOR DETAILS", sectionFont)
        //        {
        //            SpacingAfter = 5f
        //        };
        //        doc.Add(vendorHeader);

        //        PdfPTable vendorTbl = new PdfPTable(4) { WidthPercentage = 100, SpacingAfter = 10f };
        //        vendorTbl.SetWidths(new float[] { 1.2f, 2f, 1.2f, 2f });

        //        void AddVendorCell(string label, string value)
        //        {
        //            vendorTbl.AddCell(new PdfPCell(new Phrase(label, labelFont))
        //            {
        //                BackgroundColor = softBlue,
        //                Padding = 4,
        //                BorderColor = borderGray
        //            });
        //            vendorTbl.AddCell(new PdfPCell(new Phrase(value ?? "", textFont))
        //            {
        //                Padding = 4,
        //                BorderColor = borderGray
        //            });
        //        }

        //        AddVendorCell("Vendor Name", po?.VendorName ?? "");
        //        AddVendorCell("Company", po?.VendorCompanyName ?? "");
        //        AddVendorCell("Contact", po?.VendorContact ?? "");
        //        AddVendorCell("Address", po?.VendorAddress ?? "");
        //        doc.Add(vendorTbl);

        //        // === SECTION 3: ORDER ITEMS ===
        //        Paragraph itemsHeader = new Paragraph("ORDER ITEM DETAILS", sectionFont)
        //        {
        //            SpacingAfter = 3f
        //        };
        //        doc.Add(itemsHeader);

        //        PdfPTable itemTbl = new PdfPTable(8) { WidthPercentage = 100, SpacingAfter = 10f };
        //        itemTbl.SetWidths(new float[] { 0.7f, 1.5f, 2f, 1f, 1f, 1f, 1f, 1.2f });

        //        string[] headers = { "Sr.No", "Item Code", "Item Name", "Qty", "Unit Price", "Discount", "GST %", "Amount" };
        //        foreach (string h in headers)
        //        {
        //            itemTbl.AddCell(new PdfPCell(new Phrase(h, tableHeaderFont))
        //            {
        //                BackgroundColor = tableHeader,
        //                HorizontalAlignment = Element.ALIGN_CENTER,
        //                Padding = 4,
        //                BorderColor = borderGray
        //            });
        //        }

        //        decimal subTotal = 0, totalShipping = 0, grandTotal = 0;
        //        int srNo = 1;

        //        foreach (var item in items)
        //        {
        //            decimal baseAmount = item.Quantity * item.CostPerUnit;
        //            decimal discountAmt = baseAmount * (item.Discount / 100m);
        //            decimal netAmount = baseAmount - discountAmt;
        //            decimal gstAmount = netAmount * (item.GSTPct / 100m);
        //            decimal totalAmount = netAmount + gstAmount;

        //            subTotal += totalAmount;
        //            totalShipping = item.ShippingCharges;
        //            grandTotal = subTotal + totalShipping;

        //            BaseColor bg = (srNo % 2 == 0) ? new BaseColor(248, 248, 248) : BaseColor.WHITE;

        //            void AddCell(string val, int align = Element.ALIGN_LEFT)
        //            {
        //                itemTbl.AddCell(new PdfPCell(new Phrase(val, textFont))
        //                {
        //                    BackgroundColor = bg,
        //                    Padding = 4,
        //                    BorderColor = borderGray,
        //                    HorizontalAlignment = align
        //                });
        //            }

        //            AddCell(srNo.ToString(), Element.ALIGN_CENTER);
        //            AddCell(item.ItemCode ?? "");
        //            AddCell(item.ItemName ?? "");
        //            AddCell(item.Quantity.ToString("N2"), Element.ALIGN_CENTER);
        //            AddCell("₹ " + item.CostPerUnit.ToString("N2"), Element.ALIGN_RIGHT);
        //            AddCell(item.Discount.ToString("N2") + " %", Element.ALIGN_RIGHT);
        //            AddCell(item.GSTPct.ToString("N2") + " %", Element.ALIGN_CENTER);
        //            AddCell("₹ " + totalAmount.ToString("N2"), Element.ALIGN_RIGHT);

        //            srNo++;
        //        }

        //        doc.Add(itemTbl);

        //        // === SECTION 4: TOTALS ===
        //        PdfPTable totalTbl = new PdfPTable(2) { WidthPercentage = 40, HorizontalAlignment = Element.ALIGN_RIGHT };
        //        totalTbl.SetWidths(new float[] { 1f, 1f });

        //        void AddTotalRow(string label, decimal value, bool highlight = false)
        //        {
        //            totalTbl.AddCell(new PdfPCell(new Phrase(label, highlight ? labelFont : textFont))
        //            {
        //                BackgroundColor = highlight ? softBlue : BaseColor.WHITE,
        //                Padding = 5,
        //                BorderColor = borderGray
        //            });
        //            totalTbl.AddCell(new PdfPCell(new Phrase("₹ " + value.ToString("N2"), highlight ? labelFont : textFont))
        //            {
        //                BackgroundColor = highlight ? softBlue : BaseColor.WHITE,
        //                Padding = 5,
        //                BorderColor = borderGray,
        //                HorizontalAlignment = Element.ALIGN_RIGHT
        //            });
        //        }

        //        AddTotalRow("Subtotal", subTotal);
        //        AddTotalRow("Shipping Charges", totalShipping);
        //        AddTotalRow("Grand Total", grandTotal, highlight: true);
        //        doc.Add(totalTbl);



        //        doc.Close();
        //        return ms.ToArray();
        //    }
        //}


        public byte[] GenerateApprovedPOPDF(POHeader po, List<POItem> items)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                Document doc = new Document(PageSize.A4, 36f, 36f, 20f, 36f);
                PdfWriter.GetInstance(doc, ms);
                doc.Open();

                // === COLORS ===
                BaseColor softBlue = new BaseColor(232, 240, 254);
                BaseColor headerBlue = new BaseColor(44, 88, 180);
                BaseColor sectionBlue = new BaseColor(35, 76, 150);
                BaseColor borderGray = new BaseColor(210, 210, 210);
                BaseColor tableHeader = new BaseColor(52, 73, 94);

                // === FONTS ===
                iTextSharp.text.Font titleFont = FontFactory.GetFont("Segoe UI", 15, iTextSharp.text.Font.BOLD, headerBlue);
                iTextSharp.text.Font labelFont = FontFactory.GetFont("Segoe UI", 9, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
                iTextSharp.text.Font textFont = FontFactory.GetFont("Segoe UI", 9, BaseColor.BLACK);
                iTextSharp.text.Font sectionFont = FontFactory.GetFont("Segoe UI", 10, iTextSharp.text.Font.BOLD, sectionBlue);
                iTextSharp.text.Font tableHeaderFont = FontFactory.GetFont("Segoe UI", 9, iTextSharp.text.Font.BOLD, BaseColor.WHITE);

                // === TITLE ===
                Paragraph title = new Paragraph("PURCHASE ORDER", titleFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingBefore = 0f,
                    SpacingAfter = 2f
                };
                doc.Add(title);

                Paragraph poNum = new Paragraph("PO Code: " + po?.POCode, FontFactory.GetFont("Segoe UI", 10, iTextSharp.text.Font.BOLD, headerBlue))
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 10f
                };
                doc.Add(poNum);

                // === SECTION 1: ORDER DETAILS ===
                Paragraph poHeader = new Paragraph("Invoice To", sectionFont)
                {
                    SpacingAfter = 5f
                };
                doc.Add(poHeader);

                PdfPTable orderTbl = new PdfPTable(4) { WidthPercentage = 100, SpacingAfter = 10f };
                orderTbl.SetWidths(new float[] { 1.2f, 2f, 1.2f, 2f });

                void AddOrderCell(string label, string value)
                {
                    orderTbl.AddCell(new PdfPCell(new Phrase(label, labelFont))
                    {
                        BackgroundColor = softBlue,
                        Padding = 4,
                        BorderColor = borderGray
                    });
                    orderTbl.AddCell(new PdfPCell(new Phrase(value ?? "", textFont))
                    {
                        Padding = 4,
                        BorderColor = borderGray
                    });
                }

                AddOrderCell("PO Date", po?.AddedDateVK != null ? Convert.ToDateTime(po.AddedDateVK).ToString("dd-MM-yyyy") : "");
                AddOrderCell("Approved/Rejected Date", po?.ApprovedRejectedDateVK != null ? Convert.ToDateTime(po.ApprovedRejectedDateVK).ToString("dd-MM-yyyy") : "");
                AddOrderCell("Company", po?.InvoiceToCompanyName ?? "");
                AddOrderCell("Delivery Address", po?.DeliveryAddress ?? "");
                //AddOrderCell("Terms & Conditions", po?.TermConditionName ?? "");
                AddOrderCell("", "");
                doc.Add(orderTbl);

                // === SECTION 2: VENDOR DETAILS ===
                Paragraph vendorHeader = new Paragraph("VENDOR DETAILS", sectionFont)
                {
                    SpacingAfter = 5f
                };
                doc.Add(vendorHeader);

                PdfPTable vendorTbl = new PdfPTable(4) { WidthPercentage = 100, SpacingAfter = 10f };
                vendorTbl.SetWidths(new float[] { 1.2f, 2f, 1.2f, 2f });

                void AddVendorCell(string label, string value)
                {
                    vendorTbl.AddCell(new PdfPCell(new Phrase(label, labelFont))
                    {
                        BackgroundColor = softBlue,
                        Padding = 4,
                        BorderColor = borderGray
                    });
                    vendorTbl.AddCell(new PdfPCell(new Phrase(value ?? "", textFont))
                    {
                        Padding = 4,
                        BorderColor = borderGray
                    });
                }

                AddVendorCell("Vendor Name", po?.VendorName ?? "");
                AddVendorCell("Company", po?.VendorCompanyName ?? "");
                AddVendorCell("Contact", po?.VendorContact ?? "");
                AddVendorCell("Address", po?.VendorAddress ?? "");
                doc.Add(vendorTbl);

                // === SECTION 3: ORDER ITEMS ===
                Paragraph itemsHeader = new Paragraph("ORDER ITEM DETAILS", sectionFont)
                {
                    SpacingAfter = 3f
                };
                doc.Add(itemsHeader);

                PdfPTable itemTbl = new PdfPTable(8) { WidthPercentage = 100, SpacingAfter = 10f };
                itemTbl.SetWidths(new float[] { 0.7f, 1.5f, 2f, 1f, 1f, 1f, 1f, 1.2f });

                string[] headers = { "Sr.No", "Item Code", "Item Name", "Qty", "Unit Price", "Discount", "GST %", "Amount" };
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

                decimal subTotal = 0, totalShipping = 0, grandTotal = 0;
                int srNo = 1;

                foreach (var item in items)
                {
                    decimal baseAmount = item.Quantity * item.CostPerUnit;
                    decimal discountAmt = baseAmount * (item.Discount / 100m);
                    decimal netAmount = baseAmount - discountAmt;
                    decimal gstAmount = netAmount * (item.GSTPct / 100m);
                    decimal totalAmount = netAmount + gstAmount;

                    subTotal += totalAmount;
                    totalShipping = item.ShippingCharges;
                    grandTotal = subTotal + totalShipping;

                    BaseColor bg = (srNo % 2 == 0) ? new BaseColor(248, 248, 248) : BaseColor.WHITE;

                    void AddCell(string val, int align = Element.ALIGN_LEFT)
                    {
                        itemTbl.AddCell(new PdfPCell(new Phrase(val, textFont))
                        {
                            BackgroundColor = bg,
                            Padding = 4,
                            BorderColor = borderGray,
                            HorizontalAlignment = align
                        });
                    }

                    AddCell(srNo.ToString(), Element.ALIGN_CENTER);
                    AddCell(item.ItemCode ?? "");
                    AddCell(item.ItemName ?? "");
                    AddCell(item.Quantity.ToString("N2"), Element.ALIGN_CENTER);
                    AddCell("₹ " + item.CostPerUnit.ToString("N2"), Element.ALIGN_RIGHT);
                    AddCell(item.Discount.ToString("N2") + " %", Element.ALIGN_RIGHT);
                    AddCell(item.GSTPct.ToString("N2") + " %", Element.ALIGN_CENTER);
                    AddCell("₹ " + totalAmount.ToString("N2"), Element.ALIGN_RIGHT);

                    srNo++;
                }

                doc.Add(itemTbl);

                // === SECTION 4: TOTALS ===
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

                AddTotalRow("Subtotal", subTotal);
                AddTotalRow("Shipping Charges", totalShipping);
                AddTotalRow("Grand Total", grandTotal, highlight: true);
                doc.Add(totalTbl);

                // === TERMS & CONDITIONS AT BOTTOM ===
                if (!string.IsNullOrEmpty(po?.TermConditionName))
                {
                    // Header
                    var tcHeader = new iTextSharp.text.Paragraph("TERMS & CONDITIONS", sectionFont)
                    {
                        SpacingBefore = 10f,
                        SpacingAfter = 5f
                    };
                    doc.Add(tcHeader);

                    // Ordered list (1, 2, 3...)
                    var tcList = new iTextSharp.text.List(iTextSharp.text.List.ORDERED, 10f);
                    string[] terms = po.TermConditionName.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (var term in terms)
                    {
                        tcList.Add(new iTextSharp.text.ListItem(term.Trim(), textFont));
                    }

                    doc.Add(tcList);
                }


                doc.Close();
                return ms.ToArray();
            }
        }


        //nurdr




        // Default landing page
        //public ActionResult Index()
        //{
        //    // Return default view
        //    return View();
        //}

        // Simple view page
        public ActionResult view()
        {
            // Return simple view
            return View();
        }



        public ActionResult PendingPOsPartialNAM()
        {
            return PartialView("_PendingPOsNAM"); // returns the partial view
        }


        // Fetch pending purchase orders (JSON result)
        [HttpGet]
        public async Task<ActionResult> GetPendingPOsNAM()
        {
            // Call BLL to get pending POs
            var data = await bal.GetPendingPOsNAM();
            // Return JSON data
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        // Open details page for a specific pending PO
        [HttpGet]
        public ActionResult ViewPendingPoDetailsNAM(string poCode)
        {
            // Pass PO code to view
            ViewBag.POCode = poCode;
            return View();
        }

        // Fetch PO header details (JSON result)
        [HttpGet]
        public async Task<ActionResult> GetPOHeaderNAM(string poCode)
        {
            // Call BLL to get PO header details
            var data = await bal.GetPOHeaderNAM(poCode);
            // Return JSON data
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        // Fetch PO items (JSON result)
        [HttpGet]
        public async Task<ActionResult> GetPOItemsNAM(string poCode)
        {
            // Call BLL to get PO item details
            var data = await bal.GetPOItemsNAM(poCode);
            // Return JSON data
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        // Approve purchase order
        [HttpPost]
        public async Task<JsonResult> ApprovePONAM(string poCode)
        {
            try
            {
                var staffcode = Session["StaffCode"] as string;

                if (string.IsNullOrEmpty(staffcode))
                    return Json(new { success = false, message = "Staff code not found in session. Please login again." });

                await bal.ApprovePONAM(poCode, staffcode);



                DataSet ds = await bal.FetchPODetailsByPOCodeForOPDFOK(poCode);

                if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
                    return Json(new { success = false, message = "No PO details found." });

                Purchase po = new Purchase();
                List<Purchase> poItems = new List<Purchase>();

                var row = ds.Tables[0].Rows[0];
                po.CompanyName = row["CompanyName"].ToString();
                po.CompanyAddress = row["CompanyAddress"].ToString();
                po.CompanyMobileNo = Convert.ToInt64(row["CompanyPhone"]);
                po.CompanyEmail = row["CompanyEmail"].ToString();
                po.Website = row["Website"].ToString();
                po.POCode = row["POCode"].ToString();
                po.AddedDate = Convert.ToDateTime(row["AddedDate"]);
                po.ShippingCharges = Convert.ToDecimal(row["ShippingCharges"]);
                po.VendorName = row["VenderName"].ToString();
                po.Address = row["VendorAddress"].ToString();
                po.MobileNo = Convert.ToInt64(row["VendorPhone"]);
                po.Email = row["VendorEmail"].ToString(); // Vendor email (to send PDF)
                po.WarehouseName = row["WarehouseName"].ToString();
                po.WarehouseAddress = row["WarehouseAddress"].ToString();
                po.WarehousePhone = Convert.ToInt64(row["WarehousePhone"]);
                po.WarehouseEmail = row["WarehouseEmail"].ToString();
                po.SubAmount = Convert.ToDecimal(row["SubAmount"]);
                po.GrandTotal = Convert.ToDecimal(row["GrandTotal"]);

                foreach (DataRow dr in ds.Tables[1].Rows)
                {
                    poItems.Add(new Purchase
                    {
                        ItemCode = dr["ItemCode"].ToString(),
                        ItemName = dr["ItemName"].ToString(),
                        Quantity = Convert.ToInt32(dr["Quantity"]),
                        Description = dr["Description"].ToString(),
                        CostPerUnit = Convert.ToDecimal(dr["CostPerUnit"]),
                        Discount = dr["Discount"].ToString(),
                        GST = dr["GST"].ToString(),
                        Amount = Convert.ToDecimal(dr["Amount"])
                    });
                }


                byte[] pdfBytes = GeneratePurchaseOrderPDF(po, poItems);


                await SendPurchaseOrderEmailAsync(po, pdfBytes);


                return Json(new { success = true, message = "PO approved and email sent successfully." }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        //send pdf code Amg
        private async Task SendPurchaseOrderEmailAsync(Purchase po, byte[] pdfBytes)
        {

            try
            {
                var newemail = WebConfigurationManager.AppSettings["MainEmail"].ToString();
                var newpass = WebConfigurationManager.AppSettings["AppPassword"].ToString();

                var fromAddress = new MailAddress(newemail.ToString(), "Procurement System");
                string fromPassword = "lhrlntigzidizmju"; // Gmail app password
                string vemail = po.Email.ToString();
                string subject = $"Purchase Order {po.POCode} Approved";
                string body = $@"
    <p>Dear {po.VendorName},</p>
    <p> kindly find attached purchase order for your immediate refernce.In return of this mail,
         kindaly share your acceptance of the po ,and We are pleased to inform you that Purchase Order:<strong>{po.POCode}</strong> has been approved.</p>
    <p>Please find the attached Purchase Order PDF for your reference.</p>
    <br/>
    <p> Best Regards,<br/>{po.CompanyName}</p>
";

                using (var smtp = new SmtpClient("smtp.gmail.com", 587)
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential(newemail, newpass)
                })
                using (var message = new MailMessage(fromAddress, new MailAddress(vemail))
                {
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                })
                {
                    var attachment = new Attachment(new MemoryStream(pdfBytes), $"PurchaseOrder_{po.POCode}.pdf", "application/pdf");
                    message.Attachments.Add(attachment);

                    await smtp.SendMailAsync(message);
                }
            }
            catch (Exception ex)
            {
                 new Exception("Error in Sending email", ex);
            }
            
        }

        // Reject purchase order
        [HttpPost]
        public async Task<JsonResult> RejectPONAM(string poCode)
        {
            try
            {
                var staffcode = Session["StaffCode"] as string;

                if (string.IsNullOrEmpty(staffcode))
                    return Json(new { success = false, message = "Staff code not found in session. Please login again." });

                var result = await bal.RejectPONAM(poCode, staffcode);
                return Json(new { success = result, message = result ? "Purchase Order rejected successfully." : "Failed to reject Purchase Order." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error rejecting PO: " + ex.Message });
            }
        }
        

        // Send PO for higher approval
        [HttpPost]
        public async Task<JsonResult> SendForApprovalNAM(string poCode)
        {
            try
            {
                var staffcode = Session["StaffCode"] as string;

                if (string.IsNullOrEmpty(staffcode))
                    return Json(new { success = false, message = "Staff code not found in session. Please login again." });

                var result = await bal.SendForApprovalNAM(poCode, staffcode);


                DataSet ds = await bal.FetchPODetailsByPOCodeForOPDFOK(poCode);

                if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
                    return Json(new { success = false, message = "No PO details found." });

                Purchase po = new Purchase();
                List<Purchase> poItems = new List<Purchase>();

                var row = ds.Tables[0].Rows[0];
                po.CompanyName = row["CompanyName"].ToString();
                po.CompanyAddress = row["CompanyAddress"].ToString();
                po.CompanyMobileNo = Convert.ToInt64(row["CompanyPhone"]);
                po.CompanyEmail = row["CompanyEmail"].ToString();
                po.Website = row["Website"].ToString();
                po.POCode = row["POCode"].ToString();
                po.AddedDate = Convert.ToDateTime(row["AddedDate"]);
                po.ShippingCharges = Convert.ToDecimal(row["ShippingCharges"]);
                po.VendorName = row["VenderName"].ToString();
                po.Address = row["VendorAddress"].ToString();
                po.MobileNo = Convert.ToInt64(row["VendorPhone"]);
                po.Email = row["VendorEmail"].ToString(); // Vendor email (to send PDF)
                po.WarehouseName = row["WarehouseName"].ToString();
                po.WarehouseAddress = row["WarehouseAddress"].ToString();
                po.WarehousePhone = Convert.ToInt64(row["WarehousePhone"]);
                po.WarehouseEmail = row["WarehouseEmail"].ToString();
                po.SubAmount = Convert.ToDecimal(row["SubAmount"]);
                po.GrandTotal = Convert.ToDecimal(row["GrandTotal"]);

                foreach (DataRow dr in ds.Tables[1].Rows)
                {
                    poItems.Add(new Purchase
                    {
                        ItemCode = dr["ItemCode"].ToString(),
                        ItemName = dr["ItemName"].ToString(),
                        Quantity = Convert.ToInt32(dr["Quantity"]),
                        Description = dr["Description"].ToString(),
                        CostPerUnit = Convert.ToDecimal(dr["CostPerUnit"]),
                        Discount = dr["Discount"].ToString(),
                        GST = dr["GST"].ToString(),
                        Amount = Convert.ToDecimal(dr["Amount"])
                    });
                }


                byte[] pdfBytes = GeneratePurchaseOrderPDF(po, poItems);


                await SendForApprovalEmailAsync(po, pdfBytes);


                return Json(new { success = true, message = "PO approved and email sent successfully." }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        private async Task SendForApprovalEmailAsync(Purchase po, byte[] pdfBytes)
        {
            var fromAddress = new MailAddress("gstprocurmenterp@gmail.com", "Procurement System");
            string fromPassword = "pbji sngj tkgz ylow"; // Gmail app password

            string subject = $"Purchase Order Approval Required — PO No: {po.POCode}";

            string body = $@"
        <p>Dear Admin,</p>

        <p>
            A new Purchase Order (<strong>PO No: {po.POCode}</strong>) amounting to 
            <strong>₹ {po.Amount:N2}</strong> has been created and requires your approval 
            since it exceeds the ₹5,00,000 limit.
        </p>

        <p>
            Kindly review the attached Purchase Order for your consideration and approval.
        </p>

        <p>
            Please review the details and approve or reject the PO at your earliest convenience.
        </p>

        <p>
            In case of any queries, feel free to reach out.
        </p>

        <br/>
        <p>
            Best regards,<br/>
           
            
        </p>
    ";

            using (var smtp = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            })
            using (var message = new MailMessage(fromAddress, new MailAddress("mullanurjaha02@gmail.com")) // <-- Replace with admin email
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            })
            {
                var attachment = new Attachment(new MemoryStream(pdfBytes), $"PurchaseOrder_{po.POCode}.pdf", "application/pdf");
                message.Attachments.Add(attachment);

                await smtp.SendMailAsync(message);
            }
        }





     


        #endregion Vaibhavi

        #region Akash
        /// <summary>
        /// Displays the Supplier Quotation page.
        /// </summary>
        /// <returns>View of SupplierQuotationAMG</returns>
        [HttpGet]
        public ActionResult SupplierQuotationAMG()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                // Log the exception here
                return new HttpStatusCodeResult(500, ex.Message);
            }
        }

        /// <summary>
        /// Returns the Pending Supplier partial view.
        /// </summary>
        /// <returns>PartialView of Pending Suppliers</returns>
        public PartialViewResult PendingSupplierAMG()
        {
            try
            {
                return PartialView("_PendingSupplierAMG");
            }
            catch (Exception ex)
            {
                // Log exception here
                Console.WriteLine("error", ex.Message.ToString());
                throw;
            }
        }

        /// <summary>
        /// Returns the Approved Supplier partial view.
        /// </summary>
        /// <returns>PartialView of Approved Suppliers</returns>
        public PartialViewResult ApprovedSupplierAMG()
        {
            try
            {
                return PartialView("_ApprovedSupplierAMG");
            }
            catch (Exception)
            {
                // Log exception here
                throw;
            }
        }

        /// <summary>
        /// Fetches all pending quotations asynchronously.
        /// </summary>
        /// <returns>JSON list of pending quotations</returns>
        public async Task<JsonResult> AllPendingQuotsDataAMG()
        {
            try
            {
                var data = await bal.GetPendingSupplierQuotationsAsyncAMG();
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // Log exception here
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Fetches quotation header details by quotation code.
        /// </summary>
        /// <param name="quotationCode">Quotation code</param>
        /// <returns>JSON of quotation header</returns>
        [HttpGet]
        public async Task<JsonResult> GetQuotationHeaderAMG(string quotationCode)
        {
            try
            {
                var result = await bal.GetQuotationHeaderAsyncAMG(quotationCode);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Fetches quotation items for a given quotation code.
        /// </summary>
        /// <param name="quotationCode">Quotation code</param>
        /// <returns>JSON of quotation items</returns>
        [HttpGet]
        public async Task<JsonResult> GetQuotationItemsAMG(string quotationCode)
        {
            try
            {
                var result = await bal.GetQuotationItemsAsyncAMG(quotationCode);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Approves a pending quotation and sends it for admin approval.
        /// </summary>
        /// <param name="quotationCode">Quotation code</param>
        /// <returns>JSON result indicating success or failure</returns>
        [HttpPost]
        public async Task<ActionResult> ApproveRequestAdminAMG(string quotationCode)
        {
            try
            {
                var StaffCode = Session["StaffCode"].ToString();

                bool result = await bal.ApproveRequestPendingQuotAdminAMG(quotationCode, StaffCode);

                if (result)
                    return Json(new { success = true, message = "Quotation successfully sent for Approval to Admin." });
                else
                    return Json(new { success = false, message = "Failed to send quotation." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Approves a quotation.
        /// </summary>
        /// <param name="quotationCode">Quotation code</param>
        /// <returns>JSON result indicating success or failure</returns>
        [HttpPost]
        public async Task<ActionResult> ApproveQuotAMG(string quotationCode)
        {
            try
            {
                var StaffCode = Session["StaffCode"].ToString();
                bool result = await bal.ApproveQuotAMG(quotationCode, StaffCode);

                if (result)
                    return Json(new { success = true, message = "Quotation approved successfully." });
                else
                    return Json(new { success = false, message = "Failed to approve quotation." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Fetches all approved quotations asynchronously.
        /// </summary>
        /// <returns>JSON list of approved quotations</returns>
        public async Task<JsonResult> AllApprovedQuotsDataAMG()
        {
            try
            {
                var data = await bal.ApprovedSupplierQuotationsAMG();
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion


        #region Shubham

            /// <summary>
            /// Loads the view for displaying all requisitions.
            /// </summary>
            public ActionResult AllRequisitionSP()
            {
                return View();
            }

            /// <summary>
            /// Returns all requisitions (SP based) in JSON format.
            /// </summary>
            public async Task<JsonResult> GetAllRequisitionJsonSP()
            {
                try
                {
                    var list = await Task.Run(() => bal.GetAllPRSP());

                    if (list == null)
                        list = new List<Purchase>();

                    return Json(list, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {

                    return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
                }
            }

            /// <summary>
            /// Returns requisition header details for given PR code.
            /// </summary>
            public async Task<JsonResult> DetailsPartialSP(string id)
            {

                if (string.IsNullOrEmpty(id))
                    return Json(new { success = false, message = "PR not found" }, JsonRequestBehavior.AllowGet);

                BALPurchase bal = new BALPurchase();
                var pr = await bal.GetPRByCodeSP(id);

                if (pr == null)
                    return Json(new { success = false, message = "PR not found" }, JsonRequestBehavior.AllowGet);

                return Json(new { success = true, data = pr }, JsonRequestBehavior.AllowGet);
            }

        /// <summary>
        /// Returns requisition header details for given PR code.
        /// </summary>
        public async Task<JsonResult> DescriptionSP(string id)
        {

            if (string.IsNullOrEmpty(id))
                return Json(new { success = false, message = "PR not found" }, JsonRequestBehavior.AllowGet);

            BALPurchase bal = new BALPurchase();
            var pr = await bal.GetPRByCodeDesSP(id);

            if (pr == null)
                return Json(new { success = false, message = "PR not found" }, JsonRequestBehavior.AllowGet);

            return Json(new { success = true, data = pr }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Returns requisition item details for given PR code.
        /// </summary>
        public async Task<JsonResult> ItemPartialSP(string id)
            {

                if (string.IsNullOrEmpty(id))
                    return Json(new { success = false, message = "PR not found" }, JsonRequestBehavior.AllowGet);

                BALPurchase bal = new BALPurchase();
                var pr = await bal.GetPRItemsSP(id);

                if (pr == null)
                    return Json(new { success = false, message = "PR not found" }, JsonRequestBehavior.AllowGet);

                return Json(new { success = true, data = pr }, JsonRequestBehavior.AllowGet);
            }

            /// <summary>
            /// Returns all pending requisitions (status = 5).
            /// </summary>
            public async Task<JsonResult> GetPendingPRSP()
            {
                try
                {
                    var list = await Task.Run(() => bal.GetPendingPRSP(5));

                    if (list == null)
                        list = new List<Purchase>();

                    return Json(list, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {

                    return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
                }
            }

            /// <summary>
            /// Returns all approved requisitions (status = 1).
            /// </summary>
            public async Task<JsonResult> GetApprovePRSP()
            {
                try
                {
                    var list = await Task.Run(() => bal.GetPendingPRSP(1));

                    if (list == null)
                        list = new List<Purchase>();

                    return Json(list, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {

                    return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
                }
            }
        /// <summary>
        /// Returns all approved requisitions (status = 1).
        /// </summary>
        public async Task<JsonResult> GetRejectedPRSP()
        {
            try
            {
                var list = await Task.Run(() => bal.GetPendingPRSP(2));

                if (list == null)
                    list = new List<Purchase>();

                return Json(list, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Updates item quantity in requisition based on PRItemId.
        /// </summary>
        [HttpPost]
            public async Task<JsonResult> UpdateItemQuantitySP(int PRItemId, int requiredQuantity)
            {
                await bal.UpdateItemQuantitySP(PRItemId, requiredQuantity);
                return Json(new { success = true });
            }

            /// <summary>
            /// Generates and returns a new PR Code.
            /// </summary>
            public async Task<JsonResult> GetNewPRCode()
            {
                Purchase model = await bal.NewPRCodeSP(); // pass empty or required value
                return Json(model, JsonRequestBehavior.AllowGet);
            }

            /// <summary>
            /// Generates and returns a new PR Item Code.
            /// </summary>
            public async Task<JsonResult> GetNewPRItemCode()
            {
                BALPurchase bal = new BALPurchase();
                var item = await bal.NewPRItemCodeSP();

                if (item != null)
                    return Json(new { success = true, PRItemCode = item.PRItemCode }, JsonRequestBehavior.AllowGet);
                else
                    return Json(new { success = false, message = "Error fetching PRItemCode." }, JsonRequestBehavior.AllowGet);
            }

            /// <summary>
            /// Returns all available items for requisition creation.
            /// </summary>
            [HttpGet]
            public async Task<JsonResult> NewItemPartialSP(int itemcatagoryid = 0)
            {
                BALPurchase bal = new BALPurchase();
                var items = await bal.GetItemsSP(itemcatagoryid); // pass the category id

                // Always return items, even if the list is empty
                return Json(new { success = true, data = items ?? new List<Purchase>() }, JsonRequestBehavior.AllowGet);
            }


            /// <summary>
            /// Returns priority/status list for requisition creation.
            /// </summary>
            [HttpGet]
            public async Task<JsonResult> GetPrioritySP()
            {
                BALPurchase bal = new BALPurchase();
                SqlDataReader dr = await bal.GetPrioritySP();

                List<SelectListItem> statusList = new List<SelectListItem>();

                if (dr.HasRows)
                {
                    while (await dr.ReadAsync())
                    {
                        statusList.Add(new SelectListItem
                        {
                            Value = dr["StatusId"].ToString(),
                            Text = dr["StatusName"].ToString()
                        });
                    }
                }

                dr.Close();

                return Json(statusList, JsonRequestBehavior.AllowGet);
            }

            /// <summary>
            /// Returns priority/status list for requisition creation.
            /// </summary>
            [HttpGet]
            public async Task<JsonResult> GetIndustryTypeSP()
            {
                BALPurchase bal = new BALPurchase();
                SqlDataReader dr = await bal.GetIndustryTypeSP();

                List<SelectListItem> statusList = new List<SelectListItem>();

                if (dr.HasRows)
                {
                    while (await dr.ReadAsync())
                    {
                        statusList.Add(new SelectListItem
                        {
                            Value = dr["ItemCategoryId"].ToString(),
                            Text = dr["ItemCategoryName"].ToString()
                        });
                    }
                }

                dr.Close();

                return Json(statusList, JsonRequestBehavior.AllowGet);
            }

            /// <summary>
            /// Loads the Create PR view.
            /// </summary>
            [HttpGet]
            public ActionResult CreatePR()
            {
                return View();
            }

        /// <summary>
        /// Creates a new Purchase Requisition (PR) via AJAX form submission.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult> CreatePR(PurchaseHeader purchase)
        {

            var validationMessage = ValidatePurchase(purchase);

            if (!string.IsNullOrEmpty(validationMessage))
            {
                // Determine which field caused the error
                string fieldName = "";
                if (string.IsNullOrWhiteSpace(purchase.PRCode))
                    fieldName = "PRCode";
                else if (purchase.RequiredDate == DateTime.MinValue || purchase.RequiredDate < DateTime.Today)
                    fieldName = "ToDate";
                else if (purchase.PriorityId <= 0)
                    fieldName = "priority";

                return Json(new { success = false, message = validationMessage, field = fieldName });
            }

            purchase.AddedBy = Session["StaffCode"].ToString();
            await bal.CreatePR(purchase);

            return Json(new { success = true });
        }
        private string ValidatePurchase(PurchaseHeader purchase)
        {
            if (purchase == null)
                return "Invalid data submitted.";

            if (string.IsNullOrWhiteSpace(purchase.PRCode))
                return "PR Code is required.";

            if (purchase.RequiredDate == DateTime.MinValue)
                return "Required Date is required.";

            if (purchase.RequiredDate < DateTime.Today)
                return "Required Date cannot be in the past.";

            if (purchase.PriorityId <= 0)
                return "Please select a valid Priority.";


            return string.Empty; // means validation passed
        }


        #endregion

        #region Omkar
        /*################################################# Vendor Management ###################################################*/

        /// <summary>
        /// Displays the vendor management view page.
        /// </summary>
        [HttpGet]
        public ActionResult VenderManagementOK()
        {
            return View();
        }

        /// <summary>
        /// Fetches all countries from the CountryStateCity API as JSON data.
        /// </summary>
        /// <returns>JSON response containing country data</returns>

        [HttpGet]
        public async Task<ActionResult> GetCountries()
        {
            // Force TLS 1.2 (required by modern APIs)
            //The remote name could not be resolved: 'api.countrystatecity.in'
            //Old TLS/SSL in .NET Framework 4.0/4.5
            //ASP.NET Framework defaults to TLS 1.0, but the API requires TLS 1.2 +.
            //You need to force your app to use TLS 1.2.

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            HttpClient client = new HttpClient();

            // Set the base URL for API
            client.BaseAddress = new Uri("https://api.countrystatecity.in/v1/");

            // Set header to accept JSON response
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // Add API key in header (required by CountryStateCity API)
            client.DefaultRequestHeaders.Add("X-CSCAPI-KEY", "YmhEenRQYTJJaXJ1VGp6TFJ4MTZkN3dYc1g2cUFacUQwWlhlbXcwVw==");

            // Send GET request to "countries" endpoint
            // API returns all countries with id, name, iso2, iso3 etc.
            var response = await client.GetAsync("countries");

            // Throw exception if request failed (status code not 200)
            response.EnsureSuccessStatusCode();

            // Read response content as JSON string
            var json = await response.Content.ReadAsStringAsync();

            // Return raw JSON string to frontend
            return Content(json, "application/json");
        }

        /// <summary>
        /// Fetches states for a specific country from the CountryStateCity API.
        /// </summary>
        /// <param name="countrycode">The ISO2 country code (e.g., US, IN)</param>
        /// <returns>JSON response containing state data for the specified country</returns>

        public async Task<ActionResult> GetState(string countrycode)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://api.countrystatecity.in/v1/");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("X-CSCAPI-KEY", "YmhEenRQYTJJaXJ1VGp6TFJ4MTZkN3dYc1g2cUFacUQwWlhlbXcwVw==");
            // Send GET request to "countries/{iso2}/states" endpoint
            // countryCode = iso2 code of selected country (e.g., IN for India)
            var response = await client.GetAsync($"countries/{countrycode}/states");

            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync(); // Read JSON response
            return Content(json, "application/json");

        }

        /// <summary>
        /// Fetches cities for a specific state and country from the CountryStateCity API.
        /// </summary>

        public async Task<ActionResult> GetCities(string countryCode, string stateCode)
        {
            HttpClient client = new HttpClient(); // Create HttpClient

            client.BaseAddress = new Uri("https://api.countrystatecity.in/v1/");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("X-CSCAPI-KEY", "YmhEenRQYTJJaXJ1VGp6TFJ4MTZkN3dYc1g2cUFacUQwWlhlbXcwVw==");

            // Send GET request to "countries/{countryIso2}/states/{stateIso2}/cities"
            // API returns all cities for that state
            var response = await client.GetAsync($"countries/{countryCode}/states/{stateCode}/cities");

            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();


            return Content(json, "application/json");
        }

        /// <summary>
        /// Fetches industry types for vendor registration from the database.
        /// <returns>JSON result containing list of industry types</returns>
        /// </summary>

        public async Task<JsonResult> FetchIndustryType()
        {
            DataSet ds = new DataSet();
            List<Purchase> lstType = new List<Purchase>();
            ds = await bal.FetchIndustryTypeOK();
            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
            {
                Purchase itype = new Purchase();
                itype.IndustryTypeId = Convert.ToInt32(ds.Tables[0].Rows[i]["IndustryTypeId"].ToString());
                itype.IndustryType = ds.Tables[0].Rows[i]["IndustryType"].ToString();
                lstType.Add(itype);
            }
            return Json(new { lsttype = lstType }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Fetches banks and their Swift codes from the database.
        /// <returns>JSON result containing list of banks with Swift codes</returns>
        /// </summary>

        public async Task<JsonResult> FetchBankAndSwiftCodeOK()
        {
            List<Purchase> lstBank = new List<Purchase>();
            DataSet ds = await bal.FetchBankAndSwiftCodeOK();
            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
            {
                Purchase bank = new Purchase();
                bank.BankId = Convert.ToInt32(ds.Tables[0].Rows[i]["BankId"].ToString());
                bank.BankName = ds.Tables[0].Rows[i]["BankName"].ToString();
                bank.SwiftCode = ds.Tables[0].Rows[i]["SwiftCode"].ToString();
                lstBank.Add(bank);
            }
            return Json(new { banks = lstBank }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Fetches branches and IFSC codes for a specific bank from the database.
        /// <returns>JSON result containing list of branches with IFSC codes</returns>
        /// </summary>

        public async Task<JsonResult> FetchBranchAndIFSCCodeOK(int BankId)
        {
            List<Purchase> lstBranch = new List<Purchase>();
            DataSet ds = await bal.FetchBranchAndIFSCCodeOK(BankId);
            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
            {
                Purchase branch = new Purchase();
                branch.BranchId = Convert.ToInt32(ds.Tables[0].Rows[i]["BranchId"].ToString());
                branch.BranchName = ds.Tables[0].Rows[i]["Branch"].ToString();
                branch.IFSCCode = ds.Tables[0].Rows[i]["IFSCCode"].ToString();
                lstBranch.Add(branch);
            }
            return Json(new { Branch = lstBranch }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Saves vendor details including vendor information, company details, and account information.
        /// <returns>JSON result indicating success or failure of the operation</returns>
        /// </summary>

        [HttpPost]
        public async Task<ActionResult> SaveVendorDetails(Purchase p)
        {
            try
            {
                int VendorMaxId = 0;
                int VendorcompanyMaxId = 0;
                string satffcode = Session["StaffCode"].ToString();
                string date = DateTime.Now.ToString();
                DataSet ds = await bal.FetchVendorandVendorCompantMaxIdOK();
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    VendorMaxId = Convert.ToInt32(ds.Tables[0].Rows[i]["VendorMaxID"].ToString());
                }
                for (int i = 0; i < ds.Tables[1].Rows.Count; i++)
                {
                    VendorcompanyMaxId = Convert.ToInt32(ds.Tables[1].Rows[i]["VendorCompanyMaxId"].ToString());
                }
                string VendorCode = "VEN" + (VendorMaxId + 1).ToString("D3");
                string VendorCompanyCode = "VCC" + (VendorcompanyMaxId + 1).ToString("D3");
                // Purchase vendor = new Purchase();
                p.VendorCode = VendorCode;
                p.VendorCompanyCode = VendorCompanyCode;
                p.StaffCode = satffcode;
                p.AddedDateString = date;
                bool issaved = await bal.SaveVendor(p);
                if (issaved)
                {
                    var message = "Vendor saved successfully!";
                    return Json(new { success = true, message });
                }
                else
                {
                    var message = "Vendor not saved, please try again!";
                    return Json(new { success = false, message });
                }
            }
            catch (Exception ex)
            {

                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Approves a pending vendor that was registered through RFQ (Request for Quotation).
        /// <returns>JSON result indicating success or failure of the approval operation</returns>
        /// </summary>

        [HttpPost]
        public async Task<JsonResult> ApproveVendorOK(int VendorId)
        {
            string staffcode = Session["StaffCode"].ToString();
            bool isSave = await bal.ApproveVendorOK(VendorId, staffcode);
            if (isSave)
            {
                var message = "Vendor Approved successfully!";
                return Json(new { success = true, message });
            }
            else
            {
                var message = "Vendor not Approved....???, please try again!";
                return Json(new { success = false, message });
            }

        }

        /// <summary>
        /// Fetches pending vendor approvals that came from RFQ (Request for Quotation).
        /// <returns>JSON result containing list of pending vendors</returns>
        /// </summary>

        [HttpGet]
        public async Task<JsonResult> FetchPendingVendorOK()
        {
            DataSet ds = new DataSet();
            List<Purchase> Venderlist = new List<Purchase>();
            ds = await bal.FetchPendingVendorOK();
            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
            {
                Purchase ven = new Purchase();
                ven.SRNO = Convert.ToInt32(ds.Tables[0].Rows[i]["SRNO"].ToString());
                ven.VendorId = Convert.ToInt32(ds.Tables[0].Rows[i]["VendorId"].ToString());
                ven.VendorName = ds.Tables[0].Rows[i]["VenderName"].ToString();
                ven.MobileNo = Convert.ToInt64(ds.Tables[0].Rows[i]["MobileNo"].ToString());
                ven.AlternateNo = Convert.ToInt64(ds.Tables[0].Rows[i]["AlternateNo"].ToString());
                ven.Email = ds.Tables[0].Rows[i]["Email"].ToString();
                ven.Address = ds.Tables[0].Rows[i]["Address"].ToString();
                ven.CompanyEmail = ds.Tables[0].Rows[i]["CompanyEmail"].ToString();
                ven.CompanyName = ds.Tables[0].Rows[i]["CompanyName"].ToString();
                ven.CompanyMobileNo = Convert.ToInt64(ds.Tables[0].Rows[i]["CompanyMobileNo"].ToString());
                ven.CompanyAlternateNo = Convert.ToInt64(ds.Tables[0].Rows[i]["CompanyAlternetNo"].ToString());
                ven.CompanyAddress = ds.Tables[0].Rows[i]["CompanyAddress"].ToString();
                ven.IndustryTypeId = Convert.ToInt32(ds.Tables[0].Rows[i]["IndustryType_Id"].ToString());
                ven.IndustryType = ds.Tables[0].Rows[i]["Industry_Type"].ToString();
                ven.CountryCode = ds.Tables[0].Rows[i]["CountryCode"].ToString();
                ven.StateCode = ds.Tables[0].Rows[i]["StateCode"].ToString();
                ven.CityId = Convert.ToInt32(ds.Tables[0].Rows[i]["CityId"].ToString());
                ven.BankId = Convert.ToInt32(ds.Tables[0].Rows[i]["BankId"].ToString());
                ven.BankName = ds.Tables[0].Rows[i]["BankName"].ToString();
                ven.SwiftCode = ds.Tables[0].Rows[i]["SwiftCode"].ToString();
                ven.BranchId = Convert.ToInt32(ds.Tables[0].Rows[i]["BranchId"].ToString());
                ven.BranchName = ds.Tables[0].Rows[i]["BranchName"].ToString();
                ven.IFSCCode = ds.Tables[0].Rows[i]["IFSCCode"].ToString();
                ven.AccountNumber = Convert.ToInt64(ds.Tables[0].Rows[i]["AccountNumber"].ToString());
                ven.AddedBy = ds.Tables[0].Rows[i]["Created_By"].ToString();
                ven.AddedDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["AddedDate"].ToString());
                ven.AddedDateString = ven.AddedDate.ToString("yyyy/MM/dd");   //ToString("dd/MM/yyyy");
                Venderlist.Add(ven);
            }
            return Json(new { data = Venderlist }, JsonRequestBehavior.AllowGet);

        }

        /*################################################ Purchase Order #########################################################################*/
        /// <summary>
        /// Shows the view for displaying selected quotation lists.
        /// </summary>

        [HttpGet]
        public ActionResult SelectedQuotationListShowOK()
        {
            return View();
        }

        /// <summary>
        /// Retrieves the list of selected quotations for display in data tables.
        /// <returns>JSON result containing list of selected quotations with details</returns>
        /// </summary>

        [HttpGet]
        public async Task<JsonResult> ReciveQuotationListOK()
        {
            DataSet ds = new DataSet();
            List<Purchase> selectedQuotationLst = new List<Purchase>();
            ds = await bal.ReciveQuotationListOK();
            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
            {
                Purchase quotation = new Purchase();
                quotation.SRNO = Convert.ToInt32(ds.Tables[0].Rows[i]["SRNO"].ToString());
                quotation.QuotationID = ds.Tables[0].Rows[i]["QuotationID"].ToString();
                quotation.QuotationDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["QuotationDate"]);
                quotation.QuotationDateString = quotation.QuotationDate.ToString("yyyy-MM-dd");
                quotation.VendorName = ds.Tables[0].Rows[i]["VenderName"].ToString();
                quotation.CompanyName = ds.Tables[0].Rows[i]["CompanyName"].ToString();
                quotation.TotalAmount = Convert.ToDecimal(ds.Tables[0].Rows[i]["TotalAmount"].ToString());
                quotation.RFQCode = ds.Tables[0].Rows[i]["RFQCode"].ToString();
                quotation.VendorCode = ds.Tables[0].Rows[i]["VendorCode"].ToString();
                quotation.RequiredDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["RequiredDate"].ToString());
                quotation.RequiredDateString = quotation.RequiredDate.ToString("yyyy-MM-dd");
                quotation.VendorDeliveryDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["VendorDeliveryDate"]);
                quotation.VendorDeliveryDateString = quotation.VendorDeliveryDate.ToString("yyyy-MM-dd");
                quotation.ShippingCharges = Convert.ToDecimal(ds.Tables[0].Rows[i]["ShippingCharges"].ToString());
                quotation.Priority = ds.Tables[0].Rows[i]["Priority"].ToString();
                quotation.GST = ds.Tables[0].Rows[i]["GST"].ToString();
                quotation.SubAmount = Convert.ToDecimal(ds.Tables[0].Rows[i]["SubAmount"].ToString());
                selectedQuotationLst.Add(quotation);

            }
            return Json(new { data = selectedQuotationLst }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Fetches items for a specific selected quotation by RQCode.
        /// <returns>JSON result containing list of quotation items with details</returns>
        /// </summary>

        public async Task<JsonResult> FetchSelectedQuotationItemsOK(string RQCode)
        {
            try
            {
                DataSet ds = new DataSet();
                List<Purchase> lstRFQItems = new List<Purchase>();

                // Call BAL
                ds = await bal.FetchSelectedQuotationItemsOK(RQCode);

                if (ds == null || ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
                {
                    return Json(new { success = false, message = "No items found for given RQCode." }, JsonRequestBehavior.AllowGet);
                }

                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    Purchase items = new Purchase
                    {
                        SRNO = Convert.ToInt32(ds.Tables[0].Rows[i]["SRNO"].ToString()),
                        RegisterQuotationCode = ds.Tables[0].Rows[i]["RegisterQuotationCode"].ToString(),
                        ItemCode = ds.Tables[0].Rows[i]["ItemCode"].ToString(),
                        ItemName = ds.Tables[0].Rows[i]["ItemName"].ToString(),
                        Description = ds.Tables[0].Rows[i]["Description"].ToString(),
                        Quantity = Convert.ToInt32(ds.Tables[0].Rows[i]["Quantity"].ToString()),
                        UOM = ds.Tables[0].Rows[i]["UOM"].ToString(),
                        CostPerUnit = Convert.ToDecimal(ds.Tables[0].Rows[i]["CostPerUnit"].ToString()),
                        Discount = ds.Tables[0].Rows[i]["Discount"].ToString(),
                        GST = ds.Tables[0].Rows[i]["GST"].ToString(),
                        Amount = Convert.ToDecimal(ds.Tables[0].Rows[i]["Amount"].ToString())
                    };
                    lstRFQItems.Add(items);
                }

                return Json(new { success = true, items = lstRFQItems }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error fetching quotation items", error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        /// <summary>
        /// Opens the Create Purchase Order form view for a specific quotation.
        /// <returns>View for creating purchase order</returns>
        /// </summary>


        //string quotationID
        public ActionResult CreatePOOK()
        {
            HttpCookie cookie = Request.Cookies["SelectedQuotationID"];
            string quotationId = null;

            if (cookie != null)
            {
                quotationId = cookie.Value;
            }
            Session["quotationID"] = quotationId;
            return View();
        }

        /// <summary>
        /// Retrieves all necessary data for creating a purchase order including general details, vendor details, company details, and PO items.
        ///Get All Create PO Data with General details ,vendor Details and Ourcompany Details and Po Items
        /// <returns>JSON result containing PO creation data</returns>
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetCreatePoDetailsOK()
        {
            //Create PO Code 
            //fetch Max Id For PO Code
          //  int pomaxid = 0;

            string quotationID = Session["quotationID"].ToString();
            DataSet ds = await bal.GetQuotationAllDataOK(quotationID);

            //for (int i = 0; i < ds.Tables[4].Rows.Count; i++)
            //{
            //    pomaxid = Convert.ToInt32(ds.Tables[4].Rows[i]["MAXID"].ToString());
            //}

            //Create PO Code
           // string POcode = "PO" + (pomaxid + 1).ToString("D3");

            // 1️⃣ Quotation Header
            List<Purchase> lstHeader = new List<Purchase>();
            try
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    Purchase header = new Purchase();

                    header.QuotationID = ds.Tables[0].Rows[i]["QuotationNo"].ToString();
                    header.QuotationDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["QuotationDate"]);
                    header.QuotationDateString = header.QuotationDate.ToString("yyyy-MM-dd");
                    header.ApprovedBy = ds.Tables[0].Rows[i]["ApprovedBy"].ToString();
                    header.ApprovedDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["ApprovedDate"]);
                    header.OriginalApprovedDate = header.ApprovedDate.ToString("yyyy-MM-dd");
                    header.VendorName = ds.Tables[0].Rows[i]["VenderName"].ToString();
                    header.CompanyName = ds.Tables[0].Rows[i]["CompanyName"].ToString();
                    header.CompanyAddress = ds.Tables[0].Rows[i]["CompanyAddress"].ToString();
                    header.MobileNo = Convert.ToInt64(ds.Tables[0].Rows[i]["VendorContactNo"].ToString());
                    header.Email = ds.Tables[0].Rows[i]["VendorEmail"].ToString();
                    header.AccountNumber = Convert.ToInt64(ds.Tables[0].Rows[i]["VendorAccountNo"].ToString());
                    header.IFSCCode = ds.Tables[0].Rows[i]["IFSCCode"].ToString();
                    header.SwiftCode = ds.Tables[0].Rows[i]["SwiftCode"].ToString();
                    header.DeliveryAddress = ds.Tables[0].Rows[i]["DeliveryAddress"].ToString();
                    header.ShippingCharges = Convert.ToDecimal(ds.Tables[0].Rows[i]["TransportationCharges"].ToString());
                    header.TotalAmount = Convert.ToDecimal(ds.Tables[0].Rows[i]["TotalAmount"]);
                    header.SubAmount = Convert.ToDecimal(ds.Tables[0].Rows[i]["Amount"]);
                    header.BillingAddress = ds.Tables[0].Rows[i]["BillingAddress"].ToString();

                    lstHeader.Add(header);
                }
            }
            catch (Exception exx)
            {
                System.Diagnostics.Debug.WriteLine("Error in TermsConditions loop: " + exx.Message);
            }

            // 2️⃣ Accountant Staff
            List<Purchase> lstStaff = new List<Purchase>();
            try
            {
                for (int i = 0; i < ds.Tables[1].Rows.Count; i++)
                {
                    var staff = new Purchase
                    {
                        StaffCode = ds.Tables[1].Rows[i]["StaffCode"].ToString(),
                        AccountEmail = ds.Tables[1].Rows[i]["EmailAddress"].ToString()
                    };
                    lstStaff.Add(staff);
                }
            }
            catch (Exception exstaff)
            {
                System.Diagnostics.Debug.WriteLine("Error in TermsConditions loop: " + exstaff.Message);
            }

            // 3️⃣ Terms & Conditions
            List<Purchase> lstTerms = new List<Purchase>();
            try
            {
                for (int i = 0; i < ds.Tables[2].Rows.Count; i++)
                {
                    var term = new Purchase
                    {
                        TermConditionId = Convert.ToInt32(ds.Tables[2].Rows[i]["TermConditionId"]),
                        TermConditionName = ds.Tables[2].Rows[i]["TermConditionName"].ToString()
                    };
                    lstTerms.Add(term);
                }
            }
            catch (Exception exterm)
            {
                System.Diagnostics.Debug.WriteLine("Error in TermsConditions loop: " + exterm.Message);
            }

            // 4️⃣ RFQ / Items
            List<Purchase> lstRFQItems = new List<Purchase>();
            try
            {
                for (int i = 0; i < ds.Tables[3].Rows.Count; i++)
                {
                    var item = new Purchase
                    {
                        SRNO = Convert.ToInt32(ds.Tables[3].Rows[i]["SRNO"]),
                        RegisterQuotationItemCode = ds.Tables[3].Rows[i]["RegisterQuotationItemCode"].ToString(),
                        ItemCode = ds.Tables[3].Rows[i]["ItemCode"].ToString(),
                        ItemName = ds.Tables[3].Rows[i]["ItemName"].ToString(),
                        Description = ds.Tables[3].Rows[i]["Description"].ToString(),
                        Quantity = Convert.ToInt32(ds.Tables[3].Rows[i]["Quantity"]),
                        UOM = ds.Tables[3].Rows[i]["UOMName"].ToString(),
                        CostPerUnit = Convert.ToDecimal(ds.Tables[3].Rows[i]["CostPerUnit"]),
                        Discount = ds.Tables[3].Rows[i]["Discount"].ToString(),
                        GST = ds.Tables[3].Rows[i]["GST"].ToString(),
                        Amount = Convert.ToDecimal(ds.Tables[3].Rows[i]["Amount"])
                    };
                    lstRFQItems.Add(item);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error in TermsConditions loop: " + ex.Message);
                //throw ex;
            }

            // Combine into one JSON object
            var jsonData = new
            {
                QuotationHeader = lstHeader,
                AccountantStaff = lstStaff,
                TermsConditions = lstTerms,
                Items = lstRFQItems
            };

            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// Saves a purchase order with its associated items.
        /// <returns>JSON result indicating success or failure of the save operation</returns>
        /// </summary>

        [HttpPost]
        public async Task<ActionResult> SavePOOK(Purchase model, string POItems,string Itemslst)
        {
            HttpCookie cookie = Request.Cookies["POMode"];
            if(cookie!=null)
            {
                try
                {
                    Session["JITPOSave"] = "OK";
                    string staffcode = Session["StaffCode"].ToString();
                    model.StaffCode = staffcode;
                    if (TempData["StockReqirementId"] != null)
                    {
                        model.StockReqirementId = Convert.ToInt32(TempData["StockReqirementId"]);
                    }
                    bool issaved = await bal.SaveJITPOOK(model);
                    if (issaved)
                    {
                        string POCode = string.Empty;
                        SqlDataReader rd=await bal.FetchMaxPoCodeToAttachmentok();
                        if(rd.Read())
                        {
                            POCode = rd["MaxPOCode"].ToString();
                            rd.Close();
                        }
                       
                        // ✅ Generate PDF
                        byte[] pdfBytes = await GeneratePOPDFAttachment(POCode);

                        // ✅ Send Email 
                        await SendPOEmailAsync(POCode, pdfBytes);

                        var message = "Purchase Order Save Succesfully!";
                        return Json(new { success = true, message, sessionValue = Session["JITPOSave"] });
                    }
                    else
                    {
                        var message = "Purchase Order Not  Save Succesfully!, please try again!";
                        return Json(new { success = false, message });
                    }

                }
                catch (Exception ex)
                {

                    return Json(new { success = false, message = ex.Message });
                }
            }
            else
            {
                try
                {
                    string staffcode = Session["StaffCode"].ToString();
                    model.StaffCode = staffcode;
                    bool issaved = await bal.SavePOOK(model);
                    if (issaved)
                    {
                        var message = "Purchase Order Save Succesfully!";
                        return Json(new { success = true, message });
                    }
                    else
                    {
                        var message = "Purchase Order Not  Save Succesfully!, please try again!";
                        return Json(new { success = false, message });
                    }

                }
                catch (Exception ex)
                {

                    return Json(new { success = false, message = ex.Message });
                }
            }
        }

        /// <summary>
        /// Displays the purchase order history view.
        /// </summary>
        /// <returns>View for purchase order history</returns>

        [HttpGet]
        public ActionResult PurchaseOrederHistoryOk()
        {
            return View();
        }
        /// <summary>
        /// Fetches the purchase order history data for display.
        /// <returns>JSON result containing list of purchase orders with history details</returns>
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> FetchPOHistoryOK()
        {
            List<Purchase> lstPo = new List<Purchase>();
            DataSet ds = await bal.FetchPOHistoryOK();
            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
            {
                Purchase po = new Purchase();
                po.SRNO = Convert.ToInt32(ds.Tables[0].Rows[i]["SRNO"].ToString());
                po.POCode = ds.Tables[0].Rows[i]["POCode"].ToString();
                po.AddedDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["AddedDate"].ToString());
                po.AddedDateString = po.AddedDate.ToString("yyyy/MM/dd"); ;
                po.VendorName = ds.Tables[0].Rows[i]["VenderName"].ToString();
                po.CompanyName = ds.Tables[0].Rows[i]["CompanyName"].ToString();
                po.TotalAmount = Convert.ToDecimal(ds.Tables[0].Rows[i]["TotalAmount"].ToString());
                po.StateName = ds.Tables[0].Rows[i]["StatusName"].ToString();
                lstPo.Add(po);
            }
            return Json(new { data = lstPo }, JsonRequestBehavior.AllowGet);
        }

        /*#####################################Show PO PDF Logic################################################*/
        /// <summary>
        /// Generates a purchase order PDF file for the given PO code.
        /// Fetches purchase order details and items from the database, 
        /// maps them into model objects, and renders a formatted PDF.
        /// </summary>
        /// <returns>
        /// A FileResult containing the generated purchase order PDF to be displayed inline in the browser.
        /// </returns>
        public async Task<FileResult> GeneratePOPDF(string POCode)
        {
            DataSet ds = await bal.FetchPODetailsByPOCodeForOPDFOK(POCode);

            // Map PO details
            Purchase po = new Purchase();
            List<Purchase> poItems = new List<Purchase>();

            // Company, Vendor, ShipTo
            if (ds.Tables[0].Rows.Count > 0)
            {
                var row = ds.Tables[0].Rows[0];
                po.CompanyName = row["CompanyName"].ToString();
                po.CompanyAddress = row["CompanyAddress"].ToString();
                po.CompanyMobileNo = Convert.ToInt64(row["CompanyPhone"]);
                po.CompanyEmail = row["CompanyEmail"].ToString();
                po.Website = row["Website"].ToString();
                po.POCode = row["POCode"].ToString();
                po.AddedDate = Convert.ToDateTime(row["AddedDate"]);
                po.ShippingCharges = Convert.ToDecimal(row["ShippingCharges"]);
                // po.TotalAmount = Convert.ToDecimal(row["TotalAmount"]);
                po.VendorName = row["VenderName"].ToString();
                po.Address = row["VendorAddress"].ToString();
                po.MobileNo = Convert.ToInt64(row["VendorPhone"]);
                po.Email = row["VendorEmail"].ToString();
                po.WarehouseName = row["WarehouseName"].ToString();
                po.WarehouseAddress = row["WarehouseAddress"].ToString();
                po.WarehousePhone = Convert.ToInt64(row["WarehousePhone"]);
                po.WarehouseEmail = row["WarehouseEmail"].ToString();
                po.SubAmount = Convert.ToDecimal(row["SubAmount"]);
                po.GrandTotal = Convert.ToDecimal(row["GrandTotal"]);
            }

            // Items
            foreach (DataRow dr in ds.Tables[1].Rows)
            {
                poItems.Add(new Purchase
                {
                    ItemCode = dr["ItemCode"].ToString(),
                    ItemName = dr["ItemName"].ToString(),
                    Quantity = Convert.ToInt32(dr["Quantity"]),
                    Description = dr["Description"].ToString(),
                    CostPerUnit = Convert.ToDecimal(dr["CostPerUnit"]),
                    Discount = dr["Discount"].ToString(),
                    GST = dr["GST"].ToString(),
                    Amount = Convert.ToDecimal(dr["Amount"])
                });
            }

            // ==== Generate PDF ====
            byte[] fileBytes = GeneratePurchaseOrderPDF(po, poItems);
            //return File(fileBytes, "application/pdf", $"PurchaseOrder_{po.POCode}.pdf");
            Response.AppendHeader("Content-Disposition", "inline; filename=PurchaseOrder.pdf");
            return File(fileBytes, "application/pdf");


        }

        public byte[] GeneratePurchaseOrderPDF(Purchase po, List<Purchase> poItems)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                Document doc = new Document(PageSize.A4, 25, 25, 30, 30);
                PdfWriter.GetInstance(doc, ms);
                doc.Open();

                // ===== Load Unicode Font (supports ₹) =====
                string fontsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "arialuni.ttf");
                // if (!File.Exists(fontsPath))
                fontsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "segoeui.ttf"); // fallback

                BaseFont bf = BaseFont.CreateFont(fontsPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                var titleFont = new iTextSharp.text.Font(bf, 20, iTextSharp.text.Font.BOLD);
                var boldFont = new iTextSharp.text.Font(bf, 11, iTextSharp.text.Font.BOLD);
                var headerFont = new iTextSharp.text.Font(bf, 10, iTextSharp.text.Font.BOLD, BaseColor.WHITE);
                var normalFont = new iTextSharp.text.Font(bf, 9);

                // ===== Title & Date Box ======
                PdfPTable headerTable = new PdfPTable(2);
                headerTable.WidthPercentage = 100;
                headerTable.SetWidths(new float[] { 70f, 30f });

                PdfPCell titleCell = new PdfPCell(new Phrase("PURCHASE ORDER", titleFont));
                titleCell.Border = iTextRectangle.NO_BORDER;
                titleCell.HorizontalAlignment = Element.ALIGN_LEFT;
                headerTable.AddCell(titleCell);

                PdfPTable rightBox = new PdfPTable(1);
                rightBox.WidthPercentage = 100;

                PdfPCell dateCell = new PdfPCell(new Phrase(
                  $"DATE : {po.AddedDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)}",
                  boldFont
              ));

                dateCell.BackgroundColor = new BaseColor(200, 230, 250);
                dateCell.Border = iTextRectangle.NO_BORDER;
                rightBox.AddCell(dateCell);

                PdfPCell poCell = new PdfPCell(new Phrase($"PO# : {po.POCode}", boldFont));
                poCell.BackgroundColor = new BaseColor(200, 230, 250);
                poCell.Border = iTextRectangle.NO_BORDER;
                rightBox.AddCell(poCell);

                PdfPCell rightWrapper = new PdfPCell(rightBox);
                rightWrapper.Border = iTextRectangle.NO_BORDER;
                headerTable.AddCell(rightWrapper);

                doc.Add(headerTable);
                doc.Add(new Paragraph(" "));

                // ===== Company Info =====
                PdfPTable companyTable = new PdfPTable(1) { WidthPercentage = 100 };
                PdfPCell companyCell = new PdfPCell { Border = iTextRectangle.NO_BORDER };
                companyCell.AddElement(new Phrase(po.CompanyName, boldFont));
                companyCell.AddElement(new Phrase(po.CompanyAddress, normalFont));
                companyCell.AddElement(new Phrase($"Phone: {po.CompanyMobileNo}", normalFont));
                companyCell.AddElement(new Phrase($"Email: {po.CompanyEmail}", normalFont));
                companyCell.AddElement(new Phrase($"Website: {po.Website}", normalFont));
                companyTable.AddCell(companyCell);
                doc.Add(companyTable);
                doc.Add(new Paragraph(" "));

                // ===== Vendor & Ship To =====
                PdfPTable infoTable = new PdfPTable(2) { WidthPercentage = 100 };
                infoTable.SetWidths(new float[] { 50f, 50f });

                AddHeaderCell(infoTable, "VENDOR INFORMATION", boldFont, new BaseColor(135, 206, 235));
                AddHeaderCell(infoTable, "SHIP TO", boldFont, new BaseColor(135, 206, 235));

                PdfPCell vendorCell = new PdfPCell(new Phrase(
                    $"{po.VendorName}\n{po.Address}\nPhone: {po.MobileNo}\nEmail: {po.Email}", normalFont))
                { Padding = 5 };
                infoTable.AddCell(vendorCell);

                PdfPCell shipCell = new PdfPCell(new Phrase(
                    $"{po.WarehouseName}\n{po.WarehouseAddress}\nPhone: {po.WarehousePhone}\nEmail: {po.WarehouseEmail}", normalFont))
                { Padding = 5 };
                infoTable.AddCell(shipCell);

                doc.Add(infoTable);
                doc.Add(new Paragraph(" "));

                // ===== Items Table =====
                PdfPTable table = new PdfPTable(8) { WidthPercentage = 100 };
                table.SetWidths(new float[] { 10f, 15f, 20f, 10f, 10f, 10f, 10f, 15f });

                AddHeaderCell(table, "ITEM", boldFont, new BaseColor(135, 206, 235));
                AddHeaderCell(table, "ITEMNAME", boldFont, new BaseColor(135, 206, 235));
                AddHeaderCell(table, "DESCRIPTION", boldFont, new BaseColor(135, 206, 235));
                AddHeaderCell(table, "QTY", boldFont, new BaseColor(135, 206, 235));
                AddHeaderCell(table, "UNIT PRICE", boldFont, new BaseColor(135, 206, 235));
                AddHeaderCell(table, "DISCOUNT", boldFont, new BaseColor(135, 206, 235));
                AddHeaderCell(table, "GST", boldFont, new BaseColor(135, 206, 235));
                AddHeaderCell(table, "TOTAL", boldFont, new BaseColor(135, 206, 235));

                // ===== Calculations =====
                double subTotal = 0, totalGST = 0;

                foreach (var item in poItems)
                {
                    // Handle nulls and conversions safely
                    double quantity = (po.RequestTypeId == 9) ? item.JITQuantity : item.Quantity;
                    double costPerUnit = Convert.ToDouble(item.CostPerUnit);
                    double discountPercent = double.TryParse(item.Discount?.Replace("%", ""), out double d) ? d : 0;
                    double gstPercent = double.TryParse(item.GST?.Replace("%", ""), out double g) ? g : 0;

                    // Calculate amounts
                    double amountBeforeDiscount = quantity * costPerUnit;
                    double discountAmount = amountBeforeDiscount * (discountPercent / 100);
                    double taxableAmount = amountBeforeDiscount - discountAmount;
                    double gstAmount = taxableAmount * (gstPercent / 100);
                    double totalAmount = taxableAmount + gstAmount;

                    subTotal +=Convert.ToDouble(item.Amount);
                    //totalGST += subTotal+;

                    // Add data row
                    AddDataCell(table, item.ItemCode, normalFont);
                    AddDataCell(table, item.ItemName, normalFont);
                    AddDataCell(table, item.Description, normalFont);
                    AddDataCell(table, quantity.ToString("N2"), normalFont, Element.ALIGN_CENTER);
                    AddDataCell(table, $"\u20B9{costPerUnit:N2}", normalFont, Element.ALIGN_RIGHT);
                    AddDataCell(table, $"{discountPercent:N2}%", normalFont, Element.ALIGN_RIGHT);
                    AddDataCell(table, $"{gstPercent:N2}%", normalFont, Element.ALIGN_RIGHT);
                    AddDataCell(table, $"\u20B9{totalAmount:N2}", normalFont, Element.ALIGN_RIGHT);
                }

                doc.Add(table);
                doc.Add(new Paragraph(" "));

                // ===== Totals Section =====
                double shipping = Convert.ToDouble(po.ShippingCharges);
                double grandTotal = subTotal+ shipping;

                PdfPTable totalsTable = new PdfPTable(2)
                {
                    WidthPercentage = 40,
                    HorizontalAlignment = Element.ALIGN_RIGHT
                };
                totalsTable.AddCell(new PdfPCell(new Phrase("SUBTOTAL", boldFont)) { Border = 0 });
                totalsTable.AddCell(new PdfPCell(new Phrase($"\u20B9{subTotal:N2}", normalFont)) { Border = 0, HorizontalAlignment = Element.ALIGN_RIGHT });

                //totalsTable.AddCell(new PdfPCell(new Phrase("GST", boldFont)) { Border = 0 });
                //totalsTable.AddCell(new PdfPCell(new Phrase($"\u20B9{totalGST:N2}", normalFont)) { Border = 0, HorizontalAlignment = Element.ALIGN_RIGHT });

                totalsTable.AddCell(new PdfPCell(new Phrase("SHIPPING", boldFont)) { Border = 0 });
                totalsTable.AddCell(new PdfPCell(new Phrase($"\u20B9{shipping:N2}", normalFont)) { Border = 0, HorizontalAlignment = Element.ALIGN_RIGHT });

                totalsTable.AddCell(new PdfPCell(new Phrase("GRAND TOTAL", boldFont))
                {
                    Border = 0,
                    BackgroundColor = new BaseColor(135, 206, 235),
                    Padding = 5
                });
                totalsTable.AddCell(new PdfPCell(new Phrase($"\u20B9{grandTotal:N2}", boldFont))
                {
                    Border = 0,
                    BackgroundColor = new BaseColor(135, 206, 235),
                    HorizontalAlignment = Element.ALIGN_RIGHT,
                    Padding = 5
                });

                //bottomTable.AddCell(new PdfPCell(totalsTable) { Border = 0 });
                //doc.Add(bottomTable);
                doc.Add(totalsTable);



                doc.Close();
                return ms.ToArray();
            }
        }
        // ===== Helper Methods =====
        private void AddHeaderCell(PdfPTable table, string text, iTextSharp.text.Font font, BaseColor bgColor)
        {
            PdfPCell cell = new PdfPCell(new Phrase(text, font))
            {
                BackgroundColor = bgColor,
                HorizontalAlignment = Element.ALIGN_CENTER,
                Padding = 5
            };
            table.AddCell(cell);
        }

        private void AddDataCell(PdfPTable table, string text, iTextSharp.text.Font font, int alignment = Element.ALIGN_LEFT)
        {
            PdfPCell cell = new PdfPCell(new Phrase(text, font))
            {
                HorizontalAlignment = alignment,
                Padding = 5
            };
            table.AddCell(cell);
        }

        //Just in time Item list to create the PO
        [HttpGet]
        public ActionResult JustInTimeItemListOK()
        {
            return View();

        }
        public async Task<JsonResult> FetchJustInTimeItemListOK()
        {
            DataSet ds = new DataSet();
            ds = await bal.FetchAllJITItemsOK();

            //List<string> itm = new List<string>();
            //itm.Add("ITM001");
            //ds =await bal.FetchSelectedJITItemDetailstOK(itm);
            List<Purchase> lstItem = new List<Purchase>();
            try
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    Purchase p = new Purchase();
                    p.SRNO = Convert.ToInt32(ds.Tables[0].Rows[i]["SRNO"].ToString());
                    p.ItemName = ds.Tables[0].Rows[i]["ItemName"].ToString();
                    p.ItemCode = ds.Tables[0].Rows[i]["ItemCode"].ToString();
                    p.Quantity = Convert.ToInt32(ds.Tables[0].Rows[i]["Quantity"].ToString());
                    p.AddedBy = ds.Tables[0].Rows[i]["FullName"].ToString();
                    p.AddedDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["AddedDate"].ToString());
                    p.RequiredDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["RequiredDate"].ToString());
                    p.StockReqirementId = Convert.ToInt32(ds.Tables[0].Rows[i]["StockReqirementId"].ToString());
                    lstItem.Add(p);
                }

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error in TermsConditions loop: " + ex.Message);
            }
            return Json(new { data = lstItem }, JsonRequestBehavior.AllowGet);

        }
        //public async Task<JsonResult> FetchJITItemPODetailsOk(List<string> items)
        public async Task<JsonResult> FetchJITItemPODetailsOk(string itemCode, int stockRequirementId)
        {
            //Purchase p = new Purchase();
            //p.StockReqirementId = stockRequirementId;
            TempData["StockReqirementId"] = stockRequirementId;
            TempData.Keep("StockReqirementId"); //
            // DataSet ds = await bal.FetchSelectedJITItemDetailstOK(items);
            DataSet ds = await bal.FetchSelectedJITItemDetailstOK(itemCode, stockRequirementId);
            // 1️⃣ Quotation Header
            List<Purchase> lstHeader = new List<Purchase>();
            try
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    Purchase header = new Purchase();

                    header.QuotationID = ds.Tables[0].Rows[i]["QuotationNo"].ToString();
                    header.QuotationDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["QuotationDate"]);
                    header.QuotationDateString = header.QuotationDate.ToString("yyyy-MM-dd");
                    header.ApprovedBy = ds.Tables[0].Rows[i]["ApprovedBy"].ToString();
                    header.ApprovedDate = Convert.ToDateTime(ds.Tables[0].Rows[i]["ApprovedDate"]);
                    header.OriginalApprovedDate = header.ApprovedDate.ToString("yyyy-MM-dd");
                    header.VendorName = ds.Tables[0].Rows[i]["VenderName"].ToString();
                    header.CompanyName = ds.Tables[0].Rows[i]["CompanyName"].ToString();
                    header.CompanyAddress = ds.Tables[0].Rows[i]["CompanyAddress"].ToString();
                    header.MobileNo = Convert.ToInt64(ds.Tables[0].Rows[i]["VendorContactNo"].ToString());
                    header.Email = ds.Tables[0].Rows[i]["VendorEmail"].ToString();
                    header.AccountNumber = Convert.ToInt64(ds.Tables[0].Rows[i]["VendorAccountNo"].ToString());
                    header.IFSCCode = ds.Tables[0].Rows[i]["IFSCCode"].ToString();
                    header.SwiftCode = ds.Tables[0].Rows[i]["SwiftCode"].ToString();
                    header.DeliveryAddress = ds.Tables[0].Rows[i]["DeliveryAddress"].ToString();
                    header.ShippingCharges = Convert.ToDecimal(ds.Tables[0].Rows[i]["TransportationCharges"].ToString());
                    header.BillingAddress = ds.Tables[0].Rows[i]["BillingAddress"].ToString();

                    //header.TotalAmount = Convert.ToDecimal(ds.Tables[0].Rows[i]["TotalAmount"]);
                    //  header.SubAmount = Convert.ToDecimal(ds.Tables[0].Rows[i]["Amount"]);

                    lstHeader.Add(header);
                }
            }
            catch (Exception exx)
            {
                System.Diagnostics.Debug.WriteLine("Error in PO Details loop: " + exx.Message);
            }

            // 4️⃣ RFQ / Items
            List<Purchase> lstRFQItems = new List<Purchase>();
            try
            {
                for (int i = 0; i < ds.Tables[1].Rows.Count; i++)
                {
                    var item = new Purchase
                    {
                        SRNO = Convert.ToInt32(ds.Tables[1].Rows[i]["SRNO"]),
                        RegisterQuotationItemCode = ds.Tables[1].Rows[i]["RegisterQuotationItemCode"].ToString(),
                        ItemCode = ds.Tables[1].Rows[i]["ItemCode"].ToString(),
                        ItemName = ds.Tables[1].Rows[i]["ItemName"].ToString(),
                        Description = ds.Tables[1].Rows[i]["Description"].ToString(),
                        Quantity = Convert.ToInt32(ds.Tables[1].Rows[i]["Quantity"]),
                        UOM = ds.Tables[1].Rows[i]["UOMName"].ToString(),
                        CostPerUnit = Convert.ToDecimal(ds.Tables[1].Rows[i]["CostPerUnit"]),
                        Discount = ds.Tables[1].Rows[i]["Discount"].ToString(),
                        GST = ds.Tables[1].Rows[i]["GST"].ToString(),
                        Amount = Convert.ToDecimal(ds.Tables[1].Rows[i]["ItemNetAmount"])
                    };
                    lstRFQItems.Add(item);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error in TermsConditions loop: " + ex.Message);
                //throw ex;
            }

            // 2️⃣ Accountant Staff
            List<Purchase> lstStaff = new List<Purchase>();
            try
            {
                for (int i = 0; i < ds.Tables[2].Rows.Count; i++)
                {
                    var staff = new Purchase
                    {
                        StaffCode = ds.Tables[2].Rows[i]["StaffCode"].ToString(),
                        AccountEmail = ds.Tables[2].Rows[i]["EmailAddress"].ToString()
                    };
                    lstStaff.Add(staff);
                }
            }
            catch (Exception exstaff)
            {
                System.Diagnostics.Debug.WriteLine("Error in Accountent Fetch loop: " + exstaff.Message);
            }

            // 3️⃣ Terms & Conditions
            List<Purchase> lstTerms = new List<Purchase>();
            try
            {
                for (int i = 0; i < ds.Tables[3].Rows.Count; i++)
                {
                    var term = new Purchase
                    {
                        TermConditionId = Convert.ToInt32(ds.Tables[3].Rows[i]["TermConditionId"]),
                        TermConditionName = ds.Tables[3].Rows[i]["TermConditionName"].ToString()
                    };
                    lstTerms.Add(term);
                }
            }
            catch (Exception exterm)
            {
                System.Diagnostics.Debug.WriteLine("Error in TermsConditions loop: " + exterm.Message);
            }

            
            // Combine into one JSON object
            var jsonData = new
            {
                QuotationHeader = lstHeader,
                AccountantStaff = lstStaff,
                TermsConditions = lstTerms,
                Items = lstRFQItems
            };

            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }

       // PO Email with PDF Attachment Logic
        public async Task<byte[]> GeneratePOPDFAttachment(string POCode)
        {
            DataSet ds = await bal.FetchPODetailsByPOCodeForOPDFOK(POCode);

            Purchase po = new Purchase();
            List<Purchase> poItems = new List<Purchase>();

            // Company & Vendor Details
            if (ds.Tables[0].Rows.Count > 0)
            {
                var row = ds.Tables[0].Rows[0];
                po.CompanyName = row["CompanyName"].ToString();
                po.CompanyAddress = row["CompanyAddress"].ToString();
                po.CompanyMobileNo = Convert.ToInt64(row["CompanyPhone"]);
                po.CompanyEmail = row["CompanyEmail"].ToString();
                po.Website = row["Website"].ToString();
                po.POCode = row["POCode"].ToString();
                po.AddedDate = Convert.ToDateTime(row["AddedDate"]);
                po.ShippingCharges = Convert.ToDecimal(row["ShippingCharges"]);
                po.VendorName = row["VenderName"].ToString();
                po.Address = row["VendorAddress"].ToString();
                po.MobileNo = Convert.ToInt64(row["VendorPhone"]);
                po.Email = row["VendorEmail"].ToString();  // vendor email
                po.WarehouseName = row["WarehouseName"].ToString();
                po.WarehouseAddress = row["WarehouseAddress"].ToString();
                po.WarehousePhone = Convert.ToInt64(row["WarehousePhone"]);
                po.WarehouseEmail = row["WarehouseEmail"].ToString();
                po.SubAmount = Convert.ToDecimal(row["SubAmount"]);
                po.GrandTotal = Convert.ToDecimal(row["GrandTotal"]);
            }

            foreach (DataRow dr in ds.Tables[1].Rows)
            {
                poItems.Add(new Purchase
                {
                    ItemCode = dr["ItemCode"].ToString(),
                    ItemName = dr["ItemName"].ToString(),
                    Quantity = Convert.ToInt32(dr["Quantity"]),
                    Description = dr["Description"].ToString(),
                    CostPerUnit = Convert.ToDecimal(dr["CostPerUnit"]),
                    Discount = dr["Discount"].ToString(),
                    GST = dr["GST"].ToString(),
                    Amount = Convert.ToDecimal(dr["Amount"])
                });
            }

            return GeneratePurchaseOrderPDF(po, poItems);
        }
        public async Task SendPOEmailAsync(string POCode, byte[] pdfBytes)
        {
            try
            {
                // Fetch PO details again for vendor email
                DataSet ds = await bal.FetchPODetailsByPOCodeForOPDFOK(POCode);
                if (ds.Tables[0].Rows.Count == 0) return;

                string vendorEmail = ds.Tables[0].Rows[0]["VendorEmail"].ToString();
                string vendorName = ds.Tables[0].Rows[0]["VenderName"].ToString();

                using (MailMessage mail = new MailMessage())
                {
                    //mail.From = new MailAddress("sandeshjatti5329@gmail.com", "Rahitech IT Solution");
                   // mail.From = new MailAddress("gstprocurmenterp@gmail.com", "Procurement System");
                    mail.From = new MailAddress(WebConfigurationManager.AppSettings["MainEmail"], "Procurement System");
                    mail.To.Add(vendorEmail);
                   // mail.To.Add("kumbharomkar765@gmail.com"w); // Test email
                    mail.Subject = $"Purchase Order - {POCode}";
                    mail.Body = $"Dear {vendorName},\n\nPlease find attached the Purchase Order #{POCode}.\n\nThank you.\n\nRegards,\nYour Company";
                    mail.IsBodyHtml = false;

                    // Attach PDF
                    mail.Attachments.Add(new Attachment(new MemoryStream(pdfBytes), $"PurchaseOrder_{POCode}.pdf", "application/pdf"));

                    using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                    {
                       // smtp.Credentials = new NetworkCredential("gstprocurmenterp@gmail.com", "lhrlntigzidizmju");
                        smtp.Credentials = new NetworkCredential(WebConfigurationManager.AppSettings["MainEmail"], WebConfigurationManager.AppSettings["AppPassword"]);
                        smtp.EnableSsl = true;
                        await smtp.SendMailAsync(mail);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log exception or handle it silently
                Console.WriteLine("Email Error: " + ex.Message);
            }
        }



        [HttpGet]
        public async Task<JsonResult> GetRejectedPODetailsOK(string POCode)
        {
            DataSet ds = await bal.FetchPODetailsByPOCodeForOPDFOK(POCode);

            if (ds == null || ds.Tables.Count < 2 || ds.Tables[0].Rows.Count == 0)
                return Json(new { success = false, message = "No data found." }, JsonRequestBehavior.AllowGet);

            // ===== PO Main Details =====
            var row = ds.Tables[0].Rows[0];
            var po = new
            {
                CompanyName = row["CompanyName"].ToString(),
                CompanyAddress = row["CompanyAddress"].ToString(),
                CompanyPhone = row["CompanyPhone"].ToString(),
                CompanyEmail = row["CompanyEmail"].ToString(),
                Website = row["Website"].ToString(),
                POCode = row["POCode"].ToString(),
                AddedDate = Convert.ToDateTime(row["AddedDate"]).ToString("yyyy-MM-dd"),
                ShippingCharges = Convert.ToDecimal(row["ShippingCharges"]),
                ApprovedBy = row["ApprovedRejectedByName"].ToString(),
                ApprovedRejectedDate = Convert.ToDateTime(row["ApprovedRejectedDate"]).ToString("yyyy-MM-dd"),
                Note = row["Note"].ToString(),
                VendorName = row["VenderName"].ToString(),
                VendorAddress = row["VendorAddress"].ToString(),
                VendorPhone = row["VendorPhone"].ToString(),
                VendorEmail = row["VendorEmail"].ToString(),
                WarehouseName = row["WarehouseName"].ToString(),
                WarehouseAddress = row["WarehouseAddress"].ToString(),
                WarehousePhone = row["WarehousePhone"].ToString(),
                WarehouseEmail = row["WarehouseEmail"].ToString(),
                SubAmount = Convert.ToDecimal(row["SubAmount"]),
                GrandTotal = Convert.ToDecimal(row["GrandTotal"])
            };

            // ===== Item Details =====
            var poItems = ds.Tables[1].AsEnumerable().Select(dr => new
            {
                ItemCode = dr["ItemCode"].ToString(),
                ItemName = dr["ItemName"].ToString(),
                Quantity = Convert.ToInt32(dr["Quantity"]),
                Description = dr["Description"].ToString(),
                UOM = dr["UOMName"].ToString(),
                CostPerUnit = Convert.ToDecimal(dr["CostPerUnit"]),
                Discount = dr["Discount"].ToString(),
                GST = dr["GST"].ToString(),
                Amount = Convert.ToDecimal(dr["Amount"])
            }).ToList();

            return Json(new { success = true, po, poItems }, JsonRequestBehavior.AllowGet);
        }


        #endregion

        #region prathamesh

        public ActionResult UserDashboardPRK()
        {
            var RoleId = Convert.ToInt32(Session["RoleId"]);

            switch (RoleId)
            {
                case 5: return RedirectToAction("CreatePRADDItemPSM");
                case 6: return RedirectToAction("QuotationSJ");
                case 7: return RedirectToAction("ShowAllRFQsVNK");
                case 8: return view();
                case 9: return RedirectToAction("SelectedQuotationListShowOK");
            }
            return View();
        }

        // Dashboard counts view
        [HttpGet]
        public async Task<JsonResult> GetDashboardCounts(DateTime startDate, DateTime endDate)
        {
            try
            {
                var dashboardCount = await bal.GetPurchasePRK(startDate, endDate);
                return Json(dashboardCount, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new
                {
                    ApprovedPR = 0,
                    PendingPR = 0,
                    RequestedRFQ = 0,
                    PendingRFQ = 0,
                    ApprovedRC = 0,
                    PendingRC = 0,
                    ApprovedPO = 0,
                    PendingPO = 0
                }, JsonRequestBehavior.AllowGet);
            }
        }


        //dashboard Trends View
        public async Task<JsonResult> GetDashboardTrends(DateTime startDate, DateTime endDate)
        {
            try
            {
                // Optional: Include end date fully
                endDate = endDate.AddDays(1);

                var trendData = await bal.GetPurchaseTrends(startDate, endDate);
                return Json(new { Trend = trendData }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new
                {
                    Trend = new { Categories = new List<string>(), PR = new List<int>(), RFQ = new List<int>(), RQ = new List<int>(), PO = new List<int>() }
                }, JsonRequestBehavior.AllowGet);
            }
        }




        // Get all PR list (JSON)
        public async Task<ActionResult> ShowAllPRK(DateTime startDate, DateTime endDate)
        {

            var data = await bal.ShowAllPRPRK(startDate, endDate);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        // Partial view for all PR
        public ActionResult ShowAllPRPartialPRK(DateTime? startDate, DateTime? endDate)
        {
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
            return PartialView("_ShowAllPRPartialPRK");
        }

        // Get all PO list (JSON)
        public async Task<ActionResult> ShowAllPOPRK(DateTime startDate, DateTime endDate)
        {

            var data = await bal.ShowAllPOPRK(startDate, endDate);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        // Partial view for all PO
        public ActionResult ShowAllPOPartialPRK(DateTime? startDate, DateTime? endDate)
        {
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
            return PartialView("_ShowAllPOPartialPRK");
        }




        // Pending PR main view
        public ActionResult ShowPendingPRPRK()
        {
            return View();
        }

        // Get pending PR list (JSON)
        public async Task<ActionResult> ShowPndPRPRK()
        {
            var data = await bal.ShowPendingPRPRK();
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        // Approved PR main view
        public ActionResult ShowApprovePRPRK()
        {
            return View();
        }

        // Get approved PR list (JSON)
        public async Task<ActionResult> ShowApprovPRK()
        {
            var data = await bal.ShowApprovedPRPRK();
            return Json(data, JsonRequestBehavior.AllowGet);
        }

      

       

      

        // Partial view for rejected PR
        public ActionResult ShowRejectedPRPartialPRK(DateTime? startDate, DateTime? endDate)
        {
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
            return PartialView("_ShowRejectedPRPartialPRK");
        }


        // Get rejected PO list (JSON)
        public async Task<ActionResult> ShowRejectPOPRK(DateTime startDate, DateTime endDate)
        {
            var data = await bal.ShowRejectedPOPRK(startDate, endDate);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        // Partial view for rejected PR
        public ActionResult ShowRejectedOPartialPRK(DateTime? startDate, DateTime? endDate)
        {
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
            return PartialView("_ShowRejectedOPartialPRK");
        }

        ////////////////////////////////////////////



        //// Rejected PR main view
        //public ActionResult ShowRejectedPRPRK()
        //{
        //    if (Session["StaffCode"] == null)
        //        return RedirectToAction("MainLogin", "Account");

        //    return View();
        //}

        //// Get rejected PR list (JSON)
        //public async Task<ActionResult> ShowRejectPRK()
        //{
        //    var data = await bal.ShowRejectedPRmainPRK();
        //    return Json(data, JsonRequestBehavior.AllowGet);
        //}



        //// Rejected PR items partial (by PRCode)
        //public ActionResult ShowRejectedPRItemPRK(string prCode)
        //{
        //    ViewBag.PRCode = prCode;
        //    return PartialView("_ShowRejectedPRItemPRK");
        //}



        // Pending PR main view
        public ActionResult ShowRejectedPRPRK()
        {
            return View();
        }

        // Get pending PR list (JSON)
        public async Task<ActionResult> ShowRejectPRK()
        {
            var data = await bal.ShowRejectedPRmainPRK();
            return Json(data, JsonRequestBehavior.AllowGet);
        }



        // Get rejected PR items (JSON by PRCode)
        public async Task<ActionResult> ShowRejectedPRItemsPRK(string prCode)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("PRCode received: " + prCode);
                var data = await bal.ShowRejectedPRItemPRK(prCode);
                System.Diagnostics.Debug.WriteLine("Items found: " + data.Count);
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("❌ ERROR: " + ex.ToString());
                return new ContentResult
                {
                    Content = "<pre style='color:red;font-family:monospace'>" + ex.ToString() + "</pre>",
                    ContentType = "text/html"
                };
            }
        }

        // Partial view for rejected PR items
        public ActionResult ShowRejectedPRItemPartialPRK(string prCode)
        {

            ViewBag.PRCode = prCode;
            return PartialView("_ShowRejectedPRItemPartialPRK");
        }


        // Pending PR items partial (by PRCode)
        public ActionResult ShowPendingPRItemPRK(string prCode)
        {
            ViewBag.PRCode = prCode;
            return PartialView("_ShowPendingPRItemPRK");
        }

        // Get pending PR items (JSON by PRCode)
        public async Task<ActionResult> ShowPendingPRItemsPRK(string prCode)
        {
            var data = await bal.ShowPendingPRItemPRK(prCode);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

      

        // Approved PR items partial (by PRCode)
        public ActionResult ShowApporvePRItemPRK(string prCode)
        {
            ViewBag.PRCode = prCode;
            return PartialView("_ShowApporvePRItemPRK");
        }

        // Get approved PR items (JSON by PRCode)
        public async Task<ActionResult> ShowApprovePRItemPRK(string prCode)
        {
            var data = await bal.ShowApprovePRItemPRK(prCode);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        // Update PR status as approved
        [HttpPost]
        public async Task<ActionResult> UpdatePRStatusPRK(string prCode, int statusId, string note, Purchase model)
        {
            try
            {
                model.StaffCode = Session["StaffCode"].ToString();
                await bal.UpdatePRStatusPRK(prCode, statusId, note, model);
                return Json(new { success = true, message = "Status updated successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        // Update PR status as rejected
        [HttpPost]
        public async Task<ActionResult> UpdatePRStatusRejectPRK(string prCode, int statusId, string note, Purchase model)
        {
            try
            {
                model.StaffCode = Session["StaffCode"].ToString();
                await bal.UpdatePRStatusReject(prCode, statusId, note, model);
                TempData["SuccessMessage"] = "PR status updated successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error: " + ex.Message;
            }
            return RedirectToAction("ShowPendingPRPRK");
        }




        //Manager Dashboard 

        // Partial view for approved PR

        public async Task<ActionResult> ShowApprovPRDashPRK(DateTime startDate, DateTime endDate)
        {
            var data = await bal.ShowApprovedPRDashPRK(startDate, endDate);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ShowApprovePRPartialPRK(DateTime? startDate, DateTime? endDate)
        {
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
            return PartialView("_ShowApprovePRPartialPRK");
        }




        // Partial view for pending PR

        public async Task<ActionResult> ShowPendingPRDashPRK(DateTime startDate, DateTime endDate)
        {
            var data = await bal.ShowPendingPRDashPRK(startDate, endDate);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ShowPendingPRPartialPRK(DateTime? startDate, DateTime? endDate)
        {
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
            return PartialView("_ShowPendingPRPartialPRK");
        }


        // Requested RFQ
        public async Task<ActionResult> ShowRequestedRFQPRK(DateTime startDate, DateTime endDate)
        {
            var data = await bal.ShowRequestedRFQPRK(startDate, endDate);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        // Partial view for Requested RFQ
        public ActionResult ShowReuestedRFQPartialPRK(DateTime? startDate, DateTime? endDate)
        {
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd"); 
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
            return PartialView("_ShowReuestedRFQPartialPRK");
        }

        //Pending RFQ
        public async Task<ActionResult> ShowPendingRFQPRK(DateTime startDate, DateTime endDate)
        {
            var data = await bal.ShowPendingRFQPRK(startDate, endDate);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        // Partial view for Pending RFQ
        public ActionResult ShowPendingRFQPartialPRK(DateTime? startDate, DateTime? endDate)
        {
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
            return PartialView("_ShowPendingRFQPartialPRK");
        }

        // Approve RQ
        public async Task<ActionResult> ShowApproveRQPRK(DateTime startDate, DateTime endDate)
        {
            var data = await bal.ShowApproveRQPRK(startDate, endDate);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        //   // Partial view for Approve RQ
        public ActionResult ShowApproveRQPartialPRK(DateTime? startDate, DateTime? endDate)
        {
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
            return PartialView("_ShowApproveRQPartialPRK");
        }

        //Pending RQ
        public async Task<ActionResult> ShowPendingRQPRK(DateTime startDate, DateTime endDate)
        {

            var data = await bal.ShowPendingRQPRK(startDate, endDate);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        // Partial view for Pending RQ 
        public ActionResult ShowPendingRQPartialPRK(DateTime? startDate, DateTime? endDate)
        {
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
            return PartialView("_ShowPendingRQPartialPRK");
        }



        //Approve PO
        public async Task<ActionResult> ShowApprovePOPRK(DateTime startDate, DateTime endDate)
        {
            var data = await bal.ShowApprovePOPRK(startDate, endDate);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        // Partial view for Approve PO
        public ActionResult ShowApprovePOPartialPRK(DateTime? startDate, DateTime? endDate)
        {
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
            return PartialView("_ShowApprovePOPartialPRK");
        }

        //Pending PO
        public async Task<ActionResult> ShowPendingPOPRK(DateTime startDate, DateTime endDate)
        {
            var data = await bal.ShowPendingPOPRK(startDate, endDate);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        // Partial view for Approve PO
        public ActionResult ShowPendingPOPartialPRK(DateTime? startDate, DateTime? endDate)
        {
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
            return PartialView("_ShowPendingPOPartialPRK");
        }
    
        #endregion

        #region Sandesh
        public ActionResult QuotationSJ()
        {
            return View();
        }

        // For ALL Approved PR's list
        public async Task<JsonResult> GETQUATIONLISTSJ()
        {
            DataSet ds = await bal.AllQuationListSJ();
            List<Purchase> grnlist = new List<Purchase>();

            if (ds != null && ds.Tables.Count > 0)
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    grnlist.Add(new Purchase
                    {
                        SRNO =Convert.ToInt32(dr["SrNo"].ToString()),
                        PRCode = dr["PRCode"].ToString(),
                        RequiredDate = Convert.ToDateTime(dr["RequiredDate"].ToString()),
                        AddedDate = Convert.ToDateTime(dr["AddedDate"].ToString()),
                        AddedBy = dr["FullName"].ToString(),
                        StatusName = dr["StatusName"].ToString(),
                    });
                }
            }
            return Json(new { data = grnlist }, JsonRequestBehavior.AllowGet);
        }

        // For PR's Details
        public async Task<JsonResult> GETPRSJ(string id)
        {
            var RFQcode = bal.GenerateNextRFQCodeSJ();
            DataSet ds = await bal.PRSJ(id);
            List<Purchase> grnlist = new List<Purchase>();

            if (ds != null && ds.Tables.Count > 0)
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    grnlist.Add(new Purchase
                    {

                        RFQCode = RFQcode.ToString(),
                        SRNO =Convert.ToInt32(dr["SrNo"].ToString()),
                        ItemName = dr["ItemName"].ToString(),
                        Description = dr["Description"].ToString(),
                        RequiredQuantity =Convert.ToDecimal(dr["RequiredQuantity"]),
                        UOMNamee = dr["UOMName"].ToString(),
                        RequiredDate = Convert.ToDateTime(dr["RequiredDate"].ToString())

                    });
                }
            }
            return Json(new { data = grnlist }, JsonRequestBehavior.AllowGet);
        }

        // Generate new RFQ code 
        public async Task<JsonResult> GenRFQSJ()
        {
            string nextcode = await bal.GenerateNextRFQCodeSJ();
            return Json(new { data = nextcode }, JsonRequestBehavior.AllowGet);
        }



        // For Contact person dropdown
        [HttpGet]
        public async Task<JsonResult> GetContactPersonsSJ()
        {
            var ds = await bal.GetContactPersonsSJ();
            var list = new List<object>();

            if (ds != null && ds.Tables.Count > 0)
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    list.Add(new
                    {
                        Id = dr["StaffCode"]?.ToString(),
                        Name = dr["FullName"]?.ToString(),
                        Phone = dr["ContactNo"]?.ToString(),
                        Email = dr["EmailAddress"]?.ToString()
                    });
                }
            }

            return Json(new { data = list }, JsonRequestBehavior.AllowGet);
        }


        //For warehouse dropdown
        [HttpGet]
        public async Task<JsonResult> GetwareSJ()
        {
            var ds = await bal.GetWareHouseSJ();
            var list = new List<object>();

            if (ds != null && ds.Tables.Count > 0)
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    list.Add(new
                    {
                        Id = dr["WareHouseCode"]?.ToString(),
                        Name = dr["WarehouseName"]?.ToString(),
                        Address = dr["Address"]?.ToString(),
                    });
                }
            }

            return Json(new { data = list }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<JsonResult> SaveRFQSJ()
        {
            try
            {
                Request.InputStream.Seek(0, SeekOrigin.Begin);
                string jsonData = new StreamReader(Request.InputStream).ReadToEnd();

                // Allow dd/MM/yyyy parsing
                var settings = new JsonSerializerSettings
                {
                    Culture = new System.Globalization.CultureInfo("en-GB"),
                    DateFormatHandling = DateFormatHandling.IsoDateFormat
                };

                var model = JsonConvert.DeserializeObject<Purchase>(jsonData, settings);

                if (!TryValidateModel(model))
                    return Json(new { success = false, error = "Invalid model state" });

                string staffCode = Session["StaffCode"]?.ToString() ?? "";
                string date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                int result = await bal.SaveRFQSJ(model, staffCode, date);
                return Json(new { success = result > 0 });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }





        // For all RFQ's
        [HttpGet]
        public async Task<JsonResult> GETALLRFQSJ()
        {
            DataSet ds = await bal.ALLRFQLISTSJ();
            var rfqList = new List<object>();

            if (ds != null && ds.Tables.Count > 0)
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    rfqList.Add(new
                    {
                        SrNo = dr["SrNo"].ToString(),
                        PRCode = dr["PRCode"].ToString(),
                        RFQCode = dr["RFQCode"]?.ToString(),
                        RequiredDate = dr["ExpectedDate"].ToString(),
                        Description = dr["Description"]?.ToString(),
                        Status = dr["StatusName"]?.ToString()
                    });
                }
            }

            return Json(new { data = rfqList }, JsonRequestBehavior.AllowGet);
        }


        // For All RFQ's Details
        [HttpGet]
        public async Task<JsonResult> GetRFQDetailsSJ(string id)
        {
            DataSet ds = await bal.GetRFQDetailsSJ(id);

            object header = null;
            var items = new List<object>();

            if (ds != null && ds.Tables.Count > 0)
            {
                if (ds.Tables[0].Rows.Count > 0)
                {
                    DataRow firstRow = ds.Tables[0].Rows[0];

                    header = new
                    {
                        PRCode = firstRow["PRCode"]?.ToString(),
                        RFQCode = firstRow["RFQCode"]?.ToString(),
                        CreatedDate = firstRow["AddedDate"]?.ToString(),
                        CreatedBy = firstRow["FullName"]?.ToString(),
                        ContactPerson = firstRow["FullName1"]?.ToString(),
                        ContactNo = firstRow["ContactNo"]?.ToString(),
                        Email = firstRow["EmailAddress"]?.ToString(),
                        RequiredDate = firstRow["RequiredDate"]?.ToString(),
                        DeliveryAddress = firstRow["Address"]?.ToString(),
                        Note = firstRow["Description1"]?.ToString()
                    };
                }

                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    items.Add(new
                    {
                        SrNo = dr["SrNo"].ToString(),
                        ItemName = dr["ItemName"]?.ToString(),
                        Description = dr["Description"]?.ToString(),
                        Quantity = dr["RequiredQuantity"]?.ToString(),
                        UOM = dr["UOMName"]?.ToString()
                    });
                }
            }

            return Json(new { header, items }, JsonRequestBehavior.AllowGet);
        }

        // For ALL Vendors
        [HttpGet]
        public async Task<JsonResult> GetVendorsSJ(int? industryTypeId = null)
        {
            DataSet ds = await bal.GetVendorsSJ(industryTypeId);

            var vendors = new List<object>();

            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    vendors.Add(new
                    {
                        SrNo = dr["SrNo"].ToString(),
                        Id = dr["VendorCode"]?.ToString(),
                        Name = dr["VenderName"]?.ToString(),
                        CompanyName = dr["CompanyName"]?.ToString(),
                        Phone = dr["MobileNo"]?.ToString(),
                        Email = dr["CompanyEmail"]?.ToString(),
                        Address = dr["CompanyAddress"]?.ToString(),
                        IndustryTypeId = dr["IndustryType"]?.ToString()
                    });
                }
            }

            return Json(new { data = vendors }, JsonRequestBehavior.AllowGet);
        }


        // For Industry Type dropdown
        [HttpGet]
        public async Task<JsonResult> GetIndustryTypesSJ()
        {
            DataSet ds = await bal.GetIndustryTypesSJ();

            var industries = new List<object>();

            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    industries.Add(new
                    {
                        Id = dr["SubTypeId"]?.ToString(),
                        Name = dr["SubTypeName"]?.ToString()
                    });
                }
            }

            return Json(new { data = industries }, JsonRequestBehavior.AllowGet);
        }

        // For Send RFQ's to vendor
        [HttpPost]
        public async Task<JsonResult> SendRFQToVendorsSJ()
        {
            try
            {
                // 🔹 Step 1: Read JSON body
                string json;
                using (var reader = new StreamReader(Request.InputStream))
                {
                    reader.BaseStream.Seek(0, SeekOrigin.Begin);
                    json = reader.ReadToEnd();
                }

                if (string.IsNullOrEmpty(json))
                    return Json(new { success = false, message = "No data received." });

                var request = JsonConvert.DeserializeObject<Purchase>(json);

                if (request.Vendors == null || request.Vendors.Count == 0)
                    return Json(new { success = false, message = "No vendors selected." });

                // 🔹 Step 2: Get RFQ details
                DataSet ds = await bal.GetRFQDetailsSJ(request.RFQCode);
                if (ds == null || ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
                    return Json(new { success = false, message = "RFQ not found." });

                DataRow firstRow = ds.Tables[0].Rows[0];
                var header = new Purchase
                {
                    PRCode = firstRow["PRCode"]?.ToString(),
                    RFQCode = firstRow["RFQCode"]?.ToString(),
                    ContactPerson = firstRow["FullName1"]?.ToString(),
                    Email = firstRow["EmailAddress"]?.ToString(),
                    MobileNo = Convert.ToInt64(firstRow["ContactNo"]?.ToString()),
                    RequiredDate = Convert.ToDateTime(firstRow["RequiredDate"]?.ToString()),
                    Address = firstRow["Address"]?.ToString(),
                    Description = firstRow["Description1"]?.ToString(),
                    Vendors = request.Vendors
                };

                // 🔹 Step 3: Generate RFQ PDF once
                string pdfPath = GenerateRFQPdfSJ(header, ds.Tables[0].AsEnumerable().Select(dr => new Purchase
                {
                    ItemName = dr["ItemName"]?.ToString(),
                    Description = dr["Description"]?.ToString(),
                    RequiredQuantity = Convert.ToDecimal(dr["RequiredQuantity"]),
                    UOMNamee = dr["UOMName"]?.ToString()
                }).ToList(),request.ExpectedDate);

                if (string.IsNullOrEmpty(pdfPath) || !System.IO.File.Exists(pdfPath))
                    return Json(new { success = false, message = "Failed to generate RFQ PDF." });

                // 🔹 Step 4: Save RFQ-vendor mapping
                string staffCode = Session["StaffCode"]?.ToString() ?? "";
                string addedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                int savedCount = await bal.SaveRFQVendorsSJ(header, staffCode, addedDate,request.ExpectedDate);
                if (savedCount == 0)
                    return Json(new { success = false, message = "Failed to save RFQ-Vendor mapping." });

                // 🔹 Step 5: Fetch vendor details
                string vendorIdsCsv = string.Join(",", request.Vendors);
                DataSet vendorDs = await bal.GetVendorsByIdsSJ(vendorIdsCsv);

                if (vendorDs == null || vendorDs.Tables.Count == 0 || vendorDs.Tables[0].Rows.Count == 0)
                    return Json(new { success = false, message = "No vendor details found." });

                // 🔹 Step 6: Send email with attachment to each vendor
                foreach (DataRow dr in vendorDs.Tables[0].Rows)
                {
                    var vendor = new Purchase
                    {
                        VendorCode = dr["VendorCode"]?.ToString(),
                        VendorName = dr["VenderName"]?.ToString(),
                        CompanyName = dr["CompanyName"]?.ToString(),
                        MobileNo = Convert.ToInt64(dr["MobileNo"]?.ToString() ?? "0"),
                        Email = dr["CompanyEmail"]?.ToString(),
                        CompanyAddress = dr["CompanyAddress"]?.ToString()
                    };

                    if (string.IsNullOrEmpty(vendor.Email))
                        continue;

                    try
                    {
                        string subject = $"RFQ {request.RFQCode} from {request.PRCode}";
                        string body = $@"Dear {vendor.VendorName},

We are pleased to invite you to submit a quotation for the attached Request for Quotation (RFQ) document, under reference number {request.RFQCode}, pertaining to Purchase Requisition {request.PRCode}.

The RFQ includes detailed item descriptions, quantities, and terms. Kindly review the document carefully and provide your best quotation by the stated due date.

Your timely response will be highly appreciated.

Should you require any additional information or clarification, please do not hesitate to contact our procurement team.

Sincerely,
Procurement Team
[Gayasoft Technology]";


                        // 🔹 Make a temp copy for each vendor (avoids file lock issues)
                        string tempFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.pdf");
                        System.IO.File.Copy(pdfPath, tempFile, true);

                        // Send mail
                        SendEmailWithAttachmentSJ(vendor.Email, subject, body, tempFile);

                        // Clean up temp copy
                        System.IO.File.Delete(tempFile);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Failed to send email to {vendor.Email}: {ex.Message}");
                    }
                }

                // 🔹 Step 7: Delete original PDF after all emails sent
                if (System.IO.File.Exists(pdfPath))
                    System.IO.File.Delete(pdfPath);

                return Json(new { success = true, message = "RFQ sent successfully to selected vendors!" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SendRFQToVendorsSJ() Error: {ex.Message}");
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        // For Get perticular vendor select
        private Purchase GetVendorByIdSJ(string vendorId)
        {
            if (string.IsNullOrEmpty(vendorId))
                return null;

            DataSet ds = bal.GetVendorsByIdsSJ(vendorId).Result;

            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                DataRow dr = ds.Tables[0].Rows[0];

                return new Purchase
                {
                    VendorCode = dr["VendorCode"]?.ToString(),
                    VendorName = dr["VenderName"]?.ToString(),
                    CompanyName = dr["CompanyName"]?.ToString(),
                    MobileNo = Convert.ToInt64(dr["MobileNo"]?.ToString() ?? "0"),
                    Email = dr["CompanyEmail"]?.ToString(),
                    CompanyAddress = dr["CompanyAddress"]?.ToString()
                };
            }

            return null;
        }



        //For generate pdf for vendors
        private string GenerateRFQPdfSJ(Purchase header, List<Purchase> items,DateTime expdate)
        {
            string filePath = Path.Combine(
                Path.GetTempPath(),
                $"RFQ_{header.RFQCode}_{DateTime.Now:yyyyMMddHHmmss}.pdf"
            );

            using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                using (var document = new iTextSharp.text.Document(PageSize.A4, 40f, 40f, 40f, 40f))
                {
                    PdfWriter.GetInstance(document, fs);
                    document.Open();


                    var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
                    var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);

                    Paragraph title = new Paragraph("Request for Quotation (RFQ)", titleFont);
                    title.Alignment = Element.ALIGN_CENTER;
                    title.SpacingAfter = 20f;
                    document.Add(title);

                    PdfPTable headerTable = new PdfPTable(2);
                    headerTable.WidthPercentage = 100;
                    headerTable.SetWidths(new float[] { 30f, 70f });

                    headerTable.AddCell(new Phrase("Contact Person:", normalFont));
                    headerTable.AddCell(new Phrase(header.ContactPerson, normalFont));

                    headerTable.AddCell(new Phrase("Email:", normalFont));
                    headerTable.AddCell(new Phrase(header.Email, normalFont));

                    headerTable.AddCell(new Phrase("Contact No:", normalFont));
                    PdfPCell cell = new PdfPCell(new Phrase(header.MobileNo.ToString(), normalFont));
                    headerTable.AddCell(cell);



                    headerTable.AddCell(new Phrase("Required Date:", normalFont));
                    string requiredDateStr = Convert.ToDateTime(header.RequiredDate).ToString("dd/MM/yyyy");
                    headerTable.AddCell(new Phrase(requiredDateStr, normalFont));

                    headerTable.AddCell(new Phrase("Expected Date:", normalFont));
                   // string expdate = Convert.ToDateTime(header.RequiredDate).ToString("dd/MM/yyyy");
                    headerTable.AddCell(new Phrase(expdate.ToString("dd/MM/yyyy"), normalFont));

                    headerTable.AddCell(new Phrase("Delivery Address:", normalFont));
                    headerTable.AddCell(new Phrase(header.Address, normalFont));

                    headerTable.AddCell(new Phrase("Note:", normalFont));
                    headerTable.AddCell(new Phrase(header.Description, normalFont));

                    headerTable.SpacingAfter = 20f;
                    document.Add(headerTable);

                    PdfPTable itemsTable = new PdfPTable(4);
                    itemsTable.WidthPercentage = 100;
                    itemsTable.SetWidths(new float[] { 25f, 40f, 15f, 20f });

                    var boldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
                    itemsTable.AddCell(new PdfPCell(new Phrase("Item", boldFont)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = BaseColor.LIGHT_GRAY });
                    itemsTable.AddCell(new PdfPCell(new Phrase("Description", boldFont)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = BaseColor.LIGHT_GRAY });
                    itemsTable.AddCell(new PdfPCell(new Phrase("Quantity", boldFont)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = BaseColor.LIGHT_GRAY });
                    itemsTable.AddCell(new PdfPCell(new Phrase("UOM", boldFont)) { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = BaseColor.LIGHT_GRAY });

                    foreach (var item in items)
                    {
                        itemsTable.AddCell(new PdfPCell(new Phrase(item.ItemName, normalFont)));
                        itemsTable.AddCell(new PdfPCell(new Phrase(item.Description, normalFont)));
                        itemsTable.AddCell(new PdfPCell(new Phrase(item.RequiredQuantity.ToString("N2"), normalFont)) { HorizontalAlignment = Element.ALIGN_RIGHT });
                        itemsTable.AddCell(new PdfPCell(new Phrase(item.UOMNamee, normalFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                    }

                    document.Add(itemsTable);

                    document.Close();
                }
            }

            return filePath;
        }


        private void SendEmailWithAttachmentSJ(string toEmail, string subject, string body, string attachmentPath)
        {
            var fromAddress = new MailAddress(WebConfigurationManager.AppSettings["MainEmail"], "Procurement System");
            string fromPassword = WebConfigurationManager.AppSettings["AppPassword"]; // ⚠️ Move this to config or environment variable

            using (var smtp = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            })
            using (var message = new MailMessage(fromAddress, new MailAddress(toEmail))
            {
                Subject = subject,
                Body = body
            })
            {
                if (!string.IsNullOrEmpty(attachmentPath) && System.IO.File.Exists(attachmentPath))
                {
                    var attachment = new Attachment(attachmentPath, "application/pdf");
                    message.Attachments.Add(attachment);
                }

                smtp.Send(message);
            }
        }




        //Fetch Country
        [HttpGet]
        public async Task<ActionResult> GetCountriesSJ()
        {

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            HttpClient client = new HttpClient();

            client.BaseAddress = new Uri("https://api.countrystatecity.in/v1/");

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            client.DefaultRequestHeaders.Add("X-CSCAPI-KEY", "YmhEenRQYTJJaXJ1VGp6TFJ4MTZkN3dYc1g2cUFacUQwWlhlbXcwVw==");

            var response = await client.GetAsync("countries");

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            return Content(json, "application/json");
        }
        //Fetch State
        public async Task<ActionResult> GetStateSJ(string countrycode)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://api.countrystatecity.in/v1/");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("X-CSCAPI-KEY", "YmhEenRQYTJJaXJ1VGp6TFJ4MTZkN3dYc1g2cUFacUQwWlhlbXcwVw==");
            var response = await client.GetAsync($"countries/{countrycode}/states");

            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return Content(json, "application/json");

        }

        //Fetch City
        public async Task<ActionResult> GetCitiesSJ(string countryCode, string stateCode)
        {
            HttpClient client = new HttpClient();

            client.BaseAddress = new Uri("https://api.countrystatecity.in/v1/");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("X-CSCAPI-KEY", "YmhEenRQYTJJaXJ1VGp6TFJ4MTZkN3dYc1g2cUFacUQwWlhlbXcwVw==");

            var response = await client.GetAsync($"countries/{countryCode}/states/{stateCode}/cities");

            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();


            return Content(json, "application/json");
        }

        //Fetch Industry Type for Vendor Registration 
        public async Task<JsonResult> FetchIndustryTypeSJ()
        {
            DataSet ds = new DataSet();
            List<Purchase> lstType = new List<Purchase>();
            ds = await bal.FetchIndustryTypeSJ();
            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
            {
                Purchase itype = new Purchase();
                itype.IndustryTypeId = Convert.ToInt32(ds.Tables[0].Rows[i]["IndustryTypeId"].ToString());
                itype.IndustryType = ds.Tables[0].Rows[i]["IndustryType"].ToString();
                lstType.Add(itype);
            }
            return Json(new { lsttype = lstType }, JsonRequestBehavior.AllowGet);
        }

        //Fetch BanKs ans SwiftCode
        public async Task<JsonResult> FetchBankAndSwiftCodeSJ()
        {
            List<Purchase> lstBank = new List<Purchase>();
            DataSet ds = await bal.FetchBankAndSwiftCodeSJ();
            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
            {
                Purchase bank = new Purchase();
                bank.BankId = Convert.ToInt32(ds.Tables[0].Rows[i]["BankId"].ToString());
                bank.BankName = ds.Tables[0].Rows[i]["BankName"].ToString();
                bank.SwiftCode = ds.Tables[0].Rows[i]["SwiftCode"].ToString();
                lstBank.Add(bank);
            }
            return Json(new { banks = lstBank }, JsonRequestBehavior.AllowGet);
        }

        //Fetch Branch And IFSCCode
        public async Task<JsonResult> FetchBranchAndIFSCCodeSJ(int BankId)
        {
            List<Purchase> lstBranch = new List<Purchase>();
            DataSet ds = await bal.FetchBranchAndIFSCCodeSJ(BankId);
            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
            {
                Purchase branch = new Purchase();
                branch.BranchId = Convert.ToInt32(ds.Tables[0].Rows[i]["BranchId"].ToString());
                branch.BranchName = ds.Tables[0].Rows[i]["Branch"].ToString();
                branch.IFSCCode = ds.Tables[0].Rows[i]["IFSCCode"].ToString();
                lstBranch.Add(branch);
            }
            return Json(new { Branch = lstBranch }, JsonRequestBehavior.AllowGet);
        }

        //Save Vendor And VenodrCompany and AccountDetails 
        [HttpPost]
        public async Task<ActionResult> SaveVendorDetailsSJ(Purchase p)
        {
            try
            {
                int VendorMaxId = 0;
                int VendorcompanyMaxId = 0;
                string staffCode = Session["StaffCode"]?.ToString();
                string date = DateTime.Now.ToString();

                DataSet ds = await bal.FetchVendorandVendorCompantMaxIdSJ();
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    VendorMaxId = Convert.ToInt32(ds.Tables[0].Rows[i]["VendorMaxID"].ToString());
                }
                for (int i = 0; i < ds.Tables[1].Rows.Count; i++)
                {
                    VendorcompanyMaxId = Convert.ToInt32(ds.Tables[1].Rows[i]["VendorCompanyMaxId"].ToString());
                }

                string VendorCode = "VEN" + (VendorMaxId + 1).ToString("D3");
                string VendorCompanyCode = "VCC" + (VendorcompanyMaxId + 1).ToString("D3");

                p.VendorCode = VendorCode;
                p.VendorCompanyCode = VendorCompanyCode;
                p.StaffCode = staffCode;   // ✅ assign here

                bool issaved = await bal.SaveVendorSJ(p);

                if (issaved)
                {
                    return Json(new { success = true, message = "Vendor saved successfully!" });
                }
                else
                {
                    return Json(new { success = false, message = "Vendor not saved, please try again!" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        #endregion
    }
}