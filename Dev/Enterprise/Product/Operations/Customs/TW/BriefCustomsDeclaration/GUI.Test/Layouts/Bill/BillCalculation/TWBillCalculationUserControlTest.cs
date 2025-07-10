using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.GUI;
using Enterprise.Customs.TW.BriefCustomsDeclaration.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.GUI.Testing
{
	[TestedType(typeof(TWBillCalculationUserControl))]
	sealed class TWBillCalculationUserControlTest : TestCaseWithFactory
	{
		public void TestCaption()
		{
			_ = Tax;
			using (var form = new ManifestForm(header))
			{
				var calculationTabPage = GetTabPage(form);
				AssertEquals("Calculation", calculationTabPage.CaptionResourceString.Caption);
			}
		}

		public void TestGoodsValueConvertToLocalCurrencyControl_UnitFindBox_ReadOnly()
		{
			using (var form = new ManifestForm(header))
			{
				form.Show();
				var calculationTabPage = GetTabPage(form);
				var control = calculationTabPage.FindSingle<TWBillCalculationUserControl>(c => c.Name == "TWBillCalculationUserControl");
				AssertEquals("UnitFindBox should be readonly.", true, control.GoodsValueConvertToLocalCurrencyControl.Controls["UnitFindBox"].GetReadOnly());
			}
		}

		public void TestItemsGridColumns()
		{
			_ = Tax;
			using (var form = new ManifestForm(header))
			{
				var calculationTabPage = GetTabPage(form);

				var asycudaContainerBillLinkUserControl = calculationTabPage.FindSingle<TWBillCalculationUserControl>(c => c.Name == "TWBillCalculationUserControl");
				var itemsGrid = asycudaContainerBillLinkUserControl.FindSingle<ZGrid>(c => c.Name == "zGridTaxs");
				var actualList = itemsGrid.Columns.Cast<ZGridColumn>().Select(x => x.ColumnName);
				var expectedList = GetExpectedPacksGridColumnNames();
				AssertContainsExactElementsInExactOrder(expectedList, actualList);
			}
		}

		public void TestSupportMultipleResourceStringData()
		{
			var bill = header.Bills.AddNew();
			using (var form = new ManifestForm(header))
			{
				var calculationTabPage = GetTabPage(form);
				var control = calculationTabPage.FindSingle<TWBillCalculationUserControl>(c => c.Name == "TWBillCalculationUserControl");
				control.SetDataBinding(bill, "");
				AssertSame(bill, ((ISupportMultipleResourceStringDataSupporter)control).SupportMultipleResourceStringData);
			}
		}

		public void TestCustomsValueConvertToLocalCurrencyControl_Caption()
		{
			header.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Import23;
			var bill = header.Bills.AddNew();
			using (var form = new ManifestForm(header))
			{
				form.Show();
				var calculationTabPage = GetTabPage(form);
				var control = calculationTabPage.FindSingle<TWBillCalculationUserControl>(c => c.Name == "TWBillCalculationUserControl");
				var convertToLocalCurrencyControl = control.FindSingle<ConvertToLocalCurrencyControl>(c => c.Name == "CustomsValueConvertToLocalCurrencyControl");
				AssertEquals("IMP", "Customs Value", convertToLocalCurrencyControl.GetExtension<ILabelCaptionRenderer>().Caption);

				header.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Export22;
				AssertEquals("EXP", "FOB", convertToLocalCurrencyControl.GetExtension<ILabelCaptionRenderer>().Caption);
			}
		}

		public void TestDutiesTaxesAndFeesGroupBoxVisible()
		{
			_ = Tax;
			using (var form = new ManifestForm(header))
			{
				var calculationTabPage = GetTabPage(form);
				var asycudaContainerBillLinkUserControl = calculationTabPage.FindSingle<TWBillCalculationUserControl>(c => c.Name == "TWBillCalculationUserControl");
				var dutiesTaxesAndFeesGroupBox = asycudaContainerBillLinkUserControl.FindSingle<ZGroupBox>(c => c.Name == "DutiesTaxesAndFeesGroupBox");
				AssertEquals(true, dutiesTaxesAndFeesGroupBox.Visible);
				header.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Export22;
				AssertEquals(false, dutiesTaxesAndFeesGroupBox.Visible);
			}
		}

		string[] GetExpectedPacksGridColumnNames()
		{
			return [
				nameof(AsycudaTax.AET_ChargeType),
				nameof(AsycudaTax.ChargeTypeDescription),
				nameof(AsycudaTax.AET_ChargeAmount),
				nameof(AsycudaTax.AET_MethodOfPayment),
			];
		}

		AsycudaManifestHeader header;

		AsycudaTax tax;
		AsycudaTax Tax => tax ?? (tax = header.Bills.AddNew().AsycudaTaxes.AddNew());

		protected override void SetUp()
		{
			base.SetUp();
			if (header == null)
			{
				header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
				header.AMA_ApplicationCode = ManifestBase.ApplicationCodeTypeList.Codes.TWBriefCustomsDeclaration;
				header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Taiwan;
				header.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Import23;
			}
		}

		ZTabPage GetTabPage(ManifestForm form)
		{
			form.Show();
			var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>(c => c.Name == "asycudaManifestUserControl");

			var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>(c => c.Name == "mainTabControl");
			var billsAndPacksTabPage = mainTabControl.FindSingle<ZTabPage>(c => c.Name == "billsAndPacksTabPage");
			mainTabControl.SelectedTab = billsAndPacksTabPage;
			var billsAndPacksTabControl = mainTabControl.FindSingle<ZTabControl>(c => c.Name == "billsAndPacksTabControl");
			var calculationTabPage = billsAndPacksTabControl.FindSingle<ZTabPage>(c => c.Name == "billsAndPacksTabControl_TabPage_TWBillCalculationUserControl");
			billsAndPacksTabControl.SelectedTab = calculationTabPage;

			return calculationTabPage;
		}
	}
}
