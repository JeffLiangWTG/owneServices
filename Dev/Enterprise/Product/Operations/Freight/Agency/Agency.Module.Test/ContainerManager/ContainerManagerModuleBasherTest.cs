using System.Windows.Forms;
using Enterprise.Environment;
using Enterprise.Freight.Agency.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Module.Testing
{
	[TestedType(typeof(ContainerManagerModule))]
	internal class ContainerManagerModuleBasherTest : ZModuleBasherTest
	{
		public void TestLicence()
		{
			using (ContainerManagerModule module = new ContainerManagerModule())
			{
				AssertEquals("Licence", Env.Licence.ShippingManagerContainerControl, module.LicenceCheckPoint);
			}
		}

		public void TestAddBulkMovements()
		{
			using (ContainerManagerModule module = new ContainerManagerModule())
			{
				MenuItem item = MenuAssertion.AssertHasMenu(module.FormActionMenu, "&Actions", "Add Bulk Movements");
				item.PerformClick();
				using (Form form = module.LastShownBulkMovementsFormForTest)
				{
					module.LastShownBulkMovementsFormForTest = null;
					AssertType(typeof(BulkMovementsForm), form);
					AssertEquals(true, form.Visible);
					Application.DoEvents();
				}
			}
		}

		#region Implementation
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.AgencyContainerManager;
		}
		#endregion
	}
}
