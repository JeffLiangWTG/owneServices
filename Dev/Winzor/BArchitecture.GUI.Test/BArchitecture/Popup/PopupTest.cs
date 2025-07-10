using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Microsoft.Playwright;
using Moq;
using NUnit.Framework;
using WinzorFramework.JSInterop;
using WinzorTestFramework;
using WTG.PlaywrightTesting;

namespace WinzorFramework;

using static PlaywrightTestContext;

sealed class PopupTest
{
	[Test]
	public void PopupRendersChildContent()
	{
		using var ctx = new Bunit.TestContext();
		ctx.Services.AddScoped(_ => Mock.Of<IPopupJSInterop>());
		ctx.JSInterop.Mode = JSRuntimeMode.Loose;
		var rendered = ctx.RenderComponent<Popup>(parameters => parameters
			.AddChildContent("<p>This should be rendered in a popup</p>")
		);

		var popup = rendered.Find(".popup");
		Assert.That(popup.InnerHtml, Is.EqualTo("<p>This should be rendered in a popup</p>"));
	}

	[Test, WithPlaywrightPage]
	public async Task PopupIsAnchoredAboveControl()
	{
		await using var ctx = new InMemoryTestServerContext();
		Form form = null;
		TextBox textBox = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form { Height = 100, Width = 300 };
			textBox = new TextBox { Top = 75, Left = 30 };
			form.Controls.Add(textBox);
			return form;
		});

		var textBoxRect = await (await page.WaitForSelectorAsync(".textbox")).BoundingBoxAsync();

		await form.InvokeWinzorDispatcherAsync(() => form.Controls.Add(new PopupControl(textBox.ElementReference) { Width = 50, Height = 52 }));

		var popupRect = await (await page.WaitForSelectorAsync(".popup")).BoundingBoxAsync();
		var popupRect2 = await (await page.WaitForSelectorAsync(".popup")).InnerHTMLAsync();

		Assert.That(popupRect.X, Is.LessThanOrEqualTo(textBoxRect.X + textBoxRect.Width));
		Assert.That(popupRect.X + popupRect.Width, Is.GreaterThanOrEqualTo(textBoxRect.X));
		// 13 seems needed for something?
		Assert.That(popupRect.Y, Is.EqualTo(textBoxRect.Y - popupRect.Height + 13));
	}

	[Test, WithPlaywrightPage]
	public async Task PopupIsAnchoredBelowControl()
	{
		await using var ctx = new InMemoryTestServerContext();
		Form form = null;
		TextBox textBox = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form();
			textBox = new TextBox { Top = 20, Left = 30 };
			form.Controls.Add(textBox);
			return form;
		});

		var textBoxRect = await (await page.WaitForSelectorAsync(".textbox")).BoundingBoxAsync();

		await form.InvokeWinzorDispatcherAsync(() => form.Controls.Add(new PopupControl(textBox.ElementReference) { Width = 50, Height = 60 }));

		var popupRect = await (await page.WaitForSelectorAsync(".popup")).BoundingBoxAsync();

		Assert.That(popupRect.X, Is.LessThanOrEqualTo(textBoxRect.X + textBoxRect.Width));
		Assert.That(popupRect.X + popupRect.Width, Is.GreaterThanOrEqualTo(textBoxRect.X));
		Assert.That(popupRect.Y, Is.EqualTo(textBoxRect.Y + textBoxRect.Height));
	}

	[Test, WithPlaywrightPage]
	public async Task PopupStaysAnchoredBelowControlWhenParentIsScrolled()
	{
		await using var ctx = new InMemoryTestServerContext();
		Form form = null;
		TextBox textBox = null;
		Panel panel = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form() { Width = 500, Height = 500 };
			panel = new Panel() { AutoScroll = true, Width = 200, Height = 50 };
			textBox = new TextBox { Top = 70, Left = 30 };
			form.Controls.Add(panel);
			panel.Controls.Add(textBox);
			return form;
		});
		var textbox = page.Locator(".textbox");
		await textbox.WaitForAsync();
		var textBoxRect = await textbox.BoundingBoxAsync();

		await form.InvokeWinzorDispatcherAsync(() => form.Controls.Add(new PopupControl(textBox.ElementReference) { Width = 50, Height = 60 }));

		var popup = page.Locator(".popup");
		await popup.WaitForAsync();
		var popupRect = await popup.BoundingBoxAsync();

		Assert.That(popupRect.X, Is.LessThanOrEqualTo(textBoxRect.X + textBoxRect.Width));
		Assert.That(popupRect.X + popupRect.Width, Is.GreaterThanOrEqualTo(textBoxRect.X));
		Assert.That(popupRect.Y, Is.EqualTo(textBoxRect.Y + textBoxRect.Height));

		var panelLocator = page.Locator(".panel");
		await panelLocator.WaitForAsync();
		await panelLocator.EvaluateAsync("e => e.scrollTo(0, 0);");

		await textbox.WaitForAsync();
		textBoxRect = await textbox.BoundingBoxAsync();

		await popup.WaitForAsync();
		popupRect = await popup.BoundingBoxAsync();

		Assert.That(popupRect.X, Is.LessThanOrEqualTo(textBoxRect.X + textBoxRect.Width));
		Assert.That(popupRect.X + popupRect.Width, Is.GreaterThanOrEqualTo(textBoxRect.X));
		Assert.That(popupRect.Y, Is.EqualTo(textBoxRect.Y + textBoxRect.Height));
	}

	[Test]
	public void ShouldNotThrowErrorOnDisposeWhenConnectionIsNotActive()
	{
		using var ctx = new Bunit.TestContext();
		ctx.Services.AddScoped(_ => Mock.Of<IPopupJSInterop>());
		var path = "/_content/WinzorFramework/js/module/popup.js";
		var module = ctx.JSInterop.SetupModule(path);
		var popupReference = module.SetupModule("attachPopup", _ => true);
		popupReference.Setup<object>(_ => true).SetException(new JSDisconnectedException("JS connection lost"));

		var rendered = ctx.RenderComponent<Popup>();
		Assert.DoesNotThrow(ctx.DisposeComponents);
	}

	[Test, WithPlaywrightPage]
	public async Task PopupWithoutAnchorElementReferenceShouldNotAttach()
	{
		await using var ctx = new InMemoryTestServerContext();
		Form form = null;
		TextBox textBox = null;

		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form { Height = 500, Width = 500 };
			textBox = new TextBox { Top = 75, Left = 30 };
			form.Controls.Add(textBox);
			return form;
		}, pageCloseOnDispose: false);

		page.PageError += (_, error) =>
		{
			Assert.Fail(error);
		};

		await page.WaitForSelectorAsync(".textbox");

		await form.InvokeWinzorDispatcherAsync(() => form.Controls.Add(new PopupControl(default(ElementReference)) { Width = 50, Height = 60 }));
		await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
		await page.WaitForSelectorAsync(".popup", new PageWaitForSelectorOptions { State = WaitForSelectorState.Hidden });

		await Task.Delay(1000);
		await page.CloseAsync(); // this line can't be removed because the test needs to test the situation after closing the page.
		Assert.That(() => PageErrors.Count , Is.EqualTo(0).After(1000, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task PopupHiddenShouldTriggerCleanUp()
	{
		await using var ctx = new InMemoryTestServerContext();
		Form form = null;
		PopupControl popupControl = null;
		TextBox textBox = null;
		var leftPosition = 30;
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form() { Width = 500, Height = 500 };
			textBox = new TextBox { Top = 70, Left = leftPosition };
			form.Controls.Add(textBox);
			return form;
		});

		await page.WaitForSelectorAsync(".textbox");
		await form.InvokeWinzorDispatcherAsync(() =>
		{
			popupControl = new PopupControl(textBox.ElementReference) { Width = 50, Height = 60 };
			form.Controls.Add(popupControl);
		});
		var popup = await page.WaitForSelectorAsync(".popup");
		await form.InvokeRenderDispatcherAsync(popupControl.popup.HideAsync);
		await form.InvokeWinzorDispatcherAsync(() => textBox.Left = 100);
		await Task.Delay(1);
		Assert.That(async () => await popup.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('left')"), Is.EqualTo($"{leftPosition}px"));
	}

	[Test, WithPlaywrightPage]
	public async Task PopupHideReportsExceptionSilently()
	{
		Form form = null;
		PopupControl popupControl = null;
		TextBox textBox = null;
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form() { Width = 500, Height = 500 };
			textBox = new TextBox();
			form.Controls.Add(textBox);
			return form;
		});
		await page.Locator(".textbox").WaitForAsync();

		await form.InvokeWinzorDispatcherAsync(() =>
		{
			popupControl = new PopupControl(textBox.ElementReference) { Width = 50, Height = 60 };
			form.Controls.Add(popupControl);
		});
		await page.Locator(".popup").WaitForAsync();

		var mock = new Mock<IJSObjectReference>();
		mock.Setup(j => j.InvokeAsync<Microsoft.JSInterop.Infrastructure.IJSVoidResult>("unregister", It.IsAny<object[]>()));
		popupControl.popup.jsObjectReference = mock.Object;
		try
		{
			await form.InvokeRenderDispatcherAsync(popupControl.popup.HideAsync);
		}
		catch (Exception e)
		{
			Assert.That(() => e, Is.InstanceOf<TaskCanceledException>());
		}
	}

	[Test, WithPlaywrightPage]
	public async Task PopupIsAlignedToWinzorControl()
	{
		await using var ctx = new InMemoryTestServerContext();
		Form form = null;
		CheckBox checkbox = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form();
			checkbox = new CheckBox { Text = "This is a checkbox", Width = 200, Height = 30, CheckAlign = ContentAlignment.MiddleRight };
			form.Controls.Add(checkbox);
			return form;
		});
		var clientRect = await (await page.WaitForSelectorAsync(".checkbox")).BoundingBoxAsync();

		await form.InvokeWinzorDispatcherAsync(() => form.Controls.Add(new PopupControl(checkbox.ElementReference) { Width = 10, Height = 10 }));
		var popupElement = await page.WaitForSelectorAsync(".popup");

		// popup should be aligned to the control, not to the checkbox which has the element reference
		Assert.That((await popupElement.BoundingBoxAsync()).X, Is.EqualTo(clientRect.X));
	}

	[Test]
	public async Task PreloadPopupJSInterop()
	{
		using var ctx = new WinzorTestContext();
		var services = ctx.MockCargoWiseClientServices;
		var interop = new Mock<IPopupJSInterop>();
		interop.Setup(i => i.PreloadInterop());
		ctx.Services.AddScoped(_ => interop.Object);

		var rendered = await ctx.RenderControlOnFormAsync(() => new PopupControl(new ElementReference()) { Width = 50, Height = 60 });

		interop.Verify(e => e.PreloadInterop(), Times.Once());
	}

	class PopupControl : Control
	{
		readonly ElementReference anchorElement;
		internal Popup popup;

		public PopupControl(ElementReference anchorElement)
		{
			this.anchorElement = anchorElement;
		}

		protected override void BuildRenderTree(RenderTreeBuilder builder)
		{
			base.BuildRenderTree(builder);

			builder.OpenComponent(1, typeof(Popup));
			builder.AddAttribute(2, "AnchorElement", anchorElement);
			builder.AddAttribute(3, "ChildContent", (RenderFragment)((popupContent) =>
			{
				popupContent.OpenElement(4, "p");
				popupContent.AddContent(5, "this is the popup content");
				popupContent.CloseElement();
			}));
			builder.AddComponentReferenceCapture(6, (value) =>
			{
				popup = value as Popup;
			});
			builder.CloseComponent();
		}
	}
}
