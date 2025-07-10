namespace CargoWiseNext.Blazor.Components;

public class PopoverService : IPopoverService
{
	readonly PopoverJsInterop _jsInterop;

	public PopoverService(PopoverJsInterop jsInterop)
	{
		_jsInterop = jsInterop;
	}

	public async Task ShowAsync(string? id)
	{
		if (!string.IsNullOrEmpty(id))
		{
			await _jsInterop.ShowAsync(id);
		}
	}

	public async Task HideAsync(string? id)
	{
		if (!string.IsNullOrEmpty(id))
		{
			await _jsInterop.HideAsync(id);
		}
	}

	public async Task ToggleAsync(string? id)
	{
		if (!string.IsNullOrEmpty(id))
		{
			await _jsInterop.ToggleAsync(id);
		}
	}

	public async Task InitAsync(string? id)
	{
		if (!string.IsNullOrEmpty(id))
		{
			await _jsInterop.InitAsync(id);
		}
	}
}
