using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Module.Testing
{
	[TestedType(typeof(OrderLineFilterBusinessObject))]
	public class OrderLineFilterBusinessObjectTest : OrdersBaseFilterBusinessObjectTestCase
	{
		public void TestClientAssignedStaffFilter()
		{
			var order1 = Factory.NewWithValidTestData<Order>();
			var orderLine1 = order1.OrderLines.AddNew();
			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			order1.BuyerPK = buyer.PK;
			var buyerAssignment = buyer.StaffAssignments.AddNew();
			buyerAssignment.O8_Role = "CAR";
			buyerAssignment.O8_GS_NKPersonResponsible = "ABC";

			var order2 = Factory.NewWithValidTestData<Order>();
			var orderLine2 = order2.OrderLines.AddNew();
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			order2.SupplierPK = supplier.PK;
			var supplierStaff = Factory.NewWithValidTestData<GlbStaff>();
			supplierStaff.GS_Code = "PAT";
			var supplierAssignment = supplier.StaffAssignments.AddNew();
			supplierAssignment.O8_GS_NKPersonResponsible = supplierStaff.GS_Code;

			var order3 = Factory.NewWithValidTestData<Order>();
			var orderLine3 = order3.OrderLines.AddNew();
			var controllingCustomer = Factory.NewWithValidTestData<OrgHeader>();
			order3.ControllingCustomerDocAddress.E2_OA_Address = controllingCustomer.MainAddress.PK;
			var controllingCustomerAssignment = controllingCustomer.StaffAssignments.AddNew();
			controllingCustomerAssignment.O8_Department = "AIR";
			controllingCustomerAssignment.O8_GS_NKPersonResponsible = "ABC";

			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_Code = "XYZ";
			buyer.CompanyData.OB_GB_ControllingBranch = branch.PK;

			Factory.Save();

			var filter = (OrderClientAssignedStaffModuleFilter)FilterStripBizO["Client Assigned Staff"];
			filter.IsActive = true;

			var collection = GetCollectionLoadedWithFilterApplied();
			AssertContainsExactElementsInAnyOrder(new[] { orderLine1, orderLine2, orderLine3, Line1, Line2 }, collection);

			Action<ZString, ZString, ZString, ZString, ZGuid, IEnumerable<OrderLine>> assertResults = (clientType, staffRole, assignedStaff, department, controllingBranch, results) =>
			{
				filter.ClientType = clientType;
				filter.StaffRole = staffRole;
				filter.AssignedStaff = assignedStaff;
				filter.Department = department;
				filter.ControllingBranch = controllingBranch;

				collection = GetCollectionLoadedWithFilterApplied();
				AssertContainsExactElementsInAnyOrder(results, collection);
			};

			assertResults("BUY", "CAR", ZString.Empty, ZString.Empty, ZGuid.Empty, new[] { orderLine1 });
			assertResults("SUP", ZString.Empty, "PAT", ZString.Empty, ZGuid.Empty, new[] { orderLine2 });
			assertResults("CPY", ZString.Empty, ZString.Empty, "AIR", ZGuid.Empty, new[] { orderLine3 });
			assertResults("BUY", ZString.Empty, ZString.Empty, ZString.Empty, branch.PK, new[] { orderLine1 });
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

			OrderLineFilterBusinessObject filterBizo = new OrderLineFilterBusinessObject();
			filterBizo["Controlling Customer"].IsActive = true;

			((ModuleGuidFilter)filterBizo["Controlling Customer"]).Property = controllingCustomer.PK;
			OrderLineCollection lines = new OrderLineCollection(Factory);
			lines.AdditionalFilter = filterBizo.Filter;
			AssertEquals(1, lines.Count);
			AssertEquals(firstOrderLine.PK, lines[0].PK);

			((ModuleGuidFilter)filterBizo["Controlling Customer"]).Property = org2.PK;
			lines = new OrderLineCollection(Factory);
			lines.AdditionalFilter = filterBizo.Filter;
			AssertEquals(0, lines.Count);
		}

		public void TestOrderLineNumberFilter()
		{
			Order firstOrder = Factory.NewWithValidTestData<Order>();

			OrderLine firstOrderLine = firstOrder.OrderLines.AddNew();
			firstOrderLine.JO_LineNo = 123;

			OrderLine secondOrderLine = firstOrder.OrderLines.AddNew();
			secondOrderLine.JO_LineNo = 918;

			Factory.Save();

			OrderLineFilterBusinessObject filterBizo = new OrderLineFilterBusinessObject();
			filterBizo["Order Line #"].IsActive = true;
			((ModuleNumberFilter)filterBizo["Order Line #"]).Property = "Hello";

			OrderLineCollection lines = new OrderLineCollection(Factory);

			lines.AdditionalFilter = filterBizo.Filter;
			AssertEquals(0, lines.Count);

			((ModuleNumberFilter)filterBizo["Order Line #"]).Property = "123";
			lines.AdditionalFilter = filterBizo.Filter;
			AssertEquals(1, lines.Count);

			((ModuleNumberFilter)filterBizo["Order Line #"]).Property = "999";
			lines.AdditionalFilter = filterBizo.Filter;
			AssertEquals(0, lines.Count);
		}

		public override void TestOrderNumberFilter()
		{
			OrdersBaseFilterBusinessObject filter = FilterStripBizO;
			((ModuleTextFilter)filter["Order #"]).Property = "ABC";
			((ModuleTextFilter)filter["Order #"]).IsActive = true;
			((ModuleTextFilter)filter["Order #"]).SqlComparisonOperator = SQLComparisonOperator.Contains;

			OrderCollection orders = new OrderCollection(Factory);

			Order1[JobOrderHeaderSchema.JD_OrderNumber.Name] = "ABC";
			Order1[JobOrderHeaderSchema.JD_OrderNumberSplit.Name] = new ZByte(1);
			Order2[JobOrderHeaderSchema.JD_OrderNumber.Name] = "123";
			Order2[JobOrderHeaderSchema.JD_OrderNumberSplit.Name] = new ZByte(1);

			OrderLine line11 = Order1.OrderLines.AddNew();
			line11.JO_LineNo = 123;

			OrderLine line21 = Order2.OrderLines.AddNew();
			line21.JO_LineNo = 456;

			Factory.Save();

			OrderLineCollection lines = new OrderLineCollection(Factory);

			lines.AdditionalFilter = filter.Filter;
			AssertCollectionContains(line11, lines);
			AssertCollectionNotContains(line21, lines);

			Order1[JobOrderHeaderSchema.JD_OrderNumber.Name] = "ABC";
			Order1[JobOrderHeaderSchema.JD_OrderNumberSplit.Name] = new ZByte(1);
			Order2[JobOrderHeaderSchema.JD_OrderNumber.Name] = "ABC";
			Order2[JobOrderHeaderSchema.JD_OrderNumberSplit.Name] = new ZByte(2);
			Factory.Save();

			lines.AdditionalFilter = filter.Filter;
			AssertCollectionContains(line11, lines);
			AssertCollectionContains(line21, lines);

			((ModuleTextFilter)filter["Order #"]).Property = "C-1";
			((ModuleTextFilter)filter["Order #"]).IsActive = true;

			lines.AdditionalFilter = filter.Filter;
			AssertCollectionContains(line11, lines);
			AssertCollectionNotContains(line21, lines);

			Order1[JobOrderHeaderSchema.JD_OrderNumber.Name] = "ABC";
			Order1[JobOrderHeaderSchema.JD_OrderNumberSplit.Name] = new ZByte(1);
			Order2[JobOrderHeaderSchema.JD_OrderNumber.Name] = "ABC";
			Order2[JobOrderHeaderSchema.JD_OrderNumberSplit.Name] = new ZByte(12);
			Factory.Save();

			lines.AdditionalFilter = filter.Filter;
			((IActiveBusinessObjectCollection)lines).Refresh();
			AssertCollectionContains(line11, lines);
			AssertCollectionContains(line21, lines);

			((ModuleTextFilter)filter["Order #"]).Property = "ABC-1";
			((ModuleTextFilter)filter["Order #"]).IsActive = true;

			((ModuleTextFilter)filter["Order #"]).SqlComparisonOperator = SQLComparisonOperator.Equal;

			lines.AdditionalFilter = filter.Filter;
			AssertCollectionContains(line11, lines);
			AssertCollectionNotContains(line21, lines);
		}

		public void TestOrderSplitLineNumberFilter()
		{
			Order firstOrder = Factory.NewWithValidTestData<Order>();

			OrderLine firstOrderLine = firstOrder.OrderLines.AddNew();
			firstOrderLine.JO_LineSplitNumber = 456;

			OrderLine secondOrderLine = firstOrder.OrderLines.AddNew();
			secondOrderLine.JO_LineSplitNumber = 987;

			Factory.Save();

			OrderLineFilterBusinessObject filterBizo = new OrderLineFilterBusinessObject();
			filterBizo["Order Split Line #"].IsActive = true;
			((ModuleNumberFilter)filterBizo["Order Split Line #"]).Property = "Hello";

			OrderLineCollection lines = new OrderLineCollection(Factory);

			lines.AdditionalFilter = filterBizo.Filter;
			AssertEquals(0, lines.Count);

			((ModuleNumberFilter)filterBizo["Order Split Line #"]).Property = "456";
			lines.AdditionalFilter = filterBizo.Filter;
			AssertEquals(1, lines.Count);

			((ModuleNumberFilter)filterBizo["Order Split Line #"]).Property = "999";
			lines.AdditionalFilter = filterBizo.Filter;
			AssertEquals(0, lines.Count);
		}

		public void TestOrderLineReferenceFilter()
		{
			var order = Factory.NewWithValidTestData<Order>();

			var orderLine1 = order.OrderLines.AddNew();
			orderLine1.JO_LineReference = "Line #1";

			var orderLine2 = order.OrderLines.AddNew();
			orderLine2.JO_LineReference = "Line #2";

			Factory.Save();

			var filterBizo = new OrderLineFilterBusinessObject();
			filterBizo["Line Reference"].IsActive = true;
			((ModuleTextFilter)filterBizo["Line Reference"]).Property = "Line";
			((ModuleTextFilter)filterBizo["Line Reference"]).SqlComparisonOperator = SQLComparisonOperator.Equal;

			var lines = new OrderLineCollection(Factory);
			lines.AdditionalFilter = filterBizo.Filter;

			AssertEquals(0, lines.Count);

			((ModuleTextFilter)filterBizo["Line Reference"]).Property = "Line #1";
			lines.AdditionalFilter = filterBizo.Filter;

			AssertEquals(1, lines.Count);
			AssertEquals("Line #1", lines.First().JO_LineReference);

			((ModuleTextFilter)filterBizo["Line Reference"]).Property = "Line";
			((ModuleTextFilter)filterBizo["Line Reference"]).SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			lines.AdditionalFilter = filterBizo.Filter;

			AssertEquals(2, lines.Count);
			AssertArrayEqualsByElements(new[] { "Line #1", "Line #2" }, lines.Select(x => x.JO_LineReference.ToString()).ToArray());
		}

		public void TestOrderLineHSCodeFilter()
		{
			var order = Factory.NewWithValidTestData<Order>();

			var orderLine1 = order.OrderLines.AddNew();
			orderLine1.JO_HSCode = "HS Code #1";

			var orderLine2 = order.OrderLines.AddNew();
			orderLine2.JO_HSCode = "HS Code #2";

			Factory.Save();

			var filterName = "H.S. Code";
			var filterBizo = new OrderLineFilterBusinessObject();
			filterBizo[filterName].IsActive = true;
			((ModuleTextFilter)filterBizo[filterName]).Property = "XXXX";
			((ModuleTextFilter)filterBizo[filterName]).SqlComparisonOperator = SQLComparisonOperator.Equal;

			var lines = new OrderLineCollection(Factory);
			lines.AdditionalFilter = filterBizo.Filter;

			AssertEquals(0, lines.Count);

			((ModuleTextFilter)filterBizo[filterName]).Property = "HS Code #1";
			lines.AdditionalFilter = filterBizo.Filter;

			AssertEquals(1, lines.Count);
			AssertEquals("HS Code #1", lines.First().JO_HSCode);

			((ModuleTextFilter)filterBizo[filterName]).Property = "HS Code";
			((ModuleTextFilter)filterBizo[filterName]).SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			lines.AdditionalFilter = filterBizo.Filter;

			AssertEquals(2, lines.Count);
			AssertArrayEqualsByElements(new[] { "HS Code #1", "HS Code #2" }, lines.Select(x => x.JO_HSCode.ToString()).ToArray());
		}

		#region Test Manufacturer Filter

		public void TestManufacturerFallsBackOnlyWhenEmptyOnOrderLine()
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

			var docAddress3 = (JobDocAddress)docAddress1.Clone();

			Order1.DocAddresses.Add(docAddress1);
			Order2.DocAddresses.Add(docAddress3);
			Line2.DocAddresses.Add(docAddress2);

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

		#region Workflow Custom Fields Filters

		protected override bool ShouldDisplayWorkflowFilters => true;

		protected override ZString WorkflowDescriptorCode => WorkflowDescriptors.OrderLineWorkflowDescriptorCode;

		#endregion

		#region Origin/Destination filter tests

		public void TestOriginDestinationFilter()
		{
			OrderLine myline = GetOrderForOriginDestFilterTests();

			ModuleLocationFilter orderFilter = (ModuleLocationFilter)FilterStripBizO["Planned Origin / Destination"];
			orderFilter.IsActive = true;

			orderFilter.Property1 = "GBLHR";
			orderFilter.Property2 = "AUSYD";
			OrderLineCollection results = new OrderLineCollection(Factory, FilterStripBizO.Filter);
			AssertCollectionContains("should be included", myline, results);

			orderFilter.Property1 = "GB";
			orderFilter.Property2 = "AU";
			results = new OrderLineCollection(Factory, FilterStripBizO.Filter);
			AssertCollectionContains("should be included", myline, results);

			orderFilter.Property1 = "NZAUK";
			orderFilter.Property2 = "AUSYD";
			results = new OrderLineCollection(Factory, FilterStripBizO.Filter);
			AssertCollectionNotContains("should NOT be included", myline, results);

			orderFilter.Property1 = "NZ";
			orderFilter.Property2 = "AU";
			results = new OrderLineCollection(Factory, FilterStripBizO.Filter);
			AssertCollectionNotContains("should NOT be included", myline, results);
		}

		public void TestOriginDestinationFilter_OriginOnly()
		{
			OrderLine myline = GetOrderForOriginDestFilterTests();

			ModuleLocationFilter orderFilter = (ModuleLocationFilter)FilterStripBizO["Planned Origin / Destination"];
			orderFilter.IsActive = true;

			orderFilter.Property1 = "GBLHR";
			OrderLineCollection results = new OrderLineCollection(Factory, FilterStripBizO.Filter);
			AssertCollectionContains("GBLHR should be included", myline, results);

			orderFilter.Property1 = "GB";
			results = new OrderLineCollection(Factory, FilterStripBizO.Filter);
			AssertCollectionContains("GB should be included", myline, results);

			orderFilter.Property1 = "NZAUK";
			results = new OrderLineCollection(Factory, FilterStripBizO.Filter);
			AssertCollectionNotContains("NZAUK should NOT be included", myline, results);

			orderFilter.Property1 = "NZ";
			results = new OrderLineCollection(Factory, FilterStripBizO.Filter);
			AssertCollectionNotContains("NZ should NOT be included", myline, results);
		}

		public void TestOriginDestinationFilter_DestinationOnly()
		{
			OrderLine myline = GetOrderForOriginDestFilterTests();

			ModuleLocationFilter orderFilter = (ModuleLocationFilter)FilterStripBizO["Planned Origin / Destination"];
			orderFilter.IsActive = true;

			orderFilter.Property2 = "AUSYD";
			OrderLineCollection results = new OrderLineCollection(Factory, FilterStripBizO.Filter);
			AssertCollectionContains("AUSYD should be included", myline, results);

			orderFilter.Property2 = "AU";
			results = new OrderLineCollection(Factory, FilterStripBizO.Filter);
			AssertCollectionContains("AU should be included", myline, results);

			orderFilter.Property2 = "NZAUK";
			results = new OrderLineCollection(Factory, FilterStripBizO.Filter);
			AssertCollectionNotContains("NZAUK should NOT be included", myline, results);

			orderFilter.Property2 = "NZ";
			results = new OrderLineCollection(Factory, FilterStripBizO.Filter);
			AssertCollectionNotContains("NZ should NOT be included", myline, results);
		}

		OrderLine GetOrderForOriginDestFilterTests()
		{
			Order myorder = Factory.NewWithValidTestData<Order>();
			myorder.JD_RL_NKGoodsAvailableAt = "GBLHR";
			myorder.JD_RL_NKGoodsDeliveredTo = "AUSYD";

			OrderLine line1 = myorder.OrderLines.AddNew();
			line1.JO_LineNo = 123;

			Factory.Save();
			return line1;
		}

		#endregion

		#region Test Shipment Window Dates

		public void TestShipmentWindowDates()
		{
			AssertOrderLineDateFilter(OrderLineFilterBusinessObject.Filters.ShipmentWindowStart, JobOrderLineSchema.JO_ShipmentWindowStart);
			AssertOrderLineDateFilter(OrderLineFilterBusinessObject.Filters.ShipmentWindowEnd, JobOrderLineSchema.JO_ShipmentWindowEnd);
		}

		#endregion

		protected override List<Tuple<string, string>> GetFiltersExcludedFromSubgroupCheckForCommonTables()
		{
			var result = base.GetFiltersExcludedFromSubgroupCheckForCommonTables();

			result.Add(TableFilter("JobOrderHeader", "Order #"));
			result.Add(TableFilter("JobOrderHeader", "Container #"));
			result.Add(TableFilter("JobOrderHeader", "Master Bill"));
			result.Add(TableFilter("JobOrderHeader", "Shipment #"));
			result.Add(TableFilter("JobOrderHeader", "Buyer / Supplier"));
			result.Add(TableFilter("JobOrderHeader", "Planned Send / Receive Agents"));
			result.Add(TableFilter("JobOrderHeader", "Planned Load / Discharge"));
			result.Add(TableFilter("JobOrderHeader", "OrderHeader.CustomAttrib1"));
			result.Add(TableFilter("JobOrderHeader", "OrderHeader.CustomAttrib2"));
			result.Add(TableFilter("JobOrderHeader", "OrderHeader.CustomAttrib3"));
			result.Add(TableFilter("JobOrderHeader", "OrderHeader.CustomAttrib4"));
			result.Add(TableFilter("JobOrderHeader", "OrderHeader.CustomAttrib5"));
			result.Add(TableFilter("JobOrderHeader", "OrderHeader.CustomFlag1"));
			result.Add(TableFilter("JobOrderHeader", "OrderHeader.CustomFlag2"));
			result.Add(TableFilter("JobOrderHeader", "OrderHeader.CustomFlag3"));
			result.Add(TableFilter("JobOrderHeader", "OrderHeader.CustomFlag4"));
			result.Add(TableFilter("JobOrderHeader", "OrderHeader.CustomFlag5"));
			result.Add(TableFilter("JobOrderHeader", "OrderHeader.CustomDecimal1"));
			result.Add(TableFilter("JobOrderHeader", "OrderHeader.CustomDecimal2"));
			result.Add(TableFilter("JobOrderHeader", "OrderHeader.CustomDecimal3"));
			result.Add(TableFilter("JobOrderHeader", "OrderHeader.CustomDecimal4"));
			result.Add(TableFilter("JobOrderHeader", "OrderHeader.CustomDecimal5"));
			result.Add(TableFilter("JobOrderHeader", "OrderHeader.CustomContact1"));
			result.Add(TableFilter("JobOrderHeader", "OrderHeader.CustomContact2"));
			result.Add(TableFilter("JobOrderHeader", "OrderHeader.GoodsOrigin"));
			result.Add(TableFilter("JobOrderHeader", "OrderHeader.GoodsDestination"));
			result.Add(TableFilter("JobOrderHeader", "Any Text Attribute"));

			return result;
		}

		public void TestCRMSecurityFilters()
		{
			CRMSecurityProviderTest<OrderLine>.AssertFilterStrip(GetNewFilterStripBusinessObject, Env.Security.OrderLineTrackingCRMSecurity);
		}

		protected override OrdersBaseFilterBusinessObject GetNewOrdersBaseFilterBusinessObject()
		{
			return new OrderLineFilterBusinessObject();
		}

		protected override IActiveBusinessObjectCollection GetNewCollection()
		{
			return new OrderLineCollection(Factory);
		}

		protected override BusinessObject ExpectSearchResultsToContain
		{
			get { return Line1; }
		}

		protected override BusinessObject ExpectSearchResultsToNotContain
		{
			get { return Line2; }
		}
	}
}
