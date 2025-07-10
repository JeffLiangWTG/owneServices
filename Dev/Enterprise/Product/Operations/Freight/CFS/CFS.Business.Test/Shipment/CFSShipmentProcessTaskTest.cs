using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Freight.CFS.Business.Testing
{
	[TestedType(typeof(CFSShipmentProcessTask))]
	class CFSShipmentProcessTaskTest : EnterpriseBusinessObjectTestCase
	{
		public void TestParentControllerID()
		{
			CFSShipmentProcessTask task = Factory.New<CFSShipmentProcessTask>();
			AssertEquals(ControllerIDs.ShipmentReceival, task.ParentControllerID);
		}

		public void TestParent()
		{
			CFSShipment shipment = Factory.NewWithValidTestData<CFSShipment>();
			CFSShipmentProcessTask task = (CFSShipmentProcessTask)((IWorkflowProvider)shipment).WorkflowItems.AddNew();
			AssertEquals(shipment, task.Parent);
			Factory.Save();

			AssertEquals("Type decided correctly", typeof(CFSShipmentProcessTask), new BusinessObjectFactory().Load<ProcessTask>(task.PK).GetType());
		}

		#region Implementation

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObject();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			CFSShipment shipment = Factory.New<CFSShipment>();
			return ((IWorkflowProvider)shipment).WorkflowItems.AddNew();
		}

		#endregion

	}
}
