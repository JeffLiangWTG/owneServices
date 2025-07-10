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
	class AdditionalCodeBuilderTests
	{
		[Test]
		public void FilePrefix()
		{
			Assert.That(builder.FilePrefix, Is.EqualTo("GB_RefCusCodeList"));
		}

		[Test]
		public void BuildXmlFile()
		{
			var models = new List<ITariffModel>
			{
				new AdditionalCode { CodeType = "A", Code = "111", Description = "Item 1" },
				new AdditionalCode { CodeType = "B", Code = "222", Description = "Item 2", StartDate = new DateTime(2020, 01, 01), EndDate = new DateTime(2020, 12, 31, 23, 59, 59) },
				new AdditionalCode { CodeType = "A", Code = "111", Description = "Duplicate" },
				new AdditionalCode { CodeType = "B", Code = "111", Description = "Item 3" },
				new AdditionalCode { Code = "", Description = "No Code" },
				new AdditionalCode { Code = "NoDescrip", Description = "" },
			};

			var dateTimeProvider = new Mock<IDateTimeProvider>();
			dateTimeProvider.Setup(x => x.UTCDateTime).Returns(new DateTime(2019, 11, 04, 22, 36, 45, 135));
			var errorCollector = new StringBuilder();
			var builder = new AdditionalCodeBuilderTester(dateTimeProvider.Object, errorCollector);

			var publicationDate = new DateTime(2019, 11, 03, 13, 14, 15, 678, DateTimeKind.Utc);

			builder.BuildXml(publicationDate, models, TempFolder, "");

			var fileName = Path.Combine(TempFolder, builder.OutputFileName);
			Assert.That(File.Exists(fileName));

			var xml = File.ReadAllText(fileName);
			var expectedXml = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.GBReferenceData.Tests.Tariff.TestFiles.Output.RefCusCodeList_001.xml");
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
			builder = new AdditionalCodeBuilderTester(dateTimeProvider.Object, errorCollector);
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

		AdditionalCodeBuilderTester builder;
		StringBuilder errorCollector;
		string TempFolder;
	}

	internal class AdditionalCodeBuilderTester : AdditionalCodeBuilder
	{
		public AdditionalCodeBuilderTester(IDateTimeProvider dateTimeProvider, StringBuilder errorCollector) : base(dateTimeProvider, errorCollector)
		{
		}

		public new string FilePrefix => base.FilePrefix;
		public new string OutputFileName => base.OutputFileName;
	}
}
