using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Winzor.Architecture.Test;

class EmbeddedModulePopupTest
{
	[Test]
	[WinFormsWinzorTest]
	public void TestShouldNotCrashWhenSearchOnStripsInitialized()
	{
		using var form = new ZForm();
		using (var findBox = new ZCodeFindBox { ModuleID = DummyModuleIDs.Dummy })
		{
			using (var dummyModule = new DummyFilterGridModule())
			using (var popup = new EmbeddedModulePopup(dummyModule))
			{
				var filterControl = (DummyFilterControl)dummyModule.EmbeddedControl;
				filterControl.ShouldRunSearchOnStripsInitialized = true;
				popup.ShowModal(findBox, form);
			}
		}
	}
}
