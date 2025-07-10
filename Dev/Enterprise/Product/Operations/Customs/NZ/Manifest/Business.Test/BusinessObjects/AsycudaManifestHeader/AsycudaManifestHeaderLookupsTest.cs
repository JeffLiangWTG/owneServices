using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Common.NZ;

namespace Enterprise.Customs.NZ.Manifest.Business.Testing
{
	sealed class AsycudaManifestHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestMessageStatusList()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = NZManifestTypes.Codes.ICR;
			AssertEquals(new NZMessageStatusList(), header.Lookups.MessageStatusList);
			header.AMA_ManifestType = NZManifestTypes.Codes.OCR;
			AssertEquals(new MessageStatusCodeList(), header.Lookups.MessageStatusList);
		}
	}
}
