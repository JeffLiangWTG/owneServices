using System;
using System.Collections.Generic;
using Enterprise.Core.Forms;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.US.ForwarderManifest.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.ForwarderManifest.GUI
{
	public class USExportApplicationGUIProvider : ApplicationGUIProvider
	{
		public override Type ApplicationBusinessProviderType => typeof(USExportApplicationBusinessProvider);

		public override MenuBuilder GetMenuBuilder(ZForm mainForm, AsycudaManifestHeader header) => new USExportMenuBuilder(header, mainForm);

		protected override IPanelLayoutProvider GetManifestLayoutCore() => new USExportManifestLayouts();

		protected override IEnumerable<IAdditionalTabPage> GetBillAdditionalTabPageUserControlCore()
		{
			yield return new AsycudaPackUserControl();
			yield return new USExportVisitedPortUserControl();
		}

		protected override string[] GetPacksGridMandatoryColumnsCore() => PacksGridColumnsForUSAllVisibleMandatory();

		protected override IPanelLayoutProvider GetBillLayoutCore() => new USExportManifestBillLayouts();

		string[] PacksGridColumnsForUSAllVisibleMandatory()
		{
			return new string[]
			{
				UNDGDataItemFormManager.ColumnNames.UNDGSubstanceManagerValue,
				UNDGDataItemFormManager.ColumnNames.UNDGClassManagerValue,
			};
		}

		protected override AsycudaItemSelectionDialog GetNewAsycudaItemSelectionDialogCore(MessageChooser messageChooser, string itemsType, string messageType)
		{
			switch (messageType)
			{
				case MessageTypeList.Codes.ExportManifestSubmission:
					return new USExportManifestSelectionDialog((UEMMessageChooser)messageChooser, itemsType);
			}

			return base.GetNewAsycudaItemSelectionDialogCore(messageChooser, itemsType, messageType);
		}

		protected override string[] GetPacksGridColumnsOrderCore()
		{
			return new string[]
			{
				AsycudaPack.Schema.ContainerPK,
				AsycudaPack.Schema.APA_PackQty,
				AsycudaPack.Schema.APA_PackUQ,
				AsycudaPack.Schema.APA_CommodityCode,
				AsycudaPack.Schema.APA_GoodsDescription,
				AsycudaPack.Schema.APA_MarksAndNumbers,
				AsycudaPack.Schema.APA_VINNumber,
				AsycudaPack.Schema.APA_Weight,
				AsycudaPack.Schema.APA_WeightUQ,
				AsycudaPack.Schema.APA_Volume,
				AsycudaPack.Schema.APA_VolumeUQ,
				UNDGDataItemFormManager.ColumnNames.UNDGSubstanceManagerValue,
				UNDGDataItemFormManager.ColumnNames.UNDGClassManagerValue,
				USExportAsycudaPack.Schema.PSN,
				USExportAsycudaPack.Schema.ContactPK,
				USExportAsycudaPack.Schema.ContactPhone,
				USExportAsycudaPack.Schema.FlashpointTemperatureC,
				USExportAsycudaPack.Schema.FlashpointTemperatureF,
			};
		}

		protected override IEnumerable<ZGridColumnInfo> GetPacksGridExtraColumnInfosCore()
		{
			var psnTextBoxColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			psnTextBoxColumnStyleInfo.CaptionResourceString = Enterprise.Customs.US.ForwarderManifest.GUI.Res.GetData("0D1EA78E-B3A5-42C4-8C0E-D9029FEDA989", "Hazardous Material Description");
			psnTextBoxColumnStyleInfo.ColumnName = "PSN";
			psnTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			yield return psnTextBoxColumnStyleInfo;

			var contactNameCodeFindBoxColumnStyleInfo = new ZGuidFindBoxColumnStyleInfo();
			contactNameCodeFindBoxColumnStyleInfo.BindToList = "UNDGs+Contacts";
			contactNameCodeFindBoxColumnStyleInfo.CaptionResourceString = Enterprise.Customs.US.ForwarderManifest.GUI.Res.GetData("675BA832-8164-424F-BA4F-E77804F4EA16", "Hazardous Contact Name");
			contactNameCodeFindBoxColumnStyleInfo.ColumnName = "ContactPK";
			contactNameCodeFindBoxColumnStyleInfo.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
			contactNameCodeFindBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			yield return contactNameCodeFindBoxColumnStyleInfo;

			var phoneTextBoxColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			phoneTextBoxColumnStyleInfo.CaptionResourceString = Enterprise.Customs.US.ForwarderManifest.GUI.Res.GetData("030DC057-2DEC-4FFB-8D1B-D86724F15A05", "Hazardous Contact Phone");
			phoneTextBoxColumnStyleInfo.ColumnName = "ContactPhone";
			phoneTextBoxColumnStyleInfo.IsReadOnly = true;
			phoneTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			yield return phoneTextBoxColumnStyleInfo;

			var flashPointCCalcEditColumnStyleInfo = new ZArchitecture.ZCalcEditColumnStyleInfo();
			flashPointCCalcEditColumnStyleInfo.BindToDecimalPlaces = null;
			flashPointCCalcEditColumnStyleInfo.CaptionResourceString = Enterprise.Customs.US.ForwarderManifest.GUI.Res.GetData("D499F696-D0A7-4BC3-B2F1-8D3F69D3E68B", "Flash point temperature UOM (C)");
			flashPointCCalcEditColumnStyleInfo.ColumnName = "FlashpointTemperatureC";
			flashPointCCalcEditColumnStyleInfo.Decimals = 1;
			flashPointCCalcEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			yield return flashPointCCalcEditColumnStyleInfo;

			var flashPointFCalcEditColumnStyleInfo = new ZArchitecture.ZCalcEditColumnStyleInfo();
			flashPointFCalcEditColumnStyleInfo.BindToDecimalPlaces = null;
			flashPointFCalcEditColumnStyleInfo.CaptionResourceString = Enterprise.Customs.US.ForwarderManifest.GUI.Res.GetData("A065A47C-D650-4065-9F4A-8AE27DE968AE", "Flash point temperature UOM (F)");
			flashPointFCalcEditColumnStyleInfo.ColumnName = "FlashpointTemperatureF";
			flashPointFCalcEditColumnStyleInfo.Decimals = 1;
			flashPointFCalcEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			yield return flashPointFCalcEditColumnStyleInfo;
		}

		protected override IEnumerable<ZGridColumnInfo> GetBillsGridExtraColumnInfosCore()
		{
			var aesITNNumberTextBoxColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			aesITNNumberTextBoxColumnStyleInfo.CaptionResourceString = Enterprise.Customs.US.ForwarderManifest.GUI.Res.GetData("8A650773-FAB7-4F75-BEF5-53B4CEFF16F4", "AES ITN Numbers");
			aesITNNumberTextBoxColumnStyleInfo.ColumnName = "AESITNNumbers";
			aesITNNumberTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			yield return aesITNNumberTextBoxColumnStyleInfo;

			var inBondNumberTextBoxColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			inBondNumberTextBoxColumnStyleInfo.CaptionResourceString = Enterprise.Customs.US.ForwarderManifest.GUI.Res.GetData("7C43FC83-A4C6-4AB4-B5F1-19738CF5F664", "In-Bond Numbers");
			inBondNumberTextBoxColumnStyleInfo.ColumnName = "InBondNumbers";
			inBondNumberTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			yield return inBondNumberTextBoxColumnStyleInfo;
		}

		protected override IReadOnlyDictionary<bool, string[]> GetBillsGridColumnAvailabilityCore(AsycudaManifestHeader header)
		{
			return new Dictionary<bool, string[]>
			{
				{
					false,
					new[]
					{
						AsycudaBill.Schema.ABL_RX_NKCustomsValueCurrency,
						AsycudaBill.Schema.DiscountValueCurrency,
						AsycudaBill.Schema.ABL_RX_NKFreightValueCurrency,
						AsycudaBill.Schema.ABL_RX_NKTransportValueCurrency,
						AsycudaBill.Schema.ABL_RX_NKInsuranceValueCurrency,
						AsycudaBill.Schema.OtherChargesValueCurrency,
						AsycudaBill.Schema.ABL_BolType
					}
				}
			};
		}
	}
}
