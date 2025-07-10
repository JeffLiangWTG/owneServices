using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI.Organisation.UserControls.Address
{
	public partial class ContactControl : ZUserControl
	{
		public ContactControl()
		{
			InitializeComponent();
			WebLinkLabel.AllowOutsideOfParent();
			FaxLabel.AllowOverlap(PhoneLabel);
			WebLabel.AllowOutsideOfParent();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			contactDetailsForGUIBindingSource.DataSource = dataSource;
		}

		void WebLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			WebUrlLauncher.Launch(WebLinkLabel.Text);
		}
	}
}
