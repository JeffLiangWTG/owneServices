using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bunit;
using CargoWise.Blazor.Client.Integration.DependencyInjection;
using CargoWise.Blazor.Client.Integration.Menus;
using Moq;
using NUnit.Framework;
using WinzorTestFramework;

namespace WinzorFramework;
class MenuInteropIntegrationTest : Bunit.TestContext
{
	[Test]
	public async Task TestBigMenuSerializedSize()
	{
		Services.AddCargoWiseClient();
		using var ctx = new WinzorTestContext();

		Control control = null;
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			control = new Control();
		});

		var menuItem = new Mock<IWinzorMenuItem>();
		using var image = new Bitmap(2, 2);
		menuItem.Setup(m => m.Visible).Returns(true);
		menuItem.Setup(m => m.Image).Returns(image);
		menuItem.Setup(m => m.Font).Returns(Control.DefaultFont);

		var menuItems = new IWinzorMenuItem[100];
		for (var i = 0; i < menuItems.Length; i++)
		{
			menuItems[i] = menuItem.Object;
		}

		var menuInterop = new MenuInterop(control, MenuType.ContextMenu, null);
		var menuDisplayer = Services.GetService<IMenuDisplayer>();
		JSInterop.Setup<bool>(m => m is { Identifier: "checkFunctionExists" }).SetResult(true);
		JSInterop.Setup<string>().SetResult(nameof(MenuShowResultCode.Shown));

		var model = menuInterop.CreateInteropModel(menuItems, new Point(), ToolStripDropDownDirection.Default);
		await menuDisplayer.SendShowMenuRequestAsync(model, null, null);

		var menuJson = (string)JSInterop.Invocations.Last().Arguments[0];
		Assert.That(menuJson, Does.Contain(nameof(model.MenuItems)));
		Assert.That(menuJson, Has.Length.LessThanOrEqualTo(32350), "Menu sizes must be kept as low as possible. Consider optimising before changing this value");
	}
}
