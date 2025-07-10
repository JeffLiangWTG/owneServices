using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Tracking.Business.Testing
{
	sealed class TrackingShipmentFetchStrategyTest : TestCaseWithFactory
	{
		public void TestCorrectNumberOfDBHitsForOrganisations()
		{
			BusinessObjectFactory viewFactory = new BusinessObjectFactory();
			TrackingShipment shipment1 = viewFactory.Load<TrackingShipment>(TestShipment1.PK);
			TrackingShipment shipment2 = viewFactory.Load<TrackingShipment>(TestShipment2.PK);
			int beforeFetchForView = viewFactory.DatabaseLoadCount;

			TableColumn[] columns = new TableColumn[]
			{
				new TableColumn("", JobShipmentSchema.JS_OH_DeliveryAgent.Name),
				new TableColumn("", JobShipmentSchema.JS_OH_ExportBroker.Name),
			};

			shipment1.FetchStrategy.FetchForView(columns);
			shipment2.FetchStrategy.FetchForView(columns);

			int afterFetchForView = viewFactory.DatabaseLoadCount;

			AssertNotNull("shipment1.DeliveryAgent", shipment1.DeliveryAgent);
			int beforeHitExportBroker = viewFactory.DatabaseLoadCount;
			AssertNotNull("shipment1.ExportBroker", shipment1.ExportBroker);

			AssertEquals("DatabaseHitCount should not change after hitting any related Org", beforeHitExportBroker, viewFactory.DatabaseLoadCount);

			AssertNotNull("shipment2.DeliveryAgent", shipment2.DeliveryAgent);
			AssertNotNull("shipment2.ExportBroker", shipment2.ExportBroker);

			AssertEquals("DatabaseHitCount should not change after hitting any related properties on second shipment", beforeHitExportBroker, viewFactory.DatabaseLoadCount);
		}

		public void TestCorrectNumberOfDBHitsForPropertiesBasedOnDeliveryConfirms()
		{
			BusinessObjectFactory viewFactory = new BusinessObjectFactory();
			TrackingShipment shipment1 = viewFactory.Load<TrackingShipment>(TestShipment1.PK);
			TrackingShipment shipment2 = viewFactory.Load<TrackingShipment>(TestShipment2.PK);
			int beforeFetchForView = viewFactory.DatabaseLoadCount;

			TableColumn[] columns = new TableColumn[]
			{
				new TableColumn("", ShipmentDeclarationSchema.Constants.ReceivedBy),
				new TableColumn("", ShipmentDeclarationSchema.Constants.ReceivedDate),
				new TableColumn("", ShipmentDeclarationSchema.Constants.PiecesReceived)
			};
			shipment1.FetchStrategy.FetchForView(columns);
			shipment2.FetchStrategy.FetchForView(columns);
			int afterFetchForView = viewFactory.DatabaseLoadCount;

			AssertNotNull("shipment1.ReceivedBy", shipment1.ReceivedBy);
			int beforeHitReceivedDate = viewFactory.DatabaseLoadCount;
			AssertNotNull("shipment1.ReceivedDate", shipment1.ReceivedDate);
			AssertNotNull("shipment1.PiecesReceived", shipment1.PiecesReceived);

			AssertEquals("DatabaseHitCount should not change after hitting any property based on DeliveryConfirms", beforeHitReceivedDate, viewFactory.DatabaseLoadCount);

			AssertNotNull("shipment2.ReceivedBy", shipment2.ReceivedBy);
			AssertNotNull("shipment2.ReceivedDate", shipment2.ReceivedDate);
			AssertNotNull("shipment2.PiecesReceived", shipment2.PiecesReceived);

			AssertEquals("DatabaseHitCount should not change after hitting any related properties on second shipment", beforeHitReceivedDate, viewFactory.DatabaseLoadCount);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			TestShipment1 = GetTestShipment();
			TestShipment2 = GetTestShipment();

			Factory.Save();
		}

		TrackingShipment GetTestShipment()
		{
			TrackingShipment result = Factory.New<TrackingShipment>();
			result.JS_OH_DeliveryAgent = Factory.NewWithValidTestData<OrgHeader>().PK;
			result.JS_OH_ExportBroker = Factory.NewWithValidTestData<OrgHeader>().PK;

			return result;
		}

		TrackingShipment TestShipment1;
		TrackingShipment TestShipment2;

		#endregion
	}
}
