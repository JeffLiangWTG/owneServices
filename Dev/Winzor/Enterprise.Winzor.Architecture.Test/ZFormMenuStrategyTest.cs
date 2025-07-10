using System;
using System.Reflection;
using System.Threading.Tasks;
using Bunit;
using CargoWise.Blazor.Client.Integration.Messaging;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Environment;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using WinzorTestFramework;

namespace Enterprise.Winzor.Architecture.Test;

class ZFormMenuStrategyTest
{
	[Test]
	public async Task TestCopyFormToClipboard()
	{
		using var ctx = new EnterpriseTestContext();

		var windowServiceMock = new Mock<IWindowService>();
		windowServiceMock.Setup(f => f.ScreenShotAsync(It.IsAny<ScreenShotSetting>()));
		var cargowiseClientServices = MockCargoWiseClientServices.MakeMock(windowService: windowServiceMock.Object, jsRuntime: ctx.JSInterop.JSRuntime);

		var rendered = await ctx.RenderFormAsync(() =>
		{
			var factory = new BusinessObjectFactory();
			var form = new ZForm(factory.New<DummyBusinessObject>()) { ControllerID = DummyControllerIDs.Dummy };
			var actionMenuItemsProvider = form as IFileMenuItemsProvider;
			var actionsMenu = actionMenuItemsProvider.ActionsMenuItem;
			var menuItem = actionsMenu.MenuItems.FindByName(ZFormMenuStrategy.CopyFormToClipboardMenuItemName);
			menuItem.PerformClick();
			return form;
		}, cargowiseClientServices);

		Assert.That(windowServiceMock.Invocations.Count, Is.GreaterThanOrEqualTo(1));
		windowServiceMock.Verify(f => f.ScreenShotAsync(It.IsAny<ScreenShotSetting>()), Times.Once);
	}

	[Test]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1089:Do Not Use Assembly.GetEntryAssembly()", Justification = "null case handled")]
	public async Task TestCreateDesktopShortcut()
	{
		using var ctx = new EnterpriseTestContext();

		ctx.JSInterop.SetupVoid("form.saveShortcut", "file", "content");
		ZGuid pk = ZGuid.Empty;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var factory = new BusinessObjectFactory();
			var businessObject = factory.New<DummyBusinessObject>();
			pk = businessObject.PK;
			var form = new ZForm(businessObject) { ControllerID = DummyControllerIDs.Dummy };
			return form;
		});

		string expectedUrl = null;
		var form = rendered.GetForm();
		await form.InvokeWinzorDispatcherAsync(() =>
		{
			var actionMenuItemsProvider = form as IFileMenuItemsProvider;
			var actionsMenu = actionMenuItemsProvider.ActionsMenuItem;
			var menuItem = actionsMenu.MenuItems.FindByName(ZFormMenuStrategy.CreateDesktopShortcutName);
			menuItem.PerformClick();

			expectedUrl = ShowEditFormUrlHandler.Instance.Create(DummyControllerIDs.Dummy, pk);
		});

		var callInfo = ctx.JSInterop.VerifyInvoke("form.saveShortcut");

		var entryAssembly = Assembly.GetEntryAssembly();
		var expectedIconFile = (entryAssembly == null) ? "" : (new Uri(entryAssembly.Location).LocalPath);

		Assert.That(callInfo.Arguments.Count, Is.EqualTo(2));
		Assert.That(callInfo.Arguments[0], Is.EqualTo("DummyBizo.url"));
		Assert.That(callInfo.Arguments[1], Is.EqualTo(@"[InternetShortcut]
URL=" + expectedUrl + @"
IconIndex=0
IconFile=" + expectedIconFile + @"
"));
	}
}

