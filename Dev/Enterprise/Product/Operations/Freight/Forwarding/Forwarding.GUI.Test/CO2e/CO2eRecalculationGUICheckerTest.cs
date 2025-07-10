using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	sealed class CO2eRecalculationGUICheckerTest : TestCaseWithFactory
	{
		public void TestShouldRecalculate()
		{
			ICO2eRecalculationChecker recalculationChecker = new CO2eRecalculationGUIChecker();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			var resultNo = recalculationChecker.ShouldRecalculate(null);
			AssertEquals(false, resultNo);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			var resultYes = recalculationChecker.ShouldRecalculate(null);
			AssertEquals(true, resultYes);
		}

		public void TestRegister_Factory()
		{
			AssertEquals("Prerequisite", null, Factory.GetValue<ICO2eRecalculationChecker>());

			CO2eRecalculationGUIChecker.Register(Factory);
			AssertEquals(true, Factory.GetValue<ICO2eRecalculationChecker>() is CO2eRecalculationGUIChecker);
		}
	}
}
