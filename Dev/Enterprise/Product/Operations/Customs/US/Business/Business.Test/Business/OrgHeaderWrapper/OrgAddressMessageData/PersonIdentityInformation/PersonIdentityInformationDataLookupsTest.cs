using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	class PersonIdentityInformationDataLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestList()
		{
			var wrapper = OrgHeaderWrapper.New(Factory.New<OrgHeader>());
			var messageData = new OrgAddressMessageData(wrapper);
			var lookups = messageData.PIIs.AddNew().Lookups;
			AssertEquals(typeof(RefCountryCollection), lookups.CountryList.GetType());
			AssertEquals(typeof(OrgContactDependentCollection), lookups.Contacts.GetType());
			AssertEquals(typeof(PassportTypesList), lookups.PassportTypeList.GetType());
		}
	}
}
