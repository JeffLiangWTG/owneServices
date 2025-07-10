using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Warehouse.Environment.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	[TestedType(typeof(ViewPackagesCFSInfo))]
	public class ViewPackagesCFSInfoNPBOTest : NonPersistentBusinessObjectTestCase
	{
		WhsTestHelperFunctionsEnv helper;
		protected WhsTestHelperFunctionsEnv Helper
		{
			get
			{
				if (helper == null)
				{
					helper = GetNewTestHelperFunctions();
				}
				return helper;
			}
		}

		protected virtual WhsTestHelperFunctionsEnv GetNewTestHelperFunctions()
		{
			return new WhsTestHelperFunctionsEnv(Factory);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var dummyParent = Factory.NewWithValidTestData<DummyBizOWithPackLines>();
			var transitWarehouse1 = Helper.CreateTRWWarehouse("TR1");
			var viewPackagesManager = new ViewPackagesManager(Factory, dummyParent);
			var controller = new ViewPackagesCFSInfoController(viewPackagesManager);
			var cfsInfo1 = new ViewPackagesCFSInfo(Factory, transitWarehouse1.WarehouseAddress.PK, controller);
			return cfsInfo1;
		}
	}
}
