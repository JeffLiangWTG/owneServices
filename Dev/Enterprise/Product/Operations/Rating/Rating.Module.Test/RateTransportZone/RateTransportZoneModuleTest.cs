using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.Module.Testing
{
	[TestedType(typeof(RateTransportZoneModule))]
	public class RateTransportZoneModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.RateTransportZone;
		}

		public void TestAllowNewEdit()
		{
			using (var rateTransportZoneModule = new RateTransportZoneModule())
			{
				Assert(!rateTransportZoneModule.AllowNew);
				Assert(!rateTransportZoneModule.AllowEdit);
			}
		}

		public void TestActionsMenu()
		{
			using (var rateTransportZoneModule = new RateTransportZoneModule())
			{
				var newMenuItem = rateTransportZoneModule.FormActionMenu.FindByText("&New");
				var editMenuItem = rateTransportZoneModule.FormActionMenu.FindByText("&Edit");

				AssertNull(newMenuItem);
				AssertNull(editMenuItem);
			}
		}

		public void TestContextMenu()
		{
			using (var rateTransportZoneModule = new RateTransportZoneModule())
			{
				AssertNotNull(rateTransportZoneModule.DisplayGrid);
				AssertNotNull(rateTransportZoneModule.DisplayGrid.ContextMenu);

				var newMenuItem = rateTransportZoneModule.DisplayGrid.ContextMenu.MenuItems.FindByText("&New");
				var editMenuItem = rateTransportZoneModule.DisplayGrid.ContextMenu.MenuItems.FindByText("&Edit");

				AssertNull(newMenuItem);
				AssertNull(editMenuItem);
			}
		}
	}
}
