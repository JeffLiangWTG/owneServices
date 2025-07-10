using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Module
{
	public static class ForwardingShipmentQueryHelper
	{
		public static ZDBOnlySubQuery GetHazardousShipmentsAgainstQuery(SchemaGuidColumn schemaColumnToFilterAgainst, bool isHazardous)
		{
			var shipmentFilter = new ZDBOnlySubQuery(typeof(ForwardingShipment), schemaColumnToFilterAgainst, notIn: !isHazardous);
			shipmentFilter.AddToFilter(JobShipmentSchema.JS_ShipmentType, SQLComparisonOperator.NotEqual, Constants.ShipmentTypes.AssemblyMaster);
			shipmentFilter.AddSubQuery(GetHazardousPackLinesQuery(), JoinCondition.And);

			return shipmentFilter;
		}

		static ZDBOnlySubQuery GetHazardousPackLinesQuery()
		{
			var commodityHazardousFilter = new ZDBOnlySubQuery(typeof(RefCommodityCode), RefCommodityCodeSchema.RH_Code);
			commodityHazardousFilter.AddToFilter(RefCommodityCodeSchema.RH_IsHazardous, true);

			var dangerousGoodFilter = new ZDBOnlySubQuery(typeof(UNDGDataItem), UNDGDataItemSchema.DI_ParentID);
			dangerousGoodFilter.AddToFilter(UNDGDataItemSchema.DI_IMOClass, SQLComparisonOperator.NotEqual, string.Empty);

			var packLineFilter = new ZDBOnlySubQuery(typeof(PackLine), JobPackLinesSchema.JL_JS);
			packLineFilter.AddToFilter(JobPackLinesSchema.JL_RH_NKCommodityCode, RefCommodityCode.HAZD);
			packLineFilter.AddSubQuery(JobPackLinesSchema.JL_RH_NKCommodityCode, commodityHazardousFilter, JoinCondition.Or);
			packLineFilter.AddSubQuery(dangerousGoodFilter, JoinCondition.Or);

			return packLineFilter;
		}
	}
}
