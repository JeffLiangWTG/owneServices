using System;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNCommonImportTariffPopulator.Test
{
	[TestFixture]
	internal class MeasureTypeFilterFixture
	{
		[Test]
		public void IsValid()
		{
			var filter = new MeasureTypeFilter();
			var type1 = new measureType1();
			var type2 = new measureType1 { nationalSpecified = true, national = 0L };
			var type3 = new measureType1 { nationalSpecified = true, national = 1L };
			var type4 = new measureType1 { nationalSpecified = true, national = 0L, dateEndSpecified = true, dateEnd = DateTime.UtcNow.AddDays(10) };
			var type5 = new measureType1 { nationalSpecified = true, national = 0L, dateEndSpecified = true, dateEnd = DateTime.UtcNow.AddDays(-10) };
			Assert.IsFalse(filter.IsValid(type1, DateTime.UtcNow));
			Assert.IsTrue(filter.IsValid(type2, DateTime.UtcNow));
			Assert.IsFalse(filter.IsValid(type3, DateTime.UtcNow));
			Assert.IsTrue(filter.IsValid(type4, DateTime.UtcNow));
			Assert.IsFalse(filter.IsValid(type5, DateTime.UtcNow));
		}
	}
}
