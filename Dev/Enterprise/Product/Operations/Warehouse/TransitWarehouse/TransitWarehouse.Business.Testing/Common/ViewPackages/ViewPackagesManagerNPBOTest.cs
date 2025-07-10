using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Warehouse.Environment.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	[TestedType(typeof(ViewPackagesManager))]
	public class ViewPackagesManagerNPBOTest : NonPersistentBusinessObjectTestCase
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
			var warehouse = Helper.CreateTRWWarehouse();
			var parent = Factory.New<DummyBizOWithPackLines>();
			parent.TransitWarehouseAddressPK = warehouse.WW_OA_WarehouseAddress;
			var planner = new ViewPackagesManager(Factory, parent);
			return planner;
		}
	}
}
