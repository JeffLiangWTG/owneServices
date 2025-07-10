namespace Enterprise.Customs.NZ.Business.Express.Testing
{
	using NUnit.Framework;

	public class CusRelatedEventsProviderTest : TestCase
	{
		public void TestAvailableApplicationCodes()
		{
			var provider = new CusRelatedEventsProvider();
			AssertEquals("TSW", string.Join(",", provider.AvailableApplicationCodes));
		}
	}
}
