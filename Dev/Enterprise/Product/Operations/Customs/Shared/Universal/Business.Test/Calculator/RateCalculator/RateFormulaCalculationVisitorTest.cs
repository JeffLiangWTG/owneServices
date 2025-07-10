using System;
using System.Linq;
using Antlr4.Runtime;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	public class RateFormulaCalculationVisitorTest : TestCase
	{
		public void TestCalculate()
		{
			CombineAssertions("TestCase: 0.7 * VFD", () =>
			{
				AssertData("0.7 * VFD", new UniversalRateDataForTest(), 700M);
			}

			);
			CombineAssertions("TestCase: 0.7*[KG] with KG given", () =>
			{
				var testCalcData = new UniversalRateDataForTest();
				testCalcData.UnitOfMeasureValueList.Add("KG", 1);
				AssertData("0.7 *[KG]", testCalcData, 0.7M);
			}

			);
			CombineAssertions("TestCase: 0.7*[KG] with no KG given", () =>
			{
				AssertData("0.7 *[KG]", new UniversalRateDataForTest(), 0M, new[] { "UOMNotFound:Unit of Measure code: KG is not specified, using 0 for calculation" });
			}

			);
			CombineAssertions("TestCase: 0.7*[KG]+1P1 with 1P1 given", () =>
			{
				var testCalcData = new UniversalRateDataForTest();
				testCalcData.UnitOfMeasureValueList.Add("KG", 1);
				testCalcData.CountrySpecificValueList.Add("1P1", 1);
				AssertData("0.7 *[KG]+1P1", testCalcData, 1.7M);
			}

			);
			CombineAssertions("TestCase: 0.7*[KG]+1P1 with no 1P1 given", () =>
			{
				var testCalcData = new UniversalRateDataForTest();
				testCalcData.UnitOfMeasureValueList.Add("KG", 1);
				AssertData("0.7 *[KG]+1P1", testCalcData, 0.7M, new[] { "CountrySpecificValueNotFound:Country Specific Value: 1P1 is not specified, using 0 for calculation" });
			}

			);
			CombineAssertions("TestCase: 0.7*[KG]+DTY with DTY given", () =>
			{
				var testCalcData = new UniversalRateDataForTest();
				testCalcData.UnitOfMeasureValueList.Add("KG", 1);
				testCalcData.CountrySpecificValueList.Add("DTY", 1);
				AssertData("0.7 *[KG]+DTY", testCalcData, 1.7M);
			}

			);
			CombineAssertions("TestCase: 0.7*[KG]+DTY with no DTY given", () =>
			{
				var testCalcData = new UniversalRateDataForTest();
				testCalcData.UnitOfMeasureValueList.Add("KG", 1);
				AssertData("0.7 *[KG]+DTY", testCalcData, 0.7M, new[] { "CountrySpecificValueNotFound:Country Specific Value: DTY is not specified, using 0 for calculation" });
			}

			);
			CombineAssertions("TestCase: IF(CV < 10000, 0, VFD / (1 - 0.20) - VFD)", () =>
			{
				var testCalcData = new UniversalRateDataForTest();
				AssertData("IF(CV < 10000, 0, VFD / (1 - 0.20) - VFD)", testCalcData, 0M);
			}

			);
			CombineAssertions("TestCase: IF(CV < 1000, 0, VFD / (1 - 0.20) - VFD)", () =>
			{
				var testCalcData = new UniversalRateDataForTest();
				AssertData("IF(CV < 1000, 0, VFD / (1 - 0.20) - VFD)", testCalcData, 250M);
			}

			);
			CombineAssertions("TestCase: VFD / (1 - IF(CV / MAX(1, ROUND([143] / 0.2, 0)) < 50, 0.30, 0.45)) - VFD + 150 * MAX(1,ROUND([143] / 50, 0))", () =>
			{
				var testCalcData = new UniversalRateDataForTest();
				testCalcData.UnitOfMeasureValueList.Add("143", 1);
				AssertData("ROUND(VFD / (1 - IF(CV / MAX(1, ROUND([143] / 0.2, 0)) < 50, 0.30, 0.45)) - VFD + 150 * MAX(1,ROUND([143] / 50, 0)), 2)", testCalcData, 968.18M);
			}

			);
			CombineAssertions("User Question Answer", () =>
			{
				var testCalcData = new UniversalRateDataForTest();
				var answer = new QuestionForFormulaSpecificValue("USERTEST", 0, 0);
				answer.SetUserAnswer(55);
				AssertData("0.7 +{\"UserTest\"}", testCalcData, 55.7M, answer: answer);
			}

			);
			CombineAssertions("No User Question Answer", () =>
			{
				AssertData("0.7 +{\"UserTest\"}", new UniversalRateDataForTest(), 0.7M, new[] { "FormulaSpecificValueNotFound:Formula Specific Value for question: UserTest is not specified, using 0 for calculation" });
			}

			);
			CombineAssertions("User Question appear twice", () =>
			{
				var testCalcData = new UniversalRateDataForTest();
				var answer = new QuestionForFormulaSpecificValue("USERTEST", 0, 0);
				answer.SetUserAnswer(55);
				AssertData("0.7 *{\"UserTest\"} + {\"UserTest\"}", testCalcData, 93.5M, answer: answer);
			}

			);
			CombineAssertions("TestCase: IF(VFD >= 1000, 999, 888)", () =>
			{
				AssertData("IF(VFD >= 1000, 999, 888)", new UniversalRateDataForTest(), 999M);
			}

			);
			CombineAssertions("TestCase: IF(VFD >= 100, 999, 888)", () =>
			{
				AssertData("IF(VFD >= 100, 999, 888)", new UniversalRateDataForTest(), 999M);
			}

			);
			CombineAssertions("TestCase: IF(VFD <= 1000, 999, 888)", () =>
			{
				AssertData("IF(VFD <= 1000, 999, 888)", new UniversalRateDataForTest(), 999M);
			}

			);
			CombineAssertions("TestCase: IF(VFD <= 100, 999, 888)", () =>
			{
				AssertData("IF(VFD <= 100, 999, 888)", new UniversalRateDataForTest(), 888M);
			}

			);
			CombineAssertions("TestCase: IF(DOV > 20141201, 999, 888)", () =>
			{
				AssertData("IF(DOV > 20141201, 999, 888)", new UniversalRateDataForTest(), 999M);
			}

			);
			CombineAssertions("TestCase: ROUND()", () =>
			{
				AssertData("ROUND(28.01, 3)", new UniversalRateDataForTest(), 28.01M);
				AssertData("ROUND(28.0124, 3)", new UniversalRateDataForTest(), 28.012M);
				AssertData("ROUND(28.0125, 3)", new UniversalRateDataForTest(), 28.013M);
				AssertData("ROUND(28.0126, 3)", new UniversalRateDataForTest(), 28.013M);
			}

			);
			CombineAssertions("TestCase: HAS", () =>
			{
				var testCalcData = new UniversalRateDataForTest();
				testCalcData.AdditionalInformationList.Add(new Tuple<string, string>("VFD", "XX"));
				testCalcData.AdditionalInformationList.Add(new Tuple<string, string>("AAA", "YY"));
				AssertData("IF(HAS(\"VFD\",\"XX\"), 1, 0)", testCalcData, 1M);
				AssertData("IF(HAS(\"AAA\",\"XX\"), 1, 0)", testCalcData, 0M);
			}
			);
		}

		public void TestCalculate_CaseInsensitive()
		{
			CombineAssertions("TestCase: 0.7 * VfD", () =>
			{
				AssertData("0.7 * VfD", new UniversalRateDataForTest(), 700M);
			}

			);
			CombineAssertions("TestCase: 0.7*[KG] with KG given", () =>
			{
				var testCalcData = new UniversalRateDataForTest();
				testCalcData.UnitOfMeasureValueList.Add("KG", 1);
				AssertData("0.7 *[kG]", testCalcData, 0.7M);
			}

			);
			CombineAssertions("TestCase: 0.7*[KG] with no KG given", () =>
			{
				AssertData("0.7 *[Kg]", new UniversalRateDataForTest(), 0M, new[] { "UOMNotFound:Unit of Measure code: KG is not specified, using 0 for calculation" });
			}

			);
			CombineAssertions("TestCase: 0.7*[KG]+1P1 with 1P1 given", () =>
			{
				var testCalcData = new UniversalRateDataForTest();
				testCalcData.UnitOfMeasureValueList.Add("KG", 1);
				testCalcData.CountrySpecificValueList.Add("1P1", 1);
				AssertData("0.7 *[kg]+1p1", testCalcData, 1.7M);
			}

			);
			CombineAssertions("TestCase: 0.7*[KG]+1P1 with no 1P1 given", () =>
			{
				var testCalcData = new UniversalRateDataForTest();
				testCalcData.UnitOfMeasureValueList.Add("KG", 1);
				AssertData("0.7 *[Kg]+1p1", testCalcData, 0.7M, new[] { "CountrySpecificValueNotFound:Country Specific Value: 1P1 is not specified, using 0 for calculation" });
			}

			);
			CombineAssertions("TestCase: 0.7*[KG]+DTY with DTY given", () =>
			{
				var testCalcData = new UniversalRateDataForTest();
				testCalcData.UnitOfMeasureValueList.Add("KG", 1);
				testCalcData.CountrySpecificValueList.Add("DTY", 1);
				AssertData("0.7 *[kg]+dty", testCalcData, 1.7M);
			}

			);
			CombineAssertions("TestCase: 0.7*[KG]+DTY with no DTY given", () =>
			{
				var testCalcData = new UniversalRateDataForTest();
				testCalcData.UnitOfMeasureValueList.Add("KG", 1);
				AssertData("0.7 *[Kg]+dty", testCalcData, 0.7M, new[] { "CountrySpecificValueNotFound:Country Specific Value: DTY is not specified, using 0 for calculation" });
			}

			);
			CombineAssertions("TestCase: IF(CV < 10000, 0, VFD / (1 - 0.20) - VFD)", () =>
			{
				var testCalcData = new UniversalRateDataForTest();
				AssertData("if(cv < 10000, 0, vfd / (1 - 0.20) - vfd)", testCalcData, 0M);
			}

			);
			CombineAssertions("TestCase: IF(CV < 1000, 0, VFD / (1 - 0.20) - VFD)", () =>
			{
				var testCalcData = new UniversalRateDataForTest();
				AssertData("if(cv < 1000, 0, vfd / (1 - 0.20) - vfd)", testCalcData, 250M);
			}

			);
			CombineAssertions("TestCase: VFD / (1 - IF(CV / MAX(1, ROUND([143] / 0.2, 0)) < 50, 0.30, 0.45)) - VFD + 150 * MAX(1,ROUND([143] / 50, 0))", () =>
			{
				var testCalcData = new UniversalRateDataForTest();
				testCalcData.UnitOfMeasureValueList.Add("143", 1);
				AssertData("round(vfd / (1 - if(cv / max(1, round([143] / 0.2, 0)) < 50, 0.30, 0.45)) - vfd + 150 * max(1,round([143] / 50, 0)), 2)", testCalcData, 968.18M);
			}

			);
			CombineAssertions("User Question Answer", () =>
			{
				var testCalcData = new UniversalRateDataForTest();
				var answer = new QuestionForFormulaSpecificValue("USERTEST", 0, 0);
				answer.SetUserAnswer(55);
				AssertData("0.7 +{\"UserTest\"}", testCalcData, 55.7M, answer: answer);
			}

			);
			CombineAssertions("No User Question Answer", () =>
			{
				AssertData("0.7 +{\"UserTest\"}", new UniversalRateDataForTest(), 0.7M, new[] { "FormulaSpecificValueNotFound:Formula Specific Value for question: UserTest is not specified, using 0 for calculation" });
			}

			);
			CombineAssertions("User Question appear twice", () =>
			{
				var testCalcData = new UniversalRateDataForTest();
				var answer = new QuestionForFormulaSpecificValue("USERTEST", 0, 0);
				answer.SetUserAnswer(55);
				AssertData("0.7 *{\"UserTest\"} + {\"uSERtESt\"}", testCalcData, 93.5M, answer: answer);
			}

			);
		}

		public void TestCalculate_LogicalExpression()
		{
			CombineAssertions("Test Logical Expression 1", () =>
			{
				var testCalcData = new UniversalRateDataForTest();
				var answer = new QuestionForFormulaSpecificValue("UserTest", 0, 0);
				answer.SetUserAnswer(55);
				AssertData("IF({\"UserTest\"} > 10, 999, 888)", testCalcData, 999M, answer: answer);
				answer.SetUserAnswer(5);
				AssertData("IF({\"UserTest\"} > 10, 999, 888)", testCalcData, 888M, answer: answer);
			}

			);
			CombineAssertions("Test Logical Expression 2.1", () =>
			{
				var testCalcData = new UniversalRateDataForTest();
				var answer = new QuestionForFormulaSpecificValue("UserTest", 0, 0);
				answer.SetUserAnswer(55);
				AssertData("IF({\"UserTest\"} > 10 | 1=1, 999, 888)", testCalcData, 999M, answer: answer);
				answer.SetUserAnswer(5);
				AssertData("IF({\"UserTest\"} > 10 | 1=1, 999, 888)", testCalcData, 999M, answer: answer);
			}

			);
			CombineAssertions("Test Logical Expression 2.2", () =>
			{
				var testCalcData = new UniversalRateDataForTest();
				var answer = new QuestionForFormulaSpecificValue("UserTest", 0, 0);
				answer.SetUserAnswer(55);
				AssertData("IF(1=1 | {\"UserTest\"} > 10, 999, 888)", testCalcData, 999M, answer: answer);
				answer.SetUserAnswer(5);
				AssertData("IF(1=1 | {\"UserTest\"} > 10, 999, 888)", testCalcData, 999M, answer: answer);
			}

			);
			CombineAssertions("Test Logical Expression 3.1", () =>
			{
				var testCalcData = new UniversalRateDataForTest();
				var answer = new QuestionForFormulaSpecificValue("UserTest", 0, 0);
				answer.SetUserAnswer(55);
				AssertData("IF({\"UserTest\"} > 10 & 1=1, 999, 888)", testCalcData, 999M, answer: answer);
				answer.SetUserAnswer(5);
				AssertData("IF({\"UserTest\"} > 10 & 1=1, 999, 888)", testCalcData, 888M, answer: answer);
			}

			);
			CombineAssertions("Test Logical Expression 3.2", () =>
			{
				var testCalcData = new UniversalRateDataForTest();
				var answer = new QuestionForFormulaSpecificValue("UserTest", 0, 0);
				answer.SetUserAnswer(55);
				AssertData("IF(1=1 & {\"UserTest\"} > 10, 999, 888)", testCalcData, 999M, answer: answer);
				answer.SetUserAnswer(5);
				AssertData("IF(1=1 & {\"UserTest\"} > 10, 999, 888)", testCalcData, 888M, answer: answer);
			}

			);
			CombineAssertions("Test Logical Expression 4.1", () =>
			{
				var testCalcData = new UniversalRateDataForTest();
				testCalcData.AdditionalInformationList.Add(new Tuple<string, string>("TST", "RightValue"));
				AssertData("IF(HAS(\"TST\",\"RightValue\") | 1=1, 999, 888)", testCalcData, 999M);
				AssertData("IF(HAS(\"TST\",\"WrongValue\") | 1=1, 999, 888)", testCalcData, 999M);
			}

			);
			CombineAssertions("Test Logical Expression 4.2", () =>
			{
				var testCalcData = new UniversalRateDataForTest();
				testCalcData.AdditionalInformationList.Add(new Tuple<string, string>("TST", "RightValue"));
				AssertData("IF(1=1 | HAS(\"TST\",\"RightValue\"), 999, 888)", testCalcData, 999M);
				AssertData("IF(1=1 | HAS(\"TST\",\"WrongValue\"), 999, 888)", testCalcData, 999M);
			}

			);
			CombineAssertions("Test Logical Expression 4.3", () =>
			{
				var testCalcData = new UniversalRateDataForTest();
				testCalcData.AdditionalInformationList.Add(new Tuple<string, string>("TST", "RightValue"));
				AssertData("IF(HAS(\"TST\",\"RightValue\") & 1=1, 999, 888)", testCalcData, 999M);
				AssertData("IF(HAS(\"TST\",\"WrongValue\") & 1=1, 999, 888)", testCalcData, 888M);
			}

			);
			CombineAssertions("Test Logical Expression 4.4", () =>
			{
				var testCalcData = new UniversalRateDataForTest();
				testCalcData.AdditionalInformationList.Add(new Tuple<string, string>("TST", "RightValue"));
				AssertData("IF(1=1 & HAS(\"TST\",\"RightValue\"), 999, 888)", testCalcData, 999M);
				AssertData("IF(1=1 & HAS(\"TST\",\"WrongValue\"), 999, 888)", testCalcData, 888M);
			}

			);
		}

		public void TestCalculate_WhenFormulaHasMeursingData()
		{
			CombineAssertions("TestCase: VFD * 0.090 + #EA(1)# with EA given", () =>
			{
				var testCalcData = new UniversalRateDataForTest();
				testCalcData.MeursingExpressionList.Add("EA(1)", "2 * [DTN]");
				testCalcData.UnitOfMeasureValueList.Add("DTN", 3);
				AssertData("VFD * 0.090 + #EA(1)#", testCalcData, 96M);
			});

			CombineAssertions("TestCase: VFD * 0.207 +#ADFM(1)# with ADFM(1) not given", () =>
			{
				var testCalcData = new UniversalRateDataForTest();
				AssertData("VFD * 0.207 +#ADFM(1)#", testCalcData, 207M, new[] { "MeursingExpressionNotFound:Meursing code: ADFM(1) is not specified, using 0 for calculation" });
			});
		}

		#region Implementation
		static void AssertData(string testFormula, UniversalRateDataForTest testCalcData, decimal expectResult, string[] expectedErrors = null, QuestionForFormulaSpecificValue answer = null)
		{
			var errorListener = new FormulaErrorListener();
			var wrapper = new UniversalRateCalcDataWrapper(testCalcData, errorListener);
			if (answer != null)
			{
				wrapper.FormulaSpecificValueList.Add(answer.Question.ToUpperInvariant().Trim(), answer);
			}

			var input = new AntlrInputStream(testFormula);
			var lexer = new RateFormulaLexer(input);
			var tokens = new CommonTokenStream(lexer);
			var parser = new RateFormulaParser(tokens);
			parser.AddErrorListener(errorListener);
			var tree = parser.expression();
			AssertEquals("No Error in parsing", 0, errorListener.Errors.Count());
			AssertNotNull("Expression Tree Not null", tree);
			var result = new RateFormulaCalculativeVisitor(wrapper).VisitExpression(tree);
			AssertEquals(testFormula, expectResult, result);
			if (expectedErrors == null)
			{
				AssertEquals("No Error in Extracting", 0, errorListener.Errors.Count());
			}
			else
			{
				var expectedErrorCount = expectedErrors.Length;
				var actualErrors = errorListener.Errors.ToArray();
				AssertEquals("Error Counts", expectedErrorCount, actualErrors.Length);
				for (int i = 0; i < Math.Min(expectedErrorCount, actualErrors.Length); i++)
				{
					AssertEquals("Error #" + i, expectedErrors[i], actualErrors[i].ToString());
				}
			}
		}
		#endregion
	}
}
