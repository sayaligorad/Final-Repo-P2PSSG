using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace P2PLibray.Account
{
    public class Account
    {
        public string EmailAddress { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public string Code { get; set; }
        public int DepartmentId { get; set; }
        public string StaffCode { get; set; }
        public string UserName { get; set; }
        public string PhoneNumber { get; set; }

        [Display(Name = "Alternate Number")]
        [StringLength(15, ErrorMessage = "Alternate Number cannot exceed 15 digits.")]
        public string AlternamteNumber { get; set; }
        public string ProfilePhoto { get; set; }
        public string RoleName { get; set; }
        public string MotherName { get; set; }
        public string Department { get; set; }
        public string BloodGroup { get; set; }
        public string Gender { get; set; }
        public DateTime JoiningDate { get; set; }
        public DateTime DateOfBirth { get; set; }
        public bool SameLocation { get; set; }
        public string LocalLocation { get; set; }
        public string LocalLandmark { get; set; }
        public int LocalPincode { get; set; }
        public string ParmanentLocation { get; set; }
        public string ParmanentLandmark { get; set; }
        public int ParmanentPincode { get; set; }
        public string IdCode { get; set; }
        public string Status { get; set; }
        public DateTime AddedDate { get; set; }
        public string AddedBy { get; set; }
        public DateTime EndDate { get; set; }
        public int Count { get; set; }
        public string CountryCode { get; set; }
        public string StateCode { get; set; }
        public int CityId { get; set; }
        public string CountryName { get; set; }
        public string StateName { get; set; }
        public string CityName { get; set; }
        public string ExtraCountryCode { get; set; }
        public string ExtraStateCode { get; set; }
        public int ExtraCityId { get; set; }
        public string ExtraCountryName { get; set; }
        public string ExtraStateName { get; set; }
        public string ExtraCityName { get; set; }
        public List<Permissions> PermissionsData { get; set; } = new List<Permissions>();
        public int RoleId { get; set; }
    }
    public class Permissions
    {
        public string PermissionName { get; set; }
        public int HasPermission { get; set; }
        public string PermissionType { get; set; }
    }
    public class CalendarEventData
    {
        // Common properties for all events
        public string Module { get; set; }
        public string IdCode { get; set; }
        public string Title { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string StatusName { get; set; }

        // Purchase Requisition
        public string PRCode { get; set; }
        public DateTime? RequiredDate { get; set; }
        public string Description { get; set; }
        public string AddedBy { get; set; }
        public DateTime? AddedDate { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public string PriorityName { get; set; }

        // RFQ
        public string RFQCode { get; set; }
        public string DeliveryAddress { get; set; }
        public DateTime? ExpectedDate { get; set; }
        public string AccountantName { get; set; }
        public string AccountantEmail { get; set; }

        // Register Quotation
        public string RegisterQuotationCode { get; set; }
        public string VendorName { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public decimal? ShippingCharges { get; set; }

        // Purchase Order
        public string POCode { get; set; }
        public string BillingAddress { get; set; }
        public string UserName { get; set; }
        public decimal? TotalAmount { get; set; }
        public List<string> TermConditions { get; set; } = new List<string>();

        // GRN
        public string GRNCode { get; set; }
        public string InvoiceCode { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public DateTime? GRNDate { get; set; }
        public string WarehouseId { get; set; }
        public string CompanyAddress { get; set; }
        public DateTime? PODate { get; set; }

        // Goods Return
        public string GoodsReturnCode { get; set; }
        public string FailedQCCode { get; set; }
        public string TransporterName { get; set; }
        public string TransportContactNo { get; set; }
        public string VehicleType { get; set; }
        public string VehicleNo { get; set; }

        // Quality Check
        public string QualityCheckCode { get; set; }
        public string GRNItemsCode { get; set; }
        public int InspectionFrequency { get; set; }
        public long? SampleQualityChecked { get; set; }
        public long? SampleTestFailed { get; set; }
        public string Reason { get; set; }
        public string QCAddedBy { get; set; }
        public string QCFailedAddedBy { get; set; }
        public DateTime? QCAddedDate { get; set; }
        public DateTime? QCFailedDate { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public long? Quantity { get; set; }

        //Material Requirenment Planning
        public string MaterialReqPlanningCode { get; set; }
        public string PlanName { get; set; }
        public int PlanYear { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        // Items table for modules that have multiple items
        public List<ItemData> Items { get; set; } = new List<ItemData>();
    }
    public class ItemData
    {
        public string GRNCode { get; set; }
        public string PRItemCode { get; set; }
        public string PRCode { get; set; }
        public string RFQCode { get; set; }
        public string POCode { get; set; }
        public string POItemCode { get; set; }
        public string RQItemCode { get; set; }
        public string GRNItemCode { get; set; }
        public string GRItemCode { get; set; }
        public string Reason { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public decimal? CostPerUnit { get; set; }
        public int? Discount { get; set; }
        public long? Quantity { get; set; }
        public int? RequiredQuantity { get; set; }
        public decimal? FinalAmount { get; set; }
        public string TaxRate { get; set; }
        public string StatusName { get; set; }
        public int IssueItemsId { get; set; }
    }
    public class CountryDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Iso2 { get; set; }
    }
    public class StateDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Country_Id { get; set; }
        public string Country_Code { get; set; }
        public string Iso2 { get; set; }
    }
    public class CityDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string CountryName { get; set; }
        public string StateName { get; set; }
    }
    public class NotificationProperty
    {
        public int NotificationId { get; set; }
        public string StaffCode { get; set; }
        public string NotificationMessage { get; set; }
        public bool IsRead { get; set; }
    }
    public class Email
    {
        public List<string> ToEmails { get; set; } = new List<string>();
        public string Subject { get; set; }
        public string Body { get; set; }
        public bool IsBodyHtml { get; set; } = true;
        public List<string> CcEmails { get; set; } = new List<string>();
        public List<string> BccEmails { get; set; } = new List<string>();
        public List<string> AttachmentPaths { get; set; } = new List<string>();
    }
}
