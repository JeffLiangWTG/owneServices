using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(OrgAddressesModule))]
	sealed class OrgAddressesModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.OrgAddresses;
		}

		public void TestCheckpoints()
		{
			using (OrgAddressesModule module = new OrgAddressesModule())
			{
				AssertEquals("SecurityCheckpoint", Env.Security.Organisation, module.SecurityCheckpoint);
				AssertEquals("LicenseCheckPoint", Env.Licence.Core, module.LicenceCheckPoint);
			}
		}

		#region Overrides

		public void TestOverrides()
		{
			using (var module = new OrgAddressesModuleForTest())
			{
				Assert(!module.AllowDelete);
				Assert(!module.AllowNew);
				Assert(!module.AllowEdit);
				Assert(!module.ShowRecentItemsCoreNotOverriden);
				Assert(module is IModuleDecisionProvider);
				AssertEquals(module, module.GetType().GetMethod("CreateDefaultModuleDecisionProvider", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(module, null));
			}
		}

		#endregion

		[RequiresSTA]
		public void TestHandleFindBoxOKButton()
		{
			using (var popup = new OrgAddressEmbeddedModulePopup(ZFilterModule.GetZFilterModule(ModuleIDs.OrgAddresses), ZGuid.Empty, ZArchitecture.Business.AddressType.OFC))
			{
				var address = Factory.New<OrgAddress>();
				popup.Selected += (_, e) =>
				{
					AssertEquals("One address selected", 1, e.SelectedBusinessObjects.Length);
					AssertEquals("Address selected", address.PK, e.SelectedBusinessObjects[0].PK);
				};
				((IModuleDecisionProvider)popup.Module_ForTest).HandleFindBoxOKButton(new[] { address });
			}
		}

		public void TestModuleIDAndSupportsWorkflow()
		{
			using (OrgAddressesModule module = new OrgAddressesModule())
			{
				AssertEquals(ModuleIDs.OrgAddresses, module.ID);
				AssertEquals(true, module.SupportsWorkflow);
			}
		}

		#region Properties

		[RequiresSTA]
		public void TestGetNewFilterControl()
		{
			using (OrgAddressesModuleForTest module = new OrgAddressesModuleForTest())
			{
				IFilterControl filterControl = module.NewFilterControl;
				Assert("Invalid type", filterControl is OrgAddressesFilterControl);
				filterControl.Dispose();
			}
		}

		public void TestGetNewGridCollection()
		{
			using (OrgAddressesModuleForTest module = new OrgAddressesModuleForTest())
			{
				Assert("Invalid type", module.NewGridCollection is OrgAddressCollection);
			}
		}

		public void TestGetNewFilterBusinessObject()
		{
			using (OrgAddressesModuleForTest module = new OrgAddressesModuleForTest())
			{
				Assert("Invalid type", module.NewFilterBusinessObject is OrgAddressesFilterBusinessObject);
			}
		}

		public void TestIModuleDecisionProviderProperties()
		{
			using (OrgAddressesModuleForTest module = new OrgAddressesModuleForTest())
			{
				Assert(!module.ShouldDisplayNotifications);
				Assert(module.ShouldLoadFilterBizObj);
				Assert(module.ShouldSaveFilterBizObj);
				Assert(!module.ShouldIgnoreAdditionalFilter);
				Assert(module.AllowExcelExport);
				Assert(!module.EnablePreviousNextSupport);
				AssertNull(module.List);
			}
		}

		#endregion

		#region OrgAddressesModuleForTest

		public class OrgAddressesModuleForTest : OrgAddressesModule
		{
			public OrgAddressesModuleForTest()
			{
			}

			public IFilterControl NewFilterControl
			{
				get
				{
					return GetNewFilterControl();
				}
			}

			public IBusinessObjectCollection NewGridCollection
			{
				get
				{
					return GetNewGridCollection();
				}
			}

			public FilterBusinessObject NewFilterBusinessObject
			{
				get
				{
					return GetNewFilterBusinessObject();
				}
			}
		}

		#endregion
	}
}
