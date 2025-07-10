using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using Enterprise.EConversation.Business;
using Enterprise.EConversation.GUI;
using Microsoft.Playwright;
using Moq;
using NUnit.Framework;
using WTG.PlaywrightTesting;

namespace Enterprise.Winzor.Architecture.Test;

using static PlaywrightTestContext;

public class ConversationMessageUserControlTest
{
	[Test, WithPlaywrightPage]
	public async Task ConversationMessageUserControlHasAutoVerticalScrollbar()
	{
		ConversationMessageUserControl control = null;

		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			control = GetTestConversationMessageUserControl();
			control.Visible = true;
			form.Controls.Add(control);
			return form;
		});

		var data = await page.WaitForSelectorAsync(".richtextbox__data");
		Assert.That(async () => await data.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('overflow-x')"), Is.EqualTo("hidden").After(2000, 200));
		Assert.That(await data.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('overflow-y')"), Is.EqualTo("auto"));
	}

	[Test, WithPlaywrightPage]
	public async Task LineWrapping()
	{
		ConversationMessageUserControl control = null;

		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			control = GetTestConversationMessageUserControl(new string('a', 1800));
			form.Controls.Add(control);

			return form;
		});

		//Ensure that line-wrapping occurs, with appropriate CSS values,
		var data = await page.WaitForSelectorAsync(".richtextbox__data");
		Assert.That(async () => await data.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('overflow-wrap')"), Is.EqualTo("break-word").After(2000, 200));
	}

	[TestCase("a", 179, 44, TestName = "{m}_a")]
	[TestCase("a\na\na", 179, 70, TestName = "{m}_a_3Lines")]
	[TestCase("~", 527, 434, 39, TestName = "{m}_a_1800CharactersOnSingleLine")]
	[TestCase($"start ~ end", 527, 447, 39, TestName = "{m}_a_ShortAndLongWordMix")]
	public async Task CalculateSizeReturnsCorrectSize(string text, int expectedWidth, int expectedHeight, int additionalRowHeight = 0)
	{
		text = text.Replace("~", new string('a', 1800));

		ConversationMessageUserControl control = null;

		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() => control = GetTestConversationMessageUserControl(text));

		Assert.That(control, Is.Not.Null);
		Assert.That(control.bodyTextBox.Text, Is.EqualTo(text));

		await control.InvokeWinzorDispatcherAsync(() =>
		{
			var size = control.CalculateSize(DefaultParentWidth);
			Assert.That(size.Width, Is.EqualTo(expectedWidth).Within(15)); //Margin of error to allow minor TextRenderer differences.
			Assert.That(size.Height, Is.EqualTo(expectedHeight - additionalRowHeight)); //Differences in line length can lead to differences in line count.
		});
	}

	#region Test Data

	const int DefaultParentWidth = 530;

	#endregion

	#region Helper

	ConversationMessageUserControl GetTestConversationMessageUserControl(string text = "Default Text")
	{
		var message = new Mock<IConversationMessage>().Object;
		var control = new ConversationMessageUserControl(message, true)
		{
			Height = 74,
			Width = 660,
		};

		control.bodyTextBox.Text = text;

		return control;
	}

	#endregion
}
