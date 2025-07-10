using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using WinzorFramework;
using WinzorTestFramework;

namespace System.Windows.Forms;

class MenuItemTest
{
	[Test]
	public async Task AssignParentOnAdd()
	{
		using var ctx = new WinzorTestContext();

		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			MenuItem parent = new MenuItem();
			MenuItem child = new MenuItem();

			Assert.That(child.Parent, Is.Null);

			parent.MenuItems.Add(child);

			Assert.That(child.Parent, Is.EqualTo(parent));
		});
	}

	[Test]
	public async Task RemoveSelfFromParentOnDispose()
	{
		using var ctx = new WinzorTestContext();

		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			MenuItem parent = new MenuItem();
			MenuItem child = new MenuItem("Child");

			Assert.That(parent.MenuItems.Select(i => i.Text), Does.Not.Contain("Child"));

			parent.MenuItems.Add(child);

			Assert.That(parent.MenuItems.Select(i => i.Text), Does.Contain("Child"));

			child.Dispose();

			Assert.That(parent.MenuItems.Select(i => i.Text), Does.Not.Contain("Child"));
		});
	}

	[TestCase(true, 0, 3, TestName = "DisposeChildrenIfDisposing")]
	[TestCase(false, 3, 0, TestName = "DoNotDisposeChildrenIfNotDisposing")]
	public async Task DisposeChildrenOnlyCalledIfDisposing(bool disposing, int expectedChildren, int expectedDisposeCount)
	{
		using var ctx = new WinzorTestContext();

		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var parent = new MenuItemMock();
			var child1 = new MenuItem("Child1");
			var child2 = new MenuItem("Child2");
			var child3 = new MenuItem("Child3");
			var disposedCount = 0;

			child1.Disposed += (sender, args) => disposedCount++;
			child2.Disposed += (sender, args) => disposedCount++;
			child3.Disposed += (sender, args) => disposedCount++;

			parent.MenuItems.Add(child1);
			parent.MenuItems.Add(child2);
			parent.MenuItems.Add(child3);

			parent.DisposeMock(disposing);

			Assert.That(disposedCount, Is.EqualTo(expectedDisposeCount));
			Assert.That(parent.MenuItems.Count, Is.EqualTo(expectedChildren));
		});
	}

	[Test]
	public async Task PopUpEventShouldInvokeOnPopUp()
	{
		using var ctx = new WinzorTestContext();

		await ctx.WinzorDispatcher.InvokeAsync(() => {
			MenuItem menuItem = new MenuItem();

			bool isPopUp = false;

			menuItem.Popup += (s, args) =>
			{
				isPopUp = true;
			};

			((IWinzorMenuItem)menuItem).OnSelect();

			Assert.That(isPopUp, Is.True);
			Assert.That(menuItem.HandlesPopup, Is.True);
		});
	}

	[Test]
	public async Task ShouldNotAllowSameOrNullMenuItemToBeAdded()
	{
		using var ctx = new WinzorTestContext();

		MenuItem parent = null;
		MenuItem subItem = null;
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			parent = new MenuItem("parent");
			subItem = new MenuItem("subItem");
			parent.MenuItems.Add(subItem);
			Assert.That(parent.MenuItems.Count, Is.EqualTo(1));

			parent.MenuItems.Add(subItem);
			Assert.That(parent.MenuItems.Count, Is.EqualTo(1));

			subItem = null;
			Assert.Throws<ArgumentNullException>(() => parent.MenuItems.Add(subItem));
			Assert.That(parent.MenuItems.Count, Is.EqualTo(1));
		});
	}

	[Test]
	public async Task DisposeRecursiveMenuItem()
	{
		using var ctx = new WinzorTestContext();

		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var disposedCount = 0;

			var menuItem = new MenuItem("Recursive Menu Item");
			menuItem.Disposed += (sender, args) => disposedCount++;
			menuItem.MenuItems.Add(menuItem);

			var parent = new MenuItem();
			parent.MenuItems.Add(menuItem);

			Assert.DoesNotThrow(() => parent.Dispose());
			Assert.That(disposedCount, Is.EqualTo(1));
			Assert.That(parent.MenuItems.Count, Is.EqualTo(0));
		});
	}

	[Test]
	public async Task TestOneMenuItemOnlyBelongToOneMenuItemAtATime()
	{
		using var ctx = new WinzorTestContext();

		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			MenuItem parent1 = new MenuItem();
			MenuItem parent2 = new MenuItem();
			MenuItem child = new MenuItem("child");

			parent1.MenuItems.Add(child);
			Assert.That(child.Parent, Is.EqualTo(parent1));
			Assert.That(parent1.MenuItems.Count, Is.EqualTo(1));
			Assert.That(parent1.MenuItems.Select(i => i.Text).ToArray()[0], Is.EqualTo("child"));
			Assert.That(parent2.MenuItems.Count, Is.EqualTo(0));

			parent2.MenuItems.Add(child);
			Assert.That(child.Parent, Is.EqualTo(parent2));
			Assert.That(parent1.MenuItems.Count, Is.EqualTo(0));
			Assert.That(parent2.MenuItems.Count, Is.EqualTo(1));
			Assert.That(parent2.MenuItems.Select(i => i.Text).ToArray()[0], Is.EqualTo("child"));
		});
	}

	[Test]
	public async Task TestMenuItemLoadable()
	{
		using var ctx = new WinzorTestContext();

		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			MenuItem menuItem = new MenuItem();
			Assert.That(((IWinzorMenuItem)menuItem).Loadable, Is.False);

			menuItem.Select += DummyEventHandler();
			Assert.That(((IWinzorMenuItem)menuItem).Loadable, Is.True);
			menuItem.Select -= DummyEventHandler();
			Assert.That(((IWinzorMenuItem)menuItem).Loadable, Is.False);

			MenuItem subMenuItem = new MenuItem();
			menuItem.Popup += DummyEventHandler();
			menuItem.MenuItems.Add(subMenuItem);
			Assert.That(((IWinzorMenuItem)menuItem).Loadable, Is.True);

			menuItem.MenuItems.Remove(subMenuItem);
			Assert.That(((IWinzorMenuItem)menuItem).Loadable, Is.False);
		});

		static EventHandler DummyEventHandler()
		{
			return (_, __) => { };
		}
	}

	[Test]
	public async Task TestMenuItemClickable([Values] bool enabled, [Values] bool handlesClick)
	{
		using var ctx = new WinzorTestContext();

		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var menuItem = new MenuItem { Enabled = enabled };
			Assert.That(((IWinzorMenuItem)menuItem).Clickable, Is.False);

			if (handlesClick)
			{
				menuItem.Click += (_, _) => { };
			}
			Assert.That(((IWinzorMenuItem)menuItem).Clickable, Is.EqualTo(enabled && handlesClick));
		});
	}

	[Test]
	public async Task TestGetContextMenu()
	{
		using var ctx = new WinzorTestContext();

		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var contextMenu = new ContextMenu();
			contextMenu.MenuItems.Add(new MenuItem { Text = "Menu Item Do" });
			Assert.That(contextMenu.MenuItems.Count, Is.EqualTo(1));
			Assert.That(contextMenu.MenuItems[0].GetContextMenu(), Is.EqualTo(contextMenu));

			var menuItem = new MenuItem { Text = "Menu Item Re" };
			Assert.That(menuItem.GetContextMenu(), Is.EqualTo(null));

			var mainMenu = new MainMenu();
			mainMenu.MenuItems.Add(new MenuItem { Text = "Menu Item Mi" });
			Assert.That(mainMenu.MenuItems.Count, Is.EqualTo(1));
			Assert.That(mainMenu.MenuItems[0].GetContextMenu(), Is.EqualTo(null));
		});
	}

	[TestCase(0)]
	[TestCase(1)]
	public async Task TestMenuItem_Index_SetWithParent_GetReturnsExpected(int value)
	{
		using var ctx = new WinzorTestContext();

		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var parent = new MenuItem();
			var menuItem1 = new MenuItem();
			var menuItem2 = new MenuItem();

			parent.MenuItems.Add(menuItem1);
			parent.MenuItems.Add(menuItem2);

			menuItem1.Index = value;
			Assert.That(value, Is.EqualTo(menuItem1.Index));

			if (value == 0)
			{
				Assert.That(new MenuItem[] { menuItem1, menuItem2 }, Is.EqualTo(parent.MenuItems.Cast<MenuItem>()));
			}
			else
			{
				Assert.That(new MenuItem[] { menuItem2, menuItem1 }, Is.EqualTo(parent.MenuItems.Cast<MenuItem>()));
			}
		});
	}

	[TestCase(-1)]
	[TestCase(1)]
	public async Task TestMenuItem_Index_SetInvalid_ThrowsArgumentOutOfRangeException(int value)
	{
		using var ctx = new WinzorTestContext();

		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var parent = new MenuItem();
			var menuItem = new MenuItem();

			parent.MenuItems.Add(menuItem);

			Assert.Throws<ArgumentOutOfRangeException>(() => menuItem.Index = value);
		});
	}

	[Test]
	public async Task TestCloneMenu()
	{
		using var ctx = new WinzorTestContext();

		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var counterForMenuItemClick = 0;
			var counterForMenuItemPopup = 0;
			var counterForMenuItemSelect = 0;

			var menuItem = new MenuItem { Text = "Menu Item Re" };
			menuItem.MergeOrder = 3;
			menuItem.MergeType = MenuMerge.Replace;
			menuItem.Shortcut = Shortcut.AltF5;
			menuItem.ShowShortcut = true;
			menuItem.Text = "this is the menu item text hi";
			menuItem.Enabled = false;
			menuItem.Click += (_, _) => counterForMenuItemClick++;
			menuItem.Popup += (_, _) => counterForMenuItemPopup++;
			menuItem.Select += (_, _) => counterForMenuItemSelect++;

			var clone = menuItem.CloneMenu();

			Assert.That(clone.MergeOrder, Is.EqualTo(menuItem.MergeOrder));
			Assert.That(clone.MergeType, Is.EqualTo(menuItem.MergeType));
			Assert.That(clone.Shortcut, Is.EqualTo(menuItem.Shortcut));
			Assert.That(clone.ShowShortcut, Is.EqualTo(menuItem.ShowShortcut));
			Assert.That(clone.Text, Is.EqualTo(menuItem.Text));
			Assert.That(clone.Enabled, Is.EqualTo(menuItem.Enabled));

			Assert.That(counterForMenuItemClick, Is.EqualTo(0));
			clone.PerformClick();
			Assert.That(counterForMenuItemClick, Is.EqualTo(1));

			Assert.That(counterForMenuItemPopup, Is.EqualTo(0));
			clone.CallOnPopup(new EventArgs());
			Assert.That(counterForMenuItemPopup, Is.EqualTo(1));

			Assert.That(counterForMenuItemSelect, Is.EqualTo(0));
			clone.PerformSelect();
			Assert.That(counterForMenuItemSelect, Is.EqualTo(1));
		});
	}

	class MenuItemMock : MenuItem
	{
		public MenuItemMock()
		{
		}

		public void DisposeMock(bool disposing)
		{
			Dispose(disposing);
		}
	}
}
