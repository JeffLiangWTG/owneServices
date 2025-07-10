using System.Threading.Tasks;
using NUnit.Framework;
using WinzorTestFramework;

namespace System.Windows.Forms;

class MenuTest
{
	[Test]
	public async Task TestIsParent()
	{
		using var ctx = new WinzorTestContext();

		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var menu = new Menu();
			Assert.That(menu.IsParent, Is.EqualTo(false));

			menu.MenuItems.Add(new MenuItem("test item"));
			Assert.That(menu.IsParent, Is.EqualTo(true));
		});
	}

	[Test]
	public async Task TestIsNameNotNull()
	{
		using var ctx = new WinzorTestContext();

		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var menu = new Menu();
			Assert.That(menu.Name, Is.Not.Null);
		});
	}
}
