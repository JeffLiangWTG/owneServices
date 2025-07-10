using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using WTG.PlaywrightTesting;

namespace Enterprise.Winzor.Architecture.Test;

using static PlaywrightTestContext;
internal class ZCollapsiblePanelTest
{
	[Test, WithPlaywrightPage]
	[TestCase(DockStyle.Top, true, 90 )]
	[TestCase(DockStyle.Top, false, -90)]
	[TestCase(DockStyle.Left, true, 90)]
	[TestCase(DockStyle.Left, false, -90)]
	[TestCase(DockStyle.Right, true, -90)]
	[TestCase(DockStyle.Right, false, 90)]
	public async Task SelectOnlyOneReadOnlyControlInSplitContainer(DockStyle dockStyle,bool isCollapsed, int rotation)
	{
		await using var ctx = new InMemoryAppServerTestContext();

		await ctx.LoadFormAsync(() =>
		{
			var form = new Form { Width = 1000, Height = 600 };
			var panel = new ZCollapsiblePanel() { Size = new Size(300, 200) };
			var splitter2 = new KSplitter() { Size = new Size(300, 200) };
			var label = new ZLabel();
			panel.Dock = dockStyle;
			panel.IsCollapsed = isCollapsed;
			panel.Text = "CollapsiblePanel";
			form.Controls.Add(panel);
			return form;
		});
		var dataGridElement = await Page.WaitForSelectorAsync("svg");
		Assert.That(await dataGridElement.GetAttributeAsync("transform") , Is.EqualTo($"rotate({rotation})"));
	}
}
