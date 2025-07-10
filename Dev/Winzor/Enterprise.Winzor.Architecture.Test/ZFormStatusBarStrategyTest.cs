using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Html.Dom;
using Bunit;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.Winzor.Architecture;
using Enterprise.Winzor.Architecture.Test;
using Microsoft.AspNetCore.Components;
using NUnit.Framework;
using WinzorFramework;
using Res = Enterprise.Winzor.Architecture.Res;

namespace Enterprise.ZArchitecture.GUI.Test;

class ZFormStatusBarStrategyTest
{
	[Test]
	public async Task StatusUpdatedWithHints()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new WinzorTestForm();
			form.BindingSource.DataSourceType = typeof(DummyBusinessObject);
			var textBox1 = new ZTextBox();
			textBox1.CaptionResourceString = Res.GetData("69b6789d-f9e2-40fb-b6be-12d29c0c9490", "One", "This is the first text box on the form");
			form.Controls.Add(textBox1);
			var textBox2 = new ZTextBox();
			textBox2.CaptionResourceString = Res.GetData("fa7b4e62-3c3d-4e82-a973-69a9ca11b471", "Two", "This is the other item on the form");
			form.Controls.Add(textBox2);
			form.CaptionRenderingEnabled = true;
			return form;
		});
		var inputs = rendered.FindAll("input").Cast<IHtmlInputElement>().ToArray();
		await inputs[1].TriggerEventAsync("onwinzorfocusin", new WinzorFocusInEventArgs());
		Assert.That(rendered.FindAll(".statusbar__panel:contains('This is the other item on the form')"), Is.Not.Empty);
		Assert.That(rendered.FindAll(".statusbar__panel:contains('This is the first text box on the form')"), Is.Empty);
		await inputs[0].TriggerEventAsync("onwinzorfocusin", new WinzorFocusInEventArgs());
		Assert.That(rendered.FindAll("div:contains('This is the first text box on the form')"), Is.Not.Empty);
		Assert.That(rendered.FindAll(".statusbar__panel:contains('This is the other item on the form')"), Is.Empty);
	}

	[Test]
	public async Task StatusUpdatedWithValidation()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new WinzorTestForm();
			form.BindingSource.DataSourceType = typeof(DummyBusinessObject);
			var textBox1 = new ZTextBox();
			textBox1.CaptionResourceString = Res.GetData("69b6789d-f9e2-40fb-b6be-12d29c0c9490", "One", "This is the first text box on the form");
			form.Controls.Add(textBox1);
			var descriptionTextBox = new ZTextBox();
			form.Controls.Add(descriptionTextBox);
			form.BindingSource.SetBindingMember(descriptionTextBox, "Z0_Description");
			form.DataSourceType = typeof(DummyBusinessObject);
			var factory = new BusinessObjectFactory();
			var dummyBizo = factory.New<DummyBusinessObject>();
			dummyBizo.Z0_Description = "Bad";
			form.SetDataBinding(dummyBizo, "");
			return form;
		});
		var inputs = rendered.FindAll("input").Cast<IHtmlInputElement>().ToArray();
		await inputs[1].TriggerEventAsync("onwinzorfocusin", new WinzorFocusInEventArgs());
		var markup = rendered.FindAll(".statusbar__panel:contains('Bad!')").ToList().Last().ToMarkup();
		Assert.That(markup, Does.Contain("color:#FF0000"));
		Assert.That(rendered.Find(".statusbar").ToMarkup(), Does.Contain("font-weight:bold"));
	}

	[Test]
	public async Task StatusNotClearedWhenCheckboxChecked()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new WinzorTestForm();
			form.BindingSource.DataSourceType = typeof(DummyBusinessObject);
			var checkBox = new ZCheckBox();
			checkBox.CaptionResourceString = Res.GetData("69b6789d-f9e2-40fb-b6be-12d29c0c9490", "Check It", "This is a checkbox");
			form.Controls.Add(checkBox);
			form.CaptionRenderingEnabled = true;
			return form;
		});
		var inputs = rendered.FindAll("input").Cast<IHtmlInputElement>().ToArray();
		await inputs[0].TriggerEventAsync("onwinzorfocusin", new WinzorFocusInEventArgs());
		Assert.That(rendered.FindAll(".statusbar__panel:contains('This is a checkbox')"), Is.Not.Empty);
		var renderCount = rendered.RenderCount;
		await inputs[0].ChangeAsync(new ChangeEventArgs { Value = true });
		Assert.That(rendered.FindAll(".statusbar__panel:contains('This is a checkbox')"), Is.Not.Empty);
	}

	[Test]
	public async Task StatusBarPanelHiddenWhenOverflowTest()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new WinzorTestForm();
			form.BindingSource.DataSourceType = typeof(DummyBusinessObject);
			form.MainStatusBar.Height = ControlDpiScalingHelper.ScaleToCurrentDpiY(6);
			return form;
		});
		var statusBar = rendered.Find(".statusbar__panel");
		Assert.That(statusBar, Is.Not.Null);
		Assert.That(statusBar.GetAttribute("style"), Does.Contain("height:0px;"));
	}
}
