namespace CargoWise.Billing.CollectorService.Plugin.Tests
{
	[TestFixture]
	class ClockTest
	{
		[Test]
		public void TestNotify()
		{
			using (var clock = new Clock(TimeSpan.Zero))
			{
				var notifyRaised = false;
				clock.Notify += delegate { notifyRaised = true; };
				clock.SetNotificationDueTime(TimeSpan.FromMilliseconds(500));
				Assert.That(!notifyRaised);
				Thread.Sleep(TimeSpan.FromMilliseconds(550));
				Assert.That(notifyRaised);
			}
		}

		[Test]
		public void TestDelay()
		{
			const int delaySeconds = 35;
			using (var clock = new Clock(TimeSpan.FromSeconds(delaySeconds)))
			{
				var expectedUtcNow = DateTime.UtcNow.AddSeconds(-delaySeconds);
				Assert.That(clock.UtcNow, Is.InRange(expectedUtcNow.AddMilliseconds(-10), expectedUtcNow.AddMilliseconds(10)));
			}
		}
	}
}
