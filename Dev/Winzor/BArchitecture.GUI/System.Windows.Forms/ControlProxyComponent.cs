using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using WinzorFramework;
using WinzorFramework.Extensions;
using WinzorFramework.Telemetry;

namespace System.Windows.Forms;

public class ControlProxyComponent : ComponentBase, IAsyncDisposable
{
	[Parameter]
	public Control? Control { get; set; }

	[Parameter]
	public string? Style { get; set; }

	[Inject]
	public IServiceProvider? ServiceProvider { get; set; }

	string ControlType => Control is null ? string.Empty : Control.GetType().Name;

	public override async Task SetParametersAsync(ParameterView parameters)
	{
		foreach (var parameter in parameters)
		{
			switch (parameter.Name)
			{
				case nameof(Control):
					Control = (Control)parameter.Value;
					if (Control != null)
					{
						Control.Proxy = this;
					}
					break;
				case nameof(Style):
					Style = (string)parameter.Value;
					break;
				default:
					throw new InvalidOperationException($"Unknown parameter: {parameter.Name}");
			}
		}

		// base.SetParametersAsync as this will call StateHasChanged() in parallel with OnInitializedAsync() when it does not immediatly run to completion
		// this will cause undesired race conditions in winzor, so instead of calling base.SetParametersAsync, implement the required logic below
		if (!initialized)
		{
			initialized = true;
			OnInitialized();
			await OnInitializedAsync();
		}

		// base.SetParametersAsync would also call OnParametersSet/OnParametersSetAsync here, but we are currently not using this, so leave out for now
		StateHasChanged();
	}

	bool initialized;

	public new async Task InvokeAsync(Func<Task> workItem)
	{
		using var activity = TelemetryService.ActivitySource?.StartActivity($"{ControlType}Proxy.InvokeAsync");
		await base.InvokeAsync(workItem);
	}

	public Task InvokeStateHasChangedAsync()
	{
		return InvokeAsync(StateHasChanged);
	}

	public new void StateHasChanged()
	{
		base.StateHasChanged();
	}

	protected override void BuildRenderTree(RenderTreeBuilder builder)
	{
		if (Control is not null)
		{
			if (Control.RenderInPortal)
			{
				builder.OpenComponent<Portal>(0);
				builder.AddAttribute(1, "ChildContent", Control.RenderFragment);
				builder.CloseComponent();
			}
			else
			{
				builder.AddContent(0, Control.RenderFragment);
			}
		}
	}

	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		await (Control?.WithExceptionHandlingAsync(async () =>
		{
			await (Control?.OnAfterRenderAsync(firstRender) ?? Task.CompletedTask);
			await ((Control as IHandleAfterRender)?.OnAfterRenderAsync() ?? Task.CompletedTask);
			await base.OnAfterRenderAsync(firstRender);
		}) ?? Task.CompletedTask);
	}

	protected override async Task OnInitializedAsync()
	{
		await (Control?.WithExceptionHandlingAsync(async () =>
		{
			await Control.OnInitializedAsync();
			await base.OnInitializedAsync();
		}) ?? Task.CompletedTask);
	}

	public async ValueTask DisposeAsync()
	{
		// The control may have been reassigned to a different ControlProxyComponent, in which case we should not detach it from the renderer
		if (Control?.Proxy == this)
		{
			Control.ElementReference = default;
			try
			{
				await ExceptionHandlerExtension.HandleJSExceptionAsync(Control.DetachFromRendererAsync);
			}
			finally
			{
				// Call DetachFromRendererAsync before we clear the Proxy value so that
				// the control still has access to the JSInterop services to perform cleanup
				Control.Proxy = null;
			}
		}
	}
}
