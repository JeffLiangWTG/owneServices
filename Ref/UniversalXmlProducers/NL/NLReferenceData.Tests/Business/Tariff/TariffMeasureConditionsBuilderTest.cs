using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NLReferenceData.Business.Testing
{
	[TestFixture]
	sealed class TariffMeasureConditionsBuilderTest
	{
		[Test]
		public void ConvertXmlFileToRefCusConditionCode()
		{
			var errorCollector = new StringBuilder();
			var builder = new TariffMeasureConditionsBuilder(errorCollector);
			var resultList = builder.ConvertMeasureConditionsToRefCusConditionCode(ReferenceData);

			Assert.Multiple(() =>
			{
				Assert.IsNotNull(resultList, "Measure Condition nodes could not be converted, no results are returned");
				Assert.AreEqual(2, resultList.Count, "Not all Measure nodes could be converted to RefCusTariff");
				Assert.AreEqual("02", resultList[0].ZY7_ConditionCode, "RefCusConditionCode is generated with wrong Condition Code");
				Assert.AreEqual("NL", resultList[0].ZY7_ZZZ_NKDataGrouping, "RefCusConditionCode is generated with wrong DataGrouping Code");
				Assert.AreEqual("Presentation of a certificate/licence/document", resultList[0].ZY7_Description, "RefCusConditionCode is generated with wrong Description");
				Assert.AreEqual(1, resultList[0].RefCusConditionCodeLanguages.Length, "Not all descriptions are converted to RefCusConditionCodeLanguages");
				Assert.That(resultList[0].RefCusConditionCodeLanguages[0].ZY8_ZX6_NKLanguage, Is.EqualTo("NL"), "RefCusConditionCode-RefCusConditionLanguage is generated with wrong Language");
				Assert.AreEqual("Overleggen van een certificaat/vergunning/document", resultList[0].RefCusConditionCodeLanguages[0].ZY8_Description, "RefCusConditionCode-RefCusConditionLanguage is generated with wrong Description");
			});
		}

		[Test]
		public void InvalidDataInXml()
		{
			var errorCollector = new StringBuilder();
			var builder = new TariffMeasureConditionsBuilder(errorCollector);
			var refDataCollection = builder.ConvertMeasureConditionsToRefCusConditionCode(InvalidData);

			Assert.Multiple(() =>
			{
				Assert.That(errorCollector.ToString().Contains("RefCusConditionCode validation error: Key '_NL' Errors: ZY7_ConditionCode is required."), "Invalid data (missing condition code) is not detected.");
				Assert.That(errorCollector.ToString().Contains("RefCusConditionCode validation error: Key '02_EUN' Errors: ZY7_Description is required."), "Invalid data (missing EN-description) is not detected.");
				Assert.That(errorCollector.ToString().Contains("RefCusConditionCode validation error: Key '03_NL' Errors: ZY8_ZX6_NKLanguage is required."), "Invalid data (missing description language) is not detected.");
				Assert.That(errorCollector.ToString().Contains("RefCusConditionCode validation error: Key '04_NL' Errors: ZY8_Description is required."), "Invalid data (missing description) is not detected.");
				Assert.That(errorCollector.ToString().Contains("RefCusConditionCode validation error: Key '05_NL' Errors: RefCusCondition already exist, no duplicate entry is created."), "Invalid data (duplicate entries) is not detected.");
				Assert.AreEqual(1, refDataCollection.Count, "Resultset contains invalid data, no items should be in resultset");
			});
		}

		[Test]
		public void GenerateUniversalReferenceDataXml()
		{
			var errorCollector = new StringBuilder();
			var builder = new TariffMeasureConditionsBuilder(errorCollector);
			string expectedXml, generatedXml = string.Empty;

			var content = builder.ConvertMeasureConditionsToRefCusConditionCode(ReferenceData);
			TariffMeasureConditionsBuilder.GenerateUniversalReferenceDataXml(content, new DateTime(2024, 11, 07, 13, 26, 09), TempFolder);

			var expectedFileName = Path.Combine(TempFolder, "RefCusConditionCode_NL MeasureConditioncode_132609000.xml");
			if (File.Exists(expectedFileName))
			{
				generatedXml = File.ReadAllText(expectedFileName);
			}

			expectedXml = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.NLReferenceData.Tests.TestFiles.Tariff.Output.RefCusConditionCodes.xml");

			Assert.IsTrue(File.Exists(expectedFileName), $"File with name '{expectedFileName}' could not be found");
			Assert.AreEqual(expectedXml, generatedXml, "Generated Universal Reference Xml does not match the correct format.");
		}

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			ReferenceData = new List<MeasureCondition>()
			{
				new MeasureCondition()
				{
					ConditionCode = "02",
					National = "1",
					DateStart = new DateTime(1972, 01, 01),
					Type = "1",
					ChangeType = "U",
					Descriptions = new List<MeasureConditionDescription>()
					{
						new MeasureConditionDescription()
						{
							Description = "Presentation of a certificate/licence/document",
							Language = "EN",
							National = "1",
						},
						new MeasureConditionDescription()
						{
							Description = "Overleggen van een certificaat/vergunning/document",
							Language = "NL",
							National = "1",
						},
					}
				},
				new MeasureCondition()
				{
					ConditionCode = "J",
					National = "0",
					DateStart = new DateTime(2021, 03, 01),
					Type = "",
					ChangeType = "U",
					Descriptions = new List<MeasureConditionDescription>()
					{
						new MeasureConditionDescription()
						{
							Description = "Declared net mass (box 38 - data element 6/1) higer than the condition components",
							Language = "EN",
							National = "0",
						},
						new MeasureConditionDescription()
						{
							Description = "Declared net mass (box 38 - data element 6/1) higer than the condition components",
							Language = "NL",
							National = "1",
						},
					}
				},
			};

			InvalidData = new List<MeasureCondition>()
			{
				new MeasureCondition()
				{
					ConditionCode = "",
					National = "1",
					DateStart = new DateTime(1972, 01, 01),
					Type = "1",
					ChangeType = "U",
					Descriptions = new List<MeasureConditionDescription>()
					{
						new MeasureConditionDescription()
						{
							Description = "Presentation of a certificate/licence/document",
							Language = "EN",
							National = "1",
						},
						new MeasureConditionDescription()
						{
							Description = "Overleggen van een certificaat/vergunning/document",
							Language = "NL",
							National = "1",
						},
					}
				},
				new MeasureCondition()
				{
					ConditionCode = "02",
					National = "0",
					DateStart = new DateTime(1972, 01, 01),
					Type = "1",
					ChangeType = "U",
					Descriptions = new List<MeasureConditionDescription>()
					{
						new MeasureConditionDescription()
						{
							Description = "",
							Language = "EN",
							National = "1",
						},
						new MeasureConditionDescription()
						{
							Description = "Overleggen van een certificaat/vergunning/document",
							Language = "NL",
							National = "1",
						},
					}
				},
				new MeasureCondition()
				{
					ConditionCode = "03",
					National = "1",
					DateStart = new DateTime(1972, 01, 01),
					Type = "1",
					ChangeType = "U",
					Descriptions = new List<MeasureConditionDescription>()
					{
						new MeasureConditionDescription()
						{
							Description = "Presentation of a certificate/licence/document",
							Language = "EN",
							National = "1",
						},
						new MeasureConditionDescription()
						{
							Description = "Overleggen van een certificaat/vergunning/document",
							Language = "",
							National = "1",
						},
					}
				},
				new MeasureCondition()
				{
					ConditionCode = "04",
					National = "1",
					DateStart = new DateTime(1972, 01, 01),
					Type = "1",
					ChangeType = "U",
					Descriptions = new List<MeasureConditionDescription>()
					{
						new MeasureConditionDescription()
						{
							Description = "Presentation of a certificate/licence/document",
							Language = "EN",
							National = "1",
						},
						new MeasureConditionDescription()
						{
							Description = "",
							Language = "NL",
							National = "1",
						},
					}
				},
				new MeasureCondition()
				{
					ConditionCode = "05",
					National = "1",
					DateStart = new DateTime(1972, 01, 01),
					Type = "1",
					ChangeType = "U",
					Descriptions = new List<MeasureConditionDescription>()
					{
						new MeasureConditionDescription()
						{
							Description = "Presentation of a certificate/licence/document",
							Language = "EN",
							National = "1",
						},
						new MeasureConditionDescription()
						{
							Description = "Overleggen van een certificaat/vergunning/document",
							Language = "NL",
							National = "1",
						},
					}
				},
				new MeasureCondition()
				{
					ConditionCode = "05",
					National = "1",
					DateStart = new DateTime(1972, 01, 01),
					Type = "1",
					ChangeType = "U",
					Descriptions = new List<MeasureConditionDescription>()
					{
						new MeasureConditionDescription()
						{
							Description = "Presentation of a certificate/licence/document",
							Language = "EN",
							National = "1",
						},
						new MeasureConditionDescription()
						{
							Description = "Overleggen van een certificaat/vergunning/document",
							Language = "NL",
							National = "1",
						},
					}
				},
			};

			TempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			Directory.CreateDirectory(TempFolder);
		}

		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			if (Directory.Exists(TempFolder))
			{
				Directory.Delete(TempFolder, true);
			}
		}

		string TempFolder;
		List<MeasureCondition> ReferenceData, InvalidData;
	}
}
