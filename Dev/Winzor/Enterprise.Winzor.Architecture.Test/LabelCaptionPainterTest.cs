using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using AngleSharp.Html.Dom;
using Bunit;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using WinzorFramework;
using MouseEventArgs = Microsoft.AspNetCore.Components.Web.MouseEventArgs;
using Res = CargoWiseOne.ResourceStrings.Res;

namespace Enterprise.Winzor.Architecture.Test;

class LabelCaptionPainterTest
{
	[Test]
	public async Task LabelAddedToParent()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new ZForm();
			form.Size = new Size(300, 300);
			var textBox = new ZTextBox();
			textBox.CaptionResourceString = Res.GetData("3b4b21a5-7649-49a1-b1ee-092904466098", "Foo");
			textBox.Left = 100;
			textBox.Top = 100;
			textBox.Width = 100;
			form.Controls.Add(textBox);
			form.CaptionRenderingEnabled = true;
			return form;
		}, OpenFormAction.BlockUntilShown);
		rendered.WaitForState(() => rendered.FindAll("div:contains('Foo')").Any());
		Assert.That(rendered.Find("div:contains('Foo')"), Is.Not.Null);
		Assert.That(rendered.RenderCount, Is.EqualTo(2));
	}

	[Test]
	public async Task LabelWithUnicodeCharacterTranslationAddedToParent()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var cache = ctx.Using(Res.UseMockData());
			cache.SetResourceGetter((key) => new ResourceStringData("3b4b21a5-7649-49a1-b1ee-092904466098", "狗"));
			var form = new ZForm();
			form.Size = new Size(300, 300);
			var textBox = new ZTextBox();
			textBox.CaptionResourceString = Res.GetData("3b4b21a5-7649-49a1-b1ee-092904466098", "Dog");
			textBox.Left = 100;
			textBox.Top = 100;
			textBox.Width = 100;
			form.Controls.Add(textBox);
			form.CaptionRenderingEnabled = true;
			return form;
		}, OpenFormAction.BlockUntilShown);
		rendered.WaitForState(() => rendered.FindAll("div:contains('狗')").Any());
		Assert.That(rendered.Find("div:contains('狗')"), Is.Not.Null);
	}

	[Test]
	public async Task LabelRemovedWhenControlVisibleChanged()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new ZForm();
			form.Size = new Size(300, 300);
			var textBox = new ZTextBox();
			textBox.CaptionResourceString = Res.GetData("3b4b21a5-7649-49a1-b1ee-092904466098", "Foo");
			textBox.Left = 100;
			textBox.Top = 100;
			textBox.Width = 100;
			form.Controls.Add(textBox);
			var button = new ZButton();
			button.Click += Button_Click;
			form.Controls.Add(button);
			form.CaptionRenderingEnabled = true;
			return form;
		}, OpenFormAction.BlockUntilShown);
		rendered.WaitForState(() => rendered.FindAll(".label:contains('Foo')").Any());
		Assert.That(rendered.Find(".label:contains('Foo')"), Is.Not.Null);
		await rendered.Find("button").ClickAsync(new MouseEventArgs());
		Assert.That(rendered.FindAll(".label:contains('Foo')"), Is.Empty);

		void Button_Click(object sender, EventArgs e)
		{
			((ZButton)sender).FindForm().FindSingle<ZTextBox>().Visible = false;
		}
	}

	[Test]
	public async Task LabelSizeFitsAvailableSpace()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new ZForm();
			form.Size = new Size(300, 300);
			var textBox = new ZTextBox();
			textBox.CaptionResourceString = Res.GetData("3b4b21a5-7649-49a1-b1ee-092904466098", "Short", "Medium Caption", "A little bit too long of a normal caption that won't fit in the given space", "Full description");
			textBox.Left = 100;
			textBox.Top = 100;
			textBox.Width = 100;
			form.Controls.Add(textBox);
			form.CaptionRenderingEnabled = true;
			return form;
		}, OpenFormAction.BlockUntilShown);
		rendered.WaitForState(() => rendered.FindAll("div:contains('Medium Caption')").Any());
		Assert.That(rendered.Find("div:contains('Medium Caption')"), Is.Not.Null);
	}

	[Test]
	public async Task LabelAddedToParentContainer()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new ZForm();
			form.Size = new Size(300, 300);
			var groupBox = new ZGroupBox();
			groupBox.Left = 100;
			groupBox.Top = 100;
			groupBox.Size = new Size(200, 200);
			form.Controls.Add(groupBox);
			var textBox = new ZTextBox();
			textBox.CaptionResourceString = Res.GetData("3b4b21a5-7649-49a1-b1ee-092904466098", "Foo");
			textBox.Left = 50;
			textBox.Top = 20;
			textBox.Width = 100;
			groupBox.Controls.Add(textBox);
			form.CaptionRenderingEnabled = true;
			return form;
		}, OpenFormAction.BlockUntilShown);
		rendered.WaitForState(() => rendered.FindAll("div:contains('Foo')").Any());
		Assert.That(rendered.Find("div:contains('Foo')"), Is.Not.Null);
		Assert.That(rendered.RenderCount, Is.EqualTo(2));
	}

	[Test]
	public async Task LabelRenderedWhenSwitchingTabPages()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new ZForm();
			form.Size = new Size(300, 300);
			var tabControl = new ZTabControl();
			tabControl.Size = new Size(300, 300);
			var tabPage1 = new ZTabPage();
			tabPage1.Text = "Tab 1";
			tabPage1.Size = new Size(300, 300);
			tabControl.TabPages.Add(tabPage1);
			var tabPage2 = new ZTabPage();
			tabPage2.Text = "Tab 2";
			tabPage2.Size = new Size(300, 300);
			tabControl.TabPages.Add(tabPage2);
			tabControl.SelectedIndex = 0;
			form.Controls.Add(tabControl);
			var textBox1 = new ZTextBox();
			textBox1.CaptionResourceString = Res.GetData("3b4b21a5-7649-49a1-b1ee-092904466098", "Foo");
			textBox1.Left = 100;
			textBox1.Top = 100;
			tabPage1.Controls.Add(textBox1);
			var textBox2 = new ZTextBox();
			textBox2.CaptionResourceString = Res.GetData("6209e9b3-9d2d-4e3a-8c7b-fafed7deec4d", "Bar");
			textBox2.Left = 100;
			textBox2.Top = 100;
			tabPage2.Controls.Add(textBox2);
			form.CaptionRenderingEnabled = true;
			return form;
		}, OpenFormAction.BlockUntilShown);
		rendered.WaitForState(() => rendered.FindAll("div:contains('Foo')").Any());
		Assert.That(rendered.FindAll("div:contains('Foo')"), Is.Not.Empty);
		Assert.That(rendered.FindAll("div:contains('Bar')"), Is.Empty);
		await rendered.Find("button:contains('Tab 2')").ClickAsync(new MouseEventArgs());
		Assert.That(rendered.FindAll("div:contains('Foo')"), Is.Empty);
		Assert.That(rendered.FindAll("div:contains('Bar')"), Is.Not.Empty);
		await rendered.Find("button:contains('Tab 1')").ClickAsync(new MouseEventArgs());
		Assert.That(rendered.FindAll("div:contains('Bar')"), Is.Empty);
		Assert.That(rendered.FindAll("div:contains('Foo')"), Is.Not.Empty);
	}

	[Test]
	public async Task TestRightAnchoredTextBoxLabelLocationWhenResizingForm()
	{
		using var ctx = new EnterpriseTestContext();
		ZForm form = null;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			form = new ZForm();
			form.Size = new Size(300, 300);
			var textBox = new ZTextBox
			{
				CaptionResourceString = Res.GetData("Kakkoii TextBox", "Kakkoii"),
				Left = 100,
				Anchor = AnchorStyles.Right,
			};

			form.Controls.Add(textBox);
			form.CaptionRenderingEnabled = true;
			return form;
		}, OpenFormAction.BlockUntilShown);

		rendered.WaitForState(() => rendered.FindAll(".label:contains('Kakkoii')").Any());
		Assert.That(rendered.Find(".label").GetAttribute("style"), Does.Contain("left:58px"));

		await form.InvokeWinzorDispatcherAsync(() => form.Width = 700);
		Assert.That(rendered.Find(".label").GetAttribute("style"), Does.Contain("left:458px"));

		await form.InvokeWinzorDispatcherAsync(() => form.Width = 400);
		Assert.That(rendered.Find(".label").GetAttribute("style"), Does.Contain("left:158px"));
	}

	[Test]
	public async Task TestCaptionedTextBoxGrowsAndShrinksRelativeToInitialLayout()
	{
		using var ctx = new EnterpriseTestContext();
		Form form = default;
		TextBox textBox = default;
		var widthBeforeExpand = 0;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			form = new ZForm();
			form.Size = new Size(300, 300);
			var uc = new ZUserControl
			{
				Left = 0,
				Size = new Size(300, 50),
				Dock = DockStyle.Fill
			};

			textBox = new ZTextBox
			{
				CaptionResourceString = Res.GetData("Kakkoii TextBox", "Kakkoii"),
				Left = 100,
				Size = new Size(100, 50),
				MaximumSize = new Size(200, 50),
				Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
				Name = "SummaryTextBox"
			};
			uc.Controls.Add(textBox);
			uc.CaptionRenderingEnabled = true;

			form.Controls.Add(uc);

			widthBeforeExpand = textBox.Size.Width;
			return form;
		});

		var widthAfterExpand = 0;
		await form.InvokeWinzorDispatcherAsync(() =>
		{
			form.Size = new Size(form.Size.Width + 200, form.Size.Height); // Increase width by 200px, should max-out
			widthAfterExpand = textBox.Size.Width;
		});

		var widthAfterContract = 0;
		await form.InvokeWinzorDispatcherAsync(() =>
		{
			form.Size = new Size(form.Size.Width - 100, form.Size.Height); // Decrease width by 100px, should still be large
			widthAfterContract = textBox.Size.Width;
		});

		Assert.That(widthBeforeExpand, Is.EqualTo(100), "TextBox should remain at initial size before form expands");
		Assert.That(widthAfterExpand, Is.EqualTo(200), "TextBox should expand to its maximum size after form expands");
		Assert.That(widthAfterContract, Is.EqualTo(200), "TextBox should remain at maximum size after form partially contracts");
	}

	[Test]
	public async Task LabelsAddedToParentsAtCorrectTopValue()
	{
		using var ctx = new EnterpriseTestContext();
		var element1Yvalue = 20;
		var element2Yvalue = 60;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new ZForm();
			form.Size = new Size(300, 300);
			var tabControl = new ZTabControl();
			tabControl.Size = new Size(300, 300);
			var tabPage = new ZTabPage();
			tabPage.Text = "Tab 1";
			tabPage.Size = new Size(300, 300);
			tabControl.TabPages.Add(tabPage);
			tabControl.SelectedIndex = 0;
			form.Controls.Add(tabControl);
			var groupBox = new ZGroupBox();
			groupBox.Left = 100;
			groupBox.Top = 100;
			groupBox.Size = new Size(200, 200);
			tabPage.Controls.Add(groupBox);
			var textBox = new ZTextBox();
			textBox.CaptionResourceString = Res.GetData("3b4b21a5-7649-49a1-b1ee-092904466098", "Foo");
			textBox.Left = 50;
			textBox.Top = element1Yvalue;
			textBox.Width = 100;
			groupBox.Controls.Add(textBox);
			var textBox2 = new ZTextBox();
			textBox2.CaptionResourceString = Res.GetData("6209e9b3-9d2d-4e3a-8c7b-fafed7deec4d", "Bar");
			textBox2.Left = 50;
			textBox2.Top = element2Yvalue;
			groupBox.Controls.Add(textBox2);
			form.CaptionRenderingEnabled = true;
			return form;
		}, OpenFormAction.BlockUntilShown);

		rendered.WaitForState(() => rendered.FindAll("div:contains('Foo')").Any() &&
		rendered.FindAll("div:contains('Bar')").Any());

		var spanElements = rendered.FindAll("span").Cast<IHtmlSpanElement>();

		// NOTE: expected top value here is 3px extra, because of the calculation in
		// LabelCaptionRenderer, function MeasureCaption_ForLeft(), which adds to the top value:
		// ((float)Control.Height / 2) - (size.Height / 2)
		// which calculates to 23px and 63 px instead of 20px and 60px

		var span1ParentDiv = spanElements.ElementAt(0).ParentElement as IHtmlDivElement;
		var div1Html = span1ParentDiv.OuterHtml;
		Assert.That(div1Html, Does.Contain("top:23px;"));

		var span2ParentDiv = spanElements.ElementAt(1).ParentElement as IHtmlDivElement;
		var div2Html = span2ParentDiv.OuterHtml;
		Assert.That(div2Html, Does.Contain("top:63px;"));
	}

	[Test]
	public async Task LabelIsNotInControlsCollection()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new ZForm();
			form.Size = new Size(300, 300);
			var textBox = new ZTextBox();
			textBox.CaptionResourceString = Res.GetData("3b4b21a5-7649-49a1-b1ee-092904466098", "Foo");
			textBox.Left = 100;
			textBox.Top = 100;
			textBox.Width = 100;
			form.Controls.Add(textBox);
			form.CaptionRenderingEnabled = true;
			return form;
		}, OpenFormAction.BlockUntilShown);
		rendered.WaitForState(() => rendered.FindAll("div:contains('Foo')").Any());
		Assert.That(rendered.Find("div:contains('Foo')"), Is.Not.Null);
		Assert.That(rendered.Instance.Control.Controls.Select(c => c.GetType().Name), Is.EquivalentTo(new[] { "ZTextBox", "ZStatusBar" }));
	}

	[Test]
	public async Task LabelCaptionShouldNotBeVisibleIfHasObstruction()
	{
		using var ctx = new EnterpriseTestContext();
		ZForm form = null;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			form = new ZForm();
			form.Size = new Size(300, 300);
			var textBox1 = new ZTextBox();
			textBox1.CaptionResourceString = Res.GetData("3b4b21a5-7649-49a1-b1ee-092904466098", "Bar");
			textBox1.Left = 100;
			textBox1.Width = 50;
			var textBox2 = new ZTextBox();
			textBox2.CaptionResourceString = Res.GetData("6f742518-341a-48d5-b2c0-d50dc1027831", "Foo");
			textBox2.Left = 50;
			textBox2.Width = 50;
			form.Controls.Add(textBox1);
			form.Controls.Add(textBox2);
			form.CaptionRenderingEnabled = true;
			return form;
		}, OpenFormAction.BlockUntilShown);

		await form.InvokeWinzorDispatcherAsync(() =>
		{
			form.Text = "Update to force NotifyRenderRequired";
		});

		Assert.That(rendered.FindAll(".label:contains('Foo')"), Is.Not.Empty);
		Assert.That(rendered.FindAll(".label:contains('Bar')"), Is.Empty);
	}

	[Test]
	public async Task TabPageLabelCaptionRenderedOnPaint()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new ZForm();
			form.Size = new Size(300, 300);

			var tabControl = new ZTabControl();
			tabControl.Size = new Size(300, 300);
			var tabPage1 = new ZTabPage();
			tabPage1.Size = new Size(300, 300);
			tabControl.TabPages.Add(tabPage1);
			var tabPage2 = new ZTabPage();
			tabPage2.Size = new Size(300, 300);
			tabControl.TabPages.Add(tabPage2);
			tabControl.SelectedIndex = 0;
			form.Controls.Add(tabControl);

			var button = new ZButton();
			button.Text = "Go";
			button.Click += (s, e) =>
			{
				tabPage1.CaptionResourceString = Res.GetData("foo", "Foo");
				tabPage2.CaptionResourceString = Res.GetData("bar", "Bar");
				tabControl.Invalidate();
			};
			form.Controls.Add(button);

			form.CaptionRenderingEnabled = true;
			return form;
		});

		await rendered.Find("button:contains('Go')").ClickAsync(new MouseEventArgs());

		Assert.That(rendered.FindAll("button:contains('Foo')"), Is.Not.Empty);
		Assert.That(rendered.FindAll("button:contains('Bar')"), Is.Not.Empty);
	}

	[Test]
	public async Task LabelCaptionRenderedWhenCaptionResourceStringIsModifiedOnShown()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new ZForm();
			form.Size = new Size(500, 500);

			var tabControl = new ZTabControl { Size = new Size(300, 300) };
			var tabPage = new ZTabPage() { Size = new Size(300, 300) };
			ZTextBox textBox = null;
			tabPage.RunWhenBindingOrFirstShown((_, _) =>
			{
				textBox = new ZTextBox
				{
					CaptionResourceString = Res.GetData("D0BFD379-BC67-4707-8B88-9074D8F59666", "Foo"),
					Left = 100,
					Size = new Size(100, 50),
				};

				tabPage.Controls.Add(textBox);
			});
			tabControl.TabPages.Add(tabPage);

			form.Controls.Add(tabControl);
			form.CaptionRenderingEnabled = true;
			form.Shown += (_, _) => textBox.CaptionResourceString = Res.GetData("C71E9889-ADF6-4B08-A6C7-1FD30E8C4DAD", "Bar");
			return form;
		});

		Assert.That(rendered.FindAll(".tabcontrol__page:contains('Foo')"), Is.Empty);
		Assert.That(rendered.FindAll(".tabcontrol__page:contains('Bar')"), Is.Not.Empty);
	}
}
