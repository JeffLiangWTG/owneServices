using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportBookings.Shared;
using Enterprise.TransportCommon.Shared;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using static Enterprise.Integration.Customs;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.TransportBookings.Business.Testing
{
	class SqlViewTransportBookingParentsTest : DtbBookingTestCaseWithFactory
	{
		public void TestView_ForwardingShipment()
		{
			var forwardingShipmentIsCancelled = (BusinessObject)Factory.New<IForwardingShipment>();
			forwardingShipmentIsCancelled[JobShipmentSchema.JS_IsCancelled] = true;
			forwardingShipmentIsCancelled.FillWithValidTestData();

			var forwardingShipment = (BusinessObject)Factory.New<IForwardingShipment>();
			forwardingShipment.FillWithValidTestData();

			var booking1 = Helper.CreateBooking();
			booking1.ConsolidationSingleJob.KB_ParentID = forwardingShipmentIsCancelled.PK;
			booking1.ConsolidationSingleJob.KB_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			var booking2 = Helper.CreateBooking();
			booking2.ConsolidationSingleJob.KB_ParentID = forwardingShipment.PK;
			booking2.ConsolidationSingleJob.KB_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			Factory.Save();

			AssertView(1, forwardingShipment.PK, forwardingShipment[JobShipmentSchema.JS_UniqueConsignRef], "SHP",
				forwardingShipment[JobShipmentSchema.JS_RL_NKOrigin], forwardingShipment[JobShipmentSchema.JS_RL_NKDestination]);
		}

		public void TestView_AgencyShipment()
		{
			var agencyShipmentIsCancelled = (BusinessObject)Factory.New<IForwardingShipment>();
			agencyShipmentIsCancelled[JobShipmentSchema.JS_IsShipping] = true;
			agencyShipmentIsCancelled[JobShipmentSchema.JS_IsForwardRegistered] = false;
			agencyShipmentIsCancelled[JobShipmentSchema.JS_IsCancelled] = true;
			agencyShipmentIsCancelled.FillWithValidTestData();

			var agencyShipment = (BusinessObject)Factory.New<IForwardingShipment>();
			agencyShipment[JobShipmentSchema.JS_IsShipping] = true;
			agencyShipment[JobShipmentSchema.JS_IsForwardRegistered] = false;
			agencyShipment.FillWithValidTestData();

			var booking1 = Helper.CreateBooking();
			booking1.ConsolidationSingleJob.KB_ParentID = agencyShipmentIsCancelled.PK;
			booking1.ConsolidationSingleJob.KB_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			var booking2 = Helper.CreateBooking();
			booking2.ConsolidationSingleJob.KB_ParentID = agencyShipment.PK;
			booking2.ConsolidationSingleJob.KB_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			Factory.Save();

			AssertView(1, agencyShipment.PK, agencyShipment[JobShipmentSchema.JS_UniqueConsignRef], "ASH",
				agencyShipment[JobShipmentSchema.JS_RL_NKOrigin], agencyShipment[JobShipmentSchema.JS_RL_NKDestination]);
		}

		public void TestView_JobDeclaration()
		{
			var customsDeclarationIsCancelled = (BusinessObject)Factory.New<IBaseJobDeclaration>();
			customsDeclarationIsCancelled[JobDeclarationSchema.JE_IsCancelled] = true;
			customsDeclarationIsCancelled.FillWithValidTestData();

			var customsDeclaration = (BusinessObject)Factory.New<IBaseJobDeclaration>();
			customsDeclaration.FillWithValidTestData();

			var booking1 = Helper.CreateBooking();
			booking1.ConsolidationSingleJob.KB_ParentID = customsDeclarationIsCancelled.PK;
			booking1.ConsolidationSingleJob.KB_ParentTableCode = JobDeclarationSchema.Constants.Prefix;

			var booking2 = Helper.CreateBooking();
			booking2.ConsolidationSingleJob.KB_ParentID = customsDeclaration.PK;
			booking2.ConsolidationSingleJob.KB_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			Factory.Save();

			AssertView(1, customsDeclaration.PK, customsDeclaration[JobDeclarationSchema.JE_DeclarationReference], "CUS",
				customsDeclaration[JobDeclarationSchema.JE_RL_NKOrigin], customsDeclaration[JobDeclarationSchema.JE_RL_NKFinalDestination]);
		}

		public void TestView_WhsReceive()
		{
			var whsReceive = (BusinessObject)Factory.New<IWhsReceive>();
			whsReceive.FillWithValidTestData();

			var booking = Helper.CreateBooking();
			booking.ConsolidationSingleJob.KB_ParentID = whsReceive.PK;
			booking.ConsolidationSingleJob.KB_ParentTableCode = WhsDocketSchema.Constants.Prefix;
			Factory.Save();

			AssertView(1, whsReceive.PK, whsReceive[WhsDocketSchema.WD_DocketID], "WHR", "", "");
		}

		public void TestView_WhsOrder()
		{
			var whsOrder = (BusinessObject)Factory.New<IWhsOrder>();
			whsOrder.FillWithValidTestData();

			var booking = Helper.CreateBooking();
			booking.ConsolidationSingleJob.KB_ParentID = whsOrder.PK;
			booking.ConsolidationSingleJob.KB_ParentTableCode = WhsDocketSchema.Constants.Prefix;
			Factory.Save();

			AssertView(1, whsOrder.PK, whsOrder[WhsDocketSchema.WD_DocketID], "WHO", "", "");
		}

		public void TestView_QuotedBooking()
		{
			var forwardingShipmentIsCancelled = (BusinessObject)Factory.New<IForwardingShipment>();
			forwardingShipmentIsCancelled[JobShipmentSchema.JS_IsBooking] = true;
			forwardingShipmentIsCancelled[JobShipmentSchema.JS_IsForwardRegistered] = false; // Avoid to unexpected result: VP_JobType = 'SHP'
			forwardingShipmentIsCancelled[JobShipmentSchema.JS_IsCancelled] = true;
			forwardingShipmentIsCancelled.FillWithValidTestData();

			var forwardingShipment = (BusinessObject)Factory.New<IForwardingShipment>();
			forwardingShipment[JobShipmentSchema.JS_IsBooking] = true;
			forwardingShipment[JobShipmentSchema.JS_IsForwardRegistered] = false; // Avoid to unexpected result: VP_JobType = 'SHP'
			forwardingShipment.FillWithValidTestData();

			var builder = ObjectFactory.New<Freight.Integration.QuotedBooking.IQuotedBookingBuilder>();
			// QuotedBooking: Cancelled, Shipment: Not Cancelled
			var quotedBookingIsCancelled = builder.CreateNew(Freight.Integration.QuoteBookingType.BookingWithQuote, Factory);
			quotedBookingIsCancelled.Quote[RatingHeaderSchema.TH_IsCancelled] = true;
			var shipmentNotCancelled = quotedBookingIsCancelled.ForwardingShipment;
			shipmentNotCancelled[JobShipmentSchema.JS_IsBooking] = true;

			// QuotedBooking: Not Cancelled, Shipment: Cancelled
			var quotedBookingShipmentIsCancelled = builder.CreateNew(Freight.Integration.QuoteBookingType.BookingWithQuote, Factory);
			var shipmentIsCancelled = quotedBookingShipmentIsCancelled.ForwardingShipment;
			shipmentIsCancelled[JobShipmentSchema.JS_IsBooking] = true;
			shipmentIsCancelled[JobShipmentSchema.JS_IsCancelled] = true;

			// QuotedBooking: Not Cancelled, Shipment: Not Cancelled
			var quotedBooking = builder.CreateNew(Freight.Integration.QuoteBookingType.BookingWithQuote, Factory);
			var shipmentQuotedBooking = quotedBooking.ForwardingShipment;
			shipmentQuotedBooking[JobShipmentSchema.JS_IsBooking] = true;

			var booking1 = Helper.CreateBooking();
			booking1.ConsolidationSingleJob.KB_ParentID = forwardingShipmentIsCancelled.PK;
			booking1.ConsolidationSingleJob.KB_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			var booking2 = Helper.CreateBooking();
			booking2.ConsolidationSingleJob.KB_ParentID = forwardingShipment.PK;
			booking2.ConsolidationSingleJob.KB_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			var booking3 = Helper.CreateBooking();
			booking3.ConsolidationSingleJob.KB_ParentID = quotedBookingIsCancelled.ViewPK;
			booking3.ConsolidationSingleJob.KB_ParentTableCode = ViewQuotedBookingSchema.Constants.Prefix;

			var booking4 = Helper.CreateBooking();
			booking4.ConsolidationSingleJob.KB_ParentID = quotedBookingShipmentIsCancelled.ViewPK;
			booking4.ConsolidationSingleJob.KB_ParentTableCode = ViewQuotedBookingSchema.Constants.Prefix;

			var booking5 = Helper.CreateBooking();
			booking5.ConsolidationSingleJob.KB_ParentID = quotedBooking.ViewPK;
			booking5.ConsolidationSingleJob.KB_ParentTableCode = ViewQuotedBookingSchema.Constants.Prefix;
			Factory.Save();

			var sql = "select * from dbo.ViewTransportBookingParents";
			var result = new DynamicBusinessObjectCollection(Factory);
			result.Load(sql);

			AssertEquals("Should find 2 parents", 2, result.Count);

			AssertEquals(forwardingShipment.PK, result[0]["VP_PK"]);
			AssertEquals(forwardingShipment[JobShipmentSchema.JS_UniqueConsignRef], result[0]["VP_JobNumber"]);
			AssertEquals("VB", result[0]["VP_JobType"]);
			AssertEquals(forwardingShipment[JobShipmentSchema.JS_RL_NKOrigin], result[0]["VP_RL_NKOrigin"]);
			AssertEquals(forwardingShipment[JobShipmentSchema.JS_RL_NKDestination], result[0]["VP_RL_NKDestination"]);

			AssertEquals(quotedBooking.ViewPK, result[1]["VP_PK"]);
			AssertEquals(quotedBooking.Quote[RatingHeaderSchema.TH_QuoteNumber], result[1]["VP_JobNumber"]);
			AssertEquals("VB", result[1]["VP_JobType"]);
			AssertEquals(shipmentQuotedBooking[JobShipmentSchema.JS_RL_NKOrigin], result[1]["VP_RL_NKOrigin"]);
			AssertEquals(shipmentQuotedBooking[JobShipmentSchema.JS_RL_NKDestination], result[1]["VP_RL_NKDestination"]);
		}

		void AssertView(int expectedCount, object expectedPK, object expectedJobNumber, object expectedJobType,
			object expectedOrigin, object expectedDestination)
		{
			var sql = "select * from dbo.ViewTransportBookingParents";
			var result = new DynamicBusinessObjectCollection(Factory);
			result.Load(sql);

			AssertEquals("Should find 1 parent", expectedCount, result.Count);

			var expectedResult = result[0];
			AssertEquals(expectedPK, expectedResult["VP_PK"]);
			AssertEquals(expectedJobNumber, expectedResult["VP_JobNumber"]);
			AssertEquals(expectedJobType, expectedResult["VP_JobType"]);
			AssertEquals(expectedOrigin, expectedResult["VP_RL_NKOrigin"]);
			AssertEquals(expectedDestination, expectedResult["VP_RL_NKDestination"]);
		}
	}

	class SqlViewTransportBookingPODTest : DtbBookingTestCaseWithFactory
	{
		public void TestView_ForwardingShipment()
		{
			var houseBill = "HouseBill";
			var masterBill = "MasterBill";
			var deliveryAddress = DeliveryAddress.PK;
			var pickupAddress = PickupAddress.PK;
			var forwardingShipment = (BusinessObject)Factory.New<IForwardingShipment>();
			forwardingShipment.FillWithValidTestData();

			AddBookingsForTesting((IDtbBookingParent)forwardingShipment, houseBill, masterBill);
			AssertViewResultRows("SHP", 5, null, houseBill, masterBill);
			AssertViewResultRows("", 5, null, houseBill, masterBill);

			AddBookingsWithAddressForTesting((IDtbBookingParent)forwardingShipment, deliveryAddress, pickupAddress, houseBill, masterBill);
			AssertViewResultRows("SHP", 2, deliveryAddress, houseBill, masterBill);
			AssertViewResultRows("", 2, deliveryAddress, houseBill, masterBill);
		}

		public void TestView_ForwardingShipment_WithConsol()
		{
			var houseBill = "HouseBill";
			var masterBill = "MasterBill";
			var deliveryAddress = DeliveryAddress.PK;
			var pickupAddress = PickupAddress.PK;
			var forwardingShipment = (BusinessObject)Factory.New<IForwardingShipment>();
			forwardingShipment.FillWithValidTestData();

			var consol = (BusinessObject)Factory.New<IForwardingConsol>();
			consol.FillWithValidTestData();
			((IBusinessObjectCollection)forwardingShipment["Consols"]).Add(consol);

			AddBookingsForTesting((IDtbBookingParent)forwardingShipment, houseBill, masterBill);
			AssertViewResultRows("SHP", 5, null, houseBill, masterBill);
			AssertViewResultRows("", 5, null, houseBill, masterBill);

			AddBookingsWithAddressForTesting((IDtbBookingParent)forwardingShipment, deliveryAddress, pickupAddress, houseBill, masterBill);
			AssertViewResultRows("SHP", 2, deliveryAddress, houseBill, masterBill);
			AssertViewResultRows("", 2, deliveryAddress, houseBill, masterBill);
		}

		public void TestView_ForwardingShipment_WithConsolAndRouting()
		{
			var houseBill = "HouseBill";
			var masterBill = "MasterBill";
			var deliveryAddress = DeliveryAddress.PK;
			var pickupAddress = PickupAddress.PK;
			var consol = (BusinessObject)Factory.New<IForwardingConsol>();
			var transport = AddTransportDataToConsol(consol, TransportModes.Sea, "BANOWATI", "001", "AUBNE", "NLAMS");
			var forwardingShipment = (BusinessObject)Factory.New<IForwardingShipment>();
			((IBusinessObjectCollection)forwardingShipment["Consols"]).Add(consol);
			forwardingShipment[JobShipmentSchema.JS_RL_NKOrigin] = "AUBNE";
			forwardingShipment[JobShipmentSchema.JS_RL_NKDestination] = "NLAMS";

			AddBookingsForTesting((IDtbBookingParent)forwardingShipment, houseBill, masterBill);
			AssertViewResultRows("SHP", 5, null, houseBill, masterBill);
			AssertViewResultRows("", 5, null, houseBill, masterBill);

			AddBookingsWithAddressForTesting((IDtbBookingParent)forwardingShipment, deliveryAddress, pickupAddress, houseBill, masterBill);
			AssertViewResultRows("SHP", 2, deliveryAddress, houseBill, masterBill);
			AssertViewResultRows("", 2, deliveryAddress, houseBill, masterBill);
		}

		public void TestView_ForwardingShipment_WithMultipleConsols()
		{
			var houseBill = "HouseBill";
			var masterBill = "MasterBill";
			var deliveryAddress = DeliveryAddress.PK;
			var pickupAddress = PickupAddress.PK;

			var consol1 = (BusinessObject)Factory.New<IForwardingConsol>();
			var transport1 = AddTransportDataToConsol(consol1, TransportModes.Sea, "BANOWATI", "001", "AUBNE", "NLAMS");
			consol1[JobConsolSchema.JK_MasterBillNum] = "Consol1MasterBill";

			var consol2 = (BusinessObject)Factory.New<IForwardingConsol>();
			var transport2 = AddTransportDataToConsol(consol2, TransportModes.Sea, "BANOWATI", "002", "NLAMS", "USLAX");
			consol2[JobConsolSchema.JK_MasterBillNum] = "Consol2MasterBill";

			var forwardingShipment = (BusinessObject)Factory.New<IForwardingShipment>();
			forwardingShipment[JobShipmentSchema.JS_RL_NKOrigin] = "AUBNE";
			forwardingShipment[JobShipmentSchema.JS_RL_NKDestination] = "USLAX";
			forwardingShipment[JobShipmentSchema.JS_HouseBill] = "SHPHouseBill";
			((IBusinessObjectCollection)forwardingShipment["Consols"]).Add(consol1);
			((IBusinessObjectCollection)forwardingShipment["Consols"]).Add(consol2);

			AddBookingsForTesting((IDtbBookingParent)forwardingShipment, houseBill, masterBill);
			AssertViewResultRows("SHP", 5, null, houseBill, masterBill); // Should consider only delivery consol.
			AssertViewResultRows("", 5, null, houseBill, masterBill);

			AddBookingsWithAddressForTesting((IDtbBookingParent)forwardingShipment, deliveryAddress, pickupAddress, houseBill, masterBill);
			AssertViewResultRows("SHP", 2, deliveryAddress, houseBill, masterBill);
			AssertViewResultRows("", 2, deliveryAddress, houseBill, masterBill);
		}

		public void TestView_IgnoresParentLegs_WithNoMatchingSailingInfo()
		{
			var houseBill = "HouseBill";
			var masterBill = "MasterBill";
			var deliveryAddress = DeliveryAddress.PK;
			var pickupAddress = PickupAddress.PK;
			var forwardingShipment = (BusinessObject)Factory.New<IForwardingShipment>();
			forwardingShipment.FillWithValidTestData();

			// Set up two consolidations such that the view table for parent legs would return two rows, except there's no match for sailing information
			var consol1 = (BusinessObject)Factory.New<IForwardingConsol>();
			consol1.FillWithValidTestData();
			var transport1 = AddTransportDataToConsol(consol1, TransportModes.Sea, "BANOWATI", "001", "AUBNE", "NLAMS");
			((IBusinessObjectCollection)forwardingShipment["Consols"]).Add(consol1);

			var consol2 = (BusinessObject)Factory.New<IForwardingConsol>();
			var transport2 = AddTransportDataToConsol(consol2, TransportModes.Sea, "BANOWATI", "001", "AUBNE", "NLAMS");
			((IBusinessObjectCollection)forwardingShipment["Consols"]).Add(consol2);

			AddBookingsForTesting((IDtbBookingParent)forwardingShipment, houseBill, masterBill);
			// Assert that only 5, not 10, results are generated, because the condition of the left join with the view table for parent legs is not met
			AssertViewResultRows("SHP", 5, null, houseBill, masterBill);
			AssertViewResultRows("", 5, null, houseBill, masterBill);

			AddBookingsWithAddressForTesting((IDtbBookingParent)forwardingShipment, deliveryAddress, pickupAddress, houseBill, masterBill);
			AssertViewResultRows("SHP", 2, deliveryAddress, houseBill, masterBill);
			AssertViewResultRows("", 2, deliveryAddress, houseBill, masterBill);
		}

		public void TestView_AgencyShipment()
		{
			var houseBill = "HouseBill";
			var masterBill = "MasterBill";
			var deliveryAddress = DeliveryAddress.PK;
			var pickupAddress = PickupAddress.PK;
			var agencyShipment = (BusinessObject)new BusinessObjectFactory().New<IForwardingShipment>();
			agencyShipment[JobShipmentSchema.JS_IsShipping] = true;
			agencyShipment[JobShipmentSchema.JS_IsForwardRegistered] = false;
			agencyShipment.FillWithValidTestData();
			agencyShipment.Factory.Save();

			AddBookingsForTesting((IDtbBookingParent)agencyShipment, houseBill, masterBill);
			AssertViewResultRows("ASH", 5, null, houseBill, masterBill);
			AssertViewResultRows("", 5, null, houseBill, masterBill);

			AddBookingsWithAddressForTesting((IDtbBookingParent)agencyShipment, deliveryAddress, pickupAddress, houseBill, masterBill);
			AssertViewResultRows("ASH", 2, deliveryAddress, houseBill, masterBill);
			AssertViewResultRows("", 2, deliveryAddress, houseBill, masterBill);
		}

		public void TestView_WhsOrder()
		{
			var houseBill = "HouseBill";
			var masterBill = "MasterBill";
			var deliveryAddress = DeliveryAddress.PK;
			var pickupAddress = PickupAddress.PK;
			var whsOrder = (BusinessObject)Factory.New<IWhsOrder>();
			whsOrder.FillWithValidTestData();

			AddBookingsForTesting((IDtbBookingParent)whsOrder, houseBill, masterBill);
			AssertViewResultRows("WHO", 5, null, houseBill, masterBill);
			AssertViewResultRows("", 5, null, houseBill, masterBill);

			AddBookingsWithAddressForTesting((IDtbBookingParent)whsOrder, deliveryAddress, pickupAddress, houseBill, masterBill);
			AssertViewResultRows("WHO", 2, deliveryAddress, houseBill, masterBill);
			AssertViewResultRows("", 2, deliveryAddress, houseBill, masterBill);
		}

		public void TestView_CustomsDeclaration()
		{
			var houseBill = "HouseBill";
			var masterBill = "MasterBill";
			var deliveryAddress = DeliveryAddress.PK;
			var pickupAddress = PickupAddress.PK;
			var customsDeclaration = (BusinessObject)Factory.New<IBaseJobDeclaration>();
			customsDeclaration.FillWithValidTestData();

			AddBookingsForTesting((IDtbBookingParent)customsDeclaration, houseBill, masterBill);
			AssertViewResultRows("CUS", 5, null, houseBill, masterBill);
			AssertViewResultRows("", 5, null, houseBill, masterBill);

			AddBookingsWithAddressForTesting((IDtbBookingParent)customsDeclaration, deliveryAddress, pickupAddress, houseBill, masterBill);
			AssertViewResultRows("CUS", 2, deliveryAddress, houseBill, masterBill);
			AssertViewResultRows("", 2, deliveryAddress, houseBill, masterBill);
		}

		public void TestView_WhsReceive()
		{
			var houseBill = "HouseBill";
			var masterBill = "MasterBill";
			var deliveryAddress = DeliveryAddress.PK;
			var pickupAddress = PickupAddress.PK;
			var whsReceive = (BusinessObject)Factory.New<IWhsReceive>();
			whsReceive.FillWithValidTestData();

			AddBookingsForTesting((IDtbBookingParent)whsReceive, houseBill, masterBill);
			AssertViewResultRows("WHR", 5, null, houseBill, masterBill);
			AssertViewResultRows("", 5, null, houseBill, masterBill);

			AddBookingsWithAddressForTesting((IDtbBookingParent)whsReceive, deliveryAddress, pickupAddress, houseBill, masterBill);
			AssertViewResultRows("WHR", 2, deliveryAddress, houseBill, masterBill);
			AssertViewResultRows("", 2, deliveryAddress, houseBill, masterBill);
		}

		public void TestView_QuotedBookings()
		{
			var houseBill = "HouseBill";
			var masterBill = "MasterBill";
			var shipmentWithBooking = (BusinessObject)new BusinessObjectFactory().New<IForwardingShipment>();
			shipmentWithBooking[JobShipmentSchema.JS_IsForwardRegistered] = false;
			shipmentWithBooking[JobShipmentSchema.JS_IsBooking] = true;
			shipmentWithBooking.FillWithValidTestData();
			shipmentWithBooking.Factory.Save();

			AddBookingsForTesting((IDtbBookingParent)shipmentWithBooking, houseBill, masterBill);
			AssertViewResultRows("", 5, null, houseBill, masterBill);
		}

		public void TestView_StandaloneBooking()
		{
			var houseBill = "HouseBill";
			var masterBill = "MasterBill";
			var deliveryAddress = DeliveryAddress.PK;
			var pickupAddress = PickupAddress.PK;

			AddBookingsForTesting(null, houseBill, masterBill);
			AssertViewResultRows("NON", 5, null, houseBill, masterBill);
			AssertViewResultRows("", 5, null, houseBill, masterBill);

			AddBookingsWithAddressForTesting(null, deliveryAddress, pickupAddress, houseBill, masterBill);
			AssertViewResultRows("NON", 2, deliveryAddress, houseBill, masterBill);
			AssertViewResultRows("", 2, deliveryAddress, houseBill, masterBill);
		}

		public void TestView_CancelledJob_Shipment()
		{
			var deliveryAddress = DeliveryAddress.PK;
			var pickupAddress = PickupAddress.PK;
			var cancelledShipment = (BusinessObject)new BusinessObjectFactory().New<IForwardingShipment>();
			cancelledShipment[JobShipmentSchema.JS_IsCancelled] = true;
			cancelledShipment.FillWithValidTestData();
			cancelledShipment.Factory.Save();

			AddBookingsForTesting((IDtbBookingParent)cancelledShipment);
			AssertViewResultRows("ASH", 0);
			AssertViewResultRows("", 5); // Functionality must be fixed to show 0 PODs for this case.

			AddBookingsWithAddressForTesting((IDtbBookingParent)cancelledShipment, deliveryAddress, pickupAddress);
			AssertViewResultRows("ASH", 0, deliveryAddress);
			AssertViewResultRows("", 2, deliveryAddress); // Functionality must be fixed to show 0 PODs for this case.
		}

		public void TestView_HouseBillAssociatedWithConsolidation_HouseBillDisplayed()
		{
			var houseBill = "HouseBill";
			var masterBill = "MasterBill";

			AddBookingsWithReferenceNumbersOnConsolidationForTesting(null, houseBill, masterBill);
			AssertViewResultRows("NON", 5, null, houseBill, masterBill);
		}

		void AssertViewResultRows(ZString parentJobTypeFilter, int expectedNumberOfRows, ZGuid? deliveryAddressFilter = null, string expectedHouseBill = "", string expectedMasterBill = "")
		{
			var result = GetViewResultSet(parentJobTypeFilter, deliveryAddressFilter);
			var lookingFor = deliveryAddressFilter != null ? "Delivery Organization" : "Consignee Deliveries (included multi's).";

			CombineAssertions(() =>
			{
				AssertEquals(string.Format("Should find {0} Open PODs - Looking for {1}", expectedNumberOfRows, lookingFor), expectedNumberOfRows, result.Count);
				foreach (DynamicBusinessObject row in result)
				{
					AssertEquals("HouseBill in booking: ", expectedHouseBill, row["HouseBillNumber"]);
					AssertEquals("MasterBill in booking: ", expectedMasterBill, row["MasterBillNumber"]);
				}
			});
		}

		DynamicBusinessObjectCollection GetViewResultSet(string parentJobType, ZGuid? deliveryOrganization)
		{
			var deliveryOrganizationText = deliveryOrganization == null || deliveryOrganization.Value.IsEmpty || !deliveryOrganization.Value.IsValid ? "null" : string.Format("'{0}'", deliveryOrganization.Value.ToString());
			var sql = string.Format(@"
				SELECT * FROM TransportBookingPOD
				(
					--type
					'LCI','', '{0}',
					--dates 
					null, null, null, null, null, null,  
					-- locations
					{1}, null, null, null, '', '', null,
					-- has no
					0, 0, 0, 'ALL'
				)", parentJobType, deliveryOrganizationText);
			var result = new DynamicBusinessObjectCollection(Factory);
			result.Load(sql);

			return result;
		}

		DtbBooking CreateBooking(DtbBookingConsolidation consol, Inst[] instructions, string houseBill = "", string masterBill = "", bool addReferenceNumbersToBooking = true)
		{
			var booking = Helper.CreateBooking(consol);
			if (addReferenceNumbersToBooking)
			{
				booking.AdditionalReferenceNumbers.AddNewIfNotExist(TransportCommonAdditionalReferenceTypes.Codes.HouseBill, houseBill);
				booking.AdditionalReferenceNumbers.AddNewIfNotExist(TransportCommonAdditionalReferenceTypes.Codes.MasterBill, masterBill);
			}
			else
			{
				consol.AdditionalReferenceNumbers.AddNewIfNotExist(TransportCommonAdditionalReferenceTypes.Codes.HouseBill, houseBill);
				consol.AdditionalReferenceNumbers.AddNewIfNotExist(TransportCommonAdditionalReferenceTypes.Codes.MasterBill, masterBill);
			}

			var packageJob = booking.PackageJob;
			var package1 = Helper.CreatePackage("P123", 1, "BOX");
			var package10 = Helper.CreatePackage("", 10, "PLT");
			package1.KP_KJ_ParentPackageJob = packageJob.PK;
			package10.KP_KJ_ParentPackageJob = packageJob.PK;

			foreach (var instructionSetup in instructions)
			{
				var instruction = Helper.CreateInstruction(booking, instructionSetup.DirectionType, instructionSetup.OrgType, Org.MainAddress);
				Helper.CreatePackageDivot(instruction, package1);
				Helper.CreatePackageDivot(instruction, package10, 6);

				foreach (var confirmationType in instructionSetup.ConfirmationTypes)
				{
					if (!instruction.Confirmations.Any(c => c.KK_ConfirmationType == confirmationType))
					{
						var confirmation = Helper.CreateConfirmation(instruction, confirmationType);
					}
				}
			}

			return booking;
		}

		DtbBooking CreateBookingWithAddress(DtbBookingConsolidation consol, Inst[] instructions, ZGuid addressPK, string houseBill = "", string masterBill = "")
		{
			var booking = CreateBooking(consol, instructions, houseBill, masterBill);

			foreach (var instruction in booking.Instructions)
			{
				instruction.Address.E2_OA_Address = addressPK;
				instruction.Address.E2_AddressType = "LCI";
			}
			return booking;
		}

		void AddBookingsForTesting(IDtbBookingParent parent, string houseBill = "", string masterBill = "")
		{
			var consolidation = parent != null ? Helper.CreateConsolidation(parent) : Helper.CreateConsolidation();
			consolidation.KB_JobDirection = nameof(DtbBookingDirection.PIC);
			// create a variety of bookings, whose instructions contain five delivery (including multi) consignees
			var booking_01 = CreateBooking(consolidation, new[] { new Inst("PIC", "CNR"), new Inst("DLV", "CFS") }, houseBill, masterBill);
			var booking_02 = CreateBooking(consolidation, new[] { new Inst("PIC", "CNR"), new Inst("MLT", "CFS"), new Inst("DLV", "CTO") }, houseBill, masterBill);
			var booking_03 = CreateBooking(consolidation, new[] { new Inst("PIC", "CTO"), new Inst("DLV", "CNE") }, houseBill, masterBill);
			var booking_04 = CreateBooking(consolidation, new[] { new Inst("PIC", "CTO"), new Inst("MLT", "CFS"), new Inst("DLV", "CNE") }, houseBill, masterBill);
			var booking_05 = CreateBooking(consolidation, new[] { new Inst("PIC", "CTO"), new Inst("MLT", "CFS"), new Inst("DLV", "CNE"), new Inst("DLV", "CNE") }, houseBill, masterBill);
			var booking_06 = CreateBooking(consolidation, new[] { new Inst("PIC", "CTO"), new Inst("MLT", "CNE"), new Inst("DLV", "CYD") }, houseBill, masterBill);
			var booking_07 = CreateBooking(consolidation, new[] { new Inst("PIC", "CTO"), new Inst("DLV", "CYD") });

			Factory.Save();
		}

		void AddBookingsWithAddressForTesting(IDtbBookingParent parent, ZGuid deliveryAddress, ZGuid pickupAddress, string houseBill = "", string masterBill = "")
		{
			var consolidation = parent != null ? Helper.CreateConsolidation(parent) : Helper.CreateConsolidation();
			consolidation.KB_JobDirection = nameof(DtbBookingDirection.DLV);

			// create a variety of bookings
			var booking_01 = CreateBookingWithAddress(consolidation, new[] { new Inst("PIC", "CNR"), new Inst("DLV", "CNE") }, pickupAddress, houseBill, masterBill);
			var booking_02 = CreateBookingWithAddress(consolidation, new[] { new Inst("PIC", "CNR"), new Inst("MLT", "CFS"), new Inst("DLV", "CTO") }, pickupAddress, houseBill, masterBill);
			var booking_03 = CreateBookingWithAddress(consolidation, new[] { new Inst("PIC", "CTO"), new Inst("DLV", "CNE") }, deliveryAddress, houseBill, masterBill);
			var booking_04 = CreateBookingWithAddress(consolidation, new[] { new Inst("PIC", "CTO"), new Inst("MLT", "CFS"), new Inst("DLV", "CNE") }, pickupAddress, houseBill, masterBill);
			var booking_05 = CreateBookingWithAddress(consolidation, new[] { new Inst("PIC", "CTO"), new Inst("MLT", "CFS"), new Inst("DLV", "CNE"), new Inst("DLV", "CNE") }, pickupAddress, houseBill, masterBill);
			var booking_06 = CreateBookingWithAddress(consolidation, new[] { new Inst("PIC", "CTO"), new Inst("MLT", "CNE"), new Inst("DLV", "CYD") }, pickupAddress, houseBill, masterBill);
			var booking_07 = CreateBookingWithAddress(consolidation, new[] { new Inst("PIC", "CTO"), new Inst("DLV", "CYD") }, deliveryAddress, houseBill, masterBill);

			Factory.Save();
		}

		void AddBookingsWithReferenceNumbersOnConsolidationForTesting(IDtbBookingParent parent, string houseBill, string masterBill)
		{
			var consolidation = parent != null ? Helper.CreateConsolidation(parent) : Helper.CreateConsolidation();

			// create a variety of bookings, whose instructions contain five delivery (including multi) consignees
			var booking_01 = CreateBooking(consolidation, new[] { new Inst("PIC", "CNR"), new Inst("DLV", "CFS") }, houseBill, masterBill, false);
			var booking_02 = CreateBooking(consolidation, new[] { new Inst("PIC", "CNR"), new Inst("MLT", "CFS"), new Inst("DLV", "CTO") }, houseBill, masterBill, false);
			var booking_03 = CreateBooking(consolidation, new[] { new Inst("PIC", "CTO"), new Inst("DLV", "CNE") }, houseBill, masterBill, false);
			var booking_04 = CreateBooking(consolidation, new[] { new Inst("PIC", "CTO"), new Inst("MLT", "CFS"), new Inst("DLV", "CNE") }, houseBill, masterBill, false);
			var booking_05 = CreateBooking(consolidation, new[] { new Inst("PIC", "CTO"), new Inst("MLT", "CFS"), new Inst("DLV", "CNE"), new Inst("DLV", "CNE") }, houseBill, masterBill, false);
			var booking_06 = CreateBooking(consolidation, new[] { new Inst("PIC", "CTO"), new Inst("MLT", "CNE"), new Inst("DLV", "CYD") }, houseBill, masterBill, false);
			var booking_07 = CreateBooking(consolidation, new[] { new Inst("PIC", "CTO"), new Inst("DLV", "CYD") }, addReferenceNumbersToBooking: false);

			Factory.Save();
		}

		BusinessObject AddTransportDataToConsol(BusinessObject consol, string transportMode, string vessel, string voyageFlight, string loadPort, string dischargePort, int transportsIndex = 0)
		{
			var now = ZDateTime.Now;
			var transport = (BusinessObject)((IBusinessObjectCollection)consol["Transports"])[transportsIndex];

			transport[JobConsolTransportSchema.JW_IsLinked] = true;
			transport[JobConsolTransportSchema.JW_TransportMode] = transportMode;
			transport[JobConsolTransportSchema.JW_Vessel] = vessel;
			transport[JobConsolTransportSchema.JW_VoyageFlight] = voyageFlight;
			transport[JobConsolTransportSchema.JW_RL_NKLoadPort] = loadPort;
			transport[JobConsolTransportSchema.JW_ETD] = now.AddDays(-15);
			transport[JobConsolTransportSchema.JW_RL_NKDiscPort] = dischargePort;
			transport[JobConsolTransportSchema.JW_ETA] = now.AddDays(-5);

			return transport;
		}

		struct Inst
		{
			public Inst(ZString direction, ZString orgType)
			{
				DirectionType = direction;
				OrgType = orgType;
				ConfirmationTypes = DirectionType != "MLT" ? new[] { DirectionType } : new ZString[] { "DLV", "PIC" };
			}

			public readonly ZString DirectionType;
			public readonly ZString OrgType;
			public readonly ZString[] ConfirmationTypes;
		}

		OrgAddress PickupAddress
		{
			get
			{
				var organisation = Helper.CreateOrganisation("QWEASD");
				var pickupAddress = Helper.AddAddressToOrganisation(organisation, "Pickup Addy", OrgAddressType.Pickup);
				return pickupAddress;
			}
		}

		OrgAddress DeliveryAddress
		{
			get
			{
				var organisation = Helper.CreateOrganisation("DFTGH");
				var deliveryAddress = Helper.AddAddressToOrganisation(organisation, "Delivery Addy", OrgAddressType.Delivery);
				return deliveryAddress;
			}
		}

		OrgHeader Org
		{
			get { return org ?? (org = Factory.NewWithValidTestData<OrgHeader>()); }
		}
		OrgHeader org;
	}
}
