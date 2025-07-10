using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using CargoWise.RefDbRepo.GBReferenceData.Business.Tariff;
using CargoWise.RefDbRepo.SharedReferenceData.Business.Tariff.Processors;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Models;
using NUnit.Framework;
using static CargoWise.RefDbRepo.GBReferenceData.Business.Tariff.MeasureMappingProvider;

namespace CargoWise.RefDbRepo.GBReferenceData.Tests.Tariff
{
	[TestFixture]
	class MeasureMappingProviderTests
	{
		[TestCase(null, "")]
		[TestCase("", "")]
		[TestCase("Unknown", "")]
		[TestCase("103", MeasureHelper.ConditionClass.Rate)]
		[TestCase("PRE", MeasureHelper.ConditionClass.Control)]
		[TestCase("482", MeasureHelper.ConditionClass.Class)]
		[TestCase("305", MeasureHelper.ConditionClass.Vat)]
		public void MeasureTypeMapping_ConditionClass(string measureType, string expectedClass)
		{
			var actual = measureTypeHelper.GetConditionClass(measureType);

			Assert.That(actual, Is.EqualTo(expectedClass), $"MeasureType: {measureType}");
		}

		[TestCase(null, "")]
		[TestCase("", "")]
		[TestCase("Unknown", "")]
		[TestCase("103", "DTY")]
		[TestCase("555", "ADD")]
		[TestCase("690", "CVD")]
		[TestCase("652", "SEC")]
		[TestCase("306", "EXC")]
		[TestCase("680", "EXP")]
		[TestCase("EXB", "LEV")]
		public void MeasureTypeMapping_RateType(string measureType, string expectedRateType)
		{
			var actual = measureTypeHelper.GetRateType(measureType);

			Assert.That(actual, Is.EqualTo(expectedRateType), $"MeasureType: {measureType}");
		}

		[TestCase(null, "")]
		[TestCase("", "")]
		[TestCase("Unknown", "")]
		[TestCase("551", "A35")]
		[TestCase("552", "A30")]
		[TestCase("553", "A45")]
		[TestCase("554", "A40")]
		[TestCase("103", "")]
		[TestCase("DDD", "444")]
		[TestCase("695", "A20")]
		[TestCase("696", "A20")]
		[TestCase("306", "")]
		public void MeasureTypeMapping_RateCode(string measureType, string expectedRateCode)
		{
			var actual = measureTypeHelper.GetRateCode(measureType);

			Assert.That(actual, Is.EqualTo(expectedRateCode), $"MeasureType: {measureType}");
		}

		[TestCase(null, true)]
		[TestCase("", true)]
		[TestCase("Unknown", true)]
		[TestCase("103", true)]
		[TestCase("488", false)]
		[TestCase("490", false)]
		public void MeasureTypeMapping_ShouldProcessMeasureType(string measureType, bool expectedShouldProcess)
		{
			var actual = measureTypeHelper.ShouldProcessMeasureType(measureType, false);

			Assert.That(actual, Is.EqualTo(expectedShouldProcess), $"MeasureType: {measureType}");
		}

