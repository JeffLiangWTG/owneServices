using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.QuotedBookings.Business.Test
{
	[TestedType(typeof(ViewQuotedBookingForCombineCollection))]
	public class ViewQuotedBookingForCombineCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestOnAddedWarnings_NullChecks()
		{
			var booking = Factory.New<ForwardingShipment>();
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var quotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);
			quotedBooking.ServiceLevel = "";
			var quote1 = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var quotedBooking1 = QuotedBooking.New(quote1.PK, ZGuid.Empty, Factory);
			quotedBooking1.ServiceLevel = "xxx";
			Factory.Save();
			var collection = new ViewQuotedBookingForCombineCollection(quotedBooking);
			var viewQuotedBooking1 = Factory.Load<ViewQuotedBooking>(quotedBooking1.PK);
			AssertNoExceptionThrown(() => collection.Add(viewQuotedBooking1));
		}

		public void TestOnAdded_Errors()
		{
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_IsConsignor = true;
			var consignor2 = Factory.NewWithValidTestData<OrgHeader>();
			consignor2.OH_IsConsignor = true;
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_IsConsignee = true;
			var consignee2 = Factory.NewWithValidTestData<OrgHeader>();
			consignee2.OH_IsConsignee = true;
			var client = Factory.NewWithValidTestData<OrgHeader>();
			var client2 = Factory.NewWithValidTestData<OrgHeader>();
			var controllingCustomer = Factory.NewWithValidTestData<OrgHeader>();
			var controllingCustomer2 = Factory.NewWithValidTestData<OrgHeader>();
			var controllingAgent = Factory.NewWithValidTestData<OrgHeader>();
			var controllingAgent2 = Factory.NewWithValidTestData<OrgHeader>();
			var booking = Factory.New<ForwardingShipment>();
			booking.JS_TransportMode = "AIR";
			booking.JS_RL_NKOrigin = "AUSYD";
			booking.JS_RL_NKDestination = "CNSHA";
			booking.ConsigneePK = consignee.PK;
			booking.ConsignorPK = consignor.PK;
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var quotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);
			quotedBooking.ClientPK = client.PK;
			quotedBooking.ControllingCustomerDocumentaryAddress.OrganisationPK = controllingCustomer.PK;
			quotedBooking.ControllingAgentDocumentaryAddress.OrganisationPK = controllingAgent.PK;
			var booking2 = Factory.New<ForwardingShipment>();
			booking2.JS_TransportMode = "SEA";
			booking2.JS_RL_NKOrigin = "AUBNE";
			booking2.JS_RL_NKDestination = "CNNJG";
			booking2.ConsigneePK = consignee2.PK;
			booking2.ConsignorPK = consignor2.PK;
			var quote2 = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var quotedBooking2 = QuotedBooking.New(quote2.PK, booking2.PK, Factory);
			quotedBooking2.ClientPK = client2.PK;
			quotedBooking2.ControllingCustomerDocumentaryAddress.OrganisationPK = controllingCustomer2.PK;
			quotedBooking2.ControllingAgentDocumentaryAddress.OrganisationPK = controllingAgent2.PK;
			Factory.Save();
			var collection = new ViewQuotedBookingForCombineCollection(quotedBooking);
			var notificationProvider = collection as IFilterModuleExtraNotificationProvider;
			var viewQuotedBooking = Factory.Load<ViewQuotedBooking>(quotedBooking.PK);
			var viewQuotedBooking2 = Factory.Load<ViewQuotedBooking>(quotedBooking2.PK);
			AssertNoRowErrors(viewQuotedBooking);
			AssertNoRowErrors(viewQuotedBooking2);
			collection.Add(viewQuotedBooking);
			collection.Add(viewQuotedBooking2);
			AssertHasRowError(viewQuotedBooking, "Main booking cannot be chosen here. Please choose another booking.");
			AssertHasRowError(viewQuotedBooking2, "This Booking cannot be chosen here. These fields are different from the fields on the Main booking: Origin, Destination, Client, Consignee, Consignor, Transport Mode, Container Mode, Controlling Customer, Controlling Agent. Please choose another booking.");
			booking2.JS_TransportMode = "AIR";
			booking2.JS_RL_NKOrigin = "AUSYD";
			booking2.JS_RL_NKDestination = "CNSHA";
			booking2.ConsigneePK = consignee.PK;
			booking2.ConsignorPK = consignor.PK;
			quotedBooking2.ClientPK = client.PK;
			quotedBooking2.ControllingCustomerDocumentaryAddress.OrganisationPK = controllingCustomer.PK;
			quotedBooking2.ControllingAgentDocumentaryAddress.OrganisationPK = controllingAgent.PK;
			collection.RemoveAll();
			viewQuotedBooking2.ClearAllNotifications();
			collection.Add(viewQuotedBooking2);
			AssertNoRowErrors("All properties are the same, there would be no errors.", viewQuotedBooking2);
		}

		public void TestOnAdded_Errors_OneOffQuotes()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var oneOffQuote = QuotedBooking.New(quote.PK, ZGuid.Empty, Factory);
			var collection = new ViewQuotedBookingForCombineCollection(oneOffQuote);
			var viewQuotedBooking = Factory.Load<ViewQuotedBooking>(oneOffQuote.PK);
			collection.Add(viewQuotedBooking);

			AssertHasRowError(viewQuotedBooking, "One Off Quotes cannot be chosen. Please choose a booking.");
		}

		public void TestWarningOnOtherBookings()
		{
			var pickupOrg = Factory.NewWithValidTestData<OrgHeader>();
			var deliveryOrg = Factory.NewWithValidTestData<OrgHeader>();
			var pickupAgent = Factory.NewWithValidTestData<OrgHeader>();
			var deliveryAgent = Factory.NewWithValidTestData<OrgHeader>();
			var pickupCfs = Factory.NewWithValidTestData<OrgHeader>();
			var deliveryCfs = Factory.NewWithValidTestData<OrgHeader>();
			var exportBroker = Factory.NewWithValidTestData<OrgHeader>();
			var importBroker = Factory.NewWithValidTestData<OrgHeader>();
			var portTransportProvider = Factory.NewWithValidTestData<OrgHeader>();
			var bookingParty = Factory.NewWithValidTestData<OrgHeader>();
			var carrier = Factory.NewWithValidTestData<OrgHeader>();

			var vessel1 = RefVessel.LookupVesselByName("MAJAPAHIT", Factory).First();
			var vessel2 = Factory.NewWithValidTestData<RefVessel>();
			vessel2.RV_Name = "KASERT";

			var voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel1.RV_FK;
			voyage.JV_VoyageFlight = "1234";
			var voyage2 = Factory.New<JobVoyage>();
			voyage2.JV_RV_NKVessel = vessel2.RV_FK;
			voyage2.JV_VoyageFlight = "5555";
			var sailing = GetOrCreateSailing(voyage, "AUSYD", "CNSHA");
			var sailing2 = GetOrCreateSailing(voyage, "AUBNE", "CNNJG");
			var booking = Factory.New<ForwardingShipment>();
			booking.JS_JX = sailing.PK;
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var quotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);
			quotedBooking.PaymentTerms = "FOB";
			quotedBooking.ServiceLevel = "STD";
			quotedBooking.Booking.ConsignorPickupAddress.E2_OA_Address = pickupOrg.MainAddress.PK;
			quotedBooking.Booking.ConsigneeDeliveryAddress.E2_OA_Address = deliveryOrg.MainAddress.PK;
			quotedBooking.ExportReceivingDepot = pickupCfs.PK;
			quotedBooking.ImportReleaseDepot = deliveryCfs.PK;
			quotedBooking.Booking.PickupAgentDocumentaryAddress.E2_OA_Address = pickupAgent.MainAddress.PK;
			quotedBooking.Booking.JS_OH_DeliveryAgent = deliveryAgent.PK;
			quotedBooking.Booking.JS_OH_ExportBroker = exportBroker.PK;
			quotedBooking.Booking.JS_OH_ImportBroker = importBroker.PK;
			quotedBooking.Booking.DocsAndCartage.PickupCartageCoPK = portTransportProvider.PK;
			quotedBooking.Booking.BookingPartyDocumentaryAddress.E2_OA_Address = bookingParty.MainAddress.PK;
			quotedBooking.OH_Carrier = carrier.PK;
			quotedBooking.Booking.JS_RL_NKLoadPort = "AUGOC";
			quotedBooking.Booking.JS_RL_NKDischargePort = "AEJEA";
			quotedBooking.Booking.JS_E_DEP = ZDateTime.Now.AddDays(10);
			quotedBooking.Booking.JS_E_ARV = ZDateTime.Now.AddDays(20);
			var masterBooking = Factory.New<ForwardingShipment>();
			masterBooking.JS_JX = sailing2.PK;
			var masterQuote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var masterQuotedBooking = QuotedBooking.New(masterQuote.PK, masterBooking.PK, Factory);
			masterQuotedBooking.PaymentTerms = "FCA";
			masterQuotedBooking.ServiceLevel = "DIR";
			var viewQuotedBooking = ViewQuotedBooking.LoadOrCreate(quotedBooking);
			var combine = new CombineBookings(masterQuotedBooking);
			combine.OtherViewQuotedBookings.Add(viewQuotedBooking);
			var expectedWarningMessage = "These fields are different from the fields on the Main booking and will be deleted when merged with the Main booking:" + System.Environment.NewLine + "Incoterm, Service Level, Pickup Org., Delivery Org., Pickup Agent, Delivery Agent, Export Broker, Import Broker, Port Transport Provider, Booking Party, Load Port, Discharge Port, Pickup CFS/CTO, Delivery CFS/CTO, Sailing Load Port, Sailing Discharge Port, Carrier, ETD, ETA.";
			AssertHasRowWarning(viewQuotedBooking, expectedWarningMessage);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var booking = Factory.New<ForwardingShipment>();
			var quotedBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);
			Factory.Save();
			return new ViewQuotedBookingForCombineCollection(quotedBooking);
		}

		JobSailing GetOrCreateSailing(JobVoyage voyage, ZString load, ZString discharge)
		{
			if (voyage.Origins.GetOriginFromLoading(load) == null)
			{
				voyage.Origins.AddNew().JA_RL_NKPortOfLoading = load;
			}

			if (voyage.Destinations.GetDestinationFromDischarge(discharge) == null)
			{
				voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = discharge;
			}

			voyage.GenerateSailings();
			return voyage.Sailings.GetSailingFromLoadAndDischarge(load, discharge);
		}
	}
}
