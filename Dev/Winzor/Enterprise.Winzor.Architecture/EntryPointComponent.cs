using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Blazor.Client.Integration.Files;
using CargoWise.Blazor.Client.Integration.Menus;
using CargoWise.Blazor.Client.Integration.Messaging;
using CargoWise.Blazor.Client.Integration.OIDCVerify;
using CargoWise.Blazor.Common;
using CargoWise.Common;
using CargoWise.Winzor.Telemetry;
using Enterprise.URLHandler;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;
using WinzorFramework;
using WinzorFramework.Extensions;
using WinzorFramework.JSInterop;
using WTG.StaticAnalysis.Annotation;
using static WTG.StaticAnalysis.Annotation.ThreadSafeAttribute;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Enterprise.Winzor.Architecture;

public class EntryPointComponent : ComponentBase, IWinzorDispatcherContext, IRenderService
{
	[Inject]
	public IFormOpener FormOpener { get; init; }

	[Inject]
	public EntryPointPool EntryPointPool { get; init; }

	[Inject]
	public IOptions<EntryPointPoolOptions> EntryPointPoolOptions { get; init; }

	[Inject]
	public ILogger<EntryPointComponent> Logger { get; init; }

	[Inject]
	public NavigationManager NavigationManager { get; set; }

	[Inject]
	public WinzorDispatcher WinzorDispatcher { get; set; }

	[Inject]
	public ILifecycleService ClientLifecycleService { get; set; }

	[Inject]
	public IWindowService WindowService { get; set; }

	[Inject]
	public IMenuDisplayer ClientMenuDisplayer { get; set; }

	[Inject]
	public IJSRuntime JSRuntime { get; set; }

	[Inject]
	public IFormJSInterop FormJSInterop { get; set; }

	[Inject]
	public IFileService FileService { get; set; }

	[Inject]
	public IClientEventService ClientEventService { get; set; }

	[Inject]
	public IRemoteFileService RemoteFileService { get; set; }

	[Inject]
	public IWinzorTelemetry Telemetry { get; set; }

	[Inject]
	public IPingService PingService { get; set; }

	[Inject]
	public IOIDCSettingsVerification OIDCSettingsVerification { get; set; }

	[Inject]
	public IClientFileApi ClientFileApi { get; set; }

	/// <summary>
	/// Retrieved from the headers of the initial request from the client application
	/// Use to identify this window as a modal parent
	/// </summary>
	[CascadingParameter]
	public Guid? HostFormId { get; set; }

	public CargoWiseClientServices CargoWiseClientServices => Form.CargoWiseClientServices;

	new void StateHasChanged()
	{
		using var activity = Telemetry?.RenderingActivity?.StartActivity($"{nameof(EntryPointComponent)}.{nameof(StateHasChanged)}");
		base.StateHasChanged();
	}

	protected override void BuildRenderTree(RenderTreeBuilder builder)
	{
		using var activity = Telemetry?.RenderingActivity?.StartActivity($"{nameof(EntryPointComponent)}.{nameof(BuildRenderTree)}");
		if (IsDisposing)
		{
			return;
		}
		if (NotFound)
		{
			builder.OpenComponent<NotFoundComponent>(1);
			builder.CloseComponent();
		}
		else if (Form == null)
		{
			builder.AddContent(0, (NoResString)"Loading");
		}
		else
		{
			builder.OpenComponent<ControlProxyComponent>(1);
			builder.AddAttribute(2, (NoResString)"Control", Form);
			builder.CloseComponent();
		}
	}

	protected override Task OnAfterRenderAsync(bool firstRender)
	{
		renderEvent?.Set();
		return Task.CompletedTask;
	}

	protected override async Task OnInitializedAsync()
	{
		using var activity = Telemetry.LoadingActivity?.CreateActivity($"{nameof(EntryPointComponent)}.{nameof(OnInitializedAsync)}", ActivityKind.Server);

		if (!IsPooledWindow)
		{
			activity?.Start();
		}

		if (Interlocked.Exchange(ref readyMessageSent, 1) == 0)
		{
			await ClientLifecycleService.NotifyApplicationReadyAsync(TimeSpan.FromMinutes(5));
		}

		LoadFormTask = LoadFormAsync();
		FormJSInterop.PreloadInterop();
		await componentResolvedTcs.Task;

		activity?.AddTag((NoResString)"pooled", IsPooledWindow);
		activity?.AddTag((NoResString)"form", Form?.GetType().Name);

		if (IsPooledWindow)
		{
			activity?.Start();
		}

		if (IsDisposing)
		{
			await WindowService.RequestCloseAsync();
			return;
		}

		await WaitForAllRenderTasksAsync();

		if (!Form?.ClientWindowOpenSent ?? true)
		{
			InformReady();
		}

		StateHasChanged();
		await Initialization.OnInitialRenderAsync(this, Telemetry.LoadingActivity);
	}

