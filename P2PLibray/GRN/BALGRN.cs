using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using P2PHelper;

namespace P2PLibray.GRN
{

   
    public class BALGRN
    {

        MSSQL obj = new MSSQL();

        #region Rutik
        /// <summary>
        /// Gets the list of rejected goods.
        /// </summary>
        /// <returns>SqlDataReader containing rejected goods data.</returns>
        public async Task<SqlDataReader> GetRejectedGoods()
        {
            Dictionary<string, string> param = new Dictionary<string, string>
            {
                { "@Flag", "GSTtblGoodsReturnHSB" }
            };
            return await obj.ExecuteStoredProcedureReturnDataReader("GRNProcedure", param);
        }

        /// <summary>
        /// Gets the list of returned goods.
        /// </summary>
        /// <returns>SqlDataReader containing returned goods data.</returns>
        public async Task<SqlDataReader> GetReturnGoods()
        {
            Dictionary<string, string> param = new Dictionary<string, string>
            {
                { "@Flag", "ReturnListHSB" }
            };
            return await obj.ExecuteStoredProcedureReturnDataReader("GRNProcedure", param);
        }

        /// <summary>
        /// Gets GRN details along with its items for in-stock goods.
        /// Also generates a new Goods Return Code.
        /// </summary>
        /// <param name="code">The GRN code.</param>
        /// <returns>GoodsReturnViewModel containing header, items, and totals.</returns>
        public async Task<GoodsReturnViewModel> GSTInstockGRNHSB(string code)
        {
            GoodsReturnViewModel model = new GoodsReturnViewModel();

            // -------- First query: Fetch header --------
            Dictionary<string, string> headerParam = new Dictionary<string, string>
            {
                { "@Flag", "GSTInstockGRNHSB" },
                { "@GRNCode", code }
            };

            SqlDataReader drHeader = await obj.ExecuteStoredProcedureReturnDataReader("GRNProcedure", headerParam);
            if (drHeader.HasRows && await drHeader.ReadAsync())
            {
                model.GRNCode = drHeader["GRNCode"].ToString();
                model.POCode = drHeader["POCode"].ToString();
                model.InvoiceNo = drHeader["InvoiceNo"].ToString();
                model.InvoiceDate = Convert.ToDateTime(drHeader["InvoiceDate"]);
                model.VendorName = drHeader["VenderName"].ToString();
            }
            drHeader.Close();

            // -------- Second query: Fetch items --------
            Dictionary<string, string> itemParam = new Dictionary<string, string>
            {
                { "@Flag", "GSTGRNItemsHSB" },
                { "@GRNCode", code }
            };

            SqlDataReader drItems = await obj.ExecuteStoredProcedureReturnDataReader("GRNProcedure", itemParam);
            while (await drItems.ReadAsync())
            {
                int qty = Convert.ToInt32(drItems["Quantity"]);
                decimal unitRate = Convert.ToDecimal(drItems["CostPerUnit"]);

                GoodsReturnItemViewModel item = new GoodsReturnItemViewModel
                {
                    ItemName = drItems["ItemName"].ToString(),
                    ItemDescription = drItems["Description"].ToString(),
                    Qty = qty,
                    UOM = drItems["UOMName"].ToString(),
                    Reason = drItems["Reason"].ToString(),
                    FailedQCCode = drItems["FailedQCCode"].ToString(),
                    SampleTestFailed = Convert.ToInt32(drItems["SampleTestFailed"]),
                    InspectionFrequency = Convert.ToInt32(drItems["InspectionFrequency"]),
                    SampleQualityChecked = Convert.ToInt32(drItems["SampleQualityChecked"]),
                    UnitRate = unitRate,
                    Amount = qty * unitRate
                };

                model.Items.Add(item);
            }
            drItems.Close();

            // -------- Third query: Generate Goods Return Code --------
            Dictionary<string, string> CodeParam = new Dictionary<string, string>
            {
                { "@Flag", "GenerateGoodsReturnCodeHSB" }
            };

            SqlDataReader drCode = await obj.ExecuteStoredProcedureReturnDataReader("GRNProcedure", CodeParam);
            if (drCode.HasRows && await drCode.ReadAsync())
            {
                model.GRNo = drCode["GeneratedGoodsReturnCode"].ToString();
            }
            drCode.Close();

            // -------- Calculate total --------
            model.TotalAmount = model.Items.Sum(i => i.Amount);

            return model;
        }

