using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.TransportBookings.Shared.Testing
{
	// This class is for testing logic within the abstract class CartageAdviceDocumentEventsHandler
	// For more integration-style tests, see DocumentSupporterCartageAdviceHandlerTest
	public class CartageAdviceDocumentEventsHandlerTest : TestCaseWithFactory
	{
		[TestDate(2017, 7, 1)]
		public void TestSetKM_BookingOfTransportRequestedDate_OnlyIfCartageAdvicePrinted()
		{
			var booking = CreateBooking();
			Factory.Save();

			var menuItem = SetupMenuItem();
			var cartageAdviceDocumentEventsHandler = new TestCartageAdviceDocumentEventsHandler(booking);

			cartageAdviceDocumentEventsHandler.HandleDocumentPrePrinted(null, new DocumentPrintedEventArgs(DeliveryInstructionDestination.Preview, menuItem));
			Assert("KM_BookingOfTransportRequestedDate should not be set when previewing cartage advice", booking.KM_BookingOfTransportRequestedDate.IsEmpty);

			cartageAdviceDocumentEventsHandler.HandleDocumentPrePrinted(null, new DocumentPrintedEventArgs(DeliveryInstructionDestination.Print, menuItem));
			AssertEquals("KM_BookingOfTransportRequestedDate should be set when printing cartage advice", ZDateTime.Now, booking.KM_BookingOfTransportRequestedDate);
		}

		[TestDate(2017, 7, 1)]
		public void TestCanHandleMenuItem_OnlyIfDocTypeIndicatesCartageAdvice()
		{
			var booking = CreateBooking();
			Factory.Save();

			var cartageAdviceDocumentEventsHandler = new TestCartageAdviceDocumentEventsHandler(booking);

			var menuItemWrongDocType = SetupMenuItem("ZZZ");
			AssertEquals("CanHandleMenuItem should return false when printing something which is not cartage advice", false, cartageAdviceDocumentEventsHandler.CanHandleMenuItem(menuItemWrongDocType));

			var menuItemCorrectDocType = SetupMenuItem();
			AssertEquals("CanHandleMenuItem should return true when printing something which is cartage advice", true, cartageAdviceDocumentEventsHandler.CanHandleMenuItem(menuItemCorrectDocType));
		}

		public void TestCanHandleMenuItem_False_IfNoDocTypeExists()
		{
			var booking = CreateBooking();
			Factory.Save();

			var cartageAdviceDocumentEventsHandler = new TestCartageAdviceDocumentEventsHandler(booking);

			var menuItemNoDocType = Factory.New<DocumentCommand>();
			menuItemNoDocType.SU_MenuName = "Uninformative menu name, no indication of what document type it is";

			var pivot = menuItemNoDocType.Documents.AddNew();
			pivot.SI_SU = menuItemNoDocType.PK;

			AssertEquals("CanHandleMenuItem should return false when printing something which has no document type information", false, cartageAdviceDocumentEventsHandler.CanHandleMenuItem(menuItemNoDocType));
		}

		[TestDate(2017, 7, 1)]
		public void TestSetKM_BookingOfTransportRequestedDate_OnlyIfExistingValueBlank()
		{
			var booking = CreateBooking();
			booking.KM_BookingOfTransportRequestedDate = ZDateTime.Today.AddDays(5);
			Factory.Save();

			var menuItem = SetupMenuItem();
			var cartageAdviceDocumentEventsHandler = new TestCartageAdviceDocumentEventsHandler(booking);

			cartageAdviceDocumentEventsHandler.HandleDocumentPrePrinted(null, new DocumentPrintedEventArgs(DeliveryInstructionDestination.Print, menuItem));
			AssertEquals("KM_BookingOfTransportRequestedDate should not be set if it already has a value", ZDateTime.Today.AddDays(5), booking.KM_BookingOfTransportRequestedDate);
		}

		[TestDate(2017, 7, 1)]
		public void TestSetKM_BookingOfTransportRequestedDate_EvenIfDraftVersion()
		{
			var booking = CreateBooking();
			Factory.Save();
			AssertEquals("Precondition: KM_BookingOfTransportRequestedDate is not set", ZDateTime.Empty, booking.KM_BookingOfTransportRequestedDate);

			var menuItem = SetupMenuItem();
			var cartageAdviceDocumentEventsHandler = new TestCartageAdviceDocumentEventsHandler(booking);

			cartageAdviceDocumentEventsHandler.HandleDocumentPrePrinted(null, new DocumentPrintedEventArgs(DeliveryInstructionDestination.Print, menuItem, true));
			AssertEquals("KM_BookingOfTransportRequestedDate should be set even if the document is a draft version", ZDateTime.Today, booking.KM_BookingOfTransportRequestedDate);
		}

		[TestDate(2017, 7, 1)]
		public void TestSetKM_BookingOfTransportRequestedDate_ChangesSavedByFactory_IfIsUserInteractiveTrue()
		{
			var originalIsUserInteractive = Globals.IsUserInteractive;
			try
			{
				var booking = CreateBooking();
				Factory.Save();
				Assert("Precondition: KM_BookingOfTransportRequestedDate is not set", booking.KM_BookingOfTransportRequestedDate.IsEmpty);

				var menuItem = SetupMenuItem();
				var cartageAdviceDocumentEventsHandler = new TestCartageAdviceDocumentEventsHandler(booking);
				Factory.Save();

				Globals.IsUserInteractive = true;
				cartageAdviceDocumentEventsHandler.HandleDocumentPrePrinted(null, new DocumentPrintedEventArgs(DeliveryInstructionDestination.Print, menuItem));

				var changedObjects = booking.Factory.GetChanges().GetChangedObjects();
				var newFactory = new BusinessObjectFactory();
				var bookingNewFactory = newFactory.Load<IDtbBooking>(booking.PK);
				CombineAssertions(() =>
				{
					AssertEquals("Should save change to KM_BookingOfTransportRequestedDate to database", ZDateTime.Today, bookingNewFactory.KM_BookingOfTransportRequestedDate);
					AssertEquals("Should have no business objects with unsaved changes", 0, changedObjects.Length);
				});
			}
			finally
			{
				Globals.IsUserInteractive = originalIsUserInteractive;
			}
		}

		IDtbBooking CreateBooking()
		{
			var booking = Factory.New<IDtbBooking>();
			((BusinessObject)booking).FillWithValidTestData();

			return booking;
		}

		DocumentCommand SetupMenuItem(string rT_DocType = RefDocTypes.CartageAdvice)
		{
			var menuItem = Factory.New<DocumentCommand>();
			menuItem.SU_MenuName = "Uninformative menu name, no indication of what document type it is";

			var stmTemplate = Factory.NewWithValidTestData<StmTemplate>();

			var pivot = menuItem.Documents.AddNew();
			pivot.SI_SU = menuItem.PK;
			pivot.SI_SO = stmTemplate.PK;

			var docType = Factory.NewWithValidTestData<RefDocType>();
			docType.RT_DocType = rT_DocType;

			pivot.SI_RT_DocType = docType.PK;

			return menuItem;
		}
	}
}
