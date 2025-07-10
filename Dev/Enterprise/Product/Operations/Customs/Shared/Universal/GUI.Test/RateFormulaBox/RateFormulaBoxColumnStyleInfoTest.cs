using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Universal.GUI.Testing
{
	class RateFormulaBoxColumnStyleInfoTest : TestCaseWithFactory
	{
		public void TestColumnStyleType()
		{
			AssertEquals(typeof(RateFormulaBoxColumnStyle), new RateFormulaBoxColumnStyleInfo().ColumnStyleType);
		}
	}
}
