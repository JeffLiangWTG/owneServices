using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.RefDbRepo.ILReferenceData.Business.Schemas.CustomsTariff;
using CargoWise.RefDbRepo.ILReferenceData.Services;
using NUnit.Framework;
using XMLTools;

namespace CargoWise.RefDbRepo.ILReferenceData.Tests.Business
{
	[TestFixture]
	public class ILCustomsTariffProcessorTest
	{
		[Test]
		public void TestGenerateFiles()
		{
			var testFilesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\");
			ILReferenceData.Business.ApplicationConfig.Instance.OutputDirectory = Path.GetTempPath();

			var publicationDate = DateTime.ParseExact("20240220", "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);

			var customsItemDetailsHistory = new Dictionary<int, CustomsItemDetailsHistory>()
			{
				{ 1, new CustomsItemDetailsHistory() { ID = 1, EnglishGoodsDescription = "Description 1" } },
				{ 2, new CustomsItemDetailsHistory() { ID = 2, EnglishGoodsDescription = "Description 7" } },
				{ 3, new CustomsItemDetailsHistory() { ID = 3, EnglishGoodsDescription = "Description 3" } },
				{ 4, new CustomsItemDetailsHistory() { ID = 4, EnglishGoodsDescription = "Description 4" } },
				{ 5, new CustomsItemDetailsHistory() { ID = 5, EnglishGoodsDescription = "Description 5" } },
			};

			var propertiesDetailsHistory = new Dictionary<int, PropertiesDetailsHistory>
			{
				{ 1, new PropertiesDetailsHistory() { ID = 1, VatDiscountRate = 100 } },
				{ 2, new PropertiesDetailsHistory() { ID = 2, VatDiscountRate = 80 } },
				{ 3, new PropertiesDetailsHistory() { ID = 3, VatDiscountRate = 100 } },
				{ 4, new PropertiesDetailsHistory() { ID = 4, VatDiscountRate = 100 } },
				{ 5, new PropertiesDetailsHistory() { ID = 5, VatDiscountRate = 80 } },
			};

			var customsItemComputedData = new Dictionary<string, CustomsItemComputedData>
			{
				{
					"-1000000000|1",
					new CustomsItemComputedData()
					{
						ID = 1,
						IsLeaf = true,
						BaseFullClassification = "-1000000000",
						GoodsDescription = "Goods Description 1",
						StartDate = DateTime.ParseExact("20210101", "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture),
						EndDate = DateTime.ParseExact("20240506", "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture),
						CustomsBookTypeIDNum = 1,
						ComputedCheckDigit = 1,
						ValidCustomsItemDetailsHistoryID = 1,
						ValidPropertiesDetailsHistoryID = 1,
						MeasurementUnitID = 1,
					}
				},
				{
					"-2000000000|2",
					new CustomsItemComputedData()
					{
						ID = 2,
						IsLeaf = true,
						BaseFullClassification = "-2000000000",
						GoodsDescription = "Goods Description 7",
						StartDate = DateTime.ParseExact("20210506", "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture),
						EndDate = DateTime.ParseExact("20240306", "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture),
						CustomsBookTypeIDNum = 2,
						ValidCustomsItemDetailsHistoryID = 2,
						ValidPropertiesDetailsHistoryID = 2,
						MeasurementUnitID = 2,
					}
				},
				{
					"-3000000000|3",
					new CustomsItemComputedData()
					{
						ID = 3,
						IsLeaf = true,
						BaseFullClassification = "-3000000000",
						GoodsDescription = "Goods Description 3",
						StartDate = DateTime.ParseExact("20210506", "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture),
						EndDate = DateTime.ParseExact("20240206", "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture),
						CustomsBookTypeIDNum = 3,
						ComputedCheckDigit = 1,
						ValidCustomsItemDetailsHistoryID = 3,
						ValidPropertiesDetailsHistoryID = 3,
					}
				},
				{
					"-4000000000|2",
					new CustomsItemComputedData()
					{
						ID = 4,
						IsLeaf = true,
						BaseFullClassification = "-4000000000",
						GoodsDescription = "Goods Description 4",
						StartDate = DateTime.ParseExact("20210506", "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture),
						EndDate = DateTime.ParseExact("20240106", "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture),
						CustomsBookTypeIDNum = 2,
						ComputedCheckDigit = 1,
						ValidCustomsItemDetailsHistoryID = 4,
						ValidPropertiesDetailsHistoryID = 4,
					}
				},
				{
					"-5000000000|3",
					new CustomsItemComputedData()
					{
						ID = 5,
						IsLeaf = true,
						BaseFullClassification = "-5000000000",
						GoodsDescription = "Goods Description 5",
						StartDate = DateTime.ParseExact("20210506", "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture),
						EndDate = DateTime.ParseExact("20240506", "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture),
						CustomsBookTypeIDNum = 3,
						ComputedCheckDigit = 1,
						ValidCustomsItemDetailsHistoryID = 5,
						ValidPropertiesDetailsHistoryID = 5,
					}
				},
				{
					"-6000000000|3",
					new CustomsItemComputedData()
					{
						ID = 6,
						IsLeaf = false,
						BaseFullClassification = "-6000000000",
						GoodsDescription = "Goods Description 6",
						StartDate = DateTime.ParseExact("20210506", "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture),
						EndDate = DateTime.ParseExact("20240506", "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture),
						CustomsBookTypeIDNum = 3,
						ComputedCheckDigit = 1,
						ValidCustomsItemDetailsHistoryID = 6,
						ValidPropertiesDetailsHistoryID = 6,
					}
				},
				{
					"-1000000000|2",
					new CustomsItemComputedData()
					{
						ID = 7,
						IsLeaf = true,
						BaseFullClassification = "-1000000000",
						GoodsDescription = "Goods Description 1",
						StartDate = DateTime.ParseExact("20210101", "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture),
						EndDate = DateTime.ParseExact("20240506", "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture),
						CustomsBookTypeIDNum = 2,
						ComputedCheckDigit = 1,
						ValidCustomsItemDetailsHistoryID = 1,
						ValidPropertiesDetailsHistoryID = 1,
					}
				},
			};

			var processor = new ILReferenceData.Business.ILCustomsTariffProcessor(new Logger());

			processor.GenerateFiles(publicationDate, customsItemComputedData, propertiesDetailsHistory, customsItemDetailsHistory);

			var outputFileName = "ILTariffs.xml";
			var outputFilePath = Path.Combine(ILReferenceData.Business.ApplicationConfig.Instance.OutputDirectory, "ILTariffs.xml");
			Assert.True(File.Exists(outputFilePath));
			var expectedFileContent = File.ReadAllText(Path.Combine(testFilesFolder, outputFileName));
			var actualFileContent = File.ReadAllText(outputFilePath);
			XmlComparer xmlComparer = new XmlComparer();
			xmlComparer.CompareXml(expectedFileContent, actualFileContent, true);
		}

		[Test]
		public void TestGenerateFiles_WrongTariffType()
		{
			var testFilesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\");
			ILReferenceData.Business.ApplicationConfig.Instance.OutputDirectory = Path.GetTempPath();

			var publicationDate = DateTime.ParseExact("20240220", "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);

			var customsItemDetailsHistory = new Dictionary<int, CustomsItemDetailsHistory>()
			{
				{ 1, new CustomsItemDetailsHistory() { ID = 1, EnglishGoodsDescription = "Description 1" } },
			};

			var propertiesDetailsHistory = new Dictionary<int, PropertiesDetailsHistory>
			{
				{ 1, new PropertiesDetailsHistory() { ID = 1, VatDiscountRate = 100 } },
			};

			var customsItemComputedData = new Dictionary<string, CustomsItemComputedData>
			{
				{
					"-1000000000|4",
					new CustomsItemComputedData()
					{
						ID = 1,
						IsLeaf = true,
						BaseFullClassification = "-1000000000",
						GoodsDescription = "Goods Description 1",
						StartDate = DateTime.ParseExact("20210101", "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture),
						EndDate = DateTime.ParseExact("20240506", "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture),
						CustomsBookTypeIDNum = 4,
						ComputedCheckDigit = 1,
						ValidCustomsItemDetailsHistoryID = 1,
						ValidPropertiesDetailsHistoryID = 1,
					}
				}
			};

			var processor = new ILReferenceData.Business.ILCustomsTariffProcessor(new Logger());

			Assert.Throws<ArgumentOutOfRangeException>(() => processor.GenerateFiles(publicationDate, customsItemComputedData, propertiesDetailsHistory, customsItemDetailsHistory));
		}
	}
}
