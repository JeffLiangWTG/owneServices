using System;

namespace Enterprise.MarketingManager.GUI
{
	public class TrackingStatusDescriptionEventArgs : EventArgs
	{
		public TrackingStatusDescriptionEventArgs(string description)
		{
			Description = description;
		}

		public string Description { get; }
	}
}
