using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class OrganisationFilterHelperTest : TestCase
	{
		public void TestGetSecondaryOrgTypes_StandardModuleType_ExcludeNone()
		{
			var organisationFilterHelper = new OrganisationFilterHelper();
			var codeDescriptionPairs = organisationFilterHelper.GetSecondaryOrgTypes(OrgModuleType.Standard).ToArray();
			Assert(codeDescriptionPairs.Length > 0);
			AssertCollectionNotContains(codeDescriptionPairs, o => o.Code.Equals(OrganisationSecondaryTypes.None));
		}

		public void TestGetSecondaryOrgTypes_ClientIntelligenceModuleType_ExcludeNone()
		{
			var organisationFilterHelper = new OrganisationFilterHelper();
			var codeDescriptionPairs = organisationFilterHelper.GetSecondaryOrgTypes(OrgModuleType.ClientIntelligence).ToArray();
			Assert(codeDescriptionPairs.Length > 0);
			AssertCollectionNotContains(codeDescriptionPairs, o => o.Code.Equals(OrganisationSecondaryTypes.None));
		}

		public void TestGetSecondaryOrgTypes_CompanyCampaignContactModuleType_IncludeNone()
		{
			var organisationFilterHelper = new OrganisationFilterHelper();
			var codeDescriptionPairs = organisationFilterHelper.GetSecondaryOrgTypes(OrgModuleType.CompanyCampaignContact).ToArray();
			AssertCollectionContains(codeDescriptionPairs, o => o.Code.Equals(OrganisationSecondaryTypes.None));
		}

		public void TestGetSecondaryOrgTypes_CompetitorIntelligenceModuleType_ExcludeNone()
		{
			var organisationFilterHelper = new OrganisationFilterHelper();
			var codeDescriptionPairs = organisationFilterHelper.GetSecondaryOrgTypes(OrgModuleType.CompetitorIntelligence).ToArray();
			Assert(codeDescriptionPairs.Length == 0);
		}
	}
}
