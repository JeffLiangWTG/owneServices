using System.Threading.Tasks;
using Microsoft.Playwright;
using NUnit.Framework;
using WinzorTestFramework;
using WTG.PlaywrightTesting;

namespace System.Windows.Forms;

using static PlaywrightTestContext;
class TreeNodeTest
{
	[Test]
	public async Task NextNodeShouldReturnNextNode()
	{
		using var context = new WinzorTestContext();
		await context.WinzorDispatcher.InvokeAsync(() => {
			// Arrange
			TreeView treeView = new TreeView();
			TreeNode rootNode = new TreeNode("root");
			TreeNode treeNode1 = new TreeNode("1");
			TreeNode treeNode2 = new TreeNode("2");
			TreeNode treeNode3 = new TreeNode("3");
			treeView.Nodes.Add(rootNode);
			rootNode.Nodes.Add(treeNode1);
			rootNode.Nodes.Add(treeNode2);
			rootNode.Nodes.Add(treeNode3);
			// Act and Assert
			Assert.That(treeNode1.NextNode, Is.EqualTo(treeNode2));
			Assert.That(treeNode2.NextNode, Is.EqualTo(treeNode3));
		});
	}

	[Test]
	public async Task NextNodeShouldReturnNullIfLastNodeInLevel()
	{
		using var context = new WinzorTestContext();
		await context.WinzorDispatcher.InvokeAsync(() => {
			// Arrange
			TreeView treeView = new TreeView();
			TreeNode rootNode = new TreeNode("root");
			TreeNode treeNode1 = new TreeNode("1");
			TreeNode treeNode2 = new TreeNode("2");
			treeView.Nodes.Add(rootNode);
			rootNode.Nodes.Add(treeNode1);
			rootNode.Nodes.Add(treeNode2);
			// Act and Assert
			Assert.That(treeNode2.NextNode, Is.EqualTo(null));
		});
	}

	[Test]
	public async Task NextNodeShouldReturnNullIfOnlyNodeInLevel()
	{
		using var context = new WinzorTestContext();
		await context.WinzorDispatcher.InvokeAsync(() => {
			// Arrange
			TreeView treeView = new TreeView();
			TreeNode rootNode = new TreeNode("root");
			TreeNode treeNode1 = new TreeNode("1");
			treeView.Nodes.Add(rootNode);
			rootNode.Nodes.Add(treeNode1);
			// Act and Assert
			Assert.That(treeNode1.NextNode, Is.EqualTo(null));
		});
	}

	[Test]
	public async Task NextNodeShouldReturnNullIfRootNode()
	{
		using var context = new WinzorTestContext();
		await context.WinzorDispatcher.InvokeAsync(() => {
			// Arrange
			TreeView treeView = new TreeView();
			TreeNode rootNode = new TreeNode("root");
			treeView.Nodes.Add(rootNode);
			// Act and Assert
			Assert.That(rootNode.NextNode, Is.EqualTo(null));
		});
	}

	[Test]
	public async Task PrevNodeShouldReturnPrevNode()
	{
		using var context = new WinzorTestContext();
		await context.WinzorDispatcher.InvokeAsync(() => {
			// Arrange
			TreeView treeView = new TreeView();
			TreeNode rootNode = new TreeNode("root");
			TreeNode treeNode1 = new TreeNode("1");
			TreeNode treeNode2 = new TreeNode("2");
			TreeNode treeNode3 = new TreeNode("3");
			treeView.Nodes.Add(rootNode);
			rootNode.Nodes.Add(treeNode1);
			rootNode.Nodes.Add(treeNode2);
			rootNode.Nodes.Add(treeNode3);
			// Act and Assert
			Assert.That(treeNode2.PrevNode, Is.EqualTo(treeNode1));
			Assert.That(treeNode3.PrevNode, Is.EqualTo(treeNode2));
		});
	}

	[Test]
	public async Task PrevNodeShouldReturnNullIfFirstNodeInLevel()
	{
		using var context = new WinzorTestContext();
		await context.WinzorDispatcher.InvokeAsync(() => {
			// Arrange
			TreeView treeView = new TreeView();
			TreeNode rootNode = new TreeNode("root");
			TreeNode treeNode1 = new TreeNode("1");
			TreeNode treeNode2 = new TreeNode("2");
			treeView.Nodes.Add(rootNode);
			rootNode.Nodes.Add(treeNode1);
			rootNode.Nodes.Add(treeNode2);
			// Act and Assert
			Assert.That(treeNode1.PrevNode, Is.EqualTo(null));
		});
	}

