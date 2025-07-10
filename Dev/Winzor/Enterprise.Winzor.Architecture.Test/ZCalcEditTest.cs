using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using Enterprise.ZArchitecture;
using NUnit.Framework;
using WTG.PlaywrightTesting;

namespace Enterprise.Winzor.Architecture.Test;

using static PlaywrightTestContext;

internal class ZCalcEditTest
{
	[Test, WithPlaywrightPage]
	public async Task TextBoxStyle()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		ZCalcEdit zCalcEdit = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			zCalcEdit = new ZCalcEdit()
			{
				Text = "90"
			};
			var form = new Form();
			form.Controls.Add(zCalcEdit);
			return form;
		});
		var zCalcEditInput = await page.WaitForSelectorAsync("input");

		await zCalcEditInput.PressAsync("KeyT");
		await zCalcEditInput.PressAsync("KeyE");
		await zCalcEditInput.PressAsync("KeyS");
		await zCalcEditInput.PressAsync("KeyT");

		Assert.That(async () => await zCalcEditInput.InputValueAsync(), Is.EqualTo("90").After(3000, 100));
	}
}
