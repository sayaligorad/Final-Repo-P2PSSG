using P2PLibray.Account;
using P2PLibray.Inventory;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using static P2PLibray.Inventory.Inventory;
using System.IO;

namespace P2PERP.Controllers
{
    public class InventoryController : Controller
    {
        BALInventory bal = new BALInventory();
        // GET: InventoryP2P
        [Route("Inventory/InventoryDashboard")]
        [HttpGet]
        public ActionResult Index()
        {
            var RoleId = Convert.ToInt32(Session["RoleId"]);

            switch (RoleId)
            {
                case 2: return View();
                case 3: return RedirectToAction("ReceiveMaterialDRB");
                case 4: return RedirectToAction("ShowStocklevelMHB");
            }
            return View();
        }

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


        [Route("Inventory/SendMail")]
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

        #region Rutik
        // ---------------------- INVENTORY STOCK ----------------------

        // Get overall stock count
        public async Task<JsonResult> GetInventoryStockHSB(DateTime? fromDate, DateTime? toDate, string category)
        {
            var result = await bal.GetInventoryStockCountHSB(fromDate, toDate, category);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        // Get detailed stock (grouped by category or other logic)
        public async Task<JsonResult> GetInventoryStockDetailsHSB(DateTime? fromDate, DateTime? toDate, string category)
        {
            var result = await bal.GetInventoryStockDetailsHSB(fromDate, toDate, category);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        // ---------------------- CATEGORY ----------------------

        // Get top-level categories
        public async Task<JsonResult> GetItemsCategoryHSB(DateTime? fromDate, DateTime? toDate, string category)
        {
            var result = await bal.GetCategoryHSB(fromDate, toDate, category);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        // Get detailed category data
        public async Task<JsonResult> GetCategoryDetailsHSB(DateTime? fromDate, DateTime? toDate, string category)
        {
            var result = await bal.GetCategoryDetailsHSB(fromDate, toDate, category);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        // ---------------------- RECEIVED MATERIAL ----------------------

        // Get received items count per day
        public async Task<JsonResult> GetRecivedItemsPerDayCountHSB(DateTime? fromDate, DateTime? toDate, string category)
        {
            var result = await bal.ReciveItemsCountHSB(fromDate, toDate, category);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        // Get received material details
        public async Task<JsonResult> GetReceiveMaterialDetailsHSB(DateTime? fromDate, DateTime? toDate, string category)
        {
            var result = await bal.GetReceiveMaterialDetails(fromDate, toDate, category);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        // ---------------------- ISSUE IN-HOUSE ----------------------

        // Get issued items count
        public async Task<JsonResult> GetIssueInHouseHSB(DateTime? fromDate, DateTime? toDate, string category)
        {
            var result = await bal.getIssueInHouseCounttHSB(fromDate, toDate, category);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        // Get issued items details
        public async Task<JsonResult> GetIssueInHouseDetailsHSB(DateTime? fromDate, DateTime? toDate, string category)
        {
            var result = await bal.GetIssueInHouseDetails(fromDate, toDate, category);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        // ---------------------- BIN ----------------------

        // Get total bin count
        public async Task<JsonResult> GetTotalBinHSB()
        {
            var result = await bal.GetTotalBinHSB();
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Divyani

        // Loads the Receive Material view
        public ActionResult ReceiveMaterialDRB()
        {
            return View();
        }

        // Gets the list of received materials from DB and returns JSON data
        public async Task<JsonResult> getRecevieMaterialDRB()
        {
            SqlDataReader dr = await bal.GetReceiveMaterialDRB();

            List<InventoryDRB> ReceiveMaterialList = new List<InventoryDRB>();

            for (int i = 0; i < dr.FieldCount; i++)
            {
                System.Diagnostics.Debug.WriteLine("Column: " + dr.GetName(i));
            }

            if (dr.HasRows)
            {
                while (await dr.ReadAsync())
                {
                    InventoryDRB ReceiveMaterial = new InventoryDRB
                    {
                        GRNCode = dr["GRNCode"].ToString(),
                        AddedDate = Convert.ToDateTime(dr["AddedDate"]).ToString("dd/MM/yyyy"),
                    };

                    ReceiveMaterialList.Add(ReceiveMaterial);
                }
            }

            dr.Close();

            return Json(new { data = ReceiveMaterialList }, JsonRequestBehavior.AllowGet);
        }

        // Gets the Issue Codes from DB and returns as JSON
        public async Task<JsonResult> getIssueCodeDRB()
        {
            SqlDataReader dr = await bal.GetIssueCodeDRB();

            List<IssueItemViewModelDRB> IssueList = new List<IssueItemViewModelDRB>();

            if (dr.HasRows)
            {
                while (await dr.ReadAsync())
                {
                    IssueItemViewModelDRB ReceiveMaterial = new IssueItemViewModelDRB
                    {
                        IssueCode = dr["IssueCode"].ToString()
                    };

                    IssueList.Add(ReceiveMaterial);
                }
            }

            dr.Close();

            return Json(IssueList, JsonRequestBehavior.AllowGet);
        }

        // Loads the Issue In-House view
        [HttpGet]
        public ActionResult IssueInHouseDRB()
        {
            return View();
        }

        // Loads the Issue In-House view
        [HttpGet]
        public async Task<JsonResult> GetBinBasedOnItemDRB(string itemcode)
        {
            var bins = await bal.GetBins(itemcode);
            return Json(bins,JsonRequestBehavior.AllowGet);
        }

        // Loads the Issue In-House view
        [HttpGet]
        public async Task<JsonResult> GetBinNameItemDRB(string itemcode)
        {
            var bins = await bal.GetBinsName(itemcode);
            return Json(bins, JsonRequestBehavior.AllowGet);
        }

        // Gets the list of Departments from DB and returns JSON
        [HttpGet]
        public async Task<JsonResult> GetDepartmentsDRB()
        {
            DataSet ds = await bal.GetDepartmentsDRB();

            var departmentList = ds.Tables[0].AsEnumerable().Select(row => new
            {
                DeptID = Convert.ToInt32(row["DepartmentId"]),
                DepartmentName = row["DepartmentName"].ToString()
            }).ToList();

            return Json(departmentList, JsonRequestBehavior.AllowGet);
        }

        // Gets the list of In-Stock items from DB and returns JSON
        [HttpGet]
        public async Task<JsonResult> GetInStockItemsDRB()
        {
            DataSet ds = await bal.GetInstockItemsDRB();

            var InStockItemsList = ds.Tables[0].AsEnumerable().Select(row => new
            {
                ItemCode = row["ItemCode"].ToString(),
                ItemName = row["ItemName"].ToString(),
                ItemsCounts = row["QuantityStored"].ToString(),
                ItemCategoryName = row["ItemCategoryName"].ToString(),
                UOMName = row["UOMName"].ToString(),
                UnitPrice = row["UnitRates"].ToString()
            }).ToList();

            return Json(InStockItemsList, JsonRequestBehavior.AllowGet);
        }

        // Gets the Employees list by Department Id
        [HttpGet]
        public async Task<JsonResult> EmployeesByDepartmentDRB(int Did)
        {
            DataSet ds = await bal.GetEmployeeDRB(Did);

            if (ds == null || ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
            {
                return Json(new List<object>(), JsonRequestBehavior.AllowGet);
            }

            var employeeList = ds.Tables[0].AsEnumerable().Select(row => new
            {
                StaffCode = row["StaffCode"].ToString(),
                EmployeeName = row["FullName"].ToString()
            }).ToList();

            return Json(employeeList, JsonRequestBehavior.AllowGet);
        }

        // Gets the list of Warehouses from DB
        [HttpGet]
        public async Task<JsonResult> GetWareHouseDRB()
        {
            List<InventoryWareHouseDRB> grnItemsList = await bal.GetWareHouseDRB();

            return Json(grnItemsList, JsonRequestBehavior.AllowGet);
        }

        // Gets the list of Sections for a given Warehouse
        [HttpGet]
        public async Task<JsonResult> GetSectionDRB(string code)
        {
            List<InventorySectionDRB> grnItemsList = await bal.GetSectionDRB(code);

            return Json(grnItemsList, JsonRequestBehavior.AllowGet);
        }

        // Gets the list of Racks for a given Section
        [HttpGet]
        public async Task<JsonResult> GetRackDRB(string Sectioncode)
        {
            List<InventoryRackDRB> grnItemsList = await bal.GetRackDRB(Sectioncode);

            return Json(grnItemsList, JsonRequestBehavior.AllowGet);
        }

        // Gets the list of Rows for a given Rack
        [HttpGet]
        public async Task<JsonResult> GetRowDRB(string Rackcode)
        {
            List<InventoryRowDRB> grnItemsList = await bal.GetRowDRB(Rackcode);

            return Json(grnItemsList, JsonRequestBehavior.AllowGet);
        }

        // Gets the list of Bins for a given Row
        [HttpGet]
        public async Task<JsonResult> GetBinDRB(string RowCode, string GRNItemCode)
        {
            List<InventoryBinDRB> bins = await bal.GetBinDRB(RowCode, GRNItemCode);
            return Json(bins, JsonRequestBehavior.AllowGet);
        }


        // Gets the list of Bins for a given Row
       

        // Gets the Issue In-House items based on Status Id
        [HttpGet]
        public async Task<JsonResult> IssueINHouseItemDRB(int StatusId)
        {
            var balInventory = new BALInventory();
            var grnItemsList = await balInventory.IssueINHouseDRB(StatusId);

            return Json(grnItemsList, JsonRequestBehavior.AllowGet);
        }

        // Gets GRN details for given GRN code and returns PartialView
        public async Task<ActionResult> GetGRNDetailsPartialDRB(string GRNCode)
        {
            var model = await bal.GetGRNDetailsDRB(GRNCode);
            return PartialView("_ViewReciveMaterialDB", model);
        }

        // Gets In-Stock details for given GRN code and returns PartialView
        public async Task<ActionResult> InstockPartialDRB(string GRNCode)
        {
            var model = await bal.GetInStockDRB(GRNCode);
            return PartialView("_InStockDB", model);
        }

        // Saves Item Bin Assignment data
        [HttpPost]
        public async Task<ActionResult> SaveItemBinAssignmentDRB(List<SaveItemBinAssignmentDRB> assignments)
        {
            foreach (var obj in assignments)
            {
                await bal.SaveGRHSBItemBinDRB(obj);
            }
            return Json(new { success = true });
        }

        // Saves Issue Header details in DB
        [HttpPost]
        public async Task<JsonResult> SaveIssueHeaderDRB(IssueHeaderModelDRB model)
        {
            try
            {
                await bal.SaveIssueHeaderModelDRB(model);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Saves Issue Details for the given Issue Header
        [HttpPost]
        public async Task<JsonResult> SaveIssueDetailsDRB(List<IssueDetailModelDRB> items, IssueHeaderModelDRB header)
        {
            try
            {
                foreach (var item in items)
                {
                    await bal.SaveIssueDetailDRB(item);
                }
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }




        #endregion

        #region Lavmesh
        // GET: InventoryP2P

        //Stock Cheak Main View
        public Task<ActionResult> StocksCheakLM()
        {

            return Task.FromResult<ActionResult>(View());
        }

        //Current Stock Partial View
        public ActionResult _currentStocksLM()
        {
            return View();
        }

        //NonMoving Stock Partial View
        public ActionResult _nonMovingSLM()
        {
            return View();
        }

        //Quality Cheak Stock Partial View
        public ActionResult _qualityCheakLM()
        {
            return View();
        }

        /* Json Methods */

        //Current Stock Json
        [HttpGet]
        public async Task<JsonResult> CurrentStockJsonLM()
        {
            var currentList = await bal.ShowCurrentStockLM();
            return Json(new { data = currentList }, JsonRequestBehavior.AllowGet);
        }

        //NonMoving Stock Json
        [HttpGet]
        public async Task<JsonResult> NonMovingStockJsonLM()
        {
            var nonMlist = await bal.ShowNonMovingStockLM();
            return Json(new { data = nonMlist }, JsonRequestBehavior.AllowGet);
        }


        //Transfer Stock Json
        [HttpPost]
        public JsonResult TransferNonMovingToMovingByBinLM(string itemCode, int transferQty, string binCode)
        {
            string connStr = ConfigurationManager.ConnectionStrings["P2PERP"].ConnectionString;

            bool result = bal.TransferNonMovingToMovingLM(itemCode, transferQty, binCode, connStr);

            if (result)
                return Json(new { success = true });
            else
                return Json(new { success = false, message = "Transfer failed" });
        }

        //Quality Cheak Stock Json
        [HttpGet]
        public async Task<JsonResult> QualityCheackJsonLM()
        {
            var qcList = await bal.ShowQCSTockLM();
            return Json(new { data = qcList }, JsonRequestBehavior.AllowGet);
        }


        #endregion

        #region Rushikesh

        // =====================================================================
        // REQUIREMENT MASTER SECTION
        // Handles item requirement management operations
        // =====================================================================

        /// <summary>
        /// Requirement Master main view
        /// Displays list of item requirements
        /// </summary>
        public ActionResult ReqMasterRHK()
        {
            return View();
        }

        /// <summary>
        /// Retrieves list of requirement master records as JSON
        /// Used for populating requirement master grid/tables
        /// </summary>
        public async Task<JsonResult> ReqMasterListRHK()
        {
            // Get requirement master data from business layer
            DataSet ds = await bal.ReqMasterRHK();
            List<Inventory> reqlist = new List<Inventory>();

            // Process data if available
            if (ds != null && ds.Tables.Count > 0)
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    reqlist.Add(new Inventory
                    {
                        StockRequirementId = Convert.ToInt32(dr["StockReqirementId"]),
                        AddedDate = Convert.ToDateTime(dr["AddedDate"]),
                        Status = dr["StatusName"].ToString(),
                    });
                }
            }

            // Return JSON data for client-side processing
            return Json(new { data = reqlist }, JsonRequestBehavior.AllowGet);
        }



        /// <summary>
        /// Partial view for requirement master details (for modal display)
        /// </summary>
        /// <param name="id">Stock Requirement ID to view</param>
        public ActionResult ReqMasterViewPartialRHK(int id)
        {
            return PartialView("_ReqMasterViewRHK", id); // For modal display
        }

        /// <summary>
        /// Retrieves detailed list of items for a specific requirement
        /// </summary>
       
        public async Task<ActionResult> ViewReqMasterListRHK()
        {
            try
            {
               
                // Get detailed requirement data from business layer
                DataSet ds = await bal.ViewReqMasterRHK();

                Console.WriteLine($"DataSet tables count: {ds?.Tables?.Count}");
                if (ds != null && ds.Tables.Count > 0)
                {
                    Console.WriteLine($"Table 0 row count: {ds.Tables[0].Rows.Count}");
                }

                List<Inventory> reqlist = new List<Inventory>();

                // Process data if available
                if (ds != null && ds.Tables.Count > 0)
                {
                    foreach (DataRow r in ds.Tables[0].Rows)
                    {
                        reqlist.Add(new Inventory
                        {
                            ItemCode = r["ItemCode"].ToString(),
                            ItemName = r["ItemName"].ToString(),
                            Description = r["Description"].ToString(),
                            RequiredQuantity = r["RequiredQuantity"].ToString(),
                            RequiredDate = Convert.ToDateTime(r["RequiredDate"]),
                            RequestType = r["RequestType"].ToString(),
                            AddedBy = r["FullName"].ToString(),
                            AddedDate = Convert.ToDateTime(r["AddedDate"])
                        });
                    }
                }

                Console.WriteLine($"Returning {reqlist.Count} items");

                // Return JSON data for client-side display
                return Json(new { data = reqlist }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // Error handling and logging
                Console.WriteLine($"Error: {ex.Message}");
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // =====================================================================
        // STOCK MASTER SECTION
        // Handles material requirement planning operations
        // =====================================================================

        /// <summary>
        /// Stock Master main view
        /// Displays list of material requirement plans
        /// </summary>
        public ActionResult StockMasterRHK()
        {
            return View();
        }

        /// <summary>
        /// Retrieves list of stock master records as JSON
        /// Used for populating stock master grid/tables
        /// </summary>
        public async Task<JsonResult> StockMasterListRHK()
        {
            // Get stock master data from business layer
            DataSet ds = await bal.StockMasterRHK();
            List<Inventory> reqlist = new List<Inventory>();

            // Process data if available
            if (ds != null && ds.Tables.Count > 0)
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    reqlist.Add(new Inventory
                    {
                        MRPCode = dr["MaterialReqPlanningCode"].ToString(),
                        PlanName = dr["PlanName"].ToString(),
                        AddedDate = Convert.ToDateTime(dr["CreatedDate"]),
                        Status = dr["StatusName"].ToString(),
                    });
                }
            }

            // Return JSON data for client-side processing
            return Json(new { data = reqlist }, JsonRequestBehavior.AllowGet);
        }



        /// <summary>
        /// Partial view for plan details (for modal display)
        /// </summary>
        /// <param name="id">Material Requirement Planning Code to view</param>
        public ActionResult ViewPlanPartial(string id)
        {
            return PartialView("_ViewPlanPartialRHK", id); // For modal display
        }

        /// <summary>
        /// Retrieves detailed item information for a specific material requirement plan
        /// </summary>
        /// <param name="code">Material Requirement Planning Code</param>
        public async Task<JsonResult> PlanDetailsRHK(string code)
        {
            // Get plan details from business layer
            DataSet ds = await bal.ViewPlanRHK(code);
            List<Inventory> reqlist = new List<Inventory>();

            // Process data if available
            if (ds != null && ds.Tables.Count > 0)
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    reqlist.Add(new Inventory
                    {
                        ItemCode = dr["ItemCode"].ToString(),
                        ItemName = dr["ItemName"].ToString(),
                        RequiredQuantity = dr["Quantity"].ToString(),
                        UOM = dr["UOMName"].ToString(),
                        FromDate = Convert.ToDateTime(dr["FromDate"]).ToString("yyyy-MM-dd"),
                        ToDate = Convert.ToDateTime(dr["ToDate"]).ToString("yyyy-MM-dd"),
                    });
                }
            }

            // Return JSON data for client-side display
            return Json(new { data = reqlist }, JsonRequestBehavior.AllowGet);
        }
    #endregion Rushikesh

        #region Akash
    
        /// <summary>
        /// Gets current stock report data (JSON result).
        /// </summary>
        public async Task<JsonResult> CurrentStockReportDataAMG()
        {
            try
            {
                var data = await bal.GetCurrentStockAMG();
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Loads Current Stock Report view.
        /// </summary>
        public ActionResult CurrentStockReportAMG()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(500, ex.Message);
            }
        }

        /// <summary>
        /// Loads Warehouse Utilization Report view.
        /// </summary>
        [HttpGet]
        public ActionResult WarehouseUtilizationReportAMG()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(500, ex.Message);
            }
        }

        /// <summary>
        /// Loads Received Material Report view.
        /// </summary>
        [HttpGet]
        public ActionResult ReceivedMaterialReportAMG()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(500, ex.Message);
            }
        }

        /// <summary>
        /// Gets Received Material Report data between given dates (JSON result).
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetReceivedMaterialReportAMG(DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                var data = await bal.GetReceivedMaterialReportAMG(fromDate, toDate) ?? new List<ReceivedMaterialReport>();
                return Json(new { data = data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { data = new List<ReceivedMaterialReport>(), error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Loads Inhouse Transfer Report view.
        /// </summary>
        public ActionResult InhouseTransferReportVAMG()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(500, ex.Message);
            }
        }

        /// <summary>
        /// Gets Inhouse Transfer Report data between given dates (JSON result).
        /// </summary>
        public async Task<JsonResult> InhouseTransferReportDataAMG(DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                var data = await bal.GetInhouseTransferReportAMG(fromDate, toDate);
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { data = new List<object>(), error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region Saurabh
        // GET: InventoryP2P
        public ActionResult WHIndex()
        {
            return View();
        }
        /// <summary>
        /// THIS IS USING FOR WAREHOUSE ALL METHOD
        /// </summary>
        /// <returns></returns>
        /// 


        //LIST OF WAREHOUSE 
        public async Task<ActionResult> GetWarehousesSK()
        {
            var WarehouseData = await bal.GetWarehousesAsyncSK();
            return Json(new { data = WarehouseData }, JsonRequestBehavior.AllowGet);
        }

        //THIS USE FOR SAVE WAREHOUSE 
        [HttpPost]
		public async Task<ActionResult> AddWarehouseSK(InventorySK model)
		{
			try
			{
				if (ModelState.IsValid)
				{
					model.AddedBy = Session["StaffCode"].ToString();
					model.AddedDate = DateTime.Now;

					var (Success, Message, NewId) = await bal.AddWarehouseAsyncSK(model);

					if (Success)
						return Json(new { success = true, message = Message, newId = NewId });

					// ✅ If duplicate name detected
					if (Message.Contains("already"))
						return Json(new { success = false, message = Message, field = "WarehouseName" });

					return Json(new { success = false, message = Message });
				}
				return Json(new { success = false, message = "Invalid data" });
			}
			catch (Exception ex)
			{
				return Json(new { success = false, message = ex.Message });
			}
		}




		//  Update Warehouse GET ID  AND USE FOR VIEW    //////////////////////Options///////////////////////
		[HttpGet]
        public async Task<ActionResult> GetWarehouseById(int id)
        {
            try
            {
                var warehouse = await bal.GetWarehouseByIdAsyncSK(id);
                if (warehouse != null)
                    return Json(new { success = true, data = warehouse });
                else
                    return Json(new { success = false, message = "Warehouse not found" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        //   Update Warehouse
        [HttpPost]
		public async Task<ActionResult> UpdateWarehouseSK(InventorySK model)
		{
			try
			{
				if (ModelState.IsValid)
				{
					var (Success, Message) = await bal.UpdateWarehouseAsyncSK(model);

					if (Success)
						return Json(new { success = true, message = Message });

					return Json(new { success = false, message = Message, field = "WarehouseName" });
				}
				return Json(new { success = false, message = "Invalid data" });
			}
			catch (Exception ex)
			{
				return Json(new { success = false, message = ex.Message });
			}
		}

		//        View Warehouse Details
		[HttpGet]
        public async Task<ActionResult> ViewWarehouseSK(int id)
        {
            try
            {

                var warehouse = await bal.GetWarehouseByIdAsyncSK(id);
                if (warehouse != null)
                {
                    return Json(new { success = true, data = warehouse }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { success = false, message = "Warehouse not found" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        //        Delete Warehouse BY USING ID 

        [HttpPost]
        public async Task<ActionResult> DeleteWarehouseSK(int id)
        {
            try
            {
                await bal.DeleteWarehouseAsyncSK(id);
                return Json(new { success = true, message = "Warehouse deleted successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }



        // THIS IS USE FOR NEXT WAREHOUSE CODE 
        [HttpGet]
        public async Task<JsonResult> GetNextWarehouseCodeSK()
        {
            try
            {
                var nextCode = await bal.GetNextWarehouseCodeAsyncSK();
                return Json(new { success = true, code = nextCode }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// ////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////// Get Countries, States, Cities//////////////////////////////////////////
        /// </summary>
        /// <returns></returns>












        //   LIST COUNTRY
        [HttpGet]
        public async Task<JsonResult> GetCountries()
        {
            var list = await bal.GetCountries();
            return Json(list, JsonRequestBehavior.AllowGet);
        }
        //   LIST STATE
        [HttpGet]
        public async Task<JsonResult> GetStates(int countryId)
        {
            var list = await bal.GetStates(countryId);
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        //   LIST CITIES
        [HttpGet]
        public async Task<JsonResult> GetCities(int stateId)
        {
            var list = await bal.GetCities(stateId);
            return Json(list, JsonRequestBehavior.AllowGet);
        }


        ////////////////////////////////////////////////////////////////////////////RACK//////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// THIS IS ALL METHODS FOR RACKS 
        /// </summary>
        /// <returns></returns>
        /// 

        //  LIST FOR RACKS
        public async Task<ActionResult> GetRacksSK()
        {
            var WarehouseData = await bal.GetRacksAsyncSK();
            return Json(new { data = WarehouseData }, JsonRequestBehavior.AllowGet);
        }


        //   NEXT RACK CODE 
        public async Task<ActionResult> GetNextRackCodeSK()
        {
            var code = await bal.GetNextRackCodeAsyncSK();
            return Json(new { NextCode = code }, JsonRequestBehavior.AllowGet);
        }

        //  SAVE RACK
        [HttpPost]
		public async Task<ActionResult> SaveRackSK(InventorySK model)
		{
			model.AddedBy = Session["StaffCode"].ToString();
			model.AddedDate = DateTime.Now;

			try
			{
				var result = await bal.SaveRackAsyncSK(model);

				return Json(new
				{
					success = result.Success,
					message = result.Message
				}, JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				return Json(new
				{
					success = false,
					message = "Error: " + ex.Message
				}, JsonRequestBehavior.AllowGet);
			}
		}

		//    THIS IS USED FOR VIEW AND UPDATE USING ID    RACK
		public async Task<ActionResult> GetRackByIdSKK(int id)
        {
            try
            {
                var rack = await bal.GetRackByIdAsyncSK(id);
                if (rack != null)
                {
                    return Json(rack, JsonRequestBehavior.AllowGet);
                }
                return Json(new { success = false, message = "Rack not found" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        //    THIS IS USED BY DELETE RACK BY ID
        [HttpPost]
        public async Task<ActionResult> DeleteRackSK(int id)
        {
            try
            {
                var result = await bal.DeleteRackAsyncSK(id);

                return Json(new
                {
                    success = result.Success,
                    message = result.Message
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Error: " + ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }


        //  THIS IS WAREHOUSE LIST SAVE RACK USING DROPDOWEN FOR SECTION 
        public async Task<ActionResult> GetWarehousesListSK()
        {
            var data = await bal.GetWarehouseslistSK();
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        //  SECTION LIST FOR WAREHOUSE CODE 
        public async Task<ActionResult> GetSectionsSKK(string warehouseCode)
        {
            var data = await bal.GetSectionsByWarehouseAsyncSK(warehouseCode);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        //============================================================ROW==================================
        /// <summary>
        /// THIS IS USING FOR ALL ROW METHODS 
        /// </summary>
        /// <param name="sectionCode"></param>
        /// <returns></returns>
        /// 

        //  THIS IS A RACK DEPEND FOR SECTION 

        public async Task<ActionResult> GetRackss(string sectionCode)
        {
            var data = await bal.GetRackBySectionAsyncSK(sectionCode);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        //  THIS IS ROW LIST 
        public async Task<ActionResult> RowsSKK()
        {

            var rows = await bal.GetRowsAsyncSK();
            return Json(new { data = rows }, JsonRequestBehavior.AllowGet);

        }


        //  THIS IS A NEXT ROW CODE 
        [HttpGet]
        public async Task<ActionResult> GetNextRowCodeSKK()
        {
            var code = await bal.GetNextRowCodeAsyncSK();
            return Json(new { RnextCode = code }, JsonRequestBehavior.AllowGet);
        }


        //   THIS IS A SAVE ROW 
        [HttpPost]
        public async Task<ActionResult> SaveRowSBK(InventorySK model)
        {
            model.AddedBy = Session["StaffCode"].ToString();
            model.AddedDate = DateTime.Now;
            try
            {
                var result = await bal.SaveRowAsyncSK(model);

                return Json(new
                {
                    success = result.Success,
                    message = result.Message
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Error: " + ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }


        //   THIS IS USED BY UPDATE AND VIEW BY ROW 
        [HttpGet]
        public async Task<ActionResult> GetRowByIdSSK(int rowId)
        {
            try
            {
                var row = await bal.GetRowByIdAsyncSK(rowId);
                return Json(row, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }
        //   DELETE ROW 


        [HttpPost]
        public async Task<ActionResult> DeleteRowSK(int rowId)
        {
            try
            {
                await bal.DeleteRowAsyncSK(rowId);
                return Json(new { success = true, message = "Row deleted successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


        //===============================================================    BINS    ==================================
        /// <summary>
        /// THIS IS USED BY BINS ALL METHODS
        /// </summary>
        /// <returns></returns>
        /// 

        //  THIS IS A BINS LIST
        [HttpGet]
        public async Task<ActionResult> BinsSKK()
        {

            var bins = await bal.GetBinsAsyncSK();
            return Json(new { data = bins }, JsonRequestBehavior.AllowGet);

        }

        //  THIS IS NEXT BIN CODE 
        [HttpGet]
        public async Task<ActionResult> GetNextBinCodeSKK()
        {
            var code = await bal.GetNextBinCodeAsyncSK();
            return Json(new { BnextCode = code }, JsonRequestBehavior.AllowGet);
        }

        //   THIS IS USED FOR ITEMS LIST 
        [HttpGet]
        public async Task<JsonResult> GetItemsSKK()
        {
            var list = await bal.GetItemslistSK();
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        //      THIS IS USED FOR ROWLIST DEPEND ON RACKCODE 
        [HttpGet]
        public async Task<ActionResult> GetRowsSBK(string rowCode)
        {
            var data = await bal.GetRowByRacksAsyncSK(rowCode);
            return Json(data, JsonRequestBehavior.AllowGet);
        }


        //   THIS IS USED BY SAVE BIN 
        [HttpPost]
		public async Task<ActionResult> SaveBinSKK(InventorySK model)
		{
			model.AddedBy = Session["StaffCode"].ToString();
			model.AddedDate = DateTime.Now;

			System.Diagnostics.Debug.WriteLine("Description from UI: " + model.Descriptions);

			var (success, message) = await bal.SaveBinAsyncSK(model);

			// ✅ If message indicates validation or constraint issue, mark as failure
			if (message.Contains("Bin with same name") ||
				message.Contains("Max Quantity") ||
				message.Contains("Cannot update") ||
				message.Contains("required") ||
				!success)
			{
				return Json(new { success = false, message });
			}

			// ✅ Otherwise treat as success
			return Json(new { success = true, message });
		}



		//   THIS IS USED BY UPDATE AND VIEW FOR BIN 
		[HttpGet]
        public async Task<ActionResult> GetBinByIdSBK(int binId)
        {
            try
            {
                var row = await bal.GetBinByIdAsyncSK(binId);
                return Json(row, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }


        //   THIS IS USED BY DELETE BIN 
        [HttpPost]
        public async Task<ActionResult> DeleteBinSBK(int binId)
        {
            try
            {
                await bal.DeleteBinAsyncSK(binId);
                return Json(new { success = true, message = "Bin deleted successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        //////////////////////////////////////////////////////////////////////////////// SECTION //////////////////////////////////////////////////////////////////////////
        /// <summary>
        ///    THIS IS ALL METHODS FOR SECTION
        /// </summary>
        /// <returns></returns>
        /// 


        //     THIS IS USED FOR SECTION LIST
        [HttpGet]
        public async Task<ActionResult> GetSectionsSK()
        {
            var sections = await bal.GetSectionsAsyncSK();
            return Json(new { data = sections }, JsonRequestBehavior.AllowGet);
        }


        //    THIS IS USED FOR NEXT SECTION CODE 
        [HttpGet]
        public async Task<ActionResult> GetNextSectionCodeAsyncSK()
        {
            try
            {
                var Scode = await bal.GetNextSectionCodeAsyncSK();
                return Json(new { success = true, code = Scode }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        //      THIS IS USED FOR SAVE SECTION

        [HttpPost]
		public async Task<ActionResult> AddSection(InventorySK model)
		{
			if (ModelState.IsValid)
			{
				try
				{
					var (Success, Message) = await bal.AddSectionAsyncSK(model);

					if (Success)
						return Json(new { success = true, message = Message });

					return Json(new { success = false, message = Message, field = "SectionName" });
				}
				catch (Exception ex)
				{
					return Json(new { success = false, message = ex.Message });
				}
			}
			return Json(new { success = false, message = "Invalid data." });
		}


		//    THIS IS USED BY UPDATE AND VIEW BY USING ID 
		[HttpGet]
        public async Task<ActionResult> GetSectionByIdSK(int id)
        {
            try
            {
                var section = await bal.GetSectionByIdAsyncSK(id); // BAL call
                if (section != null)
                {
                    return Json(new { success = true, data = section }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = false, message = "Section not found." }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        //  UPDATE SECTION
        [HttpPost]
		public async Task<ActionResult> UpdateSectionSK(InventorySK model)
		{
			try
			{
				var (Success, Message) = await bal.UpdateSectionAsyncSK(model);

				if (Success)
					return Json(new { success = true, message = Message });

				return Json(new { success = false, message = Message, field = "SectionName" });
			}
			catch (Exception ex)
			{
				return Json(new { success = false, message = ex.Message });
			}
		}


		//  DELETE SECTION


		[HttpPost]
        public async Task<JsonResult> DeleteSectionSK(int sectionId)
        {
            try
            {
                // Await ke sath call karo
                bool isDeleted = await bal.DeleteSectionAsyncSK(sectionId);

                if (isDeleted)
                {
                    return Json(new { success = true, message = "Section deleted successfully!" });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to delete section." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }
        #endregion

        #region Mayur
        // Loads the stock level view for MHB location.
        public ActionResult ShowStocklevelMHB()

        {
            return View();
        }

        // Fetches stock planning data for MHB and returns JSON.
        public async Task<ActionResult> StockMHB()
        {
            var data = await bal.StockplanningMHB();
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        // Loads item-specific stock form partial view for MHB.
        public async Task<ActionResult> stockformMHB(string itemId)
        {
            var model = await bal.GetItemByIdMHB(itemId);
            return PartialView("_StockFormMHB", model);
        }

        // Submits a purchase request (PR) for MHB items.
        [HttpPost]
        public async Task<ActionResult> SubmitPRMHB(P2PLibray.Inventory.InventoryMHB model)
        {
            //model.StaffCode = "STF020";
            model.StaffCode = Session["StaffCode"].ToString();

            if (ModelState.IsValid)
            {
                bool result = await bal.SavePRRequestMHB(model);
                if (result)
                {
                    TempData["SuccessMessage"] = "Purchase request submitted successfully.";
                    return RedirectToAction("ShowStocklevelMHB");
                }

                ModelState.AddModelError("", "Failed to save PR.");
            }

            return View("stockformMHB", model);
        }

        // Loads the Item Stock Refill (ISR) view for MHB.
        public ActionResult ISRStockMHB()
        {
            return View();
        }

        // Fetches item stock refill data for MHB and returns JSON.
        public async Task<JsonResult> ISRStock2MHB()
        {
            List<InventoryMHB> lst = new List<InventoryMHB>();
            DataSet ds = await bal.ItemStockRefillMHB();

            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    InventoryMHB SHR = new InventoryMHB
                    {
                        ItemName = row["ItemName"].ToString(),
                        ItemCode = row["ItemCode"].ToString(),
                        ISRQuantity = row["Quantity"].ToString(),
                        Description = row["Description"].ToString(),
                        RequiredDates = row["RequiredDate"].ToString(),
                        UOMName = row["UOMName"].ToString(),
                        RequestType = row["RequestType"].ToString(),
                        StatusName = row["StatusName"].ToString()
                    };
                    lst.Add(SHR);
                }
            }

            return Json(new { Data = lst }, JsonRequestBehavior.AllowGet);
        }

        // Loads the Just-In-Time (JIT) stock view for MHB.
        public ActionResult JITStockMHB()
        {
            return View();
        }

        // Returns the list of all items for dropdown binding.
        [HttpGet]
        public async Task<JsonResult> GetItemListMHB()
        {
            DataSet ds = await bal.ItemlistMHB();
            var items = new List<object>();

            if (ds != null && ds.Tables.Count > 0)
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    items.Add(new
                    {
                        ItemId = Convert.ToInt32(row["ItemId"]),
                        ItemName = row["ItemName"].ToString()
                    });
                }
            }

            return Json(items, JsonRequestBehavior.AllowGet);
        }

        // Returns detailed information about a selected item.
        [HttpGet]
        public async Task<JsonResult> GetItemDetailsMHB(int itemId)
        {
            DataSet ds = await bal.ItemlistMHB();
            object result = new { };

            if (ds != null && ds.Tables.Count > 0)
            {
                var table = ds.Tables[0];
                DataRow[] found = table.Select($"ItemId = {itemId}");
                if (found.Length > 0)
                {
                    DataRow row = found[0];
                    result = new
                    {
                        ItemId = itemId,
                        ItemCode = row["ItemCode"]?.ToString(),
                        Description = row["Description"]?.ToString(),
                        ReorderQuantity = row["ReorderQuantity"]?.ToString(),
                        UOMName = row["UOMName"]?.ToString()
                    };
                }
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        // Saves a Just-In-Time request to the database.
        [HttpPost]
        public async Task<JsonResult> SaveJustInTimeMHB(InventoryMHB model)
        {
            //model.StaffCode = "STF020";
            model.StaffCode = Session["StaffCode"].ToString();
            bool result = await bal.SaveJITMHB(model);
            return Json(new { success = result });
        }

        // Loads the MRP new planning view for MHB.
        public ActionResult NewplanMHB()
        {
            return View();
        }

        // Submits a new MRP plan for approval.
        [HttpPost]
        public async Task<JsonResult> SubmitPlanForApprovalMHB(InventoryMHB model)
        {
            model.StaffCode = Session["StaffCode"].ToString();
            bool result = await bal.SaveMRPMHB(model);
            return Json(new { success = result });
        }

        // Loads the manager approval dashboard view.
        public ActionResult ManagerApprovalMHB()
        {
            return View();
        }
         
        // Fetches all MRP plan headers for manager approval.
        [HttpGet]
        public async Task<JsonResult> FetchPlanDetailsMHB()
        {
            DataSet ds = await bal.fetchplandetailsMHB();
            var items = new List<object>();

            if (ds != null && ds.Tables.Count > 0)
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    items.Add(new
                    {
                        PlanName = row["PlanName"].ToString(),
                        MaterialReqPlanningCode = row["MaterialReqPlanningCode"].ToString(),
                        AddedDate = row["AddedDate"].ToString(),
                        StatusName = row["StatusName"].ToString(),
                    });
                }
            }

            return Json(items, JsonRequestBehavior.AllowGet);
        }

        // Loads the partial view for manager actions on a plan.
        public ActionResult viewActionForplanlMHB(string MRPcode)
        {
            ViewBag.MRPcode = MRPcode;
            return PartialView("_viewActionForplanlMHB");
        }

        // Displays full plan details for manager approval.
        public async Task<ActionResult> showplanforApprovalMHB(string MRPcode)
        {
            DataSet ds = await bal.showplanMHB(MRPcode);
            InventoryMHB data = new InventoryMHB();

            if (ds.Tables[0].Rows.Count > 0)
            {
                var row = ds.Tables[0].Rows[0];
                data.MRPCode = row["MaterialReqPlanningCode"].ToString();
                data.PlanName = row["PlanName"].ToString();
                data.Year = row["Year"].ToString();
                data.FromDate = row["FromDate"].ToString();
                data.ToDate = row["ToDate"].ToString();
            }

            foreach (DataRow row in ds.Tables[1].Rows)
            {
                data.ItemList.Add(new InventoryMHB
                {
                    ItemName = row["ItemName"].ToString(),
                    ItemCode = row["ItemCode"].ToString(),
                    QuantityMRP = row["Quantity"].ToString(),
                    UOMName = row["UOMName"].ToString(),
                    Description = row["Description"].ToString(),
                    RequestType = row["RequestType"].ToString()
                });
            }

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        // Approves a submitted MRP plan.
        [HttpPost]
        public async Task<ActionResult> ApprovePlanMHB(string MRPCode, InventoryMHB model)
        {
            model.StaffCode = Session["StaffCode"].ToString();
            bool result = await bal.UpdateMRPPlanStatusMHB(MRPCode, true, null, model);
            return Json(result);
        }

        // Rejects a submitted MRP plan with a reason.
        [HttpPost]
        public async Task<ActionResult> RejectPlanMHB(string MRPCode, string Reason, InventoryMHB model)
        {
            model.StaffCode = Session["StaffCode"].ToString();
            bool result = await bal.UpdateMRPPlanStatusMHB(MRPCode, false, Reason, model);
            return Json(result);
        }

        // Saves the changes made to an existing MRP plan.
        [HttpPost]
        public async Task<JsonResult> SaveEditedPlanMHB(InventoryMHB model)
        {
            if (ModelState.IsValid)
            {
                model.StaffCode = Session["StaffCode"].ToString();
                bool result = await bal.saveEditedPlanMHB(model);

                if (result)
                {
                    TempData["SuccessMessage"] = "Plan updated successfully!";
                    return Json(new { success = true, message = "Plan updated successfully!" }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { success = false, message = "Failed to update plan." }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { success = false, message = "Invalid model state." }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Sayali and Om



        public ActionResult ItemMasterOJ()
        {
            if (Session["StaffCode"] == null)
                return RedirectToAction("MainLogin", "Account");
            return View();
        }

        // Fetches all items from database and returns as JSON for DataTable
        [HttpGet]
        public async Task<ActionResult> GetItemsOJ()
        {
            var itemList = await bal.GetItemOJ();
            return Json(itemList, JsonRequestBehavior.AllowGet);
        }

        // Fetches specific plan details based on PlanCode
        public async Task<JsonResult> ShowPlanOJ(string planCode)
        {
            if (string.IsNullOrEmpty(planCode))
                return Json(new { success = false, message = "PlanCode is required" });

            var splan = await bal.ShowPlanOJ(planCode);
            return Json(new { success = true, data = splan }, JsonRequestBehavior.AllowGet);
        }

        // Gets all available statuses (Active/Inactive etc.)
        public async Task<ActionResult> GetStatusOJ()
        {
            var statusList = await bal.GetStatusOJ();
            return Json(statusList, JsonRequestBehavior.AllowGet);
        }

        // Gets all available categories for items
        public async Task<ActionResult> GetCatOJ()
        {
            var catList = await bal.GetCategoryOJ();
            return Json(catList, JsonRequestBehavior.AllowGet);
        }

        // Fetches HSN (Harmonized System of Nomenclature) code based on item id
        public async Task<ActionResult> FetchHSNOJ(int id)
        {
            var hsnList = await bal.GetHSNCodeOJ(id);
            return Json(hsnList, JsonRequestBehavior.AllowGet);
        }

        // Gets list of item manufacturers/makes
        public async Task<ActionResult> GetMakeOJ()
        {
            var makeList = await bal.GetItemMakeOJ();
            return Json(makeList, JsonRequestBehavior.AllowGet);
        }

        // Gets list of Units of Measurement (UOM)
        public async Task<ActionResult> GetUOMOJ()
        {
            var uomList = await bal.GetUOMOJ();
            return Json(uomList, JsonRequestBehavior.AllowGet);
        }

        // Gets all available plans
        public async Task<ActionResult> GetPlanOJ()
        {
            var planlist = await bal.GetPlanOJ();
            return Json(planlist, JsonRequestBehavior.AllowGet);
        }

        // Gets list of inspections
        public async Task<ActionResult> InsepctionOJ()
        {
            var inspe = await bal.GetInspectionOJ();
            return Json(inspe, JsonRequestBehavior.AllowGet);
        }

        // Gets list of qualitative parameters
        public async Task<ActionResult> GetQualityOJ()
        {
            var qualityList = await bal.GetQualitativeOJ();
            return Json(qualityList, JsonRequestBehavior.AllowGet);
        }

        // Gets list of quantitative parameters
        public async Task<ActionResult> GetQuanOJ()
        {
            var quanList = await bal.GetQuantitativeOJ();
            return Json(quanList, JsonRequestBehavior.AllowGet);
        }

        // Generates next available ItemCode for new item
        [HttpGet]
        public async Task<ActionResult> GetNextItemCodeOJ()
        {
            string nextCode = await bal.GenerateNextItemCodeOJ();
            return Json(new { itemCode = nextCode }, JsonRequestBehavior.AllowGet);
        }

        // Generates next available PlanCode for new plan
        [HttpGet]
        public async Task<ActionResult> GenerateNextPlanCodeOJ()
        {
            string nexplancode = await bal.GenerateNextPlanCodeOJ();
            return Json(new { plancode = nexplancode }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public async Task<ActionResult> GenerateNextQualityCodeOJ()
        {
            string nextquacode = await bal.GenerateNextQualityCodeOJ();
            return Json(new { QualityCode = nextquacode }, JsonRequestBehavior.AllowGet);
        }

        // Fetches item details by ItemId (used for editing/updating item)
        public async Task<ActionResult> GetitemsidOJ(int id)
        {
            if (id > 0)
            {
                var items = await bal.GetItemOJ();
                var updateitem = items.FirstOrDefault(i => i.ItemIdOJ == id);

                return Json(new
                {
                    success = true,
                    id = updateitem.ItemIdOJ,
                    itemcode = updateitem.ItemCode,
                    name = updateitem.ItemName,
                    category = updateitem.ItemCategoryId,
                    categoryname= updateitem.ItemCategory,
                    status = updateitem.ItemStatusId,
                    Status = updateitem.Status,
                    uom = updateitem.UOMId,
                    descri = updateitem.Description,
                    unitR = updateitem.UnitRates,
                    recQ = updateitem.RecorderQuantity,
                    minQ = updateitem.MinQuantity,
                    itemby = updateitem.ItemMakeId,
                    itemmake =updateitem.ItemMake,
                    exp = updateitem.ExpiryDays,
                    isqua = updateitem.ISQualityBit,
                    hsn = updateitem.HSNCode,
                    date = updateitem.Date,
                }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return View();
            }
        }

        // Fetches inspection plans mapped to a specific ItemCode
        [HttpGet]
        public async Task<ActionResult> GetInspeplanOJ(string itemCode)
        {
            if (!string.IsNullOrEmpty(itemCode))
            {
                var plans = await bal.GetInspecPlanOJ(itemCode);

                if (plans != null && plans.Any())
                {
                    return Json(new
                    {
                        success = true,
                        data = plans.Select(updateitem => new
                        {
                            itemcode = updateitem.ItemCode,
                            itemquality = updateitem.ItemQualityId,
                            qualitycode = updateitem.ItemQualityCode,
                            planid = updateitem.PlanId,
                            planname = updateitem.PlanName,
                            inspectionid = updateitem.InspectionId,
                            inspectionname = updateitem.InspectionName,
                            parameters = updateitem.Parameters,
                            parametersname = updateitem.ParametersName,
                            pquality = updateitem.PQuality,
                            puomid = updateitem.PUOMId,
                            puomname = updateitem.PUOMName,
                            description = updateitem.PlanDescription,
                            plancode = updateitem.PlanCode,
                        }).ToList()
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = false, message = "No inspection plan found for this ItemCode." }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json(new { success = false, message = "Invalid ItemCode" }, JsonRequestBehavior.AllowGet);
            }
        }

        // Adds or updates item details
        public async Task<ActionResult> AddItemOJ(InventoryOJ objn)
        {
            objn.StaffCode = Session["StaffCode"].ToString();

            if (objn.ItemIdOJ > 0)
            {
                // Update existing item
                await bal.UpdateItemOJ(objn);
                return Json(new { status = 1, message = "Item updated successfully." }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                // Insert new item
                int result = await bal.AddItemOJ(objn);

                if (result == 0)
                {
                    return Json(new { status = 0, message = "Item already exists." }, JsonRequestBehavior.AllowGet);
                }
                else if (result == 1)
                {
                    return Json(new { status = 1, message = "Item added successfully." }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { status = -1, message = "Unexpected error occurred." }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        // Adds new plan for an item
        [HttpPost]
        public async Task<ActionResult> AddPlanOJ(InventoryOJ objn)
        {
            try
            {
                await bal.AddPlanOJ(objn);

                return Json(new
                {
                    success = true,
                    planCode = objn.PlanCode,
                    itemCode = objn.ItemCode,
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        // Deletes parameter (quality/inspection parameter) by ID
        [HttpPost]
        public async Task<ActionResult> DelParaOJ(int id)
        {
            try
            {
                await bal.DeleteparaOJ(id);

                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        // Main Category View
        public ActionResult CategorySSG()
        {
            return View();
        }

        // Partial View for list
        public ActionResult CategoryListSSG()
        {
            return PartialView("_CategoryListSSG");
        }

        // Get all categories (for DataTable)
        public async Task<JsonResult> GetAllCategoriesSSG()
        {
            DataSet ds = await bal.GetAllCategoriesSSG();
            List<InventorySSG> categoryList = new List<InventorySSG>();

            if (ds?.Tables.Count > 0)
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    categoryList.Add(new InventorySSG
                    {
                        ItemCategoryId = Convert.ToInt32(row["ItemCategoryId"]),
                        ItemCategoryName = row["ItemCategoryName"].ToString(),
                        Description = row["Description"].ToString(),
                        HSNCode = int.Parse(row["HSNCode"].ToString()),
                        TaxRateId = int.Parse(row["TaxRateId"].ToString())
                    });
                }
            }

            return Json(new { data = categoryList }, JsonRequestBehavior.AllowGet);
        }

        // Partial view for create or edit
        public async Task<ActionResult> CreateCategorySSG(int? id)
        {
            try
            {
                InventorySSG model = new InventorySSG();

                if (id.HasValue && id.Value > 0)
                {
                    // Directly get Inventory object instead of DataSet
                    model = await bal.GetCategorySSG(id.Value);

                    if (model == null)
                    {
                        return Json(new { success = false, message = "Category not found" },
                                    JsonRequestBehavior.AllowGet);
                    }
                }

                return PartialView("_CreateOrEditCategorySSG", model);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Server Error: " + ex.Message },
                            JsonRequestBehavior.AllowGet);
            }
        }

        // Create Category POST
        [HttpPost]
        public async Task<JsonResult> CreateCategoryySSG(InventorySSG objCategory)
        {
            if (objCategory == null || string.IsNullOrEmpty(objCategory.ItemCategoryName))
                return Json(new { success = false, message = "Invalid category data." });

            await bal.InsertCategorySSG(objCategory);
            return Json(new { success = true, message = "Category created successfully." });
        }



        // Update Category POST
        [HttpPost]
        public async Task<JsonResult> UpdateCategorySSG(InventorySSG objCategory)
        {
            if (objCategory == null || objCategory.ItemCategoryId <= 0 || string.IsNullOrEmpty(objCategory.ItemCategoryName))
                return Json(new { success = false, message = "Invalid category data." });

            await bal.UpdateCategorySSG(objCategory);
            return Json(new { success = true, message = "Category updated successfully." });
        }

        // Delete Category
        [HttpPost]
        public async Task<JsonResult> DeleteCategorySSG(int ItemCategoryId)
        {
            if (ItemCategoryId <= 0)
                return Json(new { success = false, message = "Invalid category id." });

            await bal.DeleteCategorySSG(ItemCategoryId);
            return Json(new { success = true, message = "Category deleted successfully." });
        }

        public async Task<ActionResult> ALLTaxRatesSSG()
        {
            var data = await bal.ALLTaxRatesSSG();
            return Json(data, JsonRequestBehavior.AllowGet);
        }


       


        #endregion Om and Sayali


    }
}

