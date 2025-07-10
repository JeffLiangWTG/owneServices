using System;
using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportBookings.Business.Testing;
using Enterprise.TransportBookings.Shared.Testing;
using Enterprise.TransportCommon.Business.Testing;
using Enterprise.TransportCommon.Shared;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.TransportBookings.Business.Test
{
	[TestedType(typeof(DtbBookingConsolidationDocumentSupporter))]
	public class DtbBookingConsolidationDocumentSupporterTest : DtbBookingTestCaseWithFactory
	{
		public void TestBusinessContext()
		{
			AssertEquals(BusinessContext.DtbConsolidation, DocumentSupporter.BusinessContext);
		}

		public void TestSupportedChildBusinessContexts()
		{
			AssertContainsExactElementsInAnyOrder(new[] {
					BusinessContext.DtbBooking,
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
					BusinessContext.TransitDspConsignmnt
			}, DocumentSupporter.SupportedChildBusinessContexts);
		}

		public void TestCustomisationSecurityCheckpoint()
		{
			AssertEquals(Env.Security.DtbBookingConsolidationCustomiseDocuments, DocumentSupporter.CustomisationSecurityCheckpoint);
		}

		public void TestGetSupportedDataContexts()
		{
			AssertEquals(true, DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.DtbConsolidation)));
			AssertEquals(true, DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.GenericFreightJob)));
		}

		public void TestGetDocumentWrappersInternal()
		{
			DtbBooking booking1 = Helper.CreateBooking(BookingConsolidation);
			DtbBooking booking2 = Helper.CreateBooking(BookingConsolidation);

			var stmMenuItem = Factory.New<StmMenuItem>();
			stmMenuItem.SU_MenuName = "Some Document";

			var cartageAdviceMenuItem = Factory.New<StmMenuItem>();
			cartageAdviceMenuItem.SU_MenuName = "Cartage Advice";

			DocumentWrapper[] wrappersForSomeGenericFreightJobDocument = DocumentSupporter.GetDocumentWrappers(Constants.DataContext.GenericFreightJob, stmMenuItem);
			AssertNotNull("GenericFreightJob", wrappersForSomeGenericFreightJobDocument);
			AssertEquals("GenericFreightJob, so 1 document wrapper should be created.", 1, wrappersForSomeGenericFreightJobDocument.Length);

			DocumentWrapper[] wrappersForCartageAdviceDtbBookingConsolidationDocument = DocumentSupporter.GetDocumentWrappers(Constants.DataContext.DtbConsolidation, stmMenuItem);
			AssertNull("Currently Cartage Advice documents exist in DocStrips only.", wrappersForCartageAdviceDtbBookingConsolidationDocument);

			DocumentWrapper[] wrappersForSomeDocumentInWrongDataContextDocument = DocumentSupporter.GetDocumentWrappers(Constants.DataContext.WhsAdjustment, stmMenuItem);
			AssertNull("Wrong Data Context, no document wrappers should be created.", wrappersForSomeDocumentInWrongDataContextDocument);
		}

		public void TestGetChildCollection()
		{
			var booking1 = Helper.CreateBooking(BookingConsolidation);
			var booking2 = Helper.CreateBooking(BookingConsolidation);
			var deactivatedBooking = Helper.CreateBooking(BookingConsolidation);
			var notify = Notify;
			notify.Response = true;
			deactivatedBooking.Deactivate(notify);

			BookingConsolidation.OnSelectDtbBookingsToPrint += new EventHandler<DtbBookingsToPrintEventArgs>(delegate(object sender, DtbBookingsToPrintEventArgs e)
			{
				foreach (DocumentDtbBooking booking in e.BookingsToSelectFrom)
				{
					booking.IncludeInDelivery = true;
				}
				e.ContinueToPrint = true;
			});

			var cartageAdviceMenuItem = Factory.New<StmMenuItem>();
			cartageAdviceMenuItem.SU_MenuName = "Cartage Advice";

			var supportersForCartageAdviceFromWhsOrder = DocumentSupporter.GetChildCollection(cartageAdviceMenuItem, BusinessContext.WhsOrder, null);
			AssertEquals("There are child collection for DtbBookings only.", 0, supportersForCartageAdviceFromWhsOrder.Length);

			var supportersForCartageAdviceFromDtbBooking = DocumentSupporter.GetChildCollection(cartageAdviceMenuItem, BusinessContext.DtbBooking, null);
			AssertEquals("Booking consolidation consist of 2 active bookings and 1 unactive booking, so only 2 document supporters should be created.", 2, supportersForCartageAdviceFromDtbBooking.Length);
			AssertCollectionContains(booking1, supportersForCartageAdviceFromDtbBooking);
			AssertCollectionContains(booking2, supportersForCartageAdviceFromDtbBooking);
			AssertCollectionNotContains(deactivatedBooking, supportersForCartageAdviceFromDtbBooking);
		}

		public void TestGetChildCollection_ChecksIncludeInDelivery()
		{
			var includedBooking = Helper.CreateBooking(BookingConsolidation);
			var excludedBooking = Helper.CreateBooking(BookingConsolidation);

			BookingConsolidation.OnSelectDtbBookingsToPrint += new EventHandler<DtbBookingsToPrintEventArgs>(delegate(object sender, DtbBookingsToPrintEventArgs e)
			{
				foreach (DocumentDtbBooking booking in e.BookingsToSelectFrom)
				{
					booking.IncludeInDelivery = booking.Booking.PKEquals(includedBooking);
				}
				e.ContinueToPrint = true;
			});

			var cartageAdviceMenuItem = Factory.New<StmMenuItem>();
			cartageAdviceMenuItem.SU_MenuName = "Cartage Advice";

			var supportersForCartageAdviceFromDtbBooking = DocumentSupporter.GetChildCollection(cartageAdviceMenuItem, BusinessContext.DtbBooking, null);
			CombineAssertions("Fails to include or exclude the correct bookings", () =>
			{
				AssertEquals("Only one booking was selected, so only 1 document supporter should be created.", 1, supportersForCartageAdviceFromDtbBooking.Length);
				AssertCollectionContains("Fails to include booking that was chosen as included in the delivery", includedBooking, supportersForCartageAdviceFromDtbBooking);
				AssertCollectionNotContains("Fails to exclude booking that was chosen as excluded from the delivery", excludedBooking, supportersForCartageAdviceFromDtbBooking);
			});
		}

		public void TestGetChildCollection_OnlyOneBooking()
		{
			var booking1 = Helper.CreateBooking(BookingConsolidation);
			var deactivatedBooking = Helper.CreateBooking(BookingConsolidation);
			var notify = Notify;
			notify.Response = true;
			deactivatedBooking.Deactivate(notify);

			BookingConsolidation.OnSelectDtbBookingsToPrint += new EventHandler<DtbBookingsToPrintEventArgs>(delegate(object sender, DtbBookingsToPrintEventArgs e)
			{
				throw new NotSupportedException("Should not pop up grid to choose bookings because there is only 1 active booking.");
			});

			var cartageAdviceMenuItem = Factory.New<StmMenuItem>();
			cartageAdviceMenuItem.SU_MenuName = "Cartage Advice";

			var supportersForCartageAdviceFromWhsOrder = DocumentSupporter.GetChildCollection(cartageAdviceMenuItem, BusinessContext.WhsOrder, null);
			AssertEquals("There are child collection for DtbBookings only.", 0, supportersForCartageAdviceFromWhsOrder.Length);

			var supportersForCartageAdviceFromDtbBooking = DocumentSupporter.GetChildCollection(cartageAdviceMenuItem, BusinessContext.DtbBooking, null);
			AssertEquals("Booking consolidation consists of 1 active booking, so 1 document supporter should be created.", 1, supportersForCartageAdviceFromDtbBooking.Length);
			AssertCollectionContains(booking1, supportersForCartageAdviceFromDtbBooking);
		}

		public void TestGetChildCollection_NoBookings()
		{
			var deactivatedBooking = Helper.CreateBooking(BookingConsolidation);
			var notify = Notify;
			notify.Response = true;
			deactivatedBooking.Deactivate(notify);

			BookingConsolidation.OnSelectDtbBookingsToPrint += new EventHandler<DtbBookingsToPrintEventArgs>(delegate(object sender, DtbBookingsToPrintEventArgs e)
			{
				throw new NotSupportedException("Should not pop up grid to choose bookings because there NONE!");
			});

			var cartageAdviceMenuItem = Factory.New<StmMenuItem>();
			cartageAdviceMenuItem.SU_MenuName = "Cartage Advice";

			var supportersForCartageAdviceFromWhsOrder = DocumentSupporter.GetChildCollection(cartageAdviceMenuItem, BusinessContext.WhsOrder, null);
			AssertEquals("There are child collection for DtbBookings only.", 0, supportersForCartageAdviceFromWhsOrder.Length);

			var supportersForCartageAdviceFromDtbBooking = DocumentSupporter.GetChildCollection(cartageAdviceMenuItem, BusinessContext.DtbBooking, null);
			AssertEquals("Booking consolidation consist of 0 active bookings, so NO document supporters should be created.", 0, supportersForCartageAdviceFromDtbBooking.Length);
		}

		public void TestGetChildCollection_CancelPrint()
		{
			DtbBooking booking1 = Helper.CreateBooking(BookingConsolidation);
			DtbBooking booking2 = Helper.CreateBooking(BookingConsolidation);

			BookingConsolidation.OnSelectDtbBookingsToPrint += new EventHandler<DtbBookingsToPrintEventArgs>(delegate(object sender, DtbBookingsToPrintEventArgs e)
			{
				foreach (DocumentDtbBooking booking in e.BookingsToSelectFrom)
				{
					booking.IncludeInDelivery = true;
				}
				e.ContinueToPrint = false;
			});

			var cartageAdviceMenuItem = Factory.New<StmMenuItem>();
			cartageAdviceMenuItem.SU_MenuName = "Cartage Advice";

			IDocumentSupportable[] supportersForCartageAdviceFromDtbBooking = DocumentSupporter.GetChildCollection(cartageAdviceMenuItem, BusinessContext.DtbBooking, null);
			AssertEquals("Booking consolidation consist of 0 bookings, so NO document supporters should be created.", 0, supportersForCartageAdviceFromDtbBooking.Length);
		}

		public void TestGetContactOrganisation()
		{
			var booking1 = Helper.CreateBooking(BookingConsolidation);
			var zayClient = Factory.New<OrgHeader>();
			var rylanAddress = zayClient.Addresses.AddNew();
			booking1.Address.OrganisationPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			var contact = ((IDocumentSupportable)BookingConsolidation).DocumentSupporter.GetContactOrganisation(ZString.Empty, ContactType.LocalTransport, DocumentDirection.ANY);
			AssertEquals("The contact should be the Booking Address Org.", booking1.Address.OrganisationPK, contact.OrgHeader.PK);

			var pickupInstruction = new TransportBookingTestHelper(Factory).CreateInstruction(booking1, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNR, rylanAddress);
			var job = new JobHeader.Loader(booking1).TryLoadOrCreate();
			job.LocalChargesPK = zayClient.PK;
			var quoteContact = ((IDocumentSupportable)BookingConsolidation).DocumentSupporter.GetContactOrganisation(ZString.Empty, ContactType.Sales, DocumentDirection.ANY);
			AssertEquals("The contact should be the First Pickup's Consignor Org.", zayClient.PK, quoteContact.OrgHeader.PK);
		}

		public void TestShowReasonForNotPrinting()
		{
			AssertEquals("ShowReasonForNotPrinting", false, DocumentSupporter.ShowReasonForNotPrinting(Constants.DataContext.None, null));
		}

		DtbBookingConsolidation BookingConsolidation
		{
			get { return bookingConsolidation ?? (bookingConsolidation = Helper.CreateConsolidation()); }
		}

		DtbBookingConsolidationDocumentSupporter DocumentSupporter
		{
			get { return documentSupporter ?? (documentSupporter = (DtbBookingConsolidationDocumentSupporter)((IDocumentSupportable)BookingConsolidation).DocumentSupporter); }
		}

		DtbBookingConsolidation bookingConsolidation;
		DtbBookingConsolidationDocumentSupporter documentSupporter;
	}

	public class DtbBookingConsolidationDocumentSupporterCartageAdviceHandlerTest : DocumentSupporterCartageAdviceHandlerTest
	{
		[TestDate(2017, 7, 1)]
		public void TestSetKM_BookingOfTransportRequestedDate_OnlyOnSelectedTransportBookings()
		{
			var includedBooking = Helper.CreateBooking(BookingConsolidation);
			var excludedBooking = Helper.CreateBooking(BookingConsolidation);
			Assert("Precondition: KM_BookingOfTransportRequestedDate is not set", includedBooking.KM_BookingOfTransportRequestedDate.IsEmpty);

			BookingConsolidation.OnSelectDtbBookingsToPrint += new EventHandler<DtbBookingsToPrintEventArgs>(delegate(object sender, DtbBookingsToPrintEventArgs e)
			{
				foreach (DocumentDtbBooking booking in e.BookingsToSelectFrom)
				{
					booking.IncludeInDelivery = booking.Booking.PKEquals(includedBooking);
				}
				e.ContinueToPrint = true;
			});

			var menuItem = SetupMenuItemWithChildMenuItem(Constants.RefDocTypes.CartageAdvice);
			var eventArgs = new DocumentPrintedEventArgs(DeliveryInstructionDestination.Print, menuItem, false);
			var events = new IDocumentEventsMock();
			var supporter = ((IDocumentSupportable)BookingConsolidation).DocumentSupporter;
			supporter.Initialise(events);
			supporter.GetChildCollection(menuItem, BusinessContext.DtbBooking, null);
			events.NotifyDocumentPrePrinted(eventArgs);

			CombineAssertions("Sets KM_BookingOfTransportRequestedDate on bookings that weren't selected, or vice versa", () =>
			{
				AssertEquals("KM_BookingOfTransportRequestedDate should be set on the booking that was selected", ZDateTime.Today, includedBooking.KM_BookingOfTransportRequestedDate);
				Assert("KM_BookingOfTransportRequestedDate should not be set on the booking that was not selected", excludedBooking.KM_BookingOfTransportRequestedDate.IsEmpty);
			});
		}

		[TestDate(2017, 7, 1)]
		public void TestSetKM_BookingOfTransportRequestedDate_ExcludesDeactivatedForConsolidatedCartageAdvice()
		{
			var activeBooking = Helper.CreateBooking(BookingConsolidation);
			var deactivatedBooking = Helper.CreateBooking(BookingConsolidation);
			var notify = Notify;
			notify.Response = true;
			deactivatedBooking.Deactivate(notify);

			BookingConsolidation.OnSelectDtbBookingsToPrint += new EventHandler<DtbBookingsToPrintEventArgs>(delegate(object sender, DtbBookingsToPrintEventArgs e)
			{
				throw new NotSupportedException("Should not pop up grid to choose bookings because there is only 1 active booking, and also because you don't choose bookings on consolidated cartage advice.");
			});

			var menuItem = SetupMenuItemWithChildMenuItem(Constants.RefDocTypes.CartageAdvice);
			var eventArgs = new DocumentPrintedEventArgs(DeliveryInstructionDestination.Print, menuItem, false);
			var events = new IDocumentEventsMock();
			var supporter = ((IDocumentSupportable)BookingConsolidation).DocumentSupporter;
			supporter.Initialise(events);
			// Don't call DocumentSupporter.GetChildCollection
			events.NotifyDocumentPrePrinted(eventArgs);

			CombineAssertions("Doesn't handle consolidated cartage advice correctly", () =>
			{
				AssertEquals("KM_BookingOfTransportRequestedDate should be set on the booking that is active", ZDateTime.Today, activeBooking.KM_BookingOfTransportRequestedDate);
				Assert(
					"KM_BookingOfTransportRequestedDate should not be set on the booking that has been deactivated.",
					deactivatedBooking.KM_BookingOfTransportRequestedDate.IsEmpty
				);
			});
		}

		protected override IDocumentSupportable GetDocumentSupporterParent()
		{
			Helper.CreateBooking(BookingConsolidation);
			BookingConsolidation.OnSelectDtbBookingsToPrint += new EventHandler<DtbBookingsToPrintEventArgs>(delegate(object sender, DtbBookingsToPrintEventArgs e)
			{
				foreach (DocumentDtbBooking bookingToSelectFrom in e.BookingsToSelectFrom)
				{
					throw new NotSupportedException("Should not pop up grid to choose bookings.");
				}
				e.ContinueToPrint = true;
			});
			return BookingConsolidation;
		}

		protected override IDocumentSupportable[] GetBookingsFromBookingsProperty(DocumentSupporter documentSupporter)
		{
			return ((DtbBookingConsolidationDocumentSupporter)documentSupporter).Bookings;
		}

		TransportBookingTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingTestHelper(Factory)); }
		}
		TransportBookingTestHelper helper;

		DtbBookingConsolidation BookingConsolidation
		{
			get { return bookingConsolidation ?? (bookingConsolidation = Helper.CreateConsolidation()); }
		}
		DtbBookingConsolidation bookingConsolidation;

		TestNotificationBuffer Notify
		{
			get { return notify ?? (notify = new TestNotificationBuffer()); }
		}
		TestNotificationBuffer notify;

		protected override bool ChecksChildMenuItem => true;
		protected override bool DocumentSupportablesUseDifferentFactory => false;
	}

	[TestedType(typeof(DtbBookingConsolidationDocumentSupporter))]
	public class DtbBookingConsolidationDocumentSupporterDocumentSupporterTest : DocumentSupporterTest
	{
		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return bookingConsolidation ?? (bookingConsolidation = Helper.CreateConsolidation());
		}
		DtbBookingConsolidation bookingConsolidation;

		protected override void DoSetupForDocument(IDocumentCommand command, IDocumentSupportable documentSupportableBO)
		{
			var consolidation = (DtbBookingConsolidation)documentSupportableBO;
			consolidation.Bookings.AddNew();
			base.DoSetupForDocument(command, documentSupportableBO);
		}

		protected TransportBookingTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingTestHelper(Factory)); }
		}
		TransportBookingTestHelper helper;
	}
}
