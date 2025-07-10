using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using WinzorFramework;
using WinzorTestFramework;

namespace System.Windows.Forms;

class MenuItemCollectionTest
{
	[Test]
	public async Task TestMenuItemCollectionFind()
	{
		using var ctx = new WinzorTestContext();
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var menu = GetTestMenu();

			Assert.Throws<ArgumentNullException>(() => menu.MenuItems.Find("", true));
			Assert.Throws<ArgumentNullException>(() => menu.MenuItems.Find(null, true));

			var foundMenuItems = menu.MenuItems.Find("BelovedGrandChild", false);
			Assert.That(foundMenuItems.Length, Is.EqualTo(0));

			foundMenuItems = menu.MenuItems.Find("belOvedGrandchild", true);
			Assert.That(foundMenuItems.Length, Is.EqualTo(2));

			foundMenuItems = menu.MenuItems.Find("chosen one", true);
			Assert.That(foundMenuItems.Length, Is.EqualTo(0));
		});
	}

	[Test]
	public async Task TestMenuItemCollectionIndexOfKey()
	{
		using var ctx = new WinzorTestContext();
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var menu = GetTestMenu();

			Assert.That(menu.MenuItems.IndexOfKey(""), Is.EqualTo(-1));
			Assert.That(menu.MenuItems.IndexOfKey(null), Is.EqualTo(-1));
			Assert.That(menu.MenuItems.IndexOfKey("child1"), Is.EqualTo(0));
			Assert.That(menu.MenuItems.IndexOfKey("CHILD2"), Is.EqualTo(1));
			Assert.That(menu.MenuItems.IndexOfKey("Child3"), Is.EqualTo(-1));
		});
	}

	[Test]
	public async Task TestMenuItemCollectionContainsKey()
	{
		using var ctx = new WinzorTestContext();
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var menu = GetTestMenu();

			Assert.That(menu.MenuItems.ContainsKey(""), Is.False);
			Assert.That(menu.MenuItems.ContainsKey(null), Is.False);
			Assert.That(menu.MenuItems.ContainsKey("child1"), Is.True);
			Assert.That(menu.MenuItems.ContainsKey("Child3"), Is.False);
		});
	}

	MenuItem GetTestMenu()
	{
		var parent = new MenuItem();
		var child1 = new MenuItem { Name = "Child1" };
		var child2 = new MenuItem { Name = "Child2" };
		var grandchild1 = new MenuItem { Name = "Grandchild" };
		var grandchild2 = new MenuItem { Name = "Grandchild" };
		var grandchild3 = new MenuItem { Name = "Grandchild" };
		var belovedGrandchild1 = new MenuItem { Name = "BelovedGrandchild" };
		var belovedGrandchild2 = new MenuItem { Name = "BelovedGrandchild" };

		parent.MenuItems.Add(child1);
		parent.MenuItems.Add(child2);

		child1.MenuItems.Add(grandchild1);
		child1.MenuItems.Add(grandchild2);
		child1.MenuItems.Add(belovedGrandchild1);

		child2.MenuItems.Add(grandchild3);
		child2.MenuItems.Add(belovedGrandchild2);

		return parent;
	}
}
