using System.Drawing;
using System.Threading.Tasks;
using Bunit;
using Microsoft.Playwright;
using NUnit.Framework;
using WinzorFramework.Extensions;
using WinzorTestFramework;
using WTG.PlaywrightTesting;

namespace System.Windows.Forms;

using static PlaywrightTestContext;

class LinkLabelTest
{
	[Test]
	public async Task LinkLabelText()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() => new LinkLabel { Text = "TestText" });
		Assert.That(rendered.Find(".linklabel a").InnerHtml, Is.EqualTo("TestText"));
	}

	[Test]
	public async Task LinkLabelStyle()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() => new LinkLabel { Text = "TestText" });
		Assert.That(rendered.Find(".linklabel").GetAttribute("style"), Does.Contain("position:absolute;width:100px;height:23px;top:0px;left:0px;"));
	}

	[Test, WithPlaywrightPage]
	public async Task LinkLabelSizeShouldBeAutoWhenDockFill()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			var panel = new Panel
			{
				Width = 200,
				Height = 300,
			};
			form.Controls.Add(panel);

			var linkLabel1 = new LinkLabel
			{
				Text = "Short label",
				AutoSize = true,
				Dock = DockStyle.Fill,
			};
			var linkLabel2 = new LinkLabel
			{
				Text = "This is a very long label, should be wrapped into two lines",
				AutoSize = true,
				Dock = DockStyle.Fill,
			};
			panel.Controls.Add(linkLabel1);
			panel.Controls.Add(linkLabel2);

			return form;
		});

		var linkLabel1 = await page.WaitForSelectorAsync(".linklabel");
		var linkLabel2 = await page.WaitForSelectorAsync(".linklabel:nth-of-type(2)");

		Assert.Multiple(() =>
		{
			Assert.That(async () => await GetCssValue(linkLabel1, "width"), Is.EqualTo("200px"));
			Assert.That(async () => await GetCssValue(linkLabel1, "height"), Is.EqualTo("19px"));       // 13(line-height) + 6(padding-top + padding-bottom)
			Assert.That(async () => await GetCssValue(linkLabel1, "position"), Is.EqualTo("static"));
			Assert.That(async () => await GetCssValue(linkLabel1, "padding"), Is.EqualTo("3px"));

			Assert.That(async () => await GetCssValue(linkLabel2, "width"), Is.EqualTo("200px"));
			Assert.That(async () => await GetCssValue(linkLabel2, "height"), Is.EqualTo("32px"));       // 13(line-height) * 2 + 6(padding-top + padding-bottom)
			Assert.That(async () => await GetCssValue(linkLabel2, "position"), Is.EqualTo("static"));
			Assert.That(async () => await GetCssValue(linkLabel2, "padding"), Is.EqualTo("3px"));
		});
	}

	[Test, WithPlaywrightPage]
	public async Task OverflowShouldBeHidden()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() => new LinkLabel
		{
			Text = "TestText"
		});
		var linkLabelItem = await page.WaitForSelectorAsync(".linklabel");
		var overflowStyle = (await linkLabelItem.EvaluateAsync($"e => window.getComputedStyle(e).getPropertyValue('overflow')")).Value.ToString();
		Assert.That(overflowStyle, Is.EqualTo("hidden"));
	}

	[Test]
	public async Task ClickInvoked()
	{
		await ControlAssert.ImplementsEventAsync<LinkLabel, EventHandler>(nameof(LinkLabel.Click), a => new EventHandler((o, e) => a()), ".linklabel", e => e.Click());
	}

	[Test]
	public async Task MouseEnterInvoked()
	{
		await ControlAssert.ImplementsEventAsync<LinkLabel, EventHandler>(nameof(LinkLabel.MouseEnter), a => new EventHandler((o, e) => a()), ".linklabel", e => e.MouseEnter());
	}

	[Test]
	public async Task MouseLeaveInvoked()
	{
		await ControlAssert.ImplementsEventAsync<LinkLabel, EventHandler>(nameof(LinkLabel.MouseLeave), a => new EventHandler((o, e) => a()), ".linklabel", e => e.MouseLeave());
	}

	[Test]
	public async Task ClickInvokesLinkClicked()
	{
		await ControlAssert.ImplementsEventAsync<LinkLabel, LinkLabelLinkClickedEventHandler>(nameof(LinkLabel.LinkClicked), a => new LinkLabelLinkClickedEventHandler((o, e) => a()), ".linklabel a", e => e.Click());
	}

	[Test]
	public async Task LinkLabelColorReturnsDefaultValue()
	{
		LinkLabel linkLabel = null;

		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			linkLabel = new LinkLabel()
			{
				Text = "Test",
			};

			form.Controls.Add(linkLabel);
			return form;
		});

		Assert.That(linkLabel.LinkColor, Is.EqualTo(Color.Blue));
		Assert.That(linkLabel.ActiveLinkColor, Is.EqualTo(Color.Red));
		Assert.That(linkLabel.DisabledLinkColor, Is.EqualTo(Color.FromArgb(106, 107, 102)));
		Assert.That(linkLabel.VisitedLinkColor, Is.EqualTo(Color.Purple));
		Assert.That(rendered.Find(".linklabel").GetAttribute("style"), Does.Contain("--linkcolor:#0000FFFF;--disabled-linkcolor:#6A6B66FF;--visited-linkcolor:#800080FF;--active-linkcolor:#FF0000FF"));
	}

	[Test]
	public async Task LinkLabelColorReturnsCustomValue()
	{
		LinkLabel linkLabel = null;

		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			linkLabel = new LinkLabel()
			{
				Text = "Test",
			};

			linkLabel.LinkColor = Color.Black;
			linkLabel.ActiveLinkColor = Color.Yellow;
			linkLabel.DisabledLinkColor = Color.Green;
			linkLabel.VisitedLinkColor = Color.White;

			form.Controls.Add(linkLabel);
			return form;
		});

		Assert.That(linkLabel.LinkColor, Is.EqualTo(Color.Black));
		Assert.That(linkLabel.ActiveLinkColor, Is.EqualTo(Color.Yellow));
		Assert.That(linkLabel.DisabledLinkColor, Is.EqualTo(Color.Green));
		Assert.That(linkLabel.VisitedLinkColor, Is.EqualTo(Color.White));
		Assert.That(rendered.Find(".linklabel").GetAttribute("style"), Does.Contain("--linkcolor:#000000FF;--disabled-linkcolor:#008000FF;--visited-linkcolor:#FFFFFFFF;--active-linkcolor:#FFFF00FF"));
	}

	[Test]
	public async Task LinkLabelDisabledSublinksReturnsCorrectDisabledSubLinks()
	{
		LinkLabel linkLabel = null;
		using var ctx = new WinzorTestContext();

		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			linkLabel = new LinkLabel()
			{
				Text = "Test",
			};

			linkLabel.Text = "Click here, here or here.";
			linkLabel.LinkArea = new LinkArea(6, 5);
			linkLabel.Links[0].LinkData = "1";
			linkLabel.Links.Add(20, 5, "3");
			linkLabel.Links.Add(12, 4, "2");

			linkLabel.Links[0].Enabled = false;
			linkLabel.Links[2].Enabled = false;

			form.Controls.Add(linkLabel);
			return form;
		});

		var renderedLinks = rendered.FindAll(".linklabel a");

		Assert.That(renderedLinks[0].GetAttribute("class"), Does.Contain("disabled"));
		Assert.That(renderedLinks[2].GetAttribute("class"), Does.Contain("disabled"));
	}

	[Test]
	public async Task LinkLabelDisabledPropertyReturnsCorrectDisabledSubLinks()
	{
		LinkLabel linkLabel = null;
		using var ctx = new WinzorTestContext();

		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			linkLabel = new LinkLabel()
			{
				Text = "Test",
			};

			linkLabel.Text = "Click here, here or here.";
			linkLabel.LinkArea = new LinkArea(6, 5);
			linkLabel.Links[0].LinkData = "1";
			linkLabel.Links.Add(20, 5, "3");
			linkLabel.Links.Add(12, 4, "2");

			linkLabel.Enabled = false;

			form.Controls.Add(linkLabel);
			return form;
		});

		var renderedLinks = rendered.FindAll(".linklabel a");

		Assert.That(renderedLinks[0].GetAttribute("class"), Does.Contain("disabled"));
		Assert.That(renderedLinks[1].GetAttribute("class"), Does.Contain("disabled"));
		Assert.That(renderedLinks[2].GetAttribute("class"), Does.Contain("disabled"));
	}

	[Test]
	public async Task RenderAndClickLinkLabel()
	{
		var invokedLinkClicked = false;

		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var linkLabel = new LinkLabel()
			{
				Text = "Test",
			};
			linkLabel.LinkClicked += new LinkLabelLinkClickedEventHandler((object sender, LinkLabelLinkClickedEventArgs e) => { invokedLinkClicked = true; });

			form.Controls.Add(linkLabel);
			return form;
		});

		await rendered.Find(".linklabel a").ClickAsync(new WebMouseEventArgs());
		Assert.That(invokedLinkClicked);
	}

	[Test]
	public async Task MultiLink_ThrowExceptionIfLinksOverlap()
	{
		using var ctx = new WinzorTestContext();
		await ctx.RenderControlOnFormAsync(() =>
		{
			var linkLabel = new LinkLabel();
			linkLabel.Text = "Test";
			linkLabel.Links.Add(1, 3, "");
			Assert.Throws<InvalidOperationException>(() => linkLabel.Links.Add(2, 2, ""));
			return linkLabel;
		});
	}

	[Test]
	public async Task MultiLink_RenderTest()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var linkLabel = new LinkLabel();
			linkLabel.Text = "Click here, here or here.";
			linkLabel.LinkArea = new LinkArea(6, 5);
			linkLabel.Links[0].LinkData = "1";
			linkLabel.Links.Add(20, 5, "3");
			linkLabel.Links.Add(12, 4, "2");
			return linkLabel;
		});

		Assert.That(rendered, Is.Not.Null);
		Assert.That(rendered.Find(".linklabel"), Is.Not.Null);

		var renderedLinks = rendered.FindAll(".linklabel a");
		Assert.That(renderedLinks.Count, Is.EqualTo(3));
		Assert.That(renderedLinks[0].InnerHtml, Is.EqualTo("here,"));
		Assert.That(renderedLinks[1].InnerHtml, Is.EqualTo("here"));
		Assert.That(renderedLinks[2].InnerHtml, Is.EqualTo("here."));
		Assert.That(renderedLinks[0].OuterHtml, Does.Contain("blazor:onclick"));
		Assert.That(renderedLinks[1].OuterHtml, Does.Contain("blazor:onclick"));
		Assert.That(renderedLinks[2].OuterHtml, Does.Contain("blazor:onclick"));
	}

	[Test]
	public async Task MultiLink_AddLinkUsingLinkArea()
	{
		using var ctx = new WinzorTestContext();
		LinkLabel linkLabel = null;
		await ctx.RenderControlOnFormAsync(() =>
		{
			linkLabel = new LinkLabel()
			{
				Text = "Click here.",
				LinkArea = new LinkArea(6, 4),
			};
			return linkLabel;
		});

		Assert.That(linkLabel, Is.Not.Null);
		Assert.That(linkLabel.Links.Count, Is.EqualTo(1));
		Assert.That(linkLabel.Links[0].Start, Is.EqualTo(6));
		Assert.That(linkLabel.Links[0].Length, Is.EqualTo(4));
	}

	[Test]
	public async Task MultiLink_AddLinkUsingAdd()
	{
		using var ctx = new WinzorTestContext();
		LinkLabel linkLabel = null;
		await ctx.RenderControlOnFormAsync(() =>
		{
			linkLabel = new LinkLabel()
			{
				Text = "Click here.",
			};
			linkLabel.Links.Add(6, 4, "ExampleData");
			return linkLabel;
		});

		Assert.That(linkLabel, Is.Not.Null);
		Assert.That(linkLabel.Links.Count, Is.EqualTo(1));
		Assert.That(linkLabel.Links[0].Start, Is.EqualTo(6));
		Assert.That(linkLabel.Links[0].Length, Is.EqualTo(4));
		Assert.That(linkLabel.Links[0].LinkData, Is.EqualTo("ExampleData"));
	}

	[Test]
	public async Task MultiLink_DefaultLink()
	{
		using var ctx = new WinzorTestContext();
		LinkLabel linkLabel = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			linkLabel = new LinkLabel()
			{
				Text = "Click here.",
			};
			return linkLabel;
		});

		Assert.That(linkLabel, Is.Not.Null);
		Assert.That(linkLabel.Links[0].Start, Is.EqualTo(0));
		Assert.That(linkLabel.Links[0].Length, Is.EqualTo(-1));
		Assert.That(linkLabel.Links[0].LinkData, Is.EqualTo(null));

		Assert.That(rendered.FindAll(".linklabel a").Count, Is.EqualTo(1));
		Assert.That(rendered.Find(".linklabel a").InnerHtml, Does.Contain("Click here."));
	}

	[Test]
	public async Task MultiLink_ClickLink()
	{
		var invokedLinkClicked = false;
		object invokedLinkData = null;

		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var linkLabel = new LinkLabel()
			{
				Text = "Click here, here or here.",
			};

			linkLabel.Links.Add(6, 4, "1");
			linkLabel.Links.Add(12, 4, "2");
			linkLabel.Links.Add(20, 4, "3");

			linkLabel.LinkClicked += new LinkLabelLinkClickedEventHandler((object sender, LinkLabelLinkClickedEventArgs e) =>
			{
				invokedLinkClicked = true;
				invokedLinkData = e.Link.LinkData;
			});

			form.Controls.Add(linkLabel);
			return form;
		});

		await rendered.Find(".linklabel a").ClickAsync(new WebMouseEventArgs());

		Assert.That(invokedLinkClicked);
		Assert.That(invokedLinkData, Is.EqualTo("1"));
	}

	[Explicit]
	[Test, WithPlaywrightPage(Headless = false)]
	public async Task MultiLinkDemo()
	{
		await using var ctx = new InMemoryTestServerContext();

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			var defaultText = "Click here, here or here.";
			var linkLabel = new LinkLabel()
			{
				Width = 500,
				Height = 200,
				Text = defaultText
			};

			linkLabel.LinkArea = new LinkArea(6, 4);
			linkLabel.Links[0].LinkData = "1";
			linkLabel.Links.Add(12, 4, "2");
			linkLabel.Links.Add(20, 4, "3");
			linkLabel.LinkClicked += new LinkLabelLinkClickedEventHandler((object sender, LinkLabelLinkClickedEventArgs e) =>
			{
				linkLabel.Text = $"{defaultText}{Environment.NewLine}User selected link {e.Link.LinkData}";
			});

			form.Controls.Add(linkLabel);

			return form;
		});

		var pageClosed = new TaskCompletionSource<bool>();
		page.Close += (sender, args) => pageClosed.SetResult(true);
		await pageClosed.Task.WithTimeout(TimeSpan.FromMinutes(5));
	}

	[Test]
	[TestCase(LinkBehavior.AlwaysUnderline, "linklabel--alwaysunderline")]
	[TestCase(LinkBehavior.HoverUnderline, "linklabel--hoverunderline")]
	[TestCase(LinkBehavior.NeverUnderline, "linklabel--neverunderline")]
	public async Task LinkLabelLinkBehavior(LinkBehavior linkBehavior, string expectedClassForNoHovering)
	{
		LinkLabel linkLabel = null;

		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			linkLabel = new LinkLabel()
			{
				Text = "Test",
				LinkBehavior = linkBehavior,
			};

			form.Controls.Add(linkLabel);
			return form;
		});

		Assert.That(rendered.Find(".linklabel").GetAttribute("class"), Does.Contain(expectedClassForNoHovering));
	}

	[Test, WithPlaywrightPage]
	[TestCase(LinkBehavior.NeverUnderline, "none", "none", "none")]
	[TestCase(LinkBehavior.HoverUnderline, "none", "underline", "none")]
	[TestCase(LinkBehavior.AlwaysUnderline, "underline", "underline", "underline")]
	public async Task LinkLabelLinkBehaviorStyleAttributes(LinkBehavior linkBehavior, string expectedClassAttributesBeforeHovering, string expectedClassAttributesWhileHovering, string expectedClassAttributesAfterHovering)
	{
		await using var ctx = new InMemoryTestServerContext();

		LinkLabel linkLabel = null;

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			linkLabel = new LinkLabel()
			{
				Text = "Test",
				LinkBehavior = linkBehavior,
			};

			form.Controls.Add(linkLabel);

			return form;
		});

		var selectedLabel = await page.WaitForSelectorAsync(".linklabel a");
		Assert.That(async () => await GetCssValue(selectedLabel, "text-decoration"), Does.Contain(expectedClassAttributesBeforeHovering));

		await selectedLabel.HoverAsync();

		Assert.That(async () => await GetCssValue(await page.WaitForSelectorAsync(".linklabel a"), "text-decoration"), Does.Contain(expectedClassAttributesWhileHovering));

		await page.Mouse.MoveAsync(0, 100);

		Assert.That(async () => await GetCssValue(await page.WaitForSelectorAsync(".linklabel a"), "text-decoration"), Does.Contain(expectedClassAttributesAfterHovering));
	}

	async Task<string> GetCssValue(IElementHandle element, string cssProperty) => (await element.EvaluateAsync($"e => window.getComputedStyle(e).getPropertyValue('{cssProperty}')")).Value.ToString();

	[Test]
	public async Task LinkLabelShouldWrapWhenWidthIsBiggerThanItsParentWidth()
	{
		using var ctx = new WinzorTestContext();
		LinkLabel linkLabel = null;
		int preferredWidth = 334;
		int preferredHeight = 39;

		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			FlowLayoutPanel parent = new FlowLayoutPanel
			{
				Dock = DockStyle.Fill,
				Size = new Size(preferredWidth, preferredHeight),
			};

			linkLabel = new LinkLabel
			{
				Text = "This a really long Link Label to check if a text have length which longer than parent It going to be broken into new row, This text have length of 743px",
				Dock = DockStyle.Fill,
				AutoSize = true,
			};
			parent.Controls.Add(linkLabel);
		});

		Assert.That(linkLabel.Width, Is.LessThanOrEqualTo(preferredWidth));
		Assert.That(linkLabel.Height, Is.LessThanOrEqualTo(preferredHeight));
	}

	[Test]
	public async Task CheckBoxImage()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			return new LinkLabel()
			{
				Text = "Test label",
				Image = TestImage.GetImage(),
			};
		});

		var checkboxImage = rendered.Find(".linklabel .linklabel__image img");
		Assert.That(checkboxImage.GetAttribute("src"), Does.Contain($"{TestImage.GetImage().ToBase64()}"));

		var checkboxDiv = rendered.Find(".linklabel .linklabel__image");
		Assert.That(checkboxDiv.GetAttribute("class"), Does.Not.Contain($"disabled"));
	}

	[Test]
	public async Task LinkLabelImage_Disable()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
			new LinkLabel
			{
				Text = "TestText",
				Image = TestImage.GetImage(),
				Enabled = false
			}
		);
		var checkboxImage = rendered.Find(".linklabel .linklabel__image--disabled");
		Assert.That(checkboxImage.GetAttribute("class"), Does.Contain($"disabled"));
	}

	[Test]
	public async Task HtmlInTextIsEscaped()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() => new LinkLabel() { Text = "<img src=\"x\"/>" });
		Assert.That(rendered.FindAll("img"), Is.Empty);
	}

	[Test]
	public async Task SingleLink_WithLinkAreaOverride()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var linkLabel = new LinkLabel()
			{
				Text = "Single Link With Link Area!",
				LinkArea = new LinkArea(28, 35),
			};

			form.Controls.Add(linkLabel);
			return form;
		});

		Assert.That(rendered, Is.Not.Null);
		Assert.That(rendered.Find(".linklabel"), Is.Not.Null);

		var renderedLinks = rendered.FindAll(".linklabel a");
		Assert.That(renderedLinks.Count, Is.EqualTo(1));
		Assert.That(renderedLinks[0].InnerHtml, Is.EqualTo("Single Link With Link Area!"));
	}

	[Test]
	public async Task SingleLink_WithLinkAreaOverride_LinkLabelDisabled()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var linkLabel = new LinkLabel()
			{
				Text = "Single Link With Link Area!",
				LinkArea = new LinkArea(28, 35),
				Enabled = false
			};

			form.Controls.Add(linkLabel);
			return form;
		});

		Assert.That(rendered, Is.Not.Null);
		Assert.That(rendered.Find(".linklabel"), Is.Not.Null);

		var renderedLinks = rendered.FindAll(".linklabel a");
		Assert.That(renderedLinks.Count, Is.EqualTo(1));
		Assert.That(renderedLinks[0].InnerHtml, Is.EqualTo("Single Link With Link Area!"));
	}

	[Test, WithPlaywrightPage]
	[TestCase("LinkLabel TopLeft", ContentAlignment.TopLeft, "left", "start")]
	[TestCase("LinkLabel TopCenter", ContentAlignment.TopCenter, "center", "start")]
	[TestCase("LinkLabel TopRight", ContentAlignment.TopRight, "right", "start")]
	[TestCase("LinkLabel MiddleLeft", ContentAlignment.MiddleLeft, "left", "center")]
	[TestCase("LinkLabel MiddleCenter", ContentAlignment.MiddleCenter, "center", "center")]
	[TestCase("LinkLabel MiddleRight", ContentAlignment.MiddleRight, "right", "center")]
	[TestCase("LinkLabel BottomLeft", ContentAlignment.BottomLeft, "left", "end")]
	[TestCase("LinkLabel BottomCenter", ContentAlignment.BottomCenter, "center", "end")]
	[TestCase("LinkLabel BottomRight", ContentAlignment.BottomRight, "right", "end")]
	public async Task ShouldCheckCorrectLinkAlignment_WhenContentNotOverflowed(string text, ContentAlignment alignment, string justifyContentExpect, string alignItemsExpect)
	{
		int width = 100, height = 50;
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() => new LinkLabel
		{
			Width = width,
			Height = height,
			Text = text,
			TextAlign = alignment
		});

		var linkLabelItem = await page.QuerySelectorAsync(".linklabel");
		var justifyContent = (await linkLabelItem.EvaluateAsync($"e => window.getComputedStyle(e).getPropertyValue('justify-content')")).Value.ToString();
		var alignItems = (await linkLabelItem.EvaluateAsync($"e => window.getComputedStyle(e).getPropertyValue('align-items')")).Value.ToString();
		Assert.Multiple(() =>
		{
			Assert.That(justifyContent, Is.EqualTo(justifyContentExpect));
			Assert.That(alignItems, Is.EqualTo(alignItemsExpect));
		});
	}

	[Test, WithPlaywrightPage]
	[TestCase(ContentAlignment.TopLeft, "left", Description = "TopLeft")]
	[TestCase(ContentAlignment.TopCenter, "center", Description = "TopCenter")]
	[TestCase(ContentAlignment.TopRight, "right", Description = "TopRight")]
	[TestCase(ContentAlignment.MiddleLeft, "left", Description = "MiddleLeft")]
	[TestCase(ContentAlignment.MiddleCenter, "center", Description = "MiddleCenter")]
	[TestCase(ContentAlignment.MiddleRight, "right", Description = "MiddleRight")]
	[TestCase(ContentAlignment.BottomLeft, "left", Description = "BottomLeft")]
	[TestCase(ContentAlignment.BottomCenter, "center", Description = "BottomCenter")]
	[TestCase(ContentAlignment.BottomRight, "right", Description = "BottomRight")]
	public async Task ShouldApplyCorrectLinkAlignmentAndLineClamp_WhenContentOverflowed(ContentAlignment alignment, string justifyContentExpect)
	{
		const string text = "Customer Service Ticket - CST00000003 - Please, sir, may I have a message? - Job Customer Service Ticket - CST00000003 - Please, sir, may I have a message? is complete.";
		int width = 100, height = 40;
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() => new LinkLabel
		{
			Width = width,
			Height = height,
			Text = text,
			Font = new Font(new FontFamily("Tahoma"), 8),
			TextAlign = alignment
		});

		var linkLabelItem = await page.QuerySelectorAsync(".linklabel");
		var justifyContent = (await linkLabelItem.EvaluateAsync($"e => window.getComputedStyle(e).getPropertyValue('justify-content')")).Value.ToString();
		var alignItems = (await linkLabelItem.EvaluateAsync($"e => window.getComputedStyle(e).getPropertyValue('align-items')")).Value.ToString();

		var contentItem = await page.QuerySelectorAsync("a");
		var lineClampContent = (await contentItem.EvaluateAsync($"e => window.getComputedStyle(e).getPropertyValue('-webkit-line-clamp')")).Value.ToString();

		Assert.Multiple(() =>
		{
			Assert.That(justifyContent, Is.EqualTo(justifyContentExpect));
			Assert.That(alignItems, Is.EqualTo("start"));
			Assert.That(lineClampContent, Is.EqualTo("3"));
		});
	}

	[Test, WithPlaywrightPage]
	public async Task ClickHandlerShouldBeCalledWhenClickOnLinkLabel()
	{
		var clickCount = 0;
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var linkLabel = new LinkLabel { Text = "linklabel" };
			linkLabel.Click += (sender, e) => clickCount += 1;
			return linkLabel;
		});

		var linkLabel = page.Locator(".linklabel");
		Assert.That(() => clickCount, Is.EqualTo(0).After(500, 50));

		await linkLabel.ClickAsync();
		Assert.That(() => clickCount, Is.EqualTo(1).After(500, 50));

		await linkLabel.ClickAsync();
		Assert.That(() => clickCount, Is.EqualTo(2).After(500, 50));

		await linkLabel.ClickAsync();
		Assert.That(() => clickCount, Is.EqualTo(3).After(500, 50));
	}

	[Test, WithPlaywrightPage]
	public async Task TextWrappedLinkShouldDisplayContents()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var linkLabel = new LinkLabel { Text = "This a long Link Label with a link." };
			linkLabel.Links.Add(6, 15, "ExampleData");
			return linkLabel;
		});
		var link = await page.WaitForSelectorAsync(".linklabel a");
		Assert.That(await GetCssValue(link, "display"), Is.EqualTo("contents"));
	}

	[Test, WithPlaywrightPage]
	public async Task TestLinkLabelHasCorrectTabIndex()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var linkLabel = new LinkLabel { Text = "This a long Link Label with a link." };
			linkLabel.Links.Add(6, 15, "ExampleData");

			return linkLabel;
		});

		var linkLabelElement = await page.WaitForSelectorAsync(".linklabel");
		var tabIndex = await linkLabelElement.GetAttributeAsync("tabindex");

		Assert.That(tabIndex, Is.EqualTo("0"));
	}

	[Test, WithPlaywrightPage]
	public async Task TestOnKeyDown()
	{
		await using var ctx = new InMemoryTestServerContext();
		var clicked = false;
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var linkLabel = new LinkLabel() { Text = "Click here.", };
			linkLabel.Links.Add(6, 4, "ExampleData");
			linkLabel.LinkClicked += (sender, e) =>
			{
				clicked = true;
			};
			return linkLabel;
		});

		var label = await page.WaitForSelectorAsync(".linklabel");
		Assert.That(label, Is.Not.Null);

		await label.PressAsync("Enter");
		Assert.That(() => clicked, Is.True.After(1000, 100));
	}

	[Test]
	public async Task UpdateSelectability_ShouldUpdateSelectStyle()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() => new LinkLabel());
		var linkLabel = rendered.GetControl<LinkLabel>();

		Assert.That(linkLabel.CanSelect, Is.False);
		await linkLabel.InvokeWinzorDispatcherAsync(() => linkLabel.Text = "Test");
		Assert.That(linkLabel.CanSelect, Is.True);
	}
}
