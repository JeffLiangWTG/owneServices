using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgARTermsCollection))]
	sealed class OrgARTermsCollectionTest : ActiveBusinessObjectCollectionTestCase<OrgARTermsCollection>
	{
		protected override System.Type GetExpectedCollectionType()
		{
			return typeof(OrgARTermsCollection);
		}

		protected override OrgARTermsCollection GetCollectionToTest()
		{
			OrgCompanyData companyData = Factory.NewWithValidTestData<OrgCompanyData>();
			Factory.Save();
			return new OrgARTermsCollection(companyData);
		}
	}
}
