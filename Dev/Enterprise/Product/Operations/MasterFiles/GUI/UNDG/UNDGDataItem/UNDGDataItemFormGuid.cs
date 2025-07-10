using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class UNDGDataItemFormGuid : ZChildForm
	{
		public UNDGDataItemFormGuid(IUNDGDataItemProvider uNDGDataItemProvider, IEnumerable<ZGridColumnInfo> additionalColumnInfos = null)
			: base((BusinessObject)uNDGDataItemProvider)
		{
			InitializeComponent();
			AddAdditionalColumns(additionalColumnInfos);
		}

		IEnumerable<ZGridColumnInfo> CFRColumns
		{
			get
			{
				var dangerousGoods49CfrGroupName = Res.GetData("aa9c087a-d703-422d-a897-8b5347066448", "Dangerous Goods 49 CFR");

				yield return new ZDateEditColumnStyleInfo()
				{
					ColumnName = nameof(UNDGDataItem.DI_SpecialPermitIssueDate),
					GroupName = dangerousGoods49CfrGroupName,
					DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short,
					Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(160)
				};

				yield return new ZTextBoxColumnStyleInfo()
				{
					ColumnName = nameof(UNDGDataItem.DI_SpecialPermitNumber),
					GroupName = dangerousGoods49CfrGroupName,
					Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(160)
				};

				yield return new ZTextBoxColumnStyleInfo()
				{
					ColumnName = nameof(UNDGDataItem.DI_HazardousWasteCode),
					GroupName = dangerousGoods49CfrGroupName,
					Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(160)
				};

				yield return new ZCheckBoxColumnStyleInfo()
				{
					ColumnName = nameof(UNDGDataItem.DI_IsSalvagePackaging),
					GroupName = dangerousGoods49CfrGroupName,
					Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(160)
				};

				yield return new ZCheckBoxColumnStyleInfo()
				{
					ColumnName = nameof(UNDGDataItem.DI_IsResidueLastContained),
					GroupName = dangerousGoods49CfrGroupName,
					Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(160)
				};
			}
		}

		IEnumerable<ZGridColumnInfo> RadioactiveColumns
		{
			get
			{
				var radioactiveGroupName = Res.GetData("1abbe910-4a89-b5a6-47f9-acdea90b5761", "Radioactive");

				yield return new ZDropEditColumnStyleInfo()
				{
					ColumnName = nameof(UNDGDataItem.DI_RadioactiveLabelCategory),
					GroupName = radioactiveGroupName,
					Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(160)
				};

				yield return new ZCalcEditColumnStyleInfo()
				{
					ColumnName = nameof(UNDGDataItem.DI_RadioactiveTransportIndex),
					GroupName = radioactiveGroupName,
					Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(110)
				};

				yield return new ZCheckBoxColumnStyleInfo()
				{
					ColumnName = nameof(UNDGDataItem.DI_IsHighwayRouteControlledQuantity),
					GroupName = radioactiveGroupName,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160)
				};

				yield return new ZCheckBoxColumnStyleInfo()
				{
					ColumnName = nameof(UNDGDataItem.DI_IsExclusiveUse),
					GroupName = radioactiveGroupName,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160)
				};

				yield return new ZTextBoxColumnStyleInfo()
				{
					ColumnName = nameof(UNDGDataItem.DI_MaterialFormDescription),
					GroupName = radioactiveGroupName,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160)
				};

				yield return new ZCheckBoxColumnStyleInfo()
				{
					ColumnName = nameof(UNDGDataItem.DI_IsFissileExcepted),
					GroupName = radioactiveGroupName,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160)
				};

				yield return new ZDropEditColumnStyleInfo()
				{
					ColumnName = nameof(UNDGDataItem.DI_RadionuclideElement),
					GroupName = radioactiveGroupName,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60)
				};

				yield return new ZDropEditColumnStyleInfo()
				{
					ColumnName = nameof(UNDGDataItem.DI_RadionuclideElementSuffix),
					GroupName = radioactiveGroupName,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60)
				};

				yield return new ZCalcEditColumnStyleInfo()
				{
					ColumnName = nameof(UNDGDataItem.DI_RadioactiveMaximumActivity),
					GroupName = radioactiveGroupName,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60)
				};

				yield return new ZDropEditColumnStyleInfo()
				{
					ColumnName = nameof(UNDGDataItem.DI_RadioactiveMaximumActivityUnit),
					GroupName = radioactiveGroupName,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60)
				};
			}
		}

		void CloseButton_Click(object sender, System.EventArgs e)
		{
			bool errors = false;

			foreach (BusinessObject bizo in this.zGrid1.List)
			{
				bizo.RunPreSaveValidation();
				if (bizo.HasErrors())
				{
					errors = true;
				}
			}

			if (errors)
			{
				ShowErrorsDialog();
			}
			else
			{
				Close();
			}
		}

		void AddAdditionalColumns(IEnumerable<ZGridColumnInfo> additionalColumnInfos)
		{
			if (!DesignMode)
			{
				if (additionalColumnInfos != null)
				{
					zGrid1.ColumnStyles.AddRange(additionalColumnInfos.ToArray());
				}

				zGrid1.ColumnStyles.AddRange(CFRColumns.ToArray());
				zGrid1.ColumnStyles.AddRange(RadioactiveColumns.ToArray());
			}
		}

		public override string FormVerb
		{
			get { return string.Empty; }
		}
	}
}
