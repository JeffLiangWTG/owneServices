using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using static CargoWise.Windows.UI.ControlDpiScalingHelper;

namespace Enterprise.Rating.GUI.RateSelector.UIControls
{
	public class SharedSizeGroup
	{
		readonly List<Control> includedControls = new List<Control>();

		public void Include(Control controlToInclude)
		{
			includedControls.Add(controlToInclude);
		}

		public void Exclude(Control controlToExclude)
		{
			includedControls.Remove(controlToExclude);
		}

		public void AdjustSizeInGroup()
		{
			var largest = includedControls.Select(c => c.Size.Width).Max();
			foreach (var p in includedControls)
			{
				p.AutoSize = false;
				var newSize = NewScaledSize(largest, p.MinimumSize.Height, isInStandardDpi: false);
				p.MinimumSize = newSize;
			}
		}
	}
}
