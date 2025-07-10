using System;
using System.Collections.Generic;
using System.IO;
using Antlr4.Runtime;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class SGRateFormulaExtractionVisitorTest : TestCaseWithFactory
	{
		public void TestVisitExpression()
		{
			var errorListener = new FormulaErrorListener();
			var visitor = new SGRateFormulaExtractionVisitor(errorListener, new HashSet<ZString>()
			{ "KG" });
			AssertValidRate("Valid VFD Rate", "VFD * 0.2");
			AssertValidRate("Valid VFD Rate", "0.2 * VFD");
			AssertInvalidRate("Should only have one VFD leaf in expression tree", "VFD * VFD", null);
			AssertInvalidRate("Should only have one rate value leaf in expression tree", "0.2 * 0.2", null);
			AssertValidRate("Valid UOM Rate", "[KG] * 0.2");
			AssertValidRate("Valid UOM Rate", "0.2 * [KG]");
			AssertInvalidRate("UOM 'LA' not expected", "0.2 * [LA]", "UOMNotFound:Unit of Measure code: LA is not specified");
			AssertInvalidRate("UOM 'LA' not expected", "[LA] * 0.2", "UOMNotFound:Unit of Measure code: LA is not specified");
			AssertInvalidRate("Should only have one UOM leaf in expression tree", "[KG] * [KG]", null);
			AssertInvalidRate("Should only have one UOM leaf in expression tree", "[KG] * [KG] * 0.2", null);
			AssertInvalidRate("Invalid UOM", "KG * 0.2", "SyntaxError:Unexpected expression context: VarCountrySpecificKeywordContext");
			void AssertInvalidRate(string message, string formula, string expectedError) => AssertRate(message, formula, true, expectedError);
			void AssertValidRate(string message, string formula) => AssertRate(message, formula, false, null);
			void AssertRate(string message, string formula, bool expectedResult, string expectedError)
			{
				AssertEquals(message, expectedResult, visitor.VisitExpression(GetExpressionTree(formula)));
				if (expectedError != null)
				{
					AssertCollectionContains(message, errorListener.Errors, t => t.ToString().Contains(expectedError));
				}
			}
		}

		public void TestExtractSGRate()
		{
			var errorListener = new FormulaErrorListener();
			var visitor = new SGRateFormulaExtractionVisitor(errorListener, new HashSet<ZString>()
			{ "KG" });
			AssertRate("Valid VFD Rate", "VFD * 0.2", new SGTariffRate(20.0m, 0m, ""));
			AssertRate("Valid VFD Rate", "0.2 * VFD", new SGTariffRate(20m, 0m, ""));
			AssertRate("Valid VFD Rate", "VFD * 1.4", new SGTariffRate(140m, 0m, ""));
			AssertRate("Valid VFD Rate", "1.4 * VFD", new SGTariffRate(140m, 0m, ""));
			AssertRate("Valid UOM Rate", "[KG] * 20", new SGTariffRate(0m, 20m, "KG"));
			AssertRate("Valid UOM Rate", "20 * [KG]", new SGTariffRate(0m, 20m, "KG"));
			void AssertRate(string message, string formula, SGTariffRate expectedRate)
			{
				var sgRate = visitor.ExtractSGRate(GetExpressionTree(formula));
				AssertEquals(message, expectedRate, sgRate);
			}
		}

		public void TestExtractSGRateStatic()
		{
			AssertRate("Valid VFD Rate", "VFD * 0.2", new SGTariffRate(20.0m, 0m, ""));
			AssertRate("Valid UOM Rate", "[KG] * 20", new SGTariffRate(0m, 20m, "KG"));
			AssertRate("Inalid VFD Rate", "VFD + 20", null);
			AssertRate("Inalid VFD Rate", "[KG] + 20", null);
			AssertRate("Inalid VFD Rate", "$[KG] + 20", null);
			void AssertRate(string message, string formula, SGTariffRate expectedRate)
			{
				var consolError = Console.Error;
				try
				{
					using (var writer = new StringWriter())
					{
						Console.SetError(writer);
						var sgRate = SGRateFormulaExtractionVisitor.ExtractSGRate(formula, new HashSet<ZString>()
						{ "KG" });
						AssertEquals(message, expectedRate, sgRate);
						AssertEquals("Expecting no logging to console", "", writer.ToString());
					}
				}
				finally
				{
					Console.SetError(consolError);
				}
			}
		}

		RateFormulaParser.ExpressionContext GetExpressionTree(ZString formulaString)
		{
			var input = new AntlrInputStream(formulaString);
			var lexer = new RateFormulaLexer(input);
			var tokens = new CommonTokenStream(lexer);
			var parser = new RateFormulaParser(tokens);
			return parser.expression();
		}
	}
}
