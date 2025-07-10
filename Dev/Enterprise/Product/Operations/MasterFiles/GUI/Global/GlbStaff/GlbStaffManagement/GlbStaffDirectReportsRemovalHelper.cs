using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.GUI
{
	public static class GlbStaffDirectReportsRemovalHelper
	{
		public static bool TransferOrRemoveDirectReports(GlbStaff staff, GlbStaffPopupModuleHelper staffPopupModuleHelper, Form parentForm)
		{
			var directReportsGrouped = staff.GetCurrentDirectReports().Where(x => !x.IsFutureManager).GroupBy(x => x.GSM_ManagerType);
			foreach (var managerGrouping in directReportsGrouped)
			{
				if (managerGrouping.Count() == 1)
				{
					var singleManagerRecord = managerGrouping.FirstOrDefault();
					if (singleManagerRecord.SelfManaged)
					{
						singleManagerRecord.GSM_EndDate = ZDateTime.Today;
						continue;
					}
				}

				var role = (StaffReportingRole)SystemDataRegistry.Instance.StaffReportingRoles.Value.FindByCode(managerGrouping.Key);
				GlbStaff replacementManager = null;

				if (role == null)
				{
					foreach (var manager in managerGrouping)
					{
						manager.GSM_EndDate = ZDateTime.Today;
					}
					continue;
				}

				if (role.IsMandatory)
				{
					Globals.Message.Show(Res.GetString("0e06dfec-eee4-4737-b902-34cfb2118e9e", "Deactivating this staff member requires a new manager to take over their Direct Reports for the Mandatory Reporting Role of {0} (effective from tomorrow).", role.Description));
					replacementManager = staffPopupModuleHelper.GetStaffFromPopupModule(staff.Factory, parentForm);

					if (replacementManager == null)
					{
						return false;
					}
				}
				else
				{
					if (Globals.Message.Show(Res.GetString("2a49eb14-8889-47fe-bf5c-02831c2db41a", "Would you like to transfer this staff member's Direct Reports for the Reporting Role {0} to a new manager (effective from tomorrow)?", role.Description), Res.GetString("8e814096-c604-43ca-902e-2663a92e8323", "Transfer Responsibilities"), MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.Yes)
					{
						replacementManager = staffPopupModuleHelper.GetStaffFromPopupModule(staff.Factory, parentForm);
					}
				}

				if (replacementManager != null && replacementManager == staff)
				{
					Globals.Message.ShowError(Res.GetString("dce60c75-3f88-4306-aedb-6de572330d23", "You cannot replace the current manager with themselves."));
					return false;
				}

				foreach (var manager in managerGrouping)
				{
					manager.GSM_EndDate = ZDateTime.Today;

					if (replacementManager != null && !manager.SelfManaged && !manager.Staff.IsCurrentlyManagedBy(replacementManager, manager.GSM_ManagerType))
					{
						var newManager = staff.Factory.New<GlbStaffManager>();
						newManager.GSM_GS_Staff = manager.GSM_GS_Staff;
						newManager.GSM_GS_Manager = replacementManager.PK;
						newManager.GSM_ManagerType = manager.GSM_ManagerType;
						newManager.GSM_EffectiveDate = ZDateTime.Today.AddDays(1);
					}
				}
			}

			staff.GetFutureDirectReports().ForEach(x => x.Delete());

			return true;
		}
	}
}
