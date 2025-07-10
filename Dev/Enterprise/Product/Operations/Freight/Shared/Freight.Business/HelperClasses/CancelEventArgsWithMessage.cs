using System.ComponentModel;

namespace Enterprise.Freight.Business
{
	public class CancelEventArgsWithMessage : CancelEventArgs
	{
		public CancelEventArgsWithMessage(string caption, string message)
		{
			Caption = caption;
			Message = message;
		}

		public string Caption { get; private set; }
		public string Message { get; private set; }
	}
}
