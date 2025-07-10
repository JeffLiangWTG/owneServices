using System.Threading.Tasks;
using System.Windows.Forms;
using NUnit.Framework;
using static Enterprise.ZArchitecture.GUI.StripControl;

namespace Enterprise.Winzor.Architecture.Test;
class ZFilterToolStripMenuItemTest
{
	[Test]
	public async Task OnImageMouseEnterShouldNotTriggerException()
	{
		using var ctx = new EnterpriseTestContext();

		ContextMenuStrip contextMenuStrip = null;

		var rendered = await ctx.RenderControlOnFormAsync(() => {
			var textBox = new TextBox();

			contextMenuStrip = new ContextMenuStrip();
			contextMenuStrip.Items.Add(new TestMenuItem("Parent", true, false) { Visible = true, Tag = textBox });

			return contextMenuStrip;
		});

		Assert.DoesNotThrow(() => (contextMenuStrip.Items[0] as TestMenuItem).TriggerImageMouseEnter());
	}

	class TestMenuItem : ZFilterToolStripMenuItem
	{
		public TestMenuItem(string text, bool isPublished, bool isFavorite) : base(text, isPublished, isFavorite)
		{
		}

		public override bool Visible => true;

		public void TriggerImageMouseEnter() => OnImageMouseEnter();
	}
}