		[TestCase(null, new string[] { null })]
		[TestCase("XYZ", new string[] { null })]
		[TestCase("102", new string[] { null })]
		[TestCase("103", new string[] { "100" })]
		[TestCase("105", new string[] { "140" })]
		[TestCase("106", new string[] { "400" })]
		[TestCase("112", new string[] { "110" })]
		[TestCase("115", new string[] { "115" })]
		[TestCase("117", new string[] { "140" })]
		[TestCase("119", new string[] { "119" })]
		[TestCase("122", new string[] { "120", "125", "128" })]
		[TestCase("123", new string[] { "123" })]
		[TestCase("141", new string[] { "310" })]
		[TestCase("142", new string[] { "200", "300" })]
		[TestCase("143", new string[] { "220", "225", "320", "325" })]
		[TestCase("144", new string[] { "200", "300" })]
		[TestCase("145", new string[] { "240", "340" })]
		[TestCase("146", new string[] { "223", "323" })]
		[TestCase("147", new string[] { "420" })]
		[TestCase("657", new string[] { "240", "340" })]
		[TestCase("658", new string[] { "240", "340" })]
		public void MeasureTypeMapping_GetPreferences(string measureType, string[] expectedPreferences)
		{
			var actual = measureTypeHelper.GetPreferences(measureType);

			Assert.That(actual, Is.Not.Null);
			Assert.That(expectedPreferences, Is.Not.Null);
			Assert.That(actual.Count, Is.EqualTo(expectedPreferences.Length), $"MeasureType: {measureType} Preference count mismatch");

			foreach (var pref in actual)
			{
				Assert.That(expectedPreferences.Contains(pref), $"MeasureType: {measureType} Preference not found: {pref}");
			}
		}

		[Test]
		public void MeasureTypeMapping_AllDutyRateMappingsForA00ShouldHavePreferences()
		{
			var mappings = measureMappingProvider.GetMeasureTypeMappings();

			foreach (var mapping in mappings.Where(x => x.Value.RateType == RateTypes.Duty && !x.Value.Skip))
			{
				if (string.IsNullOrEmpty(mapping.Value.RateCode) || mapping.Value.RateCode == "A00")
				{
					var prefs = mapping.Value.Preferences;

					Assert.That(prefs, Is.Not.Null, $"Null preferences for MeasureType: {mapping.Key}");
					Assert.That(prefs.Count, Is.GreaterThan(0), $"Preferences empty for MeasureType: {mapping.Key}");

					foreach (var p in prefs)
					{
						Assert.That(string.IsNullOrEmpty(p), Is.EqualTo(false), $"Null or empty preference for MeasureType: {mapping.Key}");
					}
				}
			}
		}

		[TestCase(null, false)]
		[TestCase("", false)]
		[TestCase("Unknown", false)]
		[TestCase("103", false)]
		[TestCase("109", true)]
		[TestCase("110", true)]
		public void MeasureTypeMapping_GetSupplementaryUnit(string measureType, bool expectedSupplementaryUnit)
		{
			var actual = measureTypeHelper.IsSupplementaryUnit(measureType);

			Assert.That(actual, Is.EqualTo(expectedSupplementaryUnit), $"MeasureType: {measureType}");
		}

		[TestCase(null, new string[] { })]
		[TestCase("XXX", new string[] { })]
		[TestCase("103", new string[] { "140" })]
		[TestCase("112", new string[] { "115" })]
		[TestCase("122", new string[] { "123" })]
		[TestCase("141", new string[] { "315" })]
		[TestCase("142", new string[] { "240", "340" })]
		[TestCase("143", new string[] { "223", "323" })]
		public void MeasureTypeMapping_AuthoriseUsePreferences(string measureType, string[] expectedPreferences)
		{
			var actual = measureTypeHelper.GetAuthorisedUsePreferences(measureType);

			Assert.That(actual, Is.Not.Null);
			Assert.That(expectedPreferences, Is.Not.Null);
			Assert.That(actual.Count, Is.EqualTo(expectedPreferences.Length), $"MeasureType: {measureType} Authorised Use Preference count mismatch");

			foreach (var pref in actual)
			{
				Assert.That(expectedPreferences.Contains(pref), $"MeasureType: {measureType} Authorised Use Preference not found: {pref}");
			}
		}

