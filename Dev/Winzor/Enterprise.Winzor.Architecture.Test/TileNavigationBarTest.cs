using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bunit;
using CargoWise.Blazor.Client.Integration.Menus;
using CargoWise.Main.Navigation;
using Enterprise.Core.Modules;
using Enterprise.ZArchitecture.Favorites;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Moq;
using NUnit.Framework;
using WinzorTestFramework;

namespace Enterprise.Winzor.Architecture.Test;

internal sealed class TileNavigationBarTest
{
	[Test]
	public async Task TileNavigationBarContextMenuOpensAtExpectedPosition()
	{
		using var ctx = new EnterpriseTestContext();
		var mockMenuDisplayer = new Mock<IMenuDisplayer>();
		mockMenuDisplayer.Setup(o => o.SendShowMenuRequestAsync(It.IsAny<MenuInteropModel>(), It.IsAny<Func<SubMenuLoadRequest, Task<MenuItemInteropModel[]>>>(), It.IsAny<Func<MenuClosedResult, Task>>()));
		var clientServices = MockCargoWiseClientServices.MakeMock(menuDisplayer: mockMenuDisplayer.Object, jsRuntime: ctx.JSInterop.JSRuntime);

		var rendered = await ctx.RenderFormAsync(() => {
			var form = new Form();

			var tree = new ModuleTree();
			var dummyCategory = new ModuleCategory(ModuleTreeLoaderConstant.Category.Jump, null);
			var dummySection = new ModuleSection(ModuleTreeLoaderConstant.Section.Favorites, null, null, IconTypes.Blank, IconTypes.Blank);
			var dummyItem = new LinkWrapper("Dummy", Guid.NewGuid(), string.Empty, "description");
			dummySection.Modules.Add(new LinkMainFormModule(dummyItem));
			dummyCategory.Sections.Add(dummySection);
			tree.Categories.Add(dummyCategory);

			var tileNavigationBar = new TileNavigationBar() { Top = 100, Left = 100 };
			tileNavigationBar.LoadModuleTree(tree);
			tileNavigationBar.SelectedCategory = dummyCategory;
			form.Controls.Add(tileNavigationBar);
			return form;
		}, clientServices);

		var link = rendered.WaitForElement(".tilebarcontrol__sectionitemlink");
		await link.ClickAsync(new WebMouseEventArgs()
		{
			Button = 2,
			ClientX = 123,
			ClientY = 456,
		});

		Assert.That(mockMenuDisplayer.Invocations.Count, Is.EqualTo(1));
		var menuInteropModel = (MenuInteropModel)mockMenuDisplayer.Invocations[0].Arguments[0];
		Assert.That(menuInteropModel.X, Is.EqualTo(123));
		Assert.That(menuInteropModel.Y, Is.EqualTo(456));
	}
}
