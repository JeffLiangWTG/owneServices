using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class RefComplianceCommodityAlertForm : ZTemplateForm
	{
		public RefComplianceCommodityAlertForm(RefComplianceCommodityAlert refComplianceCommodityAlert) : base(refComplianceCommodityAlert)
		{
			InitializeComponent();
		}

		protected override void SetReadOnlyIncludingChildren()
		{
			base.SetReadOnlyIncludingChildren();

			ComplianceCommodityAlertUserControl.SourceURLLinkLabel.Enabled = true;
		}

		protected override bool AllowNew => false;

		protected override bool SupportsEDocs => false;

		protected override bool ShowAuditTab => true;
	}
}
