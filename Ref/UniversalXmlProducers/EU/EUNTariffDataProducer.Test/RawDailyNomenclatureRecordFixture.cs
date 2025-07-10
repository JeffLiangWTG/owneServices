using System;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer.Test
{
	sealed class RawDailyNomenclatureRecordFixture
	{
		[TestCase("382430001010", false)]
		[TestCase("382430001080", true)]
		public void RawDailyNomenclatureRecord_WhenCreated_ShouldSetPropertyLeafCorrectly(string tariffHeader, bool expectedIsLeaf)
		{
			var endDate = DateTime.Now.AddDays(1);
			var record = new RawDailyNomenclatureRecord(
				tariffHeader,
				DateTime.Now.AddDays(-1),
				endDate,
				"EUN",
				"1",
				"-",
				"DESC",
				"INSERT",
				DateTime.Now.AddDays(-1),
				1,
				"Nomenclatrue_20241128");
			Assert.That(record.IsLeaf, Is.EqualTo(expectedIsLeaf), $"{tariffHeader} Is Leaf?");
			IRawNomenclatureRecord rawNomenclatureRecord = record;
			Assert.That(rawNomenclatureRecord.EndDate, Is.EqualTo(endDate));
		}
	}
}
