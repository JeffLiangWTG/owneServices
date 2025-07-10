using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class ProcessingPortCodeAndFilerFinderTest : TestCaseWithFactory
	{
		public void TestGetACEProcessingPortCodeFinder()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch1 = company.Branches.AddNew();
			branch1.GB_Code = "~B1";
			var branch2 = company.Branches.AddNew();
			branch2.GB_Code = "~B2";
			Factory.Save();

			USCustomsDataRegistry.Instance.ProcessingDistrictPortCode.SetValue(Guid.Empty, branch1.PK.ToGuid(), Guid.Empty, "3910");
			USCustomsDataRegistry.Instance.ProcessingDistrictPortCode.SetValue(Guid.Empty, branch2.PK.ToGuid(), Guid.Empty, "3911");
			AssertEquals(ZString.Empty, ProcessingPortCodeAndFilerFinder.GetProcessingPortCodeFromRegistry(null));
			AssertEquals("3910", ProcessingPortCodeAndFilerFinder.GetProcessingPortCodeFromRegistry(branch1));
			AssertEquals("3911", ProcessingPortCodeAndFilerFinder.GetProcessingPortCodeFromRegistry(branch2));
		}

		public void TestGetPrcessingPortCodeFromRegistryForCompany()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch1 = company.Branches.AddNew();
			branch1.GB_Code = "~B1";
			var branch2 = company.Branches.AddNew();
			branch2.GB_Code = "~B2";
			Factory.Save();

			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			var branch3 = company2.Branches.AddNew();
			branch3.GB_Code = "~B3";

			USCustomsDataRegistry.Instance.ProcessingDistrictPortCode.SetValue(Guid.Empty, branch2.PK.ToGuid(), Guid.Empty, "3911");
			AssertEquals(ZString.Empty, ProcessingPortCodeAndFilerFinder.GetPrcessingPortCodeFromRegistryForCompany(null));
			AssertEquals(ZString.Empty, ProcessingPortCodeAndFilerFinder.GetPrcessingPortCodeFromRegistryForCompany(company2));
			AssertEquals("3911", ProcessingPortCodeAndFilerFinder.GetPrcessingPortCodeFromRegistryForCompany(company));
		}
	}
}
