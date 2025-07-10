using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsDynamicWorkOrderDocManagerInfo))]
	sealed class WhsDynamicWorkOrderDocManagerInfoTest : DocManagerInfoTestCase
	{
		public override BusinessObject GetEmptyParentBusinessObject()
		{
			return Factory.New<WhsDynamicWorkOrder>();
		}

		public override BusinessObject GetPopulatedParentBusinessObject()
		{
			return Helper.CreateWhsDynamicWorkOrder(Helper.CreateClient("C1"), Helper.CreateWarehouse("W1"));
		}

		WhsTestHelperFunctions Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctions(Factory)); }
		}
		WhsTestHelperFunctions helper;
	}
}
