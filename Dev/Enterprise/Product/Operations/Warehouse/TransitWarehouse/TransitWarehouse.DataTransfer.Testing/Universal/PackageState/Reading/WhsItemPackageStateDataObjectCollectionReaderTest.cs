using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Warehouse.Transit.DataTransfer.Universal.WhsTransitPackageStateBusinessObjectFinderForASN;
using static Enterprise.Warehouse.Transit.DataTransfer.Universal.WhsTransitPackageStateDataObjectReaderConstants;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing
{
	[TestedType(typeof(WhsItemPackageStateDataObjectCollectionReader))]
	public class WhsItemPackageStateDataObjectCollectionReaderTest : DataObjectCollectionReaderTest
	{
		public override void TestReadIntoCollection()
		{
			var shipmentDataObject = UniversalData.ShipmentDataObject;
			var warehouse = UniversalData.Warehouse;
			var consignment = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			Logger.TopLevelDataObject = UniversalData.HeaderDataObject;

			var handlingUnitPackingLine1 = Helper.CreatePackingLine("HandlingUnit-01", "", Constants.PkgUnit.Pallet, 150m, 10m, 10m, 10m, marksAndNumbers: "Furnitures");
			var handlingUnitPackingLine2 = Helper.CreatePackingLine("HandlingUnit-02", "", Constants.PkgUnit.Pallet, 150m, 10m, 10m, 10m, marksAndNumbers: "Furnitures");
			var handlingUnitPackingLine3 = Helper.CreatePackingLine("HandlingUnit-03", "", Constants.PkgUnit.Pallet, 150m, 10m, 10m, 10m, marksAndNumbers: "Furnitures");
			var inner1 = Helper.CreatePackingLine("Inner-01", "", Constants.PkgUnit.Box, 150m, 5m, 5m, 5m, marksAndNumbers: "Furnitures");
			var inner2 = Helper.CreatePackingLine("Inner-02", "", Constants.PkgUnit.Carton, 150m, 2m, 2m, 2m, marksAndNumbers: "Furnitures");
			var inner3 = Helper.CreatePackingLine("Inner-03", "", Constants.PkgUnit.Bag, 150m, 1m, 1m, 1m, marksAndNumbers: "Furnitures");
			handlingUnitPackingLine1.SetPackingLineCollection(() => new List<PackingLine> { handlingUnitPackingLine2, inner3 });
			handlingUnitPackingLine2.SetPackingLineCollection(() => new List<PackingLine> { handlingUnitPackingLine3, inner2 });
			handlingUnitPackingLine3.SetPackingLineCollection(() => new List<PackingLine> { inner1 });
			shipmentDataObject.PackingLineCollection.Clear();
			shipmentDataObject.PackingLineCollection.Add(handlingUnitPackingLine1);
			// Shipment PackingLine Tree To Be Imported
			//	HandlingUnit-01
			//		HandlingUnit-02
			//			HandlingUnit-03
			//				Inner-01
			//			Inner-02
			//		Inner-03

			var manager = new Mock<TransitDataObjectReaderHandlerManager>();
			var consolHandler = new TransitReceiveConsolHandler();
			manager.Setup(d => d.GetHandler<TransitReceiveConsolHandler>()).Returns(consolHandler);
			manager.Setup(m => m.AddPackagesByContainerLink(It.IsAny<ZInt>(), It.IsAny<List<PkgPackage>>()));
			using (ObjectFactory.Substitute("TransitDataObjectReaderHandlerManager", manager.Object))
			{
				var finder = new WhsTransitPackageStateBusinessObjectFinderForRCN(consignment, Factory, Logger, shipmentDataObject, shipmentDataObject);

				var reader = new WhsItemPackageStateDataObjectCollectionReader(shipmentDataObject.PackingLineCollection, shipmentDataObject, Logger, Factory, finder, new WhsItemPackageStateCollection(consignment), consignment, WhsTransitPackageStatePopulateStrategy.New, WhsTransitPackageStateUpdateStrategy.CompleteUpdate, ProcessType.RCN);

				reader.ReadIntoCollection();
			}

			var packageJob = Factory.BOFactory.Load<PkgPackageJob>(new ZQuery(PkgPackageJobSchema.KJ_ParentID, consignment.PK)).Single();
			AssertEquals("There should be 6 packages on the package Job", 6, packageJob.GetAllPackagesOnJob().Length);

			var packages = packageJob.GetAllPackagesOnJob().AsEnumerable();
			var handlingUnitPackage1 = Helper.AssertAndReturnPackage(packages, consignment.PK, 6, "HandlingUnit-01", "", "Furnitures", 150m, "KG", 10m, 10m, 10m, "CM", 1, Constants.PkgUnit.Pallet);
			var handlingUnitPackage2 = Helper.AssertAndReturnPackage(packages, consignment.PK, 6, "HandlingUnit-02", "", "Furnitures", 150m, "KG", 10m, 10m, 10m, "CM", 1, Constants.PkgUnit.Pallet);
			var handlingUnitPackage3 = Helper.AssertAndReturnPackage(packages, consignment.PK, 6, "HandlingUnit-03", "", "Furnitures", 150m, "KG", 10m, 10m, 10m, "CM", 1, Constants.PkgUnit.Pallet);
			var inner1Package = Helper.AssertAndReturnPackage(packages, consignment.PK, 6, "Inner-01", "", "Furnitures", 150m, "KG", 5m, 5m, 5m, "CM", 1, Constants.PkgUnit.Box);
			var inner2Package = Helper.AssertAndReturnPackage(packages, consignment.PK, 6, "Inner-02", "", "Furnitures", 150m, "KG", 2m, 2m, 2m, "CM", 1, Constants.PkgUnit.Carton);
			var inner3Package = Helper.AssertAndReturnPackage(packages, consignment.PK, 6, "Inner-03", "", "Furnitures", 150m, "KG", 1m, 1m, 1m, "CM", 1, Constants.PkgUnit.Bag);

			AssertContains("Populating Packages for Receive Consignment...", Logger.Logs);
		}

		#region TestReadIntoCollection_CreatePackageStateWhenNoPackingLine

		public void TestReadIntoCollection_CreatePackageStateWhenNoPackingLine()
		{
			var shipmentDataObject = UniversalData.ShipmentDataObject;
			var warehouse = UniversalData.Warehouse;
			var consignment = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			shipmentDataObject.TotalNoOfPacks = 1;

			Logger.TopLevelDataObject = UniversalData.HeaderDataObject;

			var manager = new Mock<TransitDataObjectReaderHandlerManager>();
			var consolHandler = new TransitReceiveConsolHandler();
			manager.Setup(d => d.GetHandler<TransitReceiveConsolHandler>()).Returns(consolHandler);
			manager.Setup(m => m.AddPackagesByContainerLink(It.IsAny<ZInt>(), It.IsAny<List<PkgPackage>>()));

			using (ObjectFactory.Substitute("TransitDataObjectReaderHandlerManager", manager.Object))
			{
				var finder = new WhsTransitPackageStateBusinessObjectFinderForRCN(consignment, Factory, Logger, shipmentDataObject, shipmentDataObject);

				var reader = new WhsItemPackageStateDataObjectCollectionReader(shipmentDataObject.PackingLineCollection, shipmentDataObject, Logger, Factory, finder, new WhsItemPackageStateCollection(consignment), consignment, WhsTransitPackageStatePopulateStrategy.New, WhsTransitPackageStateUpdateStrategy.CompleteUpdate, ProcessType.RCN);

				reader.ReadIntoCollection();
			}

			var packageJob = Factory.BOFactory.Load<PkgPackageJob>(new ZQuery(PkgPackageJobSchema.KJ_ParentID, consignment.PK)).Single();
			AssertEquals("There should be 1 packages on the package Job", 1, packageJob.GetAllPackagesOnJob().Length);

			var packageStatesInDB = Factory.Load<WhsItemPackageState>(new ZQuery());

			AssertEquals("There should be 1 packageStates", 1, packageStatesInDB.Length);
			AssertEquals("The package state should be created for the consignment", consignment.PK, packageStatesInDB[0].WPS_WRC_TransitReceiveConsignment);
			AssertEquals("The package state should be created for the warehouse", warehouse.PK, packageStatesInDB[0].WPS_WW_Warehouse);
			AssertEquals("The package state type is PKL", "PKL", packageStatesInDB[0].WPS_UnitType);
		}

		public void TestReadIntoCollection_CreatePackageStateWhenNoPackingLineAndRCNContainsARVPackageState()
		{
			var shipmentDataObject = UniversalData.ShipmentDataObject;
			var warehouse = UniversalData.Warehouse;
			var consignment = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU001", warehouse.PK, warehouse.DefaultLocation.PK);
			var packageState = Helper.CreatePackageState(consignment, 1, "PKG", "PKG-1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			shipmentDataObject.TotalNoOfPacks = 1;

			Factory.SaveForTesting();

			Logger.TopLevelDataObject = shipmentDataObject;

			var manager = new Mock<TransitDataObjectReaderHandlerManager>();
			var consolHandler = new TransitReceiveConsolHandler();
			manager.Setup(d => d.GetHandler<TransitReceiveConsolHandler>()).Returns(consolHandler);

			using (ObjectFactory.Substitute("TransitDataObjectReaderHandlerManager", manager.Object))
			{
				var finder = new WhsTransitPackageStateBusinessObjectFinderForRCN(consignment, Factory, Logger, shipmentDataObject, shipmentDataObject);

				var reader = new WhsItemPackageStateDataObjectCollectionReader(shipmentDataObject.PackingLineCollection, shipmentDataObject, Logger, Factory, finder, new WhsItemPackageStateCollection(consignment), consignment, WhsTransitPackageStatePopulateStrategy.New, WhsTransitPackageStateUpdateStrategy.CompleteUpdate, ProcessType.RCN);

				reader.ReadIntoCollectionRetainingUnmatchedElements();
			}

			Factory.SaveForTesting();

			var newFactoryAfterImport = new UniversalObjectFactory();
			var packageJob = newFactoryAfterImport.Load<PkgPackageJob>(new ZQuery(PkgPackageJobSchema.KJ_ParentID, consignment.PK)).Single();
			AssertEquals("There should be 1 packages on the package Job", 1, packageJob.GetAllPackagesOnJob().Length);

			var packageStatesInDB = newFactoryAfterImport.Load<WhsItemPackageState>(new ZQuery());

			AssertEquals("There should be 1 packageStates", 1, packageStatesInDB.Length);
			AssertEquals("The package state should be existing packageState", packageState.PK, packageStatesInDB[0].PK);
			AssertContains("Receive Consignment has been partially received. Package level data will be ignored and not read in.", Logger.Logs);
		}

		#endregion

		#region TestReadIntoCollection_CreatePackageStateAndASN

		public void TestReadIntoCollection_CreatePackageStateAndASN()
		{
			var shipmentDataObject = UniversalData.ShipmentDataObject;
			var warehouse = UniversalData.Warehouse;
			var consignment = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);

			Logger.TopLevelDataObject = shipmentDataObject;

			var packingLineHasContainerLink = Helper.CreatePackingLine("packing-1", "", Constants.PkgUnit.Pallet, 150m, 5m, 5m, 5m, marksAndNumbers: "Furnitures");
			packingLineHasContainerLink.ContainerLink = 1;
			shipmentDataObject.PackingLineCollection.Clear();
			shipmentDataObject.PackingLineCollection.Add(packingLineHasContainerLink);

			var manager = new Mock<TransitDataObjectReaderHandlerManager>();
			var consolHandler = new TransitReceiveConsolHandler();
			manager.Setup(d => d.GetHandler<TransitReceiveConsolHandler>()).Returns(consolHandler);
			manager.Setup(m => m.AddPackagesByContainerLink(It.IsAny<ZInt>(), It.IsAny<List<PkgPackage>>()));

			using (ObjectFactory.Substitute("TransitDataObjectReaderHandlerManager", manager.Object))
			{
				var finder = new WhsTransitPackageStateBusinessObjectFinderForRCN(consignment, Factory, Logger, shipmentDataObject, shipmentDataObject);
				var reader = new WhsItemPackageStateDataObjectCollectionReader(shipmentDataObject.PackingLineCollection, shipmentDataObject, Logger, Factory, finder, new WhsItemPackageStateCollection(consignment), consignment, WhsTransitPackageStatePopulateStrategy.New, WhsTransitPackageStateUpdateStrategy.CompleteUpdate, ProcessType.RCN);
				reader.ReadIntoCollection();
			}

			var packageJob = Factory.BOFactory.Load<PkgPackageJob>(new ZQuery(PkgPackageJobSchema.KJ_ParentID, consignment.PK)).Single();
			AssertEquals("There should be 1 packages on the package Job", 1, packageJob.GetAllPackagesOnJob().Length);
			var packageStatesInDB = Factory.Load<WhsItemPackageState>(new ZQuery());

			AssertEquals("There should be 1 packageStates", 1, packageStatesInDB.Length);
		}

		#endregion

		#region TestReadIntoCollection_DetachPackageStateFromASN

		public void TestReadIntoCollection_DetachPackageStateFromASN()
		{
			var shipmentDataObject = UniversalData.ShipmentDataObject;
			var warehouse = UniversalData.Warehouse;
			var rcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var asn = Helper.CreateReceiveASN("ASN1", warehouse.PK);
			var packageState = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-1", TransitWarehouseStatuses.Codes.Booked, receiveASN: asn);

			shipmentDataObject.SetPackingLineCollection(() => null);

			var manager = new Mock<TransitDataObjectReaderHandlerManager>();
			var consolHandler = new TransitReceiveConsolHandler();
			manager.Setup(d => d.GetHandler<TransitReceiveConsolHandler>()).Returns(consolHandler);

			using (ObjectFactory.Substitute("TransitDataObjectReaderHandlerManager", manager.Object))
			{
				var finder = new WhsTransitPackageStateBusinessObjectFinderForRCN(rcn, Factory);

				new WhsItemPackageStateDataObjectCollectionReader(new DataObjectList<PackingLine>(), shipmentDataObject, logger, Factory, finder, new WhsItemPackageStateCollection(rcn), rcn, WhsTransitPackageStatePopulateStrategy.Detach, WhsTransitPackageStateUpdateStrategy.CompleteUpdate, ProcessType.DetachASNFromRCN).ReadIntoCollectionRetainingUnmatchedElements();
			}

			var afterReaderPackageStates = Factory.LoadTop1<WhsItemPackageState>(new ZQuery());
			AssertEquals("The package state should be detached from the ASN", Guid.Empty, afterReaderPackageStates.WPS_WRP_ReceiveExpectedPacking);
			AssertContains("Detaching the following Packages from Advanced Shipping Notice ASN1", Logger.Logs);
		}

		#endregion

		#region TestReadIntoCollection_AttachPackageStateForASN

		public void TestReadIntoCollection_AttachPackageStateForASN()
		{
			var shipmentDataObject = UniversalData.ShipmentDataObject;
			var warehouse = UniversalData.Warehouse;
			var rcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var asn = Helper.CreateReceiveASN("ASN1", warehouse.PK);
			var packageState = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-1", TransitWarehouseStatuses.Codes.Booked);

			shipmentDataObject.SetPackingLineCollection(() => null);

			var manager = new Mock<TransitDataObjectReaderHandlerManager>();
			var consolHandler = new TransitReceiveConsolHandler();
			manager.Setup(d => d.GetHandler<TransitReceiveConsolHandler>()).Returns(consolHandler);

			using (ObjectFactory.Substitute("TransitDataObjectReaderHandlerManager", manager.Object))
			{
				var finder = new WhsTransitPackageStateBusinessObjectFinderForASN(FinderType.ByRCN, Factory, rcn: rcn);
				var reader = new WhsItemPackageStateDataObjectCollectionReader(new DataObjectList<PackingLine>(), shipmentDataObject, logger, Factory, finder,
						asn, WhsTransitPackageStatePopulateStrategy.Attach, ProcessType.ASN);

				reader.ReadIntoCollectionRetainingUnmatchedElements();
			}

			var afterReaderPackageStates = Factory.LoadTop1<WhsItemPackageState>(new ZQuery());
			AssertEquals("The package state should be attached from the ASN", asn.PK, afterReaderPackageStates.WPS_WRP_ReceiveExpectedPacking);

			AssertContains("Attaching the following packages to Advanced Shipping Notice ASN1", Logger.Logs);
		}

		#endregion

		#region TestReadIntoCollection_DetachPackageStateForASN

		public void TestReadIntoCollection_DetachPackageStateForASN_WhenCannotDetach()
		{
			var shipmentDataObject = UniversalData.ShipmentDataObject;
			var warehouse = UniversalData.Warehouse;
			var rcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var asn = Helper.CreateReceiveASN("ASN1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU001", warehouse.PK, warehouse.DefaultLocation.PK);
			var packageState = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-1", TransitWarehouseStatuses.Codes.Booked, receiveASN: asn, receiveUnit: rtu);

			shipmentDataObject.SetPackingLineCollection(() => null);

			var manager = new Mock<TransitDataObjectReaderHandlerManager>();
			var consolHandler = new TransitReceiveConsolHandler();
			manager.Setup(d => d.GetHandler<TransitReceiveConsolHandler>()).Returns(consolHandler);

			using (ObjectFactory.Substitute("TransitDataObjectReaderHandlerManager", manager.Object))
			{
				var finder = new WhsTransitPackageStateBusinessObjectFinderForASN(FinderType.ByASN, Factory, asn: asn);

				var reader = new WhsItemPackageStateDataObjectCollectionReader(new DataObjectList<PackingLine>(), shipmentDataObject, logger, Factory, finder,
						asn, WhsTransitPackageStatePopulateStrategy.Detach, ProcessType.ASN);
				AssertExceptionThrown<DataObjectReadFailureException>("Cannot attach the following packages to Advanced Shipping Notice ASN1. These Packages were already received into the Warehouse. System cannot create a Receive Instruction for this ASN.", () => reader.ReadIntoCollectionRetainingUnmatchedElements());
			}
		}

		#endregion

		#region TestReadIntoCollection_AttachPackageStateForDCN

		public void TestReadIntoCollection_AttachPackageStateForDCN()
		{
			var shipmentDataObject = UniversalData.ShipmentDataObject;
			var warehouse = UniversalData.Warehouse;
			var rcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU001", warehouse.PK, warehouse.DefaultLocation.PK);
			var dcn = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var packageState = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-1", TransitWarehouseStatuses.Codes.Booked);
			Factory.SaveForTesting();

			var packingLine = Helper.CreatePackingLine("PKG-1", "TestPackingLine", Constants.PkgUnit.Pallet, 150m, 5m, 5m, 5m, marksAndNumbers: "Furnitures");

			shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine> { packingLine });

			var manager = new Mock<TransitDataObjectReaderHandlerManager>();

			using (ObjectFactory.Substitute("TransitDataObjectReaderHandlerManager", manager.Object))
			{
				var finder = new WhsTransitPackageStateBusinessObjectFinderForDCN(shipmentDataObject, dcn, logger, Factory, WhsTransitPackageStateBusinessObjectFinderForDCN.FindOption.ByPackageID);
				var reader = new WhsItemPackageStateDataObjectCollectionReader(new DataObjectList<PackingLine>(), shipmentDataObject, logger, Factory, finder,
						dcn, WhsTransitPackageStatePopulateStrategy.Attach, ProcessType.DCN);

				reader.ReadIntoCollectionRetainingUnmatchedElements();
			}

			var afterReaderPackageStates = Factory.LoadTop1<WhsItemPackageState>(new ZQuery());
			AssertEquals("The package state should be attached from the DCN", dcn.PK, afterReaderPackageStates.WPS_WDC_TransitDispatchConsignment);
		}

		#endregion

		#region TestReadIntoCollection_UpdatePackageStateForDCN

		public void TestReadIntoCollection_UpdatePackageStateForDCN()
		{
			var shipmentDataObject = UniversalData.ShipmentDataObject;
			var warehouse = UniversalData.Warehouse;
			var rcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU001", warehouse.PK, warehouse.DefaultLocation.PK);
			var dcn = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var packageState = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, dcn);
			Factory.SaveForTesting();

			shipmentDataObject.SetPackingLineCollection(() => null);

			var manager = new Mock<TransitDataObjectReaderHandlerManager>();
			var matachedPKAndPacking = new Dictionary<ZGuid, PackingLine>();
			var packingLine = Helper.CreatePackingLine("PackingLine-01", "TestPackingLine", Constants.PkgUnit.Pallet, 150m, 5m, 5m, 5m, marksAndNumbers: "Furnitures");
			packingLine.IsHighRisk = true;

			matachedPKAndPacking.Add(packageState.Package.PK, packingLine);

			manager.Setup(d => d.GetMatchedPackagePKAndPackingLine()).Returns(matachedPKAndPacking);

			using (ObjectFactory.Substitute("TransitDataObjectReaderHandlerManager", manager.Object))
			{
				var finder = new WhsTransitPackageStateBusinessObjectFinderForDCN(dcn, logger, Factory, WhsTransitPackageStateBusinessObjectFinderForDCN.FindOption.ByDCN);
				var reader = new WhsItemPackageStateDataObjectCollectionReader(new DataObjectList<PackingLine>(), shipmentDataObject, logger, Factory, finder,
						dcn, WhsTransitPackageStatePopulateStrategy.Update, ProcessType.DCN);

				reader.ReadIntoCollectionRetainingUnmatchedElements();
			}

			var afterReaderPackageStates = Factory.LoadTop1<WhsItemPackageState>(new ZQuery());
			AssertEquals("WPS_IsHighRisk", true, afterReaderPackageStates.WPS_IsHighRisk);
			AssertEquals("KP_ExternalReference", "TestPackingLine", afterReaderPackageStates.Package.KP_ExternalReference);
		}

		#endregion

		#region TestReadIntoCollection_DetachPackageStateForDCN

		public void TestReadIntoCollection_DetachPackageStateForDCN()
		{
			var shipmentDataObject = UniversalData.ShipmentDataObject;
			var warehouse = UniversalData.Warehouse;
			var rcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU001", warehouse.PK, warehouse.DefaultLocation.PK);
			var dcn = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var packageState = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, dcn);

			shipmentDataObject.SetPackingLineCollection(() => null);

			var manager = new Mock<TransitDataObjectReaderHandlerManager>();
			manager.Setup(d => d.GetPackagesAlreadyOnDCN()).Returns([]);

			using (ObjectFactory.Substitute("TransitDataObjectReaderHandlerManager", manager.Object))
			{
				var finder = new WhsTransitPackageStateBusinessObjectFinderForDCN(dcn, logger, Factory, WhsTransitPackageStateBusinessObjectFinderForDCN.FindOption.ByDCN);
				var reader = new WhsItemPackageStateDataObjectCollectionReader(new DataObjectList<PackingLine>(), shipmentDataObject, logger, Factory, finder,
						dcn, WhsTransitPackageStatePopulateStrategy.Detach, ProcessType.DCN);

				reader.ReadIntoCollectionRetainingUnmatchedElements();
			}

			var afterReaderPackageStates = Factory.LoadTop1<WhsItemPackageState>(new ZQuery());
			AssertEquals("The package state should be detached from the DCN", ZGuid.Empty, afterReaderPackageStates.WPS_WDC_TransitDispatchConsignment);
		}

		#endregion

		#region TestLogWhenCannotReaderPackageState

		public void TestLogWhenCannotReadPackageState()
		{
			var shipmentDataObject = UniversalData.ShipmentDataObject;
			var warehouse = UniversalData.Warehouse;
			var rcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU001", warehouse.PK, warehouse.DefaultLocation.PK);
			var packageState = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-1", TransitWarehouseStatuses.Codes.Arrived, rtu);
			Factory.SaveForTesting();

			var manager = new Mock<TransitDataObjectReaderHandlerManager>();
			var consolHandler = new TransitReceiveConsolHandler();
			manager.Setup(d => d.GetHandler<TransitReceiveConsolHandler>()).Returns(consolHandler);

			using (ObjectFactory.Substitute("TransitDataObjectReaderHandlerManager", manager.Object))
			{
				var finder = new WhsTransitPackageStateBusinessObjectFinderForRCN(rcn, Factory, Logger, shipmentDataObject, shipmentDataObject);

				var reader = new WhsItemPackageStateDataObjectCollectionReader(shipmentDataObject.PackingLineCollection, shipmentDataObject, Logger, Factory, finder, new WhsItemPackageStateCollection(rcn), rcn, WhsTransitPackageStatePopulateStrategy.New, WhsTransitPackageStateUpdateStrategy.CompleteUpdate, ProcessType.RCN);

				reader.ReadIntoCollection();

				var afterReaderPackageStates = Factory.LoadTop1<WhsItemPackageState>(new ZQuery());
				AssertContains("Receive Consignment has been partially received. Package level data will be ignored and not read in.", Logger.Logs);
			}
		}

		#endregion

		protected TestDataForPacking Data => data ?? (data = new TestDataForPacking(Factory.BOFactory));
		TestDataForPacking data;

		WhsTransitTestHelper Helper => helper ?? (helper = new WhsTransitTestHelper(Factory.BOFactory));
		WhsTransitTestHelper helper;

		TestErrorLogger logger;
		protected TestErrorLogger Logger => logger ?? (logger = new TestErrorLogger());

		TestDataForUniversal UniversalData => universalData ?? new TestDataForUniversal(Factory, Logger);
		readonly TestDataForUniversal universalData;
	}
}
