using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using AngleSharp.Html.Dom;
using Bunit;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Controls.Extensions;
using Microsoft.AspNetCore.Components;
using Microsoft.Playwright;
using NUnit.Framework;
using WinzorTestFramework;
using WTG.PlaywrightTesting;
using static WTG.PlaywrightTesting.PlaywrightTestContext;
using Res = CargoWiseOne.ResourceStrings.Res;

namespace Enterprise.Winzor.Architecture.Test;

class CheckBoxTest
{
	[Test]
	public async Task CheckBoxTwoWayBinding()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new WinzorTestForm();
			form.BindingSource.DataSourceType = typeof(DummyBusinessObject);
			var bitFalseCheckbox = new ZCheckBox();
			form.Controls.Add(bitFalseCheckbox);
			form.BindingSource.SetBindingMember(bitFalseCheckbox, "Z0_BitFalse");
			var bitTrueCheckbox = new ZCheckBox();
			form.Controls.Add(bitTrueCheckbox);
			form.BindingSource.SetBindingMember(bitTrueCheckbox, "Z0_BitTrue");
			form.DataSourceType = typeof(DummyBusinessObject);
			var factory = new BusinessObjectFactory();
			var dummyBizo = factory.New<DummyBusinessObject>();
			dummyBizo.Z0_BitFalseInfo.ValueChanged += (object sender, EventArgs e) =>
			{
				dummyBizo.Z0_BitTrue = !dummyBizo.Z0_BitFalse;
			};
			form.SetDataBinding(dummyBizo, "");
			return form;
		});
		var inputs = rendered.FindAll("input").Cast<IHtmlInputElement>();
		Assert.That(inputs.Select(e => e.IsChecked), Is.EqualTo(new[] { false, true }));
		await inputs.First().ChangeAsync(new ChangeEventArgs { Value = true });
		Assert.That(rendered.FindAll("input").Cast<IHtmlInputElement>().Select(e => e.IsChecked), Is.EqualTo(new[] { true, false }));
	}

	[Test]
	public async Task RenderedCaptionFromCaptionResourceString()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new ZForm();
			form.Size = new Size(300, 300);
			var checkBox = new ZCheckBox();
			checkBox.AutoSize = true;
			checkBox.CaptionResourceString = Res.GetData("4d319211-496e-42b5-8c25-7bcc9f19fcbb", "Hello world");
			checkBox.Width = 100;
			form.Controls.Add(checkBox);
			form.CaptionRenderingEnabled = true;
			return form;
		});
		Assert.That(rendered.Find("span:contains('Hello world')"), Is.Not.Null);
	}

	[Test]
	public async Task RenderedCaptionFromBinding()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new WinzorTestForm();
			form.Size = new Size(300, 300);
			form.BindingSource.DataSourceType = typeof(DummyBusinessObject);
			var checkBox = new ZCheckBox();
			checkBox.AutoSize = true;
			checkBox.Width = 100;
			form.Controls.Add(checkBox);
			form.BindingSource.SetBindingMember(checkBox, "Z0_Bool");
			form.DataSourceType = typeof(DummyBusinessObject);
			var factory = new BusinessObjectFactory();
			var dummyBizo = factory.New<DummyBusinessObject>();
			form.SetDataBinding(dummyBizo, "");
			form.CaptionRenderingEnabled = true;
			return form;
		});
		Assert.That(rendered.Find("span:contains('Flag')"), Is.Not.Null);
	}

	[Test]
	public async Task OnErrorSetErrorBackground()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new WinzorTestForm();
			form.BindingSource.DataSourceType = typeof(RefCommodityCode);
			var typeCheckbox = new ZCheckBox();
			form.Controls.Add(typeCheckbox);
			form.BindingSource.SetBindingMember(typeCheckbox, "RH_IsForwarding");
			var factory = new BusinessObjectFactory();
			var bo = factory.New<RefCommodityCode>();
			bo.RH_IsForwarding = true;
			form.SetDataBinding(bo, "");
			return form;
		});

		var inputs = rendered.FindAll("input").Cast<IHtmlInputElement>();
		await inputs.First().ChangeAsync(new ChangeEventArgs { Value = false });
		var label = rendered.Find("label");

		Assert.That(label.Attributes["style"].Value, Does.Contain("background-color:#FFD7D7FF").IgnoreCase);

		await inputs.First().ChangeAsync(new ChangeEventArgs { Value = true });

		Assert.That(label.Attributes["style"].Value, Does.Not.Contain("background-color:#FFD7D7FF").IgnoreCase);
	}

	[Test]
	public async Task CheckBoxShouldNotFireChangeEventThrowExceptionIfReadonly()
	{
		Exception expectedException = null;
		var eventFired = false;
		using var ctx = new EnterpriseTestContext();
		ZCheckBox checkBox = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			checkBox = new ZCheckBox() { ReadOnly = true };
			checkBox.Click += (_, _) => eventFired = true;
			return checkBox;
		});
		try
		{ 
			var button = rendered.Find(".checkbox__input");
			await button.ClickAsync(new WebMouseEventArgs());
		}
		catch (Exception ex)
		{
			expectedException = ex;
			Assert.That(ex, Is.Not.Null);
		}
		finally
		{
			Assert.That(eventFired, Is.False);
			Assert.That(checkBox.Checked, Is.False);
		}
	}

	[Test, WithPlaywrightPage]
	public async Task CheckBoxInDataGridEditStylesDisabled()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var factory = new BusinessObjectFactory();
			var dummyBizo = factory.New<DummyBusinessObject>();
			var child1 = factory.New<DummyBaseBusinessObject>();
			child1.Z0_Bool = true;
			dummyBizo.Collection.Add(child1);

			var grid = new ZGrid { Width = 400, Height = 400 };
			grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Z0_Description", 100));
			grid.ColumnStyles.Add(new ZCheckBoxColumnStyleInfo("Z0_Bool", 100) { IsReadOnly = true });
			grid.LayoutCategoryPK = Guid.NewGuid();

			var form = new WinzorTestForm { Width = 500, Height = 500 };
			form.BindingSource.DataSourceType = typeof(DummyBusinessObject);
			form.BindingSource.SetBindingMember(grid, "Collection");
			form.Controls.Add(grid);
			form.DataSourceType = typeof(DummyBusinessObject);
			form.SetDataBinding(dummyBizo, "");
			return form;
		});

		var checkbox = page.GetByRole(AriaRole.Checkbox);
		await Assertions.Expect(checkbox).ToHaveCountAsync(1);
		await Assertions.Expect(checkbox).ToBeCheckedAsync();

		await Assertions.Expect(checkbox).ToHaveCSSAsync("border-color", "rgb(51, 51, 51)");
		Assert.That((await checkbox.GetComputedStyleAsync("background-image")).ToString(), Contains.Substring("%23333"));

		await checkbox.ClickAsync();

		// Added delay allowing DOM to update
		await Task.Delay(500);

		await Assertions.Expect(checkbox).ToHaveCountAsync(1);
		await Assertions.Expect(checkbox).ToBeCheckedAsync();

		await Assertions.Expect(checkbox).ToHaveCSSAsync("border-color", "rgb(51, 51, 51)");
		Assert.That((await checkbox.GetComputedStyleAsync("background-image")).ToString(), Contains.Substring("%23333"));
	}
}

