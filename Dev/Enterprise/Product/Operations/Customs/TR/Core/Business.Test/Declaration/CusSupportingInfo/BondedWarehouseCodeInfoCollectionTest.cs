using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Declaration.Testing
{
	[TestedType(typeof(BondedWarehouseCodeInfoCollection))]
	public class BondedWarehouseCodeInfoCollectionTest : Customs.Business.Testing.CusSupportingInfoCollectionTest<BondedWarehouseCodeInfo>
	{
		protected override CusSupportingInfoCollection<BondedWarehouseCodeInfo> GetCusSupportingInfoCollection()
		{
			var dec = Factory.New<JobDeclaration>().CusSupportingInfoList.AddNew();
			return new BondedWarehouseCodeInfoCollection(dec);
		}
	}
}
