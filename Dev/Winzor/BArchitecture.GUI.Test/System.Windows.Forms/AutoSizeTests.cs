using System.Threading.Tasks;
using NUnit.Framework;
using WinzorTestFramework;

namespace System.Windows.Forms;

public class AutoSizeTests
{
	[Test]
	public async Task AutoSizeGrows()
	{
		using var ctx = new WinzorTestContext();
		Button button = null;
		await ctx.RenderControlOnFormAsync(() => button = new Button { AutoSize = true, Width = 1, Text = "This is a test" });
		Assert.That(button.Width, Is.GreaterThan(1));
	}

	[Test]
	public async Task AutoSizeDefaultsToGrowOnly()
	{
		using var ctx = new WinzorTestContext();
		Button button = null;
		await ctx.RenderControlOnFormAsync(() => button = new Button { AutoSize = true, Width = 1000, Text = "This is a test" });
		Assert.That(button.Width, Is.EqualTo(1000));
	}

	[Test]
	public async Task AutoSizeGrowAndShrinkGrowsAndShrinks()
	{
		using var ctx = new WinzorTestContext();
		Button button = null;
		Button button2 = null;
		await ctx.RenderControlOnFormAsync(() => button = new Button { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Width = 1, Text = "This is a test" });
		await ctx.RenderControlOnFormAsync(() => button2 = new Button { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Width = 1000, Text = "This is a test" });
		Assert.That(button.Width, Is.GreaterThan(1));
		Assert.That(button2.Width, Is.LessThan(1000));
	}

	[Test]
	public async Task AutoSizeGrowAndShrinkGrowsWhenTextChanged()
	{
		using var ctx = new WinzorTestContext();
		Button button = null;
		await ctx.RenderControlOnFormAsync(() => button = new Button { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Width = 1, Text = "" });
		Assume.That(button.Width, Is.EqualTo(6));

		button.FindForm().Invoke(() => button.Text = "A long string");

		Assert.That(button.Width, Is.GreaterThan(0));
	}

	[Test]
	public async Task AutoSizeFalseControlsNeitherGrowsNorShrink()
	{
		using var ctx = new WinzorTestContext();
		Button button = null;
		Button button2 = null;
		await ctx.RenderControlOnFormAsync(() => button = new Button { AutoSize = false, Width = 1, Text = "This is a test" });
		await ctx.RenderControlOnFormAsync(() => button2 = new Button { AutoSize = false, Width = 1000, Text = "This is a test" });
		Assert.That(button.Width, Is.EqualTo(1));
		Assert.That(button2.Width, Is.EqualTo(1000));
	}
}
