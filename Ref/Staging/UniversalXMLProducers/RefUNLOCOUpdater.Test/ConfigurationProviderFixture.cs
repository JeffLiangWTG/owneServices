using System;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.RefUNLOCOUpdater.Test
{
	[TestFixture]
	class ConfigurationProviderFixture
	{
		[Test]
		public void UNECESpecificFilePublicationDateTest()
		{
			var date = ConfigurationProvider.UNECESpecificFilePublicationDate;
			var expected = new DateTime(2024, 12, 31);
			Assert.That(date, Is.EqualTo(expected));
		}

		[Test]
		public void UNECESpecificFileEnableUntilTest()
		{
			var date = ConfigurationProvider.UNECESpecificFileEnableUntil;
			var expected = new DateTime(9999, 12, 31);
			Assert.That(date, Is.EqualTo(expected));
		}

		[SetUp]
		public void Setup()
		{
			ConfigurationProvider.SetConfigFileForTest("CargoWise.RefDbRepo.UniversalXMLProducers.RefUNLOCOUpdater.Test.config.json");
		}

		[TearDown]
		public void TearDown()
		{
			ConfigurationProvider.SetConfigFileForTest(null);
		}
	}
}
