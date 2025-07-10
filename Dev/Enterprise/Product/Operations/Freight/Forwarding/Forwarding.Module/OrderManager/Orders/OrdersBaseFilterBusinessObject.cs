using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Module;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using ResString = Enterprise.Freight.Forwarding.Module.ResString;

namespace Enterprise.Freight.Forwarding.Orders.Module
{
	public abstract class OrdersBaseFilterBusinessObject : FilterStripBusinessObject, ICustomLabelsConfigOrgProvider
	{
		#region GetModuleFilters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var collection = new ModuleFilterCollection();

			// Numbers and References
			var numberFilter = collection.AddNumberFilter("Order #", GetOrderNoQuery)
				.WithMaxLengthOf<ModuleNumberFilter>(JobOrderHeaderSchema.JD_OrderNumber);
			numberFilter.IsCommon = true;
			numberFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|OrdersBaseFilter|Order", "Order #");

			ModuleFilter filter = collection.AddNumberFilter("Booking Conf. Ref. #", JobOrderHeaderSchema.JD_BookingConfRef);
			filter.IsCommon = true;
			filter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|OrdersBaseFilter|BookingConfRef", "Booking Conf. Ref. #");
			filter.SubGroup = OrderHeaderFilterProcessor;

			filter = collection.AddNumberFilter("Container #", GetContainerNoQuery)
				.WithMaxLengthOf<ModuleNumberFilter>(JobOrderLineDeliverContainerSchema.J5_ContainerNum);
			filter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|OrdersBaseFilter|Container", "Container #");

			filter = collection.AddTextAndNkFilter("Planned Flight/Voyage # and Vessel", GetPlannedFlightVoyageNumberAndVesselQuery, ModuleIDs.RefVessel, BindingLists.RefVessel_List)
				.WithMaxLengthOf(JobOrderHeaderSchema.JD_DepartureVoyage, JobOrderHeaderSchema.JD_RV_NKDepartureVessel);
			filter.Category = FilterCategories.NumbersAndReferences;
			filter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|OrdersBaseFilter|PlannedFlightVoyageAndVessel", "Planned Flight/Voyage # and Vessel");
			filter.SubGroup = OrderHeaderFilterProcessor;

			filter = new VoyageVesselModuleFilter("Actual Flight/Voyage # and Vessel", GetActualFlightVoyageNumberAndVesselQuery, BindingLists.RefVessel_List)
				.WithMaxLengthOf(JobVoyageSchema.JV_VoyageFlight, JobVoyageSchema.JV_RV_NKVessel);
			filter.Category = FilterCategories.NumbersAndReferences;
			filter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|OrdersBaseFilter|ActualFlightVoyageAndVessel", "Actual Flight/Voyage # and Vessel");
			filter.SubGroup = OrderHeaderFilterProcessor;
			collection.AddCustomFilter(filter);

			filter = collection.AddNumberFilter("House Bill", GetHouseBillQuery)
				.WithMaxLengthOf<ModuleNumberFilter>(JobOrderHeaderSchema.JD_Waybill);
			filter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|OrdersBaseFilter|HouseBill", "House Bill");
			filter.SubGroup = OrderHeaderFilterProcessor;

			filter = collection.AddNumberFilter("Invoice #", JobOrderHeaderSchema.JD_InvoiceNumber);
			filter.IsCommon = true;
			filter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|OrdersBaseFilter|Invoice", "Invoice #");
			filter.SubGroup = OrderHeaderFilterProcessor;

			filter = collection.AddNumberFilter("Master Bill", GetMasterBillQuery)
				.WithMaxLengthOf<ModuleNumberFilter>(JobConsolSchema.JK_MasterBillNum);
			filter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|OrdersBaseFilter|MasterBill", "Master Bill");

			filter = collection.AddNumberFilter("Product #", JobOrderLineSchema.JO_Partno);
			filter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|OrdersBaseFilter|Product#", "Product #");
			filter.SubGroup = OrderLineFilterProcessor;

			filter = collection.AddFountainFilter("Shipment #", GetShipmentNoQuery, "S")
				.WithMaxLengthOf<ModuleFountainFilter>(JobShipmentSchema.JS_UniqueConsignRef);
			filter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|OrdersBaseFilter|Shipment", "Shipment #");

			var commodityCodeFilter = collection.AddNkFilter("Commodity Code", OrgSupplierPartSchema.OP_RH_NKCommodityCode, ModuleIDs.RefCommodityCode, BindingLists.RefCommodity_List);
			commodityCodeFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|OrdersBaseFilter|CommodityCode", "Commodity Code");
			commodityCodeFilter.Category = FilterCategories.NumbersAndReferences;
			commodityCodeFilter.SubGroup = OrgSupplierPartFilterProcessor;
			commodityCodeFilter.SupportsFiltersMatchComparisonOperator = false;

			var preAdviceFilter = collection.AddGuidFilter("Pre Advice #", ModuleIDs.JobShipmentPreplanning, JobOrderHeaderSchema.JD_EF_ShipmentPrePlanning, PreAdviceList);
			preAdviceFilter.Category = FilterCategories.NumbersAndReferences;
			preAdviceFilter.IsPublishedOnWeb = false;
			preAdviceFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|OrdersBaseFilter|PreAdvice", "Pre Advice #");
			preAdviceFilter.SubGroup = OrderHeaderFilterProcessor;
			preAdviceFilter.SupportsFiltersMatchComparisonOperator = false;

			// Dates
			filter = collection.AddDateFilter(OrdersConstants.DateFilterTypes.OrderDate, JobOrderHeaderSchema.JD_OrderDate);
			filter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|OrdersBaseFilter|OrderDate", "Order Date");
			filter.SubGroup = OrderHeaderFilterProcessor;

			filter = collection.AddDateFilter(OrdersConstants.DateFilterTypes.ConfirmedDate, JobOrderHeaderSchema.JD_BookingConfDate);
			filter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|OrdersBaseFilter|ConfirmedDate", "Confirmed Date");
			filter.SubGroup = OrderHeaderFilterProcessor;

			filter = collection.AddDateFilter(OrdersConstants.DateFilterTypes.FollowUpDate, JobOrderHeaderSchema.JD_FollowUpDate);
			filter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|OrdersBaseFilter|FollowUpDate", "Follow Up Date");
			filter.SubGroup = OrderHeaderFilterProcessor;

			filter = collection.AddDateFilter(OrdersConstants.DateFilterTypes.ReqInStore, JobOrderHeaderSchema.JD_DeliveryRequiredBy);
			filter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|OrdersBaseFilter|ReqInStore", "Required In Store");
			filter.SubGroup = OrderHeaderFilterProcessor;

			filter = collection.AddDateFilter(OrdersConstants.DateFilterTypes.ReqExWorks, JobOrderHeaderSchema.JD_ExWorksRequiredBy);
			filter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|OrdersBaseFilter|ReqExWorks", "Required Ex Works");
			filter.SubGroup = OrderHeaderFilterProcessor;

			// Organisations
			var buyerSupplier = new ModuleGuidsFilterForOrg((NoResString)"Buyer / Supplier", ModuleIDs.Organisation, GetOrderBuyerSupplierQuery, BindingLists.OrgConsignee_FilterList, BindingLists.OrgConsignor_FilterList);
			buyerSupplier.SetItemDescriptions(Res.GetData("Forwarding|OrdersBaseFilter|Buyer", "Buyer"), Res.GetData("Forwarding|OrdersBaseFilter|Supplier", "Supplier"));
			buyerSupplier.Property1Info.ValueChanged += delegate
			{ OnConfigOrgChanged(); };
			buyerSupplier.MultilingualDescription = ResString.GetMultilingualString("Forwarding|OrdersBaseFilter|BuyerSupplier", "Buyer / Supplier");
			buyerSupplier.SubGroup = OrderHeaderFilterProcessor;
			collection.AddFilter(buyerSupplier);
			OnConfigOrgChanged();

			ModuleFilterWithSubDescriptions filterWithSD = new ModuleGuidsFilterForOrg((NoResString)"Planned Send / Receive Agents", ModuleIDs.Organisation, GetPlannedOrderSendReceiveAgentsQuery, BindingLists.OrgForwarder_FilterList, BindingLists.OrgForwarder_FilterList);
			filterWithSD.SetItemDescriptions(Res.GetData("Forwarding|OrdersBaseFilter|SendAgent", "Send Agent"), Res.GetData("Forwarding|OrdersBaseFilter|RecAgent", "Rec. Agent"));
			filterWithSD.MultilingualDescription = ResString.GetMultilingualString("Forwarding|OrdersBaseFilter|PlannedSendReceiveAgents", "Planned Send / Receive Agents");
			filterWithSD.SubGroup = OrderHeaderFilterProcessor;
			collection.AddFilter(filterWithSD);

			filterWithSD = new ModuleGuidsFilterForOrg((NoResString)"Actual Send / Receive Agents", ModuleIDs.Organisation, GetActualOrderSendReceiveAgentsQuery, BindingLists.OrgForwarder_FilterList, BindingLists.OrgForwarder_FilterList);
			filterWithSD.SetItemDescriptions(Res.GetData("Forwarding|OrdersBaseFilter|SendAgent", "Send Agent"), Res.GetData("Forwarding|OrdersBaseFilter|RecAgent", "Rec. Agent"));
			filterWithSD.MultilingualDescription = ResString.GetMultilingualString("Forwarding|OrdersBaseFilter|ActualSendReceiveAgents", "Actual Send / Receive Agents");
			filterWithSD.SubGroup = OrderHeaderFilterProcessor;
			collection.AddFilter(filterWithSD);

