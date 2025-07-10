using System;
using System.Collections.Generic;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core.Forms;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.NO.Manifest.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NO.Manifest.GUI;

sealed class ApplicationGUIProvider : ASYCUDA.GUI.ApplicationGUIProvider
{
	public override Type ApplicationBusinessProviderType => typeof(ApplicationBusinessProvider);

	public override ASYCUDA.GUI.MenuBuilder GetMenuBuilder(ZForm mainForm, ASYCUDA.Business.AsycudaManifestHeader header) => new MenuBuilder((AsycudaManifestHeader)header, mainForm);

	protected override IEnumerable<IAdditionalTabPage> GetHeaderAdditionalTabPageUserControlsCore(ASYCUDA.Business.AsycudaManifestHeader header)
	{
		yield return new HeaderPartiesUserControl();
	}

	protected override IEnumerable<IAdditionalTabPage> GetBillAdditionalTabPageUserControlCore()
	{
		yield return new AsycudaPackUserControl();
		yield return new PreviousDocumentsUserControl();
	}

	protected override IPanelLayoutProvider GetBillLayoutCore() => new BillLayout();

	protected override IPanelLayoutProvider GetBillPartiesLayoutCore() => new NOBillPartiesLayout();

	protected override IPanelLayoutProvider GetManifestLayoutCore() => new NOManifestLayout();

	protected override ResourceStringData MainTabPageGroupBoxNameCore => Res.GetData("5C64C5B0-0014-4A9B-A27F-844EB24FEFA2", "Transport");

	protected override string[] GetPacksGridColumnsOrderCore() => new string[]
	{
		ASYCUDA.Business.AsycudaPack.Schema.APA_PackQty,
	};

	protected override IEnumerable<ZGridColumnInfo> GetBillsGridExtraColumnInfosCore()
	{
		var customsLevelTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
		customsLevelTextBoxColumnStyleInfo.ColumnName = "CustomsLevel";
		customsLevelTextBoxColumnStyleInfo.IsReadOnly = true;
		customsLevelTextBoxColumnStyleInfo.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		customsLevelTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
		yield return customsLevelTextBoxColumnStyleInfo;
	}

	protected override IReadOnlyDictionary<string, bool> GetBillsGridColumnVisiblilityOnValueChangedCore(ASYCUDA.Business.AsycudaManifestHeader header)
	{
		return new Dictionary<string, bool>()
			{
				{ AsycudaBill.Schema.ABL_ConsigneeName, true },
				{ AsycudaBill.Schema.ABL_ConsigneeStreet1, true },
				{ AsycudaBill.Schema.ABL_ConsigneePostcode, true },
				{ AsycudaBill.Schema.ABL_ConsigneeCity, true },
			};
	}

	protected override IEnumerable<string> GetBillsGridColumnsOrderCore()
	{
		yield return AsycudaBill.Schema.ABL_SequenceNumber;
		yield return AsycudaBill.Schema.ABL_BillNumber;
		yield return AsycudaBill.Schema.ABL_BolType;
		yield return AsycudaBill.Schema.CustomsLevel;
		yield return AsycudaBill.Schema.ABL_RL_NKOrigin;
		yield return AsycudaBill.Schema.ABL_RL_NKFinalDestination;
		yield return AsycudaBill.Schema.ABL_ConsigneeName;
		yield return AsycudaBill.Schema.ABL_ConsigneeStreet1;
		yield return AsycudaBill.Schema.ABL_ConsigneePostcode;
		yield return AsycudaBill.Schema.ABL_ConsigneeCity;
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
	}
}
