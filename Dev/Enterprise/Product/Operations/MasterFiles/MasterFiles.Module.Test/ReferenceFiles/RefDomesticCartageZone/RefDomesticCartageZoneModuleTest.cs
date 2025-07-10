using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(RefDomesticCartageZoneModule))]
	sealed class RefDomesticCartageZoneModuleTest : ZModuleBasherTest
	{
		public void TestACIImportMenuItem()
		{
			using (var module = new RefDomesticCartageZoneModule())
			{
				bool found = false;
				foreach (MenuItem item in module.ToolBarButtons.FindByText("Actions").FindByText("D&ata Transfer").MenuItems)
				{
					if (item.Text.Equals("Import US/Canada ACI Zone Information"))
					{
						found = true;
						break;
					}
				}
				Assert(found);
			}
		}

		public void TestCheckpoints()
		{
			using (var module = new RefDomesticCartageZoneModule())
			{
				AssertEquals("SecurityCheckpoint", Env.Security.RefDomesticCartageZone, module.SecurityCheckpoint);
				AssertEquals("LicenseCheckPoint", Env.Licence.Core, module.LicenceCheckPoint);
			}
		}
		public void TestAllows()
		{
			using (var module = new RefDomesticCartageZoneModule())
			{
				AssertEquals(module.AllowNew, false);
				AssertEquals(module.AllowEdit, false);
				AssertEquals(module.AllowView, false);
			}
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.RefDomesticCartageZone;
		}
	}
}
