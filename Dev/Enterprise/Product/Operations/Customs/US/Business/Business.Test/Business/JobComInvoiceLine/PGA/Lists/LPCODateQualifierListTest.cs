using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	public class LPCODateQualifierListTest : TestCaseWithFactory
	{
		public void TestGetListForAPHIS()
		{
			var fullList = new LPCODateQualifierList();
			var list = LPCODateQualifierList.GetListForAPHIS(Factory);
			AssertEquals("Data should be cached", list, LPCODateQualifierList.GetListForAPHIS(Factory));
			var expectedCodes = new[] {
				LPCODateQualifierList.Codes.ExpirationDate,
				LPCODateQualifierList.Codes.DateIssuedOrSigned
			};
			AssertEquals(expectedCodes.Length, list.Count);
			foreach (var code in expectedCodes)
			{
				AssertEquals(code, fullList.GetDescriptionFromCode(code), list.GetDescriptionFromCode(code));
			}
		}
	}
}
