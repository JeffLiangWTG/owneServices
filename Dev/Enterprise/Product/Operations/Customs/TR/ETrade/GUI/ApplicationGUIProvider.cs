using System;
using System.Collections.Generic;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core.Forms;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.TR.Business;
using Enterprise.Customs.TR.GUI;
using Enterprise.ZArchitecture.GUI;
using AsycudaBill = Enterprise.Customs.TR.ETrade.Business.AsycudaBill;

namespace Enterprise.Customs.TR.ETrade.GUI
{
	public class ApplicationGUIProvider : ASYCUDA.GUI.ApplicationGUIProvider
	{
		public override Type ApplicationBusinessProviderType => typeof(Business.ApplicationBusinessProvider);

		public override ASYCUDA.GUI.MenuBuilder GetMenuBuilder(ZForm mainForm, ASYCUDA.Business.AsycudaManifestHeader header)
		{
			return new MenuBuilder(header, mainForm);
		}

		protected override IEnumerable<IAdditionalTabPage> GetBillAdditionalTabPageUserControlCore()
		{
			yield return new AsycudaPackUserControl();
			yield return new TRSupportingDocumetsUserControl();
		}

		protected override IPanelLayoutProvider GetManifestLayoutCore() => new ETradeLayouts();

		protected override string[] GetTaxesGridColumnsOrderCore()
		{
			return new string[]
			{
				AutoAsycudaTax.Schema.AET_ChargeType,
				TR.ETrade.Business.AsycudaTax.Schema.AET_TypeDescription,
				AutoAsycudaTax.Schema.AET_RateOverrideReasonCode,
				AutoAsycudaTax.Schema.AET_BaseValue,
				AutoAsycudaTax.Schema.AET_Rate,
				AutoAsycudaTax.Schema.AET_ChargeAmount,
				AutoAsycudaTax.Schema.AET_MethodOfPayment,
			};
		}

		protected override ZBool ShowTaxesUserControlCore(ASYCUDA.Business.AsycudaManifestHeader header) => ZBool.True;

		protected override IPanelLayoutProvider GetBillLayoutCore()
		{
			return new ETradeBillDetailsLayouts();
		}

		protected override IPanelLayoutProvider GetPackedItemDetailsLayoutCore()
		{
			return new ETradePackedItemDetailsLayouts();
		}

		protected override ResourceStringData MainTabPageGroupBoxNameCore => Res.GetData("DA6824D4-7C5D-4CC2-B666-693DCFBB381C", "E-Trade");

