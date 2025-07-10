using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffPopulator.Test
{
	[TestFixture]
	public class EUNTariffTypeTest
	{
		[TestCase("CUSTOMS UNION DUTY", "IMP")]
		[TestCase("Key that does not exist", null)]
		public void GetEUNTariffTypeTest(string input, string result)
		{
			System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
			Assert.AreEqual(EUNDescriptionMapping.GetEUNMapping(input)?.TariffType, result);
		}
	}
}
