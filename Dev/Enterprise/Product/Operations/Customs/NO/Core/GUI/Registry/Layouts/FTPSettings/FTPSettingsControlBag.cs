using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.NO.GUI;

[CodeAlive("This class will be used in the next workflow")]
sealed class FTPSettingsControlBag : ControlBag
{
	FTPSettingsControlBag()
	{
		UserNameTextBox = RegisterControl(nameof(FTPSettingsUserControl.UserNameTextBox));
		PasswordTextBox = RegisterControl(nameof(FTPSettingsUserControl.PasswordTextBox));
		ViewButton = RegisterControl(nameof(FTPSettingsUserControl.ViewButton));
		UrlAddressTextBox = RegisterControl(nameof(FTPSettingsUserControl.UrlAddressTextBox));
		PortTextBox = RegisterControl(nameof(FTPSettingsUserControl.PortTextBox));
	}

	public static FTPSettingsControlBag Instance => instance ??= new ();

	[ThreadStatic]
	static FTPSettingsControlBag instance;

	protected override Control CreateTemplate() => new FTPSettingsUserControl();

	public ControlReference UserNameTextBox;
	public ControlReference PasswordTextBox;
	public ControlReference ViewButton;
	public ControlReference UrlAddressTextBox;
	public ControlReference PortTextBox;
}
