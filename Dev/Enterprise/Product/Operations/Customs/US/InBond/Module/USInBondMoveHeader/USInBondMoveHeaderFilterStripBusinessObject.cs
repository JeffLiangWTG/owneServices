using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.US.InBond.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.InBond.Module
{
	public class USInBondMoveHeaderFilterStripBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();
			AddNumberFilters(result);
			AddITFilters(result);
			AddLocationFilters(result);
			AddDateFilters(result);
			AddBranchFilter(result);
			return result;
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
			var result = new ZDBOnlyQuery(typeof(USInBondMoveHeader));
			var companySubQuery = new ZDBOnlySubQuery(typeof(GlbBranch), USInBondMoveHeaderSchema.BMH_GB);
			companySubQuery.AddToFilter(GlbBranchSchema.GB_GC, GlbCompany.CurrentCompany.PK);
			result.AddSubQuery(companySubQuery, JoinCondition.And);
			return result;
		}

		#region Number Filters
		public class CusInBondBillSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var subQuery = new ZDBOnlySubQuery(typeof(CusInBondBill), CusInBondBillSchema.B0_BH);
				subQuery.AddToFilter(filter);
				var result = new ZDBOnlyQuery(typeof(USInBondMoveHeader));
				result.AddSubQuery(USInBondMoveHeaderSchema.BMH_BH, subQuery, JoinCondition.And);
				return result;
			}
		}

		void AddNumberFilters(ModuleFilterCollection filters)
		{
			var cusInBondBillSubGroup = new CusInBondBillSubGroup();

			var issuerCode = filters.AddTextFilter(CusInBondHeaderFilterStripBusinessObject.FilterConstants.IssuerCode, CusInBondBillSchema.B0_IssuerCode)
				.WithMaxLengthOf<ModuleTextFilter>(CusInBondBillSchema.B0_IssuerCode);
			issuerCode.Category = FilterCategories.NumbersAndReferences;
			issuerCode.SubGroup = cusInBondBillSubGroup;
			issuerCode.UseMultiSearch = true;
			var masterBillNum = filters.AddTextFilter(CusInBondHeaderFilterStripBusinessObject.FilterConstants.InBondMasterBill, CusInBondBillSchema.B0_MasterBillNumber)
				.WithMaxLengthOf<ModuleTextFilter>(CusInBondBillSchema.B0_MasterBillNumber);
			masterBillNum.Category = FilterCategories.NumbersAndReferences;
			masterBillNum.SubGroup = cusInBondBillSubGroup;
			masterBillNum.UseMultiSearch = true;
			var houseBillNum = filters.AddTextFilter(CusInBondHeaderFilterStripBusinessObject.FilterConstants.InbondHouseBill, CusInBondBillSchema.B0_HouseBillNumber)
				.WithMaxLengthOf<ModuleTextFilter>(CusInBondBillSchema.B0_HouseBillNumber);
			houseBillNum.Category = FilterCategories.NumbersAndReferences;
			houseBillNum.SubGroup = cusInBondBillSubGroup;
			houseBillNum.UseMultiSearch = true;

			var carrierSCAC = filters.AddNumberFilter(CusInBondHeaderFilterStripBusinessObject.FilterConstants.CarrierSCAC, USInBondMoveHeaderSchema.BMH_CarrierSCAC);
			carrierSCAC.Category = FilterCategories.NumbersAndReferences;
			var jobReference = filters.AddTextFilter(CusInBondHeaderFilterStripBusinessObject.FilterConstants.JobReference, USInBondMoveHeaderSchema.BMH_JobReference);
			jobReference.Category = FilterCategories.NumbersAndReferences;
			jobReference.UseMultiSearch = true;
			var itNumber = filters.AddTextFilter(CusInBondHeaderFilterStripBusinessObject.FilterConstants.InBondNumber, GetInBondNumberQuery);
			itNumber.Category = FilterCategories.NumbersAndReferences;
			itNumber.MaxLength = CusEntryNumSchema.CE_EntryNum.MaxLength;

			var inBondCarrierCode = filters.AddTextFilter(CusInBondHeaderFilterStripBusinessObject.FilterConstants.InBondCarrierCode, USInBondMoveHeaderSchema.BMH_InBondCarrierSCAC);
			inBondCarrierCode.Category = FilterCategories.NumbersAndReferences;
			inBondCarrierCode.MaxLength = USInBondMoveHeaderSchema.BMH_InBondCarrierSCAC.MaxLength;
			inBondCarrierCode.UseMultiSearch = true;

			var usDestination = filters.AddTextFilter(CusInBondHeaderFilterStripBusinessObject.FilterConstants.InBondUSDestination, USInBondMoveHeaderSchema.BMH_DestinationPortCode);
			usDestination.Category = FilterCategories.NumbersAndReferences;
			usDestination.MaxLength = USInBondMoveHeaderSchema.BMH_DestinationPortCode.MaxLength;
			usDestination.UseMultiSearch = true;

			var foreignDestination = filters.AddTextFilter(CusInBondHeaderFilterStripBusinessObject.FilterConstants.InBondForeignDestination, USInBondMoveHeaderSchema.BMH_ForeignDestPortKCode);
			foreignDestination.Category = FilterCategories.NumbersAndReferences;
			foreignDestination.MaxLength = USInBondMoveHeaderSchema.BMH_ForeignDestPortKCode.MaxLength;
			foreignDestination.UseMultiSearch = true;
		}

		ZQuery GetInBondNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(USInBondMoveHeader));
			var inBondNumberQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
			inBondNumberQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, Enterprise.Customs.US.Business.CusEntryHeaderMessageTypeList.Codes.InBond);
			inBondNumberQuery.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.UnitedStates);
			inBondNumberQuery.AddToFilter_PossiblyCommaSeparated(CusEntryNumSchema.CE_EntryNum, comparisonOperator, value);
			result.AddSubQuery(inBondNumberQuery, JoinCondition.And);
			return result;
		}

		#endregion

		#region IT (InBond) Filters

		void AddITFilters(ModuleFilterCollection filters)
		{
			var itTypeFilter = filters.AddTextFilter(CusInBondHeaderFilterStripBusinessObject.FilterConstants.ITType, USInBondMoveHeaderSchema.BMH_InBondEntryType, Lookups.InbondCommonTypeList);
			itTypeFilter.Category = FilterCategories.ModesAndTypes;
			itTypeFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			itTypeFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Contains);
			itTypeFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotEqual);
			itTypeFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
			itTypeFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);
			itTypeFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);
			itTypeFilter.MaxLength = CusInBondMoveHeaderSchema.BM_InBondEntryType.MaxLength;

			var importerFilter = filters.AddGuidFilter(CusInBondHeaderFilterStripBusinessObject.FilterConstants.Importer, ModuleIDs.Organisation, (orgHeaderPK) => GetOrganizationRelatedQuery(USInBondMoveHeaderSchema.BMH_OA_Importer, orgHeaderPK), Lookups.ImporterList);
			importerFilter.Category = FilterCategories.Organisations;

			var bondCarrierFilter = filters.AddGuidFilter(CusInBondHeaderFilterStripBusinessObject.FilterConstants.InBondCarrierOrg, ModuleIDs.Organisation, (orgHeaderPK) => GetOrganizationRelatedQuery(USInBondMoveHeaderSchema.BMH_OA_InBondCarrier, orgHeaderPK), Lookups.ShippingProviders);
			bondCarrierFilter.Category = FilterCategories.Organisations;

			var transportModeFilter = filters.AddTextFilter(CusInBondHeaderFilterStripBusinessObject.FilterConstants.TransportMode, USInBondMoveHeaderSchema.BMH_ImportTransportMode, Lookups.TransportModeList);
			transportModeFilter.Category = FilterCategories.ModesAndTypes;

			ModuleTextFilter qpStatusFilter = filters.AddTextFilter(CusInBondHeaderFilterStripBusinessObject.FilterConstants.QPMessageStatus, USInBondMoveHeaderSchema.BMH_CustomsStatus, Lookups.InbondQPMessageStatusListForFilter);
			SetFilterConstraints(qpStatusFilter);
			qpStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsBlank);
			qpStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsNotBlank);
			qpStatusFilter.MaxLength = CusInBondMoveHeaderSchema.BM_CustomsStatus.MaxLength;

			ModuleTextFilter wpStatusFilter = filters.AddTextFilter(CusInBondHeaderFilterStripBusinessObject.FilterConstants.WPMessageStatus, USInBondMoveHeaderSchema.BMH_MessageStatus, Lookups.InbondWPMessageStatusListForFilter);
			SetFilterConstraints(wpStatusFilter);
			wpStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsBlank);
			wpStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsNotBlank);
			wpStatusFilter.MaxLength = CusInBondMoveHeaderSchema.BM_MessageStatus.MaxLength;
		}

		void SetFilterConstraints(ModuleTextFilter moduleFilter)
		{
			moduleFilter.Category = FilterCategories.StatusAndFlags;
			moduleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			moduleFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Contains);
			moduleFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
			moduleFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);
		}

		ZQuery GetOrganizationRelatedQuery(SchemaGuidColumn schemaColumn, ZGuid orgHeaderPK)
		{
			var result = new ZDBOnlyQuery(typeof(USInBondMoveHeader));
			var addressQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.PK);
			addressQuery.AddToFilter(OrgAddressSchema.OA_OH, orgHeaderPK);
			result.AddSubQuery(schemaColumn, addressQuery, JoinCondition.And);
			return result;
		}

		#endregion

		#region Location Filters

		void AddLocationFilters(ModuleFilterCollection filters)
		{
			var loadingSchedK = filters.AddTextFilter(CusInBondHeaderFilterStripBusinessObject.FilterConstants.LoadingScheduleK, USInBondMoveHeaderSchema.BMH_ImportLoadPortKCode);
			loadingSchedK.Category = FilterCategories.Locations;
			loadingSchedK.UseMultiSearch = true;

			var arrivalSchedD = filters.AddTextFilter(CusInBondHeaderFilterStripBusinessObject.FilterConstants.ArrivalScheduleD, USInBondMoveHeaderSchema.BMH_PortUnladingDCode);
			arrivalSchedD.Category = FilterCategories.Locations;
			arrivalSchedD.UseMultiSearch = true;
		}

		#endregion

		#region Date Filters

		void AddDateFilters(ModuleFilterCollection filters)
		{
			var etaFilter = filters.AddDateFilter(CusInBondHeaderFilterStripBusinessObject.FilterConstants.ETA, USInBondMoveHeaderSchema.BMH_ETA);
			etaFilter.Category = FilterCategories.Dates;

			var closedDate = filters.AddDateFilter(CusInBondHeaderFilterStripBusinessObject.FilterConstants.InBondClosedDate, USInBondMoveHeaderSchema.BMH_InBondClosedDate);
			closedDate.Category = FilterCategories.Dates;
		}

		#endregion

		#region Branch Filters

		void AddBranchFilter(ModuleFilterCollection filters)
		{
			var branchFilter = filters.AddGuidFilter(CusInBondHeaderFilterStripBusinessObject.FilterConstants.Branch, ModuleIDs.GlbBranch, USInBondMoveHeaderSchema.BMH_GB, Lookups.BranchList);
			branchFilter.Category = FilterCategories.Organisations;
			branchFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsNotBlank);
			branchFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsBlank);
			branchFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.FiltersMatch);
		}

		#endregion

		#region Lookups

		public USInBondMoveHeaderFilterLookups Lookups
		{
			get { return new USInBondMoveHeaderFilterLookups(this); }
		}

		#endregion
	}
}
