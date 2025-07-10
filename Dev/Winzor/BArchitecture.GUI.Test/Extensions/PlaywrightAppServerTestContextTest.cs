#nullable enable
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Playwright;
using NUnit.Framework;
using WinzorTestFramework;
using WTG.PlaywrightTesting;

namespace WinzorFramework.Test;

public class PlaywrightInMemoryAppServerTestContextTest
{
	public class ComponentForTest : ComponentBase
	{
		[CascadingParameter]
		public string? CascadingText { get; set; }

		[CascadingParameter]
		public bool OtherCascadingParameter { get; set; }

		[Parameter]
		public string? NonCascadingText { get; set; }

		[Parameter]
		public string? Style { get; set; }

		protected override void BuildRenderTree(RenderTreeBuilder builder)
		{
			builder.OpenElement(0, "div");
			builder.AddAttribute(0, "id", "testcomponentroot");
			builder.AddAttribute(0, "style", Style);
			builder.OpenElement(1, "span");
			builder.AddAttribute(1, "id", "cascadingtext");
			builder.AddContent(2, CascadingText);
			builder.CloseElement();
			builder.OpenElement(3, "span");
			builder.AddAttribute(3, "id", "noncascadingtext" );
			builder.AddContent(4, NonCascadingText);
			builder.CloseElement();
			builder.OpenElement(5, "span");
			builder.AddAttribute(5, "id", "greeting" );
			builder.AddContent(6, "Hello World");
			builder.CloseElement();
			builder.CloseElement();
		}
	}

	[Test, WithPlaywrightPage]
	public async Task CanLoadFormAsync()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadFormAsync(() => {
			var form = new Form();
			form.Controls.Add(new ComponentProxyControl<ComponentForTest>());
			return form;
		});

