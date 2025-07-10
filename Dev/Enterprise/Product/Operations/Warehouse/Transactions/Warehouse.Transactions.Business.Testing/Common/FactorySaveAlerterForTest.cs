using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public sealed class FactorySaveAlerterForTest : Disposable, ITransactionParticipantListener
	{
		public FactorySaveAlerterForTest()
		{
			BusinessObjectFactory.RegisterListener(this);
		}

		void ITransactionParticipantListener.FactorySaveBeginning(ITransactionParticipant[] factories)
		{
			throw new InvalidOperationException("Don't call factory save.");
		}

		void ITransactionParticipantListener.FactorySaveCompleted(ITransactionParticipant[] factories, bool successful)
		{
		}

		protected override void Dispose(bool isDisposing) => BusinessObjectFactory.UnRegisterListener(this);
	}
}
