using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PLibray.Purchase
{
        #region Pravin
        public class PurchasePSM
        {
            // item details (for GetItemsByPRCode)
            public string ItemCode { get; set; }
            public string ItemName { get; set; }
            public string Description { get; set; }
            public string UOM { get; set; }
            public int RequiredQuantity { get; set; }
            public decimal Amount { get; set; }
            public string RequiredDate { get; set; }

            // header / list
            public string PRCode { get; set; }
            public string ApproveRejectedByName { get; set; }
            public string ApproveRejectedDate { get; set; }
            public string PriorityName { get; set; }
            public string Status { get; set; }

            // for create PR page
            public string AddedBy { get; set; }
            public string AddedDate { get; set; }
            public string Priority { get; set; }
            public string Quantity { get; set; }

        }
        public class CreatePRPSM
        {
            public int PRId { get; set; }
            public string PRCode { get; set; }
            public DateTime RequiredDate { get; set; }
            public int Status { get; set; }
            public string AddedBy { get; set; }
            public DateTime AddedDate { get; set; }
            public string ApproveRejectedBy { get; set; }
            public DateTime? ApproveRejectedDate { get; set; }
            public int PriorityId { get; set; }

            public string Description { get; set; }

            // List of items
            public List<PRItemPSM> Items { get; set; } = new List<PRItemPSM>();
        }
        public class PRItemPSM
        {
            public string PRItemCode { get; set; }
            public string PRCode { get; set; }
            public string ItemCode { get; set; }
            public decimal RequiredQuantity { get; set; }
        }

    #endregion
    public class JITItem
    {
        public string ItemCode { get; set; }
        public int StockRequirementId { get; set; }
    }


    #region Akash
    public class PurchaseAMG
    {
        public string RFQCode { get; set; }
        public string RegisterQuotationCode { get; set; }
        public string AddedDate { get; set; }
        public string VenderName { get; set; }
        public string CompanyName { get; set; }
        public string TotalAmount { get; set; }

        public string ExpectedDate { get; set; }
        public string VendorDeliveryDate { get; set; }
        public string DeliverySpeed { get; set; }
        public string AffordableRank { get; set; }
        public string RecommendedQuotation { get; set; }
    }


    //Properties for Quotation Header Details
    public class PendingQuotViewHeader
    {
        public string RFQCode { get; set; }
        public string RegisterQuotationCode { get; set; }
        public string VQDate { get; set; }
        public string VendorDeliveryDate { get; set; }
        public string ShippingCharges { get; set; }
        public string VenderName { get; set; }
        public string VendorCode { get; set; }
        public string CompanyName { get; set; }
    }
    //Properties Quotations details Items Fetch
    public class PendingQuotViewItems
    {
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }
        public decimal CostPerUnit { get; set; }
        public decimal GrossAmount { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal NetAmount { get; set; }
        public decimal TotalGST { get; set; }
        public decimal GSTAmount { get; set; }
        public decimal FinalAmount { get; set; }
    }
    #endregion


    #region Shubham
    // Purchase header class
    public class PurchaseHeader
        {
            public string PRCode { get; set; }
            public DateTime RequiredDate { get; set; }
            public int Status { get; set; }
            public string AddedBy { get; set; }
            public int PriorityId { get; set; }
            public string Description { get; set; }

            // List of items
            public List<PRItem> Items { get; set; } = new List<PRItem>();
        }

        // Purchase item class
        public class PRItem
        {
            public string PRCode { get; set; }
            public string ItemCode { get; set; }
            public decimal RequiredQuantity { get; set; }
        }

    #endregion

        #region Vaibhavi
    public class RegisterQuotationItem
    {
        public string ItemCode { get; set; }
        public string ItemName { get; set; }

        public decimal Quantity { get; set; }
        public decimal CostPerUnit { get; set; }
        public decimal Discount { get; set; }
        public decimal GST { get; set; }
        public string UOMName { get; set; }
        public string Description { get; set; }
        //public decimal ShippingCharges { get; set; }




    }


    public class RegisterQuotation
    {
        public string RFQCode { get; set; }
        public string RegisterQuotationCode { get; set; }

        public string VendorCode { get; set; }
        public string VendorDeliveryDateVK { get; set; }
        public decimal ShippingCharges { get; set; }
        public List<RegisterQuotationItem> Items { get; set; }

        public string VendorName { get; set; }
        public string CompanyName { get; set; }



    }



    public class ItemGst
    {
        public string ItemCode { get; set; }
        public string HSNCode { get; set; }
        public string CGSTCode { get; set; }
        public string SCGSTCode { get; set; }
        public string IGSTCode { get; set; }
        public decimal TotalGST { get; set; }


        public string ItemName { get; set; }
        public string Category { get; set; }

    }

    public class ApprovedPurchaseOrder
    {
        public string POCode { get; set; }
        public string RegisterQuotationCode { get; set; }
        public string AddedDateVK { get; set; }
        public string ApprovedRejectedDateVK { get; set; }
        public decimal TotalAmount { get; set; }
        public string StatusName { get; set; }
        public string CreatedBy { get; set; }
    }




    public class POHeader
    {
        public string POCode { get; set; }
        public string RegisterQuotationCode { get; set; }
        public string AddedDateVK { get; set; }
        public string ApprovedRejectedDateVK { get; set; }
        public decimal TotalAmount { get; set; }
        public string DeliveryAddress { get; set; }
        public string TermConditionName { get; set; }

        public string VendorCode { get; set; }
        public string VendorName { get; set; }
        public string VendorCompanyName { get; set; }
        public string VendorContact { get; set; }
        public string VendorAddress { get; set; }
        public string InvoiceToCompanyName { get; set; }
        public decimal ShippingCharges { get; set; }


    }

    public class POItem
    {
        public string POItemCode { get; set; }
        public string POCode { get; set; }
        public string RegisterQuotationItemCode { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string Description { get; set; }
        public string UOMName { get; set; }
        public decimal Quantity { get; set; }
        public decimal CostPerUnit { get; set; }
        public decimal Discount { get; set; }
        public decimal GSTPct { get; set; }
        public decimal ShippingCharges { get; set; }


    }


    //nur

    public class PendingPurchaseOrder
    {
        public string POCode { get; set; }
        public string PODateVK { get; set; }
        public decimal POCost { get; set; }
        public string CreatedBy { get; set; }
        public string StatusName { get; set; }
        public string approvedby { get; set; }
    }

    public class POHeaderNAM
    {
        public string POCode { get; set; }
        public string RegisterQuotationCode { get; set; }
        public string AddedDateVK { get; set; }
        public string ApprovedRejectedDateVK { get; set; }
        public decimal TotalAmount { get; set; }
        public string DeliveryAddress { get; set; }
        public string TermConditionName { get; set; }

        public string VendorCode { get; set; }
        public string VendorName { get; set; }
        public string VendorCompanyName { get; set; }
        public string VendorContact { get; set; }
        public string VendorAddress { get; set; }
        public string InvoiceToCompanyName { get; set; }


    }

    public class POItemNAM
    {
        public string POItemCode { get; set; }
        public string POCode { get; set; }
        public string RegisterQuotationItemCode { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string Description { get; set; }
        public string UOMName { get; set; }
        public decimal Quantity { get; set; }
        public decimal CostPerUnit { get; set; }
        public decimal Discount { get; set; }     // percent
        public decimal GSTPct { get; set; }       // percent
        public decimal ShippingCharges { get; set; }       // percent

        
    }


    #endregion
    public class Purchase
    {

        #region prathamesh
        public int AllPR { get; set; }
        public int AllPO { get; set; }
        public int RejectedPO { get; set; }
        public int Rejected { get; set; }
        public int PendingPR { get; set; }
        public int ApprovedPR { get; set; }
        public int ApprovedPO { get; set; }
        public int PendingPO { get; set; }
        public int ApprovedRC { get; set; }
        public int PendingRC { get; set; }
        public int RequestedRFQ { get; set; }
        public int PendingRFQ { get; set; }
        public string StaffCode { get; set; }
        public string WarehouseName { get; set; }
        public string RQCode { get; set; }
        public string VenderName { get; set; }
        public DateTime ExpectedDate { get; set; }
        public string PRCode { get; set; }
        public decimal TotalAmount { get; set; }
        public string FullName { get; set; }
        public string StatusName { get; set; }
        public string Priority { get; set; }
        public DateTime ApprovedRejectedDate { get; set; }

        public string ItemName { get; set; }
        public string UnitRates { get; set; }
        public string Description { get; set; }

        public string ApprovedRejectedDateString { get; set; }


        public String ExpectedDateString { get; set; }

        public String RequiredDateString { get; set; }

        #endregion

        #region Ashutosh
        public DateTime? PRCreatedDate { get; set; }
        public DateTime? PRApprovedDate { get; set; }
        public string ConvertedToRFQ { get; set; }
        public DateTime? RFQCreatedDate { get; set; }
        public string DaysToConvert { get; set; }
        public string POCode { get; set; }
        public string VendorName { get; set; }
        public string VendorCompanyName { get; set; }
        public string AddedByName { get; set; }
        public string ApprovedRejectedByName { get; set; }
        public DateTime? ApprovedRejectedDateAT { get; set; }
        public string RFQCode { get; set; }
         public DateTime? AddedDateAT { get; set; }
        public string RegisterQuotationCode { get; set; }
        public string AddedBy { get; set; }
        public string VendorsInvited { get; set; }
        public string VendorsResponded { get; set; }
        public string ResponseRatePercent { get; set; }
        public string FinalOutcomePOCode { get; set; }
        public string UnitQuantity { get; set; }
        public string TaxRate { get; set; }
        public string FinalAmount { get; set; }
        public string DaysToReceiveQuotation { get; set; }
        public string DaysToApproveQuotation { get; set; }
        public List<Purchase> Items { get; set; }
        #endregion

        #region vaibhavi
        public string AddedDateVK { get; set; }
        public string ExpDate { get; set; }


        public int HasUnregisteredVendors { get; set; }

        public int AnyVendor { get; set; }
        public int HasApproved { get; set; }




        #endregion vaibhavi

        #region Shubham
        public int PRItemId { get; set; }
       // public string PRCode { get; set; }
        public DateTime RequiredDate { get; set; }
      //  public string AddedBy { get; set; }
        //public DateTime AddedDate { get; set; }
        public string ApproveRejectedBy { get; set; }
        public DateTime ApproveRejectedDate { get; set; }
      //  public string Priority { get; set; }
        public string Status { get; set; }
       // public string FullName { get; set; }
        public string ItemCode { get; set; }
        public string PRItemCode { get; set; }
       // public string ItemName { get; set; }
       // public string Description { get; set; }
        public decimal RequiredQuantity { get; set; }
        public string UOM { get; set; }
        public decimal UnitRate { get; set; }
        #endregion

        #region Omkar
        public int SRNO { get; set; }
        //Vendor tbl start
        public int StockReqirementId { get; set; }
        public int VendorId { get; set; }
        public string VendorCode { get; set; }
       // public string VendorName { get; set; }
        public string VendorCompanyCode { get; set; }
        public long MobileNo { get; set; }
        public long AlternateNo { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
       // public string AddedBy { get; set; }

        public DateTime AddedDate { get; set; }
        public string AddedDateString { get; set; }
        public string ApprovedRejectedBy { get; set; }
      //  public DateTime ApprovedRejectedDate { get; set; }
        public int StatusId { get; set; }
        public string ApprovedBy { get; set; }

        public string OriginalApprovedDate { get; set; }
        public DateTime ApprovedDate { get; set; }
        public string CountryCode { get; set; }
        public string StateCode { get; set; }


        //Vendortable end 

        //VendorCompany tbl start
        [DisplayName("Vendor Company")]
        public string CompanyName { get; set; }
        public long CompanyMobileNo { get; set; }
        public long CompanyAlternateNo { get; set; }
        public string CompanyEmail { get; set; }
        public string CompanyAddress { get; set; }
        public int IndustryTypeId { get; set; }
        public string Industry_Type { get; set; }
        public int CityId { get; set; }


        public string QuotationID { get; set; }  //RegisterQuotationCode
       // public string RegisterQuotationCode { get; set; }


        public DateTime VendorDeliveryDate { get; set; }
        public string VendorDeliveryDateString { get; set; }

        //public string Priority { get; set; }
      //  public DateTime RequiredDate { get; set; }
        //public string RequiredDateString { get; set; }
        public decimal ShippingCharges { get; set; }

        public string GST { get; set; }

       // public decimal TotalAmount { get; set; }
        public DateTime QuotationDate { get; set; }
        public string QuotationDateString { get; set; }

        [DisplayName("Vendor Quotation No :")]
        //public string RFQCode { get; set; }

       // public string ItemCode { get; set; }
       // public string ItemName { get; set; }
        //public string Description { get; set; }
        public int Quantity { get; set; }
       // public string UOM { get; set; }
        public decimal CostPerUnit { get; set; }

        public string Discount { get; set; }

        //this Amount is Quantity*Costperunit-Disconunt +GST
        public decimal Amount { get; set; }
        public int BankId { get; set; }
        public string BankName { get; set; }
        public string SwiftCode { get; set; }
        public int BranchId { get; set; }
        public string BranchName { get; set; }
        public string IFSCCode { get; set; }
        public long AccountNumber { get; set; }
        // public int IndustryTypeId {  get; set; }
        public string IndustryType { get; set; }

        public string CityName { get; set; }
        public string StateName { get; set; }
        public string CountryName { get; set; }
        public string BillingAddress { get; set; }
        public string DeliveryAddress { get; set; }
        public string AccountEmail { get; set; }
        //public string StaffCode { get; set; }
        public int TermConditionId { get; set; }
        public string TermConditionName { get; set; }
        public string RegisterQuotationItemCode { get; set; }
        public string TermConditionIds { get; set; }
        public string UserCode { get; set; }
       // public string POCode { get; set; }
        public string Website { get; set; }
        //public string WarehouseName { get; set; }
        public string WarehouseAddress { get; set; }
        public long WarehousePhone { get; set; }
        public string WarehouseEmail { get; set; }
        public List<string> POItems { get; set; } = new List<string>();

        public List<string> Itemslst { get; set; } = new List<string>();
        public decimal GrandTotal { get; set; }
        public decimal SubAmount { get; set; }
        public int RequestTypeId { get; set; }
        public int JITQuantity { get; set; }

        #endregion

        #region Sandesh
        public string ContactPerson { get; set; }
        public string Warehouse { get; set; }
        public string Note { get; set; }
      //  public string SrNo { get; set; }
        public string UOMNamee { get; set; }
        public List<string> Vendors { get; set; }
        #endregion


    }
}
