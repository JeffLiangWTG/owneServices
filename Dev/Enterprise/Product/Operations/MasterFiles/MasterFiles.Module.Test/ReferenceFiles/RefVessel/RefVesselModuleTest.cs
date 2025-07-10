using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(RefVesselModule))]
	public class RefVesselModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.RefVessel;
		}

		public void TestModuleID()
		{
			using (RefVesselModule module = new RefVesselModule())
			{
				AssertEquals("ModuleID", ModuleIDs.RefVessel, module.ID);
			}
		}

		public void TestCheckpoints()
		{
			using (RefVesselModule module = new RefVesselModule())
			{
				AssertEquals("SecurityCheckpoint", Env.Security.Vessels, module.SecurityCheckpoint);
				AssertEquals("LicenseCheckPoint", Env.Licence.Core, module.LicenceCheckPoint);
			}
		}

		public void TestCSVImportMenuItemHasBeenAdded()
		{
			using (RefVesselModule module = new RefVesselModule())
			{
				bool result = false;
				foreach (MenuItem item in module.ToolBarButtons.FindByText("Actions").DropDownMenu.MenuItems.FindByText("Data Transfer").MenuItems)
				{
					if (item.Text.Equals("Import From CSV"))
					{
						result = true;
						break;
					}
				}
				Assert(result);
			}
		}

		public void TestDeniedPartyScreeningMenuAdded()
		{
			using (var module = new RefVesselModule())
			{
				AssertNotNull(module.FormActionMenu.FindByText("Screen", true));
			}
		}

		protected virtual RefVesselModule GetNewVesselModule()
		{
			return new RefVesselModule();
		}

		#region Properties

		[RequiresSTA]
		public void TestGetNewFilterControl()
		{
			using (RefVesselModuleForTest module = new RefVesselModuleForTest())
			{
				IFilterControl filterControl = module.NewFilterControl;
				Assert("Invalid type", filterControl is RefVesselFilterControl);
				filterControl.Dispose();
			}
		}

		public void TestGetNewGridCollection()
		{
			using (RefVesselModuleForTest module = new RefVesselModuleForTest())
			{
				IBusinessObjectCollection vesselsCollection = module.NewGridCollection;
				Assert("Invalid type", vesselsCollection is RefVesselCollection);
			}
		}

		public void TestGetNewFilterBusinessObject()
		{
			using (RefVesselModuleForTest module = new RefVesselModuleForTest())
			{
				FilterBusinessObject filterBusinessObject = module.NewFilterBusinessObject;
				Assert("Invalid type", filterBusinessObject is RefVesselFilterBusinessObject);
			}
		}

		#endregion
	}
}