		await page.GetByText("Hello World").WaitForAsync();
	}

	[Test, WithPlaywrightPage]
	public async Task CanLoadControlAsync()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() => new ComponentProxyControl<ComponentForTest>());

		await page.GetByText("Hello World").WaitForAsync();
	}

	[Test, WithPlaywrightPage]
	public async Task CanLoadComponentWithoutOptionalArgumentsAsync()
	{
		await using var ctx = new InMemoryTestServerContext();
		var (page, _) = await ctx.LoadComponentOnFormAsync<ComponentForTest>();

		await page.GetByText("Hello World").WaitForAsync();
	}

	[Test, WithPlaywrightPage]
	public async Task CanLoadComponentWithArgumentsAsync()
	{
		await using var ctx = new InMemoryTestServerContext();
		var (page, component) = await ctx.LoadComponentOnFormAsync<ComponentForTest>(parameters =>
			parameters
				.Add(x => x.NonCascadingText, "Foo")
				.Add(x => x.CascadingText, "Bar")
				.Add(x => x.Style, "color: red;")
				.Add(x => x.OtherCascadingParameter, true)
		);
		var root = page.Locator("#testcomponentroot");
		var nonCascadingText = page.Locator("#noncascadingtext");
		var cascadingText = page.Locator("#cascadingtext");

		Assert.That(component.Instance, Is.Not.Null);
		Assert.That(component.Instance!.Style, Is.EqualTo("color: red;"));
		Assert.That(component.Instance.NonCascadingText, Is.EqualTo("Foo"));
		Assert.That(component.Instance.CascadingText, Is.EqualTo("Bar"));
		Assert.That(component.Instance.OtherCascadingParameter, Is.True);
		Assert.That(await root.GetAttributeAsync("style"), Is.EqualTo("color: red;"));
		Assert.That(await nonCascadingText.InnerTextAsync(), Is.EqualTo("Foo"));
		Assert.That(await cascadingText.InnerTextAsync(), Is.EqualTo("Bar"));
	}

	[Test, WithPlaywrightPage]
	public async Task CanInitialiseArgumentsAfterLoadAsync()
	{
		await using var ctx = new InMemoryTestServerContext();
		var (page, component) = await ctx.LoadComponentOnFormAsync<ComponentForTest>();
		var root = page.Locator("#testcomponentroot");
		var nonCascadingText = page.Locator("#noncascadingtext");
		var cascadingText = page.Locator("#cascadingtext");

		Assert.That(component.Instance, Is.Not.Null);
		Assert.That(component.Instance!.Style, Is.Null);
		Assert.That(component.Instance.NonCascadingText, Is.Null);
		Assert.That(component.Instance.CascadingText, Is.Null);
		Assert.That(component.Instance.OtherCascadingParameter, Is.False);
		Assert.That(await root.GetAttributeAsync("style"), Is.Null);
		Assert.That(await nonCascadingText.InnerTextAsync(), Is.EqualTo(string.Empty));
		Assert.That(await cascadingText.InnerTextAsync(), Is.EqualTo(string.Empty));

		await component.UpdateParametersAsync(parameters =>
			parameters
				.Add(x => x.NonCascadingText, "Foo")
				.Add(x => x.CascadingText, "Bar")
				.Add(x => x.Style, "color: red;")
				.Add(x => x.OtherCascadingParameter, true)
		);

		Assert.That(component.Instance, Is.Not.Null);
		Assert.That(component.Instance!.Style, Is.EqualTo("color: red;"));
		Assert.That(component.Instance.NonCascadingText, Is.EqualTo("Foo"));
		Assert.That(component.Instance.CascadingText, Is.EqualTo("Bar"));
		Assert.That(component.Instance.OtherCascadingParameter, Is.True);
		Assert.That(await root.GetAttributeAsync("style"), Is.EqualTo("color: red;"));
		Assert.That(await nonCascadingText.InnerTextAsync(), Is.EqualTo("Foo"));
		Assert.That(await cascadingText.InnerTextAsync(), Is.EqualTo("Bar"));
	}

	[Test, WithPlaywrightPage]
	public async Task CanUpdateArgumentsAfterLoadAsync()
	{
		await using var ctx = new InMemoryTestServerContext();
		var (page, component) = await ctx.LoadComponentOnFormAsync<ComponentForTest>(parameters => parameters
			.Add(x => x.NonCascadingText, "Foo")
			.Add(x => x.CascadingText, "Bar")
			.Add(x => x.Style, "color: red;")
			.Add(x => x.OtherCascadingParameter, true));
		var root = page.Locator("#testcomponentroot");
		var cascadingText = page.Locator("#cascadingtext");
		var nonCascadingText = page.Locator("#noncascadingtext");

		Assert.That(component.Instance, Is.Not.Null);
		Assert.That(component.Instance!.Style, Is.EqualTo("color: red;"));
		Assert.That(component.Instance.NonCascadingText, Is.EqualTo("Foo"));
		Assert.That(component.Instance.CascadingText, Is.EqualTo("Bar"));
		Assert.That(component.Instance.OtherCascadingParameter, Is.True);
		Assert.That(async () => await root.GetAttributeAsync("style"), Is.EqualTo("color: red;").After(500,50));
		Assert.That(await nonCascadingText.InnerTextAsync(), Is.EqualTo("Foo"));
		Assert.That(await cascadingText.InnerTextAsync(), Is.EqualTo("Bar"));

		await component.UpdateParametersAsync(
			parameters =>
				parameters
					.Add(x => x.NonCascadingText, "One")
					.Add(x => x.CascadingText, "Two")
					.Add(x => x.Style, "color: blue;")
					.Add(x => x.OtherCascadingParameter, false)
		);

		Assert.That(component.Instance, Is.Not.Null);
		Assert.That(component.Instance!.Style, Is.EqualTo("color: blue;"));
		Assert.That(component.Instance.NonCascadingText, Is.EqualTo("One"));
		Assert.That(component.Instance.CascadingText, Is.EqualTo("Two"));
		Assert.That(component.Instance.OtherCascadingParameter, Is.False);
		Assert.That(async () => await root.GetAttributeAsync("style"), Is.EqualTo("color: blue;").After(500, 50));
		Assert.That(await nonCascadingText.InnerTextAsync(), Is.EqualTo("One"));
		Assert.That(await cascadingText.InnerTextAsync(), Is.EqualTo("Two"));
	}

	[Test, WithPlaywrightPage]
	public async Task CanRemoveArgumentsByUpdatingToNullAsync()
	{
		await using var ctx = new InMemoryTestServerContext();
		var (page, component) = await ctx.LoadComponentOnFormAsync<ComponentForTest>(parameters =>
			parameters
				.Add(x => x.NonCascadingText, "Foo")
				.Add(x => x.CascadingText, "Bar")
				.Add(x => x.Style, "color: red;")
				.Add(x => x.OtherCascadingParameter, true)
		);
		var root = page.Locator("#testcomponentroot");
		var nonCascadingText = page.Locator("#noncascadingtext");
		var cascadingText = page.Locator("#cascadingtext");

		Assert.That(component.Instance, Is.Not.Null);
		Assert.That(component.Instance!.Style, Is.EqualTo("color: red;"));
		Assert.That(component.Instance.NonCascadingText, Is.EqualTo("Foo"));
		Assert.That(component.Instance.CascadingText, Is.EqualTo("Bar"));
		Assert.That(component.Instance.OtherCascadingParameter, Is.True);
		Assert.That(async () => await root.GetAttributeAsync("style"), Is.EqualTo("color: red;").After(500, 50));
		Assert.That(await nonCascadingText.InnerTextAsync(), Is.EqualTo("Foo"));
		Assert.That(await cascadingText.InnerTextAsync(), Is.EqualTo("Bar"));

		await component.UpdateParametersAsync(
			parameters =>
				parameters
					.Add(x => x.NonCascadingText, null)
					.Add(x => x.Style, null)
		);

		Assert.That(component.Instance, Is.Not.Null);
		Assert.That(component.Instance!.Style, Is.Null);
		Assert.That(component.Instance.NonCascadingText, Is.Null);
		Assert.That(component.Instance.CascadingText, Is.EqualTo("Bar"));
		Assert.That(component.Instance.OtherCascadingParameter, Is.True);
		Assert.That(async () => await root.GetAttributeAsync("style"), Is.Null.After(500, 50));
		Assert.That(await cascadingText.InnerTextAsync(), Is.EqualTo("Bar"));
		Assert.That(await nonCascadingText.InnerTextAsync(), Is.EqualTo(string.Empty));
	}

	[Test, WithPlaywrightPage]
	public async Task WillUsePlaywrightPageIfAtAboutBlankAsync()
	{
		Assert.That(PlaywrightTestContext.BrowserContext.Pages.Count, Is.EqualTo(1));
		Assert.That(PlaywrightTestContext.Page.Url, Is.EqualTo("about:blank"));

		await using var ctx = new InMemoryTestServerContext();
		var (page, _) = await ctx.LoadComponentOnFormAsync<ComponentForTest>();

		Assert.That(PlaywrightTestContext.BrowserContext.Pages.Count, Is.EqualTo(1));
		Assert.That(page, Is.SameAs(PlaywrightTestContext.Page));
	}

	[Test, WithPlaywrightPage]
	public async Task WillNotUsePlaywrightPageIfNotAtAboutBlankAsync()
	{
		await using var ctx = new InMemoryTestServerContext();

		Assert.That(PlaywrightTestContext.BrowserContext.Pages.Count, Is.EqualTo(1));
		Assert.That(PlaywrightTestContext.Page.Url, Is.EqualTo("about:blank"));

		var response = await PlaywrightTestContext.Page.GotoAsync(ctx.ServerBaseUrl);

		Assert.That(response!.Ok, Is.True);
		Assert.That(PlaywrightTestContext.Page.Url.TrimEnd('/'), Is.EqualTo(ctx.ServerBaseUrl));
		PlaywrightTestContext.ConsoleMessages.Clear();

		var (page, _) = await ctx.LoadComponentOnFormAsync<ComponentForTest>();

		Assert.That(PlaywrightTestContext.BrowserContext.Pages.Count, Is.EqualTo(2));
		Assert.That(page, Is.Not.SameAs(PlaywrightTestContext.Page));

		await PlaywrightTestContext.Page.CloseAsync();
	}

	[Test, WithPlaywrightPage]
	public async Task SupportsMultiplePagesAsync()
	{
		await using var ctx = new InMemoryTestServerContext();
		var (first, _) = await ctx.LoadComponentOnFormAsync<ComponentForTest>();

		Assert.That(PlaywrightTestContext.BrowserContext.Pages.Count, Is.EqualTo(1));
		Assert.That(first, Is.SameAs(PlaywrightTestContext.Page));

		var (second, _) = await ctx.LoadComponentOnFormAsync<ComponentForTest>();

		Assert.That(PlaywrightTestContext.BrowserContext.Pages.Count, Is.EqualTo(2));
		Assert.That(second, Is.Not.SameAs(PlaywrightTestContext.Page));
		Assert.That(second, Is.Not.SameAs(first));
		Assert.That(first, Is.SameAs(PlaywrightTestContext.Page));
	}

	[Test, WithPlaywrightPage]
	public async Task ClosesPagesLoadedWithFormsOnDispose()
	{
		IPage? first;
		IPage? second;
		await using (var ctx = new InMemoryTestServerContext())
		{
			first = (await ctx.LoadComponentOnFormAsync<ComponentForTest>()).page;

			second = (await ctx.LoadComponentOnFormAsync<ComponentForTest>()).page;

			Assert.That(PlaywrightTestContext.BrowserContext.Pages.Count, Is.EqualTo(2));
			Assert.That(second, Is.Not.SameAs(first));
		}

		Assert.That(PlaywrightTestContext.BrowserContext.Pages.Count, Is.EqualTo(0));
		Assert.That(PlaywrightTestContext.Page.IsClosed, Is.True);
		Assert.That(first.IsClosed, Is.True);
		Assert.That(second.IsClosed, Is.True);
	}

	[Test, WithPlaywrightPage]
	public async Task DoesNotCloseAttributePageIfNotLoadedWithForm()
	{
		IPage? formless;
		IPage? first;
		IPage? second;
		await using (var ctx = new InMemoryTestServerContext())
		{
			formless = await PageHelper.LoadPageAsync(new Uri($"{ctx.ServerBaseUrl}/?foobar"), 1280, 720);
			Assert.That(formless.Url, Is.EqualTo($"{ctx.ServerBaseUrl}/?foobar"));

			first = (await ctx.LoadComponentOnFormAsync<ComponentForTest>()).page;

			second = (await ctx.LoadComponentOnFormAsync<ComponentForTest>()).page;

			Assert.That(PlaywrightTestContext.BrowserContext.Pages.Count, Is.EqualTo(3));
			Assert.That(first, Is.Not.SameAs(PlaywrightTestContext.Page));
			Assert.That(second, Is.Not.SameAs(PlaywrightTestContext.Page));
			Assert.That(second, Is.Not.SameAs(first));
		}

		Assert.That(PlaywrightTestContext.BrowserContext.Pages.Count, Is.EqualTo(1));
		Assert.That(PlaywrightTestContext.Page.IsClosed, Is.False);
		Assert.That(first.IsClosed, Is.True);
		Assert.That(second.IsClosed, Is.True);
		Assert.That(formless.IsClosed, Is.False);

		IConsoleMessage? disconnectionError = null;
		var waitForDisconnectedMessage = Task.Run(async () =>
		{
			while (true)
			{
				disconnectionError = PlaywrightTestContext.ConsoleMessages.FirstOrDefault(e => e.Text.EndsWith("Error: Connection disconnected with error 'Error: WebSocket closed with status code: 1006 (no reason given).'."));
				if (disconnectionError != null)
				{
					break;
				}
				await Task.Delay(200);
			}
		});
		await Task.WhenAny(Task.Delay(1000), waitForDisconnectedMessage);

		Assert.That(disconnectionError, Is.Not.Null);
		PlaywrightTestContext.ConsoleMessages.Remove(disconnectionError); // We expect a websocket disconnection because we disposed the App Server above
		await formless.CloseAsync();
	}
}
