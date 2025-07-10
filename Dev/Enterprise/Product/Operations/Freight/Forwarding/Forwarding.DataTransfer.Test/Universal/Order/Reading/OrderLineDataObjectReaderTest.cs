using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Core.Constants;
using Events = Enterprise.ZArchitecture.Business.Events;
using IncoTerm = Enterprise.UniversalDataBuss.DataObjects.Universal.IncoTerm;
using Order = Enterprise.Freight.Forwarding.Orders.Business.Order;
using OrderLine = Enterprise.Freight.Forwarding.Orders.Business.OrderLine;
using UniversalOrderLine = Enterprise.UniversalDataBuss.DataObjects.Universal.OrderLine;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	class OrderLineDataObjectReaderTest : OrganizationAddressTestHelper
	{
		#region Shipment Window Mismatch

		public void TestShipmentWindowMismatchWhileBookingWithoutShipmentWindowDate()
		{
			foreach (var bookingStatus in typeof(SupplierBookingStatus).GetFields().Select(field => field.GetValue(null) as string))
			{
				var isValidStatus = bookingStatus == SupplierBookingStatus.Placed;
				AdvOrmFeatureHelper.RunTestWith(true, action: () =>
				{
					TestShipmentWindowMismatch(bookingStatus + "001", ZDate.Empty, ZDate.Empty, new ZDate(2023, 9, 2), new ZDate(2023, 9, 9), bookingStatus, isValidStatus, isValidStatus);
					TestShipmentWindowMismatch(bookingStatus + "002", new ZDate(2023, 9, 2), new ZDate(2023, 9, 9), new ZDate(2023, 9, 2), new ZDate(2023, 9, 9), bookingStatus, false, false);
					TestShipmentWindowMismatch(bookingStatus + "003", new ZDate(2023, 9, 2), new ZDate(2023, 9, 9), new ZDate(2023, 10, 2), new ZDate(2023, 10, 9), bookingStatus, isValidStatus, isValidStatus);
					TestShipmentWindowMismatch(bookingStatus + "004", new ZDate(2023, 9, 2), new ZDate(2023, 9, 9), ZDate.Empty, ZDate.Empty, bookingStatus, false, false);
				});

				AdvOrmFeatureHelper.RunTestWith(false, action: () =>
				{
					TestShipmentWindowMismatch(bookingStatus + "005", ZDate.Empty, ZDate.Empty, new ZDate(2023, 9, 2), new ZDate(2023, 9, 9), bookingStatus, false, false);
				});
			}
		}

		public void TestShipmentWindowMismatchWhileBookingWithShipmentWindowDate()
		{
			foreach (var bookingStatus in typeof(SupplierBookingStatus).GetFields().Select(field => field.GetValue(null) as string))
			{
				var isValidStatus = bookingStatus == SupplierBookingStatus.Placed;
				AdvOrmFeatureHelper.RunTestWith(true, action: () =>
				{
					TestShipmentWindowMismatch(bookingStatus + "001", new ZDate(2023, 9, 2), new ZDate(2023, 9, 9), new ZDate(2023, 9, 2), new ZDate(2023, 9, 9), bookingStatus, false, false, true);
					TestShipmentWindowMismatch(bookingStatus + "002", new ZDate(2023, 9, 2), new ZDate(2023, 9, 9), new ZDate(2023, 10, 2), new ZDate(2023, 10, 9), bookingStatus, isValidStatus, isValidStatus, true);
					TestShipmentWindowMismatch(bookingStatus + "003", new ZDate(2023, 9, 2), new ZDate(2023, 9, 9), ZDate.Empty, ZDate.Empty, bookingStatus, false, false, true);
				});

				AdvOrmFeatureHelper.RunTestWith(false, action: () =>
				{
					TestShipmentWindowMismatch(bookingStatus + "004", new ZDate(2023, 9, 2), new ZDate(2023, 9, 9), new ZDate(2023, 10, 2), new ZDate(2023, 10, 9), bookingStatus, false, false, true);
				});
			}
		}

		void TestShipmentWindowMismatch(string seq, ZDate originalShipmentWindowStart, ZDate originalShipmentWindowEnd, ZDate shipmentWindowStart, ZDate shipmentWindowEnd, string bookingStatus, bool hasStartEvent, bool hasEndEvent, bool bookingWithShipmentWindowDate = false)
		{
			var data = new UniversalTestData(Factory);
			var buyer = data.ConsigneeOrgCRAHOLSYD;
			buyer.MiscServ.OM_IMAllowAttachedOrderXMLUpdate = true;
			buyer.OH_Code = "BUY" + seq;
			buyer.OH_RL_NKClosestPort = "AUSYD";
			var order = Factory.New<Order>();
			order.BuyerPK = buyer.PK;
			order.JD_OrderNumber = "ORDER ME" + seq;
			order.JD_OrderNumberSplit = new ZByte(2);

			var orderline1 = order.OrderLines.AddNew();
			orderline1.JO_ShipmentWindowStart = originalShipmentWindowStart;
			orderline1.JO_ShipmentWindowEnd = originalShipmentWindowEnd;
			orderline1.FillWithValidTestData();
			Factory.SaveForTesting();

			var booking = Factory.NewWithValidTestData<Orders.Business.JobSupplierBooking>();
			booking.JSB_BookingId = "JSB" + seq;
			booking.JSB_Status = bookingStatus;
			var bookingLine1 = booking.SupplierBookingLines.AddNew();
			bookingLine1.JSL_JO_OrderLine = orderline1.PK;

			if (bookingWithShipmentWindowDate)
			{
				bookingLine1.JSL_ShipmentWindowStart = originalShipmentWindowStart;
				bookingLine1.JSL_ShipmentWindowEnd = originalShipmentWindowEnd;
			}
			Factory.SaveForTesting();

			var logs1 = orderline1.Logs.GetAllLogs();
			AssertEquals(seq + " check shipment window start date before importing UXML", 0, logs1.Where(log => log.SL_SE_NKEvent == Events.ExceptionRaised.Code && log.SL_Reference.EndsWith("TYP=Ship Window Start")).Count());
			AssertEquals(seq + " check shipment window start date before importing UXML", 0, logs1.Where(log => log.SL_SE_NKEvent == Events.ExceptionRaised.Code && log.SL_Reference.EndsWith("TYP=Ship Window End")).Count());

			var logsOnBooking = booking.Logs.GetAllLogs();
			AssertEquals(0, logsOnBooking.Where(log => log.SL_SE_NKEvent == Events.ExceptionRaised.Code && log.SL_Reference.EndsWith("TYP=Ship Window Start")).Count());
			AssertEquals(0, logsOnBooking.Where(log => log.SL_SE_NKEvent == Events.ExceptionRaised.Code && log.SL_Reference.EndsWith("TYP=Ship Window End")).Count());

			var orderLineDataObject = new UniversalOrderLine();
			orderLineDataObject.LineNumber = orderline1.JO_LineNo;
			orderLineDataObject.ShipmentWindowStart = shipmentWindowStart;
			orderLineDataObject.ShipmentWindowEnd = shipmentWindowEnd;
			try
			{
				new OrderLineDataObjectReader(orderLineDataObject, Logger, Factory, order).ReadIntoBusinessObject();
			}
			catch (DataObjectReadFailureException ex)
			{
				AssertEquals($"Cannot update Order because: Order has at least 1 order line attached to supplier booking/s: '{booking.JSB_BookingId}'.", ex.Message);
			}

			Factory.SaveForTesting();

			logs1 = orderline1.Logs.GetAllLogs();
			AssertEquals(seq + " check shipment window start date after importing UXML", hasStartEvent ? 1 : 0, logs1.Where(log => log.SL_SE_NKEvent == Events.ExceptionRaised.Code && log.SL_Reference.Equals($"|RES=This Order Line has a Ship Window Start mismatch with its linked Supplier Booking. Please check and review the linked Supplier Booking ({booking.JSB_BookingId}).|TYP=Ship Window Start")).Count());
			AssertEquals(seq + " check shipment window start date after importing UXML", hasEndEvent ? 1 : 0, logs1.Where(log => log.SL_SE_NKEvent == Events.ExceptionRaised.Code && log.SL_Reference.Equals($"|RES=This Order Line has a Ship Window End mismatch with its linked Supplier Booking. Please check and review the linked Supplier Booking ({booking.JSB_BookingId}).|TYP=Ship Window End")).Count());

			logsOnBooking = booking.Logs.GetAllLogs();
			AssertEquals(hasStartEvent ? 1 : 0, logsOnBooking.Where(log => log.SL_SE_NKEvent == Events.ExceptionRaised.Code && log.SL_Reference.Equals("|RES=This Supplier Booking has booking lines whose Ship Window Start dates do not match with their order line dates.|TYP=Ship Window Start")).Count());
			AssertEquals(hasEndEvent ? 1 : 0, logsOnBooking.Where(log => log.SL_SE_NKEvent == Events.ExceptionRaised.Code && log.SL_Reference.EndsWith("|RES=This Supplier Booking has booking lines whose Ship Window End dates do not match with their order line dates.|TYP=Ship Window End")).Count());
		}

		#endregion

		#region TestBasicOrderLineLevelFieldMappings

		public void TestBasicOrderLineLevelFieldMappings()
		{
			var order = Factory.New<Order>();
			order.BuyerPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			var orderLineDataObject = GetNewOrderLineDataObject();

			Factory.SaveForTesting();

			var reader = new OrderLineDataObjectReader(orderLineDataObject, Logger, Factory, order);
			var orderLine = reader.ReadIntoBusinessObject();

			AssertNotNull(orderLine);

			CombineAssertions(delegate
			{
				AssertContents(orderLine);
				AssertEquals(false, Logger.HasErrors);
				AssertEquals(false, Logger.HasWarnings);
			});
		}

		internal static UniversalOrderLine GetNewOrderLineDataObject()
		{
			var orderLineDataObject = new UniversalOrderLine();
			orderLineDataObject.AdditionalInformation = "Additional Stuff";
			orderLineDataObject.AdditionalTerms = "No terms";
			orderLineDataObject.CommercialInvoiceNumber = "COM1";
			orderLineDataObject.ConfirmationNumber = "CONF123";
			orderLineDataObject.ContainerNumber = "ABC1";
			orderLineDataObject.ContainerPackingOrder = 4;
			orderLineDataObject.CustomsData = new CustomsEntryInfo { CountryOfOrigin = new Country { Code = "NZ" } };
			orderLineDataObject.EarlyShipmentLimitDays = 4;
			orderLineDataObject.ExpectedQuantity = 6.6m;
			orderLineDataObject.ExtendedLinePrice = 110.2m;
			orderLineDataObject.IncoTerm = new IncoTerm { Code = "FOB" };
			orderLineDataObject.InnerPacksQty = 12.3m;
			orderLineDataObject.InnerPacksQtyUnit = new PackageType { Code = "BOX" };
			orderLineDataObject.LateShipmentLimitDays = 1;
			orderLineDataObject.LineNumber = 2;
			orderLineDataObject.LineSplitNumber = new ZShort(3);
			orderLineDataObject.OrderedQty = 14.4m;
			orderLineDataObject.OrderedQtyUnit = new CodeDescriptionPair { Code = "KEG" };
			orderLineDataObject.OverQuantityPercentageLimit = 20.5;
			orderLineDataObject.PackageLength = 20m;
			orderLineDataObject.PackageHeight = 30m;
			orderLineDataObject.PackageWidth = 60m;
			orderLineDataObject.PackageLengthUnit = new UnitOfLength();
			orderLineDataObject.PackageLengthUnit.Code = "CM";
			orderLineDataObject.PackageLengthUnit.Description = Constants.Length.GetDescription("CM", Constants.PluralState.Plural);
			orderLineDataObject.PackageQty = 11.2m;
			orderLineDataObject.PackageQtyUnit = new PackageType { Code = "PLT" };
			orderLineDataObject.PartAttribute1 = "Red";
			orderLineDataObject.PartAttribute2 = "Medium";
			orderLineDataObject.PartAttribute3 = "Great";
			orderLineDataObject.SerialNumber = "SERN";
			orderLineDataObject.Product = new Product { Code = "Guitars", Description = "Electric Guitars" };
			orderLineDataObject.QuantityMet = 5.5m;
			orderLineDataObject.RequiredExWorks = new ZDateTime(2013, 1, 2);
			orderLineDataObject.RequiredInStore = new ZDateTime(2013, 1, 3);
			orderLineDataObject.SupplierConfirmedAcceptance = new ZDateTime(2013, 1, 1);
			orderLineDataObject.SpecialInstructions = "Do Something Special";
			orderLineDataObject.Status = new CodeDescriptionPair { Code = "PLC" };
			orderLineDataObject.SubLineNumber = 5;
			orderLineDataObject.UnderQuantityPercentageLimit = 10.5;
			orderLineDataObject.UnitPriceRecommended = 9.2m;
			orderLineDataObject.VolumeUnit = new UnitOfVolume { Code = "CF" };
			orderLineDataObject.WeightUnit = new UnitOfWeight { Code = "LB" };
			orderLineDataObject.Volume = 4.7m;
			orderLineDataObject.Weight = 18.1m;
			orderLineDataObject.QtyPacked = 12;
			orderLineDataObject.QtyBooked = 20;
			orderLineDataObject.LineReference = "ORL001";
			orderLineDataObject.ShipmentWindowStart = new ZDate(2013, 1, 2);
			orderLineDataObject.ShipmentWindowEnd = new ZDate(2013, 1, 3);
			orderLineDataObject.HarmonisedCode = "HS009";
			orderLineDataObject.Commodity = new Commodity { Code = "CC01" };

			return orderLineDataObject;
		}

		internal static void AssertContents(OrderLine orderLine)
		{
			AssertEquals("orderLine.JO_ActualVolume", 4.7m, orderLine.JO_ActualVolume);
			AssertEquals("orderLine.JO_ActualWeight", 18.1m, orderLine.JO_ActualWeight);
			AssertEquals("orderLine.JO_AdditionalInformation", "Additional Stuff", orderLine.JO_AdditionalInformation);
			AssertEquals("orderLine.JO_AdditionalTerms", "No terms", orderLine.JO_AdditionalTerms);
			AssertEquals("orderLine.JO_CommercialInvoiceNo", "COM1", orderLine.JO_CommercialInvoiceNo);
			AssertEquals("orderLine.JO_ConfirmationDate", new ZDateTime(2013, 1, 1), orderLine.JO_ConfirmationDate);
			AssertEquals("orderLine.JO_ConfirmationNum", "CONF123", orderLine.JO_ConfirmationNum);
			AssertEquals("orderLine.JO_ContainerNumber", "ABC1", orderLine.JO_ContainerNumber);
			AssertEquals("orderLine.JO_ContainerPackingOrder", 4, orderLine.JO_ContainerPackingOrder);
			AssertEquals("orderLine.JO_Description", "Electric Guitars", orderLine.JO_Description);
			AssertEquals("orderLine.JO_EarlyShipmentLimitDays", new ZByte(4), orderLine.JO_EarlyShipmentLimitDays);
			AssertEquals("orderLine.JO_ExWorksDate", new ZDateTime(2013, 1, 2), orderLine.JO_ExWorksDate);
			AssertEquals("orderLine.JO_F3_NKPackType", "KEG", orderLine.JO_F3_NKPackType);
			AssertEquals("orderLine.JO_INCO", "FOB", orderLine.JO_INCO);
			AssertEquals("orderLine.JO_InnerPacks", 12.3m, orderLine.JO_InnerPacks);
			AssertEquals("orderLine.JO_InnerPacksUQ", "BOX", orderLine.JO_InnerPacksUQ);
			AssertEquals("orderLine.JO_ItemPrice", 9.2m, orderLine.JO_ItemPrice);
			AssertEquals("orderLine.JO_LateShipmentLimitDays", new ZByte(1), orderLine.JO_LateShipmentLimitDays);
			AssertEquals("orderLine.JO_LineDropDate", new ZDateTime(2013, 1, 3), orderLine.JO_LineDropDate);
			AssertEquals("orderLine.JO_LineNo", 2, orderLine.JO_LineNo);
			AssertEquals("orderLine.JO_LinePrice", 110.2m, orderLine.JO_LinePrice);
			AssertEquals("orderLine.JO_LineSplitNumber", new ZShort(3), orderLine.JO_LineSplitNumber);
			AssertEquals("orderLine.JO_LineStatus", "PLC", orderLine.JO_LineStatus);
			AssertEquals("orderLine.JO_OuterPacks", 11.2m, orderLine.JO_OuterPacks);
			AssertEquals("orderLine.JO_OuterPacksUQ", "PLT", orderLine.JO_OuterPacksUQ);
			AssertEquals("orderLine.JO_OuterPackLength", 20m, orderLine.JO_OuterPackLength);
			AssertEquals("orderLine.JO_OuterPackHeight", 30m, orderLine.JO_OuterPackHeight);
			AssertEquals("orderLine.JO_OuterPackHeight", 60m, orderLine.JO_OuterPackWidth);
			AssertEquals("orderLine.JO_OuterPackUnitOfDimension", "CM", orderLine.JO_OuterPackUnitOfDimension);
			AssertEquals("orderLine.JO_OverQuantityPercentageLimit", 20.5m, orderLine.JO_OverQuantityPercentageLimit);
			AssertEquals("orderLine.JO_PartAttrib1", "Red", orderLine.JO_PartAttrib1);
			AssertEquals("orderLine.JO_PartAttrib2", "Medium", orderLine.JO_PartAttrib2);
			AssertEquals("orderLine.JO_PartAttrib3", "Great", orderLine.JO_PartAttrib3);
			AssertEquals("orderLine.JO_SerialNumber", "SERN", orderLine.JO_SerialNumber);
			AssertEquals("orderLine.JO_Partno", "GUITARS", orderLine.JO_Partno);
			AssertEquals("orderLine.JO_QtyInvoiced", 6.6m, orderLine.JO_QtyInvoiced);
			AssertEquals("orderLine.JO_QtyReceived", 5.5m, orderLine.JO_QtyReceived);
			AssertEquals("orderLine.JO_Quantity", 14.4m, orderLine.JO_Quantity);
			AssertEquals("orderLine.JO_RN_NKCountryOfOrigin", "NZ", orderLine.JO_RN_NKCountryOfOrigin);
			AssertEquals("orderLine.JO_SpecialInstructions", "Do Something Special", orderLine.JO_SpecialInstructions);
			AssertEquals("orderLine.JO_SubLineNo", 5, orderLine.JO_SubLineNo);
			AssertEquals("orderLine.JO_UnderQuantityPercentageLimit", 10.5m, orderLine.JO_UnderQuantityPercentageLimit);
			AssertEquals("orderLine.JO_UnitOfVolume", "CF", orderLine.JO_UnitOfVolume);
			AssertEquals("orderLine.JO_UnitOfWeight", "LB", orderLine.JO_UnitOfWeight);
			AssertEquals("orderLine.JO_UnitOfWeight", "LB", orderLine.JO_UnitOfWeight);
			AssertEquals("orderLine.JO_QtyPacked", 0m, orderLine.JO_QtyPacked);
			AssertEquals("orderLine.JO_QtyBooked", 0m, orderLine.JO_Quantity - orderLine.JO_OpenQuantity);
			AssertEquals("orderLine.JO_LineReference", "ORL001", orderLine.JO_LineReference);
			AssertEquals("orderLine.ShipmentWindowStart", new ZDate(2013, 1, 2), orderLine.JO_ShipmentWindowStart);
			AssertEquals("orderLine.ShipmentWindowEnd", new ZDate(2013, 1, 3), orderLine.JO_ShipmentWindowEnd);
			AssertEquals("orderLine.HSCode", "HS009", orderLine.JO_HSCode);
			AssertEquals("orderLine.Commodity", "CC01", orderLine.JO_RH_NKCommodityCode);
		}

		#endregion

		#region TestCustomFields

		public void TestCustomFields()
		{
			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			var order = Factory.New<Order>();
			order.BuyerPK = buyer.PK;
			buyer.AddCustomLabel(Constants.CustomLabels.OrderLine.CustomAttribute1, "What Makes You Happy?");
			buyer.AddCustomLabel(Constants.CustomLabels.OrderLine.CustomDate1, "The Date You Are Happy");
			buyer.AddCustomLabel(Constants.CustomLabels.OrderLine.CustomDecimal1, "The Happy Decimal");
			buyer.AddCustomLabel(Constants.CustomLabels.OrderLine.CustomFlag1, "Are you Happy?");
			buyer.AddCustomLabel(Constants.CustomLabels.OrderLine.CustomText1, "Some Happy Blob");
			var orderLineDataObject = new UniversalOrderLine(DefaultDataObjectWriterStrategy.TestInstance);
			orderLineDataObject.SetCustomizedFieldCollection(() => new List<CustomizedField>());
			orderLineDataObject.CustomizedFieldCollection.Add("What Makes You Happy?", new ZString("Lots Of Ice"));
			orderLineDataObject.CustomizedFieldCollection.Add("The Date You Are Happy", ZDateTime.BrettsBirthday);
			orderLineDataObject.CustomizedFieldCollection.Add("The Happy Decimal", new ZDecimal(7.7m));
			orderLineDataObject.CustomizedFieldCollection.Add("Are you Happy?", ZBool.True);
			orderLineDataObject.CustomizedFieldCollection.Add("Some Happy Blob", new ZString("BLOBO"));

			var reader = new OrderLineDataObjectReader(orderLineDataObject, Logger, Factory, order);
			var orderLine = reader.ReadIntoBusinessObject();

			AssertNotNull(orderLine);
			CombineAssertions(delegate
			{
				AssertEquals("orderLine.JO_CustomAttrib1", "Lots Of Ice", orderLine.JO_CustomAttrib1);
				AssertEquals("orderLine.JO_CustomAttrib1", ZDateTime.BrettsBirthday, orderLine.JO_CustomDate1);
				AssertEquals("orderLine.JO_CustomDecimal1", 7.7m, orderLine.JO_CustomDecimal1);
				AssertEquals("orderLine.JO_CustomFlag1", true, orderLine.JO_CustomFlag1);
				AssertEquals("orderLine.JO_CustomTextBlob1", "BLOBO", orderLine.JO_CustomTextBlob1);
				AssertEquals(false, Logger.HasErrors);
				AssertEquals(false, Logger.HasWarnings);
			});
		}

		#endregion

		#region TestDangerousGoods

		public void TestDangerousGoods()
		{
			var order = Factory.New<Order>();
			order.BuyerPK = Factory.NewWithValidTestData<OrgHeader>().PK;

			var contact = order.Buyer.Contacts.AddNew();
			contact.OC_ContactName = "John";
			contact.OC_Phone = "0298983232";

			Factory.SaveForTesting();

			var orderLineDataObject = new UniversalOrderLine(DefaultDataObjectWriterStrategy.TestInstance);
			orderLineDataObject.SetUNDGCollection(() => new List<UNDG> { new UNDG(DefaultDataObjectWriterStrategy.TestInstance) { Contact = new OrganizationContact { FullName = "John", Phone = "0298983232" }, FlashPoint = "-90.1", UNDGCode = "0004A" } });

			var reader = new OrderLineDataObjectReader(orderLineDataObject, Logger, Factory, order);
			var orderLine = reader.ReadIntoBusinessObject();

			AssertNotNull(orderLine);
			var actualSubs = UNDGSubstanceLoader.LoadSubstances(Factory.BOFactory, "0004", "a", "IMO").First();
			AssertEquals("orderLine.UNDGs.UNDGSubstanceManagerGuid.Value", actualSubs.PK, orderLine.UNDGs.UNDGSubstanceManagerGuid.Value);
			AssertEquals("orderLine.UNDGs.UNDGFlashPointManager.Value", "-90.1", orderLine.UNDGs.UNDGFlashPointManager.Value);
			AssertEquals("orderLine.UNDGs.UNDGContactManager.Value", contact.PK, orderLine.UNDGs.UNDGContactManager.Value);
		}

		#endregion

		#region Matching

		public void TestMatchesOrderLineByLineNumberAndSubLineNumber()
		{
			var order = Factory.New<Order>();
			var matchingOrderLine = order.OrderLines.AddNew();
			matchingOrderLine.JO_LineNo = 2;
			matchingOrderLine.JO_SubLineNo = 3;

			var orderLineWithDifferentSubLineNumber = order.OrderLines.AddNew();
			orderLineWithDifferentSubLineNumber.JO_LineNo = 2;
			orderLineWithDifferentSubLineNumber.JO_SubLineNo = 2;

			var orderLineWithDifferentLineNumber = order.OrderLines.AddNew();
			orderLineWithDifferentLineNumber.JO_LineNo = 1;
			orderLineWithDifferentLineNumber.JO_SubLineNo = 3;

			var orderLineDataObject = new UniversalOrderLine();
			orderLineDataObject.LineNumber = 2;
			orderLineDataObject.SubLineNumber = 3;

			var reader = new OrderLineDataObjectReader(orderLineDataObject, Logger, Factory, order);
			var orderLine = reader.ReadIntoBusinessObject();

			AssertNotNull(orderLine);
			AssertEquals("Should match correct Order Line", matchingOrderLine, orderLine);
			AssertEquals(3, order.OrderLines.Count);
			AssertEquals(false, Logger.HasErrors);
			AssertEquals(false, Logger.HasWarnings);
		}

		public void TestMatchesOrderLineByLineNumberAndSubLineOneWhenSubLineNumberNotProvided()
		{
			var order = Factory.New<Order>();
			var matchingOrderLine = order.OrderLines.AddNew();
			matchingOrderLine.JO_LineNo = 2;
			matchingOrderLine.JO_SubLineNo = 1;

			var orderLineWithDifferentSubLineNumber = order.OrderLines.AddNew();
			orderLineWithDifferentSubLineNumber.JO_LineNo = 2;
			orderLineWithDifferentSubLineNumber.JO_SubLineNo = 0;

			var orderLineDataObject = new UniversalOrderLine();
			orderLineDataObject.LineNumber = 2;

			var reader = new OrderLineDataObjectReader(orderLineDataObject, Logger, Factory, order);
			var orderLine = reader.ReadIntoBusinessObject();

			AssertNotNull(orderLine);
			AssertEquals("Should match correct Order Line", matchingOrderLine, orderLine);
			AssertEquals(2, order.OrderLines.Count);
			AssertEquals(false, Logger.HasErrors);
			AssertEquals(false, Logger.HasWarnings);
		}

		public void TestMatchesOrderLineByLineReferenceWhenProvidedAndRegistryEnabled()
		{
			var order = Factory.New<Order>();
			var matchingOrderLine = order.OrderLines.AddNew();
			matchingOrderLine.JO_LineNo = 200;
			matchingOrderLine.JO_SubLineNo = 300;
			matchingOrderLine.JO_LineReference = "ORDER_LINE_1";

			var orderLineDataObject = new UniversalOrderLine();
			orderLineDataObject.LineReference = "ORDER_LINE_1";

			using (OrdersDataRegistry.Instance.EnableOrderLineReferenceMatching.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var reader = new OrderLineDataObjectReader(orderLineDataObject, Logger, Factory, order);
				var orderLine = reader.ReadIntoBusinessObject();

				AssertNotNull(orderLine);
				CombineAssertions("Should match correct Order Line", () =>
				{
					AssertEquals(matchingOrderLine, orderLine);
					AssertEquals(false, Logger.HasErrors);
					AssertEquals(false, Logger.HasWarnings);
				});
			}

			orderLineDataObject.LineReference = "ORDER_LINE_2";
			using (OrdersDataRegistry.Instance.EnableOrderLineReferenceMatching.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var reader = new OrderLineDataObjectReader(orderLineDataObject, Logger, Factory, order);
				var orderLine = reader.ReadIntoBusinessObject();

				AssertNotNull(orderLine);
				CombineAssertions("Should create new Order Line", () =>
				{
					AssertNotEquals(matchingOrderLine, orderLine);
					AssertEquals(false, Logger.HasErrors);
					AssertEquals(false, Logger.HasWarnings);
				});
			}
		}

		public void TestCreatingNewObject_WhenLineReferenceProvidedShouldSetDefaultSubLineNo()
		{
			(var order, var orderLineDataObject) = GenerateTestCreatingNewObject_WhenLineReferenceProvidedShouldSetDefaultSubLineNo_Data();

			using (OrdersDataRegistry.Instance.EnableOrderLineReferenceMatching.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var reader = new OrderLineDataObjectReader(orderLineDataObject, Logger, Factory, order);
				var orderLine = reader.ReadIntoBusinessObject();

				AssertNotNull(orderLine);
				CombineAssertions("Should correctly default JO_LineNo and JO_SubLineNo if creating new, regardless of LineNumber/SubLineNumber", () =>
				{
					AssertEquals("Should default to max + 1", 3, orderLine.JO_LineNo);
					AssertEquals("Should default to 1", 1, orderLine.JO_SubLineNo);
					AssertEquals("Should default to imported", "ORDER_LINE_1", orderLine.JO_LineReference);
					AssertEquals(false, Logger.HasErrors);
					AssertEquals(false, Logger.HasWarnings);
				});
			}

			(order, orderLineDataObject) = GenerateTestCreatingNewObject_WhenLineReferenceProvidedShouldSetDefaultSubLineNo_Data();
			using (OrdersDataRegistry.Instance.EnableOrderLineReferenceMatching.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var reader = new OrderLineDataObjectReader(orderLineDataObject, Logger, Factory, order);
				var orderLine = reader.ReadIntoBusinessObject();

				AssertNotNull(orderLine);
				CombineAssertions("Should use provided LineNumber/SubLineNumber", () =>
				{
					AssertEquals(1, orderLine.JO_LineNo);
					AssertEquals(1, orderLine.JO_SubLineNo);
					AssertEquals(false, Logger.HasErrors);
					AssertEquals(false, Logger.HasWarnings);
				});
			}
		}

		(Order, UniversalOrderLine) GenerateTestCreatingNewObject_WhenLineReferenceProvidedShouldSetDefaultSubLineNo_Data()
		{
			var order = Factory.New<Order>();

			var orderLine1 = order.OrderLines.AddNew();
			orderLine1.JO_LineNo = 1;
			orderLine1.JO_SubLineNo = 1;

			var orderLine2 = order.OrderLines.AddNew();
			orderLine2.JO_LineNo = 1;
			orderLine2.JO_SubLineNo = 2;

			var orderLine3 = order.OrderLines.AddNew();
			orderLine3.JO_LineNo = 2;
			orderLine3.JO_SubLineNo = 1;

			var orderLineDataObject = new UniversalOrderLine();
			orderLineDataObject.LineReference = "ORDER_LINE_1";
			orderLineDataObject.LineNumber = 1;
			orderLineDataObject.SubLineNumber = 1;
			return (order, orderLineDataObject);
		}

		public void TestUpdateMatchedOrderLine_WhenLineReferenceProvidedShouldUpdateLineNoAndSubLineNo()
		{
			var order = Factory.New<Order>();

			var orderLine1 = order.OrderLines.AddNew();
			orderLine1.JO_LineNo = 1;
			orderLine1.JO_SubLineNo = 1;
			orderLine1.JO_LineReference = "ORDER_LINE_1";

			var orderLineDataObject = new UniversalOrderLine();
			orderLineDataObject.LineReference = "ORDER_LINE_1";
			orderLineDataObject.LineNumber = 2;
			orderLineDataObject.SubLineNumber = 2;

			using (OrdersDataRegistry.Instance.EnableOrderLineReferenceMatching.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var reader = new OrderLineDataObjectReader(orderLineDataObject, Logger, Factory, order);
				var orderLine = reader.ReadIntoBusinessObject();

				AssertNotNull(orderLine);
				CombineAssertions("should not update LineNo and SubLineNo while update matched order line through line reference", () =>
				{
					AssertEquals("should match", 1, order.OrderLines.Count);
					AssertEquals("should match the right order line", orderLine.PK, order.OrderLines[0].PK);
					AssertEquals("Should not update LineNo", 1, orderLine.JO_LineNo);
					AssertEquals("Should not update SubLineNo", 1, orderLine.JO_SubLineNo);
					AssertEquals(false, Logger.HasErrors);
					AssertEquals(false, Logger.HasWarnings);
				});
			}
		}

		#endregion

		#region TestManufacturer

		public void TestManufacturer()
		{
			TestOrganizationAddress(DocAddressType.Manufacturer, DocAddressType.Manufacturer);
		}

		#endregion

		#region TestConsignee

		public void TestConsigneeDocumentaryWhileEnableAdvOrmFeature()
		{
			AdvOrmFeatureHelper.RunTestWith(true, action: () =>
			{
				TestOrganizationAddress(DocAddressType.ConsigneeDocumentaryAddress, DocAddressType.ConsigneeDocumentaryAddress);
			});
		}

		public void TestConsigneeDocumentaryWhileDisableSupplierBooking()
		{
			AdvOrmFeatureHelper.RunTestWith(false, action: () =>
			{
				TestOrganizationAddress(DocAddressType.ConsigneeDocumentaryAddress, DocAddressType.ConsigneeDocumentaryAddress, true);
			});
		}

		#endregion

		#region TestGoodsAvailableAt

		public void TestGoodsAvailableAt()
		{
			TestOrganizationAddress(DocAddressType.ConsignorPickupDeliveryAddress, DocAddressType.GoodsAvailableAt);
		}

		#endregion

		#region TestGoodsDeliveredTo

		public void TestGoodsDeliveredTo()
		{
			TestOrganizationAddress(DocAddressType.ConsigneePickupDeliveryAddress, DocAddressType.GoodsDeliveredTo);
		}

		#endregion

		void TestOrganizationAddress(DocAddressType universalAddressType, DocAddressType docAddressType, bool addressIsNull = false)
		{
			var order = Factory.New<Order>();
			order.BuyerPK = Factory.NewWithValidTestData<OrgHeader>().PK;

			Factory.SaveForTesting();

			var addressDataObject = GetNewAddressData_WUFSHIJNB(universalAddressType.ToString());
			var orderLineDataObject = new UniversalOrderLine(DefaultDataObjectWriterStrategy.TestInstance);
			orderLineDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			orderLineDataObject.OrganizationAddressCollection.Add(addressDataObject);

			var reader = new OrderLineDataObjectReader(orderLineDataObject, Logger, Factory, order);
			var orderLine = reader.ReadIntoBusinessObject();

			AssertNotNull(orderLine);

			if (addressIsNull)
			{
				AssertNull(orderLine.DocAddresses.FindByDocAddressType(docAddressType));
			}
			else
			{
				var jobDocAddress = orderLine.DocAddresses.FindByDocAddressType(docAddressType);
				AssertJobDocAddressContentMatches_WUFSHIJNB(jobDocAddress);
				AssertEquals("E2_AddressType", DocAddressTypes.GetCode(Factory.BOFactory, docAddressType), jobDocAddress.E2_AddressType);
			}
		}

		#region Test Warning Logs

		public void TestLogging_WeightValues()
		{
			TestLogging(
				(orderLineDataObject) =>
				{
					orderLineDataObject.Weight = 10;
					orderLineDataObject.WeightUnit = new UnitOfWeight { Code = string.Empty };
				},
				"Order Line Weight Unit is required when Actual Weight is greater than zero.");
		}

		public void TestLogging_VolumeValues()
		{
			TestLogging(
				(orderLineDataObject) =>
				{
					orderLineDataObject.Volume = 10;
					orderLineDataObject.VolumeUnit = new UnitOfVolume { Code = string.Empty };
				},
				"Order Line Volume Unit is required when Actual Volume is greater than zero.");
		}

		public void TestLogging_QuantityUnit_OrderedQuantity()
		{
			TestLogging(
				(orderLineDataObject) =>
				{
					orderLineDataObject.OrderedQtyUnit = new CodeDescriptionPair { Code = string.Empty };
					orderLineDataObject.OrderedQty = 10;
				},
				"Order Line Quantity Unit is required when Quantity Ordered, Quantity Invoiced, or Quantity Received are greater than zero.");
		}

		public void TestLogging_QuantityUnit_ReceivedQuantity()
		{
			TestLogging(
				(orderLineDataObject) =>
				{
					orderLineDataObject.OrderedQtyUnit = new CodeDescriptionPair { Code = string.Empty };
					orderLineDataObject.QuantityMet = 5;
				},
				"Order Line Quantity Unit is required when Quantity Ordered, Quantity Invoiced, or Quantity Received are greater than zero.");
		}

		public void TestLogging_QuantityUnit()
		{
			TestLogging(
				(orderLineDataObject) =>
				{
					orderLineDataObject.OrderedQtyUnit = new CodeDescriptionPair { Code = string.Empty };
					orderLineDataObject.OrderedQty = 10;
				},
				"Order Line Quantity Unit is required when Quantity Ordered, Quantity Invoiced, or Quantity Received are greater than zero.");
		}

		public void TestLogging_QuantityValues()
		{
			TestLogging(
				(orderLineDataObject) =>
				{
					orderLineDataObject.QuantityMet = 15;
					orderLineDataObject.OrderedQty = 10;
				},
				"Order Line Quantity Received is greater than Quantity Ordered.");
		}

		public void TestLogging_InnerPackValues()
		{
			TestLogging(
				(orderLineDataObject) =>
				{
					orderLineDataObject.InnerPacksQtyUnit = new PackageType { Code = string.Empty };
					orderLineDataObject.InnerPacksQty = 10;
				},
				"Order Line Inner Package Type is required when Inner Packs are greater than zero.");
		}

		public void TestLogging_OuterPackValues()
		{
			TestLogging(
				(orderLineDataObject) =>
				{
					orderLineDataObject.PackageQtyUnit = new PackageType { Code = string.Empty };
					orderLineDataObject.PackageQty = 10;
				},
				"Order Line Outer Package Type is required when Outer Packs are greater than zero.");
		}

		public void TestLogging_UnitOfDimension_Height()
		{
			TestLogging(
				(orderLineDataObject) =>
				{
					orderLineDataObject.PackageLengthUnit = new UnitOfLength { Code = string.Empty };
					orderLineDataObject.PackageHeight = 10;
				},
				"Order Line Unit of Dimension is required when Width, Length or Height are greater than zero.");
		}

		public void TestLogging_UnitOfDimension_Width()
		{
			TestLogging(
				(orderLineDataObject) =>
				{
					orderLineDataObject.PackageLengthUnit = new UnitOfLength { Code = string.Empty };
					orderLineDataObject.PackageWidth = 10;
				},
				"Order Line Unit of Dimension is required when Width, Length or Height are greater than zero.");
		}

		public void TestLogging_UnitOfDimension_Length()
		{
			TestLogging(
				(orderLineDataObject) =>
				{
					orderLineDataObject.PackageLengthUnit = new UnitOfLength { Code = string.Empty };
					orderLineDataObject.PackageLength = 10;
				},
				"Order Line Unit of Dimension is required when Width, Length or Height are greater than zero.");
		}

		public void TestLogging_QuantityValues_NoWarinings()
		{
			var order = Factory.New<Order>();
			order.BuyerPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			var orderLineDataObject = GetNewOrderLineDataObject();

			orderLineDataObject.QuantityMet = 9;
			orderLineDataObject.OrderedQty = 10;

			Factory.SaveForTesting();

			new OrderLineDataObjectReader(orderLineDataObject, Logger, Factory, order).ReadIntoBusinessObject();

			Assert("Expecting Logger to have no errors", !Logger.HasErrors);
			Assert("Expecting Logger to have no warnings", !Logger.HasWarnings);
		}

		void TestLogging(Action<UniversalOrderLine> modification, string expectedMessage)
		{
			var order = Factory.New<Order>();
			order.BuyerPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			var orderLineDataObject = GetNewOrderLineDataObject();
			modification(orderLineDataObject);

			Factory.SaveForTesting();

			new OrderLineDataObjectReader(orderLineDataObject, Logger, Factory, order).ReadIntoBusinessObject();

			Assert("Expecting Logger to have no errors", !Logger.HasErrors);
			Assert("Expecting Logger to have warnings", Logger.HasWarnings);

			var warnings = Logger.GetWarnings().Split(System.Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
			AssertEquals("Warning Count: ", 1, warnings.Length);
			AssertEquals("Warning Message: ", expectedMessage, warnings[0]);
		}

		#endregion

		#region Test Error Logs

		public void TestLogging_ShipmentWindowEnd()
		{
			var order = Factory.New<Order>();
			order.BuyerPK = Factory.NewWithValidTestData<OrgHeader>().PK;

			var orderLineDataObject = new UniversalOrderLine();
			orderLineDataObject.ShipmentWindowStart = new ZDate(2013, 1, 2);
			orderLineDataObject.ShipmentWindowEnd = new ZDate(2012, 1, 3);

			Factory.SaveForTesting();

			new OrderLineDataObjectReader(orderLineDataObject, Logger, Factory, order).ReadIntoBusinessObject();

			Assert("Expecting Logger to have errors", Logger.HasErrors);

			var errors = Logger.GetErrors().Split(System.Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
			AssertEquals("Errors Count: ", 1, errors.Length);
			AssertEquals("Errors Message: ", "Order Line Ship Window Start date by must be earlier than or equal to Ship Window End date.", errors[0]);
		}

		public void TestNoLogging_No_ShipmentWindowStart_And_No_ShipmentWindowEnd()
		{
			var order = Factory.New<Order>();
			order.BuyerPK = Factory.NewWithValidTestData<OrgHeader>().PK;

			var orderLineDataObject = new UniversalOrderLine();

			Factory.SaveForTesting();

			new OrderLineDataObjectReader(orderLineDataObject, Logger, Factory, order).ReadIntoBusinessObject();

			Assert("Expecting Logger to have no errors", !Logger.HasErrors);
		}

		public void TestNoLogging_No_ShipmentWindowStart_But_Have_ShipmentWindowEnd()
		{
			var order = Factory.New<Order>();
			order.BuyerPK = Factory.NewWithValidTestData<OrgHeader>().PK;

			var orderLineDataObject = new UniversalOrderLine();
			orderLineDataObject.ShipmentWindowEnd = new ZDate(2012, 1, 3);

			Factory.SaveForTesting();

			new OrderLineDataObjectReader(orderLineDataObject, Logger, Factory, order).ReadIntoBusinessObject();

			Assert("Expecting Logger to have no errors", !Logger.HasErrors);
		}

		public void TestNoLogging_Have_ShipmentWindowStart_But_No_ShipmentWindowEnd()
		{
			var order = Factory.New<Order>();
			order.BuyerPK = Factory.NewWithValidTestData<OrgHeader>().PK;

			var orderLineDataObject = new UniversalOrderLine();
			orderLineDataObject.ShipmentWindowStart = new ZDate(2012, 1, 3);

			Factory.SaveForTesting();

			new OrderLineDataObjectReader(orderLineDataObject, Logger, Factory, order).ReadIntoBusinessObject();

			Assert("Expecting Logger to have no errors", !Logger.HasErrors);
		}

		public void Test_LineReference_WhenEnableOrderLineReferenceMatchingIsOff()
		{
			var order = Factory.New<Order>();
			order.BuyerPK = Factory.NewWithValidTestData<OrgHeader>().PK;

			var orderLineDataObject = new UniversalOrderLine();

			using (OrdersDataRegistry.Instance.EnableOrderLineReferenceMatching.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var allowed = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789@(){}_#-!%^&*\"'<>=;:+./$?|\\[]";
				foreach (var c in allowed)
				{
					var testCase = $"C{c}1";
					orderLineDataObject.LineReference = testCase;

					Factory.SaveForTesting();

					new OrderLineDataObjectReader(orderLineDataObject, Logger, Factory, order).ReadIntoBusinessObject();

					Assert("Expecting Logger to have no errors", !Logger.HasErrors);
				}

				var notAllowed = " ,`~abcdefg";
				var errorMessage = "Must be upper-case and alphanumeric: Space( ), Back-tick(`), Tilde(~) and Comma(,) are not allowed.";
				for (int i = 0; i < notAllowed.Length; i++)
				{
					var c = notAllowed[i];

					var testCase = $"C{c}2";
					orderLineDataObject.LineReference = testCase;

					Factory.SaveForTesting();

					new OrderLineDataObjectReader(orderLineDataObject, Logger, Factory, order).ReadIntoBusinessObject();

					Assert("Expecting Logger to have errors", Logger.HasErrors);

					var errors = Logger.GetErrors().Split(System.Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
					AssertEquals("Errors Count: ", i + 1, errors.Length);
					AssertEquals("Errors Message: ", errorMessage, errors[i]);
				}
			}
		}

		public void Test_LineReference_WhenEnableOrderLineReferenceMatchingIsOn()
		{
			var order = Factory.New<Order>();
			order.BuyerPK = Factory.NewWithValidTestData<OrgHeader>().PK;

			var orderLineDataObject = new UniversalOrderLine();

			using (OrdersDataRegistry.Instance.EnableOrderLineReferenceMatching.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var allowed = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789@(){}_#-!%^&*\"'<>=;:+./$?|\\[]~";
				foreach (var c in allowed)
				{
					var testCase = $"C{c}1";
					orderLineDataObject.LineReference = testCase;

					Factory.SaveForTesting();

					new OrderLineDataObjectReader(orderLineDataObject, Logger, Factory, order).ReadIntoBusinessObject();

					Assert("Expecting Logger to have no errors", !Logger.HasErrors);
				}

				var notAllowed = " ,`hijklmn";
				var errorMessage = "Must be upper-case and alphanumeric: Space( ), Back-tick(`) and Comma(,) are not allowed.";
				for (int i = 0; i < notAllowed.Length; i++)
				{
					var c = notAllowed[i];

					var testCase = $"C{c}2";
					orderLineDataObject.LineReference = testCase;

					Factory.SaveForTesting();

					new OrderLineDataObjectReader(orderLineDataObject, Logger, Factory, order).ReadIntoBusinessObject();

					Assert("Expecting Logger to have errors", Logger.HasErrors);

					var errors = Logger.GetErrors().Split(System.Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
					AssertEquals("Errors Count: ", i + 1, errors.Length);
					AssertEquals("Errors Message: ", errorMessage, errors[i]);
				}
			}
		}

		#endregion
	}
}
