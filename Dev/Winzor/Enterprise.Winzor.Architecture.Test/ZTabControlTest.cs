using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using WinzorTestFramework;
using WTG.PlaywrightTesting;

namespace Enterprise.Winzor.Architecture.Test;
class ZTabControlTest
{
	[Test, WithPlaywrightPage]
	public async Task FormWithFixed3DBorder_ShouldNotAllowResizeGripper_OnZTabPageAutoSize()
	{
		await using var ctx = new InMemoryAppServerTestContext();

		var page = await ctx.LoadFormAsync(() =>
		{
			// An example of this Form's setting is RefZoneHeaderForm
			var form = new ZForm()
			{
				FormBorderStyle = FormBorderStyle.Fixed3D, // condition to prevent Size Gripper
				Width = 899, // has to be larger than MinimumAutoSizeWith + Left (835 + 4), otherwise get autosized
				Height = 547, // has to be larger than MinimumAutoSizeHeight + Top (400 + 24), otherwise get autosized
			};
			var tabControl = new ZTabControl()
			{
				Dock = DockStyle.Fill,
			};
			var tab1 = new ZTabPage() { Text = "tab0" };
			var tab2 = new ZTabPageAutoSize() { Text = "tab1" };
			tabControl.Controls.Add(tab1);
			tabControl.Controls.Add(tab2);
			tabControl.SelectedTab = tab1;
			form.Controls.Add(tabControl);
			return form;
		});

		// This resize is used to simiulate the behaviour of WZ
		// while running through WinzorDispatcher, somewhere the client size will be resized.
		// This line of code tries to replicate the behaviour, but will not be 1-to-1.
		// The point is to trigger the resize of the ZTabPageAutoSize
		await page.SetViewportSizeAsync(900, 600);
		await Task.Delay(500);

		await page.ClickAsync(".tabcontrol__button:nth-of-type(2)");
		await Task.Delay(500);

		var sizeGripper = page.Locator(".sizing-grip");
		Assert.That(await sizeGripper.CountAsync(), Is.EqualTo(0));
	}

	[Test, WithPlaywrightPage]
	public async Task MaximizedFormDoesNotChangeSizeDuringTabSwitch()
	{
		var expectedSize = new Size(3840, 2323);
		ZFormResizableByTabPage form = null;
		TabControl tabControl = null;

		await using var ctx = new InMemoryAppServerTestContext();

		var page = await ctx.LoadFormAsync(() =>
		{
			form = new ZFormResizableByTabPage() { WindowState = FormWindowState.Maximized, Size = expectedSize };
			form.Controls.Add(tabControl = new ZTabControl() { Size = expectedSize });
			var tabPage0 = new ZTabPageAutoSize() { Text = "Tab 0" };
			var tabPage1 = new ZTabPageAutoSize() { Text = "Tab 1" };
			var tabPage2 = new ZTabPageAutoSize() { Text = "Tab 2" };
			tabPage0.Controls.Add(new Label { Text = "Tab page 0 Is Visible!" });
			tabPage1.Controls.Add(new Label { Text = "Tab page 1 Is Visible!" });
			tabPage2.Controls.Add(new Label { Text = "Tab page 2 Is Visible!" });
			tabControl.TabPages.Add(tabPage0);
			tabControl.TabPages.Add(tabPage1);
			tabControl.TabPages.Add(tabPage2);
			tabControl.SelectedIndex = 0;
			return form;
		});

		await page.SetViewportSizeAsync(expectedSize.Width, expectedSize.Height);
		Assert.That(() => form, Is.Not.Null.After(5000, 100), "Form is null");
		Assert.That(() => tabControl.SelectedTab, Is.Not.Null.After(5000, 100), "SelectedTab is null");
		Assert.That(async () => (await page.QuerySelectorAllAsync(".tabcontrol__button")).Count, Is.EqualTo(3).After(5000, 100), "TabControl has incorrect number of tab buttons.");

		Assert.That(async () => await page.GetByText($"Tab page 0 Is Visible!").CountAsync(), Is.EqualTo(1).After(5000, 100), "Tab page 0 is not visible (before clicking tabs).");
		Assert.That(form.Size, Is.EqualTo(expectedSize));

		var tabsToClick = new int[] { 1, 2, 0, 2, 1 };
		for (var i = 0; i < tabsToClick.Length; i++)
		{
			var tab = tabsToClick[i];
			await page.GetByText($"Tab {tab}").ClickAsync();
			Assert.That(async () => await page.GetByText($"Tab page {tab} Is Visible!").CountAsync(), Is.EqualTo(1).After(5000, 100), $"Tab page {tab} is not visible (on click {i}).");

			Assert.That(form.Size, Is.EqualTo(expectedSize), $"Form size is incorrect (on click {i})");

			var formLocator = page.Locator(".form");
			await formLocator.WaitForAsync();

			Assert.That(async () => await (formLocator.GetComputedStyleAsync("width")), Is.EqualTo(expectedSize.Width + "px").After(5000, 100), $"Rendered form has incorrect width (on click {i})");
			Assert.That(async () => await (formLocator.GetComputedStyleAsync("height")), Is.EqualTo(expectedSize.Height + "px").After(5000, 100), $"Rendered form has incorrect height (on click {i})");
		}
	}

	[TestCase(3000, 3000, 3200, 3200)]
	[TestCase(3000, 3000, 2800, 2800)]
	public async Task SwitchingTabWindowDoesNotResize(int originalWidth, int originalHeight, int changedWidth, int changedHeight)
	{
		ZForm form = null;
		ZTabControl tabControl = null;
		ZTabPage detailsPage = null;
		ZTabPage workflowPage = null;
		using var ctx = new EnterpriseTestContext();

		var rendered = await ctx.RenderFormAsync(() =>
		{
			form = new ZForm();
			form.Size = new Size(originalWidth, originalHeight);
			tabControl = new ZTabControl();
			detailsPage = new ZTabPage();
			workflowPage = new ZTabPageAutoSize();

			tabControl.Controls.Add(detailsPage);
			tabControl.Controls.Add(workflowPage);
			tabControl.SelectedTab = detailsPage;
			form.Controls.Add(tabControl);
			form.FormBorderStyle = FormBorderStyle.FixedSingle;
			tabControl.SelectedTab = workflowPage;
			return form;
		});
		await form.OnBrowserSizeChangedAsync(changedWidth, changedHeight);
		await tabControl.InvokeWinzorDispatcherAsync(() => tabControl.SelectedTab = detailsPage);

		Assert.That(tabControl.Parent.ClientSize, Is.EqualTo(new Size(changedWidth, changedHeight)));
	}
}

#region Helper Classes

public class ZTabPageAutoSize : ZTabPage
{
	protected internal override bool IsAutoSized
	{
		get { return true; }
	}
}

public class ZFormResizableByTabPage : ZForm
{
	public override bool IsResizableByTabPageAllowed => true;
}

#endregion
