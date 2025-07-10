using System;
using CargoWise.Types;
using Enterprise.TransportConsignment.Business;

namespace Enterprise.TransportConsignment.GUI.Testing
{
	public class RunSheetMenuItemForDayTest : RunSheetMenuItemTest
	{
		#region TestDay

		public void TestDay()
		{
			AssertEquals(RunSheetDay.Today, new RunSheetMenuItemForDay(RunSheetDay.Today, null).Day);
		}

		#endregion

		#region Implementation

		protected override ZString GetExpectedCaption()
		{
			return nameof(RunSheetDay.Today);
		}

		protected override ZString GetExpectedName()
		{
			return nameof(RunSheetDay.Today);
		}

		protected override RunSheetMenuItem GetNewRunSheetMenuItem(EventHandler clickEventHandler)
		{
			return new RunSheetMenuItemForDay(RunSheetDay.Today, clickEventHandler);
		}

		#endregion
	}
}
