using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.eTail.Business;
using Enterprise.eTail.Business.Testing;
using Enterprise.eTail.DataTransfer.Universal;
using Enterprise.Integration;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using NUnit.Framework;
using DummyLogger = Enterprise.UniversalDataBuss.Integration.DummyLogger;

namespace Enterprise.eTail.DataTransfer.Testing
{
	[TestedType(typeof(HVLVConsignmentCollectionDataObjectReader))]
	class HVLVConsignmentCollectionDataObjectReaderTest : DataObjectCollectionReaderTest
	{
		public void TestFindMatchingConsignmentFromDBAsWell()
		{
			var existingConsignmentHeader = Factory.NewWithValidTestData<HVLVForwardingShipment>().GetOrCreateHVLVConsignmentHeader();
			var existingConsignment = existingConsignmentHeader.Consignments.AddNew();
			existingConsignment.HVC_ConsignmentId = "C001";
			Factory.SaveForTesting();

			var shipmentDataObject = CreateShipmentDataObjectWithSubShipment("C001");
			var newConsignmentHeader = Factory.NewWithValidTestData<HVLVForwardingShipment>().GetOrCreateHVLVConsignmentHeader();
			new HVLVConsignmentCollectionDataObjectReader(newConsignmentHeader, new DummyLogger(), Factory, shipmentDataObject.SubShipmentCollection.ToArray()).ReadIntoCollection();

			AssertNotEquals("existing consignment is not attached to another consignment header", newConsignmentHeader.PK, existingConsignment.HVC_HCH_Header);
			AssertEquals("existing consignment is kept in old consignment header", existingConsignmentHeader.PK, existingConsignment.HVC_HCH_Header);

			var existingBookingHeader2 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var existingConsignment2 = existingBookingHeader2.Consignments.AddNew();
			existingConsignment2.HVC_ConsignmentId = "C002";
			existingConsignment2.HVC_HCH_Header = Guid.Empty;

			var shipmentDataObject2 = CreateShipmentDataObjectWithSubShipment("C002");
			var newConsignmentHeader2 = Factory.NewWithValidTestData<HVLVForwardingShipment>().GetOrCreateHVLVConsignmentHeader();
			new HVLVConsignmentCollectionDataObjectReader(newConsignmentHeader, new DummyLogger(), Factory, shipmentDataObject.SubShipmentCollection.ToArray()).ReadIntoCollection();

			AssertNotEquals("existing consignment is attached to another consignment header", newConsignmentHeader.PK, existingConsignment2.HVC_HCH_Header);
		}

		public void TestTransferringExistingConsignmentAlsoTransfersItems()
		{
			var existingConsignment = Factory.New<HVLVConsignment>();
			existingConsignment.HVC_ConsignmentId = "C001";
			var existingItem1 = existingConsignment.Items.AddNew();
			var existingItem2BeginsUnattached = existingConsignment.Items.AddNew();

			var existingShipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();
			var item3ExpectUnchanged = existingConsignment.Items.AddNew();
			item3ExpectUnchanged.HVI_JS_LoadedOnShipment = existingShipment.PK;
			Factory.SaveForTesting();

			var shipmentDataObject = CreateShipmentDataObjectWithSubShipment("C001");
			var newConsignmentHeader = Factory.NewWithValidTestData<HVLVForwardingShipment>().GetOrCreateHVLVConsignmentHeader();
			new HVLVConsignmentCollectionDataObjectReader(newConsignmentHeader, new DummyLogger(), Factory, shipmentDataObject.SubShipmentCollection.ToArray()).ReadIntoCollection();

			CombineAssertions(() =>
			{
				AssertEquals("existingItem1 on same shipment as consignment is moved to new shipment", newConsignmentHeader.HCH_JS_Shipment, existingItem1.HVI_JS_LoadedOnShipment);
				AssertEquals("existingItem2BeginsUnattached is assigned to the new shipment", newConsignmentHeader.HCH_JS_Shipment, existingItem2BeginsUnattached.HVI_JS_LoadedOnShipment);
				AssertEquals("item3ExpectUnchanged stays with its original shipment", existingShipment.PK, item3ExpectUnchanged.HVI_JS_LoadedOnShipment);
			});
		}

