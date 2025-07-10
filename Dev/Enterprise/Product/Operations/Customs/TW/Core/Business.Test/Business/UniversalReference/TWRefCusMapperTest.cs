using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class TWRefCusMapperTest : TestCaseWithFactory
	{
		public void TestMapCW1UnitPriceUQToCustomsCode()
		{
			TWRefCusMapperHelperForTest.SetRefCusMapper(Factory);
			AssertEquals("YRD", TWRefCusMapper.MapCW1UnitPriceUQToCustomsCode(Factory, "YDS"));
			AssertEquals("PCE", TWRefCusMapper.MapCW1UnitPriceUQToCustomsCode(Factory, "PCS"));
			AssertEquals("DZN", TWRefCusMapper.MapCW1UnitPriceUQToCustomsCode(Factory, "DOZ"));
		}
	}
}
