using System.Windows.Forms;
using Enterprise.Winzor.Architecture.Test;
using Microsoft.AspNetCore.Components.Rendering;
using WTG.PlaywrightTesting;

namespace NetworkVisualisation.GUI.Winzor.Test.Events;

class ThrottledScrollEventTest
{
	[Test, WithPlaywrightPage]
	public async Task ScrollEventIsThrottledAsync()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadControlOnFormAsync(() => new ControlWithThrottledScroll());

		var scrollContainer = await page.WaitForSelectorAsync(".outer");
		await scrollContainer!.EvaluateAsync("window.scrollEventsFired = 0; window.throttledScrollEventsFired = 0;");
		await scrollContainer.EvaluateAsync("e => e.addEventListener('scroll', () => window.scrollEventsFired++)");
		await scrollContainer.EvaluateAsync("e => e.addEventListener('throttledscroll', () => window.throttledScrollEventsFired++)");

		for (var i = 0; i < 100; i++)
		{
			await scrollContainer.EvaluateAsync("e => e.scrollBy(0, 111)");
		}

		// Wait for all scroll events and throttled callbacks to finish firing
		await Task.Delay(1500);

		var scrollEventsFired = await scrollContainer.EvaluateAsync<int>("window.scrollEventsFired");
		var throttledScrollEventsFired = await scrollContainer.EvaluateAsync<int>("window.throttledScrollEventsFired");
		Assert.That(throttledScrollEventsFired, Is.GreaterThan(0));
		Assert.That(throttledScrollEventsFired, Is.LessThan(scrollEventsFired));
	}

	class ControlWithThrottledScroll : Control
	{
		public ControlWithThrottledScroll()
		{
			Dock = DockStyle.Fill;
		}

		protected override bool ShouldRender => true;

		protected override string ClassName => $"controlwiththrottledscroll {base.ClassName}";

		protected override void BuildRenderTree(RenderTreeBuilder builder)
		{
			builder.OpenElement(1, "div");
			builder.AddAttribute(2, "class", "outer");
			builder.AddAttribute(3, "style", "overflow: scroll; width: 100%; height: 100%;");
			builder.OpenElement(4, "div");
			builder.AddAttribute(5, "class", "inner");
			builder.AddAttribute(6, "style", "width: 12000px; height: 12000px;");
			builder.CloseElement();
			builder.CloseElement();
		}
	}
}
