using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transit.Business.Common;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	public class TransitWarehouseConsignmentJobHelperTest : TestCaseWithFactory
	{
		#region TestAttachTransitWarehouseConsignmentJobsToParentJob

		public void TestAttachTransitWarehouseConsignmentJobsToParentJob()
		{
			var shipment = (IJobHeaderParent)Factory.New<IForwardingShipment>();
			var jobOnParent = new JobHeader.Loader(shipment).TryLoadOrCreate();

			var transitWarehouse = Helper.CreateTRWWarehouse();

			var receiveConsignmentWithParent = Helper.CreateReceiveConsignment("RC00000001", transitWarehouse.PK);
			var jobOnReceiveConsignmentWithParent = new JobHeader.Loader(receiveConsignmentWithParent).TryCreate();
			receiveConsignmentWithParent.WRC_ParentID = shipment.PK;
			receiveConsignmentWithParent.WRC_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			var receiveConsignmentWithoutParent = Helper.CreateReceiveConsignment("RC00000002", transitWarehouse.PK);
			var jobOnReceiveConsignmentWithoutParent = new JobHeader.Loader(receiveConsignmentWithoutParent).TryCreate();

			var dispatchConsignmentWithParent = Helper.CreateDispatchConsignment("DC00000001", transitWarehouse.PK);
			var jobOnDispatchConsignmentWithParent = new JobHeader.Loader(dispatchConsignmentWithParent).TryCreate();
			dispatchConsignmentWithParent.WDC_ParentID = shipment.PK;
			dispatchConsignmentWithParent.WDC_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			var dispatchConsignmentWithoutParent = Helper.CreateDispatchConsignment("DC00000002", transitWarehouse.PK);
			var jobOnDispatchConsignmentWithoutParent = new JobHeader.Loader(dispatchConsignmentWithoutParent).TryCreate();

			AssertEquals("JobHeader parent does not exist", true, jobOnReceiveConsignmentWithParent.JH_JH_ParentJob.IsEmpty);
			AssertEquals("JobHeader parent does not exist", true, jobOnDispatchConsignmentWithParent.JH_JH_ParentJob.IsEmpty);
			AssertEquals("JobHeader parent does not exist", true, jobOnReceiveConsignmentWithoutParent.JH_JH_ParentJob.IsEmpty);
			AssertEquals("JobHeader parent does not exist", true, jobOnDispatchConsignmentWithoutParent.JH_JH_ParentJob.IsEmpty);

			TransitWarehouseConsignmentJobHelperObject.AttachConsignmentJobsToParentJob(jobOnParent, shipment.PK);

			AssertEquals("JobHeader parent has been populated", jobOnParent.PK, jobOnReceiveConsignmentWithParent.JH_JH_ParentJob);
			AssertEquals("JobHeader parent has been populated", jobOnParent.PK, jobOnDispatchConsignmentWithParent.JH_JH_ParentJob);
			AssertEquals("JobHeader parent has not been populated", true, jobOnReceiveConsignmentWithoutParent.JH_JH_ParentJob.IsEmpty);
			AssertEquals("JobHeader parent has not been populated", true, jobOnDispatchConsignmentWithoutParent.JH_JH_ParentJob.IsEmpty);
		}

		#endregion

		#region Implementation

		WhsTransitTestHelper Helper => new WhsTransitTestHelper(Factory);

		TransitWarehouseConsignmentJobHelper TransitWarehouseConsignmentJobHelperObject => new TransitWarehouseConsignmentJobHelper();

		#endregion
	}
}
