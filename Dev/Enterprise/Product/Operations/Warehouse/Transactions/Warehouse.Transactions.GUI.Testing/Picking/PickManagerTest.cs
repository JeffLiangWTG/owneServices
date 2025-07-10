using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	public class PickManagerTest : WhsGuiTestCaseWithFactory
	{
		public void TestGetCartonizationProgressForm()
		{
			using (var form = new ZForm())
			{
				form.Show();

				using (var progressForm = PickManager.GetCartonizationProgressForm("AAA", form))
				{
					var lastForm = ZFormModaliser.LastFormShownForTest;
					AssertNotNull("Should have shown progress form.", lastForm);
					AssertType<ProgressForm>("Should have shown progress form.", lastForm);
					AssertEquals("Should have shown progress form.", true, lastForm.Enabled);

					var progressBarForm = (ProgressForm)lastForm;
					AssertEquals("Should have no progress bar.", false, progressBarForm.ShowProgressBar);
					AssertEquals("Should have no cancel button.", false, progressBarForm.ShowCancelButton);
					AssertEquals("Current Status", "AAA", progressBarForm.Status);
				}
			}
		}
	}
}