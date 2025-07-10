using System;
using System.Linq;
using CargoWise.RefDbRepo.EUReferenceData.Business;
using CargoWise.RefDbRepo.EUReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	[TestFixture]
	sealed class AdditionalTranslationsReaderTest
	{
		[Test]
		public void Constructor()
		{
			Assert.Throws<ArgumentNullException>(() => new AdditionalTranslationsReader(null), "When path is null");
			Assert.Throws<ArgumentException>(() => new AdditionalTranslationsReader(string.Empty), "When path is empty");
		}

		[Test]
		public void GetAllTranslations()
		{
			var reader = new AdditionalTranslationsReader(ApplicationConfig.Instance.RefCusCodeListAdditionalTranslationsPath);
			var translations = reader.GetAllTranslations();
			CollectionAssert.AreEquivalent(new string[] { "FR", "IT", "PL" }, translations.Select(x => x.Key));
		}


		[Test]
		public void GetAllTranslationsWhenRefCusCodeListAdditionalTranslationsPathIsInvalid()
		{
			var reader = new AdditionalTranslationsReader(".\\Invalid\\Path");
			Assert.That(reader.GetAllTranslations(), Is.Empty);
		}

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			ManifestResourceHelper.ExtractAdditionalTranslationsResources();
		}
	}
}
