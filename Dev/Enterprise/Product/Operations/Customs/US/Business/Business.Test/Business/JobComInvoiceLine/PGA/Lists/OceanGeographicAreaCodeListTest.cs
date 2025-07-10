using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	public class OceanGeographicAreaCodeListTest : TestCaseWithFactory
	{
		public void TestGetListFor370Program()
		{
			var fullList = new OceanGeographicAreaCodeList();
			var list = OceanGeographicAreaCodeList.GetListFor370Program(Factory);
			AssertEquals("Data should be cached", list, OceanGeographicAreaCodeList.GetListFor370Program(Factory));
			var expectedCodes = new[] {
				OceanGeographicAreaCodeList.Codes.ETP,
				OceanGeographicAreaCodeList.Codes.WP,
				OceanGeographicAreaCodeList.Codes.NP,
				OceanGeographicAreaCodeList.Codes.SP,
				OceanGeographicAreaCodeList.Codes.EA,
				OceanGeographicAreaCodeList.Codes.WA,
				OceanGeographicAreaCodeList.Codes.IND,
				OceanGeographicAreaCodeList.Codes.CAR,
				OceanGeographicAreaCodeList.Codes.OTH
			};
			AssertEquals(expectedCodes.Length, list.Count);
			foreach (var code in expectedCodes)
			{
				AssertEquals(code, fullList.GetDescriptionFromCode(code), list.GetDescriptionFromCode(code));
			}
		}
	}
}
