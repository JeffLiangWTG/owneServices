using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	public sealed class GlobalBusinessIdentifierDataLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestSubmissionStatusList()
		{
			var messageData = new GlobalBusinessIdentifierData(OrgHeaderWrapper.New(Factory.New<OrgHeader>()));
			AssertType<GBISubmissionStatusList>(messageData.Lookups.SubmissionStatusList);
			AssertEquals(9, messageData.Lookups.SubmissionStatusList.Count);
		}
	}
}
