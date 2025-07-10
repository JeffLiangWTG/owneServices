using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Common;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Models;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.BEReferenceData.Business.Testing
{
	[TestFixture]
	sealed class TradeGroupBuilderTest
	{
		[Test]
		public void TestFilePrefix()
		{
			Assert.That(builder.FilePrefix, Is.EqualTo("BE_RefCusTradeGroup"));
		}

		[Test]
		public void TestSupportMultipleLanguages()
		{
			Assert.That(builder.SupportsMultipleLanguagesExposed, Is.EqualTo(true));
		}

		[Test]
		public void TestBuildXmlFile()
		{
			var models = new List<ITariffModel>
			{
				new GeographicalArea
				{
					GeographicalAreaId = "EU",
					Description = "Europe",
					StartDate = DateTime.MinValue,
				}
				.SetCountries(new[]
				{
					new GeographicalArea.GeographicalAreaCountry
					{
						HJID = "1",
						CountryCode = "BE",
						Description = "Belgium",
						StartDate = DateTime.MinValue,
						GeographicalAreaHjid = "BE",
					},
					new GeographicalArea.GeographicalAreaCountry
					{
						HJID = "2",
						CountryCode = "NL",
						Description = "Netherlands",
						StartDate = DateTime.MinValue,
						GeographicalAreaHjid = "NL",
					}
				})
				.SetDescriptions(new[]
				{
					new DescriptionPeriods.DescriptionModel
					{
						HJID = "1",
						LanguageCode = "NL",
						Description = "Europa",
					},
					new DescriptionPeriods.DescriptionModel
					{
						HJID = "2",
						LanguageCode = "EN",
						Description = "Europe",
					}
				}),
				new GeographicalArea
				{
					GeographicalAreaId = "BE",
					Description = "Belgium",
					StartDate = DateTime.MinValue,
				}
				.SetCountries(new[]
				{
					new GeographicalArea.GeographicalAreaCountry
					{
						HJID = "1",
						CountryCode = "BE",
						Description = "Belgium",
						StartDate = DateTime.MinValue,
						GeographicalAreaHjid = "BE",
					}
				})
				.SetDescriptions(new[]
				{
					new DescriptionPeriods.DescriptionModel
					{
						HJID = "1",
						LanguageCode = "NL",
						Description = "Belgie",
					},
					new DescriptionPeriods.DescriptionModel
					{
						HJID = "2",
						LanguageCode = "FR",
						Description = "Belgique",
					},
					new DescriptionPeriods.DescriptionModel
					{
						HJID = "3",
						LanguageCode = "EN",
						Description = "Belgium",
					},
					new DescriptionPeriods.DescriptionModel
					{
						HJID = "4",
						LanguageCode = "DE",
						Description = "Belgien",
					},
				}),
			};

			var errorCollector = new StringBuilder();
			var dateTimeProvider = new Mock<IDateTimeProvider>();
			dateTimeProvider.Setup(x => x.UTCDateTime).Returns(new DateTime(2019, 11, 04, 22, 36, 45, 135));
			var builder = new TradeGroupBuilderTester(dateTimeProvider.Object, errorCollector);

			var publicationDate = new DateTime(2019, 11, 03, 13, 14, 15, 678, DateTimeKind.Utc);

			builder.BuildXml(publicationDate, models, TempFolder, "");

			var fileName = Path.Combine(TempFolder, builder.OutputFileName);
			Assert.That(File.Exists(fileName));

			var xml = File.ReadAllText(fileName);
			var expectedXml = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.BEReferenceData.Tests.Tariff.TestFiles.Output.TradeGroups_001.xml");
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
		public bool SupportsMultipleLanguagesExposed => base.SupportsMultipleLanguages;
	}
}
