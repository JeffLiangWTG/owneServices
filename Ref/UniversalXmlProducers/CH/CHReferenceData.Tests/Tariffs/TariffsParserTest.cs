using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using CargoWise.RefDbRepo.CHReferenceData.Business.Tariffs;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.CHReferenceData.Tests.Tariffs
{
	[TestFixture]
	internal abstract class TariffsParserTest
	{
		internal abstract TariffsParser GetTariffsParser(string tariffMasterData = null, string tradeGroupsData = null, DateTime? actualDate = null);
		internal abstract string TemporaryOutputFolder { get; }
		internal abstract string BuildTariffCode(string commodityCode, string customsFavourCode, string statisticalCode);

		internal Type classType => GetType();
		internal string classNamespace => classType.Namespace;

		[TestCase("1234.5600", null)]
		[TestCase("0207.1200", "KGM")]
		[TestCase("0207.1401", "KGM")]
		[TestCase("1701.1302", "KGM")]
		[TestCase("1701.1403", "KGM")]
		[TestCase("1701.1404", "KGM")]
		[TestCase("1701.9905", "KGM")]
		[TestCase("2403.1106", "KGM")]
		[TestCase("2403.1907", "KGM")]
		[TestCase("2208.2008", "LPA")]
		[TestCase("2208.3009", "LPA")]
		[TestCase("2208.4010", "LPA")]
		[TestCase("2208.5011", "LPA")]
		[TestCase("2208.6012", "LPA")]
		[TestCase("2208.7013", "LPA")]
		[TestCase("2208.9014", "LPA")]
		[TestCase("2402.2015", "MIL")]
		public void TestMapSensibleGoodsUOM(string commodityCode, string expectedUOM)
		{
			Assert.That(TariffsParser.MapSensibleGoodsUOM(commodityCode), Is.EqualTo(expectedUOM));
		}

		[TestCase("1234.5600", null)]
		[TestCase("0207.1201", "0")]
		[TestCase("0207.1402", "0")]
		[TestCase("1701.1203", "0")]
		[TestCase("1701.1304", "0")]
		[TestCase("1701.1405", "0")]
		[TestCase("1701.9106", "0")]
		[TestCase("1701.9907", "0")]
		[TestCase("2208.2008", "0")]
		[TestCase("2208.3009", "0")]
		[TestCase("2208.4010", "0")]
		[TestCase("2208.5011", "0")]
		[TestCase("2208.6012", "0")]
		[TestCase("2208.7013", "0")]
		[TestCase("2402.2014", "0")]
		[TestCase("2403.1115", "0")]
		[TestCase("2403.1916", "0")]
		[TestCase("2208.9017", "1")]
		public void TestMapSensibleGoodsCode(string commodityCode, string expectedCode)
		{
			Assert.That(TariffsParser.MapSensibleGoodsCode(commodityCode), Is.EqualTo(expectedCode));
		}

		[Test]
		public void TestValidityRangesDontOverlap()
		{
			using (var outputFile = new TemporaryOutputFile($@"{TemporaryOutputFolder}\TestValidityRangesDontOverlap.xml"))
			{
				GetTariffsParser().ConvertToRefXML(outputFile.FullPath);

				var outputDoc = XDocument.Load(outputFile.FullPath);

				var tariffs = new Dictionary<string, List<(DateTime, DateTime)>>();
				foreach (var tariff in outputDoc.XPathSelectElements(@"//RefCusTariff"))
				{
					var tariffCode = tariff.Element("ZZ1_TariffCode").Value;
					var startDate = DateTime.Parse(tariff.Element("ZZ1_StartDate").Value, CultureInfo.InvariantCulture);
					var endDate = DateTime.Parse(tariff.Element("ZZ1_EndDate").Value, CultureInfo.InvariantCulture);
					if (!tariffs.TryGetValue(tariffCode, out var ranges))
					{
						ranges = new List<(DateTime, DateTime)>();
						tariffs.Add(tariffCode, ranges);
					}
					else
					{
						foreach (var range in ranges)
						{
							Assert.IsFalse(range.Item1 <= endDate && range.Item2 >= startDate, $"overlapping date ranges: TariffCode={tariffCode}");
						}
					}
					ranges.Add((startDate, endDate));
				}
			}
		}

		public void AssertElementValue(string inputFile, string outputFileName, string element, string expectedValue)
		{
			using (var outputFile = new TemporaryOutputFile(outputFileName))
			{
				GetTariffsParser(inputFile).ConvertToRefXML(outputFile.FullPath);

				var outputDoc = XDocument.Load(outputFile.FullPath);

				var value = outputDoc.XPathSelectElement(element)?.Value;

				Assert.AreEqual(expectedValue, value);
			}
		}

		public void AssertAttribute(XDocument outputDoc, string tariffCode, string attributeName, string expectedValue)
		{
			var assertionMessage = $"Tariff {tariffCode}";
			var tariff = outputDoc.XPathSelectElement($"//RefCusTariff[ZZ1_TariffCode='{tariffCode}']");
			Assert.That(tariff, Is.Not.Null, $"{assertionMessage} Tariff exists");
			if (tariff != null)
			{
				var attribute = tariff.XPathSelectElement($"RefCusTariffAttribute[ZZ3_Name='{attributeName}']");
				if (expectedValue == null)
				{
					Assert.That(attribute, Is.Null, assertionMessage);
				}
				else
				{
					var value = attribute.Element("ZZ3_Value")?.Value;
					Assert.That(value, Is.EqualTo(expectedValue), assertionMessage);
				}
			}
		}

		public void AssertRefCusCondition(XDocument outputDoc, string tariffCode, string conditionType, string startDate, string endDate, params Action<XElement>[] conditionAssertions) =>
			AssertRefCusCondition (outputDoc, tariffCode, conditionType, startDate, endDate, 0, conditionAssertions);

		public void AssertRefCusCondition(XDocument outputDoc, string tariffCode, string conditionType, string startDate, string endDate, int conditionIndex, params Action<XElement>[] conditionAssertions)
		{
			var assertionMessage = $"Authority {tariffCode} {conditionType}";
			var tariff = outputDoc.XPathSelectElement($@"//RefCusTariff[ZZ1_TariffCode='{tariffCode}']");
			var condition = (XElement)tariff.XPathSelectElements($"RefCusCondition[ZX1_ZX2_NKConditionType='{conditionType}']").ToArray().GetValue(conditionIndex);
			Assert.That(condition, Is.Not.Null, $"Condition exists {assertionMessage}");
			if (condition != null)
			{
				Assert.That(condition.Element("ZX1_StartDate")?.Value, Is.EqualTo($"{startDate}T00:00:00"), $"StartDate {assertionMessage}");
				Assert.That(condition.Element("ZX1_EndDate")?.Value, Is.EqualTo($"{endDate}T23:59:00"), $"EndDate {assertionMessage}");

				foreach (var assertion in conditionAssertions)
				{
					assertion(condition);
				}
			}
		}

		public void AssertNoRefCusCondition(XDocument outputDoc, string tariffCode, string conditionType)
		{
			var assertionMessage = $"Authority {tariffCode} {conditionType}";
			var tariff = outputDoc.XPathSelectElement($@"//RefCusTariff[ZZ1_TariffCode='{tariffCode}']");
			var condition = tariff.XPathSelectElement($"RefCusCondition[ZX1_ZX2_NKConditionType='{conditionType}']");
			Assert.That(condition, Is.Null, $"Condition should not exist {assertionMessage}");
		}

		public void AssertComment(XDocument outputDoc, string tariffCode, string conditionType, params string[] expectedComment)
		{
			var assertionMessage = $"{tariffCode} #{conditionType}";
			var condition = outputDoc.XPathSelectElement($"//RefCusTariff[ZZ1_TariffCode='{tariffCode}']/RefCusCondition[ZX1_ZX2_NKConditionType='{conditionType}']");
			Assert.That(condition, Is.Not.Null, $"{assertionMessage} condition found");
			Assert.That(condition?.XPathSelectElement("ZX1_Comment")?.Value, Is.EqualTo(expectedComment[0]), $"{assertionMessage} DE");
			Assert.That(condition?.XPathSelectElement("RefCusConditionLanguage[ZXJ_ZX6_NKLanguage='FR']/ZXJ_Comment")?.Value, Is.EqualTo(expectedComment[1]), $"{assertionMessage} FR");
			Assert.That(condition?.XPathSelectElement("RefCusConditionLanguage[ZXJ_ZX6_NKLanguage='IT']/ZXJ_Comment")?.Value, Is.EqualTo(expectedComment[2]), $"{assertionMessage} IT");
			Assert.That(condition?.XPathSelectElement("RefCusConditionLanguage[ZXJ_ZX6_NKLanguage='EN']/ZXJ_Comment")?.Value, Is.EqualTo(expectedComment[3]), $"{assertionMessage} EN");
		}

		string GetAssertionMessage(XElement condition) => $"{condition.Parent.Element("ZZ1_TariffCode")?.Value} {condition.Element("ZX1_ZX2_NKConditionType")?.Value} {condition}";

		public void AssertRefCusConditionValueCount(XElement condition, int expectedValueCount)
		{
			var assertionMessage = $"Value count {GetAssertionMessage(condition)}";
			Assert.That(condition.Elements("RefCusConditionValue").Count, Is.EqualTo(expectedValueCount), assertionMessage);
		}

		public void AssertRefCusConditionValue(XElement condition, string valueType, string value)
		{
			var assertionMessage = $"Exists ValueType={valueType} Value={value} {GetAssertionMessage(condition)}";
			Assert.That(condition.XPathSelectElement($"RefCusConditionValue[ZX3_ZX4_NKValueType='{valueType}'][ZX3_Value='{value}']"), Is.Not.Null, assertionMessage);
		}

		public void AssertRefCusConditionNoValue(XElement condition, string valueType)
		{
			var assertionMessage = $"ValueType={valueType} {GetAssertionMessage(condition)}";
			Assert.That(condition.XPathSelectElement($"RefCusConditionValue[ZX3_ZX4_NKValueType='{valueType}']"), Is.Null, assertionMessage);
		}

		public void AssertApplicability(XDocument outputDoc, string tariffCode, string conditionType, string includedTradeGroup, string startDate, string endDate, string[] excludedTradeGroups = null, int conditionIndex = 0)
		{
			excludedTradeGroups = excludedTradeGroups ?? new string[0];
			var assertionMessage = $"{tariffCode} {conditionType} {includedTradeGroup}";
			var condition = (XNode)outputDoc.XPathSelectElements($"//RefCusTariff[ZZ1_TariffCode='{tariffCode}']/RefCusCondition[ZX1_ZX2_NKConditionType='{conditionType}']").ToArray().GetValue(conditionIndex);
			Assert.That(condition, Is.Not.Null, $"{assertionMessage} Condition exists");
			var applicability = condition.XPathSelectElement($"RefCusApplicability[ZZT_ZZA_NKTradeGroup={includedTradeGroup}]");
			Assert.That(applicability, Is.Not.Null, $"{assertionMessage} Applicability exists");
			if (applicability != null)
			{
				Assert.That(applicability.Element("ZZT_StartDate")?.Value, Is.EqualTo($"{startDate}T00:00:00"), $"{assertionMessage} StartDate");
				Assert.That(applicability.Element("ZZT_EndDate")?.Value, Is.EqualTo($"{endDate}T23:59:00"), $"{assertionMessage} EndDate");
				Assert.That(applicability.Elements("RefCusExcludedTradeGroup").Count, Is.EqualTo(excludedTradeGroups.Length), $"{assertionMessage} ExcludedTradeGroups count");
				foreach (var excludedTradeGroup in excludedTradeGroups)
				{
					Assert.That(applicability.XPathSelectElement($"RefCusExcludedTradeGroup[ZZC_ZZA_NKTradeGroup='{excludedTradeGroup}']"), Is.Not.Null, $"{assertionMessage} ExcludedTradeGrroup");
				}
			}
		}

		[Test]
		public void TestNonCustomsLawConditions()
		{
			using (var outputFile = new TemporaryOutputFile($@"{TemporaryOutputFolder}\TestNonCustomsLaw.xml"))
			{
				GetTariffsParser(NonCustomsLawTestInput).ConvertToRefXML(outputFile.FullPath);

				var outputDoc = XDocument.Load(outputFile.FullPath);

				Assert.Multiple(() =>
				{
					AssertRefCusCondition(outputDoc, BuildTariffCode("0106.1200", "0", "0"), $"{NonCustomsLawPrefix}190", "2013-02-01", "2079-06-06",
						(condition) => AssertRefCusConditionValueCount(condition, 1),
						(condition) => AssertRefCusConditionValue(condition, NonCustomsLawConditionValueType, "190"));
					AssertRefCusCondition(outputDoc, BuildTariffCode("0106.4900", "0", "0"), $"{NonCustomsLawPrefix}270", "2012-01-01", "2079-06-06",
						(condition) => AssertRefCusConditionValueCount(condition, 2),
						(condition) => AssertRefCusConditionValue(condition, NonCustomsLawConditionValueType, "270"),
						(condition) => AssertRefCusConditionValue(condition, "INF", "Optional"));
					AssertRefCusCondition(outputDoc, BuildTariffCode("8701.9110", "0", "911"), $"{NonCustomsLawPrefix}66", "2017-01-01", "2079-06-06",
						(condition) => AssertRefCusConditionValueCount(condition, 2),
						(condition) => AssertRefCusConditionValue(condition, NonCustomsLawConditionValueType, "66"),
						(condition) => AssertRefCusConditionValue(condition, "INF", "Optional"));
					AssertRefCusCondition(outputDoc, BuildTariffCode("8701.9110", "0", "911"), $"{NonCustomsLawPrefix}67", "2017-01-01", "2079-06-06",
						(condition) => AssertRefCusConditionValueCount(condition, 2),
						(condition) => AssertRefCusConditionValue(condition, NonCustomsLawConditionValueType, "67"),
						(condition) => AssertRefCusConditionValue(condition, "INF", "Optional"));
					AssertRefCusCondition(outputDoc, BuildTariffCode("8701.9110", "0", "911"), $"{NonCustomsLawPrefix}270", "2020-01-01", "2079-06-06",
						(condition) => AssertRefCusConditionValueCount(condition, 2),
						(condition) => AssertRefCusConditionValue(condition, NonCustomsLawConditionValueType, "270"),
						(condition) => AssertRefCusConditionValue(condition, "INF", "Optional"));
					AssertNoRefCusCondition(outputDoc, BuildTariffCode("8701.9110", "0", "911"), $"{NonCustomsLawPrefix}271");
				});
			}
		}

		[Test]
		public void TestNonCustomsLawApplicability()
		{
			using (var outputFile = new TemporaryOutputFile($@"{TemporaryOutputFolder}\TestNonCustomsLaw.xml"))
			{
				GetTariffsParser(NonCustomsLawTestInput).ConvertToRefXML(outputFile.FullPath);

				var outputDoc = XDocument.Load(outputFile.FullPath);

				Assert.Multiple(() =>
				{
					AssertApplicability(outputDoc, BuildTariffCode("0106.1200", "0", "0"), $"{NonCustomsLawPrefix}190", "200000", "2013-02-01", "2079-06-06");
					AssertApplicability(outputDoc, BuildTariffCode("0106.4900", "0", "0"), $"{NonCustomsLawPrefix}270", "200000", "2012-01-01", "2079-06-06");
					AssertApplicability(outputDoc, BuildTariffCode("8701.9110", "0", "911"), $"{NonCustomsLawPrefix}270", "200000", "2020-01-01", "2079-06-06", new[] { "200001" });
				});
			}
		}

		[Test]
		public void TestNonCustomsLawComments()
		{
			string[] commentPart1_1 = { "Kontrolle von Nichtzollrechtlichen Erlassen", "Contrôle des actes législatifs autres que douaniers", "Controllo delle leggi non doganali", "Non-customs laws check" };
			string[] commentPart1_2 = { "Kontrolle von Nichtzollrechtlichen Erlassen OPTIONAL", "Contrôle des actes législatifs autres que douaniers OPTIONNEL", "Controllo delle leggi non doganali OPZIONALE", "Non-customs laws check OPTIONAL" };

			using (var outputFile = new TemporaryOutputFile($@"{TemporaryOutputFolder}\TestNonCustomsLawComments.xml"))
			{
				GetTariffsParser(NonCustomsLawTestInput).ConvertToRefXML(outputFile.FullPath);

				var outputDoc = XDocument.Load(outputFile.FullPath);

				Assert.Multiple(() =>
				{
					AssertComment(outputDoc, BuildTariffCode("0106.1200", "0", "0"), $"{NonCustomsLawPrefix}190",
						$"{commentPart1_1[0]} (Wale, Delfine und Tümmler (Säugetiere der Ordnung der Cetacea): Einfurverbot)",
						$"{commentPart1_1[1]} (baleines, dauphins et marsouins (mammifères de l'ordre des cétacés): interdiction d'importation)",
						$"{commentPart1_1[2]} (balene, delfini e marsovini (mammiferi della specie dei cetacei): divieto d'importazione)",
						$"{commentPart1_1[3]} (whales, dolphins and porpoises (mammals of the order Cetacea): import ban)");
					AssertComment(outputDoc, BuildTariffCode("0106.4900", "0", "0"), $"{NonCustomsLawPrefix}270",
						$"{commentPart1_2[0]} (Einfuhrverbot: besonders gefährliche Schadorganismen gemäss dem Anhang 1 der Pflanzengesundheitserordnung (PGesV; SR 916.20); Ausnahmebewilligung siehe \"Bemerkungen\", \"Pflanzengesundheit ...\")",
						$"{commentPart1_2[1]} (importation interdite: organismes nuisibles particulièrement dangereux selon l'annexe 1 de l'ordonnance sur la santé des végétaux (OSaVé; RS 916.20); dérogation voir \"Remarques\", \"Prescriptions phytosanitaires ...\")",
						$"{commentPart1_2[2]} (importazione vietata: organismi nocivi particolarmente pericolosi giusta l'allegato 1 dell'ordinanza sulla salute dei vegetali (OSaIV; RS 916.20); autorizzazione eccezionale vedi \"Osservazioni\", \"Disposizioni fitosanitarie ...\")",
						$"{commentPart1_2[3]} (import ban: particularly dangerous organisms under the terms of the Annex 1 of the Plant Health Ordinance (CCFL 916.20); exception (Ausnahmebewilligung) see \"Remarks\", \"Plant health ...\")");
					AssertComment(outputDoc, BuildTariffCode("8701.9110", "0", "911"), $"{NonCustomsLawPrefix}66",
						$"{commentPart1_2[0]} (s. \"Bemerkungen\", \"Abfallrecht\")",
						$"{commentPart1_2[1]} (v. \"Remarques\", \" Législation sur les déchets\")",
						$"{commentPart1_2[2]} (v. \"Osservazioni\", \"Legislazione sui rifiuti\")",
						$"{commentPart1_2[3]} (s. \"Remarks\", \" Waste legislation\")");
					AssertComment(outputDoc, BuildTariffCode("8701.9110", "0", "911"), $"{NonCustomsLawPrefix}67",
						$"{commentPart1_2[0]} (s. \"Bemerkungen\", \"Abfallrecht\")",
						$"{commentPart1_2[1]} (v. \"Remarques\", \" Législation sur les déchets\")",
						$"{commentPart1_2[2]} (v. \"Osservazioni\", \"Legislazione sui rifiuti\")",
						$"{commentPart1_2[3]} (s. \"Remarks\", \" Waste legislation\")");
				});

			}
		}

		protected abstract string NonCustomsLawTestInput { get; }

		protected virtual string NonCustomsLawPrefix { get; }

		protected abstract string NonCustomsLawConditionValueType { get; }

		public static class PermitConditionComment
		{
			public static string[] Part1_1 = { "Vorlage einer Bewilligung", "Présentation d'un permis", "Presentazione di un permesso", "Presentation of a permit" };
			public static string[] Part1_2 = { "Vorlage einer Bewilligung bei Überschreitung des Toleranzgewichtes", "Présentation d'un permis en cas de dépassement du poids de tolérance", "Presentazione di un permesso quando la tolleranza del peso è superata", "Presentation of a permit when the weight tolerance is exceeded" };
			public static string[] Part1_3 = { "Vorlage einer Bewilligung OPTIONAL", "Présentation d’un permis OPTIONNEL", "Presentazione di un permesso OPZIONALE", "Presentation of a permit OPTIONAL" };
		}

		internal static readonly DateTime DefaultTestDate = new DateTime(2024, 1, 18);
	}

	class TariffParserForTesting : TariffsParser
	{
		TariffParserForTesting() : base(TariffsParserTest.DefaultTestDate)
		{
		}

		protected override string TariffType => "IMP";

		protected override string[] WriterConfigurationProperties => new string[] { };

		protected override string RateTypeConstantValue => "DTY";

		protected override string RateCodeConstantValue => "DTY";

		public override void ConvertToRefXML(string outputFile, Func<string, bool> tariffCodeFilter = null)
		{
			throw new NotImplementedException();
		}
	}
}
