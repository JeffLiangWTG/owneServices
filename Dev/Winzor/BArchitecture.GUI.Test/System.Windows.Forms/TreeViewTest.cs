using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;
using Moq;
using NUnit.Framework;
using WinzorFramework.JSInterop;
using WinzorTestFramework;
using WTG.PlaywrightTesting;

namespace System.Windows.Forms;

using static PlaywrightTestContext;

class TreeViewTest
{
	[Test]
	public async Task HasNodes()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() => TestTreeView());
		Assert.That(rendered.Find(".treeview__node"), Is.Not.Null);
	}

	[Test]
	public async Task NodeHasChildNodes()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() => TestTreeView());
		Assert.That(rendered.Find(".treeview__node .treeview__node .treeview__node"), Is.Not.Null);
	}

	[Test]
	public async Task NodesAreCorrect()
	{
		using var ctx = new WinzorTestContext();
		AssertAllNodesExpanded(await ctx.RenderControlOnFormAsync(() => TestTreeView()));
	}

	[Test]
	public async Task ExpandAllNodes()
	{
		using var ctx = new WinzorTestContext();
		TreeView treeView = null;
		var rendered = await ctx.RenderControlOnFormAsync(() => treeView = TestTreeView(false));
		AssertAllNodesCollapsed(rendered);
		await treeView.InvokeWinzorDispatcherAsync(() => treeView.ExpandAll());
		AssertAllNodesExpanded(rendered);
	}

	[Test]
	public async Task CollapseAllNodes()
	{
		using var ctx = new WinzorTestContext();
		TreeView treeView = null;
		var rendered = await ctx.RenderControlOnFormAsync(() => treeView = TestTreeView());
		AssertAllNodesExpanded(rendered);
		await treeView.InvokeWinzorDispatcherAsync(() => treeView.CollapseAll());
		AssertAllNodesCollapsed(rendered);
	}

	[Test]
	public async Task GetNodeCountIsCorrect()
	{
		using var ctx = new WinzorTestContext();
		TreeView treeView = null;
		var rendered = await ctx.RenderControlOnFormAsync(() => treeView = TestTreeView());
		Assert.That(treeView.GetNodeCount(false), Is.EqualTo(5));
		Assert.That(treeView.GetNodeCount(true), Is.EqualTo(155)); // Node count == 5 + (5 * 5) + (5 * 5 * 5)
	}

	[Test]
	public async Task NodeHasCheckboxes()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var treeView = TestTreeView();
			treeView.CheckBoxes = true;
			return treeView;
		});
		Assert.That(rendered.Find(".treeview > .treeview__nodes > li:first-child > .treeview__nodecontent > input").GetAttribute("type"), Is.EqualTo("checkbox"));
	}

	[Test]
	public async Task NodesGetCheckboxesAfterRender()
	{
		using var ctx = new WinzorTestContext();
		TreeView treeView = null;
		var rendered = await ctx.RenderControlOnFormAsync(() => treeView = TestTreeView());
		await treeView.InvokeWinzorDispatcherAsync(() => treeView.CheckBoxes = true);
		Assert.That(rendered.Find(".treeview > .treeview__nodes > li:first-child > .treeview__nodecontent > input").GetAttribute("type"), Is.EqualTo("checkbox"));
	}

	[TestCase(false, false)]
	[TestCase(true, false)]
	[TestCase(true, true)]
	public async Task TreeNodeButtonDown(bool bExpand, bool bDescendantNodeSelected)
	{
		using var ctx = new WinzorTestContext();
		TreeView treeView = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			treeView = TestTreeView(bExpand);
			return treeView;
		});

		var togglerNodeButton = rendered.Find(".treeview > .treeview__nodes > li:nth-child(2) .treeview__nodebutton");

		Assert.That(treeView.Nodes[1].IsExpanded, Is.EqualTo(bExpand));
		Assert.That(treeView.Nodes[1].IsSelected, Is.False);

		if (bExpand && bDescendantNodeSelected)
		{
			Assert.That(treeView.Nodes[1].Nodes[2].IsSelected, Is.False);
			var togglerChildNodeText = rendered.Find(".treeview > .treeview__nodes > li:nth-child(2) > ul > li:nth-child(3) .treeview__nodetext");
			await togglerChildNodeText.MouseDownAsync(new WebMouseEventArgs());
			Assert.That(treeView.Nodes[1].Nodes[2].IsSelected, Is.True);
		}
		else if (bExpand && !bDescendantNodeSelected)
		{
			Assert.That(treeView.Nodes[2].Nodes[2].IsSelected, Is.False);
			var togglerChildNodeText = rendered.Find(".treeview > .treeview__nodes > li:nth-child(3) > ul > li:nth-child(3) .treeview__nodetext");
			await togglerChildNodeText.MouseDownAsync(new WebMouseEventArgs());
			Assert.That(treeView.Nodes[2].Nodes[2].IsSelected, Is.True);
		}

		bool bShouldSelected = bExpand && bDescendantNodeSelected;

		await togglerNodeButton.MouseDownAsync(new WebMouseEventArgs());
		Assert.That(treeView.Nodes[1].IsExpanded, Is.EqualTo(!bExpand));
		Assert.That(treeView.Nodes[1].IsSelected, Is.EqualTo(bShouldSelected));
	}

	[TestCase]
	public async Task TreeNodeIsDescendantOf()
	{
		using var ctx = new WinzorTestContext();
		TreeView treeView = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			treeView = TestTreeView(false);
			return treeView;
		});

		Assert.That(treeView.Nodes[1].Nodes[2].IsDescendantOf(treeView.Nodes[1]), Is.True);
		Assert.That(treeView.Nodes[1].Nodes[2].IsDescendantOf(treeView.Nodes[2]), Is.False);

		Assert.That(treeView.Nodes[1].Nodes[2].Nodes[3].IsDescendantOf(treeView.Nodes[1]), Is.True);
		Assert.That(treeView.Nodes[1].Nodes[2].Nodes[3].IsDescendantOf(treeView.Nodes[1].Nodes[2]), Is.True);
		Assert.That(treeView.Nodes[1].Nodes[2].Nodes[3].IsDescendantOf(treeView.Nodes[0]), Is.False);
		Assert.That(treeView.Nodes[1].Nodes[2].Nodes[3].IsDescendantOf(treeView.Nodes[1].Nodes[3]), Is.False);
	}

	[Test]
	public async Task TreeNodeButtonDownCollapseSelected()
	{
		using var ctx = new WinzorTestContext();
		var beforeSelect = 0;
		var afterSelect = 0;
		TreeView treeView = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			treeView = TestTreeView(true);
			treeView.BeforeSelect += (s, e) => beforeSelect++;
			treeView.AfterSelect += (s, e) => afterSelect++;
			return treeView;
		});

		// deselect node since first node is selected on tree view focused
		await treeView.InvokeWinzorDispatcherAsync(() =>
		{
			treeView.SelectedNode = null;
		});

		var togglerNodeButton = rendered.Find(".treeview__node > .treeview__nodebutton");

		Assert.That(treeView.Nodes[0].IsExpanded, Is.True);
		Assert.That(beforeSelect, Is.EqualTo(1));
		Assert.That(afterSelect, Is.EqualTo(1));
		Assert.That(treeView.Nodes[0].IsSelected, Is.False);

		await togglerNodeButton.MouseDownAsync(new WebMouseEventArgs());

		//When press down the treeview__node button, the tree view should only be expanded, not be selected.
		Assert.That(treeView.Nodes[0].IsExpanded, Is.False);
		Assert.That(beforeSelect, Is.EqualTo(1));
		Assert.That(afterSelect, Is.EqualTo(1));
		Assert.That(treeView.Nodes[0].IsSelected, Is.False);
	}

	[Test]
	public async Task ChangeNodeShouldChangeElementReference()
	{
		using var ctx = new WinzorTestContext();
		TreeView treeView = null;
		TreeNode node = null;
		_ = await ctx.RenderControlOnFormAsync(() =>
		{
			treeView = new TreeView();
			node = new TreeNode();
			treeView.Nodes.Add(node);
			return treeView;
		});

		var oldNodeReference = node.ElementReference;

		await treeView.InvokeWinzorDispatcherAsync(() =>
		{
			treeView.Nodes.Clear();
			treeView.Nodes.Add(new TreeNode());
		});

		Assert.That(treeView.Nodes[0].ElementReference, Is.Not.EqualTo(default(ElementReference)));
		Assert.That(oldNodeReference, Is.Not.EqualTo(default(ElementReference)));
		Assert.That(oldNodeReference, Is.Not.EqualTo(treeView.Nodes[0].ElementReference));
	}

	[Test, WithPlaywrightPage]
	public async Task NullNodeOrReferenceShouldNotInvokeJs()
	{
		using var ctx = new WinzorTestContext();
		var interop = new Mock<ITreeViewJSInterop>();
		_ = interop.Setup(i => i.CopyTreeNodeContentAsync(It.IsAny<ElementReference>()));
		_ = ctx.Services.AddScoped(_ => interop.Object);
		TreeView treeView = null;

		_ = await ctx.RenderControlOnFormAsync(() =>
		{
			treeView = TestTreeView();
			_ = treeView.Focus();
			return treeView;
		});

		await Page.Keyboard.PressAsync("Control+C");
		interop.Verify(i => i.CopyTreeNodeContentAsync(It.IsAny<ElementReference>()), Times.Never());
	}

	[Test]
	public async Task NodeExpandedEvents()
	{
		using var ctx = new WinzorTestContext();
		var beforeExpand = false;
		var afterExpand = false;
		TreeView treeView = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			treeView = TestTreeView(false);
			treeView.BeforeExpand += (s, e) => beforeExpand = true;
			treeView.AfterExpand += (s, e) => afterExpand = true;
			return treeView;
		});
		var toggler = rendered.Find(".treeview__node > .treeview__nodebutton");
		Assert.That(beforeExpand, Is.False);
		Assert.That(afterExpand, Is.False);
		Assert.That(toggler.TextContent, Is.EqualTo("+"));
		Assert.That(treeView.Nodes[0].IsExpanded, Is.False);
		await toggler.MouseDownAsync(new WebMouseEventArgs());
		Assert.That(beforeExpand, Is.True);
		Assert.That(afterExpand, Is.True);
		Assert.That(treeView.Nodes[0].IsExpanded, Is.True);
		Assert.That(toggler.TextContent, Is.EqualTo("−"));
	}

	[Test]
	public async Task SelectFirstNodeOnFormShowAsync()
	{
		using var ctx = new WinzorTestContext();
		TreeView treeView = null;
		TreeNode node1 = null;
		var rendered = await ctx.RenderControlOnFormAsync(() => {
			treeView = new TreeView();
			node1 = new TreeNode("Node1");
			var node2 = new TreeNode("Node2");
			treeView.Nodes.Add(node1);
			treeView.Nodes.Add(node2);

			Assert.That(treeView.SelectedNode, Is.Null);

			return treeView;
		});

		Assert.That(treeView.SelectedNode, Is.EqualTo(node1));
	}

	[Test]
	public async Task DoNotReselectNodeOnFormShowAsync()
	{
		using var ctx = new WinzorTestContext();
		TreeView treeView = null;
		TreeNode node2 = null;
		var rendered = await ctx.RenderControlOnFormAsync(() => {
			treeView = new TreeView();
			var node1 = new TreeNode("Node1");
			node2 = new TreeNode("Node2");
			treeView.Nodes.Add(node1);
			treeView.Nodes.Add(node2);
			treeView.SelectedNode = node2;

			Assert.That(treeView.SelectedNode, Is.EqualTo(node2));

			return treeView;
		});

		Assert.That(treeView.SelectedNode, Is.EqualTo(node2));
	}

	[Test, WithPlaywrightPage]
	public async Task NodeButtonStyle()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			form.Controls.Add(TestTreeView(false));
			return form;
		});
		await page.WaitForSelectorAsync(".treeview__nodebutton");
		Assert.That(await page.EvaluateAsync<string>("window.getComputedStyle(document.querySelector('.treeview__nodebutton')).getPropertyValue('align-items')"), Is.EqualTo("center"));
		Assert.That(await page.EvaluateAsync<string>("window.getComputedStyle(document.querySelector('.treeview__nodebutton')).getPropertyValue('background-image')"), Is.EqualTo("linear-gradient(rgb(255, 255, 255), rgb(227, 227, 227))"));
	}

	[Test, WithPlaywrightPage]
	public async Task InteropHandlesBadElementReference()
	{
		var treeView = default(TreeView);
		await using var ctx = new InMemoryTestServerContext();
		await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			form.Controls.Add(treeView = new TreeView());
			return form;
		});

		Assert.DoesNotThrowAsync(async () => await treeView.GetJSInterop<ITreeViewJSInterop>().ScrollIntoViewAsync(new ElementReference()));
	}

	[Test, WithPlaywrightPage]
	public async Task BlankClickShouldFocusTreeView()
	{
		await using var ctx = new InMemoryTestServerContext();
		TreeView treeView1 = null;
		TreeView treeView2 = null;

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			var panel1 = new Panel();
			var panel2 = new Panel() { Top = 200 };
			treeView1 = TestTreeView();
			treeView2 = TestTreeView();
			treeView1.SelectedNode = treeView1.Nodes[0];
			treeView2.SelectedNode = treeView2.Nodes[0];
			panel1.Controls.Add(treeView1);
			panel2.Controls.Add(treeView2);
			form.Controls.Add(panel1);
			form.Controls.Add(panel2);

			return form;
		});

		var selectedNodes = await page.QuerySelectorAllAsync(".treeview__node--selected > .treeview__nodecontent > .treeview__nodetext");
		var treeViews = await page.QuerySelectorAllAsync(".treeview");
		var nodeRect1 = await treeViews[0].BoundingBoxAsync();
		var nodeRect2 = await treeViews[1].BoundingBoxAsync();
		await page.Mouse.ClickAsync(nodeRect2.X + 60, nodeRect2.Y + 5);

		Assert.That(() => treeView1.Focused, Is.False.After(3000, 100));
		Assert.That(() => treeView2.Focused, Is.True.After(3000, 100));

		var nodeFontColor1 = await selectedNodes[0].GetComputedStyleAsync("color");
		var nodeFontColor2 = await selectedNodes[1].GetComputedStyleAsync("color");
		var nodeBackgroundColor1 = await selectedNodes[0].GetComputedStyleAsync("background-color");
		var nodeBackgroundColor2 = await selectedNodes[1].GetComputedStyleAsync("background-color");
		Assert.That(nodeFontColor1, Is.EqualTo("rgb(255, 255, 255)"));
		Assert.That(nodeFontColor2, Is.EqualTo("rgb(255, 255, 255)"));
		Assert.That(nodeBackgroundColor1, Is.EqualTo("rgb(211, 211, 211)"));
		Assert.That(nodeBackgroundColor2, Is.EqualTo("rgb(0, 120, 215)"));

		await page.Mouse.ClickAsync(nodeRect1.X + 60, nodeRect1.Y + 5);
		Assert.That(() => treeView1.Focused, Is.True.After(3000, 100));
		Assert.That(() => treeView2.Focused, Is.False.After(3000, 100));

		nodeFontColor1 = await selectedNodes[0].GetComputedStyleAsync("color");
		nodeFontColor2 = await selectedNodes[1].GetComputedStyleAsync("color");
		nodeBackgroundColor1 = await selectedNodes[0].GetComputedStyleAsync("background-color");
		nodeBackgroundColor2 = await selectedNodes[1].GetComputedStyleAsync("background-color");
		Assert.That(nodeFontColor1, Is.EqualTo("rgb(255, 255, 255)"));
		Assert.That(nodeFontColor2, Is.EqualTo("rgb(255, 255, 255)"));
		Assert.That(nodeBackgroundColor1, Is.EqualTo("rgb(0, 120, 215)"));
		Assert.That(nodeBackgroundColor2, Is.EqualTo("rgb(211, 211, 211)"));

		await page.Mouse.ClickAsync(nodeRect2.X + 60, nodeRect2.Y + 5);
		Assert.That(() => treeView1.Focused, Is.False.After(3000, 100));
		Assert.That(() => treeView2.Focused, Is.True.After(3000, 100));

		nodeFontColor1 = await selectedNodes[0].GetComputedStyleAsync("color");
		nodeFontColor2 = await selectedNodes[1].GetComputedStyleAsync("color");
		nodeBackgroundColor1 = await selectedNodes[0].GetComputedStyleAsync("background-color");
		nodeBackgroundColor2 = await selectedNodes[1].GetComputedStyleAsync("background-color");
		Assert.That(nodeFontColor1, Is.EqualTo("rgb(255, 255, 255)"));
		Assert.That(nodeFontColor2, Is.EqualTo("rgb(255, 255, 255)"));
		Assert.That(nodeBackgroundColor1, Is.EqualTo("rgb(211, 211, 211)"));
		Assert.That(nodeBackgroundColor2, Is.EqualTo("rgb(0, 120, 215)"));
	}

	[WithPlaywrightPage]
	[TestCase(".treeview__node--selected .treeview__nodecontent .treeview__nodetext", TestName = "{m}_nodeText")]
	[TestCase(".treeview__node--selected .treeview__nodecontent .treeview__nodeimage", TestName = "{m}_nodeImage")]
	[TestCase(".treeview__node--selected .treeview__nodecontent .treeview__nodestateimage", TestName = "{m}_nodeStateImage")]
	public async Task DoubleClickShouldExpandOrCollapseNode(string clickElement)
	{
		TreeView treeView = null;
		await using var ctx = new InMemoryTestServerContext();
		var collapseTime = 0;
		var expandTime = 0;

		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			treeView = TestTreeView();
			treeView.StateImageList = new ImageList();
			treeView.StateImageList.Images.Add(new Bitmap(1, 1));
			treeView.ImageList = new ImageList();
			treeView.ImageList.Images.Add(new Bitmap(1, 1));
			treeView.AfterCollapse += (sender, args) => collapseTime++;
			treeView.AfterExpand += (sender, args) => expandTime++;
			return treeView;
		});

		var firstNode = treeView.Nodes[0];
		var node = await page.QuerySelectorAsync(clickElement);
		Assert.That(firstNode.IsExpanded, Is.True);

		await node.ClickAsync(new ElementHandleClickOptions() { ClickCount = 6 });
		Assert.That(() => collapseTime, Is.EqualTo(2).After(3000,100));
		Assert.That(() => expandTime, Is.EqualTo(1).After(3000, 100));
		Assert.That(firstNode.IsExpanded, Is.False);

		await node.ClickAsync(new ElementHandleClickOptions() { ClickCount = 6 });
		Assert.That(() => collapseTime, Is.EqualTo(3).After(3000, 100));
		Assert.That(() => expandTime, Is.EqualTo(3).After(3000, 100));
		Assert.That(firstNode.IsExpanded, Is.True);
	}

	[Test]
	public async Task TreeVeiwDraggable([Values] bool allowDrop)
	{
		TreeView treeView = null;
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			treeView = TestTreeView(true, xSize: 2, ySize: 2, zSize: 0);
			treeView.AllowDrop = allowDrop;
			return treeView;
		});

		var li = rendered.FindAll("li");
		Assert.That(li[0].Attributes["draggable"].Value, Is.EqualTo(allowDrop ? "true" : "false"));
	}

	[Test, WithPlaywrightPage]
	public async Task TreeVeiwDragDrop()
	{
		TreeView treeView = null;
		bool dragDrop = false;
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			treeView = TestTreeView(true, xSize: 2, ySize: 2, zSize: 0);
			treeView.AllowDrop = true;
			treeView.DragDrop += (s, e) => dragDrop = true;
			return treeView;
		});

		await page.Locator("li .treeview__node").Nth(0).WaitForAsync();
		var nodes = await page.QuerySelectorAllAsync("li .treeview__node");
		var nodeRect1 = await nodes[0].BoundingBoxAsync();
		var nodeRect2 = await nodes[1].BoundingBoxAsync();

		await page.Mouse.ClickAsync(nodeRect1.X + 10, nodeRect1.Y + 1);
		await page.Mouse.MoveAsync(nodeRect1.X + 10, nodeRect1.Y + 1);
		await page.Mouse.DownAsync();
		await page.Mouse.MoveAsync(nodeRect2.X + 10, nodeRect2.Y + 1);
		await page.Mouse.UpAsync();

		Assert.That(() => dragDrop, Is.EqualTo(true).After(1000,100));
	}

	[Test, WithPlaywrightPage]
	public async Task TestCopyTreeNodeContent()
	{
		TreeView treeView = null;
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			treeView = new TreeView();
			treeView.Nodes.Add(new TreeNode("TestNode"));
			return treeView;
		});
		await page.AttachMockClipboardWrite();
		var firstNode = page.Locator(".treeview__node--selected .treeview__nodetext");
		await firstNode.ClickAsync();
		await page.Keyboard.PressAsync("Control+C");
		Assert.That(async () => await page.EvaluateAsync<string>("e => navigator.clipboard.writtenText"), Is.EqualTo("TestNode").After(3000, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task ExpandNodeDoesNotLoseFocus()
	{
		TreeView treeView = null;
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			treeView = TestTreeView();
			return treeView;
		});

		var firstNode = await page.QuerySelectorAsync(".treeview__node--selected .treeview__nodetext");
		var button = await page.QuerySelectorAsync(".treeview__node--selected > button");
		Assert.That(treeView.Nodes[0].IsExpanded, Is.True);

		await firstNode.ClickAsync();
		Assert.That(() => firstNode.GetAttributeAsync("style"), Is.EqualTo("color:var(--color-highlight-text);background:var(--color-highlight);").After(3000, 100));

		await button.ClickAsync();
		Assert.That(() => treeView.Nodes[0].IsExpanded, Is.False.After(3000, 100));
		Assert.That(() => firstNode.GetAttributeAsync("style"), Is.EqualTo("color:var(--color-highlight-text);background:var(--color-highlight);").After(3000, 100));

		await button.ClickAsync();
		Assert.That(() => treeView.Nodes[0].IsExpanded, Is.True.After(3000, 100));
		Assert.That(() => firstNode.GetAttributeAsync("style"), Is.EqualTo("color:var(--color-highlight-text);background:var(--color-highlight);").After(3000, 100));
	}

	[Test]
	public async Task PreloadTreeViewJSInterop()
	{
		using var ctx = new WinzorTestContext();
		var services = ctx.MockCargoWiseClientServices;
		var interop = new Mock<ITreeViewJSInterop>();
		interop.Setup(i => i.PreloadInterop());
		ctx.Services.AddScoped(_ => interop.Object);

		var rendered = await ctx.RenderControlOnFormAsync(() => new TreeView());

		interop.Verify(e => e.PreloadInterop(), Times.Once());
	}

	[Test]
	public async Task NodeExpandedEventCancelled()
	{
		using var ctx = new WinzorTestContext();
		var beforeExpand = false;
		var afterExpand = false;
		TreeView treeView = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			treeView = TestTreeView(false);
			treeView.BeforeExpand += (s, e) =>
			{
				beforeExpand = true;
				e.Cancel = true;
			};
			treeView.AfterExpand += (s, e) => afterExpand = true;
			return treeView;
		});
		var toggler = rendered.Find(".treeview__node > .treeview__nodebutton");
		Assert.That(beforeExpand, Is.False);
		Assert.That(afterExpand, Is.False);
		Assert.That(toggler.TextContent, Is.EqualTo("+"));
		Assert.That(treeView.Nodes[0].IsExpanded, Is.False);
		await toggler.MouseDownAsync(new WebMouseEventArgs());
		Assert.That(beforeExpand, Is.True);
		Assert.That(afterExpand, Is.False);
		Assert.That(treeView.Nodes[0].IsExpanded, Is.False);
		Assert.That(toggler.TextContent, Is.EqualTo("+"));
	}

	[Test]
	public async Task NodeCollapsedEvents()
	{
		using var ctx = new WinzorTestContext();
		var beforeCollapse = false;
		var afterCollapse = false;
		TreeView treeView = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			treeView = TestTreeView(true);
			treeView.BeforeCollapse += (s, e) => beforeCollapse = true;
			treeView.AfterCollapse += (s, e) => afterCollapse = true;
			return treeView;
		});
		var toggler = rendered.Find(".treeview__node > .treeview__nodebutton");
		Assert.That(beforeCollapse, Is.False);
		Assert.That(afterCollapse, Is.False);
		Assert.That(treeView.Nodes[0].IsExpanded, Is.True);
		Assert.That(toggler.TextContent, Is.EqualTo("−"));
		await toggler.MouseDownAsync(new WebMouseEventArgs());
		Assert.That(beforeCollapse, Is.True);
		Assert.That(afterCollapse, Is.True);
		Assert.That(treeView.Nodes[0].IsExpanded, Is.False);
		Assert.That(toggler.TextContent, Is.EqualTo("+"));
	}

	[Test]
	public async Task NodeCollapsedEventCancelled()
	{
		using var ctx = new WinzorTestContext();
		var beforeCollapse = false;
		var afterCollapse = false;
		TreeView treeView = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			treeView = TestTreeView(true);
			treeView.BeforeCollapse += (s, e) =>
			{
				beforeCollapse = true;
				e.Cancel = true;
			};
			treeView.AfterCollapse += (s, e) => afterCollapse = true;
			return treeView;
		});
		var toggler = rendered.Find(".treeview__node > .treeview__nodebutton");
		Assert.That(beforeCollapse, Is.False);
		Assert.That(afterCollapse, Is.False);
		Assert.That(treeView.Nodes[0].IsExpanded, Is.True);
		Assert.That(toggler.TextContent, Is.EqualTo("−"));
		await toggler.MouseDownAsync(new WebMouseEventArgs());
		Assert.That(beforeCollapse, Is.True);
		Assert.That(afterCollapse, Is.False);
		Assert.That(treeView.Nodes[0].IsExpanded, Is.True);
		Assert.That(toggler.TextContent, Is.EqualTo("−"));
	}

	[Test]
	public async Task NodeSelectedEvents()
	{
		using var ctx = new WinzorTestContext();
		var beforeSelect = 0;
		var afterSelect = 0;
		TreeView treeView = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			treeView = TestTreeView(false);
			treeView.BeforeSelect += (s, e) => beforeSelect++;
			treeView.AfterSelect += (s, e) => afterSelect++;
			return treeView;
		});

		// first node should be selected on treeView focused
		Assert.That(beforeSelect, Is.EqualTo(1));
		Assert.That(afterSelect, Is.EqualTo(1));
		Assert.That(treeView.SelectedNode, Is.EqualTo(treeView.Nodes[0]));
		Assert.That(treeView.Nodes[0].IsSelected, Is.True);

		// deselect node since first node is selected on tree view focused
		await treeView.InvokeWinzorDispatcherAsync(() =>
		{
			treeView.SelectedNode = null;
		});

		Assert.That(beforeSelect, Is.EqualTo(1));
		Assert.That(afterSelect, Is.EqualTo(1));
		Assert.That(treeView.SelectedNode, Is.Null);
		Assert.That(treeView.Nodes[0].IsSelected, Is.False);
		Assert.That(rendered.FindAll("treeview__node--selected").Count, Is.EqualTo(0));
		await rendered.Find(".treeview__node > .treeview__nodecontent > .treeview__nodetext").MouseDownAsync(new WebMouseEventArgs());
		Assert.That(beforeSelect, Is.EqualTo(2));
		Assert.That(afterSelect, Is.EqualTo(2));
		Assert.That(treeView.SelectedNode, Is.EqualTo(treeView.Nodes[0]));
		Assert.That(treeView.Nodes[0].IsSelected, Is.True);
	}

	[Test]
	public async Task NodeSelectedEventCancelled()
	{
		using var ctx = new WinzorTestContext();
		var beforeSelect = 0;
		var afterSelect = 0;
		TreeView treeView = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			treeView = TestTreeView(false);
			treeView.BeforeSelect += (s, e) =>
			{
				beforeSelect++;
				e.Cancel = true;
			};
			treeView.AfterSelect += (s, e) => afterSelect++;
			return treeView;
		});

		// deselect node since first node is selected on tree view focused
		await treeView.InvokeWinzorDispatcherAsync(() =>
		{
			treeView.SelectedNode = null;
		});

		Assert.That(beforeSelect, Is.EqualTo(1));
		Assert.That(afterSelect, Is.EqualTo(0));
		Assert.That(treeView.SelectedNode, Is.Null);
		Assert.That(rendered.FindAll("treeview__node--selected").Count, Is.EqualTo(0));
		await rendered.Find(".treeview__node > .treeview__nodecontent > .treeview__nodetext").MouseDownAsync(new WebMouseEventArgs());
		Assert.That(beforeSelect, Is.EqualTo(2));
		Assert.That(afterSelect, Is.EqualTo(0));
		Assert.That(treeView.SelectedNode, Is.EqualTo(null));
		Assert.That(rendered.FindAll("treeview__node--selected").Count, Is.EqualTo(0));
	}

	[Test]
	public async Task TreeRedrawsIfCollectionChanges()
	{
		using var ctx = new WinzorTestContext();
		TreeView treeView = null;
		var rendered = await ctx.RenderControlOnFormAsync(() => treeView = TestTreeView());
		AssertAllNodesExpanded(rendered);
		await treeView.InvokeWinzorDispatcherAsync(() =>
		{
			treeView.Nodes.Add("6");
			treeView.Nodes[2].Nodes[1].Nodes.Add(new TreeNode("2 - 1 - 6"));
		});
		Assert.That(rendered.Find(".treeview > .treeview__nodes > li:nth-child(6) .treeview__nodetext").InnerHtml, Is.EqualTo("6"));
		Assert.That(rendered.Find(".treeview > .treeview__nodes > li:nth-child(3) > ul > li:nth-child(2) > ul > li:nth-child(6) .treeview__nodetext").InnerHtml, Is.EqualTo("2 - 1 - 6"));
	}

	[Test]
	public async Task TreeRedrawsIfNodeTextChanges()
	{
		using var ctx = new WinzorTestContext();
		TreeView treeView = null;
		var rendered = await ctx.RenderControlOnFormAsync(() => treeView = TestTreeView());
		AssertAllNodesExpanded(rendered);
		await treeView.InvokeWinzorDispatcherAsync(() =>
		{
			treeView.Nodes[1].Text = "1 - Changed";
			treeView.Nodes[2].Nodes[1].Nodes[3].Text = "2 - 1 - 3 - Changed";
		});
		Assert.That(rendered.Find(".treeview > .treeview__nodes > li:nth-child(2) .treeview__nodetext").InnerHtml, Is.EqualTo("1 - Changed"));
		Assert.That(rendered.Find(".treeview > .treeview__nodes > li:nth-child(3) > ul > li:nth-child(2) > ul > li:nth-child(4) .treeview__nodetext").InnerHtml, Is.EqualTo("2 - 1 - 3 - Changed"));
	}

	[Test]
	public async Task ImplementsBeforeExpandEvent()
	{
		await ControlAssert.ImplementsEventAsync<TreeView, TreeViewCancelEventHandler>(nameof(TreeView.BeforeExpand), c => TestTreeView(false, c), a => new TreeViewCancelEventHandler((o, e) => a()), ".treeview__nodebutton", e => e.MouseDown());
	}

	[Test]
	public async Task ImplementsAfterExpandEvent()
	{
		await ControlAssert.ImplementsEventAsync<TreeView, TreeViewEventHandler>(nameof(TreeView.AfterExpand), c => TestTreeView(false, c), a => new TreeViewEventHandler((o, e) => a()), ".treeview__nodebutton", e => e.MouseDown());
	}

	[Test]
	public async Task ImplementsBeforeCollapseEvent()
	{
		await ControlAssert.ImplementsEventAsync<TreeView, TreeViewCancelEventHandler>(nameof(TreeView.BeforeCollapse), c => TestTreeView(true, c), a => new TreeViewCancelEventHandler((o, e) => a()), ".treeview__nodebutton", e => e.MouseDown());
	}

	[Test]
	public async Task ImplementsAfterCollapseEvent()
	{
		await ControlAssert.ImplementsEventAsync<TreeView, TreeViewEventHandler>(nameof(TreeView.AfterCollapse), c => TestTreeView(true, c), a => new TreeViewEventHandler((o, e) => a()), ".treeview__nodebutton", e => e.MouseDown());
	}

	[Test]
	public async Task ImplementsBeforeSelectEvent()
	{
		await ControlAssert.ImplementsEventAsync<TreeView, TreeViewCancelEventHandler>(nameof(TreeView.BeforeSelect), c => TestTreeView(true, c), a => new TreeViewCancelEventHandler((o, e) => a()), ".treeview__nodetext", e => e.MouseDown());
	}

	[Test]
	public async Task ImplementsAfterSelectEvent()
	{
		await ControlAssert.ImplementsEventAsync<TreeView, TreeViewEventHandler>(nameof(TreeView.AfterSelect), c => TestTreeView(true, c), a => new TreeViewEventHandler((o, e) => a()), ".treeview__nodetext", e => e.MouseDown());
	}

	[TestCase(false, new[] { "1tw", "42h", "ard", "fsd", "xwr", "zue" })]
	[TestCase(true, new[] { "zue", "xwr", "fsd", "ard", "42h", "1tw" })]
	public async Task TreeNodesSort(bool useCustomizedComparer, string[] expectedSortedItems)
	{
		var randItems = new[] { "42h", "ard", "1tw", "zue", "xwr", "fsd" };

		using var ctx = new WinzorTestContext();
		TreeView treeView = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			treeView = new TreeView();
			if (useCustomizedComparer)
			{
				treeView.TreeViewNodeSorter = new TestComparer();
			}

			foreach (var x in randItems)
			{
				var nodeX = treeView.Nodes.Add(x);
				foreach (var y in randItems)
				{
					var nodeY = nodeX.Nodes.Add(y);
					foreach (var z in randItems)
					{
						nodeY.Nodes.Add(z);
					}
				}
			}
			treeView.ExpandAll();
			return treeView;
		});

		AssertArrayOfNodeItems(rendered, randItems);

		await treeView.InvokeWinzorDispatcherAsync(() => treeView.Sort());
		AssertArrayOfNodeItems(rendered, expectedSortedItems);
	}

	[Test]
	public async Task TreeNodesSortWhenAdd([Values] bool useCustomizedComparer)
	{
		using var ctx = new WinzorTestContext();
		TreeView treeView = null;
		await ctx.RenderControlOnFormAsync(() =>
		{
			var node0 = new TreeNode("cw2");
			var node1 = new TreeNode("ujl");
			var node2 = new TreeNode("evt");
			var node11 = new TreeNode("dwe");
			var node12 = new TreeNode("klp");
			node0.Nodes.Add(node1);
			node0.Nodes.Add(node2);
			node1.Nodes.Add(node11);
			node1.Nodes.Add(node12);

			treeView = new TreeView();
			treeView.Sorted = true;
			if (useCustomizedComparer)
			{
				treeView.TreeViewNodeSorter = new TestComparer();
			}
			treeView.Nodes.Add(node0);
			return treeView;
		});

		if (useCustomizedComparer)
		{
			Assert.That(treeView.Nodes[0].Text, Is.EqualTo("cw2"));
			Assert.That(treeView.Nodes[0].Nodes[0].Text, Is.EqualTo("ujl"));
			Assert.That(treeView.Nodes[0].Nodes[0].Nodes[0].Text, Is.EqualTo("klp"));
			Assert.That(treeView.Nodes[0].Nodes[0].Nodes[1].Text, Is.EqualTo("dwe"));
			Assert.That(treeView.Nodes[0].Nodes[1].Text, Is.EqualTo("evt"));
		}
		else
		{
			Assert.That(treeView.Nodes[0].Text, Is.EqualTo("cw2"));
			Assert.That(treeView.Nodes[0].Nodes[0].Text, Is.EqualTo("evt"));
			Assert.That(treeView.Nodes[0].Nodes[1].Text, Is.EqualTo("ujl"));
			Assert.That(treeView.Nodes[0].Nodes[1].Nodes[0].Text, Is.EqualTo("dwe"));
			Assert.That(treeView.Nodes[0].Nodes[1].Nodes[1].Text, Is.EqualTo("klp"));
		}
	}

	[Test]
	public async Task TreeNodesSortWhenAddSameValue()
	{
		using var ctx = new WinzorTestContext();
		TreeView treeView = null;
		await ctx.RenderControlOnFormAsync(() =>
		{
			treeView = new TreeView();
			treeView.Sorted = true;
			treeView.TreeViewNodeSorter = new TestComparer();
			return treeView;
		});

		var node1 = new TreeNode("bbc");
		var node2 = new TreeNode("abc");
		var node3 = new TreeNode("abc");

		await treeView.InvokeWinzorDispatcherAsync(() => treeView.Nodes.Add(node1));
		Assert.That(treeView.Nodes[0], Is.EqualTo(node1));

		await treeView.InvokeWinzorDispatcherAsync(() => treeView.Nodes.Add(node2));
		Assert.That(treeView.Nodes[0], Is.EqualTo(node1));
		Assert.That(treeView.Nodes[1], Is.EqualTo(node2));

		await treeView.InvokeWinzorDispatcherAsync(() => treeView.Nodes.Add(node3));
		Assert.That(treeView.Nodes[0], Is.EqualTo(node1));
		Assert.That(treeView.Nodes[1], Is.EqualTo(node2));
		Assert.That(treeView.Nodes[2], Is.EqualTo(node3));
	}

	[Test]
	public async Task TreeNodesSortedWhenCallSort()
	{
		using var ctx = new WinzorTestContext();
		TreeView treeView = null;
		await ctx.RenderControlOnFormAsync(() =>
		{
			treeView = new TreeView();
			treeView.TreeViewNodeSorter = new TestComparer();
			return treeView;
		});

		var node1 = new TreeNode("abc");
		var node2 = new TreeNode("bbc");
		var node3 = new TreeNode("abc");

		await treeView.InvokeWinzorDispatcherAsync(() => treeView.Nodes.Add(node1));
		await treeView.InvokeWinzorDispatcherAsync(() => treeView.Nodes.Add(node2));
		await treeView.InvokeWinzorDispatcherAsync(() => treeView.Nodes.Add(node3));

		Assert.That(treeView.Nodes[0], Is.EqualTo(node1));
		Assert.That(treeView.Nodes[1], Is.EqualTo(node2));
		Assert.That(treeView.Nodes[2], Is.EqualTo(node3));

		await treeView.InvokeWinzorDispatcherAsync(() => treeView.Sort());

		Assert.That(treeView.Nodes[0], Is.EqualTo(node2));
		Assert.That(treeView.Nodes[1], Is.EqualTo(node3));
		Assert.That(treeView.Nodes[2], Is.EqualTo(node1));
	}

	TreeView TestTreeView(bool expanded = true, TreeView treeView = null, int xSize = 5, int ySize = 5, int zSize = 5)
	{
		treeView = treeView ?? new TreeView();

		for (int x = 0; x < xSize; x++)
		{
			var nodeX = treeView.Nodes.Add($"{x}");
			for (int y = 0; y < ySize; y++)
			{
				var nodeY = nodeX.Nodes.Add($"{x} - {y}");
				for (int z = 0; z < zSize; z++)
				{
					nodeY.Nodes.Add($"{x} - {y} - {z}");
				}
			}

			if (expanded)
			{
				nodeX.ExpandAll();
			}
		}

		return treeView;
	}

	[Test]
	public async Task TreeViewSelectNodeSetsSelectedNode()
	{
		using var ctx = new WinzorTestContext();
		TreeView treeView = null;
		var rendered = await ctx.RenderControlOnFormAsync(() => treeView = TestTreeView(false));
		await rendered.Find(".treeview__node > .treeview__nodecontent > .treeview__nodetext").MouseDownAsync(new WebMouseEventArgs());
		Assert.That(treeView.SelectedNode, Is.EqualTo(treeView.Nodes[0]));
	}

	void AssertAllNodesExpanded(IRenderedFragment rendered)
	{
		for (int x = 0; x < 5; x++)
		{
			Assert.That(rendered.Find($".treeview > .treeview__nodes > li:nth-child({x + 1}) > .treeview__nodecontent > .treeview__nodetext").InnerHtml, Is.EqualTo($"{x}"));
			for (int y = 0; y < 5; y++)
			{
				Assert.That(rendered.Find($".treeview > .treeview__nodes > li:nth-child({x + 1}) > ul > li:nth-child({y + 1}) > .treeview__nodecontent > .treeview__nodetext").InnerHtml, Is.EqualTo($"{x} - {y}"));
				for (int z = 0; z < 5; z++)
				{
					Assert.That(rendered.Find($".treeview > .treeview__nodes > li:nth-child({x + 1}) > ul > li:nth-child({y + 1}) > ul > li:nth-child({z + 1}) > .treeview__nodecontent > .treeview__nodetext").InnerHtml, Is.EqualTo($"{x} - {y} - {z}"));
				}
			}
		}
	}

	void AssertAllNodesCollapsed(IRenderedFragment rendered)
	{
		for (int x = 0; x < 5; x++)
		{
			Assert.That(rendered.Find($".treeview > .treeview__nodes > li:nth-child({x + 1}) > .treeview__nodecontent > .treeview__nodetext").InnerHtml, Is.EqualTo($"{x}"));
			Assert.That(rendered.FindAll($".treeview > .treeview__nodes > li:nth-child({x + 1}) > ul").Count, Is.EqualTo(0));
		}
	}

	void AssertArrayOfNodeItems(IRenderedFragment rendered, string[] items)
	{
		for (int x = 0; x < items.Length; x++)
		{
			Assert.That(rendered.Find($".treeview > .treeview__nodes > li:nth-child({x + 1}) > .treeview__nodecontent > .treeview__nodetext").InnerHtml, Is.EqualTo($"{items[x]}"));
			for (int y = 0; y < items.Length; y++)
			{
				Assert.That(rendered.Find($".treeview > .treeview__nodes > li:nth-child({x + 1}) > ul > li:nth-child({y + 1}) > .treeview__nodecontent > .treeview__nodetext").InnerHtml, Is.EqualTo($"{items[y]}"));
				for (int z = 0; z < items.Length; z++)
				{
					Assert.That(rendered.Find($".treeview > .treeview__nodes > li:nth-child({x + 1}) > ul > li:nth-child({y + 1}) > ul > li:nth-child({z + 1}) > .treeview__nodecontent > .treeview__nodetext").InnerHtml, Is.EqualTo($"{items[z]}"));
				}
			}
		}
	}

	[Explicit]
	[Test, WithPlaywrightPage(Headless = false)]
	public async Task TreeViewDemo()
	{
		await using var ctx = new InMemoryTestServerContext();

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			var treeView = TestTreeView();
			treeView.Height = 300;
			treeView.Width = 200;
			form.Controls.Add(treeView);
			return form;
		});

		await page.WaitForTimeoutAsync(120000);
	}

	[Test]
	public async Task DefaultBackColor()
	{
		using var ctx = new WinzorTestContext();
		TreeView treeView = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			treeView = new TreeView();
			return treeView;
		});

		Assert.That(treeView.BackColor, Is.EqualTo(SystemColors.Window));
		Assert.That(rendered.Find(".treeview").GetAttribute("style"), Does.Contain("background-color:var(--color-window)"));
	}

	[Test]
	public async Task CustomBackColor()
	{
		using var ctx = new WinzorTestContext();
		TreeView treeView = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			treeView = new TreeView();
			treeView.BackColor = Color.Blue;
			return treeView;
		});

		Assert.That(treeView.BackColor, Is.EqualTo(Color.Blue));
		Assert.That(rendered.Find(".treeview").GetAttribute("style"), Does.Contain($"background-color:#0000FFFF"));
	}

	[Test]
	public async Task TreeNodeForeColor()
	{
		Form form = null;
		TreeView treeView = null;
		Button anotherControl = null;
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			form = new Form() { Width = 400, Height = 400 };

			treeView = new TreeView() { Dock = DockStyle.Left, Width = 400 };
			anotherControl = new Button() { Dock = DockStyle.Right, Width = 100 };

			treeView.BackColor = Color.Blue;
			treeView.ForeColor = Color.Red;
			treeView.Font = new Font("Consolas", 8);

			treeView.Nodes.Add(new TreeNode("Test Item 1"));
			treeView.Nodes.Add(new TreeNode("Test Item 2") { ForeColor = Color.Green, BackColor = Color.Purple });
			treeView.Nodes.Add(new TreeNode("Test Item 3") { NodeFont = new Font("Segoe UI", 15, FontStyle.Italic) });
			treeView.Nodes.Add(new TreeNode("Test Item 4") { NodeFont = new Font("Consolas", 15, FontStyle.Italic), ForeColor = Color.Green });
			treeView.Nodes.Add(new TreeNode("Test Item 5") { NodeFont = new Font("Segoe UI", 16, FontStyle.Bold) });
			treeView.Nodes.Add(new TreeNode("Test Item 6") { NodeFont = new Font("Consolas", 16, FontStyle.Bold), ForeColor = Color.Green });
			treeView.Nodes.Add(new TreeNode("Test Item 7") { NodeFont = new Font("Segoe UI", 17, FontStyle.Italic | FontStyle.Bold) });
			treeView.Nodes.Add(new TreeNode("Test Item 8") { NodeFont = new Font("Consolas", 17, FontStyle.Italic | FontStyle.Bold), ForeColor = Color.Green });
			treeView.Nodes.Add(new TreeNode("Test Item 9") { NodeFont = new Font("Segoe UI", 8, FontStyle.Italic | FontStyle.Bold) });
			treeView.Nodes.Add(new TreeNode("Test Item Selected") { NodeFont = new Font("Segoe UI", 18, FontStyle.Italic | FontStyle.Bold), ForeColor = Color.Red });

			form.Controls.Add(treeView);
			form.Controls.Add(anotherControl);

			treeView.SelectedNode = treeView.Nodes.Last();
			treeView.Focus();

			return form;
		});

		var treeNodes = rendered.FindAll(".treeview li");

		Assert.That(treeNodes, Has.Count.EqualTo(10));
		Assert.That(treeNodes[0].QuerySelector("div span").TextContent, Is.EqualTo("Test Item 1"));
		Assert.That(treeNodes[0].QuerySelector("div span").GetAttribute("style"), Is.EqualTo(string.Empty));
		Assert.That(treeNodes[1].QuerySelector("div span").TextContent, Is.EqualTo("Test Item 2"));
		Assert.That(treeNodes[1].QuerySelector("div span").GetAttribute("style"), Is.EqualTo("color:#008000FF;background:#800080FF;"));
		Assert.That(treeNodes[2].QuerySelector("div span").TextContent, Is.EqualTo("Test Item 3"));
		Assert.That(treeNodes[2].QuerySelector("div span").GetAttribute("style"), Is.EqualTo("font-style:italic;font-family:Segoe UI;font-size:15pt;"));
		Assert.That(treeNodes[3].QuerySelector("div span").TextContent, Is.EqualTo("Test Item 4"));
		Assert.That(treeNodes[3].QuerySelector("div span").GetAttribute("style"), Is.EqualTo("color:#008000FF;font-style:italic;font-size:15pt;"));
		Assert.That(treeNodes[4].QuerySelector("div span").TextContent, Is.EqualTo("Test Item 5"));
		Assert.That(treeNodes[4].QuerySelector("div span").GetAttribute("style"), Is.EqualTo("font-weight:bold;font-family:Segoe UI;font-size:16pt;"));
		Assert.That(treeNodes[5].QuerySelector("div span").TextContent, Is.EqualTo("Test Item 6"));
		Assert.That(treeNodes[5].QuerySelector("div span").GetAttribute("style"), Is.EqualTo("color:#008000FF;font-weight:bold;font-size:16pt;"));
		Assert.That(treeNodes[6].QuerySelector("div span").TextContent, Is.EqualTo("Test Item 7"));
		Assert.That(treeNodes[6].QuerySelector("div span").GetAttribute("style"), Is.EqualTo("font-weight:bold;font-style:italic;font-family:Segoe UI;font-size:17pt;"));
		Assert.That(treeNodes[7].QuerySelector("div span").TextContent, Is.EqualTo("Test Item 8"));
		Assert.That(treeNodes[7].QuerySelector("div span").GetAttribute("style"), Is.EqualTo("color:#008000FF;font-weight:bold;font-style:italic;font-size:17pt;"));
		Assert.That(treeNodes[8].QuerySelector("div span").TextContent, Is.EqualTo("Test Item 9"));
		Assert.That(treeNodes[8].QuerySelector("div span").GetAttribute("style"), Is.EqualTo("font-weight:bold;font-style:italic;font-family:Segoe UI;"));
		Assert.That(treeNodes[9].QuerySelector("div span").TextContent, Is.EqualTo("Test Item Selected"));
		Assert.That(treeNodes[9].QuerySelector("div span").GetAttribute("style"), Is.EqualTo("color:var(--color-highlight-text);background:var(--color-highlight);font-weight:bold;font-style:italic;font-family:Segoe UI;font-size:18pt;"));

		await form.InvokeWinzorDispatcherAsync(() => { treeView.Font = new Font("Segoe UI", 10, FontStyle.Bold | FontStyle.Italic); });
		treeNodes = rendered.FindAll(".treeview li");

		Assert.That(treeNodes[0].QuerySelector("div span").TextContent, Is.EqualTo("Test Item 1"));
		Assert.That(treeNodes[0].QuerySelector("div span").GetAttribute("style"), Is.EqualTo(string.Empty));
		Assert.That(treeNodes[1].QuerySelector("div span").TextContent, Is.EqualTo("Test Item 2"));
		Assert.That(treeNodes[1].QuerySelector("div span").GetAttribute("style"), Is.EqualTo("color:#008000FF;background:#800080FF;"));
		Assert.That(treeNodes[2].QuerySelector("div span").TextContent, Is.EqualTo("Test Item 3"));
		Assert.That(treeNodes[2].QuerySelector("div span").GetAttribute("style"), Is.EqualTo("font-size:15pt;"));
		Assert.That(treeNodes[3].QuerySelector("div span").TextContent, Is.EqualTo("Test Item 4"));
		Assert.That(treeNodes[3].QuerySelector("div span").GetAttribute("style"), Is.EqualTo("color:#008000FF;font-family:Consolas;font-size:15pt;"));
		Assert.That(treeNodes[4].QuerySelector("div span").TextContent, Is.EqualTo("Test Item 5"));
		Assert.That(treeNodes[4].QuerySelector("div span").GetAttribute("style"), Is.EqualTo("font-size:16pt;"));
		Assert.That(treeNodes[5].QuerySelector("div span").TextContent, Is.EqualTo("Test Item 6"));
		Assert.That(treeNodes[5].QuerySelector("div span").GetAttribute("style"), Is.EqualTo("color:#008000FF;font-family:Consolas;font-size:16pt;"));
		Assert.That(treeNodes[6].QuerySelector("div span").TextContent, Is.EqualTo("Test Item 7"));
		Assert.That(treeNodes[6].QuerySelector("div span").GetAttribute("style"), Is.EqualTo("font-size:17pt;"));
		Assert.That(treeNodes[7].QuerySelector("div span").TextContent, Is.EqualTo("Test Item 8"));
		Assert.That(treeNodes[7].QuerySelector("div span").GetAttribute("style"), Is.EqualTo("color:#008000FF;font-family:Consolas;font-size:17pt;"));
		Assert.That(treeNodes[8].QuerySelector("div span").TextContent, Is.EqualTo("Test Item 9"));
		Assert.That(treeNodes[8].QuerySelector("div span").GetAttribute("style"), Is.EqualTo("font-size:8pt;"));
		Assert.That(treeNodes[9].QuerySelector("div span").TextContent, Is.EqualTo("Test Item Selected"));
		Assert.That(treeNodes[9].QuerySelector("div span").GetAttribute("style"), Is.EqualTo("color:var(--color-highlight-text);background:var(--color-highlight);font-size:18pt;"));

		await form.InvokeWinzorDispatcherAsync(() => anotherControl.Focus());
		treeNodes = rendered.FindAll(".treeview li");

		Assert.That(treeNodes[9].QuerySelector("div span").TextContent, Is.EqualTo("Test Item Selected"));
		Assert.That(treeNodes[9].QuerySelector("div span").GetAttribute("style"), Is.EqualTo("color:var(--color-highlight-text);background:#D3D3D3FF;font-size:18pt;"));

		await form.InvokeWinzorDispatcherAsync(() =>
		{
			treeView.Focus();
			treeView.SelectedNode = treeView.Nodes[1];
		});
		treeNodes = rendered.FindAll(".treeview li");

		Assert.That(treeNodes[1].QuerySelector("div span").TextContent, Is.EqualTo("Test Item 2"));
		Assert.That(treeNodes[1].QuerySelector("div span").GetAttribute("style"), Is.EqualTo("color:var(--color-highlight-text);background:var(--color-highlight);"));

		Assert.That(treeNodes[9].QuerySelector("div span").TextContent, Is.EqualTo("Test Item Selected"));
		Assert.That(treeNodes[9].QuerySelector("div span").GetAttribute("style"), Is.EqualTo("font-size:18pt;"));
	}

	[Test]
	public async Task MouseDownShouldHaveFocus()
	{
		TreeView treeView = null;
		Button anotherControl = null;
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form() { Size = new Size(500, 500) };

			treeView = new TreeView() { Dock = DockStyle.Left, Width = 400 };
			anotherControl = new Button() { Dock = DockStyle.Right, Width = 100 };

			form.Controls.Add(treeView);
			form.Controls.Add(anotherControl);

			return form;
		});

		await anotherControl.InvokeWinzorDispatcherAsync(() => anotherControl.Focus());
		Assert.That(anotherControl.Focused);
		Assert.That(!treeView.Focused);

		await rendered.Find(".treeview").MouseDownAsync(new WebMouseEventArgs());
		Assert.That(!anotherControl.Focused);
		Assert.That(treeView.Focused);
	}

	[Test]
	public async Task AfterSelectTriggeredAfterTreeViewBecomesVisible()
	{
		using var ctx = new WinzorTestContext();
		TreeView treeView = null;
		var afterSelectTriggered = false;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			treeView = new TreeView();
			treeView.Visible = false;
			treeView.Nodes.Add("abc");
			treeView.AfterSelect += (s, e) => afterSelectTriggered = true;
			return treeView;
		});

		await treeView.InvokeWinzorDispatcherAsync(() => treeView.SelectedNode = treeView.Nodes[0]);

		Assert.That(afterSelectTriggered, Is.False);

		await treeView.InvokeWinzorDispatcherAsync(() => treeView.Visible = true);

		Assert.That(afterSelectTriggered, Is.True);
	}

	[Test]
	public async Task AfterSelectShouldNotBeTriggeredIfSelectedNodeIsNullAfterTurningVisible()
	{
		using var ctx = new WinzorTestContext();

		TreeView treeView = null;
		await ctx.RenderControlOnFormAsync(() =>
		{
			treeView = new TreeView();
			treeView.Nodes.Add("abc");
			treeView.SelectedNode = treeView.Nodes[0];
			return treeView;
		});

		var afterSelectTriggered = false;
		treeView.AfterSelect += (s, e) => afterSelectTriggered = true;

		await treeView.InvokeWinzorDispatcherAsync(() =>
		{
			treeView.Visible = false;
			treeView.SelectedNode = null!;
			treeView.Visible = true;
		});

		Assert.That(afterSelectTriggered, Is.False);
	}

	[Test]
	public async Task AfterSelectShouldNotBeTriggeredIfSelectedNodeIsNullAndControlIsVisible()
	{
		using var ctx = new WinzorTestContext();

		TreeView treeView = null;
		await ctx.RenderControlOnFormAsync(() =>
		{
			treeView = new TreeView();
			treeView.Nodes.Add("abc");
			treeView.SelectedNode = treeView.Nodes[0];
			return treeView;
		});

		var afterSelectTriggered = false;
		treeView.AfterSelect += (s, e) => afterSelectTriggered = true;

		await treeView.InvokeWinzorDispatcherAsync(() =>
		{
			treeView.SelectedNode = null!;
		});

		Assert.That(afterSelectTriggered, Is.False);
		Assert.That(treeView.Visible, Is.True);
		Assert.That(treeView.SelectedNode, Is.Null);
	}

	[TestCase(1, "treeview__nodestateimage")]
	[TestCase(2, "treeview__nodeimage")]
	public async Task NodeHasTheImages(int nthOfImg, string imgClass)
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() => TestTreeView());
		Assert.That(rendered.Find(".treeview > .treeview__nodes > li:first-child > .treeview__nodecontent").Children.FirstOrDefault(e => e.ClassName == imgClass), Is.Null);

		rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var treeView = TestTreeView();
			treeView.StateImageList = new ImageList();
			treeView.StateImageList.Images.Add(new Bitmap(1, 1));
			treeView.ImageList = new ImageList();
			treeView.ImageList.Images.Add(new Bitmap(1, 1));

			return treeView;
		});
		Assert.That(rendered.Find($".treeview > .treeview__nodes > li:first-child > .treeview__nodecontent > span:nth-of-type({nthOfImg})").GetAttribute("class"), Is.EqualTo(imgClass));
	}

	[Test, WithPlaywrightPage]
	public async Task ShouldNotFocusNodeWhenTreeViewIsNotFocused()
	{
		await using var ctx = new InMemoryTestServerContext();
		TreeView treeView = null;
		TreeView treeView2 = null;

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			treeView = new TreeView();
			treeView2 = new TreeView();
			treeView.Nodes.Add(new TreeNode("TestNode"));
			treeView2.Nodes.Add(new TreeNode("TestNode"));
			treeView2.Top = 150;
			form.Controls.Add(treeView);
			form.Controls.Add(treeView2);
			treeView.AfterSelect += (sender, args) =>
			{
				treeView2.Nodes.Clear();
				treeView2.Nodes.Add(new TreeNode("TestNode"));
				treeView2.SelectedNode = treeView2.Nodes[0];
			};

			return form;
		});
		var selectedNode = page.Locator(".treeview__nodes > li:first-child .treeview__nodetext");
		await selectedNode.First.ClickAsync();

		Assert.That(treeView.Focused, Is.True);
		Assert.That(treeView2.Focused, Is.False);
		Assert.That(async () => await selectedNode.First.GetComputedStyleAsync("color"), Is.EqualTo("rgb(255, 255, 255)"));
		Assert.That(async () => await selectedNode.First.GetComputedStyleAsync("background-color"), Is.EqualTo("rgb(0, 120, 215)"));
		Assert.That(async () => await selectedNode.Nth(1).GetComputedStyleAsync("color"), Is.EqualTo("rgb(255, 255, 255)"));
		Assert.That(async () => await selectedNode.Nth(1).GetComputedStyleAsync("background-color"), Is.EqualTo("rgb(211, 211, 211)"));
	}

	[WithPlaywrightPage, Test]
	public async Task TreeNodeImagesHaveCorrectSize()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var tree = new TreeView();
			tree.ImageList = new ImageList();
			tree.ImageList.Images.Add(new Bitmap(24, 24));
			tree.Nodes.Add("Parent 1");
			return tree;
		});

		var nodeImage = page.Locator("img").Nth(0);
		Assert.That(await nodeImage.GetComputedStyleAsync("width"), Is.EqualTo("16px"));
		Assert.That(await nodeImage.GetComputedStyleAsync("height"), Is.EqualTo("16px"));
	}

	[Test]
	public async Task TreeViewSelectedNodeAllParentNodesExpandedWhenVisible()
	{
		using var ctx = new WinzorTestContext();
		TreeView treeView = null;

		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			treeView = TestTreeView();
			treeView.Visible = true;
			treeView.SelectedNode = treeView.Nodes[0].Nodes[0];
			return treeView;
		});

		Assert.That(rendered.Find(".treeview > .treeview__nodes > li:first-child > .treeview__nodes"), Is.Not.Null);
	}

	[Test]
	[TestCase(BorderStyle.None, "treeview--border-none")]
	[TestCase(BorderStyle.FixedSingle, "treeview--border-fixedsingle")]
	[TestCase(BorderStyle.Fixed3D, "treeview--border-fixed3d")]
	public async Task TreeViewBorderStyle(BorderStyle borderStyle, string borderStyleClass)
	{
		using var ctx = new WinzorTestContext();
		TreeView treeView = null;

		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			treeView = TestTreeView();
			treeView.Visible = true;
			treeView.SelectedNode = treeView.Nodes[0].Nodes[0];
			treeView.BorderStyle = borderStyle;
			return treeView;
		});

		var renderedLabel = rendered.Find(".treeview");
		Assert.That(renderedLabel.ClassList, Does.Contain(borderStyleClass));
	}

	[Test]
	public async Task TestAddRangeInSortedTreeView()
	{
		using var ctx = new WinzorTestContext();
		TreeView treeView = null;

		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			treeView = new TreeView { Top = 35 };
			treeView.Sorted = true;
			treeView.TreeViewNodeSorter = new TestComparer();
			treeView.Nodes.AddRange(new[]
			{
				new TreeNode("Node1"),
				new TreeNode("Node3")
			});
			return treeView;
		});

		Assert.That(treeView.Nodes[0].Text, Is.EqualTo("Node3"));
		Assert.That(treeView.Nodes[1].Text, Is.EqualTo("Node1"));

		await treeView.InvokeWinzorDispatcherAsync(() =>
		{
			treeView.Nodes.AddRange(new[] { new TreeNode("Node2") });
		});

		Assert.That(treeView.Nodes[0].Text, Is.EqualTo("Node3"));
		Assert.That(treeView.Nodes[1].Text, Is.EqualTo("Node2"));
		Assert.That(treeView.Nodes[2].Text, Is.EqualTo("Node1"));
	}

	[Test, WithPlaywrightPage]
	public async Task TreeViewInFlushedDOMShouldBeNoErrors()
	{
		await using var ctx = new InMemoryTestServerContext();
		TreeView tree = null;
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			tree = TestTreeView(false, xSize: 2, ySize: 2, zSize: 0);
			return tree;
		});

		var firstNodeButton = page.Locator("li:nth-child(1) > Button");
		Assert.That(firstNodeButton, Is.Not.Null);
		await firstNodeButton.ClickAsync();

		var firstItem = page.Locator("li:nth-child(1) > ul > li:nth-child(1) > div > span");
		await firstItem.ClickAsync();
		var activeNodeText = await page.EvaluateAsync<string>("document.activeElement.innerText");
		Assert.That(activeNodeText, Is.EqualTo(tree.Nodes[0].FirstNode.Text));

		// mock the result of searching scenario that makes node.reference to be empty
		tree.Nodes[1].ElementReference = new ElementReference();

		var secondNodeButton = page.Locator("li:nth-child(2) > Button");
		Assert.That(secondNodeButton, Is.Not.Null);
		await secondNodeButton.ClickAsync();

		var secondItem = page.Locator("li:nth-child(2) > ul > li:nth-child(1) > div > span");
		await secondItem.ClickAsync();
		activeNodeText = await page.EvaluateAsync<string>("document.activeElement.innerText");
		Assert.That(activeNodeText, Is.EqualTo(tree.Nodes[1].FirstNode.Text));
	}

	[Test, WithPlaywrightPage]
	public async Task TestAutoScrollAfterExpand()
	{
		await using var ctx = new InMemoryTestServerContext();
		TreeView tree = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			tree = TestTreeView(false, xSize: 5, ySize: 10, zSize: 0);
			form.Controls.Add(tree);
			return form;
		});

		var lastNode = await page.WaitForSelectorAsync("li:nth-child(5)");
		Assert.That(lastNode, Is.Not.Null);

		var rectTopBeforeExpand = await lastNode.EvaluateAsync<double>("e => e.getBoundingClientRect().top");

		var lastNodeButton = await page.WaitForSelectorAsync("li:nth-child(5) > Button");
		Assert.That(lastNodeButton, Is.Not.Null);
		await lastNodeButton.ClickAsync();

		Assert.That(() => lastNode.EvaluateAsync<double>("e => e.getBoundingClientRect().top"), Is.LessThan(rectTopBeforeExpand).After(2000, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task TestClickBlankAreaGetNull()
	{
		await using var ctx = new InMemoryTestServerContext();
		TreeView tree = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			tree = TestTreeView(false);
			form.Controls.Add(tree);
			return form;
		});
		var node = await page.WaitForSelectorAsync(".treeview .treeview__node:last-child .treeview__nodecontent");
		var nodeRect = await node.BoundingBoxAsync();
		await page.Mouse.ClickAsync(nodeRect.X + 5, nodeRect.Y + 5);
		await Task.Delay(500);
		Assert.That(tree.GetNodeAt((int)nodeRect.X + 5, (int)nodeRect.Y + 5), Is.Not.Null);

		await page.Mouse.ClickAsync(nodeRect.X + 5, nodeRect.Y + 25);
		await Task.Delay(500);
		Assert.That(tree.GetNodeAt((int)nodeRect.X + 5, (int)nodeRect.Y + 25), Is.EqualTo(null));
	}

	[Test, WithPlaywrightPage]
	public async Task HalfVerticalLineHeightForSingleChildNode()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			var tree = new TreeView();
			var parent = tree.Nodes.Add("Parent 1");
			var child = parent.Nodes.Add("Child 1");

			parent.ExpandAll();

			form.Controls.Add(tree);
			return form;
		});
		var node = await page.WaitForSelectorAsync(".treeview .treeview__node .treeview__node");

		Assert.That(
			await node.EvaluateAsync<float>($"e => parseFloat(window.getComputedStyle(e, ':before').getPropertyValue('height'))"),
			Is.EqualTo(7.6).Within(0.1));
	}

	[Test, WithPlaywrightPage]
	public async Task TabStopsAtTreeVewAndFocusOnSelectedNode()
	{
		await using var ctx = new InMemoryTestServerContext();
		TreeView treeview = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			var input = new TextBox();
			form.Controls.Add(input);

			treeview = new TreeView { Top = 35 };
			_ = treeview.Nodes.Add("Node1");
			_ = treeview.Nodes.Add("Node2");
			form.Controls.Add(treeview);
			form.Controls.Add(new TextBox() { Top = 150 });

			treeview.SelectedNode = treeview.Nodes[0];

			return form;
		});

		var input = await page.WaitForSelectorAsync(".textbox");
		await input.ClickAsync();
		await page.Keyboard.PressAsync("Tab");

		Assert.That(() => treeview.SelectedNode?.Text, Is.EqualTo("Node1").After(3000, 100));
		var activeNodeText = await page.EvaluateAsync<string>("document.activeElement.innerText");
		Assert.That(activeNodeText, Is.EqualTo("Node1"));

		var node = await page.WaitForSelectorAsync(".treeview .treeview__node:first-child .treeview__nodetext");
		var nodeBackgroundColor = await node.GetComputedStyleAsync("background-color");
		Assert.That(nodeBackgroundColor, Is.EqualTo("rgb(0, 120, 215)"));     // #0078d7

		await page.Keyboard.PressAsync("Tab");
		Assert.That(() => treeview.Focused, Is.EqualTo(false).After(3000, 100));

		var nodeBackgroundColorFocusOut = await node.GetComputedStyleAsync("background-color");
		Assert.That(nodeBackgroundColorFocusOut, Is.EqualTo("rgb(211, 211, 211)"));       // #d3d3d3
	}

	[Test, WithPlaywrightPage]
	public async Task TreeViewPreventArrowKeysDefaultEvent()
	{
		await using var ctx = new InMemoryTestServerContext();

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form() { Width = 200, Height = 500 };
			var treeview = new TreeView { Top = 35 };
			for (var i = 0; i < 10; i++)
			{
				treeview.Nodes.Add($"Node with long name");
			}
			form.Controls.Add(treeview);
			return form;
		});

		var treeView = page.Locator(".treeview");
		await page.Locator(".treeview__nodes > li").First.ClickAsync();

		Assert.That(async () => await treeView.EvaluateAsync<double>("e => e.scrollLeft"), Is.EqualTo(0).After(3000, 100));
		Assert.That(async () => await treeView.EvaluateAsync<double>("e => e.scrollTop"), Is.EqualTo(0).After(3000, 100));

		await page.Keyboard.PressAsync("ArrowDown");
		await page.Keyboard.PressAsync("ArrowRight");

		Assert.That(async () => await treeView.EvaluateAsync<double>("e => e.scrollLeft"), Is.EqualTo(0).After(3000, 100));
		Assert.That(async () => await treeView.EvaluateAsync<double>("e => e.scrollTop"), Is.EqualTo(0).After(3000, 100));

		await page.Keyboard.PressAsync("ArrowUp");
		await page.Keyboard.PressAsync("ArrowLeft");

		Assert.That(async () => await treeView.EvaluateAsync<double>("e => e.scrollLeft"), Is.EqualTo(0).After(3000, 100));
		Assert.That(async () => await treeView.EvaluateAsync<double>("e => e.scrollTop"), Is.EqualTo(0).After(3000, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task TreeViewShouldFocusNodeAfterSelect()
	{
		await using var ctx = new InMemoryTestServerContext();
		TreeView treeview = null;

		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			treeview = new TreeView { Top = 35 };
			treeview.Nodes.Add("Node1");
			treeview.Nodes.Add("Node2");
			return treeview;
		});

		await treeview.InvokeWinzorDispatcherAsync(() =>
		{
			treeview.SelectedNode = treeview.Nodes[0];
		});
		Assert.That(async () => await page.EvaluateAsync<string>("document.activeElement.textContent"), Is.EqualTo("Node1").After(3000, 100));

		await treeview.InvokeWinzorDispatcherAsync(() =>
		{
			treeview.SelectedNode = treeview.Nodes[1];
		});
		Assert.That(async () => await page.EvaluateAsync<string>("document.activeElement.textContent"), Is.EqualTo("Node2").After(3000, 100));
	}

	[WithPlaywrightPage]
	[TestCase(".treeview__nodetext")]
	[TestCase(".treeview__nodestateimage")]
	[TestCase(".treeview__nodeimage")]
	[TestCase(".treeview__nodebutton")]
	public async Task TreeNodesHaveCorrectTabIndex(string queryElement)
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			var tree = new TreeView();
			tree.StateImageList = new ImageList();
			tree.StateImageList.Images.Add(new Bitmap(1, 1));
			tree.ImageList = new ImageList();
			tree.ImageList.Images.Add(new Bitmap(1, 1));
			var parent1 = tree.Nodes.Add("Parent 1");
			parent1.Nodes.Add("Child 1");
			var parent2 = tree.Nodes.Add("Parent 2");
			parent2.Nodes.Add("Child 2");
			form.Controls.Add(tree);

			return form;
		});
		await page.WaitForSelectorAsync(".treeview");

		var nodes = await page.QuerySelectorAllAsync(queryElement);
		await checkTabIndexOfNodeTexts(page, 0);

		if (queryElement == ".treeview__nodebutton")
		{
			await clickAndCheckTabIndex(page, nodes, 1, 0);
			await clickAndCheckTabIndex(page, nodes, 0, 0);
		}
		else
		{
			await clickAndCheckTabIndex(page, nodes, 1);
			await clickAndCheckTabIndex(page, nodes, 0);
		}
	}

	async Task clickAndCheckTabIndex(IPage page, IReadOnlyList<IElementHandle> nodes, int indexToClick, int indexToBeOfTabIndexZero = -1)
	{
		if (indexToBeOfTabIndexZero == -1)
		{
			indexToBeOfTabIndexZero = indexToClick;
		}
		await nodes[indexToClick].ClickAsync();
		await Task.Delay(100);
		await checkTabIndexOfNodeTexts(page, indexToBeOfTabIndexZero);
		await Task.Delay(100);
	}

	async Task checkTabIndexOfNodeTexts(IPage page, int nodeIndexWithTabIndexZero)
	{
		var nodes = await page.QuerySelectorAllAsync(".treeview__nodetext");
		for (int i = 0; i < nodes.Count; i++)
		{
			var nodeTabIndex = await nodes[i].EvaluateAsync<int>("e => e.tabIndex");
			Assert.That(nodeTabIndex, Is.EqualTo(i == nodeIndexWithTabIndexZero ? 0 : -1));
		}
	}

	[Test, WithPlaywrightPage]
	public async Task TreeNodesHaveCorrectSelectedStyle()
	{
		await using var ctx = new InMemoryTestServerContext();
		TreeView tree = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			var checkBox = new CheckBox { Text = "CheckBox" };
			tree = new TreeView();
			tree.Nodes.Add("node 1");
			tree.Nodes.Add("node 2");
			// add a control before the tree view so that the first tree view node is not selected straight away
			form.Controls.Add(checkBox);
			form.Controls.Add(tree);

			return form;
		});

		await page.WaitForSelectorAsync(".treeview");

		var nodes = await page.QuerySelectorAllAsync(".treeview__nodetext");
		Assert.That(await nodes[0].EvaluateAsync<string>("e => e.parentNode.parentNode.className"), Does.Contain("treeview__node--selected"));
		Assert.That(await page.EvaluateAsync<string>("() => document.activeElement.textContent"), Is.Not.EqualTo("node 1"));

		await page.Keyboard.PressAsync("Tab"); // firstnode has tabindex 0 on init
		await Task.Delay(500);
		nodes = await page.QuerySelectorAllAsync(".treeview__nodetext");
		Assert.That(await nodes[0].EvaluateAsync<string>("e => e.parentNode.parentNode.className"), Does.Contain("treeview__node--selected"));
		Assert.That(await page.EvaluateAsync<string>("() => document.activeElement.textContent"), Is.EqualTo("node 1"));
		Assert.That(await nodes[1].EvaluateAsync<string>("e => e.parentNode.parentNode.className"), Does.Not.Contain("treeview__node--selected"));

		await nodes[1].ClickAsync();
		await Task.Delay(500);
		nodes = await page.QuerySelectorAllAsync(".treeview__nodetext");
		Assert.That(await nodes[1].EvaluateAsync<string>("e => e.parentNode.parentNode.className"), Does.Contain("treeview__node--selected"));
		Assert.That(await page.EvaluateAsync<string>("() => document.activeElement.textContent"), Is.EqualTo("node 2"));
		Assert.That(await nodes[0].EvaluateAsync<string>("e => e.parentNode.parentNode.className"), Does.Not.Contain("treeview__node--selected"));
	}

	public static object[] TreeViewNavigationKeyTestCaseSource =
	{
		new object[] { "ArrowUp", false, "1", "0", "0" },
		new object[] { "Control+ArrowUp", false, "1", "1", "1" },
		new object[] { "ArrowDown", false, "3", "4", "4" },
		new object[] { "Control+ArrowDown", false, "3", "3", "3" },
		new object[] { "PageUp", true, "1 - 2", "0 - 3", "0" },
		new object[] { "Control+PageUp", true, "4 - 4 - 4", "4 - 4 - 4", "4 - 4 - 4" },
		new object[] { "PageDown", true, "4", "4 - 4", "4 - 4 - 4" },
		new object[] { "Control+PageDown", true, "0", "0", "0" },
		new object[] { "Home", true, "2 - 2", "0", "0" },
		new object[] { "Control+Home", true, "2 - 2", "2 - 2", "2 - 2" },
		new object[] { "End", true, "3 - 3", "4 - 4 - 4", "4 - 4 - 4" },
		new object[] { "Control+End", true, "3 - 3", "3 - 3", "3 - 3" },
	};

	[Test, WithPlaywrightPage]
	[TestCaseSource(nameof(TreeViewNavigationKeyTestCaseSource))]
	public async Task TreeViewResponseToNavigationKey(string key, bool expanded, string initNodeSelector, string selectedNodeText1, string selectedNodeText2)
	{
		await using var ctx = new InMemoryTestServerContext();
		TreeView treeview = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form { Width = 500, Height = 520 };
			treeview = TestTreeView(expanded: expanded);
			treeview.Width = 400;
			treeview.Height = 418;                              // without horizontal scrollbar, one page should contains 418 / 16 = 26 TreeNodes.
			form.Controls.Add(treeview);

			return form;
		});

		var initNode = page.GetByText(initNodeSelector, new () { Exact = true }).Locator("xpath=../..");
		await initNode.Locator("> .treeview__nodecontent >.treeview__nodetext").ClickAsync();
		await AssertTreeViewNodeElementExpanded(initNode, expanded: expanded);

		var selectedNode = page.Locator(".treeview__node--selected");
		await selectedNode.PressAsync(key);
		await Task.Delay(200);
		await AssertTreeViewNodeElementExpanded(selectedNode, expanded: expanded);
		Assert.That(treeview.SelectedNode?.Text, Is.EqualTo(selectedNodeText1));

		await selectedNode.PressAsync(key);
		await Task.Delay(200);
		await AssertTreeViewNodeElementExpanded(selectedNode, expanded: expanded);
		Assert.That(treeview.SelectedNode?.Text, Is.EqualTo(selectedNodeText2));
	}

	[Test, WithPlaywrightPage]
	public async Task TreeViewResponseToNavigationKey_Left()
	{
		await using var ctx = new InMemoryTestServerContext();
		TreeView treeview = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form { Width = 300, Height = 400 };
			treeview = TestTreeView(expanded: true);
			form.Controls.Add(treeview);

			return form;
		});

		var initNode = page.Locator(".treeview .treeview__node:nth-child(1) .treeview__node:nth-child(1) .treeview__node:nth-child(1)");
		Assert.That(initNode, Is.Not.Null);
		await initNode.Locator(".treeview__nodetext").ClickAsync();
		await Task.Delay(200);
		await AssertTreeViewNodeElementExpanded(initNode, expanded: false);

		var selectedNode = page.Locator(".treeview__node--selected");
		Assert.That(selectedNode, Is.Not.Null);

		await selectedNode.PressAsync("ArrowLeft");                 // select parent node
		await Task.Delay(200);
		await AssertTreeViewNodeElementExpanded(initNode, expanded: false);
		Assert.That(treeview.SelectedNode?.Text, Is.EqualTo("0 - 0"));
		Assert.That(treeview.SelectedNode?.IsExpanded, Is.True);

		await selectedNode.PressAsync("ArrowLeft");                 // collapse
		await Task.Delay(200);
		await AssertTreeViewNodeElementExpanded(selectedNode, expanded: false);
		Assert.That(treeview.SelectedNode?.Text, Is.EqualTo("0 - 0"));
	}

	[Test, WithPlaywrightPage]
	public async Task TreeViewResponseToNavigationKey_Right()
	{
		await using var ctx = new InMemoryTestServerContext();
		TreeView treeview = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form { Width = 300, Height = 400 };
			treeview = TestTreeView(expanded: false);
			form.Controls.Add(treeview);

			return form;
		});

		var initNode = page.Locator(".treeview .treeview__node:nth-child(2)");
		Assert.That(initNode, Is.Not.Null);
		await initNode.Locator(".treeview__nodetext").ClickAsync();
		//await Task.Delay(200);
		await AssertTreeViewNodeElementExpanded(initNode, expanded: false);

		var selectedNode = page.Locator(".treeview__node--selected");
		Assert.That(selectedNode, Is.Not.Null);

		await selectedNode.PressAsync("ArrowRight");                // expand
		await Task.Delay(200);
		await AssertTreeViewNodeElementExpanded(selectedNode, expanded: true);
		Assert.That(treeview.SelectedNode?.Text, Is.EqualTo("1"));

		await selectedNode.PressAsync("ArrowRight");                // select first child node
		await Task.Delay(200);
		Assert.That(treeview.SelectedNode?.Text, Is.EqualTo("1 - 0"));
		await AssertTreeViewNodeElementExpanded(selectedNode, expanded: false);
	}

	async Task AssertTreeViewNodeElementExpanded(ILocator element, bool expanded)
	{
		var childrenLength = await element.EvaluateAsync<int>("e => e.children.length");
		if (childrenLength != 1)        // 1 means no child nodes
		{
			Assert.That(childrenLength, Is.EqualTo(expanded ? 3 : 2));
		}
	}

	class TestComparer : IComparer<TreeNode>, IComparer
	{
		internal TestComparer()
		{
		}

		public int Compare(TreeNode x, TreeNode y)
		{
			int result;

			if (x == null && y == null)
			{
				result = 0;
			}
			else if (x == null)
			{
				result = 1;
			}
			else if (y == null)
			{
				result = -1;
			}
			else
			{
				result = string.Compare(y.Text, x.Text, StringComparison.Ordinal);
			}

			return result;
		}

		public int Compare(object x, object y)
		{
			return Compare((TreeNode)x, (TreeNode)y);
		}
	}
}
