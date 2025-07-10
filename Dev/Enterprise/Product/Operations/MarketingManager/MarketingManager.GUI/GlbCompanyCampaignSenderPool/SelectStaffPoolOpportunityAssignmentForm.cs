using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI
{
	sealed partial class SelectStaffPoolOpportunityAssignmentForm : ZChildForm
	{
		public SelectStaffPoolOpportunityAssignmentForm(GlbCompanyCampaignSenderPool senderPool) : base(senderPool)
		{
			if (senderPool == null)
			{
				throw new ArgumentNullException(nameof(senderPool));
			}

			InitializeComponent();
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
				Globals.Message.Show(Res.GetString("877AAC24-B4D3-4BB4-9FB5-77E295828F8B", "There are errors in Staff Assignment Pool. Please correct them first."), FormCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
				e.Cancel = true;
			}

			base.OnClosing(e);
		}
	}
}
