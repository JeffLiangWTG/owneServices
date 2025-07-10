using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using Enterprise.VisualBoards.GUI;
using Enterprise.ZArchitecture.GUI;
using Microsoft.Playwright;
using NUnit.Framework;
using WTG.PlaywrightTesting;

namespace Enterprise.Winzor.Architecture.Test;

using static PlaywrightTestContext;

class DragDropHelperTest
{
	[Test, WithPlaywrightPage]
	public async Task TestDragDropHelperTestMakePositionChange()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		ZUserControl userControl = null;
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			userControl = new ZUserControl();
			var handler = DragDropHelper.AddDragDropSupport(userControl);
			return userControl;
		});

		var element = page.Locator("div[draggable='true']");
		await element.WaitForAsync();

		Assert.That(userControl.Left, Is.EqualTo(0));
		Assert.That(userControl.Top, Is.EqualTo(0));

		await page.Mouse.MoveAsync(10, 10);
		await page.Mouse.DownAsync();
		await page.Mouse.MoveAsync(30, 30);
		await page.Mouse.UpAsync();

		await Assertions.Expect(element).ToHaveCSSAsync("left", "20px");
		await Assertions.Expect(element).ToHaveCSSAsync("top", "20px");

		Assert.That(() => userControl.Left, Is.EqualTo(20).After(1000, 100));
		Assert.That(() => userControl.Top, Is.EqualTo(20).After(1000, 100));
	}
}