			var controllingCustomerFilter = collection.AddGuidFilter("Controlling Customer", ModuleIDs.Organisation, OrgAddressSchema.OA_OH, BindingLists.OrgHeader_List);
			controllingCustomerFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|OrdersBaseFilter|ControllingCustomer", "Controlling Customer");
			controllingCustomerFilter.SubGroup = ControllingCustomerFilterProcessor;
			controllingCustomerFilter.SupportsFiltersMatchComparisonOperator = false;

			var registeredStaffFilter = collection.AddNkFilter("Registered Staff", JobOrderHeaderSchema.JD_SystemCreateUser, ModuleIDs.GlbStaff, new GlbStaffCollection(Factory));
			registeredStaffFilter.Category = FilterCategories.Organisations;
			registeredStaffFilter.IsPublishedOnWeb = false;
			registeredStaffFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|OrdersBaseFilter|RegisteredStaff", "Registered Staff");
			registeredStaffFilter.SubGroup = OrderHeaderFilterProcessor;
			registeredStaffFilter.SupportsFiltersMatchComparisonOperator = false;

			var buyerCompanyName = collection.AddTextFilter("Buyer Company Name", OrgHeaderSchema.OH_FullName);
			buyerCompanyName.Category = FilterCategories.Organisations;
			buyerCompanyName.MultilingualDescription = ResString.GetMultilingualString("Forwarding|OrdersBaseFilter|BuyerCompanyName", "Buyer Company Name");
			buyerCompanyName.SubGroup = BuyerCompanyNameFilterProcessor;

			var supplierCompanyName = collection.AddTextFilter("Supplier Company Name", OrgHeaderSchema.OH_FullName);
			supplierCompanyName.Category = FilterCategories.Organisations;
			supplierCompanyName.MultilingualDescription = ResString.GetMultilingualString("Forwarding|OrdersBaseFilter|SupplierCompanyName", "Supplier Company Name");
			supplierCompanyName.SubGroup = SupplierCompanyNameFilterProcessor;

			var plannedCarrierFilter = collection.AddGuidFilter("Planned Carrier", ModuleIDs.Organisation, JobOrderHeaderSchema.JD_OH_Carrier, BindingLists.OrgHeader_List);
			plannedCarrierFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|OrdersBaseFilter|PlannedCarrier", "Planned Carrier");
			plannedCarrierFilter.SubGroup = OrderHeaderFilterProcessor;
			plannedCarrierFilter.SupportsFiltersMatchComparisonOperator = false;

			var actualCarrierFilter = collection.AddGuidFilter("Actual Carrier", ModuleIDs.Organisation, GetActualCarrierQuery, BindingLists.OrgHeader_List);
			actualCarrierFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|OrdersBaseFilter|ActualCarrier", "Actual Carrier");
			actualCarrierFilter.SubGroup = OrderHeaderFilterProcessor;

			var clientAssignedStaffFilter = new OrderClientAssignedStaffModuleFilter("Client Assigned Staff", GetClientAssignedStaffQuery);
			clientAssignedStaffFilter.Category = FilterCategories.Organisations;
			clientAssignedStaffFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|OrdersBaseFilter|ClientAassignedStaff", "Client Assigned Staff");
			clientAssignedStaffFilter.SubGroup = OrderHeaderFilterProcessor;
			collection.AddCustomFilter(clientAssignedStaffFilter);

			var plannedNotifyPartyFilter = collection.AddGuidFilter("Planned Notify Party", ModuleIDs.Organisation, OrgAddressSchema.OA_OH, BindingLists.OrgHeader_List);
			plannedNotifyPartyFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|OrdersBaseFilter|PlannedNotifyParty", "Planned Notify Party");
			plannedNotifyPartyFilter.SubGroup = NotifyPartyFilterProcessor;
			plannedNotifyPartyFilter.SupportsFiltersMatchComparisonOperator = false;

			var actualNotifyPartyFilter = collection.AddGuidFilter("Actual Notify Party", ModuleIDs.Organisation, GetActualNotifyPartyQuery, BindingLists.OrgHeader_List);
			actualNotifyPartyFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|OrdersBaseFilter|ActualNotifyParty", "Actual Notify Party");
			actualNotifyPartyFilter.SubGroup = OrderHeaderFilterProcessor;

			var manufacturerNameFilter = collection.AddTextFilter("Manufacturer Company Name", (comparisonOperator, name) => GetDocAddressNameFilter(comparisonOperator, name, DocAddressType.Manufacturer));
			manufacturerNameFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|OrdersBaseFilter|ManufacturerCompanyName", "Manufacturer Company Name");

			var manufacturerFilter = collection.AddGuidFilter("Manufacturer", ModuleIDs.Organisation, pk => GetDocAddressFilter(pk, DocAddressType.Manufacturer), BindingLists.OrgHeader_List);
			manufacturerFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|OrdersBaseFilter|Manufacturer", "Manufacturer");

			var goodsAvailableAtNameFilter = collection.AddTextFilter("GoodsAvailableAt Company Name", (comparisonOperator, name) => GetDocAddressNameFilter(comparisonOperator, name, DocAddressType.GoodsAvailableAt));
			goodsAvailableAtNameFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|OrdersBaseFilter|goodsAvailableAtCompanyName", "Pickup Address Company Name");

			var goodsAvailableAtFilter = collection.AddGuidFilter("GoodsAvailableAt", ModuleIDs.Organisation, pk => GetDocAddressFilter(pk, DocAddressType.GoodsAvailableAt), BindingLists.OrgHeader_List);
			goodsAvailableAtFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|OrdersBaseFilter|goodsAvailableAt", "Pickup Address");

			var goodsDeliveredToNameFilter = collection.AddTextFilter("GoodsDeliveredTo Company Name", (comparisonOperator, name) => GetDocAddressNameFilter(comparisonOperator, name, DocAddressType.GoodsDeliveredTo));
			goodsDeliveredToNameFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|OrdersBaseFilter|GoodsDeliveredToCompanyName", "Delivery Address Company Name");

			var goodsDeliveredToFilter = collection.AddGuidFilter("GoodsDeliveredTo", ModuleIDs.Organisation, pk => GetDocAddressFilter(pk, DocAddressType.GoodsDeliveredTo), BindingLists.OrgHeader_List);
			goodsDeliveredToFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|OrdersBaseFilter|GoodsDeliveredTo", "Delivery Address");

			// Locations
			filterWithSD = collection.AddLocationFilter("Planned Load / Discharge", GetPlannedOrderLoadDischargePortsQuery, BindingLists.RefLocation_List, BindingLists.RefLocation_List);
			filterWithSD.SetItemDescriptions(Res.GetData("Forwarding|OrdersBaseFilter|PlannedLoad", "Load"), Res.GetData("Forwarding|OrdersBaseFilter|PlannedDischarge", "Discharge"));
			filterWithSD.MultilingualDescription = ResString.GetMultilingualString("Forwarding|OrdersBaseFilter|PlannedLoadDischarge", "Planned Load / Discharge");
			filterWithSD.SubGroup = OrderHeaderFilterProcessor;

			filterWithSD = collection.AddLocationFilter("Actual Load / Discharge", GetActualOrderLoadDischargePortsQuery, BindingLists.RefLocation_List, BindingLists.RefLocation_List);
			filterWithSD.SetItemDescriptions(Res.GetData("Forwarding|OrdersBaseFilter|ActualLoad", "Load"), Res.GetData("Forwarding|OrdersBaseFilter|ActualDischarge", "Discharge"));
			filterWithSD.MultilingualDescription = ResString.GetMultilingualString("Forwarding|OrdersBaseFilter|ActualLoadDischarge", "Actual Load / Discharge");
			filterWithSD.SubGroup = OrderHeaderFilterProcessor;

			filterWithSD = collection.AddLocationFilter("Planned Origin / Destination", GetPlannedOrderOriginDestinationQuery, BindingLists.RefLocation_List, BindingLists.RefLocation_List);
			filterWithSD.SetItemDescriptions(Res.GetData("Forwarding|OrdersBaseFilter|Origin", "Origin"), Res.GetData("Forwarding|OrdersBaseFilter|Dest", "Dest."));
			filterWithSD.MultilingualDescription = ResString.GetMultilingualString("Forwarding|OrdersBaseFilter|PlannedOriginDestination", "Planned Origin / Destination");
			filterWithSD.SubGroup = OrderHeaderFilterProcessor;

			filterWithSD = collection.AddLocationFilter("Actual Origin / Destination", GetActualOrderOriginDestinationQuery, BindingLists.RefLocation_List, BindingLists.RefLocation_List);
			filterWithSD.SetItemDescriptions(Res.GetData("Forwarding|OrdersBaseFilter|Origin", "Origin"), Res.GetData("Forwarding|OrdersBaseFilter|Dest", "Dest."));
			filterWithSD.MultilingualDescription = ResString.GetMultilingualString("Forwarding|OrdersBaseFilter|ActualOriginDestination", "Actual Origin / Destination");
			filterWithSD.SubGroup = OrderHeaderFilterProcessor;

			// Modes
			filter = collection.AddTextFilter("Container Mode", JobOrderHeaderSchema.JD_ContainerMode, GetJD_ContainerMode_List);
			filter.Category = FilterCategories.ModesAndTypes;
			filter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|OrdersBaseFilter|ContainerMode", "Container Mode");
			filter.SubGroup = OrderHeaderFilterProcessor;