	void InformReady()
	{
		if (Form is not null)
		{
			Form.ClientWindowOpenSent = true;
		}

		var showWindowOptions = form is not null ? form.GenerateShowWindowOptions() : new ShowWindowOptions();
		var windowStyleOptions = form is not null ? form.GenerateWindowStyleOptions() : new WindowStyleOptions();

#pragma warning disable VSTHRD110 // We don't await this call so that rendering can proceed while the form is opening
		ExceptionHandlerExtension.HandleJSExceptionAsync(async () => await WindowService.RequestShowWindowAsync(showWindowOptions, windowStyleOptions))
			.ContinueWith((t) =>
			{
				if (t.Exception is not null)
				{
					Logger.LogError(t.Exception, $"A request to initialize an {nameof(EntryPointComponent)}");
				}
			}, TaskScheduler.Default);
#pragma warning restore VSTHRD110
	}

	async Task LoadFormAsync()
	{
		if (IsPooledWindow)
		{
			await LoadFormFromQueueAsync();
		}
		else
		{
			await LoadFormFromRegisterAsync();
		}
	}

	async Task LoadFormFromQueueAsync()
	{
		var queuedForm = await EntryPointPool.EnterAsync(new Uri(NavigationManager.BaseUri), WindowService);
		using var activity = Telemetry.LoadingActivity?.StartActivity($"{nameof(EntryPointComponent)}.{nameof(LoadFormFromQueueAsync)}");
		WinzorDispatcher.FormInstanceRegister.Remove(queuedForm);
		await queuedForm.InvokeWinzorDispatcherAsync(() => Form = queuedForm);
	}

	async Task LoadFormFromRegisterAsync()
	{
		using var activity = Telemetry.LoadingActivity?.StartActivity($"{nameof(EntryPointComponent)}.{nameof(LoadFormFromRegisterAsync)}");
		// When pooling is enabled the first request is not served from the pool
		// This call fills the pool
		if (EntryPointPoolOptions.Value.Enabled)
		{
			EntryPointPool.CheckPool(new Uri(NavigationManager.BaseUri), WindowService);
		}

		Form registeredFormInstance = null;

		try
		{
			registeredFormInstance = WinzorDispatcher.FormInstanceRegister.ClaimFormInstance(new Uri(NavigationManager.Uri));
			if (registeredFormInstance != null && registeredFormInstance.WinzorDispatcher != WinzorDispatcher)
			{
				WinzorDispatcher = registeredFormInstance.WinzorDispatcher;
			}
		}
		catch (Exception ex)
		{
			await WinzorDispatcher.InvokeAsync(() => ExceptionReporter.Instance.HandleUnhandledException(ex));
			return;
		}

		if (registeredFormInstance == null)
		{
			if (!Initialization.MainFormInstanceInitializedTask.IsCompleted)
			{
				InitializeFormBeforeMainFormInitialized();
				await WinzorDispatcher.InvokeAsync(() => Form = Initialization.SplashFormInstance);
				return;
			}

			if (Initialization.MainFormInstanceInitializedTask.IsFaulted)
			{
				await CallShutdownApplicationAsync();
				await WinzorDispatcher.InvokeAsync(() => Form = Initialization.SplashFormInstance);
				return;
			}
		}

		if (registeredFormInstance != null && registeredFormInstance.IsDisposed)
		{
			Form = registeredFormInstance;
			return;
		}

		var loadForm = Telemetry.LoadingActivity.WithTracing(() =>
		{
			try
			{
				using (WinzorDispatcher.WithContext(this))
				{
					Form = registeredFormInstance;
					if (Form == null)
					{
						CreateForm();
					}
					if (Form == null)
					{
						NotFound = true;
					}
				}
			}
			catch (Exception ex)
			{
				ExceptionReporter.Instance.HandleUnhandledException(ex);
			}
		}, $"{nameof(EntryPointComponent)}.LoadFormTask");

		await WinzorDispatcher.InvokeAsync(loadForm);
	}

	void CreateForm()
	{
		using var activity = Telemetry.LoadingActivity?.StartActivity($"{nameof(EntryPointComponent)}.{nameof(CreateForm)}");
		var uri = new Uri(NavigationManager.Uri);
		var uriQueryString = uri.Query.TrimStart('?');

		if (string.IsNullOrEmpty(uriQueryString))
		{
			Initialization.MainFormInstance.Show();
			Initialization.MainFormInstance.RegisterAfterRenderAction(PingService.Start);
			Initialization.SplashFormInstance.Close();
		}
		else
		{
			var queryString = new QueryString(uriQueryString);
			try
			{
				foreach (var urlHandler in EnterpriseUrlHandlerService.UrlHandlers)
				{
					if (urlHandler.CanHandle(queryString))
					{
						if (urlHandler.Handle(queryString))
						{
							return;
						}
					}
				}
			}
			catch (EnterpriseUrlHandlerException)
			{
			}
		}
	}

