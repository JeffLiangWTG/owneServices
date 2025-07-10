using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using WinzorFramework;
using WinzorFramework.JSInterop;

#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits

namespace WinzorTestFramework;

public class WinzorTestContext : TestContext
{
	internal WinzorTestContext()
		: this(ConfigTestServices().GetRequiredService<WinzorDispatcher>())
	{
		ownsWinzorDispatcher = true;
	}

	internal WinzorTestContext(IFormOpener formOpener, IFormInstanceRegister formInstanceRegister)
		: this(new WinzorDispatcher(formOpener, formInstanceRegister))
	{
		ownsWinzorDispatcher = true;
	}

	static ServiceProvider ConfigTestServices()
	{
		var serviceCollection = new ServiceCollection()
			.AddLogging()
			.AddSingleton<WinzorDispatcher>()
			.AddSingleton<IFormInstanceRegister, RegisteredFormInstances>()
			.AddSingleton<IFormOpener, FormOpener>();

		return serviceCollection.BuildServiceProvider();
	}

	protected WinzorTestContext(WinzorDispatcher dispatcher)
	{
		WinzorDispatcher = dispatcher;
		JSInterop.Mode = JSRuntimeMode.Loose;
		MockCargoWiseClientServices = new MockCargoWiseClientServices(this);
		HookExceptionEvents();
		Services.AddSingleton<IFileVersionHash, DummyFileVersionHash>();
		Services.AddScoped<IJSRuntimeWithMonitor, JSRuntimeWithMonitor>();
		Services.AddSingleton(WinzorDispatcher.FormInstanceRegister);
		Services.AddSingleton(WinzorDispatcher.FormOpener);
		RegisterJSInteropServices();
	}

	public async Task<IRenderedComponent<ControlProxyComponent>> RenderFormAsync<T>()
		where T : Form, new()
	{
		return await RenderFormAsync(() => new T());
	}

	public async Task<IRenderedComponent<ControlProxyComponent>> RenderFormAsync(Func<Form> constructor, OpenFormAction openFormAction = OpenFormAction.None, bool clientWindowOpenSent = true)
	{
		return await RenderFormAsync(constructor, DefaultClientServices, openFormAction, clientWindowOpenSent);
	}

	public async Task<IRenderedComponent<ControlProxyComponent>> RenderFormAsync(Func<Form> constructor, CargoWiseClientServices clientServices, OpenFormAction openFormAction = OpenFormAction.None, bool clientWindowOpenSent = true)
	{
		Form form = null;
		var dispatcherContext = new DispatcherContext(this, clientServices, openFormAction);
		await WinzorDispatcher.InvokeAsync(() =>
		{
			form = constructor();
			Using(form);
			form.ClientWindowOpenSent = clientWindowOpenSent;
			using (WinzorDispatcher.WithContext(dispatcherContext))
			{
				form.Show();
			}
		});

		// Wait for a dummy message to run to ensure that the Form.Show
		// event that was queued in the message loop has been processed
		await WinzorDispatcher.InvokeAsync(() => { });
		return dispatcherContext.Rendered;
	}

	public async Task<(IRenderedComponent<ControlProxyComponent>, T)> RenderControlOnFormAsync<T>() where T : Control, new()
	{
		T control = null;
		var rendered = await RenderControlOnFormAsync(() => control = new T());
		return (rendered, control);
	}

	public async Task<IRenderedComponent<ControlProxyComponent>> RenderControlOnFormAsync<T>(Func<T> constructor, bool clientWindowOpenSent = true) where T : Control
	{
		return await RenderFormAsync(() =>
		{
#pragma warning disable CW1109 // Do Not Use System.Windows.Forms. Form Or KForm Class
			var form = new Form() { ClientWindowOpenSent = clientWindowOpenSent };
#pragma warning restore CW1109 // Do Not Use System.Windows.Forms. Form Or KForm Class
			var control = constructor();
			form.Controls.Add(control);
			return form;
		});
	}

