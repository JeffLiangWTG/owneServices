using System.Globalization;
using CargoWise.RefDbRepo.PLReferenceData.Business.Tariff;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.PLReferenceData.Tests.Tariff
{
	[TestFixture]
	sealed class RateFormulaGeneratorTest
	{
		[Test]
		public void TestOnMeasureUnitCode_EmptyMeasureUnitCode()
		{
			CultureInfo.CurrentCulture = new CultureInfo("en-US", false);
			var data = new measureComponent()
			{
				measurementUnit = new measurementUnit()
				{
					measurementUnitCode = ""
				},
				dutyAmount = 9.2m
			};
			var expected = $"0.092 * {Constants.CW1ValueForDuty}";
			string result = RateFormulaGenerator.OnMeasureUnitCode(data);
			Assert.AreEqual(expected, result);
		}

		[Test]
		public void TestOnMeasureUnitCode_NotEmptyMeasureUnitCode()
		{
			CultureInfo.CurrentCulture = new CultureInfo("en-US", false);
			var data = new measureComponent()
			{
				measurementUnit = new measurementUnit()
				{
					measurementUnitCode = "KG"
				},
				dutyAmount = 9.2m
			};
			var expected = "9.2 * [KG]";
			string result = RateFormulaGenerator.OnMeasureUnitCode(data);
			Assert.AreEqual(expected, result);
		}

		[Test]
		public void TestOnExpressionId_Maximum_EmptyMeasureUnitCode()
		{
			CultureInfo.CurrentCulture = new CultureInfo("en-US", false);
			var data = new measureComponent()
			{
				measurementUnit = new measurementUnit()
				{
					measurementUnitCode = ""
				},
				dutyAmount = 1.0m
			};
			string basicFormula = "9.2 * [KG]";
			var expected = $"MIN(0.01 * {Constants.CW1ValueForDuty}, {basicFormula})";
			var result = RateFormulaGenerator.OnExpressionId_MaximumGetMin(data, basicFormula);
			Assert.AreEqual(expected, result);
		}

		[Test]
		public void TestOnExpressionId_Maximum_NotEmptyMeasureUnitCode()
		{
			CultureInfo.CurrentCulture = new CultureInfo("en-US", false);
			var data = new measureComponent()
			{
				measurementUnit = new measurementUnit()
				{
					measurementUnitCode = "KG"
				},
				dutyAmount = 1.0m
			};
			string basicFormula = "9.2 * [KG]";
			var expected = $"MIN(1 * [KG], {basicFormula})";
			var result = RateFormulaGenerator.OnExpressionId_MaximumGetMin(data, basicFormula);
			Assert.AreEqual(expected, result);
		}

		[Test]
		public void TestOnExpressionId_Minimum_EmptyMeasureUnitCode()
		{
			CultureInfo.CurrentCulture = new CultureInfo("en-US", false);
			var data = new measureComponent()
			{
				measurementUnit = new measurementUnit()
				{
					measurementUnitCode = ""
				},
				dutyAmount = 1.0m
			};
			string basicFormula = "9.2 * [KG]";
			var expected = $"MAX(0.01 * {Constants.CW1ValueForDuty}, {basicFormula})";
			var result = RateFormulaGenerator.OnExpressionId_MinimumGetMax(data, basicFormula);
			Assert.AreEqual(expected, result);
		}

		[Test]
		public void TestOnExpressionId_Minimum_NotEmptyMeasureUnitCode()
		{
			CultureInfo.CurrentCulture = new CultureInfo("en-US", false);
			var data = new measureComponent()
			{
				measurementUnit = new measurementUnit()
				{
					measurementUnitCode = "KG"
				},
				dutyAmount = 1.0m
			};
			string basicFormula = "9.2 * [KG]";
			var expected = $"MAX(1 * [KG], {basicFormula})";
			var result = RateFormulaGenerator.OnExpressionId_MinimumGetMax(data, basicFormula);
			Assert.AreEqual(expected, result);
		}

		[Test]
		public void TestOnExpressionId_04_EmptyMeasureUnitCode()
		{
			CultureInfo.CurrentCulture = new CultureInfo("en-US", false);
			var data = new measureComponent()
			{
				measurementUnit = new measurementUnit()
				{
					measurementUnitCode = ""
				},
				dutyAmount = 1.0m
			};
			string basicFormula = "9.2 * [KG]";
			var expected = $"{basicFormula} + 0.01 * {Constants.CW1ValueForDuty}";
			var result = RateFormulaGenerator.OnExpressionId_04GetSum(data, basicFormula);
			Assert.AreEqual(expected, result);
		}

		[Test]
		public void TestOnExpressionId_04_NotEmptyMeasureUnitCode()
		{
			CultureInfo.CurrentCulture = new CultureInfo("en-US", false);
			var data = new measureComponent()
			{
				measurementUnit = new measurementUnit()
				{
					measurementUnitCode = "KG"
				},
				dutyAmount = 1.0m
			};
			string basicFormula = "9.2 * [KG]";
			var expected = $"{basicFormula} + 1 * [KG]";
			var result = RateFormulaGenerator.OnExpressionId_04GetSum(data, basicFormula);
			Assert.AreEqual(expected, result);
		}

		[Test]
		public void TestOnExpressionId_01_EmptyMeasureUnitCode()
		{
			CultureInfo.CurrentCulture = new CultureInfo("en-US", false);
			var data = new measureComponent()
			{
				measurementUnit = new measurementUnit()
				{
					measurementUnitCode = ""
				},
				dutyAmount = 1.0m
			};
			var expected = $"0.01 * {Constants.CW1ValueForDuty}";
			var result = RateFormulaGenerator.OnExpressionId_01GetValue(data);
			Assert.AreEqual(expected, result);
		}

		[Test]
		public void TestOnExpressionId_01_NotEmptyMeasureUnitCode()
		{
			CultureInfo.CurrentCulture = new CultureInfo("en-US", false);
			var data = new measureComponent()
			{
				measurementUnit = new measurementUnit()
				{
					measurementUnitCode = "KG"
				},
				dutyAmount = 1.0m
			};
			var expected = $"1 * [KG]";
			var result = RateFormulaGenerator.OnExpressionId_01GetValue(data);
			Assert.AreEqual(expected, result);
		}

		[Test]
		public void TestGenerateRateFormulaFromMeasure()
		{
			CultureInfo.CurrentCulture = new CultureInfo("en-US", false);
			var data = new measure()
			{
				measureComponent = new measureComponent[]
				{
					new measureComponent()
					{
						dutyExpression = new dutyExpression()
						{
							dutyExpressionId = Constants.DutyExpressionId.ExpressionId_01
						},
						measurementUnit = new measurementUnit()
						{
							measurementUnitCode = "KG"
						},
						dutyAmount = 1.0m
					},
					new measureComponent()
					{
						dutyExpression = new dutyExpression()
						{
							dutyExpressionId = Constants.DutyExpressionId.ExpressionId_04
						},
						measurementUnit = new measurementUnit()
						{
							measurementUnitCode = ""
						},
						dutyAmount = 9.0m
					},
					new measureComponent()
					{
						dutyExpression = new dutyExpression()
						{
							dutyExpressionId = Constants.DutyExpressionId.ExpressionId_15
						},
						measurementUnit = new measurementUnit()
						{
							measurementUnitCode = "KG"
						},
						dutyAmount = 9.0m
					},
					new measureComponent()
					{
						dutyExpression = new dutyExpression()
						{
							dutyExpressionId = Constants.DutyExpressionId.ExpressionId_35
						},
						measurementUnit = new measurementUnit()
						{
							measurementUnitCode = ""
						},
						dutyAmount = 2.0m
					}
				}
			};
			var expected = $"MIN(0.02 * VFD, MAX(9 * [KG], 1 * [KG] + 0.09 * VFD))";
			var result = RateFormulaGenerator.GenerateRateFormulaFromMeasure(data);
			Assert.AreEqual(expected, result);
		}
	}
}

