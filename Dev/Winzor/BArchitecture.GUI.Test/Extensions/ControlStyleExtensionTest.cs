using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bunit;
using NUnit.Framework;
using WinzorTestFramework;

namespace WinzorFramework.Extensions;

public class ControlStyleExtensionTest
{
	[Test]
	public async Task StyleString()
	{
		// Arrange / Act
		using var ctx = new WinzorTestContext();
		Button button = null;
		await ctx.RenderControlOnFormAsync(() => button = new Button { Text = "buttontext" });

		// Assert
		Assert.That(button.LayoutStyle(), Is.EqualTo("position:absolute;width:75px;height:23px;top:0px;left:0px;"));
		Assert.That(button.LookStyle(), Is.EqualTo("background-color:var(--color-control);"));
		Assert.That(button.SizeStyle(), Is.EqualTo("width:75px;height:23px;"));
		Assert.That(button.ControlStyle(), Is.EqualTo("position:absolute;width:75px;height:23px;top:0px;left:0px;background-color:var(--color-control);"));
	}

	[TestCase(FontStyle.Bold, "font-weight:bold", TestName = "FontStyleBoldConversion")]
	[TestCase(FontStyle.Italic, "font-style:italic", TestName = "FontStyleItalicConversion")]
	[TestCase(FontStyle.Underline, "text-decoration: underline", TestName = "FontStyleUnderlineConversion")]
	[TestCase(FontStyle.Strikeout, "text-decoration: line-through", TestName = "FontStyleStrikeoutConversion")]
	[TestCase(FontStyle.Underline | FontStyle.Strikeout, "text-decoration: underline line-through", TestName = "FontStyleUnderlineAndStrikeoutConversion")]
	public async Task FontStyleString(FontStyle fontStyle, string expectedCssProperty)
	{
		// Arrange / Act
		using var ctx = new WinzorTestContext();
		Button button = null;
		await ctx.RenderControlOnFormAsync(() => button = new Button { Text = "buttontext", Left = 100, Top = 200, Width = 300, Height = 400, BackColor = Color.White, ForeColor = Color.Red, Font = new Font("arial", 11f, fontStyle) });

		// Assert
		Assert.That(button.LayoutStyle(), Is.EqualTo("position:absolute;width:300px;height:400px;top:200px;left:100px;"));
		Assert.That(button.LookStyle(), Is.EqualTo($"background-color:#FFFFFFFF;color:#FF0000FF;font-family:Arial;line-height:17px;font-size:11pt;{expectedCssProperty};"));
		Assert.That(button.SizeStyle(), Is.EqualTo("width:300px;height:400px;"));
		Assert.That(button.ControlStyle(), Is.EqualTo($"position:absolute;width:300px;height:400px;top:200px;left:100px;background-color:#FFFFFFFF;color:#FF0000FF;font-family:Arial;line-height:17px;font-size:11pt;{expectedCssProperty};"));
	}

	[Test]
	public async Task LabelStylesWithFont()
	{
		using var ctx = new WinzorTestContext();
		Label label1 = null;
		using Font font = new Font("Microsoft Sans Serif", 8f);
		await ctx.RenderControlOnFormAsync(() => label1 = new Label { Font = font, Text = "The Label", Left = 10, Top = 20, Width = 100, Height = 30 });

		Assert.That(label1.LayoutStyle(), Is.EqualTo("position:absolute;width:100px;height:30px;top:20px;left:10px;"));
		Assert.That(label1.LookStyle(), Is.EqualTo("background-color:var(--color-control);font-family:Microsoft Sans Serif;"));
		Assert.That(label1.SizeStyle(), Is.EqualTo("width:100px;height:30px;"));
		Assert.That(label1.ControlStyle(), Is.EqualTo("position:absolute;width:100px;height:30px;top:20px;left:10px;background-color:var(--color-control);font-family:Microsoft Sans Serif;"));
	}

	[Test]
	public async Task LabelBorderStyleClass()
	{
		using var ctx = new WinzorTestContext();
		Label label1, label2 = label1 = null;
		await ctx.RenderControlOnFormAsync(() => label1 = new Label { Text = string.Empty });
		await ctx.RenderControlOnFormAsync(() => label2 = new Label { Text = string.Empty, BorderStyle = BorderStyle.Fixed3D });

		Assert.That(label1.BorderStyle.BorderStyleClass(), Is.EqualTo("none"));
		Assert.That(label2.BorderStyle.BorderStyleClass(), Is.EqualTo("fixed3d"));
	}

