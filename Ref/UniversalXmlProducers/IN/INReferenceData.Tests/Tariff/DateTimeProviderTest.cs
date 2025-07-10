using System;
using CargoWise.RefDbRepo.INReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.INReferenceData.Tests
{
	sealed class DateTimeProviderTest
	{
		[Test]
		public void TestGetDateTimeNow()
		{
			var dateTimeProvider = new DateTimeProvider();
			var actualTime = dateTimeProvider.GetDateTimeNow();
			var expectedTime = DateTime.Now;
			Assert.That(actualTime, Is.EqualTo(expectedTime).Within(TimeSpan.FromSeconds(1)));
		}

		[Test]
		public void TestGetIndiaToday()
		{
			var dateTimeProvider = new DateTimeProvider();
			var actualTime = dateTimeProvider.GetIndiaToday();
			var expectedTime = GetIndiaTime().Date;
			Assert.That(actualTime, Is.EqualTo(expectedTime).Within(TimeSpan.FromSeconds(1)));
		}

		[Test]
		public void TestGetIndiaTime()
		{
			var dateTimeProvider = new DateTimeProvider();
			var actualTime = dateTimeProvider.GetIndiaTime();
			var expectedTime = GetIndiaTime();
			Assert.That(actualTime, Is.EqualTo(expectedTime).Within(TimeSpan.FromSeconds(1)));
		}

		DateTime GetIndiaTime()
		{
			return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
		}
	}
}
