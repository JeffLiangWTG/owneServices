using CargoWise.RefDbRepo.ILReferenceData.Business;
using CargoWise.RefDbRepo.ILReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ILReferenceData.Tests.Business
{
	[TestFixture]
	public class CustomsCodesProcessorsProviderTest
	{
		[Test]
		public void TestProvider()
		{
			var logger = new Logger();
			Assert.IsInstanceOf<TradeGroupsCustomsResponseProcessor>(CustomsResponseProcessorProvider.GetCustomsResponseProcessor("2009", logger));
			Assert.IsInstanceOf<TradeGroupCountriesCustomsResponseProcessor>(CustomsResponseProcessorProvider.GetCustomsResponseProcessor("23689", logger));
			Assert.IsInstanceOf<ILCustomsCodesProcessor>(CustomsResponseProcessorProvider.GetCustomsResponseProcessor("910", logger));
		}
	}
}
