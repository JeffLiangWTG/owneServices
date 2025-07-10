using System;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.SharedReferenceData.Business.Common.Tests
{
	[TestFixture]
	class DateTimeProviderTests
	{
		[Test]
		public void HistoricalDate()
		{
			var expected = DateTime.UtcNow.Date.AddMonths(-5);
			var dtProvider = new Services.Common.DateTimeProvider(5);

			Assert.That(dtProvider.UTCHistoricalDate, Is.EqualTo(expected));
		}
	}
}
