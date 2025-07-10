using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Common;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Models;
using CargoWise.RefDbRepo.SharedReferenceData.Tests;
using Moq;
using NUnit.Framework;
using static CargoWise.RefDbRepo.SharedReferenceData.Business.Tariff.Helpers.Tests.TestHelperClasses;

namespace CargoWise.RefDbRepo.SharedReferenceData.Business.Tariff.Tests
{
	[TestFixture]
	class BuilderBaseTests
	{
		[Test]
		public void OutputFileName()
		{
			builder = new BuilderBaseTester(dateTimeProvider.Object, errorCollector);
			builder.SimulateFileWrite();
			dateTimeProvider.Setup(x => x.UTCDateTime).Returns(new DateTime(2019, 10, 29, 15, 14, 13, 876));
			Assert.That(builder.OutputFileName, Is.EqualTo("UnitTest_00001_151413876.xml"));

			builder.SimulateFileWrite();
			dateTimeProvider.Setup(x => x.UTCDateTime).Returns(new DateTime(2019, 10, 29, 16, 17, 18, 999));
			Assert.That(builder.OutputFileName, Is.EqualTo("UnitTest_00002_161718999.xml"));
		}

		[Test]
		public void NullConstructorArgs()
		{
			Assert.Throws(Is.TypeOf<ArgumentNullException>().And.Message.Contains("Value cannot be null.").And.Message.Contains("(Parameter 'dateTimeProvider')"), () => new BuilderBaseTester(null, errorCollector));
			Assert.Throws(Is.TypeOf<ArgumentNullException>().And.Message.Contains("Value cannot be null.").And.Message.Contains("(Parameter 'errorCollector')"), () => new BuilderBaseTester(dateTimeProvider.Object, null));
		}

		[Test]
		public void BuildXml()
		{
			var models = new List<ITariffModel>()
			{
				new TestTariffModel { Id = "ValidAndUnique", Valid = true, EndDate = DateTime.Today },
				new TestTariffModel { Id = "ValidNotUnique", Valid = true, EndDate = DateTime.Today },
				new TestTariffModel { Id = "ValidNotUnique", Valid = true, EndDate = DateTime.Today },
				new TestTariffModel { Id = "NotValidUnique", Valid = false, EndDate = DateTime.Today },
				new TestTariffModel { Id = "ExpiredWillBeSkipped", Valid = true, EndDate = DateTime.MinValue },
			};

			errorCollector.Clear();

			dateTimeProvider.Setup(x => x.UTCDateTime).Returns(new DateTime(2019, 11, 04, 22, 36, 45, 135));
			var publicationDate = new DateTime(2019, 11, 03, 13, 14, 15, 678, DateTimeKind.Utc);

			builder.BuildXml(publicationDate, models, TempFolder, "UnitTest");

			Assert.That(builder.ErrorCollector.ToString(), Contains.Substring("IsValid[NotValidUnique]"));
			Assert.That(builder.ErrorCollector.ToString(), Contains.Substring("Duplicate[ValidNotUnique]"));

			var fileName = Path.Combine(TempFolder, builder.OutputFileName);
			Assert.That(File.Exists(fileName));

			var xml = File.ReadAllText(fileName);
			var expectedXml = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.SharedReferenceData.Tests.Business.Tariff.TestFiles.Output.UnitTest_000000000.xml");
			Assert.That(xml, Is.EqualTo(expectedXml));
		}

		[Test]
		public void XMLDataSourceLengthGreaterThan75()
		{
			var models = new List<ITariffModel>()
			{
				new TestTariffModel { Id = "ValidAndUnique", Valid = true, EndDate = DateTime.Today }
			};

			errorCollector.Clear();

			dateTimeProvider.Setup(x => x.UTCDateTime).Returns(new DateTime(2019, 11, 04, 22, 36, 45, 135));
			var publicationDate = new DateTime(2019, 11, 03, 13, 14, 15, 678, DateTimeKind.Utc);

			builder.BuildXml(publicationDate, models, TempFolder, "Thisisaridiculouslylongdatasourcenamewhichwouldcomefromanxmlfilenamethatgetsextractedfromazip");

			var fileName = Path.Combine(TempFolder, builder.OutputFileName);
			Assert.That(File.Exists(fileName));

			var xml = File.ReadAllText(fileName);

			Assert.That(xml, Does.Contain("<DataSource>UnitTest_Filter_Thisisaridiculouslylongdatasourcenamewhichwouldcomefromanxm</DataSource>"));
		}

		[Test]
		public void XmlDataSource()
		{
			var models = new List<ITariffModel>()
			{
				new TestTariffModel { Id = "ValidAndUnique", Valid = true, EndDate = DateTime.Today }
			};

			errorCollector.Clear();

			dateTimeProvider.Setup(x => x.UTCDateTime).Returns(new DateTime(2019, 11, 04, 22, 36, 45, 135));
			var publicationDate = new DateTime(2019, 11, 03, 13, 14, 15, 678, DateTimeKind.Utc);

			builder.BuildXml(publicationDate, models, TempFolder, "5");

			var fileName = Path.Combine(TempFolder, builder.OutputFileName);
			Assert.That(File.Exists(fileName));

			var xml = File.ReadAllText(fileName);

			Assert.That(xml, Does.Contain("<DataSource>UnitTest_Filter_5</DataSource>"));

			builder.BuildXml(publicationDate, models, TempFolder, string.Empty);

			fileName = Path.Combine(TempFolder, builder.OutputFileName);
			Assert.That(File.Exists(fileName));

			xml = File.ReadAllText(fileName);

			Assert.That(xml, Does.Contain("<DataSource>UnitTest</DataSource>"));
		}

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			TempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			dateTimeProvider = new Mock<IDateTimeProvider>();
			errorCollector = new StringBuilder();
			builder = new BuilderBaseTester(dateTimeProvider.Object, errorCollector);
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

		Mock<IDateTimeProvider> dateTimeProvider;
		BuilderBaseTester builder;
		StringBuilder errorCollector;
		string TempFolder;
	}
}
