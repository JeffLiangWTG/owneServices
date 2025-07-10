using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.GBReferenceData.Business.ExchangeRates;
using CargoWise.RefDbRepo.GBReferenceData.Services.Common;
using CargoWise.RefDbRepo.GBReferenceData.Services.ExchangeRates;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Common;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.GBReferenceData.Tests.ExchangeRates
{
	[TestFixture]
	public class ExchangeRateBuilderTests
	{
		[Test]
		public void XmlWriterConfirguration()
		{
			var config = builder.XmlWriterConfiguration_Exposed();

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
		}

		[Test]
		public void ConvertExchangeRateModelToRefExchangeRate()
		{
			var rates = new[]
			{
				new ExchangeRate { Currency = "AUD", StartDate = new DateTime(2022, 1, 1), EndDate = new DateTime(2022, 1, 31, 23, 59, 59), Rate = 1.8874m },
				new ExchangeRate { Currency = "EUR", StartDate = new DateTime(2022, 2, 1), EndDate = new DateTime(2022, 2, 28, 23, 59, 0), Rate = 1.2013m }
			};

			var refModels = builder.ConvertToRefModels_Exposed(rates).ToList();

			Assert.That(refModels, Is.Not.Null);
			Assert.That(refModels.Count, Is.EqualTo(2));

			Assert.That(refModels[0].ZZN_RX_NKExCurrency, Is.EqualTo("AUD"));
			Assert.That(refModels[0].ZZN_StartDate, Is.EqualTo(new DateTime(2022, 1, 1)));
			Assert.That(refModels[0].ZZN_EndDate, Is.EqualTo(new DateTime(2022, 1, 31, 23, 59, 0)));
			Assert.That(refModels[0].ZZN_Rate, Is.EqualTo(1.8874m));

			Assert.That(refModels[1].ZZN_RX_NKExCurrency, Is.EqualTo("EUR"));
		}

		[Test]
		public void XmlFileContent()
		{
			var publicationDate = new DateTime(2022, 2, 3, 14, 15, 16);
			var rates = new[]
			{
				new ExchangeRate { Currency = "AUD", StartDate = new DateTime(2022, 1, 1), EndDate = new DateTime(2022, 1, 31, 23, 59, 59), Rate = 1.8874m },
				new ExchangeRate { Currency = "EUR", StartDate = new DateTime(2022, 2, 1), EndDate = new DateTime(2022, 2, 28, 23, 59, 59), Rate = 1.2013m },
				new ExchangeRate { Currency = "ZAR", StartDate = new DateTime(2022, 2, 1), EndDate = new DateTime(2022, 2, 28, 23, 59, 59), Rate = 20.8626m }
			};

			builder.BuildXml_Exposed(publicationDate, rates, TempFolder);

			var fileName = Path.Combine(TempFolder, builder.GetOutputFileName_Exposed(publicationDate));
			Assert.That(File.Exists(fileName));

			var xml = File.ReadAllText(fileName);
			var expectedXml = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.GBReferenceData.Tests.ExchangeRates.TestFiles.Output.RefExchangeRates_01.xml");
			Assert.That(xml, Is.EqualTo(expectedXml));
		}

		[Test]
		public void XMLWriterDataSource()
		{
			Assert.That(builder.XMLWriterDataSource_Exposed, Is.EqualTo("GB Exchange Rates"));
		}

		[Test]
		public void GetOutputFileName()
		{
			var pDate = new DateTime(2022, 2, 3, 15, 16, 17);
			var outputFileName = builder.GetOutputFileName_Exposed(pDate);
			Assert.That(outputFileName, Is.EqualTo("GBExchangeRates_20220203151617.xml"));
		}

		[Test]
		public void RunProcess()
		{
			var errorCollector = new StringBuilder();
			var dateTimeProvider = new SharedReferenceData.Business.Common.Tests.CommonHelpers.DateTimeProvider();
			var webClientWrapper = new Mock<IWebClientWrapper>();

			dateTimeProvider.TestDateTime = new DateTime(2022, 1, 27);

			var janRates = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.GBReferenceData.Tests.ExchangeRates.TestFiles.Input.ExchangeRates_0122.xml");
			var febRates = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.GBReferenceData.Tests.ExchangeRates.TestFiles.Input.ExchangeRates_0222.xml");

			var janDate = new DateTime(2022, 1, 31, 13, 14, 15);
			var febDate = new DateTime(2022, 2, 3, 16, 15, 14);

			webClientWrapper.Setup(x => x.GetDatedContent("https://www.trade-tariff.service.gov.uk/api/v2/exchange_rates/files/monthly_xml_2022-01.xml")).Returns((janDate, janRates));
			webClientWrapper.Setup(x => x.GetDatedContent("https://www.trade-tariff.service.gov.uk/api/v2/exchange_rates/files/monthly_xml_2022-02.xml")).Returns((febDate, febRates));

			var builder = new ExchangeRateBuilderForTest(dateTimeProvider, errorCollector, webClientWrapper.Object);

			builder.RunProcess(TempFolder);

			var fileName = Path.Combine(TempFolder, builder.GetOutputFileName_Exposed(febDate));
			Assert.That(File.Exists(fileName));
		}

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			TempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			builder = new ExchangeRateBuilderForTest();
		}

		[OneTimeTearDown]
		public void TearDown()
		{
			if (Directory.Exists(TempFolder))
			{
				Directory.Delete(TempFolder, true);
			}
		}
		ExchangeRateBuilderForTest builder;
		string TempFolder;
	}

	class ExchangeRateBuilderForTest : ExchangeRateBuilder
	{
		public ExchangeRateBuilderForTest() : this(null, null, null) { }
		public ExchangeRateBuilderForTest(IDateTimeProvider dateTimeProvider, StringBuilder errorCollector, IWebClientWrapper webClientWrapper) : base(dateTimeProvider, errorCollector, webClientWrapper) { }

		public XmlWriterConfiguration XmlWriterConfiguration_Exposed() => XmlWriterConfiguration();
		public IEnumerable<RefExchangeRateZZ> ConvertToRefModels_Exposed(IEnumerable<ExchangeRate> data) => ConvertToRefModels(data);
		public string XMLWriterDataSource_Exposed => XMLWriterDataSource;
		public string GetOutputFileName_Exposed(DateTime publicationDate) => GetOutputFileName(publicationDate);
		public void BuildXml_Exposed(DateTime publicationDate, IEnumerable<ExchangeRate> data, string outputPath) => BuildXml(publicationDate, data, outputPath);
	}
}