		[Test]
		public void MeasureTypeMapping_MappingCounts()
		{
			var mappings = measureMappingProvider.GetMeasureTypeMappings();

			Assert.Multiple(() =>
			{
				Assert.That(mappings.Count, Is.EqualTo(273));

				Assert.That(mappings.Where(x => x.Value.Skip).Count(), Is.EqualTo(31), "Skip = True");

				Assert.That(mappings.Where(x => string.IsNullOrEmpty(x.Value.ConditionClass)).Count(), Is.EqualTo(45), "ConditionClass Empty");
				Assert.That(mappings.Where(x => x.Value.ConditionClass == MeasureHelper.ConditionClass.Class).Count(), Is.EqualTo(3), "ConditionClass Class");
				Assert.That(mappings.Where(x => x.Value.ConditionClass == MeasureHelper.ConditionClass.Control).Count(), Is.EqualTo(89), "ConditionClass Control");
				Assert.That(mappings.Where(x => x.Value.ConditionClass == MeasureHelper.ConditionClass.Rate).Count(), Is.EqualTo(135), "ConditionClass Rate");
				Assert.That(mappings.Where(x => x.Value.ConditionClass == MeasureHelper.ConditionClass.Vat).Count(), Is.EqualTo(1), "ConditionClass Vat");

				Assert.That(mappings.Where(x => string.IsNullOrEmpty(x.Value.RateType)).Count(), Is.EqualTo(138), "RateType Empty");
				Assert.That(mappings.Where(x => x.Value.RateType == RateTypes.Duty).Count(), Is.EqualTo(29), "RateType Duty");
				Assert.That(mappings.Where(x => x.Value.RateType == RateTypes.AntiDumping).Count(), Is.EqualTo(10), "RateType AntiDumping");
				Assert.That(mappings.Where(x => x.Value.RateType == RateTypes.Countervailing).Count(), Is.EqualTo(3), "RateType Countervailing");
				Assert.That(mappings.Where(x => x.Value.RateType == RateTypes.Security).Count(), Is.EqualTo(7), "RateType Security");
				Assert.That(mappings.Where(x => x.Value.RateType == RateTypes.Excises).Count(), Is.EqualTo(71), "RateType Excises");
				Assert.That(mappings.Where(x => x.Value.RateType == RateTypes.Levies).Count(), Is.EqualTo(4), "RateType Levies");
				Assert.That(mappings.Where(x => x.Value.RateType == RateTypes.Export).Count(), Is.EqualTo(11), "RateType Export");
			});
		}

		[TestCase("Use Default", "DEF", "ABC", "", "DEF")]
		[TestCase("Duty rate code", "", "ABC", "", "A00")]
		[TestCase("Excise Default", "", "306", "", "306")]
		[TestCase("Excise Default", "", "306", "X", "306")]
		[TestCase("Exccise Additoinal", "", "306", "XXX", "XXX")]
		public void GetRateCode(string description, string defRateCode, string measureType, string additionalCode, string expectedRateCode)
		{
			var measure = new Measure { MeasureType = measureType, AdditionalCode = additionalCode };

			var actual = measureMappingProvider.ConvertRateCode(defRateCode, measure);

			Assert.That(actual, Is.EqualTo(expectedRateCode), description);
		}

		[TestCase("Standard", 20, MeasureHelper.ConditionClass.Vat, VatCodesAndValues.StandardCode)]
		[TestCase("Reduced", 5, MeasureHelper.ConditionClass.Vat, VatCodesAndValues.ReducedCode)]
		[TestCase("Zero-Rated", 0, MeasureHelper.ConditionClass.Vat, VatCodesAndValues.ZeroRatedCode)]
		[TestCase("Other", 37, MeasureHelper.ConditionClass.Vat, "")]
		[TestCase("Not Vat", 20, MeasureHelper.ConditionClass.Rate, "")]
		[TestCase("No value", null, MeasureHelper.ConditionClass.Vat, "")]
		public void GetVatCode(string descrip, decimal? value, string conClass, string expectedCode)
		{
			var measure = new Measure
			{
				ConditionClass = conClass,
			}
			.SetComponents(new[] { new MeasureComponent { DutyAmount = value, HJID = "1" } });

			Assert.That(measureMappingProvider.GetVatCode(measure), Is.EqualTo(expectedCode), descrip);
		}

