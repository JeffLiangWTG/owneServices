using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgCompanyDataDependentCollection))]
	sealed class OrgCompanyDataDependentCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new OrgCompanyDataDependentCollection(Factory.New<OrgHeader>());
		}

		public void TestOrgCompanyDataCollectionDoesNotIncludeSameCompany()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();
			var companyData1 = org.CompanyData;
			var companyData2 = Factory.NewWithValidTestData<OrgCompanyData>();
			companyData2.OB_OH = org.PK;
			org.CompanyDataCollection.Add(companyData2);

			CombineAssertions(() =>
			{
				AssertEquals("CompanyData 1 has row notification", false, companyData1.HasRowNotifications);

				AssertEquals("CompanyData 2 has row notification", true, companyData2.HasRowNotifications);
				var expectedMsg = $"This company ({companyData2.Company?.GC_Code}) has already been added to the Organization ({org.OH_Code}).";
				AssertHasRowError(companyData2, expectedMsg);
			});
		}
	}
}
