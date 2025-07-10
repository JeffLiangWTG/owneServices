using System.Collections;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer.Test
{
	[TestFixture]
	public class MeasuringUnitTransformerFixture
	{
		[TestCaseSource(nameof(TransformTestCases))]
		public void Transform(string rawRateFormula, string expectedResult)
		{
			IMeasuringUnitTransformer transformer = new MeasuringUnitTransformer();

			var result = transformer.Transform(rawRateFormula);
			Assert.AreEqual(expectedResult, result);
		}

		protected static IEnumerable TransformTestCases
		{
			get
			{
				yield return new TestCaseData("63.000 TNE ", "63.000 * [TNE]")
				{
					TestName = "{m}_WhenKeywordHasNoSpace"
				};

				yield return new TestCaseData("3.800 % MIN 0.600 DTN G ", "3.800 % MIN 0.600 * [DTNG]")
				{
					TestName = "{m}_WhenKeywordHasSpace"
				};

				yield return new TestCaseData("51.500 /DTN(01)", "51.500 * [DTN]")
				{
					TestName = "{m}_WhenKeywordIsFollowedByParenthesis"
				};

				yield return new TestCaseData("51.500 / DTN(01)", "51.500 * [DTN]")
				{
					TestName = "{m}_WhenKeywordIsPreceedBySpaceAndFollowedByParenthesis"
				};

				var condition = @"Cond:  V 52.600 /DTN(01):12.000 % ; V 51.500 /DTN(01):12.000 % + 1.100  DTN ; V 50.500 /DTN(01):12.000 % + 2.100  DTN ; V 49.400 /DTN(01):12.000 % + 3.200  DTN ; V 48.400 /DTN(01):12.000 % + 4.200  DTN ; V 0.000 /DTN(01):12.000 % + 29.800  DTN ";
				var expectedResult = @"Cond:  V 52.600 * [DTN]:12.000 % ; V 51.500 * [DTN]:12.000 % + 1.100 * [DTN]; V 50.500 * [DTN]:12.000 % + 2.100 * [DTN]; V 49.400 * [DTN]:12.000 % + 3.200 * [DTN]; V 48.400 * [DTN]:12.000 % + 4.200 * [DTN]; V 0.000 * [DTN]:12.000 % + 29.800 * [DTN]";
				yield return new TestCaseData(condition, expectedResult)
				{
					TestName = "{m}_WhenKeywordIsInCondition"
				};

				yield return new TestCaseData("1.31KGM P + 22.00DTN", "1.31 * [KGMP] + 22.00 * [DTN]")
				{
					TestName = "{m}_WhenMulitpleUOMExists"
				};

				yield return new TestCaseData("10.000 % MIN 22.000 DTN MAX 56.000 DTN", "10.000 % MIN 22.000 * [DTN] MAX 56.000 * [DTN]")
				{
					TestName = "{m}_DTN_MAX"
				};
			}
		}

		[TestCase("0.500 DTN", "0.500")]
		public void ReplaceUom(string rawRateFormula, string expectedResult)
		{
			IMeasuringUnitTransformer transformer = new MeasuringUnitTransformer();

			var result = transformer.ReplaceUom(rawRateFormula, string.Empty);
			Assert.AreEqual(expectedResult, result);
		}
	}
}
