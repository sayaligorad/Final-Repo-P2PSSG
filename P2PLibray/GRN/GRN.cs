using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PLibray.GRN
{
    public class GRN
    {
        #region sayali


        public string PODate { get; set; }
        public string VendorName { get; set; }
        public string CompanyAddress { get; set; }
        public string BillingAddress { get; set; }
        public string InvoiceDate { get; set; }
        public string GRNDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string POStatus { get; set; }
        public string QCStatus { get; set; }
        public bool ShowAssignQCButton { get; set; }
        public int StatusID { get; set; }

        // Item-level properties
        public string GRNItemCode { get; set; }
        public string UOMName { get; set; }
        public decimal UnitRate { get; set; }
        public decimal Discount { get; set; }
        public decimal DiscountPercent { get; set; }
        public string GST { get; set; }
        public string Description { get; set; }
        public string IsQuality { get; set; }
        public int WareHouseId { get; set; }
        public string WarehouseName { get; set; }

        // PO / GRN quantity info
        public decimal POQuantity { get; set; }      // ✅ decimal to match DB
        public decimal GRNQuantity { get; set; }
        public decimal RemainingQuantity { get; set; }
        public decimal CostPerUnit { get; set; }

        public string qc { get; set; }

        #endregion sayali

        #region Rushikesh

        public int TotalGRN { get; set; }
        public int TotalItem { get; set; }
        public int GRNNo { get; set; }
        public string GRNCode { get; set; }
        public string POCode { get; set; }
        public string InvoiceNo { get; set; }
        public string Status { get; set; }
        public string AddedBy { get; set; }
        public DateTime? AddedDate { get; set; }
        public string Vendor { get; set; }

        public string QualityCheckCode { get; set; }
        public string ItemName { get; set; }
        public string ItemCode { get; set; }
        public int Quantity { get; set; }
        public int OrderQuantity { get; set; }

        public int RecievedQuantity { get; set; }
        public int PendingQuantity { get; set; }

        public string OrderedBy { get; set; }
        public DateTime? ExpectedDate { get; set; }


        #endregion Rushikesh
    }

    #region Rutik
    // Main ViewModel for GRN Return
    public class GoodsReturnViewModel
    {
        public string GRNo { get; set; }
        public string GRNCode { get; set; }
        public string POCode { get; set; }
        public string VendorName { get; set; }
        public string InvoiceNo { get; set; }
        public DateTime InvoiceDate { get; set; }

        public string CompanyAddress { get; set; }
        public string BillingAddress { get; set; }
        public string ReasonForGoodReturn { get; set; }
        public string AddedBy { get; set; }


        // Line items
        public List<GoodsReturnItemViewModel> Items { get; set; } = new List<GoodsReturnItemViewModel>();

        // Computed total
        public decimal TotalAmount { get; set; }
    }

    // Line Item model
    public class GoodsReturnItemViewModel
    {
        public string ItemName { get; set; }
        public string ItemDescription { get; set; }
        public int Qty { get; set; }
        public string UOM { get; set; }
        public decimal UnitRate { get; set; }
        public decimal Amount { get; set; }

        public string Reason { get; set; }
        public string FailedQCCode { get; set; }
        public int SampleTestFailed { get; set; }
        public int InspectionFrequency { get; set; }
        public int SampleQualityChecked { get; set; }
    }

    public class ReturnGoodsClass
    {
        public string AddedDate { get; set; }

        public string AddedBy { get; set; }

        public string GRNCode { get; set; }
        public string StatusName { get; set; }
        public string FullName { get; set; }
    }



    public class ReturnGood
    {
        public string AddedDate { get; set; }
        public string GoodReturnCode { get; set; }
        public string GRNCode { get; set; }
        public string VenderName { get; set; }
        public string StatusName { get; set; }
        public string InvoiceNo { get; set; }
        public string ReasonOfRejection { get; set; }
    }

    public class PrintDetailsClass
    {
        public string GoodReturnCode { get; set; }
        public string POCode { get; set; }
        public string InvoiceNo { get; set; }
        public string CompanyAddress { get; set; }
        public string BillingAddress { get; set; }
        public List<GoodsReturnItemViewModel> Items { get; set; } = new List<GoodsReturnItemViewModel>();
        public decimal TotalAmount { get; set; }
    }

    public class GoodDispatchModel : GoodsReturnViewModel
    {
        // Primary Key
        //public int Id { get; set; }

        // Dispatch Address
        public string DispatchAddress { get; set; }
        public string GoodReturnCode { get; set; }

        // Transport Details
        public string TransportName { get; set; }

        public string TransportContact { get; set; }

        public string VehicleType { get; set; }

        public string VehicleNo { get; set; }

        // Audit Info
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }

    public class vehicleTypeModel
    {
        public int SubTypeId { get; set; }
        public string SubTypeName { get; set; }

    }

    #endregion
    #region Pravin
    public class GRNPSM
    {
        public string Fromdate { get; set; }
        public string ToDate { get; set; }
        public string AddedDate { get; set; }
        public class GRNChartModelPSM
        {
            public string StatusName { get; set; }
            public int TotalGRN { get; set; }
        }
        public class GRNQualityCheckModelPSM
        {
            public string AddedDate { get; set; }
            public string DayName { get; set; }
            public string StatusName { get; set; }
            public int StatusCount { get; set; }
        }


        public class ReceiveMaterialDTOPSM
        {
            public string GRNCode { get; set; }
            public string InvoiceNo { get; set; }
            public string FullName { get; set; }
            public string VendorName { get; set; }
            public DateTime AddedDate { get; set; }
            public string StatusName { get; set; }
        }
        public class ApprovedPOModelPSM
        {
            public string POCode { get; set; }
            public string VendorName { get; set; }
            public string VendorCompanyName { get; set; }
            public string AddedByName { get; set; }
            public string ApprovedRejectedByName { get; set; }
            public string ApprovedRejectedDate { get; set; }
            public string ItemName { get; set; }
            public string StatusName { get; set; }
        }



        #endregion

      

    }
}
