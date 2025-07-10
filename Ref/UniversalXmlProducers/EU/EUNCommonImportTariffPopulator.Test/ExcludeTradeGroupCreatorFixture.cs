using System.Linq;
using CargoWise.RefDbRepo.SEReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNCommonImportTariffPopulator.Test
{
	[TestFixture]
	internal class ExcludeTradeGroupCreatorFixture
	{
		[Test]
		public void Get()
		{
			var measure = new measure
			{
				measureExcludedGeographicalArea = new[] {
					new measureExcludedGeographicalArea { geographicalAreaId = "GB" },
					new measureExcludedGeographicalArea { geographicalAreaId = "AU" }
				}
			};
			var ex = new ExcludeTradeGroupCreator().Get(measure).ToArray();
			Assert.AreEqual("GB", ex[0].ZZC_ZZA_NKTradeGroup);
			Assert.AreEqual("AU", ex[1].ZZC_ZZA_NKTradeGroup);
		}
	}
}
