using Enterprise.Environment;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(EntryLineModule))]
	internal class EntryLineModuleTest : ZModuleBasherTest
	{
		public void TestLicenseCheckPoint()
		{
			using (EntryLineModule module = (EntryLineModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(Env.Licence.WarehouseManagerCoreAnd4PL, module.LicenceCheckPoint);
			}
		}

		public void TestSecurityCheckpoint()
		{
			using (EntryLineModule module = (EntryLineModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(Env.Security.Warehouse, module.SecurityCheckpoint);
			}
		}

		public void TestModuleID()
		{
			using (EntryLineModule module = (EntryLineModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(ModuleIDs.WhsEntryLine, module.ID);
			}
		}

		public void TestHasActions()
		{
			using (EntryLineModule module = (EntryLineModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(false, module.HasActions);
			}
		}

		public void TestSetFindBoxCodeDescription()
		{
			using (EntryLineModule module = (EntryLineModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				var inventory = Factory.New<WhsInventoryView>();
				inventory.WI_BondedEntryKey = "11-2";

				using (ZCodeFindBox findBox = new ZCodeFindBox())
				{
					module.SetFindBoxCodeDescription(findBox, inventory);
					AssertEquals("11-2", ((IFindBox)findBox).Code);
				}
			}
		}

		public void TestGetModuleDecisionProviderForFindBoxPopup()
		{
			using (EntryLineModule module = (EntryLineModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(typeof(PopupModuleDecisionProvider), module.GetModuleDecisionProviderForFindBoxPopup(new DummyFindBox()).GetType());
			}
		}

		public override void TestControllersDefinedForAllCountriesModuleDefinedOn()
		{
			Assert("No controller available for this module", true);
		}

		public override void TestBusinessObjectHasIndexedPKIfExcelExportIsEnabled()
		{
			Assert("Not required.", true);
		}

		#region Implementation

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.WhsEntryLine;

		#endregion
	}
}
