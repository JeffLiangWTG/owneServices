using System;
using System.Linq;
using Antlr4.Runtime;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	class ConditionFormulaCalculatorVisitorTestclass : TestCase
	{
		public void TestCalculate()
		{
			CombineAssertions("TestCase: 0.7 * VFD < 1000", () =>
			{
				AssertData("0.7 * VFD < 1000", new UniversalRateDataForTest(), true);
			});

			CombineAssertions("TestCase: 0.7*[KG]<1000 with KG given", () =>
			{
				var testCalcData = new UniversalRateDataForTest();
				testCalcData.UnitOfMeasureValueList.Add("KG", 1);
				AssertData("0.7 *[KG]<1000", testCalcData, true);
			});

			CombineAssertions("TestCase: 0.7*[KG]>100 with no KG given", () =>
			{
				AssertData("0.7 *[KG]>100", new UniversalRateDataForTest(), false, new[] { "UOMNotFound:Unit of Measure code: KG is not specified, using 0 for calculation" });
			});

			CombineAssertions("TestCase: 0.7*[KG]+1P1<1 with 1P1 given", () =>
			{
				var testCalcData = new UniversalRateDataForTest();
				testCalcData.UnitOfMeasureValueList.Add("KG", 1);
				testCalcData.CountrySpecificValueList.Add("1P1", 1);
				AssertData("0.7 *[KG]+1P1<1", testCalcData, false);
			});

			CombineAssertions("TestCase: 0.7*[KG]+1P1<1 with no 1P1 given", () =>
			{
				var testCalcData = new UniversalRateDataForTest();
				testCalcData.UnitOfMeasureValueList.Add("KG", 1);
				AssertData("0.7 *[KG]+1P1<1", testCalcData, true, new[] { "CountrySpecificValueNotFound:Country Specific Value: 1P1 is not specified, using 0 for calculation" });
			});

			CombineAssertions("User Question Answer", () =>
			{
				var testCalcData = new UniversalRateDataForTest();
				var answer = new QuestionForFormulaSpecificValue("USERTEST", 0, 0);
				answer.SetUserAnswer(55);
				AssertData("0.7 +{\"UserTest\"}>=50", testCalcData, true, answer: answer);
			});

			CombineAssertions("No User Question Answer", () =>
			{
				var testCalcData = new UniversalRateDataForTest();
				AssertData("0.7 +{\"UserTest\"}<1", testCalcData, true, new[] { "FormulaSpecificValueNotFound:Formula Specific Value for question: UserTest is not specified, using 0 for calculation" });
			});

			CombineAssertions("User Question appear twice", () =>
			{
				var testCalcData = new UniversalRateDataForTest();
				var answer = new QuestionForFormulaSpecificValue("USERTEST", 0, 0);
				answer.SetUserAnswer(55);
				AssertData("0.7 *{\"UserTest\"} + {\"UserTest\"}<=100", testCalcData, true, answer: answer);
			});

			CombineAssertions("TestCase: DOV > 20141201", () =>
			{
				AssertData("DOV > 20141201", new UniversalRateDataForTest(), true);
			});
		}

		public void TestCalculate_CaseInSensitive()
		{
			CombineAssertions("TestCase: 0.7 * VfD<1000", () =>
			{
				AssertData("0.7 * VfD<1000", new UniversalRateDataForTest(), true);
			});

			CombineAssertions("TestCase: 0.7*[kG]=7 with KG given", () =>
			{
				var testCalcData = new UniversalRateDataForTest();
				testCalcData.UnitOfMeasureValueList.Add("Kg", 10m);
				AssertData("0.7 *[kG]=7", testCalcData, true);
			});

			CombineAssertions("TestCase: 0.7*[Kg]>1 with no KG given", () =>
			{
				AssertData("0.7 *[Kg]>1", new UniversalRateDataForTest(), false, new[] { "UOMNotFound:Unit of Measure code: KG is not specified, using 0 for calculation" });
			});

			CombineAssertions("TestCase: 0.7*[KG]+1pA>10 with 1P1 given", () =>
			{
				var testCalcData = new UniversalRateDataForTest();
				testCalcData.UnitOfMeasureValueList.Add("KG", 1);
				testCalcData.CountrySpecificValueList.Add("1PA", 10);
				AssertData("0.7 *[kg]+1pA>10", testCalcData, true);
			});

			CombineAssertions("TestCase: 0.7*[KG]+1p1<1 with no 1P1 given", () =>
			{
				var testCalcData = new UniversalRateDataForTest();
				testCalcData.UnitOfMeasureValueList.Add("KG", 1);
				AssertData("0.7 *[Kg]+1p1<1", testCalcData, true, new[] { "CountrySpecificValueNotFound:Country Specific Value: 1P1 is not specified, using 0 for calculation" });
			});

			CombineAssertions("User Question appear twice", () =>
			{
				var testCalcData = new UniversalRateDataForTest();
				var answer = new QuestionForFormulaSpecificValue("USERTEST", 0, 0);
				answer.SetUserAnswer(55);
				AssertData("0.7 *{\"UserTest\"} + {\"uSERtESt\"}<100", testCalcData, true, answer: answer);
			});
		}

		public void TestCalculate_LogicalExpression()
		{
			CombineAssertions("Test Logical Expression 1", () =>
			{
				var testCalcData = new UniversalRateDataForTest();
				var answer = new QuestionForFormulaSpecificValue("UserTest", 0, 0);
				answer.SetUserAnswer(55);
				AssertData("IF({\"UserTest\"} > 10, 999, 888)=999", testCalcData, true, answer: answer);

				answer.SetUserAnswer(5);
				AssertData("IF({\"UserTest\"} > 10, 999, 888)=999", testCalcData, false, answer: answer);
			});

			CombineAssertions("Test Logical Expression 2.1", () =>
			{
				var testCalcData = new UniversalRateDataForTest();
				var answer = new QuestionForFormulaSpecificValue("UserTest", 0, 0);
				answer.SetUserAnswer(55);
				AssertData("IF({\"UserTest\"} > 10 | 1=1, 999, 888)=999", testCalcData, true, answer: answer);

				answer.SetUserAnswer(5);
				AssertData("IF({\"UserTest\"} > 10 | 1=1, 999, 888)=999", testCalcData, true, answer: answer);
			});

			CombineAssertions("Test Logical Expression 2.2", () =>
			{
				var testCalcData = new UniversalRateDataForTest();
				var answer = new QuestionForFormulaSpecificValue("UserTest", 0, 0);
				answer.SetUserAnswer(55);
				AssertData("IF(1=1 | {\"UserTest\"} > 10, 999, 888)=999", testCalcData, true, answer: answer);

				answer.SetUserAnswer(5);
				AssertData("IF(1=1 | {\"UserTest\"} > 10, 999, 888)=999", testCalcData, true, answer: answer);
			});

			CombineAssertions("Test Logical Expression 3.1", () =>
			{
				var testCalcData = new UniversalRateDataForTest();
				var answer = new QuestionForFormulaSpecificValue("UserTest", 0, 0);
				answer.SetUserAnswer(55);
				AssertData("IF({\"UserTest\"} > 10 & 1=1, 999, 888)=999", testCalcData, true, answer: answer);

				answer.SetUserAnswer(5);
				AssertData("IF({\"UserTest\"} > 10 & 1=1, 999, 888)=999", testCalcData, false, answer: answer);
			});

			CombineAssertions("Test Logical Expression 3.2", () =>
			{
				var testCalcData = new UniversalRateDataForTest();
				var answer = new QuestionForFormulaSpecificValue("UserTest", 0, 0);
				answer.SetUserAnswer(55);
				AssertData("IF(1=1 & {\"UserTest\"} > 10, 999, 888)=999", testCalcData, true, answer: answer);

				answer.SetUserAnswer(5);
				AssertData("IF(1=1 & {\"UserTest\"} > 10, 999, 888)=999", testCalcData, false, answer: answer);
			});
		}

		#region Implementation

		static void AssertData(string testFormula, UniversalRateDataForTest testCalcData, bool expectResult, string[] expectedErrors = null, QuestionForFormulaSpecificValue answer = null)
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

			var tree = parser.boolExpression();
			AssertEquals("No Error in parsing", 0, errorListener.Errors.Count());
			AssertNotNull("Expression Tree Not null", tree);
			var result = new ConditionFormulaCalculatorVisitor(wrapper).VisitBoolExpression(tree) > decimal.Zero;
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
