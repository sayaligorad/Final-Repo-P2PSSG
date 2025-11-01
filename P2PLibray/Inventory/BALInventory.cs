using P2PHelper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static P2PLibray.Inventory.Fetch;
using static P2PLibray.Inventory.Inventory;

namespace P2PLibray.Inventory
{
    public class BALInventory
    {
        MSSQL obj = new MSSQL();


        #region Rutik
        /// <summary>
        /// Gets total, low, and most stock counts from inventory.
        /// </summary>
        /// <param name="fromDate">Optional start date filter</param>
        /// <param name="toDate">Optional end date filter</param>
        /// <param name="category">Optional category filter</param>
        /// <returns>InventoryStock model with counts</returns>
        public async Task<InventoryStock> GetInventoryStockCountHSB(DateTime? fromDate, DateTime? toDate, string category)
        {
            InventoryStock model = new InventoryStock();

            // ----------------- Total Stock -----------------
            Dictionary<string, string> TotalStock = new Dictionary<string, string>
            {
                { "@Flag", "TotalItemsInInventoryHSB" }
            };
            if (fromDate.HasValue) TotalStock.Add("@fromDate", fromDate.Value.ToString("yyyy-MM-dd"));
            if (toDate.HasValue) TotalStock.Add("@toDate", toDate.Value.ToString("yyyy-MM-dd"));
            if (!string.IsNullOrEmpty(category)) TotalStock.Add("@category", category);

            SqlDataReader drTotal = await obj.ExecuteStoredProcedureReturnDataReader("InventoryProcedure", TotalStock);

            if (drTotal.HasRows && await drTotal.ReadAsync())
            {
                model.TotalCount = Convert.ToInt32(drTotal["TotalItemsInInventory"]);
            }
            drTotal.Close();

            // ----------------- Low Stock -----------------
            Dictionary<string, string> LowStock = new Dictionary<string, string>
            {
                { "@Flag", "LowInStocksHSB" }
            };
            if (fromDate.HasValue) LowStock.Add("@fromDate", fromDate.Value.ToString("yyyy-MM-dd"));
            if (toDate.HasValue) LowStock.Add("@toDate", toDate.Value.ToString("yyyy-MM-dd"));
            if (!string.IsNullOrEmpty(category)) LowStock.Add("@category", category);

            SqlDataReader drLow = await obj.ExecuteStoredProcedureReturnDataReader("InventoryProcedure", LowStock);
            if (drLow.HasRows && await drLow.ReadAsync())
            {
                model.LowInStocks = Convert.ToInt32(drLow["LowInStocks"]);
            }
            drLow.Close();

            // ----------------- Most Stock -----------------
            Dictionary<string, string> MostStock = new Dictionary<string, string>
            {
                { "@Flag", "MostInStocksHSB" }
            };
            if (fromDate.HasValue) MostStock.Add("@fromDate", fromDate.Value.ToString("yyyy-MM-dd"));
            if (toDate.HasValue) MostStock.Add("@toDate", toDate.Value.ToString("yyyy-MM-dd"));
            if (!string.IsNullOrEmpty(category)) MostStock.Add("@category", category);

            SqlDataReader drMostStock = await obj.ExecuteStoredProcedureReturnDataReader("InventoryProcedure", MostStock);
            if (drMostStock.HasRows && await drMostStock.ReadAsync())
            {
                model.MostInStocks = Convert.ToInt32(drMostStock["MostInStocks"]);
            }
            drMostStock.Close();

            return model;
        }

        /// <summary>
        /// Gets inventory counts grouped by category (Finished Goods, Raw Material, Dead Stock).
        /// </summary>
        public async Task<InventoryCategory> GetCategoryHSB(DateTime? fromDate, DateTime? toDate, string category)
        {
            InventoryCategory model = new InventoryCategory();

            // ----------------- Finished Goods -----------------
            Dictionary<string, string> finishedGoodsParam = new Dictionary<string, string>
            {
                { "@Flag", "FinishedGoodsHSB" }
            };
            if (fromDate.HasValue) finishedGoodsParam.Add("@fromDate", fromDate.Value.ToString("yyyy-MM-dd"));
            if (toDate.HasValue) finishedGoodsParam.Add("@toDate", toDate.Value.ToString("yyyy-MM-dd"));
            if (!string.IsNullOrEmpty(category)) finishedGoodsParam.Add("@category", category);

            SqlDataReader drFinishedGoods = await obj.ExecuteStoredProcedureReturnDataReader("InventoryProcedure", finishedGoodsParam);
            if (drFinishedGoods.HasRows && await drFinishedGoods.ReadAsync())
            {
                model.FinishedGoods = Convert.ToInt32(drFinishedGoods["FinishedGoods"]);
            }
            drFinishedGoods.Close();

            // ----------------- SemiFinished Goods -----------------
            Dictionary<string, string> semifinishedGoodsParam = new Dictionary<string, string>
            {
                { "@Flag", "SemiFinishedGoodsHSB" }
            };
            if (fromDate.HasValue) semifinishedGoodsParam.Add("@fromDate", fromDate.Value.ToString("yyyy-MM-dd"));
            if (toDate.HasValue) semifinishedGoodsParam.Add("@toDate", toDate.Value.ToString("yyyy-MM-dd"));
            if (!string.IsNullOrEmpty(category)) semifinishedGoodsParam.Add("@category", category);

            SqlDataReader drsemiFinishedGoods = await obj.ExecuteStoredProcedureReturnDataReader("InventoryProcedure", semifinishedGoodsParam);
            if (drsemiFinishedGoods.HasRows && await drsemiFinishedGoods.ReadAsync())
            {
                model.SemiFinishedGoods = Convert.ToInt32(drsemiFinishedGoods["SemiFinished"]);
            }
            drsemiFinishedGoods.Close();

            // ----------------- Raw Material -----------------
            Dictionary<string, string> rawMaterialParam = new Dictionary<string, string>
            {
                { "@Flag", "RawMaterialHSB" }
            };
            if (fromDate.HasValue) rawMaterialParam.Add("@fromDate", fromDate.Value.ToString("yyyy-MM-dd"));
            if (toDate.HasValue) rawMaterialParam.Add("@toDate", toDate.Value.ToString("yyyy-MM-dd"));
            if (!string.IsNullOrEmpty(category)) rawMaterialParam.Add("@category", category);

            SqlDataReader drRawMaterial = await obj.ExecuteStoredProcedureReturnDataReader("InventoryProcedure", rawMaterialParam);
            if (drRawMaterial.HasRows && await drRawMaterial.ReadAsync())
            {
                model.RawMaterial = Convert.ToInt32(drRawMaterial["RawMaterial"]);
            }
            drRawMaterial.Close();

            // ----------------- Dead Stock -----------------
            Dictionary<string, string> deadStockParam = new Dictionary<string, string>
            {
                { "@Flag", "DeadStockHSB" }
            };
            if (fromDate.HasValue) deadStockParam.Add("@fromDate", fromDate.Value.ToString("yyyy-MM-dd"));
            if (toDate.HasValue) deadStockParam.Add("@toDate", toDate.Value.ToString("yyyy-MM-dd"));
            if (!string.IsNullOrEmpty(category)) deadStockParam.Add("@category", category);

            SqlDataReader drDeadStock = await obj.ExecuteStoredProcedureReturnDataReader("InventoryProcedure", deadStockParam);
            if (drDeadStock.HasRows && await drDeadStock.ReadAsync())
            {
                model.DeadStock = Convert.ToInt32(drDeadStock["DeadStock"]);
            }
            drDeadStock.Close();

            return model;
        }

        /// <summary>
        /// Gets list of received items over time.
        /// </summary>
        public async Task<List<ReciveItemsCountClass>> ReciveItemsCountHSB(DateTime? fromDate, DateTime? toDate, string category)
        {
            List<ReciveItemsCountClass> ReciveItemsList = new List<ReciveItemsCountClass>();

            Dictionary<string, string> param = new Dictionary<string, string>
            {
                { "@Flag", "ItemsReceivedOverTime" }
            };
            if (fromDate.HasValue) param.Add("@fromDate", fromDate.Value.ToString("yyyy-MM-dd"));
            if (toDate.HasValue) param.Add("@toDate", toDate.Value.ToString("yyyy-MM-dd"));
            if (!string.IsNullOrEmpty(category)) param.Add("@category", category);

            SqlDataReader dr = await obj.ExecuteStoredProcedureReturnDataReader("InventoryProcedure", param);

            if (dr.HasRows)
            {
                while (await dr.ReadAsync())
                {
                    ReciveItemsList.Add(new ReciveItemsCountClass
                    {
                        Received = Convert.ToInt32(dr["Received"]),
                        POCode = dr["POCode"].ToString(),
                        POQuantity = Convert.ToInt32(dr["POQuantity"]),
                        ReceiveDate = Convert.ToDateTime(dr["ReceiveDate"]).ToString("dd-MMM-yyyy")
                    });
                }
            }

            dr.Close();
            return ReciveItemsList;
        }

        /// <summary>
        /// Gets issued items in-house count grouped by date.
        /// </summary>
        public async Task<List<IssueInHouseCountClass>> getIssueInHouseCounttHSB(DateTime? fromDate, DateTime? toDate, string category)
        {
            Dictionary<string, string> param = new Dictionary<string, string>
            {
                { "@Flag", "IssueInHouseHSB" }
            };
            if (fromDate.HasValue) param.Add("@fromDate", fromDate.Value.ToString("yyyy-MM-dd"));
            if (toDate.HasValue) param.Add("@toDate", toDate.Value.ToString("yyyy-MM-dd"));
            if (!string.IsNullOrEmpty(category)) param.Add("@category", category);

            SqlDataReader dr = await obj.ExecuteStoredProcedureReturnDataReader("InventoryProcedure", param);

            List<IssueInHouseCountClass> IssueInhouseItemList = new List<IssueInHouseCountClass>();

            if (dr.HasRows)
            {
                while (await dr.ReadAsync())
                {
                    IssueInhouseItemList.Add(new IssueInHouseCountClass
                    {
                        Issue = Convert.ToInt32(dr["Issue"]),
                        IssueMonth = Convert.ToDateTime(dr["IssueMonth"]).ToString("dd-MMM-yyyy")
                    });
                }
            }

            dr.Close();
            return IssueInhouseItemList;
        }

        /// <summary>
        /// Gets grouped stock details (total, low, most).
        /// </summary>
        public async Task<InventoryStockGrouped> GetInventoryStockDetailsHSB(DateTime? fromDate, DateTime? toDate, string category)
        {
            InventoryStockGrouped grouped = new InventoryStockGrouped();

            // ----------------- Total Stock Details -----------------
            Dictionary<string, string> TotalStock = new Dictionary<string, string>
            {
                { "@Flag", "TotalItemsDetailsHSB" }
            };
            if (fromDate.HasValue) TotalStock.Add("@fromDate", fromDate.Value.ToString("yyyy-MM-dd"));
            if (toDate.HasValue) TotalStock.Add("@toDate", toDate.Value.ToString("yyyy-MM-dd"));
            if (!string.IsNullOrEmpty(category)) TotalStock.Add("@category", category);

            SqlDataReader drTotal = await obj.ExecuteStoredProcedureReturnDataReader("InventoryProcedure", TotalStock);
            while (await drTotal.ReadAsync())
            {
                grouped.TotalStock.Add(new InventoryStockDetails
                {
                    ItemsCounts = Convert.ToInt32(drTotal["ItemsCounts"]),
                    ReorderQuantity = Convert.ToInt32(drTotal["ReorderQuantity"]),
                    ItemCode = drTotal["ItemCode"].ToString(),
                    ItemName = drTotal["ItemName"].ToString(),
                    BinCode = drTotal["BinCodes"].ToString()
                });
            }
            drTotal.Close();

            // ----------------- Low Stock Details -----------------
            Dictionary<string, string> LowStock = new Dictionary<string, string>
            {
                { "@Flag", "LowInStocksDetailsHSB" }
            };
            if (fromDate.HasValue) LowStock.Add("@fromDate", fromDate.Value.ToString("yyyy-MM-dd"));
            if (toDate.HasValue) LowStock.Add("@toDate", toDate.Value.ToString("yyyy-MM-dd"));
            if (!string.IsNullOrEmpty(category)) LowStock.Add("@category", category);

            SqlDataReader drLow = await obj.ExecuteStoredProcedureReturnDataReader("InventoryProcedure", LowStock);
            while (await drLow.ReadAsync())
            {
                grouped.LowStock.Add(new InventoryStockDetails
                {
                    ItemsCounts = Convert.ToInt32(drLow["ItemsCounts"]),
                    ReorderQuantity = Convert.ToInt32(drLow["ReorderQuantity"]),
                    ItemCode = drLow["ItemCode"].ToString(),
                    ItemName = drLow["ItemName"].ToString(),
                    BinCode = drLow["BinCodes"].ToString()
                });
            }
            drLow.Close();

            // ----------------- Most Stock Details -----------------
            Dictionary<string, string> MostStock = new Dictionary<string, string>
            {
                { "@Flag", "MostInStocksDetailsHSB" }
            };
            if (fromDate.HasValue) MostStock.Add("@fromDate", fromDate.Value.ToString("yyyy-MM-dd"));
            if (toDate.HasValue) MostStock.Add("@toDate", toDate.Value.ToString("yyyy-MM-dd"));
            if (!string.IsNullOrEmpty(category)) MostStock.Add("@category", category);

            SqlDataReader drMostStock = await obj.ExecuteStoredProcedureReturnDataReader("InventoryProcedure", MostStock);
            while (await drMostStock.ReadAsync())
            {
                grouped.MostStock.Add(new InventoryStockDetails
                {
                    ItemsCounts = Convert.ToInt32(drMostStock["ItemsCounts"]),
                    ReorderQuantity = Convert.ToInt32(drMostStock["ReorderQuantity"]),
                    ItemCode = drMostStock["ItemCode"].ToString(),
                    ItemName = drMostStock["ItemName"].ToString(),
                    BinCode = drMostStock["BinCodes"].ToString()
                });
            }
            drMostStock.Close();

            return grouped;
        }

