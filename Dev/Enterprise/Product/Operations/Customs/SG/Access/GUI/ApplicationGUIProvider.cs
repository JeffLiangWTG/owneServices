using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.SG.Access.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.SG.Access.GUI
{
	public class ApplicationGUIProvider : ASYCUDA.GUI.ApplicationGUIProvider
	{
		public override Type ApplicationBusinessProviderType => typeof(ApplicationBusinessProvider);

		public override ASYCUDA.GUI.MenuBuilder GetMenuBuilder(ZForm mainForm, ASYCUDA.Business.AsycudaManifestHeader header)
		{
			return new MenuBuilder(header, mainForm);
		}

		protected override ASYCUDA.GUI.AsycudaItemSelectionDialog GetNewAsycudaItemSelectionDialogCore(ASYCUDA.Business.MessageChooser messageChooser, string itemsType, string messageType)
		{
			return new AsycudaItemSelectionDialog((MessageChooser)messageChooser, itemsType);
		}

		protected override IEnumerable<ZGridColumnInfo> GetBillsGridExtraColumnInfosCore()
		{
			var cycleDateEditColumnStyleInfo = new ZArchitecture.ZDateEditColumnStyleInfo();
			cycleDateEditColumnStyleInfo.ColumnName = "CycleDate";
			cycleDateEditColumnStyleInfo.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			cycleDateEditColumnStyleInfo.GroupName = Enterprise.Customs.SG.Access.GUI.Res.GetData("827eac74-5d8e-405a-8145-1048055c7f58", "Cycle Fields");
			cycleDateEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			yield return cycleDateEditColumnStyleInfo;
			var cycleNumberDropEditColumnStyleInfo = new ZDropEditColumnStyleInfo();
			cycleNumberDropEditColumnStyleInfo.ColumnName = "CycleNumber";
			cycleNumberDropEditColumnStyleInfo.GroupName = Enterprise.Customs.SG.Access.GUI.Res.GetData("827eac74-5d8e-405a-8145-1048055c7f58", "Cycle Fields");
			cycleNumberDropEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			yield return cycleNumberDropEditColumnStyleInfo;
			var messageStatusTextBoxColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			messageStatusTextBoxColumnStyleInfo.IsVisible = false;
			messageStatusTextBoxColumnStyleInfo.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			messageStatusTextBoxColumnStyleInfo.ColumnName = "ABL_MessageStatus";
			messageStatusTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			yield return messageStatusTextBoxColumnStyleInfo;
			var billStatusDropEditColumnStyleInfo = new ZDropEditColumnStyleInfo();
			billStatusDropEditColumnStyleInfo.IsVisible = false;
			billStatusDropEditColumnStyleInfo.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			billStatusDropEditColumnStyleInfo.ColumnName = "ABL_BillStatus";
			billStatusDropEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			yield return billStatusDropEditColumnStyleInfo;
		}

		protected override IEnumerable<string> GetBillsGridColumnsOrderCore()
		{
			yield return AsycudaBill.Schema.ABL_BillNumber;
			yield return AsycudaBill.Schema.ABL_BolType;
			yield return AsycudaBill.Schema.ABL_RL_NKOrigin;
			yield return AsycudaBill.Schema.ABL_RL_NKFinalDestination;
			yield return AsycudaBill.Schema.ABL_GoodsDescription;
			yield return AsycudaBill.Schema.ABL_ManifestQty;
			yield return AsycudaBill.Schema.ABL_ManifestUQ;
			yield return AsycudaBill.Schema.ABL_GrossWeight;
			yield return AsycudaBill.Schema.ABL_GrossWeightUQ;
			yield return AsycudaBill.Schema.ABL_Volume;
			yield return AsycudaBill.Schema.ABL_VolumeUQ;
			yield return AsycudaBill.Schema.ABL_MarksAndNumbers;
			yield return AsycudaBill.Schema.ABL_Remarks;
			yield return AsycudaBill.Schema.ABL_UCRNumber;
			yield return AsycudaBill.Schema.CustomsJobNumber;
			yield return AsycudaBill.Schema.CycleDate;
			yield return AsycudaBill.Schema.CycleNumber;
			yield return AsycudaBill.Schema.ABL_MessageStatus;
			yield return AsycudaBill.Schema.ABL_BillStatus;
		}

		protected override IReadOnlyDictionary<bool, string[]> GetBillsGridColumnAvailabilityCore(ASYCUDA.Business.AsycudaManifestHeader header)
		{
			var result = new Dictionary<bool, string[]>();
			result.Add(((AsycudaManifestHeader)header).IsImport, new[] { AsycudaBill.Schema.CycleDate, AsycudaBill.Schema.CycleNumber });
			return result;
		}

		protected override IEnumerable<ZMenuItem> GetBillsGridExtraMenuItemsCore(ZGridWithDynamicColumnHandler billsGrid)
		{
			var amendBillDetails = new ZMenuItem(amendBillDetailsMenuItemText);
			var grid = billsGrid;
			amendBillDetails.Click += (s, e) =>
			{
				var bill = grid.GetCurrent() as AsycudaBill;
				bill?.AmendBillDetails();
			};

			yield return amendBillDetails;
		}
		const string amendBillDetailsMenuItemText = "Amend Bill Details";

		protected override void SetBillsGridExtraMenuItemsVisibilityCore(ZMenuItem[] billsGridExtraMenuItems, ASYCUDA.Business.AsycudaBill[] selectedBills, ASYCUDA.Business.AsycudaBill currentBill)
		{
			var amendBillDetails = billsGridExtraMenuItems.FirstOrDefault(x => x.Text == amendBillDetailsMenuItemText);
			if (amendBillDetails != null && currentBill is AsycudaBill bill)
			{
				amendBillDetails.Visible = GetManifestType(bill) == Constants.ManifestType.Export
					&& (!GetHouseBillRegistrationNumber(bill).IsEmpty || bill.Packs.OfType<AsycudaPack>().Any(x => !GetPackItemRegistrationNumber(x).IsEmpty));
			}
		}

		static ZString GetManifestType(AsycudaBill bill) => bill?.Header.AMA_ManifestType ?? ZString.Empty;

		static ZString GetHouseBillRegistrationNumber(AsycudaBill bill) => bill?.RegistrationNumber ?? ZString.Empty;

		static ZString GetPackItemRegistrationNumber(AsycudaPack pack) => pack?.PackedItem.RegistrationNumber ?? ZString.Empty;

		protected override string[] GetPackedItemColumnsCore()
		{
			return new string[]
			{
				AsycudaPackedItem.Schema.API_FormattedTariff,
				AsycudaPackedItem.Schema.API_GoodsDescription,
				AsycudaPackedItem.Schema.API_CustomsQty,
				AsycudaPackedItem.Schema.API_CustomsUQ,
				AsycudaPackedItem.Schema.API_CustomsValue,
				AsycudaPackedItem.Schema.API_DutyAmount,
				AsycudaPackedItem.Schema.API_TaxAmount,
				AsycudaPackedItem.Schema.API_RN_NKGoodsOrigin,
				AsycudaPackedItem.Schema.API_MessageStatus,
				AsycudaPackedItem.Schema.API_PackStatus,
				AsycudaPackedItem.Schema.GoodsType,
				AsycudaPackedItem.Schema.GSTPaid
			};
		}

		protected override IEnumerable<ZGridColumnInfo> GetPacksGridExtraColumnInfosCore()
		{
			var tariffColumnStyleInfo = new V4.GUI.TariffColumnStyleInfo();
			tariffColumnStyleInfo.CaptionResourceString = Res.GetData("9E16BE32-9607-43E0-B8B1-8C85E7ACAF21", "Tariff");
			tariffColumnStyleInfo.ColumnName = "PackedItem+API_FormattedTariff";
			tariffColumnStyleInfo.IsMandatory = true;
			tariffColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			tariffColumnStyleInfo.TariffType = Universal.Constants.TariffTypes.HarmonizedSystem;
			yield return tariffColumnStyleInfo;

			var goodsDescriptionTextBoxColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			goodsDescriptionTextBoxColumnStyleInfo.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			goodsDescriptionTextBoxColumnStyleInfo.ColumnName = "PackedItem+API_GoodsDescription";
			goodsDescriptionTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(111);
			yield return goodsDescriptionTextBoxColumnStyleInfo;

			var customsQtyCalcEditColumnStyleInfo = new ZArchitecture.ZCalcEditColumnStyleInfo();
			customsQtyCalcEditColumnStyleInfo.BindToDecimalPlaces = null;
			customsQtyCalcEditColumnStyleInfo.CaptionResourceString = Enterprise.Customs.SG.Access.GUI.Res.GetData("AB578BCF-0D5D-4562-B479-0640CD95E714", "Customs Qty");
			customsQtyCalcEditColumnStyleInfo.ColumnName = "PackedItem+API_CustomsQty";
			customsQtyCalcEditColumnStyleInfo.Decimals = 5;
			customsQtyCalcEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			yield return customsQtyCalcEditColumnStyleInfo;

			var customsUQDropEditColumnStyleInfo = new ZDropEditColumnStyleInfo();
			customsUQDropEditColumnStyleInfo.ColumnName = "PackedItem+API_CustomsUQ";
			customsUQDropEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(38);
			yield return customsUQDropEditColumnStyleInfo;

			var customsValueCalcEditColumnStyleInfo = new ZArchitecture.ZCalcEditColumnStyleInfo();
			customsValueCalcEditColumnStyleInfo.BindToDecimalPlaces = null;
			customsValueCalcEditColumnStyleInfo.ColumnName = "PackedItem+API_CustomsValue";
			customsValueCalcEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(98);
			yield return customsValueCalcEditColumnStyleInfo;

			var dutyAmountCalcEditColumnStyleInfo = new ZArchitecture.ZCalcEditColumnStyleInfo();
			dutyAmountCalcEditColumnStyleInfo.BindToDecimalPlaces = null;
			dutyAmountCalcEditColumnStyleInfo.ColumnName = "PackedItem+API_DutyAmount";
			dutyAmountCalcEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			yield return dutyAmountCalcEditColumnStyleInfo;

			var taxAmountCalcEditColumnStyleInfo = new ZArchitecture.ZCalcEditColumnStyleInfo();
			taxAmountCalcEditColumnStyleInfo.BindToDecimalPlaces = null;
			taxAmountCalcEditColumnStyleInfo.ColumnName = "PackedItem+API_TaxAmount";
			taxAmountCalcEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(85);
			yield return taxAmountCalcEditColumnStyleInfo;

			var goodsOriginCodeFindBoxColumnStyleInfo = new ZCodeFindBoxColumnStyleInfo();
			goodsOriginCodeFindBoxColumnStyleInfo.ColumnName = "PackedItem+API_RN_NKGoodsOrigin";
			goodsOriginCodeFindBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(86);
			yield return goodsOriginCodeFindBoxColumnStyleInfo;

			var messageStatusTextBoxColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			messageStatusTextBoxColumnStyleInfo.CaptionResourceString = Enterprise.Customs.SG.Access.GUI.Res.GetData("C3499F44-6C60-4B02-81F1-4B96BA6C4984", "Msg. Status");
			messageStatusTextBoxColumnStyleInfo.ColumnName = "PackedItem+API_MessageStatus";
			messageStatusTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			yield return messageStatusTextBoxColumnStyleInfo;

			var packStatusTextBoxColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			packStatusTextBoxColumnStyleInfo.CaptionResourceString = Enterprise.Customs.SG.Access.GUI.Res.GetData("920D5FD1-49F6-4F9D-9BA4-640D0F4D78A2", "Pack Status");
			packStatusTextBoxColumnStyleInfo.ColumnName = "PackedItem+API_PackStatus";
			packStatusTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65);
			yield return packStatusTextBoxColumnStyleInfo;

			var goodsTypeDropEditColumnStyleInfo = new ZDropEditColumnStyleInfo();
			goodsTypeDropEditColumnStyleInfo.CaptionResourceString = Enterprise.Customs.SG.Access.GUI.Res.GetData("54604AB7-FEE0-4B61-85A8-20B9CB042675", "Goods Type");
			goodsTypeDropEditColumnStyleInfo.ColumnName = "PackedItem+GoodsType";
			goodsTypeDropEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			yield return goodsTypeDropEditColumnStyleInfo;

			var gstPaidDropEditColumnStyleInfo = new ZDropEditColumnStyleInfo();
			gstPaidDropEditColumnStyleInfo.ColumnName = "PackedItem+GSTPaid";
			gstPaidDropEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			yield return gstPaidDropEditColumnStyleInfo;
		}

		protected override string[] GetPacksGridColumnsOrderCore()
		{
			var packedItemProperty = nameof(AsycudaPack.PackedItem) + "+";
			return new string[]
			{
				AsycudaPack.Schema.APA_PackQty,
				AsycudaPack.Schema.APA_PackUQ,
				AsycudaPack.Schema.APA_CommodityCode,
				AsycudaPack.Schema.APA_GoodsDescription,
				AsycudaPack.Schema.APA_MarksAndNumbers,
				AsycudaPack.Schema.APA_Weight,
				AsycudaPack.Schema.APA_WeightUQ,
				AsycudaPack.Schema.APA_Volume,
				AsycudaPack.Schema.APA_VolumeUQ,
				AsycudaPack.Schema.LinePrice,
				AsycudaPack.Schema.LinePriceCurrency,
				packedItemProperty + AsycudaPackedItem.Schema.API_GoodsDescription,
				packedItemProperty + AsycudaPackedItem.Schema.API_FormattedTariff,
				packedItemProperty + AsycudaPackedItem.Schema.API_CustomsQty,
				packedItemProperty + AsycudaPackedItem.Schema.API_CustomsUQ,
				packedItemProperty + AsycudaPackedItem.Schema.API_CustomsValue,
				packedItemProperty + AsycudaPackedItem.Schema.API_DutyAmount,
				packedItemProperty + AsycudaPackedItem.Schema.API_TaxAmount,
				packedItemProperty + AsycudaPackedItem.Schema.API_RN_NKGoodsOrigin,
				packedItemProperty + AsycudaPackedItem.Schema.API_MessageStatus,
				packedItemProperty + AsycudaPackedItem.Schema.API_PackStatus,
				packedItemProperty + AsycudaPackedItem.Schema.GoodsType,
				packedItemProperty + AsycudaPackedItem.Schema.GSTPaid,
			};
		}

		protected override IReadOnlyDictionary<bool, string[]> GetPacksGridColumnAvailabilityCore(ASYCUDA.Business.AsycudaManifestHeader header)
		{
			var sgHeader = (AsycudaManifestHeader)header;
			return new Dictionary<bool, string[]>()
			{
				{ sgHeader.IsOVRApplicable, new[] { nameof(AsycudaPack.PackedItem) + "+" + nameof(AsycudaPackedItem.GSTPaid) } }
			};
		}

		protected override IPanelLayoutProvider GetManifestLayoutCore()
		{
			return new SGManifestLayouts();
		}

		protected override IPanelLayoutProvider GetBillLayoutCore()
		{
			return new SGBillLayouts();
		}

		protected override IPanelLayoutProvider GetPackedItemDetailsLayoutCore()
		{
			return new SGPackedItemDetailsLayouts();
		}

		protected override IEnumerable<IAdditionalTabPage> GetBillAdditionalTabPageUserControlCore()
		{
			yield return new AsycudaPackUserControl();
		}
	}
}
