using System.Windows.Forms;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	partial class ChangingServiceTaskBranchForm : ZChildForm
	{
		public new BranchSwitcherBusinessObject BusinessEntity => (BranchSwitcherBusinessObject)base.BusinessEntity;

		public ChangingServiceTaskBranchForm()
		{
			InitializeComponent();
			ShowServiceTasks();
		}

		public ChangingServiceTaskBranchForm(BranchSwitcherBusinessObject branchSwitcher)
			: base(branchSwitcher)
		{
			InitializeComponent();
			ShowServiceTasks();
		}

		public override string FormVerb => "";

		void ShowServiceTasks()
		{
			var switchToNewServiceTasks = SystemDataRegistry.Instance.SwitchToNewServiceTasksModule.Value;

			gridServiceTasks.Visible = !switchToNewServiceTasks;
			gridStmServiceTasks.Visible = switchToNewServiceTasks;
		}

		void cancelButton_Click(object sender, System.EventArgs e)
		{
			BusinessEntity.CancelBranchChanges();
			DialogResult = DialogResult.Cancel;
		}

		void okButton_Click(object sender, System.EventArgs e)
		{
			BusinessEntity.RefreshValidationOnBranches();

			if (BusinessEntity.HasErrors)
			{
				ShowErrorsDialog();
			}
			else
			{
				DialogResult = DialogResult.OK;
			}
		}
	}
}
