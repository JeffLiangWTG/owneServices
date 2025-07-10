using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.CreditControl.Business;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportBookings.Business.Options;
using Enterprise.TransportBookings.Business.Testing;
using Enterprise.TransportBookings.GUI.Testing;
using Enterprise.TransportBookings.Shared;
using Enterprise.TransportCommon.Business;
using Enterprise.TransportCommon.Registry;
using Enterprise.TransportCommon.Shared;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using static Enterprise.Integration.Customs;
using IForwardingShipment = Enterprise.Integration.Forwarding.IForwardingShipment;

namespace Enterprise.TransportBookings.GUI.Options.QueryProvider.Test
{
	public class DtbDeliveryManagerTest : DtbBookingTestCaseWithFactory
	{
		public void TestWhenOrgHeaderIsNull_DoesNotCallPublishUniversalShipment()
		{
			var dummy = Factory.New<DummyWithDtbBooking>();
			dummy.Z0_Description = "DUM456";

			var oldProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;

			try
			{
				GlbCompany.CurrentCompany.GC_OH_OrgProxy = ZGuid.Empty;
				Factory.Save();

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
				var manager = new DtbDeliveryManager(Factory, new[] { dummy }, dummy.GetSupportedDirections()[0], false);

				AssertNoExceptionThrown(
					"ArgumentNullException should be thrown if GetCommunicationModesForRecipient is called with null recipient",
					() => manager.DeliverTransportBooking());
				AssertEquals("Assert the correct message is display when the OrgHeader is Null", "Recipient Organization not set.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_OH_OrgProxy = oldProxy;
				Factory.Save();
			}
		}

		public void TestViewTransportBooking_NoBooking()
		{
			var dummy = Factory.New<DummyWithDtbBooking>();
			var dummy2 = Factory.New<DummyWithDtbBooking>();
			dummy.Z0_Description = "DUM456"; // required as this is the source object key, DataContextDataObjectWriter.PopulateDataObject - line 29 : dataContext.AddDataSource(manager.DataContextType, manager.DataContextKey);
			dummy2.Z0_Description = "DUM123";

			var manager = new DtbDeliveryManager(Factory, new[] { dummy, dummy2 }, dummy.GetSupportedDirections()[0], false);
			manager.ViewTransportBooking();
			AssertEquals("Could not find existing Transport Booking.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		[ExpectNoExceptions()]
		public void TestSelectBookingBeforeFormShow()
		{
			var dummy = Factory.New<DummyWithDtbBooking>();
			var dummy2 = Factory.New<DummyWithDtbBooking>();
			dummy.Z0_Description = "DUM456"; // required as this is the source object key, DataContextDataObjectWriter.PopulateDataObject - line 29 : dataContext.AddDataSource(manager.DataContextType, manager.DataContextKey);
			dummy2.Z0_Description = "DUM123";

			var consolidation = Helper.CreateConsolidation(dummy);
			var booking1 = Helper.CreateBooking(consolidation);
			var booking2 = Helper.CreateBooking(consolidation);
			Factory.Save();

			var manager = new DtbDeliveryManager(Factory, new[] { dummy, dummy2 }, dummy.GetSupportedDirections()[0], false);

			using (var form = new TransportBookingMultiForm(consolidation))
			{
				manager.SelectBookingForTest(booking1, form);
				form.Show();
				Application.DoEvents();
			}
		}

		public void TestViewTransportBooking()
		{
			var dummy = Factory.New<DummyWithDtbBooking>();
			var dummy2 = Factory.New<DummyWithDtbBooking>();
			dummy.Z0_Description = "DUM456"; // required as this is the source object key, DataContextDataObjectWriter.PopulateDataObject - line 29 : dataContext.AddDataSource(manager.DataContextType, manager.DataContextKey);
			dummy2.Z0_Description = "DUM123";

			var consolidation = Helper.CreateConsolidation(dummy);
			var booking = Helper.CreateBooking(consolidation);

			var consolidation2 = Helper.CreateConsolidation(dummy2);

			Factory.Save();

			var manager = new DtbDeliveryManager(Factory, new[] { dummy, dummy2 }, dummy.GetSupportedDirections()[0], false);
			using (TransportRegistry.Instance.DefaultTransportBookingTabView.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, BookingViews.Codes.Standard))
			{
				manager.ViewTransportBooking();
				using (var bookingFromParentForm1 = manager.LastControllerForTest.LastShownForm)
				{
					AssertNotNull(bookingFromParentForm1);
					AssertEquals("Should be the booking form for dummy1", typeof(TransportBookingForm), bookingFromParentForm1.GetType());
					AssertEquals("Should be the booking form for dummy1", booking.PK, ((DtbBooking)((TransportBookingForm)bookingFromParentForm1).BusinessEntity).PK);
					AssertEquals(TransportBookingInstructionView.Standard, ((TransportBookingForm)bookingFromParentForm1).InstructionViewsControl.View);
				}
			}

			using (TransportRegistry.Instance.DefaultTransportBookingTabView.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, BookingViews.Codes.Instruction))
			{
				manager.ViewTransportBooking();
				using (var bookingFromParentForm2 = manager.LastControllerForTest.LastShownForm)
				{
					AssertNotNull(bookingFromParentForm2);
					AssertEquals("Should be the booking form for dummy1", typeof(TransportBookingForm), bookingFromParentForm2.GetType());
					AssertEquals("Should be the booking form for dummy1", booking.PK, ((DtbBooking)((TransportBookingForm)bookingFromParentForm2).BusinessEntity).PK);
					AssertEquals(TransportBookingInstructionView.Instruction, ((TransportBookingForm)bookingFromParentForm2).InstructionViewsControl.View);
				}
			}
		}

		public void TestViewTransportBooking_ActiveAndInactive()
		{
			var dummy = Factory.New<DummyWithDtbBooking>();
			dummy.Z0_Description = "DUM456";
			var consolidation = Helper.CreateConsolidation(dummy);
			consolidation.KB_IsOverridden = false;
			var booking1 = Helper.CreateBooking(consolidation);
			var booking2 = Helper.CreateBooking(consolidation);
			Factory.Save();

			// multiple bookings all inactive shows multi form
			booking1.KM_IsActive = false;
			booking2.KM_IsActive = false;
			Factory.Save();

			var manager = new DtbDeliveryManager(Factory, new[] { dummy }, dummy.GetSupportedDirections()[0], false);
			manager.ViewTransportBooking();
			using (var bookingFromParentForm1 = manager.LastControllerForTest.LastShownForm)
			{
				AssertNotNull(bookingFromParentForm1);
				AssertEquals("Should be multi booking form", typeof(TransportBookingMultiForm), bookingFromParentForm1.GetType());
			}

			// strictly one active booking presents form of active booking
			booking1.KM_IsActive = false;
			booking2.KM_IsActive = true;
			Factory.Save();

			manager.ViewTransportBooking();
			using (var bookingFromParentForm2 = manager.LastControllerForTest.LastShownForm)
			{
				AssertNotNull(bookingFromParentForm2);
				AssertEquals("Should be multi booking form", typeof(TransportBookingForm), bookingFromParentForm2.GetType());

				var createdBooking = (DtbBooking)((TransportBookingForm)bookingFromParentForm2).BusinessEntity;
				AssertEquals("Form should be that of active booking", booking2.PK, createdBooking.PK);
			}

			// multiple active bookings present multi form
			booking1.KM_IsActive = true;
			booking2.KM_IsActive = true;
			Factory.Save();

			manager.ViewTransportBooking();
			using (var bookingFromParentForm3 = manager.LastControllerForTest.LastShownForm)
			{
				AssertNotNull(bookingFromParentForm3);
				AssertEquals("Should be multi booking form", typeof(TransportBookingMultiForm), bookingFromParentForm3.GetType());
			}
		}

		public void TestViewTransportBooking_BookingAlreadyOpen()
		{
			// for viewing, if booking form already open, should just switch focus to that form
			var dummy = Factory.New<DummyWithDtbBooking>();
			dummy.Z0_Description = "DUM456";
			var consolidation = Helper.CreateConsolidation(dummy);
			consolidation.KB_IsOverridden = false;
			var booking = Helper.CreateBooking(consolidation);
			Factory.Save();

			// multi booking form already open
			var controller = ZControllerFactory.Create(ControllerIDs.DtbBookingConsolidation);
			var manager = new DtbDeliveryManager(Factory, new[] { dummy }, dummy.GetSupportedDirections()[0], false);
			using (var multiForm = (TransportBookingMultiForm)controller.ShowEditForm(consolidation))
			{
				AssertEquals(true, multiForm.Visible);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes; // options form
				ZFormModaliser.ShowDialogsInTest = true;

				manager.ViewTransportBooking();
				using (var bookingFromParentForm = manager.LastControllerForTest.LastShownForm)
				{
					AssertEquals(typeof(TransportBookingMultiForm), bookingFromParentForm.GetType());
					AssertEquals(multiForm, bookingFromParentForm);
				}
			}

			// single booking form already open
			controller = ZControllerFactory.Create(ControllerIDs.DtbBooking);
			using (var singleForm = (TransportBookingForm)controller.ShowEditForm(booking))
			{
				AssertEquals(true, singleForm.Visible);
				manager.ViewTransportBooking();
				using (var bookingFromParentForm = manager.LastControllerForTest.LastShownForm)
				{
					AssertEquals(typeof(TransportBookingForm), bookingFromParentForm.GetType());
					AssertEquals(singleForm, bookingFromParentForm);
				}
			}

			// no form open previously
			manager.ViewTransportBooking();
			using (var bookingFromParentForm = manager.LastControllerForTest.LastShownForm)
			{
				AssertNotNull(bookingFromParentForm);
				AssertEquals(typeof(TransportBookingForm), bookingFromParentForm.GetType());
			}
		}

		public void TestCreateTransportBooking_ErrorMessageIncludeErrorsOnly()
		{
			var setupFactory = new BusinessObjectFactory();
			var helper = new TransportBookingTestHelper(setupFactory);
			var shipmentBO = helper.CreateForwardingShipment("S123", "", "SEA", "FCL");
			var consolBO = helper.CreateForwardingConsol(shipmentBO, "C123", "SEA", "");
			var packLine = helper.CreateForwardingPackline(shipmentBO, "PLT", 0);
			setupFactory.Save();

			var shipment = Factory.Load<IForwardingShipment>(shipmentBO.PK);
			var consol = new Shipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			consol.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.CTG } } });
			consol.DataContext.AddDataSource(DataContextType.ForwardingConsol, "C123");
			consol.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S123");

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes; // options form
			ZFormModaliser.ShowDialogsInTest = true;
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
			{
				var formAsDocumentForm = form as TransportBookingDocumentForm;
				if (formAsDocumentForm != null)
				{
					formAsDocumentForm.DeliveryOptionsResultForTesting = TransportBookingDocumentOptions.DeliveryOption.AutoDelivery;
				}
			});

			var manager = new DtbDeliveryManager(Factory, (IDtbBookingParent)shipment, DtbBookingDirection.PIC, false);
			manager.TypeOfExceptionToThrowDuringSave = DtbDeliveryManagerErrorsToTestFor.UniversalEventDeliveryFailureException;

			var bookings = manager.DeliverTransportBooking();
			var lastMessage = UnitTestUserNotification.Instance.LastMessage.Text;
			AssertEquals("Should show error message.", true, lastMessage.Contains("Test Universal Event Delivery Failure"));
			AssertEquals("Should not include non-error logging message.", false, lastMessage.Contains("No matching DtbBookingConsolidation found, creating new DtbBookingConsolidation."));
		}

		public void TestDeliverTransportBookingWithLoggerAndAutomatedErrorManager_LogsOnErrorAndCollectsError()
		{
			var setupFactory = new BusinessObjectFactory();
			var helper = new TransportBookingTestHelper(setupFactory);
			var shipmentBO = helper.CreateForwardingShipment("S123", "", "SEA", "FCL");
			var consolBO = helper.CreateForwardingConsol(shipmentBO, "C123", "SEA", "");
			var packLine = helper.CreateForwardingPackline(shipmentBO, "PLT", 0);
			setupFactory.Save();

			var shipment = Factory.Load<IForwardingShipment>(shipmentBO.PK);
			var consol = new Shipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			consol.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.CTG } } });
			consol.DataContext.AddDataSource(DataContextType.ForwardingConsol, "C123");
			consol.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S123");

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes; // options form
			ZFormModaliser.ShowDialogsInTest = true;
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
			{
				var formAsDocumentForm = form as TransportBookingDocumentForm;
				if (formAsDocumentForm != null)
				{
					formAsDocumentForm.DeliveryOptionsResultForTesting = TransportBookingDocumentOptions.DeliveryOption.AutoDelivery;
				}
			});

			var logger = new TestServiceLogger();
			var errorManager = NewAutomatedErrorManager();
			var manager = new DtbDeliveryManager(Factory, (IDtbBookingParent)shipment, DtbBookingDirection.PIC, false, logger, errorManager);
			manager.TypeOfExceptionToThrowDuringSave = DtbDeliveryManagerErrorsToTestFor.UniversalEventDeliveryFailureException;

			var bookings = manager.DeliverTransportBooking();
			var lastMessage = UnitTestUserNotification.Instance.LastMessage.Text;
			var expectedError = "Test Universal Event Delivery Failure";
			AssertEquals("Should show error message.", true, lastMessage.Contains(expectedError));
			AssertEquals("Should not include non-error logging message.", false, lastMessage.Contains("No matching DtbBookingConsolidation found, creating new DtbBookingConsolidation."));
			var expectedCollectedError = "Failed to create Transport Booking: " + expectedError;
			AssertEquals("Should log errors", "Error|" + expectedCollectedError + System.Environment.NewLine, logger.ToString());
			AssertEquals("Should be 1 error in error manager list", 1, errorManager.ErrorList.Count);
			var error = errorManager.ErrorList.Single();
			CombineAssertions("Should have collected error in error manager", () =>
			{
				AssertEquals("Error should have correct type - UniversalEventDeliveryFailure", DtbBookingCreationErrorType.UniversalEventDeliveryFailure, error.ErrorType);
				AssertEquals("Error should have correct message", expectedError, error.Message);
			});
		}

		public void TestDeliverTransportBookingWithLogger_DoesNotLogIfSuccessful()
		{
			var setupFactory = new BusinessObjectFactory();
			var helper = new TransportBookingTestHelper(setupFactory);
			var shipmentBO = helper.CreateForwardingShipment("S123", "", "SEA", "FCL");
			var consolBO = helper.CreateForwardingConsol(shipmentBO, "C123", "SEA", "");
			var packLine = helper.CreateForwardingPackline(shipmentBO, "PLT", 1);
			setupFactory.Save();

			var shipment = Factory.Load<IForwardingShipment>(shipmentBO.PK);
			var consol = new Shipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			consol.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.CTG } } });
			consol.DataContext.AddDataSource(DataContextType.ForwardingConsol, "C123");
			consol.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S123");

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes; // options form
			ZFormModaliser.ShowDialogsInTest = true;
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
			{
				var formAsDocumentForm = form as TransportBookingDocumentForm;
				if (formAsDocumentForm != null)
				{
					formAsDocumentForm.DeliveryOptionsResultForTesting = TransportBookingDocumentOptions.DeliveryOption.AutoDelivery;
				}
			});

			var logger = new TestServiceLogger();
			var manager = new DtbDeliveryManager(Factory, (IDtbBookingParent)shipment, DtbBookingDirection.PIC, false, logger);

			var bookings = manager.DeliverTransportBooking();
			var lastMessages = UnitTestUserNotification.Instance.LastMessage.Text;
			AssertNullOrEmpty("Should not include non-error logging message.", lastMessages);

			AssertEquals("Should not log when no errors found", string.Empty, logger.ToString());
		}

		public void TestCreateTransportBooking_WithCallback()
		{
			var dummy = Factory.New<DummyWithDtbBooking>();
			dummy.Z0_Description = "123546";
			Factory.Save();

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes; // options form
			ZFormModaliser.ShowDialogsInTest = true;

			var callbackWasCalled = false;

			Func<bool> callbackFalse = () =>
			{
				callbackWasCalled = true;
				return false;
			};

			Func<bool> callbackTrue = () =>
			{
				callbackWasCalled = true;
				return true;
			};

			var transportBookingCreatedAndSavedHit = false;
			dummy.TransportBookingCreatedOrUpdated_ForTesting += delegate
			{ transportBookingCreatedAndSavedHit = true; };

			DtbDeliveryManager createdManager = null;
			DtbDeliveryManager.CreateTransportBooking(Factory, dummy, dummy.GetSupportedDirections()[0], false, manager => createdManager = manager, callbackFalse);
			AssertEquals(false, transportBookingCreatedAndSavedHit);
			AssertEquals(true, callbackWasCalled);
			AssertNull(createdManager);

			transportBookingCreatedAndSavedHit = false;
			callbackWasCalled = false;
			DtbDeliveryManager.CreateTransportBooking(Factory, dummy, dummy.GetSupportedDirections()[0], false, manager => createdManager = manager, callbackTrue);
			using (var bookingFromParentForm = createdManager.LastControllerForTest.LastShownForm)
			{
				AssertEquals(true, transportBookingCreatedAndSavedHit);
				AssertEquals(true, callbackWasCalled);
				AssertNotNull(createdManager);
			}
		}

		public void TestCreateTransportBooking()
		{
			var dummy = Factory.New<DummyWithDtbBooking>();
			var dummy2 = Factory.New<DummyWithDtbBooking>();
			dummy.Z0_Description = "DUM456"; // required as this is the source object key, DataContextDataObjectWriter.PopulateDataObject - line 29 : dataContext.AddDataSource(manager.DataContextType, manager.DataContextKey);
			dummy2.Z0_Description = "DUM123";

			Factory.Save();

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes; // options form
			ZFormModaliser.ShowDialogsInTest = true;

			bool transportBookingCreatedAndSavedHit = false;
			dummy.TransportBookingCreatedOrUpdated_ForTesting += delegate
			{ transportBookingCreatedAndSavedHit = true; };

			var manager = new DtbDeliveryManager(Factory, new[] { dummy, dummy2 }, dummy.GetSupportedDirections()[0], false);
			manager.TurnOnSaveCounting();
			AssertEquals(false, transportBookingCreatedAndSavedHit);
			manager.CreateTransportBooking();
			AssertEquals(true, transportBookingCreatedAndSavedHit);

			// NOTE: To fix this unit test when ShipmentDataContextManager.ReadIntoBusinessObject() no longer calls Factory.SaveAtEndOfImport() simply change the test implementation
			// of int const DtbDeliveryManager.ExpectedSavesDuringMainProcessing from 1 to 0
			AssertEquals("Should not call factory save during processing", DtbDeliveryManager.ExpectedSavesDuringMainProcessing, manager.SavesDuringMainProcessing);

			using (var bookingFromParentForm = manager.LastControllerForTest.LastShownForm)
			{
				AssertNotNull(bookingFromParentForm);
				AssertEquals("Should be the booking form", typeof(TransportBookingForm), bookingFromParentForm.GetType());

				var booking = (DtbBooking)((TransportBookingForm)bookingFromParentForm).BusinessEntity;
				AssertEquals("Should be the booking form for dummy1", dummy.PK, booking.ConsolidationSingleJob.Parent.ParentWithWorkflow.PK);
				AssertEquals("When Creating, always set overridden on", true, booking.ConsolidationSingleJob.KB_IsOverridden);
			}
		}

		public void TestCreateTransportBooking_Static()
		{
			var dummy = Factory.New<DummyWithDtbBooking>();
			dummy.Z0_Description = "DUM456";
			Factory.Save();

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes; // options form
			ZFormModaliser.ShowDialogsInTest = true;

			var transportBookingCreatedAndSavedHit = false;
			dummy.TransportBookingCreatedOrUpdated_ForTesting += delegate
			{ transportBookingCreatedAndSavedHit = true; };

			DtbDeliveryManager createdManager = null;
			DtbDeliveryManager.CreateTransportBooking(Factory, dummy, dummy.GetSupportedDirections()[0], false, manager => createdManager = manager);
			AssertEquals(true, transportBookingCreatedAndSavedHit);
			AssertNotNull(createdManager);
			AssertEquals(false, createdManager.CombineContainersForTest);

			using (var bookingFromParentForm = createdManager.LastControllerForTest.LastShownForm)
			{
				AssertNotNull(bookingFromParentForm);
				AssertEquals("Should be the booking form", typeof(TransportBookingForm), bookingFromParentForm.GetType());

				var booking = (DtbBooking)((TransportBookingForm)bookingFromParentForm).BusinessEntity;
				AssertEquals("Should be the booking form for dummy1", dummy.PK, booking.ConsolidationSingleJob.Parent.ParentWithWorkflow.PK);
				AssertEquals("When Creating, always set overridden on", true, booking.ConsolidationSingleJob.KB_IsOverridden);
			}
		}

		public void TestCreateTransportBooking_Static_WithSecurity()
		{
			var dummy = Factory.New<DummyWithDtbBooking>();
			dummy.Z0_Description = "DUM456";
			Factory.Save();

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes; // options form
			ZFormModaliser.ShowDialogsInTest = true;

			var transportBookingCreatedAndSavedHit = false;
			dummy.TransportBookingCreatedOrUpdated_ForTesting += delegate
			{ transportBookingCreatedAndSavedHit = true; };

			try
			{
				Env.Security.DtbBookingNew.IsAllowed = false;
				DtbDeliveryManager createdManager = null;
				DtbDeliveryManager.CreateTransportBooking(Factory, dummy, dummy.GetSupportedDirections()[0], false, manager => createdManager = manager);
				AssertEquals(false, transportBookingCreatedAndSavedHit);
				AssertNull(createdManager);
				AssertEquals(@"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Operate -> Transport Booking -> Bookings -> New",
UnitTestUserNotification.Instance.LastMessage.Text);
			}
			finally
			{
				Env.Security.DtbBookingNew.IsAllowed = true;
			}
		}

		public void TestHasExistingBooking()
		{
			AssertEquals(false, DtbDeliveryManager.HasExistingBooking(null, false));

			var dummy = Factory.New<DummyWithDtbBooking>();
			var consol = Helper.CreateConsolidation(dummy);
			AssertEquals(false, DtbDeliveryManager.HasExistingBooking(consol, false));

			var box1 = consol.PackageJob.Packages.AddNew("BOX", "BX123");
			var box2 = consol.PackageJob.Packages.AddNew("BOX", "BX456");

			var booking = Helper.CreateBooking(consol, "IFFD", "Test", "PIC", "REF");
			AssertEquals(true, DtbDeliveryManager.HasExistingBooking(consol, false));

			booking.Instructions[0].DivotsWithPackages.AddPackage(box1);
			booking.Instructions[0].DivotsWithPackages.AddPackage(box2);
			AssertEquals(true, DtbDeliveryManager.HasExistingBooking(consol, false));

			var container1 = consol.PackageJob.Packages.AddNew("CNT", "CONT123");
			booking.Instructions[0].DivotsWithPackages.AddPackage(container1);
			AssertEquals(true, DtbDeliveryManager.HasExistingBooking(consol, false));

			var container2 = consol.PackageJob.Packages.AddNew("CNT", "CONT456");
			booking.Instructions[0].DivotsWithPackages.AddPackage(container2);
			AssertEquals(false, DtbDeliveryManager.HasExistingBooking(consol, false));
		}

		public void TestHasExistingBooking_CombineContainersTrue()
		{
			AssertEquals(false, DtbDeliveryManager.HasExistingBooking(null, true));

			var dummy = Factory.New<DummyWithDtbBooking>();
			var consol = Helper.CreateConsolidation(dummy);
			AssertEquals(false, DtbDeliveryManager.HasExistingBooking(consol, true));

			var box1 = consol.PackageJob.Packages.AddNew("BOX", "BX123");
			var box2 = consol.PackageJob.Packages.AddNew("BOX", "BX456");

			var booking = Helper.CreateBooking(consol, "IFFD", "Test", "PIC", "REF");
			AssertEquals(false, DtbDeliveryManager.HasExistingBooking(consol, true));

			booking.Instructions[0].DivotsWithPackages.AddPackage(box1);
			booking.Instructions[0].DivotsWithPackages.AddPackage(box2);
			AssertEquals(false, DtbDeliveryManager.HasExistingBooking(consol, true));

			var container1 = consol.PackageJob.Packages.AddNew("CNT", "CONT123");
			booking.Instructions[0].DivotsWithPackages.AddPackage(container1);
			AssertEquals(false, DtbDeliveryManager.HasExistingBooking(consol, true));

			var container2 = consol.PackageJob.Packages.AddNew("CNT", "CONT456");
			booking.Instructions[0].DivotsWithPackages.AddPackage(container2);
			AssertEquals(true, DtbDeliveryManager.HasExistingBooking(consol, true));
		}

		public void TestSingleBookingFormPresentedWhenThereAreNoActiveBookings()
		{
			// create new consolidation with inactive entry
			var dummy = Factory.New<DummyWithDtbBooking>();
			dummy.Z0_Description = "DUM456";
			var consolidation = Helper.CreateConsolidation(dummy);
			consolidation.KB_IsOverridden = true;
			var booking1 = Helper.CreateBooking(consolidation);
			var booking2 = Helper.CreateBooking(consolidation);
			booking1.KM_IsActive = ZBool.False;
			booking2.KM_IsActive = ZBool.False;
			Factory.Save();

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes; // options form
			ZFormModaliser.ShowDialogsInTest = true;
			// go through process of creating a new booking
			bool transportBookingCreatedAndSavedHit = false;
			dummy.TransportBookingCreatedOrUpdated_ForTesting += delegate
			{ transportBookingCreatedAndSavedHit = true; };

			var manager = new DtbDeliveryManager(Factory, new[] { dummy }, dummy.GetSupportedDirections()[0], false);
			manager.CreateTransportBooking();
			AssertEquals("Booking window should be displayed", true, transportBookingCreatedAndSavedHit);

			using (var bookingFromParentForm = manager.LastControllerForTest.LastShownForm)
			{
				AssertNotNull(bookingFromParentForm);
				AssertEquals("Should be the booking form", typeof(TransportBookingForm), bookingFromParentForm.GetType());

				var createdBooking = (DtbBooking)((TransportBookingForm)bookingFromParentForm).BusinessEntity;
				AssertNotEquals(booking1.PK, createdBooking.PK);
				AssertNotEquals(booking2.PK, createdBooking.PK);
			}
		}

		public void TestInactiveBookingsAreNotOverriddenByNewRecordsThatMatch()
		{
			// create new consolidation with inactive entry
			var dummy = Factory.New<DummyWithDtbBooking>();
			dummy.Z0_Description = "DUM456";
			var consolidation = Helper.CreateConsolidation(dummy);
			var booking = Helper.CreateBooking(consolidation);
			booking.KM_IsActive = ZBool.False;
			Factory.Save();

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes; // options form
			ZFormModaliser.ShowDialogsInTest = true;

			// go through process of creating a new booking
			bool transportBookingCreatedAndSavedHit = false;
			dummy.TransportBookingCreatedOrUpdated_ForTesting += delegate
			{ transportBookingCreatedAndSavedHit = true; };

			var manager = new DtbDeliveryManager(Factory, new[] { dummy }, dummy.GetSupportedDirections()[0], false);
			manager.CreateTransportBooking();
			AssertEquals("Booking window should be displayed", true, transportBookingCreatedAndSavedHit);
			using (var bookingFromParentForm = manager.LastControllerForTest.LastShownForm)
			{
				var createdBooking = (DtbBooking)((TransportBookingForm)bookingFromParentForm).BusinessEntity;
				AssertNotEquals("New booking should not be the old booking", booking.PK, createdBooking.PK);
				AssertEquals("should be 2 bookings now", consolidation.Bookings.Count, 2);
			}
		}

		public void TestCreateTransportBooking_Commenced()
		{
			var dummy = Factory.New<DummyWithDtbBooking>();
			dummy.Z0_Description = "DUM456";
			var consolidation = Helper.CreateConsolidation(dummy);
			consolidation.KB_IsOverridden = false;
			var booking = Helper.CreateBooking(consolidation);
			booking.Logs.AddNew(Events.ServiceCommenced, "", ZDateTimeOffset.Today, isEstimate: false);
			Factory.Save();

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes; // options form
			ZFormModaliser.ShowDialogsInTest = true;

			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
			{
				if (form is TransportBookingDocumentForm formAsDocumentForm)
				{
					formAsDocumentForm.DeliveryOptionsResultForTesting = TransportBookingDocumentOptions.DeliveryOption.OpenStandardDesigner;
				}
			});

			bool transportBookingCreatedAndSavedHit = false;
			dummy.TransportBookingCreatedOrUpdated_ForTesting += delegate
			{ transportBookingCreatedAndSavedHit = true; };

			var factoryToUse = new BusinessObjectFactory();
			var manager = new DtbDeliveryManager(factoryToUse, new[] { factoryToUse.Load<DummyWithDtbBooking>(dummy.PK) }, dummy.GetSupportedDirections()[0], false);
			manager.CreateTransportBooking();
			using (var bookingFromParentForm = manager.LastControllerForTest.LastShownForm)
			{
				AssertEquals("Should have not hit TransportBookingCreatedOrUpdated", false, transportBookingCreatedAndSavedHit);
				AssertMaxTableHits("", 1, StmALogSchema.Constants.TableName, factoryToUse);
				AssertEquals("Should have sent correct alert", true, UnitTestUserNotification.Instance.LastMessage.Text.Contains(
					"Changes may have been made on the job that effects the Transport Booking. These changes will not be reflected on the Transport Booking as the Transport Company has commenced work on transport related to this job."));

				AssertNotNull(bookingFromParentForm);
				AssertEquals("Should be the booking form", typeof(TransportBookingForm), bookingFromParentForm.GetType());
				AssertEquals(booking.PK, ((DtbBooking)((TransportBookingForm)bookingFromParentForm).BusinessEntity).PK);
			}
		}

		public void TestCreateTransportBooking_FromShipmentAndAttachedDispatch()
		{
			var dummy = Factory.New<DummyWithDtbBookingAndConfirmMessage>();

			Factory.Save();

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes; // options form
			ZFormModaliser.ShowDialogsInTest = true;

			bool transportBookingCreatedAndSavedHit = false;
			dummy.TransportBookingCreatedOrUpdated_ForTesting += delegate
			{ transportBookingCreatedAndSavedHit = true; };

			var factoryToUse = new BusinessObjectFactory();
			var manager = new DtbDeliveryManager(factoryToUse, new[] { dummy }, dummy.GetSupportedDirections()[0], false);

			manager.TurnOnSaveCounting();
			AssertEquals(false, transportBookingCreatedAndSavedHit);
			CombineAssertions(() =>
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				manager.CreateTransportBooking();

				AssertEquals("Should have sent Warning", "Test confrim message", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("No Transport Booking created", false, transportBookingCreatedAndSavedHit);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				manager.CreateTransportBooking();

				AssertEquals("Should have sent Warning", "Test confrim message", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Transport Booking has Created", true, transportBookingCreatedAndSavedHit);
			});

			using (var bookingFromParentForm = manager.LastControllerForTest.LastShownForm)
			{
				AssertNotNull(bookingFromParentForm);
				AssertEquals("Should be the booking form", typeof(TransportBookingForm), bookingFromParentForm.GetType());
			}
		}

		public void TestCreateTransportBooking_NoActiveCommenced()
		{
			var dummy = Factory.New<DummyWithDtbBooking>();
			dummy.Z0_Description = "DUM456";
			var consolidation = Helper.CreateConsolidation(dummy);
			consolidation.KB_IsOverridden = false;
			var inactiveBooking = Helper.CreateBooking(consolidation);
			inactiveBooking.Logs.AddNew(Events.ServiceCommenced, "", ZDateTimeOffset.Today, isEstimate: false);
			inactiveBooking.KM_IsActive = false;
			Factory.Save();

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes; // options form
			ZFormModaliser.ShowDialogsInTest = true;

			bool transportBookingCreatedAndSavedHit = false;
			dummy.TransportBookingCreatedOrUpdated_ForTesting += delegate
			{ transportBookingCreatedAndSavedHit = true; };

			var manager = new DtbDeliveryManager(Factory, new[] { dummy }, dummy.GetSupportedDirections()[0], false);
			AssertEquals("Precondition: TransportBookingCreatedOrUpdated not yet hit", false, transportBookingCreatedAndSavedHit);
			manager.CreateTransportBooking();
			using (var bookingFromParentForm = manager.LastControllerForTest.LastShownForm)
			{
				CombineAssertions("Should have reacted correctly to existing inactive and service commenced Transport Booking", () =>
				{
					AssertEquals("Should have hit TransportBookingCreatedOrUpdated", true, transportBookingCreatedAndSavedHit);

					AssertNotNull(bookingFromParentForm);
					AssertEquals("Should be the booking form", typeof(TransportBookingForm), bookingFromParentForm.GetType());

					var booking = (DtbBooking)((TransportBookingForm)bookingFromParentForm).BusinessEntity;
					AssertNotEquals("Should be a new booking", inactiveBooking.PK, booking.ConsolidationSingleJob.Parent.ParentWithWorkflow.PK);
				});
			}
		}

		public void TestCreateTransportBooking_Overridden()
		{
			var dummy = Factory.New<DummyWithDtbBooking>();
			dummy.Z0_Description = "DUM456";
			var consolidation = Helper.CreateConsolidation(dummy);
			consolidation.KB_IsOverridden = true;
			var booking = Helper.CreateBooking(consolidation);
			Factory.Save();

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes; // options form
			ZFormModaliser.ShowDialogsInTest = true;
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
			{
				if (form is TransportBookingDocumentForm formAsDocumentForm)
				{
					formAsDocumentForm.DeliveryOptionsResultForTesting = TransportBookingDocumentOptions.DeliveryOption.OpenStandardDesigner;
				}
			});

			bool transportBookingCreatedAndSavedHit = false;
			dummy.TransportBookingCreatedOrUpdated_ForTesting += delegate
			{ transportBookingCreatedAndSavedHit = true; };

			var manager = new DtbDeliveryManager(Factory, new[] { dummy }, dummy.GetSupportedDirections()[0], false);
			manager.CreateTransportBooking();
			using (var bookingFromParentForm = manager.LastControllerForTest.LastShownForm)
			{
				CombineAssertions("Should have reacted correctly to existing overridden Transport Booking Consolidation", () =>
				{
					Assert("Should have sent correct alert", UnitTestUserNotification.Instance.LastMessage.Text.Contains(
						"Changes may have been made on the job that effects the Transport Booking. These changes will not be reflected on the Transport Booking as the Booking has been overridden and is now managed independently of the job."));
					AssertEquals(false, transportBookingCreatedAndSavedHit);

					AssertNotNull(bookingFromParentForm);
					AssertEquals("Should be the booking form", typeof(TransportBookingForm), bookingFromParentForm.GetType());
					AssertEquals(booking.PK, ((DtbBooking)((TransportBookingForm)bookingFromParentForm).BusinessEntity).PK);
				});
			}
		}

		public void TestCreateTransportBooking_SubBooking()
		{
			TestCreateTransportBooking_SubBookingCore(isOverridden: false, isCommenced: false);
		}

		public void TestCreateTransportBooking_SubBookingOverridden()
		{
			TestCreateTransportBooking_SubBookingCore(isOverridden: true, isCommenced: false);
		}

		public void TestCreateTransportBooking_SubBookingCommenced()
		{
			TestCreateTransportBooking_SubBookingCore(isOverridden: false, isCommenced: true);
		}

		public void TestCreateTransportBooking_SubBookingOverriddenAndCommenced()
		{
			TestCreateTransportBooking_SubBookingCore(isOverridden: true, isCommenced: true);
		}

		void TestCreateTransportBooking_SubBookingCore(bool isOverridden, bool isCommenced)
		{
			var dummy = Factory.New<DummyWithDtbBooking>();
			dummy.Z0_Description = "DUM456";
			var subConsolidation = Helper.CreateConsolidation(dummy);
			var subBooking = Helper.CreateBooking(subConsolidation);
			subBooking.KM_JobID = "Sub1";
			Factory.Save();

			var masterBooking = Helper.CreateBooking();
			masterBooking.KM_IsMaster = true;
			masterBooking.KM_JobID = "Master1";
			Factory.Save();

			masterBooking.SubBookings.Add(subBooking);
			Factory.Save();

			if (isOverridden)
			{
				subConsolidation.KB_IsOverridden = true;
				Factory.Save();
			}

			if (isCommenced)
			{
				subBooking.Logs.AddNew(Events.ServiceCommenced, "", ZDateTimeOffset.Today, isEstimate: false);
				Factory.Save();
			}

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes; // options form
			ZFormModaliser.ShowDialogsInTest = true;
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
			{
				if (form is TransportBookingDocumentForm formAsDocumentForm)
				{
					formAsDocumentForm.DeliveryOptionsResultForTesting = TransportBookingDocumentOptions.DeliveryOption.OpenStandardDesigner;
				}
			});

			bool transportBookingCreatedAndSavedHit = false;
			dummy.TransportBookingCreatedOrUpdated_ForTesting += delegate
			{ transportBookingCreatedAndSavedHit = true; };

			var manager = new DtbDeliveryManager(Factory, new[] { dummy }, dummy.GetSupportedDirections()[0], false);
			manager.CreateTransportBooking();
			using (var bookingFromParentForm = manager.LastControllerForTest.LastShownForm)
			{
				CombineAssertions("CreateTransportBooking() should have reacted correctly to this situation", () =>
				{
					Assert("Should have sent correct sub booking alert", UnitTestUserNotification.Instance.LastMessage.Text.Contains(
						"Booking is a sub booking under Master Transport Booking Master1 and cannot be overwritten."));
					Assert("Should have sent correct final part of sub booking alert message", UnitTestUserNotification.Instance.LastMessage.Text.Contains(
						"Existing sub booking will now be displayed."));
					if (isOverridden)
					{
						Assert("Should have sent correct overridden alert", UnitTestUserNotification.Instance.LastMessage.Text.Contains(
			"Booking has been overridden and is now managed independently of the job."));
					}
					if (isCommenced)
					{
						Assert("Should have sent correct booking service commenced alert", UnitTestUserNotification.Instance.LastMessage.Text.Contains("Transport Company has commenced work on transport related to this job."));
					}

					AssertEquals(false, transportBookingCreatedAndSavedHit);

					AssertNotNull(bookingFromParentForm);
					AssertEquals("Should be the booking form", typeof(TransportBookingForm), bookingFromParentForm.GetType());
					AssertEquals(subBooking.PK, ((DtbBooking)((TransportBookingForm)bookingFromParentForm).BusinessEntity).PK);
				});
			}
		}

		public void TestCreateTransportBooking_WithErrorsFromUniversalXML()
		{
			var dummy = Factory.New<DummyWithDtbBooking>();
			var dummy2 = Factory.New<DummyWithDtbBooking>();
			dummy.Z0_Description = "DUM456";
			dummy2.Z0_Description = "DUM123";
			Factory.Save();

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes; // options form
			ZFormModaliser.ShowDialogsInTest = true;

			bool transportBookingCreatedAndSavedHit = false;
			dummy.TransportBookingCreatedOrUpdated_ForTesting += delegate
			{ transportBookingCreatedAndSavedHit = true; };

			var manager = new DtbDeliveryManager(Factory, new[] { dummy, dummy2 }, dummy.GetSupportedDirections()[0], false);
			manager.SetErrorMessageForTest = true;
			AssertNoExceptionThrown(() => manager.CreateTransportBooking());
			AssertEquals(false, transportBookingCreatedAndSavedHit);
		}

		public void TestCreateTransportBooking_NoContainerMode()
		{
			var dummy = Factory.New<DummyWithDtbBooking>();

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			var containerFCL = new Container(DefaultDataObjectWriterStrategy.TestInstance);
			var containerLCL = new Container(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.SetContainerCollection(() => new DataObjectList<Container>());
			shipment.ContainerCollection.Add(containerFCL);
			shipment.ContainerCollection.Add(containerLCL);

			containerFCL.FCL_LCL_AIR = new ContainerMode() { Code = "FCL" };
			containerLCL.FCL_LCL_AIR = new ContainerMode() { Code = "LCL" };

			var manager = new DtbDeliveryManager(Factory, new[] { dummy }, dummy.GetSupportedDirections()[0], false);
			var options = manager.GetOptionsForTest(dummy, shipment, shipment);

			AssertEquals("EFPR", options.Template); // Export FCL CYD-CNR-CTO
		}

		public void TestGetOptions_SpecifiedTemplate()
		{
			var dummy = Factory.New<DummyWithDtbBooking>();

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			var containerFCL = new Container(DefaultDataObjectWriterStrategy.TestInstance);
			var containerLCL = new Container(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.SetContainerCollection(() => new DataObjectList<Container>());
			shipment.ContainerCollection.Add(containerFCL);
			shipment.ContainerCollection.Add(containerLCL);

			containerFCL.FCL_LCL_AIR = new ContainerMode() { Code = "FCL" };
			containerLCL.FCL_LCL_AIR = new ContainerMode() { Code = "LCL" };

			var manager = new DtbDeliveryManager(Factory, new[] { dummy }, dummy.GetSupportedDirections()[0], false);
			var options = manager.GetOptionsForTest(dummy, shipment, shipment, "XXXX");

			AssertEquals("Should have specified template in options", "XXXX", options.Template);
		}

		public void TestCreateTransportBooking_BookingAlreadyOpen()
		{
			var dummy = Factory.New<DummyWithDtbBooking>();
			dummy.Z0_Description = "DUM456";
			var consolidation = Helper.CreateConsolidation(dummy);
			consolidation.KB_IsOverridden = false;
			var booking = Helper.CreateBooking(consolidation);
			Factory.Save();

			var expectedMessage = @"Cannot create a new Transport Booking as an existing Transport Booking screen is open.

Close the Transport Booking screen and try again.";

			// multi booking form already open
			var controller = ZControllerFactory.Create(ControllerIDs.DtbBookingConsolidation);
			var manager = new DtbDeliveryManager(Factory, new[] { dummy }, dummy.GetSupportedDirections()[0], false);
			using (var multiForm = (TransportBookingMultiForm)controller.ShowEditForm(consolidation))
			{
				AssertEquals(true, multiForm.Visible);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes; // options form
				ZFormModaliser.ShowDialogsInTest = true;

				manager.CreateTransportBooking();
				Assert(UnitTestUserNotification.Instance.LastMessage.Text.Contains(expectedMessage));
			}

			// single booking form already open
			controller = ZControllerFactory.Create(ControllerIDs.DtbBooking);
			using (var singleForm = (TransportBookingForm)controller.ShowEditForm(booking))
			{
				AssertEquals(true, singleForm.Visible);
				manager.CreateTransportBooking();
				Assert(UnitTestUserNotification.Instance.LastMessage.Text.Contains(expectedMessage));
			}

			// no form is open previously
			manager.CreateTransportBooking();
			using (var bookingFromParentForm = manager.LastControllerForTest.LastShownForm)
			{
				AssertNotNull(bookingFromParentForm);
				var createdBooking = (DtbBooking)((TransportBookingForm)bookingFromParentForm).BusinessEntity;
				AssertEquals(createdBooking.ConsolidationSingleJob.PK, consolidation.PK);    // same consolidation
				AssertEquals(2, createdBooking.ConsolidationSingleJob.Bookings.Count);   // should have 2 bookings (just created a new booking)
			}
		}

		public void TestCreateTransportBooking_BookingCollectionOnParentFactoryRefreshed()
		{
			var dummy = Factory.New<DummyWithDtbBooking>();
			dummy.Z0_Description = "DUM456";
			var consolidation = Helper.CreateConsolidation(dummy);
			consolidation.KB_IsOverridden = false;
			var booking = Helper.CreateBooking(consolidation);
			Factory.Save();

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;

			dummy.TransportBookingCreatedOrUpdated_ForTesting = delegate
			{
				AssertEquals("Should have refereshed bookings collection in parent factory.", 2, consolidation.Bookings.Count);
			};

			var manager = new DtbDeliveryManager(Factory, new[] { dummy }, dummy.GetSupportedDirections()[0], false);
			try
			{
				manager.CreateTransportBooking();
			}
			finally
			{
				manager.LastControllerForTest.LastShownForm.Dispose();
			}
		}

		public void TestCreateTransportBooking_WithUSCustoms()
		{
			var declaration = Factory.New<US.IJobDeclaration>();
			Factory.Save();

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes; // options form
			ZFormModaliser.ShowDialogsInTest = true;

			var manager = new DtbDeliveryManager(Factory, (IDtbBookingParent)declaration, DtbBookingDirection.PIC, false);
			manager.CreateTransportBooking();

			using (var bookingFromParentForm = manager.LastControllerForTest.LastShownForm)
			{
				AssertNotNull(bookingFromParentForm);
				AssertEquals("Should be the booking form", typeof(TransportBookingForm), bookingFromParentForm.GetType());

				var booking = (DtbBooking)((TransportBookingForm)bookingFromParentForm).BusinessEntity;
				AssertEquals("Should be the booking form for declaration", declaration.PK, booking.ConsolidationSingleJob.Parent.ParentWithWorkflow.PK);
				AssertEquals("When Creating, always set overridden on", true, booking.ConsolidationSingleJob.KB_IsOverridden);
			}
		}

		public void TestCreateTransportBooking_AddEventsShouldBeByActiveUser()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "ABC";
			staff.GS_LoginName = "fakeuser";
			staff.GS_IsActive = true;
			staff.GS_IsController = true;
			staff.GS_GB_HomeBranch = Env.CurrentBranchPK;
			staff.GS_GE_HomeDepartment = Env.CurrentDepartmentPK;

			Factory.Save();

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				var dummy = Factory.New<DummyWithDtbBooking>();
				dummy.Z0_Description = "DUM456";

				Factory.Save();

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes; // options form
				ZFormModaliser.ShowDialogsInTest = true;

				var manager = new DtbDeliveryManager(Factory, new[] { dummy }, dummy.GetSupportedDirections()[0], false);
				manager.CreateTransportBooking();

				using (var bookingFromParentForm = manager.LastControllerForTest.LastShownForm)
				{
					var booking = (DtbBooking)((TransportBookingForm)bookingFromParentForm).BusinessEntity;
					var addEvents = booking.Logs.Find(x => x.Event.SE_Code == "ADD");
					AssertEquals("There should be no ADD logs not made by the active user", false, addEvents.Any(x => x.SL_GS_NKUser != staff.GS_Code));
				}
			}
		}

		[TestDate(2018, 1, 10)]
		public void TestCreateTransportBooking_PortTransport()
		{
			var localClient = Helper.CreateOrganisation("LCL");
			var staff = CreateStaff("ABC", "fakeuser", Env.CurrentBranch, Env.CurrentDepartment);
			Factory.Save();

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				var dummyBookingParent = Factory.New<DummyWithDtbBooking>();
				dummyBookingParent.Z0_Description = "DUM456";
				var job = new JobHeader.Loader(dummyBookingParent).TryLoadOrCreate();
				job.JH_OA_LocalChargesAddr = localClient.MainAddress.PK;

				Factory.Save();

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes; // options form
				ZFormModaliser.ShowDialogsInTest = true;

				var manager = new DtbDeliveryManager(Factory, new[] { dummyBookingParent }, dummyBookingParent.GetSupportedDirections()[0], false);
				manager.CreateTransportBooking();

				using (var bookingFromParentForm = manager.LastControllerForTest.LastShownForm)
				{
					var booking = Factory.Load<DtbBooking>(((BusinessObject)((TransportBookingForm)bookingFromParentForm).BusinessEntity).PK);

					BookingToTransportJobCreatorTestHelper.MakeBookingValidForManuallyCreatingATransportJobFrom(Helper, booking);
					BookingToTransportJobCreatorTestHelper.SetDatesRelatedToAutomaticTransportJobCreationForBooking(Factory, booking, ZDateTime.Now, ZDateTime.Now.AddHours(1));

					Factory.Save();

					AssertEquals("Ensure Current Branch is currrently on the Booking.", GlbBranch.CurrentBranch.PK, booking.KM_GB_Branch);
					AssertEquals("Ensure TB has JobHeader from it's parent.", job.PK, booking.Job.PK);

					TestDateAttribute.Date = ZDateTime.Now.AddMinutes(5).ToDateTime(); // run 5 minutes in the future
					using (TransportRegistry.Instance.AutoCreateTransportConsignmentsGracePeriod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 1)) // find TB Jobs created >1 minute ago
					using (TransportRegistry.Instance.CreateTransportJobsFromTransportBookingEffectiveDate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Now.AddDays(-11).ToDateTime()))
					{
						var buffer = new NotificationBuffer();

						var orgZZZ = Helper.CreateOrganisation("ZZZ");
						var companyZZZ = Helper.CreateCompany("ZZZ", "ZZ Zee", orgZZZ);
						var departmentZZZ = Helper.CreateDepartment("ZZZ");
						var branchZZZ = Helper.CreateBranch("ZZZ", "Dilanka", companyZZZ, orgZZZ);
						var staffZZZ = CreateStaff("ZZZ", "ZZYYZZ", branchZZZ, departmentZZZ);
						Factory.Save();

						using (Env.SetTemporaryUserContext(staffZZZ.PK.ToGuid(), branchZZZ.PK.ToGuid(), departmentZZZ.PK.ToGuid()))
						{
							var creator = new BookingToTransportJobCommonCreator(buffer);
							creator.CreateTransportJobsFromTransportBookings();
						}

						var portTransportJob = AssertBookingHasAttachedPortTransport(booking);
						var jobOnPortTransport = new JobHeader.Loader((IJobHeaderParent)portTransportJob).Load();
						AssertEquals("Even though the caller (ie Service Task) maybe running under a random branch, the creator corrects the User Context for the booking.", localClient.MainAddress.PK, jobOnPortTransport.JH_OA_LocalChargesAddr);
					}
				}
			}
		}

		[TestDate(2018, 1, 10)]
		public void TestCreateTransportBooking_PortTransport_MultipleBookingsCreatedUnderDifferentCompanies()
		{
			var localClientAAA = Helper.CreateOrganisation("LAA");
			var localClientBBB = Helper.CreateOrganisation("LBB");

			var orgAAA = Helper.CreateOrganisation("AAA");
			var companyAAA = Helper.CreateCompany("AAA", "AA Aye", orgAAA);
			var departmentAAA = Helper.CreateDepartment("AAA");
			var branchAAA = Helper.CreateBranch("AAA", "ABranch", companyAAA, orgAAA);
			var staffAAA = CreateStaff("AAA", "A Staff", branchAAA, departmentAAA);

			var orgBBB = Helper.CreateOrganisation("BBB");
			var companyBBB = Helper.CreateCompany("BBB", "BB Aye", orgBBB);
			var departmentBBB = Helper.CreateDepartment("BBB");
			var branchBBB = Helper.CreateBranch("BBB", "BBranch", companyBBB, orgBBB);
			var staffBBB = CreateStaff("BBB", "B Staff", branchBBB, departmentBBB);
			Factory.Save();

			var bookingInAAA = CreateTransportBookingUnderStaff(localClientAAA, staffAAA, "DUMAAA");
			var bookingInBBB = CreateTransportBookingUnderStaff(localClientBBB, staffBBB, "DUMBBB");

			TestDateAttribute.Date = ZDateTime.Now.AddMinutes(5).ToDateTime(); // run 5 minutes in the future
			using (TransportRegistry.Instance.AutoCreateTransportConsignmentsGracePeriod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 1)) // find TB Jobs created >1 minute ago
			using (TransportRegistry.Instance.CreateTransportJobsFromTransportBookingEffectiveDate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Now.AddDays(-11).ToDateTime()))
			{
				var buffer = new NotificationBuffer();

				var orgZZZ = Helper.CreateOrganisation("ZZZ");
				var companyZZZ = Helper.CreateCompany("ZZZ", "ZZ Zee", orgZZZ);
				var departmentZZZ = Helper.CreateDepartment("ZZZ");
				var branchZZZ = Helper.CreateBranch("ZZZ", "Dilanka", companyZZZ, orgZZZ);
				var staffZZZ = CreateStaff("ZZZ", "ZZYYZZ", branchZZZ, departmentZZZ);
				Factory.Save();

				using (Env.SetTemporaryUserContext(staffZZZ.PK.ToGuid(), branchZZZ.PK.ToGuid(), departmentZZZ.PK.ToGuid()))
				{
					var creator = new BookingToTransportJobCommonCreator(buffer);
					creator.CreateTransportJobsFromTransportBookings();
				}

				var portTransportJobFromAAA = AssertBookingHasAttachedPortTransport(bookingInAAA);
				var portTransportJobFromBBB = AssertBookingHasAttachedPortTransport(bookingInBBB);
				var jobOnPortTransportAAA = new JobHeader.Loader((IJobHeaderParent)portTransportJobFromAAA).Load();
				var jobOnPortTransportBBB = new JobHeader.Loader((IJobHeaderParent)portTransportJobFromBBB).Load();
				AssertEquals("Even though the caller (ie Service Task) maybe running under a random branch, the creator corrects the User Context for each booking.", localClientAAA.MainAddress.PK, jobOnPortTransportAAA.JH_OA_LocalChargesAddr);
				AssertEquals("Even though the caller (ie Service Task) maybe running under a random branch, the creator corrects the User Context for each booking.", localClientBBB.MainAddress.PK, jobOnPortTransportBBB.JH_OA_LocalChargesAddr);
			}
		}

		DtbBooking CreateTransportBookingUnderStaff(OrgHeader localClient, GlbStaff staff, string dummyCode)
		{
			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				var dummy = Factory.New<DummyWithDtbBooking>();
				dummy.Z0_Description = dummyCode;
				var job = new JobHeader.Loader(dummy).TryLoadOrCreate();
				job.JH_OA_LocalChargesAddr = localClient.MainAddress.PK;

				Factory.Save();

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes; // options form
				ZFormModaliser.ShowDialogsInTest = true;

				var manager = new DtbDeliveryManager(Factory, new[] { dummy }, dummy.GetSupportedDirections()[0], false);
				manager.CreateTransportBooking();

				using (var bookingFromParentForm = manager.LastControllerForTest.LastShownForm)
				{
					var booking = Factory.Load<DtbBooking>(((BusinessObject)((TransportBookingForm)bookingFromParentForm).BusinessEntity).PK);

					BookingToTransportJobCreatorTestHelper.MakeBookingValidForManuallyCreatingATransportJobFrom(Helper, booking);
					BookingToTransportJobCreatorTestHelper.SetDatesRelatedToAutomaticTransportJobCreationForBooking(Factory, booking, ZDateTime.Now, ZDateTime.Now.AddHours(1));

					Factory.Save();
					AssertEquals("Ensure Current Branch is currrently on the Booking.", GlbBranch.CurrentBranch.PK, booking.KM_GB_Branch);
					AssertEquals("Ensure TB has JobHeader from it's parent.", job.PK, booking.Job.PK);

					return booking;
				}
			}
		}

		GlbStaff CreateStaff(ZString code, ZString name, IBranch branch, IDepartment department)
		{
			var staff2 = Factory.New<GlbStaff>();
			staff2.GS_Code = code;
			staff2.GS_LoginName = name;
			staff2.GS_IsActive = true;
			staff2.GS_IsController = true;
			staff2.GS_GB_HomeBranch = branch.PK;
			staff2.GS_GE_HomeDepartment = department.PK;
			return staff2;
		}

		BusinessObject AssertBookingHasAttachedPortTransport(DtbBooking booking)
		{
			var query = new ZQuery(JobCartageSchema.JJ_ParentID, booking.PK);
			query.AddToFilter(JobCartageSchema.JJ_ParentTableCode, booking.TablePrefix);

			var portTransports = new BusinessObjectFactory() { RefreshEnabled = false }.Load<Freight.LocalCartage.Integration.ICommonCartage>(query);
			AssertEquals(booking.HumanReadableName + " was unable to create a Port Transport.", 1, portTransports.Length);

			var portTransport = (EnterpriseBusinessObject)portTransports[0];
			var expectedTransportReference = portTransport[JobCartageSchema.Constants.JJ_ConsignmentID];
			var serviceLogs = portTransport.Logs.GetAllLogs().Cast<StmALog>().Where(l => l.SL_SE_NKEvent == AutoEvents.ServiceCommencedCode);
			AssertEquals(1, serviceLogs.Count());
			AssertEquals(expectedTransportReference, serviceLogs.ElementAt(0).ReferenceFreeText);

			AssertEquals(TransportStatuses.Codes.ServiceCommenced, new BusinessObjectFactory().Load<DtbBooking>(booking.PK).KM_Status);

			return portTransport;
		}

		public void TestCreateTransportBooking_ShouldUseCurrentBranch()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "ZZZ";

			var company = Factory.New<GlbCompany>();
			company.GC_Code = "AAB";
			company.GC_OH_OrgProxy = orgHeader.PK;

			var department = Factory.New<GlbDepartment>();
			department.GE_Code = "FDP";

			var branches = Enumerable.Range(1, 3).Select(x =>
			{
				var branch = Factory.New<GlbBranch>();
				branch.GB_Code = $"A{x}";
				branch.GB_GC = company.PK;
				branch.GB_OH_OrgProxy = orgHeader.PK;
				return branch;
			}).ToArray();

			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "ABC";
			staff.GS_LoginName = "fakeuser";
			staff.GS_IsActive = true;
			staff.GS_IsController = true;
			staff.GS_GE_HomeDepartment = department.PK;

			Factory.Save();

			foreach (var branchCombination in branches.SelectMany(x => branches, (x, y) => new Tuple<GlbBranch, GlbBranch>(x, y)))
			{
				var staffBranch = branchCombination.Item1;
				var environmentBranch = branchCombination.Item2;

				var factory = new BusinessObjectFactory();
				staff.GS_GB_HomeBranch = staffBranch.PK;

				factory.Save();

				using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), environmentBranch.PK.ToGuid(), department.PK.ToGuid()))
				{
					var dummy = factory.New<DummyWithDtbBooking>();

					factory.Save();

					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes; // options form
					ZFormModaliser.ShowDialogsInTest = true;

					var manager = new DtbDeliveryManager(factory, new[] { dummy }, dummy.GetSupportedDirections()[0], false);
					manager.CreateTransportBooking();

					using (var bookingFromParentForm = manager.LastControllerForTest.LastShownForm)
					{
						var booking = (DtbBooking)((TransportBookingForm)bookingFromParentForm).BusinessEntity;
						AssertEquals("Branch should be the same as the active user", environmentBranch.GB_Code, booking.Branch.GB_Code);
					}
				}
			}
		}

		public void TestCreateTransportBooking_Cancelled()
		{
			var dummy = Factory.New<DummyWithDtbBooking>();
			dummy.Z0_Description = "DUM456";

			Factory.Save();

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.No;

			bool transportBookingCreatedAndSavedHit = false;
			dummy.TransportBookingCreatedOrUpdated_ForTesting += delegate
			{ transportBookingCreatedAndSavedHit = true; };

			var manager = new DtbDeliveryManager(Factory, new[] { dummy }, dummy.GetSupportedDirections()[0], false);
			AssertEquals(false, transportBookingCreatedAndSavedHit);
			manager.CreateTransportBooking();

			AssertEquals("Transport booking should not be created after clicking Cancel", false, transportBookingCreatedAndSavedHit);
			AssertNull(manager.LastControllerForTest);
		}

		public void TestDeliverTransportBooking()
		{
			var dummy = Factory.New<DummyWithDtbBooking>();
			var dummy2 = Factory.New<DummyWithDtbBooking>();
			dummy.Z0_Description = "DUM456"; // required as this is the source object key, DataContextDataObjectWriter.PopulateDataObject - line 29 : dataContext.AddDataSource(manager.DataContextType, manager.DataContextKey);
			dummy2.Z0_Description = "DUM789"; // must be different to "DUM123" - otherwise we will try to create 'duplicate' booking consolidation on Dummy record without "DUM123" as Z0_Description (due to DummyWithDtbBookingShipmentDataObjectWriter.GetDataObject() always writing DUM123 as one of the top level DataSources

			Factory.Save();

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes; // options form
			ZFormModaliser.ShowDialogsInTest = true;
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
			{
				var formAsDocumentForm = ((TransportBookingDocumentForm)form);
				formAsDocumentForm.DeliveryOptionsResultForTesting = TransportBookingDocumentOptions.DeliveryOption.AutoDelivery;
			});

			int transportBookingCreatedAndSavedHit_Dummy1 = 0;
			dummy.TransportBookingCreatedOrUpdated_ForTesting += delegate
			{ transportBookingCreatedAndSavedHit_Dummy1++; };

			int transportBookingCreatedAndSavedHit_Dummy2 = 0;
			dummy.TransportBookingCreatedOrUpdated_ForTesting += delegate
			{ transportBookingCreatedAndSavedHit_Dummy2++; };

			var manager = new DtbDeliveryManager(Factory, new[] { dummy, dummy2 }, dummy.GetSupportedDirections()[0], false);
			AssertEquals(0, transportBookingCreatedAndSavedHit_Dummy1);
			AssertEquals(0, transportBookingCreatedAndSavedHit_Dummy2);
			var bookings = manager.DeliverTransportBooking();
			AssertEquals(1, transportBookingCreatedAndSavedHit_Dummy1);
			AssertEquals(1, transportBookingCreatedAndSavedHit_Dummy2);

			AssertContainsExactElementsInAnyOrder(new[] { dummy.PK, dummy2.PK }, bookings.Select(b => ((DtbBooking)b).ConsolidationSingleJob.Parent.ParentWithWorkflow.PK));
			AssertEquals("When Delivering, always leave overridden off", false, bookings.Cast<DtbBooking>().First().ConsolidationSingleJob.KB_IsOverridden);
		}

		public void TestDeliverTransportBooking_Commenced()
		{
			TestDeliverTransportBooking_CommencedOrOverridden(testingCommenced: true, testMultiple: false, withLoggerAndErrorManager: false);
		}

		public void TestDeliverTransportBooking_Commenced_WithLoggerAndErrorManager()
		{
			TestDeliverTransportBooking_CommencedOrOverridden(testingCommenced: true, testMultiple: false, withLoggerAndErrorManager: true);
		}

		public void TestDeliverTransportBooking_Commenced_Multiple()
		{
			TestDeliverTransportBooking_CommencedOrOverridden(testingCommenced: true, testMultiple: true, withLoggerAndErrorManager: false);
		}

		void TestDeliverTransportBooking_CommencedOrOverridden(bool testingCommenced, bool testMultiple, bool withLoggerAndErrorManager)
		{
			var parents = new List<IDtbBookingParent>();
			var expectedBookings = new List<DtbBooking>();

			var dummy1 = Factory.New<DummyWithDtbBooking>();
			dummy1.Z0_Description = "DUM456";
			var consolidation1 = Helper.CreateConsolidation(dummy1);
			var booking1 = Helper.CreateBooking(consolidation1);
			consolidation1.KB_IsOverridden = !testingCommenced;
			expectedBookings.Add(booking1);
			parents.Add(dummy1);

			if (testingCommenced)
			{
				booking1.Logs.AddNew(Events.ServiceCommenced, "", ZDateTimeOffset.Today, isEstimate: false);
				AssertEquals("Precondition", true, booking1.HasServiceCommencedLogs);
			}

			if (testMultiple)
			{
				var dummy2 = Factory.New<DummyWithDtbBooking>();
				dummy2.Z0_Description = "DUM123";

				var consolidation2 = Helper.CreateConsolidation(dummy2);
				var booking2 = Helper.CreateBooking(consolidation2);
				consolidation2.KB_IsOverridden = !testingCommenced;
				expectedBookings.Add(booking2);
				parents.Add(dummy2);

				if (testingCommenced)
				{
					booking2.Logs.AddNew(Events.ServiceCommenced, "", ZDateTimeOffset.Today, isEstimate: false);
					AssertEquals("Precondition", true, booking2.HasServiceCommencedLogs);
				}
			}

			Factory.Save();

			var optionsFormShownAndAssertionsRun = false;
			TransportBookingDocumentForm formAsDocumentForm = null;
			try
			{
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes; // options form
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
				{
					if (!optionsFormShownAndAssertionsRun && form is TransportBookingDocumentForm formAsDocumentFormTemp)
					{
						formAsDocumentForm = formAsDocumentFormTemp;
						AssertEquals(testingCommenced, formAsDocumentForm.DocumentOptions_ForTesting.ShowCommencedError);
						formAsDocumentForm.DeliveryOptionsResultForTesting = TransportBookingDocumentOptions.DeliveryOption.AutoDelivery;
						optionsFormShownAndAssertionsRun = true;
					}
				});

				ZFormModaliser.ShowDialogsInTest = true;

				var factoryToUse = new BusinessObjectFactory();
				ILogger logger = null;
				IAutomatedDtbBookingCreationErrorManager errorManager = null;
				if (withLoggerAndErrorManager)
				{
					logger = new TestServiceLogger();
					errorManager = NewAutomatedErrorManager();
				}
				var manager = new DtbDeliveryManager(factoryToUse, parents.ToArray(), dummy1.GetSupportedDirections()[0], false, logger, errorManager);
				var result = manager.DeliverTransportBooking();
				AssertContainsExactElementsInAnyOrder(expectedBookings.Select(b => b.PK), result.Select(b => b.PK));

				if (!testMultiple) // Only given option of printing the existing cartage advices
				{
					AssertEquals(true, optionsFormShownAndAssertionsRun);
				}

				if (withLoggerAndErrorManager)
				{
					var expectedError = testingCommenced ? "Transport Company has commenced work on transport related to this job." : "Booking has been overridden and is now managed independently of the job.";
					var expectedErrorType = testingCommenced ? DtbBookingCreationErrorType.ServiceHasCommencedError : DtbBookingCreationErrorType.ConsolidationCannotBeOverriddenAgainError;
					var expectedLog = "Error|Failed to Create Transport Booking:" + System.Environment.NewLine + expectedError +
		System.Environment.NewLine;
					AssertEquals("Should log errors", expectedLog, logger.ToString());
					AssertEquals("Should be 1 error in error manager list", 1, errorManager.ErrorList.Count);
					var errorManagerError = errorManager.ErrorList.Single();
					AssertEquals("Should have collected correct error type", expectedErrorType, errorManagerError.ErrorType);
					AssertEquals("Should have collected correct error message", expectedError, errorManagerError.Message);
				}
			}
			finally
			{
				formAsDocumentForm?.Dispose();
			}
		}

		public void TestDeliverTransportBooking_Sub()
		{
			TestDeliverTransportBooking_SubCore(isOverridden: false, isCommenced: false);
		}

		public void TestDeliverTransportBooking_SubOverridden()
		{
			TestDeliverTransportBooking_SubCore(isOverridden: true, isCommenced: false);
		}

		public void TestDeliverTransportBooking_SubCommenced()
		{
			TestDeliverTransportBooking_SubCore(isOverridden: false, isCommenced: true);
		}

		public void TestDeliverTransportBooking_SubOverriddenAndCommenced()
		{
			TestDeliverTransportBooking_SubCore(isOverridden: true, isCommenced: true);
		}

		void TestDeliverTransportBooking_SubCore(bool isOverridden, bool isCommenced)
		{
			var parents = new List<IDtbBookingParent>();

			var dummy1 = Factory.New<DummyWithDtbBooking>();
			dummy1.Z0_Description = "DUM456";
			var subConsolidation1 = Helper.CreateConsolidation(dummy1);
			var subBooking1 = Helper.CreateBooking(subConsolidation1);
			subBooking1.KM_JobID = "Sub1";
			parents.Add(dummy1);
			Factory.Save();

			var masterBooking1 = Helper.CreateBooking();
			masterBooking1.KM_IsMaster = true;
			masterBooking1.KM_JobID = "Master1";
			Factory.Save();

			masterBooking1.SubBookings.Add(subBooking1);
			Factory.Save();

			if (isOverridden)
			{
				subConsolidation1.KB_IsOverridden = true;
				Factory.Save();
			}

			if (isCommenced)
			{
				subBooking1.Logs.AddNew(Events.ServiceCommenced, "", ZDateTimeOffset.Today, isEstimate: false);
				Factory.Save();
			}

			var optionsFormShownAndAssertionsRun = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes; // options form
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
			{
				if (!optionsFormShownAndAssertionsRun && form is TransportBookingDocumentForm formAsDocumentForm)
				{
					AssertEquals(isCommenced, formAsDocumentForm.DocumentOptions_ForTesting.ShowCommencedError);
					formAsDocumentForm.DeliveryOptionsResultForTesting = TransportBookingDocumentOptions.DeliveryOption.AutoDelivery;
					optionsFormShownAndAssertionsRun = true;
				}
			});

			ZFormModaliser.ShowDialogsInTest = true;

			var factoryToUse = new BusinessObjectFactory();
			var logger = new DetailedLoggerForTest();
			var errorManager = NewAutomatedErrorManager();

			var manager = new DtbDeliveryManager(factoryToUse, parents.ToArray(), dummy1.GetSupportedDirections()[0], false, logger, errorManager);
			var result = manager.DeliverTransportBooking();

			const string ExpectedExistingBookingIsSubError = "Booking is a sub booking under Master Transport Booking Master1 and cannot be overwritten.";
			const string ExpectedConsolidationIsOverriddenError = "Booking has been overridden and is now managed independently of the job.";
			const string ExpectedBookingIsCommencedError = "Transport Company has commenced work on transport related to this job.";

			var numberOfErrors = 1 + (isOverridden ? 1 : 0) + (isCommenced ? 1 : 0);

			CombineAssertions("Should have returned existing sub booking and should have logged and collected errors correctly", () =>
			{
				AssertEquals("Should have returned subBooking1", subBooking1.PK, result.Single().PK);
				AssertEquals(FormattableString.Invariant($"Should log {numberOfErrors} error(s)"), numberOfErrors, logger.Logs.Count);
				var expectedSubError = GetLogLineFromError(ExpectedExistingBookingIsSubError);
				Assert("Should have logged existing sub booking error", logger.Logs.Any(l => l.Item1 == LogType.Error && l.Item2.Equals(expectedSubError)));
				if (isOverridden)
				{
					Assert("Should have logged consolidation overridden error", logger.Logs.Any(l => l.Item1 == LogType.Error && l.Item2.Equals(GetLogLineFromError(ExpectedConsolidationIsOverriddenError))));
				}
				if (isCommenced)
				{
					Assert("Should have logged booking service commenced error", logger.Logs.Any(l => l.Item1 == LogType.Error && l.Item2.Equals(GetLogLineFromError(ExpectedBookingIsCommencedError))));
				}

				AssertEquals(FormattableString.Invariant($"Should be {numberOfErrors} error(s) in error manager list"), numberOfErrors, errorManager.ErrorList.Count);
				Assert("Should have collected error for existing sub booking", errorManager.ErrorList.Any(e => e.ErrorType == DtbBookingCreationErrorType.ExistingBookingIsSubError && e.Message.Equals(ExpectedExistingBookingIsSubError)));
				if (isOverridden)
				{
					Assert("Should have collected error for consolidation overridden", errorManager.ErrorList.Any(e => e.ErrorType == DtbBookingCreationErrorType.ConsolidationCannotBeOverriddenAgainError && e.Message.Equals(ExpectedConsolidationIsOverriddenError)));
				}
				if (isCommenced)
				{
					Assert("Should have collected error for booking service commenced", errorManager.ErrorList.Any(e => e.ErrorType == DtbBookingCreationErrorType.ServiceHasCommencedError && e.Message.Equals(ExpectedBookingIsCommencedError)));
				}
			});
			string GetLogLineFromError(string errorMessage)
			{
				return "Failed to Create Transport Booking:" + System.Environment.NewLine + errorMessage;
			}
		}

		public void TestDeliverTransportBooking_DisposesForm()
		{
			var dummy = Factory.New<DummyWithDtbBooking>();
			dummy.Z0_Description = "distractions";
			var consolidation = Helper.CreateConsolidation(dummy);
			consolidation.KB_IsOverridden = true;
			Factory.Save();

			ZFormModaliser.ShowDialogsInTest = true;

			var manager = new DtbDeliveryManager(Factory, new[] { dummy }, dummy.GetSupportedDirections()[0], false);

			var bookings = manager.DeliverTransportBooking();
			AssertEquals("Precondition: Form Shown.", typeof(TransportBookingDocumentForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
		}

		public void TestDeliverTransportBooking_Overridden()
		{
			TestDeliverTransportBooking_CommencedOrOverridden(testingCommenced: false, testMultiple: false, withLoggerAndErrorManager: false);
		}

		public void TestDeliverTransportBooking_Overridden_WithLoggerAndErrorManager()
		{
			TestDeliverTransportBooking_CommencedOrOverridden(testingCommenced: false, testMultiple: false, withLoggerAndErrorManager: true);
		}

		public void TestDeliverTransportBooking_Overridden_Multiple()
		{
			TestDeliverTransportBooking_CommencedOrOverridden(testingCommenced: false, testMultiple: true, withLoggerAndErrorManager: false);
		}

		public void TestDeliverTransportBooking_WorkflowNoGUI()
		{
			var dummy = Factory.New<DummyWithDtbBooking>();
			var dummy2 = Factory.New<DummyWithDtbBooking>();
			dummy.Z0_Description = "DUM456"; // required as this is the source object key, DataContextDataObjectWriter.PopulateDataObject - line 29 : dataContext.AddDataSource(manager.DataContextType, manager.DataContextKey);
			dummy2.Z0_Description = "DUM789"; // must be different to "DUM123" - otherwise we will try to create 'duplicate' booking consolidation on Dummy record without "DUM123" as Z0_Description (due to DummyWithDtbBookingShipmentDataObjectWriter.GetDataObject() always writing DUM123 as one of the top level DataSources

			Factory.Save();

			var isUserInteractive = Globals.IsUserInteractive;
			try
			{
				Globals.IsUserInteractive = false;

				var manager = new DtbDeliveryManager(Factory, new[] { dummy, dummy2 }, dummy.GetSupportedDirections()[0], false);
				var bookings = manager.DeliverTransportBooking();

				AssertNull(ZFormModaliser.LastFormShownDialogForTest);
				AssertNull(ZFormModaliser.LastCommonDialogShownDialogForTest);
				AssertNull(ZFormModaliser.LastFormShownForTest);

				AssertContainsExactElementsInAnyOrder(new[] { dummy.PK, dummy2.PK }, bookings.Select(b => ((DtbBooking)b).ConsolidationSingleJob.Parent.ParentWithWorkflow.PK));
				AssertEquals("When Delivering, always leave overridden off", false, bookings.Cast<DtbBooking>().First().ConsolidationSingleJob.KB_IsOverridden);
			}
			finally
			{
				Globals.IsUserInteractive = isUserInteractive;
			}
		}

		public void TestDeliverTransportBookingHasChanged_WorkflowNoGUI()
		{
			var dummy = Factory.New<DummyWithDtbBooking>();
			var dummy2 = Factory.New<DummyWithDtbBooking>();
			dummy.Z0_Description = "DUM456"; // required as this is the source object key, DataContextDataObjectWriter.PopulateDataObject - line 29 : dataContext.AddDataSource(manager.DataContextType, manager.DataContextKey);
			dummy2.Z0_Description = "DUM789"; // must be different to "DUM123" - otherwise we will try to create 'duplicate' booking consolidation on Dummy record without "DUM123" as Z0_Description (due to DummyWithDtbBookingShipmentDataObjectWriter.GetDataObject() always writing DUM123 as one of the top level DataSources

			Factory.Save();

			AssertEquals(false, dummy.HasChanges);
			AssertEquals(false, dummy2.HasChanges);
			dummy.HasChanges = true;
			dummy2.HasChanges = true;
			AssertEquals(true, dummy.HasChanges);
			AssertEquals(true, dummy2.HasChanges);

			var isUserInteractive = Globals.IsUserInteractive;
			try
			{
				Globals.IsUserInteractive = false;

				var manager = new DtbDeliveryManager(Factory, new[] { dummy, dummy2 }, dummy.GetSupportedDirections()[0], false);
				var bookings = manager.DeliverTransportBooking();

				AssertNull(ZFormModaliser.LastFormShownDialogForTest);
				AssertNull(ZFormModaliser.LastCommonDialogShownDialogForTest);
				AssertNull(ZFormModaliser.LastFormShownForTest);

				AssertContainsExactElementsInAnyOrder(new[] { dummy.PK, dummy2.PK }, bookings.Select(b => ((DtbBooking)b).ConsolidationSingleJob.Parent.ParentWithWorkflow.PK));
				AssertEquals("When Delivering, always leave overridden off", false, bookings.Cast<DtbBooking>().First().ConsolidationSingleJob.KB_IsOverridden);
			}
			finally
			{
				Globals.IsUserInteractive = isUserInteractive;
			}
		}

		public void TestDeliverTransportBookingNotSaved_WorkflowNoGUI()
		{
			var dummy = Factory.New<DummyWithDtbBooking>();
			dummy.Z0_Description = "DUM456";

			var isUserInteractive = Globals.IsUserInteractive;
			try
			{
				Globals.IsUserInteractive = false;

				var manager = new DtbDeliveryManager(Factory, new[] { dummy }, dummy.GetSupportedDirections()[0], false);
				manager.DeliverTransportBooking();
				AssertEquals("Cannot deliver, the parent object Dummy Business Object DUM456 is not in database.", ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			}
			finally
			{
				Globals.IsUserInteractive = isUserInteractive;
			}
		}

		public void TestDeliverTransportBooking_NoGUI_FCL()
		{
			var dummy = Factory.New<DummyWithDtbBooking>();
			var dummy2 = Factory.New<DummyWithDtbBooking>();
			dummy.Z0_Description = "DUM456"; // required as this is the source object key, DataContextDataObjectWriter.PopulateDataObject - line 29 : dataContext.AddDataSource(manager.DataContextType, manager.DataContextKey);
			dummy2.Z0_Description = "DUM789"; // must be different to "DUM123" - otherwise we will try to create 'duplicate' booking consolidation on Dummy record without "DUM123" as Z0_Description (due to DummyWithDtbBookingShipmentDataObjectWriter.GetDataObject() always writing DUM123 as one of the top level DataSources

			Factory.Save();

			using (var dummyWriterDecider = ObjectFactory.Get<IDtbParentInfoLoader>().SetDummyWriterDecider(true))
			{
				var isUserInteractive = Globals.IsUserInteractive;
				try
				{
					Globals.IsUserInteractive = false;

					var manager = new DtbDeliveryManager(Factory, new[] { dummy, dummy2 }, dummy.GetSupportedDirections()[0], false);
					var bookings = manager.DeliverTransportBooking();

					AssertNull(ZFormModaliser.LastFormShownDialogForTest);
					AssertNull(ZFormModaliser.LastCommonDialogShownDialogForTest);
					AssertNull(ZFormModaliser.LastFormShownForTest);

					AssertEquals("Each booking has 2 containers, in all should create 4 bookings", 4, bookings.Count());
					AssertContainsExactElementsInAnyOrder(new[] { dummy.PK, dummy2.PK, dummy.PK, dummy2.PK }, bookings.Select(b => ((DtbBooking)b).ConsolidationSingleJob.Parent.ParentWithWorkflow.PK));
					AssertEquals("When Delivering, always leave overridden off", false, bookings.Cast<DtbBooking>().First().ConsolidationSingleJob.KB_IsOverridden);
				}
				finally
				{
					Globals.IsUserInteractive = isUserInteractive;
				}
			}
		}

		public void TestDeliverTransportBooking_Shipment()
		{
			var helper = new TransportBookingTestHelper(Factory);
			var bookingParent = helper.CreateForwardingShipment("S00001234", "", "SEA", "FCL") as IDtbBookingParent;
			var workflowProvider = bookingParent as IWorkflowProvider;
			workflowProvider.Logs.AddNew(Events.CustomisableEvent00, ZDateTimeOffset.Now.AddMonths(1));
			Factory.Save();

			var template = Factory.CreateNewFactory().NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode;
			var milestone = template.WorkflowItems.Milestones.AddNew();
			milestone.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			template.Factory.Save();

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;

			using (var form = new ZForm_Testo())
			using (var control = new ZCheckBox())
			using (helper.SetRegistryItem_PreventMilestoneFutureActualStart(true))
			{
				form.Controls.Add(control);
				form.BindingSource_Exposed.SetBindingMember(control, JobShipmentSchema.JS_IsBooking.Name);
				form.SetDataBinding(bookingParent as BusinessObject, "");

				form.Show();
				Application.DoEvents();

				var manager = new DtbDeliveryManager(Factory, new[] { bookingParent }, bookingParent.GetSupportedDirections()[0], false);
				AssertEquals("Template has not been applied yet", 0, workflowProvider.WorkflowItems.Milestones.Count);
				AssertEquals("This exception only happens on bound bizos", true, (bookingParent as BusinessObject).IsBound);
				AssertNoExceptionThrown(() => manager.DeliverTransportBooking());
				var templateLoader = new ProcessTask.Loader(Factory);
				templateLoader.CreateTasksAndMilestonesFromTemplateIfRequired(workflowProvider, TemplateApplicationParameters.ApplyIgnoreHasChanges());
				AssertEquals(1, workflowProvider.WorkflowItems.Milestones.Count);
			}
		}

		class ZForm_Testo : ZForm
		{
			internal KBindingSource BindingSource_Exposed => BindingSource;
		}

		public void TestDeliverTransportBooking_NoGUI_FCL_SingleParent_WithConsolidationOverridden()
		{
			var dummy = Factory.New<DummyWithDtbBooking>();
			dummy.Z0_Description = "DUM456"; // required as this is the source object key, DataContextDataObjectWriter.PopulateDataObject - line 29 : dataContext.AddDataSource(manager.DataContextType, manager.DataContextKey);
			var consolidation = Helper.CreateConsolidation(dummy);
			consolidation.KB_IsOverridden = true;
			Factory.Save();

			using (ObjectFactory.Get<IDtbParentInfoLoader>().SetDummyWriterDecider(true))
			{
				var isUserInteractive = Globals.IsUserInteractive;
				long initialFormConstructedCount = TestingState.FormConstructedCount;

				try
				{
					Globals.IsUserInteractive = false;

					var manager = new DtbDeliveryManager(Factory, new[] { dummy }, dummy.GetSupportedDirections()[0], false);
					AssertNoExceptionThrown("No exception is thrown.", () => manager.DeliverTransportBooking());
					AssertEquals("No new forms are constructed by the DeliverTransportBooking call.", initialFormConstructedCount, TestingState.FormConstructedCount);
					AssertNull(ZFormModaliser.LastFormShownDialogForTest);
					AssertNull(ZFormModaliser.LastCommonDialogShownDialogForTest);
					AssertNull(ZFormModaliser.LastFormShownForTest);
				}
				finally
				{
					Globals.IsUserInteractive = isUserInteractive;
				}
			}
		}

		public void TestDeliverTransportBooking_LocalCharges()
		{
			var shipment = Factory.New<IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "SHP123456"; // required as this is the source object key, DataContextDataObjectWriter.PopulateDataObject - line 29 : dataContext.AddDataSource(manager.DataContextType, manager.DataContextKey);

			var code = Factory.NewWithValidTestData<AccChargeCode>();
			code.AC_Code = "TST";
			code.AC_Desc = "test charege code";
			code.AC_GC = GlbCompany.CurrentCompany.PK;

			var job = new JobHeader.Loader((IJobHeaderParent)shipment).TryLoadOrCreate();
			job.JH_GE = Factory.LoadTop1<GlbDepartment>(new ZQuery()).PK;

			var charge = Factory.New<JobCharge>();
			charge.JR_JH = job.PK;
			charge.JR_GE = job.JH_GE;
			charge.JR_GB = job.JH_GB;
			charge.JR_AC = code.PK;
			charge.JR_LocalSellAmt = 1;
			charge.JR_OSSellAmt = 1;

			Factory.Save();

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes; // options form
			ZFormModaliser.ShowDialogsInTest = true;
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
			{
				var formAsDocumentForm = ((TransportBookingDocumentForm)form);
				formAsDocumentForm.DeliveryOptionsResultForTesting = TransportBookingDocumentOptions.DeliveryOption.AutoDelivery;
			});

			var manager = new DtbDeliveryManager(Factory, (IDtbBookingParent)shipment, DtbBookingDirection.PIC, false);
			var bookings = manager.DeliverTransportBooking();
			var booking = (DtbBooking)bookings.First();
			AssertEquals(shipment.PK, booking.ConsolidationSingleJob.Parent.ParentWithWorkflow.PK);

			var bookingJobHeader = Factory.LoadTop1<JobHeader>(new ZQuery(JobHeaderSchema.JH_ParentID, booking.PK).AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK));
			AssertNull(bookingJobHeader);
		}

		public void TestDeliverTransportBooking_WithErrorsFromUniversalXMLForSingle()
		{
			var dummy = Factory.New<DummyWithDtbBooking>();
			dummy.Z0_Description = "DUM456";
			Factory.Save();

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes; // options form
			ZFormModaliser.ShowDialogsInTest = true;

			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
			{
				var formAsDocumentForm = ((TransportBookingDocumentForm)form);
				formAsDocumentForm.DeliveryOptionsResultForTesting = TransportBookingDocumentOptions.DeliveryOption.OpenStandardDesigner;
			});

			var manager = new DtbDeliveryManager(Factory, new[] { dummy }, dummy.GetSupportedDirections()[0], false);
			manager.SetErrorMessageForTest = true;
			AssertNoExceptionThrown(() => manager.DeliverTransportBooking());
		}

		public void TestCreateBooking_UniversalResultIsNull()
		{
			var dummy = Factory.New<DummyWithDtbBooking>();
			dummy.Z0_Description = "DUM456";
			Factory.Save();

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes; // options form
			ZFormModaliser.ShowDialogsInTest = true;
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
			{
				var formAsDocumentForm = ((TransportBookingDocumentForm)form);
				formAsDocumentForm.DeliveryOptionsResultForTesting = TransportBookingDocumentOptions.DeliveryOption.OpenStandardDesigner;
			});

			var manager = new DtbDeliveryManager(Factory, new[] { dummy }, dummy.GetSupportedDirections()[0], false);
			manager.SetResultNullForTest = true;
			manager.DeliverTransportBooking();
			AssertEquals("Failed to Save Transport Booking due to Unknown Error", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestDeliverTransportBooking_DeliversMultipleBookings_WhenParentContainsMultipleContainersWithNoContainerNumber()
		{
			var setupFactory = new BusinessObjectFactory();
			var helper = new TransportBookingTestHelper(setupFactory);
			var shipmentBO = helper.CreateForwardingShipment("S00001234", "", "SEA", "FCL");
			var consolBO = helper.CreateForwardingConsol(shipmentBO, "C00001432", "SEA", "");

			var container1 = helper.CreateForwardingContainer(consolBO, null, "20GP", releaseNum: "RELEASENUM1");
			var packlineBO1 = helper.CreateForwardingPackline(shipmentBO, container1, 10);

			var container2 = helper.CreateForwardingContainer(consolBO, null, "20GP", releaseNum: "RELEASENUM2");
			var packlineBO2 = helper.CreateForwardingPackline(shipmentBO, container2, 10);

			setupFactory.Save();

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes; // options form
			ZFormModaliser.ShowDialogsInTest = true;
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
			{
				var formAsDocumentForm = form as TransportBookingDocumentForm;
				if (formAsDocumentForm != null)
				{
					formAsDocumentForm.DeliveryOptionsResultForTesting = TransportBookingDocumentOptions.DeliveryOption.AutoDelivery;
				}
			});

			var parent = (BusinessObject)Factory.Load<IForwardingShipment>(shipmentBO.PK);

			var manager = new DtbDeliveryManager(Factory, (IDtbBookingParent)parent, DtbBookingDirection.PIC, false);
			var bookings = manager.DeliverTransportBooking();

			AssertEquals(2, bookings.Count());
			AssertContainsExactElementsInAnyOrder(new[] { "RELEASENUM1", "RELEASENUM2" }, bookings.Select(p => ((DtbBooking)p).DeliveryConfirmations.Single().KK_ReferenceNum));
		}

		public void TestDeliverTransportBooking_WithErrorsFromUniversalXMLForMultiple()
		{
			var dummy = Factory.New<DummyWithDtbBooking>();
			var dummy2 = Factory.New<DummyWithDtbBooking>();
			dummy.Z0_Description = "DUM456";
			dummy2.Z0_Description = "DUM123";
			Factory.Save();

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes; // options form
			ZFormModaliser.ShowDialogsInTest = true;

			var manager = new DtbDeliveryManager(Factory, new[] { dummy, dummy2 }, dummy.GetSupportedDirections()[0], false);
			manager.SetErrorMessageForTest = true;
			AssertNoExceptionThrown(() => manager.DeliverTransportBooking());
		}

		public void TestDeliverTransportBooking_BookingAlreadyOpen()
		{
			var dummy = Factory.New<DummyWithDtbBooking>();
			dummy.Z0_Description = "DUM456";
			var consolidation = Helper.CreateConsolidation(dummy);
			consolidation.KB_IsOverridden = false;
			var booking = Helper.CreateBooking(consolidation);
			Factory.Save();

			var expectedMessage = @"Cannot prepare new Cartage Advice as an existing Transport Booking screen is open.

Close the Transport Booking screen and try again.";

			// multi booking form already open
			var controller = ZControllerFactory.Create(ControllerIDs.DtbBookingConsolidation);
			var manager = new DtbDeliveryManager(Factory, new[] { dummy }, dummy.GetSupportedDirections()[0], false);
			IEnumerable<IDtbBooking> bookings = null;
			using (var multiForm = (TransportBookingMultiForm)controller.ShowEditForm(consolidation))
			{
				AssertEquals(true, multiForm.Visible);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes; // options form
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
				{
					var formAsDocumentForm = ((TransportBookingDocumentForm)form);
					formAsDocumentForm.DeliveryOptionsResultForTesting = TransportBookingDocumentOptions.DeliveryOption.OpenStandardDesigner;
				});

				bookings = manager.DeliverTransportBooking();
				Assert(UnitTestUserNotification.Instance.LastMessage.Text.Contains(expectedMessage));
			}

			// single booking form already open
			controller = ZControllerFactory.Create(ControllerIDs.DtbBooking);
			using (var singleForm = (TransportBookingForm)controller.ShowEditForm(booking))
			{
				AssertEquals(true, singleForm.Visible);
				bookings = manager.DeliverTransportBooking();
				Assert(UnitTestUserNotification.Instance.LastMessage.Text.Contains(expectedMessage));
			}

			// no booking form previously open
			bookings = manager.DeliverTransportBooking();
			using (var bookingFromParentForm = manager.LastControllerForTest.LastShownForm)
			{
				AssertNotNull(bookingFromParentForm);
				var createdBooking = (DtbBooking)((TransportBookingForm)bookingFromParentForm).BusinessEntity;
				AssertEquals(createdBooking.ConsolidationSingleJob.PK, consolidation.PK);    // same consolidation
				AssertEquals(2, createdBooking.ConsolidationSingleJob.Bookings.Count);   // should have 2 bookings (just created a new booking)
			}
		}

		public void TestDeliverTransportBooking_MultipleParents_BookingAlreadyOpen()
		{
			var dummy = Factory.New<DummyWithDtbBooking>();
			var dummy2 = Factory.New<DummyWithDtbBooking>();
			dummy.Z0_Description = "DUM456"; // required as this is the source object key, DataContextDataObjectWriter.PopulateDataObject - line 29 : dataContext.AddDataSource(manager.DataContextType, manager.DataContextKey);
			dummy2.Z0_Description = "DUM123";
			var consolidation = Helper.CreateConsolidation(dummy);
			consolidation.KB_IsOverridden = true;
			var booking = Helper.CreateBooking(consolidation);

			var consolidation2 = Helper.CreateConsolidation(dummy2);
			consolidation2.KB_IsOverridden = false;
			var booking2 = Helper.CreateBooking(consolidation2);

			Factory.Save();

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes; // options form
			ZFormModaliser.ShowDialogsInTest = true;

			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
			{
				var formAsDocumentForm = ((TransportBookingDocumentForm)form);
				formAsDocumentForm.DeliveryOptionsResultForTesting = TransportBookingDocumentOptions.DeliveryOption.AutoDelivery;
			});

			// single booking form already open
			var controller = ZControllerFactory.Create(ControllerIDs.DtbBooking);
			using (var singleForm = (TransportBookingForm)controller.ShowEditForm(booking2))
			{
				AssertEquals(true, singleForm.Visible);

				var manager = new DtbDeliveryManager(Factory, new[] { dummy, dummy2 }, dummy.GetSupportedDirections()[0], false);
				var bookings = manager.DeliverTransportBooking();
				AssertEquals("Should not deliver any bookings when a form is open", 0, bookings.Count());
			}
		}

		public void TestDeliverTransportBooking_CreditControlledDocumentDelivery()
		{
			var shipment = Factory.New<IForwardingShipment>();
			Factory.Save();

			int calledCount = 0;
			(shipment as ICreditControlledDocumentDelivery).GetDocumentLogin += (sender, e) => calledCount++;

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes; // options form
			ZFormModaliser.ShowDialogsInTest = true;
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
			{
				var formAsDocumentForm = ((TransportBookingDocumentForm)form);
				formAsDocumentForm.DeliveryOptionsResultForTesting = TransportBookingDocumentOptions.DeliveryOption.AutoDelivery;
			});

			var manager = new DtbDeliveryManager(Factory, (IDtbBookingParent)shipment, DtbBookingDirection.PIC, false);
			var booking = manager.DeliverTransportBooking().First();

			var currentCount = calledCount;
			(booking as ICreditControlledDocumentDelivery).RaiseOnGetDocumentLogin(null);
			AssertEquals(currentCount + 1, calledCount);
		}

		public void TestDeliverTransportBooking_BookingInstructionsCanRefreshFromDb()
		{
			var shipment = Factory.New<IForwardingShipment>();

			Factory.Save();

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes; // options form
			ZFormModaliser.ShowDialogsInTest = true;
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
			{
				var formAsDocumentForm = ((TransportBookingDocumentForm)form);
				formAsDocumentForm.DeliveryOptionsResultForTesting = TransportBookingDocumentOptions.DeliveryOption.AutoDelivery;
			});

			var originalManager = new DtbDeliveryManager(Factory, (IDtbBookingParent)shipment, DtbBookingDirection.DLV, false);

			// Create original transport booking, and access the pickup instruction address,
			// so that the transport booking, pickup instruction, and the pickup instruction address will be cached in the factory
			var originalBooking = originalManager.DeliverTransportBooking().First();
			var originalPickupInstruction = originalBooking.Instructions.Single(x => x.KN_InstructionType == InstructionTypes.Codes.PickUp);
			AssertEquals("Precondition: Access pickup instruction address, and confirm it is null", ZGuid.Empty, originalPickupInstruction.Address.E2_OA_Address);

			// this change is necessary because without it the unit test fails as my change to production has meant that the creation of TB now updates shipment,
			// which means that without a refresh on the shipment the code would throw a ZSaveConcurrencyException.
			// Hence we force shipment to get a clean copy from the database
			var shipmentPK = shipment.PK;
			var query = new ZDBOnlyQuery(typeof(IForwardingShipment));
			query.AddToFilter(JobShipmentSchema.PK, shipmentPK);
			query.ReLoadExistingRows = true;
			shipment = Factory.LoadTop1<IForwardingShipment>(query);

			OrgAddress unpackdepotAddress = Factory.NewWithValidTestData<OrgAddress>();
			unpackdepotAddress.OA_Address1 = "TEST UNPACK DEPOT ADDRESS";
			unpackdepotAddress.OA_RN_NKCountryCode = "AU";
			shipment.JS_OA_ImportReleaseDepot = unpackdepotAddress.PK;

			Factory.Save();

			// Use a new delivery manager, because the original delivery manager cannot access cached information about the transport booking
			var newManager = new DtbDeliveryManager(Factory, (IDtbBookingParent)shipment, DtbBookingDirection.DLV, false);
			var newBooking = newManager.DeliverTransportBooking().First();

			((DtbBooking)newBooking).Instructions.RefreshFromDb();

			var reloadedBookingInstruction = newBooking.Instructions.Single(x => x.KN_InstructionType == InstructionTypes.Codes.PickUp);
			AssertNotEquals("Booking instruction does not change when refreshed", originalPickupInstruction.PK, reloadedBookingInstruction.PK);
			AssertEquals("Address of booking instruction is not set correctly after refreshing of instruction", unpackdepotAddress.PK, reloadedBookingInstruction.Address.E2_OA_Address);
		}

		public void TestDeliverTransportBooking_WhenRepeatedlyDeliveredAndEdited_DoesNotCauseConcurrencyError()
		{
			var shipment = Factory.New<IForwardingShipment>();

			Factory.Save();

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes; // options form
			ZFormModaliser.ShowDialogsInTest = true;
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
			{
				var formAsDocumentForm = ((TransportBookingDocumentForm)form);
				formAsDocumentForm.DeliveryOptionsResultForTesting = TransportBookingDocumentOptions.DeliveryOption.AutoDelivery;
			});

			var originalManager = new DtbDeliveryManager(Factory, (IDtbBookingParent)shipment, DtbBookingDirection.DLV, false);
			_ = originalManager.DeliverTransportBooking().First().Instructions;
			Factory.Save();

			var newManager = new DtbDeliveryManager(Factory, (IDtbBookingParent)shipment, DtbBookingDirection.DLV, false);
			newManager.DeliverTransportBooking();

			AssertNoExceptionThrown("Repeatedly delivering and editing a transport booking causes an internal error",
				() => Factory.Save()
			);
		}

		public void TestDeliverTransportBooking_WhenRepeatedlyDelivered_DoesNotLoseInstructions()
		{
			var shipment = Factory.New<IForwardingShipment>();

			OrgAddress unpackdepotAddress = Factory.NewWithValidTestData<OrgAddress>();
			unpackdepotAddress.OA_Address1 = "TEST UNPACK DEPOT ADDRESS";
			unpackdepotAddress.OA_RN_NKCountryCode = "AU";
			shipment.JS_OA_ImportReleaseDepot = unpackdepotAddress.PK;

			Factory.Save();

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes; // options form
			ZFormModaliser.ShowDialogsInTest = true;
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
			{
				var formAsDocumentForm = ((TransportBookingDocumentForm)form);
				formAsDocumentForm.DeliveryOptionsResultForTesting = TransportBookingDocumentOptions.DeliveryOption.AutoDelivery;
			});

			var originalManager = new DtbDeliveryManager(Factory, (IDtbBookingParent)shipment, DtbBookingDirection.DLV, false);
			_ = originalManager.DeliverTransportBooking().First().Instructions;

			var newManager = new DtbDeliveryManager(Factory, (IDtbBookingParent)shipment, DtbBookingDirection.DLV, false);
			var newBooking = newManager.DeliverTransportBooking().First();

			var newBookingInstruction = newBooking.Instructions.Single(x => x.KN_InstructionType == InstructionTypes.Codes.PickUp);
			AssertEquals("Instructions lost during second delivery", unpackdepotAddress.PK, newBookingInstruction.Address.E2_OA_Address);
		}

		public void TestDeliverTransportBooking_Containerised_OLDNamespace()
		{
			TestDeliverTransportBooking_Containerised();
		}

		public void TestDeliverTransportBooking_Containerised_2012Namespace()
		{
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2012_11))
			{
				TestDeliverTransportBooking_Containerised();
			}
		}

		void TestDeliverTransportBooking_Containerised()
		{
			var shipmentBO = GetSavedForwardingShipment();

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes; // options form
			ZFormModaliser.ShowDialogsInTest = true;
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
			{
				var formAsDocumentForm = form as TransportBookingDocumentForm;
				if (formAsDocumentForm != null)
				{
					formAsDocumentForm.DeliveryOptionsResultForTesting = TransportBookingDocumentOptions.DeliveryOption.AutoDelivery;
				}
			});

			var manager = new DtbDeliveryManager(Factory, (IDtbBookingParent)shipmentBO, DtbBookingDirection.PIC, false);
			var bookings = manager.DeliverTransportBooking();
			var booking = (DtbBooking)bookings.First();
			AssertEquals(shipmentBO.PK, booking.ConsolidationSingleJob.Parent.ParentWithWorkflow.PK);

			var packages = booking.ConsolidationSingleJob.PackageJob.Packages;
			AssertEquals(1, packages.Count);

			var container = packages[0];
			AssertEquals("CNT", container.KP_F3_NKPackType);
			AssertEquals("CONT123", container.KP_PackageID);
			AssertEquals(1, container.Packages.Count);
			AssertEquals(10, container.Packages[0].KP_PackageQty);
			AssertContainsExactElementsInAnyOrder(new[] { container }, booking.AssignedPackages);
		}

		BusinessObject GetSavedForwardingShipment()
		{
			var setupFactory = new BusinessObjectFactory();
			var helper = new TransportBookingTestHelper(setupFactory);
			var shipmentBO = helper.CreateForwardingShipment("S00001234", "", "SEA", "FCL");
			var consolBO = helper.CreateForwardingConsol(shipmentBO, "C00001432", "SEA", "");
			var containerBO = helper.CreateForwardingContainer(consolBO, "CONT123", "20GP");
			var packlineBO = helper.CreateForwardingPackline(shipmentBO, containerBO, 10);
			setupFactory.Save();

			return (BusinessObject)Factory.Load<IForwardingShipment>(shipmentBO.PK);
		}

		public void TestDeliverTransportBooking_ParentInactive_TransportBookingsExist_GetReturned()
		{
			TestDeliverTransportBooking_ParentInactive_TransportBookingsExist_GetReturned_Core(false);
		}

		public void TestDeliverTransportBooking_ParentInactive_TransportBookingsExist_GetReturned_WithLoggerAndErrorManager()
		{
			TestDeliverTransportBooking_ParentInactive_TransportBookingsExist_GetReturned_Core(true);
		}

		void TestDeliverTransportBooking_ParentInactive_TransportBookingsExist_GetReturned_Core(bool withLoggerAndErrorManager)
		{
			var dummy = Factory.New<DummyWithDtbBooking>();
			dummy.Z0_Description = "DUM456"; // required as this is the source object key, DataContextDataObjectWriter.PopulateDataObject - line 29 : dataContext.AddDataSource(manager.DataContextType, manager.DataContextKey);

			var consolidation = Helper.CreateConsolidation(dummy);
			// For non-cancelled parents, KB_IsOverridden being false would mean that another booking can be created
			consolidation.KB_IsOverridden = false;
			var booking = Helper.CreateBooking(consolidation);
			dummy.IsCancelled = true;

			Factory.Save();

			var userAskedDeliveryOption = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes; // options form
			ZFormModaliser.ShowDialogsInTest = true;
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
			{
				var formAsDocumentForm = ((TransportBookingDocumentForm)form);
				userAskedDeliveryOption = userAskedDeliveryOption || formAsDocumentForm != null;
				formAsDocumentForm.DeliveryOptionsResultForTesting = TransportBookingDocumentOptions.DeliveryOption.AutoDelivery;
			});

			ILogger logger = null;
			IAutomatedDtbBookingCreationErrorManager errorManager = null;
			if (withLoggerAndErrorManager)
			{
				logger = new TestServiceLogger();
				errorManager = NewAutomatedErrorManager();
			}
			var manager = new DtbDeliveryManager(Factory, new[] { dummy }, dummy.GetSupportedDirections()[0], false, logger, errorManager);
			var bookings = manager.DeliverTransportBooking();

			CombineAssertions(() =>
			{
				AssertEquals("Should not be asked delivery option", false, userAskedDeliveryOption);
				AssertEquals("Should deliver the existing booking", booking.PK, ((BusinessObject)bookings.Single()).PK);
				AssertEquals("Should not provide error message to user", null, UnitTestUserNotification.Instance.LastMessage.Text);
			});

			if (withLoggerAndErrorManager)
			{
				AssertEquals("Should have logged no errors", string.Empty, logger.ToString());
				AssertEquals("Should have collected no errors", 0, errorManager.ErrorList.Count);
			}
		}

		public void TestDeliverTransportBooking_ParentInactive_NoTransportBookings_ReturnNothing()
		{
			TestDeliverTransportBooking_ParentInactive_NoTransportBookings_ReturnNothing_Core(false);
		}

		public void TestDeliverTransportBooking_ParentInactive_NoTransportBookings_ReturnNothing_WithLoggerAndErrorManager()
		{
			TestDeliverTransportBooking_ParentInactive_NoTransportBookings_ReturnNothing_Core(true);
		}

		void TestDeliverTransportBooking_ParentInactive_NoTransportBookings_ReturnNothing_Core(bool withLoggerAndErrorManager)
		{
			var dummy = Factory.New<DummyWithDtbBooking>();
			dummy.Z0_Description = "DUM456"; // required as this is the source object key, DataContextDataObjectWriter.PopulateDataObject - line 29 : dataContext.AddDataSource(manager.DataContextType, manager.DataContextKey);
			dummy.IsCancelled = true;

			Factory.Save();

			var userAskedDeliveryOption = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes; // options form
			ZFormModaliser.ShowDialogsInTest = true;
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
			{
				var formAsDocumentForm = ((TransportBookingDocumentForm)form);
				userAskedDeliveryOption = userAskedDeliveryOption || formAsDocumentForm != null;
				formAsDocumentForm.DeliveryOptionsResultForTesting = TransportBookingDocumentOptions.DeliveryOption.AutoDelivery;
			});

			ILogger logger = null;
			IAutomatedDtbBookingCreationErrorManager errorManager = null;
			if (withLoggerAndErrorManager)
			{
				logger = new TestServiceLogger();
				errorManager = NewAutomatedErrorManager();
			}
			var manager = new DtbDeliveryManager(Factory, new[] { dummy }, dummy.GetSupportedDirections()[0], false, logger, errorManager);
			var bookings = manager.DeliverTransportBooking();
			var expectedError = "Dummy Business Object DUM456 is deactivated and cannot create new Transport Bookings.";

			CombineAssertions(() =>
			{
				AssertEquals("Should not be asked delivery option", false, userAskedDeliveryOption);
				AssertEquals("Should not deliver anything", false, bookings.Any());
				AssertEquals("Should provide error message to user", expectedError, UnitTestUserNotification.Instance.LastMessage.Text);
			});

			if (withLoggerAndErrorManager)
			{
				var expectedLog = "Error|Failed to Create Transport Booking:" + System.Environment.NewLine + expectedError +
	System.Environment.NewLine;
				AssertEquals("Should log errors", expectedLog, logger.ToString());
				AssertEquals("Should be 1 error in error manager list", 1, errorManager.ErrorList.Count);
				var errorManagerError = errorManager.ErrorList.Single();
				AssertEquals("Should have collected correct error - CancelledParentError", DtbBookingCreationErrorType.CancelledParentError, errorManagerError.ErrorType);
				AssertEquals("Collected error should have correct message", expectedError, errorManagerError.Message);
			}
		}

		public void TestDeliverTransportBooking_ParentInactive_Consolidation_NoTransportBookings_ReturnNothing()
		{
			TestDeliverTransportBooking_ParentInactive_Consolidation_NoTransportBookings_ReturnNothing_Core(false);
		}

		public void TestDeliverTransportBooking_ParentInactive_Consolidation_NoTransportBookings_ReturnNothing_WithLoggerAndErrorManager()
		{
			TestDeliverTransportBooking_ParentInactive_Consolidation_NoTransportBookings_ReturnNothing_Core(true);
		}

		// Same as TestDeliverTransportBooking_ParentInactive_NoTransportBookings_ReturnNothing_Core, except this one has a consolidation with no TBs, as opposed to no consolidation
		void TestDeliverTransportBooking_ParentInactive_Consolidation_NoTransportBookings_ReturnNothing_Core(bool withLoggerAndErrorManager)
		{
			var dummy = Factory.New<DummyWithDtbBooking>();
			dummy.Z0_Description = "DUM456"; // required as this is the source object key, DataContextDataObjectWriter.PopulateDataObject - line 29 : dataContext.AddDataSource(manager.DataContextType, manager.DataContextKey);

			var consolidation = Helper.CreateConsolidation(dummy);
			dummy.IsCancelled = true;

			Factory.Save();

			var userAskedDeliveryOption = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes; // options form
			ZFormModaliser.ShowDialogsInTest = true;
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
			{
				var formAsDocumentForm = ((TransportBookingDocumentForm)form);
				userAskedDeliveryOption = userAskedDeliveryOption || formAsDocumentForm != null;
				formAsDocumentForm.DeliveryOptionsResultForTesting = TransportBookingDocumentOptions.DeliveryOption.AutoDelivery;
			});

			ILogger logger = new TestServiceLogger();
			IAutomatedDtbBookingCreationErrorManager errorManager = NewAutomatedErrorManager();
			var manager = new DtbDeliveryManager(Factory, new[] { dummy }, dummy.GetSupportedDirections()[0], false, logger, errorManager);
			var bookings = manager.DeliverTransportBooking();

			CombineAssertions(() =>
			{
				AssertEquals("Should not be asked delivery option", false, userAskedDeliveryOption);
				AssertEquals("Should not deliver anything", false, bookings.Any());
				AssertEquals("Should provide error message to user", "Dummy Business Object DUM456 is deactivated and cannot create new Transport Bookings.", UnitTestUserNotification.Instance.LastMessage.Text);
			});
		}

		public void TestDeliverTransportBooking_MultipleParents_ParentInactive_TransportBookingsExist_GetReturned()
		{
			var dummy = Factory.New<DummyWithDtbBooking>();
			var dummy2 = Factory.New<DummyWithDtbBooking>();
			dummy.Z0_Description = "DUM456"; // required as this is the source object key, DataContextDataObjectWriter.PopulateDataObject - line 29 : dataContext.AddDataSource(manager.DataContextType, manager.DataContextKey);
			dummy2.Z0_Description = "DUM123";
			var consolidation = Helper.CreateConsolidation(dummy);
			consolidation.KB_IsOverridden = true; // Ensure the user is not asked delivery option for active dummy's bookings, and just return its existing booking
			var booking = Helper.CreateBooking(consolidation);

			var consolidation2 = Helper.CreateConsolidation(dummy2);
			consolidation2.KB_IsOverridden = false; // If it weren't for dummy2 being cancelled, user would be asked delivery option for dummy2's bookings
			var booking2 = Helper.CreateBooking(consolidation2);
			// Create an inactive booking that shouldn't be returned
			var inactiveBooking = Helper.CreateBooking(consolidation2);
			inactiveBooking.KM_IsActive = false;
			dummy2.IsCancelled = true;

			Factory.Save();

			var userAskedDeliveryOption = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes; // options form
			ZFormModaliser.ShowDialogsInTest = true;
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
			{
				var formAsDocumentForm = ((TransportBookingDocumentForm)form);
				userAskedDeliveryOption = userAskedDeliveryOption || formAsDocumentForm != null;
				formAsDocumentForm.DeliveryOptionsResultForTesting = TransportBookingDocumentOptions.DeliveryOption.AutoDelivery;
			});

			var manager = new DtbDeliveryManager(Factory, new[] { dummy, dummy2 }, dummy.GetSupportedDirections()[0], false);
			var bookings = manager.DeliverTransportBooking();

			CombineAssertions(() =>
			{
				AssertEquals("Should not be asked delivery option", false, userAskedDeliveryOption);
				AssertContainsExactElementsInAnyOrder("Should deliver the existing active bookings", new[] { booking.PK, booking2.PK }, bookings.Select(b => b.PK));
				AssertEquals("Should not provide error message to user", null, UnitTestUserNotification.Instance.LastMessage.Text);
			});
		}

		public void TestDeliverTransportBooking_MultipleParents_ParentInactive_NoTransportBookings_ReturnNothing()
		{
			var dummy = Factory.New<DummyWithDtbBooking>();
			var dummy2 = Factory.New<DummyWithDtbBooking>();
			var dummy3 = Factory.New<DummyWithDtbBooking>();
			var dummy4 = Factory.New<DummyWithDtbBooking>();
			dummy.Z0_Description = "DUM456"; // required as this is the source object key, DataContextDataObjectWriter.PopulateDataObject - line 29 : dataContext.AddDataSource(manager.DataContextType, manager.DataContextKey);
			dummy2.Z0_Description = "DUM123";
			dummy3.Z0_Description = "DUM789";
			dummy4.Z0_Description = "DUM987";
			var consolidation = Helper.CreateConsolidation(dummy);
			consolidation.KB_IsOverridden = true; // This will result in GetAllOptionsFromParents adding its booking to the variable called "result"
			var booking = Helper.CreateBooking(consolidation);

			// Don't create a consolidation or booking for dummy2. This will result in GetAllOptionsFromParents adding to the variable "allOptions"
			// Don't create a consolidation or booking for dummy3

			dummy3.IsCancelled = true;

			var dummy4Consolidation = Helper.CreateConsolidation(dummy4);
			dummy4Consolidation.KB_IsOverridden = true;
			var dummy4Booking = Helper.CreateBooking(dummy4Consolidation);

			Factory.Save();

			var userAskedDeliveryOption = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes; // options form
			ZFormModaliser.ShowDialogsInTest = true;
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
			{
				var formAsDocumentForm = ((TransportBookingDocumentForm)form);
				userAskedDeliveryOption = userAskedDeliveryOption || formAsDocumentForm != null;
				formAsDocumentForm.DeliveryOptionsResultForTesting = TransportBookingDocumentOptions.DeliveryOption.AutoDelivery;
			});

			var manager = new DtbDeliveryManager(Factory, new[] { dummy, dummy2, dummy3, dummy4 }, dummy.GetSupportedDirections()[0], false);
			var bookings = manager.DeliverTransportBooking();

			CombineAssertions(() =>
			{
				AssertEquals("Should not be asked delivery option", false, userAskedDeliveryOption);
				AssertEquals("Should not deliver any bookings", false, bookings.Any());
				AssertEquals("Should provide error message to user", "Dummy Business Object DUM789 is deactivated and cannot create new Transport Bookings.", UnitTestUserNotification.Instance.LastMessage.Text);
			});
		}

		public void TestDeliverTransportBooking_MultipleParents_ParentInactive_Consolidation_NoActiveBookings_ReturnNothing()
		{
			var dummy = Factory.New<DummyWithDtbBooking>();
			var dummy2 = Factory.New<DummyWithDtbBooking>();
			var dummy3 = Factory.New<DummyWithDtbBooking>();
			var dummy4 = Factory.New<DummyWithDtbBooking>();
			dummy.Z0_Description = "DUM456"; // required as this is the source object key, DataContextDataObjectWriter.PopulateDataObject - line 29 : dataContext.AddDataSource(manager.DataContextType, manager.DataContextKey);
			dummy2.Z0_Description = "DUM123";
			dummy3.Z0_Description = "DUM789";
			dummy4.Z0_Description = "DUM987";
			var consolidation = Helper.CreateConsolidation(dummy);
			consolidation.KB_IsOverridden = true; // This will result in GetAllOptionsFromParents adding its booking to the variable called "result"
			var booking = Helper.CreateBooking(consolidation);

			// Don't create a consolidation or booking for dummy2. This will result in GetAllOptionsFromParents adding to the variable "allOptions"

			var dummy3Consolidation = Helper.CreateConsolidation(dummy3);
			// Create an inactive booking that shouldn't be returned
			var dummy3Booking = Helper.CreateBooking(dummy3Consolidation);
			dummy3Booking.KM_IsActive = false;

			dummy3.IsCancelled = true;

			var dummy4Consolidation = Helper.CreateConsolidation(dummy4);
			dummy4Consolidation.KB_IsOverridden = true;
			var dummy4Booking = Helper.CreateBooking(dummy4Consolidation);

			Factory.Save();

			var userAskedDeliveryOption = false;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes; // options form
			ZFormModaliser.ShowDialogsInTest = true;
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
			{
				var formAsDocumentForm = ((TransportBookingDocumentForm)form);
				userAskedDeliveryOption = userAskedDeliveryOption || formAsDocumentForm != null;
				formAsDocumentForm.DeliveryOptionsResultForTesting = TransportBookingDocumentOptions.DeliveryOption.AutoDelivery;
			});

			var manager = new DtbDeliveryManager(Factory, new[] { dummy, dummy2, dummy3, dummy4 }, dummy.GetSupportedDirections()[0], false);
			var bookings = manager.DeliverTransportBooking();

			CombineAssertions(() =>
			{
				AssertEquals("Should not be asked delivery option", false, userAskedDeliveryOption);
				AssertEquals("Should not deliver any bookings", false, bookings.Any());
				AssertEquals("Should provide error message to user", "Dummy Business Object DUM789 is deactivated and cannot create new Transport Bookings.", UnitTestUserNotification.Instance.LastMessage.Text);
			});
		}

		public void TestDeliverTransportBooking_ConsolidationShouldLinkToBookingParentPKAndTableCodeOfParent()
		{
			var dummy = Factory.New<DummyWithDtbBooking>();
			dummy.Z0_Description = "DUM456";

			var bookingParentPK = new ZGuid("e0eeda18-3a15-4e4a-8e19-328c4f40c092");
			var bookingParentTablePrefix = "JS";

			dummy.BookingParentPK = bookingParentPK;
			dummy.BookingParentTablePrefix = bookingParentTablePrefix;

			Factory.Save();

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes; // options form
			ZFormModaliser.ShowDialogsInTest = true;
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
			{
				var formAsDocumentForm = ((TransportBookingDocumentForm)form);
				formAsDocumentForm.DeliveryOptionsResultForTesting = TransportBookingDocumentOptions.DeliveryOption.AutoDelivery;
			});

			var manager = new DtbDeliveryManager(Factory, new[] { dummy }, dummy.GetSupportedDirections()[0], false);
			var bookings = manager.DeliverTransportBooking();

			AssertEquals("One IDtbBooking should have been returned.", 1, bookings.Count());

			var consolidation = ((DtbBooking)bookings.First()).ConsolidationSingleJob;

			CombineAssertions(() =>
			{
				AssertEquals("KB_ParentID of the Consolidation should be the BookingParentPK of the Booking Parent.", bookingParentPK, consolidation.KB_ParentID);
				AssertEquals("KB_ParentTableCode of the Consolidation should be the BookingParentTablePrefix of the Booking Parent.", bookingParentTablePrefix, consolidation.KB_ParentTableCode);
			});
		}

		public void TestDeliveryTransportBooking_Transhipment()
		{
			var dummy = Factory.New<DummyWithDtbBooking>();
			var container1 = new Container(DefaultDataObjectWriterStrategy.TestInstance) { Link = 1 };
			var container2 = new Container(DefaultDataObjectWriterStrategy.TestInstance) { Link = 2 };
			var container3 = new Container(DefaultDataObjectWriterStrategy.TestInstance) { Link = 3 };
			var packingLine1 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { ContainerLink = 1 };
			var packingLine2 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { ContainerLink = 2 };
			var consol = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			var parentShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			parentShipment.SetPackingLineCollection(() => new DataObjectList<PackingLine> { packingLine1, packingLine2 });

			consol.SetContainerCollection(() => new DataObjectList<Container>());
			consol.ContainerCollection.Add(container1);
			consol.ContainerCollection.Add(container2);

			parentShipment.SetContainerCollection(() => new DataObjectList<Container>());
			parentShipment.ContainerCollection.Add(container1);
			parentShipment.ContainerCollection.Add(container2);
			parentShipment.ContainerCollection.Add(container3);

			var manager = new DtbDeliveryManager(Factory, new[] { dummy }, dummy.GetSupportedDirections()[0], false);
			var options = manager.GetOptionsForTest(dummy, parentShipment, consol);
			var containers = options.Containers;

			AssertEquals("Only consol's containers should be included", 2, containers.Count);
		}

		public void TestDeliveryTransportBooking_Transhipment_NullLink()
		{
			var dummy = Factory.New<DummyWithDtbBooking>();
			var container1 = new Container(DefaultDataObjectWriterStrategy.TestInstance) { Link = 1 };
			var container2 = new Container(DefaultDataObjectWriterStrategy.TestInstance) { Link = 2 };
			var container3 = new Container(DefaultDataObjectWriterStrategy.TestInstance) { Link = null };
			var packingLine1 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { ContainerLink = 1 };
			var packingLine2 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { ContainerLink = 2 };
			var consol = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			var parentShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			parentShipment.SetPackingLineCollection(() => new DataObjectList<PackingLine> { packingLine1, packingLine2 });

			consol.SetContainerCollection(() => new DataObjectList<Container>());
			consol.ContainerCollection.Add(container1);
			consol.ContainerCollection.Add(container2);

			parentShipment.SetContainerCollection(() => new DataObjectList<Container>());
			parentShipment.ContainerCollection.Add(container1);
			parentShipment.ContainerCollection.Add(container2);
			parentShipment.ContainerCollection.Add(container3);

			var manager = new DtbDeliveryManager(Factory, new[] { dummy }, dummy.GetSupportedDirections()[0], false);
			var options = manager.GetOptionsForTest(dummy, parentShipment, consol);
			var containers = options.Containers;

			AssertEquals("All containers should be included", 3, containers.Count);
		}

		public void TestDeliveryTransportBooking_Transhipment_ConsolOnly()
		{
			var dummy = Factory.New<DummyWithDtbBooking>();
			var container1 = new Container(DefaultDataObjectWriterStrategy.TestInstance) { Link = 1 };
			var container2 = new Container(DefaultDataObjectWriterStrategy.TestInstance) { Link = 2 };
			var packingLine1 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { ContainerLink = 1 };
			var packingLine2 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { ContainerLink = 2 };
			var consol = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			var parentShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			parentShipment.SetPackingLineCollection(() => new DataObjectList<PackingLine> { packingLine1, packingLine2 });

			consol.SetContainerCollection(() => new DataObjectList<Container>());
			consol.ContainerCollection.Add(container1);
			consol.ContainerCollection.Add(container2);

			var manager = new DtbDeliveryManager(Factory, new[] { dummy }, dummy.GetSupportedDirections()[0], false);
			var options = manager.GetOptionsForTest(dummy, parentShipment, consol);
			var containers = options.Containers;

			AssertEquals("Consol's containers should be included", 2, containers.Count);
		}

		public void TestDeliveryTransportBooking_Transhipment_ShipmentOnly()
		{
			var dummy = Factory.New<DummyWithDtbBooking>();
			var container1 = new Container(DefaultDataObjectWriterStrategy.TestInstance) { Link = 1 };
			var container2 = new Container(DefaultDataObjectWriterStrategy.TestInstance) { Link = 2 };
			var container3 = new Container(DefaultDataObjectWriterStrategy.TestInstance) { Link = 3 };
			var packingLine1 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { ContainerLink = 1 };
			var packingLine2 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { ContainerLink = 2 };
			var consol = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			var parentShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			parentShipment.SetPackingLineCollection(() => new DataObjectList<PackingLine> { packingLine1, packingLine2 });

			parentShipment.SetContainerCollection(() => new DataObjectList<Container>());
			parentShipment.ContainerCollection.Add(container1);
			parentShipment.ContainerCollection.Add(container2);
			parentShipment.ContainerCollection.Add(container3);

			var manager = new DtbDeliveryManager(Factory, new[] { dummy }, dummy.GetSupportedDirections()[0], false);
			var options = manager.GetOptionsForTest(dummy, parentShipment, consol);
			var containers = options.Containers;

			AssertEquals("Shipment's containers should be included", 3, containers.Count);
		}

		public void TestDeliveryTransportBooking_Transhipment_NoContainers()
		{
			var dummy = Factory.New<DummyWithDtbBooking>();
			var packingLine1 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { ContainerLink = 1 };
			var packingLine2 = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { ContainerLink = 2 };
			var consol = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			var parentShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			parentShipment.SetPackingLineCollection(() => new DataObjectList<PackingLine> { packingLine1, packingLine2 });

			var manager = new DtbDeliveryManager(Factory, new[] { dummy }, dummy.GetSupportedDirections()[0], false);
			var options = manager.GetOptionsForTest(dummy, parentShipment, consol);
			var containers = options.Containers;

			AssertEquals("No containers should be included", 0, containers.Count);
		}

		public void TestDeliveryTransportBooking_Transhipment_Pic()
		{
			var shipmentBO = GetSavedShipmentForTranshipment();
			var manager = new DtbDeliveryManager(Factory, shipmentBO, DtbBookingDirection.PIC, false);
			var options = manager.GetOptionsForTest();

			var containers = options.Containers;
			AssertEquals("Only 2 containers should be included.", 2, containers.Count);
			AssertEquals("'SAME' container should be included only once.", 1, containers.Where(cnt => cnt.ContainerNumber == "CONTSAME").Count());
			AssertEquals("'PICKUP' container should be included.", 1, containers.Where(cnt => cnt.ContainerNumber == "CONTPIC2").Count());
			AssertEquals("'DELIVERY' container should not be included.", 0, containers.Where(cnt => cnt.ContainerNumber == "CONTDEL2").Count());
		}

		public void TestDeliveryTransportBooking_Transhipment_Dlv()
		{
			var shipmentBO = GetSavedShipmentForTranshipment();
			var manager = new DtbDeliveryManager(Factory, shipmentBO, DtbBookingDirection.DLV, false);
			var options = manager.GetOptionsForTest();

			var containers = options.Containers;
			AssertEquals("Only 2 containers should be included.", 2, containers.Count);
			AssertEquals("'SAME' container should be included only once.", 1, containers.Where(cnt => cnt.ContainerNumber == "CONTSAME").Count());
			AssertEquals("'DELIVERY' container should be included.", 1, containers.Where(cnt => cnt.ContainerNumber == "CONTDEL2").Count());
			AssertEquals("'PICKUP' container should not be included.", 0, containers.Where(cnt => cnt.ContainerNumber == "CONTPIC2").Count());
		}

		IDtbBookingParent GetSavedShipmentForTranshipment()
		{
			// S1234
			//		C1PIC
			//		C2DEL

			var now = ZDateTime.Now;
			var setupFactory = new BusinessObjectFactory();
			var helper = new TransportBookingTestHelper(setupFactory);
			var shipment = helper.CreateForwardingShipment("S00001234", "", "SEA", "FCL");

			var consolPic = helper.CreateForwardingConsol(shipment, "C0001PIC", "SEA", "");
			var transportPic = (BusinessObject)((IBusinessObjectCollection)((BusinessObject)consolPic)["Transports"])[0];
			transportPic[JobConsolTransportSchema.JW_IsLinked] = true;
			transportPic[JobConsolTransportSchema.JW_ETD] = now.AddDays(-20);
			transportPic[JobConsolTransportSchema.JW_ETA] = now.AddDays(-10);

			var consolDel = helper.CreateForwardingConsol(shipment, "C0002DEL", "SEA", "");
			var transportDel = (BusinessObject)((IBusinessObjectCollection)((BusinessObject)consolDel)["Transports"])[0];
			transportDel[JobConsolTransportSchema.JW_IsLinked] = true;
			transportDel[JobConsolTransportSchema.JW_ETD] = now.AddDays(-10);
			transportDel[JobConsolTransportSchema.JW_ETA] = now;

			var containerPic1 = helper.CreateForwardingContainer(consolPic, "CONTSAME", "20GP");
			var containerDel1 = helper.CreateForwardingContainer(consolDel, "CONTSAME", "20GP");
			var packline1 = helper.CreateForwardingPackline(shipment, containerPic1, 10);
			var packlineContainerPivotBO1 = Factory.New(ObjectFactory.GetType<Freight.Integration.IJobContainerPackPivot>());
			packlineContainerPivotBO1[JobContainerPackPivotSchema.J6_JC] = ((BusinessObject)containerDel1).PK;
			packlineContainerPivotBO1[JobContainerPackPivotSchema.J6_JL] = ((BusinessObject)packline1).PK;

			var containerPic2 = helper.CreateForwardingContainer(consolPic, "CONTPIC2", "20GP");
			var containerDel2 = helper.CreateForwardingContainer(consolDel, "CONTDEL2", "20GP");
			var packline2 = helper.CreateForwardingPackline(shipment, containerPic2, 10);
			var packlineContainerPivotBO2 = Factory.New(ObjectFactory.GetType<Freight.Integration.IJobContainerPackPivot>());
			packlineContainerPivotBO2[JobContainerPackPivotSchema.J6_JC] = ((BusinessObject)containerDel2).PK;
			packlineContainerPivotBO2[JobContainerPackPivotSchema.J6_JL] = ((BusinessObject)packline2).PK;

			setupFactory.Save();

			return (IDtbBookingParent)Factory.Load<IForwardingShipment>(shipment.PK);
		}

		IDtbBookingParent GetSavedShipmentForTranshipment_1Packline2Consols()
		{
			// S1234
			//		10x PLT
			//		C1PIC
			//			CONTSAME
			//		C2DEL
			//			CONTSAME

			var now = ZDateTime.Now;
			var setupFactory = new BusinessObjectFactory();
			var helper = new TransportBookingTestHelper(setupFactory);
			var shipment = helper.CreateForwardingShipment("S00001234", "", "SEA", "FCL");
			var packline = helper.CreateForwardingPackline(shipment, "PLT", 10);

			// consol 1 with container
			var consolPic = helper.CreateForwardingConsol(shipment, "C0001PIC", "SEA", "");
			var transportPic = (BusinessObject)((IBusinessObjectCollection)((BusinessObject)consolPic)["Transports"])[0];
			transportPic[JobConsolTransportSchema.JW_IsLinked] = true;
			transportPic[JobConsolTransportSchema.JW_ETD] = now.AddDays(-20);
			transportPic[JobConsolTransportSchema.JW_ETA] = now.AddDays(-10);

			var containerPic = helper.CreateForwardingContainer(consolPic, "CONTSAME", "20GP", "SealPic");
			var packlineContainerPivotBO1 = Factory.New(ObjectFactory.GetType<Freight.Integration.IJobContainerPackPivot>());
			packlineContainerPivotBO1[JobContainerPackPivotSchema.J6_JC] = ((BusinessObject)containerPic).PK;
			packlineContainerPivotBO1[JobContainerPackPivotSchema.J6_JL] = ((BusinessObject)packline).PK;

			// consol 2 with container
			var consolDel = helper.CreateForwardingConsol(shipment, "C0002DEL", "SEA", "");
			var transportDel = (BusinessObject)((IBusinessObjectCollection)((BusinessObject)consolDel)["Transports"])[0];
			transportDel[JobConsolTransportSchema.JW_IsLinked] = true;
			transportDel[JobConsolTransportSchema.JW_ETD] = now.AddDays(-10);
			transportDel[JobConsolTransportSchema.JW_ETA] = now;

			var containerDel = helper.CreateForwardingContainer(consolDel, "CONTSAME", "20GP", "SealDlv");
			var packlineContainerPivotBO2 = Factory.New(ObjectFactory.GetType<Freight.Integration.IJobContainerPackPivot>());
			packlineContainerPivotBO2[JobContainerPackPivotSchema.J6_JC] = ((BusinessObject)containerDel).PK;
			packlineContainerPivotBO2[JobContainerPackPivotSchema.J6_JL] = ((BusinessObject)packline).PK;

			setupFactory.Save();

			return (IDtbBookingParent)Factory.Load<IForwardingShipment>(shipment.PK);
		}

		public void TestDeliveryTransportBooking_Transhipment_Pic_CreateTransportBooking()
		{
			var shipmentBO = GetSavedShipmentForTranshipment_1Packline2Consols();
			var manager = new DtbDeliveryManager(Factory, shipmentBO, DtbBookingDirection.PIC, false);

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes; // options form
			ZFormModaliser.ShowDialogsInTest = true;
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
			{
				var formAsDocumentForm = form as TransportBookingDocumentForm;
				if (formAsDocumentForm != null)
				{
					formAsDocumentForm.DeliveryOptionsResultForTesting = TransportBookingDocumentOptions.DeliveryOption.AutoDelivery;
				}
			});

			var bookings = manager.DeliverTransportBooking();
			var booking = (DtbBooking)bookings.Single();
			AssertEquals(shipmentBO.PK, booking.ConsolidationSingleJob.Parent.ParentWithWorkflow.PK);

			var packages = booking.ConsolidationSingleJob.PackageJob.Packages;
			AssertEquals(1, packages.Count);

			var container = packages[0];
			AssertEquals("CNT", container.KP_F3_NKPackType);
			AssertEquals("CONTSAME", container.KP_PackageID);
			AssertEquals("SealPic", container.Container.K0_Seal1);
			AssertEquals(1, container.Packages.Count);
			AssertEquals(10, container.Packages[0].KP_PackageQty);
			AssertContainsExactElementsInAnyOrder(new[] { container }, booking.AssignedPackages);
		}

		public void TestDeliveryTransportBooking_Transhipment_Dlv_CreateTransportBooking()
		{
			var shipmentBO = GetSavedShipmentForTranshipment_1Packline2Consols();
			var manager = new DtbDeliveryManager(Factory, shipmentBO, DtbBookingDirection.DLV, false);

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes; // options form
			ZFormModaliser.ShowDialogsInTest = true;
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
			{
				var formAsDocumentForm = form as TransportBookingDocumentForm;
				if (formAsDocumentForm != null)
				{
					formAsDocumentForm.DeliveryOptionsResultForTesting = TransportBookingDocumentOptions.DeliveryOption.AutoDelivery;
				}
			});

			var bookings = manager.DeliverTransportBooking();
			var booking = (DtbBooking)bookings.Single();
			AssertEquals(shipmentBO.PK, booking.ConsolidationSingleJob.Parent.ParentWithWorkflow.PK);

			var packages = booking.ConsolidationSingleJob.PackageJob.Packages;
			AssertEquals(1, packages.Count);

			var container = packages[0];
			AssertEquals("CNT", container.KP_F3_NKPackType);
			AssertEquals("CONTSAME", container.KP_PackageID);
			AssertEquals("SealDlv", container.Container.K0_Seal1);
			AssertEquals(1, container.Packages.Count);
			AssertEquals(10, container.Packages[0].KP_PackageQty);
			AssertContainsExactElementsInAnyOrder(new[] { container }, booking.AssignedPackages);
		}

		public void TestCreateBookingConsolidationCannotSaveException()
		{
			TestCreateBookingConsolidationCannotSaveExceptionCore();
		}

		public void TestCreateBookingConsolidationCannotSaveException_WithLoggerAndAutomatedManager()
		{
			var logger = new TestServiceLogger();
			var errorManager = NewAutomatedErrorManager();
			TestCreateBookingConsolidationCannotSaveExceptionCore(logger, errorManager);
			var expectedError = "Test Cannot Save Exception";
			var expectedLog = "Error|Cannot save error occurred: " + expectedError + "." + System.Environment.NewLine;
			AssertEquals("Should log errors", expectedLog, logger.ToString());
			AssertEquals("Should be 1 error in error manager list", 1, errorManager.ErrorList.Count);
			var errorManagerError = errorManager.ErrorList.Single();
			AssertEquals("Should have collected correct error - ZCannotSaveError", DtbBookingCreationErrorType.ZCannotSaveError, errorManagerError.ErrorType);
			AssertEquals("Collected error should have correct message", expectedError, errorManagerError.Message);
		}

		void TestCreateBookingConsolidationCannotSaveExceptionCore(ILogger logger = null, IAutomatedDtbBookingCreationErrorManager errorManager = null)
		{
			var dummy = Factory.New<DummyWithDtbBooking>();
			dummy.Z0_Description = "DUM456"; // required as this is the source object key, DataContextDataObjectWriter.PopulateDataObject - line 29 : dataContext.AddDataSource(manager.DataContextType, manager.DataContextKey);

			Factory.Save();

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;

			var manager = new DtbDeliveryManager(Factory, new[] { dummy }, dummy.GetSupportedDirections()[0], false, logger, errorManager);
			try
			{
				manager.TypeOfExceptionToThrowDuringSave = DtbDeliveryManagerErrorsToTestFor.ZCannotSaveException;
				AssertNoExceptionThrown(
					"Test call to CreateTransportBooking should not throw exception - ZCannotSaveException should be swallowed and message displayed",
					() => manager.CreateTransportBooking());

				CombineAssertions(() =>
				{
					AssertNull(manager.LastControllerForTest);
					AssertEquals("Cannot save error occurred: Test Cannot Save Exception.", UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
			finally
			{
				manager?.LastControllerForTest?.LastShownForm?.Dispose();
			}
		}

		public void TestCreateBookingConsolidationSaveConcurrencyException()
		{
			TestCreateBookingConsolidationSaveConcurrencyExceptionCore();
		}

		public void TestCreateBookingConsolidationSaveConcurrencyException_WithLoggerAndAutomatedManager()
		{
			var logger = new TestServiceLogger();
			var errorManager = NewAutomatedErrorManager();
			TestCreateBookingConsolidationSaveConcurrencyExceptionCore(logger, errorManager);
			var expectedLog = "Error|Another user has made changes while this job was being worked on." + System.Environment.NewLine;
			AssertEquals("Should log errors", expectedLog, logger.ToString());
			AssertEquals("Should be 1 error in error manager list", 1, errorManager.ErrorList.Count);
			var errorManagerError = errorManager.ErrorList.Single();
			AssertEquals("Should have collected correct error - ZSaveConcurrencyError", DtbBookingCreationErrorType.ZSaveConcurrencyError, errorManagerError.ErrorType);
			AssertEquals("Collected error should have correct message", "Another user has made changes while this job was being worked on.", errorManagerError.Message);
		}

		public void TestCreateBookingConsolidationSaveConcurrencyExceptionCore(ILogger logger = null, IAutomatedDtbBookingCreationErrorManager errorManager = null)
		{
			var dummy = Factory.New<DummyWithDtbBooking>();
			dummy.Z0_Description = "DUM456"; // required as this is the source object key, DataContextDataObjectWriter.PopulateDataObject - line 29 : dataContext.AddDataSource(manager.DataContextType, manager.DataContextKey);

			Factory.Save();

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;

			var manager = new DtbDeliveryManager(Factory, new[] { dummy }, dummy.GetSupportedDirections()[0], false, logger, errorManager);
			try
			{
				manager.TypeOfExceptionToThrowDuringSave = DtbDeliveryManagerErrorsToTestFor.ZSaveConcurrencyException;
				AssertNoExceptionThrown(
					"Test call to CreateTransportBooking should not throw exception - ZSaveConcurrencyException should be swallowed and message displayed",
					() => manager.CreateTransportBooking());

				CombineAssertions(() =>
				{
					AssertNull(manager.LastControllerForTest);
					var expectedMessage = logger == null ?
						"Another user has made changes while you were working on this job. Re-open the form and try the action again." :
						"Another user has made changes while this job was being worked on.";
					AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
			finally
			{
				manager?.LastControllerForTest?.LastShownForm?.Dispose();
			}
		}

		public void TestCreateBookingConsolidationSaveExceptionFromTryingToCreateDuplicateConsolidationRecord()
		{
			TestCreateBookingConsolidationSaveExceptionFromTryingToCreateDuplicateConsolidationRecordCore();
		}

		public void TestCreateBookingConsolidationSaveExceptionFromTryingToCreateDuplicateConsolidationRecord_WithLoggerAndAutomatedManager()
		{
			var logger = new TestServiceLogger();
			var errorManager = NewAutomatedErrorManager();
			TestCreateBookingConsolidationSaveExceptionFromTryingToCreateDuplicateConsolidationRecordCore(logger, errorManager);
			var expectedLog = "Error|Another user created a booking while we were attempting to create it, booking creation canceled." + System.Environment.NewLine;
			AssertEquals("Should log errors", expectedLog, logger.ToString());
			AssertEquals("Should be 1 error in error manager list", 1, errorManager.ErrorList.Count);
			var errorManagerError = errorManager.ErrorList.Single();
			AssertEquals("Should have collected correct error - ZSaveConcurrencyError", DtbBookingCreationErrorType.DuplicateBookingConsolidationError, errorManagerError.ErrorType);
			AssertEquals("Collected error should have correct message", "Another user created a booking while we were attempting to create it, booking creation canceled.", errorManagerError.Message);
		}

		void TestCreateBookingConsolidationSaveExceptionFromTryingToCreateDuplicateConsolidationRecordCore(ILogger logger = null, IAutomatedDtbBookingCreationErrorManager errorManager = null)
		{
			var dummy = Factory.New<DummyWithDtbBooking>();
			dummy.Z0_Description = "DUM456"; // required as this is the source object key, DataContextDataObjectWriter.PopulateDataObject - line 29 : dataContext.AddDataSource(manager.DataContextType, manager.DataContextKey);

			Factory.Save();

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;

			var manager = new DtbDeliveryManager(Factory, new[] { dummy }, dummy.GetSupportedDirections()[0], false, logger, errorManager);
			try
			{
				manager.TypeOfExceptionToThrowDuringSave = DtbDeliveryManagerErrorsToTestFor.SqlDuplicateConsolidationException;
				AssertNoExceptionThrown(
					"Test call to CreateTransportBooking should not throw exception - ZSave exception with SqlException indicating duplicate on DtbBookingConsolidation index NR_UX__KB_ParentID_KB_ParentTableCode_KB_JobDirection should be swallowed and message displayed",
					() => manager.CreateTransportBooking());

				CombineAssertions(() =>
				{
					AssertNull(manager.LastControllerForTest);
					var expectedMessage = logger == null ?
						"Another user created a booking while we were attempting to create it, booking creation canceled. Re-open the form and try the action again." :
						"Another user created a booking while we were attempting to create it, booking creation canceled.";
					AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
			finally
			{
				manager?.LastControllerForTest?.LastShownForm?.Dispose();
			}
		}

		public void TestCreateBookingConsolidationOtherSaveException()
		{
			var dummy = Factory.New<DummyWithDtbBooking>();
			dummy.Z0_Description = "DUM456"; // required as this is the source object key, DataContextDataObjectWriter.PopulateDataObject - line 29 : dataContext.AddDataSource(manager.DataContextType, manager.DataContextKey);

			Factory.Save();

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;

			var manager = new DtbDeliveryManager(Factory, dummy, dummy.GetSupportedDirections()[0], false);
			try
			{
				manager.TypeOfExceptionToThrowDuringSave = DtbDeliveryManagerErrorsToTestFor.OtherZSaveException;
				AssertExceptionThrown<ZSaveException>(
					"Test call to CreateTransportBooking should throw exception, to be handled by usual Error Handling",
					() => manager.CreateTransportBooking());
			}
			finally
			{
				manager?.LastControllerForTest?.LastShownForm?.Dispose();
			}
		}

		public void TestUpdatingPackagesInAContainerRemovesTheOldPackages()
		{
			// set up forwarding shipment with containers and consols
			// shipment
			var shipmentBO = Factory.NewWithValidTestData(ObjectFactory.GetType<IForwardingShipment>());
			shipmentBO[JobShipmentSchema.JS_UniqueConsignRef] = "S00001234";
			shipmentBO[JobShipmentSchema.JS_HouseBill] = "HB31278903";
			// consol
			var consolBO = Factory.New(ObjectFactory.GetType<Forwarding.IForwardingConsol>());
			consolBO[JobConsolSchema.JK_UniqueConsignRef] = "C00001432";
			consolBO[JobConsolSchema.JK_AgentType] = "AGT";
			consolBO[JobConsolSchema.JK_TransportMode] = "SEA";
			consolBO[JobConsolSchema.JK_MasterBillNum] = "MB234890232";
			// consol-shipment pivot
			var consolShipmentLinkBO = Factory.New(ObjectFactory.GetType<Freight.Integration.IJobConShipLink>());
			consolShipmentLinkBO[JobConShipLinkSchema.JN_JK] = consolBO.PK;
			consolShipmentLinkBO[JobConShipLinkSchema.JN_JS] = shipmentBO.PK;
			// containers
			var forwardingContainerBO = Factory.New(ObjectFactory.GetType<Forwarding.IForwardingContainer>());
			forwardingContainerBO[JobContainerSchema.JC_ContainerNum] = "CN1";
			var gp20 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			forwardingContainerBO[JobContainerSchema.JC_RC] = gp20.PK;
			forwardingContainerBO[JobContainerSchema.JC_JK] = consolBO.PK;
			((BusinessObjectCollection)consolBO["Containers"]).Add(forwardingContainerBO);
			// shipment packlines
			var packlineBO = Factory.New(ObjectFactory.GetType<Freight.Integration.Forwarding.IForwardingPackLine>());
			// packcontainer pivot
			var packlineContainerPivotBO = Factory.New(ObjectFactory.GetType<Freight.Integration.IJobContainerPackPivot>());
			packlineContainerPivotBO[JobContainerPackPivotSchema.J6_JC] = forwardingContainerBO.PK;
			packlineContainerPivotBO[JobContainerPackPivotSchema.J6_JL] = packlineBO.PK;

			packlineBO[JobPackLinesSchema.JL_FreightMode] = "OUT";
			packlineBO[JobPackLinesSchema.JL_PackageCount] = 30;
			packlineBO[JobPackLinesSchema.JL_JS] = shipmentBO.PK;

			Factory.Save();

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;

			// publish shipment and create TB
			var deliveryManager = new DtbDeliveryManager(Factory, (IDtbBookingParent)shipmentBO, DtbBookingDirection.PIC, false);
			deliveryManager.CreateTransportBooking();
			using (var lastShownForm = deliveryManager.LastControllerForTest.LastShownForm)
			{
				var consolidation = Factory.LoadTop1<DtbBookingConsolidation>(new ZQuery());
				// check containers and packing lines
				var packages = consolidation.Bookings.Single().PackageJob.Containers.Single().GetAllPackages();
				AssertEquals(1, packages.Count);
				AssertEquals(30, packages[0].KP_PackageQty);
				// set bookingConsolidation.IsOverriden to allow shipment to override
				consolidation.KB_IsOverridden = false;
				// now change packline details on shipment
				packlineBO[JobPackLinesSchema.JL_PackageCount] = 10;
				Factory.Save();
			}

			var deliveryManager2 = new DtbDeliveryManager(Factory, (IDtbBookingParent)shipmentBO, DtbBookingDirection.PIC, false);
			deliveryManager2.CreateTransportBooking();
			using (var lastShownForm2 = deliveryManager2.LastControllerForTest.LastShownForm)
			{
				var consolidationLoadedAgain = new BusinessObjectFactory().Load<DtbBookingConsolidation>(new ZQuery()).Single();
				// check containers and packing lines use new packing lines
				var packagesInConsolidationAfterReading = consolidationLoadedAgain.Bookings.Single().PackageJob.Containers.Single().GetAllPackages();
				AssertEquals(1, packagesInConsolidationAfterReading.Count);
				AssertEquals("Booking packages which were created before have been replaced.", 10, packagesInConsolidationAfterReading[0].KP_PackageQty);
			}
		}

		public void TestCreateTransportBooking_WhenInstructionToImportAlreadyInCache_DoesNotThrowAndCompletesSuccessfully()
		{
			var dummy = Factory.New<DummyWithDtbBooking>();
			var dummy2 = Factory.New<DummyWithDtbBooking>();
			dummy.Z0_Description = "DUM456";
			dummy2.Z0_Description = "DUM123";

			Factory.Save();

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
			ZFormModaliser.ShowDialogsInTest = true;

			bool transportBookingCreatedAndSavedHit = false;
			dummy.TransportBookingCreatedOrUpdated_ForTesting += delegate
			{ transportBookingCreatedAndSavedHit = true; };

			var manager = new DtbDeliveryManager(Factory, new[] { dummy, dummy2 }, dummy.GetSupportedDirections()[0], false);
			manager.InsertInstructionForTest = true;
			try
			{
				AssertNoExceptionThrown(
					"Test threw exception, probably due to conflicting KN_PK on DtbBookingInstruction record that was wrongly imported from another factory",
					() => manager.CreateTransportBooking());
				CombineAssertions(() =>
				{
					AssertEquals("Transport Booking was created", true, transportBookingCreatedAndSavedHit);
					AssertEquals("Should have no errors reported now", 0, ErrorReporter.TotalErrorCount);
				});
			}
			finally
			{
				ErrorReporter.Clear();
				manager?.LastControllerForTest?.LastShownForm?.Dispose();
			}
		}

		public void TestCreateTransportBooking_WhenDuplicateTBDataImportEventIsCreated_DoesNotThrowAndCompletesSuccessfully()
		{
			var dummy = Factory.New<DummyWithDtbBooking>();
			var dummy2 = Factory.New<DummyWithDtbBooking>();
			dummy.Z0_Description = "DUM456";
			dummy2.Z0_Description = "DUM123";

			Factory.Save();

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
			ZFormModaliser.ShowDialogsInTest = true;

			bool transportBookingCreatedAndSavedHit = false;
			dummy.TransportBookingCreatedOrUpdated_ForTesting += delegate
			{ transportBookingCreatedAndSavedHit = true; };

			var manager = new DtbDeliveryManager(Factory, new[] { dummy, dummy2 }, dummy.GetSupportedDirections()[0], false);
			manager.SimulateDuplicateTransportBookingDataImportEventsForTest = true;

			AssertNoExceptionThrown(
				"Test threw exception, due to conflicting KN_PK on DtbBookingInstruction record that was already previously created/imported in a previous iteration of the (ImportFromAntoherFactory) foreach loop",
				() => manager.CreateTransportBooking());
			CombineAssertions(() =>
			{
				AssertEquals("Transport Booking was created", true, transportBookingCreatedAndSavedHit);
				AssertEquals("Should have no errors reported now", 0, ErrorReporter.TotalErrorCount);
			});
			ErrorReporter.Clear();

			manager?.LastControllerForTest?.LastShownForm?.Dispose();
		}

		public void TestCreateTransportBooking_WhenStillFailsDueToDuplicateInstruction_DoesNotThrowAndReportsError()
		{
			var dummy = Factory.New<DummyWithDtbBooking>();
			var dummy2 = Factory.New<DummyWithDtbBooking>();
			dummy.Z0_Description = "DUM456";
			dummy2.Z0_Description = "DUM123";

			Factory.Save();

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
			ZFormModaliser.ShowDialogsInTest = true;

			bool transportBookingCreatedAndSavedHit = false;
			dummy.TransportBookingCreatedOrUpdated_ForTesting += delegate
			{ transportBookingCreatedAndSavedHit = true; };

			var manager = new DtbDeliveryManager(Factory, new[] { dummy, dummy2 }, dummy.GetSupportedDirections()[0], false);
			manager.InsertInstructionForTestAfterCheck = true;

			try
			{
				AssertNoExceptionThrown(
					"Test threw exception, due to conflicting KN_PK on DtbBookingInstruction record that somehow made it into the database before Transport Booking finished being created",
					() => manager.CreateTransportBooking());
				CombineAssertions(() =>
				{
					AssertEquals("Transport Booking was created", true, transportBookingCreatedAndSavedHit);
					Assert("Last reported error should have correct message text", ErrorReporter.LastMessageReported.StartsWith("Was attempting to import into factory booking instruction(s) which were already in the database. Details below:", StringComparison.InvariantCultureIgnoreCase));
					AssertEquals("Last reported error key should be DtbDeliveryManager_CreateBookingConsolidation_InstructionClash", "DtbDeliveryManager_CreateBookingConsolidation_InstructionClash", ErrorReporter.LastKeyReported);
				});
			}
			finally
			{
				ErrorReporter.Clear();
				manager?.LastControllerForTest?.LastShownForm?.Dispose();
			}
		}

		public void TestReportImportPostSyncClash_WhenBookingDeleted_DoesNotReportErrorAboutBookingDeletion()
		{
			var dummy = Factory.New<DummyWithDtbBooking>();
			var dummy2 = Factory.New<DummyWithDtbBooking>();
			dummy.Z0_Description = "DUM456";
			dummy2.Z0_Description = "DUM123";

			Factory.Save();

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
			ZFormModaliser.ShowDialogsInTest = true;

			bool transportBookingCreatedAndSavedHit = false;
			dummy.TransportBookingCreatedOrUpdated_ForTesting += delegate
			{ transportBookingCreatedAndSavedHit = true; };

			var manager = new DtbDeliveryManager(Factory, new[] { dummy, dummy2 }, dummy.GetSupportedDirections()[0], false);
			manager.InsertInstructionForTestAfterCheck = true;
			manager.DeleteBookingForTest = true;

			try
			{
				AssertNoExceptionThrown(
					"Test threw exception, probably due to conflicting KN_PK on DtbBookingInstruction record that was wrongly imported from another factory",
					() => manager.CreateTransportBooking());
				CombineAssertions(() =>
				{
					AssertEquals("Transport Booking was created", true, transportBookingCreatedAndSavedHit);
					Assert("Last reported error should have correct message text", ErrorReporter.LastMessageReported.StartsWith("Was attempting to import into factory booking instruction(s) which were already in the database. Details below:", StringComparison.InvariantCultureIgnoreCase));
					AssertEquals("Last reported error key should be DtbDeliveryManager_CreateBookingConsolidation_InstructionClash", "DtbDeliveryManager_CreateBookingConsolidation_InstructionClash", ErrorReporter.LastKeyReported);
					AssertEquals("Should only have error about import clash, not about deleted bookings", 1, ErrorReporter.TotalErrorCount);
					AssertContainsExactElementsInAnyOrder("Should not have errors about deleted bookings", Array.Empty<string>(), ErrorReporter.LastExceptionsReported().Where(x => x.Contains("System.Data.RowNotInTableException")).ToArray());
				});
			}
			finally
			{
				ErrorReporter.Clear();
				manager?.LastControllerForTest?.LastShownForm?.Dispose();
			}
		}

		public void TestCreateBookingConsolidation_WhenImportResultsEmptyDueToInactiveContextBranch_DoesNotThrowException()
		{
			TestCreateBookingConsolidation_WhenImportResultsEmptyDueToInactiveContextBranchCore(null);
		}

		public void TestCreateBookingConsolidation_WhenImportResultsEmptyDueToInactiveContextBranch_WithLoggerAndAutomatedErrorManager()
		{
			var logger = new TestServiceLogger();
			var errorManager = NewAutomatedErrorManager();
			var inactiveBranchCode = "XYZ";

			TestCreateBookingConsolidation_WhenImportResultsEmptyDueToInactiveContextBranchCore(logger, errorManager, inactiveBranchCode);
			var expectedError = $@"Failed to Create Transport Booking:
Error - Message Rejected as Branch '{inactiveBranchCode}' is inactive.";
			AssertEquals("Should log errors", "Error|" + expectedError + System.Environment.NewLine, logger.ToString());
			AssertEquals("Should be 1 error in error manager list", 1, errorManager.ErrorList.Count);
			var errorManagerError = errorManager.ErrorList.Single();
			AssertEquals("Should have collected correct error - UniversalEventDeliveryFailure", DtbBookingCreationErrorType.UniversalResultError, errorManagerError.ErrorType);
			AssertEquals("Collected error should have correct message", expectedError, errorManagerError.Message);
		}

		void TestCreateBookingConsolidation_WhenImportResultsEmptyDueToInactiveContextBranchCore(ILogger logger = null, IAutomatedDtbBookingCreationErrorManager errorManager = null, string inactiveBranchCode = "XYZ")
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "ZZZ";

			var company = Factory.New<GlbCompany>();
			company.GC_Code = "AAB";
			company.GC_OH_OrgProxy = orgHeader.PK;

			var department = Factory.New<GlbDepartment>();
			department.GE_Code = "FDP";

			var branch = Factory.New<GlbBranch>();
			branch.GB_Code = inactiveBranchCode;
			branch.GB_GC = company.PK;
			branch.GB_OH_OrgProxy = orgHeader.PK;
			// when current branch is inactive this causes UniversalXmlWorkflowProcessor.ProcessAsInboundDataObjectWithRetry() to return result.ImportResults empty and hence send UniversalEventDeliveryFailureException
			branch.GB_IsActive = false;

			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "ABC";
			staff.GS_LoginName = "fakeuser";
			staff.GS_IsActive = true;
			staff.GS_IsController = true;
			staff.GS_GE_HomeDepartment = department.PK;
			staff.GS_GB_HomeBranch = branch.PK;

			Factory.Save();

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), branch.PK.ToGuid(), department.PK.ToGuid()))
			{
				var dummy = Factory.New<DummyWithDtbBooking>();

				Factory.Save();

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes; // options form
				ZFormModaliser.ShowDialogsInTest = true;

				var manager = new DtbDeliveryManager(Factory, new[] { dummy }, dummy.GetSupportedDirections()[0], false, logger, errorManager);
				AssertNoExceptionThrown(() => manager.CreateTransportBooking());
				AssertEquals("Assert the correct message is displayed", $@"Failed to Create Transport Booking:
Error - Message Rejected as Branch '{branch.GB_Code}' is inactive.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestCreateBookingConsolidation_WhenImportResultsEmptyDueToInactiveContextBranch_ParentFactorySaved_DoesNotThrowException()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "ZZZ";

			var company = Factory.New<GlbCompany>();
			company.GC_Code = "AAB";
			company.GC_OH_OrgProxy = orgHeader.PK;

			var department = Factory.New<GlbDepartment>();
			department.GE_Code = "FDP";

			var branch = Factory.New<GlbBranch>();
			branch.GB_Code = "XYZ";
			branch.GB_GC = company.PK;
			branch.GB_OH_OrgProxy = orgHeader.PK;
			// when current branch is inactive this causes UniversalXmlWorkflowProcessor.ProcessAsInboundDataObjectWithRetry() to return result.ImportResults empty and hence send UniversalEventDeliveryFailureException
			branch.GB_IsActive = false;

			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "ABC";
			staff.GS_LoginName = "fakeuser";
			staff.GS_IsActive = true;
			staff.GS_IsController = true;
			staff.GS_GE_HomeDepartment = department.PK;
			staff.GS_GB_HomeBranch = branch.PK;

			Factory.Save();

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), branch.PK.ToGuid(), department.PK.ToGuid()))
			{
				var dummy = Factory.New<DummyWithDtbBooking>();

				Factory.Save();

				var saveCount = Factory.SaveCount;

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes; // options form
				ZFormModaliser.ShowDialogsInTest = true;

				var newFactoryForDeliveryManager = new BusinessObjectFactory();
				var manager = new DtbDeliveryManager(newFactoryForDeliveryManager, new[] { dummy }, dummy.GetSupportedDirections()[0], false);
				AssertNoExceptionThrown(() => manager.CreateTransportBooking());
				AssertEquals("Assert the correct message is displayed", $@"Failed to Create Transport Booking:
Error - Message Rejected as Branch '{branch.GB_Code}' is inactive.", UnitTestUserNotification.Instance.LastMessage.Text);

				AssertNoExceptionThrown("Saving the original factory should not have the above validation failure present", () => Factory.Save());
				Assert("Save count incremented by only 1", Factory.IsEqualToCurrentSaveCount(++saveCount));
			}
		}

		public void TestCaptionsAndText()
		{
			AssertEquals("Create", DtbDeliveryManager.CreateEnglishCaption);
			AssertEquals("Create/Override", DtbDeliveryManager.CreateEnglishCaptionWhenHasExistingBooking);
			AssertEquals("Create", DtbDeliveryManager.CreateText.Caption);
			AssertEquals("Create/Override", DtbDeliveryManager.CreateTextWhenHasExistingBooking.Caption);
			AssertEquals("Multi-Container", DtbDeliveryManager.MultiContainerEnglishCaption);
			AssertEquals("Multi-Container", DtbDeliveryManager.MultiContainerText.Caption);
			AssertEquals("View/Edit", DtbDeliveryManager.ViewText.Caption);
			AssertEquals("View", DtbDeliveryManager.ViewTextWhenParentCancelled.Caption);
		}

		public void TestDefaultBookingView()
		{
			AssertEquals(TransportBookingInstructionView.Standard, DtbDeliveryManager.DefaultBookingView);

			using (TransportRegistry.Instance.DefaultTransportBookingTabView.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, BookingViews.Codes.Instruction))
			{
				AssertEquals(TransportBookingInstructionView.Instruction, DtbDeliveryManager.DefaultBookingView);
			}

			using (TransportRegistry.Instance.DefaultTransportBookingTabView.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, BookingViews.Codes.Standard))
			{
				AssertEquals(TransportBookingInstructionView.Standard, DtbDeliveryManager.DefaultBookingView);
			}
		}

		public void TestGetOptionsForParentShipmentWithCustomsDeclarationReturnsAllContainersIntegration_DirectionPIC()
		{
			GetOptionsForParentShipmentWithCustomsDeclarationReturnsAllContainersIntegrationCoreTest(DtbBookingDirection.PIC);
		}

		public void TestGetOptionsForParentShipmentWithCustomsDeclarationReturnsAllContainersIntegration_DirectionDLV()
		{
			GetOptionsForParentShipmentWithCustomsDeclarationReturnsAllContainersIntegrationCoreTest(DtbBookingDirection.DLV);
		}

		void GetOptionsForParentShipmentWithCustomsDeclarationReturnsAllContainersIntegrationCoreTest(DtbBookingDirection direction)
		{
			var setupFactory = new BusinessObjectFactory();
			var setupHelper = new TransportBookingTestHelper(setupFactory);

			var shipment = setupHelper.CreateForwardingShipment("S01234", "", "SEA", "FCL");

			var consol = setupHelper.CreateForwardingConsol(shipment, "C01234", "SEA", "");

			var jobContainer1 = consol.Containers.AddNew();
			jobContainer1[JobContainerSchema.JC_ContainerNum] = "A01";
			jobContainer1[JobContainerSchema.JC_RC] = setupFactory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			var jobPackLine1 = (BusinessObject)setupHelper.CreateForwardingPackline(shipment, (Forwarding.IForwardingContainer)jobContainer1, 1);

			var jobContainer2 = consol.Containers.AddNew();
			jobContainer2[JobContainerSchema.JC_ContainerNum] = "B02";
			jobContainer2[JobContainerSchema.JC_RC] = setupFactory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			var jobPackLine2 = (BusinessObject)setupHelper.CreateForwardingPackline(shipment, (Forwarding.IForwardingContainer)jobContainer2, 1);

			var jobContainer3 = consol.Containers.AddNew();
			jobContainer3[JobContainerSchema.JC_ContainerNum] = "C03";
			jobContainer3[JobContainerSchema.JC_RC] = setupFactory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			var jobPackLine3 = (BusinessObject)setupHelper.CreateForwardingPackline(shipment, (Forwarding.IForwardingContainer)jobContainer3, 1);

			var customsDeclaration = (BusinessObject)Factory.New<IBaseJobDeclaration>();
			customsDeclaration[JobDeclarationSchema.JE_JS] = shipment.PK;
			customsDeclaration.FillWithValidTestData();

			var customsContainer1 = ((BusinessObjectCollection)customsDeclaration["CusContainers"]).AddNew();
			customsContainer1[CusContainerSchema.CO_JC] = jobContainer1.PK;
			customsContainer1.FillWithValidTestData();

			setupFactory.Save();

			var parentShipmentBO = (BusinessObject)Factory.Load<IForwardingShipment>(shipment.PK);

			var manager = new DtbDeliveryManager(Factory, (IDtbBookingParent)parentShipmentBO, direction, false);
			var options = manager.GetOptionsForTest();
			var optionContainers = options.Containers;

			CombineAssertions("Check that all option containers populated correctly for direction " + direction.ToString(), () =>
			{
				AssertEquals("Check that all containers are included in options", 3, optionContainers.Count);
				AssertContainsExactElementsInAnyOrder("Check that all container numbers are included in options", new[] { "A01", "B02", "C03" }, optionContainers.Select(c => c.ContainerNumber).ToArray());
			});
		}

		IAutomatedDtbBookingCreationErrorManager NewAutomatedErrorManager()
		{
			return ObjectFactory.Get<IAutomatedDtbBookingCreationErrorManager>();
		}

		public void TestConstructorParameterSuppressDialogsAndUserInteractivityTrue()
		{
			using (Globals.SetIsWinzorForTest(true))
			using (Globals.SetIsUserInteractiveForTest(true))
			{
				Assert("Precondition: Globals.IsUserInteractive is true", Globals.IsUserInteractive);
				Assert("Precondition: Globals.CanShowDialogs is true", Globals.CanShowDialogs);
				var dummy = Factory.New<DummyWithDtbBooking>();
				var manager = new DtbDeliveryManager(Factory, new IDtbBookingParent[] { dummy }, DtbBookingDirection.PIC, combineContainers: true, logger: null, errorManager: null, suppressDialogsAndUserInteractivity: true, selectAllContainers: false);
				CombineAssertions("With constructor parameter suppressDialogsAndUserInteractivity true, manager properties IsUserInteractive and CanShowDialogs should be false", () =>
				{
					Assert("Constructor parameter suppressDialogsAndUserInteractivity set to true should set property SuppressDialogsAndUserInteractivity to true", manager.SuppressDialogsAndUserInteractivity);
					Assert("Constructor parameter suppressDialogsAndUserInteractivity set to true should force IsUserInteractive to false", !manager.IsUserInteractive);
					Assert("Constructor parameter suppressDialogsAndUserInteractivity set to true should force CanShowDialogs to false", !manager.CanShowDialogs);
				});
			}
		}

		public void TestConstructorParameterSuppressDialogsAndUserInteractivityFalse()
		{
			using (Globals.SetIsWinzorForTest(true))
			using (Globals.SetIsUserInteractiveForTest(true))
			{
				Assert("Precondition: Globals.IsUserInteractive is true", Globals.IsUserInteractive);
				Assert("Precondition: Globals.CanShowDialogs is true", Globals.CanShowDialogs);
				var dummy = Factory.New<DummyWithDtbBooking>();
				var manager = new DtbDeliveryManager(Factory, new IDtbBookingParent[] { dummy }, DtbBookingDirection.PIC, combineContainers: true, logger: null, errorManager: null, suppressDialogsAndUserInteractivity: false, selectAllContainers: false);
				CombineAssertions("With constructor parameter suppressDialogsAndUserInteractivity true, manager properties IsUserInteractive and CanShowDialogs should be false", () =>
				{
					Assert("Constructor parameter suppressDialogsAndUserInteractivity set to false should set property SuppressDialogsAndUserInteractivity to false", !manager.SuppressDialogsAndUserInteractivity);
					Assert("Constructor parameter suppressDialogsAndUserInteractivity set to false should leave IsUserInteractive as true", manager.IsUserInteractive);
					Assert("Constructor parameter suppressDialogsAndUserInteractivity set to false should leave CanShowDialogs as true", manager.CanShowDialogs);
				});
			}
		}

		public void TestConstructorParameterSelectAllContainersTrue()
		{
			var dummy = Factory.New<DummyWithDtbBooking>();
			var manager = new DtbDeliveryManager(Factory, new IDtbBookingParent[] { dummy }, DtbBookingDirection.PIC, combineContainers: true, logger: null, errorManager: null, suppressDialogsAndUserInteractivity: false, selectAllContainers: true);
			Assert("SelectAllContainers should be set to true from the constructor parameter selectAllContainers", manager.SelectAllContainers);

			var options = new TransportBookingDocumentOptions(dummy, Factory, DataContextType.ForwardingShipment, DtbBookingDirection.PIC, combineContainers: true, transportMode: "SEA", containerMode: "FCL", hasValidCFSAddress: false);
			Assert("Precondition: options.IsFCL is true", options.IsFCL);
			Assert("SelectAllContainersFromOptions() should return true if options.IsFCL", manager.SelectAllContainersFromOptions(options));

			options = new TransportBookingDocumentOptions(dummy, Factory, DataContextType.ForwardingShipment, DtbBookingDirection.PIC, combineContainers: true, transportMode: "SEA", containerMode: "LTL", hasValidCFSAddress: false);
			Assert("Precondition: options.IsFCL is false", !options.IsFCL);
			Assert("SelectAllContainersFromOptions() should return true as manager.SelectAllContainers is true", manager.SelectAllContainersFromOptions(options));
		}

		public void TestConstructorParameterSelectAllContainersFalse()
		{
			var dummy = Factory.New<DummyWithDtbBooking>();
			var manager = new DtbDeliveryManager(Factory, new IDtbBookingParent[] { dummy }, DtbBookingDirection.PIC, combineContainers: true, logger: null, errorManager: null, suppressDialogsAndUserInteractivity: false, selectAllContainers: false);
			Assert("SelectAllContainers should be set to false from the constructor parameter selectAllContainers", !manager.SelectAllContainers);

			var options = new TransportBookingDocumentOptions(dummy, Factory, DataContextType.ForwardingShipment, DtbBookingDirection.PIC, combineContainers: true, transportMode: "SEA", containerMode: "FCL", hasValidCFSAddress: false);
			Assert("Precondition: options.IsFCL is true", options.IsFCL);
			Assert("SelectAllContainersFromOptions() should return true if options.IsFCL", manager.SelectAllContainersFromOptions(options));

			options = new TransportBookingDocumentOptions(dummy, Factory, DataContextType.ForwardingShipment, DtbBookingDirection.PIC, combineContainers: true, transportMode: "SEA", containerMode: "LTL", hasValidCFSAddress: false);
			Assert("Precondition: options.IsFCL is false", !options.IsFCL);
			Assert("SelectAllContainersFromOptions() should return false as manager.SelectAllContainers is false and options.IsFCL is false", !manager.SelectAllContainersFromOptions(options));
		}

		public void TestOperationalActionLogLoggerWrapper()
		{
			var dummy = Factory.New<DummyWithDtbBooking>();

			var nonOperationalActionLogLoggerWrapper = new TestServiceLogger();
			var managerWithNonOperationalActionLogLoggerWrapper = new DtbDeliveryManager(Factory, new IDtbBookingParent[] { dummy }, DtbBookingDirection.PIC, combineContainers: true, logger: nonOperationalActionLogLoggerWrapper);
			AssertNull("Manager with non-OperationalActionLogLoggerWrapper logger has OperationalActionSectionLogLoggerWrapper return null", managerWithNonOperationalActionLogLoggerWrapper.OperationalActionSectionLogLoggerWrapper);

			var operationalActionLogLoggerWrapper = new Mock<IOperationalActionSectionLogLoggerWrapper>().Object;
			var managerWithOperationalActionLogLoggerWrapper = new DtbDeliveryManager(Factory, new IDtbBookingParent[] { dummy }, DtbBookingDirection.PIC, combineContainers: true, logger: operationalActionLogLoggerWrapper);
			AssertNotNull("Manager with non-OperationalActionLogLoggerWrapper logger has OperationalActionSectionLogLoggerWrapper has non-null value", managerWithOperationalActionLogLoggerWrapper.OperationalActionSectionLogLoggerWrapper);
		}

		// this is the current use case, calls to CreateTransportBookings() that do not have a use case eg with suppressDialogsAndUserInteractivity == false or selectAllContainers == false are not tested yet
		public void TestCreateTransportBookings()
		{
			// in the real log, it would be an OperationalActionLogErrorLevel instead of LogType but we can't include Enterprise.OperationalActions.Support due to Winzor
			var log = new List<(LogType logType, string message)>();
			var progress = 0;
			var progressMax = 0;

			var setupFactory = new BusinessObjectFactory();
			var helper = new TransportBookingTestHelper(setupFactory);

			var shipment1PK = CreateForwardingShipmentWithConsolAndContainersAndReturnShipmentPK(helper, "S1", "C1", "LCL", "LCL");
			var shipment2PK = CreateForwardingShipmentWithConsolAndContainersAndReturnShipmentPK(helper, "S2", "C2", "LTL", "LTL");
			var shipment3PK = CreateForwardingShipmentWithConsolAndContainersAndReturnShipmentPK(helper, "S3", "C3", "LCL", "LCL");
			var shipment4PK = CreateForwardingShipmentWithConsolAndContainersAndReturnShipmentPK(helper, "S4", "C4", "FCL", "FCL");

			// need to load again otherwise containers do not populate on ForwardingShipment objects properly
			var shipment1 = Factory.Load<IForwardingShipment>(shipment1PK);
			var shipment2 = Factory.Load<IForwardingShipment>(shipment2PK);
			var shipment3 = Factory.Load<IForwardingShipment>(shipment3PK);
			var shipment4 = Factory.Load<IForwardingShipment>(shipment4PK);

			var parents = new[] { shipment1, shipment2, shipment3, shipment4 }.Cast<IDtbBookingParent>().ToArray();
			var operationalActionSectionLogger = new Mock<IOperationalActionSectionLogLoggerWrapper>();
			operationalActionSectionLogger.Setup(l => l.Log(It.IsAny<LogType>(), It.IsAny<string>())).Callback(LogToOperationalActionSectionLogger);
			operationalActionSectionLogger.Setup(l => l.Log(It.IsAny<LogType>(), It.IsAny<string>(), It.IsAny<Exception>())).Callback((LogType logType, string message, Exception ex) => LogToOperationalActionSectionLogger(logType, message));
			operationalActionSectionLogger.Setup(l => l.SetSectionProgressMax(It.IsAny<int>())).Callback((int max) => progressMax = max);
			operationalActionSectionLogger.Setup(l => l.BumpSectionProgress()).Callback(() => progress++);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.ClearUserResponses();
			var manager = new DtbDeliveryManager(Factory, parents, DtbBookingDirection.PIC, combineContainers: true, logger: operationalActionSectionLogger.Object, errorManager: null, suppressDialogsAndUserInteractivity: true, selectAllContainers: true);

			var defaultTemplate = "EFPR";
			var bookingsCreated = manager.CreateTransportBookings(defaultTemplate);

			var dtbBookingsCreated = bookingsCreated.Cast<DtbBooking>().ToArray();
			var jobIDs = new List<string>();
			var logOutput = GetLogOutput(log);

			CombineAssertions("Check that CreateTransportBookings() with default template and settings from operational action will result in correct bookings and log", () =>
			{
				AssertEquals("Should have been four Transport Bookings created, one for each Shipment", 4, bookingsCreated.Count());
				foreach (var shipment in parents)
				{
					var jobID = AssertBookingWasCreatedForShipmentAndHasCorrectDetailsAndReturnBookingJobID(shipment, defaultTemplate, dtbBookingsCreated);
					jobIDs.Add(jobID);
				}

				var expectedMaxProgress = parents.Length * 2;
				AssertEquals(FormattableString.Invariant($"progressMax should have been set to {expectedMaxProgress} (for each parent one for check, one for processing)"), expectedMaxProgress, progressMax);
				AssertEquals("progress should have been bumped up to max", expectedMaxProgress, progress);

				var jobIDsArray = jobIDs.ToArray();
				var expectedLog = FormattableString.Invariant(
@$"Information|Checking Shipment S1
Information|Checking Shipment S2
Information|Checking Shipment S3
Information|Checking Shipment S4
Information|Transport Booking {jobIDsArray[0]} created for Shipment S1
Information|Transport Booking {jobIDsArray[1]} created for Shipment S2
Information|Transport Booking {jobIDsArray[2]} created for Shipment S3
Information|Transport Booking {jobIDsArray[3]} created for Shipment S4
");
				AssertEquals(
					"Operational Action Section Log is correct",
					expectedLog,
					logOutput);

				Assert("Should have been no Global messages", !UnitTestUserNotification.Instance.PreviousMessages.Any(m => !m.Text.IsNullOrEmpty()));
			});

			void LogToOperationalActionSectionLogger(LogType logType, string message)
			{
				log.Add((logType, message));
			}
		}

		public void TestCreateTransportBookingsWithIssues()
		{
			// in the real log, it would be an OperationalActionLogErrorLevel instead of LogType but we can't include Enterprise.OperationalActions.Support due to Winzor
			var log = new List<(LogType logType, string message)>();
			var progress = 0;
			var progressMax = 0;

			var setupFactory = new BusinessObjectFactory();
			var helper = new TransportBookingTestHelper(setupFactory);
			var otherTemplate = "EFPU";

			var shipment1PK = CreateForwardingShipmentWithConsolAndContainersButWithOverriddenTransportBookingConsolidationAndReturnShipmentPK("S1", "C1", "LTL", "LTL", "XC1", "XB1", otherTemplate);
			var shipment2PK = CreateForwardingShipmentWithConsolAndContainersButWithServiceCommencedTransportBookingAndReturnShipmentPK("S2", "C2", "LCL", "LCL", "XC2", "XB2", otherTemplate);
			var shipment3PK = CreateInactiveForwardingShipmentWithConsolAndContainersAndNoBookingsAndReturnShipmentPK("S3", "C3", "LCL", "LCL");
			var shipment4PK = CreateInactiveForwardingShipmentWithConsolAndContainersAndInactiveBookingAndReturnShipmentPK("S4", "C4", "LCL", "LCL", "XC4", "XB4", otherTemplate);
			var shipment5PK = CreateInactiveForwardingShipmentWithConsolAndContainersAndActiveBookingAndReturnShipmentPK("S5", "C5", "LCL", "LCL", "XC5", "XB5", otherTemplate);
			var shipment6PK = CreateForwardingShipmentWithConsolAndContainersAndActiveBookingAndReturnShipmentPK("S6", "C6", "LCL", "LCL", "XC6", "XB6", otherTemplate);
			var shipment7PK = CreateForwardingShipmentWithConsolAndContainersAndReturnShipmentPK(helper, "S7", "C7", "FCL", "FCL");
			var shipment8PK = CreateForwardingShipmentWithConsolAndContainersButWithSubTransportBookingAndReturnShipmentPK("S8", "C8", "FCL", "FCL", "XC8", "XB8", otherTemplate, isOverridden: false, isCommenced: false);
			var shipment9PK = CreateForwardingShipmentWithConsolAndContainersButWithSubTransportBookingAndReturnShipmentPK("S9", "C9", "LCL", "LCL", "XC9", "XB9", otherTemplate, isOverridden: true, isCommenced: false);
			var shipment10PK = CreateForwardingShipmentWithConsolAndContainersButWithSubTransportBookingAndReturnShipmentPK("S10", "C10", "FCL", "FCL", "XC10", "XB10", otherTemplate, isOverridden: false, isCommenced: true);
			var shipment11PK = CreateForwardingShipmentWithConsolAndContainersButWithSubTransportBookingAndReturnShipmentPK("S11", "C11", "FCL", "FCL", "XC11", "XB11", otherTemplate, isOverridden: true, isCommenced: true);

			// need to load again otherwise containers do not populate on ForwardingShipment objects properly
			var shipment1 = Factory.Load<IForwardingShipment>(shipment1PK);
			var shipment2 = Factory.Load<IForwardingShipment>(shipment2PK);
			var shipment3 = Factory.Load<IForwardingShipment>(shipment3PK);
			var shipment4 = Factory.Load<IForwardingShipment>(shipment4PK);
			var shipment5 = Factory.Load<IForwardingShipment>(shipment5PK);
			var shipment6 = Factory.Load<IForwardingShipment>(shipment6PK);
			var shipment7 = Factory.Load<IForwardingShipment>(shipment7PK);
			var shipment8 = Factory.Load<IForwardingShipment>(shipment8PK);
			var shipment9 = Factory.Load<IForwardingShipment>(shipment9PK);
			var shipment10 = Factory.Load<IForwardingShipment>(shipment10PK);
			var shipment11 = Factory.Load<IForwardingShipment>(shipment11PK);

			var parents = new[] { shipment1, shipment2, shipment3, shipment4, shipment5, shipment6, shipment7, shipment8, shipment9, shipment10, shipment11 }.Cast<IDtbBookingParent>().ToArray();
			var operationalActionSectionLogger = new Mock<IOperationalActionSectionLogLoggerWrapper>();
			operationalActionSectionLogger.Setup(l => l.Log(It.IsAny<LogType>(), It.IsAny<string>())).Callback(LogToOperationalActionSectionLogger);
			operationalActionSectionLogger.Setup(l => l.Log(It.IsAny<LogType>(), It.IsAny<string>(), It.IsAny<Exception>())).Callback((LogType logType, string message, Exception ex) => LogToOperationalActionSectionLogger(logType, message));
			operationalActionSectionLogger.Setup(l => l.SetSectionProgressMax(It.IsAny<int>())).Callback((int max) => progressMax = max);
			operationalActionSectionLogger.Setup(l => l.BumpSectionProgress()).Callback(() => progress++);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.ClearUserResponses();
			var manager = new DtbDeliveryManager(Factory, parents, DtbBookingDirection.PIC, combineContainers: true, logger: operationalActionSectionLogger.Object, errorManager: null, suppressDialogsAndUserInteractivity: true, selectAllContainers: true);

			var defaultTemplate = "EFPR";
			var queryBookingUnderShipment6 = new ZQuery(DtbBookingSchema.KM_JobID, "XB6");
			var bookingUnderShipment6 = Factory.LoadTop1<DtbBooking>(queryBookingUnderShipment6);
			DtbChildEditableService.SetState(Factory, DtbChildEditableServiceState.Transport);
			IEnumerable<IDtbBooking> bookingsCreated;
			using (var form = new TransportBookingFormForTest(bookingUnderShipment6))
			{
				OpenedFormCache.GetInstance().Add(bookingUnderShipment6.PK.ToGuid(), form, nameof(ControllerIDs.DtbBooking));
				bookingsCreated = manager.CreateTransportBookings(defaultTemplate);
			}

			var dtbBookingsCreated = bookingsCreated.Cast<DtbBooking>().ToArray();
			var logOutput = GetLogOutput(log);

			CombineAssertions("Check that CreateTransportBookings() with default template and settings from operational action will result in correct bookings and log", () =>
			{
				var expectedBookingsInReturnedOutput = 8;
				AssertEquals(FormattableString.Invariant($"Should have been {expectedBookingsInReturnedOutput} Transport Bookings in returned output from CreateTransportBookings()"), expectedBookingsInReturnedOutput, bookingsCreated.Count());
				AssertBookingUnderShipmentStillHasCorrectDetailsAndIsInBookingsCreated((IDtbBookingParent)shipment1, "XC1", "XB1", otherTemplate, expectedIsOverridden: true, expectedIsActive: true, expectedInBookingsCreated: true, dtbBookingsCreated);
				AssertBookingUnderShipmentStillHasCorrectDetailsAndIsInBookingsCreated((IDtbBookingParent)shipment2, "XC2", "XB2", otherTemplate, expectedIsOverridden: false, expectedIsActive: true, expectedInBookingsCreated: true, dtbBookingsCreated);
				AssertNoBookingsUnderShipment((IDtbBookingParent)shipment3);
				AssertBookingUnderShipmentStillHasCorrectDetailsAndIsInBookingsCreated((IDtbBookingParent)shipment4, "XC4", "XB4", otherTemplate, expectedIsOverridden: false, expectedIsActive: false, expectedInBookingsCreated: false, dtbBookingsCreated);
				AssertBookingUnderShipmentStillHasCorrectDetailsAndIsInBookingsCreated((IDtbBookingParent)shipment5, "XC5", "XB5", otherTemplate, expectedIsOverridden: false, expectedIsActive: true, expectedInBookingsCreated: true, dtbBookingsCreated);
				AssertBookingUnderShipmentStillHasCorrectDetailsAndIsInBookingsCreated((IDtbBookingParent)shipment6, "XC6", "XB6", otherTemplate, expectedIsOverridden: false, expectedIsActive: true, expectedInBookingsCreated: false, dtbBookingsCreated);
				var jobIDForShipment7 = AssertBookingWasCreatedForShipmentAndHasCorrectDetailsAndReturnBookingJobID((IDtbBookingParent)shipment7, defaultTemplate, dtbBookingsCreated);
				AssertBookingUnderShipmentStillHasCorrectDetailsAndIsInBookingsCreated((IDtbBookingParent)shipment8, "XC8", "XB8", otherTemplate, expectedIsOverridden: false, expectedIsActive: true, expectedInBookingsCreated: true, dtbBookingsCreated);
				AssertBookingUnderShipmentStillHasCorrectDetailsAndIsInBookingsCreated((IDtbBookingParent)shipment9, "XC9", "XB9", otherTemplate, expectedIsOverridden: true, expectedIsActive: true, expectedInBookingsCreated: true, dtbBookingsCreated);
				AssertBookingUnderShipmentStillHasCorrectDetailsAndIsInBookingsCreated((IDtbBookingParent)shipment10, "XC10", "XB10", otherTemplate, expectedIsOverridden: false, expectedIsActive: true, expectedInBookingsCreated: true, dtbBookingsCreated);
				AssertBookingUnderShipmentStillHasCorrectDetailsAndIsInBookingsCreated((IDtbBookingParent)shipment11, "XC11", "XB11", otherTemplate, expectedIsOverridden: true, expectedIsActive: true, expectedInBookingsCreated: true, dtbBookingsCreated);

				var expectedMaxProgress = parents.Length * 2;
				AssertEquals(FormattableString.Invariant($"progressMax should have been set to {expectedMaxProgress} (for each parent one for check, one for processing)"), expectedMaxProgress, progressMax);
				AssertEquals("progress should have been bumped up to max", expectedMaxProgress, progress);

				var expectedLog = FormattableString.Invariant(
@$"Information|Checking Shipment S1
Warning|Booking Consolidation XC1 for Shipment S1 is overridden and cannot be overwritten
Warning|Bookings XB1 under Booking Consolidation XC1 for Shipment S1 have not been changed
Information|Checking Shipment S2
Warning|Booking Consolidation XC2 for Shipment S2 has at least one service commenced booking XB2 and cannot be overwritten
Warning|Bookings XB2 under Booking Consolidation XC2 for Shipment S2 have not been changed
Information|Checking Shipment S3
Error|Shipment S3 is deactivated and cannot create new Transport Bookings.
Information|Checking Shipment S4
Error|Shipment S4 is deactivated and cannot create new Transport Bookings.
Information|Checking Shipment S5
Warning|Shipment S5 is deactivated but has one or more active bookings
Warning|Bookings XB5 under Booking Consolidation XC5 for Shipment S5 have not been changed
Information|Checking Shipment S6
Error|Cannot create a new Transport Booking as an existing Transport Booking screen is open.

Close the Transport Booking screen and try again.
Information|Checking Shipment S7
Information|Checking Shipment S8
Warning|Booking XB8 is a sub booking under Master Transport Booking MasterXB8 and cannot be overwritten
Warning|Bookings XB8 under Booking Consolidation XC8 for Shipment S8 have not been changed
Information|Checking Shipment S9
Warning|Booking XB9 is a sub booking under Master Transport Booking MasterXB9 and cannot be overwritten
Warning|Booking Consolidation XC9 for Shipment S9 is overridden and cannot be overwritten
Warning|Bookings XB9 under Booking Consolidation XC9 for Shipment S9 have not been changed
Information|Checking Shipment S10
Warning|Booking XB10 is a sub booking under Master Transport Booking MasterXB10 and cannot be overwritten
Warning|Booking Consolidation XC10 for Shipment S10 has at least one service commenced booking XB10 and cannot be overwritten
Warning|Bookings XB10 under Booking Consolidation XC10 for Shipment S10 have not been changed
Information|Checking Shipment S11
Warning|Booking XB11 is a sub booking under Master Transport Booking MasterXB11 and cannot be overwritten
Warning|Booking Consolidation XC11 for Shipment S11 is overridden and cannot be overwritten
Warning|Booking Consolidation XC11 for Shipment S11 has at least one service commenced booking XB11 and cannot be overwritten
Warning|Bookings XB11 under Booking Consolidation XC11 for Shipment S11 have not been changed
Information|Transport Booking {jobIDForShipment7} created for Shipment S7
");
				AssertEquals(
					"Operational Action Section Log is correct, indicating that processing did continue even though some previously 'fatal' errors had occurred in the checking phase",
					expectedLog,
					logOutput);

				Assert("Should have been no Global messages", !UnitTestUserNotification.Instance.PreviousMessages.Any(m => !m.Text.IsNullOrEmpty()));
			});

			ZGuid CreateForwardingShipmentWithConsolAndContainersButWithOverriddenTransportBookingConsolidationAndReturnShipmentPK(string shipmentID, string consolID, string shipmentPackingMode, string containerMode, string dtbBookingConsolidationJobID, string dtbBookingJobID, string bookingTemplate)
			{
				var shipment = CreateForwardingShipmentWithConsolAndContainersAndReturnShipment(helper, shipmentID, consolID, shipmentPackingMode, containerMode);
				var bookingConsolidation = helper.CreateConsolidation((IDtbBookingParent)shipment);
				bookingConsolidation.KB_IsOverridden = true;
				var booking = helper.CreateBooking(bookingConsolidation);
				booking.KM_JobID = dtbBookingJobID;
				booking.KM_KT_NKBookingTemplate = bookingTemplate;
				helper.Factory.Save();
				bookingConsolidation.KB_JobID = dtbBookingConsolidationJobID;
				helper.Factory.Save();

				return shipment.PK;
			}

			ZGuid CreateForwardingShipmentWithConsolAndContainersButWithServiceCommencedTransportBookingAndReturnShipmentPK(string shipmentID, string consolID, string shipmentPackingMode, string containerMode, string dtbBookingConsolidationJobID, string dtbBookingJobID, string bookingTemplate)
			{
				var shipment = CreateForwardingShipmentWithConsolAndContainersAndReturnShipment(helper, shipmentID, consolID, shipmentPackingMode, containerMode);
				var bookingConsolidation = helper.CreateConsolidation((IDtbBookingParent)shipment);
				var booking = helper.CreateBooking(bookingConsolidation);
				booking.KM_JobID = dtbBookingJobID;
				booking.KM_KT_NKBookingTemplate = bookingTemplate;
				booking.Logs.AddNew(Events.ServiceCommenced, "", ZDateTimeOffset.Today, isEstimate: false);
				helper.Factory.Save();
				bookingConsolidation.KB_JobID = dtbBookingConsolidationJobID;
				helper.Factory.Save();

				return shipment.PK;
			}

			ZGuid CreateInactiveForwardingShipmentWithConsolAndContainersAndNoBookingsAndReturnShipmentPK(string shipmentID, string consolID, string shipmentPackingMode, string containerMode)
			{
				var shipment = CreateForwardingShipmentWithConsolAndContainersAndReturnShipment(helper, shipmentID, consolID, shipmentPackingMode, containerMode);
				shipment.JS_IsCancelled = true;
				helper.Factory.Save();

				return shipment.PK;
			}

			ZGuid CreateInactiveForwardingShipmentWithConsolAndContainersAndInactiveBookingAndReturnShipmentPK(string shipmentID, string consolID, string shipmentPackingMode, string containerMode, string dtbBookingConsolidationJobID, string dtbBookingJobID, string bookingTemplate)
			{
				var shipment = CreateForwardingShipmentWithConsolAndContainersAndReturnShipment(helper, shipmentID, consolID, shipmentPackingMode, containerMode);
				shipment.JS_IsCancelled = true;
				var bookingConsolidation = helper.CreateConsolidation((IDtbBookingParent)shipment);
				bookingConsolidation.KB_JobDirection = "PIC";
				var booking = helper.CreateBooking(bookingConsolidation);
				booking.KM_JobID = dtbBookingJobID;
				booking.KM_KT_NKBookingTemplate = bookingTemplate;
				booking.KM_IsActive = false;
				booking.KM_Direction = "ORG";
				helper.Factory.Save();
				bookingConsolidation.KB_JobID = dtbBookingConsolidationJobID;
				helper.Factory.Save();

				return shipment.PK;
			}

			ZGuid CreateInactiveForwardingShipmentWithConsolAndContainersAndActiveBookingAndReturnShipmentPK(string shipmentID, string consolID, string shipmentPackingMode, string containerMode, string dtbBookingConsolidationJobID, string dtbBookingJobID, string bookingTemplate)
			{
				var shipment = CreateForwardingShipmentWithConsolAndContainersAndReturnShipment(helper, shipmentID, consolID, shipmentPackingMode, containerMode);
				shipment.JS_IsCancelled = true;
				var bookingConsolidation = helper.CreateConsolidation((IDtbBookingParent)shipment);
				bookingConsolidation.KB_JobDirection = "PIC";
				var booking = helper.CreateBooking(bookingConsolidation);
				booking.KM_JobID = dtbBookingJobID;
				booking.KM_KT_NKBookingTemplate = bookingTemplate;
				booking.KM_Direction = "ORG";
				helper.Factory.Save();
				bookingConsolidation.KB_JobID = dtbBookingConsolidationJobID;
				helper.Factory.Save();

				return shipment.PK;
			}

			ZGuid CreateForwardingShipmentWithConsolAndContainersAndActiveBookingAndReturnShipmentPK(string shipmentID, string consolID, string shipmentPackingMode, string containerMode, string dtbBookingConsolidationJobID, string dtbBookingJobID, string bookingTemplate)
			{
				var shipment = CreateForwardingShipmentWithConsolAndContainersAndReturnShipment(helper, shipmentID, consolID, shipmentPackingMode, containerMode);

				var bookingConsolidation = helper.CreateConsolidation((IDtbBookingParent)shipment);
				bookingConsolidation.KB_JobDirection = "PIC";
				var booking = helper.CreateBooking(bookingConsolidation);
				booking.KM_JobID = dtbBookingJobID;
				booking.KM_KT_NKBookingTemplate = bookingTemplate;
				booking.KM_Direction = "ORG";
				helper.Factory.Save();
				bookingConsolidation.KB_JobID = dtbBookingConsolidationJobID;
				helper.Factory.Save();

				return shipment.PK;
			}

			ZGuid CreateForwardingShipmentWithConsolAndContainersButWithSubTransportBookingAndReturnShipmentPK(string shipmentID, string consolID, string shipmentPackingMode, string containerMode, string dtbBookingConsolidationJobID, string dtbBookingJobID, string bookingTemplate, bool isOverridden, bool isCommenced)
			{
				var shipment = CreateForwardingShipmentWithConsolAndContainersAndReturnShipment(helper, shipmentID, consolID, shipmentPackingMode, containerMode);
				var bookingConsolidation = helper.CreateConsolidation((IDtbBookingParent)shipment);
				if (isOverridden)
				{
					bookingConsolidation.KB_IsOverridden = true;
				}
				var booking = helper.CreateBooking(bookingConsolidation);
				booking.KM_JobID = dtbBookingJobID;
				booking.KM_KT_NKBookingTemplate = bookingTemplate;
				if (isCommenced)
				{
					booking.Logs.AddNew(Events.ServiceCommenced, "", ZDateTimeOffset.Today, isEstimate: false);
				}
				helper.Factory.Save();
				bookingConsolidation.KB_JobID = dtbBookingConsolidationJobID;
				helper.Factory.Save();
				var masterBooking = helper.CreateBooking();
				masterBooking.KM_IsMaster = true;
				masterBooking.KM_JobID = "Master" + dtbBookingJobID;
				masterBooking.KM_KT_NKBookingTemplate = bookingTemplate;
				helper.Factory.Save();
				masterBooking.SubBookings.Add(booking);
				helper.Factory.Save();

				return shipment.PK;
			}

			void LogToOperationalActionSectionLogger(LogType logType, string message)
			{
				log.Add((logType, message));
			}

			void AssertNoBookingsUnderShipment(IDtbBookingParent shipment)
			{
				var booking = dtbBookingsCreated.FirstOrDefault(b => b.ParentID == shipment.JobNumber);
				var bookingConsolidationUnderShipmentQuery = new ZQuery(DtbBookingConsolidationSchema.KB_ParentID, shipment.PK);
				var bookingConsolidationUnderShipment = Factory.LoadTop1<DtbBookingConsolidation>(bookingConsolidationUnderShipmentQuery);
				AssertNull("Should still be no bookings under shipment", bookingConsolidationUnderShipment);
			}
		}

		ZGuid CreateForwardingShipmentWithConsolAndContainersAndReturnShipmentPK(TransportBookingTestHelper helper, string shipmentID, string consolID, string shipmentPackingMode, string containerMode)
		{
			return CreateForwardingShipmentWithConsolAndContainersAndReturnShipment(helper, shipmentID, consolID, shipmentPackingMode, containerMode).PK;
		}

		IForwardingShipment CreateForwardingShipmentWithConsolAndContainersAndReturnShipment(TransportBookingTestHelper helper, string shipmentID, string consolID, string shipmentPackingMode, string containerMode)
		{
			var shipment = helper.CreateForwardingShipment(shipmentID, "", "SEA", shipmentPackingMode);
			var consol = helper.CreateForwardingConsol(shipment, consolID, "SEA", "");
			var containerA = helper.CreateForwardingContainer(consol, "CONT1A", "20GP");
			containerA.JC_ContainerMode = containerMode;
			var packlineA = (BusinessObject)helper.CreateForwardingPackline(shipment, containerA, 1);
			packlineA[JobPackLinesSchema.JL_ActualWeight] = 1;
			packlineA[JobPackLinesSchema.JL_ActualWeightUQ] = "KG";
			packlineA[JobPackLinesSchema.JL_ActualVolume] = 1;
			packlineA[JobPackLinesSchema.JL_ActualVolumeUQ] = "M3";
			var containerB = helper.CreateForwardingContainer(consol, "CONT1B", "20GP");
			containerB.JC_ContainerMode = containerMode;
			var packlineB = (BusinessObject)helper.CreateForwardingPackline(shipment, containerB, 1);
			packlineB[JobPackLinesSchema.JL_ActualWeight] = 1;
			packlineB[JobPackLinesSchema.JL_ActualWeightUQ] = "KG";
			packlineB[JobPackLinesSchema.JL_ActualVolume] = 1;
			packlineB[JobPackLinesSchema.JL_ActualVolumeUQ] = "M3";
			helper.Factory.Save();

			return shipment;
		}

		string GetLogOutput(List<(LogType logType, string message)> log)
		{
			var logOutputBuilder = new StringBuilder();
			foreach (var logLine in log)
			{
				logOutputBuilder.AppendLine(logLine.logType.ToString() + "|" + logLine.message);
			}
			return logOutputBuilder.ToString();
		}

		string AssertBookingUnderShipmentStillHasCorrectDetailsAndIsInBookingsCreated(IDtbBookingParent shipment, string expectedBookingConsolidationJobID, string expectedBookingJobID, string defaultTemplate, bool expectedIsOverridden, bool expectedIsActive, bool expectedInBookingsCreated, DtbBooking[] dtbBookingsCreated)
		{
			var bookingConsolidationQuery = new ZQuery(DtbBookingConsolidationSchema.KB_ParentID, shipment.PK);
			var bookingConsolidation = Factory.LoadTop1<DtbBookingConsolidation>(bookingConsolidationQuery);
			DtbBooking booking = null;
			if (bookingConsolidation != null)
			{
				var bookingQuery = new ZQuery(DtbBookingSchema.KM_KB_Booking, bookingConsolidation.PK);
				booking = Factory.LoadTop1<DtbBooking>(bookingQuery);
			}
			var bookingInBookingsCreated = dtbBookingsCreated.FirstOrDefault(b => b.ParentID == shipment.JobNumber);
			AssertNotNull("Booking for shipment " + shipment.JobNumber + " should exist", booking);
			if (expectedInBookingsCreated)
			{
				AssertNotNull("Booking for shipment " + shipment.JobNumber + " should exist in returned bookings from CreateTransportBookings()", bookingInBookingsCreated);
			}
			else
			{
				AssertNull("Booking for shipment " + shipment.JobNumber + " should not exist in returned bookings from CreateTransportBookings()", bookingInBookingsCreated);
			}
			if (booking != null)
			{
				AssertEquals("Booking for shipment " + shipment.JobNumber + " should still have Job ID", expectedBookingJobID, booking.KM_JobID);
				AssertEquals("Booking consolidation for shipment " + shipment.JobNumber + " should still have Job ID", expectedBookingConsolidationJobID, booking.ConsolidationSingleJob.KB_JobID);
				AssertEquals("Booking for shipment " + shipment.JobNumber + " should still have specified template", defaultTemplate, booking.KM_KT_NKBookingTemplate);
				AssertEquals("Booking consolidation for shipment " + shipment.JobNumber + " should still have correct direction", "PIC", booking.ConsolidationSingleJob.KB_JobDirection);
				AssertEquals("Booking for shipment " + shipment.JobNumber + " should still have correct direction", "ORG", booking.KM_Direction);
				AssertEquals("Booking consolidation for shipment " + shipment.JobNumber + " should still " + (expectedIsOverridden ? string.Empty : "not") + " be overridden", expectedIsOverridden, booking.ConsolidationSingleJob.KB_IsOverridden);
				AssertEquals("Booking for shipment " + shipment.JobNumber + " should still " + (expectedIsActive ? string.Empty : "not") + " be active", expectedIsActive, booking.KM_IsActive);
			}

			return booking?.KM_JobID ?? string.Empty;
		}

		string AssertBookingWasCreatedForShipmentAndHasCorrectDetailsAndReturnBookingJobID(IDtbBookingParent shipment, string defaultTemplate, DtbBooking[] dtbBookingsCreated)
		{
			var booking = dtbBookingsCreated.FirstOrDefault(b => b.ParentID == shipment.JobNumber);
			AssertNotNull("Should have created a booking for shipment " + shipment.JobNumber, booking);
			if (booking != null)
			{
				AssertEquals("Booking for shipment " + shipment.JobNumber + " should have specified template", defaultTemplate, booking.KM_KT_NKBookingTemplate);
				AssertEquals("Booking consolidation for shipment " + shipment.JobNumber + " should have correct direction", "PIC", booking.ConsolidationSingleJob.KB_JobDirection);
				AssertEquals("Booking for shipment " + shipment.JobNumber + " should have correct direction", "ORG", booking.KM_Direction);
				AssertEquals("Booking consolidation for shipment " + shipment.JobNumber + " should not be overridden", false, booking.ConsolidationSingleJob.KB_IsOverridden);
				AssertEquals("Booking for shipment " + shipment.JobNumber + " should have all packages from shipment", 2, booking.TotalPackages);
			}

			return booking?.KM_JobID ?? string.Empty;
		}

		protected override BusinessObjectFactory NewFactory()
		{
			var factory = base.NewFactory();
			DtbChildEditableService.SetState(factory, DtbChildEditableServiceState.Consolidation);
			return factory;
		}
	}
}
