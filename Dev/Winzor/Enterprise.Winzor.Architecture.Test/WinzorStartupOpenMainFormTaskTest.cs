using System;
using System.Threading.Tasks;
using CargoWise.Blazor.Client.Integration;
using CargoWise.Blazor.Client.Integration.Messaging;
using Enterprise.Startup;
using Enterprise.ZArchitecture.Environment;
using Microsoft.JSInterop;
using Moq;
using NUnit.Framework;
using WinzorTestFramework;

namespace Enterprise.Winzor.Architecture.Test;
public class WinzorStartupOpenMainFormTaskTest
{
	[SetUp]
	public async Task SetUpAsync()
	{
		MockLifecycleService = new Mock<ILifecycleService>();
		var clientServices = MockCargoWiseClientServices.MakeMock(lifecycleService: MockLifecycleService.Object);
		await EnterpriseTestSetup.WinzorDispatcher.InvokeAsync(Initialization.ResetSplashFormInstanceForTesting);
		Initialization.SplashFormInstance.ReadyToRender(clientServices);
	}

	[TearDown]
	public async Task TearDownAsync()
	{
		await Initialization.SplashFormInstance.DetachFromRendererAsync();
	}

	[Test]
	public async Task WinzorStartupOpenMainFormTaskShouldCreateMainFormAndReturnTrueWhenLoginSuccess()
	{
		await EnterpriseTestSetup.WinzorDispatcher.InvokeAsync(() =>
		{
			var task = new WinzorStartupOpenMainFormTask();
			var arguments = new ApplicationArguments(Array.Empty<string>());
			StartupOpenMainFormTask.MainFormInstance?.Dispose();
			StartupOpenMainFormTask.MainFormInstance = null;
			Assert.That(LoginDirector.Instance.AuthenticatedUser.IsOK, Is.True);
			Assert.That(LoginDirector.Instance.LoggedInLocation, Is.True);

			var result = task.Execute(arguments);

			Assert.That(result, Is.True);
			Assert.That(StartupOpenMainFormTask.MainFormInstance, Is.Not.Null);
			Assert.That(StartupOpenMainFormTask.MainFormInstance.Controls.Find("LoginLocationControl", true), Has.Length.EqualTo(0));
			MockLifecycleService.Verify(ls => ls.StartNewApplicationAsync(It.IsAny<Uri>()), Times.Never);
		});
	}

	[Test]
	public async Task WinzorStartupNoLoggedInLocationShowsLocationControl()
	{
		await EnterpriseTestSetup.WinzorDispatcher.InvokeAsync(() =>
		{
			var task = new WinzorStartupOpenMainFormTask();
			var arguments = new ApplicationArguments(Array.Empty<string>());
			StartupOpenMainFormTask.MainFormInstance?.Dispose();
			StartupOpenMainFormTask.MainFormInstance = null;
			Assert.That(LoginDirector.Instance.AuthenticatedUser.IsOK, Is.True);
			var originalLoggedInLocation = LoginDirector.Instance.LoggedInLocation;
			try
			{
				LoginDirector.UpdateLoginState(LoginDirector.Instance.AuthenticatedUser, false);
				var result = task.Execute(arguments);

				Assert.That(result, Is.True);
				Assert.That(StartupOpenMainFormTask.MainFormInstance, Is.Not.Null);
				Assert.That(StartupOpenMainFormTask.MainFormInstance.Controls.Find("LoginLocationControl", true), Has.Length.EqualTo(1));
				MockLifecycleService.Verify(ls => ls.StartNewApplicationAsync(It.IsAny<Uri>()), Times.Never);
			}
			finally
			{
				LoginDirector.UpdateLoginState(LoginDirector.Instance.AuthenticatedUser, originalLoggedInLocation);
			}
		});
	}

	[Test]
	public async Task WinzorStartupOpenMainFormTaskShouldNotLaunchNewInstanceAndReturnFalseWhenLoginFailedAndDonotRetry()
	{
		UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Cancel);
		await EnterpriseTestSetup.WinzorDispatcher.InvokeAsync(() =>
		{
			var task = new WinzorStartupOpenMainFormTask();
			var arguments = new ApplicationArguments(Array.Empty<string>());
			var originalAuthenticatedUser = LoginDirector.Instance.AuthenticatedUser;
			try
			{
				LoginDirector.Instance.AuthenticatedUser = null;
				StartupOpenMainFormTask.MainFormInstance?.Dispose();
				StartupOpenMainFormTask.MainFormInstance = null;
				Assert.That(LoginDirector.Instance.AuthenticatedUser.IsOK, Is.False);

				var result = task.Execute(arguments);

				Assert.That(UnitTestUserNotification.Instance.LastMessage.Text, Is.EqualTo("Would you like to restart and try to login again?"));
				Assert.That(result, Is.False);
				Assert.That(StartupOpenMainFormTask.MainFormInstance, Is.Null);
				MockLifecycleService.Verify(ls => ls.StartNewApplicationAsync(It.IsAny<Uri>()), Times.Never);
			}
			finally
			{
				LoginDirector.Instance.AuthenticatedUser = originalAuthenticatedUser;
				var result = task.Execute(arguments);
				Assert.That(result, Is.True);
				Assert.That(StartupOpenMainFormTask.MainFormInstance, Is.Not.Null);
			}
		});
	}

	[Test]
	public async Task WinzorStartupOpenMainFormTaskShouldNotLaunchNewInstanceAndReturnFalseWhenLoginFailedAndDoRetry()
	{
		UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.OK);
		await EnterpriseTestSetup.WinzorDispatcher.InvokeAsync(() =>
		{
			var task = new WinzorStartupOpenMainFormTask();
			var arguments = new ApplicationArguments(Array.Empty<string>());
			var originalAuthenticatedUser = LoginDirector.Instance.AuthenticatedUser;
			try
			{
				LoginDirector.Instance.AuthenticatedUser = null;
				StartupOpenMainFormTask.MainFormInstance?.Dispose();
				StartupOpenMainFormTask.MainFormInstance = null;
				Assert.That(LoginDirector.Instance.AuthenticatedUser.IsOK, Is.False);

				var result = task.Execute(arguments);

				Assert.That(UnitTestUserNotification.Instance.LastMessage.Text, Is.EqualTo("Would you like to restart and try to login again?"));
				Assert.That(result, Is.False);
				Assert.That(StartupOpenMainFormTask.MainFormInstance, Is.Null);
				MockLifecycleService.Verify(ls => ls.StartNewApplicationAsync(It.IsAny<Uri>()), Times.Once);
			}
			finally
			{
				LoginDirector.Instance.AuthenticatedUser = originalAuthenticatedUser;
				var result = task.Execute(arguments);
				Assert.That(result, Is.True);
				Assert.That(StartupOpenMainFormTask.MainFormInstance, Is.Not.Null);
			}
		});
	}

	[Test, WithTransaction]
	public async Task WinzorStartupShouldSetLaunchProtocolInLocalStorageWhenMainFormShown()
	{
		using var ctx = new EnterpriseTestContext();
		var mockJS = new Mock<IJSRuntime>();
		var clientServices = MockCargoWiseClientServices.MakeMock(jsRuntime: mockJS.Object);
		await ctx.RenderFormAsync(() => Initialization.MainFormInstance, clientServices);
		mockJS.Verify(js => js.InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>("localStorage.setItem", It.Is<object[]>(args => args.Length == 2 && args[0].Equals(LocalStorageItemKeys.LaunchProtocol))), Times.Once);
	}

	Mock<ILifecycleService> MockLifecycleService { get; set; }
}
