using System.Data;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Data.SqlProxy.Interface.Models;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.Data.SqlProxyServer.Core;

[SuppressMessage("CargoWiseOne", "CW1061:DoNotUseDateTimeUtcNow", Justification = "SQL Over Http")]
[SuppressMessage("CargoWiseOne", "CW1116:UseEnterpriseCoreDataWrapperClasses", Justification = "connection over HTTP")]
class SqlConnectionSlot
{
	public SqlConnectionSlot(SqlProxyDatabaseDetails connection, IsolationLevel isolationLevel, SqlTransaction? transaction = null)
	{
		Connection = connection;
		TransactionId = Guid.NewGuid();
		IsolationLevel = isolationLevel;
		Transaction = transaction;
		ReservationTime = DateTime.UtcNow;
		LastUsed = ReservationTime;
	}

	public void IncreaseUsageCount()
	{
		usage++;
	}

	public void DecreaseUsageCount()
	{
		usage--;
	}

	[SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Auto generated baseline suppressions - WI00637629")]
	int usage;

	public SqlProxyDatabaseDetails Connection { get; }
	public IsolationLevel IsolationLevel { get; }
	public DateTime LastUsed { get; set; }
	public DateTime ReservationTime { get; }
	public SqlTransaction? Transaction { get; private set; }
	public Guid TransactionId { get; private set; }

	public void Rollback()
	{
		if (Transaction == null)
		{
			return;
		}

		Interlocked.Decrement(ref transactionCount);
		ResetConnectionToReadCommitted(Transaction);
		var connection = Transaction.Connection;
		Transaction.Dispose();
		connection.Dispose();
	}

	static void ResetConnectionToReadCommitted(SqlTransaction transaction)
	{
		if (transaction.IsolationLevel == IsolationLevel.ReadCommitted)
		{
			return;
		}

		using var cmd = transaction.Connection.CreateCommand();
		cmd.Transaction = transaction;
		cmd.CommandText = "SET TRANSACTION ISOLATION LEVEL READ COMMITTED";
		cmd.ExecuteNonQuery();
	}

	public void Commit()
	{
		if (Transaction == null)
		{
			return;
		}

		Interlocked.Decrement(ref transactionCount);
		ResetConnectionToReadCommitted(Transaction);
		var connection = Transaction.Connection;
		Transaction.Commit();
		connection.Dispose();
	}

	[ThreadSafe]
	static int transactionCount;

	public void EstablishTransaction()
	{
		LastUsed = DateTime.UtcNow;
		if (Transaction != null)
		{
			return;
		}

		var sqlConnection = SqlConnectionProvider.GetNewOpenConnection(Connection);
		Transaction = sqlConnection.BeginTransaction(IsolationLevel);
		Interlocked.Increment(ref transactionCount);
	}

	public TimeSpan IdleTime => DateTime.UtcNow - LastUsed;
}
