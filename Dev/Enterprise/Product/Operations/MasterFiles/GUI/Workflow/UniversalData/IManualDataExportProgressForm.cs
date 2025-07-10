using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public interface IManualDataExportProgressForm : IZForm
	{
		ZForm Form { get; }

		ZLabel TitleLabel { get; }
		ZTextBox NotificationsTextBox { get; }

		ZButton SendButton { get; }
		ZButton CloseButton { get; }
	}
}
