using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.ForwarderManifest.Business;
using Enterprise.Customs.US.ForwarderManifest.Business.Test;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.ForwarderManifest.GUI.Test.Bill.UserControls
{
	public class LadingPortUserControlTest : TestCaseWithFactory
	{
		public void TestControls()
		{
			using (var userControl = new LadingPortUserControl())
			{
				AssertNotNull(userControl.Controls.Find("LadingPortScheduleDTextBox", true));
				AssertNotNull(userControl.Controls.Find("LadingPortCodeFindBox", true));
			}
		}

		public void TestUNLOCOPortsComponentVisibility()
		{
			RefUNLOCOTestDataHelper.CreateScheduleDPort(Factory, true);
			var header = Factory.New<USExportAsycudaManifestHeader>();
			header.AMA_TransportMode = "SEA";
			var bill = header.Bills.AddNew();
			using (var userControl = new LadingPortUserControl())
			{
				userControl.SetDataBinding(bill, "");
				userControl.Show();
				AssertEquals(false, userControl.FindSingle<ZDropEdit>("LadingPortScheduleDDropEdit").Visible);
				AssertEquals(true, userControl.FindSingle<ZTextBox>("LadingPortScheduleDTextBox").Visible);

				bill.ABL_RL_NKPortOfLoading = "USTES";
				AssertEquals(true, userControl.FindSingle<ZDropEdit>("LadingPortScheduleDDropEdit").Visible);
				AssertEquals(false, userControl.FindSingle<ZTextBox>("LadingPortScheduleDTextBox").Visible);

				header.AMA_TransportMode = "AIR";
				AssertEquals(false, userControl.FindSingle<ZDropEdit>("LadingPortScheduleDDropEdit").Visible);
				AssertEquals(true, userControl.FindSingle<ZTextBox>("LadingPortScheduleDTextBox").Visible);
			}
		}
	}
}
