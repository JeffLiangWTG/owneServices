using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer.Test
{
	public class AgriculturalComponentFormulaExtractorFixture
	{
		[TestCaseSource(nameof(MultipleFormulaExtractorTestCases))]
		public void MultipleFormulaExtractorTest(string formula, IList<IFormulaExtractionResult> expectedFormulaExtractionResults)
		{
			const string reductionIndicator = "1";
			const string rateCode = "A00";
			var a20FormulaExtractor = new AgriculturalComponentFormulaExtractor(new MeasuringUnitTransformer());

			var result = a20FormulaExtractor.GetFormula(formula, rateCode, reductionIndicator).ToList();

			Assert.That(result.Count, Is.EqualTo(expectedFormulaExtractionResults.Count));
			for (var i = 0; i < result.Count; i++)
			{
				Assert.That(result.ElementAt(i).Formula, Is.EqualTo(expectedFormulaExtractionResults.ElementAt(i).Formula));
				Assert.That(result.ElementAt(i).RateCode, Is.EqualTo(expectedFormulaExtractionResults.ElementAt(i).RateCode));
			}
		}

		protected static IEnumerable MultipleFormulaExtractorTestCases
		{
			get
			{
				var formulaExtractorResult1 = new FormulaExtractionResult("VFD * 0.055 + #EA(1)#", "A00");

				yield return new TestCaseData("5.500 % + EA", new List<IFormulaExtractionResult>
				{
					formulaExtractorResult1
				});

				formulaExtractorResult1 = new FormulaExtractionResult("MIN(VFD * 0.090 + #EA(1)#, VFD * 0.187 +#ADSZ(1)#)", "A00");
				yield return new TestCaseData("9.000 % + EA MAX 18.700 % +ADSZ", new List<IFormulaExtractionResult>
				{
					formulaExtractorResult1
				});

				formulaExtractorResult1 = new FormulaExtractionResult("MIN(MIN(VFD * 0.045 + #EAR(1)#, VFD * 0.093 +#ADSZR(1)#), 35.150 * [DTN])", "A00");
				yield return new TestCaseData("4.500 % + EAR MAX 9.300 % +ADSZR MAX 35.150 EUR DTN",
					new List<IFormulaExtractionResult>
					{
						formulaExtractorResult1
					});

				formulaExtractorResult1 = new FormulaExtractionResult("MIN(0 + #EA(1)#, 0 +#ADSZ(1)#)", "A00");
				yield return new TestCaseData("0.000 % + EA MAX 0.000 % +ADSZ",
					new List<IFormulaExtractionResult>
					{
						formulaExtractorResult1
					});

				formulaExtractorResult1 = new FormulaExtractionResult("MIN(VFD * 0.100 + #EA(1)#, VFD * 0.100 +#ADSZ(1)#)", "A00");
				yield return new TestCaseData("10.000 % + EA MAX 10.000 % +ADSZ",
					new List<IFormulaExtractionResult>
					{
						formulaExtractorResult1
					});

				formulaExtractorResult1 = new FormulaExtractionResult("MIN(VFD * 0.055 + #EAR(1)#, VFD * 0.155 +#ADSZR(1)#)", "A00");
				yield return new TestCaseData("5.50 % + EAR MAX 15.50 % +ADSZR",
					new List<IFormulaExtractionResult>
					{
						formulaExtractorResult1
					});
			}
		}
	}
}
