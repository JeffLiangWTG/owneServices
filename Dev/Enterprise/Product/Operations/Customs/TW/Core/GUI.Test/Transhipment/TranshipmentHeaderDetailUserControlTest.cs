using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.TW.GUI.Testing
{
	public sealed class TranshipmentHeaderDetailUserControlTest : TestCaseWithFactory
	{
		public void TestDynamicControlVisibility()
		{
			var customsOfficeSea = Factory.New<ZZRefCusCodeListCombined>();
			customsOfficeSea.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.Taiwan;
			customsOfficeSea.ZZD_CodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice;
			customsOfficeSea.ZZD_Code = "SE";
			customsOfficeSea.ZZD_StartDate = ZDateTime.Today;
			customsOfficeSea.ZZD_EndDate = ZDateTime.Today.AddYears(1);
			customsOfficeSea.ZZD_IsSea = true;
			var customsOfficeAir = Factory.New<ZZRefCusCodeListCombined>();
			customsOfficeAir.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.Taiwan;
			customsOfficeAir.ZZD_CodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice;
			customsOfficeAir.ZZD_Code = "AR";
			customsOfficeAir.ZZD_StartDate = ZDateTime.Today;
			customsOfficeAir.ZZD_EndDate = ZDateTime.Today.AddYears(1);
			customsOfficeAir.ZZD_IsAir = true;
			Factory.Save();
			var header = Factory.NewWithValidTestData<CusInBondHeader>();
			var importBill = header.ArrivalBill;
			var exportBill = header.MovementBill;
			var moveHeader = header.MovementHeader;
			var moveDetail = moveHeader.InBondMoveDetail;
			var moveLine = moveDetail.InBondMoveLineItem;
			using (var form = new TranshipmentForm(header))
			{
				form.Show();
				Application.DoEvents();
				var inBondHeaderDetailUserControl = form.TWInBondHeaderDetailUserControl;
				header.ReceiptOffice = "AR";
				header.UnladingOffice = "AR";
				AssertEquals("Import MasterBill Textbox Visibility", false, inBondHeaderDetailUserControl.Import_MasterBillForSeaBoundTextBox.Visible);
				AssertEquals("Import MasterBill Textbox Visibility", true, inBondHeaderDetailUserControl.Import_MasterBillForAirBoundTextBox.Visible);
				AssertEquals("Export MasterBill Textbox Visibility", false, inBondHeaderDetailUserControl.Export_MasterBillForSeaBoundTextBox.Visible);
				AssertEquals("Export MasterBill Textbox Visibility", true, inBondHeaderDetailUserControl.Export_MasterBillForAirBoundTextBox.Visible);
				header.ReceiptOffice = "SE";
				header.UnladingOffice = "SE";
				AssertEquals("Import MasterBill Textbox Visibility", true, inBondHeaderDetailUserControl.Import_MasterBillForSeaBoundTextBox.Visible);
				AssertEquals("Import MasterBill Textbox Visibility", false, inBondHeaderDetailUserControl.Import_MasterBillForAirBoundTextBox.Visible);
				AssertEquals("Export MasterBill Textbox Visibility", true, inBondHeaderDetailUserControl.Export_MasterBillForSeaBoundTextBox.Visible);
				AssertEquals("Export MasterBill Textbox Visibility", false, inBondHeaderDetailUserControl.Export_MasterBillForAirBoundTextBox.Visible);
			}
		}

		public void TestControlAlwaysVisible()
		{
			var inBondHeader = Factory.NewWithValidTestData<CusInBondHeader>();
			using (var form = new TranshipmentForm(inBondHeader))
			{
				form.Show();
				var inBondHeaderDetailUserControl = form.TWInBondHeaderDetailUserControl;
				Assert(inBondHeaderDetailUserControl.BM_ForeignDestPortKCodeCodeFindBox.Visible);
				Assert(inBondHeaderDetailUserControl.BM_RL_NKForeignDestPortCodeFindBox.Visible);
			}
		}

		public void TestBI_PackagingDescriptionLongTextControlVisible()
		{
			var header = Factory.NewWithValidTestData<CusInBondHeader>();
			var moveHeader = header.MovementHeader;
			var moveDetail = moveHeader.InBondMoveDetail;
			var moveLine = moveDetail.InBondMoveLineItem;
			moveLine.TW_IsCoPackaged = false;
			using (var form = new TranshipmentForm(header))
			{
				form.Show();
				var inBondHeaderDetailUserControl = form.TWInBondHeaderDetailUserControl;
				moveLine.TW_IsCoPackaged = true;
				Assert(inBondHeaderDetailUserControl.BI_PackagingDescriptionLongTextControl.Visible);
				moveLine.TW_IsCoPackaged = false;
				Assert(inBondHeaderDetailUserControl.BI_PackagingDescriptionLongTextControl.Visible);
			}
		}

		public void TestVisiblContainersGridControls()
		{
			var header = Factory.NewWithValidTestData<CusInBondHeader>();
			var moveHeader = header.MovementHeader;
			var moveDetail = moveHeader.InBondMoveDetail;
			var moveLine = moveDetail.InBondMoveLineItem;
			moveLine.TW_IsCoPackaged = false;
			using (var form = new TranshipmentForm(header))
			{
				form.Show();
				var containersGrid = form.TWInBondHeaderDetailUserControl.ContainersGrid;
				AssertNull(containersGrid.GetColumnStyle("BC_Seal1"));
			}
		}

		public void TestAllocateEntryNumberButtonEnable()
		{
			var header = Factory.NewWithValidTestData<CusInBondHeader>();
			header.BH_ReleaseStatus = ZString.Empty;
			using (var form = new TranshipmentForm(header))
			{
				form.Show();
				CombineAssertions(() =>
				{
					AssertEquals("enable when empty", true, form.TWInBondHeaderDetailUserControl.AllocateEntryNumberButton.Enabled);
					header.BH_ReleaseStatus = ClearanceStatusCodeList.Codes.C1;
					AssertEquals("disable when C1", false, form.TWInBondHeaderDetailUserControl.AllocateEntryNumberButton.Enabled);
					header.BH_ReleaseStatus = ClearanceStatusCodeList.Codes.C2;
					AssertEquals("disable when C2", false, form.TWInBondHeaderDetailUserControl.AllocateEntryNumberButton.Enabled);
					header.BH_ReleaseStatus = ClearanceStatusCodeList.Codes.C3M;
					AssertEquals("disable when C3M", false, form.TWInBondHeaderDetailUserControl.AllocateEntryNumberButton.Enabled);
					header.BH_ReleaseStatus = ClearanceStatusCodeList.Codes.C3X;
					AssertEquals("disable when C3X", false, form.TWInBondHeaderDetailUserControl.AllocateEntryNumberButton.Enabled);
					header.BH_ReleaseStatus = "XXX";
					AssertEquals("enable when XXX", true, form.TWInBondHeaderDetailUserControl.AllocateEntryNumberButton.Enabled);
				});
			}
		}
	}
}
