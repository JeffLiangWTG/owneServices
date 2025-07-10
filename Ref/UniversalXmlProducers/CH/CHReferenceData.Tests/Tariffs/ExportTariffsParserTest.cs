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
	class ExportTariffsParserTest : TariffsParserTest
	{
		internal override string TemporaryOutputFolder => "ExportTariffs";

		internal override TariffsParser GetTariffsParser(string tariffMasterData = null, string tradeGroupsData = null, DateTime? actualDate = null)
		{
			DownloadResult masterDataDownload = new DownloadResult
			{
				Content = classType.GetTestArray(tariffMasterData ?? "TestFiles.Input.passarTariffMasterData_v3.xml"),
			};

			DownloadResult tariffStructureDownload = new DownloadResult
			{
				Content = classType.GetTestArray("TestFiles.Input.Tarifstruktur.xlsx"),
			};

			DownloadResult keyStructureDownload = new DownloadResult
			{
				Content = classType.GetTestArray("TestFiles.Input.key_structure_export.xlsx"),
			};

			DownloadResult permitInformationDownload = new DownloadResult
			{
				Content = classType.GetTestArray("TestFiles.Input.statistical_keys_export_with_Bew_and_Tol.xlsx"),
			};

			DownloadResult nonCustomsLawInformationDownload = new DownloadResult
			{
				Content = classType.GetTestArray("TestFiles.Input.statistical_keys_export_with_nze.xlsx"),
			};

			return new ExportTariffsParser(masterDataDownload, tariffStructureDownload, keyStructureDownload, permitInformationDownload, nonCustomsLawInformationDownload, actualDate ?? DefaultTestDate);
		}

		internal override string BuildTariffCode(string commodityCode, string customsFavourCode, string controlCode) => $"{commodityCode.Replace(".", string.Empty).PadLeft(8, '0')}{controlCode.PadLeft(3, '0')}";

		protected override string NonCustomsLawTestInput => "TestFiles.Input.passarTariffMasterData_nonCustomsLaw.xml";

		protected override string NonCustomsLawPrefix => "PN";

		protected override string NonCustomsLawConditionValueType => "RST";

		[TestCase]
		public void TestGetCusTariffConfiguration()
		{
			using (var outputFile = new TemporaryOutputFile(@"ExportTariffs\TestOutputFile.xml"))
			{
				var parser = new ExportTariffsParser(null, null, null, null, null, DateTime.Today);
				var writerConfiguration = parser.GetXmlWriterConfiguration();
				var writer = new XmlWriter(writerConfiguration);
				writer.SetDataSource("TEST");
				writer.SetPublicationTime(DateTime.Parse("2021-12-31", CultureInfo.InvariantCulture));
				writer.SetUpdateType(UpdateType.Full);
				writer.SaveXml(outputFile.FullPath);
				var expectedXML = TestExtensions.ReadManifestResourceContent($"{classNamespace}.TestFiles.Output.CusTariffConfiguration_export.xml");
				Assert.That(TestHelper.RemoveIgnoreTagsFromXml(File.ReadAllText(outputFile.FullPath)), Is.EqualTo(expectedXML));
			}
		}

		internal static bool TariffTestFilter(string x) =>
					x.StartsWith("05021000") ||
					x.StartsWith("87120000") ||
					x.StartsWith("91029100455");

		[Test]
		public void TestNoMissingDescription()
		{
			using (var outputFile = new TemporaryOutputFile(@"ExportTariffs\TestNoMissingDescription.xml"))
			{
				GetTariffsParser().ConvertToRefXML(outputFile.FullPath);
				var outputDoc = XDocument.Load(outputFile.FullPath);

				var tariffWithoutDescription = outputDoc.XPathSelectElement(@"//RefCusTariff[ZZ1_Description='']");
				Assert.IsNull(tariffWithoutDescription, $"No description: Tariff={tariffWithoutDescription?.Element("ZZ1_TariffCode")?.Value}");

				var tariffWithoutLanguageDescription = outputDoc.XPathSelectElement(@"//RefCusTariff[RefCusTariffLanguage/ZX7_Description='']");
				Assert.IsNull(tariffWithoutLanguageDescription, $"No language description: Tariff={tariffWithoutLanguageDescription?.Element("ZZ1_TariffCode")?.Value}");
			}
		}

		[Test]
		public void TestDescription()
		{
			using (var outputFile = new TemporaryOutputFile(@"ExportTariffs\TestDescriptionWithoutHierarchy.xml"))
			{
				GetTariffsParser().ConvertToRefXML(outputFile.FullPath);
				var outputDoc = XDocument.Load(outputFile.FullPath);

				var tariff = outputDoc.XPathSelectElement(@"//RefCusTariff[ZZ1_TariffCode='01022191912']");
				Assert.AreEqual("der Rassen Braunvieh, Fleckvieh, Holstein Fleckvieh", tariff?.Element("ZZ1_Description")?.Value, "Description doesn't have any VLS text");

				tariff = outputDoc.XPathSelectElement(@"//RefCusTariff[ZZ1_TariffCode='04069019962']");
				const string level3 = "45 % oder mehr, jedoch weniger als 55 %";
				const string level2 = "andere, mit einem Fettgehalt in der Trockensubstanz von";
				const string level1 = "Weichkäse mit Weissschimmel";
				Assert.AreEqual($"andere {level1} {level2} {level3}", tariff?.Element("ZZ1_Description")?.Value, "Description should include VLS text");
			}
		}

		[Test]
		public void TestNetMassOptional()
		{
			using (var outputFile = new TemporaryOutputFile($@"ExportTariffs\nameof(TestNetMassOptional).xml"))
			{
				GetTariffsParser("TestFiles.Input.passarTariffMasterData_netMassOptional.xml").ConvertToRefXML(outputFile.FullPath);
				var outputDoc = XDocument.Load(outputFile.FullPath);
				Assert.Multiple(() =>
				{
					var tariff = outputDoc.XPathSelectElement(@"//RefCusTariff[ZZ1_TariffCode='07129070911']");
					Assert.That(tariff?.XPathSelectElement(@"RefCusTariffAttribute[ZZ3_Name='netMassOptional']/ZZ3_Value")?.Value, Is.EqualTo("Y"), "Optional net mass");
					tariff = outputDoc.XPathSelectElement(@"//RefCusTariff[ZZ1_TariffCode='07129070912']");
					Assert.That(tariff?.XPathSelectElement(@"RefCusTariffAttribute[ZZ3_Name='netMassOptional']/ZZ3_Value")?.Value, Is.EqualTo("N"), "Required net mass");
					tariff = outputDoc.XPathSelectElement(@"//RefCusTariff[ZZ1_TariffCode='07129070913']");
					Assert.That(tariff?.XPathSelectElement(@"RefCusTariffAttribute[ZZ3_Name='netMassOptional']"), Is.Null, "No net mass");
				});
			}
		}

		[Test]
		public void TestUOM()
		{
			using (var outputFile = new TemporaryOutputFile(@"ExportTariffs\TestUOM.xml"))
			{
				GetTariffsParser("TestFiles.Input.passarTariffMasterData_UOM.xml").ConvertToRefXML(outputFile.FullPath);
				var outputDoc = XDocument.Load(outputFile.FullPath);
				Assert.Multiple(() =>
				{
					var tariffCode = "07129070000";
					var tariff = outputDoc.XPathSelectElement($@"//RefCusTariff[ZZ1_TariffCode='{tariffCode}']");
					Assert.That(tariff, Is.Not.Null, $"{tariffCode} not found");
					Assert.That(tariff.XPathSelectElement(@"RefCusTariffUOM[ZZ8_Type='CU1']/ZZ8_UOM")?.Value, Is.EqualTo(("KGMG")), $"{tariffCode} CU1");
					Assert.That(tariff.XPathSelectElement(@"RefCusTariffUOM[ZZ8_Type='CU2']/ZZ8_UOM")?.Value, Is.EqualTo(("KGM")), $"{tariffCode} CU2");
					Assert.That(tariff.XPathSelectElement(@"RefCusTariffUOM[ZZ8_Type='CU3']/ZZ8_UOM")?.Value, Is.EqualTo(("MTK")), $"{tariffCode} CU3");
					Assert.That(tariff.XPathSelectElement(@"RefCusTariffUOM[ZZ8_Type='CU4']/ZZ8_UOM")?.Value, Is.Null, $"{tariffCode} CU4 non-sensible goods");

					tariffCode = "02071210000";
					tariff = outputDoc.XPathSelectElement($@"//RefCusTariff[ZZ1_TariffCode='{tariffCode}']");
					Assert.That(tariff, Is.Not.Null, $"{tariffCode} not found");
					Assert.That(tariff.XPathSelectElement(@"RefCusTariffUOM[ZZ8_Type='CU4']/ZZ8_UOM")?.Value, Is.EqualTo(("KGM")), $"{tariffCode} CU4 KGM");

					tariffCode = "22082011801";
					tariff = outputDoc.XPathSelectElement($@"//RefCusTariff[ZZ1_TariffCode='{tariffCode}']");
					Assert.That(tariff, Is.Not.Null, $"{tariffCode} not found");
					Assert.That(tariff.XPathSelectElement(@"RefCusTariffUOM[ZZ8_Type='CU4']/ZZ8_UOM")?.Value, Is.EqualTo(("LPA")), $"{tariffCode} CU4 LPA");

					tariffCode = "24022010922";
					tariff = outputDoc.XPathSelectElement($@"//RefCusTariff[ZZ1_TariffCode='{tariffCode}']");
					Assert.That(tariff, Is.Not.Null, $"{tariffCode} not found");
					Assert.That(tariff.XPathSelectElement(@"RefCusTariffUOM[ZZ8_Type='CU4']/ZZ8_UOM")?.Value, Is.EqualTo(("MIL")), $"{tariffCode}CU4 MIL");
				});
			}
		}

		[Test]
		public void TestSensibleGoodsCode()
		{
			using (var outputFile = new TemporaryOutputFile(@"ExportTariffs\TestSensibleGoodsCode.xml"))
			{
				GetTariffsParser("TestFiles.Input.passarTariffMasterData_sensibleGoodsCode.xml").ConvertToRefXML(outputFile.FullPath);
				var outputDoc = XDocument.Load(outputFile.FullPath);
				Assert.Multiple(() =>
				{
					var tariffCode = "07129070000";
					var tariff = outputDoc.XPathSelectElement($@"//RefCusTariff[ZZ1_TariffCode='{tariffCode}']");
					Assert.That(tariff, Is.Not.Null, $"{tariffCode} not found");
					Assert.That(tariff?.XPathSelectElement(@"RefCusTariffAttribute[ZZ3_Name='sensibleGoodsCode']/ZZ3_Value")?.Value, Is.Null, $"{tariffCode} no sensible goods");

					tariffCode = "24022010911";
					tariff = outputDoc.XPathSelectElement($@"//RefCusTariff[ZZ1_TariffCode='{tariffCode}']");
					Assert.That(tariff, Is.Not.Null, $"{tariffCode} not found");
					Assert.That(tariff?.XPathSelectElement(@"RefCusTariffAttribute[ZZ3_Name='sensibleGoodsCode']/ZZ3_Value")?.Value, Is.EqualTo(("0")), $"{tariffCode} 0");

					tariffCode = "22089010801";
					tariff = outputDoc.XPathSelectElement($@"//RefCusTariff[ZZ1_TariffCode='{tariffCode}']");
					Assert.That(tariff, Is.Not.Null, $"{tariffCode} not found");
					Assert.That(tariff?.XPathSelectElement(@"RefCusTariffAttribute[ZZ3_Name='sensibleGoodsCode']/ZZ3_Value")?.Value, Is.EqualTo(("1")), $"{tariffCode} 1");
				});
			}
		}

		[Test]
		public void TestStorageTypeModification()
		{
			using (var outputFile = new TemporaryOutputFile(@"ExportTariffs\TestStorageType.xml"))
			{
				GetTariffsParser("TestFiles.Input.passarTariffMasterData_storageType.xml").ConvertToRefXML(outputFile.FullPath);

				var outputDoc = XDocument.Load(outputFile.FullPath);
				var tariffCode = "01012110912";
				Assert.That(outputDoc.XPathSelectElement($@"//RefCusTariff[ZZ1_TariffCode='{tariffCode}']/RefCusTariffAttribute[ZZ3_Name='storageType']/ZZ3_Value")?.Value, Is.EqualTo("Y"), tariffCode);
				tariffCode = "01012110911";
				Assert.That(outputDoc.XPathSelectElement($@"//RefCusTariff[ZZ1_TariffCode='{tariffCode}']/RefCusTariffAttribute[ZZ3_Name='storageType']/ZZ3_Value")?.Value, Is.Null, tariffCode);
			}
		}

		[Test]
		public void TestWeightCheck()
		{
			using (var outputFile = new TemporaryOutputFile($@"ExportTariffs\{nameof(TestWeightCheck)}.xml"))
			{
				GetTariffsParser("TestFiles.Input.passarTariffMasterData_weightCheck.xml").ConvertToRefXML(outputFile.FullPath);

				var outputDoc = XDocument.Load(outputFile.FullPath);

				Assert.Multiple(() =>
				{
					AssertCondition("85030091000", "1988-01-01", "2069-12-31", "WGTC1", "[KGM] >= 5000");
					AssertCondition("85011010000", "1988-01-01", "2069-12-31", "WGTC2", "[KGM]/[NAR] <= 200 & [KGM]/[NAR] >= 1");
					AssertNoCondition("07129070911");
					AssertNoCondition("85030091901");
				});

				void AssertCondition(string tariffCode, string startDate, string endDate, string conditionType, string value)
				{
					string[] comments = null;
					switch (conditionType)
					{
						case "WGTC1":
							comments = new[]
							{
								"Staffelgewichtsprüfung 1 - Nettogewicht",
								"Contrôle du poids1 – masse nette",
								"Prova del peso 1– massa netta",
								"Weight check 1 - net mass"
							};
							break;
						case "WGTC2":
							comments = new[]
							{
								"Staffelgewichtsprüfung 2 – Eigenmasse/Zusatzmenge",
								"Contrôle du poids 2 – masse nette/quantité supplémentaire",
								"Prova del peso 2 – massa netta/quantità supplementare",
								"Weight check2 - net mass/additional quantity"
							};
							break;
					}

					var condition = outputDoc.XPathSelectElement($@"//RefCusTariff[ZZ1_TariffCode='{tariffCode}']/RefCusCondition");
					Assert.That(condition, Is.Not.Null, $"{tariffCode} Condition");
					Assert.That(condition?.XPathSelectElement("ZX1_ZX2_NKConditionType")?.Value, Is.EqualTo(conditionType), $"{tariffCode} ConditionType");
					Assert.That(condition?.XPathSelectElement("ZX1_StartDate")?.Value, Is.EqualTo($"{startDate}T00:00:00"), $"{tariffCode} StartDate");
					Assert.That(condition?.XPathSelectElement("ZX1_EndDate")?.Value, Is.EqualTo($"{endDate}T23:59:00"), $"{tariffCode} EndDate");
					Assert.That(condition?.XPathSelectElement("ZX1_Comment")?.Value, Is.EqualTo(comments[0]), $"{tariffCode} Comment DE");
					Assert.That(condition?.XPathSelectElement("RefCusConditionLanguage[ZXJ_ZX6_NKLanguage='FR']/ZXJ_Comment")?.Value, Is.EqualTo(comments[1]), $"{tariffCode} Comment FR");
					Assert.That(condition?.XPathSelectElement("RefCusConditionLanguage[ZXJ_ZX6_NKLanguage='IT']/ZXJ_Comment")?.Value, Is.EqualTo(comments[2]), $"{tariffCode} Comment IT");
					Assert.That(condition?.XPathSelectElement("RefCusConditionLanguage[ZXJ_ZX6_NKLanguage='EN']/ZXJ_Comment")?.Value, Is.EqualTo(comments[3]), $"{tariffCode} Comment EN");
					Assert.That(condition?.XPathSelectElement("RefCusConditionValue/ZX3_Value")?.Value, Is.EqualTo(value), $"{tariffCode} Value");
				}

				void AssertNoCondition(string tariffCode)
				{
					var condition = outputDoc.XPathSelectElement($@"//RefCusTariff[ZZ1_TariffCode='{tariffCode}']/RefCusCondition");
					Assert.That(condition, Is.Null, $"{tariffCode}");
				}
			}
		}

		[Test]
		public void TestOverlappingDateRanges()
		{
			using (var outputFile = new TemporaryOutputFile(@"ExportTariffs\TestOverlappingDateRanges.xml"))
			{
				GetTariffsParser("TestFiles.Input.passarExportTariffMasterData_dateRanges.xml").ConvertToRefXML(outputFile.FullPath);

				var outputDoc = XDocument.Load(outputFile.FullPath);

				Assert.IsNotNull(outputDoc.XPathSelectElement(@"//RefCusTariff[ZZ1_TariffCode='15179067000' and ZZ1_StartDate='1999-02-01T00:00:00' and ZZ1_EndDate='2019-12-31T23:59:00']"));
				Assert.IsNotNull(outputDoc.XPathSelectElement(@"//RefCusTariff[ZZ1_TariffCode='15179067000' and ZZ1_StartDate='2020-01-01T00:00:00' and ZZ1_EndDate='2021-12-31T23:59:00']"));
				Assert.IsNotNull(outputDoc.XPathSelectElement(@"//RefCusTariff[ZZ1_TariffCode='15179067000' and ZZ1_StartDate='2022-01-01T00:00:00' and ZZ1_EndDate='2079-06-06T23:59:00']"));
			}
		}

		[TestCase(@"//RefCusTariff[ZZ1_TariffCode='07129070911']/RefCusCondition[ZX1_ZX2_NKConditionType='WGTC1']/RefCusConditionValue/ZX3_Value", null)]
		[TestCase(@"//RefCusTariff[ZZ1_TariffCode='07129070911']/RefCusCondition[ZX1_ZX2_NKConditionType='WGTC2']/RefCusConditionValue/ZX3_Value", null)]
		[TestCase(@"//RefCusTariff[ZZ1_TariffCode='85030000999']/RefCusCondition[ZX1_ZX2_NKConditionType='WGTC1']/RefCusConditionValue/ZX3_Value", "[KGM] >= 5000")]
		[TestCase(@"//RefCusTariff[ZZ1_TariffCode='85030000999']/RefCusCondition[ZX1_ZX2_NKConditionType='WGTC2']/RefCusConditionValue/ZX3_Value", null)]
		[TestCase(@"//RefCusTariff[ZZ1_TariffCode='84111200999']/RefCusCondition[ZX1_ZX2_NKConditionType='WGTC1']/RefCusConditionValue/ZX3_Value", null)]
		[TestCase(@"//RefCusTariff[ZZ1_TariffCode='84111200999']/RefCusCondition[ZX1_ZX2_NKConditionType='WGTC2']/RefCusConditionValue/ZX3_Value", "[KGM]/[NAR] <= 200 & [KGM]/[NAR] >= 1")]
		public void TestRefCusWeightCheckValue(string element, string expectedValue) => AssertElementValue("TestFiles.Input.passarTariffMasterData_weightCheck.xml", @"ExportTariffs\TestAssertElementValue.xml", element, expectedValue);

		[Test]
		public void TestPermitConditions() => Assert.Multiple(() =>
		{
			using (var outputFile = new TemporaryOutputFile(@"ExportTariffs\TestPermits.xml"))
			{
				GetTariffsParser("TestFiles.Input.passarExportTariffMasterData_permit.xml").ConvertToRefXML(outputFile.FullPath);

				var outputDoc = XDocument.Load(outputFile.FullPath);

				AssertRefCusCondition(outputDoc, "29031400000", "PP3", "2004-09-01", "2079-06-06",
					(condition) => AssertRefCusConditionValueCount(condition, 2),
					(condition) => AssertRefCusConditionValue(condition, "RST", "3"));
				AssertRefCusCondition(outputDoc, "24011090000", "PP21", "2004-01-01", "2079-06-06",
					(condition) => AssertRefCusConditionValueCount(condition, 2),
					(condition) => AssertRefCusConditionValue(condition, "RST", "21"),
					(condition) => AssertRefCusConditionValue(condition, "FRM", "[KGMG] <= 2.5"));
				AssertRefCusCondition(outputDoc, "29031400000", "PP6", "2018-12-01", "2079-06-06",
					(condition) => AssertRefCusConditionValueCount(condition, 2),
					(condition) => AssertRefCusConditionValue(condition, "RST", "6"),
					(condition) => AssertRefCusConditionValue(condition, "FRM", "[KGMG] <= 20"));

				AssertRefCusCondition(outputDoc, "93069010000", "PP4", "2004-01-01", "2079-06-06",
					(condition) => AssertRefCusConditionNoValue(condition, "INF"));

				AssertRefCusCondition(outputDoc, "28433090000", "PP4", "2004-01-01", "2079-06-06",
					(condition) => AssertRefCusConditionValue(condition, "INF", "Optional"));

				AssertNoRefCusCondition(outputDoc, "28433090000", "PP8");
			}
		});

		[Test]
		public void TestPermitGrpObligation() => Assert.Multiple(() =>
		{
			using (var outputFile = new TemporaryOutputFile($@"ExportTariffs\{nameof(TestPermitGrpObligation)}.xml"))
			{
				GetTariffsParser("TestFiles.Input.passarExportTariffMasterData_permit.xml").ConvertToRefXML(outputFile.FullPath);

				var outputDoc = XDocument.Load(outputFile.FullPath);

				AssertComment(outputDoc, "93020000401", "PP102",
					PermitConditionComment.Part1_3[0] + " / OBLIGATORISCH in Bewilligungsgruppe 1",
					PermitConditionComment.Part1_3[1] + " / OBLIGATOIRE en groupe de permis 1",
					PermitConditionComment.Part1_3[2] + " / OBBLIGATORIO nel gruppo di permesso 1",
					PermitConditionComment.Part1_3[3] + " / REQUIRED for Permit Group 1");
				AssertComment(outputDoc, "93020000401", "PP201",
					PermitConditionComment.Part1_3[0],
					PermitConditionComment.Part1_3[1],
					PermitConditionComment.Part1_3[2],
					PermitConditionComment.Part1_3[3]);
			}
		});

		[Test]
		public void TestPermitComments()
		{
			using (var outputFile = new TemporaryOutputFile(@"ExportTariffs\TestPermitComments.xml"))
			{
				GetTariffsParser("TestFiles.Input.passarExportTariffMasterData_permitComments.xml").ConvertToRefXML(outputFile.FullPath);

				var outputDoc = XDocument.Load(outputFile.FullPath);

				Assert.Multiple(() =>
				{
					AssertComment(outputDoc, "93069010000", "PP4",
						PermitConditionComment.Part1_1[0],
						PermitConditionComment.Part1_1[1],
						PermitConditionComment.Part1_1[2],
						PermitConditionComment.Part1_1[3]);
					AssertComment(outputDoc, "24011090000", "PP21",
						PermitConditionComment.Part1_2[0],
						PermitConditionComment.Part1_2[1],
						PermitConditionComment.Part1_2[2],
						PermitConditionComment.Part1_2[3]);
					AssertComment(outputDoc, "24011090000", "PP6",
						PermitConditionComment.Part1_2[0],
						PermitConditionComment.Part1_2[1],
						PermitConditionComment.Part1_2[2],
						PermitConditionComment.Part1_2[3],
						PermitConditionComment.Part1_3[3]);
					AssertComment(outputDoc, "29031400000", "PP4",
						PermitConditionComment.Part1_3[0],
						PermitConditionComment.Part1_3[1],
						PermitConditionComment.Part1_3[2],
						PermitConditionComment.Part1_3[3]);
					AssertComment(outputDoc, "29031400000", "PP5",
						PermitConditionComment.Part1_3[0],
						PermitConditionComment.Part1_3[1],
						PermitConditionComment.Part1_3[2],
						PermitConditionComment.Part1_3[3]);
					AssertComment(outputDoc, "29031400000", "PP1",
						PermitConditionComment.Part1_3[0],
						PermitConditionComment.Part1_3[1],
						PermitConditionComment.Part1_3[2],
						PermitConditionComment.Part1_3[3]);
					AssertComment(outputDoc, "29023090000", "PP3",
						$"{PermitConditionComment.Part1_1[0]} (zivil und militärisch verwendbare Güter (Dual-Use Güter). \r\nBei nicht bewilligungspflichtigen Ausfuhren ist in der Zollanmeldung der Vermerk \"bewilligungsfrei\" anzubringen (s. \"Bemerkungen\", \"Bewilligungspflicht\").)",
						$"{PermitConditionComment.Part1_1[1]} (biens utilisables à des fins civiles et militaires (biens à double usage). \r\nLors d'exportations non soumises au régime du permis, la mention \"exempt de permis\" doit être apportée sur la déclaration en douane (v. \"Remarques\", \"Assujettissement au permis\").)",
						$"{PermitConditionComment.Part1_1[2]} (beni utilizzabili ai fini civili e militari (beni a duplice impiego). \r\nPer le esportazioni non soggette a permesso, è necessario indicare la menzione \"esente da permesso\" nella dichiarazione doganale (v. \"Osservazioni\", \"Obbligo del permesso\").)",
						$"{PermitConditionComment.Part1_1[3]} (goods for civil and military use (dual-use goods).\r\nFor goods which are not subject to a permit, the export declaration must be marked with \"no permit required\" (cf. \"Remarks\", \"Permit Obligation\").\r\n)");
					AssertComment(outputDoc, "29032900000", "PP6",
						$"{PermitConditionComment.Part1_2[0]} (Stoffe und Zubereitungen zur Verwendung als Pflanzenschutzmittel gem. Anhang 2.5 Ziff. 4.2.1 ChemRRV\r\nwww.admin.ch/opc/de/classified-compilation/20021520/index.html#app26ahref0)",
						$"{PermitConditionComment.Part1_2[1]} (Substances et préparations destinées à être utilisées comme produit phytosanitaire selon l'annexe 2.5 ch. 4.2.1 ORRChim\r\nwww.admin.ch/opc/fr/classified-compilation/20021520/index.html#app26ahref0)",
						$"{PermitConditionComment.Part1_2[2]} (Sostanze e preparati destinati a essere utilizzati come prodotti fitosanitari giusta l'allegato 2.5 cifra 4.2.1 ORRPChim\r\nwww.admin.ch/opc/it/classified-compilation/20021520/index.html#app26ahref0)",
						$"{PermitConditionComment.Part1_2[3]} (Substances and preparations for use as plant protection products by annex 2.5 no. 4.2.1 ORRChem\r\nwww.admin.ch/opc/en/classified-compilation/20021520/index.html#app26)");
					AssertComment(outputDoc, "29031400000", "PP3",
						$"{PermitConditionComment.Part1_3[0]} (zivil und militärisch verwendbare Güter (Dual-Use Güter). \r\nBei nicht bewilligungspflichtigen Ausfuhren ist in der Zollanmeldung der Vermerk \"bewilligungsfrei\" anzubringen (s. \"Bemerkungen\", \"Bewilligungspflicht\").)",
						$"{PermitConditionComment.Part1_3[1]} (biens utilisables à des fins civiles et militaires (biens à double usage). \r\nLors d'exportations non soumises au régime du permis, la mention \"exempt de permis\" doit être apportée sur la déclaration en douane (v. \"Remarques\", \"Assujettissement au permis\").)",
						$"{PermitConditionComment.Part1_3[2]} (beni utilizzabili ai fini civili e militari (beni a duplice impiego). \r\nPer le esportazioni non soggette a permesso, è necessario indicare la menzione \"esente da permesso\" nella dichiarazione doganale (v. \"Osservazioni\", \"Obbligo del permesso\").)",
						$"{PermitConditionComment.Part1_3[3]} (goods for civil and military use (dual-use goods).\r\nFor goods which are not subject to a permit, the export declaration must be marked with \"no permit required\" (cf. \"Remarks\", \"Permit Obligation\").\r\n)");
					AssertComment(outputDoc, "08061021000", "PP1",
						string.Empty,
						null,
						null,
						null);
				});
			}
		}

		[Test]
		public void TestPermitApplicability()
		{
			using (var outputFile = new TemporaryOutputFile(@"ExportTariffs\TestPermits.xml"))
			{
				GetTariffsParser("TestFiles.Input.passarExportTariffMasterData_permit.xml").ConvertToRefXML(outputFile.FullPath);

				var outputDoc = XDocument.Load(outputFile.FullPath);

				Assert.Multiple(() =>
				{
					AssertApplicability(outputDoc, "29031400000", "PP3", "200000", "2004-09-01", "2079-06-06");
					AssertApplicability(outputDoc, "24011090000", "PP21", "200000", "2004-01-01", "2079-06-06", new[] { "200001", "200004", "200006" });
					AssertApplicability(outputDoc, "29031400000", "PP6", "200000", "2018-12-01", "2079-06-06");
					AssertApplicability(outputDoc, "28433090000", "PP4", "200000", "2004-01-01", "2079-06-06");
				});
			}
		}

		[Test]
		public void TestPlaceholderTariff()
		{
			using (var outputFile = new TemporaryOutputFile(@"ExportTariffs\TestPlaceholderTariff.xml"))
			{
				GetTariffsParser("TestFiles.Input.passarExportTariffMasterData_dateRanges.xml").ConvertToRefXML(outputFile.FullPath);

				var outputDoc = XDocument.Load(outputFile.FullPath);

				Assert.Multiple(() =>
				{
					var tariff = outputDoc.XPathSelectElement($@"//RefCusTariff[ZZ1_TariffCode='99999999000']");
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
					Assert.That(tariff.XPathSelectElements("RefCusCondition").Count(), Is.EqualTo(0), "RefCusCondition");
					Assert.That(tariff.XPathSelectElements("RefCusTariffAttribute").Count(), Is.EqualTo(0), "RefCusTariffAttribute");
				});
			}
		}

		[TestCase]
		public void TestMapQuantityCode() => Assert.Multiple(() =>
		{
			Assert.That(PassarTariffsParser.MapQuantityCode(200), Is.EqualTo("KGM"));
			Assert.That(PassarTariffsParser.MapQuantityCode(201), Is.EqualTo("NAR"));
			Assert.That(PassarTariffsParser.MapQuantityCode(202), Is.EqualTo("NAR"));
			Assert.That(PassarTariffsParser.MapQuantityCode(203), Is.EqualTo("MTR"));
			Assert.That(PassarTariffsParser.MapQuantityCode(204), Is.EqualTo("LTR"));
			Assert.That(PassarTariffsParser.MapQuantityCode(205), Is.EqualTo("MWH"));
			Assert.That(PassarTariffsParser.MapQuantityCode(206), Is.EqualTo("KGMG"));
			Assert.That(PassarTariffsParser.MapQuantityCode(207), Is.EqualTo("MTK"));
			Assert.That(PassarTariffsParser.MapQuantityCode(208), Is.EqualTo("MTQ"));
			Assert.That(PassarTariffsParser.MapQuantityCode(209), Is.EqualTo("NCR"));
			Assert.That(PassarTariffsParser.MapQuantityCode(210), Is.EqualTo("MTQ"));
			Assert.That(PassarTariffsParser.MapQuantityCode(211), Is.EqualTo("LTR"));
			Assert.That(PassarTariffsParser.MapQuantityCode(212), Is.EqualTo("NPR"));
			Assert.That(PassarTariffsParser.MapQuantityCode(223), Is.EqualTo("NBR"));
			Assert.That(PassarTariffsParser.MapQuantityCode(225), Is.EqualTo("NAR"));
			Assert.That(PassarTariffsParser.MapQuantityCode(100), Is.Null, "Unknown code");
		});

		[Test]
		public void TestAllTariffsHaveLength11()
		{
			using (var outputFile = new TemporaryOutputFile($@"ExportTariffs\{nameof(TestAllTariffsHaveLength11)}.xml"))
			{
				GetTariffsParser().ConvertToRefXML(outputFile.FullPath);

				var outputDoc = XDocument.Load(outputFile.FullPath);

				Assert.Multiple(() =>
				{
					var tariffs = outputDoc.XPathSelectElements($@"//RefCusTariff[string-length(ZZ1_TariffCode)!=11]");
					Assert.That(tariffs.Count(), Is.Zero, "Tariffs with length <> 11 should not exist");
				});
			}
		}
	}
}
