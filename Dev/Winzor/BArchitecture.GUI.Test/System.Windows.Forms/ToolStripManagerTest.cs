using System.Threading.Tasks;
using NUnit.Framework;
using WinzorTestFramework;

namespace System.Windows.Forms;

internal class ToolStripManagerTest
{
	[Test]
	public async Task TestProcessShortcutShouldMatchRootWindow()
	{
		using var ctx = new WinzorTestContext();
		bool formAMenuItemClicked = false;
		bool formBMenuItemClicked = false;
		var renderedA = await ctx.RenderFormAsync(() =>
		{
			var formA = new Form() { Text = "FormA" };
			AddToolStrip(formA, "Help", "New eRequest", Keys.F1, (s, e) => formAMenuItemClicked = true);
			return formA;
		});
		var renderedB = await ctx.RenderFormAsync(() =>
		{
			var formB = new Form() { Text = "FormB" };
			AddToolStrip(formB, "Help", "New eRequest", Keys.F1, (s, e) => formBMenuItemClicked = true);
			return formB;
		});
		Assert.That(Form.ActiveForm?.Text, Is.EqualTo("FormB"));

		bool result = false;
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var msg = new Message { Msg = Control.WM_KEYDOWN, WParam = (IntPtr)Keys.F1 };
			result = ToolStripManager.ProcessShortcut(ref msg, Keys.F1);
		});

		Assert.That(result, Is.True);
		Assert.That(formAMenuItemClicked, Is.False);
		Assert.That(formBMenuItemClicked, Is.True);
	}

	void AddToolStrip(Form form, string toolStripText, string toolStripMenuItemText, Keys shortcut, EventHandler onClick)
	{
		var toolStrip = new ToolStrip { Name = "TopToolStrip", Dock = DockStyle.Top };

		var menuOne = new ToolStripMenuItem(toolStripText) { Name = "MenuOne" };
		toolStrip.Items.Add(menuOne);

		var menuOneChild = new ToolStripMenuItem(toolStripMenuItemText) { Name = "MenuOneChild" };
		menuOneChild.ShortcutKeys = shortcut;
		menuOneChild.ShowShortcutKeys = true;
		menuOneChild.Click += onClick;
		menuOne.DropDownItems.Add(menuOneChild);

		form.Controls.Add(toolStrip);
	}
}
