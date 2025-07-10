using System.Threading;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TR.NCTS.Business.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.TR.NCTS.Business.Testing
{
	public class NctsResponseDownloaderTest : TestCaseWithFactory
	{
		public void TestNoExceptionThrownForFunctionalityNotImplemented()
		{
			AssertNoExceptionThrown(() =>
			{
				new NctsResponseDownloader().ExecuteDownload(new Logger(), GlbBranch.CurrentBranch, CancellationToken.None);
			});
		}
	}
}
