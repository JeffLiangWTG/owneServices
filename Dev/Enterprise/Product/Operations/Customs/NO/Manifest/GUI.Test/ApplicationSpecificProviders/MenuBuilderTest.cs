using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.GUI.Testing;
using Enterprise.Customs.NO.Manifest.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Manifest.GUI.Testing;

[TestedType(typeof(MenuBuilder))]
sealed class MenuBuilderTest : TestCaseWithFactory
{
	public void TestHeaderType()
	{
		var header = Factory.New<AsycudaManifestHeader>();
		using var form = new ZForm(header);

		var menuBuilder = new MenuBuilder(header, form);
		AssertType<AsycudaManifestHeader>(menuBuilder.Header);
	}

	public void TestMenuCaption()
	{
		var header = Factory.New<AsycudaManifestHeader>();
		using var form = new ZForm(header);

		var menuBuilder = new MenuBuilder(header, form);
		AssertEquals("Menu Caption", "NO Manifest", menuBuilder.MenuCaption.EnglishText);
	}

	public void TestBuildMenu()
	{
		var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
		using var menu = new AsycudaMenuForTest(header);
		using var form = new ZForm(header);

		form.Menu.MenuItems.Add(menu);
		form.Show();
		menu.OnPopup(EventArgs.Empty);
		AssertEquals("MenuItems Count", 1, menu.MenuItems.Count);
	}

	public void TestBuildMenuSendToCustomsMenuItem()
	{
		var header = Factory.NewMoq<AsycudaManifestHeader>().Object;
		using var form = new ZForm(header);

		var menuBuilder = new MenuBuilder(header, form);
		var menuItems = menuBuilder.BuildMenu();

		var sendToCustomsMenuItem = menuItems.SingleOrDefault(m => m.CaptionResourceString?.Caption == "Send to Customs");
		AssertType<SendToCustomsMenuItem>(sendToCustomsMenuItem);
	}
}
