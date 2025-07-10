using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.JSInterop;
using NUnit.Framework;
using WinzorTestFramework;
using WTG.PlaywrightTesting;

namespace WinzorFramework.Extensions;

internal sealed class ElementReferenceExtensionTest
{
	[Test, WithPlaywrightPage]
	public async Task TryFocusOnClientAsync_FocusableElement_ReturnsTrueAndDoesNotFireFocusIn()
	{
		await using var ctx = new InMemoryTestServerContext();
		Form form = null;
		ControlForTest control = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form();
			control = new ControlForTest();
			form.Controls.Add(control);
			return form;
		});
		
		var controlElement = await page.WaitForSelectorAsync(".componentfortest");

		await form.InvokeWinzorDispatcherAsync(() => form.ActiveControl = null);
		Assert.That(() => control.Focused, Is.False.After(3000, 100));
		await controlElement.EvaluateAsync("element => element.blur()");
		Assert.That(async () => await controlElement.EvaluateAsync<bool>("element => document.activeElement === element"), Is.False.After(3000, 100));

		var winformsFocusEventFired = new TaskCompletionSource();

		await control.InvokeWinzorDispatcherAsync(() => {
			// Wire up to the pre focus in event to intercept the focus message invoked from the browser to validate the arguments and also remove the focus on the server
			EventHandler<WinzorFocusInEventArgs> handleBeforeOnFocusIn = null;
			handleBeforeOnFocusIn = delegate(object sender, WinzorFocusInEventArgs args)
			{
				control.BeforeOnFocusIn -= handleBeforeOnFocusIn;
				Assert.That(args.InitiatedFromServer, Is.True);

				// We want to simualate a scenario where something else on the server has taken the focus in the time the event has round tripped from the server so we will manually 
				// remove the focus from the element here. Without this, the focus event would not be fired in winforms as we don't fire for elements that are already focused.
				form.ActiveControl = null;
				Assert.That(control.Focused, Is.False);

				// Wire up to the winforms focus event so we can ensure it is not being fired
				control.GotFocus += (sender, args) => winformsFocusEventFired.SetResult();
			};
			control.BeforeOnFocusIn += handleBeforeOnFocusIn;

			// With everything now setup, we can fire the focus event from the server
			control.Focus();
		});

		Assert.That(await winformsFocusEventFired.Task.WithTimeout(TimeSpan.FromSeconds(3)), Is.False);
		// While we did not fire a focus in event on the server, we should still have focused the input on the client
		Assert.That(async () => await controlElement.EvaluateAsync<bool>("element => document.activeElement === element"), Is.True.After(3000, 100));

		// Now lets test that the focus in is still being fired for non server invoked focus events
		await controlElement.EvaluateAsync("element => element.blur()");
		Assert.That(async () => await controlElement.EvaluateAsync<bool>("element => document.activeElement === element"), Is.False.After(3000, 100));
		await controlElement.EvaluateAsync("element => element.focus()");
		Assert.That(async () => await controlElement.EvaluateAsync<bool>("element => document.activeElement === element"), Is.True.After(3000, 100));
		Assert.That(await winformsFocusEventFired.Task.WithTimeout(TimeSpan.FromSeconds(3)), Is.True);
	}

	[Test, WithPlaywrightPage]
	public async Task TryFocusOnClientAsync_AlreadyFocusedElement_ReturnsTrueAndDoesNotFireFocusIn()
	{
		await using var ctx = new InMemoryTestServerContext();
		TextBox textBox1 = null;
		TextBox textBox2 = null;
		Form form = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form();
			form.Controls.Add(textBox1 = new TextBox() { Text = "One", Top = 0 });
			form.Controls.Add(textBox2 = new TextBox() { Text = "Two", Top = 100 });
			return form;
		});
		Assert.That(form, Is.Not.Null);
		
		var textBoxElement2 = await page.WaitForSelectorAsync("input:nth-child(2)");
		await textBoxElement2.ClickAsync();
		Assert.That(() => textBox2.Focused, Is.True.After(3000, 100));
		await textBox2.InvokeWinzorDispatcherAsync(() =>
		{
			textBox2.Focus();
		});
		var textBoxElement1 = await page.WaitForSelectorAsync("input:nth-child(1)");
		await textBoxElement1.ClickAsync();
		Assert.That(() => textBox1.Focused, Is.True.After(3000, 100));
		var controlGotFocus = false;
		await textBox2.InvokeWinzorDispatcherAsync(() =>
		{
			textBox2.GotFocus += (s, e) => controlGotFocus = true;
		});
		textBoxElement2 = await page.WaitForSelectorAsync("input:nth-child(2)");
		await textBoxElement2.ClickAsync();
		Assert.That(() => textBox2.Focused, Is.True.After(3000, 100));
		Assert.That(controlGotFocus);
	}

	[Test, WithPlaywrightPage]
	public async Task TryFocusOnClientAsync_DefaultElementReference_ReturnsFalseAndResetsFocus()
	{
		await using var ctx = new InMemoryTestServerContext();
		Form form = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form();
			var control = new TextBox();
			form.Controls.Add(control);
			return form;
		});

		var textboxElement = page.Locator("input");
		Assert.That(async () => await textboxElement.EvaluateAsync<bool>("element => document.activeElement === element"), Is.True, "The textbox element should be focused");

		Assert.That(async () => await default(ElementReference).TryFocusOnClientAsync(form.JSRuntime), Is.False);
		Assert.That(async () => await textboxElement.EvaluateAsync<bool>("element => document.activeElement === element"), Is.False, "The textbox element should not be focused");
	}

	[Test, WithPlaywrightPage]
	public async Task TryFocusOnClientAsync_UnassignedElementReference_ReturnsFalseAndResetsFocus()
	{
		await using var ctx = new InMemoryTestServerContext();
		Control controlToRemove = null;
		Form form = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form();
			form.Controls.Add(new TextBox());
			form.Controls.Add(controlToRemove = new TextBox());
			return form;
		});

		var elementReference = controlToRemove.ElementReference;
		await form.InvokeWinzorDispatcherAsync(() => form.Controls.Remove(controlToRemove));

		Assert.That(elementReference, Is.Not.EqualTo(default(ElementReference)));

		var textboxElement = page.Locator("input");
		Assert.That(async () => await textboxElement.EvaluateAsync<bool>("element => document.activeElement === element"), Is.True, "The textbox element should be focused");

		Assert.That(async () => await elementReference.TryFocusOnClientAsync(form.JSRuntime), Is.False);
		Assert.That(async () => await textboxElement.EvaluateAsync<bool>("element => document.activeElement === element"), Is.False, "The textbox element should not be focused");
	}

	[Test, WithPlaywrightPage]
	public async Task TryFocusOnClientAsync_NonFocusableElement_ReturnsFalseAndResetsFocus()
	{
		await using var ctx = new InMemoryTestServerContext();
		NonFocusableControl nonFocusableControl = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			form.Controls.Add(new TextBox());
			form.Controls.Add(nonFocusableControl = new NonFocusableControl());
			return form;
		});

		var textboxElement = page.Locator("input");
		Assert.That(async () => await textboxElement.EvaluateAsync<bool>("element => document.activeElement === element"), Is.True, "The textbox element should be focused");

		Assert.That(async () => await nonFocusableControl.ElementReference.TryFocusOnClientAsync(nonFocusableControl.JSRuntime), Is.False);
		Assert.That(async () => await textboxElement.EvaluateAsync<bool>("element => document.activeElement === element"), Is.False, "The textbox element should not be focused");
	}

	class ControlForTest : Control
	{
		public ControlForTest()
		{
			SetStyle(ControlStyles.Selectable, true);
			Width = 300;
			Height = 300;
		}

		public event EventHandler<WinzorFocusInEventArgs> BeforeOnFocusIn;

		protected async Task OnFocusInOverrideAsync(WinzorFocusInEventArgs args)
		{
			await InvokeWinzorDispatcherAsync(() => BeforeOnFocusIn?.Invoke(this, args));
			await OnFocusInAsync(args);
		}

		protected override void BuildRenderTree(RenderTreeBuilder builder)
		{
			builder.OpenElement(0, "input");
			builder.AddAttribute(1, "class", "componentfortest");
			builder.AddAttribute(2, "style", ControlStyleString);
			builder.AddAttribute(3, "onwinzorfocusin", OnFocusInOverrideAsync);
			builder.AddElementReferenceCapture(4, reference => ElementReference = reference);
			builder.CloseElement();
		}
	}

	class NonFocusableControl : Control
	{
		public NonFocusableControl()
		{
			Width = 300;
			Height = 300;
		}

		protected override void BuildRenderTree(RenderTreeBuilder builder)
		{
			builder.OpenElement(0, "div");
			builder.AddAttribute(1, "class", "nonfocusablecontrol");
			builder.AddAttribute(2, "style", ControlStyleString);
			builder.AddAttribute(3, "tabindex", string.Empty);
			builder.AddElementReferenceCapture(4, reference => ElementReference = reference);
			builder.CloseElement();
		}
	}

	[Test, WithPlaywrightPage]
	public async Task ElementScrollTo()
	{
		await using var ctx = new InMemoryTestServerContext();
		ControlWithScroll control = null;
		var page = await ctx.LoadControlOnFormAsync(() => control = new ControlWithScroll());

		Assert.That(control, Is.Not.Null);

		var scrollContainer = await page.WaitForSelectorAsync(".outer");
		Assert.That(scrollContainer, Is.Not.Null);
		Assert.That(async () => await scrollContainer.EvaluateAsync<double>("e => e.scrollLeft"), Is.EqualTo(0).After(3000, 100));
		Assert.That(async () => await scrollContainer.EvaluateAsync<double>("e => e.scrollTop"), Is.EqualTo(0).After(3000, 100));

		await control.ElementReference.ScrollToAsync(control.IJSRuntime, 123, 456);
		Assert.That(async () => await scrollContainer.EvaluateAsync<double>("e => e.scrollLeft"), Is.EqualTo(123).After(3000, 100));
		Assert.That(async () => await scrollContainer.EvaluateAsync<double>("e => e.scrollTop"), Is.EqualTo(456).After(3000, 100));
	}

	[Test]
	public async Task ElementScrollToScrollBehavior()
	{
		using var ctx = new WinzorTestContext();

		ControlWithScroll control = null!;
		_ = await ctx.RenderControlOnFormAsync(() => control = new ControlWithScroll());

		var controlElement = control.ElementReference;

		await controlElement.ScrollToAsync(control.IJSRuntime, 123, 456);

		var invocations = ctx.JSInterop.Invocations
			.Where(i => i.Identifier == "scrollElementTo" && (i.Arguments[0])!.Equals(controlElement) && !(bool)i.Arguments[3]);
		Assert.That(invocations.Count, Is.EqualTo(1));

		await controlElement.ScrollToAsync(control.IJSRuntime, 123, 456, animated: true);

		var animatedInvocations = ctx.JSInterop.Invocations
			.Where(i => i.Identifier == "scrollElementTo" && (i.Arguments[0])!.Equals(controlElement) && (bool)i.Arguments[3]);
		Assert.That(animatedInvocations.Count, Is.EqualTo(1));
	}

	class ControlWithScroll : Control
	{
		public ControlWithScroll()
		{
			Dock = DockStyle.Fill;
		}

		protected internal override bool ShouldRender => true;

		protected override string ClassName => $"controlwithscroll {base.ClassName}";

		protected override void BuildRenderTree(RenderTreeBuilder builder)
		{
			builder.OpenElement(1, "div");
			builder.AddAttribute(2, "class", "outer");
			builder.AddAttribute(3, "style", "overflow: scroll; width: 100%; height: 100%;");
			builder.AddElementReferenceCapture(4, reference => ElementReference = reference);
			builder.OpenElement(5, "div");
			builder.AddAttribute(6, "class", "inner");
			builder.AddAttribute(7, "style", "width: 10000px; height: 10000px;");
			builder.CloseElement();
			builder.CloseElement();
		}

		public IJSRuntime IJSRuntime => JSRuntime;
	}
}
