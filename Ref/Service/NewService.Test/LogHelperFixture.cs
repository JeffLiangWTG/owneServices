using CargoWise.RefDbRepo.Common.Utils;
using Common.Logging;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NewService.Test
{
	[TestFixture]
	class LogHelperFixture
	{
		[Test]
		public void LogInfo()
		{
			var user = "UserA";
			var message = "Message A";
			string uri = null;
			logHelper.LogInfo(user, message);
			log.Verify(x => x.Info(It.Is<ApiLogRequest>(a => a.UserId == user && a.Body == message && a.Uri == null)));

			message = "Message B";
			uri = "/DummyDataSet/GetData";
			logHelper.LogInfo(user, message, uri);
			log.Verify(x => x.Info(It.Is<ApiLogRequest>(a => a.UserId == user && a.Body == message && a.Uri == uri)));
		}

		[SetUp]
		public void SetUp()
		{
			log = new Mock<ILog>();
			logWrapper = new Mock<ILogWrapper>();
			logWrapper.Setup(x => x.GetLog<LogHelper>()).Returns(log.Object);
			logHelper = new LogHelper(logWrapper.Object);
		}

		Mock<ILog> log;
		Mock<ILogWrapper> logWrapper;
		LogHelper logHelper;
	}
}
