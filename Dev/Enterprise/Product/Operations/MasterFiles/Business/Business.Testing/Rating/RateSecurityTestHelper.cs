using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	public static class RateSecurityTestHelper
	{
		public static GetTwoRateSecurityGroupsResult GetTwoRateSecurityGroups(BusinessObjectFactory factory)
		{
			var result = new GetTwoRateSecurityGroupsResult();

			var list = new CodeDescriptionPairList();
			list.AddPair("ABC", "ABC Description");
			list.AddPair("XYZ", "XYZ Description");

			OrganisationsDataRegistry.Instance.RatesSecurity.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new ReadOnlyCodeDescriptionPairList(list));

			var group = factory.NewWithValidTestData<GlbGroup>();
			result.Staff = group.Staff.AddNew();
			result.Staff.GS_GB_HomeBranch = Env.CurrentBranch.PK;
			result.Staff.GS_LoginName = "chuck norris";
			result.Staff.GS_Code = "CN";

			var glbSecurity1 = factory.New<GlbSecurity>();
			glbSecurity1.GU_SecurityItemIsAllowed = false;
			glbSecurity1.GU_SecurityRight = Env.Security.RatesSecurity.Code + "ABC";
			glbSecurity1.GU_GS = result.Staff.PK;

			var glbSecurity2 = factory.New<GlbSecurity>();
			glbSecurity2.GU_SecurityItemIsAllowed = true;
			glbSecurity2.GU_SecurityRight = Env.Security.RatesSecurity.Code + "XYZ";
			glbSecurity2.GU_GS = result.Staff.PK;

			result.DeniedOrg = factory.NewWithValidTestData<OrgHeader>();
			result.DeniedOrg.CompanyData.OB_RateSecurityGroup = "ABC";

			result.AllowedOrg = factory.NewWithValidTestData<OrgHeader>();
			result.AllowedOrg.CompanyData.OB_RateSecurityGroup = "XYZ";

			factory.Save();

			using (Env.SetTemporaryUserContext(result.Staff.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				result.DeniedSecurity = Env.Security.FindCheckPoint(Env.Security.RatesSecurity.Code + "ABC");
				result.AllowedSecurity = Env.Security.FindCheckPoint(Env.Security.RatesSecurity.Code + "XYZ");
			}

			return result;
		}

		public static void CreateSecurityRight(BusinessObjectFactory factory, GlbStaff user, string code, bool allowed) =>
			CreateSecurityRight(factory, code, allowed, user: user);

		public static void CreateSecurityRight(BusinessObjectFactory factory, GlbGroup group, string code, bool allowed) =>
			CreateSecurityRight(factory, code, allowed, group: group);

		public static void CreateSecurityRight(BusinessObjectFactory factory, string code, bool allowed, GlbGroup group = null, GlbStaff user = null, GlbCompany company = null, GlbBranch branch = null, GlbDepartment department = null)
		{
			var loginSecurity = factory.New<GlbSecurity>();
			loginSecurity.GU_SecurityRight = code;
			loginSecurity.GU_SecurityItemIsAllowed = allowed;

			if (group != null)
			{
				loginSecurity.GU_GG = group.PK;
			}

			if (user != null)
			{
				loginSecurity.GU_GS = user.PK;
			}

			if (company != null)
			{
				loginSecurity.GU_GC = company.PK;
			}

			if (branch != null)
			{
				loginSecurity.GU_GB = branch.PK;
			}

			if (department != null)
			{
				loginSecurity.GU_GE = department.PK;
			}
		}

		public class GetTwoRateSecurityGroupsResult
		{
			public GlbStaff Staff { get; set; }
			public SecurityCheckpoint DeniedSecurity { get; set; }
			public SecurityCheckpoint AllowedSecurity { get; set; }
			public OrgHeader DeniedOrg { get; set; }
			public OrgHeader AllowedOrg { get; set; }
		}
	}
}
