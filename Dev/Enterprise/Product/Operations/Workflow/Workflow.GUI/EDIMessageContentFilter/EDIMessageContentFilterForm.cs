using Enterprise.Workflow.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Workflow.GUI
{
	public partial class EDIMessageContentFilterForm : ZTemplateForm
	{
		EDIMessageContentFilterForm()
		{
			InitializeComponent();
		}

		public override string FormCaption
		{
			get { return Res.GetString("cb5cb916-22ff-4177-9d06-1d0af4a07cec", "EDI Message Profile Form"); }
		}

		public EDIMessageContentFilterForm(EDIMessageContentFilter filter)
			: base(filter)
		{
			InitializeComponent();
		}

		protected override bool SupportsEDocs => false;
		protected override bool ShowNotesTab => false;
		protected override bool ShowAuditTab => true;
	}
}
