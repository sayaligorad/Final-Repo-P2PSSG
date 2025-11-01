using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using P2PHelper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using static P2PLibray.Purchase.Purchase;

namespace P2PLibray.Purchase
{
    public class BALPurchase
    {
        MSSQL obj = new MSSQL();

        #region Pravin
        // Generate PR Code
        /// <summary>
        /// Generates a new Purchase Requisition (PR) code by calling the stored procedure "PurchaseProcedure"
        /// with flag "GeneratePRCodePSM". Returns the generated PR code as a string.
        /// </summary>
        public async Task<string> GeneratePRCodePSM()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("@flag", "GeneratePRCodePSM");

            using (SqlDataReader dr = await obj.ExecuteStoredProcedureReturnDataReader("PurchaseProcedure", data))
            {
                if (dr.Read())
                {
                    return dr["PRCode"].ToString();
                }
            }
            return string.Empty;
        }

        // Add Items DropDown and Textboxes
        /// <summary>
        /// Retrieves item names for populating dropdowns and textboxes by calling the "PurchaseProcedure"
        /// stored procedure with flag "ItemNamePSM". Returns a DataSet containing item information.
        /// </summary>
        public async Task<DataSet> ItemNamePSM()
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("@flag", "ItemNamePSM");
            return await obj.ExecuteStoredProcedureReturnDS("PurchaseProcedure", param);
        }

        // Add Items DropDown and Textboxes
        /// <summary>
        /// Retrieves item names for populating dropdowns and textboxes by calling the "PurchaseProcedure"
        /// stored procedure with flag "ItemNamePSM". Returns a DataSet containing item information.
        /// </summary>
        public async Task<SqlDataReader> GetMRPItemsPSM(string plancode)
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("@flag", "MRPITemsListPSM");
            param.Add("@PlanCode", plancode);
            return await obj.ExecuteStoredProcedureReturnDataReader("PurchaseProcedure", param);
        }

        // Add Items DropDown and Textboxes
        /// <summary>
        /// Retrieves item names for populating dropdowns and textboxes by calling the "PurchaseProcedure"
        /// stored procedure with flag "ItemNamePSM". Returns a DataSet containing item information.
        /// </summary>
        public async Task<SqlDataReader> AddPlanNamePSM()
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("@flag", "AddPlanNamePSM");
            return await obj.ExecuteStoredProcedureReturnDataReader("PurchaseProcedure", param);
        }

        /// <summary>
        /// Creates a Purchase Requisition (PR) along with its items.
        /// Inserts PR and then each item (uses purchase.PRCode for item inserts).
        /// </summary>
        public async Task CreatePRADDItemPSM(CreatePRPSM purchase)
        {
            // Basic validation
            if (purchase == null)
                throw new ArgumentNullException(nameof(purchase));
            if (string.IsNullOrEmpty(purchase.PRCode))
                throw new ArgumentException("PRCode is required", nameof(purchase.PRCode));

            // Insert PR header
            Dictionary<string, object> prParam = new Dictionary<string, object>();
            prParam.Add("@flag", "CreatePRPSM");
            prParam.Add("@PRCode", purchase.PRCode);
            prParam.Add("@RequiredDate", purchase.RequiredDate); 
            prParam.Add("@AddedBy", purchase.AddedBy ?? "");
            prParam.Add("@AddedDate", purchase.AddedDate);  
            prParam.Add("@Priority", purchase.PriorityId);

            // Insert header record
            await obj.ExecuteStoredProcedure("PurchaseProcedure", prParam);

            // Insert PR items
            foreach (var item in purchase.Items)
            {
                Dictionary<string, object> itemParam = new Dictionary<string, object>();
                itemParam.Add("@flag", "AddPRItemPSM");
                itemParam.Add("@PRCode", purchase.PRCode);          
                itemParam.Add("@ItemCode", item.ItemCode ?? "");
                itemParam.Add("@RequiredQuantity", item.RequiredQuantity);

                await obj.ExecuteStoredProcedure("PurchaseProcedure", itemParam);
            }
        }

        // Add ItemReqStatusPSM to Dropdown
        /// <summary>
        /// Retrieves the list of ItemReqStatus options for PR creation by calling the "PurchaseProcedure"
        /// stored procedure with flag "ItemReqStatusPSM". Returns a SqlDataReader with ItemReqStatus data.
        /// </summary>
        public async Task<SqlDataReader> ItemReqStatusPSM()
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("@flag", "ItemReqStatusPSM");
            return await obj.ExecuteStoredProcedureReturnDataReader("PurchaseProcedure", param);
        }

        // Add Priority to Dropdown
        /// <summary>
        /// Retrieves the list of priority options for PR creation by calling the "PurchaseProcedure"
        /// stored procedure with flag "PriorityPSM". Returns a SqlDataReader with priority data.
        /// </summary>
        public async Task<SqlDataReader> PriorityPSM()
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("@flag", "PriorityPSM");
            return await obj.ExecuteStoredProcedureReturnDataReader("PurchaseProcedure", param);
        }

        // Delete PR Items
        /// <summary>
        /// Deletes a specific item from a PR by calling the "PurchaseProcedure" stored procedure
        /// with flag "DeletePRItemPSM" and passing the ItemCode as parameter.
        /// </summary>
        /// <param name="ItemCode">The code of the item to be deleted.</param>
        public async Task DeletePRItemPSM(string ItemCode)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("@flag", "DeletePRItemPSM");
            data.Add("@ItemCode", ItemCode);
            await obj.ExecuteStoredProcedureReturnDataReader("PurchaseProcedure", data);
        }

        #endregion



        #region Ashutosh
        /// <summary>
        /// BAL class method for Purchase Requisition Reports
        /// </summary>
        public async Task<List<Purchase>> ShowDataAT()
        {
            var para = new Dictionary<string, string>
        {
            { "@Flag", "PurchaseRequisitionReportAT" }
        };

            DataSet ds = await obj.ExecuteStoredProcedureReturnDS("PurchaseProcedure", para);
            var lstUserDtl = new List<Purchase>();

            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    var obj = new Purchase
                    {
                        PRCode = row["PRCode"]?.ToString(),
                        //PRCreatedDate = row["PRCreatedDate"]?.ToString(),
                        PRCreatedDate = row["PRCreatedDate"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(row["PRCreatedDate"]) : null,
                        //PRApprovedDate = row["PRApprovedDate"]?.ToString(),
                        PRApprovedDate = row["PRApprovedDate"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(row["PRApprovedDate"]) : null,

                        ConvertedToRFQ = row["ConvertedToRFQ"]?.ToString(),
                        RFQCode = row["RFQCode"]?.ToString(),
                        //RFQCreatedDate = row["RFQCreatedDate"]?.ToString(),
                        RFQCreatedDate = row["RFQCreatedDate"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(row["RFQCreatedDate"]) : null,

                        DaysToConvert = row["DaysToConvert"]?.ToString(),
                        StatusName = row["StatusName"]?.ToString()
                    };
                    lstUserDtl.Add(obj);
                }
            }
            return lstUserDtl;
        }
        /// <returns> List for PR</returns>


        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


        /// <summary>
        /// BAL class method for Purchase Order Reports
        /// </summary>

        public async Task<List<Purchase>> GetPurchaseOrdersAT()
        {
            var para = new Dictionary<string, string> { { "@Flag", "PurchaseOrderReportAT" } };
            DataSet ds = await obj.ExecuteStoredProcedureReturnDS("PurchaseProcedure", para);

            var purchaseList = new List<Purchase>();
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    purchaseList.Add(new Purchase
                    {
                        POCode = row["POCode"]?.ToString(),
                        VendorName = row["VendorName"]?.ToString(),
                        VendorCompanyName = row["VendorCompanyName"]?.ToString(),
                        AddedByName = row["AddedByName"]?.ToString(),
                        ApprovedRejectedByName = row["ApprovedRejectedByName"]?.ToString(),
                        //ApprovedRejectedDate = row["ApprovedRejectedDate"]?.ToString(),
                        ApprovedRejectedDateAT = row["ApprovedRejectedDate"] != DBNull.Value
    ? (DateTime?)Convert.ToDateTime(row["ApprovedRejectedDate"])
    : null,
                        ItemName = row["ItemName"]?.ToString(),
                        StatusName = row["StatusName"]?.ToString()
                    });
                }
            }

            // group by POCode
            var grouped = purchaseList
                .GroupBy(p => p.POCode)
                .Select(g => new Purchase
                {
                    POCode = g.Key,
                    VendorName = g.First().VendorName,
                    VendorCompanyName = g.First().VendorCompanyName,
                    AddedByName = g.First().AddedByName,
                    ApprovedRejectedByName = g.First().ApprovedRejectedByName,
                    ApprovedRejectedDateAT = g.First().ApprovedRejectedDateAT,
                    StatusName = g.First().StatusName,
                    Items = g.ToList()
                }).ToList();

            return grouped;
        }
        /// <returns>List for PO</returns>


        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        /// <summary>
        /// BAL class method for RFQ Reports
        /// </summary>

        public async Task<List<Purchase>> GetRFQReportAT()
        {
            var para = new Dictionary<string, string>
    {
        { "@Flag", "RFQReportAT" }
    };

            DataSet ds = await obj.ExecuteStoredProcedureReturnDS("PurchaseProcedure", para);

            var rfqList = new List<Purchase>();

            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    rfqList.Add(new Purchase
                    {
                        RFQCode = row["RFQCode"]?.ToString(),
                        AddedBy = row["AddedBy"]?.ToString(),
                        //AddedDate = row["AddedDate"]?.ToString(),
                        AddedDateAT = row["AddedDate"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(row["AddedDate"]) : null,

                        StatusName = row["StatusName"]?.ToString(),
                        VendorsInvited = row["VendorsInvited"]?.ToString(),
                        VendorsResponded = row["VendorsResponded"]?.ToString(),
                        ResponseRatePercent = row["ResponseRatePercent"]?.ToString(),
                        FinalOutcomePOCode = row["FinalOutcomePOCode"]?.ToString()
                    });
                }
            }
            return rfqList;
        }
        /// <returns> List for RFQ</returns>

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        /// <summary>
        /// po items to show on the view button
        /// </summary>
        public async Task<List<Purchase>> GetPOItemsAT(string poCode)
        {
            var para = new Dictionary<string, string>
            {
                { "@Flag", "POModalReportsAT" },
                { "@Pocode", poCode }
            };

            DataSet ds = await obj.ExecuteStoredProcedureReturnDS("PurchaseProcedure", para);
            var itemList = new List<Purchase>();

            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    itemList.Add(new Purchase
                    {
                        ItemName = row["ItemName"]?.ToString(),
                        UnitQuantity = row["UnitQuantity"]?.ToString(),
                        CostPerUnit = Convert.ToDecimal(row["CostPerUnit"]?.ToString()),
                        Discount = row["Discount"]?.ToString(),
                        TaxRate = row["TaxRate"]?.ToString(),
                        FinalAmount = row["FinalAmount"]?.ToString()
                    });
                }
            }

            return itemList;
        }
        /// <returns>List of PO Items</returns>

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


        /// <summary>
        //pr items to show on the view button
        /// </summary>
        public async Task<List<Purchase>> GetPRItemsAT(string prCode)
        {
            var para = new Dictionary<string, string>
    {
        { "@Flag", "PRModalAT" },
        { "@PRCode", prCode }
    };

            DataSet ds = await obj.ExecuteStoredProcedureReturnDS("PurchaseProcedure", para);
            var itemList = new List<Purchase>();

            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    itemList.Add(new Purchase
                    {
                        PRCode = row["PRCode"]?.ToString(),
                        ItemName = row["ItemName"]?.ToString(),
                        UnitRates = row["UnitRates"]?.ToString(),
                        RequiredQuantity = Convert.ToDecimal(row["RequiredQuantity"]?.ToString())
                    });
                }
            }

            return itemList;
        }
        /// <returns>List of PR Items</returns>

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////




        /// <summary>
        /// RFQ Registered Quotation List
        /// </summary>
        /// <returns></returns>
        public async Task<List<Purchase>> GetRFQVendorResponsesAT(string rfqCode)
        {
            if (string.IsNullOrEmpty(rfqCode))
                return new List<Purchase>();

            var para = new Dictionary<string, string>
    {
        { "@Flag", "RFQRQListAT" },
        { "@RFQCode", rfqCode }
    };

            DataSet ds = await obj.ExecuteStoredProcedureReturnDS("PurchaseProcedure", para);
            var vendorList = new List<Purchase>();

            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    vendorList.Add(new Purchase
                    {
                        RegisterQuotationCode = row["RegisterQuotationCode"]?.ToString(),
                        VendorName = ds.Tables[0].Columns.Contains("VenderName")
                                                                    ? row["VenderName"]?.ToString()
                                                                    : row["VendorName"]?.ToString(),
                        DaysToReceiveQuotation = row["DaysToReceiveQuotation"]?.ToString(),
                        DaysToApproveQuotation = row["DaysToApproveQuotation"]?.ToString()
                    });
                }
            }

            return vendorList;
        }
        /// <returns>List of RFQ Items</returns>
        #endregion


        #region Vaibhavi
        /// <summary>
        /// Retrieves all RFQs.
        /// </summary>
        /// <returns>List of Purchase objects containing RFQ details.</returns>
        public async Task<List<Purchase>> ShowAllRFQVNK()
        {
            try
            {
                Dictionary<string, string> dic = new Dictionary<string, string>();
                dic.Add("@Flag", "ShowAllRFQVNK");
                var ds = await obj.ExecuteStoredProcedureReturnDS("PurchaseProcedure", dic);
                List<Purchase> items = new List<Purchase>();

                if (ds != null && ds.Tables.Count > 0)
                {
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        items.Add(new Purchase
                        {
                            RFQCode = row["RFQCode"].ToString(),
                            PRCode = row["PRCode"].ToString(),
                            AddedDateVK = row["AddedDate"].ToString(),
                            FullName = row["FullName"].ToString(),
                            Description = row["Description"].ToString(),
                            HasUnregisteredVendors = row.Table.Columns.Contains("HasUnregisteredVendors") && row["HasUnregisteredVendors"] != DBNull.Value
                                ? Convert.ToInt32(row["HasUnregisteredVendors"])
                                : 0,

                            AnyVendor = row.Table.Columns.Contains("HasRegisteredQuotation") && row["HasRegisteredQuotation"] != DBNull.Value
                                ? Convert.ToInt32(row["HasRegisteredQuotation"])
                                : 0,
                            HasApproved = row.Table.Columns.Contains("HasApproved") && row["HasApproved"] != DBNull.Value
                                ? Convert.ToInt32(row["HasApproved"])
                                : 0

                        });


                    }
                }
                return items;
            }
            catch (Exception ex)
            {
                throw new Exception("Error in ShowAllRFQVNK", ex);
            }
        }

        /// <summary>
        /// Retrieves all registered quotations for a given RFQ.
        /// </summary>
        /// <param name="rfqCode">RFQ Code</param>
        /// <returns>List of RegisterQuotation objects.</returns>
        public async Task<List<RegisterQuotation>> ViewRFQVNK(string rfqCode)
        {
            try
            {
                Dictionary<string, string> dic = new Dictionary<string, string>();
                dic.Add("@Flag", "ViewRFQVNK");
                dic.Add("@RFQCode", rfqCode);

                var ds = await obj.ExecuteStoredProcedureReturnDS("PurchaseProcedure", dic);
                List<RegisterQuotation> items = new List<RegisterQuotation>();

                if (ds != null && ds.Tables.Count > 0)
                {
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        items.Add(new RegisterQuotation
                        {
                            RFQCode = row["RFQCode"].ToString(),
                            RegisterQuotationCode = row["RegisterQuotationCode"].ToString(),
                            VendorName = row["VenderName"].ToString(),
                            VendorDeliveryDateVK = row["VendorDeliveryDate"] != DBNull.Value
                           ? Convert.ToDateTime(row["VendorDeliveryDate"]).ToString("yyyy-MM-ddTHH:mm:ss")
                           : null

                        });
                    }
                }
                return items;
            }
            catch (Exception ex)
            {
                throw new Exception("Error in ViewRFQVNK", ex);
            }
        }

        /// <summary>
        /// Retrieves RFQ Header by RFQ Code.
        /// </summary>
        /// <param name="rfqCode">RFQ Code</param>
        /// <returns>DataTable containing RFQ header.</returns>
        public async Task<object> GetRFQHeaderVNK(string rfqCode)
        {
            try
            {
                var d = new Dictionary<string, string>{
                    {"@Flag","GetRFQHeaderVNK"},
                    {"@RFQCode", rfqCode}
                };
                var ds = await obj.ExecuteStoredProcedureReturnDS("PurchaseProcedure", d);
                return ds?.Tables[0];
            }
            catch (Exception ex)
            {
                throw new Exception("Error in GetRFQHeaderVNK", ex);
            }
        }

        /// <summary>
        /// Retrieves vendors linked to an RFQ.
        /// </summary>
        /// <param name="rfqCode">RFQ Code</param>
        /// <returns>List of RegisterQuotation objects containing vendor info.</returns>
        public async Task<List<RegisterQuotation>> GetVendorsByRFQVNK(string rfqCode)
        {
            try
            {
                Dictionary<string, string> dic = new Dictionary<string, string>();
                dic.Add("@Flag", "GetVendorsByRFQVNK");
                dic.Add("@RFQCode", rfqCode);

                DataSet ds = await obj.ExecuteStoredProcedureReturnDS("PurchaseProcedure", dic);

                List<RegisterQuotation> vendors = new List<RegisterQuotation>();

                if (ds != null && ds.Tables.Count > 0)
                {
                    vendors = ds.Tables[0].AsEnumerable().Select(row => new RegisterQuotation
                    {
                        VendorCode = row["VendorCode"].ToString(),
                        VendorName = row["VendorName"].ToString()
                    }).ToList();
                }

                return vendors;
            }
            catch (Exception ex)
            {
                throw new Exception("Error in GetVendorsByRFQVNK", ex);
            }
        }

        /// <summary>
        /// Gets GST details of an item by item code.
        /// </summary>
        /// <param name="itemCode">Item code</param>
        /// <returns>ItemGst object containing GST details.</returns>
        public async Task<ItemGst> GetItemGSTVNK(string itemCode)
        {
            try
            {
                Dictionary<string, string> dic = new Dictionary<string, string>();
                dic.Add("@Flag", "GetItemGSTVNK");
                dic.Add("@ItemCode", itemCode);

                DataSet ds = await obj.ExecuteStoredProcedureReturnDS("PurchaseProcedure", dic);

                ItemGst gst = null;
                if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    var row = ds.Tables[0].Rows[0];
                    gst = new ItemGst
                    {
                        ItemCode = row["ItemCode"].ToString(),
                        ItemName = row["ItemName"].ToString(),
                        Category = row["ItemCategoryName"].ToString(),
                        HSNCode = row["HSNCode"].ToString(),
                        CGSTCode = row["CGSTCode"].ToString(),
                        SCGSTCode = row["SCGSTCode"].ToString(),
                        IGSTCode = row["IGSTCode"].ToString(),
                        TotalGST = Convert.ToDecimal(row["TotalGST"])
                    };
                }
                return gst;
            }
            catch (Exception ex)
            {
                throw new Exception("Error in GetItemGSTVNK", ex);
            }
        }

        /// <summary>
        /// Gets vendor details by vendor code.
        /// </summary>
        /// <param name="vendorCode">Vendor code</param>
        /// <returns>List of RegisterQuotation containing vendor details.</returns>
        public async Task<List<RegisterQuotation>> GetVendorDetailsVNK(string vendorCode)
        {
            try
            {
                var dic = new Dictionary<string, string>
                {
                    { "@Flag", "GetVendorDetailsVNK" },
                    { "@VendorCode", vendorCode }
                };

                DataSet ds = await obj.ExecuteStoredProcedureReturnDS("PurchaseProcedure", dic);

                var list = new List<RegisterQuotation>();
                if (ds != null && ds.Tables.Count > 0)
                {
                    list = ds.Tables[0].AsEnumerable().Select(r => new RegisterQuotation
                    {
                        VendorCode = r["VendorCode"].ToString(),
                        CompanyName = r["CompanyName"].ToString()
                    }).ToList();
                }
                return list;
            }
            catch (Exception ex)
            {
                throw new Exception("Error in GetVendorDetailsVNK", ex);
            }
        }


        /// <summary>
        /// Retrieves items for a given PR code.
        /// </summary>
        /// <param name="prCode">Purchase Requisition Code</param>
        /// <returns>List of RegisterQuotationItem objects.</returns>
        public async Task<List<RegisterQuotationItem>> GetItemsForRFQVNK(string prCode)
        {
            try
            {
                var dic = new Dictionary<string, string>
                {
                    { "@Flag", "GetItemsForRFQVNK" },
                    { "@PRCode", prCode },
                    { "@ItemsJson", "" }
                };

                DataSet ds = await obj.ExecuteStoredProcedureReturnDS("PurchaseProcedure", dic);

                var items = new List<RegisterQuotationItem>();
                if (ds != null && ds.Tables.Count > 0)
                {
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        items.Add(new RegisterQuotationItem
                        {
                            ItemCode = row["ItemCode"].ToString(),
                            ItemName = row["ItemName"].ToString(),
                            UOMName = row["UOMName"]?.ToString(),
                            Description = row["Description"]?.ToString(),
                            Quantity = Convert.ToInt32(row["RequiredQuantity"]?.ToString())
                        });
                    }
                }
                return items;
            }
            catch (Exception ex)
            {
                throw new Exception("Error in GetItemsForRFQVNK", ex);
            }
        }

        /// <summary>
        /// Saves a registered quotation for a given RFQ.
        /// </summary>
        /// <param name="rq">RegisterQuotation object</param>
        /// <returns>RegisterQuotationCode if saved successfully, otherwise null.</returns>
        public async Task<string> SaveRegisterQuotationVNK(RegisterQuotation rq, string addedby)
        {
            try
            {
                var dic = new Dictionary<string, string>
                {
                    { "@Flag", "SaveRegisterQuotationVNK" },
                    { "@RFQCode", rq.RFQCode },
                    { "@VendorCode", rq.VendorCode },
                    { "@VendorDeliveryDate", rq.VendorDeliveryDateVK.ToString() },
                    { "@ShippingCharges", rq.ShippingCharges.ToString() },
                    { "@ItemsJson", BuildItemsJson(rq.Items) },
                    {"@AddedBy",addedby },
                    { "@AddedDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") }
                };

                var ds = await obj.ExecuteStoredProcedureReturnDS("PurchaseProcedure", dic);

                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    string code = ds.Tables[0].Rows[0]["RegisterQuotationCode"].ToString();
                    return code;
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception("Error in SaveRegisterQuotationVNK", ex);
            }
        }

        /// <summary>
        /// Builds a JSON string for quotation items.
        /// </summary>
        /// <param name="items">List of RegisterQuotationItem</param>
        /// <returns>JSON string</returns>
        private string BuildItemsJson(List<RegisterQuotationItem> items)
        {
            if (items == null || items.Count == 0) return "[]";

            var sb = new StringBuilder();
            sb.Append("[");
            for (int i = 0; i < items.Count; i++)
            {
                var it = items[i];
                if (i > 0) sb.Append(",");
                sb.Append("{");
                sb.AppendFormat("\"ItemCode\":\"{0}\",", it.ItemCode);
                sb.AppendFormat("\"Quantity\":{0},", it.Quantity);
                sb.AppendFormat("\"CostPerUnit\":{0},", it.CostPerUnit);
                sb.AppendFormat("\"Discount\":{0}", it.Discount);
                sb.Append("}");
            }
            sb.Append("]");
            return sb.ToString();
        }

        /// <summary>
        /// Gets register quotation header by code.
        /// </summary>
        /// <param name="rqCode">Register Quotation Code</param>
        /// <returns>RegisterQuotation object</returns>
        public async Task<RegisterQuotation> GetRQHeaderByCodeVNK(string rqCode)
        {
            try
            {
                var dic = new Dictionary<string, string>
                {
                    {"@Flag", "GetRQHeaderByCodeVNK"},
                    {"@RegisterQuotationCode", rqCode}
                };

                var ds = await obj.ExecuteStoredProcedureReturnDS("PurchaseProcedure", dic);

                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    var row = ds.Tables[0].Rows[0];
                    return new RegisterQuotation
                    {
                        RegisterQuotationCode = row["RegisterQuotationCode"].ToString(),
                        RFQCode = row["RFQCode"].ToString(),
                        VendorCode = row["VendorCode"].ToString(),
                        VendorName = row["VendorName"].ToString(),
                        CompanyName = row["CompanyName"].ToString(),
                        VendorDeliveryDateVK = row["VendorDeliveryDate"] == DBNull.Value
                            ? null
                            : ((DateTime)row["VendorDeliveryDate"]).ToString("yyyy-MM-dd"),
                        ShippingCharges = row["ShippingCharges"] == DBNull.Value ? 0
                            : Convert.ToDecimal(row["ShippingCharges"])
                    };
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception("Error in GetRQHeaderByCodeVNK", ex);
            }
        }

        /// <summary>
        /// Gets items linked to a registered quotation.
        /// </summary>
        /// <param name="rqCode">Register Quotation Code</param>
        /// <returns>List of RegisterQuotationItem objects</returns>
        public async Task<List<RegisterQuotationItem>> GetRQItemsByCodeVNK(string rqCode)
        {
            try
            {
                var dic = new Dictionary<string, string>
                {
                    {"@Flag", "GetRQItemsByCodeVNK"},
                    {"@RegisterQuotationCode", rqCode}
                };

                var ds = await obj.ExecuteStoredProcedureReturnDS("PurchaseProcedure", dic);
                var items = new List<RegisterQuotationItem>();

                if (ds != null && ds.Tables.Count > 0)
                {
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        items.Add(new RegisterQuotationItem
                        {
                            ItemCode = row["ItemCode"].ToString(),
                            ItemName = row["ItemName"].ToString(),
                            Description = row["Description"]?.ToString(),
                            UOMName = row["UOMName"]?.ToString(),
                            Quantity = row["Quantity"] == DBNull.Value ? 0 : Convert.ToDecimal(row["Quantity"]),
                            CostPerUnit = row["CostPerUnit"] == DBNull.Value ? 0 : Convert.ToDecimal(row["CostPerUnit"]),
                            Discount = row["Discount"] == DBNull.Value ? 0 : Convert.ToDecimal(row["Discount"]),
                            GST = row["GSTPct"] == DBNull.Value ? 0 : Convert.ToDecimal(row["GSTPct"]),
                           // ShippingCharges= row["ShippingCharges"]== DBNull.Value ? 0 : Convert.ToDecimal(row["ShippingCharges"])
                        });
                    }
                }
                return items;
            }
            catch (Exception ex)
            {
                throw new Exception("Error in GetRQItemsByCodeVNK", ex);
            }
        }

        /// <summary>
        /// Retrieves approved purchase orders.
        /// </summary>
        /// <returns>List of ApprovedPurchaseOrder objects</returns>
        public async Task<List<ApprovedPurchaseOrder>> GetApprovedPOsVNK()
        {
            try
            {
                var dic = new Dictionary<string, string> { { "@Flag", "GetApprovedPOsVNK" } };
                var ds = await obj.ExecuteStoredProcedureReturnDS("PurchaseProcedure", dic);
                var list = new List<ApprovedPurchaseOrder>();

                if (ds != null && ds.Tables.Count > 0)
                {
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        list.Add(new ApprovedPurchaseOrder
                        {
                            POCode = row["POCode"]?.ToString(),
                            RegisterQuotationCode = row.Table.Columns.Contains("RegisterQuotationCode") ? row["RegisterQuotationCode"]?.ToString() : null,
                            AddedDateVK = row["AddedDate"] == DBNull.Value ? null : Convert.ToDateTime(row["AddedDate"]).ToString("yyyy-MM-dd"),
                            ApprovedRejectedDateVK = row["ApprovedRejectedDate"] == DBNull.Value ? null : Convert.ToDateTime(row["ApprovedRejectedDate"]).ToString("yyyy-MM-dd"),
                            TotalAmount = row.Table.Columns.Contains("TotalAmount") && row["TotalAmount"] != DBNull.Value ? Convert.ToDecimal(row["TotalAmount"]) : 0m,
                            StatusName = row.Table.Columns.Contains("StatusName") ? row["StatusName"]?.ToString() : null,
                            CreatedBy = row.Table.Columns.Contains("CreatedBy") ? row["CreatedBy"]?.ToString() : null
                        });
                    }
                }
                return list;
            }
            catch (Exception ex)
            {
                throw new Exception("Error in GetApprovedPOsVNK", ex);
            }
        }

        /// <summary>
        /// Gets purchase order header by code.
        /// </summary>
        /// <param name="poCode">Purchase Order Code</param>
        /// <returns>POHeader object</returns>
        public async Task<POHeader> GetPOHeaderByCodeVNK(string poCode)
        {
            try
            {
                var dic = new Dictionary<string, string>
                {
                    { "@Flag", "GetPOHeaderByCodeVNK" },
                    { "@POCode", poCode }
                };

                var ds = await obj.ExecuteStoredProcedureReturnDS("PurchaseProcedure", dic);
                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    var row = ds.Tables[0].Rows[0];
                    var h = new POHeader
                    {
                        POCode = row.Table.Columns.Contains("POCode") ? row["POCode"]?.ToString() : null,
                        RegisterQuotationCode = row.Table.Columns.Contains("RegisterQuotationCode") ? row["RegisterQuotationCode"]?.ToString() : null,
                        AddedDateVK = row.Table.Columns.Contains("AddedDate") && row["AddedDate"] != DBNull.Value ? Convert.ToDateTime(row["AddedDate"]).ToString("yyyy-MM-dd") : null,
                        ApprovedRejectedDateVK = row.Table.Columns.Contains("ApprovedRejectedDate") && row["ApprovedRejectedDate"] != DBNull.Value ? Convert.ToDateTime(row["ApprovedRejectedDate"]).ToString("yyyy-MM-dd") : null,
                        TotalAmount = row.Table.Columns.Contains("TotalAmount") && row["TotalAmount"] != DBNull.Value ? Convert.ToDecimal(row["TotalAmount"]) : 0m,
                        DeliveryAddress = row.Table.Columns.Contains("DeliveryAddress") ? row["DeliveryAddress"]?.ToString() : null,
                        TermConditionName = row.Table.Columns.Contains("TermConditionName") ? row["TermConditionName"]?.ToString() : null,
                        VendorCode = row.Table.Columns.Contains("VendorCode") ? row["VendorCode"]?.ToString() : null,
                        VendorName = row.Table.Columns.Contains("VendorName") ? row["VendorName"]?.ToString() : null,
                        VendorCompanyName = row.Table.Columns.Contains("VendorCompanyName") ? row["VendorCompanyName"]?.ToString() : null,
                        VendorContact = row.Table.Columns.Contains("VendorContact") ? row["VendorContact"]?.ToString() : null,
                        VendorAddress = row.Table.Columns.Contains("VendorAddress") ? row["VendorAddress"]?.ToString() : null,
                        InvoiceToCompanyName = row.Table.Columns.Contains("InvoiceToCompanyName") ? row["InvoiceToCompanyName"]?.ToString() : null
                    };

                    return h;
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception("Error in GetPOHeaderByCodeVNK", ex);
            }
        }

        /// <summary>
        /// Gets purchase order items by PO code.
        /// </summary>
        /// <param name="poCode">Purchase Order Code</param>
        /// <returns>List of POItem objects</returns>
        public async Task<List<POItem>> GetPOItemsByCodeVNK(string poCode)
        {
            try
            {
                var dic = new Dictionary<string, string>
                {
                    { "@Flag", "GetPOItemsByCodeVNK" },
                    { "@POCode", poCode }
                };

                var ds = await obj.ExecuteStoredProcedureReturnDS("PurchaseProcedure", dic);
                var list = new List<POItem>();
                if (ds != null && ds.Tables.Count > 0)
                {
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        list.Add(new POItem
                        {
                            POItemCode = row.Table.Columns.Contains("POItemCode") ? row["POItemCode"]?.ToString() : null,
                            POCode = row.Table.Columns.Contains("POCode") ? row["POCode"]?.ToString() : null,
                            RegisterQuotationItemCode = row.Table.Columns.Contains("RegisterQuotationItemCode") ? row["RegisterQuotationItemCode"]?.ToString() : null,
                            ItemCode = row.Table.Columns.Contains("ItemCode") ? row["ItemCode"]?.ToString() : null,
                            ItemName = row.Table.Columns.Contains("ItemName") ? row["ItemName"]?.ToString() : null,
                            Description = row.Table.Columns.Contains("Description") ? row["Description"]?.ToString() : null,
                            UOMName = row.Table.Columns.Contains("UOMName") ? row["UOMName"]?.ToString() : null,
                            Quantity = row.Table.Columns.Contains("Quantity") && row["Quantity"] != DBNull.Value ? Convert.ToDecimal(row["Quantity"]) : 0m,
                            CostPerUnit = row.Table.Columns.Contains("CostPerUnit") && row["CostPerUnit"] != DBNull.Value ? Convert.ToDecimal(row["CostPerUnit"]) : 0m,
                            Discount = row.Table.Columns.Contains("Discount") && row["Discount"] != DBNull.Value ? Convert.ToDecimal(row["Discount"]) : 0m,
                            GSTPct = row.Table.Columns.Contains("GSTPct") && row["GSTPct"] != DBNull.Value ? Convert.ToDecimal(row["GSTPct"]) : 0m,
                        ShippingCharges =row.Table.Columns.Contains("ShippingCharges") && row["ShippingCharges"] != DBNull.Value ? Convert.ToDecimal(row["ShippingCharges"]) : 0m


                        });
                    }
                }
                return list;
            }
            catch (Exception ex)
            {
                throw new Exception("Error in GetPOItemsByCodeVNK", ex);
            }
        }

        //nur







        /// <summary>
        /// Retrieves the list of pending purchase orders (NAM).
        /// </summary>
        /// <returns>
        /// A list of <see cref="PendingPurchaseOrder"/> containing pending purchase orders.
        /// </returns>
        public async Task<List<PendingPurchaseOrder>> GetPendingPOsNAM()
        {
            var dic = new Dictionary<string, string> { { "@Flag", "GetPendingPOsNAM" } };
            var ds = await obj.ExecuteStoredProcedureReturnDS("PurchaseProcedure", dic);
            var list = new List<PendingPurchaseOrder>();

            if (ds != null && ds.Tables.Count > 0)
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    list.Add(new PendingPurchaseOrder
                    {
                        POCode = row["POCode"]?.ToString(),
                        PODateVK = row["PODate"] == DBNull.Value ? null : Convert.ToDateTime(row["PODate"]).ToString("yyyy-MM-dd"),
                        POCost = row["POCost"] == DBNull.Value ? 0 : Convert.ToDecimal(row["POCost"]),
                        CreatedBy = row["CreatedBy"]?.ToString(),
                        StatusName = row["StatusName"]?.ToString()
                    });
                }
            }
            return list;
        }

        /// <summary>
        /// Retrieves the purchase order header details by PO code.
        /// </summary>
        /// <param name="poCode">The purchase order code.</param>
        /// <returns>
        /// A <see cref="POHeaderNAM"/> object containing header details if found; otherwise, null.
        /// </returns>
        public async Task<POHeaderNAM> GetPOHeaderNAM(string poCode)
        {
            var dic = new Dictionary<string, string>
            {
                { "@Flag", "GetPOHeaderNAM" },
                { "@POCode", poCode }
            };

            var ds = await obj.ExecuteStoredProcedureReturnDS("PurchaseProcedure", dic);
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                var row = ds.Tables[0].Rows[0];
                var h = new POHeaderNAM
                {
                    POCode = row.Table.Columns.Contains("POCode") ? row["POCode"]?.ToString() : null,
                    RegisterQuotationCode = row.Table.Columns.Contains("RegisterQuotationCode") ? row["RegisterQuotationCode"]?.ToString() : null,
                    AddedDateVK = row.Table.Columns.Contains("AddedDate") && row["AddedDate"] != DBNull.Value ? Convert.ToDateTime(row["AddedDate"]).ToString("yyyy-MM-dd") : null,
                    ApprovedRejectedDateVK = row.Table.Columns.Contains("ApprovedRejectedDate") && row["ApprovedRejectedDate"] != DBNull.Value ? Convert.ToDateTime(row["ApprovedRejectedDate"]).ToString("yyyy-MM-dd") : null,
                    TotalAmount = row.Table.Columns.Contains("TotalAmount") && row["TotalAmount"] != DBNull.Value ? Convert.ToDecimal(row["TotalAmount"]) : 0m,
                    DeliveryAddress = row.Table.Columns.Contains("DeliveryAddress") ? row["DeliveryAddress"]?.ToString() : null,
                    TermConditionName = row.Table.Columns.Contains("TermConditionName") ? row["TermConditionName"]?.ToString() : null,
                    VendorCode = row.Table.Columns.Contains("VendorCode") ? row["VendorCode"]?.ToString() : null,
                    VendorName = row.Table.Columns.Contains("VendorName") ? row["VendorName"]?.ToString() : null,
                    VendorCompanyName = row.Table.Columns.Contains("VendorCompanyName") ? row["VendorCompanyName"]?.ToString() : null,
                    VendorContact = row.Table.Columns.Contains("VendorContact") ? row["VendorContact"]?.ToString() : null,
                    VendorAddress = row.Table.Columns.Contains("VendorAddress") ? row["VendorAddress"]?.ToString() : null,
                    InvoiceToCompanyName = row.Table.Columns.Contains("InvoiceToCompanyName") ? row["InvoiceToCompanyName"]?.ToString() : null,

                };

                return h;
            }
            return null;
        }

        /// <summary>
        /// Retrieves the purchase order item details by PO code.
        /// </summary>
        /// <param name="poCode">The purchase order code.</param>
        /// <returns>
        /// A list of <see cref="POItemNAM"/> containing the purchase order items.
        /// </returns>
        public async Task<List<POItemNAM>> GetPOItemsNAM(string poCode)
        {
            var dic = new Dictionary<string, string>
            {
                { "@Flag", "GetPOItemsNAM" },
                { "@POCode", poCode }
            };

            var ds = await obj.ExecuteStoredProcedureReturnDS("PurchaseProcedure", dic);
            var list = new List<POItemNAM>();
            if (ds != null && ds.Tables.Count > 0)
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    list.Add(new POItemNAM
                    {
                        POItemCode = row.Table.Columns.Contains("POItemCode") ? row["POItemCode"]?.ToString() : null,
                        POCode = row.Table.Columns.Contains("POCode") ? row["POCode"]?.ToString() : null,
                        RegisterQuotationItemCode = row.Table.Columns.Contains("RegisterQuotationItemCode") ? row["RegisterQuotationItemCode"]?.ToString() : null,
                        ItemCode = row.Table.Columns.Contains("ItemCode") ? row["ItemCode"]?.ToString() : null,
                        ItemName = row.Table.Columns.Contains("ItemName") ? row["ItemName"]?.ToString() : null,
                        Description = row.Table.Columns.Contains("Description") ? row["Description"]?.ToString() : null,
                        UOMName = row.Table.Columns.Contains("UOMName") ? row["UOMName"]?.ToString() : null,
                        Quantity = row.Table.Columns.Contains("Quantity") && row["Quantity"] != DBNull.Value ? Convert.ToDecimal(row["Quantity"]) : 0m,
                        CostPerUnit = row.Table.Columns.Contains("CostPerUnit") && row["CostPerUnit"] != DBNull.Value ? Convert.ToDecimal(row["CostPerUnit"]) : 0m,
                        Discount = row.Table.Columns.Contains("Discount") && row["Discount"] != DBNull.Value ? Convert.ToDecimal(row["Discount"]) : 0m,
                        GSTPct = row.Table.Columns.Contains("GSTPct") && row["GSTPct"] != DBNull.Value ? Convert.ToDecimal(row["GSTPct"]) : 0m,
                        ShippingCharges = row.Table.Columns.Contains("ShippingCharges") && row["ShippingCharges"] != DBNull.Value ? Convert.ToDecimal(row["ShippingCharges"]) : 0m

                    });
                }
            }
            return list;
        }

        /// <summary>
        /// Approves the purchase order by its PO code.
        /// </summary>
        /// <param name="poCode">The purchase order code to approve.</param>
        /// <returns>
        /// True if the purchase order is approved successfully; otherwise, false.
        /// </returns>
        public async Task<bool> ApprovePONAM(string poCode)
        {
            var dic = new Dictionary<string, string>
            {
                { "@Flag", "ApprovePoNAM" },
                { "@POCode", poCode }
            };

            try
            {
                await obj.ExecuteStoredProcedure("PurchaseProcedure", dic);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }




        /// <summary>
        /// Rejects the purchase order by its PO code.
        /// </summary>
        /// <param name="poCode">The purchase order code to reject.</param>
        /// <returns>
        /// True if the purchase order is rejected successfully; otherwise, false.
        /// </returns>
        public async Task<bool> RejectPONAM(string poCode)
        {
            var dic = new Dictionary<string, string>
            {
                { "@Flag", "RejectPoNAM" },
                { "@POCode", poCode }
            };

            try
            {
                await obj.ExecuteStoredProcedure("PurchaseProcedure", dic);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Sends the purchase order for higher approval by its PO code.
        /// </summary>
        /// <param name="poCode">The purchase order code to send for approval.</param>
        /// <returns>
        /// True if the purchase order is sent successfully; otherwise, false.
        /// </returns>
        public async Task<bool> SendForApprovalNAM(string poCode)
        {
            var dic = new Dictionary<string, string>
            {
                { "@Flag", "SendforApprovalPONAM" },
                { "@POCode", poCode }
            };

            try
            {
                await obj.ExecuteStoredProcedure("PurchaseProcedure", dic);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }



        #endregion Vaibhavi

        #region Akash
        // <summary>
        /// Retrieves a list of supplier quotations that are pending approval (AMG).
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation. 
        /// The task result contains a list of <see cref="PurchaseAMG"/> objects.
        /// </returns>
        public async Task<List<PurchaseAMG>> GetPendingSupplierQuotationsAsyncAMG()
        {
            List<PurchaseAMG> list = new List<PurchaseAMG>();

            try
            {
                Dictionary<string, string> param = new Dictionary<string, string>
        {
            { "@Flag", "PendingSupplierQuotAMG" }
        };

                using (SqlDataReader dr = await obj.ExecuteStoredProcedureReturnDataReader("PurchaseProcedure", param))
                {
                    DataTable dt = new DataTable();
                    dt.Load(dr);

                    foreach (DataRow row in dt.Rows)
                    {
                        PurchaseAMG item = new PurchaseAMG
                        {
                            RFQCode = row["RFQCode"].ToString(),
                            RegisterQuotationCode = row["RegisterQuotationCode"].ToString(),
                            AddedDate = Convert.ToDateTime(row["AddedDate"]).ToString("dd/MM/yyyy"),
                            VenderName = row["VenderName"].ToString(),
                            CompanyName = row["CompanyName"].ToString(),
                            TotalAmount = row["TotalAmount"].ToString(),

                            ExpectedDate = row["ExpectedDate"] != DBNull.Value
                                ? Convert.ToDateTime(row["ExpectedDate"]).ToString("dd/MM/yyyy")
                                : string.Empty,
                            VendorDeliveryDate = row["VendorDeliveryDate"] != DBNull.Value
                                ? Convert.ToDateTime(row["VendorDeliveryDate"]).ToString("dd/MM/yyyy")
                                : string.Empty,
                            DeliverySpeed = row["DeliverySpeed"].ToString(),
                            AffordableRank = row["AffordableRank"].ToString(),
                            RecommendedQuotation = row["RecommendedQuotation"].ToString()
                        };

                        list.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error while retrieving pending supplier quotations.", ex);
            }

            return list;
        }


        /// <summary>
        /// Retrieves the header information of a specific quotation (AMG).
        /// </summary>
        /// <param name="quotationCode">The quotation code to fetch header details.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. 
        /// The task result contains a list of <see cref="PendingQuotViewHeader"/> objects.
        /// </returns>
        public async Task<List<PendingQuotViewHeader>> GetQuotationHeaderAsyncAMG(string quotationCode)
        {
            List<PendingQuotViewHeader> list = new List<PendingQuotViewHeader>();

            try
            {
                Dictionary<string, string> param = new Dictionary<string, string>
                {
                    { "@Flag", "getPendingQuotHeaderViewAMG" },
                    { "@RegisterQuotationCode", quotationCode }
                };

                using (SqlDataReader dr = await obj.ExecuteStoredProcedureReturnDataReader("PurchaseProcedure", param))
                {
                    DataTable dt = new DataTable();
                    dt.Load(dr);

                    foreach (DataRow row in dt.Rows)
                    {
                        PendingQuotViewHeader item = new PendingQuotViewHeader
                        {
                            RFQCode = row["RFQCode"].ToString(),
                            RegisterQuotationCode = row["RegisterQuotationCode"].ToString(),
                            VQDate = row["VQDate"] == DBNull.Value ? null : row["VQDate"].ToString(),
                            VendorDeliveryDate = row["VendorDeliveryDate"] == DBNull.Value ? null : row["VendorDeliveryDate"].ToString(),

                            ShippingCharges = row["ShippingCharges"].ToString(),
                            VenderName = row["VenderName"].ToString(),
                            VendorCode = row["VendorCode"].ToString(),
                            CompanyName = row["CompanyName"].ToString()
                        };
                        list.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while retrieving quotation header for {quotationCode}.", ex);
            }

            return list;
        }

        /// <summary>
        /// Retrieves the item details of a specific quotation (AMG).
        /// </summary>
        /// <param name="quotationCode">The quotation code to fetch item details.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. 
        /// The task result contains a list of <see cref="PendingQuotViewItems"/> objects.
        /// </returns>
        public async Task<List<PendingQuotViewItems>> GetQuotationItemsAsyncAMG(string quotationCode)
        {
            List<PendingQuotViewItems> list = new List<PendingQuotViewItems>();

            try
            {
                Dictionary<string, string> param = new Dictionary<string, string>
                {
                    { "@Flag", "getPendingQuotItemsDetailsAMG" },
                    { "@RegisterQuotationCode", quotationCode }
                };

                using (SqlDataReader dr = await obj.ExecuteStoredProcedureReturnDataReader("PurchaseProcedure", param))
                {
                    DataTable dt = new DataTable();
                    dt.Load(dr);

                    foreach (DataRow row in dt.Rows)
                    {
                        PendingQuotViewItems item = new PendingQuotViewItems
                        {
                            ItemCode = row["ItemCode"].ToString(),
                            ItemName = row["ItemName"].ToString(),
                            Quantity = Convert.ToInt32(row["Quantity"]),
                            CostPerUnit = Convert.ToDecimal(row["CostPerUnit"]),
                            GrossAmount = Convert.ToDecimal(row["GrossAmount"]),

                            // Discount %
                            DiscountPercent = Convert.ToDecimal(row["DiscountPercent"]),

                            // DiscountAmount = Gross - Net (you can calculate here if SP doesn’t return it)
                            DiscountAmount = row.Table.Columns.Contains("DiscountAmount")
             ? Convert.ToDecimal(row["DiscountAmount"])
             : (Convert.ToDecimal(row["GrossAmount"]) * Convert.ToDecimal(row["DiscountPercent"]) / 100),

                            // NetAmount = Gross - Discount (before GST)
                            NetAmount = Convert.ToDecimal(row["GrossAmount"]) -
                     (Convert.ToDecimal(row["GrossAmount"]) * Convert.ToDecimal(row["DiscountPercent"]) / 100),

                            // New fields from SP
                            TotalGST = Convert.ToDecimal(row["TotalGST"]),
                            FinalAmount = Convert.ToDecimal(row["FinalAmount"])
                        };
                        list.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while retrieving quotation items for {quotationCode}.", ex);
            }

            return list;
        }

        /// <summary>
        /// Approves a pending quotation request as an administrator (AMG).
        /// </summary>
        /// <param name="quotationCode">The quotation code to approve.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. 
        /// The task result is true if approval was successful; otherwise, false.
        /// </returns>
        public async Task<bool> ApproveRequestPendingQuotAdminAMG(string quotationCode, string StaffCode)
        {
            try
            {
                Dictionary<string, object> param = new Dictionary<string, object>
                {
                    { "@Flag", "ApproveRequestPendingQuotAdminAMG" },
                    { "@RegisterQuotationCode", quotationCode },
                    { "@approvedate", DateTime.Now },
                     {"@StaffCode", StaffCode}
                };

                await obj.ExecuteStoredProcedure("PurchaseProcedure", param);

                return true;

            }
            catch (Exception ex)
            {
                throw new Exception($"Error while approving pending quotation request {quotationCode}.", ex);
            }
        }

        /// <summary>
        /// Approves a pending quotation (AMG).
        /// </summary>
        /// <param name="quotationCode">The quotation code to approve.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. 
        /// The task result is true if approval was successful; otherwise, false.
        /// </returns>
        public async Task<bool> ApproveQuotAMG(string quotationCode, string StaffCode)
        {
            try
            {
                Dictionary<string, object> param = new Dictionary<string, object>
                {
                    { "@Flag", "ApprovePendingQuotAMG" },
                    { "@RegisterQuotationCode", quotationCode },
                    { "@approvedate", DateTime.Now },
                    {"@StaffCode", StaffCode}
                };

                await obj.ExecuteStoredProcedure("PurchaseProcedure", param);

                return true;

            }
            catch (Exception ex)
            {
                throw new Exception($"Error while approving quotation {quotationCode}.", ex);
            }
        }

        /// <summary>
        /// Retrieves a list of supplier quotations that have been approved (AMG).
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation. 
        /// The task result contains a list of <see cref="PurchaseAMG"/> objects.
        /// </returns>
        public async Task<List<PurchaseAMG>> ApprovedSupplierQuotationsAMG()
        {
            List<PurchaseAMG> list = new List<PurchaseAMG>();

            try
            {
                Dictionary<string, string> param = new Dictionary<string, string>
                {
                    { "@Flag", "ApprovedSupplierQuotAMG" }
                };

                using (SqlDataReader dr = await obj.ExecuteStoredProcedureReturnDataReader("PurchaseProcedure", param))
                {
                    DataTable dt = new DataTable();
                    dt.Load(dr);

                    foreach (DataRow row in dt.Rows)
                    {
                        PurchaseAMG item = new PurchaseAMG
                        {
                            RFQCode = row["RFQCode"].ToString(),
                            RegisterQuotationCode = row["RegisterQuotationCode"].ToString(),
                            AddedDate = Convert.ToDateTime(row["ApprovedRejectedDate"])
                 .ToString("dd/MM/yyyy"),
                            VenderName = row["VenderName"].ToString(),
                            CompanyName = row["CompanyName"].ToString(),
                            TotalAmount = row["TotalAmount"].ToString()
                        };

                        list.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error while retrieving approved supplier quotations.", ex);
            }

            return list;
        }
        #endregion


        #region Shubham
        /// <summary>
        /// Creates a new Purchase Requisition (PR) and its items.
        /// Inserts both header and item details into the database using stored procedure.
        /// </summary>
        /// <param name="purchase">PR header with item list</param>
        public async Task CreatePR(PurchaseHeader purchase)
        {
            Dictionary<string, string> prParam = new Dictionary<string, string>();
            prParam.Add("@Flag", "NewPRSP");
            prParam.Add("@PRCode", purchase.PRCode);
            prParam.Add("@RequiredDate", purchase.RequiredDate.ToString("yyyy-MM-dd"));
            prParam.Add("@StatusId", purchase.Status.ToString());
            prParam.Add("@AddedBy", purchase.AddedBy);
            prParam.Add("@AddedDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            prParam.Add("@PriorityId", purchase.PriorityId.ToString());

            await obj.ExecuteStoredProcedure("PurchaseProcedure", prParam);

            // Insert PR Items
            foreach (var item in purchase.Items)
            {
                Dictionary<string, string> itemParam = new Dictionary<string, string>();
                itemParam.Add("@Flag", "NewPRItemSP");
                itemParam.Add("@PRCode", item.PRCode);
                itemParam.Add("@ItemCode", item.ItemCode);
                itemParam.Add("@RequiredQuantity", Convert.ToInt32(item.RequiredQuantity).ToString());

                await obj.ExecuteStoredProcedure("PurchaseProcedure", itemParam);
            }
        }

        /// <summary>
        /// Returns all Purchase Requisitions.
        /// </summary>
        public async Task<List<Purchase>> GetAllPRSP()
        {
            List<Purchase> list = new List<Purchase>();
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("@Flag", "GetAllSP");

            SqlDataReader dr = await obj.ExecuteStoredProcedureReturnDataReader("PurchaseProcedure", param);
            while (await dr.ReadAsync())
            {
                list.Add(new Purchase
                {
                    PRCode = dr["PRCode"].ToString(),
                    RequiredDate = Convert.ToDateTime(dr["RequiredDate"]),
                    AddedBy = dr.IsDBNull(dr.GetOrdinal("AddedBy")) ? string.Empty : dr["AddedBy"].ToString(),
                    AddedDate = dr.IsDBNull(dr.GetOrdinal("AddedDate")) ? DateTime.MinValue : dr.GetDateTime(dr.GetOrdinal("AddedDate")),
                    ApproveRejectedBy = dr.IsDBNull(dr.GetOrdinal("ApproveRejectedBy")) ? string.Empty : dr["ApproveRejectedBy"].ToString(),
                    ApproveRejectedDate = dr.IsDBNull(dr.GetOrdinal("ApproveRejectedDate")) ? DateTime.MinValue : dr.GetDateTime(dr.GetOrdinal("ApproveRejectedDate")),
                    Priority = dr.IsDBNull(dr.GetOrdinal("Priority")) ? string.Empty : dr["Priority"].ToString(),
                    Status = dr.IsDBNull(dr.GetOrdinal("Status")) ? string.Empty : dr["Status"].ToString()
                });
            }
            return list;
        }

        /// <summary>
        /// Returns requisition header details by PR code.
        /// </summary>
        public async Task<Purchase> GetPRByCodeSP(string prCode)
        {
            Purchase pr = null;

            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("@Flag", "GetPRByCodeSP");
            param.Add("@PRCode", prCode);

            using (SqlDataReader dr = await obj.ExecuteStoredProcedureReturnDataReader("PurchaseProcedure", param))
            {
                if (await dr.ReadAsync())
                {
                    pr = new Purchase();

                    pr.PRCode = dr["PRCode"].ToString();
                    pr.RequiredDate = Convert.ToDateTime(dr["RequiredDate"]);
                    pr.FullName = dr["FullName"].ToString();
                    pr.Status = dr["StatusName"].ToString();
                }
            }
            return pr;
        }

        /// <summary>
        /// Returns all items of a requisition by PR code.
        /// </summary>
        public async Task<List<Purchase>> GetPRItemsSP(string prCode)
        {
            List<Purchase> list = new List<Purchase>();

            Dictionary<string, string> itemParam = new Dictionary<string, string>();
            itemParam.Add("@Flag", "GetPRItemsSP");
            itemParam.Add("@PRCode", prCode);

            SqlDataReader dr = await obj.ExecuteStoredProcedureReturnDataReader("PurchaseProcedure", itemParam);
            while (await dr.ReadAsync())
            {
                list.Add(new Purchase
                {
                    PRItemId = Convert.ToInt32(dr["PRItemId"]),
                    ItemCode = dr["ItemCode"].ToString(),
                    ItemName = dr["ItemName"].ToString(),
                    Description = dr["Description"].ToString(),
                    RequiredQuantity = dr["RequiredQuantity"] != DBNull.Value ? Convert.ToInt32(dr["RequiredQuantity"]) : 0,
                    UnitRate = dr["UnitRates"] != DBNull.Value ? Convert.ToDecimal(dr["UnitRates"]) : 0,
                    Amount = dr["Amount"] != DBNull.Value ? Convert.ToDecimal(dr["Amount"]) : 0
                });
            }
            return list;
        }

        /// <summary>
        /// Updates required quantity of a PR item.
        /// </summary>
        public async Task UpdateItemQuantitySP(int @PRItemId, int requiredQuantity)
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("@Flag", "UpdateItemQuantitySP");   // Stored procedure flag to identify operation
            param.Add("@PRItemId", @PRItemId.ToString());
            param.Add("@RequiredQuantity", requiredQuantity.ToString());

            await obj.ExecuteStoredProcedure("PurchaseProcedure", param);
        }

        /// <summary>
        /// Returns all pending requisitions by status id.
        /// </summary>
        public async Task<List<Purchase>> GetPendingPRSP(int statusid)
        {
            List<Purchase> list = new List<Purchase>();
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("@Flag", "GetPendingPRSP");
            param.Add("@StatusId", statusid.ToString());

            SqlDataReader dr = await obj.ExecuteStoredProcedureReturnDataReader("PurchaseProcedure", param);
            while (await dr.ReadAsync())
            {
                list.Add(new Purchase
                {
                    PRCode = dr.IsDBNull(dr.GetOrdinal("PRCode")) ? string.Empty : dr["PRCode"].ToString(),  // safe string
                    RequiredDate = dr.IsDBNull(dr.GetOrdinal("RequiredDate")) ? DateTime.MinValue : dr.GetDateTime(dr.GetOrdinal("RequiredDate")),
                    AddedBy = dr.IsDBNull(dr.GetOrdinal("AddedBy")) ? string.Empty : dr["AddedBy"].ToString(),
                    AddedDate = dr.IsDBNull(dr.GetOrdinal("AddedDate")) ? DateTime.MinValue : dr.GetDateTime(dr.GetOrdinal("AddedDate")),
                    ApproveRejectedBy = dr.IsDBNull(dr.GetOrdinal("ApproveRejectedBy")) ? string.Empty : dr["ApproveRejectedBy"].ToString(),
                    ApproveRejectedDate = dr.IsDBNull(dr.GetOrdinal("ApproveRejectedDate")) ? DateTime.MinValue : dr.GetDateTime(dr.GetOrdinal("ApproveRejectedDate")),
                    Priority = dr.IsDBNull(dr.GetOrdinal("Priority")) ? string.Empty : dr["Priority"].ToString(),
                    Status = dr.IsDBNull(dr.GetOrdinal("Status")) ? string.Empty : dr["Status"].ToString()
                });
            }
            return list;
        }

        /// <summary>
        /// Returns master item list (ItemCode, Name, UOM, Description, UnitRate).
        /// </summary>
        public async Task<List<Purchase>> GetItemsSP(int itemcatagoryid)
        {
            List<Purchase> list = new List<Purchase>();

            Dictionary<string, string> itemParam = new Dictionary<string, string>();
            itemParam.Add("@Flag", "GetItemSP");
            itemParam.Add("@ItemCatagoryId", itemcatagoryid.ToString());


            SqlDataReader dr = await obj.ExecuteStoredProcedureReturnDataReader("PurchaseProcedure", itemParam);
            while (await dr.ReadAsync())
            {
                list.Add(new Purchase
                {
                    ItemCode = dr["ItemCode"].ToString(),
                    ItemName = dr["ItemName"].ToString(),
                    UOM = dr["UOMName"].ToString(),
                    Description = dr["Description"].ToString(),
                    UnitRate = dr["UnitRates"] != DBNull.Value ? Convert.ToInt32(dr["UnitRates"]) : 0
                });
            }
            return list;
        }

        /// <summary>
        /// Returns priority/status list from database.
        /// </summary>
        public async Task<SqlDataReader> GetPrioritySP()
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("@Flag", "GetPrioritySP");
            return await obj.ExecuteStoredProcedureReturnDataReader("PurchaseProcedure", param);
        }

        /// <summary>
        /// Returns Industry Type list from database.
        /// </summary>
        public async Task<SqlDataReader> GetIndustryTypeSP()
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("@Flag", "GetIndustryTypeSP");
            return await obj.ExecuteStoredProcedureReturnDataReader("PurchaseProcedure", param);
        }

        /// <summary>
        /// Generates a new PR Code from database.
        /// </summary>
        public async Task<Purchase> NewPRCodeSP()
        {
            Purchase pr = null;

            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("@Flag", "PRCodeSP");


            using (SqlDataReader dr = await obj.ExecuteStoredProcedureReturnDataReader("PurchaseProcedure", param))
            {
                if (await dr.ReadAsync())
                {
                    pr = new Purchase();

                    pr.PRCode = dr["PRCode"].ToString();
                }
            }
            return pr;
        }

        /// <summary>
        /// Generates a new PR Item Code from database.
        /// </summary>
        public async Task<Purchase> NewPRItemCodeSP()
        {
            Purchase pr = null;

            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("@Flag", "PRItemCodeSP");


            using (SqlDataReader dr = await obj.ExecuteStoredProcedureReturnDataReader("PurchaseProcedure", param))
            {
                if (await dr.ReadAsync())
                {
                    pr = new Purchase();

                    pr.PRItemCode = dr["PRItemCode"].ToString();
                }
            }
            return pr;
        }
        #endregion


        #region Omkar
        /// <summary>
        /// Fetches pending vendors awaiting approval that came from RFQ (Request for Quotation).
        /// <returns>DataSet containing pending vendor details</returns>
        /// </summary>
        public async Task<DataSet> FetchPendingVendorOK()
        {
            Dictionary<string, string> para = new Dictionary<string, string>();
            para.Add("@Flag", "FetchApprovalPendingVendorOK");
            DataSet ds = await obj.ExecuteStoredProcedureReturnDS("PurchaseProcedure", para);
            return ds;
        }

        /// <summary>
        /// Retrieves the list of selected quotations for display in the system.
        /// <returns>DataSet containing selected quotation list with details</returns>
        /// </summary>

        public async Task<DataSet> ReciveQuotationListOK()
        {
            Dictionary<string, string> para = new Dictionary<string, string>();
            para.Add("@Flag", "ReciveQuotationListOK");
            DataSet ds = await obj.ExecuteStoredProcedureReturnDS("PurchaseProcedure", para);
            return ds;
        }

        /// <summary>
        /// Fetches items for a specific selected quotation by register quotation code.
        /// <returns>DataSet containing quotation items with details</returns>
        /// </summary>

        public async Task<DataSet> FetchSelectedQuotationItemsOK(string RQCode)
        {
            Dictionary<string, string> para = new Dictionary<string, string>();
            para.Add("@Flag", "FetchSelectedQuotationItemsOK");
            para.Add("@RegisterQuotationCode", RQCode);
            DataSet ds = await obj.ExecuteStoredProcedureReturnDS("PurchaseProcedure", para);
            return ds;

        }

        /// <summary>
        /// Fetches all industry types available for vendor registration.
        /// <returns>DataSet containing list of industry types</returns>
        /// </summary>

        public async Task<DataSet> FetchIndustryTypeOK()
        {
            Dictionary<string, string> para = new Dictionary<string, string>();
            para.Add("@Flag", "FetchIndustryTypeOK");
            DataSet ds = await obj.ExecuteStoredProcedureReturnDS("PurchaseProcedure", para);
            return ds;
        }

        /// <summary>
        /// Fetches banks and their corresponding Swift codes from the database.
        /// <returns>DataSet containing banks with Swift codes</returns>
        /// </summary>

        public async Task<DataSet> FetchBankAndSwiftCodeOK()
        {
            Dictionary<string, string> para = new Dictionary<string, string>();
            para.Add("@Flag", "FetchBankAndSwiftCodeOK");
            DataSet ds = await obj.ExecuteStoredProcedureReturnDS("PurchaseProcedure", para);
            return ds;
        }

        /// <summary>
        /// Fetches branches and IFSC codes for a specific bank.
        /// </summary>
        /// <param name="BankId">The ID of the bank to fetch branches for</param>
        /// <returns>DataSet containing branches with IFSC codes for the specified bank</returns>


        public async Task<DataSet> FetchBranchAndIFSCCodeOK(int BankId)
        {
            Dictionary<string, string> para = new Dictionary<string, string>();
            para.Add("@Flag", "FetchBranchAndIFSCCodeOK");
            para.Add("@BankId", BankId.ToString());
            DataSet ds = await obj.ExecuteStoredProcedureReturnDS("PurchaseProcedure", para);
            return ds;
        }

        /// <summary>
        /// Fetches the maximum IDs for vendor and vendor company to generate new codes.
        /// </summary>
        /// <returns>DataSet containing maximum IDs for vendor and vendor company</returns>

        public async Task<DataSet> FetchVendorandVendorCompantMaxIdOK()
        {
            Dictionary<string, string> para = new Dictionary<string, string>();
            para.Add("@Flag", "FetchVendorandVendorCompantMaxIdOK");
            DataSet ds = await obj.ExecuteStoredProcedureReturnDS("PurchaseProcedure", para);
            return ds;
        }

        /// <summary>
        /// Saves vendor details including vendor information, company details, and account information.
        /// </summary>
        /// <param name="p">Purchase object containing vendor details</param>
        /// <returns>Boolean indicating success or failure of the save operation</returns>

        public async Task<bool> SaveVendor(Purchase p)
        {
            try
            {
                Dictionary<string, object> para = new Dictionary<string, object>();

                para.Add("@Flag", "InsertVendorOK");


                // Vendor parameters
                para.Add("@VendorCode", p.VendorCode);
                para.Add("@VenderName", p.VendorName);
                para.Add("@VendorMobileNo", p.MobileNo.ToString());
                para.Add("@VendorAlternateNo", p.AlternateNo.ToString());
                para.Add("@VendorEmail", p.Email);
                para.Add("@VendorAddress", p.Address);
                para.Add("@StaffCode", p.StaffCode);
                para.Add("@AddedDate", DateTime.Now);

                // VendorCompany parameters
                para.Add("@VendorCompanyCode", p.VendorCompanyCode);
                para.Add("@CompanyName", p.CompanyName);
                para.Add("@CompanyMobileNo", p.CompanyMobileNo.ToString());
                para.Add("@CompanyAlternateNo", p.CompanyAlternateNo.ToString());
                para.Add("@CompanyEmail", p.CompanyEmail);
                para.Add("@CompanyAddress", p.CompanyAddress);
                para.Add("@IndustryTypeId ", p.IndustryTypeId.ToString());
                para.Add("@CountryCode", p.CountryCode);
                para.Add("@StateCode", p.StateCode);
                para.Add("@CityId", p.CityId.ToString());
                para.Add("@VStaffCode", p.StaffCode);
                para.Add("@VAddedDate", DateTime.Now);
                para.Add("@ApprovedRejectedBy", p.StaffCode);
                para.Add("@ApprovedRejectedDate", DateTime.Now);


                // Vendor Account parameters
                para.Add("@BranchId", p.BranchId.ToString());
                para.Add("@UserCode", p.VendorCode);
                para.Add("@AccountNumber", p.AccountNumber.ToString());


                //  para.Add("@AddedBy", "STF008");

                // Execute SP
                await obj.ExecuteStoredProcedure("PurchaseProcedure", para);
                return true;
            }
            catch (Exception ex)
            {
                return false;
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Approves a pending vendor by their ID..
        /// </summary>
        /// <param name="VendorId">The ID of the vendor to approve</param>
        /// <returns>Boolean indicating success or failure of the approval operation</returns>

        public async Task<bool> ApproveVendorOK(int VendorId,string staffcode)
        {
            try
            {
                Dictionary<string, string> para = new Dictionary<string, string>();
                para.Add("@Flag", "ApproveVendorOK");
                para.Add("@VendorId", VendorId.ToString());
                para.Add("@StaffCode", staffcode);
                para.Add("@ApprovedRejectedDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                await obj.ExecuteStoredProcedure("PurchaseProcedure", para);
                
                return true;
            }
            catch (Exception ex)
            {
                return false;
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Fetches all quotation data required to create a purchase order for a specific register quotation code.
        /// </summary>
        /// <param name="RegisterQuotationCode">The register quotation code to fetch data for</param>
        /// <returns>DataSet containing comprehensive quotation data for PO creation</returns>

        public async Task<DataSet> GetQuotationAllDataOK(string RegisterQuotationCode)
        {
            Dictionary<string, string> para = new Dictionary<string, string>();
            para.Add("@Flag", "GetQuotationAllDatatoCreatePOOK");
            para.Add("@RegisterQuotationCode", RegisterQuotationCode);
            DataSet ds = await obj.ExecuteStoredProcedureReturnDS("PurchaseProcedure", para);
            for (int i = 0; i < ds.Tables.Count; i++)
            {
                Console.WriteLine($"Table {i}: {ds.Tables[i].Rows.Count} rows");
            }
            return ds;
        }

        /// <summary>
        /// Saves a purchase order with its header information and associated items.
        /// </summary>
        /// <param name="model">Purchase object containing PO details</param>
        /// <returns>Boolean indicating success or failure of the save operation</returns>

        public async Task<bool> SavePOOK(Purchase model)
        {
            try
            {
                DateTime date = DateTime.Now;
                if (model == null || model.POItems == null || !model.POItems.Any())
                    throw new ArgumentException("PO items are required");

                string itemsCsv = string.Join(",", model.POItems);

                // Join terms as CSV string
                string termsCsv = model.TermConditionIds;

                // Prepare parameters
                Dictionary<string, object> para = new Dictionary<string, object>();
                para.Add("@Flag", "SavePOOK");
                para.Add("@RegisterQuotationCode", model.RegisterQuotationCode);
                para.Add("@BillingAddress", model.BillingAddress);
                para.Add("@TermsConditionIds", termsCsv);
                para.Add("@UserCode", model.UserCode);
                para.Add("@AddedDate", date);
                para.Add("@StaffCode", model.StaffCode);
                para.Add("@TotalAmount", model.TotalAmount);

                SqlDataReader rd = await obj.ExecuteStoredProcedureReturnDataReaderObject("PurchaseProcedure", para);

                string poCode = null;
                if (rd != null && await rd.ReadAsync())
                {
                    poCode = rd["POCode"].ToString();
                }
                else
                {
                    throw new Exception("No POCode returned from stored procedure.");
                }
                rd.Close();

                // Save items
                await SavePOItemsOK(model, poCode);


                return true;
            }
            catch (Exception ex)
            {
                return false;
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Saves individual purchase order items associated with a specific PO code.
        /// </summary>
        /// <param name="model">Purchase object containing item details</param>
        /// <param name="POcode">The PO code to associate items with</param>
        /// <returns>Boolean indicating success or failure of the save operation</returns>

        public async Task<bool> SavePOItemsOK(Purchase model, string POcode)
        {
            foreach (var item in model.POItems)
            {
                // Parse JSON string
                var arr = Newtonsoft.Json.JsonConvert.DeserializeObject<JArray>(item);

                foreach (var objs in arr)
                {
                    string rqItemCode = objs["RQItemCode"].ToString();

                    Dictionary<string, string> para = new Dictionary<string, string>();
                    para.Add("@Flag", "SavePOItemsOK");
                    para.Add("@POCode", POcode);
                    para.Add("@RQItemCode", rqItemCode);

                    await obj.ExecuteStoredProcedureReturnDS("PurchaseProcedure", para);
                }
            }
            return true;
        }

        /// <summary>
        /// Fetches purchase order history data for display in the system.
        /// </summary>
        /// <returns>DataSet containing PO history with details</returns>

        public async Task<DataSet> FetchPOHistoryOK()
        {
            Dictionary<string, string> para = new Dictionary<string, string>();
            para.Add("@Flag", "FetchPOHistroyOK");
            DataSet ds = await obj.ExecuteStoredProcedureReturnDS("PurchaseProcedure", para);
            return ds;
        }

        /// <summary>
        /// Fetches specific purchase order details by PO code for PDF generation.
        /// </summary>
        /// <param name="POCode">The PO code to fetch details for</param>
        /// <returns>DataSet containing comprehensive PO details for PDF</returns>

        public async Task<DataSet> FetchPODetailsByPOCodeForOPDFOK(string POCode)
        {
            Dictionary<string, string> para = new Dictionary<string, string>();
            para.Add("@Flag", "FetchPOdetailsForPOPDFOK");
            para.Add("@POCode", POCode);
            DataSet ds = await obj.ExecuteStoredProcedureReturnDS("PurchaseProcedure", para);
            return ds;
        }
        //<summary>
        //Fetch All Just In Time Items to create the PO
        //</summary>
        public async Task<DataSet> FetchAllJITItemsOK()
        {
            Dictionary<string, string> para = new Dictionary<string, string>();
            para.Add("@Flag", "FetchAllJITItemsOK");
            DataSet ds = await obj.ExecuteStoredProcedureReturnDS("PurchaseProcedure", para);
            return ds;
        }
        /// <summary>
        /// Fetch item details for selected JIT items
        /// </summary>
        public async Task<DataSet> FetchSelectedJITItemDetailstOK(string itemCode, int stockRequirementId)
        {
            Dictionary<string, string> para = new Dictionary<string, string>();
            para.Add("@Flag", "FetchJITDetaistoPOOK");
            para.Add("@ItemCode", itemCode);
            para.Add("@StockRequirementId", stockRequirementId.ToString());

            DataSet ds = await obj.ExecuteStoredProcedureReturnDS("PurchaseProcedure", para);
            return ds;
        }




        /// <summary>
        /// Save Just In Time Items to create PO
        /// <summary>

        public async Task<bool> SaveJITPOOK(Purchase model)
        {
            try
            {
                DateTime date = DateTime.Now;
                if (model == null || model.POItems == null || !model.POItems.Any())
                    throw new ArgumentException("PO items are required");

                string itemsCsv = string.Join(",", model.POItems);

                string itemscodeCsv = string.Join(",", model.Itemslst);

                // Join terms as CSV string
                string termsCsv = model.TermConditionIds;

                // Prepare parameters
                Dictionary<string, object> para = new Dictionary<string, object>();
                para.Add("@Flag", "SaveJITPOOK");
                para.Add("@RegisterQuotationCode", model.RegisterQuotationCode);
                para.Add("@BillingAddress", model.BillingAddress);
                para.Add("@TermsConditionIds", termsCsv);
                para.Add("@UserCode", model.UserCode);
                para.Add("@AddedDate", date);
                para.Add("@StaffCode", model.StaffCode);
                para.Add("@TotalAmount", model.TotalAmount);

                SqlDataReader rd = await obj.ExecuteStoredProcedureReturnDataReaderObject("PurchaseProcedure", para);

                string poCode = null;
                if (rd != null && await rd.ReadAsync())
                {
                    poCode = rd["POCode"].ToString();
                }
                else
                {
                    throw new Exception("No POCode returned from stored procedure.");
                }
                rd.Close();

                // Save items
                await SavePOJITItemsOK(model, poCode);

                // ✅ Update ItemRequirement Status separately
                await UpdateItemRequirementStatus(model);

                return true;
            }
            catch (Exception ex)
            {
                return false;
                throw new Exception(ex.Message);
            }
        }

        public async Task<bool> SavePOJITItemsOK(Purchase model, string POcode)
        {
            //Dictionary<string, string> parameters = new Dictionary<string, string>();
            //parameters.Add("@Flag", "GetNewRQItemCodeOK");
            //SqlDataReader rd = await obj.ExecuteStoredProcedureReturnDataReader("PurchaseProcedure", parameters);
            //string rqItemCode = null;
            //if(rd.Read())
            //{
            //    rqItemCode = rd["NewRQItemCode"].ToString();
            //}
            foreach (var item in model.POItems)
            {
                // Parse JSON string
                    var arr = Newtonsoft.Json.JsonConvert.DeserializeObject<JArray>(item);

                    foreach (var objs in arr)
                    {
                        string rqItemCode = objs["RQItemCode"].ToString();

                        Dictionary<string, string> para = new Dictionary<string, string>();
                        para.Add("@Flag", "SaveJITPOItemsOK");
                        para.Add("@POCode", POcode);
                        para.Add("@RQItemCode", rqItemCode);
                       // para.Add("@ItemCode", itemcode);

                        await obj.ExecuteStoredProcedureReturnDS("PurchaseProcedure", para);
                    }

            }
            return true;
        }


        // ✅ Separate function to update ItemRequirement Status
        public async Task<bool> UpdateItemRequirementStatus(Purchase model)
        {
            foreach (var item in model.Itemslst)
            {
                var arr = Newtonsoft.Json.JsonConvert.DeserializeObject<JArray>(item);
                foreach (var objs in arr)
                {
                    string itemCode = objs["ItemCode"]?.ToString();
                    if (string.IsNullOrEmpty(itemCode))
                        continue;

                    Dictionary<string, string> para = new Dictionary<string, string>
            {
                { "@Flag", "UpdateItemRequirementStatusOK" }, // New flag
                { "@ItemCode", itemCode },
                {@"StockrequirementId",model.StockReqirementId.ToString()}
            };

                    await obj.ExecuteStoredProcedureReturnDS("PurchaseProcedure", para);
                }
            }
            return true;
        }


        public async Task<SqlDataReader> FetchMaxPoCodeToAttachmentok()
        {
            Dictionary<string, string> para = new Dictionary<string, string>();
            para.Add("@Flag", "FetchMaxPOCodeOK");
            SqlDataReader dr = await obj.ExecuteStoredProcedureReturnDataReader("PurchaseProcedure", para);
            return dr;
        }

        #endregion

        #region prathamesh
        /// <summary>
        /// This Function Showing UserDashboard Count
        /// </summary>
        /// <returns> Counts </returns>
        public async Task<Purchase> GetPurchasePRK(DateTime startDate, DateTime endDate)
        {
            var counts = new Purchase
            {
                AllPR = 0,
                ApprovedPR = 0,
                PendingPR = 0,
                Rejected = 0,
                ApprovedRC = 0,
                PendingRC = 0,
                ApprovedPO = 0,
                PendingPO = 0,
                RequestedRFQ = 0,
                PendingRFQ = 0
            };

            try
            {
                // Helper function to get count by flag
                async Task<int> GetCountAsync(string flag)
                {
                    var param = new Dictionary<string, string>
            {
                { "@Flag", flag },
                { "@StartDate", startDate.ToString("yyyy-MM-dd") },
                { "@EndDate", endDate.ToString("yyyy-MM-dd") }
            };
                    var ds = await obj.ExecuteStoredProcedureReturnDS("PurchaseProcedure", param);
                    if (ds?.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                    {
                        int.TryParse(ds.Tables[0].Rows[0][0]?.ToString(), out int result);
                        return result;
                    }
                    return 0;
                }

                counts.AllPR = await GetCountAsync("AllPRCountPRK");
                counts.ApprovedPR = await GetCountAsync("ApprovePRCountPRK");
                counts.PendingPR = await GetCountAsync("PendingPRCountPRK");
                counts.Rejected = await GetCountAsync("RejectPRCountPRK");
                counts.PendingRC = await GetCountAsync("PendingRCCountPRK");
                counts.ApprovedRC = await GetCountAsync("ApproveRCCountPRK");
                counts.ApprovedPO = await GetCountAsync("ApprovePOCountPRK");
                counts.PendingPO = await GetCountAsync("PendingPOCountPRK");
                counts.RequestedRFQ = await GetCountAsync("RequestedRFQCountPRK");
                counts.PendingRFQ = await GetCountAsync("PendingRFQCountPRK");

                return counts;
            }
            catch (Exception ex)
            {
                throw new Exception("Error in GetPurchasePRK", ex);
            }
        }

        /// <summary>
        /// This Function Showing Trends Count
        /// </summary>
        /// <returns> Counts </returns>
        public async Task<object> GetPurchaseTrends(DateTime startDate, DateTime endDate)
        {
            try
            {
                var param = new Dictionary<string, string>
        {
            { "@Flag", "PurchaseTrendsPRK" },
            { "@StartDate", startDate.ToString("yyyy-MM-dd") },
            { "@EndDate", endDate.ToString("yyyy-MM-dd") }
        };

                var ds = await obj.ExecuteStoredProcedureReturnDS("PurchaseProcedure", param);

                var categories = new List<string>();
                var prData = new List<int>();
                var rfqData = new List<int>();
                var rqData = new List<int>();
                var poData = new List<int>();

                if (ds.Tables.Count > 0)
                {
                    categories = ds.Tables[0].AsEnumerable()
                        .Select(r => Convert.ToDateTime(r["Date"]).ToString("dd-MM-yyyy"))
                        .Distinct()
                        .OrderBy(d => DateTime.ParseExact(d, "dd-MM-yyyy", null))
                        .ToList();

                    foreach (var date in categories)
                    {
                        prData.Add(ds.Tables[0].AsEnumerable()
                            .Where(r => Convert.ToDateTime(r["Date"]).ToString("dd-MM-yyyy") == date && r["Type"].ToString() == "PR")
                            .Select(r => Convert.ToInt32(r["Count"]))
                            .FirstOrDefault());

                        rfqData.Add(ds.Tables[0].AsEnumerable()
                            .Where(r => Convert.ToDateTime(r["Date"]).ToString("dd-MM-yyyy") == date && r["Type"].ToString() == "RFQ")
                            .Select(r => Convert.ToInt32(r["Count"]))
                            .FirstOrDefault());

                        rqData.Add(ds.Tables[0].AsEnumerable()
                            .Where(r => Convert.ToDateTime(r["Date"]).ToString("dd-MM-yyyy") == date && r["Type"].ToString() == "RQ")
                            .Select(r => Convert.ToInt32(r["Count"]))
                            .FirstOrDefault());

                        poData.Add(ds.Tables[0].AsEnumerable()
                            .Where(r => Convert.ToDateTime(r["Date"]).ToString("dd-MM-yyyy") == date && r["Type"].ToString() == "PO")
                            .Select(r => Convert.ToInt32(r["Count"]))
                            .FirstOrDefault());
                    }
                }

                return new { Categories = categories, PR = prData, RFQ = rfqData, RQ = rqData, PO = poData };
            }
            catch (Exception ex)
            {
                throw new Exception("Error in GetPurchaseTrends", ex);
            }
        }





        /// <summary>
        /// This Function Show list of PR
        /// </summary>
        /// <returns> LIst Of All PR</returns>

        public async Task<List<Purchase>> ShowAllPRPRK()
        {
            try
            {
                List<Purchase> lst = new List<Purchase>();
                Dictionary<string, string> paradic = new Dictionary<string, string> { { "@Flag", "ShowAllPRPRK" } };
                DataSet ds = await obj.ExecuteStoredProcedureReturnDS("PurchaseProcedure", paradic);

                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        Purchase p = new Purchase();

                        p.PRCode = row["PRCode"].ToString();
                        p.Priority = row["Priority"].ToString();
                        p.StatusName = row["StatusName"].ToString();
                        p.AddedDate = Convert.ToDateTime(row["AddedDate"].ToString());
                        p.AddedDateString = p.AddedDate.ToString("dd-MM-yyyy");
                        p.ApprovedRejectedDate = Convert.ToDateTime(row["ApproveRejectedDate"].ToString());
                        p.ApprovedRejectedDateString = p.ApprovedRejectedDate.ToString("dd-MM-yyyy");
                        p.FullName = row["FullName"].ToString();
                        p.Description = row["Description"].ToString();
                        lst.Add(p);

                    }
                }

                return lst;
            }
            catch (Exception ex)
            {
                throw new Exception("Error in ShowAllPRPRK", ex);
            }
        }

        /// <summary>
        /// This Function Show Pending PR List
        /// </summary>
        /// <returns>List of Pending PR </returns>
        public async Task<List<Purchase>> ShowPendingPRPRK()
        {
            try
            {
                List<Purchase> lst = new List<Purchase>();
                Dictionary<string, string> paradic = new Dictionary<string, string> { { "@Flag", "ShowAllPendingPRPRK" } };
                DataSet ds = await obj.ExecuteStoredProcedureReturnDS("PurchaseProcedure", paradic);

                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {

                        Purchase p = new Purchase();


                        p.PRCode = row["PRCode"].ToString();
                        p.AddedDate = Convert.ToDateTime(row["AddedDate"].ToString());
                        p.AddedDateString = p.AddedDate.ToString("dd-MM-yyyy");
                        p.FullName = row["FullName"].ToString();
                        p.RequiredDate = Convert.ToDateTime(row["RequiredDate"].ToString());
                        //p.StatusName = row["StatusName"].ToString();
                        p.Priority = row["Priority"].ToString();
                        lst.Add(p);

                    }
                }

                return lst;
            }
            catch (Exception ex)
            {
                throw new Exception("Error in ShowPendingPRPRK", ex);
            }
        }

        /// <summary>
        /// This Function Show Pending PR Items
        /// </summary>
        /// <returns>List Of Show Pending PR Items</returns>
        public async Task<List<Purchase>> ShowPendingPRItemPRK(string prCode)
        {
            try
            {
                List<Purchase> lst = new List<Purchase>();
                Dictionary<string, string> paradic = new Dictionary<string, string>
                {
                    { "@Flag", "ShowPendingPRItemPRK" },
                    { "@PRCode", prCode }
                };

                DataSet ds = await obj.ExecuteStoredProcedureReturnDS("PurchaseProcedure", paradic);
                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        Purchase p = new Purchase();

                        p.PRCode = row["PRCode"].ToString();
                        p.ItemName = row["ItemName"].ToString();
                        p.UnitRates = row["UnitRates"].ToString();
                        p.RequiredQuantity = Convert.ToInt32(row["RequiredQuantity"]);
                        p.RequiredDate = Convert.ToDateTime(row["RequiredDate"].ToString());
                        p.RequiredDateString = p.RequiredDate.ToString("dd-MM-yyyy");

                        //p.Priority = row["Priority"].ToString();
                        lst.Add(p);

                    }
                }

                return lst;
            }
            catch (Exception ex)
            {
                throw new Exception("Error in ShowPendingPRItemPRK", ex);
            }
        }


        /// <summary>
        /// This Function Show Approved PR List
        /// </summary>
        /// <returns>Approved PR List</returns>
        public async Task<List<Purchase>> ShowApprovedPRPRK()
        {
            try
            {
                List<Purchase> lst = new List<Purchase>();
                Dictionary<string, string> paradic = new Dictionary<string, string> { { "@Flag", "ViewApprovePRPRK" } };
                DataSet ds = await obj.ExecuteStoredProcedureReturnDS("PurchaseProcedure", paradic);

                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        Purchase p = new Purchase();


                        p.PRCode = row["PRCode"].ToString();
                        p.AddedDate = Convert.ToDateTime(row["AddedDate"].ToString());
                        p.AddedDateString = p.AddedDate.ToString("dd-MM-yyyy");
                        p.RequiredDate = Convert.ToDateTime(row["RequiredDate"].ToString());
                        //p.StatusName = row["StatusName"].ToString();
                        p.ApprovedRejectedDate = Convert.ToDateTime(row["ApproveRejectedDate"].ToString());
                        p.ApprovedRejectedDateString = p.ApprovedRejectedDate.ToString("dd-MM-yyyy");
                        p.Priority = row["Priority"].ToString();
                        lst.Add(p);
                    }
                }

                return lst;
            }
            catch (Exception ex)
            {
                throw new Exception("Error in ShowApprovedPRPRK", ex);
            }
        }


        /// <summary>
        /// This Function Show Approved PR Items
        /// </summary>
        /// <returns>List of Approved PR Items</returns>
        public async Task<List<Purchase>> ShowApprovePRItemPRK(string prCode)
        {
            try
            {
                List<Purchase> lst = new List<Purchase>();
                Dictionary<string, string> paradic = new Dictionary<string, string>
                {
                    { "@Flag", "ShowPendingPRItemPRK" },
                    { "@PRCode", prCode }
                };

                DataSet ds = await obj.ExecuteStoredProcedureReturnDS("PurchaseProcedure", paradic);
                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        Purchase p = new Purchase();
                        p.PRCode = row["PRCode"].ToString();
                        p.ItemName = row["ItemName"].ToString();
                        p.UnitRates = row["UnitRates"].ToString();
                        p.RequiredQuantity = Convert.ToInt32(row["RequiredQuantity"]);
                        p.RequiredDate = Convert.ToDateTime(row["RequiredDate"].ToString());
                        p.RequiredDateString = p.RequiredDate.ToString("dd-MM-yyyy");
                        //p.Priority = row["Priority"].ToString();
                        lst.Add(p);
                    }
                }

                return lst;
            }
            catch (Exception ex)
            {
                throw new Exception("Error in ShowApprovePRItemPRK", ex);
            }
        }


        /// <summary>
        /// This Function Show Rejected PR List
        /// </summary>
        /// <returns>Show Rejected PR List</returns>
        public async Task<List<Purchase>> ShowRejectedPRPRK()
        {
            try
            {
                List<Purchase> lst = new List<Purchase>();
                Dictionary<string, string> paradic = new Dictionary<string, string> { { "@Flag", "ViewRejectedPRPRK" } };
                DataSet ds = await obj.ExecuteStoredProcedureReturnDS("PurchaseProcedure", paradic);

                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        Purchase p = new Purchase();

                        p.PRCode = row["PRCode"].ToString();
                        p.AddedDate = Convert.ToDateTime(row["AddedDate"].ToString());
                        p.AddedDateString = p.AddedDate.ToString("dd-MM-yyyy");
                        p.StatusName = row["StatusName"].ToString();
                        p.Priority = row["Priority"].ToString();
                        p.Description = row["Description"].ToString();

                        lst.Add(p);
                    }
                }

                return lst;
            }
            catch (Exception ex)
            {
                throw new Exception("Error in ShowRejectedPRPRK", ex);
            }
        }


        /// <summary>
        /// This Function Show Rejected PR Items
        /// </summary>
        /// <returns>List of Show Rejected PR Items</returns>

        public async Task<List<Purchase>> ShowRejectedPRItemPRK(string prCode)
        {
            try
            {
                List<Purchase> lst = new List<Purchase>();
                Dictionary<string, string> paradic = new Dictionary<string, string>
                {
                    { "@Flag", "ViewRejectedPRItemPRK" },
                    { "@PRCode", prCode }
                };

                DataSet ds = await obj.ExecuteStoredProcedureReturnDS("PurchaseProcedure", paradic);
                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        lst.Add(new Purchase
                        {
                            PRCode = row["PRCode"].ToString(),
                            ItemName = row["ItemName"].ToString(),
                            UnitRates = row["UnitRates"].ToString(),
                            //RequiredQuantity = row["RequiredQuantity"].ToString(),
                            RequiredDate = Convert.ToDateTime(row["RequiredDate"].ToString())
                        });
                    }
                }


                return lst;
            }
            catch (Exception ex)
            {
                throw new Exception("Error in ShowRejectedPRItemPRK", ex);
            }
        }


        /// <summary>
        ///  This Function Status Update Approve
        /// </summary>
        /// <returns>Status Update Approve</returns>
        public async Task<int> UpdatePRStatusPRK(string prCode, int statusId, string note, Purchase model)
        {
            try
            {
                Dictionary<string, string> paradic = new Dictionary<string, string>
                {
                    { "@Flag","UpdatedApprovePRK" },
                    { "@PRCode", prCode },
                    {"@StaffCode", model.StaffCode },
                    { "@StatusId", statusId.ToString() },
                    { "@ApproveRejectedDate",DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")},
                    { "@Description", note }
                };

                object result = await obj.ExecuteStoredProcedureReturnObject("PurchaseProcedure", paradic);
                return result != null ? Convert.ToInt32(result) : 0;
            }
            catch (Exception ex)
            {
                throw new Exception("Error in UpdatePRStatusPRK", ex);
            }
        }


        /// <summary>
        /// This Function Update Status Reject
        /// </summary>
        /// <returns>Update Status Reject</returns>

        public async Task<int> UpdatePRStatusReject(string prCode, int statusId, string note, Purchase model)
        {
            try
            {
                Dictionary<string, string> paradic = new Dictionary<string, string>
                {
                    { "@Flag","UpdatedRejectPRK" },
                    { "@PRCode", prCode },
                    {"@StaffCode",model.StaffCode },
                    { "@StatusId", statusId.ToString() },
                    { "@ApproveRejectedDate",DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")},
                    { "@Description", note }
                };

                object result = await obj.ExecuteStoredProcedureReturnObject("PurchaseProcedure", paradic);
                return result != null ? Convert.ToInt32(result) : 0;
            }
            catch (Exception ex)
            {
                throw new Exception("Error in UpdatePRStatusReject", ex);
            }
        }



        /////DashBord/////

        /// <summary>
        /// This Function Show Pending PR List
        /// </summary>
        /// <returns>List of Pending PR </returns>
        public async Task<List<Purchase>> ShowPendingPRDashPRK(DateTime startDate, DateTime endDate)
        {
            try
            {
                List<Purchase> lst = new List<Purchase>();
                Dictionary<string, string> paradic = new Dictionary<string, string> 
                { 
                    { "@Flag", "ShowAllPendingPRDashPRK" },
                    { "@StartDate", startDate.ToString("yyyy-MM-dd") },
                     { "@EndDate", endDate.ToString("yyyy-MM-dd") }
                };
                DataSet ds = await obj.ExecuteStoredProcedureReturnDS("PurchaseProcedure", paradic);

                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {

                        Purchase p = new Purchase();


                        p.PRCode = row["PRCode"].ToString();
                        p.AddedDate = Convert.ToDateTime(row["AddedDate"].ToString());
                        p.AddedDateString = p.AddedDate.ToString("dd-MM-yyyy");
                        p.FullName = row["FullName"].ToString();
                        p.RequiredDate = Convert.ToDateTime(row["RequiredDate"].ToString());
                        //p.StatusName = row["StatusName"].ToString();
                        p.Priority = row["Priority"].ToString();
                        lst.Add(p);

                    }
                }

                return lst;
            }
            catch (Exception ex)
            {
                throw new Exception("Error in ShowPendingPRPRK", ex);
            }
        }

        /// <summary>
        /// This Function Show Approved PR List
        /// </summary>
        /// <returns>Approved PR List</returns>
        public async Task<List<Purchase>> ShowApprovedPRDashPRK(DateTime startDate, DateTime endDate)
        {
            try
            {
                List<Purchase> lst = new List<Purchase>();
                Dictionary<string, string> paradic = new Dictionary<string, string> 
                {
                    { "@Flag", "ViewApprovePRDashPRK" },
                    { "@StartDate", startDate.ToString("yyyy-MM-dd") },
            { "@EndDate", endDate.ToString("yyyy-MM-dd") }
                };
                DataSet ds = await obj.ExecuteStoredProcedureReturnDS("PurchaseProcedure", paradic);

                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        Purchase p = new Purchase();


                        p.PRCode = row["PRCode"].ToString();
                        p.AddedDate = Convert.ToDateTime(row["AddedDate"].ToString());
                        p.AddedDateString = p.AddedDate.ToString("dd-MM-yyyy");
                        p.RequiredDate = Convert.ToDateTime(row["RequiredDate"].ToString());
                        //p.StatusName = row["StatusName"].ToString();
                        p.ApprovedRejectedDate = Convert.ToDateTime(row["ApproveRejectedDate"].ToString());
                        p.ApprovedRejectedDateString = p.ApprovedRejectedDate.ToString("dd-MM-yyyy");
                        p.Priority = row["Priority"].ToString();
                        lst.Add(p);
                    }
                }

                return lst;
            }
            catch (Exception ex)
            {
                throw new Exception("Error in ShowApprovedPRPRK", ex);
            }
        }



        /// <summary>
        /// This Function Show Approved PR List
        /// </summary>
        /// <returns>Approved PR List</returns>
        public async Task<List<Purchase>> ShowRequestedRFQPRK(DateTime startDate, DateTime endDate)
        {
            try
            {
                List<Purchase> lst = new List<Purchase>();
                Dictionary<string, string> paradic = new Dictionary<string, string> 
                {
                    { "@Flag", "RequestedRFQCDashPRK" },
                     { "@StartDate", startDate.ToString("yyyy-MM-dd") },
            { "@EndDate", endDate.ToString("yyyy-MM-dd") }
                };
                DataSet ds = await obj.ExecuteStoredProcedureReturnDS("PurchaseProcedure", paradic);

                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        Purchase p = new Purchase();

                        p.PRCode = row["PRCode"].ToString();
                        p.RFQCode = row["RFQCode"].ToString();
                        p.ExpectedDate = Convert.ToDateTime(row["ExpectedDate"].ToString());
                        p.ExpectedDateString = p.ExpectedDate.ToString("dd-MM-yyyy");
                        p.AddedDate = Convert.ToDateTime(row["AddedDate"].ToString());
                        p.AddedDateString = p.AddedDate.ToString("dd-MM-yyyy");
                        p.FullName = row["FullName"].ToString();
                        p.WarehouseName = row["WarehouseName"].ToString();
                        p.Description = row["Description"].ToString();

                        lst.Add(p);
                    }
                }


                return lst;
            }
            catch (Exception ex)
            {
                throw new Exception("Error in ShowApprovedPRPRK", ex);
            }
        }


        /// <summary>
        /// This Function Show RFQ PR List
        /// </summary>
        /// <returns>Approved RFQ  List</returns>
        public async Task<List<Purchase>> ShowPendingRFQPRK(DateTime startDate, DateTime endDate)
        {
            try
            {
                List<Purchase> lst = new List<Purchase>();
                Dictionary<string, string> paradic = new Dictionary<string, string> 
                {
                    { "@Flag", "PendingRFQDashPRK" },
                     { "@StartDate", startDate.ToString("yyyy-MM-dd") },
            { "@EndDate", endDate.ToString("yyyy-MM-dd") }
                };
                DataSet ds = await obj.ExecuteStoredProcedureReturnDS("PurchaseProcedure", paradic);

                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        Purchase p = new Purchase();
                        p.RFQCode = row["RFQCode"].ToString();
                        p.PRCode = row["PRCode"].ToString();
                        p.ExpectedDate = Convert.ToDateTime(row["ExpectedDate"].ToString());
                        p.ExpectedDateString = p.ExpectedDate.ToString("dd-MM-yyyy");
                        p.AddedDate = Convert.ToDateTime(row["AddedDate"].ToString());
                        p.AddedDateString = p.AddedDate.ToString("dd-MM-yyyy");
                        p.FullName = row["FullName"].ToString();
                        p.WarehouseName = row["WarehouseName"].ToString();
                        p.StatusName = row["StatusName"].ToString();
                        p.Description = row["Description"].ToString();

                        lst.Add(p);
                    }
                }

                return lst;
            }
            catch (Exception ex)
            {
                throw new Exception("Error in ShowApprovedPRPRK", ex);
            }
        }

        /// <summary>
        /// This Function Show RQ PR List
        /// </summary>
        /// <returns>Approved RQ  List</returns>
        public async Task<List<Purchase>> ShowApproveRQPRK(DateTime startDate, DateTime endDate)
        {
            try
            {
                List<Purchase> lst = new List<Purchase>();
                Dictionary<string, string> paradic = new Dictionary<string, string> 
                { 
                    { "@Flag", "ApprovedRQDashPRK" },
                     { "@StartDate", startDate.ToString("yyyy-MM-dd") },
            { "@EndDate", endDate.ToString("yyyy-MM-dd") }
                };
                DataSet ds = await obj.ExecuteStoredProcedureReturnDS("PurchaseProcedure", paradic);

                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        Purchase p = new Purchase();
                        p.RFQCode = row["RFQCode"].ToString();
                        p.RQCode = row["RegisterQuotationCode"].ToString();
                        p.AddedDate = Convert.ToDateTime(row["AddedDate"].ToString());
                        p.AddedDateString = p.AddedDate.ToString("dd-MM-yyyy");
                        p.VenderName = row["VenderName"].ToString();
                        p.CompanyName = row["CompanyName"].ToString();
                        p.TotalAmount = Convert.ToDecimal(row["TotalAmount"].ToString());
                        p.ExpectedDate = Convert.ToDateTime(row["VendorDeliveryDate"].ToString());
                        p.ExpectedDateString = p.ExpectedDate.ToString("dd-MM-yyyy");

                        lst.Add(p);
                    }
                }

                return lst;
            }

            catch (Exception ex)
            {
                throw new Exception("Error in ShowApprovedPRPRK", ex);
            }
        }


        /// <summary>
        /// This Function Show RFQ PR List
        /// </summary>
        /// <returns>Pending RFQ  List</returns>
        public async Task<List<Purchase>> ShowPendingRQPRK(DateTime startDate, DateTime endDate)
        {
            try
            {
                List<Purchase> lst = new List<Purchase>();
                Dictionary<string, string> paradic = new Dictionary<string, string> 
                {
                    { "@Flag", "PendingRQDashPRK" },
                     { "@StartDate", startDate.ToString("yyyy-MM-dd") },
            { "@EndDate", endDate.ToString("yyyy-MM-dd") }
                };
                DataSet ds = await obj.ExecuteStoredProcedureReturnDS("PurchaseProcedure", paradic);

                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        Purchase p = new Purchase();

                        p.RFQCode = row["RFQCode"].ToString();
                        p.RQCode = row["RegisterQuotationCode"].ToString();
                        p.AddedDate = Convert.ToDateTime(row["AddedDate"].ToString());
                        p.AddedDateString = p.AddedDate.ToString("dd-MM-yyyy");
                        p.VenderName = row["VenderName"].ToString();
                        p.CompanyName = row["CompanyName"].ToString();
                        decimal totalAmount;
                        if (decimal.TryParse(row["TotalAmount"].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out totalAmount))
                            p.TotalAmount = totalAmount;
                        else
                            p.TotalAmount = 0;

                        lst.Add(p);
                    }
                }

                return lst;
            }
            catch (Exception ex)
            {
                throw new Exception("Error in ShowApprovedPRPRK", ex);
            }
        }


        /// <summary>
        /// This Function Show PO  List
        /// </summary>
        /// <returns>Approved PO  List</returns>
        public async Task<List<Purchase>> ShowApprovePOPRK(DateTime startDate, DateTime endDate)
        {
            try
            {
                List<Purchase> lst = new List<Purchase>();
                Dictionary<string, string> paradic = new Dictionary<string, string> 
                { 
                    { "@Flag", "ApprovePODashPRK" },
                     { "@StartDate", startDate.ToString("yyyy-MM-dd") },
            { "@EndDate", endDate.ToString("yyyy-MM-dd") }
                };
                DataSet ds = await obj.ExecuteStoredProcedureReturnDS("PurchaseProcedure", paradic);

                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        Purchase p = new Purchase();

                        p.POCode = row["POCode"].ToString();
                        p.AddedDate = Convert.ToDateTime(row["PODate"].ToString());
                        p.RQCode = row["RegisterQuotationCode"].ToString();
                        p.ApprovedRejectedDate = Convert.ToDateTime(row["ApprovedRejectedDate"].ToString());
                        p.ApprovedRejectedDateString = p.ApprovedRejectedDate.ToString("dd-MM-yyyy");
                        p.AddedDateString = p.AddedDate.ToString("dd-MM-yyyy");
                        p.TotalAmount = Convert.ToDecimal(row["POCost"].ToString());
                        p.FullName = row["CreatedBy"].ToString();
                        p.Address = row["BillingAddress"].ToString();

                        lst.Add(p);


                    }
                }

                return lst;
            }
            catch (Exception ex)
            {
                throw new Exception("Error in ShowApprovedPRPRK", ex);
            }
        }


        /// <summary>
        /// This Function Show PO  List
        /// </summary>
        /// <returns>Pending PO  List</returns>
        public async Task<List<Purchase>> ShowPendingPOPRK(DateTime startDate, DateTime endDate)
        {
            try
            {
                List<Purchase> lst = new List<Purchase>();
                Dictionary<string, string> paradic = new Dictionary<string, string> 
                {
                    { "@Flag", "PendingPODashPRK" },
                     { "@StartDate", startDate.ToString("yyyy-MM-dd") },
            { "@EndDate", endDate.ToString("yyyy-MM-dd") }
                };
                DataSet ds = await obj.ExecuteStoredProcedureReturnDS("PurchaseProcedure", paradic);

                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        Purchase p = new Purchase();

                        p.POCode = row["POCode"].ToString();
                        p.AddedDate = Convert.ToDateTime(row["PODate"].ToString());
                        p.AddedDateString = p.AddedDate.ToString("dd-MM-yyyy");
                        p.TotalAmount = Convert.ToDecimal(row["POCost"].ToString());
                        p.FullName = row["CreatedBy"].ToString();

                        lst.Add(p);

                    }
                }


                return lst;
            }
            catch (Exception ex)
            {
                throw new Exception("Error in ShowApprovedPRPRK", ex);
            }
        }

        #endregion

        #region Sandesh

        /// <summary>
        /// Fetch all approved pr list 
        /// </summary>
        /// <returns>
        /// Return PR list
        /// </returns>
        public async Task<DataSet> AllQuationListSJ()
        {
            Dictionary<string, string> para = new Dictionary<string, string>();
            para.Add("@Flag", "AllQListSJ");
            DataSet ds = await obj.ExecuteStoredProcedureReturnDS("PurchaseProcedure", para);
            return ds;
        }

        /// <summary>
        /// Fetch the Perticular PR's Details
        /// </summary>
        /// <returns>
        /// Returns List of PR items
        /// </returns>
        public async Task<DataSet> PRSJ(string id)
        {
            //var RFQ = GenerateNextRFQCode();
            Dictionary<string, string> para = new Dictionary<string, string>();
            para.Add("@Flag", "PRSJ");
            para.Add("@PRCode", id.ToString());

            DataSet ds = await obj.ExecuteStoredProcedureReturnDS("PurchaseProcedure", para);
            return ds;
        }

        /// <summary>
        /// Genrate new RFQ code
        /// </summary>
        /// <returns>
        /// New RFQ code
        /// </returns>
        //public async Task<string> GenerateNextRFQCodeSJ()
        //{
        //    var para = new Dictionary<string, string>
        //    {
        //        { "@Flag", "GenerateNextRFQCodeSJ" }
        //    };

        //    DataSet ds = await obj.ExecuteStoredProcedureReturnDS("PurchaseProcedure", para);

        //    if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
        //    {
        //        string lastCode = ds.Tables[0].Rows[0]["RFQCode"].ToString();
        //        int number = int.Parse(lastCode.Substring(3));
        //        string nextCode = "RFQ" + (number + 1).ToString("D3");
        //        return nextCode;
        //    }
        //    else
        //    {
        //        return "RFQ001";
        //    }
        //}

        public async Task<string> GenerateNextRFQCodeSJ()
        {
            var para = new Dictionary<string, string>
            {
                ["@Flag"] = "GenerateNextRFQCodeSJ"
            };

            DataSet ds = await obj.ExecuteStoredProcedureReturnDS("PurchaseProcedure", para);

            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                string lastCode = ds.Tables[0].Rows[0]["RFQCode"].ToString();
                if (!string.IsNullOrEmpty(lastCode) && lastCode.Length > 3)
                {
                    int number = int.Parse(lastCode.Substring(3));
                    string nextCode = "RFQ" + (number + 1).ToString("D3");
                    return nextCode;
                }
            }

            return "RFQ001"; // first code if table empty
        }



        /// <summary>
        /// Fetch Contact persons information
        /// </summary>
        /// <returns>
        /// Name email contact no
        /// </returns>
        public async Task<DataSet> GetContactPersonsSJ()
        {
            var para = new Dictionary<string, string>
        {
        { "@Flag", "ContactPersonsSJ" }
        };

            return await obj.ExecuteStoredProcedureReturnDS("PurchasePRocedure", para);
        }


        /// <summary>
        /// Fetch the Warehouse 
        /// </summary>
        /// <returns>
        /// list of warehouse
        /// </returns>
        public async Task<DataSet> GetWareHouseSJ()
        {
            var para = new Dictionary<string, string>
        {
        { "@Flag", "GetwarehouseSJ" }
        };

            return await obj.ExecuteStoredProcedureReturnDS("PurchasePRocedure", para);
        }

        /// <summary>
        /// Save the new RFQ which is created from PR
        /// </summary>
        //public async Task<int> SaveRFQSJ(Purchase model)
        //{
        //    var para = new Dictionary<string, string>
        //        {
        //           { "@Flag", "SaveRFQSJ" },
        //           { "@RFQCode", model.RFQCode },
        //           { "@ContactPersonId", model.ContactPerson },
        //           { "@PRCode", model.PRCode },
        //           { "@ExpectedDate", model.ExpectedDate.ToString()},
        //           { "@WarehouseCode", model.Warehouse },
        //           { "@Note", model.Note ?? "" },
        //        };

        //    DataSet ds = await obj.ExecuteStoredProcedureReturnDS("PurchaseProcedure", para);
        //    return (ds != null) ? 1 : 0;

        //}

        public async Task<int> SaveRFQSJ(Purchase model, string staffCode, string addedDate)
        {
            var para = new Dictionary<string, string>
            {
                ["@Flag"] = "SaveRFQSJ",
                ["@RFQCode"] = model.RFQCode ?? "",
                ["@ContactPersonId"] = model.ContactPerson ?? "",
                ["@PRCode"] = model.PRCode ?? "",
                ["@ExpectedDate"] = model.ExpectedDate.ToString("yyyy-MM-dd"),
                ["@WarehouseCode"] = model.Warehouse ?? "",
                ["@Note"] = model.Note ?? "",
                ["@StaffCode"] = staffCode ?? "",   // ✅ Passed as string
                ["@AddedDate"] = addedDate ?? ""    // ✅ Passed as string
            };

            DataSet ds = await obj.ExecuteStoredProcedureReturnDS("PurchaseProcedure", para);
            return (ds != null) ? 1 : 0;
        }



        /// <summary>
        /// Fetch the All RFQ's list
        /// </summary>
        /// <returns>
        /// returns the list of All QFQ's
        /// </returns>
        public async Task<DataSet> ALLRFQLISTSJ()
        {
            var para = new Dictionary<string, string>
        {
        { "@Flag", "AllRFQsSJ" }
        };
            return await obj.ExecuteStoredProcedureReturnDS("PurchaseProcedure", para);
        }


        /// <summary>
        /// Fetch perticuler RFQ' Details
        /// </summary>
        /// <returns>
        /// Perticuler RFQ's item list
        /// </returns>
        public async Task<DataSet> GetRFQDetailsSJ(string id)
        {
            Dictionary<string, string> para = new Dictionary<string, string>();
            para.Add("@Flag", "RFQDetailsSJ");
            para.Add("@RFQCode", id.ToString());

            DataSet ds = await obj.ExecuteStoredProcedureReturnDS("PurchaseProcedure", para);
            return ds;
        }


        /// <summary>
        /// Fetch the All venders
        /// </summary>
        public async Task<DataSet> GetVendorsSJ(int? industryTypeId = null)
        {
            var para = new Dictionary<string, string>
        {
        { "@Flag", "GetVendorsSJ" }
        };

            if (industryTypeId.HasValue)
            {
                para.Add("@IndustryTypeId", industryTypeId.Value.ToString());
            }

            return await obj.ExecuteStoredProcedureReturnDS("PurchaseProcedure", para);
        }


        /// <summary>
        /// Fetch the Industry Type
        /// </summary>
        public async Task<DataSet> GetIndustryTypesSJ()
        {
            var para = new Dictionary<string, string>
        {
        { "@Flag", "GetIndustryTypesSJ" }
        };

            return await obj.ExecuteStoredProcedureReturnDS("PurchaseProcedure", para);
        }

        /// <summary>
        /// Fetch Venders by ID for send mail
        /// </summary>
        /// <returns>
        /// vendors information
        /// </returns>
        public async Task<DataSet> GetVendorsByIdsSJ(string vendorIdsCsv)
        {
            Dictionary<string, string> para = new Dictionary<string, string>();
            para.Add("@Flag", "GetVendorsByIdsSJ");
            para.Add("@VendorIds", vendorIdsCsv); // comma-separated IDs

            DataSet ds = await obj.ExecuteStoredProcedureReturnDS("PurchaseProcedure", para);
            return ds;
        }

        /// <summary>
        /// Fetch Industry type for vender registration
        /// </summary>
        /// <returns>
        /// Industry types list 
        /// </returns>
        public async Task<DataSet> FetchIndustryTypeSJ()
        {
            Dictionary<string, string> para = new Dictionary<string, string>();
            para.Add("@Flag", "FetchIndustryTypeSJ");
            DataSet ds = await obj.ExecuteStoredProcedureReturnDS("PurchaseProcedure", para);
            return ds;
        }

        /// <summary>
        /// Fetch the swift code
        /// </summary>
        /// <returns>
        /// returns swift code
        /// </returns>
        public async Task<DataSet> FetchBankAndSwiftCodeSJ()
        {
            Dictionary<string, string> para = new Dictionary<string, string>();
            para.Add("@Flag", "FetchBankAndSwiftCodeSJ");
            DataSet ds = await obj.ExecuteStoredProcedureReturnDS("PurchaseProcedure", para);
            return ds;
        }

        /// <summary>
        /// Fetch the bank branch address
        /// </summary>
        /// <returns>
        /// returns the bank branch 
        /// </returns>
        public async Task<DataSet> FetchBranchAndIFSCCodeSJ(int BankId)
        {
            Dictionary<string, string> para = new Dictionary<string, string>();
            para.Add("@Flag", "FetchBranchAndIFSCCodeSJ");
            para.Add("@BankId", BankId.ToString());
            DataSet ds = await obj.ExecuteStoredProcedureReturnDS("PurchaseProcedure", para);
            return ds;
        }

        /// <summary>
        /// Genrate the new vendor code
        /// </summary>
        /// <returns>
        /// new vedor code
        /// </returns>
        public async Task<DataSet> FetchVendorandVendorCompantMaxIdSJ()
        {
            Dictionary<string, string> para = new Dictionary<string, string>();
            para.Add("@Flag", "FetchVendorandVendorCompantMaxIdSJ");
            DataSet ds = await obj.ExecuteStoredProcedureReturnDS("PurchaseProcedure", para);
            return ds;
        }

        /// <summary>
        /// Saves the venders information
        /// </summary>
        public async Task<bool> SaveVendorSJ(Purchase p)
        {
            try
            {
                Dictionary<string, string> para = new Dictionary<string, string>();

                para.Add("@Flag", "InsertVendorSJ");


                // Vendor parameters
                para.Add("@VendorCode", p.VendorCode);
                para.Add("@VenderName", p.VendorName);
                para.Add("@VendorMobileNo", p.MobileNo.ToString());
                para.Add("@VendorAlternateNo", p.AlternateNo.ToString());
                para.Add("@VendorEmail", p.Email);
                para.Add("@VendorAddress", p.Address);
                para.Add("@StaffCode", p.StaffCode ?? "");
                para.Add("@AddedDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));  // formatted string

                // VendorCompany parameters
                para.Add("@VendorCompanyCode", p.VendorCompanyCode);
                para.Add("@CompanyName", p.CompanyName);
                para.Add("@CompanyMobileNo", p.CompanyMobileNo.ToString());
                para.Add("@CompanyAlternateNo", p.CompanyAlternateNo.ToString());
                para.Add("@CompanyEmail", p.CompanyEmail);
                para.Add("@CompanyAddress", p.CompanyAddress);
                para.Add("@IndustryTypeId", p.IndustryTypeId.ToString());
                para.Add("@CityId", p.CityId.ToString());

                // Vendor Account parameters
                para.Add("@BranchId", p.BranchId.ToString());
                para.Add("@UserCode", p.VendorCode);
                para.Add("@AccountNumber", p.AccountNumber.ToString());


                //  para.Add("@AddedBy", "STF008");

                // Execute SP
                await obj.ExecuteStoredProcedure("PurchaseProcedure", para);
                return true;
            }
            catch (Exception ex)
            {
                return false;
                throw new Exception(ex.Message);
            }
        }


        /// <summary>
        /// Save the RFQ and vendors code 
        /// </summary>
        public async Task<int> SaveRFQVendorsSJ(Purchase model, string staffCode, string addedDate)
        {
            if (model == null || model.Vendors == null || model.Vendors.Count == 0)
                return 0;

            int rowsAffected = 0;

            foreach (var vendorId in model.Vendors)
            {
                var para = new Dictionary<string, string>
        {
            { "@Flag", "SaveRFQVendorSJ" },
            { "@RFQCode", model.RFQCode },
            { "@VendorCode", vendorId.ToString() },
            { "@StaffCode", staffCode ?? "" }, 
            { "@AddedDate", addedDate ?? "" }    
        };

                DataSet ds = await obj.ExecuteStoredProcedureReturnDS("PurchaseProcedure", para);

                if (ds != null)
                    rowsAffected++;
            }

            return rowsAffected;
        }


        #endregion
    }
}
