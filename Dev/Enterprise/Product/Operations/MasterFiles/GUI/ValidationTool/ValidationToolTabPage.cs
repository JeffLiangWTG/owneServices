using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI;

sealed class ValidationToolTabPage : ZTabPage
{
	internal readonly ValidationToolTabPageUserControl ValidationToolTabPageUserControl;

	public ValidationToolTabPage()
	{
		ValidationToolTabPageUserControl = new ValidationToolTabPageUserControl();
		ValidationToolTabPageUserControl.Dock = DockStyle.Fill;
		ValidationToolTabPageUserControl.CaptionRenderingEnabled = true;
		Controls.Add(ValidationToolTabPageUserControl);
	}
}
