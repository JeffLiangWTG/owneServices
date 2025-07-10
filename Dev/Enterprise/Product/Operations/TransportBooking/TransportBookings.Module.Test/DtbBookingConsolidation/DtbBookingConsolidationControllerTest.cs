using System;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportBookings.GUI;
using Enterprise.TransportCommon.Business;
using Enterprise.TransportCommon.Registry;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.TransportBookings.Module.Testing
{
	[TestedType(typeof(DtbBookingConsolidationController))]
	public class DtbBookingConsolidationControllerTest : ZPopupControllerBasherTest
	{
		public void TestNewConsolidationIsMultiJob()
		{
			var form = (TransportBookingMultiForm)Controller.ShowNewForm();
			AssertEquals(TransportConsolidationJobTypes.Codes.BookingTransportConsolidation, ((DtbBookingConsolidation)form.BusinessEntity).KB_JobType);
		}

		public void TestSetStrategyProvider()
		{
			var controller = new DtbBookingConsolidationController();
			AssertEquals(DtbChildEditableServiceState.Consolidation, DtbChildEditableService.GetState(controller.Factory));
			AssertEquals(DtbFormState.Booking, DtbFormStateService.GetState(controller.Factory));
		}

		public void TestShowModelessForm()
		{
			var controller = new DtbBookingConsolidationController();

			using (TransportRegistry.Instance.DefaultTransportBookingTabView.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, BookingViews.Codes.Standard))
			using (var form = (TransportBookingMultiForm)controller.ShowNewForm())
			{
				var instructionViewControl = GUITestHelper.FindControl<DtbInstructionViewsControl>(form.Controls, "DtbBookingInstructionsViewsPanel");
				var viewTabControl = GUITestHelper.FindControl<ZTabControl>(instructionViewControl.Controls, "ViewTabControl");
				var standardTabPage = GUITestHelper.FindControl<ZTabPage>(viewTabControl.Controls, "StandardViewTabPage");
				AssertEquals("Selected tab should be the Standard View Tab Page", standardTabPage, viewTabControl.SelectedTab);
			}

			using (TransportRegistry.Instance.DefaultTransportBookingTabView.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, BookingViews.Codes.Instruction))
			using (var form = (TransportBookingMultiForm)controller.ShowNewForm())
			{
				var instructionViewControl = GUITestHelper.FindControl<DtbInstructionViewsControl>(form.Controls, "DtbBookingInstructionsViewsPanel");
				var viewTabControl = GUITestHelper.FindControl<ZTabControl>(instructionViewControl.Controls, "ViewTabControl");
				var instructionsTabPage = GUITestHelper.FindControl<ZTabPage>(viewTabControl.Controls, "InstructionViewTabPage");
				AssertEquals("Selected tab should be the Instruction View Tab Page", instructionsTabPage, viewTabControl.SelectedTab);
			}
		}

		public void TestControllerID()
		{
			AssertEquals(ControllerIDs.DtbBookingConsolidation, new DtbBookingConsolidationController().ID);
		}

		public void TestID()
		{
			AssertEquals(ModuleIDs.DtbBookingConsolidation, new DtbBookingConsolidationController().ModuleID);
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.DtbBookingConsolidation;
		}
	}
}
