using NUnit.Framework;

namespace Enterprise.Customs.NZ.TradeSingleWindow.Testing
{
	class NZWeightHelperTest : TestCase
	{
		public void TestApplyWeightRounding()
		{
			AssertEquals("Zero is rounded as Zero", 0m, NZWeightHelper.ApplyWeightRounding(0m));
			AssertEquals("1/3 is rounded as One", 1m, NZWeightHelper.ApplyWeightRounding(0.33m));
			AssertEquals("2 9/20 is rounded as Two", 2m, NZWeightHelper.ApplyWeightRounding(2.45m));
			AssertEquals("2 11/20 is rounded as Three", 3m, NZWeightHelper.ApplyWeightRounding(2.55m));
		}
	}
}
