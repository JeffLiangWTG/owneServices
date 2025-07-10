using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.TaiwanReferenceData.TurnkeyPlugInUpdateService;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.TaiwanReferenceData.Test
{
	[TestFixture]
	public class TariffUpdateInfoTest
	{
		public class TariffUpdateInfoForTest : TariffUpdateInfo
		{
			public TariffUpdateInfoForTest(updateInfoBean info, string filename) : base(info, filename, SystemContext.Now())
			{
				BeforeExecuteEvent += TariffUpdateInfoForTest_BeforeExecuteEvent;
			}

			private void TariffUpdateInfoForTest_BeforeExecuteEvent()
			{
				SystemContext.UtcNow = () => TimeZoneInfo.ConvertTimeToUtc(SystemContext.Now());
				SystemContext.Now = () => new DateTime(2018, 07, 23, 13, 52, 15);
			}

			protected override IWebClient GetWebClientWrapper()
			{
				var mock = new Mock<IWebClient>();
				byte[] responseTariff2Bytes = File.ReadAllBytes(Path.Combine(FolderHelper.GetBinFolder(), "doc/Tariff/TARIFF_2.txt"));
				mock.Setup(x => x.DownloadData("http://192.118.118.1/TARIFF_2.txt")).Returns(responseTariff2Bytes);

				byte[] responseTariff3Bytes = File.ReadAllBytes(Path.Combine(FolderHelper.GetBinFolder(), "doc/Tariff/TARIFF_3_1.txt"));
				mock.Setup(x => x.DownloadData("http://192.118.118.1/TARIFF_3.txt")).Returns(responseTariff3Bytes);

				byte[] responsenote10CBytes = File.ReadAllBytes(Path.Combine(FolderHelper.GetBinFolder(), "doc/Tariff/note_10_C.txt"));
				mock.Setup(x => x.DownloadData("http://192.118.118.1/note_10_C.txt")).Returns(responsenote10CBytes);

				byte[] responsenote10EBytes = File.ReadAllBytes(Path.Combine(FolderHelper.GetBinFolder(), "doc/Tariff/note_10_E.txt"));
				mock.Setup(x => x.DownloadData("http://192.118.118.1/note_10_E.txt")).Returns(responsenote10EBytes);

				byte[] responseEnnameBytes = File.ReadAllBytes(Path.Combine(FolderHelper.GetBinFolder(), "doc/Tariff/enname.doc"));
				mock.Setup(x => x.DownloadData("http://192.118.118.1/enname.doc")).Returns(responseEnnameBytes);

				byte[] responseEPTBytes = File.ReadAllBytes(Path.Combine(FolderHelper.GetBinFolder(), "doc/Tariff/EnvironmentalProtectionTariffs.ods"));
				mock.Setup(x => x.DownloadData("http://192.118.118.1/EnvironmentalProtectionTariffs.ods")).Returns(responseEPTBytes);

				byte[] responseTWTariffF5ValidationBytes = File.ReadAllBytes(Path.Combine(FolderHelper.GetBinFolder(), "doc/Tariff/TWTariff-F5 Validation.csv"));
				mock.Setup(x => x.DownloadData(Path.Combine(FolderHelper.GetBinFolder(), AppConfig.Tariff.TWTariffF5ValidationFileName))).Returns(responseTWTariffF5ValidationBytes);
				return mock.Object;
			}

			public new List<TariffColumn2DataRow> ApplyOverrides(List<TariffColumn2DataRow> rows, List<TariffColumn2OverrideDataRow> overrideList)
			{
				return base.ApplyOverrides(rows, overrideList);
			}

			public new List<TariffColumn2DataRowGroup> GroupTariffColumn2DataRows(List<TariffColumn2DataRow> rows)
			{
				return base.GroupTariffColumn2DataRows(rows);
			}

			public new IEnumerable<Common.UniversalXmlWriter.EntityType.RefCusRate> CreateRefCusRates(IEnumerable<RefCusRate> refCusRates)
			{
				return base.CreateRefCusRates(refCusRates);
			}

			public new IEnumerable<Common.UniversalXmlWriter.EntityType.RefCusRate> CreateRefCusRates(TariffColumn1And3DataRow tariffData)
			{
				return base.CreateRefCusRates(tariffData);
			}

			public new IEnumerable<Common.UniversalXmlWriter.EntityType.RefCusRate> CreateRefCusRateElement(string specificRateUnit, TariffColumn2DataRowGroup col2TariffData)
			{
				return base.CreateRefCusRateElement(specificRateUnit, col2TariffData);
			}

			public new Func<List<TariffColumn1And3DataRow>, TariffColumn1And3DataRow, KeyValuePair<int, int>, string> CompositeKeyOnZZ5 => base.CompositeKeyOnZZ5;

			public new static string GetTens(List<TariffColumn1And3DataRow> tariffDataRowList, TariffColumn1And3DataRow tariffRow)
			{
				return TariffUpdateInfo.GetTens(tariffDataRowList, tariffRow);
			}

			public new IEnumerable<RefCusTariffAttribute> CreateAttributeForEnvironmentalProtectionTariff(string tariffCode, List<string> referenceList)
			{
				return base.CreateAttributeForEnvironmentalProtectionTariff(tariffCode, referenceList);
			}

			public new IEnumerable<RefCusTariffAttribute> CreateAttributeFromTariffAttributeDataRow(string tariffCode, List<TariffAttributeDataRow> tariffAttributeDataRowList)
			{
				return base.CreateAttributeFromTariffAttributeDataRow(tariffCode, tariffAttributeDataRowList);
			}
		}


		[SetUp]
		public void Init()
		{
			SystemContext.Now = () => new DateTime(2018, 07, 23, 13, 52, 15);
		}

		[Test]
		public void TestGroupTariffColumn2DataRows()
		{
			var rows = new List<TariffColumn2DataRow>
			{
				new TariffColumn2DataRow("01012100GT9999999920131129          0000000000                                    GT        "),
				new TariffColumn2DataRow("01012100HN9999999920131129          0000000000                                    HN        "),
				new TariffColumn2DataRow("01012100NI9999999920131129          0000000000                                    NI        "),
				new TariffColumn2DataRow("01012100NZ9999999920131201          0000000000                                    NZ        "),
				new TariffColumn2DataRow("01012100PA9999999920131129          0000000000                                    PA        "),
				new TariffColumn2DataRow("01012100SG9999999920140419          0000000000                                    SG        "),
				new TariffColumn2DataRow("01012100SV9999999920131129          0000000000                                    SV        "),
				new TariffColumn2DataRow("01061490GT9999999920170101          0000000000                                    GT        "),
				new TariffColumn2DataRow("01061490HN9999999920170101          0000000000                                    HN        "),
				new TariffColumn2DataRow("01061490NI9999999920170101          0000000000                                    NI        "),
				new TariffColumn2DataRow("01061490NZ9999999920170101          0000000000                                    NZ        "),
				new TariffColumn2DataRow("01061490PA9999999920170101          0000000000                                    PA        "),
				new TariffColumn2DataRow("01061490SV9999999920170101          0000000000                                    SV        "),
				new TariffColumn2DataRow("02075100NZ9999999920180101          0000000000                                    NZ        "),
				new TariffColumn2DataRow("02075100PA9999999920180101          0000000000                                    PA        "),
				new TariffColumn2DataRow("02075100GT9999999920180101          0000003300                                    GT        "),
				new TariffColumn2DataRow("02075100HN9999999920180101          0000006600                                    HN        "),
				new TariffColumn2DataRow("02075100NI9999999920180101          0000006600                                    NI        "),
				new TariffColumn2DataRow("02075100SV9999999920180101          0000006600                                    SV        "),
				new TariffColumn2DataRow("02075100SG9999999920180101          0000012500                                    SG        "),
				new TariffColumn2DataRow("02011010GT9999999920170101          0000000000                                    GT        "),
				new TariffColumn2DataRow("02011010HN9999999920170101          0000000000                                    HN        "),
				new TariffColumn2DataRow("02011010NI9999999920170101          0000000000                                    NI        "),
				new TariffColumn2DataRow("02011010NZ9999999920170101          0000000000                                    NZ        "),
				new TariffColumn2DataRow("02011010PA9999999920170101          0000000000                                    PA        "),
				new TariffColumn2DataRow("02011010SG9999999920170101          0000000000                                    SG        "),
				new TariffColumn2DataRow("02011010SV9999999920170101          0000000000                                    SV        "),
				new TariffColumn2DataRow("01031000GT9999999920060701          0000000000                                    GT        "),
				new TariffColumn2DataRow("01031000HN9999999920080715          0000000000                                    HN        "),
				new TariffColumn2DataRow("01031000AF9999999920051209          0000000000                                    LDCs      "),
				new TariffColumn2DataRow("01031000AO9999999920051209          0000000000                                    LDCs      "),
				new TariffColumn2DataRow("01031000BD9999999920051209          0000000000                                    LDCs      "),
				new TariffColumn2DataRow("01031000BF9999999920051209          0000000000                                    LDCs      "),
				new TariffColumn2DataRow("01031000BI9999999920051209          0000000000                                    LDCs      "),
				new TariffColumn2DataRow("01031000BJ9999999920051209          0000000000                                    LDCs      "),
				new TariffColumn2DataRow("01031000BT9999999920051209          0000000000                                    LDCs      "),
				new TariffColumn2DataRow("01031000CD9999999920051209          0000000000                                    LDCs      "),
				new TariffColumn2DataRow("01031000CF9999999920051209          0000000000                                    LDCs      "),
				new TariffColumn2DataRow("01031000DJ9999999920051209          0000000000                                    LDCs      "),
				new TariffColumn2DataRow("01031000ER9999999920051209          0000000000                                    LDCs      "),
				new TariffColumn2DataRow("01031000ET9999999920051209          0000000000                                    LDCs      "),
				new TariffColumn2DataRow("01031000GM9999999920051209          0000000000                                    LDCs      "),
				new TariffColumn2DataRow("01031000GN9999999920051209          0000000000                                    LDCs      "),
				new TariffColumn2DataRow("01031000GW9999999920051209          0000000000                                    LDCs      "),
				new TariffColumn2DataRow("01031000HT9999999920051209          0000000000                                    LDCs      "),
				new TariffColumn2DataRow("01031000KH9999999920051209          0000000000                                    LDCs      "),
				new TariffColumn2DataRow("01031000KI9999999920051209          0000000000                                    LDCs      "),
				new TariffColumn2DataRow("01031000NZ9999999920131201          0000000000                                    NZ        "),
				new TariffColumn2DataRow("01031000PA9999999920040101          0000000000                                    PA        "),
				new TariffColumn2DataRow("01031000SG9999999920140419          0000000000                                    SG        "),
				new TariffColumn2DataRow("01031000SV9999999920080301          0000000000                                    SV         ")
			};

			var tariffUpdateInfo = new TariffUpdateInfoForTest(new updateInfoBean(), string.Empty);
			var result = tariffUpdateInfo.GroupTariffColumn2DataRows(rows);
			Assert.AreEqual(16, result.Count);
			Assert.AreEqual(2, result.Where(t => t.tariffColumn2DataRows.First().TradeGroup == TradeGroup.SPE.ToString()).Count());
			Assert.AreEqual(1, result.Where(t => t.tariffColumn2DataRows.First().TradeGroup == TradeGroup.LDC.ToString()).Count());
			Assert.AreEqual(13, result.Where(t => t.tariffColumn2DataRows.First().TradeGroup != TradeGroup.SPE.ToString()
			&& t.tariffColumn2DataRows.First().TradeGroup != TradeGroup.LDC.ToString()).Count());
		}

		[Test]
		public void TestCreateRefCusRateElement_RefCusRate()
		{
			var currentTaipeiTime = TimeZoneInfo.ConvertTimeFromUtc(SystemContext.UtcNow(), TimeZoneInfo.FindSystemTimeZoneById("Taipei Standard Time"));
			var tariffUpdateInfo = new TariffUpdateInfoForTest(new updateInfoBean(), string.Empty);

			var refCusRate = new RefCusRate();
			refCusRate.ZZ2_EndDate = currentTaipeiTime.AddDays(-1);
			refCusRate.ZZ2_RateFormula = "XXX";
			var result = tariffUpdateInfo.CreateRefCusRates(new RefCusRate[] { refCusRate });
			Assert.AreEqual(false, result.Any());

			refCusRate = new RefCusRate();
			refCusRate.ZZ2_EndDate = currentTaipeiTime.AddDays(1);
			refCusRate.ZZ2_RateFormula = string.Empty;
			result = tariffUpdateInfo.CreateRefCusRates(new RefCusRate[] { refCusRate });
			Assert.AreEqual(false, result.Any());

			refCusRate = new RefCusRate();
			refCusRate.ZZ2_EndDate = currentTaipeiTime.AddDays(-1);
			refCusRate.ZZ2_RateFormula = string.Empty;
			result = tariffUpdateInfo.CreateRefCusRates(new RefCusRate[] { refCusRate });
			Assert.AreEqual(false, result.Any());

			refCusRate = new RefCusRate();
			refCusRate.ZZ2_EndDate = currentTaipeiTime.AddDays(1);
			refCusRate.ZZ2_RateFormula = "XXX";
			result = tariffUpdateInfo.CreateRefCusRates(new RefCusRate[] { refCusRate });
			Assert.AreEqual(true, result.Any());

			refCusRate = new RefCusRate();
			refCusRate.ZZ2_EndDate = new DateTime(2079, 6, 6, 11, 59, 00);
			refCusRate.ZZ2_StartDate = new DateTime(2013, 11, 29, 12, 00, 00);
			refCusRate.ZZ2_RateFormula = "0.025*VFD";
			refCusRate.ZZ2_RateFormulaDerivedFrom = "0.025";
			refCusRate.ZZ2_ZY1_NKRateCode = "DTA";
			refCusRate.ZZ2_ZZS_NKPreference = "STD";
			var cusApp = refCusRate.AddNewRefCusApplicability();
			cusApp.ZZT_ZZA_NKTradeGroup = "ALL";

			result = tariffUpdateInfo.CreateRefCusRates(new RefCusRate[] { refCusRate });
			AssertRefCusRateXml(result, @"<RefCusRate>
    <ZZ2_EndDate>2079-06-06T11:59:00</ZZ2_EndDate>
    <ZZ2_RateFormula>0.025*VFD</ZZ2_RateFormula>
    <ZZ2_RateFormulaDerivedFrom>0.025</ZZ2_RateFormulaDerivedFrom>
    <ZZ2_StartDate>2013-11-29T12:00:00</ZZ2_StartDate>
    <ZZ2_ZY1_NKRateCode>DTA</ZZ2_ZY1_NKRateCode>
    <ZZ2_ZZS_NKPreference>STD</ZZ2_ZZS_NKPreference>
    <RefCusApplicability>
      <ZZT_EndDate>2079-06-06T11:59:00</ZZT_EndDate>
      <ZZT_OrderNumber />
      <ZZT_StartDate>2013-11-29T12:00:00</ZZT_StartDate>
      <ZZT_ZZA_NKTradeGroup>ALL</ZZT_ZZA_NKTradeGroup>
    </RefCusApplicability>
  </RefCusRate>");

			cusApp.RefCusExcludedTradeGroup = new List<string>(new string[] { "AA", "BB" });
			result = tariffUpdateInfo.CreateRefCusRates(new RefCusRate[] { refCusRate });
			AssertRefCusRateXml(result, @"<RefCusRate>
    <ZZ2_EndDate>2079-06-06T11:59:00</ZZ2_EndDate>
    <ZZ2_RateFormula>0.025*VFD</ZZ2_RateFormula>
    <ZZ2_RateFormulaDerivedFrom>0.025</ZZ2_RateFormulaDerivedFrom>
    <ZZ2_StartDate>2013-11-29T12:00:00</ZZ2_StartDate>
    <ZZ2_ZY1_NKRateCode>DTA</ZZ2_ZY1_NKRateCode>
    <ZZ2_ZZS_NKPreference>STD</ZZ2_ZZS_NKPreference>
    <RefCusApplicability>
      <ZZT_EndDate>2079-06-06T11:59:00</ZZT_EndDate>
      <ZZT_OrderNumber />
      <ZZT_StartDate>2013-11-29T12:00:00</ZZT_StartDate>
      <ZZT_ZZA_NKTradeGroup>ALL</ZZT_ZZA_NKTradeGroup>
      <RefCusExcludedTradeGroup>
        <ZZC_ZZA_NKTradeGroup>AA</ZZC_ZZA_NKTradeGroup>
      </RefCusExcludedTradeGroup>
      <RefCusExcludedTradeGroup>
        <ZZC_ZZA_NKTradeGroup>BB</ZZC_ZZA_NKTradeGroup>
      </RefCusExcludedTradeGroup>
    </RefCusApplicability>
  </RefCusRate>");

			cusApp.ZZT_OrderNumber = "ABC";
			result = tariffUpdateInfo.CreateRefCusRates(new RefCusRate[] { refCusRate });
			AssertRefCusRateXml(result, @"<RefCusRate>
    <ZZ2_EndDate>2079-06-06T11:59:00</ZZ2_EndDate>
    <ZZ2_RateFormula>0.025*VFD</ZZ2_RateFormula>
    <ZZ2_RateFormulaDerivedFrom>0.025</ZZ2_RateFormulaDerivedFrom>
    <ZZ2_StartDate>2013-11-29T12:00:00</ZZ2_StartDate>
    <ZZ2_ZY1_NKRateCode>DTA</ZZ2_ZY1_NKRateCode>
    <ZZ2_ZZS_NKPreference>STD</ZZ2_ZZS_NKPreference>
    <RefCusApplicability>
      <ZZT_EndDate>2079-06-06T11:59:00</ZZT_EndDate>
      <ZZT_OrderNumber>ABC</ZZT_OrderNumber>
      <ZZT_StartDate>2013-11-29T12:00:00</ZZT_StartDate>
      <ZZT_ZZA_NKTradeGroup>ALL</ZZT_ZZA_NKTradeGroup>
      <RefCusExcludedTradeGroup>
        <ZZC_ZZA_NKTradeGroup>AA</ZZC_ZZA_NKTradeGroup>
      </RefCusExcludedTradeGroup>
      <RefCusExcludedTradeGroup>
        <ZZC_ZZA_NKTradeGroup>BB</ZZC_ZZA_NKTradeGroup>
      </RefCusExcludedTradeGroup>
    </RefCusApplicability>
  </RefCusRate>");
		}

		[Test]
		public void TestCreateAttributeFromTariffAttributeDataRow()
		{
			var tariffAttributeDataRow1 = new TariffAttributeDataRow("87120010109,F5FTZDestination,AT;BE");
			var tariffAttributeDataRow2 = new TariffAttributeDataRow("87120010110,F5FTZDestination,BG;CY");
			var tariffAttributeDataRowList = new List<TariffAttributeDataRow>() { tariffAttributeDataRow1 , tariffAttributeDataRow2};
			var tariffUpdateInfo = new TariffUpdateInfoForTest(new updateInfoBean(), string.Empty);

			var tariffAttributes = tariffUpdateInfo.CreateAttributeFromTariffAttributeDataRow("87120010108", tariffAttributeDataRowList);
			Assert.AreEqual(0, tariffAttributes.Count());

			tariffAttributes = tariffUpdateInfo.CreateAttributeFromTariffAttributeDataRow("87120010109", tariffAttributeDataRowList);
			Assert.AreEqual(1, tariffAttributes.Count());
			Assert.AreEqual("F5FTZDestination", tariffAttributes.ElementAt(0).ZZ3_Name);
			Assert.AreEqual("AT;BE", tariffAttributes.ElementAt(0).ZZ3_Value);

			tariffAttributes = tariffUpdateInfo.CreateAttributeFromTariffAttributeDataRow("87120010110", tariffAttributeDataRowList);
			Assert.AreEqual(1, tariffAttributes.Count());
			Assert.AreEqual("F5FTZDestination", tariffAttributes.ElementAt(0).ZZ3_Name);
			Assert.AreEqual("BG;CY", tariffAttributes.ElementAt(0).ZZ3_Value);
		}

		struct TestCaseForTariffColumn1And3
		{
			public DateTime ExpectedStartDate;
			public DateTime ExpectedEndDate;
			public (string RateFormula, string RateFormulaDerivedFrom)? Column3SpecificRateInfo;
			public (string RateFormula, string RateFormulaDerivedFrom)? Column3AdValoremRateInfo;
			public (string RateFormula, string RateFormulaDerivedFrom)? Column1SpecificRateInfo;
			public (string RateFormula, string RateFormulaDerivedFrom)? Column1AdValoremRateInfo;

			public TestCaseForTariffColumn1And3(DateTime expectedStartDate, DateTime expectedEndDate,
				(string RateFormula, string RateFormulaDerivedFrom)? column3SpecificRateInfo,
				(string RateFormula, string RateFormulaDerivedFrom)? column3AdValoremRateInfo,
				(string RateFormula, string RateFormulaDerivedFrom)? column1SpecificRateInfo,
				(string RateFormula, string RateFormulaDerivedFrom)? column1AdValoremRateInfo)
			{
				ExpectedStartDate = expectedStartDate;
				ExpectedEndDate = expectedEndDate;
				Column3SpecificRateInfo = column3SpecificRateInfo;
				Column3AdValoremRateInfo = column3AdValoremRateInfo;
				Column1SpecificRateInfo = column1SpecificRateInfo;
				Column1AdValoremRateInfo = column1AdValoremRateInfo;
			}
		}

		void AssertRefCusRateElement_TariffColumn1And3(IEnumerable<Common.UniversalXmlWriter.EntityType.RefCusRate> enumerableResult, TestCaseForTariffColumn1And3 testCase, bool forProvisional = false)
		{
			var expectedStartDate = testCase.ExpectedStartDate;
			var expectedEndDate = testCase.ExpectedEndDate;
			var column3SpecificRateInfo = testCase.Column3SpecificRateInfo;
			var column3AdValoremRateInfo = testCase.Column3AdValoremRateInfo;
			var column1SpecificRateInfo = testCase.Column1SpecificRateInfo;
			var column1AdValoremRateInfo = testCase.Column1AdValoremRateInfo;
			var messagePrefix = $"{expectedStartDate} - {expectedEndDate}: ";

			var column3ZZSValue = forProvisional ? "PT3" : "STD";
			var column1ZZSValue = forProvisional ? "PT1" : "PR1";

			var qColumn3SpecificRate = enumerableResult.Where(r => r.ZZ2_ZY1_NKRateCode == "DTS" && r.ZZ2_ZZS_NKPreference == column3ZZSValue && r.ZZ2_StartDate == expectedStartDate && r.ZZ2_EndDate == expectedEndDate);
			if (column3SpecificRateInfo == null)
			{
				Assert.AreEqual(0, qColumn3SpecificRate.Count(), message: $"{messagePrefix}qColumn3SpecificRate.Count");
			}
			else
			{
				Assert.AreEqual(1, qColumn3SpecificRate.Count(), message: $"{messagePrefix}qColumn3SpecificRate.Count");
				Assert.Multiple(() =>
				{
					var firstRate = qColumn3SpecificRate.First();
					Assert.AreEqual(column3SpecificRateInfo?.RateFormula, firstRate.ZZ2_RateFormula, message: $"{messagePrefix}qColumn3SpecificRate.ZZ2_RateFormula");
					Assert.AreEqual(column3SpecificRateInfo?.RateFormulaDerivedFrom, firstRate.ZZ2_RateFormulaDerivedFrom, message: $"{messagePrefix}qColumn3SpecificRate.ZZ2_RateFormulaDerivedFrom");
					Assert.AreEqual("ALL", firstRate.RefCusApplicabilities.First().ZZT_ZZA_NKTradeGroup, message: $"{messagePrefix}qColumn3SpecificRate.ZZT_ZZA_NKTradeGroup");
				});
			}

			var qColumn3AdValoremRate = enumerableResult.Where(r => r.ZZ2_ZY1_NKRateCode == "DTA" && r.ZZ2_ZZS_NKPreference == column3ZZSValue && r.ZZ2_StartDate == expectedStartDate && r.ZZ2_EndDate == expectedEndDate);
			if (column3AdValoremRateInfo == null)
			{
				Assert.AreEqual(0, qColumn3AdValoremRate.Count(), message: $"{messagePrefix}column3AdValoremRate.Count");
			}
			else
			{
				Assert.AreEqual(1, qColumn3AdValoremRate.Count(), message: $"{messagePrefix}column3AdValoremRate.Count");
				Assert.Multiple(() =>
				{
					var firstRate = qColumn3AdValoremRate.First();
					Assert.AreEqual(column3AdValoremRateInfo?.RateFormula, firstRate.ZZ2_RateFormula, message: $"{messagePrefix}column3AdValoremRate.ZZ2_RateFormula");
					Assert.AreEqual(column3AdValoremRateInfo?.RateFormulaDerivedFrom, firstRate.ZZ2_RateFormulaDerivedFrom, message: $"{messagePrefix}column3AdValoremRate.ZZ2_RateFormulaDerivedFrom");
					Assert.AreEqual("ALL", firstRate.RefCusApplicabilities.First().ZZT_ZZA_NKTradeGroup, message: $"{messagePrefix}column3AdValoremRate.ZZT_ZZA_NKTradeGroup");
				});
			}

			var qColumn1SpecificRate = enumerableResult.Where(r => r.ZZ2_ZY1_NKRateCode == "DTS" && r.ZZ2_ZZS_NKPreference == column1ZZSValue && r.ZZ2_StartDate == expectedStartDate && r.ZZ2_EndDate == expectedEndDate);
			if (column1SpecificRateInfo == null)
			{
				Assert.AreEqual(0, qColumn1SpecificRate.Count(), message: $"{messagePrefix}column1SpecificRate.Count");
			}
			else
			{
				Assert.AreEqual(1, qColumn1SpecificRate.Count(), message: $"{messagePrefix}column1SpecificRate.Count");
				Assert.AreEqual(2, qColumn1SpecificRate.First().RefCusApplicabilities.Count(), message: $"{messagePrefix}column1SpecificRate.RefCusApplicability.Count");
				Assert.Multiple(() =>
				{
					var firstRate = qColumn1SpecificRate.First();
					Assert.AreEqual(column1SpecificRateInfo?.RateFormula, firstRate.ZZ2_RateFormula, message: $"{messagePrefix}column1SpecificRate.ZZ2_RateFormula");
					Assert.AreEqual(column1SpecificRateInfo?.RateFormulaDerivedFrom, firstRate.ZZ2_RateFormulaDerivedFrom, message: $"{messagePrefix}column1SpecificRate.ZZ2_RateFormulaDerivedFrom");
					var q1 = firstRate.RefCusApplicabilities.Where(a => a.ZZT_ZZA_NKTradeGroup == "FTA");
					Assert.AreEqual(1, q1.Count());
					var q2 = qColumn1SpecificRate.SelectMany(rate => rate.RefCusApplicabilities.Where(a => a.ZZT_ZZA_NKTradeGroup == "WTO"));
					Assert.AreEqual(1, q2.Count());
				});
			}
			var qColumn1AdValoremRate = enumerableResult.Where(r => r.ZZ2_ZY1_NKRateCode == "DTA" && r.ZZ2_ZZS_NKPreference == column1ZZSValue && r.ZZ2_StartDate == expectedStartDate && r.ZZ2_EndDate == expectedEndDate);
			if (column1AdValoremRateInfo == null)
			{
				Assert.AreEqual(0, qColumn1AdValoremRate.Count(), message: $"{messagePrefix}column1AdValoremRate.Count");
			}
			else
			{
				Assert.AreEqual(1, qColumn1AdValoremRate.Count(), message: $"{messagePrefix}column1AdValoremRate.Count");
				Assert.AreEqual(2, qColumn1AdValoremRate.First().RefCusApplicabilities.Count(), message: $"{messagePrefix}column1AdValoremRate.RefCusApplicability.Count");
				Assert.Multiple(() =>
				{
					var firstRate = qColumn1AdValoremRate.First();
					Assert.AreEqual(column1AdValoremRateInfo?.RateFormula, firstRate.ZZ2_RateFormula, message: $"{messagePrefix}column1AdValoremRate.ZZ2_RateFormula");
					Assert.AreEqual(column1AdValoremRateInfo?.RateFormulaDerivedFrom, firstRate.ZZ2_RateFormulaDerivedFrom, message: $"{messagePrefix}column1AdValoremRate.ZZ2_RateFormulaDerivedFrom");
					var q1 = firstRate.RefCusApplicabilities.Where(a => a.ZZT_ZZA_NKTradeGroup == "FTA");
					Assert.AreEqual(1, q1.Count());
					var q2 = qColumn1AdValoremRate.SelectMany(rate => rate.RefCusApplicabilities.Where(a => a.ZZT_ZZA_NKTradeGroup == "WTO"));
					Assert.AreEqual(1, q2.Count());
				});
			}
		}

		[Test]
		public void TestCreateRefCusRateElement_TariffColumn1And3DataRow()
		{
			var tariffUpdateInfo = new TariffUpdateInfoForTest(new updateInfoBean(), string.Empty);

			var tariffDataWithoutProvisional = new TariffColumn1And3DataRow("0106900090599999999201001150003800000000003000000010000000000009000KGM                                                           KGM            B01                                                     ");
			var enumerableResult = tariffUpdateInfo.CreateRefCusRates(tariffDataWithoutProvisional);
			Assert.AreEqual(4, enumerableResult.Count());
			AssertRefCusRateElement_TariffColumn1And3(enumerableResult, new TestCaseForTariffColumn1And3(new DateTime(2010, 1, 15, 0, 0, 0), new DateTime(2079, 6, 6, 23, 59, 0), ("38*[KGM]", "38/KGM"), ("0.3*VFD", "0.3"), ("10*[KGM]", "10/KGM"), ("0.09*VFD", "0.09")));

			var tariffDataWithProvisional = new TariffColumn1And3DataRow("0201101000399999999201001150003800000000003000000010000000000009000KGM00039000000000035000000050000000000100002020011420111201   KGMD*          B01 F01 MW0                                             ");
			enumerableResult = tariffUpdateInfo.CreateRefCusRates(tariffDataWithProvisional);
			Assert.AreEqual(8, enumerableResult.Count());
			AssertRefCusRateElement_TariffColumn1And3(enumerableResult, new TestCaseForTariffColumn1And3(new DateTime(2010, 1, 15, 0, 0, 0), new DateTime(2079, 6, 6, 23, 59, 00), ("38*[KGM]", "38/KGM"), ("0.3*VFD", "0.3"), ("10*[KGM]", "10/KGM"), ("0.09*VFD", "0.09")));
			AssertRefCusRateElement_TariffColumn1And3(enumerableResult, new TestCaseForTariffColumn1And3(new DateTime(2011, 12, 1, 0, 0, 0), new DateTime(2020, 1, 14, 23, 59, 59), ("39*[KGM]", "39/KGM"), ("0.35*VFD", "0.35"), ("5*[KGM]", "5/KGM"), ("0.1*VFD", "0.1")), forProvisional: true);

			var tariffData2 = new TariffColumn1And3DataRow("020110100039999999920220115000380000000000300000001000000          KGM000380000000000300000000500000          2024033120211201   KGMD*          B01 F01 MW0                                             ");
			enumerableResult = tariffUpdateInfo.CreateRefCusRates(tariffData2);
			Assert.AreEqual(6, enumerableResult.Count());
			AssertRefCusRateElement_TariffColumn1And3(enumerableResult, new TestCaseForTariffColumn1And3(new DateTime(2022, 1, 15, 0, 0, 0), new DateTime(2079, 6, 6, 23, 59, 00), ("38*[KGM]", "38/KGM"), ("0.3*VFD", "0.3"), ("10*[KGM]", "10/KGM"), null));
			AssertRefCusRateElement_TariffColumn1And3(enumerableResult, new TestCaseForTariffColumn1And3(new DateTime(2021, 12, 1, 0, 0, 0), new DateTime(2024, 3, 31, 23, 59, 59), ("38*[KGM]", "38/KGM"), ("0.3*VFD", "0.3"), ("5*[KGM]", "5/KGM"), null), forProvisional: true);

			var tariffDataForQuotaTariffCode = new TariffColumn1And3DataRow("980500000099999999920040101          0000022500          0000022500                                                              KGM            B01 MW0                                                 ");
			enumerableResult = tariffUpdateInfo.CreateRefCusRates(tariffDataForQuotaTariffCode).Where(x => x != null);
			var q9 = enumerableResult.Where(r => r.RefCusApplicabilities.Any(a => a.ZZT_ZZA_NKTradeGroup == "WTO"));
			Assert.AreEqual(1, q9.Count());
			q9 = enumerableResult.Where(r => r.RefCusApplicabilities.Any(a => a.ZZT_ZZA_NKTradeGroup != "WTO"));
			Assert.AreEqual(0, q9.Count());
			q9 = enumerableResult.Where(r => r.RefCusApplicabilities.Count() != 1);
			Assert.AreEqual(0, q9.Count());
			foreach (var item in enumerableResult)
			{
				Assert.AreEqual("QUOTA", item.RefCusApplicabilities.First().ZZT_OrderNumber);
			}

			var tariffDataForNotQuotaTariffCode = new TariffColumn1And3DataRow("989900000069999999920060623          0000005000          0000005000                                                              KGM                                                                    ");
			enumerableResult = tariffUpdateInfo.CreateRefCusRates(tariffDataForNotQuotaTariffCode);
			Assert.AreEqual(2, enumerableResult.Count());

			var q10 = enumerableResult.Where(r => r.RefCusApplicabilities.Count() == 2);
			Assert.AreEqual(1, q10.Count());

			var q10_1 = q10.SelectMany(r => r.RefCusApplicabilities.Where(a => a.ZZT_ZZA_NKTradeGroup == "FTA"));
			Assert.AreEqual(1, q10_1.Count());

			var q10_2 = q10.SelectMany(r => r.RefCusApplicabilities.Where(a => a.ZZT_ZZA_NKTradeGroup == "WTO"));
			Assert.AreEqual(1, q10_2.Count());

			var tariffData = new TariffColumn1And3DataRow("010310000049999999920130101          0000002500          0000002500                                                           HEDKGM            401 B01                     441");
			enumerableResult = tariffUpdateInfo.CreateRefCusRates(tariffData);
			Assert.AreEqual(2, enumerableResult.Count());
		}

		struct TestCaseForTariffColumn2
		{
			public DateTime ExpectedStartDate;
			public DateTime ExpectedEndDate;
			public (string RateFormula, string RateFormulaDerivedFrom)? AdValoremRateInfo;
			public (string RateFormula, string RateFormulaDerivedFrom)? SpecificRateRateInfo;

			public TestCaseForTariffColumn2(DateTime expectedStartDate, DateTime expectedEndDate,
				(string RateFormula, string RateFormulaDerivedFrom)? adValoremRateInfo,
				(string RateFormula, string RateFormulaDerivedFrom)? specificRateRateInfo)
			{
				ExpectedStartDate = expectedStartDate;
				ExpectedEndDate = expectedEndDate;
				AdValoremRateInfo = adValoremRateInfo;
				SpecificRateRateInfo = specificRateRateInfo;
			}
		}

		void AssertRefCusRateElement_TariffColumn2(IEnumerable<Common.UniversalXmlWriter.EntityType.RefCusRate> enumerableResult, TestCaseForTariffColumn2 testCase, bool forProvisional = false)
		{
			var expectedStartDate = testCase.ExpectedStartDate;
			var expectedEndDate = testCase.ExpectedEndDate;
			var adValoremRateInfo = testCase.AdValoremRateInfo;
			var specificRateRateInfo = testCase.SpecificRateRateInfo;
			var messagePrefix = $"{expectedStartDate} - {expectedEndDate}: ";

			var column2ZZSValue = forProvisional ? "PT2" : "PR2";

			var qAdValoremRate = enumerableResult.Where(r => r.ZZ2_ZY1_NKRateCode == "DTA" && r.ZZ2_ZZS_NKPreference == column2ZZSValue && r.ZZ2_StartDate == expectedStartDate && r.ZZ2_EndDate == expectedEndDate);
			if (adValoremRateInfo == null)
			{
				Assert.AreEqual(0, qAdValoremRate.Count(), message: $"{messagePrefix}adValoremRateInfo.Count");
			}
			else
			{
				Assert.AreEqual(1, qAdValoremRate.Count(), message: $"{messagePrefix}adValoremRateInfo.Count");
				Assert.Multiple(() =>
				{
					var firstRate = qAdValoremRate.First();
					Assert.AreEqual(adValoremRateInfo?.RateFormula, firstRate.ZZ2_RateFormula, message: $"{messagePrefix}adValoremRateInfo.ZZ2_RateFormula");
					Assert.AreEqual(adValoremRateInfo?.RateFormulaDerivedFrom, firstRate.ZZ2_RateFormulaDerivedFrom, message: $"{messagePrefix}adValoremRateInfo.ZZ2_RateFormulaDerivedFrom");
					Assert.AreEqual("GT", firstRate.RefCusApplicabilities.First().ZZT_ZZA_NKTradeGroup, message: $"{messagePrefix}adValoremRateInfo.ZZT_ZZA_NKTradeGroup");
				});
			}

			var qSpecificRate = enumerableResult.Where(r => r.ZZ2_ZY1_NKRateCode == "DTS" && r.ZZ2_ZZS_NKPreference == column2ZZSValue && r.ZZ2_StartDate == expectedStartDate && r.ZZ2_EndDate == expectedEndDate);
			if (specificRateRateInfo == null)
			{
				Assert.AreEqual(0, qSpecificRate.Count(), message: $"{messagePrefix}qSpecificRateRate.Count");
			}
			else
			{
				Assert.AreEqual(1, qSpecificRate.Count());
				Assert.Multiple(() =>
				{
					var firstRate = qSpecificRate.First();
					Assert.AreEqual(specificRateRateInfo?.RateFormula, firstRate.ZZ2_RateFormula, message: $"{messagePrefix}qSpecificRateRate.ZZ2_RateFormula");
					Assert.AreEqual(specificRateRateInfo?.RateFormulaDerivedFrom, firstRate.ZZ2_RateFormulaDerivedFrom, message: $"{messagePrefix}qSpecificRateRate.ZZ2_RateFormulaDerivedFrom");
					Assert.AreEqual("GT", firstRate.RefCusApplicabilities.First().ZZT_ZZA_NKTradeGroup, message: $"{messagePrefix}qSpecificRateRate.ZZT_ZZA_NKTradeGroup");
				});
			}
		}

		[Test]
		public void TestCreateRefCusRateElement_TariffColumn2DataRowGroup()
		{
			var currentTaipeiTime = TimeZoneInfo.ConvertTimeFromUtc(SystemContext.UtcNow(), TimeZoneInfo.FindSystemTimeZoneById("Taipei Standard Time"));
			var tariffUpdateInfo = new TariffUpdateInfoForTest(new updateInfoBean(), string.Empty);
			var specificRateUnit = "KGM";

			var col2TariffDataWithoutProvisional = new TariffColumn2DataRow("03035400GT999999992003010100007300000000025000                                    GT        ");
			var col2TariffDataGroup = new TariffColumn2DataRowGroup();
			col2TariffDataGroup.tariffColumn2DataRows.Add(col2TariffDataWithoutProvisional);
			var enumerableResult = tariffUpdateInfo.CreateRefCusRateElement(specificRateUnit, col2TariffDataGroup);
			Assert.AreEqual(2, enumerableResult.Where(x => x != null).Count());
			AssertRefCusRateElement_TariffColumn2(enumerableResult, new TestCaseForTariffColumn2(new DateTime(2003, 1, 1, 0, 0, 0), new DateTime(2079, 6, 6, 23, 59, 0), ("0.25*VFD", "0.25"), ("7.3*[KGM]", "7.3/KGM")));

			var col2TariffDataWithProvisional = new TariffColumn2DataRow("03035400GT999999992003010100007300000000025000202001142011120100001500000000030000GT        ");
			col2TariffDataGroup = new TariffColumn2DataRowGroup();
			col2TariffDataGroup.tariffColumn2DataRows.Add(col2TariffDataWithProvisional);
			enumerableResult = tariffUpdateInfo.CreateRefCusRateElement(specificRateUnit, col2TariffDataGroup);
			Assert.AreEqual(4, enumerableResult.Where(x => x != null).Count());
			AssertRefCusRateElement_TariffColumn2(enumerableResult, new TestCaseForTariffColumn2(new DateTime(2003, 1, 1, 0, 0, 0), new DateTime(2079, 06, 06, 23, 59, 0), ("0.25*VFD", "0.25"), ("7.3*[KGM]", "7.3/KGM")));
			AssertRefCusRateElement_TariffColumn2(enumerableResult, new TestCaseForTariffColumn2(new DateTime(2011, 12, 1, 0, 0, 0), new DateTime(2020, 01, 14, 23, 59, 59), ("0.3*VFD", "0.3"), ("1.5*[KGM]", "1.5/KGM")), forProvisional: true);

			var col2TariffData2 = new TariffColumn2DataRow("03035400GT99999999200301010000730000          2020011420111201          0000030000GT        ");
			col2TariffDataGroup = new TariffColumn2DataRowGroup();
			col2TariffDataGroup.tariffColumn2DataRows.Add(col2TariffData2);
			enumerableResult = tariffUpdateInfo.CreateRefCusRateElement(specificRateUnit, col2TariffDataGroup);
			Assert.AreEqual(2, enumerableResult.Where(x => x != null).Count());
			AssertRefCusRateElement_TariffColumn2(enumerableResult, new TestCaseForTariffColumn2(new DateTime(2003, 1, 1, 0, 0, 0), new DateTime(2079, 6, 6, 23, 59, 0), null, ("7.3*[KGM]", "7.3/KGM")));
			AssertRefCusRateElement_TariffColumn2(enumerableResult, new TestCaseForTariffColumn2(new DateTime(2011, 12, 1, 0, 0, 0), new DateTime(2020, 01, 14, 23, 59, 59), ("0.3*VFD", "0.3"), null), forProvisional: true);

			var col2TariffData = new TariffColumn2DataRow("01012100GT9999999920131129          0000000000                                    GT        ");
			var col2TariffDataList = new List<TariffColumn2DataRow>
			{
				col2TariffData
			};

			var col2OverrideList = new List<TariffColumn2OverrideDataRow>
			{
				new TariffColumn2OverrideDataRow("01012100,GT,20190930,20190101,,0000000000,QUOTA"),
				new TariffColumn2OverrideDataRow("01012100,GT,20190930,20190101,,0001500000,")
			};
			col2TariffDataList = tariffUpdateInfo.ApplyOverrides(col2TariffDataList, col2OverrideList);

			Assert.AreEqual(2, col2TariffDataList.Count);
		}

		[Test]
		public void TestCompositeKeyOnZZ5()
		{
			var tariffDataRowList = new List<TariffColumn1And3DataRow>();
			var tariffRow = new TariffColumn1And3DataRow("010310000049999999920130101          0000002500          0000002500                                                           HEDKGM            401 B01                     441");
			var chapterAndSection = new KeyValuePair<int, int>(1, 1);
			tariffDataRowList.Add(new TariffColumn1And3DataRow("010121000039999999920131129          0000002500          0000002500                                                           HEDKGM            401 B01                     441"));
			tariffDataRowList.Add(tariffRow);
			tariffDataRowList.Add(new TariffColumn1And3DataRow("010614900089999999920131129          0000015000          0000012500                                                           HEDKGM            B01"));

			var tariffUpdateInfo = new TariffUpdateInfoForTest(new updateInfoBean(), string.Empty);
			var result = tariffUpdateInfo.CompositeKeyOnZZ5(tariffDataRowList, tariffRow, chapterAndSection);
			Assert.AreEqual("01.01..03.1.10.10", result);
		}

		[Test]
		public void TestGetTens()
		{
			var tariffDataRowList = new List<TariffColumn1And3DataRow>();
			var tariffRow = new TariffColumn1And3DataRow("010614900189999999920131129          0000015000          0000012500                                                           HEDKGM            B01");
			tariffDataRowList.Add(new TariffColumn1And3DataRow("010121000039999999920131129          0000002500          0000002500                                                           HEDKGM            401 B01                     441"));
			tariffDataRowList.Add(new TariffColumn1And3DataRow("010310000049999999920130101          0000002500          0000002500                                                           HEDKGM            401 B01                     441"));
			tariffDataRowList.Add(new TariffColumn1And3DataRow("010614900089999999920131129          0000015000          0000012500                                                           HEDKGM            B01"));
			tariffDataRowList.Add(tariffRow);

			var result = TariffUpdateInfoForTest.GetTens(tariffDataRowList, tariffRow);
			Assert.AreEqual("10.20", result);

			tariffDataRowList.Add(new TariffColumn1And3DataRow("020110100039999999920140210000380000000000300000001000000          KGM                                                           KGMD*          B01 F01 MW0"));
			result = TariffUpdateInfoForTest.GetTens(tariffDataRowList, tariffDataRowList.Last());
			Assert.AreEqual("10.10", result);
		}

		[Test]
		public void TestExecute()
		{
			string tariffColumn1And3Url = "http://192.118.118.1/TARIFF_2.txt";
			string tariffColumn2Url = "http://192.118.118.1/TARIFF_3.txt";
			string tariffChineseDescriptionUrl = "http://192.118.118.1/note_10_C.txt";
			string tariffEnglishDescriptionUrl = "http://192.118.118.1/note_10_E.txt";
			string hsSectionsAndChaptersUrl = "http://192.118.118.1/enname.doc";
			string environmentalProtectionTariffsUrl = "http://192.118.118.1/EnvironmentalProtectionTariffs.ods";
			string fileName = Path.Combine(Utility.TempDirectory, string.Format("{0}.xml", Guid.NewGuid().ToString()));
			var info = new updateInfoBean();
			info.downloadURL = string.Join(";", new string[] { tariffColumn1And3Url, tariffColumn2Url, tariffChineseDescriptionUrl, hsSectionsAndChaptersUrl, environmentalProtectionTariffsUrl, tariffEnglishDescriptionUrl });
			var updateInfo = new TariffUpdateInfoForTest(info, fileName);
			updateInfo.Run();
			var actualXml = XDocument.Load(fileName);
			string path = Path.Combine(FolderHelper.GetBinFolder(), "doc/Tariff/Tariff.xml");
			var expectedXml = XDocument.Load(path);
			Assert.AreEqual(expectedXml.ToString(), actualXml.ToString());
			File.Delete(fileName);
		}

		[Test]
		[SetCulture("en-US")]
		public void TestExecuteWhenUS()
		{
			TestExecute();
		}

		[Test]
		[SetCulture("fr-FR")]
		public void TestExecuteWhenFR()
		{
			TestExecute();
		}

		[Test]
		public void TestOverrideLogicWithSingleDTARate()
		{
			var rows = new List<TariffColumn2DataRow>
			{
				new TariffColumn2DataRow("01041000GT9999999920160101          0000000000                                    GT        "),
				new TariffColumn2DataRow("01041000HN9999999920160101          0000000000                                    HN        "),
				new TariffColumn2DataRow("01041000NI9999999920160101          0000000000                                    NI        "),
				new TariffColumn2DataRow("01041000NZ9999999920160101          0000000000                                    NZ        "),
				new TariffColumn2DataRow("01041000PA9999999920160101          0000000000                                    PA        "),
				new TariffColumn2DataRow("01041000SG9999999920160101          0000000000                                    SG        "),
				new TariffColumn2DataRow("01041000SV9999999920160101          0000000000                                    SV        ")
			};

			var col2OverrideList = new List<TariffColumn2OverrideDataRow>
			{
				new TariffColumn2OverrideDataRow("01041000,GT,20190930,20170101,,0000000000,QUOTA"),
				new TariffColumn2OverrideDataRow("01041000,GT,20190930,20170101,,0001500000,")
			};

			var tariffUpdateInfo = new TariffUpdateInfoForTest(new updateInfoBean(), string.Empty);
			rows = tariffUpdateInfo.ApplyOverrides(rows, col2OverrideList);

			var groupedRows = tariffUpdateInfo.GroupTariffColumn2DataRows(rows);
			AssertRefCusRateXml(groupedRows, tariffUpdateInfo, @"<RefCusRate>
    <ZZ2_EndDate>2019-09-30T23:59:59</ZZ2_EndDate>
    <ZZ2_RateFormula>0</ZZ2_RateFormula>
    <ZZ2_RateFormulaDerivedFrom>0</ZZ2_RateFormulaDerivedFrom>
    <ZZ2_StartDate>2017-01-01T00:00:00</ZZ2_StartDate>
    <ZZ2_ZY1_NKRateCode>DTA</ZZ2_ZY1_NKRateCode>
    <ZZ2_ZZS_NKPreference>PT2</ZZ2_ZZS_NKPreference>
    <RefCusApplicability>
      <ZZT_EndDate>2019-09-30T23:59:00</ZZT_EndDate>
      <ZZT_OrderNumber>QUOTA</ZZT_OrderNumber>
      <ZZT_StartDate>2017-01-01T00:00:00</ZZT_StartDate>
      <ZZT_ZZA_NKTradeGroup>GT</ZZT_ZZA_NKTradeGroup>
    </RefCusApplicability>
  </RefCusRate>
  <RefCusRate>
    <ZZ2_EndDate>2019-09-30T23:59:59</ZZ2_EndDate>
    <ZZ2_RateFormula>15*VFD</ZZ2_RateFormula>
    <ZZ2_RateFormulaDerivedFrom>15</ZZ2_RateFormulaDerivedFrom>
    <ZZ2_StartDate>2017-01-01T00:00:00</ZZ2_StartDate>
    <ZZ2_ZY1_NKRateCode>DTA</ZZ2_ZY1_NKRateCode>
    <ZZ2_ZZS_NKPreference>PT2</ZZ2_ZZS_NKPreference>
    <RefCusApplicability>
      <ZZT_EndDate>2019-09-30T23:59:00</ZZT_EndDate>
      <ZZT_OrderNumber />
      <ZZT_StartDate>2017-01-01T00:00:00</ZZT_StartDate>
      <ZZT_ZZA_NKTradeGroup>GT</ZZT_ZZA_NKTradeGroup>
    </RefCusApplicability>
  </RefCusRate>
  <RefCusRate>
    <ZZ2_EndDate>2079-06-06T23:59:00</ZZ2_EndDate>
    <ZZ2_RateFormula>0</ZZ2_RateFormula>
    <ZZ2_RateFormulaDerivedFrom>0</ZZ2_RateFormulaDerivedFrom>
    <ZZ2_StartDate>2016-01-01T00:00:00</ZZ2_StartDate>
    <ZZ2_ZY1_NKRateCode>DTA</ZZ2_ZY1_NKRateCode>
    <ZZ2_ZZS_NKPreference>PR2</ZZ2_ZZS_NKPreference>
    <RefCusApplicability>
      <ZZT_EndDate>2079-06-06T23:59:00</ZZT_EndDate>
      <ZZT_OrderNumber />
      <ZZT_StartDate>2016-01-01T00:00:00</ZZT_StartDate>
      <ZZT_ZZA_NKTradeGroup>HN</ZZT_ZZA_NKTradeGroup>
    </RefCusApplicability>
    <RefCusApplicability>
      <ZZT_EndDate>2079-06-06T23:59:00</ZZT_EndDate>
      <ZZT_OrderNumber />
      <ZZT_StartDate>2016-01-01T00:00:00</ZZT_StartDate>
      <ZZT_ZZA_NKTradeGroup>NI</ZZT_ZZA_NKTradeGroup>
    </RefCusApplicability>
    <RefCusApplicability>
      <ZZT_EndDate>2079-06-06T23:59:00</ZZT_EndDate>
      <ZZT_OrderNumber />
      <ZZT_StartDate>2016-01-01T00:00:00</ZZT_StartDate>
      <ZZT_ZZA_NKTradeGroup>NZ</ZZT_ZZA_NKTradeGroup>
    </RefCusApplicability>
    <RefCusApplicability>
      <ZZT_EndDate>2079-06-06T23:59:00</ZZT_EndDate>
      <ZZT_OrderNumber />
      <ZZT_StartDate>2016-01-01T00:00:00</ZZT_StartDate>
      <ZZT_ZZA_NKTradeGroup>PA</ZZT_ZZA_NKTradeGroup>
    </RefCusApplicability>
    <RefCusApplicability>
      <ZZT_EndDate>2079-06-06T23:59:00</ZZT_EndDate>
      <ZZT_OrderNumber />
      <ZZT_StartDate>2016-01-01T00:00:00</ZZT_StartDate>
      <ZZT_ZZA_NKTradeGroup>SG</ZZT_ZZA_NKTradeGroup>
    </RefCusApplicability>
    <RefCusApplicability>
      <ZZT_EndDate>2079-06-06T23:59:00</ZZT_EndDate>
      <ZZT_OrderNumber />
      <ZZT_StartDate>2016-01-01T00:00:00</ZZT_StartDate>
      <ZZT_ZZA_NKTradeGroup>SV</ZZT_ZZA_NKTradeGroup>
    </RefCusApplicability>
  </RefCusRate>");
		}

		[Test]
		public void TestOverrideLogicWithMultipleDTARates()
		{
			var rows = new List<TariffColumn2DataRow>
			{
				new TariffColumn2DataRow("01041000GT9999999920160101          0000000000                                    GT        "),
				new TariffColumn2DataRow("01041000HN9999999920160101          0000000000                                    HN        "),
				new TariffColumn2DataRow("01041000NI9999999920160101          0000120000                                    NI        "),
				new TariffColumn2DataRow("01041000NZ9999999920160101          0000120000                                    NZ        "),
				new TariffColumn2DataRow("01041000PA9999999920160101          0000000000                                    PA        "),
				new TariffColumn2DataRow("01041000SG9999999920160101          0000000000                                    SG        "),
				new TariffColumn2DataRow("01041000SV9999999920160101          0000000000                                    SV        ")
			};

			var col2OverrideList = new List<TariffColumn2OverrideDataRow>
			{
				new TariffColumn2OverrideDataRow("01041000,GT,20190930,20170101,,0000000000,QUOTA"),
				new TariffColumn2OverrideDataRow("01041000,GT,20190930,20170101,,0001500000,")
			};

			var tariffUpdateInfo = new TariffUpdateInfoForTest(new updateInfoBean(), string.Empty);
			rows = tariffUpdateInfo.ApplyOverrides(rows, col2OverrideList);

			var groupedRows = tariffUpdateInfo.GroupTariffColumn2DataRows(rows);
			AssertRefCusRateXml(groupedRows, tariffUpdateInfo, @"<RefCusRate>
    <ZZ2_EndDate>2019-09-30T23:59:59</ZZ2_EndDate>
    <ZZ2_RateFormula>0</ZZ2_RateFormula>
    <ZZ2_RateFormulaDerivedFrom>0</ZZ2_RateFormulaDerivedFrom>
    <ZZ2_StartDate>2017-01-01T00:00:00</ZZ2_StartDate>
    <ZZ2_ZY1_NKRateCode>DTA</ZZ2_ZY1_NKRateCode>
    <ZZ2_ZZS_NKPreference>PT2</ZZ2_ZZS_NKPreference>
    <RefCusApplicability>
      <ZZT_EndDate>2019-09-30T23:59:00</ZZT_EndDate>
      <ZZT_OrderNumber>QUOTA</ZZT_OrderNumber>
      <ZZT_StartDate>2017-01-01T00:00:00</ZZT_StartDate>
      <ZZT_ZZA_NKTradeGroup>GT</ZZT_ZZA_NKTradeGroup>
    </RefCusApplicability>
  </RefCusRate>
  <RefCusRate>
    <ZZ2_EndDate>2019-09-30T23:59:59</ZZ2_EndDate>
    <ZZ2_RateFormula>15*VFD</ZZ2_RateFormula>
    <ZZ2_RateFormulaDerivedFrom>15</ZZ2_RateFormulaDerivedFrom>
    <ZZ2_StartDate>2017-01-01T00:00:00</ZZ2_StartDate>
    <ZZ2_ZY1_NKRateCode>DTA</ZZ2_ZY1_NKRateCode>
    <ZZ2_ZZS_NKPreference>PT2</ZZ2_ZZS_NKPreference>
    <RefCusApplicability>
      <ZZT_EndDate>2019-09-30T23:59:00</ZZT_EndDate>
      <ZZT_OrderNumber />
      <ZZT_StartDate>2017-01-01T00:00:00</ZZT_StartDate>
      <ZZT_ZZA_NKTradeGroup>GT</ZZT_ZZA_NKTradeGroup>
    </RefCusApplicability>
  </RefCusRate>
  <RefCusRate>
    <ZZ2_EndDate>2079-06-06T23:59:00</ZZ2_EndDate>
    <ZZ2_RateFormula>0</ZZ2_RateFormula>
    <ZZ2_RateFormulaDerivedFrom>0</ZZ2_RateFormulaDerivedFrom>
    <ZZ2_StartDate>2016-01-01T00:00:00</ZZ2_StartDate>
    <ZZ2_ZY1_NKRateCode>DTA</ZZ2_ZY1_NKRateCode>
    <ZZ2_ZZS_NKPreference>PR2</ZZ2_ZZS_NKPreference>
    <RefCusApplicability>
      <ZZT_EndDate>2079-06-06T23:59:00</ZZT_EndDate>
      <ZZT_OrderNumber />
      <ZZT_StartDate>2016-01-01T00:00:00</ZZT_StartDate>
      <ZZT_ZZA_NKTradeGroup>HN</ZZT_ZZA_NKTradeGroup>
    </RefCusApplicability>
    <RefCusApplicability>
      <ZZT_EndDate>2079-06-06T23:59:00</ZZT_EndDate>
      <ZZT_OrderNumber />
      <ZZT_StartDate>2016-01-01T00:00:00</ZZT_StartDate>
      <ZZT_ZZA_NKTradeGroup>PA</ZZT_ZZA_NKTradeGroup>
    </RefCusApplicability>
    <RefCusApplicability>
      <ZZT_EndDate>2079-06-06T23:59:00</ZZT_EndDate>
      <ZZT_OrderNumber />
      <ZZT_StartDate>2016-01-01T00:00:00</ZZT_StartDate>
      <ZZT_ZZA_NKTradeGroup>SG</ZZT_ZZA_NKTradeGroup>
    </RefCusApplicability>
    <RefCusApplicability>
      <ZZT_EndDate>2079-06-06T23:59:00</ZZT_EndDate>
      <ZZT_OrderNumber />
      <ZZT_StartDate>2016-01-01T00:00:00</ZZT_StartDate>
      <ZZT_ZZA_NKTradeGroup>SV</ZZT_ZZA_NKTradeGroup>
    </RefCusApplicability>
  </RefCusRate>
  <RefCusRate>
    <ZZ2_EndDate>2079-06-06T23:59:00</ZZ2_EndDate>
    <ZZ2_RateFormula>1.2*VFD</ZZ2_RateFormula>
    <ZZ2_RateFormulaDerivedFrom>1.2</ZZ2_RateFormulaDerivedFrom>
    <ZZ2_StartDate>2016-01-01T00:00:00</ZZ2_StartDate>
    <ZZ2_ZY1_NKRateCode>DTA</ZZ2_ZY1_NKRateCode>
    <ZZ2_ZZS_NKPreference>PR2</ZZ2_ZZS_NKPreference>
    <RefCusApplicability>
      <ZZT_EndDate>2079-06-06T23:59:00</ZZT_EndDate>
      <ZZT_OrderNumber />
      <ZZT_StartDate>2016-01-01T00:00:00</ZZT_StartDate>
      <ZZT_ZZA_NKTradeGroup>NI</ZZT_ZZA_NKTradeGroup>
    </RefCusApplicability>
    <RefCusApplicability>
      <ZZT_EndDate>2079-06-06T23:59:00</ZZT_EndDate>
      <ZZT_OrderNumber />
      <ZZT_StartDate>2016-01-01T00:00:00</ZZT_StartDate>
      <ZZT_ZZA_NKTradeGroup>NZ</ZZT_ZZA_NKTradeGroup>
    </RefCusApplicability>
  </RefCusRate>");
		}

		[Test]
		public void TestOverrideLogicWithSingleDTSRates()
		{
			var rows = new List<TariffColumn2DataRow>
			{
				new TariffColumn2DataRow("01041000GT99999999201601010000000000                                              GT        "),
				new TariffColumn2DataRow("01041000HN99999999201601010000000000                                              HN        "),
				new TariffColumn2DataRow("01041000NI99999999201601010000000000                                              NI        "),
				new TariffColumn2DataRow("01041000NZ99999999201601010000000000                                              NZ        "),
				new TariffColumn2DataRow("01041000PA99999999201601010000000000                                              PA        "),
				new TariffColumn2DataRow("01041000SG99999999201601010000000000                                              SG        "),
				new TariffColumn2DataRow("01041000SV99999999201601010000000000                                              SV        ")
			};

			var col2OverrideList = new List<TariffColumn2OverrideDataRow>
			{
				new TariffColumn2OverrideDataRow("01041000,GT,20190930,20170101,,0000000000,QUOTA"),
				new TariffColumn2OverrideDataRow("01041000,GT,20190930,20170101,,0001500000,")
			};

			var tariffUpdateInfo = new TariffUpdateInfoForTest(new updateInfoBean(), string.Empty);
			rows = tariffUpdateInfo.ApplyOverrides(rows, col2OverrideList);

			var groupedRows = tariffUpdateInfo.GroupTariffColumn2DataRows(rows);
			AssertRefCusRateXml(groupedRows, tariffUpdateInfo, @"<RefCusRate>
    <ZZ2_EndDate>2019-09-30T23:59:59</ZZ2_EndDate>
    <ZZ2_RateFormula>0</ZZ2_RateFormula>
    <ZZ2_RateFormulaDerivedFrom>0</ZZ2_RateFormulaDerivedFrom>
    <ZZ2_StartDate>2017-01-01T00:00:00</ZZ2_StartDate>
    <ZZ2_ZY1_NKRateCode>DTA</ZZ2_ZY1_NKRateCode>
    <ZZ2_ZZS_NKPreference>PT2</ZZ2_ZZS_NKPreference>
    <RefCusApplicability>
      <ZZT_EndDate>2019-09-30T23:59:00</ZZT_EndDate>
      <ZZT_OrderNumber>QUOTA</ZZT_OrderNumber>
      <ZZT_StartDate>2017-01-01T00:00:00</ZZT_StartDate>
      <ZZT_ZZA_NKTradeGroup>GT</ZZT_ZZA_NKTradeGroup>
    </RefCusApplicability>
  </RefCusRate>
  <RefCusRate>
    <ZZ2_EndDate>2019-09-30T23:59:59</ZZ2_EndDate>
    <ZZ2_RateFormula>15*VFD</ZZ2_RateFormula>
    <ZZ2_RateFormulaDerivedFrom>15</ZZ2_RateFormulaDerivedFrom>
    <ZZ2_StartDate>2017-01-01T00:00:00</ZZ2_StartDate>
    <ZZ2_ZY1_NKRateCode>DTA</ZZ2_ZY1_NKRateCode>
    <ZZ2_ZZS_NKPreference>PT2</ZZ2_ZZS_NKPreference>
    <RefCusApplicability>
      <ZZT_EndDate>2019-09-30T23:59:00</ZZT_EndDate>
      <ZZT_OrderNumber />
      <ZZT_StartDate>2017-01-01T00:00:00</ZZT_StartDate>
      <ZZT_ZZA_NKTradeGroup>GT</ZZT_ZZA_NKTradeGroup>
    </RefCusApplicability>
  </RefCusRate>
  <RefCusRate>
    <ZZ2_EndDate>2079-06-06T23:59:00</ZZ2_EndDate>
    <ZZ2_RateFormula>0*[]</ZZ2_RateFormula>
    <ZZ2_RateFormulaDerivedFrom>0/</ZZ2_RateFormulaDerivedFrom>
    <ZZ2_StartDate>2016-01-01T00:00:00</ZZ2_StartDate>
    <ZZ2_ZY1_NKRateCode>DTS</ZZ2_ZY1_NKRateCode>
    <ZZ2_ZZS_NKPreference>PR2</ZZ2_ZZS_NKPreference>
    <RefCusApplicability>
      <ZZT_EndDate>2079-06-06T23:59:00</ZZT_EndDate>
      <ZZT_OrderNumber />
      <ZZT_StartDate>2016-01-01T00:00:00</ZZT_StartDate>
      <ZZT_ZZA_NKTradeGroup>HN</ZZT_ZZA_NKTradeGroup>
    </RefCusApplicability>
    <RefCusApplicability>
      <ZZT_EndDate>2079-06-06T23:59:00</ZZT_EndDate>
      <ZZT_OrderNumber />
      <ZZT_StartDate>2016-01-01T00:00:00</ZZT_StartDate>
      <ZZT_ZZA_NKTradeGroup>NI</ZZT_ZZA_NKTradeGroup>
    </RefCusApplicability>
    <RefCusApplicability>
      <ZZT_EndDate>2079-06-06T23:59:00</ZZT_EndDate>
      <ZZT_OrderNumber />
      <ZZT_StartDate>2016-01-01T00:00:00</ZZT_StartDate>
      <ZZT_ZZA_NKTradeGroup>NZ</ZZT_ZZA_NKTradeGroup>
    </RefCusApplicability>
    <RefCusApplicability>
      <ZZT_EndDate>2079-06-06T23:59:00</ZZT_EndDate>
      <ZZT_OrderNumber />
      <ZZT_StartDate>2016-01-01T00:00:00</ZZT_StartDate>
      <ZZT_ZZA_NKTradeGroup>PA</ZZT_ZZA_NKTradeGroup>
    </RefCusApplicability>
    <RefCusApplicability>
      <ZZT_EndDate>2079-06-06T23:59:00</ZZT_EndDate>
      <ZZT_OrderNumber />
      <ZZT_StartDate>2016-01-01T00:00:00</ZZT_StartDate>
      <ZZT_ZZA_NKTradeGroup>SG</ZZT_ZZA_NKTradeGroup>
    </RefCusApplicability>
    <RefCusApplicability>
      <ZZT_EndDate>2079-06-06T23:59:00</ZZT_EndDate>
      <ZZT_OrderNumber />
      <ZZT_StartDate>2016-01-01T00:00:00</ZZT_StartDate>
      <ZZT_ZZA_NKTradeGroup>SV</ZZT_ZZA_NKTradeGroup>
    </RefCusApplicability>
  </RefCusRate>");
		}

		[Test]
		public void TestOverrideLogicWithMultipleDTSRates()
		{
			var rows = new List<TariffColumn2DataRow>
			{
				new TariffColumn2DataRow("01041000GT99999999201601010000000000                                              GT        "),
				new TariffColumn2DataRow("01041000HN99999999201601010000200000                                              HN        "),
				new TariffColumn2DataRow("01041000NI99999999201601010000000000                                              NI        "),
				new TariffColumn2DataRow("01041000NZ99999999201601010000000000                                              NZ        "),
				new TariffColumn2DataRow("01041000PA99999999201601010000200000                                              PA        "),
				new TariffColumn2DataRow("01041000SG99999999201601010000000000                                              SG        "),
				new TariffColumn2DataRow("01041000SV99999999201601010000000000                                              SV        ")
			};

			var col2OverrideList = new List<TariffColumn2OverrideDataRow>
			{
				new TariffColumn2OverrideDataRow("01041000,GT,20190930,20170101,,0000000000,QUOTA"),
				new TariffColumn2OverrideDataRow("01041000,GT,20190930,20170101,,0001500000,")
			};

			var tariffUpdateInfo = new TariffUpdateInfoForTest(new updateInfoBean(), string.Empty);
			rows = tariffUpdateInfo.ApplyOverrides(rows, col2OverrideList);

			var groupedRows = tariffUpdateInfo.GroupTariffColumn2DataRows(rows);
			AssertRefCusRateXml(groupedRows, tariffUpdateInfo, @"<RefCusRate>
    <ZZ2_EndDate>2019-09-30T23:59:59</ZZ2_EndDate>
    <ZZ2_RateFormula>0</ZZ2_RateFormula>
    <ZZ2_RateFormulaDerivedFrom>0</ZZ2_RateFormulaDerivedFrom>
    <ZZ2_StartDate>2017-01-01T00:00:00</ZZ2_StartDate>
    <ZZ2_ZY1_NKRateCode>DTA</ZZ2_ZY1_NKRateCode>
    <ZZ2_ZZS_NKPreference>PT2</ZZ2_ZZS_NKPreference>
    <RefCusApplicability>
      <ZZT_EndDate>2019-09-30T23:59:00</ZZT_EndDate>
      <ZZT_OrderNumber>QUOTA</ZZT_OrderNumber>
      <ZZT_StartDate>2017-01-01T00:00:00</ZZT_StartDate>
      <ZZT_ZZA_NKTradeGroup>GT</ZZT_ZZA_NKTradeGroup>
    </RefCusApplicability>
  </RefCusRate>
  <RefCusRate>
    <ZZ2_EndDate>2019-09-30T23:59:59</ZZ2_EndDate>
    <ZZ2_RateFormula>15*VFD</ZZ2_RateFormula>
    <ZZ2_RateFormulaDerivedFrom>15</ZZ2_RateFormulaDerivedFrom>
    <ZZ2_StartDate>2017-01-01T00:00:00</ZZ2_StartDate>
    <ZZ2_ZY1_NKRateCode>DTA</ZZ2_ZY1_NKRateCode>
    <ZZ2_ZZS_NKPreference>PT2</ZZ2_ZZS_NKPreference>
    <RefCusApplicability>
      <ZZT_EndDate>2019-09-30T23:59:00</ZZT_EndDate>
      <ZZT_OrderNumber />
      <ZZT_StartDate>2017-01-01T00:00:00</ZZT_StartDate>
      <ZZT_ZZA_NKTradeGroup>GT</ZZT_ZZA_NKTradeGroup>
    </RefCusApplicability>
  </RefCusRate>
  <RefCusRate>
    <ZZ2_EndDate>2079-06-06T23:59:00</ZZ2_EndDate>
    <ZZ2_RateFormula>2*[]</ZZ2_RateFormula>
    <ZZ2_RateFormulaDerivedFrom>2/</ZZ2_RateFormulaDerivedFrom>
    <ZZ2_StartDate>2016-01-01T00:00:00</ZZ2_StartDate>
    <ZZ2_ZY1_NKRateCode>DTS</ZZ2_ZY1_NKRateCode>
    <ZZ2_ZZS_NKPreference>PR2</ZZ2_ZZS_NKPreference>
    <RefCusApplicability>
      <ZZT_EndDate>2079-06-06T23:59:00</ZZT_EndDate>
      <ZZT_OrderNumber />
      <ZZT_StartDate>2016-01-01T00:00:00</ZZT_StartDate>
      <ZZT_ZZA_NKTradeGroup>HN</ZZT_ZZA_NKTradeGroup>
    </RefCusApplicability>
    <RefCusApplicability>
      <ZZT_EndDate>2079-06-06T23:59:00</ZZT_EndDate>
      <ZZT_OrderNumber />
      <ZZT_StartDate>2016-01-01T00:00:00</ZZT_StartDate>
      <ZZT_ZZA_NKTradeGroup>PA</ZZT_ZZA_NKTradeGroup>
    </RefCusApplicability>
  </RefCusRate>
  <RefCusRate>
    <ZZ2_EndDate>2079-06-06T23:59:00</ZZ2_EndDate>
    <ZZ2_RateFormula>0*[]</ZZ2_RateFormula>
    <ZZ2_RateFormulaDerivedFrom>0/</ZZ2_RateFormulaDerivedFrom>
    <ZZ2_StartDate>2016-01-01T00:00:00</ZZ2_StartDate>
    <ZZ2_ZY1_NKRateCode>DTS</ZZ2_ZY1_NKRateCode>
    <ZZ2_ZZS_NKPreference>PR2</ZZ2_ZZS_NKPreference>
    <RefCusApplicability>
      <ZZT_EndDate>2079-06-06T23:59:00</ZZT_EndDate>
      <ZZT_OrderNumber />
      <ZZT_StartDate>2016-01-01T00:00:00</ZZT_StartDate>
      <ZZT_ZZA_NKTradeGroup>NI</ZZT_ZZA_NKTradeGroup>
    </RefCusApplicability>
    <RefCusApplicability>
      <ZZT_EndDate>2079-06-06T23:59:00</ZZT_EndDate>
      <ZZT_OrderNumber />
      <ZZT_StartDate>2016-01-01T00:00:00</ZZT_StartDate>
      <ZZT_ZZA_NKTradeGroup>NZ</ZZT_ZZA_NKTradeGroup>
    </RefCusApplicability>
    <RefCusApplicability>
      <ZZT_EndDate>2079-06-06T23:59:00</ZZT_EndDate>
      <ZZT_OrderNumber />
      <ZZT_StartDate>2016-01-01T00:00:00</ZZT_StartDate>
      <ZZT_ZZA_NKTradeGroup>SG</ZZT_ZZA_NKTradeGroup>
    </RefCusApplicability>
    <RefCusApplicability>
      <ZZT_EndDate>2079-06-06T23:59:00</ZZT_EndDate>
      <ZZT_OrderNumber />
      <ZZT_StartDate>2016-01-01T00:00:00</ZZT_StartDate>
      <ZZT_ZZA_NKTradeGroup>SV</ZZT_ZZA_NKTradeGroup>
    </RefCusApplicability>
  </RefCusRate>");
		}

		[Test]
		public void TestExclusion()
		{
			var rows = new List<TariffColumn2DataRow>
			{
				new TariffColumn2DataRow("01041000GT9999999920170101          0000000000                                    GT        "),
				new TariffColumn2DataRow("01041000HN9999999920170101          0000000000                                    HN        "),
				new TariffColumn2DataRow("01041000NI9999999920170101          0000000000                                    NI        "),
				new TariffColumn2DataRow("01041000NZ9999999920170101          0000000000                                    NZ        "),
				new TariffColumn2DataRow("01041000PA9999999920170101          0000000000                                    PA        "),
				new TariffColumn2DataRow("01041000SG9999999920170101          0000000000                                    SG        "),
				new TariffColumn2DataRow("01042000SV9999999920170101          0000000000                                    SV        ")
			};

			var tariffUpdateInfo = new TariffUpdateInfoForTest(new updateInfoBean(), string.Empty);
			var groupedRows = tariffUpdateInfo.GroupTariffColumn2DataRows(rows);
			AssertRefCusRateXml(groupedRows, tariffUpdateInfo, @"<RefCusRate>
    <ZZ2_EndDate>2079-06-06T23:59:00</ZZ2_EndDate>
    <ZZ2_RateFormula>0</ZZ2_RateFormula>
    <ZZ2_RateFormulaDerivedFrom>0</ZZ2_RateFormulaDerivedFrom>
    <ZZ2_StartDate>2017-01-01T00:00:00</ZZ2_StartDate>
    <ZZ2_ZY1_NKRateCode>DTA</ZZ2_ZY1_NKRateCode>
    <ZZ2_ZZS_NKPreference>PR2</ZZ2_ZZS_NKPreference>
    <RefCusApplicability>
      <ZZT_EndDate>2079-06-06T23:59:00</ZZT_EndDate>
      <ZZT_OrderNumber />
      <ZZT_StartDate>2017-01-01T00:00:00</ZZT_StartDate>
      <ZZT_ZZA_NKTradeGroup>SPE</ZZT_ZZA_NKTradeGroup>
      <RefCusExcludedTradeGroup>
        <ZZC_ZZA_NKTradeGroup>SV</ZZC_ZZA_NKTradeGroup>
      </RefCusExcludedTradeGroup>
    </RefCusApplicability>
  </RefCusRate>
  <RefCusRate>
    <ZZ2_EndDate>2079-06-06T23:59:00</ZZ2_EndDate>
    <ZZ2_RateFormula>0</ZZ2_RateFormula>
    <ZZ2_RateFormulaDerivedFrom>0</ZZ2_RateFormulaDerivedFrom>
    <ZZ2_StartDate>2017-01-01T00:00:00</ZZ2_StartDate>
    <ZZ2_ZY1_NKRateCode>DTA</ZZ2_ZY1_NKRateCode>
    <ZZ2_ZZS_NKPreference>PR2</ZZ2_ZZS_NKPreference>
    <RefCusApplicability>
      <ZZT_EndDate>2079-06-06T23:59:00</ZZT_EndDate>
      <ZZT_OrderNumber />
      <ZZT_StartDate>2017-01-01T00:00:00</ZZT_StartDate>
      <ZZT_ZZA_NKTradeGroup>SPE</ZZT_ZZA_NKTradeGroup>
      <RefCusExcludedTradeGroup>
        <ZZC_ZZA_NKTradeGroup>GT</ZZC_ZZA_NKTradeGroup>
      </RefCusExcludedTradeGroup>
      <RefCusExcludedTradeGroup>
        <ZZC_ZZA_NKTradeGroup>HN</ZZC_ZZA_NKTradeGroup>
      </RefCusExcludedTradeGroup>
      <RefCusExcludedTradeGroup>
        <ZZC_ZZA_NKTradeGroup>NI</ZZC_ZZA_NKTradeGroup>
      </RefCusExcludedTradeGroup>
      <RefCusExcludedTradeGroup>
        <ZZC_ZZA_NKTradeGroup>NZ</ZZC_ZZA_NKTradeGroup>
      </RefCusExcludedTradeGroup>
      <RefCusExcludedTradeGroup>
        <ZZC_ZZA_NKTradeGroup>PA</ZZC_ZZA_NKTradeGroup>
      </RefCusExcludedTradeGroup>
      <RefCusExcludedTradeGroup>
        <ZZC_ZZA_NKTradeGroup>SG</ZZC_ZZA_NKTradeGroup>
      </RefCusExcludedTradeGroup>
    </RefCusApplicability>
  </RefCusRate>");
		}

		[Test]
		public void TestNoExclusion()
		{
			var rows = new List<TariffColumn2DataRow>
			{
				new TariffColumn2DataRow("01041000GT9999999920170101          0000000000                                    GT        "),
				new TariffColumn2DataRow("01041000HN9999999920170101          0000000000                                    HN        "),
				new TariffColumn2DataRow("01041000NI9999999920170101          0000000000                                    NI        "),
				new TariffColumn2DataRow("01041000NZ9999999920170101          0000000000                                    NZ        "),
				new TariffColumn2DataRow("01041000PA9999999920170101          0000000000                                    PA        "),
				new TariffColumn2DataRow("01041000SG9999999920170101          0000000000                                    SG        ")
			};

			var tariffUpdateInfo = new TariffUpdateInfoForTest(new updateInfoBean(), string.Empty);
			var groupedRows = tariffUpdateInfo.GroupTariffColumn2DataRows(rows);
			AssertRefCusRateXml(groupedRows, tariffUpdateInfo, @"<RefCusRate>
    <ZZ2_EndDate>2079-06-06T23:59:00</ZZ2_EndDate>
    <ZZ2_RateFormula>0</ZZ2_RateFormula>
    <ZZ2_RateFormulaDerivedFrom>0</ZZ2_RateFormulaDerivedFrom>
    <ZZ2_StartDate>2017-01-01T00:00:00</ZZ2_StartDate>
    <ZZ2_ZY1_NKRateCode>DTA</ZZ2_ZY1_NKRateCode>
    <ZZ2_ZZS_NKPreference>PR2</ZZ2_ZZS_NKPreference>
    <RefCusApplicability>
      <ZZT_EndDate>2079-06-06T23:59:00</ZZT_EndDate>
      <ZZT_OrderNumber />
      <ZZT_StartDate>2017-01-01T00:00:00</ZZT_StartDate>
      <ZZT_ZZA_NKTradeGroup>SPE</ZZT_ZZA_NKTradeGroup>
    </RefCusApplicability>
  </RefCusRate>");
		}

		[Test]
		public void TestOverrideLogicNotUsedWithExclusion()
		{
			var rows = new List<TariffColumn2DataRow>
			{
				new TariffColumn2DataRow("01041000GT9999999920160101          0000000000                                    GT        "),
				new TariffColumn2DataRow("01041000HN9999999920160101          0000000000                                    HN        "),
				new TariffColumn2DataRow("01041000NI9999999920160101          0000000000                                    NI        "),
				new TariffColumn2DataRow("01041000NZ9999999920160101          0000000000                                    NZ        "),
				new TariffColumn2DataRow("01041000PA9999999920160101          0000000000                                    PA        "),
				new TariffColumn2DataRow("01041000SG9999999920160101          0000000000                                    SG        "),
				new TariffColumn2DataRow("01042000SV9999999920160101          0000000000                                    SV        ")
			};

			var col2OverrideList = new List<TariffColumn2OverrideDataRow>
			{
				new TariffColumn2OverrideDataRow("01041000,GT,20190930,20170101,,0000000000,QUOTA"),
				new TariffColumn2OverrideDataRow("01041000,GT,20190930,20170101,0001500000,,"),
				new TariffColumn2OverrideDataRow("01042000,SV,20190930,20170101,,0000000000,QUOTA"),
				new TariffColumn2OverrideDataRow("01042000,SV,20190930,20170101,0001500000,,")
			};

			var tariffUpdateInfo = new TariffUpdateInfoForTest(new updateInfoBean(), string.Empty);
			rows = tariffUpdateInfo.ApplyOverrides(rows, col2OverrideList);

			var groupedRows = tariffUpdateInfo.GroupTariffColumn2DataRows(rows);
			AssertRefCusRateXml(groupedRows, tariffUpdateInfo, @"<RefCusRate>
    <ZZ2_EndDate>2019-09-30T23:59:59</ZZ2_EndDate>
    <ZZ2_RateFormula>0</ZZ2_RateFormula>
    <ZZ2_RateFormulaDerivedFrom>0</ZZ2_RateFormulaDerivedFrom>
    <ZZ2_StartDate>2017-01-01T00:00:00</ZZ2_StartDate>
    <ZZ2_ZY1_NKRateCode>DTA</ZZ2_ZY1_NKRateCode>
    <ZZ2_ZZS_NKPreference>PT2</ZZ2_ZZS_NKPreference>
    <RefCusApplicability>
      <ZZT_EndDate>2019-09-30T23:59:00</ZZT_EndDate>
      <ZZT_OrderNumber>QUOTA</ZZT_OrderNumber>
      <ZZT_StartDate>2017-01-01T00:00:00</ZZT_StartDate>
      <ZZT_ZZA_NKTradeGroup>GT</ZZT_ZZA_NKTradeGroup>
    </RefCusApplicability>
  </RefCusRate>
  <RefCusRate>
    <ZZ2_EndDate>2019-09-30T23:59:59</ZZ2_EndDate>
    <ZZ2_RateFormula>15*[]</ZZ2_RateFormula>
    <ZZ2_RateFormulaDerivedFrom>15/</ZZ2_RateFormulaDerivedFrom>
    <ZZ2_StartDate>2017-01-01T00:00:00</ZZ2_StartDate>
    <ZZ2_ZY1_NKRateCode>DTS</ZZ2_ZY1_NKRateCode>
    <ZZ2_ZZS_NKPreference>PT2</ZZ2_ZZS_NKPreference>
    <RefCusApplicability>
      <ZZT_EndDate>2019-09-30T23:59:00</ZZT_EndDate>
      <ZZT_OrderNumber />
      <ZZT_StartDate>2017-01-01T00:00:00</ZZT_StartDate>
      <ZZT_ZZA_NKTradeGroup>GT</ZZT_ZZA_NKTradeGroup>
    </RefCusApplicability>
  </RefCusRate>
  <RefCusRate>
    <ZZ2_EndDate>2079-06-06T23:59:00</ZZ2_EndDate>
    <ZZ2_RateFormula>0</ZZ2_RateFormula>
    <ZZ2_RateFormulaDerivedFrom>0</ZZ2_RateFormulaDerivedFrom>
    <ZZ2_StartDate>2016-01-01T00:00:00</ZZ2_StartDate>
    <ZZ2_ZY1_NKRateCode>DTA</ZZ2_ZY1_NKRateCode>
    <ZZ2_ZZS_NKPreference>PR2</ZZ2_ZZS_NKPreference>
    <RefCusApplicability>
      <ZZT_EndDate>2079-06-06T23:59:00</ZZT_EndDate>
      <ZZT_OrderNumber />
      <ZZT_StartDate>2016-01-01T00:00:00</ZZT_StartDate>
      <ZZT_ZZA_NKTradeGroup>HN</ZZT_ZZA_NKTradeGroup>
    </RefCusApplicability>
    <RefCusApplicability>
      <ZZT_EndDate>2079-06-06T23:59:00</ZZT_EndDate>
      <ZZT_OrderNumber />
      <ZZT_StartDate>2016-01-01T00:00:00</ZZT_StartDate>
      <ZZT_ZZA_NKTradeGroup>NI</ZZT_ZZA_NKTradeGroup>
    </RefCusApplicability>
    <RefCusApplicability>
      <ZZT_EndDate>2079-06-06T23:59:00</ZZT_EndDate>
      <ZZT_OrderNumber />
      <ZZT_StartDate>2016-01-01T00:00:00</ZZT_StartDate>
      <ZZT_ZZA_NKTradeGroup>NZ</ZZT_ZZA_NKTradeGroup>
    </RefCusApplicability>
    <RefCusApplicability>
      <ZZT_EndDate>2079-06-06T23:59:00</ZZT_EndDate>
      <ZZT_OrderNumber />
      <ZZT_StartDate>2016-01-01T00:00:00</ZZT_StartDate>
      <ZZT_ZZA_NKTradeGroup>PA</ZZT_ZZA_NKTradeGroup>
    </RefCusApplicability>
    <RefCusApplicability>
      <ZZT_EndDate>2079-06-06T23:59:00</ZZT_EndDate>
      <ZZT_OrderNumber />
      <ZZT_StartDate>2016-01-01T00:00:00</ZZT_StartDate>
      <ZZT_ZZA_NKTradeGroup>SG</ZZT_ZZA_NKTradeGroup>
    </RefCusApplicability>
  </RefCusRate>
  <RefCusRate>
    <ZZ2_EndDate>2019-09-30T23:59:59</ZZ2_EndDate>
    <ZZ2_RateFormula>0</ZZ2_RateFormula>
    <ZZ2_RateFormulaDerivedFrom>0</ZZ2_RateFormulaDerivedFrom>
    <ZZ2_StartDate>2017-01-01T00:00:00</ZZ2_StartDate>
    <ZZ2_ZY1_NKRateCode>DTA</ZZ2_ZY1_NKRateCode>
    <ZZ2_ZZS_NKPreference>PT2</ZZ2_ZZS_NKPreference>
    <RefCusApplicability>
      <ZZT_EndDate>2019-09-30T23:59:00</ZZT_EndDate>
      <ZZT_OrderNumber>QUOTA</ZZT_OrderNumber>
      <ZZT_StartDate>2017-01-01T00:00:00</ZZT_StartDate>
      <ZZT_ZZA_NKTradeGroup>SV</ZZT_ZZA_NKTradeGroup>
    </RefCusApplicability>
  </RefCusRate>
  <RefCusRate>
    <ZZ2_EndDate>2019-09-30T23:59:59</ZZ2_EndDate>
    <ZZ2_RateFormula>15*[]</ZZ2_RateFormula>
    <ZZ2_RateFormulaDerivedFrom>15/</ZZ2_RateFormulaDerivedFrom>
    <ZZ2_StartDate>2017-01-01T00:00:00</ZZ2_StartDate>
    <ZZ2_ZY1_NKRateCode>DTS</ZZ2_ZY1_NKRateCode>
    <ZZ2_ZZS_NKPreference>PT2</ZZ2_ZZS_NKPreference>
    <RefCusApplicability>
      <ZZT_EndDate>2019-09-30T23:59:00</ZZT_EndDate>
      <ZZT_OrderNumber />
      <ZZT_StartDate>2017-01-01T00:00:00</ZZT_StartDate>
      <ZZT_ZZA_NKTradeGroup>SV</ZZT_ZZA_NKTradeGroup>
    </RefCusApplicability>
  </RefCusRate>");
		}

		[Test]
		public void TestCreateAttributeForEnvironmentalProtectionTariff()
		{
			var tariffUpdateInfo = new TariffUpdateInfoForTest(new updateInfoBean(), string.Empty);
			var referenceList = new List<string>() { "04011010001" };
			var tariffAttributes = tariffUpdateInfo.CreateAttributeForEnvironmentalProtectionTariff("04011010001", referenceList);
			Assert.AreEqual(1, tariffAttributes.Count());
			Assert.AreEqual("EnvironmentalProtectionTariff", tariffAttributes.ElementAt(0).ZZ3_Name);
			Assert.AreEqual("TRUE", tariffAttributes.ElementAt(0).ZZ3_Value);

			tariffAttributes = tariffUpdateInfo.CreateAttributeForEnvironmentalProtectionTariff("04012010001", referenceList);
			Assert.AreEqual(0, tariffAttributes.Count());
		}

		void AssertRefCusRateXml(IEnumerable<Common.UniversalXmlWriter.EntityType.RefCusRate> rates, string expectedXml)
		{
			var actualXml = GetRefCusRateTestXml(rates.ToList());
			Assert.AreEqual(true, actualXml.Contains(expectedXml));
		}


		void AssertRefCusRateXml(List<TariffColumn2DataRowGroup> groupedRows, TariffUpdateInfoForTest tariffUpdateInfo, string expectedXml)
		{
			var refCusRateList = new List<Common.UniversalXmlWriter.EntityType.RefCusRate>();
			foreach (var item in groupedRows)
			{
				var result = tariffUpdateInfo.CreateRefCusRateElement(string.Empty, item);
				refCusRateList.AddRange(result);
			}
			var actualXml = GetRefCusRateTestXml(refCusRateList);
			Assert.AreEqual(true, actualXml.Contains(expectedXml));
		}


		string GetRefCusRateTestXml(List<Common.UniversalXmlWriter.EntityType.RefCusRate> refCusRateList)
		{
			var writerConfiguration = new XmlWriterConfiguration();
			#region RefCusRate
			var cusRateConfiguration = new EntityTypeConfiguration<Common.UniversalXmlWriter.EntityType.RefCusRate>(false);
			cusRateConfiguration.IncludeColumn(x => x.ZZ2_StartDate, false);
			cusRateConfiguration.IncludeColumn(x => x.ZZ2_EndDate, false);
			cusRateConfiguration.IncludeColumn(x => x.ZZ2_ZY1_NKRateCode, true);
			cusRateConfiguration.IncludeColumnWithConstantValue(x => x.ZZ2_ZY1_ZZR_NKRateType, true, "DTY");
			cusRateConfiguration.IncludeColumnWithConstantValue(x => x.ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping, true, Constants.DataGroupings.Taiwan);
			cusRateConfiguration.IncludeColumn(x => x.ZZ2_RateFormula, false);
			cusRateConfiguration.IncludeColumn(x => x.ZZ2_RateFormulaDerivedFrom, false);
			cusRateConfiguration.IncludeColumn(x => x.ZZ2_ZZS_NKPreference, true);
			cusRateConfiguration.IncludeColumnWithConstantValue(x => x.ZZ2_ZZS_ZZZ_NKDataGrouping, true, Constants.DataGroupings.Taiwan);
			cusRateConfiguration.IncludeColumnWithConstantValue(x => x.ZZ2_ZZZ_NKDataGrouping, true, Constants.DataGroupings.Taiwan);
			cusRateConfiguration.IncludeColumn(x => x.RefCusApplicabilities, true);
			writerConfiguration.IncludeEntityTypeConfiguration(cusRateConfiguration);
			#endregion
			#region RefCusApplicability
			var cusApplicabilityConfiguration = new EntityTypeConfiguration<RefCusApplicability>(false);
			cusApplicabilityConfiguration.IncludeColumn(x => x.ZZT_StartDate, false);
			cusApplicabilityConfiguration.IncludeColumn(x => x.ZZT_EndDate, false);
			cusApplicabilityConfiguration.IncludeColumn(x => x.ZZT_ZZA_NKTradeGroup, true);
			cusApplicabilityConfiguration.IncludeColumn(x => x.ZZT_OrderNumber, true);
			cusApplicabilityConfiguration.IncludeColumnWithConstantValue(x => x.ZZT_ZZA_ZZZ_NKDataGrouping, true, Constants.DataGroupings.Taiwan);
			cusApplicabilityConfiguration.IncludeColumn(x => x.RefCusExcludedTradeGroups, false);
			writerConfiguration.IncludeEntityTypeConfiguration(cusApplicabilityConfiguration);
			#endregion
			#region RefCusExcludedTradeGroup
			var cusExcludedTradeGroupConfiguration = new EntityTypeConfiguration<RefCusExcludedTradeGroup>(false);
			cusExcludedTradeGroupConfiguration.IncludeColumn(x => x.ZZC_ZZA_NKTradeGroup, true);
			cusExcludedTradeGroupConfiguration.IncludeColumnWithConstantValue(x => x.ZZC_ZZA_ZZZ_NKDataGrouping, true, Constants.DataGroupings.Taiwan);
			writerConfiguration.IncludeEntityTypeConfiguration(cusExcludedTradeGroupConfiguration);
			#endregion

			string fileName = Path.Combine(Utility.TempDirectory, string.Format("{0}.xml", Guid.NewGuid().ToString()));
			WriteXmlHelper.ExportToXMLFile(fileName, "CusRate Test", SystemContext.Now(), refCusRateList, writerConfiguration);
			return XDocument.Load(fileName).ToString();
		}
	}
}
