using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Models;
using CargoWise.RefDbRepo.SharedReferenceData.Tests;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Tests
{
	[TestFixture]
	sealed class XmlElementReaderTests
	{
		[TestCase]
		public void ProcessXml()
		{
			var filePath1 = Path.Combine(tempFolder, "Content_001.xml");
			var filePath2 = Path.Combine(tempFolder, "Content_002.xml");
			var filePath3 = Path.Combine(tempFolder, "Content_003.xml");
			TestHelper.SimulateDownload(filePath1, "CargoWise.RefDbRepo.SharedReferenceData.Tests.Services.Tariff.TestFiles.Input.Content_001.xml");
			TestHelper.SimulateDownload(filePath2, "CargoWise.RefDbRepo.SharedReferenceData.Tests.Services.Tariff.TestFiles.Input.Content_002.xml");
			TestHelper.SimulateDownload(filePath3, "CargoWise.RefDbRepo.SharedReferenceData.Tests.Services.Tariff.TestFiles.Input.Content_003.xml");

			var file1 = new FileDetails { Content = new ContentDetails { ExecutionDate = new DateTime(2019, 10, 01, 14, 15, 16) }, Filename = filePath1 };
			var file2 = new FileDetails { Content = new ContentDetails { ExecutionDate = new DateTime(2019, 10, 02, 14, 15, 16) }, Filename = filePath2 };
			var file3 = new FileDetails { Content = new ContentDetails { ExecutionDate = new DateTime(2019, 10, 03, 14, 15, 16) }, Filename = filePath3 };
			var files = new List<FileDetails>() { file1, file2, file3 };

			var errorCollector = new StringBuilder();

			var processedFiles = new List<string>();

			var models = reader.ProcessXml("1", files, errorCollector, (file) => { processedFiles.Add(file.Filename); });

			Assert.That(models, Is.Not.Null.And.Not.Empty);
			Assert.That(models.Count, Is.EqualTo(5));
			Assert.That(processedFiles.Count, Is.EqualTo(2));
			Assert.That(processedFiles[0], Is.EqualTo(filePath1));
			Assert.That(processedFiles[1], Is.EqualTo(filePath3));

			Assert.That(!models.Any(x => x.HJID == "8835558"), "Chapter Filtering");
			Assert.That(!models.Any(x => x.HJID == "8835486"), "Model Validation Filtering");
			Assert.That(!models.Any(x => x.HJID == "8835726"), "Element Validation Filtering");

			Assert.That(errorCollector.ToString(), Is.EqualTo("Not valid for source: Content_001.xml\r\nNot valid for source: Content_003.xml\r\n"));
		}

		[TestCase]
		public void LoadMetaInfo()
		{
			var model = new XmlElementReaderTariffModel();
			var element = XElement.Parse("<x><nometainfo><opType>U</opType></nometainfo></x>");
			XmlElementReaderForTest.LoadMetaInfo(model, element);

			Assert.That(model.OpType, Is.Null.And.Not.Empty, "OpType not set");
			Assert.That(model.OpDate, Is.Null, "OpDate not set");

			element = XElement.Parse("<xxx><metainfo><opType>U</opType><origin>N</origin><status>L</status><transactionDate>2019-10-04T11:07:54</transactionDate></metainfo></xxx>");
			XmlElementReaderForTest.LoadMetaInfo(model, element);

			Assert.That(model.OpType, Is.EqualTo("U"), "OpType should be set");
			Assert.That(model.OpDate, Is.EqualTo(new DateTime(2019, 10, 4, 11, 7, 54)), "OpDate should be set");

			element = XElement.Parse("<xxx><metainfo><transactionDate>2019-10-04T11:07:54</transactionDate></metainfo></xxx>");
			XmlElementReaderForTest.LoadMetaInfo(model, element);

			Assert.That(model.OpType, Is.EqualTo(string.Empty), "OpType should be defaulted if missing");
		}

		[TestCase]
		public void GetDateTimeFromElement()
		{
			var element = XElement.Parse("<x><valExists>2019-10-04T11:07:54</valExists><bad>not a date</bad></x>");

			var val = XmlElementReader.GetDateTimeFromElement(element, "valNotExists");
			Assert.That(val, Is.Null, "Element does not exist");

			val = XmlElementReader.GetDateTimeFromElement(element, "valExists");
			Assert.That(val, Is.Not.Null.And.EqualTo(new DateTime(2019, 10, 4, 11, 7, 54)), "Element found and parsed");

			Assert.Throws(typeof(FormatException), () => { val = XmlElementReader.GetDateTimeFromElement(element, "bad"); }, "Bad data");
		}

		[TestCase]
		public void UpdateDateTimeFromElementIfPresent()
		{
			var element = XElement.Parse("<x><valExists>2019-10-04T11:07:54</valExists><bad>not a date</bad></x>");

			DateTime? val = new DateTime(2011, 12, 13);
			XmlElementReader.UpdateDateTimeFromElementIfPresent(element, "valNotExists", x => val = x);
			Assert.That(val, Is.EqualTo(new DateTime(2011, 12, 13)), "Element does not exist");

			XmlElementReader.UpdateDateTimeFromElementIfPresent(element, "valExists", x => val = x);
			Assert.That(val, Is.EqualTo(new DateTime(2019, 10, 4, 11, 7, 54)), "Element found and parsed");

			Assert.Throws(typeof(FormatException), () => XmlElementReader.UpdateDateTimeFromElementIfPresent(element, "bad", x => val = x), "Bad data");
		}

		[TestCase]
		public void UpdateDecimalFromElementIfPresent()
		{
			var element = XElement.Parse("<x><valExists>123.456</valExists><bad>not a decimal</bad></x>");

			var val = 42.0m;
			XmlElementReader.UpdateDecimalFromElementIfPresent(element, "valNotExists", x => val = x);
			Assert.That(val, Is.EqualTo(42.0m), "Element does not exist");

			XmlElementReader.UpdateDecimalFromElementIfPresent(element, "valExists", x => val = x);
			Assert.That(val, Is.EqualTo(123.456m), "Element found and parsed");

			Assert.Throws(typeof(FormatException), () => XmlElementReader.UpdateDecimalFromElementIfPresent(element, "bad", x => val = x), "Bad data");
		}

		[TestCase]
		public void UpdateIntFromElementIfPresent()
		{
			var element = XElement.Parse("<x><valExists>123</valExists><bad>abc</bad></x>");

			var val = 42;
			XmlElementReader.UpdateIntFromElementIfPresent(element, "valNotExists", x => val = x);
			Assert.That(val, Is.EqualTo(42), "Element does not exist");

			XmlElementReader.UpdateIntFromElementIfPresent(element, "valExists", x => val = x);
			Assert.That(val, Is.EqualTo(123), "Element found and parsed");

			Assert.Throws(typeof(FormatException), () => XmlElementReader.UpdateIntFromElementIfPresent(element, "bad", x => val = x), "Bad data");
		}

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			reader = new XmlElementReaderForTest();
			tempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			Directory.CreateDirectory(tempFolder);
		}

		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			if (Directory.Exists(tempFolder))
			{
				Directory.Delete(tempFolder, true);
			}
		}

		XmlElementReaderForTest reader;
		string tempFolder;
	}

	class XmlElementReaderTariffModel : BaseModel, ICopyable<XmlElementReaderTariffModel>
	{
		public string ChapterForTest { get; set; } = string.Empty;
		public bool IsValidForTest { get; set; }

		public override bool IsChapterSpecific => throw new NotImplementedException();

		public XmlElementReaderTariffModel Copy()
		{
			return new XmlElementReaderTariffModel
			{
				HJID = HJID,
				OpType = OpType,
				OpDate = OpDate,
				IsValidForTest = IsValidForTest,
			};
		}

		public override bool IsInChapter(string chapterFilter) => chapterFilter == ChapterForTest;

		protected override bool IsValidCore(StringBuilder errorCollector, string source)
		{
			if (!IsValidForTest)
			{
				errorCollector.AppendLine(CultureInfo.InvariantCulture, $"Not valid for source: {source}");
			}

			return IsValidForTest;
		}
	}

	class XmlElementReaderForTest : XmlElementReader<XmlElementReaderTariffModel>
	{
		protected override string ParentElement => "findMeasureByDatesResponseHistory";

		protected override string ElementName => "Measure";

		protected override XmlElementReaderTariffModel CreateMinimumModelFromXElement(XElement element)
		{
			var model = new XmlElementReaderTariffModel();

			model.HJID = element.Element("hjid").Value;

			model.ChapterForTest = model.HJID == "8835558" ? "2" : "1";
			model.IsValidForTest = model.HJID != "8835486";

			return model;
		}

		protected override bool IsValidElement(XElement element)
		{
			var hjid = element.Element("hjid").Value;
			return hjid != "8835726";
		}
	}

}
