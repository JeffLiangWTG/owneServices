using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI
{
	class WrappedZOpenFileDialog : ZOpenFileDialog, IOpenFileDialog
	{
		string IOpenFileDialog.FileName
		{
			get => base.ForceLocalFile() ?? string.Empty;
			set => base.FileName = value;
		}
	}
}
