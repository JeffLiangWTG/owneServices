using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Universal.Testing
{
	public class AdditionalAttributeInformationProviderTest : TestCaseWithFactory
	{
		public void TestAdditionalAttributeInformationProviderProperties()
		{
			var provider = new AdditionalAttributeInformationProvider();
			AssertEquals("", provider.AdditionalDescription("", ""));
			AssertEquals(false, provider.AdditionalDescriptionVisible);
		}
	}
}
