using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Blazor.Client.Integration.Files;
using CargoWise.Blazor.Client.Integration.Menus;
using CargoWise.Blazor.Client.Integration.Messaging;
using CargoWise.Blazor.Client.Integration.OIDCVerify;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using WinzorFramework;
using WinzorFramework.Extensions;
using WinzorFramework.JSInterop;
using FormBorderStyle = System.Windows.Forms.FormBorderStyle;
using FormStartPosition = System.Windows.Forms.FormStartPosition;
using IntegrationFormBorderStyle = CargoWise.Blazor.Client.Integration.Messaging.FormBorderStyle;
using IntegrationFormStartPosition = CargoWise.Blazor.Client.Integration.Messaging.FormStartPosition;

namespace WinzorTestFramework.TestServer;

public class EntryPointComponent : ComponentBase, IWinzorDispatcherContext
{
	[Inject]
	public NavigationManager NavigationManager { get; set; }

	[Inject]
	public WinzorDispatcher WinzorDispatcher { get; set; }

	[Inject]
	public ILogger<EntryPointComponent> Logger { get; init; }

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
	public IOIDCSettingsVerification OIDCSettingsVerification { get; set; }

	[Inject]
	public IClientFileApi ClientFileApi { get; set; }

	public CargoWiseClientServices CargoWiseClientServices => Form.CargoWiseClientServices;

	protected override void BuildRenderTree(RenderTreeBuilder builder)
	{
		if (NotFound)
		{
			builder.OpenComponent<NotFoundComponent>(1);
			builder.CloseComponent();
		}
		else if (Form == null)
		{
			builder.AddContent(0, "Loading");
		}
		else
		{
			builder.OpenComponent<ControlProxyComponent>(1);
			builder.AddAttribute(2, "Control", Form);
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
		LoadFormTask = LoadFormAsync();
		await componentResolvedTcs.Task;

		await WaitForAllRenderTasksAsync();
		if (!Form?.ClientWindowOpenSent ?? true)
		{
			await ExceptionHandlerExtension.HandleJSExceptionAsync(() =>
			{
				InformReady();
				return Task.CompletedTask;
			});
		}
		StateHasChanged();
		FormJSInterop.PreloadInterop();
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
		Form registeredFormInstance = null;
		if (WinzorDispatcher.FormInstanceRegister is RegisteredFormInstances formInstanceRegister)
		{
			registeredFormInstance = formInstanceRegister.ClaimFormInstance(new Uri(NavigationManager.Uri));
		}

		await WinzorDispatcher.InvokeAsync(() =>
		{
			using (WinzorDispatcher.WithContext(this))
			{
				if (Form == null)
				{
					Form = registeredFormInstance;
				}
				if (Form == null)
				{
					NotFound = true;
				}
			}
		});
	}

	OpenFormAction IWinzorDispatcherContext.OpenForm(Form form)
	{
		Form = form;
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

	public Form Form
	{
		get => form;
		private set
		{
			form = value;
			if (form != null)
			{
				form.ReadyToRender(
					new CargoWiseClientServices(
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
				});
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

	public bool ComponentResolved => Form is not null && Form.HasRendered || NotFound;

	readonly TaskCompletionSource componentResolvedTcs = new TaskCompletionSource();

	readonly AutoResetEvent renderEvent;

	public Task LoadFormTask { get; private set; }

	public async Task WaitForAllRenderTasksAsync()
	{
		await (renderTasks?.WaitAllAsync() ?? Task.CompletedTask);
		renderTasks = null;
	}

	RenderTasks renderTasks;
}
