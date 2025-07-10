using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.ForwarderManifest.Business;
using Enterprise.Customs.US.ForwarderManifest.Business.Test;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.ForwarderManifest.GUI.Test.Bill.UserControls
{
	public class DeparturePortUserControlTest : TestCaseWithFactory
	{
		public void TestControls()
		{
			using (var userControl = new DeparturePortUserControl())
			{
				AssertNotNull(userControl.Controls.Find("DeparturePortScheduleDTextBox", true));
				AssertNotNull(userControl.Controls.Find("DeparturePortCodeFindBox", true));
			}
		}

		public void TestUNLOCOPortsComponentVisibility()
		{
			RefUNLOCOTestDataHelper.CreateScheduleDPort(Factory, true);
			var header = Factory.New<USExportAsycudaManifestHeader>();
			header.AMA_TransportMode = "SEA";
			var bill = header.Bills.AddNew();
			using (var userControl = new DeparturePortUserControl())
			{
				userControl.SetDataBinding(bill, "");
				userControl.Show();
				AssertEquals(false, userControl.FindSingle<ZDropEdit>("DeparturePortScheduleDDropEdit").Visible);
				AssertEquals(true, userControl.FindSingle<ZTextBox>("DeparturePortScheduleDTextBox").Visible);

				header.AMA_RL_NKPortOfFinalDeparture = "USTES";
				AssertEquals(true, userControl.FindSingle<ZDropEdit>("DeparturePortScheduleDDropEdit").Visible);
				AssertEquals(false, userControl.FindSingle<ZTextBox>("DeparturePortScheduleDTextBox").Visible);

				header.AMA_TransportMode = "AIR";
				AssertEquals(false, userControl.FindSingle<ZDropEdit>("DeparturePortScheduleDDropEdit").Visible);
				AssertEquals(true, userControl.FindSingle<ZTextBox>("DeparturePortScheduleDTextBox").Visible);
			}
		}
	}
}
