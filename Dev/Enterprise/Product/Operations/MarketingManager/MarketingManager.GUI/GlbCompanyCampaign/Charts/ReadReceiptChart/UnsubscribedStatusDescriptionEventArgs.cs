using System;

namespace Enterprise.MarketingManager.GUI
{
	public class UnsubscribedStatusDescriptionEventArgs : EventArgs
	{
		public UnsubscribedStatusDescriptionEventArgs(bool unsubscribed)
		{
			Unsubscribed = unsubscribed;
		}

		public bool Unsubscribed { get; }
	}
}
