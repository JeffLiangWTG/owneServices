using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer.Test
{
	[TestFixture]
	public class ConditionGeneratorFixture
	{
		[TestCaseSource(nameof(ConvertTestCases))]
		public void Convert(string conditionAmount, string monetaryUnit, string measureUnit, string expectedComment)
		{
			var conditionValueDescriptionExtractor = new Mock<IConditionValueDescriptionExtractor>();
			conditionValueDescriptionExtractor.Setup(x => x.GetComment("B")).Returns("Condition B:Presentation of a certificate/licence/document");

			var conditionGenerator = new ConditionGenerator(conditionValueDescriptionExtractor.Object);
			var measureConditionRecords = new[]
			{
				new RawMeasureConditionRecord("0101290000", null, null, DateTime.MinValue, DateTime.MaxValue, "1006", "142", "B",
					"U088", conditionAmount, monetaryUnit, measureUnit, "27", string.Empty),
				new RawMeasureConditionRecord("0101290000", null, null, DateTime.MinValue, DateTime.MaxValue, "1006", "142", "B",
					"U092", conditionAmount, monetaryUnit, measureUnit, "27", string.Empty)
			};
			var key = new GroupedMeasureConditionKey("142", null, null, DateTime.MinValue, DateTime.MaxValue, "1006", "B", string.Empty);
			var result = conditionGenerator.Convert(key, measureConditionRecords, null, null);
			Assert.NotNull(result);

			Assert.AreEqual(DateTime.MinValue, result.ZX1_StartDate);
			Assert.AreEqual(DateTime.MaxValue, result.ZX1_EndDate);
			Assert.AreEqual("142", result.ZX1_ZX2_NKConditionType);
			Assert.AreEqual(expectedComment, result.ZX1_Comment);

			Assert.AreEqual(result.RefCusApplicabilities.Length, 1);
			var applicability = result.RefCusApplicabilities.First();
			Assert.AreEqual("1006", applicability.ZZT_ZZA_NKTradeGroup);

			Assert.AreEqual(result.RefCusConditionValues.Length, 2);
			var conditionValue = result.RefCusConditionValues.First();
			Assert.AreEqual("SNR", conditionValue.ZX3_ZX4_NKValueType);
			Assert.AreEqual("U088", conditionValue.ZX3_Value);
			var conditionValue2 = result.RefCusConditionValues.Last();
			Assert.AreEqual("SNR", conditionValue2.ZX3_ZX4_NKValueType);
			Assert.AreEqual("U092", conditionValue2.ZX3_Value);
		}

		[Test]
		public void ConvertNoOtherCondition()
		{
			var conditionValueDescriptionExtractor = new Mock<IConditionValueDescriptionExtractor>();
			conditionValueDescriptionExtractor.Setup(x => x.GetComment(ApplicationConfig.NotApplicableForNoOtherCondition)).Returns(string.Empty);

			var conditionGenerator = new ConditionGenerator(conditionValueDescriptionExtractor.Object);
			var measureConditionRecords = new[]
			{
				new RawMeasureConditionRecord("1516209821", null, null, DateTime.MinValue, DateTime.MaxValue, "1006", "552", ApplicationConfig.NotApplicableForNoOtherCondition,
					string.Empty, string.Empty, string.Empty, string.Empty, ApplicationConfig.NotApplicableForNoOtherCondition, string.Empty)
			};
			var key = new GroupedMeasureConditionKey("552", "A999", null, DateTime.MinValue, DateTime.MaxValue, "1006", ApplicationConfig.NotApplicableForNoOtherCondition, string.Empty);
			var result = conditionGenerator.Convert(key, measureConditionRecords, null, null);
			Assert.NotNull(result);

			Assert.AreEqual(DateTime.MinValue, result.ZX1_StartDate);
			Assert.AreEqual(DateTime.MaxValue, result.ZX1_EndDate);
			Assert.AreEqual("552", result.ZX1_ZX2_NKConditionType);
			Assert.AreEqual(ApplicationConfig.NoOtherConditionAppliedComment, result.ZX1_Comment);

			Assert.AreEqual(result.RefCusApplicabilities.Length, 1);
			var applicability = result.RefCusApplicabilities.First();
			Assert.AreEqual("1006", applicability.ZZT_ZZA_NKTradeGroup);
			Assert.AreEqual("A999", applicability.ZZT_AdditionalCode);

			Assert.IsNull(result.RefCusConditionValues);
		}

		protected static IEnumerable ConvertTestCases
		{
			get
			{
				yield return new TestCaseData(null, null, null, "Condition B:Presentation of a certificate/licence/document")
				{
					TestName = "{m}_SimpleComment"
				};
				yield return new TestCaseData("100", "EUR", null, "Condition B:Presentation of a certificate/licence/document")
				{
					TestName = "{m}_CommentWithAmount&MonetaryUnit"
				};
				yield return new TestCaseData("100", "EUR", "TNE", "Condition B:Presentation of a certificate/licence/document")
				{
					TestName = "{m}_CommentWithAmountMonetaryUnit&MeasureUnit"
				};
				yield return new TestCaseData("100", null, "DTN", "Condition B:Presentation of a certificate/licence/document")
				{
					TestName = "{m}_CommentWithAmount&MeasureUnit"
				};
			}
		}

		[Test]
		public void ConvertNoOtherConditionDirectionFollowsKeyDirection()
		{
			var conditionValueDescriptionExtractor = new Mock<IConditionValueDescriptionExtractor>();
			conditionValueDescriptionExtractor.Setup(x => x.GetComment(ApplicationConfig.NotApplicableForNoOtherCondition)).Returns(string.Empty);
			var conditionGenerator = new ConditionGenerator(conditionValueDescriptionExtractor.Object);

			var measureConditionRecords = new[]
{
				new RawMeasureConditionRecord("5702420000", null, null, DateTime.MinValue, DateTime.MaxValue, "1008", "467", ApplicationConfig.NotApplicableForNoOtherCondition,
					string.Empty, string.Empty, string.Empty, string.Empty, ApplicationConfig.NotApplicableForNoOtherCondition, string.Empty, false)
			};

			var key1 = new GroupedMeasureConditionKey("467", "", "", DateTime.MinValue, DateTime.MaxValue, "1008", ApplicationConfig.NotApplicableForNoOtherCondition, string.Empty, false);
			var result = conditionGenerator.Convert(key1, measureConditionRecords, null, null);

			Assert.AreEqual(false, result.ZX1_IsImport);
			Assert.AreEqual(true, result.ZX1_IsExport);
		}

		[Test]
		public void ConvertMeasureConditionsDirectionFollowsKeyDirection()
		{
			var conditionValueDescriptionExtractor = new Mock<IConditionValueDescriptionExtractor>();
			var conditionGenerator = new ConditionGenerator(conditionValueDescriptionExtractor.Object);

			conditionValueDescriptionExtractor.Setup(x => x.GetComment("B")).Returns("Condition B:Presentation of a certificate/licence/document");
			var data = new List<IRawMeasureConditionRecord>
			{
				new RawMeasureConditionRecord("0102030405", "", "", DateTime.MinValue, DateTime.MaxValue, "1011", "410", "B", "C640", "", "", "", "29", "Veterinary control Decision 0275/07")
			};

			var key1 = new GroupedMeasureConditionKey("410", "", "", DateTime.MinValue, DateTime.MaxValue, "1011", "B", "Veterinary control Decision 0275/07", false);
			var result = conditionGenerator.Convert(key1, data, null, null);
			Assert.AreEqual(false, result.ZX1_IsImport);
			Assert.AreEqual(true, result.ZX1_IsExport);

			var key2 = new GroupedMeasureConditionKey("410", "", "", DateTime.MinValue, DateTime.MaxValue, "1011", "B", "Veterinary control Decision 0275/07", true);
			result = conditionGenerator.Convert(key2, data, null, null);
			Assert.AreEqual(true, result.ZX1_IsImport);
			Assert.AreEqual(false, result.ZX1_IsExport);
		}

		[Test]
		public void ConvertMeasureConditionsFromImportRate()
		{
			var conditionValueDescriptionExtractor = new Mock<IConditionValueDescriptionExtractor>();
			var conditionGenerator = new ConditionGenerator(conditionValueDescriptionExtractor.Object);

			conditionValueDescriptionExtractor.Setup(x => x.GetComment("B")).Returns("Condition B:Presentation of a certificate/licence/document");

			var key = new GroupedMeasureConditionKey("410", "", "", DateTime.MinValue, DateTime.MaxValue, "1011", "B", "Veterinary control Decision 0275/07");
			var data = new List<IRawMeasureConditionRecord>
			{
				new RawMeasureConditionRecord("0102030405", "", "", DateTime.MinValue, DateTime.MaxValue, "1011", "410", "B", "C640", "", "", "", "29", "Veterinary control Decision 0275/07"),
				new RawMeasureConditionRecord("0102030405", "", "", DateTime.MinValue, DateTime.MaxValue, "1011", "410", "B", "Y078", "", "", "", "29", "Veterinary control Decision 0275/07"),
				new RawMeasureConditionRecord("0102030405", "", "", DateTime.MinValue, DateTime.MaxValue, "1011", "410", "B", "Y079", "", "", "", "29", "Veterinary control Decision 0275/07")
			};

			var result = conditionGenerator.Convert(key, data, null, null);
			Assert.IsNotNull(result);
			Assert.IsNotNull(result.RefCusConditionValues);
			Assert.True(result.RefCusConditionValues.Any());

			Assert.AreEqual(DateTime.MinValue, result.ZX1_StartDate);
			Assert.AreEqual(DateTime.MaxValue, result.ZX1_EndDate);
			Assert.AreEqual(true, result.ZX1_IsImport);
			Assert.AreEqual(false, result.ZX1_IsExport);
			Assert.AreEqual("410", result.ZX1_ZX2_NKConditionType);
			Assert.AreEqual("Condition B:Presentation of a certificate/licence/document", result.ZX1_Comment);

			Assert.AreEqual(result.RefCusApplicabilities.Length, 1);
			var applicability = result.RefCusApplicabilities.First();
			Assert.AreEqual("1011", applicability.ZZT_ZZA_NKTradeGroup);
			Assert.IsNull(applicability.RefCusExcludedTradeGroups);

			var refCusConditionValues = result.RefCusConditionValues.ToList();
			Assert.AreEqual(3, refCusConditionValues.Count);

			var firstRecord = refCusConditionValues.First();
			Assert.AreEqual("SUP", firstRecord.ZX3_ZX4_NKValueType);
			Assert.AreEqual("C640", firstRecord.ZX3_Value);

			var lastRecord = refCusConditionValues.Last();
			Assert.AreEqual("SNR", lastRecord.ZX3_ZX4_NKValueType);
			Assert.AreEqual("Y079", lastRecord.ZX3_Value);
		}

		[Test]
		public void ConvertMeasureConditionsFromRate_WithExcludedTradeGroup()
		{
			var conditionValueDescriptionExtractor = new Mock<IConditionValueDescriptionExtractor>();
			var conditionGenerator = new ConditionGenerator(conditionValueDescriptionExtractor.Object);

			var key = new GroupedMeasureConditionKey("410", "", "", DateTime.MinValue, DateTime.MaxValue, "1011", "B", "Veterinary control Decision 0275/07");
			var data = new List<IRawMeasureConditionRecord>
			{
				new RawMeasureConditionRecord("0102030405", "", "", DateTime.MinValue, DateTime.MaxValue, "1011", "410", "B", "C640", "", "", "", "29", "Veterinary control Decision 0275/07")
			};

			var result = conditionGenerator.Convert(key, data, new[] { "AU", "NZ" }, null);
			Assert.IsNotNull(result);
			Assert.IsNotNull(result.RefCusConditionValues);
			Assert.True(result.RefCusConditionValues.Any());

			Assert.AreEqual(result.RefCusApplicabilities.Length, 1);
			var applicability = result.RefCusApplicabilities.First();
			Assert.AreEqual("1011", applicability.ZZT_ZZA_NKTradeGroup);
			Assert.IsNotNull(applicability.RefCusExcludedTradeGroups);

			var refCusExcludedTradeGroups = applicability.RefCusExcludedTradeGroups;
			Assert.AreEqual(2, refCusExcludedTradeGroups.Length);
			Assert.AreEqual("AU", refCusExcludedTradeGroups.First().ZZC_ZZA_NKTradeGroup);
			Assert.AreEqual("NZ", refCusExcludedTradeGroups.Last().ZZC_ZZA_NKTradeGroup);

			var refCusConditionValues = result.RefCusConditionValues.ToList();
			Assert.AreEqual(1, refCusConditionValues.Count);
		}

		[Test]
		public void ConvertMeasureConditionsFromRate_OperatorFromMeasureCode()
		{
			var conditionValueDescriptionExtractor = new Mock<IConditionValueDescriptionExtractor>();
			var conditionGenerator = new ConditionGenerator(conditionValueDescriptionExtractor.Object);

			var key = new GroupedMeasureConditionKey("410", "", "", DateTime.MinValue, DateTime.MaxValue, "1011", "E", "Veterinary control Decision 0275/07");
			var data = new List<IRawMeasureConditionRecord>
			{
				new RawMeasureConditionRecord("0102030405", "", "", DateTime.MinValue, DateTime.MaxValue, "1011", "483", "E", "", "7.900", "EUR", "LPA", "28", "Declaration of subheading submitted to restrictions (value) Regulation 0927/12")
			};

			var result = conditionGenerator.Convert(key, data, null, null);
			Assert.IsNotNull(result);
			Assert.AreEqual("[LPA] <= 7.900", result.RefCusConditionValues.First().ZX3_Value);

			key = new GroupedMeasureConditionKey("410", "", "", DateTime.MinValue, DateTime.MaxValue, "1011", "I", "Veterinary control Decision 0275/07");
			data = new List<IRawMeasureConditionRecord>
			{
				new RawMeasureConditionRecord("0102030405", "", "", DateTime.MinValue, DateTime.MaxValue, "1011", "483", "I", "", "7.900", "EUR", "LPA", "28", "Declaration of subheading submitted to restrictions (value) Regulation 0927/12")
			};

			result = conditionGenerator.Convert(key, data, null, null);
			Assert.IsNotNull(result);
			Assert.AreEqual("[LPA] <= 7.900", result.RefCusConditionValues.First().ZX3_Value);
		}

		[Test]
		public void ConvertMeasureConditionsFromRate_ConditionValueIncorrectEmptyFormula()
		{
			var conditionValueDescriptionExtractor = new Mock<IConditionValueDescriptionExtractor>();
			var conditionGenerator = new ConditionGenerator(conditionValueDescriptionExtractor.Object);

			var key = new GroupedMeasureConditionKey("552", "", "", DateTime.MinValue, DateTime.MaxValue, "1011", "A", "");
			var data = new List<IRawMeasureConditionRecord>
			{
				new RawMeasureConditionRecord("7607119046", "", "", DateTime.MinValue, DateTime.MaxValue, "1011", "552", "A", "D019", "", "EUR", "", "07", ""),
				new RawMeasureConditionRecord("7607119046", "", "", DateTime.MinValue, DateTime.MaxValue, "1011", "552", "A", "", "", "EUR", "", "27", "")
			};

			var result = conditionGenerator.Convert(key, data, null, null);
			Assert.IsNotNull(result);
			Assert.That(result.RefCusConditionValues.Count, Is.EqualTo(2));
			Assert.That(result.RefCusConditionValues.First().ZX3_Value, Is.Not.EqualTo("[] <= "));
			Assert.That(result.RefCusConditionValues.First().ZX3_Value, Is.EqualTo("D019"));

			Assert.That(result.RefCusConditionValues.Last().ZX3_ZX4_NKValueType, Is.EqualTo("INF"));
			Assert.That(result.RefCusConditionValues.Last().ZX3_Value, Is.EqualTo("Not presented"));
		}
	}
}
