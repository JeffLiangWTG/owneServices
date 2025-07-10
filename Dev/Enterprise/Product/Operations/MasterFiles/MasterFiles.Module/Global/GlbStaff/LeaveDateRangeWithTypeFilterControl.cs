using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.MasterFiles.Module
{
	internal class LeaveDateRangeWithTypeFilterControl : ZDateRangeControl
	{
		public LeaveDateRangeWithTypeFilterControl(ZFilterStrip parentStrip) : base(parentStrip)
		{
			Controls.Add(GetLeaveTypeDropEditControl(parentStrip));
		}

		ZFilterStripDropEdit GetLeaveTypeDropEditControl(ZFilterStrip filterStrip)
		{
			var leaveTypeDropEdit = new ZFilterStripDropEdit { CharacterCasing = System.Windows.Forms.CharacterCasing.Normal };
			ControlDpiScalingHelper.SetWidth(ref leaveTypeDropEdit, ZFilterStrip.DropListCodeBoxWidth, true);
			ControlDpiScalingHelper.SetWidth(leaveTypeDropEdit.Controls["CodeBox"], ZFilterStrip.DropListCodeBoxWidth, true);
			ControlDpiScalingHelper.SetTop(ref leaveTypeDropEdit, filterStrip.FilterControlTop, false);
#pragma warning disable CW1040 // Check for mixed arithmatic between scaled and unscaled components
			ControlDpiScalingHelper.SetLeft(ref leaveTypeDropEdit, filterStrip.FilterControlsBox1Start - ZFilterStrip.DropListCodeBoxWidth - 8, true);
#pragma warning restore CW1040 // Check for mixed arithmatic between scaled and unscaled components
			leaveTypeDropEdit.Name = "LeaveTypeDropEdit";
			leaveTypeDropEdit.MaxItemsToShowInDropDown = 28;
			leaveTypeDropEdit.ShowDescriptionBox = false;
			leaveTypeDropEdit.TabIndex = 0;
			leaveTypeDropEdit.BindTo = "WorkHolidayType";
			return leaveTypeDropEdit;
		}

		protected override void InitializeControls()
		{
			base.InitializeControls();
			ControlDpiScalingHelper.SetWidth(ref FromDateEdit, FromDateEdit.Width - ControlDpiScalingHelper.ScaleToCurrentDpiX(13), false);
			ControlDpiScalingHelper.SetWidth(ref ToDateEdit, ToDateEdit.Width - ControlDpiScalingHelper.ScaleToCurrentDpiX(13), false);
			ControlDpiScalingHelper.SetLeft(ref FromDateEdit, ControlDpiScalingHelper.ScaleToCurrentDpiX(ParentStrip.FilterControlsBox1Start + 4) + PropertySearchDropEdit.Width, false);
			ControlDpiScalingHelper.SetLeft(ref ToDateEdit, ParentStrip.FilterControlsBox2Start(ToDateEdit) + 0, true);
		}

		protected override void UpdatePropertySearchDropEditLayout()
		{
			base.UpdatePropertySearchDropEditLayout();
			if (ShowDateControls)
			{
				ControlDpiScalingHelper.SetLeft(ref PropertySearchDropEdit, ParentStrip.FilterControlsBox1Start, true);
				PropertySearchDropEdit.PreBoundMaxLength = ZFilterStrip.FilterComparisonOperatorBoxPreBoundMaxLength;
				ControlDpiScalingHelper.SetLeft(ref FromDateEdit, PropertySearchDropEdit.Right + ControlDpiScalingHelper.ScaleToCurrentDpiX(4), false);
				if (FromDateEdit.Right > ToDateEdit.Left)
				{
					int overlap = FromDateEdit.Right - ToDateEdit.Left;
					ControlDpiScalingHelper.SetWidth(FromDateEdit, FromDateEdit.Width - (overlap / 2 + ControlDpiScalingHelper.ScaleToCurrentDpiX(1)), false);
					ControlDpiScalingHelper.SetWidth(ToDateEdit, ToDateEdit.Width - (overlap / 2 + ControlDpiScalingHelper.ScaleToCurrentDpiX(1)), false);
					ControlDpiScalingHelper.SetLeft(ToDateEdit, ToDateEdit.Left + overlap / 2 + ControlDpiScalingHelper.ScaleToCurrentDpiX(2), false);
				}
			}
		}
	}
}