	[Test]
	public async Task PrevNodeShouldReturnNullIfOnlyNodeInLevel()
	{
		using var context = new WinzorTestContext();
		await context.WinzorDispatcher.InvokeAsync(() => {
			// Arrange
			TreeView treeView = new TreeView();
			TreeNode rootNode = new TreeNode("root");
			TreeNode treeNode1 = new TreeNode("1");
			treeView.Nodes.Add(rootNode);
			rootNode.Nodes.Add(treeNode1);
			// Act and Assert
			Assert.That(treeNode1.PrevNode, Is.EqualTo(null));
		});
	}

	[Test]
	public async Task PrevNodeShouldReturnNullIfRootNode()
	{
		using var context = new WinzorTestContext();
		await context.WinzorDispatcher.InvokeAsync(() => {
			// Arrange
			TreeView treeView = new TreeView();
			TreeNode rootNode = new TreeNode("root");
			treeView.Nodes.Add(rootNode);
			// Act and Assert
			Assert.That(rootNode.PrevNode, Is.EqualTo(null));
		});
	}

	[Test]
	public async Task FirstNodeShouldReturnFirstChildNode()
	{
		using var context = new WinzorTestContext();
		await context.WinzorDispatcher.InvokeAsync(() => {
			// Arrange
			TreeView treeView = new TreeView();
			TreeNode rootNode = new TreeNode("root");
			TreeNode treeNode1 = new TreeNode("1");
			TreeNode treeNode2 = new TreeNode("2");
			treeView.Nodes.Add(rootNode);
			rootNode.Nodes.Add(treeNode1);
			rootNode.Nodes.Add(treeNode2);
			// Act and Assert
			Assert.That(rootNode.FirstNode, Is.EqualTo(treeNode1));
		});
	}

	[Test]
	public async Task FirstNodeShouldReturnNullIfNodeHasNoChildren()
	{
		using var context = new WinzorTestContext();
		await context.WinzorDispatcher.InvokeAsync(() => {
			// Arrange
			TreeView treeView = new TreeView();
			TreeNode rootNode = new TreeNode("root");
			treeView.Nodes.Add(rootNode);
			// Act and Assert
			Assert.That(rootNode.FirstNode, Is.EqualTo(null));
		});
	}

	[Test]
	public async Task NextVisibleNodeShouldReturnFirstChildIfExpanded()
	{
		using var context = new WinzorTestContext();
		await context.WinzorDispatcher.InvokeAsync(() => {
			// Arrange
			TreeView treeView = new TreeView();
			TreeNode rootNode = new TreeNode("root");
			TreeNode treeNode1 = new TreeNode("1");
			TreeNode treeNode2 = new TreeNode("2");
			TreeNode treeNode3 = new TreeNode("3");
			treeView.Nodes.Add(rootNode);
			rootNode.Nodes.Add(treeNode1);
			treeNode1.Nodes.Add(treeNode2);
			treeNode1.Nodes.Add(treeNode3);
			treeNode1.Expand();
			// Act and Assert
			Assert.That(treeNode1.NextVisibleNode, Is.EqualTo(treeNode2));
		});
	}

	[Test]
	public async Task NextVisibleNodeShouldReturnFirstSiblingIfNotExpanded()
	{
		using var context = new WinzorTestContext();
		await context.WinzorDispatcher.InvokeAsync(() => {
			// Arrange
			TreeView treeView = new TreeView();
			TreeNode rootNode = new TreeNode("root");
			TreeNode treeNode1 = new TreeNode("1");
			TreeNode treeNode2 = new TreeNode("2");
			TreeNode treeNode3 = new TreeNode("3");
			treeView.Nodes.Add(rootNode);
			rootNode.Nodes.Add(treeNode1);
			treeNode1.Nodes.Add(treeNode2);
			rootNode.Nodes.Add(treeNode3);
			treeNode1.Collapse();
			// Act and Assert
			Assert.That(treeNode1.NextVisibleNode, Is.EqualTo(treeNode3));
		});
	}

	[Test]
	public async Task NextVisibleNodeShouldReturnSiblingIfNoChildren()
	{
		using var context = new WinzorTestContext();
		await context.WinzorDispatcher.InvokeAsync(() => {
			// Arrange
			TreeView treeView = new TreeView();
			TreeNode rootNode = new TreeNode("root");
			TreeNode treeNode1 = new TreeNode("1");
			TreeNode treeNode2 = new TreeNode("2");
			TreeNode treeNode3 = new TreeNode("3");
			treeView.Nodes.Add(rootNode);
			rootNode.Nodes.Add(treeNode1);
			rootNode.Nodes.Add(treeNode2);
			rootNode.Nodes.Add(treeNode3);
			// Act and Assert
			Assert.That(treeNode1.NextVisibleNode, Is.EqualTo(treeNode2));
		});
	}