        /// <summary>
        /// Gets the details required for printing a Goods Return note.
        /// </summary>
        /// <param name="grCode">The Goods Return code.</param>
        /// <returns>PrintDetailsClass containing header, items, and totals.</returns>
        public async Task<PrintDetailsClass> PrintDetailsHSB(string grCode)
        {
            PrintDetailsClass model = new PrintDetailsClass();

            // -------- First query: Fetch header --------
            Dictionary<string, string> headerParam = new Dictionary<string, string>
            {
                { "@Flag", "PrintOrderDetailHSB" },
                { "@GRCode", grCode }
            };

            SqlDataReader drHeader = await obj.ExecuteStoredProcedureReturnDataReader("GRNProcedure", headerParam);
            if (drHeader.HasRows && await drHeader.ReadAsync())
            {
                model.GoodReturnCode = drHeader["GoodReturnCode"].ToString();
                model.POCode = drHeader["POCode"].ToString();
                model.InvoiceNo = drHeader["InvoiceNo"].ToString();
            }
            drHeader.Close();

            // -------- Second query: Fetch item details --------
            Dictionary<string, string> itemParam = new Dictionary<string, string>
            {
                { "@Flag", "PrintOrderItemsHSB" },
                { "@GRCode", grCode }
            };

            SqlDataReader drItems = await obj.ExecuteStoredProcedureReturnDataReader("GRNProcedure", itemParam);
            while (await drItems.ReadAsync())
            {
                int qty = Convert.ToInt32(drItems["Quantity"]);
                decimal unitRate = Convert.ToDecimal(drItems["CostPerUnit"]);

                GoodsReturnItemViewModel item = new GoodsReturnItemViewModel
                {
                    ItemName = drItems["ItemName"].ToString(),
                    ItemDescription = drItems["Description"].ToString(),
                    Qty = qty,
                    UOM = drItems["UOMName"].ToString(),
                    UnitRate = unitRate,
                    Amount = qty * unitRate
                };

                model.Items.Add(item);
            }
            drItems.Close();

            // -------- Calculate total --------
            model.TotalAmount = model.Items.Sum(i => i.Amount);

            return model;
        }

        /// <summary>
        /// Saves a Goods Return and its items.
        /// </summary>
        /// <param name="model">The GoodsReturnViewModel containing header and items.</param>
        public async Task SaveGRHSB(GoodsReturnViewModel model)
        {
            // -------- Save GR Code --------
            Dictionary<string, string> SaveGR = new Dictionary<string, string>
            {
                { "@Flag", "InsertGoodsReturnHSB" },
                { "@GRCode", model.GRNo },
                { "@GRNCode", model.GRNCode },
                { "@addDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
                { "@AddedBy", model.AddedBy },
                { "@ReasonForGoodReturn", model.ReasonForGoodReturn }
            };

            await obj.ExecuteStoredProcedure("GRNProcedure", SaveGR);

            // -------- Save GR Items --------
            if (model.Items != null && model.Items.Any())
            {
                foreach (var item in model.Items)
                {
                    Dictionary<string, string> SaveItem = new Dictionary<string, string>
                    {
                        { "@Flag", "InsertGoodsReturnItemHSB" },
                        { "@GRCode", model.GRNo },
                        { "@FailedQCCode", item.FailedQCCode }
                    };
                    await obj.ExecuteStoredProcedure("GRNProcedure", SaveItem);
                }
            }
        }

        /// <summary>
        /// Gets the dispatch details of goods return.
        /// </summary>
        /// <param name="code">The Goods Return code.</param>
        /// <returns>GoodDispatchModel containing header, items, and totals.</returns>
        public async Task<GoodDispatchModel> DispatchDetailsHSB(string code)
        {
            GoodDispatchModel model = new GoodDispatchModel();

            // -------- First query: Fetch header --------
            Dictionary<string, string> headerParam = new Dictionary<string, string>
            {
                { "@Flag", "DispatchHeaderHSB" },
                { "@GRCode", code }
            };

            SqlDataReader drHeader = await obj.ExecuteStoredProcedureReturnDataReader("GRNProcedure", headerParam);
            if (drHeader.HasRows && await drHeader.ReadAsync())
            {
                model.GRNCode = drHeader["GRNCode"].ToString();
                model.POCode = drHeader["POCode"].ToString();
                model.InvoiceNo = drHeader["InvoiceNo"].ToString();
                model.VendorName = drHeader["VenderName"].ToString();
                model.GoodReturnCode = drHeader["GoodReturnCode"].ToString();
            }
            drHeader.Close();

            // -------- Second query: Fetch items --------
            Dictionary<string, string> itemParam = new Dictionary<string, string>
            {
                { "@Flag", "GRItemsHSB" },
                { "@GRCode", code }
            };

            SqlDataReader drItems = await obj.ExecuteStoredProcedureReturnDataReader("GRNProcedure", itemParam);
            while (await drItems.ReadAsync())
            {
                int qty = Convert.ToInt32(drItems["Quantity"]);
                decimal unitRate = Convert.ToDecimal(drItems["CostPerUnit"]);

                GoodsReturnItemViewModel item = new GoodsReturnItemViewModel
                {
                    ItemName = drItems["ItemName"].ToString(),
                    ItemDescription = drItems["Description"].ToString(),
                    Qty = qty,
                    UOM = drItems["UOMName"].ToString(),
                    FailedQCCode = drItems["FailedQCCode"].ToString(),
                    UnitRate = unitRate,
                    Amount = qty * unitRate
                };

                model.Items.Add(item);
            }
            drItems.Close();

            // -------- Calculate total --------
            model.TotalAmount = model.Items.Sum(i => i.Amount);

            return model;
        }

        /// <summary>
        /// Saves dispatch details for a goods return.
        /// </summary>
        /// <param name="model">GoodDispatchModel containing dispatch details.</param>
        public async Task SaveDispachHSB(GoodDispatchModel model)
        {
            Dictionary<string, string> SaveGR = new Dictionary<string, string>
            {
                { "@Flag", "SaveDispatchHSB" },
                { "@TransporterName", model.TransportName },
                { "@TransportContactNo", model.TransportContact },
                { "@VehicleTypeId", model.VehicleType },
                { "@VehicleNo", model.VehicleNo },
                { "@ReasonForGoodReturn", model.ReasonForGoodReturn },
                { "@GRCode", model.GoodReturnCode }
            };

            await obj.ExecuteStoredProcedure("GRNProcedure", SaveGR);
        }

        /// <summary>
        /// Gets the list of vehicle types.
        /// </summary>
        /// <returns>List of vehicleTypeModel.</returns>
        public async Task<List<vehicleTypeModel>> GetVehicleTypesHSB()
        {
            Dictionary<string, string> param = new Dictionary<string, string>
            {
                { "@Flag", "VehicleTypeHSB" }
            };
            List<vehicleTypeModel> typeList = new List<vehicleTypeModel>();

            DataSet ds = await obj.ExecuteStoredProcedureReturnDS("GRNProcedure", param);
            if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    typeList.Add(new vehicleTypeModel
                    {
                        SubTypeId = Convert.ToInt32(dr["SubTypeId"]?.ToString()),
                        SubTypeName = dr["SubTypeName"]?.ToString(),
                    });
                }
            }
            return typeList;
        }

