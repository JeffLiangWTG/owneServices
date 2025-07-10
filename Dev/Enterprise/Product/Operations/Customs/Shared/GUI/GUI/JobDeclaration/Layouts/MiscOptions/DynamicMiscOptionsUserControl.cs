using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public sealed partial class DynamicMiscOptionsUserControl : BaseCustomsEntryUserControl
	{
		public DynamicMiscOptionsUserControl()
		{
			InitializeComponent();
		}

		public void SetMiscOptionsLayout(IPanelLayoutProvider miscOptionsPanelLayout)
		{
			DynamicMiscOptionsPanel.UpdateLayout(miscOptionsPanelLayout);
		}
	}
}