        /// <summary>
        /// Gets inventory stock grouped by categories with details.
        /// </summary>
        public async Task<InventoryCategoryGrouped> GetCategoryDetailsHSB(DateTime? fromDate, DateTime? toDate, string category)
        {
            InventoryCategoryGrouped grouped = new InventoryCategoryGrouped();

            // ----------------- Finished Goods -----------------
            Dictionary<string, string> finishedGoodsParam = new Dictionary<string, string>
            {
                { "@Flag", "FinishedGoodsDetailsHSB" }
            };
            if (fromDate.HasValue) finishedGoodsParam.Add("@fromDate", fromDate.Value.ToString("yyyy-MM-dd"));
            if (toDate.HasValue) finishedGoodsParam.Add("@toDate", toDate.Value.ToString("yyyy-MM-dd"));
            if (!string.IsNullOrEmpty(category)) finishedGoodsParam.Add("@category", category);

            SqlDataReader drFinished = await obj.ExecuteStoredProcedureReturnDataReader("InventoryProcedure", finishedGoodsParam);
            while (await drFinished.ReadAsync())
            {
                grouped.FinishedGoods.Add(new InventoryStockDetails
                {
                    QuantityStored = Convert.ToInt32(drFinished["QuantityStored"]),
                    ItemCode = drFinished["ItemCode"].ToString(),
                    ItemName = drFinished["ItemName"].ToString(),
                    CreatedDate = Convert.ToDateTime(drFinished["CreatedDate"]).ToString("dd-MMM-yyyy")
                });
            }
            drFinished.Close();

            // ----------------- Finished Goods -----------------
            Dictionary<string, string> SemifinishedGoodsParam = new Dictionary<string, string>
            {
                { "@Flag", "SemiFinishedGoodsDetailsHSB" }
            };
            if (fromDate.HasValue) SemifinishedGoodsParam.Add("@fromDate", fromDate.Value.ToString("yyyy-MM-dd"));
            if (toDate.HasValue) SemifinishedGoodsParam.Add("@toDate", toDate.Value.ToString("yyyy-MM-dd"));
            if (!string.IsNullOrEmpty(category)) SemifinishedGoodsParam.Add("@category", category);

            SqlDataReader drSemiFinished = await obj.ExecuteStoredProcedureReturnDataReader("InventoryProcedure", SemifinishedGoodsParam);
            while (await drSemiFinished.ReadAsync())
            {
                grouped.SemiFinishedGoods.Add(new InventoryStockDetails
                {
                    QuantityStored = Convert.ToInt32(drSemiFinished["QuantityStored"]),
                    ItemCode = drSemiFinished["ItemCode"].ToString(),
                    ItemName = drSemiFinished["ItemName"].ToString(),
                    CreatedDate = Convert.ToDateTime(drSemiFinished["CreatedDate"]).ToString("dd-MMM-yyyy")
                });
            }
            drSemiFinished.Close();

            // ----------------- Raw Material -----------------
            Dictionary<string, string> rawMaterialParam = new Dictionary<string, string>
            {
                { "@Flag", "RawMaterialDetailsHSB" }
            };
            if (fromDate.HasValue) rawMaterialParam.Add("@fromDate", fromDate.Value.ToString("yyyy-MM-dd"));
            if (toDate.HasValue) rawMaterialParam.Add("@toDate", toDate.Value.ToString("yyyy-MM-dd"));
            if (!string.IsNullOrEmpty(category)) rawMaterialParam.Add("@category", category);

            SqlDataReader drRaw = await obj.ExecuteStoredProcedureReturnDataReader("InventoryProcedure", rawMaterialParam);
            while (await drRaw.ReadAsync())
            {
                grouped.RawMaterial.Add(new InventoryStockDetails
                {
                    QuantityStored = Convert.ToInt32(drRaw["QuantityStored"]),
                    ItemCode = drRaw["ItemCode"].ToString(),
                    ItemName = drRaw["ItemName"].ToString(),
                    CreatedDate = Convert.ToDateTime(drRaw["CreatedDate"]).ToString("dd-MMM-yyyy")
                });
            }
            drRaw.Close();

            // ----------------- Dead Stock -----------------
            Dictionary<string, string> deadStockParam = new Dictionary<string, string>
            {
                { "@Flag", "DeadStockDetailsHSB" }
            };
            if (fromDate.HasValue) deadStockParam.Add("@fromDate", fromDate.Value.ToString("yyyy-MM-dd"));
            if (toDate.HasValue) deadStockParam.Add("@toDate", toDate.Value.ToString("yyyy-MM-dd"));
            if (!string.IsNullOrEmpty(category)) deadStockParam.Add("@category", category);

            SqlDataReader drDead = await obj.ExecuteStoredProcedureReturnDataReader("InventoryProcedure", deadStockParam);
            while (await drDead.ReadAsync())
            {
                grouped.DeadStock.Add(new InventoryStockDetails
                {
                    QuantityStored = Convert.ToInt32(drDead["QuantityStored"]),
                    ItemCode = drDead["ItemCode"].ToString(),
                    ItemName = drDead["ItemName"].ToString(),
                    CreatedDate = Convert.ToDateTime(drDead["CreatedDate"]).ToString("dd-MMM-yyyy")
                });
            }
            drDead.Close();

            return grouped;
        }

        /// <summary>
        /// Gets detailed list of received materials.
        /// </summary>
        public async Task<List<ReceiveMaterialDetail>> GetReceiveMaterialDetails(DateTime? fromDate, DateTime? toDate, string category)
        {
            List<ReceiveMaterialDetail> detailsList = new List<ReceiveMaterialDetail>();

            Dictionary<string, string> param = new Dictionary<string, string>
            {
                { "@Flag", "ItemsReceivedOverTimeDetails" }
            };
            if (fromDate.HasValue) param.Add("@fromDate", fromDate.Value.ToString("yyyy-MM-dd"));
            if (toDate.HasValue) param.Add("@toDate", toDate.Value.ToString("yyyy-MM-dd"));
            if (!string.IsNullOrEmpty(category)) param.Add("@category", category);

            SqlDataReader dr = await obj.ExecuteStoredProcedureReturnDataReader("InventoryProcedure", param);

            if (dr.HasRows)
            {
                while (await dr.ReadAsync())
                {
                    detailsList.Add(new ReceiveMaterialDetail
                    {
                        GRNItemCode = dr["GRNItemCode"].ToString(),
                        ItemsCounts = Convert.ToInt32(dr["ItemsCounts"]),
                        ItemName = dr["ItemName"].ToString(),
                        AddedDate = Convert.ToDateTime(dr["AddedDate"]).ToString("dd-MMM-yyyy")
                    });
                }
            }

            dr.Close();
            return detailsList;
        }

        /// <summary>
        /// Gets detailed list of in-house issued items.
        /// </summary>
        public async Task<List<IssueInHouseDetail>> GetIssueInHouseDetails(DateTime? fromDate, DateTime? toDate, string category)
        {
            List<IssueInHouseDetail> detailsList = new List<IssueInHouseDetail>();

            Dictionary<string, string> param = new Dictionary<string, string>
            {
                { "@Flag", "IssueInHouseDetailsHSB" }
            };
            if (fromDate.HasValue) param.Add("@fromDate", fromDate.Value.ToString("yyyy-MM-dd"));
            if (toDate.HasValue) param.Add("@toDate", toDate.Value.ToString("yyyy-MM-dd"));
            if (!string.IsNullOrEmpty(category)) param.Add("@category", category);

            SqlDataReader dr = await obj.ExecuteStoredProcedureReturnDataReader("InventoryProcedure", param);

            if (dr.HasRows)
            {
                while (await dr.ReadAsync())
                {
                    detailsList.Add(new IssueInHouseDetail
                    {
                        ItemsCounts = Convert.ToInt32(dr["ItemsCounts"]),
                        ItemName = dr["ItemName"].ToString(),
                        AddedDate = Convert.ToDateTime(dr["AddedDate"]).ToString("dd-MMM-yyyy")
                    });
                }
            }

            dr.Close();
            return detailsList;
        }

        /// <summary>
        /// Gets total counts of bins, warehouses, and racks.
        /// </summary>
        public async Task<InventoryStorage> GetTotalBinHSB()
        {
            InventoryStorage model = new InventoryStorage();

            // ----------------- Bin Count -----------------
            Dictionary<string, string> TotalBin = new Dictionary<string, string>
            {
                { "@Flag", "TotalBinCountHSB" }
            };

            SqlDataReader drTotal = await obj.ExecuteStoredProcedureReturnDataReader("InventoryProcedure", TotalBin);

            if (drTotal.HasRows && await drTotal.ReadAsync())
            {
                model.Bin = Convert.ToInt32(drTotal["Bin"]);
            }
            drTotal.Close();

            // ----------------- Warehouse Count -----------------
            Dictionary<string, string> WareHouse = new Dictionary<string, string>
            {
                { "@Flag", "TotalWareHouseCountHSB" }
            };

            SqlDataReader drLow = await obj.ExecuteStoredProcedureReturnDataReader("InventoryProcedure", WareHouse);
            if (drLow.HasRows && await drLow.ReadAsync())
            {
                model.WareHouse = Convert.ToInt32(drLow["WareHouse"]);
            }
            drLow.Close();

            // ----------------- Rack Count -----------------
            Dictionary<string, string> Rack = new Dictionary<string, string>
            {
                { "@Flag", "TotalRackCountHSB" }
            };

            SqlDataReader drMostStock = await obj.ExecuteStoredProcedureReturnDataReader("InventoryProcedure", Rack);
            if (drMostStock.HasRows && await drMostStock.ReadAsync())
            {
                model.Rack = Convert.ToInt32(drMostStock["Rack"]);
            }
            drMostStock.Close();

            return model;
        }
        #endregion

        #region Divyani
        /// <summary>
        /// Gets received material details using stored procedure.
        /// Returns a SqlDataReader with received material data.
        /// </summary>
        public async Task<SqlDataReader> GetReceiveMaterialDRB()
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("@Flag", "GetReceiveMaterialDRB");
            SqlDataReader dr = await obj.ExecuteStoredProcedureReturnDataReader("InventoryProcedure", param);
            return dr;
        }

