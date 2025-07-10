using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using CargoWise.RefDbRepo.CHReferenceData.Business.Tariffs;
using CargoWise.RefDbRepo.CHReferenceData.Services;
using CargoWise.RefDbRepo.CHReferenceData.Tests.TradeGroups;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.CHReferenceData.Tests.Tariffs
{
	[TestFixture]
	class ImportTariffsParserTest : TariffsParserTest
	{
		internal override string TemporaryOutputFolder => "ImportTariffs";

		internal override TariffsParser GetTariffsParser(string tariffMasterData = null, string tradeGroupsData = null, DateTime? actualDate = null)
		{
			DownloadResult masterDataDownload = new DownloadResult
			{
				Content = classType.GetTestArray(tariffMasterData ?? "TestFiles.Input.edecTariffMasterData_1_0.xml"),
			};

			DownloadResult tariffStructureDownload = new DownloadResult
			{
				Content = classType.GetTestArray("TestFiles.Input.Tarifstruktur.xlsx"),
			};

			DownloadResult keyStructureDownload = new DownloadResult
			{
				Content = classType.GetTestArray("TestFiles.Input.key_structure_import.xlsx"),
			};

			DownloadResult permitInformationDownload = new DownloadResult
			{
				Content = classType.GetTestArray("TestFiles.Input.statistical_keys_import_with_Bew_and_Tol.xlsx"),
			};

			DownloadResult nonCustomsLawInformationDownload = new DownloadResult
			{
				Content = classType.GetTestArray("TestFiles.Input.statistical_keys_import_with_nze.xlsx"),
			};

			DownloadResult customsFacilitiesDownload = new DownloadResult
			{
				Content = classType.GetTestArray("TestFiles.Input.customs_facilities.xlsx"),
			};

			DownloadResult tradeGroupsDownload = new DownloadResult
			{
				Content = tradeGroupsData == null ? typeof(TradeGroupsTest).GetTestArray("TestFiles.Input.edecCountryCodes_1_0.xml") : classType.GetTestArray(tradeGroupsData),
			};

			return new ImportTariffsParser(masterDataDownload, tariffStructureDownload, keyStructureDownload, permitInformationDownload, nonCustomsLawInformationDownload, customsFacilitiesDownload, tradeGroupsDownload, actualDate ?? DefaultTestDate);
		}

		internal override string BuildTariffCode(string commodityCode, string customsFavourCode, string statisticalCode) => $"{commodityCode.Replace(".", string.Empty).PadLeft(8, '0')}{customsFavourCode.PadLeft(3, '0')}{statisticalCode.PadLeft(3, '0')}";

		protected override string NonCustomsLawTestInput => "TestFiles.Input.edecTariffMasterData_nonCustomsLaw.xml";

		protected override string NonCustomsLawPrefix => "N";

		protected override string NonCustomsLawConditionValueType => "NCL";

		[TestCase]
		public void TestGetCusTariffConfiguration()
		{
			using (var outputFile = new TemporaryOutputFile(@"ImportTariffs\TestGetCusTariffConfiguration.xml"))
			{
				var parser = new ImportTariffsParser(null, null, null, null, null, null, null, DateTime.Today);
				var writerConfiguration = parser.GetXmlWriterConfiguration();
				var writer = new XmlWriter(writerConfiguration);
				writer.SetDataSource("TEST");
				writer.SetPublicationTime(DateTime.Parse("2021-12-31", CultureInfo.InvariantCulture));
				writer.SetUpdateType(UpdateType.Full);
				writer.SaveXml(outputFile.FullPath);
				var expectedXML = TestExtensions.ReadManifestResourceContent($"{classNamespace}.TestFiles.Output.CusTariffConfiguration_import.xml");
				Assert.That(TestHelper.RemoveIgnoreTagsFromXml(File.ReadAllText(outputFile.FullPath)), Is.EqualTo(expectedXML));
			}
		}

		[Test]
		public void TestTariffsParser() => Assert.Multiple(() =>
		{
			using (var outputFile = new TemporaryOutputFile(@"ImportTariffs\TestTariffsParser.xml"))
			using (var addcdOutputFile = new TemporaryOutputFile(@"ImportTariffs\TestTariffsParser-ADDCD.xml"))
			{
				GetTariffsParser().ConvertToRefXML(outputFile.FullPath);

				assertTariffImportFile();
				assertCodeListImportFile();

				void assertTariffImportFile()
				{
					using (var actualStream = new FileStream(outputFile.FullPath, FileMode.Open))
					using (var expectedStream = classType.GetTestStream("TestFiles.Output.edecTariffMasterData_1_0_import.xml"))
					{
						var expectedXml = XDocument.Load(expectedStream);
						var actualXml = TestHelper.RemoveIgnoreTagsFromXml(XDocument.Load(actualStream));
						Assert.IsTrue(XNode.DeepEquals(actualXml, expectedXml), $"File content does not match (Tariff)");
					}
				}

				void assertCodeListImportFile()
				{
					using (var actualStream = new FileStream(addcdOutputFile.FullPath, FileMode.Open))
					using (var expectedStream = classType.GetTestStream("TestFiles.Output.edecTariffMasterData_1_0_import-ADDCD.xml"))
					{
						var expectedXml = XDocument.Load(expectedStream);
						var actualXml = TestHelper.RemoveIgnoreTagsFromXml(XDocument.Load(actualStream));
						Assert.IsTrue(XNode.DeepEquals(actualXml, expectedXml), $"File content does not match (CodeListImportFile)");
					}
				}
			}
		});

		[Test]
		public void TestOverlappingDateRanges()
		{
			using (var outputFile = new TemporaryOutputFile(@"ImportTariffs\TestOverlappingDateRanges.xml"))
			{
				GetTariffsParser("TestFiles.Input.edecImportTariffMasterData_dateRanges.xml").ConvertToRefXML(outputFile.FullPath);

				var outputDoc = XDocument.Load(outputFile.FullPath);

				Assert.IsNotNull(outputDoc.XPathSelectElement(@"//RefCusTariff[ZZ1_TariffCode='01012110000911' and ZZ1_StartDate='2010-01-01T00:00:00' and ZZ1_EndDate='2012-12-31T23:59:00']"));
				Assert.IsNotNull(outputDoc.XPathSelectElement(@"//RefCusTariff[ZZ1_TariffCode='01012110000911' and ZZ1_StartDate='2013-01-01T00:00:00' and ZZ1_EndDate='2014-12-31T23:59:00']"));
				Assert.IsNotNull(outputDoc.XPathSelectElement(@"//RefCusTariff[ZZ1_TariffCode='01012110000911' and ZZ1_StartDate='2015-01-01T00:00:00' and ZZ1_EndDate='2019-12-31T23:59:00']"));
			}
		}

		[Test]
		public void TestNoMissingDescription()
		{
			using (var outputFile = new TemporaryOutputFile(@"ImportTariffs\TestNoMissingDescription.xml"))
			{
				GetTariffsParser().ConvertToRefXML(outputFile.FullPath, t => t.Equals("1509.2010", StringComparison.Ordinal));

				var outputDoc = XDocument.Load(outputFile.FullPath);

				var tariffWithoutDescription = outputDoc.XPathSelectElement(@"//RefCusTariff[ZZ1_Description='']");

				Assert.IsNull(tariffWithoutDescription, $"No description: Tariff={tariffWithoutDescription?.Element("ZZ1_TariffCode")?.Value}");

				var tariffWithoutLanguageDescription = outputDoc.XPathSelectElement(@"//RefCusTariff[RefCusTariffLanguage/ZX7_Description='']");
				Assert.IsNull(tariffWithoutLanguageDescription, $"No language description: Tariff={tariffWithoutLanguageDescription?.Element("ZZ1_TariffCode")?.Value}");
			}
		}

		[Test]
		public void TestStorageTypeModification()
		{
			using (var outputFile = new TemporaryOutputFile(@"ImportTariffs\TestStorageType.xml"))
			{
				GetTariffsParser("TestFiles.Input.edecTariffMasterData_storageType.xml").ConvertToRefXML(outputFile.FullPath);

				var outputDoc = XDocument.Load(outputFile.FullPath);

				Assert.IsNotNull(outputDoc.XPathSelectElement(@"//RefCusTariffAttribute[ZZ3_Name='storageType' and ZZ3_Value='N']"));
				Assert.IsNotNull(outputDoc.XPathSelectElement(@"//RefCusTariffAttribute[ZZ3_Name='storageType' and ZZ3_Value='Y']"));
			}
		}

		[Test]
		public void TestWeightCheck()
		{
			using (var outputFile = new TemporaryOutputFile($@"ImportTariffs\{nameof(TestWeightCheck)}.xml"))
			{
				GetTariffsParser("TestFiles.Input.edecTariffMasterData_weightCheck.xml").ConvertToRefXML(outputFile.FullPath);

				var outputDoc = XDocument.Load(outputFile.FullPath);

				const string tariffNoCheck = "07129070000911";
				const string tariffNotImport = "85030091000902";
				const string tariffCheck1 = "85030091000000";
				const string tariffCheck2 = "85011010000000";

				Assert.Multiple(() =>
				{
					Assert.IsNull(outputDoc.XPathSelectElement($@"//RefCusTariff[ZZ1_TariffCode='{tariffNoCheck}/RefCusCondition']"), $"{nameof(tariffNoCheck)}");
					Assert.IsNull(outputDoc.XPathSelectElement($@"//RefCusTariff[ZZ1_TariffCode='{tariffNotImport}/RefCusCondition']"), $"{nameof(tariffNotImport)}");

					var condition = outputDoc.XPathSelectElement($@"//RefCusTariff[ZZ1_TariffCode='{tariffCheck1}']/RefCusCondition");
					Assert.IsNotNull(condition, $"{nameof(tariffCheck1)} Condition");
					Assert.That(condition?.Element("ZX1_ZX2_NKConditionType")?.Value, Is.EqualTo("WGTC1"), $"{nameof(tariffCheck1)} ConditionType");
					Assert.That(condition?.XPathSelectElement("RefCusConditionValue/ZX3_Value")?.Value, Is.EqualTo("[KGM] >= 5000"), $"{nameof(tariffCheck1)} ConditionValue");
					Assert.That(condition?.XPathSelectElement("RefCusConditionValue/ZX3_ZX4_NKValueType")?.Value, Is.EqualTo("FRM"), $"{nameof(tariffCheck1)} ConditionValueType");
					Assert.That(condition?.Element("ZX1_Comment")?.Value, Is.EqualTo("Staffelgewichtsprüfung 1 - Nettogewicht"), $"{nameof(tariffCheck1)} Comment");
					Assert.That(condition?.XPathSelectElement(@"RefCusConditionLanguage[ZXJ_ZX6_NKLanguage='FR']/ZXJ_Comment")?.Value, Is.EqualTo("Contrôle du poids1 – masse nette"), $"{nameof(tariffCheck1)} Comment FR");

					condition = outputDoc.XPathSelectElement($@"//RefCusTariff[ZZ1_TariffCode='{tariffCheck2}']/RefCusCondition");
					Assert.IsNotNull(condition, $"{nameof(tariffCheck2)} Condition");
					Assert.That(condition?.Element("ZX1_ZX2_NKConditionType")?.Value, Is.EqualTo("WGTC2"), $"{nameof(tariffCheck2)} ConditionType");
					Assert.That(condition?.XPathSelectElement("RefCusConditionValue/ZX3_Value")?.Value, Is.EqualTo("[KGM]/[NAR] <= 200 & [KGM]/[NAR] >= 1"), $"{nameof(tariffCheck2)} ConditionValue");
					Assert.That(condition?.XPathSelectElement("RefCusConditionValue/ZX3_ZX4_NKValueType")?.Value, Is.EqualTo("FRM"), $"{nameof(tariffCheck2)} ConditionValueType");
					Assert.That(condition?.Element("ZX1_Comment")?.Value, Is.EqualTo("Staffelgewichtsprüfung 2 – Eigenmasse/Zusatzmenge"), $"{nameof(tariffCheck1)} Comment");
				});
			}
		}

		[TestCase(@"//RefCusTariff[ZZ1_TariffCode='01012110000911']/RefCusRate/ZZ2_RateFormula", "0.27 * [NAR]")]
		[TestCase(@"//RefCusTariff[ZZ1_TariffCode='01012110000912']/RefCusRate/ZZ2_RateFormula", "0.0026 * [KGMG]")]
		[TestCase(@"//RefCusTariff[ZZ1_TariffCode='01012110000913']/RefCusRate/ZZ2_RateFormula", "0.0025 * [KGM]")]
		[TestCase(@"//RefCusTariff[ZZ1_TariffCode='01012110000914']/RefCusRate/ZZ2_RateFormula", "0")]
		public void TestRefCusRateValue(string element, string expectedValue) => AssertElementValue("TestFiles.Input.edecTariffMasterData_RefCusRate.xml", @"ImportTariffs\TestAssertElementValue.xml", element, expectedValue);

		[TestCase("//RefCusTariff[ZZ1_TariffCode='01012110000911']/RefCusCondition[ZX1_ZX2_NKConditionType='WGTC2']/RefCusConditionValue/ZX3_Value", "[KGM]/[LTR] <= 0.9 & [KGM]/[LTR] >= 0.1")]
		[TestCase("//RefCusTariff[ZZ1_TariffCode='01012110000911']/RefCusCondition[ZX1_ZX2_NKConditionType='MVC']/RefCusConditionValue/ZX3_Value", "VFS/[KGM] <= 0.72 & VFS/[KGM] >= 0.26")]
		[TestCase("//RefCusTariff[ZZ1_TariffCode='01012110000912']/RefCusCondition[ZX1_ZX2_NKConditionType='WGTC1']/RefCusConditionValue/ZX3_Value", "[KGM] >= 0.1")]
		[TestCase("//RefCusTariff[ZZ1_TariffCode='01012110000912']/RefCusCondition[ZX1_ZX2_NKConditionType='MVC']/RefCusConditionValue/ZX3_Value", "VFS/[LTR] <= 0.72 & VFS/[LTR] >= 0.26")]
		[TestCase("//RefCusTariff[ZZ1_TariffCode='01012110000911']/RefCusCondition[ZX1_ZX2_NKConditionType='WGTC2']/RefCusConditionValue/ZX3_ZX4_NKValueType", "FRM")]
		[TestCase("//RefCusTariff[ZZ1_TariffCode='01012110000911']/RefCusCondition[ZX1_ZX2_NKConditionType='MVC']/RefCusConditionValue/ZX3_ZX4_NKValueType", "FRM")]
		[TestCase("//RefCusTariff[ZZ1_TariffCode='01012110000912']/RefCusCondition[ZX1_ZX2_NKConditionType='WGTC1']/RefCusConditionValue/ZX3_ZX4_NKValueType", "FRM")]
		[TestCase("//RefCusTariff[ZZ1_TariffCode='01012110000912']/RefCusCondition[ZX1_ZX2_NKConditionType='MVC']/RefCusConditionValue/ZX3_ZX4_NKValueType", "FRM")]
		public void TestRefCusConditionValue(string element, string expectedValue) => AssertElementValue("TestFiles.Input.edecTariffMasterData_RefCusCondition.xml", @"ImportTariffs\TestAssertElementValue.xml", element, expectedValue);

		[Test]
		public void TestPermitConditions()
		{
			using (var outputFile = new TemporaryOutputFile(@"ImportTariffs\TestPermits.xml"))
			{
				GetTariffsParser("TestFiles.Input.edecTariffMasterData_permit.xml").ConvertToRefXML(outputFile.FullPath);

				var outputDoc = XDocument.Load(outputFile.FullPath);

				Assert.Multiple(() =>
				{
					AssertRefCusCondition(outputDoc, "07052971000000", "PA1", "2004-01-01", "2079-06-06",
						(condition) => AssertRefCusConditionValueCount(condition, 1),
						(condition) => AssertRefCusConditionValue(condition, "PRM", "1"));
					AssertRefCusCondition(outputDoc, "07052971000000", "PA2", "2004-01-01", "2024-12-31",
						(condition) => AssertRefCusConditionValueCount(condition, 2),
						(condition) => AssertRefCusConditionValue(condition, "PRM", "2"),
						(condition) => AssertRefCusConditionValue(condition, "FRM", "[KGMG] <= 2.5"));
					AssertRefCusCondition(outputDoc, "07052971000000", "PA2", "2025-01-01", "2079-06-06", 1,
						(condition) => AssertRefCusConditionValueCount(condition, 3),
						(condition) => AssertRefCusConditionValue(condition, "PRM", "2"),
						(condition) => AssertRefCusConditionValue(condition, "FRM", "[KGMG] <= 2.5"));
					AssertRefCusCondition(outputDoc, "07052971000000", "PA3", "2004-01-01", "2079-06-06",
						(condition) => AssertRefCusConditionValueCount(condition, 2),
						(condition) => AssertRefCusConditionValue(condition, "PRM", "3"),
						(condition) => AssertRefCusConditionValue(condition, "FRM", "[KGMG] <= 20"));
					AssertRefCusCondition(outputDoc, "07052971000000", "PA4", "2004-01-01", "2079-06-06",
						(condition) => AssertRefCusConditionValueCount(condition, 3),
						(condition) => AssertRefCusConditionValue(condition, "PRM", "4"),
						(condition) => AssertRefCusConditionValue(condition, "FRM", "[KGMG] <= 2.5"));

					AssertRefCusCondition(outputDoc, "01022110000911", "PA1", "2019-01-01", "2079-06-06",
						(condition) => AssertRefCusConditionNoValue(condition, "INF"));
					AssertRefCusCondition(outputDoc, "01022110000911", "PA26", "2019-01-01", "2079-06-06",
						(condition) => AssertRefCusConditionNoValue(condition, "INF"));

					AssertRefCusCondition(outputDoc, "93020000000000", "PA18", "2004-01-01", "2079-06-06",
						(condition) => AssertRefCusConditionValue(condition, "INF", "Optional"));
				});

			}
		}

		[Test]
		public void TestPermitApplicability()
		{
			using (var outputFile = new TemporaryOutputFile(@"ImportTariffs\TestPermits.xml"))
			{
				GetTariffsParser("TestFiles.Input.edecTariffMasterData_permit.xml").ConvertToRefXML(outputFile.FullPath);

				var outputDoc = XDocument.Load(outputFile.FullPath);

				Assert.Multiple(() =>
				{
					AssertApplicability(outputDoc, "07052971000000", "PA1", "200000", "2004-01-01", "2079-06-06");
					AssertApplicability(outputDoc, "07052971000000", "PA2", "200000", "2004-01-01", "2024-12-31");
					AssertApplicability(outputDoc, "07052971000000", "PA2", "200000", "2025-01-01", "2079-06-06", conditionIndex: 1);
					AssertApplicability(outputDoc, "07052971000000", "PA3", "200000", "2004-01-01", "2079-06-06");
					AssertApplicability(outputDoc, "01012110000911", "PA26", "200000", "2019-01-01", "2079-06-06", new[] { "200001", "200004", "200006" });
				});
			}
		}

		[Test]
		public void TestPermitComments()
		{
			using (var outputFile = new TemporaryOutputFile(@"ImportTariffs\TestPermitComments.xml"))
			{
				GetTariffsParser("TestFiles.Input.edecTariffMasterData_permitComments.xml").ConvertToRefXML(outputFile.FullPath);

				var outputDoc = XDocument.Load(outputFile.FullPath);

				Assert.Multiple(() =>
				{
					AssertComment(outputDoc, "01042010000911", "PA1",
						PermitConditionComment.Part1_1[0],
						PermitConditionComment.Part1_1[1],
						PermitConditionComment.Part1_1[2],
						PermitConditionComment.Part1_1[3]);
					AssertComment(outputDoc, "24011090000000", "PA21",
						PermitConditionComment.Part1_2[0],
						PermitConditionComment.Part1_2[1],
						PermitConditionComment.Part1_2[2],
						PermitConditionComment.Part1_2[3]);
					AssertComment(outputDoc, "01051100000911", "PA1",
						PermitConditionComment.Part1_2[0],
						PermitConditionComment.Part1_2[1],
						PermitConditionComment.Part1_2[2],
						PermitConditionComment.Part1_2[3]);
					AssertComment(outputDoc, "01042020000000", "PA1",
						PermitConditionComment.Part1_3[0],
						PermitConditionComment.Part1_3[1],
						PermitConditionComment.Part1_3[2],
						PermitConditionComment.Part1_3[3]);
					AssertComment(outputDoc, "24012090000000", "PA21",
						PermitConditionComment.Part1_3[0],
						PermitConditionComment.Part1_3[1],
						PermitConditionComment.Part1_3[2],
						PermitConditionComment.Part1_3[3]);
					AssertComment(outputDoc, "01051200000000", "PA1",
						PermitConditionComment.Part1_3[0],
						PermitConditionComment.Part1_3[1],
						PermitConditionComment.Part1_3[2],
						PermitConditionComment.Part1_3[3]);
					AssertComment(outputDoc, "01042090000000", "PA26",
						$"{PermitConditionComment.Part1_1[0]} (GGED, Bewilligung oder Gesundheitsbescheinigung erforderlich (s. \"Bemerkungen\", \"Veterinärrecht\"))",
						$"{PermitConditionComment.Part1_1[1]} (DSCE, permis ou certificat sanitaire nécessaire (v. \"Remarques\", \"Législation vétérinaire\"))",
						$"{PermitConditionComment.Part1_1[2]} (DSCE, permesso o certificato sanitario necessario  (v. \"Osservazioni\", \"Legislazione veterinaria\"))",
						$"{PermitConditionComment.Part1_1[3]} (CHED, permit or health certificate necessary (cf. \"Remarks\", \"Veterinary legislation\"))");
					AssertComment(outputDoc, "06021000000000", "PA6",
						$"{PermitConditionComment.Part1_1[0]} (Waldbäume (Anhang 1 der Verordnung über forstliches Vermehrungsgut; SR 921.552.1) in Sendungen von über 200 Stück (s. \"Bemerkungen\", \"Pflanzengesundheit ...\"))",
						$"{PermitConditionComment.Part1_1[1]} (arbres forestiers (annexe 1 de l'Ordonnance sur le matériel forestier de reproduction; RS 921.552.1) en envoi de plus de 200 pièces (v. \"Remarques\", \"Prescriptions phytosanitaires ...\"))",
						$"{PermitConditionComment.Part1_1[2]} (alberi forestali (allegato 1 dell'Ordinanza sul materiale di riproduzione forestale; RS 921.552.1) in invii eccedenti 200 capi (v. \"Osservazioni\", \"Disposizioni fitosanitarie ...\"))",
						$"{PermitConditionComment.Part1_1[3]} (forest trees (Annex 1 of the Ordinance on Forest Reproductive Material; SR 921.552.1) in consignments of over 200 units (s. \"Remarks\", \"Plant Health...\"))");
					AssertComment(outputDoc, "06029099000911", "PA6",
						$"{PermitConditionComment.Part1_1[0]} (Waldbäume (Anhang 1 der Verordnung über forstliches Vermehrungsgut; SR 921.552.1) (s. \"Bemerkungen\", \"Pflanzengesundheit ...\"), in Sendungen von über 200 Stück)",
						$"{PermitConditionComment.Part1_1[1]} (arbres forestiers (annexe 1 de l'Ordonnance sur le matériel forestier de reproduction; RS 921.552.1) (v. \"Remarques\", \"Prescriptions phytosanitaires ...\"), en envois de plus de 200 pièces)",
						$"{PermitConditionComment.Part1_1[2]} (alberi forestali (allegato 1 dell'Ordinanza sul materiale di riproduzione forestale; RS 921.552.1)  (v. \"Osservazioni\", \"Disposizioni fitosanitarie ...\"), in invii eccedenti 200 capi)",
						$"{PermitConditionComment.Part1_1[3]} (forest trees (Annex 1 of the Ordinance on Forest Reproductive Material; SR 921.552.1) (s. \"Remarks\", \"Plant Health...\"), in consignments of over 200 units)");
					AssertComment(outputDoc, "08061021000000", "PA1",
						string.Empty,
						null,
						null,
						null);
				});
			}
		}

		[Test]
		public void TestPermitOptionalAttribute()
		{
			using (var outputFile = new TemporaryOutputFile(@"ImportTariffs\TestPermits.xml"))
			{
				GetTariffsParser("TestFiles.Input.edecTariffMasterData_permit.xml").ConvertToRefXML(outputFile.FullPath);

				var outputDoc = XDocument.Load(outputFile.FullPath);
				const string attributeName = "hasOptionalPermit";

				Assert.Multiple(() =>
				{
					AssertAttribute(outputDoc, "93020000000000", attributeName, "1");
					AssertAttribute(outputDoc, "93020000000001", attributeName, "0");
					AssertAttribute(outputDoc, "85011010000000", attributeName, null);
				});
			}
		}

		[Test]
		public void TestPermitDuplicateKey()
		{
			using (var outputFile = new TemporaryOutputFile(@"ImportTariffs\TestPermitsDuplicateKey.xml"))
			{

				GetTariffsParser("TestFiles.Input.edecTariffMasterData_permitDuplicateKey.xml").ConvertToRefXML(outputFile.FullPath);

				var outputDoc = XDocument.Load(outputFile.FullPath);

				var tariffCode = "06021000000000";
				var tariff = outputDoc.XPathSelectElement($"//RefCusTariff[ZZ1_TariffCode='{tariffCode}']");
				var condition = outputDoc.XPathSelectElement($"//RefCusTariff[ZZ1_TariffCode='{tariffCode}']/RefCusCondition[ZX1_ZX2_NKConditionType='PA20']");

				Assert.Multiple(() =>
				{
					Assert.That(tariff, Is.Not.Null, $"{tariffCode} Tariff exists");
					Assert.That(condition?.XPathSelectElements("RefCusConditionValue[ZX3_Value = '20']"), Is.Unique, $"{tariffCode} unique key of permits");
				});
			}
		}

		[Test]
		public void TestAdditionalCode()
		{
			using (var outputFile = new TemporaryOutputFile(@"ImportTariffs\TestAdditionalCode.xml"))
			{
				var parser = GetTariffsParser("TestFiles.Input.edecTariffMasterData_additionalCode.xml") as ImportTariffsParser;
				parser.ConvertToRefXML(outputFile.FullPath);

				var outputDoc = TestHelper.RemoveIgnoreTagsFromXml(XDocument.Load(outputFile.FullPath));

				Assert.Multiple(() =>
				{
					assertAdditionalCode("06029091000000", "100005", 1, "Zierpflanzen", "ornamental plants", "plantes d'ornement", "piante da ornamento");
					assertAdditionalCode("06029091000000", "100005", 2, "andere als Zierpflanzen", "other than ornamental plants", "autres que les plantes d'ornement", "diversi dalle piante da ornamento");
					assertAdditionalCode("06029091000000", "100110", 3, "Zierpflanzen", "ornamental plants", "plantes d'ornement", "piante da ornamento");
					assertAdditionalCode("06029091000000", "100110", 4, "andere als Zierpflanzen", "other than ornamental plants", "autres que les plantes d'ornement", "diversi dalle piante da ornamento");
					assertNoAdditionalCode("06029091000000", "200000", 5);
					assertNoAdditionalCode("06029091000000", "200000", 6);

					assertAdditionalCode("07052971000000", "100005", 1, "Zierpflanzen", "ornamental plants", "plantes d'ornement", "piante da ornamento");
					assertAdditionalCode("07052971000000", "100005", 2, "andere als Zierpflanzen", "other than ornamental plants", "autres que les plantes d'ornement", "diversi dalle piante da ornamento");
					assertAdditionalCode("07052971000000", "100099", 4, "4", null, null, null);
					assertNoAdditionalCode("07052971000000", "100110", 3);

					assertNoAdditionalCode("97060000000000", "100001", 1, startDate: "2010-01-01", endDate: "2010-12-31");
					assertAdditionalCode("97060000000000", "100001", 1, "#9706.0000_1", null, null, null, startDate: "2011-01-01", endDate: "2011-12-31");
					assertAdditionalCode("97060000000000", "100001", 2, "#9706.0000_2", null, null, null, startDate: "2011-01-01", endDate: "2011-12-31");
					assertNoAdditionalCode("97060000000000", "100001", 2, startDate: "2012-01-01", endDate: "2012-12-31");

					assertNoAdditionalCode("97060000000000", "100002", 3, startDate: "2010-01-01", endDate: "2010-12-31");
					assertAdditionalCode("97060000000000", "100002", 3, "#9706.0000_3", null, null, null, startDate: "2011-01-01", endDate: "2011-12-31");
					assertAdditionalCode("97060000000000", "100002", 4, "#9706.0000_4", null, null, null, startDate: "2011-01-01", endDate: "2011-12-31");

					assertAdditionalCode("97060000000000", "100003", 5, "#9706.0000_5", null, null, null, startDate: "2011-01-01", endDate: "2011-12-31");
					assertAdditionalCode("97060000000000", "100003", 6, "#9706.0000_6", null, null, null, startDate: "2011-01-01", endDate: "2011-12-31");
					assertNoAdditionalCode("97060000000000", "100003", 6, startDate: "2012-01-01", endDate: "2012-12-31");

					assertNoAdditionalCode("97060000000000", "100004", 7, startDate: "2010-01-01", endDate: "2010-12-31");
					assertNoAdditionalCode("97060000000000", "100004", 8, startDate: "2012-01-01", endDate: "2012-12-31");
					assertRateCount("97060000000000", "100004", 2);

					Assert.That(parser.AdditionalCodeList.Count(), Is.EqualTo(9));
				});

				string getRatePredicate(int rate, string startDate = null, string endDate = null)
				{
					var ratePredicate = $"ZZ2_RateFormula='0.0{rate} * [KGMG]'";
					if (startDate != null)
					{
						ratePredicate += $" and ZZ2_StartDate='{startDate}T00:00:00'";
					}
					if (endDate != null)
					{
						ratePredicate += $" and ZZ2_EndDate='{endDate}T23:59:00'";
					}
					return ratePredicate;
				}

				void assertAdditionalCode(string tariffCode, string tradeGroup, int rate, string descriptionDE, string descriptionEN, string descriptionFR, string descriptionIT, string startDate = null, string endDate = null)
				{
					var assertionMessage = $"{tariffCode}/{tradeGroup}/{rate}";

					var tariff = outputDoc.XPathSelectElement($"//RefCusTariff[ZZ1_TariffCode='{tariffCode}']");
					Assert.That(tariff, Is.Not.Null, $"{assertionMessage} Tariff exists");

					var additionalCode = $@"RefCusRate[{getRatePredicate(rate, startDate, endDate)}]/RefCusApplicability[ZZT_ZZA_NKTradeGroup='{tradeGroup}']/ZZT_AdditionalCode";
					Assert.That(additionalCode, Is.Not.Null, $"{assertionMessage} rate={rate} tradeGroup={tradeGroup} No AdditionalCode ({additionalCode})");
					var additionalCodeValue = tariff.XPathSelectElement(additionalCode)?.Value;
					var additionalCodeItem = parser.AdditionalCodeList.GetCusCodeListByHashCode(additionalCodeValue);
					Assert.That(additionalCodeItem, Is.Not.Null, $"{assertionMessage} Code {additionalCodeValue} not in AdditionalCodeList");
					Assert.That(additionalCodeItem?.ZZD_Code, Is.EqualTo(additionalCodeValue), $"{assertionMessage} Code value");
					Assert.That(additionalCodeItem?.ZZD_Description, Is.EqualTo(descriptionDE), $"{assertionMessage} Description DE");
					Assert.That(additionalCodeItem?.RefCusCodeListLanguages.FirstOrDefault(l => l.ZXA_ZX6_NKLanguage == "EN")?.ZXA_Description, Is.EqualTo(descriptionEN), $"{assertionMessage} Description EN");
					Assert.That(additionalCodeItem?.RefCusCodeListLanguages.FirstOrDefault(l => l.ZXA_ZX6_NKLanguage == "FR")?.ZXA_Description, Is.EqualTo(descriptionFR), $"{assertionMessage} Description FR");
					Assert.That(additionalCodeItem?.RefCusCodeListLanguages.FirstOrDefault(l => l.ZXA_ZX6_NKLanguage == "IT")?.ZXA_Description, Is.EqualTo(descriptionIT), $"{assertionMessage} Description IT");
				}

				void assertNoAdditionalCode(string tariffCode, string tradeGroup, int rate, string startDate = null, string endDate = null)
				{
					var assertionMessage = $"{tariffCode}/{tradeGroup}/{rate}";
					var tariff = outputDoc.XPathSelectElement($"//RefCusTariff[ZZ1_TariffCode='{tariffCode}']");
					Assert.That(tariff, Is.Not.Null, $"{assertionMessage} tariff exists");
					var additionalCode = tariff.XPathSelectElement($@"RefCusRate[{getRatePredicate(rate, startDate, endDate)}]/RefCusApplicability[ZZT_ZZA_NKTradeGroup='{tradeGroup}']/ZZT_AdditionalCode");
					Assert.That(additionalCode, Is.Null, $"{assertionMessage} No AdditionalCode expected: {additionalCode?.Value}");
				}

				void assertRateCount(string tariffCode, string tradeGroup, int expectedCount)
				{
					var assertionMessage = $"Tariff {tariffCode}/{tradeGroup}";
					var tariff = outputDoc.XPathSelectElement($"//RefCusTariff[ZZ1_TariffCode='{tariffCode}']");
					Assert.That(tariff, Is.Not.Null, $"{assertionMessage} tariff exists");
					var expr = $"RefCusRate[RefCusApplicability/ZZT_ZZA_NKTradeGroup='{tradeGroup}']";
					Assert.That(tariff.XPathSelectElements(expr).Count(), Is.EqualTo(expectedCount), $"{assertionMessage} Rate count - {expr}");
				}
			}
		}

		[Test]
		public void TestPlaceholderTariff()
		{
			using (var outputFile = new TemporaryOutputFile(@"ExportTariffs\TestPlaceholderTariff.xml"))
			{
				GetTariffsParser("TestFiles.Input.edecTariffMasterData_storageType.xml").ConvertToRefXML(outputFile.FullPath);

				var outputDoc = XDocument.Load(outputFile.FullPath);

				Assert.Multiple(() =>
				{
					var tariff = outputDoc.XPathSelectElement($@"//RefCusTariff[ZZ1_TariffCode='99999999000000']");
					Assert.That(tariff, Is.Not.Null, "Tariff exists");
					Assert.That(tariff.XPathSelectElement("ZZ1_StartDate")?.Value, Is.EqualTo("1999-07-01T00:00:00"), "ZZ1_StartDate");
					Assert.That(tariff.XPathSelectElement("ZZ1_EndDate")?.Value, Is.EqualTo("2079-06-06T23:59:00"), "ZZ1_EndDate");
					Assert.That(tariff.XPathSelectElement("ZZ1_CompositeKeyOnZZ5")?.Value, Is.EqualTo("21.99.99.99.99"), "ZZ1_CompositeKeyOnZZ5");
					Assert.That(tariff.XPathSelectElements("RefCusTariffUOM").Count(), Is.EqualTo(2), "RefCusTariffUOM count");
					Assert.That(tariff.XPathSelectElement("RefCusTariffUOM[ZZ8_Type='CU1']/ZZ8_UOM")?.Value, Is.EqualTo("KGMG"), "ZZ8_UOM CU1");
					Assert.That(tariff.XPathSelectElement("RefCusTariffUOM[ZZ8_Type='CU2']/ZZ8_UOM")?.Value, Is.EqualTo("KGM"), "ZZ8_UOM CU2");
					Assert.That(tariff.XPathSelectElement("ZZ1_Description")?.Value, Is.EqualTo("Warenmuster und Warenproben – Sendungen in kleinen Mengen und von unbedeutendem Wert"), "ZZ1_Description");
					Assert.That(tariff.XPathSelectElement("RefCusTariffLanguage[ZX7_ZX6_NKLanguage='FR']/ZX7_Description")?.Value, Is.EqualTo("Echantillons et spécimens de marchandises – Envoies en petites quantités et d’une valeur insignifiantes"), "ZX7_Description FR");
					Assert.That(tariff.XPathSelectElement("RefCusTariffLanguage[ZX7_ZX6_NKLanguage='IT']/ZX7_Description")?.Value, Is.EqualTo("Campioni e saggi di merci – Invii di quantità e valore esigui"), "ZX7_Description IT");
					Assert.That(tariff.XPathSelectElement("RefCusTariffLanguage[ZX7_ZX6_NKLanguage='EN']/ZX7_Description")?.Value, Is.EqualTo("Commercial samples and specimens – Consignments in small quantities and of insignificant value"), "ZX7_Description EN");
					Assert.That(tariff.XPathSelectElements("RefCusRate").Count(), Is.EqualTo(1), "RefCusRate count");
					Assert.That(tariff.XPathSelectElement("RefCusRate/ZZ2_StartDate")?.Value, Is.EqualTo("2000-01-01T00:00:00"), "ZZ2_StartDate");
					Assert.That(tariff.XPathSelectElement("RefCusRate/ZZ2_EndDate")?.Value, Is.EqualTo("2079-06-06T23:59:00"), "ZZ2_EndDate");
					Assert.That(tariff.XPathSelectElement("RefCusRate/ZZ2_RateFormula")?.Value, Is.EqualTo("0"), "ZZ2_RateFormula");
					Assert.That(tariff.XPathSelectElement("RefCusRate/ZZ2_ZZS_NKPreference")?.Value, Is.EqualTo("NT"), "ZZ2_ZZS_NKPreference");
					Assert.That(tariff.XPathSelectElement("RefCusRate/ZZ2_ZZS_ZZZ_NKDataGrouping")?.Value, Is.EqualTo("CH"), "ZZ2_ZZS_ZZZ_NKDataGrouping");
					Assert.That(tariff.XPathSelectElement("RefCusRate/RefCusApplicability/ZZT_StartDate")?.Value, Is.EqualTo("2000-01-01T00:00:00"), "ZZT_StartDate");
					Assert.That(tariff.XPathSelectElement("RefCusRate/RefCusApplicability/ZZT_EndDate")?.Value, Is.EqualTo("2079-06-06T23:59:00"), "ZZT_EndDate");
					Assert.That(tariff.XPathSelectElements("RefCusCondition").Count(), Is.EqualTo(0), "RefCusCondition");
					Assert.That(tariff.XPathSelectElements("RefCusTariffAttribute").Count(), Is.EqualTo(0), "RefCusTariffAttribute");
				});
			}
		}

		[Test]
		public void TestCountriesInMultipleTrageGroups() => Assert.Multiple(() =>
		{
			using (var outputFile = new TemporaryOutputFile(@"ExportTariffs\TestPlaceholderTariff.xml"))
			{
				GetTariffsParser("TestFiles.Input.edecMultipleRates.xml", tradeGroupsData: "TestFiles.Input.multipleRateCountries.xml").ConvertToRefXML(outputFile.FullPath);
				var outputDoc = XDocument.Load(outputFile.FullPath);

				AssertRate("15179067001000", "100011");
				AssertRate("15179067002000", "100021", "100022,100023");
				AssertRate("15179067002000", "100022", "100023");
				AssertRate("15179067002000", "100023");

				void AssertRate(string tariffCode, string tradeGroup, string expectedExcludedTradeGroups = "")
				{
					var tariff = outputDoc.XPathSelectElement($@"//RefCusTariff[ZZ1_TariffCode='{tariffCode}']");
					Assert.That(tariff, Is.Not.Null, $"{tariffCode}: Tariff exists");
					var applicability = tariff.XPathSelectElement($@"RefCusRate/RefCusApplicability[ZZT_ZZA_NKTradeGroup='{tradeGroup}']");
					Assert.That(applicability, Is.Not.Null, $"{tariffCode}: Applicability exists for TradeGroup {tradeGroup}");
					var actualExcludedTradeGroups = string.Join(",", applicability.XPathSelectElements(@"RefCusExcludedTradeGroup/ZZC_ZZA_NKTradeGroup").Select(x => x.Value));
					Assert.That(actualExcludedTradeGroups, Is.EqualTo(expectedExcludedTradeGroups), $"{tariffCode} TradeGoup={tradeGroup}: Excluded TradeGroups");
				}
			}
		});
	}
}
