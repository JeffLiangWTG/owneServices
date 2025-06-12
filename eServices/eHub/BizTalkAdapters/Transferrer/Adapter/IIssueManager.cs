using System;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;

namespace CargoWise.eHub.BizTalkAdapters.Transferrer.Adapter
{
	public interface IIssueManager
	{
		Task ReportToIssueManagerAsync(string activityId, string key, Exception exception, ILog logger, CancellationToken cancellationToken);
	}
}
