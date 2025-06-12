using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Threading;
using Moq;
using NUnit.Framework;

namespace CargoWise.eServices.Authentication.WindowsService.Tests
{
	[TestFixture]
	public class AuthenticationServiceTaskTests
	{
		[Test]
		public void TestRunServiceTask()
		{
			var settings = new AuthenticationSettings
			{
				INTERVAL = 2
			};
			var expectedInterval = TimeSpan.FromSeconds(settings.INTERVAL);
			var task = new DummyServiceTask(settings);
			task.Start();
			while (task.times < DummyServiceTask.DummyTaskRuns + 1)
			{
				Thread.Sleep(TimeSpan.FromMilliseconds(100));
			}
			task.Dispose();
			for (int i = 0; i < DummyServiceTask.DummyTaskRuns; i++)
			{
				var interval = task.dts[i + 1] - task.dts[i];

				Assert.That(interval, Is.EqualTo(expectedInterval).Within(250).Milliseconds, "Next run time is within 250 milliseconds of expected interval");
			}

			Assert.That(task.dts[DummyServiceTask.DummyTaskRuns] - task.dts[0], Is.EqualTo(TimeSpan.FromSeconds(settings.INTERVAL * DummyServiceTask.DummyTaskRuns)).Within(250).Milliseconds, "Next run time stays within 250 milliseconds of expected time over multiple intervals");
		}

		[Test]
		public void TestCopyFromediProdToAuthanticationDB()
		{
			var settings = new AuthenticationSettings();
			settings.EDIPROD_DB_CONNECTION_STRING = "EDIPROD_DB_CONNECTION_STRING";
			settings.AUTH_DB_CONNECTION_STRING = "AUTH_DB_CONNECTION_STRING";

			var connection = new SqlConnection();
			var databaseHelper = new Mock<IDatabaseHelper>();
			databaseHelper.Setup(_ => _.GetDbConnection("EDIPROD_DB_CONNECTION_STRING")).Returns(connection);
			databaseHelper.Setup(_ => _.GetDbConnection("AUTH_DB_CONNECTION_STRING")).Returns(connection);

			var task = new Mock<AuthenticationServiceTask>(settings) { CallBase = true };
			task.Setup(_ => _.DatabaseHelper).Returns(databaseHelper.Object);
			task.Object.Start();
			task.Verify(_ => _.DatabaseHelper, Times.AtLeastOnce);
		}
	}

	class DummyServiceTask : AuthenticationServiceTask
	{
		public const int DummyTaskRuns = 3;
		Stopwatch stopwatch;

		public DummyServiceTask(AuthenticationSettings settings)
			: base(settings)
		{
			stopwatch = Stopwatch.StartNew();
		}

		public override void Do()
		{
			if (times < dts.Length)
			{
				dts[times++] = stopwatch.Elapsed;
			}
		}

		public int times = 0;
		public TimeSpan[] dts = new TimeSpan[DummyTaskRuns + 1];
	}
}
