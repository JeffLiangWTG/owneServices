using NUnit.Framework;

namespace Enterprise.TransportConsignment.Business.Testing
{
	class PackTypeCountTest : TestCase
	{
		#region TestConstructor

		public void TestConstructor()
		{
			AssertEquals("PackType", "PLT", new PackTypeCount("PLT", 10).PackType);
			AssertEquals("Quantity", 10, new PackTypeCount("PLT", 10).Quantity);
		}

		#endregion

		#region TestEquals

		public void TestEquals()
		{
			AssertEquals(true, new PackTypeCount("PLT", 10).Equals(new PackTypeCount("PLT", 10)));
			AssertEquals(false, new PackTypeCount("BOX", 10).Equals(new PackTypeCount("PLT", 10)));
			AssertEquals(false, new PackTypeCount("PLT", 9).Equals(new PackTypeCount("PLT", 10)));
			AssertEquals(false, new PackTypeCount("PLT", 10).Equals("10x PLT"));
		}

		#endregion

		#region TestGetHashCode

		public void TestGetHashCode()
		{
			AssertEquals("PLT".GetHashCode() ^ 10.GetHashCode(), new PackTypeCount("PLT", 10).GetHashCode());
		}

		#endregion

		#region TestEqualityOperators

		public void TestEqualityOperators()
		{
			AssertEquals(true, new PackTypeCount("PLT", 10) == new PackTypeCount("PLT", 10));
			AssertEquals(false, new PackTypeCount("BOX", 10) == new PackTypeCount("PLT", 10));
			AssertEquals(false, new PackTypeCount("PLT", 9) == new PackTypeCount("PLT", 10));
			AssertEquals(false, new PackTypeCount("PLT", 10) != new PackTypeCount("PLT", 10));
			AssertEquals(true, new PackTypeCount("BOX", 10) != new PackTypeCount("PLT", 10));
			AssertEquals(true, new PackTypeCount("PLT", 9) != new PackTypeCount("PLT", 10));
		}

		#endregion

		#region TestToString

		public void TestToString()
		{
			AssertEquals("10x PLT", new PackTypeCount("PLT", 10).ToString());
			AssertEquals("5x BOX", new PackTypeCount("BOX", 5).ToString());
		}

		#endregion
	}
}
