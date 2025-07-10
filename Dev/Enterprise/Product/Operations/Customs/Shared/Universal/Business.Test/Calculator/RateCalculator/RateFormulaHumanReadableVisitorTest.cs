using System;
using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Calculator.Testing
{
	class RateFormulaHumanReadableVisitorDataForTesting : RateFormulaHumanReadableVisitorData
	{
		public RateFormulaHumanReadableVisitorDataForTesting(
			string currencyCode,
			IDictionary<string, string> unitOfMeasureList,
			IDictionary<string, string> countrySpecificList,
			IDictionary<string, string> additionalInformationList,
			IDictionary<string, string> meursingExpressionList
		) : base(currencyCode, unitOfMeasureList, countrySpecificList, additionalInformationList, meursingExpressionList)
		{
		}

		public static RateFormulaHumanReadableVisitorDataForTesting New()
		{
			return new RateFormulaHumanReadableVisitorDataForTesting(
				"AUD",
				new Dictionary<string, string>(),
				new Dictionary<string, string>(),
				new Dictionary<string, string>(),
				new Dictionary<string, string>()
			);
		}

		public static RateFormulaHumanReadableVisitorDataForTesting NewWithUnitOfMeasureList(string unit, string description)
		{
			return new RateFormulaHumanReadableVisitorDataForTesting(
				"AUD",
				new Dictionary<string, string> { { unit, description } },
				new Dictionary<string, string>(),
				new Dictionary<string, string>(),
				new Dictionary<string, string>()
			);
		}
		public static RateFormulaHumanReadableVisitorDataForTesting NewWithCountrySpecificList(string countryVar, string description)
		{
			return new RateFormulaHumanReadableVisitorDataForTesting(
				"AUD",
				new Dictionary<string, string>(),
				new Dictionary<string, string> { { countryVar, description } },
				new Dictionary<string, string>(),
				new Dictionary<string, string>()
			);
		}
		public static RateFormulaHumanReadableVisitorDataForTesting NewWithAdditionalInformationList(string addInfo, string description)
		{
			return new RateFormulaHumanReadableVisitorDataForTesting(
				"AUD",
				new Dictionary<string, string>(),
				new Dictionary<string, string>(),
				new Dictionary<string, string> { { addInfo, description } },
				new Dictionary<string, string>()
			);
		}

		public static RateFormulaHumanReadableVisitorDataForTesting NewWithMeursingExpressionList(string meursingCode, string description)
		{
			return new RateFormulaHumanReadableVisitorDataForTesting(
				"AUD",
				new Dictionary<string, string>(),
				new Dictionary<string, string>(),
				new Dictionary<string, string>(),
				new Dictionary<string, string> { { meursingCode, description } }
			);
		}
	}

	public class RateFormulaHumanReadableVisitorTest : TestCase
	{
		public void TestReservedVFDContext_ValueForDuty()
		{
			AssertData("VFD", "value for duty");
		}

		public void TestReservedCVContext_CustomsValue()
		{
			AssertData("CV", "customs value");
		}

		public void TestUomPlaceHolderContext()
		{
			AssertData("[DTN]", "DTN", null);
			AssertData("[DTN]", RateFormulaHumanReadableVisitorDataForTesting.NewWithUnitOfMeasureList("DTN", "100 kg"), "100 kg");
		}

		public void TestMeursingPlaceHolderContext()
		{
			AssertData("#EA(1)#", "EA(1)");
			var rateFormulaHumanReadableVisitor = RateFormulaHumanReadableVisitorDataForTesting.NewWithMeursingExpressionList("EA(1)", "Agricultural Element");
			AssertData("#EA(1)#", rateFormulaHumanReadableVisitor, "Agricultural Element");
		}

		public void TestVarCountrySpecificKeywordContext()
		{
			AssertData("1P1", "1P1");
			AssertData("1P1", RateFormulaHumanReadableVisitorDataForTesting.NewWithCountrySpecificList("1P1", "Schedule 1 Part 1"), "Schedule 1 Part 1");
		}

		public void TestHasExpressionContext()
		{
			AssertData("IF(HAS(\"CERT\", \"D008\"), A, B)", "when CERT D008 presented then A, otherwise B");
			AssertData("IF(HAS(\"CERT\", \"D008\"), A, B)", RateFormulaHumanReadableVisitorDataForTesting.NewWithAdditionalInformationList("CERT", "certificate"), "when certificate D008 presented then A, otherwise B");
		}

		public void TestHasExpressionContext_NestedIf()
		{
			AssertData("If(has(\"CERT\", \"D017\"), 0, If(has(\"CERT\", \"D018\"), VFD * 0.250, VFD * 0.260))",
				RateFormulaHumanReadableVisitorDataForTesting.NewWithAdditionalInformationList("CERT", "certificate"),
				@"when certificate D017 presented then 0, otherwise
when certificate D018 presented then 25.0% of the Value for Duty, otherwise
26.0% of the Value for Duty");
		}

		public void TestBoolEQContext()
		{
			AssertData("IF(A = B, C, D)", "when A is equal to B then C, otherwise D");
		}

		public void TestBoolGTNEContext()
		{
			AssertData("IF(A != B, C, D)", "when A is not equal to B then C, otherwise D");
		}

		public void TestBoolGTContext()
		{
			AssertData("IF(A > B, C, D)", "when A is greater than B then C, otherwise D");
		}

		public void TestBoolGTEQContext()
		{
			AssertData("IF(A >= B, C, D)", "when A is greater than or equal to B then C, otherwise D");
		}

		public void TestBoolLTContext()
		{
			AssertData("IF(A < B, C, D)", "when A is less than B then C, otherwise D");
		}

		public void TestBoolLTEQContext()
		{
			AssertData("IF(A <= B, C, D)", "when A is less than or equal to B then C, otherwise D");
		}

		public void TestDivisionCapture()
		{
			AssertData("IF(VFD/[HLT] >= 42.5, C, D)", "when value for duty is greater than or equal to 42.5 AUD per HLT then C, otherwise D");
		}

		public void TestBoolAndExpressionContext()
		{
			AssertData("IF(1=1 & 2=2, C, D)", "when 1 is equal to 1 and 2 is equal to 2 then C, otherwise D");
		}

		public void TestBoolExpressionContext()
		{
			AssertData("IF(1=1 | 2=2, C, D)", "when 1 is equal to 1 or 2 is equal to 2 then C, otherwise D");
		}

		public void TestIfExpressionContext()
		{
			AssertData("IF(1=1, C, D)", "when 1 is equal to 1 then C, otherwise D");
		}

		public void TestIfExpressionContext_Nested()
		{
			AssertData("IF(1=1, A, IF(2=2, B, IF(3=3, C, D)))", @"when 1 is equal to 1 then A, otherwise
when 2 is equal to 2 then B, otherwise
when 3 is equal to 3 then C, otherwise
D");
		}

		public void TestIfExpressionContext_ComplexTrueFalse()
		{
			AssertData("If(VFD/[HLT] >= 42.50, VFD * 0.365 + 20.60 * [DTN], VFD * 0.365 + 27.00 * [HLT] + 20.60 * [DTN])",
				"when value for duty is greater than or equal to 42.50 AUD per HLT then 36.5% of the Value for Duty plus 20.60 AUD per DTN, otherwise 36.5% of the Value for Duty plus 27.00 AUD per HLT plus 20.60 AUD per DTN");
		}

		public void TestIfExpressionContext_Long()
		{
			AssertData("If(VFD/[DTN] >= 46.200, VFD * 0.064, If(VFD/[DTN] >= 45.300, VFD * 0.064 + 0.900 * [DTN], If(VFD/[DTN] >= 44.400, VFD * 0.064 + 1.800 * [DTN], If(VFD/[DTN] >= 43.400, VFD * 0.064 + 2.800 * [DTN], If(VFD/[DTN] >= 42.500, VFD * 0.064 + 3.700 * [DTN], If(VFD/[DTN] >= 41.600, VFD * 0.064 + 4.600 * [DTN], If(VFD/[DTN] >= 40.700, VFD * 0.064 + 5.500 * [DTN], If(VFD/[DTN] >= 39.700, VFD * 0.064 + 6.500 * [DTN], If(VFD/[DTN] >= 38.800, VFD * 0.064 + 7.400 * [DTN], VFD * 0.064 + 25.600 * [DTN])))))))))",
				@"when value for duty is greater than or equal to 46.200 AUD per DTN then 6.4% of the Value for Duty, otherwise
when value for duty is greater than or equal to 45.300 AUD per DTN then 6.4% of the Value for Duty plus 0.900 AUD per DTN, otherwise
when value for duty is greater than or equal to 44.400 AUD per DTN then 6.4% of the Value for Duty plus 1.800 AUD per DTN, otherwise
when value for duty is greater than or equal to 43.400 AUD per DTN then 6.4% of the Value for Duty plus 2.800 AUD per DTN, otherwise
when value for duty is greater than or equal to 42.500 AUD per DTN then 6.4% of the Value for Duty plus 3.700 AUD per DTN, otherwise
when value for duty is greater than or equal to 41.600 AUD per DTN then 6.4% of the Value for Duty plus 4.600 AUD per DTN, otherwise
when value for duty is greater than or equal to 40.700 AUD per DTN then 6.4% of the Value for Duty plus 5.500 AUD per DTN, otherwise
when value for duty is greater than or equal to 39.700 AUD per DTN then 6.4% of the Value for Duty plus 6.500 AUD per DTN, otherwise
when value for duty is greater than or equal to 38.800 AUD per DTN then 6.4% of the Value for Duty plus 7.400 AUD per DTN, otherwise
6.4% of the Value for Duty plus 25.600 AUD per DTN");
		}

		public void TestIfExpressionContext_Long2()
		{
			AssertData("If(VFD/[DTN] >= 35.400, 0, If(VFD/[DTN] >= 34.700, 0, If(VFD/[DTN] >= 34.000, 0, If(VFD/[DTN] >= 33.300, 0, If(VFD/[DTN] >= 32.600, 0, If(VFD/[DTN] >= 26.400, 0, If(VFD/[DTN] >= 25.900, 0.500 * [DTN], If(VFD/[DTN] >= 25.300, 1.100 * [DTN], If(VFD/[DTN] >= 24.800, 1.600 * [DTN], If(VFD/[DTN] >= 24.300, 2.100 * [DTN], 7.100 * [DTN]))))))))))",
				@"when value for duty is greater than or equal to 35.400 AUD per DTN then 0, otherwise
when value for duty is greater than or equal to 34.700 AUD per DTN then 0, otherwise
when value for duty is greater than or equal to 34.000 AUD per DTN then 0, otherwise
when value for duty is greater than or equal to 33.300 AUD per DTN then 0, otherwise
when value for duty is greater than or equal to 32.600 AUD per DTN then 0, otherwise
when value for duty is greater than or equal to 26.400 AUD per DTN then 0, otherwise
when value for duty is greater than or equal to 25.900 AUD per DTN then 0.500 AUD per DTN, otherwise
when value for duty is greater than or equal to 25.300 AUD per DTN then 1.100 AUD per DTN, otherwise
when value for duty is greater than or equal to 24.800 AUD per DTN then 1.600 AUD per DTN, otherwise
when value for duty is greater than or equal to 24.300 AUD per DTN then 2.100 AUD per DTN, otherwise
7.100 AUD per DTN");
		}

		public void TestIfExpressionContext_Long3()
		{
			AssertData("If(CIF/[DTN] >= 118.080, 0, If(CIF/[DTN] >= 78.720, 35.424 * [DTN]- CIF * 0.300, If(CIF/[DTN] >= 52.480, 51.168 * [DTN]- CIF * 0.500, If(CIF/[DTN] >= 32.800, 61.664 * [DTN]- CIF * 0.700, 68.224 * [DTN]- CIF * 0.900))))",
				@"when CIF is greater than or equal to 118.080 AUD per DTN then 0, otherwise
when CIF is greater than or equal to 78.720 AUD per DTN then 35.424 AUD per DTN minus 30.0% of CIF, otherwise
when CIF is greater than or equal to 52.480 AUD per DTN then 51.168 AUD per DTN minus 50.0% of CIF, otherwise
when CIF is greater than or equal to 32.800 AUD per DTN then 61.664 AUD per DTN minus 70.0% of CIF, otherwise
68.224 AUD per DTN minus 90.0% of CIF");
		}

		public void TestMinExpressionContext()
		{
			AssertData("MIN(A, B)", "A, or B, whichever is lesser");
		}

		public void TestMaxExpressionContext()
		{
			AssertData("MAX(A, B)", "A, or B, whichever is greater");
		}

		public void TestRoundExpressionContext()
		{
			AssertData("ROUND(A, 3)", "round A to 3 decimal places");
		}

		public void TestMultiplyingExpressionContext()
		{
			AssertData("A * B", "A multiplied by B");
		}

		public void TestMultiplyingExpressionContext_UomPlaceHolderContext()
		{
			AssertData("12.30 * [DTN]", "12.30 AUD per DTN", 12.3m);
			AssertData("[DTN] * 12.30", "12.30 AUD per DTN", 12.3m);
		}

		public void TestMultiplyingExpressionContext_MeursingPlaceHolderContext()
		{
			var rateFormulaHumanReadableVisitor = RateFormulaHumanReadableVisitorDataForTesting.NewWithMeursingExpressionList("ADSZ(1)", "Additional Sugar Element");
			AssertData("12.30 * #ADSZ(1)#", rateFormulaHumanReadableVisitor, "12.30 multiplied by Additional Sugar Element");
		}

		public void TestMultiplyingExpressionContext_ReservedVFDContext()
		{
			AssertData("VFD * 0.123", "12.3% of the Value for Duty", 0.123m);
			AssertData("0.123 * VFD", "12.3% of the Value for Duty", 0.123m);

			using (var mockGrm = Res.GetLanguageInstance(Core.SharedConstants.Languages.German).UseMockData())
			{
				mockGrm.Put("EECD2168-44DA-4E1B-9F99-16AA19469897", new ResourceStringData("EECD2168-44DA-4E1B-9F99-16AA19469897", "{0} des Zollbetrags"));
				using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.German))
				{
					AssertEquals("de-DE", Culture.GetCultureForLanguage(Res.CurrentLanguage).Name);
					AssertData("VFD * 0.123", "12,3 % des Zollbetrags", 0.123m);
					AssertData("0.123 * VFD", "12,3 % des Zollbetrags", 0.123m);
				}
			}
		}

		public void TestMultiplyingExpressionContext_ReservedVFDContext_NonNumeric()
		{
			AssertData("MAX(A, B) * VFD", "Greater of A and B, multiplied by value for duty");
		}

		public void TestMultiplyingExpressionContext_VarCountrySpecificKeywordContext()
		{
			AssertData("0.51*PVP + 24.7*[MIL]", "51.0% of PVP plus 24.7 AUD per MIL");
			AssertData("PVP*0.51 + 24.7*[MIL]", "51.0% of PVP plus 24.7 AUD per MIL");

			using (var mockGrm = Res.GetLanguageInstance(Core.SharedConstants.Languages.German).UseMockData())
			{
				mockGrm.Put("8D403559-6E38-40F3-9788-0144C14312F7", new ResourceStringData("8D403559-6E38-40F3-9788-0144C14312F7", "{0} von {1}"));
				mockGrm.Put("1D2D4E19-52EB-4F0A-9952-370C905E0A62", new ResourceStringData("1D2D4E19-52EB-4F0A-9952-370C905E0A62", "{0} {1} pro {2}"));
				using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.German))
				{
					AssertEquals("de-DE", Culture.GetCultureForLanguage(Res.CurrentLanguage).Name);
					AssertData("0.51*PVP + 24.7*[MIL]", "51,0 % von PVP plus 24.7 AUD pro MIL");
					AssertData("PVP*0.51 + 24.7*[MIL]", "51,0 % von PVP plus 24.7 AUD pro MIL");
				}
			}
		}

		public void TestMultiplyingExpressionContext_DivExpContext()
		{
			AssertData("A / B", "A / B");
		}

		public void TestContExpressionContext()
		{
			AssertData("A + B", "A plus B");
		}

		public void TestContExpressionContext_VarCountrySpecificKeywordContext()
		{
			AssertData("1P1+12A+12B", "1P1 plus 12A plus 12B");
			var data = RateFormulaHumanReadableVisitorDataForTesting.New();
			data.CountrySpecificList.Add("1P1", "Schedule 1 Part 1 duty");
			data.CountrySpecificList.Add("12A", "Schedule 1 Part 2A duty");
			data.CountrySpecificList.Add("12B", "Schedule 1 Part 2B duty");
			AssertData("1P1+12A+12B", data, "Schedule 1 Part 1 duty plus Schedule 1 Part 2A duty plus Schedule 1 Part 2B duty");
		}

		public void TestContExpressionContext_MINUS()
		{
			AssertData("VFD - 10", "value for duty minus 10");
			AssertData("10 - VFD", "10 minus value for duty");
			AssertData("1P1-12A-12B", "1P1 minus 12A minus 12B");
		}

		public void TestNumberContext_Percent()
		{
			AssertData("12% * VFD", "12.0% of the Value for Duty", 0.12m);
		}

		public void TestReservedDOVContext()
		{
			AssertData("If(DOV < 20211115, A, B)", "when date of valuation is less than 20211115 then A, otherwise B");
		}

		public void TestFormulaSpecificValueContext()
		{
			AssertData("{\"Rebate Amount\"}", "Rebate Amount");
		}

		public void TestIssue14()
		{
			AssertData("MAX(347.860 * [MIL], 16.500 * [RET] + 262.900 * [MIL])", "347.860 AUD per MIL, or 16.500 AUD per RET plus 262.900 AUD per MIL, whichever is greater");
		}

		public void TestIssue15()
		{
			AssertData("MIN(VFD * 0.056 + 45.100 * [DTN], VFD * 0.189 + 16.500 * [DTN])", "5.6% of the Value for Duty plus 45.100 AUD per DTN, or 18.9% of the Value for Duty plus 16.500 AUD per DTN, whichever is lesser");
		}

		public void TestIssue18()
		{
			AssertData("MIN(MAX(VFD * 0.112, 22.000 * [DTN]), 56.000 * [DTN])", "Greater of 11.2% of the Value for Duty, and 22.000 AUD per DTN, but not more than 56.000 AUD per DTN");
			AssertData("MIN(56.000 * [DTN], MAX(VFD * 0.112, 22.000 * [DTN]))", "Greater of 11.2% of the Value for Duty, and 22.000 AUD per DTN, but not more than 56.000 AUD per DTN");
		}

		public void TestCalculate()
		{
			CombineAssertions("TestCase: 0.7 * VFD", () =>
			{
				AssertData("0.7 * VFD", "70.0% of the Value for Duty", 0.7m);
			});

			CombineAssertions("TestCase: VFD * 0.7", () =>
			{
				AssertData("VFD * 0.7", "70.0% of the Value for Duty", 0.7m);
			});

			CombineAssertions("TestCase: 70 * [DTN]", () =>
			{
				AssertData("70 * [DTN]", "70 AUD per DTN", 70m);
			});

			CombineAssertions("TestCase: 70 * [DTN] * [MIL]", () =>
			{
				AssertData("70 * [DTN] * [MIL]", "70 AUD per DTN and MIL", 70m);
			});

			CombineAssertions("TestCase: 70 * [MIL] * [DTN]", () =>
			{
				AssertData("70 * [MIL] * [DTN]", "70 AUD per MIL and DTN", 70m);
			});

			CombineAssertions("TestCase: MAX(VFD * 0.7, 70 * [DTN])", () =>
			{
				AssertData("MAX(VFD * 0.7, 70 * [DTN])", "70.0% of the Value for Duty, or 70 AUD per DTN, whichever is greater");
			});

			CombineAssertions("TestCase: MIN(VFD * 0.7, 70 * [DTN])", () =>
			{
				AssertData("MIN(VFD * 0.7, 70 * [DTN])", "70.0% of the Value for Duty, or 70 AUD per DTN, whichever is lesser");
			});

			CombineAssertions("TestCase: VFD * 0.224 + 13.730 * [DTN]", () =>
			{
				AssertData("VFD * 0.224 + 13.730 * [DTN]", "22.4% of the Value for Duty plus 13.730 AUD per DTN");
			});

			CombineAssertions("TestCase: MIN(350 * [FLAT], MAX(1.5 * [TNE3], 30 * [FLAT]))", () =>
			{
				AssertData("MIN(350 * [FLAT], MAX(1.5 * [TNE3], 30 * [FLAT]))", "Greater of 1.5 AUD per TNE3, and 30 AUD per FLAT, but not more than 350 AUD per FLAT");
			});

			CombineAssertions("TestCase: IF(HAS(\"CERT\", \"D008\"), VFD * 0.107, VFD * 0.172)", () =>
			{
				AssertData("IF(HAS(\"CERT\", \"D008\"), VFD * 0.107, VFD * 0.172)", RateFormulaHumanReadableVisitorDataForTesting.NewWithAdditionalInformationList("CERT", "certificate"), "when certificate D008 presented then 10.7% of the Value for Duty, otherwise 17.2% of the Value for Duty");
			});

			CombineAssertions("TestCase: IF(HAS(\"CERT\", \"D008\"), VFD * 0.107, 70 * [DTN])", () =>
			{
				AssertData("IF(HAS(\"CERT\", \"D008\"), VFD * 0.107, 70 * [DTN])", RateFormulaHumanReadableVisitorDataForTesting.NewWithAdditionalInformationList("CERT", "certificate"), "when certificate D008 presented then 10.7% of the Value for Duty, otherwise 70 AUD per DTN");
			});
		}

		#region Implementation

		static void AssertData(string testFormula, string expectedResult, decimal? expectedNumber = null)
			=> AssertData(testFormula, RateFormulaHumanReadableVisitorDataForTesting.New(), expectedResult, expectedNumber: expectedNumber);

		static void AssertData(string testFormula, RateFormulaHumanReadableVisitorData data, string expectResult, int expectedErrorsInParsing = 0, string[] expectedErrors = null, decimal? expectedNumber = null)//, QuestionForFormulaSpecificValue answer = null)
		{
			var errorListener = new FormulaErrorListener();

			var input = new AntlrInputStream(testFormula);
			var lexer = new RateFormulaLexer(input);
			var tokens = new CommonTokenStream(lexer);
			var parser = new RateFormulaParser(tokens);
			parser.AddErrorListener(errorListener);
			var tree = parser.expression();
			AssertEquals("No Error in parsing", expectedErrorsInParsing, errorListener.Errors.Count());
			AssertNotNull("Expression Tree Not null", tree);
			var result = new RateFormulaHumanReadableVisitor(data, errorListener).VisitExpression(tree);
			AssertEquals($"{testFormula} HumanReadableString", expectResult, result.HumanReadableString);
			AssertEquals($"{testFormula} Number", expectedNumber, result.Number);
			if (expectedErrors == null)
			{
				AssertEquals("No Error in Extracting", 0, errorListener.Errors.Count());
				AssertContainsExactElementsInAnyOrder(Array.Empty<string>(), errorListener.Errors);
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
