using Enterprise.Workflow.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Workflow.GUI
{
	public partial class EDIMessagePurposeForm : ZTemplateForm
	{
		EDIMessagePurposeForm()
		{
			InitializeComponent();
		}

		public override string FormCaption
		{
			get { return Res.GetString("56c28879-fe63-49d1-86bc-4a6e6d1e8ab2", "Purpose Code Form"); }
		}

		public EDIMessagePurposeForm(EDIMessagePurpose purpose)
			: base(purpose)
		{
			InitializeComponent();
		}

		protected override bool SupportsEDocs => false;
		protected override bool ShowNotesTab => false;
		protected override bool ShowAuditTab => true;
	}
}
