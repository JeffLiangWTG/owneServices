using System.Windows.Forms;
using CargoWise.BrandManager;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;

namespace Enterprise.Winzor.Architecture;

public class SplashForm : Form
{
	readonly SplashScreen splash;

	public SplashForm() : base(false)
	{
		FormBorderStyle = FormBorderStyle.None;
		splash = new SplashScreen();

		splash.Size = ControlDpiScalingHelper.NewScaledSize(512, 298);
		Controls.Add(splash);
		ClientSize = splash.Size;
		ControlBox = false;
		var mainBounds = CachedScreenInfo.Instance.BoundsInfos[0];
		Location = ControlDpiScalingHelper.NewScaledPoint(mainBounds.Left + mainBounds.Width / 2 - this.Width / 2, mainBounds.Top + mainBounds.Height / 2 - this.Height / 2, false);
		StartPosition = FormStartPosition.CenterScreen;
		Text = BrandingFactory.Instance.ProductBrandingName;
	}

	public void UpdateText(string text) => splash.UpdateMessage(text);

	public void UpdateProgress(int progress) => splash.UpdateProgress(progress);
}
