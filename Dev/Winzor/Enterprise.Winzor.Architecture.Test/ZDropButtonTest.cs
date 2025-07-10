using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bunit;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Microsoft.Playwright;
using NUnit.Framework;
using WTG.PlaywrightTesting;

namespace Enterprise.Winzor.Architecture.Test;

using static PlaywrightTestContext;

class ZDropButtonTest
{
	[TestCase(true, true, "zdropbutton zdropbutton--disabled", TestName = "{m}_IsTrue_ShouldBeDisabled")]
	[TestCase(false, false, "zdropbutton ", TestName = "{m}_IsFalse_ShouldNotBeDisabled")]
	public async Task ZDropButtonReadOnly(bool @readonly, bool isEmpty, string expectedClass)
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() => new ZDropEdit() { ReadOnly = @readonly });

		var zDropButton = rendered.Find(".zdropbutton");
		Assert.That(zDropButton.Attributes["class"].Value, Is.EqualTo(expectedClass));

		var button = rendered.Find(".zdropbutton button");
		Assert.That(button.Attributes.Any(a => a.Name == "disabled"), Is.EqualTo(isEmpty));
	}

	[TestCase(true, false, "zdropbutton ", TestName = "{m}_IsFalse_ShouldNotBeDisabled")]
	[TestCase(false, true, "zdropbutton zdropbutton--disabled", TestName = "{m}_IsTrue_ShouldBeDisabled")]
	public async Task ZDropButtonEnable(bool enable, bool isEmpty, string expectedClass)
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var dropEdit = new ZDropEdit();
			dropEdit.SetDropButtonAvailability(enable);

			return dropEdit;
		});

		var zDropButton = rendered.Find(".zdropbutton");
		Assert.That(zDropButton.Attributes["class"].Value, Is.EqualTo(expectedClass));

		var button = rendered.Find(".zdropbutton button");
		Assert.That(button.Attributes.Any(a => a.Name == "disabled"), Is.EqualTo(isEmpty));
	}

	[Test]
	public async Task ZDropButtonShouldNotHandleClickIfDisabled()
	{
		var eventFired = false;
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var dropEdit = new ZDropEdit();
			dropEdit.SetDropButtonAvailability(false);
			dropEdit.DropButton.MouseDown += (_, _) => eventFired = true;
			return dropEdit;
		});

		var button = rendered.Find(".zdropbutton button");
		await button.MouseDownAsync(new WebMouseEventArgs());
		Assert.That(eventFired, Is.False);
	}

	[Test]
	public async Task ZDropButtonShouldNotHandleClickIfReadonly()
	{
		var eventFired = false;
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var dropEdit = new ZDropEdit() { ReadOnly = true };
			dropEdit.DropButton.MouseDown += (_, _) => eventFired = true;
			return dropEdit;
		});

		var button = rendered.Find(".zdropbutton button");
		await button.MouseDownAsync(new WebMouseEventArgs());
		Assert.That(eventFired, Is.False);
	}

	[Test]
	public async Task ZDropButtonRendersEnabledAfterReadOnlyChange()
	{
		ZDropEdit dropEdit = null;
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			dropEdit = new ZDropEdit() { ReadOnly = true };
			dropEdit.SetDropButtonAvailability(true);

			return dropEdit;
		});

		var button = rendered.Find(".zdropbutton button");
		Assert.That(button.Attributes.Any(a => a.Name == "disabled"), Is.EqualTo(true));

		await dropEdit.InvokeWinzorDispatcherAsync(() => { dropEdit.ReadOnly = false; });
		var button1 = rendered.Find(".zdropbutton button");
		Assert.That(button.Attributes.Any(a => a.Name == "disabled"), Is.EqualTo(false));
	}

	[Test]
	public async Task ZDropButtonClickShowsAndHidesDropEdit()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new WinzorTestForm();
			form.BindingSource.DataSourceType = typeof(DummyBusinessObjectWithList);
			var dropedit = new ZDropEdit();
			dropedit.BindToList = "List";
			form.Controls.Add(dropedit);
			form.BindingSource.SetBindingMember(dropedit, "Z0_Code");
			var factory = new BusinessObjectFactory();
			var dummyBizo = factory.New<DummyBusinessObjectWithList>();
			dummyBizo.Z0_Code = string.Empty;
			dummyBizo.List.AddNew().Z0_Code = "AAA";
			dummyBizo.List.AddNew().Z0_Code = "BBB";
			dummyBizo.List.AddNew().Z0_Code = "CCC";
			form.SetDataBinding(dummyBizo, "");

			return form;
		});

		rendered.Find(".zdropbutton button").MouseDown();
		Assert.That(rendered.WaitForElement(".zdropform"), Is.Not.Null);

		await rendered.Find(".zdropbutton button").MouseDownAsync(new WebMouseEventArgs());
		Assert.Throws<ElementNotFoundException>(() => rendered.Find(".zdropform"));
	}

	[Test]
	public async Task ZGridDropButtonInlineStyle()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() => new ZGridDropEdit { IsDropButtonVisible = true });

		var renderedButton = rendered.Find(".zdropbutton button");
		Assert.That(renderedButton.GetAttribute("style"), Does.Contain("left:214px;width:17px;height:20px;"));
	}

	[TestCase(true, true, TestName = "{m}IsTrue_ShouldRenderButton")]
	[TestCase(false, false, TestName = "{m}IsFalse_ShouldNotRenderButton")]
	public async Task ZGridDropButtonIsVisible(bool isDropButtonVisible, bool expectedResult)
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() => new ZGridDropEdit { IsDropButtonVisible = isDropButtonVisible });

		Assert.That(rendered.FindAll(".zdropbutton").Any(), Is.EqualTo(expectedResult));
	}

	[Test, WithPlaywrightPage]
	[TestCase(true, "0px")]
	[TestCase(false, "1px")]
	public async Task ZDropButtonCorrectBorderAndColor(bool showDescriptionBox, string borderRightWidth)
	{
		await using var ctx = new InMemoryAppServerTestContext();
		Form form = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form();
			var button = new ZDropEdit();
			button.ShowDescriptionBox = showDescriptionBox;
			var dropButton = new ZDropButtonOnly();
			form.Controls.Add(button);
			form.Controls.Add(dropButton);
			return form;
		});

		var dropDownButton = await page.WaitForSelectorAsync(".zdropbutton");
		Assert.That(async () => await dropDownButton.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('border-left-color')"), Is.EqualTo("rgb(171, 173, 177)"));
		Assert.That(async () => await dropDownButton.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('border-top-color')"), Is.EqualTo("rgb(171, 173, 177)"));
		Assert.That(async () => await dropDownButton.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('border-bottom-color')"), Is.EqualTo("rgb(171, 173, 177)"));
		Assert.That(async () => await dropDownButton.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('border-right-width')"), Is.EqualTo(borderRightWidth));

		var innerButton = await page.WaitForSelectorAsync(".zdropbutton > button");
		Assert.That(async () => await innerButton.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('color')"), Is.EqualTo("rgb(110, 110, 110)"));
		Assert.That(async () => await innerButton.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('border-left-color')"), Is.EqualTo("rgb(210, 210, 210)"));
		Assert.That(async () => await innerButton.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('background-color')"), Is.EqualTo("rgb(253, 253, 253)"));
		Assert.That(async () => await innerButton.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('border-left-width')"), Is.EqualTo("1px"));
		Assert.That(async () => await innerButton.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('border-right-width')"), Is.EqualTo("0px"));
		Assert.That(async () => await innerButton.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('display')"), Is.EqualTo("flex"));
		Assert.That(async () => await innerButton.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('align-items')"), Is.EqualTo("center"));
		Assert.That(async () => await innerButton.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('justify-content')"), Is.EqualTo("center"));

		var dropButtonOnly = await page.WaitForSelectorAsync(".zdropbuttononly");
		Assert.That(async () => await dropButtonOnly.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('color')"), Is.EqualTo("rgb(110, 110, 110)"));
		Assert.That(async () => await dropButtonOnly.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('border-width')"), Is.EqualTo("1px"));
	}
}
