using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Agency.GUI
{
	public partial class ReleaseImportOrderControl : ZUserControl
	{
		public ReleaseImportOrderControl()
		{
			InitializeComponent();
			this.descriptionLabel.Text = Res.GetString("ReleaseImportOrderControl|DescriptionLabel", "Allow messages to be sent for containers that are still waiting for a response to the last message.");
		}
	}
}
