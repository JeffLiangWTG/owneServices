using System;
using System.Threading;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Business
{
	/// <summary>
	/// TriLockMutex will make 3 attempts at locking the mutex before giving up.
	/// Waits 300ms between attempts.
	/// </summary>
	public class TriLockMutex : ZGlobalMutex
	{
		public TriLockMutex(MutexID mutexID) : base(mutexID)
		{
		}

		public TriLockMutex(MutexID mutexID, ZString recordIdentifier) : base(mutexID, recordIdentifier)
		{
		}

		new public bool Lock()
		{
			bool locked = false;
			int attempts = 0;

			while (true)
			{
				locked = base.Lock();
				if (locked || maxAttempts == ++attempts)
				{
					break;
				}

				OnLockFailed?.Invoke(this, new ProgressEventArgs(attempts, maxAttempts));
				Thread.Sleep(millisecondsBetweenTries);
			}

			return locked;
		}

		internal event EventHandler<ProgressEventArgs> OnLockFailed;

		const int maxAttempts = 3;
		const int millisecondsBetweenTries = 300;
	}
}
