using System.Drawing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	public partial class WorkflowExceptionTypeForm : ZTemplateForm
	{
		public WorkflowExceptionTypeForm(ProcessWorkflowExceptionType businessEntity) : base(businessEntity)
		{
			ControllerID = ControllerIDs.WorkflowExceptionTypes;
			InitializeComponent();
		}

		protected override bool SupportsEDocs => false;
		protected override bool ShowAuditTab => true;
		public override Size MinimumSize { get => CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1085, 660); set => base.MinimumSize = value; }
		public override Size MaximumSize { get => CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1085, 660); set => base.MaximumSize = value; }
	}
}
