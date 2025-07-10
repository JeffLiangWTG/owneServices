using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using WTG.PlaywrightTesting;

namespace Enterprise.Winzor.Architecture.Test;
public class ZMessageBoxTest
{
	[Test]
	public async Task TestTextBoxResizeOnBrowserSizeChanged()
	{
		ZMessageBox msgBox = null;
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var multiLineString = string.Join('\n', Enumerable.Range(1, 1000).Select(i => $"This is line {i}."));
			msgBox = new ZMessageBox(multiLineString, "Test", MessageBoxButtons.OKCancel, MessageBoxIcon.Asterisk);
			return msgBox;
		});

		var button = msgBox.Controls.Find("OKButton", false).Single();
		var designerInitialOffsets = 12; // 8 from TextBox.Top and 4 from the gap between textbox and the button

		await msgBox.OnBrowserSizeChangedAsync(900, 900);

		Assert.That(msgBox.TextBox.Height, Is.EqualTo(button.Top - designerInitialOffsets));
		Assert.That(msgBox.TextBox.Width, Is.EqualTo(msgBox.TextBox.Size.Width - (msgBox.ClientSize.Width - 900)));

		//only when going up in size we have to account for the extra space that could be occupied by additional controls above the OKButton(Button1, see SetClientSizeCore in ZMessageBox.cs)
		var buttonTopBefore = button.Top;
		await msgBox.OnBrowserSizeChangedAsync(1000, 1000);
		var buttonTopAfter = button.Top;
		var buttonTopDifference = buttonTopAfter - buttonTopBefore;

		Assert.That(msgBox.TextBox.Height, Is.EqualTo(button.Top - designerInitialOffsets - buttonTopDifference));
		Assert.That(msgBox.TextBox.Width, Is.EqualTo(msgBox.TextBox.Size.Width - (msgBox.ClientSize.Width - 1000)));

		await msgBox.OnBrowserSizeChangedAsync(200, 600);

		Assert.That(msgBox.TextBox.Height, Is.EqualTo(button.Top - designerInitialOffsets));
		Assert.That(msgBox.TextBox.Width, Is.EqualTo(msgBox.TextBox.Size.Width - (msgBox.ClientSize.Width - 200)));
	}
}
