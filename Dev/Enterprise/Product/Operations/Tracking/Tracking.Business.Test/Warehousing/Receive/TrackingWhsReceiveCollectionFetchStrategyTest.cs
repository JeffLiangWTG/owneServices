using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportBookings.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Tracking.Business.Testing
{
	sealed class TrackingWhsReceiveCollectionFetchStrategyTest : TestCaseWithFactory
	{
		public void TestFetchForView_MilestoneColumns()
		{
			var client = Factory.NewWithValidTestData<OrgHeader>();
			client.OH_Code = "XYZ";
			var whs = Factory.NewWithValidTestData<WhsWarehouse>();

			CreateReceiveWithRelatedObjects(client, whs);
			CreateReceiveWithRelatedObjects(client, whs);
			CreateReceiveWithRelatedObjects(client, whs);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var filter = new ZQuery(WhsDocketSchema.WD_OH_Client, client.PK);
			var collection = new TrackingWhsReceiveCollection(newFactory);
			collection.Load(filter);
			var strategy = new TrackingWhsReceiveCollectionFetchStrategy(collection);

			strategy.FetchForView(collection.ToArray(), new TableColumn[] { new TableColumn("ProcessTasks", "Milestones") });

			foreach (TrackingWhsReceive receive in collection)
			{
				_ = receive.WhsReceive.BusinessObjectsWithRelatedEvents;
				_ = receive.WhsReceive.WorkflowItems;
			}

			CombineAssertions(() =>
			{
				AssertEquals("JobHeader Hits", 1, newFactory.GetTableHitCount(JobHeaderSchema.Constants.TableName));
				AssertEquals("DtbBookingConsolidation Hits", 1, newFactory.GetTableHitCount(DtbBookingConsolidationSchema.Constants.TableName));
				AssertEquals("WhsDocketLine Hits", 1, newFactory.GetTableHitCount(WhsDocketLineSchema.Constants.TableName));
				AssertEquals("WhsDocketJobPivot Hits", 1, newFactory.GetTableHitCount(WhsDocketJobPivotSchema.Constants.TableName));
				AssertEquals("ProcessTasks Hits", 1, newFactory.GetTableHitCount(ProcessTasksSchema.Constants.TableName));
			});
		}

		public void TestFetchForView_NoMilestones()
		{
			var client = Factory.NewWithValidTestData<OrgHeader>();
			client.OH_Code = "XYZ";
			var whs = Factory.NewWithValidTestData<WhsWarehouse>();

			CreateReceiveWithRelatedObjects(client, whs);
			Factory.Save();

			var initialFetchHintCount = Factory.ActiveTableFetchHints;

			var filter = new ZQuery(WhsDocketSchema.WD_OH_Client, client.PK);
			var collection = new TrackingWhsReceiveCollection(Factory);
			collection.Load(filter);
			var strategy = new TrackingWhsReceiveCollectionFetchStrategy(collection);

			strategy.FetchForView(collection.ToArray(), System.Array.Empty<TableColumn>());

			AssertEquals(initialFetchHintCount, Factory.ActiveTableFetchHints);
		}

		void CreateReceiveWithRelatedObjects(OrgHeader client, WhsWarehouse whs)
		{
			var receive = TrackingHelper.Get(Factory.New<WhsReceive>());
			receive.WhsReceive.WD_OH_Client = client.PK;
			receive.WhsReceive.WD_WW_Whs = whs.PK;

			var bookingConsolidation = Factory.NewWithValidTestData<DtbBookingConsolidation>();
			bookingConsolidation.KB_ParentID = receive.WhsReceive.PK;
			bookingConsolidation.KB_ParentTableCode = WhsDocketSchema.Constants.Prefix;

			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			var docketJobPivot = Factory.NewWithValidTestData<WhsDocketJobPivot>();
			docketJobPivot.WV_DocketType = DocketType.Codes.Receive;
			docketJobPivot.WV_WD_Docket = receive.WhsReceive.PK;
			docketJobPivot.WV_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			docketJobPivot.WV_ParentId = shipment.PK;

			var processTask = Factory.NewWithValidTestData<WhsReceiveProcessTasks>();
			processTask.P9_ParentTableCode = WhsDocketSchema.Constants.Prefix;
			processTask.P9_Type = Core.Constants.Workflow.MilestoneType;
			processTask.P9_ParentID = receive.WhsReceive.PK;
		}
	}
}
