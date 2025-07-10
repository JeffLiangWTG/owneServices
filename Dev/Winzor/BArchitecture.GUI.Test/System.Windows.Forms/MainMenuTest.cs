using System.Linq;
using System.Threading.Tasks;
using Bunit;
using CargoWise.Blazor.Client.Integration.Menus;
using Moq;
using NUnit.Framework;
using WinzorFramework;
using WinzorTestFramework;

namespace System.Windows.Forms;

class MainMenuTest
{
	TestMenuDisplayer menuDisplayer;
	CargoWiseClientServices clientServices;
	[SetUp]
	public void Setup()
	{
		menuDisplayer = new TestMenuDisplayer();
		clientServices = MockCargoWiseClientServices.MakeMock(menuDisplayer: menuDisplayer);
	}

	[Test]
	public async Task MenuHasItems()
	{
		Assert.That((await GetMenuItemsAsync()).Count, Is.GreaterThan(0));
	}

	[Test]
	public async Task MenuHasSubItems()
	{
		Assert.That((await GetMenuItemsAsync()).First().SubMenuItems[2].SubMenuItems.Count, Is.GreaterThan(0));
	}

	[Test]
	public async Task MenuItemsAreCorrect()
	{
		var menuItems = await GetMenuItemsAsync();
		for (int x = 0; x < 5; x++)
		{
			Assert.That(menuItems[x].Text, Is.EqualTo($"{x}"));
			for (int y = 0; y < 5; y++)
			{
				Assert.That(menuItems[x].SubMenuItems[y].Text, Is.EqualTo($"{x} - {y}"));
				if (y == 2)
				{
					for (int z = 0; z < 5; z++)
					{
						Assert.That(menuItems[x].SubMenuItems[y].SubMenuItems[z].Text, Is.EqualTo($"{x} - {y} - {z}"));
					}
				}
			}
		}
	}

