using System;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class DangerousGoodsProviderTest : Customs.Business.Testing.DataProviderTestCase<DangerousGoodsProvider>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Null Dangerous Goods Data Item", "Value cannot be null.\r\nParameter name: dangerousSubstance", () => new DangerousGoodsProvider(0, null));
	}

	public void TestSequenceNumber()
	{
		AssertEquals("SequenceNumber", "42", Provider.SequenceNumber);
	}

	public void TestUNNumber()
	{
		dangerousSubstance.DG_UNNO = "1234";
		AssertEquals("UNNumber", "1234", Provider.UNNumber);
	}

	protected override DangerousGoodsProvider GetProvider()
	{
		return new DangerousGoodsProvider(42, dangerousSubstance);
	}

	protected override void SetUp()
	{
		base.SetUp();
		dangerousSubstance = Factory.New<UNDGSubstance>();
	}

	UNDGSubstance dangerousSubstance;
}
