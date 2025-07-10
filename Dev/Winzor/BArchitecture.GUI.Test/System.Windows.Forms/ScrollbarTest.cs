using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.JSInterop;
using NUnit.Framework;
using WinzorTestFramework;
using WTG.PlaywrightTesting;

namespace WinzorFramework.Test;
class ScrollbarTest
{
	[Test, WithPlaywrightPage]
	public async Task ScrollbarShouldRenderAccurately()
	{
		await using var ctx = new InMemoryTestServerContext();
		ControlWithScroll control = null;
		var page = await ctx.LoadControlOnFormAsync(() => control = new ControlWithScroll());

		Assert.That(control, Is.Not.Null);

		var scrollContainer = await page.WaitForSelectorAsync(".outer");
		Assert.That(scrollContainer, Is.Not.Null);
		Assert.That(async () => await scrollContainer.EvaluateAsync<string>("sC => getComputedStyle(sC, '::-webkit-scrollbar').backgroundColor"), Is.EqualTo("rgb(240, 240, 240)"));
		Assert.That(async () => await scrollContainer.EvaluateAsync<string>("sC => getComputedStyle(sC, '::-webkit-scrollbar-track').backgroundColor"), Is.EqualTo("rgb(241, 241, 241)"));
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
