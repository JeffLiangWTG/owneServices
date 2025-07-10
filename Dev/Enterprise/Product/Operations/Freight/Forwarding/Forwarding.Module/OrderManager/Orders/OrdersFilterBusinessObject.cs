using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Module;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Orders.Module
{
	public class OrdersFilterBusinessObject : OrdersBaseFilterBusinessObject
	{
		#region GetContainerNoQuery

		protected override ZQuery GetContainerNoQuery(SQLComparisonOperator comparisonOperator, ZString containerNo)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(Order));

			// containers via JobOrderLineDeliverContainerSchema

			ZDBOnlyQuery dbOnlyResult1 = new ZDBOnlyQuery(typeof(Order));
			ZDBOnlySubQuery linesSubQuery = new ZDBOnlySubQuery(typeof(OrderLine), JobOrderLineSchema.JO_JD);
			ZDBOnlySubQuery deliveriesSubQuery = new ZDBOnlySubQuery(typeof(OrderLineDelivery), JobOrderLineDeliverySchema.J4_JO);
			ZDBOnlySubQuery containersSubQuery = new ZDBOnlySubQuery(typeof(OrderLineDeliverContainer), JobOrderLineDeliverContainerSchema.J5_J4);

			containersSubQuery.AddToFilter_PossiblyCommaSeparated(JobOrderLineDeliverContainerSchema.J5_ContainerNum, comparisonOperator, containerNo);
			deliveriesSubQuery.AddSubQuery(containersSubQuery, JoinCondition.And);
			linesSubQuery.AddSubQuery(deliveriesSubQuery, JoinCondition.And);

			// containers via JobOrderContainerSchema

			ZDBOnlySubQuery orderContainerSubQuery = new ZDBOnlySubQuery(typeof(OrderContainer), JobOrderContainerSchema.J1_ParentID);
			orderContainerSubQuery.AddToFilter_PossiblyCommaSeparated(JobOrderContainerSchema.J1_ContainerNumber, comparisonOperator, containerNo);
			linesSubQuery.AddAsUnionQuery(orderContainerSubQuery, addAsUnionAll: true);

			dbOnlyResult1.AddSubQuery(linesSubQuery, JoinCondition.And);

			// containers via JobContainerPackPivotSchema

			ZDBOnlyQuery dbOnlyResult2 = new ZDBOnlyQuery(typeof(Order));
			ZDBOnlySubQuery jobContainerSubQuery = new ZDBOnlySubQuery(typeof(CommonContainer), JobContainerPackPivotSchema.J6_JC);
			ZDBOnlySubQuery shipmentQuery = new ZDBOnlySubQuery(typeof(ForwardingShipment), JobOrderHeaderSchema.JD_JS);
			ZDBOnlySubQuery packLineSubQuery = new ZDBOnlySubQuery(typeof(ForwardingPackLine), JobPackLinesSchema.JL_JS);
			ZDBOnlySubQuery jobContainerPackPivotSubQuery = new ZDBOnlySubQuery(typeof(JobContainerPackPivot), JobContainerPackPivotSchema.J6_JL);

			jobContainerSubQuery.AddToFilter_PossiblyCommaSeparated(JoinCondition.And, JobContainerSchema.JC_ContainerNum, comparisonOperator, containerNo);
			jobContainerPackPivotSubQuery.AddSubQuery(jobContainerSubQuery, JoinCondition.And);
			packLineSubQuery.AddSubQuery(jobContainerPackPivotSubQuery, JoinCondition.And);
			shipmentQuery.AddSubQuery(packLineSubQuery, JoinCondition.And);
			dbOnlyResult2.AddSubQuery(shipmentQuery, JoinCondition.And);

			result.AddToFilter(dbOnlyResult1, JoinCondition.Or);
			result.AddToFilter(dbOnlyResult2, JoinCondition.Or);

			return result;
		}

		#endregion

		#region GetQuery Methods

		protected override ZQuery GetOrderLineQuery(ZQuery filter)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(Order));
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(OrderLine), JobOrderLineSchema.JO_JD);
			subQuery.AddToFilter(filter);
			result.AddSubQuery(subQuery, JoinCondition.And);
			return result;
		}

		protected override ZQuery GetConsolQuery(SchemaColumn consolSchemaColumn, SQLComparisonOperator comparisonOperator, IZType value)
		{
			return GetConsolQueryToOrder(consolSchemaColumn, comparisonOperator, value);
		}

		protected override ZQuery GetConsolQueryForMasterBill(SchemaColumn consolSchemaColumn, SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetConsolQueryForMasterBillToOrder(consolSchemaColumn, comparisonOperator, value);
		}

		protected override ZQuery GetShipmentQuery(SchemaColumn shipmentSchemaColumn, SQLComparisonOperator comparisonOperator, IZType value)
		{
			return GetShipmentQueryToOrder(shipmentSchemaColumn, comparisonOperator, value);
		}

		protected override ModuleFilterSubGroup GetOrderLineFilterProcessorCore()
		{
			return new OrderLineSubGroup();
		}

		protected override ModuleFilterSubGroup GetOrderHeaderFilterProcessorCore()
		{
			return ModuleFilterSubGroup.Default;
		}

		class OrderLineSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var lineSubQuery = new ZDBOnlySubQuery(typeof(OrderLine), JobOrderLineSchema.JO_JD);
				lineSubQuery.AddToFilter(filter);

				var orderQuery = new ZDBOnlyQuery(typeof(Order));
				orderQuery.AddSubQuery(lineSubQuery, JoinCondition.And);

				return orderQuery;
			}
		}

		#endregion

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filterCollection = base.GetModuleFiltersCore();
			securityProvider.AddCRMSecurityFilterStrips(Factory, filterCollection);

			var plannedNotifyParty2Filter = filterCollection.AddGuidFilter("Planned Notify Party 2", ModuleIDs.Organisation, OrgAddressSchema.OA_OH, BindingLists.OrgHeader_List);
			plannedNotifyParty2Filter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|OrdersBaseFilter|PlannedNotifyParty2", "Planned Notify Party 2");
			plannedNotifyParty2Filter.SubGroup = NotifyParty2FilterProcessor;
			plannedNotifyParty2Filter.SupportsFiltersMatchComparisonOperator = false;

			var actualNotifyParty2Filter = filterCollection.AddGuidFilter("Actual Notify Party 2", ModuleIDs.Organisation, GetActualNotifyParty2Query, BindingLists.OrgHeader_List);
			actualNotifyParty2Filter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|OrdersBaseFilter|ActualNotifyParty2", "Actual Notify Party 2");
			actualNotifyParty2Filter.SubGroup = OrderHeaderFilterProcessor;

			var plannedNotifyParty3Filter = filterCollection.AddGuidFilter("Planned Notify Party 3", ModuleIDs.Organisation, OrgAddressSchema.OA_OH, BindingLists.OrgHeader_List);
			plannedNotifyParty3Filter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|OrdersBaseFilter|PlannedNotifyParty3", "Planned Notify Party 3");
			plannedNotifyParty3Filter.SubGroup = NotifyParty3FilterProcessor;
			plannedNotifyParty3Filter.SupportsFiltersMatchComparisonOperator = false;

			var actualNotifyParty3Filter = filterCollection.AddGuidFilter("Actual Notify Party 3", ModuleIDs.Organisation, GetActualNotifyParty3Query, BindingLists.OrgHeader_List);
			actualNotifyParty3Filter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|OrdersBaseFilter|ActualNotifyParty3", "Actual Notify Party 3");
			actualNotifyParty3Filter.SubGroup = OrderHeaderFilterProcessor;

			var shipmentWindowDatesFilter = filterCollection.AddDateFilter(OrdersConstants.DateFilterTypes.ShipmentWindowStart, JobOrderHeaderSchema.JD_ShipmentWindowStart);
			shipmentWindowDatesFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|OrdersBaseFilter|ShipmentWindowStart", "Shipment Window Start Date");

			shipmentWindowDatesFilter = filterCollection.AddDateFilter(OrdersConstants.DateFilterTypes.ShipmentWindowEnd, JobOrderHeaderSchema.JD_ShipmentWindowEnd);
			shipmentWindowDatesFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|OrdersBaseFilter|ShipmentWindowEnd", "Shipment Window End Date");

			var isReleasedFilter = filterCollection.AddFlagFilter("Is Released", Res.GetString("f668de37-9b3f-49ca-964b-5b949abaa385", "Is Released"), JobOrderHeaderSchema.JD_IsReleased, ModuleFilterSubGroup.Default);
			isReleasedFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|OrdersBaseFilter|JD_IsReleased", "Is Released");

			return filterCollection;
		}

		protected internal override bool ShouldAddWorkflowFilters
		{
			get { return true; }
		}

		#region CRM Security

		readonly OrdersCRMSecurityProvider securityProvider = new OrdersCRMSecurityProvider();

		#endregion

		ZQuery GetActualNotifyParty2Query(ZGuid notifyPartyPK)
		{
			return GetActualNotifyPartyQuery(notifyPartyPK, DocAddressTypes.Codes.NotifyParty2);
		}

		ZQuery GetActualNotifyParty3Query(ZGuid notifyPartyPK)
		{
			return GetActualNotifyPartyQuery(notifyPartyPK, DocAddressTypes.Codes.NotifyParty3);
		}

		ModuleFilterSubGroup NotifyParty2FilterProcessor => notifyParty2FilterProcessor ?? (notifyParty2FilterProcessor = new JobDocAddressTypesSubGroup(OrderHeaderFilterProcessor, DocAddressTypes.Codes.NotifyParty2));
		JobDocAddressTypesSubGroup notifyParty2FilterProcessor;

		ModuleFilterSubGroup NotifyParty3FilterProcessor => notifyParty3FilterProcessor ?? (notifyParty3FilterProcessor = new JobDocAddressTypesSubGroup(OrderHeaderFilterProcessor, DocAddressTypes.Codes.NotifyParty3));
		JobDocAddressTypesSubGroup notifyParty3FilterProcessor;
	}
}
