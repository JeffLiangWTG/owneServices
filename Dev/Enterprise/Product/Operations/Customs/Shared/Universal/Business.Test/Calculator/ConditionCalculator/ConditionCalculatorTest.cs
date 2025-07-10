using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	public class ConditionCalculatorTest : TestCase
	{
		public void TestCaclculate()
		{
			var testFormulas = new Dictionary<string, bool>();
			testFormulas.Add("0.8 * VFD < 1000", true);
			testFormulas.Add("7% * VFD > 80", false);
			FillRealTestFormulas(testFormulas);
			CombineAssertions(() =>
			{
				foreach (var testKey in testFormulas.Keys)
				{
					try
					{
						var expectedResult = testFormulas[testKey];
						var testRateData = new UniversalRateDataForTestWithData();
						var calculator = new ConditionCalculator(testKey, testRateData);
						testRateData.AddDefaultFormulaSpecificAnswer(calculator.AddAnswer);
						Assert(testKey + " SyntaxError", !calculator.HasParseError);
						AssertEquals(string.Format("{0}=>{1}", testKey, expectedResult), expectedResult, calculator.Evaluate());
					}
					catch (Exception e)
					{
						AssertEquals(testKey + " Exception", string.Empty, e.Message);
					}
				}
			}

			);
		}

		public void TestCaclculate_AbnormalNumbers()
		{
			CombineAssertions("Test_1", () =>
			{
				var testFormula = "99999999999999999999999999999999999999999999999<1";
				var testRateData = new UniversalRateDataForTestWithData();
				var expectedResult = false;
				var calculator = new ConditionCalculator(testFormula, testRateData);
				Assert("No Error Before Calculation", !calculator.HasParseError);
				AssertEquals("Retrun 0 as Default", expectedResult, calculator.Evaluate());
				Assert("Has Error After Calculation", calculator.HasParseError);
				AssertEquals("Has 1 Error", 1, calculator.Errors.Count());
				AssertEquals("Has Calculation Error", FormulaVisitErrorType.SyntaxError, calculator.Errors.First().Type);
				AssertEquals("ErrorMessage", "Failed to convert \"99999999999999999999999999999999999999999999999\" to Decimal", calculator.Errors.First().ErrorMessage);
			}

			);
			CombineAssertions("Test_2", () =>
			{
				var testFormula = "99999999999999999999999999999999999999999999999*999999<1";
				var testRateData = new UniversalRateDataForTestWithData();
				var expectedResult = false;
				var calculator = new ConditionCalculator(testFormula, testRateData);
				Assert("No Error Before Calculation", !calculator.HasParseError);
				AssertEquals("Retrun 0 as Default", expectedResult, calculator.Evaluate());
				Assert("Has Error After Calculation", calculator.HasParseError);
				AssertEquals("Has 1 Error", 1, calculator.Errors.Count());
				AssertEquals("Has Calculation Error", FormulaVisitErrorType.SyntaxError, calculator.Errors.First().Type);
				AssertEquals("ErrorMessage", "Failed to convert \"99999999999999999999999999999999999999999999999\" to Decimal", calculator.Errors.First().ErrorMessage);
			}

			);
			CombineAssertions("Test_3", () =>
			{
				var testFormula = "999999/0<1";
				var testRateData = new UniversalRateDataForTestWithData();
				var expectedResult = false;
				var calculator = new ConditionCalculator(testFormula, testRateData);
				Assert("No Error Before Calculation", !calculator.HasParseError);
				AssertEquals("Retrun 0 as Default", expectedResult, calculator.Evaluate());
				Assert("Has Error After Calculation", calculator.HasParseError);
				AssertEquals("Has 1 Error", 1, calculator.Errors.Count());
				AssertEquals("Has Calculation Error", FormulaVisitErrorType.CalculationError, calculator.Errors.First().Type);
				AssertEquals("ErrorMessage", "Error when evaluating condition base on the input and the formula \rAttempted to divide by zero.", calculator.Errors.First().ErrorMessage.Trim());
			}

			);
			CombineAssertions("Test_4", () =>
			{
				var testFormula = "999999*999999*999999*999999*999999<1";
				var testRateData = new UniversalRateDataForTestWithData();
				var expectedResult = false;
				var calculator = new ConditionCalculator(testFormula, testRateData);
				Assert("No Error Before Calculation", !calculator.HasParseError);
				AssertEquals("Retrun 0 as Default", expectedResult, calculator.Evaluate());
				Assert("Has Error After Calculation", calculator.HasParseError);
				AssertEquals("Has 1 Error", 1, calculator.Errors.Count());
				AssertEquals("Has Calculation Error", FormulaVisitErrorType.CalculationError, calculator.Errors.First().Type);
				AssertEquals("ErrorMessage", "Error when evaluating condition base on the input and the formula \rValue was either too large or too small for a Decimal.", calculator.Errors.First().ErrorMessage);
			}

			);
			CombineAssertions("Test_5", () =>
			{
				var testFormula = "1/1000000/1000000/1000000/1000000/10000<1";
				var testRateData = new UniversalRateDataForTestWithData();
				var expectedResult = true;
				var calculator = new ConditionCalculator(testFormula, testRateData);
				Assert("No Error Before Calculation", !calculator.HasParseError);
				AssertEquals("Retrun 0 as Default", expectedResult, calculator.Evaluate());
				Assert("No Error After Calculation", !calculator.HasParseError);
				AssertEquals("Has 0 Error", 0, calculator.Errors.Count());
			}

			);
			CombineAssertions("Test_6", () =>
			{
				var testFormula = "1/1000000/1000000/1000000/1000000/100000<1";
				var testRateData = new UniversalRateDataForTestWithData();
				var expectedResult = true;
				var calculator = new ConditionCalculator(testFormula, testRateData);
				Assert("No Error Before Calculation", !calculator.HasParseError);
				AssertEquals("Retrun 0 as Default", expectedResult, calculator.Evaluate());
				Assert("No Error After Calculation", !calculator.HasParseError);
				AssertEquals("Has 0 Error", 0, calculator.Errors.Count());
			}

			);
		}

		void FillRealTestFormulas(Dictionary<string, bool> dict)
		{
			dict.Add("{\"Rebate Amount\"}<=50.5", true);
			dict.Add("0.00091 * [L]<1", true);
			dict.Add("0.00183 * [L]<2", true);
			dict.Add("0.0022 * [l]>3", false);
			dict.Add("0.0044 * [KG]<=4.4", true);
			dict.Add("0.0045 * [Kg]>=4.6", false);
			dict.Add("0.0065 * [KG]!=6", true);
			dict.Add("[PR] > 185 & [Pr] <= 2000", true);
			dict.Add("DOV > 20140810 & DOV <= 20150911", true);
			dict.Add("0.0085 * [KG]=8.5", true);
			dict.Add("(0.05 * ((VFD * 1.15) + 1P1)) < 70", true);
			dict.Add("0.07 * ((VFD * 1.15) + 1P1) > 50", true);
			dict.Add("125 * MAX(([GK] - 175), 0)> 10000", true);
			dict.Add("155.54 * [KG NET] = 155540", true);
			dict.Add("1P1+1P2A+1P2B+1P3A+1P3B+1P3C+1P3D+1P5A+1P5B<1900", true);
			dict.Add("1P1+1P2A+1P2B+1P3A+1P3B+1P3C+1P5A+1P5B>1700", false);
			dict.Add("(IF({DECIMAL(5,0):\"Days Owned where owned for less than 12 months\"} = 0, (1P1+1P2B+1P3A+1P3B+1P3C+1P3D+1P5A+1P5B), ROUND((1P1+1P2B+1P3A+1P3B+1P3C+1P3D+1P5A+1P5B) * MIN({DECIMAL(5,0):\"Days Owned where owned for less than 12 months\"}, 365) / 365, 3)))<300", true);
			dict.Add("Max((1P1 - 0.134 * VFD), 0)<70", true);
			dict.Add("MAX((1P1 - 0.146 * VFD), 0)>50", true);
			dict.Add("MIN(MaX((ROUND(0.00003 * ((VFD * 1.15) + 1P1), 3) - 0.75), 0) * ((VFD * 1.15) + 1P1), 0.25 * ((VFD * 1.15) + 1P1))=0", true);
		}
	}
}