        /// <summary>
        /// Gets issue code details using stored procedure.
        /// Returns a SqlDataReader with issue code data.
        /// </summary>
        public async Task<SqlDataReader> GetIssueCodeDRB()
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("@Flag", "GetIssueCodeDRB");
            SqlDataReader dr = await obj.ExecuteStoredProcedureReturnDataReader("InventoryProcedure", param);
            return dr;
        }

        public async Task<List<InventoryBinDRB>> GetBins(string itemcode)
        {
            List<InventoryBinDRB> inventoryBinDRBs = new List<InventoryBinDRB>();
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("@Flag", "GetBinsDRB");
            param.Add("@ItemCode", itemcode);
            SqlDataReader dr = await obj.ExecuteStoredProcedureReturnDataReader("InventoryProcedure", param);
            if (dr != null)
            {
                while (dr.Read())
                {
                    InventoryBinDRB obj =  new InventoryBinDRB
                    {
                        BinCode = dr["BinCode"].ToString(),
                        BinName = dr["BinName"].ToString(),
                        CurrentItems = dr["QuantityStored"].ToString()

                    };

                    inventoryBinDRBs.Add(obj);
                }

                return inventoryBinDRBs;

            }
            else
            {
                return null;
            }
            
        }

        /// <summary>
        /// Gets list of departments from database.
        /// Returns a DataSet containing department data.
        /// </summary>
        public async Task<DataSet> GetDepartmentsDRB()
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("@Flag", "GetDepartmentsDRB");
            DataSet ds = await obj.ExecuteStoredProcedureReturnDS("InventoryProcedure", param);
            return ds;
        }

        /// <summary>
        /// Gets instock items from database.
        /// Returns a DataSet containing instock items.
        /// </summary>
        public async Task<DataSet> GetInstockItemsDRB()
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("@Flag", "GetInstockItemsDRB");
            DataSet ds = await obj.ExecuteStoredProcedureReturnDS("InventoryProcedure", param);
            return ds;
        }

        /// <summary>
        /// Gets employees by department id.
        /// Returns a DataSet containing employees for given department.
        /// </summary>
        public async Task<DataSet> GetEmployeeDRB(int Did)
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("@Flag", "GetEmployeeDRB");
            param.Add("@DepartmentId", Did.ToString());
            DataSet ds = await obj.ExecuteStoredProcedureReturnDS("InventoryProcedure", param);
            return ds;
        }

        /// <summary>
        /// Gets list of warehouses.
        /// Returns List of InventoryWareHouseDRB (Code, Name).
        /// </summary>
        public async Task<List<InventoryWareHouseDRB>> GetWareHouseDRB()
        {
            Dictionary<string, string> param = new Dictionary<string, string>
                {
                    { "@Flag", "GetWareHouseDRB" },
                };

            DataSet ds = await obj.ExecuteStoredProcedureReturnDS("InventoryProcedure", param);

            List<InventoryWareHouseDRB> grnItemsList = new List<InventoryWareHouseDRB>();

            if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    grnItemsList.Add(new InventoryWareHouseDRB
                    {
                        WareHouseCode = dr["WareHouseCode"]?.ToString(),
                        WareHouseName = dr["WarehouseName"]?.ToString(),
                    });
                }
            }

            return grnItemsList;
        }

        /// <summary>
        /// Gets sections by warehouse code.
        /// Returns List of InventorySectionDRB (Code, Name).
        /// </summary>
        public async Task<List<InventorySectionDRB>> GetSectionDRB(string code)
        {
            Dictionary<string, string> param = new Dictionary<string, string>
                {
                    { "@Flag", "GetSectionDRB" },
                    { "@WareHouseCode", code }
                };

            DataSet ds = await obj.ExecuteStoredProcedureReturnDS("InventoryProcedure", param);

            List<InventorySectionDRB> grnItemsList = new List<InventorySectionDRB>();

            if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    grnItemsList.Add(new InventorySectionDRB
                    {
                        SectionCode = dr["SectionCode"]?.ToString(),
                        SectionName = dr["SectionName"]?.ToString(),
                    });
                }
            }

            return grnItemsList;
        }

        /// <summary>
        /// Gets racks by section code.
        /// Returns List of InventoryRackDRB (Code, Name).
        /// </summary>
        public async Task<List<InventoryRackDRB>> GetRackDRB(string code)
        {
            Dictionary<string, string> param = new Dictionary<string, string>
                {
                    { "@Flag", "GetRackDRB" },
                    { "@SectionCode", code }
                };

            DataSet ds = await obj.ExecuteStoredProcedureReturnDS("InventoryProcedure", param);

            List<InventoryRackDRB> grnItemsList = new List<InventoryRackDRB>();

            if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    grnItemsList.Add(new InventoryRackDRB
                    {
                        RackCode = dr["RackCode"]?.ToString(),
                        RackName = dr["RackName"]?.ToString(),
                    });
                }
            }

            return grnItemsList;
        }

        /// <summary>
        /// Gets rows by rack code.
        /// Returns List of InventoryRowDRB (Code, Name).
        /// </summary>
        public async Task<List<InventoryRowDRB>> GetRowDRB(string code)
        {
            Dictionary<string, string> param = new Dictionary<string, string>
                {
                    { "@Flag", "GetRowDRB" },
                    { "@RackCode", code }
                };

            DataSet ds = await obj.ExecuteStoredProcedureReturnDS("InventoryProcedure", param);

            List<InventoryRowDRB> grnItemsList = new List<InventoryRowDRB>();

            if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    grnItemsList.Add(new InventoryRowDRB
                    {
                        RowCode = dr["RowCode"]?.ToString(),
                        RowName = dr["RowName"]?.ToString(),
                    });
                }
            }

            return grnItemsList;
        }

        /// <summary>
        /// Gets bins by row code.
        /// Returns List of InventoryBinDRB (Code, Name, CurrentItems, MaxQuantity).
        /// </summary>
        public async Task<List<InventoryBinDRB>> GetBinDRB(string code,string GrnItemCode)
        {
            Dictionary<string, string> param = new Dictionary<string, string>
                {
                    { "@Flag", "GetBinDRB" },
                    { "@RowCode", code },
                    { "@GRNItemCode", GrnItemCode },
                };

            DataSet ds = await obj.ExecuteStoredProcedureReturnDS("InventoryProcedure", param);

            List<InventoryBinDRB> grnItemsList = new List<InventoryBinDRB>();

            if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    grnItemsList.Add(new InventoryBinDRB
                    {
                        BinCode = dr["BinCode"]?.ToString(),
                        BinName = dr["BinName"]?.ToString(),
                        CurrentItems = dr["CurrentItems"]?.ToString(),
                        MaxQuantity = dr["MaxQuantity"]?.ToString(),
                    });
                }
            }

            return grnItemsList;
        }

        /// <summary>
        /// Gets in-house issued items by status.
        /// Returns List of IssueinHouseDRB with item details.
        /// </summary>
        public async Task<List<IssueinHouseDRB>> IssueINHouseDRB(int StatusId)
        {
            Dictionary<string, string> param = new Dictionary<string, string>
                {
                    { "@Flag", "IssueINHouseDRB" },
                    { "@StatusId", StatusId.ToString() }
                };

            DataSet ds = await obj.ExecuteStoredProcedureReturnDS("InventoryProcedure", param);

            List<IssueinHouseDRB> grnItemsList = new List<IssueinHouseDRB>();

            if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    grnItemsList.Add(new IssueinHouseDRB
                    {
                        ItemCode = dr["ItemCode"]?.ToString(),
                        ItemName = dr["ItemName"]?.ToString(),
                        ItemCategoryName = dr["ItemCategoryName"]?.ToString(),
                        Quantity = dr["Quantity"] != DBNull.Value ? Convert.ToDecimal(dr["Quantity"]) : 0,
                        Price = dr["Price"] != DBNull.Value ? Convert.ToDecimal(dr["Price"]) : 0,
                        Amount = dr["Amount"] != DBNull.Value ? Convert.ToDecimal(dr["Amount"]) : 0,
                        StatusName = dr["StatusName"]?.ToString()
                    });
                }
            }

            return grnItemsList;
        }

        /// <summary>
        /// Gets GRN details by GRNCode.
        /// Returns GRN header and items as GRNdetailsDRB.
        /// </summary>
        public async Task<GRNdetailsDRB> GetGRNDetailsDRB(string GRNCode)
        {
            Dictionary<string, string> param = new Dictionary<string, string>
                {
                     { "@Flag", "GetGRNDetailsDRB" },
                     { "@GRNCode", GRNCode }
                };

            DataSet ds = await obj.ExecuteStoredProcedureReturnDS("InventoryProcedure", param);

            GRNdetailsDRB details = new GRNdetailsDRB();
            details.Items = new List<GRNItemDRB>();

            if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                DataRow dr = ds.Tables[0].Rows[0];

                details.GRNCode = dr["GRNCode"]?.ToString();
                details.GRNDate = dr["GRNDate"] != DBNull.Value ? Convert.ToDateTime(dr["GRNDate"]) : DateTime.MinValue;
                details.POReference = dr["POAddedByName"]?.ToString();
                details.InvoiceCode = dr["InvoiceCode"]?.ToString();
                details.InvoiceDate = dr["InvoiceDate"] != DBNull.Value ? Convert.ToDateTime(dr["InvoiceDate"]) : DateTime.MinValue;
                details.VendorCode = dr["VendorCode"]?.ToString();
                details.VendorName = dr["VendorName"]?.ToString();
                details.WareHouseCode = dr["WareHouseCode"]?.ToString();
                details.WarehouseName = dr["WarehouseName"]?.ToString();
                details.ContactNo = dr["ContactNo"]?.ToString();
            }

            if (ds.Tables.Count > 1)
            {
                foreach (DataRow itemRow in ds.Tables[1].Rows)
                {
                    details.Items.Add(new GRNItemDRB
                    {
                        ItemCode = itemRow["ItemCode"]?.ToString(),
                        ItemName = itemRow["ItemName"]?.ToString(),
                        Description = itemRow["Description"]?.ToString(),
                        UOMName = itemRow["UOMName"]?.ToString(),
                        Quantity = itemRow["Quantity"] != DBNull.Value ? Convert.ToDecimal(itemRow["Quantity"]) : 0,
                        Price = itemRow["Price"] != DBNull.Value ? Convert.ToDecimal(itemRow["Price"]) : 0
                    });
                }
            }

            return details;
        }

        /// <summary>
        /// Gets in-stock details by GRNCode.
        /// Returns GRN header and stored items as GRNdetailsDRB.
        /// </summary>
        public async Task<GRNdetailsDRB> GetInStockDRB(string GRNCode)
        {
            Dictionary<string, string> param = new Dictionary<string, string>
                {
                    { "@Flag", "GetInStockDRB" },
                    { "@GRNCode", GRNCode }
                };

            DataSet ds = await obj.ExecuteStoredProcedureReturnDS("InventoryProcedure", param);

            GRNdetailsDRB details = new GRNdetailsDRB();
            details.Items = new List<GRNItemDRB>();

            if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                DataRow dr = ds.Tables[0].Rows[0];

                details.GRNCode = dr["GRNCode"]?.ToString();
                details.GRNDate = dr["GRNDate"] != DBNull.Value ? Convert.ToDateTime(dr["GRNDate"]) : DateTime.MinValue;
                details.POReference = dr["POAddedByName"]?.ToString();
                details.InvoiceCode = dr["InvoiceCode"]?.ToString();
                details.InvoiceDate = dr["InvoiceDate"] != DBNull.Value ? Convert.ToDateTime(dr["InvoiceDate"]) : DateTime.MinValue;
                details.VendorCode = dr["VendorCode"]?.ToString();
                details.VendorName = dr["VendorName"]?.ToString();
                details.WareHouseCode = dr["WareHouseCode"]?.ToString();
                details.WarehouseName = dr["WarehouseName"]?.ToString();
                details.ContactNo = dr["ContactNo"]?.ToString();
            }

            if (ds.Tables.Count > 1)
            {
                foreach (DataRow itemRow in ds.Tables[1].Rows)
                {
                    details.Items.Add(new GRNItemDRB
                    {
                        ItemCode = itemRow["GRNItemcode"]?.ToString(),
                        ItemName = itemRow["ItemName"]?.ToString(),
                        ItemCategoryName = itemRow["ItemCategoryName"]?.ToString(),
                        Description = itemRow["Description"]?.ToString(),
                        UOMName = itemRow["UOMName"]?.ToString(),
                        Quantity = itemRow["Quantity"] != DBNull.Value ? Convert.ToDecimal(itemRow["Quantity"]) : 0,
                        QuantityStored = itemRow["QuantityStored"] != DBNull.Value ? Convert.ToDecimal(itemRow["QuantityStored"]) : 0,
                        Price = itemRow["Price"] != DBNull.Value ? Convert.ToDecimal(itemRow["Price"]) : 0
                    });
                }
            }

            return details;
        }

        /// <summary>
        /// Saves GRN Item Bin assignment to database.
        /// </summary>
        public async Task SaveGRHSBItemBinDRB(SaveItemBinAssignmentDRB model)
        {
            Dictionary<String, String> SaveGR = new Dictionary<String, String>();
            SaveGR.Add("@Flag", "SaveGRHSBItemBinDRB");
            SaveGR.Add("@GRNItemCode", model.GRNItemCode);
            SaveGR.Add("@BinCode", model.BinCode);
            SaveGR.Add("@QuantityStored", model.QuantityStored);
            SaveGR.Add("@CreatedDate", model.CreatedDate);

            await obj.ExecuteStoredProcedure("InventoryProcedure", SaveGR);
        }

        /// <summary>
        /// Saves Issue header details to database.
        /// </summary>
        public async Task SaveIssueHeaderModelDRB(IssueHeaderModelDRB model)
        {
            Dictionary<String, String> SaveGR = new Dictionary<String, String>();
            SaveGR.Add("@Flag", "SaveIssueHeaderModelDRB");
            SaveGR.Add("@IssueCode", model.IssueCode);
            SaveGR.Add("@StaffCode", model.StaffCode);
            SaveGR.Add("@AddedDate", model.IssueDate);
            SaveGR.Add("@AddedBy", "STF004");
            //SaveGR.Add("@StatusId", "29");

            await obj.ExecuteStoredProcedure("InventoryProcedure", SaveGR);
        }

        /// <summary>
        /// Saves Issue detail line items to database.
        /// </summary>
        public async Task SaveIssueDetailDRB(IssueDetailModelDRB model)
        {
            Dictionary<String, String> SaveGR = new Dictionary<String, String>();
            SaveGR.Add("@Flag", "SaveIssueDetailDRB");
            SaveGR.Add("@IssueCode", model.IssueCode);
            SaveGR.Add("@ItemCode", model.ItemCode);
            SaveGR.Add("@BinCode", model.Bincode);
            SaveGR.Add("@Quantity", model.Quantity);

            await obj.ExecuteStoredProcedure("InventoryProcedure", SaveGR);
        }
        #endregion

        #region Lavmesh
        /* Get All Current Stock */
        public async Task<List<InventoryLM>> ShowCurrentStockLM()
        {
            List<InventoryLM> currentStockList = new List<InventoryLM>();

            var param = new Dictionary<string, string>
    {
        { "@Flag", "ShowCurrentStockLM" }
    };

            SqlDataReader dr = await obj.ExecuteStoredProcedureReturnDataReader("InventoryProcedure", param);

            if (dr.HasRows)
            {
                while (await dr.ReadAsync())
                {
                    currentStockList.Add(new InventoryLM
                    {
                        ItemCode = dr["ItemCode"].ToString(),
                        ItemName = dr["ItemName"].ToString(),
                        CurrentQty = Convert.ToInt32(dr["ItemsCounts"]),
                        ReorderQuantity = Convert.ToInt32(dr["ReorderQuantity"]),
                        BinsList = dr["Bins"].ToString(),
                        BinName = dr["BinList"].ToString()
                    });
                }
            }

            dr.Close();
            return currentStockList;
        }

        // 🔹 Show Non-Moving Stock
        public async Task<List<InventoryLM>> ShowNonMovingStockLM()
        {
            List<InventoryLM> nonMovingStocks = new List<InventoryLM>();
            var param = new Dictionary<string, string>
    {
        { "@Flag", "ShowNonMovingStockLM" },
        { "@CurrentDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") }  //  pass current date
    };
            SqlDataReader dr = await obj.ExecuteStoredProcedureReturnDataReader("InventoryProcedure", param);

            if (dr.HasRows)
            {
                while (await dr.ReadAsync())
                {
                    nonMovingStocks.Add(new InventoryLM
                    {
                        ItemCode = dr["ItemCode"].ToString(),
                        ItemName = dr["ItemName"].ToString(),
                        CurrentQty = Convert.ToInt32(dr["TotalQty"]),
                        AddedDate = dr["AddedDate"] != DBNull.Value ? Convert.ToDateTime(dr["AddedDate"]).ToString("dd/MM/yyyy") : string.Empty,
                        ExpiryDate = dr["ExpiryDate"] != DBNull.Value ? Convert.ToDateTime(dr["ExpiryDate"]).ToString("dd/MM/yyyy") : string.Empty,
                        BinName = dr["BinList"].ToString(),
                        BinCode = dr["BinCodeList"].ToString()
                    });
                }
            }

            dr.Close();
            return nonMovingStocks;
        }

        // Transfer stock with Bin match
        public bool TransferNonMovingToMovingLM(string itemCode, int transferQty, string binCode, string connStr)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connStr))
                using (SqlCommand cmd = new SqlCommand("InventoryProcedure", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Flag", SqlDbType.NVarChar, 50).Value = "TransferNonMovingToMovingLM";
                    cmd.Parameters.Add("@ItemCode", SqlDbType.NVarChar, 50).Value = itemCode;
                    cmd.Parameters.Add("@TransferQty", SqlDbType.Int).Value = transferQty;
                    cmd.Parameters.Add("@BinCode", SqlDbType.NVarChar, 50).Value = binCode;

                    con.Open();
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        /* Get All QCStock */
        public async Task<List<InventoryLM>> ShowQCSTockLM()
        {
            List<InventoryLM> qc = new List<InventoryLM>();
            var param = new Dictionary<string, string>
            {
                {"@Flag","ShowQualityCheckStockLM" }
            };
            SqlDataReader dr = await obj.ExecuteStoredProcedureReturnDataReader("InventoryProcedure", param);
            if (dr.HasRows)
            {
                while (await dr.ReadAsync())
                {
                    qc.Add(new InventoryLM
                    {
                        QualityCheackCode = dr["QualityCheckCode"].ToString(),
                        ItemName = dr["ItemName"].ToString(),
                        ItemCount = Convert.ToInt32(dr["TotalItems"]),
                        Date = dr["AddedDate"] != DBNull.Value
                        ? Convert.ToDateTime(dr["AddedDate"]).ToString("dd/MM/yyyy")
                        : string.Empty,
                        Status = dr["StatusName"].ToString()
                    });
                }
            }
            dr.Close();
            return qc;
        }

        #endregion lavmesh

        #region Rushikesh
        // =====================================================================

        /// <summary>
        /// Retrieves all requirement master records from the database
        /// </summary>
        /// <returns>DataSet containing requirement master records</returns>
        public async Task<DataSet> ReqMasterRHK()
        {
            // Create parameter dictionary for stored procedure
            Dictionary<string, string> para = new Dictionary<string, string>();
            para.Add("@Flag", "RequirementMasterRHK"); // Flag to identify which operation to execute

            // Execute stored procedure and return results
            DataSet ds = await obj.ExecuteStoredProcedureReturnDS("InventoryProcedure", para);
            return ds;
        }

        /// <summary>
        /// Retrieves detailed view of a specific requirement master record
        /// </summary>
        /// <param name="id">The unique identifier of the requirement to view</param>
        /// <returns>DataSet containing detailed requirement information</returns>
        public async Task<DataSet> ViewReqMasterRHK()
        {
            // Create parameter dictionary for stored procedure
            Dictionary<string, string> para = new Dictionary<string, string>();
            para.Add("@Flag", "ViewRequirementMasterRHK"); // Flag for detailed view operation
          

            // Execute stored procedure and return results
            DataSet ds = await obj.ExecuteStoredProcedureReturnDS("InventoryProcedure", para);
            return ds;
        }

        // =====================================================================
        // STOCK MASTER METHODS
        // Handles operations related to material requirement planning
        // =====================================================================

        /// <summary>
        /// Retrieves all stock master records from the database
        /// </summary>
        /// <returns>DataSet containing stock master records</returns>
        public async Task<DataSet> StockMasterRHK()
        {
            // Create parameter dictionary for stored procedure
            Dictionary<string, string> para = new Dictionary<string, string>();
            para.Add("@Flag", "StockMasterRHK"); // Flag to identify stock master operation

            // Execute stored procedure and return results
            DataSet ds = await obj.ExecuteStoredProcedureReturnDS("InventoryProcedure", para);
            return ds;
        }

        /// <summary>
        /// Retrieves detailed view of a specific material requirement plan
        /// </summary>
        /// <param name="plancode">The unique code identifying the material requirement plan</param>
        /// <returns>DataSet containing detailed plan information</returns>
        public async Task<DataSet> ViewPlanRHK(string plancode)
        {
            // Create parameter dictionary for stored procedure
            Dictionary<string, string> para = new Dictionary<string, string>();
            para.Add("@Flag", "ViewStockMasterRHK"); // Flag for detailed plan view operation
            para.Add("@plancode", plancode); // Specific plan code to retrieve

            // Execute stored procedure and return results
            DataSet ds = await obj.ExecuteStoredProcedureReturnDS("InventoryProcedure", para);
            return ds;
        }

    #endregion Rushikesh

        #region Akash
     /// <summary>
        /// Gets the current stock report from the inventory.
        /// </summary>
        /// <returns>List of <see cref="StockReport"/> containing stock details.</returns>
        public async Task<List<StockReport>> GetCurrentStockAMG()
        {
            var stockList = new List<StockReport>();

            try
            {
                var param = new Dictionary<string, object>
                {
                    { "@Flag", "CurrentStockReportsAMG" }
                };

                using (SqlDataReader dr = await obj.ExecuteStoredProcedureReturnDataReaderObject("InventoryProcedure", param))
                {
                    DataTable dt = new DataTable();
                    dt.Load(dr);

                    foreach (DataRow row in dt.Rows)
                    {
                        var stock = new StockReport
                        {
                            ItemCode = row["ItemCode"].ToString(),
                            ItemName = row["ItemName"].ToString(),
                            ItemCategoryName = row["ItemCategoryName"].ToString(),
                            UnitRates = Convert.ToDecimal(row["UnitRates"]),
                            CurrentQuantity = Convert.ToInt32(row["CurrentQuantity"]),
                            MinQuantity = Convert.ToInt32(row["MinQuantity"]),
                            ReorderQuantity = Convert.ToInt32(row["ReorderQuantity"]),
                            StockStatus = row["StockStatus"].ToString()
                        };
                        stockList.Add(stock);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log ex here
                throw new Exception("Error while fetching Current Stock Report.", ex);
            }

            return stockList;
        }

        /// <summary>
        /// Gets the received material report between the given dates.
        /// </summary>
        /// <param name="fromDate">Start date filter.</param>
        /// <param name="toDate">End date filter.</param>
        /// <returns>List of <see cref="ReceivedMaterialReport"/> containing received material details.</returns>
        public async Task<List<ReceivedMaterialReport>> GetReceivedMaterialReportAMG(DateTime? fromDate, DateTime? toDate)
        {
            var list = new List<ReceivedMaterialReport>();

            try
            {
                var param = new Dictionary<string, object>
                {
                    {"@Flag","ReceivedMaterialReportAMG"},
                    {"@FromDate", fromDate.HasValue ? (object)fromDate.Value.Date : DBNull.Value},
                    {"@ToDate", toDate.HasValue ? (object)toDate.Value.Date : DBNull.Value}
                };

                using (SqlDataReader dr = await obj.ExecuteStoredProcedureReturnDataReaderObject("InventoryProcedure", param))
                {
                    DataTable dt = new DataTable();
                    dt.Load(dr);

                    list = dt.AsEnumerable().Select(r => new ReceivedMaterialReport
                    {
                        ReceiveMaterialId = r.Field<int>("ReceiveMaterialId"),
                        ReceivedDate = r.Field<DateTime>("ReceivedDate").ToString("yyyy-MM-dd"),
                        GRNCode = r["GRNCode"]?.ToString(),
                        POCode = r["POCode"]?.ToString(),
                        ItemCode = r["ItemCode"]?.ToString(),
                        ItemCategory = r["ItemCategoryName"]?.ToString(),
                        ItemName = r["ItemName"]?.ToString(),
                        GRNQuantity = r["GRNQuantity"] != DBNull.Value ? Convert.ToInt64(r["GRNQuantity"]) : 0,
                        ReceivedQuantity = r["ReceivedQuantity"] != DBNull.Value ? Convert.ToInt64(r["ReceivedQuantity"]) : 0,
                        WarehouseName = r["WarehouseName"]?.ToString(),
                        BinCode = string.IsNullOrEmpty(r["BinCode"]?.ToString()) ? "Bin not assigned yet" : r["BinCode"].ToString()
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                // Log ex here
                throw new Exception("Error while fetching Received Material Report.", ex);
            }

            return list;
        }

        /// <summary>
        /// Gets the inhouse transfer report between the given dates.
        /// </summary>
        /// <param name="fromDate">Start date filter.</param>
        /// <param name="toDate">End date filter.</param>
        /// <returns>List of <see cref="InhouseTransferReport"/> containing transfer details.</returns>
        public async Task<List<InhouseTransferReport>> GetInhouseTransferReportAMG(DateTime? fromDate, DateTime? toDate)
        {
            var list = new List<InhouseTransferReport>();

            try
            {
                var param = new Dictionary<string, object>
                {
                    { "@Flag", "TransferedInhouseReportAMG" },
                    { "@FromDate", fromDate.HasValue ? (object)fromDate.Value.Date : DBNull.Value },
                    { "@ToDate", toDate.HasValue ? (object)toDate.Value.Date : DBNull.Value }
                };

                using (SqlDataReader dr = await obj.ExecuteStoredProcedureReturnDataReaderObject("InventoryProcedure", param))
                {
                    DataTable dt = new DataTable();
                    dt.Load(dr);

                    list = dt.AsEnumerable().Select(r => new InhouseTransferReport
                    {
                        InhouseCode = r["InhouseCode"]?.ToString(),
                        TransferTo = r["TransferTo"]?.ToString(),
                        ItemName = r["ItemName"]?.ToString(),
                        ItemCategory = r["ItemCategory"]?.ToString(),
                        UOM = r["UOM"]?.ToString(),
                        Quantity = r["Quantity"] != DBNull.Value ? Convert.ToInt32(r["Quantity"]) : 0,
                        TransferDate = r.Field<DateTime>("TransferDate").ToString("yyyy-MM-dd")
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                // Log ex here
                throw new Exception("Error while fetching Inhouse Transfer Report.", ex);
            }

            return list;
        }
        #endregion

        #region Saurabh
        /// <summary>
        /// WareHouise List
        /// </summary>
        /// <returns></returns>

        //  Get Warehouse List
        public async Task<List<Inventory>> GetWarehousesAsyncSK()
        {
            List<Inventory> warehouses = new List<Inventory>();
            var parameters = new Dictionary<string, string>
        {
            { "@Flag", "WarehousesSK" }
        };

            DataSet ds = await obj.ExecuteStoredProcedureReturnDS("InventoryProcedure", parameters);

            if (ds.Tables.Count > 0)
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    warehouses.Add(new Inventory
                    {
                        WareHouseId = Convert.ToInt32(row["WareHouseId"]),
                        WarehouseCode = row["WareHouseCode"].ToString(),
                        WarehouseName = row["WarehouseName"].ToString(),
                        Address = row["Address"].ToString(),
                        AddedDate = Convert.ToDateTime(row["AddedDate"]),
                        AddedBy = row["AddedBy"].ToString(),
                        Phone = row["Phone"].ToString(),
                        Email = row["Email"].ToString(),
                        Description = row["Description"].ToString(),
                        Capacity = Convert.ToInt32(row["Capacity"]),
                        StateCode = row["StateCode"].ToString(),
                        CountryCode = row["CountryCode"].ToString(),
                        CityId = row["Cityid"] != DBNull.Value ? Convert.ToInt32(row["Cityid"]) : 0,
                    });
                }
            }

            return warehouses;
        }

        /// <summary>
        /// Warehouse By Id Using View And Edit 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>

        // ================= Get warehouse by ID =================
        public async Task<Inventory> GetWarehouseByIdAsyncSK(int id)
        {
            var ds = await obj.ExecuteStoredProcedureReturnDS("InventoryProcedure",
                new Dictionary<string, string> {
            { "@Flag", "GetWarehouseByIdSK" },
            { "@WareHouseId", id.ToString() }
                });

            if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                var row = ds.Tables[0].Rows[0];
                return new Inventory
                {
                    WareHouseId = Convert.ToInt32(row["WareHouseId"]),
                    WarehouseCode = row["WarehouseCode"].ToString(),
                    WarehouseName = row["WarehouseName"].ToString(),
                    Address = row["Address"].ToString(),

                    CountryCode = row["CountryCode"].ToString(),
                    StateCode = row["StateCode"].ToString(),
                    CityId = row["CityId"] != DBNull.Value ? Convert.ToInt32(row["CityId"]) : 0,


                    CountryName = string.Empty,
                    StateName = string.Empty,
                    CityName = string.Empty,

                    Phone = row["Phone"].ToString(),
                    Email = row["Email"].ToString(),
                    AddedBy = row["AddedBy"].ToString(),
                    Description = row["Description"].ToString(),
                    Capacity = row["Capacity"] != DBNull.Value ? Convert.ToInt32(row["Capacity"]) : 0,
                    AddedDate = row["AddedDate"] != DBNull.Value ? Convert.ToDateTime(row["AddedDate"]) : DateTime.MinValue
                };

            }
            return null;
        }

        /// <summary>
        /// Save Warehouse 
        /// </summary>
        /// <param name="warehouse"></param>
        /// <returns></returns>

        //  Add Warehouse

        public async Task<(bool Success, string Message, int NewId)> AddWarehouseAsyncSK(InventorySK warehouse)
        {
            if (warehouse == null) return (false, "Warehouse data cannot be null.", 0);

            if (string.IsNullOrWhiteSpace(warehouse.WarehouseCode)) return (false, "Warehouse Code is required.", 0);
            if (string.IsNullOrWhiteSpace(warehouse.WarehouseName)) return (false, "Warehouse Name is required.", 0);
            if (warehouse.CityId <= 0) return (false, "Invalid City selected.", 0);


            try
            {
                var parameters = new Dictionary<string, string>
    {
        { "@Flag", "AddWarehouseSK" },
        { "@WarehouseCode", warehouse.WarehouseCode },
        { "@WarehouseName", warehouse.WarehouseName },
        { "@Address", warehouse.Address ?? "" },
        { "@CityId", warehouse.CityId.ToString() },
        { "@AddedBy", warehouse.AddedBy },
        { "@Phone", warehouse.Phone ?? "" },
        { "@Email", warehouse.Email ?? "" },
        { "@Description", warehouse.Description ?? "" },
        { "@Capacity", warehouse.Capacity.ToString() },
                { "@StateCode", warehouse.StateCode.ToString() },
                {"@CountryCode", warehouse.CountryCode.ToString() },
                {"@AddedDate", warehouse.AddedDate.ToString("yyyy-MM-dd")  }

    };

                object result = await obj.ExecuteStoredProcedureReturnObject("InventoryProcedure", parameters);
                int newId = result != null ? Convert.ToInt32(result) : 0;

				return newId > 0
	? (true, "Warehouse added successfully.", newId)
	: (false, "Warehouse name already exists.", 0);
			}
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}", 0);
            }
        }


		/// <summary>
		/// Update Warehouse 
		/// </summary>
		/// <param name="Update warehouse"></param>
		/// <returns></returns>
		//  Update Warehouse
		public async Task<(bool Success, string Message)> UpdateWarehouseAsyncSK(InventorySK warehouse)
		{
			try
			{
				var parameters = new Dictionary<string, string>
		{
			{ "@Flag", "UpdateWarehouseSK" },
			{ "@WareHouseId", warehouse.WareHouseId.ToString() },
			{ "@WarehouseName", warehouse.WarehouseName },
			{ "@Address", warehouse.Address ?? "" },
			{ "@CityId", warehouse.CityId.ToString() },
			{ "@Phone", warehouse.Phone ?? "" },
			{ "@Email", warehouse.Email ?? "" },
			{ "@Description", warehouse.Description ?? "" },
			{ "@Capacity", warehouse.Capacity.ToString() },
			{ "@StateCode", warehouse.StateCode ?? "" },
			{ "@CountryCode", warehouse.CountryCode ?? "" }
		};

				DataSet ds = await obj.ExecuteStoredProcedureReturnDS("InventoryProcedure", parameters);

				if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
				{
					string result = ds.Tables[0].Rows[0]["Result"].ToString();
					string message = ds.Tables[0].Rows[0]["Message"].ToString();

					return (result == "1", message);
				}

				return (false, "Failed to update warehouse.");
			}
			catch (Exception ex)
			{
				return (false, ex.Message);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name=" delete warehouseId"></param>
		/// <returns></returns>
		// Delete Warehouse
		public async Task DeleteWarehouseAsyncSK(int warehouseId)
        {
            var parameters = new Dictionary<string, string>
{
    { "@Flag", "DeleteWarehouseSK" },
    { "@WareHouseId", warehouseId.ToString() }
};

            await obj.ExecuteStoredProcedure("InventoryProcedure", parameters);
        }

        /// <summary>
        /// Next Warehouse Code 
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetNextWarehouseCodeAsyncSK()
        {
            Dictionary<string, string> para = new Dictionary<string, string>();
            para.Add("@Flag", "WarehousesCodeSK");

            DataSet ds = await obj.ExecuteStoredProcedureReturnDS("InventoryProcedure", para);

            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                return ds.Tables[0].Rows[0]["NextCode"].ToString();
            }

            return "WHS0001";
        }
        /// <summary>
        /// This is Using By Country State City 
        /// </summary>
        /// <returns></returns>

        //------------------------------------------------------------------------------------------------COUNTRY STATE CITY--------------------------------------------------------------- 

        public async Task<List<Inventory>> GetCountries()
        {
            Dictionary<string, string> para = new Dictionary<string, string>();
            para.Add("@Flag", "DrpCountry");

            var ds = await obj.ExecuteStoredProcedureReturnDS("InventoryProcedure", para);

            return ds.Tables[0].AsEnumerable().Select(r => new Inventory
            {
                CountryId = r.Field<int>("CountryID"),
                CountryName = r.Field<string>("CountryName")
            }).ToList();
        }

        public async Task<List<Inventory>> GetStates(int countryId)
        {
            Dictionary<string, string> para = new Dictionary<string, string>();
            para.Add("@Flag", "DrpState");
            para.Add("@CountryId", countryId.ToString());

            var ds = await obj.ExecuteStoredProcedureReturnDS("InventoryProcedure", para);

            return ds.Tables[0].AsEnumerable().Select(r => new Inventory
            {
                StateId = r.Field<int>("StateID"),
                StateName = r.Field<string>("StateName")
            }).ToList();
        }

        public async Task<List<Inventory>> GetCities(int stateId)
        {
            Dictionary<string, string> para = new Dictionary<string, string>();
            para.Add("@Flag", "DrpCity");
            para.Add("@StateId", stateId.ToString());

            var ds = await obj.ExecuteStoredProcedureReturnDS("InventoryProcedure", para);

            return ds.Tables[0].AsEnumerable().Select(r => new Inventory
            {
                CityId = r.Field<int>("CityID"),
                CityName = r.Field<string>("City_Name")
            }).ToList();
        }


        //_______________________________________________________________________________BINS BAL ____________________________________________________________________________________









        /// <summary>
        /// BINS LIST
        /// </summary>
        /// <returns></returns>

        //  Get Racks List
        public async Task<List<Inventory>> GetRacksAsyncSK()
        {
            List<Inventory> racks = new List<Inventory>();
            var parameters = new Dictionary<string, string>
        {
            { "@Flag", "RacksSK" }
        };

            DataSet ds = await obj.ExecuteStoredProcedureReturnDS("InventoryProcedure", parameters);

            if (ds.Tables.Count > 0)
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    racks.Add(new Inventory
                    {
                        RackId = Convert.ToInt32(row["RackId"]),
                        RackCode = row["RackCode"].ToString(),
                        RackName = row["RackName"].ToString(),
                        SectionName = row["SectionName"].ToString(),
                        WarehouseName = row["WarehouseName"].ToString(),
                        AddedDate = Convert.ToDateTime(row["AddedDate"]),
                        AddedBy = row["AddedBy"].ToString(),

                        Description = row["Description"].ToString(),

                    });
                }
            }

            return racks;
        }
        /// <summary>
        /// WAREHOUSE LIST
        /// </summary>
        /// <returns></returns>
        //   WareHouse List
        public async Task<List<Inventory>> GetWarehouseslistSK()
        {
            List<Inventory> warehouses = new List<Inventory>();

            var parameters = new Dictionary<string, string>
{
    { "@Flag", "DRPWarehouseSK" }
};

            DataSet ds = await obj.ExecuteStoredProcedureReturnDS("InventoryProcedure", parameters);

            if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    warehouses.Add(new Inventory
                    {
                        WarehouseCode = row["WareHouseCode"].ToString(),
                        WarehouseName = row["WarehouseName"].ToString()
                    });
                }
            }

            return warehouses;
        }
        /// <summary>
        /// SECTION LIST By Using warehouse 
        /// </summary>
        /// <param name="warehouseCode"></param>
        /// <returns></returns>

        //  Section List By Warehouse
        public async Task<List<Inventory>> GetSectionsByWarehouseAsyncSK(string warehouseCode)
        {
            List<Inventory> sections = new List<Inventory>();

            var parameters = new Dictionary<string, string>
{
    { "@Flag", "DRPSectionSK" },
    { "@warehouseCode", warehouseCode }
};

            DataSet ds = await obj.ExecuteStoredProcedureReturnDS("InventoryProcedure", parameters);

            if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    sections.Add(new Inventory
                    {
                        SectionCode = row["SectionCode"].ToString(),
                        SectionName = row["SectionName"].ToString()
                    });
                }
            }

            return sections;
        }
        /// <summary>
        /// Next RAck CODE
        /// </summary>
        /// <returns></returns>
        //  Next rack Code
        public async Task<string> GetNextRackCodeAsyncSK()
        {
            string rackCode = "";

            var parameters = new Dictionary<string, string>
{
    { "@Flag", "RackCodeSK" }
};

            DataSet ds = await obj.ExecuteStoredProcedureReturnDS("InventoryProcedure", parameters);

            if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                rackCode = ds.Tables[0].Rows[0]["NextCode"].ToString();
            }

            return rackCode;
        }


		/// <summary>
		/// Save RACK AND UPDATE 
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>

		//  Save Rack (Insert/Update)
		public async Task<(bool Success, string Message)> SaveRackAsyncSK(InventorySK model)
		{
			var parameters = new Dictionary<string, string>
	{
		{ "@Flag", "SaveRackSK" },
		{ "@RackId", model.RackId.ToString() },
		{ "@RackCode", model.RackCode ?? "" },
		{ "@RackName", model.RackName ?? "" },
		{ "@SectionCode", model.SectionCode ?? "" },
		{ "@WareHouseCode", model.WarehouseCode ?? "" },
		{ "@Description", model.Description ?? "" },
		{ "@AddedBy", model.AddedBy ?? "" },
		{ "@AddedDate", model.AddedDate.ToString("yyyy-MM-dd") }
	};

			DataSet ds = await obj.ExecuteStoredProcedureReturnDS("InventoryProcedure", parameters);

			if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
			{
				var row = ds.Tables[0].Rows[0];
				return (Convert.ToBoolean(row["Success"]), row["Message"].ToString());
			}

			return (false, "Something went wrong while saving rack.");
		}

		/// <summary>
		/// Update Rack 
		/// </summary>
		/// <param name="rackId"></param>
		/// <returns></returns>
		//   View Rack By ID
		public async Task<Inventory> GetRackByIdAsyncSK(int rackId)
        {
            var parameters = new Dictionary<string, string>
{
    { "@Flag", "GetRackByIdSK" },
    { "@RackId", rackId.ToString() }
};

            DataSet ds = await obj.ExecuteStoredProcedureReturnDS("InventoryProcedure", parameters);

            if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                var row = ds.Tables[0].Rows[0];
                return new Inventory
                {
                    RackId = Convert.ToInt32(row["RackId"]),
                    RackCode = row["RackCode"].ToString(),
                    RackName = row["RackName"].ToString(),
                    SectionCode = row["SectionCode"].ToString(),
                    SectionName = row["SectionName"].ToString(),
                    WarehouseCode = row["WareHouseCode"].ToString(),
                    WarehouseName = row["WarehouseName"].ToString(),
                    Description = row["Description"].ToString(),
                    AddedBy = row["AddedBy"].ToString(),
                    AddedDate = Convert.ToDateTime(row["AddedDate"]),
                };
            }
            return null;
        }
        /// <summary>
        /// Delete Rack
        /// </summary>
        /// <param name="rackId"></param>
        /// <returns></returns>
        // DELETE RACK
        public async Task<(bool Success, string Message)> DeleteRackAsyncSK(int rackId)
        {
            var parameters = new Dictionary<string, string>
{
    { "@Flag", "DeleteRackSK" },
    { "@RackId", rackId.ToString() }
};

            DataSet ds = await obj.ExecuteStoredProcedureReturnDS("InventoryProcedure", parameters);

            if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                var row = ds.Tables[0].Rows[0];
                return (true, row["Message"].ToString());
            }

            return (false, "Something went wrong while deleting rack.");
        }





        //=============================================================================ROW============================================================================

        /// <summary>
        /// Delete Row 
        /// </summary>
        /// <param name="rowId"></param>
        /// <returns></returns>
        //  Delete ROW
        public async Task DeleteRowAsyncSK(int rowId)
        {
            var parameters = new Dictionary<string, string>
{
    { "@Flag", "DeleteRowSK" },
    { "@RowId", rowId.ToString() }
};

            await obj.ExecuteStoredProcedure("InventoryProcedure", parameters);
        }
        /// <summary>
        /// Row List 
        /// </summary>
        /// <returns></returns>
        // ROW LIST


        public async Task<List<Inventory>> GetRowsAsyncSK()
        {
            List<Inventory> rows = new List<Inventory>();

            try
            {
                var parameters = new Dictionary<string, string>
            {
                { "@Flag", "RowsSK" }
            };

                DataSet ds = await obj.ExecuteStoredProcedureReturnDS("InventoryProcedure", parameters);

                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        Inventory row = new Inventory
                        {
                            WarehouseName = dr["WarehouseName"].ToString(),
                            SectionName = dr["SectionName"].ToString(),
                            RackName = dr["RackName"].ToString(),
                            RowId = Convert.ToInt32(dr["RowId"]),
                            RowCode = dr["RowCode"].ToString(),
                            RowName = dr["RowName"].ToString(),
                            AddedDate = Convert.ToDateTime(dr["AddedDate"]),
                            AddedBy = dr["AddedBy"].ToString(),
                            Description = dr["Description"].ToString()
                        };

                        rows.Add(row);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return rows;
        }
        /// <summary>
        /// Next Row Code 
        /// </summary>
        /// <returns></returns>
        // Next row code
        public async Task<string> GetNextRowCodeAsyncSK()
        {
            string nextCode = string.Empty;
            try
            {
                var parameters = new Dictionary<string, string>
            {
                { "@Flag", "RowCodeSK" }
            };

                object result = await obj.ExecuteStoredProcedureReturnObject("InventoryProcedure", parameters);

                if (result != null && result != DBNull.Value)
                {
                    nextCode = result.ToString();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return nextCode;
        }
        /// <summary>
        /// List Rack 
        /// </summary>
        /// <param name="sectionCode"></param>
        /// <returns></returns>
        // Rack List By using  Section
        public async Task<List<Inventory>> GetRackBySectionAsyncSK(string sectionCode)
        {
            List<Inventory> racks = new List<Inventory>();

            var parameters = new Dictionary<string, string>
{
    { "@Flag", "DRPRackSK" },
    { "@sectionCode", sectionCode }
};

            DataSet ds = await obj.ExecuteStoredProcedureReturnDS("InventoryProcedure", parameters);

            if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    racks.Add(new Inventory
                    {
                        RackCodes = row["RackCode"].ToString(),
                        RackNames = row["RackName"].ToString()
                    });
                }
            }

            return racks;
        }

		/// <summary>
		/// Save Row 
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		//  Save Row (Insert/Update)
		public async Task<(bool Success, string Message)> SaveRowAsyncSK(InventorySK model)
		{
			var parameters = new Dictionary<string, string>
	{
		{ "@Flag", "SaveRowSK" },
		{ "@RowId", model.RowId.ToString() },
		{ "@RowCode", model.RowCode ?? "" },
		{ "@RowName", model.RowName ?? "" },
		{ "@RackCode", model.RackCode ?? "" },
		{ "@Description", model.Description ?? "" },
		{ "@AddedBy", model.AddedBy ?? "" },
		{ "@AddedDate", model.AddedDate.ToString("yyyy-MM-dd")  }
	};

			DataSet ds = await obj.ExecuteStoredProcedureReturnDS("InventoryProcedure", parameters);

			if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
			{
				var row = ds.Tables[0].Rows[0];
				bool success = row["Success"] != DBNull.Value && Convert.ToBoolean(row["Success"]);
				string message = row["Message"].ToString();
				return (success, message);
			}

			return (false, "Something went wrong while saving row.");
		}

		/// <summary>
		/// View RoW 
		/// </summary>
		/// <param name="rowId"></param>
		/// <returns></returns>
		// List View Row 
		public async Task<Inventory> GetRowByIdAsyncSK(int rowId)
        {
            var parameters = new Dictionary<string, string>
{
    { "@Flag", "GetRowByIdSK" },
    { "@RowId", rowId.ToString() }
};

            DataSet ds = await obj.ExecuteStoredProcedureReturnDS("InventoryProcedure", parameters);

            if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                var dr = ds.Tables[0].Rows[0];
                return new Inventory
                {
                    RowId = Convert.ToInt32(dr["RowId"]),
                    RowCode = dr["RowCode"].ToString(),
                    RowName = dr["RowName"].ToString(),
                    RackCode = dr["RackCode"].ToString(),
                    RackName = dr["RackName"].ToString(),
                    SectionName = dr["SectionName"].ToString(),
                    SectionCode = dr["SectionCode"].ToString(),
                    WarehouseCode = dr["WarehouseCode"].ToString(),
                    WarehouseName = dr["WarehouseName"].ToString(),
                    AddedBy = dr["AddedBy"].ToString(),
                    AddedDate = Convert.ToDateTime(dr["AddedDate"]),
                    Description = dr["Description"].ToString()
                };
            }

            return null;
        }


        //==============================================================    BINS  ====================================
        /// <summary>
        /// Bin List 
        /// </summary>
        /// <returns></returns>
        //   Bins List
        public async Task<List<Inventory>> GetBinsAsyncSK()
        {

            List<Inventory> bins = new List<Inventory>();

            try
            {
                var parameters = new Dictionary<string, string>
            {
                { "@Flag", "BinsSK" }
            };

                DataSet ds = await obj.ExecuteStoredProcedureReturnDS("InventoryProcedure", parameters);

                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        Inventory bin = new Inventory
                        {
                            BinId = Convert.ToInt32(dr["BinId"]),
                            BinCode = dr["BinCode"].ToString(),
                            RowCode = dr["RowCode"].ToString(),
                            RowName = dr["RowName"].ToString(),
                            BinName = dr["BinName"].ToString(),
                            ItemName = dr["ItemName"].ToString(),
                            MaxQuantity = Convert.ToInt32(dr["MaxQuantity"]),
                            CurrentItems = Convert.ToInt32(dr["CurrentItems"]),
                            AddedDate = Convert.ToDateTime(dr["AddedDate"]),
                            AddedBy = dr["AddedBy"].ToString(),
                            Description = dr["Description"].ToString()
                        };

                        bins.Add(bin);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return bins;
        }
        /// <summary>
        /// Row List Using Rack Code 
        /// </summary>
        /// <param name="rowCode"></param>
        /// <returns></returns>
        // Row List By using Rack 
        public async Task<List<Inventory>> GetRowByRacksAsyncSK(string rowCode)
        {
            List<Inventory> racks = new List<Inventory>();

            var parameters = new Dictionary<string, string>
{
    { "@Flag", "DrpRowSK" },
    { "@RackCode", rowCode }
};

            DataSet ds = await obj.ExecuteStoredProcedureReturnDS("InventoryProcedure", parameters);

            if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    racks.Add(new Inventory
                    {
                        RowCode = row["RowCode"].ToString(),
                        RowName = row["RowName"].ToString()
                    });
                }
            }

            return racks;
        }
        /// <summary>
        /// Next Bin Code 
        /// </summary>
        /// <returns></returns>
        // Next Bin Code 
        public async Task<string> GetNextBinCodeAsyncSK()
        {
            string rackCode = "";

            var parameters = new Dictionary<string, string>
{
    { "@Flag", "BinCodeSK" }
};

            DataSet ds = await obj.ExecuteStoredProcedureReturnDS("InventoryProcedure", parameters);

            if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                rackCode = ds.Tables[0].Rows[0]["NextCode"].ToString();
            }

            return rackCode;
        }

        /// <summary>
        /// Item List 
        /// </summary>
        /// <returns></returns>
        //  Items List
        public async Task<List<Inventory>> GetItemslistSK()
        {
            List<Inventory> items = new List<Inventory>();

            var parameters = new Dictionary<string, string>
{
    { "@Flag", "ItemsSK" }
};

            DataSet ds = await obj.ExecuteStoredProcedureReturnDS("InventoryProcedure", parameters);

            if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    items.Add(new Inventory
                    {
                        ItemCode = row["ItemCode"].ToString(),
                        ItemName = row["ItemName"].ToString()
                    });
                }
            }

            return items;
        }
		/// <summary>
		/// Save BIn
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		// SAVE Bin
		public async Task<(bool Success, string Message)> SaveBinAsyncSK(InventorySK model)
		{
			var parameters = new Dictionary<string, string>
	{
		{ "@Flag", "SaveBinSK" },
		{ "@BinId", model.BinId.ToString() },
		{ "@BinCode", model.BinCode ?? "" },
		{ "@BinName", model.BinName ?? "" },
		{ "@ItemCode", model.ItemCode?.ToString() ?? "" },
		{ "@MaxQuantity", model.MaxQuantity?.ToString() ?? "0" },
		{ "@RowCode", model.RowCode?.ToString() ?? "0" },
		{ "@Description", model.Description ?? "" },
		{ "@AddedBy", model.AddedBy ?? "" },
		{ "@AddedDate", model.AddedDate.ToString("yyyy-MM-dd") }
	};

			DataSet ds = await obj.ExecuteStoredProcedureReturnDS("InventoryProcedure", parameters);

			if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
			{
				var row = ds.Tables[0].Rows[0];
				bool isSuccess = row.Table.Columns.Contains("Success") && Convert.ToBoolean(row["Success"]);
				string message = row["Message"].ToString();

				return (isSuccess, message);
			}

			return (false, "Something went wrong while saving Bin.");
		}

		/// <summary>
		/// View Bin Using ID
		/// </summary>
		/// <param name="binId"></param>
		/// <returns></returns>
		//  View Bin
		public async Task<Inventory> GetBinByIdAsyncSK(int binId)
        {
            var parameters = new Dictionary<string, string>
{
    { "@Flag", "GetBinByIdSK" },
    { "@BinId", binId.ToString() }
};

            DataSet ds = await obj.ExecuteStoredProcedureReturnDS("InventoryProcedure", parameters);

            if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                var dr = ds.Tables[0].Rows[0];
                return new Inventory
                {
                    BinId = Convert.ToInt32(dr["BinId"]),
                    BinCode = dr["BinCode"].ToString(),
                    BinName = dr["BinName"].ToString(),

                    RowCode = dr["RowCode"].ToString(),
                    RowName = dr["RowName"].ToString(),

                    RackCode = dr["RackCode"].ToString(),
                    RackName = dr["RackName"].ToString(),

                    SectionCode = dr["SectionCode"].ToString(),
                    SectionName = dr["SectionName"].ToString(),

                    WarehouseCode = dr["WareHouseCode"].ToString(),
                    WarehouseName = dr["WarehouseName"].ToString(),

                    ItemCode = dr["ItemCode"].ToString(),
                    ItemName = dr["ItemName"].ToString(),

                    MaxQuantity = dr["MaxQuantity"] != DBNull.Value ? Convert.ToInt32(dr["MaxQuantity"]) : 0,
                    CurrentItems = dr["CurrentItems"] != DBNull.Value ? Convert.ToInt32(dr["CurrentItems"]) : 0,

                    Description = dr["Description"].ToString(),
                    AddedBy = dr["AddedBy"].ToString(),
                    AddedDate = Convert.ToDateTime(dr["AddedDate"])
                };
            }

            return null;
        }

        /// <summary>
        /// Delete BiN Using Id
        /// </summary>
        /// <param name="binId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>

        //  Delete Bin
        public async Task DeleteBinAsyncSK(int binId)
        {
            try
            {
                var parameters = new Dictionary<string, string>
    {
        { "@Flag", "DeleteBinSK" },
        { "@BinId", binId.ToString() }
    };

                await obj.ExecuteStoredProcedure("InventoryProcedure", parameters);
            }
            catch (SqlException sqlEx)
            {

                throw new Exception($"Database error while deleting bin (ID: {binId}). Details: {sqlEx.Message}", sqlEx);
            }
            catch (Exception ex)
            {

                throw new Exception($"An unexpected error occurred while deleting bin (ID: {binId}). Details: {ex.Message}", ex);
            }
        }






        ////////////////////////////////////////////////////////////////////////// = SECTION = //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Section List 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ApplicationException"></exception>
        //  Get Section List
        public async Task<List<Inventory>> GetSectionsAsyncSK()
        {
            var sections = new List<Inventory>();

            try
            {
                var parameters = new Dictionary<string, string>
            {
                { "@Flag", "GetSectionSK" }
            };

                DataSet ds = await obj.ExecuteStoredProcedureReturnDS("InventoryProcedure", parameters);

                if (ds != null && ds.Tables.Count > 0)
                {
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        sections.Add(new Inventory
                        {
                            SectionId = Convert.ToInt32(dr["SectionId"]),
                            SectionCode = dr["SectionCode"].ToString(),
                            SectionName = dr["SectionName"].ToString(),
                            WarehouseCode = dr["WareHouseCode"].ToString(),
                            WarehouseName = dr["WarehouseName"].ToString(),
                            Description = dr["Description"].ToString()
                        });
                    }
                }
            }
            catch (Exception ex)
            {

                throw new ApplicationException("Error fetching Sections", ex);
            }

            return sections;
        }

        /// <summary>
        /// Next Code 
        /// </summary>
        /// <returns></returns>
        /// 
        // Get Next Section Code
        public async Task<string> GetNextSectionCodeAsyncSK()
        {
            try
            {
                string sectionCode = "";

                var parameters = new Dictionary<string, string>
    {
        { "@Flag", "NextSectionCodeSK" }
    };

                DataSet ds = await obj.ExecuteStoredProcedureReturnDS("InventoryProcedure", parameters);

                if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    sectionCode = ds.Tables[0].Rows[0]["NextSectionCode"].ToString();
                }

                return sectionCode;
            }
            catch (Exception)
            {


                return string.Empty;
            }
        }
		/// <summary>
		/// Save Section
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		// Save Section
		public async Task<(bool Success, string Message)> AddSectionAsyncSK(InventorySK model)
		{
			try
			{
				var parameters = new Dictionary<string, string>
		{
			{ "@Flag", "InsertSectionSK" },
			{ "@SectionCode", model.SectionCode },
			{ "@SectionName", model.SectionName },
			{ "@WarehouseCode", model.WarehouseCode },
			{ "@Description", model.Description }
		};

				DataSet ds = await obj.ExecuteStoredProcedureReturnDS("InventoryProcedure", parameters);

				if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
				{
					string result = ds.Tables[0].Rows[0]["Result"].ToString();
					string message = ds.Tables[0].Rows[0]["Message"].ToString();

					return (result == "1", message);
				}

				return (false, "Failed to save section.");
			}
			catch (Exception ex)
			{
				return (false, ex.Message);
			}
		}


		/// <summary>
		/// View Section Using Id 
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>

		///  View Section
		public async Task<Inventory> GetSectionByIdAsyncSK(int id)
        {
            try
            {
                var parameters = new Dictionary<string, string>
    {
        { "@Flag", "GetSectionByIdSK" },
        { "@SectionId", id.ToString() }
    };

                DataSet ds = await obj.ExecuteStoredProcedureReturnDS("InventoryProcedure", parameters);

                if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    DataRow row = ds.Tables[0].Rows[0];
                    return new Inventory
                    {
                        SectionId = Convert.ToInt32(row["SectionId"]),
                        SectionCode = row["SectionCode"].ToString(),
                        SectionName = row["SectionName"].ToString(),
                        WarehouseCode = row["WarehouseCode"].ToString(),
                        WarehouseName = row["WarehouseName"].ToString(),
                        Description = row["Description"].ToString()
                    };
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

		/// <summary>
		/// Update Section
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		// Update 
		public async Task<(bool Success, string Message)> UpdateSectionAsyncSK(InventorySK model)
		{
			try
			{
				var parameters = new Dictionary<string, string>
		{
			{ "@Flag", "UpdateSectionSK" },
			{ "@SectionId", model.SectionId.ToString() },
			{ "@SectionCode", model.SectionCode },
			{ "@SectionName", model.SectionName },
			{ "@WarehouseCode", model.WarehouseCode },
			{ "@Description", model.Description }
		};

				DataSet ds = await obj.ExecuteStoredProcedureReturnDS("InventoryProcedure", parameters);

				if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
				{
					string result = ds.Tables[0].Rows[0]["Result"].ToString();
					string message = ds.Tables[0].Rows[0]["Message"].ToString();

					return (result == "1", message);
				}

				return (false, "Failed to update section.");
			}
			catch (Exception ex)
			{
				return (false, ex.Message);
			}
		}



		/// <summary>
		/// Delete Section Using By ID
		/// </summary>
		/// <param name="sectionId"></param>
		/// <returns></returns>
		/// <exception cref="Exception"></exception>
		//  Delete Section
		public async Task<bool> DeleteSectionAsyncSK(int sectionId)
        {
            try
            {
                var parameters = new Dictionary<string, string>
    {
        { "@Flag", "DeleteSectionSK" },
        { "@SectionId", sectionId.ToString() }
    };

                await obj.ExecuteStoredProcedure("InventoryProcedure", parameters);

                return true;
            }
            catch (SqlException sqlEx)
            {

                throw new Exception($"Database error while deleting Section (ID: {sectionId}). Details: {sqlEx.Message}", sqlEx);
            }
            catch (Exception ex)
            {

                throw new Exception($"Unexpected error while deleting Section (ID: {sectionId}). Details: {ex.Message}", ex);
            }
        }


        #endregion

            #region Mayur
        /// <summary>
        /// Retrieves stock planning data for MHB by executing the 'StockAnalysisMHB' flag.
        /// </summary>
        /// <returns>List of inventory items with stock status.</returns>
        public async Task<List<InventoryMHB>> StockplanningMHB()
        {
            List<InventoryMHB> lst = new List<InventoryMHB>();
            Dictionary<string, string> paradic = new Dictionary<string, string>();

            paradic.Add("@flag", "StockAnalysisMHB");

            DataSet ds = await obj.ExecuteStoredProcedureReturnDS("InventoryProcedure", paradic);

            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    InventoryMHB SHR = new InventoryMHB
                    {
                        ItemName = row["ItemName"].ToString(),
                        ItemCode = row["ItemCode"].ToString(),
                        ReorderQuantity = row["ReorderQuantity"].ToString(),
                        minQuantity = row["minQuantity"].ToString(),
                        //MaxQuantity = Convert.ToInt32(row["MaxQuantity"]),
                        CurrentItems = Convert.ToInt32(row["CurrentItems"]),
                        StockStatus = row["StockStatus"].ToString()
                    };
                    lst.Add(SHR);
                }
            }
            
            return lst;
        }

        /// <summary>
        /// Retrieves an inventory item by its ItemCode for MHB.
        /// </summary>
        /// <param name="itemId">The ItemCode to look up.</param>
        /// <returns>The inventory item with all related information.</returns>
        public async Task<InventoryMHB> GetItemByIdMHB(string itemId)
        {
            Dictionary<string, string> paradic = new Dictionary<string, string>();
            paradic.Add("@flag", "GetItemByIdProcMHB");
            paradic.Add("@ItemCode", itemId);

            DataSet ds = await obj.ExecuteStoredProcedureReturnDS("InventoryProcedure", paradic);
            InventoryMHB itm = new InventoryMHB();

            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)







































































































































































































            {
                itm.ItemId = ds.Tables[0].Rows[i]["ItemId"].ToString();
                itm.ItemName = ds.Tables[0].Rows[i]["ItemName"].ToString();
                itm.ItemCode = ds.Tables[0].Rows[i]["ItemCode"].ToString();
                itm.Description = ds.Tables[0].Rows[i]["Description"].ToString();
                itm.ReorderQuantity = ds.Tables[0].Rows[i]["ReorderQuantity"].ToString();
            }

            for (int i = 0; i < ds.Tables[1].Rows.Count; i++)
            {
                itm.UOMName = ds.Tables[1].Rows[i]["UOMName"].ToString();
            }

            return itm;
        }

        /// <summary>
        /// Saves a Purchase Requisition (PR) request for MHB.
        /// </summary>
        /// <param name="model">Inventory model containing PR details.</param>
        /// <returns>True if the PR is saved successfully.</returns>
        public async Task<bool> SavePRRequestMHB(InventoryMHB model)
        {
            Dictionary<string, string> paramdic = new Dictionary<string, string>();
            paramdic.Add("@flag", "saveprrequestMHB");
            paramdic.Add("@ItemCode", model.ItemCode);
            paramdic.Add("@Quantity", model.Quantity.ToString());
            paramdic.Add("@AddedDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            paramdic.Add("@RequiredDate", model.RequiredDate.HasValue ? model.RequiredDate.Value.ToString("yyyy-MM-dd") : DBNull.Value.ToString());
            paramdic.Add("@StaffCode", model.StaffCode);

            DataSet ds = await obj.ExecuteStoredProcedureReturnDS("InventoryProcedure", paramdic);
            return true;
        }

        /// <summary>
        /// Retrieves dataset of items that require stock refill for MHB.
        /// </summary>
        /// <returns>Dataset containing stock refill data.</returns>
        public async Task<DataSet> ItemStockRefillMHB()
        {
            Dictionary<string, string> paradic = new Dictionary<string, string>();
            paradic.Add("@flag", "ISRstockMHB");

            DataSet ds = await obj.ExecuteStoredProcedureReturnDS("InventoryProcedure", paradic);
            return ds;
        }

        /// <summary>
        /// Retrieves list of all inventory items for MHB.
        /// </summary>
        /// <returns>Dataset of item list.</returns>
        public async Task<DataSet> ItemlistMHB()
        {
            Dictionary<string, string> paradic = new Dictionary<string, string>();
            paradic.Add("@flag", "ItemlistjitMHB");

            DataSet ds = await obj.ExecuteStoredProcedureReturnDS("InventoryProcedure", paradic);
            return ds;
        }

        /// <summary>
        /// Saves a Just-In-Time (JIT) inventory request in MHB.
        /// </summary>
        /// <param name="model">Inventory model with JIT details.</param>
        /// <returns>True if saved successfully.</returns>
        public async Task<bool> SaveJITMHB(InventoryMHB model)
        {
            Dictionary<string, string> paramdic = new Dictionary<string, string>();
            paramdic.Add("@flag", "saveJustInTimeMHB");
            paramdic.Add("@ItemCode", model.ItemCode);
            paramdic.Add("@Quantity", model.Quantity.ToString());
            paramdic.Add("@AddedDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            paramdic.Add("@RequiredDate", model.RequiredDate?.ToString("yyyy-MM-dd HH:mm:ss"));

            paramdic.Add("@StaffCode", model.StaffCode);

            DataSet ds = await obj.ExecuteStoredProcedureReturnDS("InventoryProcedure", paramdic);
            return true;
        }

        /// <summary>
        /// Saves a Material Requirements Planning (MRP) plan including header and item list.
        /// </summary>
        /// <param name="model">Inventory model containing plan and items.</param>
        /// <returns>True if saved successfully.</returns>
        public async Task<bool> SaveMRPMHB(InventoryMHB model)
        {
            string mrpCode = await GenerateNextMRPCodeMHB();

            Dictionary<string, string> paramHeader = new Dictionary<string, string>
            {
                { "flag", "SaveMRPHeaderMHB" },
                { "PlanName", model.PlanName },
                { "Year", model.Year },
                { "FromDate", model.FromDate },
                { "ToDate", model.ToDate },
                { "MRPCode", mrpCode },
                { "StaffCode", model.StaffCode },
                { "@AddedDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") }
            };

            DataSet dsHeader = await obj.ExecuteStoredProcedureReturnDS("InventoryProcedure", paramHeader);

            foreach (var item in model.Items)
            {
                Dictionary<string, string> paramItem = new Dictionary<string, string>
                {
                    { "flag", "SaveMRPItemMHB" },
                    { "MRPCode", mrpCode },
                    { "ItemCode", item.ItemCode },
                    { "Quantity", item.QuantityMRP }
                };

                await obj.ExecuteStoredProcedureReturnDS("InventoryProcedure", paramItem);
            }

            return true;
        }

        /// <summary>
        /// Generates the next available MRP code by incrementing the last used code.
        /// </summary>
        /// <returns>New MRP code string in format "MRP###".</returns>
        public async Task<string> GenerateNextMRPCodeMHB()
        {
            Dictionary<string, string> param = new Dictionary<string, string>
            {
                { "flag", "GetMaxMRPCodeMHB" }
            };

            DataSet ds = await obj.ExecuteStoredProcedureReturnDS("InventoryProcedure", param);

            if (ds.Tables[0].Rows.Count > 0)
            {
                string lastCode = ds.Tables[0].Rows[0]["MaterialReqPlanningCode"].ToString();
                int next = int.Parse(lastCode.Substring(3)) + 1;
                return "MRP" + next.ToString("000");
            }

            return "MRP001";
        }

        /// <summary>
        /// Fetches all saved MRP plan headers for MHB.
        /// </summary>
        /// <returns>Dataset containing MRP plan list.</returns>
        public async Task<DataSet> fetchplandetailsMHB()
        {
            Dictionary<string, string> paradic = new Dictionary<string, string>();
            paradic.Add("@flag", "fetchPlanDetailsMHB");

            DataSet ds = await obj.ExecuteStoredProcedureReturnDS("InventoryProcedure", paradic);
            return ds;
        }

        /// <summary>
        /// Retrieves the details of a specific MRP plan based on MRP code.
        /// </summary>
        /// <param name="MRPCode">The MRP code to search.</param>
        /// <returns>Dataset with plan details.</returns>
        public async Task<DataSet> showplanMHB(string MRPCode)
        {
            Dictionary<string, string> paradic = new Dictionary<string, string>();
            paradic.Add("@flag", "showplandetailsMHB");
            paradic.Add("@MRPCode", MRPCode);

            DataSet ds = await obj.ExecuteStoredProcedureReturnDS("InventoryProcedure", paradic);
            return ds;
        }

        /// <summary>
        /// Approves or rejects an MRP plan based on the status flag.
        /// </summary>
        /// <param name="MRPCode">MRP plan code.</param>
        /// <param name="isApproved">True for approval, false for rejection.</param>
        /// <param name="reason">Reason for rejection (if applicable).</param>
        /// <param name="model">Inventory model with staff info.</param>
        /// <returns>True if status was updated successfully.</returns>
        public async Task<bool> UpdateMRPPlanStatusMHB(string MRPCode, bool isApproved, string reason, InventoryMHB model)
        {
            Dictionary<string, string> para = new Dictionary<string, string>();
            para.Add("@flag", "aprv-rjct-planMHB");
            para.Add("@MRPCode", MRPCode);
            para.Add("@statusid", isApproved ? "1" : "2");
            para.Add("@AddedDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            para.Add("@StaffCode", model.StaffCode);

            if (!isApproved && !string.IsNullOrEmpty(reason))
                para.Add("@reason", reason);

            object resObj = await obj.ExecuteStoredProcedureReturnObject("InventoryProcedure", para);
            int result = resObj != null ? Convert.ToInt32(resObj) : 0;

            return result > 0;
        }

        /// <summary>
        /// Deletes existing MRP items and saves updated list for an MRP plan.
        /// </summary>
        /// <param name="model">Inventory model with new item list and MRP code.</param>
        /// <returns>True if successfully saved.</returns>
        public async Task<bool> saveEditedPlanMHB(InventoryMHB model)
        {
            try
            {
                var deleteParams = new Dictionary<string, string>
                {
                    { "@flag", "deleteMRPItemsMHB" },
                    { "@MRPCode", model.MRPCode }
                };

                await obj.ExecuteStoredProcedureReturnDS("InventoryProcedure", deleteParams);

                foreach (var item in model.ItemList)
                {
                    var insertParams = new Dictionary<string, string>
                    {
                        { "@flag", "insertMRPItemMHB" },
                        { "@MRPCode", model.MRPCode },
                        { "@ItemCode", item.ItemCode },
                        { "@Quantity", item.QuantityMRP }
                    };

                    await obj.ExecuteStoredProcedureReturnDS("InventoryProcedure", insertParams);
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in saveEditedPlanMHB: " + ex.Message);
                return false;
            }
        }

        #endregion

        #region Sayali and OM
        /// <summary>
        /// Fetches all inventory items with details (UOM, Category, Status, etc.) from DB.
        /// </summary>
        /// <returns>List of Inventory items</returns>
        public async Task<List<InventoryOJ>> GetItemOJ()
        {
            List<InventoryOJ> ItemList = new List<InventoryOJ>();
            Dictionary<string, string> item = new Dictionary<string, string>();
            item.Add("@Flag", "ShowItemOJ");

            var ds = await obj.ExecuteStoredProcedureReturnDS("InventoryProcedure", item);

            foreach (DataRow da in ds.Tables[0].Rows)
            {
                //Itemmater
                InventoryOJ items = new InventoryOJ();
                items.ItemIdOJ = Convert.ToInt32(da["ItemId"]);
                items.ItemCode = da["ItemCode"].ToString();
                items.ItemName = da["ItemName"].ToString();
                items.UOMId = Convert.ToInt32(da["UOMId"]);
                items.UOM = da["UOMName"].ToString();
                items.Date = Convert.ToDateTime(da["Date"].ToString());
                items.UnitRates = da["UnitRates"] == DBNull.Value ? 0m : Convert.ToDecimal(da["UnitRates"]);
                items.ItemCategoryId = Convert.ToInt32(da["ItemCategoryId"]);
                items.ItemCategory = da["ItemCategoryName"].ToString();
                items.MinQuantity = Convert.ToInt32(da["MinQuantity"]);
                items.ItemStatusId = Convert.ToInt32(da["StatusId"]);
                items.Status = da["StatusName"].ToString();
                items.HSNCode = Convert.ToInt32(da["HSNCode"]);
                items.ExpiryDays = Convert.ToInt32(da["ExpiryDays"]);
                items.ISQualityBit = Convert.ToInt32(da["ISQualityBit"]);
                items.ISQuality = da["ISQuality"].ToString();
                items.RecorderQuantity = Convert.ToInt32(da["ReorderQuantity"]);
                items.Description = da["Description"].ToString();
                items.ItemMakeId = Convert.ToInt32(da["SubTypeId"]);
                items.ItemMake = da["SubTypeName"].ToString();


                ItemList.Add(items);
            }
            return ItemList;
        }
        /// <summary>
        /// Fetches all available item statuses (e.g., Active/Inactive).
        /// </summary>
        /// <returns>List of statuses</returns>
        public async Task<List<Fetch>> GetStatusOJ()
        {

            Dictionary<string, string> status = new Dictionary<string, string>();
            status.Add("@Flag", "ShowStatusOJ");


            var ds = await obj.ExecuteStoredProcedureReturnDS("InventoryProcedure", status);
            List<Fetch> statuses = new List<Fetch>();
            if (ds != null && ds.Tables.Count > 0)
            {
                foreach (DataRow da in ds.Tables[0].Rows)
                {

                    statuses.Add(new Fetch
                    {

                        StatusId = Convert.ToInt32(da["StatusId"]),
                        StatusName = da["StatusName"].ToString()

                    });


                }
            }
            return statuses;



        }
        /// <summary>
        /// Fetches all available item categories.
        /// </summary>
        /// <returns>List of categories</returns>
        public async Task<List<Fetch>> GetCategoryOJ()
        {
            Dictionary<string, string> category = new Dictionary<string, string>();
            category.Add("@Flag", "ShowCategoryOJ");

            var cat = await obj.ExecuteStoredProcedureReturnDS("InventoryProcedure", category);
            List<Fetch> categories = new List<Fetch>();
            if (cat != null && cat.Tables.Count > 0)
            {
                foreach (DataRow da in cat.Tables[0].Rows)
                {
                    categories.Add(new Fetch
                    {
                        ItemCategoryId = Convert.ToInt32(da["ItemCategoryId"]),
                        ItemCategoryName = da["ItemCategoryName"].ToString()
                    });
                }
            }
            return categories;
        }
        /// <summary>
        /// Fetches HSN codes based on selected item category.
        /// </summary>
        /// <param name="id">ItemCategoryId</param>
        /// <returns>List of HSN codes with tax rates</returns>
        public async Task<List<InventoryOJ>> GetHSNCodeOJ(int id)
        {
            Dictionary<string, string> HSNCode = new Dictionary<string, string>();
            HSNCode.Add("@Flag", "ShowHSNCodeOJ");
            HSNCode.Add("@ItemCategoryId", id.ToString());

            var hsn = await obj.ExecuteStoredProcedureReturnDS("InventoryProcedure", HSNCode);
            List<InventoryOJ> HSN = new List<InventoryOJ>();
            if (hsn != null && hsn.Tables.Count > 0)
            {
                foreach (DataRow da in hsn.Tables[0].Rows)
                {
                    HSN.Add(new InventoryOJ
                    {
                        TaxRateId = Convert.ToInt32(da["TaxRateId"]),
                        HSNCode = Convert.ToInt32(da["HSNCode"]),
                    });
                }
            }
            return HSN;
        }
        /// <summary>
        /// Fetches all item makes/manufacturers.
        /// </summary>
        /// <returns>List of item makes</returns>
        public async Task<List<Fetch>> GetItemMakeOJ()
        {
            Dictionary<string, string> ItemMake = new Dictionary<string, string>();
            ItemMake.Add("@Flag", "FetchItemMakeOJ");

            var ds = await obj.ExecuteStoredProcedureReturnDS("InventoryProcedure", ItemMake);
            List<Fetch> itemake = new List<Fetch>();

            if (ds != null && ds.Tables.Count > 0)
            {
                foreach (DataRow da in ds.Tables[0].Rows)
                {
                    itemake.Add(new Fetch
                    {
                        ItemMakeId = Convert.ToInt32(da["SubTypeId"]),
                        ItemMake = da["SubTypeName"].ToString(),
                    });
                }
            }
            return itemake;
        }
        /// <summary>
        /// Fetches all available Units of Measurement (UOM).
        /// </summary>
        /// <returns>List of UOMs</returns>
        public async Task<List<Fetch>> GetUOMOJ()
        {
            Dictionary<string, string> UOM = new Dictionary<string, string>();
            UOM.Add("@Flag", "FetchUOMOJ");

            var uom = await obj.ExecuteStoredProcedureReturnDS("InventoryProcedure", UOM);
            List<Fetch> uomList = new List<Fetch>();

            if (uom != null && uom.Tables.Count > 0)
            {
                foreach (DataRow da in uom.Tables[0].Rows)
                {
                    uomList.Add(new Fetch
                    {
                        UOMId = Convert.ToInt32(da["UOMId"]),
                        UOMName = da["UOMName"].ToString(),

                    });
                }
            }
            return uomList;
        }
        /// <summary>
        /// Fetches all available plan types.
        /// </summary>
        /// <returns>List of plan types</returns>
        public async Task<List<Fetch>> GetPlanOJ()
        {
            Dictionary<string, string> plan = new Dictionary<string, string>();
            plan.Add("@Flag", "plantypeOJ");

            var type = await obj.ExecuteStoredProcedureReturnDS("InventoryProcedure", plan);

            List<Fetch> planlist = new List<Fetch>();

            if (type != null && type.Tables.Count > 0)
            {
                foreach (DataRow da in type.Tables[0].Rows)
                {
                    planlist.Add(new Fetch
                    {
                        PlanTypeId = Convert.ToInt32(da["SubTypeId"]),
                        PlanType = da["SubTypeName"].ToString(),
                    });
                }
            }
            return planlist;
        }
        /// <summary>
        /// Fetches qualitative parameters (e.g., Quality parameters).
        /// </summary>
        /// <returns>List of qualitative parameters</returns>
        public async Task<List<Fetch>> GetInspectionOJ()
        {
            Dictionary<string, string> inspec = new Dictionary<string, string>();
            inspec.Add("@Flag", "inspectiontypeOJ");

            var type = await obj.ExecuteStoredProcedureReturnDS("InventoryProcedure", inspec);

            List<Fetch> inspection = new List<Fetch>();

            if (type != null && type.Tables.Count > 0)
            {
                foreach (DataRow da in type.Tables[0].Rows)
                {
                    inspection.Add(new Fetch
                    {
                        InseepctionId = Convert.ToInt32(da["SubTypeId"]),
                        InsepctionName = da["SubTypeName"].ToString(),
                    });
                }
            }
            return inspection;
        }
        /// <summary>
        /// Fetches quantitative parameters.
        /// </summary>
        /// <returns>List of quantitative parameters</returns>
        public async Task<List<Fetch>> GetQualitativeOJ()
        {
            Dictionary<string, string> quali = new Dictionary<string, string>();
            quali.Add("@Flag", "QualitativeOJ");

            var type = await obj.ExecuteStoredProcedureReturnDS("InventoryProcedure", quali);

            List<Fetch> qualitative = new List<Fetch>();

            if (type != null && type.Tables.Count > 0)
            {
                foreach (DataRow da in type.Tables[0].Rows)
                {
                    qualitative.Add(new Fetch
                    {
                        Qualitative = Convert.ToInt32(da["SubTypeId"]),
                        QualitativeName = da["SubTypeName"].ToString(),
                    });
                }
            }
            return qualitative;
        }
        /// <summary>
        /// Fetches quantitative parameters.
        /// </summary>
        /// <returns>List of quantitative parameters</returns>
        public async Task<List<Fetch>> GetQuantitativeOJ()
        {
            Dictionary<string, string> quan = new Dictionary<string, string>();
            quan.Add("@Flag", "QuantitativeOJ");

            var type = await obj.ExecuteStoredProcedureReturnDS("InventoryProcedure", quan);

            List<Fetch> qualitative = new List<Fetch>();

            if (type != null && type.Tables.Count > 0)
            {
                foreach (DataRow da in type.Tables[0].Rows)
                {
                    qualitative.Add(new Fetch
                    {
                        QuantitativeId = Convert.ToInt32(da["SubTypeId"]),
                        QuantitativeName = da["SubTypeName"].ToString(),
                    });
                }
            }
            return qualitative;
        }
        /// <summary>
        /// Generates the next sequential ItemCode (e.g., ITM001, ITM002).
        /// </summary>
        /// <returns>New ItemCode string</returns>
        public async Task<string> GenerateNextItemCodeOJ()
        {

            Dictionary<string, string> para = new Dictionary<string, string>();
            para.Add("@Flag", "GenerateItemCodeOJ");

            DataSet ds = await obj.ExecuteStoredProcedureReturnDS("InventoryProcedure", para);

            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                string lastCode = dt.Rows[0]["ItemCode"].ToString();

                int number = int.Parse(lastCode.Substring(3));
                string nextCode = "ITM" + (number + 1).ToString("D3");

                return nextCode;
            }
            else
            {
                return "ITM001";
            }

        }
        /// <summary>
        /// Inserts a new item into the inventory table.
        /// </summary>
        /// <return>Inventory object with item details</return>

        public async Task<int> AddItemOJ(InventoryOJ n)
        {
            try
            {
                var nextcode = await GenerateNextItemCodeOJ();

                Dictionary<string, string> Additem = new Dictionary<string, string>();

                Additem.Add("@Flag", "InsertItemOJ");
                Additem.Add("@ItemCode", nextcode.ToString());
                Additem.Add("@ItemName", n.ItemName ?? "");
                Additem.Add("@ItemCategoryId", n.ItemCategoryId.ToString());
                Additem.Add("@ItemStatusId", n.ItemStatusId.ToString());
                Additem.Add("@Date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")); 
                Additem.Add("@UOMId", n.UOMId.ToString());
                Additem.Add("@Description", n.Description ?? "");
                Additem.Add("@UnitRates", n.UnitRates.ToString());
                Additem.Add("@RecorderQ", n.RecorderQuantity.ToString());
                Additem.Add("@minQ", n.MinQuantity.ToString());
                Additem.Add("@itemby", n.ItemMakeId.ToString());
                Additem.Add("@Addedby", n.StaffCode ?? "");
                Additem.Add("@ExpiryDays", n.ExpiryDays.ToString());
                Additem.Add("@IsQuality", n.ISQualityBit.ToString());

                var ds = await obj.ExecuteStoredProcedureReturnDS("InventoryProcedure", Additem);

                if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    return Convert.ToInt32(ds.Tables[0].Rows[0]["Result"]);
                }

                return -1; // unexpected
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error inserting item: " + ex.Message);
                throw new Exception("Error while inserting item in DB", ex);
            }
        }


        /// <summary>
        /// Generates the next sequential QualityCode (e.g., IQ001, IQ002).
        /// </summary>
        /// <returns>New ItemQualityCode string</returns>
        public async Task<string> GenerateNextQualityCodeOJ()
        {
            Dictionary<string, string> para = new Dictionary<string, string>();
            para.Add("@Flag", "GenerateQualityCodeOJ");

            DataSet ds = await obj.ExecuteStoredProcedureReturnDS("InventoryProcedure", para);

            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                string lastCode = dt.Rows[0]["ItemQualityCode"].ToString();

                int number = int.Parse(lastCode.Substring(3));
                string nextCode = "IQ" + (number + 1).ToString("D3");

                return nextCode;
            }
            else
            {
                return "IQ001";
            }
        }


        /// <summary>
        /// Generates the next sequential PlanCode (e.g., PLN001, PLN002).
        /// </summary>
        /// <returns>New PlanCode string</returns>

        public async Task<string> GenerateNextPlanCodeOJ()
        {

            Dictionary<string, string> para = new Dictionary<string, string>();
            para.Add("@Flag", "GeneratePlanCodeOJ");

            DataSet ds = await obj.ExecuteStoredProcedureReturnDS("InventoryProcedure", para);

            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                string lastCode = dt.Rows[0]["PlanCode"].ToString();

                int number = int.Parse(lastCode.Substring(3));
                string nextCode = "PLN" + (number + 1).ToString("D3");

                return nextCode;
            }
            else
            {
                return "PLN001";
            }

        }
        /// <summary>
        /// Inserts a new plan (inspection/quality/quantity parameters).
        /// </summary>
        /// <return>Inventory object with plan details</return>
        public async Task AddPlanOJ(InventoryOJ n)
        {
            try
            {
                string plancode = string.IsNullOrEmpty(n.PlanCode) ? await GenerateNextPlanCodeOJ() : n.PlanCode;


                var nextcode = string.IsNullOrEmpty(n.ItemCode) ? await GenerateNextItemCodeOJ() : n.ItemCode;

                var qualitycode = await GenerateNextQualityCodeOJ();

                Dictionary<string, object> AddPlan = new Dictionary<string, object>();
                AddPlan.Add("@Flag", "InsertPlanOJ");
                AddPlan.Add("@ItemCode", nextcode);
                AddPlan.Add("@ItemQualityCode", qualitycode);
                AddPlan.Add("@PlanCode", plancode);
                AddPlan.Add("@PlanId", n.PlanId);
                AddPlan.Add("@Descriptionplan", n.PlanDescription);
                AddPlan.Add("@InspectionId", n.InspectionId);

                int value = (n.QualityParametersId != 0) ? n.QualityParametersId
                         : (n.QuantityParametersId != 0) ? n.QuantityParametersId : 0;

                if (value != 0)
                {
                    AddPlan.Add("@QualityParameters", value);
                }

                AddPlan.Add("@Quality", n.PQuality);
                AddPlan.Add("@UOMId", n.PUOMId);

                await obj.ExecuteStoredProcedure("InventoryProcedure", AddPlan);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error inserting item: " + ex.Message);
                throw new Exception("Error while inserting item in DB", ex);
            }
        }

        /// <summary>
        /// Fetches plan details by PlanCode.
        /// </summary>
        /// <returns>List of plans with quality parameters</returns>
        public async Task<List<InventoryOJ>> ShowPlanOJ(string plan)
        {
            List<InventoryOJ> list = new List<InventoryOJ>();
            Dictionary<string, string> showplan = new Dictionary<string, string>();
            showplan.Add("@Flag", "ShowPlanOJ");
            showplan.Add("@PlanCode", plan);

            var ds = await obj.ExecuteStoredProcedureReturnDS("InventoryProcedure", showplan);

            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow da in ds.Tables[0].Rows)
                {
                    InventoryOJ splan = new InventoryOJ();
                    splan.ItemQualityId = Convert.ToInt32(da["ItemQualityId"]);
                    splan.QualityParametersName = da["QualityParamName"].ToString();
                    splan.PQuality = da["Quality"].ToString();
                    splan.PUOMName = da["UOMName"].ToString();
                    list.Add(splan);

                }
            }
            return list;
        }


        /// <summary>
        /// Fetches inspection plans linked to an item.
        /// </summary>
        /// <returns>List of inspection plans for the item</returns>
        public async Task<List<InventoryOJ>> GetInspecPlanOJ(string itemcode)
        {
            List<InventoryOJ> PlanList = new List<InventoryOJ>();
            Dictionary<string, string> item = new Dictionary<string, string>();
            item.Add("@Flag", "InspectionplanOJ");
            item.Add("@ItemCode", itemcode);



            var ds = await obj.ExecuteStoredProcedureReturnDS("InventoryProcedure", item);
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {

                foreach (DataRow da in ds.Tables[0].Rows)
                {
                    // InspectionPlan
                    InventoryOJ plan = new InventoryOJ();
                    plan.ItemQualityId = Convert.ToInt32(da["ItemQualityId"]);
                    plan.ItemQualityCode = da["ItemQualityCode"].ToString();
                    plan.ItemCode = da["ItemCode"].ToString();
                    plan.PlanId = Convert.ToInt32(da["PlanId"]);
                    plan.PlanName = da["PlanName"].ToString();
                    plan.InspectionId = Convert.ToInt32(da["InspectionId"]);
                    plan.InspectionName = da["InspectionName"].ToString();
                    plan.Parameters = Convert.ToInt32(da["QualityParametersId"]);
                    plan.ParametersName = da["QualityParamName"].ToString();
                    plan.PQuality = da["Quality"].ToString();
                    plan.PUOMId = Convert.ToInt32(da["UOM"]);
                    plan.PUOMName = da["UOMName"].ToString();
                    plan.PlanCode = da["PlanCode"].ToString();
                    plan.PlanDescription = da["Description"].ToString();
                    PlanList.Add(plan);
                }

            }
            return PlanList;


        }
        /// <summary>
        /// Updates an existing item in the inventory table.
        /// </summary>
        /// <param>Inventory object with updated details</param>

        public async Task UpdateItemOJ(InventoryOJ n)
        {
            try
            {

                Dictionary<string, object> Edititem = new Dictionary<string, object>();
                Edititem.Add("@Flag", "UpdateItemOJ");
                Edititem.Add("@ItemId", n.ItemIdOJ);
                Edititem.Add("@ItemStatusId", n.ItemStatusId);
                Edititem.Add("@Date", DateTime.Now);
                Edititem.Add("@UnitRates", n.UnitRates);
                Edititem.Add("@RecorderQ", n.RecorderQuantity);
                Edititem.Add("@minQ", n.MinQuantity);
                Edititem.Add("@Addedby", n.StaffCode ?? "");
                Edititem.Add("@ExpiryDays", n.ExpiryDays);
                Edititem.Add("@IsQuality", n.ISQualityBit);

                await obj.ExecuteStoredProcedure("InventoryProcedure", Edititem);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error inserting item: " + ex.Message);

                throw new Exception("Error while inserting item in DB", ex);
            }
        }
        /// <summary>
        /// Deletes inspection/quality parameters by ItemQualityId.
        /// </summary>
        /// <param >ItemQualityId</param>

        public async Task DeleteparaOJ(int id)
        {
            try
            {
                Dictionary<string, object> deletepara = new Dictionary<string, object>();
                deletepara.Add("@Flag", "DeleteParametersOJ");
                deletepara.Add("@ItemQualityId", id);

                await obj.ExecuteStoredProcedure("InventoryProcedure", deletepara);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error delete para: " + ex.Message);

                throw new Exception("Error while deleting para in DB", ex);
            }

        }


        /// <summary>
        /// Retrieves all item categories from the database.
        /// Calls InventoryProcedure with flag = "AllCategorySSG".
        /// </summary>
        /// <returns>A DataSet containing all categories.</returns>
        public async Task<DataSet> GetAllCategoriesSSG()
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("@Flag", "AllCategorySSG");

            DataSet ds = await obj.ExecuteStoredProcedureReturnDS("InventoryProcedure", param);
            return ds;
        }

        /// <summary>
        /// Inserts a new item category into the database.
        /// Calls InventoryProcedure with flag = "InsertCategorySSG".
        /// </summary>
        /// <param name="i">Inventory object containing category details.</param>
        public async Task InsertCategorySSG(InventorySSG i)
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("@Flag", "InsertCategorySSG");
            param.Add("@ItemCategoryName", i.ItemCategoryName);
            param.Add("@Description", i.Description);
            param.Add("@TaxRateId", i.TaxRateId.ToString());
            await obj.ExecuteStoredProcedure("InventoryProcedure", param);
        }
        /// <summary>
        /// Updates an existing item category in the database.
        /// Calls InventoryProcedure with flag = "UpdateCategorySSG".
        /// </summary>
        /// <param name="i">Inventory object containing updated category details.</param>
        public async Task UpdateCategorySSG(InventorySSG i)
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("@Flag", "UpdateCategorySSG");
            param.Add("@ItemCategoryId", i.ItemCategoryId.ToString());
            param.Add("@ItemCategoryName", i.ItemCategoryName);
            param.Add("@Description", i.Description);
            param.Add("@TaxRateId", i.TaxRateId.ToString());
            await obj.ExecuteStoredProcedure("InventoryProcedure", param);
        }

        /// <summary>
        /// Retrieves details of a specific item category by its ID.
        /// Calls InventoryProcedure with flag = "GetCategorySSG".
        /// </summary>
        /// <param name="id">The ID of the item category to fetch.</param>
        /// <returns>An Inventory object containing category details.</returns>
        public async Task<InventorySSG> GetCategorySSG(int id)
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("@Flag", "GetCategorySSG");
            param.Add("@ItemCategoryId", id.ToString());

            DataSet ds = await this.obj.ExecuteStoredProcedureReturnDS("InventoryProcedure", param);

            InventorySSG obj = new InventorySSG();
            if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                DataRow row = ds.Tables[0].Rows[0];
                obj.ItemCategoryId = Convert.ToInt32(row["ItemCategoryId"]);
                obj.ItemCategoryName = row["ItemCategoryName"].ToString();
                obj.Description = row["Description"].ToString();
                obj.TaxRateId = int.Parse(row["TaxRateId"].ToString());
                obj.HSNCode = int.Parse(row["HSNCode"].ToString());
            }

            return obj;
        }

        /// <summary>
        /// Deletes an item category from the database.
        /// Calls InventoryProcedure with flag = "DeleteCategorySSG".
        /// </summary>
        /// <param name="id">The ID of the item category to delete.</param>
        public async Task DeleteCategorySSG(int id)
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("@Flag", "DeleteCategorySSG");
            param.Add("@ItemCategoryId", id.ToString());

            await obj.ExecuteStoredProcedure("InventoryProcedure", param);
        }

        /// <summary>
        /// Retrieves all available tax rates from the database.
        /// Calls InventoryProcedure with flag = "ALLTaxRatesSSG".
        /// </summary>
        /// <returns>A list of AllTaxRates objects containing tax rate details.</returns>
        public async Task<List<AllTaxRates>> ALLTaxRatesSSG()
        {
            Dictionary<String, String> dic = new Dictionary<string, string>();
            dic.Add("@flag", "ALLTaxRatesSSG");
            var ds = await obj.ExecuteStoredProcedureReturnDS("InventoryProcedure", dic);

            List<AllTaxRates> lst = new List<AllTaxRates>();
            foreach (DataRow row in ds.Tables[0].Rows)
            {
                lst.Add(new AllTaxRates
                {
                    TaxRateId = int.Parse(row["TaxRateId"].ToString()),
                    HSNCode = row["HSNCode"].ToString(),
                });
            }
            return lst;
        }
        #endregion Om and Sayali
    }
}

