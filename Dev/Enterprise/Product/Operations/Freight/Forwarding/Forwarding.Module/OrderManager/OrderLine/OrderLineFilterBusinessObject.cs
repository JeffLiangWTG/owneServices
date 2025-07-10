using System.Diagnostics.CodeAnalysis;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Module;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Orders.Module
{
	public class OrderLineFilterBusinessObject : OrdersBaseFilterBusinessObject
	{
		#region GetModuleFilterThatOverridesAllOtherFilters - Order #

		protected override ZQuery GetOrderNoQuery(SQLComparisonOperator comparisonOperator, ZString orderNo)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(OrderLine));
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(Order), JobOrderLineSchema.JO_JD);
			subQuery.AddToFilter(base.GetOrderNoQuery(comparisonOperator, orderNo));

			result.AddSubQuery(subQuery, JoinCondition.And);

			return result;
		}

		#endregion

		#region Order Line Filters

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Constant strings")]
		public static class Filters
		{
			public const string ShipmentWindowStart = "Shipment Window Start";
			public const string ShipmentWindowEnd = "Shipment Window End";
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection collection = base.GetModuleFiltersCore();
			collection.AddNumberFilter("Order Line #", GetLineNumberQuery).MultilingualDescription = ResString.GetMultilingualString("Forwarding|OrdersBaseFilter|OrderLine", "Order Line #");
			collection.AddNumberFilter("Order Split Line #", GetSplitLineNumberQuery).MultilingualDescription = ResString.GetMultilingualString("Forwarding|OrderLineFilter|OrderSplitLine", "Order Split Line #");
			collection.AddTextFilter("Line Reference", GetLineReferenceQuery).MultilingualDescription = ResString.GetMultilingualString("Forwarding|OrderLineFilter|LineReference", "Line Reference");
			collection.AddTextFilter("H.S. Code", JobOrderLineSchema.JO_HSCode).MultilingualDescription = ResString.GetMultilingualString("Forwarding|OrderLineFilter|HSCode", "H.S. Code");

			collection.AddDateFilter(Filters.ShipmentWindowStart, JobOrderLineSchema.JO_ShipmentWindowStart)
				.MultilingualDescription = ResString.GetMultilingualString("Forwarding|OrderLineFilter|ShipmentWindowStart", "Shipment Window Start Date");
			collection.AddDateFilter(Filters.ShipmentWindowEnd, JobOrderLineSchema.JO_ShipmentWindowEnd)
				.MultilingualDescription = ResString.GetMultilingualString("Forwarding|OrderLineFilter|ShipmentWindowEnd", "Shipment Window End Date");

			SecurityProvider.AddCRMSecurityFilterStrips(Factory, collection);
			return collection;
		}

		ZQuery GetLineNumberQuery(SQLComparisonOperator comparisonOperator, ZString value) => ZInt.TryParse(value, out var lineNumber)
			? GetOrderLineQuery(new ZQuery().AddToFilter_PossiblyCommaSeparated(JobOrderLineSchema.JO_LineNo, SQLComparisonOperator.Equal, lineNumber))
			: ZQuery.NoResultQuery;

		ZQuery GetSplitLineNumberQuery(SQLComparisonOperator comparisonOperator, ZString value) => ZShort.TryParse(value, out var lineSplitNumber)
			? GetOrderLineQuery(new ZQuery().AddToFilter_PossiblyCommaSeparated(JobOrderLineSchema.JO_LineSplitNumber, SQLComparisonOperator.Equal, lineSplitNumber))
			: ZQuery.NoResultQuery;

		ZQuery GetLineReferenceQuery(SQLComparisonOperator comparisonOperator, ZString value) =>
			GetOrderLineQuery(new ZQuery().AddToFilter_PossiblyCommaSeparated(JobOrderLineSchema.JO_LineReference, comparisonOperator, value));

		#endregion

		#region GetContainerNoQuery

		protected override ZQuery GetContainerNoQuery(SQLComparisonOperator comparisonOperator, ZString containerNo)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(OrderLine));

			// containers via JobOrderLineDeliverContainerSchema
			var deliveriesSubQuery = new ZDBOnlySubQuery(typeof(OrderLineDelivery), JobOrderLineDeliverySchema.J4_JO);
			var containersSubQuery = new ZDBOnlySubQuery(typeof(OrderLineDeliverContainer), JobOrderLineDeliverContainerSchema.J5_J4);
			containersSubQuery.AddToFilter_PossiblyCommaSeparated(JoinCondition.And, JobOrderLineDeliverContainerSchema.J5_ContainerNum, comparisonOperator, containerNo);
			deliveriesSubQuery.AddSubQuery(containersSubQuery, JoinCondition.And);
			result.AddSubQuery(deliveriesSubQuery, JoinCondition.Or);

			// containers via JobOrderContainerSchema
			var orderContainerSubQuery = new ZDBOnlySubQuery(typeof(OrderContainer), JobOrderContainerSchema.J1_ParentID);
			orderContainerSubQuery.AddToFilter(JobOrderContainerSchema.J1_ParentTableCode, JobOrderHeaderSchema.Constants.Prefix);
			orderContainerSubQuery.AddToFilter_PossiblyCommaSeparated(JoinCondition.And, JobOrderContainerSchema.J1_ContainerNumber, comparisonOperator, containerNo);
			result.AddSubQuery(JobOrderLineSchema.JO_JD, orderContainerSubQuery, JoinCondition.Or);

			// containers via JobContainerPackPivotSchema
			var jobOrderHeaderSubQuery3 = new ZDBOnlySubQuery(typeof(Order), JobOrderHeaderSchema.PK);
			var jobPackLineSubQuery3 = new ZDBOnlySubQuery(typeof(PackLine), JobPackLinesSchema.JL_JS);
			var jobContainerPackPivotSubQuery3 = new ZDBOnlySubQuery(typeof(JobContainerPackPivot), JobContainerPackPivotSchema.J6_JL);
			var jobContainerSubQuery3 = new ZDBOnlySubQuery(typeof(CommonContainer), JobContainerSchema.PK);
			jobContainerSubQuery3.AddToFilter_PossiblyCommaSeparated(JoinCondition.And, JobContainerSchema.JC_ContainerNum, comparisonOperator, containerNo);
			jobContainerPackPivotSubQuery3.AddSubQuery(JobContainerPackPivotSchema.J6_JC, jobContainerSubQuery3, JoinCondition.And);
			jobPackLineSubQuery3.AddSubQuery(jobContainerPackPivotSubQuery3, JoinCondition.And);
			jobOrderHeaderSubQuery3.AddSubQuery(JobOrderHeaderSchema.JD_JS, jobPackLineSubQuery3, JoinCondition.And);
			result.AddSubQuery(JobOrderLineSchema.JO_JD, jobOrderHeaderSubQuery3, JoinCondition.Or);

			return result;
		}

		#endregion

		#region AddManufacturerQueryToSpecificOrderType

		protected override ZQuery GetDocAddressQueryToOrder(ZDBOnlySubQuery docAddressQuery, DocAddressType docAddressType)
		{
			var orderLineQuery = new ZDBOnlyQuery(typeof(OrderLine));

			var orderDocAddressQuery = (ZDBOnlySubQuery)docAddressQuery.ShallowClone();
			docAddressQuery.AddToFilter(JobDocAddressSchema.E2_ParentTableCode, JobOrderLineSchema.Constants.Prefix);
			orderLineQuery.AddSubQuery(docAddressQuery, JoinCondition.And);

			var orderQuery = new ZDBOnlySubQuery(typeof(OrderLine), JobOrderLineSchema.PK);
			orderDocAddressQuery.AddToFilter(JobDocAddressSchema.E2_ParentTableCode, JobOrderHeaderSchema.Constants.Prefix);
			orderQuery.AddSubQuery(JobOrderLineSchema.JO_JD, orderDocAddressQuery, JoinCondition.And);

			var notInSubQuery = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID, notIn: true);
			notInSubQuery.AddToFilter(JobDocAddressSchema.E2_AddressType, DocAddressTypes.GetCode(Factory, docAddressType));
			notInSubQuery.AddToFilter(JobDocAddressSchema.E2_ParentTableCode, JobOrderLineSchema.Constants.Prefix);
			orderQuery.AddSubQuery(notInSubQuery, JoinCondition.And);

			orderLineQuery.AddSubQuery(orderQuery, JoinCondition.Or);
			return orderLineQuery;
		}

		#endregion

		#region GetQuery Methods

		protected override ZQuery GetOrderHeaderQuery(ZQuery orderFilter)
		{
			ZDBOnlySubQuery orderSubQuery = new ZDBOnlySubQuery(typeof(Order), JobOrderLineSchema.JO_JD);
			orderSubQuery.AddToFilter(orderFilter);

			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(OrderLine));
			result.AddSubQuery(orderSubQuery, JoinCondition.And);

			return result;
		}

		protected override ZQuery GetConsolQuery(SchemaColumn consolSchemaColumn, SQLComparisonOperator comparisonOperator, IZType value)
		{
			ZDBOnlyQuery orderQuery = GetConsolQueryToOrder(consolSchemaColumn, comparisonOperator, value);
			return GetOrderHeaderQuery(orderQuery);
		}

		protected override ZQuery GetConsolQueryForMasterBill(SchemaColumn consolSchemaColumn, SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZDBOnlyQuery orderQuery = GetConsolQueryForMasterBillToOrder(consolSchemaColumn, comparisonOperator, value);
			return GetOrderHeaderQuery(orderQuery);
		}

		protected override ZQuery GetShipmentQuery(SchemaColumn shipmentSchemaColumn, SQLComparisonOperator comparisonOperator, IZType value)
		{
			ZDBOnlyQuery orderQuery = GetShipmentQueryToOrder(shipmentSchemaColumn, comparisonOperator, value);
			return GetOrderHeaderQuery(orderQuery);
		}

		protected override ModuleFilterSubGroup GetOrderLineFilterProcessorCore()
		{
			return ModuleFilterSubGroup.Default;
		}

		protected override ModuleFilterSubGroup GetOrderHeaderFilterProcessorCore()
		{
			return new OrderHeaderSubGroup();
		}

		class OrderHeaderSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var subQuery = new ZDBOnlySubQuery(typeof(Order), JobOrderLineSchema.JO_JD);
				subQuery.AddToFilter(filter);

				var result = new ZDBOnlyQuery(typeof(OrderLine));
				result.AddSubQuery(subQuery, JoinCondition.And);

				return result;
			}
		}

		#endregion

		protected internal override bool ShouldAddWorkflowFilters
		{
			get { return true; }
		}

		readonly OrderLineCRMSecurityProvider SecurityProvider = new OrderLineCRMSecurityProvider();

		protected override WorkflowFilterStripsHelperWithRoutingSupport GetWorkflowFilterStripsHelperWithRoutingSupport()
		{
			return new WorkflowFilterStripsHelperWithRoutingSupport(typeof(OrderLine), WorkflowDescriptors.OrderLineWorkflowDescriptorCode, Factory, false)
			{ ShouldAddMilestoneFilters = ShouldAddWorkflowFilters };
		}
	}
}