	public async Task<IRenderedComponent<ControlProxyComponent>> ShowFormWithoutRenderAsync(Func<Form> constructor, CargoWiseClientServices clientServices, OpenFormAction openFormAction = OpenFormAction.None)
	{
		Form form = null;
		var dispatcherContext = new DispatcherWithoutRenderedContext(this, clientServices, openFormAction);
		await WinzorDispatcher.InvokeAsync(() =>
		{
			form = constructor();
			Using(form);
			using (WinzorDispatcher.WithContext(dispatcherContext))
			{
				form.Show();
			}
		});
		return dispatcherContext.Rendered;
	}

	public async Task<IRenderedComponent<ControlProxyComponent>> ShowFormWithoutRenderAsync(Func<Form> constructor, OpenFormAction openFormAction = OpenFormAction.None)
		=> await ShowFormWithoutRenderAsync(constructor, DefaultClientServices, openFormAction);

	public async Task<IRenderedComponent<ControlProxyComponent>> ShowFormInTestContextAsync(Action showForm)
	{
		var dispatcherContext = new DispatcherContext(this, DefaultClientServices, OpenFormAction.None);
		await WinzorDispatcher.InvokeAsync(() =>
		{
			using (WinzorDispatcher.WithContext(dispatcherContext))
			{
				showForm();
			}
		});
		return dispatcherContext.Rendered;
	}

	public MockCargoWiseClientServices MockCargoWiseClientServices;

	public CargoWiseClientServices DefaultClientServices => new CargoWiseClientServices(
		TestNavigationManager.BaseServerUri,
		MockCargoWiseClientServices.LifecycleService.Object,
		MockCargoWiseClientServices.WindowService.Object,
		MockCargoWiseClientServices.MenuDisplayer.Object,
		JSInterop.JSRuntime,
		MockCargoWiseClientServices.FileService.Object,
		MockCargoWiseClientServices.ClientEventService.Object,
		MockCargoWiseClientServices.OIDCSettingsVerification.Object,
		MockCargoWiseClientServices.ClientFileApi.Object)
	{
		RemoteFileService = MockCargoWiseClientServices.RemoteFileService.Object,
	};

	public delegate bool ExceptionRaisedEventHandler(Exception exception);

	public event ExceptionRaisedEventHandler ThreadExceptionExceptionRaised;

	public event ExceptionRaisedEventHandler DeveloperExceptionRaised;

