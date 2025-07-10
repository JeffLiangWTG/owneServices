using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.Customs.US.eManifest.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;
using BusinessContext = CargoWise.Definitions.BusinessContext;

namespace Enterprise.Customs.US.eManifest.Module.Testing
{
	[TestedType(typeof(eManifestModule))]
	public sealed class eManifestModuleBasherTest : ZModuleBasherTest
	{
		public void TestLicenceAndSecurityCheckPoint()
		{
			using (var module = new eManifestModule())
			{
				AssertEquals("Licence Checkpoint", Env.Licence.Core, module.LicenceCheckPoint);
				AssertEquals("SecurityCheckpoint", Env.Security.USeManifest, module.SecurityCheckpoint);
			}
		}

		public void TestBusinessContexts()
		{
			using (eManifestModule module = new eManifestModule())
			{
				AssertEquals("Only one business context should be returned", 1, module.BusinessContexts.Length);
				AssertEquals("eManifest business context should be returned", BusinessContext.eManifest, module.BusinessContexts[0]);
				Assert("Business context array should be returned", module.BusinessContexts is BusinessContext[]);
			}
		}

		public void TestAllows()
		{
			using (var module = new eManifestModule())
			{
				AssertEquals("module.AllowNew", true, module.AllowNew);
				AssertEquals("module.AllowEdit", true, module.AllowEdit);
				AssertEquals("module.AllowDelete", true, module.AllowDelete);
				AssertEquals("module.SupportsWorkflow", true, module.SupportsWorkflow);
			}
		}

		public void TestGetNewFilterControlForGrid()
		{
			var glbCompany = Factory.NewWithValidTestData<GlbCompany>();
			glbCompany.GC_Code = "t1";
			glbCompany.GC_RN_NKCountryCode = "US";
			var glbCompany2 = Factory.NewWithValidTestData<GlbCompany>();
			glbCompany2.GC_Code = "t2";
			glbCompany2.GC_RN_NKCountryCode = "US";
			var branch = glbCompany.Branches.AddNew();
			branch.GB_GC = glbCompany.PK;
			branch.GB_Code = "Ts1";
			var branch2 = glbCompany.Branches.AddNew();
			branch2.GB_GC = glbCompany.PK;
			branch2.GB_Code = "Ts2";
			var branch3 = glbCompany.Branches.AddNew();
			branch3.GB_GC = glbCompany2.PK;
			branch3.GB_Code = "Ts3";
			var trip1 = Factory.NewWithValidTestData<Trip>();
			trip1.BH_GB = branch.PK;
			trip1.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.eManifest;
			var trip2 = Factory.NewWithValidTestData<Trip>();
			trip2.BH_GB = branch2.PK;
			trip2.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.eManifest;
			var trip3 = Factory.NewWithValidTestData<Trip>();
			trip3.BH_GB = branch3.PK;
			trip3.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.eManifest;
			Factory.Save();
			using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
			using (var module = new eManifestModule())
			using (var form = new ZForm())
			{
				Enterprise.ZArchitecture.Environment.EnvProxy.Instance.Registry.RunSearchOnEnteringAModule = true;
				var filterControl = (ZFilterStripControl)module.EmbeddedControl;
				var grid = filterControl.FilteredGrid;
				form.Controls.Add(filterControl);
				form.Show();
				System.Windows.Forms.Application.DoEvents();
				AssertEquals(branch.PK, GlbBranch.CurrentBranch.PK);
				AssertEquals(2, module.GridCollection.Count);
				AssertEquals(true, module.GridCollection.Contains(trip2.PK));
				AssertEquals(false, module.GridCollection.Contains(trip3.PK));
			}
		}

