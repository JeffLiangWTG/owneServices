using Microsoft.AspNetCore.Components;

namespace WinzorFramework;

public sealed partial class PortalOutlet : IDisposable
{
	[CascadingParameter]
	public IPortalProvider? Provider { get; set; }

	protected override Task OnInitializedAsync()
	{
		if (Provider is not null)
		{
			Provider.PortalChanged += PortalChanged;
		}
		return base.OnInitializedAsync();
	}

	public void Dispose()
	{
		if (Provider is not null)
		{
			Provider.PortalChanged -= PortalChanged;
		}
	}

	void PortalChanged(object? sender, EventArgs e)
	{
		StateHasChanged();
	}
}
