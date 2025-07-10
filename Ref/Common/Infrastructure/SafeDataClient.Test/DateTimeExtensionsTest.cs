using System;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Common.SafeDataClient.Test
{
	[TestFixture]
	class DateTimeExtensionsTest
	{
		[Test]
		public void DateTime_ToUTCDateTimeOffset()
		{
			var d = new DateTime(1, 1, 1, 10, 10, 10);
			var d2 = d.ToUTCDateTimeOffset();
			Assert.That(d2, Is.EqualTo(new DateTimeOffset(d, TimeSpan.Zero)));
		}

		[Test]
		public void DateTimeOffset_ToUTCDateTimeOffset()
		{
			var d = new DateTime(1, 1, 1, 10, 10, 10);
			var offset = new DateTimeOffset(d, new TimeSpan(10, 0, 0));
			var d2 = offset.ToUTCDateTimeOffset();
			Assert.That(d2, Is.EqualTo(new DateTimeOffset(d, TimeSpan.Zero)));
		}

		[Test]
		public void MidnightToEndOfDay()
		{
			var d = new DateTime(2024, 5, 22, 0, 0, 0);
			var d2 = new DateTime(2024, 5, 22, 14, 8, 35);
			Assert.That(d.MidnightToEndOfDay(), Is.EqualTo(new DateTime(2024, 5, 22, 23, 59, 00)));
			Assert.That(d2.MidnightToEndOfDay(), Is.EqualTo(d2));
		}
	}
}
