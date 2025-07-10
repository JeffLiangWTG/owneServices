using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.CFS.Business.Testing
{
	[TestedType(typeof(CFSShipmentProcessTaskCollection))]
	public class CFSShipmentProcessTaskCollectionTest : ProcessTaskCollectionTest<CFSShipmentProcessTaskCollection>
	{
		public void TestParent()
		{
			CFSShipment shipment = Factory.New<CFSShipment>();
			AssertEquals(shipment, ((IWorkflowProvider)shipment).WorkflowItems.Parent);
		}

		public void TestIndexer()
		{
			CFSShipment shipment = Factory.New<CFSShipment>();
			CFSShipmentProcessTaskCollection collection = (CFSShipmentProcessTaskCollection)((IWorkflowProvider)shipment).WorkflowItems;
			CFSShipmentProcessTask task = collection.AddNew();
			AssertEquals(task, collection[0]);
		}

		public void TestLoad_DoNotReturnResultIfShipmentIsAlsoForwardRegistered()
		{
			CFSShipment shipment = Factory.New<CFSShipment>();
			shipment.JS_IsForwardRegistered = false;
			CFSShipment shipment2 = Factory.New<CFSShipment>();
			shipment2.JS_IsForwardRegistered = true;

			ProcessTask task1a = Factory.New<CFSShipmentProcessTask>();
			task1a.P9_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			task1a.P9_ParentID = shipment.PK;
			ProcessTask task1b = Factory.New<CFSShipmentProcessTask>();
			task1b.P9_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			task1b.P9_ParentID = shipment.PK;
			ProcessTask task2a = Factory.New<ProcessTask>();
			task2a.P9_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			task2a.P9_ParentID = shipment2.PK;
			ProcessTask task2b = Factory.New<ProcessTask>();
			task2b.P9_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			task2b.P9_ParentID = shipment2.PK;

			CombineAssertions(delegate
			{
				AssertEquals(2, ((IWorkflowProvider)shipment).WorkflowItems.Count);
				AssertEquals(0, ((IWorkflowProvider)shipment2).WorkflowItems.Count);
			});
		}

		protected override CFSShipmentProcessTaskCollection GetCollectionToTestCore()
		{
			CFSShipment shipment = Factory.New<CFSShipment>();
			return new CFSShipmentProcessTaskCollection(shipment);
		}
	}
}
