using System;
using System.Threading.Tasks;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using WTG.PlaywrightTesting;

namespace Enterprise.Winzor.Architecture.Test;

using static PlaywrightTestContext;

public class PanelTest
{
	[Test, WithPlaywrightPage]
	public async Task ZCodeFindBoxOnPanelCanBeFocused()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		ZCodeFindBox findBox = null;
		ZForm form = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new ZForm();
			var panel = new KPanel();
			form.Controls.Add(panel);
			form.ActiveControl = panel;
			findBox = new ZCodeFindBox();
			panel.Controls.Add(findBox);
			return form;
		});

		var input = await page.WaitForSelectorAsync("input");
		await input.FillAsync("DEN");

		Assert.That(async () => await input.InputValueAsync(), Is.EqualTo("DEN").After(1000, 100));
		Assert.That(form.ActiveControl, Is.EqualTo(findBox));
	}
}
