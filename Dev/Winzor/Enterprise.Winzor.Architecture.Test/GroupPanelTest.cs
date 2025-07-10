using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bunit;
using Enterprise.ZArchitecture.GUI;
using Microsoft.Playwright;
using NUnit.Framework;
using WTG.PlaywrightTesting;

namespace Enterprise.Winzor.Architecture.Test;

using static PlaywrightTestContext;

class GroupPanelTest
{
	[Test]
	public async Task TestBorderStyle()
	{
		// Arrange / Act
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var groupPanel = new GroupPanel();
			groupPanel.Size = new Size(200, 100);
			return groupPanel;
		});

		// Assert
		Assert.That(rendered.Find(".grouppanel").Attributes["style"].Value, Does.Contain("border: 1px solid silver;"));
	}

	[Test, WithPlaywrightPage]
	public async Task GroupPanelDoesNotOverflow()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			form.Controls.Add(new GroupPanel());

			return form;
		});

		var groupPanel = await page.WaitForSelectorAsync(".grouppanel");
		Assert.That(async () => await groupPanel.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('overflow-x')"), Is.EqualTo("hidden"));
		Assert.That(async () => await groupPanel.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('overflow-y')"), Is.EqualTo("hidden"));
	}
}
