using CargoWise.RefDbRepo.SEReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNCommonImportTariffPopulator.Test
{
	[TestFixture]
	internal class RateCodeCreatorFixture
	{
		[TestCase("ABC", "C", "A00")]
		[TestCase("ABC", "J", "A20")]
		[TestCase("652", "S", "A20")]
		[TestCase("651", "S", "")]
		[TestCase("551", "D", "A35")]
		public void Get(string measureId, string measureSeriesId, string expected)
		{
			var measureType = new measureType1
			{
				measureType = measureId,
				measureTypeSeriesId = measureSeriesId
			};
			var creator = new RateCodeCreator(new[] { measureType });
			Assert.AreEqual(expected, creator.Get(new measure {  measureType = measureId }));
		}
	}
}
