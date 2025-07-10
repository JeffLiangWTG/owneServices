using System;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class TextEventArgs : EventArgs
	{
		public string Message { get; }

		public TextEventArgs(string message)
		{
			Message = message;
		}
	}
}
