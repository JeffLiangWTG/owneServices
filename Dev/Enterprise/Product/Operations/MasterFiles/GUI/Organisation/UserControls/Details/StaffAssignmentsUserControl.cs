using System;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class StaffAssignmentsUserControl : ZUserControl
	{
		public StaffAssignmentsUserControl()
		{
			InitializeComponent();
			ShowForAllCompaniesCheckBox.ReadOnlyChanged += ShowForAllCompaniesCheckBox_ReadOnlyChanged;
			if (!DesignModeFinder.IsDesigning && !Env.Security.OrgDetailsViewOtherCompanysStaffAssignments.IsAllowed)
			{
				ShowForAllCompaniesCheckBox.Checked = false;
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			ShowForAllCompaniesCheckBox.Checked = OrganisationRegistry.Instance.DefaultBehaviorOfShowForAllCompaniesCheckbox.Value && Env.Security.OrgDetailsViewOtherCompanysStaffAssignments.IsAllowed;
		}

		void ShowForAllCompaniesCheckBox_ReadOnlyChanged(object sender, EventArgs e)
		{
			ShowForAllCompaniesCheckBox.ReadOnly = false;
		}

		void ShowForAllCompaniesCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			if (ShowForAllCompaniesCheckBox.Checked && !Env.Security.OrgDetailsViewOtherCompanysStaffAssignments.IsAllowed)
			{
				ShowForAllCompaniesCheckBox.Checked = false;
				Env.Security.OrgDetailsViewOtherCompanysStaffAssignments.ShowError();
			}
			else
			{
				staffAssignmentsControl1.SetCompanySpecific(!ShowForAllCompaniesCheckBox.Checked);
			}
		}
	}
}
