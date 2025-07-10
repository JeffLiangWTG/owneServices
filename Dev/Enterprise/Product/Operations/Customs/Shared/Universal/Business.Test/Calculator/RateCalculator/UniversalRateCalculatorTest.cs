using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	public class UniversalRateCalculatorTest : TestCase
	{
		public void TestGetRateFormulaParseError()
		{
			AssertRateFormulaValidation("@30Z", "", "Lexical error at line 1 position 1.");
			AssertRateFormulaValidation("~$%~$^", "Syntax error at line 1 position 3.", "Lexical error at line 1 position 1.\r\nLexical error at line 1 position 2.\r\nLexical error at line 1 position 4.\r\nLexical error at line 1 position 5.\r\nLexical error at line 1 position 6.");
			AssertRateFormulaValidation("999/(1 && 2)", "Syntax error at line 1 position 8.", "");
			AssertRateFormulaValidation("999 * VFD + 0.23 * [KG", "Syntax error at line 1 position 20.", "");
			AssertRateFormulaValidation("999 * VFD + 0.23 * [KG]", "", "");
			AssertRateFormulaValidation("999 / 0", "", ""); // The error can't be detected by parser and will be catched by DivideByZeroException in Calculate()
			AssertRateFormulaValidation(@"{""Rebate Amount""}", "", "");
			AssertRateFormulaValidation(@"\{""Rebate Amount""\}", "", "Lexical error at line 1 position 1.\r\nLexical error at line 1 position 18."); // it has lexerErrorListener for "\{"
			AssertRateFormulaValidation(@"\{""Rebate Amount""\}", "", "", true); // it has lexerErrorListener for "\{" but it is ok to caculate rate. See TestAddAnswer(). So skip lexerError in GetRateFormulaParseError() as current to avoid make such existed formulas invalid to use
			AssertRateFormulaValidation("@30Z", "", "", true);
			AssertRateFormulaValidation("~$%~$^", "Syntax error at line 1 position 3.", "", true);
		}

		void AssertRateFormulaValidation(ZString fomula, string expectedParserError, string expectedLexerError, bool isSkipLexicalError = false)
		{
			RateFormulaParser.ExpressionContext tree = null;
			var lexerErrorListener = new LexicalErrorListener();
			var parserErrorListener = new FormulaErrorListener();
			var error = UniversalRateCalculator.GetRateFormulaParseError(fomula, out tree, parserErrorListener, lexerErrorListener, isSkipLexicalError);
			CombineAssertions("", () =>
			{
				if (string.IsNullOrEmpty(expectedParserError))
				{
					AssertEquals(fomula + ": parserErrorListener.Errors.Count", 0, parserErrorListener.Errors.Count());
				}
				else
				{
					AssertNull(fomula + ": tree", tree);
					AssertEquals(fomula + "parserErrorListener.Errors", expectedParserError, string.Join("\r\n", parserErrorListener.Errors.Select(x => x.ErrorMessage)));
				}

				var expectedError = expectedParserError;
				if (!isSkipLexicalError)
				{
					if (string.IsNullOrEmpty(expectedLexerError))
					{
						AssertEquals(fomula + ": lexerErrorListener.Errors.Count", 0, lexerErrorListener.Errors.Count());
					}
					else
					{
						AssertNull(fomula + ": tree", tree);
						expectedError = string.IsNullOrEmpty(expectedError) ? expectedLexerError : expectedError + "\r\n" + expectedLexerError;
						AssertEquals(fomula + ": lexerErrorListener.Errors", expectedLexerError, string.Join("\r\n", lexerErrorListener.Errors.Select(x => x.ErrorMessage)));
					}
				}

				if (string.IsNullOrEmpty(expectedError))
				{
					AssertNotNull(fomula + ": tree", tree);
				}

				AssertEquals(fomula + ": Errors", expectedError, error);
			}

			);
		}

		public void TestCaclculate()
		{
			var testFormulas = new Dictionary<string, decimal>();
			testFormulas.Add("0.8 * VFD", 800);
			testFormulas.Add("7% * VFD", 70);
			FillRealTestFormulas(testFormulas);
			CombineAssertions(() =>
			{
				foreach (var testKey in testFormulas.Keys)
				{
					try
					{
						var expectedResult = testFormulas[testKey];
						var testRateData = new UniversalRateDataForTestWithData();
						var calculator = new UniversalRateCalculator(testKey, testRateData);
						testRateData.AddDefaultFormulaSpecificAnswer(calculator.AddAnswer);
						Assert(testKey + " SyntaxError", !calculator.HasParseError);
						AssertEquals(string.Format("{0}=>{1}", testKey, expectedResult), expectedResult, calculator.Calculate());
					}
					catch (Exception e)
					{
						AssertEquals(testKey + " Exception", string.Empty, e.Message);
					}
				}
			}

			);
		}

		public void TestAddAnswer()
		{
			var testRateData = new UniversalRateDataForTest();
			var calculator = new UniversalRateCalculator(@"\{ ""Rebate Amount""\}", testRateData);
			AssertEquals(decimal.Zero, calculator.Calculate());
			calculator.AddAnswer("REBATE  AmounT   ", 29.83m);
			AssertEquals("question don't match", decimal.Zero, calculator.Calculate());
			calculator.AddAnswer("REBATE AmounT   ", 10.45m);
			AssertEquals(10.45m, calculator.Calculate());
			calculator.AddAnswer("   rebate amount   ", 302.43m);
			AssertEquals(302.43m, calculator.Calculate());
		}

		public void TestGetQuestionForFormulaSpecificValues()
		{
			var testRateData = new UniversalRateDataForTest();
			var calculator = new UniversalRateCalculator(@"\{""Rebate Amount""\}", testRateData);
			var list = calculator.GetQuestionForFormulaSpecificValues().ToList();
			AssertEquals(1, list.Count);
			Assert(list[0], "Rebate Amount", 0, 0);
			calculator = new UniversalRateCalculator(@"MIN((12A + 12B), \{ ""Duty payable per quarter for Excise duty purposes""\})", testRateData);
			list = calculator.GetQuestionForFormulaSpecificValues().ToList();
			AssertEquals(1, list.Count);
			Assert(list[0], "Duty payable per quarter for Excise duty purposes", 0, 0);
			calculator = new UniversalRateCalculator(@"IF(\{DECIMAL(5,3):""Days Owned where owned for less than 12 months""\} = 0, (1P1+12B+13A+13B+13C+13D+15A+15B), ROUND((1P1+12B+13A+13B+13C+13D+15A+15B) * MIN(\{DECIMAL(5,3):""Days Owned where owned for less than 12 months""\}, 365) / 365, 3))", testRateData);
			list = calculator.GetQuestionForFormulaSpecificValues().ToList();
			AssertEquals(1, list.Count);
			Assert(list[0], "Days Owned where owned for less than 12 months", 5, 3);
		}

		void Assert(QuestionForFormulaSpecificValue data, string question, int precision, int scale)
		{
			AssertEquals("Question", question, data.Question);
			AssertEquals("Precision", precision, data.Precision);
			AssertEquals("Scale", scale, data.Scale);
		}

		public void TestCaclculate_MixedCase()
		{
			var testFormulas = new Dictionary<string, decimal>();
			testFormulas.Add("0.8 * VFD", 800);
			testFormulas.Add("7% * VFD", 70);
			FillRealTestFormulas_MixedCase(testFormulas);
			CombineAssertions(() =>
			{
				foreach (var testKey in testFormulas.Keys)
				{
					try
					{
						var expectedResult = testFormulas[testKey];
						var testRateData = new UniversalRateDataForTestWithData();
						var calculator = new UniversalRateCalculator(testKey, testRateData);
						testRateData.AddDefaultFormulaSpecificAnswer(calculator.AddAnswer);
						Assert(testKey + " SyntaxError", !calculator.HasParseError);
						AssertEquals(string.Format("{0}=>{1}", testKey, expectedResult), expectedResult, calculator.Calculate());
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
			CombineAssertions(() =>
			{
				var testFormula = "99999999999999999999999999999999999999999999999";
				var testRateData = new UniversalRateDataForTestWithData();
				var expectedResult = 0M;
				var calculator = new UniversalRateCalculator(testFormula, testRateData);
				Assert("No Error Before Calculation", !calculator.HasParseError);
				AssertEquals("Retrun 0 as Default", expectedResult, calculator.Calculate());
				Assert("Has Error After Calculation", calculator.HasParseError);
				AssertEquals("Has 1 Error", 1, calculator.Errors.Count());
				AssertEquals("Has Calculation Error", FormulaVisitErrorType.SyntaxError, calculator.Errors.First().Type);
				AssertEquals("ErrorMessage", "Failed to convert \"99999999999999999999999999999999999999999999999\" to Decimal", calculator.Errors.First().ErrorMessage);
			}

			);
			CombineAssertions(() =>
			{
				var testFormula = "99999999999999999999999999999999999999999999999*999999";
				var testRateData = new UniversalRateDataForTestWithData();
				var expectedResult = 0M;
				var calculator = new UniversalRateCalculator(testFormula, testRateData);
				Assert("No Error Before Calculation", !calculator.HasParseError);
				AssertEquals("Retrun 0 as Default", expectedResult, calculator.Calculate());
				Assert("Has Error After Calculation", calculator.HasParseError);
				AssertEquals("Has 1 Error", 1, calculator.Errors.Count());
				AssertEquals("Has Calculation Error", FormulaVisitErrorType.SyntaxError, calculator.Errors.First().Type);
				AssertEquals("ErrorMessage", "Failed to convert \"99999999999999999999999999999999999999999999999\" to Decimal", calculator.Errors.First().ErrorMessage);
			}

			);
			CombineAssertions(() =>
			{
				var testFormula = "999999/0";
				var testRateData = new UniversalRateDataForTestWithData();
				var expectedResult = 0M;
				var calculator = new UniversalRateCalculator(testFormula, testRateData);
				Assert("No Error Before Calculation", !calculator.HasParseError);
				AssertEquals("Retrun 0 as Default", expectedResult, calculator.Calculate());
				Assert("Has Error After Calculation", calculator.HasParseError);
				AssertEquals("Has 1 Error", 1, calculator.Errors.Count());
				AssertEquals("Has Calculation Error", FormulaVisitErrorType.CalculationError, calculator.Errors.First().Type);
				AssertEquals("ErrorMessage", "Error when calculating rate base on the input and the formula \rAttempted to divide by zero.", calculator.Errors.First().ErrorMessage.Trim());
			}

			);
			CombineAssertions(() =>
			{
				var testFormula = "999999*999999*999999*999999*999999";
				var testRateData = new UniversalRateDataForTestWithData();
				var expectedResult = 0M;
				var calculator = new UniversalRateCalculator(testFormula, testRateData);
				Assert("No Error Before Calculation", !calculator.HasParseError);
				AssertEquals("Retrun 0 as Default", expectedResult, calculator.Calculate());
				Assert("Has Error After Calculation", calculator.HasParseError);
				AssertEquals("Has 1 Error", 1, calculator.Errors.Count());
				AssertEquals("Has Calculation Error", FormulaVisitErrorType.CalculationError, calculator.Errors.First().Type);
				AssertEquals("ErrorMessage", "Error when calculating rate base on the input and the formula \rValue was either too large or too small for a Decimal.", calculator.Errors.First().ErrorMessage);
			}

			);
			CombineAssertions(() =>
			{
				var testFormula = "1/1000000/1000000/1000000/1000000/10000";
				var testRateData = new UniversalRateDataForTestWithData();
				var expectedResult = 0.0000000000000000000000000001M;
				var calculator = new UniversalRateCalculator(testFormula, testRateData);
				Assert("No Error Before Calculation", !calculator.HasParseError);
				AssertEquals("Retrun 0 as Default", expectedResult, calculator.Calculate());
				Assert("No Error After Calculation", !calculator.HasParseError);
				AssertEquals("Has 0 Error", 0, calculator.Errors.Count());
			}

			);
			CombineAssertions(() =>
			{
				var testFormula = "1/1000000/1000000/1000000/1000000/100000";
				var testRateData = new UniversalRateDataForTestWithData();
				var expectedResult = 0M;
				var calculator = new UniversalRateCalculator(testFormula, testRateData);
				Assert("No Error Before Calculation", !calculator.HasParseError);
				AssertEquals("Retrun 0 as Default", expectedResult, calculator.Calculate());
				Assert("No Error After Calculation", !calculator.HasParseError);
				AssertEquals("Has 0 Error", 0, calculator.Errors.Count());
			}

			);
		}

		void FillRealTestFormulas(Dictionary<string, decimal> dict)
		{
			dict.Add("{\"Rebate Amount\"}", 50M);
			dict.Add("0", 0M);
			dict.Add("0.00091 * [L]", 0.91M);
			dict.Add("0.00183 * [L]", 1.83M);
			dict.Add("0.0022 * [L]", 2.2M);
			dict.Add("0.0044 * [KG]", 4.4M);
			dict.Add("0.0045 * [KG]", 4.5M);
			dict.Add("0.0065 * [KG]", 6.5M);
			dict.Add("0.0085 * [KG]", 8.5M);
			dict.Add("0.0099 * [KG]", 9.9M);
			dict.Add("0.01 * VFD", 10M);
			dict.Add("0.011 * [KG]", 11M);
			dict.Add("0.01209 * [L]", 12.09M);
			dict.Add("0.01409 * [L]", 14.09M);
			dict.Add("0.0225 * [KG]", 22.5M);
			dict.Add("0.024 * [KG NET]", 24M);
			dict.Add("0.0275 * [KG]", 27.5M);
			dict.Add("0.03 * [KG]", 30M);
			dict.Add("0.03 * VFD", 30M);
			dict.Add("0.033 * [L]", 33M);
			dict.Add("0.035 * [KWH]", 35M);
			dict.Add("0.03817 * [L]", 38.17M);
			dict.Add("0.0386 * VFD", 38.6M);
			dict.Add("0.03909 * [L]", 39.09M);
			dict.Add("0.04 * [KG]", 40M);
			dict.Add("0.04 * VFD", 40M);
			dict.Add("0.0415 * [KG]", 41.5M);
			dict.Add("0.0436 * [L]", 43.6M);
			dict.Add("0.05 * ((VFD * 1.15) + 1P1)", 67.5M);
			dict.Add("0.05 * [KG]", 50M);
			dict.Add("0.05 * [L]", 50M);
			dict.Add("0.05 * VFD", 50M);
			dict.Add("0.055 * [KG]", 55M);
			dict.Add("0.055 * VFD", 55M);
			dict.Add("0.06 * [BG]", 60M);
			dict.Add("0.06 * [KG]", 60M);
			dict.Add("0.06 * VFD", 60M);
			dict.Add("0.066 * VFD", 66M);
			dict.Add("0.0661 * VFD", 66.1M);
			dict.Add("0.067 * [L]", 67M);
			dict.Add("0.07 * ((VFD * 1.15) + 1P1)", 94.5M);
			dict.Add("0.074 * VFD", 74M);
			dict.Add("0.075 * VFD", 75M);
			dict.Add("0.0782 * [L]", 78.2M);
			dict.Add("0.08 * [KG]", 80M);
			dict.Add("0.08 * VFD", 80M);
			dict.Add("0.085 * VFD", 85M);
			dict.Add("0.089 * [L]", 89M);
			dict.Add("0.09 * VFD", 90M);
			dict.Add("0.092 * [KG]", 92M);
			dict.Add("0.094 * VFD", 94M);
			dict.Add("0.098 * VFD", 98M);
			dict.Add("0.1 * [KG]", 100M);
			dict.Add("0.1 * VFD", 100M);
			dict.Add("0.11 * [L]", 110M);
			dict.Add("0.11 * VFD", 110M);
			dict.Add("0.1109 * VFD", 110.9M);
			dict.Add("0.12 * VFD", 120M);
			dict.Add("0.1207 * VFD", 120.7M);
			dict.Add("0.125 * VFD", 125M);
			dict.Add("0.1251 * VFD", 125.1M);
			dict.Add("0.132 * VFD", 132M);
			dict.Add("0.14 * VFD", 140M);
			dict.Add("0.15 * VFD", 150M);
			dict.Add("0.16 * VFD", 160M);
			dict.Add("0.16966 * [L]", 169.66M);
			dict.Add("0.17 * VFD", 170M);
			dict.Add("0.17466 * [L]", 174.66M);
			dict.Add("0.175 * VFD", 175M);
			dict.Add("0.18 * VFD", 180M);
			dict.Add("0.19 * VFD", 190M);
			dict.Add("0.193 * VFD", 193M);
			dict.Add("0.197 * VFD", 197M);
			dict.Add("0.2 * VFD", 200M);
			dict.Add("0.2045 * VFD", 204.5M);
			dict.Add("0.21 * VFD", 210M);
			dict.Add("0.22 * VFD", 220M);
			dict.Add("0.226 * VFD", 226M);
			dict.Add("0.2281 * VFD", 228.1M);
			dict.Add("0.24 * VFD", 240M);
			dict.Add("0.2465 * VFD", 246.5M);
			dict.Add("0.25 * VFD", 250M);
			dict.Add("0.27 * VFD", 270M);
			dict.Add("0.2982 * VFD", 298.2M);
			dict.Add("0.3 * VFD", 300M);
			dict.Add("0.305 * VFD", 305M);
			dict.Add("0.3099 * VFD", 309.9M);
			dict.Add("0.31 * VFD", 310M);
			dict.Add("0.313 * VFD", 313M);
			dict.Add("0.3232 * VFD", 323.2M);
			dict.Add("0.327 * VFD", 327M);
			dict.Add("0.346 * VFD", 346M);
			dict.Add("0.347 * [KG]", 347M);
			dict.Add("0.35 * [NO]", 350M);
			dict.Add("0.35 * VFD", 350M);
			dict.Add("0.36 * VFD", 360M);
			dict.Add("0.37 * VFD", 370M);
			dict.Add("0.3992 * VFD", 399.2M);
			dict.Add("0.4 * VFD", 400M);
			dict.Add("0.4022 * VFD", 402.2M);
			dict.Add("0.43 * VFD", 430M);
			dict.Add("0.45 * VFD", 450M);
			dict.Add("0.5 * [NO]", 500M);
			dict.Add("0.5 * VFD", 500M);
			dict.Add("0.541 * VFD", 541M);
			dict.Add("0.55 * VFD", 550M);
			dict.Add("0.554 * VFD", 554M);
			dict.Add("0.621 * [NO]", 621M);
			dict.Add("0.6241 * VFD", 624.1M);
			dict.Add("0.6547 * VFD", 654.7M);
			dict.Add("0.6874 * VFD", 687.4M);
			dict.Add("0.7333 * VFD", 733.3M);
			dict.Add("0.7393 * VFD", 739.3M);
			dict.Add("0.75 * VFD", 750M);
			dict.Add("0.7617 * VFD", 761.7M);
			dict.Add("0.77 * [KG]", 770M);
			dict.Add("0.82 * VFD", 820M);
			dict.Add("0.9112 * [KG]", 911.2M);
			dict.Add("0.93 * VFD", 930M);
			dict.Add("0.9586 * VFD", 958.6M);
			dict.Add("1.1 * [KG NET]", 1100M);
			dict.Add("1.1325 * VFD", 1132.5M);
			dict.Add("1.2 * [L]", 1200M);
			dict.Add("1.227 * VFD", 1227M);
			dict.Add("1.36 * [L]", 1360M);
			dict.Add("1.3668 * [KG]", 1366.8M);
			dict.Add("1.54 * [L]", 1540M);
			dict.Add("1.581 * [KG]", 1581M);
			dict.Add("1.6 * [KG]", 1600M);
			dict.Add("10.37 * [KG]", 10370M);
			dict.Add("125 * MAX(([GK] - 175), 0)", 103125M);
			dict.Add("13.87 * [M2]", 13870M);
			dict.Add("149.23 * [LA]", 149230M);
			dict.Add("155.54 * [KG NET]", 155540M);
			dict.Add("174.66 * [L]", 174660M);
			dict.Add("1P1", 200M);
			dict.Add("1P1+1P2A+1P2B+1P3A+1P3B+1P3C+1P3D+1P5A+1P5B", 1800M);
			dict.Add("1P1+1P2A+1P2B+1P3A+1P3B+1P3C+1P5A+1P5B", 1600M);
			dict.Add("1P1+1P2A+1P3A+1P3B+1P3C+1P3D+1P5A+1P5B", 1600M);
			dict.Add("1P1+1P2B+1P3A+1P3B+1P3C+1P3D+1P5A+1P5B", 1600M);
			dict.Add("1P2A+1P2B", 400M);
			dict.Add("1P2B", 200M);
			dict.Add("1P5A", 200M);
			dict.Add("2.2 * [KG]", 2200M);
			dict.Add("2.4 * [L]", 2400M);
			dict.Add("2.426 * [KG]", 2426M);
			dict.Add("2.55 * [L]", 2550M);
			dict.Add("2.627 * [KG]", 2627M);
			dict.Add("278.82 * [KG]", 278820M);
			dict.Add("28.34 * [KG]", 28340M);
			dict.Add("2824.55 * [KG NET]", 2824550M);
			dict.Add("3.07 * [L]", 3070M);
			dict.Add("3.17 * [LA]", 3170M);
			dict.Add("3.65 * [L]", 3650M);
			dict.Add("3.692 * [KG]", 3692M);
			dict.Add("4 * [KG]", 4000M);
			dict.Add("4 * [NO]", 4000M);
			dict.Add("4.8 * [KG]", 4800M);
			dict.Add("5 * [KG]", 5000M);
			dict.Add("5.46 * [L]", 5460M);
			dict.Add("5.62 * [M2]", 5620M);
			dict.Add("5.87 * [M2]", 5870M);
			dict.Add("6.91 * [KG]", 6910M);
			dict.Add("60.97 * [LA]", 60970M);
			dict.Add("7.2 * [M2]", 7200M);
			dict.Add("73.05 * [LA]", 73050M);
			dict.Add("8.02 * [M2]", 8020M);
			dict.Add("8.86 * [M2]", 8860M);
			dict.Add("9.4 * [KG]", 9400M);
			dict.Add("9.75 * [L]", 9750M);
			dict.Add("90 * MAX(([GK] - 120), 0)", 79200M);
			dict.Add("IF({DECIMAL(5,0):\"Days Owned where owned for less than 12 months\"} = 0, (1P1+1P2B+1P3A+1P3B+1P3C+1P3D+1P5A+1P5B), ROUND((1P1+1P2B+1P3A+1P3B+1P3C+1P3D+1P5A+1P5B) * MIN({DECIMAL(5,0):\"Days Owned where owned for less than 12 months\"}, 365) / 365, 3))", 219.178M);
			dict.Add("MAX((1P1 - 0.134 * VFD), 0)", 66M);
			dict.Add("MAX((1P1 - 0.146 * VFD), 0)", 54M);
			dict.Add("MAX((1P1 - 0.196 * VFD), 0)", 4M);
			dict.Add("MAX((1P1 - 0.2 * VFD), 0)", 0M);
			dict.Add("MAX((1P1 - 0.242 * VFD), 0)", 0M);
			dict.Add("MAX((1P1 - 1.194 * VFD), 0)", 0M);
			dict.Add("MAX((1P1+1P2A+1P2B+1P3A+1P3B+1P3C+1P3D+1P5A+1P5B - 0.00091 * [L]), 0)", 1799.09M);
			dict.Add("MAX((1P1+1P2A+1P2B+1P3A+1P3B+1P3C+1P3D+1P5A+1P5B - 0.038 * VFD), 0)", 1762M);
			dict.Add("MAX((1P1+1P2A+1P2B+1P3A+1P3B+1P3C+1P3D+1P5A+1P5B - 0.046 * VFD), 0)", 1754M);
			dict.Add("MAX((1P1+1P2A+1P2B+1P3A+1P3B+1P3C+1P3D+1P5A+1P5B - 0.048 * VFD), 0)", 1752M);
			dict.Add("MAX((1P1+1P2A+1P2B+1P3A+1P3B+1P3C+1P3D+1P5A+1P5B - 0.05 * VFD), 0)", 1750M);
			dict.Add("MAX((1P1+1P2A+1P2B+1P3A+1P3B+1P3C+1P3D+1P5A+1P5B - 0.06 * VFD), 0)", 1740M);
			dict.Add("MAX((1P1+1P2A+1P2B+1P3A+1P3B+1P3C+1P3D+1P5A+1P5B - 0.066 * VFD), 0)", 1734M);
			dict.Add("MAX((1P1+1P2A+1P2B+1P3A+1P3B+1P3C+1P3D+1P5A+1P5B - 0.074 * VFD), 0)", 1726M);
			dict.Add("MAX((1P1+1P2A+1P2B+1P3A+1P3B+1P3C+1P3D+1P5A+1P5B - 0.075 * VFD), 0)", 1725M);
			dict.Add("MAX((1P1+1P2A+1P2B+1P3A+1P3B+1P3C+1P3D+1P5A+1P5B - 0.08 * VFD), 0)", 1720M);
			dict.Add("MAX((1P1+1P2A+1P2B+1P3A+1P3B+1P3C+1P3D+1P5A+1P5B - 0.086 * VFD), 0)", 1714M);
			dict.Add("MAX((1P1+1P2A+1P2B+1P3A+1P3B+1P3C+1P3D+1P5A+1P5B - 0.088 * VFD), 0)", 1712M);
			dict.Add("MAX((1P1+1P2A+1P2B+1P3A+1P3B+1P3C+1P3D+1P5A+1P5B - 0.098 * VFD), 0)", 1702M);
			dict.Add("MAX((1P1+1P2A+1P2B+1P3A+1P3B+1P3C+1P3D+1P5A+1P5B - 0.1 * VFD), 0)", 1700M);
			dict.Add("MAX((1P1+1P2A+1P2B+1P3A+1P3B+1P3C+1P3D+1P5A+1P5B - 0.108 * VFD), 0)", 1692M);
			dict.Add("MAX((1P1+1P2A+1P2B+1P3A+1P3B+1P3C+1P3D+1P5A+1P5B - 0.11 * [KG]), 0)", 1690M);
			dict.Add("MAX((1P1+1P2A+1P2B+1P3A+1P3B+1P3C+1P3D+1P5A+1P5B - 0.11 * VFD), 0)", 1690M);
			dict.Add("MAX((1P1+1P2A+1P2B+1P3A+1P3B+1P3C+1P3D+1P5A+1P5B - 0.12 * VFD), 0)", 1680M);
			dict.Add("MAX((1P1+1P2A+1P2B+1P3A+1P3B+1P3C+1P3D+1P5A+1P5B - 0.122 * VFD), 0)", 1678M);
			dict.Add("MAX((1P1+1P2A+1P2B+1P3A+1P3B+1P3C+1P3D+1P5A+1P5B - 0.132 * VFD), 0)", 1668M);
			dict.Add("MAX((1P1+1P2A+1P2B+1P3A+1P3B+1P3C+1P3D+1P5A+1P5B - 0.138 * VFD), 0)", 1662M);
			dict.Add("MAX((1P1+1P2A+1P2B+1P3A+1P3B+1P3C+1P3D+1P5A+1P5B - 0.144 * VFD), 0)", 1656M);
			dict.Add("MAX((1P1+1P2A+1P2B+1P3A+1P3B+1P3C+1P3D+1P5A+1P5B - 0.15 * VFD), 0)", 1650M);
			dict.Add("MAX((1P1+1P2A+1P2B+1P3A+1P3B+1P3C+1P3D+1P5A+1P5B - 0.158 * VFD), 0)", 1642M);
			dict.Add("MAX((1P1+1P2A+1P2B+1P3A+1P3B+1P3C+1P3D+1P5A+1P5B - 0.164 * VFD), 0)", 1636M);
			dict.Add("MAX((1P1+1P2A+1P2B+1P3A+1P3B+1P3C+1P3D+1P5A+1P5B - 0.165 * VFD), 0)", 1635M);
			dict.Add("MAX((1P1+1P2A+1P2B+1P3A+1P3B+1P3C+1P3D+1P5A+1P5B - 0.19 * VFD), 0)", 1610M);
			dict.Add("MAX((1P1+1P2A+1P2B+1P3A+1P3B+1P3C+1P3D+1P5A+1P5B - 0.192 * VFD), 0)", 1608M);
			dict.Add("MAX((1P1+1P2A+1P2B+1P3A+1P3B+1P3C+1P3D+1P5A+1P5B - 0.198 * VFD), 0)", 1602M);
			dict.Add("MAX((1P1+1P2A+1P2B+1P3A+1P3B+1P3C+1P3D+1P5A+1P5B - 0.2 * VFD), 0)", 1600M);
			dict.Add("MAX((1P1+1P2A+1P2B+1P3A+1P3B+1P3C+1P3D+1P5A+1P5B - 0.22 * VFD), 0)", 1580M);
			dict.Add("MAX((1P1+1P2A+1P2B+1P3A+1P3B+1P3C+1P3D+1P5A+1P5B - 0.3 * VFD), 0)", 1500M);
			dict.Add("MAX((1P1+1P2A+1P2B+1P3A+1P3B+1P3C+1P3D+1P5A+1P5B - 0.32 * VFD), 0)", 1480M);
			dict.Add("MAX((1P1+1P2A+1P2B+1P3A+1P3B+1P3C+1P3D+1P5A+1P5B - 0.56 * [KG]), 0)", 1240M);
			dict.Add("MAX((1P2A+1P2B - {\"Duty Paid On Entry\"}), 0)", 350M);
			dict.Add("MAX(1P1+1P2A+1P2B+1P3A+1P3B+1P3C+1P3D+1P5A+1P5B - {\"Duty Paid On Entry\"}, 0)", 1750M);
			dict.Add("MAX(1P1+1P2A+1P2B+1P3A+1P3B+1P3C+1P3D+1P5A+1P5B - {\"Duty Payable on Value Calculated in Terms of Note 29\"}, 0)", 1750M);
			dict.Add("MAX(1P1+1P2A+1P2B+1P3A+1P3B+1P3C+1P3D+1P5A+1P5B - {\"Duty Payable on Value Calculated in Terms of Note 8.1\"}, 0)", 1750M);
			dict.Add("MAX(1P1+1P2A+1P2B+1P3A+1P3B+1P3C+1P3D+1P5A+1P5B - MAX(0.25 * VFD, 0.23 * [M3]), 0)", 1550M);
			dict.Add("MAX(1P1+1P2A+1P2B+1P3A+1P3B+1P3C+1P3D+1P5A+1P5B - ({\"Rebate, Refund and Drawback previously granted\"} + {\"Duty on the Cost of Manufacture, Processing or Repair\"}), 0)", 1700M);
			dict.Add("MAX(1P1+1P2A+1P2B+1P3A+1P3B+1P3C+1P3D+1P5A+1P5B - {\"Rebate, Refund and Drawback previously granted\"}, 0)", 1750M);
			dict.Add("MIN((1P1+1P2A+1P2B+1P3A+1P3B+1P3C+1P3D+1P5A+1P5B), (0.15* VFD))", 150M);
			dict.Add("MIN((1P1+1P2A+1P2B+1P3A+1P3B+1P3C+1P3D+1P5A+1P5B), {\"Duty as calculated in terms of the IRCC\"})", 50M);
			dict.Add("MIN((1P1+1P2A+1P2B+1P3A+1P3B+1P3C+1P3D+1P5A+1P5B), {\"Duty as calculated in terms of the notes to this rebate item\"})", 50M);
			dict.Add("MIN((1P1+1P2A+1P2B+1P3A+1P3B+1P3C+1P3D+1P5A+1P5B), {\"Duty of Schedule 1 Part 1 as calculated in terms of the IRCC\"})", 50M);
			dict.Add("MIN((1P1+1P2A+1P2B+1P3A+1P3B+1P3C+1P3D+1P5A+1P5B), {\"Duty payable per quarter for Excise duty purposes\"})", 50M);
			dict.Add("MIN((1P1+1P2A+1P2B+1P3A+1P3B+1P3C+1P3D+1P5A+1P5B), {\"Maximum Rebate Amount\"})", 50M);
			dict.Add("MIN((1P2A+1P2B), {\"Duty payable per quarter for Excise duty purposes\"})", 50M);
			dict.Add("MIN(0.001 * [L], 0.08 * VFD)", 1M);
			dict.Add("MIN(0.0055 * [L], 0.08 * VFD)", 5.5M);
			dict.Add("MIN(0.018 * [KG], 0.15 * VFD)", 18M);
			dict.Add("MIN(0.036 * [KG], 0.25 * VFD)", 36M);
			dict.Add("MIN(0.066 * [KG], 0.25 * VFD)", 66M);
			dict.Add("MIN(0.0725 * [KG], 0.22 * VFD)", 72.5M);
			dict.Add("MIN(0.1 * VFD, MAX((0.55 * [KG] - 0.9 * VFD), 0))", 0M);
			dict.Add("MIN(0.15 * VFD, 1.3 * [KG])", 150M);
			dict.Add("MIN(0.15 * VFD, MAX((8.6 * [KG] - 0.85 * VFD), 0))", 150M);
			dict.Add("MIN(0.165 * [KG], 0.25 * VFD)", 165M);
			dict.Add("MIN(0.2 * VFD, MAX((2.15 * [KG] - 0.8 * VFD), 0))", 200M);
			dict.Add("MIN(0.22 * VFD, 2.4 * [KG])", 220M);
			dict.Add("MIN(0.25 * VFD, 2 * [KG])", 250M);
			dict.Add("MIN(0.3 * VFD, 0.045 * [KG])", 45M);
			dict.Add("MIN(0.3 * VFD, 0.0725 * [KG])", 72.5M);
			dict.Add("MIN(0.3 * VFD, 1.3 * [KG])", 300M);
			dict.Add("MIN(0.3 * VFD, 5 * [PR])", 300M);
			dict.Add("MIN(0.37 * VFD, 2.4 * [KG])", 370M);
			dict.Add("MIN(0.4 * VFD, 2 * [KG])", 400M);
			dict.Add("MIN(0.4 * VFD, 2.4 * [KG])", 400M);
			dict.Add("MIN(0.6 * VFD, 25 * [KG])", 600M);
			dict.Add("MIN(2.5 * [KG], 0.475 * VFD)", 475M);
			dict.Add("MIN(3.25 * [KG], 0.37 * VFD)", 370M);
			dict.Add("MIN(4.5 * [KG], 0.96 * VFD)", 960M);
			dict.Add("MIN(5 * [KG], 0.79 * VFD)", 790M);
			dict.Add("MIN(5 * [KG], 0.95 * VFD)", 950M);
			dict.Add("MIN(MAX((1.1 * [KG] - 0.8 * VFD), 0), 0.37 * VFD)", 300M);
			dict.Add("MIN(MAX((8.6 * [KG] - 0.85 * VFD), 0), 0.44 * VFD)", 440M);
			dict.Add("MIN(MAX((ROUND(0.00003 * ((VFD * 1.15) + 1P1), 3) - 0.75), 0) * ((VFD * 1.15) + 1P1), 0.25 * ((VFD * 1.15) + 1P1))", 0M);
			dict.Add("CV +VFD", 2200M);
		}

		void FillRealTestFormulas_MixedCase(Dictionary<string, decimal> dict)
		{
			dict.Add("{\"RebATE Amount\"}", 50M);
			dict.Add("0", 0M);
			dict.Add("0.00091 * [l]", 0.91M);
			dict.Add("0.00091 * [L]", 0.91M);
			dict.Add("0.0044 * [kG]", 4.4M);
			dict.Add("0.0045 * [Kg]", 4.5M);
			dict.Add("0.0065 * [Kg]", 6.5M);
			dict.Add("0.01 * vFD", 10M);
			dict.Add("0.04 * VfD", 40M);
			dict.Add("0.04 * VFd", 40M);
			dict.Add("0.04 * vFd", 40M);
			dict.Add("0.04 * vfd", 40M);
			dict.Add("0.024 * [kg NET]", 24M);
			dict.Add("0.024 * [KG net]", 24M);
			dict.Add("0.024 * [kg net]", 24M);
			dict.Add("0.024 * [KG NET]", 24M);
			dict.Add("0.035 * [KWH]", 35M);
			dict.Add("0.035 * [Kwh]", 35M);
			dict.Add("0.035 * [kwH]", 35M);
			dict.Add("0.035 * [kwh]", 35M);
			dict.Add("0.05 * ((VfD * 1.15) + 1p1)", 67.5M);
			dict.Add("0.621 * [No]", 621M);
			dict.Add("0.621 * [nO]", 621M);
			dict.Add("0.621 * [no]", 621M);
			dict.Add("1P1", 200M);
			dict.Add("1p1", 200M);
			dict.Add("1p1+1p2a+1p2b+1p3a+1p3b+1P3C+1P3D+1P5A+1P5B", 1800M);
			dict.Add("1P1+1P2A+1P2B+1P3A+1P3b+1p3c+1p5a+1p5B", 1600M);
			dict.Add("90 * MaX(([GK] - 120), 0)", 79200M);
			dict.Add("If({DeCiMaL(5,0):\"Days Owned where owned for less than 12 months\"} = 0, (1P1+1P2b+1p3a+1p3b+1p3c+1p3d+1p5a+1p5b), rounD((1P1+1P2B+1P3A+1p3b+1p3c+1p3d+1p5a+1p5b) * min({DEcimAL(5,0):\"Days OwNED WHERE OWNED FOR Less than 12 months\"}, 365) / 365, 3))", 219.178M);
			dict.Add("max((1p1 - 0.134 * vfd), 0)", 66M);
			dict.Add("Max((1P1+1P2A+1P2B+1p3a+1p3b+1p3c+1p3d+1P5A+1P5B - 0.2 * VfD), 0)", 1600M);
			dict.Add("mAX((1p1+1p2a+1P2B+1P3A+1P3B+1P3C+1P3D+1P5A+1P5B - 0.22 * vFD), 0)", 1580M);
			dict.Add("MaX((1P1+1P2A+1p2b+1P3A+1P3B+1P3C+1P3D+1P5A+1P5B - 0.3 * VFd), 0)", 1500M);
			dict.Add("MAx((1P1+1P2A+1P2B+1p3a+1p3b+1P3C+1P3D+1P5A+1P5B - 0.32 * VFD), 0)", 1480M);
			dict.Add("maX((1P1+1P2A+1P2B+1P3A+1P3B+1p3c+1p3D+1P5A+1P5B - 0.56 * [Kg]), 0)", 1240M);
			dict.Add("Max((1P2A+1P2B - {\"Duty Paid On Entry\"}), 0)", 350M);
			dict.Add("MaX(1P1+1P2A+1P2B+1P3A+1P3B+1P3C+1P3D+1P5A+1P5B - {\"Duty PAId On Entry\"}, 0)", 1750M);
			dict.Add("MAx(1P1+1P2A+1P2B+1P3A+1P3B+1P3C+1P3D+1P5A+1P5B - {\"Duty PayABLE ON vALue Calculated in Terms of Note 29\"}, 0)", 1750M);
			dict.Add("mAX(1P1+1P2A+1P2B+1P3A+1P3B+1P3C+1P3D+1P5A+1P5B - {\"Duty Payable on ValUE cALCULated in Terms of Note 8.1\"}, 0)", 1750M);
			dict.Add("maX(1P1+1P2A+1P2B+1P3A+1P3B+1P3C+1P3D+1P5A+1P5B - MAX(0.25 * VFD, 0.23 * [M3]), 0)", 1550M);
			dict.Add("Max(1P1+1P2A+1P2B+1P3A+1P3B+1P3C+1P3D+1P5A+1P5B - ({\"Rebate, Refund and Drawback PREVIOUSLY GRanted\"} + {\"Duty on the Cost of Manufacture, Processing or Repair\"}), 0)", 1700M);
			dict.Add("max(1P1+1P2A+1P2B+1P3A+1P3B+1P3C+1P3D+1P5A+1P5B - {\"Rebate, Refund and dRAWBACK previously granted\"}, 0)", 1750M);
			dict.Add("mIN((1P1+1P2A+1P2B+1P3A+1P3B+1P3C+1P3D+1P5A+1P5B), (0.15* VFD))", 150M);
			dict.Add("MiN((1P1+1P2A+1P2B+1P3A+1P3B+1P3C+1P3D+1P5A+1P5B), {\"Duty as calculated in terms of the ircC\"})", 50M);
			dict.Add("MIn((1P1+1P2A+1P2B+1P3A+1P3B+1P3C+1P3D+1P5A+1P5B), {\"Duty as calculated in termS OF THE notes to this rebate item\"})", 50M);
			dict.Add("miN((1P1+1P2A+1P2B+1P3A+1P3B+1P3C+1P3D+1P5A+1P5B), {\"Duty of Schedule 1 Part 1 as calculated in terms of the IRCC\"})", 50M);
			dict.Add("Min((1P1+1P2A+1P2B+1P3A+1P3B+1P3C+1P3D+1P5A+1P5B), {\"Duty pAYABLE per quarter for Excise duty purposes\"})", 50M);
			dict.Add("min((1P1+1P2A+1P2B+1P3A+1P3B+1P3C+1P3D+1P5A+1P5B), {\"Maximum RebaTE Amount\"})", 50M);
			dict.Add("MIN((1p2A+1P2b), {\"Duty payable per quarter for Excise duty purposes\"})", 50M);
			dict.Add("Min(maX((1.1 * [KG] - 0.8 * VFd), 0), 0.37 * vFD)", 300M);
			dict.Add("mIN(MAx((8.6 * [KG] - 0.85 * vFD), 0), 0.44 * VFd)", 440M);
			dict.Add("MiN(MaX((RoUnD(0.00003 * ((VfD * 1.15) + 1P1), 3) - 0.75), 0) * ((vfD * 1.15) + 1P1), 0.25 * ((Vfd * 1.15) + 1P1))", 0M);
			dict.Add("cV + Cv - vFd", 1400M);
		}
	}
}
