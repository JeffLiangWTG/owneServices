using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.EUReferenceData.Business;
using CargoWise.RefDbRepo.EUReferenceData.CommonXmlCoverter.Business;
using CargoWise.RefDbRepo.EUReferenceData.KindOfPackages.Business;
using CargoWise.RefDbRepo.EUReferenceData.Services;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	[TestFixture]
	sealed class AdditionalTranslationsProviderTest
	{
		[Test]
		public void Constructor()
		{
			Assert.Throws<ArgumentNullException>(() => new AdditionalTranslationsProvider(null, new KindOfPackagesDataParser()), "When reader is null");
			Assert.Throws<ArgumentNullException>(() => new AdditionalTranslationsProvider(reader, null), "When dataParser is null");
		}

		[Test]
		public void GetTranslationsWhenSupportAdditionalTranslationsIsTrue()
		{
			var provider = new AdditionalTranslationsProvider(reader, new KindOfPackagesDataParser());

			var expectedTranslations = new Dictionary<string, (string Language, string Description)[]>()
			{
				{ "1A", new (string, string)[] { ("IT", "Fusto di acciaio"), ("FR", "XXX") } },
				{ "1B", new (string, string)[] { ("IT", "Fusto di alluminio"), ("FR", "XXX") } },
				{ "1W", new (string, string)[] { ("IT", "Fusto di legno"), ("FR", "XXX") } },
				{ "TL", new (string, string)[] { ("PL", "Pojemnik z pokrywą") } },
				{ "TN", new (string, string)[] { ("IT", "XXX") } },
				{ "ZL", new (string, string)[] { ("IT", "Contenitore intermedio per rinfuse, composito, in plastica rigida, per solidi"), ("FR", "XXX") } },
				{ "XXX", new (string, string)[] { ("IT", "XXX IT"), ("FR", "XXX FR") } },
				{ "ZZZ", new (string, string)[] { } },
			};

			foreach (var translation in expectedTranslations)
			{
				CollectionAssert.AreEquivalent(translation.Value, provider.GetTranslations(translation.Key).Select(x => (x.Language, x.Description)), $"Translations for {translation.Key}");
			}

		}

		[Test]
		public void GetTranslationsWhenSupportAdditionalTranslationsIsFalse()
		{
			var provider = new AdditionalTranslationsProvider(reader, new KindOfPackagesDataParser());
			Assert.That(provider.GetTranslations("31"), Is.Empty);
		}

		[Test]
		public void GetTranslationsWhenLanguageIsDuplicated()
		{
			var readerMock = new Mock<IAdditionalTranslationsReader>();
			var language = new Language("IT", [new DataParser { Key = "KindOfPackagesDataParser", AdditionalTranslations = [] }]);
			readerMock.Setup(x => x.GetAllTranslations()).Returns(() => [language, language]);
			var provider = new AdditionalTranslationsProvider(readerMock.Object, new KindOfPackagesDataParser());

			var ex = Assert.Throws<InvalidOperationException>(() => provider.GetTranslations("CODE"));
			Assert.That(ex.Message, Does.StartWith("Duplicated language"));
		}

		[Test]
		public void GetTranslationsWhenDataParserIsDuplicated()
		{
			var readerMock = new Mock<IAdditionalTranslationsReader>();
			var dataParser = new DataParser { Key = "KindOfPackagesDataParser" };
			readerMock.Setup(x => x.GetAllTranslations()).Returns(() => [new Language("IT", [dataParser, dataParser])]);
			var provider = new AdditionalTranslationsProvider(readerMock.Object, new KindOfPackagesDataParser());

			var ex = Assert.Throws<InvalidOperationException>(() => provider.GetTranslations("CODE"));
			Assert.That(ex.Message, Does.StartWith("Duplicated data parser"));
		}

		[Test]
		public void GetTranslationsWhenCodeIsDuplicated()
		{
			var readerMock = new Mock<IAdditionalTranslationsReader>();
			var additionalTranslation = new AdditionalTranslation { Code = "XXX", Description = "Description" };
			readerMock.Setup(x => x.GetAllTranslations()).Returns(() => [new Language("IT", [new DataParser { Key = "KindOfPackagesDataParser", AdditionalTranslations = [additionalTranslation, additionalTranslation] }])]);
			var provider = new AdditionalTranslationsProvider(readerMock.Object, new KindOfPackagesDataParser());
			var ex = Assert.Throws<InvalidOperationException>(() => provider.GetTranslations("CODE"));
			Assert.That(ex.Message, Does.StartWith("Duplicated code"));
		}

		[Test]
		public void GetTranslationsWhenDataParserIsNotInJson()
		{
			var provider = new AdditionalTranslationsProvider(reader, new TestDataParser());
			var ex = Assert.Throws<InvalidOperationException>(() => provider.GetTranslations("CODE"));
			Assert.That(ex.Message, Does.StartWith("No translations for data parser"));
		}

		[SetUp]
		public void SetUp()
		{
			reader = new AdditionalTranslationsReader(ApplicationConfig.Instance.RefCusCodeListAdditionalTranslationsPath);
		}

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			ManifestResourceHelper.ExtractAdditionalTranslationsResources();
		}

		AdditionalTranslationsReader reader;

		sealed class TestDataParser : CommonDataParser, IAdditionalTranslationSupporter
		{
			public TestDataParser() : base("TEST", "TEST")
			{
			}

			public string DataParserKey => nameof(TestDataParser);
		}
	}
}
