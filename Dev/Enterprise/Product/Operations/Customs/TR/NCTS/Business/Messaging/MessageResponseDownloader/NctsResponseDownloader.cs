using System.Threading;
using Enterprise.Customs.EU.NCTS.ServiceTasks;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TR.NCTS.Business.Messaging
{
	public class NctsResponseDownloader : INctsResponseDownloader
	{
		public void ExecuteDownload(ILogger serviceLogger, GlbBranch branch, CancellationToken token)
		{
		}
	}
}
