using System.Data;
using CargoWise.Data.SqlProxy.Interface.Models;

namespace CargoWise.Data.SqlProxyServer.Core;

class SqlTransactionManager
{
	readonly TimeSpan maximumConnectionIdleTime = TimeSpan.FromMinutes(5);
	readonly TimeSpan sweepTime = TimeSpan.FromSeconds(3);

	void StartIdleConnectionKiller()
	{
		lock (threadScavengerLock)
		{
			if (idleConnectionThreadScavenger is not { IsAlive: true })
			{
				var cancelConnectionKillerEvent = new AutoResetEvent(false);
				idleConnectionThreadScavenger = new Thread(new ThreadStart(delegate
				{
					while (!cancelConnectionKillerEvent.WaitOne(sweepTime))
					{
						lock (slotLock)
						{
							var slotsToRemove =
								from slot in slots.Values
								where slot.IdleTime > maximumConnectionIdleTime
								select slot;

							foreach (var slot in slotsToRemove.ToArray())
							{
								ReleaseSlot(slot.TransactionId);
								slot.Rollback();
							}
						}
					}
				})) { IsBackground = true, Name = nameof(SqlTransactionManager) };

				idleConnectionThreadScavenger.Start();
			}
		}
	}

	readonly object threadScavengerLock = new();
	Thread? idleConnectionThreadScavenger;

	readonly object slotLock = new();
	readonly Dictionary<Guid, SqlConnectionSlot> slots = new(100);

	internal Guid CreateSlot(SqlProxyDatabaseDetails connection, IsolationLevel isolationLevel)
	{
		StartIdleConnectionKiller();
		var slot = new SqlConnectionSlot(connection, isolationLevel);
		lock (slotLock)
		{
			slots[slot.TransactionId] = slot;
		}

		return slot.TransactionId;
	}

	internal SqlConnectionSlot? ReleaseSlot(Guid transactionId)
	{
		SqlConnectionSlot? slot;
		lock (slotLock)
		{
			slots.TryGetValue(transactionId, out slot);
			slots.Remove(transactionId);
		}

		return slot;
	}

	internal DisposableConnection GetConnection(Guid? transactionId)
	{
		ArgumentNullException.ThrowIfNull(transactionId);

		lock (slotLock)
		{
			if (slots.TryGetValue(transactionId!.Value, out var slot))
			{
				slot.EstablishTransaction();
				slot.IncreaseUsageCount();

				if (slot.Transaction?.Connection != null)
				{
					var result = new DisposableConnection(slot.Transaction!.Connection, slot.Transaction);
					result.Disposing += delegate { slot.DecreaseUsageCount(); };
					return result;
				}
			}
		}

		throw new InvalidOperationException($"Requested TransactionID {transactionId} has been removed - middle tier may have been rebooted");
	}
}
