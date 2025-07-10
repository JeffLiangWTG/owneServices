using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using WinzorTestFramework;

namespace System.Windows.Forms;
class ContextMenuStripTest
{
	[Test]
	public async Task ContextMenuStripDisposesItemsAsync()
	{
		// Arrange
		using var ctx = new WinzorTestContext();
		ToolStripMenuItem item1 = null;
		ToolStripMenuItem item2 = null;
		Form form = null;
		ContextMenuStrip menuStrip = null;
		using var container = new ComponentModel.Container();

		var rendered = await ctx.RenderFormAsync(() =>
		{
			form = new Form();
			menuStrip = new ContextMenuStrip(container);
			item1 = new ToolStripMenuItem();
			item2 = new ToolStripMenuItem();
			menuStrip.Items.Add(item1);
			menuStrip.Items.Add(item2);
			form.ContextMenuStrip = menuStrip;
			return form;
		});

		// Precondition
		Assert.That(item1.IsDisposed, Is.False);
		Assert.That(item2.IsDisposed, Is.False);
		Assert.That(menuStrip.IsDisposed, Is.False);

		// Act
		await form.InvokeWinzorDispatcherAsync(container.Dispose);

		// Postcondition
		Assert.That(item1.IsDisposed, Is.True);
		Assert.That(item2.IsDisposed, Is.True);
		Assert.That(menuStrip.IsDisposed, Is.True);
	}

	[Test]
	public async Task NullContainerThrowsExceptionAsync()
	{
		using var ctx = new WinzorTestContext();

		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			Assert.Throws<ArgumentNullException>(() => new ContextMenuStrip(null));
		});
	}
}
