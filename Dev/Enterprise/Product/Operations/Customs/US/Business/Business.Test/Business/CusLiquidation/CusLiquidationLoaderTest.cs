using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(CusLiquidation.Loader))]
	sealed class CusLiquidationLoaderTest : LoaderTestCase
	{
		public void TestHumanReadableName()
		{
			var liquidation = Factory.New<CusLiquidation>();
			liquidation.B8_EntryNumber = "71007700";
			AssertEquals("Liquidation Notice 71007700", liquidation.HumanReadableName);
		}

		public void TestLoadTop1WithActualLiquidationDate()
		{
			var liquidation1 = Factory.New<CusLiquidation>();
			liquidation1.B8_EntryFilerCode = "XJ5";
			liquidation1.B8_EntryNumber = "1";
			var liquidation2 = Factory.New<CusLiquidation>();
			liquidation2.B8_EntryFilerCode = "XJ5";
			liquidation2.B8_EntryNumber = "2";
			liquidation2.B8_LiquidationDate = new CargoWise.Types.ZDateTime(2020, 12, 12);
			AssertNull(new CusLiquidation.Loader(Factory).LoadTop1WithActualLiquidationDate("XJ5", "1"));
			AssertEquals(liquidation2, new CusLiquidation.Loader(Factory).LoadTop1WithActualLiquidationDate("XJ5", "2"));
		}

		protected override BusinessObject.Loader GetNewLoaderToTest() => new CusLiquidation.Loader(Factory);
	}
}
