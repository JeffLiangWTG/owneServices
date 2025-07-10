using System.Windows.Forms;
using static CargoWise.Windows.UI.ControlDpiScalingHelper;

namespace Enterprise.Rating.GUI.RateSelector.UIControls
{
	public static class LayoutHelper
	{
		public static void RealignCenter(this Control controlToMove, Control container)
		{
			var xPosition = (container.Width - controlToMove.Width) / 2;
			controlToMove.Location = NewScaledPoint(xPosition, controlToMove.Location.Y, isInStandardDpi: false);
		}
	}
}
