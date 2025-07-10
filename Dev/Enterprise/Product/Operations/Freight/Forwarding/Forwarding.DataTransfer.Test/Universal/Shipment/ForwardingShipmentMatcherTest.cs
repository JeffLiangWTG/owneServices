using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.Integration;
using Moq;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	class ForwardingShipmentMatcherTest : TestCaseWithFactory
	{
		public void TestBestMatchByEmptyCoLoadBookingConfirmationReference()
		{
			var shipment1 = Factory.New<ForwardingShipment>();
			shipment1.JS_UniqueConsignRef = "S00005001";
			shipment1.JS_IsForwardRegistered = true;

			var shipment2 = Factory.New<ForwardingShipment>();
			shipment2.JS_UniqueConsignRef = "S00005002";
			shipment2.JS_IsForwardRegistered = true;

			var booking3 = Factory.New<ForwardingShipment>();
			booking3.JS_UniqueConsignRef = "S00005003";
			booking3.JS_IsForwardRegistered = false;
			booking3.JS_IsBooking = true;

			Factory.Save();

			var references = new ShipmentReferences();
			references.IsNVOCC = true;
			references.CoLoadBookingConfirmationReference = string.Empty;

			var matcher = new ForwardingShipmentMatcher(Factory, references, new DummyLogger(), new UniversalForwardingHelper());
			var (matchedShipment, reason) = matcher.GetBestMatchWithReason();
			AssertNull(matchedShipment);
			AssertEquals("HIR entry number and booking confirmation reference are empty.", reason);
		}

		public void TestBestMatchByCoLoadBookingConfirmationReference()
		{
			var shipment1 = Factory.New<ForwardingShipment>();
			shipment1.JS_UniqueConsignRef = "S00005001";
			shipment1.JS_IsForwardRegistered = true;

			var shipment2 = Factory.New<ForwardingShipment>();
			shipment2.JS_UniqueConsignRef = "S00005002";
			shipment2.JS_IsForwardRegistered = true;

			var booking3 = Factory.New<ForwardingShipment>();
			booking3.JS_UniqueConsignRef = "S00005003";
			booking3.JS_IsForwardRegistered = false;
			booking3.JS_IsBooking = true;

			var booking4 = Factory.New<ForwardingShipment>();
			booking4.JS_UniqueConsignRef = "S00005004";
			booking4.JS_IsForwardRegistered = false;
			booking4.JS_IsBooking = true;
			booking4.JS_IsCancelled = true;

			Factory.Save();

			var references = new ShipmentReferences();
			references.IsNVOCC = true;
			references.CoLoadBookingConfirmationReference = "S00005002";

			var logger = new DummyLogger();
			var helper = new UniversalForwardingHelper();
			var matcher = new ForwardingShipmentMatcher(Factory, references, logger, helper);
			var (matchedShipment, reason) = matcher.GetBestMatchWithReason();
			AssertEquals(shipment2.PK, matchedShipment.PK);
			AssertEquals(ZString.Empty, reason);

			references.CoLoadBookingConfirmationReference = "S00005003";
			matcher = new ForwardingShipmentMatcher(Factory, references, logger, helper);
			(matchedShipment, reason) = matcher.GetBestMatchWithReason();
			AssertEquals(booking3.PK, matchedShipment.PK);
			AssertEquals(ZString.Empty, reason);

			references.CoLoadBookingConfirmationReference = "S00005004";
			matcher = new ForwardingShipmentMatcher(Factory, references, logger, helper);
			(matchedShipment, reason) = matcher.GetBestMatchWithReason();
			AssertNull(matchedShipment);
			AssertEquals( "No matching forwarding shipment.", reason);

			references.CoLoadBookingConfirmationReference = "S00006000";
			matcher = new ForwardingShipmentMatcher(Factory, references, logger, helper);
			(matchedShipment, reason) = matcher.GetBestMatchWithReason();
			AssertNull(matchedShipment);
			AssertEquals("No matching forwarding shipment.", reason);
		}

		public void TestBestMatchByCoLoadMasterBillNumber()
		{
			var shipment1 = Factory.New<ForwardingShipment>();
			shipment1.JS_HouseBill = "BOOK";
			shipment1.JS_IsForwardRegistered = true;

			var shipment2 = Factory.New<ForwardingShipment>();
			shipment2.JS_HouseBill = "CAR";
			shipment2.JS_IsForwardRegistered = true;

			var booking3 = Factory.New<ForwardingShipment>();
			booking3.JS_HouseBill = "COMPUTER";
			booking3.JS_IsForwardRegistered = false;
			booking3.JS_IsBooking = true;

			var booking4 = Factory.New<ForwardingShipment>();
			booking4.JS_HouseBill = "BIKE";
			booking4.JS_IsForwardRegistered = false;
			booking4.JS_IsBooking = true;

			Factory.Save();

			var references = new ShipmentReferences();
			references.IsNVOCC = true;
			references.CoLoadBookingConfirmationReference = "S00006000";
			references.CoLoadMasterBillNumber = "CAR";

			var logger = new DummyLogger();
			var helper = new UniversalForwardingHelper();
			var matcher = new ForwardingShipmentMatcher(Factory, references, logger, helper);
			var (matchedShipment, reason) = matcher.GetBestMatchWithReason();
			AssertEquals(shipment2.PK, matchedShipment.PK);
			AssertEquals(ZString.Empty, reason);

			booking4.JS_HouseBill = "CAR";
			booking4.JS_UniqueConsignRef = "S00006000";

			Factory.Save();

			matcher = new ForwardingShipmentMatcher(Factory, references, logger, helper);
			(matchedShipment, reason) = matcher.GetBestMatchWithReason();
			AssertEquals(booking4.PK, matchedShipment.PK);
			AssertEquals(ZString.Empty, reason);
		}

		public void TestBestMatchShipmentOtherThanBooking()
		{
			var shipment1 = Factory.New<ForwardingShipment>();
			shipment1.JS_HouseBill = "BOOK";
			shipment1.JS_IsForwardRegistered = true;

			var shipment2 = Factory.New<ForwardingShipment>();
			shipment2.JS_HouseBill = "CAR";
			shipment2.JS_IsForwardRegistered = true;

			var booking3 = Factory.New<ForwardingShipment>();
			booking3.JS_HouseBill = "CAR";
			booking3.JS_IsForwardRegistered = false;
			booking3.JS_IsBooking = true;

			var booking4 = Factory.New<ForwardingShipment>();
			booking4.JS_HouseBill = "BIKE";
			booking4.JS_IsForwardRegistered = false;
			booking4.JS_IsBooking = true;

			Factory.Save();

			var references = new ShipmentReferences();
			references.IsNVOCC = true;
			references.CoLoadBookingConfirmationReference = "S00006000";
			references.CoLoadMasterBillNumber = "CAR";

			var logger = new DummyLogger();
			var helper = new UniversalForwardingHelper();
			var matcher = new ForwardingShipmentMatcher(Factory, references, logger, helper);
			var (matchedShipment, reason) = matcher.GetBestMatchWithReason();
			AssertEquals(shipment2.PK, matchedShipment.PK);
			AssertEquals(ZString.Empty, reason);
		}

		public void TestBestMatchByCoLoadMasterBillNumberAndBookingParty()
		{
			var bookingParty = Factory.New<OrgHeader>();
			bookingParty.MainAddress.OA_Address1 = "Booking Party Address 1";
			bookingParty.OH_Code = "BKGPARTY";
			bookingParty.OH_FullName = "BKG COMPANY PTY LTD";

			var shipment1 = Factory.New<ForwardingShipment>();
			shipment1.JS_UniqueConsignRef = "S00005001";
			shipment1.JS_HouseBill = "CAR";
			shipment1.JS_SystemCreateTimeUtc = new ZDateTime(2020, 2, 3);
			shipment1.BookingPartyDocumentaryAddress.E2_OA_Address = bookingParty.MainAddress.PK;
			shipment1.BookingPartyDocumentaryAddress.E2_AddressOverride = true;
			shipment1.BookingPartyDocumentaryAddress.E2_CompanyName = "BKG COMPANY PTY LTD";

			var shipment2 = Factory.New<ForwardingShipment>();
			shipment2.JS_UniqueConsignRef = "S00005002";
			shipment2.JS_HouseBill = "CAR";
			shipment2.JS_SystemCreateTimeUtc = new ZDateTime(2020, 2, 10);
			shipment2.BookingPartyDocumentaryAddress.E2_OA_Address = bookingParty.MainAddress.PK;
			shipment2.BookingPartyDocumentaryAddress.E2_AddressOverride = true;
			shipment2.BookingPartyDocumentaryAddress.E2_CompanyName = "TEST COMPANY";

			var shipment3 = Factory.New<ForwardingShipment>();
			shipment3.JS_UniqueConsignRef = "S00005003";
			shipment3.JS_HouseBill = "CAR";
			shipment3.JS_SystemCreateTimeUtc = new ZDateTime(2020, 2, 5);

			Factory.Save();

			var references = new ShipmentReferences();
			references.IsNVOCC = true;
			references.CoLoadBookingConfirmationReference = "S00006000";
			references.CoLoadMasterBillNumber = "CAR";
			references.BookingPartyPK = bookingParty.PK;
			references.BookingPartyName = "BKG COMPANY PTY LTD";

			var logger = new DummyLogger();
			var helper = new UniversalForwardingHelper();
			var matcher = new ForwardingShipmentMatcher(Factory, references, logger, helper);
			var (matchedShipment, reason) = matcher.GetBestMatchWithReason();
			AssertEquals("Matched by CoLoadMasterBillNumber and overrided Booking Party Company Name", shipment1.PK, matchedShipment.PK);
			AssertEquals(ZString.Empty, reason);

			shipment3.BookingPartyDocumentaryAddress.E2_OA_Address = bookingParty.MainAddress.PK;
			Factory.Save();
			(matchedShipment, reason) = matcher.GetBestMatchWithReason();
			AssertEquals("Matched by CoLoadMasterBillNumber, Booking Party or overridden Booking Party Company Name (Latest)", shipment3.PK, matchedShipment.PK);
			AssertEquals(ZString.Empty, reason);

			shipment2.BookingPartyDocumentaryAddress.E2_CompanyName = "BKG COMPANY PTY LTD";
			Factory.Save();
			(matchedShipment, reason) = matcher.GetBestMatchWithReason();
			AssertEquals("Matched by CoLoadMasterBillNumber, Booking Party or overridden Booking Party Company Name (Latest)", shipment2.PK, matchedShipment.PK);
			AssertEquals(ZString.Empty, reason);

			references.CoLoadBookingConfirmationReference = "S00005001";
			(matchedShipment, reason) = matcher.GetBestMatchWithReason();
			AssertEquals("Matched by CoLoadBookingConfirmationReference, CoLoadMasterBillNumber, Booking Party or overridden Booking Party Company Name", shipment1.PK, matchedShipment.PK);
			AssertEquals(ZString.Empty, reason);
		}

		public void TestBestMatchByHIR()
		{
			var shipment1 = Factory.New<ForwardingShipment>();
			shipment1.JS_UniqueConsignRef = "S00005001";
			shipment1.JS_IsForwardRegistered = true;
			var hir = shipment1.Numbers.AddNew();
			hir.CE_EntryType = CustomsReferenceNumberType.eHubInterchangeReference.HIR;
			hir.CE_EntryNum = "HIR00001";

			var shipment2 = Factory.New<ForwardingShipment>();
			shipment2.JS_UniqueConsignRef = "S00005002";
			shipment2.JS_IsForwardRegistered = true;

			Factory.Save();

			var references = new ShipmentReferences();
			references.AdditionalReferences = new List<KeyValuePair<ZString, ZString>>();
			references.AdditionalReferences.Add(new KeyValuePair<ZString, ZString>(CustomsReferenceNumberType.eHubInterchangeReference.HIR, "HIR00001"));
			references.CoLoadBookingConfirmationReference = "S00005002";
			references.IsNVOCC = false;

			var matcher = new ForwardingShipmentMatcher(Factory, references, new DummyLogger(), new UniversalForwardingHelper());
			var (matchedShipment, reason) = matcher.GetBestMatchWithReason();
			AssertEquals("shipment1 is the best match since HIR has highest score.", shipment1.PK, matchedShipment.PK);
			AssertEquals(ZString.Empty, reason);

			references.IsNVOCC = true;

			matcher = new ForwardingShipmentMatcher(Factory, references, new DummyLogger(), new UniversalForwardingHelper());
			(matchedShipment, reason) = matcher.GetBestMatchWithReason();
			AssertEquals("shipment1 is the best match since HIR has highest score.", shipment1.PK, matchedShipment.PK);
			AssertEquals(ZString.Empty, reason);
		}

		public void TestBestMatchByShipmentID()
		{
			var shipment1 = Factory.New<ForwardingShipment>();
			shipment1.JS_UniqueConsignRef = "S00005001";

			var shipment2 = Factory.New<ForwardingShipment>();
			shipment2.JS_UniqueConsignRef = "S00005002";

			var shipment3 = Factory.New<ForwardingShipment>();
			shipment3.JS_UniqueConsignRef = "S00005003";

			Factory.Save();

			var references = new ShipmentReferences();
			references.IsNVOCC = true;
			references.ShipmentID = "S00005001";

			var matcher = new ForwardingShipmentMatcher(Factory, references, new DummyLogger(), new UniversalForwardingHelper());
			var (matchedShipment, reason) = matcher.GetBestMatchWithReason();
			AssertEquals(shipment1.PK, matchedShipment.PK);
			AssertEquals(ZString.Empty, reason);
		}

		public void TestBestMatchByHBOLNumber()
		{
			var shipment1 = Factory.New<ForwardingShipment>();
			shipment1.JS_HouseBill = "S0001";

			var shipment2 = Factory.New<ForwardingShipment>();
			shipment2.JS_HouseBill = "S0002";

			var shipment3 = Factory.New<ForwardingShipment>();
			shipment3.JS_HouseBill = "S0003";

			Factory.Save();

			var references = new ShipmentReferences();
			references.IsNVOCC = true;
			references.HBOLNumber = "S0001";

			var matcher = new ForwardingShipmentMatcher(Factory, references, new DummyLogger(), new UniversalForwardingHelper());
			var (matchedShipment, reason) = matcher.GetBestMatchWithReason();
			AssertEquals(shipment1.PK, matchedShipment.PK);
			AssertEquals(ZString.Empty, reason);
		}

		public void TestBestMatchByShipmentIDAndHBOLNumber()
		{
			var shipment1 = Factory.New<ForwardingShipment>();
			shipment1.JS_UniqueConsignRef = "S00005001";
			shipment1.JS_HouseBill = "S0001";

			var shipment2 = Factory.New<ForwardingShipment>();
			shipment2.JS_UniqueConsignRef = "S00005002";
			shipment2.JS_HouseBill = "S0002";

			var shipment3 = Factory.New<ForwardingShipment>();
			shipment3.JS_UniqueConsignRef = "S00005003";
			shipment3.JS_HouseBill = "S0003";

			Factory.Save();

			var references = new ShipmentReferences();
			references.IsNVOCC = true;
			references.ShipmentID = "S00005001";
			references.HBOLNumber = "S0002";

			var matcher = new ForwardingShipmentMatcher(Factory, references, new DummyLogger(), new UniversalForwardingHelper());
			var (matchedShipment, reason) = matcher.GetBestMatchWithReason();
			AssertEquals("Matched by ShipmentID as it has priority over HBOLNumber when SubscriptionType is not specified", shipment1.PK, matchedShipment.PK);
			AssertEquals(ZString.Empty, reason);

			var context = new Mock<IXmlEventValueObject>();
			context.Setup(c => c.Context.SubscriptionType).Returns("CarrierBookingReference");
			matcher = new ForwardingShipmentMatcher(Factory, references, new DummyLogger(), new UniversalForwardingHelper(), context.Object);
			(matchedShipment, reason) = matcher.GetBestMatchWithReason();
			AssertEquals("Matched by ShipmentID as it has priority over HBOLNumber when SubscriptionType is CarrierBookingReference", shipment1.PK, matchedShipment.PK);
			AssertEquals(ZString.Empty, reason);

			context.Setup(c => c.Context.SubscriptionType).Returns("MasterBillNumber");
			matcher = new ForwardingShipmentMatcher(Factory, references, new DummyLogger(), new UniversalForwardingHelper(), context.Object);
			(matchedShipment, reason) = matcher.GetBestMatchWithReason();
			AssertEquals("Matched by HBOLNumber as it has priority over ShipmentID when SubscriptionType is MasterBillNumber", shipment2.PK, matchedShipment.PK);
			AssertEquals(ZString.Empty, reason);
		}

		public void TestGetRepresentativeShipmentMatchingOrderNumber()
		{
			foreach (var uniqueConsignRef in new[]
			{
				"S00005001", "S00005002", "S00005003"
			})
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_UniqueConsignRef = uniqueConsignRef;
				shipment.JS_IsForwardRegistered = true;
				var orderItem1 = shipment.DocsAndCartage.OrderItems.AddNew();
				orderItem1.JT_OrderReference = "123";
			}

			Factory.Save();

			var orderNumbers = new List<ZString>();
			orderNumbers.Add("123");

			var references = new ShipmentReferences();
			references.OrderNumbers = orderNumbers;
			var matcher = new ForwardingShipmentMatcher(Factory, references, new DummyLogger(), new UniversalForwardingHelper());

			var baseShipmentMatcherType = matcher.GetType().BaseType?.BaseType;
			AssertNotNull(baseShipmentMatcherType);
			var matchMethod = baseShipmentMatcherType.GetMethod("GetRepresentativeShipmentMatchingOrderNumbers", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			AssertNotNull(matchMethod);

			var matchedShipment = (ForwardingShipment)matchMethod.Invoke(matcher, new[] { orderNumbers });
			AssertNotNull(matchedShipment);
		}

		public void TestMatchParentByOrderNumber()
		{
			var shipment1 = Factory.New<ForwardingShipment>();
			shipment1.JS_UniqueConsignRef = "S00005001";
			shipment1.JS_BookingReference = "BOOKREF1";
			shipment1.JS_IsForwardRegistered = true;
			var orderItem1 = shipment1.DocsAndCartage.OrderItems.AddNew();
			orderItem1.JT_OrderReference = "123";

			var shipment2 = Factory.New<ForwardingShipment>();
			shipment2.JS_UniqueConsignRef = "S00005002";
			shipment2.JS_BookingReference = "BOOKREF2";
			shipment2.JS_IsForwardRegistered = true;
			var orderItem2 = shipment2.DocsAndCartage.OrderItems.AddNew();
			orderItem2.JT_OrderReference = "123";

			var shipment3 = Factory.New<ForwardingShipment>();
			shipment3.JS_UniqueConsignRef = "S00005003";
			shipment3.JS_IsForwardRegistered = true;
			var orderItem3 = shipment3.DocsAndCartage.OrderItems.AddNew();
			orderItem3.JT_OrderReference = "123";

			var shipment4 = Factory.New<ForwardingShipment>();
			shipment4.JS_UniqueConsignRef = "S00005004";
			shipment4.JS_BookingReference = "BOOKREF1";
			shipment4.JS_IsForwardRegistered = true;
			var orderItem4 = shipment4.DocsAndCartage.OrderItems.AddNew();
			orderItem4.JT_OrderReference = "999";

			Factory.Save();

			var orderNumbers = new List<ZString>();
			orderNumbers.Add("123");

			var references = new ShipmentReferences();
			references.ShippersReference = "BOOKREF1";
			references.OrderNumbers = orderNumbers;

			var newFactory = new BusinessObjectFactory();
			var matcher = new ForwardingShipmentMatcher(newFactory, references, new DummyLogger(), new UniversalForwardingHelper());

			var (matchedShipment, reason) = matcher.GetBestMatchWithReason();
			AssertEquals("shipment1 is the best match since it matches on both Booking Reference and Order Number.", shipment1.PK, matchedShipment.PK);
			AssertEquals(ZString.Empty, reason);

			var loadedShipmentPKs = ((IBusinessObjectFactoryInternals)newFactory).AllBusinessObjects
				.Where(bo => bo is ForwardingShipment)
				.Select(bo => bo.PK);

			AssertCollectionContains("shipment1 has been loaded as it matches on Booking Ref and Order Number", shipment1.PK, loadedShipmentPKs);
			AssertCollectionContains("shipment4 has been loaded as it matches on Booking Ref", shipment4.PK, loadedShipmentPKs);
			Assert("At most 3 shipments have been loaded. One Representative Shipment from shipment1, shipment2, shipment3 matching order number 123, and both Shipments from shipment1, shipment4 matching Booking Ref.", loadedShipmentPKs.Count() <= 3);
		}
	}
}
