using System;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor.Test
{
	[TestFixture]
	class DateTimeRangeFixture
	{
		[Test]
		public void Equals()
		{
			var dateRange1 = new DateTimeRange(new DateTime(1990, 01, 01), new DateTime(2000, 01, 01));
			var dateRange2 = new DateTimeRange(new DateTime(1990, 01, 01), new DateTime(2000, 01, 01));
			var dateRange3 = new DateTimeRange(new DateTime(1990, 01, 02), new DateTime(2000, 01, 01));
			Assert.False(dateRange1 == dateRange2);
			Assert.True(dateRange1.Equals(dateRange2));
			Assert.False(dateRange1.Equals(dateRange3));
		}
	}
}
