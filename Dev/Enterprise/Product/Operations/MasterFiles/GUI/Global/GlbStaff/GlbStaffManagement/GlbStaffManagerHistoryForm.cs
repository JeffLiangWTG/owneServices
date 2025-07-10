using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class GlbStaffManagerHistoryForm : ZEditForm
	{
		public GlbStaffManagerHistoryForm(GlbStaffManagerCollection managers)
			: base(managers)
		{
			Managers = managers;
			InitializeComponent();

			if (!Env.Security.StaffReportingManagerRolesHistoryEdit.IsAllowed)
			{
				staffManagementHistoryGrid.SetReadOnly(true);
			}
		}

		GlbStaffManagerCollection Managers { get; }

		#region Overrides

		protected override ContinueWithSave ValidateAndSave()
		{
			foreach (var manager in Managers)
			{
				manager.Validation.ValidateGSM_GS_Manager();

				if (manager.GSM_ManagerTypeInfo.HasWarning(manager.Validation.CannotShareRoleMessage))
				{
					Globals.Message.ShowError(Res.GetString("f4d3f516-ae69-405a-9192-c97348865151", "Roles that have not been flagged as Shared Role Allowed in the registry cannot have more than one manager for any given date. Please manually fix records which are displaying warnings about shared roles so that effective date ranges do not overlap."));
					return ContinueWithSave.No;
				}
			}

			return base.ValidateAndSave();
		}

		protected override void SaveToRecentItems()
		{
		}

		protected override bool AllowNew => false;

		#endregion
	}
}
