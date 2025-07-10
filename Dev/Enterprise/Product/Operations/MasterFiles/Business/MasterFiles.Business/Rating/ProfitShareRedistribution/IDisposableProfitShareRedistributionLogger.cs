using System;

namespace Enterprise.MasterFiles.Business
{
	public interface IDisposableProfitShareRedistributionLogger : IDisposableLogger
	{
		event EventHandler<ProfitShareRedistributedEventArgs> IndividualCompleted;
		void OnIndividualCompleted(ProfitShareRedistributedEventArgs eventArgs);

		event EventHandler<EventArgs> AllCompleted;
		void OnAllCompleted(EventArgs eventArgs);

		string DumpLogs();
	}
}
