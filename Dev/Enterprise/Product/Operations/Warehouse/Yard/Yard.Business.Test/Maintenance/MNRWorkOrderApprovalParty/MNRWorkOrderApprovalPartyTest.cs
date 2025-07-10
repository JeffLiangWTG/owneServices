using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Business.Test
{
	[TestedType(typeof(MNRWorkOrderApprovalParty))]
	class MNRWorkOrderApprovalPartyTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.NewWithValidTestData<MNRWorkOrderApprovalParty>();
		}

		#region TestYardUnitState

		public void TestWorkOrder()
		{
			var workOrder = Factory.NewWithValidTestData<MNRWorkOrderHeader>();
			var workOrderApprovalParty = (MNRWorkOrderApprovalParty)GetNewBusinessObject();
			workOrderApprovalParty.MNA_MWO_WorkOrder = workOrder.PK;
			AssertEquals(workOrder, workOrderApprovalParty.WorkOrderHeader);
		}

		#endregion
	}
}
