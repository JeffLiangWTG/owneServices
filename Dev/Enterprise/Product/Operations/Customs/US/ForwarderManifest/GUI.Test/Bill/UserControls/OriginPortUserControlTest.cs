using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.ForwarderManifest.Business;
using Enterprise.Customs.US.ForwarderManifest.Business.Test;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.ForwarderManifest.GUI.Test.Bill.UserControls
{
	public class OriginPortUserControlTest : TestCaseWithFactory
	{
		public void TestControls()
		{
			using (var userControl = new OriginPortUserControl())
			{
				AssertNotNull(userControl.Controls.Find("OriginScheduleDTextBox", true));
				AssertNotNull(userControl.Controls.Find("OriginCodeFindBox", true));
			}
		}

		public void TestUNLOCOPortsComponentVisibility()
		{
			RefUNLOCOTestDataHelper.CreateScheduleDPort(Factory, true);
			var header = Factory.New<USExportAsycudaManifestHeader>();
			header.AMA_TransportMode = "SEA";
			var bill = header.Bills.AddNew();
			using (var userControl = new OriginPortUserControl())
			{
				userControl.SetDataBinding(bill, "");
				userControl.Show();
				AssertEquals(false, userControl.FindSingle<ZDropEdit>("OriginScheduleDDropEdit").Visible);
				AssertEquals(true, userControl.FindSingle<ZTextBox>("OriginScheduleDTextBox").Visible);

				bill.ABL_RL_NKOrigin = "USTES";
				AssertEquals(true, userControl.FindSingle<ZDropEdit>("OriginScheduleDDropEdit").Visible);
				AssertEquals(false, userControl.FindSingle<ZTextBox>("OriginScheduleDTextBox").Visible);

				header.AMA_TransportMode = "AIR";
				AssertEquals(false, userControl.FindSingle<ZDropEdit>("OriginScheduleDDropEdit").Visible);
				AssertEquals(true, userControl.FindSingle<ZTextBox>("OriginScheduleDTextBox").Visible);
			}
		}
	}
}