		public void TestMenuCreateJobDeclaration()
		{
			var glbCompany = Factory.NewWithValidTestData<GlbCompany>();
			glbCompany.GC_Code = "t1";
			glbCompany.GC_RN_NKCountryCode = "US";
			var glbCompany2 = Factory.NewWithValidTestData<GlbCompany>();
			glbCompany2.GC_Code = "t2";
			glbCompany2.GC_RN_NKCountryCode = "US";
			var branch = glbCompany.Branches.AddNew();
			branch.GB_GC = glbCompany.PK;
			branch.GB_Code = "Ts1";
			var branch2 = glbCompany.Branches.AddNew();
			branch2.GB_GC = glbCompany.PK;
			branch2.GB_Code = "Ts2";
			var branch3 = glbCompany.Branches.AddNew();
			branch3.GB_GC = glbCompany2.PK;
			branch3.GB_Code = "Ts3";
			var trip1 = Factory.NewWithValidTestData<Trip>();
			trip1.BH_GB = branch.PK;
			trip1.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.eManifest;
			var trip2 = Factory.NewWithValidTestData<Trip>();
			trip2.BH_GB = branch2.PK;
			trip2.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.eManifest;
			var trip3 = Factory.NewWithValidTestData<Trip>();
			trip3.BH_GB = branch3.PK;
			trip3.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.eManifest;
			Factory.Save();
			using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
			using (var module = new eManifestModule())
			using (var form = new ZForm())
			{
				Enterprise.ZArchitecture.Environment.EnvProxy.Instance.Registry.RunSearchOnEnteringAModule = true;
				var filterControl = (ZFilterStripControl)module.EmbeddedControl;
				var grid = filterControl.FilteredGrid;
				form.Controls.Add(filterControl);
				form.Show();
				System.Windows.Forms.Application.DoEvents();
				AssertEquals(branch.PK, GlbBranch.CurrentBranch.PK);
				AssertEquals(2, module.GridCollection.Count);
				AssertEquals(true, module.GridCollection.Contains(trip2.PK));
				AssertEquals(false, module.GridCollection.Contains(trip3.PK));
			}
		}

		public void TestActionMenuOperationalActions()
		{
			using (var module = GetModule())
			{
				var grid = module.DisplayGrid as ZDisplayGrid;
				var actionMenu = grid.ContextMenu.MenuItems.FindByText("Actions");
				actionMenu.ShowPopupMenu();
				var operationalActionsMenu = actionMenu.MenuItems.FindByText("Operational Actions");
				AssertNotNull(operationalActionsMenu);
			}
		}

		[ExpectNoExceptions]
		public void TestActionMenuOnTrip()
		{
			var glbCompany = Factory.NewWithValidTestData<GlbCompany>();
			glbCompany.GC_Code = "t1";
			glbCompany.GC_RN_NKCountryCode = "US";
			var glbCompany2 = Factory.NewWithValidTestData<GlbCompany>();
			glbCompany2.GC_Code = "t2";
			glbCompany2.GC_RN_NKCountryCode = "US";
			var branch = glbCompany.Branches.AddNew();
			branch.GB_GC = glbCompany.PK;
			branch.GB_Code = "Ts1";
			var branch2 = glbCompany.Branches.AddNew();
			branch2.GB_GC = glbCompany.PK;
			branch2.GB_Code = "Ts2";
			var branch3 = glbCompany.Branches.AddNew();
			branch3.GB_GC = glbCompany2.PK;
			branch3.GB_Code = "Ts3";
			var trip1 = Factory.NewWithValidTestData<Trip>();
			trip1.BH_GB = branch.PK;
			trip1.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.eManifest;
			var trip2 = Factory.NewWithValidTestData<Trip>();
			trip2.BH_GB = branch2.PK;
			trip2.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.eManifest;
			var trip3 = Factory.NewWithValidTestData<Trip>();
			trip3.BH_GB = branch3.PK;
			trip3.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.eManifest;
			Factory.Save();
			using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
			using (var module = new eManifestModuleForTesting())
			{
				Menu.MenuItemCollection menus = module.GetNewActionMenuItems().FindByText(eManifestModule.CreateNewDeclarationMenuName).MenuItems;
				AssertNotNull(menus);
				foreach (MenuItem menuItem in menus)
				{
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					menuItem.PerformClick();
				}
			}
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.US.eManifest;

		protected override bool HasController() => true;

		protected override void AddTestObjects(IBusinessObjectCollection collection)
		{
			var trip1 = Factory.NewWithValidTestData<Trip>();
			trip1.BH_GB = GlbBranch.CurrentBranch.PK;
			trip1.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.eManifest;
			Factory.Save();
		}

		sealed class eManifestModuleForTesting : eManifestModule
		{
			public new MenuItem[] GetNewActionMenuItems() => base.GetNewActionMenuItems();
		}
	}
}
