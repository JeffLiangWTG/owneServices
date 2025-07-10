using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.MasterFiles.Module
{
	[SuppressFormDesignerAnalysis]
	public partial class OrgTransactionsFilterControl : ZDateRangeControl
	{
		public OrgTransactionsFilterControl(ZFilterStrip parentStrip)
			: base(parentStrip)
		{
			InitializeComponent();
		}

		protected override void InitializeControls()
		{
			base.InitializeControls();
			ControlDpiScalingHelper.SetWidth(ref FromDateEdit, FromDateEdit.Width - ControlDpiScalingHelper.ScaleToCurrentDpiX(13), false);
			ControlDpiScalingHelper.SetWidth(ref ToDateEdit, ToDateEdit.Width - ControlDpiScalingHelper.ScaleToCurrentDpiX(13), false);
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
			}
		}
	}
}
