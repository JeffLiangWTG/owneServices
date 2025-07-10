using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	public class WhsUNDGLimitValidationInfoTest : TestCase
	{
		public void TestConstructor_DG()
		{
			var code = "123";
			var totalWeight = 10m;
			var totalVolume = 11m;

			var info = new WhsUNDGLimitValidationInfo(UNDGLimitType.DG, code, totalWeight, totalVolume);
			CombineAssertions(() =>
			{
				AssertEquals("DG", info.Title);
				AssertEquals(code, info.Code);
				AssertEquals(totalWeight, info.TotalWeight);
				AssertEquals(totalVolume, info.TotalVolume);
			});
		}

		public void TestConstructor_CountryReference()
		{
			var code = "123";
			var totalWeight = 10m;
			var totalVolume = 11m;

			var info = new WhsUNDGLimitValidationInfo(UNDGLimitType.CountryReference, code, totalWeight, totalVolume);
			CombineAssertions(() =>
			{
				AssertEquals("Country Reference", info.Title);
				AssertEquals(code, info.Code);
				AssertEquals(totalWeight, info.TotalWeight);
				AssertEquals(totalVolume, info.TotalVolume);
			});
		}

		public void TestConstructor_UNDGClass()
		{
			var code = "123";
			var totalWeight = 10m;
			var totalVolume = 11m;

			var info = new WhsUNDGLimitValidationInfo(UNDGLimitType.UNDGClass, code, totalWeight, totalVolume);
			CombineAssertions(() =>
			{
				AssertEquals("UNDG Class", info.Title);
				AssertEquals(code, info.Code);
				AssertEquals(totalWeight, info.TotalWeight);
				AssertEquals(totalVolume, info.TotalVolume);
			});
		}
	}
}
