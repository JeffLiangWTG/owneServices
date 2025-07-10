using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Common;
using NUnit.Framework;

namespace CargoWise.Data.Testing
{
	public class SqlBlockerObserverTest : TestCase
	{
		[UseSnapshotProtection]
		public void TestGetLockingInfo()
		{
			// Arrange
			const int lockTimeoutMs = 50;
			var interval = TimeSpan.FromMilliseconds(lockTimeoutMs);

			var blockingWaitHandle = $"Blocking_{Guid.NewGuid():N}";
			var stringBuilder = new StringBuilder();

			using var cancellationTokenSource = new CancellationTokenSource();
			using var concurrentExecuteQueryEvent = new ManualResetEvent(false);

			try
			{
				SqlBlockingObserver.Start(interval);

				var tasks = new List<Task>();
				for (var i = 0; i < 3; i++)
				{
					tasks.Add(Task.Run(() =>
					{
						ExecuteQueryBeginTranOnNewConnection();
					}));
				}

				_ = concurrentExecuteQueryEvent.Set();
				Task.WaitAll(tasks.ToArray());

				// Act
				var lockingInfo = SqlBlockingObserver.GetLockingInfo();

				// Assert
				var logs = stringBuilder.ToString();
				var started = Regex.Matches(logs, @"spid\((?<spid>\d+)\) started.");
				var blocking = Regex.Matches(logs, @"spid\((?<spid>\d+)\) completed.");
				var blocked = Regex.Matches(logs, @"spid\((?<spid>\d+)\) SQL Error\(1222\)");
				var startedSpids = started.Cast<Match>().Select(m => m.Groups["spid"].Value).ToArray();
				var blockingSpids = blocking.Cast<Match>().Select(m => m.Groups["spid"].Value).ToArray();
				var blockedSpids = blocked.Cast<Match>().Select(m => m.Groups["spid"].Value).ToArray();

				CombineAssertions($"logs:{Environment.NewLine}{logs}", () =>
				{
					AssertEquals("3 spids started", 3, started.Count);
					AssertEquals("1 blocking spid", 1, blocking.Count);
					AssertEquals("2 spids blocked", 2, blocked.Count);
				});

				CombineAssertions($"lockingInfo:{Environment.NewLine}{lockingInfo}", () =>
				{
					startedSpids.ForEach(spid => Assert(lockingInfo.Contains(spid)));
					blockingSpids.ForEach(spid => Assert(lockingInfo.Contains(spid)));
					blockedSpids.ForEach(spid => Assert(lockingInfo.Contains(spid)));

					var json = JsonArray.Parse(lockingInfo);
					Assert(json is not null);
				});

				var extraInfo = SqlBlockingObserver.GetExtraDebugInformation();
				Assert(!string.IsNullOrEmpty(extraInfo));
			}
			finally
			{
				SqlBlockingObserver.Stop();
			}

			void ExecuteQueryBeginTranOnNewConnection()
			{
				using var connection = Db.NewExtraConnectionToMainDb();
				var spid = connection.SPID;

				connection.DefaultCommandTimeOutInSeconds = 300; // 5 min command time out so that we don't cancel our command too soon
				connection.SetLockTimeout(1000); // 1 sec lock timeout so as to get a lock request time out error 1222 for repro

				try
				{
					_ = concurrentExecuteQueryEvent.WaitOne();
					_ = stringBuilder.AppendLine($"spid({spid}) started.");

					_ = connection.ExecuteNonQuery(@"
BEGIN TRAN
;

WITH DATA AS
(
	SELECT TOP 1 * FROM dbo.StmData ORDER BY SD_PK
)

UPDATE DATA SET SD_PK = NEWID()
;

WAITFOR DELAY '0:0:03'
;
"); //  -- wait for 3 sec so that the command execution is long enough to encounter with a lock request time out error
					_ = stringBuilder.AppendLine($"spid({spid}) completed.");
				}
				catch (SqlException ex)
				{
					_ = stringBuilder.AppendLine($@"spid({spid}) SQL Error({ex.Number}): {ex.Message}.");
				}
			}
		}
	}
}
