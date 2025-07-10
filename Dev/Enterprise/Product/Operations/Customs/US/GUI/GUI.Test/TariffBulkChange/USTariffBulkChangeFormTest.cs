using System.Windows.Forms;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI.Testing
{
	[TestedType(typeof(USTariffBulkChangeForm))]
	sealed class USTariffBulkChangeFormTest : ZFormBasherTest
	{
		public void TestContinueBtn_Click()
		{
			USTariffBulkChange changer = new USTariffBulkChange(Factory);
			changer.Tariffs.AddNew();
			using (USTariffBulkChangeForm form = new USTariffBulkChangeForm(changer))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				form.ContinueBtn.PerformClick();
				Assert(UnitTestUserNotification.Instance.LastMessage.Text.Contains("Error(s) found"));
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				changer.Tariffs[0].OldTariffNum = "4823908000";
				changer.Tariffs[0].NewTariffNum = "4823908001";
				form.ContinueBtn.PerformClick();
				AssertEquals("0 Product(s) changed.\r\n\r\n0 Lookup(s) changed.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		protected override Form GetFormToBashCore() => new USTariffBulkChangeForm(new USTariffBulkChange(Factory));

		protected override bool AllowSaveOnFormForTestHasChanges => false;
	}
}
