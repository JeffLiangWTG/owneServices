using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.SG.Access.GUI.Testing
{
	[TestedType(typeof(ManifestModule))]
	public class ManifestModuleTest : ASYCUDA.Module.Testing.ASYCUDAManifestBillModuleAbstractTest
	{
		public void TestNewManifestDropDownCountryList()
		{
			using (var module = new ManifestModuleForTest())
			{
				var menuItems = module.GetNewStandardMenuItems();
				var newMenuItem = menuItems.FindByText("&New Manifest");
				AssertNotNull("New menu item", newMenuItem);
				AssertEquals(IconTypes.NewButtonActive, (newMenuItem as ZMenuItem).ActiveIcon);
				AssertEquals(IconTypes.NewButtonRest, (newMenuItem as ZMenuItem).RestIcon);
				newMenuItem = menuItems.FindByText("&New Forwarder Manifest");
				AssertNull(newMenuItem);
				newMenuItem = menuItems.FindByText("&New Carrier Manifest");
				AssertNull(newMenuItem);
			}
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.ASYCUDA.SGAccess.Manifest;
	}
}
