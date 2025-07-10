using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.TW.BriefCustomsDeclaration.Business;
using Enterprise.Customs.Universal.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.GUI.Testing
{
	[TestedType(typeof(TWBillItemsUserControl))]
	sealed class TWBillItemsUserControlTest : TestCaseWithFactory
	{
		public void TestUserControlSize()
		{
			using (var control = new TWBillItemsUserControl())
			{
				var size = control.Size;
				CombineAssertions(() =>
				{
					AssertEquals(400, size.Width);
					AssertEquals(209, size.Height);
				});
			}
		}

		[TestDate(2024, 7, 9)]
		public void TestUniversalTariffColumn()
		{
			using (var control = new TWBillItemsUserControl())
			{
				var billItemsGrid = control.FindSingle<ZGrid>(c => c.Name == "BillItemsGrid");
				var columnStyleInfo = (TariffColumnStyleInfo)billItemsGrid.GetColumnStyle("API_FormattedTariff");
				CombineAssertions(() =>
				{
					AssertEquals("GetCountryCode", Core.Constants.CountryCodes.Taiwan, columnStyleInfo.GetCountryCode());
					AssertEquals("GetDataGrouping", Core.Constants.CountryCodes.Taiwan, columnStyleInfo.GetDataGrouping());
					AssertEquals("GetTariffType", Universal.Constants.TariffTypes.HarmonizedSystem, columnStyleInfo.GetTariffType());
					AssertEquals("GetEffectiveDate", new ZDateTime(2024, 7, 9), columnStyleInfo.GetEffectiveDate());
					AssertEquals("Width", 100, columnStyleInfo.Width);
				});
			}
		}

		public void TestTaxesGroupBoxVisible()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_ApplicationCode = ManifestBase.ApplicationCodeTypeList.Codes.TWBriefCustomsDeclaration;
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Taiwan;
			header.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Import23;
			var bill = header.Bills.AddNew();

			using (var form = new ManifestForm(header))
			{
				form.Show();
				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>(c => c.Name == "asycudaManifestUserControl");

				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>(c => c.Name == "mainTabControl");
				var billsAndPacksTabPage = mainTabControl.FindSingle<ZTabPage>(c => c.Name == "billsAndPacksTabPage");
				mainTabControl.SelectedTab = billsAndPacksTabPage;
				var billsAndPacksTabControl = mainTabControl.FindSingle<ZTabControl>(c => c.Name == "billsAndPacksTabControl");
				var additionalTabPage = billsAndPacksTabControl.FindSingle<ZTabPage>(c => c.Name == "billsAndPacksTabControl_TabPage_TWBillItemsUserControl");
				billsAndPacksTabControl.SelectedTab = additionalTabPage;
				var asycudaContainerBillLinkUserControl = additionalTabPage.FindSingle<TWBillItemsUserControl>(c => c.Name == "TWBillItemsUserControl");
				var taxesGroupBox = asycudaContainerBillLinkUserControl.FindSingle<ZGroupBox>(c => c.Name == "TaxesGroupBox");
				AssertEquals("TaxesGroupBox Visible When IMP", true, taxesGroupBox.Visible);

				header.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Export22;
				AssertEquals("TaxesGroupBox Hidden When IMP", false, taxesGroupBox.Visible);
			}
		}

		public void TestBillItemsGridColumns()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_ApplicationCode = ManifestBase.ApplicationCodeTypeList.Codes.TWBriefCustomsDeclaration;
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Taiwan;
			header.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Import23;
			var bill = header.Bills.AddNew();

			using (var form = new ManifestForm(header))
			{
				form.Show();
				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>(c => c.Name == "asycudaManifestUserControl");

				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>(c => c.Name == "mainTabControl");
				var billsAndPacksTabPage = mainTabControl.FindSingle<ZTabPage>(c => c.Name == "billsAndPacksTabPage");
				mainTabControl.SelectedTab = billsAndPacksTabPage;
				var billsAndPacksTabControl = mainTabControl.FindSingle<ZTabControl>(c => c.Name == "billsAndPacksTabControl");
				var additionalTabPage = billsAndPacksTabControl.FindSingle<ZTabPage>(c => c.Name == "billsAndPacksTabControl_TabPage_TWBillItemsUserControl");
				billsAndPacksTabControl.SelectedTab = additionalTabPage;
				AssertEquals("Items", additionalTabPage.CaptionResourceString.Caption);

				var asycudaContainerBillLinkUserControl = additionalTabPage.FindSingle<TWBillItemsUserControl>(c => c.Name == "TWBillItemsUserControl");
				var billItemsGrid = asycudaContainerBillLinkUserControl.FindSingle<ZGrid>(c => c.Name == "BillItemsGrid");
				billItemsGrid.ResetColumns();
				var actualList = billItemsGrid.Columns.Cast<ZGridColumn>().Select(x => x.ColumnName);
				var expectedList = GetImportExpectedPacksGridColumnNames();
				AssertContainsExactElementsInExactOrder("Import", expectedList, actualList);

				header.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Export22;
				header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
				billItemsGrid.ResetColumns();
				actualList = billItemsGrid.Columns.Cast<ZGridColumn>().Select(x => x.ColumnName);
				expectedList = GetExportSeaExpectedPacksGridColumnNames();
				AssertContainsExactElementsInExactOrder("Export Sea", expectedList, actualList);

				header.AMA_TransportMode = Core.Constants.TransportModes.Air;
				billItemsGrid.ResetColumns();
				actualList = billItemsGrid.Columns.Cast<ZGridColumn>().Select(x => x.ColumnName);
				expectedList = GetExportAirExpectedPacksGridColumnNames();
				AssertContainsExactElementsInExactOrder("Export Air", expectedList, actualList);
			}
		}

		public void TestColumnCaptions()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_ApplicationCode = ManifestBase.ApplicationCodeTypeList.Codes.TWBriefCustomsDeclaration;
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Taiwan;
			header.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Import23;
			var bill = header.Bills.AddNew();

			using (var form = new ManifestForm(header))
			{
				form.Show();
				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>(c => c.Name == "asycudaManifestUserControl");

				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>(c => c.Name == "mainTabControl");
				var billsAndPacksTabPage = mainTabControl.FindSingle<ZTabPage>(c => c.Name == "billsAndPacksTabPage");
				mainTabControl.SelectedTab = billsAndPacksTabPage;
				var billsAndPacksTabControl = mainTabControl.FindSingle<ZTabControl>(c => c.Name == "billsAndPacksTabControl");
				var additionalTabPage = billsAndPacksTabControl.FindSingle<ZTabPage>(c => c.Name == "billsAndPacksTabControl_TabPage_TWBillItemsUserControl");
				billsAndPacksTabControl.SelectedTab = additionalTabPage;
				AssertEquals("Items", additionalTabPage.CaptionResourceString.Caption);

				var asycudaContainerBillLinkUserControl = additionalTabPage.FindSingle<TWBillItemsUserControl>(c => c.Name == "TWBillItemsUserControl");
				var billItemsGrid = asycudaContainerBillLinkUserControl.FindSingle<ZGrid>(c => c.Name == "BillItemsGrid");
				CombineAssertions(() =>
				{
					AssertEquals("Customs Value", billItemsGrid.GetColumnCaption("API_CustomsValue"));
					AssertEquals("Duty Treatment", billItemsGrid.GetColumnCaption("ModeOfStatisticsOrDutyTreatment"));

					header.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Export22;
					AssertEquals("FOB Value", billItemsGrid.GetColumnCaption("API_CustomsValue"));
					AssertEquals("Mode of Statistics", billItemsGrid.GetColumnCaption("ModeOfStatisticsOrDutyTreatment"));

					header.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Transit24;
					AssertEquals("Procedure", billItemsGrid.GetColumnCaption("ModeOfStatisticsOrDutyTreatment"));
				});
			}
		}

		public void TestSequenceColumnStyleInfo()
		{
			using (var control = new TWBillItemsUserControl())
			{
				var billItemsGrid = control.FindSingle<ZGrid>(c => c.Name == "BillItemsGrid");
				var columnStyleInfo = billItemsGrid.GetColumnStyle(nameof(AsycudaPackedItem.API_LineNo)) as ZCalcEditColumnStyleInfo;
				AssertEquals(9999m, columnStyleInfo.MaxValue);
			}
		}

		string[] GetImportExpectedPacksGridColumnNames()
		{
			return new string[]
			{
				nameof(AsycudaPackedItem.API_LineNo),
				nameof(AsycudaPackedItem.API_CustomsQty),
				nameof(AsycudaPackedItem.API_CustomsUQ),
				nameof(AsycudaPackedItem.API_UnitPrice),
				nameof(AsycudaPackedItem.API_GoodsValue),
				nameof(AsycudaPackedItem.API_RX_NKGoodsValueCurrency),
				nameof(AsycudaPackedItem.API_CustomsValue),
				nameof(AsycudaPackedItem.API_NetWeight),
				nameof(AsycudaPackedItem.API_NetWeightUQ),
				nameof(AsycudaPackedItem.API_GoodsDescription),
				nameof(AsycudaPackedItem.API_FormattedTariff),
				nameof(AsycudaPackedItem.API_CustomsQty2),
				nameof(AsycudaPackedItem.API_CustomsUQ2),
				nameof(AsycudaPackedItem.ModeOfStatisticsOrDutyTreatment),
				nameof(AsycudaPackedItem.API_RN_NKGoodsOrigin),
				nameof(AsycudaPackedItem.API_Preference),
				nameof(AsycudaPackedItem.FormattedAdValoremDutyRate),
				nameof(AsycudaPackedItem.FormattedSpecificDutyRate),
				nameof(AsycudaPackedItem.API_Brand),
				nameof(AsycudaPackedItem.API_Model),
				nameof(AsycudaPackedItem.API_Remarks),
			};
		}

		string[] GetExportAirExpectedPacksGridColumnNames()
		{
			return new string[]
			{
					nameof(AsycudaPackedItem.API_LineNo),
					nameof(AsycudaPackedItem.API_CustomsQty),
					nameof(AsycudaPackedItem.API_CustomsUQ),
					nameof(AsycudaPackedItem.API_UnitPrice),
					nameof(AsycudaPackedItem.API_GoodsValue),
					nameof(AsycudaPackedItem.API_RX_NKGoodsValueCurrency),
					nameof(AsycudaPackedItem.API_CustomsValue),
					nameof(AsycudaPackedItem.API_NetWeight),
					nameof(AsycudaPackedItem.API_NetWeightUQ),
					nameof(AsycudaPackedItem.API_GoodsDescription),
					nameof(AsycudaPackedItem.API_FormattedTariff),
					nameof(AsycudaPackedItem.API_CustomsQty2),
					nameof(AsycudaPackedItem.API_CustomsUQ2),
					nameof(AsycudaPackedItem.ModeOfStatisticsOrDutyTreatment),
					nameof(AsycudaPackedItem.API_RN_NKGoodsOrigin),
					nameof(AsycudaPackedItem.API_Brand),
					nameof(AsycudaPackedItem.API_Model),
					nameof(AsycudaPackedItem.API_Remarks),
			};
		}

		string[] GetExportSeaExpectedPacksGridColumnNames()
		{
			return new string[]
			{
					nameof(AsycudaPackedItem.API_LineNo),
					nameof(AsycudaPackedItem.API_CustomsQty),
					nameof(AsycudaPackedItem.API_CustomsUQ),
					nameof(AsycudaPackedItem.API_UnitPrice),
					nameof(AsycudaPackedItem.API_GoodsValue),
					nameof(AsycudaPackedItem.API_RX_NKGoodsValueCurrency),
					nameof(AsycudaPackedItem.API_CustomsValue),
					nameof(AsycudaPackedItem.API_NetWeight),
					nameof(AsycudaPackedItem.API_NetWeightUQ),
					nameof(AsycudaPackedItem.API_GoodsDescription),
					nameof(AsycudaPackedItem.API_FormattedTariff),
					nameof(AsycudaPackedItem.API_CustomsQty2),
					nameof(AsycudaPackedItem.API_CustomsUQ2),
					nameof(AsycudaPackedItem.ModeOfStatisticsOrDutyTreatment),
					nameof(AsycudaPackedItem.API_RN_NKGoodsOrigin),
					nameof(AsycudaPackedItem.API_CustomsBuyerPartNo),
					nameof(AsycudaPackedItem.API_CustomsSupplierPartNo),
					nameof(AsycudaPackedItem.API_PreviousEntryNo),
					nameof(AsycudaPackedItem.API_PreviousEntryLineNo),
					nameof(AsycudaPackedItem.API_Brand),
					nameof(AsycudaPackedItem.API_Model),
					nameof(AsycudaPackedItem.API_Remarks),
			};
		}
	}
}
