using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using AngleSharp.Css.Dom;
using AngleSharp.Dom;
using Bunit;
using CargoWise.Blazor.Common;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.DocumentScanning.Business;
using Enterprise.DocumentScanning.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Schema;
using Microsoft.AspNetCore.Components;
using Microsoft.Playwright;
using NUnit.Framework;
using WinzorFramework;
using WinzorTestFramework;
using WTG.PlaywrightTesting;

namespace Enterprise.Winzor.Architecture.Test;

using static PlaywrightTestContext;

class ZDropEditTest
{
	static readonly IEnumerable<string> dropEditBackgroundSource = new[] { null, "#ffffff", "#ececec" };

	[Test]
	public async Task FirstSelectionHasNoError()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var factory = new BusinessObjectFactory();
			var docFactory = new DbBackendDocumentFactory(factory);
			var numFactory = new NumberedBusinessObjectFactory(1, docFactory);
			var file = StorageFile.NewWithParent_DEBUG(numFactory);
			return new EditPropertiesFileForm(file);
		});

		await rendered.Find(".zdropbutton button").MouseDownAsync(new WebMouseEventArgs() { Detail = 1 });
		await rendered.Find(".zdropform tr").ClickAsync(new WebMouseEventArgs());
		Assert.That(rendered.FindAll("input")[1].Attributes["value"].Value, Is.Not.Empty);
	}

	[Test]
	public async Task ZDropButtonRender()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() => new ZDropEdit());

		Assert.That(rendered.Find(".zdropbutton button").InnerHtml, Is.EqualTo("keyboard_arrow_down"));
		Assert.That(rendered.Find(".zdropbutton button").Attributes["data-winzor-control-id"].Value, Is.Not.Empty);
		Assert.That(rendered.FindAll(".zdropform").Count, Is.EqualTo(0));
	}

	[Test, WithPlaywrightPage]
	public async Task InvokeCloseFormWithADropEditShouldNotThrowException()
	{
		WinzorTestForm form = null;
		var clientServiceProvider = new MockCargoWiseClientSeviceProvider();
		await using var ctx = new InMemoryAppServerTestContext(clientServiceProvider);
		var exceptionThrown = false;
		TaskScheduler.UnobservedTaskException += UnobservedHandler;

		var page = await ctx.LoadFormAsync(() =>
		{
			form = new WinzorTestForm();
			var dropEdit = new ZDropEdit();
			form.Controls.Add(dropEdit);
			return form;
		});

		await form.InvokeWinzorDispatcherAsync(() => form.Close());

		GC.Collect();
		GC.WaitForPendingFinalizers();
		TaskScheduler.UnobservedTaskException -= UnobservedHandler;

		Assert.That(exceptionThrown, Is.False);

		void UnobservedHandler(object sender, UnobservedTaskExceptionEventArgs args)
		{
			exceptionThrown = true;
		}
	}

	[Test]
	public async Task ZDropFormRenderEmpty()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new WinzorTestForm();
			var dropEdit = new ZDropEdit();
			form.Controls.Add(dropEdit);
			return form;
		});

		Assert.That(rendered.FindAll(".zdropform").Count, Is.EqualTo(0));
	}

	[TestCase("common", "Category1", "Desc Category1")]
	[TestCase("timeCategory", "Category1", "")]
	[TestCase("category", "Category1", "Desc Category1")]
	[TestCase("emptyCategory", "", "Foo")]
	[TestCase("emptyDescription", "Category1", "")]
	public async Task ZDropFormItemsHaveCorrectClassAndContent(string firstRowType, string firstRowCode, string firstRowDescription)
	{
		using var ctx = new EnterpriseTestContext();
		ZDropEdit dropEdit = null;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new WinzorTestForm();
			var list = new CodeDescriptionPairList();
			if (firstRowType.ToLower().Contains("category"))
			{
				list.Add(new CategoryCodeDescriptionPair(firstRowCode, firstRowDescription));
			}
			else
			{
				list.AddPair(Guid.NewGuid(), firstRowCode, firstRowDescription);
			}

			list.AddPair(Guid.NewGuid(), "ONE", "Description 01");
			list.AddPair(Guid.NewGuid(), "TWO", "Description 02");
			list.AddPair(Guid.NewGuid(), "THREE", "Description 03");
			list.AddPair(Guid.NewGuid(), "", "");

			dropEdit = firstRowType.Equals("common") ? new ZDropEdit { List = list, DisableInvalidation = true } : new ZFilterStripDropEdit { List = list, DisableInvalidation = true };
			dropEdit.ShowDropDown();
			form.Controls.Add(dropEdit);
			return form;
		});

		Assert.That(rendered.FindAll(".zdropform tr").Count, Is.EqualTo(firstRowType.Equals("common") ? 5 : 4));

		// Test the content of the first row
		Assert.That(rendered.Find(".zdropform tr:nth-child(1) td:first-child div span").InnerHtml, Is.EqualTo(firstRowType.Equals("emptyCategory") ? string.Empty : firstRowCode));
		Assert.That(rendered.Find(".zdropform tr:nth-child(1) td:nth-child(2) div span").InnerHtml, Is.EqualTo(firstRowDescription));

		// Test the class name of the first row
		switch (firstRowType)
		{
			case "category":
				Assert.That(rendered.Find(".zdropform tr:nth-child(1)").Attributes["class"]?.Value, Is.EqualTo("zdropform__item zdropform__item--unselectable zdropform__item--category"));
				break;
			case "timeCategory":
				Assert.That(rendered.Find(".zdropform tr:nth-child(1)").Attributes["class"]?.Value, Is.EqualTo("zdropform__item zdropform__item--unselectable zdropform__item--category"));
				break;
			case "emptyCategory":
				Assert.That(rendered.Find(".zdropform tr:nth-child(1)").Attributes["class"]?.Value, Is.EqualTo("zdropform__item zdropform__item--unselectable zdropform__item--separator"));
				break;
			case "emptyDescription":
				Assert.That(rendered.Find(".zdropform tr:nth-child(1)").Attributes["class"]?.Value, Is.EqualTo("zdropform__item"));
				break;
			default:
				Assert.That(rendered.Find(".zdropform tr:nth-child(1)").Attributes["class"]?.Value, Is.EqualTo("zdropform__item"));
				break;
		}

		// Test the content of other rows
		Assert.That(rendered.Find(".zdropform tr:nth-child(2)").Attributes["class"]?.Value, Is.EqualTo("zdropform__item"));
		Assert.That(rendered.Find(".zdropform tr:nth-child(2) td:first-child div span").InnerHtml, Is.EqualTo("ONE"));
		Assert.That(rendered.Find(".zdropform tr:nth-child(2) td:nth-child(2) div span").InnerHtml, Is.EqualTo("Description 01"));

		Assert.That(rendered.Find(".zdropform tr:nth-child(3)").Attributes["class"]?.Value, Is.EqualTo("zdropform__item"));
		Assert.That(rendered.Find(".zdropform tr:nth-child(3) td:first-child div span").InnerHtml, Is.EqualTo("TWO"));
		Assert.That(rendered.Find(".zdropform tr:nth-child(3) td:nth-child(2) div span").InnerHtml, Is.EqualTo("Description 02"));

		Assert.That(rendered.Find(".zdropform tr:nth-child(4) td:first-child div span").InnerHtml, Is.EqualTo("THREE"));
		Assert.That(rendered.Find(".zdropform tr:nth-child(4) td:nth-child(2) div span").InnerHtml, Is.EqualTo("Description 03"));
		Assert.That(rendered.Find(".zdropform tr:nth-child(4)").Attributes["class"]?.Value, Is.EqualTo("zdropform__item"));

		if (firstRowType == "common")
		{
			Assert.That(rendered.Find(".zdropform tr:nth-child(5) td:first-child div span").InnerHtml, Is.Empty);
			Assert.That(rendered.Find(".zdropform tr:nth-child(5) td:nth-child(2) div span").InnerHtml, Is.Empty);
		}

		Assert.That(rendered.Find(".zdropform").Attributes["data-winzor-control-id"]?.Value, Is.EqualTo(dropEdit.DropButton.DropDown.WinzorControlGuid.ToString("N")));
	}

	[Test]
	public async Task ZDropFormShouldHaveCorrectColumn([Values] ZDropEdit.ShowInDropDownList showType)
	{
		using var ctx = new EnterpriseTestContext();
		var list = new CodeDescriptionPairList();

		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new WinzorTestForm();

			list.AddPair(Guid.NewGuid(), "ONE", "Description 01");
			list.AddPair(Guid.NewGuid(), "TWO", "Description 02");
			list.AddPair(Guid.NewGuid(), "THREE", "Description 03");

			var dropEdit = new ZDropEdit { List = list, ShowInDropDown = showType, DisableInvalidation = true };
			dropEdit.ShowDropDown();
			form.Controls.Add(dropEdit);
			return form;
		});

		var trList = rendered.FindAll(".zdropform tr");
		Assert.That(trList.Count, Is.EqualTo(list.Count));

		for (var i = 0; i < trList.Count; i++)
		{
			var trNode = trList[i];
			var tdTexts = trNode.QuerySelectorAll("span").Select(m => m.Text()).ToList();
			var codeDescriptionPair = list[i];
			switch (showType)
			{
				case ZDropEdit.ShowInDropDownList.ShowCodeAndDescription:
					Assert.That(tdTexts.Count, Is.EqualTo(2));
					Assert.That(tdTexts[0], Is.EqualTo(codeDescriptionPair.Code));
					Assert.That(tdTexts[1], Is.EqualTo(codeDescriptionPair.Description));
					break;
				case ZDropEdit.ShowInDropDownList.OnlyShowCode:
					Assert.That(tdTexts.Count, Is.EqualTo(1));
					Assert.That(tdTexts[0], Is.EqualTo(codeDescriptionPair.Code));
					break;
				case ZDropEdit.ShowInDropDownList.OnlyShowDescription:
					Assert.That(tdTexts.Count, Is.EqualTo(1));
					Assert.That(tdTexts[0], Is.EqualTo(codeDescriptionPair.Description));
					break;
			}
		}
	}

	static object[] ZDropFormHeightStyleTestCases => new object[] {
		new object[] { 18, 254 },
		new object[] { 20, 282 },
	};

	[TestCaseSource(nameof(ZDropFormHeightStyleTestCases))]
	public async Task ZDropFormHeightStyle(int maxItemsToShow, int expectedHeight)
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var list = new CodeDescriptionPairList();
			for (int i = 0; i < 20; i++)
			{
				list.AddPair(Guid.NewGuid(), i.ToString(), i.ToString());
			}

			var form = new WinzorTestForm() { Height = 500 };
			var dropEdit = new ZDropEdit();
			dropEdit.DropButton.Width = 135;
			dropEdit.MaxItemsToShowInDropDown = maxItemsToShow;
			dropEdit.List = list;
			dropEdit.DisableInvalidation = true;
			dropEdit.ShowDropDown();
			form.Controls.Add(dropEdit);
			return form;
		});

		var sizeStyleString = rendered.Find(".zdropform").GetAttribute("style");
		Assert.That(sizeStyleString, Does.Contain($"width:135px;height:{expectedHeight}px;"));
	}

	[Test]
	public async Task ZDropFormHighlightedItem()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new WinzorTestForm();
			var list = new CodeDescriptionPairList();
			list.AddPair(Guid.NewGuid(), "ONE", "Description 01");
			list.AddPair(Guid.NewGuid(), "TWO", "Description 02");
			list.AddPair(Guid.NewGuid(), "THREE", "Description 03");
			list.AddPair(Guid.NewGuid(), "", "");
			var dropEdit = new ZDropEdit { List = list, DisableInvalidation = true };
			dropEdit.SelectItem("THREE");
			dropEdit.ShowDropDown();
			form.Controls.Add(dropEdit);
			return form;
		});

		var item1CodeElement = rendered.Find(".zdropform tr:nth-child(1) td:first-child");
		Assert.That(item1CodeElement.TextContent, Is.EqualTo("ONE"));
		Assert.That(item1CodeElement.ClassList, Does.Not.Contain("selected"));
		var item1DescriptionElement = rendered.Find(".zdropform tr:nth-child(1) td:nth-child(2)");
		Assert.That(item1DescriptionElement.TextContent, Is.EqualTo("Description 01"));
		Assert.That(item1DescriptionElement.ClassList, Does.Not.Contain("selected__description"));

		var item2CodeElement = rendered.Find(".zdropform tr:nth-child(2) td:first-child");
		Assert.That(item2CodeElement.TextContent, Is.EqualTo("TWO"));
		Assert.That(item2CodeElement.ClassList, Does.Not.Contain("selected"));
		var item2DescriptionElement = rendered.Find(".zdropform tr:nth-child(2) td:nth-child(2)");
		Assert.That(item2DescriptionElement.TextContent, Is.EqualTo("Description 02"));
		Assert.That(item2DescriptionElement.ClassList, Does.Not.Contain("selected__description"));

		var item3CodeElement = rendered.Find(".zdropform tr:nth-child(3) td:first-child");
		Assert.That(item3CodeElement.TextContent, Is.EqualTo("THREE"));
		Assert.That(item3CodeElement.ClassList, Does.Contain("selected"));
		var item3DescriptionElement = rendered.Find(".zdropform tr:nth-child(3) td:nth-child(2)");
		Assert.That(item3DescriptionElement.TextContent, Is.EqualTo("Description 03"));
		Assert.That(item3DescriptionElement.ClassList, Does.Contain("selected__description"));
	}

	[Test, WithPlaywrightPage]
	public async Task ZDropFormHighlightedItemScroll()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var itemNumber = 15;
		var maxItemsToShow = 10;
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new WinzorTestForm();
			form.BindingSource.DataSourceType = typeof(DummyBusinessObjectWithList);
			var dropEdit = new ZDropEdit();
			dropEdit.MaxItemsToShowInDropDown = maxItemsToShow;
			dropEdit.BindToList = "List";
			form.Controls.Add(dropEdit);
			form.BindingSource.SetBindingMember(dropEdit, "Z0_Code");
			var factory = new BusinessObjectFactory();
			var dummyBizo = factory.New<DummyBusinessObjectWithList>();

			for (int i = 0; i < itemNumber; i++)
			{
				var a = dummyBizo.List.AddNew();
				a.Z0_Code = i.ToString();
				a.Z0_Description = "Description " + i;
			}

			dummyBizo.Z0_Code = "13";
			form.SetDataBinding(dummyBizo, "");

			return form;
		});

		var zDropButton = page.Locator(".zdropbutton button");
		await zDropButton.ClickAsync();

		var highlighted = page.Locator(".selected");
		await highlighted.WaitForAsync();
		var itemHeight = await highlighted.EvaluateAsync<int>("e => e.getBoundingClientRect().height");

		// Offset on the top is 20px, getBoundingClientRect() returns a decimal point, the rounding varies +-1 px
		Assert.That(await highlighted.EvaluateAsync<int>("e => e.getBoundingClientRect().bottom"), Is.InRange(itemHeight * maxItemsToShow + 19, itemHeight * maxItemsToShow + 21));
	}

	[Test, WithPlaywrightPage]
	public async Task CodeBoxShouldNotFocusedWhenSetSelection()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		DummyBusinessObjectWithList dummyBizo = null;
		ZDropEdit dropEdit = null;
		Button button = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new WinzorTestForm();
			button = new Button() { Dock = DockStyle.Top, Text = "test button", Size = new Size(200, 50) };
			form.BindingSource.DataSourceType = typeof(DummyBusinessObjectWithList);
			dropEdit = new ZDropEdit() { BindToList = "List", Dock = DockStyle.Fill };
			form.Controls.Add(dropEdit);
			form.Controls.Add(button);
			form.BindingSource.SetBindingMember(dropEdit, "Z0_Code");

			var factory = new BusinessObjectFactory();
			dummyBizo = factory.New<DummyBusinessObjectWithList>();

			for (int i = 0; i < 5; i++)
			{
				var a = dummyBizo.List.AddNew();
				a.Z0_Code = i.ToString();
				a.Z0_Description = "Description " + i;
			}

			form.SetDataBinding(dummyBizo, "");
			return form;
		});

		await button.InvokeWinzorDispatcherAsync(() => button.Focus());
		await Assertions.Expect(page.Locator("button", new() { HasText = "test button" })).ToBeFocusedAsync();

		await dropEdit.InvokeWinzorDispatcherAsync(() => dropEdit.SetSelection(dummyBizo.List[2], 0));
		await Assertions.Expect(page.Locator("button", new() { HasText = "test button" })).ToBeFocusedAsync();

		await Assertions.Expect(page.Locator(".zdropcodebox")).Not.ToBeFocusedAsync();
	}

	[Test]
	public async Task ZDropEditWithBinding()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new WinzorTestForm();
			form.BindingSource.DataSourceType = typeof(RefPackType);
			var dropedit = new ZDropEdit();
			dropedit.Size = new Size(300, 20);
			form.Controls.Add(dropedit);
			form.BindingSource.SetBindingMember(dropedit, "F3_UnitOfDimension");
			form.DataSourceType = typeof(RefPackType);
			var factory = new BusinessObjectFactory();
			var dummyBizo = factory.New<RefPackType>();
			dummyBizo.F3_UnitOfDimension = "CM";
			form.SetDataBinding(dummyBizo, "");
			return form;
		});

		await rendered.Find(".zdropbutton button").MouseDownAsync(new WebMouseEventArgs() { Detail = 1 });

		var selectedCode = rendered.Find(".zdropform td:first-child div span").InnerHtml;
		var selectedDescription = rendered.Find(".zdropform td:nth-child(2) div span").InnerHtml;

		Assert.That(rendered.FindAll("input")[0].Attributes["value"].Value, Is.EqualTo("CM"));
		Assert.That(rendered.FindAll("input")[1].Attributes["value"].Value, Is.EqualTo("Centimeters"));
		Assert.That(rendered.Find(".zdropbutton").ParentElement.Attributes["style"].Value.Contains("width:45px", StringComparison.OrdinalIgnoreCase), Is.True);
		Assert.That(rendered.FindAll("input")[0].Attributes["style"].Value.Contains("width:24px", StringComparison.OrdinalIgnoreCase), Is.True);
		Assert.That(rendered.FindAll("input")[1].Attributes["style"].Value.Contains("width:255px", StringComparison.OrdinalIgnoreCase), Is.True);

		Assert.That(selectedCode, Is.Not.Empty);
		Assert.That(selectedDescription, Is.Not.Empty);

		await rendered.Find(".zdropform tr").ClickAsync(new WebMouseEventArgs());

		Assert.That(rendered.FindAll("input")[0].Attributes["value"].Value, Is.EqualTo(selectedCode));
		Assert.That(rendered.FindAll("input")[1].Attributes["value"].Value, Is.EqualTo(selectedDescription));

		Assert.That(rendered.FindAll(".zdropform").Count, Is.EqualTo(0));
	}

	[Test, WithPlaywrightPage]
	public async Task ZDropEditAutoComplete()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		ZDropEdit dropEdit = null;

		var page = await ctx.LoadFormAsync(() =>
		{
			var factory = new BusinessObjectFactory();
			var bizobj = factory.New<DummyBusinessObjectWithList>();

			var relatedBizObj0 = bizobj.List.AddNew();
			relatedBizObj0.Z0_Code = "ABC";
			var relatedBizObj1 = bizobj.List.AddNew();
			relatedBizObj1.Z0_Code = "BCD";
			var relatedBizObj2 = bizobj.List.AddNew();
			relatedBizObj2.Z0_Code = "CAS";

			relatedBizObj0.LookupList.Add(relatedBizObj1);
			relatedBizObj0.LookupList.Add(relatedBizObj2);

			var form = new WinzorTestForm();
			form.BindingSource.DataSourceType = typeof(DummyBusinessObjectWithList);
			dropEdit = new ZDropEdit();
			dropEdit.List = relatedBizObj0.LookupList;
			form.Controls.Add(dropEdit);
			return form;
		});
		var input = page.Locator("input.zdropcodebox");
		await input.WaitForAsync();

		await input.PressAsync("C");
		await AssertDropEditState(input, dropEdit, "CAS", 1, 3);

		await input.PressAsync("A");
		await AssertDropEditState(input, dropEdit, "CAS", 2, 3);

		await input.PressAsync("S");
		await AssertDropEditState(input, dropEdit, "CAS", 3, 3);
	}

	[Test, WithPlaywrightPage]
	public async Task ZDropEditAutoCompleteSupportCompositionText()
	{
		await using var ctx = new InMemoryAppServerTestContext();

		ZDropEdit dropEdit = null;

		var page = await ctx.LoadFormAsync(() =>
		{
			var factory = new BusinessObjectFactory();
			var bizobj = factory.New<DummyBusinessObjectWithList>();

			var relatedBizObj0 = bizobj.List.AddNew();
			relatedBizObj0.Z0_Code = "ABC";
			var relatedBizObj1 = bizobj.List.AddNew();
			relatedBizObj1.Z0_Code = "BCD";
			var relatedBizObj2 = bizobj.List.AddNew();
			relatedBizObj2.Z0_Code = "CAS";

			relatedBizObj0.LookupList.Add(relatedBizObj1);
			relatedBizObj0.LookupList.Add(relatedBizObj2);

			var form = new WinzorTestForm();
			form.BindingSource.DataSourceType = typeof(DummyBusinessObjectWithList);
			dropEdit = new ZDropEdit();
			dropEdit.List = relatedBizObj0.LookupList;
			form.Controls.Add(dropEdit);
			return form;
		});
		var input = page.Locator("input.zdropcodebox");
		var compositionText = "😁";
		await input.FillAsync(compositionText);
		await AssertDropEditState(input, dropEdit, compositionText, 2, 2);
	}

	async Task AssertDropEditState(ILocator input, ZDropEdit dropEdit, string inputValue, int selectionStart, int selectionEnd)
	{
		await Assertions.Expect(input).ToHaveValueAsync(inputValue);
		await Assertions.Expect(input).ToHaveJSPropertyAsync("selectionStart", selectionStart);
		await Assertions.Expect(input).ToHaveJSPropertyAsync("selectionEnd", selectionEnd);
		Assert.That(dropEdit.CodeBox.SelectionStart, Is.EqualTo(selectionStart));
		Assert.That(dropEdit.CodeBox.SelectionLength, Is.EqualTo(selectionEnd - selectionStart));
	}

	[Test]
	public async Task ZDropEditAutoCompleteReadOnly()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new WinzorTestForm();
			form.BindingSource.DataSourceType = typeof(RefPackType);
			var dropEdit = new ZDropEdit() { ReadOnly = true };
			form.Controls.Add(dropEdit);
			form.BindingSource.SetBindingMember(dropEdit, "F3_UOMType");
			form.DataSourceType = typeof(RefPackType);
			var factory = new BusinessObjectFactory();
			var dummyBizo = factory.New<RefPackType>();
			form.SetDataBinding(dummyBizo, "");
			return form;
		});

		var input = rendered.FindAll("input")[0];

		await rendered.KeyPressAsync(Keys.C, input);

		Assert.That(rendered.FindAll("input")[0].Attributes["value"].Value, Is.Empty);
	}

	[TestCaseSource(nameof(dropEditBackgroundSource)), WithPlaywrightPage]
	public async Task ZDropEditCodeBoxShouldFillTheContainerWhenEditable(string backgroundColor)
	{
		await using var ctx = new InMemoryAppServerTestContext();

		ZDropEdit dropEdit = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new WinzorTestForm();
			form.BindingSource.DataSourceType = typeof(RefPackType);
			dropEdit = new ZDropEdit() { Width = 200, Height = 20 };
			dropEdit.PreBoundMaxLength = 3;
			if (backgroundColor is not null)
			{
				dropEdit.CodeBox.BackColor = Color.FromName(backgroundColor);
			}
			form.Controls.Add(dropEdit);
			return form;
		});

		var codeBox = page.Locator(".zdropcodebox");
		await Assertions.Expect(codeBox).ToHaveCSSAsync("box-shadow", "rgb(255, 255, 225) 0px 0px 0px 1px");

		await dropEdit.InvokeWinzorDispatcherAsync(() =>
		{
			dropEdit.Enabled = false;
		});
		await Assertions.Expect(codeBox).ToHaveCSSAsync("box-shadow", "none");

		await dropEdit.InvokeWinzorDispatcherAsync(() =>
		{
			dropEdit.Enabled = true;
			dropEdit.ReadOnly = true;
		});
		await Assertions.Expect(codeBox).ToHaveCSSAsync("box-shadow", "none");
	}

	[Test, WithPlaywrightPage]
	public async Task ZDropEditCodeBoxShouldHaveCorrectFontSize()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new WinzorTestForm();
			form.BindingSource.DataSourceType = typeof(RefPackType);
			var dropEdit = new ZDropEdit() { Width = 200, Height = 20 };
			dropEdit.PreBoundMaxLength = 3;
			form.Controls.Add(dropEdit);
			return form;
		});

		var codeBox = page.Locator(".zdropcodebox");
		await Assertions.Expect(codeBox).ToHaveCSSAsync("font-size", "11px");
	}

	[Test]
	public async Task ZDropEditValidation()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new WinzorTestForm();
			form.BindingSource.DataSourceType = typeof(RefPackType);
			var dropedit = new ZDropEdit();
			form.Controls.Add(dropedit);
			form.BindingSource.SetBindingMember(dropedit, "F3_UnitOfDimension");
			form.DataSourceType = typeof(RefPackType);
			var factory = new BusinessObjectFactory();
			var dummyBizo = factory.New<RefPackType>();
			form.SetDataBinding(dummyBizo, "");
			var button = new ZButton() { Text = "Validate", Top = 200 };
			form.Controls.Add(button);
			return form;
		});

		var input = rendered.Find("input");
		var validateButton = rendered.Find("button:contains('Validate')");

		await input.TriggerEventAsync("oninput", new ChangeEventArgs { Value = "ZZ" });
		await validateButton.TriggerEventAsync("onwinzorfocusin", new WinzorFocusInEventArgs());
		Assert.That(rendered.Find(".notification"), Is.Not.Null);
	}

	[Test]
	public async Task ZDropEditClickOnDropButtonTriggersValidation()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new WinzorTestForm();
			form.BindingSource.DataSourceType = typeof(RefPackType);
			var dropedit = new ZDropEdit();
			form.Controls.Add(dropedit);
			form.BindingSource.SetBindingMember(dropedit, "F3_UnitOfDimension");
			form.DataSourceType = typeof(RefPackType);
			var factory = new BusinessObjectFactory();
			var dummyBizo = factory.New<RefPackType>();
			form.SetDataBinding(dummyBizo, "");
			return form;
		});
		var input = rendered.Find("input");
		var dropButton = rendered.Find(".zdropbutton button");

		await input.TriggerEventAsync("oninput", new ChangeEventArgs { Value = "ZZ" });
		await dropButton.TriggerEventAsync("onmousedown", new WebMouseEventArgs());
		Assert.That(rendered.Find(".notification"), Is.Not.Null);
	}

	[Test]
	public async Task ZDropEditClickOnDropButtonSelectionBehavior()
	{
		using var ctx = new EnterpriseTestContext();
		ZDropEdit dropedit = null;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new WinzorTestForm();
			form.BindingSource.DataSourceType = typeof(RefPackType);
			dropedit = new ZDropEdit();
			form.Controls.Add(dropedit);
			form.BindingSource.SetBindingMember(dropedit, "F3_UnitOfDimension");
			form.DataSourceType = typeof(RefPackType);
			var factory = new BusinessObjectFactory();
			var dummyBizo = factory.New<RefPackType>();
			dummyBizo.F3_UnitOfDimension = "CM";
			form.SetDataBinding(dummyBizo, "");
			return form;
		});

		var input = rendered.Find("input");
		var dropButton = rendered.Find(".zdropbutton button");

		await dropButton.TriggerEventAsync("onmousedown", new WebMouseEventArgs());
		Assert.That(dropedit.CodeBox.SelectedText, Is.EqualTo("CM"));

		await input.TriggerEventAsync("oninput", new ChangeEventArgs { Value = "MM" });
		await input.TriggerEventAsync("ontextboxselectionchange", new TextboxSelectionChangeEventArgs { SelectionStart = 2, SelectionEnd = 2 });
		Assert.That(dropedit.CodeBox.SelectionStart, Is.EqualTo(2));
		await dropButton.TriggerEventAsync("onmousedown", new WebMouseEventArgs());
		Assert.That(dropedit.CodeBox.SelectionStart, Is.EqualTo(0));
		Assert.That(dropedit.CodeBox.SelectionLength, Is.EqualTo(0));
	}

	[Test]
	public async Task ListNotAccessedByRenderThread()
	{
		ErrorReporter.Clear();

		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new WinzorTestForm();
			form.BindingSource.DataSourceType = typeof(DummyBusinessObjectInList);
			var dropedit = new ZDropEdit();
			dropedit.BindToList = "LookupListFromDataBase";
			form.Controls.Add(dropedit);
			form.BindingSource.SetBindingMember(dropedit, "Z0_Description");
			form.DataSourceType = typeof(DummyBusinessObjectInList);
			var factory = new BusinessObjectFactory();
			var dummyBizo = factory.New<DummyBusinessObjectInList>();
			dummyBizo.Z0_Description = string.Empty;
			form.SetDataBinding(dummyBizo, "");
			dropedit.ShowDropDown();
			return form;
		});

		Assert.That(ErrorReporter.LastExceptionReported, Is.Null);
	}

	[Test, WithPlaywrightPage]
	public async Task ZDropEditFocusesCodeBoxOnClick()
	{
		var clientServiceProvider = new MockCargoWiseClientSeviceProvider();
		await using var ctx = new InMemoryAppServerTestContext(clientServiceProvider);

		WinzorTestForm form = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new WinzorTestForm();
			var list = new CodeDescriptionPairList();
			list.AddPair(Guid.NewGuid(), "ONE", "Description 01");
			list.AddPair(Guid.NewGuid(), "TWO", "Description 02");
			list.AddPair(Guid.NewGuid(), "THREE", "Description 03");
			var dropEdit = new ZDropEdit { List = list, DisableInvalidation = true };
			form.Controls.Add(new ZCheckBox());
			form.Controls.Add(dropEdit);
			return form;
		});

		try
		{
			await (page.Locator(".zdropbutton button")).ClickAsync();
			await (page.Locator(".zdropform table tr").Nth(1)).ClickAsync();
		}
		finally
		{
			Assert.That(() => form.ActiveControl.GetType(), Is.EqualTo(typeof(ZDropEdit)).After(3000, 100));
			var zGridDropEdit = form.ActiveControl as ZDropEdit;
			Assert.That(zGridDropEdit.CodeBox.Focused);
		}
	}

	[Test]
	public async Task ZFilterStripDropFormHasTitleAttributeForTooltip()
	{
		using var ctx = new EnterpriseTestContext();
		var list = new CodeDescriptionPairList();

		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new WinzorTestForm();
			list.AddPair(Guid.NewGuid(), "Asdf", "Test 1234");
			list.AddPair(Guid.NewGuid(), "ONE", "Description 01");
			list.AddPair(Guid.NewGuid(), "TWO", "Description 02");
			list.AddPair(Guid.NewGuid(), "THREE", "Description 03");
			list.AddPair(Guid.NewGuid(), "FOUR", "Description 04");
			var dropEdit = new ZFilterStripDropEdit { List = list, DisableInvalidation = true };
			dropEdit.ShowDropDown();
			form.Controls.Add(dropEdit);
			return form;
		});

		var dropFormEntries = rendered.FindAll(".zdropform tr");
		Assert.That(dropFormEntries.Count, Is.EqualTo(5));

		int listIndex = 0;
		foreach (var entry in dropFormEntries)
		{
			Assert.That(entry.GetAttribute("title"), Is.Not.Empty);
			Assert.That(entry.GetAttribute("title"), Is.EqualTo(list[listIndex].Code));
			listIndex++;
		}
	}

	[Test]
	public async Task ZDropFormHasNoTitleAttributeForTooltip()
	{
		using var ctx = new EnterpriseTestContext();
		var list = new CodeDescriptionPairList();

		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new WinzorTestForm();
			list.AddPair(Guid.NewGuid(), "Asdf", "Test 1234");
			list.AddPair(Guid.NewGuid(), "ONE", "Description 01");
			list.AddPair(Guid.NewGuid(), "TWO", "Description 02");
			list.AddPair(Guid.NewGuid(), "THREE", "Description 03");
			list.AddPair(Guid.NewGuid(), "FOUR", "Description 04");
			var dropEdit = new ZDropEdit { List = list, DisableInvalidation = true };
			dropEdit.ShowDropDown();
			form.Controls.Add(dropEdit);
			return form;
		});

		var dropFormEntries = rendered.FindAll(".zdropform tr");
		Assert.That(dropFormEntries.Count, Is.EqualTo(5));

		int listIndex = 0;
		foreach (var entry in dropFormEntries)
		{
			Assert.That(entry.GetAttribute("title"), Is.Empty);
			listIndex++;
		}
	}

	[Test, WithPlaywrightPage]
	public async Task ZDropEditTextBoxFocusedWhenClickingDropButton()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() => ZDropEditTestForm());

		var zDropEditInput = page.Locator("input.zdropcodebox");
		await zDropEditInput.WaitForAsync();
		await zDropEditInput.BlurAsync();
		await Assertions.Expect(zDropEditInput).Not.ToBeFocusedAsync();
		await (page.Locator(".zdropbutton button")).ClickAsync();
		await Assertions.Expect(zDropEditInput).ToBeFocusedAsync();
	}

	[Test, WithPlaywrightPage]
	public async Task ZDropEditTextBoxRetainFocusWhenSelectingItem()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() => ZDropEditTestForm());

		var zDropEditInput = page.Locator("input.zdropcodebox");
		await zDropEditInput.ClickAsync();
		await Assertions.Expect(zDropEditInput).ToBeFocusedAsync();
		await (page.Locator(".zdropbutton button")).ClickAsync();
		await Assertions.Expect(zDropEditInput).ToBeFocusedAsync();
		await (page.Locator(".zdropform tr:first-child")).ClickAsync();
		await Assertions.Expect(zDropEditInput).ToBeFocusedAsync();
	}

	[Test, WithPlaywrightPage]
	public async Task ZDropEditDropDownClosedWhenControlLosesFocus()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() => ZDropEditTestForm());

		var zdropcodebox = page.Locator(".zdropcodebox");
		await zdropcodebox.WaitForAsync();
		await zdropcodebox.PressAsync("ArrowDown");
		var zdropform = page.Locator(".zdropform");
		await Assertions.Expect(zdropform).ToHaveCountAsync(1);
		await zdropcodebox.BlurAsync();
		await Assertions.Expect(zdropform).ToHaveCountAsync(0);
	}

	[Test, WithPlaywrightPage]
	public async Task ZDropEditDropDownClosedWhenTextBoxClicked()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = ZDropEditTestForm();
			return form;
		});

		await (page.Locator(".zdropbutton button")).ClickAsync();
		await Assertions.Expect(page.Locator(".zdropform")).ToHaveCountAsync(1);
		await (page.Locator("input.zdropcodebox")).ClickAsync();
		await Assertions.Expect(page.Locator(".zdropform")).ToHaveCountAsync(0);
	}

	[Test, WithPlaywrightPage]
	public async Task ZDropEditDropDownClosedWhenDescriptionBoxClicked()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(ZDropEditTestForm);

		await page.Locator(".zdropbutton button").ClickAsync();
		await Assertions.Expect(page.Locator(".zdropform")).ToBeVisibleAsync();
		await page.Locator(".textbox:nth-of-type(2)").ClickAsync();
		await Assertions.Expect(page.Locator(".zdropform")).Not.ToBeVisibleAsync();
	}

	[Test, WithPlaywrightPage]
	public async Task ZDropEditDropDownClosedWhenParentUserControlClicked()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = ZDropEditTestForm();
			var dropEdit = form.Controls[0];
			var userControl = new ZUserControl { Dock = DockStyle.Fill };
			userControl.Controls.Add(dropEdit);
			form.Controls.Remove(dropEdit);
			form.Controls.Add(userControl);
			return form;
		});

		await page.Locator(".zdropbutton button").ClickAsync();
		await Assertions.Expect(page.Locator(".zdropform")).ToBeVisibleAsync();
		await page.Locator(".form").ClickAsync(new () { Position = new () { X = 100, Y = 100 } });
		await Assertions.Expect(page.Locator(".zdropform")).Not.ToBeVisibleAsync();
	}

	WinzorTestForm ZDropEditTestForm()
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
	}

	WinzorTestForm ZAddressDropEditTestForm(bool showDropForm = false)
	{
		var form = new WinzorTestForm();
		var addressList = new ZAddressList();
		addressList.AddAddress(ZGuid.NewZGuid(), "NJ", "Nanjing", new AddressCapabilityItem { Capability = nameof(AddressType.ARM) });
		addressList.AddAddress(ZGuid.NewZGuid(), "SH", "Shanghai", new AddressCapabilityItem { Capability = nameof(AddressType.OFC) });
		addressList.AddAddress(ZGuid.NewZGuid(), "HZ", "Hangzhou", new AddressCapabilityItem { Capability = nameof(AddressType.ARM) });

		var dummy = new BusinessObjectFactory().New<DummyWithZAddress>();
		dummy.Z0_Guid_ZAddress.AddresssListOverride = (f, l) => addressList;
		dummy.Z0_Guid_ZAddress.DefaultAddressType = AddressType.ARM;
		form.SetDataBinding(dummy, "");

		var dropEdit = new ZAddressDropEdit();
		dropEdit.BindingSource.DataSourceType = typeof(DummyWithZAddress);
		dropEdit.SetBindingMember(".");

		var addressParentControl = new AddressParentControl();
		addressParentControl.SetBindingMember(DummyBizoSchema.Constants.Z0_Guid);
		addressParentControl.Controls.Add(dropEdit);
		form.Controls.Add(addressParentControl);

		if (showDropForm)
		{
			dropEdit.ShowDropDown();
		}

		return form;
	}

	[Test]
	public async Task ZFilterStripDropFormRendersCommonItem()
	{
		using var ctx = new EnterpriseTestContext();

		var rendered = await ctx.RenderFormAsync(() =>
		{
			var helper = new ZFilterStripDropHelper();
			var list = new CodeDescriptionPairList();
			var form = new WinzorTestForm();

			var filter = new DummyModuleFilter("Description", DummyBizoSchema.Z0_Code);
			filter.IsCommon = true;
			list.Add(filter);
			list.AddPair(Guid.NewGuid(), "Hello", "World!");

			Assert.That(helper.IsCommonItem(filter));

			var dropEdit = new ZFilterStripDropEdit { List = list, DisableInvalidation = true };
			dropEdit.ShowDropDown();
			form.Controls.Add(dropEdit);
			return form;
		});

		var dropFormEntries = rendered.FindAll(".zdropform tr");
		Assert.That(dropFormEntries.Count, Is.EqualTo(2));

		Assert.That(dropFormEntries[0].InnerHtml, Does.Contain("(Common)"));
		Assert.That(dropFormEntries[0].OuterHtml, Does.Contain("zdropform__item--common"));

		Assert.That(dropFormEntries[1].InnerHtml, Does.Not.Contain("(Common)"));
		Assert.That(dropFormEntries[1].OuterHtml, Does.Not.Contain("zdropform__item--common"));
	}

	[Test]
	public async Task ZFilterStripDropFormRendersExclusiveItem()
	{
		using var ctx = new EnterpriseTestContext();

		var rendered = await ctx.RenderFormAsync(() =>
		{
			var helper = new ZFilterStripDropHelper();
			var list = new CodeDescriptionPairList();
			var form = new WinzorTestForm();

			var filter = new DummyModuleFilter("Description", DummyBizoSchema.Z0_Code);
			filter.IsExclusiveHelper = true;
			list.Add(filter);
			list.AddPair(Guid.NewGuid(), "Hello", "World!");

			Assert.That(helper.IsExclusiveItem(filter));

			var dropEdit = new ZFilterStripDropEdit { List = list, DisableInvalidation = true };
			dropEdit.ShowDropDown();
			form.Controls.Add(dropEdit);
			return form;
		});

		var dropFormEntries = rendered.FindAll(".zdropform tr");
		Assert.That(dropFormEntries.Count, Is.EqualTo(2));

		Assert.That(dropFormEntries[0].OuterHtml, Does.Contain("zdropform__item--exclusive"));
		Assert.That(dropFormEntries[1].OuterHtml, Does.Not.Contain("zdropform__item--exclusive"));
	}

	[Test]
	public async Task ZFilterStripDropFormRendersFilterAlreadySelected()
	{
		using var ctx = new EnterpriseTestContext();

		var rendered = await ctx.RenderFormAsync(() =>
		{
			var helper = new ZFilterStripDropHelper();
			var list = new CodeDescriptionPairList();
			var form = new WinzorTestForm();

			var filter = new DummyModuleFilter("Description", DummyBizoSchema.Z0_Code);
			filter.IsActive = true;
			list.Add(filter);
			list.AddPair(Guid.NewGuid(), "Hello", "World!");

			Assert.That(helper.IsFilterAlreadySelected(filter));

			var dropEdit = new ZFilterStripDropEdit { List = list, DisableInvalidation = true };
			dropEdit.ShowDropDown();
			form.Controls.Add(dropEdit);
			return form;
		});

		var dropFormEntries = rendered.FindAll(".zdropform tr");
		Assert.That(dropFormEntries.Count, Is.EqualTo(2));

		Assert.That(dropFormEntries[0].OuterHtml, Does.Contain("zdropform__item--active"));
		Assert.That(dropFormEntries[1].OuterHtml, Does.Not.Contain("zdropform__item--active"));
	}

	[Test]
	public async Task ZDropFormTableTrStyle()
	{
		var list = new CodeDescriptionPairList();
		for (int i = 0; i < 2; i++)
		{
			list.AddPair(Guid.NewGuid(), i.ToString(), i.ToString());
		}

		using var ctx = new EnterpriseTestContext();
		ZDropEdit dropEdit = null;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new WinzorTestForm();
			dropEdit = new ZDropEdit { List = list, DisableInvalidation = true };
			dropEdit.ShowDropDown();
			form.Controls.Add(dropEdit);
			return form;
		});

		var height = (int)(TextRenderer.MeasureText("Item", dropEdit.DropButton.DropDown.Font).Height * 1.1);
		var classString = rendered.Find(".zdropform tr").GetAttribute("class");
		Assert.That(classString, Is.EqualTo("zdropform__item"));
		var sizeStyleString = rendered.Find(".zdropform tr td").GetAttribute("style");
		Assert.That(sizeStyleString, Does.Contain($"height:{height}px;"));
	}

	[Test]
	public async Task RenderedListUpdatedAfterRefreshList()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var list = new CodeDescriptionPairList();
			for (var i = 0; i < 2; i++)
			{
				list.AddPair(Guid.NewGuid(), i.ToString(), i.ToString());
			}

			var form = new WinzorTestForm();
			var dropEdit = new ZDropEdit { List = list, DisableInvalidation = true };
			dropEdit.ShowDropDown();
			form.Controls.Add(dropEdit);

			var button = new Button { Text = "Test Refresh List" };
			button.Click += Button_Click;
			form.Controls.Add(button);

			void Button_Click(object sender, EventArgs e)
			{
				var list = new CodeDescriptionPairList();
				for (var i = 0; i < 3; i++)
				{
					list.AddPair(Guid.NewGuid(), i.ToString(), i.ToString());
				}
				dropEdit.List = list;
				dropEdit.DropButton.DropDown.RefreshList();

				button.Click -= Button_Click;
			}

			return form;
		});

		var dropFormEntries = rendered.FindAll(".zdropform tr");
		Assert.That(dropFormEntries.Count, Is.EqualTo(2));
		await rendered.Find("button:contains('Test Refresh List')").ClickAsync(new WebMouseEventArgs());

		dropFormEntries = rendered.FindAll(".zdropform tr");
		Assert.That(dropFormEntries.Count, Is.EqualTo(3));
	}

	[TestCase(AddressType.NoDefault, false, TestName = "{m}_Show")]
	[TestCase(AddressType.ARM, true, TestName = "{m}_NotShow")]
	public async Task ZAddressDropFormAddressTypeFilter(AddressType addressType, bool showExtraControl)
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new WinzorTestForm();
			var dummy = new BusinessObjectFactory().New<DummyWithZAddress>();

			dummy.Z0_Guid_ZAddress.DefaultAddressType = addressType;
			form.SetDataBinding(dummy, "");

			var dropEdit = new ZAddressDropEdit();
			dropEdit.BindingSource.DataSourceType = typeof(DummyWithZAddress);
			dropEdit.SetBindingMember(".");

			var addressParentControl = new AddressParentControl();
			addressParentControl.SetBindingMember(DummyBizoSchema.Constants.Z0_Guid);
			addressParentControl.Controls.Add(dropEdit);
			form.Controls.Add(addressParentControl);

			dropEdit.ShowDropDown();
			return form;
		});

		var typeFilter = rendered.FindAll(".addresstypefilter");
		Assert.That(typeFilter.Any(), Is.EqualTo(showExtraControl));

		var checkBox = rendered.FindAll(".addresstypefilter .checkbox");
		Assert.That(checkBox.Any(), Is.EqualTo(showExtraControl));
	}

	[Test]
	public async Task ZAddressDropFormFilteredListUpdatedAfterClickTypeFilter()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = ZAddressDropEditTestForm(true);
			return form;
		});

		var addressItems = rendered.FindAll(".zdropform tr");
		Assert.That(addressItems.Count, Is.EqualTo(2));
		Assert.That(addressItems[0].Children[1].GetInnerText(), Is.EqualTo("Nanjing"));
		Assert.That(addressItems[1].Children[1].GetInnerText(), Is.EqualTo("Hangzhou"));

		await rendered.FindAll(".checkbox__input")[0].ChangeAsync(new ChangeEventArgs { Value = true });
		addressItems = rendered.FindAll(".zdropform tr");
		Assert.That(addressItems.Count, Is.EqualTo(3));
		Assert.That(addressItems[0].Children[1].GetInnerText(), Is.EqualTo("Nanjing"));
		Assert.That(addressItems[1].Children[1].GetInnerText(), Is.EqualTo("Shanghai"));
		Assert.That(addressItems[2].Children[1].GetInnerText(), Is.EqualTo("Hangzhou"));
	}

	[Test, WithPlaywrightPage]
	public async Task ZAddressDropFormAddressTypeFilterStyle()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() => ZAddressDropEditTestForm());

		var dropButton = page.Locator(".zdropbutton > button");
		await dropButton.ClickAsync();

		var dropForm = page.Locator(".zdropform");
		await Assertions.Expect(dropForm).ToHaveCSSAsync("width", "268px");

		var typeFilter = dropForm.Locator(".addresstypefilter");
		await Assertions.Expect(typeFilter).ToHaveCSSAsync("position", "relative");

		var checkBox = typeFilter.Locator(".checkbox");
		await Assertions.Expect(checkBox).ToHaveCSSAsync("position", "relative");
		await Assertions.Expect(checkBox).ToHaveCSSAsync("padding", "0px");
		await Assertions.Expect(checkBox).ToHaveCSSAsync("height", "18px");
		await Assertions.Expect(checkBox).ToHaveCSSAsync("width", "266px");
		await Assertions.Expect(checkBox).ToHaveCSSAsync("background-color", "rgb(240, 240, 240)");

		await checkBox.ClickAsync();
		await Assertions.Expect(checkBox).ToHaveCSSAsync("background-color", "rgb(240, 240, 240)");
	}

	[Test, WithPlaywrightPage]
	public async Task ZDropCodeBoxHasPaddingStyle()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			form.Controls.Add(new ZDropCodeBox());
			return form;
		});

		var codeBoxInput = page.Locator(".zdropcodebox");
		await Assertions.Expect(codeBoxInput).ToHaveCountAsync(1);

		await Assertions.Expect(codeBoxInput).ToHaveCSSAsync("padding-left", "0px");
		await Assertions.Expect(codeBoxInput).ToHaveCSSAsync("padding-bottom", "0px");
	}

	[Test]
	public async Task DateAcceptAbilityControlButtonsHaveCorrectMargins()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var acceptabilityMockControl = new ContainerControl();
			var dropedit = new ZDropEdit();
			dropedit.Anchor = (((AnchorStyles.Top | AnchorStyles.Bottom)
				| AnchorStyles.Left) | AnchorStyles.Right);
			dropedit.Location = ControlDpiScalingHelper.NewScaledPoint(104, 0, true);
			dropedit.Name = "DateAcceptabilityDropEdit";
			dropedit.PreBoundMaxLength = 3;
			dropedit.ShowDescriptionBox = false;
			dropedit.Size = ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			dropedit.TabIndex = 0;

			var qButton = new Button();
			qButton.Anchor = (AnchorStyles.Top | AnchorStyles.Right);
			qButton.Location = ControlDpiScalingHelper.NewScaledPoint(163, 0, true);
			qButton.Name = "LegendButton";
			qButton.Size = ControlDpiScalingHelper.NewScaledSize(22, 20, true);
			qButton.TabIndex = 1;
			qButton.Text = "?";
			qButton.UseVisualStyleBackColor = true;

			acceptabilityMockControl.AutoScaleDimensions = new SizeF(6F, 13F);
			acceptabilityMockControl.AutoScaleMode = AutoScaleMode.None;
			acceptabilityMockControl.Controls.Add(qButton);
			acceptabilityMockControl.Controls.Add(dropedit);
			acceptabilityMockControl.Name = "DateAcceptabilityControl";
			acceptabilityMockControl.Size = ControlDpiScalingHelper.NewScaledSize(257, 20, true);
			acceptabilityMockControl.ResumeLayout(false);
			form.Controls.Add(acceptabilityMockControl);
			return form;
		});

		Assert.That(rendered.Find("input").Attributes["style"].Value.Contains("width:33px", StringComparison.OrdinalIgnoreCase), Is.True);
	}

	[Test]
	public async Task ZDropFormShouldRenderInPortal()
	{
		using var ctx = new EnterpriseTestContext();
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			using var dropEdit = new ZDropEdit();
			dropEdit.ShowDropDown();
			Assert.That(dropEdit.DropButton.DropDown.RenderInPortal, Is.True);
		});
	}

	[Test, WithPlaywrightPage]
	public async Task TextShouldScrollToEndWhenSelectedItem()
	{
		var clientServiceProvider = new MockCargoWiseClientSeviceProvider();
		await using var ctx = new InMemoryAppServerTestContext(clientServiceProvider);

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new WinzorTestForm();
			var list = new CodeDescriptionPairList();
			list.AddPair(Guid.NewGuid(), "ONEONEONEONEONE", "Description 01");
			list.AddPair(Guid.NewGuid(), "TWOTWOTWOTWOTWO", "Description 02");
			list.AddPair(Guid.NewGuid(), "THREETHREETHREE", "Description 03");
			var dropEdit = new ZDropEdit { List = list, DisableInvalidation = true };
			form.Controls.Add(new ZCheckBox());
			form.Controls.Add(dropEdit);
			return form;
		});

		try
		{
			await (page.Locator(".zdropbutton button")).ClickAsync();
			await (page.Locator(".zdropform table tr").Nth(1)).ClickAsync();
		}
		finally
		{
			var input = page.Locator(".zdropcodebox");
			await input.WaitForAsync();
			var scrollLeft = 0;
			Assert.That(async () => scrollLeft = await input.EvaluateAsync<int>("e => e.scrollLeft"), Is.GreaterThan(0).After(1000, 100));
			Assert.That(await input.EvaluateAsync<int>("e => e.scrollWidth - e.clientWidth"), Is.EqualTo(scrollLeft));
		}
	}

	[Test]
	public async Task DropDownShouldNotShowColor_WhenShowColorIsFalseAsync()
	{
		using var ctx = new EnterpriseTestContext();
		var list = new CodeDescriptionPairList();

		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new WinzorTestForm();
			list.AddPair(Guid.NewGuid(), "Asdf", "Test 1234");
			list.AddPair(Guid.NewGuid(), "ONE", "Description 01");
			list.AddPair(Guid.NewGuid(), "TWO", "Description 02");
			list.AddPair(Guid.NewGuid(), "THREE", "Description 03");
			list.AddPair(Guid.NewGuid(), "FOUR", "Description 04");
			var dropEdit = new ZDropEdit { List = list, ShowColorInDropDown = false, DisableInvalidation = true };
			dropEdit.ShowDropDown();
			form.Controls.Add(dropEdit);
			return form;
		});

		var dropFormEntries = rendered.FindAll(".zdropform tr");
		Assert.That(dropFormEntries.Count, Is.EqualTo(5));
		dropFormEntries.ForEach((entry) => Assert.That(entry.QuerySelectorAll("td").Length, Is.EqualTo(2)));
	}

	[Test]
	public async Task DropDownShouldShowColor_WhenShowColorIsTrueAsync()
	{
		using var ctx = new EnterpriseTestContext();
		var list = new CodeDescriptionPairList();

		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new WinzorTestForm();
			list.AddPair(Guid.NewGuid(), "Black", "Test 1234");
			list.AddPair(Guid.NewGuid(), "Invalid Color", "Description 01");
			list.AddPair(Guid.NewGuid(), "Blue", "Description 02");
			list.AddPair(Guid.NewGuid(), "Dark Gray", "Description 03");
			list.AddPair(Guid.NewGuid(), "Green", "Description 04");
			var dropEdit = new ZDropEdit { List = list, ShowColorInDropDown = true, DisableInvalidation = true };
			dropEdit.ShowDropDown();
			form.Controls.Add(dropEdit);
			return form;
		});

		var expectedColors = list.GetAllCodes().Select((c) =>
		{
			var color = Color.FromName(c.Replace(" ", ""));
			return $"rgba({color.R}, {color.G}, {color.B}, {color.A / 255})";
		}).ToList();

		var dropFormEntries = rendered.FindAll(".zdropform tr");
		Assert.That(dropFormEntries.Count, Is.EqualTo(5));
		Assert.Multiple(() =>
		{
			foreach (var (entry, i) in dropFormEntries.WithIndex())
			{
				Assert.That(entry.QuerySelectorAll("td").Length, Is.EqualTo(3));
				Assert.That(entry.QuerySelectorAll("td")[2].GetStyle().CssText, Does.Contain($"background-color: {expectedColors[i]}"),
					$"Color {list.GetAllCodes()[i]} is displayed correctly");
			}
		});
	}
}

class AddressParentControl : ZUserControl, IZAddressParent
{
	void IZAddressParent.SetControlSize() { }

	ZGuid IZAddressParent.ParseCode(string code)
	{
		return ZGuid.Invalid;
	}

	bool IZAddressParent.CheckZAddressBindingSuffix => true;
}
