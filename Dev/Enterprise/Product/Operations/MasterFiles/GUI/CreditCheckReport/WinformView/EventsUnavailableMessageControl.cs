using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class EventsUnavailableMessageControl : ZUserControl
	{
		public EventsUnavailableMessageControl()
		{
			InitializeComponent();
		}

		public void SetLoading(bool isLoading)
		{
			messageLabel.Visible = !isLoading;
			loadingPicContainer.Visible = isLoading;
		}
	}
}
