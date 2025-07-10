#nullable enable
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using NUnit.Framework;
using WinzorTestFramework;
using WTG.PlaywrightTesting;

namespace WinzorFramework.Extensions;

public class PlaywrightExtensionsTest
{
	public class ComponentForTest : ComponentBase
	{
		protected override void BuildRenderTree(RenderTreeBuilder builder)
		{
			builder.OpenElement(0, "div");
			builder.AddAttribute(0, "id", "testcomponentroot");
			builder.OpenElement(1, "style");
			builder.AddContent(2, new MarkupString($@"
				#greeting {{
					color: blue;
				}}
				#greeting::before {{
					content: 'Foo Bar';
				}}
			"));
			builder.CloseElement();
			builder.OpenElement(3, "style");
			builder.AddAttribute(4, "id", "greeting" );
			builder.AddContent(5, "Hello World");
			builder.CloseElement();
			builder.CloseElement();
		}
	}
	[Test, WithPlaywrightPage]
	public async Task CanGetComputedCssFromLocatorOrHandleAsync()
	{
		await using var ctx = new InMemoryTestServerContext();
		var (page, component) = await ctx.LoadComponentOnFormAsync<ComponentForTest>();
		var greeting = page.Locator("#greeting");
		var colorFromLocator = await greeting.GetComputedStyleAsync("color");
		Assert.That(colorFromLocator, Is.EqualTo(new CSSValue("rgb(0, 0, 255)")));
		var colorFromHandle = await (await greeting.ElementHandleAsync()).GetComputedStyleAsync("color");
		Assert.That(colorFromHandle, Is.EqualTo(colorFromLocator));
	}

	[Test, WithPlaywrightPage]
	public async Task CanGetComputedCssOfPseudoElementFromLocatorOrHandleAsync()
	{
		await using var ctx = new InMemoryTestServerContext();
		var (page, component) = await ctx.LoadComponentOnFormAsync<ComponentForTest>();
		var greeting = page.Locator("#greeting");
		var colorFromLocator = await greeting.GetComputedStyleAsync("color");
		var contentFromLocator = await greeting.GetComputedStyleAsync("::before", "content");
		Assert.That(contentFromLocator, Is.EqualTo(new CSSValue("\"Foo Bar\"")));
		var contentFromHandle = await (await greeting.ElementHandleAsync()).GetComputedStyleAsync("::before", "content");
		Assert.That(contentFromHandle, Is.EqualTo(contentFromLocator));
	}

	[Test]
	public void ComputedCssIsStringComparable()
	{
		Assert.That(new CSSValue("Foo"), Is.EqualTo("Foo"));
	}

	[Test]
	public void CanExtractPixelsFromComputedCss()
	{
		Assert.That(new CSSValue("10px").AsPixels(), Is.EqualTo(10));
	}

	[Test, WithPlaywrightPage]
	public async Task CanGetStyleMessageWithSelectorAndPropertyInfo()
	{
		await using var ctx = new InMemoryTestServerContext();
		var (page, component) = await ctx.LoadComponentOnFormAsync<ComponentForTest>();
		var greeting = page.Locator("#greeting");
		var greetingColorMessage = greeting.GetStyleMessage("color");
		Assert.That(greetingColorMessage, Is.EqualTo("Locator@#greeting { color }"));
	}
}
