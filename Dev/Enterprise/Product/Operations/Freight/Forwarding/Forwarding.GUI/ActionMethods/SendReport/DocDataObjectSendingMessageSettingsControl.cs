using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class DocDataObjectSendingMessageSettingsControl : ZUserControl
	{
		public DocDataObjectSendingMessageSettingsControl(bool includeMessageErrors)
		{
			InitializeComponent();
			if (includeMessageErrors)
			{
				lblErrorAction.Text = Res.GetString("8243D74C-74AC-4C42-9167-AB8B71372378", "On Error / Message Error:");
			}
		}
	}
}
