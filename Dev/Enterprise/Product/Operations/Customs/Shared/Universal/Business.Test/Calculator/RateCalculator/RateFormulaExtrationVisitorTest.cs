using System;
using System.Linq;
using Antlr4.Runtime;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	public class RateFormulaExtrationVisitorTest : TestCase
	{
		public void TestExtract()
		{
			CombineAssertions("TestCase: 0.7 * VFD", () =>
			{
				var testFormula = "0.7 * VFD";
				var testCalcData = new UniversalRateDataForTest();
				var errorListener = new FormulaErrorListener();
				var expectedUOMList = Array.Empty<string>();
				var expectedCountrySpecificList = Array.Empty<string>();
				var expectedFormulaSpecificList = Array.Empty<string>();
				var expectedMeursingExpressionList = Array.Empty<string>();
				BasicExtractionTest(testFormula, testCalcData, errorListener, expectedUOMList, expectedCountrySpecificList, expectedFormulaSpecificList, expectedMeursingExpressionList);
			}

			);
			CombineAssertions("TestCase: 0.7*[KG] with KG given", () =>
			{
				var testFormula = "0.7 *[KG]";
				var testCalcData = new UniversalRateDataForTest();
				var errorListener = new FormulaErrorListener();
				var expectedUOMList = new string[] { "KG" };
				var expectedCountrySpecificList = Array.Empty<string>();
				var expectedFormulaSpecificList = Array.Empty<string>();
				var expectedMeursingExpressionList = Array.Empty<string>();
				testCalcData.UnitOfMeasureValueList.Add("KG", 1);
				BasicExtractionTest(testFormula, testCalcData, errorListener, expectedUOMList, expectedCountrySpecificList, expectedFormulaSpecificList, expectedMeursingExpressionList);
			}

			);
			CombineAssertions("TestCase: 0.7*[KG] with no KG given", () =>
			{
				var testFormula = "0.7 *[KG]";
				var testCalcData = new UniversalRateDataForTest();
				var errorListener = new FormulaErrorListener();
				var expectedUOMList = Array.Empty<string>();
				var expectedCountrySpecificList = Array.Empty<string>();
				var expectedFormulaSpecificList = Array.Empty<string>();
				var expectedMeursingExpressionList = Array.Empty<string>();
				BasicExtractionTestExpectingError(testFormula, testCalcData, errorListener, expectedUOMList, expectedCountrySpecificList, expectedFormulaSpecificList, expectedMeursingExpressionList);
				AssertEquals(1, errorListener.Errors.Count());
				AssertEquals(FormulaVisitErrorType.UOMNotFound, errorListener.Errors.FirstOrDefault().Type);
				AssertEquals("Unit of Measure code: KG is not specified", errorListener.Errors.FirstOrDefault().ErrorMessage);
			}

			);
			CombineAssertions("TestCase: 0.7*[KG]+1P1 with 1P1 given", () =>
			{
				var testFormula = "0.7 *[KG] + 1P1";
				var testCalcData = new UniversalRateDataForTest();
				var errorListener = new FormulaErrorListener();
				var expectedUOMList = new string[] { "KG" };
				var expectedCountrySpecificList = new string[] { "1P1" };
				var expectedFormulaSpecificList = Array.Empty<string>();
				var expectedMeursingExpressionList = Array.Empty<string>();
				testCalcData.UnitOfMeasureValueList.Add("KG", 1);
				testCalcData.CountrySpecificValueList.Add("1P1", 1);
				BasicExtractionTest(testFormula, testCalcData, errorListener, expectedUOMList, expectedCountrySpecificList, expectedFormulaSpecificList, expectedMeursingExpressionList);
			}

			);
			CombineAssertions("TestCase: 0.7*[KG]+1P1 with no 1P1 given", () =>
			{
				var testFormula = "0.7 *[KG] + 1P1";
				var testCalcData = new UniversalRateDataForTest();
				var errorListener = new FormulaErrorListener();
				var expectedUOMList = new string[] { "KG" };
				var expectedCountrySpecificList = Array.Empty<string>();
				var expectedFormulaSpecificList = Array.Empty<string>();
				var expectedMeursingExpressionList = Array.Empty<string>();
				testCalcData.UnitOfMeasureValueList.Add("KG", 1);
				BasicExtractionTestExpectingError(testFormula, testCalcData, errorListener, expectedUOMList, expectedCountrySpecificList, expectedFormulaSpecificList, expectedMeursingExpressionList);
				AssertEquals(1, errorListener.Errors.Count());
				AssertEquals(FormulaVisitErrorType.CountrySpecificValueNotFound, errorListener.Errors.FirstOrDefault().Type);
				AssertEquals("Country Specific Value: 1P1 is not specified", errorListener.Errors.FirstOrDefault().ErrorMessage);
			}

			);
			CombineAssertions("TestCase: 0.7*[KG]+DTY with DTY given", () =>
			{
				var testFormula = "0.7 *[KG] + DTY";
				var testCalcData = new UniversalRateDataForTest();
				var errorListener = new FormulaErrorListener();
				var expectedUOMList = new string[] { "KG" };
				var expectedCountrySpecificList = new string[] { "DTY" };
				var expectedFormulaSpecificList = Array.Empty<string>();
				var expectedMeursingExpressionList = Array.Empty<string>();
				testCalcData.UnitOfMeasureValueList.Add("KG", 1);
				testCalcData.CountrySpecificValueList.Add("DTY", 1);
				BasicExtractionTest(testFormula, testCalcData, errorListener, expectedUOMList, expectedCountrySpecificList, expectedFormulaSpecificList, expectedMeursingExpressionList);
			}

			);
			CombineAssertions("TestCase: 0.7*[KG]+DTY with no DTY given", () =>
			{
				var testFormula = "0.7 *[KG] + DTY";
				var testCalcData = new UniversalRateDataForTest();
				var errorListener = new FormulaErrorListener();
				var expectedUOMList = new string[] { "KG" };
				var expectedCountrySpecificList = Array.Empty<string>();
				var expectedFormulaSpecificList = Array.Empty<string>();
				var expectedMeursingExpressionList = Array.Empty<string>();
				testCalcData.UnitOfMeasureValueList.Add("KG", 1);
				BasicExtractionTestExpectingError(testFormula, testCalcData, errorListener, expectedUOMList, expectedCountrySpecificList, expectedFormulaSpecificList, expectedMeursingExpressionList);
				AssertEquals(1, errorListener.Errors.Count());
				AssertEquals(FormulaVisitErrorType.CountrySpecificValueNotFound, errorListener.Errors.FirstOrDefault().Type);
				AssertEquals("Country Specific Value: DTY is not specified", errorListener.Errors.FirstOrDefault().ErrorMessage);
			}

			);
			CombineAssertions("TestCase: IF(CV < 10000, 0, VFD / (1 - 0.20) - VFD)", () =>
			{
				var testFormula = "IF(CV < 10000, 0, VFD / (1 - 0.20) - VFD)";
				var testCalcData = new UniversalRateDataForTest();
				var errorListener = new FormulaErrorListener();
				var expectedUOMList = Array.Empty<string>();
				var expectedCountrySpecificList = Array.Empty<string>();
				var expectedFormulaSpecificList = Array.Empty<string>();
				var expectedMeursingExpressionList = Array.Empty<string>();
				BasicExtractionTest(testFormula, testCalcData, errorListener, expectedUOMList, expectedCountrySpecificList, expectedFormulaSpecificList, expectedMeursingExpressionList);
			}

			);
			CombineAssertions("TestCase: VFD / (1 - IF(CV / MAX(1, ROUND([143] / 0.2, 0)) < 50, 0.30, 0.45)) - VFD + 150 * MAX(1,ROUND([143] / 50, 0)) with 143 given", () =>
			{
				var testFormula = "VFD / (1 - IF(CV / MAX(1, ROUND([143] / 0.2, 0)) < 50, 0.30, 0.45)) - VFD + 150 * MAX(1,ROUND([143] / 50, 0))";
				var testCalcData = new UniversalRateDataForTest();
				var errorListener = new FormulaErrorListener();
				var expectedUOMList = new string[] { "143" };
				var expectedCountrySpecificList = Array.Empty<string>();
				var expectedFormulaSpecificList = Array.Empty<string>();
				var expectedMeursingExpressionList = Array.Empty<string>();
				testCalcData.UnitOfMeasureValueList.Add("143", 1);
				BasicExtractionTest(testFormula, testCalcData, errorListener, expectedUOMList, expectedCountrySpecificList, expectedFormulaSpecificList, expectedMeursingExpressionList);
			}

			);
			CombineAssertions("TestCase: VFD / (1 - IF(CV / MAX(1, ROUND([143] / 0.2, 0)) < 50, 0.30, 0.45)) - VFD + 150 * MAX(1,ROUND([143] / 50, 0)) with no 143 given", () =>
			{
				var testFormula = "VFD / (1 - IF(CV / MAX(1, ROUND([143] / 0.2, 0)) < 50, 0.30, 0.45)) - VFD + 150 * MAX(1,ROUND([143] / 50, 0))";
				var testCalcData = new UniversalRateDataForTest();
				var errorListener = new FormulaErrorListener();
				var expectedUOMList = Array.Empty<string>();
				var expectedCountrySpecificList = Array.Empty<string>();
				var expectedFormulaSpecificList = Array.Empty<string>();
				var expectedMeursingExpressionList = Array.Empty<string>();
				BasicExtractionTestExpectingError(testFormula, testCalcData, errorListener, expectedUOMList, expectedCountrySpecificList, expectedFormulaSpecificList, expectedMeursingExpressionList);
				AssertEquals(2, errorListener.Errors.Count());
				AssertEquals(FormulaVisitErrorType.UOMNotFound, errorListener.Errors.FirstOrDefault().Type);
				AssertEquals("Unit of Measure code: 143 is not specified", errorListener.Errors.FirstOrDefault().ErrorMessage);
			});

			CombineAssertions("TestCase: 0.7* + #EA(1)# with EA given", () =>
			{
				var testFormula = "0.7 + #EA(1)#";
				var testCalcData = new UniversalRateDataForTest();
				var errorListener = new FormulaErrorListener();
				var expectedUOMList = Array.Empty<string>();
				var expectedCountrySpecificList = Array.Empty<string>();
				var expectedFormulaSpecificList = Array.Empty<string>();
				var expectedMeursingExpressionList = new string[] { "EA(1)" };
				testCalcData.MeursingExpressionList.Add("EA(1)", "10 * KG");
				BasicExtractionTest(testFormula, testCalcData, errorListener, expectedUOMList, expectedCountrySpecificList, expectedFormulaSpecificList, expectedMeursingExpressionList);
			});

			CombineAssertions("TestCase: 0.7 * #EA(1)# with no EA(1) given", () =>
			{
				var testFormula = "0.7 * #EA(1)#";
				var testCalcData = new UniversalRateDataForTest();
				var errorListener = new FormulaErrorListener();
				var expectedUOMList = Array.Empty<string>();
				var expectedCountrySpecificList = Array.Empty<string>();
				var expectedFormulaSpecificList = Array.Empty<string>();
				var expectedMeursingExpressionList = Array.Empty<string>();
				BasicExtractionTestExpectingError(testFormula, testCalcData, errorListener, expectedUOMList, expectedCountrySpecificList, expectedFormulaSpecificList, expectedMeursingExpressionList);
				AssertEquals(1, errorListener.Errors.Count());
				AssertEquals(FormulaVisitErrorType.MeursingExpressionNotFound, errorListener.Errors.FirstOrDefault().Type);
				AssertEquals("Meursing code: EA(1) is not specified", errorListener.Errors.FirstOrDefault().ErrorMessage);
			});

			CombineAssertions("User Question appear once", () =>
			{
				var testFormula = "0.7 *{\"UserTest\"}";
				var testCalcData = new UniversalRateDataForTest();
				var errorListener = new FormulaErrorListener();
				var expectedUOMList = Array.Empty<string>();
				var expectedCountrySpecificList = Array.Empty<string>();
				var expectedFormulaSpecificList = new string[] { "USERTEST" };
				var expectedMeursingExpressionList = Array.Empty<string>();
				BasicExtractionTest(testFormula, testCalcData, errorListener, expectedUOMList, expectedCountrySpecificList, expectedFormulaSpecificList, expectedMeursingExpressionList);
			}

			);
			CombineAssertions("User Question appear twice", () =>
			{
				var testFormula = "0.7 *{\"UserTest\"} + {\"UserTest\"}";
				var testCalcData = new UniversalRateDataForTest();
				var errorListener = new FormulaErrorListener();
				var expectedUOMList = Array.Empty<string>();
				var expectedCountrySpecificList = Array.Empty<string>();
				var expectedFormulaSpecificList = new string[] { "USERTEST" };
				var expectedMeursingExpressionList = Array.Empty<string>();
				BasicExtractionTest(testFormula, testCalcData, errorListener, expectedUOMList, expectedCountrySpecificList, expectedFormulaSpecificList, expectedMeursingExpressionList);
			}

			);
			CombineAssertions(() =>
			{
				var testFormula = "0.7 *[KG] + 1P1 - {\"UserTest\"} + MIN([M3], {DECIMAL(5,0): \"Days Owned where owned for less than 12 months\"})";
				var testCalcData = new UniversalRateDataForTest();
				var errorListener = new FormulaErrorListener();
				var expectedUOMList = new string[] { "KG", "M3" };
				var expectedCountrySpecificList = new string[] { "1P1" };
				var expectedFormulaSpecificList = new string[] { "USERTEST", "DAYS OWNED WHERE OWNED FOR LESS THAN 12 MONTHS" };
				var expectedMeursingExpressionList = Array.Empty<string>();
				testCalcData.UnitOfMeasureValueList.Add("KG", 1);
				testCalcData.UnitOfMeasureValueList.Add("M3", 1);
				testCalcData.CountrySpecificValueList.Add("1P1", 1);
				var wrapper = BasicExtractionTest(testFormula, testCalcData, errorListener, expectedUOMList, expectedCountrySpecificList, expectedFormulaSpecificList, expectedMeursingExpressionList);
				AssertEquals(0, wrapper.FormulaSpecificValueList["USERTEST"].Precision);
				AssertEquals(0, wrapper.FormulaSpecificValueList["USERTEST"].Scale);
				AssertEquals(true, wrapper.FormulaSpecificValueList["USERTEST"].IsDefaultPrecisionScale);
				AssertEquals(5, wrapper.FormulaSpecificValueList["DAYS OWNED WHERE OWNED FOR LESS THAN 12 MONTHS"].Precision);
				AssertEquals(0, wrapper.FormulaSpecificValueList["DAYS OWNED WHERE OWNED FOR LESS THAN 12 MONTHS"].Scale);
				AssertEquals(false, wrapper.FormulaSpecificValueList["DAYS OWNED WHERE OWNED FOR LESS THAN 12 MONTHS"].IsDefaultPrecisionScale);
			}

			);
			CombineAssertions(() =>
			{
				var testFormula = "0.7 *[KG] + 1P1 + DTY - {\"UserTest\"} + MIN([M3], {DECIMAL(5,0): \"Days Owned where owned for less than 12 months\"})";
				var testCalcData = new UniversalRateDataForTest();
				var errorListener = new FormulaErrorListener();
				var expectedUOMList = new string[] { "KG", "M3" };
				var expectedCountrySpecificList = new string[] { "1P1", "DTY" };
				var expectedFormulaSpecificList = new string[] { "USERTEST", "DAYS OWNED WHERE OWNED FOR LESS THAN 12 MONTHS" };
				var expectedMeursingExpressionList = Array.Empty<string>();
				testCalcData.UnitOfMeasureValueList.Add("KG", 1);
				testCalcData.UnitOfMeasureValueList.Add("M3", 1);
				testCalcData.CountrySpecificValueList.Add("1P1", 1);
				testCalcData.CountrySpecificValueList.Add("DTY", 1);
				var wrapper = BasicExtractionTest(testFormula, testCalcData, errorListener, expectedUOMList, expectedCountrySpecificList, expectedFormulaSpecificList, expectedMeursingExpressionList);
				AssertEquals(0, wrapper.FormulaSpecificValueList["USERTEST"].Precision);
				AssertEquals(0, wrapper.FormulaSpecificValueList["USERTEST"].Scale);
				AssertEquals(true, wrapper.FormulaSpecificValueList["USERTEST"].IsDefaultPrecisionScale);
				AssertEquals(5, wrapper.FormulaSpecificValueList["DAYS OWNED WHERE OWNED FOR LESS THAN 12 MONTHS"].Precision);
				AssertEquals(0, wrapper.FormulaSpecificValueList["DAYS OWNED WHERE OWNED FOR LESS THAN 12 MONTHS"].Scale);
				AssertEquals(false, wrapper.FormulaSpecificValueList["DAYS OWNED WHERE OWNED FOR LESS THAN 12 MONTHS"].IsDefaultPrecisionScale);
			}

			);
		}

		public void TestExtract_CaseInsensitive()
		{
			CombineAssertions("TestCase: 0.7 * VfD", () =>
			{
				var testFormula = "0.7 * VfD";
				var testCalcData = new UniversalRateDataForTest();
				var errorListener = new FormulaErrorListener();
				var expectedUOMList = Array.Empty<string>();
				var expectedCountrySpecificList = Array.Empty<string>();
				var expectedFormulaSpecificList = Array.Empty<string>();
				var expectedMeursingExpressionList = Array.Empty<string>();
				BasicExtractionTest(testFormula, testCalcData, errorListener, expectedUOMList, expectedCountrySpecificList, expectedFormulaSpecificList, expectedMeursingExpressionList);
			}

			);
			CombineAssertions("TestCase: 0.7*[KG] with KG given", () =>
			{
				var testFormula = "0.7 *[Kg]";
				var testCalcData = new UniversalRateDataForTest();
				var errorListener = new FormulaErrorListener();
				var expectedUOMList = new string[] { "KG" };
				var expectedCountrySpecificList = Array.Empty<string>();
				var expectedFormulaSpecificList = Array.Empty<string>();
				var expectedMeursingExpressionList = Array.Empty<string>();
				testCalcData.UnitOfMeasureValueList.Add("KG", 1);
				BasicExtractionTest(testFormula, testCalcData, errorListener, expectedUOMList, expectedCountrySpecificList, expectedFormulaSpecificList, expectedMeursingExpressionList);
			}

			);
			CombineAssertions("TestCase: 0.7*[KG] with no KG given", () =>
			{
				var testFormula = "0.7 *[kG]";
				var testCalcData = new UniversalRateDataForTest();
				var errorListener = new FormulaErrorListener();
				var expectedUOMList = Array.Empty<string>();
				var expectedCountrySpecificList = Array.Empty<string>();
				var expectedFormulaSpecificList = Array.Empty<string>();
				var expectedMeursingExpressionList = Array.Empty<string>();
				BasicExtractionTestExpectingError(testFormula, testCalcData, errorListener, expectedUOMList, expectedCountrySpecificList, expectedFormulaSpecificList, expectedMeursingExpressionList);
				AssertEquals(1, errorListener.Errors.Count());
				AssertEquals(FormulaVisitErrorType.UOMNotFound, errorListener.Errors.FirstOrDefault().Type);
				AssertEquals("Unit of Measure code: KG is not specified", errorListener.Errors.FirstOrDefault().ErrorMessage);
			}

			);

			CombineAssertions("TestCase: 0.7 * #eA(1)# with eA(1) given", () =>
			{
				var testFormula = "0.7 * #eA(1)#";
				var testCalcData = new UniversalRateDataForTest();
				var errorListener = new FormulaErrorListener();
				var expectedUOMList = Array.Empty<string>();
				var expectedCountrySpecificList = Array.Empty<string>();
				var expectedFormulaSpecificList = Array.Empty<string>();
				var expectedMeursingExpressionList = new string[] { "EA(1)" };
				testCalcData.MeursingExpressionList.Add("EA(1)", "0.5 * [kg]");
				BasicExtractionTest(testFormula, testCalcData, errorListener, expectedUOMList, expectedCountrySpecificList, expectedFormulaSpecificList, expectedMeursingExpressionList);
			});

			CombineAssertions("TestCase: 0.7* #eA(1)# with no eA(1) given", () =>
			{
				var testFormula = "0.7 * #eA(1)#";
				var testCalcData = new UniversalRateDataForTest();
				var errorListener = new FormulaErrorListener();
				var expectedUOMList = Array.Empty<string>();
				var expectedCountrySpecificList = Array.Empty<string>();
				var expectedFormulaSpecificList = Array.Empty<string>();
				var expectedMeursingExpressionList = Array.Empty<string>();
				BasicExtractionTestExpectingError(testFormula, testCalcData, errorListener, expectedUOMList, expectedCountrySpecificList, expectedFormulaSpecificList, expectedMeursingExpressionList);
				AssertEquals(1, errorListener.Errors.Count());
				AssertEquals(FormulaVisitErrorType.MeursingExpressionNotFound, errorListener.Errors.FirstOrDefault().Type);
				AssertEquals("Meursing code: EA(1) is not specified", errorListener.Errors.FirstOrDefault().ErrorMessage);
			});

			CombineAssertions("TestCase: 0.7*[KG]+1P1 with 1P1 given", () =>
			{
				var testFormula = "0.7 *[KG] + 1p1";
				var testCalcData = new UniversalRateDataForTest();
				var errorListener = new FormulaErrorListener();
				var expectedUOMList = new string[] { "KG" };
				var expectedCountrySpecificList = new string[] { "1P1" };
				var expectedFormulaSpecificList = Array.Empty<string>();
				var expectedMeursingExpressionList = Array.Empty<string>();
				testCalcData.UnitOfMeasureValueList.Add("KG", 1);
				testCalcData.CountrySpecificValueList.Add("1P1", 1);
				BasicExtractionTest(testFormula, testCalcData, errorListener, expectedUOMList, expectedCountrySpecificList, expectedFormulaSpecificList, expectedMeursingExpressionList);
			}

			);
			CombineAssertions("TestCase: 0.7*[KG]+1P1 with no 1P1 given", () =>
			{
				var testFormula = "0.7 *[KG] + 1p1";
				var testCalcData = new UniversalRateDataForTest();
				var errorListener = new FormulaErrorListener();
				var expectedUOMList = new string[] { "KG" };
				var expectedCountrySpecificList = Array.Empty<string>();
				var expectedFormulaSpecificList = Array.Empty<string>();
				var expectedMeursingExpressionList = Array.Empty<string>();
				testCalcData.UnitOfMeasureValueList.Add("KG", 1);
				BasicExtractionTestExpectingError(testFormula, testCalcData, errorListener, expectedUOMList, expectedCountrySpecificList, expectedFormulaSpecificList, expectedMeursingExpressionList);
				AssertEquals(1, errorListener.Errors.Count());
				AssertEquals(FormulaVisitErrorType.CountrySpecificValueNotFound, errorListener.Errors.FirstOrDefault().Type);
				AssertEquals("Country Specific Value: 1P1 is not specified", errorListener.Errors.FirstOrDefault().ErrorMessage);
			}

			);
			CombineAssertions("User Question appear once", () =>
			{
				var testFormula = "0.7 *{\"usertest\"}";
				var testCalcData = new UniversalRateDataForTest();
				var errorListener = new FormulaErrorListener();
				var expectedUOMList = Array.Empty<string>();
				var expectedCountrySpecificList = Array.Empty<string>();
				var expectedFormulaSpecificList = new string[] { "USERTEST" };
				var expectedMeursingExpressionList = Array.Empty<string>();
				BasicExtractionTest(testFormula, testCalcData, errorListener, expectedUOMList, expectedCountrySpecificList, expectedFormulaSpecificList, expectedMeursingExpressionList);
			}

			);
			CombineAssertions("User Question appear twice", () =>
			{
				var testFormula = "0.7 *{\"usertest\"} + {\"USERTest\"}";
				var testCalcData = new UniversalRateDataForTest();
				var errorListener = new FormulaErrorListener();
				var expectedUOMList = Array.Empty<string>();
				var expectedCountrySpecificList = Array.Empty<string>();
				var expectedFormulaSpecificList = new string[] { "USERTEST" };
				var expectedMeursingExpressionList = Array.Empty<string>();
				BasicExtractionTest(testFormula, testCalcData, errorListener, expectedUOMList, expectedCountrySpecificList, expectedFormulaSpecificList, expectedMeursingExpressionList);
			}

			);
		}

		#region Implementation

		static void BasicExtractionTestExpectingError(string testFormula
			, UniversalRateDataForTest testCalcData
			, FormulaErrorListener errorListener
			, string[] expectedUOMList
			, string[] expectedCountrySpecificList
			, string[] expectedFormulaSpecificList
			, string[] expectedMeursingExpressionList)
		{
			var input = new AntlrInputStream(testFormula);
			var lexer = new RateFormulaLexer(input);
			var tokens = new CommonTokenStream(lexer);
			var parser = new RateFormulaParser(tokens);
			parser.AddErrorListener(errorListener);
			var tree = parser.expression();
			AssertEquals("No Error in parsing", 0, errorListener.Errors.Count());
			AssertNotNull("Expression Tree Not null", tree);
			var wrapper = new UniversalRateCalcDataWrapper(testCalcData, errorListener);
			new RateFormulaExtractionVisitor(wrapper).VisitExpression(tree);
			AssertNotEquals(" Error in Extracting", 0, errorListener.Errors.Count());
			AssertArrayEqualsByElements("UOM List", expectedUOMList, testCalcData.UnitOfMeasureValueList.Keys.ToArray());
			AssertArrayEqualsByElements("CountrySpecificValue List", expectedCountrySpecificList, testCalcData.CountrySpecificValueList.Keys.ToArray());
			AssertArrayEqualsByElements("User Input Value", expectedFormulaSpecificList, wrapper.FormulaSpecificValueList.Keys.ToArray());
			AssertArrayEqualsByElements("Meursing Expression List", expectedMeursingExpressionList, wrapper.MeursingExpressionList.Keys.ToArray());
		}

		static UniversalRateCalcDataWrapper BasicExtractionTest(string testFormula
			, UniversalRateDataForTest testCalcData
			, FormulaErrorListener errorListener
			, string[] expectedUOMList
			, string[] expectedCountrySpecificList
			, string[] expectedFormulaSpecificList
			, string[] expectedMeursingExpressionList)
		{
			var input = new AntlrInputStream(testFormula);
			var lexer = new RateFormulaLexer(input);
			var tokens = new CommonTokenStream(lexer);
			var parser = new RateFormulaParser(tokens);
			parser.AddErrorListener(errorListener);
			var tree = parser.expression();
			AssertEquals("No Error in parsing", 0, errorListener.Errors.Count());
			AssertNotNull("Expression Tree Not null", tree);
			var wrapper = new UniversalRateCalcDataWrapper(testCalcData, errorListener);
			new RateFormulaExtractionVisitor(wrapper).VisitExpression(tree);
			AssertEquals("No Error in Extracting", 0, errorListener.Errors.Count());
			AssertArrayEqualsByElements("UOM List", expectedUOMList, wrapper.UnitOfMeasureValueList.Keys.ToArray());
			AssertArrayEqualsByElements("CountrySpecificValue List", expectedCountrySpecificList, wrapper.CountrySpecificValueList.Keys.ToArray());
			AssertArrayEqualsByElements("User Input Value", expectedFormulaSpecificList, wrapper.FormulaSpecificValueList.Keys.ToArray());
			AssertArrayEqualsByElements("Meursing Expression List", expectedMeursingExpressionList, wrapper.MeursingExpressionList.Keys.ToArray());
			return wrapper;
		}

		#endregion
	}
}
