using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.GUI.Testing
{
	sealed class ConfirmationProviderTest : TestCaseWithFactory
	{
		public void TestGetConfirmation()
		{
			var confirmationProvider = new ConfirmationProvider();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			AssertEquals(ConfirmationResult.No, confirmationProvider.GetConfirmation("Test", "Please confirm", ConfirmationOption.YesNo));

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			AssertEquals(ConfirmationResult.Yes, confirmationProvider.GetConfirmation("Test", "Please confirm", ConfirmationOption.YesNo));
		}

		public void TestRegister_Factory()
		{
			AssertEquals("Prerequisite", null, Factory.GetValue<IConfirmationProvider>());

			ConfirmationProvider.Register(Factory);
			AssertEquals(true, Factory.GetValue<IConfirmationProvider>() is ConfirmationProvider);
		}

		public void TestRegister_ZForm()
		{
			AssertEquals("Prerequisite", null, Factory.GetValue<IConfirmationProvider>());

			var dummy = Factory.New<DummyBusinessObject>();
			using (var form = new ZForm(dummy))
			{
				ConfirmationProvider.Register(form);
				AssertEquals(true, Factory.GetValue<IConfirmationProvider>() is ConfirmationProvider);
			}
		}
	}
}
