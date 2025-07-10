using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.Freight.QuotedBookings.DataTransfer.Universal.Test
{
	class QuotedBookingMatcherTest : TestCaseWithFactory
	{
		public void TestBestMatchByCoLoadBookingConfirmationReference()
		{
			var quotedBooking1 = CreateQuotedBooking();
			quotedBooking1.Booking.JS_UniqueConsignRef = "S00005001";
			var quotedBooking2 = CreateQuotedBooking();
			quotedBooking2.Booking.JS_UniqueConsignRef = "S00005002";
			var quotedBooking3 = CreateQuotedBooking();
			quotedBooking3.Booking.JS_UniqueConsignRef = "S00005003";
			Factory.Save();
			var references = new ShipmentReferences();
			references.IsNVOCC = true;
			references.CoLoadBookingConfirmationReference = "S00005002";
			var matcher = new QuotedBookingMatcher(Factory, references, new DummyLogger(), new UniversalForwardingHelper());

			var (matchedQuotedBooking, reason) = matcher.GetBestMatchWithReason();
			AssertEquals(quotedBooking2.PK, matchedQuotedBooking.PK);
			AssertEquals(ZString.Empty, reason);
		}

		public void TestBestMatchByEmptyCoLoadBookingConfirmationNVOCCReference()
		{
			var quotedBooking1 = CreateQuotedBooking();
			quotedBooking1.Booking.JS_BookingReference = "BOOKS";
			var quotedBooking2 = CreateQuotedBooking();
			quotedBooking2.Booking.JS_UniqueConsignRef = "S00005002";
			var quotedBooking3 = CreateQuotedBooking();
			quotedBooking3.Booking.JS_UniqueConsignRef = "S00005003";
			Factory.Save();
			var references = new ShipmentReferences();
			references.IsNVOCC = true;
			references.CoLoadBookingConfirmationReference = ZString.Empty;
			references.AgentsReference = ZString.Empty;
			var matcher = new QuotedBookingMatcher(Factory, references, new DummyLogger(), new UniversalForwardingHelper());

			var (matchedQuotedBooking, reason) = matcher.GetBestMatchWithReason();
			AssertNull(matchedQuotedBooking);
			AssertEquals("Booking confirmation and agents reference are empty.", reason);
		}

		public void TestBestMatchByEmptyCoLoadBookingConfirmationVGMReference()
		{
			var quotedBooking1 = CreateQuotedBooking();
			quotedBooking1.Booking.JS_BookingReference = "BOOKS";
			var quotedBooking2 = CreateQuotedBooking();
			quotedBooking2.Booking.JS_UniqueConsignRef = "S00005002";
			var quotedBooking3 = CreateQuotedBooking();
			quotedBooking3.Booking.JS_UniqueConsignRef = "S00005003";
			Factory.Save();

			var references = new ShipmentReferences();
			references.IsVGM = true;
			references.CoLoadBookingConfirmationReference = ZString.Empty;
			references.AgentsReference = ZString.Empty;
			var matcher = new QuotedBookingMatcher(Factory, references, new DummyLogger(), new UniversalForwardingHelper());

			var (matchedQuotedBooking, reason) = matcher.GetBestMatchWithReason();
			AssertNull(matchedQuotedBooking);
			AssertEquals("Master bill of lading number and shipper's reference are invalid.", reason);
		}

		public void TestBestMatchNoMatchingResult()
		{
			var quotedBooking1 = CreateQuotedBooking();
			quotedBooking1.Booking.JS_BookingReference = "BOOKS";
			var quotedBooking2 = CreateQuotedBooking();
			quotedBooking2.Booking.JS_BookingReference = "CAR";
			var quotedBooking3 = CreateQuotedBooking();
			quotedBooking3.Booking.JS_BookingReference = "COMPUTER";
			Factory.Save();

			var references = new ShipmentReferences();
			references.IsNVOCC = true;
			references.CoLoadBookingConfirmationReference = "S00005001";
			var matcher = new QuotedBookingMatcher(Factory, references, new DummyLogger(), new UniversalForwardingHelper());

			var (matchedQuotedBooking, reason) = matcher.GetBestMatchWithReason();
			AssertNull(matchedQuotedBooking);
			AssertEquals("No matching quoted booking.", reason);
		}

		public void TestBestMatchConsolidatedShipment()
		{
			var quotedBooking = CreateQuotedBooking();
			quotedBooking.Booking.JS_UniqueConsignRef = "S00005001";
			Factory.Save();

			var bookingView = Factory.LoadTop1<ViewQuotedBooking>(new ZQuery(ViewQuotedBookingSchema.VB_JS, quotedBooking.Booking.PK));
			bookingView.VB_IsConsolidated = true;
			Factory.Save();

			var references = new ShipmentReferences();
			references.IsNVOCC = true;
			references.CoLoadBookingConfirmationReference = "S00005001";

			var context = new Mock<IXmlEventValueObject>();
			context.Setup(c => c.Context.SubscriptionType).Returns("CarrierBookingReference");
			var matcher = new QuotedBookingMatcher(Factory, references, new DummyLogger(), new UniversalForwardingHelper(), context.Object);

			var (matchedQuotedBooking, reason) = matcher.GetBestMatchWithReason();
			AssertNull(matchedQuotedBooking);
			AssertEquals($"Quoted booking ({quotedBooking.Booking.PK}) is consolidated or does not exist.", reason);
		}

		public void TestBestMatchByAgentsReference()
		{
			var quotedBooking1 = CreateQuotedBooking();
			quotedBooking1.Booking.JS_BookingReference = "BOOKS";
			var quotedBooking2 = CreateQuotedBooking();
			quotedBooking2.Booking.JS_BookingReference = "CAR";
			var quotedBooking3 = CreateQuotedBooking();
			quotedBooking3.Booking.JS_BookingReference = "COMPUTER";
			Factory.Save();
			var references = new ShipmentReferences();
			references.IsNVOCC = true;
			references.AgentsReference = "CAR";
			var matcher = new QuotedBookingMatcher(Factory, references, new DummyLogger(), new UniversalForwardingHelper());

			var (matchedQuotedBooking, reason) = matcher.GetBestMatchWithReason();
			AssertEquals(quotedBooking2.PK, matchedQuotedBooking.PK);
			AssertEquals(ZString.Empty, reason);
		}

		public void TestBestMatchByAgentsReferenceAndBookingParty()
		{
			var bookingParty = Factory.New<OrgHeader>();
			bookingParty.MainAddress.OA_Address1 = "Booking Party Address 1";
			bookingParty.OH_Code = "BKGPARTY";
			bookingParty.OH_FullName = "BKG COMPANY PTY LTD";
			var quotedBooking1 = CreateQuotedBooking();
			quotedBooking1.Booking.JS_UniqueConsignRef = "S00005001";
			quotedBooking1.Booking.JS_BookingReference = "CAR";
			quotedBooking1.Booking.JS_SystemCreateTimeUtc = new ZDateTime(2020, 2, 3);
			quotedBooking1.Booking.BookingPartyDocumentaryAddress.E2_OA_Address = bookingParty.MainAddress.PK;
			quotedBooking1.Booking.BookingPartyDocumentaryAddress.E2_AddressOverride = true;
			quotedBooking1.Booking.BookingPartyDocumentaryAddress.E2_CompanyName = "BKG COMPANY PTY LTD";
			var quotedBooking2 = CreateQuotedBooking();
			quotedBooking2.Booking.JS_UniqueConsignRef = "S00005002";
			quotedBooking2.Booking.JS_BookingReference = "CAR";
			quotedBooking2.Booking.JS_SystemCreateTimeUtc = new ZDateTime(2020, 2, 10);
			quotedBooking2.Booking.BookingPartyDocumentaryAddress.E2_OA_Address = bookingParty.MainAddress.PK;
			quotedBooking2.Booking.BookingPartyDocumentaryAddress.E2_AddressOverride = true;
			quotedBooking2.Booking.BookingPartyDocumentaryAddress.E2_CompanyName = "TEST COMPANY";
			var quotedBooking3 = CreateQuotedBooking();
			quotedBooking3.Booking.JS_UniqueConsignRef = "S00005003";
			quotedBooking3.Booking.JS_BookingReference = "CAR";
			quotedBooking3.Booking.JS_SystemCreateTimeUtc = new ZDateTime(2020, 2, 5);
			Factory.Save();

			var references = new ShipmentReferences();
			references.IsNVOCC = true;
			references.AgentsReference = "CAR";
			references.BookingPartyPK = bookingParty.PK;
			references.BookingPartyName = "BKG COMPANY PTY LTD";
			var helper = new UniversalForwardingHelper();
			var matcher = new QuotedBookingMatcher(Factory, references, new DummyLogger(), helper);

			var matchedQuotedBooking = matcher.GetBestMatch();
			AssertEquals("Matched by AgentsReference and overrided Booking Party Company Name", quotedBooking1.PK, matchedQuotedBooking.PK);

			quotedBooking3.Booking.BookingPartyDocumentaryAddress.E2_OA_Address = bookingParty.MainAddress.PK;
			Factory.Save();
			var (matchedQuotedBookingWithReason, reason) = matcher.GetBestMatchWithReason();
			AssertEquals("Matched by AgentsReference, Booking Party or overridden Booking Party Company Name (Latest)", quotedBooking3.PK, matchedQuotedBookingWithReason.PK);
			AssertEquals(ZString.Empty, reason);

			quotedBooking2.Booking.BookingPartyDocumentaryAddress.E2_CompanyName = "BKG COMPANY PTY LTD";
			Factory.Save();
			(matchedQuotedBookingWithReason, reason) = matcher.GetBestMatchWithReason();
			AssertEquals("Matched by AgentsReference, Booking Party or overridden Booking Party Company Name (Latest)", quotedBooking2.PK, matchedQuotedBookingWithReason.PK);
			AssertEquals(ZString.Empty, reason);

			references.CoLoadBookingConfirmationReference = "S00005001";
			(matchedQuotedBookingWithReason, reason) = matcher.GetBestMatchWithReason();
			AssertEquals("Matched by CoLoadBookingConfirmationReference, AgentsReference, Booking Party or overridden Booking Party Company Name", quotedBooking1.PK, matchedQuotedBookingWithReason.PK);
			AssertEquals(ZString.Empty, reason);
		}

		public void TestBestMatchByShipmentID()
		{
			var quotedBooking1 = CreateQuotedBooking();
			quotedBooking1.Booking.JS_UniqueConsignRef = "S00005001";

			var quotedBooking2 = CreateQuotedBooking();
			quotedBooking2.Booking.JS_UniqueConsignRef = "S00005002";

			var quotedBooking3 = CreateQuotedBooking();
			quotedBooking3.Booking.JS_UniqueConsignRef = "S00005003";

			Factory.Save();

			var references = new ShipmentReferences();
			references.IsNVOCC = true;
			references.ShipmentID = "S00005001";

			var matcher = new QuotedBookingMatcher(Factory, references, new DummyLogger(), new UniversalForwardingHelper());
			var (matchedQuotedBooking, reason) = matcher.GetBestMatchWithReason();
			AssertEquals(quotedBooking1.PK, matchedQuotedBooking.PK);
			AssertEquals(ZString.Empty, reason);
		}

		public void TestNoMatchByShipmentID()
		{
			var quotedBooking1 = CreateQuotedBooking();
			quotedBooking1.Booking.JS_UniqueConsignRef = "S00005001";

			var quotedBooking2 = CreateQuotedBooking();
			quotedBooking2.Booking.JS_UniqueConsignRef = "S00005002";

			var quotedBooking3 = CreateQuotedBooking();
			quotedBooking3.Booking.JS_UniqueConsignRef = "S00005003";

			Factory.Save();

			var references = new ShipmentReferences();
			references.IsNVOCC = true;
			references.ShipmentID = "S00005004";

			var matcher = new QuotedBookingMatcher(Factory, references, new DummyLogger(), new UniversalForwardingHelper());
			var (matchedQuotedBooking, reason) = matcher.GetBestMatchWithReason();
			AssertNull(matchedQuotedBooking);
			AssertEquals( "Carrier Booking Number is Invalid.", reason);
		}

		public void TestBestMatchByHBOLNumber()
		{
			var quotedBooking1 = CreateQuotedBooking();
			quotedBooking1.Booking.JS_HouseBill = "S0001";

			var quotedBooking2 = CreateQuotedBooking();
			quotedBooking2.Booking.JS_HouseBill = "S0002";

			var quotedBooking3 = CreateQuotedBooking();
			quotedBooking3.Booking.JS_HouseBill = "S0003";

			Factory.Save();

			var references = new ShipmentReferences();
			references.IsNVOCC = true;
			references.HBOLNumber = "S0001";

			var matcher = new QuotedBookingMatcher(Factory, references, new DummyLogger(), new UniversalForwardingHelper());
			var (matchedQuotedBooking, reason) = matcher.GetBestMatchWithReason();
			AssertEquals(quotedBooking1.PK, matchedQuotedBooking.PK);
			AssertEquals(ZString.Empty, reason);
		}

		public void TestNoMatchByHBOLNumber()
		{
			var quotedBooking1 = CreateQuotedBooking();
			quotedBooking1.Booking.JS_HouseBill = "S0001";

			var quotedBooking2 = CreateQuotedBooking();
			quotedBooking2.Booking.JS_HouseBill = "S0002";

			var quotedBooking3 = CreateQuotedBooking();
			quotedBooking3.Booking.JS_HouseBill = "S0003";

			Factory.Save();

			var references = new ShipmentReferences();
			references.IsNVOCC = true;
			references.HBOLNumber = "S0004";

			var matcher = new QuotedBookingMatcher(Factory, references, new DummyLogger(), new UniversalForwardingHelper());
			var (matchedQuotedBooking, reason) = matcher.GetBestMatchWithReason();
			AssertNull(matchedQuotedBooking);
			AssertEquals("Master Bill Number is Invalid.", reason);
		}

		public void TestBestMatchByShipmentIDAndHBOLNumber()
		{
			var quotedBooking1 = CreateQuotedBooking();
			quotedBooking1.Booking.JS_UniqueConsignRef = "S00005001";
			quotedBooking1.Booking.JS_HouseBill = "S0001";

			var quotedBooking2 = CreateQuotedBooking();
			quotedBooking2.Booking.JS_UniqueConsignRef = "S00005002";
			quotedBooking2.Booking.JS_HouseBill = "S0002";

			var quotedBooking3 = CreateQuotedBooking();
			quotedBooking3.Booking.JS_UniqueConsignRef = "S00005003";
			quotedBooking3.Booking.JS_HouseBill = "S0003";

			Factory.Save();

			var references = new ShipmentReferences();
			references.IsNVOCC = true;
			references.ShipmentID = "S00005001";
			references.HBOLNumber = "S0002";

			var matcher = new QuotedBookingMatcher(Factory, references, new DummyLogger(), new UniversalForwardingHelper());
			var (matchedQuotedBooking, reason) = matcher.GetBestMatchWithReason();
			AssertEquals("Matched by ShipmentID as it has priority over HBOLNumber when SubscriptionType is not specified", quotedBooking1.PK, matchedQuotedBooking.PK);
			AssertEquals(ZString.Empty, reason);

			var context = new Mock<IXmlEventValueObject>();
			context.Setup(c => c.Context.SubscriptionType).Returns("CarrierBookingReference");
			matcher = new QuotedBookingMatcher(Factory, references, new DummyLogger(), new UniversalForwardingHelper(), context.Object);
			(matchedQuotedBooking, reason) = matcher.GetBestMatchWithReason();
			AssertEquals("Matched by ShipmentID as it has priority over HBOLNumber when SubscriptionType is CarrierBookingReference", quotedBooking1.PK, matchedQuotedBooking.PK);
			AssertEquals(ZString.Empty, reason);

			context.Setup(c => c.Context.SubscriptionType).Returns("MasterBillNumber");
			matcher = new QuotedBookingMatcher(Factory, references, new DummyLogger(), new UniversalForwardingHelper(), context.Object);
			(matchedQuotedBooking, reason) = matcher.GetBestMatchWithReason();
			AssertEquals("Matched by HBOLNumber as it has priority over ShipmentID when SubscriptionType is MasterBillNumber", quotedBooking2.PK, matchedQuotedBooking.PK);
			AssertEquals(ZString.Empty, reason);
		}

		public void TestNoMatchByShipmentIDAndHBOLNumber()
		{
			var quotedBooking1 = CreateQuotedBooking();
			quotedBooking1.Booking.JS_UniqueConsignRef = "S00005001";
			quotedBooking1.Booking.JS_HouseBill = "S0001";

			var quotedBooking2 = CreateQuotedBooking();
			quotedBooking2.Booking.JS_UniqueConsignRef = "S00005002";
			quotedBooking2.Booking.JS_HouseBill = "S0002";

			var quotedBooking3 = CreateQuotedBooking();
			quotedBooking3.Booking.JS_UniqueConsignRef = "S00005003";
			quotedBooking3.Booking.JS_HouseBill = "S0003";

			Factory.Save();

			var references = new ShipmentReferences();
			references.IsNVOCC = true;
			references.ShipmentID = "S00005004";
			references.HBOLNumber = "S0004";

			var matcher = new QuotedBookingMatcher(Factory, references, new DummyLogger(), new UniversalForwardingHelper());
			var (matchedQuotedBooking, reason) = matcher.GetBestMatchWithReason();
			AssertNull(matchedQuotedBooking);
			AssertEquals("Master Bill Number and Carrier Booking Number are Invalid.", reason);
		}

		#region Implementation
		QuotedBooking CreateQuotedBooking()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);
			quotedBooking.ClientPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			quotedBooking.Mode = Core.Constants.RateMode.FCL;
			quotedBooking.TryLoadOrCreateJob();
			quotedBooking.Job.JH_GE = Env.CurrentDepartment.PK;
			return quotedBooking;
		}
		#endregion
	}
}
