using System.Linq;
using CargoWise.RefDbRepo.SEReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNCommonImportTariffPopulator.Test
{
	[TestFixture]
	internal class TariffUOMCreatorFixture
	{
		[Test]
		public void Get_measure_measureComponent()
		{
			var creator = new TariffUOMCreator();
			var measure = new measure
			{
				measureType = "103",
				geographicalAreaId = "CN",
				measureComponent = new[] { new measureComponent { measurementUnitCode = "DTN" } }
			};
			var results = creator.Get(measure).ToArray();
			Assert.AreEqual(0, results.Length);

			measure = new measure
			{
				measureType = "109",
				geographicalAreaId = "BR",
				measureComponent = new[] { new measureComponent { measurementUnitCode = "DTN", measurementUnitQualifierCode = "A" } }
			};
			results = creator.Get(measure).ToArray();
			Assert.AreEqual(1, results.Length);
			Assert.AreEqual("DTNA", results[0].ZZ8_UOM);
			Assert.AreEqual("CU2", results[0].ZZ8_Type);
			Assert.AreEqual("BR", results[0].ZZ8_ZZA_NKTradeGroup);

			measure = new measure
			{
				measureType = "110",
				geographicalAreaId = "CN",
				measureComponent = new[] { new measureComponent { measurementUnitCode = "TNE" } }
			};
			results = creator.Get(measure).ToArray();
			Assert.AreEqual(1, results.Length);
			Assert.AreEqual("TNE", results[0].ZZ8_UOM);
			Assert.AreEqual("CU2", results[0].ZZ8_Type);
			Assert.AreEqual("CN", results[0].ZZ8_ZZA_NKTradeGroup);
		}

		[Test]
		public void GetDefaultUOM()
		{
			var defaultUOM = TariffUOMCreator.GetDefaultTariffUOM;
			Assert.IsNotNull(defaultUOM);
			Assert.AreEqual("KGM", defaultUOM.ZZ8_UOM);
			Assert.AreEqual("CU1", defaultUOM.ZZ8_Type);
		}
	}
}
