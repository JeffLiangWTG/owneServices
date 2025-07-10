using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.FRReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.FRReferenceData.Tests.Generators
{
	[TestFixture]
	class TARICTariffUniversalReferenceDataFileGeneratorTest
	{
		[Test]
		public void TestErrorWhenRateTypeIsUnknown()
		{
			var expectedOutputFile = GenerateFileForGivenTariff("TariffWithUnknownTaxCode", UpdateType.Full);
			Assert.That(File.Exists(expectedOutputFile), "Tariff should continue to get parsed when encountering an unknown tax in Full update type.");

			try
			{
				var expectedOutputFile2 = GenerateFileForGivenTariff("TariffWithUnknownTaxCode", UpdateType.Partial);
			}
			catch (Exception e)
			{
				Assert.That(e.Message == "No rate type found for tax code Z000 when parsing measure -249214 of tariff 0100000000.");
			}
		}

		[Test]
		public void CheckOutputFilesForPartialUpdate()
		{
			string[] tariffCodeList = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\", "TariffList.txt")).Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
			ApplicationConfig.Instance.DownloadDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\");
			ApplicationConfig.Instance.OutputDirectory = Path.GetTempPath();
			var expectedOutputFile = Path.Combine(ApplicationConfig.Instance.OutputDirectory, "FRTariffData_Update_20220413.xml");
			File.Delete(expectedOutputFile);

			var generator = new TARICTariffUniversalReferenceDataFileGenerator();
			var outputFileCount = generator.GenerateURDFiles(tariffCodeList, Common.UniversalXmlWriter.UpdateType.Partial, new DateTime(2022, 04, 13));

			Assert.AreEqual(1, outputFileCount);
			Assert.That(File.Exists(expectedOutputFile));
			var outputFileContent = File.ReadAllText(expectedOutputFile);
			Assert.That(outputFileContent.Contains("<DataSource>FR Tariff</DataSource>"));
			var tariffCountInFirstOutputFile = (outputFileContent.Length - outputFileContent.Replace("<RefCusTariff>", "").Length) / 14;
			Assert.AreEqual(11, tariffCountInFirstOutputFile);
			Assert.That(outputFileContent.Contains("<ZZ1_TariffCode>0101210000</ZZ1_TariffCode>"));
			Assert.That(outputFileContent.Contains("<ZZ1_TariffCode>2203000100</ZZ1_TariffCode>"));
			Assert.That(outputFileContent.Contains("<ZZ1_TariffCode>2208409900</ZZ1_TariffCode>"));
		}

		[Test]
		public void CheckOutputFilesForFullUpdate()
		{
			string[] tariffCodeList = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\", "TariffList.txt")).Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
			ApplicationConfig.Instance.DownloadDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\");
			ApplicationConfig.Instance.OutputDirectory = Path.GetTempPath();

			var expectedFirstOutputFile = Path.Combine(ApplicationConfig.Instance.OutputDirectory, "FRTariffData_Chapter01.xml");
			var expected2ndOutputFile = Path.Combine(ApplicationConfig.Instance.OutputDirectory, "FRTariffData_Chapter22.xml");
			var expected3rdOutputFile = Path.Combine(ApplicationConfig.Instance.OutputDirectory, "FRTariffData_Chapter99.xml");
			File.Delete(expectedFirstOutputFile);
			File.Delete(expected2ndOutputFile);
			File.Delete(expected3rdOutputFile);

			var generator = new TARICTariffUniversalReferenceDataFileGenerator();
			var outputFileCount = generator.GenerateURDFiles(tariffCodeList, Common.UniversalXmlWriter.UpdateType.Full, DateTime.Now);

			Assert.AreEqual(2, outputFileCount);

			Assert.That(File.Exists(expectedFirstOutputFile));
			var firstOutputFileContent = File.ReadAllText(expectedFirstOutputFile);
			var tariffCountInFirstOutputFile = (firstOutputFileContent.Length - firstOutputFileContent.Replace("<RefCusTariff>", "").Length) / 14;
			Assert.AreEqual(9, tariffCountInFirstOutputFile);

			Assert.That(firstOutputFileContent.Contains("<DataSource>FR Tariff chapter 01</DataSource>"));
			Assert.That(firstOutputFileContent.Contains("<ZZ1_TariffCode>0101210000</ZZ1_TariffCode>"));

			Assert.That(File.Exists(expected2ndOutputFile));
			var secondOutputFileContent = File.ReadAllText(expected2ndOutputFile);
			var tariffCountIn2ndOutputFile = (secondOutputFileContent.Length - secondOutputFileContent.Replace("<RefCusTariff>", "").Length) / 14;
			Assert.AreEqual(2, tariffCountIn2ndOutputFile);
			Assert.That(secondOutputFileContent.Contains("<DataSource>FR Tariff chapter 22</DataSource>"));
			Assert.That(secondOutputFileContent.Contains("<ZZ1_TariffCode>2203000100</ZZ1_TariffCode>"));
			Assert.That(secondOutputFileContent.Contains("<ZZ1_TariffCode>2208409900</ZZ1_TariffCode>"));

			Assert.IsFalse(File.Exists(expected3rdOutputFile), "This file was not created, because no import condition, nor VAT applicability nor rates where available for tariffs in chapter 99");
		}

		[Test]
		public void AdditionalCodeDescriptionCorrectlyTruncated()
		{
			var tariffCode = "7228302010";
			var tariffCodeList = new List<string> { tariffCode };
			ApplicationConfig.Instance.DownloadDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\");
			ApplicationConfig.Instance.OutputDirectory = Path.GetTempPath();
			var expectedOutputFile = Path.Combine(ApplicationConfig.Instance.OutputDirectory, $"FRTariffData_Chapter{tariffCode.Substring(0, 2)}.xml");
			File.Delete(expectedOutputFile);
			var generator = new TARICTariffUniversalReferenceDataFileGenerator();
			generator.GenerateURDFiles(tariffCodeList.ToArray(), Common.UniversalXmlWriter.UpdateType.Full, DateTime.Now);
			Assert.IsTrue(File.Exists(expectedOutputFile));
			var outputFileContent = File.ReadAllText(expectedOutputFile);
			Assert.IsTrue(outputFileContent.Contains("<ZX5_Description>seringues pour insuline ou hormone de croissance, à usage unique ou réutilisables, quels que soient leurs usages effectifs (utilisation pour une personne diabétique ou pour tout autre personne) et la nature des produits effectivement injectés  (insuline, hormone de croissance, interféron...), ainsi que les trousses de prévention de la contamination par les virus du sida et hépatites composés d'objets relevant eux-mêmes du taux réduit (préservatifs, seringues pour insuline, solutions stériles pou</ZX5_Description>"));
		}

		public string GenerateFileForGivenTariff(string tariffCode, UpdateType updateType = UpdateType.Full)
		{
			var tariffCodeList = new List<string> { tariffCode };
			ApplicationConfig.Instance.DownloadDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\");
			ApplicationConfig.Instance.OutputDirectory = Path.GetTempPath();

			var outputFilePath = updateType == UpdateType.Full ? Path.Combine(ApplicationConfig.Instance.OutputDirectory, $"FRTariffData_Chapter{tariffCode.Substring(0, 2)}.xml") : $"FRTariffData_Update_{DateTime.Today.ToString("yyyyMMdd", CultureInfo.InvariantCulture)}.XML";
			File.Delete(outputFilePath);

			var generator = new TARICTariffUniversalReferenceDataFileGenerator();
			generator.GenerateURDFiles(tariffCodeList.ToArray(), updateType, DateTime.Now);

			return outputFilePath;
		}

		[Test]
		public void GenerateFilesForNonExistingTariffs()
		{
			var expectedOutputFile = GenerateFileForGivenTariff("xxxxxxxx");
			Assert.IsFalse(File.Exists(expectedOutputFile));
		}

		[Test]
		public void GenerateFilesForTariffs()
		{
			var expectedOutputFile = GenerateFileForGivenTariff("7228302010");

			Assert.IsTrue(File.Exists(expectedOutputFile));
			var outputFileContent = File.ReadAllText(expectedOutputFile);
			Assert.IsTrue(outputFileContent.Contains("<Schema>"));
			Assert.IsTrue(outputFileContent.Contains("<DataSource>FR Tariff chapter 72</DataSource>"));
			Assert.IsTrue(outputFileContent.Contains("<UpdateType>Full</UpdateType>"));
			Assert.IsTrue(outputFileContent.Contains(@"<Schema>
    <EntityType Name=""RefCusTariff"">
      <Key>
        <PropertyRef Name=""ZZ1_TariffCode"" />
        <PropertyRef Name=""ZZ1_ZZI_NKTariffType"" />
        <PropertyRef Name=""ZZ1_ZZI_ZZZ_NKDataGrouping"" />
        <PropertyRef Name=""ZZ1_ZZZ_NKDataGrouping"" />
      </Key>
      <Property Name=""RefCusCondition"" Type=""RefCusCondition"" />
      <Property Name=""RefCusRate"" Type=""RefCusRate"" />
      <Property Name=""RefCusTariffAdditionalCode"" Type=""RefCusTariffAdditionalCode"" />
      <Property Name=""RefCusTariffUOM"" Type=""RefCusTariffUOM"" />
      <Property Name=""RefCusVATApplicability"" Type=""RefCusVATApplicability"" />
      <Property Name=""ZZ1_TariffCode"" Type=""varchar"" MaxLength=""35"" />
      <Property Name=""ZZ1_ZZI_NKTariffType"" Type=""varchar"" MaxLength=""5"" ConstantValue=""IMP"" />
      <Property Name=""ZZ1_ZZI_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""3"" ConstantValue=""EUN"" />
      <Property Name=""ZZ1_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""3"" ConstantValue=""EUN"" />
    </EntityType>
    <EntityType Name=""RefCusVATApplicability"" Data=""true"">
      <Key>
        <PropertyRef Name=""ZX5_AdditionalCode"" />
        <PropertyRef Name=""ZX5_ZZA_NKTradeGroup"" />
        <PropertyRef Name=""ZX5_ZZA_ZZZ_NKDataGrouping"" />
        <PropertyRef Name=""ZX5_ZZF_NKTaxOrFeeCode"" />
        <PropertyRef Name=""ZX5_ZZZ_NKDataGrouping"" />
      </Key>
      <Property Name=""ZX5_AdditionalCode"" Type=""varchar"" MaxLength=""15"" />
      <Property Name=""ZX5_Description"" Type=""nvarchar"" MaxLength=""500"" />
      <Property Name=""ZX5_EndDate"" Type=""smalldatetime"" />
      <Property Name=""ZX5_StartDate"" Type=""smalldatetime"" />
      <Property Name=""ZX5_VATCategory"" Type=""varchar"" MaxLength=""4"" />
      <Property Name=""ZX5_ZZA_NKTradeGroup"" Type=""varchar"" MaxLength=""35"" />
      <Property Name=""ZX5_ZZA_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""3"" ConstantValue=""FR"" />
      <Property Name=""ZX5_ZZF_NKTaxOrFeeCode"" Type=""varchar"" MaxLength=""4"" />
      <Property Name=""ZX5_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""3"" ConstantValue=""FR"" />
    </EntityType>
    <EntityType Name=""RefCusCondition"" Data=""true"">
      <Key>
        <PropertyRef Name=""RefCusApplicability"" />
        <PropertyRef Name=""ZX1_ZX2_NKConditionType"" />
        <PropertyRef Name=""ZX1_ZX2_ZZZ_NKDataGrouping"" />
        <PropertyRef Name=""ZX1_ZZZ_NKDataGrouping"" />
      </Key>
      <Property Name=""RefCusApplicability"" Type=""RefCusApplicability"" />
      <Property Name=""RefCusConditionValue"" Type=""RefCusConditionValue"" />
      <Property Name=""ZX1_Comment"" Type=""nvarchar"" />
      <Property Name=""ZX1_ConditionValueTrueMeansStop"" Type=""bit"" ConstantValue=""False"" />
      <Property Name=""ZX1_EndDate"" Type=""smalldatetime"" />
      <Property Name=""ZX1_IsExport"" Type=""bit"" />
      <Property Name=""ZX1_IsImport"" Type=""bit"" />
      <Property Name=""ZX1_LogicalANDWithinGroup"" Type=""tinyint"" ConstantValue=""0"" />
      <Property Name=""ZX1_Source"" Type=""nvarchar"" ConstantValue=""RITA"" />
      <Property Name=""ZX1_StartDate"" Type=""smalldatetime"" />
      <Property Name=""ZX1_ZX2_NKConditionType"" Type=""varchar"" MaxLength=""6"" />
      <Property Name=""ZX1_ZX2_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""3"" ConstantValue=""FR"" />
      <Property Name=""ZX1_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""3"" ConstantValue=""FR"" />
    </EntityType>
    <EntityType Name=""RefCusRate"" Data=""true"">
      <Key>
        <PropertyRef Name=""RefCusApplicability"" />
        <PropertyRef Name=""ZZ2_ZY1_NKRateCode"" />
        <PropertyRef Name=""ZZ2_ZY1_ZZR_NKRateType"" />
        <PropertyRef Name=""ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping"" />
        <PropertyRef Name=""ZZ2_ZZZ_NKDataGrouping"" />
      </Key>
      <Property Name=""RefCusApplicability"" Type=""RefCusApplicability"" />
      <Property Name=""RefCusRateUOM"" Type=""RefCusRateUOM"" />
      <Property Name=""ZZ2_EndDate"" Type=""datetime"" />
      <Property Name=""ZZ2_RateFormula"" Type=""varchar"" MaxLength=""500"" />
      <Property Name=""ZZ2_RateFormulaDerivedFrom"" Type=""nvarchar"" MaxLength=""1000"" />
      <Property Name=""ZZ2_StartDate"" Type=""datetime"" />
      <Property Name=""ZZ2_ZY1_NKRateCode"" Type=""varchar"" MaxLength=""5"" />
      <Property Name=""ZZ2_ZY1_ZZR_NKRateType"" Type=""varchar"" MaxLength=""3"" />
      <Property Name=""ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""3"" ConstantValue=""FR"" />
      <Property Name=""ZZ2_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""3"" ConstantValue=""FR"" />
    </EntityType>
    <EntityType Name=""RefCusRateUOM"" Data=""true"">
      <Key>
        <PropertyRef Name=""ZXG_UOM"" />
      </Key>
      <Property Name=""ZXG_UOM"" Type=""varchar"" MaxLength=""10"" />
    </EntityType>
    <EntityType Name=""RefCusApplicability"" Data=""true"">
      <Key>
        <PropertyRef Name=""ZZT_AdditionalCode"" />
        <PropertyRef Name=""ZZT_ZZA_NKSecondTradeGroup"" />
        <PropertyRef Name=""ZZT_ZZA_NKTradeGroup"" />
        <PropertyRef Name=""ZZT_ZZA_ZZZ_NKDataGrouping"" />
        <PropertyRef Name=""ZZT_ZZA_ZZZ_NKSecondDataGrouping"" />
      </Key>
      <Property Name=""RefCusExcludedTradeGroup"" Type=""RefCusExcludedTradeGroup"" />
      <Property Name=""ZZT_AdditionalCode"" Type=""nvarchar"" MaxLength=""15"" />
      <Property Name=""ZZT_EndDate"" Type=""smalldatetime"" />
      <Property Name=""ZZT_StartDate"" Type=""smalldatetime"" />
      <Property Name=""ZZT_ZZA_NKSecondTradeGroup"" Type=""varchar"" MaxLength=""35"" />
      <Property Name=""ZZT_ZZA_NKTradeGroup"" Type=""varchar"" MaxLength=""35"" />
      <Property Name=""ZZT_ZZA_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""3"" ConstantValue=""FR"" />
      <Property Name=""ZZT_ZZA_ZZZ_NKSecondDataGrouping"" Type=""varchar"" MaxLength=""3"" />
    </EntityType>
    <EntityType Name=""RefCusExcludedTradeGroup"" Data=""true"">
      <Key>
        <PropertyRef Name=""ZZC_ZZA_NKTradeGroup"" />
        <PropertyRef Name=""ZZC_ZZA_ZZZ_NKDataGrouping"" />
      </Key>
      <Property Name=""ZZC_ZZA_NKTradeGroup"" Type=""varchar"" MaxLength=""35"" />
      <Property Name=""ZZC_ZZA_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""3"" ConstantValue=""FR"" />
    </EntityType>
    <EntityType Name=""RefCusConditionValue"" Data=""true"">
      <Key>
        <PropertyRef Name=""ZX3_Value"" />
        <PropertyRef Name=""ZX3_ZX4_NKValueType"" />
        <PropertyRef Name=""ZX3_ZX4_ZZZ_NKDataGrouping"" />
      </Key>
      <Property Name=""ZX3_LogicalORWithinGroup"" Type=""tinyint"" DefaultValue=""0"" />
      <Property Name=""ZX3_Value"" Type=""nvarchar"" MaxLength=""500"" />
      <Property Name=""ZX3_ZX4_NKValueType"" Type=""varchar"" MaxLength=""5"" />
      <Property Name=""ZX3_ZX4_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""3"" ConstantValue=""FR"" />
    </EntityType>
    <EntityType Name=""RefCusTariffAdditionalCode"" Data=""true"">
      <Key>
        <PropertyRef Name=""RefCusApplicability"" />
        <PropertyRef Name=""ZY2_AdditionalCode"" />
        <PropertyRef Name=""ZY2_ZY3_NKCategory"" />
        <PropertyRef Name=""ZY2_ZZZ_NKDataGrouping"" />
      </Key>
      <Property Name=""RefCusApplicability"" Type=""RefCusApplicability"" />
      <Property Name=""ZY2_AdditionalCode"" Type=""nvarchar"" MaxLength=""15"" />
      <Property Name=""ZY2_Description"" Type=""nvarchar"" MaxLength=""200"" />
      <Property Name=""ZY2_ZY3_NKCategory"" Type=""char"" MaxLength=""3"" />
      <Property Name=""ZY2_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""3"" ConstantValue=""FR"" />
    </EntityType>
    <EntityType Name=""RefCusTariffUOM"" Data=""true"" EnableExpirable=""true"">
      <Key>
        <PropertyRef Name=""ZZ8_Type"" />
        <PropertyRef Name=""ZZ8_UOM"" />
        <PropertyRef Name=""ZZ8_ZZA_NKSecondTradeGroup"" />
        <PropertyRef Name=""ZZ8_ZZA_NKTradeGroup"" />
        <PropertyRef Name=""ZZ8_ZZA_ZZZ_NKDataGrouping"" />
        <PropertyRef Name=""ZZ8_ZZA_ZZZ_NKSecondDataGrouping"" />
        <PropertyRef Name=""ZZ8_ZZZ_NKDataGrouping"" />
      </Key>
      <Property Name=""ZZ8_EndDate"" Type=""smalldatetime"" />
      <Property Name=""ZZ8_StartDate"" Type=""smalldatetime"" />
      <Property Name=""ZZ8_Type"" Type=""varchar"" MaxLength=""3"" />
      <Property Name=""ZZ8_UOM"" Type=""varchar"" MaxLength=""10"" />
      <Property Name=""ZZ8_ZZA_NKSecondTradeGroup"" Type=""varchar"" MaxLength=""35"" />
      <Property Name=""ZZ8_ZZA_NKTradeGroup"" Type=""varchar"" MaxLength=""35"" />
      <Property Name=""ZZ8_ZZA_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""3"" ConstantValue=""FR"" />
      <Property Name=""ZZ8_ZZA_ZZZ_NKSecondDataGrouping"" Type=""varchar"" MaxLength=""3"" />
      <Property Name=""ZZ8_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""3"" ConstantValue=""FR"" />
    </EntityType>
  </Schema>"));

			Assert.IsTrue(outputFileContent.Contains(@"<RefCusVATApplicability>
      <ZX5_AdditionalCode />
      <ZX5_Description />
      <ZX5_EndDate>2079-06-06T23:59:00</ZX5_EndDate>
      <ZX5_StartDate>2014-01-01T00:00:00</ZX5_StartDate>
      <ZX5_VATCategory>A445</ZX5_VATCategory>
      <ZX5_ZZA_NKTradeGroup>MAYOT</ZX5_ZZA_NKTradeGroup>
      <ZX5_ZZF_NKTaxOrFeeCode>NIL</ZX5_ZZF_NKTaxOrFeeCode>
    </RefCusVATApplicability>"));

			Assert.IsTrue(outputFileContent.Contains(@"<RefCusVATApplicability>
      <ZX5_AdditionalCode>V903</ZX5_AdditionalCode>
      <ZX5_Description>déchets neufs d'industrie et matières premières de récupération issus de produits soumis au taux de 5,5 pour cent en application de l'article 278-0 bis du CGI</ZX5_Description>
      <ZX5_EndDate>2079-06-06T23:59:00</ZX5_EndDate>
      <ZX5_StartDate>1900-01-01T00:00:00</ZX5_StartDate>
      <ZX5_VATCategory>A505</ZX5_VATCategory>
      <ZX5_ZZA_NKTradeGroup>CORSE</ZX5_ZZA_NKTradeGroup>
      <ZX5_ZZF_NKTaxOrFeeCode>RDE</ZX5_ZZF_NKTaxOrFeeCode>
    </RefCusVATApplicability>"));

			Assert.IsTrue(outputFileContent.Contains(@"<RefCusCondition>
      <ZX1_Comment />
      <ZX1_EndDate>2079-06-06T23:59:00</ZX1_EndDate>
      <ZX1_IsExport>False</ZX1_IsExport>
      <ZX1_IsImport>True</ZX1_IsImport>
      <ZX1_StartDate>2014-01-01T00:00:00</ZX1_StartDate>
      <ZX1_ZX2_NKConditionType>VAT</ZX1_ZX2_NKConditionType>
      <RefCusApplicability>
        <ZZT_AdditionalCode>V900</ZZT_AdditionalCode>
        <ZZT_EndDate>2079-06-06T23:59:00</ZZT_EndDate>
        <ZZT_StartDate>2014-01-01T00:00:00</ZZT_StartDate>
        <ZZT_ZZA_NKSecondTradeGroup>CONTI</ZZT_ZZA_NKSecondTradeGroup>
        <ZZT_ZZA_NKTradeGroup>FR01</ZZT_ZZA_NKTradeGroup>
        <ZZT_ZZA_ZZZ_NKSecondDataGrouping />
      </RefCusApplicability>
      <RefCusConditionValue>
        <ZX3_Value>6018</ZX3_Value>
        <ZX3_ZX4_NKValueType>SUP</ZX3_ZX4_NKValueType>
      </RefCusConditionValue>
      <RefCusConditionValue>
        <ZX3_Value>6802</ZX3_Value>
        <ZX3_ZX4_NKValueType>SNR</ZX3_ZX4_NKValueType>
      </RefCusConditionValue>
    </RefCusCondition>"));
		}

		[Test]
		public void TestOutputFor3002201000()
		{
			var expectedOutputFile = GenerateFileForGivenTariff("3002201000");
			var outputFileContent = File.ReadAllText(expectedOutputFile);
			Assert.IsTrue(outputFileContent.Contains(@"<RefCusCondition>
      <ZX1_Comment />
      <ZX1_EndDate>2079-06-06T23:59:00</ZX1_EndDate>
      <ZX1_IsExport>False</ZX1_IsExport>
      <ZX1_IsImport>True</ZX1_IsImport>
      <ZX1_StartDate>2006-01-01T00:00:00</ZX1_StartDate>
      <ZX1_ZX2_NKConditionType>VAT</ZX1_ZX2_NKConditionType>
      <RefCusApplicability>
        <ZZT_AdditionalCode>V027</ZZT_AdditionalCode>
        <ZZT_EndDate>2079-06-06T23:59:00</ZZT_EndDate>
        <ZZT_StartDate>2006-01-01T00:00:00</ZZT_StartDate>
        <ZZT_ZZA_NKSecondTradeGroup>MGPRE</ZZT_ZZA_NKSecondTradeGroup>
        <ZZT_ZZA_NKTradeGroup>FR01</ZZT_ZZA_NKTradeGroup>
        <ZZT_ZZA_ZZZ_NKSecondDataGrouping />
      </RefCusApplicability>
      <RefCusConditionValue>
        <ZX3_Value>6011</ZX3_Value>
        <ZX3_ZX4_NKValueType>SUP</ZX3_ZX4_NKValueType>
      </RefCusConditionValue>
      <RefCusConditionValue>
        <ZX3_Value>6012</ZX3_Value>
        <ZX3_ZX4_NKValueType>SUP</ZX3_ZX4_NKValueType>
      </RefCusConditionValue>
      <RefCusConditionValue>
        <ZX3_Value>6013</ZX3_Value>
        <ZX3_ZX4_NKValueType>SUP</ZX3_ZX4_NKValueType>
      </RefCusConditionValue>
      <RefCusConditionValue>
        <ZX3_Value>6801</ZX3_Value>
        <ZX3_ZX4_NKValueType>SNR</ZX3_ZX4_NKValueType>
      </RefCusConditionValue>
      <RefCusConditionValue>
        <ZX3_Value>6015</ZX3_Value>
        <ZX3_ZX4_NKValueType>SUP</ZX3_ZX4_NKValueType>
      </RefCusConditionValue>
    </RefCusCondition>"));
		}

		[Test]
		public void TestOutputFor2501009900NotContainsEmptyConditionValue()
		{
			var expectedOutputFile = GenerateFileForGivenTariff("2501009900");
			var outputFileContent = File.ReadAllText(expectedOutputFile);
			Assert.IsFalse(outputFileContent.Contains(@"<ZX3_Value />"));
		}

		[Test]
		public void TestOutputFor0208903000()
		{
			var expectedOutputFile = GenerateFileForGivenTariff("0208903000");
			var outputFileContent = File.ReadAllText(expectedOutputFile);
			Assert.IsTrue(outputFileContent.Contains(@"<RefCusRate>
      <ZZ2_EndDate>2079-06-06T23:59:00</ZZ2_EndDate>
      <ZZ2_RateFormula>MIN(350 * [FLAT], MAX(1.5 * [TNE3], 30 * [FLAT]))</ZZ2_RateFormula>
      <ZZ2_RateFormulaDerivedFrom>1,5 EUR/Tonne lot - Minimum : 30 EUR - Maximum : 350 EUR</ZZ2_RateFormulaDerivedFrom>
      <ZZ2_StartDate>2012-04-01T00:00:00</ZZ2_StartDate>
      <ZZ2_ZY1_NKRateCode>G065</ZZ2_ZY1_NKRateCode>
      <ZZ2_ZY1_ZZR_NKRateType>RED</ZZ2_ZY1_ZZR_NKRateType>
      <RefCusApplicability>
        <ZZT_AdditionalCode>Q213</ZZT_AdditionalCode>
        <ZZT_EndDate>2079-06-06T23:59:00</ZZT_EndDate>
        <ZZT_StartDate>2012-04-01T00:00:00</ZZT_StartDate>
        <ZZT_ZZA_NKSecondTradeGroup />
        <ZZT_ZZA_NKTradeGroup>NZ</ZZT_ZZA_NKTradeGroup>
        <ZZT_ZZA_ZZZ_NKSecondDataGrouping />
      </RefCusApplicability>
      <RefCusRateUOM>
        <ZXG_UOM>TNE3</ZXG_UOM>
      </RefCusRateUOM>
    </RefCusRate>"));
			Assert.IsTrue(outputFileContent.Contains(@"<RefCusRate>
      <ZZ2_EndDate>2079-06-06T23:59:00</ZZ2_EndDate>
      <ZZ2_RateFormula>MIN(457.35 * [FLAT], MAX(6.1 * [TNE3], 30.49 * [FLAT]))</ZZ2_RateFormula>
      <ZZ2_RateFormulaDerivedFrom>6,1 EUR/Tonne lot - Minimum : 30,49 EUR - Maximum : 457,35 EUR</ZZ2_RateFormulaDerivedFrom>
      <ZZ2_StartDate>2012-04-01T00:00:00</ZZ2_StartDate>
      <ZZ2_ZY1_NKRateCode>G065</ZZ2_ZY1_NKRateCode>
      <ZZ2_ZY1_ZZR_NKRateType>RED</ZZ2_ZY1_ZZR_NKRateType>
      <RefCusApplicability>
        <ZZT_AdditionalCode>Q202</ZZT_AdditionalCode>
        <ZZT_EndDate>2079-06-06T23:59:00</ZZT_EndDate>
        <ZZT_StartDate>2012-04-01T00:00:00</ZZT_StartDate>
        <ZZT_ZZA_NKSecondTradeGroup />
        <ZZT_ZZA_NKTradeGroup>1011</ZZT_ZZA_NKTradeGroup>
        <ZZT_ZZA_ZZZ_NKSecondDataGrouping />
        <RefCusExcludedTradeGroup>
          <ZZC_ZZA_NKTradeGroup>AD</ZZC_ZZA_NKTradeGroup>
        </RefCusExcludedTradeGroup>
        <RefCusExcludedTradeGroup>
          <ZZC_ZZA_NKTradeGroup>CH</ZZC_ZZA_NKTradeGroup>
        </RefCusExcludedTradeGroup>
        <RefCusExcludedTradeGroup>
          <ZZC_ZZA_NKTradeGroup>FO</ZZC_ZZA_NKTradeGroup>
        </RefCusExcludedTradeGroup>
        <RefCusExcludedTradeGroup>
          <ZZC_ZZA_NKTradeGroup>IS</ZZC_ZZA_NKTradeGroup>
        </RefCusExcludedTradeGroup>
        <RefCusExcludedTradeGroup>
          <ZZC_ZZA_NKTradeGroup>LI</ZZC_ZZA_NKTradeGroup>
        </RefCusExcludedTradeGroup>
        <RefCusExcludedTradeGroup>
          <ZZC_ZZA_NKTradeGroup>NO</ZZC_ZZA_NKTradeGroup>
        </RefCusExcludedTradeGroup>
        <RefCusExcludedTradeGroup>
          <ZZC_ZZA_NKTradeGroup>NZ</ZZC_ZZA_NKTradeGroup>
        </RefCusExcludedTradeGroup>
        <RefCusExcludedTradeGroup>
          <ZZC_ZZA_NKTradeGroup>SM</ZZC_ZZA_NKTradeGroup>
        </RefCusExcludedTradeGroup>
      </RefCusApplicability>
      <RefCusRateUOM>
        <ZXG_UOM>TNE3</ZXG_UOM>
      </RefCusRateUOM>
    </RefCusRate>"));
			Assert.IsTrue(outputFileContent.Contains(@"<RefCusRate>
      <ZZ2_EndDate>2079-06-06T23:59:00</ZZ2_EndDate>
      <ZZ2_RateFormula>0</ZZ2_RateFormula>
      <ZZ2_RateFormulaDerivedFrom>0</ZZ2_RateFormulaDerivedFrom>
      <ZZ2_StartDate>2012-04-01T00:00:00</ZZ2_StartDate>
      <ZZ2_ZY1_NKRateCode>G065</ZZ2_ZY1_NKRateCode>
      <ZZ2_ZY1_ZZR_NKRateType>RED</ZZ2_ZY1_ZZR_NKRateType>
      <RefCusApplicability>
        <ZZT_AdditionalCode>Q210</ZZT_AdditionalCode>
        <ZZT_EndDate>2079-06-06T23:59:00</ZZT_EndDate>
        <ZZT_StartDate>2012-04-01T00:00:00</ZZT_StartDate>
        <ZZT_ZZA_NKSecondTradeGroup />
        <ZZT_ZZA_NKTradeGroup>1011</ZZT_ZZA_NKTradeGroup>
        <ZZT_ZZA_ZZZ_NKSecondDataGrouping />
        <RefCusExcludedTradeGroup>
          <ZZC_ZZA_NKTradeGroup>AD</ZZC_ZZA_NKTradeGroup>
        </RefCusExcludedTradeGroup>
        <RefCusExcludedTradeGroup>
          <ZZC_ZZA_NKTradeGroup>CH</ZZC_ZZA_NKTradeGroup>
        </RefCusExcludedTradeGroup>
        <RefCusExcludedTradeGroup>
          <ZZC_ZZA_NKTradeGroup>FO</ZZC_ZZA_NKTradeGroup>
        </RefCusExcludedTradeGroup>
        <RefCusExcludedTradeGroup>
          <ZZC_ZZA_NKTradeGroup>IS</ZZC_ZZA_NKTradeGroup>
        </RefCusExcludedTradeGroup>
        <RefCusExcludedTradeGroup>
          <ZZC_ZZA_NKTradeGroup>LI</ZZC_ZZA_NKTradeGroup>
        </RefCusExcludedTradeGroup>
        <RefCusExcludedTradeGroup>
          <ZZC_ZZA_NKTradeGroup>NO</ZZC_ZZA_NKTradeGroup>
        </RefCusExcludedTradeGroup>
        <RefCusExcludedTradeGroup>
          <ZZC_ZZA_NKTradeGroup>SM</ZZC_ZZA_NKTradeGroup>
        </RefCusExcludedTradeGroup>
      </RefCusApplicability>
    </RefCusRate>"));
			Assert.IsTrue(outputFileContent.Contains(@"<RefCusRate>
      <ZZ2_EndDate>2079-06-06T23:59:00</ZZ2_EndDate>
      <ZZ2_RateFormula>2 * [007]</ZZ2_RateFormula>
      <ZZ2_RateFormulaDerivedFrom>2 EUR/Tonne nette de viande importée déduction faite du poids des abats</ZZ2_RateFormulaDerivedFrom>
      <ZZ2_StartDate>2013-01-01T00:00:00</ZZ2_StartDate>
      <ZZ2_ZY1_NKRateCode>E485</ZZ2_ZY1_NKRateCode>
      <ZZ2_ZY1_ZZR_NKRateType>RED</ZZ2_ZY1_ZZR_NKRateType>
      <RefCusApplicability>
        <ZZT_AdditionalCode>Q236</ZZT_AdditionalCode>
        <ZZT_EndDate>2079-06-06T23:59:00</ZZT_EndDate>
        <ZZT_StartDate>2013-01-01T00:00:00</ZZT_StartDate>
        <ZZT_ZZA_NKSecondTradeGroup />
        <ZZT_ZZA_NKTradeGroup>FR13</ZZT_ZZA_NKTradeGroup>
        <ZZT_ZZA_ZZZ_NKSecondDataGrouping />
      </RefCusApplicability>
      <RefCusRateUOM>
        <ZXG_UOM>007</ZXG_UOM>
      </RefCusRateUOM>
    </RefCusRate>"));
		}

		[Test]
		public void TestOutputFor3816000000()
		{
			var expectedOutputFile = GenerateFileForGivenTariff("3816000000");
			var outputFileContent = File.ReadAllText(expectedOutputFile);
			Assert.IsTrue(outputFileContent.Contains(@"<RefCusRate>
      <ZZ2_EndDate>2079-06-06T23:59:00</ZZ2_EndDate>
      <ZZ2_RateFormula>VFD * 0.0033</ZZ2_RateFormula>
      <ZZ2_RateFormulaDerivedFrom>0,33 %</ZZ2_RateFormulaDerivedFrom>
      <ZZ2_StartDate>2019-01-01T00:00:00</ZZ2_StartDate>
      <ZZ2_ZY1_NKRateCode>N625</ZZ2_ZY1_NKRateCode>
      <ZZ2_ZY1_ZZR_NKRateType>ROC</ZZ2_ZY1_ZZR_NKRateType>
      <RefCusApplicability>
        <ZZT_AdditionalCode>T118</ZZT_AdditionalCode>
        <ZZT_EndDate>2079-06-06T23:59:00</ZZT_EndDate>
        <ZZT_StartDate>2019-01-01T00:00:00</ZZT_StartDate>
        <ZZT_ZZA_NKSecondTradeGroup />
        <ZZT_ZZA_NKTradeGroup>1011</ZZT_ZZA_NKTradeGroup>
        <ZZT_ZZA_ZZZ_NKSecondDataGrouping />
        <RefCusExcludedTradeGroup>
          <ZZC_ZZA_NKTradeGroup>IS</ZZC_ZZA_NKTradeGroup>
        </RefCusExcludedTradeGroup>
        <RefCusExcludedTradeGroup>
          <ZZC_ZZA_NKTradeGroup>LI</ZZC_ZZA_NKTradeGroup>
        </RefCusExcludedTradeGroup>
        <RefCusExcludedTradeGroup>
          <ZZC_ZZA_NKTradeGroup>NO</ZZC_ZZA_NKTradeGroup>
        </RefCusExcludedTradeGroup>
        <RefCusExcludedTradeGroup>
          <ZZC_ZZA_NKTradeGroup>TR</ZZC_ZZA_NKTradeGroup>
        </RefCusExcludedTradeGroup>
      </RefCusApplicability>
    </RefCusRate>"));
			Assert.IsTrue(outputFileContent.Contains(@"<RefCusRate>
      <ZZ2_EndDate>2079-06-06T23:59:00</ZZ2_EndDate>
      <ZZ2_RateFormula>0</ZZ2_RateFormula>
      <ZZ2_RateFormulaDerivedFrom>0</ZZ2_RateFormulaDerivedFrom>
      <ZZ2_StartDate>2009-01-01T00:00:00</ZZ2_StartDate>
      <ZZ2_ZY1_NKRateCode>N625</ZZ2_ZY1_NKRateCode>
      <ZZ2_ZY1_ZZR_NKRateType>ROC</ZZ2_ZY1_ZZR_NKRateType>
      <RefCusApplicability>
        <ZZT_AdditionalCode>T004</ZZT_AdditionalCode>
        <ZZT_EndDate>2079-06-06T23:59:00</ZZT_EndDate>
        <ZZT_StartDate>2009-01-01T00:00:00</ZZT_StartDate>
        <ZZT_ZZA_NKSecondTradeGroup />
        <ZZT_ZZA_NKTradeGroup>1011</ZZT_ZZA_NKTradeGroup>
        <ZZT_ZZA_ZZZ_NKSecondDataGrouping />
        <RefCusExcludedTradeGroup>
          <ZZC_ZZA_NKTradeGroup>IS</ZZC_ZZA_NKTradeGroup>
        </RefCusExcludedTradeGroup>
        <RefCusExcludedTradeGroup>
          <ZZC_ZZA_NKTradeGroup>LI</ZZC_ZZA_NKTradeGroup>
        </RefCusExcludedTradeGroup>
        <RefCusExcludedTradeGroup>
          <ZZC_ZZA_NKTradeGroup>NO</ZZC_ZZA_NKTradeGroup>
        </RefCusExcludedTradeGroup>
        <RefCusExcludedTradeGroup>
          <ZZC_ZZA_NKTradeGroup>TR</ZZC_ZZA_NKTradeGroup>
        </RefCusExcludedTradeGroup>
      </RefCusApplicability>
    </RefCusRate>"));
		}

		[Test]
		public void TestOutputFor4303900000()
		{
			var expectedOutputFile = GenerateFileForGivenTariff("4303900000");
			var outputFileContent = File.ReadAllText(expectedOutputFile);
			Assert.IsTrue(outputFileContent.Contains(@"<RefCusRate>
      <ZZ2_EndDate>2079-06-06T23:59:00</ZZ2_EndDate>
      <ZZ2_RateFormula>VFD * 0.00068</ZZ2_RateFormula>
      <ZZ2_RateFormulaDerivedFrom>0,068 %</ZZ2_RateFormulaDerivedFrom>
      <ZZ2_StartDate>2019-01-01T00:00:00</ZZ2_StartDate>
      <ZZ2_ZY1_NKRateCode>M830</ZZ2_ZY1_NKRateCode>
      <ZZ2_ZY1_ZZR_NKRateType>TDH</ZZ2_ZY1_ZZR_NKRateType>
      <RefCusApplicability>
        <ZZT_AdditionalCode />
        <ZZT_EndDate>2079-06-06T23:59:00</ZZT_EndDate>
        <ZZT_StartDate>2019-01-01T00:00:00</ZZT_StartDate>
        <ZZT_ZZA_NKSecondTradeGroup />
        <ZZT_ZZA_NKTradeGroup>1011</ZZT_ZZA_NKTradeGroup>
        <ZZT_ZZA_ZZZ_NKSecondDataGrouping />
        <RefCusExcludedTradeGroup>
          <ZZC_ZZA_NKTradeGroup>IS</ZZC_ZZA_NKTradeGroup>
        </RefCusExcludedTradeGroup>
        <RefCusExcludedTradeGroup>
          <ZZC_ZZA_NKTradeGroup>LI</ZZC_ZZA_NKTradeGroup>
        </RefCusExcludedTradeGroup>
        <RefCusExcludedTradeGroup>
          <ZZC_ZZA_NKTradeGroup>NO</ZZC_ZZA_NKTradeGroup>
        </RefCusExcludedTradeGroup>
        <RefCusExcludedTradeGroup>
          <ZZC_ZZA_NKTradeGroup>TR</ZZC_ZZA_NKTradeGroup>
        </RefCusExcludedTradeGroup>
      </RefCusApplicability>
    </RefCusRate>"));
		}

		[Test]
		public void TestOutputFor4805500000()
		{
			var expectedOutputFile = GenerateFileForGivenTariff("4805500000");
			var outputFileContent = File.ReadAllText(expectedOutputFile);
			Assert.IsTrue(outputFileContent.Contains(@"    <RefCusRate>
      <ZZ2_EndDate>2079-06-06T23:59:00</ZZ2_EndDate>
      <ZZ2_RateFormula>VFD * 0.0004</ZZ2_RateFormula>
      <ZZ2_RateFormulaDerivedFrom>0,04 %</ZZ2_RateFormulaDerivedFrom>
      <ZZ2_StartDate>2020-01-01T00:00:00</ZZ2_StartDate>
      <ZZ2_ZY1_NKRateCode>Q422</ZZ2_ZY1_NKRateCode>
      <ZZ2_ZY1_ZZR_NKRateType>TPC</ZZ2_ZY1_ZZR_NKRateType>
      <RefCusApplicability>
        <ZZT_AdditionalCode />
        <ZZT_EndDate>2079-06-06T23:59:00</ZZT_EndDate>
        <ZZT_StartDate>2020-01-01T00:00:00</ZZT_StartDate>
        <ZZT_ZZA_NKSecondTradeGroup />
        <ZZT_ZZA_NKTradeGroup>1011</ZZT_ZZA_NKTradeGroup>
        <ZZT_ZZA_ZZZ_NKSecondDataGrouping />
        <RefCusExcludedTradeGroup>
          <ZZC_ZZA_NKTradeGroup>IS</ZZC_ZZA_NKTradeGroup>
        </RefCusExcludedTradeGroup>
        <RefCusExcludedTradeGroup>
          <ZZC_ZZA_NKTradeGroup>LI</ZZC_ZZA_NKTradeGroup>
        </RefCusExcludedTradeGroup>
        <RefCusExcludedTradeGroup>
          <ZZC_ZZA_NKTradeGroup>NO</ZZC_ZZA_NKTradeGroup>
        </RefCusExcludedTradeGroup>
      </RefCusApplicability>
    </RefCusRate>"));
		}

		[Test]
		public void TestOutputFor0709609920()
		{
			var expectedOutputFile = GenerateFileForGivenTariff("0709609920");
			var outputFileContent = File.ReadAllText(expectedOutputFile);
			Assert.IsTrue(outputFileContent.Contains(@"<RefCusRate>
      <ZZ2_EndDate>2079-06-06T23:59:00</ZZ2_EndDate>
      <ZZ2_RateFormula>{""Precalcule""}</ZZ2_RateFormula>
      <ZZ2_RateFormulaDerivedFrom>Précalculée</ZZ2_RateFormulaDerivedFrom>
      <ZZ2_StartDate>2013-04-01T00:00:00</ZZ2_StartDate>
      <ZZ2_ZY1_NKRateCode>E615</ZZ2_ZY1_NKRateCode>
      <ZZ2_ZY1_ZZR_NKRateType>RED</ZZ2_ZY1_ZZR_NKRateType>
      <RefCusApplicability>
        <ZZT_AdditionalCode>Q003</ZZT_AdditionalCode>
        <ZZT_EndDate>2079-06-06T23:59:00</ZZT_EndDate>
        <ZZT_StartDate>2013-04-01T00:00:00</ZZT_StartDate>
        <ZZT_ZZA_NKSecondTradeGroup>DPDOM</ZZT_ZZA_NKSecondTradeGroup>
        <ZZT_ZZA_NKTradeGroup>1011</ZZT_ZZA_NKTradeGroup>
        <ZZT_ZZA_ZZZ_NKSecondDataGrouping>FR</ZZT_ZZA_ZZZ_NKSecondDataGrouping>
      </RefCusApplicability>
    </RefCusRate>
    <RefCusRate>
      <ZZ2_EndDate>2079-06-06T23:59:00</ZZ2_EndDate>
      <ZZ2_RateFormula>0</ZZ2_RateFormula>
      <ZZ2_RateFormulaDerivedFrom>0</ZZ2_RateFormulaDerivedFrom>
      <ZZ2_StartDate>2008-08-01T00:00:00</ZZ2_StartDate>
      <ZZ2_ZY1_NKRateCode>E615</ZZ2_ZY1_NKRateCode>
      <ZZ2_ZY1_ZZR_NKRateType>RED</ZZ2_ZY1_ZZR_NKRateType>
      <RefCusApplicability>
        <ZZT_AdditionalCode>Q004</ZZT_AdditionalCode>
        <ZZT_EndDate>2079-06-06T23:59:00</ZZT_EndDate>
        <ZZT_StartDate>2008-08-01T00:00:00</ZZT_StartDate>
        <ZZT_ZZA_NKSecondTradeGroup>DPDOM</ZZT_ZZA_NKSecondTradeGroup>
        <ZZT_ZZA_NKTradeGroup>1011</ZZT_ZZA_NKTradeGroup>
        <ZZT_ZZA_ZZZ_NKSecondDataGrouping>FR</ZZT_ZZA_ZZZ_NKSecondDataGrouping>
      </RefCusApplicability>
    </RefCusRate>
    <RefCusRate>
      <ZZ2_EndDate>2079-06-06T23:59:00</ZZ2_EndDate>
      <ZZ2_RateFormula>{""Precalcule""}</ZZ2_RateFormula>
      <ZZ2_RateFormulaDerivedFrom>Précalculée</ZZ2_RateFormulaDerivedFrom>
      <ZZ2_StartDate>2014-10-01T00:00:00</ZZ2_StartDate>
      <ZZ2_ZY1_NKRateCode>E615</ZZ2_ZY1_NKRateCode>
      <ZZ2_ZY1_ZZR_NKRateType>RED</ZZ2_ZY1_ZZR_NKRateType>
      <RefCusApplicability>
        <ZZT_AdditionalCode>Q234</ZZT_AdditionalCode>
        <ZZT_EndDate>2079-06-06T23:59:00</ZZT_EndDate>
        <ZZT_StartDate>2014-10-01T00:00:00</ZZT_StartDate>
        <ZZT_ZZA_NKSecondTradeGroup>METRO</ZZT_ZZA_NKSecondTradeGroup>
        <ZZT_ZZA_NKTradeGroup>1011</ZZT_ZZA_NKTradeGroup>
        <ZZT_ZZA_ZZZ_NKSecondDataGrouping>FR</ZZT_ZZA_ZZZ_NKSecondDataGrouping>
      </RefCusApplicability>
    </RefCusRate>"));
		}

		[Test]
		public void TestOutputFor9706900000()
		{
			var expectedOutputFile = GenerateFileForGivenTariff("9706900000");
			var outputFileContent = File.ReadAllText(expectedOutputFile);
			Assert.IsTrue(outputFileContent.Contains(@"<RefCusRate>
      <ZZ2_EndDate>2079-06-06T23:59:00</ZZ2_EndDate>
      <ZZ2_RateFormula>If(has(""CERT"", ""5003""), 0, VFD * 0.005)</ZZ2_RateFormula>
      <ZZ2_RateFormulaDerivedFrom>Si présentation du document 5003 alors le montant à percevoir est 0 sinon le montant à percevoir est 0,5 %</ZZ2_RateFormulaDerivedFrom>
      <ZZ2_StartDate>2014-08-15T00:00:00</ZZ2_StartDate>
      <ZZ2_ZY1_NKRateCode>L565</ZZ2_ZY1_NKRateCode>
      <ZZ2_ZY1_ZZR_NKRateType>CMP</ZZ2_ZY1_ZZR_NKRateType>
      <RefCusApplicability>
        <ZZT_AdditionalCode>T062</ZZT_AdditionalCode>
        <ZZT_EndDate>2079-06-06T23:59:00</ZZT_EndDate>
        <ZZT_StartDate>2014-08-15T00:00:00</ZZT_StartDate>
        <ZZT_ZZA_NKSecondTradeGroup />
        <ZZT_ZZA_NKTradeGroup>1011</ZZT_ZZA_NKTradeGroup>
        <ZZT_ZZA_ZZZ_NKSecondDataGrouping />
      </RefCusApplicability>
    </RefCusRate>"));
			Assert.IsTrue(outputFileContent.Contains(@"<RefCusRate>
      <ZZ2_EndDate>2079-06-06T23:59:00</ZZ2_EndDate>
      <ZZ2_RateFormula>If(has(""CERT"", ""5003""), 0, VFD * 0.06)</ZZ2_RateFormula>
      <ZZ2_RateFormulaDerivedFrom>Si présentation du document 5003 alors le montant à percevoir est 0 sinon le montant à percevoir est 6 %</ZZ2_RateFormulaDerivedFrom>
      <ZZ2_StartDate>2014-08-15T00:00:00</ZZ2_StartDate>
      <ZZ2_ZY1_NKRateCode>A325</ZZ2_ZY1_NKRateCode>
      <ZZ2_ZY1_ZZR_NKRateType>CMP</ZZ2_ZY1_ZZR_NKRateType>
      <RefCusApplicability>
        <ZZT_AdditionalCode>T062</ZZT_AdditionalCode>
        <ZZT_EndDate>2079-06-06T23:59:00</ZZT_EndDate>
        <ZZT_StartDate>2014-08-15T00:00:00</ZZT_StartDate>
        <ZZT_ZZA_NKSecondTradeGroup />
        <ZZT_ZZA_NKTradeGroup>1011</ZZT_ZZA_NKTradeGroup>
        <ZZT_ZZA_ZZZ_NKSecondDataGrouping />
      </RefCusApplicability>
    </RefCusRate>"));
		}

		[Test]
		public void TestOutputFor2208409900()
		{
			var expectedOutputFile = GenerateFileForGivenTariff("2208409900");
			var outputFileContent = File.ReadAllText(expectedOutputFile);
			Assert.IsTrue(outputFileContent.Contains(@"    <RefCusCondition>
      <ZX1_Comment>Le document ou disposition tarifaire particulière suivant doit être présent</ZX1_Comment>
      <ZX1_EndDate>2079-06-06T23:59:00</ZX1_EndDate>
      <ZX1_IsExport>True</ZX1_IsExport>
      <ZX1_IsImport>False</ZX1_IsImport>
      <ZX1_StartDate>2018-01-01T00:00:00</ZX1_StartDate>
      <ZX1_ZX2_NKConditionType>SOU</ZX1_ZX2_NKConditionType>
      <RefCusApplicability>
        <ZZT_AdditionalCode>Q034</ZZT_AdditionalCode>
        <ZZT_EndDate>2079-06-06T23:59:00</ZZT_EndDate>
        <ZZT_StartDate>2018-01-01T00:00:00</ZZT_StartDate>
        <ZZT_ZZA_NKSecondTradeGroup>DPDOM</ZZT_ZZA_NKSecondTradeGroup>
        <ZZT_ZZA_NKTradeGroup>FR</ZZT_ZZA_NKTradeGroup>
        <ZZT_ZZA_ZZZ_NKSecondDataGrouping>FR</ZZT_ZZA_ZZZ_NKSecondDataGrouping>
      </RefCusApplicability>
      <RefCusConditionValue>
        <ZX3_Value>2005</ZX3_Value>
        <ZX3_ZX4_NKValueType>SUP</ZX3_ZX4_NKValueType>
      </RefCusConditionValue>
    </RefCusCondition>"));
			Assert.IsTrue(outputFileContent.Contains(@"    <RefCusCondition>
      <ZX1_Comment>Le document ou disposition tarifaire particulière suivant doit être présent</ZX1_Comment>
      <ZX1_EndDate>2079-06-06T23:59:00</ZX1_EndDate>
      <ZX1_IsExport>True</ZX1_IsExport>
      <ZX1_IsImport>False</ZX1_IsImport>
      <ZX1_StartDate>2009-01-01T00:00:00</ZX1_StartDate>
      <ZX1_ZX2_NKConditionType>SOU</ZX1_ZX2_NKConditionType>
      <RefCusApplicability>
        <ZZT_AdditionalCode>Q033</ZZT_AdditionalCode>
        <ZZT_EndDate>2079-06-06T23:59:00</ZZT_EndDate>
        <ZZT_StartDate>2009-01-01T00:00:00</ZZT_StartDate>
        <ZZT_ZZA_NKSecondTradeGroup>DPDOM</ZZT_ZZA_NKSecondTradeGroup>
        <ZZT_ZZA_NKTradeGroup>FR</ZZT_ZZA_NKTradeGroup>
        <ZZT_ZZA_ZZZ_NKSecondDataGrouping>FR</ZZT_ZZA_ZZZ_NKSecondDataGrouping>
      </RefCusApplicability>
      <RefCusConditionValue>
        <ZX3_Value>2005</ZX3_Value>
        <ZX3_ZX4_NKValueType>SUP</ZX3_ZX4_NKValueType>
      </RefCusConditionValue>
    </RefCusCondition>"));
			Assert.IsTrue(outputFileContent.Contains(@"<RefCusRate>
      <ZZ2_EndDate>2079-06-06T23:59:00</ZZ2_EndDate>
      <ZZ2_RateFormula>0</ZZ2_RateFormula>
      <ZZ2_RateFormulaDerivedFrom>0</ZZ2_RateFormulaDerivedFrom>
      <ZZ2_StartDate>2009-01-01T00:00:00</ZZ2_StartDate>
      <ZZ2_ZY1_NKRateCode>D285</ZZ2_ZY1_NKRateCode>
      <ZZ2_ZY1_ZZR_NKRateType>MSC</ZZ2_ZY1_ZZR_NKRateType>
      <RefCusApplicability>
        <ZZT_AdditionalCode>Q033</ZZT_AdditionalCode>
        <ZZT_EndDate>2079-06-06T23:59:00</ZZT_EndDate>
        <ZZT_StartDate>2009-01-01T00:00:00</ZZT_StartDate>
        <ZZT_ZZA_NKSecondTradeGroup>DPDOM</ZZT_ZZA_NKSecondTradeGroup>
        <ZZT_ZZA_NKTradeGroup>FR</ZZT_ZZA_NKTradeGroup>
        <ZZT_ZZA_ZZZ_NKSecondDataGrouping>FR</ZZT_ZZA_ZZZ_NKSecondDataGrouping>
      </RefCusApplicability>
      <RefCusRateUOM>
        <ZXG_UOM>005</ZXG_UOM>
      </RefCusRateUOM>
    </RefCusRate>"));
			Assert.IsTrue(outputFileContent.Contains(@"<RefCusRate>
      <ZZ2_EndDate>2079-06-06T23:59:00</ZZ2_EndDate>
      <ZZ2_RateFormula>304.9 * [005]</ZZ2_RateFormula>
      <ZZ2_RateFormulaDerivedFrom>304,9 EUR/Hectolitre d'alcool pur</ZZ2_RateFormulaDerivedFrom>
      <ZZ2_StartDate>2018-01-01T00:00:00</ZZ2_StartDate>
      <ZZ2_ZY1_NKRateCode>D285</ZZ2_ZY1_NKRateCode>
      <ZZ2_ZY1_ZZR_NKRateType>MSC</ZZ2_ZY1_ZZR_NKRateType>
      <RefCusApplicability>
        <ZZT_AdditionalCode>Q034</ZZT_AdditionalCode>
        <ZZT_EndDate>2079-06-06T23:59:00</ZZT_EndDate>
        <ZZT_StartDate>2018-01-01T00:00:00</ZZT_StartDate>
        <ZZT_ZZA_NKSecondTradeGroup>DPDOM</ZZT_ZZA_NKSecondTradeGroup>
        <ZZT_ZZA_NKTradeGroup>FR</ZZT_ZZA_NKTradeGroup>
        <ZZT_ZZA_ZZZ_NKSecondDataGrouping>FR</ZZT_ZZA_ZZZ_NKSecondDataGrouping>
      </RefCusApplicability>
      <RefCusRateUOM>
        <ZXG_UOM>005</ZXG_UOM>
      </RefCusRateUOM>
    </RefCusRate>"));
			Assert.IsTrue(outputFileContent.Contains(@"<RefCusCondition>
      <ZX1_Comment>Tous les documents ou dispositions tarifaires particulières suivants sont présents</ZX1_Comment>
      <ZX1_EndDate>2079-06-06T23:59:00</ZX1_EndDate>
      <ZX1_IsExport>False</ZX1_IsExport>
      <ZX1_IsImport>True</ZX1_IsImport>
      <ZX1_StartDate>2021-01-01T00:00:00</ZX1_StartDate>
      <ZX1_ZX2_NKConditionType>MAN</ZX1_ZX2_NKConditionType>
      <RefCusApplicability>
        <ZZT_AdditionalCode>Q031</ZZT_AdditionalCode>
        <ZZT_EndDate>2079-06-06T23:59:00</ZZT_EndDate>
        <ZZT_StartDate>2021-01-01T00:00:00</ZZT_StartDate>
        <ZZT_ZZA_NKSecondTradeGroup>METRO</ZZT_ZZA_NKSecondTradeGroup>
        <ZZT_ZZA_NKTradeGroup>FR06</ZZT_ZZA_NKTradeGroup>
        <ZZT_ZZA_ZZZ_NKSecondDataGrouping>FR</ZZT_ZZA_ZZZ_NKSecondDataGrouping>
      </RefCusApplicability>"));
			Assert.IsTrue(outputFileContent.Contains(@"    <RefCusCondition>
      <ZX1_Comment>Si présentation de l'un des documents ou dispositions tarifaires particulières suivants alors les droits sont suspendus</ZX1_Comment>
      <ZX1_EndDate>2079-06-06T23:59:00</ZX1_EndDate>
      <ZX1_IsExport>False</ZX1_IsExport>
      <ZX1_IsImport>True</ZX1_IsImport>
      <ZX1_StartDate>2021-01-01T00:00:00</ZX1_StartDate>
      <ZX1_ZX2_NKConditionType>AMC</ZX1_ZX2_NKConditionType>
      <RefCusApplicability>
        <ZZT_AdditionalCode>Q031</ZZT_AdditionalCode>
        <ZZT_EndDate>2079-06-06T23:59:00</ZZT_EndDate>
        <ZZT_StartDate>2021-01-01T00:00:00</ZZT_StartDate>
        <ZZT_ZZA_NKSecondTradeGroup>METRO</ZZT_ZZA_NKSecondTradeGroup>
        <ZZT_ZZA_NKTradeGroup>FR06</ZZT_ZZA_NKTradeGroup>
        <ZZT_ZZA_ZZZ_NKSecondDataGrouping>FR</ZZT_ZZA_ZZZ_NKSecondDataGrouping>
      </RefCusApplicability>
      <RefCusConditionValue>
        <ZX3_Value>2001</ZX3_Value>
        <ZX3_ZX4_NKValueType>SUP</ZX3_ZX4_NKValueType>
      </RefCusConditionValue>
      <RefCusConditionValue>
        <ZX3_Value>2003</ZX3_Value>
        <ZX3_ZX4_NKValueType>SUP</ZX3_ZX4_NKValueType>
      </RefCusConditionValue>
      <RefCusConditionValue>
        <ZX3_Value>5005</ZX3_Value>
        <ZX3_ZX4_NKValueType>SUP</ZX3_ZX4_NKValueType>
      </RefCusConditionValue>
      <RefCusConditionValue>
        <ZX3_Value>Sinon le montant à percevoir est égal à 901,84 EUR/Hectolitre d'alcool pur</ZX3_Value>
        <ZX3_ZX4_NKValueType>INF</ZX3_ZX4_NKValueType>
      </RefCusConditionValue>
    </RefCusCondition>"));
			Assert.IsTrue(outputFileContent.Contains(@"<RefCusRate>
      <ZZ2_EndDate>2079-06-06T23:59:00</ZZ2_EndDate>
      <ZZ2_RateFormula>If(has(""CERT"", ""2005"") &amp;&amp; (has(""CERT"", ""2001"") || has(""CERT"", ""2003"") || has(""CERT"", ""5005"")), 0, 901.84 * [005])</ZZ2_RateFormula>
      <ZZ2_RateFormulaDerivedFrom>Si présentation du document 2005 et (présentation du document 2001 ou présentation du document 2003 ou présentation du document 5005) alors le montant à percevoir est 0 sinon le montant à percevoir est 901,84 EUR/Hectolitre d'alcool pur</ZZ2_RateFormulaDerivedFrom>
      <ZZ2_StartDate>2021-01-01T00:00:00</ZZ2_StartDate>
      <ZZ2_ZY1_NKRateCode>L433</ZZ2_ZY1_NKRateCode>
      <ZZ2_ZY1_ZZR_NKRateType>AMC</ZZ2_ZY1_ZZR_NKRateType>
      <RefCusApplicability>
        <ZZT_AdditionalCode>Q031</ZZT_AdditionalCode>
        <ZZT_EndDate>2079-06-06T23:59:00</ZZT_EndDate>
        <ZZT_StartDate>2021-01-01T00:00:00</ZZT_StartDate>
        <ZZT_ZZA_NKSecondTradeGroup>METRO</ZZT_ZZA_NKSecondTradeGroup>
        <ZZT_ZZA_NKTradeGroup>FR06</ZZT_ZZA_NKTradeGroup>
        <ZZT_ZZA_ZZZ_NKSecondDataGrouping>FR</ZZT_ZZA_ZZZ_NKSecondDataGrouping>
      </RefCusApplicability>
      <RefCusRateUOM>
        <ZXG_UOM>005</ZXG_UOM>
      </RefCusRateUOM>
    </RefCusRate>"));
		}

		[Test]
		public void TestOutputFor2203000100()
		{
			var expectedOutputFile = GenerateFileForGivenTariff("2203000100");
			var outputFileContent = File.ReadAllText(expectedOutputFile);
			Assert.IsTrue(outputFileContent.Contains(@"<RefCusRate>
      <ZZ2_EndDate>2079-06-06T23:59:00</ZZ2_EndDate>
      <ZZ2_RateFormula>If(has(""CERT"", ""2001"") || has(""CERT"", ""2003"") || has(""CERT"", ""5005""), 0, 48.87 * [HLT])</ZZ2_RateFormula>
      <ZZ2_RateFormulaDerivedFrom>Si présentation du document 2001 ou présentation du document 2003 ou présentation du document 5005 alors le montant à percevoir est 0 sinon le montant à percevoir est 48,87 EUR/Hectolitre</ZZ2_RateFormulaDerivedFrom>
      <ZZ2_StartDate>2021-01-01T00:00:00</ZZ2_StartDate>
      <ZZ2_ZY1_NKRateCode>L688</ZZ2_ZY1_NKRateCode>
      <ZZ2_ZY1_ZZR_NKRateType>CSS</ZZ2_ZY1_ZZR_NKRateType>
      <RefCusApplicability>
        <ZZT_AdditionalCode>Q261</ZZT_AdditionalCode>
        <ZZT_EndDate>2079-06-06T23:59:00</ZZT_EndDate>
        <ZZT_StartDate>2021-01-01T00:00:00</ZZT_StartDate>
        <ZZT_ZZA_NKSecondTradeGroup>METRO</ZZT_ZZA_NKSecondTradeGroup>
        <ZZT_ZZA_NKTradeGroup>FR01</ZZT_ZZA_NKTradeGroup>
        <ZZT_ZZA_ZZZ_NKSecondDataGrouping>FR</ZZT_ZZA_ZZZ_NKSecondDataGrouping>
      </RefCusApplicability>
      <RefCusRateUOM>
        <ZXG_UOM>HLT</ZXG_UOM>
      </RefCusRateUOM>
    </RefCusRate>"));
			Assert.IsTrue(outputFileContent.Contains(@"<RefCusCondition>
      <ZX1_Comment>Si présentation de l'un des documents ou dispositions tarifaires particulières suivants alors les droits sont suspendus</ZX1_Comment>
      <ZX1_EndDate>2079-06-06T23:59:00</ZX1_EndDate>
      <ZX1_IsExport>False</ZX1_IsExport>
      <ZX1_IsImport>True</ZX1_IsImport>
      <ZX1_StartDate>2021-01-01T00:00:00</ZX1_StartDate>
      <ZX1_ZX2_NKConditionType>CSS</ZX1_ZX2_NKConditionType>
      <RefCusApplicability>
        <ZZT_AdditionalCode>Q261</ZZT_AdditionalCode>
        <ZZT_EndDate>2079-06-06T23:59:00</ZZT_EndDate>
        <ZZT_StartDate>2021-01-01T00:00:00</ZZT_StartDate>
        <ZZT_ZZA_NKSecondTradeGroup>METRO</ZZT_ZZA_NKSecondTradeGroup>
        <ZZT_ZZA_NKTradeGroup>FR01</ZZT_ZZA_NKTradeGroup>
        <ZZT_ZZA_ZZZ_NKSecondDataGrouping>FR</ZZT_ZZA_ZZZ_NKSecondDataGrouping>
      </RefCusApplicability>
      <RefCusConditionValue>
        <ZX3_Value>2001</ZX3_Value>
        <ZX3_ZX4_NKValueType>SUP</ZX3_ZX4_NKValueType>
      </RefCusConditionValue>
      <RefCusConditionValue>
        <ZX3_Value>2003</ZX3_Value>
        <ZX3_ZX4_NKValueType>SUP</ZX3_ZX4_NKValueType>
      </RefCusConditionValue>
      <RefCusConditionValue>
        <ZX3_Value>5005</ZX3_Value>
        <ZX3_ZX4_NKValueType>SUP</ZX3_ZX4_NKValueType>
      </RefCusConditionValue>
      <RefCusConditionValue>
        <ZX3_Value>Sinon le montant à percevoir est égal à 48,87 EUR/Hectolitre</ZX3_Value>
        <ZX3_ZX4_NKValueType>INF</ZX3_ZX4_NKValueType>
      </RefCusConditionValue>
    </RefCusCondition>
    <RefCusCondition>
      <ZX1_Comment>Si présentation de l'un des documents ou dispositions tarifaires particulières suivants alors les droits sont suspendus</ZX1_Comment>
      <ZX1_EndDate>2079-06-06T23:59:00</ZX1_EndDate>
      <ZX1_IsExport>False</ZX1_IsExport>
      <ZX1_IsImport>True</ZX1_IsImport>
      <ZX1_StartDate>2021-01-01T00:00:00</ZX1_StartDate>
      <ZX1_ZX2_NKConditionType>CSS</ZX1_ZX2_NKConditionType>
      <RefCusApplicability>
        <ZZT_AdditionalCode>Q261</ZZT_AdditionalCode>
        <ZZT_EndDate>2079-06-06T23:59:00</ZZT_EndDate>
        <ZZT_StartDate>2021-01-01T00:00:00</ZZT_StartDate>
        <ZZT_ZZA_NKSecondTradeGroup>DPDOM</ZZT_ZZA_NKSecondTradeGroup>
        <ZZT_ZZA_NKTradeGroup>FR01</ZZT_ZZA_NKTradeGroup>
        <ZZT_ZZA_ZZZ_NKSecondDataGrouping>FR</ZZT_ZZA_ZZZ_NKSecondDataGrouping>
      </RefCusApplicability>
      <RefCusConditionValue>
        <ZX3_Value>2001</ZX3_Value>
        <ZX3_ZX4_NKValueType>SUP</ZX3_ZX4_NKValueType>
      </RefCusConditionValue>
      <RefCusConditionValue>
        <ZX3_Value>2003</ZX3_Value>
        <ZX3_ZX4_NKValueType>SUP</ZX3_ZX4_NKValueType>
      </RefCusConditionValue>
      <RefCusConditionValue>
        <ZX3_Value>5005</ZX3_Value>
        <ZX3_ZX4_NKValueType>SUP</ZX3_ZX4_NKValueType>
      </RefCusConditionValue>
      <RefCusConditionValue>
        <ZX3_Value>Sinon le montant à percevoir est égal à 48,87 EUR/Hectolitre</ZX3_Value>
        <ZX3_ZX4_NKValueType>INF</ZX3_ZX4_NKValueType>
      </RefCusConditionValue>
    </RefCusCondition>"));
			Assert.IsFalse(outputFileContent.Contains(@"<ZZ2_ZY1_NKRateCode>N/A</ZZ2_ZY1_NKRateCode>
      <ZZ2_ZY1_ZZR_NKRateType>MSC</ZZ2_ZY1_ZZR_NKRateType>"));
		}

		[Test]
		public void TestOutputFor0304992310()
		{
			var expectedOutputFile = GenerateFileForGivenTariff("0304992310");
			var outputFileContent = File.ReadAllText(expectedOutputFile);
			Assert.IsTrue(outputFileContent.Contains(@"    <RefCusRate>
      <ZZ2_EndDate>2079-06-06T23:59:00</ZZ2_EndDate>
      <ZZ2_RateFormula>If([TNE3] &gt; 100, 2.52 * [TNE3] - 252 * [FLAT] + 610 * [FLAT], MAX(6.1 * [TNE3], 30.49 * [FLAT]))</ZZ2_RateFormula>
      <ZZ2_RateFormulaDerivedFrom>Si la quantité est supérieure à 100 alors le montant à percevoir est 2,52 EUR/Tonne lot - 252 EUR + 610 EUR sinon le montant à percevoir est 6,1 EUR/Tonne lot - Minimum : 30,49 EUR</ZZ2_RateFormulaDerivedFrom>
      <ZZ2_StartDate>2012-04-01T00:00:00</ZZ2_StartDate>
      <ZZ2_ZY1_NKRateCode>G065</ZZ2_ZY1_NKRateCode>
      <ZZ2_ZY1_ZZR_NKRateType>RED</ZZ2_ZY1_ZZR_NKRateType>
      <RefCusApplicability>
        <ZZT_AdditionalCode>Q204</ZZT_AdditionalCode>
        <ZZT_EndDate>2079-06-06T23:59:00</ZZT_EndDate>
        <ZZT_StartDate>2012-04-01T00:00:00</ZZT_StartDate>
        <ZZT_ZZA_NKSecondTradeGroup />
        <ZZT_ZZA_NKTradeGroup>1011</ZZT_ZZA_NKTradeGroup>
        <ZZT_ZZA_ZZZ_NKSecondDataGrouping />
        <RefCusExcludedTradeGroup>
          <ZZC_ZZA_NKTradeGroup>AD</ZZC_ZZA_NKTradeGroup>
        </RefCusExcludedTradeGroup>
        <RefCusExcludedTradeGroup>
          <ZZC_ZZA_NKTradeGroup>CH</ZZC_ZZA_NKTradeGroup>
        </RefCusExcludedTradeGroup>
        <RefCusExcludedTradeGroup>
          <ZZC_ZZA_NKTradeGroup>FO</ZZC_ZZA_NKTradeGroup>
        </RefCusExcludedTradeGroup>
        <RefCusExcludedTradeGroup>
          <ZZC_ZZA_NKTradeGroup>IS</ZZC_ZZA_NKTradeGroup>
        </RefCusExcludedTradeGroup>
        <RefCusExcludedTradeGroup>
          <ZZC_ZZA_NKTradeGroup>LI</ZZC_ZZA_NKTradeGroup>
        </RefCusExcludedTradeGroup>
        <RefCusExcludedTradeGroup>
          <ZZC_ZZA_NKTradeGroup>NO</ZZC_ZZA_NKTradeGroup>
        </RefCusExcludedTradeGroup>
        <RefCusExcludedTradeGroup>
          <ZZC_ZZA_NKTradeGroup>NZ</ZZC_ZZA_NKTradeGroup>
        </RefCusExcludedTradeGroup>
        <RefCusExcludedTradeGroup>
          <ZZC_ZZA_NKTradeGroup>SM</ZZC_ZZA_NKTradeGroup>
        </RefCusExcludedTradeGroup>
      </RefCusApplicability>
      <RefCusRateUOM>
        <ZXG_UOM>TNE3</ZXG_UOM>
      </RefCusRateUOM>
    </RefCusRate>"));
		}

		[Test]
		public void TestOutputFor0101210000()
		{
			var expectedOutputFile = GenerateFileForGivenTariff("0101210000");
			var outputFileContent = File.ReadAllText(expectedOutputFile);
			Assert.IsTrue(outputFileContent.Contains(@"457.35 * [FLAT]"));
			Assert.IsTrue(outputFileContent.Contains(@"<RefCusCondition>
      <ZX1_Comment>L'un des documents ou dispositions tarifaires particulières suivants doit être présent</ZX1_Comment>
      <ZX1_EndDate>2079-06-06T23:59:00</ZX1_EndDate>
      <ZX1_IsExport>False</ZX1_IsExport>
      <ZX1_IsImport>True</ZX1_IsImport>
      <ZX1_StartDate>2018-03-01T00:00:00</ZX1_StartDate>
      <ZX1_ZX2_NKConditionType>CWI</ZX1_ZX2_NKConditionType>
      <RefCusApplicability>
        <ZZT_AdditionalCode />
        <ZZT_EndDate>2079-06-06T23:59:00</ZZT_EndDate>
        <ZZT_StartDate>2018-03-01T00:00:00</ZZT_StartDate>
        <ZZT_ZZA_NKSecondTradeGroup />
        <ZZT_ZZA_NKTradeGroup>1011</ZZT_ZZA_NKTradeGroup>
        <ZZT_ZZA_ZZZ_NKSecondDataGrouping />
      </RefCusApplicability>
      <RefCusConditionValue>
        <ZX3_LogicalORWithinGroup>1</ZX3_LogicalORWithinGroup>
        <ZX3_Value>C638</ZX3_Value>
        <ZX3_ZX4_NKValueType>SUP</ZX3_ZX4_NKValueType>
      </RefCusConditionValue>
      <RefCusConditionValue>
        <ZX3_LogicalORWithinGroup>1</ZX3_LogicalORWithinGroup>
        <ZX3_Value>C639</ZX3_Value>
        <ZX3_ZX4_NKValueType>SUP</ZX3_ZX4_NKValueType>
      </RefCusConditionValue>
      <RefCusConditionValue>
        <ZX3_LogicalORWithinGroup>1</ZX3_LogicalORWithinGroup>
        <ZX3_Value>Y900</ZX3_Value>
        <ZX3_ZX4_NKValueType>SNR</ZX3_ZX4_NKValueType>
      </RefCusConditionValue>
      <RefCusConditionValue>
        <ZX3_LogicalORWithinGroup>2</ZX3_LogicalORWithinGroup>
        <ZX3_Value>C402</ZX3_Value>
        <ZX3_ZX4_NKValueType>SUP</ZX3_ZX4_NKValueType>
      </RefCusConditionValue>
      <RefCusConditionValue>
        <ZX3_LogicalORWithinGroup>2</ZX3_LogicalORWithinGroup>
        <ZX3_Value>2898</ZX3_Value>
        <ZX3_ZX4_NKValueType>SNR</ZX3_ZX4_NKValueType>
      </RefCusConditionValue>
    </RefCusCondition>
    <RefCusCondition>
      <ZX1_Comment>L'un des documents ou dispositions tarifaires particulières suivants doit être présent</ZX1_Comment>
      <ZX1_EndDate>2079-06-06T23:59:00</ZX1_EndDate>
      <ZX1_IsExport>False</ZX1_IsExport>
      <ZX1_IsImport>True</ZX1_IsImport>
      <ZX1_StartDate>2012-01-01T00:00:00</ZX1_StartDate>
      <ZX1_ZX2_NKConditionType>CZO</ZX1_ZX2_NKConditionType>
      <RefCusApplicability>
        <ZZT_AdditionalCode>R063</ZZT_AdditionalCode>
        <ZZT_EndDate>2079-06-06T23:59:00</ZZT_EndDate>
        <ZZT_StartDate>2012-01-01T00:00:00</ZZT_StartDate>
        <ZZT_ZZA_NKSecondTradeGroup />
        <ZZT_ZZA_NKTradeGroup>1011</ZZT_ZZA_NKTradeGroup>
        <ZZT_ZZA_ZZZ_NKSecondDataGrouping />
        <RefCusExcludedTradeGroup>
          <ZZC_ZZA_NKTradeGroup>CH</ZZC_ZZA_NKTradeGroup>
        </RefCusExcludedTradeGroup>
      </RefCusApplicability>
      <RefCusConditionValue>
        <ZX3_Value>2018</ZX3_Value>
        <ZX3_ZX4_NKValueType>SUP</ZX3_ZX4_NKValueType>
      </RefCusConditionValue>
    </RefCusCondition>
    <RefCusCondition>"));
			Assert.IsTrue(outputFileContent.Contains(@"<RefCusCondition>
      <ZX1_Comment>L'un des documents ou dispositions tarifaires particulières suivants doit être présent</ZX1_Comment>
      <ZX1_EndDate>2079-06-06T23:59:00</ZX1_EndDate>
      <ZX1_IsExport>True</ZX1_IsExport>
      <ZX1_IsImport>False</ZX1_IsImport>
      <ZX1_StartDate>2018-03-01T00:00:00</ZX1_StartDate>
      <ZX1_ZX2_NKConditionType>CWE</ZX1_ZX2_NKConditionType>
      <RefCusApplicability>
        <ZZT_AdditionalCode />
        <ZZT_EndDate>2079-06-06T23:59:00</ZZT_EndDate>
        <ZZT_StartDate>2018-03-01T00:00:00</ZZT_StartDate>
        <ZZT_ZZA_NKSecondTradeGroup />
        <ZZT_ZZA_NKTradeGroup>1008</ZZT_ZZA_NKTradeGroup>
        <ZZT_ZZA_ZZZ_NKSecondDataGrouping />
      </RefCusApplicability>
      <RefCusConditionValue>
        <ZX3_Value>C401</ZX3_Value>
        <ZX3_ZX4_NKValueType>SUP</ZX3_ZX4_NKValueType>
      </RefCusConditionValue>
      <RefCusConditionValue>
        <ZX3_Value>Y900</ZX3_Value>
        <ZX3_ZX4_NKValueType>SNR</ZX3_ZX4_NKValueType>
      </RefCusConditionValue>
    </RefCusCondition>"));
			Assert.IsTrue(outputFileContent.Contains(@"<RefCusCondition>
      <ZX1_Comment>Aucun document requis</ZX1_Comment>
      <ZX1_EndDate>2079-06-06T23:59:00</ZX1_EndDate>
      <ZX1_IsExport>False</ZX1_IsExport>
      <ZX1_IsImport>True</ZX1_IsImport>
      <ZX1_StartDate>2012-01-01T00:00:00</ZX1_StartDate>
      <ZX1_ZX2_NKConditionType>CZO</ZX1_ZX2_NKConditionType>
      <RefCusApplicability>
        <ZZT_AdditionalCode>R065</ZZT_AdditionalCode>
        <ZZT_EndDate>2079-06-06T23:59:00</ZZT_EndDate>
        <ZZT_StartDate>2012-01-01T00:00:00</ZZT_StartDate>
        <ZZT_ZZA_NKSecondTradeGroup />
        <ZZT_ZZA_NKTradeGroup>1011</ZZT_ZZA_NKTradeGroup>
        <ZZT_ZZA_ZZZ_NKSecondDataGrouping />
        <RefCusExcludedTradeGroup>
          <ZZC_ZZA_NKTradeGroup>CH</ZZC_ZZA_NKTradeGroup>
        </RefCusExcludedTradeGroup>
      </RefCusApplicability>
    </RefCusCondition>"));
		}

		[Test]
		public void TestOutputFor2402100000()
		{
			var expectedOutputFile = GenerateFileForGivenTariff("2402100000");
			var outputFileContent = File.ReadAllText(expectedOutputFile);
			Assert.IsTrue(outputFileContent.Contains(@"<RefCusCondition>
      <ZX1_Comment>L'un des documents ou dispositions tarifaires particulières suivants doit être présent</ZX1_Comment>
      <ZX1_EndDate>2079-06-06T23:59:00</ZX1_EndDate>
      <ZX1_IsExport>False</ZX1_IsExport>
      <ZX1_IsImport>True</ZX1_IsImport>
      <ZX1_StartDate>2017-01-01T00:00:00</ZX1_StartDate>
      <ZX1_ZX2_NKConditionType>TMC</ZX1_ZX2_NKConditionType>
      <RefCusApplicability>
        <ZZT_AdditionalCode />
        <ZZT_EndDate>2079-06-06T23:59:00</ZZT_EndDate>
        <ZZT_StartDate>2017-01-01T00:00:00</ZZT_StartDate>
        <ZZT_ZZA_NKSecondTradeGroup>METRO</ZZT_ZZA_NKSecondTradeGroup>
        <ZZT_ZZA_NKTradeGroup>FR01</ZZT_ZZA_NKTradeGroup>
        <ZZT_ZZA_ZZZ_NKSecondDataGrouping>FR</ZZT_ZZA_ZZZ_NKSecondDataGrouping>
      </RefCusApplicability>
      <RefCusConditionValue>
        <ZX3_Value>2001</ZX3_Value>
        <ZX3_ZX4_NKValueType>SUP</ZX3_ZX4_NKValueType>
      </RefCusConditionValue>
      <RefCusConditionValue>
        <ZX3_Value>2003</ZX3_Value>
        <ZX3_ZX4_NKValueType>SUP</ZX3_ZX4_NKValueType>
      </RefCusConditionValue>
      <RefCusConditionValue>
        <ZX3_Value>2002</ZX3_Value>
        <ZX3_ZX4_NKValueType>SUP</ZX3_ZX4_NKValueType>
      </RefCusConditionValue>
    </RefCusCondition>"));
			Assert.IsTrue(outputFileContent.Contains(@"<RefCusRate>
      <ZZ2_EndDate>2079-06-06T23:59:00</ZZ2_EndDate>
      <ZZ2_RateFormula>If(has(""CERT"", ""2001"") || has(""CERT"", ""2003"") || has(""CERT"", ""2002""), {""Precalcule""})</ZZ2_RateFormula>
      <ZZ2_RateFormulaDerivedFrom>Si présentation du document 2001 ou présentation du document 2003 ou présentation du document 2002 alors le montant à percevoir est précalculé sinon la mise à la consommation est interdite</ZZ2_RateFormulaDerivedFrom>
      <ZZ2_StartDate>2017-01-01T00:00:00</ZZ2_StartDate>
      <ZZ2_ZY1_NKRateCode>C495</ZZ2_ZY1_NKRateCode>
      <ZZ2_ZY1_ZZR_NKRateType>TMC</ZZ2_ZY1_ZZR_NKRateType>
      <RefCusApplicability>
        <ZZT_AdditionalCode />
        <ZZT_EndDate>2079-06-06T23:59:00</ZZT_EndDate>
        <ZZT_StartDate>2017-01-01T00:00:00</ZZT_StartDate>
        <ZZT_ZZA_NKSecondTradeGroup>METRO</ZZT_ZZA_NKSecondTradeGroup>
        <ZZT_ZZA_NKTradeGroup>FR01</ZZT_ZZA_NKTradeGroup>
        <ZZT_ZZA_ZZZ_NKSecondDataGrouping>FR</ZZT_ZZA_ZZZ_NKSecondDataGrouping>
      </RefCusApplicability>
    </RefCusRate>"));
		}

		[Test]
		public void TestOutputFor6404209000()
		{
			var expectedOutputFile = GenerateFileForGivenTariff("6404209000");
			var outputFileContent = File.ReadAllText(expectedOutputFile);
			Assert.IsTrue(outputFileContent.Contains(@"<RefCusCondition>
      <ZX1_Comment>L'un des documents ou dispositions tarifaires particulières suivants doit être présent</ZX1_Comment>
      <ZX1_EndDate>2079-06-06T23:59:00</ZX1_EndDate>
      <ZX1_IsExport>False</ZX1_IsExport>
      <ZX1_IsImport>True</ZX1_IsImport>
      <ZX1_StartDate>2018-03-01T00:00:00</ZX1_StartDate>
      <ZX1_ZX2_NKConditionType>CWI</ZX1_ZX2_NKConditionType>
      <RefCusApplicability>
        <ZZT_AdditionalCode />
        <ZZT_EndDate>2079-06-06T23:59:00</ZZT_EndDate>
        <ZZT_StartDate>2018-03-01T00:00:00</ZZT_StartDate>
        <ZZT_ZZA_NKSecondTradeGroup />
        <ZZT_ZZA_NKTradeGroup>1011</ZZT_ZZA_NKTradeGroup>
        <ZZT_ZZA_ZZZ_NKSecondDataGrouping />
      </RefCusApplicability>
      <RefCusConditionValue>
        <ZX3_LogicalORWithinGroup>1</ZX3_LogicalORWithinGroup>
        <ZX3_Value>C638</ZX3_Value>
        <ZX3_ZX4_NKValueType>SUP</ZX3_ZX4_NKValueType>
      </RefCusConditionValue>
      <RefCusConditionValue>
        <ZX3_LogicalORWithinGroup>1</ZX3_LogicalORWithinGroup>
        <ZX3_Value>C639</ZX3_Value>
        <ZX3_ZX4_NKValueType>SUP</ZX3_ZX4_NKValueType>
      </RefCusConditionValue>
      <RefCusConditionValue>
        <ZX3_LogicalORWithinGroup>1</ZX3_LogicalORWithinGroup>
        <ZX3_Value>Y900</ZX3_Value>
        <ZX3_ZX4_NKValueType>SNR</ZX3_ZX4_NKValueType>
      </RefCusConditionValue>
      <RefCusConditionValue>
        <ZX3_LogicalORWithinGroup>2</ZX3_LogicalORWithinGroup>
        <ZX3_Value>C402</ZX3_Value>
        <ZX3_ZX4_NKValueType>SUP</ZX3_ZX4_NKValueType>
      </RefCusConditionValue>
      <RefCusConditionValue>
        <ZX3_LogicalORWithinGroup>2</ZX3_LogicalORWithinGroup>
        <ZX3_Value>2898</ZX3_Value>
        <ZX3_ZX4_NKValueType>SNR</ZX3_ZX4_NKValueType>
      </RefCusConditionValue>
    </RefCusCondition>"));
			Assert.IsTrue(outputFileContent.Contains(@"<RefCusCondition>
      <ZX1_Comment>L'un des documents ou dispositions tarifaires particulières suivants doit être présent</ZX1_Comment>
      <ZX1_EndDate>2079-06-06T23:59:00</ZX1_EndDate>
      <ZX1_IsExport>True</ZX1_IsExport>
      <ZX1_IsImport>False</ZX1_IsImport>
      <ZX1_StartDate>2018-03-01T00:00:00</ZX1_StartDate>
      <ZX1_ZX2_NKConditionType>CWE</ZX1_ZX2_NKConditionType>
      <RefCusApplicability>
        <ZZT_AdditionalCode />
        <ZZT_EndDate>2079-06-06T23:59:00</ZZT_EndDate>
        <ZZT_StartDate>2018-03-01T00:00:00</ZZT_StartDate>
        <ZZT_ZZA_NKSecondTradeGroup />
        <ZZT_ZZA_NKTradeGroup>1008</ZZT_ZZA_NKTradeGroup>
        <ZZT_ZZA_ZZZ_NKSecondDataGrouping />
      </RefCusApplicability>
      <RefCusConditionValue>
        <ZX3_Value>C401</ZX3_Value>
        <ZX3_ZX4_NKValueType>SUP</ZX3_ZX4_NKValueType>
      </RefCusConditionValue>
      <RefCusConditionValue>
        <ZX3_Value>Y900</ZX3_Value>
        <ZX3_ZX4_NKValueType>SNR</ZX3_ZX4_NKValueType>
      </RefCusConditionValue>
    </RefCusCondition>"), "Conditions for export should be included");
			Assert.IsTrue(outputFileContent.Contains(@"<RefCusTariffUOM>
      <ZZ8_EndDate>2079-06-06T23:59:00</ZZ8_EndDate>
      <ZZ8_StartDate>2008-01-01T00:00:00</ZZ8_StartDate>
      <ZZ8_Type>CU2</ZZ8_Type>
      <ZZ8_UOM>NPR</ZZ8_UOM>
      <ZZ8_ZZA_NKSecondTradeGroup />
      <ZZ8_ZZA_NKTradeGroup>FR04</ZZ8_ZZA_NKTradeGroup>
      <ZZ8_ZZA_ZZZ_NKSecondDataGrouping />
    </RefCusTariffUOM>"));
		}

		[Test]
		public void TestOutputFor3006400000()
		{
			var expectedOutputFile = GenerateFileForGivenTariff("3006400000");
			var outputFileContent = File.ReadAllText(expectedOutputFile);
			Assert.IsTrue(outputFileContent.Contains(@"<RefCusRate>
      <ZZ2_EndDate>2079-06-06T23:59:00</ZZ2_EndDate>
      <ZZ2_RateFormula>VFD * 0.07</ZZ2_RateFormula>
      <ZZ2_RateFormulaDerivedFrom>7 %</ZZ2_RateFormulaDerivedFrom>
      <ZZ2_StartDate>2007-01-01T00:00:00</ZZ2_StartDate>
      <ZZ2_ZY1_NKRateCode>V395</ZZ2_ZY1_NKRateCode>
      <ZZ2_ZY1_ZZR_NKRateType>OME</ZZ2_ZY1_ZZR_NKRateType>
      <RefCusApplicability>
        <ZZT_AdditionalCode />
        <ZZT_EndDate>2079-06-06T23:59:00</ZZT_EndDate>
        <ZZT_StartDate>2007-01-01T00:00:00</ZZT_StartDate>
        <ZZT_ZZA_NKSecondTradeGroup>MARTI</ZZT_ZZA_NKSecondTradeGroup>
        <ZZT_ZZA_NKTradeGroup>FR01</ZZT_ZZA_NKTradeGroup>
        <ZZT_ZZA_ZZZ_NKSecondDataGrouping>FR</ZZT_ZZA_ZZZ_NKSecondDataGrouping>
        <RefCusExcludedTradeGroup>
          <ZZC_ZZA_NKTradeGroup>GF</ZZC_ZZA_NKTradeGroup>
        </RefCusExcludedTradeGroup>
        <RefCusExcludedTradeGroup>
          <ZZC_ZZA_NKTradeGroup>GP</ZZC_ZZA_NKTradeGroup>
        </RefCusExcludedTradeGroup>
      </RefCusApplicability>
    </RefCusRate>"));
			Assert.IsTrue(outputFileContent.Contains(@"<RefCusRate>
      <ZZ2_EndDate>2079-06-06T23:59:00</ZZ2_EndDate>
      <ZZ2_RateFormula>0</ZZ2_RateFormula>
      <ZZ2_RateFormulaDerivedFrom>0</ZZ2_RateFormulaDerivedFrom>
      <ZZ2_StartDate>2015-07-01T00:00:00</ZZ2_StartDate>
      <ZZ2_ZY1_NKRateCode>V395</ZZ2_ZY1_NKRateCode>
      <ZZ2_ZY1_ZZR_NKRateType>OME</ZZ2_ZY1_ZZR_NKRateType>
      <RefCusApplicability>
        <ZZT_AdditionalCode>Z910</ZZT_AdditionalCode>
        <ZZT_EndDate>2079-06-06T23:59:00</ZZT_EndDate>
        <ZZT_StartDate>2015-07-01T00:00:00</ZZT_StartDate>
        <ZZT_ZZA_NKSecondTradeGroup>MARTI</ZZT_ZZA_NKSecondTradeGroup>
        <ZZT_ZZA_NKTradeGroup>FR01</ZZT_ZZA_NKTradeGroup>
        <ZZT_ZZA_ZZZ_NKSecondDataGrouping>FR</ZZT_ZZA_ZZZ_NKSecondDataGrouping>
      </RefCusApplicability>
    </RefCusRate>"));
			Assert.IsTrue(outputFileContent.Contains(@"<RefCusRate>
      <ZZ2_EndDate>2079-06-06T23:59:00</ZZ2_EndDate>
      <ZZ2_RateFormula>VFD * 0.03</ZZ2_RateFormula>
      <ZZ2_RateFormulaDerivedFrom>3 %</ZZ2_RateFormulaDerivedFrom>
      <ZZ2_StartDate>2021-02-01T00:00:00</ZZ2_StartDate>
      <ZZ2_ZY1_NKRateCode>K947</ZZ2_ZY1_NKRateCode>
      <ZZ2_ZY1_ZZR_NKRateType>OMR</ZZ2_ZY1_ZZR_NKRateType>
      <RefCusApplicability>
        <ZZT_AdditionalCode />
        <ZZT_EndDate>2079-06-06T23:59:00</ZZT_EndDate>
        <ZZT_StartDate>2021-02-01T00:00:00</ZZT_StartDate>
        <ZZT_ZZA_NKSecondTradeGroup>GUYAN</ZZT_ZZA_NKSecondTradeGroup>
        <ZZT_ZZA_NKTradeGroup>FR01</ZZT_ZZA_NKTradeGroup>
        <ZZT_ZZA_ZZZ_NKSecondDataGrouping>FR</ZZT_ZZA_ZZZ_NKSecondDataGrouping>
        <RefCusExcludedTradeGroup>
          <ZZC_ZZA_NKTradeGroup>GP</ZZC_ZZA_NKTradeGroup>
        </RefCusExcludedTradeGroup>
        <RefCusExcludedTradeGroup>
          <ZZC_ZZA_NKTradeGroup>MQ</ZZC_ZZA_NKTradeGroup>
        </RefCusExcludedTradeGroup>
      </RefCusApplicability>
    </RefCusRate>"));
			Assert.IsTrue(outputFileContent.Contains(@"<RefCusRate>
      <ZZ2_EndDate>2025-12-31T00:00:00</ZZ2_EndDate>
      <ZZ2_RateFormula>0</ZZ2_RateFormula>
      <ZZ2_RateFormulaDerivedFrom>0</ZZ2_RateFormulaDerivedFrom>
      <ZZ2_StartDate>2020-04-17T00:00:00</ZZ2_StartDate>
      <ZZ2_ZY1_NKRateCode>K943</ZZ2_ZY1_NKRateCode>
      <ZZ2_ZY1_ZZR_NKRateType>OMR</ZZ2_ZY1_ZZR_NKRateType>
      <RefCusApplicability>
        <ZZT_AdditionalCode>Z915</ZZT_AdditionalCode>
        <ZZT_EndDate>2025-12-31T23:59:00</ZZT_EndDate>
        <ZZT_StartDate>2020-04-17T00:00:00</ZZT_StartDate>
        <ZZT_ZZA_NKSecondTradeGroup>GUYAN</ZZT_ZZA_NKSecondTradeGroup>
        <ZZT_ZZA_NKTradeGroup>FR01</ZZT_ZZA_NKTradeGroup>
        <ZZT_ZZA_ZZZ_NKSecondDataGrouping>FR</ZZT_ZZA_ZZZ_NKSecondDataGrouping>
      </RefCusApplicability>
    </RefCusRate>"));
			Assert.IsTrue(outputFileContent.Contains(@"<RefCusCondition>
      <ZX1_Comment>Le document ou disposition tarifaire particulière suivant doit être présent</ZX1_Comment>
      <ZX1_EndDate>2079-06-06T23:59:00</ZX1_EndDate>
      <ZX1_IsExport>False</ZX1_IsExport>
      <ZX1_IsImport>True</ZX1_IsImport>
      <ZX1_StartDate>2021-02-01T00:00:00</ZX1_StartDate>
      <ZX1_ZX2_NKConditionType>OEA</ZX1_ZX2_NKConditionType>
      <RefCusApplicability>
        <ZZT_AdditionalCode />
        <ZZT_EndDate>2079-06-06T23:59:00</ZZT_EndDate>
        <ZZT_StartDate>2021-02-01T00:00:00</ZZT_StartDate>
        <ZZT_ZZA_NKSecondTradeGroup>GUYAN</ZZT_ZZA_NKSecondTradeGroup>
        <ZZT_ZZA_NKTradeGroup>FR09</ZZT_ZZA_NKTradeGroup>
        <ZZT_ZZA_ZZZ_NKSecondDataGrouping>FR</ZZT_ZZA_ZZZ_NKSecondDataGrouping>
      </RefCusApplicability>
      <RefCusConditionValue>
        <ZX3_Value>4800</ZX3_Value>
        <ZX3_ZX4_NKValueType>SNR</ZX3_ZX4_NKValueType>
      </RefCusConditionValue>
    </RefCusCondition>"));
			Assert.IsTrue(outputFileContent.Contains(@"<RefCusCondition>
      <ZX1_Comment>Si présentation de l'un des documents ou dispositions tarifaires particulières suivants alors les droits sont suspendus</ZX1_Comment>
      <ZX1_EndDate>2079-06-06T23:59:00</ZX1_EndDate>
      <ZX1_IsExport>False</ZX1_IsExport>
      <ZX1_IsImport>True</ZX1_IsImport>
      <ZX1_StartDate>2017-01-02T00:00:00</ZX1_StartDate>
      <ZX1_ZX2_NKConditionType>OEB</ZX1_ZX2_NKConditionType>
      <RefCusApplicability>
        <ZZT_AdditionalCode>Z916</ZZT_AdditionalCode>
        <ZZT_EndDate>2079-06-06T23:59:00</ZZT_EndDate>
        <ZZT_StartDate>2017-01-02T00:00:00</ZZT_StartDate>
        <ZZT_ZZA_NKSecondTradeGroup>MARTI</ZZT_ZZA_NKSecondTradeGroup>
        <ZZT_ZZA_NKTradeGroup>FR01</ZZT_ZZA_NKTradeGroup>
        <ZZT_ZZA_ZZZ_NKSecondDataGrouping>FR</ZZT_ZZA_ZZZ_NKSecondDataGrouping>
      </RefCusApplicability>
      <RefCusConditionValue>
        <ZX3_Value>4502</ZX3_Value>
        <ZX3_ZX4_NKValueType>SUP</ZX3_ZX4_NKValueType>
      </RefCusConditionValue>
    </RefCusCondition>"));
			Assert.IsTrue(outputFileContent.Contains(@"<RefCusCondition>
      <ZX1_Comment>Le document ou disposition tarifaire particulière suivant doit être présent</ZX1_Comment>
      <ZX1_EndDate>2079-06-06T23:59:00</ZX1_EndDate>
      <ZX1_IsExport>False</ZX1_IsExport>
      <ZX1_IsImport>True</ZX1_IsImport>
      <ZX1_StartDate>2021-02-01T00:00:00</ZX1_StartDate>
      <ZX1_ZX2_NKConditionType>ORA</ZX1_ZX2_NKConditionType>
      <RefCusApplicability>
        <ZZT_AdditionalCode />
        <ZZT_EndDate>2079-06-06T23:59:00</ZZT_EndDate>
        <ZZT_StartDate>2021-02-01T00:00:00</ZZT_StartDate>
        <ZZT_ZZA_NKSecondTradeGroup>GUYAN</ZZT_ZZA_NKSecondTradeGroup>
        <ZZT_ZZA_NKTradeGroup>FR09</ZZT_ZZA_NKTradeGroup>
        <ZZT_ZZA_ZZZ_NKSecondDataGrouping>FR</ZZT_ZZA_ZZZ_NKSecondDataGrouping>
      </RefCusApplicability>
      <RefCusConditionValue>
        <ZX3_Value>4800</ZX3_Value>
        <ZX3_ZX4_NKValueType>SNR</ZX3_ZX4_NKValueType>
      </RefCusConditionValue>
    </RefCusCondition>"));
			Assert.IsTrue(outputFileContent.Contains(@"<RefCusCondition>
      <ZX1_Comment>Si présentation de l'un des documents ou dispositions tarifaires particulières suivants alors les droits sont suspendus</ZX1_Comment>
      <ZX1_EndDate>2079-06-06T23:59:00</ZX1_EndDate>
      <ZX1_IsExport>False</ZX1_IsExport>
      <ZX1_IsImport>True</ZX1_IsImport>
      <ZX1_StartDate>2017-01-02T00:00:00</ZX1_StartDate>
      <ZX1_ZX2_NKConditionType>ORB</ZX1_ZX2_NKConditionType>
      <RefCusApplicability>
        <ZZT_AdditionalCode>Z916</ZZT_AdditionalCode>
        <ZZT_EndDate>2079-06-06T23:59:00</ZZT_EndDate>
        <ZZT_StartDate>2017-01-02T00:00:00</ZZT_StartDate>
        <ZZT_ZZA_NKSecondTradeGroup>MARTI</ZZT_ZZA_NKSecondTradeGroup>
        <ZZT_ZZA_NKTradeGroup>FR01</ZZT_ZZA_NKTradeGroup>
        <ZZT_ZZA_ZZZ_NKSecondDataGrouping>FR</ZZT_ZZA_ZZZ_NKSecondDataGrouping>
      </RefCusApplicability>
      <RefCusConditionValue>
        <ZX3_Value>4502</ZX3_Value>
        <ZX3_ZX4_NKValueType>SUP</ZX3_ZX4_NKValueType>
      </RefCusConditionValue>
    </RefCusCondition>"));
			Assert.IsTrue(outputFileContent.Contains(@"    <RefCusCondition>
      <ZX1_Comment>Tous les documents ou dispositions tarifaires particulières suivants sont présents</ZX1_Comment>
      <ZX1_EndDate>2079-06-06T23:59:00</ZX1_EndDate>
      <ZX1_IsExport>False</ZX1_IsExport>
      <ZX1_IsImport>True</ZX1_IsImport>
      <ZX1_StartDate>2019-12-11T00:00:00</ZX1_StartDate>
      <ZX1_ZX2_NKConditionType>OEB</ZX1_ZX2_NKConditionType>
      <RefCusApplicability>
        <ZZT_AdditionalCode>Z917</ZZT_AdditionalCode>
        <ZZT_EndDate>2079-06-06T23:59:00</ZZT_EndDate>
        <ZZT_StartDate>2019-12-11T00:00:00</ZZT_StartDate>
        <ZZT_ZZA_NKSecondTradeGroup>MAYOT</ZZT_ZZA_NKSecondTradeGroup>
        <ZZT_ZZA_NKTradeGroup>FR01</ZZT_ZZA_NKTradeGroup>
        <ZZT_ZZA_ZZZ_NKSecondDataGrouping>FR</ZZT_ZZA_ZZZ_NKSecondDataGrouping>
      </RefCusApplicability>
      <RefCusConditionValue>
        <ZX3_Value>4502</ZX3_Value>
        <ZX3_ZX4_NKValueType>SUP</ZX3_ZX4_NKValueType>
      </RefCusConditionValue>
    </RefCusCondition>"));
		}

		[Test]
		public void TestOutputFor2204214210()
		{
			var expectedOutputFile = GenerateFileForGivenTariff("2204214210");
			var outputFileContent = File.ReadAllText(expectedOutputFile);
			Assert.IsTrue(outputFileContent.Contains(@"<RefCusTariffAdditionalCode>
      <ZY2_AdditionalCode>S156</ZY2_AdditionalCode>
      <ZY2_Description>Bordeaux rosé et Clairet</ZY2_Description>
      <ZY2_ZY3_NKCategory>SIP</ZY2_ZY3_NKCategory>
      <RefCusApplicability>
        <ZZT_AdditionalCode />
        <ZZT_EndDate>2079-06-06T23:59:00</ZZT_EndDate>
        <ZZT_StartDate>2010-01-01T00:00:00</ZZT_StartDate>
        <ZZT_ZZA_NKSecondTradeGroup />
        <ZZT_ZZA_NKTradeGroup>FR01</ZZT_ZZA_NKTradeGroup>
        <ZZT_ZZA_ZZZ_NKSecondDataGrouping />
      </RefCusApplicability>
    </RefCusTariffAdditionalCode>"));
			Assert.IsTrue(outputFileContent.Contains(@"
    <RefCusTariffAdditionalCode>
      <ZY2_AdditionalCode>S156</ZY2_AdditionalCode>
      <ZY2_Description>Bordeaux rosé et Clairet</ZY2_Description>
      <ZY2_ZY3_NKCategory>SEP</ZY2_ZY3_NKCategory>
      <RefCusApplicability>
        <ZZT_AdditionalCode />
        <ZZT_EndDate>2079-06-06T23:59:00</ZZT_EndDate>
        <ZZT_StartDate>2010-01-01T00:00:00</ZZT_StartDate>
        <ZZT_ZZA_NKSecondTradeGroup />
        <ZZT_ZZA_NKTradeGroup>FR01</ZZT_ZZA_NKTradeGroup>
        <ZZT_ZZA_ZZZ_NKSecondDataGrouping />
      </RefCusApplicability>
    </RefCusTariffAdditionalCode>"));
		}

		[Test]
		public void TestPPHUMIMeasuresCollected()
		{
			var expectedOutputFile = GenerateFileForGivenTariff("3002901000");
			var outputFileContent = File.ReadAllText(expectedOutputFile);
			Assert.IsTrue(outputFileContent.Contains(@"<RefCusCondition>
      <ZX1_Comment>L'un des documents ou dispositions tarifaires particulières suivants doit être présent</ZX1_Comment>
      <ZX1_EndDate>2079-06-06T23:59:00</ZX1_EndDate>
      <ZX1_IsExport>False</ZX1_IsExport>
      <ZX1_IsImport>True</ZX1_IsImport>
      <ZX1_StartDate>2023-10-11T00:00:00</ZX1_StartDate>
      <ZX1_ZX2_NKConditionType>PPHUMI</ZX1_ZX2_NKConditionType>
      <RefCusApplicability>
        <ZZT_AdditionalCode />
        <ZZT_EndDate>2079-06-06T23:59:00</ZZT_EndDate>
        <ZZT_StartDate>2023-10-11T00:00:00</ZZT_StartDate>
        <ZZT_ZZA_NKSecondTradeGroup />
        <ZZT_ZZA_NKTradeGroup>1011</ZZT_ZZA_NKTradeGroup>
        <ZZT_ZZA_ZZZ_NKSecondDataGrouping />
      </RefCusApplicability>
      <RefCusConditionValue>
        <ZX3_Value>2440</ZX3_Value>
        <ZX3_ZX4_NKValueType>SUP</ZX3_ZX4_NKValueType>
      </RefCusConditionValue>
      <RefCusConditionValue>
        <ZX3_Value>2441</ZX3_Value>
        <ZX3_ZX4_NKValueType>SUP</ZX3_ZX4_NKValueType>
      </RefCusConditionValue>
      <RefCusConditionValue>
        <ZX3_Value>2442</ZX3_Value>
        <ZX3_ZX4_NKValueType>SUP</ZX3_ZX4_NKValueType>
      </RefCusConditionValue>
      <RefCusConditionValue>
        <ZX3_Value>2887</ZX3_Value>
        <ZX3_ZX4_NKValueType>SNR</ZX3_ZX4_NKValueType>
      </RefCusConditionValue>
    </RefCusCondition>"));
		}

		[Test]
		public void TestPPHUMEMeasuresCollected()
		{
			var expectedOutputFile = GenerateFileForGivenTariff("3002901000");
			var outputFileContent = File.ReadAllText(expectedOutputFile);
			Assert.IsTrue(outputFileContent.Contains(@"<RefCusCondition>
      <ZX1_Comment>L'un des documents ou dispositions tarifaires particulières suivants doit être présent</ZX1_Comment>
      <ZX1_EndDate>2079-06-06T23:59:00</ZX1_EndDate>
      <ZX1_IsExport>True</ZX1_IsExport>
      <ZX1_IsImport>False</ZX1_IsImport>
      <ZX1_StartDate>2023-10-11T00:00:00</ZX1_StartDate>
      <ZX1_ZX2_NKConditionType>PPHUME</ZX1_ZX2_NKConditionType>
      <RefCusApplicability>
        <ZZT_AdditionalCode />
        <ZZT_EndDate>2079-06-06T23:59:00</ZZT_EndDate>
        <ZZT_StartDate>2023-10-11T00:00:00</ZZT_StartDate>
        <ZZT_ZZA_NKSecondTradeGroup />
        <ZZT_ZZA_NKTradeGroup>1011</ZZT_ZZA_NKTradeGroup>
        <ZZT_ZZA_ZZZ_NKSecondDataGrouping />
      </RefCusApplicability>
      <RefCusConditionValue>
        <ZX3_Value>2440</ZX3_Value>
        <ZX3_ZX4_NKValueType>SUP</ZX3_ZX4_NKValueType>
      </RefCusConditionValue>
      <RefCusConditionValue>
        <ZX3_Value>2441</ZX3_Value>
        <ZX3_ZX4_NKValueType>SUP</ZX3_ZX4_NKValueType>
      </RefCusConditionValue>
      <RefCusConditionValue>
        <ZX3_Value>2442</ZX3_Value>
        <ZX3_ZX4_NKValueType>SUP</ZX3_ZX4_NKValueType>
      </RefCusConditionValue>
      <RefCusConditionValue>
        <ZX3_Value>2887</ZX3_Value>
        <ZX3_ZX4_NKValueType>SNR</ZX3_ZX4_NKValueType>
      </RefCusConditionValue>
    </RefCusCondition>"));
		}
	}
}
