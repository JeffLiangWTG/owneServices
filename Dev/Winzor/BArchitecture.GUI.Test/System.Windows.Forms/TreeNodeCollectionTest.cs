using System.Threading.Tasks;
using NUnit.Framework;
using WinzorTestFramework;

namespace System.Windows.Forms;
internal class TreeNodeCollectionTest
{
	[Test]
	public async Task TestKeyShouldReturnFirstMatchingNode()
	{
		using var context = new WinzorTestContext();
		await context.WinzorDispatcher.InvokeAsync(() => {
			// Arrange
			TreeNode rootNode = new TreeNode("root");
			TreeNode treeNode1 = new TreeNode("1");
			TreeNode treeNode2 = new TreeNode("1");

			TreeNodeCollection tree = new TreeNodeCollection(rootNode);

			// Act and Assert
			tree.Add(treeNode1);
			tree.Add(treeNode2);

			var result = tree[treeNode1.Name];

			Assert.That(result, Is.Not.Null);
			Assert.That(treeNode1, Is.EqualTo(result));
		});
	}
}
