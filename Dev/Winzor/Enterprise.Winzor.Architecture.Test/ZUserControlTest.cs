using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;
using Microsoft.Playwright;
using NUnit.Framework;
using WTG.PlaywrightTesting;

namespace Enterprise.Winzor.Architecture.Test;

using static PlaywrightTestContext;
internal class ZUserControlTest
{
	[Test, WithPlaywrightPage]
	public async Task ZUserControlShouldBeSelectableByTabKey()
	{
		await using var ctx = new InMemoryAppServerTestContext();

		Form form;
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form();
			var button1 = new ZButton() { Name = "ButtonOne", Text = "button_one", Location = new Point(20, 100) };
			var linkLabelUserControl1 = new LinkLabelUserControlForTest() { Name = "TestUserControl", Location = new Point(20, 150) };
			var button2 = new ZButton() { Name = "ButtonTwo", Text = "button_two", Location = new Point(20, 200) };
			form.Controls.Add(button1);
			form.Controls.Add(linkLabelUserControl1);
			form.Controls.Add(button2);
			return form;
		});

		await Assertions.Expect(page.Locator("button[title='button_one']")).ToBeFocusedAsync();

		await page.Keyboard.PressAsync("Tab");
		var testUserControl = page.Locator("[data-name='TestUserControl']");
		await Assertions.Expect(testUserControl).ToBeFocusedAsync();

		await page.Keyboard.PressAsync("Tab");
		await Assertions.Expect(page.Locator("button[title='button_two']")).ToBeFocusedAsync();

		await page.Keyboard.PressAsync("Shift+Tab");
		await Assertions.Expect(testUserControl).ToBeFocusedAsync();
	}

	internal class LinkLabelUserControlForTest : ZUserControl
	{
		readonly ZLinkLabel zLinkLabel1;

		public LinkLabelUserControlForTest()
		{
			zLinkLabel1 = new ZLinkLabel();
			zLinkLabel1.Text = "Linnie has a interesting link here!";
			zLinkLabel1.Links.Add(6, 15, "ExampleData");
			zLinkLabel1.LinkArea = new LinkArea(38, 45);
			Controls.Add(zLinkLabel1);
		}
	}

	[Test, WithPlaywrightPage]
	public async Task SelectOnlyOneReadOnlyControlInSplitContainer()
	{
		await using var ctx = new InMemoryAppServerTestContext();

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form { Width = 1000, Height = 600 };
			var userControl = new ZUserControl();
			var splitContainer = new SplitContainer() { Name = "SplitContainer" };

			var dataGrid = new DataGrid();
			dataGrid.ReadOnly = true;
			splitContainer.Panel1.Controls.Add(dataGrid);

			userControl.Controls.Add(splitContainer);
			form.Controls.Add(userControl);
			return form;
		});

		await Assertions.Expect(page.Locator(".datagrid")).ToBeFocusedAsync();
	}
}
