using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class StaffAssignmentsControl : ZUserControl
	{
		public StaffAssignmentsControl()
		{
			InitializeComponent();
		}

		#region Company Specific Columns

		internal void SetCompanySpecific(bool value)
		{
			Org.StaffAssignments.CompanySpecific = value;
		}

		internal OrgHeader Org
		{
			get { return (OrgHeader)((ZForm)FindForm()).BusinessEntity; }
		}

		#endregion
	}
}
