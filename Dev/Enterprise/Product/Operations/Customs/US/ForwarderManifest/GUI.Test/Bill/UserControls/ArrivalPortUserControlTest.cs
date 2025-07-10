using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.ForwarderManifest.Business;
using Enterprise.Customs.US.ForwarderManifest.Business.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.ForwarderManifest.GUI.Test.Bill.UserControls
{
	public class ArrivalPortUserControlTest : TestCaseWithFactory
	{
		public void TestControls()
		{
			using (var userControl = new ArrivalPortUserControl())
			{
				AssertNotNull(userControl.Controls.Find("ArrivalPortScheduleKTextBox", true));
				AssertNotNull(userControl.Controls.Find("ArrivalPortCodeFindBox", true));
			}
		}

		public void TestUNLOCOPortsComponentVisibility()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("PORT", "Port");
			helper.CreateNewOrGetExistingCusCodeList("US", "PORT", "60001", "60001 Port", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList("US", "PORT", "60002", "60002 Port", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			Factory.Save();

			RefUNLOCOTestDataHelper.CreateLocoMapIfNotExists(Factory, "60001", "USLAX", USLocoMapSystemUsageList.Codes.SCK);
			RefUNLOCOTestDataHelper.CreateLocoMapIfNotExists(Factory, "60002", "USLAX", USLocoMapSystemUsageList.Codes.SCK);
			var header = Factory.New<USExportAsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			using (var userControl = new ArrivalPortUserControl())
			{
				userControl.SetDataBinding(bill, "");
				userControl.Show();
				AssertEquals(false, userControl.FindSingle<ZDropEdit>("ArrivalPortScheduleKDropEdit").Visible);
				AssertEquals(true, userControl.FindSingle<ZTextBox>("ArrivalPortScheduleKTextBox").Visible);

				header.AMA_RL_NKPortOfFirstArrival = "USLAX";
				AssertEquals(true, userControl.FindSingle<ZDropEdit>("ArrivalPortScheduleKDropEdit").Visible);
				AssertEquals(false, userControl.FindSingle<ZTextBox>("ArrivalPortScheduleKTextBox").Visible);
			}
		}
	}
}