	[Test]
	public async Task ControlShouldHaveCorrectFontColor()
	{
		using var ctx = new WinzorTestContext();
		Label subControl1, subControl2 = subControl1 = null;
		var panelStyle = string.Empty;

		await ctx.RenderControlOnFormAsync(() =>
		{
			var panel = new Panel();
			subControl1 = new Label();
			subControl2 = new Label();
			panel.ForeColor = Color.Black;
			subControl1.ForeColor = Color.Black;
			subControl2.ForeColor = Color.Green;
			panel.Controls.Add(subControl1);
			panel.Controls.Add(subControl2);
			panelStyle = panel.FontStyleString();
			return panel;
		});

		Assert.That(panelStyle, Does.Contain("color:#000000FF;"));
		Assert.That(subControl1.FontStyleString(), Is.EqualTo(string.Empty));
		Assert.That(subControl2.FontStyleString(), Does.Contain("color:#008000FF;"));
	}

	[Test]
	public async Task ControlLookString_NoFontSetWithNullParent_StyleStringContainsFontProperties()
	{
		using var ctx = new WinzorTestContext();
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var control = new Control();
			var styleString = control.LookStyle();

			Assert.That(styleString, Does.Contain("font-family:Tahoma;"));
			Assert.That(styleString, Does.Contain("font-size:8pt;"));
			Assert.That(styleString, Does.Contain("line-height:13px;"));
		});
	}

	[Test]
	public async Task ControlLookString_FontSetWithNullParent_StyleStringContainsFontProperties()
	{
		using var ctx = new WinzorTestContext();
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var control = new Control() { Font = new Font("Arial", 10) };
			var styleString = control.LookStyle();

			Assert.That(styleString, Does.Contain("font-family:Arial;"));
			Assert.That(styleString, Does.Contain("font-size:10pt;"));
			Assert.That(styleString, Does.Contain("line-height:16px;"));
		});
	}

	[Test]
	public async Task ControlLookString_NoFontSetWithParent_StyleStringDoesNotContainFontProperties()
	{
		using var ctx = new WinzorTestContext();
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var parent = new Control();
			var control = new Control();
			parent.Controls.Add(control);
			var styleString = control.LookStyle();

			Assert.That(styleString, Does.Not.Contain("font-family:"));
			Assert.That(styleString, Does.Not.Contain("font-size:"));
			Assert.That(styleString, Does.Not.Contain("line-height:"));
		});
	}

	[Test]
	public async Task ControlLookString_FontDifferentFromParent_StyleStringContainsFontProperties()
	{
		using var ctx = new WinzorTestContext();
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var parent = new Control();
			var control = new Control() { Font = new Font("Arial", 10) };
			parent.Controls.Add(control);
			var styleString = control.LookStyle();

			Assert.That(styleString, Does.Contain("font-family:Arial;"));
			Assert.That(styleString, Does.Contain("font-size:10pt;"));
			Assert.That(styleString, Does.Contain("line-height:16px;"));
		});
	}

	[Test]
	public async Task ControlLookString_FontSameAsParent_StyleStringDoesNotContainFontProperties()
	{
		using var ctx = new WinzorTestContext();
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var parent = new Control() { Font = new Font("Arial", 10) };
			var control = new Control() { Font = new Font("Arial", 10) };
			parent.Controls.Add(control);
			var styleString = control.LookStyle();

			Assert.That(styleString, Does.Not.Contain("font-family:"));
			Assert.That(styleString, Does.Not.Contain("font-size:"));
			Assert.That(styleString, Does.Not.Contain("line-height:"));
		});
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Structure", "NUnit1011:The TestCaseSource argument does not specify an existing member", Justification = "wrong analyzer error after migrating CursorCases into another project")]
	[TestCaseSource(typeof(CursorCases), nameof(CursorCases.TestCursors))]
	public void CursorsStyles(Cursor cursor, string style)
	{
		var expectedStyle = cursor == null || cursor == Cursors.Default ? string.Empty : $"--current-cursor:{style};";
		Assert.That(cursor.CursorStyleVars(), Is.EqualTo(expectedStyle));
	}

	[TestCase(true, true, 0, "0")]
	[TestCase(false, false, 0, "-1")]
	[TestCase(true, true, 1, "0")]
	[TestCase(true, false, 1, "-1")]
	[TestCase(false, true, 1, "-1")]
	[TestCase(false, false, 1, "-1")]
	[TestCase(true, true, 2, "0")]
	[TestCase(true, false, 2, "-1")]
	[TestCase(false, true, 2, "-1")]
	[TestCase(false, false, 2, "-1")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1105:Do not use System.Windows.Forms.UserControl or System.Windows.Forms.KUserControl Class", Justification = "Testing base winforms functionality")]
	public async Task TestParentTabStopPreventsTabsIntoChildren(bool parentTabStop, bool childTabStop, int parentDepth, string expectedTabIndex)
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var tb = new TextBox();
			tb.Name = "targetTextBox";
			tb.TabStop = childTabStop;

			Control c = tb;
			while (parentDepth > 0)
			{
				var uc = new UserControl();
				uc.Controls.Add(tb);
				c = uc;
				parentDepth--;
			}

			c.TabStop = parentTabStop;
			return c;
		});

		var tbInput = rendered.Find("input");
		var tbInputTabIndex = tbInput.GetAttribute("tabindex");

		Assert.That(tbInputTabIndex, Is.EqualTo(expectedTabIndex));
	}

	[Test]
	public async Task TestParentTabStopOnlyAppliesToContainerControls()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var tb = new TextBox();
			tb.TabStop = true;

			var nonContainerControlParent = new Panel();
			nonContainerControlParent.Controls.Add(tb);
			nonContainerControlParent.TabStop = false;

			return nonContainerControlParent;
		});

		var tbInput = rendered.Find("input");
		var tbInputTabIndex = tbInput.GetAttribute("tabindex");

		Assert.That(tbInputTabIndex, Is.EqualTo("0"));
	}

	[Test]
	public async Task TabStopIsNotAffectedBySplitContainerTabStop()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var button1 = new Button();
			var splitContainer = new SplitContainer();
			var splitterPanel = splitContainer.Controls[0];

			splitterPanel.Controls.Add(button1);
			form.Controls.Add(splitContainer);

			button1.TabStop = true;
			splitterPanel.TabStop = true;
			splitContainer.TabStop = false;

			return form;
		});

		var button = rendered.Find("button");
		Assert.That(button.GetAttribute("tabindex"), Is.EqualTo("0"));
	}

	[Test]
	public async Task TabStopIsNotAffectedByFormTabStop()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var button1 = new Button();

			button1.TabStop = true;
			form.TabStop = false;

			form.Controls.Add(button1);
			return form;
		});

		var button = rendered.Find("button");
		Assert.That(button.GetAttribute("tabindex"), Is.EqualTo("0"));
	}

	[Test]
	public async Task FontStyleStringMicrosoftSansSerifBold()
	{
		using var ctx = new WinzorTestContext();
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var control = new Control();
			control.Font = new Font("Microsoft Sans Serif", 10, FontStyle.Bold);
			var styleString = control.LookStyle();

			Assert.That(styleString, Does.Contain("font-weight:bold;"));
		});
	}

	[Test]
	public async Task BackgroundImageStyleString_BackgroundColorNotTransparent()
	{
		using var ctx = new WinzorTestContext();
		Control control = null;
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var parent = new Control();
			control = new Control { BackgroundImage = TestImage.GetImage() };
			parent.Controls.Add(control);
		});

		Assert.That(control.BackgroundImageStyleString, Is.EqualTo($"background-image: url('{TestImage.GetImage().ToBase64DataUrl()}'); background-repeat: no-repeat;"));
	}

	[Test]
	public async Task BackgroundImageStyleString_BackgroundColorTransparent()
	{
		using var ctx = new WinzorTestContext();
		Control control = null;
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var parent = new Control { BackgroundImage = TestImage.GetImage() };
			control = new Control
			{
				Left = 10,
				Top = 10,
				BackColor = Color.Transparent
			};
			parent.Controls.Add(control);
		});

		Assert.That(control.BackgroundImageStyleString, Is.EqualTo("background-image: inherit; background-repeat: no-repeat; background-position: -10px -10px;"));
	}

	[Test]
	public async Task ControlBackgroundImageIsNotOverridenByParent()
	{
		using var ctx = new WinzorTestContext();
		Control control = null;
		string imageString = "iVBORw0KGgoAAAANSUhEUgAAAAgAAAAIAQMAAAD+wSzIAAAABlBMVEX///+/v7+jQ3Y5AAAADklEQVQI12P4AIX8EAgALgAD/aNpbtEAAAAASUVORK5CYII=";

		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var parent = new Control { BackgroundImage = TestImage.GetImage() };
			control = new Control
			{
				Left = 10,
				Top = 10,
				BackColor = Color.Transparent,
				BackgroundImage = imageString.ToImage()
			};
			parent.Controls.Add(control);
		});

		Assert.That(control.BackgroundImageStyleString, Is.EqualTo($"background-image: url('{imageString.ToImage().ToBase64DataUrl()}'); background-repeat: no-repeat;"));
	}
}
