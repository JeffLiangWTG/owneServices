using System;

namespace Enterprise.Warehouse.Invoicing.Business
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
