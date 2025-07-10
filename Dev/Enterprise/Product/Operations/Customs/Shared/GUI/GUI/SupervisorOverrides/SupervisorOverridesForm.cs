using CargoWise.ComponentModel;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public partial class SupervisorOverridesForm : ZChildForm
	{
		public SupervisorOverridesForm(SupervisorOverrides supervisorOverrides)
			: base(supervisorOverrides)
		{
		}

		void DisallowButton_Click(object sender, System.EventArgs e)
		{
			DialogResult = System.Windows.Forms.DialogResult.Cancel;
		}

		void AllowButton_Click(object sender, System.EventArgs e)
		{
			BusinessEntity.RunPreSaveValidation();

			if (BusinessEntity.Notifications.HasErrors())
			{
				ShowErrorsDialog();
			}
			else
			{
				DialogResult = System.Windows.Forms.DialogResult.OK;
			}
		}
	}
}
