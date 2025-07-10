using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;
using Microsoft.Playwright;
using NUnit.Framework;
using WinzorTestFramework;
using WTG.PlaywrightTesting;
using static WTG.PlaywrightTesting.PlaywrightTestContext;

namespace Enterprise.Winzor.Architecture.Test;

class ZStmNoteUserControlTest
{
	[Test, WithPlaywrightPage]
	[TestCase(1000, TestName = "CheckRichTextBoxWidthResizes-1000")]
	[TestCase(500, TestName = "CheckRichTextBoxWidthResizes-500")]
	public async Task CheckRichTextBoxSize(int parentWidth)
	{
		var padding = 6 + 6 + 16;
		var expectedWidth = parentWidth - padding;
		await using var ctx = new InMemoryAppServerTestContext();

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form() { Size = new System.Drawing.Size(parentWidth, 700) };
			var control = new ZStmNoteUserControl();
			form.Controls.Add(control);
			return form;
		});

		await page.BringToFrontAsync();
		var internalZRichTextBox = await page.WaitForSelectorAsync("[data-type='Enterprise.ZArchitecture.GUI.Internal.ZStmNoteRichTextBox+InternalZRichTextBox']");
		var internalZRichTextBoxSize = await internalZRichTextBox.BoundingBoxAsync();
		var richTextBox = await page.WaitForSelectorAsync(".richtextbox");
		var richTextBoxSize = await richTextBox.BoundingBoxAsync();

		Assert.That(internalZRichTextBoxSize.Width, Is.EqualTo(expectedWidth), "Internal ZRichTextBox size was incorrect");
		Assert.That(richTextBoxSize.Width, Is.EqualTo(expectedWidth), "RichTextBox size was incorrect");
	}

	[Test, WithPlaywrightPage]
	public async Task CheckRichTextBoxTop()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form() { Size = new System.Drawing.Size(1000, 1000) };
			var control = new ZStmNoteUserControl(); 
			var zNoteTextBox = new Control();

			form.Controls.Add(control);
			return form;
		});
		var richTextBox = page.Locator(".richtextbox");
		Assert.That((await richTextBox.GetComputedStyleAsync("top")).AsPixels, Is.EqualTo(0).After(2000,100));
	}

	[Test, WithPlaywrightPage]
	public async Task ZRichTextBoxPopupButtonShouldNotBeVisible()
	{
		await using var ctx = new InMemoryAppServerTestContext();

		var page = await ctx.LoadControlOnFormAsync(() => new ZStmNoteUserControl());

		var popupButtonOfZStmNoteRichTextBox = page.Locator("[data-type='Enterprise.ZArchitecture.GUI.Internal.ZStmNoteRichTextBox+InternalZRichTextBox'] + button[title='Popup']");
		var popupButtonOfZRichTextBox = page.Locator("[data-type='Enterprise.ZArchitecture.GUI.Internal.ZStmNoteRichTextBox+InternalZRichTextBox'] button[title='Popup']");

		Assert.That(await popupButtonOfZStmNoteRichTextBox.CountAsync(), Is.EqualTo(1));
		Assert.That(await popupButtonOfZRichTextBox.CountAsync(), Is.EqualTo(0));
	}
}
