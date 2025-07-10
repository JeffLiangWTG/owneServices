using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(TrackingBookingOrderLink))]
	[HttpContextEnabledTest]
	public class TrackingBookingOrderLinkTest : NonPersistentBusinessObjectTestCase
	{
		#region Test Cases

		public void TestAvailableOrders()
		{
			AssertEquals(TestBooking.AvailableOrders.Count, TestOrderLink.AvailableOrders.Count);
			foreach (Order itemOrder in TestBooking.AvailableOrders)
			{
				Assert(TestOrderLink.AvailableOrders.Contains(itemOrder));
			}
			foreach (Order itemOrder in TestOrderLink.AvailableOrders)
			{
				Assert(TestBooking.AvailableOrders.Contains(itemOrder));
			}
		}

		public void TestBookingLink()
		{
			AssertNotNull(TestOrderLink.Booking);
			AssertEquals(TestBooking, TestOrderLink.Booking);
		}

		public void TestHasChanges()
		{
			AssertNotNull(TestOrderLink.LinkedOrder);
			AssertEquals(true, TestOrderLink.HasChanges);

			TestOrderLink.HasChanges = false;
			AssertEquals(true, TestOrderLink.HasChanges);

			TestOrderLink.OrderPK = ZGuid.Empty;
			AssertNull(TestOrderLink.LinkedOrder);
			AssertEquals(false, TestOrderLink.HasChanges);

			TestOrderLink.HasChanges = true;
			AssertNull(TestOrderLink.LinkedOrder);
			AssertEquals(false, TestOrderLink.HasChanges);
		}

		public void TestOrderNumber()
		{
			AssertNotNull(TestOrderLink.LinkedOrder);

			TestOrderLink.LinkedOrder.JD_OrderNumber = "Order1";
			AssertEquals("Order1", TestOrderLink.OrderNumber);

			TestOrderLink.LinkedOrder.JD_OrderNumber = "Order2";
			AssertEquals("Order2", TestOrderLink.OrderNumber);

			TestOrderLink.OrderPK = ZGuid.Empty;
			AssertNull(TestOrderLink.LinkedOrder);
			AssertEquals(ZString.Empty, TestOrderLink.OrderNumber);
		}

		public void TestOrderNumberInfo()
		{
			AssertEquals(TrackingBookingOrderLink.Schema.OrderNumber, TestOrderLink.OrderNumberInfo.Name);
		}

		public void TestOrderDate()
		{
			AssertNotNull(TestOrderLink.LinkedOrder);

			TestOrderLink.LinkedOrder.JD_OrderDate = new ZDateTime(2009, 07, 27);
			AssertEquals(new ZDateTime(2009, 07, 27), TestOrderLink.OrderDate);

			TestOrderLink.LinkedOrder.JD_OrderDate = new ZDateTime(2009, 07, 28);
			AssertEquals(new ZDateTime(2009, 07, 28), TestOrderLink.OrderDate);

			TestOrderLink.OrderPK = ZGuid.Empty;
			AssertNull(TestOrderLink.LinkedOrder);
			AssertEquals(ZDateTime.Empty, TestOrderLink.OrderDate);
		}

		public void TestOrderDateInfo()
		{
			AssertEquals(TrackingBookingOrderLink.Schema.OrderDate, TestOrderLink.OrderDateInfo.Name);
		}

		public void TestOrderGoodsDescription()
		{
			AssertNotNull(TestOrderLink.LinkedOrder);

			TestOrderLink.LinkedOrder.JD_OrderGoodsDescription = "Order1";
			AssertEquals("Order1", TestOrderLink.OrderGoodsDescription);

			TestOrderLink.LinkedOrder.JD_OrderGoodsDescription = "Order2";
			AssertEquals("Order2", TestOrderLink.OrderGoodsDescription);

			TestOrderLink.OrderPK = ZGuid.Empty;
			AssertNull(TestOrderLink.LinkedOrder);
			AssertEquals(ZString.Empty, TestOrderLink.OrderGoodsDescription);
		}

		public void TestOrderGoodsDescriptionInfo()
		{
			AssertEquals(TrackingBookingOrderLink.Schema.OrderGoodsDescription, TestOrderLink.OrderGoodsDescriptionInfo.Name);
		}

		public void TestOrderPKAndLinkedOrder()
		{
			AssertNotNull(TestOrderLink);
			AssertNotNull(TestOrderLink.LinkedOrder);
			AssertEquals(TestOrder.PK, TestOrderLink.OrderPK);
			AssertEquals(TestOrder, TestOrderLink.LinkedOrder);
			Assert(TestBooking.AttachedOrders.Contains(TestOrder));

			Order newOrder = Factory.NewWithValidTestData<Order>();

			newOrder.JD_TransportMode = Constants.RateMode.AIR;
			newOrder.SupplierPK = Helper.TestSiteUser.LoggedInOrganisation.PK;
			Factory.Save();

			TestOrderLink.OrderPK = newOrder.PK;
			AssertNoErrors(TestOrderLink.OrderPKInfo);
			AssertEquals(newOrder.PK, TestOrderLink.OrderPK);
			AssertEquals(newOrder, TestOrderLink.LinkedOrder);
			AssertEquals(true, TestBooking.AttachedOrders.Contains(newOrder));
			AssertEquals(false, TestBooking.AttachedOrders.Contains(TestOrder));
		}

		public void TestOrderPKForAlreadyAttachedOrder()
		{
			AssertNotNull(TestOrderLink);
			TestOrderLink.OrderPK = ZGuid.Empty;
			AssertNull(TestOrderLink.LinkedOrder);
			AssertNotEquals(TestOrder.PK, TestOrderLink.OrderPK);
			Assert(!TestBooking.AttachedOrders.Contains(TestOrder));

			TrackingBooking newBooking = new TrackingBooking(Factory, Helper.TestSiteUser);
			TestOrder.JD_TransportMode = ((IAttachOrders)TestBooking.Booking).TransportMode;
			TestOrder.JD_ContainerMode = ((IAttachOrders)TestBooking.Booking).ContainerMode;
			newBooking.AttachedOrderLinks.Add(new TrackingBookingOrderLink(newBooking, TestOrder.PK));
			AssertEquals(true, newBooking.AttachedOrders.Contains(TestOrder));

			AssertNoErrors(TestOrderLink.OrderPKInfo);
			TestOrderLink.OrderPK = TestOrder.PK;
			AssertEquals(false, TestBooking.AttachedOrders.Contains(TestOrder));
			AssertHasError(TestOrderLink.OrderPKInfo, "This Order cannot be chosen here as it is already attached to a Shipment.");
		}

		public void TestOrderPKForInactiveOrder()
		{
			AssertNotNull(TestOrderLink);
			TestOrderLink.OrderPK = ZGuid.Empty;
			Assert(!TestBooking.AttachedOrders.Contains(TestOrder));

			TestOrder.IsCancelled = true;
			AssertNoErrors(TestOrderLink.OrderPKInfo);
			TestOrderLink.OrderPK = TestOrder.PK;
			AssertEquals(false, TestBooking.AttachedOrders.Contains(TestOrder));
			AssertHasError(TestOrderLink.OrderPKInfo, "This Order cannot be chosen here as it is inactive.");
		}

		public void TestOrderPKForCanceledOrder()
		{
			AssertNotNull(TestOrderLink);
			TestOrderLink.OrderPK = ZGuid.Empty;
			Assert(!TestBooking.AttachedOrders.Contains(TestOrder));

			TestOrder.JD_OrderStatus = Constants.OrderStatus.Cancelled;
			AssertNoErrors(TestOrderLink.OrderPKInfo);
			TestOrderLink.OrderPK = TestOrder.PK;
			AssertEquals(false, TestBooking.AttachedOrders.Contains(TestOrder));
			AssertHasError(TestOrderLink.OrderPKInfo, "This Order cannot be chosen here as it has been canceled.");
		}

		public void TestOrderPKWithDifferentTransportMode()
		{
			AssertNotNull(TestOrderLink);
			TestOrderLink.OrderPK = ZGuid.Empty;
			Assert(!TestBooking.AttachedOrders.Contains(TestOrder));

			TestOrder.JD_TransportMode = Constants.RateMode.SEA;
			TestOrder.JD_ContainerMode = ((IAttachOrders)TestBooking.Booking).ContainerMode;
			AssertNoErrors(TestOrderLink.OrderPKInfo);
			TestOrderLink.OrderPK = TestOrder.PK;
			AssertEquals(false, TestBooking.AttachedOrders.Contains(TestOrder));
			AssertHasError(TestOrderLink.OrderPKInfo, "This Order cannot be chosen here as the Transport Mode is different to that of the Shipment.");
		}

		public void TestOrderPKWithDifferentContainerMode()
		{
			AssertNotNull(TestOrderLink);
			TestOrderLink.OrderPK = ZGuid.Empty;
			Assert(!TestBooking.AttachedOrders.Contains(TestOrder));

			TestOrder.JD_TransportMode = Constants.RateMode.AIR;
			TestOrder.JD_ContainerMode = Constants.RateMode.AIR;
			AssertNoErrors(TestOrderLink.OrderPKInfo);
			TestOrderLink.OrderPK = TestOrder.PK;
			AssertEquals(true, TestBooking.AttachedOrders.Contains(TestOrder));
			AssertHasWarning(TestOrderLink.OrderPKInfo, "The Container Mode of this Order is different to that of the Shipment.");
		}

		public void TestOrderPKWithNoSupplierAndMatchingBuyer()
		{
			bool cachedRegistryValue = WebDataRegistry.Instance.BookingAttachOrdersWithoutSupplier.Value;
			WebDataRegistry.Instance.BookingAttachOrdersWithoutSupplier.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertNotNull(TestOrderLink);
			TestOrderLink.OrderPK = ZGuid.Empty;
			Assert(!TestBooking.AttachedOrders.Contains(TestOrder));
			OrgHeader consignee = Factory.NewWithValidTestData<OrgHeader>();
			TestBooking.ConsigneeOrganisationPK = consignee.PK;

			TestOrder.SupplierPK = ZGuid.Empty;
			TestOrder.BuyerPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			Factory.Save();
			AssertNoErrors(TestOrderLink.OrderPKInfo);
			TestOrderLink.OrderPK = TestOrder.PK;
			AssertEquals(false, TestBooking.AttachedOrders.Contains(TestOrder));
			AssertHasError(TestOrderLink.OrderPKInfo, "This Order cannot be chosen here as the Buyer does not match the Consignee");

			TestOrder.BuyerPK = consignee.PK;
			Factory.Save();
			TestOrderLink.OrderPK = TestOrder.PK;
			AssertEquals(true, TestBooking.AttachedOrders.Contains(TestOrder));
			AssertNoErrors(TestOrderLink.OrderPKInfo);

			TestOrderLink.OrderPK = ZGuid.Empty;
			Assert(!TestBooking.AttachedOrders.Contains(TestOrder));
			TestOrder.SupplierPK = Helper.TestSiteUser.LoggedInOrganisation.PK;
			TestOrder.BuyerPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			Factory.Save();
			TestOrderLink.OrderPK = TestOrder.PK;
			AssertEquals(true, TestBooking.AttachedOrders.Contains(TestOrder));
			AssertNoErrors(TestOrderLink.OrderPKInfo);

			WebDataRegistry.Instance.BookingAttachOrdersWithoutSupplier.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			TestOrderLink.OrderPK = ZGuid.Empty;
			Assert(!TestBooking.AttachedOrders.Contains(TestOrder));

			TestOrder.SupplierPK = ZGuid.Empty;
			TestOrder.BuyerPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			Factory.Save();
			AssertNoErrors(TestOrderLink.OrderPKInfo);
			TestOrderLink.OrderPK = TestOrder.PK;
			AssertEquals(false, TestBooking.AttachedOrders.Contains(TestOrder));
			AssertHasError(TestOrderLink.OrderPKInfo, "This Order is not available for this Booking.");

			TestOrder.BuyerPK = consignee.PK;
			TestOrderLink.OrderPK = TestOrder.PK;
			Factory.Save();
			AssertEquals(false, TestBooking.AttachedOrders.Contains(TestOrder));
			AssertHasError(TestOrderLink.OrderPKInfo, "This Order is not available for this Booking.");

			TestOrderLink.OrderPK = ZGuid.Empty;
			Assert(!TestBooking.AttachedOrders.Contains(TestOrder));
			TestOrder.SupplierPK = Helper.TestSiteUser.LoggedInOrganisation.PK;
			TestOrder.BuyerPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			Factory.Save();
			TestOrderLink.OrderPK = TestOrder.PK;
			AssertEquals(true, TestBooking.AttachedOrders.Contains(TestOrder));
			AssertNoErrors(TestOrderLink.OrderPKInfo);

			WebDataRegistry.Instance.BookingAttachOrdersWithoutSupplier.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, cachedRegistryValue);
		}

		public void TestOrderPKInfo()
		{
			AssertEquals(TrackingBookingOrderLink.Schema.OrderPK, TestOrderLink.OrderPKInfo.Name);
			AssertEquals(TestOrder, TestOrderLink.LinkedOrder);
			AssertEquals(TestOrder.HumanReadableName, TestOrderLink.OrderPKInfo.HumanReadableName);

			TestOrderLink.OrderPK = ZGuid.Empty;
			AssertNull(TestOrderLink.LinkedOrder);
			AssertEquals("Order", TestOrderLink.OrderPKInfo.HumanReadableName);
		}

		public void TestSchema()
		{
			AssertEquals("OrderDate", TrackingBookingOrderLink.Schema.OrderDate);
			AssertEquals("OrderPK", TrackingBookingOrderLink.Schema.OrderPK);
			AssertEquals("OrderNumber", TrackingBookingOrderLink.Schema.OrderNumber);
			AssertEquals("OrderGoodsDescription", TrackingBookingOrderLink.Schema.OrderGoodsDescription);
		}

		#endregion

		#region Implementation

		protected TrackingBookingOrderLink TestOrderLink;
		protected TrackingBooking TestBooking;
		protected Order TestOrder;
		protected TestHelper Helper;

		protected override void SetUp()
		{
			base.SetUp();
			Helper = new TestHelper(Factory);
			TestBooking = new TrackingBooking(Factory, Helper.TestSiteUser);
			TestBooking.Booking.JS_TransportMode = Constants.RateMode.AIR;
			TestOrder = Factory.NewWithValidTestData<Order>();
			TestOrder.JD_TransportMode = Constants.RateMode.AIR;
			TestOrder.JD_ContainerMode = ((IAttachOrders)TestBooking.Booking).ContainerMode;
			TestOrder.BuyerPK = Helper.TestSiteUser.LoggedInOrganisation.PK;
			Factory.Save();

			TestBooking.AttachedOrders.Add(TestOrder);
			Assert(TestBooking.AttachedOrders.Contains(TestOrder));
			TestOrderLink = new TrackingBookingOrderLink(TestBooking, TestOrder.PK);
			Factory.Save();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new TrackingBookingOrderLink(new TrackingBooking(Factory, new TestHelper(Factory).TestSiteUser), ZGuid.Empty);
		}

		#endregion
	}
}
