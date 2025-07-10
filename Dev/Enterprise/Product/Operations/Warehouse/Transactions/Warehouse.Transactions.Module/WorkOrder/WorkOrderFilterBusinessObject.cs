using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class WorkOrderFilterBusinessObject : WhsComponentOrderFilterBusinessObject
	{
		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = base.GetModuleFiltersCore();

			var filter = filters.AddTextFilter("Parent Order No.", GetParentOrderNoQuery);
			filter.MultilingualDescription = ResString.GetMultilingualString("2f34d007-e0cf-4c52-853e-c47ec4dd1cd6", "Parent Order No.");
			filter.MaxLength = WhsDocketSchema.WD_ExternalReference.MaxLength;
			filter.Category = FilterCategories.NumbersAndReferences;

			return filters;
		}

		ZQuery GetParentOrderNoQuery(SQLComparisonOperator comparisonOperator, ZString parentOrderNo)
		{
			var child = new ZDBOnlySubQuery(typeof(WhsDocket), WhsDocketSchema.PK);
			child.AddToFilter(WhsDocketSchema.WD_ExternalReference, comparisonOperator, parentOrderNo);

			var parent = new ZDBOnlyQuery(typeof(WhsDocket));
			parent.AddSubQuery(WhsDocketSchema.WD_WD_ParentDocket, child, JoinCondition.And);

			return parent;
		}

		protected override bool IncludeTransportCoFilter => true;

		protected override bool IncludeConsigneeFilter => true;

		protected override bool IncludeDeliveryRouteFilters => true;

		#endregion

		#region Queries

		protected override ZDBOnlySubQuery GetJobDocAddressOrgHeaderParentSubQuery(ZString docAddressTypeCode, ZGuid orgPK, SQLComparisonOperator comparisonOperator)
		{
			return WhsWorkOrderDocAddressQueryHelper.GetWorkOrderAddressQuery(docAddressTypeCode, orgPK);
		}

		protected override ZQuery GetTransportCompanyNameQuery(SQLComparisonOperator comparisonOperator, ZString companyName)
		{
			var docketSubQuery = WhsWorkOrderDocAddressQueryHelper.GetWorkOrderCompanyNameQuery(TransportCoConstants.AddressTypeCode, comparisonOperator, companyName);

			var result = new ZDBOnlyQuery(typeof(WhsDocket));
			result.AddSubQuery(docketSubQuery, JoinCondition.And);

			return result;
		}

		#endregion

		#region AddTransportCoFilter

		protected override void AddTransportCoFilter(ModuleFilterCollection filterCollection)
		{
			var filter = filterCollection.AddGuidFilter("Transport Co", ModuleIDs.Organisation, value => GetDocAddressQuery(value, TransportCoConstants.AddressTypeCode), TransportCos);
			filter.MultilingualDescription = ResString.GetMultilingualString("010b54f5-4d1a-42d6-96bf-6ae7fff54f20", "Transport Co");
		}

		#endregion

		protected override CodeDescriptionPairList OrderTypes => new WorkOrderType();
	}
}
