using CargoWise.Data.SqlProxyServer;
using NUnit.Framework;

namespace Enterprise.Services.GlowLoaderService.Test.Server;

class GlobalMutexTest
{
	[TestCase("server1", "database1", TestName = "ValidServerName")]
	[TestCase("", "database1", TestName = "EmptyServerName")]
	[TestCase(null, "database1", TestName = "NullServerName")]
	[TestCase("   ", "database1", TestName = "WhitespaceServerName")]
	[TestCase("server_with_special_chars!@#$%^&*()", "database1", TestName = "SpecialCharactersInServerName")]
	[TestCase("server_with_unicode_测试", "database1", TestName = "UnicodeCharactersInServerName")]
	[TestCase("server1", "", TestName = "EmptyDatabaseName")]
	[TestCase("server1", null, TestName = "NullDatabaseName")]
	[TestCase("server1", "   ", TestName = "WhitespaceDatabaseName")]
	public void TestGlobalMutexWithVariousServerNames(string serverName, string databaseName)
	{
		// Arrange
		var mutex = GetMutex();
		var isNewInstance = mutex.WaitOne();
		Assert.That(isNewInstance, Is.True, "The mutex should be acquired by this instance.");
		Assert.That(mutex.IsCreatedNew, Is.True);

		// Act
		var acquirableByAnotherThread = false;
		AcquireMutexFromAnotherTask();

		// Assert
		Assert.That(acquirableByAnotherThread, Is.False, "Same mutex should not be acquirable by another instance until it's released.");
		mutex.Dispose();

		// Act again
		AcquireMutexFromAnotherTask();

		// Assert
		Assert.That(acquirableByAnotherThread, Is.True, "Same mutex can be acquired by another instance after it's released.");

		return;

		GlobalMutex GetMutex()
		{
			return GlobalMutex.GetSqlProxyServiceMutex(serverName, databaseName);
		}

		void AcquireMutexFromAnotherTask()
		{
			var task = Task.Run(() =>
			{
				try
				{
					using var mutex1 = GetMutex();
					acquirableByAnotherThread = mutex1.WaitOne(TimeSpan.FromMilliseconds(10));
				}
				catch (TimeoutException)
				{
					acquirableByAnotherThread = false;
				}
			});

			task.Wait();
		}
	}
}
