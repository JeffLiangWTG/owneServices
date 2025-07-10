using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.Freight.LocalCartage.GUI;
using Microsoft.Playwright;
using NUnit.Framework;
using WinzorTestFramework;
using WTG.PlaywrightTesting;

namespace Enterprise.Winzor.Architecture.Test;

class RunSheetTileTest
{
	static readonly Color EditBtnBorderColor = Color.FromArgb(180, 180, 180);
	static readonly Color EditBtnFocusBorderColor = Color.FromArgb(57, 150, 224);

	[Test, WithPlaywrightPage]
	public async Task TestRunSheetTileWinzor()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			BusinessObjectFactory dashboardFactory = new BusinessObjectFactory();
			CommonWorkSheet runSheet = dashboardFactory.New<CommonWorkSheet>();
			CommonWorkSheet dashboardRunSheet = dashboardFactory.Load<CommonWorkSheet>(runSheet.PK);
			RunSheetTile tile = new RunSheetTile();
			tile.SetDataBinding(dashboardRunSheet, "");
			form.Controls.Add(tile);
			return form;
		});

		var titleLabel = page.Locator(".label__text");
		await Assertions.Expect(titleLabel).ToHaveCSSAsync("background-color", "rgba(0, 0, 0, 0)");
		Assert.That((await titleLabel.InnerTextAsync()), Is.EqualTo("Idle"));

		var runSheetTile = page.Locator("[data-name=RunSheetTile]");
		await Assertions.Expect(runSheetTile).ToHaveCSSAsync("border-radius", "10px");
		Assert.That((await runSheetTile.GetComputedStyleAsync("border-width")).AsPixels, Is.GreaterThan(0).Within(1.1));
		await Assertions.Expect(runSheetTile).ToHaveCSSAsync("border-style", "solid");
		await Assertions.Expect(runSheetTile).ToHaveCSSAsync("border-color", "rgb(67, 67, 67)");
		await Assertions.Expect(runSheetTile).ToHaveCSSAsync("background-image", "linear-gradient(rgb(117, 117, 117), rgb(255, 255, 255))");

		var editButton = runSheetTile.Locator(".button");
		await Assertions.Expect(editButton).ToHaveCSSAsync("border-radius", "4px");

		var bodyPanel = page.Locator("[data-name=BodyPanel]");
		await Assertions.Expect(bodyPanel).ToHaveCSSAsync("background-color", "rgb(255, 255, 255)");
		Assert.That((await bodyPanel.GetComputedStyleAsync("border-top-width")).AsPixels, Is.GreaterThan(0).Within(1.1));
		await Assertions.Expect(bodyPanel).ToHaveCSSAsync("border-top-style", "solid");
		await Assertions.Expect(bodyPanel).ToHaveCSSAsync("border-top-color", "rgb(92, 92, 92)");
		Assert.That((await bodyPanel.GetComputedStyleAsync("border-left-width")).AsPixels, Is.GreaterThan(0).Within(1.1));
		await Assertions.Expect(bodyPanel).ToHaveCSSAsync("border-left-style", "solid");
		await Assertions.Expect(bodyPanel).ToHaveCSSAsync("border-left-color", "rgb(92, 92, 92)");
		Assert.That((await bodyPanel.GetComputedStyleAsync("border-right-width")).AsPixels, Is.GreaterThan(0).Within(1.1));
		await Assertions.Expect(bodyPanel).ToHaveCSSAsync("border-right-style", "solid");
		await Assertions.Expect(bodyPanel).ToHaveCSSAsync("border-right-color", "rgb(92, 92, 92)");

		var statusBarPanel = page.Locator("[data-name=StatusBarPanel]");
		Assert.That((await statusBarPanel.GetComputedStyleAsync("border-bottom-width")).AsPixels, Is.GreaterThan(0).Within(1.1));
		await Assertions.Expect(statusBarPanel).ToHaveCSSAsync("border-bottom-style", "solid");
		await Assertions.Expect(statusBarPanel).ToHaveCSSAsync("border-bottom-color", "rgb(92, 92, 92)");
		Assert.That((await statusBarPanel.GetComputedStyleAsync("border-left-width")).AsPixels, Is.GreaterThan(0).Within(1.1));
		await Assertions.Expect(statusBarPanel).ToHaveCSSAsync("border-left-style", "solid");
		await Assertions.Expect(statusBarPanel).ToHaveCSSAsync("border-left-color", "rgb(92, 92, 92)");
		Assert.That((await statusBarPanel.GetComputedStyleAsync("border-right-width")).AsPixels, Is.GreaterThan(0).Within(1.1));
		await Assertions.Expect(statusBarPanel).ToHaveCSSAsync("border-right-style", "solid");
		await Assertions.Expect(statusBarPanel).ToHaveCSSAsync("border-right-color", "rgb(92, 92, 92)");
	}

	[Test, WithPlaywrightPage]
	public async Task RunSheetTile_ClickTileEdge_ExpectFocusOnEditButton()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			form.Size = new Size(500, 500);
			BusinessObjectFactory dashboardFactory = new BusinessObjectFactory();
			CommonWorkSheet runSheet = dashboardFactory.New<CommonWorkSheet>();
			CommonWorkSheet dashboardRunSheet = dashboardFactory.Load<CommonWorkSheet>(runSheet.PK);
			RunSheetTile tile1 = new RunSheetTile();
			tile1.SetDataBinding(dashboardRunSheet, "");
			RunSheetTile tile2 = new RunSheetTile();
			tile2.SetDataBinding(dashboardRunSheet, "");
			tile2.Location = new Point(0, 200);
			form.Controls.Add(tile1);
			form.Controls.Add(tile2);
			return form;
		});

		var runSheetTile1 = page.Locator("[data-name=RunSheetTile]").First;
		var runSheetTile2 = page.Locator("[data-name=RunSheetTile]").Last;
		var editButton1 = runSheetTile1.Locator(".button");
		var editButton2 = runSheetTile2.Locator(".button");

		await runSheetTile2.ClickAsync(new () { Position = new Position { X = 1, Y = 1 } });
		await Assertions.Expect(editButton1).ToHaveCSSAsync("border-color", String.Format("rgb({0}, {1}, {2})", EditBtnBorderColor.R, EditBtnBorderColor.G, EditBtnBorderColor.B));
		await Assertions.Expect(editButton2).ToHaveCSSAsync("border-color", String.Format("rgb({0}, {1}, {2})", EditBtnFocusBorderColor.R, EditBtnFocusBorderColor.G, EditBtnFocusBorderColor.B));
	}
}
