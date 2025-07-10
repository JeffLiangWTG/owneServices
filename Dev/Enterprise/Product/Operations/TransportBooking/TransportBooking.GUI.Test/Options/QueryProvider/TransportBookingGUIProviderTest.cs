using System;
using System.Linq;
using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportBookings.Business.Options;
using Enterprise.TransportBookings.Business.Testing;
using Enterprise.TransportBookings.GUI.Options;
using Enterprise.TransportBookings.Shared;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.TransportBookings.GUI.Test
{
	public class TransportBookingGUIProviderTest : DtbBookingTestCaseWithFactory
	{
		public void TestITransportBookingQueryProvider_GetBookingsToDeliver_Any_NoSupportedDirections()
		{
			var dummy = Factory.New<DummyWithDtbBooking>();
			dummy.Z0_Description = "DUM456"; // required as this is the source object key, DataContextDataObjectWriter.PopulateDataObject - line 29 : dataContext.AddDataSource(manager.DataContextType, manager.DataContextKey);

			Factory.Save();

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes; // options form
			ZFormModaliser.ShowDialogsInTest = true;
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
			{
				var formAsDocumentForm = ((TransportBookingDocumentForm)form);
				formAsDocumentForm.DeliveryOptionsResultForTesting = TransportBookingDocumentOptions.DeliveryOption.OpenStandardDesigner;
			});

			dummy.SupportedDirections = Array.Empty<DtbBookingDirection>();
			var menu = Factory.New<StmMenuItem>();
			menu.SU_MenuName = "Bobs Doc";
			menu.SU_DocumentDirection = "ANY";

			var guiProvider = (ITransportBookingGUIProvider)new TransportBookingGUIProvider();
			var bookings = guiProvider.GetBookingsToDeliver(dummy, menu);
			AssertEquals("Error with direction, so there should be no bookings.", 0, bookings.Count());

			AssertMultilineASCIIEquals("Should have showed an error message to the user.", @"This menu Bobs Doc has a Direction of ANY, but the Job(s) Dummy Business Object DUM456 don't support any Transport Booking Direction.

Ensure the menu has a Direction set.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestITransportBookingQueryProvider_GetBookingsToDeliver_Any_2SupportedDirections()
		{
			var dummy = Factory.New<DummyWithDtbBooking>();
			dummy.Z0_Description = "DUM456"; // required as this is the source object key, DataContextDataObjectWriter.PopulateDataObject - line 29 : dataContext.AddDataSource(manager.DataContextType, manager.DataContextKey);

			Factory.Save();

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes; // options form
			ZFormModaliser.ShowDialogsInTest = true;
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
			{
				var formAsDocumentForm = ((TransportBookingDocumentForm)form);
				formAsDocumentForm.DeliveryOptionsResultForTesting = TransportBookingDocumentOptions.DeliveryOption.OpenStandardDesigner;
			});

			dummy.SupportedDirections = new DtbBookingDirection[] { DtbBookingDirection.PIC, DtbBookingDirection.DLV };
			var menu = Factory.New<StmMenuItem>();
			menu.SU_MenuName = "Bobs Doc";
			menu.SU_DocumentDirection = "ANY";

			var guiProvider = (ITransportBookingGUIProvider)new TransportBookingGUIProvider();
			var bookings = guiProvider.GetBookingsToDeliver(dummy, menu);
			AssertEquals("Error with direction, so there should be no bookings.", 0, bookings.Count());
			AssertMultilineASCIIEquals("Should have showed an error message to the user.", @"This menu Bobs Doc has a Direction of ANY, but the Job(s) Dummy Business Object DUM456 support both Origin and Destination Transport Bookings.

Ensure the menu has a Direction set.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestITransportBookingQueryProvider_GetBookingsToDeliver_Any_PicSupportedDirections()
		{
			var dummy = Factory.New<DummyWithDtbBooking>();
			dummy.Z0_Description = "DUM456"; // required as this is the source object key, DataContextDataObjectWriter.PopulateDataObject - line 29 : dataContext.AddDataSource(manager.DataContextType, manager.DataContextKey);

			Factory.Save();

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes; // options form
			ZFormModaliser.ShowDialogsInTest = true;
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((aa) =>
			{
				var formAsDocumentForm = ((TransportBookingDocumentForm)aa);
				formAsDocumentForm.DeliveryOptionsResultForTesting = TransportBookingDocumentOptions.DeliveryOption.OpenStandardDesigner;
			});

			dummy.SupportedDirections = new DtbBookingDirection[] { DtbBookingDirection.PIC };
			var menu = Factory.New<StmMenuItem>();
			menu.SU_DocumentDirection = "ANY";

			var guiProvider = (ITransportBookingGUIProvider)new TransportBookingGUIProvider();
			guiProvider.GetBookingsToDeliver(dummy, menu);
			var form = ((TransportBookingGUIProvider)guiProvider).LastDeliveryManagerForTest.LastControllerForTest.LastShownForm;
			AssertNotNull(form);
			AssertEquals("Should be the booking form", typeof(TransportBookingForm), form.GetType());
			var bookingForm = (TransportBookingForm)form;
			var booking = (DtbBooking)bookingForm.BusinessEntity;
			AssertEquals(nameof(DtbBookingDirection.PIC), booking.ConsolidationSingleJob.KB_JobDirection);

			form.Dispose();
		}

		public void TestITransportBookingQueryProvider_GetBookingsToDeliver_Dlv_2SupportedDirections()
		{
			var dummy = Factory.New<DummyWithDtbBooking>();
			dummy.Z0_Description = "DUM456"; // required as this is the source object key, DataContextDataObjectWriter.PopulateDataObject - line 29 : dataContext.AddDataSource(manager.DataContextType, manager.DataContextKey);

			Factory.Save();

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes; // options form
			ZFormModaliser.ShowDialogsInTest = true;
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((aa) =>
			{
				var formAsDocumentForm = ((TransportBookingDocumentForm)aa);
				formAsDocumentForm.DeliveryOptionsResultForTesting = TransportBookingDocumentOptions.DeliveryOption.OpenStandardDesigner;
			});

			dummy.SupportedDirections = new DtbBookingDirection[] { DtbBookingDirection.PIC, DtbBookingDirection.DLV };
			var menu = Factory.New<StmMenuItem>();
			menu.SU_DocumentDirection = "ARV";

			var guiProvider = (ITransportBookingGUIProvider)new TransportBookingGUIProvider();
			guiProvider.GetBookingsToDeliver(dummy, menu);
			var form = ((TransportBookingGUIProvider)guiProvider).LastDeliveryManagerForTest.LastControllerForTest.LastShownForm;
			AssertNotNull(form);
			AssertEquals("Should be the booking form", typeof(TransportBookingForm), form.GetType());
			var bookingForm = (TransportBookingForm)form;
			var booking = (DtbBooking)bookingForm.BusinessEntity;
			AssertEquals(nameof(DtbBookingDirection.DLV), booking.ConsolidationSingleJob.KB_JobDirection);

			form.Dispose();
		}

		public void TestITransportBookingQueryProvider_GetBookingsToDeliver_DoesNotUseOurFactory()
		{
			var dummy = Factory.New<DummyWithDtbBooking>();
			dummy.Z0_Description = "DUM456"; // required as this is the source object key, DataContextDataObjectWriter.PopulateDataObject - line 29 : dataContext.AddDataSource(manager.DataContextType, manager.DataContextKey);

			Factory.Save();

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes; // options form
			ZFormModaliser.ShowDialogsInTest = true;
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((aa) =>
			{
				var formAsDocumentForm = ((TransportBookingDocumentForm)aa);
				formAsDocumentForm.DeliveryOptionsResultForTesting = TransportBookingDocumentOptions.DeliveryOption.OpenStandardDesigner;
			});

			dummy.SupportedDirections = new DtbBookingDirection[] { DtbBookingDirection.PIC, DtbBookingDirection.DLV };
			var menu = Factory.New<StmMenuItem>();
			menu.SU_DocumentDirection = "ARV";

			var guiProvider = (ITransportBookingGUIProvider)new TransportBookingGUIProvider();

			var saveCount = Factory.SaveCount;
			guiProvider.GetBookingsToDeliver(dummy, menu);

			var form = ((TransportBookingGUIProvider)guiProvider).LastDeliveryManagerForTest.LastControllerForTest.LastShownForm;
			form.Dispose();

			Assert("Save count remains unchanged", Factory.IsEqualToCurrentSaveCount(saveCount));
		}
	}
}
