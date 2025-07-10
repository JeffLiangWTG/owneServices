using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(BillOfLadingProcessTask))]
	internal class BillOfLadingProcessTaskTest : ProcessTaskTest
	{
		public void TestParent()
		{
			BillOfLading shipment = Factory.NewWithValidTestData<BillOfLading>();
			ProcessTask milestone = shipment.WorkflowItems.Milestones.AddNew();
			AssertEquals(typeof(BillOfLading), milestone.Parent.GetType());
			Factory.Save();
			AssertEquals("type decided correctly", typeof(BillOfLadingProcessTask), new BusinessObjectFactory().Load<ProcessTask>(milestone.PK).GetType());
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			BillOfLading shipment = Factory.New<BillOfLading>();
			return shipment.WorkflowItems.AddNew();
		}
	}
}
