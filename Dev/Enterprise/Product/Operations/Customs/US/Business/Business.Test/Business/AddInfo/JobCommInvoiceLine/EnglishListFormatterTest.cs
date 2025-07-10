using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class EnglishListFormatterTest : TestCase
	{
		public void TestZeroItems()
		{
			AssertEquals("", EnglishListFormatter.GetString(System.Array.Empty<string>()));
		}

		public void TestSingleItem()
		{
			AssertEquals("Chicken", EnglishListFormatter.GetString(new string[] { "Chicken" }));
		}

		public void TestTwoItems()
		{
			AssertEquals("Chicken or Beef", EnglishListFormatter.GetString(new string[] { "Chicken", "Beef" }));
		}

		public void TestThreeItems()
		{
			AssertEquals("Chicken, Beef or Fish", EnglishListFormatter.GetString(new string[] { "Chicken", "Beef", "Fish" }));
		}

		public void TestFourItems()
		{
			AssertEquals("Chicken, Beef, Fish or Lamb", EnglishListFormatter.GetString(new string[] { "Chicken", "Beef", "Fish", "Lamb" }));
		}
	}
}
