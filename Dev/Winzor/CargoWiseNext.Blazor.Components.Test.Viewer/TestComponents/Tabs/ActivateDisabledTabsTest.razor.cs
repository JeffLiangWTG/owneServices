using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Components;

namespace CargoWiseNext.Blazor.Components.Test.TestComponents.Tabs;

public partial class ActivateDisabledTabsTest
{
	public static string __description__ = "activating tabs should work";
#pragma warning disable IDE0044
	CwnTabs _tabElement = null!;
#pragma warning restore IDE0044 // Add readonly modifier

	[Parameter]
	public int InitialStartIndex { get; set; } = 0;

	public List<TabBindingHelper> Tabs { get; } = [];

	public ActivateDisabledTabsTest()
	{
		for (int i = 0; i < 5; i++)
		{
			Tabs.Add(new TabBindingHelper { Name = (i + 1).ToString(), Disabled = true, Tag = new DummyPlaceHolder { Id = Guid.NewGuid() }, Index = i, Content = $"Tab Content {i + 1}" });
		}
	}

	public void ActivateAll()
	{
		InvokeAsync(() =>
		{
			foreach (var item in Tabs)
			{
				item.Disabled = false;
			}

			StateHasChanged();
		});
	}

	protected override void OnParametersSet()
	{
		base.OnParametersSet();

		Tabs[InitialStartIndex].Disabled = false;
	}

	public void EnableTab(int index)
	{
		InvokeAsync(() =>
		{
			Tabs[index].Disabled = false;
			StateHasChanged();
		});
	}

	public void ActivateTab(int index, bool ignoreState)
	{
		InvokeAsync(() =>
		{
			_tabElement.ActivatePanel(index, ignoreState);
		});
	}

	public void ActivateTab(int index)
	{
		InvokeAsync(() =>
		{
			_tabElement.ActivatePanel(index);
		});
	}

	public void DisableTab(int index)
	{
		InvokeAsync(() =>
		{
			Tabs[index].Disabled = true;
			StateHasChanged();
		});
	}

	public void ActivateTab(CwnTabPanel panel, bool ignoreState)
	{
		InvokeAsync(() =>
		{
			_tabElement.ActivatePanel(panel, ignoreState);
		});
	}

	public void ActivateTab(CwnTabPanel panel)
	{
		InvokeAsync(() =>
		{
			_tabElement.ActivatePanel(panel);
		});
	}

	public void ActivateTab(DummyPlaceHolder id, bool ignoreState)
	{
		InvokeAsync(() =>
		{
			_tabElement.ActivatePanel(id, ignoreState);
		});
	}

	public void ActivateTab(DummyPlaceHolder id)
	{
		InvokeAsync(() =>
		{
			_tabElement.ActivatePanel(id);
		});
	}

	public class TabBindingHelper
	{
		public required string Name { get; set; }

		public required bool Disabled { get; set; }

		public required DummyPlaceHolder Tag { get; set; }

		public CwnTabPanel Panel { get; set; } = null!;

		public required int Index { get; set; }

		public required string Content { get; set; }
	}

	public class DummyPlaceHolder
	{
		public Guid Id { get; set; }
	}
}

