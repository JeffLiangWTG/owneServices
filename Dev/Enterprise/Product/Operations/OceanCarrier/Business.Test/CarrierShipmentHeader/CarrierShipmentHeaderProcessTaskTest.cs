using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.OceanCarrier.Business.Testing
{
	[TestedType(typeof(CarrierShipmentHeaderProcessTask))]
	sealed class CarrierShipmentHeaderProcessTaskTest : ProcessTaskTest
	{
		public void TestParent()
		{
			var shipment = Factory.NewWithValidTestData<CarrierShipmentHeader>();
			var milestone = shipment.WorkflowItems.Milestones.AddNew();
			AssertType<CarrierShipmentHeader>("ProcessTask Parent Type", milestone.Parent);

			Factory.Save();

			AssertType<CarrierShipmentHeaderProcessTask>("ProcessTask of correct type has been created", new BusinessObjectFactory().Load<ProcessTask>(milestone.PK));
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var shipment = Factory.NewWithValidTestData<CarrierShipmentHeader>();
			return shipment.WorkflowItems.AddNew();
		}
	}
}
