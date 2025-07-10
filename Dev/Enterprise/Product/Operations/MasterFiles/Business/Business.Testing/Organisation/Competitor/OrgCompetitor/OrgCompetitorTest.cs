using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgCompetitor))]
	public class OrgCompetitorTest : EnterpriseBusinessObjectTestCase
	{
		public void TestHumanReadableName()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var competitor = Factory.NewWithValidTestData<OrgHeader>();
			var orgCompetitor = org.Competitors.AddNew();
			orgCompetitor.OCP_OH_Competitor = competitor.PK;
			Factory.Save();

			AssertEquals("Competitor", orgCompetitor.HumanReadableName);
		}

		public void TestDescription()
		{
			var parent = Factory.NewWithValidTestData<OrgHeader>();
			var competitor = Factory.NewWithValidTestData<OrgHeader>();

			var orgCompetitor = parent.Competitors.AddNew();
			orgCompetitor.OCP_Type = CompetitorTypeList.Codes.Customs;
			orgCompetitor.OCP_OH_Competitor = competitor.PK;
			Factory.Save();

			AssertEquals(CompetitorTypeList.Descriptions.Customs, orgCompetitor.Description);
		}

		public void TestOrganisationName()
		{
			var parent = Factory.NewWithValidTestData<OrgHeader>();
			var competitor = Factory.NewWithValidTestData<OrgHeader>();
			competitor.OH_FullName = "Test Competitor";

			var orgCompetitor = parent.Competitors.AddNew();
			orgCompetitor.OCP_Type = CompetitorTypeList.Codes.Customs;
			orgCompetitor.OCP_OH_Competitor = competitor.PK;
			Factory.Save();

			AssertEquals(competitor.OH_FullName, orgCompetitor.OrganisationName);
		}

		public void TestSettingOrgCompetitorCompanyLevel()
		{
			var parent = Factory.NewWithValidTestData<OrgHeader>();
			var competitor = Factory.NewWithValidTestData<OrgHeader>();

			var orgCompetitor1 = parent.Competitors.AddNew();
			orgCompetitor1.OCP_Type = CompetitorTypeList.Codes.Customs;
			orgCompetitor1.OCP_OH_Competitor = competitor.PK;
			orgCompetitor1.CompanyLevel = "COM";

			var orgCompetitor2 = parent.Competitors.AddNew();
			orgCompetitor2.OCP_Type = CompetitorTypeList.Codes.Customs;
			orgCompetitor2.OCP_OH_Competitor = competitor.PK;
			orgCompetitor2.CompanyLevel = "ENT";

			Factory.Save();

			AssertEquals(GlbCompany.CurrentCompany.PK, orgCompetitor1.OCP_GC_Company);
			AssertEquals(ZGuid.Empty, orgCompetitor2.OCP_GC_Company);
		}
	}
}
