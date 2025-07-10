using System.Drawing;
using System.Threading.Tasks;
using Bunit;
using CargoWise.Windows.UI;
using NUnit.Framework;

namespace Enterprise.Winzor.Architecture.Test;

class KPanelTest
{
	[Test]
	public async Task ShouldHaveAdditionalStyleIfSetProperty()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var panel = new KPanel { BackColor = Color.Aquamarine };
			panel.htmlBorder.BorderRadius = 3;
			return panel;
		});
		var panelEl = rendered.Find(".panel");

		Assert.That(panelEl.GetAttribute("style"), Does.Contain("border-radius:3px"));
		Assert.That(panelEl.GetAttribute("style"), Does.Contain($"background-color:#7FFFD4FF"));
	}

	[Test]
	public async Task HasBorderHtmlStyle()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var panel = new KPanel()
			{
				Width = 100,
				Height = 100,
			};
			panel.htmlBorder.BorderLineStyle = KBorderHtmlStyle.Solid;
			panel.htmlBorder.BorderColor = Color.Red;
			panel.htmlBorder.BorderWidth = 1f;
			return panel;
		});

		var button = rendered.Find(".panel");
		Assert.That(button.GetAttribute("style"), Does.Contain("border: 1px Solid #FF0000FF;"));
	}
}
