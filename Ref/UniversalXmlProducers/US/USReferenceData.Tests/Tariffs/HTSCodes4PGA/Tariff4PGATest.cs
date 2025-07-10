using CargoWise.RefDbRepo.USReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.USReferenceData.Tests
{
	[TestFixture]
	public class Tariff4PGATest
	{
		[Test]
		public void TestTariff4PGA()
		{
			var tariff4PGA = new Tariff4PGA("2500.10", true, "AES");
			Assert.AreEqual("2500.10", tariff4PGA.TariffCode);
			Assert.AreEqual(true, tariff4PGA.IsMandatory);
			Assert.AreEqual("AES", tariff4PGA.PGACode);
		}
	}
}
