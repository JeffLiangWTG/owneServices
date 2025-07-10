using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.NO.GUI;

[CodeAlive("This class will be used in the next workflow")]
sealed class FTPSettingsCustomsControlBag : ControlBag
{
	FTPSettingsCustomsControlBag()
	{
		SendToCustomFolderTextBox = RegisterControl(nameof(FTPSettingsCustomsUserControl.SendToCustomFolderTextBox));
		ReceiveFromCustomFolderTextBox = RegisterControl(nameof(FTPSettingsCustomsUserControl.ReceiveFromCustomFolderTextBox));
	}

	public static FTPSettingsCustomsControlBag Instance => instance ??= new ();

	[ThreadStatic]
	static FTPSettingsCustomsControlBag instance;

	protected override Control CreateTemplate() => new FTPSettingsCustomsUserControl();

	public ControlReference SendToCustomFolderTextBox;
	public ControlReference ReceiveFromCustomFolderTextBox;
}
