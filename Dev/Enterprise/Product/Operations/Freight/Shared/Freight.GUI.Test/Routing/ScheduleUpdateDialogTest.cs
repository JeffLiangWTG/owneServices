using System.Reflection;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Freight.Common.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.GUI.Testing
{
	[TestedType(typeof(ScheduleUpdateDialog))]
	sealed class ScheduleUpdateDialogTest : BasherTest
	{
		#region ShowDialog

		public void TestShowDialog()
		{
			QueryFreshMatchBehaviourArgs args = new QueryFreshMatchBehaviourArgs()
			{
				DateName = "[date]",
				FoundDate = new ZDateTime(2007, 02, 05, 11, 0, 0),
				RequestedDate = new ZDateTime(2007, 02, 05, 13, 0, 0)
			};

			ScheduleUpdateDialog.ShowDialog(args);
			ScheduleUpdateDialog dialog = (ScheduleUpdateDialog)ZFormModaliser.LastFormShownDialogForTest;
			string text = GetControl<Label>(dialog, "textLabel").Text;
			string expected = $"{Core.Constants.ProductName} has found a more appropriate schedule to link to, but this new schedule has a different [date].\r\nDo you want to keep the schedules existing date, update it or create an entirely new schedule?\r\n\r\nSchedule [date]: 05-Feb-07 11:00\r\nEntered [date]: 05-Feb-07 13:00";

			AssertMultilineASCIIEquals("Dialog Text", expected, text);
		}

		#endregion

		#region Implementation

		T GetControl<T>(ScheduleUpdateDialog dialog, string name)
		{
			return (T)typeof(ScheduleUpdateDialog).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(dialog);
		}

		public override Form GetFormToBash()
		{
			return new ScheduleUpdateDialog();
		}

		#endregion
	}
}