	void InitializeFormBeforeMainFormInitialized()
	{
		_ = Initialization.MainFormInstanceInitializedTask.ContinueWith(async t =>
		{
			using var activity = Telemetry.LoadingActivity?.StartActivity($"{nameof(EntryPointComponent)}.MainFormInitializeTask");
			ResetRenderedForm();
			await LoadFormAsync();
			await InvokeAsync(StateHasChanged);
			InformReady();
		}, TaskScheduler.Default);
	}

	async Task CallShutdownApplicationAsync()
	{
		await JSRuntime.InvokeVoidAsync("window.cargoWiseClient.shutDownApplication");
	}

	OpenFormAction IWinzorDispatcherContext.OpenForm(Form form)
	{
		using var activity = Telemetry.LoadingActivity?.StartActivity($"{nameof(EntryPointComponent)}.OpenForm");
		if (Form != null)
		{
			var uri = WinzorDispatcher.FormInstanceRegister.Add(CargoWiseClientServices.ServerBaseUri, form, out var isNewForm, out _);

			if (componentResolvedTcs.Task.IsCompleted)
			{
				Form.InvokeRenderDispatcher(() => FormOpener.OpenFormAsync(uri, WindowService, form));
			}
			else
			{
				Form.RegisterAfterRenderAction(async () =>
				{
					await FormOpener.OpenFormAsync(uri, WindowService, form);
				});
				componentResolvedTcs.TrySetResult();
			}
		}
		else
		{
			Form = form;
		}
		return OpenFormAction.None;
	}

	public void RegisterRenderTask(Task task)
	{
		renderTasks ??= new RenderTasks();
		renderTasks.Add(task);
	}

	void IWinzorDispatcherContext.NotifyRenderRequired(Control control)
	{
		if (control.HasRendered)
		{
			control.ReadyToRender();
			_ = control.InvokeStateHasChangedAsync();
		}
	}

	void IWinzorDispatcherContext.InvokeRenderDispatcher(Func<Task> workItem)
	{
		if (!componentResolvedTcs.Task.IsCompleted)
		{
			throw new InvalidOperationException("No render invocations expected for a form without a render handle");
		}

		Form.InvokeRenderDispatcher(workItem);
	}

	void IWinzorDispatcherContext.OnEnterMessageLoop()
	{
		form?.ReadyToRender();
		RegisterRenderTask(InvokeAsync(StateHasChanged));
	}

	void IRenderService.ReplaceRenderedForm(Form form)
	{
		using (WinzorDispatcher.Current.WithContext(this))
		{
			ResetRenderedForm();
			Form = form;
			RegisterRenderTask(InvokeAsync(StateHasChanged));
		}
	}

	void ResetRenderedForm()
	{
		Form = null;
		renderEvent = new AutoResetEvent(initialState: false);
		RegisterRenderTask(InvokeAsync(StateHasChanged));
		renderEvent.WaitOne();
		renderEvent = null;
	}

	public Form Form
	{
		get => form;
		private set
		{
			form = value;
			if (form != null)
			{
				form.ReadyToRender(new CargoWiseClientServices(
					new Uri(NavigationManager.BaseUri),
					ClientLifecycleService,
					WindowService,
					ClientMenuDisplayer,
					JSRuntime,
					FileService,
					ClientEventService,
					OIDCSettingsVerification,
					ClientFileApi)
				{
					RemoteFileService = RemoteFileService,
					RenderService = this,
				},
					HostFormId); // When the HostFormId is not set it means we are in a browser and the WindowId is redundant
				componentResolvedTcs.TrySetResult();
			}
		}
	}
	Form form;

	bool NotFound
	{
		get => notFound;
		set
		{
			notFound = value;
			if (notFound)
			{
				componentResolvedTcs.TrySetResult();
			}
		}
	}
	bool notFound;

	bool IsDisposing => Form != null && Form.IsDisposed;

	public bool ComponentResolved => (Form is not null && Form.HasRendered) || NotFound;

	readonly TaskCompletionSource componentResolvedTcs = new TaskCompletionSource();

	AutoResetEvent renderEvent;

	public Task LoadFormTask { get; private set; }

	public async Task WaitForAllRenderTasksAsync()
	{
		if (renderTasks is not null && renderTasks.Any())
		{
			using var activity = Telemetry.RenderingActivity?.StartActivity($"{nameof(EntryPointComponent)}.{nameof(WaitForAllRenderTasksAsync)}");
			await renderTasks.WaitAllAsync();
		}
	}

	internal bool IsPooledWindow => new Uri(NavigationManager.Uri).PathAndQuery.StartsWith(EntryPointPool.EntryPointPoolPath);

	RenderTasks renderTasks;

	[ThreadSafe(Mechanism.Interlocked)]
	// Using an int instead of a bool to make use of Interlocked operations
	// 0 = false
	// 1 = true
	static int readyMessageSent;

	internal static bool ReadyMessageSent
	{
		set
		{
			Interlocked.Exchange(ref readyMessageSent, value ? 1 : 0);
		}
	}
}
