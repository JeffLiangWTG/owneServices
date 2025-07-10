using Enterprise.Environment;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.GUI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(AdHocServiceJobModule))]
	public class AdHocServiceJobModuleTest : ZModuleBasherTest
	{
		public void TestModuleID()
		{
			using (var module = (AdHocServiceJobModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(ModuleIDs.WhsAdHocServiceJob, module.ID);
			}
		}

		public void TestLicenseCheckPoint()
		{
			using (var module = (AdHocServiceJobModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(Env.Licence.WarehouseManagerOperationsAnd3PL, module.LicenceCheckPoint);
			}
		}

		public void TestSecurityCheckPoint()
		{
			using (var module = (AdHocServiceJobModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(Env.Security.WhsAdHocServiceJob, module.SecurityCheckpoint);
			}
		}

		public void TestShowNewForm()
		{
			var data = new TestDataForInventory(Factory, new TestNotificationBuffer());
			data.CreateMultiWarehouseClientProductInventory();

			using (var adHocServiceJobModule = new AdHocServiceJobModule_ForTesting())
			{
				using (var adHocServiceJobForm = (AdHocServiceJobEntryForm)adHocServiceJobModule.ShowNewForm_ForTesting())
				{
					var adHocServiceJob = (WhsAdHocServiceJob)adHocServiceJobForm.BusinessEntity;
					AssertEquals("WSJ_OH_Client should be empty.", true, adHocServiceJob.WSJ_OH_Client.IsEmpty);
					AssertEquals("WSJ_WW_Whs should be empty.", true, adHocServiceJob.WSJ_WW_Whs.IsEmpty);
				}

				var filterBusinessObject = (AdHocServiceJobFilterBusinessObject)adHocServiceJobModule.FilterBusinessObject;

				var clientFilter = (ModuleGuidFilter)filterBusinessObject.ModuleFilters[AdHocServiceJobFilterBusinessObject.FilterConstants.Client];
				clientFilter.IsActive = true;
				clientFilter.Property = data.Org1.PK;

				var warehouseFilter = (ModuleGuidFilter)filterBusinessObject.ModuleFilters[AdHocServiceJobFilterBusinessObject.FilterConstants.Warehouse];
				warehouseFilter.IsActive = true;
				warehouseFilter.Property = data.Whs1.PK;

				using (var adHocServiceJobForm = (AdHocServiceJobEntryForm)adHocServiceJobModule.ShowNewForm_ForTesting())
				{
					var adHocServiceJob = (WhsAdHocServiceJob)adHocServiceJobForm.BusinessEntity;
					AssertEquals("WSJ_OH_Client:", data.Org1.PK, adHocServiceJob.WSJ_OH_Client);
					AssertEquals("WSJ_WW_Whs:", data.Whs1.PK, adHocServiceJob.WSJ_WW_Whs);
				}
			}
		}

		class AdHocServiceJobModule_ForTesting : AdHocServiceJobModule
		{
			internal IZForm ShowNewForm_ForTesting()
			{
				return base.ShowNewForm();
			}
		}

		#region Implementation

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.WhsAdHocServiceJob;
		}

		#endregion
	}
}
