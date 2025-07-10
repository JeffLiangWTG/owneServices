using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using CargoWise.RefDbRepo.CHReferenceData.Business.Tariffs;
using CargoWise.RefDbRepo.CHReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.CHReferenceData.Tests.Tariffs
{
	[TestFixture]
	class AdditionalTaxesTariffsParserTest
	{
		[TestCase]
		public void TestGetCusTariffConfiguration()
		{
			using (var outputFile = new TemporaryOutputFile($@"{TemporaryOutputFolder}\TestGetCusTariffConfiguration.xml"))
			{
				var parser = new AdditionalTaxesTariffsParser(null, null, DateTime.Today);
				var writerConfiguration = parser.GetXmlWriterConfiguration();
				var writer = new XmlWriter(writerConfiguration);
				writer.SetDataSource("CH Tariff ADT");
				writer.SetPublicationTime(DateTime.Parse("2021-12-31", CultureInfo.InvariantCulture));
				writer.SetUpdateType(UpdateType.Full);
				writer.SaveXml(outputFile.FullPath);
				var expectedXML = TestExtensions.ReadManifestResourceContent($"{GetType().Namespace}.TestFiles.Output.CusTariffConfiguration_additionalTaxes.xml");
				Assert.That(TestHelper.RemoveIgnoreTagsFromXml(File.ReadAllText(outputFile.FullPath)), Is.EqualTo(expectedXML));
			}
		}

		[TestCase]
		public void TestOptionalityAndGroups()
		{
			using (var outputFile = new TemporaryOutputFile($@"AdditionalTaxes\{nameof(TestOptionalityAndGroups)}.xml"))
			{
				GetTariffsParser("TestFiles.Input.edecTariffMasterData_1_0_additionalTaxes_groups.xml").ConvertToRefXML(outputFile.FullPath);

				var outputDoc = XDocument.Load(outputFile.FullPath);

				Assert.Multiple(() =>
				{
					assertTariff("603-801", "15011091922");
					assertNoTariff("603-000", "15011091922");

					assertTariff("970-003", "20091210801");
					assertTariff("970-004", "20091210801");
					assertNoTariff("970-000", "20091210801");

					assertTariff("290-002", "15011099911");
					assertTariff("290-003", "15011099911");
					assertTariff("290-000", "15011099911");
					assertTradeGroupIncluded("290-000", "15011099911", "200000");
					assertTradeGroupExcluded("290-000", "15011099911", "200006");
					assertTradeGroupNotExcluded("290-000", "15011099911", "200007");

					assertTariff("292-001", "97069000911");
					assertTariff("292-000", "97069000911");
					assertTradeGroupIncludedNoAdditionalCode("292-000", "200000");

					var cusTariff = outputDoc.XPathSelectElement($@"//RefCusTariff[ZZ1_TariffCode='290-000']");
					Assert.That(cusTariff.XPathSelectElement($@"RefCusRate/ZZ2_RateFormula")?.Value, Is.EqualTo("0"), "290-000: should have rate 0");
					Assert.That(cusTariff.XPathSelectElement($@"ZZ1_Description")?.Value, Is.EqualTo("TypeDescription DE - OPTIONAL"), "290-000: description de");
					Assert.That(cusTariff.XPathSelectElement($@"RefCusTariffLanguage[ZX7_ZX6_NKLanguage='FR']/ZX7_Description")?.Value, Is.EqualTo("TypeDescription FR - OPTIONNEL"), "290-000: description fr");
					Assert.That(cusTariff.XPathSelectElement($@"RefCusTariffLanguage[ZX7_ZX6_NKLanguage='IT']/ZX7_Description")?.Value, Is.EqualTo("TypeDescription IT - OPZIONALE"), "290-000: description it");
					Assert.That(cusTariff.XPathSelectElement($@"RefCusTariffLanguage[ZX7_ZX6_NKLanguage='EN']/ZX7_Description")?.Value, Is.EqualTo("TypeDescription EN - OPTIONAL"), "290-000: description en");
					Assert.That(cusTariff.XPathSelectElement($@"RefCusTariffUOM"), Is.Null, "290-000: should have no UOM");
				});
				void assertTariff(string tariff, string expectedRelatedTariff)
				{
					Assert.That(outputDoc.XPathSelectElement($@"//RefCusTariff[ZZ1_TariffCode='{tariff}']"), Is.Not.Null, $"{tariff} should exist");
					Assert.That(getRelationshipOrAdditionalCode(tariff, expectedRelatedTariff), Is.Not.Null, $"{tariff}: Expected related tariff {expectedRelatedTariff}");
				}
				void assertNoTariff(string tariff, string unexpectedRelatedTariff)
				{
					Assert.That(getRelationshipOrAdditionalCode(tariff, unexpectedRelatedTariff), Is.Null, $"{tariff}: Unexpected related tariff {unexpectedRelatedTariff}");
				}
				void assertTradeGroupIncludedNoAdditionalCode(string tariff, string expectedIncludedTradeGroup)
				{
					Assert.That(outputDoc.XPathSelectElement($@"//RefCusTariff[ZZ1_TariffCode='{tariff}']/RefCusRate/RefCusApplicability/ZZT_ZZA_NKTradeGroup")?.Value, Is.EqualTo(expectedIncludedTradeGroup), $"{tariff}: Expected included trade group");
				}
				void assertTradeGroupIncluded(string tariff, string relatedTariff, string expectedIncludedTradeGroup)
				{
					Assert.That(outputDoc.XPathSelectElement($@"//RefCusTariff[ZZ1_TariffCode='{tariff}']/RefCusRate/RefCusApplicability[ZZT_AdditionalCode='{relatedTariff}']/ZZT_ZZA_NKTradeGroup")?.Value, Is.EqualTo(expectedIncludedTradeGroup), $"{tariff}: Expected included trade group");
				}
				void assertTradeGroupExcluded(string tariff, string relatedTariff, string expectedExcludedTradeGroup)
				{
					Assert.That(outputDoc.XPathSelectElement($@"//RefCusTariff[ZZ1_TariffCode='{tariff}']/RefCusRate/RefCusApplicability[ZZT_AdditionalCode='{relatedTariff}']/RefCusExcludedTradeGroup[ZZC_ZZA_NKTradeGroup='{expectedExcludedTradeGroup}']"), Is.Not.Null, $"{tariff}: Expected excluded trade group {expectedExcludedTradeGroup}");
				}
				void assertTradeGroupNotExcluded(string tariff, string relatedTariff, string unexpectedExcludedTradeGroup)
				{
					Assert.That(outputDoc.XPathSelectElement($@"//RefCusTariff[ZZ1_TariffCode='{tariff}']/RefCusRate/RefCusApplicability[ZZT_AdditionalCode='{relatedTariff}']/RefCusExcludedTradeGroup[ZZC_ZZA_NKTradeGroup='{unexpectedExcludedTradeGroup}']"), Is.Null, $"{tariff}: Unexpected excluded trade group {unexpectedExcludedTradeGroup}");
				}
				XElement getRelationshipOrAdditionalCode(string tariff, string relatedTariff)
				{
					return outputDoc.XPathSelectElement(
						$@"//RefCusTariff[ZZ1_TariffCode='{tariff}']/RefCusTariffRelationship[ZZH_TariffCode='{relatedTariff}']|" +
						$@"//RefCusTariff[ZZ1_TariffCode='{tariff}']/RefCusRate/RefCusApplicability[ZZT_AdditionalCode='{relatedTariff}']");
				}
			}
		}

		[TestCase]
		public void TestMultipleContinousDates()
		{
			using (var outputFile = new TemporaryOutputFile(@"AdditionalTaxes\TestMultipleContinousDates.xml"))
			{
				GetTariffsParser("TestFiles.Input.edecTariffMasterData_1_0_additionalTaxes_dates.xml").ConvertToRefXML(outputFile.FullPath);

				var outputDoc = XDocument.Load(outputFile.FullPath);

				Assert.Multiple(() =>
				{
					assertTariff("290-002", "2001-01-01", "2001-12-31");

					assertTariff("292-001", "2001-01-01", "2001-06-16");
					assertTariff("292-001", "2001-06-18", "2001-12-31");
				});
				void assertTariff(string tariff, string startDate, string endDate)
				{
					Assert.That(outputDoc.XPathSelectElement($@"//RefCusTariff[ZZ1_TariffCode='{tariff}'][ZZ1_StartDate='{startDate}T00:00:00'][ZZ1_EndDate='{endDate}T23:59:00']")?.Value, Is.Not.Null, $"{tariff} startDate={startDate} endDate={endDate}");
				}
			}
		}

		[TestCase]
		public void TestRateFormula()
		{
			using (var outputFile = new TemporaryOutputFile(@"AdditionalTaxes\TestRateFormula.xml"))
			{
				GetTariffsParser("TestFiles.Input.edecTariffMasterData_1_0_additionalTaxes_RateFormulas.xml").ConvertToRefXML(outputFile.FullPath);

				var outputDoc = XDocument.Load(outputFile.FullPath);

				Assert.Multiple(() =>
				{
					assertFormula("(rate 0)", "603-801", "0");
					assertFormula("11", "290-002", "MIN(MAX(88, 0.0147 * [KGMG]), 676)");
					assertFormula("13", "465-002", "1.73 * [KGM]");
					assertFormula("17", "480-001", "16.88 * [HLT]");
					assertFormula("22", "465-001", "0.0013 * [NAR]");
					assertFormula("23", "603-813", "0.4206 * [LTR]");
					assertFormula("24", "743-042", "0.2832 * [KGM]");
					assertFormula("25", "660-001", "0.04 * (VFD + DTY)");
					assertFormula("26", "700-001", "3 * [KGMV]");
					assertFormula("27", "280-001", "29 * [LPA]");
					assertFormula("28", "450-011", "18 * [KGMG]");
					assertFormula("29", "792-001", "60");
				});
				void assertFormula(string code, string tariff, string expectedFormula)
				{
					Assert.That(outputDoc.XPathSelectElement($@"//RefCusTariff[ZZ1_TariffCode='{tariff}']/RefCusRate/ZZ2_RateFormula")?.Value, Is.EqualTo(expectedFormula), $"{tariff} code={code}");
				}
			}
		}

		[TestCase]
		public void TestRefCusTariffUOM_CU1_Edec() => Assert.Multiple(() =>
		{
			using (var outputFile = new TemporaryOutputFile(@"AdditionalTaxes\TestCU1.xml"))
			{
				GetTariffsParser("TestFiles.Input.edecTariffMasterData_1_0_additionalTaxes_CU1.xml").ConvertToRefXML(outputFile.FullPath);

				var outputDoc = XDocument.Load(outputFile.FullPath);

				assertCU1("290-002", "KGMG", "11");
				assertCU1("970-003", "NAR", "12");
				assertCU1("465-002", "KGM", "13");
				assertCU1("480-001", "HLT", "17");
				assertCU1("465-001", "NAR", "22");
				assertCU1("603-801", "LTR", "23");
				assertCU1("743-042", "KGM", "24");
				assertCU1("660-001", null, "25");
				assertCU1("700-002", "KGMV", "26");
				assertCU1("280-001", "LPA", "27");
				assertCU1("450-011", "KGMG", "28");
				assertCU1("792-001", null, "29");

				assertCU1("450-001", "KGM");
				assertCU1("450-201", "KGM");
				assertCU1("450-002", "NAR");
				assertCU1("450-202", "NAR");
				assertCU1("450-003", "ML");
				assertCU1("450-203", "ML");

				void assertCU1(string tariff, string expectedUOM, string assessmentCode = "")
				{
					Assert.That(outputDoc.XPathSelectElement($@"//RefCusTariff[ZZ1_TariffCode='{tariff}']/RefCusTariffUOM[ZZ8_Type='CU1']/ZZ8_UOM")?.Value, Is.EqualTo(expectedUOM), $"{tariff} assessmentCode={assessmentCode}");
				}
			}
		});

		[TestCase]
		public void TestRefCusTariffUOM_CU1_Passar() => Assert.Multiple(() =>
		{
			using (var outputFile = new TemporaryOutputFile(@"AdditionalTaxes\TestCU1.xml"))
			{
				GetTariffsParser("TestFiles.Input.edecTariffMasterData_1_0_additionalTaxes_CU1.xml", "TestFiles.Input.statistical_keys_import_with_Zusabg_Passar.xlsx").ConvertToRefXML(outputFile.FullPath);

				var outputDoc = XDocument.Load(outputFile.FullPath);

				assertCU1("290-002", "KGMG", "206");
				assertCU1("970-003", "NAR", "201");
				assertCU1("465-002", "KGM", "227");
				assertCU1("480-001", "HLT", "221");
				assertCU1("465-001", "NAR", "201");
				assertCU1("603-801", "LTR", "211");
				assertCU1("743-042", "KGM", "227");
				assertCU1("660-001", null, "224");
				assertCU1("700-002", "KGMV", "220");
				assertCU1("280-001", "LPA", "222");
				assertCU1("450-011", "KGMG", "206");
				assertCU1("792-001", null, "226");

				assertCU1("450-001", "KGM");
				assertCU1("450-201", "KGM");
				assertCU1("450-002", "NAR");
				assertCU1("450-202", "NAR");
				assertCU1("450-003", "ML");
				assertCU1("450-203", "ML");

				void assertCU1(string tariff, string expectedUOM, string assessmentCode = "")
				{
					Assert.That(outputDoc.XPathSelectElement($@"//RefCusTariff[ZZ1_TariffCode='{tariff}']/RefCusTariffUOM[ZZ8_Type='CU1']/ZZ8_UOM")?.Value, Is.EqualTo(expectedUOM), $"{tariff} assessmentCode={assessmentCode}");
				}
			}
		});

		[TestCase]
		public void TestApplicability()
		{
			using (var outputFile = new TemporaryOutputFile(@"AdditionalTaxes\TestApplicability.xml"))
			{
				GetTariffsParser("TestFiles.Input.edecTariffMasterData_1_0_additionalTaxes_applicability.xml").ConvertToRefXML(outputFile.FullPath);

				var outputDoc = XDocument.Load(outputFile.FullPath);

				Assert.Multiple(() =>
				{
					var tariffCode = "280-001";
					var rate = outputDoc.XPathSelectElement($"//RefCusTariff[ZZ1_TariffCode='{tariffCode}']/RefCusRate");
					Assert.That(rate.XPathSelectElements("RefCusApplicability").Count(), Is.EqualTo(1), $"{tariffCode} should have only a single applicability");
					Assert.That(rate.XPathSelectElements("RefCusApplicability/ZZT_AdditionalCode").Count(), Is.EqualTo(0), $"{tariffCode} should have no additional code");
					Assert.That(rate.XPathSelectElement("RefCusApplicability/ZZT_ZZA_NKTradeGroup")?.Value, Is.EqualTo("200000"), $"{tariffCode} included trade group");

					tariffCode = "290-002";
					rate = outputDoc.XPathSelectElement($"//RefCusTariff[ZZ1_TariffCode='{tariffCode}']/RefCusRate");
					Assert.That(rate.XPathSelectElement("RefCusApplicability[1]/ZZT_AdditionalCode")?.Value, Is.EqualTo("01012110911"), $"{tariffCode} should have an additional code");
					Assert.That(rate.XPathSelectElement("RefCusApplicability[2]/ZZT_AdditionalCode")?.Value, Is.EqualTo("01012110999"), $"{tariffCode} should have an additional code");
					Assert.That(rate.XPathSelectElement("RefCusApplicability[1]/ZZT_ZZA_NKTradeGroup")?.Value, Is.EqualTo("200000"), $"{tariffCode} included trade group");
					Assert.That(rate.XPathSelectElements("RefCusApplicability[1]/RefCusExcludedTradeGroup").Count(), Is.EqualTo(3), $"{tariffCode} excluded trade groups");
					Assert.That(rate.XPathSelectElement("RefCusApplicability[1]/RefCusExcludedTradeGroup[ZZC_ZZA_NKTradeGroup='200001']"), Is.Not.Null, $"{tariffCode} excluded trade group 200001");
					Assert.That(rate.XPathSelectElement("RefCusApplicability[1]/RefCusExcludedTradeGroup[ZZC_ZZA_NKTradeGroup='200004']"), Is.Not.Null, $"{tariffCode} excluded trade group 200004");
					Assert.That(rate.XPathSelectElement("RefCusApplicability[1]/RefCusExcludedTradeGroup[ZZC_ZZA_NKTradeGroup='200006']"), Is.Not.Null, $"{tariffCode} excluded trade group 200006");
				});
			}
		}

		[TestCase]
		public void TestEitherRelationshipOrAdditionalCode()
		{
			using (var outputFile = new TemporaryOutputFile($@"AdditionalTaxes\{nameof(TestEitherRelationshipOrAdditionalCode)}.xml"))
			{
				GetTariffsParser("TestFiles.Input.edecTariffMasterData_1_0_additionalTaxes_EitherRelationshipOrAdditionalCode.xml").ConvertToRefXML(outputFile.FullPath);

				var outputDoc = XDocument.Load(outputFile.FullPath);

				Assert.Multiple(() =>
				{
					var tariffCode = "280-001";
					var tariff = outputDoc.XPathSelectElement($"//RefCusTariff[ZZ1_TariffCode='{tariffCode}']");
					var rate = tariff.XPathSelectElement("RefCusRate");
					Assert.That(rate.XPathSelectElements("RefCusApplicability").Count(), Is.EqualTo(1), $"{tariffCode} should have only a single applicability");
					Assert.That(rate.XPathSelectElements("RefCusApplicability/ZZT_AdditionalCode").Count(), Is.EqualTo(0), $"{tariffCode} should have no additional code");
					Assert.That(tariff.XPathSelectElement("RefCusTariffRelationship[ZZH_TariffCode='08121000000']"), Is.Not.Null, $"{tariffCode} relationship 0812.1000 000");
					Assert.That(tariff.XPathSelectElement("RefCusTariffRelationship[ZZH_TariffCode='08129010000']"), Is.Not.Null, $"{tariffCode} relationship 0812.9010 000");

					tariffCode = "290-002";
					tariff = outputDoc.XPathSelectElement($"//RefCusTariff[ZZ1_TariffCode='{tariffCode}']");
					rate = tariff.XPathSelectElement("RefCusRate");
					Assert.That(rate.XPathSelectElements("RefCusApplicability").Count(), Is.GreaterThan(1), $"{tariffCode} should have multiple applicabilities");
					Assert.That(rate.XPathSelectElement("RefCusApplicability[1]/ZZT_AdditionalCode")?.Value, Is.EqualTo("01012110911"), $"{tariffCode} should have an additional code");
					Assert.That(rate.XPathSelectElement("RefCusApplicability[2]/ZZT_AdditionalCode")?.Value, Is.EqualTo("01012110999"), $"{tariffCode} should have an additional code");
					Assert.That(tariff.XPathSelectElement("RefCusTariffRelationship"), Is.Null, $"{tariffCode} should have no relationship");
				});
			}
		}

		[TestCase]
		public void TestAssessmentCodeEdec()
		{
			using (var outputFile = new TemporaryOutputFile(@"AdditionalTaxes\TestAssessmentCode.xml"))
			{
				GetTariffsParser("TestFiles.Input.edecTariffMasterData_1_0_additionalTaxes_assessmentCode.xml").ConvertToRefXML(outputFile.FullPath);

				var outputDoc = XDocument.Load(outputFile.FullPath);
				AssertAssessmentCodes(outputDoc);
			}
		}

		[TestCase]
		public void TestAssessmentCodePassar()
		{
			using (var outputFile = new TemporaryOutputFile(@"AdditionalTaxes\TestAssessmentCode.xml"))
			{
				GetTariffsParser("TestFiles.Input.edecTariffMasterData_1_0_additionalTaxes_assessmentCode.xml", "TestFiles.Input.statistical_keys_import_with_Zusabg_Passar.xlsx").ConvertToRefXML(outputFile.FullPath);

				var outputDoc = XDocument.Load(outputFile.FullPath);
				AssertAssessmentCodes(outputDoc);
			}
		}

		void AssertAssessmentCodes(XDocument outputDoc)
		{
			AssertAssessmentCode(outputDoc, "292-001", "11");
			AssertAssessmentCode(outputDoc, "970-001", "12");
			AssertAssessmentCode(outputDoc, "465-002", "13");
			AssertAssessmentCode(outputDoc, "480-001", "17");
			AssertAssessmentCode(outputDoc, "465-001", "22");
			AssertAssessmentCode(outputDoc, "623-004", "23");
			AssertAssessmentCode(outputDoc, "743-042", "24");
			AssertAssessmentCode(outputDoc, "660-001", "25");
			AssertAssessmentCode(outputDoc, "700-001", "26");
			AssertAssessmentCode(outputDoc, "280-001", "27");
			AssertAssessmentCode(outputDoc, "450-011", "28");
			AssertAssessmentCode(outputDoc, "792-001", "29");
		}

		void AssertAssessmentCode(XDocument outputDoc, string tariffCode, string expectedAssessmentCode)
		{
			var tariff = outputDoc.XPathSelectElement($"//RefCusTariff[ZZ1_TariffCode='{tariffCode}']");
			Assert.That(tariff, Is.Not.Null, $"{tariffCode} should exist");
			Assert.That(tariff.XPathSelectElement("RefCusTariffAttribute[ZZ3_Name='assessmentCode']/ZZ3_Value")?.Value, Is.EqualTo(expectedAssessmentCode), $"{tariffCode} assessmentCode");
		}

		[TestCase]
		public void TestRateCode()
		{
			using (var outputFile = new TemporaryOutputFile(@"AdditionalTaxes\TestRateCode.xml"))
			{
				GetTariffsParser("TestFiles.Input.edecTariffMasterData_1_0_additionalTaxes_rateCode.xml").ConvertToRefXML(outputFile.FullPath);

				var outputDoc = XDocument.Load(outputFile.FullPath);

				var tariffCode = "792-001";
				var rate = outputDoc.XPathSelectElement($"//RefCusTariff[ZZ1_TariffCode='{tariffCode}']/RefCusRate");
				Assert.That(rate.XPathSelectElement("ZZ2_ZY1_NKRateCode")?.Value, Is.EqualTo("792"), $"{tariffCode} rateCode");
			}
		}

		internal TariffsParser GetTariffsParser(string tariffMasterData = null, string keyStructureAdditionalTaxes = null)
		{
			DownloadResult masterDataDownload = new DownloadResult
			{
				Content = GetType().GetTestArray(tariffMasterData ?? "TestFiles.Input.edecTariffMasterData_1_0.xml"),
			};

			DownloadResult keyStructureAdditionalTaxesDownload = new DownloadResult
			{
				Content = GetType().GetTestArray(keyStructureAdditionalTaxes ?? "TestFiles.Input.statistical_keys_import_with_Zusabg_Edec.xlsx"),
			};

			return new AdditionalTaxesTariffsParser(masterDataDownload, keyStructureAdditionalTaxesDownload, DefaultTestDate);
		}

		internal static bool TariffTestFilter(string x) =>
					x.StartsWith("280") || x == "290-000" || x == "290-003";

		internal readonly static DateTime DefaultTestDate = new DateTime(2024, 1, 18);

		const string TemporaryOutputFolder = "AdditionalTaxes";
	}
}
