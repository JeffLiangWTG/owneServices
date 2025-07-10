using Enterprise.Workflow.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Workflow.GUI
{
	public partial class EDIMessageDeliveryContextForm : ZTemplateForm
	{
		EDIMessageDeliveryContextForm()
		{
			InitializeComponent();
		}

		public EDIMessageDeliveryContextForm(EDIMessageDeliveryContextSelector bizo)
			: base(bizo)
		{
			InitializeComponent();
		}

		public override string FormCaption => Res.GetString("21DFE5A5-90E7-4188-A938-D2906D29B197", "Additional Event Context");

		protected override bool SupportsEDocs => false;

		protected override bool ShowAuditTab => true;
	}
}
