using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportBookings.Business.Testing;
using Enterprise.TransportCommon.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.TransportBookings.GUI.Testing
{
	public class DtbInstructionViewsControlTest : DtbBookingTestCaseWithFactory
	{
		public void TestUpdatePackagesTabPage()
		{
			using (var instructionViewsControl = new DtbInstructionViewsControl())
			{
				instructionViewsControl.PackageViewUserControl.TopLevelSplitContainer.Width = instructionViewsControl.InstructionViewUserControl.GridsSplitContainer.SplitterDistance - 100;
				instructionViewsControl.View = TransportBookingInstructionView.Package;

				AssertNoExceptionThrown("No exception is expected when the method is called", () => instructionViewsControl.UpdateTabPages());

				Assert("Splitter distance is: " + instructionViewsControl.PackageViewUserControl.TopLevelSplitContainer.SplitterDistance,
							 instructionViewsControl.PackageViewUserControl.TopLevelSplitContainer.SplitterDistance >= 0);
			}
		}

		public void TestIsAdditionalReferencesVisible()
		{
			using (var form = new ZForm())
			using (var instructionViewsControlWithAdditionalReferences = new DtbInstructionViewsControl())
			using (var instructionViewsControlWithoutAdditionalReferences = new DtbInstructionViewsControl())
			{
				form.Controls.Add(instructionViewsControlWithAdditionalReferences);
				form.Controls.Add(instructionViewsControlWithoutAdditionalReferences);
				instructionViewsControlWithAdditionalReferences.IsAdditionalReferencesVisible = true;
				Assert("Precondition", instructionViewsControlWithAdditionalReferences.IsAdditionalReferencesVisible);
				Assert("Precondition", !instructionViewsControlWithoutAdditionalReferences.IsAdditionalReferencesVisible);

				form.Show();
				AssertEquals(true, ((ZTabPage)instructionViewsControlWithAdditionalReferences.Controls.Find("AdditionalReferencesTabPage", true).Single()).TabVisible);
				AssertEquals(0, instructionViewsControlWithoutAdditionalReferences.Controls.Find("AdditionalReferencesTabPage", true).Length);
			}
		}

		public void TestPackageViewCastsCorrectly()
		{
			var booking = Helper.CreateBooking();

			using (var form = new DummyBookingFormForControlBinding(booking))
			{
				var instructionViewsControl = new DtbInstructionViewsControl();
				form.AddControlAndShowForm(instructionViewsControl);

				booking.KM_KT_NKBookingTemplate = "LC2C";
				var divot = booking.PackageDivots.AddNew();
				var pkg = Helper.CreatePackage("p1", divot);
				booking.FirstPickup.DivotsWithPackages.AddPackage(pkg);

				var grid = (ZGrid)form.Controls.Find("PackageViewDatesGrid", true)[0];
				var tab = GetTab(instructionViewsControl, "PackageViewTabPage");
				tab.Show();
				grid.SelectAllElements();

				var button = grid.ContextMenu.MenuItems.FindByText("View Data Version Logs...");
				AssertNoExceptionThrown(() => button.PerformClick());
			}
		}

		public void TestCustomFieldsTabs()
		{
			using (var instructionViewsControl = new DtbInstructionViewsControl())
			{
				// Preconditions
				AssertTabExists(instructionViewsControl, "InstructionViewTabPage", tabOnControl: true);
				AssertTabExists(instructionViewsControl, "PackageViewTabPage", tabOnControl: true);
				AssertTabExists(instructionViewsControl, "CustomFieldsViewTabPage", tabOnControl: true);

				var instructionViewTabPage = GetTab(instructionViewsControl, "InstructionViewTabPage");
				var packageViewTabPage = GetTab(instructionViewsControl, "PackageViewTabPage");
				var customFieldsViewTabPage = GetTab(instructionViewsControl, "CustomFieldsViewTabPage");

				var instructionView_CustomFieldsTabPage = (ZTabPage)instructionViewTabPage.Controls["InstructionViewSplitContainer"].Controls[1].Controls["InstructionViewDatesGroupBox"].Controls["InstructionViewDatesTabControl"].Controls["CustomFieldsTabPage"];

				AssertEquals("Under the Instruction View tab in the Dates and References group box the Custom Fields tab should have the correct caption.", "Custom Fields", instructionView_CustomFieldsTabPage.CaptionResourceString.Caption);

				AssertEquals("Under the Package View tab, the Dates and References group box should only have 1 control.", 1, packageViewTabPage.Controls["PackageViewSplitContainer"].Controls[1].Controls["PackageViewDatesGroupBox"].Controls.Count);
				AssertEquals("Under the Package View tab, no tab should exist in the Dates and References group box, only the Dates Grid.", "PackageViewDatesGrid", packageViewTabPage.Controls["PackageViewSplitContainer"].Controls[1].Controls["PackageViewDatesGroupBox"].Controls[0].Name);

				AssertEquals("The Custom Fields View tab should have the correct caption.", "Custom Fields", customFieldsViewTabPage.CaptionResourceString.Caption);
			}
		}

		void AssertTabExists(DtbInstructionViewsControl instructionViewsControl, ZString controlName, bool tabOnControl)
		{
			if (tabOnControl)
			{
				AssertNotNull("Wrong visibility on this ViewMode for the tab " + controlName, GetTab(instructionViewsControl, controlName));
			}
			else
			{
				AssertNull("Wrong visibility on this ViewMode for the tab " + controlName, GetTab(instructionViewsControl, controlName));
			}
		}

		public void TestTransportBookingsTab_NotMasterBooking_TabNotVisible()
		{
			AssertTransportBookingsTabVisibility(false, "Transport Booking Tab should not be visible");
		}

		public void TestTransportBookingsTab_IsMasterBooking_TabVisible()
		{
			AssertTransportBookingsTabVisibility(true, "Transport Booking Tab should be visible");
		}

		void AssertTransportBookingsTabVisibility(bool bookingIsMaster, string message)
		{
			var booking = Helper.CreateBooking();
			booking.KM_IsMaster = bookingIsMaster;

			using (var form = new DummyBookingFormForControlBinding(booking))
			{
				var instructionViewsControl = new DtbInstructionViewsControl();
				form.AddControlAndShowForm(instructionViewsControl);
				form.Show();
				Application.DoEvents();

				var tab = instructionViewsControl.TransportBookingsTabPage;
				AssertEquals(message, bookingIsMaster, tab.TabVisible);
			}
		}

		public void TestKM_StatusInfo_ValueChanged()
		{
			var consolidation = Helper.CreateConsolidation();
			var booking1 = Helper.CreateBooking(consolidation);
			var booking1Instruction1 = Helper.CreateInstruction(booking1);
			var booking1Instruction2 = Helper.CreateInstruction(booking1);

			Factory.Save();

			using (var form = new TransportBookingMultiForm(consolidation))
			{
				form.Show();
				form.transportBookingsControl.SetInstructionView(TransportBookingInstructionView.Instruction);

				AssertEquals("Precondition", ODisplayMode.Browse, form.DisplayMode);
				booking1.KM_TransportReference = "Trigger display mode change";
				AssertEquals(
					"Change in KM_TransportReference has changed display mode, so TransportBookingInstructionViewControl method ParentForm_DisplayModeChanged won't be called when KM_Status is changed",
					ODisplayMode.Edit,
					form.DisplayMode);
				AssertEquals("Assign Packages button should be enabled for regular Transport Booking when initially loaded", true, form.transportBookingsControl.DtbBookingInstructionsViewsPanel.InstructionViewUserControl.AssignPackagesButton.Enabled);
				AssertEquals("Move down button should be enabled for regular Transport Booking when initially loaded", true, form.transportBookingsControl.DtbBookingInstructionsViewsPanel.InstructionViewUserControl.MoveDownButton.Enabled);

				booking1.KM_Status = TransportStatuses.Codes.Delivered;
				AssertEquals("Assign Packages button should remain enabled when status is changed to something other than held", true, form.transportBookingsControl.DtbBookingInstructionsViewsPanel.InstructionViewUserControl.AssignPackagesButton.Enabled);
				AssertEquals("Move down button should remain enabled when status is changed to something other than held", true, form.transportBookingsControl.DtbBookingInstructionsViewsPanel.InstructionViewUserControl.MoveDownButton.Enabled);

				booking1.KM_Status = TransportStatuses.Codes.Held;
				AssertEquals("Assign Packages button should be disabled as the booking is held, and in addition is readonly", false, form.transportBookingsControl.DtbBookingInstructionsViewsPanel.InstructionViewUserControl.AssignPackagesButton.Enabled);
				AssertEquals("Move down button should be disabled as the booking is held, and in addition is readonly", false, form.transportBookingsControl.DtbBookingInstructionsViewsPanel.InstructionViewUserControl.MoveDownButton.Enabled);
			}
		}

		ZTabPage GetTab(DtbInstructionViewsControl instructionViewsControl, ZString controlName)
		{
			return (ZTabPage)instructionViewsControl.Controls["ViewGroupBox"].Controls["ViewTabControl"].Controls[controlName];
		}

		protected override BusinessObjectFactory NewFactory()
		{
			var factory = base.NewFactory();
			DtbChildEditableService.SetState(factory, DtbChildEditableServiceState.Consolidation);
			return factory;
		}
	}
}
