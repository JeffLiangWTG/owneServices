using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bunit;
using CargoWise.Common;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Microsoft.Playwright;
using NUnit.Framework;
using WTG.PlaywrightTesting;

namespace Enterprise.Winzor.Architecture.Test;

using static PlaywrightTestContext;
class ZToolStripTest
{
	[TestCase(true, ToolStripTextDirection.Vertical90, TestName = "{m}_Vertical90")]
	[TestCase(true, ToolStripTextDirection.Vertical270, TestName = "{m}_Vertical270")]
	[TestCase(false, ToolStripTextDirection.Horizontal, TestName = "{m}_Horizontal")]
	public async Task ZToolStripMenuItemVerticalText(bool vertical, ToolStripTextDirection textDirection)
	{
		using var ctx = new EnterpriseTestContext();
		ZToolStripMenuItem menuItem = null;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new WinzorTestForm();

			var toolStrip = new ZToolStrip();
			menuItem = new ZToolStripMenuItem() { Text = "文章" };
			toolStrip.Items.Add(menuItem);
			toolStrip.Items[0].TextDirection = textDirection;

			form.Controls.Add(toolStrip);
			return form;
		});

		var renderedItem = rendered.Find(".toolstrip__menuitem-text");
		Assert.That(renderedItem.OuterHtml.Contains("writing-mode: vertical-rl"), Is.EqualTo(vertical));
	}
}
