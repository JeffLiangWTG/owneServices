using System;
using System.Globalization;
using System.IO;
using CargoWise.RefDbRepo.INReferenceData.Services;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.INReferenceData.Tests
{
	sealed class LoggerTest
	{
		[TestCase(LogType.ReviewRequired, "Test value", true, TestName = "Log_WhenLogTypeIsReviewRequired_WritesToConsoleError")]
		[TestCase(LogType.Info, "Test value", false, TestName = "Log_WhenLogTypeIsNotReviewRequired_WritesToConsole")]
		[TestCase(LogType.Info, null, false, TestName = "Log_WhenValueIsNull")]
		public void TestLog(LogType logType, string value, bool isError)
		{
			var logMessage = "Test message";
			var timeNow = DateTime.Now;

			Assert.Multiple(() =>
			{
				DateTimeProviderMock.Setup(x => x.GetIndiaTime()).Returns(timeNow);

				using (var output = new StringWriter())
				{
					if (isError)
					{
						Console.SetError(output);
					}
					else
					{
						Console.SetOut(output);
					}

					Logger.Log(logType, logMessage, value);

					var expectedMessage = string.Format(CultureInfo.InvariantCulture, "{0} [{1}]: {2}: {3}", timeNow, logType, logMessage, value);
					Assert.IsTrue(output.ToString().Contains(expectedMessage));
				}
			});
		}

		Mock<IDateTimeProvider> DateTimeProviderMock => dateTimeProviderMock ?? (dateTimeProviderMock = new Mock<IDateTimeProvider>());
		Mock<IDateTimeProvider> dateTimeProviderMock;

		Logger Logger => logger ?? (logger = new Logger(DateTimeProviderMock.Object));
		Logger logger;
	}
}
