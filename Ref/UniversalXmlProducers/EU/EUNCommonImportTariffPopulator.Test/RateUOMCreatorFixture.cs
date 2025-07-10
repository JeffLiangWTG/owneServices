using System.Linq;
using CargoWise.RefDbRepo.SEReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNCommonImportTariffPopulator.Test
{
	[TestFixture]
	internal class RateUOMCreatorFixture
	{
		[Test]
		public void Get_measurecomponent()
		{
			var creator = new RateUOMCreator();
			var results = creator.Get(new[] {
				new measureComponent { measurementUnitCode = "DTN" },
				new measureComponent { measurementUnitCode = "DTN" },
				new measureComponent { measurementUnitCode = "TNE" } }).ToArray();
			Assert.AreEqual(2, results.Length);
			Assert.AreEqual("DTN", results[0].ZXG_UOM);
			Assert.AreEqual("TNE", results[1].ZXG_UOM);
		}
	}
}
