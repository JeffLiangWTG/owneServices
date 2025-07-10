using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNCommonImportTariffPopulator.Test
{
	[TestFixture]
	internal class RateTypeCreatorFixture
	{
		public void Get()
		{
			var creator = new RateTypeCreator();
			Assert.AreEqual("DTY", creator.Get("A00"));
			Assert.AreEqual("DTY", creator.Get("A20"));
			Assert.AreEqual("ADD", creator.Get("A30"));
			Assert.AreEqual("ADD", creator.Get("A35"));
			Assert.AreEqual("CVD", creator.Get("A40"));
			Assert.AreEqual("CVD", creator.Get("A45"));
		}
	}
}