		public void TestAttachingExistingConsignmentSetsBookingHeaderIsProcessedAtOriginDepot()
		{
			var existingConsignment = Factory.NewWithValidTestData<HVLVConsignment>();
			existingConsignment.HVC_ConsignmentId = "C001";
			Factory.SaveForTesting();

			var shipmentDataObject = CreateShipmentDataObjectWithSubShipment("C001");
			var newConsignmentHeader = Factory.NewWithValidTestData<HVLVForwardingShipment>().GetOrCreateHVLVConsignmentHeader();
			new HVLVConsignmentCollectionDataObjectReader(newConsignmentHeader, new DummyLogger(), Factory, shipmentDataObject.SubShipmentCollection.ToArray()).ReadIntoCollection();

			Assert(existingConsignment.BookingHeader.HVH_IsProcessedAtOriginDepot);
		}

		public void TestShouldNotAttachAnExistingConsignmentFromOriginalBookingHeader()
		{
			var existingBookingHeader = Factory.New<HVLVBookingHeader>();
			existingBookingHeader.HVH_BookingReference = "HVH001";

			var existingConsignment = existingBookingHeader.Consignments.AddNew();
			existingConsignment.HVC_ConsignmentId = "HVC001";

			var bookingHeader = Factory.New<HVLVBookingHeader>();
			bookingHeader.HVH_BookingReference = "HVH002";

			var universalShipment = CreateShipmentDataObjectWithSubShipment(new[] { "HVC001" });
			var logger = new LoggerForTest(new LoggingInformation());
			new HVLVConsignmentCollectionDataObjectReader(bookingHeader, logger, Factory, universalShipment.SubShipmentCollection.ToArray()).ReadIntoCollection();
			Assert(logger.Logs.Any(log => log.Message == "Consignment HVC001 already existed in Booking Header HVH001, it can't be linked to the imported Booking Header HVH002."));
		}

		#region Implementation

		Shipment CreateShipmentDataObjectWithSubShipment(params string[] subShipmentDataTargetKeys)
		{
			var universalShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			universalShipment.ShipmentType = new CodeDescriptionPair() { Code = Core.Constants.ShipmentTypes.HighVolumeLowValue };

			var subShipments = new DataObjectList<Shipment>();

			foreach (var dataTargetKey in subShipmentDataTargetKeys)
			{
				var subShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
				var dataContext = DataContextFactory.New();
				dataContext.AddDataTarget(DataContextType.HVLVConsignment, dataTargetKey);
				subShipment.DataContext = dataContext;
				subShipments.Add(subShipment);
			}

			universalShipment.SetSubShipmentCollection(() => subShipments);
			return universalShipment;
		}

		public override void TestReadIntoCollection()
		{
			var shipmentBO = Factory.NewWithValidTestData<HVLVForwardingShipment>();
			var consignmentHeader = shipmentBO.GetOrCreateHVLVConsignmentHeader();
			Factory.SaveForTesting();

			var shipmentDataObject = CreateShipmentDataObjectWithSubShipment("", "");
			shipmentDataObject.SubShipmentCollection[0].WayBillNumber = "TestConsignment1";
			shipmentDataObject.SubShipmentCollection[1].WayBillNumber = "TestConsignment2";

			var consignmentCollection = consignmentHeader.Consignments;
			Assert("Precondition: collection is empty", !consignmentCollection.Any());

			new HVLVConsignmentCollectionDataObjectReader(consignmentHeader, new DummyLogger(), Factory, shipmentDataObject.SubShipmentCollection.ToArray()).ReadIntoCollection();
			AssertEquals("Collection should have 2 elements", 2, consignmentCollection.Count);
			AssertContainsExactElementsInAnyOrder("Consignments are added to the collection", new[] { "TestConsignment1", "TestConsignment2" }, consignmentCollection.Select(c => c.HVC_WaybillNumber));
		}

