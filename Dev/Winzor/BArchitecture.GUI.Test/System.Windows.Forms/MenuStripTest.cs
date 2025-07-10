using System.Threading.Tasks;
using NUnit.Framework;
using WinzorTestFramework;

namespace System.Windows.Forms;

sealed class MenuStripTest
{
	[Test]
	public async Task DefaultItemIsToolStripMenuItem()
	{
		using var ctx = new WinzorTestContext();

		ToolStripItem item = null;
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var menu = new MenuStrip();
			item = menu.Items.Add("A New Item");
		});
		Assert.That(item, Is.TypeOf<ToolStripMenuItem>());
		Assert.That(item.Text, Is.EqualTo("A New Item"));
	}

	[Test]
	public async Task DefaultSeparatorItemIsToolStripSeparator()
	{
		using var ctx = new WinzorTestContext();

		ToolStripItem item = null;
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var menu = new MenuStrip();
			item = menu.Items.Add("-");
		});
		Assert.That(item, Is.TypeOf<ToolStripSeparator>());
	}
}