		protected override IEnumerable<ZGridColumnInfo> GetBillsGridExtraColumnInfosCore()
		{
			var domesticExpenditureGroup = Enterprise.Customs.TR.ETrade.GUI.Res.GetData("609B1119-35DA-4FE7-8873-5EB9D31A69E2", "Domestic Expenditure");

			var shipmentTypeDropEditColumnStyleInfo = new ZDropEditColumnStyleInfo();
			shipmentTypeDropEditColumnStyleInfo.ColumnName = "Header.AMA_Nature";
			shipmentTypeDropEditColumnStyleInfo.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			shipmentTypeDropEditColumnStyleInfo.IsReadOnly = true;
			shipmentTypeDropEditColumnStyleInfo.CaptionResourceString = Res.GetData("BD3B8A50-C345-4E91-80E4-48AD62612206", "Shipment Type");
			shipmentTypeDropEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			yield return shipmentTypeDropEditColumnStyleInfo;

			var natureOfBusinessDropEditColumnStyleInfo = new ZDropEditColumnStyleInfo();
			natureOfBusinessDropEditColumnStyleInfo.ColumnName = "NatureOfBusiness";
			natureOfBusinessDropEditColumnStyleInfo.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			natureOfBusinessDropEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			yield return natureOfBusinessDropEditColumnStyleInfo;

			var exemptionCode1DropEditColumnStyleInfo = new ZDropEditColumnStyleInfo();
			exemptionCode1DropEditColumnStyleInfo.ColumnName = "ExemptionCode1";
			exemptionCode1DropEditColumnStyleInfo.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			exemptionCode1DropEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			yield return exemptionCode1DropEditColumnStyleInfo;

			var exemptionCode2DropEditColumnStyleInfo = new ZDropEditColumnStyleInfo();
			exemptionCode2DropEditColumnStyleInfo.ColumnName = "ExemptionCode2";
			exemptionCode2DropEditColumnStyleInfo.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			exemptionCode2DropEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			yield return exemptionCode2DropEditColumnStyleInfo;

			var departureCountryCodeFindBoxColumnStyleInfo = new ZCodeFindBoxColumnStyleInfo();
			departureCountryCodeFindBoxColumnStyleInfo.ColumnName = "DepartureCountry";
			departureCountryCodeFindBoxColumnStyleInfo.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			departureCountryCodeFindBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			yield return departureCountryCodeFindBoxColumnStyleInfo;

			var tradeCountryCodeFindBoxColumnStyleInfo = new ZCodeFindBoxColumnStyleInfo();
			tradeCountryCodeFindBoxColumnStyleInfo.ColumnName = "TradeCountry";
			tradeCountryCodeFindBoxColumnStyleInfo.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			tradeCountryCodeFindBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			yield return tradeCountryCodeFindBoxColumnStyleInfo;

			var exportCountryCodeFindBoxColumnStyleInfo = new ZCodeFindBoxColumnStyleInfo();
			exportCountryCodeFindBoxColumnStyleInfo.ColumnName = "ExportCountry";
			exportCountryCodeFindBoxColumnStyleInfo.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			exportCountryCodeFindBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			yield return exportCountryCodeFindBoxColumnStyleInfo;

			var arrivalCountryCodeFindBoxColumnStyleInfo = new ZCodeFindBoxColumnStyleInfo();
			arrivalCountryCodeFindBoxColumnStyleInfo.ColumnName = "ArrivalCountry";
			arrivalCountryCodeFindBoxColumnStyleInfo.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			arrivalCountryCodeFindBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			yield return arrivalCountryCodeFindBoxColumnStyleInfo;

			var paymentMethodDropEditColumnStyleInfo = new ZDropEditColumnStyleInfo();
			paymentMethodDropEditColumnStyleInfo.ColumnName = "PaymentMethod";
			paymentMethodDropEditColumnStyleInfo.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			paymentMethodDropEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			yield return paymentMethodDropEditColumnStyleInfo;

			var aBL_IncotermDropEditColumnStyleInfo = new ZDropEditColumnStyleInfo();
			aBL_IncotermDropEditColumnStyleInfo.ColumnName = "ABL_Incoterm";
			aBL_IncotermDropEditColumnStyleInfo.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			aBL_IncotermDropEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			yield return aBL_IncotermDropEditColumnStyleInfo;

			var accountantTextBoxColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			accountantTextBoxColumnStyleInfo.ColumnName = "AccountantName";
			accountantTextBoxColumnStyleInfo.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			accountantTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			yield return accountantTextBoxColumnStyleInfo;

			var accountantVATTextBoxColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			accountantVATTextBoxColumnStyleInfo.ColumnName = "AccountantVAT";
			accountantVATTextBoxColumnStyleInfo.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			accountantVATTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			yield return accountantVATTextBoxColumnStyleInfo;

			var netWeightCalcEditColumnStyleInfo = new ZArchitecture.ZCalcEditColumnStyleInfo();
			netWeightCalcEditColumnStyleInfo.BindToDecimalPlaces = null;
			netWeightCalcEditColumnStyleInfo.CaptionResourceString = Enterprise.Customs.TR.ETrade.GUI.Res.GetData("43E7FAD9-3D3C-4CBB-A0AC-D53BA08BD434", "Net Weight");
			netWeightCalcEditColumnStyleInfo.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			netWeightCalcEditColumnStyleInfo.ColumnName = "ABL_NetWeight";
			netWeightCalcEditColumnStyleInfo.GroupName = Enterprise.Customs.TR.ETrade.GUI.Res.GetData("FF1DE30F-75BA-4D33-B708-2A521F14B1EA", "Net Weight Group");
			netWeightCalcEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			yield return netWeightCalcEditColumnStyleInfo;

			var netWeightUQDropEditColumnStyleInfo = new ZDropEditColumnStyleInfo();
			netWeightUQDropEditColumnStyleInfo.CaptionResourceString = Enterprise.Customs.TR.ETrade.GUI.Res.GetData("5A74C73B-954D-405C-BFE6-E745E2DFE22A", "UQ");
			netWeightUQDropEditColumnStyleInfo.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			netWeightUQDropEditColumnStyleInfo.ColumnName = "ABL_NetWeightUQ";
			netWeightUQDropEditColumnStyleInfo.GroupName = Enterprise.Customs.TR.ETrade.GUI.Res.GetData("FF1DE30F-75BA-4D33-B708-2A521F14B1EA", "Net Weight Group");
			netWeightUQDropEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			yield return netWeightUQDropEditColumnStyleInfo;

			var aBL_OtherValueTextBoxColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			aBL_OtherValueTextBoxColumnStyleInfo.ColumnName = "ABL_OtherValue";
			aBL_OtherValueTextBoxColumnStyleInfo.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			aBL_OtherValueTextBoxColumnStyleInfo.GroupName = domesticExpenditureGroup;
			aBL_OtherValueTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			yield return aBL_OtherValueTextBoxColumnStyleInfo;

			var aBL_RX_NKOtherValueCurrencyFinfBoxColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			aBL_RX_NKOtherValueCurrencyFinfBoxColumnStyleInfo.ColumnName = "ABL_RX_NKOtherValueCurrency";
			aBL_RX_NKOtherValueCurrencyFinfBoxColumnStyleInfo.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			aBL_RX_NKOtherValueCurrencyFinfBoxColumnStyleInfo.CaptionResourceString = Enterprise.Customs.TR.ETrade.GUI.Res.GetData("7DBD9657-76E2-4B8B-887C-412C02BB1DFF", "", "Currency", "Curr.", "");
			aBL_RX_NKOtherValueCurrencyFinfBoxColumnStyleInfo.GroupName = domesticExpenditureGroup;
			aBL_RX_NKOtherValueCurrencyFinfBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(35);
			yield return aBL_RX_NKOtherValueCurrencyFinfBoxColumnStyleInfo;

			var aBL_GoodsValueTextBoxColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			aBL_GoodsValueTextBoxColumnStyleInfo.ColumnName = "ABL_GoodsValue";
			aBL_GoodsValueTextBoxColumnStyleInfo.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			aBL_GoodsValueTextBoxColumnStyleInfo.GroupName = domesticExpenditureGroup;
			aBL_GoodsValueTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			yield return aBL_GoodsValueTextBoxColumnStyleInfo;

			var aBL_RX_NKGoodsValueCurrencyFinfBoxColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			aBL_RX_NKGoodsValueCurrencyFinfBoxColumnStyleInfo.ColumnName = "ABL_RX_NKGoodsValueCurrency";
			aBL_RX_NKGoodsValueCurrencyFinfBoxColumnStyleInfo.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			aBL_RX_NKGoodsValueCurrencyFinfBoxColumnStyleInfo.CaptionResourceString = Enterprise.Customs.TR.ETrade.GUI.Res.GetData("75743AFF-636F-4E3D-9F50-2D3F3AD87FA5", "", "Currency", "Curr.", "");
			aBL_RX_NKGoodsValueCurrencyFinfBoxColumnStyleInfo.GroupName = domesticExpenditureGroup;
			aBL_RX_NKGoodsValueCurrencyFinfBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(35);
			yield return aBL_RX_NKGoodsValueCurrencyFinfBoxColumnStyleInfo;

			var aBL_ProcedureCodeFindBoxColumnStyleInfo = new ZCodeFindBoxColumnStyleInfo();
			aBL_ProcedureCodeFindBoxColumnStyleInfo.ColumnName = "ABL_Procedure";
			aBL_ProcedureCodeFindBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			aBL_ProcedureCodeFindBoxColumnStyleInfo.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusProcedure;
			yield return aBL_ProcedureCodeFindBoxColumnStyleInfo;

			var billStatusDropEditColumnStyleInfo = new ZDropEditColumnStyleInfo();
			billStatusDropEditColumnStyleInfo.ColumnName = "ABL_BillStatus";
			billStatusDropEditColumnStyleInfo.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			billStatusDropEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			yield return billStatusDropEditColumnStyleInfo;

			var guaranteeTypeZDropEditColumnStyleInfo = new ZDropEditColumnStyleInfo();
			guaranteeTypeZDropEditColumnStyleInfo.ColumnName = "GuaranteeType";
			guaranteeTypeZDropEditColumnStyleInfo.GroupName = Enterprise.Customs.TR.ETrade.GUI.Res.GetData("4F74ECC4-9AAA-4BCE-B826-439239784AD3", "Guarantee Fields");
			guaranteeTypeZDropEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			yield return guaranteeTypeZDropEditColumnStyleInfo;

			var guaranteeRefNoZTextBoxColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			guaranteeRefNoZTextBoxColumnStyleInfo.ColumnName = "GuaranteeRefNo";
			guaranteeRefNoZTextBoxColumnStyleInfo.GroupName = Enterprise.Customs.TR.ETrade.GUI.Res.GetData("4F74ECC4-9AAA-4BCE-B826-439239784AD3", "Guarantee Fields");
			guaranteeRefNoZTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			yield return guaranteeRefNoZTextBoxColumnStyleInfo;

			var guaranteeAmountZCalcEditColumnStyleInfo = new ZArchitecture.ZCalcEditColumnStyleInfo();
			guaranteeAmountZCalcEditColumnStyleInfo.ColumnName = "GuaranteeAmount";
			guaranteeAmountZCalcEditColumnStyleInfo.GroupName = Enterprise.Customs.TR.ETrade.GUI.Res.GetData("4F74ECC4-9AAA-4BCE-B826-439239784AD3", "Guarantee Fields");
			guaranteeAmountZCalcEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			yield return guaranteeAmountZCalcEditColumnStyleInfo;
		}

