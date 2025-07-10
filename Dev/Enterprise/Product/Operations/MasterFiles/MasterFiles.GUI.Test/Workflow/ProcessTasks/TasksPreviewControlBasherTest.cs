using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(TasksPreviewFormForBash))]
	sealed class TasksPreviewControlBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new TasksPreviewFormForBash(Factory.New<DummyWithWorkflow>());
		}

		#region Implementation

		class TasksPreviewFormForBash : ZForm
		{
			public TasksPreviewFormForBash(IWorkflowProvider dataSource)
				: base(dataSource)
			{
				ControllerID = DummyControllerIDs.Dummy;
			}

			protected override void InitializeComponent()
			{
				base.InitializeComponent();
				Size = ControlDpiScalingHelper.NewScaledSize(945, 580, true);

				Controls.Add(TasksPreviewControl);
				this.BindingSource.SetBindingMember(TasksPreviewControl, "WorkflowItems");
				this.CaptionRenderingEnabled = true;
			}

			TasksPreviewControl tasksPreviewControl;
			TasksPreviewControl TasksPreviewControl
			{
				get { return tasksPreviewControl ?? (tasksPreviewControl = new TasksPreviewControl()); }
			}
		}

		#endregion
	}
}
