using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.Common;
using Enterprise.Customs.US.InBond.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using CusInBondMoveDetail = Enterprise.Customs.US.InBond.Business.CusInBondMoveDetail;
using CusInBondMoveHeader = Enterprise.Customs.US.InBond.Business.CusInBondMoveHeader;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.InBond.Module
{
	public class CusInBondHeaderFilterStripBusinessObject : FilterStripBusinessObject
	{
		public static class FilterConstants
		{
			public const string CarrierSCAC = "Carrier SCAC";
			public const string JobReference = "Job Reference";
			public const string InBondNumber = "In-Bond Number";
			public const string ITDate = "IT Date";
			public const string ITType = "In-Bond Entry Type";
			public const string ITCarrier = "IT Carrier - Departure";
			public const string ITCarrierTOL = "IT Carrier - TOL";
			public const string Importer = "Importer";
			public const string LoadingScheduleK = "Loading Port (Sch.K)";
			public const string ArrivalScheduleD = "Arrival Port (Sch.D)";
			public const string TransportMode = "Transport Mode";
			public const string ETA = "ETA";
			public const string MessageStatus = "Message Status";
			public const string QPMessageStatus = "QP Status";
			public const string WPMessageStatus = "WP Status";
			public const string BillDispositions = "Bill Dispositions";
			public const string CurrentBillDisposition = "Bill Disposition - Current";
			public const string NotSentForFilter = "NOT";
			public const string NotSentForFilterDescription = "Not Sent - only valid for exact match";
			public const string Branch = "Branch";

			public const string InBondClosedDate = "In-Bond Closed Date";
			public const string InBondCarrierOrg = "In-Bond Carrier (org)";
			public const string InBondCarrierCode = "In-Bond Carrier Code (SCAC)";
			public const string InBondUSDestination = "US Destination (Port Code)";
			public const string InBondForeignDestination = "Foreign Destination (Port Code)";
			public const string IssuerCode = "Issuer Code";
			public const string InBondMasterBill = "Master Bill";
			public const string InbondHouseBill = "House Bill";
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection result = new ModuleFilterCollection();
			AddNumberFilters(result);
			AddITFilters(result);
			AddLocationFilters(result);
			AddDateFilters(result);
			AddBranchFilter(result);
			return result;
		}

		protected override List<IFilterStripsHelper> GetCustomFilterStripsHelpersCore()
		{
			var helpers = base.GetCustomFilterStripsHelpersCore();
			var workflowHelper = new WorkflowFilterStripsHelperCusInBond(typeof(CusInBondHeader), WorkflowDescriptors.CusInBondHeaderWorkflowDescriptorCode, Factory);
			workflowHelper.SetShouldAddWorkflowCustomFieldsFilters(true);
			helpers.Add(workflowHelper);
			return helpers;
		}

		public override ZQuery Filter
		{
			get
			{
				var result = base.Filter;
				result.AddToFilter(GetCompanyQuery());
				return result;
			}
		}

		ZQuery GetCompanyQuery()
		{
			var result = new ZDBOnlyQuery(typeof(CusInBondHeader));
			var companySubQuery = new ZDBOnlySubQuery(typeof(GlbBranch), CusInBondHeaderSchema.BH_GB);
			companySubQuery.AddToFilter(GlbBranchSchema.GB_GC, GlbCompany.CurrentCompany.PK);

			result.AddSubQuery(companySubQuery, JoinCondition.And);
			return result;
		}

		public class CusInBondBillSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var subQuery = new ZDBOnlySubQuery(typeof(CusInBondBill), CusInBondBillSchema.B0_BH);
				subQuery.AddToFilter(filter);
				var result = new ZDBOnlyQuery(typeof(CusInBondHeader));
				result.AddSubQuery(CusInBondHeaderSchema.PK, subQuery, JoinCondition.And);
				return result;
			}
		}

		public class CusInBondMoveHeaderSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var subQuery = new ZDBOnlySubQuery(typeof(CusInBondMoveHeader), CusInBondMoveHeaderSchema.BM_BH);
				subQuery.AddToFilter(filter);
				var result = new ZDBOnlyQuery(typeof(CusInBondHeader));
				result.AddSubQuery(CusInBondHeaderSchema.PK, subQuery, JoinCondition.And);
				return result;
			}
		}

		void AddNumberFilters(ModuleFilterCollection filters)
		{
			var cusInBondBillSubGroup = new CusInBondBillSubGroup();
			var cusInBondMoveHeaderSubGroup = new CusInBondMoveHeaderSubGroup();

			var issuerCode = filters.AddTextFilter(FilterConstants.IssuerCode, CusInBondBillSchema.B0_IssuerCode)
				.WithMaxLengthOf<ModuleTextFilter>(CusInBondBillSchema.B0_IssuerCode);
			issuerCode.Category = FilterCategories.NumbersAndReferences;
			issuerCode.SubGroup = cusInBondBillSubGroup;
			issuerCode.UseMultiSearch = true;
			var masterBillNum = filters.AddTextFilter(FilterConstants.InBondMasterBill, CusInBondBillSchema.B0_MasterBillNumber)
				.WithMaxLengthOf<ModuleTextFilter>(CusInBondBillSchema.B0_MasterBillNumber);
			masterBillNum.Category = FilterCategories.NumbersAndReferences;
			masterBillNum.SubGroup = cusInBondBillSubGroup;
			masterBillNum.UseMultiSearch = true;
			var houseBillNum = filters.AddTextFilter(FilterConstants.InbondHouseBill, CusInBondBillSchema.B0_HouseBillNumber)
				.WithMaxLengthOf<ModuleTextFilter>(CusInBondBillSchema.B0_HouseBillNumber);
			houseBillNum.Category = FilterCategories.NumbersAndReferences;
			houseBillNum.SubGroup = cusInBondBillSubGroup;
			houseBillNum.UseMultiSearch = true;

			ModuleFilter carrierSCAC = filters.AddNumberFilter(FilterConstants.CarrierSCAC, CusInBondHeaderSchema.BH_CarrierSCAC);
			carrierSCAC.Category = FilterCategories.NumbersAndReferences;
			var jobReference = filters.AddTextFilter(FilterConstants.JobReference, CusInBondHeaderSchema.BH_JobReference);
			jobReference.Category = FilterCategories.NumbersAndReferences;
			jobReference.UseMultiSearch = true;
			var itNumber = filters.AddTextFilter(FilterConstants.InBondNumber, GetInBondNumberQuery)
				.WithMaxLengthOf<ModuleTextFilter>(CusEntryNumSchema.CE_EntryNum);
			itNumber.Category = FilterCategories.NumbersAndReferences;

			var inBondCarrierCode = filters.AddTextFilter(FilterConstants.InBondCarrierCode, CusInBondMoveHeaderSchema.BM_InBondCarrierSCAC)
				.WithMaxLengthOf<ModuleTextFilter>(CusInBondMoveHeaderSchema.BM_InBondCarrierSCAC);
			inBondCarrierCode.Category = FilterCategories.NumbersAndReferences;
			inBondCarrierCode.SubGroup = cusInBondMoveHeaderSubGroup;
			inBondCarrierCode.UseMultiSearch = true;

			var usDestination = filters.AddTextFilter(FilterConstants.InBondUSDestination, CusInBondMoveHeaderSchema.BM_DestinationPortCode)
				.WithMaxLengthOf<ModuleTextFilter>(CusInBondMoveHeaderSchema.BM_DestinationPortCode);
			usDestination.Category = FilterCategories.NumbersAndReferences;
			usDestination.SubGroup = cusInBondMoveHeaderSubGroup;
			usDestination.UseMultiSearch = true;

			var foreignDestination = filters.AddTextFilter(FilterConstants.InBondForeignDestination, CusInBondMoveHeaderSchema.BM_ForeignDestPortKCode)
				.WithMaxLengthOf<ModuleTextFilter>(CusInBondMoveHeaderSchema.BM_ForeignDestPortKCode);
			foreignDestination.Category = FilterCategories.NumbersAndReferences;
			foreignDestination.SubGroup = cusInBondMoveHeaderSubGroup;
			foreignDestination.UseMultiSearch = true;
		}

		#region IT (InBond) Filters

		void AddITFilters(ModuleFilterCollection filters)
		{
			ModuleTextFilter itTypeFilter = filters.AddTextFilter(FilterConstants.ITType, GetITTypeQuery, Lookups.InbondCommonTypeList);
			itTypeFilter.Category = FilterCategories.ModesAndTypes;
			itTypeFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			itTypeFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Contains);
			itTypeFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotEqual);
			itTypeFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
			itTypeFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);
			itTypeFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);
			itTypeFilter.MaxLength = CusInBondMoveHeaderSchema.BM_InBondEntryType.MaxLength;

			ModuleGuidFilter importerFilter = filters.AddGuidFilter(FilterConstants.Importer, ModuleIDs.Organisation, GetImporterQuery, Lookups.ImporterList);
			importerFilter.Category = FilterCategories.Organisations;

			var bondCarrierFilter = filters.AddGuidFilter(FilterConstants.InBondCarrierOrg, ModuleIDs.Organisation, GetBondCarrierQuery, Lookups.ShippingProviders);
			bondCarrierFilter.Category = FilterCategories.Organisations;

			ModuleFilter transportModeFilter = filters.AddTextFilter(FilterConstants.TransportMode, CusInBondHeaderSchema.BH_ImportTransportMode, Lookups.TransportModeList);
			transportModeFilter.Category = FilterCategories.ModesAndTypes;

			ModuleTextFilter qpStatusFilter = filters.AddTextFilter(CusInBondHeaderFilterStripBusinessObject.FilterConstants.QPMessageStatus, (filterOperator, value) => GetInBondStatusQuery(filterOperator, value, CusInBondMoveHeaderSchema.BM_CustomsStatus), Lookups.InbondQPMessageStatusListForFilter);
			SetFilterConstraints(qpStatusFilter);
			qpStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsBlank);
			qpStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsNotBlank);
			qpStatusFilter.MaxLength = CusInBondMoveHeaderSchema.BM_CustomsStatus.MaxLength;

			ModuleTextFilter wpStatusFilter = filters.AddTextFilter(CusInBondHeaderFilterStripBusinessObject.FilterConstants.WPMessageStatus, (filterOperator, value) => GetInBondStatusQuery(filterOperator, value, CusInBondMoveHeaderSchema.BM_MessageStatus), Lookups.InbondWPMessageStatusListForFilter);
			SetFilterConstraints(wpStatusFilter);
			wpStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsBlank);
			wpStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsNotBlank);
			wpStatusFilter.MaxLength = CusInBondMoveHeaderSchema.BM_MessageStatus.MaxLength;

			ModuleTextFilter billDispositionFilter = filters.AddTextFilter(FilterConstants.BillDispositions, GetBillDispositionQuery, Lookups.BillDispostionList);
			SetFilterConstraints(billDispositionFilter);
			billDispositionFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);

			ModuleTextFilter currentDispositionFilter = filters.AddTextFilter(FilterConstants.CurrentBillDisposition, GetCurrentBillDispositionQuery, Lookups.BillDispostionList);
			SetFilterConstraints(currentDispositionFilter);
			currentDispositionFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);
		}

		void SetFilterConstraints(ModuleTextFilter moduleFilter)
		{
			moduleFilter.Category = FilterCategories.StatusAndFlags;
			moduleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			moduleFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Contains);
			moduleFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
			moduleFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);
		}

		ZQuery GetITTypeQuery(SQLComparisonOperator filterOperator, ZString value)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(CusInBondHeader));
			ZDBOnlySubQuery moveHeaderQuery = new ZDBOnlySubQuery(typeof(CusInBondMoveHeader), CusInBondMoveHeaderSchema.BM_BH);
			moveHeaderQuery.AddToFilter(CusInBondMoveHeaderSchema.BM_InBondEntryType, filterOperator, value);
			result.AddSubQuery(moveHeaderQuery, JoinCondition.And);
			return result;
		}

		ZQuery GetImporterQuery(ZGuid importerPK)
		{
			ZDBOnlySubQuery addressQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.PK);
			addressQuery.AddToFilter(OrgAddressSchema.OA_OH, importerPK);

			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(CusInBondHeader));
			result.AddSubQuery(CusInBondHeaderSchema.BH_OA_Importer, addressQuery, JoinCondition.And);

			return result;
		}

		ZQuery GetBondCarrierQuery(ZGuid carrierOrgPK)
		{
			var addressQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.PK);
			addressQuery.AddToFilter(OrgAddressSchema.OA_OH, carrierOrgPK);
			var result = new ZDBOnlyQuery(typeof(CusInBondHeader));
			var moveHeaderQuery = new ZDBOnlySubQuery(typeof(CusInBondMoveHeader), CusInBondMoveHeaderSchema.BM_BH);
			moveHeaderQuery.AddSubQuery(CusInBondMoveHeaderSchema.BM_OA_InBondCarrier, addressQuery, JoinCondition.And);
			result.AddSubQuery(moveHeaderQuery, JoinCondition.And);

			return result;
		}

		ZQuery GetInBondStatusQuery(SQLComparisonOperator filterOperator, ZString value, SchemaColumn moveHeaderColumn)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(CusInBondHeader));
			if (!value.IsEmpty)
			{
				if (value == FilterConstants.NotSentForFilter && filterOperator != SQLComparisonOperator.Equal)
				{
					result.IsNoResultQuery = true;
				}
				else
				{
					if (value == FilterConstants.NotSentForFilter)
					{
						value = ZString.Empty;
					}

					ZDBOnlySubQuery moveHeaderQuery = new ZDBOnlySubQuery(typeof(CusInBondMoveHeader), CusInBondMoveHeaderSchema.BM_BH);
					moveHeaderQuery.AddToFilter(moveHeaderColumn, filterOperator, value);
					result.AddSubQuery(moveHeaderQuery, JoinCondition.And);
				}
			}

			return result;
		}

		ZQuery GetBillDispositionQuery(SQLComparisonOperator filterOperator, ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(CusInBondHeader));
			var inBondHeaderQuery = new ZDBOnlySubQuery(typeof(CusInBondHeader), CusInBondHeaderSchema.PK, filterOperator == SQLComparisonOperator.NotEqual || filterOperator == SpecialComparisonOperator.IsBlank);
			var moveHeaderQuery = new ZDBOnlySubQuery(typeof(CusInBondMoveHeader), CusInBondMoveHeaderSchema.BM_BH);

			var condition = "1=1";
			if (filterOperator == SQLComparisonOperator.Equal || filterOperator == SQLComparisonOperator.NotEqual)
			{
				condition = ZString.Format("B7_Type = 'UDP' AND AddInfoTable.Value = '{0}'", value);
			}
			else if (filterOperator == SpecialComparisonOperator.IsBlank || filterOperator == SpecialComparisonOperator.IsNotBlank)
			{
				condition = "B7_Type = 'UDP' AND AddInfoTable.Value <> ''";
			}

			var sqlFilter = ZString.Format(@"BM_PK IN (SELECT B9_BM FROM dbo.CusInBondMoveDetail JOIN dbo.CusAddInfo ON B7_ParentID = B9_PK CROSS APPLY dbo.csfn_GetAddInfoValueFromCodeInlineToReturnEmptyIfNull(B7_AddInfoData, 'Code') AS AddInfoTable WHERE {0})", condition);
			moveHeaderQuery.AddFilterAndZSQLParameterCollection(sqlFilter, null);

			inBondHeaderQuery.AddSubQuery(moveHeaderQuery, JoinCondition.And);
			result.AddSubQuery(inBondHeaderQuery, JoinCondition.And);
			return result;
		}

		ZQuery GetCurrentBillDispositionQuery(SQLComparisonOperator filterOperator, ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(CusInBondHeader));
			var inBondHeaderQuery = new ZDBOnlySubQuery(typeof(CusInBondHeader), CusInBondHeaderSchema.PK, filterOperator == SQLComparisonOperator.NotEqual || filterOperator == SpecialComparisonOperator.IsBlank);

			var moveHeaderQuery = new ZDBOnlySubQuery(typeof(CusInBondMoveHeader), CusInBondMoveHeaderSchema.BM_BH);
			var moveDetailQuery = new ZDBOnlySubQuery(typeof(CusInBondMoveDetail), CusInBondMoveDetailSchema.B9_BM);
			var cusAddInfoQuery = new ZDBOnlySubQuery(typeof(CusAddInfo), CusAddInfoSchema.B7_ParentID);
			var genAddOnColumnQuery = new ZDBOnlySubQuery(typeof(GenAddOnColumn), GenAddOnColumnSchema.XA_ParentID);

			genAddOnColumnQuery.AddToFilter(GenAddOnColumnSchema.XA_Name, "US_Code");
			genAddOnColumnQuery.AddToFilter(GenAddOnColumnSchema.XA_ParentTableCode, CusAddInfoSchema.Constants.Prefix);

			if (filterOperator != SpecialComparisonOperator.IsBlank && filterOperator != SpecialComparisonOperator.IsNotBlank)
			{
				genAddOnColumnQuery.AddToFilter(GenAddOnColumnSchema.XA_Data, SQLComparisonOperator.Equal, value);
			}

			cusAddInfoQuery.AddSubQuery(genAddOnColumnQuery, JoinCondition.And);
			moveDetailQuery.AddSubQuery(cusAddInfoQuery, JoinCondition.And);
			moveHeaderQuery.AddSubQuery(moveDetailQuery, JoinCondition.And);
			inBondHeaderQuery.AddSubQuery(moveHeaderQuery, JoinCondition.And);
			result.AddSubQuery(inBondHeaderQuery, JoinCondition.And);
			return result;
		}

		#endregion

		void AddLocationFilters(ModuleFilterCollection filters)
		{
			var loadingSchedK = filters.AddTextFilter(FilterConstants.LoadingScheduleK, CusInBondHeaderSchema.BH_ImportLoadPortKCode);
			loadingSchedK.Category = FilterCategories.Locations;
			loadingSchedK.UseMultiSearch = true;

			var arrivalSchedD = filters.AddTextFilter(FilterConstants.ArrivalScheduleD, CusInBondHeaderSchema.BH_PortUnladingDCode);
			arrivalSchedD.Category = FilterCategories.Locations;
			arrivalSchedD.UseMultiSearch = true;
		}

		void AddDateFilters(ModuleFilterCollection filters)
		{
			ModuleFilter etaFilter = filters.AddDateFilter(FilterConstants.ETA, CusInBondHeaderSchema.BH_ETA);
			etaFilter.Category = FilterCategories.Dates;

			var closedDate = filters.AddDateFilter(FilterConstants.InBondClosedDate, GetClosedDateQuery);
			closedDate.Category = FilterCategories.Dates;
		}

		void AddBranchFilter(ModuleFilterCollection filters)
		{
			var branchFilter = filters.AddGuidFilter(FilterConstants.Branch, ModuleIDs.GlbBranch, CusInBondHeaderSchema.BH_GB, Lookups.BranchList);
			branchFilter.Category = FilterCategories.Organisations;
			branchFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsNotBlank);
			branchFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsBlank);
			branchFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.FiltersMatch);
		}

		ZQuery GetClosedDateQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			var result = new ZDBOnlyQuery(typeof(CusInBondHeader));
			var moveHeaderQuery = new ZDBOnlySubQuery(typeof(CusInBondMoveHeader), CusInBondMoveHeaderSchema.BM_BH);
			AddDateRange(moveHeaderQuery, comparisonOperator, JoinCondition.And, CusInBondMoveHeaderSchema.BM_InBondClosedDate, date1.Date, date2.Date);
			result.AddSubQuery(moveHeaderQuery, JoinCondition.And);
			return result;
		}

		ZQuery GetInBondNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			if (comparisonOperator == SpecialComparisonOperator.IsBlank)
			{
				return InBondNumberIsBlankQuery();
			}

			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(CusInBondHeader));
			ZDBOnlySubQuery moveHeaderQuery = new ZDBOnlySubQuery(typeof(CusInBondMoveHeader), CusInBondMoveHeaderSchema.BM_BH);
			ZDBOnlySubQuery inBondNumberQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
			inBondNumberQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, Enterprise.Customs.US.Business.CusEntryHeaderMessageTypeList.Codes.InBond);
			inBondNumberQuery.AddToFilter_PossiblyCommaSeparated(CusEntryNumSchema.CE_EntryNum, comparisonOperator, value);
			moveHeaderQuery.AddSubQuery(inBondNumberQuery, JoinCondition.And);
			result.AddSubQuery(moveHeaderQuery, JoinCondition.And);
			return result;
		}

		ZQuery InBondNumberIsBlankQuery()
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(CusInBondHeader));
			ZDBOnlySubQuery moveHeaderQuery = new ZDBOnlySubQuery(typeof(CusInBondMoveHeader), CusInBondMoveHeaderSchema.BM_BH);
			ZDBOnlySubQuery inBondNumberQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID, true);
			inBondNumberQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, Enterprise.Customs.US.Business.CusEntryHeaderMessageTypeList.Codes.InBond);
			moveHeaderQuery.AddSubQuery(inBondNumberQuery, JoinCondition.And);
			result.AddSubQuery(moveHeaderQuery, JoinCondition.And);

			ZDBOnlySubQuery headerWithNoMovementQuery = new ZDBOnlySubQuery(typeof(CusInBondMoveHeader), CusInBondMoveHeaderSchema.BM_BH, true);
			result.AddSubQuery(headerWithNoMovementQuery, JoinCondition.Or);

			return result;
		}

		#region Lookups

		public CusInBondHeaderFilterLookups Lookups
		{
			get { return new CusInBondHeaderFilterLookups(this); }
		}

		#endregion
	}
}
