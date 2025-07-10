using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using AngleSharp.Css.Dom;
using Bunit;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.JSInterop;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;
using Moq;
using NUnit.Framework;
using WinzorTestFramework;
using WTG.PlaywrightTesting;
using static WTG.PlaywrightTesting.PlaywrightTestContext;

namespace Enterprise.Winzor.Architecture.Test;

class ZDropFormTest
{
	static CodeDescriptionPairList TestList => new CodeDescriptionPairList { new CodeDescriptionPair("ONE", "Description 1"), new CodeDescriptionPair("TWO", "Description 2") };

	[Test, WithPlaywrightPage]
	public async Task ZDropFormPositionSet()
	{
		await using var ctx = new InMemoryAppServerTestContext();

		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var dropEdit = new ZDropEdit { List = TestList, Top = 0, Left = 0 };
			var dropForm = new ZDropForm(dropEdit);
			dropForm.ShowDropDown(dropForm.Location, true);
			return dropEdit;
		});

		var zdropform = page.Locator(".zdropform");
		await Assertions.Expect(zdropform).ToHaveCSSAsync("position", "relative");
		await Assertions.Expect(zdropform).ToHaveCSSAsync("z-index", "100");
	}

	[Test]
	public async Task ZDropFormTestItemsHaveCorrectSize()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new WinzorTestForm();
			var list = new CodeDescriptionPairList();

			list.AddPair(Guid.NewGuid(), "ONE", "Description 01");
			var dropEdit = new ZDropEdit { List = list };
			var dropForm = new ZDropForm(dropEdit);
			dropForm.ShowDropDown(dropForm.Location, false);
			form.Controls.Add(dropEdit);
			return form;
		});
		Assert.That(rendered.Find(".zdropform").GetStyle().GetPropertyValue("height"), Is.EqualTo("16px"));
		Assert.That(rendered.Find(".zdropform").GetStyle().GetPropertyValue("width"), Is.EqualTo("136px"));
	}

	[Test]
	public async Task TestShowDropDownTrace()
	{
		var traces = new List<Activity>();
		using var listener = new ActivityListener
		{
			ShouldListenTo = source => source.Name == "WinzorFramework",
			Sample = (ref ActivityCreationOptions<ActivityContext> options) => ActivitySamplingResult.AllData,
			ActivityStarted = activity => traces.Add(activity),
			ActivityStopped = activity => { }
		};
		ActivitySource.AddActivityListener(listener);

		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new WinzorTestForm();
			var list = new CodeDescriptionPairList();

			list.AddPair(Guid.NewGuid(), "ONE", "Description 01");
			var dropEdit = new ZDropEdit { List = list };
			var dropForm = new ZDropForm(dropEdit);
			dropForm.ShowDropDown(dropForm.Location, false);
			form.Controls.Add(dropEdit);
			return form;
		});

		Assert.That(traces.Count, Is.GreaterThanOrEqualTo(1));
		Assert.That(traces.Any(t => t.DisplayName == "ZDropForm.ShowDropDown"), Is.True);
	}

	[TestCase(ZDropEdit.ShowInDropDownList.OnlyShowCode, TestName = "ZDropFormHaveCorrentColumnWidth_OnlyCode")]
	[TestCase(ZDropEdit.ShowInDropDownList.OnlyShowDescription, TestName = "ZDropFormHaveCorrentColumnWidth_OnlyDescription")]
	[TestCase(ZDropEdit.ShowInDropDownList.ShowCodeAndDescription, TestName = "ZDropFormHaveCorrentColumnWidth_CodeAndDescription")]
	public async Task ZDropFormHaveCorrentColumnWidth(ZDropEdit.ShowInDropDownList showOption)
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new WinzorTestForm();
			var list = new CodeDescriptionPairList
			{
				new CodeDescriptionPair("Apple", "Description 01"),
				new CodeDescriptionPair("Bananananana", "Description of banana"),
			};
			var dropEdit = new ZDropEdit { List = list, ShowInDropDown = showOption };
			var dropForm = new DummyZDropForm(dropEdit);
			dropForm.ShowDropDown(dropForm.Location, false);
			form.Controls.Add(dropEdit);
			return form;
		});

		var styleString = rendered.Find(".zdropform table tr td").GetAttribute("style") ?? string.Empty;
		if (showOption == ZDropEdit.ShowInDropDownList.ShowCodeAndDescription)
		{
			Assert.That(styleString, Does.Contain("width:44%"));
		}
		else
		{
			Assert.That(styleString, Does.Not.Contain("width"));
		}
	}

	[Test]
	public async Task SelectedItemReturnsItem()
	{
		using var ctx = new EnterpriseTestContext();
		ZDropForm dropForm = null;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new WinzorTestForm();
			var dropEdit = new ZDropEdit { List = TestList };
			dropForm = new ZDropForm(dropEdit);
			dropForm.ShowDropDown(dropForm.Location, false);
			form.Controls.Add(dropEdit);
			return form;
		});

		Assert.That(dropForm.SelectedItem, Is.Null);
		dropForm.HighlightedItem_Exposed = 0;
		Assert.That(dropForm.SelectedItem.Code, Is.EqualTo("ONE"));
		dropForm.HighlightedItem_Exposed = 1;
		Assert.That(dropForm.SelectedItem.Code, Is.EqualTo("TWO"));
		dropForm.HighlightedItem_Exposed = 99;
		Assert.That(dropForm.SelectedItem, Is.Null);
	}

	[Test]
	public async Task IsSelectableHandlesBoundsChecking()
	{
		using var ctx = new EnterpriseTestContext();
		DummyZDropForm dropForm = null;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new WinzorTestForm();
			var dropEdit = new ZDropEdit { List = TestList };
			dropForm = new DummyZDropForm(dropEdit);
			dropForm.ShowDropDown(dropForm.Location, false);
			form.Controls.Add(dropEdit);
			return form;
		});

		dropForm.HighlightedItem_Exposed = -1;
		Assert.That(dropForm.CurrentRowSelectable(-1), Is.False);
		Assert.That(dropForm.CurrentRowSelectable(0), Is.False);
		dropForm.HighlightedItem_Exposed = 0;
		Assert.That(dropForm.CurrentRowSelectable(0), Is.True);
		dropForm.HighlightedItem_Exposed = TestList.Count;
		Assert.That(dropForm.CurrentRowSelectable(TestList.Count), Is.False);
	}

	[Test]
	public async Task SelectedItemReturnsNullForEmptyList()
	{
		using var ctx = new EnterpriseTestContext();
		ZDropForm dropForm = null;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new WinzorTestForm();
			var dropEdit = new ZDropEdit();
			dropForm = new ZDropForm(dropEdit);
			dropForm.ShowDropDown(dropForm.Location, false);
			form.Controls.Add(dropEdit);
			return form;
		});

		Assert.That(dropForm.SelectedItem, Is.Null);
	}

	[TestCase(null)]
	[TestCase("ONE")]
	public async Task FirstItemReturnsFirstItemInTheList(string expected)
	{
		using var ctx = new EnterpriseTestContext();
		ZDropForm dropForm = null;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new WinzorTestForm();
			var list = expected == null ? null : TestList;
			var dropEdit = new ZDropEdit { List = list };
			dropForm = new ZDropForm(dropEdit);
			dropForm.ShowDropDown(dropForm.Location, false);
			form.Controls.Add(dropEdit);
			return form;
		});

		Assert.That(dropForm.FirstItem?.Code, Is.EqualTo(expected));
	}

	[Test]
	public async Task ZDropFormItemSelectable_WithZDropEdit()
	{
		using var ctx = new EnterpriseTestContext();
		ZDropForm dropForm = null;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new WinzorTestForm();
			var list = new CodeDescriptionPairList();
			list.AddPair(Guid.NewGuid(), "ONE", "Description 01");
			list.AddPair(Guid.NewGuid(), "TWO", "Description 02");
			var dropEdit = new ZDropEdit { List = list };
			dropForm = new ZDropForm(dropEdit);
			dropForm.ShowDropDown(dropForm.Location, false);
			form.Controls.Add(dropEdit);
			return form;
		});

		Assert.That(dropForm.IsItemSelectable(new CodeDescriptionPair("code", "description")), Is.EqualTo(true));
		Assert.That(dropForm.IsItemSelectable(null), Is.EqualTo(false));

		var items = rendered.FindAll(".zdropform__item");

		Assert.That(items.Count, Is.EqualTo(2));
		Assert.That(items[0].GetAttribute("class"), Does.Not.Contain("zdropform__item--unselectable"));
		Assert.That(items[1].GetAttribute("class"), Does.Not.Contain("zdropform__item--unselectable"));
	}

	[Test]
	public async Task ZDropFormItemSelectable_WithZFilterStripDropEdit()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new WinzorTestForm();
			var list = new CodeDescriptionPairList
			{
				new CategoryCodeDescriptionPair("Category", "This is a category")
			};
			list.AddPair(Guid.NewGuid(), "", "This is a separator");
			list.AddPair(Guid.NewGuid(), "Item", "This is normal item");

			var dropEdit = new ZFilterStripDropEdit { List = list };
			var dropForm = new ZFilterStripDropForm(dropEdit);
			dropForm.ShowDropDown(dropForm.Location, false);
			form.Controls.Add(dropEdit);
			return form;
		});

		var items = rendered.FindAll(".zdropform__item");

		Assert.That(items.Count, Is.EqualTo(3));
		Assert.That(items[0].GetAttribute("class"), Does.Contain("zdropform__item--unselectable"));
		Assert.That(items[1].GetAttribute("class"), Does.Contain("zdropform__item--unselectable"));
		Assert.That(items[2].GetAttribute("class"), Does.Not.Contain("zdropform__item--unselectable"));
	}

	[Test]
	public async Task ZDropFormKeyUpDownShouldHighlightItemInList()
	{
		using var ctx = new EnterpriseTestContext();
		ZDropForm dropForm = null;
		await ctx.RenderFormAsync(() =>
		{
			var form = new WinzorTestForm();
			var dropEdit = new ZDropEdit { List = TestList };
			dropForm = new ZDropForm(dropEdit);
			dropForm.ShowDropDown(dropForm.Location, false);
			form.Controls.Add(dropEdit);
			return form;
		});

		Assert.That(dropForm.SelectedItem, Is.Null);

		dropForm.HighlightedItem_Exposed = 0;
		Assert.That(dropForm.SelectedItem.Code, Is.EqualTo("ONE"));

		await dropForm.InvokeWinzorDispatcherAsync(() => { dropForm.HandleCommandKey(Keys.Down); });
		Assert.That(dropForm.SelectedItem.Code, Is.EqualTo("TWO"));

		await dropForm.InvokeWinzorDispatcherAsync(() => { dropForm.HandleCommandKey(Keys.Up); });
		Assert.That(dropForm.SelectedItem.Code, Is.EqualTo("ONE"));
	}

	[Test, WithPlaywrightPage]
	public async Task ZDropFormKeyUpDownScrollBehavior()
	{
		await using var ctx = new InMemoryAppServerTestContext();

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new WinzorTestForm() { Height = 300, Width = 300 };
			form.BindingSource.DataSourceType = typeof(DummyBusinessObjectWithList);
			var dropEdit = new ZDropEdit();
			dropEdit.BindToList = "List";
			form.Controls.Add(dropEdit);
			form.BindingSource.SetBindingMember(dropEdit, "Z0_Code");
			var factory = new BusinessObjectFactory();
			var dummyBizo = factory.New<DummyBusinessObjectWithList>();
			dummyBizo.Z0_Code = string.Empty;
			for (var i = 0; i < 11; i++)
			{
				dummyBizo.List.AddNew().Z0_Code = $"{i}";
			}
			form.SetDataBinding(dummyBizo, "");
			return form;
		});
		var zdropcodebox = page.Locator(".zdropcodebox");
		await zdropcodebox.PressAsync("ArrowDown");

		var zdropform = page.Locator(".zdropform");
		await zdropform.WaitForAsync();
		await Assertions.Expect(zdropform).ToHaveJSPropertyAsync("scrollTop", 0);

		// mock top to bottom scroll behavior and check each item index
		for (var i = 0; i < 11; i++)
		{
			await page.Keyboard.PressAsync("ArrowDown");
			await Assertions.Expect(zdropcodebox).ToHaveValueAsync(i.ToString());
		}
		await Assertions.Expect(zdropform).ToHaveJSPropertyAsync("scrollTop", 14);

		// mock bottom to top scroll behavior and check each item index
		for (var i = 10; i >= 0; i--)
		{
			await Assertions.Expect(zdropcodebox).ToHaveValueAsync(i.ToString());
			await page.Keyboard.PressAsync("ArrowUp");
		}
		await Assertions.Expect(zdropform).ToHaveJSPropertyAsync("scrollTop", 0);

		await page.Keyboard.PressAsync("ArrowDown");
		await page.Keyboard.PressAsync("ArrowDown");
		await page.Keyboard.PressAsync("ArrowUp");
		await Assertions.Expect(zdropform).ToHaveJSPropertyAsync("scrollTop", 0);
		await page.Keyboard.PressAsync("ArrowDown");
		await Assertions.Expect(zdropform).ToHaveJSPropertyAsync("scrollTop", 0);
	}

	[Test]
	public async Task ZDropFormPageUpDownShouldHighlightItemInList()
	{
		using var ctx = new EnterpriseTestContext();
		ZDropForm dropForm = null;
		var pageUpPageDownTestList = new CodeDescriptionPairList();

		for (var i = 0; i < 11; i++)
		{
			pageUpPageDownTestList.AddPair(i.ToString(), $"Description {i}");
		}

		await ctx.RenderFormAsync(() =>
		{
			var form = new WinzorTestForm();
			var dropEdit = new ZDropEdit { List = pageUpPageDownTestList };
			dropForm = new ZDropForm(dropEdit);
			dropForm.ShowDropDown(dropForm.Location, false);
			form.Controls.Add(dropEdit);
			return form;
		});

		Assert.That(dropForm.SelectedItem, Is.Null);

		dropForm.HighlightedItem_Exposed = 0;
		Assert.That(dropForm.SelectedItem.Code, Is.EqualTo("0"));

		await dropForm.InvokeWinzorDispatcherAsync(() => { dropForm.HandleCommandKey(Keys.PageDown); });
		Assert.That(dropForm.SelectedItem.Code, Is.EqualTo("9"));

		await dropForm.InvokeWinzorDispatcherAsync(() => { dropForm.HandleCommandKey(Keys.PageUp); });
		Assert.That(dropForm.SelectedItem.Code, Is.EqualTo("0"));
	}

	[Test, WithPlaywrightPage]
	public async Task ZDropFormPageUpDownScrollBehavior()
	{
		await using var ctx = new InMemoryAppServerTestContext();

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new WinzorTestForm() { Height = 300, Width = 300 };
			form.BindingSource.DataSourceType = typeof(DummyBusinessObjectWithList);
			var dropEdit = new ZDropEdit();
			dropEdit.BindToList = "List";
			form.Controls.Add(dropEdit);
			form.BindingSource.SetBindingMember(dropEdit, "Z0_Code");
			var factory = new BusinessObjectFactory();
			var dummyBizo = factory.New<DummyBusinessObjectWithList>();
			dummyBizo.Z0_Code = string.Empty;
			for (var i = 0; i < 11; i++)
			{
				dummyBizo.List.AddNew().Z0_Code = $"{i}";
			}
			form.SetDataBinding(dummyBizo, "");
			return form;
		});

		var zdropcodebox = page.Locator(".zdropcodebox");
		await zdropcodebox.WaitForAsync();
		await zdropcodebox.PressAsync("ArrowDown");

		var zdropform = page.Locator(".zdropform");
		await zdropform.WaitForAsync();
		await Assertions.Expect(zdropform).ToHaveJSPropertyAsync("scrollTop", 0);

		await page.Keyboard.PressAsync("PageDown");
		var input = page.Locator("input.zdropcodebox");
		await Assertions.Expect(input).ToHaveValueAsync("9");
		await Assertions.Expect(zdropform).ToHaveJSPropertyAsync("scrollTop", 0);

		await page.Keyboard.PressAsync("PageDown");
		await Assertions.Expect(input).ToHaveValueAsync("10");
		await Assertions.Expect(zdropform).ToHaveJSPropertyAsync("scrollTop", 14);

		await page.Keyboard.PressAsync("PageUp");
		await Assertions.Expect(input).ToHaveValueAsync("1");
		await Assertions.Expect(zdropform).ToHaveJSPropertyAsync("scrollTop", 14);

		await page.Keyboard.PressAsync("PageUp");
		await Assertions.Expect(input).ToHaveValueAsync("0");
		await Assertions.Expect(zdropform).ToHaveJSPropertyAsync("scrollTop", 0);
	}

	[Test, WithPlaywrightPage]
	public async Task TestZDropEditShouldHideDropFormAfterClick()
	{
		await using var ctx = new InMemoryAppServerTestContext();

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new WinzorTestForm() { Height = 500, Width = 500 };
			form.BindingSource.DataSourceType = typeof(DummyBusinessObjectWithList);
			var dropEdit = new ZDropEdit { List = TestList, Top = 0, Left = 0 };
			dropEdit.BindToList = "List";
			form.Controls.Add(dropEdit);
			form.BindingSource.SetBindingMember(dropEdit, "Z0_Code");
			var factory = new BusinessObjectFactory();
			var dummyBizo = factory.New<DummyBusinessObjectWithList>();
			dummyBizo.Z0_Code = string.Empty;

			dummyBizo.List.AddNew().Z0_Code = "ONE";

			form.SetDataBinding(dummyBizo, "");
			return form;
		});

		var zdropcodebox = page.Locator(".zdropcodebox");
		await zdropcodebox.PressAsync("ArrowDown");
		var zdropform = page.Locator(".zdropform");
		await Assertions.Expect(zdropform).ToHaveCountAsync(1);

		await page.Mouse.ClickAsync(200, 200);
		await Assertions.Expect(zdropform).ToHaveCountAsync(0);
	}

	[Test, WithPlaywrightPage]
	public async Task ZFilterStripDropEditEnterKeyShouldHideDropForm()
	{
		await using var ctx = new InMemoryAppServerTestContext();

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new WinzorTestForm() { Height = 300, Width = 300 };
			form.BindingSource.DataSourceType = typeof(DummyBusinessObjectWithList);
			var dropEdit = new ZFilterStripDropEdit();
			dropEdit.BindToList = "List";
			form.Controls.Add(dropEdit);
			form.BindingSource.SetBindingMember(dropEdit, "Z0_Code");
			var factory = new BusinessObjectFactory();
			var dummyBizo = factory.New<DummyBusinessObjectWithList>();
			dummyBizo.Z0_Code = string.Empty;

			dummyBizo.List.AddNew().Z0_Code = "ONE";

			form.SetDataBinding(dummyBizo, "");
			return form;
		});

		var zdropcodebox = page.Locator(".zdropcodebox");
		await zdropcodebox.PressAsync("ArrowDown");
		var zdropform = page.Locator(".zdropform");
		await Assertions.Expect(zdropform).ToHaveCountAsync(1);

		await page.Keyboard.PressAsync("Enter");
		await Assertions.Expect(zdropform).ToHaveCountAsync(0);
	}

	[Test, WithPlaywrightPage]
	public async Task ZDropFormStaysWithinBoundaries()
	{
		await using var ctx = new InMemoryAppServerTestContext();

		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var loremIpsum1 = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Proin tempus rhoncus ornare.";
			var loremIpsum2 = "Donec vestibulum, tortor quis tristique rhoncus, sapien lorem sodales sapien, nec faucibus velit est vitae enim.";

			var list = new CodeDescriptionPairList
			{
				new CodeDescriptionPair("Apple", "Description 01"),
				new CodeDescriptionPair(loremIpsum1, loremIpsum2),
				new CodeDescriptionPair(loremIpsum1, loremIpsum2),
				new CodeDescriptionPair(loremIpsum1, loremIpsum2),
				new CodeDescriptionPair(loremIpsum1, loremIpsum2),
				new CodeDescriptionPair(loremIpsum1, loremIpsum2),
				new CodeDescriptionPair(loremIpsum1, loremIpsum2),
				new CodeDescriptionPair(loremIpsum1, loremIpsum2),
				new CodeDescriptionPair(loremIpsum1, loremIpsum2),
				new CodeDescriptionPair(loremIpsum1, loremIpsum2),
				new CodeDescriptionPair(loremIpsum1, loremIpsum2),
				new CodeDescriptionPair(loremIpsum1, loremIpsum2),
				new CodeDescriptionPair(loremIpsum1, loremIpsum2),
				new CodeDescriptionPair(loremIpsum1, loremIpsum2),
			};

			var dropEdit = new ZDropEdit { List = list, Top = 0, Left = 0 };
			var dropForm = new ZDropForm(dropEdit);
			dropForm.ShowDropDown(dropForm.Location, true);
			return dropEdit;
		});

		var zdropform = page.Locator(".zdropform");
		await zdropform.WaitForAsync();

		var zdropform_top = await zdropform.EvaluateAsync<int>("e => e.getBoundingClientRect().top");
		var zdropform_left = await zdropform.EvaluateAsync<int>("e => e.getBoundingClientRect().left");
		var zdropform_width = await zdropform.EvaluateAsync<int>("e => e.getBoundingClientRect().width");
		var zdropform_height = await zdropform.EvaluateAsync<int>("e => e.getBoundingClientRect().height");

		Assert.That(zdropform_top, Is.GreaterThanOrEqualTo(0));
		Assert.That(zdropform_left, Is.GreaterThanOrEqualTo(0));
		Assert.That(zdropform_left + zdropform_width, Is.LessThanOrEqualTo(300));
		Assert.That(zdropform_top + zdropform_height, Is.LessThanOrEqualTo(300));

		var firstRowFirstCell = page.Locator(".zdropform tr:nth-child(1) td:first-child");
		await firstRowFirstCell.WaitForAsync();
		var secondRowFirstCell = page.Locator(".zdropform tr:nth-child(2) td:first-child");
		await secondRowFirstCell.WaitForAsync();

		var firstRowFirstCell_width = await firstRowFirstCell.EvaluateAsync<int>("e => e.getBoundingClientRect().width");
		var secondRowFirstCell_width = await secondRowFirstCell.EvaluateAsync<int>("e => e.getBoundingClientRect().width");

		Assert.That(firstRowFirstCell_width, Is.EqualTo(secondRowFirstCell_width));
	}

	[Test, WithPlaywrightPage]
	public async Task ZDropFormWidth_TinyCodeAndDescription()
	{
		await using var ctx = new InMemoryAppServerTestContext();

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form { Height = 300, Width = 800 };
			var list = new CodeDescriptionPairList
			{
				new CodeDescriptionPair("A", "A Desc"),
				new CodeDescriptionPair("B", "B Desc"),
				new CodeDescriptionPair("C", "C Desc"),
				new CodeDescriptionPair("D", "D Desc"),
			};

			var dropEdit = new ZDropEdit { List = list, Top = 0, Left = 0, UseFullWidthForCodeBox = true, Width = 400 };
			var dropForm = new ZDropForm(dropEdit);
			form.Controls.Add(dropEdit);
			dropForm.ShowDropDown(dropForm.Location, true);
			return form;
		});

		var zdropform = page.Locator(".zdropform");
		await zdropform.WaitForAsync();

		var zdropform_width = await zdropform.EvaluateAsync<int>("e => e.getBoundingClientRect().width");

		Assert.That(zdropform_width, Is.EqualTo(400));
	}

	[Test, WithPlaywrightPage]
	public async Task ZDropFormWidth_EmptyList()
	{
		await using var ctx = new InMemoryAppServerTestContext();

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form { Height = 300, Width = 800 };
			var list = new CodeDescriptionPairList
			{
			};

			var dropEdit = new ZDropEdit { List = list, Top = 0, Left = 0, UseFullWidthForCodeBox = true, Width = 400 };
			var dropForm = new ZDropForm(dropEdit);
			form.Controls.Add(dropEdit);
			dropForm.ShowDropDown(dropForm.Location, true);
			return form;
		});

		var zDropForm = page.Locator(".zdropform");
		await zDropForm.WaitForAsync();
		var zDropFormWidth = await zDropForm.EvaluateAsync<int>("e => e.getBoundingClientRect().width");

		Assert.That(zDropFormWidth, Is.EqualTo(400));
	}

	[Test, WithPlaywrightPage]
	public async Task ZDropFormWidth_WithScrollBarWidth()
	{
		await using var ctx = new InMemoryAppServerTestContext();

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form { Height = 300, Width = 500 };
			var list = new CodeDescriptionPairList
			{
				new CodeDescriptionPair("Code", "Long Description"),
				new CodeDescriptionPair("Code", "Description"),
				new CodeDescriptionPair("Code", "Description"),
				new CodeDescriptionPair("Code", "Description")
			};

			var dropEdit = new ZDropEdit { List = list, Top = 0, MaxItemsToShowInDropDown = 3 };
			var dropForm = new ZDropForm(dropEdit);
			form.Controls.Add(dropEdit);
			dropForm.ShowDropDown(dropForm.Location, true);

			var dropEdit2 = new ZDropEdit { List = list, Top = 100, MaxItemsToShowInDropDown = 8 };
			var dropForm2 = new ZDropForm(dropEdit2);
			form.Controls.Add(dropEdit2);
			dropForm2.ShowDropDown(dropForm2.Location, true);
			return form;
		});

		var dropForms = page.Locator(".zdropform");
		await dropForms.Nth(0).WaitForAsync();
		await dropForms.Nth(1).WaitForAsync();
		var width1 = await dropForms.Nth(0).EvaluateAsync<int>("e => e.getBoundingClientRect().width");
		var width2 = await dropForms.Nth(1).EvaluateAsync<int>("e => e.getBoundingClientRect().width");
		Assert.That(width1, Is.GreaterThan(width2));
	}

	[Test, WithPlaywrightPage]
	public async Task ZDropFormWidth_LongCodeOrDesc([Values] bool longCode)
	{
		await using var ctx = new InMemoryAppServerTestContext();

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form { Height = 300, Width = 800 };
			var longs = new[]
			{
				"Normal text",
				"A rather long text that shouldn't wrap",
				"Another Normal text",
				"A very long text that should not wrap around and should remain in one line",
			};

			var shorts = new[]
			{
				"A Text",
				"B Text",
				"C Text",
				"D Text",
			};

			var list = new CodeDescriptionPairList
			{
				new CodeDescriptionPair(longCode ? longs[0] : shorts[0], !longCode ? longs[0] : shorts[0]),
				new CodeDescriptionPair(longCode ? longs[1] : shorts[1], !longCode ? longs[1] : shorts[1]),
				new CodeDescriptionPair(longCode ? longs[2] : shorts[2], !longCode ? longs[2] : shorts[2]),
				new CodeDescriptionPair(longCode ? longs[3] : shorts[3], !longCode ? longs[3] : shorts[3]),
			};

			var dropEdit = new ZDropEdit { List = list, Top = 0, Left = 0, UseFullWidthForCodeBox = true, Width = 100 };
			var dropForm = new ZDropForm(dropEdit);
			form.Controls.Add(dropEdit);
			dropForm.ShowDropDown(dropForm.Location, true);
			return form;
		});

		var zdropform = page.Locator(".zdropform");
		await zdropform.WaitForAsync();

		var zdropform_width = await zdropform.EvaluateAsync<int>("e => e.getBoundingClientRect().width");

		Assert.That(zdropform_width, Is.GreaterThan(400));
	}

	[Test, WithPlaywrightPage]
	public async Task ZDropEditEnterKeyShouldHideDropForm()
	{
		await using var ctx = new InMemoryAppServerTestContext();

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new WinzorTestForm() { Height = 300, Width = 300 };
			form.BindingSource.DataSourceType = typeof(DummyBusinessObjectWithList);
			var dropEdit = new ZDropEdit();
			dropEdit.BindToList = "List";
			form.Controls.Add(dropEdit);
			form.Controls.Add(new TextBox());
			form.BindingSource.SetBindingMember(dropEdit, "Z0_Code");
			var factory = new BusinessObjectFactory();
			var dummyBizo = factory.New<DummyBusinessObjectWithList>();
			dummyBizo.Z0_Code = string.Empty;

			dummyBizo.List.AddNew().Z0_Code = "ONE";

			form.SetDataBinding(dummyBizo, "");
			return form;
		});

		var zdropcodebox = page.Locator(".zdropcodebox");
		await zdropcodebox.PressAsync("ArrowDown");
		var zdropform = page.Locator(".zdropform");
		await Assertions.Expect(zdropform).ToHaveCountAsync(1);

		await page.Keyboard.PressAsync("Enter");
		await Assertions.Expect(zdropform).ToHaveCountAsync(0);
	}

	class DummyBusinessObjectWithListNotThreadSafe : DummyBusinessObject
	{
		public DummyBusinessObjectWithListNotThreadSafe(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
		DummyBusinessObjectListNotThreadSafe list;

		public DummyBusinessObjectListNotThreadSafe List
		{
			get
			{
				if (list == null)
				{
					list = new DummyBusinessObjectListNotThreadSafe(base.Factory);
				}

				return list;
			}
		}
	}

	class DummyBusinessObjectListNotThreadSafe : BusinessObjectCollection<DummyBusinessObjectNotThreadSafe>
	{
		public new DummyBusinessObjectNotThreadSafe this[int i] => (DummyBusinessObjectNotThreadSafe)base.Elements[i];

		public DummyBusinessObjectListNotThreadSafe(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}

	class DummyBusinessObjectNotThreadSafe : DummyBusinessObjectInList
	{
		public DummyBusinessObjectNotThreadSafe(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public override ZString Z0_Code
		{
			get
			{
				CheckThread();
				return base.Z0_Code;
			}
			set
			{
				base.Z0_Code = value;
			}
		}

		void CheckThread()
		{
			if (System.Environment.CurrentManagedThreadId != DispatcherThreadId)
			{
				throw new InvalidOperationException("The current thread is not the WinzorDispatcher thread");
			}
		}

		public int DispatcherThreadId { get; set; }
	}

	[Test, WithPlaywrightPage]
	public async Task UnderlyingDataObjectsNotAccessedByRenderThread()
	{
		var itemCount = 10;
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var factory = new BusinessObjectFactory();
			var dummyBizo = factory.New<DummyBusinessObjectWithListNotThreadSafe>();
			for (int i = 0; i < itemCount; i++)
			{
				var a = dummyBizo.List.AddNew();
				a.DispatcherThreadId = ctx.WinzorDispatcher.ManagedThreadId;
				a.Z0_Code = i.ToString();
				a.Z0_Description = "Description " + i;
			}

			var form = new ZForm();
			var dropEdit = new ZDropEdit();
			dropEdit.BindToList = "List";
			dropEdit.SetDataBinding(dummyBizo, AutoDummyBizo.Schema.Z0_Code);
			form.Controls.Add(dropEdit);
			return form;
		});

		await page.Locator(".zdropbutton button").ClickAsync();
		await Assertions.Expect(page.Locator(".zdropform__item")).ToHaveCountAsync(itemCount);
	}

	[Test, WithPlaywrightPage]
	public async Task ZDropFrom_ShouldUpdateHighlightAsTextUpdated()
	{
		await using var ctx = new InMemoryAppServerTestContext();

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new WinzorTestForm() { Height = 300, Width = 300 };
			form.BindingSource.DataSourceType = typeof(DummyBusinessObjectWithList);
			var dropEdit = new ZDropEdit();
			dropEdit.BindToList = "List";
			form.Controls.Add(dropEdit);
			form.BindingSource.SetBindingMember(dropEdit, "Z0_Code");
			var factory = new BusinessObjectFactory();
			var dummyBizo = factory.New<DummyBusinessObjectWithList>();
			dummyBizo.Z0_Code = string.Empty;

			dummyBizo.List.AddNew().Z0_Code = "ONE";

			form.SetDataBinding(dummyBizo, "");
			return form;
		});

		await page.Locator("button", new PageLocatorOptions { HasText = "keyboard_arrow_down" }).ClickAsync();
		var selectedItem = page.Locator(".selected");
		await Assertions.Expect(selectedItem).ToHaveCountAsync(0);

		await page.Locator(".zdropcodebox").PressAsync("O");
		await Assertions.Expect(selectedItem).ToHaveCountAsync(1);
	}

	[Test]
	public async Task ZDropFormSeparatorCorrectlyApplied_WithZDropEdit([Values("blah", "")] string itemCode, [Values("blah", "")] string itemDescription)
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new WinzorTestForm();
			var list = new CodeDescriptionPairList();
			list.AddPair(Guid.NewGuid(), itemCode, itemDescription);
			list.AddPair(Guid.NewGuid(), "TWO", "Description 02");

			var dropEdit = new ZDropEdit { List = list };
			var dropForm = new ZDropForm(dropEdit);
			dropForm.ShowDropDown(dropForm.Location, false);
			form.Controls.Add(dropEdit);
			return form;
		});

		var items = rendered.FindAll(".zdropform tr");

		Assert.That(items.Count, Is.EqualTo(2));
		Assert.That(items[0].GetAttribute("class"), Does.Not.Contain("zdropform__item--separator"));
	}

	[TestCase("blah", "", false)]
	[TestCase("blah", "blah", false)]
	[TestCase("", "", true)]
	[TestCase("", "blah", true)]
	public async Task ZDropFormSeparatorCorrectlyApplied_WithZFilterStripDropEdit(string itemCode, string itemDescription, bool isSeparator)
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new WinzorTestForm();
			var list = new CodeDescriptionPairList();
			list.AddPair(Guid.NewGuid(), itemCode, itemDescription);
			list.AddPair(Guid.NewGuid(), "TWO", "Description 02");

			var dropEdit = new ZFilterStripDropEdit { List = list };
			var dropForm = new ZFilterStripDropForm(dropEdit);
			dropForm.ShowDropDown(dropForm.Location, false);
			form.Controls.Add(dropEdit);
			return form;
		});

		var items = rendered.FindAll(".zdropform tr");

		Assert.That(items.Count, Is.EqualTo(2));
		Assert.That(items[0].GetAttribute("class").Contains("zdropform__item--separator"), Is.EqualTo(isSeparator));
	}

	[WithPlaywrightPage]
	[TestCase(ZDropEdit.ShowInDropDownList.ShowCodeAndDescription, "6px", 2)]
	[TestCase(ZDropEdit.ShowInDropDownList.OnlyShowDescription, "0px", 1)]
	[TestCase(ZDropEdit.ShowInDropDownList.OnlyShowCode, "0px", 1)]
	public async Task DescriptionAndCodeColumnPadding(ZDropEdit.ShowInDropDownList showInDropDown, string padding, int expectedSpanCount)
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();

			var list = new CodeDescriptionPairList
			{
				new CodeDescriptionPair("Code", "Description"),
			};

			var dropEdit = new ZDropEdit { List = list, Top = 0, Left = 0, Width = 100, ShowInDropDown = showInDropDown };
			var dropForm = new ZDropForm(dropEdit);
			dropForm.ShowDropDown(dropForm.Location, false);

			form.Controls.Add(dropEdit);
			return form;
		});

		var zDropFormItem = page.Locator(".zdropform__item td div span");
		await Assertions.Expect(zDropFormItem).ToHaveCountAsync(expectedSpanCount);
		var targetColumn = zDropFormItem.Nth(expectedSpanCount - 1);
		await Assertions.Expect(targetColumn).ToHaveCSSAsync("padding-left", padding);
	}

	[Test, WithPlaywrightPage]
	public async Task DescriptionAndCodeColumnPaddingWhenHasCategory()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();

			var list = new CodeDescriptionPairList
			{
				new CodeDescriptionPair("Code", "Description"),
			};

			var dropEdit = new ZFilterStripDropEdit { List = list };
			var dropForm = new ZFilterStripDropForm(dropEdit);
			dropForm.ShowDropDown(dropForm.Location, false);
			form.Controls.Add(dropEdit);
			return form;
		});

		var zDropFormItem = page.Locator(".zdropform__item td div span");
		await Assertions.Expect(zDropFormItem).ToHaveCountAsync(2);
		var targetColumn = zDropFormItem.Nth(1);
		await Assertions.Expect(targetColumn).ToHaveCSSAsync("padding-left", "6px");
	}

	[Test, WithPlaywrightPage]
	public async Task RenderItemsWithVirtualize()
	{
		const int count = 2000;
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var list = new CodeDescriptionPairList();
			list.AddRange(Enumerable.Range(0, count).Select(i => new CodeDescriptionPair($"Code{i}", $"Description{i}")).ToArray());
			var dropEdit = new ZDropEdit { List = list };
			var dropForm = new ZDropForm(dropEdit);
			dropForm.ShowDropDown(dropForm.Location, false);
			return dropEdit;
		});

		var zdropform = page.Locator(".zdropform");
		var items = zdropform.Locator(".zdropform__item");
		await Assertions.Expect(items).ToHaveCountAsync(24);
		await Assertions.Expect(items.First).ToHaveTextAsync("Code0Description0");
		await Assertions.Expect(items.Last).ToHaveTextAsync("Code23Description23");

		await zdropform.EvaluateAsync($"e => e.scrollTop = {14 * 24}");
		await Assertions.Expect(items).ToHaveCountAsync(24);
		await Assertions.Expect(items.First).ToHaveTextAsync("Code17Description17");
		await Assertions.Expect(items.Last).ToHaveTextAsync("Code40Description40");
	}

	[Test]
	public async Task PreloadDropFormJSInterop()
	{
		using var ctx = new EnterpriseTestContext();
		var interop = new Mock<IDropFormJSInterop>();
		interop.Setup(i => i.PreloadInterop());
		ctx.Services.AddScoped(_ => interop.Object);

		await ctx.RenderControlOnFormAsync(() =>
		{
			var dropEdit = new ZFilterStripDropEdit
			{
				List = new CodeDescriptionPairList()
				{
					new CodeDescriptionPair("Code", "Description"),
				}
			};
			var dropForm = new ZFilterStripDropForm(dropEdit);
			dropForm.ShowDropDown(dropForm.Location, false);
			return dropEdit;
		});
		interop.Verify(e => e.PreloadInterop(), Times.Once());
	}

	[Test, WithPlaywrightPage]
	public async Task CorrectItemIsChosenForItemsWithSameCode_ZFilterStripDropEdit()
	{
		await using var ctx = new InMemoryAppServerTestContext();

		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var list = new CodeDescriptionPairList
			{
				new CategoryCodeDescriptionPair("Hello", "Non-selectable"),
				new CodeDescriptionPair("Hello", "Selectable"),
				new CodeDescriptionPair("Hello", "Another Selectable"),
				new CodeDescriptionPair("Hello", "Selectable Again"),
			};

			var dropEdit = new ZFilterStripDropEdit { List = list };
			var dropForm = new ZDropForm(dropEdit);
			dropForm.ShowDropDown(dropForm.Location, true);
			return dropEdit;
		});

		await page.Locator(".zdropform__item").Nth(2).ClickAsync();

		var descriptionBox = page.Locator("input").Nth(1);
		await Assertions.Expect(descriptionBox).ToHaveValueAsync("Another Selectable");
	}

	[Test, WithPlaywrightPage]
	public async Task DropFormScrollToWithElementReferenceNotSetDoesNotThrow()
	{
		DummyZDropForm dropForm = null;
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var pair = new CodeDescriptionPair("Hello", "Kitty");
			var list = new CodeDescriptionPairList { pair };
			var dropEdit = new ZFilterStripDropEdit { List = list };
			dropForm = new DummyZDropForm(dropEdit);
			return dropEdit;
		});

		dropForm.SetElementReference(default);
		await page.Locator(".zdropbutton button").ClickAsync();
		await Assertions.Expect(page.Locator(".zdropform")).ToBeVisibleAsync();
	}

	#region DummyZDropForm

	class DummyZDropForm : ZDropForm
	{
		public DummyZDropForm(IDropFormParent parent) : base(parent) { }

		public int CodeWidthExposed => base.CodeWidth;

		public bool CurrentRowSelectable(int row) => base.IsCurrentRowSelectable(row);

		public void SetElementReference(ElementReference elementReference)
		{
			base.ElementReference = elementReference;
		}
	}

	#endregion
}
