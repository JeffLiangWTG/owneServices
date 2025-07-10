using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NLReferenceData.Business.Testing
{
	[TestFixture]
	sealed class TariffsBuilderTests
	{
		[Test]
		public void TestValidEndDate()
		{
			var dutiesWithEndTimeAtMidnight = new List<Measure>()
			{
				new Measure()
				{
					AdditionalCodeId = "319",
					SIDAdditionalCode = "-10014",
					AdditionalCodeType = "U",
					GeographicalAreaId = "1011",
					GoodsNomenclatureCode = "2203000000",
					SIDGoodsNomenclature = "34648",
					MeasureType = "NLACC",
					National = "1",
					RegulationId = "1NLWETAC",
					RegulationRoleType = "1",
					SID = "-24400",
					SIDGeographicalArea = "400",
					DateStart = new DateTime(2014, 01, 01),
					DateEnd = new DateTime(2079, 06, 12, 00, 00, 00),
					StoppedFlag = "0",
					ChangeType = "U",
					Formula = "47.48 * [HLT]",
					UnitOfMeasure = new List<string>
					{
						"HLT",
						"MIL",
					},
					CleanId = "22030",
					RateCode = "035",
					RateType = "050",
					AdditionalCode = "U319"
				},
			};
			var errorCollector = new StringBuilder();
			var builder = new TariffsBuilder(errorCollector);
			var resultList = builder.ConvertMeasuresToRefCusTariff(dutiesWithEndTimeAtMidnight);
			var refCusRate = resultList[0].RefCusRates[0];
			var refCusApplicability = refCusRate.RefCusApplicabilities[0];

			Assert.AreEqual(new DateTime(2079, 06, 12, 23, 59, 00), refCusApplicability.ZZT_EndDate, "RefCusRate End Date should be '2079-06-12T23:59:00'");
		}

		[Test]
		public void ConvertXmlFileToRefCusTariff_Duties()
		{
			var errorCollector = new StringBuilder();
			var builder = new TariffsBuilder(errorCollector);

			var resultList = builder.ConvertMeasuresToRefCusTariff(ReferenceDataDuties);

			Assert.IsNotNull(resultList, "Measure nodes could not be converted, no results are returned");
			Assert.AreEqual(2, resultList.Count, "Not all Measure nodes could be converted to RefCusTariff");
			Assert.AreEqual("22030", resultList[0].ZZ1_TariffCode, "RefCusTariff is generated with wrong Tariff Code");

			Assert.AreEqual(1, resultList[0].RefCusRates.Length, "There should be one RefCusRate");
			var refCusRate = resultList[0].RefCusRates[0];
			Assert.AreEqual("43.92 * [HLT]", refCusRate.ZZ2_RateFormula, "RefCusRate Rate Formula should be equal to '43.92 * [HLT]'");
			Assert.AreEqual(new DateTime(2014, 01, 01, 00, 00, 00), refCusRate.ZZ2_StartDate, "RefCusRate Rate Start Date should be equal to '2014-01-01T00:00:00'");
			Assert.AreEqual("035", refCusRate.ZZ2_ZY1_NKRateCode, "RefCusRate Rate Code should be equal to '035'");
			Assert.AreEqual("050", refCusRate.ZZ2_ZY1_ZZR_NKRateType, "RefCusRate Rate Type should be equal to '050'");

			Assert.AreEqual(1, refCusRate.RefCusApplicabilities.Length, "There should be one RefCusApplicability");
			var refCusApplicability = refCusRate.RefCusApplicabilities[0];
			Assert.AreEqual("U313", refCusApplicability.ZZT_AdditionalCode, "RefCusApplicability Additional Code should be equal to 'U313'");
			Assert.AreEqual(new DateTime(2014, 01, 01, 00, 00, 00), refCusApplicability.ZZT_StartDate, "RefCusRate Rate Start Date should be equal to '2014-01-01T00:00:00'");
			Assert.AreEqual(new DateTime(2079, 06, 06, 23, 59, 00), refCusApplicability.ZZT_EndDate, "RefCusRate Rate End Date should be equal to '2079-06-06T23:59:00'");

			Assert.AreEqual(2, refCusRate.RefCusRateUOMs.Length, "There should be two RefCusRateUOMs");
			var refCusRateUOM1 = refCusRate.RefCusRateUOMs[0];
			Assert.AreEqual("HLT", refCusRateUOM1.ZXG_UOM, "refCusRateUOM1 Unit Of Measure should be equal to 'HLT'");
			var refCusRateUOM2 = refCusRate.RefCusRateUOMs[1];
			Assert.AreEqual("MIL", refCusRateUOM2.ZXG_UOM, "refCusRateUOM2 Unit Of Measure should be equal to 'MIL'");
		}

		[Test]
		public void ConvertXmlFileToRefCusTariff_VAT()
		{
			var errorCollector = new StringBuilder();
			var builder = new TariffsBuilder(errorCollector);

			var resultList = builder.ConvertMeasuresToRefCusTariff(ReferenceDataVAT);

			Assert.IsNotNull(resultList, "Measure nodes could not be converted, no results are returned");
			Assert.AreEqual(1, resultList.Count, "Not all Measure nodes could be converted to RefCusTariff");
			Assert.AreEqual("22010", resultList[0].ZZ1_TariffCode, "RefCusTariff is generated with wrong Tariff Code");

			Assert.AreEqual(2, resultList[0].RefCusVATApplicabilities.Length, "There should be two RefCusVATApplicabilities");
			var refCusVATApplicabilities1 = resultList[0].RefCusVATApplicabilities[0];
			Assert.AreEqual("LOW", refCusVATApplicabilities1.ZX5_ZZF_NKTaxOrFeeCode, "VAT Applicability 1 Tax or fee code should be equal to 'LOW'");
			Assert.AreEqual(new DateTime(2019, 12, 01, 00, 00, 00), refCusVATApplicabilities1.ZX5_StartDate, "VAT Applicability 1 Start date should be equal to '2019-12-01T00:00:00'");
			Assert.AreEqual("U283", refCusVATApplicabilities1.ZX5_AdditionalCode, "VAT Applicability 1 Additional Code should be equal to 'U283'");

			var refCusVATApplicabilities2 = resultList[0].RefCusVATApplicabilities[1];
			Assert.AreEqual("LOW", refCusVATApplicabilities2.ZX5_ZZF_NKTaxOrFeeCode, "VAT Applicability 2 Tax or fee code should be equal to 'LOW'");
			Assert.AreEqual(new DateTime(2019, 12, 01, 00, 00, 00), refCusVATApplicabilities2.ZX5_StartDate, "VAT Applicability 2 Start date should be equal to '2019-12-01T00:00:00'");
			Assert.AreEqual("U285", refCusVATApplicabilities2.ZX5_AdditionalCode, "VAT Applicability 2 Additional Code should be equal to 'U285'");
		}

		[Test]
		public void InvalidDataInXml()
		{
			var errorCollector = new StringBuilder();
			var builder = new TariffsBuilder(errorCollector);
			var refDataCollection = builder.ConvertMeasuresToRefCusTariff(InvalidData);

			Assert.That(errorCollector.ToString().Contains("RefCusTariff validation error: Key '_00010101' Errors: ZZ1_TariffCode is required."), "Invalid data (missing tariff code) is not detected.");
			Assert.That(!errorCollector.ToString().Contains("RefCusTariff validation error: Key '22030_00010101' Errors: ZX5_ZZF_NKTaxOrFeeCode is required. ZX5_AdditionalCode is required."));
			Assert.That(errorCollector.ToString().Contains("RefCusTariff validation error: Key '22030_00010101' Errors: ZZ2_RateFormula is required."));
			Assert.That(!errorCollector.ToString().Contains("RefCusTariff validation error: Key '22035_00010101' Errors: ZZ2_RateFormula is required. ZZ2_ZY1_NKRateCode and ZZ2_ZY1_ZZR_NKRateType are required. Some mappings might be missing."));

			Assert.AreEqual(0, refDataCollection.Count, "Resultset contains invalid data, no items should be in resultset");
		}

		[Test]
		public void GenerateUniversalReferenceDataXml()
		{
			var errorCollector = new StringBuilder();
			var builder = new TariffsBuilder(errorCollector);
			string expectedXml, generatedXml = string.Empty;

			var content = builder.ConvertMeasuresToRefCusTariff(ReferenceData);
			TariffsBuilder.GenerateUniversalReferenceDataXml(content, new DateTime(2021, 01, 18, 13, 31, 25), TempFolder);

			var expectedFileName = Path.Combine(TempFolder, "RefCusTariff_NL Tariff_133125000.xml");
			if (File.Exists(expectedFileName))
			{
				generatedXml = File.ReadAllText(expectedFileName);
			}

			expectedXml = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.NLReferenceData.Tests.TestFiles.Tariff.Output.RefCusTariffs.xml");

			Assert.IsTrue(File.Exists(expectedFileName), $"File with name '{expectedFileName}' could not be found");
			Assert.AreEqual(expectedXml, generatedXml, "Generated Universal Reference Xml does not match the correct format.");
		}

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			ReferenceDataVAT = new List<Measure>()
			{
				new Measure()
				{
					AdditionalCodeId = "283",
					AdditionalCode = "U283",
					SIDAdditionalCode = "-10105",
					AdditionalCodeType = "U",
					GeographicalAreaId = "1011",
					GoodsNomenclatureCode= "2201000000",
					SIDGoodsNomenclature = "34629",
					MeasureType = "NLBTW",
					National = "1",
					RegulationId = "1NLWETOB",
					RegulationRoleType = "1",
					SID = "-38028",
					SIDGeographicalArea = "400",
					DateStart = new DateTime(2019, 12, 01),
					StoppedFlag = "0",
					ChangeType = "U",
					TaxOrFeeCode = "LOW",
					CleanId = "22010",
				},
				new Measure()
				{
					AdditionalCodeId = "285",
					AdditionalCode = "U285",
					SIDAdditionalCode = "-10107",
					AdditionalCodeType = "U",
					GeographicalAreaId = "1011",
					GoodsNomenclatureCode="2201000000",
					SIDGoodsNomenclature = "34629",
					MeasureType = "NLBTW",
					National = "1",
					RegulationId = "1NLWETOB",
					RegulationRoleType = "1",
					SID = "-38037",
					SIDGeographicalArea = "400",
					DateStart = new DateTime(2019, 12, 01),
					StoppedFlag = "0",
					ChangeType = "U",
					TaxOrFeeCode = "LOW",
					CleanId = "22010",
				}
			};

			ReferenceDataDuties = new List<Measure>()
			{
				new Measure()
				{
					AdditionalCodeId = "313",
					SIDAdditionalCode = "-10013",
					AdditionalCodeType = "U",
					GeographicalAreaId = "1011",
					GoodsNomenclatureCode = "2203000000",
					SIDGoodsNomenclature = "34648",
					MeasureType = "NLACC",
					National = "1",
					RegulationId = "1NLWETAC",
					RegulationRoleType = "1",
					SID = "-24399",
					SIDGeographicalArea = "400",
					DateStart = new DateTime(2014, 01, 01),
					StoppedFlag = "0",
					ChangeType = "U",
					Formula = "43.92 * [HLT]",
					UnitOfMeasure = new List<string>
					{
						"HLT",
						"MIL"
					},
					CleanId = "22030",
					RateCode = "035",
					RateType = "050",
					AdditionalCode = "U313"
				},
				new Measure()
				{
					AdditionalCodeId = "319",
					SIDAdditionalCode = "-10014",
					AdditionalCodeType = "U",
					GeographicalAreaId = "1011",
					GoodsNomenclatureCode = "2203000000",
					SIDGoodsNomenclature = "34648",
					MeasureType = "NLACC",
					National = "1",
					RegulationId = "1NLWETAC",
					RegulationRoleType = "1",
					SID = "-24400",
					SIDGeographicalArea = "400",
					DateStart = new DateTime(2014, 01, 01),
					StoppedFlag = "0",
					ChangeType = "U",
					Formula = "47.48 * [HLT]",
					UnitOfMeasure = new List<string>
					{
						"HLT",
						"MIL",
					},
					CleanId = "22030",
					RateCode = "035",
					RateType = "050",
					AdditionalCode = "U319"
				},
			};

			ReferenceData = new List<Measure>();
			ReferenceData.AddRange(ReferenceDataVAT);
			ReferenceData.AddRange(ReferenceDataDuties);

			InvalidData = new List<Measure>()
			{
				new Measure()
				{
					AdditionalCodeId = "283",
					AdditionalCode = "U283",
					SIDAdditionalCode = "-10105",
					AdditionalCodeType = "U",
					GeographicalAreaId = "1011",
					GoodsNomenclatureCode="",
					SIDGoodsNomenclature = "34629",
					MeasureType = "NLBTW",
					National = "1",
					RegulationId = "1NLWETOB",
					RegulationRoleType = "1",
					SID = "-38028",
					SIDGeographicalArea = "400",
					DateStart = new DateTime(2019, 12, 01),
					StoppedFlag = "0",
					ChangeType = "U",
					TaxOrFeeCode = "LOW",
					CleanId = "",
				},
				new Measure()
				{
					AdditionalCodeId = "283",
					AdditionalCode = "U283",
					SIDAdditionalCode = "-10105",
					AdditionalCodeType = "U",
					GeographicalAreaId = "1011",
					GoodsNomenclatureCode="2203000000",
					SIDGoodsNomenclature = "34629",
					MeasureType = "NLBTW",
					National = "1",
					RegulationId = "1NLWETOB",
					RegulationRoleType = "1",
					SID = "-38028",
					SIDGeographicalArea = "400",
					DateStart = new DateTime(2019, 12, 01),
					StoppedFlag = "0",
					ChangeType = "U",
					TaxOrFeeCode = "",
					CleanId = "22030",
				},
				new Measure()
				{
					AdditionalCodeId = "283",
					AdditionalCode = "",
					SIDAdditionalCode = "-10105",
					AdditionalCodeType = "U",
					GeographicalAreaId = "1011",
					GoodsNomenclatureCode="2203000000",
					SIDGoodsNomenclature = "34629",
					MeasureType = "NLBTW",
					National = "1",
					RegulationId = "1NLWETOB",
					RegulationRoleType = "1",
					SID = "-38028",
					SIDGeographicalArea = "400",
					DateStart = new DateTime(2019, 12, 01),
					StoppedFlag = "0",
					ChangeType = "U",
					TaxOrFeeCode = "LOW",
					CleanId = "22030",
				},
				new Measure()
				{
					AdditionalCodeId = "283",
					AdditionalCode = "U238",
					SIDAdditionalCode = "-10105",
					AdditionalCodeType = "U",
					GeographicalAreaId = "",
					GoodsNomenclatureCode="2203000000",
					SIDGoodsNomenclature = "34629",
					MeasureType = "NLBTW",
					National = "1",
					RegulationId = "1NLWETOB",
					RegulationRoleType = "1",
					SID = "-38028",
					SIDGeographicalArea = "400",
					DateStart = new DateTime(2019, 12, 01),
					StoppedFlag = "0",
					ChangeType = "U",
					TaxOrFeeCode = "LOW",
					CleanId = "22030",
				},
				new Measure()
				{
					AdditionalCodeId = "319",
					SIDAdditionalCode = "-10014",
					AdditionalCodeType = "U",
					GeographicalAreaId = "1011",
					GoodsNomenclatureCode = "2203000000",
					SIDGoodsNomenclature = "34648",
					MeasureType = "NLACC",
					National = "1",
					RegulationId = "1NLWETAC",
					RegulationRoleType = "1",
					SID = "-24400",
					SIDGeographicalArea = "400",
					DateStart = new DateTime(2014, 01, 01),
					StoppedFlag = "0",
					ChangeType = "U",
					UnitOfMeasure = new List<string>
					{
						"HLT",
						"MIL",
					},
					CleanId = "22030",
					RateCode = "035",
					RateType = "050",
					AdditionalCode = "U319"
				},
				new Measure()
				{
					AdditionalCodeId = "319",
					SIDAdditionalCode = "-10014",
					AdditionalCodeType = "U",
					GeographicalAreaId = "1011",
					GoodsNomenclatureCode = "2203000000",
					SIDGoodsNomenclature = "34648",
					MeasureType = "NLACC",
					National = "1",
					RegulationId = "1NLWETAC",
					RegulationRoleType = "1",
					SID = "-24400",
					SIDGeographicalArea = "400",
					DateStart = new DateTime(2014, 01, 01),
					StoppedFlag = "0",
					ChangeType = "U",
					UnitOfMeasure = new List<string>
					{
						"HLT",
						"MIL",
					},
					CleanId = "22035",
					RateCode = "",
					RateType = "",
					AdditionalCode = "X987"
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
		List<Measure> ReferenceData, ReferenceDataVAT, ReferenceDataDuties, InvalidData;
	}
}