			filter = collection.AddTextFilter("Transport Mode", JobOrderHeaderSchema.JD_TransportMode, OrdersConstants.GetTransportModeList());
			filter.Category = FilterCategories.ModesAndTypes;
			filter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|OrdersBaseFilter|TransportMode", "Transport Mode");
			filter.SubGroup = OrderHeaderFilterProcessor;

			var serviceLevelFilter = collection.AddNkFilter("Service Level", JobOrderHeaderSchema.JD_RS_NKServiceLevel_NI, ModuleIDs.ServiceLevel, BindingLists.RefServiceLevel_List);
			serviceLevelFilter.Category = FilterCategories.ModesAndTypes;
			serviceLevelFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|OrdersBaseFilter|ServiceLevel", "Service Level");
			serviceLevelFilter.SubGroup = OrderHeaderFilterProcessor;
			serviceLevelFilter.SupportsFiltersMatchComparisonOperator = false;

			// Other
			var whsConfigProductFilter = collection.AddGuidFilter("Product", ModuleIDs.WhsConfigProduct, OrgSupplierPartSchema.PK, ProductsCollection);
			whsConfigProductFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|OrdersBaseFilter|Product", "Product");
			whsConfigProductFilter.SubGroup = OrgSupplierPartFilterProcessor;
			whsConfigProductFilter.SupportsFiltersMatchComparisonOperator = false;

			var qtyInvoiced = new ModuleNumberRangeFilter("Order Line - Quantity Invoiced", GetOrderLineQtyInvoicedQuery);
			qtyInvoiced.PropertyType = ZCalcEditPropertyType.Decimal;
			qtyInvoiced.Category = FilterCategories.Other;
			qtyInvoiced.MultilingualDescription = ResString.GetMultilingualString("Forwarding|OrdersBaseFilter|QtyInvoiced", "Order Line - Quantity Invoiced");
			qtyInvoiced.SubGroup = OrderLineFilterProcessor;
			collection.AddCustomFilter(qtyInvoiced);

			var qtyReceived = new ModuleNumberRangeFilter("Order Line - Quantity Received", GetOrderLineQtyReceivedQuery);
			qtyReceived.PropertyType = ZCalcEditPropertyType.Decimal;
			qtyReceived.Category = FilterCategories.Other;
			qtyReceived.MultilingualDescription = ResString.GetMultilingualString("Forwarding|OrdersBaseFilter|QtyReceived", "Order Line - Quantity Received");
			qtyReceived.SubGroup = OrderLineFilterProcessor;
			collection.AddCustomFilter(qtyReceived);

			// Statuses and Flags
			filter = collection.AddTextFilter("Order Status", GetOrderStatusQuery, GetOrderStatus_List());
			filter.Category = FilterCategories.StatusAndFlags;
			filter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|OrdersBaseFilter|OrderStatus", "Order Status");
			filter.SubGroup = OrderHeaderFilterProcessor;

			filter = collection.AddTextFilter("Line Status", GetLineStatusQuery, new OrderStatusLists().GetOrderLineStatusList());
			filter.Category = FilterCategories.StatusAndFlags;
			filter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|OrdersBaseFilter|LineStatus", "Line Status");
			filter.SubGroup = OrderLineFilterProcessor;

			filter = collection.AddTextFilter("Attached / Unattached Orders", GetOrdersAttachedUnattachedQuery, OrdersAttachedState_List);
			filter.Category = FilterCategories.StatusAndFlags;
			filter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|OrdersBaseFilter|AttachedUnattachedOrders", "Attached / Unattached Orders");

			collection.AddAttributeFilters(GetAttributesWithoutUserTrackDates(), GetAttributeFilter);

			AddUserTrackDateFilters(collection);

			return collection;
		}

		protected override List<IFilterStripsHelper> GetCustomFilterStripsHelpersCore()
		{
			var helpers = base.GetCustomFilterStripsHelpersCore();
			var helper = GetWorkflowFilterStripsHelperWithRoutingSupport();

			helper.ShouldAddMiscFilters = ShouldAddWorkflowFilters;
			helper.SetShouldAddWorkflowCustomFieldsFilters(ShouldAddWorkflowFilters);
			helpers.Add(helper);

			return helpers;
		}

		protected virtual WorkflowFilterStripsHelperWithRoutingSupport GetWorkflowFilterStripsHelperWithRoutingSupport()
		{
			return new WorkflowFilterStripsHelperWithRoutingSupport(typeof(Order), WorkflowDescriptors.OrderWorkflowDescriptorCode, Factory, false)
			{ ShouldAddMilestoneFilters = ShouldAddWorkflowFilters };
		}

		IEnumerable<CustomAttribute> GetAttributesWithoutUserTrackDates()
		{
			return new AttributeManager().GetAllAttributes(AttributeManager.AttributeModules.Order, Buyer, LoggedInWebUsersOrg)
				.Where((a) => a.Key != Constants.CustomLabels.Order.UserTrackDate1
					&& a.Key != Constants.CustomLabels.Order.UserTrackDate2
					&& a.Key != Constants.CustomLabels.Order.UserTrackDate3
					&& a.Key != Constants.CustomLabels.Order.UserTrackDate4);
		}

		void AddUserTrackDateFilters(ModuleFilterCollection collection)
		{
			var orderAttributes = new AttributeManager().GetAllAttributes(AttributeManager.AttributeModules.Order, Buyer, LoggedInWebUsersOrg);

			var dateFilterTypes = new OrdersConstants.DateFilterTypes(this);

			AddUserTrackDateFilterIfRequired(orderAttributes, collection, Constants.CustomLabels.Order.UserTrackDate1,
				dateFilterTypes.E_CustomDate1, JobOrderHeaderSchema.JD_EstimateUserDate1,
				dateFilterTypes.A_CustomDate1, JobOrderHeaderSchema.JD_ActualUserDate1);

			AddUserTrackDateFilterIfRequired(orderAttributes, collection, Constants.CustomLabels.Order.UserTrackDate2,
				dateFilterTypes.E_CustomDate2, JobOrderHeaderSchema.JD_EstimateUserDate2,
				dateFilterTypes.A_CustomDate2, JobOrderHeaderSchema.JD_ActualUserDate2);

			AddUserTrackDateFilterIfRequired(orderAttributes, collection, Constants.CustomLabels.Order.UserTrackDate3,
				dateFilterTypes.E_CustomDate3, JobOrderHeaderSchema.JD_EstimateUserDate3,
				dateFilterTypes.A_CustomDate3, JobOrderHeaderSchema.JD_ActualUserDate3);

			AddUserTrackDateFilterIfRequired(orderAttributes, collection, Constants.CustomLabels.Order.UserTrackDate4,
				dateFilterTypes.E_CustomDate4, JobOrderHeaderSchema.JD_EstimateUserDate4,
				dateFilterTypes.A_CustomDate4, JobOrderHeaderSchema.JD_ActualUserDate4);
		}

		void AddUserTrackDateFilterIfRequired(IEnumerable<CustomAttribute> attributes, ModuleFilterCollection collection, string key,
			MultilingualString estimatedDescription, SchemaDateTimeColumn estimatedDateFilterColumn,
			MultilingualString actualDescription, SchemaDateTimeColumn actualDateFilterColumn)
		{
			var attribute = attributes.FirstOrDefault(item => item.Key == key);
			if (attribute != null)
			{
				MultilingualString estDescription = (Globals.IsWeb) ? estimatedDescription : ResString.GetMultilingualString("F893530D-6976-4E86-9CA6-1D4AFAF72856", "Estimated {0}", attribute.Caption);
				MultilingualString actDescription = (Globals.IsWeb) ? actualDescription : ResString.GetMultilingualString("909E218D-911E-406C-AD9D-4905E1F4F307", "Actual {0}", attribute.Caption);

				var estimatedUserTrackDateFilter = collection.AddDateFilter(string.Format(CultureInfo.InvariantCulture, "Estimated {0}", attribute.Key), estimatedDateFilterColumn); // Filter key should not be translated
				estimatedUserTrackDateFilter.MultilingualDescription = estDescription;
				estimatedUserTrackDateFilter.Category = FilterCategories.AttributeSearch;
				estimatedUserTrackDateFilter.SubGroup = OrderHeaderFilterProcessor;

				var actualUserTrackDateFilter = collection.AddDateFilter(string.Format(CultureInfo.InvariantCulture, "Actual {0}", attribute.Key), actualDateFilterColumn); // Filter key should not be translated
				actualUserTrackDateFilter.MultilingualDescription = actDescription;
				actualUserTrackDateFilter.Category = FilterCategories.AttributeSearch;
				actualUserTrackDateFilter.SubGroup = OrderHeaderFilterProcessor;
			}
		}

		protected internal abstract bool ShouldAddWorkflowFilters { get; }

		#region SubGroups

		ModuleFilterSubGroup OrgSupplierPartFilterProcessor => orgSupplierPartFilterProcessor ?? (orgSupplierPartFilterProcessor = new OrgSupplierPartSubGroup(OrderLineFilterProcessor));
		OrgSupplierPartSubGroup orgSupplierPartFilterProcessor;