	[Test]
	public async Task LoadFixedSubmenuItemsRepeatedly()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			form.Menu = new MainMenu();
			var test = form.Menu.MenuItems.Add("Test");
			test.MenuItems.Add("Item 1");
			test.MenuItems.Add("Item 2");
			return form;
		}, clientServices, OpenFormAction.BlockUntilShown);
		var testMenu = menuDisplayer.Menu.MenuItems.Single(m => m.Text == "Test");
		var subMenuInterop = await menuDisplayer.LoadSubMenuCallback(new SubMenuLoadRequest(menuDisplayer.Menu.Id.Value, testMenu.Id.Value));
		Assert.That(subMenuInterop.Select(o => o.Text), Is.EquivalentTo(new[] { "Item 1", "Item 2" }));
		var subMenuInterop2 = await menuDisplayer.LoadSubMenuCallback(new SubMenuLoadRequest(menuDisplayer.Menu.Id.Value, testMenu.Id.Value));
		Assert.That(subMenuInterop2.Select(o => o.Text), Is.EquivalentTo(new[] { "Item 1", "Item 2" }));
	}

	[Test]
	public async Task LoadDynamicSubmenuItem()
	{
		var dynamicClicked = false;
		using var ctx = new WinzorTestContext();
		await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			form.Menu = new MainMenu();
			var test = form.Menu.MenuItems.Add("Test");
			test.MenuItems.Add("Placeholder");
			test.Select += (s, e) =>
			{
				test.MenuItems.Clear();
				var dynamic = test.MenuItems.Add("Dynamic");
				dynamic.Click += (s, e) => dynamicClicked = true;
			};
			return form;
		}, clientServices, OpenFormAction.BlockUntilShown);
		var testMenu = menuDisplayer.Menu.MenuItems.Single(m => m.Text == "Test");
		Assert.That(testMenu.Loadable, Is.True);
		var subMenuInterop = await menuDisplayer.LoadSubMenuCallback(new SubMenuLoadRequest(menuDisplayer.Menu.Id.Value, testMenu.Id.Value));
		Assert.That(subMenuInterop.Select(o => o.Text), Is.EquivalentTo(new[] { "Dynamic" }));
		await menuDisplayer.MenuClosedCallback(new MenuClosedResult(MenuClosedResultCode.SelectionMade, menuDisplayer.Menu.Id.Value, subMenuInterop.Single().Id.Value));
		Assert.That(dynamicClicked, Is.True);
	}

	[Test]
	public async Task LoadDynamicSubmenuItemWithExisting()
	{
		using var ctx = new WinzorTestContext();
		await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			form.Menu = new MainMenu();
			var test = form.Menu.MenuItems.Add("Test");
			test.MenuItems.Add("Permanent");
			test.Select += (s, e) =>
			{
				test.MenuItems.Add("Dynamic");
			};
			return form;
		}, clientServices, OpenFormAction.BlockUntilShown);
		var testMenu = menuDisplayer.Menu.MenuItems.Single(m => m.Text == "Test");
		var subMenuInterop = await menuDisplayer.LoadSubMenuCallback(new SubMenuLoadRequest(menuDisplayer.Menu.Id.Value, testMenu.Id.Value));
		Assert.That(subMenuInterop.Select(o => o.Text), Is.EquivalentTo(new[] { "Permanent", "Dynamic" }));
		Assert.That(subMenuInterop.Single(m => m.Text == "Permanent").Id, Is.EqualTo(testMenu.SubMenuItems.Single(m => m.Text == "Permanent").Id));
	}

	[Test]
	public async Task NoDynamicSubmenuItems()
	{
		using var ctx = new WinzorTestContext();
		await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			form.Menu = new MainMenu();
			var test = form.Menu.MenuItems.Add("Test");
			test.MenuItems.Add("Whatever");
			test.Select += (s, e) =>
			{
			};
			return form;
		}, clientServices, OpenFormAction.BlockUntilShown);
		var testMenu = menuDisplayer.Menu.MenuItems.Single(m => m.Text == "Test");
		var subMenuInterop = await menuDisplayer.LoadSubMenuCallback(new SubMenuLoadRequest(menuDisplayer.Menu.Id.Value, testMenu.Id.Value));
		Assert.That(subMenuInterop.Select(o => o.Text), Is.EqualTo(new[] { "Whatever" }));
	}

	[Test]
	public async Task NonVisibleMenuItem()
	{
		using var ctx = new WinzorTestContext();
		await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			form.Menu = new MainMenu();
			var test = form.Menu.MenuItems.Add("Test");
			test.MenuItems.Add("Invisible").Visible = false;
			test.MenuItems.Add("Visible").Visible = true;
			return form;
		}, clientServices, OpenFormAction.BlockUntilShown);
		var testMenu = menuDisplayer.Menu.MenuItems.Single(m => m.Text == "Test");
		Assert.That(testMenu.SubMenuItems.Select(m => m.Text), Is.EquivalentTo(new[] { "Visible" }));
	}

	[Test]
	public async Task SetVisibleFalseOnSlect()
	{
		using var ctx = new WinzorTestContext();
		await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			form.Menu = new MainMenu();
			var test = form.Menu.MenuItems.Add("Test");
			var invisible = test.MenuItems.Add("Invisible");
			test.MenuItems.Add("Visible");
			test.Select += (s, e) =>
			{
				invisible.Visible = false;
			};
			return form;
		}, clientServices, OpenFormAction.BlockUntilShown);
		var testMenu = menuDisplayer.Menu.MenuItems.Single(m => m.Text == "Test");
		var subMenuInterop = await menuDisplayer.LoadSubMenuCallback(new SubMenuLoadRequest(menuDisplayer.Menu.Id.Value, testMenu.Id.Value));
		Assert.That(subMenuInterop.Select(o => o.Text), Is.EquivalentTo(new[] { "Visible" }));
	}

	[Test]
	public async Task RerenderMenuAfterRootMenuItemsAreUpdated()
	{
		MenuItem test = null;
		Form form = null;
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			form = new Form();
			form.Menu = new MainMenu();
			test = form.Menu.MenuItems.Add("Test");
			return form;
		}, clientServices, OpenFormAction.BlockUntilShown);
		Assert.That(menuDisplayer.ShowMenuRequestCount, Is.EqualTo(1));
		var testMenu = menuDisplayer.Menu.MenuItems.Single(m => m.Text == "Test");
		Assert.That(testMenu.Enabled, Is.True);

		await test.InvokeWinzorDispatcherAsync(() =>
		{
			test.Enabled = false;
		});
		Assert.That(menuDisplayer.ShowMenuRequestCount, Is.EqualTo(2));
		rendered.WaitForState(() => !menuDisplayer.Menu.MenuItems.Single(m => m.Text == "Test").Enabled);
		await test.InvokeWinzorDispatcherAsync(() =>
		{
			test.Text = "Text Changed";
		});
		Assert.That(menuDisplayer.ShowMenuRequestCount, Is.EqualTo(3));
		rendered.WaitForState(() => menuDisplayer.Menu.MenuItems.Single(m => m.Text == "Text Changed") != null);

		MenuItem added = null;
		MenuItem[] subItems = null;
		bool clickFlag = false;
		await form.InvokeWinzorDispatcherAsync(() =>
		{
			subItems = new MenuItem[2];
			subItems[0] = new MenuItem("Sub Menu Test 1")
			{
				Enabled = false,
				Shortcut = Shortcut.CtrlH,
			};
			subItems[0].Click += (e, args) => { clickFlag = true; };
			subItems[1] = new MenuItem("Sub Menu Test 2");
			added = form.Menu.MenuItems.Add("Add", subItems);
		});
		Assert.That(menuDisplayer.ShowMenuRequestCount, Is.EqualTo(4));
		rendered.WaitForState(() => menuDisplayer.Menu.MenuItems.Length == 2);
		Assert.That(menuDisplayer.Menu.MenuItems.Select(o => o.Text), Is.EquivalentTo(new[] { "Text Changed", "Add" }));
		Assert.That(menuDisplayer.Menu.MenuItems[1].SubMenuItems[0].Enabled, Is.EqualTo(false));
		var formElement = rendered.FindAll("div")[0];
		await rendered.KeyPressAsync(Keys.Control | Keys.H, formElement);
		Assert.That(clickFlag, Is.EqualTo(true));

		await subItems[0].InvokeWinzorDispatcherAsync(() =>
		{
			subItems[0].Enabled = true;
		});
		Assert.That(menuDisplayer.ShowMenuRequestCount, Is.EqualTo(4));

		await subItems[1].InvokeWinzorDispatcherAsync(() =>
		{
			subItems[1].Text = "Sub Menu Test Change";
		});
		Assert.That(menuDisplayer.ShowMenuRequestCount, Is.EqualTo(4));

		await form.InvokeWinzorDispatcherAsync(() =>
		{
			form.Menu.MenuItems.Remove(added);
		});
		Assert.That(menuDisplayer.ShowMenuRequestCount, Is.EqualTo(5));
		rendered.WaitForState(() => menuDisplayer.Menu.MenuItems.Length == 1);
		Assert.That(menuDisplayer.Menu.MenuItems.Select(o => o.Text), Is.EquivalentTo(new[] { "Text Changed" }));
	}

	[Test]
	public async Task TopLevelMenuNotRerenderedWhenSubmenuChanges()
	{
		using var ctx = new WinzorTestContext();
		MenuItem test = null;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			form.Menu = new MainMenu();
			test = form.Menu.MenuItems.Add("Test");
			return form;
		}, clientServices, OpenFormAction.BlockUntilShown);
		Assert.That(menuDisplayer.ShowMenuRequestCount, Is.EqualTo(1));

		await test.InvokeWinzorDispatcherAsync(() =>
		{
			test.MenuItems.Add("Child");
		});
		Assert.That(menuDisplayer.ShowMenuRequestCount, Is.EqualTo(1));

		await test.InvokeWinzorDispatcherAsync(() =>
		{
			test.MenuItems.Single().Text = "Child Changed";
		});
		Assert.That(menuDisplayer.ShowMenuRequestCount, Is.EqualTo(1));

		await test.InvokeWinzorDispatcherAsync(() =>
		{
			test.MenuItems.Single().Enabled = false;
		});
		Assert.That(menuDisplayer.ShowMenuRequestCount, Is.EqualTo(1));

		await test.InvokeWinzorDispatcherAsync(() =>
		{
			test.MenuItems.Single().Visible = false;
		});
		Assert.That(menuDisplayer.ShowMenuRequestCount, Is.EqualTo(1));

		await test.InvokeWinzorDispatcherAsync(() =>
		{
			test.MenuItems.Single().Visible = true;
		});
		Assert.That(menuDisplayer.ShowMenuRequestCount, Is.EqualTo(1));

		await test.InvokeWinzorDispatcherAsync(() =>
		{
			test.MenuItems.Clear();
		});
		Assert.That(menuDisplayer.ShowMenuRequestCount, Is.EqualTo(1));
	}

	[Test]
	public async Task TestMainMenuShortCutKeys()
	{
		MenuItem test = null;
		Form form = null;
		bool clickFlag = false;
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			form = new Form();
			form.Menu = new MainMenu();
			test = form.Menu.MenuItems.Add("Test");
			test.Shortcut = Shortcut.CtrlH;
			test.Click += (e, args) => { clickFlag = true; };
			return form;
		}, clientServices, OpenFormAction.BlockUntilShown);

		var formElement = rendered.FindAll("div")[0];
		await rendered.KeyPressAsync(Keys.Control | Keys.H, formElement);
		Assert.That(clickFlag, Is.EqualTo(true));
	}

	[TestCase(Keys.Alt | Keys.T, false, true)]
	[TestCase(Keys.Alt | Keys.F, true, false)]
	[TestCase(Keys.Alt | Keys.A, false, false)]
	[TestCase(Keys.Control | Keys.T, false, false)]
	public async Task TestMainMenuMnemonics_WhenMenuItemHasNoChildren(Keys key, bool expectAltF, bool expectAltT)
	{
		var menuDisplayerMock = new Mock<IMenuDisplayer>();
		var clientServices = MockCargoWiseClientServices.MakeMock(menuDisplayer: menuDisplayerMock.Object);

		var clickAltF = false;
		var clickAltT = false;

		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			form.Menu = new MainMenu();
			form.Menu.MenuItems
				.Add("&First")
				.Click += (e, args) => { clickAltF = true; };
			form.Menu.MenuItems
				.Add("&Test")
				.Click += (e, args) => { clickAltT = true; };
			return form;
		}, clientServices, OpenFormAction.BlockUntilShown);

		var formElement = rendered.FindAll("div")[0];
		await rendered.KeyPressAsync(key, formElement);

		Assert.That(clickAltF, Is.EqualTo(expectAltF));
		Assert.That(clickAltT, Is.EqualTo(expectAltT));
		menuDisplayerMock.Verify(m => m.SendShowDropDownRequestAsync(It.IsAny<Guid>()), Times.Never);
	}

	[Test]
	public async Task TestMainMenuMnemonics_WhenMenuItemHasChildren()
	{
		var menuDisplayerMock = new Mock<IMenuDisplayer>();
		var clientServices = MockCargoWiseClientServices.MakeMock(menuDisplayer: menuDisplayerMock.Object);

		MenuItem menuItem = null;
		var clickFlag = false;

		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			form.Menu = new MainMenu();
			form.Menu.MenuItems.Add("&First");
			menuItem = form.Menu.MenuItems.Add("&Test");
			menuItem.MenuItems.Add("Test 1");
			menuItem.MenuItems.Add("Test 2");
			menuItem.Click += (e, args) => { clickFlag = true; };
			return form;
		}, clientServices, OpenFormAction.BlockUntilShown);

		menuDisplayerMock
			.Setup(m => m.SendShowDropDownRequestAsync(menuItem.WinzorControlGuid))
			.ReturnsAsync(MenuDropDownResultCode.Shown);

		var formElement = rendered.FindAll("div")[0];
		await rendered.KeyPressAsync(Keys.Alt | Keys.T, formElement);

		Assert.That(clickFlag, Is.False);
		menuDisplayerMock.Verify(m => m.SendShowDropDownRequestAsync(menuItem.WinzorControlGuid), Times.AtLeastOnce());
	}

	[Test]
	public async Task TestMainMenuMnemonics_WhenMenuItemHasChildren_WhenNotMnemonic()
	{
		var menuDisplayerMock = new Mock<IMenuDisplayer>();
		var clientServices = MockCargoWiseClientServices.MakeMock(menuDisplayer: menuDisplayerMock.Object);

		var clickFlag = false;

		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			form.Menu = new MainMenu();
			form.Menu.MenuItems.Add("&First");
			var menuItem = form.Menu.MenuItems.Add("&Test");
			menuItem.MenuItems.Add("Test 1");
			menuItem.MenuItems.Add("Test 2");
			menuItem.Click += (e, args) => { clickFlag = true; };
			return form;
		}, clientServices, OpenFormAction.BlockUntilShown);

		var formElement = rendered.FindAll("div")[0];
		await rendered.KeyPressAsync(Keys.Alt | Keys.A, formElement);

		Assert.That(clickFlag, Is.False);
		menuDisplayerMock.Verify(m => m.SendShowDropDownRequestAsync(It.IsAny<Guid>()), Times.Never);
	}

	async Task<CargoWiseClientServices> FormWithMenu(EventHandler callBack = null)
	{
		using var ctx = new WinzorTestContext();
		var clientServices = MockCargoWiseClientServices.MakeMock();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			form.Menu = new MainMenu();
			for (int x = 0; x < 5; x++)
			{
				var menuItemX = new MenuItem($"{x}");
				for (int y = 0; y < 5; y++)
				{
					var menuItemY = x == 0 && y == 0 ? new MenuItem($"{x} - {y}", callBack) : new MenuItem($"{x} - {y}");
					menuItemX.MenuItems.Add(menuItemY);

					if (y == 2)
					{
						for (int z = 0; z < 5; z++)
						{
							menuItemY.MenuItems.Add(new MenuItem($"{x} - {y} - {z}"));
						}
					}
				}
				form.Menu.MenuItems.Add(menuItemX);
			}

			return form;
		}, clientServices);

		return clientServices;
	}

	async Task<MenuItemInteropModel[]> GetMenuItemsAsync()
	{
		var clientServices = await FormWithMenu();
		var menuDisplayer = Mock.Get(clientServices.MenuDisplayer);
		return ((MenuInteropModel)menuDisplayer.Invocations[0].Arguments[0]).MenuItems;
	}
}
