using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bunit;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.GUI;
using Microsoft.Playwright;
using NUnit.Framework;
using WinzorFramework;
using WTG.PlaywrightTesting;
using Res = CargoWiseOne.ResourceStrings.Res;

namespace Enterprise.Winzor.Architecture.Test;

using static PlaywrightTestContext;
class ZCheckBoxTest
{
	[Test]
	public async Task ZCheckBoxShouldBeFocusableWhenReadonly()
	{
		using var ctx = new EnterpriseTestContext();
		ZCheckBox checkbox = null;
		var rendered = await ctx.RenderControlOnFormAsync(() => checkbox = new ZCheckBox() { ReadOnly = true, Enabled = true });
		Assert.That(rendered.Find("input.checkbox__input").Attributes["disabled"], Is.Null);
		Assert.That(rendered.Find("input.checkbox__input").Attributes["aria-disabled"], Is.Not.Null);

		await checkbox.InvokeWinzorDispatcherAsync(() => checkbox.Enabled = false);
		Assert.That(rendered.Find("input.checkbox__input").Attributes["disabled"], Is.Not.Null);
	}

	[Test]
	public async Task CheckBoxFocus()
	{
		using var ctx = new EnterpriseTestContext();
		ZCheckBox checkbox1 = null;
		ZCheckBox checkbox2 = null;
		Color checkparentcolor1;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form() { BackColor = Color.AliceBlue };
			checkbox1 = new ZCheckBox();
			checkbox2 = new ZCheckBox();
			form.Controls.Add(checkbox1);
			form.Controls.Add(checkbox2);
			checkbox1.Parent = form;
			checkbox2.Parent = form;
			checkparentcolor1 = checkbox1.BackColor;
			return form;
		});
		var checkbox_focused = rendered.FindAll(".checkbox")[0];
		Assert.That(checkbox_focused.GetAttribute("style"), Is.EqualTo("position:absolute;width:104px;height:24px;top:0px;left:0px;background-color:var(--color-info);"));

		var checkbox_tofocus = rendered.FindAll(".checkbox input[type = checkbox].checkbox__input")[1];
		await checkbox_tofocus.TriggerEventAsync("onwinzorfocusin", new WinzorFocusInEventArgs());

		var checkbox_focused1 = rendered.FindAll(".checkbox")[0];
		Assert.That(checkbox_focused1.GetAttribute("style"), Is.EqualTo("position:absolute;width:104px;height:24px;top:0px;left:0px;background-color:#F0F8FFFF;"));
	}

	[TestCaseSource(nameof(ZCheckboxToolTipSource))]
	public async Task CheckboxTooltip(ResourceStringData captions, string text, bool enable, string expectedTitle)
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new ZForm() { BackColor = Color.AliceBlue };
			var checkbox = new ZCheckBox();
			// what should affect the title value is text and captions
			checkbox.Text = text;
			checkbox.Enabled = enable;
			checkbox.CaptionResourceString = captions;
			form.Controls.Add(checkbox);
			form.CaptionRenderingEnabled = true;
			return form;
		});

		var checkbox_focused = rendered.FindAll(".checkbox")[0];

		Assert.That(checkbox_focused.GetAttribute("title"), Is.EqualTo(expectedTitle));
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "EDI001:ResourceStringStaticReferenceRule", Justification = "Testing code")]
	static readonly ResourceStringData captionData = Res.GetData("69b6789d-f9e2-40fb-b6be-12d29c0c9490", "Check It", "This is a checkbox");
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "EDI001:ResourceStringStaticReferenceRule", Justification = "Testing code")]
	static readonly ResourceStringData emptyCaptionData = Res.GetData("69b6789d-f9e2-40fb-b6be-12d29c0c6490", string.Empty, string.Empty);

	static IEnumerable<TestCaseData> ZCheckboxToolTipSource()
	{
		yield return new TestCaseData(captionData, string.Empty, true, "Check It") { TestName = "{m}_CaptionData_Empty_true_CheckIt" };
		yield return new TestCaseData(emptyCaptionData, "test text", true, "test text") { TestName = "{m}_EmptyCaptionData_testtext_true_testtext" };
		yield return new TestCaseData(captionData, "test text", false, null) { TestName = "{m}_CaptionData_testtext_false_null" };
		yield return new TestCaseData(emptyCaptionData, "test text", false, null) { TestName = "{m}_EmptyCaptionData_testtext_false_null" };
		yield return new TestCaseData(null, null, false, null) { TestName = "{m}_null_null_false_null" };
		yield return new TestCaseData(null, null, true, null) { TestName = "{m}_null_null_true_null" };
	}

	[Test, WithPlaywrightPage]
	public async Task ReadOnlyZCheckBoxShouldNotBeFocusByTab()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var zCheckbox1 = new ZCheckBox() { Enabled = true, ReadOnly = true };
			var zCheckbox2 = new ZCheckBox() { Enabled = true, ReadOnly = true };
			var zCheckbox3 = new ZCheckBox() { Enabled = true, ReadOnly = false };
			var zCheckbox4 = new ZCheckBox() { Enabled = true, ReadOnly = false };

			var form = new Form();
			form.Controls.Add(zCheckbox1);
			form.Controls.Add(zCheckbox2);
			form.Controls.Add(zCheckbox3);
			form.Controls.Add(zCheckbox4);

			return form;
		});

		var form = await page.WaitForSelectorAsync(".form");
		Assert.That(form, Is.Not.Null);

		var tabNumber = 20;
		for (var i = 0; i < tabNumber; i++)
		{
			await form.PressAsync("Tab");

			var box1 = await page.WaitForSelectorAsync("label:nth-child(1) .checkbox__input");
			var box1IsFocused = await page.EvaluateAsync<bool>("document.activeElement === document.querySelector('label:nth-child(1) .checkbox__input')");
			Assert.That(box1IsFocused, Is.False);

			var box2 = await page.WaitForSelectorAsync("label:nth-child(2) .checkbox__input");
			var box2IsFocused = await page.EvaluateAsync<bool>("document.activeElement === document.querySelector('label:nth-child(2) .checkbox__input')");
			Assert.That(box2IsFocused, Is.False);
		}
	}
}
