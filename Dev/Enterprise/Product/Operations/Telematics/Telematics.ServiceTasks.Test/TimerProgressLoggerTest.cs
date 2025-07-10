using System;
using System.Threading;
using NUnit.Framework;

namespace Enterprise.Telematics.ServiceTasks.Test
{
	class TimerProgressLoggerTest : TestCase
	{
		public void TestLogTimeSpan()
		{
			AssertEquals(TimeSpan.FromMinutes(5), timerProgressLogger.LogTimeSpan);
		}

		public void TestShouldLog()
		{
			// Arrange
			timerProgressLogger.LogTimeSpan = TimeSpan.FromSeconds(0.1);
			timerProgressLogger.Initialize();
			Thread.Sleep(TimeSpan.FromSeconds(1));

			// Act
			var result = timerProgressLogger.ShouldLog();

			// Assert
			AssertEquals(true, result);
		}

		public void TestShouldLog_ShouldNotLogRightAfterLog()
		{
			// Arrange
			timerProgressLogger.LogTimeSpan = TimeSpan.FromSeconds(0.1);
			timerProgressLogger.Initialize();
			Thread.Sleep(TimeSpan.FromSeconds(1));

			// Act
			timerProgressLogger.ShouldLog();
			var result = timerProgressLogger.ShouldLog();

			// Assert
			AssertEquals(false, result);
		}

		public void TestShouldNotLog()
		{
			// Arrange
			timerProgressLogger.Initialize();
			Thread.Sleep(TimeSpan.FromSeconds(1));

			// Act
			var result = timerProgressLogger.ShouldLog();

			// Assert
			AssertEquals(false, result);
		}

		protected override void SetUp()
		{
			base.SetUp();
			timerProgressLogger = new TimerProgressLogger();
		}

		TimerProgressLogger timerProgressLogger;
	}
}
