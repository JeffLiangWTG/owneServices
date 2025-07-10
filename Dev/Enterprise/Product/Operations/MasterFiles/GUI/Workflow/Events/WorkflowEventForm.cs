using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class WorkflowEventForm : ZTemplateForm
	{
		public WorkflowEventForm(StmEvent stmEvent) : base(stmEvent)
		{
			InitializeComponent();

			SetDataBinding(stmEvent, "");
		}

		#region Implementation

		protected new StmEvent BusinessEntity
		{
			get { return (StmEvent)base.DataSource; }
		}

		internal protected new bool AllowNew { get { return false; } }

		internal protected new bool SupportsEDocs { get { return false; } }

		protected override bool ShowAuditTab => true;

		#endregion
	}
}