	[Test]
	public async Task NextVisibleNodeShouldReturnParentsSiblingIfNoChildrenAndLastNodeInLevel()
	{
		using var context = new WinzorTestContext();
		await context.WinzorDispatcher.InvokeAsync(() => {
			// Arrange
			TreeView treeView = new TreeView();
			TreeNode rootNode = new TreeNode("root");
			TreeNode treeNode1 = new TreeNode("1");
			TreeNode treeNode2 = new TreeNode("2");
			TreeNode treeNode3 = new TreeNode("3");
			TreeNode treeNode4 = new TreeNode("4");
			TreeNode treeNode5 = new TreeNode("5");
			treeView.Nodes.Add(rootNode);
			rootNode.Nodes.Add(treeNode1);
			treeNode1.Nodes.Add(treeNode2);
			treeNode1.Nodes.Add(treeNode3);
			rootNode.Nodes.Add(treeNode4);
			rootNode.Nodes.Add(treeNode5);
			// Act and Assert
			Assert.That(treeNode3.NextVisibleNode, Is.EqualTo(treeNode4));
		});
	}

	[Test]
	public async Task NextVisibleNodeShouldReturnNullIfLastNodeInTree()
	{
		using var context = new WinzorTestContext();
		await context.WinzorDispatcher.InvokeAsync(() => {
			// Arrange
			TreeView treeView = new TreeView();
			TreeNode rootNode = new TreeNode("root");
			TreeNode treeNode1 = new TreeNode("1");
			TreeNode treeNode2 = new TreeNode("2");
			TreeNode treeNode3 = new TreeNode("3");
			TreeNode treeNode4 = new TreeNode("4");
			TreeNode treeNode5 = new TreeNode("5");
			treeView.Nodes.Add(rootNode);
			rootNode.Nodes.Add(treeNode1);
			treeNode1.Nodes.Add(treeNode2);
			treeNode1.Nodes.Add(treeNode3);
			rootNode.Nodes.Add(treeNode4);
			treeNode4.Nodes.Add(treeNode5);
			// Act and Assert
			Assert.That(treeNode5.NextVisibleNode, Is.EqualTo(null));
		});
	}

	[Test]
	public async Task NextVisibleNodeShouldReturnNullIfOnlyNodeInTree()
	{
		using var context = new WinzorTestContext();
		await context.WinzorDispatcher.InvokeAsync(() => {
			// Arrange
			TreeView treeView = new TreeView();
			TreeNode rootNode = new TreeNode("root");
			treeView.Nodes.Add(rootNode);
			// Act and Assert
			Assert.That(rootNode.NextVisibleNode, Is.EqualTo(null));
		});
	}

	[Test]
	public async Task TreeView_Parent_GetWithParentWithTreeView_ReturnsExpected()
	{
		using var context = new WinzorTestContext();
		await context.WinzorDispatcher.InvokeAsync(() => {
			// Arrange
			using var control = new TreeView();
			var parent = new TreeNode();
			var node1 = new TreeNode();
			var node2 = new TreeNode();
			var node3 = new TreeNode();
			var node4 = new TreeNode();
			control.Nodes.Add(parent);
			parent.Nodes.Add(node1);
			parent.Nodes.Add(node2);
			parent.Nodes.Add(node3);
			node3.Nodes.Add(node4);
			// Act and Assert
			Assert.That(parent, Is.EqualTo(node1.Parent));
			Assert.That(parent, Is.EqualTo(node2.Parent));
			Assert.That(parent, Is.EqualTo(node3.Parent));
			Assert.That(node3, Is.EqualTo(node4.Parent));

			node3.Nodes.Remove(node4);
			Assert.That(node4.Parent, Is.EqualTo(null));
		});
	}

