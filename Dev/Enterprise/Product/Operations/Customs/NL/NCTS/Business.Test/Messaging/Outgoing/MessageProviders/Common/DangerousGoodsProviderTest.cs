using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

[TestedType(typeof(DangerousGoodsProvider))]
sealed class DangerousGoodsProviderTest : Customs.Business.Testing.DataProviderTestCase<DangerousGoodsProvider>
{
	public void TestSequenceNumeric()
	{
		AssertEquals(3, Provider.SequenceNumeric);
	}

	public void TestUndgid()
	{
		AssertEquals("xyz", Provider.Undgid);
	}

	protected override void SetUp()
	{
		base.SetUp();

		substance = Factory.New<UNDGSubstance>();
		substance.DG_UNNO = "xyz";
		var undg = Factory.New<UNDGDataItem>();
		undg.DI_DG = substance.PK;
		provider = new DangerousGoodsProvider(undg, 3);
	}

	protected override DangerousGoodsProvider GetProvider() => provider;

	UNDGSubstance substance;
	DangerousGoodsProvider provider;
}
