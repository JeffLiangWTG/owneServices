using System;
using System.Collections.Generic;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core.Forms;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.TW.BriefCustomsDeclaration.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.GUI
{
	public class ApplicationGUIProvider : ASYCUDA.GUI.ApplicationGUIProvider
	{
		public override ASYCUDA.GUI.MenuBuilder GetMenuBuilder(ZForm mainForm, ASYCUDA.Business.AsycudaManifestHeader header)
		{
			return new MenuBuilder(header, mainForm);
		}

		public override Type ApplicationBusinessProviderType => typeof(ApplicationBusinessProvider);

		protected override IPanelLayoutProvider GetManifestLayoutCore() => new TWManifestLayouts();

		protected override IPanelLayoutProvider GetBillLayoutCore() => new TWBillLayouts();

		protected override IPanelLayoutProvider GetBillPartiesLayoutCore() => new TWBillPartiesLayouts();

		protected override ResourceStringData MainTabPageGroupBoxNameCore => ResourceStringData.Empty;

		protected override IReadOnlyDictionary<bool, string[]> GetBillsGridColumnAvailabilityCore(ASYCUDA.Business.AsycudaManifestHeader header)
		{
			return new Dictionary<bool, string[]>()
			{
				{ false, new[]
					{
						AsycudaBill.Schema.ABL_BolType,
						AsycudaBill.Schema.ABL_RL_NKOrigin,
						AsycudaBill.Schema.ABL_CarrierReference,
						AsycudaBill.Schema.ABL_GoodsDescription,
						AsycudaBill.Schema.ABL_FreightValue,
						AsycudaBill.Schema.ABL_RX_NKFreightValueCurrency,
						AsycudaBill.Schema.ABL_InsuranceValue,
						AsycudaBill.Schema.ABL_RX_NKInsuranceValueCurrency,
						AsycudaBill.Schema.ABL_TransportValue,
						AsycudaBill.Schema.ABL_RX_NKTransportValueCurrency,
						AsycudaBill.Schema.ABL_PrepaidCollect,
						AsycudaBill.Schema.ABL_CargoStatus,
						AsycudaBill.Schema.ShipperOrgPK,
						AsycudaBill.Schema.ABL_OA_Shipper,
						AsycudaBill.Schema.ABL_ShipperName,
						AsycudaBill.Schema.ABL_ShipperStreet1,
						AsycudaBill.Schema.ABL_ShipperStreet2,
						AsycudaBill.Schema.ABL_ShipperCity,
						AsycudaBill.Schema.ABL_ShipperState,
						AsycudaBill.Schema.ABL_ShipperPostcode,
						AsycudaBill.Schema.ABL_RN_NKShipperCountry,
						AsycudaBill.Schema.ConsigneeOrgPK,
						AsycudaBill.Schema.ABL_OA_Consignee,
						AsycudaBill.Schema.ABL_ConsigneeName,
						AsycudaBill.Schema.ABL_ConsigneeStreet1,
						AsycudaBill.Schema.ABL_ConsigneeStreet2,
						AsycudaBill.Schema.ABL_ConsigneeCity,
						AsycudaBill.Schema.ABL_ConsigneeState,
						AsycudaBill.Schema.ABL_ConsigneePostcode,
						AsycudaBill.Schema.ABL_RN_NKConsigneeCountry,
						AsycudaBill.Schema.ABL_ConsigneePhone,
						AsycudaBill.Schema.NotifyPartyOrgPK,
						AsycudaBill.Schema.ABL_OA_NotifyParty,
						AsycudaBill.Schema.ABL_NotifyPartyName,
						AsycudaBill.Schema.ABL_NotifyPartyStreet1,
						AsycudaBill.Schema.ABL_NotifyPartyStreet2,
						AsycudaBill.Schema.ABL_NotifyPartyCity,
						AsycudaBill.Schema.ABL_NotifyPartyState,
						AsycudaBill.Schema.ABL_NotifyPartyPostcode,
						AsycudaBill.Schema.ABL_RN_NKNotifyPartyCountry,
						AsycudaBill.Schema.ABL_NotifyPartyPhone,
						AsycudaBill.Schema.DiscountValue,
						AsycudaBill.Schema.DiscountValueCurrency,
						AsycudaBill.Schema.OtherChargesValue,
						AsycudaBill.Schema.OtherChargesValueCurrency,
						AsycudaBill.Schema.CustomsJobNumber,
						AsycudaBill.Schema.ABL_RL_NKFinalDestination,
						AsycudaBill.Schema.ABL_Volume,
						AsycudaBill.Schema.ABL_VolumeUQ,
						AsycudaBill.Schema.ABL_MarksAndNumbers,
						AsycudaBill.Schema.ABL_UCRNumber,
						AsycudaBill.Schema.ABL_Remarks
					}
				}
			};
		}

		protected override IEnumerable<ZGridColumnInfo> GetBillsGridExtraColumnInfosCore()
		{
			var remarksMultiLineTextBoxColumnInfo = new ZMultiLineTextBoxColumnInfo();
			remarksMultiLineTextBoxColumnInfo.ColumnName = AsycudaBill.Schema.Remarks;
			remarksMultiLineTextBoxColumnInfo.MinimumEditControlWidth = 300;
			remarksMultiLineTextBoxColumnInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			remarksMultiLineTextBoxColumnInfo.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			yield return remarksMultiLineTextBoxColumnInfo;

			var procedureDropEditColumnStyleInfo = new ZDropEditColumnStyleInfo();
			procedureDropEditColumnStyleInfo.ColumnName = AsycudaBill.Schema.ABL_Procedure;
			procedureDropEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			yield return procedureDropEditColumnStyleInfo;

			var incotermDropEditColumnStyleInfo = new ZDropEditColumnStyleInfo();
			incotermDropEditColumnStyleInfo.ColumnName = AsycudaBill.Schema.ABL_Incoterm;
			incotermDropEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			yield return incotermDropEditColumnStyleInfo;

			var goodsValueResourceStringData = Res.GetData("EEAA27FA-A7B1-466A-A2D0-A19EF1FB0085", "Total Invoice Amount");
			var goodsValueCalcEditColumnStyleInfo = new ZArchitecture.ZCalcEditColumnStyleInfo();
			goodsValueCalcEditColumnStyleInfo.ColumnName = AsycudaBill.Schema.ABL_GoodsValue;
			goodsValueCalcEditColumnStyleInfo.GroupName = goodsValueResourceStringData;
			goodsValueCalcEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			yield return goodsValueCalcEditColumnStyleInfo;

			var goodsValueCurrencyCodeFindBoxColumnStyleInfo = new ZCodeFindBoxColumnStyleInfo();
			goodsValueCurrencyCodeFindBoxColumnStyleInfo.ColumnName = AsycudaBill.Schema.ABL_RX_NKGoodsValueCurrency;
			goodsValueCurrencyCodeFindBoxColumnStyleInfo.GroupName = goodsValueResourceStringData;
			goodsValueCurrencyCodeFindBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			yield return goodsValueCurrencyCodeFindBoxColumnStyleInfo;

			var portOfLoadingCodeFindBoxColumnStyleInfo = new ZCodeFindBoxColumnStyleInfo();
			portOfLoadingCodeFindBoxColumnStyleInfo.ColumnName = AsycudaBill.Schema.ABL_RL_NKPortOfLoading;
			portOfLoadingCodeFindBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			yield return portOfLoadingCodeFindBoxColumnStyleInfo;

			var portOfDischargeCodeFindBoxColumnStyleInfo = new ZCodeFindBoxColumnStyleInfo();
			portOfDischargeCodeFindBoxColumnStyleInfo.ColumnName = AsycudaBill.Schema.ABL_RL_NKPortOfDischarge;
			portOfDischargeCodeFindBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			yield return portOfDischargeCodeFindBoxColumnStyleInfo;

			var locationInformationTextBoxColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			locationInformationTextBoxColumnStyleInfo.ColumnName = AsycudaBill.Schema.ABL_LocationInformation;
			locationInformationTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			yield return locationInformationTextBoxColumnStyleInfo;

			var otherValueCalcEditColumnStyleInfo = new ZArchitecture.ZCalcEditColumnStyleInfo();
			otherValueCalcEditColumnStyleInfo.ColumnName = AsycudaBill.Schema.ABL_OtherValue;
			otherValueCalcEditColumnStyleInfo.GroupName = Enterprise.Customs.TW.BriefCustomsDeclaration.GUI.Res.GetData("CF87C394-9E4B-4DD2-9E5C-3F0C2F795348", "Additions");
			otherValueCalcEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			yield return otherValueCalcEditColumnStyleInfo;

			var otherValueCurrencyCodeFindBoxColumnStyleInfo = new ZCodeFindBoxColumnStyleInfo();
			otherValueCurrencyCodeFindBoxColumnStyleInfo.ColumnName = AsycudaBill.Schema.ABL_RX_NKOtherValueCurrency;
			otherValueCurrencyCodeFindBoxColumnStyleInfo.GroupName = Enterprise.Customs.TW.BriefCustomsDeclaration.GUI.Res.GetData("CF87C394-9E4B-4DD2-9E5C-3F0C2F795348", "Additions");
			otherValueCurrencyCodeFindBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			yield return otherValueCurrencyCodeFindBoxColumnStyleInfo;

			var otherDeductionsCalcEditColumnStyleInfo = new ZArchitecture.ZCalcEditColumnStyleInfo();
			otherDeductionsCalcEditColumnStyleInfo.ColumnName = AsycudaBill.Schema.ABL_OtherDeductions;
			otherDeductionsCalcEditColumnStyleInfo.GroupName = Enterprise.Customs.TW.BriefCustomsDeclaration.GUI.Res.GetData("88432D55-04EB-43DC-9822-11EA22792F17", "Deductions");
			otherDeductionsCalcEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			yield return otherDeductionsCalcEditColumnStyleInfo;

			var otherDeductionsCurrencyCodeFindBoxColumnStyleInfo = new ZCodeFindBoxColumnStyleInfo();
			otherDeductionsCurrencyCodeFindBoxColumnStyleInfo.ColumnName = AsycudaBill.Schema.ABL_RX_NKOtherDeductionsCurrency;
			otherDeductionsCurrencyCodeFindBoxColumnStyleInfo.GroupName = Enterprise.Customs.TW.BriefCustomsDeclaration.GUI.Res.GetData("88432D55-04EB-43DC-9822-11EA22792F17", "Deductions");
			otherDeductionsCurrencyCodeFindBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			yield return otherDeductionsCurrencyCodeFindBoxColumnStyleInfo;

			var exchangeRateCalcEditColumnStyleInfo = new ZArchitecture.ZCalcEditColumnStyleInfo();
			exchangeRateCalcEditColumnStyleInfo.ColumnName = AsycudaBill.Schema.ExchangeRate;
			exchangeRateCalcEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			yield return exchangeRateCalcEditColumnStyleInfo;
		}

		protected override IReadOnlyDictionary<string, bool> GetBillsGridColumnVisiblilityOnValueChangedCore(ASYCUDA.Business.AsycudaManifestHeader header)
		{
			var asycudaManifestHeader = (AsycudaManifestHeader)header;
			return new Dictionary<string, bool>()
			{
				{ AsycudaBill.Schema.ABL_CustomsValue, true },
				{ AsycudaBill.Schema.ABL_RX_NKCustomsValueCurrency, true },
				{ AsycudaBill.Schema.ABL_RL_NKPortOfLoading, asycudaManifestHeader.IsImport },
				{ AsycudaBill.Schema.ABL_RL_NKPortOfDischarge, asycudaManifestHeader.IsExport },
				{ AsycudaBill.Schema.ABL_LocationInformation, asycudaManifestHeader.IsExport },
				{ AsycudaBill.Schema.ABL_FreightValue, asycudaManifestHeader.IsImport },
				{ AsycudaBill.Schema.ABL_RX_NKFreightValueCurrency, asycudaManifestHeader.IsImport },
				{ AsycudaBill.Schema.ABL_InsuranceValue, asycudaManifestHeader.IsImport },
				{ AsycudaBill.Schema.ABL_RX_NKInsuranceValueCurrency, asycudaManifestHeader.IsImport },
				{ AsycudaBill.Schema.ABL_OtherValue, asycudaManifestHeader.IsImport },
				{ AsycudaBill.Schema.ABL_RX_NKOtherValueCurrency, asycudaManifestHeader.IsImport },
				{ AsycudaBill.Schema.ABL_OtherDeductions, asycudaManifestHeader.IsImport },
				{ AsycudaBill.Schema.ABL_RX_NKOtherDeductionsCurrency, asycudaManifestHeader.IsImport }
			};
		}

		protected override IEnumerable<string> GetBillsGridColumnsOrderCore()
		{
			yield return AsycudaBill.Schema.ABL_SequenceNumber;
			yield return AsycudaBill.Schema.ABL_BillNumber;
			yield return AsycudaBill.Schema.Remarks;
			yield return AsycudaBill.Schema.ABL_Procedure;
			yield return AsycudaBill.Schema.ABL_Incoterm;
			yield return AsycudaBill.Schema.ABL_ManifestQty;
			yield return AsycudaBill.Schema.ABL_ManifestUQ;
			yield return AsycudaBill.Schema.ABL_GrossWeight;
			yield return AsycudaBill.Schema.ABL_GrossWeightUQ;
			yield return AsycudaBill.Schema.ABL_CustomsValue;
			yield return AsycudaBill.Schema.ABL_RX_NKCustomsValueCurrency;
			yield return AsycudaBill.Schema.ABL_GoodsValue;
			yield return AsycudaBill.Schema.ABL_RX_NKGoodsValueCurrency;
			yield return AsycudaBill.Schema.ABL_RL_NKPortOfLoading;
			yield return AsycudaBill.Schema.ABL_RL_NKPortOfDischarge;
			yield return AsycudaBill.Schema.ABL_LocationInformation;
			yield return AsycudaBill.Schema.ABL_FreightValue;
			yield return AsycudaBill.Schema.ABL_RX_NKFreightValueCurrency;
			yield return AsycudaBill.Schema.ABL_InsuranceValue;
			yield return AsycudaBill.Schema.ABL_RX_NKInsuranceValueCurrency;
			yield return AsycudaBill.Schema.ABL_OtherValue;
			yield return AsycudaBill.Schema.ABL_RX_NKOtherValueCurrency;
			yield return AsycudaBill.Schema.ABL_OtherDeductions;
			yield return AsycudaBill.Schema.ABL_RX_NKOtherDeductionsCurrency;
			yield return AsycudaBill.Schema.ExchangeRate;
		}

		protected override void AMA_NatureInfoValueChangedCore(ASYCUDA.Business.AsycudaManifestHeader header, ZGridWithDynamicColumnHandler billsGrid, object sender, EventArgs e)
		{
			base.AMA_NatureInfoValueChangedCore(header, billsGrid, sender, e);

			var isExport = header is AsycudaManifestHeader twHeader && twHeader.IsExport;
			var groupName = isExport ? Res.GetData("D7E235D1-7669-4F8C-B69C-634EBBAC2192", "FOB") : Res.GetData("5A369198-D0B8-404F-AC13-3D784FDF9EEA", "Customs Value");
			billsGrid.SetColumnCaption(AsycudaBill.Schema.ABL_CustomsValue, groupName.Caption);
			billsGrid.SetColumnGroupName(AsycudaBill.Schema.ABL_CustomsValue, groupName);
			billsGrid.SetColumnGroupName(AsycudaBill.Schema.ABL_RX_NKCustomsValueCurrency, groupName);
		}

		protected override void CustomizeBillsGridCore(ZGridWithDynamicColumnHandler billsGrid)
		{
			base.CustomizeBillsGridCore(billsGrid);

			var freightValueResourceStringData = Res.GetData("F6A63331-5164-4506-B27B-0BC987DB95CA", "Freight Value");
			billsGrid.SetColumnGroupName(AsycudaBill.Schema.ABL_FreightValue, freightValueResourceStringData);
			billsGrid.SetColumnGroupName(AsycudaBill.Schema.ABL_RX_NKFreightValueCurrency, freightValueResourceStringData);

			var transportValueResourceStringData = Res.GetData("22035B0A-8482-41C7-A6D8-A8013EA3CDB3", "Transport Value");
			billsGrid.SetColumnGroupName(AsycudaBill.Schema.ABL_TransportValue, transportValueResourceStringData);
			billsGrid.SetColumnGroupName(AsycudaBill.Schema.ABL_RX_NKTransportValueCurrency, transportValueResourceStringData);
		}

		protected override IReadOnlyDictionary<string, int> GetBillsGridColumnsWidthCore()
		{
			return new Dictionary<string, int>()
			{
				{ AsycudaBill.Schema.ABL_ManifestQty, 70 },
			};
		}

		protected override ResourceString GetAsycudaMenuCaptionCore() => ResString.GetMultilingualString("TWBriefDeclarationAsycudaMenu|MainMenuItem", "Brokerage");

		protected override IEnumerable<IAdditionalTabPage> GetBillAdditionalTabPageUserControlCore() => new IAdditionalTabPage[] {
			new TWBillCalculationUserControl(),
			new TWBillItemsUserControl()
		};
	}
}
