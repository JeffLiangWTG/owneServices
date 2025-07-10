using System.Security.AccessControl;
using System.Security.Principal;
using CargoWise.Data.SqlProxy.Interface;
using static System.FormattableString;

namespace CargoWise.Data.SqlProxyServer;

public class GlobalMutex : IDisposable
{
	public GlobalMutex(string mutexName)
	{
#if NET48
		mutex = new Mutex(false, mutexName, out isCreatedNew, AllowEveryoneFullControlExceptChangePermissionAndDeletion);
#else
		mutex = MutexAcl.Create(initiallyOwned: false, mutexName, out isCreatedNew, AllowEveryoneFullControlExceptChangePermissionAndDeletion);
#endif

		this.mutexName = mutexName;
	}

	public bool IsCreatedNew => isCreatedNew;

	protected virtual TimeSpan TimeOutSpan => new(0, 0, 3);

	public async Task AcquireMutexAsync(CancellationToken cancellationToken)
	{
		using var scheduler = new SingleThreadTaskScheduler();
		var tcs = new TaskCompletionSource<bool>();

		await Task.Factory.StartNew(async () =>
			{
				try
				{
					_ = WaitOne();

					using (cancellationToken.Register(() => tcs.SetResult(true)))
					{
						await tcs.Task.ConfigureAwait(false);
					}
				}
				finally
				{
					await Task.Factory.StartNew(
						Dispose,
						CancellationToken.None,
						TaskCreationOptions.None,
						scheduler
					).ConfigureAwait(false);
				}
			}, CancellationToken.None, TaskCreationOptions.None, scheduler)
			.Unwrap();
	}

	public bool WaitOne(TimeSpan timeout)
	{
		return mutexObtained = WaitPossibleAbandonedMutex(() => mutex.WaitOne(timeout));
	}

	public bool WaitOne()
	{
		mutexObtained = WaitPossibleAbandonedMutex(() => mutex.WaitOne(TimeOutSpan));

		return !mutexObtained
			? throw new TimeoutException(Invariant($"Could not obtain mutex {mutexName} after {TimeOutSpan}."))
			: true;
	}

	public bool WaitOne(int timeout)
	{
		return mutexObtained = WaitPossibleAbandonedMutex(() => mutex.WaitOne(timeout));
	}

	public void Dispose()
	{
		if (!disposed)
		{
			disposed = true;

			if (mutexObtained)
			{
				mutex.ReleaseMutex();
			}

			mutex.Dispose();
		}
	}

	static bool WaitPossibleAbandonedMutex(Func<bool> waitOneFunc)
	{
		bool obtained;

		try
		{
			obtained = waitOneFunc();
		}
		catch (AbandonedMutexException)
		{
			obtained = true;
		}

		return obtained;
	}

	static MutexSecurity AllowEveryoneFullControlExceptChangePermissionAndDeletion
	{
		get
		{
			var sid = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
			var mutexSecurity = new MutexSecurity();

			mutexSecurity.AddAccessRule(new MutexAccessRule(sid, MutexRights.FullControl, AccessControlType.Allow));
			mutexSecurity.AddAccessRule(new MutexAccessRule(sid, MutexRights.ChangePermissions | MutexRights.Delete, AccessControlType.Deny));

			return mutexSecurity;
		}
	}

	public static GlobalMutex GetSqlProxyServiceMutex(string serverName, string databaseName)
	{
		return new GlobalMutex(SqlProxyNamingConvention.GlobalSqlProxyServiceMutexName(serverName, databaseName));
	}

	readonly string mutexName;
	readonly Mutex mutex;
	readonly bool isCreatedNew;
	bool mutexObtained;
	bool disposed;
}
