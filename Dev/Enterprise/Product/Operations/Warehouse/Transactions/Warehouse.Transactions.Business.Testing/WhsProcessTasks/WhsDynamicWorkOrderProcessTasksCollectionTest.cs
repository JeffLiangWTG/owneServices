using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsDynamicWorkOrderProcessTasksCollection))]
	public class WhsDynamicWorkOrderProcessTasksCollectionTest : WhsDocketProcessTasksCollectionTest
	{
		#region Overrides

		protected override WhsDocketProcessTasksCollection GetNewCollection()
		{
			return new WhsDynamicWorkOrderProcessTasksCollection((WhsDynamicWorkOrder)GetNewDocket());
		}

		protected override WhsDocket GetNewDocket()
		{
			var workOrder = Factory.New<WhsDynamicWorkOrder>();
			workOrder.WD_OH_Client = GetNewClient().PK;
			workOrder.WD_WW_Whs = GetNewWarehouse().PK;
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;
			return workOrder;
		}

		#endregion
	}
}