		[TestCase("Default", new string[] { "123", "456" }, "ABC", new string[] { "F1", "F2" }, new string[] { "123", "456" })]
		[TestCase("2's no 3's", new string[] { "201", "202", "555" }, "ABC", new string[] { "F1", "F2" }, new string[] { "201", "202", "555" })]
		[TestCase("3's no 2's", new string[] { "301", "302", "555" }, "ABC", new string[] { "F1", "F2" }, new string[] { "301", "302", "555" })]
		[TestCase("2's & 3's - Non-DCTS", new string[] { "201", "202", "301", "302", "555" }, "ABC", new string[] { "F1", "F2" }, new string[] { "301", "302", "555" })]
		[TestCase("2's & 3's - DCTS", new string[] { "201", "202", "301", "302", "555" }, "1060", new string[] { "F1", "F2" }, new string[] { "201", "202", "555" })]
		[TestCase("3rd non-1011 CNSub", new string[] { "100", "456" }, "ABC", new string[] { "CD376", "F2" }, new string[] { "100", "456" })]
		[TestCase("3rd 1011 non-CNSub", new string[] { "100", "456" }, "1011", new string[] { "F1", "F2" }, new string[] { "100", "456" })]
		[TestCase("Non-3rd 1011 CNSub ", new string[] { "123", "456" }, "1011", new string[] { "CD376", "F2" }, new string[] { "123", "456" })]
		[TestCase("3rd 1011 CNSub ", new string[] { "100", "456" }, "1011", new string[] { "CD376", "F2" }, new string[] { "100", "456", "150" })]
		public void GetPreferences(string description, string[] defaultPreferences, string geographicalArea, string[] footnotes, string[] expectedPreferences)
		{
			IMeasureMappingProvider mappingProvider = new MeasureMappingProvider();

			var actual = mappingProvider.ConvertPreferences(defaultPreferences, geographicalArea, footnotes).OrderBy(x => x).ToList();

			Assert.That(actual, Is.EquivalentTo(expectedPreferences), description);
		}

		[Test]
		public void AllPublishedMeasureTypesAreMapped()
		{
			var published = publishedMeasureTypesList.Data;
			var mapped = measureMappingProvider.GetMeasureTypeMappings();

			var unmapped = published
				.Where(x => !mapped.ContainsKey(x.Attributes.Id))
				.Select(x => $"{x.Attributes.Id}: {x.Attributes.Description}")
				.ToList();

			Assert.That(unmapped, Is.Empty, "Unmapped published measure types");
		}

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			measureMappingProvider = new MeasureMappingProvider();
			measureTypeHelper = new MeasureTypeHelper(measureMappingProvider);

			var publishedMeasureTypesJson = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.GBReferenceData.Tests.Tariff.TestFiles.measure_types.json");
			publishedMeasureTypesList = JsonSerializer.Deserialize<PublishedMeasureTypesList>(publishedMeasureTypesJson, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower });
		}

		IMeasureMappingProvider measureMappingProvider;
		MeasureTypeHelper measureTypeHelper;
		PublishedMeasureTypesList publishedMeasureTypesList;

		internal class PublishedMeasureTypeAttributes
		{
			public string Id { get; set; }
			public string Description { get; set; }
			public string MeasureTypeSeriesId { get; set; }
			public string MeasureTypeSeriesDescription { get; set; }
			public int MeasureComponentApplicableCode { get; set; }
			public int OrderNumberCaptureCode { get; set; }
			public int TradeMovementCode { get; set; }
			public DateTime? ValidityStartDate { get; set; }
			public DateTime? ValidityEndDate { get; set; }
		}

		internal class PublishedMeasureType
		{
			public string Id { get; set; }
			public string Type { get; set; }
			public PublishedMeasureTypeAttributes Attributes { get; set; }
		}

		internal class PublishedMeasureTypesList
		{
			public PublishedMeasureType[] Data { get; set; }
		}
	}
}
