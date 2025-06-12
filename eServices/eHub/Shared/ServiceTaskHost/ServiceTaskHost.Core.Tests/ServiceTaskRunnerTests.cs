using System.Configuration;
using System.Threading;
using CargoWise.eHub.Shared.ServiceTaskHost.Integration;
using Common.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Shared.ServiceTaskHost.Core.Tests
{
	[TestClass]
	public class ServiceTaskRunnerTests
	{
		[TestMethod]
		public void ServiceTaskRunner_OnStartOnStop()
		{
			var logger = new LoggerMock();
			var configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

			var task = MockRepository.GenerateStrictMock<IServiceTask>();
			task.Expect(c => c.Run(Arg<ILog>.Is.Anything, Arg<Configuration>.Is.Anything, Arg<CancellationToken>.Is.Anything)).Repeat.Twice();

			using (var runner = new ServiceTaskRunnerMock(task, logger, configuration))
			{
				runner.Start();
				Thread.Sleep(3*1000);
				runner.Stop();
			}

			task.VerifyAllExpectations();
		}

		[TestMethod]
		[Ignore]
		public void ServiceTaskRunner_Cancel()
		{
			var logger = new LoggerMock();
			var configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

			var task = new ServiceTaskMock();

			using (var runner = new ServiceTaskRunnerMock(task, logger, configuration))
			{
				runner.Start();
				Assert.IsFalse(task.ServiceCancellationToken.IsCancellationRequested);
				runner.Stop();
				Assert.IsTrue(task.ServiceCancellationToken.IsCancellationRequested);
			}
		}

		[TestMethod]
		[Ignore]
		public void ServiceTaskRunner_Cancel_ServiceTaskReturnTrue()
		{
			var logger = new LoggerMock();
			var configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

			var task = new ServiceTaskMockReturnTrue();

			using (var runner = new ServiceTaskRunnerMock(task, logger, configuration))
			{
				runner.Start();
				Assert.IsFalse(task.ServiceCancellationToken.IsCancellationRequested);
				runner.Stop();
				Assert.IsTrue(task.ServiceCancellationToken.IsCancellationRequested);
			}
		}

		class ServiceTaskRunnerMock : ServiceTaskRunner
		{
			public ServiceTaskRunnerMock(IServiceTask task, ILog logger, Configuration configuration)
				: base(task, logger, configuration, "Service Task 1")
			{
			}

			protected override int RunIntervalInSecondsCore
			{
				get { return 2; }
			}
		}

		class ServiceTaskMock : IServiceTask
		{
			public ILog ServiceLogger { get; set; }
			public Configuration ServiceConfiguration { get; set; }
			readonly object locker = 1;

			public CancellationToken ServiceCancellationToken
			{
				get
				{
					lock (locker)
					{
						return serviceCancellationToken;
					}
				}
			}

			CancellationToken serviceCancellationToken;
			
				

			public bool Run(ILog logger, Configuration configuration, CancellationToken cancellationToken)
			{
				ServiceLogger = logger;
				ServiceConfiguration = configuration;
				lock (locker)
				{
					this.serviceCancellationToken = cancellationToken;
				}

				return false;
			}
		}

		class ServiceTaskMockReturnTrue : IServiceTask
		{
			public ILog ServiceLogger { get; set; }
			public Configuration ServiceConfiguration { get; set; }
			public CancellationToken ServiceCancellationToken { get; set; }


			public bool Run(ILog logger, Configuration configuration, CancellationToken cancellationToken)
			{
				ServiceLogger = logger;
				ServiceConfiguration = configuration;
				ServiceCancellationToken = cancellationToken;

				return true;
			}
		}
	}
}