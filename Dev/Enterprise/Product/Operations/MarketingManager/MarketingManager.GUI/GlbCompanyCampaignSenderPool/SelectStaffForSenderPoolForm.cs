using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using Enterprise.MarketingManager.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI
{
	sealed partial class SelectStaffForSenderPoolForm : ZChildForm
	{
		public SelectStaffForSenderPoolForm(GlbCompanyCampaignSenderPool senderPool)
			: base(senderPool)
		{
			if (senderPool == null)
			{
				throw new ArgumentNullException(nameof(senderPool));
			}
			InitializeComponent();

			if (!DesignModeFinder.IsDesigning)
			{
				BackColor = SystemDataRegistry.Instance.ColorTheme.TabBackgroundColor;
				DescriptionLabel.Text = ResString.GetMultilingualString("35d3e9c6-bba5-48be-9df1-ceeae4283ced", "Emails will be delivered randomly from the following pool of email senders. Enter the preferred staff email senders and the send ratio to control the amount sent from each sender.");
			}
			ActiveControl = CloseButton;
		}

		GlbCompanyCampaignSenderPool DataSourceSenderPool => (GlbCompanyCampaignSenderPool)DataSource;

		public override string FormVerb => string.Empty;

		protected override bool AllowNew => DataSourceSenderPool.ReadOnly;

		void CloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			DataSourceSenderPool.ValidateAll();
			if (DataSourceSenderPool.HasErrors())
			{
				Globals.Message.Show(ResString.GetMultilingualString("03531516-bcda-4a35-8d56-5302882610b5", "There are errors in Sender Pool. Please correct them first."), FormCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
				e.Cancel = true;
			}
			base.OnClosing(e);
		}
	}
}