		ModuleFilterSubGroup OrderLineFilterProcessor => orderLineSubGroupFilterProcessor ?? (orderLineSubGroupFilterProcessor = GetOrderLineFilterProcessorCore());
		ModuleFilterSubGroup orderLineSubGroupFilterProcessor;
		protected abstract ModuleFilterSubGroup GetOrderLineFilterProcessorCore();

		protected ModuleFilterSubGroup OrderHeaderFilterProcessor => orderHeaderFilterProcessor ?? (orderHeaderFilterProcessor = GetOrderHeaderFilterProcessorCore());
		ModuleFilterSubGroup orderHeaderFilterProcessor;
		protected abstract ModuleFilterSubGroup GetOrderHeaderFilterProcessorCore();
		ModuleFilterSubGroup BuyerCompanyNameFilterProcessor => buyerCompanyNameFilterProcessor ?? (buyerCompanyNameFilterProcessor = new BuyerCompanyNameSubGroup(OrderHeaderFilterProcessor));

		BuyerCompanyNameSubGroup buyerCompanyNameFilterProcessor;

		ModuleFilterSubGroup SupplierCompanyNameFilterProcessor => supplierCompanyNameFilterProcessor ?? (supplierCompanyNameFilterProcessor = new SupplierCompanyNameSubGroup(OrderHeaderFilterProcessor));
		SupplierCompanyNameSubGroup supplierCompanyNameFilterProcessor;

		ModuleFilterSubGroup ControllingCustomerFilterProcessor => controllingCustomerFilterProcessor ?? (controllingCustomerFilterProcessor = new JobDocAddressTypesSubGroup(OrderHeaderFilterProcessor, DocAddressTypes.Codes.ControllingCustomer));
		JobDocAddressTypesSubGroup controllingCustomerFilterProcessor;

		ModuleFilterSubGroup NotifyPartyFilterProcessor => notifyPartyFilterProcessor ?? (notifyPartyFilterProcessor = new JobDocAddressTypesSubGroup(OrderHeaderFilterProcessor, DocAddressTypes.Codes.NotifyParty));
		JobDocAddressTypesSubGroup notifyPartyFilterProcessor;

		class OrgSupplierPartSubGroup : ModuleFilterSubGroup
		{
			public OrgSupplierPartSubGroup(ModuleFilterSubGroup parent)
				: base(parent)
			{
			}

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var supplierPartQuery = new ZDBOnlySubQuery(typeof(OrgSupplierPart), OrgSupplierPartSchema.OP_PartNum);
				supplierPartQuery.AddToFilter(filter);

				var orderLineQuery = new ZDBOnlyQuery(typeof(OrderLine));
				orderLineQuery.AddSubQuery(JobOrderLineSchema.JO_Partno, OrgSupplierPartSchema.OP_PartNum, supplierPartQuery, JoinCondition.And);

