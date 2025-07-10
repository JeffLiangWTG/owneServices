using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.ZAReferenceData.Business.ExchangeRates;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Common;
using CargoWise.RefDbRepo.ZAReferenceData.Services.ExchangeRates.Models;
using CargoWise.RefDbRepo.ZAReferenceData.Tests.Helpers;
using CargoWise.RefDbRepo.ZAReferenceData.Tests.Services.Common;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ZAReferenceData.Tests.Business.ExchangeRates
{
	[TestFixture]
	sealed class ExchangeRateBuilderTests
	{
		[Test]
		public void FilePrefix()
		{
			var builder = new ExchangeRateBuilderForTest(null, null);
			Assert.That(builder.FilePrefix, Is.EqualTo("ZA_RefExchangeRate"));
		}

		[Test]
		public void DataSource()
		{
			var builder = new ExchangeRateBuilderForTest(null, null);
			Assert.That(builder.DataSource, Is.EqualTo("ZAExchangeRates"));
		}

		[Test]
		public void UpdateType()
		{
			var builder = new ExchangeRateBuilderForTest(null, null);
			Assert.That(builder.UpdateType, Is.EqualTo(Common.UniversalXmlWriter.UpdateType.Full));
		}

		[Test]
		public void XmlWriterConfig()
		{
			var builder = new ExchangeRateBuilderForTest(null, null);

			var config = builder.GetXmlWriterConfiguration();

			Assert.That(config, Is.Not.Null);
			var refType = typeof(RefExchangeRateZZ);
			var entityConfig = config.GetConfiguration(refType);

			Assert.That(entityConfig, Is.Not.Null);
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefExchangeRateZZ.ZZN_RX_NKExCurrency))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefExchangeRateZZ.ZZN_RN_NKCountry))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefExchangeRateZZ.ZZN_ExRateType))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefExchangeRateZZ.ZZN_StartDate))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefExchangeRateZZ.ZZN_EndDate))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefExchangeRateZZ.ZZN_Rate))));

			XmlWriterConfigTestHelper.AssertKeySets(nameof(RefExchangeRateZZ), entityConfig.GetKeySets(),
				nameof(RefExchangeRateZZ.ZZN_ExRateType),
				nameof(RefExchangeRateZZ.ZZN_RN_NKCountry),
				nameof(RefExchangeRateZZ.ZZN_RX_NKExCurrency));
		}

		[Test]
		public void ConvertModels()
		{
			var effDate = new DateTime(2023, 9, 22);
			var data = new ExchangeRateData
			{
				PublishDate = new DateTime(2023, 9, 22, 14, 13, 12),
				ExchangeRates = new[]
				{
					new ExchangeRate { CountryCode = "ZA", Currency = "USD", RateType = "CUS", Rate = 0.0556m, StartDate = effDate, EndDate = effDate },
					new ExchangeRate { CountryCode = "NA", Currency = "GBP", RateType = "CUS", Rate = 0.1234m, StartDate = effDate, EndDate = effDate }
				}
			};

			var logger = new TestLogger();
			var builder = new ExchangeRateBuilderForTest(data, logger);
			var refModels = builder.ConvertToRefModels();

			Assert.That(refModels, Is.Not.Null);
			Assert.That(refModels.Count, Is.EqualTo(2));

			Assert.That(refModels[0].ZZN_RN_NKCountry, Is.EqualTo("ZA"));
			Assert.That(refModels[0].ZZN_RX_NKExCurrency, Is.EqualTo("USD"));
			Assert.That(refModels[0].ZZN_ExRateType, Is.EqualTo("CUS"));
			Assert.That(refModels[0].ZZN_Rate, Is.EqualTo(0.0556m));
			Assert.That(refModels[0].ZZN_StartDate, Is.EqualTo(effDate));
			Assert.That(refModels[0].ZZN_EndDate, Is.EqualTo(effDate));
		}

		[Test]
		public void BuildXmlFile()
		{
			var effDate = new DateTime(2023, 9, 22);
			var data = new ExchangeRateData
			{
				PublishDate = new DateTime(2023, 9, 22, 14, 13, 12),
				ExchangeRates = new[]
				{
					new ExchangeRate { CountryCode = "ZA", Currency = "USD", RateType = "CUS", Rate = 0.0556m, StartDate = effDate, EndDate = effDate },
					new ExchangeRate { CountryCode = "NA", Currency = "GBP", RateType = "CUS", Rate = 0.1234m, StartDate = effDate, EndDate = effDate }
				}
			};

			var logger = new TestLogger();
			var builder = new ExchangeRateBuilderForTest(data, logger);
			builder.CreateXmlFile(TempFolder, new DateTime(2022, 5, 6, 13, 14, 15));

			var fileName = Path.Combine(TempFolder, builder.OutputFilename);
			Assert.That(File.Exists(fileName));

			var xml = File.ReadAllText(fileName);

			var expectedXml = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.ZAReferenceData.Tests.Business.ExchangeRates.TestFiles.Output.ZAExchangeRates_001.xml");

			Assert.That(xml, Is.EqualTo(expectedXml));

			Assert.That(logger.InfoString, Contains.Substring($"Created {fileName}"));
		}

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			TempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			logger = new TestLogger();
		}

		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			if (Directory.Exists(TempFolder))
			{
				Directory.Delete(TempFolder, true);
			}
		}

		TestLogger logger;
		string TempFolder;
	}

	class ExchangeRateBuilderForTest : ExchangeRateBuilder
	{
		public ExchangeRateBuilderForTest(ExchangeRateData sourceData, ILogger logger) : base(sourceData, logger)
		{
		}

		public new XmlWriterConfiguration GetXmlWriterConfiguration() => base.GetXmlWriterConfiguration();
		public new string FilePrefix => base.FilePrefix;
		public new string OutputFilename => base.OutputFilename;
		public new string DataSource => base.DataSource;
		public new UpdateType UpdateType => base.UpdateType;
		public new List<RefExchangeRateZZ> ConvertToRefModels() => base.ConvertToRefModels();
	}
}
