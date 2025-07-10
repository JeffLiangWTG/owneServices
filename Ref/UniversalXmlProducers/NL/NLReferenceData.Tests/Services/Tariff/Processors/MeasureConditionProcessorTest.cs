using System;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.NLReferenceData.Business.Testing;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NLReferenceData.Services.Testing
{
	[TestFixture]
	sealed class MeasureConditionProcessorTest
	{
		[Test]
		public void ConvertXmlElementToModel()
		{
			var processor = new MeasureConditionProcessor();
			var element = TestHelper.GetXmlElement("measureConditionCode", "CargoWise.RefDbRepo.NLReferenceData.Tests.TestFiles.Tariff.Input.UT_MeasureCondition_001.xml");
			var model = processor.ConvertXElementToModel(element);

			Assert.Multiple(() =>
			{
				Assert.That(model.ConditionCode, Is.EqualTo("02"), "ConditionCode");
				Assert.That(model.National, Is.EqualTo("1"), "National");
				Assert.That(model.DateStart, Is.EqualTo(new DateTime(1972, 01, 01)), "DateStart");
				Assert.That(model.Type, Is.EqualTo("1"), "Type");
				Assert.That(model.ChangeType, Is.EqualTo("U"), "ChangeType");
				Assert.That(model.Descriptions, Is.Not.Null.And.Not.Empty, "Descriptions");

				var descriptionModel = model.Descriptions.FirstOrDefault();
				Assert.That(descriptionModel.Description, Is.EqualTo("Presentation of a certificate/licence/document"), "Description");
				Assert.That(descriptionModel.Language, Is.EqualTo("EN"), "Language");
				Assert.That(descriptionModel.National, Is.EqualTo("1"), "National");
			});
		}


		[Test]
		public void LoadData()
		{
			var filePath = Path.Combine(ContentFolder, "UT_Measure_001.xml");
			TestHelper.SimulateDownload(filePath, "CargoWise.RefDbRepo.NLReferenceData.Tests.TestFiles.Tariff.Input.UT_MeasureCondition_001.xml");
			var processor = new MeasureConditionProcessor();
			processor.LoadData(filePath);

			Assert.That(processor.MeasureConditionCodes.Count, Is.EqualTo(4), "All Measure Condition elements from XML loaded");
		}

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{

			TempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			Directory.CreateDirectory(TempFolder);
		}

		[SetUp]
		public void SetUp()
		{
			ContentFolder = Path.Combine(TempFolder, Path.GetRandomFileName());
			Directory.CreateDirectory(ContentFolder);
		}

		[OneTimeTearDown]
		public void TearDown()
		{
			if (Directory.Exists(TempFolder))
			{
				Directory.Delete(TempFolder, true);
			}
		}

		string TempFolder;
		string ContentFolder;
	}
}
