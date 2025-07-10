using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	sealed class BaseCusInBondHeaderTest : TestCaseWithFactory
	{
		[TestDate(2020, 8, 4)]
		public void TestGetBusinessObjectBaseTypeFromTablePrefix()
		{
			AssertEquals("FullName", "Enterprise.Customs.Business.BaseCusInBondHeader",
				BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(CusInBondHeaderSchema.Constants.Prefix)
					.FullName);
		}

		public void TestCompany()
		{
			var header = (BaseCusInBondHeader)Factory.New<Integration.Customs.AsycudaCustoms.ICusInBondHeader>();
			CombineAssertions(() =>
			{
				AssertEquals("default to current company", GlbCompany.CurrentCompany.PK, header.Company.PK);
				header.BH_GB = branch.PK;
				AssertEquals("company is set", branch.Company.PK, header.Company.PK);
			});
		}

		public void TestRegistryBranchPK()
		{
			CombineAssertions(() =>
			{
				var header = (BaseCusInBondHeader)Factory.New<Integration.Customs.AsycudaCustoms.ICusInBondHeader>();
				AssertEquals("default to empty", Guid.Empty, header.RegistryBranchPK);

				var header1 = (BaseCusInBondHeader)Factory.New<Integration.Customs.AsycudaCustoms.ICusInBondHeader>();
				header1.BH_GB = branch.PK;
				AssertEquals("branch is set", branch.PK.ToGuid(), header1.RegistryBranchPK);
			});
		}

		public void TestRegistryCompanyPK()
		{
			CombineAssertions(() =>
			{
				var header = (BaseCusInBondHeader)Factory.New<Integration.Customs.AsycudaCustoms.ICusInBondHeader>();
				AssertEquals("default to current company", GlbCompany.CurrentCompany.PK.ToGuid(), header.RegistryCompanyPK);

				var header1 = (BaseCusInBondHeader)Factory.New<Integration.Customs.AsycudaCustoms.ICusInBondHeader>();
				header1.BH_GB = branch.PK;
				AssertEquals("company is set", branch.GB_GC.ToGuid(), header1.RegistryCompanyPK);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "ABC";
			branch = Factory.New<GlbBranch>();
			branch.GB_Code = "DEF";
			branch.GB_GC = company.PK;
			Factory.Save();
		}

		GlbBranch branch;
	}
}
