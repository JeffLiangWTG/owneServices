using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	class FWSWildlifeSourceListTest : TestCaseWithFactory
	{
		public void TestGetList()
		{
			var fullList = new FWSWildlifeSourceList();
			var list1 = FWSWildlifeSourceList.GetList(Factory, false);
			var list2 = FWSWildlifeSourceList.GetList(Factory, false);
			AssertEquals("Is Cached", list1, list2);
			var expectedList = new[]
			{
				FWSWildlifeSourceList.Codes.C,
				FWSWildlifeSourceList.Codes.D,
				FWSWildlifeSourceList.Codes.F,
				FWSWildlifeSourceList.Codes.I,
				FWSWildlifeSourceList.Codes.P2,
				FWSWildlifeSourceList.Codes.R,
				FWSWildlifeSourceList.Codes.DOM,
				FWSWildlifeSourceList.Codes.W,
				FWSWildlifeSourceList.Codes.U6,
				FWSWildlifeSourceList.Codes.X
			};
			AssertEquals(expectedList.Length, list1.Count);
			foreach (var code in expectedList)
			{
				AssertEquals(code, fullList.GetDescriptionFromCode(code), list1.GetDescriptionFromCode(code));
			}

			expectedList = new[]
			{
				FWSWildlifeSourceList.Codes.W,
				FWSWildlifeSourceList.Codes.R,
				FWSWildlifeSourceList.Codes.P2,
				FWSWildlifeSourceList.Codes.F,
				FWSWildlifeSourceList.Codes.U6,
				FWSWildlifeSourceList.Codes.C,
				FWSWildlifeSourceList.Codes.I,
				FWSWildlifeSourceList.Codes.D,
				FWSWildlifeSourceList.Codes.DOM,
				FWSWildlifeSourceList.Codes.X
			};
			var list3 = FWSWildlifeSourceList.GetList(Factory, true);
			AssertEquals(10, list3.Count);
			foreach (var code in expectedList)
			{
				AssertEquals(code, fullList.GetDescriptionFromCode(code), list3.GetDescriptionFromCode(code));
			}
		}
	}
}
