using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core.Forms;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.TW.BriefCustomsDeclaration.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.GUI.Testing
{
	[TestedType(typeof(ApplicationGUIProvider))]
	sealed class ApplicationGUIProviderTest : ASYCUDA.GUI.Testing.ApplicationGUIProviderAbstractTest<ApplicationGUIProvider, AsycudaManifestHeader>
	{
		protected override Type ExpectedMenuBuilderType => typeof(MenuBuilder);

		protected override Type ExpectedBillLayoutType => typeof(TWBillLayouts);

		protected override Type ExpectedBillPartiesLayoutType => typeof(TWBillPartiesLayouts);

		public void TestMainTabPageGroupBoxNameCore()
		{
			var header = CreateNewManifest();

			using (var form = new BriefDeclarationForm(header))
			{
				var control = new AsycudaManifestUserControl();
				form.Controls.Add(control);
				form.Show();

				var manifestGroupBox = (ZGroupBox)control.Controls.Find("ManifestGroupBox", true)[0];
				AssertEquals(ResourceStringData.Empty, manifestGroupBox.CaptionResourceString);
			}
		}

		public void TestSetDeclarationDetailsGroupBoxVisibilityCore()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.FillWithValidTestData();

			using (var form = new BriefDeclarationForm(header))
			{
				var control = new AsycudaManifestUserControl();
				form.Controls.Add(control);
				form.Show();

				AssertEquals(0, control.Controls.Find("DeclarationDetailsGroupBox", true).Length);
			}
		}

		protected override int MaxColumnsOfManifestLayout => 3;

		protected override Form GetFormToBashCore()
		{
			var form = base.GetFormToBashCore();
			MissingResourceStringChecker.ExcludeFromTest(form.Controls.Find("ManifestGroupBox", true).Single());
			return form;
		}

		protected override AsycudaManifestHeader CreateNewManifest()
		{
			var header = base.CreateNewManifest();
			header.AMA_ApplicationCode = ManifestBase.ApplicationCodeTypeList.Codes.TWBriefCustomsDeclaration;
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Taiwan;
			header.AMA_ManifestType = TWManifestTypes.Codes.ImportLowValueDutyFreeGoods;
			return header;
		}

		public override bool AllowUntranslatableFormTitle() => true;

		public void TestAMA_NatureInfoValueChanged()
		{
			var header = CreateNewManifest();
			header.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Export22;
			using (var form = new BriefDeclarationForm(header))
			{
				var control = new AsycudaManifestUserControl();
				form.Controls.Add(control);
				form.Show();

				var expectedCaption = "FOB";
				var billsGrid = (ZGridWithDynamicColumnHandler)control.Controls.Find("BillsGrid", true)[0];
				AssertEquals(expectedCaption, billsGrid.GetColumnCaption(AsycudaBill.Schema.ABL_CustomsValue));
				AssertEquals(expectedCaption, billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_CustomsValue).GroupName.Caption);
				AssertEquals(expectedCaption, billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_RX_NKCustomsValueCurrency).GroupName.Caption);

				expectedCaption = "Customs Value";
				header.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Import23;
				AssertEquals(expectedCaption, billsGrid.GetColumnCaption(AsycudaBill.Schema.ABL_CustomsValue));
				AssertEquals(expectedCaption, billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_CustomsValue).GroupName.Caption);
				AssertEquals(expectedCaption, billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_RX_NKCustomsValueCurrency).GroupName.Caption);
			}
		}

		public void TestBillsGridColumnAvailability()
		{
			var manifest = CreateNewManifest();
			manifest.Bills.AddNew();

			using (var form = new ManifestForm(manifest))
			{
				form.Show();
				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>("asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>("mainTabControl");
				var billsAndPacksTabPage = mainTabControl.FindSingle<ZTabPage>("billsAndPacksTabPage");
				mainTabControl.SelectedTab = billsAndPacksTabPage;
				var billsGrid = billsAndPacksTabPage.FindSingle<ZGrid>("BillsGrid");

				CombineAssertions(() =>
				{
					Assert("ABL_SequenceNumber IsVisible", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_SequenceNumber).IsVisible);
					Assert("ABL_BillNumber IsVisible", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_BillNumber).IsVisible);
					Assert("ABL_CustomsValue IsVisible", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_CustomsValue).IsVisible);
					Assert("ABL_RX_NKCustomsValueCurrency IsVisible", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_RX_NKCustomsValueCurrency).IsVisible);
					Assert("ABL_ManifestQty IsVisible", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_ManifestQty).IsVisible);
					Assert("ABL_ManifestUQ IsVisible", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_ManifestUQ).IsVisible);
					Assert("ABL_GrossWeight IsVisible", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_GrossWeight).IsVisible);
					Assert("ABL_GrossWeightUQ IsVisible", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_GrossWeightUQ).IsVisible);
					Assert("ABL_RL_NKPortOfLoading IsVisible", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_RL_NKPortOfLoading).IsVisible);
					Assert("ABL_FreightValue IsVisible", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_FreightValue).IsVisible);
					Assert("ABL_RX_NKFreightValueCurrency IsVisible", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_RX_NKFreightValueCurrency).IsVisible);
					Assert("ABL_InsuranceValue IsVisible", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_InsuranceValue).IsVisible);
					Assert("ABL_RX_NKInsuranceValueCurrency IsVisible", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_RX_NKInsuranceValueCurrency).IsVisible);
					Assert("ABL_OtherValue IsVisible", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_OtherValue).IsVisible);
					Assert("ABL_RX_NKOtherValueCurrency  IsVisible", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_RX_NKOtherValueCurrency).IsVisible);
					Assert("ABL_OtherDeductions IsVisible", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_OtherDeductions).IsVisible);
					Assert("ABL_RX_NKOtherDeductionsCurrency IsVisible", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_RX_NKOtherDeductionsCurrency).IsVisible);

					Assert("ABL_BolType IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_BolType).IsUnavailable);
					Assert("ABL_RL_NKOrigin IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_RL_NKOrigin).IsUnavailable);
					Assert("ABL_CarrierReference IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_CarrierReference).IsUnavailable);
					Assert("ABL_GoodsDescription IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_GoodsDescription).IsUnavailable);
					Assert("ABL_TransportValue IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_TransportValue).IsUnavailable);
					Assert("ABL_RX_NKTransportValueCurrency IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_RX_NKTransportValueCurrency).IsUnavailable);
					Assert("ABL_PrepaidCollect IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_PrepaidCollect).IsUnavailable);
					Assert("ABL_CargoStatus IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_CargoStatus).IsUnavailable);
					Assert("ShipperOrgPK IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ShipperOrgPK).IsUnavailable);
					Assert("ABL_OA_Shipper IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_OA_Shipper).IsUnavailable);
					Assert("ABL_ShipperName IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_ShipperName).IsUnavailable);
					Assert("ABL_ShipperStreet1 IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_ShipperStreet1).IsUnavailable);
					Assert("ABL_ShipperStreet2 IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_ShipperStreet2).IsUnavailable);
					Assert("ABL_ShipperCity IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_ShipperCity).IsUnavailable);
					Assert("ABL_ShipperState IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_ShipperState).IsUnavailable);
					Assert("ABL_ShipperPostcode IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_ShipperPostcode).IsUnavailable);
					Assert("ABL_RN_NKShipperCountry IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_RN_NKShipperCountry).IsUnavailable);
					Assert("ConsigneeOrgPK IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ConsigneeOrgPK).IsUnavailable);
					Assert("ABL_OA_Consignee IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_OA_Consignee).IsUnavailable);
					Assert("ABL_ConsigneeName IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_ConsigneeName).IsUnavailable);
					Assert("ABL_ConsigneeStreet1 IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_ConsigneeStreet1).IsUnavailable);
					Assert("ABL_ConsigneeStreet2 IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_ConsigneeStreet2).IsUnavailable);
					Assert("ABL_ConsigneeCity IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_ConsigneeCity).IsUnavailable);
					Assert("ABL_ConsigneeState IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_ConsigneeState).IsUnavailable);
					Assert("ABL_ConsigneePostcode IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_ConsigneePostcode).IsUnavailable);
					Assert("ABL_RN_NKConsigneeCountry IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_RN_NKConsigneeCountry).IsUnavailable);
					Assert("ABL_ConsigneePhone IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_ConsigneePhone).IsUnavailable);
					Assert("NotifyPartyOrgPK IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.NotifyPartyOrgPK).IsUnavailable);
					Assert("ABL_OA_NotifyParty IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_OA_NotifyParty).IsUnavailable);
					Assert("ABL_NotifyPartyName IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_NotifyPartyName).IsUnavailable);
					Assert("ABL_NotifyPartyStreet1 IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_NotifyPartyStreet1).IsUnavailable);
					Assert("ABL_NotifyPartyStreet2 IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_NotifyPartyStreet2).IsUnavailable);
					Assert("ABL_NotifyPartyCity IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_NotifyPartyCity).IsUnavailable);
					Assert("ABL_NotifyPartyState IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_NotifyPartyState).IsUnavailable);
					Assert("ABL_NotifyPartyPostcode IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_NotifyPartyPostcode).IsUnavailable);
					Assert("ABL_RN_NKNotifyPartyCountry IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_RN_NKNotifyPartyCountry).IsUnavailable);
					Assert("ABL_NotifyPartyPhone IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_NotifyPartyPhone).IsUnavailable);
					Assert("DiscountValue IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.DiscountValue).IsUnavailable);
					Assert("DiscountValueCurrency IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.DiscountValueCurrency).IsUnavailable);
					Assert("OtherChargesValue IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.OtherChargesValue).IsUnavailable);
					Assert("OtherChargesValueCurrency IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.OtherChargesValueCurrency).IsUnavailable);
					Assert("CustomsJobNumber IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.CustomsJobNumber).IsUnavailable);
					Assert("ABL_RL_NKFinalDestination IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_RL_NKFinalDestination).IsUnavailable);
					Assert("ABL_UCRNumber IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_UCRNumber).IsUnavailable);
					Assert("ABL_Volume IsVisIsUnavailableible", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_Volume).IsUnavailable);
					Assert("ABL_VolumeUQ IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_VolumeUQ).IsUnavailable);
					Assert("ABL_MarksAndNumbers IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_MarksAndNumbers).IsUnavailable);
					Assert("ABL_Remarks IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_Remarks).IsUnavailable);
					Assert("ABL_RL_NKPortOfDischarge IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_RL_NKPortOfDischarge).IsUnavailable);
					Assert("ABL_LocationInformation IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_LocationInformation).IsUnavailable);
				});

				manifest.AMA_Nature = "EXP";
				CombineAssertions(() =>
				{
					Assert("ABL_RL_NKPortOfLoading IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_RL_NKPortOfLoading).IsUnavailable);
					Assert("ABL_RL_NKPortOfDischarge IsVisible", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_RL_NKPortOfDischarge).IsVisible);
					Assert("ABL_LocationInformation IsVisible", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_LocationInformation).IsVisible);

					Assert("ABL_FreightValue IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_FreightValue).IsUnavailable);
					Assert("ABL_RX_NKFreightValueCurrency IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_RX_NKFreightValueCurrency).IsUnavailable);
					Assert("ABL_InsuranceValue IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_InsuranceValue).IsUnavailable);
					Assert("ABL_RX_NKInsuranceValueCurrency IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_RX_NKInsuranceValueCurrency).IsUnavailable);
					Assert("ABL_OtherValue IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_OtherValue).IsUnavailable);
					Assert("ABL_RX_NKOtherValueCurrency  IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_RX_NKOtherValueCurrency).IsUnavailable);
					Assert("ABL_OtherDeductions IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_OtherDeductions).IsUnavailable);
					Assert("ABL_RX_NKOtherDeductionsCurrency IsUnavailable", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_RX_NKOtherDeductionsCurrency).IsUnavailable);
				});
			}
		}

		public void TestBillsGridExtraColumns()
		{
			var manifest = CreateNewManifest();
			manifest.Bills.AddNew();

			using (var form = new ManifestForm(manifest))
			{
				form.Show();
				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>("asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>("mainTabControl");
				var billsAndPacksTabPage = mainTabControl.FindSingle<ZTabPage>("billsAndPacksTabPage");
				mainTabControl.SelectedTab = billsAndPacksTabPage;
				var billsGrid = billsAndPacksTabPage.FindSingle<ZGrid>("BillsGrid");

				CombineAssertions(() =>
				{
					Assert("Remarks IsVisible", billsGrid.GetColumnStyle(AsycudaBill.Schema.Remarks).IsVisible);
					Assert("ABL_Procedure IsVisible", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_Procedure).IsVisible);
					Assert("ABL_Incoterm IsVisible", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_Incoterm).IsVisible);
					Assert("ABL_GoodsValue IsVisible", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_GoodsValue).IsVisible);
					Assert("ABL_RX_NKGoodsValueCurrency IsVisible", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_RX_NKGoodsValueCurrency).IsVisible);
					Assert("ABL_OtherValue IsVisible", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_OtherValue).IsVisible);
					Assert("ABL_RX_NKOtherValueCurrency IsVisible", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_RX_NKOtherValueCurrency).IsVisible);
					Assert("ABL_OtherDeductions IsVisible", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_OtherDeductions).IsVisible);
					Assert("ABL_RX_NKOtherDeductionsCurrency IsVisible", billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_RX_NKOtherDeductionsCurrency).IsVisible);
					Assert("ExchangeRate IsVisible", billsGrid.GetColumnStyle(AsycudaBill.Schema.ExchangeRate).IsVisible);
				});
			}
		}

		public void TestGetBillsGridColumnsOrder()
		{
			var header = CreateNewManifest();
			var provider = ApplicationGUIProvider.GetApplicationGuiProvider(header);
			var expected = new[]
			{
				AsycudaBill.Schema.ABL_SequenceNumber,
				AsycudaBill.Schema.ABL_BillNumber,
				AsycudaBill.Schema.Remarks,
				AsycudaBill.Schema.ABL_Procedure,
				AsycudaBill.Schema.ABL_Incoterm,
				AsycudaBill.Schema.ABL_ManifestQty,
				AsycudaBill.Schema.ABL_ManifestUQ,
				AsycudaBill.Schema.ABL_GrossWeight,
				AsycudaBill.Schema.ABL_GrossWeightUQ,
				AsycudaBill.Schema.ABL_CustomsValue,
				AsycudaBill.Schema.ABL_RX_NKCustomsValueCurrency,
				AsycudaBill.Schema.ABL_GoodsValue,
				AsycudaBill.Schema.ABL_RX_NKGoodsValueCurrency,
				AsycudaBill.Schema.ABL_RL_NKPortOfLoading,
				AsycudaBill.Schema.ABL_RL_NKPortOfDischarge,
				AsycudaBill.Schema.ABL_LocationInformation,
				AsycudaBill.Schema.ABL_FreightValue,
				AsycudaBill.Schema.ABL_RX_NKFreightValueCurrency,
				AsycudaBill.Schema.ABL_InsuranceValue,
				AsycudaBill.Schema.ABL_RX_NKInsuranceValueCurrency,
				AsycudaBill.Schema.ABL_OtherValue,
				AsycudaBill.Schema.ABL_RX_NKOtherValueCurrency,
				AsycudaBill.Schema.ABL_OtherDeductions,
				AsycudaBill.Schema.ABL_RX_NKOtherDeductionsCurrency,
				AsycudaBill.Schema.ExchangeRate
			};
			AssertContainsExactElementsInAnyOrder(expected, provider.GetBillsGridColumnsOrder());
		}

		public void TestCustomizeBillsGrid()
		{
			var header = CreateNewManifest();
			header.Bills.AddNew();
			using (var form = new BriefDeclarationForm(header))
			{
				form.Show();

				var asycudaManifestUserControl = form.FindSingle<AsycudaManifestUserControl>("asycudaManifestUserControl");
				var mainTabControl = asycudaManifestUserControl.FindSingle<ZTabControl>("mainTabControl");
				var billsAndPacksTabPage = mainTabControl.FindSingle<ZTabPage>("billsAndPacksTabPage");
				mainTabControl.SelectedTab = billsAndPacksTabPage;
				var billsGrid = billsAndPacksTabPage.FindSingle<ZGrid>("BillsGrid");

				var expectedCaption = "Total Invoice Amount";
				AssertEquals(expectedCaption, billsGrid.GetColumnCaption(AsycudaBill.Schema.ABL_GoodsValue));
				AssertEquals(expectedCaption, billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_GoodsValue).GroupName.Caption);
				AssertEquals(expectedCaption, billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_RX_NKGoodsValueCurrency).GroupName.Caption);

				expectedCaption = "Freight Value";
				AssertEquals(expectedCaption, billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_FreightValue).GroupName.Caption);
				AssertEquals(expectedCaption, billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_RX_NKFreightValueCurrency).GroupName.Caption);

				expectedCaption = "Transport Value";
				AssertEquals(expectedCaption, billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_TransportValue).GroupName.Caption);
				AssertEquals(expectedCaption, billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_RX_NKTransportValueCurrency).GroupName.Caption);
			}
		}

		protected override void AssertGetBillsGridColumnsWidth(IReadOnlyDictionary<string, int> columnsWidth)
		{
			AssertContainsExactElementsInExactOrder(
				new[]
				{
					AsycudaBill.Schema.ABL_ManifestQty,
				},
				columnsWidth.Keys);
			AssertContainsExactElementsInExactOrder(
				new[]
				{
					70,
				},
				columnsWidth.Values);
		}

		protected override void AssertGetBillsGridColumnVisiblilityOnValueChanged(IReadOnlyDictionary<string, bool> columnsVisiblility, ASYCUDA.Business.AsycudaManifestHeader header)
		{
			AssertContainsExactElementsInExactOrder(
				new[]
				{
					AsycudaBill.Schema.ABL_CustomsValue,
					AsycudaBill.Schema.ABL_RX_NKCustomsValueCurrency,
					AsycudaBill.Schema.ABL_RL_NKPortOfLoading,
					AsycudaBill.Schema.ABL_RL_NKPortOfDischarge,
					AsycudaBill.Schema.ABL_LocationInformation,
					AsycudaBill.Schema.ABL_FreightValue,
					AsycudaBill.Schema.ABL_RX_NKFreightValueCurrency,
					AsycudaBill.Schema.ABL_InsuranceValue,
					AsycudaBill.Schema.ABL_RX_NKInsuranceValueCurrency,
					AsycudaBill.Schema.ABL_OtherValue,
					AsycudaBill.Schema.ABL_RX_NKOtherValueCurrency,
					AsycudaBill.Schema.ABL_OtherDeductions,
					AsycudaBill.Schema.ABL_RX_NKOtherDeductionsCurrency
				},
				columnsVisiblility.Keys);
		}

		protected override void AssertGetBillsGridExtraColumnInfos(ZGridColumnInfo[] columnInfos)
		{
			AssertContainsExactElementsInExactOrder(new[] {
				"Remarks",
				"ABL_Procedure",
				"ABL_Incoterm",
				"ABL_GoodsValue",
				"ABL_RX_NKGoodsValueCurrency",
				"ABL_RL_NKPortOfLoading",
				"ABL_RL_NKPortOfDischarge",
				"ABL_LocationInformation",
				"ABL_OtherValue",
				"ABL_RX_NKOtherValueCurrency",
				"ABL_OtherDeductions",
				"ABL_RX_NKOtherDeductionsCurrency",
				"ExchangeRate",
			}, columnInfos.Select(s => s.ColumnName));
		}

		protected override Type[] ExpectedBillAdditionalTabPageUserControls => new[] {
			typeof(TWBillItemsUserControl),
			typeof(TWBillCalculationUserControl)
		};

		protected override string ExpectedMenuCaptionEnglishText => "Brokerage";
	}
}
