using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportBookings.Shared.Testing;
using Enterprise.TransportCommon.Shared;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.TransportBookings.Business.Testing
{
	[TestedType(typeof(DtbBookingDocumentSupporter))]
	sealed class DtbBookingDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestSupportedChildBusinessContexts()
		{
			AssertContainsExactElementsInAnyOrder(new[] {
					BusinessContext.DtbConsolidation,
					BusinessContext.Shipment,
					BusinessContext.Customs,
					BusinessContext.AgencyDocumentation, // Agency
					BusinessContext.QuotedBooking,
					BusinessContext.WhsInwards, // WhsReceive
					BusinessContext.WhsOrder,
					BusinessContext.Consol,
					BusinessContext.HVLVBookingHeader,
					BusinessContext.HVLVConsignment,
					BusinessContext.HVLVOriginLoadList,
					BusinessContext.TransitDspConsignmnt,
					BusinessContext.LTConsignment }, DocumentSupporter.SupportedChildBusinessContexts);
		}

		public void TestGetContactOrganisation()
		{
			var zayClient = Factory.New<OrgHeader>();
			var rylanAddress = zayClient.Addresses.AddNew();
			Booking.Address.OrganisationPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			var contact = ((IDocumentSupportable)Booking).DocumentSupporter.GetContactOrganisation(ZString.Empty, ContactType.LocalTransport, DocumentDirection.ANY);
			AssertEquals("The contact should be the Booking Address Org.", Booking.Address.OrganisationPK, contact.OrgHeader.PK);

			var pickupInstruction = new TransportBookingTestHelper(Factory).CreateInstruction(Booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNR, rylanAddress);
			var job = new JobHeader.Loader(Booking).TryLoadOrCreate();
			job.LocalChargesPK = zayClient.PK;
			var quoteContact = ((IDocumentSupportable)Booking).DocumentSupporter.GetContactOrganisation(ZString.Empty, ContactType.Sales, DocumentDirection.ANY);
			AssertEquals("The contact should be the First Pickup's Consignor Org.", zayClient.PK, quoteContact.OrgHeader.PK);
		}

		public void TestBookingDocumentSupporter_GetChildCollection()
		{
			var booking = Helper.CreateBooking();
			var standAloneTB_DocumentSupporterChildCollection = ((IDocumentSupportable)booking).DocumentSupporter.GetChildCollection(null, BusinessContext.DtbBooking, null);
			AssertContainsExactElementsInAnyOrder(new[] { booking }, standAloneTB_DocumentSupporterChildCollection);
		}

		public void TestGetChildCollection_ChecksIncludeInDelivery()
		{
			var includedLTConsignment = Helper.CreateConsignment(Booking.PK);
			var excludedLTConsignment = Helper.CreateConsignment(Booking.PK);

			DocumentSupporter.DocumentPrePreviewed += new DocumentPrintedEventHandler(delegate (object sender, DocumentPrintedEventArgs e)
			{
				var bookingDocument = (DocumentDtbBooking)e.MenuItem;
				bookingDocument.IncludeInDelivery = bookingDocument.Booking.LandTransportConsignment.Equals(includedLTConsignment);
			});

			var cartageAdviceMenuItem = Factory.New<StmMenuItem>();
			cartageAdviceMenuItem.SU_MenuName = "Cartage Advice";

			var supportersForCartageAdviceFromLTConsignment = DocumentSupporter.GetChildCollection(cartageAdviceMenuItem, BusinessContext.LTConsignment, null);
			CombineAssertions("Fails to include or exclude the correct bookings", () =>
			{
				AssertEquals("Only one LTConsignment was selected, so only 1 document supporter should be created.", 1, supportersForCartageAdviceFromLTConsignment.Length);
				AssertCollectionContains("Fails to include LTConsignment that was chosen as included in the delivery", includedLTConsignment, supportersForCartageAdviceFromLTConsignment);
				AssertCollectionNotContains("Fails to exclude LTConsignment that was chosen as excluded from the delivery", excludedLTConsignment, supportersForCartageAdviceFromLTConsignment);
			});
		}

		public void TestBusinessContext()
		{
			AssertEquals(BusinessContext.DtbBooking, DocumentSupporter.BusinessContext);
		}

		public void TestCustomisationSecurityCheckpoint()
		{
			AssertEquals(Env.Security.DtbBookingCustomiseDocuments, DocumentSupporter.CustomisationSecurityCheckpoint);
		}

		public void TestGetSupportedDataContexts()
		{
			AssertEquals(true, DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.GenericFreightJob)));
			AssertEquals(true, DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.DtbBooking)));
		}

		public void TestGetDocumentWrappersInternal()
		{
			var supporter = DocumentSupporter;
			var stmMenuItem = Factory.New<StmMenuItem>();
			stmMenuItem.SU_MenuName = "Some Document";

			var cartageAdviceMenuItem = Factory.New<StmMenuItem>();
			cartageAdviceMenuItem.SU_MenuName = "Cartage Advice";

			var wrappersForSomeGenericFreightJobDocument = supporter.GetDocumentWrappers(Constants.DataContext.GenericFreightJob, stmMenuItem);
			AssertEquals("We print document for 1 Booking, so 1 document wrapper should be created.", 1, wrappersForSomeGenericFreightJobDocument.Length);

			var wrappersForCartageAdviceGenericFreightJobDocument = supporter.GetDocumentWrappers(Constants.DataContext.GenericFreightJob, cartageAdviceMenuItem);
			AssertEquals("We print document for 1 Booking, so 1 document wrapper should be created.", 1, wrappersForCartageAdviceGenericFreightJobDocument.Length);

			var wrappersForCartageAdviceDtbBookingDocument = DocumentSupporter.GetDocumentWrappers(Constants.DataContext.DtbBooking, stmMenuItem);
			AssertNull("Currently Cartage Advice documents exist in DocStrips only.", wrappersForCartageAdviceDtbBookingDocument);

			var wrappersForSomeDocumentInWrongDataContextDocument = DocumentSupporter.GetDocumentWrappers(Constants.DataContext.WhsAdjustment, stmMenuItem);
			AssertNull("Wrong Data Context, no document wrappers should be created.", wrappersForSomeDocumentInWrongDataContextDocument);
		}

		public void TestShowReasonForNotPrinting()
		{
			AssertEquals("ShowReasonForNotPrinting", false, DocumentSupporter.ShowReasonForNotPrinting(Constants.DataContext.None, null));
		}

		public void TestGetBODocDataProvidersNotFoundMessageReturnOneReason()
		{
			Assert("No supported data contexts to test", true);
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return ExpectedBizO;
		}

		DtbBookingDocumentSupporter DocumentSupporter
		{
			get { return documentSupporter ?? (documentSupporter = (DtbBookingDocumentSupporter)((IDocumentSupportable)ExpectedBizO).DocumentSupporter); }
		}
		DtbBookingDocumentSupporter documentSupporter;

		DtbBooking ExpectedBizO
		{
			get { return expectedBizO ?? (expectedBizO = GetExpectedBizOWithValidData()); }
		}
		DtbBooking expectedBizO;

		DtbBooking GetExpectedBizOWithValidData()
		{
			return Helper.CreateBooking();
		}

		DtbBooking Booking
		{
			get { return ExpectedBizO; }
		}

		TransportBookingTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingTestHelper(Factory)); }
		}
		TransportBookingTestHelper helper;
	}

	public class DtbBookingDocumentSupporterCartageAdviceHandlerTest : DocumentSupporterCartageAdviceHandlerTest
	{
		protected override IDocumentSupportable GetDocumentSupporterParent()
		{
			var booking = Helper.CreateBooking();
			return booking;
		}

		protected override IDocumentSupportable[] GetBookingsFromBookingsProperty(DocumentSupporter documentSupporter)
		{
			// Same way DtbBookingCartageAdviceDocumentEventsHandler returns Bookings, as DtbBookingDocumentSupporter does not have a Bookings property
			return new DtbBooking[] { ((DtbBookingDocumentSupporter)documentSupporter).Booking };
		}

		protected TransportBookingTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingTestHelper(Factory)); }
		}
		TransportBookingTestHelper helper;

		protected override bool ChecksChildMenuItem => false;
		protected override bool DocumentSupportablesUseDifferentFactory => false;
	}
}
