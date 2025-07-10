using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Module.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Module.Testing
{
	[TestedType(typeof(OrdersFilterBusinessObject))]
	public class OrdersFilterBusinessObjectTest : OrdersBaseFilterBusinessObjectTestCase
	{
		public override void TestPlannedNotifyPartyFilter()
		{
			#region Setup

			Order1.NotifyPartyDocAddress.OrganisationPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			Order1.NotifyParty2DocAddress.OrganisationPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			Order1.NotifyParty3DocAddress.OrganisationPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			Order2.NotifyPartyDocAddress.OrganisationPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			Order2.NotifyParty2DocAddress.OrganisationPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			Order2.NotifyParty3DocAddress.OrganisationPK = Factory.NewWithValidTestData<OrgHeader>().PK;

			Factory.Save();

			#endregion

			var filter = (ModuleGuidFilter)FilterStripBizO["Planned Notify Party"];
			filter.IsActive = true;
			filter.Property = Order1.NotifyPartyDocAddress.OrganisationPK;

			var filterResults = GetCollectionLoadedWithFilterApplied();
			AssertCollectionContains("Notify Party should match 1st order", ExpectSearchResultsToContain, filterResults);
			AssertCollectionNotContains("Notify Party should not match 2nd order", ExpectSearchResultsToNotContain, filterResults);

			filter.Clear();
			filter = (ModuleGuidFilter)FilterStripBizO["Planned Notify Party 2"];
			filter.IsActive = true;
			filter.Property = Order1.NotifyParty2DocAddress.OrganisationPK;

			filterResults = GetCollectionLoadedWithFilterApplied();
			AssertCollectionContains("Notify Party2 should match 1st order", ExpectSearchResultsToContain, filterResults);
			AssertCollectionNotContains("Notify Party2 should not match 2nd order", ExpectSearchResultsToNotContain, filterResults);

			filter.Clear();
			filter = (ModuleGuidFilter)FilterStripBizO["Planned Notify Party 3"];
			filter.IsActive = true;
			filter.Property = Order1.NotifyParty3DocAddress.OrganisationPK;

			filterResults = GetCollectionLoadedWithFilterApplied();
			AssertCollectionContains("Notify Party3 should match 1st order", ExpectSearchResultsToContain, filterResults);
			AssertCollectionNotContains("Notify Party3 should not match 2nd order", ExpectSearchResultsToNotContain, filterResults);
		}

		public override void TestActualNotifyPartyFilter_ShipmentLinked()
		{
			#region Setup

			var shipment1 = Factory.New<ForwardingShipment>();
			shipment1.NotifyPartyDocumentaryAddress.OrganisationPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			shipment1.NotifyParty2DocumentaryAddress.OrganisationPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			shipment1.NotifyParty3DocumentaryAddress.OrganisationPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			Order1.NotifyPartyDocAddress.OrganisationPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			Order1.NotifyParty2DocAddress.OrganisationPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			Order1.NotifyParty3DocAddress.OrganisationPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			Order1.JD_JS = shipment1.PK;

			var shipment2 = Factory.New<ForwardingShipment>();
			shipment2.NotifyPartyDocumentaryAddress.OrganisationPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			shipment2.NotifyParty2DocumentaryAddress.OrganisationPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			shipment2.NotifyParty3DocumentaryAddress.OrganisationPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			Order2.NotifyPartyDocAddress.OrganisationPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			Order2.NotifyParty2DocAddress.OrganisationPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			Order2.NotifyParty3DocAddress.OrganisationPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			Order2.JD_JS = shipment2.PK;

			Factory.Save();

			#endregion

			var filter = (ModuleGuidFilter)FilterStripBizO["Actual Notify Party"];
			filter.IsActive = true;
			filter.Property = shipment1.NotifyPartyDocumentaryAddress.OrganisationPK;

			IBusinessObjectCollection filterResults = GetCollectionLoadedWithFilterApplied();
			AssertCollectionContains("Notify Party should match 1st order", ExpectSearchResultsToContain, filterResults);
			AssertCollectionNotContains("Notify Party should not match 2nd order", ExpectSearchResultsToNotContain, filterResults);

			filter.Clear();
			filter = (ModuleGuidFilter)FilterStripBizO["Actual Notify Party 2"];
			filter.IsActive = true;
			filter.Property = shipment1.NotifyParty2DocumentaryAddress.OrganisationPK;

			filterResults = GetCollectionLoadedWithFilterApplied();
			AssertCollectionContains("Notify Party 2 should match 1st order", ExpectSearchResultsToContain, filterResults);
			AssertCollectionNotContains("Notify Party2 should not match 2nd order", ExpectSearchResultsToNotContain, filterResults);

			filter.Clear();
			filter = (ModuleGuidFilter)FilterStripBizO["Actual Notify Party 3"];
			filter.IsActive = true;
			filter.Property = shipment1.NotifyParty3DocumentaryAddress.OrganisationPK;

			filterResults = GetCollectionLoadedWithFilterApplied();
			AssertCollectionContains("Notify Party 3 should match 1st order", ExpectSearchResultsToContain, filterResults);
			AssertCollectionNotContains("Notify Party3 should not match 2nd order", ExpectSearchResultsToNotContain, filterResults);

			filter.Clear();
			filter = (ModuleGuidFilter)FilterStripBizO["Actual Notify Party"];
			filter.IsActive = true;
			filter.Property = shipment2.NotifyPartyDocumentaryAddress.OrganisationPK;

			filterResults = GetCollectionLoadedWithFilterApplied();
			AssertCollectionNotContains("Notify Party should not match 1st order", ExpectSearchResultsToContain, filterResults);
			AssertCollectionContains("Notify Party should match 2nd order", ExpectSearchResultsToNotContain, filterResults);

			filter.Clear();
			filter = (ModuleGuidFilter)FilterStripBizO["Actual Notify Party 2"];
			filter.IsActive = true;
			filter.Property = shipment2.NotifyParty2DocumentaryAddress.OrganisationPK;

			filterResults = GetCollectionLoadedWithFilterApplied();
			AssertCollectionNotContains("Notify Party 2 should not match 1st order", ExpectSearchResultsToContain, filterResults);
			AssertCollectionContains("Notify Party2 should match 2nd order", ExpectSearchResultsToNotContain, filterResults);

			filter.Clear();
			filter = (ModuleGuidFilter)FilterStripBizO["Actual Notify Party 3"];
			filter.IsActive = true;
			filter.Property = shipment2.NotifyParty3DocumentaryAddress.OrganisationPK;

			filterResults = GetCollectionLoadedWithFilterApplied();
			AssertCollectionNotContains("Notify Party 3 should not match 1st order", ExpectSearchResultsToContain, filterResults);
			AssertCollectionContains("Notify Party3 should match 2nd order", ExpectSearchResultsToNotContain, filterResults);
		}

		public void TestClientAssignedStaffFilter()
		{
			var order1 = Factory.NewWithValidTestData<Order>();
			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			order1.BuyerPK = buyer.PK;
			var buyerAssignment = buyer.StaffAssignments.AddNew();
			buyerAssignment.O8_Role = "CAR";
			buyerAssignment.O8_GS_NKPersonResponsible = "ABC";

			var order2 = Factory.NewWithValidTestData<Order>();
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			order2.SupplierPK = supplier.PK;
			var supplierStaff = Factory.NewWithValidTestData<GlbStaff>();
			supplierStaff.GS_Code = "PAT";
			var supplierAssignment = supplier.StaffAssignments.AddNew();
			supplierAssignment.O8_GS_NKPersonResponsible = supplierStaff.GS_Code;

			var order3 = Factory.NewWithValidTestData<Order>();
			var controllingCustomer = Factory.NewWithValidTestData<OrgHeader>();
			order3.ControllingCustomerDocAddress.E2_OA_Address = controllingCustomer.MainAddress.PK;
			var controllingPartyAssignment = controllingCustomer.StaffAssignments.AddNew();
			controllingPartyAssignment.O8_Department = "AIR";
			controllingPartyAssignment.O8_GS_NKPersonResponsible = "ABC";

			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_Code = "XYZ";
			buyer.CompanyData.OB_GB_ControllingBranch = branch.PK;

			Factory.Save();

			var filter = (OrderClientAssignedStaffModuleFilter)FilterStripBizO["Client Assigned Staff"];
			filter.IsActive = true;

			var collection = GetCollectionLoadedWithFilterApplied();
			AssertContainsExactElementsInAnyOrder(new[] { order1, order2, order3, Order1, Order2 }, collection);

			Action<ZString, ZString, ZString, ZString, ZGuid, IEnumerable<Order>> assertResults = (clientType, staffRole, assignedStaff, department, controllingBranch, results) =>
			{
				filter.ClientType = clientType;
				filter.StaffRole = staffRole;
				filter.AssignedStaff = assignedStaff;
				filter.Department = department;
				filter.ControllingBranch = controllingBranch;

				collection = GetCollectionLoadedWithFilterApplied();
				AssertContainsExactElementsInAnyOrder(results, collection);
			};

			assertResults("BUY", "CAR", ZString.Empty, ZString.Empty, ZGuid.Empty, new[] { order1 });
			assertResults("SUP", ZString.Empty, "PAT", ZString.Empty, ZGuid.Empty, new[] { order2 });
			assertResults("CPY", ZString.Empty, ZString.Empty, "AIR", ZGuid.Empty, new[] { order3 });
			assertResults("BUY", ZString.Empty, ZString.Empty, ZString.Empty, branch.PK, new[] { order1 });
		}

		public void TestControllingCustomerFilter()
		{
			OrgHeader org = Factory.LoadTop1<OrgHeader>(new ZQuery());
			OrgHeader org2 = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, org.PK));

			var controllingCustomer = Factory.NewWithValidTestData<OrgHeader>();
			controllingCustomer.OH_Code = "CTL_CUSTOMER";
			OrgSupplierBuyerLink supplierBuyerLink = org.SupplierLinks.AddNew(org2);
			supplierBuyerLink.OrgSupBuyLinkTrnModes[0].PF_TransportMode = "ALL";
			supplierBuyerLink.OrgSupBuyLinkTrnModes[0].PF_OH_ControllingCustomer = controllingCustomer.PK;

			Order firstOrder = Factory.NewWithValidTestData<Order>();
			firstOrder.BuyerPK = org.PK;

			OrderLine firstOrderLine = firstOrder.OrderLines.AddNew();
			firstOrderLine.JO_LineNo = 123;

			Order order2 = Factory.NewWithValidTestData<Order>();
			OrderLine orderLine2 = order2.OrderLines.AddNew();
			orderLine2.JO_LineNo = 555;

			Factory.Save();

			OrdersFilterBusinessObject filterBizo = new OrdersFilterBusinessObject();
			filterBizo["Controlling Customer"].IsActive = true;

			((ModuleGuidFilter)filterBizo["Controlling Customer"]).Property = controllingCustomer.PK;
			OrderCollection orders = new OrderCollection(Factory);
			orders.AdditionalFilter = filterBizo.Filter;
			AssertEquals(1, orders.Count);
			AssertEquals(firstOrder.PK, orders[0].PK);

			((ModuleGuidFilter)filterBizo["Controlling Customer"]).Property = org2.PK;
			orders = new OrderCollection(Factory);
			orders.AdditionalFilter = filterBizo.Filter;
			AssertEquals(0, orders.Count);
		}

		public void TestGetHouseBillQuery_CommaSeparated()
		{
			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.JS_HouseBill = "HouseBill1";
			var order1 = Factory.NewWithValidTestData<Order>();
			order1.JD_JS = shipment1.PK;

			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment2.JS_HouseBill = "HouseBill2";
			var order2 = Factory.NewWithValidTestData<Order>();
			order2.JD_JS = shipment2.PK;

			var shipment3 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment3.JS_HouseBill = "HouseBill3";
			var order3 = Factory.NewWithValidTestData<Order>();
			order3.JD_JS = shipment3.PK;

			var order4 = Factory.NewWithValidTestData<Order>();
			order4.JD_Waybill = "HouseBill1";

			var order5 = Factory.NewWithValidTestData<Order>();
			order5.JD_Waybill = "HouseBill2";

			var order6 = Factory.NewWithValidTestData<Order>();
			order6.JD_Waybill = "HouseBill3";

			Factory.Save();

			var filter = FilterStripBizO;
			((ModuleTextFilter)filter["House Bill"]).Property = "HouseBill1,HouseBill2";
			((ModuleTextFilter)filter["House Bill"]).IsActive = true;

			var orders = new OrderCollection(Factory);
			orders.AdditionalFilter = filter.Filter;
			AssertContainsExactElementsInAnyOrder(new BusinessObject[] { order1, order2, order4, order5 }, orders);
		}

		#region TestManufacturerFilter

		public void TestManufacturerFallsbackRegardlessOfValueOnOrderLine()
		{
			#region Setup

			var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader1.OH_FullName = "Valid Company 1";
			var orgAddress1 = orgHeader1.Addresses[0];
			var docAddress1 = Factory.NewWithValidTestData<JobDocAddress>();
			docAddress1.E2_AddressType = "MAN";
			docAddress1.E2_OA_Address = orgAddress1.PK;

			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader2.OH_FullName = "Invalid Company 2";
			var orgAddress2 = orgHeader2.Addresses[0];
			var docAddress2 = Factory.NewWithValidTestData<JobDocAddress>();
			docAddress2.E2_AddressType = "MAN";
			docAddress2.E2_OA_Address = orgAddress2.PK;

			Line1.DocAddresses.Add(docAddress2);
			Order1.DocAddresses.Add(docAddress1);

			Factory.Save();

			#endregion

			var filter = FilterStripBizO;
			((ModuleTextFilter)filter["Manufacturer Company Name"]).Property = "Valid Company 1";
			((ModuleTextFilter)filter["Manufacturer Company Name"]).IsActive = true;

			var filterResults = GetCollectionLoadedWithFilterApplied();

			AssertCollectionContains("Manufacturer Name should match 1st order.", ExpectSearchResultsToContain, filterResults);
			AssertCollectionNotContains("Manufacturer Name should not match 2nd order.", ExpectSearchResultsToNotContain, filterResults);

			((ModuleTextFilter)filter["Manufacturer Company Name"]).IsActive = false;
			((ModuleGuidFilter)filter["Manufacturer"]).Property = orgHeader1.PK;
			((ModuleGuidFilter)filter["Manufacturer"]).IsActive = true;

			filterResults = GetCollectionLoadedWithFilterApplied();

			AssertCollectionContains("Manufacturer should match 1st order.", ExpectSearchResultsToContain, filterResults);
			AssertCollectionNotContains("Manufacturer should not match 2nd order.", ExpectSearchResultsToNotContain, filterResults);
		}

		#endregion

		#region Test Origin/Destination filter

		Order GetOrderForOriginDestFilterTests()
		{
			Order myorder = Factory.NewWithValidTestData<Order>();
			myorder.JD_RL_NKGoodsAvailableAt = "GBLHR";
			myorder.JD_RL_NKGoodsDeliveredTo = "AUSYD";
			Factory.Save();
			return myorder;
		}

		public void TestOriginDestinationFilter()
		{
			Order myorder = GetOrderForOriginDestFilterTests();

			ModuleLocationFilter orderFilter = (ModuleLocationFilter)FilterStripBizO["Planned Origin / Destination"];
			orderFilter.IsActive = true;

			orderFilter.Property1 = "GBLHR";
			orderFilter.Property2 = "AUSYD";
			OrderCollection results = new OrderCollection(Factory, FilterStripBizO.Filter);
			AssertNotNull("filtered - should be included", results.FindByPK(myorder.PK));

			orderFilter.Property1 = "GB";
			orderFilter.Property2 = "AU";
			results = new OrderCollection(Factory, FilterStripBizO.Filter);
			AssertNotNull("filtered - should be included", results.FindByPK(myorder.PK));

			orderFilter.Property1 = "USJFK";
			orderFilter.Property2 = "NZAUK";
			results = new OrderCollection(Factory, FilterStripBizO.Filter);
			AssertNull("filtered - should NOT be included", results.FindByPK(myorder.PK));

			orderFilter.Property1 = "US";
			orderFilter.Property2 = "GB";
			results = new OrderCollection(Factory, FilterStripBizO.Filter);
			AssertNull("filtered - should NOT be included", results.FindByPK(myorder.PK));
		}

		public void TestOriginDestinationFilter_OriginOnly()
		{
			Order myorder = GetOrderForOriginDestFilterTests();

			ModuleLocationFilter orderFilter = (ModuleLocationFilter)FilterStripBizO["Planned Origin / Destination"];
			orderFilter.IsActive = true;

			orderFilter.Property1 = "GBLHR";
			OrderCollection results = new OrderCollection(Factory, FilterStripBizO.Filter);
			AssertNotNull("filtered - should be included", results.FindByPK(myorder.PK));

			orderFilter.Property1 = "GB";
			results = new OrderCollection(Factory, FilterStripBizO.Filter);
			AssertNotNull("filtered - should be included", results.FindByPK(myorder.PK));

			orderFilter.Property1 = "NZAUK";
			results = new OrderCollection(Factory, FilterStripBizO.Filter);
			AssertNull("filtered - should NOT be included", results.FindByPK(myorder.PK));

			orderFilter.Property1 = "NZ";
			results = new OrderCollection(Factory, FilterStripBizO.Filter);
			AssertNull("filtered - should NOT be included", results.FindByPK(myorder.PK));
		}

		public void TestOriginDestinationFilter_DestinationOnly()
		{
			Order myorder = GetOrderForOriginDestFilterTests();

			ModuleLocationFilter orderFilter = (ModuleLocationFilter)FilterStripBizO["Planned Origin / Destination"];
			orderFilter.IsActive = true;

			orderFilter.Property2 = "AUSYD";
			OrderCollection results = new OrderCollection(Factory, FilterStripBizO.Filter);
			AssertNotNull("filtered - should be included", results.FindByPK(myorder.PK));

			orderFilter.Property2 = "AU";
			results = new OrderCollection(Factory, FilterStripBizO.Filter);
			AssertNotNull("filtered - should be included", results.FindByPK(myorder.PK));

			orderFilter.Property2 = "GBLHR";
			results = new OrderCollection(Factory, FilterStripBizO.Filter);
			AssertNull("filtered - should NOT be included", results.FindByPK(myorder.PK));

			orderFilter.Property2 = "GB";
			results = new OrderCollection(Factory, FilterStripBizO.Filter);
			AssertNull("filtered - should NOT be included", results.FindByPK(myorder.PK));
		}

		#endregion

		#region Workflow Custom Fields Filters

		protected override bool ShouldDisplayWorkflowFilters => true;

		#region TestWorkflowFiltersArentAddedTwice

		public void TestWorkflowFiltersArentAddedTwice()
		{
			var template = Factory.New<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptors.OrderWorkflowDescriptorCode;
			template.P0_IsActive = true;
			template.P0_Name = "WFTesting";

			var customColumn = Factory.New<GenCustomColumnDefinition>();
			customColumn.XC_ParentID = template.PK;
			customColumn.XC_ParentTableCode = "P0";
			customColumn.XC_Type = "INT";
			customColumn.XC_Name = "WFCustomField";

			Factory.Save();

			var filterBizo = GetNewFilterStripBusinessObject();
			filterBizo.LoadModuleFilters();

			AssertNull("We shouldn't be duplicating this custom field", filterBizo["WFCustomField (WF)"]);
		}

		#endregion

		#endregion

		#region CRM Security

		public void TestCRMSecurityFilters()
		{
			CRMSecurityProviderTest<Order>.AssertFilterStrip(GetNewFilterStripBusinessObject, Env.Security.OrderTrackingCRMSecurity);
		}

		#endregion

		#region Test Shipment Window Dates

		public void TestShipmentWindowDates()
		{
			AssertOrderDateFilter(OrdersConstants.DateFilterTypes.ShipmentWindowStart, JobOrderHeaderSchema.JD_ShipmentWindowStart);
			AssertOrderDateFilter(OrdersConstants.DateFilterTypes.ShipmentWindowEnd, JobOrderHeaderSchema.JD_ShipmentWindowEnd);
		}

		#endregion

		public void TestIsReleased()
		{
			var releasedOrder = Factory.NewWithValidTestData<Order>();
			releasedOrder.JD_IsReleased = true;

			var unreleasedOrder = Factory.NewWithValidTestData<Order>();
			unreleasedOrder.JD_IsReleased = false;

			Factory.Save();

			var filter = (ModuleFlagsFilter)FilterStripBizO["Is Released"];
			filter.Property0 = true;
			filter.IsActive = true;

			var orders = Factory.Load<Order>(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new BusinessObject[] { releasedOrder }, orders);

			filter.Property0 = false;

			orders = Factory.Load<Order>(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new BusinessObject[] { Order1, Order2, unreleasedOrder }, orders);
		}

		protected override List<Tuple<string, string>> GetFiltersExcludedFromSubgroupCheckForCommonTables()
		{
			var result = base.GetFiltersExcludedFromSubgroupCheckForCommonTables();

			result.Add(TableFilter("JobShipment", "Container #"));

			result.Add(TableFilter("JobOrderLine", "Container #"));
			result.Add(TableFilter("JobOrderLine", "Order Line - Quantity Invoiced"));
			result.Add(TableFilter("JobOrderLine", "Order Line - Quantity Received"));
			result.Add(TableFilter("JobOrderLine", "Part Attribute 1"));
			result.Add(TableFilter("JobOrderLine", "Part Attribute 2"));
			result.Add(TableFilter("JobOrderLine", "Part Attribute 3"));
			result.Add(TableFilter("JobOrderLine", "OrderLine.CustomAttrib1"));
			result.Add(TableFilter("JobOrderLine", "OrderLine.CustomAttrib2"));
			result.Add(TableFilter("JobOrderLine", "OrderLine.CustomAttrib3"));
			result.Add(TableFilter("JobOrderLine", "OrderLine.CustomAttrib4"));
			result.Add(TableFilter("JobOrderLine", "OrderLine.CustomAttrib5"));
			result.Add(TableFilter("JobOrderLine", "OrderLine.CustomAttrib6"));
			result.Add(TableFilter("JobOrderLine", "OrderLine.CustomText1"));
			result.Add(TableFilter("JobOrderLine", "OrderLine.CustomFlag1"));
			result.Add(TableFilter("JobOrderLine", "OrderLine.CustomFlag2"));
			result.Add(TableFilter("JobOrderLine", "OrderLine.CustomFlag3"));
			result.Add(TableFilter("JobOrderLine", "OrderLine.CustomFlag4"));
			result.Add(TableFilter("JobOrderLine", "OrderLine.CustomFlag5"));
			result.Add(TableFilter("JobOrderLine", "OrderLine.CustomDecimal1"));
			result.Add(TableFilter("JobOrderLine", "OrderLine.CustomDecimal2"));
			result.Add(TableFilter("JobOrderLine", "OrderLine.CustomDecimal3"));
			result.Add(TableFilter("JobOrderLine", "OrderLine.CustomDecimal4"));
			result.Add(TableFilter("JobOrderLine", "OrderLine.CustomDecimal5"));
			result.Add(TableFilter("JobOrderLine", "Any Text Attribute"));

			result.Add(TableFilter("ProcessTasks", "Milestone Completed"));
			result.Add(TableFilter("ProcessTasks", "Any Open Task Assigned To"));
			result.Add(TableFilter("ProcessTasks", "Next Task Assigned To"));

			return result;
		}

		protected override OrdersBaseFilterBusinessObject GetNewOrdersBaseFilterBusinessObject()
		{
			return new OrdersFilterBusinessObject();
		}

		protected override IActiveBusinessObjectCollection GetNewCollection()
		{
			return new OrderCollection(Factory);
		}

		protected override BusinessObject ExpectSearchResultsToContain
		{
			get { return Order1; }
		}

		protected override BusinessObject ExpectSearchResultsToNotContain
		{
			get { return Order2; }
		}
	}
}
