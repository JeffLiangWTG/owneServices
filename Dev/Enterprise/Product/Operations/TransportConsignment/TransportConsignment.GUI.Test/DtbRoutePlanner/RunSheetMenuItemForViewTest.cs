using System;
using CargoWise.Types;
using Enterprise.TransportConsignment.Business;

namespace Enterprise.TransportConsignment.GUI.Testing
{
	public class RunSheetMenuItemForViewTest : RunSheetMenuItemTest
	{
		#region TestView

		public void TestView()
		{
			AssertEquals(RunSheetView.Drivers, new RunSheetMenuItemForView(RunSheetView.Drivers, null).View);
		}

		#endregion

		#region Implementation

		protected override ZString GetExpectedCaption()
		{
			return RunSheetView.Drivers.GetDescription().Caption;
		}

		protected override ZString GetExpectedName()
		{
			return nameof(RunSheetView.Drivers);
		}

		protected override RunSheetMenuItem GetNewRunSheetMenuItem(EventHandler clickEventHandler)
		{
			return new RunSheetMenuItemForView(RunSheetView.Drivers, clickEventHandler);
		}

		#endregion
	}
}
