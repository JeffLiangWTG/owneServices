using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.NCTS.Business.Testing
{
	[TestedType(typeof(NctsExportToOpenCollection))]
	class NctsExportToOpenCollectionTest : CusSupportingInfoCollectionTest<NctsExportToOpen>
	{
		protected override CusSupportingInfoCollection<NctsExportToOpen> GetCusSupportingInfoCollection()
		{
			var header = Factory.NewWithValidTestData<NctsHeader>();
			return new NctsExportToOpenCollection(header, CusSupportingInfoTypeList.Codes.ETO);
		}
	}
}
