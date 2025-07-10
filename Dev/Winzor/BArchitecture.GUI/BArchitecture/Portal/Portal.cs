using Microsoft.AspNetCore.Components;

namespace WinzorFramework;

public sealed class Portal : ComponentBase, IAsyncDisposable
{
	[CascadingParameter]
	public IPortalProvider? Provider { get; set; }

	[Parameter]
	public RenderFragment? ChildContent { get; set; }

	protected override async Task OnParametersSetAsync()
	{
		if (ChildContent is null)
		{
			Provider?.RemovePortalContent(portalId);
		}
		else
		{
			Provider?.AddOrUpdatePortalContent(portalId, ChildContent);
		}
		await base.OnParametersSetAsync();
	}

	public ValueTask DisposeAsync()
	{
		Provider?.RemovePortalContent(portalId);
		return ValueTask.CompletedTask;
	}

	readonly Guid portalId = Guid.NewGuid();
}
