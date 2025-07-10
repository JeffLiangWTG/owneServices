using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.NCTS.Business.Testing
{
	[TestedType(typeof(NctsWarehouseToOpenCollection))]
	class NctsWarehouseToOpenCollectionTest : CusSupportingInfoCollectionTest<NctsWarehouseToOpen>
	{
		protected override CusSupportingInfoCollection<NctsWarehouseToOpen> GetCusSupportingInfoCollection()
		{
			var header = Factory.NewWithValidTestData<NctsHeader>();
			return new NctsWarehouseToOpenCollection(header, CusSupportingInfoTypeList.Codes.PRE);
		}
	}
}
