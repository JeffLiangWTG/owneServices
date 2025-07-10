using System;
using System.Collections;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Tracking.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Tracking.Business
{
	[TestedType(typeof(OrdersFilterBusinessObject_Old_ForWeb))]
	[HttpContextEnabledTest]
	class OrderFilterBusinessObject_Old_ForWebTest : FilterBusinessObjectTestCase
	{
		internal TestHelper Helper;
		protected override void SetUp()
		{
			base.SetUp();
			Helper = new TestHelper(Factory);
			AssertNotNull(Helper.TestSiteUser);
		}

		public void TestFilterMatchesEvenWhenOrderHasNoLines()
		{
			FilterBO.BuyerPK = Order.BuyerPK;
			OrderCollection collection = new OrderCollection(Factory, FilterBO.Filter);

			Factory.Save();
			AssertEquals("Matches filter without any order lines", 1, collection.Count);
		}

		public void TestLineStatusFilter()
		{
			OrderLine line = Order.OrderLines.AddNew();
			FilterBO.BuyerPK = ZGuid.Empty;
			line.JO_Partno = "partno";
			line.JO_LineStatus = "sta";
			Factory.Save();

			FilterBO.JD_JO_LineStatus = "xxx";
			OrderCollection collection = new OrderCollection(NewFactory(), FilterBO.Filter);
			AssertEquals("No match", 0, collection.Count);

			FilterBO.JD_JO_LineStatus = "sta";
			collection = new OrderCollection(NewFactory(), FilterBO.Filter);
			AssertEquals("1 match", 1, collection.Count);
		}

		public void TestBuyerSupplier()
		{
			AssertEquals("Should be filtering by All", OrdersConstants.OrgFilterTypes.All, FilterBO.JD_OrgFilterType);

			FilterBO.JD_OH_Org1 = Buyer.PK;
			FilterBO.JD_OH_Org2 = Supplier.PK;

			AssertEquals("Empty buyer - not filtering by BuyerSupplier", ZGuid.Empty, FilterBO.BuyerPK);
			AssertEquals("Empty supplier - not filtering by BuyerSupplier", ZGuid.Empty, FilterBO.SupplierPK);

			FilterBO.JD_OrgFilterType = OrdersConstants.OrgFilterTypes.BuyerSupplier;

			AssertEquals("Populated buyer", Buyer.PK, FilterBO.BuyerPK);
			AssertEquals("Populated supplier", Supplier.PK, FilterBO.SupplierPK);
		}

		#region TestIncludeInactiveFilter

		public void TestIncludeInactiveFilter()
		{
			Order order1 = GetNewOrder();
			Order order2 = GetNewOrder();
			Order order3 = GetNewOrder();

			order1.JD_IsCancelled = ZBool.False;
			order2.JD_IsCancelled = ZBool.False;
			order3.JD_IsCancelled = ZBool.False;

			OrderCollection collection = new OrderCollection(Factory);

			Factory.Save();
			collection.AdditionalFilter = FilterBO.Filter;

			AssertContainsExactElementsInAnyOrder(new ZGuid[] { order1.PK, order2.PK, order3.PK }, collection.Select(order => order.PK));

			order1.JD_IsCancelled = ZBool.True;
			order3.JD_IsCancelled = ZBool.True;

			Factory.Save();
			collection.AdditionalFilter = FilterBO.Filter;

			AssertContainsExactElementsInAnyOrder(new ZGuid[] { order2.PK }, collection.Select(order => order.PK));

			FilterBO.JD_IsCancelled = ZBool.True;
			collection.AdditionalFilter = FilterBO.Filter;

			AssertContainsExactElementsInAnyOrder(new ZGuid[] { order1.PK, order2.PK, order3.PK }, collection.Select(order => order.PK));
		}

		#endregion

		[ExpectNoExceptions()]
		[TestDate(2004, 6, 1)]
		public void TestQueryDeciderFilters()
		{
			OrderCollection collection = new OrderCollection(Factory);

			Order.JD_OrderDate = new ZDateTime(2004, 5, 28);
			Order.JD_OrderNumber = "ORDERNUM1";

			Factory.Save();

			foreach (ZQueryProviderCodeDescription pair in FilterBO.JD_DateFilterType_List)
			{
				FilterBO.JD_DateFilterType = pair.Code;
				FilterBO.JD_FromDate = new ZDateTime(2004, 5, 25);
				FilterBO.JD_ToDate = new ZDateTime(2004, 5, 30);

				collection.AdditionalFilter = FilterBO.Filter;

				if (pair.Code == OrdersConstants.DateFilterTypes.None ||
					pair.Code == OrdersConstants.DateFilterTypes.All ||
					pair.Code == OrdersConstants.DateFilterTypes.MostCommon ||
					pair.Code == OrdersConstants.DateFilterTypes.OrderDate)
				{
					Assert("Contains Order", collection.Contains(Order));
				}
				else
				{
					Assert("Does not contain Order", !collection.Contains(Order));
				}

				FilterBO.JD_FromDate = ZDateTime.Empty;
				FilterBO.JD_ToDate = ZDateTime.Empty;
			}

			foreach (ZQueryProviderCodeDescription pair in FilterBO.JD_NumberFilterType_List)
			{
				FilterBO.JD_NumberFilterType = pair.Code;
				FilterBO.JD_Number = "ORDERNUM1";

				collection.AdditionalFilter = FilterBO.Filter;

				if (pair.Code == OrdersConstants.NumberFilterTypes.None ||
					pair.Code == OrdersConstants.NumberFilterTypes.All ||
					pair.Code == OrdersConstants.NumberFilterTypes.MostCommon ||
					pair.Code == OrdersConstants.NumberFilterTypes.OrderNumber)
				{
					Assert("Contains Order", collection.Contains(Order));
				}
				else
				{
					Assert("Does not contain Order", !collection.Contains(Order));
				}

				FilterBO.JD_Number = "";
			}

			foreach (ZQueryProviderCodeDescription pair in FilterBO.JD_OrgFilterType_List)
			{
				FilterBO.JD_OrgFilterType = pair.Code;
				FilterBO.JD_OH_Org1 = Buyer.PK;
				FilterBO.JD_OH_Org2 = Supplier.PK;

				collection.AdditionalFilter = FilterBO.Filter;

				FilterBO.JD_OH_Org1 = ZGuid.Empty;
				FilterBO.JD_OH_Org2 = ZGuid.Empty;
			}

			foreach (ZQueryProviderCodeDescription pair in FilterBO.JD_PortFilterType_List)
			{
				FilterBO.JD_PortFilterType = pair.Code;
				FilterBO.JD_RL_NKPort1 = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD").RL_Code;
				FilterBO.JD_RL_NKPort2 = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "NZAKL").RL_Code;

				collection.AdditionalFilter = FilterBO.Filter;

				FilterBO.JD_RL_NKPort1 = ZString.Empty;
				FilterBO.JD_RL_NKPort2 = ZString.Empty;
			}
		}

		#region Number Filters

		public void TestInvalidNumberFilterTypeSelection()
		{
			FilterBO.JD_ContainerMode = "xxx";
			FilterBO.JD_NumberFilterType = "splaty";
			Order.JD_OrderNumber = "order";
			FilterBO.JD_Number = "order";

			OrderCollection collection = new OrderCollection(Factory, FilterBO.Filter);
			AssertEquals("No match", 0, collection.Count);
		}

		public void TestOrderNumberFilter()
		{
			FilterBO.JD_NumberFilterType = OrdersConstants.NumberFilterTypes.OrderNumber;
			Order.JD_OrderNumber = "order";
			FilterBO.JD_Number = "splaty";

			OrderCollection collection = new OrderCollection(Factory, FilterBO.Filter);
			Factory.Save();

			AssertEquals("No match", 0, collection.Count);

			FilterBO.JD_Number = "order";
			collection = new OrderCollection(Factory, FilterBO.Filter);
			AssertEquals("1 match", 1, collection.Count);
		}

		public void TestMasterBillFilter()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_MasterBillNum = "master";

			CommonShipment shipment = consol.Shipments.AddNew();
			FilterBO.JD_Number = "master";
			Order.JD_JS = shipment.PK;
			Factory.Save();
			FilterBO.JD_NumberFilterType = OrdersConstants.NumberFilterTypes.MasterBill;

			FilterBO.JD_Number = "splaty";
			OrderCollection collection = new OrderCollection(Factory, FilterBO.Filter);
			AssertEquals("No match", 0, collection.Count);

			FilterBO.JD_Number = "master";
			collection = new OrderCollection(Factory, FilterBO.Filter);
			AssertEquals("1 match", 1, collection.Count);
		}

		public void TestHouseBillFilter()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_HouseBill = "house";
			Order.JD_JS = shipment.PK;
			Factory.Save();
			FilterBO.JD_NumberFilterType = OrdersConstants.NumberFilterTypes.HouseBill;

			FilterBO.JD_Number = "splaty";
			OrderCollection collection = new OrderCollection(Factory, FilterBO.Filter);
			AssertEquals("No match", 0, collection.Count);

			FilterBO.JD_Number = "house";
			collection = new OrderCollection(Factory, FilterBO.Filter);
			AssertEquals("1 match", 1, collection.Count);
		}

		public void TestProductNoFilter()
		{
			OrderLine line = Order.OrderLines.AddNew();
			FilterBO.BuyerPK = ZGuid.Empty;
			line.JO_Partno = "partno";
			Factory.Save();
			FilterBO.JD_NumberFilterType = OrdersConstants.NumberFilterTypes.ProductNo;

			FilterBO.JD_Number = "splaty";
			OrderCollection collection = new OrderCollection(Factory, FilterBO.Filter);
			AssertEquals("No match", 0, collection.Count);

			FilterBO.JD_Number = "partno";
			collection = new OrderCollection(Factory, FilterBO.Filter);
			AssertEquals("1 match", 1, collection.Count);
		}

		public void TestFilterByContainerNumber()
		{
			Order order1 = GetNewOrder();
			order1.JD_OrderNumber = "myorder1";
			order1.BuyerPK = Buyer.PK;
			order1.SupplierPK = Supplier.PK;

			OrderLine line1 = order1.OrderLines.AddNew();
			OrderLineDelivery delivery1 = line1.Deliveries.AddNew();
			OrderLineDeliverContainer container1 = delivery1.Containers.AddNew();
			container1.J5_ContainerNum = "contmatch";
			OrderLineDeliverContainer container2 = delivery1.Containers.AddNew();
			container2.J5_ContainerNum = "contmatch";

			Order order2 = GetNewOrder();
			order2.JD_OrderNumber = "myorder2";
			order2.BuyerPK = Buyer.PK;
			order2.SupplierPK = Supplier.PK;

			OrderLine line2 = order2.OrderLines.AddNew();
			OrderLineDelivery delivery2 = line2.Deliveries.AddNew();
			OrderLineDeliverContainer container3 = delivery1.Containers.AddNew();
			container3.J5_ContainerNum = "nomatch";
			OrderLineDeliverContainer container4 = delivery1.Containers.AddNew();
			container4.J5_ContainerNum = "nomatch";
			Factory.Save();

			FilterBO.JD_NumberFilterType = OrdersConstants.NumberFilterTypes.ContainerNo;
			FilterBO.JD_Number = "contmatch";

			OrderCollection collection = new OrderCollection(Factory, FilterBO.Filter);
			AssertEquals("Should only have 1 match", 1, collection.Count);
			AssertEquals("Matches order that has the container", order1.PK, collection[0].PK);
		}

		#endregion

		#region Organisation Filters

		public void TestInvalidOrgFilterTypeSelection()
		{
			FilterBO.JD_ContainerMode = "xxx";
			FilterBO.JD_OrgFilterType = "splaty";
			FilterBO.JD_OH_Org1 = ZGuid.NewZGuid();
			FilterBO.JD_OH_Org2 = ZGuid.NewZGuid();

			OrderCollection collection = new OrderCollection(Factory, FilterBO.Filter);
			AssertEquals("No match", 0, collection.Count);
		}

		public void TestBuyerFilter()
		{
			TestOrganisationFilter(OrdersConstants.OrgFilterTypes.BuyerSupplier, Order.Schema.BuyerPK, true);
		}

		public void TestSupplierFilter()
		{
			TestOrganisationFilter(OrdersConstants.OrgFilterTypes.BuyerSupplier, Order.Schema.SupplierPK, false);
		}

		public void TestSendingAgentFilter()
		{
			TestOrganisationFilter(OrdersConstants.OrgFilterTypes.SendingRecvAgent, AutoJobOrderHeader.Schema.JD_OH_SendingAgent, true);
		}

		public void TestRecvAgentFilter()
		{
			TestOrganisationFilter(OrdersConstants.OrgFilterTypes.SendingRecvAgent, AutoJobOrderHeader.Schema.JD_OH_ReceivingAgent, false);
		}

		void TestOrganisationFilter(ZString orgFilterType, string orderPropName, bool testOrg1)
		{
			FilterBO.JD_OrgFilterType = orgFilterType;
			Order.BuyerPK = Buyer.PK;
			FilterBO.JD_OH_Org1 = ZGuid.NewZGuid();
			FilterBO.JD_OH_Org2 = ZGuid.NewZGuid();

			Factory.Save();
			OrderCollection collection = new OrderCollection(Factory, FilterBO.Filter);

			AssertEquals("No match", 0, collection.Count);

			if (testOrg1)
			{
				FilterBO.JD_OH_Org1 = (ZGuid)Order[orderPropName];
				FilterBO.JD_OH_Org2 = ZGuid.Empty;
			}
			else
			{
				FilterBO.JD_OH_Org1 = ZGuid.Empty;
				FilterBO.JD_OH_Org2 = (ZGuid)Order[orderPropName];
			}

			collection = new OrderCollection(Factory, FilterBO.Filter);
			AssertEquals("1 match", 1, collection.Count);
		}

		#endregion

		#region Port Filters

		public void TestInvalidPortFilterTypeSelection()
		{
			FilterBO.JD_ContainerMode = "xxx";
			FilterBO.JD_PortFilterType = "splaty";
			FilterBO.JD_RL_NKPort1 = "a";
			FilterBO.JD_RL_NKPort1 = "b";

			OrderCollection collection = new OrderCollection(Factory, FilterBO.Filter);
			AssertEquals("No match", 0, collection.Count);
		}

		public void TestPortOfLoadingFilter()
		{
			TestPortFilter(OrdersConstants.PortFilterTypes.LoadDischargeCode, AutoJobOrderHeader.Schema.JD_RL_NKPortOfLoading, true);
		}

		public void TestPortOfDischargeFilter()
		{
			TestPortFilter(OrdersConstants.PortFilterTypes.LoadDischargeCode, AutoJobOrderHeader.Schema.JD_RL_NKPortOfDischarge, false);
		}

		public void TestGoodsAvailableAtFilter()
		{
			TestPortFilter(OrdersConstants.PortFilterTypes.AvailableAtDeliveredToCode, AutoJobOrderHeader.Schema.JD_RL_NKGoodsAvailableAt, true);
		}

		public void TestGoodsDeliveredToFilter()
		{
			TestPortFilter(OrdersConstants.PortFilterTypes.AvailableAtDeliveredToCode, AutoJobOrderHeader.Schema.JD_RL_NKGoodsDeliveredTo, false);
		}

		void TestPortFilter(ZString portFilterType, string portPropName, bool testPort1)
		{
			FilterBO.JD_PortFilterType = portFilterType;
			RefUNLOCO unloco = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
			Order[portPropName] = unloco.RL_Code;

			FilterBO.JD_RL_NKPort1 = "NZAKL";
			FilterBO.JD_RL_NKPort2 = "HKHKG";

			Factory.Save();

			OrderCollection collection = new OrderCollection(Factory, FilterBO.Filter);
			AssertEquals("No match", 0, collection.Count);

			if (testPort1)
			{
				FilterBO.JD_RL_NKPort1 = (ZString)Order[portPropName];
				FilterBO.JD_RL_NKPort2 = ZString.Empty;
			}
			else
			{
				FilterBO.JD_RL_NKPort1 = ZString.Empty;
				FilterBO.JD_RL_NKPort2 = (ZString)Order[portPropName];
			}
			collection = new OrderCollection(Factory, FilterBO.Filter);
			AssertEquals("1 match", 1, collection.Count);
		}

		#endregion

		#region List Properties

		public void TestJD_Shipment_List()
		{
			ShipmentCollection shipments = FilterBO.JD_JS_List;
			AssertNotNull(shipments);
			shipments.Load();
		}

		public void TestJD_RL_List()
		{
			RefUNLOCOCollection uNLOCOs = FilterBO.JD_RL_List;
			ZQuery topNFilter = new ZQuery();
			topNFilter.MaximumRows = 10;
			uNLOCOs.AdditionalFilter = topNFilter;
			AssertEquals("JD_RL_List", true, uNLOCOs.Count > 0);
		}

		#endregion

		#region Vessel Filter

		public void TestVesselFilter()
		{
			IList collection;

			ZString vesselName = "VesselName";
			Order order1 = GetNewOrder();
			order1.JD_RV_NKArrivalVessel = vesselName;

			Order order2 = GetNewOrder();
			Factory.Save();

			collection = Factory.Load(GetExpectedOrderType(), FilterBO.Filter);

			//Testing with empty Filter
			AssertEquals("Expecting to find Order1", true, collection.Contains(order1));
			AssertEquals("Expecting to find Order2", true, collection.Contains(order2));

			FilterBO.Vessel = vesselName;
			collection = Factory.Load(GetExpectedOrderType(), FilterBO.Filter);

			//Testing with Vessel Filter for the Vessel "VesselName"
			//ArrivalVessel is VesselName in Order1
			AssertEquals("Expecting to find Order1", true, collection.Contains(order1));
			AssertEquals("Not expecting to find Order2", false, collection.Contains(order2));

			order1.JD_RV_NKArrivalVessel = ZString.Empty;
			order1.JD_RV_NKIntermediateVessel = ZString.Empty;
			order1.JD_RV_NKDepartureVessel = ZString.Empty;
			order2.JD_RV_NKIntermediateVessel = vesselName;
			Factory.Save();
			collection = Factory.Load(GetExpectedOrderType(), FilterBO.Filter);

			//Testing with Vessel Filter for the Vessel "VesselName"
			//IntermediateVessel is VesselName in Order2
			AssertEquals("Not expecting to find Order1", false, collection.Contains(order1));
			AssertEquals("Expecting to find Order2", true, collection.Contains(order2));

			order2.JD_RV_NKIntermediateVessel = ZString.Empty;
			order1.JD_RV_NKDepartureVessel = vesselName;
			Factory.Save();
			collection = Factory.Load(GetExpectedOrderType(), FilterBO.Filter);

			//Testing with Vessel Filter for the Vessel "VesselName"
			//DepartureVessel is VesselName in Order1
			AssertEquals("Expecting to find Order1", true, collection.Contains(order1));
			AssertEquals("Not expecting to find Order2", false, collection.Contains(order2));

			order1.JD_RV_NKDepartureVessel = ZString.Empty;
			order1.JD_RV_NKArrivalVessel = ZString.Empty;
			order1.JD_RV_NKIntermediateVessel = ZString.Empty;
			Factory.Save();
			collection = Factory.Load(GetExpectedOrderType(), FilterBO.Filter);

			//Testing with Vessel Filter for the Vessel "VesselName"
			//VesselName is not present in either Order
			AssertEquals("Not expecting to find Order1", false, collection.Contains(order1));
			AssertEquals("Not expecting to find Order2", false, collection.Contains(order2));
		}
		#endregion

		#region AttachedOrders Filter

		public void TestAttachedOrdersFilter()
		{
			Order order1 = GetNewOrder();
			Order order2 = GetNewOrder();
			Order order3 = GetNewOrder();
			BusinessObject declaration = (BusinessObject)Factory.New<Integration.Customs.IBaseJobDeclaration>();
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();

			order1.JD_JE = declaration.PK;
			order2.JD_JS = shipment.PK;

			OrderCollection collection = new OrderCollection(Factory);
			FilterBO.ShowAttachedOrders = false;
			FilterBO.ShowUnAttachedOrders = false;

			Factory.Save();
			collection.AdditionalFilter = FilterBO.Filter;

			AssertEquals("Collection should contain Order1", true, collection.Contains(order1));
			AssertEquals("Collection should contain Order2", true, collection.Contains(order2));
			AssertEquals("Collection should contain Order3", true, collection.Contains(order3));

			FilterBO.ShowAttachedOrders = true;

			collection.AdditionalFilter = FilterBO.Filter;
			AssertEquals("Collection should contain Order1", true, collection.Contains(order1));
			AssertEquals("Collection should contain Order2", true, collection.Contains(order2));
			AssertEquals("Collection should not contain Order3", false, collection.Contains(order3));
		}
		#endregion

		#region UnAttachedOrders Filter
		public void TestUnAttachedOrdersFilter()
		{
			Order order1 = GetNewOrder();
			Order order2 = GetNewOrder();
			Order order3 = GetNewOrder();
			BusinessObject declaration = (BusinessObject)Factory.New<Integration.Customs.IBaseJobDeclaration>();
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();

			order1.JD_JE = declaration.PK;
			order2.JD_JS = shipment.PK;

			OrderCollection collection = new OrderCollection(Factory);
			FilterBO.ShowAttachedOrders = false;
			FilterBO.ShowUnAttachedOrders = false;

			Factory.Save();
			collection.AdditionalFilter = FilterBO.Filter;

			AssertEquals("Collection should container Order", true, collection.Contains(order1));
			AssertEquals("Collection should container Order", true, collection.Contains(order2));
			AssertEquals("Collection should container Order", true, collection.Contains(order3));

			FilterBO.ShowUnAttachedOrders = true;

			collection.AdditionalFilter = FilterBO.Filter;
			AssertEquals("Collection should not contain Order1", false, collection.Contains(order1));
			AssertEquals("Collection should not contain Order2", false, collection.Contains(order2));
			AssertEquals("Collection should contain Order3", true, collection.Contains(order3));
		}
		#endregion

		#region Staff Filter

		public void TestStaffFilter()
		{
			AssertNotNull("Order should be created", Order);
			OrderCollection collection = new OrderCollection(Factory, FilterBO.Filter);
			Factory.Save();

			AssertEquals("set-up order", 1, collection.Count);
			AssertEquals("Filter on Created User is default", OrdersConstants.StaffFilterTypes.StaffFilterUserRegistered, FilterBO.StaffFilterOption);

			GlbStaff creatingUser = Factory.New<GlbStaff>();
			creatingUser.GS_Code = "U1";
			Order testOrder = GetNewOrder();
			Factory.Save();

			collection.AdditionalFilter = FilterBO.Filter;
			AssertEquals("2 orders exist", 2, collection.Count);

			FilterBO.StaffFilterOption = OrdersConstants.StaffFilterTypes.StaffFilterUserRegistered;
			FilterBO.StaffFilter = creatingUser.PK;
			collection = new OrderCollection(Factory, FilterBO.Filter);
			collection.AdditionalFilter = FilterBO.Filter;
			AssertEquals("No orders created by Creating User", 0, collection.Count);

			testOrder.JD_SystemCreateUser = creatingUser.GS_Code;
			Factory.Save();
			collection = new OrderCollection(Factory, FilterBO.Filter);
			collection.AdditionalFilter = FilterBO.Filter;
			AssertEquals("1 order where creating user is the Creating User", 1, collection.Count);
		}

		public void TestStaffFilterValidation()
		{
			FilterBO.StaffFilterOption = "hello";
			AssertHasErrors(FilterBO.StaffFilterOptionInfo);

			FilterBO.StaffFilterOption = OrdersConstants.StaffFilterTypes.StaffFilterUserRegistered;
			AssertNoErrors(FilterBO.StaffFilterOptionInfo);

			FilterBO.StaffFilterOption = "unknown";
			AssertHasErrors(FilterBO.StaffFilterOptionInfo);

			FilterBO.StaffFilterOption = "";
			AssertNoErrors(FilterBO.StaffFilterOptionInfo);
		}

		[ExpectNoExceptions]
		public void TestShipmentNo_MaxLength()
		{
			FilterBO.JD_NumberFilterType = OrdersConstants.NumberFilterTypes.ShipmentNo;
			FilterBO.JD_Number = new string('A', AutoJobShipment.Schema.JS_UniqueConsignRefMaxLength + 1);

			var collection = new TrackingLegacyOrderCollection(Factory);
			collection.Load(FilterBO.Filter);
		}

		[ExpectNoExceptions]
		public void TestHouseBill_MaxLength()
		{
			FilterBO.JD_NumberFilterType = OrdersConstants.NumberFilterTypes.HouseBill;
			FilterBO.JD_Number = new string('A', AutoJobShipment.Schema.JS_HouseBillMaxLength + 1);

			var collection = new TrackingLegacyOrderCollection(Factory);
			collection.Load(FilterBO.Filter);
		}

		#endregion

		#region Implementation

		#region Order

		protected virtual Order GetNewOrder()
		{
			return Factory.NewWithValidTestData<Order>();
		}

		Order Order
		{
			get
			{
				if (fOrder == null)
				{
					fOrder = GetNewOrder();
				}
				return fOrder;
			}
		}
		Order fOrder;

		protected virtual Type GetExpectedOrderType()
		{
			return typeof(Order);
		}

		#endregion

		#region FilterFactory

		protected WebFilterBusinessObjectFactory FilterFactory
		{
			get
			{
				if (fFilterFactory == null)
				{
					fFilterFactory = GetNewFilterFactory();
				}
				return fFilterFactory;
			}
		}
		WebFilterBusinessObjectFactory fFilterFactory;

		protected virtual WebFilterBusinessObjectFactory GetNewFilterFactory()
		{
			return new WebFilterBusinessObjectFactory(new BusinessObjectFactory());
		}

		#endregion

		#region FilterBO

		protected OrdersFilterBusinessObject_Old_ForWeb FilterBO
		{
			get
			{
				if (fFilterBO == null)
				{
					fFilterBO = GetNewFilterBusinessObject();
				}
				return fFilterBO;
			}
		}
		OrdersFilterBusinessObject_Old_ForWeb fFilterBO;

		protected virtual OrdersFilterBusinessObject_Old_ForWeb GetNewFilterBusinessObject()
		{
			return FilterFactory.New<OrdersFilterBusinessObject_Old_ForWeb>();
		}

		#endregion

		#region Buyer / Supplier

		protected OrgHeader Buyer
		{
			get
			{
				if (fBuyer == null)
				{
					fBuyer = Helper.TestOrg;
					fBuyer.OH_Code = "_BUYER";
				}
				return fBuyer;
			}
		}
		OrgHeader fBuyer;

		protected OrgHeader Supplier
		{
			get
			{
				if (fSupplier == null)
				{
					fSupplier = Factory.New<OrgHeader>();
					fSupplier.OH_Code = "_SPLIR";
				}
				return fSupplier;
			}
		}
		OrgHeader fSupplier;

		#endregion

		#endregion
	}
}
