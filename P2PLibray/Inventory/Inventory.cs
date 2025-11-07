using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PLibray.Inventory
{
        #region Rutik
        // ---------------------- STOCK ----------------------
        public class InventoryStock
        {
            public int TotalCount { get; set; }
            public int LowInStocks { get; set; }
            public int MostInStocks { get; set; }
        }

        public class InventoryStorage
        {
            public int Bin { get; set; }
            public int WareHouse { get; set; }
            public int Rack { get; set; }
        }

        public class InventoryStockDetails
        {
            public int ItemsCounts { get; set; }
            public int QuantityStored { get; set; }
            public int ReorderQuantity { get; set; }
            public string ItemCode { get; set; }
            public string ItemName { get; set; }
            public string BinCode { get; set; }
            public string AddedDate { get; set; }
            public string CreatedDate { get; set; }
        }

        public class InventoryStockGrouped
        {
            public List<InventoryStockDetails> TotalStock { get; set; }
            public List<InventoryStockDetails> LowStock { get; set; }
            public List<InventoryStockDetails> MostStock { get; set; }

            public InventoryStockGrouped()
            {
                TotalStock = new List<InventoryStockDetails>();
                LowStock = new List<InventoryStockDetails>();
                MostStock = new List<InventoryStockDetails>();
            }
        }

        // ---------------------- CATEGORY ----------------------
        public class InventoryCategory
        {
            public int FinishedGoods { get; set; }
            public int SemiFinishedGoods { get; set; }
            public int RawMaterial { get; set; }
            public int DeadStock { get; set; }
        }

        public class InventoryCategoryGrouped
        {
            public List<InventoryStockDetails> FinishedGoods { get; set; } = new List<InventoryStockDetails>();
            public List<InventoryStockDetails> SemiFinishedGoods { get; set; } = new List<InventoryStockDetails>();
            public List<InventoryStockDetails> RawMaterial { get; set; } = new List<InventoryStockDetails>();
            public List<InventoryStockDetails> DeadStock { get; set; } = new List<InventoryStockDetails>();
        }

        // ---------------------- RECEIVE ----------------------
        public class ReceiveMaterialDetail
        {
            public string GRNItemCode { get; set; }
            public string BinCode { get; set; }
            public int ItemsCounts { get; set; }
            public string ItemName { get; set; }
            public string AddedDate { get; set; }
        }

        public class ReciveItemsCountClass
        {
            public int Received { get; set; }
            public int POQuantity { get; set; }
            public string ReceiveDate { get; set; }
            public string POCode { get; set; }
        }

        // ---------------------- ISSUE ----------------------
        public class IssueInHouseDetail
        {
            public string BinCode { get; set; }
            public int ItemsCounts { get; set; }
            public string ItemName { get; set; }
            public string AddedDate { get; set; }
        }

        public class IssueInHouseCountClass
        {
            public int Issue { get; set; }
            public string IssueMonth { get; set; }
        }
    #endregion
    #region Divyani
    public class InventoryDRB
    {
        public string GRNCode { get; set; }
        public string AddedDate { get; set; }

        //public string StatusName { get; set; }

    }

    public class IssueItemViewModelDRB
    {
        public string IssueCode { get; set; }
        public DateTime IssueDate { get; set; }
        public string Department { get; set; }
        public string EmployeeName { get; set; }

        public List<IssueinHouseDRB> Items { get; set; } = new List<IssueinHouseDRB>();
    }

    public class IssueinHouseDRB
    {
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string ItemCategoryName { get; set; }
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Amount { get; set; }
        public string StatusName { get; set; }
    }

    public class IssueHeaderModelDRB
    {
        public string IssueCode { get; set; }
        public string IssueDate { get; set; }
        public string StaffCode { get; set; }




    }

    public class IssueDetailModelDRB
    {
        public string IssueCode { get; set; }
        public string ItemCode { get; set; }
        public string Bincode { get; set; }
        public string Quantity { get; set; }

    }


    public class GRNdetailsDRB
    {
        public string GRNCode { get; set; }
        public DateTime GRNDate { get; set; }
        public string ContactNo { get; set; }
        public string WareHouseCode { get; set; }
        public string WarehouseName { get; set; }
        public DateTime InvoiceDate { get; set; }
        public string InvoiceCode { get; set; }
        public string POCode { get; set; }
        public string POReference { get; set; }
        public string POContactInfo { get; set; }
        public string VendorCode { get; set; }
        public string VendorName { get; set; }
        public List<GRNItemDRB> Items { get; set; }
    }

    public class GRNItemDRB
    {
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string Description { get; set; }
        public string UOMName { get; set; }
        public string ItemCategoryName { get; set; }
        public decimal Quantity { get; set; }
        public decimal QuantityStored { get; set; }
        public decimal Price { get; set; }
    }

    public class InventorySectionDRB
    {
        public string SectionCode { get; set; }
        public string SectionName { get; set; }

    }

    public class InventoryWareHouseDRB
    {
        public string WareHouseCode { get; set; }
        public string WareHouseName { get; set; }
    }

    public class InventoryRackDRB
    {
        public string RackCode { get; set; }
        public string RackName { get; set; }
    }

    public class InventoryRowDRB
    {
        public string RowCode { get; set; }
        public string RowName { get; set; }
    }

    public class InventoryBinDRB
    {
        public string BinCode { get; set; }
        public string BinName { get; set; }
        public string CurrentItems { get; set; }
        public string MaxQuantity { get; set; }
    }

    public class SaveItemBinAssignmentDRB
    {
        public string GRNItemCode { get; set; }
        public string BinCode { get; set; }
        public string QuantityStored { get; set; }
        public string CreatedDate { get; set; }

    }


    #endregion

    #region Lavmesh
    public class InventoryLM
    {
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string ItemCategoryName { get; set; }
        public string UOMName { get; set; }
        public int CurrentQty { get; set; }
        public int ReorderQuantity { get; set; }
        public string AddedDate { get; set; }
        public string QualityCheackCode { get; set; }
        public int ItemCount { get; set; }
        public string Date { get; set; }
        public string Status { get; set; }
        public string ExpiryDate { get; set; }
        public string BinName { get; set; }
        public string BinsList { get; set; }
        public string RowName { get; set; }
        public string RackName { get; set; }
        public int TransferQty { get; set; }
        public string Message { get; set; }
        public string BinCode { get; set; }
    }

    #endregion

    #region Akash
    public class WarehouseUtilizationReport
    {
        public string WareHouseCode { get; set; }
        public string WarehouseName { get; set; }
        public string SectionCode { get; set; }
        public string SectionName { get; set; }
        public int TotalRacks { get; set; }
        public int TotalRows { get; set; }
        public int TotalBins { get; set; }
        public int TotalItemsStored { get; set; }
        public int TotalCapacity { get; set; }
        public int FreeSpace { get; set; }
        public decimal UtilizationPercentage { get; set; }
        public decimal FreeSpacePercentage { get; set; }
    }

    public class StockReport
    {
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string ItemCategoryName { get; set; }
        public decimal UnitRates { get; set; }
        public int CurrentQuantity { get; set; }
        public int MinQuantity { get; set; }
        public int ReorderQuantity { get; set; }
        public string StockStatus { get; set; }
    }

    public class ReceivedReportDTO
    {
        public string ReceiptDate { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string ItemCategoryName { get; set; }
        public int ItemsCount { get; set; }   // TotalReceived
        public string BinCode { get; set; }
        public string WarehouseName { get; set; }
        public string SectionName { get; set; }
    }


    public class ReceivedMaterialReport
    {
        public int ReceiveMaterialId { get; set; }
        public string ReceivedDate { get; set; }

        public string GRNCode { get; set; }
        public string POCode { get; set; }
        public string ItemCode { get; set; }

        public string ItemCategory { get; set; }
        public string ItemName { get; set; }
        public long GRNQuantity { get; set; }
        public long ReceivedQuantity { get; set; }
        public string WarehouseName { get; set; }
        public string BinCode { get; set; }
    }
    public class InhouseTransferReport
    {
        public string InhouseCode { get; set; }
        public string TransferTo { get; set; }
        public string ItemName { get; set; }
        public string ItemCategory { get; set; }
        public string UOM { get; set; }
        public int Quantity { get; set; }
        public string TransferDate { get; set; }
    }

    #endregion

    #region Mayur
    public class InventoryMHB
    {
        public string ItemName { get; set; }
        public string ItemCode { get; set; }
        public string ReorderQuantity { get; set; }
        public string minQuantity { get; set; }
        public int MaxQuantity { get; set; }
        public int CurrentItems { get; set; }
        public string StockStatus { get; set; }


        public string ItemId { get; set; }
        public int Quantity { get; set; }
        public string UOMName { get; set; }
        //public string UOMNameMB { get; set; }
        public string Description { get; set; }
        public DateTime? RequiredDate { get; set; }


        public string ISRQuantity { get; set; }
        public string RequestType { get; set; }
        public string StatusName { get; set; }


        public string PlanName { get; set; }
        public string Year { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public List<InventoryMHB> Items { get; set; }

        public string QuantityMRP { get; set; }

        public string MRPCode { get; set; }

        public List<InventoryMHB> ItemList { get; set; } = new List<InventoryMHB>();


        public string RequiredDates { get; set; }

        public string StaffCode { get; set; }

    }
    #endregion Mayur
    public class Inventory
    {

        #region Rushikesh
        public int StockRequirementId { get; set; }
       // public string AddedDate { get; set; }
        public string Status { get; set; }

        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string Description { get; set; }
        public string RequiredQuantity { get; set; }
        public DateTime RequiredDate { get; set; }
        public string RequestType { get; set; }

        public string PlanName { get; set; }
        public string MRPCode { get; set; }
        public string UOM { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }

        #endregion Rushikesh

        #region Saurabh

        public int CountryId { get; set; }
        public string CountryName { get; set; }
        public string SectionName { get; set; }
        public string SectionCode { get; set; }
        public int SectionId { get; set; }
        public int StateId { get; set; }
        public string StateName { get; set; }
        public int CityId { get; set; }

        public int RackId { get; set; }
        public string RackCode { get; set; }


        public int BinId { get; set; }
        public string BinCode { get; set; }


        public int RowId { get; set; }
        public string RowCode { get; set; }

        public int WareHouseId { get; set; }
        public string WarehouseCode { get; set; }

        public string WarehouseName { get; set; }
        public string Address { get; set; }
        public string CityName { get; set; }
        public DateTime AddedDate { get; set; }
        public string AddedBy { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
    
        public string Descriptions { get; set; }

        public int Capacity { get; set; }


        public string RackNames { get; set; }
        public string RackCodes { get; set; }
        public string RowName { get; set; }
        public string BinName { get; set; }
      //  public string ItemName { get; set; }
        public int MaxQuantity { get; set; }
        public int CurrentItems { get; set; }


        public string RackName { get; set; }
        public string CountryCode { get; set; }
        public string StateCode { get; set; }


        //  public string ItemCode { get; set; }

        #endregion

     //   #region Mayur
     ////   public string ItemName { get; set; }
     ////   public string ItemCode { get; set; }
     //   public string ReorderQuantity { get; set; }
     //   public string minQuantity { get; set; }
     //  // public string MaxQuantity { get; set; }
     // //  public string CurrentItems { get; set; }
     //   public string StockStatus { get; set; }


     //   public string ItemId { get; set; }
     //   public int Quantity { get; set; }
     //   public string UOMName { get; set; }
     // //  public string Description { get; set; }
     //  // public DateTime? RequiredDate { get; set; }


     //   public string ISRQuantity { get; set; }
     //  // public string RequestType { get; set; }
     //   public string StatusName { get; set; }


     //   //public string PlanName { get; set; }
     //   public string Year { get; set; }
     // //  public string FromDate { get; set; }
     //     //public string ToDate { get; set; }
     //   public List<Inventory> Items { get; set; }

     //   public string QuantityMRP { get; set; }

     ////   public string MRPCode { get; set; }

     //   public List<Inventory> ItemList { get; set; } = new List<Inventory>();


     //   public string RequiredDates { get; set; }

     //   public string StaffCode { get; set; }

        //#endregion

        #region Sayali and Om
        public class InventoryOJ
        {
            //public string ItemCategoryName { get; set; }

            public int ItemIdOJ { get; set; } // Added for ItemId

            public string ItemCode { get; set; }
            public string ItemName { get; set; }
            public string UOM { get; set; }          // instead of UOM
            public decimal UnitRates { get; set; }
            public string ItemCategory { get; set; } // instead of ItemCategory
            public string Status { get; set; }       // instead of Status
            public string ISQuality { get; set; }
            public int ISQualityBit { get; set; }     // 0 or 1

            public int MinQuantity { get; set; }

            public int ItemCategoryId { get; set; } // Added for ItemCategoryId

            public int ItemStatusId { get; set; } // Added for ItemStatusId

            public int UOMId { get; set; } // Added for UOMId

            public string Description { get; set; } // Added for Description

            public int RecorderQuantity { get; set; } // Added for RecorderQuantity

            public int ExpiryDays { get; set; } // Added for ExpiryDays

            public int ItemMakeId { get; set; } // Added for ItemMakeId

            public string ItemMake { get; set; } // Added for ItemMake

            public int TaxRateId { get; set; } // Added for TaxRateId

            public int HSNCode { get; set; } // Added for HSNCode

            //public int Quality { get; set; }
            public DateTime Date { get; set; }

            public string ItemQualityCode { get; set; }
            public int InspectionId { get; set; }

            public int QualityParametersId { get; set; }

            public int QuantityParametersId { get; set; }


            public string PQuality { get; set; }
            public int PlanId { get; set; }

            public int ItemQualityId { get; set; }

            public string PlanName { get; set; }

            public string InspectionName { get; set; }

            public int Parameters { get; set; }

            public string ParametersName { get; set; }

            public int PUOMId { get; set; }

            public string PUOMName { get; set; }

            public string PlanCode { get; set; }

            //public string QualityCode { get; set; }

            public string QualityParametersName { get; set; }

            public string PlanDescription { get; set; }

            public string StaffCode { get; set; }
        }

        #endregion
    }

    #region Sayali
    public class InventorySSG
    {
        public int ItemCategoryId { get; set; }
        public string ItemCategoryName { get; set; }
        public string Description { get; set; }

        // Tax / HSN
        public int HSNCode { get; set; }
        public int TaxRateId { get; set; }

    }

    public class AllTaxRates
    {
        public int TaxRateId { get; set; }
        public string HSNCode { get; set; }
    }
    #endregion Sayali

    #region Sourabh

    public class InventorySK
    {
        public int CountryId { get; set; }
        public string CountryName { get; set; }
        public string SectionName { get; set; }
        public string SectionCode { get; set; }
        public int SectionId { get; set; }
        public int StateId { get; set; }
        public string StateName { get; set; }
        public int CityId { get; set; }

        public int RackId { get; set; }
        public string RackCode { get; set; }


        public int BinId { get; set; }
        public string BinCode { get; set; }


        public int RowId { get; set; }
        public string RowCode { get; set; }

        public int WareHouseId { get; set; }
        public string WarehouseCode { get; set; }

        public string WarehouseName { get; set; }
        public string Address { get; set; }
        public string CityName { get; set; }
        public DateTime AddedDate { get; set; }
        public string AddedBy { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Description { get; set; }
        public string Descriptions { get; set; }

        public int Capacity { get; set; }


        public string RackNames { get; set; }
        public string RackCodes { get; set; }
        public string RowName { get; set; }
        public string BinName { get; set; }
        public string ItemName { get; set; }
        public string MaxQuantity { get; set; }
        public int CurrentItems { get; set; }


        public string RackName { get; set; }
        public string CountryCode { get; set; }
        public string StateCode { get; set; }


        public string ItemCode { get; set; }


    }

    #endregion
    public class Fetch
    {
        public string SectionCode { get; set; }
        public int StatusId { get; set; }
        public string StatusName { get; set; }

        public int ItemCategoryId { get; set; }
        public string ItemCategoryName { get; set; }
        public int ItemMakeId { get; set; }
        public string ItemMake { get; set; }
        public int UOMId { get; set; }

        public string UOMName { get; set; } // Added for UOM

        public int PlanTypeId { get; set; } // Added for plantypeid
        public string PlanType { get; set; } // Added for plantypename

        public int InseepctionId { get; set; } // Added for TaxRateId

        public string InsepctionName { get; set; }

        public int Qualitative { get; set; } // Added for Qualitative
        public string QualitativeName { get; set; } // Added for QualitativeName

        public int QuantitativeId { get; set; }

        public string QuantitativeName { get; set; } // Added for QuantitativeName


       
       



    }
}
