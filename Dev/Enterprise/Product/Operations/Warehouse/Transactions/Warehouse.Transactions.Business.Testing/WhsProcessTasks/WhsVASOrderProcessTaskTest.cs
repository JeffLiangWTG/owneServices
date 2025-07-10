using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsVASOrderProcessTask))]
	class WhsVASOrderProcessTaskTest : ProcessTaskTest
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var vasOrder = Factory.NewWithValidTestData<WhsVASOrder>();
			return vasOrder.WorkflowItems.AddNew();
		}

		#endregion
	}
}
