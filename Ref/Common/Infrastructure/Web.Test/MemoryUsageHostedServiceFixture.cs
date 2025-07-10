using System.Threading;
using CargoWise.RefDbRepo.Common.Utils;
using Common.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Common.Web.Test
{
	[TestFixture]
	public class MemoryUsageHostedServiceFixture
	{
		Mock<ILogWrapper> logWrapperMock;
		Mock<ILog> logMock;
		MemoryUsageHostedService service;
		Mock<IOptions<MemoryUsageHostedServiceOptions>> optionsMock;


		[SetUp]
		public void SetUp()
		{
			logWrapperMock = new Mock<ILogWrapper>();
			logMock = new Mock<ILog>();
			logWrapperMock.Setup(lw => lw.GetLog<MemoryUsageHostedService>()).Returns(logMock.Object);
			optionsMock = new Mock<IOptions<MemoryUsageHostedServiceOptions>>();
			optionsMock.Setup(s => s.Value).Returns(new MemoryUsageHostedServiceOptions
			{
				IntervalSeconds = 10
			});
			service = new MemoryUsageHostedService(logWrapperMock.Object, optionsMock.Object);
		}

		[TearDown]
		public void TearDown()
		{
			service.Dispose();
		}

		[Test]
		public void LogMemoryUsage_ShouldLogMemoryUsage()
		{
			var method = service.GetType().GetMethod("LogMemoryUsage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			method.Invoke(service, new object[] { null });

			logMock.Verify(log => log.Info(It.IsAny<WebServiceMemoryUsageLog>()), Times.Once);
		}

		[Test]
		public void Timer_ShouldTriggerLogMemoryUsage()
		{
			service.StartAsync(CancellationToken.None).Wait();
			Thread.Sleep(1000);
			logMock.Verify(log => log.Info(It.IsAny<WebServiceMemoryUsageLog>()), Times.AtLeastOnce);
			service.StopAsync(CancellationToken.None).Wait();
		}
	}
}
