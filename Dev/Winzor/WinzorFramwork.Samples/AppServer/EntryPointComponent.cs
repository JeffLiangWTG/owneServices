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

namespace WinzorFramework.Samples;

public class EntryPointComponent : ComponentBase, IWinzorDispatcherContext, IRenderService
{
	[Inject]
	public required IFormOpener FormOpener { get; init; }

	[Inject]
	public required NavigationManager NavigationManager { get; set; }

	[Inject]
	public required WinzorDispatcher WinzorDispatcher { get; set; }

	[Inject]
	public required ILogger<EntryPointComponent> Logger { get; init; }

	[Inject]
	public required ILifecycleService ClientLifecycleService { get; set; }

	[Inject]
	public required IWindowService WindowService { get; set; }

	[Inject]
	public required IMenuDisplayer ClientMenuDisplayer { get; set; }

	[Inject]
	public required IJSRuntime JSRuntime { get; set; }

	[Inject]
	public required IFormJSInterop FormJSInterop { get; set; }

	[Inject]
	public required IFileService FileService { get; set; }

	[Inject]
	public required IClientEventService ClientEventService { get; set; }

	[Inject]
	public required IRemoteFileService RemoteFileService { get; set; }

	[Inject]
	public required IOIDCSettingsVerification OIDCSettingsVerification { get; set; }

	[Inject]
	public required IClientFileApi ClientFileApi { get; set; }

	/// <summary>
	/// Retrieved from the headers of the initial request from the client application
	/// Use to identify this window as a modal parent
	/// </summary>
	[CascadingParameter]
	public Guid? HostFormId { get; set; }

	public CargoWiseClientServices CargoWiseClientServices => Form?.CargoWiseClientServices;

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "<Pending>")]
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
		if (Interlocked.Exchange(ref readyMessageSent, 1) == 0)
		{
			await ClientLifecycleService.NotifyApplicationReadyAsync(TimeSpan.FromMinutes(5));
		}
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

		if (registeredFormInstance == null)
		{
			await ApplicationServer.MainFormInstanceInitializedTask.ContinueWith(t =>
			{
				registeredFormInstance = ApplicationServer.MainFormInstance;
			}, TaskScheduler.Default);
		}

		await WinzorDispatcher.InvokeAsync(() =>
		{
			using (WinzorDispatcher.WithContext(this))
			{
				if (Form == null && registeredFormInstance != null)
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
		if (Form != null && CargoWiseClientServices != null)
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

		Form?.InvokeRenderDispatcher(workItem);
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

	public bool ComponentResolved => Form is not null && Form.HasRendered || NotFound;

	readonly TaskCompletionSource componentResolvedTcs = new TaskCompletionSource();

	AutoResetEvent renderEvent;

	public Task LoadFormTask { get; private set; }

	public async Task WaitForAllRenderTasksAsync()
	{
		await (renderTasks?.WaitAllAsync() ?? Task.CompletedTask);
		renderTasks = null;
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

	RenderTasks renderTasks;

	// Using an int instead of a bool to make use of Interlocked operations
	// 0 = false
	// 1 = true
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021:Static Fields Are Thread Static Rule", Justification = "<Pending>")]
	static int readyMessageSent;

	internal static bool ReadyMessageSent
	{
		set
		{
			Interlocked.Exchange(ref readyMessageSent, value ? 1 : 0);
		}
	}
}
