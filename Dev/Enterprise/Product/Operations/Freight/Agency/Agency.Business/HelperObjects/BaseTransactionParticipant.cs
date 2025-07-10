using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Utils;
using CargoWise.Integration;

namespace Enterprise.Freight.Agency.Business
{
	public abstract class BaseTransactionParticipant : ITransactionParticipant, IDbConnected
	{
		protected BaseTransactionParticipant(IDbConnected connected)
		{
			if (connected == null)
			{
				throw new ArgumentNullException(nameof(connected));
			}

			Connection = connected.Connection;
		}

		protected DbConnection Connection { get; private set; }

		protected virtual void OnAllTransactionsCommitted(IChangedTableNames changedTableNames)
		{
		}

		protected abstract IChangedTableNames SaveInTransaction();

		#region ITransactionParticipant Members

		ITransactionManager ITransactionStarter.BeginTransactionWithManager()
		{
			return Connection.BeginTransactionWithManager();
		}

		ITransactionParticipant[] ITransactionParticipant.ChildParticipants
		{
			get { return Array.Empty<ITransactionParticipant>(); }
		}

		bool ITransactionParticipant.IsInTransaction
		{
			get { return Connection.IsInTransactionOtherThanTransactionedTestCase; }
		}

		bool ITransactionParticipant.AllowTransactionWithOtherParticipant => false;

		void ITransactionParticipant.OnAllTransactionsBeginning()
		{
		}

		void ITransactionParticipant.OnAllTransactionsRolledBack()
		{
		}

		void ITransactionParticipant.OnAllTransactionsCommitted(IChangedTableNames changedTableNames)
		{
			OnAllTransactionsCommitted(changedTableNames);
		}

		IChangedTableNames ITransactionParticipant.SaveInTransaction()
		{
			return SaveInTransaction();
		}

		IEnumerable<ISqlApplicationLock> ITransactionParticipant.TransactionLocks => Enumerable.Empty<ISqlApplicationLock>();

		#endregion

		#region IDbConnected Members

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		DbConnection IDbConnected.Connection
		{
			[DebuggerStepThrough]
			get { return Connection; }
		}

		#endregion
	}
}
