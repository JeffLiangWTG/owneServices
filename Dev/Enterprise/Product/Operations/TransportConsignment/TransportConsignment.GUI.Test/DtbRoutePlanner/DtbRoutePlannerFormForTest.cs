using System;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Enterprise.TransportConsignment.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.TransportConsignment.GUI.Testing
{
	public class DtbRoutePlannerFormForTest : DtbRoutePlannerForm
	{
		public DtbRoutePlannerFormForTest(DtbRoutePlannerCollection planners)
			: base(planners)
		{
		}

		// planner viewmode buttons
		public new PlannerButton NextAvailableButton { get { return base.NextAvailableButton; } }
		public new PlannerButton PickupsOnlyButton { get { return base.PickupsOnlyButton; } }
		public new PlannerButton DeliveriesOnlyButton { get { return base.DeliveriesOnlyButton; } }
		public new PlannerButton DirectButton { get { return base.DirectButton; } }

		// buttons
		public new ZToolStripButton AssignPartialButton { get { return base.AssignPartialButton; } }
		public new ZToolStripButton RefreshButton { get { return base.RefreshButton; } }
		public new ZToolStripButton ConsignmentDetailsButton { get { return base.ConsignmentDetailsButton; } }
		public new ZToolStripButton RunSheetDetailsButton { get { return base.RunSheetDetailsButton; } }
		public new ZToolStripButton FiltersButton { get { return base.FiltersButton; } }
		public new ZToolStripButton RunSheetsArrowButton { get { return base.RunSheetsArrowButton; } }
		public new ZToolStripDropDownButton RunSheetsDropButton { get { return base.RunSheetsDropButton; } }
		public new ZToolStripButton AssignAddressPointButton { get { return base.AssignAddressPointButton; } }

		// split containers
		public new SplitContainer AddressPointsAndConfirmationsSplitContainer { get { return base.AddressPointsAndConfirmationsSplitContainer; } }
		public new SplitContainer MainSplitContainer { get { return base.MainSplitContainer; } }
		public new SplitContainer RunSheetsVerticalSplitContainer { get { return base.RunSheetsVerticalSplitContainer; } }
		public new SplitContainer RunSheetsHorizontalSplitContainer { get { return base.RunSheetsHorizontalSplitContainer; } }

		// grids
		public ZGrid FilterControlGrid { get { return base.FilterControl.Grid; } }
		public ZGrid DetailsGrid { get { return base.DetailsUserControl.ConfirmationsGrid; } }
		public ZGrid DirectModeDetailsGrid { get { return base.DirectDetailsUserControl.ConfirmationsGrid; } }
		public ZGrid DriversGrid { get { return base.DriverFilterControl.Grid; } }
		public ZGrid CarriersGrid { get { return base.CarrierFilterControl.Grid; } }
		public ZGrid VehiclesGrid { get { return base.VehicleFilterControl.Grid; } }
		public new ZGrid RunSheetsGrid { get { return base.RunSheetsGrid; } }
		public ZGrid RunSheetInstructionsGrid { get { return RunSheetInstructionsControl.InstructionsGrid; } }

		// misc
		public bool IsFilterVisible { get { return FilterControl.IsFilterVisible; } }
		public new bool IsRunSheetPaneVisible { get { return base.IsRunSheetPaneVisible; } }

		public ZFilterStripCommonControl GetFilterControl()
		{
			return FilterControl;
		}

		// run sheet menu items
		public RunSheetMenuItemForView GetRunSheetViewMenuItem(RunSheetView view)
		{
			return RunSheetViewMenuItems.Single(i => i.View == view);
		}
		public RunSheetMenuItemForDay GetRunSheetDayMenuItem(RunSheetDay day)
		{
			return RunSheetDayMenuItems.Single(i => i.Day == day);
		}

		// Calendar
		public ZPopupCalendar Calendar_ForTesting
		{
			get { return Calendar; }
		}

		public void DeactivatePopupCalendar()
		{
			var onDeactivate = typeof(ZForm).GetMethod("OnDeactivate", BindingFlags.Instance | BindingFlags.NonPublic);
			onDeactivate.Invoke(Calendar, new[] { new EventArgs() });
		}
	}
}
