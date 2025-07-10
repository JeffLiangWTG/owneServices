using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Winzor.Architecture.Test;

sealed class TabConfigurationManagerTest
{
	[Test]
	public async Task TestTabConfigurationManagerDoesNotRefreshMenuItemGuid()
	{
		DummyForm form = null;
		var winzorControlGuid = Guid.Empty;
		var winzorControlGuidAfterRefresh = Guid.Empty;

		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() => form = new DummyForm());

		await form.InvokeWinzorDispatcherAsync(() =>
		{
			var manager = new TabConfigurationManager(form.MainMenu, form.MainTabControl);
			var viewMenu = form.MainMenu.MenuItems.FindByText("View");
			LoadMenuItems(manager);
			winzorControlGuid = viewMenu.MenuItems[0].WinzorControlGuid;
			LoadMenuItems(manager);
			winzorControlGuidAfterRefresh = viewMenu.MenuItems[0].WinzorControlGuid;
		});

		Assert.That(winzorControlGuid, Is.Not.EqualTo(Guid.Empty));
		Assert.That(winzorControlGuid, Is.EqualTo(winzorControlGuidAfterRefresh));
	}

	void LoadMenuItems(TabConfigurationManager manager)
	{
		typeof(TabConfigurationManager).InvokeMember("LoadMenuItems", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, manager, null);
	}
}

class DummyForm : ZForm, ITabVisibilityDeciderPersistence
{
	public new MainMenu MainMenu
	{
		get { return base.MainMenu; }
	}

	protected override void OnLoad(EventArgs e)
	{
		base.OnLoad(e);

		TabControl = new ZTabControl();
		Controls.Add(TabControl);

		var page = new ZTabPage();
		page.Name = "Test";
		page.Text = "Test Text";
		MainTabControl.TabPages.Add(page);
	}

	ZTabControl TabControl;

	public ZTabControl MainTabControl
	{
		get { return base.TopLevelTabControl; }
	}

	public bool HasTabVisiblePersisted { get; set; }

	public bool RetrieveTabPageVisible(ZTabPage page)
	{
		RetrieveTabPageVisibleCalled = true;
		return RetrieveTabPageVisibleOverride;
	}

	public bool RetrieveTabPageVisibleCalled;
	public bool RetrieveTabPageVisibleOverride;

	public void StoreTabVisible(ZTabPage page)
	{
		StoreTabVisibleCalled = true;
	}
	public bool StoreTabVisibleCalled;
}
