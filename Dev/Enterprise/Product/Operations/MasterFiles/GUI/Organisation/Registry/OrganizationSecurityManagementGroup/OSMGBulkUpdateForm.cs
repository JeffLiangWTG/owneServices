using System;
using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class OSMGBulkUpdateForm : ZChildForm
	{
		public OSMGBulkUpdateForm()
		{
			InitializeComponent();
		}

		public OSMGBulkUpdateForm(OSMGBulkUpdater osmgBulkUpdate)
			: base(osmgBulkUpdate)
		{
		}

		public override string FormVerb => string.Empty;

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (!DesignModeFinder.IsDesigning)
			{
				((OSMGBulkUpdater)this.BusinessEntity).BulkOrgSecurityGroupInfo.ValueChanged += BulkOrgSecurityGroupInfo_ValueChanged;
			}
		}

		void BulkOrgSecurityGroupInfo_ValueChanged(object sender, EventArgs e)
		{
			UpdateButton.Enabled = ((OSMGBulkUpdater)this.BusinessEntity).BulkOrgSecurityGroup.IsValid;
		}

		void UpdateButton_Click(object sender, EventArgs e)
		{
			if (Globals.Message.ShowConfirmation(
					Res.GetString("5d06a98b-b515-4c86-b5ab-adbc38fc877d", "All organizations will be attached to the selected OSMG group.\r\n\r\nThis action cannot be reversed and can only be repaired in batches of the search filter result limit via the Set Organization Security Access Group function or by attaching organizations in bulk within an OSMG."),
					Res.GetString("55af4b00-6195-4934-b941-8c78cf728e38", "Bulk Update OSMG"),
					Res.GetString("c49473fe-6941-4e3b-9db3-72cb48dcb05d", "Please type the following to continue:"),
					Res.GetString("7d409bdc-d6ee-492a-b7ea-83db75ccc795", "Yes"),
					MessageBoxIcon.Warning,
					ConfirmationMessageLayout.AllInOneLine
				) == DialogResult.OK)
			{
				var affectedOrgs = 0;
				using (var progressForm = new ProgressForm())
				{
					var cancel = false;
					progressForm.Status = Res.GetString("feaa162c-dee0-4ff7-9107-d04bf8157a91", "Updating organizations...");
					progressForm.ShowCancelButton = true;
					progressForm.ShowProgressBar = false;
					progressForm.Cancelled += (_, __) => { cancel = true; };
					progressForm.ShowModalTo(this);

					var updater = (OSMGBulkUpdater)BusinessEntity;
					updater.BulkUpdateEvent += (_, arg) =>
					{
						progressForm.SetStatusAndPercentComplete(Res.GetString("1a11e57a-5fc5-4603-8d84-95f6e5a19440", "Updating organizations... {0} organization(s) have been updated.", arg.ProcessedCount), 0);
						arg.Cancel = cancel;
					};

					affectedOrgs = updater.BulkUpdateOSMG();
				}

				if (Globals.Message.Show(
						Res.GetString("140d2411-658f-468b-8e2f-281c4b54ec0a", "{0} organization(s) have been updated.", affectedOrgs),
						Res.GetString("BB2F2528-66A4-4725-8A2B-B9070C803282", "Complete"),
						MessageBoxButtons.OK,
						MessageBoxIcon.Information
					) == DialogResult.OK)
				{
					DialogResult = DialogResult.Cancel;
					Close();
				}
			}
		}

		void CancelFormButton_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}
