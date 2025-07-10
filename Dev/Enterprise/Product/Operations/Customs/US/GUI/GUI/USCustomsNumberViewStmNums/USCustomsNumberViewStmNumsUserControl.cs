using System;
using System.Linq;
using CargoWise.Common;
using Enterprise.Core.Forms;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class USCustomsNumberViewStmNumsUserControl : MasterFiles.GUI.CustomsNumberViewStmNumsCompanyUserControl
	{
		[Obsolete("Required by the designer on subclass. Please do not use.")]
		public USCustomsNumberViewStmNumsUserControl()
		{
			InitializeComponent();
		}

		public USCustomsNumberViewStmNumsUserControl(USCustomsNumberViewStmNumsWrapperCollection collection)
			: base(collection)
		{
			InitializeComponent();
			AddColumnsToGrid();
		}

		void AddColumnsToGrid()
		{
			using (NumberRangesGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				NumberRangesGrid.RemoveFromAvailableColumns(USCustomsNumberViewStmNumsWrapper.Schema.SN_FountainName);
				NumberRangesGrid.SetColumnVisible(false, USCustomsNumberViewStmNumsWrapper.Schema.SN_TypeDescription);

				var checkDigitAdditionCalcEditColumnStyleInfo = new ZArchitecture.ZCalcEditColumnStyleInfo()
				{
					ColumnName = USCustomsNumberViewStmNumsWrapper.Schema.CheckDigitAddition,
					ShowGroupSeparators = false,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(62),
					Decimals = 0,
					MaxValue = 9
				};
				var appliesToTextBoxColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo()
				{
					CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
					ColumnName = USCustomsNumberViewStmNumsWrapper.Schema.AppliesTo,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(61)
				};
				NumberRangesGrid.SetColumnWidth(USCustomsNumberViewStmNumsWrapper.Schema.SN_Type, 43);
				NumberRangesGrid.SetColumnWidth(USCustomsNumberViewStmNumsWrapper.Schema.SN_Count, 60);
				NumberRangesGrid.SetColumnWidth(USCustomsNumberViewStmNumsWrapper.Schema.SN_MinimumValue, 74);
				NumberRangesGrid.SetColumnWidth(USCustomsNumberViewStmNumsWrapper.Schema.SN_MaximumValue, 74);
				NumberRangesGrid.ColumnStyles.AddRange(new ZGridColumnInfo[] { checkDigitAdditionCalcEditColumnStyleInfo, appliesToTextBoxColumnStyleInfo });

				NumberRangesGrid.ReOrderColumns(
					[
						USCustomsNumberViewStmNumsWrapper.Schema.IsBranchLevel,
						USCustomsNumberViewStmNumsWrapper.Schema.SN_OwnerForDisplay,
						USCustomsNumberViewStmNumsWrapper.Schema.SN_Type,
						USCustomsNumberViewStmNumsWrapper.Schema.SN_TypeDescription,
						USCustomsNumberViewStmNumsWrapper.Schema.AppliesTo,
						USCustomsNumberViewStmNumsWrapper.Schema.CheckDigitAddition,
						USCustomsNumberViewStmNumsWrapper.Schema.SN_ValueForDisplay,
						USCustomsNumberViewStmNumsWrapper.Schema.SN_AvailableNumbers,
						USCustomsNumberViewStmNumsWrapper.Schema.SN_MinimumValue,
						USCustomsNumberViewStmNumsWrapper.Schema.SN_Count,
						USCustomsNumberViewStmNumsWrapper.Schema.SN_MaximumValue,
						USCustomsNumberViewStmNumsWrapper.Schema.SN_SystemCreateTimeUtc
					]);
			}

			using (ThresholdRunOutWarningGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				ThresholdRunOutWarningGrid.SetColumnWidth(CustomsNumberStmNumberRange.Schema.Detail, 574);
				allocateBondeLineReleaseMenuItem = new ZMenuItem("Allocate &Border Line Release", AllocateBondeLineRelease);

				ThresholdRunOutWarningGrid.ContextMenu.MenuItems.Add(allocateBondeLineReleaseMenuItem);
				ThresholdRunOutWarningGrid.ContextMenu.Popup += ContextMenu_Popup;
			}
		}

		void ContextMenu_Popup(object sender, EventArgs e)
		{
			allocateBondeLineReleaseMenuItem.Visible = CurrentThresholdRunOutWarningWrapper?.IsCustomsEntry ?? false;
		}

		ZMenuItem allocateBondeLineReleaseMenuItem;

		USCustomsNumberViewStmNumsWrapper CurrentThresholdRunOutWarningWrapper => (USCustomsNumberViewStmNumsWrapper)CurrentThresholdRunOutWarning.GetStmNums().FirstOrDefault()?.Wrapper;

		void AllocateBondeLineRelease(object sender, EventArgs args)
		{
			if (CurrentThresholdRunOutWarningWrapper?.IsCustomsEntry ?? false)
			{
				var allocator = new BorderLineReleaseAllocator(CurrentThresholdRunOutWarningWrapper);
				if (allocator != null)
				{
					using (var form = new BorderLineReleaseAllocationForm(allocator))
					{
						ZFormModaliser.ShowDialogWithoutDispose(form);
					}
					CurrentThresholdRunOutWarning.Factory.InvalidateCachedProperties();
					CurrentThresholdRunOutWarning.GetStmNums().Select(x => x.Wrapper).ForEach(x => x.RefreshBinding());
					CurrentThresholdRunOutWarning.TotalAvailableNumbersInfo.RefreshBinding();
				}
			}
		}

		protected new USCustomsNumberViewStmNumsWrapper CurrentWrapper => (USCustomsNumberViewStmNumsWrapper)base.CurrentWrapper;
	}
}
