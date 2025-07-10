using CargoWise.RefDbRepo.SEReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNCommonImportTariffPopulator.Test
{
	[TestFixture]
	internal class PreferenceCreatorFixture
	{
		[Test]
		public void Get()
		{
			var creator = new PreferenceCreator();
			var measure = new measure { measureType = "103" };
			CollectionAssert.AreEqual(new[] { "100", "150" }, creator.Get(measure));
		}
	}
}
