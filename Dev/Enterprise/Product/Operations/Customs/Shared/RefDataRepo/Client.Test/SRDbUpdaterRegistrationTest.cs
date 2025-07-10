using System;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Client.Common;
using CargoWise.RefDbRepo.Client.Common.ErrorReporting;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDataRepo.Ent.Client.Test
{
	public class SRDbUpdaterRegistrationTest : TestCase
	{
		public void TestExceptionCaughtAndReturnNullWhenGetUpdaters()
		{
			var logger = new Mock<ILogger>();
			var proxyMock = new Mock<IServerProxy>();
			proxyMock.Setup(x => x.GetAllDataSetScripts(It.IsAny<IHttpClient>(), It.IsAny<int>())).Throws(new TaskCanceledException("A task was canceled."));
			var dbHelperMock = new Mock<IDBHelper>();
			dbHelperMock.Setup(x => x.LoadDbExtendedProperty(It.IsAny<string>(), It.IsAny<System.Data.IDbTransaction>())).Returns(It.IsAny<string>());
			var wrapperMock = new Mock<IErrorReportingClientWrapper>();

			var registration = Array.Empty<ISRDbDataSetUpdater>();
			AssertNoExceptionThrown(() => registration = new SRDbUpdaterRegistration().Get(proxyMock.Object, dbHelperMock.Object, wrapperMock.Object, logger.Object));
			AssertNull(registration);
			wrapperMock.Verify(x => x.PostCrashReport(It.Is<TaskCanceledException>(e => e.Message == "A task was canceled.")));
		}
	}
}
