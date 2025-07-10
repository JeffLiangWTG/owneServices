using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.RefDbRepo.FRReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.FRReferenceData.Tests.Generators
{
	[TestFixture]
	class CommonTariffUniversalReferenceDataFileGeneratorTest
	{
		[Test]
		public void CheckOutputFilesForPartialUpdate()
		{
			string[] tariffCodeList = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\", "TariffList.txt")).Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
			ApplicationConfig.Instance.DownloadDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\");
			ApplicationConfig.Instance.OutputDirectory = Path.GetTempPath();
			var expectedOutputFile = Path.Combine(ApplicationConfig.Instance.OutputDirectory, "FR_8DigitsExportTariffData_Update_20220413.xml");
			File.Delete(expectedOutputFile);

			var generator = new CommonTariffUniversalReferenceDataFileGenerator();
			var outputFileCount = generator.GenerateURDFiles(tariffCodeList, Common.UniversalXmlWriter.UpdateType.Partial, new DateTime(2022, 04, 13));

			Assert.AreEqual(1, outputFileCount);
			Assert.IsTrue(File.Exists(expectedOutputFile));
			var outputFileContent = File.ReadAllText(expectedOutputFile);
			Assert.That(outputFileContent.Contains("<DataSource>FR Tariff (8 digits Export)</DataSource>"));
			var tariffCountInFirstOutputFile = (outputFileContent.Length - outputFileContent.Replace("<RefCusTariff>", "").Length) / 14;
			Assert.AreEqual(6, tariffCountInFirstOutputFile);
			Assert.True(outputFileContent.Contains("<ZZ1_TariffCode>01012100</ZZ1_TariffCode>"));
			Assert.True(outputFileContent.Contains("<ZZ1_TariffCode>22030001</ZZ1_TariffCode>"));
		}

		[Test]
		public void GenerateFilesForTariffs()
		{
			var expectedOutputFile = GenerateFileForGivenTariff("6404209000");

			Assert.IsTrue(File.Exists(expectedOutputFile));
			var outputFileContent = File.ReadAllText(expectedOutputFile);
			Assert.IsTrue(outputFileContent.Contains("<Schema>"));
			Assert.IsTrue(outputFileContent.Contains("<DataSource>FR Tariff (8 digits Export) chapter 64</DataSource>"));
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
      <Property Name=""RefCusTariffAdditionalCode"" Type=""RefCusTariffAdditionalCode"" />
      <Property Name=""RefCusTariffUOM"" Type=""RefCusTariffUOM"" />
      <Property Name=""ZZ1_TariffCode"" Type=""varchar"" MaxLength=""35"" />
      <Property Name=""ZZ1_ZZI_NKTariffType"" Type=""varchar"" MaxLength=""5"" ConstantValue=""EXP"" />
      <Property Name=""ZZ1_ZZI_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""3"" ConstantValue=""EUN"" />
      <Property Name=""ZZ1_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""3"" ConstantValue=""EUN"" />
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
      <Property Name=""ZX1_IsExport"" Type=""bit"" ConstantValue=""True"" />
      <Property Name=""ZX1_IsImport"" Type=""bit"" ConstantValue=""False"" />
      <Property Name=""ZX1_LogicalANDWithinGroup"" Type=""tinyint"" ConstantValue=""0"" />
      <Property Name=""ZX1_Source"" Type=""nvarchar"" ConstantValue=""RITA"" />
      <Property Name=""ZX1_StartDate"" Type=""smalldatetime"" />
      <Property Name=""ZX1_ZX2_NKConditionType"" Type=""varchar"" MaxLength=""6"" />
      <Property Name=""ZX1_ZX2_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""3"" ConstantValue=""FR"" />
      <Property Name=""ZX1_ZZZ_NKDataGrouping"" Type=""varchar"" MaxLength=""3"" ConstantValue=""FR"" />
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
		}
		public string GenerateFileForGivenTariff(string tariffCode)
		{
			var tariffCodeList = new List<string> { tariffCode };
			ApplicationConfig.Instance.DownloadDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\");
			ApplicationConfig.Instance.OutputDirectory = Path.GetTempPath();

			var outputFilePath = Path.Combine(ApplicationConfig.Instance.OutputDirectory, $"FR_8DigitsExportTariffData_Chapter{tariffCode.Substring(0, 2)}.xml");
			File.Delete(outputFilePath);

			var generator = new CommonTariffUniversalReferenceDataFileGenerator();
			generator.GenerateURDFiles(tariffCodeList.ToArray(), Common.UniversalXmlWriter.UpdateType.Full, DateTime.Now);

			return outputFilePath;
		}

		[Test]
		public void TestOutputFor6404209000()
		{
			var expectedOutputFile = GenerateFileForGivenTariff("6404209000");
			var outputFileContent = File.ReadAllText(expectedOutputFile);
			Assert.IsTrue(outputFileContent.Contains(@"<ZZ1_TariffCode>64042090</ZZ1_TariffCode>"));
			Assert.IsTrue(outputFileContent.Contains(@"<RefCusCondition>
      <ZX1_Comment>L'un des documents ou dispositions tarifaires particulières suivants doit être présent</ZX1_Comment>
      <ZX1_EndDate>2079-06-06T23:59:00</ZX1_EndDate>
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
		}

		[Test]
		public void TestGetOutputFor0101210000()
		{
			var expectedOutputFile = GenerateFileForGivenTariff("0101210000");
			var outputFileContent = File.ReadAllText(expectedOutputFile);
			Assert.IsTrue(outputFileContent.Contains(@"<ZZ1_TariffCode>01012100</ZZ1_TariffCode>"));
			Assert.IsTrue(outputFileContent.Contains(@"<RefCusTariffUOM>
      <ZZ8_EndDate>2079-06-06T23:59:00</ZZ8_EndDate>
      <ZZ8_StartDate>2008-01-01T00:00:00</ZZ8_StartDate>
      <ZZ8_Type>CU2</ZZ8_Type>
      <ZZ8_UOM>NAR</ZZ8_UOM>
      <ZZ8_ZZA_NKSecondTradeGroup />
      <ZZ8_ZZA_NKTradeGroup>FR04</ZZ8_ZZA_NKTradeGroup>
      <ZZ8_ZZA_ZZZ_NKSecondDataGrouping />
    </RefCusTariffUOM>"));
		}

		[Test]
		public void TestOutputFor2204214210()
		{
			var expectedOutputFile = GenerateFileForGivenTariff("2204214210");
			var outputFileContent = File.ReadAllText(expectedOutputFile);
			Assert.IsFalse(outputFileContent.Contains(@"<RefCusTariffAdditionalCode>
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
		public void TestPPHUMEMeasuresCollected()
		{
			var expectedOutputFile = GenerateFileForGivenTariff("3002901000");
			var outputFileContent = File.ReadAllText(expectedOutputFile);
			Assert.IsTrue(outputFileContent.Contains(@"<RefCusCondition>
      <ZX1_Comment>L'un des documents ou dispositions tarifaires particulières suivants doit être présent</ZX1_Comment>
      <ZX1_EndDate>2079-06-06T23:59:00</ZX1_EndDate>
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
