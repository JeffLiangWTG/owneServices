using System;
using System.Threading.Tasks;
using CargoWise.Blazor.Client.Integration.Messaging;
using CargoWise.Blazor.Common;
using CargoWise.Common;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using WinzorTestFramework;

namespace Enterprise.Winzor.Architecture.Test;
class WinzorCargoWiseLoginHandlerTest
{
	[SetUp]
	public async Task SetUpAsync()
	{
		AuthOptions = new CargoWiseAuthOptions();
		MockAuthStateProvider = new Mock<ICargoWiseAuthStateProvider>();
		MockLogger = new Mock<ILogger<WinzorCargoWiseLoginHandler>>();
		LoginHandler = new WinzorCargoWiseLoginHandler(MockAuthStateProvider.Object, Options.Create(AuthOptions), MockLogger.Object);
		MockLifecycleService = new Mock<ILifecycleService>();
		var clientServices = MockCargoWiseClientServices.MakeMock(lifecycleService: MockLifecycleService.Object);
		await EnterpriseTestSetup.WinzorDispatcher.InvokeAsync(Initialization.ResetSplashFormInstanceForTesting);
		Initialization.SplashFormInstance.ReadyToRender(clientServices, Guid.NewGuid());
	}

	[TearDown]
	public async Task TearDownAsync()
	{
		await Initialization.SplashFormInstance.DetachFromRendererAsync();
	}

	CargoWiseAuthOptions AuthOptions { get; set; }
	Mock<ICargoWiseAuthStateProvider> MockAuthStateProvider { get; set; }
	Mock<ILifecycleService> MockLifecycleService { get; set; }
	Mock<ILogger<WinzorCargoWiseLoginHandler>> MockLogger { get; set; }
	IWinzorCargoWiseLoginHandler LoginHandler { get; set; }

	[Test]
	public async Task LoginShouldReturnTrueWhenLoginSuccess()
	{
		var userMock = new Mock<IUser>();
		MockAuthStateProvider.Setup(asp => asp.Login(MockAuthStateProvider.Object, AuthOptions)).Returns(CargoWiseAuthResult.Success(LoginAuthenticationInfo.NewSuccessfulLogin(userMock.Object)));

		await EnterpriseTestSetup.WinzorDispatcher.InvokeAsync(() =>
		{
			var result = LoginHandler.Login();
			Assert.That(UnitTestUserNotification.Instance.LastMessage.Text, Is.Null);
			Assert.That(result, Is.True);
			MockLifecycleService.Verify(ls => ls.StartNewApplicationAsync(It.IsAny<Uri>()), Times.Never);
			MockLogger.Verify(
				logger => logger.Log(
					LogLevel.Error,
					It.IsAny<EventId>(),
					It.Is<It.IsAnyType>((v, t) => v.ToString() == "Error login."),
					It.IsAny<Exception>(),
					(Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
				Times.Never());
		});
	}

	[Test]
	public async Task LoginShouldNotLaunchNewInstanceAndReturnTrueWhenLoginFailedAndDonotRetry()
	{
		MockAuthStateProvider.Setup(asp => asp.Login(MockAuthStateProvider.Object, AuthOptions)).Returns(CargoWiseAuthResult.Failed("Sample failure message", canRetryLogin: true));

		await EnterpriseTestSetup.WinzorDispatcher.InvokeAsync(() =>
		{
			var result = LoginHandler.Login();
			Assert.That(result, Is.True);
			MockLifecycleService.Verify(ls => ls.StartNewApplicationAsync(It.IsAny<Uri>()), Times.Never);
			MockLogger.Verify(
				logger => logger.Log(
					LogLevel.Error,
					It.IsAny<EventId>(),
					It.Is<It.IsAnyType>((v, t) => v.ToString() == "Error login."),
					It.IsAny<Exception>(),
					(Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
				Times.Never());
		});
	}

	[Test]
	public async Task LoginShouldNotLaunchNewInstanceAndReturnTrueWhenLoginFailedAndCannotRetry()
	{
		MockAuthStateProvider.Setup(asp => asp.Login(MockAuthStateProvider.Object, AuthOptions)).Returns(CargoWiseAuthResult.Failed("Sample failure message"));

		await EnterpriseTestSetup.WinzorDispatcher.InvokeAsync(() =>
		{
			var result = LoginHandler.Login();
			Assert.That(result, Is.True);
			MockLifecycleService.Verify(ls => ls.StartNewApplicationAsync(It.IsAny<Uri>()), Times.Never);
			MockLogger.Verify(
				logger => logger.Log(
					LogLevel.Error,
					It.IsAny<EventId>(),
					It.Is<It.IsAnyType>((v, t) => v.ToString() == "Error login."),
					It.IsAny<Exception>(),
					(Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
				Times.Never());
		});
	}

	[Test]
	public async Task LoginShouldNotLaunchNewInstanceAndReturnFalseWhenLoginError()
	{
		MockAuthStateProvider.Setup(asp => asp.Login(MockAuthStateProvider.Object, AuthOptions)).Throws(new Exception("Test exception"));

		UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.OK);

		await EnterpriseTestSetup.WinzorDispatcher.InvokeAsync(() =>
		{
			var result = LoginHandler.Login();
			Assert.That(UnitTestUserNotification.Instance.LastMessage.Text, Is.EqualTo($"Unexpected error encountered during login attempt."));
			Assert.That(result, Is.False);
			MockLifecycleService.Verify(ls => ls.StartNewApplicationAsync(It.IsAny<Uri>()), Times.Never);
			MockLogger.Verify(
				logger => logger.Log(
					LogLevel.Error,
					It.IsAny<EventId>(),
					It.Is<It.IsAnyType>((v, t) => v.ToString() == "Error login."),
					It.IsAny<Exception>(),
					(Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
				Times.Once());
		});
	}

	[Test]
	public async Task LoginShouldNotLogErrorAndThrowEceptionWhenLoginErrorWithCriticalException()
	{
		MockAuthStateProvider.Setup(asp => asp.Login(MockAuthStateProvider.Object, AuthOptions)).Throws(new CriticalException("Test critical exception"));

		UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.OK);

		await EnterpriseTestSetup.WinzorDispatcher.InvokeAsync(() =>
		{
			Assert.Throws<CriticalException>(() => LoginHandler.Login());

			MockLifecycleService.Verify(ls => ls.StartNewApplicationAsync(It.IsAny<Uri>()), Times.Never);
			MockLogger.Verify(
				logger => logger.Log(
					LogLevel.Error,
					It.IsAny<EventId>(),
					It.Is<It.IsAnyType>((v, t) => v.ToString() == "Error login."),
					It.IsAny<Exception>(),
					(Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
				Times.Never());
		});
	}

	class CriticalException : Exception, ICriticalException
	{
		public bool IsCriticalException => true;
		public CriticalException(string message) : base(message)
		{
		}
	}
}