	[Test, WithPlaywrightPage]
	public async Task TreeViewNodeTextSelectedWithMouseDown()
	{
		await using var ctx = new InMemoryTestServerContext();
		TreeView treeView = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			treeView = new TreeView();
			form.Controls.Add(CreateTestTreeView(treeView));
			return form;
		});
		var node = await page.WaitForSelectorAsync(".treeview .treeview__node:first-child .treeview__nodecontent .treeview__nodetext");
		var nodeRect = await node.BoundingBoxAsync();

		//simulate a Mouse Down and NOT Click 
		await Page.Mouse.MoveAsync(nodeRect.X + nodeRect.Width / 2, nodeRect.Y + nodeRect.Height / 2);
		await Page.Mouse.DownAsync();
		await Task.Delay(500);
		await Page.Mouse.MoveAsync(nodeRect.X + nodeRect.Width + 5, nodeRect.Y + nodeRect.Height + 5);
		await Page.Mouse.UpAsync();
		await Task.Delay(500);
		Assert.That(treeView.Nodes[0].IsSelected, Is.True);
	}

	[Test, WithPlaywrightPage]
	public async Task TreeViewNodeButtonSelectedWithMouseDown()
	{
		await using var ctx = new InMemoryTestServerContext();
		TreeView treeView = null; 
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			treeView = new TreeView();
			form.Controls.Add(CreateTestTreeView(treeView));
			return form;
		});
		// deselect node since first node is selected on tree view focused
		await treeView.InvokeWinzorDispatcherAsync(() =>
		{
			treeView.SelectedNode = null;
		});

		var nodeButton = await page.WaitForSelectorAsync(".treeview__nodebutton");
		var nodeButtonRect = await nodeButton.BoundingBoxAsync();

		await Page.Mouse.MoveAsync(nodeButtonRect.X + nodeButtonRect.Width / 2, nodeButtonRect.Y + nodeButtonRect.Height / 2);
		await Page.Mouse.DownAsync();
		await Task.Delay(500);
		await Page.Mouse.MoveAsync(nodeButtonRect.X + nodeButtonRect.Width + 5, nodeButtonRect.Y + nodeButtonRect.Height + 5);
		await Page.Mouse.UpAsync();
		await Task.Delay(500);
		//When Tree view node button down, only need to expand the tree, no need to become selected status.
		Assert.That(treeView.Nodes[0].IsSelected, Is.False);
	}

	[Test]
	public async Task TestTreeNodeNavigation()
	{
		using var context = new WinzorTestContext();
		await context.WinzorDispatcher.InvokeAsync(() =>
		{
			var treeview = CreateTestTreeView();
			treeview.Nodes[0].ExpandAll();
			treeview.Nodes[1].Expand();

			var node = treeview.Nodes[1];
			Assert.That(node.Text, Is.EqualTo("1"));
			Assert.That(node.Parent, Is.Null);
			Assert.That(node.Index, Is.EqualTo(1));
			Assert.That(node.PrevNode?.Text, Is.EqualTo("0"));
			Assert.That(node.PrevVisibleNode?.Text, Is.EqualTo("0 - 4 - 4"));
			Assert.That(node.NextNode?.Text, Is.EqualTo("2"));
			Assert.That(node.NextVisibleNode?.Text, Is.EqualTo("1 - 0"));

			var node2 = treeview.Nodes[0].Nodes[4].Nodes[4];
			Assert.That(node2.Text, Is.EqualTo("0 - 4 - 4"));
			Assert.That(node2.Parent?.Text, Is.EqualTo("0 - 4"));
			Assert.That(node2.PrevNode?.Text, Is.EqualTo("0 - 4 - 3"));
			Assert.That(node2.PrevVisibleNode?.Text, Is.EqualTo("0 - 4 - 3"));
			Assert.That(node2.NextNode, Is.Null);
			Assert.That(node2.NextVisibleNode?.Text, Is.EqualTo("1"));
		});
	}

	[Test]
	public async Task ExpandCollapsedNodeRestoresChildExpandedState()
	{
		using var context = new WinzorTestContext();
		await context.WinzorDispatcher.InvokeAsync(() => {
			// Arrange
			using var control = new TreeView();
			var parent = new TreeNode();
			var node1 = new TreeNode();
			var node2 = new TreeNode();
			var node3 = new TreeNode();
			var node4 = new TreeNode();
			var node5 = new TreeNode();
			var node6 = new TreeNode();
			var node7 = new TreeNode();
			control.Nodes.Add(parent);
			parent.Nodes.Add(node1);
			node1.Nodes.Add(node2);
			node2.Nodes.Add(node3);
			node2.Nodes.Add(node5);
			node2.Nodes.Add(node6);
			node3.Nodes.Add(node4);
			node6.Nodes.Add(node7);

			parent.Expand();
			node1.Expand();
			node2.Expand();
			node6.Expand();

			// Precondition
			Assert.That(parent.IsExpanded);
			Assert.That(node1.IsExpanded);
			Assert.That(node2.IsExpanded);
			Assert.That(node3.IsExpanded, Is.False);
			Assert.That(node4.IsExpanded, Is.False);
			Assert.That(node5.IsExpanded, Is.False);
			Assert.That(node6.IsExpanded);

			// Act

			parent.Collapse();
			parent.Expand();

			// Assert
			Assert.That(parent.IsExpanded);
			Assert.That(node1.IsExpanded);
			Assert.That(node2.IsExpanded);
			Assert.That(node3.IsExpanded, Is.False);
			Assert.That(node4.IsExpanded, Is.False);
			Assert.That(node5.IsExpanded, Is.False);
			Assert.That(node6.IsExpanded);
		});
	}

	[Test]
	public async Task CollapseNodeCollapsesChildNodes()
	{
		using var context = new WinzorTestContext();
		await context.WinzorDispatcher.InvokeAsync(() => {
			// Arrange
			using var control = new TreeView();
			var parent = new TreeNode();
			var node1 = new TreeNode();
			var node2 = new TreeNode();
			var node3 = new TreeNode();
			var node4 = new TreeNode();
			var node5 = new TreeNode();
			var node6 = new TreeNode();
			var node7 = new TreeNode();
			control.Nodes.Add(parent);
			parent.Nodes.Add(node1);
			node1.Nodes.Add(node2);
			node2.Nodes.Add(node3);
			node2.Nodes.Add(node5);
			node2.Nodes.Add(node6);
			node3.Nodes.Add(node4);
			node6.Nodes.Add(node7);

			parent.Expand();
			node1.Expand();
			node2.Expand();
			node6.Expand();

			// Precondition
			Assert.That(parent.IsExpanded);
			Assert.That(node1.IsExpanded);
			Assert.That(node2.IsExpanded);
			Assert.That(node3.IsExpanded, Is.False);
			Assert.That(node4.IsExpanded, Is.False);
			Assert.That(node5.IsExpanded, Is.False);
			Assert.That(node6.IsExpanded);

			// Act

			parent.Collapse();

			// Assert
			Assert.That(parent.IsExpanded, Is.False);
			Assert.That(node1.IsExpanded, Is.False);
			Assert.That(node2.IsExpanded, Is.False);
			Assert.That(node3.IsExpanded, Is.False);
			Assert.That(node4.IsExpanded, Is.False);
			Assert.That(node5.IsExpanded, Is.False);
			Assert.That(node6.IsExpanded, Is.False);
		});
	}

	[Test]
	public async Task AfterCheckCalledOnCheckedSet()
	{
		using var context = new WinzorTestContext();
		await context.WinzorDispatcher.InvokeAsync(() => {
			// Arrange
			using var treeView = new TreeView();
			var node = new TreeNode();
			treeView.Nodes.Add(node);
			var afterCheckCalled = false;
			treeView.AfterCheck += (_, _) => afterCheckCalled = true;

			// Act
			node.Checked = true;

			// Assert
			Assert.That(afterCheckCalled, Is.True);
		});
	}

	[Test]
	public async Task NameReturnsEmptyStringWhenNameSetToNull()
	{
		using var ctx = new WinzorTestContext();

		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var treeView = new TreeView();
			var treeNode = new TreeNode();
			treeView.Nodes.Add(treeNode);
			return treeView;
		});
		var treeView = rendered.GetControl<TreeView>();
		var treeNode = treeView.Nodes[0];

		Assert.That(treeNode.Name, Is.EqualTo(string.Empty));

		await treeView.InvokeWinzorDispatcherAsync(() => treeNode.Name = "hello!!");

		Assert.That(treeNode.Name, Is.EqualTo("hello!!"));

		await treeView.InvokeWinzorDispatcherAsync(() => treeNode.Name = null);

		Assert.That(treeNode.Name, Is.EqualTo(string.Empty));
	}

	TreeView CreateTestTreeView(TreeView treeView = null)
	{
		treeView = treeView ?? new TreeView();

		for (int x = 0; x < 5; x++)
		{
			var nodeX = treeView.Nodes.Add($"{x}");
			for (int y = 0; y < 5; y++)
			{
				var nodeY = nodeX.Nodes.Add($"{x} - {y}");
				for (int z = 0; z < 5; z++)
				{
					nodeY.Nodes.Add($"{x} - {y} - {z}");
				}
			}
		}
		return treeView;
	}
}
