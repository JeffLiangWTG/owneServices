using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	public class OrgSupplierPartFormWhsPluginTestCase : WhsTestCaseWithFactory
	{
		public void TestConstructor()
		{
			using (var plugin = new OrgSupplierPartFormWhsPlugin(SupplierPart))
			{
				AssertEquals(SupplierPart, plugin.WhsProductForTest.Parent);
			}
		}

		public void TestName()
		{
			using (var plugin = new OrgSupplierPartFormWhsPlugin(SupplierPart))
			{
				AssertEquals("Warehouse", plugin.Name);
			}
		}

		public void TestLicenceCheckPoint()
		{
			using (var plugin = new OrgSupplierPartFormWhsPlugin(SupplierPart))
			{
				AssertEquals(Env.Licence.Core, plugin.LicenceCheckPointForTest);
			}
		}

		public void TestGetNewUserControl()
		{
			using (var plugin = new OrgSupplierPartFormWhsPlugin(SupplierPart))
			{
				using (var control = plugin.GetNewUserControlForTest())
				{
					AssertEquals(typeof(ProductEntryUserControl), control.GetType());
				}
			}
		}

		public void TestGetBusinessEntityForPlugIn()
		{
			using (var plugin = new OrgSupplierPartFormWhsPlugin(SupplierPart))
			{
				AssertEquals(SupplierPart, ((WhsProduct)plugin.GetBusinessEntityForPlugInForTest()).Parent);
			}
		}

		public void TestHasUserControl()
		{
			using (var plugin = new OrgSupplierPartFormWhsPlugin(SupplierPart))
			{
				AssertEquals(true, plugin.HasUserControlForTest);
			}
		}

		OrgSupplierPart SupplierPart => supplierPart ?? (supplierPart = Factory.New<OrgSupplierPart>());
		OrgSupplierPart supplierPart;
	}
}
