using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.Testing
{
	[TestedType(typeof(RefUNLOCOCollectionWithCarrierMapping))]
	sealed class RefUNLOCOCollectionWithCarrierMappingTest : RefUNLOCOCollectionTest<RefUNLOCOCollectionWithCarrierMapping>
	{
		protected override RefUNLOCOCollectionWithCarrierMapping GetCollectionToTest()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			var result = new RefUNLOCOCollectionWithCarrierMapping(Factory, carrier.PK);

			var fetchFromLocalCacheFilter = new ZQuery();
			fetchFromLocalCacheFilter.FetchOnlyFromLocalCache = true;
			result.AdditionalFilter = fetchFromLocalCacheFilter;

			return result;
		}
	}
}
