using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bunit;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using WinzorTestFramework;

namespace Enterprise.Winzor.Architecture.Test;

internal class MenuItemToToolStripItemConverterTest
{
	static EventHandler[] ClickHandlers => new[]
	{
		new EventHandler((s, e) => { }),
		null
	};

	[TestCaseSource(nameof(ClickHandlers))]
	public async Task SplitButtonShouldShowDropDownMenu_IfClickIsNotHandled(EventHandler handler)
	{
		var menuDisplayer = new TestMenuDisplayer();
		var clientServices = MockCargoWiseClientServices.MakeMock(menuDisplayer: menuDisplayer);
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var legacyMenuItem = new MenuItem("Test");

			if (handler != null)
			{
				legacyMenuItem.Click += handler;
			}

			legacyMenuItem.MenuItems.Add(new MenuItem("One"));
			legacyMenuItem.MenuItems.Add(new MenuItem("Two"));
			legacyMenuItem.MenuItems.Add(new MenuItem("Three"));

			var toolStrip = new ToolStrip();

			var splitButton = MenuItemToToolStripItemConverter.ConvertMenuItemToToolStripItem(legacyMenuItem, null, true);

			toolStrip.Items.Add(splitButton);
			form.Controls.Add(toolStrip);

			return form;
		}, clientServices);
		await rendered.Find(".splitbutton__button").TriggerEventAsync("onclick", new WebMouseEventArgs { Button = 1 });
		Assert.That(menuDisplayer.Menu, handler == null ? Is.Not.EqualTo(null) : Is.EqualTo(null));
		await rendered.Find(".splitbutton__dropdownbutton").TriggerEventAsync("onmousedown", new WebMouseEventArgs());
		Assert.That(menuDisplayer.Menu, Is.Not.EqualTo(null));
	}
}
