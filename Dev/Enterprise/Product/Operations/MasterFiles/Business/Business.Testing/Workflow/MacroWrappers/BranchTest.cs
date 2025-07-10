using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Macros.Testing
{
	sealed class BranchTest : TestCaseWithFactory
	{
		public void TestBranch()
		{
			var glbBranch = Factory.New<GlbBranch>();
			glbBranch.GB_BranchName = "Dragon Breath Logistics";
			glbBranch.GB_City = "Alexandria";
			glbBranch.GB_Code = "DAU";
			glbBranch.GB_RL_NKHomePort = "AUSYD";
			glbBranch.GB_RN_NKCountryCode = "AU";

			var orgProxy = Factory.New<OrgHeader>();
			orgProxy.OH_Code = "AAA";
			orgProxy.OH_RL_NKClosestPort = "AUSYD";

			glbBranch.GB_OH_OrgProxy = orgProxy.PK;

			var glbCompany = Factory.New<GlbCompany>();

			var branch = new Branch(glbBranch, glbCompany);

			CombineAssertions(() =>
			{
				AssertEquals("Name", "Dragon Breath Logistics", branch.Name);
				AssertEquals("City", "Alexandria", branch.City);
				AssertEquals("PK", glbBranch.PK, branch.PK);
				AssertEquals("Code", "DAU", branch.Code);
				AssertEquals("HomePort.Code", "AUSYD", branch.HomePort.Code);
				AssertEquals("Country.Code", "AU", branch.Country.Code);
				AssertEquals("Organization.Code", "AAA", branch.Organization.Code);
				AssertEquals("PortCode", "AUSYD", branch.PortCode);
				AssertEquals("PortName", "Sydney", branch.PortName);
				AssertEquals("PortCountry", "Australia", branch.PortCountry);
			});
		}

		public void TestNullBranch()
		{
			var branch = new Branch(null, null);

			CombineAssertions(() =>
			{
				AssertNullOrEmpty("Name", branch.Name);
				AssertNullOrEmpty("City", branch.City);
				AssertEquals("PK", ZGuid.Empty, branch.PK);
				AssertNullOrEmpty("Code", branch.Code);
				AssertNotNull("HomePort", branch.HomePort);
				AssertNotNull("Country", branch.Country);
				AssertNotNull("Organization", branch.Organization);
				AssertNullOrEmpty("PortCode", branch.PortCode);
				AssertNullOrEmpty("PortName", branch.PortName);
				AssertNullOrEmpty("PortCountry", branch.PortCountry);
			});
		}
		public void TestNullBranchOrganizationFallsBackToCompany()
		{
			var company = Factory.New<GlbCompany>();
			var orgProxy = Factory.New<OrgHeader>();
			orgProxy.OH_FullName = "Australia Air Company";
			orgProxy.OH_Code = "AUSAIRSYD";
			orgProxy.OH_RL_NKClosestPort = "AUSYD";

			company.GC_OH_OrgProxy = orgProxy.PK;

			var branch = new Branch(null, company);

			CombineAssertions(() =>
			{
				AssertEquals("Organization.PK", orgProxy.PK, branch.Organization.PK);
				AssertEquals("Organization.Code", "AUSAIRSYD", branch.Organization.Code);
				AssertEquals("Organization.Name", "Australia Air Company", branch.Organization.Name);
			});
		}
	}
}
