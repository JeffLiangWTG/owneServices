using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgCompetitorCollection))]
	class OrgCompetitorCollectionTest : ActiveBusinessObjectCollectionTestCase<OrgCompetitorCollection>
	{
		public void TestDefaultValues()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var competitorCollection = new OrgCompetitorCollection(org);
			var orgCompetitor = competitorCollection.AddNew();

			AssertEquals(org.PK, orgCompetitor.OCP_OH_Parent);
		}

		public void TestCompanyLevelFilter()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var competitor = Factory.NewWithValidTestData<OrgHeader>();

			var orgCompetitor1 = Factory.NewWithValidTestData<OrgCompetitor>();
			orgCompetitor1.OCP_OH_Parent = org.PK;
			orgCompetitor1.OCP_OH_Competitor = competitor.PK;
			orgCompetitor1.CompanyLevel = "ENT";

			var orgCompetitor2 = Factory.NewWithValidTestData<OrgCompetitor>();
			orgCompetitor2.OCP_OH_Parent = org.PK;
			orgCompetitor2.OCP_OH_Competitor = competitor.PK;
			orgCompetitor2.CompanyLevel = "COM";

			var oldCompanyPK = GlbCompany.CurrentCompany.PK;
			var companyA = Factory.NewWithValidTestData<GlbCompany>();
			var branchA = Factory.NewWithValidTestData<GlbBranch>();
			branchA.GB_GC = companyA.PK;

			Factory.Save();

			var competitorCollectionOldCompany = new OrgCompetitorCollection(org);
			AssertEquals("Both competitors can be seen in old company", 2, competitorCollectionOldCompany.Count);

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branchA.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				AssertNotEquals(oldCompanyPK, GlbCompany.CurrentCompany.PK);
				var competitorCollectionNewCompany = new OrgCompetitorCollection(org);
				AssertEquals("Only CompanyLevel ENT competitor can be seen in another company", 1, competitorCollectionNewCompany.Count);
				AssertEquals("Only CompanyLevel ENT competitor can be seen in another company", orgCompetitor1.PK, competitorCollectionNewCompany[0].PK);
			}
		}

		protected override OrgCompetitorCollection GetCollectionToTest()
		{
			return new OrgCompetitorCollection(Factory.NewWithValidTestData<OrgHeader>());
		}
	}
}
