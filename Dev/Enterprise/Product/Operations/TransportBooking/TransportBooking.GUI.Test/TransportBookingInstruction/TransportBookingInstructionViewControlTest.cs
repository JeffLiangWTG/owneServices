using System;
using CargoWise.EntityFramework;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportBookings.Business.Testing;
using Enterprise.TransportCommon.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.TransportBookings.GUI.Testing
{
	public class TransportBookingInstructionViewControlTest : DtbBookingTestCaseWithFactory
	{
		public void TestAssignAllOuterPackagesToAllInstructions()
		{
			var consolidation = Helper.CreateConsolidation();
			var packingJob = Helper.CreatePackageJob(consolidation);
			var container1 = packingJob.Packages.AddNew("CNT");
			var container2 = packingJob.Packages.AddNew("CNT");
			var pallet1 = container1.Packages.AddNew("PLT");
			var pallet2 = container2.Packages.AddNew("PLT");

			var booking = Helper.CreateBooking(consolidation);
			var picInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp);
			var dlvInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery);

			DtbChildEditableService.SetState(Factory, DtbChildEditableServiceState.Consolidation);

			using (var form = new TransportBookingMultiForm(consolidation))
			{
				form.Show();

				form.transportBookingsControl.DtbBookingInstructionsViewsPanel.ViewTabControl.SelectedTab = form.transportBookingsControl.DtbBookingInstructionsViewsPanel.InstructionViewTabPage;
				form.transportBookingsControl.DtbBookingInstructionsViewsPanel.InstructionViewUserControl.AssignAllOuterPackagesToAllInstructionsMenuItem.PerformClick();
				AssertContainsExactElementsInAnyOrder(new[] { pallet1, pallet2 }, picInstruction.DivotsWithPackages.Packages);
				AssertContainsExactElementsInAnyOrder(new[] { pallet1, pallet2 }, dlvInstruction.DivotsWithPackages.Packages);
			}
		}

		public void TestAssignAllContainersToAllInstructions()
		{
			var consolidation = Helper.CreateConsolidation();
			var packingJob = Helper.CreatePackageJob(consolidation);
			var container1 = packingJob.Packages.AddNew("CNT");
			var container2 = packingJob.Packages.AddNew("CNT");
			var pallet1 = container1.Packages.AddNew("PLT");
			var pallet2 = container2.Packages.AddNew("PLT");

			var booking = Helper.CreateBooking(consolidation);
			var picInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp);
			var dlvInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery);

			DtbChildEditableService.SetState(Factory, DtbChildEditableServiceState.Consolidation);

			using (var form = new TransportBookingMultiForm(consolidation))
			{
				form.Show();

				form.transportBookingsControl.DtbBookingInstructionsViewsPanel.ViewTabControl.SelectedTab = form.transportBookingsControl.DtbBookingInstructionsViewsPanel.InstructionViewTabPage;
				form.transportBookingsControl.DtbBookingInstructionsViewsPanel.InstructionViewUserControl.AssignAllContainersToAllInstructionsMenuItem.PerformClick();
				AssertContainsExactElementsInAnyOrder(new[] { container1, container2 }, picInstruction.DivotsWithPackages.Packages);
				AssertContainsExactElementsInAnyOrder(new[] { container1, container2 }, dlvInstruction.DivotsWithPackages.Packages);
			}
		}

		public void TestDeleteInstructionFromInstructionsGrid()
		{
			var booking = Helper.CreateBooking();
			var picInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp);
			var picConfirmation = Helper.CreateConfirmation(picInstruction, ConfirmationTypes.Codes.PickUp);
			var dlvInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery);
			var dlvConfirmation = Helper.CreateConfirmation(dlvInstruction, ConfirmationTypes.Codes.Delivery);

			using (var form = new TransportBookingForm(booking))
			{
				form.Show();
				form.SetInstructionView(TransportBookingInstructionView.Instruction);

				var grid = form.InstructionViewsControl.InstructionViewUserControl.InstructionsGrid;
				AssertEquals("Precondition: should have 2 instructions in the grid.", 2, grid.List.Count);
				dlvInstruction.Confirmations[0].KK_Estimated = DateTime.Now;
				dlvInstruction.Confirmations[0].KK_Actual = DateTime.Now;

				form.FireSaveButton();

				grid.SelectSingleElement(dlvInstruction);
				grid.DeleteMenuItem.PerformClick();     // should not blow up

				AssertEquals(1, grid.List.Count);
			}
		}

		public void TestAssignButton_ShouldBeVisible_WhenItIsNormalTB()
		{
			var booking = Helper.CreateBooking();
			booking.KM_IsMaster = false;

			using (var form = new TransportBookingForm(booking))
			{
				form.Show();
				form.SetInstructionView(TransportBookingInstructionView.Instruction);

				AssertEquals("Assign Packages button should be visible for regular Transport Booking", true, form.InstructionViewsControl.InstructionViewUserControl.AssignPackagesButton.Visible);
				AssertEquals("Unassign Packages button should be visible for regular Transport Booking", true, form.InstructionViewsControl.InstructionViewUserControl.UnassignPackagesButton.Visible);
			}
		}

		public void TestAssignButton_ShouldBeVisible_EvenWhenItIsMasterTB()
		{
			var booking = Helper.CreateBooking();
			booking.KM_IsMaster = true;

			using (var form = new TransportBookingForm(booking))
			{
				form.Show();
				form.SetInstructionView(TransportBookingInstructionView.Instruction);

				AssertEquals("Assign Packages button should be visible even for Master Transport Booking", true, form.InstructionViewsControl.InstructionViewUserControl.AssignPackagesButton.Visible);
				AssertEquals("Unassign Packages button should be visible even for Master Transport Booking", true, form.InstructionViewsControl.InstructionViewUserControl.UnassignPackagesButton.Visible);
			}
		}

		public void TestToolStrip_WhenChangedFromNonReadOnlyBookingToReadOnlyBooking_ShouldBeDisabled()
		{
			var consolidation = Helper.CreateConsolidation();
			var booking1 = Helper.CreateBooking(consolidation);
			var booking1Instruction1 = Helper.CreateInstruction(booking1);
			var booking1Instruction2 = Helper.CreateInstruction(booking1);
			var booking2 = Helper.CreateBooking(consolidation);
			var booking2Instruction1 = Helper.CreateInstruction(booking2);
			var booking2Instruction2 = Helper.CreateInstruction(booking2);
			booking2.KM_IsActive = false;

			DtbChildEditableService.SetState(Factory, DtbChildEditableServiceState.Consolidation);

			Factory.Save();

			using (var form = new TransportBookingMultiForm(consolidation))
			{
				form.Show();
				form.transportBookingsControl.SetInstructionView(TransportBookingInstructionView.Instruction);

				AssertEquals("Assign Packages button should be enabled for regular Transport Booking when initially loaded", true, form.transportBookingsControl.DtbBookingInstructionsViewsPanel.InstructionViewUserControl.AssignPackagesButton.Enabled);
				AssertEquals("Move down button should be enabled for regular Transport Booking when initially loaded", true, form.transportBookingsControl.DtbBookingInstructionsViewsPanel.InstructionViewUserControl.MoveDownButton.Enabled);

				form.transportBookingsControl.BookingsModuleButtonGrid.InnerGrid.SelectSingleElement(booking2);
				AssertEquals("Assign Packages button should be disabled for deactivated Transport Booking", false, form.transportBookingsControl.DtbBookingInstructionsViewsPanel.InstructionViewUserControl.AssignPackagesButton.Enabled);
				AssertEquals("Move down button should be disabled for deactivated Transport Booking", false, form.transportBookingsControl.DtbBookingInstructionsViewsPanel.InstructionViewUserControl.MoveDownButton.Enabled);

				form.transportBookingsControl.BookingsModuleButtonGrid.InnerGrid.SelectSingleElement(booking1);
				AssertEquals("Assign Packages button should be enabled for regular Transport Booking when transitioning from deactivated Transport Booking", true, form.transportBookingsControl.DtbBookingInstructionsViewsPanel.InstructionViewUserControl.AssignPackagesButton.Enabled);
				AssertEquals("Move down button should be enabled for regular Transport Booking when transitioning from deactivated Transport Booking", true, form.transportBookingsControl.DtbBookingInstructionsViewsPanel.InstructionViewUserControl.MoveDownButton.Enabled);
			}
		}

		public void TestToolStrip_WhenFirstBookingReadOnly_ShouldBeDisabled()
		{
			var consolidation = Helper.CreateConsolidation();
			var booking1 = Helper.CreateBooking(consolidation);
			var booking1Instruction1 = Helper.CreateInstruction(booking1);
			var booking1Instruction2 = Helper.CreateInstruction(booking1);
			booking1.KM_IsActive = false;
			var booking2 = Helper.CreateBooking(consolidation);
			var booking2Instruction1 = Helper.CreateInstruction(booking2);
			var booking2Instruction2 = Helper.CreateInstruction(booking2);

			DtbChildEditableService.SetState(Factory, DtbChildEditableServiceState.Consolidation);

			Factory.Save();

			using (var form = new TransportBookingMultiForm(consolidation))
			{
				form.Show();
				form.transportBookingsControl.SetInstructionView(TransportBookingInstructionView.Instruction);

				AssertEquals("Assign Packages button should be disabled for deactivated Transport Booking when initially loaded", false, form.transportBookingsControl.DtbBookingInstructionsViewsPanel.InstructionViewUserControl.AssignPackagesButton.Enabled);
				AssertEquals("Move down button should be disabled for deactivated Transport Booking when initially loaded", false, form.transportBookingsControl.DtbBookingInstructionsViewsPanel.InstructionViewUserControl.MoveDownButton.Enabled);

				form.transportBookingsControl.BookingsModuleButtonGrid.InnerGrid.SelectSingleElement(booking2);
				AssertEquals("Assign Packages button should be enabled for regular Transport Booking", true, form.transportBookingsControl.DtbBookingInstructionsViewsPanel.InstructionViewUserControl.AssignPackagesButton.Enabled);
				AssertEquals("Move down button should be enabled for regular Transport Booking", true, form.transportBookingsControl.DtbBookingInstructionsViewsPanel.InstructionViewUserControl.MoveDownButton.Enabled);

				form.transportBookingsControl.BookingsModuleButtonGrid.InnerGrid.SelectSingleElement(booking1);
				AssertEquals("Assign Packages button should be disabled for deactivated Transport Booking when transitioning from regular Transport Booking", false, form.transportBookingsControl.DtbBookingInstructionsViewsPanel.InstructionViewUserControl.AssignPackagesButton.Enabled);
				AssertEquals("Move down button should be disabled for deactivated Transport Booking when transitioning from regular Transport Booking", false, form.transportBookingsControl.DtbBookingInstructionsViewsPanel.InstructionViewUserControl.MoveDownButton.Enabled);
			}
		}

		public void TestMoveUpDownButtons_WhenInstructionChanged()
		{
			var consolidation = Helper.CreateConsolidation();
			var booking1 = Helper.CreateBooking(consolidation);
			var booking2 = Helper.CreateBooking(consolidation);
			var booking2Instruction1 = Helper.CreateInstruction(booking2);
			var booking3 = Helper.CreateBooking(consolidation);
			var booking3Instruction1 = Helper.CreateInstruction(booking3);
			var booking3Instruction2 = Helper.CreateInstruction(booking3);
			var booking3Instruction3 = Helper.CreateInstruction(booking3);
			var booking4 = Helper.CreateBooking(consolidation);
			booking4.KM_IsActive = false;
			var booking4Instruction1 = Helper.CreateInstruction(booking4);
			var booking4Instruction2 = Helper.CreateInstruction(booking4);
			var booking4Instruction3 = Helper.CreateInstruction(booking4);
			var booking4Instruction4 = Helper.CreateInstruction(booking4);
			var booking5 = Helper.CreateBooking(consolidation);
			booking5.KM_IsActive = false;

			DtbChildEditableService.SetState(Factory, DtbChildEditableServiceState.Consolidation);

			Factory.Save();

			using (var form = new TransportBookingMultiForm(consolidation))
			{
				form.Show();
				form.transportBookingsControl.SetInstructionView(TransportBookingInstructionView.Instruction);

				AssertEquals("Precondition: booking1 has one instruction due to the form causing one to be created", 1, booking1.Instructions.Count);
				AssertEquals("Move up button should be disabled for booking with one new instruction", false, form.transportBookingsControl.DtbBookingInstructionsViewsPanel.InstructionViewUserControl.MoveUpButton.Enabled);
				AssertEquals("Move down button should be disabled for booking with one new instruction", false, form.transportBookingsControl.DtbBookingInstructionsViewsPanel.InstructionViewUserControl.MoveDownButton.Enabled);

				form.transportBookingsControl.BookingsModuleButtonGrid.InnerGrid.SelectSingleElement(booking2);
				AssertEquals("Move up button should be disabled for booking with one instruction", false, form.transportBookingsControl.DtbBookingInstructionsViewsPanel.InstructionViewUserControl.MoveUpButton.Enabled);
				AssertEquals("Move down button should be disabled for booking with one instruction", false, form.transportBookingsControl.DtbBookingInstructionsViewsPanel.InstructionViewUserControl.MoveDownButton.Enabled);

				form.transportBookingsControl.BookingsModuleButtonGrid.InnerGrid.SelectSingleElement(booking3);
				AssertEquals("Move up button should be disabled for first instruction", false, form.transportBookingsControl.DtbBookingInstructionsViewsPanel.InstructionViewUserControl.MoveUpButton.Enabled);
				AssertEquals("Move down button should be enabled for first instruction", true, form.transportBookingsControl.DtbBookingInstructionsViewsPanel.InstructionViewUserControl.MoveDownButton.Enabled);

				form.transportBookingsControl.DtbBookingInstructionsViewsPanel.InstructionViewUserControl.InstructionsGrid.SelectSingleElement(booking3Instruction2);
				AssertEquals("Move up button should be enabled for non-first, non-last instruction", true, form.transportBookingsControl.DtbBookingInstructionsViewsPanel.InstructionViewUserControl.MoveUpButton.Enabled);
				AssertEquals("Move down button should be enabled for non-first, non-last instruction", true, form.transportBookingsControl.DtbBookingInstructionsViewsPanel.InstructionViewUserControl.MoveDownButton.Enabled);

				form.transportBookingsControl.DtbBookingInstructionsViewsPanel.InstructionViewUserControl.InstructionsGrid.SelectSingleElement(booking3Instruction3);
				AssertEquals("Move up button should be enabled for last instruction", true, form.transportBookingsControl.DtbBookingInstructionsViewsPanel.InstructionViewUserControl.MoveUpButton.Enabled);
				AssertEquals("Move down button should be disabled for last instruction", false, form.transportBookingsControl.DtbBookingInstructionsViewsPanel.InstructionViewUserControl.MoveDownButton.Enabled);

				form.transportBookingsControl.BookingsModuleButtonGrid.InnerGrid.SelectSingleElement(booking4);
				AssertEquals("Move up button should be disabled for deactivated booking", false, form.transportBookingsControl.DtbBookingInstructionsViewsPanel.InstructionViewUserControl.MoveUpButton.Enabled);
				AssertEquals("Move down button should be disabled for deactivated booking", false, form.transportBookingsControl.DtbBookingInstructionsViewsPanel.InstructionViewUserControl.MoveDownButton.Enabled);

				form.transportBookingsControl.DtbBookingInstructionsViewsPanel.InstructionViewUserControl.InstructionsGrid.SelectSingleElement(booking4Instruction4);
				AssertEquals("Move up button should be disabled for deactivated booking", false, form.transportBookingsControl.DtbBookingInstructionsViewsPanel.InstructionViewUserControl.MoveUpButton.Enabled);
				AssertEquals("Move down button should be disabled for deactivated booking", false, form.transportBookingsControl.DtbBookingInstructionsViewsPanel.InstructionViewUserControl.MoveDownButton.Enabled);

				form.transportBookingsControl.BookingsModuleButtonGrid.InnerGrid.SelectSingleElement(booking3);
				AssertEquals("Move up button should be enabled as the last instruction is selected on regular booking", true, form.transportBookingsControl.DtbBookingInstructionsViewsPanel.InstructionViewUserControl.MoveUpButton.Enabled);
				AssertEquals("Move down button is disabled as the last instruction is selected", false, form.transportBookingsControl.DtbBookingInstructionsViewsPanel.InstructionViewUserControl.MoveDownButton.Enabled);

				form.transportBookingsControl.BookingsModuleButtonGrid.InnerGrid.SelectSingleElement(booking5);
				AssertEquals("Precondition: booking5 should still have no instructions", 0, booking5.Instructions.Count);
				AssertEquals("Move up button should be disabled for booking with no instructions", false, form.transportBookingsControl.DtbBookingInstructionsViewsPanel.InstructionViewUserControl.MoveUpButton.Enabled);
				AssertEquals("Move down button should be disabled for booking with no instructions", false, form.transportBookingsControl.DtbBookingInstructionsViewsPanel.InstructionViewUserControl.MoveDownButton.Enabled);

				form.transportBookingsControl.BookingsModuleButtonGrid.InnerGrid.SelectSingleElement(booking3);
				AssertEquals("Move up button should be disabled as the first instruction is selected when transitioning from booking with no instructions", false, form.transportBookingsControl.DtbBookingInstructionsViewsPanel.InstructionViewUserControl.MoveUpButton.Enabled);
				AssertEquals("Move down button should be enabled as the first instruction is selected when transitioning from booking with no instructions", true, form.transportBookingsControl.DtbBookingInstructionsViewsPanel.InstructionViewUserControl.MoveDownButton.Enabled);
			}
		}

		public void TestParentForm_DisplayModeChanged()
		{
			var consolidation = Helper.CreateConsolidation();
			var booking1 = Helper.CreateBooking(consolidation);
			var booking1Instruction1 = Helper.CreateInstruction(booking1);
			var booking1Instruction2 = Helper.CreateInstruction(booking1);
			var booking2 = Helper.CreateBooking(consolidation);
			var booking2Instruction1 = Helper.CreateInstruction(booking2);
			var booking2Instruction2 = Helper.CreateInstruction(booking2);
			booking2.KM_IsActive = false;

			DtbChildEditableService.SetState(Factory, DtbChildEditableServiceState.Consolidation);

			Factory.Save();

			using (var form = new TransportBookingMultiForm(consolidation))
			{
				form.Show();
				form.transportBookingsControl.SetInstructionView(TransportBookingInstructionView.Instruction);

				AssertEquals("Assign Packages button should be enabled for regular Transport Booking when initially loaded", true, form.transportBookingsControl.DtbBookingInstructionsViewsPanel.InstructionViewUserControl.AssignPackagesButton.Enabled);
				AssertEquals("Move down button should be enabled for regular Transport Booking when initially loaded", true, form.transportBookingsControl.DtbBookingInstructionsViewsPanel.InstructionViewUserControl.MoveDownButton.Enabled);
				AssertEquals("Precondition: existing display mode", ODisplayMode.Browse, form.DisplayMode);

				form.DisplayMode = ODisplayMode.Edit;
				AssertEquals("Assign Packages button should remain enabled when display mode changes", true, form.transportBookingsControl.DtbBookingInstructionsViewsPanel.InstructionViewUserControl.AssignPackagesButton.Enabled);
				AssertEquals("Move down button should remain enabled when display mode changes", true, form.transportBookingsControl.DtbBookingInstructionsViewsPanel.InstructionViewUserControl.MoveDownButton.Enabled);

				form.transportBookingsControl.BookingsModuleButtonGrid.InnerGrid.SelectSingleElement(booking2);
				AssertEquals("Precondition: Assign Packages button should be disabled", false, form.transportBookingsControl.DtbBookingInstructionsViewsPanel.InstructionViewUserControl.AssignPackagesButton.Enabled);
				AssertEquals("Precondition: Move down button should be disabled", false, form.transportBookingsControl.DtbBookingInstructionsViewsPanel.InstructionViewUserControl.MoveDownButton.Enabled);

				form.DisplayMode = ODisplayMode.Browse;
				AssertEquals("Assign Packages button should remain disabled when display mode changes", false, form.transportBookingsControl.DtbBookingInstructionsViewsPanel.InstructionViewUserControl.AssignPackagesButton.Enabled);
				AssertEquals("Move down button should remain disabled when display mode changes", false, form.transportBookingsControl.DtbBookingInstructionsViewsPanel.InstructionViewUserControl.MoveDownButton.Enabled);
			}
		}

		public void TestKB_IsOverriddenInfo_ValueChanged()
		{
			var dummy = Factory.New<DummyWithDtbBooking>();
			var consolidation = Helper.CreateConsolidation(dummy);
			consolidation.KB_IsOverridden = true;
			var booking1 = Helper.CreateBooking(consolidation);
			var booking1Instruction1 = Helper.CreateInstruction(booking1);
			var booking1Instruction2 = Helper.CreateInstruction(booking1);
			var booking2 = Helper.CreateBooking(consolidation);
			var booking2Instruction1 = Helper.CreateInstruction(booking2);
			var booking2Instruction2 = Helper.CreateInstruction(booking2);
			booking2.KM_IsActive = false;

			DtbChildEditableService.SetState(Factory, DtbChildEditableServiceState.Consolidation);

			Factory.Save();

			using (var form = new TransportBookingMultiForm(consolidation))
			{
				form.Show();
				form.transportBookingsControl.SetInstructionView(TransportBookingInstructionView.Instruction);

				AssertEquals("Precondition", ODisplayMode.Browse, form.DisplayMode);
				consolidation.KB_IsOverridden = false;
				consolidation.KB_IsOverridden = true;
				AssertEquals(
					"Change in KB_IsOverridden has changed display mode, so TransportBookingInstructionViewControl method ParentForm_DisplayModeChanged won't be called when KB_IsOverridden is changed",
					ODisplayMode.Edit,
					form.DisplayMode);

				AssertEquals("Assign Packages button should be enabled for regular Transport Booking when initially loaded", true, form.transportBookingsControl.DtbBookingInstructionsViewsPanel.InstructionViewUserControl.AssignPackagesButton.Enabled);
				AssertEquals("Move down button should be enabled for regular Transport Booking when initially loaded", true, form.transportBookingsControl.DtbBookingInstructionsViewsPanel.InstructionViewUserControl.MoveDownButton.Enabled);

				consolidation.KB_IsOverridden = false;
				AssertEquals("Assign Packages button should become disabled when KB_IsOverridden is set to false", false, form.transportBookingsControl.DtbBookingInstructionsViewsPanel.InstructionViewUserControl.AssignPackagesButton.Enabled);
				AssertEquals("Move down button should become disabled when KB_IsOverridden is set to false", false, form.transportBookingsControl.DtbBookingInstructionsViewsPanel.InstructionViewUserControl.MoveDownButton.Enabled);

				form.transportBookingsControl.BookingsModuleButtonGrid.InnerGrid.SelectSingleElement(booking2);
				AssertEquals("Precondition: Assign Packages button should be disabled", false, form.transportBookingsControl.DtbBookingInstructionsViewsPanel.InstructionViewUserControl.AssignPackagesButton.Enabled);
				AssertEquals("Precondition: Move down button should be disabled", false, form.transportBookingsControl.DtbBookingInstructionsViewsPanel.InstructionViewUserControl.MoveDownButton.Enabled);

				consolidation.KB_IsOverridden = true;
				AssertEquals("Assign Packages button should remain disabled when KB_IsOverridden is set to true on a deactivated booking", false, form.transportBookingsControl.DtbBookingInstructionsViewsPanel.InstructionViewUserControl.AssignPackagesButton.Enabled);
				AssertEquals("Move down button should remain disabled when KB_IsOverridden is set to true on a deactivated booking", false, form.transportBookingsControl.DtbBookingInstructionsViewsPanel.InstructionViewUserControl.MoveDownButton.Enabled);

				consolidation.KB_IsOverridden = false;
				form.transportBookingsControl.BookingsModuleButtonGrid.InnerGrid.SelectSingleElement(booking1);
				AssertEquals("Precondition: Assign Packages button should be disabled", false, form.transportBookingsControl.DtbBookingInstructionsViewsPanel.InstructionViewUserControl.AssignPackagesButton.Enabled);
				AssertEquals("Precondition: Move down button should be disabled", false, form.transportBookingsControl.DtbBookingInstructionsViewsPanel.InstructionViewUserControl.MoveDownButton.Enabled);

				consolidation.KB_IsOverridden = true;
				AssertEquals("Assign Packages button should be enabled", true, form.transportBookingsControl.DtbBookingInstructionsViewsPanel.InstructionViewUserControl.AssignPackagesButton.Enabled);
				AssertEquals("Move down button should be enabled", true, form.transportBookingsControl.DtbBookingInstructionsViewsPanel.InstructionViewUserControl.MoveDownButton.Enabled);
			}
		}

		protected override BusinessObjectFactory NewFactory()
		{
			var factory = base.NewFactory();
			DtbChildEditableService.SetState(factory, DtbChildEditableServiceState.Transport);
			return factory;
		}
	}
}
