using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.US.ForwarderManifest.Business;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.US.ForwarderManifest.GUI.Test.Bill.UserControls
{
	public class IssuerSCACUserControlTest : TestCaseWithFactory
	{
		public void TestChangeBolType()
		{
			var header = Factory.New<USExportAsycudaManifestHeader>();
			header.AMA_ApplicationCode = ManifestBase.ApplicationCodeTypeList.Codes.Consolidator;
			var masterBill = header.MasterBill;
			var bill = header.Bills.AddNew();
			using (var userControl = new IssuerSCACUserControl())
			{
				userControl.SetDataBinding(header, "");
				userControl.Show();

				AssertEquals(AsycudaBill.ChildBolCode, masterBill.ABL_BolType);
				AssertEquals(AsycudaBill.ChildBolCode, bill.ABL_BolType);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddOKAnswer();
				masterBill.ABL_BillIssuer = "ABCD";
				header.MasterBOL = "1234567";
				AssertEquals(AsycudaBill.ChildBolCode, masterBill.ABL_BolType);
				AssertEquals(ShipmentTypes.StandardHouse, bill.ABL_BolType);
				AssertEquals("ABCD", masterBill.ABL_BillIssuer);
				AssertEquals("1234567", header.MasterBOL);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddOKAnswer();
				header.MasterBOL = string.Empty;
				masterBill.ABL_BillIssuer = string.Empty;
				AssertEquals(AsycudaBill.ChildBolCode, masterBill.ABL_BolType);
				AssertEquals(AsycudaBill.ChildBolCode, bill.ABL_BolType);
				AssertEquals(string.Empty, masterBill.ABL_BillIssuer);
				AssertEquals(string.Empty, header.MasterBOL);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				masterBill.ABL_BillIssuer = "ABCD";
				header.MasterBOL = "1234567";
				AssertEquals(AsycudaBill.ChildBolCode, masterBill.ABL_BolType);
				AssertEquals(AsycudaBill.ChildBolCode, bill.ABL_BolType);
				AssertEquals(string.Empty, masterBill.ABL_BillIssuer);
				AssertEquals(string.Empty, header.MasterBOL);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddOKAnswer();
				masterBill.ABL_BillIssuer = "ABCD";
				header.MasterBOL = "1234567";
				AssertEquals(AsycudaBill.ChildBolCode, masterBill.ABL_BolType);
				AssertEquals(ShipmentTypes.StandardHouse, bill.ABL_BolType);
				AssertEquals("ABCD", masterBill.ABL_BillIssuer);
				AssertEquals("1234567", header.MasterBOL);
			}
		}
	}
}
