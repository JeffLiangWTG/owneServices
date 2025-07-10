using System;
using System.Collections.Generic;
using Enterprise.Core.Forms;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PE.Manifest.GUI
{
	public class ApplicationGUIProvider : ASYCUDA.GUI.ApplicationGUIProvider
	{
		public override Type ApplicationBusinessProviderType => typeof(Business.ApplicationBusinessProvider);

		public override ASYCUDA.GUI.MenuBuilder GetMenuBuilder(ZForm mainForm, AsycudaManifestHeader header) => new MenuBuilder(header, mainForm);

		protected override IPanelLayoutProvider GetBillLayoutCore() => new PEBillLayouts();

		protected override IPanelLayoutProvider GetManifestLayoutCore() => new PEManifestLayouts();

		protected override IPanelLayoutProvider GetBillPartiesLayoutCore() => new PEBillPartiesLayouts();

		protected override IEnumerable<ZGridColumnInfo> GetBillsGridExtraColumnInfosCore()
		{
			var billIssueDateEditInfo = new ZDateEditColumnStyleInfo();
			billIssueDateEditInfo.ColumnName = "ABL_BillIssueDate";
			billIssueDateEditInfo.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
			billIssueDateEditInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			yield return billIssueDateEditInfo;
		}

		protected override IReadOnlyDictionary<bool, string[]> GetBillsGridColumnAvailabilityCore(ASYCUDA.Business.AsycudaManifestHeader header)
		{
			return new Dictionary<bool, string[]>() { { false, new[] { AsycudaBill.Schema.ABL_BolType, } } };
		}

		protected override IEnumerable<IAdditionalTabPage> GetBillAdditionalTabPageUserControlCore()
		{
			yield return new AsycudaPackUserControl();
		}

		protected override string[] GetPacksGridColumnsOrderCore()
		{
			return new string[]
			{
				AsycudaPack.Schema.APA_LineNo,
				AsycudaPack.Schema.APA_PackQty,
				AsycudaPack.Schema.APA_PackUQ,
				AsycudaPack.Schema.APA_Weight,
				AsycudaPack.Schema.APA_WeightUQ,
				AsycudaPack.Schema.APA_Volume,
				AsycudaPack.Schema.APA_VolumeUQ,
				AsycudaPack.Schema.APA_GoodsDescription,
				AsycudaPack.Schema.ContainerPK,
				UNDGDataItemFormManager.ColumnNames.UNDGSubstanceManagerValue,
				UNDGDataItemFormManager.ColumnNames.UNDGClassManagerValue,
				AsycudaPack.Schema.LinePrice,
				AsycudaPack.Schema.LinePriceCurrency
			};
		}
	}
}
