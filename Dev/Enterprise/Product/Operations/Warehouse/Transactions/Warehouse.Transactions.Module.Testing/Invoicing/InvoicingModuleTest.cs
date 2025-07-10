using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Transactions.GUI;
using Enterprise.Warehouse.Transactions.Invoicing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	public class InvoicingModuleTest : TestCaseWithFactory
	{
		#region TestConstructor

		public void TestConstructor()
		{
			using (var module = new InvoicingModule())
			{
				AssertNotNull("Operational Actions should be plugged in.", module.Plugins.GetPlugin(ControllerIDs.OperationalActions));
			}
		}

		#endregion

		public void TestNewForm()
		{
			var someClient = Factory.NewWithValidTestData<OrgHeader>();
			var someWhs = Factory.NewWithValidTestData<WhsWarehouse>();
			Factory.Save();

			using (var module = (InvoicingModule)ZModuleFactory.Instance.Create(ModuleIDs.WhsInvoicing))
			{
				var fbo = (InvoicingFilterBusinessObject)module.FilterBusinessObject;
				fbo.ET_OH_Client = someClient.PK;
				fbo.ET_WW = someWhs.PK;

				using (var form = (InvoicingForm)module.ShowNewFormForTest())
				{
					form.Show();
					UserIdleWorker.Flush();

					AssertEquals(someClient.PK, ((WhsInvoice)form.BusinessEntity).ET_OH_Client);
					AssertEquals(someClient.PK, ((WhsInvoice)form.BusinessEntity).JobHeaderClient);
					AssertEquals(someWhs.PK, ((WhsInvoice)form.BusinessEntity).ET_WW);
				}
			}
		}

		public void TestSpecifyPropertyToDifferentGuid_ShouldResetSelectedFilters()
		{
			var helper = new WhsTestHelperFunctionsEnv(Factory);

			var warehouse = helper.CreateWarehouse("MI");
			var warehouse2 = helper.CreateWarehouse("MU");

			var user = helper.CreateGlbStaff("JDL", "jessie");
			IOrgsAndWarehousesAccessProvider provider = user;
			provider.SecurityAllowedOrgsAndWarehousesView.SecurityRight = GlbSecurity.AllowedWarehousesSecurityRightName;
			AssertEquals(0, user.SecurityAllowedOrgsAndWarehousesView.Count);

			provider.AddSecurityToAccessOrgOrWarehouse("MI");
			AssertEquals("should now have access to a warehouse", 1, provider.SecurityAllowedOrgsAndWarehousesView.Count);
			AssertEquals("should have access to the correct warehouse", warehouse.PK, provider.SecurityAllowedOrgsAndWarehousesView[0].GU_ItemGUID);
			AssertEquals(1, user.SecurityAllowedOrgsAndWarehousesView.Count);
			var userContext = new UserContext(user, Env.CurrentBranch.PK, Env.CurrentDepartment.PK);
			Factory.Save();

			using (Env.SetTemporaryUserContext(userContext))
			{
				using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.WhsInvoicing))
				using (var form = module.ShowPopup())
				{
					var filterBizo = module.FilterBusinessObject;
					DisableAllFilters(filterBizo);

					var filter = (ModuleGuidFilter)filterBizo["Warehouse"];
					filter.IsActive = true;

					filter.Property = warehouse2.PK;
					AssertHasError(filter.PropertyInfo, "Enter a valid selection.");

					FireSearch(form);
					AssertEquals("Should display error message", "There are errors. Please correct these before searching.", UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				}

				using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.WhsInvoicing))
				using (var form = module.ShowPopup())
				{
					var filterBizo = module.FilterBusinessObject;
					DisableAllFilters(filterBizo);

					var filter = (ModuleGuidFilter)filterBizo["Warehouse"];
					filter.IsActive = true;

					filter.Property = warehouse.PK;
					AssertNoErrors(filter.PropertyInfo);

					FireSearch(form);
					AssertEquals("Should display error message", "There are no records that match your search.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		void DisableAllFilters(FilterStripBusinessObject filterBizo)
			=> filterBizo.ModuleFilters.ForEach(m => m.IsActive = false);

		void FireSearch(IZForm form)
			=> ((Control)form).FindSingle<ZFilterStripCommonControl>().FirePerformSearch();

		StmModuleFilter SaveLayout(FilterStripBusinessObject filterStripBizO, ZString layoutName, bool global = false)
		{
			StmModuleFilter result = null;
			if (!layoutName.IsEmpty && filterStripBizO.FilterStrips.Count > 0)
			{
				result = new DataGridLayoutManager().SavePreconfiguredLayout(filterStripBizO, layoutName, false, global, SaveColumnLayout.Yes);
			}
			return result;
		}

		public void TestRestrictedWarehouseUserCannotPerformSavedFilterSearch()
		{
			var helper = new WhsTestHelperFunctionsEnv(Factory);

			var warehouse = helper.CreateWarehouse("MI");
			var client = helper.CreateClient("JJJJ");

			var user = helper.CreateGlbStaff("JDL", "jessie");
			IOrgsAndWarehousesAccessProvider provider = user;

			provider.SecurityAllowedOrgsAndWarehousesView.SecurityRight = GlbSecurity.AllowedWarehousesSecurityRightName;
			provider.AddSecurityToAccessOrgOrWarehouse("MI");

			AssertEquals("Should now have access to a warehouse", 1, provider.SecurityAllowedOrgsAndWarehousesView.Count);
			AssertEquals("Should have access to the correct warehouse", warehouse.PK, provider.SecurityAllowedOrgsAndWarehousesView[0].GU_ItemGUID);
			AssertEquals(1, user.SecurityAllowedOrgsAndWarehousesView.Count);

			provider.SecurityAllowedOrgsAndWarehousesView.SecurityRight = GlbSecurity.AllowedClientsSecurityRightName;
			provider.AddSecurityToAccessOrgOrWarehouse("JJJJ");

			AssertEquals("Should now have access to a client", 1, provider.SecurityAllowedOrgsAndWarehousesView.Count);
			AssertEquals("Should have access to the correct client", client.PK, provider.SecurityAllowedOrgsAndWarehousesView[0].GU_ItemGUID);
			AssertEquals(1, user.SecurityAllowedOrgsAndWarehousesView.Count);

			var userContext = new UserContext(user, Env.CurrentBranch.PK, Env.CurrentDepartment.PK);
			Factory.Save();

			using (Env.SetTemporaryUserContext(userContext))
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.WhsInvoicing))
			{
				using (var form = module.ShowPopup())
				{
					var filterBizo = module.FilterBusinessObject;

					var filterWarehouse = (ModuleGuidFilter)filterBizo["Warehouse"];
					filterWarehouse.Property = warehouse.PK;
					filterWarehouse.IsActive = true;

					var filterClient = (ModuleGuidFilter)filterBizo["Client"];
					filterClient.Property = client.PK;
					filterClient.IsActive = true;

					var savedFilter = SaveLayout(filterBizo, "test");

					filterBizo.LoadLayout(savedFilter);

					var control = ((Control)form).FindSingle<ZFilterStripCommonControl>();
					control.FirePerformSearch();
					AssertEquals("Should not display error message", "There are no records that match your search.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		#region TestIOperationalActionSupportable

		public void TestIOperationalActionSupportable()
		{
			using (var invoicingModule = new InvoicingModule())
			{
				AssertNotNull(invoicingModule.Plugins.GetPlugin(ControllerIDs.OperationalActions));
				AssertEquals(typeof(InvoicingOperationalActionsSupporter), ((IOperationalActionSupportable)invoicingModule).OperationalActionSupporter.GetType());
			}
		}

		#endregion
	}
}
