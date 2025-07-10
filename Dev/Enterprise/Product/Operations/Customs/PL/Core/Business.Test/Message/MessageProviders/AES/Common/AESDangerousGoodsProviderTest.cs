using System;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class AESDangerousGoodsProviderTest : Customs.Business.Testing.DataProviderTestCase<AESDangerousGoodsProvider>
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>("Null UNDGSubstance", "Value cannot be null.\r\nParameter name: substance", () => new AESDangerousGoodsProvider(null, 1));
		});
	}

	public void TestSequenceNumber()
	{
		AssertEquals(2, Provider.SequenceNumber);
	}

	public void TestUNNumber()
	{
		substance.DG_Code = "AB";
		AssertEquals("AB", Provider.UNNumber);
	}

	protected override AESDangerousGoodsProvider GetProvider() => new AESDangerousGoodsProvider(substance, 2);

	protected override void SetUp()
	{
		base.SetUp();
		substance = Factory.New<UNDGSubstance>();
	}

	UNDGSubstance substance;
}
