using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class CompanyCredentialsUserControl : ZUserControl
	{
		public CompanyCredentialsUserControl()
		{
			InitializeComponent();
		}

		public void SetCompanyCredentialsDetailsLayout(IPanelLayoutProvider layout)
		{
			DynamicLayoutPanel = new DynamicLayoutPanel();
			DynamicLayoutPanel.AutoScroll = true;
			DynamicLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			Controls.Remove(ICS2CredentialUserControl);
			Controls.Add(DynamicLayoutPanel);

			DynamicLayoutPanel.UpdateLayout(layout);
			DynamicLayoutPanel.PerformLayout();
		}
	}
}
