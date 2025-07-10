namespace Enterprise.MasterFiles.Business
{
	public class GlbSecurityLookups : AutoGlbSecurityLookups
	{
		public GlbSecurityLookups(AutoGlbSecurity parent) : base(parent)
		{
		}

		GlbStaffCollection staff;
		public override GlbStaffCollection Staff
		{
			get
			{
				if (staff == null)
				{
					staff = base.Staff;
				}
				return staff;
			}
		}

		GlbBranchCollection branches;
		public override GlbBranchCollection Branches
		{
			get
			{
				if (branches == null)
				{
					branches = base.Branches;
				}
				return branches;
			}
		}

		GlbCompanyCollection companies;
		public override GlbCompanyCollection Companies
		{
			get
			{
				if (companies == null)
				{
					companies = base.Companies;
				}
				return companies;
			}
		}

		GlbDepartmentCollection departments;
		public override GlbDepartmentCollection Departments
		{
			get
			{
				if (departments == null)
				{
					departments = base.Departments;
				}
				return departments;
			}
		}

		#region StaffMembers

		public GlbStaffCollection StaffMembers
		{
			get
			{
				if (fStaffMembers == null)
				{
					fStaffMembers = new GlbStaffCollection(Factory);
				}
				return fStaffMembers;
			}
		}

		GlbStaffCollection fStaffMembers;

		#endregion
	}
}
