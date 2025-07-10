using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using SupplierBookingStatus = Enterprise.Core.Constants.SupplierBookingStatus;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	[TestedType(typeof(JobSupplierBookingLine))]
	sealed class JobSupplierBookingLineTest : EnterpriseBusinessObjectTestCase
	{
		public void TestBookingAndBookingLineId()
		{
			var booking = Factory.New<JobSupplierBooking>();
			booking.JSB_BookingId = "JSB001";
			var bookingLine = booking.SupplierBookingLines.AddNew();
			bookingLine.JSL_BookingLineId = "JSL001";

			AssertEquals("JSB001 - JSL001", bookingLine.BookingAndBookingLineId);
		}

		#region Order Line Open Quantity Synchronisation

		public void TestChangingBookedQuantityUpdatesOrderLineOpenQuantity_WithValidStatus()
		{
			var bookingLine = GetSupplierBookingLine();

			AssertEquals("Precondition: JO_OpenQuantity", 100m, bookingLine.OrderLine.JO_OpenQuantity);
			AssertEquals("Precondition: JSL_BookedQuantity", 0m, bookingLine.JSL_BookedQuantity);

			bookingLine.JSL_BookedQuantity = 20m;

			AssertEquals("JO_OpenQuantity should have been updated", 80m, bookingLine.OrderLine.JO_OpenQuantity);
		}

		public void TestChangingBookedQuantityUpdatesOrderLineOpenQuantity_WithInValidStatus()
		{
			var bookingLine = GetSupplierBookingLine();
			bookingLine.SupplierBooking.JSB_Status = "INC";

			AssertEquals("Precondition: JO_OpenQuantity", 100m, bookingLine.OrderLine.JO_OpenQuantity);
			AssertEquals("Precondition: JSL_BookedQuantity", 0m, bookingLine.JSL_BookedQuantity);

			bookingLine.JSL_BookedQuantity = 20m;

			AssertEquals("JO_OpenQuantity should not have been updated", 100m, bookingLine.OrderLine.JO_OpenQuantity);
		}

		public void TestChangingStatusUpdatesOrderLineOpenQuantity()
		{
			var bookingLine = GetSupplierBookingLine();
			bookingLine.JSL_BookedQuantity = 20;

			AssertEquals("Precondition: JO_OpenQuantity", 80m, bookingLine.OrderLine.JO_OpenQuantity);

			bookingLine.SupplierBooking.JSB_Status = "INC";
			AssertEquals("JO_OpenQuantity should have been updated as the status implies it shouldn't be considered", 100m, bookingLine.OrderLine.JO_OpenQuantity);

			bookingLine.SupplierBooking.JSB_Status = "CAN";
			AssertEquals("JO_OpenQuantity should not have been updated as the status still implies it shouldn't be considered", 100m, bookingLine.OrderLine.JO_OpenQuantity);

			bookingLine.SupplierBooking.JSB_Status = "PLC";
			AssertEquals("JO_OpenQuantity should have been updated as the status implies it should be considered", 80m, bookingLine.OrderLine.JO_OpenQuantity);

			bookingLine.SupplierBooking.JSB_Status = "CNV";
			AssertEquals("JO_OpenQuantity should have been updated as the status implies it should be considered", 80m, bookingLine.OrderLine.JO_OpenQuantity);

			bookingLine.SupplierBooking.JSB_Status = "APP";
			AssertEquals("JO_OpenQuantity should not have been updated as the status still implies it should be considered", 80m, bookingLine.OrderLine.JO_OpenQuantity);
		}

		public void TestDetachingOrderLineUpdatesOrderLineOpenQuantity()
		{
			var bookingLine = GetSupplierBookingLine();
			bookingLine.JSL_BookedQuantity = 20;
			var orderLine = bookingLine.OrderLine;

			AssertEquals("Precondition: JO_OpenQuantity", 80m, orderLine.JO_OpenQuantity);

			bookingLine.JSL_JO_OrderLine = ZGuid.Empty;

			AssertEquals("JO_OpenQuantity should have updated when detaching the supplier booking line", 100m, orderLine.JO_OpenQuantity);
		}

		public void TestAttachingOrderLineUpdatesOrderLineOpenQuantity()
		{
			var bookingLine = GetSupplierBookingLine();
			bookingLine.JSL_BookedQuantity = 20;
			bookingLine.JSL_JO_OrderLine = ZGuid.Empty;

			var orderLine = Factory.NewWithValidTestData<OrderLine>();
			orderLine.JO_Quantity = 100;

			AssertEquals("Precondition: JO_OpenQuantity", 100m, orderLine.JO_OpenQuantity);

			bookingLine.JSL_JO_OrderLine = orderLine.PK;
			AssertEquals("JO_OpenQuantity should have updated when attaching the supplier booking line", 80m, orderLine.JO_OpenQuantity);
		}

		public void TestAttachingBookingUpdatesOrderLineOpenQuantity()
		{
			var bookingLine = GetSupplierBookingLine();
			var booking = bookingLine.SupplierBooking;
			bookingLine.JSL_JSB_Booking = ZGuid.Empty;
			bookingLine.JSL_BookedQuantity = 20;
			bookingLine.OrderLine.JO_Quantity = 100;

			// it's okay that this says 100 when it has an attached line that should make it 80 - because that attached line has no booking parent which makes it invalid.
			// that booking will either be deleted, or it will add a valid booking parent, which will correct the quantity
			AssertEquals("Precondition: JO_OpenQuantity", 100m, bookingLine.OrderLine.JO_OpenQuantity);

			bookingLine.JSL_JSB_Booking = booking.PK;
			AssertEquals("JO_OpenQuantity should have updated when attaching a valid supplier booking", 80m, bookingLine.OrderLine.JO_OpenQuantity);

			bookingLine.JSL_JSB_Booking = ZGuid.Empty;
			AssertEquals("JO_OpenQuantity should have updated when removing a valid supplier booking", 100m, bookingLine.OrderLine.JO_OpenQuantity);

			booking.JSB_Status = "INC";
			bookingLine.JSL_JSB_Booking = booking.PK;
			AssertEquals("JO_OpenQuantity shouldn't have updated when removing an invalid supplier booking", 100m, bookingLine.OrderLine.JO_OpenQuantity);

			bookingLine.JSL_JSB_Booking = ZGuid.Empty;
			AssertEquals("JO_OpenQuantity shouldn't have updated when attaching an invalid supplier booking", 100m, bookingLine.OrderLine.JO_OpenQuantity);
		}

		public void TestDeletingUpdatesOrderLineOpenQuantity()
		{
			var bookingLine = GetSupplierBookingLine();
			bookingLine.JSL_BookedQuantity = 20;
			var orderLine = bookingLine.OrderLine;

			AssertEquals("Precondition: JO_OpenQuantity", 80m, orderLine.JO_OpenQuantity);

			bookingLine.Delete();

			AssertEquals("JO_OpenQuantity should have updated when deleting the supplier booking line", 100m, orderLine.JO_OpenQuantity);
		}

		#endregion

		#region To Be Packed Synchronisation

		public void TestOnUpdateReceivedQuantity_ShouldUpdateRemainingQuantityToBePacked()
		{
			var bookingLine = GetSupplierBookingLine();

			CombineAssertions("Preconditions", () =>
			{
				AssertEquals(50m, bookingLine.JSL_ReceivedQuantity);
				AssertEquals(100m, bookingLine.JSL_RemainingQuantityToBePacked);
			});

			bookingLine.JSL_ReceivedQuantity = 60m;
			AssertEquals("Should add difference, which is +10", 110m, bookingLine.JSL_RemainingQuantityToBePacked);

			bookingLine.JSL_ReceivedQuantity = 40m;
			AssertEquals("Should add difference, which is -10", 90m, bookingLine.JSL_RemainingQuantityToBePacked);

			bookingLine.JSL_ReceivedQuantity = 0m;
			AssertEquals("Should add difference, which is 0", 50m, bookingLine.JSL_RemainingQuantityToBePacked);
		}

		public void TestOnUpdateReceivedPackages_ShouldUpdateRemainingPackagesToBePacked()
		{
			var bookingLine = GetSupplierBookingLine();

			CombineAssertions("Preconditions", () =>
			{
				AssertEquals(50, bookingLine.JSL_ReceivedPackages);
				AssertEquals(100, bookingLine.JSL_RemainingPackagesToBePacked);
			});

			bookingLine.JSL_ReceivedPackages = 60;
			AssertEquals("Should add difference, which is +10", 110, bookingLine.JSL_RemainingPackagesToBePacked);

			bookingLine.JSL_ReceivedPackages = 40;
			AssertEquals("Should add difference, which is -10", 90, bookingLine.JSL_RemainingPackagesToBePacked);

			bookingLine.JSL_ReceivedPackages = 0;
			AssertEquals("Should add difference, which is 0", 50, bookingLine.JSL_RemainingPackagesToBePacked);
		}

		public void TestOnUpdateReceivedWeight_ShouldUpdateRemainingWeightToBePacked()
		{
			var bookingLine = GetSupplierBookingLine();

			CombineAssertions("Preconditions", () =>
			{
				AssertEquals(50m, bookingLine.JSL_ReceivedWeight);
				AssertEquals(100m, bookingLine.JSL_RemainingWeightToBePacked);
			});

			bookingLine.JSL_ReceivedWeight = 60m;
			AssertEquals("Should add difference, which is +10", 110m, bookingLine.JSL_RemainingWeightToBePacked);

			bookingLine.JSL_ReceivedWeight = 40m;
			AssertEquals("Should add difference, which is -10", 90m, bookingLine.JSL_RemainingWeightToBePacked);

			bookingLine.JSL_ReceivedWeight = 0m;
			AssertEquals("Should add difference, which is 0", 50m, bookingLine.JSL_RemainingWeightToBePacked);
		}

		public void TestOnUpdateReceivedVolume_ShouldUpdateRemainingVolumeToBePacked()
		{
			var bookingLine = GetSupplierBookingLine();

			CombineAssertions("Preconditions", () =>
			{
				AssertEquals(50m, bookingLine.JSL_ReceivedVolume);
				AssertEquals(100m, bookingLine.JSL_RemainingVolumeToBePacked);
			});

			bookingLine.JSL_ReceivedVolume = 60m;
			AssertEquals("Should add difference, which is +10", 110m, bookingLine.JSL_RemainingVolumeToBePacked);

			bookingLine.JSL_ReceivedVolume = 40m;
			AssertEquals("Should add difference, which is -10", 90m, bookingLine.JSL_RemainingVolumeToBePacked);

			bookingLine.JSL_ReceivedVolume = 0m;
			AssertEquals("Should add difference, which is 0", 50m, bookingLine.JSL_RemainingVolumeToBePacked);
		}

		#endregion

		JobSupplierBookingLine GetSupplierBookingLine()
		{
			var supplierBookingLine = Factory.NewWithValidTestData<JobSupplierBookingLine>();
			supplierBookingLine.OrderLine.JO_Quantity = 100;
			supplierBookingLine.SupplierBooking.JSB_Status = "PLC";
			OrderManagerTestHelper.SetReceivedValues(supplierBookingLine, 50m, 50, 50m, 50m);
			OrderManagerTestHelper.SetToBePacked(supplierBookingLine, 100m, 100, 100m, 100);

			return supplierBookingLine;
		}

		public void TestManufacturerAddress()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var address = org.MainAddress;
			var supplierBookingLine = Factory.NewWithValidTestData<JobSupplierBookingLine>();
			var manufacturerQuery = new ZQuery(JobDocAddressSchema.E2_ParentID, supplierBookingLine.PK);
			manufacturerQuery.AddToFilter(JobDocAddressSchema.E2_AddressType, "MAN");

			AssertEquals("No Manufacturer Address were found", false, Factory.Exists(typeof(JobDocAddress), manufacturerQuery));

			supplierBookingLine.ManufacturerNameOrPK = org.PK.ToString();
			AssertEquals("Manufacturer Address was found", 1, Factory.Load<JobDocAddress>(manufacturerQuery).Length);
			AssertEquals("ManufacturerCode", org.OH_Code, supplierBookingLine.ManufacturerAddress.Organisation.OH_Code);
			AssertEquals("Manufacturer AdressCode", address.OA_Address1, supplierBookingLine.ManufacturerAddress.E2_Address1);
		}

		#region Shipment Window Log

		public void TestExceptionRaisedEventForMismatchedShipmentWindow()
		{
			foreach (var orderWindowDate in new ZDate[] { ZDate.Empty, new ZDate(2022, 9, 1) })
			{
				foreach (var orderLineWindowDate in new ZDate[] { ZDate.Empty, new ZDate(2022, 9, 1), new ZDate(2022, 9, 2) })
				{
					foreach (var bookingLineWindowDate in new ZDate[] { ZDate.Empty, new ZDate(2022, 9, 1), new ZDate(2022, 9, 2), new ZDate(2022, 9, 3) })
					{
						var shipmentWindowDate = (orderLineWindowDate.IsEmpty ? orderWindowDate : orderLineWindowDate);

						var hasEvent = !shipmentWindowDate.IsEmpty && bookingLineWindowDate != shipmentWindowDate;

						CheckExceptionRaisedEvent(orderWindowDate, ZDate.Empty, orderLineWindowDate, ZDate.Empty, bookingLineWindowDate, ZDate.Empty, true, hasEvent, false);
						CheckExceptionRaisedEvent(orderWindowDate, ZDate.Empty, orderLineWindowDate, ZDate.Empty, bookingLineWindowDate, ZDate.Empty, false, hasEvent, false);

						CheckExceptionRaisedEvent(ZDate.Empty, orderWindowDate, ZDate.Empty, orderLineWindowDate, ZDate.Empty, bookingLineWindowDate, true, false, hasEvent);
						CheckExceptionRaisedEvent(ZDate.Empty, orderWindowDate, ZDate.Empty, orderLineWindowDate, ZDate.Empty, bookingLineWindowDate, false, false, hasEvent);
					}
				}
			}
		}

		public void TestExceptionRaisedEventForMismatchedShipmentWindow_ValidStatus()
		{
			foreach (var bookingStatus in typeof(SupplierBookingStatus).GetFields().Select(field => field.GetValue(null) as string))
			{
				var hasEvent = bookingStatus == SupplierBookingStatus.Placed;
				CheckExceptionRaisedEvent(ZDate.Empty, ZDate.Empty, new ZDate(2022, 9, 1), new ZDate(2022, 9, 2), new ZDate(2022, 10, 1), new ZDate(2022, 10, 2), false, hasEvent, hasEvent, bookingStatus);
				CheckExceptionRaisedEvent(ZDate.Empty, ZDate.Empty, new ZDate(2022, 9, 1), new ZDate(2022, 9, 2), new ZDate(2022, 10, 1), new ZDate(2022, 10, 2), true, hasEvent, hasEvent, bookingStatus);
			}
		}

		public void TestExceptionRaisedEventForMismatchedShipmentWindow_ChangesStatus()
		{
			CreateOrderLineAndSupplierBooingWithShipmentWindow(ZDate.Empty, ZDate.Empty, new ZDate(2022, 9, 1), new ZDate(2022, 9, 2), new ZDate(2022, 10, 1), new ZDate(2022, 10, 2), SupplierBookingStatus.Incomplete, out var orderLine, out var supplierBookingLine);
			AssertOrderLineShipmentWindowLogs(supplierBookingLine, orderLine, 0, 0);

			Factory.Save();
			supplierBookingLine.SupplierBooking.JSB_Status = SupplierBookingStatus.Placed;
			AssertOrderLineShipmentWindowLogs(supplierBookingLine, orderLine, 1, 1);

			supplierBookingLine.SupplierBooking.JSB_Status = SupplierBookingStatus.Rejected;
			Factory.Save();
			supplierBookingLine.SupplierBooking.JSB_Status = SupplierBookingStatus.Placed;
			AssertOrderLineShipmentWindowLogs(supplierBookingLine, orderLine, 2, 2);

			supplierBookingLine.SupplierBooking.JSB_Status = SupplierBookingStatus.Cancelled;
			Factory.Save();
			supplierBookingLine.SupplierBooking.JSB_Status = SupplierBookingStatus.Placed;
			AssertOrderLineShipmentWindowLogs(supplierBookingLine, orderLine, 2, 2);
		}

		void CheckExceptionRaisedEvent(ZDate orderShipmentWindowStart, ZDate orderShipmentWindowEnd, ZDate orderLineShipmentWindowStart, ZDate orderLineShipmentWindowEnd, ZDate bookingLineShipmentWindowStart, ZDate bookingLineShipmentWindowEnd, bool testInsert, bool hasStartEvent, bool hasEndEvent, string bookingStatus = SupplierBookingStatus.Placed)
		{
			CreateOrderLineAndSupplierBooingWithShipmentWindow(orderShipmentWindowStart, orderShipmentWindowEnd, orderLineShipmentWindowStart, orderLineShipmentWindowEnd, bookingLineShipmentWindowStart, bookingLineShipmentWindowEnd, bookingStatus, out var orderLine, out var supplierBookingLine);
			if (!testInsert)
			{
				supplierBookingLine.JSL_ShipmentWindowStart = orderLineShipmentWindowStart.IsEmpty ? orderShipmentWindowStart : orderLineShipmentWindowStart;
				supplierBookingLine.JSL_ShipmentWindowEnd = orderLineShipmentWindowEnd.IsEmpty ? orderShipmentWindowEnd : orderLineShipmentWindowEnd;
				AssertOrderLineShipmentWindowLogs(supplierBookingLine, orderLine, 0, 0);
				Factory.Save();

				supplierBookingLine.JSL_ShipmentWindowStart = bookingLineShipmentWindowStart;
				supplierBookingLine.JSL_ShipmentWindowEnd = bookingLineShipmentWindowEnd;
			}
			AssertOrderLineShipmentWindowLogs(supplierBookingLine, orderLine, hasStartEvent ? 1 : 0, hasEndEvent ? 1 : 0);

			Factory.Save();
			supplierBookingLine.JSL_ShipmentWindowStart = bookingLineShipmentWindowStart;
			supplierBookingLine.JSL_ShipmentWindowEnd = bookingLineShipmentWindowEnd;
			AssertOrderLineShipmentWindowLogs(supplierBookingLine, orderLine, hasStartEvent ? 1 : 0, hasEndEvent ? 1 : 0);
		}

		void CreateOrderLineAndSupplierBooingWithShipmentWindow(ZDate orderShipmentWindowStart, ZDate orderShipmentWindowEnd, ZDate orderLineShipmentWindowStart, ZDate orderLineShipmentWindowEnd, ZDate bookingLineShipmentWindowStart, ZDate bookingLineShipmentWindowEnd, string bookingStatus, out OrderLine orderLine, out JobSupplierBookingLine supplierBookingLine)
		{
			var order = Factory.NewWithValidTestData<Order>();
			order.JD_ShipmentWindowStart = orderShipmentWindowStart;
			order.JD_ShipmentWindowEnd = orderShipmentWindowEnd;
			orderLine = Factory.NewWithValidTestData<OrderLine>();
			orderLine.JO_JD = order.PK;
			orderLine.JO_ShipmentWindowStart = orderLineShipmentWindowStart;
			orderLine.JO_ShipmentWindowEnd = orderLineShipmentWindowEnd;
			Factory.Save();

			var supplierBooking = Factory.NewWithValidTestData<JobSupplierBooking>();
			supplierBooking.JSB_Status = bookingStatus;
			supplierBookingLine = Factory.NewWithValidTestData<JobSupplierBookingLine>();
			supplierBookingLine.JSL_JSB_Booking = supplierBooking.PK;
			supplierBookingLine.JSL_JO_OrderLine = orderLine.PK;
			supplierBookingLine.JSL_ShipmentWindowStart = bookingLineShipmentWindowStart;
			supplierBookingLine.JSL_ShipmentWindowEnd = bookingLineShipmentWindowEnd;
		}

		void AssertOrderLineShipmentWindowLogs(JobSupplierBookingLine supplierBookingLine, OrderLine orderLine, int startEventCount, int endEventCount)
		{
			supplierBookingLine.LogEventOnShipmentWindowDatesIfNeeded();
			var logs = orderLine.Logs.GetAllLogs();
			AssertEquals(startEventCount, logs.Where(log => log.SL_SE_NKEvent == Events.ExceptionRaised.Code && log.SL_Reference.EndsWith("TYP=Ship Window Start")).Count());
			AssertEquals(endEventCount, logs.Where(log => log.SL_SE_NKEvent == Events.ExceptionRaised.Code && log.SL_Reference.EndsWith("TYP=Ship Window End")).Count());
		}

		#endregion

		#region IDocAddresses

		public void TestDocAddresses()
		{
			var supplierBookingLine = Factory.New<JobSupplierBookingLine>();
			AssertEquals(typeof(JobDocAddressDependentCollection), supplierBookingLine.DocAddresses.GetType());

			var docAddresses = supplierBookingLine as IDocAddresses;
			var addressType = docAddresses.GetDocAddressRequirement(DocAddressType.Manufacturer).DefaultDocAddressType;
			AssertEquals(DocAddressType.Manufacturer, addressType);
		}

		public void TestSupportedAddressTypes()
		{
			IDocAddresses supplierBookingLine = Factory.New<JobSupplierBookingLine>();
			AssertContainsExactElementsInAnyOrder(new[]
			{
				DocAddressType.Manufacturer,
			}, supplierBookingLine.SupportedAddressTypes);
		}

		public void TestGetOrgHeaderList()
		{
			IDocAddresses supplierBookingLine = Factory.New<JobSupplierBookingLine>();
			AssertNull(supplierBookingLine.GetOrgHeaderList(DocAddressType.ControllingCustomer));

			var orgHeaderList = supplierBookingLine.GetOrgHeaderList(DocAddressType.Manufacturer);
			AssertNotNull(orgHeaderList);
		}

		public void TestGetCanOverrideCheckpoint()
		{
			IDocAddresses supplierBookingLine = Factory.New<JobSupplierBookingLine>();
			AssertEquals(Env.Security.None, supplierBookingLine.GetCanOverrideCheckpoint(null));
		}

		#endregion

		#region LooseCargo PackLine

		public void TestLooseCargoPackLine()
		{
			var bookingLine = GetSupplierBookingLine();
			Factory.Save();

			var packLine = Factory.NewWithValidTestData<ForwardingPackLine>();
			packLine.JL_JSL_BookingLine = bookingLine.PK;
			Factory.Save();

			bookingLine.SupplierBooking.JSB_Status = SupplierBookingStatus.Placed;
			bookingLine.SupplierBooking.JSB_ContainerMode = SupplierBookingLoadModeList.Codes.LSE;
			AssertNull("LooseCargoPackLine when SupplierBookingStatus is not CNV", bookingLine.LooseCargoPackLine);

			bookingLine.SupplierBooking.JSB_Status = SupplierBookingStatus.Converted;
			bookingLine.SupplierBooking.JSB_ContainerMode = SupplierBookingLoadModeList.Codes.CY;
			AssertNull("LooseCargoPackLine when SupplierBookingContainerMode is not LSE", bookingLine.LooseCargoPackLine);

			bookingLine.SupplierBooking.JSB_Status = SupplierBookingStatus.Converted;
			bookingLine.SupplierBooking.JSB_ContainerMode = SupplierBookingLoadModeList.Codes.LSE;
			AssertNotNull("LooseCargoPackLine when booking is CNV and LSE", bookingLine.LooseCargoPackLine);
		}

		#endregion

		#region IExternalRequestGenerationProvider

		public void TestGetRequestJobID()
		{
			var booking = Factory.NewWithValidTestData<JobSupplierBooking>();
			booking.JSB_BookingId = "S125";
			var bookingLine = Factory.NewWithValidTestData<JobSupplierBookingLine>();
			booking.SupplierBookingLines.Add(bookingLine);
			Factory.Save();

			AssertEquals("S125", bookingLine.GetRequestJobID());
		}

		public void TestGetRequestTypeCode()
		{
			var bookingLine = Factory.NewWithValidTestData<JobSupplierBookingLine>();
			AssertEquals(ExternalRequestTypes.Codes.SupplierBookingLine, bookingLine.GetRequestTypeCode());
		}

		public void TestGetRequestSupportedAddressInfo()
		{
			var assigneeOrg = Factory.NewWithValidTestData<OrgHeader>();
			assigneeOrg.Contacts.Add(Factory.NewWithValidTestData<OrgContact>());
			var reviewerOrg = Factory.NewWithValidTestData<OrgHeader>();
			reviewerOrg.Contacts.Add(Factory.NewWithValidTestData<OrgContact>());
			var controllingCustomerOrg = Factory.NewWithValidTestData<OrgHeader>();
			controllingCustomerOrg.Contacts.Add(Factory.NewWithValidTestData<OrgContact>());
			var manufactureOrg = Factory.NewWithValidTestData<OrgHeader>();
			manufactureOrg.Contacts.Add(Factory.NewWithValidTestData<OrgContact>());
			Factory.Save();

			var order = Factory.NewWithValidTestData<Order>();
			order.JD_OrderNumber = "X125";
			order.JD_OA_BuyerAddress = assigneeOrg.MainAddress.PK;
			order.JD_OC_BuyerContact = assigneeOrg.Contacts[0].PK;

			var orderLine = Factory.NewWithValidTestData<OrderLine>();
			order.OrderLines.Add(orderLine);

			var booking = Factory.NewWithValidTestData<JobSupplierBooking>();
			OrderManagerTestHelper.CreateDocAddressAndFillWithAddressAndContact(booking, (DocAddressType.ControllingCustomer), controllingCustomerOrg.MainAddress.PK, controllingCustomerOrg.Contacts[0].OC_ContactName);
			OrderManagerTestHelper.CreateDocAddressAndFillWithAddressAndContact(booking, (DocAddressType.SupplierDocumentaryAddress), reviewerOrg.MainAddress.PK, reviewerOrg.Contacts[0].OC_ContactName);

			var bookingLine = Factory.NewWithValidTestData<JobSupplierBookingLine>();
			bookingLine.JSL_JO_OrderLine = orderLine.PK;
			booking.SupplierBookingLines.Add(bookingLine);
			OrderManagerTestHelper.CreateDocAddressAndFillWithAddressAndContact(bookingLine, (DocAddressType.Manufacturer), manufactureOrg.MainAddress.PK, manufactureOrg.Contacts[0].OC_ContactName);
			Factory.Save();

			AssertEquals(assigneeOrg.PK, bookingLine.GetRequestSupportedAddressInfo(DocAddressTypes.Codes.BuyerDocumentaryAddress).OrginzationPK);
			AssertEquals(assigneeOrg.Contacts[0].PK, bookingLine.GetRequestSupportedAddressInfo(DocAddressTypes.Codes.BuyerDocumentaryAddress).ContactPK);
			AssertEquals(reviewerOrg.PK, bookingLine.GetRequestSupportedAddressInfo(DocAddressTypes.Codes.SupplierDocumentaryAddress).OrginzationPK);
			AssertEquals(reviewerOrg.Contacts[0].PK, bookingLine.GetRequestSupportedAddressInfo(DocAddressTypes.Codes.SupplierDocumentaryAddress).ContactPK);
			AssertEquals(controllingCustomerOrg.PK, bookingLine.GetRequestSupportedAddressInfo(DocAddressTypes.Codes.ControllingCustomer).OrginzationPK);
			AssertEquals(controllingCustomerOrg.Contacts[0].PK, bookingLine.GetRequestSupportedAddressInfo(DocAddressTypes.Codes.ControllingCustomer).ContactPK);
			AssertEquals(manufactureOrg.PK, bookingLine.GetRequestSupportedAddressInfo(DocAddressTypes.Codes.Manufacturer).OrginzationPK);
			AssertEquals(manufactureOrg.Contacts[0].PK, bookingLine.GetRequestSupportedAddressInfo(DocAddressTypes.Codes.Manufacturer).ContactPK);
		}

		#endregion
	}
}