				return orderLineQuery;
			}
		}

		class BuyerCompanyNameSubGroup : ModuleFilterSubGroup
		{
			public BuyerCompanyNameSubGroup(ModuleFilterSubGroup parent)
				: base(parent)
			{
			}

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var result = new ZDBOnlyQuery(typeof(Order));
				var orgAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), JobOrderHeaderSchema.JD_OA_BuyerAddress);
				var orgHeaderSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgAddressSchema.OA_OH);
				orgHeaderSubQuery.AddToFilter(filter);
				orgAddressSubQuery.AddSubQuery(orgHeaderSubQuery, JoinCondition.And);
				result.AddSubQuery(orgAddressSubQuery, JoinCondition.And);
				return result;
			}
		}

		class SupplierCompanyNameSubGroup : ModuleFilterSubGroup
		{
			public SupplierCompanyNameSubGroup(ModuleFilterSubGroup parent)
				: base(parent)
			{
			}

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var result = new ZDBOnlyQuery(typeof(Order));
				var orgAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), JobOrderHeaderSchema.JD_OA_SupplierAddress);
				var orgHeaderSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgAddressSchema.OA_OH);
				orgHeaderSubQuery.AddToFilter(filter);
				orgAddressSubQuery.AddSubQuery(orgHeaderSubQuery, JoinCondition.And);
				result.AddSubQuery(orgAddressSubQuery, JoinCondition.And);
				return result;
			}
		}

		protected class JobDocAddressTypesSubGroup : ModuleFilterSubGroup
		{
			public JobDocAddressTypesSubGroup(ModuleFilterSubGroup parent, string docAddressType)
				: base(parent)
			{
				addressType = docAddressType;
			}
			readonly string addressType;

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var docAddressQuery = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID);
				docAddressQuery.AddToFilter(JobDocAddressSchema.E2_AddressType, addressType);
				var orgAddressQuery = new ZDBOnlySubQuery(typeof(OrgAddress), JobDocAddressSchema.E2_OA_Address);
				orgAddressQuery.AddToFilter(filter);
				docAddressQuery.AddSubQuery(orgAddressQuery, JoinCondition.And);

				var orderQuery = new ZDBOnlyQuery(typeof(Order));
				orderQuery.AddSubQuery(docAddressQuery, JoinCondition.And);

				return orderQuery;
			}
		}

		#endregion

		protected override void OnModuleFiltersCreated()
		{
			moduleFiltersCreated = true;

			ModuleFilter anyOpenTaskAssignedToFilter = this["Any Open Task Assigned To"];
			if (anyOpenTaskAssignedToFilter != null)
			{
				anyOpenTaskAssignedToFilter.IsPublishedOnWeb = false;
			}

			ModuleFilter nextTaskAssignedToFilter = this["Next Task Assigned To"];
			if (nextTaskAssignedToFilter != null)
			{
				nextTaskAssignedToFilter.IsPublishedOnWeb = false;
			}
		}

		protected override void OnModuleFiltersReset()
		{
			moduleFiltersCreated = false;
		}

		bool moduleFiltersCreated;

		#endregion

		#region Numbers and References

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1075:DoNotUseLoopToAddOrConditionsToFilter", Justification = "Baseline")]
		protected virtual ZQuery GetOrderNoQuery(SQLComparisonOperator comparisonOperator, ZString orderNo)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(Order));

			if (orderNo.Contains('-') || orderNo.Contains(RawDataRegistry.Instance.MultiSearchSeparator.Value))
			{
				var multiOrderNo = new ZString[] { orderNo };
				if (orderNo.Contains(RawDataRegistry.Instance.MultiSearchSeparator.Value))
				{
					multiOrderNo = orderNo.Split(RawDataRegistry.Instance.MultiSearchSeparator.Value);
				}

				foreach (var thisOrderNo in multiOrderNo)
				{
					var subResult = new ZDBOnlyQuery(typeof(Order));
					if (thisOrderNo.Contains('-'))
					{
						var splitOrderNo = thisOrderNo.Split('-');
						var orderNumberSplitName = "Cast(" + JobOrderHeaderSchema.JD_OrderNumberSplit.Name + " As Varchar(10))";
						var orderNumberSplitValue = splitOrderNo[1].Replace("'", "''");

						if (comparisonOperator == SQLComparisonOperator.Equal)
						{
							subResult.AddToFilter_PossiblyCommaSeparated(JobOrderHeaderSchema.JD_OrderNumber, SQLComparisonOperator.Equal, splitOrderNo[0]);
							subResult.AddFilterAndZSQLParameterCollection(orderNumberSplitName + " = '" + orderNumberSplitValue + "'", null);
						}
						else
						{
							subResult.AddToFilter_PossiblyCommaSeparated(JobOrderHeaderSchema.JD_OrderNumber, SQLComparisonOperator.EndsWith, splitOrderNo[0]);
							subResult.AddFilterAndZSQLParameterCollection(orderNumberSplitName + " like '" + orderNumberSplitValue + "%'", null);
						}
					}
					if (!string.IsNullOrEmpty(thisOrderNo))
					{
						subResult.AddToFilter_PossiblyCommaSeparated(JoinCondition.Or, JobOrderHeaderSchema.JD_OrderNumber, comparisonOperator, thisOrderNo);
					}

					result.AddToFilter(subResult, comparisonOperator.IsNegativeSQLOperator() ? JoinCondition.And : JoinCondition.Or);
				}
			}
			else
			{
				result.AddToFilter_PossiblyCommaSeparated(JobOrderHeaderSchema.JD_OrderNumber, comparisonOperator, orderNo);
			}
			return result;
		}

		ZQuery GetHouseBillQuery(SQLComparisonOperator comparisonOperator, ZString houseBill)
		{
			var orderHeaderQuery = new ZDBOnlySubQuery(typeof(Order), JobOrderHeaderSchema.PK);
			orderHeaderQuery.AddToFilter_PossiblyCommaSeparated(JobOrderHeaderSchema.JD_Waybill, comparisonOperator, houseBill);

			var shipmentQuery = new ZDBOnlySubQuery(typeof(CommonShipment), JobOrderHeaderSchema.JD_JS);
			shipmentQuery.AddToFilter_PossiblyCommaSeparated(JoinCondition.And, JobShipmentSchema.JS_HouseBill, comparisonOperator, houseBill);

			var shipmentAndOrderHeaderQuery = new ZDBOnlySubQuery(typeof(Order), JobOrderHeaderSchema.PK);
			shipmentAndOrderHeaderQuery.AddSubQuery(shipmentQuery, JoinCondition.And);
			shipmentAndOrderHeaderQuery.AddToFilter(JobOrderHeaderSchema.JD_IsCancelled, false);
			shipmentAndOrderHeaderQuery.AddAsUnionQuery(orderHeaderQuery, true);

			var orderFilter = new ZDBOnlyQuery(typeof(Order));
			orderFilter.AddSubQuery(shipmentAndOrderHeaderQuery, JoinCondition.And);

			return orderFilter;
		}

		ZQuery GetMasterBillQuery(SQLComparisonOperator comparisonOperator, ZString masterBill)
		{
			ZQuery filter = new ZQuery();
			filter.DefaultJoinCondition = JoinCondition.Or;
			filter.AddToFilter(GetConsolQueryForMasterBill(JobConsolSchema.JK_MasterBillNum, comparisonOperator, masterBill));
			filter.AddToFilter(GetOrderHeaderQueryForMasterBill(JobOrderHeaderSchema.JD_MasterWaybill, comparisonOperator, masterBill));
			return filter;
		}

		ZQuery GetShipmentNoQuery(SQLComparisonOperator comparisonOperator, ZString shipmentNo)
		{
			if (comparisonOperator == SpecialComparisonOperator.IsBlank)
			{
				return GetOrderHeaderQuery(JobOrderHeaderSchema.JD_JS, SQLComparisonOperator.Equal, null);
			}

			return GetShipmentQuery(JobShipmentSchema.JS_UniqueConsignRef, comparisonOperator, shipmentNo);
		}

		#region GetContainerNoQuery

		protected abstract ZQuery GetContainerNoQuery(SQLComparisonOperator comparisonOperator, ZString shipmentNo);

		#endregion

		#region GetFlightVoyageNumberAndVesselQuery

		ZQuery GetPlannedFlightVoyageNumberAndVesselQuery(SQLComparisonOperator flightOrVoyageNoComparisonOperator, ZString flightOrVoyageNo, ZString vesselNK)
		{
			flightOrVoyageNo = flightOrVoyageNo.Left(Order.Schema.JD_DepartureVoyageMaxLength);
			vesselNK = vesselNK.Left(Order.Schema.JD_RV_NKDepartureVesselMaxLength);

			var departureFilter = new ZQuery();
			var intermediateFilter = new ZQuery();
			var arrivalFilter = new ZQuery();

			if (!flightOrVoyageNo.IsEmpty || flightOrVoyageNoComparisonOperator == SpecialComparisonOperator.IsBlank || flightOrVoyageNoComparisonOperator == SpecialComparisonOperator.IsNotBlank)
			{
				departureFilter.AddToFilter(JobOrderHeaderSchema.JD_DepartureVoyage, flightOrVoyageNoComparisonOperator, flightOrVoyageNo);
				intermediateFilter.AddToFilter(JobOrderHeaderSchema.JD_IntermediateVoyage, flightOrVoyageNoComparisonOperator, flightOrVoyageNo);
				arrivalFilter.AddToFilter(JobOrderHeaderSchema.JD_ArrivalVoyage, flightOrVoyageNoComparisonOperator, flightOrVoyageNo);
			}

			if (!vesselNK.IsEmpty || flightOrVoyageNoComparisonOperator == SpecialComparisonOperator.IsBlank || flightOrVoyageNoComparisonOperator == SpecialComparisonOperator.IsNotBlank)
			{
				departureFilter.AddToFilter(JobOrderHeaderSchema.JD_RV_NKDepartureVessel, flightOrVoyageNoComparisonOperator, vesselNK);
				intermediateFilter.AddToFilter(JobOrderHeaderSchema.JD_RV_NKIntermediateVessel, flightOrVoyageNoComparisonOperator, vesselNK);
				arrivalFilter.AddToFilter(JobOrderHeaderSchema.JD_RV_NKArrivalVessel, flightOrVoyageNoComparisonOperator, vesselNK);
			}

			var orderFilter = new ZDBOnlyQuery(typeof(Order));

			orderFilter.DefaultJoinCondition = JoinCondition.Or;
			orderFilter.AddToFilter(departureFilter);
			orderFilter.AddToFilter(intermediateFilter);
			orderFilter.AddToFilter(arrivalFilter);

			return orderFilter;
		}

		ZQuery GetActualFlightVoyageNumberAndVesselQuery(SQLComparisonOperator flightOrVoyageNoComparisonOperator, ZString flightOrVoyageNo, ZString vesselNK, ZBool includeArchived)
		{
			flightOrVoyageNo = flightOrVoyageNo.Left(Order.Schema.JD_DepartureVoyageMaxLength);
			vesselNK = vesselNK.Left(Order.Schema.JD_RV_NKDepartureVesselMaxLength);

			var builder = new SailingFilterBuilder(Factory);
			builder.Vessel = vesselNK;
			builder.VoyageFlight = flightOrVoyageNo;
			builder.VoyageFlightComparisonOperator = flightOrVoyageNoComparisonOperator;
			builder.IncludeArchived = includeArchived;

			var shipmentQuery = builder.ToShipmentFilter(SailingFilterBuilder.RelationshipFlags.AllSchedules);
			var consolQuery = builder.ToConsolFilter();
			var declarationQuery = new ZQuery();

			if (!flightOrVoyageNo.IsEmpty || flightOrVoyageNoComparisonOperator == SpecialComparisonOperator.IsBlank || flightOrVoyageNoComparisonOperator == SpecialComparisonOperator.IsNotBlank)
			{
				declarationQuery.AddToFilter(JobDeclarationSchema.JE_VoyageFlightNo, flightOrVoyageNoComparisonOperator, flightOrVoyageNo);
			}
			if (!vesselNK.IsEmpty || flightOrVoyageNoComparisonOperator == SpecialComparisonOperator.IsBlank || flightOrVoyageNoComparisonOperator == SpecialComparisonOperator.IsNotBlank)
			{
				declarationQuery.AddToFilter(JobDeclarationSchema.JE_VesselName, flightOrVoyageNoComparisonOperator, vesselNK); // or?
			}

			var orderQuery = new ZQuery();

			orderQuery.DefaultJoinCondition = JoinCondition.Or;
			orderQuery.AddToFilter(GetShipmentQueryToOrder(shipmentQuery));
			orderQuery.AddToFilter(GetConsolQueryToOrder(consolQuery));
			orderQuery.AddToFilter(GetDeclarationQueryToOrder(declarationQuery));

			return orderQuery;
		}

		#endregion

		#endregion

		#region Organisations

		ZQuery GetConsolOrgAddressQuery(ZGuid orgPK, SchemaColumn consolAddressForeignKeyColumn)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(ForwardingConsol));
			ZDBOnlySubQuery orgAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), consolAddressForeignKeyColumn);
			orgAddressSubQuery.AddToFilter(OrgAddressSchema.OA_OH, orgPK);
			result.AddSubQuery(orgAddressSubQuery, JoinCondition.And);
			return result;
		}

		protected virtual ZQuery GetOrderBuyerSupplierQuery(ZGuid buyerPK, ZGuid supplierPK)
		{
			var result = new ZDBOnlyQuery(typeof(Order));

			if (buyerPK.IsValid)
			{
				result.AddSubQuery(GetOrgAddressSubQuery(JobOrderHeaderSchema.JD_OA_BuyerAddress, buyerPK), JoinCondition.And);
			}

			if (supplierPK.IsValid)
			{
				result.AddSubQuery(GetOrgAddressSubQuery(JobOrderHeaderSchema.JD_OA_SupplierAddress, supplierPK), JoinCondition.And);
			}

			return result;
		}

		ZDBOnlySubQuery GetOrgAddressSubQuery(SchemaColumn orderAddressColumn, ZGuid orgPK)
		{
			var orgAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), orderAddressColumn);
			orgAddressSubQuery.AddToFilter(OrgAddressSchema.OA_OH, orgPK);
			return orgAddressSubQuery;
		}

		ZQuery GetPlannedOrderSendReceiveAgentsQuery(ZGuid sendingAgentPK, ZGuid receivingAgentPK)
		{
			return GetQuery(JobOrderHeaderSchema.JD_OH_SendingAgent, JobOrderHeaderSchema.JD_OH_ReceivingAgent, sendingAgentPK, receivingAgentPK);
		}

		ZQuery GetActualOrderSendReceiveAgentsQuery(ZGuid sendingAgentPK, ZGuid receivingAgentPK)
		{
			var consolQuery = new ZQuery();
			var declarationSendQuery = new ZQuery();
			var declarationReceiveQuery = new ZQuery();

			if (!sendingAgentPK.IsEmpty)
			{
				consolQuery.AddToFilter(GetConsolOrgAddressQuery(sendingAgentPK, JobConsolSchema.JK_OA_SendingForwarderAddress));
				declarationSendQuery.AddToFilter(JobDeclarationSchema.JE_OH_Forwarder, SQLComparisonOperator.Equal, sendingAgentPK);
				declarationSendQuery.AddToFilter(JoinCondition.And, JobDeclarationSchema.JE_MessageType, SQLComparisonOperator.Equal, (ZString)Core.Constants.Sales.Mode.Export);
			}
			if (!receivingAgentPK.IsEmpty)
			{
				consolQuery.AddToFilter(GetConsolOrgAddressQuery(receivingAgentPK, JobConsolSchema.JK_OA_ReceivingForwarderAddress));
				declarationReceiveQuery.AddToFilter(JobDeclarationSchema.JE_OH_Forwarder, SQLComparisonOperator.Equal, receivingAgentPK);
				declarationReceiveQuery.AddToFilter(JoinCondition.And, JobDeclarationSchema.JE_MessageType, SQLComparisonOperator.Equal, (ZString)Core.Constants.Sales.Mode.Import);
			}

			var declarationQuery = new ZQuery();
			declarationQuery.AddToFilter(declarationSendQuery);
			declarationQuery.AddToFilter(declarationReceiveQuery, JoinCondition.Or);

			var orderQuery = new ZQuery();
			orderQuery.DefaultJoinCondition = JoinCondition.Or;
			orderQuery.AddToFilter(GetConsolQueryToOrder(consolQuery));
			orderQuery.AddToFilter(GetDeclarationQueryToOrder(declarationQuery));

			return orderQuery;
		}

		ZQuery GetActualCarrierQuery(ZGuid carrierPK)
		{
			var declarationSubQuery = new ZQuery(JobDeclarationSchema.JE_OH_ShippingLine, SQLComparisonOperator.Equal, carrierPK);

			var consolShippingAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), JobConsolSchema.JK_OA_ShippingLineAddress);
			consolShippingAddressSubQuery.AddToFilter(OrgAddressSchema.OA_OH, carrierPK);

			var transportCarrierAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), JobConsolTransportSchema.JW_OA_CarrierAddress);
			transportCarrierAddressSubQuery.AddToFilter(OrgAddressSchema.OA_OH, carrierPK);

			var notIncludeSeaLinkedTransportsQuery = new ZQuery();
			notIncludeSeaLinkedTransportsQuery.AddToFilter(JoinCondition.Or, JobConsolTransportSchema.JW_TransportMode, SQLComparisonOperator.NotEqual, Constants.TransportModes.Sea);
			notIncludeSeaLinkedTransportsQuery.AddToFilter(JoinCondition.Or, JobConsolTransportSchema.JW_JX, null);

			var consolTransportSubQuery = new ZDBOnlySubQuery(typeof(Transport), JobConsolTransportSchema.JW_ParentGUID);
			consolTransportSubQuery.AddToFilter(notIncludeSeaLinkedTransportsQuery, JoinCondition.And);
			consolTransportSubQuery.AddSubQuery(transportCarrierAddressSubQuery, JoinCondition.And);

			// Linked Sea Transport
			var voyageSubQuery = new ZDBOnlySubQuery(typeof(JobVoyage), JobVoyOriginSchema.JA_JV);
			voyageSubQuery.AddToFilter(JobVoyageSchema.JV_OH_Line, carrierPK);

			var originSubQuery = new ZDBOnlySubQuery(typeof(VoyageOrigin), JobSailingSchema.JX_JA);
			originSubQuery.AddSubQuery(voyageSubQuery, JoinCondition.And);

			var sailingSubQuery = new ZDBOnlySubQuery(typeof(JobSailing), JobConsolTransportSchema.JW_JX);
			sailingSubQuery.AddSubQuery(originSubQuery, JoinCondition.And);

			var consolLinkedSeaTransportSubQuery = new ZDBOnlySubQuery(typeof(Transport), JobConsolTransportSchema.JW_ParentGUID);
			consolLinkedSeaTransportSubQuery.AddSubQuery(sailingSubQuery, JoinCondition.And);

			var consolQuery = new ZDBOnlyQuery(typeof(ForwardingConsol));
			consolQuery.AddSubQuery(consolShippingAddressSubQuery, JoinCondition.Or);
			consolQuery.AddSubQuery(consolTransportSubQuery, JoinCondition.Or);
			consolQuery.AddSubQuery(consolLinkedSeaTransportSubQuery, JoinCondition.Or);

			var orderQuery = new ZQuery();
			orderQuery.DefaultJoinCondition = JoinCondition.Or;
			orderQuery.AddToFilter(GetDeclarationQueryToOrder(declarationSubQuery));
			orderQuery.AddToFilter(GetConsolQueryToOrder(consolQuery));

			return orderQuery;
		}

		ZQuery GetActualNotifyPartyQuery(ZGuid notifyPartyPK)
		{
			return GetActualNotifyPartyQuery(notifyPartyPK, DocAddressTypes.Codes.NotifyParty);
		}

		protected ZQuery GetActualNotifyPartyQuery(ZGuid notifyPartyPK, ZString addressTypeCode)
		{
			var declarationNotifyPartySubQuery = new ZQuery(JobDeclarationSchema.JE_OH_NotifyParty, SQLComparisonOperator.Equal, notifyPartyPK);

			var shipmentNotifyPartyQuery = new ZDBOnlyQuery(typeof(ForwardingShipment));
			var shipmentNotifyPartyJobDocAddressFilter = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID);
			var shipmentNotifyPartyOrgDocAddressFilter = new ZDBOnlySubQuery(typeof(OrgAddress), JobDocAddressSchema.E2_OA_Address);

			shipmentNotifyPartyOrgDocAddressFilter.AddToFilter(OrgAddressSchema.OA_OH, notifyPartyPK);
			shipmentNotifyPartyJobDocAddressFilter.AddSubQuery(shipmentNotifyPartyOrgDocAddressFilter, JoinCondition.And);
			shipmentNotifyPartyJobDocAddressFilter.AddToFilter(JobDocAddressSchema.E2_AddressType, addressTypeCode);

			shipmentNotifyPartyQuery.AddSubQuery(shipmentNotifyPartyJobDocAddressFilter, JoinCondition.And);
			var orderQuery = new ZQuery();
			orderQuery.DefaultJoinCondition = JoinCondition.Or;
			orderQuery.AddToFilter(GetDeclarationQueryToOrder(declarationNotifyPartySubQuery));
			orderQuery.AddToFilter(GetShipmentQueryToOrder(shipmentNotifyPartyQuery));
			return orderQuery;
		}

		ZQuery GetDocAddressNameFilter(SQLComparisonOperator comparisonOperator, ZString name, DocAddressType docAddressType)
		{
			var orgHeaderSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgHeaderSchema.PK);
			orgHeaderSubQuery.AddToFilter(OrgHeaderSchema.OH_FullName, comparisonOperator, name);

			var orgAddressQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.PK);
			orgAddressQuery.AddSubQuery(OrgAddressSchema.OA_OH, orgHeaderSubQuery, JoinCondition.And);

			var docAddressQuery = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID);
			docAddressQuery.AddSubQuery(JobDocAddressSchema.E2_OA_Address, orgAddressQuery, JoinCondition.And);
			docAddressQuery.AddToFilter(JobDocAddressSchema.E2_AddressType, DocAddressTypes.GetCode(Factory, docAddressType));
			docAddressQuery.AddToFilter(JobDocAddressSchema.E2_AddressOverride, false);

			var addressOverrideQuery = new ZQuery();
			addressOverrideQuery.AddToFilter(JobDocAddressSchema.E2_CompanyName, comparisonOperator, name);
			addressOverrideQuery.AddToFilter(JobDocAddressSchema.E2_AddressOverride, true);
			addressOverrideQuery.AddToFilter(JobDocAddressSchema.E2_AddressType, DocAddressTypes.GetCode(Factory, docAddressType));
			docAddressQuery.AddToFilter(addressOverrideQuery, JoinCondition.Or);

			return GetDocAddressQueryToOrder(docAddressQuery, docAddressType);
		}

		ZQuery GetDocAddressFilter(ZGuid pk, DocAddressType docAddressType)
		{
			var orgHeaderSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgHeaderSchema.PK);
			orgHeaderSubQuery.AddToFilter(OrgHeaderSchema.PK, pk);

			var orgAddressQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.PK);
			orgAddressQuery.AddSubQuery(OrgAddressSchema.OA_OH, orgHeaderSubQuery, JoinCondition.And);

			var docAddressQuery = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID);
			docAddressQuery.AddSubQuery(JobDocAddressSchema.E2_OA_Address, orgAddressQuery, JoinCondition.And);
			docAddressQuery.AddToFilter(JobDocAddressSchema.E2_AddressType, DocAddressTypes.GetCode(Factory, docAddressType));

			return GetDocAddressQueryToOrder(docAddressQuery, docAddressType);
		}

		protected virtual ZQuery GetDocAddressQueryToOrder(ZDBOnlySubQuery docAddressQuery, DocAddressType docAddressType)
		{
			var orderQuery = new ZDBOnlyQuery(typeof(Order));
			var clonedDocAddressQuery = (ZDBOnlySubQuery)docAddressQuery.ShallowClone();
			docAddressQuery.AddToFilter(JobDocAddressSchema.E2_ParentTableCode, JobOrderHeaderSchema.Constants.Prefix);
			orderQuery.AddSubQuery(docAddressQuery, JoinCondition.And);

			clonedDocAddressQuery.AddToFilter(JobDocAddressSchema.E2_ParentTableCode, JobOrderLineSchema.Constants.Prefix);
			var orderLineQuery = new ZDBOnlySubQuery(typeof(OrderLine), JobOrderLineSchema.JO_JD);
			orderLineQuery.AddSubQuery(JobOrderLineSchema.PK, clonedDocAddressQuery, JoinCondition.And);

			orderQuery.AddSubQuery(orderLineQuery, JoinCondition.Or);
			return orderQuery;
		}

		protected ZQuery GetClientAssignedStaffQuery(ZDBOnlySubQuery orgFilter)
		{
			var orderQuery = new ZDBOnlyQuery(typeof(Order));
			orderQuery.AddSubQuery(orgFilter, JoinCondition.And);

			return orderQuery;
		}

		#endregion

		#region Locations

		ZQuery GetPlannedOrderLoadDischargePortsQuery(ZString loadPortNK, ZString dischargePortNK)
		{
			var filter = new ZQuery();
			if (!loadPortNK.IsEmpty)
			{
				filter.AddToFilter(LocationHelper.GetLocationFilter(Factory, loadPortNK, JobOrderHeaderSchema.JD_RL_NKPortOfLoading, typeof(Order)));
			}

			if (!dischargePortNK.IsEmpty)
			{
				filter.AddToFilter(LocationHelper.GetLocationFilter(Factory, dischargePortNK, JobOrderHeaderSchema.JD_RL_NKPortOfDischarge, typeof(Order)));
			}

			return filter;
		}

		ZQuery GetActualOrderLoadDischargePortsQuery(ZString loadPortNK, ZString dischargePortNK)
		{
			var builder = new SailingFilterBuilder(Factory);
			builder.LoadPort = loadPortNK;
			builder.DischargePort = dischargePortNK;

			var shipmentQuery = builder.ToShipmentFilter(SailingFilterBuilder.RelationshipFlags.ViaConsol);
			var consolQuery = builder.ToConsolFilter();
			var declarationQuery = new ZQuery();

			if (!loadPortNK.IsEmpty)
			{
				declarationQuery.AddToFilter(LocationHelper.GetLocationFilter(Factory, loadPortNK, JobDeclarationSchema.JE_RL_NKPortOfLoading, typeof(Enterprise.Integration.Customs.IBaseJobDeclaration)));
			}
			if (!dischargePortNK.IsEmpty)
			{
				declarationQuery.AddToFilter(LocationHelper.GetLocationFilter(Factory, dischargePortNK, JobDeclarationSchema.JE_RL_NKPortOfArrival, typeof(Enterprise.Integration.Customs.IBaseJobDeclaration)));
			}

			var orderQuery = new ZQuery();
			orderQuery.DefaultJoinCondition = JoinCondition.Or;
			orderQuery.AddToFilter(GetShipmentQueryToOrder(shipmentQuery));
			orderQuery.AddToFilter(GetConsolQueryToOrder(consolQuery));
			orderQuery.AddToFilter(GetDeclarationQueryToOrder(declarationQuery));

			return orderQuery;
		}

		ZQuery GetPlannedOrderOriginDestinationQuery(ZString originNK, ZString destinationNK)
		{
			var filter = new ZQuery();

			if (!originNK.IsEmpty)
			{
				filter.AddToFilter(LocationHelper.GetLocationFilter(Factory, originNK, JobOrderHeaderSchema.JD_RL_NKGoodsAvailableAt, typeof(Order)));
			}
			if (!destinationNK.IsEmpty)
			{
				filter.AddToFilter(LocationHelper.GetLocationFilter(Factory, destinationNK, JobOrderHeaderSchema.JD_RL_NKGoodsDeliveredTo, typeof(Order)));
			}

			return filter;
		}

		ZQuery GetActualOrderOriginDestinationQuery(ZString originNK, ZString destinationNK)
		{
			var shipmentQuery = new ZQuery();
			var declarationQuery = new ZQuery();

			if (!originNK.IsEmpty)
			{
				shipmentQuery.AddToFilter(LocationHelper.GetLocationFilter(Factory, originNK, JobShipmentSchema.JS_RL_NKOrigin, typeof(ForwardingShipment)));
				declarationQuery.AddToFilter(LocationHelper.GetLocationFilter(Factory, originNK, JobDeclarationSchema.JE_RL_NKOrigin, typeof(Enterprise.Integration.Customs.IBaseJobDeclaration)));
			}
			if (!destinationNK.IsEmpty)
			{
				shipmentQuery.AddToFilter(LocationHelper.GetLocationFilter(Factory, destinationNK, JobShipmentSchema.JS_RL_NKDestination, typeof(ForwardingShipment)));
				declarationQuery.AddToFilter(LocationHelper.GetLocationFilter(Factory, destinationNK, JobDeclarationSchema.JE_RL_NKFinalDestination, typeof(Enterprise.Integration.Customs.IBaseJobDeclaration)));
			}

			var orderQuery = new ZQuery();
			orderQuery.DefaultJoinCondition = JoinCondition.Or;
			orderQuery.AddToFilter(GetShipmentQueryToOrder(shipmentQuery));
			orderQuery.AddToFilter(GetDeclarationQueryToOrder(declarationQuery));

			return orderQuery;
		}

		#endregion

		#region Status and Flags Filters

		public static readonly string UndeliveredOrderStatus = "UND";

		ZQuery GetOrderStatusQuery(ZString orderStatus)
		{
			var result = new ZQuery();
			if (!orderStatus.IsEmpty && orderStatus != Constants.OrderStatus.All)
			{
				if (orderStatus == UndeliveredOrderStatus)
				{
					result = new ZQuery(JobOrderHeaderSchema.JD_OrderStatus, SQLComparisonOperator.NotEqual, new ZString(Constants.OrderStatus.Delivered));
				}
				else
				{
					result = new ZQuery(JobOrderHeaderSchema.JD_OrderStatus, SQLComparisonOperator.Equal, orderStatus);
				}
			}
			return result;
		}

		ZQuery GetLineStatusQuery(ZString lineStatus)
		{
			return new ZQuery(JobOrderLineSchema.JO_LineStatus, lineStatus);
		}

		#region GetOrdersAttachedUnattachedQuery

		ZQuery GetOrdersAttachedUnattachedQuery(ZString attachState)
		{
			ZQuery result = new ZQuery();

			if (attachState.EqualsIgnoringCase(OrdersConstants.OrdersAttachedState.AttachedOnly))
			{
				result.AddToFilter(GetOrdersAttachedUnattachedQueryInternal(true));
			}
			else if (attachState.EqualsIgnoringCase(OrdersConstants.OrdersAttachedState.UnattachedOnly))
			{
				result.AddToFilter(GetOrdersAttachedUnattachedQueryInternal(false));
			}

			return result;
		}

		ZQuery GetOrdersAttachedUnattachedQueryInternal(bool attachedOrders)
		{
			ZQuery result = new ZQuery();

			SQLComparisonOperator comparisonOperator = attachedOrders ? SQLComparisonOperator.NotEqual : SQLComparisonOperator.Equal;
			JoinCondition joinCondition = attachedOrders ? JoinCondition.Or : JoinCondition.And;

			ZQuery shipmentAttachedOrdersFilter = GetOrderHeaderQuery(JobOrderHeaderSchema.JD_JS, comparisonOperator, null);
			ZQuery declarationAttachedOrdersFilter = GetOrderHeaderQuery(JobOrderHeaderSchema.JD_JE, comparisonOperator, null);
			result.AddToFilter(shipmentAttachedOrdersFilter);
			result.AddToFilter(declarationAttachedOrdersFilter, joinCondition);

			return result;
		}

		#endregion

		#endregion

		#region Atttribute Filters

		public OrgHeader LoggedInWebUsersOrg
		{
			get { return fLoggedInWebUsersOrg; }
			set { fLoggedInWebUsersOrg = value; }
		}
		OrgHeader fLoggedInWebUsersOrg;

		ZQuery GetAttributeFilter(ZQuery filter, SchemaColumn column)
		{
			if (column.TableSchema == JobOrderHeaderSchema.Instance)
			{
				return GetOrderHeaderQuery(filter);
			}
			if (column.TableSchema == JobOrderLineSchema.Instance)
			{
				return GetOrderLineQuery(filter);
			}

			throw new NotImplementedException(string.Format(CultureInfo.InvariantCulture, "{0} schema is not supported", column.TableSchema));
		}

		#endregion

		#region Quantity Received / Invoiced

		protected virtual ZQuery GetOrderLineQtyInvoicedQuery(INumericZType qtyFrom, INumericZType qtyTo)
		{
			var query = GetOrderLineNumberRangeFilterQuery(JobOrderLineSchema.JO_QtyInvoiced, qtyFrom, qtyTo);
			return query;
		}

		protected virtual ZQuery GetOrderLineQtyReceivedQuery(INumericZType qtyFrom, INumericZType qtyTo)
		{
			var query = GetOrderLineNumberRangeFilterQuery(JobOrderLineSchema.JO_QtyReceived, qtyFrom, qtyTo);
			return query;
		}

		#endregion

		#region GetQuery Methods

		#region Shipment

		protected ZDBOnlyQuery GetShipmentQueryToOrder(SchemaColumn shipmentSchemaColumn, SQLComparisonOperator comparisonOperator, IZType value)
		{
			ZQuery shipmentQuery = new ZQuery().AddToFilter_PossiblyCommaSeparated(shipmentSchemaColumn, comparisonOperator, value);

			return GetShipmentQueryToOrder(shipmentQuery);
		}

		protected ZDBOnlyQuery GetShipmentQueryToOrder(ZQuery shipmentQuery)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(Order));
			ZDBOnlySubQuery shipmentSubQuery = new ZDBOnlySubQuery(typeof(CommonShipment), JobOrderHeaderSchema.JD_JS);

			shipmentSubQuery.AddToFilter(shipmentQuery);
			result.AddSubQuery(shipmentSubQuery, JoinCondition.And);

			return result;
		}

		#endregion

		#region Consol

		protected ZDBOnlyQuery GetConsolQueryForMasterBillToOrder(SchemaColumn consolSchemaColumn, SQLComparisonOperator comparisonOperator, ZString value)
		{
			var alteredValue = value.Replace(" ", "").Replace("-", "");

			ZQuery consolQuery = new ZQuery { DefaultJoinCondition = JoinCondition.Or };
			consolQuery.AddToFilter_PossiblyCommaSeparated(consolSchemaColumn, comparisonOperator, value);
			consolQuery.AddToFilter_PossiblyCommaSeparated(consolSchemaColumn, comparisonOperator, alteredValue);

			return GetConsolQueryToOrder(consolQuery);
		}

		protected ZDBOnlyQuery GetConsolQueryToOrder(SchemaColumn consolSchemaColumn, SQLComparisonOperator comparisonOperator, IZType value)
		{
			ZQuery consolQuery = new ZQuery().AddToFilter_PossiblyCommaSeparated(consolSchemaColumn, comparisonOperator, value);
			return GetConsolQueryToOrder(consolQuery);
		}

		protected ZDBOnlyQuery GetConsolQueryToOrder(ZQuery consolQuery)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(Order));
			ZDBOnlySubQuery shipmentSubQuery = new ZDBOnlySubQuery(typeof(CommonShipment), JobOrderHeaderSchema.JD_JS);
			ZDBOnlySubQuery pivotSubQuery = new ZDBOnlySubQuery(typeof(JobConShipLink), JobConShipLinkSchema.JN_JS);
			ZDBOnlySubQuery consolSubQuery = new ZDBOnlySubQuery(typeof(ForwardingConsol), JobConShipLinkSchema.JN_JK);

			consolSubQuery.AddToFilter(consolQuery);
			pivotSubQuery.AddSubQuery(consolSubQuery, JoinCondition.And);
			shipmentSubQuery.AddSubQuery(pivotSubQuery, JoinCondition.And);
			result.AddSubQuery(shipmentSubQuery, JoinCondition.And);

			return result;
		}

		#endregion

		#region Declaration

		protected ZDBOnlyQuery GetDeclarationQueryToOrder(SchemaColumn declarationSchemaColumn, SQLComparisonOperator comparisonOperator, IZType value)
		{
			ZQuery declarationQuery = new ZQuery(declarationSchemaColumn, comparisonOperator, value);

			return GetDeclarationQueryToOrder(declarationQuery);
		}

		protected ZDBOnlyQuery GetDeclarationQueryToOrder(ZQuery declarationQuery)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(Order));
			ZDBOnlySubQuery declarationSubQuery = new ZDBOnlySubQuery(typeof(Enterprise.Integration.Customs.IBaseJobDeclaration), JobOrderHeaderSchema.JD_JE);

			declarationSubQuery.AddToFilter(declarationQuery);
			result.AddSubQuery(declarationSubQuery, JoinCondition.And);

			return result;
		}
		#endregion

		protected virtual ZQuery GetOrderLineQuery(ZQuery filter)
		{
			return filter;
		}

		protected virtual ZQuery GetOrderHeaderQuery(ZQuery filter)
		{
			return filter;
		}

		protected virtual ZQuery GetOrderHeaderQuery(SchemaColumn orderSchemaColumn, SQLComparisonOperator comparisonOperator, IZType value)
		{
			return GetOrderHeaderQuery(new ZQuery().AddToFilter_PossiblyCommaSeparated(orderSchemaColumn, comparisonOperator, value));
		}

		protected virtual ZQuery GetOrderHeaderQuery(DateComparisonOperator comparisonOperator, SchemaDateTimeColumn orderSchemaColumn, ZDateTime date1, ZDateTime date2)
		{
			ZQuery filter = new ZQuery();
			AddDateRange(filter, comparisonOperator, JoinCondition.And, orderSchemaColumn, date1.Date, date2.Date);

			return GetOrderHeaderQuery(filter);
		}

		protected virtual ZQuery GetOrderHeaderQueryForMasterBill(SchemaColumn orderSchemaColumn, SQLComparisonOperator comparisonOperator, ZString value)
		{
			var alteredValue = value.Replace(" ", "").Replace("-", "");

			ZQuery orderQuery = new ZQuery { DefaultJoinCondition = JoinCondition.Or };
			orderQuery.AddToFilter_PossiblyCommaSeparated(orderSchemaColumn, comparisonOperator, value);
			orderQuery.AddToFilter_PossiblyCommaSeparated(orderSchemaColumn, comparisonOperator, alteredValue);
			return GetOrderHeaderQuery(orderQuery);
		}

		protected ZQuery GetQuery(SchemaGuidColumn orderOrg1SchemaColumn, SchemaGuidColumn orderOrg2SchemaColumn, ZGuid org1PK, ZGuid org2PK)
		{
			var filter = new ZQuery();
			if (org1PK.IsValid)
			{
				filter.AddToFilter(orderOrg1SchemaColumn, org1PK);
			}

			if (org2PK.IsValid)
			{
				filter.AddToFilter(orderOrg2SchemaColumn, org2PK);
			}

			return filter;
		}

		protected virtual ZQuery GetOrderLineNumberRangeFilterQuery(SchemaColumn schemaColumn, INumericZType value1, INumericZType value2)
		{
			var filter = ModuleNumberRangeFilter.AddToFilters(new ZQuery(), schemaColumn, value1, value2);
			return GetOrderLineQuery(filter);
		}

		protected abstract ZQuery GetConsolQuery(SchemaColumn consolSchemaColumn, SQLComparisonOperator comparisonOperator, IZType value);

		protected abstract ZQuery GetConsolQueryForMasterBill(SchemaColumn consolSchemaColumn, SQLComparisonOperator comparisonOperator, ZString value);

		protected abstract ZQuery GetShipmentQuery(SchemaColumn shipmentSchemaColumn, SQLComparisonOperator comparisonOperator, IZType value);

		#endregion

		#region Lookups

		protected BindToLists BindingLists
		{
			get { return BindToLists.GetCachedLists(Factory); }
		}

		protected CodeDescriptionPairList OrdersAttachedState_List
		{
			get
			{
				if (fOrdersAttachedState_List == null)
				{
					fOrdersAttachedState_List = new CodeDescriptionPairList();
					fOrdersAttachedState_List.AddPair(OrdersConstants.OrdersAttachedState.Both, Res.GetString("Forwarding|OrdersBaseFilter|BothAttachedAndUnattachedOrders", "Both Attached and Unattached Orders"));
					fOrdersAttachedState_List.AddPair(OrdersConstants.OrdersAttachedState.AttachedOnly, Res.GetString("Forwarding|OrdersBaseFilter|AttachedOrdersOnly", "Attached Orders Only"));
					fOrdersAttachedState_List.AddPair(OrdersConstants.OrdersAttachedState.UnattachedOnly, Res.GetString("Forwarding|OrdersBaseFilter|UnattachedOrdersOnly", "Unattached Orders Only"));
				}
				return fOrdersAttachedState_List;
			}
		}

		protected CodeDescriptionPairList GetJD_ContainerMode_List()
		{
			ModuleTextFilter transportModeFilter = (ModuleTextFilter)this["Transport Mode"];
			ZString transportMode = (transportModeFilter.IsActive) ? transportModeFilter.Property : ZString.Empty;

			return OrdersConstants.GetContainerModeList(transportMode);
		}

		CodeDescriptionPairList fOrdersAttachedState_List;

		JobShipmentPreplanningCollection PreAdviceList
		{
			get { return fPreAdviceList ?? (fPreAdviceList = new JobShipmentPreplanningCollection(Factory)); }
		}

		JobShipmentPreplanningCollection fPreAdviceList;

		OrgSupplierPartCollection ProductsCollection
		{
			get { return fProductsCollection ?? (fProductsCollection = new OrgSupplierPartCollection(Factory)); }
		}
		OrgSupplierPartCollection fProductsCollection;

		protected CodeDescriptionPairList GetOrderStatus_List()
		{
			CodeDescriptionPairList result = new OrderStatusLists().GetOrderStatusList();

			if (result.ContainsCode(Constants.OrderStatus.Delivered))
			{
				result.AddPair(UndeliveredOrderStatus, Res.GetString("Forwarding|OrdersBaseFilter|UndeliveredOrders", "Undelivered Orders"));
			}

			result.Insert(0, new CodeDescriptionPair(Core.Constants.OrderStatus.All, Res.GetString("bcc62c87-e85d-421f-a405-d5f85211c759", "All Orders")));

			return result;
		}

		#endregion

		#region ICustomLabelsConfigOrgProvider Members

		OrgHeader ICustomLabelsConfigOrgProvider.ConfigOrg
		{
			get { return Buyer; }
		}

		event EventHandler ICustomLabelsConfigOrgProvider.ConfigOrgChanged
		{
			add { ConfigOrgChanged += value; }
			remove { ConfigOrgChanged -= value; }
		}

		void OnConfigOrgChanged()
		{
			if (ConfigOrgChanged != null)
			{
				ConfigOrgChanged(this, new EventArgs());
			}
		}

		event EventHandler ConfigOrgChanged;

		ModuleGuidsFilter BuyerSupplierFilter
		{
			get { return (ModuleGuidsFilter)this["Buyer / Supplier"]; }
		}

		protected OrgHeader Buyer
		{
			get
			{
				OrgHeader result = null;

				if (moduleFiltersCreated && BuyerSupplierFilter.IsActive && BuyerSupplierFilter.Property1.IsValid)
				{
					ZGuid buyerPK = BuyerSupplierFilter.Property1;
					result = Factory.Load<OrgHeader>(buyerPK);
				}

				return result;
			}
		}

		#endregion
	}
}
