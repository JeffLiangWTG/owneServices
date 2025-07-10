using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bunit;
using Bunit.Extensions.WaitForHelpers;
using CargoWise.Async;
using CargoWise.Blazor.Client.Integration;
using CargoWise.Blazor.Client.Integration.DependencyInjection;
using CargoWise.Blazor.Client.Integration.Messaging;
using CargoWise.Blazor.Common;
using CargoWise.Common;
using CargoWise.DataProtection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Startup;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.JSInterop;
using Microsoft.Playwright;
using Moq;
using NUnit.Framework;
using WinzorFramework;
using WinzorFramework.Extensions;
using WinzorFramework.JSInterop;
using WinzorTestFramework;
using WTG.PlaywrightTesting;
using FormBorderStyle = CargoWise.Blazor.Client.Integration.Messaging.FormBorderStyle;
using FormStartPosition = CargoWise.Blazor.Client.Integration.Messaging.FormStartPosition;

namespace Enterprise.Winzor.Architecture.Test;

using static PlaywrightTestContext;

class EntryPointComponentTest
{
	[SetUp]
	public void RunBeforeEachTest()
	{
		EntryPointComponent.ReadyMessageSent = false;
	}

	[TestCase("ShowEditForm", "Browse")]
	[TestCase("ShowViewForm", "ReadOnly")]
	[TestCase("ShowDeleteForm", "Delete")]
	[WithTransaction]
	public async Task HandleUnconstructedShowForm(string command, string expectedDisplayMode)
	{
		using var ctx = new EnterpriseTestContext();
		var pk = Guid.Empty;
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var factory = new BusinessObjectFactory();
			var dummy = factory.New<DummyBusinessObject>();
			factory.Save();
			pk = dummy.PK.ToGuid();
		});

		var queryString = new QueryString
		{
			{ "Command", command },
			{ "ControllerID", DummyControllerIDs.Dummy.Name },
			{ "BusinessEntityPK", pk.ToString() }
		};
		var uri = new UriBuilder(TestNavigationManager.BaseServerUri) { Query = queryString.ToString() }.Uri.ToString();
		var mockWindowsService = new Mock<IWindowService>();

		var rendered = ctx.RenderEntryPointComponent(uri, mockWindowsService.Object);
		Assert.That(rendered.Instance.Form, Is.TypeOf<ZDummyForm>());
		Assert.That(((DummyBusinessObject)((ZDummyForm)rendered.Instance.Form).BusinessEntity).PK.ToGuid(), Is.EqualTo(pk));
		Assert.That(((ZDummyForm)rendered.Instance.Form).DisplayMode, Is.EqualTo(Enum.Parse(typeof(ODisplayMode), expectedDisplayMode)));

		mockWindowsService.Verify(o => o.RequestShowWindowAsync(
			rendered.Instance.Form.GenerateShowWindowOptions(),
			rendered.Instance.Form.GenerateWindowStyleOptions()),
		Times.Once);

		Assert.That(rendered.Instance.Form.CargoWiseClientServices, Is.Not.Null);
	}

	[Test]
	public async Task DisposeFormRenderNotShowNotFound()
	{
		using var ctx = new EnterpriseTestContext();
		Uri uri = null;
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var form = new ZForm();
			uri = ctx.WinzorDispatcher.FormInstanceRegister.Add(TestNavigationManager.BaseServerUri, form);
			form.Dispose();
		});
		var mockWindowsService = new Mock<IWindowService>();
		Assert.Throws<WaitForFailedException>(() => ctx.RenderEntryPointComponent(uri.ToString(), mockWindowsService.Object, 5));
		mockWindowsService.Verify(o => o.RequestCloseAsync(), Times.Once());
	}

	[Test]
	public void HandleUnconstructedShowEditFormNotFoundPk()
	{
		using var ctx = new EnterpriseTestContext();
		var queryString = new QueryString();
		queryString.Add("Command", "ShowEditForm");
		queryString.Add("ControllerID", DummyControllerIDs.Dummy.Name);
		queryString.Add("BusinessEntityPK", Guid.NewGuid().ToString());
		var uri = new UriBuilder(TestNavigationManager.BaseServerUri) { Query = queryString.ToString() }.Uri.ToString();
		var mockWindowsService = new Mock<IWindowService>();
		var rendered = ctx.RenderEntryPointComponent(uri, mockWindowsService.Object);
		rendered.WaitForState(() => rendered.Markup.Contains("Not Found", StringComparison.OrdinalIgnoreCase));
	}

	[Test]
	public void HandleUnconstructedShowEditFormInvalidController()
	{
		using var ctx = new EnterpriseTestContext();
		var queryString = new QueryString();
		queryString.Add("Command", "ShowEditForm");
		queryString.Add("ControllerID", "NoSuchController");
		queryString.Add("BusinessEntityPK", Guid.NewGuid().ToString());
		var uri = new UriBuilder(TestNavigationManager.BaseServerUri) { Query = queryString.ToString() }.Uri.ToString();
		var mockWindowsService = new Mock<IWindowService>();
		var rendered = ctx.RenderEntryPointComponent(uri, mockWindowsService.Object);
		rendered.WaitForState(() => rendered.Markup.Contains("Not Found", StringComparison.OrdinalIgnoreCase));
	}

	[Test]
	public void HandleUnconstructedShowEditFormMissingParameters()
	{
		using var ctx = new EnterpriseTestContext();
		var queryString = new QueryString();
		queryString.Add("Command", "ShowEditForm");
		var uri = new UriBuilder(TestNavigationManager.BaseServerUri) { Query = queryString.ToString() }.Uri.ToString();
		var mockWindowsService = new Mock<IWindowService>();
		var rendered = ctx.RenderEntryPointComponent(uri, mockWindowsService.Object);
		rendered.WaitForState(() => rendered.Markup.Contains("Not Found", StringComparison.OrdinalIgnoreCase));
	}

	[Test]
	public void HandleInvalidCommand()
	{
		using var ctx = new EnterpriseTestContext();
		var queryString = new QueryString();
		queryString.Add("Command", "NoSuchCommand");
		var uri = new UriBuilder(TestNavigationManager.BaseServerUri) { Query = queryString.ToString() }.Uri.ToString();
		var mockWindowsService = new Mock<IWindowService>();
		var rendered = ctx.RenderEntryPointComponent(uri, mockWindowsService.Object);
		rendered.WaitForState(() => rendered.Markup.Contains("Not Found", StringComparison.OrdinalIgnoreCase));
	}

	[Test]
	public void HandleUnconstructedShowNewForm()
	{
		using var ctx = new EnterpriseTestContext();
		var queryString = new QueryString
		{
			{ "Command", "ShowNewForm" },
			{ "ControllerID", DummyControllerIDs.Dummy.Name }
		};
		var uri = new UriBuilder(TestNavigationManager.BaseServerUri) { Query = queryString.ToString() }.Uri.ToString();
		var mockWindowsService = new Mock<IWindowService>();

		var rendered = ctx.RenderEntryPointComponent(uri, mockWindowsService.Object);
		Assert.That(rendered.Instance.Form, Is.TypeOf<ZDummyForm>());
		Assert.That(((DummyBusinessObject)((ZDummyForm)rendered.Instance.Form).BusinessEntity).IsInDatabase, Is.EqualTo(false));

		mockWindowsService.Verify(o => o.RequestShowWindowAsync(
			rendered.Instance.Form.GenerateShowWindowOptions(),
			rendered.Instance.Form.GenerateWindowStyleOptions()),
		Times.Once);

		Assert.That(rendered.Instance.Form.CargoWiseClientServices, Is.Not.Null);
	}

	[Test]
	public void HandleUnconstructedShowModule()
	{
		var initializedAction = ShowModuleUrlHandler.Instance.OnFormInitialized;
		ShowModuleUrlHandler.Instance.OnFormInitialized = null;
		try
		{
			using var ctx = new EnterpriseTestContext();
			var queryString = new QueryString
			{
				{ "Command", "ShowModule" },
				{ "ModuleID", DummyModuleIDs.Dummy.Name }
			};
			var uri = new UriBuilder(TestNavigationManager.BaseServerUri) { Query = queryString.ToString() }.Uri.ToString();
			var mockWindowsService = new Mock<IWindowService>();

			var rendered = ctx.RenderEntryPointComponent(uri, mockWindowsService.Object);
			Assert.That(rendered.Instance.Form, Is.InstanceOf<EmbeddedModulePopup>());
			Assert.That(((EmbeddedModulePopup)rendered.Instance.Form).CurrentModule.ID, Is.EqualTo(DummyModuleIDs.Dummy.Name));

			mockWindowsService.Verify(o => o.RequestShowWindowAsync(
				rendered.Instance.Form.GenerateShowWindowOptions(),
				rendered.Instance.Form.GenerateWindowStyleOptions()),
			Times.Once);

			Assert.That(rendered.Instance.Form.CargoWiseClientServices, Is.Not.Null);
		}
		finally
		{
			ShowModuleUrlHandler.Instance.OnFormInitialized = initializedAction;
		}
	}

	[Test]
	public void HandleInvalidTransientForm()
	{
		using var ctx = new EnterpriseTestContext();
		var queryString = new QueryString();
		queryString.Add("Command", EnterpriseRegisteredFormInstances.ShowTransientFormUrlCommand);
		queryString.Add("ID", Guid.NewGuid().ToString("N"));
		var uri = new UriBuilder(TestNavigationManager.BaseServerUri) { Query = queryString.ToString() }.Uri.ToString();
		var mockWindowsService = new Mock<IWindowService>();
		var rendered = ctx.RenderEntryPointComponent(uri, mockWindowsService.Object);
		rendered.WaitForState(() => rendered.Markup.Contains("Not Found", StringComparison.OrdinalIgnoreCase));
	}

	[Test, WithTransaction]
	public async Task ReloadShowFormWithDisposeSucceeds()
	{
		using var ctx = new EnterpriseTestContext();
		var pk = Guid.Empty;
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var factory = new BusinessObjectFactory();
			var dummy = factory.New<DummyBusinessObject>();
			factory.Save();
			pk = dummy.PK.ToGuid();
		});
		var queryString = new QueryString
		{
			{ "Command", "ShowEditForm" },
			{ "ControllerID", DummyControllerIDs.Dummy.Name },
			{ "BusinessEntityPK", pk.ToString() }
		};
		var uri = new UriBuilder(TestNavigationManager.BaseServerUri) { Query = queryString.ToString() }.Uri.ToString();
		var mockWindowsService = new Mock<IWindowService>();

		var rendered = ctx.RenderEntryPointComponent(uri, mockWindowsService.Object);
		Assert.That(rendered.Instance.Form, Is.TypeOf<ZDummyForm>());
		await rendered.Instance.Form.InvokeWinzorDispatcherAsync(() => rendered.Instance.Form.Close());

		using var ctx2 = new EnterpriseTestContext();
		var rendered2 = ctx2.RenderEntryPointComponent(uri, mockWindowsService.Object).Instance;
		Assert.That(((DummyBusinessObject)((ZDummyForm)rendered2.Form).BusinessEntity).PK.ToGuid(), Is.EqualTo(pk));

		mockWindowsService.Verify(o => o.RequestShowWindowAsync(
			rendered.Instance.Form.GenerateShowWindowOptions(),
			rendered.Instance.Form.GenerateWindowStyleOptions()),
			Times.Exactly(2));

		Assert.That(rendered2.Form.CargoWiseClientServices, Is.Not.Null);
	}

	[Test, WithTransaction]
	public async Task ReloadShowFormWithoutDisposeFormIsNull()
	{
		using var ctx = new EnterpriseTestContext();
		var pk = Guid.Empty;
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var factory = new BusinessObjectFactory();
			var dummy = factory.New<DummyBusinessObject>();
			factory.Save();
			pk = dummy.PK.ToGuid();
		});
		var queryString = new QueryString();
		queryString.Add("Command", "ShowEditForm");
		queryString.Add("ControllerID", DummyControllerIDs.Dummy.Name);
		queryString.Add("BusinessEntityPK", pk.ToString());
		var uri = new UriBuilder(TestNavigationManager.BaseServerUri) { Query = queryString.ToString() }.Uri.ToString();
		var mockWindowsService = new Mock<IWindowService>();
		var rendered = ctx.RenderEntryPointComponent(uri, mockWindowsService.Object).Instance;
		Assert.That(rendered.Form, Is.TypeOf<ZDummyForm>());
		using var ctx2 = new EnterpriseTestContext();
		var rendered2 = ctx2.RenderEntryPointComponent(uri, mockWindowsService.Object).Instance;
		Assert.That(rendered2.Form, Is.Null);
	}

	[Test, WithTransaction]
	public void TestWithEmptyOnAfterRenderAsync()
	{
		/*
		 * To ensure that all setup is performed in OnInitializedAsync for performance reasons,
		 * we override OnAfterRenderAsync to not perform any tasks and check that the component is still initialized correctly.
		*/
		using var ctx = new EnterpriseTestContext();
		var queryString = new QueryString();
		queryString.Add("Command", "ShowNewForm");
		queryString.Add("ControllerID", DummyControllerIDs.Dummy.Name);
		var uri = new UriBuilder(TestNavigationManager.BaseServerUri) { Query = queryString.ToString() }.Uri.ToString();
		var mockWindowsService = new Mock<IWindowService>();
		var rendered = ctx.RenderEntryPointComponent<EntryPointComponentWithoutOnAfterRenderAsync>(uri, Mock.Of<ILifecycleService>(), mockWindowsService.Object);
		Assert.That(rendered.Instance.Form, Is.TypeOf<ZDummyForm>());
	}

	[Test, WithTransaction]
	public async Task NotifyReadyAsyncCalled()
	{
		using var ctx = new EnterpriseTestContext();
		var pk = Guid.Empty;
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var factory = new BusinessObjectFactory();
			var dummy = factory.New<DummyBusinessObject>();
			factory.Save();
			pk = dummy.PK.ToGuid();
		});

		var queryString = new QueryString()
		{
			{ "Command", "ShowEditForm" },
			{ "ControllerID", DummyControllerIDs.Dummy.Name },
			{ "BusinessEntityPK", pk.ToString() },
		};
		var uri = new UriBuilder(TestNavigationManager.BaseServerUri) { Query = queryString.ToString() }.Uri.ToString();
		var mockLifecycleService = new Mock<ILifecycleService>();
		ctx.RenderEntryPointComponent(uri, mockLifecycleService.Object);
		mockLifecycleService.Verify(o => o.NotifyApplicationReadyAsync(TimeSpan.FromMinutes(5)), Times.Once());
	}

	[Test, WithTransaction]
	public async Task NotifyReadyAsyncOnlyCalledForInitialForm()
	{
		using var ctx = new EnterpriseTestContext();
		var pk = Guid.Empty;
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var factory = new BusinessObjectFactory();
			var dummy = factory.New<DummyBusinessObject>();
			factory.Save();
			pk = dummy.PK.ToGuid();
		});

		var queryString = new QueryString()
		{
			{ "Command", "ShowEditForm" },
			{ "ControllerID", DummyControllerIDs.Dummy.Name },
			{ "BusinessEntityPK", pk.ToString() },
		};
		var uri = new UriBuilder(TestNavigationManager.BaseServerUri) { Query = queryString.ToString() }.Uri.ToString();
		var mockLifecycleService = new Mock<ILifecycleService>();
		var entryComponent = ctx.RenderEntryPointComponent(uri, mockLifecycleService.Object);
		entryComponent.WaitForState(() => entryComponent.Instance.ComponentResolved);
		mockLifecycleService.Verify(o => o.NotifyApplicationReadyAsync(It.IsAny<TimeSpan>()), Times.Once());

		var entryPointComponent = ctx.RenderComponent<EntryPointComponent>();
		entryPointComponent.WaitForState(() => entryPointComponent.Instance.ComponentResolved);
		mockLifecycleService.Verify(o => o.NotifyApplicationReadyAsync(It.IsAny<TimeSpan>()), Times.Once());
	}

	[Test, WithTransaction]
	public async Task ShowFormWithRealCargoWiseClient()
	{
		using var ctx = new EnterpriseTestContext();
		var pk = Guid.Empty;
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var factory = new BusinessObjectFactory();
			var dummy = factory.New<DummyBusinessObject>();
			factory.Save();
			pk = dummy.PK.ToGuid();
		});
		var queryString = new QueryString
		{
			{ "Command", "ShowEditForm" },
			{ "ControllerID", DummyControllerIDs.Dummy.Name },
			{ "BusinessEntityPK", pk.ToString() }
		};
		var uri = new UriBuilder(TestNavigationManager.BaseServerUri) { Query = queryString.ToString() }.Uri.ToString();

		ctx.Services.AddSingleton<NavigationManager>(new TestNavigationManager(uri));
		ctx.Services.AddSingleton(ctx.WinzorDispatcher);
		ctx.Services.AddSingleton<IDownloadObjectManager, DownloadObjectManager>();
		ctx.Services.AddTransient<IFileService, FileService>();
		ctx.Services.AddTransient<IClientEventService, ClientEventService>();
		ctx.Services.AddCargoWiseClient();
		ctx.Services.Decorate<IWindowService, WinzorWindowService>();
		ctx.JSInterop.SetupVoid("window.chrome.webview.postMessage", _ => true).SetVoidResult();
		ctx.JSInterop.SetupVoid("window.cargoWiseClient.setApplicationReady", "\"00:00:30\"").SetVoidResult();
		ctx.JSInterop.Setup<string>("window.cargoWiseClient.openMenu", _ => true).SetResult("Shown");

		var rendered = ctx.RenderEntryPointComponent(uri);
		rendered.WaitForState(() => rendered.Instance.Form != null, TimeSpan.FromSeconds(30));
	}

	[TestCase("ShowEditForm")]
	[TestCase("InvalidCommand")]
	[WithTransaction]
	public async Task MarkedReadyAfterRenderEntryPointComponentCalled(string command)
	{
		using var ctx = new EnterpriseTestContext();
		var pk = Guid.Empty;
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var factory = new BusinessObjectFactory();
			var dummy = factory.New<DummyBusinessObject>();
			factory.Save();
			pk = dummy.PK.ToGuid();
		});
		var queryString = new QueryString { { "Command", command }, { "ControllerID", DummyControllerIDs.Dummy.Name }, { "BusinessEntityPK", pk.ToString() } };
		var uri = new UriBuilder(TestNavigationManager.BaseServerUri) { Query = queryString.ToString() }.Uri.ToString();
		var mockLifecycleService = new Mock<ILifecycleService>();
		var rendered = ctx.RenderEntryPointComponent(uri, mockLifecycleService.Object);
		Assert.That(rendered.Instance.ComponentResolved, Is.True);
	}

	[Test]
	public async Task DoNotLogJSDisconnectedErrorDuringFormConstruction()
	{
		using var ctx = new EnterpriseTestContext();
		Uri uri = null;
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var form = new ZForm();
			uri = ctx.WinzorDispatcher.FormInstanceRegister.Add(TestNavigationManager.BaseServerUri, form);
		});
		var mockLogger = new Mock<ILogger<EntryPointComponent>>();
		var mockWindowsService = new Mock<IWindowService>();
		mockWindowsService.SetupSequence(
			o => o.RequestShowWindowAsync(It.IsAny<ShowWindowOptions>(), It.IsAny<WindowStyleOptions>()))
			.Throws(new JSDisconnectedException("Mock SignalR is not connected correctly"))
			.Throws(new ObjectDisposedException("Mock Object has disposed"));

		ctx.RenderEntryPointComponent(uri.ToString(), mockWindowsService.Object, mockLogger.Object);
		mockWindowsService.Verify(
						o => o.RequestShowWindowAsync(It.IsAny<ShowWindowOptions>(), It.IsAny<WindowStyleOptions>()), Times.Once());

		mockLogger.Verify(
			o => o.Log(
				It.Is<LogLevel>(logLevel => logLevel == LogLevel.Error),
				It.IsAny<EventId>(),
				It.Is<It.IsAnyType>((v, t) => true),
				It.Is<Exception>(ex => ex is JSDisconnectedException || ex is ObjectDisposedException),
				It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Never);
	}

	[Test, WithSnapshotProtection]
	public void ShowErrorReporterFormDuringFormConstruction()
	{
		ExceptionReporter.DisableExposed(); // reset the instance becase internal state prevents multiple exceptions being reported during a short period of time
		var mockSystemRegistry = new Mock<ISystemDataRegistry>();
		mockSystemRegistry.SetupGet(o => o.ColorTheme).Throws(new InvalidOperationException());

		using (CargoWise.Application.ObjectFactory.Substitute(mockSystemRegistry.Object))
		{
			using var isTestOverride = Globals.TemporaryOverrideForIsTest(false);
			using var ctx = new EnterpriseTestContext();
			var queryString = new QueryString
			{
				{ "Command", "ShowNewForm" },
				{ "ControllerID", DummyControllerIDs.Dummy.Name }
			};
			var uri = new UriBuilder(TestNavigationManager.BaseServerUri) { Query = queryString.ToString() }.Uri.ToString();
			var mockWindowsService = new Mock<IWindowService>();

			var rendered = ctx.RenderEntryPointComponent(uri, mockWindowsService.Object);
			Assert.That(() => rendered.Instance.Form, Is.TypeOf<ExceptionReportingForm>().After(3000, 100));

			mockWindowsService.Verify(o => o.RequestShowWindowAsync(
			rendered.Instance.Form.GenerateShowWindowOptions(),
			rendered.Instance.Form.GenerateWindowStyleOptions()),
				Times.Once());

			Assert.That(() => rendered.Instance.Form.CargoWiseClientServices, Is.Not.Null.After(3000, 100));
		}
	}

	[Test, WithSnapshotProtection]
	public async Task EntryPointComponentTestMainFormAsync()
	{
		await ReInitializeMainForm();

		using var ctx = new EnterpriseTestContext();
		var uri = new UriBuilder(TestNavigationManager.BaseServerUri).Uri.ToString();
		var rendered = ctx.RenderEntryPointComponent(uri);
		Assert.That(rendered.Instance.Form, Is.TypeOf<MainForm>());
	}

	[Test]
	public async Task NotifyRenderRequiredAfterShowingDialogInOnShown()
	{
		ErrorReporter.Clear();

		using var ctx = new EnterpriseTestContext();
		Form newForm = null;
		var dialogCallerTcs = new TaskCompletionSource();
		var windowService = new Mock<IWindowService>();
		var loadRequestSent = new TaskCompletionSource();
		string loadRequestUrl = null;
		windowService.Setup(o => o.RequestCreateAndShowWindowAsync(It.IsAny<CreateWindowOptions>(), It.IsAny<ShowWindowOptions>(), It.IsAny<WindowStyleOptions>()))
			.Callback<CreateWindowOptions, ShowWindowOptions, WindowStyleOptions>((createWindowOptions, showWindowOptions, windowStyleOptions) =>
			{
				loadRequestUrl = createWindowOptions.Uri.ToString();
				loadRequestSent.SetResult();
			});
		var renderedForm = await ctx.RenderEntryPointComponent(() =>
		{
			var form = new Form();
			var button = new Button();
			form.Controls.Add(button);
			form.Shown += Form_Shown;
			return form;
		}, windowService.Object);
		Assert.That(await loadRequestSent.Task.WithTimeout(TimeSpan.FromSeconds(1)), Is.True);

		using var ctx2 = new EnterpriseTestContext();
		var renderedNewForm = ctx2.RenderEntryPointComponent(loadRequestUrl);
		renderedNewForm.WaitForState(() => renderedNewForm.Instance.Form != null);
		Assert.That(renderedNewForm.Instance.Form, Is.SameAs(newForm));
		renderedNewForm.Instance.Form.Dispose();

		Assert.That(await dialogCallerTcs.Task.WithTimeout(TimeSpan.FromSeconds(1)), Is.True);
		Assert.That(await renderedForm.Instance.LoadFormTask.WithTimeout(TimeSpan.FromSeconds(1)), Is.True);
		Assert.That(ErrorReporter.LastExceptionReported, Is.Null);

		Assert.That(() => renderedForm.Find("button").TextContent, Is.EqualTo("Foo").After(3000, 100));

		void Form_Shown(object sender, EventArgs e)
		{
			newForm = new Form();
			newForm.ShowDialog();
			dialogCallerTcs.SetResult();
			var button = ((Form)sender).Controls.Single(c => c is Button);
			button.Text = "Foo";
		}
	}

	[Test]
	public async Task ShowDialogAfterShowDialogInOnShown()
	{
		ErrorReporter.Clear();

		using var ctx = new EnterpriseTestContext();
		Form newForm1 = null;
		Form newForm2 = null;
		var dialogCallerTcs = new TaskCompletionSource();
		var windowService = new Mock<IWindowService>();
		var loadRequestSent = new TaskCompletionSource();
		string loadRequestUrl = null;
		windowService
			.Setup(o => o.RequestCreateAndShowWindowAsync(It.IsAny<CreateWindowOptions>(), It.IsAny<ShowWindowOptions>(), It.IsAny<WindowStyleOptions>()))
			.Callback<CreateWindowOptions, ShowWindowOptions, WindowStyleOptions>((createWindowOptions, showWindowOptions, windowStyleOptions) =>
			{
				loadRequestUrl = createWindowOptions.Uri.ToString();
				loadRequestSent.SetResult();
			});
		var renderedForm = await ctx.RenderEntryPointComponent(() =>
		{
			var form = new Form();
			var button = new Button();
			form.Controls.Add(button);
			form.Shown += Form_Shown;
			return form;
		}, windowService.Object);
		Assert.That(await loadRequestSent.Task.WithTimeout(TimeSpan.FromSeconds(1)), Is.True);

		using var ctx2 = new EnterpriseTestContext();
		var renderedNewForm1 = ctx2.RenderEntryPointComponent(loadRequestUrl);
		renderedNewForm1.WaitForState(() => renderedNewForm1.Instance.Form != null);
		Assert.That(renderedNewForm1.Instance.Form, Is.SameAs(newForm1));
		loadRequestSent = new TaskCompletionSource();
		renderedNewForm1.Instance.Form.Dispose();

		Assert.That(await loadRequestSent.Task.WithTimeout(TimeSpan.FromSeconds(1)), Is.True);
		using var ctx3 = new EnterpriseTestContext();
		var renderedNewForm2 = ctx3.RenderEntryPointComponent(loadRequestUrl);
		renderedNewForm2.WaitForState(() => renderedNewForm2.Instance.Form != null);
		Assert.That(renderedNewForm2.Instance.Form, Is.SameAs(newForm2));
		loadRequestSent = new TaskCompletionSource();
		renderedNewForm2.Instance.Form.Dispose();

		Assert.That(await dialogCallerTcs.Task.WithTimeout(TimeSpan.FromSeconds(1)), Is.True);

		Assert.That(await renderedForm.Instance.LoadFormTask.WithTimeout(TimeSpan.FromSeconds(1)), Is.True);
		Assert.That(ErrorReporter.LastExceptionReported, Is.Null);

		void Form_Shown(object sender, EventArgs e)
		{
			newForm1 = new Form();
			newForm1.ShowDialog();
			newForm2 = new Form();
			newForm2.ShowDialog();
			dialogCallerTcs.SetResult();
		}
	}

	[Test]
	public async Task ShouldNotCloseFormIfAlreadyClosedOnDispose()
	{
		using var ctx = new EnterpriseTestContext();
		Form form = null;
		var closedCount = 0;
		var formClosedCount = 0;
		var disposedCount = 0;
		var rendered = await ctx.RenderEntryPointComponent(() =>
		{
			form = new Form();
			form.Closed += (sender, args) => closedCount++;
			form.FormClosed += (sender, args) => formClosedCount++;
			form.Disposed += (sender, args) => disposedCount++;
			return form;
		}, Mock.Of<IWindowService>());
		Assert.That(form, Is.Not.Null);
		await form.InvokeWinzorDispatcherAsync(() => form.Close());
		await rendered.FindComponent<ControlProxyComponent>().Instance.DisposeAsync();
		Assert.That(closedCount, Is.EqualTo(1));
		Assert.That(formClosedCount, Is.EqualTo(1));
		Assert.That(disposedCount, Is.EqualTo(1));
	}

	[Test]
	public async Task OpenFormFromRunInAnotherWinformsThreadAsync()
	{
		using var ctx = new EnterpriseTestContext();
		Form newForm = null;
		Task otherThreadTask = null;
		var newFormShownThreadId = 0;
		var windowService = new Mock<IWindowService>();
		var loadRequestSent = new TaskCompletionSource();
		string loadRequestUrl = null;
		windowService
			.Setup(o => o.RequestCreateAndShowWindowAsync(It.IsAny<CreateWindowOptions>(), It.IsAny<ShowWindowOptions>(), It.IsAny<WindowStyleOptions>()))
			.Callback<CreateWindowOptions, ShowWindowOptions, WindowStyleOptions>((createWindowOptions, showWindowOptions, windowStyleOptions) =>
			{
				loadRequestUrl = createWindowOptions.Uri.ToString();
				loadRequestSent.SetResult();
			});
		var renderedForm = await ctx.RenderEntryPointComponent(() =>
		{
			var form = new Form();
			var button = new Button();
			form.Controls.Add(button);
			button.Click += Button_Click;
			return form;
		}, windowService.Object);
		await renderedForm.Find("button").ClickAsync(new WebMouseEventArgs());
		Assert.That(await loadRequestSent.Task.WithTimeout(TimeSpan.FromSeconds(1)), Is.True);
		using var ctx2 = new EnterpriseTestContext();
		var renderedNewForm = ctx2.RenderEntryPointComponent(loadRequestUrl);
		renderedNewForm.WaitForState(() => renderedNewForm.Instance.Form != null);
		Assert.That(renderedNewForm.Instance.Form, Is.SameAs(newForm));
		Assert.That(newForm.WinzorDispatcher.ManagedThreadId, Is.Not.EqualTo(ctx.WinzorDispatcher.ManagedThreadId));
		Assert.That(newFormShownThreadId, Is.EqualTo(newForm.WinzorDispatcher.ManagedThreadId).And.Not.EqualTo(0));

		renderedNewForm.Instance.Form.Dispose();

		Assert.That(await otherThreadTask.WithTimeout(TimeSpan.FromSeconds(1)), Is.True);

		void Button_Click(object sender, EventArgs e)
		{
			otherThreadTask = DefaultAsyncStrategy.Get().RunInAnotherWinformsThreadAsync(() =>
			{
				newForm = new Form();
				newForm.Shown += NewForm_Shown;
				newForm.ShowDialog();
			});
		}

		void NewForm_Shown(object sender, EventArgs e)
		{
			newFormShownThreadId = System.Environment.CurrentManagedThreadId;
		}
	}

	[Test, WithTransaction, WithPlaywrightPage]
	public async Task EnterEventLoopShouldFlushPendingRenderEvents()
	{
		var mockCargoWiseServiceProvider = new MockCargoWiseClientSeviceProvider();
		await using var ctx = new InMemoryAppServerTestContext(mockCargoWiseServiceProvider);
		using var cts = new CancellationTokenSource();
		var page = await ctx.LoadFormAsync(() =>
		{
			var mockContext = Mock.Of<IWinzorDispatcherContext>();
			using var c = ctx.WinzorDispatcher.WithContext(mockContext);
			var factory = new BusinessObjectFactory();
			var dummy = factory.New<DummyBusinessObject>();
			factory.Save();
			var form = (Form)new DummyController().ShowEditForm(dummy);
			form.Shown += (s, e) =>
			{
				WinzorDispatcher.Current.RunMessageLoop(cts);
			};
			return form;
		});

		await page.WaitForSelectorAsync(".textbox:nth-child(3)");

		await cts.CancelAsync();
	}

	[Test, WithSnapshotProtection]
	public async Task UrlRegistrationAndCallbackOnMainForm()
	{
		await ReInitializeMainForm();

		using var ctx = new EnterpriseTestContext();
		var pk1 = Guid.Empty;
		var pk2 = Guid.Empty;
		string expectedLicenseKey = null;
		string ediEntUrl = null;

		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			expectedLicenseKey = EnvProxy.Instance.CurrentCompany.LicenceKeyIdentifier;

			var factory = new BusinessObjectFactory();
			var dummy1 = factory.New<DummyBusinessObject>();
			var dummy2 = factory.New<DummyBusinessObject>();
			factory.Save();
			pk1 = dummy1.PK.ToGuid();
			pk2 = dummy2.PK.ToGuid();

			ediEntUrl = ShowEditFormUrlHandler.Instance.Create(DummyControllerIDs.Dummy, pk2);
		});

		var mockWindowsService = new Mock<IWindowService>();
		Func<string, Task<bool>> callback = null;
		mockWindowsService.Setup(o => o.RegisterUrlHandlerAsync(It.Is<UrlHandlerInstanceInfo>(i => i.LicenceKeyIdentifier == expectedLicenseKey), It.IsAny<Func<string, Task<bool>>>()))
			.Callback<UrlHandlerInstanceInfo, Func<string, Task<bool>>>((i, c) => callback = c);

		var loadRequestSent = new TaskCompletionSource();
		string loadRequestUrl = null;
		mockWindowsService
			.Setup(o => o.RequestCreateAndShowWindowAsync(It.IsAny<CreateWindowOptions>(), It.IsAny<ShowWindowOptions>(), It.IsAny<WindowStyleOptions>()))
			.Callback<CreateWindowOptions, ShowWindowOptions, WindowStyleOptions>((createWindowOptions, showWindowOptions, windowStyleOptions) =>
			{
					loadRequestUrl = createWindowOptions.Uri.ToString();
					loadRequestSent.SetResult();
			});

		var uri = new UriBuilder(TestNavigationManager.BaseServerUri).Uri.ToString();
		var mainForm = ctx.RenderEntryPointComponent(uri, mockWindowsService.Object);
		mainForm.WaitForState(() => mainForm.Instance.Form != null);

		Assert.That(mainForm.Instance.Form, Is.TypeOf<MainForm>());
		mockWindowsService.Verify(o => o.RegisterUrlHandlerAsync(It.Is<UrlHandlerInstanceInfo>(i => i.LicenceKeyIdentifier == expectedLicenseKey), It.IsAny<Func<string, Task<bool>>>()), Times.Once);

		var callbackTask = callback(ediEntUrl);
		Assert.That(await loadRequestSent.Task.WithTimeout(TimeSpan.FromSeconds(1)), Is.True);

		using var ctx2 = new EnterpriseTestContext();
		var renderedNewForm = ctx2.RenderEntryPointComponent(loadRequestUrl);
		renderedNewForm.WaitForState(() => renderedNewForm.Instance.Form != null);
		Assert.That(((DummyBusinessObject)((ZDummyForm)renderedNewForm.Instance.Form).BusinessEntity).PK.ToGuid(), Is.EqualTo(pk2));
		await callbackTask;
	}

	[Test]
	public async Task DoesNotRegisterUrlHandlerForNonMainForm()
	{
		using var ctx = new EnterpriseTestContext();
		string expectedLicenseKey = null;
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			expectedLicenseKey = EnvProxy.Instance.CurrentCompany.LicenceKeyIdentifier;
		});

		var mockWindowsService = new Mock<IWindowService>();
		Func<string, Task<bool>> callback = null;
		mockWindowsService.Setup(o => o.RegisterUrlHandlerAsync(It.Is<UrlHandlerInstanceInfo>(i => i.LicenceKeyIdentifier == expectedLicenseKey), It.IsAny<Func<string, Task<bool>>>()))
			.Callback<UrlHandlerInstanceInfo, Func<string, Task<bool>>>((i, c) => callback = c);

		var nonMainForm = await ctx.RenderEntryPointComponent(() => new Form(), mockWindowsService.Object);
		nonMainForm.WaitForState(() => nonMainForm.Instance.Form != null);
		Assert.That(nonMainForm.Instance.Form, Is.Not.TypeOf<MainForm>());
		mockWindowsService.Verify(o => o.RegisterUrlHandlerAsync(It.Is<UrlHandlerInstanceInfo>(i => i.LicenceKeyIdentifier == expectedLicenseKey), It.IsAny<Func<string, Task<bool>>>()), Times.Never);
		Assert.That(callback, Is.Null);
	}

	[Test, WithSnapshotProtection]
	public async Task ExecuteUrlShouldHandleOrReportExceptionOnMainFormAsync()
	{
		await ReInitializeMainForm();

		using var ctx = new EnterpriseTestContext();
		var expectedLicenseKey = EnvProxy.Instance.CurrentCompany.LicenceKeyIdentifier;
		var mockWindowsService = new Mock<IWindowService>();
		Func<string, Task<bool>> callback = null;
		mockWindowsService.Setup(o => o.RegisterUrlHandlerAsync(It.Is<UrlHandlerInstanceInfo>(i => i.LicenceKeyIdentifier == expectedLicenseKey), It.IsAny<Func<string, Task<bool>>>()))
			.Callback<UrlHandlerInstanceInfo, Func<string, Task<bool>>>((i, c) => callback = c);

		var uri = new UriBuilder(TestNavigationManager.BaseServerUri).Uri.ToString();
		var mainForm = ctx.RenderEntryPointComponent(uri, mockWindowsService.Object);
		mainForm.WaitForState(() => mainForm.Instance.Form is MainForm);
		Assert.That(mainForm.Instance.Form, Is.TypeOf<MainForm>());

		var result = await callback(null);
		Assert.That(result, Is.False);
		Assert.That(ErrorReporter.LastExceptionReported, Is.Not.Null);
		ErrorReporter.Clear();
	}

	[Test, WithSnapshotProtection]
	public async Task ExecuteUrlShouldNotThrowEnterpriseUrlHandlerExceptionOnMainFormAsync()
	{
		await ReInitializeMainForm();

		using var ctx = new EnterpriseTestContext();
		var expectedLicenseKey = EnvProxy.Instance.CurrentCompany.LicenceKeyIdentifier;
		var mockWindowsService = new Mock<IWindowService>();
		Func<string, Task<bool>> callback = null;
		mockWindowsService.Setup(o => o.RegisterUrlHandlerAsync(It.Is<UrlHandlerInstanceInfo>(i => i.LicenceKeyIdentifier == expectedLicenseKey), It.IsAny<Func<string, Task<bool>>>()))
			.Callback<UrlHandlerInstanceInfo, Func<string, Task<bool>>>((i, c) => callback = c);

		var uri = new UriBuilder(TestNavigationManager.BaseServerUri).Uri.ToString();
		var mainForm = ctx.RenderEntryPointComponent(uri, mockWindowsService.Object);
		mainForm.WaitForState(() => mainForm.Instance.Form != null);
		Assert.That(mainForm.Instance.Form, Is.TypeOf<MainForm>());

		string ediEntUrl = null;
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			ediEntUrl = ShowEditFormUrlHandler.Instance.Create(DummyControllerIDs.Dummy, new Guid());
		});

		mainForm.WaitForState(() => callback != null);
		mockWindowsService.Verify(o => o.RegisterUrlHandlerAsync(It.Is<UrlHandlerInstanceInfo>(i => i.LicenceKeyIdentifier == expectedLicenseKey), It.IsAny<Func<string, Task<bool>>>()), Times.Once);
		Assert.That(callback, Is.Not.Null, "Callback delegate is null");
		var result = await callback(ediEntUrl);
		Assert.That(result, Is.False);
		Assert.That(ErrorReporter.LastExceptionReported, Is.Null);
	}

	[Test]
	public async Task PreloadFormJSInterop()
	{
		using var ctx = new EnterpriseTestContext();
		var services = ctx.MockCargoWiseClientServices;
		var interop = new Mock<IFormJSInterop>();
		interop.Setup(i => i.PreloadInterop());
		ctx.Services.AddScoped(_ => interop.Object);

		var uri = new UriBuilder(TestNavigationManager.BaseServerUri).Uri.ToString();
		var rendered = await ctx.RenderEntryPointComponent(() => new Form(), ctx.DefaultClientServices.WindowService);
		interop.Verify(e => e.PreloadInterop(), Times.Once());
	}

	[Test, WithPlaywrightPage]
	public async Task PrefetchCommonUsedJs()
	{
		var waitFormJs = Page.WaitForResponseAsync(r => r.Url.Contains("form.js"), new PageWaitForResponseOptions() { Timeout = 5000 });
		var waitOverlayJs = Page.WaitForResponseAsync(r => r.Url.Contains("overlay.js"), new PageWaitForResponseOptions() { Timeout = 5000 });
		var waitClientEventServiceJs = Page.WaitForResponseAsync(r => r.Url.Contains("clientEventService.js"), new PageWaitForResponseOptions() { Timeout = 5000 });
		var waitRichTextBoxJs = Page.WaitForResponseAsync(r => r.Url.Contains("richTextBox.js"), new PageWaitForResponseOptions() { Timeout = 5000 });
		var waitTextBoxJs = Page.WaitForResponseAsync(r => r.Url.Contains("textbox.js"), new PageWaitForResponseOptions() { Timeout = 5000 });
		var waitGridJs = Page.WaitForResponseAsync(r => r.Url.Contains("grid.js"), new PageWaitForResponseOptions() { Timeout = 5000 });
		await using var ctx = new InMemoryAppServerTestContext();
		await ctx.LoadFormAsync(() =>
		{
			var form = new Form { Width = 500, Height = 500 };
			return form;
		});
		await Task.WhenAll(waitFormJs, waitOverlayJs, waitClientEventServiceJs, waitGridJs, waitRichTextBoxJs, waitTextBoxJs);
	}

	[Test, WithPlaywrightPage]
	public async Task TestUserMonitoringPostEvent()
	{
		var mockMonitoringServerURL = "http://localhost:8000";
		var mockMonitoringServerURI = new Uri(mockMonitoringServerURL);
		var rumEndpointPath = "/intake/v2/rum/events";
		var builder = WebApplication.CreateBuilder();
		builder.WebHost.UseUrls(mockMonitoringServerURL);
		using var app = builder.Build();
		var rumEventSent = new TaskCompletionSource<(string, string)>();
		app.MapPost(rumEndpointPath, async (context) =>
		{
			context.Response.StatusCode = 202;
			using (var reader = new StreamReader(new GZipStream(context.Request.Body, CompressionMode.Decompress)))
			{
				var rumMetadata = await reader.ReadLineAsync();
				var rumTransactiondata = await reader.ReadLineAsync();
				rumEventSent.SetResult((rumMetadata, rumTransactiondata));
			}
		});
		await app.StartAsync();

		var userMonitorRegistry = new Mock<UserMonitorRegistry>(Mock.Of<ILogger<UserMonitorRegistry>>(), Mock.Of<IProtectedDataServiceFactory>(), Mock.Of<ISqlConnectionProvider>());
		userMonitorRegistry.SetupGet(m => m.MonitoringEnabled).Returns(true);
		userMonitorRegistry.SetupGet(m => m.MonitoringURI).Returns(mockMonitoringServerURI);

		var mockServiceProvider = new Mock<ICargoWiseClientServiceProvider>();
		mockServiceProvider.Setup((m) => m.AddCargoWiseClient(It.IsAny<IServiceCollection>()))
			.Callback<IServiceCollection>((services) =>
			{
				services.AddSingleton((_) => userMonitorRegistry.Object);
				services.AddSingleton((_) => Mock.Of<ILifecycleService>());
				services.AddSingleton((_) => Mock.Of<IWindowService>());
			});
		await using var ctx = new InMemoryAppServerTestContext(mockServiceProvider.Object);
		await ctx.LoadFormAsync(() => new Form { Text = "Form Title" });
		var (rumMetadata, rumTransactiondata) = await rumEventSent.Task;
		Assert.That(rumMetadata, Contains.Substring("CargoWise Winzor AppServer"));
		Assert.That(rumTransactiondata, Contains.Substring("appServerProcessCorrelationId"));
		await app.StopAsync();
	}

	[Test, WithPlaywrightPage]
	public async Task TestUserMonitoringPostEventFilterDisconnectErrors()
	{
		var mockMonitoringServerURL = "http://localhost:8000";
		var mockMonitoringServerURI = new Uri(mockMonitoringServerURL);
		var rumEndpointPath = "/intake/v2/rum/events";
		var builder = WebApplication.CreateBuilder();
		builder.WebHost.UseUrls(mockMonitoringServerURL);
		using var app = builder.Build();
		var rumEventSent = new TaskCompletionSource<(string, string)>();
		app.MapPost(rumEndpointPath, async (context) =>
		{
			context.Response.StatusCode = 202;
			using (var reader = new StreamReader(new GZipStream(context.Request.Body, CompressionMode.Decompress)))
			{
				var rumMetadata = await reader.ReadLineAsync();
				var rumTransactiondata = await reader.ReadLineAsync();
				rumEventSent.SetResult((rumMetadata, rumTransactiondata));
			}
		});
		await app.StartAsync();

		var userMonitorRegistry = new Mock<UserMonitorRegistry>(Mock.Of<ILogger<UserMonitorRegistry>>(), Mock.Of<IProtectedDataServiceFactory>(), Mock.Of<ISqlConnectionProvider>());
		userMonitorRegistry.SetupGet(m => m.MonitoringEnabled).Returns(true);
		userMonitorRegistry.SetupGet(m => m.MonitoringURI).Returns(mockMonitoringServerURI);

		var mockServiceProvider = new Mock<ICargoWiseClientServiceProvider>();
		mockServiceProvider.Setup((m) => m.AddCargoWiseClient(It.IsAny<IServiceCollection>()))
			.Callback<IServiceCollection>((services) =>
			{
				services.AddSingleton((_) => userMonitorRegistry.Object);
				services.AddSingleton((_) => Mock.Of<ILifecycleService>());
				services.AddSingleton((_) => Mock.Of<IWindowService>());
			});
		await using var ctx = new InMemoryAppServerTestContext(mockServiceProvider.Object);
		var page = await ctx.LoadFormAsync(() => new Form());
		var errorMessage = "No interop methods are registered for renderer 1";
		var script = $"setTimeout(() => {{ throw new Error('{errorMessage}') }});";

		await page.EvaluateAsync(script);

		var (rumMetadata, rumTransactiondata) = await rumEventSent.Task;
		Assert.That(rumMetadata, Contains.Substring("CargoWise Winzor AppServer"));
		if (rumTransactiondata != null)
		{
			Assert.That(rumTransactiondata, Does.Not.Contain(errorMessage));
		}
		await app.StopAsync();
		WithPlaywrightPageAttribute.Context.PageErrors.Clear();
	}

	[Test]
	public async Task RequestShowWindowAsyncNotFiredWhenClientWindowOpenSent([Values] bool clientWindowOpenSent)
	{
		using var ctx = new EnterpriseTestContext();
		var initializeCallback = new TaskCompletionSource();

		var windowService = new Mock<IWindowService>();
		windowService.Setup(o => o.RequestShowWindowAsync(It.IsAny<ShowWindowOptions>(), It.IsAny<WindowStyleOptions>())).Callback(() => initializeCallback.SetResult());

		var entryPointComponent = await ctx.RenderEntryPointComponent(() => new Form() { ClientWindowOpenSent = clientWindowOpenSent }, windowService.Object);
		entryPointComponent.WaitForState(() => entryPointComponent.Instance.Form != null);

		Assert.That(() => initializeCallback.Task.IsCompleted, Is.Not.EqualTo(clientWindowOpenSent).After(2000));
	}

	[Test]
	public async Task InvokeRequestShowWindowAsyncOnceWhenRenderForm()
	{
		var entryPointPoolOptions = new EntryPointPoolOptions { MinimumPoolSize = 5 };
		using var ctx = new EnterpriseTestContext().ConfigureEntryPointPool(entryPointPoolOptions);
		var initializeCallback = new TaskCompletionSource();

		var windowService = new Mock<IWindowService>();
		windowService.Setup(o => o.RequestShowWindowAsync(It.IsAny<ShowWindowOptions>(), It.IsAny<WindowStyleOptions>())).Callback(() => initializeCallback.SetResult());

		var entryPointComponent = await ctx.RenderEntryPointComponent(() => new Form(), windowService.Object, true);
		entryPointComponent.WaitForState(() => entryPointComponent.Instance.Form != null);
		windowService.Verify(w => w.RequestShowWindowAsync(It.IsAny<ShowWindowOptions>(), It.IsAny<WindowStyleOptions>()), Times.Once);
	}

	[Test, WithPlaywrightPage]
	public async Task SetWinzorWindowIdCorrectly()
	{
		var guid = Guid.NewGuid();
		await BrowserContext.SetExtraHTTPHeadersAsync(new List<KeyValuePair<string, string>> { new(RequestHeaders.ClientAppHostFormId, guid.ToString()) });

		await using var ctx = new InMemoryAppServerTestContext();

		Form form = null;
		await ctx.LoadFormAsync(() =>
		{
			form = new Form { Width = 500, Height = 500 };
			return form;
		});

		Assert.That(form.WinzorWindowId, Is.EqualTo(guid));
	}

	[Test, WithPlaywrightPage]
	public async Task SetWinzorClientIp()
	{
		Globals.WinzorClientIpAddress = null;
		var guid = Guid.NewGuid();

		await using var ctx = new InMemoryAppServerTestContext();

		Form form = null;
		await ctx.LoadFormAsync(() =>
		{
			form = new Form { Width = 500, Height = 500 };
			return form;
		});

		Assert.That(IPAddress.IsLoopback(Globals.WinzorClientIpAddress), "IP address should be set. In unit test environments we expect it to be a loopback address (but we don't care if it's IPv4 or IPv6)");
	}

	[Test]
	public async Task FirstLoadFormShouldCallWindowOpenRequestToFillPool()
	{
		var mockCargoWiseClientServiceProvider = new MockCargoWiseClientSeviceProvider();
		var mockWindowService = mockCargoWiseClientServiceProvider.MockWindowService;
		var entryPointPoolOptions = new EntryPointPoolOptions { MinimumPoolSize = 5 };
		using var ctx = new EnterpriseTestContext().ConfigureEntryPointPool(entryPointPoolOptions);

		var entryPointComponent = await ctx.RenderEntryPointComponent(() => new Form { Width = 500, Height = 500 }, mockWindowService.Object);
		entryPointComponent.WaitForState(() => entryPointComponent.Instance.Form != null);

		mockWindowService.Verify(o => o.RequestCreateHiddenWindowAsync(It.IsAny<CreateWindowOptions>()), Times.Exactly((int)entryPointPoolOptions.MinimumPoolSize));
		mockWindowService.Verify(o => o.RequestShowWindowAsync(It.IsAny<ShowWindowOptions>(), It.IsAny<WindowStyleOptions>()), Times.Exactly(1));
	}

	[Test]
	[TestCase("https://test.wisecloud.com/pool/", true)]
	[TestCase("https://test.wisecloud.com/", false)]
	public void IsPooledWindow_ShouldReturnExpectedResult(string uri, bool expectedResult)
	{
		using var ctx = new EnterpriseTestContext();
		ctx.Services.AddSingleton<IOpeningFormQueue>(new OpeningFormQueue());
		ctx.Services.AddSingleton<EntryPointPool>();

		var testNavigationManager = new TestNavigationManager(uri);
		var entryPointComponent = new EntryPointComponent
		{
			NavigationManager = testNavigationManager,
			EntryPointPool = ctx.Services.GetRequiredService<EntryPointPool>()
		};

		var result = entryPointComponent.IsPooledWindow;

		Assert.That(expectedResult, Is.EqualTo(result));
	}

	[Test]
	public async Task RemoveFormFromRegisteredFormInstancesWhileRenderingPooledWindow()
	{
		using var ctx = new EnterpriseTestContext();
		var queue = new OpeningFormQueue();
		ctx.Services.AddSingleton<IOpeningFormQueue>(queue);

		var form = default(Form);
		var uri = default(Uri);
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			form = new Form { Width = 500, Height = 500 };
			queue.Write(form);
			uri = ctx.WinzorDispatcher.FormInstanceRegister.Add(TestNavigationManager.BaseServerUri, form);
		});

		Assert.That(form, Is.Not.Null);
		Assert.That(ReferenceEquals(ctx.WinzorDispatcher.FormInstanceRegister.Lookup(uri), form), Is.True);

		const string url = "https://test.wisecloud.com/pool/123456";
		ctx.RenderEntryPointComponent(url);

		Assert.That(ctx.WinzorDispatcher.FormInstanceRegister.ClaimFormInstance(uri), Is.Null);
	}

	public class EntryPointComponentWithoutOnAfterRenderAsync : EntryPointComponent
	{
		protected override Task OnAfterRenderAsync(bool firstRender)
		{
			return Task.CompletedTask;
		}
	}

	async Task ReInitializeMainForm()
	{
		await EnterpriseTestSetup.WinzorDispatcher.InvokeAsync(Initialization.UnconfigureCargoWise);

		var serviceProvider = InitializationConfigurationHelper.ConfigTestServices(new CargoWiseAuthStateProvider(NullLogger<CargoWiseAuthStateProvider>.Instance, It.IsAny<ITokenValidatorWrapper>()), null);

		Initialization.ConfigureCargoWise(
			EnterpriseTestSetup.WinzorDispatcher,
			serviceProvider.GetRequiredService<IWinzorCargoWiseLoginHandler>(),
			serviceProvider.GetRequiredService<UserMonitorRegistry>());
	}
}