		protected override IReadOnlyDictionary<bool, string[]> GetBillsGridColumnAvailabilityCore(ASYCUDA.Business.AsycudaManifestHeader header)
		{
			var result = new Dictionary<bool, string[]>();
			result.Add(true, new[] { AsycudaBill.Schema.GuaranteeType, AsycudaBill.Schema.GuaranteeRefNo, AsycudaBill.Schema.GuaranteeAmount/* AsycudaBill.Schema.ABL_CargoStatus*/ });
			return result;
		}

		protected override IPanelLayoutProvider GetBillPartiesLayoutCore() => new ETradeBillPartiesLayouts();

		protected override string[] GetPacksGridColumnsOrderCore()
		{
			return new[]
			{
				AsycudaPack.Schema.APA_PackQty,
				AsycudaPack.Schema.APA_PackUQ,
				AsycudaPack.Schema.APA_CommodityCode,
				AsycudaPack.Schema.APA_GoodsDescription,
				AsycudaPack.Schema.APA_MarksAndNumbers
			};
		}

		void ManualRegistrationNoEntry(object sender, EventArgs args)
		{
			var menu = sender as ZMenuItem;
			var mainForm = menu.GetMainMenu()?.GetForm() as ZForm;
			var header = mainForm.BusinessEntity as IRegistrationNoEntryProvider;
			ManualRegistrationHelper.ManualRegistrationNoEntry(header, mainForm);
		}

		protected override IEnumerable<ZMenuItem> GetActionsExtraMenuItemsCore()
		{
			var caption = ResString.GetMultilingualString("1A2D38DE-354E-4D28-902A-BEC99DF60E8F", "Manual Registration No Entry");
			yield return new ZMenuItem(caption, ManualRegistrationNoEntry);
		}

		protected override IEnumerable<ZGridColumnInfo> GetTaxesGridExtraColumnInfosCore()
		{
			var aet_descriptionTextBoxColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			aet_descriptionTextBoxColumnStyleInfo.ColumnName = "AET_TypeDescription";
			aet_descriptionTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			yield return aet_descriptionTextBoxColumnStyleInfo;
		}
	}
}
