using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.eTail.Business;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.eTail.Module
{
	public class HVLVFilterProviderForShipment : IHVLVFilterProviderForShipment
	{
		static class Descriptions
		{
			#region SuppressResourceStringsCheckRegion

			public const string ItemId = "HVLV Item ID";
			public const string ConsignmentId = "HVLV Consignment ID";
			public const string ShipperReference = "HVLV Shipper Reference";

			#endregion
		}

		public void AddHVLVFilters(IModuleFilterCollection filterCollection)
		{
			var filters = Argument.NotNull(filterCollection as ModuleFilterCollection, nameof(filterCollection));

			var itemFilterProcessor = new ItemSubGroup();
			var consignmentFilterProcessor = new ConsignmentSubGroup(itemFilterProcessor);

			var itemIdFilter = filters.AddTextFilter(Descriptions.ItemId, HVLVItemSchema.HVI_ItemId);
			itemIdFilter.MultilingualDescription = ResString.GetMultilingualString("1f0266e3-3f0a-4dcf-8170-01543a9c0416", "Item ID");
			itemIdFilter.SupportsBlankComparisonOperators = false;
			itemIdFilter.Category = FilterCategories.NumbersAndReferences;
			itemIdFilter.SubGroup = itemFilterProcessor;

			var consignmentIdFilter = filters.AddTextFilter(Descriptions.ConsignmentId, HVLVConsignmentSchema.HVC_ConsignmentId);
			consignmentIdFilter.MultilingualDescription = ResString.GetMultilingualString("c8f08266-b63b-40cc-be6d-40e288c2b1ec", "Consignment ID");
			consignmentIdFilter.SupportsBlankComparisonOperators = false;
			consignmentIdFilter.Category = FilterCategories.NumbersAndReferences;
			consignmentIdFilter.SubGroup = consignmentFilterProcessor;

			var shipperReferenceFilter = filters.AddTextFilter(Descriptions.ShipperReference, HVLVConsignmentSchema.HVC_ShipperReference);
			shipperReferenceFilter.MultilingualDescription = ResString.GetMultilingualString("08c006aa-999b-4049-8251-80e359422988", "Shipper Reference");
			shipperReferenceFilter.Category = FilterCategories.NumbersAndReferences;
			shipperReferenceFilter.SubGroup = consignmentFilterProcessor;
		}

		class ItemSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var itemSubQuery = new ZDBOnlySubQuery(typeof(HVLVItem), HVLVItemSchema.HVI_JS_LoadedOnShipment);
				itemSubQuery.AddToFilter(filter);

				var shipmentQuery = new ZDBOnlyQuery(typeof(ForwardingShipment));
				shipmentQuery.AddSubQuery(itemSubQuery, JoinCondition.And);

				return shipmentQuery;
			}
		}

		class ConsignmentSubGroup : ModuleFilterSubGroup
		{
			public ConsignmentSubGroup(ModuleFilterSubGroup parent)
				: base(parent)
			{
			}

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var consignmentSubQuery = new ZDBOnlySubQuery(typeof(HVLVConsignment), HVLVItemSchema.HVI_HVC_Consignment);
				consignmentSubQuery.AddToFilter(filter);

				var itemQuery = new ZDBOnlyQuery(typeof(HVLVItem));
				itemQuery.AddSubQuery(consignmentSubQuery, JoinCondition.And);

				return itemQuery;
			}
		}
	}
}
