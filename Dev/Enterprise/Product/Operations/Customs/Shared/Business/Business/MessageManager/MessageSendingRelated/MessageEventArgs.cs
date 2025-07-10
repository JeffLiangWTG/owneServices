using System;

namespace Enterprise.Customs.Business
{
	public class MessageEventArgs : EventArgs
	{
		public MessageEventArgs(string messageText)
		{
			this.MessageText = messageText;
		}

		public string MessageText { get; set; }
	}
}
