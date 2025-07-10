using System;
using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class PrintFailedEventArgs : EventArgs
	{
		public PrintFailedEventArgs(ZString message)
		{
			Message = message;
		}

		public readonly ZString Message;
	}
}