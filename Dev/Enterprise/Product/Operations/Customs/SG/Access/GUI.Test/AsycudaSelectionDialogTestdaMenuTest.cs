using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.SG.Access.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.SG.Access.GUI.Testing
{
	public class AsycudaSelectionDialogTestdaMenuTest : TestCaseWithFactory
	{
		public void TestSendManifest_SelectedItemTextOnDialog()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill1 = header.Bills.AddNew();
			bill1.ABL_GoodsDescription = "Bill1";
			var bill2 = header.Bills.AddNew();
			bill2.ABL_GoodsDescription = "Bill2";
			var bill3 = header.Bills.AddNew();
			bill3.ABL_GoodsDescription = "Bill3";
			var bill4 = header.Bills.AddNew();
			bill4.ABL_GoodsDescription = "Bill4";
			header.AMA_ManifestType = "IAM";
			header.AMA_CustomsOffice = "X";
			var billCountries = header.Bills.OfType<AsycudaBill>().Where(x => x != null).ToArray();
			using (var dlg = GetAsycudaItemSelectionDialog(new MessageChooser(header, billCountries, true)))
			{
				dlg.SelectOnlyBillNodes_ForTestOnly(bizoPK => bizoPK == bill1.PK);
				AssertEquals("1 Selected", "1 of 4 bill(s) selected.", dlg.SeletedItem);
				dlg.SelectOnlyBillNodes_ForTestOnly(bizoPK => (bizoPK == bill2.PK || bizoPK == bill1.PK));
				AssertEquals("2 Selected", "2 of 4 bill(s) selected.", dlg.SeletedItem);
			}
		}

		public void TestDoNotSendWhenHasMessageErrors()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill1 = header.Bills.AddNew();
			bill1.ABL_BillNumber = "BILL1";
			var bill2 = header.Bills.AddNew();
			bill2.ABL_BillNumber = "BILL2";
			Factory.Save();
			var billCountries = header.Bills.OfType<AsycudaBill>().Where(x => x != null).ToArray();
			var chooser = new MessageChooser(header, billCountries, true);
			chooser.Validation.ValidateAll();
			Assert(chooser.HasMessageErrors);
			using (var dlg = GetAsycudaItemSelectionDialog(chooser))
			{
				dlg.Show();
				var fromIsClosed = false;
				dlg.FormClosed += (sender, e) => fromIsClosed = true;
				Env.Security.GlobalManifestSendWithMessageErrors.IsAllowed = false;
				var sendButton = (ZButton)dlg.Controls.Find("SendButton", true).First();
				sendButton.PerformClick();
				AssertEquals("Not Sent", DialogResult.None, dlg.DialogResult);
				Assert("Form is not closed", !fromIsClosed);
				AssertEquals(@"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Operate -> Customs Global -> Global Manifest -> Send With Message Errors", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessages();
				Env.Security.GlobalManifestSendWithMessageErrors.IsAllowed = true;
				sendButton.PerformClick();
				AssertEquals("Sent", DialogResult.OK, dlg.DialogResult);
				Assert("Form has been closed", fromIsClosed);
				AssertNullOrEmpty(string.Empty, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		AsycudaItemSelectionDialog GetAsycudaItemSelectionDialog(MessageChooser chooser)
		{
			return new AsycudaItemSelectionDialog(chooser, "BillsForManifestTest");
		}
	}
}
