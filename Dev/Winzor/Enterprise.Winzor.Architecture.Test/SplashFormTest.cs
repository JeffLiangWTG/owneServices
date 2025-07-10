using System.Threading.Tasks;
using Bunit;
using NUnit.Framework;

namespace Enterprise.Winzor.Architecture.Test;

class SplashFormTest
{
	[Test]
	public async Task TestUpdateProgress()
	{
		using var ctx = new EnterpriseTestContext();
		SplashForm form = null;
		var rendered = await ctx.RenderFormAsync(() => form = new SplashForm());
		var filler = rendered.Find(".splash > .splash__top > .splash__progressbar > .progressbar__filler");
		Assert.That(filler, Is.Not.Null);

		for (int i = 0; i < 10; i++)
		{
			await form.InvokeWinzorDispatcherAsync(() => form.UpdateProgress((i + 1) * 10));
			Assert.That(filler.GetAttribute("style"), Does.Contain($"width: {i + 1}0%"));
		}
	}

	[Test]
	public async Task TestSplashFormTextNotEmpty()
	{
		using var ctx = new EnterpriseTestContext();
		SplashForm form = null;
		var rendered = await ctx.RenderFormAsync(() => form = new SplashForm());
		Assert.That(string.IsNullOrEmpty(form.Text), Is.False);
	}
}
