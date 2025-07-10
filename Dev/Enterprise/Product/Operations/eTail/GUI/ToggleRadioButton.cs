using System;
using CargoWise.Windows.UI;

namespace Enterprise.eTail.GUI
{
	public class ToggleRadioButton : KRadioButton
	{
		protected override void OnClick(EventArgs e)
		{
			var currentChecked = Checked;
			base.OnClick(e);
			if (AutoCheck)
			{
				Checked = !currentChecked;
			}
		}
	}
}
