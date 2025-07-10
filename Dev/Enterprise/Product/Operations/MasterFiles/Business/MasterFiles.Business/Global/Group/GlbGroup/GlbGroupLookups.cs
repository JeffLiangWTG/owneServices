using CargoWise.Application;
using CargoWise.Integration;
using Enterprise.Integration;
using Enterprise.Registry.Business;

namespace Enterprise.MasterFiles.Business
{
	public class GlbGroupLookups : AutoGlbGroupLookups
	{
		public GlbGroupLookups(AutoGlbGroup parent) : base(parent)
		{
		}

		#region Complete Staff List

		public GlbStaffCollection CompleteStaffList
		{
			get
			{
				if (fCompleteStaffList == null)
				{
					fCompleteStaffList = new GlbStaffCollection(Factory);
				}
				return fCompleteStaffList;
			}
		}

		GlbStaffCollection fCompleteStaffList;

		#endregion

		#region Complete Organisation List

		public OrgHeaderCollection CompleteOrganisationList
		{
			get
			{
				if (fCompleteOrganisationList == null)
				{
					fCompleteOrganisationList = new OrgHeaderCollectionForGlbGroupLookup(Factory);
				}
				return fCompleteOrganisationList;
			}
		}

		OrgHeaderCollection fCompleteOrganisationList;

		#endregion

		#region Complete Group List

		GlbGroupCollection completeGroupList;

		public GlbGroupCollection CompleteGroupList
		{
			get { return completeGroupList ?? (completeGroupList = new GlbGroupCollection(Factory)); }
		}

		#endregion

		#region CategoryList

		public ICodeDescriptionPairList CategoryList => SystemDataRegistry.Instance.GroupCategoryList.Value;

		#endregion

		#region Branches

		public GlbBranchCollection Branches
		{
			get
			{
				if (fBranches == null)
				{
					fBranches = new GlbBranchCollection(Factory);
				}
				return fBranches;
			}
		}

		GlbBranchCollection fBranches;

		#endregion

		#region Departments

		public GlbDepartmentCollection Departments
		{
			get
			{
				if (fDepartments == null)
				{
					fDepartments = new GlbDepartmentCollection(Factory);
				}
				return fDepartments;
			}
		}

		GlbDepartmentCollection fDepartments;

		#endregion

		#region DomainNames

		public ICodeDescriptionPairList DomainNames => ObjectFactory.Get<IADRegistry>().DomainCredentialsCollectionAsCodeDescriptionPairList;

		#endregion

		#region Types

		public ICodeDescriptionPairList Types => GetTypes();

		protected virtual ICodeDescriptionPairList GetTypes()
		{
			return new GlbGroupTypeList();
		}

		#endregion
	}
}
