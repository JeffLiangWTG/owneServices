using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.Business;

namespace Enterprise.Customs.VN.Manifest.Business.Testing
{
	public class AsycudaManifestHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestAMA_MessageStatusList()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var actualList = header.Lookups.MessageStatusList.GetAllCodes();
			var expectedList = new[]
			{
				MessageStatusCodeList.Codes.NotSent,
				MessageStatusCodeList.Codes.Sent,
				MessageStatusCodeList.Codes.Updated,
				MessageStatusCodeList.Codes.Cancel,
			};

			AssertArrayEqualsByElements(expectedList, actualList);
		}
	}
}
