using CargoWise.RefDbRepo.SEReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNCommonImportTariffPopulator.Test
{
	[TestFixture]
	internal class ApplicabilityCreatorFixture
	{
		[Test]
		public void Get()
		{
			var measure = new measure
			{
				geographicalAreaId = "2200",
				additionalCodeId = "249",
				additionalCodeType = "A",
				quotaOrderNumber = "094112",
				dateEnd = new System.DateTime(2025, 10, 31, 0, 0, 0),
				dateEndSpecified = true
			};
			var app = ApplicabilityCreator.Get(measure);
			Assert.AreEqual("2200", app.ZZT_ZZA_NKTradeGroup);
			Assert.AreEqual("A249", app.ZZT_AdditionalCode);
			Assert.AreEqual("094112", app.ZZT_OrderNumber);
			Assert.AreEqual(new System.DateTime(2025, 10, 31, 23, 59, 0), app.ZZT_EndDate);
		}
	}
}
