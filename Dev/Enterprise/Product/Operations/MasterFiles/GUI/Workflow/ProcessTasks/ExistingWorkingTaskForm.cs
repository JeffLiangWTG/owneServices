using System;
using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	public partial class ExistingWorkingTaskForm : ZChildForm
	{
		public ExistingWorkingTaskForm(ProcessTask task) : base(task)
		{
			InitializeComponent();

			this.InstructionsLabel.Text = Res.GetString("ExistingWorkingTaskForm|InstructionsLabel",
@"You have three choices:

    1. Select 'Continue' to suspend all above tasks, and continue updating your current task to working
    2. Choose one of the above tasks and select 'Work on Selected Task'
    3. Cancel and make no changes to any tasks.");

			this.HeadingLabel.Text = Res.GetString("ExistingWorkingTaskForm|HeadingLabel", "There is already a task in the system that this user is working on. You can only be working one task at a time. Details for this existing task, and all other suspended tasks, are shown below:");

			TasksGrid.AllowOverlap(CancelButtonX);
		}

		public override string FormVerb
		{
			get { return ""; }
		}

		new ProcessTask BusinessEntity
		{
			get { return (ProcessTask)base.BusinessEntity; }
		}

		#region Parent

		void OperationsJobButton_Click(object sender, EventArgs e)
		{
			if (SelectedTask != null)
			{
				if (SelectedTask.ParentControllerID != null &&
					SelectedTask.ParentBusinessObject != null)
				{
					ZController controller = ZControllerFactory.Create(SelectedTask.ParentControllerID);
#if DEBUG
					LastControllerForTest = controller;
#endif
					LastShownParentForm = controller.ShowEditForm(SelectedTask.ParentBusinessObject);
					WorkflowParentFormFactory.NavigateToWorkflowItem(LastShownParentForm, BusinessEntity);
				}
				else
				{
					Globals.Message.Show(Res.GetString("52846125-9637-4630-9fcb-1ede8812f699", "This is a stand-alone or template task."));
					LastShownParentForm = null;
				}
			}
		}

#if DEBUG
		internal ZController LastControllerForTest;
#endif

		IZForm LastShownParentForm;

		ProcessTask SelectedTask
		{
			get
			{
				return (ProcessTask)TasksGrid.ListManager.GetCurrent();
			}
		}

		#endregion

		#region Continue / Suspend

		void ContinueExistingWorkButton_Click(object sender, EventArgs e)
		{
			TaskToStart = SelectedTask;
			DialogResult = DialogResult.No;
			Close();
		}

		public ProcessTask TaskToStart
		{
			get { return fTaskToStart; }
			set { fTaskToStart = value; }
		}

		ProcessTask fTaskToStart;

		void SuspendExistingTaskButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Yes;
			Close();
		}

		void CancelButtonX_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}

		#endregion
	}
}
