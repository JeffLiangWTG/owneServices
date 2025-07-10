using System;
using CargoWise.Types;
using Enterprise.Integration;

namespace Enterprise.Warehouse.Transactions.Invoicing
{
	public interface IAutoRatingServiceLogger : ILogger
	{
		IDisposable EnableAddingNoteWhileLogging(ZGuid invoicePK);
		void Information(string message);
		void Error(string message);
	}
}
