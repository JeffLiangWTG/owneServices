using System;

namespace Enterprise.Freight.Integration
{
	public interface IServiceRequestManager
	{
		TimeSpan RequestTimeout { get; }
		bool IsSuppressed();
		void OnTimedOutRequest();
		void OnSuccessfulRequest();
		void HandleException(Exception ex, Uri uri = null);
	}
}
