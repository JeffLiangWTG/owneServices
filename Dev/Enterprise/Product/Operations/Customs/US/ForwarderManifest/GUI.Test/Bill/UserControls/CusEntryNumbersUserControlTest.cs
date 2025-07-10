using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.ForwarderManifest.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.ForwarderManifest.GUI.Test.Bill.UserControls
{
	public class CusEntryNumbersUserControlTest : TestCaseWithFactory
	{
		public void TestMoreNumbersButton_Click()
		{
			var header = Factory.New<USExportAsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			using (var userControl = new ITNUserControl())
			{
				userControl.SetDataBinding(bill, "");
				userControl.Show();

				var moreButton = userControl.FindSingle<ZButton>("AES ITN");
				moreButton.PerformClick();

				using (var newForm = ZFormModaliser.LastFormShownDialogForTest)
				{
					AssertNotNull(newForm);
					AssertEquals("Module New Object title", "AES ITN", newForm.Text);
					newForm.Close();
				}
			}

			using (var userControl = new InBondUserControl())
			{
				userControl.SetDataBinding(bill, "");
				userControl.Show();

				var moreButton = userControl.FindSingle<ZButton>("In-Bond Number");
				moreButton.PerformClick();

				using (var newForm = ZFormModaliser.LastFormShownDialogForTest)
				{
					AssertNotNull(newForm);
					AssertEquals("Module New Object title", "In-Bond Number", newForm.Text);
					newForm.Close();
				}
			}
		}
	}
}
