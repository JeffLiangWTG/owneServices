using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ZA.DataRegistry.Business;
using Enterprise.Customs.ZA.ModuleRegistration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Module.Testing
{
	[TestedType(typeof(WarehouseOperatorTransactionsModule))]
	class WarehouseOperatorTransactionsModuleBasherTest : ZModuleBasherTest
	{
		public void TestOperationalActionsPlugInAdded()
		{
			using (var module = GetTypedModule())
			{
				AssertNotNull(module.Plugins.GetPlugin(ControllerIDs.OperationalActions));
			}
		}

		protected override ModuleIdentifier GetModuleID() => ZAModuleIDs.WarehouseOperatorTransactions;

		protected override string CountryCode => Core.Constants.CountryCodes.SouthAfrica;

		public void TestAllowView()
		{
			using (var module = GetTypedModule())
			{
				AssertEquals("View Operation should be allowed", true, module.AllowView);
			}
		}

		public void TestAllowEdit()
		{
			using (var module = GetTypedModule())
			{
				AssertEquals("Edit Operation should not be allowed", false, module.AllowEdit);
			}
		}

		public void TestAllowDelete()
		{
			using (var module = GetTypedModule())
			{
				AssertEquals("Delete Operation should not be allowed", false, module.AllowDelete);
			}
		}

		public void TestAllowNew()
		{
			using (var module = GetTypedModule())
			{
				AssertEquals("New Operation should not be allowed", false, module.AllowNew);
			}
		}

		public void TestGridCollection()
		{
			using (var module = GetTypedModule())
			{
				var collection = module.GridCollection as ModuleCusWHSOperatorTransactionsCollection;
				AssertNotNull(collection);
			}
		}

		public void TestFormActionMenu()
		{
			using (var module = GetTypedModule())
			{
				AssertMaintenanceMenuItem(module, "Cancel Unallocated Order", shouldExist: true);
				AssertMaintenanceMenuItem(module, "Reverse a Batch", shouldExist: false);
				AssertMaintenanceMenuItem(module, "Reset Receipts for a Batch", shouldExist: true);

				var normalUser = Factory.NewWithValidTestData<GlbStaff>();
				Factory.Save();
				using (Env.SetTemporaryUserContext(normalUser.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
				{
					AssertMaintenanceMenuItem(module, "Reset Receipts for a Batch", shouldExist: false);
				}
			}
			using (var module = GetTypedModule())
			{
				using (ZACustomsRegistry.Instance.AllowReversalOfCusWarehouseBatch.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					AssertMaintenanceMenuItem(module, "Reverse a Batch", shouldExist: true);
				}
			}
		}

		void AssertMaintenanceMenuItem(ZFilterModule module, string menuItemName, bool shouldExist)
		{
			var maintenanceMenu = module.FormActionMenu.SingleOrDefault(menu => menu.Text == "Maintenance");
			var targetMenuItem = maintenanceMenu?.MenuItems.Cast<MenuItem>().SingleOrDefault(menu => menu.Text == menuItemName);
			if (shouldExist)
			{
				AssertNotNull($"Menu \"{menuItemName}\" should exist", targetMenuItem);
			}
			else
			{
				AssertNull($"Menu \"{menuItemName}\" should not exist", targetMenuItem);
			}
		}

		WarehouseOperatorTransactionsModule GetTypedModule() => (WarehouseOperatorTransactionsModule)base.GetModule();

		public override void TestModuleShowsAndCanSearch()
		{
			CreateOperatorTransaction(GlbCompany.CurrentCompany, 1);
			Factory.Save();
			base.TestModuleShowsAndCanSearch();
		}

		CusWHSOperatorTransaction CreateOperatorTransaction(GlbCompany company, int quantity)
		{
			var result = Factory.NewWithValidTestData<CusWHSOperatorTransaction>();
			result.Batch.WOB_GC_Company = company.PK;
			result.WOT_TransactionType = WarehouseOperatorTransactionTypeList.Codes.ORD;
			result.WOT_ExportType = ZString.Empty;
			result.WOT_Status = WarehouseOperatorTransactionStatusList.Codes.VAL;
			result.WOT_Quantity = quantity;
			return result;
		}
	}
}
