using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.MasterFiles.Module
{
	[CargoWise.Windows.UI.Testing.SuppressFormDesignerAnalysis]
	public partial class WorkflowFilterStripControl : ZDateRangeControl
	{
		/// <summary>
		/// Only for VS designer.
		/// </summary>
		public WorkflowFilterStripControl()
		{
			InitializeComponent();
		}

		public WorkflowFilterStripControl(ZFilterStrip parentStrip) : base(parentStrip)
		{
			InitializeComponent();
		}

		protected override void InitializeControls()
		{
			base.InitializeControls();
			ControlDpiScalingHelper.SetWidth(ref FromDateEdit, 83, true);
			ControlDpiScalingHelper.SetWidth(ref ToDateEdit, 83, true);
			ControlDpiScalingHelper.SetLeft(ref FromDateEdit, ParentStrip.FilterControlsBox1Start + 105, true);
			ControlDpiScalingHelper.SetLeft(ref ToDateEdit, ParentStrip.FilterControlsBox2Start(ToDateEdit) + 0, true);
		}

		protected override void UpdatePropertySearchDropEditLayout()
		{
			base.UpdatePropertySearchDropEditLayout();
			if (ShowDateControls)
			{
				ControlDpiScalingHelper.SetLeft(ref PropertySearchDropEdit, ParentStrip.FilterControlsBox1Start, true);
				PropertySearchDropEdit.PreBoundMaxLength = ZFilterStrip.FilterComparisonOperatorBoxPreBoundMaxLength;

				if (Filter != null && Filter.IsPropertySearchUsingSpecifiedDateTimeRange)
				{
					ControlDpiScalingHelper.SetWidth(ref FromDateEdit, 114, true);
					ControlDpiScalingHelper.SetWidth(ref ToDateEdit, 114, true);

					ControlDpiScalingHelper.SetLeft(ref PropertySearchDropEdit, ParentStrip.FilterControlsBox1Start - 26, true);

					ControlDpiScalingHelper.SetLeft(ref FromDateEdit, 349, true);
					ControlDpiScalingHelper.SetLeft(ref ToDateEdit, 486, true);
				}
				else
				{
					ControlDpiScalingHelper.SetWidth(ref FromDateEdit, FromDateEdit.Width - ControlDpiScalingHelper.ScaleToCurrentDpiX(13), false);
					ControlDpiScalingHelper.SetWidth(ref ToDateEdit, ToDateEdit.Width - ControlDpiScalingHelper.ScaleToCurrentDpiX(13), false);
					ControlDpiScalingHelper.SetLeft(ref FromDateEdit, ParentStrip.FilterControlsBox1Start + 105, true);
					ControlDpiScalingHelper.SetLeft(ref ToDateEdit, ParentStrip.FilterControlsBox2Start(ToDateEdit) + 0, true);
				}
			}

			ControlDpiScalingHelper.SetTop(ref FromDateEdit, 1, true);
			ControlDpiScalingHelper.SetTop(ref ToDateEdit, 1, true);
		}
	}
}
