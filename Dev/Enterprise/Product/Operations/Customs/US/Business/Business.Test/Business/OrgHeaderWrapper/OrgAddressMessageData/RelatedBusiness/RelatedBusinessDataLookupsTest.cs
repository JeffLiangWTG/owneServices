using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	class RelatedBusinessDataLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestList()
		{
			var wrapper = OrgHeaderWrapper.New(Factory.New<OrgHeader>());
			var messageData = new OrgAddressMessageData(wrapper);
			var lookups = messageData.RelatedBusinessItems.AddNew().Lookups;
			AssertEquals(typeof(ImporterRelatedBusinessTypeList), lookups.RelatedBusinessTypes.GetType());
		}
	}
}
