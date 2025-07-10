using System;
using CargoWise.RefDbRepo.SEReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNCommonImportTariffPopulator.Test
{
	[TestFixture]
	internal class BaseRegulationFilterFixture
	{
		[Test]
		public void IsValid()
		{
			var filter = new BaseRegulationFilter();
			var reg1 = new baseRegulation();
			var reg2 = new baseRegulation { nationalSpecified = true, national = 0L };
			var reg3 = new baseRegulation { nationalSpecified = true, national = 1L };
			var reg4 = new baseRegulation { nationalSpecified = true, national = 0L, dateEndSpecified = true, dateEnd = DateTime.UtcNow.AddDays(10) };
			var reg5 = new baseRegulation { nationalSpecified = true, national = 0L, dateEndSpecified = true, dateEnd = DateTime.UtcNow.AddDays(-10) };
			Assert.IsFalse(filter.IsValid(reg1, DateTime.UtcNow));
			Assert.IsTrue(filter.IsValid(reg2, DateTime.UtcNow));
			Assert.IsFalse(filter.IsValid(reg3, DateTime.UtcNow));
			Assert.IsTrue(filter.IsValid(reg4, DateTime.UtcNow));
			Assert.IsFalse(filter.IsValid(reg5, DateTime.UtcNow));
		}
	}
}
