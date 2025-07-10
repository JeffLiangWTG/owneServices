using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using AngleSharp.Dom;
using Bunit;
using CargoWise.Blazor.Client.Integration.Menus;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Packing.GUI;
using Enterprise.ZArchitecture.GUI;
using Microsoft.Playwright;
using Moq;
using NUnit.Framework;
using WinzorTestFramework;
using WTG.PlaywrightTesting;

namespace Enterprise.Winzor.Architecture.Test;

class PackingTreeViewTest
{
	[Test]
	public async Task PackingTreeViewRendered()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var treeViewForTest = new PackingTreeView() { Size = new Size(500, 300) };
			var factory = new BusinessObjectFactory();
			var data = new TestDataForPacking(factory);
			data.CreatePackingData();

			var pkgPackageJob = data.PackageJob;
			treeViewForTest.Nodes.Add(new PackingTreeNode(pkgPackageJob));
			treeViewForTest.TopNode.Text = "test 1";

			var pkgPackage = pkgPackageJob.Packages.AddNew("PLT", "PID01");
			var node2 = new PackingTreeNode(pkgPackage);
			node2.Text = "test 2";
			treeViewForTest.Nodes.Add(node2);

			return treeViewForTest;
		});

		var packingTreeView = rendered.Find(".treeview");
		Assert.That(packingTreeView, Is.Not.Null);

		var packingTreeNodes = rendered.Find(".treeview__nodes");
		Assert.That(packingTreeNodes, Is.Not.Null);

		var nodes = rendered.FindAll("ul > li");
		Assert.That(nodes.Count, Is.EqualTo(2));
		Assert.That(nodes[0].TextContent, Does.Contain("test 1"));
		Assert.That(nodes[1].TextContent, Does.Contain("test 2"));
	}

	[Test]
	public async Task TestPackingTreeViewTokenUpdated()
	{
		using var ctx = new EnterpriseTestContext();
		PackingTreeView treeView = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			treeView = new PackingTreeView() { Size = new Size(500, 300) };
			var factory = new BusinessObjectFactory();
			var data = new TestDataForPacking(factory);
			data.CreatePackingData();

			var pkgPackageJob = data.PackageJob;
			treeView.Nodes.Add(new PackingTreeNode(pkgPackageJob));
			treeView.TopNode.Text = "test 1";

			var pkgPackage = pkgPackageJob.Packages.AddNew("PLT", "PID01");
			var node2 = new PackingTreeNode(pkgPackage);
			node2.Text = "test 2";
			treeView.Nodes.Add(node2);

			return treeView;
		});

		var packingTreeView = rendered.Find(".treeview");
		Assert.That(packingTreeView, Is.Not.Null);

		var nodeSelector = "ul > li";
		var nodes = rendered.FindAll(nodeSelector);
		Assert.That(nodes[0].TextContent, Does.Contain("test 1(1x PLT, Wgt: 0 KG)"));
		Assert.That(nodes[1].TextContent, Does.Contain("test 2(ID: PID01, Wgt: 0 KG)"));

		var tokenList = new List<PackingTreeNodeSummaryToken>() { treeView.NodePainter.Summary.Tokens.Cast<PackingTreeNodeSummaryToken>().ToArray()[2] };
		await treeView.InvokeWinzorDispatcherAsync(() => treeView.NodePainter.UpdateSummaryAndRepaint(tokenList));

		rendered.WaitForState(() => rendered.FindAll(nodeSelector).Count == 2);
		rendered.WaitForAssertion(() => Assert.That(rendered.FindAll(nodeSelector)[0].TextContent, Is.EqualTo("test 1(Wgt: 0 KG)")));
		rendered.WaitForAssertion(() => Assert.That(rendered.FindAll(nodeSelector)[1].TextContent, Is.EqualTo("test 2(Wgt: 0 KG)")));
	}

	[Test]
	public async Task TestPackingTreeViewStatusUpdated()
	{
		using var ctx = new EnterpriseTestContext();
		PackingTreeView treeView = null;
		PkgPackage pkgPackage = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			treeView = new PackingTreeView() { Size = new Size(500, 300) };
			var factory = new BusinessObjectFactory();
			var data = new TestDataForPacking(factory);
			data.CreatePackingData();

			var pkgPackageJob = data.PackageJob;
			pkgPackageJob.AddRowWarning("damage goods");

			var node = new PackingTreeNode(pkgPackageJob);
			treeView.Nodes.Add(node);
			treeView.TopNode.Text = "test 1";

			pkgPackage = pkgPackageJob.Packages.AddNew("PLT", "PID01");
			pkgPackage.KP_ClosedTimeUtc = ZDateTime.UtcNow;

			var node2 = new PackingTreeNode(pkgPackage);
			node2.Text = "test 2";
			treeView.Nodes.Add(node2);

			return treeView;
		});

		rendered.WaitForState(() => rendered.FindAll("ul > li").Count == 2);
		var packingTreeView = rendered.Find(".treeview");
		Assert.That(packingTreeView, Is.Not.Null);

		var nodeSelector = "ul > li";
		var nodes = rendered.FindAll(nodeSelector);
		Assert.That(nodes[0].TextContent, Does.Contain("test 1(1x PLT, Wgt: 0 KG)"));
		Assert.That(nodes[1].TextContent, Does.Contain("test 2(ID: PID01, Wgt: 0 KG)Closed"));

		await treeView.InvokeWinzorDispatcherAsync(() =>
		{
			pkgPackage.KP_IsReleasedViaJob = true;
			var tokenList = new List<PackingTreeNodeSummaryToken> { treeView.NodePainter.Summary.Tokens.Cast<PackingTreeNodeSummaryToken>().ToArray()[2] };
			treeView.NodePainter.UpdateSummaryAndRepaint(tokenList);
		});

		rendered.WaitForState(() => rendered.FindAll(nodeSelector).Count == 2);
		rendered.WaitForAssertion(() => Assert.That(rendered.FindAll(nodeSelector)[0].TextContent, Is.EqualTo("test 1(Wgt: 0 KG)")));
		rendered.WaitForAssertion(() => Assert.That(rendered.FindAll(nodeSelector)[1].TextContent, Is.EqualTo("test 2(Wgt: 0 KG)Released via Job")));
	}

	[Test, WithPlaywrightPage]
	public async Task PackingTreeViewSelectNodeOnKeyboardNavigation()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		PackingTreeView treeView = null;

		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			treeView = new PackingTreeView() { Size = new Size(500, 300) };
			var factory = new BusinessObjectFactory();
			var data = new TestDataForPacking(factory);
			data.CreatePackingData();

			var pkgPackageJob = data.PackageJob;
			treeView.Nodes.Add(new PackingTreeNode(pkgPackageJob));
			treeView.TopNode.Text = "test 1";

			var pkgPackage = pkgPackageJob.Packages.AddNew("PLT", "PID01");
			var node2 = new PackingTreeNode(pkgPackage);
			node2.Text = "test 2";
			treeView.Nodes.Add(node2);

			return treeView;
		});

		var node1 = page.Locator("ul > li:nth-child(1) .treeview__nodetext");
		Assert.That(treeView.SelectedNode, Is.EqualTo(null));

		await node1.ClickAsync();
		Assert.That(() => treeView.SelectedNode, Is.EqualTo(treeView.Nodes[0]).After(2000, 500));

		await page.Keyboard.PressAsync("ArrowDown");
		Assert.That(() => treeView.SelectedNode, Is.EqualTo(treeView.Nodes[1]).After(2000, 500));

		await page.Keyboard.PressAsync("ArrowUp");
		Assert.That(() => treeView.SelectedNode, Is.EqualTo(treeView.Nodes[0]).After(2000, 500));
	}

	[Test, WithPlaywrightPage]
	public async Task PackingTreeViewNodeTextAndStatusStyles()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new ZForm() { Size = new Size(500, 300) };
			var factory = new BusinessObjectFactory();
			var data = new TestDataForPacking(factory);

			data.CreatePackingData();

			var treeViewForTest = new PackingTreeView() { Size = new Size(500, 300) };
			var pkgPackageJob = data.PackageJob;
			var packagePLT = pkgPackageJob.Packages.AddNew("PLT", "PID01");
			packagePLT.KP_ClosedTimeUtc = ZDateTime.UtcNow;

			treeViewForTest.Nodes.Add(new PackingTreeNode(pkgPackageJob));
			treeViewForTest.TopNode.Text = "PackingTreeNode TestText";
			treeViewForTest.Nodes.Add(new PackingTreeNode(packagePLT));

			form.Controls.Add(treeViewForTest);

			return form;
		});

		await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

		var secondNode = await page.WaitForSelectorAsync("div.treeview.treeview--border-fixed3d > ul > li:nth-child(2) > div");
		var secondNodeTokens = await secondNode.QuerySelectorAllAsync("p");

		Assert.That(secondNodeTokens.Count, Is.EqualTo(8));
		AssertPackingTreeViewStyle(secondNodeTokens[7], "Closed", "5px", "0px", "rgb(128, 128, 128)", "700 12.3333px / 12.3333px Calibri");
	}

	[Test, WithPlaywrightPage]
	public async Task ShowContextMenuOnRightClick()
	{
		var clientServiceProvider = new MockCargoWiseClientSeviceProvider();
		await using var ctx = new InMemoryAppServerTestContext(clientServiceProvider);

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form { Width = 1000, Height = 600 };
			var treeViewForTest = new PackingTreeView() { Size = new Size(500, 300) };

			treeViewForTest.ContextMenuStrip = new ContextMenuStrip();
			treeViewForTest.ContextMenuStrip.Items.Add(new ZToolStripMenuItem { Text = "Menu Item Do" });
			treeViewForTest.ContextMenuStrip.Items.Add(new ZToolStripMenuItem { Text = "Menu Item Re" });

			form.Controls.Add(treeViewForTest);

			return form;
		});

		MenuInteropModel menu = null;
		clientServiceProvider.MockMenuDisplayer.Setup(o => o.SendShowMenuRequestAsync(It.IsAny<MenuInteropModel>(), It.IsAny<Func<SubMenuLoadRequest, Task<MenuItemInteropModel[]>>>(), It.IsAny<Func<MenuClosedResult, Task>>()))
			.Callback<MenuInteropModel, Func<SubMenuLoadRequest, Task<MenuItemInteropModel[]>>, Func<MenuClosedResult, Task>>(
			(model, _, _) => menu = model)
			.Returns(Task.FromResult(MenuShowResultCode.Shown));

		var span = await page.WaitForSelectorAsync(".treeview");
		await span.ClickAsync(new ElementHandleClickOptions { Button = MouseButton.Right });

		await Task.Delay(100);

		clientServiceProvider.MockMenuDisplayer.Verify(
			m => m.SendShowMenuRequestAsync(
				It.IsAny<MenuInteropModel>(),
				It.IsAny<Func<SubMenuLoadRequest, Task<MenuItemInteropModel[]>>>(),
				It.IsAny<Func<MenuClosedResult, Task>>()),
			Times.Once());
		Assert.That(menu.MenuItems[0].Text, Is.EqualTo("Menu Item Do"));
		Assert.That(menu.MenuItems[1].Text, Is.EqualTo("Menu Item Re"));
	}

	[Test, WithPlaywrightPage]
	public async Task PackingTreeViewNodeShouldRefreshWhenTokenIsUpdated()
	{
		PackingTreeView treeViewForTest = null;
		PkgPackage pkgPackage = null;

		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new ZForm() { Size = new Size(500, 300) };
			var factory = new BusinessObjectFactory();
			var data = new TestDataForPacking(factory);

			data.CreatePackingData();

			treeViewForTest = new PackingTreeView() { Size = new Size(500, 300) };
			var pkgPackageJob = data.PackageJob;
			treeViewForTest.Nodes.Add(new PackingTreeNode(pkgPackageJob));
			treeViewForTest.TopNode.Text = "PackingTreeNode TestText";

			pkgPackage = pkgPackageJob.Packages.AddNew("PLT", "PID01");
			var packingTreeNode = new PackingTreeNode(pkgPackage);
			treeViewForTest.Nodes.Add(packingTreeNode);

			form.Controls.Add(treeViewForTest);

			return form;
		});

		var firstNode = await page.WaitForSelectorAsync("div.treeview.treeview--border-fixed3d > ul > li:nth-child(1) > div");
		var secondNode = await page.WaitForSelectorAsync("div.treeview.treeview--border-fixed3d > ul > li:nth-child(2) > div");

		Assert.That(async () => await firstNode.TextContentAsync(), Is.EqualTo("PackingTreeNode TestText(1x PLT, Wgt: 0 KG)"));
		Assert.That(async () => await secondNode.TextContentAsync(), Is.EqualTo("(ID: PID01, Wgt: 0 KG)"));

		await treeViewForTest.InvokeWinzorDispatcherAsync(() => pkgPackage.KP_Weight = 20m);

		firstNode = await page.WaitForSelectorAsync("div.treeview.treeview--border-fixed3d > ul > li:nth-child(1) > div");
		secondNode = await page.WaitForSelectorAsync("div.treeview.treeview--border-fixed3d > ul > li:nth-child(2) > div");

		Assert.That(async () => await firstNode.TextContentAsync(), Is.EqualTo("PackingTreeNode TestText(1x PLT, Wgt: 20 KG)"));
		Assert.That(async () => await secondNode.TextContentAsync(), Is.EqualTo("(ID: PID01, Wgt: 20 KG)"));
	}

	[Test, WithPlaywrightPage]
	public async Task PackingTreeViewNodeTextAndTokensStyles()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new ZForm() { Size = new Size(500, 300) };
			var factory = new BusinessObjectFactory();
			var data = new TestDataForPacking(factory);

			data.CreatePackingData();

			var treeViewForTest = new PackingTreeView() { Size = new Size(500, 300) };
			var pkgPackageJob = data.PackageJob;
			var packagePLT = pkgPackageJob.Packages.AddNew("PLT", "PID01");

			treeViewForTest.Nodes.Add(new PackingTreeNode(pkgPackageJob));
			treeViewForTest.TopNode.Text = "PackingTreeNode TestText";
			treeViewForTest.Nodes.Add(new PackingTreeNode(packagePLT));

			form.Controls.Add(treeViewForTest);

			return form;
		});

		var firstNode = await page.WaitForSelectorAsync("div.treeview.treeview--border-fixed3d > ul > li:nth-child(1) > div");
		var secondNode = await page.WaitForSelectorAsync("div.treeview.treeview--border-fixed3d > ul > li:nth-child(2) > div");
		var firstNodeTokens = await firstNode.QuerySelectorAllAsync("p");
		var secondNodeToekens = await secondNode.QuerySelectorAllAsync("p");

		AssertPackingTreeViewStyle(firstNode, "PackingTreeNode TestText(1x PLT, Wgt: 0 KG)", "0px", "0px", "rgb(0, 0, 0)", "10.6667px / 10.6667px Tahoma");
		Assert.That(firstNodeTokens.Count, Is.EqualTo(6));

		AssertPackingTreeViewStyle(firstNodeTokens[0], "(", "5px", "0px", "rgb(1, 121, 20)", "12px / 12px Calibri");
		AssertPackingTreeViewStyle(firstNodeTokens[1], "1x PLT", "0px", "0px", "rgb(1, 121, 20)", "700 12.3333px / 12.3333px Calibri");
		AssertPackingTreeViewStyle(firstNodeTokens[2], ", ", "0px", "5px", "rgb(1, 121, 20)", "12px / 12px Calibri");
		AssertPackingTreeViewStyle(firstNodeTokens[3], "Wgt: ", "0px", "5px", "rgb(1, 121, 20)", "12px / 12px Calibri");
		AssertPackingTreeViewStyle(firstNodeTokens[4], "0 KG", "0px", "0px", "rgb(1, 121, 20)", "700 12.3333px / 12.3333px Calibri");
		AssertPackingTreeViewStyle(firstNodeTokens[5], ")", "0px", "0px", "rgb(1, 121, 20)", "12px / 12px Calibri");

		AssertPackingTreeViewStyle(secondNode, "(ID: PID01, Wgt: 0 KG)", "0px", "0px", "rgb(0, 0, 0)", "10.6667px / 10.6667px Tahoma");
		Assert.That(secondNodeToekens.Count, Is.EqualTo(7));

		AssertPackingTreeViewStyle(secondNodeToekens[0], "(", "5px", "0px", "rgb(18, 97, 225)", "12px / 12px Calibri");
		AssertPackingTreeViewStyle(secondNodeToekens[1], "ID: ", "0px", "5px", "rgb(18, 97, 225)", "12px / 12px Calibri");
		AssertPackingTreeViewStyle(secondNodeToekens[2], "PID01", "0px", "0px", "rgb(18, 97, 225)", "700 12.3333px / 12.3333px Calibri");
		AssertPackingTreeViewStyle(secondNodeToekens[3], ", ", "0px", "5px", "rgb(18, 97, 225)", "12px / 12px Calibri");
		AssertPackingTreeViewStyle(secondNodeToekens[4], "Wgt: ", "0px", "5px", "rgb(18, 97, 225)", "12px / 12px Calibri");
		AssertPackingTreeViewStyle(secondNodeToekens[5], "0 KG", "0px", "0px", "rgb(18, 97, 225)", "700 12.3333px / 12.3333px Calibri");
		AssertPackingTreeViewStyle(secondNodeToekens[6], ")", "0px", "0px", "rgb(18, 97, 225)", "12px / 12px Calibri");
	}

	[Test, WithPlaywrightPage]
	public async Task ShouldSelectNodeOnMouseDown()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new ZForm() { Size = new Size(500, 300) };

			var treeViewForTest = new PackingTreeView() { Size = new Size(500, 300) };

			var factory = new BusinessObjectFactory();
			var data = new TestDataForPacking(factory);
			data.CreatePackingData();

			var pkgPackageJob = data.PackageJob;
			var packagePLT = pkgPackageJob.Packages.AddNew("PLT", "PID01");

			var packageJob = new PackingTreeNode(pkgPackageJob);
			treeViewForTest.Nodes.Add(packageJob);
			treeViewForTest.TopNode.Text = "TestNode 1";

			var node2 = new PackingTreeNode(packagePLT);
			node2.Text = "TestNode 2";
			treeViewForTest.Nodes.Add(node2);

			form.Controls.Add(treeViewForTest);

			return form;
		});
		var firstNodeContent = await page.WaitForSelectorAsync("div.treeview.treeview--border-fixed3d > ul > li:nth-child(1) > div > .treeview__nodetext");

		//It should highlight the node after clicking on it
		await page.Mouse.DblClickAsync(65, 8);
		await page.Mouse.MoveAsync(65, 8);

		Assert.That(async () => await firstNodeContent.EvaluateAsync<string>($"e => window.getComputedStyle(e).getPropertyValue('color')"), Is.EqualTo("rgb(255, 255, 255)").After(1000, 10));
		Assert.That(async () => await firstNodeContent.EvaluateAsync<string>($"e => window.getComputedStyle(e).getPropertyValue('background')"), Is.EqualTo("rgb(0, 120, 215) none repeat scroll 0% 0% / auto padding-box border-box").After(1000, 10));
	}

	[Test, WithPlaywrightPage]
	public async Task ShouldHighlightLatestWhenAddNewPackage()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var factory = new BusinessObjectFactory();
			DummyBaseBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyWithPacking);

			var data = new TestDataForPacking(factory);
			data.CreatePackingData();

			var control = new PackingUserControl();
			control.SetDataBinding(data.PackageJob, "");

			var form = new ZForm() { Size = new Size(1280, 720) };
			form.Controls.Add(control);
			return form;
		});

		var allNodes = page.Locator(".treeview__node");
		var topNode = page.Locator(".treeview .treeview__node:first-child");
		var topNodeNodeTexts = topNode.Locator(".treeview__nodes .treeview__nodetext");
		await Assertions.Expect(allNodes).ToHaveCountAsync(1);
		await Assertions.Expect(topNodeNodeTexts).ToHaveCountAsync(0);

		await page.GetByTitle("Add").ClickAsync();
		await Assertions.Expect(topNodeNodeTexts).ToHaveCountAsync(1);
		Assert.That(async () => await topNodeNodeTexts.First.GetAttributeAsync("style"), Does.Contain("color:var(--color-highlight-text);background:var(--color-highlight);"));

		await page.GetByTitle("Add").ClickAsync();
		await Assertions.Expect(allNodes).ToHaveCountAsync(3);
		await Assertions.Expect(topNodeNodeTexts).ToHaveCountAsync(2);

		Assert.That(async () => await topNodeNodeTexts.Nth(0).GetAttributeAsync("style"), Does.Not.Contain("color:var(--color-highlight-text);background:var(--color-highlight);"));
		Assert.That(async () => await topNodeNodeTexts.Nth(1).GetAttributeAsync("style"), Does.Contain("color:var(--color-highlight-text);background:var(--color-highlight);"));
	}

	[Test]
	public async Task GetSummaryTokensShouldOnlyHandlePackageJobAndPackage()
	{
		using var ctx = new EnterpriseTestContext();
		PackingTreeNode pkgPackageJobNode = null;
		PackingTreeNode pkgPackageNode = null;
		PackingTreeNode wrapperItemNode = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var treeViewForTest = new PackingTreeView();
			var factory = new BusinessObjectFactory();

			var data = new TestDataForPacking(factory);
			data.CreatePackingData();

			var pkgPackageJob = data.PackageJob;
			pkgPackageJobNode = new PackingTreeNode(pkgPackageJob);
			treeViewForTest.Nodes.Add(pkgPackageJobNode);

			var pkgPackage = pkgPackageJob.Packages.AddNew("PLT", "PID01");
			pkgPackageNode = new PackingTreeNode(pkgPackage);
			treeViewForTest.Nodes.Add(pkgPackageNode);

			var itemDivot = factory.New<PkgPackageItemDivot>();
			var wrapperItem = PkgPackageItemDivotsWrapper.New(itemDivot, data.DummyLine1);
			wrapperItemNode = new PackingTreeNode(wrapperItem);
			treeViewForTest.Nodes.Add(wrapperItemNode);

			return treeViewForTest;
		});

		var nodes = rendered.FindAll(".treeview__node");
		Assert.That(nodes.Count, Is.EqualTo(3));

		Assert.Multiple(() =>
		{
			var pkgPackageJobNodeSummaryTokens = rendered.FindAll(".treeview__node:nth-child(1) p");
			var pkgPackageNodeSummaryTokens = rendered.FindAll(".treeview__node:nth-child(2) p");
			var wrapperItemNodeSummaryTokens = rendered.FindAll(".treeview__node:nth-child(3) p");

			Assert.That(pkgPackageJobNodeSummaryTokens.Count, Is.EqualTo(6));
			Assert.That(pkgPackageNodeSummaryTokens.Count, Is.EqualTo(7));
			Assert.That(wrapperItemNodeSummaryTokens.Count, Is.EqualTo(0));
		});
	}

	void AssertPackingTreeViewStyle(IElementHandle element, string expectedContent, string expectedMarginLeft, string expectedPaddingRight, string expectedColor, string expectedFont)
	{
		Assert.That(async () => await element.TextContentAsync(), Is.EqualTo($"{expectedContent}"));
		Assert.That(async () => await element.EvaluateAsync<string>($"e => window.getComputedStyle(e).getPropertyValue('margin-left')"), Is.EqualTo($"{expectedMarginLeft}"));
		Assert.That(async () => await element.EvaluateAsync<string>($"e => window.getComputedStyle(e).getPropertyValue('padding-right')"), Is.EqualTo($"{expectedPaddingRight}"));
		Assert.That(async () => await element.EvaluateAsync<string>($"e => window.getComputedStyle(e).getPropertyValue('color')"), Is.EqualTo($"{expectedColor}"));
		Assert.That(async () => await element.EvaluateAsync<string>($"e => window.getComputedStyle(e).getPropertyValue('font')"), Is.EqualTo($"{expectedFont}"));
	}
}
