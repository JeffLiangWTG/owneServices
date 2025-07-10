using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer.Test
{
	[TestFixture]
	public class TariffMergerFixture
	{
		[Test]
		public void ConvertToActualTariff()
		{
			var webTariffHeader1 = new WebTariffHeader("0101211000", "tariff 1");
			webTariffHeader1.StartDate = new DateTime(2010, 1, 1);
			var webTariffHeader2 = new WebTariffHeader("0101219000", "tariff 2");
			webTariffHeader2.StartDate = new DateTime(2011, 1, 1);
			var webTariffHeader3 = new WebTariffHeader("0101300000", "tariff 3");
			webTariffHeader3.StartDate = new DateTime(2012, 1, 1);

			var tariffCodeExtractor = new Mock<ITariffCodeExtractor>();
			tariffCodeExtractor.Setup(x => x.GetActualTariffHeaders("0100000000", null)).Returns(new[] { webTariffHeader1, webTariffHeader2, webTariffHeader3 });
			tariffCodeExtractor.Setup(x => x.GetActualTariffHeaders("0101000000", null)).Returns(new[] { webTariffHeader1, webTariffHeader2, webTariffHeader3 });
			tariffCodeExtractor.Setup(x => x.GetActualTariffHeaders("0101210000", null)).Returns(new[] { webTariffHeader1, webTariffHeader2 });
			tariffCodeExtractor.Setup(x => x.GetActualTariffHeaders("0101300000", null)).Returns(new[] { webTariffHeader3 });

			var rawTariffs = new List<RefCusTariff> { tariff1, tariff2, tariff3, tariff4 };
			var preferenceMapper = new Mock<IPreferenceMapper>();
			preferenceMapper.Setup(x => x.SetPreferenceOnRates(It.IsAny<IEnumerable<RefCusRate>>())).Returns(new RefCusRate[] { });
			preferenceMapper.Setup(x => x.SetPreferenceOnConditions(It.IsAny<IEnumerable<RefCusCondition>>())).Returns(new RefCusCondition[] { });

			var tariffMerger = new TariffMerger(tariffCodeExtractor.Object, preferenceMapper.Object);
			var result = tariffMerger.ConvertToActualTariffs(rawTariffs).ToList();

			Assert.IsNotNull(result);
			Assert.AreEqual(3, result.Count);

			Assert.AreEqual("0101211000", result[0].ZZ1_TariffCode);
			Assert.AreEqual(new DateTime(2010, 1, 1), result[0].ZZ1_StartDate);
			Assert.AreEqual("0101219000", result[1].ZZ1_TariffCode);
			Assert.AreEqual(new DateTime(2011, 1, 1), result[1].ZZ1_StartDate);
			Assert.AreEqual("0101300000", result[2].ZZ1_TariffCode);
			Assert.AreEqual(new DateTime(2012, 1, 1), result[2].ZZ1_StartDate);
			preferenceMapper.Verify(x => x.SetPreferenceOnRates(It.IsAny<IEnumerable<RefCusRate>>()), Times.Exactly(3));
			preferenceMapper.Verify(x => x.SetPreferenceOnConditions(It.IsAny<IEnumerable<RefCusCondition>>()), Times.Exactly(3));
		}

		[Test]
		public void ConvertToActualAndSetEndDateOnTariff()
		{
			var webTariffHeader1 = new WebTariffHeader("0101210010", "tariff 4");
			var webTariffHeader2 = new WebTariffHeader("0101210020", "tariff 5");

			var tariffCodeExtractor = new Mock<ITariffCodeExtractor>();
			tariffCodeExtractor.Setup(x => x.GetActualTariffHeaders("0100000000", null)).Returns(new[] { webTariffHeader1, webTariffHeader2 });
			tariffCodeExtractor.Setup(x => x.GetActualTariffHeaders("0101000000", null)).Returns(new[] { webTariffHeader1, webTariffHeader2 });
			tariffCodeExtractor.Setup(x => x.GetActualTariffHeaders("0101210000", null)).Returns(new[] { webTariffHeader1, webTariffHeader2 });

			var rawTariffs = new List<RefCusTariff> { tariff1, tariff2, tariff3, tariff4, tariff5, tariff6 };
			var preferenceMapper = new Mock<IPreferenceMapper>();
			preferenceMapper.Setup(x => x.SetPreferenceOnRates(It.IsAny<IEnumerable<RefCusRate>>())).Returns(new RefCusRate[] { });
			preferenceMapper.Setup(x => x.SetPreferenceOnConditions(It.IsAny<IEnumerable<RefCusCondition>>())).Returns(new RefCusCondition[] { });

			var tariffMerger = new TariffMerger(tariffCodeExtractor.Object, preferenceMapper.Object);
			var result = tariffMerger.ConvertToActualTariffs(rawTariffs).ToList();

			Assert.IsNotNull(result);
			Assert.AreEqual(2, result.Count);

			Assert.AreEqual("0101210010", result[0].ZZ1_TariffCode);
			Assert.AreEqual("0101210020", result[1].ZZ1_TariffCode);
			Assert.AreEqual(rate1.ZZ2_EndDate, result[0].ZZ1_EndDate);
			Assert.AreEqual(rate1.ZZ2_EndDate, result[1].ZZ1_EndDate);
		}

		[Test]
		public void ConvertToActualAndDoNotSetEndDateOnTariff()
		{
			ReplaceRatesEndDate(new DateTime(2024, 01, 01), tariff1, tariff2, tariff3, tariff4, tariff5, tariff6);

			var webTariffHeader1 = new WebTariffHeader("0101210010", "tariff 5");
			var webTariffHeader2 = new WebTariffHeader("0101210020", "tariff 6");

			var tariffCodeExtractor = new Mock<ITariffCodeExtractor>();
			tariffCodeExtractor.Setup(x => x.GetActualTariffHeaders("0100000000", null)).Returns(new[] { webTariffHeader1, webTariffHeader2 });
			tariffCodeExtractor.Setup(x => x.GetActualTariffHeaders("0101000000", null)).Returns(new[] { webTariffHeader1, webTariffHeader2 });
			tariffCodeExtractor.Setup(x => x.GetActualTariffHeaders("0101210000", null)).Returns(new[] { webTariffHeader1, webTariffHeader2 });

			var rawTariffs = new List<RefCusTariff> { tariff1, tariff2, tariff3, tariff4, tariff5, tariff6 };
			var preferenceMapper = new Mock<IPreferenceMapper>();
			preferenceMapper.Setup(x => x.SetPreferenceOnRates(It.IsAny<IEnumerable<RefCusRate>>())).Returns(new RefCusRate[] { });
			preferenceMapper.Setup(x => x.SetPreferenceOnConditions(It.IsAny<IEnumerable<RefCusCondition>>())).Returns(new RefCusCondition[] { });

			var tariffMerger = new TariffMerger(tariffCodeExtractor.Object, preferenceMapper.Object, expireTariffBasedOnMaxRateDate: false);
			var result = tariffMerger.ConvertToActualTariffs(rawTariffs).ToList();

			Assert.IsNotNull(result);
			Assert.AreEqual(2, result.Count);

			Assert.AreEqual("0101210010", result[0].ZZ1_TariffCode);
			Assert.AreEqual("0101210020", result[1].ZZ1_TariffCode);
			Assert.AreEqual(new DateTime(2079, 06, 06, 23, 59, 00), result[0].ZZ1_EndDate);
			Assert.AreEqual(new DateTime(2079, 06, 06, 23, 59, 00), result[1].ZZ1_EndDate);

			void ReplaceRatesEndDate(DateTime rateEndDate, params RefCusTariff[] tariffs)
			{
				foreach (var tariff in tariffs)
				{
					foreach (var rate in tariff.RefCusRates)
					{
						rate.ZZ2_EndDate = rateEndDate;
					}
				}
			}
		}

		[Test]
		public void ConvertToActualTariffForSelectedTariffs()
		{
			var webTariffHeader1 = new WebTariffHeader("0101211000", "tariff 1");
			var source = new[] { webTariffHeader1 };

			var tariffCodeExtractor = SetupTariffCodeExtractorMock(webTariffHeader1, source);

			var rawTariffs = new List<RefCusTariff> { tariff1, tariff2, tariff3, tariff4 };

			var preferenceMapper = new Mock<IPreferenceMapper>();
			preferenceMapper.Setup(x => x.SetPreferenceOnRates(It.IsAny<IEnumerable<RefCusRate>>())).Returns(new RefCusRate[] { });
			preferenceMapper.Setup(x => x.SetPreferenceOnConditions(It.IsAny<IEnumerable<RefCusCondition>>())).Returns(new RefCusCondition[] { });
			var tariffMerger = new TariffMerger(tariffCodeExtractor.Object, preferenceMapper.Object);
			var result = tariffMerger.ConvertToActualTariffs(rawTariffs, new[] { webTariffHeader1 }).ToList();

			Assert.IsNotNull(result);
			Assert.AreEqual(1, result.Count);

			Assert.AreEqual("0101211000", result[0].ZZ1_TariffCode);

			tariffCodeExtractor.Verify(x => x.GetActualTariffHeaders("0100000000", source), Times.Once);
			tariffCodeExtractor.Verify(x => x.GetActualTariffHeaders("0101000000", source), Times.Once);
			tariffCodeExtractor.Verify(x => x.GetActualTariffHeaders("0101210000", source), Times.Once);

			preferenceMapper.Verify(x => x.SetPreferenceOnRates(It.IsAny<IEnumerable<RefCusRate>>()), Times.Once);
			preferenceMapper.Verify(x => x.SetPreferenceOnConditions(It.IsAny<IEnumerable<RefCusCondition>>()), Times.Once);
		}

		[Test]
		public void AcceptTariffCodeExtractorNull()
		{
			var rawTariffs = new List<RefCusTariff> { tariff1, tariff2, tariff3, tariff4 };

			var preferenceMapper = new Mock<IPreferenceMapper>();
			preferenceMapper.Setup(x => x.SetPreferenceOnRates(It.IsAny<IEnumerable<RefCusRate>>())).Returns(new RefCusRate[] { });
			preferenceMapper.Setup(x => x.SetPreferenceOnConditions(It.IsAny<IEnumerable<RefCusCondition>>())).Returns(new RefCusCondition[] { });

			var tariffMerger = new TariffMerger(null, preferenceMapper.Object);
			var result = tariffMerger.ConvertToActualTariffs(rawTariffs).ToList();

			Assert.IsNotNull(result);
			Assert.AreEqual(4, result.Count);

			Assert.AreEqual("0100000000", result[0].ZZ1_TariffCode);
			Assert.AreEqual("0101000000", result[1].ZZ1_TariffCode);
			Assert.AreEqual("0101210000", result[2].ZZ1_TariffCode);
			Assert.AreEqual("0101300000", result[3].ZZ1_TariffCode);
		}

		[Test]
		public void ConvertToActualTariffs_Uom()
		{
			var webTariffHeader1 = new WebTariffHeader("0101211000", "tariff 1");
			webTariffHeader1.StartDate = new DateTime(2010, 1, 1);
			var source = new[] { webTariffHeader1 };

			var tariffCodeExtractor = SetupTariffCodeExtractorMock(webTariffHeader1, source);

			var uomRecord = new RawRateRecord("0101211000", null, null, DateTime.Now.AddDays(-1), DateTime.Now.AddDays(+1), "ERGA OMNES", "Supplementary unit", "", "1011", "109", "MTQ", "", true, "A00");
			var uomRecord2 = new RawRateRecord("0101200000", null, null, DateTime.Now.AddDays(-1), DateTime.Now.AddDays(+1), "ERGA OMNES", "Supplementary unit2", "", "1011", "109", "KGM", "", true, "A00");
			var rawUomRecords = new List<RawRateRecord> { uomRecord, uomRecord2 };
			var rawTariffs = new List<RefCusTariff> { tariff1, tariff2, tariff3 };

			var preferenceMapper = new Mock<IPreferenceMapper>();
			preferenceMapper.Setup(x => x.SetPreferenceOnRates(It.IsAny<IEnumerable<RefCusRate>>())).Returns(new RefCusRate[] { });
			preferenceMapper.Setup(x => x.SetPreferenceOnConditions(It.IsAny<IEnumerable<RefCusCondition>>())).Returns(new RefCusCondition[] { });
			var tariffMerger = new TariffMerger(tariffCodeExtractor.Object, preferenceMapper.Object);
			var result = tariffMerger.ConvertToActualTariffs(rawTariffs, new[] { webTariffHeader1 }, rawUomRecords).ToList();

			var tariffUoms = result[0].RefCusTariffUOMs;
			Assert.AreEqual(2, tariffUoms.Length);
			var uomList = tariffUoms.Select(x => x.ZZ8_UOM).ToList();
			Assert.Contains("MTQ", uomList);
			Assert.AreEqual(new DateTime(2010, 1, 1), result[0].ZZ1_StartDate);

			var mtqUom = tariffUoms.Single(x => x.ZZ8_UOM == "MTQ");
			Assert.That(mtqUom.ZZ8_ZZA_NKTradeGroup, Is.EqualTo("1011"));
			Assert.That(mtqUom.ZZ8_ZZA_ZZZ_NKDataGrouping, Is.EqualTo("EUN"));
		}

		[Test]
		public void ConvertToActualTariffs_RemoveDuplicateUoms()
		{
			var webTariffHeader1 = new WebTariffHeader("0101211000", "tariff 1");
			var source = new[] { webTariffHeader1 };

			var tariffCodeExtractor = SetupTariffCodeExtractorMock(webTariffHeader1, source);

			var uomRecord = new RawRateRecord("0101211000", null, null, DateTime.Now.AddDays(-1), DateTime.Now.AddDays(+1), "ERGA OMNES", "Supplementary unit", "", "1011", "109", "MTQ", "", true, "A00");
			var rawUomRecords = new List<RawRateRecord> { uomRecord };
			var tariffUom = new RefCusTariffUOM { ZZ8_Type = "CU2", ZZ8_UOM = "MTQ", ZZ8_ZZA_NKTradeGroup = "1011", ZZ8_ZZA_ZZZ_NKDataGrouping = "EUN" };
			tariff1.RefCusTariffUOMs = new[] { tariffUom };
			tariff2.RefCusTariffUOMs = new[] { tariffUom };
			tariff3.RefCusTariffUOMs = new[] { tariffUom };
			var rawTariffs = new List<RefCusTariff> { tariff1, tariff2, tariff3 };

			var preferenceMapper = new Mock<IPreferenceMapper>();
			preferenceMapper.Setup(x => x.SetPreferenceOnRates(It.IsAny<IEnumerable<RefCusRate>>())).Returns(new RefCusRate[] { });
			preferenceMapper.Setup(x => x.SetPreferenceOnConditions(It.IsAny<IEnumerable<RefCusCondition>>())).Returns(new RefCusCondition[] { });
			var tariffMerger = new TariffMerger(tariffCodeExtractor.Object, preferenceMapper.Object);
			var result = tariffMerger.ConvertToActualTariffs(rawTariffs, new[] { webTariffHeader1 }, rawUomRecords).ToList();

			var tariffUoms = result[0].RefCusTariffUOMs.Where(x => x.ZZ8_UOM == "MTQ");
			Assert.AreEqual(1, tariffUoms.Count());
		}

		[Test]
		public void ConvertToActualTariffsDoesNotMapUomAdditionalDataForExport()
		{
			var webTariffHeader1 = new WebTariffHeader("0101211000", "tariff 1");
			webTariffHeader1.StartDate = new DateTime(2010, 1, 1);
			var source = new[] { webTariffHeader1 };

			var tariffCodeExtractor = SetupTariffCodeExtractorMock(webTariffHeader1, source);

			var uomRecord = new RawRateRecord("0123456000", null, null, DateTime.Now.AddDays(-1), DateTime.Now.AddDays(+1), "ERGA OMNES", "Supplementary unit", "", "1011", "109", "MTQ", "", true, "A00");
			var uomRecord2 = new RawRateRecord("1234567800", null, null, DateTime.Now.AddDays(-1), DateTime.Now.AddDays(+1), "ERGA OMNES", "Supplementary unit2", "", "1011", "109", "KGM", "", true, "A00");
			var rawUomRecords = new List<RawRateRecord> { uomRecord, uomRecord2 };
			var rawTariffs = new List<RefCusTariff> { tariff1, tariff2, tariff3 };

			var preferenceMapper = new Mock<IPreferenceMapper>();
			preferenceMapper.Setup(x => x.SetPreferenceOnRates(It.IsAny<IEnumerable<RefCusRate>>())).Returns(new RefCusRate[] { });
			preferenceMapper.Setup(x => x.SetPreferenceOnConditions(It.IsAny<IEnumerable<RefCusCondition>>())).Returns(new RefCusCondition[] { });

			var tariffMerger = new TariffMerger(tariffCodeExtractor.Object, preferenceMapper.Object, isExport: true, expireTariffBasedOnMaxRateDate: true);

			var result = tariffMerger.ConvertToActualTariffs(rawTariffs, new[] { webTariffHeader1 }, rawUomRecords).ToList();

			Assert.Multiple(() =>
			{
				var uom1Tariff = result.Single(x => x.ZZ1_TariffCode == "01234560");
				var uom2Tariff = result.Single(x => x.ZZ1_TariffCode == "12345678");
				Assert.IsNotNull(uom1Tariff.RefCusTariffUOMs.FirstOrDefault(x => x.ZZ8_UOM == "KGM" && x.ZZ8_Type == "CU1"));
				Assert.IsNotNull(uom2Tariff.RefCusTariffUOMs.FirstOrDefault(x => x.ZZ8_UOM == "KGM" && x.ZZ8_Type == "CU1"));
			});
		}

		[Test]
		public void ConvertToActualTariffsDoesNotMapUomAdditionalDataForImport()
		{
			var webTariffHeader1 = new WebTariffHeader("0101211000", "tariff 1");
			webTariffHeader1.StartDate = new DateTime(2010, 1, 1);
			var source = new[] { webTariffHeader1 };

			var tariffCodeExtractor = SetupTariffCodeExtractorMock(webTariffHeader1, source);

			var uomRecord = new RawRateRecord("0123456000", null, null, DateTime.Now.AddDays(-1), DateTime.Now.AddDays(+1), "ERGA OMNES", "Supplementary unit", "", "1011", "109", "MTQ", "", true, "A00");
			var uomRecord2 = new RawRateRecord("1234567800", null, null, DateTime.Now.AddDays(-1), DateTime.Now.AddDays(+1), "ERGA OMNES", "Supplementary unit2", "", "1011", "109", "KGM", "", true, "A00");
			var rawUomRecords = new List<RawRateRecord> { uomRecord, uomRecord2 };
			var rawTariffs = new List<RefCusTariff> { tariff1, tariff2, tariff3 };

			var preferenceMapper = new Mock<IPreferenceMapper>();
			preferenceMapper.Setup(x => x.SetPreferenceOnRates(It.IsAny<IEnumerable<RefCusRate>>())).Returns(new RefCusRate[] { });
			preferenceMapper.Setup(x => x.SetPreferenceOnConditions(It.IsAny<IEnumerable<RefCusCondition>>())).Returns(new RefCusCondition[] { });

			var tariffMerger = new TariffMerger(tariffCodeExtractor.Object, preferenceMapper.Object, isExport: false, expireTariffBasedOnMaxRateDate: true);

			var result = tariffMerger.ConvertToActualTariffs(rawTariffs, new[] { webTariffHeader1 }, rawUomRecords).ToList();

			Assert.Multiple(() =>
			{
				Assert.IsNull(result.FirstOrDefault(x => x.ZZ1_TariffCode == "01234560"));
				Assert.IsNull(result.FirstOrDefault(x => x.ZZ1_TariffCode == "12345678"));
			});
		}

		#region Setup

		RefCusRate rate1, rate2, rate3, rate4, rate5, rate6, rate7, rate8, rate9, rate10;
		RefCusCondition condition1, condition2, condition3;
		RefCusTariff tariff1, tariff2, tariff3, tariff4, tariff5, tariff6;

		[SetUp]
		protected void Setup()
		{
			rate1 = new RefCusRate { ZZ2_RateFormula = "Rate 1", ZZ2_EndDate = new DateTime(2079, 6, 6, 23, 59, 0), RefCusApplicabilities = new[] { new RefCusApplicability { ZZT_ZZA_NKTradeGroup = "TG1" } } };
			rate2 = new RefCusRate { ZZ2_RateFormula = "Rate 2", ZZ2_EndDate = new DateTime(2079, 6, 6, 23, 59, 0), RefCusApplicabilities = new[] { new RefCusApplicability { ZZT_ZZA_NKTradeGroup = "TG2", RefCusExcludedTradeGroups = new[] { new RefCusExcludedTradeGroup { ZZC_ZZA_NKTradeGroup = "EX1" } } } } };
			rate3 = new RefCusRate { ZZ2_RateFormula = "Rate 3", ZZ2_EndDate = new DateTime(2079, 6, 6, 23, 59, 0), RefCusApplicabilities = new[] { new RefCusApplicability { ZZT_ZZA_NKTradeGroup = "TG3" } } };
			rate4 = new RefCusRate { ZZ2_RateFormula = "Rate 4", ZZ2_EndDate = new DateTime(2079, 6, 6, 23, 59, 0), RefCusApplicabilities = new[] { new RefCusApplicability { ZZT_ZZA_NKTradeGroup = "TG4" } } };
			rate5 = new RefCusRate { ZZ2_RateFormula = "Rate 5", ZZ2_EndDate = new DateTime(2079, 6, 6, 23, 59, 0), RefCusApplicabilities = new[] { new RefCusApplicability { ZZT_ZZA_NKTradeGroup = "TG5" } } };
			rate6 = new RefCusRate { ZZ2_RateFormula = "Rate 6", ZZ2_EndDate = new DateTime(2079, 6, 6, 23, 59, 0), RefCusApplicabilities = new[] { new RefCusApplicability { ZZT_ZZA_NKTradeGroup = "TG6" } } };
			rate7 = new RefCusRate { ZZ2_RateFormula = "Rate 7", ZZ2_StartDate = new DateTime(2000, 1, 1), ZZ2_EndDate = new DateTime(2010, 1, 1), RefCusApplicabilities = new[] { new RefCusApplicability { ZZT_ZZA_NKTradeGroup = "TG6" } } };
			rate8 = new RefCusRate { ZZ2_RateFormula = "Rate 8", ZZ2_StartDate = new DateTime(2000, 1, 1), ZZ2_EndDate = new DateTime(2079, 6, 6, 23, 59, 0), RefCusApplicabilities = new[] { new RefCusApplicability { ZZT_ZZA_NKTradeGroup = "TG6" } } };
			rate9 = new RefCusRate { ZZ2_RateFormula = "Rate 9", ZZ2_StartDate = new DateTime(2000, 1, 1), ZZ2_EndDate = new DateTime(2010, 1, 1), RefCusApplicabilities = new[] { new RefCusApplicability { ZZT_ZZA_NKTradeGroup = "TG6" } } };
			rate10 = new RefCusRate { ZZ2_RateFormula = "Rate 10", ZZ2_StartDate = new DateTime(2000, 1, 1), ZZ2_EndDate = new DateTime(2010, 1, 1), RefCusApplicabilities = new[] { new RefCusApplicability { ZZT_ZZA_NKTradeGroup = "TG6" } } };

			condition1 = new RefCusCondition { ZX1_Comment = "condition 1", ZX1_ZX2_NKConditionType = "142", RefCusApplicabilities = new[] { new RefCusApplicability { ZZT_ZZA_NKTradeGroup = "TG1" } }, RefCusConditionValues = new[] { new RefCusConditionValue { ZX3_Value = "condition value 1" } } };
			condition2 = new RefCusCondition { ZX1_Comment = "condition 2", ZX1_ZX2_NKConditionType = "103", RefCusApplicabilities = new[] { new RefCusApplicability { ZZT_ZZA_NKTradeGroup = "TG2" } }, RefCusConditionValues = new[] { new RefCusConditionValue { ZX3_Value = "condition value 2" } } };
			condition3 = new RefCusCondition { ZX1_Comment = "condition 3", ZX1_ZX2_NKConditionType = "143", RefCusApplicabilities = new[] { new RefCusApplicability { ZZT_ZZA_NKTradeGroup = "TG3" } }, RefCusConditionValues = new[] { new RefCusConditionValue { ZX3_Value = "condition value 3" } } };

			tariff1 = new RefCusTariff { ZZ1_TariffCode = "0100000000", ZZ1_ZZI_NKTariffType = "IMP", ZZ1_ZZZ_NKDataGrouping = "EUN", ZZ1_ZZI_ZZZ_NKDataGrouping = "EUN", RefCusRates = new[] { rate1, rate2 }, RefCusConditions = new[] { condition1 } };
			tariff2 = new RefCusTariff { ZZ1_TariffCode = "0101000000", ZZ1_ZZI_NKTariffType = "IMP", ZZ1_ZZZ_NKDataGrouping = "EUN", ZZ1_ZZI_ZZZ_NKDataGrouping = "EUN", RefCusRates = new[] { rate3 }, RefCusConditions = new[] { condition1 } };
			tariff3 = new RefCusTariff { ZZ1_TariffCode = "0101210000", ZZ1_ZZI_NKTariffType = "IMP", ZZ1_ZZZ_NKDataGrouping = "EUN", ZZ1_ZZI_ZZZ_NKDataGrouping = "EUN", RefCusRates = new[] { rate4, rate5 }, RefCusConditions = new[] { condition2 } };
			tariff4 = new RefCusTariff { ZZ1_TariffCode = "0101300000", ZZ1_ZZI_NKTariffType = "IMP", ZZ1_ZZZ_NKDataGrouping = "EUN", ZZ1_ZZI_ZZZ_NKDataGrouping = "EUN", RefCusRates = new[] { rate6 }, RefCusConditions = new[] { condition3 } };
			tariff5 = new RefCusTariff { ZZ1_TariffCode = "0101210010", ZZ1_ZZI_NKTariffType = "IMP", ZZ1_ZZZ_NKDataGrouping = "EUN", ZZ1_ZZI_ZZZ_NKDataGrouping = "EUN", RefCusRates = new[] { rate7, rate8 }, RefCusConditions = new[] { condition3 } };
			tariff6 = new RefCusTariff { ZZ1_TariffCode = "0101210020", ZZ1_ZZI_NKTariffType = "IMP", ZZ1_ZZZ_NKDataGrouping = "EUN", ZZ1_ZZI_ZZZ_NKDataGrouping = "EUN", RefCusRates = new[] { rate9, rate10 }, RefCusConditions = new[] { condition3 } };
		}

		#endregion

		Mock<ITariffCodeExtractor> SetupTariffCodeExtractorMock(WebTariffHeader webTariffHeader1, WebTariffHeader[] source)
		{
			var tariffCodeExtractor = new Mock<ITariffCodeExtractor>();
			tariffCodeExtractor.Setup(x => x.GetActualTariffHeaders("0100000000", source)).Returns(new[] { webTariffHeader1 });
			tariffCodeExtractor.Setup(x => x.GetActualTariffHeaders("0101000000", source)).Returns(new[] { webTariffHeader1 });
			tariffCodeExtractor.Setup(x => x.GetActualTariffHeaders("0101210000", source)).Returns(new[] { webTariffHeader1 });
			return tariffCodeExtractor;
		}
	}
}
