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
	public class UnladingPortUserControlTest : TestCaseWithFactory
	{
		public void TestControls()
		{
			using (var userControl = new UnladingPortUserControl())
			{
				AssertNotNull(userControl.Controls.Find("UnladingPortScheduleKTextBox", true));
				AssertNotNull(userControl.Controls.Find("UnladingPortCodeFindBox", true));
			}
		}

		public void TestUNLOCOPortsComponentVisibility()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("PORT", "Port");
			helper.CreateNewOrGetExistingCusCodeList("US","PORT", "60001", "60001 Port", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList("US", "PORT", "60002", "60002 Port", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			Factory.Save();
			RefUNLOCOTestDataHelper.CreateLocoMapIfNotExists(Factory, "60001", "USLAX", USLocoMapSystemUsageList.Codes.SCK);
			RefUNLOCOTestDataHelper.CreateLocoMapIfNotExists(Factory, "60002", "USLAX", USLocoMapSystemUsageList.Codes.SCK);
			var header = Factory.New<USExportAsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			using (var userControl = new UnladingPortUserControl())
			{
				userControl.SetDataBinding(bill, "");
				userControl.Show();
				AssertEquals(false, userControl.FindSingle<ZDropEdit>("UnladingPortScheduleKDropEdit").Visible);
				AssertEquals(true, userControl.FindSingle<ZTextBox>("UnladingPortScheduleKTextBox").Visible);

				bill.ABL_RL_NKPortOfDischarge = "USLAX";
				AssertEquals(true, userControl.FindSingle<ZDropEdit>("UnladingPortScheduleKDropEdit").Visible);
				AssertEquals(false, userControl.FindSingle<ZTextBox>("UnladingPortScheduleKTextBox").Visible);
			}
		}
	}
}