	public event ExceptionRaisedEventHandler UnhandledExceptionRaised;

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1067:Application ThreadException Rule", Justification = "<Pending>")]
	protected virtual void HookExceptionEvents()
	{
		WinzorDispatcher.InvokeAsync(() =>
		{
			Application.ThreadException += Application_ThreadException;
			Application.DeveloperException += Application_DeveloperException;
			Application.UnhandledException += Application_UnhandledException;
		}).GetAwaiter().GetResult();
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1067:Application ThreadException Rule", Justification = "<Pending>")]
	protected virtual void UnHookExceptionEvents()
	{
		WinzorDispatcher.InvokeAsync(() =>
		{
			Application.ThreadException -= Application_ThreadException;
			Application.DeveloperException -= Application_DeveloperException;
			Application.UnhandledException -= Application_UnhandledException;
		}).GetAwaiter().GetResult();
	}

	protected virtual void RegisterJSInteropServices()
	{
		Services.AddJSInteropServices();
	}

	void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
	{
		if (ThreadExceptionExceptionRaised == null || !ThreadExceptionExceptionRaised.Invoke(e.Exception))
		{
			exceptions.Add(e.Exception);
		}
	}

	void Application_DeveloperException(object sender, string message, Exception ex)
	{
		if (DeveloperExceptionRaised == null || !DeveloperExceptionRaised.Invoke(ex))
		{
			exceptions.Add(new InvalidOperationException("DeveloperException: " + message, ex));
		}
	}

	void Application_UnhandledException(object sender, UnhandledExceptionEventArgs ex)
	{
		var exception = ex.ExceptionObject as Exception;
		if (UnhandledExceptionRaised == null || !UnhandledExceptionRaised.Invoke(exception))
		{
			exceptions.Add(new InvalidOperationException("UnhandledException: " + exception.Message, exception));
		}
	}

	public T Using<T>(T disposable) where T : IDisposable
	{
		disposables.Add(disposable);
		return disposable;
	}

	protected override void Dispose(bool disposing)
	{
		try
		{
			if (Renderer.UnhandledException.IsCompleted)
			{
				throw new TargetInvocationException("Unhandled exception thrown during rendering", Renderer.UnhandledException.Result);
			}
		}
		finally
		{
			RenderedForms.Clear();
			DisposeComponents();

			base.Dispose(disposing);

			WinzorDispatcher.InvokeAsync(() =>
			{
				foreach (var disposable in disposables)
				{
					disposable.Dispose();
				}
			}).GetAwaiter().GetResult();

			UnHookExceptionEvents();

			if (ownsWinzorDispatcher)
			{
				WinzorDispatcher.Dispose();
			}

			if (exceptions.Count > 0)
			{
				throw new AggregateException("One or more exceptions where raised during the test", exceptions);
			}
		}
	}

	public WinzorDispatcher WinzorDispatcher { get; }
	readonly bool ownsWinzorDispatcher;
	readonly List<IDisposable> disposables = new List<IDisposable>();
	readonly List<Exception> exceptions = new List<Exception>();
	protected internal readonly List<Form> RenderedForms = new List<Form>();

	internal class DispatcherContext : IWinzorDispatcherContext
	{
		public DispatcherContext(WinzorTestContext testContext, CargoWiseClientServices cargoWiseClientServices, OpenFormAction openFormAction, Form contextForm = null)
		{
			this.testContext = testContext;
			this.openFormAction = openFormAction;
			this.cargoWiseClientServices = cargoWiseClientServices;
			this.OpenFormTcs = new TaskCompletionSource();
			Form = contextForm;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "<Pending>")]
		public virtual OpenFormAction OpenForm(Form form)
		{
			form.ReadyToRender(cargoWiseClientServices, Guid.NewGuid());
			if (form.IsHandleCreated) // Don't try and render if the form has been detached from renderer
			{
				Task.Run(() =>
				{
					Rendered = testContext.RenderComponent<ControlProxyComponent>(ComponentParameter.CreateParameter("Control", form));
				}).GetAwaiter().GetResult();
				this.OpenFormTcs.SetResult();
				testContext.RenderedForms.Add(form);
			}
			return openFormAction;
		}

		public void RegisterRenderTask(Task task)
		{
		}

		public void NotifyRenderRequired(Control control)
		{
			if (OpenFormTcs.Task.IsCompleted)
			{
				control.ReadyToRender(); // need to trigger OnBeforeRender
				RegisterRenderTask(control.InvokeStateHasChangedAsync());
			}
		}

		public void OnEnterMessageLoop()
		{
		}

		public void InvokeRenderDispatcher(Func<Task> workItem)
		{
			workItem().GetAwaiter().GetResult();
		}

		public IRenderedComponent<ControlProxyComponent> Rendered { get; private set; }

		internal TaskCompletionSource OpenFormTcs { get; }

		readonly CargoWiseClientServices cargoWiseClientServices;
		public CargoWiseClientServices CargoWiseClientServices => cargoWiseClientServices;
		public Form Form { get; }

		readonly WinzorTestContext testContext;
		internal readonly OpenFormAction openFormAction;
	}

	internal class DispatcherWithoutRenderedContext : DispatcherContext
	{
		public DispatcherWithoutRenderedContext(WinzorTestContext testContext, CargoWiseClientServices cargoWiseClientServices, OpenFormAction openFormAction) : base(testContext, cargoWiseClientServices, openFormAction)
		{
		}

		public override OpenFormAction OpenForm(Form form)
		{
			form.ReadyToRender(base.CargoWiseClientServices, Guid.NewGuid());
			if (form.IsHandleCreated)
			{
				this.OpenFormTcs.SetResult();
			}
			return base.openFormAction;
		}
	}
}
