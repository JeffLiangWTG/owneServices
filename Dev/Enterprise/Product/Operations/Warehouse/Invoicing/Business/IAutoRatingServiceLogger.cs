using System;
using CargoWise.Types;
using Enterprise.Integration;

namespace Enterprise.Warehouse.Invoicing.Business
{
	public interface IAutoRatingServiceLogger : ILogger
	{
		IDisposable EnableAddingNoteWhileLogging(ZGuid invoicePK);
		void Information(string message);
		void Error(string message);
	}
}
