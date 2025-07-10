using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportConsignment.Module
{
	public class DtbCarrierFilterBusinessObject : DtbChildFilterBusinessObject
	{
		public DtbCarrierFilterBusinessObject()
			: base()
		{
			((IFilterStripBusinessObjectInternals)this).LayoutContext = "DtbRoutePlannerCarrier";
		}

		#region FilterConstants

		public static class FilterConstants
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "filter name")]
			public const string Branch = "Carrier Branch";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "filter name")]
			public const string State = "Carrier State";

			public static FilterCategory CarrierCategory
			{
				get { return FilterCategories.GetOrCreateFilterCategory(ResString.GetMultilingualString("CarrierFilterBusinessObject|Carrier", "Carrier")); }
			}
		}

		#endregion

		#region ModuleFilters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();

			var stateFilter = result.AddTextFilter(FilterConstants.State, GetStateQuery);
			stateFilter.MultilingualDescription = ResString.GetMultilingualString("CarrierFilterBusinessObject|State", "Carrier State");
			stateFilter.Category = FilterConstants.CarrierCategory;
			stateFilter.MaxLength = OrgAddressSchema.OA_State.MaxLength;

			var branchFilter = result.AddGuidFilter(FilterConstants.Branch, ModuleIDs.GlbBranch, GetBranchQuery, new GlbBranchCollection(Factory));
			branchFilter.MultilingualDescription = ResString.GetMultilingualString("CarrierFilterBusinessObject|Branch", "Carrier Branch");
			branchFilter.Category = FilterConstants.CarrierCategory;

			return result;
		}

		ZQuery GetStateQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(OrgHeader));

			var subQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.OA_OH);
			subQuery.AddToFilter(OrgAddressSchema.OA_State, comparisonOperator, value);

			query.AddSubQuery(subQuery, JoinCondition.And);
			return query;
		}

		ZQuery GetBranchQuery(ZGuid value)
		{
			var query = new ZDBOnlyQuery(typeof(OrgHeader));

			var orgCompanyDataSubQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
			orgCompanyDataSubQuery.AddToFilter(OrgCompanyDataSchema.OB_GC, GlbCompany.CurrentCompany.PK);

			var glbBranchSubQuery = new ZDBOnlySubQuery(typeof(GlbBranch), OrgCompanyDataSchema.OB_GB_ControllingBranch);
			glbBranchSubQuery.AddToFilter(GlbBranchSchema.PK, value);
			orgCompanyDataSubQuery.AddSubQuery(glbBranchSubQuery, JoinCondition.And);

			query.AddSubQuery(orgCompanyDataSubQuery, JoinCondition.And);

			return query;
		}

		#endregion

		#region Implementation

		public override SchemaColumn FieldOnRunsheet
		{
			get { return DtbConsignmentRunSheetSchema.KG_OH_TransportCo; }
		}

		public override SchemaColumn ChildBizOPKOrNK
		{
			get { return OrgHeaderSchema.PK; }
		}

		public override IEnumerable<ModuleFilter> ActiveFilters
		{
			get { return routePlannerFilterBusinessObject != null ? routePlannerFilterBusinessObject.ActiveModuleFiltersForChildFilter.Where(c => c.Category == FilterConstants.CarrierCategory) : ActiveModuleFilters; }
		}

		public override Type TypeOfBusinessObjectToQuery
		{
			get { return typeof(OrgHeader); }
		}

		#endregion

#if DEBUG
		public GetTextQueryWithOperator GetStateQueryForTest()
		{
			return GetStateQuery;
		}
#endif
	}
}
