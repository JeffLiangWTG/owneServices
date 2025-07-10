using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.Module
{
	public partial class DGClassDGSubstanceFilterControl : ZUserControl
	{
		public DGClassDGSubstanceFilterControl(ZFilterStrip filterStripControl)
		{
			InitializeComponent();
			ControlDpiScalingHelper.SetLeft(ref operatorDropEdit, ControlDpiScalingHelper.ScaleToCurrentDpiX(filterStripControl.FilterControlsBox1Start - ZFilterStrip.SpaceBetweenLabelAndControl) - operatorDropEdit.Width, false);
			ControlDpiScalingHelper.SetLeft(ref dgClassDropEdit, operatorDropEdit.Right + ControlDpiScalingHelper.ScaleToCurrentDpiX(4), false);
			this.operatorDropEdit.CodeBox.Font = filterStripControl.ComparisonOperatorFont;
		}

		public void RestrictDGSubstanceFindBoxResize()
		{
			dgSubstanceFindBox.ShouldResize = false;
		}
	}
}
