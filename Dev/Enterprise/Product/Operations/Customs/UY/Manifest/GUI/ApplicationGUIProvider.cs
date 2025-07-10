using System;
using System.Collections.Generic;
using Enterprise.Core.Forms;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.UY.Manifest.GUI
{
	public class ApplicationGUIProvider : ASYCUDA.GUI.ApplicationGUIProvider
	{
		public override Type ApplicationBusinessProviderType => typeof(Business.ApplicationBusinessProvider);

		public override ASYCUDA.GUI.MenuBuilder GetMenuBuilder(ZForm mainForm, AsycudaManifestHeader header) => new MenuBuilder(header, mainForm);

		protected override IPanelLayoutProvider GetBillLayoutCore() => new UYBillLayouts();

		protected override IPanelLayoutProvider GetBillPartiesLayoutCore() => new UYBillPartiesLayouts();

		protected override IEnumerable<IAdditionalTabPage> GetBillAdditionalTabPageUserControlCore()
		{
			yield return new AsycudaPackUserControl();
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
				AsycudaPack.Schema.APA_Weight,
				AsycudaPack.Schema.APA_WeightUQ,
				AsycudaPack.Schema.APA_Volume,
				AsycudaPack.Schema.APA_VolumeUQ,
				Business.AsycudaPack.Schema.APA_ArrivedQuantity,
				Business.AsycudaPack.Schema.APA_ArrivedWeight
			};
		}

		protected override IEnumerable<ZGridColumnInfo> GetPacksGridExtraColumnInfosCore()
		{
			var arrivedQtyCalcEditColumnStyleInfo = new ZCalcEditColumnStyleInfo();
			arrivedQtyCalcEditColumnStyleInfo.BindToDecimalPlaces = null;
			arrivedQtyCalcEditColumnStyleInfo.ColumnName = "APA_ArrivedQuantity";
			arrivedQtyCalcEditColumnStyleInfo.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
			arrivedQtyCalcEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(117);
			yield return arrivedQtyCalcEditColumnStyleInfo;

			var arrivedWeightDropEditColumnStyleInfo = new ZCalcEditColumnStyleInfo();
			arrivedWeightDropEditColumnStyleInfo.BindToDecimalPlaces = null;
			arrivedWeightDropEditColumnStyleInfo.ColumnName = "APA_ArrivedWeight";
			arrivedWeightDropEditColumnStyleInfo.Decimals = 3;
			arrivedWeightDropEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			yield return arrivedWeightDropEditColumnStyleInfo;
		}

		protected override IPanelLayoutProvider GetManifestLayoutCore() => new UYManifestLayouts();

		protected override IReadOnlyDictionary<bool, string[]> GetBillsGridColumnAvailabilityCore(AsycudaManifestHeader header)
		{
			return new Dictionary<bool, string[]>() { { false, new[] { AsycudaBill.Schema.ABL_BolType, } } };
		}
	}
}
