using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	public class ItemIdentityNumberQualifierListTest : TestCaseWithFactory
	{
		public void TestGetListForFWS()
		{
			var fullList = new ItemIdentityNumberQualifierList();
			var list = ItemIdentityNumberQualifierList.GetListForFWS(Factory);
			AssertEquals("Data should be cached", list, ItemIdentityNumberQualifierList.GetListForFWS(Factory));
			var expectedCodes = new[] {
				ItemIdentityNumberQualifierList.Codes.OfficialAnimalNumber,
				ItemIdentityNumberQualifierList.Codes.SerialNumber,
				ItemIdentityNumberQualifierList.Codes.Tattoo
			};
			AssertEquals(expectedCodes.Length, list.Count);
			foreach (var code in expectedCodes)
			{
				AssertEquals(code, fullList.GetDescriptionFromCode(code), list.GetDescriptionFromCode(code));
			}
		}
	}
}
