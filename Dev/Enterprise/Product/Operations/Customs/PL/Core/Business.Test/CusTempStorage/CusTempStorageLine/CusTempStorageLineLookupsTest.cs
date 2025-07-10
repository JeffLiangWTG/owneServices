using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.PL.Business.CusTempStorage.Testing;

class CusTempStorageLineLookupsTest : TestCaseWithFactory
{
	public void TestWeightUQLookup()
	{
		var lookup = lookups.WeightUQList;
		Assert("Lookup should contain KG for Kilograms", lookup.ContainsCode("KG"));
	}

	protected override void SetUp()
	{
		base.SetUp();

		line = Factory.New<CusTempStorageLine>();
		lookups = new CusTempStorageLineLookups(line);
	}

	CusTempStorageLine line;
	CusTempStorageLineLookups lookups;
}
