using System;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using Enterprise.ServiceManager.Module.ScheduleNow;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.ServiceHostClient.Abstractions;
using ServiceManager.Integration.ServiceHostClient.Abstractions.DataContracts;
using WinzorFramework;

namespace Enterprise.Winzor.Architecture.Test;

internal class ScheduleNowControllerTest
{
	[Test]
	public void WinzorDispatcherThreadTest()
	{
		using var winzorDispatcher = new WinzorDispatcher(Mock.Of<IFormOpener>(), Mock.Of<IFormInstanceRegister>());
		var hostName = "hostName1";
		(string taskCode, TaskActionResultDTO outcome)[] requests = { ("task", TaskActionResultDTO.EnqueuedAlready) };
		scheduleNowController = new ScheduleNowController();
		serviceHostClientMock = new Mock<IServiceHostClient>();

		using var flag = new ManualResetEvent(false);
		serviceHostClientMock.Reset();

		var serviceHostsCacheMock = new Mock<IServiceHostsCache>();
		serviceHostsCacheMock
			.SetupGet(cache => cache.ConfiguredServiceHosts)
			.Returns(new[] { serviceHostClientMock.Object });

		serviceHostsCacheMock
			.Setup(cache => cache.CallAllConfiguredHostsUntilFirstSuccess(It.IsAny<Func<IServiceHostClient, TasksActionResultDTO>>()))
			.Returns(new AggregateResult<TasksActionResultDTO>(new[] { new AggregateResultEntry<TasksActionResultDTO>(hostName, new Exception()) }));

		var exceptionOccurred = false;
		using var serviceHostsCacheConfig = ObjectFactory.Substitute(serviceHostsCacheMock.Object);
		var task = winzorDispatcher.InvokeAsync(() =>
		{
			scheduleNowController.ScheduleNow(Enumerable.Empty<string>(), s =>
			{
				try
				{
					var x = WinzorDispatcher.Current;
					flag.Set();
				}
				catch (InvalidOperationException)
				{
					exceptionOccurred = true;
					flag.Set();
				}
			},
			forceRestart: false);
		});

		flag.WaitOne();
		Assert.That(exceptionOccurred, Is.False, "The current thread is not a WinzorDispatcher");
	}

	ScheduleNowController scheduleNowController;
	Mock<IServiceHostClient> serviceHostClientMock;
}
