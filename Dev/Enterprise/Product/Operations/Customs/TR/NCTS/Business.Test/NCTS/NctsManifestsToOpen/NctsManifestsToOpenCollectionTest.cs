using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.NCTS.Business.Testing
{
	[TestedType(typeof(NctsManifestsToOpenCollection))]
	class NctsManifestsToOpenCollectionTest : CusSupportingInfoCollectionTest<NctsManifestsToOpen>
	{
		protected override CusSupportingInfoCollection<NctsManifestsToOpen> GetCusSupportingInfoCollection()
		{
			var header = Factory.NewWithValidTestData<NctsHeader>();
			return new NctsManifestsToOpenCollection(header, CusSupportingInfoTypeList.Codes.MTO);
		}
	}
}
