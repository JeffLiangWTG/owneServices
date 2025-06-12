using System;
using System.Threading;
using NUnit.Framework;
using CargoWise.eHub.Nudge;
using Rhino.Mocks;

namespace CargoWise.eHub.Nudge.Tests
{
	[TestFixture]
	public class ReloadSystemInfoManagerTest
	{
		[Test]
		public void TestReloadSystemInfo()
		{
			failCount = 0;
			succCount = 0;
			ReloadSystemInfoManager reloadSystemInfoManager = MockRepository.GeneratePartialMock<ReloadSystemInfoManager>();
			// firstly simulate DB access error
			reloadSystemInfoManager.Stub(m => m.RetrieveUpdatedSystemInfo(DateTime.MinValue)).IgnoreArguments().Do(new RetrieveUpdatedSystemInfoDelegate(FuncOfException)).Repeat.Twice();
			// and then simulate success access to DB
			reloadSystemInfoManager.Stub(m => m.RetrieveUpdatedSystemInfo(DateTime.MinValue)).IgnoreArguments().Do(new RetrieveUpdatedSystemInfoDelegate(FuncOfSuccess)).Repeat.Once();
			// and finally simulate DB access error again
			reloadSystemInfoManager.Stub(m => m.RetrieveUpdatedSystemInfo(DateTime.MinValue)).IgnoreArguments().Do(new RetrieveUpdatedSystemInfoDelegate(FuncOfException)).Repeat.Once();
			Thread thread = new Thread(reloadSystemInfoManager.ReloadSystemInfo);
			thread.Start();
			Thread.Sleep(500);
			// since the simulation of DB access error, the operation should not end by itself because of the DB access error
			Assert.IsTrue(thread.IsAlive);
			Assert.AreEqual(reloadSystemInfoManager.lastLoadUTC, DateTime.MinValue);
			Assert.AreEqual(1, failCount);
			Assert.AreEqual(0, succCount);

			// the operation should have ended by itself during the given timespan
			Assert.IsTrue(thread.Join(30000));
			Assert.IsFalse(thread.IsAlive);
			Assert.AreNotEqual(reloadSystemInfoManager.lastLoadUTC, DateTime.MinValue);
			Assert.AreEqual(2, failCount);
			Assert.AreEqual(1, succCount);

			// start a thread to execute reloadSystemInfoManager.ReloadSystemInfo again
			thread = new Thread(reloadSystemInfoManager.ReloadSystemInfo);
			thread.Start();
			Thread.Sleep(500);
			// the operation should not end by itself because of the DB access error
			Assert.IsTrue(thread.IsAlive);
			Assert.AreEqual(3, failCount);
			Assert.AreEqual(1, succCount);
		}

		delegate int RetrieveUpdatedSystemInfoDelegate(DateTime dateTime);

		int FuncOfException(DateTime dateTime)
		{
			failCount++;
			throw new InvalidOperationException();
		}

		int FuncOfSuccess(DateTime dateTime)
		{
			succCount++;
			return 1;
		}

		int failCount = 0;
		int succCount = 0;
	}
}