		public void TestReadIntoCollection_WhenSubShipmentDataSourceIsNotHVLVConsignment_ShouldSkip()
		{
			var shipmentBO = Factory.NewWithValidTestData<HVLVForwardingShipment>();
			var consignmentHeader = shipmentBO.GetOrCreateHVLVConsignmentHeader();
			Factory.SaveForTesting();

			var shipmentDataObject = CreateShipmentDataObjectWithSubShipmentAndDataSource(DataContextType.DummyBusinessObject, "0001");

			var consignmentCollection = consignmentHeader.Consignments;
			Assert("Precondition: collection is empty", !consignmentCollection.Any());

			new HVLVConsignmentCollectionDataObjectReader(consignmentHeader, new DummyLogger(), Factory, shipmentDataObject.SubShipmentCollection.ToArray()).ReadIntoCollection();
			AssertEquals("Collection should be empty", 0, consignmentCollection.Count);
		}

		public void TestReadIntoCollection_WhenSubShipmentDataSourceIsHVLVConsignment_ShouldNotSkip()
		{
			var shipmentBO = Factory.NewWithValidTestData<HVLVForwardingShipment>();
			var consignmentHeader = shipmentBO.GetOrCreateHVLVConsignmentHeader();
			Factory.SaveForTesting();

			var shipmentDataObject = CreateShipmentDataObjectWithSubShipmentAndDataSource(DataContextType.HVLVConsignment, "0001");

			var consignmentCollection = consignmentHeader.Consignments;
			Assert("Precondition: collection is empty", !consignmentCollection.Any());

			new HVLVConsignmentCollectionDataObjectReader(consignmentHeader, new DummyLogger(), Factory, shipmentDataObject.SubShipmentCollection.ToArray()).ReadIntoCollection();
			AssertEquals("Collection should have 1 element", 1, consignmentCollection.Count);
		}

		Shipment CreateShipmentDataObjectWithSubShipmentAndDataSource(DataContextType dataSourceType, ZString dataSourceKey)
		{
			var universalShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			universalShipment.ShipmentType = new CodeDescriptionPair() { Code = Core.Constants.ShipmentTypes.HighVolumeLowValue };

			var subShipments = new DataObjectList<Shipment>();

			var subShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			var dataContext = DataContextFactory.New();
			dataContext.AddDataSource(dataSourceType, dataSourceKey);
			subShipment.DataContext = dataContext;
			subShipments.Add(subShipment);

			universalShipment.SetSubShipmentCollection(() => subShipments);
			return universalShipment;
		}

		sealed class LoggerForTest : IXmlImportLogger
		{
			public LoggerForTest(LoggingInformation logger)
			{
				this.logger = logger;
			}
			readonly LoggingInformation logger;

			public bool IsUpdatingConsol { get; set; }
			public bool HasIgnoredModule { get; set; }

			public ITopLevelDataObject TopLevelDataObject => null;

			public IDataContextDataObject TopLevelDataContext => null;

			public bool OrgMatchingDisabled => false;

			public IEnumerable<ISimpleLog> Logs => logger.Logs;

			public void FireDataImportedToBusinessObject(BusinessObject targetBO)
			{
			}

			public void Log(LogType type, string message)
			{
				logger.Log(type, message);
			}

			public void LogBoth(LogType type, string message)
			{
			}

			public void LogErrorToServiceTaskOnly(string message)
			{
			}

			public void LogTopLevelDataContextKey(GetDataContextKey getDataContextKey)
			{
			}

			public IEnumerable<IValidationRule> ValidationRuleCollection { get; set; }
		}

		#endregion
	}
}
