using System;
using System.Globalization;
using CargoWise.RefDbRepo.Staging.ProcessorRunner;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.Schedulers.Common.Test
{
	[TestFixture]
	class QuartzLogProviderFixture
	{
		[Test]
		public void WriteLog()
		{
			var logHelper = new Mock<ILogHelper>();
			var logProvider = new QuartzLogProvider(logHelper.Object);
			Func<string> func = GetMessage;
			Exception ex = null;
			object[] paramsObj = null;

			var level = Quartz.Logging.LogLevel.Trace;
			logProvider.WriteLog(level, func, null, null);
			logHelper.Verify(x => x.LogInfoFormat(CultureInfo.InvariantCulture, It.IsAny<string>(), ex, paramsObj), Times.Never);
			level = Quartz.Logging.LogLevel.Debug;
			logProvider.WriteLog(level, func, null, null);
			logHelper.Verify(x => x.LogInfoFormat(CultureInfo.InvariantCulture, It.IsAny<string>(), ex, paramsObj), Times.Never);

			level = Quartz.Logging.LogLevel.Info;
			logProvider.WriteLog(level, func, null, null);
			logHelper.Verify(x => x.LogInfoFormat(CultureInfo.InvariantCulture, It.IsAny<string>(), ex, paramsObj));
			level = Quartz.Logging.LogLevel.Error;
			logProvider.WriteLog(level, func, null, null);
			logHelper.Verify(x => x.LogErrorFormat(CultureInfo.InvariantCulture, It.IsAny<string>(), ex, paramsObj));
			level = Quartz.Logging.LogLevel.Fatal;
			logProvider.WriteLog(level, func, null, null);
			logHelper.Verify(x => x.LogFatalFormat(CultureInfo.InvariantCulture, It.IsAny<string>(), ex, paramsObj));
		}

		string GetMessage()
		{
			return $"This is a message.";
		}
	}
}
