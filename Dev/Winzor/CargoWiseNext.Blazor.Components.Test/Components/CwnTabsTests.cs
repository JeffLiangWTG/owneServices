using Bunit;
using CargoWiseNext.Blazor.Components.Test.TestComponents.Tabs;

namespace CargoWiseNext.Blazor.Components.Test;

public class CwnTabsTests : BunitTestContext
{
	[Test]
	public void AddingAndRemovingTabPanels()
	{
		var comp = RenderComponent<TabsAddingRemovingTabsTest>();
		Assert.That(comp.Find("div.cwn-tabs__panels").InnerHtml.Trim(), Is.Empty);
		Assert.That(comp.FindAll("div.cwn-tab"), Is.Empty);
		Assert.That(comp.Instance.Tabs.Panels, Is.Not.Null);
		Assert.That(comp.Instance.Tabs.Panels, Is.Empty);

		// add a panel
		comp.FindAll("button")[0].Click();
		Assert.That(comp.Find("div.cwn-tabs__panels").InnerHtml.Trim(), Is.Not.Empty);
		Assert.That(comp.FindAll("div.cwn-tab").Count, Is.EqualTo(1));

		Assert.That(comp.Instance.Tabs.Panels, Is.Not.Null);
		Assert.That(comp.Instance.Tabs.Panels, Has.Count.EqualTo(1));
		Assert.That(comp.FindComponents<CwnTabPanel>().First().Instance, Is.EqualTo(comp.Instance.Tabs.Panels[0]));

		// add another
		comp.FindAll("button")[0].Click();
		Assert.That(comp.FindAll("div.cwn-tab"), Has.Count.EqualTo(2));

		Assert.That(comp.Instance.Tabs.Panels, Is.Not.Null);
		Assert.That(comp.Instance.Tabs.Panels, Has.Count.EqualTo(2));
		Assert.That(comp.FindComponents<CwnTabPanel>().ElementAt(0).Instance, Is.EqualTo(comp.Instance.Tabs.Panels[0]));
		Assert.That(comp.FindComponents<CwnTabPanel>().ElementAt(1).Instance, Is.EqualTo(comp.Instance.Tabs.Panels[1]));

		comp.FindAll("div.cwn-tab")[1].Click();
		comp.FindAll("button")[1].Click();
		Assert.That(comp.FindAll("div.cwn-tab"), Has.Count.EqualTo(1));

		Assert.That(comp.Instance.Tabs.Panels, Is.Not.Null);
		Assert.That(comp.Instance.Tabs.Panels, Has.Count.EqualTo(1));
		Assert.That(comp.FindComponents<CwnTabPanel>().ElementAt(0).Instance, Is.EqualTo(comp.Instance.Tabs.Panels[0]));

		comp.FindAll("button")[1].Click();
		Assert.That(comp.Find("div.cwn-tabs__panels").InnerHtml.Trim(), Is.Empty);
		Assert.That(comp.FindAll("div.cwn-tab"), Is.Empty);

		Assert.That(comp.Instance.Tabs.Panels, Is.Not.Null);
		Assert.That(comp.Instance.Tabs.Panels, Is.Empty);
	}

	[Test]
	public void TabHeaderClassPropagated()
	{
		var comp = RenderComponent<CwnTabs>();

		comp.SetParametersAndRender(builder => builder.Add(tabs => tabs.TabHeaderClass, "testA testB"));

		Assert.That(comp.Find(".cwn-tabs__bar").ClassList, Does.Contain("testA"));
		Assert.That(comp.Find(".cwn-tabs__bar").ClassList, Does.Contain("testB"));
	}

	[Test]
	public void ActivatePanels()
	{
		var activator = new Action<IRenderedComponent<ActivateDisabledTabsTest>, ActivateDisabledTabsTest.TabBindingHelper>[] {
			(x,y) => x.Instance.ActivateTab(y.Index),
			(x,y) => x.Instance.ActivateTab(y.Panel),
			(x,y) => x.Instance.ActivateTab(y.Tag),

			(x,y) => x.Instance.ActivateTab(y.Index, false),
			(x,y) => x.Instance.ActivateTab(y.Panel, false),
			(x,y) => x.Instance.ActivateTab(y.Tag, false),
		};

		foreach (var invoker in activator)
		{
			for (var i = 0; i < 2; i++)
			{
				var comp = RenderComponent<ActivateDisabledTabsTest>();

				if (i == 0)
				{
					comp.Instance.ActivateAll();
				}
				else
				{
					comp.Instance.EnableTab(0);
				}

				var panels = comp.FindAll(".test-panel-selector");
				var activePanels = comp.FindAll(".cwn-tab--active");

				//checking expected default values
				Assert.That(panels, Has.Count.EqualTo(5));
				Assert.That(activePanels, Has.Count.EqualTo(1));
				Assert.That(panels[0].ClassList.Contains("cwn-tab--active"), Is.True);

				for (var j = 1; j < comp.Instance.Tabs.Count; j++)
				{
					invoker(comp, comp.Instance.Tabs[j]);

					panels.Refresh();
					activePanels.Refresh();

					if (i == 0)
					{
						Assert.That(panels[j - 1].ClassList.Contains("cwn-tab--active"), Is.False);
						Assert.That(panels[j].ClassList.Contains("cwn-tab--active"), Is.True);
					}
					else
					{
						Assert.That(panels[0].ClassList.Contains("cwn-tab--active"), Is.True);
						Assert.That(panels[j].ClassList.Contains("cwn-tab--disabled"), Is.True);
					}
				}
			}
		}
	}

