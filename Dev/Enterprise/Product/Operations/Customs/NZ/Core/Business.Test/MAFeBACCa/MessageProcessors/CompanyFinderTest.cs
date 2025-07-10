using System;
using Enterprise.Customs.NZ.Registry;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NZ.Business.MAFeBACCa.MessageProcessors.Testing
{
	using CargoWise.EntityFramework.Testing;

	class CompanyFinderTest : TestCaseWithFactory
	{
		public void TestGetsCompanyIfBrokerageIDSame()
		{
			GlbBranch branch1 = Factory.NewWithValidTestData<GlbBranch>();
			AssertEquals("NZ", branch1.Company.GC_RN_NKCountryCode);
			GlbBranch branch2 = Factory.NewWithValidTestData<GlbBranch>();
			AssertEquals("NZ", branch2.Company.GC_RN_NKCountryCode);
			branch1.Company.GC_Code = "BBB";
			branch2.Company.GC_Code = "AAA";

			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(branch1.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, "01234567D");
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(branch2.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, "01234567D");
			Factory.Save();

			AssertNoExceptionThrown(() => new CompanyFinder());

			var finder = new CompanyFinder();
			AssertEquals(branch2.Company.PK, finder.FindFromBrokerageID("01234567D").PK);
		}

		public void TestGetsCompanyWhenBrokerageIDExists()
		{
			GlbBranch branch = Factory.NewWithValidTestData<GlbBranch>();
			AssertEquals("Precondition: branch.Company.GC_RN_NKCountryCode", "NZ", branch.Company.GC_RN_NKCountryCode);

			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(branch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, "01234567D");
			Factory.Save();

			CompanyFinder finder = new CompanyFinder();
			AssertEquals("finder.FindFromBrokerageID('01234567D')", branch.Company.PK, finder.FindFromBrokerageID("01234567D").PK);
		}

		public void TestReturnsNullWhenBrokerageIDDoesntExist()
		{
			GlbBranch branch = Factory.NewWithValidTestData<GlbBranch>();
			AssertEquals("Precondition: branch.Company.GC_RN_NKCountryCode", "NZ", branch.Company.GC_RN_NKCountryCode);

			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(branch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, "00121212Z");
			Factory.Save();

			CompanyFinder finder = new CompanyFinder();
			AssertNull("finder.FindFromBrokerageID('01234567D')", finder.FindFromBrokerageID("01234567D"));
		}
	}
}
