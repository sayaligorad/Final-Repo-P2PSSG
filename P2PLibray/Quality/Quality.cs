using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PLibray.Quality
{
    public class Quality
    {
        #region Rajlaxmi
        public string Status { get; set; }

        public string strAddedDate { get; set; }
        public string AssignedDate { get; set; }



        public string GRNCode { get; set; }
        public string POcode { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string ItemType { get; set; }
        public string InspectionType { get; set; }
        public string PlanName { get; set; }
        public string Parameters { get; set; }
        public string GrnItemCode { get; set; }

        public string InspDate { get; set; }

        public int Quantity { get; set; }
        public string VendorName { get; set; }
        public string AddedBy { get; set; }



        #endregion Rajlaxmi

        #region Nurjha
        public int InProcessCount { get; set; }
        public int CompletedCount { get; set; }
        public int ConfirmCount { get; set; }
        public int NonConfirmCount { get; set; }
        public int PendingCount { get; set; }
        public string QualityCheckCode { get; set; }
        public string GRNNo { get; set; }
        public string StatusName { get; set; }
        public string AddedDate { get; set; }
        #endregion
    }
    #region Prashant
    public class QualityPR
    {
        public string StatusName { get; set; }
        public int TotalQc { get; set; }
        public int toatalcount { get; set; }
    }

    // Confirmed GRN summary
    public class QualityConfirmItemPR
    {
        public string GRNCode { get; set; }
        public string VenderName { get; set; }
        public string AddDate { get; set; }
        public string QualityCheckDate { get; set; }

    }

    // Detailed confirmed item
    public class ConfirmedItemDetailPSR
    {
        public string GRNCode { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string ItemAddedDate { get; set; }
        public string QualityCheckDate { get; set; }
    }

    // Non-confirmed GRN items (failed QC)
    public class QualityNonConfirmItemPR
    {
        public string GRNCode { get; set; }
        public string Itemcode { get; set; }
        public string ItemName { get; set; }
        public string VenderName { get; set; }
        public string AddDate { get; set; }
        public string QualityCheckDate { get; set; }
        public string QCFailedDate { get; set; }
		public string Reason { get; set; }
    }

    // Detailed failed item info
    public class FailedItemDetailPR
    {
        public string GRNCode { get; set; }
        public string FailedQCCode { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string Reason { get; set; }
        public string AddedDate { get; set; }
	}

    // GRN basic info for display
    public class GRNShowItemPR
    {
        public string GRNCode { get; set; }
        public string POCode { get; set; }
        public string StatusName { get; set; }
        public string AddedDate { get; set; }

		// Model class for Pending Items
		public class PendingItemPR
		{
			public string GRNCode { get; set; }
			public string ItemCode { get; set; }
			public string ItemName { get; set; }
			public string AddedDate { get; set; }
		}
		#endregion Prashant


	}
}
