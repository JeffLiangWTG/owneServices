using System;
using CargoWise.Blazor.Common;
using CargoWise.DataProtection;
using CargoWise.Winzor.Telemetry;
using Enterprise.Startup;
using Enterprise.Winzor.Architecture;
using Enterprise.ZArchitecture.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using WinzorFramework;

namespace CargoWise.Blazor.HostedAppRuntime.Test
{
	public class CargoWiseRuntimeTests
	{
		CargoWiseOptions options;

		[SetUp]
		public void SetUp()
		{
			options = new CargoWiseOptions();
			ConfigurationBinder.Bind(new TestConfiguration(), "CargoWiseOptions", options);
		}

		[TearDown]
		public void TearDown()
		{
			Initialization.UnconfigureCargoWise();
		}

		[Test]
		public void Initialise_AlreadyInitialised_DoesNotThrow()
		{
			using var winzorDispatcher = new WinzorDispatcher(Mock.Of<IFormOpener>(), Mock.Of<IFormInstanceRegister>());
			var logger = new Mock<ILogger<CargoWiseRuntime>>();

			var runtime = CreateRuntime(logger.Object, options, winzorDispatcher);

			// setup an 'already initialised' state
			Assert.DoesNotThrow(() => runtime.Initialise());

			//subsequent calls to initialise should not throw
			Assert.DoesNotThrow(
				code: () =>
				{
					runtime.Initialise();
					runtime.Initialise();
				},
				message: "should not throw on first or subsequent invocations for the same process");

			logger.Verify(l => l.Log(LogLevel.Information, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString() == "CargoWise runtime already initialised"), It.IsAny<Exception>(), (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Exactly(2));
		}

		[Test]
		public void Initialise_Fails_Throws()
		{
			using var winzorDispatcher = new WinzorDispatcher(Mock.Of<IFormOpener>(), Mock.Of<IFormInstanceRegister>());
			var runtime = CreateRuntime(NullLogger<CargoWiseRuntime>.Instance, null, winzorDispatcher);

			Assert.Throws<NullReferenceException>(() => runtime.Initialise(), message: "should pass through the underlying exception");
		}

		[Test]
		public void Initialise_CalledWhenAlreadyFailed_ThrowsPriorFailureException()
		{
			using var winzorDispatcher = new WinzorDispatcher(Mock.Of<IFormOpener>(), Mock.Of<IFormInstanceRegister>());

			var runtime = CreateRuntime(NullLogger<CargoWiseRuntime>.Instance, null, winzorDispatcher);

			// simulate the initial failure here
			Assert.Throws<NullReferenceException>(() => runtime.Initialise());

			// subsequent calls should short circuit and not re-initialise the runtime
			var subsequentInitException = Assert.Throws<InvalidOperationException>(() => runtime.Initialise(), message: "should throw an exception if a previous call failed");
			Assert.That(subsequentInitException.Message, Does.Contain("CargoWise runtime status 'initialisation failed' due to a previous Initialise call."));
			Assert.That(subsequentInitException.InnerException, Is.TypeOf<NullReferenceException>(), message: "subsequent exceptions should provide the initial exception as InnerException");
		}

		[Test]
		public void Initialise_SetsUsedToLaunchApplicationInCommandLineArguments()
		{
			using var winzorDispatcher = new WinzorDispatcher(Mock.Of<IFormOpener>(), Mock.Of<IFormInstanceRegister>());
			var runtime = CreateRuntime(NullLogger<CargoWiseRuntime>.Instance, options, winzorDispatcher);
			runtime.Initialise();
			Assert.That(CommandLineArguments.UsedToLaunchApplication, Is.Not.Null);
		}

		[Test]
		public void Initialise_SetsPersistInApplicationArguments()
		{
			using var winzorDispatcher = new WinzorDispatcher(Mock.Of<IFormOpener>(), Mock.Of<IFormInstanceRegister>());
			options.Persist = "TestPersistForm";
			var runtime = CreateRuntime(NullLogger<CargoWiseRuntime>.Instance, options, winzorDispatcher);
			runtime.Initialise();
			Assert.That(CommandLineArguments.UsedToLaunchApplication[ApplicationArguments.OptionPersist], Is.EqualTo(options.Persist));
		}

		static CargoWiseRuntime CreateRuntime(ILogger<CargoWiseRuntime> logger, CargoWiseOptions cargoWiseOptions, WinzorDispatcher winzorDispatcher)
		{
			var collection = new ServiceCollection();
			collection.ConfigureProtectedDataFactoryServices();
			collection.ConfigureProtectedDataSqlExtensions(ApplicationType.Web);
			var serviceProvider = collection.BuildServiceProvider();
			var protectedDataServiceFactory = serviceProvider.GetRequiredService<IProtectedDataServiceFactory>();
			var sqlConnectionProvider = serviceProvider.GetRequiredService<ISqlConnectionProvider>();

			var authProvider = new CargoWiseAuthStateProvider(NullLogger<CargoWiseAuthStateProvider>.Instance, It.IsAny<ITokenValidatorWrapper>());
			var options = new CargoWiseAuthOptions();
			var loginHandler = new WinzorCargoWiseLoginHandler(authProvider, Options.Create(options), null);

			return new CargoWiseRuntime(
				logger,
				Options.Create(cargoWiseOptions),
				loginHandler,
				winzorDispatcher,
				new UserMonitorRegistry(NullLogger<UserMonitorRegistry>.Instance, protectedDataServiceFactory, sqlConnectionProvider),
				Mock.Of<IWinzorTelemetry>());
		}
	}
}
