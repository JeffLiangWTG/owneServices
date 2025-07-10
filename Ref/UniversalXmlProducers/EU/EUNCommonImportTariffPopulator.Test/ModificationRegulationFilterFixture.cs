using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.SEReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNCommonImportTariffPopulator.Test
{
	[TestFixture]
	internal class ModificationRegulationFilterFixture
	{
		[Test]
		public void IsValid()
		{
			var filter = new ModificationRegulationFilter();
			var reg1 = new modificationRegulation();
			var reg2 = new modificationRegulation { nationalSpecified = true, national = 0L };
			var reg3 = new modificationRegulation { nationalSpecified = true, national = 1L };
			var reg4 = new modificationRegulation { nationalSpecified = true, national = 0L, dateEndSpecified = true, dateEnd = DateTime.UtcNow.AddDays(10) };
			var reg5 = new modificationRegulation { nationalSpecified = true, national = 0L, dateEndSpecified = true, dateEnd = DateTime.UtcNow.AddDays(-10) };
			Assert.IsFalse(filter.IsValid(reg1, DateTime.UtcNow));
			Assert.IsTrue(filter.IsValid(reg2, DateTime.UtcNow));
			Assert.IsFalse(filter.IsValid(reg3, DateTime.UtcNow));
			Assert.IsTrue(filter.IsValid(reg4, DateTime.UtcNow));
			Assert.IsFalse(filter.IsValid(reg5, DateTime.UtcNow));
		}
	}
}
