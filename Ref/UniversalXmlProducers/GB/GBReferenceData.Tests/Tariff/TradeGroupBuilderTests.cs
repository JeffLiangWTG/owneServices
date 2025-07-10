using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CargoWise.RefDbRepo.GBReferenceData.Business.Tariff;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Common;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Models;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.GBReferenceData.Tests.Tariff
{
	[TestFixture]
	class TradeGroupBuilderTests
	{
		[Test]
		public void FilePrefix()
		{
			Assert.That(builder.FilePrefix, Is.EqualTo("GB_RefCusTradeGroup"));
		}

		[Test]
		public void BuildXmlFile()
		{
			var models = new List<ITariffModel>()
			{
				new GeographicalArea
				{
					GeographicalAreaId = "1011",
					StartDate = new DateTime(2019, 10, 01, 15, 14, 13),
					EndDate = new DateTime(2019, 12, 31, 23, 59, 59),
					Description = "Test Model Convertion",
				}
				.SetCountries(new[]
				{
					new GeographicalArea.GeographicalAreaCountry { HJID = "1", CountryCode = "ZA", StartDate = new DateTime(2020, 01, 01), EndDate = new DateTime(2021, 12, 31, 23, 59, 59), Description = "South Africa" },
					new GeographicalArea.GeographicalAreaCountry { HJID = "2", CountryCode = "AU", StartDate = new DateTime(2020, 01, 01), EndDate = new DateTime(2021, 12, 31, 23, 59, 59), Description = "Australia" }
				}),
				new GeographicalArea { GeographicalAreaId = "ZA", Description = "South Africa", StartDate = new DateTime(2020, 01, 01), EndDate = new DateTime(2021, 12, 31, 23, 59, 59) },
				new GeographicalArea { GeographicalAreaId = "ZA", Description = "Duplicate", StartDate = new DateTime(2020, 01, 01), EndDate = new DateTime(2021, 12, 31, 23, 59, 59) },
				new GeographicalArea { GeographicalAreaId = "GB", Description = "No Start Date", EndDate = new DateTime(2021, 12, 31, 23, 59, 59) },
				new GeographicalArea { GeographicalAreaId = "US", Description = "No End Date", StartDate = new DateTime(2020, 01, 01) },
				new GeographicalArea { GeographicalAreaId = "", Description = "Validation error 1" },
				new GeographicalArea { GeographicalAreaId = "FR", Description = "Validation error 2" }.SetCountries(new[] { new GeographicalArea.GeographicalAreaCountry { HJID = "1", CountryCode = "" } }),
				new GeographicalArea { GeographicalAreaId = "404", Description = "No trade group country" },
			};

			var dateTimeProvider = new Mock<IDateTimeProvider>();
			dateTimeProvider.Setup(x => x.UTCDateTime).Returns(new DateTime(2019, 11, 04, 22, 36, 45, 135));
			var errorCollector = new StringBuilder();
			var builder = new TradeGroupBuilderTester(dateTimeProvider.Object, errorCollector);

			var publicationDate = new DateTime(2019, 11, 03, 13, 14, 15, 678, DateTimeKind.Utc);

			builder.BuildXml(publicationDate, models, TempFolder, "");

			var fileName = Path.Combine(TempFolder, builder.OutputFileName);
			Assert.That(File.Exists(fileName));

			var xml = File.ReadAllText(fileName);
			var expectedXml = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.GBReferenceData.Tests.Tariff.TestFiles.Output.RefCusTradeGroup_001.xml");
			Assert.That(xml, Is.EqualTo(expectedXml));
		}

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			TempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			var dateTimeProvider = new Mock<IDateTimeProvider>();
			dateTimeProvider.Setup(x => x.UTCDateTime).Returns(new DateTime(2020, 11, 01, 0, 0, 0, 0));
			dateTimeProvider.Setup(x => x.UTCHistoricalDate).Returns(new DateTime(2019, 11, 01, 0, 0, 0, 0));
			errorCollector = new StringBuilder();
			builder = new TradeGroupBuilderTester(dateTimeProvider.Object, errorCollector);
		}

		[OneTimeTearDown]
		public void TearDown()
		{
			if (Directory.Exists(TempFolder))
			{
				Directory.Delete(TempFolder, true);
			}
		}

		[SetUp]
		public void Setup()
		{
			errorCollector.Clear();
		}

		TradeGroupBuilderTester builder;
		StringBuilder errorCollector;
		string TempFolder;
	}

	internal class TradeGroupBuilderTester : TradeGroupBuilder
	{
		public TradeGroupBuilderTester(IDateTimeProvider dateTimeProvider, StringBuilder errorCollector) : base(dateTimeProvider, errorCollector)
		{
		}

		public new string FilePrefix => base.FilePrefix;

		public new string OutputFileName => base.OutputFileName;
	}
}
