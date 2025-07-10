using CargoWise.RefDbRepo.ZAReferenceData.Services.Common;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Tariff.Loader;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ZAReferenceData.Tests.Services.Common
{
	[TestFixture]
	public class ObjectFactoryHelperTests
	{
		[Test]
		public void GetCountryCodeLoader()
		{
			var logger = new TestLogger();
			var loader = ObjectFactoryHelper.GetCountryCodeLoader(logger);

			Assert.That(loader, Is.Not.Null);
			Assert.That(loader.GetType(), Is.EqualTo(typeof(CountryCodeLoader)));
		}

		[Test]
		public void GetMessageHandler()
		{
			var logger = new TestLogger();
			var messageHandler = ObjectFactoryHelper.GetMessageHandler(logger, SupportedMessageTypes.Prodat);

			Assert.That(messageHandler, Is.Not.Null);
			Assert.That(messageHandler.GetType(), Is.EqualTo(typeof(MessageHandler)));
		}

		[Test]
		public void GetTariffHelper()
		{
			var logger = new TestLogger();
			var tariffHelper = ObjectFactoryHelper.GetTariffHelper(logger);

			Assert.That(tariffHelper, Is.Not.Null);
			Assert.That(tariffHelper.GetType(), Is.EqualTo(typeof(TariffHelper)));
		}
	}
}
