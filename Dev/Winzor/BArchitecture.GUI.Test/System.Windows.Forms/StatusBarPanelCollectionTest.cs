using System.Threading.Tasks;
using NUnit.Framework;
using WinzorTestFramework;

namespace System.Windows.Forms;
class StatusBarPanelCollectionTest
{
	[Test]
	public async Task ContainsKey()
	{
		using var ctx = new WinzorTestContext();

		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var statusBar = new StatusBar();
			var panel = new StatusBarPanel { Name = "panel1" };
			statusBar.Panels.Add(panel);
			return statusBar;
		});

		var statusBarRendered = rendered.GetControl<StatusBar>();

		Assert.That(statusBarRendered.Panels.ContainsKey("panel1"), Is.True);
		Assert.That(statusBarRendered.Panels.ContainsKey("panel2"), Is.False);
	}

	[Test]
	public async Task IndexOfKey()
	{
		using var ctx = new WinzorTestContext();

		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var statusBar = new StatusBar();
			var panel = new StatusBarPanel { Name = "panel1" };
			statusBar.Panels.Add(panel);
			return statusBar;
		});

		var statusBarRendered = rendered.GetControl<StatusBar>();
		var index = statusBarRendered.Panels.IndexOfKey("panel1");

		Assert.That(index, Is.EqualTo(0));
	}

	[Test]
	public async Task RemoveByKey()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var statusBar = new StatusBar();
			var panel = new StatusBarPanel { Name = "panel1" };
			statusBar.Panels.Add(panel);
			return statusBar;
		});

		var statusBarRendered = rendered.GetControl<StatusBar>();

		statusBarRendered.Panels.RemoveByKey("panel1");

		Assert.That(statusBarRendered.Panels.ContainsKey("panel1"), Is.False);
		Assert.That(statusBarRendered.Panels.Count, Is.EqualTo(0));
	}
}