        /// <summary>
        /// Gets company address based on staff code.
        /// </summary>
        /// <param name="staffCode">The staff code.</param>
        /// <returns>Company address as string.</returns>
        public async Task<string> GetCompanyAddressHSB(string staffCode)
        {
            string staff = "";

            Dictionary<string, string> headerParam = new Dictionary<string, string>
            {
                { "@Flag", "CompanyAddressHSB" },
                { "@StaffCode", staffCode }
            };

            SqlDataReader drHeader = await obj.ExecuteStoredProcedureReturnDataReader("GRNProcedure", headerParam);
            if (drHeader.HasRows && await drHeader.ReadAsync())
            {
                staff = drHeader["Address"].ToString();
            }
            drHeader.Close();

            return staff;
        }
        #endregion

        #region Pravin
        /// <summary>
        /// Retrieves the GRN summary data using the stored procedure "GRNProcedure" with flag = GRNSummaryPSM.
        /// </summary>
        /// <param name="objs">GRNPSM object containing parameters if needed.</param>
        /// <returns>A DataTable containing GRN summary data.</returns>
        public async Task<DataTable> GRNSummaryPSM()
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("@Flag", "GRNSummaryPSM");
            DataSet ds = await obj.ExecuteStoredProcedureReturnDS("GRNProcedure", param);
            return ds.Tables[0];
        }

        /// <summary>
        /// Retrieves the GRN items for a specific GRN code.
        /// </summary>
        /// <param name="GRNCode">The GRN code to filter items.</param>
        /// <returns>A DataTable containing items linked to the given GRN.</returns>
        public async Task<DataTable> GRNItemsPSM(string GRNCode)
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("@Flag", "GRNItemsPSM");
            param.Add("@GRNCode", GRNCode);
            DataSet ds = await obj.ExecuteStoredProcedureReturnDS("GRNProcedure", param);
            return ds.Tables[0];
        }

        /// <summary>
        /// Retrieves the GRN summary list as a SqlDataReader.
        /// </summary>
        /// <returns>A SqlDataReader containing GRN summary list data.</returns>
        public async Task<SqlDataReader> GRNSummaryListPSM()
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("@Flag", "GRNSummaryListPSM");
            SqlDataReader dr = await obj.ExecuteStoredProcedureReturnDataReader("GRNProcedure", param);
            return dr;
        }

        /// <summary>
        /// Retrieves quality check summary for GRN.
        /// </summary>
        /// <returns>A DataTable containing GRN quality check data.</returns>
        public async Task<DataTable> GRNQualityCheckPSM()
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("@Flag", "GRNQualityCheckPSM");
            DataSet ds = await obj.ExecuteStoredProcedureReturnDS("GRNProcedure", param);
            return ds.Tables[0];
        }

        /// <summary>
        /// Retrieves GRN assign quality check data.
        /// </summary>
        /// <returns>A DataTable containing assigned QC data for GRN.</returns>
        public async Task<DataTable> GRNAssignQCPSM()
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("@Flag", "GRNAssignQCPSM");
            DataSet ds = await obj.ExecuteStoredProcedureReturnDS("GRNProcedure", param);
            return ds.Tables[0];
        }

        /// <summary>
        /// Retrieves GRN assigned quality check list data.
        /// </summary>
        /// <returns>A DataTable containing list of GRN assigned QC records.</returns>
        public async Task<DataTable> GRNAssignQCListPSM()
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("@Flag", "GRNAssignQCListPSM");
            DataSet ds = await obj.ExecuteStoredProcedureReturnDS("GRNProcedure", param);
            return ds.Tables[0];
        }

        /// <summary>
        /// Retrieves summary count for Goods Return.
        /// </summary>
        /// <returns>A DataTable containing goods return summary count.</returns>
        public async Task<DataTable> GoodsReturnSummaryCountPSM()
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("@Flag", "GoodsReturnSummaryCountPSM");
            DataSet ds = await obj.ExecuteStoredProcedureReturnDS("GRNProcedure", param);
            return ds.Tables[0];
        }

        /// <summary>
        /// Retrieves goods return summary details.
        /// </summary>
        /// <returns>A DataTable containing goods return summary data.</returns>
        public async Task<DataTable> GoodsReturnPieChartPSM()
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("@Flag", "GoodsReturnPieChartPSM");
            DataSet ds = await obj.ExecuteStoredProcedureReturnDS("GRNProcedure", param);
            return ds.Tables[0];
        }

        /// <summary>
        /// Retrieves goods return summary list as a SqlDataReader.
        /// </summary>
        /// <returns>A SqlDataReader containing goods return summary list data.</returns>
        public async Task<SqlDataReader> GoodsReturnSummaryListPSM()
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("@Flag", "GoodsReturnSummaryListPSM");
            SqlDataReader dr = await obj.ExecuteStoredProcedureReturnDataReader("GRNProcedure", param);
            return dr;
        }

        /// <summary>
        /// Retrieves the approved PO total count chart data.
        /// </summary>
        /// <returns>A DataTable containing approved PO total count chart data.</returns>
        public async Task<DataTable> ApprovedPOTotalCountChartPSM()
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("@Flag", "ApprovedPOTotalCountChartPSM");
            DataSet ds = await obj.ExecuteStoredProcedureReturnDS("GRNProcedure", param);
            return ds.Tables[0];
        }

        /// <summary>
        /// Retrieves the list of approved POs.
        /// </summary>
        /// <returns>A DataTable containing approved PO list data.</returns>
        public async Task<DataTable> ApprovedPOListPSM()
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("@Flag", "ApprovedPOListPSM");
            DataSet ds = await obj.ExecuteStoredProcedureReturnDS("GRNProcedure", param);
            return ds.Tables[0];
        }

        /// <summary>
        /// Retrieves approved PO items for a given PO code.
        /// </summary>
        /// <param name="Pocode">The PO code to filter items.</param>
        /// <returns>A DataTable containing approved PO items.</returns>
        public async Task<DataTable> ApprovedPOItemsPSM(string Pocode)
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("@Flag", "ApprovedPOItemsPSM");
            param.Add("@Pocode", Pocode);
            DataSet ds = await obj.ExecuteStoredProcedureReturnDS("GRNProcedure", param);
            return ds.Tables[0];
        }

        #endregion


        #region Rushikesh
        /// <summary>
        /// Returns total number of GRNs created in the given date range.
        /// Calls GRNProcedure with Flag = 'TotalGRNRHK'.
        /// </summary>
        public async Task<DataTable> TotalGRNRHK(DateTime? startDate, DateTime? endDate)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("@Flag", "TotalGRNRHK");
            parameters.Add("@StartDate", startDate.HasValue ? startDate.Value.ToString("yyyy-MM-dd") : null);
            parameters.Add("@EndDate", endDate.HasValue ? endDate.Value.ToString("yyyy-MM-dd") : null);

            DataTable dt = new DataTable();
            using (SqlDataReader dr = await obj.ExecuteStoredProcedureReturnDataReader("GRNProcedure", parameters))
            {
                dt.Load(dr);
            }
            return dt;
        }

        /// <summary>
        /// Returns total number of items recorded in GRNs in the given date range.
        /// Calls GRNProcedure with Flag = 'TotalGRNItemsRHK'.
        /// </summary>
        public async Task<DataTable> TotalGRNItemRHK(DateTime? startDate, DateTime? endDate)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("@Flag", "TotalGRNItemsRHK");
            parameters.Add("@StartDate", startDate.HasValue ? startDate.Value.ToString("yyyy-MM-dd") : null);
            parameters.Add("@EndDate", endDate.HasValue ? endDate.Value.ToString("yyyy-MM-dd") : null);

            DataTable dt = new DataTable();
            using (SqlDataReader dr = await obj.ExecuteStoredProcedureReturnDataReader("GRNProcedure", parameters))
            {
                dt.Load(dr);
            }
            return dt;
        }

        /// <summary>
        /// Returns detailed list of GRN items (GRNCode, ItemName, Quantity) in date range.
        /// Calls GRNProcedure with Flag = 'TotalGRNItemsListRHK'.
        /// </summary>
        public async Task<DataTable> TotalGRNItemListRHK(DateTime? startDate, DateTime? endDate)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("@Flag", "TotalGRNItemsListRHK");
            parameters.Add("@StartDate", startDate.HasValue ? startDate.Value.ToString("yyyy-MM-dd") : null);
            parameters.Add("@EndDate", endDate.HasValue ? endDate.Value.ToString("yyyy-MM-dd") : null);

            DataTable dt = new DataTable();
            using (SqlDataReader dr = await obj.ExecuteStoredProcedureReturnDataReader("GRNProcedure", parameters))
            {
                dt.Load(dr);
            }
            return dt;
        }

        /// <summary>
        /// Returns count of approved QC items (StatusId = 14) in date range.
        /// Calls GRNProcedure with Flag = 'ApproveCountRHK'.
        /// </summary>
        public async Task<DataTable> ApproveCountRHK(DateTime? startDate, DateTime? endDate)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("@Flag", "ApproveCountRHK");
            parameters.Add("@StartDate", startDate.HasValue ? startDate.Value.ToString("yyyy-MM-dd") : null);
            parameters.Add("@EndDate", endDate.HasValue ? endDate.Value.ToString("yyyy-MM-dd") : null);

            DataTable dt = new DataTable();
            using (SqlDataReader dr = await obj.ExecuteStoredProcedureReturnDataReader("GRNProcedure", parameters))
            {
                dt.Load(dr);
            }
            return dt;
        }


        /// <summary>
        /// Returns count of approved QC items (StatusId = 14) in date range.
        /// Calls GRNProcedure with Flag = 'ApproveCountRHK'.
        /// </summary>
        public async Task<DataTable> PendingCountRHK(DateTime? startDate, DateTime? endDate)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("@Flag", "PendingCountRHK");
            parameters.Add("@StartDate", startDate.HasValue ? startDate.Value.ToString("yyyy-MM-dd") : null);
            parameters.Add("@EndDate", endDate.HasValue ? endDate.Value.ToString("yyyy-MM-dd") : null);

            DataTable dt = new DataTable();
            using (SqlDataReader dr = await obj.ExecuteStoredProcedureReturnDataReader("GRNProcedure", parameters))
            {
                dt.Load(dr);
            }
            return dt;
        }
        /// <summary>
        /// Returns count of items assigned for QC in date range.
        /// Calls GRNProcedure with Flag = 'QCAssignedCountRHK'.
        /// </summary>
        public async Task<DataTable> QCAssignedCountRHK(DateTime? startDate, DateTime? endDate)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("@Flag", "QCAssignedCountRHK");
            parameters.Add("@StartDate", startDate.HasValue ? startDate.Value.ToString("yyyy-MM-dd") : null);
            parameters.Add("@EndDate", endDate.HasValue ? endDate.Value.ToString("yyyy-MM-dd") : null);

            DataTable dt = new DataTable();
            using (SqlDataReader dr = await obj.ExecuteStoredProcedureReturnDataReader("GRNProcedure", parameters))
            {
                dt.Load(dr);
            }
            return dt;
        }

        /// <summary>
        /// Returns count of upcoming items in open POs (not yet GRN’ed).
        /// Calls GRNProcedure with Flag = 'UpcomingItemCountRHK'.
        /// </summary>
        public async Task<DataTable> UpcomingItemCountRHK(DateTime? startDate, DateTime? endDate)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("@Flag", "UpcomingItemCountRHK");
            parameters.Add("@StartDate", startDate.HasValue ? startDate.Value.ToString("yyyy-MM-dd") : null);
            parameters.Add("@EndDate", endDate.HasValue ? endDate.Value.ToString("yyyy-MM-dd") : null);

            DataTable dt = new DataTable();
            using (SqlDataReader dr = await obj.ExecuteStoredProcedureReturnDataReader("GRNProcedure", parameters))
            {
                dt.Load(dr);
            }
            return dt;
        }

        /// <summary>
        /// Returns detailed list of upcoming items (in POs not yet GRN’ed).
        /// Calls GRNProcedure with Flag = 'UpcomingItemListRHK'.
        /// </summary>
        public async Task<DataTable> UpcomingItemListRHK(DateTime? startDate, DateTime? endDate)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("@Flag", "UpcomingItemListRHK");
            parameters.Add("@StartDate", startDate.HasValue ? startDate.Value.ToString("yyyy-MM-dd") : null);
            parameters.Add("@EndDate", endDate.HasValue ? endDate.Value.ToString("yyyy-MM-dd") : null);

            DataTable dt = new DataTable();
            using (SqlDataReader dr = await obj.ExecuteStoredProcedureReturnDataReader("GRNProcedure", parameters))
            {
                dt.Load(dr);
            }
            return dt;
        }

        /// <summary>
        /// Returns detailed list of items assigned for QC.
        /// Calls GRNProcedure with Flag = 'QCAssignedItemsRHK'.
        /// </summary>
        public async Task<DataTable> QCAssignedItemsRHK(DateTime? startDate, DateTime? endDate)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("@Flag", "QCAssignedItemsRHK");
            parameters.Add("@StartDate", startDate.HasValue ? startDate.Value.ToString("yyyy-MM-dd") : null);
            parameters.Add("@EndDate", endDate.HasValue ? endDate.Value.ToString("yyyy-MM-dd") : null);
            DataTable dt = new DataTable();
            using (SqlDataReader dr = await obj.ExecuteStoredProcedureReturnDataReader("GRNProcedure", parameters))
            {
                dt.Load(dr);
            }
            return dt;
        }

        /// <summary>
        /// Returns count of rejected GRNs (from Goods Return table).
        /// Calls GRNProcedure with Flag = 'RejectedGRNCountRHK'.
        /// </summary>
        public async Task<DataTable> RejectedGRNCountRHK(DateTime? startDate, DateTime? endDate)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("@Flag", "RejectedGRNCountRHK");
            parameters.Add("@StartDate", startDate.HasValue ? startDate.Value.ToString("yyyy-MM-dd") : null);
            parameters.Add("@EndDate", endDate.HasValue ? endDate.Value.ToString("yyyy-MM-dd") : null);

            DataTable dt = new DataTable();
            using (SqlDataReader dr = await obj.ExecuteStoredProcedureReturnDataReader("GRNProcedure", parameters))
            {
                dt.Load(dr);
            }
            return dt;
        }

        /// <summary>
        /// Returns daily GRN trends (date-wise GRN counts).
        /// Calls GRNProcedure with Flag = 'GRNTrendsRHK'.
        /// </summary>
        public async Task<DataSet> GRNTrendsRHK(DateTime? startDate, DateTime? endDate)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("@Flag", "GRNTrendsRHK");
            parameters.Add("@StartDate", startDate.HasValue ? startDate.Value.ToString("yyyy-MM-dd") : null);
            parameters.Add("@EndDate", endDate.HasValue ? endDate.Value.ToString("yyyy-MM-dd") : null);
            DataSet ds = await obj.ExecuteStoredProcedureReturnDS("GRNProcedure", parameters);
            return ds;
        }

        /// <summary>
        /// Returns all GRNs in the given date range with vendor and staff details.
        /// Calls GRNProcedure with Flag = 'GRNListRHK'.
        /// </summary>
        public async Task<DataSet> GRNListRHK(DateTime? startDate, DateTime? endDate)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("@Flag", "GRNListRHK");
            parameters.Add("@StartDate", startDate.HasValue ? startDate.Value.ToString("yyyy-MM-dd") : null);
            parameters.Add("@EndDate", endDate.HasValue ? endDate.Value.ToString("yyyy-MM-dd") : null);
            DataSet ds = await obj.ExecuteStoredProcedureReturnDS("GRNProcedure", parameters);
            return ds;
        }

        /// <summary>
        /// Returns 10 most recent GRNs with vendor, status, and employee details.
        /// Calls GRNProcedure with Flag = 'RecentGRNListRHK'.
        /// </summary>
        public async Task<DataSet> RecentGRNListRHK()
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("@Flag", "RecentGRNListRHK");

            DataSet ds = await obj.ExecuteStoredProcedureReturnDS("GRNProcedure", parameters);
            return ds;
        }

        /// <summary>
        /// Returns details of approved QC items (approved list).
        /// Calls GRNProcedure with Flag = 'ApproveItemsRHK'.
        /// </summary>
        public async Task<DataTable> ApprovedItemsRHK(DateTime? startDate, DateTime? endDate)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("@Flag", "ApproveItemsRHK");
            parameters.Add("@StartDate", startDate.HasValue ? startDate.Value.ToString("yyyy-MM-dd") : null);
            parameters.Add("@EndDate", endDate.HasValue ? endDate.Value.ToString("yyyy-MM-dd") : null);
            DataTable dt = new DataTable();
            using (SqlDataReader dr = await obj.ExecuteStoredProcedureReturnDataReader("GRNProcedure", parameters))
            {
                dt.Load(dr);
            }
            return dt;
        }

        /// <summary>
        /// Returns details of rejected QC items (StatusId = 15).
        /// Calls GRNProcedure with Flag = 'RejectItemsRHK'.
        /// </summary>
        public async Task<DataTable> RejectedItemsRHK(DateTime? startDate, DateTime? endDate)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("@Flag", "RejectItemsRHK");
            parameters.Add("@StartDate", startDate.HasValue ? startDate.Value.ToString("yyyy-MM-dd") : null);
            parameters.Add("@EndDate", endDate.HasValue ? endDate.Value.ToString("yyyy-MM-dd") : null);
            DataTable dt = new DataTable();
            using (SqlDataReader dr = await obj.ExecuteStoredProcedureReturnDataReader("GRNProcedure", parameters))
            {
                dt.Load(dr);
            }
            return dt;
        }

        /// <summary>
        /// Returns pending QC items (same as QCAssignedItems).
        /// Calls GRNProcedure with Flag = 'QCAssignedItemsRHK'.
        /// </summary>
        public async Task<DataTable> PendingItemsRHK(DateTime? startDate, DateTime? endDate)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("@Flag", "PendingItemsRHK");
            parameters.Add("@StartDate", startDate.HasValue ? startDate.Value.ToString("yyyy-MM-dd") : null);
            parameters.Add("@EndDate", endDate.HasValue ? endDate.Value.ToString("yyyy-MM-dd") : null);
            DataTable dt = new DataTable();
            using (SqlDataReader dr = await obj.ExecuteStoredProcedureReturnDataReader("GRNProcedure", parameters))
            {
                dt.Load(dr);
            }
            return dt;
        }

        #endregion Rushikesh

        #region sayali
        /// <summary>
        /// Retrieves the list of all GRNs.
        /// </summary>
        /// <returns>DataSet containing all GRNs.</returns>
        public async Task<DataSet> ShowGRNListSSG()
        {
            try
            {
                Dictionary<string, string> param = new Dictionary<string, string>();
                param.Add("@Flag", "ShowGRNListSSG");
                return await obj.ExecuteStoredProcedureReturnDS("GRNProcedure", param);
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching GRN list: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Retrieves the list of all approved POs.
        /// </summary>
        /// <returns>DataSet containing all approved POs.</returns>
        public async Task<DataSet> ShowApprovedPOSSG()
        {
            try
            {
                Dictionary<string, string> param = new Dictionary<string, string>();
                param.Add("@Flag", "ShowApprovedPOSSG");
                return await obj.ExecuteStoredProcedureReturnDS("GRNProcedure", param);
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching approved PO: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Retrieves all warehouses.
        /// </summary>
        /// <returns>DataSet containing warehouse information.</returns>
        public async Task<DataSet> GetWarehouseSSG()
        {
            try
            {
                Dictionary<string, string> param = new Dictionary<string, string>();
                param.Add("@Flag", "GetWarehouseSSG");
                return await obj.ExecuteStoredProcedureReturnDS("GRNProcedure", param);
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching warehouse list: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Retrieves all items associated with a specific PO.
        /// </summary>
        /// <param name="objGRN">GRN object containing PO code.</param>
        /// <returns>DataSet containing PO items.</returns>
        public async Task<DataSet> GetPOItemsSSG(GRN objGRN)
        {
            try
            {
                Dictionary<string, string> param = new Dictionary<string, string>();
                param.Add("@Flag", "GetPOItemsSSG");
                param.Add("@POCode", objGRN.POCode);
                return await obj.ExecuteStoredProcedureReturnDS("GRNProcedure", param);
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching PO items: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Retrieves detailed information of a specific PO.
        /// </summary>
        /// <param name="objGRN">GRN object containing PO code.</param>
        /// <returns>DataSet containing PO info.</returns>
        public async Task<DataSet> POInfoSSG(GRN objGRN)
        {
            try
            {
                Dictionary<string, string> param = new Dictionary<string, string>();
                param.Add("@Flag", "GetPOInfoSSG");
                param.Add("@POCode", objGRN.POCode);
                return await obj.ExecuteStoredProcedureReturnDS("GRNProcedure", param);
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching PO info: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Saves GRN header information.
        /// </summary>
        /// <param name="objGRN">GRN object containing header details.</param>
        /// <param name="staffcode">Staff code who is adding the GRN.</param>
        public async Task SaveGRNHeaderSSG(GRN objGRN, string staffcode)
        {
            try
            {
                Dictionary<string, string> param = new Dictionary<string, string>();
                param.Add("@Flag", "SaveGRNHeaderSSG");
                param.Add("@GRNCode", objGRN.GRNCode);
                param.Add("@POCode", objGRN.POCode);
                param.Add("@InvoiceNo", objGRN.InvoiceNo);
                param.Add("@InvoiceDate", objGRN.InvoiceDate);
                param.Add("@AddedBy", staffcode);
                param.Add("@AddedDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                await obj.ExecuteStoredProcedure("GRNProcedure", param);
            }
            catch (Exception ex)
            {
                throw new Exception("Error saving GRN header: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Saves GRN item details.
        /// </summary>
        /// <param name="objItem">GRN object containing item details.</param>
        public async Task SaveGRNItemSSG(GRN objItem)
        {
            try
            {
                Dictionary<string, string> param = new Dictionary<string, string>();
                param.Add("@Flag", "SaveGRNItemSSG");
                param.Add("@GRNCode", objItem.GRNCode);
                param.Add("@ItemCode", objItem.ItemCode);
                param.Add("@Quantity", objItem.Quantity.ToString());
                param.Add("@WareHouseId", objItem.WareHouseId.ToString());
                await obj.ExecuteStoredProcedure("GRNProcedure", param);
            }
            catch (Exception ex)
            {
                throw new Exception("Error saving GRN item: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Fetches GRN header for viewing or updating.
        /// </summary>
        /// <param name="objGRN">GRN object containing GRN code.</param>
        /// <returns>DataSet containing GRN header details.</returns>
        public async Task<DataSet> ViewGRNSSG(GRN objGRN)
        {
            try
            {
                Dictionary<string, string> param = new Dictionary<string, string>();
                param.Add("@Flag", "viewGRNSSG");
                param.Add("@GRNCode", objGRN.GRNCode);
                return await obj.ExecuteStoredProcedureReturnDS("GRNProcedure", param);
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching GRN header for update: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Fetches GRN items for viewing or updating.
        /// </summary>
        /// <param name="objGRN">GRN object containing GRN code.</param>
        /// <returns>DataSet containing GRN item details.</returns>
        public async Task<DataSet> ViewGRNItemSSG(GRN objGRN)
        {
            try
            {
                Dictionary<string, string> param = new Dictionary<string, string>();
                param.Add("@Flag", "ViewGRNItemSSG");
                param.Add("@GRNCode", objGRN.GRNCode);
                return await obj.ExecuteStoredProcedureReturnDS("GRNProcedure", param);
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching GRN items for update: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Fetches QC items for a specific GRN.
        /// </summary>
        /// <param name="objGRN">GRN object containing GRN code.</param>
        /// <returns>DataSet containing QC items.</returns>
        public async Task<DataSet> FetchQCItemsSSG(GRN objGRN)
        {
            try
            {
                Dictionary<string, string> param = new Dictionary<string, string>();
                param.Add("@Flag", "FetchQCItemsSSG");
                param.Add("@GRNCode", objGRN.GRNCode);
                return await obj.ExecuteStoredProcedureReturnDS("GRNProcedure", param);
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching QC items: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Assigns QC for a GRN.
        /// </summary>
        /// <param name="objGRN">GRN object containing GRN code and added by details.</param>
        /// <returns>Number of inserted QC records.</returns>
        public async Task<int> AssignQCSSG(GRN objGRN, string staffcode)
        {
            try
            {
                Dictionary<string, string> param = new Dictionary<string, string>();
                param.Add("@Flag", "AssignQCSSG");
                param.Add("@GRNCode", objGRN.GRNCode);
                param.Add("@AddedDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                param.Add("@AddedBy", staffcode);


                DataSet ds = await obj.ExecuteStoredProcedureReturnDS("GRNProcedure", param);
                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                    return Convert.ToInt32(ds.Tables[0].Rows[0]["InsertedCount"]);
                else
                    return 0;
            }
            catch (Exception ex)
            {
                throw new Exception("Error assigning QC: " + ex.Message, ex);
            }
        }


        /// <summary>
        /// Updates PO Item Status after GRN creation
        /// </summary>
        public async Task UpdatePOItemStatusSSG(string poCode)
        {
            try
            {
                Dictionary<string, string> param = new Dictionary<string, string>();
                param.Add("@Flag", "UpdatePOItemStatusSSG");
                param.Add("@POCode", poCode);

                await obj.ExecuteStoredProcedure("GRNProcedure", param);
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating PO item status: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Updates overall Purchase Order Status based on item statuses
        /// </summary>
        public async Task UpdatePOStatusSSG(string poCode)
        {
            try
            {
                Dictionary<string, string> param = new Dictionary<string, string>();
                param.Add("@Flag", "UpdatePOStatusSSG");
                param.Add("@POCode", poCode);

                await obj.ExecuteStoredProcedure("GRNProcedure", param);
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating PO status: " + ex.Message, ex);
            }
        }

        #endregion sayali


    }
}
