using System;
using NUnit.Framework;
using CargoWise.eHub.Nudge;

namespace CargoWise.eHub.Nudge.Tests
{
	[TestFixture]
	public class RepeatableTaskTest
	{
		[Test]
		public void TestRunRepeatableTask()
		{
			task = new RepeatableTask(1001, delegate { DoTask(); });
			task.Start();
			while (times < 6) ;
			task.Dispose();
			for (int i = 0; i < 5; i++)
			{
				Assert.IsTrue(dts[i + 1] >= dts[i] + TimeSpan.FromSeconds(1));
			}
		}

		void DoTask()
		{
			if (times < 6)
			{
				dts[times++] = DateTime.UtcNow;
			}
		}

		RepeatableTask task;
		int times = 0;
		DateTime[] dts = new DateTime[6];
	}
}
