using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Macros.Testing
{
	sealed class CompanyTest : TestCaseWithFactory
	{
		public void TestCompany()
		{
			var glbCompany = Factory.New<GlbCompany>();
			glbCompany.GC_Code = "AAA";
			glbCompany.GC_Name = "Monsters Inc";
			glbCompany.GC_RN_NKCountryCode = "AU";
			glbCompany.GC_IsReciprocal = ZBool.True;

			var orgProxy = Factory.New<OrgHeader>();
			orgProxy.OH_Code = "ZZZ";

			glbCompany.GC_OH_OrgProxy = orgProxy.PK;

			var company = new Company(glbCompany);

			CombineAssertions(() =>
			{
				AssertEquals("Code", "AAA", company.Code);
				AssertEquals("Name", "Monsters Inc", company.Name);
				AssertEquals("PK", glbCompany.PK, company.PK);
				AssertEquals("Country.Code", "AU", company.Country.Code);
				AssertEquals("LicenceCode", "EDIAAADAT", company.LicenceCode);
				AssertEquals("Organization.Code", "ZZZ", company.Organization.Code);
				AssertEquals("IsReciprocal (Currency)", true, company.IsReciprocal);
			});
		}

		public void TestNullCompany()
		{
			var company = new Company(null);

			CombineAssertions(() =>
			{
				AssertNullOrEmpty("Code", company.Code);
				AssertNullOrEmpty("Name", company.Name);
				AssertEquals("PK", ZGuid.Empty, company.PK);
				AssertNotNull("Country", company.Country);
				AssertNullOrEmpty("LicenceCode", company.LicenceCode);
				AssertNotNull("Organization.Code", company.Organization);
				AssertEquals("IsReciprocal (Currency)", false, company.IsReciprocal);
			});
		}
	}
}
