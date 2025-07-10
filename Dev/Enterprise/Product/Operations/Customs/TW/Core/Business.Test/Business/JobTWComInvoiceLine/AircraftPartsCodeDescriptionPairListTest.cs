using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class AircraftPartsCodeDescriptionPairListTest : TestCase
	{
		public void TestSort()
		{
			var list = new AircraftPartsCodeDescriptionPairList();
			list.AddPair("4", "A");
			list.AddPair("2", "B");
			list.AddPair("6", "C");
			list.AddPair("a1", "D");
			list.AddPair("1", "E");
			list.AddPair("3", "F");
			list.AddPair("5", "G");
			list.AddPair("b1", "H");
			list.Sort();

			CombineAssertions(() =>
			{
				AssertEquals("1", list[0].Code);
				AssertEquals("2", list[1].Code);
				AssertEquals("3", list[2].Code);
				AssertEquals("4", list[3].Code);
				AssertEquals("5", list[4].Code);
				AssertEquals("6", list[5].Code);
				AssertEquals("a1", list[6].Code);
				AssertEquals("b1", list[7].Code);
			});
		}
	}
}
