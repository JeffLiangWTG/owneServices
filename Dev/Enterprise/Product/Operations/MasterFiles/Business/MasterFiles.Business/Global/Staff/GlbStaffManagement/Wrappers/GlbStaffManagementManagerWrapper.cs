using System.Globalization;
using CargoWise.Types;
using Enterprise.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class GlbStaffManagementManagerWrapper : GlbStaffManagementTreeBizObjWrapperBase
	{
		public GlbStaffManagementManagerWrapper(GlbStaffManagementTreeModel treeModel, GlbStaffManager manager, bool isManager = false)
			: base(treeModel)
		{
			Manager = manager;
			IsManager = isManager;
		}

		#region Properties

		public GlbStaffManager Manager { get; }
		public readonly bool IsManager;

		public bool ViewAllowed
		{
			get
			{
				if (IsManager)
				{
					return Env.CurrentUserPK == Manager.GSM_GS_Staff || Env.Security.StaffViewOtherDirectReports.IsAllowed;
				}
				else
				{
					return Env.CurrentUserPK == Manager.GSM_GS_Staff && Env.Security.StaffViewOwnReportingManagerRoles.IsAllowed ||
						Env.CurrentUserPK == Manager.GSM_GS_Manager ||
						Env.Security.StaffViewOtherReportingManagerRoles.IsAllowed ||
						Env.Security.StaffReportingManagerRoles.IsAllowed;
				}
			}
		}

		#endregion

		#region Overrides

		#region Role

		ZString RoleName => IsManager ? Manager.Staff.GS_FullName : Manager.Manager.GS_FullName;
		ZString RoleCode => IsManager ? Manager.Staff.GS_Code : Manager.Manager.GS_Code;

		public override ZString Role => ViewAllowed ? (ZString)(RoleName + " (" + RoleCode + ")") : ViewDeniedMessage;

		#endregion

		#region JobTitle

		public override ZString JobTitle => ViewAllowed ? Manager.Manager.GS_Title : ViewDeniedMessage;

		#endregion

		#region EffectiveDate

		public override ZString EffectiveDate => ViewAllowed ? (ZString)Manager.GSM_EffectiveDate.ToShortDateString() : ViewDeniedMessage;

		#endregion

		#region Location

		public override ZString Branch => ViewAllowed ? GetBranchID(Manager.Manager.HomeBranch) : ViewDeniedMessage;

		#endregion

		public override GlbStaffManagementTreeBizObjWrapperBase[] Children
		{
			get
			{
				return System.Array.Empty<GlbStaffManagementTreeBizObjWrapperBase>();
			}
		}

		#endregion

		#region Methods

		internal ZString GetBranchID(GlbBranch homeBranch)
		{
			if (homeBranch == null)
			{
				return ZString.Empty;
			}

			return string.Format(CultureInfo.InvariantCulture, "{0} ({1})", homeBranch.GB_Code, homeBranch.GB_RN_NKCountryCode);
		}

		#endregion
	}
}
