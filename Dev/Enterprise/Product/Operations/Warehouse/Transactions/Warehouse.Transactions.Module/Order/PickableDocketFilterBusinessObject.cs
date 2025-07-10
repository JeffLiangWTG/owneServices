using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Module
{
	public abstract class PickableDocketFilterBusinessObject : ExtendedDocketFilterBusinessObject
	{
		public static class Schema
		{
			public const string TransportJobNumber = "Transport Job Number";
			public const string TrolleyNumber = "Trolley Number";
			public const string PackingRequired = "Packing Required";
			public const string ShortStatus = "Shortfall status";
			public const string TransportZone = "Transport Zone";
			public const string HasDangerousGoods = "Has Dangerous Goods";
			public const string FinalizedStatus = "Finalized Status";
			public const string LoadID = "Load ID";
			public const string OutboundLocation = "Outbound Location";
		}

		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = base.GetModuleFiltersCore();

			if (IncludeConsigneeFilter)
			{
				AddConsigneeFilter(filters);
			}

			var pickNoFilter = filters.AddFountainFilter("Pick No", GetPickNoQuery, "P");
			pickNoFilter.MaxLength = WhsPickSchema.WP_PickNo.MaxLength;
			pickNoFilter.MultilingualDescription = ResString.GetMultilingualString("0068f4e6-890d-4ceb-a58e-e0328a84bc4c", "Pick No");

			filters.AddDateFilter("Required Date", WhsDocketSchema.WD_RequiredDate).MultilingualDescription = ResString.GetMultilingualString("5c98bcd0-0781-4bd9-b492-9be163e3ae86", "Required Date");

			var filter = filters.AddTextFilter("Order Type", WhsDocketSchema.WD_DocketSubType, OrderTypes);
			filter.MultilingualDescription = ResString.GetMultilingualString("072cae89-51e0-47e7-b332-581711b03d2d", "Order Type");
			filter.Category = FilterCategories.StatusAndFlags;

			filter = filters.AddTextFilter("Pick Option", WhsDocketSchema.WD_PickOption, PickOptions);
			filter.MultilingualDescription = ResString.GetMultilingualString("a701cef4-6a02-4041-bb02-5593743a166c", "Pick Option");
			filter.Category = FilterCategories.StatusAndFlags;

			var filterNumber = filters.AddNumberRangeFilter("Priority", GetPriority);
			filterNumber.PropertyType = ZCalcEditPropertyType.Byte;
			filterNumber.MultilingualDescription = ResString.GetMultilingualString("71D3A49B-CF72-42D1-A9B0-59FF2592EE22", "Pick Priority");
			filterNumber.Category = FilterCategories.NumbersAndReferences;

			if (IncludeDeliveryRouteFilters)
			{
				filter = filters.AddTextFilter("Delivery Route", GetDeliveryRoute, Env.Registry.DeliveryRoutesListSorted);
				filter.MultilingualDescription = ResString.GetMultilingualString("480B296E-7EC9-40CD-B790-9ABB252E1836", "Delivery Route");
				filter.Category = FilterCategories.Organisations;

				filterNumber = filters.AddNumberRangeFilter("Delivery Route Sequence", GetDeliveryRouteSequence);
				filterNumber.PropertyType = ZCalcEditPropertyType.Short;
				filterNumber.MultilingualDescription = ResString.GetMultilingualString("BB03A64B-0B36-4009-AE65-0FA13C0A5CDC", "Delivery Route Sequence");
				filterNumber.Category = FilterCategories.Organisations;
				filterNumber.MinValue = 0;
			}

			AddReferenceFilters(filters);

			return filters;
		}

		protected virtual void AddConsigneeFilter(ModuleFilterCollection filters)
		{
			var consigneeFilter = new ModuleGuidFilterForOrg((NoResString)"Consignee", ModuleIDs.Organisation, value => GetDocAddressQuery(value, DocAddressTypes.Codes.ConsigneeAddress), FilterConsignees);
			consigneeFilter.MultilingualDescription = ResString.GetMultilingualString("42daa3bc-d36c-448b-b1e2-eb047b63486b", "Consignee");
			filters.AddFilter(consigneeFilter);
		}

		protected override IList<FilterGroupMember> ReferenceFields
		{
			get
			{
				IList<FilterGroupMember> result = base.ReferenceFields;
				result.Add(new FilterGroupMember(ResString.GetMultilingualString("BDF2534D-DE04-4EFE-8688-4E053A617A8F", "Order No."), WhsDocketSchema.WD_ExternalReference)); // May be a filter name.

				return result;
			}
		}

		protected override ModuleGuidFilter ConsigneeFilter => (ModuleGuidFilter)ModuleFilters["Consignee"];

		protected override MultilingualString DocketStatusFilterMultilingualDescriptionCore()
			=> ResString.GetMultilingualString("ccc284fa-be84-4976-b995-33aca55f93c2", "Order Status");

		protected override string DocketStatusFilterDescriptionCore() => (NoResString)"Order Status";  // Filter Description Text

		protected abstract bool IncludeConsigneeFilter { get; }

		protected abstract bool IncludeDeliveryRouteFilters { get; }

		#endregion

		#region Lookups

		protected abstract CodeDescriptionPairList OrderTypes { get; }

		#endregion

		#region Queries

		ZQuery GetDeliveryRoute(ZString value)
		{
			var docketResult = new ZDBOnlyQuery(typeof(WhsDocket));
			var jobDocAddresses = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID);
			jobDocAddresses.AddToFilter(JobDocAddressSchema.E2_AddressType, DocAddressTypes.Codes.ConsigneeAddress);
			var orgAddresses = new ZDBOnlySubQuery(typeof(OrgAddress), JobDocAddressSchema.E2_OA_Address);
			orgAddresses.AddToFilter(OrgAddressSchema.OA_DeliveryRoute, value);
			jobDocAddresses.AddSubQuery(orgAddresses, JoinCondition.And);
			docketResult.AddSubQuery(jobDocAddresses, JoinCondition.And);

			var result = new ZQuery();
			result.AddToFilter(docketResult);

			return result;
		}

		ZQuery GetDeliveryRouteSequence(INumericZType value1, INumericZType value2)
		{
			var docketResult = new ZDBOnlyQuery(typeof(WhsDocket));
			var jobDocAddresses = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID);
			jobDocAddresses.AddToFilter(JobDocAddressSchema.E2_AddressType, DocAddressTypes.Codes.ConsigneeAddress);
			var orgAddresses = new ZDBOnlySubQuery(typeof(OrgAddress), JobDocAddressSchema.E2_OA_Address);
			ModuleNumberRangeFilter.AddToFilters(orgAddresses, OrgAddressSchema.OA_DeliveryRouteSequence, value1, value2);
			jobDocAddresses.AddSubQuery(orgAddresses, JoinCondition.And);

			var result = new ZQuery();
			result.AddToFilter(docketResult);
			docketResult.AddSubQuery(jobDocAddresses, JoinCondition.And);

			return result;
		}

		ZQuery GetPriority(INumericZType value1, INumericZType value2)
		{
			return ModuleNumberRangeFilter.AddToFilters(new ZQuery(), WhsDocketSchema.WD_PickPriority, value1, value2);
		}

		ZQuery GetPickNoQuery(SQLComparisonOperator @operator, ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(WhsDocket));
			var pickSubQuery = new ZDBOnlySubQuery(typeof(WhsPick), WhsDocketSchema.WD_WP);
			pickSubQuery.AddToFilter_PossiblyCommaSeparated(WhsPickSchema.WP_PickNo, @operator, value);
			query.AddSubQuery(pickSubQuery, JoinCondition.And);
			return query;
		}

		protected override ZQuery GetPalletIDQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			bool notIn = SQLComparisonOperator.ReverseOperatorIfItIsNotInOperator(ref comparisonOperator);

			var orderLineQuery = new ZDBOnlySubQuery(typeof(WhsOrderLine), WhsDocketLineSchema.WE_WD, notIn);
			orderLineQuery.AddSubQuery(GetPickLineQueryForPalletID(comparisonOperator, value), JoinCondition.And);

			var query = new ZDBOnlyQuery(typeof(WhsDocket));

			// picklines used for reserving stock should not be considered for filtering on PalletID
			if (notIn)
			{
				query.AddToFilter(WhsDocketSchema.WD_WP, null);
				query.AddSubQuery(orderLineQuery, JoinCondition.Or);
			}
			else
			{
				query.AddToFilter(WhsDocketSchema.WD_WP, SQLComparisonOperator.NotEqual, null);
				query.AddSubQuery(orderLineQuery, JoinCondition.And);
			}

			return query;
		}

		#endregion
	}
}
