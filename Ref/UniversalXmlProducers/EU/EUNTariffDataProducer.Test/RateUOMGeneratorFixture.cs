using System.Linq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer.Test
{
	[TestFixture]
	public class RateUOMGeneratorFixture
	{
		[TestCase(@"1 * [KGME]", new[] { "KGME" })]
		[TestCase(@"1 * [KGME] + 2 * [KGME]", new[] { "KGME" })]
		[TestCase(@"1 * [KGME] + 2 * [DTN]", new[] { "KGME", "DTN" })]
		[TestCase(@"1 * [ENP]", new[] { "ENP" })]
		public void RateUOMGeneratorTest(string rateFormula, string[] uom)
		{
			var result = RateUOMGenerator.GenerateRateUomRecords(rateFormula);
			Assert.IsTrue(result.All(x => uom.Contains(x.ZXG_UOM)), "Actual does not match expected result", new object[] { rateFormula, uom });
		}
	}
}
