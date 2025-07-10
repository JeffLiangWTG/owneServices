using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.InBond.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.InBond.GUI.Testing
{
	sealed class USAirInBondBillsAndMovementsDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestBillDispositions()
		{
			var importer = Factory.New<MasterFiles.Business.OrgHeader>();
			importer.CompanyData.OB_IMUsedBondedWhs = true;
			var helper = new WhsDataTestHelper(Factory);
			helper.GetNewWhsWarehouse(importer.MainAddress.PK, true, "HO2");
			var header = Factory.New<CusInBondHeader>();
			header.BH_OA_Importer = importer.MainAddress.PK;
			header.BH_FTZMove = true;
			var bill1 = header.Bills.AddNew();
			bill1.B0_MasterBillNumber = "XJ5-32423";
			var moveHeader = header.MovementHeaders.AddNew();
			moveHeader.BM_OA_WarehouseAddress = helper.Warehouse.MainAddress.PK;
			var moveDetail1 = moveHeader.MovementDetails.AddNew(bill1.PK);
			moveDetail1.DispositionCodes.AddNewIfNotExist("93", ZDateTime.Today, US.Business.BillDispositionSourceList.Codes.SO);
			using (var form = new USInBondForm(header))
			{
				form.Show();
				var tabControl = ((IFormPlugInsProvider)form).TopLevelTabControl;
				tabControl.SelectedTab = form.MovementDetailsTabPage;
				var foundControls = form.MovementDetailsTabPage.Controls.Find("BillsDispositionsGrid", true);
				var billDispositionsGrid = foundControls.Length > 0 ? (ZGrid)foundControls[0] : null;
				AssertNotNull("BillsDispositionsGrid", billDispositionsGrid);
				billDispositionsGrid.Focus();
				billDispositionsGrid.ListManager.Position = 0;
				AssertEquals(moveDetail1.LatestDisposition, billDispositionsGrid.ListManager.GetCurrent());
			}
		}
	}
}
