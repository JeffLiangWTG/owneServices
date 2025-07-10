using System;
using CargoWise.RefDbRepo.SEReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNCommonImportTariffPopulator.Test
{
	[TestFixture]
	internal class MeasureConditionCodeFilterFixture
	{
		[Test]
		public void IsValid()
		{
			var filter = new MeasureConditionCodeFilter();
			var type1 = new measureConditionCode();
			var type2 = new measureConditionCode { nationalSpecified = true, national = 0L };
			var type3 = new measureConditionCode { nationalSpecified = true, national = 1L };
			var type4 = new measureConditionCode { nationalSpecified = true, national = 0L, dateEndSpecified = true, dateEnd = DateTime.UtcNow.AddDays(10) };
			var type5 = new measureConditionCode { nationalSpecified = true, national = 0L, dateEndSpecified = true, dateEnd = DateTime.UtcNow.AddDays(-10) };
			Assert.IsFalse(filter.IsValid(type1, DateTime.UtcNow));
			Assert.IsTrue(filter.IsValid(type2, DateTime.UtcNow));
			Assert.IsFalse(filter.IsValid(type3, DateTime.UtcNow));
			Assert.IsTrue(filter.IsValid(type4, DateTime.UtcNow));
			Assert.IsFalse(filter.IsValid(type5, DateTime.UtcNow));
		}
	}
}
