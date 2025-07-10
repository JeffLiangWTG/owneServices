using System.Threading.Tasks;
using Bunit;
using NUnit.Framework;
using WinzorTestFramework;

namespace System.Windows.Forms;

class ToolStripGripTest
{
	[Test]
	public async Task ToolStripGripShouldContainImage()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var toolStrip = new ToolStrip();
			toolStrip.Items.Add(new ToolStripGrip());
			form.Controls.Add(toolStrip);
			return form;
		});

		var imgElement = rendered.Find("img");

		Assert.That(imgElement, Is.Not.Null);
		Assert.That(imgElement.GetAttribute("src"), Does.StartWith("data:image/png;base64,"));
	}
}
