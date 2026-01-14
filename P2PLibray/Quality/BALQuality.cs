using P2PHelper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PLibray.Quality
{
    public class BALQuality
    {
        MSSQL obj = new MSSQL();


        #region Prashant
        //Confirm non confirm item count method//
        public async Task<SqlDataReader> GetStatusWiseQualityAsyncPR(string startDate = null, string endDate = null)
        {
            var param = new Dictionary<string, string>
    {
        { "@Flag", "countPR" }
    };

            if (!string.IsNullOrEmpty(startDate)) param.Add("@StartDate", startDate);
            if (!string.IsNullOrEmpty(endDate)) param.Add("@EndDate", endDate);

            return await obj.ExecuteStoredProcedureReturnDataReader("QualityCheckProcedure", param);
        }


        //GRN count method//



        public async Task<SqlDataReader> GRNAllPR(string startDate = null, string endDate = null)
        {
            var param = new Dictionary<string, string>
    {
        { "@Flag", "GRNCOUNTPR" }
    };

            if (!string.IsNullOrEmpty(startDate)) param.Add("@StartDate", startDate);
            if (!string.IsNullOrEmpty(endDate)) param.Add("@EndDate", endDate);

            return await obj.ExecuteStoredProcedureReturnDataReader("QualityCheckProcedure", param);
        }




        //Confirm GRN list method//
        public async Task<List<QualityConfirmItemPR>> ConfirmItemGrnPSR()
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("@Flag", "ConfirmItemPR");

            SqlDataReader dr = await obj.ExecuteStoredProcedureReturnDataReader("QualityCheckProcedure", param);

            List<QualityConfirmItemPR> list = new List<QualityConfirmItemPR>();

            if (dr.HasRows)
            {
                while (await dr.ReadAsync())
                {
                    list.Add(new QualityConfirmItemPR
                    {
                        GRNCode = dr["GRNCode"].ToString(),
                        VenderName = dr["VenderName"].ToString(),
                        AddDate = dr["AddDate"] == DBNull.Value ? "" : Convert.ToDateTime(dr["AddDate"]).ToString("dd/MM/yyyy"),
                        QualityCheckDate = dr["QualityCheckDate"] == DBNull.Value ? "" : Convert.ToDateTime(dr["QualityCheckDate"]).ToString("dd/MM/yyyy")
                    });
                }
            }

            return list;
        }



        // New method to get confirmed item details by GRNCode

        public async Task<List<ConfirmedItemDetailPSR>> ConfirmItemPR(string grnCode)
        {
            Dictionary<string, string> param = new Dictionary<string, string>
    {
        { "@Flag", "ConfirmItemDtailsPR" },
        { "@GRNCode", grnCode }  // Passing GRNCode to filter
    };

            SqlDataReader dr = await obj.ExecuteStoredProcedureReturnDataReader("QualityCheckProcedure", param);

            List<ConfirmedItemDetailPSR> list = new List<ConfirmedItemDetailPSR>();

            if (dr.HasRows)
            {
                while (await dr.ReadAsync())
                {
                    list.Add(new ConfirmedItemDetailPSR
                    {
                        ItemCode = dr["ItemCode"].ToString(),
                        ItemName = dr["ItemName"].ToString(),
                        ItemAddedDate = dr["ItemAddedDate"] == DBNull.Value
               ? ""
               : Convert.ToDateTime(dr["ItemAddedDate"]).ToString("dd/MM/yyyy"),
                        QualityCheckDate = dr["QualityCheckDate"] == DBNull.Value
               ? ""
               : Convert.ToDateTime(dr["QualityCheckDate"]).ToString("dd/MM/yyyy"),
                        GRNCode = dr["GRNCode"].ToString() // if you want to pass it

                    });
                }
            }

            dr.Close();
            return list;
        }







		// Non confirm item GRN list method
		public async Task<List<QualityNonConfirmItemPR>> NonConfirmItemGrnPR()
		{
			List<QualityNonConfirmItemPR> list = new List<QualityNonConfirmItemPR>();
			try
			{
				Dictionary<string, string> param = new Dictionary<string, string>();
				param.Add("@Flag", "Non-ConfirmItemGRNPR");

				SqlDataReader dr = await obj.ExecuteStoredProcedureReturnDataReader("QualityCheckProcedure", param);

				if (dr.HasRows)
				{
					while (await dr.ReadAsync())
					{
						list.Add(new QualityNonConfirmItemPR
						{
							GRNCode = dr["GRNCode"].ToString(),
							VenderName = dr["VenderName"].ToString(),
							AddDate = dr["Add Date"] == DBNull.Value
								? ""
								: Convert.ToDateTime(dr["Add Date"]).ToString("dd/MM/yyyy"),
							QualityCheckDate = dr["Quality Check Date"] == DBNull.Value
								? ""
								: Convert.ToDateTime(dr["Quality Check Date"]).ToString("dd/MM/yyyy"),
							QCFailedDate = dr["QC Failed Date"] == DBNull.Value
								? ""
								: Convert.ToDateTime(dr["QC Failed Date"]).ToString("dd/MM/yyyy")
						});
					}
				}
			}
			catch (Exception ex)
			{
				// Log error or debug here
				throw new Exception("Error in NonConfirmItemListAsync: " + ex.Message);
			}

			return list;
		}





		// Non-confirm item details method by GRNCode

		public async Task<List<FailedItemDetailPR>> NonConfirmItemListPR(string grnCode)
        {
            Dictionary<string, string> param = new Dictionary<string, string>
    {
        { "@Flag", "FaildItemPR" },
        { "@GRNCode", grnCode }
    };

            SqlDataReader dr = await obj.ExecuteStoredProcedureReturnDataReader("QualityCheckProcedure", param);

            List<FailedItemDetailPR> list = new List<FailedItemDetailPR>();

            if (dr.HasRows)
            {
                while (await dr.ReadAsync())
                {
                    list.Add(new FailedItemDetailPR
                    {
                        GRNCode = dr["GRNCode"].ToString(),
                        FailedQCCode = dr["FailedQCCode"].ToString(),
                        ItemCode = dr["Itemcode"].ToString(),
                        ItemName = dr["ItemName"].ToString(),
                        Reason = dr["Reason"].ToString(),
						AddedDate = dr["QalityCheackDate"] == DBNull.Value
								? ""
								: Convert.ToDateTime(dr["QalityCheackDate"]).ToString("dd/MM/yyyy")
					});
                }
            }

            dr.Close();
            return list;
        }







        //GRN show list method//
        public async Task<List<GRNShowItemPR>> GRNShowListAsyncPR()

        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("@flag", "GRNShowPR");

            SqlDataReader dr = await obj.ExecuteStoredProcedureReturnDataReader("QualityCheckProcedure", param);

            List<GRNShowItemPR> list = new List<GRNShowItemPR>();

            if (dr.HasRows)
            {
                while (await dr.ReadAsync())
                {
                    list.Add(new GRNShowItemPR
                    {
                        GRNCode = dr["GRNCode"].ToString(),
                        POCode = dr["POCode"].ToString(),
                        StatusName = dr["StatusName"].ToString(),
                        AddedDate = dr["AddedDate"] == DBNull.Value
                                    ? ""
                                    : Convert.ToDateTime(dr["AddedDate"]).ToString("dd/MM/yyyy")
                    });
                }
            }

            return list;
        }




		// For Confirmed Items
		public async Task<List<ConfirmedItemDetailPSR>> ConfirmItemDetailsPSR(DateTime? startDate = null, DateTime? endDate = null)
		{
			Dictionary<string, string> param = new Dictionary<string, string>
	{
		{ "@Flag", "ConfirmItemDtailsPSR" },
		{ "@StartDate", startDate?.ToString("yyyy/MM/dd") },
		{ "@EndDate", endDate?.ToString("yyyy/MM/dd") }
	};

			SqlDataReader dr = await obj.ExecuteStoredProcedureReturnDataReader("QualityCheckProcedure", param);

			List<ConfirmedItemDetailPSR> list = new List<ConfirmedItemDetailPSR>();

			if (dr.HasRows)
			{
				while (await dr.ReadAsync())
				{
					list.Add(new ConfirmedItemDetailPSR
					{
						GRNCode = dr["GRNCode"].ToString(),
						ItemCode = dr["ItemCode"].ToString(),
						ItemName = dr["ItemName"].ToString(),
						ItemAddedDate = dr["ItemAddedDate"] == DBNull.Value
							? ""
							: Convert.ToDateTime(dr["ItemAddedDate"]).ToString("dd/MM/yyyy"),
						QualityCheckDate = dr["QualityCheckDate"] == DBNull.Value
							? ""
							: Convert.ToDateTime(dr["QualityCheckDate"]).ToString("dd/MM/yyyy")

					});
				}
			}

			dr.Close();
			return list;
		}



		// For Failed Items
		public async Task<List<FailedItemDetailPR>> GetFailedItemsPR(DateTime? startDate = null, DateTime? endDate = null)
		{
			Dictionary<string, string> param = new Dictionary<string, string>()
	{
		{ "@Flag", "GraphFaildItemPR" },
		{ "@StartDate", startDate?.ToString("yyyy/MM/dd") },
		{ "@EndDate", endDate?.ToString("yyyy/MM/dd") }
	};

			SqlDataReader dr = await obj.ExecuteStoredProcedureReturnDataReader("QualityCheckProcedure", param);

			List<FailedItemDetailPR> list = new List<FailedItemDetailPR>();

			if (dr.HasRows)
			{
				while (await dr.ReadAsync())
				{
					list.Add(new FailedItemDetailPR
					{
						GRNCode = dr["GRNCode"].ToString(),  // Ye naya line add karein
						ItemCode = dr["ItemCode"].ToString(),
						ItemName = dr["ItemName"].ToString(),
						FailedQCCode = dr["FailedQCCode"].ToString(),
						Reason = dr["Reason"].ToString(),
						AddedDate = dr["AddedDate"] == DBNull.Value
							? ""
							: Convert.ToDateTime(dr["AddedDate"]).ToString("dd/MM/yyyy")
					});
				}
			}

			dr.Close();
			return list;
		}


		//pending iem method 
		public async Task<SqlDataReader> GetPendingItemsAsyncPR(string startDate = null, string endDate = null)
		{
			var param = new Dictionary<string, string>
	{
		{ "@Flag", "PendingItem" }
	};

			if (!string.IsNullOrEmpty(startDate))
				param.Add("@StartDate", startDate);
			if (!string.IsNullOrEmpty(endDate))
				param.Add("@EndDate", endDate);

			return await obj.ExecuteStoredProcedureReturnDataReader("QualityCheckProcedure", param);
		}

        #endregion Prashant






		#region Rajlaxmi
		/// <summary>
		/// Retrieves all GRN items for quality check grid (RG view).
		/// </summary>
		/// <returns>List of <see cref="Quality"/> with GRN details.</returns>
		public async Task<List<Quality>> AllItemCheckGridRG()
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("@Flag", "AllQualityGRNItemRG");


            var ds = await obj.ExecuteStoredProcedureReturnDS("QualityCheckProcedure", dic);
            List<Quality> lst = new List<Quality>();

            foreach (DataRow row in ds.Tables[0].Rows)
            {
                lst.Add(new Quality
                {
                    GRNCode = row["GRNCode"].ToString(),
                    strAddedDate = row["AddedDate"] != DBNull.Value
                        ? Convert.ToDateTime(row["AddedDate"]).ToString("dd-MM-yyyy")  //  return dd-MM-yyyy
                        : string.Empty,
                    POcode = row["POCode"].ToString(),

                });
            }
            return lst;
        }




        /// <summary>
        /// Retrieves item details by GRN code for RG inspection.
        /// </summary>
        /// <param name="id">GRN Code</param>
        /// <returns>List of <see cref="Quality"/> items linked to the GRN.</returns>
        public async Task<List<Quality>> ItemByGRNCodeRG()
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("@Flag", "ItemsBYGRNCodeRG");

            var ds = await obj.ExecuteStoredProcedureReturnDS("QualityCheckProcedure", dic);
            List<Quality> lst = new List<Quality>();
            foreach (DataRow row in ds.Tables[0].Rows)
            {
                lst.Add(new Quality
                {
                    GRNCode = row["GRNCode"].ToString(),
                    ItemCode = row["ItemCode"].ToString(),
                    GrnItemCode = row["GRNItemcode"].ToString(),
                    ItemName = row["ItemName"].ToString(),
                    ItemType = row["ItemType"].ToString(),
                    InspectionType = row["InspectionType"].ToString(),
                    PlanName = row["PlanName"].ToString(),
                    strAddedDate = row["AssignedDate"] != DBNull.Value
                    ? Convert.ToDateTime(row["AssignedDate"]).ToString("dd-MM-yyyy") : string.Empty,

                });
            }
            return lst;


        }



        /// <summary>
        /// Retrieves inspection details form for a specific item.
        /// </summary>
        /// <param name="id">Item Code</param>
        /// <returns>List of <see cref="Quality"/> with inspection plan and parameters.</returns>
        public async Task<List<Quality>> InspecdetailsFormRG(string id)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("@Flag", "InspecdetailsFormRG");
            dic.Add("@GRNItemCode", id);

            var ds = await obj.ExecuteStoredProcedureReturnDS("QualityCheckProcedure", dic);
            List<Quality> lst = new List<Quality>();
            foreach (DataRow row in ds.Tables[0].Rows)
            {
                lst.Add(new Quality
                {
                    GRNCode = row["GRNCode"].ToString(),
                    ItemCode = row["ItemCode"].ToString(),
                    ItemName = row["ItemName"].ToString(),
                    ItemType = row["ItemType"].ToString(),
                    InspectionType = row["InspectionType"].ToString(),
                    PlanName = row["PlanName"].ToString(),
                    strAddedDate = row["AssignedDate"] != DBNull.Value
                    ? Convert.ToDateTime(row["AssignedDate"]).ToString("dd/MM/yyyy") : string.Empty,
                    Parameters = row["Parametersc"].ToString(),
                    Quantity = int.Parse(row["Quantity"].ToString()),
                    GrnItemCode = row["GRNItemcode"].ToString(),

                });
            }
            return lst;

        }




        /// <summary>
        /// Retrieves parameter list for a given item.
        /// </summary>
        /// <param name="id">Item Code</param>
        /// <returns>List of <see cref="Quality"/> with parameter data.</returns>
        public async Task<List<Quality>> ParametertableRG(string id)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("@Flag", "ParametertableRG");
            dic.Add("@ItemCode", id);

            var ds = await obj.ExecuteStoredProcedureReturnDS("QualityCheckProcedure", dic);
            List<Quality> lst = new List<Quality>();
            foreach (DataRow row in ds.Tables[0].Rows)
            {
                lst.Add(new Quality
                {
                    ItemCode = row["ItemCode"].ToString(),
                    Parameters = row["parameter"].ToString(),

                });
            }
            return lst;


        }


        /// <summary>
        /// Generates the next sequential Quality Check (QC) code.
        /// </summary>
        /// <returns>New QC code (e.g., QC001, QC002).</returns>
        public async Task<string> GenerateNextQCCode()
        {

            Dictionary<string, string> para = new Dictionary<string, string>();
            para.Add("@Flag", "GenerateQualityCheckCode");

            DataSet ds = await obj.ExecuteStoredProcedureReturnDS("QualityCheckProcedure", para);

            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                string lastCode = dt.Rows[0]["QualityCheckCode"].ToString();
                int number = int.Parse(lastCode.Substring(2));
                string nextCode = "QC" + (number + 1).ToString("D3");

                return nextCode;
            }
            else
            {
                return "QC001";
            }

        }


        /// <summary>
        /// Confirms the inspection status for a GRN item and inserts a new QC record.
        /// </summary>
        /// <param name="id">GRN Item Code</param>
        /// <param name="sqc">Sample quantity checked</param>
        /// <param name="Inf">Inspection frequency</param>
        public async Task ConfirmBtnInsStatusRG(string id, string sqc, string Inf, string staffcode)
        {
            //var newQCcode =await GenerateNextQCCode();
            Dictionary<string, object> dc = new Dictionary<string, object>();

            dc.Add("@Flag", "ConFirmBtnInsStatusRG");
            dc.Add("@GRNItemCode", id);
            dc.Add("@statusId", 14);
            dc.Add("@Inspectionfrequency", Inf);
            dc.Add("@AddedBy", staffcode);
            dc.Add("@AddedDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            dc.Add("@SampleQualityChecked", sqc);
            await obj.ExecuteStoredProcedure("QualityCheckProcedure", dc);
        }



        /// <summary>
        /// Retrieves the Quality Check Code (QCCode) for a given GRN Item.
        /// Executes the stored procedure 'QualityCheckProcedure' with the flag 'GetQCRRG'
        /// and returns the corresponding QualityCheckCode from the database.
        /// </summary>
        /// <param name="GRNICode">The GRN Item Code used to look up the Quality Check Code.</param>
        /// <returns>
        /// A <see cref="string"/> containing the Quality Check Code if found; 
        /// otherwise, an empty string.
        /// </returns>

        public async Task<string> GetQcCodeRG(string GRNICode)
        {
            Dictionary<string, string> dc = new Dictionary<string, string>();
            string code = "";
            dc.Add("@Flag", "GetQCRRG");
            dc.Add("@GRNItemCode", GRNICode);

            SqlDataReader dr = await obj.ExecuteStoredProcedureReturnDataReader("QualityCheckProcedure", dc);
            while (dr.Read())
            {
                code = dr["QualityCheckCode"].ToString();
            }
            return code;


        }


        /// <summary>
        /// Initiates inspection for a GRN item with a given QC code.
        /// </summary>
        /// <param name="QC">Quality Check Code</param>
        /// <param name="GRNICode">GRN Item Code</param>
        /// <param name="INF">Inspection frequency</param>
        /// <param name="SQC">Sample quality checked</param>

        public async Task InitiatebtnstatusRG(string GRNICode, string INF, string SQC, string staffcode)
        {

            Dictionary<string, object> dc = new Dictionary<string, object>();

            dc.Add("@Flag", "InitiatebtnstatusRG");
            //dc.Add("@QCCode", QC);
            dc.Add("@GRNItemCode", GRNICode);
            dc.Add("@statusId", 15);
            dc.Add("@Inspectionfrequency", int.Parse(INF));
            dc.Add("@AddedBy", staffcode);
            dc.Add("@AddedDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            dc.Add("@SampleQualityChecked", long.Parse(SQC));
            await obj.ExecuteStoredProcedure("QualityCheckProcedure", dc);
        }





        /// <summary>
        /// Generates the next sequential Failed Quality Check (FQC) code.
        /// </summary>
        /// <returns>New Failed QC code (e.g., FQITM001, FQITM002).</returns>
        public async Task<string> GenerateNextNCcode()
        {

            Dictionary<string, string> para = new Dictionary<string, string>();
            para.Add("@Flag", "GenerateFailedQCCode");

            DataSet ds = await obj.ExecuteStoredProcedureReturnDS("QualityCheckProcedure", para);

            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                string lastCode = dt.Rows[0]["FailedQCCode"].ToString();

                int number = int.Parse(lastCode.Substring(5));
                string nextCode = "FQITM" + (number + 1).ToString("D3");

                return nextCode;
            }
            else
            {
                return "FQITM001";
            }

        }

        /// <summary>
        /// Retrieves non-confirmation form details for an item.
        /// </summary>
        /// <param name="id">Item Code</param>
        /// <returns>List of <see cref="Quality"/> with failure details.</returns>

        public async Task<List<Quality>> NonConfirmformRG(string id)
        {

            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("@Flag", "NonConfirmFormRG");
            dic.Add("@ItemCode", id);

            var ds = await obj.ExecuteStoredProcedureReturnDS("QualityCheckProcedure", dic);
            List<Quality> lst = new List<Quality>();
            foreach (DataRow row in ds.Tables[0].Rows)
            {
                lst.Add(new Quality
                {

                    GRNCode = row["GRNCode"].ToString(),
                    GrnItemCode = row["GRNItemcode"].ToString(),
                    ItemCode = row["ItemCode"].ToString(),
                    ItemName = row["ItemName"].ToString(),
                    InspectionType = row["InspectionType"].ToString(),
                    PlanName = row["PlanName"].ToString(),

                });
            }
            return lst;

        }

        /// <summary>
        /// Inserts failed quality check item details into the database.
        /// </summary>
        /// <param name="FQC">Failed QC Code</param>
        /// <param name="QC">Quality Check Code</param>
        /// <param name="STF">Sample Tested Failed</param>
        /// <param name="ROR">Reason of Rejection</param>

        public async Task insertQFitemsRG(string FQC, string QC, string STF, string ROR, string staffcode)
        {

            Dictionary<string, object> dc = new Dictionary<string, object>();

            dc.Add("@Flag", "insertQFitemsRG");
            dc.Add("@FailedQCCode", FQC);
            dc.Add("@QualityheckCode", QC);
            dc.Add("@SampleTestedFailed", long.Parse(STF));
            dc.Add("@Reason", ROR);
            dc.Add("@AddedBy", staffcode);
            dc.Add("@AddedDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            await obj.ExecuteStoredProcedure("QualityCheckProcedure", dc);
        }


        /// <summary>
        /// Retrieves completed tasks for a GRN code.
        /// </summary>
        /// <param name="id">GRN Code</param>
        /// <returns>List of <see cref="Quality"/> with inspection completion details.</returns>
        public async Task<List<Quality>> CompletedTaskRG(string id)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("@Flag", "CompletedTaskRG");
            dic.Add("@GRNCode", id);

            var ds = await obj.ExecuteStoredProcedureReturnDS("QualityCheckProcedure", dic);
            List<Quality> lst = new List<Quality>();
            foreach (DataRow row in ds.Tables[0].Rows)
            {
                lst.Add(new Quality
                {
                    GRNCode = row["GRNCode"].ToString(),
                    ItemCode = row["ItemCode"].ToString(),
                    ItemName = row["ItemName"].ToString(),
                    ItemType = row["ItemType"].ToString(),
                    Quantity = int.Parse(row["Quantity"].ToString()),
                    VendorName = row["VenderName"].ToString(),
                    // Format AssignedDate to "dd-MM-yyyy"
                    AssignedDate = row["AssignedDate"] != DBNull.Value
                ? Convert.ToDateTime(row["AssignedDate"]).ToString("dd-MM-yyyy")
                : string.Empty,

                    // Format InspectionDate to "dd-MM-yyyy"
                    InspDate = row["InspectionDate"] != DBNull.Value
                ? Convert.ToDateTime(row["InspectionDate"]).ToString("dd-MM-yyyy")
                : string.Empty,
                    Status = row["Status"].ToString(),   //  fix here
                });
            }
            return lst;
        }


        /// <summary>
        /// Updates the status of a GRN after inspection.
        /// </summary>
        /// <param name="id">GRN Code</param>
        public async Task UpdateGRNStatusRG(string id)
        {
            Dictionary<string, object> dic = new Dictionary<string, object>();
            dic.Add("@Flag", "UpdateGRNStatusRG");
            dic.Add("@GRNCode", id);
            await obj.ExecuteStoredProcedure("QualityCheckProcedure", dic);
        }

        #endregion Rajlaxmi


        #region Nurjha

        /// <summary>
        /// Retrieves the dashboard counts (Pending / Completed / In-Process) for NAM.
        /// </summary>
        /// <param name="startDate">Optional start date filter.</param>
        /// <param name="endDate">Optional end date filter.</param>
        /// <returns>A <see cref="Quality"/> object containing Pending, Completed, and In-Process counts.</returns>
        public async Task<Quality> GetConfirmCountNAM(DateTime? startDate, DateTime? endDate)
        {
            var parameters = new Dictionary<string, string>
    {
        { "@Flag", "ConfirNonConCountNAM" },
        { "@StartDate", startDate?.ToString("yyyy-MM-dd") },
        { "@EndDate", endDate?.ToString("yyyy-MM-dd") }
    };

            using (var rdr = await obj.ExecuteStoredProcedureReturnDataReader("QualityCheckProcedure", parameters))
            {
                var q = new Quality();
                if (await rdr.ReadAsync())
                {
                    q.PendingCount = rdr["PendingCount"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["PendingCount"]);
                    q.ConfirmCount = rdr["ConfirmedCount"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["ConfirmedCount"]);
                    q.NonConfirmCount = rdr["NonConfirmedCount"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["NonConfirmedCount"]);
                }


                return q;
            }
        }
        /// <summary>
        /// Retrieves the list of Completed GRN items for NAM.
        /// </summary>
        /// <returns>A list of <see cref="GRNItemsList"/> containing Completed GRN data.</returns>
        public async Task<List<Quality>> GetCompletedListNAM()
        {
            var ds = await obj.ExecuteStoredProcedureReturnDS("QualityCheckProcedure", new Dictionary<string, string>
            {
                { "@Flag", "CompletedCountNAM" }
            });

            var list = new List<Quality>();
            if (ds != null && ds.Tables.Count > 0)
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    list.Add(new Quality
                    {
                        GRNNo = row["GRNNo"].ToString(),
                        GRNCode = row["GRNCode"].ToString(),
                        strAddedDate = Convert.ToDateTime(row["AddedDate"]).ToString("yyyy-MM-dd"),
                        AddedBy = row["AddedBy"].ToString(),
                        StatusName = row["StatusName"].ToString()
                    });
                }
            }
            return list;
        }





        /// <summary>
        /// Retrieves the list of Confirmed GRN items for NAM.
        /// </summary>
        /// <returns>A list of <see cref="GRNItemsList"/> containing Confirmed GRN data.</returns>
        public async Task<List<Quality>> GetConfirmedListNAM(DateTime? startDate, DateTime? endDate)
        {
            var ds = await obj.ExecuteStoredProcedureReturnDS("QualityCheckProcedure", new Dictionary<string, string>
            {
                { "@Flag", "ConfirmedListNAM" },
                { "@StartDate", startDate?.ToString("yyyy-MM-dd") },
        { "@EndDate", endDate?.ToString("yyyy-MM-dd") }
            });

            var list = new List<Quality>();
            if (ds != null && ds.Tables.Count > 0)
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    list.Add(new Quality
                    {
                        QualityCheckCode = row["QualityCheckCode"].ToString(),
                        ItemName = row["ItemName"].ToString(),
                        StatusName = row["StatusName"].ToString(),
                        Quantity = row["Quantity"] != DBNull.Value
                           ? Convert.ToInt32(row["Quantity"])
                           : 0,

                        AddedDate = Convert.ToDateTime(row["AddedDate"]).ToString("yyyy-MM-dd"),
                        AddedBy = row["AddedBy"].ToString()
                    });
                }
            }
            return list;
        }
        public async Task<List<Quality>> GetNonConfirmedListNAM(DateTime? startDate, DateTime? endDate)
        {
            var ds = await obj.ExecuteStoredProcedureReturnDS("QualityCheckProcedure", new Dictionary<string, string>
            {
                { "@Flag", "NonConfirmedListNAM" },

                { "@StartDate", startDate?.ToString("yyyy-MM-dd") },
        { "@EndDate", endDate?.ToString("yyyy-MM-dd") }




        });

            var list = new List<Quality>();
            if (ds != null && ds.Tables.Count > 0)
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    list.Add(new Quality
                    {
                        QualityCheckCode = row["QualityCheckCode"].ToString(),
                        ItemName = row["ItemName"].ToString(),
                        StatusName = row["StatusName"].ToString(),
                        Quantity = row["Quantity"] != DBNull.Value
                           ? Convert.ToInt32(row["Quantity"])
                           : 0,
                        AddedDate = Convert.ToDateTime(row["AddedDate"]).ToString("yyyy-MM-dd"),
                        AddedBy = row["AddedBy"].ToString()
                    });
                }
            }
            return list;



        }




        public async Task<List<Quality>> GetPendingListNAM(DateTime? startDate, DateTime? endDate)
        {
            var ds = await obj.ExecuteStoredProcedureReturnDS("QualityCheckProcedure", new Dictionary<string, string>
    {
        { "@Flag", "PendingCountListNAM" },

                { "@StartDate", startDate?.ToString("yyyy-MM-dd") },
        { "@EndDate", endDate?.ToString("yyyy-MM-dd") }// ✅ Flag for Pending records
    });

            var list = new List<Quality>();

            if (ds != null && ds.Tables.Count > 0)
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    list.Add(new Quality
                    {
                        QualityCheckCode = row["QualityCheckCode"].ToString(),
                        ItemName = row["ItemName"].ToString(),
                        StatusName = row["StatusName"].ToString(),
                        Quantity = row["Quantity"] != DBNull.Value
                            ? Convert.ToInt32(row["Quantity"])
                            : 0,
                        AddedDate = Convert.ToDateTime(row["AddedDate"]).ToString("yyyy-MM-dd"),
                        AddedBy = row["AddedBy"].ToString()
                    });
                }
            }

            return list;
        }
        #endregion
    }
}


