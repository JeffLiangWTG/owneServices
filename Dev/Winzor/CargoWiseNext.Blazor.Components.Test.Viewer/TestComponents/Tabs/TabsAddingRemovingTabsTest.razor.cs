using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CargoWiseNext.Blazor.Components.Test.TestComponents.Tabs;

public partial class TabsAddingRemovingTabsTest
{
	public static string __description__ = "Adding and removing tabs should work";

	readonly List<string> _tabs = new List<string>();
	int i;

	public CwnTabs Tabs { get; set; } = null!;

	async Task AddTab()
	{
		_tabs.Add($"Tab {i++}");
		await InvokeAsync(StateHasChanged);
	}

	async Task RemoveLast()
	{
		if (_tabs.Any())
		{
			_tabs.RemoveAt(_tabs.Count - 1);
			await InvokeAsync(StateHasChanged);
		}
	}
}
