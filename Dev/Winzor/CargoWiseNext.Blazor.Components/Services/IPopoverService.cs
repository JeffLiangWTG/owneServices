namespace CargoWiseNext.Blazor.Components;

public interface IPopoverService
{
	Task ShowAsync(string? id);
	Task HideAsync(string? id);
	Task ToggleAsync(string? id);
	Task InitAsync(string? id);
}
