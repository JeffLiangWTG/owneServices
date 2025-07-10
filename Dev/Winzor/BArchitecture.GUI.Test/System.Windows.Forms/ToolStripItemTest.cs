using System.Threading.Tasks;
using NUnit.Framework;
using WinzorFramework;
using WinzorTestFramework;

namespace System.Windows.Forms;

class ToolStripItemTest
{
	[Test]
	public async Task TestToolStripItemClickable([Values] bool enabled, [Values] bool handlesClick)
	{
		using var ctx = new WinzorTestContext();

		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var toolStripItem = new ToolStripItemForTest { Enabled = enabled };
			Assert.That(((IWinzorMenuItem)toolStripItem).Clickable, Is.False);

			if (handlesClick)
			{
				toolStripItem.Click += (_, _) => { };
			}
			Assert.That(((IWinzorMenuItem)toolStripItem).Clickable, Is.EqualTo(enabled && handlesClick));
		});
	}

	[Test]
	public async Task ToolStripItemShouldReturnParentOwnerItem()
	{
		using var ctx = new WinzorTestContext();

		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var ownerItem = new ToolStripItemForTest();
			var owner = new ToolStripDropDown()
			{
				OwnerItem = ownerItem
			};
			var parentItem = new ToolStripItemForTest();
			var parent = new ToolStripDropDown()
			{
				OwnerItem = parentItem
			};
			var item = new ToolStripItemForTest()
			{
				Owner = owner,
				ParentInternal = parent
			};
			Assert.That(parentItem, Is.EqualTo(item.OwnerItem));
		});
	}

	[Test]
	public async Task ToolStripItemShouldReturnOwnerOwnerItem()
	{
		using var ctx = new WinzorTestContext();

		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var ownerItem = new ToolStripItemForTest();
			var owner = new ToolStripDropDown()
			{
				OwnerItem = ownerItem
			};
			var item = new ToolStripItemForTest()
			{
				Owner = owner,
			};
			Assert.That(ownerItem, Is.EqualTo(item.OwnerItem));
		});
	}

	[TestCase(true, true, true)]
	[TestCase(false, true, false)]
	[TestCase(true, false, false)]
	[TestCase(false, false, false)]
	public async Task TestToolStripItemPerformClick(bool enabled, bool available, bool expectedClickSuccess)
	{
		using var ctx = new WinzorTestContext();

		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var toolStripItem = new ToolStripItemForTest { Enabled = enabled, Available = available };
			var clicked = false;
			toolStripItem.Click += (_, _) => { clicked = true; };

			toolStripItem.PerformClick();

			Assert.That(clicked, Is.EqualTo(expectedClickSuccess));
		});
	}

	public class ToolStripItemForTest : ToolStripItem
	{
	}
}
