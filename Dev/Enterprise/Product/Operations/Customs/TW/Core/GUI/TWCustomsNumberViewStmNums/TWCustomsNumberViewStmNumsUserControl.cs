using System;
using Enterprise.Customs.TW.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TW.GUI
{
	public partial class TWCustomsNumberViewStmNumsUserControl : MasterFiles.GUI.CustomsNumberViewStmNumsCompanyUserControl
	{
		[Obsolete("Required by the designer on subclass. Please do not use.")]
		public TWCustomsNumberViewStmNumsUserControl()
		{
			InitializeComponent();
		}

		public TWCustomsNumberViewStmNumsUserControl(TWCustomsNumberViewStmNumsWrapperCollection collection)
			: base(collection)
		{
			InitializeComponent();
			AddColumnsToGrid();
		}

		void AddColumnsToGrid()
		{
			using (NumberRangesGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				NumberRangesGrid.RemoveFromAvailableColumns(TWCustomsNumberViewStmNumsWrapper.Schema.SN_FountainName);
				NumberRangesGrid.RemoveFromAvailableColumns(TWCustomsNumberViewStmNumsWrapper.Schema.SN_Type);
				NumberRangesGrid.RemoveFromAvailableColumns(TWCustomsNumberViewStmNumsWrapper.Schema.SN_TypeDescription);
				NumberRangesGrid.RemoveFromAvailableColumns(TWCustomsNumberViewStmNumsWrapper.Schema.SN_Count);
				NumberRangesGrid.RemoveFromAvailableColumns(TWCustomsNumberViewStmNumsWrapper.Schema.SN_MinimumValue);
				NumberRangesGrid.RemoveFromAvailableColumns(TWCustomsNumberViewStmNumsWrapper.Schema.SN_MaximumValue);
				NumberRangesGrid.RemoveFromAvailableColumns(TWCustomsNumberViewStmNumsWrapper.Schema.SN_ValueForDisplay);
				NumberRangesGrid.SetColumnVisible(false, TWCustomsNumberViewStmNumsWrapper.Schema.SN_AvailableNumbers);
				NumberRangesGrid.ColumnStyles.Add(new ZDropEditColumnStyleInfo()
				{
					ColumnName = TWCustomsNumberViewStmNumsWrapper.Schema.MessageType,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(94),
					IsMandatory = true
				});

				NumberRangesGrid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo()
				{
					ColumnName = TWCustomsNumberViewStmNumsWrapper.Schema.RangeType,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(84),
					IsMandatory = true
				});

				NumberRangesGrid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo()
				{
					ColumnName = TWCustomsNumberViewStmNumsWrapper.Schema.RangeTypeDescription,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(142)
				});

				NumberRangesGrid.ColumnStyles.Add(new ZCalcEditColumnStyleInfo()
				{
					ColumnName = TWCustomsNumberViewStmNumsWrapper.Schema.Count,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(84)
				});
				NumberRangesGrid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo()
				{
					ColumnName = TWCustomsNumberViewStmNumsWrapper.Schema.StartNumber,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90)
				});
				NumberRangesGrid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo()
				{
					ColumnName = TWCustomsNumberViewStmNumsWrapper.Schema.EndNumber,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90)
				});
				NumberRangesGrid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo()
				{
					ColumnName = TWCustomsNumberViewStmNumsWrapper.Schema.CurrentValue,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90)
				});

				NumberRangesGrid.ReOrderColumns(
					[
						TWCustomsNumberViewStmNumsWrapper.Schema.MessageType,
						TWCustomsNumberViewStmNumsWrapper.Schema.RangeType,
						TWCustomsNumberViewStmNumsWrapper.Schema.RangeTypeDescription,
						TWCustomsNumberViewStmNumsWrapper.Schema.StartNumber,
						TWCustomsNumberViewStmNumsWrapper.Schema.EndNumber,
						TWCustomsNumberViewStmNumsWrapper.Schema.CurrentValue,
						TWCustomsNumberViewStmNumsWrapper.Schema.Count,
						TWCustomsNumberViewStmNumsWrapper.Schema.SN_SystemCreateTimeUtc,
						TWCustomsNumberViewStmNumsWrapper.Schema.SN_OwnerForDisplay
					]);
			}
		}
	}
}
