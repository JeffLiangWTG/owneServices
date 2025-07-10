using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Common.Utils.Test;

[TestFixture]
class FileLockerFixture
{
	private string lockFilePath;

	[SetUp]
	public void Setup()
	{
		lockFilePath = Path.Combine(Path.GetTempPath(), $"file_locker_test_{Guid.NewGuid()}.lock");
	}

	[TearDown]
	public void Teardown()
	{
		if (File.Exists(lockFilePath))
		{
			File.Delete(lockFilePath);
		}
	}

	[Test]
	public void RunWithLock_ShouldBlockConcurrentAccess()
	{
		var longRunningTask = Task.Run(() =>
			FileLocker.RunWithLockAsync(lockFilePath, async () => await Task.Delay(3 * 1000)));

		Thread.Sleep(100);

		bool secondThreadSuccess = false;
		var shortTask = Task.Run(() =>
			FileLocker.RunWithLockAsync(lockFilePath, async () =>
			{
				await Task.Delay(100);
				secondThreadSuccess = true;
			}));

		Task.WaitAll([longRunningTask, shortTask], 3000);

		Assert.That(secondThreadSuccess, Is.False);
	}
}
