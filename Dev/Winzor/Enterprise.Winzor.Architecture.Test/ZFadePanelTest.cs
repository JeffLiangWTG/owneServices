using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bunit;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using WTG.PlaywrightTesting;
using static WTG.PlaywrightTesting.PlaywrightTestContext;

namespace Enterprise.Winzor.Architecture.Test;

class ZFadePanelTest
{
	[TestCase((float)0.0, "background-color:transparent;background-image:linear-gradient(90deg, rgba(255, 255, 0, 0.50) 67%, rgba(128, 128, 128, 0.31) 77%);")]
	[TestCase((float)30.0, "background-color:transparent;background-image:linear-gradient(120deg, rgba(255, 255, 0, 0.50) 67%, rgba(128, 128, 128, 0.31) 77%);")]
	[TestCase((float)90.0, "background-color:transparent;background-image:linear-gradient(180deg, rgba(255, 255, 0, 0.50) 67%, rgba(128, 128, 128, 0.31) 77%);")]
	public async Task TestFadePanelGradient(float angle, string testString)
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() => new ZFadePanel()
		{
			Width = 100,
			Height = 50,
			FadeEndColor = Color.FromArgb(0x4f, Color.Gray),
			FadeStartColor = Color.FromArgb(0x7f, Color.Yellow),
			GradientAngle = angle,
			GradientStartPercent = .67f,
			GradientSizePercent = .10f
		});

		Assert.That(rendered.Find(".fadepanel").Attributes["style"].Value, Does.Contain(testString));
	}

	[Test, WithPlaywrightPage]
	public async Task BackgroundForTransparentChildrenAndItSelf()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			var fadePanel = new ZFadePanel()
			{
				Left = 3,
				Top = 4,
				Width = 13,
				Height = 14,
				BackgroundImage = new Bitmap(13, 14)
			};
			var panel = new Panel() { BackColor = Color.Transparent };
			panel.Controls.Add(new Label() { BackColor = Color.Transparent });
			panel.Controls.Add(new PictureBox() { BackColor = Color.Transparent });
			panel.Controls.Add(new Label());
			panel.Controls.Add(new PictureBox() { BackColor = Color.Transparent });
			panel.Controls.Add(new PictureBox());
			fadePanel.Controls.Add(panel);
			form.Controls.Add(fadePanel);
			return form;
		});

		var fadePanel = await page.WaitForSelectorAsync(".fadepanel");
		Assert.That(await fadePanel.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('background-image')"), Does.StartWith("url(\"data:image/png;"));
		Assert.That(await fadePanel.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('background-position')"), Is.EqualTo("3px 4px"));
		Assert.That(await fadePanel.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('background-attachment')"), Is.EqualTo("fixed"));

		var cssSelector = "[style*=\"background-color:inherit;\"]";
		var elementCount = await page.EvaluateAsync<int>($"e => document.querySelectorAll('{cssSelector}').length");
		var backgroundImageExpression = "nodes => nodes.map(node => window.getComputedStyle(node).getPropertyValue('background-image').slice(0, 20))";
		var backgroundSizeExpression = "nodes => nodes.map(node => window.getComputedStyle(node).getPropertyValue('background-size'))";
		var backgroundPositionExpression = "nodes => nodes.map(node => window.getComputedStyle(node).getPropertyValue('background-position'))";
		var backgroundAttachmentExpression = "nodes => nodes.map(node => window.getComputedStyle(node).getPropertyValue('background-attachment'))";

		Assert.That(await page.EvalOnSelectorAllAsync<string[]>(cssSelector, backgroundImageExpression), Is.EqualTo(Enumerable.Repeat("url(\"data:image/png;", elementCount).ToArray()));
		Assert.That(await page.EvalOnSelectorAllAsync<string[]>(cssSelector, backgroundSizeExpression), Is.EqualTo(Enumerable.Repeat("13px 14px", elementCount).ToArray()));
		Assert.That(await page.EvalOnSelectorAllAsync<string[]>(cssSelector, backgroundPositionExpression), Is.EqualTo(Enumerable.Repeat("3px 4px", elementCount).ToArray()));
		Assert.That(await page.EvalOnSelectorAllAsync<string[]>(cssSelector, backgroundAttachmentExpression), Is.EqualTo(Enumerable.Repeat("fixed", elementCount).ToArray()));
	}
}
