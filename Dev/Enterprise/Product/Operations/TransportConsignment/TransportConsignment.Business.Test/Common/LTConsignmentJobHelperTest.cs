using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.TransportConsignment.Business.Testing
{
	sealed class LTConsignmentJobHelperTest : DtbConsignmentTestCaseWithFactory
	{
		public void TestAttachLTConsignmentJobsToParentJob_AttachesIfParentIsNotInDatabase()
		{
			var shipment = Factory.New<IForwardingShipment>();
			var consolidation = Factory.New<IDtbBookingConsolidation>();
			consolidation.KB_ParentID = shipment.PK;
			consolidation.KB_ParentTableCode = ((BusinessObject)shipment).TablePrefix;
			var booking = Factory.New<IDtbBooking>();
			booking.KM_KB_Booking = consolidation.PK;

			var ltConsignment = Helper.CreateConsignment("CN1");
			ltConsignment.LTC_KM_Booking = booking.PK;
			Factory.Save();

			var consignmentJobHeader = Factory.NewJobForTesting<JobHeader>();
			consignmentJobHeader.JH_ParentID = ltConsignment.PK;
			consignmentJobHeader.JH_ParentTableCode = DtbConsignmentSchema.Constants.Prefix;

			var shipmentJobHeader = Factory.NewJobForTesting<JobHeader>();
			shipmentJobHeader.JH_ParentID = shipment.PK;
			shipmentJobHeader.JH_ParentTableCode = ((BusinessObject)shipment).TablePrefix;
			var shipmentAsInvoicingSupporter = new JobInvoicingSupporter((IJobHeaderParent)shipment);

			AssertEquals("Precondition - shipment has job header", shipmentAsInvoicingSupporter.Job.PK, shipmentJobHeader.PK);
			AssertNotNull("Precondition - consignment has job header", ltConsignment.Job);
			AssertEquals("Precondition - consignment job header parent is empty", ZGuid.Empty, ltConsignment.Job.JH_JH_ParentJob);

			helper.AttachLTConsignmentJobsToParentJob(shipmentJobHeader, shipment.PK);
			AssertEquals("After call to ConsignmentJobHelper.AttachLTConsigmentJobsToParentJob() consignment job header should be child of shipment job header", shipmentJobHeader.PK, ltConsignment.Job.JH_JH_ParentJob);
		}

		public void TestAttachLTConsignmentJobsToParentJob_DoesNotAttachIfParentIsInDatabase()
		{
			var shipment = Factory.New<IForwardingShipment>();
			var consolidation = Factory.New<IDtbBookingConsolidation>();
			consolidation.KB_ParentID = shipment.PK;
			consolidation.KB_ParentTableCode = ((BusinessObject)shipment).TablePrefix;
			var booking = Factory.New<IDtbBooking>();
			booking.KM_KB_Booking = consolidation.PK;

			var ltConsignment = Helper.CreateConsignment("CN1");
			ltConsignment.LTC_KM_Booking = booking.PK;
			Factory.Save();

			var consignmentJobHeader = Factory.NewJobForTesting<JobHeader>();
			consignmentJobHeader.JH_ParentID = ltConsignment.PK;
			consignmentJobHeader.JH_ParentTableCode = DtbConsignmentSchema.Constants.Prefix;

			var shipmentJobHeader = Factory.NewJobForTesting<JobHeader>();
			shipmentJobHeader.JH_ParentID = shipment.PK;
			shipmentJobHeader.JH_ParentTableCode = ((BusinessObject)shipment).TablePrefix;
			Factory.Save();

			ltConsignment.Job.JH_JH_ParentJob = ZGuid.Empty;
			Factory.Save();

			var shipmentAsInvoicingSupporter = new JobInvoicingSupporter((IJobHeaderParent)shipment);

			AssertEquals("Precondition - shipment has job header", shipmentAsInvoicingSupporter.Job.PK, shipmentJobHeader.PK);
			AssertNotNull("Precondition - consignment has job header", ltConsignment.Job);
			AssertEquals("Precondition - consignment job header parent is empty", ZGuid.Empty, ltConsignment.Job.JH_JH_ParentJob);

			helper.AttachLTConsignmentJobsToParentJob(shipmentJobHeader, shipment.PK);
			AssertEquals("After call to ConsignmentJobHelper.AttachLTConsigmentJobsToParentJob() consignment job header parent should still be empty", ZGuid.Empty, ltConsignment.Job.JH_JH_ParentJob);
		}

		public void TestAttachLTConsignmentJobsToParentJob_AttachesIfParentIsInDatabaseButAttachFlagIsSet()
		{
			var shipment = Factory.New<IForwardingShipment>();
			var consolidation = Factory.New<IDtbBookingConsolidation>();
			consolidation.KB_ParentID = shipment.PK;
			consolidation.KB_ParentTableCode = ((BusinessObject)shipment).TablePrefix;
			var booking = Factory.New<IDtbBooking>();
			booking.KM_KB_Booking = consolidation.PK;

			var ltConsignment = Helper.CreateConsignment("CN1");
			ltConsignment.LTC_KM_Booking = booking.PK;
			Factory.Save();

			var consignmentJobHeader = Factory.NewJobForTesting<JobHeader>();
			consignmentJobHeader.JH_ParentID = ltConsignment.PK;
			consignmentJobHeader.JH_ParentTableCode = DtbConsignmentSchema.Constants.Prefix;

			var shipmentJobHeader = Factory.NewJobForTesting<JobHeader>();
			shipmentJobHeader.JH_ParentID = shipment.PK;
			shipmentJobHeader.JH_ParentTableCode = ((BusinessObject)shipment).TablePrefix;
			Factory.Save();

			ltConsignment.Job.JH_JH_ParentJob = ZGuid.Empty;
			Factory.Save();

			var shipmentAsInvoicingSupporter = new JobInvoicingSupporter((IJobHeaderParent)shipment);

			AssertEquals("Precondition - shipment has job header", shipmentAsInvoicingSupporter.Job.PK, shipmentJobHeader.PK);
			AssertNotNull("Precondition - consignment has job header", ltConsignment.Job);
			AssertEquals("Precondition - consignment job header parent is empty", ZGuid.Empty, ltConsignment.Job.JH_JH_ParentJob);

			helper.AttachLTConsignmentJobsToParentJob(shipmentJobHeader, shipment.PK, true);
			AssertEquals("After call to ConsignmentJobHelper.AttachLTConsigmentJobsToParentJob() consignment job header should be child of shipment job header", shipmentJobHeader.PK, ltConsignment.Job.JH_JH_ParentJob);
		}

		protected override void SetUp()
		{
			helper = new ConsignmentJobHelper();
		}

		ConsignmentJobHelper helper;
	}
}
