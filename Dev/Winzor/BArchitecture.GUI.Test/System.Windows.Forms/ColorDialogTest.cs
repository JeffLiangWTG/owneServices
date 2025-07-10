using System.Drawing;
using System.Threading.Tasks;
using Bunit;
using Microsoft.Playwright;
using NUnit.Framework;
using WinzorFramework;
using WinzorTestFramework;
using WTG.PlaywrightTesting;

namespace System.Windows.Forms;

using static WinzorTestContext;

sealed class ColorDialogTest
{
	[Test]
	public async Task ColorDialogHasCorrectClass()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() => new ColorDialog());

		Assert.That(rendered.Find(".colordialog"), Is.Not.Null);
		Assert.That(rendered.Find(".colordialog").GetAttribute("style"), Does.Contain("--panelminwidth: 220px;"));
		Assert.That(rendered.Find(".colordialog").GetAttribute("style"), Does.Contain("background-color:var(--color-control);"));
	}

	[Test]
	public async Task ColorDialogButtonsHaveCorrectPositions()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var colorDialog = new ColorDialog();
			colorDialog.FullOpen = true;
			return colorDialog;
		});
		var buttons = rendered.FindAll(".colordialog__buttonspanel > button");
		var addButton = rendered.Find(".colordialog__extendedpanel > button");

		for (int i = 0; i < 3; i++)
		{
			Assert.That(buttons[i].ClassList, Does.Contain("button--center"));
		}

		Assert.That(addButton.ClassList, Does.Contain("button--center"));
	}

	[Test]
	public async Task ColorDialogOpensWithBasicPanelOnly()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() => new ColorDialog());

		Assert.That(rendered.FindAll(".colordialog__basicpanel").Count, Is.EqualTo(1));
		Assert.That(rendered.FindAll(".colordialog__extendedpanel").Count, Is.EqualTo(0));
	}

	[Test]
	public async Task ColorDialogButtonsDefineCustomColorExpandsMenu()
	{
		using var ctx = new WinzorTestContext();
		ColorDialog colorDialog = null;
		var rendered = await ctx.RenderFormAsync(() => colorDialog = new ColorDialog());

		Assert.That(colorDialog, Is.Not.Null);
		Assert.That(colorDialog.FullOpen, Is.EqualTo(false));
		Assert.That(rendered.FindAll(".colordialog__basicpanel").Count, Is.EqualTo(1));
		Assert.That(rendered.FindAll(".colordialog__extendedpanel").Count, Is.EqualTo(0));
		await rendered.FindAll(".colordialog__buttonspanel > button")[0].ClickAsync(new WebMouseEventArgs());
		Assert.That(colorDialog.FullOpen, Is.EqualTo(true));
		Assert.That(rendered.FindAll(".colordialog__basicpanel").Count, Is.EqualTo(1));
		Assert.That(rendered.FindAll(".colordialog__extendedpanel").Count, Is.EqualTo(1));
	}

	[Test]
	public async Task ColorDialogButtonsDefineCustomColorResizesForm()
	{
		using var ctx = new WinzorTestContext();
		ColorDialog colorDialog = null;
		var rendered = await ctx.RenderFormAsync(() => colorDialog = new ColorDialog());

		Assert.That(colorDialog, Is.Not.Null);
		Assert.That(colorDialog.Width, Is.EqualTo(220));
		Assert.That(colorDialog.Height, Is.EqualTo(300));
		await rendered.FindAll(".colordialog__buttonspanel > button")[0].ClickAsync(new WebMouseEventArgs());
		Assert.That(colorDialog.FullOpen, Is.EqualTo(true));
		Assert.That(colorDialog.Width, Is.EqualTo(440));
		Assert.That(colorDialog.Height, Is.EqualTo(300));
	}

	[Test]
	public async Task ColorDialogButtonsHaveCorrectClasses()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() => new ColorDialog());

		var buttons = rendered.FindAll(".colordialog__buttonspanel > button");
		Assert.That(buttons.Count, Is.EqualTo(3));
		foreach (var button in buttons)
		{
			Assert.That(button.ClassList, Does.Contain("button"));
			Assert.That(button.ClassList, Does.Contain("button--standard"));
		}
	}

	[Test]
	public async Task ColorDialogButtonsDefineCustomColorDisabledAfterClick()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() => new ColorDialog());

		var defineColorButton = rendered.FindAll(".colordialog__buttonspanel > button")[0];
		Assert.That(defineColorButton.GetAttribute("disabled"), Is.Null);
		await defineColorButton.ClickAsync(new WebMouseEventArgs());
		defineColorButton = rendered.FindAll(".colordialog__buttonspanel > button")[0];
		Assert.That(defineColorButton.GetAttribute("disabled"), Is.EqualTo(string.Empty));
	}

	[Test]
	public async Task ColorDialogButtonsExtendColorDialogDoesNotHandleEventWhenFullOpen()
	{
		using var ctx = new WinzorTestContext();
		var module = ctx.JSInterop.SetupModule("/js/module/colorDialog.js");
		module.Setup<object>("initColorPicker", _ => true).SetResult(null);

		ColorDialog colorDialog = null;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			colorDialog = new ColorDialog();
			colorDialog.FullOpen = true;
			return colorDialog;
		});
		var defineColorButton = rendered.FindAll(".colordialog__buttonspanel > button")[0];

		await defineColorButton.ClickAsync(new WebMouseEventArgs());
		ctx.JSInterop.VerifyNotInvoke("initColorPicker");
	}

	[Test]
	public async Task ColorDialogButtonsExtendColorDialogDoesNotHandleEventWhenNotAllowFullOpen()
	{
		using var ctx = new WinzorTestContext();
		var module = ctx.JSInterop.SetupModule("/js/module/colorDialog.js");
		module.Setup<object>("initColorPicker", _ => true).SetResult(null);

		ColorDialog colorDialog = null;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			colorDialog = new ColorDialog();
			colorDialog.AllowFullOpen = false;
			return colorDialog;
		});
		var defineColorButton = rendered.FindAll(".colordialog__buttonspanel > button")[0];

		await defineColorButton.ClickAsync(new WebMouseEventArgs());
		ctx.JSInterop.VerifyNotInvoke("initColorPicker");
	}

	[Test]
	public async Task ColorDialogButtonsOkSetsResultsAndClosesDialog()
	{
		using var ctx = new WinzorTestContext();
		ColorDialog colorDialog = null;
		var formClosed = new TaskCompletionSource<bool>();
		Form form = null;
		var dispatcherContext = new DispatcherContext(ctx, ctx.DefaultClientServices, OpenFormAction.BlockUntilShown);
		await ctx.RenderFormAsync(() => form = new Form() { Text = "Main Form" });
		var shownTCS = new TaskCompletionSource();
		var showDialogTask = ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			colorDialog = new ColorDialog();
			colorDialog.Closed += (sender, args) => formClosed.SetResult(true);
			colorDialog.Shown += (sender, args) => shownTCS.SetResult();
			using (ctx.WinzorDispatcher.WithContext(dispatcherContext))
			{
				colorDialog.ShowDialog();
			}
		});

		await shownTCS.Task;
		Assert.That(colorDialog, Is.Not.Null);
		var okButton = dispatcherContext.Rendered.FindAll(".colordialog__buttonspanel > button")[1];
		Assert.That(okButton.TextContent, Is.EqualTo("OK"));
		Assert.That(colorDialog.DialogResult, Is.EqualTo(DialogResult.None));
		await okButton.ClickAsync(new WebMouseEventArgs());
		await showDialogTask;
		Assert.That(colorDialog.DialogResult, Is.EqualTo(DialogResult.OK));
		Assert.That(await formClosed.Task.WithTimeout(TimeSpan.FromSeconds(3)), Is.EqualTo(true));
	}

	[Test]
	public async Task ColorDialogButtonsCancelSetsResultsAndClosesDialog()
	{
		using var ctx = new WinzorTestContext();
		ColorDialog colorDialog = null;
		var formClosed = new TaskCompletionSource<bool>();
		Form form = null;
		var dispatcherContext = new DispatcherContext(ctx, ctx.DefaultClientServices, OpenFormAction.BlockUntilShown);
		await ctx.RenderFormAsync(() => form = new Form() { Text = "Main Form" });
		var shownTCS = new TaskCompletionSource();
		var showDialogTask = ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			colorDialog = new ColorDialog();
			colorDialog.Closed += (sender, args) => formClosed.SetResult(true);
			colorDialog.Shown += (sender, args) => shownTCS.SetResult();
			using (ctx.WinzorDispatcher.WithContext(dispatcherContext))
			{
				colorDialog.ShowDialog();
			}
		});

		await shownTCS.Task;

		Assert.That(colorDialog, Is.Not.Null);
		var cancelButton = dispatcherContext.Rendered.FindAll(".colordialog__buttonspanel > button")[2];
		Assert.That(cancelButton.TextContent, Is.EqualTo("Cancel"));
		Assert.That(colorDialog.DialogResult, Is.EqualTo(DialogResult.None));
		await cancelButton.ClickAsync(new WebMouseEventArgs());
		await showDialogTask;
		Assert.That(colorDialog.DialogResult, Is.EqualTo(DialogResult.Cancel));
		Assert.That(await formClosed.Task.WithTimeout(TimeSpan.FromSeconds(3)), Is.EqualTo(true));
	}

	[Test]
	public async Task ColorDialogResetColorValueOnCancelColorDialog()
	{
		using var ctx = new WinzorTestContext();
		ColorDialog colorDialog = null;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			colorDialog = new ColorDialog();
			return colorDialog;
		});

		var cancelButton = rendered.FindAll(".colordialog__buttonspanel > button")[2];

		Assert.That(colorDialog, Is.Not.Null);
		Assert.That(colorDialog.Color.ToArgb(), Is.EqualTo(Color.FromArgb(255, 0, 0, 0).ToArgb()));

		var basicColorSwatches = rendered.FindAll(".colordialog__basicpalette > .colordialog__colorswatch");
		await basicColorSwatches[0].ClickAsync(new WebMouseEventArgs());
		Assert.That(colorDialog.Color, Is.EqualTo(Color.FromArgb(255, 255, 128, 128)));

		basicColorSwatches = rendered.FindAll(".colordialog__basicpalette > .colordialog__colorswatch");
		await basicColorSwatches[1].ClickAsync(new WebMouseEventArgs());
		Assert.That(colorDialog.Color, Is.EqualTo(Color.FromArgb(255, 255, 255, 128)));

		await cancelButton.ClickAsync(new WebMouseEventArgs());
		Assert.That(colorDialog.Color, Is.EqualTo(Color.FromArgb(255, 255, 128, 128)));
	}

	[Test]
	public async Task ColorDialogColorSwatchesRendered()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() => new ColorDialog());

		Assert.That(rendered.Find(".colordialog__basicpalette"), Is.Not.Null);
		var basicColorSwatches = rendered.FindAll(".colordialog__basicpalette > .colordialog__colorswatch");
		Assert.That(basicColorSwatches.Count, Is.EqualTo(48));

		Assert.That(rendered.Find(".colordialog__custompalette"), Is.Not.Null);
		var customColorSwatches = rendered.FindAll(".colordialog__custompalette > .colordialog__colorswatch");
		Assert.That(customColorSwatches.Count, Is.EqualTo(16));
	}

	[Test]
	public async Task ColorDialogColorSwatchesContainColorVariable()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var colorDialog = new ColorDialog();
			var customColors = colorDialog.CustomColors;
			customColors[0] = 16711935;
			colorDialog.CustomColors = customColors;
			return colorDialog;
		});

		var basicColorSwatches = rendered.FindAll(".colordialog__basicpalette > .colordialog__colorswatch");
		Assert.That(basicColorSwatches[0].GetAttribute("style"), Does.Contain("--swatch-color: #ff8080"));
		Assert.That(basicColorSwatches[1].GetAttribute("style"), Does.Contain("--swatch-color: #ffff80"));

		var customColorSwatches = rendered.FindAll(".colordialog__custompalette > .colordialog__colorswatch");
		Assert.That(customColorSwatches[0].GetAttribute("style"), Does.Contain("--swatch-color: #ff00ff"));
		Assert.That(customColorSwatches[1].GetAttribute("style"), Does.Contain("--swatch-color: #ffffff"));
	}

	[Test]
	public async Task ColorDialogColorSwatchesClickSetSelectedColor()
	{
		using var ctx = new WinzorTestContext();
		ColorDialog colorDialog = null;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			colorDialog = new ColorDialog();
			var customColors = colorDialog.CustomColors;
			customColors[0] = 16711935;
			colorDialog.CustomColors = customColors;
			return colorDialog;
		});

		Assert.That(colorDialog, Is.Not.Null);
		Assert.That(colorDialog.Color.ToArgb(), Is.EqualTo(Color.FromArgb(255, 0, 0, 0).ToArgb()));

		var basicColorSwatches = rendered.FindAll(".colordialog__basicpalette > .colordialog__colorswatch");
		await basicColorSwatches[0].ClickAsync(new WebMouseEventArgs());
		Assert.That(colorDialog.Color, Is.EqualTo(Color.FromArgb(255, 255, 128, 128)));

		basicColorSwatches = rendered.FindAll(".colordialog__basicpalette > .colordialog__colorswatch");
		await basicColorSwatches[1].ClickAsync(new WebMouseEventArgs());
		Assert.That(colorDialog.Color, Is.EqualTo(Color.FromArgb(255, 255, 255, 128)));

		var customColorSwatches = rendered.FindAll(".colordialog__custompalette > .colordialog__colorswatch");
		await customColorSwatches[0].ClickAsync(new WebMouseEventArgs());
		Assert.That(colorDialog.Color, Is.EqualTo(Color.FromArgb(255, 255, 0, 255)));

		customColorSwatches = rendered.FindAll(".colordialog__custompalette > .colordialog__colorswatch");
		await customColorSwatches[1].ClickAsync(new WebMouseEventArgs());
		Assert.That(colorDialog.Color, Is.EqualTo(Color.FromArgb(255, 255, 255, 255)));
	}

	[Test]
	public async Task ColorDialogColorSwatchesSelectedWhenColorSet()
	{
		using var ctx = new WinzorTestContext();
		ColorDialog colorDialog = null;
		var rendered = await ctx.RenderFormAsync(() => colorDialog = new ColorDialog());

		var basicColorSwatches = rendered.FindAll(".colordialog__basicpalette > .colordialog__colorswatch");
		Assert.That(basicColorSwatches[40].ClassList, Does.Contain("colordialog__colorswatch--selected"));
		Assert.That(rendered.FindAll(".colordialog__colorswatch--selected").Count, Is.EqualTo(1));

		await colorDialog.InvokeWinzorDispatcherAsync(() =>
		{
			colorDialog.Color = Color.FromArgb(0x00FFFFFF);
		});

		basicColorSwatches = rendered.FindAll(".colordialog__basicpalette > .colordialog__colorswatch");
		Assert.That(basicColorSwatches[47].ClassList, Does.Contain("colordialog__colorswatch--selected"));
		Assert.That(rendered.FindAll(".colordialog__colorswatch--selected").Count, Is.EqualTo(1));

		await colorDialog.InvokeWinzorDispatcherAsync(() =>
		{
			colorDialog.Color = Color.Red;
		});

		basicColorSwatches = rendered.FindAll(".colordialog__basicpalette > .colordialog__colorswatch");
		Assert.That(basicColorSwatches[8].ClassList, Does.Contain("colordialog__colorswatch--selected"));
		Assert.That(rendered.FindAll(".colordialog__colorswatch--selected").Count, Is.EqualTo(1));
	}

	[Test]
	public async Task ColorDialogColorSwatchesSelectedSwatchHasStyleClass()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() => new ColorDialog());

		var basicColorSwatches = rendered.FindAll(".colordialog__basicpalette > .colordialog__colorswatch");
		Assert.That(basicColorSwatches[40].ClassList, Does.Contain("colordialog__colorswatch--selected"));
		Assert.That(rendered.FindAll(".colordialog__colorswatch--selected").Count, Is.EqualTo(1));

		await basicColorSwatches[0].ClickAsync(new WebMouseEventArgs());
		basicColorSwatches = rendered.FindAll(".colordialog__basicpalette > .colordialog__colorswatch");
		Assert.That(basicColorSwatches[0].ClassList, Does.Contain("colordialog__colorswatch--selected"));
		Assert.That(rendered.FindAll(".colordialog__colorswatch--selected").Count, Is.EqualTo(1));

		await basicColorSwatches[15].ClickAsync(new WebMouseEventArgs());
		basicColorSwatches = rendered.FindAll(".colordialog__basicpalette > .colordialog__colorswatch");
		Assert.That(basicColorSwatches[15].ClassList, Does.Contain("colordialog__colorswatch--selected"));
		Assert.That(rendered.FindAll(".colordialog__colorswatch--selected").Count, Is.EqualTo(1));

		var customColorSwatches = rendered.FindAll(".colordialog__custompalette > .colordialog__colorswatch");
		await customColorSwatches[0].ClickAsync(new WebMouseEventArgs());
		customColorSwatches = rendered.FindAll(".colordialog__custompalette > .colordialog__colorswatch");
		Assert.That(customColorSwatches[0].ClassList, Does.Contain("colordialog__colorswatch--selected"));
		Assert.That(rendered.FindAll(".colordialog__colorswatch--selected").Count, Is.EqualTo(1));

		await customColorSwatches[10].ClickAsync(new WebMouseEventArgs());
		customColorSwatches = rendered.FindAll(".colordialog__custompalette > .colordialog__colorswatch");
		Assert.That(customColorSwatches[10].ClassList, Does.Contain("colordialog__colorswatch--selected"));
		Assert.That(rendered.FindAll(".colordialog__colorswatch--selected").Count, Is.EqualTo(1));
	}

	[Test, WithPlaywrightPage]
	public async Task ColorDialogTitleIsSet()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new ColorDialog();
			return form;
		}, formClassName: ".colordialog");
		await page.WaitForSelectorAsync(".colordialog");
		Assert.That(async () => await page.TitleAsync(), Is.EqualTo("Color").After(1000, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task ColorDialogCustomColorPickerInitializesValues()
	{
		await using var ctx = new InMemoryTestServerContext();

		ColorDialog colorDialog = null;
		var page = await ctx.LoadFormAsync(() => colorDialog = new ColorDialog(), formClassName: ".colordialog");

		await (await page.WaitForSelectorAsync(".colordialog__buttonspanel > button:first-child")).ClickAsync();

		await AssertHSLInputValuesAsync(page, 160, 0, 0);
		await AssertRGBInputValuesAsync(page, 0, 0, 0);
		await AssertColorPickerGuidePositionAsync(page, 67, 100);
		await AssertLuminanceGuidePositionAsync(page, 0);
		await AssertPreviewColorAsync(page, "#000000", "#808080");
	}

	[Test, WithPlaywrightPage]
	public async Task ColorDialogCustomColorPickerLoadsSelectedColorValues()
	{
		await using var ctx = new InMemoryTestServerContext();

		ColorDialog colorDialog = null;
		var page = await ctx.LoadFormAsync(() => colorDialog = new ColorDialog(), formClassName: ".colordialog");
		Assert.That(colorDialog, Is.Not.Null);

		await (await page.WaitForSelectorAsync(".colordialog__buttonspanel > button:first-child")).ClickAsync();

		await AssertHSLInputValuesAsync(page, 160, 0, 0);
		await AssertRGBInputValuesAsync(page, 0, 0, 0);
		await AssertColorPickerGuidePositionAsync(page, 67, 100);
		await AssertLuminanceGuidePositionAsync(page, 0);
		await AssertPreviewColorAsync(page, "#000000", "#808080");

		await (await page.WaitForSelectorAsync(".colordialog__basicpalette > button:nth-child(9)")).ClickAsync();

		await AssertHSLInputValuesAsync(page, 0, 240, 120);
		await AssertRGBInputValuesAsync(page, 255, 0, 0);
		await AssertColorPickerGuidePositionAsync(page, 0, 0);
		await AssertLuminanceGuidePositionAsync(page, 50);
		await AssertPreviewColorAsync(page, "#ff0000", "#ff0000");
	}

	[Test, WithPlaywrightPage]
	public async Task ColorDialogCustomColorPickerUpdatesFromSelectedColor()
	{
		await using var ctx = new InMemoryTestServerContext();

		ColorDialog colorDialog = null;
		var page = await ctx.LoadFormAsync(() => colorDialog = new ColorDialog(), formClassName: ".colordialog");
		Assert.That(colorDialog, Is.Not.Null);

		await (await page.WaitForSelectorAsync(".colordialog__basicpalette > button:nth-child(9)")).ClickAsync();
		await (await page.WaitForSelectorAsync(".colordialog__buttonspanel > button:first-child")).ClickAsync();

		await AssertHSLInputValuesAsync(page, 0, 240, 120);
		await AssertRGBInputValuesAsync(page, 255, 0, 0);
		await AssertColorPickerGuidePositionAsync(page, 0, 0);
		await AssertLuminanceGuidePositionAsync(page, 50);
		await AssertPreviewColorAsync(page, "#ff0000", "#ff0000");
	}

	[Test, WithPlaywrightPage]
	public async Task ColorDialogCustomColorPickerSpectrumUpdatesOnMouseUp()
	{
		await using var ctx = new InMemoryTestServerContext();
		ColorDialog colorDialog = null;
		var page = await ctx.LoadFormAsync(() => colorDialog = new ColorDialog(), formClassName: ".colordialog");

		await (await page.WaitForSelectorAsync(".colordialog__buttonspanel > button:first-child")).ClickAsync();

		await AssertHSLInputValuesAsync(page, 160, 0, 0);
		await AssertRGBInputValuesAsync(page, 0, 0, 0);
		await AssertColorPickerGuidePositionAsync(page, 67, 100);
		await AssertLuminanceGuidePositionAsync(page, 0);
		await AssertPreviewColorAsync(page, "#000000", "#808080");

		var clickArea = await page.WaitForSelectorAsync(".colordialog__spectrum");
		await clickArea.HoverAsync();
		await page.Mouse.DownAsync();
		await page.Mouse.UpAsync();

		await AssertHSLInputValuesAsync(page, 119, 120, 0);
		await AssertRGBInputValuesAsync(page, 0, 0, 0);
		await AssertColorPickerGuidePositionAsync(page, 50, 50);
		await AssertLuminanceGuidePositionAsync(page, 0);
		await AssertPreviewColorAsync(page, "#000000", "#40c0bc");
	}

	[Test, WithPlaywrightPage]
	public async Task ColorDialogCustomColorPickerSpectrumUpdatesOnMouseMove()
	{
		await using var ctx = new InMemoryTestServerContext();
		ColorDialog colorDialog = null;
		var page = await ctx.LoadFormAsync(() => colorDialog = new ColorDialog(), formClassName: ".colordialog");

		await (await page.WaitForSelectorAsync(".colordialog__buttonspanel > button:first-child")).ClickAsync();

		await AssertHSLInputValuesAsync(page, 160, 0, 0);
		await AssertRGBInputValuesAsync(page, 0, 0, 0);
		await AssertColorPickerGuidePositionAsync(page, 67, 100);
		await AssertLuminanceGuidePositionAsync(page, 0);
		await AssertPreviewColorAsync(page, "#000000", "#808080");

		await MouseDownAndMoveToCenterOfElementAsync(page, ".colordialog__spectrum");

		await AssertHSLInputValuesAsync(page, 119, 120, 0);
		await AssertRGBInputValuesAsync(page, 0, 0, 0);
		await AssertColorPickerGuidePositionAsync(page, 50, 50);
		await AssertLuminanceGuidePositionAsync(page, 0);
		await AssertPreviewColorAsync(page, "#000000", "#40c0bc");
	}

	[Test, WithPlaywrightPage]
	public async Task ColorDialogCustomColorPickerLuminanceBarUpdatesOnMouseUp()
	{
		await using var ctx = new InMemoryTestServerContext();
		ColorDialog colorDialog = null;
		var page = await ctx.LoadFormAsync(() => colorDialog = new ColorDialog(), formClassName: ".colordialog");

		await (await page.WaitForSelectorAsync(".colordialog__buttonspanel > button:first-child")).ClickAsync();

		await AssertHSLInputValuesAsync(page, 160, 0, 0);
		await AssertRGBInputValuesAsync(page, 0, 0, 0);
		await AssertColorPickerGuidePositionAsync(page, 67, 100);
		await AssertLuminanceGuidePositionAsync(page, 0);
		await AssertPreviewColorAsync(page, "#000000", "#808080");

		var clickArea = await page.WaitForSelectorAsync(".colordialog__luminanceclickarea");
		await clickArea.HoverAsync();
		await page.Mouse.DownAsync();
		await page.Mouse.UpAsync();

		await AssertHSLInputValuesAsync(page, 160, 0, 120);
		await AssertRGBInputValuesAsync(page, 128, 128, 128);
		await AssertColorPickerGuidePositionAsync(page, 67, 100);
		await AssertLuminanceGuidePositionAsync(page, 50);
		await AssertPreviewColorAsync(page, "#808080", "#808080");
	}

	[Test, WithPlaywrightPage]
	public async Task ColorDialogCustomColorPickerLuminanceBarUpdatesOnMouseMove()
	{
		await using var ctx = new InMemoryTestServerContext();
		ColorDialog colorDialog = null;
		var page = await ctx.LoadFormAsync(() => colorDialog = new ColorDialog(), formClassName: ".colordialog");

		await (await page.WaitForSelectorAsync(".colordialog__buttonspanel > button:first-child")).ClickAsync();

		await AssertHSLInputValuesAsync(page, 160, 0, 0);
		await AssertRGBInputValuesAsync(page, 0, 0, 0);
		await AssertColorPickerGuidePositionAsync(page, 67, 100);
		await AssertLuminanceGuidePositionAsync(page, 0);
		await AssertPreviewColorAsync(page, "#000000", "#808080");

		await MouseDownAndMoveToCenterOfElementAsync(page, ".colordialog__luminanceclickarea");

		await AssertHSLInputValuesAsync(page, 160, 0, 120);
		await AssertRGBInputValuesAsync(page, 128, 128, 128);
		await AssertColorPickerGuidePositionAsync(page, 67, 100);
		await AssertLuminanceGuidePositionAsync(page, 50);
		await AssertPreviewColorAsync(page, "#808080", "#808080");
	}

	[Test, WithPlaywrightPage]
	public async Task ColorDialogCustomColorPickerUpdatesFromHSLValueChange()
	{
		await using var ctx = new InMemoryTestServerContext();
		ColorDialog colorDialog = null;
		var page = await ctx.LoadFormAsync(() => colorDialog = new ColorDialog(), formClassName: ".colordialog");

		await (await page.WaitForSelectorAsync(".colordialog__buttonspanel > button:first-child")).ClickAsync();

		await AssertHSLInputValuesAsync(page, 160, 0, 0);
		await AssertRGBInputValuesAsync(page, 0, 0, 0);
		await AssertColorPickerGuidePositionAsync(page, 67, 100);
		await AssertLuminanceGuidePositionAsync(page, 0);
		await AssertPreviewColorAsync(page, "#000000", "#808080");

		var lumInput = page.Locator("input[name=lum]");
		await lumInput.SelectTextAsync();
		await lumInput.PressSequentiallyAsync("120");
		await lumInput.EvaluateAsync("e => e.blur()");

		await AssertHSLInputValuesAsync(page, 160, 0, 120);
		await AssertRGBInputValuesAsync(page, 128, 128, 128);
		await AssertColorPickerGuidePositionAsync(page, 67, 100);
		await AssertLuminanceGuidePositionAsync(page, 50);
		await AssertPreviewColorAsync(page, "#808080", "#808080");

		var satInput = page.Locator("input[name=sat]");
		await satInput.SelectTextAsync();
		await satInput.PressSequentiallyAsync("240");
		await satInput.EvaluateAsync("e => e.blur()");

		await AssertHSLInputValuesAsync(page, 160, 240, 120);
		await AssertRGBInputValuesAsync(page, 0, 0, 255);
		await AssertColorPickerGuidePositionAsync(page, 67, 0);
		await AssertLuminanceGuidePositionAsync(page, 50);
		await AssertPreviewColorAsync(page, "#0000ff", "#0000ff");

		var hueInput = page.Locator("input[name=hue]");
		await hueInput.SelectTextAsync();
		await hueInput.PressSequentiallyAsync("0");
		await hueInput.EvaluateAsync("e => e.blur()");

		await AssertHSLInputValuesAsync(page, 0, 240, 120);
		await AssertRGBInputValuesAsync(page, 255, 0, 0);
		await AssertColorPickerGuidePositionAsync(page, 0, 0);
		await AssertLuminanceGuidePositionAsync(page, 50);
		await AssertPreviewColorAsync(page, "#ff0000", "#ff0000");
	}

	[Test, WithPlaywrightPage]
	public async Task ColorDialogCustomColorPickerUpdatesFromRGBValueChange()
	{
		await using var ctx = new InMemoryTestServerContext();

		ColorDialog colorDialog = null;
		var page = await ctx.LoadFormAsync(() => colorDialog = new ColorDialog(), formClassName: ".colordialog");

		await (await page.WaitForSelectorAsync(".colordialog__buttonspanel > button:first-child")).ClickAsync();

		await AssertHSLInputValuesAsync(page, 160, 0, 0);
		await AssertRGBInputValuesAsync(page, 0, 0, 0);
		await AssertColorPickerGuidePositionAsync(page, 67, 100);
		await AssertLuminanceGuidePositionAsync(page, 0);
		await AssertPreviewColorAsync(page, "#000000", "#808080");

		var redInput = page.Locator("input[name=red]");
		await redInput.SelectTextAsync();
		await redInput.PressSequentiallyAsync("255");
		await redInput.EvaluateAsync("e => e.blur()");

		await AssertHSLInputValuesAsync(page, 0, 240, 120);
		await AssertRGBInputValuesAsync(page, 255, 0, 0);
		await AssertColorPickerGuidePositionAsync(page, 0, 0);
		await AssertLuminanceGuidePositionAsync(page, 50);
		await AssertPreviewColorAsync(page, "#ff0000", "#ff0000");

		var greenInput = page.Locator("input[name=green]");
		await greenInput.SelectTextAsync();
		await greenInput.PressSequentiallyAsync("255");
		await greenInput.EvaluateAsync("e => e.blur()");

		await AssertHSLInputValuesAsync(page, 40, 240, 120);
		await AssertRGBInputValuesAsync(page, 255, 255, 0);
		await AssertColorPickerGuidePositionAsync(page, 17, 0);
		await AssertLuminanceGuidePositionAsync(page, 50);
		await AssertPreviewColorAsync(page, "#ffff00", "#ffff00");

		var blueInput = page.Locator("input[name=blue]");
		await blueInput.SelectTextAsync();
		await blueInput.PressSequentiallyAsync("255");
		await blueInput.EvaluateAsync("e => e.blur()");

		await AssertHSLInputValuesAsync(page, 160, 0, 240);
		await AssertRGBInputValuesAsync(page, 255, 255, 255);
		await AssertColorPickerGuidePositionAsync(page, 67, 100);
		await AssertLuminanceGuidePositionAsync(page, 100);
		await AssertPreviewColorAsync(page, "#ffffff", "#808080");
	}

	[Test, WithPlaywrightPage]
	public async Task ColorDialogAddCustomColor()
	{
		await using var ctx = new InMemoryTestServerContext();

		ColorDialog colorDialog = null;
		var page = await ctx.LoadFormAsync(() => colorDialog = new ColorDialog(), formClassName: ".colordialog");

		await (await page.WaitForSelectorAsync(".colordialog__basicpalette > button:nth-child(9)")).ClickAsync();
		await (await page.WaitForSelectorAsync(".colordialog__buttonspanel > button:first-child")).ClickAsync();

		var addCustomColor = await page.WaitForSelectorAsync(".colordialog__addbutton");

		AssertCustomPaletteSwatchColor(page, 1, "#ffffff");
		await addCustomColor.ClickAsync();
		AssertCustomPaletteSwatchColor(page, 1, "#ff0000");
	}

	[Test, WithPlaywrightPage]
	public async Task ColorDialogAddCustomColorCorrectOrder()
	{
		await using var ctx = new InMemoryTestServerContext();

		ColorDialog colorDialog = null;
		var page = await ctx.LoadFormAsync(() => colorDialog = new ColorDialog(), formClassName: ".colordialog");

		await (await page.WaitForSelectorAsync(".colordialog__basicpalette > button:nth-child(9)")).ClickAsync();
		await (await page.WaitForSelectorAsync(".colordialog__buttonspanel > button:first-child")).ClickAsync();

		var addCustomColor = await page.WaitForSelectorAsync(".colordialog__addbutton");

		AssertCustomPaletteSwatchColor(page, 1, "#ffffff");
		await addCustomColor.ClickAsync();
		AssertCustomPaletteSwatchColor(page, 1, "#ff0000");

		AssertCustomPaletteSwatchColor(page, 2, "#ffffff");
		await addCustomColor.ClickAsync();
		AssertCustomPaletteSwatchColor(page, 2, "#ff0000");

		await (await page.WaitForSelectorAsync(".colordialog__custompalette > button:nth-child(15)")).ClickAsync(); // The next color should now be added to the 15th slot
		await (await page.WaitForSelectorAsync(".colordialog__basicpalette > button:nth-child(19)")).ClickAsync(); // Change the color to green

		AssertCustomPaletteSwatchColor(page, 3, "#ffffff");
		AssertCustomPaletteSwatchColor(page, 15, "#ffffff");
		await addCustomColor.ClickAsync();
		AssertCustomPaletteSwatchColor(page, 3, "#ffffff");
		AssertCustomPaletteSwatchColor(page, 15, "#00ff00");

		AssertCustomPaletteSwatchColor(page, 3, "#ffffff");
		AssertCustomPaletteSwatchColor(page, 16, "#ffffff");
		await addCustomColor.ClickAsync();
		AssertCustomPaletteSwatchColor(page, 3, "#ffffff");
		AssertCustomPaletteSwatchColor(page, 16, "#00ff00");

		AssertCustomPaletteSwatchColor(page, 1, "#ff0000");
		await addCustomColor.ClickAsync();
		AssertCustomPaletteSwatchColor(page, 1, "#00ff00");
	}

	[Test, WithPlaywrightPage]
	public async Task ColorDialogCustomColorPickerHideSpectrumGuideWhileDragging()
	{
		await using var ctx = new InMemoryTestServerContext();

		ColorDialog colorDialog = null;
		var page = await ctx.LoadFormAsync(() => colorDialog = new ColorDialog(), formClassName: ".colordialog");

		await (await page.WaitForSelectorAsync(".colordialog__buttonspanel > button:first-child")).ClickAsync();

		var colorSpectrumGuide = await page.WaitForSelectorAsync(".colordialog__spectrumguide", new PageWaitForSelectorOptions() { State = WaitForSelectorState.Attached });
		Assert.That(async () => await GetCssValue(colorSpectrumGuide, "visibility"), Is.EqualTo("visible").After(1000, 100));

		await MouseDownAndMoveToCenterOfElementAsync(page, ".colordialog__spectrum");
		Assert.That(async () => await GetCssValue(colorSpectrumGuide, "visibility"), Is.EqualTo("hidden").After(1000, 100));

		await page.Mouse.UpAsync();
		Assert.That(async () => await GetCssValue(colorSpectrumGuide, "visibility"), Is.EqualTo("visible").After(1000, 100));
	}

	[Test]
	public async Task ColorDialogOpenAndCloseMultipleTimes()
	{
		using var ctx = new WinzorTestContext();
		ColorDialog colorDialog = null;
		var formClosed = new TaskCompletionSource<bool>();
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			colorDialog = new ColorDialog();
			colorDialog.Closed += (sender, e) =>
			{
				formClosed.SetResult(true);
			};
		});

		for (var i = 0; i < 3; i++)
		{
			var dispatcherContext = new DispatcherContext(ctx, ctx.DefaultClientServices, OpenFormAction.None);
			var showDialogTask = ctx.WinzorDispatcher.InvokeAsync(() =>
			{
				using (ctx.WinzorDispatcher.WithContext(dispatcherContext))
				{
					colorDialog.ShowDialog();
				}
			});
			await dispatcherContext.OpenFormTcs.Task;
			Assert.That(() => dispatcherContext.Rendered, Is.Not.Null);

			await colorDialog.InvokeWinzorDispatcherAsync(colorDialog.Close);
			var isClosed = await formClosed.Task;
			formClosed = new TaskCompletionSource<bool>();
			Assert.That(isClosed, Is.True);

			await showDialogTask;
		}
	}

	[Test]
	public async Task FormWidthResetOnFullOpenFalse()
	{
		using var ctx = new WinzorTestContext();
		ColorDialog colorDialog = null;
		var rendered = await ctx.RenderFormAsync(() => colorDialog = new ColorDialog());

		Assert.That(colorDialog, Is.Not.Null);
		Assert.That(colorDialog.Width, Is.EqualTo(220));
		Assert.That(colorDialog.Height, Is.EqualTo(300));
		await rendered.FindAll(".colordialog__buttonspanel > button")[0].ClickAsync(new WebMouseEventArgs());
		Assert.That(colorDialog.FullOpen, Is.EqualTo(true));
		Assert.That(colorDialog.Width, Is.EqualTo(440));
		Assert.That(colorDialog.Height, Is.EqualTo(300));

		await colorDialog.InvokeWinzorDispatcherAsync(() => colorDialog.FullOpen = false);

		Assert.That(colorDialog, Is.Not.Null);
		Assert.That(colorDialog.Width, Is.EqualTo(220));
		Assert.That(colorDialog.Height, Is.EqualTo(300));
	}

	[Test, WithPlaywrightPage]
	public async Task ColorDialogStylesAndCustomColorsWhenReOpen()
	{
		await using var ctx = new InMemoryTestServerContext();
		ColorDialog colorDialog = null;
		var redColor = "#ff0000";
		var whiteColor = "#ffffff";
		var page = await ctx.LoadFormAsync(() =>
		{
			colorDialog = new ColorDialog { FullOpen = true };
			Assert.That(colorDialog.ClientWindowOpenSent, Is.EqualTo(false));
			return colorDialog;
		}, formClassName: ".colordialog");

		var colorSelection = page.Locator(".colordialog__basicpalette > button:nth-child(9)");
		var addCustomColor = page.Locator(".colordialog__addbutton");

		AssertProperties(colorDialog);
		await colorSelection.ClickAsync();
		AssertCustomPaletteSwatchColor(page, 1, whiteColor);
		await addCustomColor.ClickAsync();
		AssertCustomPaletteSwatchColor(page, 1, redColor);

		await colorDialog.InvokeWinzorDispatcherAsync(() => colorDialog.FullOpen = true);
		AssertProperties(colorDialog);
		AssertCustomPaletteSwatchColor(page, 1, redColor);
		await addCustomColor.ClickAsync();
		AssertCustomPaletteSwatchColor(page, 2, redColor);

		await colorDialog.InvokeWinzorDispatcherAsync(() => colorDialog.FullOpen = false);
		AssertProperties(colorDialog);
		AssertCustomPaletteSwatchColor(page, 1, redColor);
		AssertCustomPaletteSwatchColor(page, 2, redColor);

		await colorDialog.InvokeWinzorDispatcherAsync(() => colorDialog.Reset());
		AssertProperties(colorDialog);
		AssertCustomPaletteSwatchColor(page, 1, whiteColor);
		AssertCustomPaletteSwatchColor(page, 2, whiteColor);

		void AssertProperties(ColorDialog colorDialog)
		{
			Assert.That(colorDialog.MaximizeBox, Is.EqualTo(false));
			Assert.That(colorDialog.MinimizeBox, Is.EqualTo(false));
			Assert.That(colorDialog.StartPosition, Is.EqualTo(FormStartPosition.CenterScreen));
			Assert.That(colorDialog.FormBorderStyle, Is.EqualTo(FormBorderStyle.FixedDialog));
			Assert.That(colorDialog.ClientWindowOpenSent, Is.EqualTo(true));
		}
	}

	async Task AssertColorPickerGuidePositionAsync(IPage page, int expectedXPosition, int expectedYPosition)
	{
		var colorPicker = await page.WaitForSelectorAsync(".colordialog__spectrum");
		Assert.That(async () => await GetCssVariablePercentAsync(colorPicker, "spectrumguideleftoffset"), Is.EqualTo(expectedXPosition).Within(1).After(1000, 100), "Incorrect spectrum guide x value.");
		Assert.That(async () => await GetCssVariablePercentAsync(colorPicker, "spectrumguidetopoffset"), Is.EqualTo(expectedYPosition).Within(1).After(1000, 100), "Incorrect spectrum guide y value.");
	}

	async Task AssertLuminanceGuidePositionAsync(IPage page, int expectedPosition)
	{
		var luminanceGuide = await page.WaitForSelectorAsync(".colordialog__luminanceguide");
		Assert.That(async () => await GetCssVariablePercentAsync(luminanceGuide, "luminanceguideoffset"), Is.EqualTo(expectedPosition).Within(1).After(1000, 100), "Incorrect luminance guide y value.");
	}

	async Task AssertPreviewColorAsync(IPage page, string expectedPreviewColor, string expectedLuminancePreviewColor)
	{
		var colorPicker = await page.WaitForSelectorAsync(".colordialog");
		Assert.That(async () => IsEqualWithTolerance(await GetCssVariableStringAsync(colorPicker, "customcolorpreview"), expectedPreviewColor, 1), Is.True.After(1000, 100), "Incorrect preview color value.");
		Assert.That(async () => IsEqualWithTolerance(await GetCssVariableStringAsync(colorPicker, "customcolorluminancepreview"), expectedLuminancePreviewColor, 1), Is.True.After(1000, 100), "Incorrect luminance preview color value.");
	}

	bool IsEqualWithTolerance(string actualColor, string expectedColor, int tolerance)
	{
		var actual = Color.FromArgb(int.Parse(actualColor.TrimStart('#'), Globalization.NumberStyles.HexNumber));
		var expected = Color.FromArgb(int.Parse(expectedColor.TrimStart('#'), Globalization.NumberStyles.HexNumber));
		return Math.Abs(actual.R - expected.R) <= tolerance
			&& Math.Abs(actual.G - expected.G) <= tolerance
			&& Math.Abs(actual.B - expected.B) <= tolerance;
	}

	async Task AssertHSLInputValuesAsync(IPage page, int expectedHue, int expectedSat, int expectedLum)
	{
		var hueInput = await page.WaitForSelectorAsync("input[name=hue]");
		Assert.That<Task<double>>(async () => await ParseInputValueAsync(hueInput), Is.EqualTo(expectedHue).Within(1).After(1000, 100), "Incorrect hue input value.");
		var satInput = await page.WaitForSelectorAsync("input[name=sat]");
		Assert.That<Task<double>>(async () => await ParseInputValueAsync(satInput), Is.EqualTo(expectedSat).Within(1).After(1000, 100), "Incorrect sat input value.");
		var lumInput = await page.WaitForSelectorAsync("input[name=lum]");
		Assert.That<Task<double>>(async () => await ParseInputValueAsync(lumInput), Is.EqualTo(expectedLum).Within(1).After(1000, 100), "Incorrect lum input value.");
	}

	async Task AssertRGBInputValuesAsync(IPage page, int expectedRed, int expectedGreen, int expectedBlue)
	{
		var redInput = await page.WaitForSelectorAsync("input[name=red]");
		Assert.That<Task<double>>(async () => await ParseInputValueAsync(redInput), Is.EqualTo(expectedRed).Within(1).After(1000, 100), "Incorrect red input value.");
		var greenInput = await page.WaitForSelectorAsync("input[name=green]");
		Assert.That<Task<double>>(async () => await ParseInputValueAsync(greenInput), Is.EqualTo(expectedGreen).Within(1).After(1000, 100), "Incorrect green input value.");
		var blueInput = await page.WaitForSelectorAsync("input[name=blue]");
		Assert.That<Task<double>>(async () => await ParseInputValueAsync(blueInput), Is.EqualTo(expectedBlue).Within(1).After(1000, 100), "Incorrect blue input value.");
	}

	async Task<int> ParseInputValueAsync(IElementHandle input)
	{
		var value = await input.InputValueAsync();
		if (int.TryParse(value, out var parsedValue))
		{
			return parsedValue;
		}
		return int.MaxValue;
	}

	async Task<string> GetCssValue(IElementHandle element, string cssProperty) => (await element.EvaluateAsync($"e => window.getComputedStyle(e).getPropertyValue('{cssProperty}')")).Value.ToString();

	async Task<string> GetCssVariableStringAsync(IElementHandle element, string cssVariableName) => await GetCssValue(element, $"--{cssVariableName}");

	async Task<double> GetCssVariablePercentAsync(IElementHandle element, string cssVariableName) => double.Parse((await GetCssVariableStringAsync(element, cssVariableName)).TrimEnd(new char[] { '%', ' ' }));

	async Task MouseDownAndMoveToCenterOfElementAsync(IPage page, string selector)
	{
		var element = await page.WaitForSelectorAsync(selector);
		var elementRect = await element.BoundingBoxAsync();
		await page.Mouse.MoveAsync(elementRect.X, elementRect.Y);
		await page.Mouse.DownAsync();
		await page.Mouse.MoveAsync(elementRect.X + (elementRect.Width / 2), elementRect.Y + (elementRect.Height / 2));
	}

	void AssertCustomPaletteSwatchColor(IPage page, int index, string expectedColor) => Assert.That(async () => await (await page.QuerySelectorAsync($".colordialog__custompalette > button:nth-child({index})")).GetAttributeAsync("style"), Is.EqualTo($"--swatch-color: {expectedColor}").After(1000, 100));
}
