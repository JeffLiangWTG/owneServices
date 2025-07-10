using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.LocalCartage.Module.Testing
{
	public class CartageWorkSheetFilterControlTest : TestCaseWithFactory
	{
		public void TestOnGetCartageLegsToPrint()
		{
			var runSheets = new ModuleCartageRunSheetCollection(Factory);
			var runSheet = runSheets.AddNew();
			var strip = new CartageWorkSheetFilterStripBusinessObject();
			using (var control = new CartageWorkSheetFilterControl(runSheets, strip))
			{
				var documentLegs = new DocumentCartageLegCollection(runSheet.CartageLegs);
				var options = new DocumentCartageLegOptions(documentLegs);
				var e = new DocumentCartageLegEventArgs(options);
				runSheet.RaiseOnGetCartageLegsToPrint(e);
				AssertEquals("There are no Port Transport Legs to print.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(1, control.runSheet_OnGetCartageLegsToPrintHitCountTest);
				var runSheet2 = runSheets.AddNew();
				runSheet2.RaiseOnGetCartageLegsToPrint(e);
				AssertEquals("There are no Port Transport Legs to print.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(2, control.runSheet_OnGetCartageLegsToPrintHitCountTest);
				runSheet.RaiseOnGetCartageLegsToPrint(e);
				AssertEquals("There are no Port Transport Legs to print.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(3, control.runSheet_OnGetCartageLegsToPrintHitCountTest);
			}
		}

		public void TestGetCartageLegsToPrint_Regression_UsesCollectionTypeProvidedByAssemblyData()
		{
			var runSheets = new RunSheetJobData().GetBusinessObjectCollection(Factory);
			var runSheet = (CommonWorkSheet)(runSheets.AddNew());
			var strip = new CartageWorkSheetFilterStripBusinessObject();
			AssertNoExceptionThrown("Cartage worksheet filter control is expecting a collection type not provided by assembly data for run sheets", () =>
			{
				using (var control = new CartageWorkSheetFilterControl(runSheets, strip))
				{
					var documentLegs = new DocumentCartageLegCollection(runSheet.CartageLegs);
					var options = new DocumentCartageLegOptions(documentLegs);
					var e = new DocumentCartageLegEventArgs(options);
					runSheet.RaiseOnGetCartageLegsToPrint(e);
					AssertEquals("There are no Port Transport Legs to print.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(1, control.runSheet_OnGetCartageLegsToPrintHitCountTest);
					var runSheet2 = (CommonWorkSheet)(runSheets.AddNew());
					runSheet2.RaiseOnGetCartageLegsToPrint(e);
					AssertEquals("There are no Port Transport Legs to print.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(2, control.runSheet_OnGetCartageLegsToPrintHitCountTest);
					runSheet.RaiseOnGetCartageLegsToPrint(e);
					AssertEquals("There are no Port Transport Legs to print.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(3, control.runSheet_OnGetCartageLegsToPrintHitCountTest);
				}
			});
		}
	}
}
