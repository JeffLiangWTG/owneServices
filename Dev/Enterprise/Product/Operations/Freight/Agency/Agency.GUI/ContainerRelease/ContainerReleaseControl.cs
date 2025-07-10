using Enterprise.Freight.Agency.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Agency.GUI
{
	public partial class ContainerReleaseControl : ZUserControl
	{
		public ContainerReleaseControl()
		{
			InitializeComponent();
		}

		public void ExposeSendMessage(ReleaseHeader header)
		{
			if (header.SupportsMessageSending)
			{
				sendMessageCheckBox.Visible = true;
				sendMessageCheckBox.Text = header.IncludeMessageCaption;
			}
		}
	}
}
