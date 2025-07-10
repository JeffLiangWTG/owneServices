using Microsoft.AspNetCore.Components;

namespace System.Windows.Forms;

[Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Referenced in TabSlider.razor")]
public partial class TabSlider : ComponentBase
{
	[Parameter] public TabControl? TabControl { get; set; }

	public const int SLIDER_WIDTH = 32;

	async Task ShiftLeftAsync()
	{
		if (TabControl is not null && TabControl.StartingIndexOfVisibleTabs > 0)
		{
			TabControl.StartingIndexOfVisibleTabs--;
			await TabControl.InvokeStateHasChangedAsync();
		}
	}

	async Task ShiftRightAsync()
	{
		if (TabControl is not null)
		{
			int lastCompletelyVisibleTab = TabControl.StartingIndexOfVisibleTabs + TabControl.NumberOfVisibleTabs;
			if (TabControl.LastTabPartiallyVisible)
			{
				lastCompletelyVisibleTab--;
			}
			if (lastCompletelyVisibleTab < TabControl.TabCount)
			{
				TabControl.StartingIndexOfVisibleTabs++;
				await TabControl.InvokeStateHasChangedAsync();
			}
		}
	}
}
