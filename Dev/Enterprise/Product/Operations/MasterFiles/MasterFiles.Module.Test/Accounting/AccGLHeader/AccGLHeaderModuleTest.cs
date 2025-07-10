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
	[TestedType(typeof(AccGLHeaderModule))]
	sealed class AccGLHeaderModuleTest : ZModuleBasherTest
	{
		public AccGLHeaderModuleTest()
		{
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.AccGLHeader;
		}

		protected override string CountryCode
		{
			get { return "AU"; }
		}

		public void TestCheckpoints()
		{
			using (AccGLHeaderModule module = new AccGLHeaderModule())
			{
				AssertEquals("SecurityCheckpoint", Env.Security.GLAccounts, module.SecurityCheckpoint);
				AssertEquals("LicenseCheckPoint", Env.Licence.Core, module.LicenceCheckPoint);
			}
		}

		public void TestMenuItems()
		{
			using (AccGLHeaderModule module = new AccGLHeaderModule())
			{
				AssertNotNull(module.ToolBarButtons.FindByText("View"));
				AssertNotNull(module.ToolBarButtons.FindByText("New"));
				AssertNotNull(module.ToolBarButtons.FindByText("Edit"));
				AssertNotNull(module.ToolBarButtons.FindByText("Delete"));
				ToolBarButton actionsButton = module.ToolBarButtons.FindByText("&Actions");
				AssertNotNull(actionsButton);
				AssertNotNull(actionsButton.DropDownMenu.MenuItems.FindByText("D&ata Transfer"));
				AssertNotNull(actionsButton.DropDownMenu.MenuItems.FindByText("Define General Ledger Sections"));
			}
		}

		#region Properties

		[RequiresSTA]
		public void TestGetNewFilterControl()
		{
			using (AccGLHeaderModuleForTest module = new AccGLHeaderModuleForTest())
			{
				IFilterControl filterControl = module.NewFilterControl;
				Assert("Invalid type", filterControl is AccGLHeaderFilterControl);
				filterControl.Dispose();
			}
		}

		public void TestGetNewGridCollection()
		{
			using (AccGLHeaderModuleForTest module = new AccGLHeaderModuleForTest())
			{
				IBusinessObjectCollection glHeadersCollection = module.NewGridCollection;
				Assert("Invalid type", glHeadersCollection is AccGLHeaderCollection);
			}
		}

		public void TestGetNewFilterBusinessObject()
		{
			using (AccGLHeaderModuleForTest module = new AccGLHeaderModuleForTest())
			{
				FilterBusinessObject filterBusinessObject = module.NewFilterBusinessObject;
				Assert("Invalid type", filterBusinessObject is AccGLHeaderFilterBusinessObject);
			}
		}

		#endregion
	}
}