	[Test]
	public void ActivatePanels_EvenWhenDisabled()
	{
		var activator = new Action<IRenderedComponent<ActivateDisabledTabsTest>, ActivateDisabledTabsTest.TabBindingHelper>[] {
			(x,y) => x.Instance.ActivateTab(y.Index, true),
			(x,y) => x.Instance.ActivateTab(y.Panel, true),
			(x,y) => x.Instance.ActivateTab(y.Tag, true),
		};

		foreach (var invoker in activator)
		{
			var comp = RenderComponent<ActivateDisabledTabsTest>();

			var panels = comp.FindAll(".test-panel-selector");

			//checking expected default values
			Assert.That(panels, Has.Count.EqualTo(5));
			Assert.That(panels[0].ClassList.Contains("cwn-tab--active"), Is.True);

			for (var i = 1; i < comp.Instance.Tabs.Count; i++)
			{
				invoker(comp, comp.Instance.Tabs[i]);

				panels.Refresh();

				Assert.That(panels[i - 1].ClassList.Contains("cwn-tab--active"), Is.False);
				Assert.That(panels[i].ClassList.Contains("cwn-tab--active"), Is.True);
				Assert.That(panels[i].ClassList.Contains("cwn-tab--disabled"), Is.True);

				var contentElement = comp.Find(".test-active-panel");

				Assert.That(contentElement.TextContent, Is.EqualTo(comp.Instance.Tabs[i].Content));
			}
		}
	}

	[Test]
	public void SelectedIndex_Binding()
	{
		//starting with index 1:
		var comp = RenderComponent<SelectedIndexTabsTest>();
		Assert.That(comp.Instance.Tabs.ActivePanelIndex, Is.EqualTo(1));
		var panels = comp.FindAll(".cwn-tab");
		var activePanels = comp.FindAll(".cwn-tab--active");
		Assert.That(activePanels, Has.Count.EqualTo(1));
		Assert.That(panels[1].ClassList.Contains("cwn-tab--active"), Is.True);

		//starting with index 2:
		SelectedIndexTabsTest.SelectedTab = 2;
		comp = RenderComponent<SelectedIndexTabsTest>();
		Assert.That(comp.Instance.Tabs.ActivePanelIndex, Is.EqualTo(2));
		panels = comp.FindAll(".cwn-tab");
		activePanels = comp.FindAll(".cwn-tab--active");
		Assert.That(activePanels, Has.Count.EqualTo(1));
		Assert.That(panels[2].ClassList.Contains("cwn-tab--active"), Is.True);

		//starting with index 0:
		SelectedIndexTabsTest.SelectedTab = 0;
		comp = RenderComponent<SelectedIndexTabsTest>();
		Assert.That(comp.Instance.Tabs.ActivePanelIndex, Is.EqualTo(0));
		panels = comp.FindAll(".cwn-tab");
		activePanels = comp.FindAll(".cwn-tab--active");
		Assert.That(activePanels, Has.Count.EqualTo(1));
		Assert.That(panels[0].ClassList.Contains("cwn-tab--active"), Is.True);
	}

	[Test]
	public async Task DefaultValuesForHeadersAsync()
	{
		await using var tabs = new CwnTabs();

		Assert.That(tabs.HeaderPosition, Is.EqualTo(TabHeaderPosition.After));
		Assert.That(tabs.Header, Is.Null);

		Assert.That(tabs.TabPanelHeaderPosition, Is.EqualTo(TabHeaderPosition.After));
		Assert.That(tabs.TabPanelHeader, Is.Null);
	}

	[Test]
	public void HtmlTextTabs()
	{
		// get the tab panels, we must have 2 tabs, one with html text and one without
		var comp = RenderComponent<HtmlTextTabsTest>();
		Assert.That(comp.Markup, Does.Contain("<div class=\"cwn-tabs__panels cwn-tabs__scrollable\""));
		var panels = comp.FindAll(".cwn-tab");
		Assert.That(panels, Has.Count.EqualTo(2));

		// index 0 : html text "Hello <span>World</span>!"
		Assert.That(panels[0].InnerHtml, Is.EqualTo("Hello &lt;span&gt;World&lt;/span&gt;!"));
		Assert.That(panels[0].TextContent, Is.EqualTo("Hello <span>World</span>!"));

		// index 1 : simple text without html "Hello World!"
		Assert.That(panels[1].InnerHtml, Does.Contain("Hello World!"));
		Assert.That(panels[1].TextContent, Does.Contain("Hello World!"));
	}
}

