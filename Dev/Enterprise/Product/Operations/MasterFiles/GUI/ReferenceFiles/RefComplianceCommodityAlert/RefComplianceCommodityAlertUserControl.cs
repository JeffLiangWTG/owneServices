using System.ComponentModel;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class RefComplianceCommodityAlertUserControl : ZUserControl
	{
		public RefComplianceCommodityAlertUserControl()
		{
			InitializeComponent();
#if DEBUG
			TypeDescriptor.AddAttributes(CautionCaptionLabel, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(SourceURLLinkLabel, new SuppressFormsLocalizedTestAttribute());
#endif
		}

			void SourceURLLinkLabel_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			WebUrlLauncher.Launch(SourceURLLinkLabel.Text);
		}
	}
}
