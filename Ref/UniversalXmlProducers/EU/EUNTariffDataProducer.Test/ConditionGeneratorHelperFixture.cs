using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer.Test
{
	[TestFixture]
	public class ConditionGeneratorHelperFixture
	{
		[Test]
		public void ConvertCertificateAndFormula()
		{
			var key = new GroupedMeasureConditionKey("465", "", "", DateTime.MinValue, DateTime.MaxValue, "1011", "E", "Restriction on entry into free circulation Regulation 0555/08");
			var data = new List<IRawMeasureConditionRecord>
				{
					new RawMeasureConditionRecord("0102030405", "", "", DateTime.MinValue, DateTime.MaxValue, "1011", "465", "E", "", "100.000", "", "LTR", "29", "Restriction on entry into free circulation Regulation 0555/08"),
					new RawMeasureConditionRecord("0102030405", "", "", DateTime.MinValue, DateTime.MaxValue, "1011", "465", "E", "C014", "", "", "", "29", "Restriction on entry into free circulation Regulation 0555/08"),
					new RawMeasureConditionRecord("0102030405", "", "", DateTime.MinValue, DateTime.MaxValue, "1011", "465", "E", "C015", "", "", "", "29", "Restriction on entry into free circulation Regulation 0555/08"),
					new RawMeasureConditionRecord("0102030405", "", "", DateTime.MinValue, DateTime.MaxValue, "1011", "465", "B", "Y022", "", "", "", "29", "Restriction on entry into free circulation Regulation 0555/08"),
					new RawMeasureConditionRecord("0102030405", "", "", DateTime.MinValue, DateTime.MaxValue, "1011", "465", "Y", "Y021", "", "", "", "29", "Restriction on entry into free circulation Regulation 0555/08"),
					new RawMeasureConditionRecord("0102030405", "", "", DateTime.MinValue, DateTime.MaxValue, "1011", "465", "B", "L136", "", "", "", "29", "Restriction on entry into free circulation Regulation 0555/08"),
					new RawMeasureConditionRecord("0102030405", "", "", DateTime.MinValue, DateTime.MaxValue, "1011", "465", "B", "Y032", "", "", "", "29", "Restriction on entry into free circulation Regulation 0555/08")
				};

			var result = ConditionGeneratorHelper.GetNonRatioConditionValues(key, data);

			Assert.IsNotNull(result);

			var refCusConditionValues = result.ToList();
			Assert.AreEqual(data.Count, refCusConditionValues.Count);

			var record = refCusConditionValues.First();
			Assert.AreEqual("FRM", record.ZX3_ZX4_NKValueType);
			Assert.AreEqual("[LTR] <= 100.000", record.ZX3_Value);

			record = refCusConditionValues[1];
			Assert.AreEqual("SUP", record.ZX3_ZX4_NKValueType);
			Assert.AreEqual("C014", record.ZX3_Value);

			record = refCusConditionValues[2];
			Assert.AreEqual("SUP", record.ZX3_ZX4_NKValueType);
			Assert.AreEqual("C015", record.ZX3_Value);

			record = refCusConditionValues[3];
			Assert.AreEqual("SUP", record.ZX3_ZX4_NKValueType);
			Assert.AreEqual("Y022", record.ZX3_Value);

			record = refCusConditionValues[4];
			Assert.AreEqual("SNR", record.ZX3_ZX4_NKValueType);
			Assert.AreEqual("Y021", record.ZX3_Value);

			record = refCusConditionValues[5];
			Assert.AreEqual("SNR", record.ZX3_ZX4_NKValueType);
			Assert.AreEqual("L136", record.ZX3_Value);

			record = refCusConditionValues.Last();
			Assert.AreEqual("SNR", record.ZX3_ZX4_NKValueType);
			Assert.AreEqual("Y032", record.ZX3_Value);
		}

		[TestCaseSource(nameof(RatioTestCases))]
		public void ConvertRatioFormula(IEnumerable<IRawMeasureConditionRecord> data, string expectedValue, string expectedValueType, string conditionCode, string supplementaryUnit)
		{
			var key = new GroupedMeasureConditionKey("483", "", "", DateTime.MinValue, DateTime.MaxValue, "1011", conditionCode, "Declaration of subheading submitted to restrictions (value) Regulation 0927/12");

			var result = ConditionGeneratorHelper.GetRatioConditionValues(key, data, supplementaryUnit);
			Assert.IsNotNull(result);

			var refCusConditionValues = result.ToList();
			Assert.AreEqual(1, refCusConditionValues.Count);

			var firstRecord = refCusConditionValues.First();
			Assert.AreEqual(expectedValueType, firstRecord.ZX3_ZX4_NKValueType);
			Assert.AreEqual(expectedValue, firstRecord.ZX3_Value);
		}

		protected static IEnumerable RatioTestCases
		{
			get
			{
				var data = new List<IRawMeasureConditionRecord>
				{
					new RawMeasureConditionRecord("0102030405", "", "", DateTime.MinValue, DateTime.MaxValue, "1011", "483", "U", "", "7.900", "EUR", "LPA", "28", "Declaration of subheading submitted to restrictions (value) Regulation 0927/12"),
					new RawMeasureConditionRecord("0102030405", "", "", DateTime.MinValue, DateTime.MaxValue, "1011", "483", "U", "", "0.000", "EUR", "LPA", "10", "Declaration of subheading submitted to restrictions (value) Regulation 0927/12")
				};
				yield return new TestCaseData(data, "VFD/[MTK] > 7.900", "FRM", "U", "MTK") { TestName = "{m}_WhenTwoURatioReturnsOneCondition" };
				data = new List<IRawMeasureConditionRecord>
				{
					new RawMeasureConditionRecord("0102030405", "", "", DateTime.MinValue, DateTime.MaxValue, "1011", "483", "U", "", "7.900", "EUR", "LPA", "10", "Declaration of subheading submitted to restrictions (value) Regulation 0927/12"),
					new RawMeasureConditionRecord("0102030405", "", "", DateTime.MinValue, DateTime.MaxValue, "1011", "483", "U", "", "0.000", "EUR", "LPA", "28", "Declaration of subheading submitted to restrictions (value) Regulation 0927/12")
				};
				yield return new TestCaseData(data, "VFD/[MTK] < 7.900 & VFD/[MTK] > 0.000", "FRM", "U", "MTK") { TestName = "{m}_WhenTwoURatioReturnsTwoConditions" };
				data = new List<IRawMeasureConditionRecord>
				{
					new RawMeasureConditionRecord("0102030405", "", "", DateTime.MinValue, DateTime.MaxValue, "1011", "483", "U", "", "16.001", "", "KGM", "10", "Declaration of subheading submitted to restrictions (value) Regulation 0927/12"),
					new RawMeasureConditionRecord("0102030405", "", "", DateTime.MinValue, DateTime.MaxValue, "1011", "483", "U", "", "80.001", "", "KGM", "28", "Declaration of subheading submitted to restrictions (value) Regulation 0927/12"),
					new RawMeasureConditionRecord("0102030405", "", "", DateTime.MinValue, DateTime.MaxValue, "1011", "483", "U", "", "0.000", "", "KGM", "10", "Declaration of subheading submitted to restrictions (value) Regulation 0927/12")
				};
				yield return new TestCaseData(data, "VFD/[MTK] < 16.001 & VFD/[MTK] > 80.001", "FRM", "U", "MTK") { TestName = "{m}_WhenThreeURatioReturnsTwoConditions" };

				data = new List<IRawMeasureConditionRecord>
				{
					new RawMeasureConditionRecord("0102030405", "", "", DateTime.MinValue, DateTime.MaxValue, "1011", "483", "M", "", "7.900", "EUR", "LPA", "28", "Declaration of subheading submitted to restrictions (value) Regulation 0927/12"),
					new RawMeasureConditionRecord("0102030405", "", "", DateTime.MinValue, DateTime.MaxValue, "1011", "483", "M", "", "0.000", "EUR", "LPA", "10", "Declaration of subheading submitted to restrictions (value) Regulation 0927/12")
				};
				yield return new TestCaseData(data, "VFD/[LPA] >= 7.900", "FRM", "M", "MTK") { TestName = "{m}_WhenTwoMRatioReturnsOneCondition" };
				data = new List<IRawMeasureConditionRecord>
				{
					new RawMeasureConditionRecord("0102030405", "", "", DateTime.MinValue, DateTime.MaxValue, "1011", "483", "M", "", "7.900", "EUR", "LPA", "10", "Declaration of subheading submitted to restrictions (value) Regulation 0927/12"),
					new RawMeasureConditionRecord("0102030405", "", "", DateTime.MinValue, DateTime.MaxValue, "1011", "483", "M", "", "0.001", "EUR", "LPA", "28", "Declaration of subheading submitted to restrictions (value) Regulation 0927/12")
				};
				yield return new TestCaseData(data, "VFD/[LPA] < 7.900 & VFD/[LPA] >= 0.001", "FRM", "M", "MTK") { TestName = "{m}_WhenTwoMRatioReturnsTwoConditions" };
				data = new List<IRawMeasureConditionRecord>
				{
					new RawMeasureConditionRecord("0102030405", "", "", DateTime.MinValue, DateTime.MaxValue, "1011", "483", "M", "", "16.001", "", "KGM", "10", "Declaration of subheading submitted to restrictions (value) Regulation 0927/12"),
					new RawMeasureConditionRecord("0102030405", "", "", DateTime.MinValue, DateTime.MaxValue, "1011", "483", "M", "", "80.001", "", "KGM", "28", "Declaration of subheading submitted to restrictions (value) Regulation 0927/12"),
					new RawMeasureConditionRecord("0102030405", "", "", DateTime.MinValue, DateTime.MaxValue, "1011", "483", "M", "", "0.000", "", "KGM", "10", "Declaration of subheading submitted to restrictions (value) Regulation 0927/12")
				};
				yield return new TestCaseData(data, "VFD/[KGM] < 16.001 & VFD/[KGM] >= 80.001", "FRM", "M", "MTK") { TestName = "{m}_WhenThreeMRatioReturnsTwoConditions" };

				data = new List<IRawMeasureConditionRecord>
				{
					new RawMeasureConditionRecord("0102030405", "", "", DateTime.MinValue, DateTime.MaxValue, "1011", "483", "R", "", "7.900", "EUR", "LPA", "28", "Declaration of subheading submitted to restrictions (value) Regulation 0927/12"),
					new RawMeasureConditionRecord("0102030405", "", "", DateTime.MinValue, DateTime.MaxValue, "1011", "483", "R", "", "0.000", "EUR", "LPA", "10", "Declaration of subheading submitted to restrictions (value) Regulation 0927/12")
				};
				yield return new TestCaseData(data, "[LPA]/[MTK] >= 7.900", "FRM", "R", "MTK") { TestName = "{m}_WhenTwoRRatioReturnsOneConditionW" };
				data = new List<IRawMeasureConditionRecord>
				{
					new RawMeasureConditionRecord("0102030405", "", "", DateTime.MinValue, DateTime.MaxValue, "1011", "483", "R", "", "7.900", "EUR", "LPA", "10", "Declaration of subheading submitted to restrictions (value) Regulation 0927/12"),
					new RawMeasureConditionRecord("0102030405", "", "", DateTime.MinValue, DateTime.MaxValue, "1011", "483", "R", "", "0.001", "EUR", "LPA", "28", "Declaration of subheading submitted to restrictions (value) Regulation 0927/12")
				};
				yield return new TestCaseData(data, "[LPA]/[MTK] < 7.900 & [LPA]/[MTK] >= 0.001", "FRM", "R", "MTK") { TestName = "{m}_WhenTwoRRatioReturnsTwoConditions" };
				data = new List<IRawMeasureConditionRecord>
				{
					new RawMeasureConditionRecord("0102030405", "", "", DateTime.MinValue, DateTime.MaxValue, "1011", "483", "U", "", "16.001", "", "KGM", "10", "Declaration of subheading submitted to restrictions (value) Regulation 0927/12"),
					new RawMeasureConditionRecord("0102030405", "", "", DateTime.MinValue, DateTime.MaxValue, "1011", "483", "U", "", "80.001", "", "KGM", "28", "Declaration of subheading submitted to restrictions (value) Regulation 0927/12"),
					new RawMeasureConditionRecord("0102030405", "", "", DateTime.MinValue, DateTime.MaxValue, "1011", "483", "U", "", "0.000", "", "KGM", "10", "Declaration of subheading submitted to restrictions (value) Regulation 0927/12")
				};
				yield return new TestCaseData(data, "[KGM]/[MTK] < 16.001 & [KGM]/[MTK] >= 80.001", "FRM", "R", "MTK") { TestName = "{m}_WhenThreeRRatioReturnsTwoConditions" };

				data = new List<IRawMeasureConditionRecord>
				{
					new RawMeasureConditionRecord("0102030405", "", "", DateTime.MinValue, DateTime.MaxValue, "1011", "483", "M", "", "7.900", "EUR", "LPA", "28", "Declaration of subheading submitted to restrictions (value) Regulation 0927/12"),
					new RawMeasureConditionRecord("0102030405", "", "", DateTime.MinValue, DateTime.MaxValue, "1011", "483", "M", "", "0.001", "EUR", "LPA", "01", "Declaration of subheading submitted to restrictions (value) Regulation 0927/12")
				};
				yield return new TestCaseData(data, "VFD/[LPA] >= 7.900", "FRM", "M", "MTK") { TestName = "{m}_WhenTwoMRatioReturnsOneCondition2" };
			}
		}

		[Test]
		public void ConvertMTypeRatioFormulaWithFiveRatios()
		{
			var key = new GroupedMeasureConditionKey("483", "", "", DateTime.MinValue, DateTime.MaxValue, "1011", "M", "Declaration of subheading submitted to restrictions (value) Regulation 0927/12");
			var data = new List<IRawMeasureConditionRecord>
			{
				new RawMeasureConditionRecord("0102030405", "", "", DateTime.MinValue, DateTime.MaxValue, "1011", "483", "M", "", "46.301", "", "KGM", "10", "Declaration of subheading submitted to restrictions (value) Regulation 0927/12"),
				new RawMeasureConditionRecord("0102030405", "", "", DateTime.MinValue, DateTime.MaxValue, "1011", "483", "M", "", "46.300", "", "KGM", "28", "Declaration of subheading submitted to restrictions (value) Regulation 0927/12"),
				new RawMeasureConditionRecord("0102030405", "", "", DateTime.MinValue, DateTime.MaxValue, "1011", "483", "M", "", "45.901", "", "KGM", "10", "Declaration of subheading submitted to restrictions (value) Regulation 0927/12"),
				new RawMeasureConditionRecord("0102030405", "", "", DateTime.MinValue, DateTime.MaxValue, "1011", "483", "M", "", "45.900", "", "KGM", "28", "Declaration of subheading submitted to restrictions (value) Regulation 0927/12"),
				new RawMeasureConditionRecord("0102030405", "", "", DateTime.MinValue, DateTime.MaxValue, "1011", "483", "M", "", "0.000", "", "KGM", "10", "Declaration of subheading submitted to restrictions (value) Regulation 0927/12")
			};

			var result = ConditionGeneratorHelper.GetRatioConditionValues(key, data, string.Empty);
			Assert.IsNotNull(result);

			var refCusConditionValues = result.ToList();
			Assert.AreEqual(2, refCusConditionValues.Count);

			var firstRecord = refCusConditionValues.First();
			Assert.AreEqual("FRM", firstRecord.ZX3_ZX4_NKValueType);
			Assert.AreEqual("VFD/[KGM] = 46.300", firstRecord.ZX3_Value);

			var lastRecord = refCusConditionValues.Last();
			Assert.AreEqual("FRM", lastRecord.ZX3_ZX4_NKValueType);
			Assert.AreEqual("VFD/[KGM] = 45.900", lastRecord.ZX3_Value);
		}

		[Test]
		public void ConvertRTypeRatioFormulaWithFiveRatios()
		{
			var key = new GroupedMeasureConditionKey("483", "", "", DateTime.MinValue, DateTime.MaxValue, "1011", "R", "Declaration of subheading submitted to restrictions (value) Regulation 0927/12");
			var data = new List<IRawMeasureConditionRecord>
			{
				new RawMeasureConditionRecord("0102030405", "", "", DateTime.MinValue, DateTime.MaxValue, "1011", "483", "R", "", "46.301", "", "KGM", "10", "Declaration of subheading submitted to restrictions (value) Regulation 0927/12"),
				new RawMeasureConditionRecord("0102030405", "", "", DateTime.MinValue, DateTime.MaxValue, "1011", "483", "R", "", "46.300", "", "KGM", "28", "Declaration of subheading submitted to restrictions (value) Regulation 0927/12"),
				new RawMeasureConditionRecord("0102030405", "", "", DateTime.MinValue, DateTime.MaxValue, "1011", "483", "R", "", "45.901", "", "KGM", "10", "Declaration of subheading submitted to restrictions (value) Regulation 0927/12"),
				new RawMeasureConditionRecord("0102030405", "", "", DateTime.MinValue, DateTime.MaxValue, "1011", "483", "R", "", "45.900", "", "KGM", "28", "Declaration of subheading submitted to restrictions (value) Regulation 0927/12"),
				new RawMeasureConditionRecord("0102030405", "", "", DateTime.MinValue, DateTime.MaxValue, "1011", "483", "R", "", "0.000", "", "KGM", "10", "Declaration of subheading submitted to restrictions (value) Regulation 0927/12")
			};

			var result = ConditionGeneratorHelper.GetRatioConditionValues(key, data, "MTK");
			Assert.IsNotNull(result);

			var refCusConditionValues = result.ToList();
			Assert.AreEqual(2, refCusConditionValues.Count);

			var firstRecord = refCusConditionValues.First();
			Assert.AreEqual("FRM", firstRecord.ZX3_ZX4_NKValueType);
			Assert.AreEqual("[KGM]/[MTK] = 46.300", firstRecord.ZX3_Value);

			var lastRecord = refCusConditionValues.Last();
			Assert.AreEqual("FRM", lastRecord.ZX3_ZX4_NKValueType);
			Assert.AreEqual("[KGM]/[MTK] = 45.900", lastRecord.ZX3_Value);
		}

		[Test]
		public void ConvertUTypeRatioFormulaWithFiveRatios()
		{
			var key = new GroupedMeasureConditionKey("483", "", "", DateTime.MinValue, DateTime.MaxValue, "1011", "U", "Declaration of subheading submitted to restrictions (value) Regulation 0927/12");
			var data = new List<IRawMeasureConditionRecord>
			{
				new RawMeasureConditionRecord("0102030405", "", "", DateTime.MinValue, DateTime.MaxValue, "1011", "483", "U", "", "46.301", "", "KGM", "10", "Declaration of subheading submitted to restrictions (value) Regulation 0927/12"),
				new RawMeasureConditionRecord("0102030405", "", "", DateTime.MinValue, DateTime.MaxValue, "1011", "483", "U", "", "46.300", "", "KGM", "28", "Declaration of subheading submitted to restrictions (value) Regulation 0927/12"),
				new RawMeasureConditionRecord("0102030405", "", "", DateTime.MinValue, DateTime.MaxValue, "1011", "483", "U", "", "45.901", "", "KGM", "10", "Declaration of subheading submitted to restrictions (value) Regulation 0927/12"),
				new RawMeasureConditionRecord("0102030405", "", "", DateTime.MinValue, DateTime.MaxValue, "1011", "483", "U", "", "45.900", "", "KGM", "28", "Declaration of subheading submitted to restrictions (value) Regulation 0927/12"),
				new RawMeasureConditionRecord("0102030405", "", "", DateTime.MinValue, DateTime.MaxValue, "1011", "483", "U", "", "0.000", "", "KGM", "10", "Declaration of subheading submitted to restrictions (value) Regulation 0927/12")
			};

			var result = ConditionGeneratorHelper.GetRatioConditionValues(key, data, "MTK");
			Assert.IsNotNull(result);

			var refCusConditionValues = result.ToList();
			Assert.AreEqual(2, refCusConditionValues.Count);

			var firstRecord = refCusConditionValues.First();
			Assert.AreEqual("FRM", firstRecord.ZX3_ZX4_NKValueType);
			Assert.AreEqual("VFD/[MTK] = 46.300", firstRecord.ZX3_Value);

			var lastRecord = refCusConditionValues.Last();
			Assert.AreEqual("FRM", lastRecord.ZX3_ZX4_NKValueType);
			Assert.AreEqual("VFD/[MTK] = 45.900", lastRecord.ZX3_Value);
		}
	}
}
