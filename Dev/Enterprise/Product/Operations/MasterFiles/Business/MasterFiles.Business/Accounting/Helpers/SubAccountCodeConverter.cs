using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public static class SubAccountCodeConverter
	{
		public static ZString ConvertSubClassCodeToSubAccountDBParentTableCode(ZString subClassCode)
		{
			switch (subClassCode)
			{
				case Core.Constants.SubAccountType.Organization:
					return OrgHeaderSchema.Constants.Prefix;
				case Core.Constants.SubAccountType.SalesGroup:
					return AccGroupsSchema.Constants.Prefix;
				case Core.Constants.SubAccountType.StaffAndResources:
					return GlbStaffSchema.Constants.Prefix;
				case Core.Constants.SubAccountType.StaffGroup:
					return GlbGroupSchema.Constants.Prefix;
				default:
					return subClassCode;
			}
		}

		public static ZString ConvertSubAccountDBParentTableCodeToSubClassCode(ZString dbTableCode)
		{
			switch (dbTableCode)
			{
				case OrgHeaderSchema.Constants.Prefix:
					return Core.Constants.SubAccountType.Organization;
				case AccGroupsSchema.Constants.Prefix:
					return Core.Constants.SubAccountType.SalesGroup;
				case GlbStaffSchema.Constants.Prefix:
					return Core.Constants.SubAccountType.StaffAndResources;
				case GlbGroupSchema.Constants.Prefix:
					return Core.Constants.SubAccountType.StaffGroup;
				default:
					return dbTableCode;
			}
		}
	}
}
