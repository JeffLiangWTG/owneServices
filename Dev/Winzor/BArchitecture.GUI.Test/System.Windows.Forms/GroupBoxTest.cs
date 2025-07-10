using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Bunit;
using NUnit.Framework;
using WinzorTestFramework;
using WTG.PlaywrightTesting;

namespace System.Windows.Forms;

class GroupBoxTest
{
	[Test]
	public async Task MouseDownEvent()
	{
		await ControlAssert.ImplementsEventAsync<GroupBox, MouseEventHandler>(nameof(GroupBox.MouseDown), a => new MouseEventHandler((o, e) => a()), ".groupbox", e => e.MouseDown());
	}

	[Test]
	public async Task GroupBoxDefaultValues()
	{
		using var ctx = new WinzorTestContext();
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var control = new GroupBox();
			Assert.That(control.CanSelect, Is.False);
			Assert.That(control.FlatStyle, Is.EqualTo(FlatStyle.Standard));
		});
	}

	[Test]
	public async Task GroupBox_InlineStyles()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var groupBox = new GroupBox() { Top = 100, Left = 200, Width = 300, Height = 400 };
			form.Controls.Add(groupBox);
			return form;
		});

		var groupBoxWrapper = rendered.Find("fieldset").ParentElement;

		Assert.That(groupBoxWrapper.GetAttribute("style"), Is.EqualTo("position:absolute;width:300px;height:400px;top:100px;left:200px;background-color:var(--color-control);"));
	}

	[Test]
	public async Task GroupBox_ContentShouldBeWrappedByGroupBox()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var groupBox = new GroupBox();
			groupBox.Controls.Add(new TextBox());
			groupBox.Controls.Add(new TextBox());
			form.Controls.Add(groupBox);
			return form;
		});

		var groupBox = rendered.FindAll("fieldset").Single();
		Assert.That(groupBox.Children.Where(c => c.LocalName == "input").ToList(), Has.Count.EqualTo(2));
	}

	[Test]
	public async Task GroupBox_CaptionShouldBeRendered()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var groupBox = new GroupBox();
			groupBox.Text = "The Captain";
			form.Controls.Add(groupBox);
			return form;
		});

		var legend = rendered.FindAll("legend");
		Assert.That(legend.Single().TextContent, Is.EqualTo("The Captain"));
	}

	[TestCase("", 0, TestName = "{m}_NoCaption")]
	[TestCase("text", 1, TestName = "{m}_Caption")]
	public async Task GroupBoxBorderNoGap(string legendText, int expectedLegendNum)
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var groupBox = new GroupBox();
			groupBox.Text = legendText;
			form.Controls.Add(groupBox);
			return form;
		});

		Assert.That(rendered.FindAll("legend").Count, Is.EqualTo(expectedLegendNum));
	}

	[Test, WithPlaywrightPage]
	public async Task GroupBox_BorderShouldBeRendered()
	{
		await using var ctx = new InMemoryTestServerContext();
		Form form = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form();
			var groupBox = new GroupBox();
			form.Controls.Add(groupBox);
			return form;
		});

		var fieldset = await page.WaitForSelectorAsync($".groupbox");

		// using a fieldset will render border by default also should have a groupbox CSS class
		Assert.That(async () => await fieldset.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('margin-top')"), Is.EqualTo("6.5px"));
		Assert.That(async () => await fieldset.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('padding')"), Is.EqualTo("0px"));
		Assert.That(async () => await fieldset.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('border-style')"), Is.EqualTo("solid"));
		Assert.That(async () => await fieldset.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('border-color')"), Is.EqualTo("rgb(220, 220, 220)"));
		Assert.That(async () => await fieldset.EvaluateAsync<float>("e => parseFloat(window.getComputedStyle(e).getPropertyValue('border-width'))"), Is.InRange(0.5, 1));
		// We would prefer that the border is a hairline 0.5px, however Playwright can only show a computed value here if it's not running in headless mode.
		// Headless mode will run at 100% scaling and the border will be 1px.
	}

	const int captionHeight = 13;

	[Test]
	[TestCase(TestName = "Padding-AllSides-DefaultsTo-3")]
	public async Task PaddingAllSidesDefaultsTo3()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = BuildFormWithLabel(out GroupBox groupBox);
			return form;
		});
		var styleStr = GetRenderedLabelStyle(rendered);
		var defaultPadding = 3;
		var expectedStyleStr = $"top:{captionHeight + defaultPadding}px;left:{defaultPadding}px;";
		Assert.That(styleStr, Does.Contain(expectedStyleStr));
	}

	[Test]
	[TestCase(0, TestName = "Padding-AllSides-0")]
	[TestCase(4, TestName = "Padding-AllSides-4")]
	[TestCase(5, TestName = "Padding-AllSides-5")]
	public async Task RespectsPaddingOverridesAll(int padding)
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = BuildFormWithLabel(out GroupBox groupBox);
			groupBox.Padding = new Padding(padding);
			return form;
		});
		var styleStr = GetRenderedLabelStyle(rendered);
		var expectedStyleStr = $"top:{captionHeight + padding}px;left:{padding}px;";
		Assert.That(styleStr, Does.Contain(expectedStyleStr));
	}

	Form BuildFormWithLabel(out GroupBox groupBox)
	{
		var form = new Form();
		groupBox = new GroupBox();
		form.Controls.Add(groupBox);
		var label = new Label { Text = "The Label", Dock = DockStyle.Top };
		groupBox.Controls.Add(label);
		return form;
	}

	string GetRenderedLabelStyle(IRenderedComponent<ControlProxyComponent> rendered)
	{
		var labelEle = rendered.Find(".label");
		var styleStr = labelEle.GetAttribute("style");
		return styleStr;
	}

	[Test, Explicit] //Explicit until groupbox process mnemonic is implemented
	public async Task GroupBoxProcessMnemonicSelectsFirstControl()
	{
		using var ctx = new WinzorTestContext();
		TextBox textBox = null;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var groupBox = new GroupBox() { Text = "&Test" };
			textBox = new TextBox();
			groupBox.Controls.Add(textBox);
			form.Controls.Add(groupBox);
			return form;
		});
		var form = rendered.Find(".form");
		await rendered.KeyPressAsync(Keys.Alt | Keys.T, form);
		Assert.That(textBox.Focused, Is.True);
	}

	[Test]
	public async Task GroupBoxUseMnemonicFormatsTestWithMnemonic()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() => new GroupBox() { Text = "&Test" });
		Assert.That(rendered.Find(".groupbox__text").InnerHtml, Is.EqualTo("<span class=\"mnemonickey\">T</span>est"));
	}

	[TestCase("", false, 8, "6.5")]
	[TestCase("", false, 10, "8.5")]
	[TestCase(null, false, 12, "10")]
	[TestCase("Test", true, 10, "0")]
	public async Task GroupBoxShouldHaveCorrectTopMargin(string text, bool hasText, float fontSize, string topMarginValue)
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var groupBox = new GroupBox { Text = text, Font = new Font("Tahoma", fontSize) };
			form.Controls.Add(groupBox);
			return form;
		});

		var groupBox = rendered.Find(".groupbox");

		Assert.That(groupBox.Children.Any(e => e.ClassName.Contains("groupbox__text", StringComparison.Ordinal)), Is.EqualTo(hasText));
		if (hasText)
		{
			Assert.That(groupBox.GetAttribute("style"), Is.EqualTo(string.Empty));
		}
		else
		{
			Assert.That(groupBox.GetAttribute("style"), Does.Contain($"margin-top:{topMarginValue}"));
		}
	}

	[Test, WithPlaywrightPage]
	public async Task GroupBoxLegendIsNotBold()
	{
		await using var ctx = new InMemoryTestServerContext();
		Form form = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form();
			var groupBox = new GroupBox
			{
				Text = "123"
			};
			form.Controls.Add(groupBox);
			return form;
		});

		var buttonText = await page.WaitForSelectorAsync($".groupbox__text");

		Assert.That(async () => await buttonText.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('font-weight')"), Is.EqualTo("400"));
	}

	[TestCase("GroupBox", TestName = "{m}_GroupBoxHasText")]
	[TestCase(null, TestName = "{m}_GroupBoxHasNoText")]
	[WithPlaywrightPage]
	public async Task GroupBoxShouldHaveCorrectHeight(string groupBoxText)
	{
		await using var ctx = new InMemoryTestServerContext();
		GroupBox groupBox = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			var textBox1 = new TextBox() { Width = 100, Height = 20, Top = 50, Left = 20, Text = "TextBox1" };
			groupBox = new GroupBox() { Width = 200, Height = 200, Top = 0, Left = 0, Text = groupBoxText };
			groupBox.Controls.Add(textBox1);
			form.Controls.Add(groupBox);

			return form;
		});

		var groupBoxActualHeight = await (await page.WaitForSelectorAsync("fieldset")).EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('height')");
		var groupBoxExpectedHeight = groupBoxText is null ? groupBox.Height - groupBox.FontHeight * 0.5 : 200;

		Assert.That(groupBoxActualHeight, Is.EqualTo($"{groupBoxExpectedHeight}px"));
	}

	[Test, WithPlaywrightPage]
	public async Task GroupBoxTextMarginAndLabelTextPadding()
	{
		await using var ctx = new InMemoryTestServerContext();

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form { Width = 500, Height = 500 };

			GroupBox groupBox = new GroupBox { Text = "Descriptio" };
			Label label = new Label { Text = "Label" };
			groupBox.Controls.Add(label);
			form.Controls.Add(groupBox);
			return form;
		});

		//check property of the legend.groupbox__text to make sure that the headline "Description"  same as CW1
		var legendElement = page.Locator("legend.groupbox__text");
		Assert.That((await legendElement.GetComputedStyleAsync("margin-left")).AsPixels(), Is.EqualTo(0));
		Assert.That((await legendElement.GetComputedStyleAsync("font-size")).AsPixels(), Is.EqualTo(10.6667).Within(0.1));
		Assert.That(await legendElement.GetComputedStyleAsync("font-family"), Is.EqualTo("Tahoma"));

		//check .groupbox > .label > .label__text has padding-left: 1.5px
		var labelTextElement = page.Locator(".groupbox > .label > .label__text");
		Assert.That((await labelTextElement.GetComputedStyleAsync("padding-left")).AsPixels(), Is.EqualTo(1.5));
	}

	[Test]
	public async Task GroupBoxChangeFontInvokeCallOfFontChanged()
	{
		using var ctx = new WinzorTestContext();
		var callCount = 0;
		GroupBox control = null;

		await ctx.RenderControlOnFormAsync(() =>
		{
			control = new GroupBox() { Text = "Test" };
			control.FontChanged += (sender, e) => { callCount++; };
			return control;
		});
		Assert.That(control.Font.Name, Is.EqualTo(Control.DefaultFont.Name));

		await control.InvokeWinzorDispatcherAsync(() => control.Font = new Font("Arial", 10));
		Assert.That(callCount, Is.EqualTo(1));
		Assert.That(control.Font.Name, Is.EqualTo("Arial"));
		Assert.That(control.Font.Size, Is.EqualTo(10));
	}

	[Test]
	public async Task ToString_ReturnsExpected()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() => new GroupBox() { Text = "Test" });
		var control = rendered.GetControl<GroupBox>();
		Assert.That(control.ToString(), Is.EqualTo("System.Windows.Forms.GroupBox, Text: Test"));
	}
}
