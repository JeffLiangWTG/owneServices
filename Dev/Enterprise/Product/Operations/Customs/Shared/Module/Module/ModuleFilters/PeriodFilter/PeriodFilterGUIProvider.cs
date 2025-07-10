using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.Module
{
	public static class PeriodFilterGUIProvider
	{
		public static Control[] GetPeriodFilterControls(ZFilterStrip filterStripControl, ZBindingSource bindingSource)
		{
			ZPeriodUserControl control = new ZPeriodUserControl(filterStripControl);
			bindingSource.SetBindingMember(control, ".");
			ControlDpiScalingHelper.SetTop(ref control, filterStripControl.FilterControlTop, false);
			ControlDpiScalingHelper.SetLeft(ref control, ControlDpiScalingHelper.ScaleToCurrentDpiX(filterStripControl.FilterControlsBox1Start - ZFilterStrip.SpaceBetweenLabelAndControl) - control.OperatorDropEdit.Width, false);

			return new Control[] { control };
		}
	}
}
