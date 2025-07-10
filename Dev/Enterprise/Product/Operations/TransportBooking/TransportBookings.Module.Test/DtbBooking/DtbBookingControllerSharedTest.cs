using System;
using System.Reflection;
using System.Windows.Forms;
using Enterprise.Environment;
using Enterprise.MasterFiles.Module.Testing;
using Enterprise.Security;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportBookings.Business.Testing;
using Enterprise.TransportBookings.GUI;
using Enterprise.TransportCommon.Business;
using Enterprise.TransportCommon.Registry;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.TransportBookings.Module.Testing
{
	[TestsSubclassesOf(typeof(DtbBookingControllerShared))]
	abstract class DtbBookingControllerSharedTest<TController> : ZPopupControllerBasherTest
		where TController : DtbBookingControllerShared, new()
	{
		public void TestFormType()
		{
			DtbChildEditableService.SetState(Factory, DtbChildEditableServiceState.Transport);
			var controller = new TController();

			var booking = Helper.CreateBooking();

			Factory.Save();

			using (var normalTBForm = controller.ShowEditForm(booking))
			{
				AssertEquals(typeof(TransportBookingForm), normalTBForm.GetType());
			}
		}

		public void TestSetStrategyProvider()
		{
			var controller = new TController();
			AssertEquals("Default", DtbChildEditableServiceState.Transport, DtbChildEditableService.GetState(controller.Factory));
			AssertEquals(DtbFormState.Booking, DtbFormStateService.GetState(controller.Factory));
		}

		public void TestShowModelessForm()
		{
			var controller = new TController();

			using (TransportRegistry.Instance.DefaultTransportBookingTabView.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, BookingViews.Codes.Standard))
			using (var form = (TransportBookingForm)controller.ShowNewForm())
			{
				var instructionViewControl = GUITestHelper.FindControl<DtbInstructionViewsControl>(form.Controls, "InstructionViewsControl");
				var viewTabControl = GUITestHelper.FindControl<ZTabControl>(instructionViewControl.Controls, "ViewTabControl");
				var standardTabPage = GUITestHelper.FindControl<ZTabPage>(viewTabControl.Controls, "StandardViewTabPage");
				AssertEquals(standardTabPage, viewTabControl.SelectedTab);
			}

			using (TransportRegistry.Instance.DefaultTransportBookingTabView.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, BookingViews.Codes.Instruction))
			using (var form = (TransportBookingForm)controller.ShowNewForm())
			{
				var instructionViewControl = GUITestHelper.FindControl<DtbInstructionViewsControl>(form.Controls, "InstructionViewsControl");
				var viewTabControl = GUITestHelper.FindControl<ZTabControl>(instructionViewControl.Controls, "ViewTabControl");
				var instructionsTabPage = GUITestHelper.FindControl<ZTabPage>(viewTabControl.Controls, "InstructionViewTabPage");
				AssertEquals(instructionsTabPage, viewTabControl.SelectedTab);
			}
		}

		public void TestSwitchToFormFor()
		{
			// test through calling ShowEditForm
			DtbChildEditableService.SetState(Factory, DtbChildEditableServiceState.Transport);
			var booking = Helper.CreateBooking();
			Factory.Save();

			var controller = new TController();
			using (var singleBookingForm = controller.ShowEditForm(booking))
			{
				var lastForm = controller.ShowEditForm(booking);
				AssertEquals(typeof(TransportBookingForm), lastForm.GetType());
				AssertEquals(singleBookingForm, lastForm);

				lastForm.Dispose();
			}

			var multiBookingController = new DtbBookingConsolidationController();
			using (var multiBookingform = multiBookingController.ShowEditForm(booking.ConsolidationSingleJob))
			{
				controller.ShowEditForm(booking);
				var lastForm = controller.LastShownForm;
				AssertEquals(typeof(TransportBookingMultiForm), lastForm.GetType());
				AssertEquals(multiBookingform, lastForm);

				lastForm.Dispose();
			}
		}

		public void TestSwitchToFormFor_TryToOpenDifferentBooking()
		{
			DtbChildEditableService.SetState(Factory, DtbChildEditableServiceState.Transport);
			var consolidation = Helper.CreateConsolidation();
			var booking1 = Helper.CreateBooking(consolidation);
			var booking2 = Helper.CreateBooking(consolidation);
			Factory.Save();

			AssertEquals(consolidation, booking1.ConsolidationSingleJob);
			AssertEquals(consolidation, booking2.ConsolidationSingleJob);

			var controller = new TController();
			var multiBookingController = new DtbBookingConsolidationController();
			using (var multiBookingform = (TransportBookingMultiForm)multiBookingController.ShowEditForm(consolidation))
			{
				var bookingControl = GUITestHelper.FindControl<TransportBookingsControl>(multiBookingform.Controls, "transportBookingsControl");
				var grid = GUITestHelper.FindControl<ZGrid>(bookingControl.Controls, "Grid");
				grid.Select(0);

				typeof(Control).InvokeMember("OnMouseDown", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, grid, new object[] { new MouseEventArgs(MouseButtons.Left, 2, 50, 30, 0) });
				var singleBookingForm = ZFormModaliser.ActiveForm;
				AssertNotNull("Should Open a single Transport Booking form.", singleBookingForm);
				AssertEquals(typeof(TransportBookingForm), singleBookingForm.GetType());
				AssertEquals(multiBookingform, singleBookingForm.Owner);

				var secondForm = controller.ShowEditForm(booking2);
				var expectedMessage = "A Transport Booking screen belonging to the same consolidation is open.";
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Text.Contains(expectedMessage));

				singleBookingForm.Dispose();
				if (secondForm != null)
				{
					secondForm.Dispose();
				}
			}
		}

		public void TestIsFormShownFor()
		{
			DtbChildEditableService.SetState(Factory, DtbChildEditableServiceState.Transport);
			var booking = Helper.CreateBooking();
			Factory.Save();

			var controller = new TController();
			AssertEquals(false, controller.IsFormShownFor(booking));

			using (var singleBookingForm = controller.ShowEditForm(booking))
			{
				AssertEquals(typeof(TransportBookingForm), singleBookingForm.GetType());
				AssertEquals(true, controller.IsFormShownFor(booking));
			}

			var multiBookingController = new DtbBookingConsolidationController();
			using (var multiBookingform = multiBookingController.ShowEditForm(booking.ConsolidationSingleJob))
			{
				AssertEquals(typeof(TransportBookingMultiForm), multiBookingform.GetType());
				AssertEquals(true, controller.IsFormShownFor(booking));
			}
		}

		public void TestGetForm()
		{
			// test through calling ShowEditForm
			DtbChildEditableService.SetState(Factory, DtbChildEditableServiceState.Transport);
			var booking = Helper.CreateBooking();
			Factory.Save();

			var controller = new TController();
			using (var singleBookingForm = controller.ShowEditForm(booking))
			{
				var openedForm = controller.ShowEditForm(booking);
				AssertEquals(typeof(TransportBookingForm), openedForm.GetType());
				AssertEquals(singleBookingForm, openedForm);

				openedForm.Dispose();
			}

			var multiBookingController = new DtbBookingConsolidationController();
			using (var multiBookingform = multiBookingController.ShowEditForm(booking.ConsolidationSingleJob))
			{
				var openedForm = controller.ShowEditForm(booking);
				AssertEquals(typeof(TransportBookingMultiForm), openedForm.GetType());
				AssertEquals(multiBookingform, openedForm);

				openedForm.Dispose();
			}
		}

		public void TestCRMSecurityCheckpoints()
		{
			var bizObj = Factory.NewWithValidTestData<DtbBooking>();
			CRMSecurityProviderTest<DtbBooking>.AssertController(new TController(), bizObj, Env.Security.DtbBookingCRMSecurity);
		}

		public void TestModuleID()
		{
			AssertEquals(ExpectedModuleID, new TController().ModuleID);
		}

		ModuleIdentifier ExpectedModuleID
		{
			get { return ModuleIDs.DtbBooking; }
		}

		public void TestGetNewBusinessEntityInLocalFactory()
		{
			var controller = new TController();
			using (var form = (ZForm)controller.ShowNewForm())
			{
				AssertNotNull(((DtbBooking)form.BusinessEntity).ConsolidationSingleJob);
				if (!AllowHasChangesOnNewForm)
				{
					AssertEquals(false, form.BusinessEntity.HasChanges);
				}
			}
		}

		protected virtual bool AllowHasChangesOnNewForm
		{
			get { return false; }
		}

		public void TestSecurityCheckpoints()
		{
			var transport = Factory.New<DtbBooking>();
			var controller = new TController();

			ExpectedSecurityForDelete.IsAllowed = false;
			ExpectedSecurityForEdit.IsAllowed = false;
			ExpectedSecurityForNew.IsAllowed = false;
			ExpectedSecurityForView.IsAllowed = false;

			AssertEquals(ExpectedSecurityForDelete, controller.GetCheckPointForDelete(transport));
			AssertEquals(ExpectedSecurityForEdit, controller.GetCheckPointForEdit(transport));
			AssertEquals(ExpectedSecurityForNew, controller.GetCheckPointForNew(transport));
			AssertEquals(ExpectedSecurityForView, controller.GetCheckPointForView(transport));
		}

		SecurityCheckpoint ExpectedSecurityForDelete
		{
			get { return Env.Security.DtbBookingDelete; }
		}

		SecurityCheckpoint ExpectedSecurityForEdit
		{
			get { return Env.Security.DtbBookingEdit; }
		}

		SecurityCheckpoint ExpectedSecurityForNew
		{
			get { return Env.Security.DtbBookingNew; }
		}

		SecurityCheckpoint ExpectedSecurityForView
		{
			get { return Env.Security.DtbBookingView; }
		}

		protected TransportBookingTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingTestHelper(Factory)); }
		}
		TransportBookingTestHelper helper;
	}
}
