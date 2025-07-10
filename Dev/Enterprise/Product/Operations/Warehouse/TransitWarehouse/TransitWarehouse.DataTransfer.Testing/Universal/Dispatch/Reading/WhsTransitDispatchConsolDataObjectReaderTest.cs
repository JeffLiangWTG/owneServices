using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.DataTransfer.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Packing.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing
{
	class WhsTransitDispatchConsolDataObjectReaderTest : TransitUniversalTestCase
	{
		#region TestImportConsol

		#region TestImportingConsol_DTUHasDifferentDispatchLoadLists

		public void TestImportingConsol_DTUHasDifferentDispatchLoadLists()
		{
			Data.SetupForForwardingImport();
			var consol = new Shipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			consol.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.ATW, ServiceCode = ServiceCodeType.TWD } } });
			consol.DataContext.AddDataSource(DataContextType.ForwardingConsol, "C123");
			consol.ContainerMode = new ContainerMode { Code = "FCL" };
			consol.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { Data.Orgs.Consignor_CRAHOLSYD, Data.Orgs.Consignee_INTHEMSYD, Data.Orgs.Warehouse_WUFSHIJNB, Data.Orgs.Warehouse_INTHEMSYD });

			consol.SetContainerCollection(() => new DataObjectList<Container>());
			var container1InShipment1 = Data.CreateContainer("A", 1);
			var container2InShipment1 = Data.CreateContainer("B", 2);
			var containerInShipment2 = Data.CreateContainer("C", 3);
			consol.ContainerCollection.Add(container1InShipment1);
			consol.ContainerCollection.Add(container2InShipment1);
			consol.ContainerCollection.Add(containerInShipment2);

			// shipment1
			consol.SetSubShipmentCollection(() => new DataObjectList<Shipment>());
			var shipment1 = Data.CreateShipmentWithPackagesAndContainerLinks("WAYBILLNO1", Tuple.Create((int?)1, "PKG1"), Tuple.Create((int?)1, "PKG2"), Tuple.Create((int?)2, "PKG3"));
			consol.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S123");
			consol.SubShipmentCollection.Add(shipment1);

			// shipment2
			var shipment2 = Data.CreateShipmentWithPackagesAndContainerLinks("WAYBILLNO2", Tuple.Create((int?)2, "PKG4"), Tuple.Create((int?)3, "PKG5"));
			consol.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S456");
			consol.SubShipmentCollection.Add(shipment2);

			Logger.TopLevelDataObject = consol;

			// create receive consignments and packages
			var receiveConsol = Data.CreateReceiveConsolAndConsignmentsInDB(consol);
			AssertEquals("Precondition - Two consignments are created.", 2, receiveConsol.PopulatedConsignmentsForTesting.Length);

			var consolDataObjectReader = new WhsTransitDispatchConsolDataObjectReader(consol, Logger, Factory);
			var consolidation = consolDataObjectReader.ReadIntoBusinessObject();
			AssertEquals("Precondition", DataContextType.TransitDispatchConsol, consolDataObjectReader.DataContextType);

			var headers = consolidation.PopulatedDispatchTransportationUnitsForTesting;
			Factory.SaveForTesting();
			AssertEquals("Three headers must be created for each container.", 3, headers.Count);
			var headerForContainerA = headers.Single(h => h.WDH_VehicleReference == "A");
			var headerForContainerB = headers.Single(h => h.WDH_VehicleReference == "B");
			var headerForContainerC = headers.Single(h => h.WDH_VehicleReference == "C");

			var dtuforAPK = headerForContainerA.DispatchLoadLists.Single().PK;
			var dtuforBPK = headerForContainerB.DispatchLoadLists.Single().PK;
			var dtuforCPK = headerForContainerC.DispatchLoadLists.Single().PK;
			Assert("Precondition : There must be a dispatch load list.", dtuforAPK.IsValid);
			Assert("Precondition : There must be a dispatch load list.", dtuforBPK.IsValid);
			Assert("Precondition : There must be a dispatch load list.", dtuforCPK.IsValid);
			AssertNotEquals("Dispatch load list must be NOT be same for all dtus.", dtuforAPK, dtuforBPK);
			AssertNotEquals("Dispatch load list must be NOT be same for all dtus.", dtuforAPK, dtuforCPK);
			AssertNotEquals("Dispatch load list must be NOT be same for all dtus.", dtuforBPK, dtuforCPK);

			var dispatchLoadList = consolidation.PopulatedDispatchLoadListForTesting;
			Assert("Load list must not be ready to stage.", !dispatchLoadList.WDL_IsReadyToStage);
			AssertNoExceptionThrown("Should not throw errors during the saving.", () => Factory.SaveForTesting());
		}

		public void TestImportingConsol_SomeContainersAreNotAssignedWithPackLines()
		{
			Data.SetupForForwardingImport();
			var consol = new Shipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			consol.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.ATW, ServiceCode = ServiceCodeType.TWD } } });
			consol.DataContext.AddDataSource(DataContextType.ForwardingConsol, "C123");
			consol.ContainerMode = new ContainerMode { Code = "FCL" };
			consol.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { Data.Orgs.Consignor_CRAHOLSYD, Data.Orgs.Consignee_INTHEMSYD, Data.Orgs.Warehouse_WUFSHIJNB, Data.Orgs.Warehouse_INTHEMSYD });

			consol.SetContainerCollection(() => new DataObjectList<Container>());
			var container1InShipment1 = Data.CreateContainer("A", 1);
			var container2InShipment1 = Data.CreateContainer("B", 2);
			var containerNotWithPackLines = Data.CreateContainer("C", 3);
			consol.ContainerCollection.Add(container1InShipment1);
			consol.ContainerCollection.Add(container2InShipment1);
			consol.ContainerCollection.Add(containerNotWithPackLines);

			// shipment1
			consol.SetSubShipmentCollection(() => new DataObjectList<Shipment>());
			var shipment1 = Data.CreateShipmentWithPackagesAndContainerLinks("WAYBILLNO1", Tuple.Create((int?)1, "PKG1"), Tuple.Create((int?)1, "PKG2"), Tuple.Create((int?)2, "PKG3"));
			consol.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S123");
			consol.SubShipmentCollection.Add(shipment1);

			// shipment2
			var shipment2 = Data.CreateShipmentWithPackagesAndContainerLinks("WAYBILLNO2", Tuple.Create((int?)2, "PKG4"));
			consol.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S456");
			consol.SubShipmentCollection.Add(shipment2);

			Logger.TopLevelDataObject = consol;

			// create receive consignments and packages
			var receiveConsol = Data.CreateReceiveConsolAndConsignmentsInDB(consol);
			AssertEquals("Precondition - Two consignments are created.", 2, receiveConsol.PopulatedConsignmentsForTesting.Length);

			var consolDataObjectReader = new WhsTransitDispatchConsolDataObjectReader(consol, Logger, Factory);
			AssertNoExceptionThrown("Allow container without packlines", () => consolDataObjectReader.ReadIntoBusinessObject());
			var loadList = Factory.Load<WhsItemDispatchLoadList>(new ZQuery());
			AssertEquals("Should create DLL for empty container", 2, loadList.Length);
		}

		#endregion

		#region TestImportingConsol_AllContainersAreAssignedAndSomePackagesAreUnAssigned

		public void TestImportingConsol_AllContainersAreAssignedAndSomePackagesAreUnAssigned()
		{
			Data.SetupForForwardingImport();
			var consol = new Shipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			consol.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.ATW, ServiceCode = ServiceCodeType.TWD } } });
			consol.DataContext.AddDataSource(DataContextType.ForwardingConsol, "C123");
			consol.ContainerMode = new ContainerMode { Code = "FCL" };
			consol.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { Data.Orgs.Consignor_CRAHOLSYD, Data.Orgs.Consignee_INTHEMSYD, Data.Orgs.Warehouse_WUFSHIJNB, Data.Orgs.Warehouse_INTHEMSYD });

			consol.SetContainerCollection(() => new DataObjectList<Container>());
			var container1InShipment1 = Data.CreateContainer("A", 1);
			var container2InShipment1 = Data.CreateContainer("B", 2);
			consol.ContainerCollection.Add(container1InShipment1);
			consol.ContainerCollection.Add(container2InShipment1);

			// shipment1
			consol.SetSubShipmentCollection(() => new DataObjectList<Shipment>());
			var shipment1 = Data.CreateShipmentWithPackagesAndContainerLinks("WAYBILLNO1", Tuple.Create((int?)1, "PKG1"), Tuple.Create((int?)1, "PKG2"),
				Tuple.Create((int?)2, "PKG3"), Tuple.Create((int?)null, "PKG4"), Tuple.Create((int?)null, "PKG5"));
			consol.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S123");
			consol.SubShipmentCollection.Add(shipment1);

			Logger.TopLevelDataObject = consol;

			// create receive consignments and packages
			var receiveConsol = Data.CreateReceiveConsolAndConsignmentsInDB(consol);
			AssertEquals("Precondition - One consignments is created.", 1, receiveConsol.PopulatedConsignmentsForTesting.Length);

			var consolDataObjectReader = new WhsTransitDispatchConsolDataObjectReader(consol, Logger, Factory);
			AssertExceptionThrown(typeof(DataObjectReadFailureException), @"Cannot determine Loading Plan for Packages PKG4, PKG5 as they have not been allocated to any Container.
If all Containers have a Loading Plan(allocated Packages), then all Packages are required to be allocated to a Container.",
				() => consolDataObjectReader.ReadIntoBusinessObject());
		}

		#endregion

		#region TestImportingConsol_CreateConsignmentForEachShipment

		public void TestImportingConsol_CreateConsignmentForEachShipment()
		{
			Data.SetupForForwardingImport();
			var consol = new Shipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			consol.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.ATW, ServiceCode = ServiceCodeType.TWD } } });
			consol.DataContext.AddDataSource(DataContextType.ForwardingConsol, "C123");
			consol.ContainerMode = new ContainerMode { Code = "FCL" };
			consol.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { Data.Orgs.Consignor_CRAHOLSYD, Data.Orgs.Consignee_INTHEMSYD, Data.Orgs.Warehouse_WUFSHIJNB, Data.Orgs.Warehouse_INTHEMSYD });

			consol.SetContainerCollection(() => new DataObjectList<Container>());
			var container = Data.CreateContainer("A", 1);
			consol.ContainerCollection.Add(container);

			// shipment1
			consol.SetSubShipmentCollection(() => new DataObjectList<Shipment>());
			var shipment1 = Data.CreateShipmentWithPackagesAndContainerLinks("WAYBILLNO1", Tuple.Create((int?)1, "PKG1"));
			consol.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S123");
			consol.SubShipmentCollection.Add(shipment1);

			// shipment2
			var shipment2 = Data.CreateShipmentWithPackagesAndContainerLinks("WAYBILLNO2", Tuple.Create((int?)1, "PKG2"));
			consol.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S456");
			consol.SubShipmentCollection.Add(shipment2);

			// create receive consignments and packages
			var receiveConsol = Data.CreateReceiveConsolAndConsignmentsInDB(consol);
			AssertEquals("Precondition - Two consignments are created.", 2, receiveConsol.PopulatedConsignmentsForTesting.Length);

			var consolDataObjectReader = new WhsTransitDispatchConsolDataObjectReader(consol, Data.Logger, Factory);
			var consolidation = consolDataObjectReader.ReadIntoBusinessObject();

			var dispatchConsignments = consolidation.PopulatedConsignmentsForTesting;
			AssertEquals("Two dispatch consignments should be created for two shipments.", 2, dispatchConsignments.Count);
			AssertContainsExactElementsInAnyOrder(new[] { "WAYBILLNO1", "WAYBILLNO2" }, dispatchConsignments.Select(d => d.WDC_ConsignmentID));
			AssertNoExceptionThrown("Should not throw errors during the saving.", () => Factory.SaveForTesting());
		}

		#endregion

		#region TestImportConsol_WhenConsolHasMultipleShipments_ThrowsException_ErrorsFromEveryShipmentShouldBeLogged

		public void TestImportConsol_WhenConsolHasMultipleShipments_ThrowsException_ErrorsFromEveryShipmentShouldBeLogged()
		{
			Data.SetupForForwardingImport();
			Data.ShipmentDataObject.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.DTW, ServiceCode = ServiceCodeType.TWD } } });
			var consignmentDataObject = Data.CreateShipmentWithPackages("A123", new[] { "Pack1", "Pack2" });

			Factory.SaveForTesting();

			var shipment1 = Data.CreateShipmentWithPackages("A123", "Pack1", "Pack2");
			var shipment2 = Data.CreateShipmentWithPackages("B456", "Pack3", "Pack4", "Pack5");
			Data.ShipmentDataObject.SetSubShipmentCollection(() => new DataObjectList<Shipment> { shipment1, shipment2 });

			Logger.TopLevelDataObject = Data.ShipmentDataObject;
			var consolDataObjectReader = new WhsTransitDispatchConsolDataObjectReader(Data.ShipmentDataObject, Logger, Factory);
			AssertExceptionThrown<DataObjectReadFailureException>("Cannot Import Dispatch Consignment as no House Bill was provided.",
@"The following Sub Shipments encountered errors when importing ForwardingShipment - S1000000:
Data Source
ForwardingShipment - A123
ForwardingShipment - B456
Error importing ForwardingShipment - A123:
Failed to match valid Packages in Transit Warehouse 'TWH'.

Could not find the following Package IDs: PACK1, PACK2
These Packages may have already departed, may not be Booked, or have been relabeled.
You may resend the Receive Outturn to update all Package IDs or otherwise check these Packages via the Transit Warehouse Desktop.

Error importing ForwardingShipment - B456:
Failed to match valid Packages in Transit Warehouse 'TWH'.

Could not find the following Package IDs: PACK3, PACK4, PACK5
These Packages may have already departed, may not be Booked, or have been relabeled.
You may resend the Receive Outturn to update all Package IDs or otherwise check these Packages via the Transit Warehouse Desktop.
",
				() => consolDataObjectReader.ReadIntoBusinessObject());
		}

		#endregion

		#region TestImportingConsol_DispatchPackageStatesArePopulated

		public void TestImportingConsol_DispatchPackageStatesArePopulated()
		{
			Data.SetupForForwardingImport();
			var consol = new Shipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			consol.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.ATW, ServiceCode = ServiceCodeType.TWD } } });
			consol.DataContext.AddDataSource(DataContextType.ForwardingConsol, "C123");
			consol.ContainerMode = new ContainerMode { Code = "FCL" };
			consol.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { Data.Orgs.Consignor_CRAHOLSYD, Data.Orgs.Consignee_INTHEMSYD, Data.Orgs.Warehouse_WUFSHIJNB, Data.Orgs.Warehouse_INTHEMSYD });

			consol.SetContainerCollection(() => new DataObjectList<Container>());
			var container = Data.CreateContainer("A", 1);
			consol.ContainerCollection.Add(container);

			// shipment1
			consol.SetSubShipmentCollection(() => new DataObjectList<Shipment>());
			var shipment1 = Data.CreateShipmentWithPackagesAndContainerLinks("WAYBILLNO1", Tuple.Create((int?)1, "PKG1"), Tuple.Create((int?)1, "PKG2"));
			consol.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S123");
			consol.SubShipmentCollection.Add(shipment1);

			Logger.TopLevelDataObject = consol;

			// create receive consignments and packages
			var receiveConsol = Data.CreateReceiveConsolAndConsignmentsInDB(consol);
			AssertEquals("Precondition - One consignment is created.", 1, receiveConsol.PopulatedConsignmentsForTesting.Length);

			var consolDataObjectReader = new WhsTransitDispatchConsolDataObjectReader(consol, Logger, Factory);
			var consolidation = consolDataObjectReader.ReadIntoBusinessObject();
			AssertContains("Attempting to create Container Loading Plans as Containers have been specified.", Logger.Logs);
			AssertContains("Creating 1 Load List(s) for Containers with a Loading Plan.", Logger.Logs);

			var dispatchConsignment = consolidation.PopulatedConsignmentsForTesting.Single();
			var packageStates = dispatchConsignment.PackageStates;
			AssertEquals("Two package states must be created.", 2, packageStates.Count);
			foreach (var packageState in packageStates)
			{
				AssertEquals("Dispatch header must not be populated.", ZGuid.Empty, packageState.WPS_WDH_TransitDispatchHeader);
				AssertEquals("Load List must be populated.", consolidation.PopulatedDispatchLoadListForTesting.PK, packageState.WPS_WDL_LoadList);
				AssertEquals("Package job must be receive consignment.", packageState.ReceiveConsignment, packageState.Package.PackageJob.ParentJob);
			}
			AssertNoExceptionThrown("Should not throw errors during the saving.", () => Factory.SaveForTesting());
		}

		public void TestImportingConsol_DispatchPackageStatesArePopulated_SomePackagesWithoutIds()
		{
			Data.SetupForForwardingImport();
			var consol = new Shipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			consol.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.ATW, ServiceCode = ServiceCodeType.TWD } } });
			consol.DataContext.AddDataSource(DataContextType.ForwardingConsol, "C123");
			consol.ContainerMode = new ContainerMode { Code = "FCL" };
			consol.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { Data.Orgs.Consignor_CRAHOLSYD, Data.Orgs.Consignee_INTHEMSYD, Data.Orgs.Warehouse_WUFSHIJNB, Data.Orgs.Warehouse_INTHEMSYD });

			consol.SetContainerCollection(() => new DataObjectList<Container>());
			var container = Data.CreateContainer("A", 1);
			consol.ContainerCollection.Add(container);

			// shipment1
			consol.SetSubShipmentCollection(() => new DataObjectList<Shipment>());
			var shipment1 = Data.CreateShipmentWithPackagesAndContainerLinks("WAYBILLNO1", Tuple.Create((int?)1, "PKG1"), Tuple.Create((int?)1, "PKG2"));
			consol.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S123");
			consol.SubShipmentCollection.Add(shipment1);

			Logger.TopLevelDataObject = consol;

			// create receive consignments and packages
			var receiveConsol = Data.CreateReceiveConsolAndConsignmentsInDB(consol);
			var rcn = receiveConsol.PopulatedConsignmentsForTesting.Single();
			AssertEquals("Precondition", 2, rcn.PackageStates.Count);

			var consolDataObjectReader = new WhsTransitDispatchConsolDataObjectReader(consol, Logger, Factory);
			var consolidation = consolDataObjectReader.ReadIntoBusinessObject();
			AssertContains("Attempting to create Container Loading Plans as Containers have been specified.", Logger.Logs);
			AssertContains("Creating 1 Load List(s) for Containers with a Loading Plan.", Logger.Logs);

			var dispatchConsignment = consolidation.PopulatedConsignmentsForTesting.Single();
			AssertEquals("DCN must have two package states.", 2, dispatchConsignment.PackageStates.Count);
			CombineAssertions(() =>
			{
				var pkg1PackageStateForPKG1 = dispatchConsignment.PackageStates.Single(p => p.Package.KP_PackageID == "PKG1");
				AssertEquals("PKG1's Dispatch header must not be populated.", ZGuid.Empty, pkg1PackageStateForPKG1.WPS_WDH_TransitDispatchHeader);
				AssertEquals("PKG1's Load List must be populated.", consolidation.PopulatedDispatchLoadListForTesting.PK, pkg1PackageStateForPKG1.WPS_WDL_LoadList);
				AssertEquals("PKG1's Package job must be receive consignment.", pkg1PackageStateForPKG1.ReceiveConsignment, pkg1PackageStateForPKG1.Package.PackageJob.ParentJob);

				var packageStateForPKG2 = dispatchConsignment.PackageStates.Single(p => p.Package.KP_PackageID == "PKG2");
				AssertEquals("Package state with no ID's Dispatch header must not be populated.", ZGuid.Empty, packageStateForPKG2.WPS_WDH_TransitDispatchHeader);
				AssertEquals("Package state with no ID's Load List must not be populated.", consolidation.PopulatedDispatchLoadListForTesting.PK, packageStateForPKG2.WPS_WDL_LoadList);
				AssertEquals("Package state with no ID's Package job must be receive consignment.", pkg1PackageStateForPKG1.ReceiveConsignment, packageStateForPKG2.Package.PackageJob.ParentJob);

				AssertNoExceptionThrown("Should not throw errors during the saving.", () => Factory.SaveForTesting());
			});
		}

		public void TestImportingConsol_DispatchPackageStatesToBePickedArePopulated()
		{
			Data.SetupForForwardingImport();
			var consol = new Shipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			consol.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.DTW, ServiceCode = ServiceCodeType.TWD } } });
			consol.DataContext.AddDataSource(DataContextType.ForwardingConsol, "C123");
			consol.ContainerMode = new ContainerMode { Code = "FCL" };
			consol.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { Data.Orgs.Consignor_CRAHOLSYD, Data.Orgs.Consignee_INTHEMSYD, Data.Orgs.Warehouse_WUFSHIJNB });
			consol.SetSubShipmentCollection(() => new DataObjectList<Shipment>());

			// create consignment and packages
			var consignmentDataObject = Data.CreateShipmentWithPackages("A123", new[] { "Pack1", "Pack2", "Pack3" });
			Data.CreateReceiveConsignmentInDB(consignmentDataObject);
			var location = Data.Warehouse.DefaultOutboundDockDoorLocation;
			location.WLV_WA_PickingArea = Factory.NewWithValidTestData<WhsArea>().PK;
			location.WLV_WA_PutawayArea = Factory.NewWithValidTestData<WhsArea>().PK;
			var rtu = Factory.NewWithValidTestData<WhsItemReceiveTransportationUnit>();
			rtu.WRH_WW_Warehouse = Data.Warehouse.PK;

			var packagesToBePicked = Factory.Load<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_IsSecure, false));
			AssertEquals("Three packages in consignment.", 3, packagesToBePicked.Length);

			// setting package status to PUT, PIC, CTT
			packagesToBePicked[0].WPS_WL_LastLocation = location.PK;
			packagesToBePicked[0].WPS_WRH_TransitReceiveHeader = rtu.PK;
			packagesToBePicked[0].WPS_Status = TransitWarehouseStatuses.Codes.Putaway;
			packagesToBePicked[1].WPS_WL_LastLocation = location.PK;
			packagesToBePicked[1].WPS_WRH_TransitReceiveHeader = rtu.PK;
			packagesToBePicked[1].WPS_Status = TransitWarehouseStatuses.Codes.Picked;
			packagesToBePicked[2].WPS_WL_LastLocation = location.PK;
			packagesToBePicked[2].WPS_WRH_TransitReceiveHeader = rtu.PK;
			packagesToBePicked[2].WPS_Status = TransitWarehouseStatuses.Codes.Committed;

			Factory.SaveForTesting();

			var shipment = Data.CreateShipmentWithPackages("A123", "Pack1", "Pack2", "Pack3");
			AssertEquals("Precondition - Three packages in shipment.", 3, shipment.PackingLineCollection.Count);

			consol.SubShipmentCollection.Add(shipment);

			Logger.TopLevelDataObject = consol;
			var consolDataObjectReader = new WhsTransitDispatchConsolDataObjectReader(consol, Logger, Factory);
			var consolidation = consolDataObjectReader.ReadIntoBusinessObject();

			var dispatchConsignment = consolidation.PopulatedConsignmentsForTesting.Single();
			var packageStates = dispatchConsignment.PackageStates;
			AssertEquals("Three package states must be created.", 3, packageStates.Count);

			foreach (var packageState in packageStates)
			{
				AssertEquals("Dispatch header must not be populated.", ZGuid.Empty, packageState.WPS_WDH_TransitDispatchHeader);
				AssertEquals("Load List must be populated.", consolidation.PopulatedDispatchLoadListForTesting.PK, packageState.WPS_WDL_LoadList);
				AssertEquals("Package job must be receive consignment.", packageState.ReceiveConsignment, packageState.Package.PackageJob.ParentJob);
			}
			AssertNoExceptionThrown("Should not throw errors during the saving.", () => Factory.SaveForTesting());
		}

		#endregion

		#region TestImportingConsol_SameShipmentCanBeImportedAgain

		public void TestImportingConsol_SameShipmentCanBeImportedAgain()
		{
			Data.SetupForForwardingImport();
			var consol = new Shipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			consol.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.ATW, ServiceCode = ServiceCodeType.TWD } } });
			consol.DataContext.AddDataSource(DataContextType.ForwardingConsol, "C123");
			consol.ContainerMode = new ContainerMode { Code = "FCL" };
			consol.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { Data.Orgs.Consignor_CRAHOLSYD, Data.Orgs.Consignee_INTHEMSYD, Data.Orgs.Warehouse_WUFSHIJNB, Data.Orgs.Warehouse_INTHEMSYD });

			consol.SetContainerCollection(() => new DataObjectList<Container>());
			var container = Data.CreateContainer("A", 1);
			consol.ContainerCollection.Add(container);

			// shipment
			consol.SetSubShipmentCollection(() => new DataObjectList<Shipment>());
			var shipment1 = Data.CreateShipmentWithPackagesAndContainerLinks("WAYBILLNO1", Tuple.Create((int?)1, "PKG1"), Tuple.Create((int?)1, "PKG2"));
			consol.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S123");
			consol.SubShipmentCollection.Add(shipment1);

			Logger.TopLevelDataObject = consol;

			// create receive consignments and packages
			var receiveConsol = Data.CreateReceiveConsolAndConsignmentsInDB(consol);
			AssertEquals("Precondition - One consignment is created.", 1, receiveConsol.PopulatedConsignmentsForTesting.Length);

			var consolDataObjectReader = new WhsTransitDispatchConsolDataObjectReader(consol, Logger, Factory);
			var consolidation = consolDataObjectReader.ReadIntoBusinessObject();

			var dispatchConsignment = consolidation.PopulatedConsignmentsForTesting.Single();
			AssertEquals("Precondition: There must be two package States.", 2, dispatchConsignment.PackageStates.Count);
			AssertNoExceptionThrown("Should not throw errors during the saving.", () => Factory.SaveForTesting());

			// Read UXML file again
			var newFactory = new UniversalObjectFactory();
			var newConsolReader = new WhsTransitDispatchConsolDataObjectReader(consol, Logger, newFactory);
			var newConsolidation = newConsolReader.ReadIntoBusinessObject();
			AssertEquals("Load list must be matched.", consolidation.PopulatedDispatchLoadListForTesting.PK, newConsolidation.PopulatedDispatchLoadListForTesting.PK);
			AssertEquals("Dispatch header must be same.", consolidation.PopulatedDispatchTransportationUnitsForTesting.Single().PK, newConsolidation.PopulatedDispatchTransportationUnitsForTesting.Single().PK);
			AssertEquals("Consignment must be same.", consolidation.PopulatedConsignmentsForTesting.Single().PK, newConsolidation.PopulatedConsignmentsForTesting.Single().PK);

			var newPackageStates = newFactory.RowFactory.Load(WhsItemPackageStateSchema.Constants.TableName,
				new ZQuery(WhsItemPackageStateSchema.WPS_WDC_TransitDispatchConsignment, newConsolidation.PopulatedConsignmentsForTesting.Select(c => c.PK)));
			foreach (var newPackageState in newPackageStates)
			{
				AssertEquals("All package states must have the same load list.", newConsolidation.PopulatedDispatchLoadListForTesting.PK,
					DataObjectReader.GetColumnIndexerFromRow(newPackageState).GetValue(WhsItemPackageStateSchema.WPS_WDL_LoadList));
			}

			AssertNoExceptionThrown("Should not throw errors during the second saving.", () => newFactory.SaveForTesting());
		}

		#endregion

		#region TestImportConsol_ForwarderModifiesContainersAndReimports

		public void TestImportConsol_ForwarderRemovesContainersAndReimports_DetachesPackagesAndDeactivatesUnusedLoadLists()
		{
			Data.SetupForForwardingImport();
			var consignmentDataObject1 = Data.CreateShipmentWithPackages("S00000001", "PKG-1");
			var consignmentDataObject2 = Data.CreateShipmentWithPackages("S00000002", "PKG-2");
			var consignmentDataObject3 = Data.CreateShipmentWithPackages("S00000003", "PKG-3");
			var consol = new Shipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			consol.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.DTW, ServiceCode = ServiceCodeType.TWD } } });
			consol.DataContext.AddDataSource(DataContextType.ForwardingConsol, "CS00000001");
			consol.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S00000001");
			consol.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { Data.Orgs.Consignor_CRAHOLSYD, Data.Orgs.Consignee_INTHEMSYD, Data.Orgs.Warehouse_WUFSHIJNB, Data.Orgs.Warehouse_INTHEMSYD });

			// Create the corresponding transit jobs
			var receiveConsignment1 = Data.CreateReceiveConsignmentInDB(consignmentDataObject1);
			var receiveConsignment2 = Data.CreateReceiveConsignmentInDB(consignmentDataObject2);
			var receiveConsignment3 = Data.CreateReceiveConsignmentInDB(consignmentDataObject3);

			// Setup a load list for a container (to be found but have packages reassigned to a new load list)
			var utcNow = ZDateTime.UtcNow;
			var loadList = Factory.NewWithValidTestData<WhsItemDispatchLoadList>();
			loadList.WDL_JobID = "DLL00000010";
			loadList.WDL_WW_Warehouse = Data.Warehouse.PK;
			Helper.CreateAdditionalReference(loadList, "CS00000001", WarehouseAdditionalReferenceTypes.Codes.ForwardingConsolNumber);
			var dtu = Helper.CreateDispatchTransportationUnitWithContainerType("Cont1", Data.Warehouse.PK, containerID: "Cont1");
			Helper.CreateDispatchDLLDTUPivot(loadList.PK, dtu.PK);
			loadList.WDL_SystemCreateTimeUtc = utcNow.AddDays(-1);

			receiveConsignment1.PackageStates.Single().WPS_WDL_LoadList = loadList.PK;
			receiveConsignment2.PackageStates.Single().WPS_WDL_LoadList = loadList.PK;

			// Setup a load list for a container (to be matched but have packages detached)
			var loadList2 = Factory.NewWithValidTestData<WhsItemDispatchLoadList>();
			loadList2.WDL_JobID = "DLL00000020";
			loadList2.WDL_WW_Warehouse = Data.Warehouse.PK;
			Helper.CreateAdditionalReference(loadList2, "CS00000001", WarehouseAdditionalReferenceTypes.Codes.ForwardingConsolNumber);
			var dtu2 = Helper.CreateDispatchTransportationUnitWithContainerType("Cont2", Data.Warehouse.PK, containerID: "Cont2");
			Helper.CreateDispatchDLLDTUPivot(loadList2.PK, dtu2.PK);
			loadList2.WDL_WL_StagingLocation = Data.Warehouse.DefaultLocation.PK;
			loadList2.WDL_IsReadyToStage = true;
			loadList2.WDL_SystemCreateTimeUtc = utcNow;

			receiveConsignment3.PackageStates.Single().WPS_WDL_LoadList = loadList2.PK;

			Factory.SaveForTesting();

			// Read in the consol, but without the containers and one shipment
			Logger.TopLevelDataObject = consol;
			consol.SetSubShipmentCollection(() => new DataObjectList<Shipment> { consignmentDataObject1 });
			var reader = new WhsTransitDispatchConsolDataObjectReader(consol, Logger, Factory);
			var loadListAfterImport = reader.ReadIntoBusinessObject().PopulatedDispatchLoadListForTesting;

			Factory.SaveForTesting();

			var factoryAfterImport = new UniversalObjectFactory();
			AssertNotEquals("Should have replaced the old load list.", loadList.PK, loadListAfterImport.PK);
			AssertEquals("Should have reused the most recent load list.", loadList2.PK, loadListAfterImport.PK);
			AssertContainsExactElementsInAnyOrder("The included shipment's package should be assigned to the new load list.",
				new string[] { "PKG-1" }, loadListAfterImport.PackageStates.Select(p => p.Package.KP_PackageID));

			var oldloadListAfterImport = factoryAfterImport.Load<WhsItemDispatchLoadList>(loadList.PK);
			AssertContainsExactElementsInAnyOrder("The excluded shipment's package should be detached from the old load list.",
				Array.Empty<string>(), oldloadListAfterImport.PackageStates.Select(p => p.Package.KP_PackageID));
			AssertEquals("The replaced load lists should be stopped.", false, oldloadListAfterImport.WDL_IsReadyToStage);
			AssertEquals("The replaced load lists should be deactivated.", false, oldloadListAfterImport.WDL_IsActive);
			AssertEquals("The replaced load list should not be completed.", false, oldloadListAfterImport.WDL_CompleteTime.IsValid);
			AssertEquals("The replaced load list should have no related DTUs.", 0, oldloadListAfterImport.DispatchTransportationUnits.Count);
			AssertEquals(1, oldloadListAfterImport.Logs.Find(l => l.SL_SE_NKEvent == Events.SetToInactiveCode).Count());
			AssertContains("It should log the deactivated load list", $"Deactivating the following Load Lists as all their Packages were reassigned: {oldloadListAfterImport.WDL_JobID}", Logger.Logs);
		}

		public void TestImportConsol_ForwarderAddsContainersAndReimports_DetachesPackagesAndDeactivatesLooseLoadList()
			=> TestImportConsol_ForwarderAddsContainersAndReimports(originalLoadListIsActive: true);

		public void TestImportConsol_ForwarderAddsContainersAndReimports_OriginalLoadListIsAlreadyDeactive()
			=> TestImportConsol_ForwarderAddsContainersAndReimports(originalLoadListIsActive: false);

		void TestImportConsol_ForwarderAddsContainersAndReimports(bool originalLoadListIsActive)
		{
			Data.SetupForForwardingImport();
			const string shipmentNumber1 = "S00000001";
			const string shipmentNumber2 = "S00000002";
			var consignmentDataObject1 = Data.CreateShipmentWithPackages(shipmentNumber1, "PKG-1");
			var consignmentDataObject2 = Data.CreateShipmentWithPackages(shipmentNumber2, "PKG-2");
			var consol = new Shipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			consol.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.DTW, ServiceCode = ServiceCodeType.TWD } } });
			consol.DataContext.AddDataSource(DataContextType.ForwardingConsol, "CS00000001");
			consol.DataContext.AddDataSource(DataContextType.ForwardingShipment, shipmentNumber1);
			consol.ContainerMode = new ContainerMode { Code = "FCL" };
			consol.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { Data.Orgs.Consignor_CRAHOLSYD, Data.Orgs.Consignee_INTHEMSYD, Data.Orgs.Warehouse_WUFSHIJNB, Data.Orgs.Warehouse_INTHEMSYD });

			// Create the corresponding transit jobs
			var receiveConsignment1 = Data.CreateReceiveConsignmentInDB(consignmentDataObject1);
			var receiveConsignment2 = Data.CreateReceiveConsignmentInDB(consignmentDataObject2);

			// Setup a load list for the consol (To be found but replaced by the reader)
			var loadList = Factory.NewWithValidTestData<WhsItemDispatchLoadList>();
			loadList.WDL_JobID = "DLL00000010";
			loadList.WDL_WW_Warehouse = Data.Warehouse.PK;
			Helper.CreateAdditionalReference(loadList, "CS00000001", WarehouseAdditionalReferenceTypes.Codes.ForwardingConsolNumber);

			// Deactivate load lists, remove packages
			loadList.WDL_IsActive = originalLoadListIsActive;
			loadList.WDL_WL_StagingLocation = Data.Warehouse.DefaultLocation.PK;

			if (originalLoadListIsActive)
			{
				loadList.WDL_IsReadyToStage = true;

				receiveConsignment1.PackageStates.Single().WPS_WDL_LoadList = loadList.PK;
				receiveConsignment2.PackageStates.Single().WPS_WDL_LoadList = loadList.PK;
			}

			Factory.SaveForTesting();

			// Read in the consol, but with a container and without one of its shipments
			consol.SetContainerCollection(() => new DataObjectList<Container>());
			var container = Data.CreateContainer("A", 1);
			consol.ContainerCollection.Add(container);
			consignmentDataObject1.PackingLineCollection.Single().ContainerLink = 1;
			Logger.TopLevelDataObject = consol;
			consol.SetSubShipmentCollection(() => new DataObjectList<Shipment> { consignmentDataObject1 });
			var reader = new WhsTransitDispatchConsolDataObjectReader(consol, Logger, Factory);
			var newLoadListAfterImport = reader.ReadIntoBusinessObject().PopulatedDispatchLoadListForTesting;

			Factory.SaveForTesting();

			var factoryAfterImport = new UniversalObjectFactory();

			AssertContainsExactElementsInAnyOrder("The included shipment's package should be assigned to the new load list.",
				new string[] { "PKG-1" }, newLoadListAfterImport.PackageStates.Select(p => p.Package.KP_PackageID));
			var loadListAfterImport = factoryAfterImport.Load<WhsItemDispatchLoadList>(loadList.PK);

			Assert("The replaced load list should not be completed.", loadListAfterImport.WDL_CompleteTime.IsEmpty);
			Assert("The replaced load list should be stopped.", !loadListAfterImport.WDL_IsReadyToStage);

			if (originalLoadListIsActive)
			{
				Assert("The load list should be active.", loadListAfterImport.WDL_IsActive);
				AssertEquals("The package should not be detached from the load list.", 1, loadListAfterImport.PackageStates.Count);
				AssertEquals("Should not have replaced the loose load list.", loadList.PK, newLoadListAfterImport.PK);
			}
			else
			{
				Assert("The replaced load list should be deactivated.", !loadListAfterImport.WDL_IsActive);
				AssertEquals("The excluded shipment's package should be detached from the old load list.", 0, loadListAfterImport.PackageStates.Count);
				AssertNotEquals("Should have replaced the loose load list.", loadList.PK, newLoadListAfterImport.PK);
				AssertNotContains("It should not log deactivating the load list", $"Deactivating the following Load Lists as all their Packages were reassigned: {loadList.WDL_JobID}", Logger.Logs);
			}
		}

		public void TestImportConsol_ForwarderAddsContainersAndReimports_PackagesCannotBeDetached()
		{
			Data.SetupForForwardingImport();
			const string shipmentNumber1 = "S00000001";
			const string shipmentNumber2 = "S00000002";
			var consignmentDataObject1 = Data.CreateShipmentWithPackages(shipmentNumber1, "PKG-1");
			var consignmentDataObject2 = Data.CreateShipmentWithPackages(shipmentNumber2, "PKG-2");
			var consol = new Shipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			consol.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.DTW, ServiceCode = ServiceCodeType.TWD } } });
			consol.DataContext.AddDataSource(DataContextType.ForwardingConsol, "CS00000001");
			consol.DataContext.AddDataSource(DataContextType.ForwardingShipment, shipmentNumber1);
			consol.ContainerMode = new ContainerMode { Code = "FCL" };
			consol.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { Data.Orgs.Consignor_CRAHOLSYD, Data.Orgs.Consignee_INTHEMSYD, Data.Orgs.Warehouse_WUFSHIJNB, Data.Orgs.Warehouse_INTHEMSYD });

			// Create the corresponding transit jobs
			var receiveConsignment1 = Data.CreateReceiveConsignmentInDB(consignmentDataObject1);
			var receiveConsignment2 = Data.CreateReceiveConsignmentInDB(consignmentDataObject2);

			// Read in the consol, but without containers and with both shipments
			consol.SetContainerCollection(() => new DataObjectList<Container>());
			Logger.TopLevelDataObject = consol;
			consol.SetSubShipmentCollection(() => new DataObjectList<Shipment> { consignmentDataObject1, consignmentDataObject2 });
			var reader = new WhsTransitDispatchConsolDataObjectReader(consol, Logger, Factory);
			var loadList = reader.ReadIntoBusinessObject().PopulatedDispatchLoadListForTesting;
			loadList.WDL_WL_StagingLocation = Data.Warehouse.DefaultLocation.PK;
			loadList.WDL_IsReadyToStage = true;

			Factory.SaveForTesting();

			AssertContainsExactElementsInAnyOrder("Precondition: The included shipments' packages should be assigned to the load list.",
				new string[] { "PKG-1", "PKG-2" }, loadList.PackageStates.Select(p => p.Package.KP_PackageID));

			var rtu = Helper.CreateReceiveTransportationUnit("Veh1", Data.Warehouse.PK, Data.Warehouse.DefaultLocation.PK);

			var matchedPackageState = loadList.PackageStates.Single(p => p.Package.KP_PackageID == "PKG-1");
			matchedPackageState.Reload();
			matchedPackageState.WPS_WRH_TransitReceiveHeader = rtu.PK;
			matchedPackageState.WPS_WL_LastLocation = Data.Warehouse.DefaultLocation.PK;
			matchedPackageState.WPS_WDL_LoadList = loadList.PK;

			var unmatchedPackageState = loadList.PackageStates.Single(p => p.Package.KP_PackageID == "PKG-2");
			unmatchedPackageState.Reload();
			unmatchedPackageState.WPS_WRH_TransitReceiveHeader = rtu.PK;
			unmatchedPackageState.WPS_WL_LastLocation = Data.Warehouse.DefaultLocation.PK;
			unmatchedPackageState.WPS_WDL_LoadList = loadList.PK;

			var incompleteDTU = Helper.CreateDispatchTransportationUnitWithContainerType("Cont1", Data.Warehouse.PK, containerID: "Cont1");
			Helper.CreateDispatchDLLDTUPivot(loadList.PK, incompleteDTU.PK);
			Helper.LoadPackage(incompleteDTU, matchedPackageState);

			var completeDTU = Helper.CreateDispatchTransportationUnitWithContainerType("Cont2", Data.Warehouse.PK, containerID: "Cont2");
			Helper.CreateDispatchDLLDTUPivot(loadList.PK, completeDTU.PK);
			Helper.LoadPackage(completeDTU, unmatchedPackageState);
			var now = DateTimeOffset.Now;
			Factory.SaveForTesting();
			Helper.FinishLoading(completeDTU, now.AddDays(-2), now, ZDateTimeOffset.Empty);

			Factory.SaveForTesting();

			// Read in the consol, but with a container and without one of its shipments
			consol.SetContainerCollection(() => new DataObjectList<Container>());
			var container = Data.CreateContainer("A", 1);
			consol.ContainerCollection.Add(container);
			consignmentDataObject1.PackingLineCollection.Single().ContainerLink = 1;
			Logger.TopLevelDataObject = consol;
			consol.SetSubShipmentCollection(() => new DataObjectList<Shipment> { consignmentDataObject1 });
			var reader2 = new WhsTransitDispatchConsolDataObjectReader(consol, Logger, Factory);
			var newLoadListAfterImport = reader2.ReadIntoBusinessObject().PopulatedDispatchLoadListForTesting;

			Factory.SaveForTesting();

			AssertNotEquals("Should have replaced the loose load list.", loadList.PK, newLoadListAfterImport.PK);
			AssertContainsExactElementsInAnyOrder("The included shipment's package should be assigned to the new load list.",
				new string[] { "PKG-1" }, newLoadListAfterImport.PackageStates.Select(p => p.Package.KP_PackageID));

			var factoryAfterImport = new UniversalObjectFactory();
			var oldLoadListAfterImport = factoryAfterImport.Load<WhsItemDispatchLoadList>(loadList.PK);
			AssertContainsExactElementsInAnyOrder("The package that is on a load complete DTU should not be detached from the old load list.",
				new string[] { "PKG-2" }, oldLoadListAfterImport.PackageStates.Select(p => p.Package.KP_PackageID));
			AssertEquals("The replaced load list should not be changed.", true, oldLoadListAfterImport.WDL_IsActive);
			AssertEquals("The replaced load list should be stopped.", false, oldLoadListAfterImport.WDL_IsReadyToStage);
			AssertEquals("The replaced load list should not be completed.", false, oldLoadListAfterImport.WDL_CompleteTime.IsValid);
			AssertEquals("The replaced load list should have related DTUs.", 2, oldLoadListAfterImport.DispatchTransportationUnits.Count);
			AssertNotContains("It should not log deactivating the load list", $"Deactivating the following Load Lists as all their Packages were reassigned: {oldLoadListAfterImport.WDL_JobID}", Logger.Logs);
		}

		public void TestImportConsol_ForwarderRemovesLoadCompleteContainerAndPackages_DoesNotDetachPackages()
		{
			Data.SetupForForwardingImport();
			var consignmentDataObject = Data.CreateShipmentWithPackages("S00000001", "PKG-1", "PKG-2");

			// Create the corresponding transit jobs
			var receiveConsignment = Data.CreateReceiveConsignmentInDB(consignmentDataObject);

			// Read in the consol, but with 2 containers and shipments
			var consol = Data.HeaderDataObject;
			Logger.TopLevelDataObject = consol;
			consol.SetSubShipmentCollection(() => new DataObjectList<Shipment> { consignmentDataObject });
			consol.SetContainerCollection(() => new DataObjectList<Container>());
			var incompleteContainer = Data.CreateContainer("Cont1", 1);
			var containerToBeLoadCompleted = Data.CreateContainer("Cont2", 2);
			consol.ContainerCollection.Add(incompleteContainer);
			consol.ContainerCollection.Add(containerToBeLoadCompleted);
			var bookedPackline = consignmentDataObject.PackingLineCollection.Single(p => p.ReferenceNumber.ToString() == "PKG-1");
			var packlineToLoad = consignmentDataObject.PackingLineCollection.Single(p => p.ReferenceNumber.ToString() == "PKG-2");
			bookedPackline.ContainerLink = 1;
			packlineToLoad.ContainerLink = 2;

			var reader = new WhsTransitDispatchConsolDataObjectReader(consol, Logger, Factory);
			reader.ReadIntoBusinessObject();

			Factory.SaveForTesting();

			// Load one of the load list's package on a load complete container (to be excluded from the reimport)
			var loadListsAfterImport = Factory.Load<WhsItemDispatchLoadList>(new ZQuery());
			var loadListForIncompleteContainer = loadListsAfterImport.Single(l => l.PackageStates.Single().Package.KP_PackageID == "PKG-1");
			var loadListForLoadCompleteContainer = loadListsAfterImport.Single(l => l.PackageStates.Single().Package.KP_PackageID == "PKG-2");

			var incompleteDTU = Helper.CreateDispatchTransportationUnitWithContainerType("Cont1", Data.Warehouse.PK, containerID: "Cont1");
			Helper.CreateDispatchDLLDTUPivot(loadListForIncompleteContainer.PK, incompleteDTU.PK);

			var loadCompleteDTU = Helper.CreateDispatchTransportationUnitWithContainerType("Cont2", Data.Warehouse.PK, containerID: "Cont2");
			Helper.CreateDispatchDLLDTUPivot(loadListForLoadCompleteContainer.PK, loadCompleteDTU.PK);
			loadListForLoadCompleteContainer.WDL_WL_StagingLocation = Data.Warehouse.DefaultLocation.PK;
			loadListForLoadCompleteContainer.WDL_IsReadyToStage = true;

			var bookedPackage = receiveConsignment.PackageStates.Single(p => p.Package.KP_PackageID == "PKG-1");

			var location = Data.Warehouse.DefaultLocation;
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", Data.Warehouse.PK, location.PK);
			var loadedPackage = receiveConsignment.PackageStates.Single(p => p.Package.KP_PackageID == "PKG-2");
			loadedPackage.Reload();
			loadedPackage.WPS_WRH_TransitReceiveHeader = rtu.PK;
			loadedPackage.WPS_WL_LastLocation = location.PK;
			loadedPackage.WPS_WDL_LoadList = loadListForLoadCompleteContainer.PK;
			Helper.LoadPackage(loadCompleteDTU, loadedPackage);
			Helper.FinishLoading(loadCompleteDTU, gateIn: DateTime.UtcNow.AddDays(-1), loadComplete: DateTime.UtcNow, gateOut: ZDateTimeOffset.Empty);

			Factory.SaveForTesting();

			// Read in the consol, but with a single Container
			consol.ContainerCollection.Remove(containerToBeLoadCompleted);
			consignmentDataObject.PackingLineCollection.Remove(packlineToLoad);

			var reader2 = new WhsTransitDispatchConsolDataObjectReader(consol, Logger, Factory);
			var newLoadListAfterImport2 = reader2.ReadIntoBusinessObject().PopulatedDispatchLoadListForTesting;

			// Reload WhsItemPackageState into Factory
			Factory.Load<WhsItemPackageState>(new ZQuery());
			Factory.SaveForTesting();

			AssertEquals("Should have reused container 1's load list.", loadListForIncompleteContainer.PK, newLoadListAfterImport2.PK);
			AssertContainsExactElementsInAnyOrder("The included shipment's package should be assigned to the new load list.",
				new string[] { "PKG-1" }, newLoadListAfterImport2.PackageStates.Select(p => p.Package.KP_PackageID));

			var factoryAfterImport2 = new UniversalObjectFactory();
			var loadCompleteContainerloadListAfterImport2 = factoryAfterImport2.Load<WhsItemDispatchLoadList>(loadListForLoadCompleteContainer.PK);
			AssertContainsExactElementsInAnyOrder("The excluded shipment's package should not be detached as its DTU is load complete.",
				new string[] { "PKG-2" }, loadCompleteContainerloadListAfterImport2.PackageStates.Select(p => p.Package.KP_PackageID));
			AssertEquals("The unused load list should not be changed.", true, loadCompleteContainerloadListAfterImport2.WDL_IsActive);
			AssertEquals("The unused load list should not be completed.", false, loadCompleteContainerloadListAfterImport2.WDL_CompleteTime.IsValid);
			AssertEquals("The unused load list should have related DTUs.", 2, loadCompleteContainerloadListAfterImport2.DispatchTransportationUnits.Count);
			AssertNull(loadCompleteContainerloadListAfterImport2.Logs.Find(l => l.SL_SE_NKEvent == Events.SetToInactiveCode).FirstOrDefault());
		}

		public void TestImportConsol_ForwarderReimportsLoadCompleteConsol_DoesNotThrow()
		{
			Data.SetupForForwardingImport();
			var consignmentDataObject = Data.CreateShipmentWithPackages("S00000001", "PKG-1", "PKG-2");

			// Create the corresponding transit jobs
			var receiveConsignment = Data.CreateReceiveConsignmentInDB(consignmentDataObject);

			// Read in the consol
			var consol = Data.HeaderDataObject;
			Logger.TopLevelDataObject = consol;
			consol.SetSubShipmentCollection(() => new DataObjectList<Shipment> { consignmentDataObject });
			consol.SetContainerCollection(() => new DataObjectList<Container>());
			var containerToBeLoadCompleted = Data.CreateContainer("Cont1", 1);
			consol.ContainerCollection.Add(containerToBeLoadCompleted);
			var packline1 = consignmentDataObject.PackingLineCollection.Single(p => p.ReferenceNumber.ToString() == "PKG-1");
			packline1.ContainerLink = 1;

			// Remove the unused packline (add it back in for the second Dispatch Instruction)
			var packlineForReimport = consignmentDataObject.PackingLineCollection.Single(p => p.ReferenceNumber.ToString() == "PKG-2");
			consignmentDataObject.PackingLineCollection.Remove(packlineForReimport);

			var reader = new WhsTransitDispatchConsolDataObjectReader(consol, Logger, Factory);
			reader.ReadIntoBusinessObject();

			Factory.SaveForTesting();

			// Load the package on a completed DTU
			var loadListsAfterImport = Factory.Load<WhsItemDispatchLoadList>(new ZQuery());
			var loadList = loadListsAfterImport.Single();
			AssertEquals("Precondition", "PKG-1", loadList.PackageStates.Single().Package.KP_PackageID);
			var loadCompleteDTU = loadList.DispatchTransportationUnits.Single();

			var location = Data.Warehouse.DefaultLocation;
			loadList.WDL_WL_StagingLocation = location.PK;
			loadList.WDL_IsReadyToStage = true;
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", Data.Warehouse.PK, location.PK);
			var package = receiveConsignment.PackageStates.Single(p => p.Package.KP_PackageID == "PKG-1");
			package.Reload();
			package.WPS_WRH_TransitReceiveHeader = rtu.PK;
			package.WPS_WDL_LoadList = loadList.PK;
			package.WPS_WL_LastLocation = location.PK;
			Helper.LoadPackage(loadCompleteDTU, package);
			Helper.FinishLoading(loadCompleteDTU, gateIn: DateTime.UtcNow.AddDays(-1), loadComplete: DateTime.UtcNow, gateOut: ZDateTimeOffset.Empty);

			Factory.SaveForTesting();

			// Reimport the consol with an additional container and package
			var newContainerForReimport = Data.CreateContainer("Cont2", 2);
			consol.ContainerCollection.Add(newContainerForReimport);
			consignmentDataObject.PackingLineCollection.Add(packlineForReimport);
			packlineForReimport.ContainerLink = 2;

			var reader2 = new WhsTransitDispatchConsolDataObjectReader(consol, Logger, Factory);

			AssertNoExceptionThrown("Should allow updates when reimporting a load complete container.", () => reader2.ReadIntoBusinessObject());

			Factory.SaveForTesting();

			var factoryAfterImport2 = new UniversalObjectFactory();
			var loadListsAfterImport2 = factoryAfterImport2.Load<WhsItemDispatchLoadList>(new ZQuery());
			var loadListForloadCompletedContainerAfterImport2 = loadListsAfterImport2.SingleOrDefault(l => l.PK == loadList.PK);
			var loadListForNewContainerAfterImport2 = loadListsAfterImport2.SingleOrDefault(l => l.PK != loadList.PK);
			AssertNotNull("Should have reused Load Completed Container's load list.", loadListForloadCompletedContainerAfterImport2);
			AssertNotNull("Should have created the new Container's load list.", loadListForNewContainerAfterImport2);
			AssertEquals("Currently the load list is stopped by the consignment reader", false, loadListForloadCompletedContainerAfterImport2.WDL_IsReadyToStage); // We don't stop load lists if all their packages are departed, but we do if they're only load complete?
			AssertEquals("Should not have started the Load List.", false, loadListForNewContainerAfterImport2.WDL_IsReadyToStage);
			AssertEquals("Should have reused Load Completed Container's DTU.", 1, loadListForloadCompletedContainerAfterImport2.DispatchTransportationUnits.Count);
			AssertEquals("Should have reused new Container's DTU.", 1, loadListForNewContainerAfterImport2.DispatchTransportationUnits.Count);
			var loadCompletedDTUAfterImport2 = loadListForloadCompletedContainerAfterImport2.DispatchTransportationUnits.Single();
			AssertEquals("Should have reused the Load Completed Container's DTU.", loadCompleteDTU.PK, loadCompletedDTUAfterImport2?.PK);
			AssertEquals("Should not have stopped the Load List", loadCompleteDTU.PK, loadCompletedDTUAfterImport2?.PK);
		}

		public void TestImportConsol_ForwarderReimportsConsolWithoutDataSource_DoesNotThrow()
		{
			Data.SetupForForwardingImport();
			var consignmentDataObject = Data.CreateShipmentWithPackages("S00000001", "PKG-1", "PKG-2");

			// Create the corresponding transit jobs
			var receiveConsignment = Data.CreateReceiveConsignmentInDB(consignmentDataObject);

			// Read in the consol
			var consol = Data.HeaderDataObject;
			Logger.TopLevelDataObject = consol;
			consol.SetSubShipmentCollection(() => new DataObjectList<Shipment> { consignmentDataObject });
			consol.SetContainerCollection(() => new DataObjectList<Container>());
			var containerToBeLoadCompleted = Data.CreateContainer("Cont1", 1);
			consol.ContainerCollection.Add(containerToBeLoadCompleted);
			var packline1 = consignmentDataObject.PackingLineCollection.Single(p => p.ReferenceNumber.ToString() == "PKG-1");
			packline1.ContainerLink = 1;

			// Remove the unused packline (add it back in for the second Dispatch Instruction)
			var packlineForReimport = consignmentDataObject.PackingLineCollection.Single(p => p.ReferenceNumber.ToString() == "PKG-2");
			consignmentDataObject.PackingLineCollection.Remove(packlineForReimport);

			var reader = new WhsTransitDispatchConsolDataObjectReader(consol, Logger, Factory);
			reader.ReadIntoBusinessObject();

			Factory.SaveForTesting();

			// Start Load List
			var loadListsAfterImport = Factory.Load<WhsItemDispatchLoadList>(new ZQuery());
			var loadList = loadListsAfterImport.Single();
			AssertEquals("Precondition", "PKG-1", loadList.PackageStates.Single().Package.KP_PackageID);

			var location = Data.Warehouse.DefaultLocation;
			loadList.WDL_WL_StagingLocation = location.PK;
			loadList.WDL_IsReadyToStage = true;

			Factory.SaveForTesting();

			var packageStates = Factory.Load<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_WRC_TransitReceiveConsignment, receiveConsignment.PK));
			AssertEquals("Should create 2 package", 2, packageStates.Length);

			var packageState1 = packageStates.FirstOrDefault(p => p.Package.KP_PackageID == "PKG-1");
			packageState1.Reload();
			var packageState2 = packageStates.FirstOrDefault(p => p.Package.KP_PackageID == "PKG-2");
			packageState2.Reload();

			// Remove DataSource and add DataTarget then reimport

			Data.SetupNewDataContextWithDispatchDataTarget(consol, true);
			Data.SetupNewDataContextWithDispatchDataTarget(consignmentDataObject, true);

			var reader2 = new WhsTransitDispatchConsolDataObjectReader(consol, Logger, Factory);
			AssertNoExceptionThrown("Should not throw any exceptions.", () => reader2.ReadIntoBusinessObject());

			// Reload WhsItemPackageState into Factory
			Factory.Load<WhsItemPackageState>(new ZQuery());
			Factory.SaveForTesting();

			var factoryAfterImport2 = new UniversalObjectFactory();
			var loadListAfterImport2 = factoryAfterImport2.Load<WhsItemDispatchLoadList>(loadList.PK);
			AssertNotNull("Should have reused the same load list.", loadListAfterImport2);
			AssertEquals("Currently the load list is stopped by the consignment reader", false, loadListAfterImport2.WDL_IsReadyToStage);
		}

		public void TestImportConsol_DepartedPackageOnUnmatchedLoadCompleteDTU_Throws() => TestImportConsol_PackageOnUnmatchedDTU_Core(isLoadComplete: true, isDeparted: true);

		public void TestImportConsol_PackageOnUnmatchedLoadCompleteDTU_Throws() => TestImportConsol_PackageOnUnmatchedDTU_Core(isLoadComplete: true, isDeparted: false);

		public void TestImportConsol_PackageOnUnmatchedIncompleteDTU_SetsRemoveFromDTU() => TestImportConsol_PackageOnUnmatchedDTU_Core(isLoadComplete: false, isDeparted: false);

		void TestImportConsol_PackageOnUnmatchedDTU_Core(bool isLoadComplete, bool isDeparted)
		{
			Data.SetupForForwardingImport();
			var consignmentDataObject = Data.CreateShipmentWithPackages("S00000001", "PKG-Loaded");

			// Create the corresponding transit jobs
			var receiveConsignment = Data.CreateReceiveConsignmentInDB(consignmentDataObject);

			Factory.SaveForTesting();

			// Import the consol
			var consol = Data.HeaderDataObject;
			Logger.TopLevelDataObject = consol;
			consol.SetSubShipmentCollection(() => new DataObjectList<Shipment> { consignmentDataObject });

			var reader = new WhsTransitDispatchConsolDataObjectReader(consol, Logger, Factory);
			reader.ReadIntoBusinessObject();

			Factory.SaveForTesting();

			var newFactoryAfterImport = new UniversalObjectFactory();
			var loadListAfterImport = newFactoryAfterImport.Load<WhsItemDispatchLoadList>(new ZQuery()).Single();

			// Change the Package's load list and reimport
			var unmatchedLoadList = Helper.CreateDispatchLoadList("DLL00000010", Data.Warehouse.PK);
			var dtu = Helper.CreateDispatchTransportationUnitWithContainerType("DTU0000010", Data.Warehouse.PK, containerID: "Cont1");
			Helper.CreateDispatchDLLDTUPivot(unmatchedLoadList.PK, dtu.PK);
			unmatchedLoadList.WDL_WL_StagingLocation = Data.Warehouse.DefaultLocation.PK;
			unmatchedLoadList.WDL_IsReadyToStage = true;

			var package = receiveConsignment.PackageStates.Single(p => p.Package.KP_PackageID == "PKG-Loaded");
			package.Reload();
			package.WPS_WDL_LoadList = unmatchedLoadList.PK;
			var rtu = Helper.CreateReceiveTransportationUnit("RT0000001", Data.Warehouse.PK, Data.Warehouse.DefaultLocation.PK);
			package.WPS_WL_LastLocation = rtu.WRH_WL_StagingLocation;
			package.WPS_WRH_TransitReceiveHeader = rtu.PK;
			Helper.LoadPackage(dtu, package);
			package.WPS_Status = TransitWarehouseStatuses.Codes.FreightLoaded;

			if (isDeparted)
			{
				Helper.FinishLoading(dtu, gateIn: DateTime.UtcNow.AddDays(-1), loadComplete: DateTime.UtcNow, gateOut: DateTime.UtcNow);
			}
			else if (isLoadComplete)
			{
				Helper.FinishLoading(dtu, gateIn: DateTime.UtcNow.AddDays(-1), loadComplete: DateTime.UtcNow, gateOut: ZDateTimeOffset.Empty);
			}

			Factory.SaveForTesting();

			var reader2 = new WhsTransitDispatchConsolDataObjectReader(consol, Logger, Factory);

			if (isDeparted)
			{
				AssertExceptionThrown<DataObjectReadFailureException>("Should check if Package DTUs are Departed before detaching.",
					"At least one package is departed already. Therefore cannot be removed from load list.", () => reader2.ReadIntoBusinessObject());
			}
			else if (isLoadComplete)
			{
				AssertExceptionThrown<DataObjectReadFailureException>("Should check if Package DTUs are Load Complete before detaching.",
@"Attempting to reassign Packages from Load Lists for Dispatch Transportation Units that have already been closed.
These Packages can only be removed from their current Load List via Transit Warehouse Desktop:
Package        Load List        DTU
PKG-Loaded     (DLL00000010)    Cont1 (DTU0000010)", () => reader2.ReadIntoBusinessObject());
			}
			else
			{
				AssertNoExceptionThrown("Should not throw if a reassigned Freight Loaded Package's DTU is incomplete.", () => reader2.ReadIntoBusinessObject());

				Factory.SaveForTesting();

				var newFactoryAfterImport2 = new UniversalObjectFactory();
				var packageAfterImport2 = newFactoryAfterImport2.Load<WhsItemPackageState>(package.PK);
				AssertEquals("The Package should be assigned to the Consol's load list", loadListAfterImport.PK, packageAfterImport2.WPS_WDL_LoadList);
				AssertEquals("The Package should be marked as Remove From DTU", true, packageAfterImport2.WPS_RemoveFromDTU);

				var unmatchedLoadListAfterImport = newFactoryAfterImport2.Load<WhsItemDispatchLoadList>(unmatchedLoadList.PK);
				AssertEquals("The unmatched Load List should be stopped", false, unmatchedLoadListAfterImport.WDL_IsReadyToStage);
				AssertContains("Should log removing the packages from the unmatched Load List",
					$"Packages were found on Load List {unmatchedLoadListAfterImport.WDL_JobID} but were either assigned to another Load List or were not included in the UXML.", Logger.Logs);
			}
		}

		#endregion

		#region TestImportConsol_ForwarderRemovesContainersAndShipments

		public void TestImportConsol_ForwarderRemovesContainers_DeactivatesUnusedLoadListsAndRemoveDTU()
		{
			Data.SetupForForwardingImport();
			var consignmentDataObject1 = Data.CreateShipmentWithPackages("S00000001", "PKG-1");
			var consignmentDataObject2 = Data.CreateShipmentWithPackages("S00000002", "PKG-2");

			// Create the corresponding transit jobs
			var receiveConsignment1 = Data.CreateReceiveConsignmentInDB(consignmentDataObject1);
			var receiveConsignment2 = Data.CreateReceiveConsignmentInDB(consignmentDataObject2);

			// Read in the consol
			var consol = Data.HeaderDataObject;
			Logger.TopLevelDataObject = consol;
			consol.SetSubShipmentCollection(() => new DataObjectList<Shipment> { consignmentDataObject1, consignmentDataObject2 });
			consol.SetContainerCollection(() => new DataObjectList<Container>());
			var container1 = Data.CreateContainer("Cont1", 1);
			var container2 = Data.CreateContainer("Cont2", 2);
			consol.ContainerCollection.Add(container1);
			consol.ContainerCollection.Add(container2);
			var packline1 = consignmentDataObject1.PackingLineCollection.Single();
			packline1.ContainerLink = 1;
			var packline2 = consignmentDataObject2.PackingLineCollection.Single();
			packline2.ContainerLink = 2;

			var reader = new WhsTransitDispatchConsolDataObjectReader(consol, Logger, Factory);
			reader.ReadIntoBusinessObject();

			Factory.SaveForTesting();

			var factoryAfterImport1 = new UniversalObjectFactory();
			var dtusAfterImport1 = factoryAfterImport1.Load<WhsItemDispatchTransportationUnit>(new ZQuery());
			AssertContainsExactElementsInAnyOrder(new string[] { "Cont1", "Cont2" }, dtusAfterImport1.Select(d => d.WDH_VehicleReference));
			AssertEquals(2, factoryAfterImport1.Load<WhsItemDispatchLoadList>(new ZQuery(WhsItemDispatchLoadListSchema.WDL_IsActive, true)).Length);
			AssertEquals(2, factoryAfterImport1.Load<WhsItemDispatchLoadListDTUPivot>(new ZQuery()).Length);
			AssertEquals(2, factoryAfterImport1.Load<PkgPackageExtension>(new ZQuery()).Length);
			AssertEquals(2, factoryAfterImport1.Load<PkgPackageContainer>(new ZQuery()).Length);
			var pkgPackageHeaders = factoryAfterImport1.Load<PkgPackageHeader>(new ZQuery(PkgPackageHeaderSchema.KPH_PackageID, new string[] { "Cont1", "Cont2" }));
			AssertEquals(2, pkgPackageHeaders.Length);
			var pkgPackages = factoryAfterImport1.Load<PkgPackage>(new ZQuery(PkgPackageSchema.KP_KPH_PackageHeader, pkgPackageHeaders.Select(t => t.PK)));
			AssertEquals(2, pkgPackages.Length);
			var pkgPackageStates = factoryAfterImport1.Load<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_KP_Package, pkgPackages.Select(t => t.PK)));
			AssertEquals(2, pkgPackageStates.Length);

			consol.ContainerCollection.Remove(container2);
			consol.SubShipmentCollection.Remove(consignmentDataObject2);

			Factory.SaveForTesting();

			var rcn1PackageStates = Factory.LoadTop1<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_WRC_TransitReceiveConsignment, receiveConsignment1.PK));
			var rcn2PackageStates = Factory.LoadTop1<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_WRC_TransitReceiveConsignment, receiveConsignment2.PK));

			rcn1PackageStates.Reload();
			rcn2PackageStates.Reload();

			var reader2 = new WhsTransitDispatchConsolDataObjectReader(consol, Logger, Factory);

			AssertNoExceptionThrown("Should not throw any exceptions.", () => reader2.ReadIntoBusinessObject());

			Factory.SaveForTesting();

			var factoryAfterImport2 = new UniversalObjectFactory();
			var dtusAfterImport2 = factoryAfterImport2.Load<WhsItemDispatchTransportationUnit>(new ZQuery());
			AssertEquals("Cont1", dtusAfterImport2.Single().WDH_VehicleReference);
			AssertEquals(1, factoryAfterImport2.Load<WhsItemDispatchLoadList>(new ZQuery(WhsItemDispatchLoadListSchema.WDL_IsActive, true)).Length);
			AssertEquals(1, factoryAfterImport2.Load<WhsItemDispatchLoadListDTUPivot>(new ZQuery()).Length);
			AssertEquals(1, factoryAfterImport2.Load<PkgPackageExtension>(new ZQuery()).Length);
			AssertEquals(1, factoryAfterImport2.Load<PkgPackageContainer>(new ZQuery()).Length);
			pkgPackageHeaders = factoryAfterImport2.Load<PkgPackageHeader>(new ZQuery(PkgPackageHeaderSchema.KPH_PackageID, new string[] { "Cont1", "Cont2" }));
			AssertEquals(1, pkgPackageHeaders.Length);
			pkgPackages = factoryAfterImport2.Load<PkgPackage>(new ZQuery(PkgPackageSchema.KP_KPH_PackageHeader, pkgPackageHeaders.Select(t => t.PK)));
			AssertEquals(1, pkgPackages.Length);
			pkgPackageStates = factoryAfterImport2.Load<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_KP_Package, pkgPackages.Select(t => t.PK)));
			AssertEquals(1, pkgPackageStates.Length);
			AssertContains($@"Information - The following Load List(s) deactivated from Consol C1000000:
Load List
WaybillParent (DLL00000002)
Information - The following Dispatch Transportation Unit(s) removed from Consol C1000000:
DTU
Cont2 (TD00000002)", Logger.Logs);
		}

		#endregion

		#region TestImportConsol_UnmatchedLoadListBecomesEmpty_DeactivatesLoadList

		public void TestImportConsol_UnmatchedLoadListBecomesEmpty_DeactivatesLoadList()
		{
			Data.SetupForForwardingImport();
			// Create transit jobs for an imported consol with a shipment on an unmatched load list, then import the consol
			const string shipmentNumber1 = "S00000001";
			var consignmentDataObject1 = Data.CreateShipmentWithPackages(shipmentNumber1, "PKG-1");
			var consol = new Shipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			consol.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.DTW, ServiceCode = ServiceCodeType.TWD } } });
			consol.DataContext.AddDataSource(DataContextType.ForwardingConsol, "CS00000001");
			consol.DataContext.AddDataSource(DataContextType.ForwardingShipment, shipmentNumber1);
			consol.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { Data.Orgs.Consignor_CRAHOLSYD, Data.Orgs.Consignee_INTHEMSYD, Data.Orgs.Warehouse_WUFSHIJNB, Data.Orgs.Warehouse_INTHEMSYD });

			// Create the corresponding transit jobs
			var receiveConsignment1 = Data.CreateReceiveConsignmentInDB(consignmentDataObject1);

			// Create a load list that won't match the consol
			var loadList = Factory.NewWithValidTestData<WhsItemDispatchLoadList>();
			loadList.WDL_JobID = "DLL00000010";
			loadList.WDL_WW_Warehouse = Data.Warehouse.PK;

			var unchangedEmptyLoadList = Factory.NewWithValidTestData<WhsItemDispatchLoadList>();
			unchangedEmptyLoadList.WDL_JobID = "DLL00000020";
			unchangedEmptyLoadList.WDL_WW_Warehouse = Data.Warehouse.PK;

			receiveConsignment1.PackageStates.Single().WPS_WDL_LoadList = loadList.PK;

			Factory.SaveForTesting();

			// Read in the consol
			Logger.TopLevelDataObject = consol;
			consol.SetSubShipmentCollection(() => new DataObjectList<Shipment> { consignmentDataObject1 });
			var newConsolReader = new WhsTransitDispatchConsolDataObjectReader(consol, Logger, Factory);
			var newLoadListAfterImport = newConsolReader.ReadIntoBusinessObject().PopulatedDispatchLoadListForTesting;

			Factory.SaveForTesting();

			var factoryAfterImport = new UniversalObjectFactory();
			AssertNotEquals(loadList.PK, newLoadListAfterImport.PK);
			var loadListAfterImport = factoryAfterImport.Load<WhsItemDispatchLoadList>(loadList.PK);
			AssertContainsExactElementsInAnyOrder("The package should be assigned to the new load list.",
				new string[] { "PKG-1" }, newLoadListAfterImport.PackageStates.Select(p => p.Package.KP_PackageID));
			AssertContainsExactElementsInAnyOrder("The package should be detached from the old load list.",
				Array.Empty<string>(), loadListAfterImport.PackageStates.Select(p => p.Package.KP_PackageID));
			AssertEquals("The replaced load list should be deactivated.", false, loadListAfterImport.WDL_IsActive);
			AssertEquals(1, loadListAfterImport.Logs.Find(l => l.SL_SE_NKEvent == Events.SetToInactiveCode).Count());
			AssertEquals("The replaced load list should not be completed.", false, loadListAfterImport.WDL_CompleteTime.IsValid);
			AssertContains("It should add a log as the load list was deactivated",
				$"Deactivating the following Load Lists as all their Packages were reassigned: {loadList.WDL_JobID}", Logger.Logs);

			var unchangedLoadListAfterImport = factoryAfterImport.Load<WhsItemDispatchLoadList>(unchangedEmptyLoadList.PK);
			AssertEquals("The unchanged load list should not be changed.", true, unchangedEmptyLoadList.WDL_IsActive);
			AssertEquals("The unchanged load list should not be completed.", false, unchangedEmptyLoadList.WDL_CompleteTime.IsValid);
		}

		#endregion

		#region TestImportConsol_UnmatchedLoadListIsNotEmpty_IgnoresLoadList

		public void TestImportConsol_UnmatchedLoadListIsNotEmpty_IgnoresLoadList()
		{
			Data.SetupForForwardingImport();
			// Create transit jobs for an imported consol with two shipments, then import the consol with one of the shipments removed
			const string shipmentNumber1 = "S00000001";
			const string shipmentNumber2 = "S00000002";
			var consignmentDataObject1 = Data.CreateShipmentWithPackages(shipmentNumber1, "PKG-1");
			var consignmentDataObject2 = Data.CreateShipmentWithPackages(shipmentNumber2, "PKG-2");
			var consol = new Shipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			consol.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.DTW, ServiceCode = ServiceCodeType.TWD } } });
			consol.DataContext.AddDataSource(DataContextType.ForwardingConsol, "CS00000001");
			consol.DataContext.AddDataSource(DataContextType.ForwardingShipment, shipmentNumber1);
			consol.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { Data.Orgs.Consignor_CRAHOLSYD, Data.Orgs.Consignee_INTHEMSYD, Data.Orgs.Warehouse_WUFSHIJNB, Data.Orgs.Warehouse_INTHEMSYD });

			// Create the corresponding transit jobs
			var receiveConsignment1 = Data.CreateReceiveConsignmentInDB(consignmentDataObject1);
			var receiveConsignment2 = Data.CreateReceiveConsignmentInDB(consignmentDataObject2);

			// Create a load list that won't match the consol with packages from a shipment that is not on the consol
			var loadList = Factory.NewWithValidTestData<WhsItemDispatchLoadList>();
			loadList.WDL_JobID = "DLL00000010";
			loadList.WDL_WW_Warehouse = Data.Warehouse.PK;

			receiveConsignment1.PackageStates.Single().WPS_WDL_LoadList = loadList.PK;
			receiveConsignment2.PackageStates.Single().WPS_WDL_LoadList = loadList.PK;

			Factory.SaveForTesting();

			// Read in the consol with only one shipment from the existing load list
			Logger.TopLevelDataObject = consol;
			consol.SetSubShipmentCollection(() => new DataObjectList<Shipment> { consignmentDataObject1 });
			var newConsolReader = new WhsTransitDispatchConsolDataObjectReader(consol, Logger, Factory);
			var newLoadListAfterImport = newConsolReader.ReadIntoBusinessObject().PopulatedDispatchLoadListForTesting;

			Factory.SaveForTesting();

			var factoryAfterImport = new UniversalObjectFactory();
			AssertNotEquals(loadList.PK, newLoadListAfterImport.PK);
			var loadListAfterImport = factoryAfterImport.Load<WhsItemDispatchLoadList>(loadList.PK);
			AssertContainsExactElementsInAnyOrder("The included Shipment's package should be assigned to the new load list.",
				new string[] { "PKG-1" }, newLoadListAfterImport.PackageStates.Select(p => p.Package.KP_PackageID));
			AssertContainsExactElementsInAnyOrder("The excluded Shipment's package should remain on the old load list.",
				new string[] { "PKG-2" }, loadListAfterImport.PackageStates.Select(p => p.Package.KP_PackageID));
			AssertEquals("The replaced load list should not be changed.", true, loadListAfterImport.WDL_IsActive);
			AssertEquals("The replaced load list should be open.", false, loadListAfterImport.WDL_CompleteTime.IsValid);
			AssertNotContains("It should not add a log as the load list wasn't deactivated",
				$"Deactivating the following Load Lists as all their Packages were reassigned: {loadList.WDL_JobID}", Logger.Logs);
		}

		#endregion

		#region TestImportingConsol_SameUXMLComingFromRunSheetCanBeImportedAgain

		public void TestImportingConsol_SameUXMLComingFromRunSheetCanBeImportedAgain()
		{
			Data.SetupForForwardingImport();
			var consol = new Shipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			consol.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.ATW, ServiceCode = ServiceCodeType.TWD } } });
			consol.ContainerMode = new ContainerMode { Code = "FCL" };
			consol.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { Data.Orgs.Consignor_CRAHOLSYD, Data.Orgs.Consignee_INTHEMSYD, Data.Orgs.Warehouse_WUFSHIJNB, Data.Orgs.Warehouse_INTHEMSYD });

			consol.SetContainerCollection(() => new DataObjectList<Container>());
			var container = Data.CreateContainer("A", 1);
			consol.ContainerCollection.Add(container);

			// shipment
			consol.SetSubShipmentCollection(() => new DataObjectList<Shipment>());
			var shipment1 = Data.CreateShipmentWithPackagesAndContainerLinks("", Tuple.Create((int?)1, "PKG1"), Tuple.Create((int?)1, "PKG2"));
			consol.DataContext.AddDataSource(DataContextType.LandTransportConsignment, "C123");
			consol.SubShipmentCollection.Add(shipment1);

			Logger.TopLevelDataObject = consol;

			// create receive consignments and packages
			var receiveConsol = Data.CreateReceiveConsolAndConsignmentsInDB(consol);
			AssertEquals("Precondition - One consignment is created.", 1, receiveConsol.PopulatedConsignmentsForTesting.Length);

			var consolDataObjectReader = new WhsTransitDispatchConsolDataObjectReader(consol, Logger, Factory);
			var consolidation = consolDataObjectReader.ReadIntoBusinessObject();

			var dispatchConsignment = consolidation.PopulatedConsignmentsForTesting.Single();
			AssertEquals("Precondition: There must be two package States.", 2, dispatchConsignment.PackageStates.Count);
			AssertNoExceptionThrown("Should not throw errors during the saving.", () => Factory.SaveForTesting());

			// Read UXML file again
			var newFactory = new UniversalObjectFactory();
			var newConsolReader = new WhsTransitDispatchConsolDataObjectReader(consol, Logger, newFactory);
			var newConsolidation = newConsolReader.ReadIntoBusinessObject();
			AssertNotEquals("New load list must be created.", consolidation.PopulatedDispatchLoadListForTesting.PK, newConsolidation.PopulatedDispatchLoadListForTesting.PK);
			AssertEquals("Dispatch header must be same.", consolidation.PopulatedDispatchTransportationUnitsForTesting.Single().PK, newConsolidation.PopulatedDispatchTransportationUnitsForTesting.Single().PK);
			AssertEquals("Consignment must be same.", consolidation.PopulatedConsignmentsForTesting.Single().PK, newConsolidation.PopulatedConsignmentsForTesting.Single().PK);

			var newPackageStates = newFactory.RowFactory.Load(WhsItemPackageStateSchema.Constants.TableName,
				new ZQuery(WhsItemPackageStateSchema.WPS_WDC_TransitDispatchConsignment, newConsolidation.PopulatedConsignmentsForTesting.Select(c => c.PK)));
			foreach (var newPackageState in newPackageStates)
			{
				AssertEquals("All package states must have the same load list.", newConsolidation.PopulatedDispatchLoadListForTesting.PK,
					DataObjectReader.GetColumnIndexerFromRow(newPackageState).GetValue(WhsItemPackageStateSchema.WPS_WDL_LoadList));
			}

			AssertNoExceptionThrown("Should not throw errors during the second saving.", () => newFactory.SaveForTesting());
		}

		#endregion

		#region TestImportingConsol_NoMatchingWarehouse

		public void TestImportingConsol_NoMatchingWarehouse()
		{
			Data.SetupForForwardingImport();
			var consol = new Shipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			consol.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.DTW, ServiceCode = ServiceCodeType.TWD } } });
			consol.DataContext.AddDataSource(DataContextType.ForwardingConsol, "C123");
			consol.ContainerMode = new ContainerMode { Code = "FCL" };
			consol.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { Data.Orgs.Warehouse_INTHEMSYD });

			consol.SetContainerCollection(() => new DataObjectList<Container>());
			var container = Data.CreateContainer("A", 1);
			consol.ContainerCollection.Add(container);

			// shipment1
			consol.SetSubShipmentCollection(() => new DataObjectList<Shipment>());
			var shipment1 = Data.CreateShipmentWithPackagesAndContainerLinks("WAYBILLNO1", Tuple.Create((int?)1, "PKG1"), Tuple.Create((int?)1, "PKG2"));
			consol.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S123");
			consol.SubShipmentCollection.Add(shipment1);

			Logger.TopLevelDataObject = consol;

			var consolDataObjectReader = new WhsTransitDispatchConsolDataObjectReader(consol, Logger, Factory);
			AssertExceptionThrown<DataObjectReadFailureException>("Cannot import without a valid Warehouse supplied. ", () => consolDataObjectReader.ReadIntoBusinessObject());
		}

		#endregion

		#region TestImportingConsol_HasHandlingUnit

		public void TestImportingConsol_HasHandlingUnit()
		{
			var warehouse = Data.Warehouse;
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;

			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);

			var handlingUnit = Helper.CreatePackageHandlingUnit();
			var handlingUnitPackage = Helper.CreateHandlingUnitPackage("HU1", handlingUnit, rtu);
			var package1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var package2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);

			Helper.PackPackageIntoHandlingUnit(handlingUnitPackage, package1, ZDateTimeOffset.Now, "AAA", handlingUnitPackage);
			Helper.PackPackageIntoHandlingUnit(handlingUnitPackage, package2, ZDateTimeOffset.Now, "AAA", handlingUnitPackage);
			Factory.SaveForTesting();

			Data.SetupForForwardingImport();
			var consol = new Shipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			consol.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.DTW, ServiceCode = ServiceCodeType.TWD } } });
			consol.DataContext.AddDataSource(DataContextType.ForwardingConsol, "C123");
			consol.ContainerMode = new ContainerMode { Code = "FCL" };
			consol.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { Data.Orgs.Consignor_CRAHOLSYD, Data.Orgs.Consignee_INTHEMSYD, Data.Orgs.Warehouse_WUFSHIJNB, Data.Orgs.Warehouse_INTHEMSYD });

			consol.SetContainerCollection(() => new DataObjectList<Container>());
			var container = Data.CreateContainer("A", 1);
			consol.ContainerCollection.Add(container);

			// shipment
			consol.SetSubShipmentCollection(() => new DataObjectList<Shipment>());
			var shipment1 = Data.CreateShipmentWithPackagesAndContainerLinks("WAYBILLNO1", Tuple.Create((int?)1, "HU1"));
			consol.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S123");
			consol.SubShipmentCollection.Add(shipment1);

			Logger.TopLevelDataObject = consol;

			var consolDataObjectReader = new WhsTransitDispatchConsolDataObjectReader(consol, Logger, Factory);

			AssertNoExceptionThrown(() =>
			{
				var consolidation = consolDataObjectReader.ReadIntoBusinessObject();

				var dispatchConsignment = consolidation.PopulatedConsignmentsForTesting.Single();
				AssertEquals("Precondition: There must be two package States.", 2, dispatchConsignment.PackageStates.Count);
				AssertNoExceptionThrown("Should not throw errors during the saving.", () => Factory.SaveForTesting());
			});
		}

		#endregion

		#region TestImportingConsol_DuplicatedReferenceNumbers

		public void TestImportingConsol_DuplicatedReferenceNumbers()
		{
			var warehouse = Data.Warehouse;
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;

			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);

			var package1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var package2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			Factory.SaveForTesting();

			Data.SetupForForwardingImport();
			var consol = new Shipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			consol.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.DTW, ServiceCode = ServiceCodeType.TWD } } });
			consol.DataContext.AddDataSource(DataContextType.ForwardingConsol, "C123");
			consol.ContainerMode = new ContainerMode { Code = "FCL" };
			consol.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { Data.Orgs.Consignor_CRAHOLSYD, Data.Orgs.Consignee_INTHEMSYD, Data.Orgs.Warehouse_WUFSHIJNB, Data.Orgs.Warehouse_INTHEMSYD });

			consol.SetContainerCollection(() => new DataObjectList<Container>());
			var container = Data.CreateContainer("A", 1);
			consol.ContainerCollection.Add(container);

			// shipment
			consol.SetSubShipmentCollection(() => new DataObjectList<Shipment>());
			var shipment1 = Data.CreateShipmentWithPackagesAndContainerLinks("WAYBILLNO1", Tuple.Create((int?)1, "PKG1"), Tuple.Create((int?)1, "PKG1"));
			consol.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S123");
			consol.SubShipmentCollection.Add(shipment1);

			Logger.TopLevelDataObject = consol;

			var consolDataObjectReader = new WhsTransitDispatchConsolDataObjectReader(consol, Logger, Factory);

			AssertNoExceptionThrown(() =>
			{
				var consolidation = consolDataObjectReader.ReadIntoBusinessObject();

				var dispatchConsignment = consolidation.PopulatedConsignmentsForTesting.Single();
				AssertEquals("Precondition: There must be two package States.", 1, dispatchConsignment.PackageStates.Count);
				AssertEquals("Precondition: Package1 should be attached.", true, dispatchConsignment.PackageStates.Any(p => p.PK == package1.PK));
				AssertNoExceptionThrown("Should not throw errors during the saving.", () => Factory.SaveForTesting());
			});
		}

		#endregion

		#region TestImportingConsol_AllPackagesAreDeparted

		public void TestImportingConsol_AllPackagesAreDeparted()
		{
			var warehouse = Data.WarehouseINTHEMSYD;
			Data.SetupForForwardingImport();
			var consol = new Shipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			consol.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.ATW, ServiceCode = ServiceCodeType.TWD } } });
			consol.DataContext.AddDataSource(DataContextType.ForwardingConsol, "C123");
			consol.ContainerMode = new ContainerMode { Code = "FCL" };
			consol.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { Data.Orgs.Consignor_CRAHOLSYD, Data.Orgs.Consignee_INTHEMSYD, Data.Orgs.Warehouse_WUFSHIJNB, Data.Orgs.Warehouse_INTHEMSYD });

			consol.SetContainerCollection(() => new DataObjectList<Container>());
			var container = Data.CreateContainer("A", 1);
			consol.ContainerCollection.Add(container);

			// shipment1
			consol.SetSubShipmentCollection(() => new DataObjectList<Shipment>());
			var shipment1 = Data.CreateShipmentWithPackagesAndContainerLinks("WAYBILLNO1", Tuple.Create((int?)1, "PKG1"), Tuple.Create((int?)1, "PKG2"));
			consol.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S123");
			consol.SubShipmentCollection.Add(shipment1);

			Logger.TopLevelDataObject = consol;

			// create receive consignments and packages
			var receiveConsol = Data.CreateReceiveConsolAndConsignmentsInDB(consol);
			AssertEquals("Precondition - One consignment is created.", 1, receiveConsol.PopulatedConsignmentsForTesting.Length);
			Factory.SaveForTesting();

			var consignmentReader = new WhsTransitDispatchConsignmentDataObjectReader(shipment1, Logger, Factory);
			var dcn = consignmentReader.ReadIntoBusinessObject();
			Factory.SaveForTesting();

			var stageLocation = warehouse.DefaultInboundDockDoorLocation;
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			dtu.WDH_VehicleReference = "A";
			var containerPackage = dtu.PackageJob.Packages.AddNew("CNT", "A");
			containerPackage.Container.K0_RC_ContainerType = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;

			var dll = Helper.CreateDispatchLoadList("C123", warehouse.PK);
			var ent = Factory.New<CusEntryNumber>();
			ent.CE_EntryType = WarehouseAdditionalReferenceTypes.Codes.ForwardingConsolNumber;
			ent.CE_EntryNum = "C123";
			ent.CE_ParentID = dll.PK;
			ent.CE_ParentTable = WhsItemDispatchLoadListSchema.Constants.TableName;
			ent.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;

			Helper.CreateDispatchDLLDTUPivot(dll.PK, dtu.PK);

			var query = new ZQuery(WhsItemPackageStateSchema.WPS_WRC_TransitReceiveConsignment, receiveConsol.PopulatedConsignmentsForTesting[0].PK);
			var packages = Factory.Load<WhsItemPackageState>(query);
			packages.ForEach(p => p.Reload());
			AssertEquals("Precondition:", 2, packages.Length);

			SetupPackageState(packages[0], stageLocation, rtu, dcn, dtu, dll, TransitWarehouseStatuses.Codes.Departed);
			SetupPackageState(packages[1], stageLocation, rtu, dcn, dtu, dll, TransitWarehouseStatuses.Codes.Finalized);
			Factory.SaveForTesting();

			var consolDataObjectReader = new WhsTransitDispatchConsolDataObjectReader(consol, Logger, Factory);
			var consolidation = consolDataObjectReader.ReadIntoBusinessObject();
			AssertContains("Attempting to create Container Loading Plans as Containers have been specified.", Logger.Logs);
			AssertContains("Creating 1 Load List(s) for Containers with a Loading Plan.", Logger.Logs);
			Factory.SaveForTesting();

			var dispatchConsignment = consolidation.PopulatedConsignmentsForTesting.Single();
			var packageStates = dispatchConsignment.PackageStates;
			AssertEquals("Two package states must be created.", 2, packageStates.Count);
			foreach (var packageState in packageStates)
			{
				AssertEquals("New Dispatch header for container should not be populated if all packages are departed.", dtu.PK, packageState.WPS_WDH_TransitDispatchHeader);
				AssertEquals("Load List must be populated.", consolidation.PopulatedDispatchLoadListForTesting.PK, packageState.WPS_WDL_LoadList);
				AssertEquals("Package job must be receive consignment.", packageState.ReceiveConsignment, packageState.Package.PackageJob.ParentJob);
			}
			AssertNoExceptionThrown("Should not throw errors during the saving.", () => Factory.SaveForTesting());
		}

		void SetupPackageState(WhsItemPackageState packageState, WhsLocation location, WhsItemReceiveTransportationUnit rtu, WhsItemDispatchConsignment dcn, WhsItemDispatchTransportationUnit dtu, WhsItemDispatchLoadList dll, string status)
		{
			packageState.WPS_WL_LastLocation = location.PK;
			packageState.WPS_Status = status;
			packageState.WPS_WRH_TransitReceiveHeader = rtu.PK;
			packageState.WPS_WDC_TransitDispatchConsignment = dcn.PK;
			packageState.WPS_WDH_TransitDispatchHeader = dtu?.PK ?? ZGuid.Empty;
			packageState.WPS_WDL_LoadList = dll.PK;
			packageState.WPS_IsSecure = true;
			packageState.WPS_SecurityStatus = "SEC";
		}

		#endregion

		#region TestImportingConsol_NoSubShipments

		public void TestImportingConsol_NoSubShipments()
		{
			Data.SetupForForwardingImport();
			var consol = new Shipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			consol.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.ATW, ServiceCode = ServiceCodeType.TWD } } });
			consol.DataContext.AddDataSource(DataContextType.ForwardingConsol, "C123");
			consol.ContainerMode = new ContainerMode { Code = "FCL" };
			consol.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { Data.Orgs.Consignor_CRAHOLSYD, Data.Orgs.Consignee_INTHEMSYD, Data.Orgs.Warehouse_WUFSHIJNB, Data.Orgs.Warehouse_INTHEMSYD });
			consol.SetSubShipmentCollection(() => new DataObjectList<Shipment>());

			var consolDataObjectReader = new WhsTransitDispatchConsolDataObjectReader(consol, Logger, Factory);

			AssertExceptionThrown(typeof(DataObjectReadFailureException), @"No Dispatch Consignments (e.g. Forwarding Shipments) were included in the Dispatch Instruction.",
				() => consolDataObjectReader.ReadIntoBusinessObject());
		}

		public void TestImportingConsol_SubShipmentCollectionIsNull()
		{
			Data.SetupForForwardingImport();
			var consol = new Shipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			consol.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.ATW, ServiceCode = ServiceCodeType.TWD } } });
			consol.DataContext.AddDataSource(DataContextType.ForwardingConsol, "C123");
			consol.ContainerMode = new ContainerMode { Code = "FCL" };
			consol.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { Data.Orgs.Consignor_CRAHOLSYD, Data.Orgs.Consignee_INTHEMSYD, Data.Orgs.Warehouse_WUFSHIJNB, Data.Orgs.Warehouse_INTHEMSYD });
			consol.SetSubShipmentCollection(() => null);

			var consolDataObjectReader = new WhsTransitDispatchConsolDataObjectReader(consol, Logger, Factory);

			AssertExceptionThrown(typeof(DataObjectReadFailureException), @"No Dispatch Consignments (e.g. Forwarding Shipments) were included in the Dispatch Instruction.",
				() => consolDataObjectReader.ReadIntoBusinessObject());
		}

		#endregion

		#region TestImportingConsol_RemovedPackages

		public void TestImportingConsol_ShipmentDetached_RelatedDCNBecomeUnAuthorized()
		{
			Data.SetupForForwardingImport();
			var consol = new Shipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			consol.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.ATW, ServiceCode = ServiceCodeType.TWD } } });
			consol.DataContext.AddDataSource(DataContextType.ForwardingConsol, "C123");
			consol.ContainerMode = new ContainerMode { Code = "FCL" };
			consol.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { Data.Orgs.Consignor_CRAHOLSYD, Data.Orgs.Consignee_INTHEMSYD, Data.Orgs.Warehouse_WUFSHIJNB, Data.Orgs.Warehouse_INTHEMSYD });

			consol.SetContainerCollection(() => new DataObjectList<Container>());
			var container = Data.CreateContainer("A", 1);
			consol.ContainerCollection.Add(container);

			// shipment1
			consol.SetSubShipmentCollection(() => new DataObjectList<Shipment>());
			var shipment1 = Data.CreateShipmentWithPackagesAndContainerLinks("WAYBILLNO1", Tuple.Create((int?)1, "PKG1"));
			consol.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S123");
			consol.SubShipmentCollection.Add(shipment1);

			// shipment2
			var shipment2 = Data.CreateShipmentWithPackagesAndContainerLinks("WAYBILLNO2", Tuple.Create((int?)1, "PKG2"));
			consol.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S456");
			consol.SubShipmentCollection.Add(shipment2);

			// create receive consignments and packages
			var receiveConsol = Data.CreateReceiveConsolAndConsignmentsInDB(consol);
			AssertEquals("Precondition - Two consignments are created.", 2, receiveConsol.PopulatedConsignmentsForTesting.Length);

			var consolDataObjectReader = new WhsTransitDispatchConsolDataObjectReader(consol, Data.Logger, Factory);
			var consolidation = consolDataObjectReader.ReadIntoBusinessObject();

			var dispatchConsignments = consolidation.PopulatedConsignmentsForTesting;
			AssertEquals("Two dispatch consignments should be created for two shipments.", 2, dispatchConsignments.Count);
			AssertContainsExactElementsInAnyOrder(new[] { "WAYBILLNO1", "WAYBILLNO2" }, dispatchConsignments.Select(d => d.WDC_ConsignmentID));
			AssertNoExceptionThrown("Should not throw errors during the saving.", () => Factory.SaveForTesting());

			// detach a shipment
			consol.SubShipmentCollection.Remove(shipment1);
			consolDataObjectReader = new WhsTransitDispatchConsolDataObjectReader(consol, Data.Logger, Factory);
			consolidation = consolDataObjectReader.ReadIntoBusinessObject();
			dispatchConsignments = consolidation.PopulatedConsignmentsForTesting;
			AssertEquals("One dispatch consignment should be created.", 1, dispatchConsignments.Count);

			// The one removed is not authorized anymore
			var query = new ZQuery(WhsItemDispatchConsignmentSchema.WDC_ConsignmentID, "WAYBILLNO1");
			var dcnRelatedToDetachedShipment = Factory.Load<WhsItemDispatchConsignment>(query).First();
			AssertEquals("The dcn related to the detached shipment should not be authorized anymore.", false, dcnRelatedToDetachedShipment.WDC_IsAuthorizedForDispatch);
		}

		public void TestImportingConsol_SomePackageRemovedFromShipment_RelatedDCNStayAuthorized()
		{
			Data.SetupForForwardingImport();
			var consol = new Shipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			consol.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.ATW, ServiceCode = ServiceCodeType.TWD } } });
			consol.DataContext.AddDataSource(DataContextType.ForwardingConsol, "C123");
			consol.ContainerMode = new ContainerMode { Code = "FCL" };
			consol.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { Data.Orgs.Consignor_CRAHOLSYD, Data.Orgs.Consignee_INTHEMSYD, Data.Orgs.Warehouse_WUFSHIJNB, Data.Orgs.Warehouse_INTHEMSYD });

			consol.SetContainerCollection(() => new DataObjectList<Container>());
			var container = Data.CreateContainer("A", 1);
			consol.ContainerCollection.Add(container);

			// shipment1
			consol.SetSubShipmentCollection(() => new DataObjectList<Shipment>());
			var shipment1 = Data.CreateShipmentWithPackagesAndContainerLinks("WAYBILLNO1", Tuple.Create((int?)1, "PKG1"));
			consol.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S123");
			consol.SubShipmentCollection.Add(shipment1);

			// shipment2
			var shipment2 = Data.CreateShipmentWithCombinedPackages("WAYBILLNO2", ("PKG", 1, "PKG2"), ("PKG", 1, "PKG3"), ("BOX", 1, "BOX1"), ("BOX", 5, ""));
			consol.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S456");
			consol.SubShipmentCollection.Add(shipment2);

			// create receive consignments and packages
			var receiveConsol = Data.CreateReceiveConsolAndConsignmentsInDB(consol);
			AssertEquals("Precondition - Two consignments are created.", 2, receiveConsol.PopulatedConsignmentsForTesting.Length);

			var consolDataObjectReader = new WhsTransitDispatchConsolDataObjectReader(consol, Data.Logger, Factory);
			var consolidation = consolDataObjectReader.ReadIntoBusinessObject();

			var dispatchConsignments = consolidation.PopulatedConsignmentsForTesting;
			AssertEquals("Two dispatch consignments should be created for two shipments.", 2, dispatchConsignments.Count);
			AssertContainsExactElementsInAnyOrder(new[] { "WAYBILLNO1", "WAYBILLNO2" }, dispatchConsignments.Select(d => d.WDC_ConsignmentID));
			AssertNoExceptionThrown("Should not throw errors during the saving.", () => Factory.SaveForTesting());

			// remove a package under one shipment
			consol.SubShipmentCollection.First().PackingLineCollection.Remove(consol.SubShipmentCollection.First().PackingLineCollection.First());
			consolDataObjectReader = new WhsTransitDispatchConsolDataObjectReader(consol, Data.Logger, Factory);
			consolidation = consolDataObjectReader.ReadIntoBusinessObject();
			dispatchConsignments = consolidation.PopulatedConsignmentsForTesting;
			AssertEquals("Two dispatch consignments should be created for two shipments.", 2, dispatchConsignments.Count);

			// both dcn are still authorized
			foreach (var dispatchConsignment in dispatchConsignments)
			{
				AssertEquals("The consignment is authorized.", true, dispatchConsignment.WDC_IsAuthorizedForDispatch);
			}
		}

		#endregion

		#region TestLogMessage

		public void TestLogMessage_NoConsolObjectLoadingMessage()
		{
			var warehouse = Data.Warehouse;
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;

			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);

			var package1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var package2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			Factory.SaveForTesting();

			Data.SetupForForwardingImport();
			var consol = new Shipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			consol.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.DTW, ServiceCode = ServiceCodeType.TWD } } });
			consol.DataContext.AddDataSource(DataContextType.ForwardingConsol, "C123");
			consol.ContainerMode = new ContainerMode { Code = "FCL" };
			consol.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { Data.Orgs.Consignor_CRAHOLSYD, Data.Orgs.Consignee_INTHEMSYD, Data.Orgs.Warehouse_WUFSHIJNB, Data.Orgs.Warehouse_INTHEMSYD });

			consol.SetContainerCollection(() => new DataObjectList<Container>());
			var container = Data.CreateContainer("A", 1);
			consol.ContainerCollection.Add(container);

			// shipment
			consol.SetSubShipmentCollection(() => new DataObjectList<Shipment>());
			var shipment1 = Data.CreateShipmentWithPackagesAndContainerLinks("WAYBILLNO1", Tuple.Create((int?)1, "PKG1"));
			consol.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S123");
			consol.SubShipmentCollection.Add(shipment1);

			Logger.TopLevelDataObject = consol;

			var consolDataObjectReader = new WhsTransitDispatchConsolDataObjectReader(consol, Logger, Factory);
			consolDataObjectReader.ReadIntoBusinessObject();
			AssertEquals("Log message must not have dispatch consol loading text.",
@"Information - Recipient Role Service is 'DTW - Departure Transit Warehouse'.
Information - Searching for Transit Warehouse matching Departure CFS Address 'WUFSHIJNB - Level 2, Building G'.
Information - Matching 'DepartureCFSAddress':- Matched to 'WUFSHIJNB' by code, address 'SC1' (only address).
Information - Found Warehouse TWH from Departure CFS Address.
Error - Could not find 'Booking Party':- Organization Code is empty in 'SendingForwarderAddress' for this Departure Transit Warehouse, and UXML does not originate from this system.
Information - Data Source is 'ForwardingShipment - S1000000'.
Information - Searching for Dispatch Consignment for 'ForwardingShipment - S1000000'.
Information - No matching Dispatch Consignment found, creating new Dispatch Consignment.
Information - Populating Dispatch Consignment...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Populating Packages for Dispatch Consignment...
Information - Some Imported Packlines have Package IDs. Attempting to match by Package IDs.
Information - Some Imported Packlines have Package IDs. Attempting to match by Package IDs.
Information - Successfully loaded matching WhsItemPackageState.
Information - Populating WhsItemPackageState...
Information - The following packages have been attached to Dispatch Consignment WAYBILLNO1:
Package        RCN
PKG1           RCN1 (EXTREF1)
Information - Successfully loaded matching WhsItemPackageState.
Information - Populating WhsItemPackageState...
Information - Populating Addresses for Dispatch Consignment...
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Matching 'ConsigneeDocumentaryAddress':- Matched to 'INTHEMSYD' by code, address '' (only address).
Information - Added Dispatch Consignment DC00000001 from UniversalShipment.
Information - Attempting to create Container Loading Plans as Containers have been specified.
Information - Creating 1 Load List(s) for Containers with a Loading Plan.
Error - Could not find 'Booking Party':- Organization Code is empty in 'SendingForwarderAddress' for this Departure Transit Warehouse, and UXML does not originate from this system.
Information - No matching Dispatch Load List found, creating new Dispatch Load List.
Information - Populating Dispatch Load List...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Successfully loaded matching WhsItemPackageState.
Information - Populating WhsItemPackageState...
Information - The following packages have been attached to Load List DLL00000001:
Package        RCN               DCN
PKG1           RCN1 (EXTREF1)    WAYBILLNO1 (DC00000001)
Information - Added Dispatch Load List DLL00000001 from UniversalShipment.
Information - No matching Dispatch Transportation Unit found, creating new Dispatch Transportation Unit.
Information - Populating Dispatch Transportation Unit...
Error - Could not find 'Booking Party':- Organization Code is empty in 'SendingForwarderAddress' for this Departure Transit Warehouse, and UXML does not originate from this system.
Information - No matching PkgPackage found, creating new PkgPackage.
Information - Populating PkgPackage...
Information - Successfully loaded matching Container Type.
Information - Updated record from UniversalShipment.", Logger.Logs);
		}

		#endregion

		#endregion

		#region TestUXMLFromTransportRunSheetWithoutVehicleNumber

		public void TestUXMLFromTransportRunSheetWithoutVehicleNumber()
		{
			Data.SetupForForwardingImport();

			var runSheetUXMlWithEmptyFlightNumber = CreateUXML("R1", "");
			var runSheetUXMlWithoutFlightNumber = CreateUXML("R1", null);
			var runSheetUXMlWithoutRunSheetNumber = CreateUXML("", "");
			var runSheetUXMlWithoutRunSheet = CreateUXML("", "V1");
			var runSheetUXMlWithRunSheetAndFlightNumber = CreateUXML("R1", "V1");
			var shipmentWithoutRunSheetDataSource = CreateUXML(null, "V1");

			AssertExceptionThrown<DataObjectReadFailureException>("Run Sheet number is present in UXML but flight number is empty.", "Run Sheet vehicle number must be populated in UXML.", () => new WhsTransitDispatchConsolDataObjectReader(runSheetUXMlWithEmptyFlightNumber, Logger, Factory).ReadIntoBusinessObject());
			AssertExceptionThrown<DataObjectReadFailureException>("Run Sheet number is present in UXML but flight number is null.", "Run Sheet vehicle number must be populated in UXML.", () => new WhsTransitDispatchConsolDataObjectReader(runSheetUXMlWithoutFlightNumber, Logger, Factory).ReadIntoBusinessObject());
			AssertNoExceptionThrown(() => new WhsTransitDispatchConsolDataObjectReader(runSheetUXMlWithoutRunSheetNumber, Logger, Factory).ReadIntoBusinessObject());
			AssertNoExceptionThrown(() => new WhsTransitDispatchConsolDataObjectReader(runSheetUXMlWithoutRunSheet, Logger, Factory).ReadIntoBusinessObject());
			AssertNoExceptionThrown(() => new WhsTransitDispatchConsolDataObjectReader(runSheetUXMlWithRunSheetAndFlightNumber, Logger, Factory).ReadIntoBusinessObject());
			AssertNoExceptionThrown(() => new WhsTransitDispatchConsolDataObjectReader(shipmentWithoutRunSheetDataSource, Logger, Factory).ReadIntoBusinessObject());
		}

		Shipment CreateUXML(string runSheetNumber, string voyageNumber)
		{
			var consol = new Shipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			consol.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.ATW, ServiceCode = ServiceCodeType.TWD } } });
			consol.ContainerMode = new ContainerMode { Code = "FCL" };
			consol.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { Data.Orgs.Consignor_CRAHOLSYD, Data.Orgs.Consignee_INTHEMSYD, Data.Orgs.Warehouse_WUFSHIJNB, Data.Orgs.Warehouse_INTHEMSYD });
			consol.SetSubShipmentCollection(() => new DataObjectList<Shipment>());
			var shipment = Data.CreateShipmentWithPackages("S123");
			consol.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S123");
			consol.SubShipmentCollection.Add(shipment);

			consol.DataContext = DataContextFactory.New();
			consol.DataContext.AddDataSource(DataContextType.TransportConsignmentRunSheet, runSheetNumber);
			consol.VoyageFlightNo = voyageNumber;
			return consol;
		}

		#endregion

		#region TestImportingConsolCreateMultipleShipments

		public void TestImportingConsolCreateMultipleShipments()
		{
			Data.SetupForForwardingImport();
			Data.ShipmentDataObject.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.DTW, ServiceCode = ServiceCodeType.TWD } } });
			var consignmentDataObject = Data.CreateShipmentWithPackages("A123", new[] { "Pack1", "Pack2" });

			Data.CreateReceiveConsignmentInDB(consignmentDataObject);
			var consignmentDataObject1 = Data.CreateShipmentWithPackages("B456", new[] { "Pack3", "Pack4", "Pack5" });
			Data.CreateReceiveConsignmentInDB(consignmentDataObject1);
			Factory.SaveForTesting();

			var shipment1 = Data.CreateShipmentWithPackages("A123", "Pack1", "Pack2");
			var shipment2 = Data.CreateShipmentWithPackages("B456", "Pack3", "Pack4", "Pack5");
			Data.ShipmentDataObject.SetSubShipmentCollection(() => new DataObjectList<Shipment> { shipment1, shipment2 });

			Logger.TopLevelDataObject = Data.ShipmentDataObject;
			var consolDataObjectReader = new WhsTransitDispatchConsolDataObjectReader(Data.ShipmentDataObject, Logger, Factory);
			var consol = consolDataObjectReader.ReadIntoBusinessObject();
			AssertEquals(DataContextType.TransitDispatchConsol, consolDataObjectReader.DataContextType);

			var dispatchConsignments = consol.PopulatedConsignmentsForTesting;
			AssertEquals("Two dispatch consignments should be created for two shipments.", 2, dispatchConsignments.Count);
			AssertEquals("No dispatch transportation units should be created as part of the import process.", 0, Factory.Load<WhsItemDispatchTransportationUnit>(new ZQuery()).Length);

			var packageStatesForShipment1 = dispatchConsignments.Single(d => d.WDC_ConsignmentID == "A123").PackageStates;
			AssertEquals("Shipment A123 must have two packages.", 2, packageStatesForShipment1.Count);
			AssertContainsExactElementsInAnyOrder(new[] { "Pack1", "Pack2" }, packageStatesForShipment1.Select(p => p.Package.OriginalPackageID.ToString()));

			var packageStatesForShipment2 = dispatchConsignments.Single(d => d.WDC_ConsignmentID == "B456").PackageStates;
			AssertEquals("Shipment B456 must have three packages.", 3, packageStatesForShipment2.Count);
			AssertContainsExactElementsInAnyOrder(new[] { "Pack3", "Pack4", "Pack5" }, packageStatesForShipment2.Select(p => p.Package.OriginalPackageID.ToString()));
		}

		#endregion

		#region TestAdditionalReferences

		public void TestAdditionalReferencesForLoadList()
		{
			var now = ZDateTime.Now;
			Data.SetupForForwardingImport();
			Data.Warehouse.RelatedCompanyBranch.GB_RL_NKHomePort = "ZAJNB";

			var consol = new Shipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			consol.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.DTW, ServiceCode = ServiceCodeType.TWD } } });
			consol.DataContext.AddDataSource(DataContextType.ForwardingConsol, "C123");
			consol.ContainerMode = new ContainerMode { Code = "FCL" };
			consol.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { Data.Orgs.Consignor_CRAHOLSYD, Data.Orgs.Consignee_INTHEMSYD, Data.Orgs.Warehouse_WUFSHIJNB });
			consol.SetSubShipmentCollection(() => new DataObjectList<Shipment>());

			var shipment = Data.CreateShipmentWithPackages("S123");
			consol.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S123");
			consol.SubShipmentCollection.Add(shipment);

			consol.SetTransportLegCollection(() => new DataObjectList<TransportLeg>
			{
				new TransportLeg { VesselName = "TestVessel", VoyageFlightNo = "TestVoyage", PortOfLoading = new UNLOCO() { Code = "ZAJNB" }, PortOfDischarge = new UNLOCO() { Code = "SLCMB" }, LegOrder = 1, EstimatedDeparture = now.AddDays(2) },
				new TransportLeg { VesselName = "Vessel2", VoyageFlightNo = "Voyage2", PortOfLoading = new UNLOCO() { Code = "SLCMB" }, PortOfDischarge = new UNLOCO() { Code = "AUSYD" }, LegOrder = 2, EstimatedDeparture = now.AddDays(4) }
			});

			Logger.TopLevelDataObject = consol;
			var consolDataObjectReader = new WhsTransitDispatchConsolDataObjectReader(consol, Logger, Factory);
			consolDataObjectReader.ReadIntoBusinessObject();
			var loadList = Factory.Load<WhsItemDispatchLoadList>(new ZQuery()).Single();
			var additionalReferenceNumbers = Factory.Load<CusEntryNumber>(new ZQuery(CusEntryNumSchema.CE_ParentID, loadList.PK));
			AssertEquals("Vessel name must be imported as an additional reference.", "TestVessel", additionalReferenceNumbers.Single(e => e.CE_EntryType == WarehouseAdditionalReferenceTypes.Codes.Vessel).CE_EntryNum);
			AssertEquals("Voyage number must be imported as an additional reference.", "TestVoyage", additionalReferenceNumbers.Single(e => e.CE_EntryType == WarehouseAdditionalReferenceTypes.Codes.VoyageFlightNumber).CE_EntryNum);
			AssertEquals("Destination port must be imported as an additional reference.", "SLCMB", additionalReferenceNumbers.Single(e => e.CE_EntryType == WarehouseAdditionalReferenceTypes.Codes.DestinationPort).CE_EntryNum);
			AssertEquals("Estimated delivery date must be imported as an additional reference.", now.AddDays(2).FormatDateTime(), additionalReferenceNumbers.Single(e => e.CE_EntryType == WarehouseAdditionalReferenceTypes.Codes.ETDDate).CE_EntryNum);
		}

		#endregion

		#region TestSubShipmentWithNoPackingLineCollection

		public void TestSubShipmentWithNoPackingLineCollection()
		{
			Data.SetupForForwardingImport();
			Data.ShipmentDataObject.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.DTW, ServiceCode = ServiceCodeType.TWD } } });
			var consignmentDataObject = Data.CreateShipmentWithPackages("A123", new[] { "Pack1", "Pack2" });

			Data.CreateReceiveConsignmentInDB(consignmentDataObject);
			Factory.SaveForTesting();

			var shipment1 = Data.CreateShipmentWithPackages("A123");
			var shipment2 = Data.CreateShipmentWithPackages("B456");
			shipment1.SetPackingLineCollection(() => null);
			shipment2.SetPackingLineCollection(() => null);
			Data.ShipmentDataObject.SetSubShipmentCollection(() => new DataObjectList<Shipment> { shipment1, shipment2 });

			Logger.TopLevelDataObject = Data.ShipmentDataObject;
			var consolDataObjectReader = new WhsTransitDispatchConsolDataObjectReader(Data.ShipmentDataObject, Logger, Factory);
			AssertNoExceptionThrown("Should not throw Null Reference Exception", () => consolDataObjectReader.ReadIntoBusinessObject());
		}

		#endregion

		#region TestGateBooking

		public void TestGateBookingWithConsolImportingWithLoosePackages()
		{
			// Prepare data
			Data.SetupForForwardingImport();
			var shipment = Data.HeaderDataObject;
			var subShipment = Data.CreateShipmentWithPackages("A123", "Pack1", "Pack2");
			shipment.SetSubShipmentCollection(() => new DataObjectList<Shipment> { subShipment });

			// Fill Gate Booking Information
			Data.SetupHeaderObjectForGateBooking(shipment);
			Data.SetupNewDataContextWithDataSource(shipment, gateBookingNumber: "GTB001", isArrival: true, keepExistingDataContext: true);
			var gateBookingSubShipment = Data.CreateSubShipmentForGateBookingHeaderObject("GMB001", shipment.WayBillNumber.Value, isPickup: false, new ZString[] { "Pack1", "Pack2" });
			Data.SetupNewDataContextWithDataSource(gateBookingSubShipment, null, isArrival: false, keepExistingDataContext: true);
			shipment.SubShipmentCollection.Add(gateBookingSubShipment);
			shipment.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.ATW } } });
			shipment.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			Logger.TopLevelDataObject = shipment;

			// Prepare receive consignments and packages
			var receiveConsol = Data.CreateReceiveConsolAndConsignmentsInDB(shipment);
			AssertEquals("Precondition - One consignment is created.", 1, receiveConsol.PopulatedConsignmentsForTesting.Length);

			new WhsTransitDispatchConsolDataObjectReader(shipment, Logger, Factory).ReadIntoBusinessObject();

			(WhsItemDispatchLoadList[] dlls, WhsItemDispatchConsignment[] dcns, WhsItemDispatchLoadListDTUPivot[] pivots, WhsItemDispatchTransportationUnit[] dtus) =
				AssertEntitiesCount(dllCount: 2, dcnCount: 1, dllDtuPivotCount: 1, dtuCount: 1);

			var originalDll = dlls.FirstOrDefault(dll => !dll.WDL_IsActive);
			var newVehicleDll = dlls.FirstOrDefault(dll => dll.WDL_IsActive);
			AssertPackageStatesUnderTheDCNsShouldLinkToASpecificDll(new[] { dcns[0].PK }, newVehicleDll.PK);
			AssertNoExceptionThrown("Should not throw errors during the saving.", () => Factory.SaveForTesting());
			AssertNotEquals("A new DLL for the vehicle should be created", originalDll.PK, newVehicleDll.PK);
			AssertEquals("Vehicle Registration Number should be populated.", "DEF-023", dtus[0].WDH_VehicleReference);
			AssertEquals("Driver should be populated.", "DRIVER1", dtus[0].WDH_SignedBy);
			AssertEquals("Original DLL should be deactivated", false, originalDll.WDL_IsActive);
			AssertEquals("New Vehicle DLL should be active", true, newVehicleDll.WDL_IsActive);
			var links = Factory.Load<StmUniversalJobLink>(new ZQuery());
			AssertUniversalJobLink(links, null, dtus[0].PK, WhsItemDispatchTransportationUnitSchema.Constants.Prefix, DataContextType.GateBooking, "GTB001", "EDI", "DAT", "EDI");
			AssertUniversalJobLink(links, null, dtus[0].PK, WhsItemDispatchTransportationUnitSchema.Constants.Prefix, DataContextType.GateMovementBooking, "GMB001", "EDI", "DAT", "EDI");
			AssertUniversalJobLinkShouldBeUnique(links, DataContextType.GateBooking, "GTB001", WhsItemDispatchTransportationUnitSchema.Constants.Prefix);
			AssertUniversalJobLinkShouldBeUnique(links, DataContextType.GateMovementBooking, "GMB001", WhsItemDispatchTransportationUnitSchema.Constants.Prefix);
			AssertGateRelatedBookingConfirmedLogs(dtus[0],
				expectedGateBookingConfirmedLogRef: $"{dtus[0].WDH_ReferenceNumber}|FAC=WHS|LOC=ThereVille|MST=GateBooking|RFN=GTB001|TYP=WhsItemDispatchTransportationUnit|WHS=TW2",
				expectedGateMovementBookingConfirmedLogRef: $"{dtus[0].WDH_ReferenceNumber}|FAC=WHS|LOC=ThereVille|MST=GateMovementBooking|RFN=GMB001|TYP=WhsItemDispatchTransportationUnit|WHS=TW2");
		}

		public void TestGateBookingAfterConsolImportingWithLoosePackages_BookingReferenceIsDispatchLoadList()
		{
			TestGateBookingAfterConsolImportingWithLoosePackagesCore(ReferenceNumberTypes.DispatchLoadList);
		}

		public void TestGateBookingAfterConsolImportingWithLoosePackages_BookingReferenceIsForwardingConsolNumber()
		{
			TestGateBookingAfterConsolImportingWithLoosePackagesCore(ReferenceNumberTypes.ForwardingConsolNumber);
		}

		public void TestGateBookingAfterConsolImportingWithLoosePackages_BookingReferenceIsMasterBill()
		{
			TestGateBookingAfterConsolImportingWithLoosePackagesCore(ReferenceNumberTypes.MasterBill);
		}

		public void TestGateBookingAfterConsolImportingWithLoosePackagesCore(ReferenceNumberTypes referenceNumberType)
		{
			// Prepare data
			Data.SetupForForwardingImport();
			var consol = new Shipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			consol.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.ATW, ServiceCode = ServiceCodeType.TWD } } });
			consol.DataContext.AddDataSource(DataContextType.ForwardingConsol, "C123");
			consol.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { Data.Orgs.Consignor_CRAHOLSYD, Data.Orgs.Consignee_INTHEMSYD, Data.Orgs.Warehouse_WUFSHIJNB, Data.Orgs.Warehouse_INTHEMSYD });
			consol.SetSubShipmentCollection(() => new DataObjectList<Shipment>());
			consol.WayBillNumber = "WAYBILLNO1";

			var subShipment1 = Data.CreateShipmentWithPackages("WAYBILLNO2", new string[] { "PKG1", "PKG2" });
			consol.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S123");
			consol.SubShipmentCollection.Add(subShipment1);
			Logger.TopLevelDataObject = consol;

			// Prepare receive consignments and packages
			var receiveConsol = Data.CreateReceiveConsolAndConsignmentsInDB(consol);
			AssertEquals("Precondition - One consignment is created.", 1, receiveConsol.PopulatedConsignmentsForTesting.Length);

			new WhsTransitDispatchConsolDataObjectReader(consol, Logger, Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			(WhsItemDispatchLoadList[] dlls, WhsItemDispatchConsignment[] dcns, WhsItemDispatchLoadListDTUPivot[] pivots, WhsItemDispatchTransportationUnit[] dtus) =
				AssertEntitiesCount(dllCount: 1, dcnCount: 1, dllDtuPivotCount: 0, dtuCount: 0);
			var originalDll = dlls[0];
			AssertPackageStatesUnderTheDCNsShouldLinkToASpecificDll(new[] { dcns[0].PK }, originalDll.PK);
			AssertNoExceptionThrown("Should not throw errors during the saving.", () => Factory.SaveForTesting());

			// Prepare gate booking shipment
			var bookingReference = referenceNumberType == ReferenceNumberTypes.DispatchLoadList ? originalDll.WDL_JobID : referenceNumberType == ReferenceNumberTypes.ForwardingConsolNumber ? "C123" : consol.WayBillNumber.Value;
			var newUniversalObjectFactory = NewUniversalObjectFactory();
			var gateBookingShipment = Data.HeaderDataObjectForGateBooking_ATW;
			var subShipment2 = Data.CreateSubShipmentForGateBookingHeaderObject("GMB001", bookingReference, isPickup: false, new ZString[] { "PKG1", "PKG2" });
			gateBookingShipment.SetSubShipmentCollection(() => new DataObjectList<Shipment>() { subShipment2 });
			Logger.TopLevelDataObject = gateBookingShipment;

			new WhsTransitDispatchConsolDataObjectReader(gateBookingShipment, Logger, newUniversalObjectFactory).ReadIntoBusinessObject();
			newUniversalObjectFactory.SaveForTesting();

			(dlls, dcns, pivots, dtus) = AssertEntitiesCount(dllCount: referenceNumberType == ReferenceNumberTypes.MasterBill ? 1 : 2, dcnCount: 1, dllDtuPivotCount: 1, dtuCount: 1, newUniversalObjectFactory);
			var dllAfterFirstGateBooking = dlls.FirstOrDefault(d => d.WDL_IsActive);
			var dtuAfterFirstGateBooking = dtus[0];
			AssertPackageStatesUnderTheDCNsShouldLinkToASpecificDll(new[] { dcns[0].PK }, dllAfterFirstGateBooking.PK, newUniversalObjectFactory);
			if (referenceNumberType == ReferenceNumberTypes.MasterBill)
			{
				AssertEquals("Existing DLL should be matched by master bill and will be used as the vehicle DLL.", originalDll.PK, dllAfterFirstGateBooking.PK);
			}
			else
			{
				AssertEquals("Other DLL should be inactive", 1, dlls.Count(d => !d.WDL_IsActive));
			}
			AssertEquals("Transport Company Address should be populated.", "INTHEMSYD", dtuAfterFirstGateBooking.DocAddresses.FindByDocAddressType(DocAddressType.TransportCompanyDocumentaryAddress).Organisation.OH_Code);
			AssertEquals("Vehicle Registration Number should be populated.", "DEF-023", dtuAfterFirstGateBooking.WDH_VehicleReference);
			AssertEquals("Driver should be populated.", "DRIVER1", dtuAfterFirstGateBooking.WDH_SignedBy);
			AssertEquals("Vehicle DLL should be active", true, dllAfterFirstGateBooking.WDL_IsActive);
			var links = Factory.Load<StmUniversalJobLink>(new ZQuery());
			AssertUniversalJobLink(links, null, dtuAfterFirstGateBooking.PK, WhsItemDispatchTransportationUnitSchema.Constants.Prefix, DataContextType.GateBooking, "GTB001", "EDI", "DAT", "EDI");
			AssertUniversalJobLink(links, null, dtuAfterFirstGateBooking.PK, WhsItemDispatchTransportationUnitSchema.Constants.Prefix, DataContextType.GateMovementBooking, "GMB001", "EDI", "DAT", "EDI");
			AssertGateRelatedBookingConfirmedLogs(dtuAfterFirstGateBooking,
				expectedGateBookingConfirmedLogRef: $"{dtuAfterFirstGateBooking.WDH_ReferenceNumber}|FAC=WHS|LOC=ThereVille|MST=GateBooking|RFN=GTB001|TYP=WhsItemDispatchTransportationUnit|WHS=TW2",
				expectedGateMovementBookingConfirmedLogRef: $"{dtuAfterFirstGateBooking.WDH_ReferenceNumber}|FAC=WHS|LOC=ThereVille|MST=GateMovementBooking|RFN=GMB001|TYP=WhsItemDispatchTransportationUnit|WHS=TW2");
			TestGateBooking_UpdateBookingCore(referenceNumberType, dllAfterFirstGateBooking, bookingReference, expectedDLLCount: 3);
		}

		void TestGateBooking_UpdateBookingCore(ReferenceNumberTypes referenceNumberType, WhsItemDispatchLoadList dllAfterFirstGateBooking, ZString bookingReference, int expectedDLLCount)
		{
			// Update gate booking shipment
			var newTransportCompanyDocAddress = OrganizationAddressTestHelper.GetNewAddressData_CRAHOLSYD(DocAddressType.TransportCompanyDocumentaryAddress);
			var newUniversalObjectFactory = NewUniversalObjectFactory();
			var gateBookingShipment = Data.CreateHeaderDataObjectForGateBooking_ATW_WithTransportCompany(newTransportCompanyDocAddress);
			var subShipment2 = Data.CreateSubShipmentForGateBookingHeaderObject("GMB001", bookingReference, isPickup: false, new ZString[] { "PKG1", "PKG2" }, newTransportCompanyDocAddress);
			gateBookingShipment.SetSubShipmentCollection(() => new DataObjectList<Shipment>() { subShipment2 });
			if (referenceNumberType == ReferenceNumberTypes.DispatchLoadList)
			{
				subShipment2.BookingConfirmationReference = dllAfterFirstGateBooking.WDL_JobID;
			}

			gateBookingShipment.VehicleRun.CrewCollection.FirstOrDefault().FullName = "DRIVER2";
			gateBookingShipment.PreCarriageShipmentCollection.FirstOrDefault().VehicleRun.Vehicle.Registration.Number = "ABC-123";
			Logger.TopLevelDataObject = gateBookingShipment;

			new WhsTransitDispatchConsolDataObjectReader(gateBookingShipment, Logger, newUniversalObjectFactory).ReadIntoBusinessObject();
			newUniversalObjectFactory.SaveForTesting();

			var (dlls, dcns, pivots, dtus) = AssertEntitiesCount(dllCount: referenceNumberType == ReferenceNumberTypes.MasterBill ? 1 : expectedDLLCount, dcnCount: 1, dllDtuPivotCount: 1, dtuCount: 1, newUniversalObjectFactory);
			var dtuDll = pivots[0].WLD_WDL_TransitDispatchLoadList;
			var dtuAfterSecondGateBooking = dtus[0];
			AssertEquals("Vehicle DLL should be active", true, dlls.FirstOrDefault(d => d.PK == dtuDll)?.WDL_IsActive);
			if (referenceNumberType != ReferenceNumberTypes.MasterBill)
			{
				var expectedOtherInactiveDLLsCount = expectedDLLCount - 1;
				AssertEquals("Other DLLs should be inactive", expectedOtherInactiveDLLsCount, dlls.Count(d => !d.WDL_IsActive));
			}
			AssertEquals("Transport Company Address should be updated.", "CRAHOLSYD", dtuAfterSecondGateBooking.DocAddresses.FindByDocAddressType(DocAddressType.TransportCompanyDocumentaryAddress).Organisation.OH_Code);
			AssertEquals("Vehicle Registration Number should be updated.", "ABC-123", dtuAfterSecondGateBooking.WDH_VehicleReference);
			AssertEquals("Driver should be updated.", "DRIVER2", dtuAfterSecondGateBooking.WDH_SignedBy);

			var links = Factory.Load<StmUniversalJobLink>(new ZQuery());
			AssertUniversalJobLink(links, null, dtuAfterSecondGateBooking.PK, WhsItemDispatchTransportationUnitSchema.Constants.Prefix, DataContextType.GateBooking, "GTB001", "EDI", "DAT", "EDI");
			AssertUniversalJobLink(links, null, dtuAfterSecondGateBooking.PK, WhsItemDispatchTransportationUnitSchema.Constants.Prefix, DataContextType.GateMovementBooking, "GMB001", "EDI", "DAT", "EDI");
			AssertUniversalJobLinkShouldBeUnique(links, DataContextType.GateBooking, "GTB001");
			AssertUniversalJobLinkShouldBeUnique(links, DataContextType.GateMovementBooking, "GMB001");
			AssertGateRelatedBookingConfirmedLogs(dtuAfterSecondGateBooking,
				expectedGateBookingConfirmedLogRef: $"{dtuAfterSecondGateBooking.WDH_ReferenceNumber}|FAC=WHS|LOC=ThereVille|MST=GateBooking|RFN=GTB001|TYP=WhsItemDispatchTransportationUnit|WHS=TW2",
				expectedGateMovementBookingConfirmedLogRef: $"{dtuAfterSecondGateBooking.WDH_ReferenceNumber}|FAC=WHS|LOC=ThereVille|MST=GateMovementBooking|RFN=GMB001|TYP=WhsItemDispatchTransportationUnit|WHS=TW2");
		}

		public void TestGateBookingAfterShipmentImportingWithLoosePackages_BookingReferenceIsDispatchConsignment()
		{
			TestGateBookingAfterShipmentImportingWithLoosePackagesCore(ReferenceNumberTypes.DispatchConsignment);
		}

		public void TestGateBookingAfterShipmentImportingWithLoosePackages_BookingReferenceIsForwardingShipmentNumber()
		{
			TestGateBookingAfterShipmentImportingWithLoosePackagesCore(ReferenceNumberTypes.ForwardingShipmentNumber);
		}

		public void TestGateBookingAfterShipmentImportingWithLoosePackages_BookingReferenceIsHouseBill()
		{
			TestGateBookingAfterShipmentImportingWithLoosePackagesCore(ReferenceNumberTypes.HouseBill);
		}

		public void TestGateBookingAfterShipmentImportingWithLoosePackagesCore(ReferenceNumberTypes referenceNumberType)
		{
			// Prepare data
			var houseBillNumber = "HSB001";

			Data.SetupForForwardingImport();
			var consol = new Shipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			consol.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.ATW, ServiceCode = ServiceCodeType.TWD } } });
			consol.DataContext.AddDataSource(DataContextType.ForwardingConsol, "C123");
			consol.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { Data.Orgs.Consignor_CRAHOLSYD, Data.Orgs.Consignee_INTHEMSYD, Data.Orgs.Warehouse_WUFSHIJNB, Data.Orgs.Warehouse_INTHEMSYD });
			consol.SetSubShipmentCollection(() => new DataObjectList<Shipment>());
			consol.WayBillNumber = "WAYBILLNO1";
			var subShipment1 = Data.CreateShipmentWithPackages(houseBillNumber, new string[] { "PKG1", "PKG2" });
			consol.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S123");
			consol.SubShipmentCollection.Add(subShipment1);
			Logger.TopLevelDataObject = consol;

			// Prepare receive consignments and packages
			var receiveConsol = Data.CreateReceiveConsolAndConsignmentsInDB(consol);
			AssertEquals("Precondition - One consignment is created.", 1, receiveConsol.PopulatedConsignmentsForTesting.Length);

			// Prepare dispatch consignment and packages
			var warehouse = Data.WarehouseINTHEMSYD;
			var dcn = Helper.CreateDispatchConsignment("DC1", warehouse.PK);
			dcn.WDC_HouseBillNumber = houseBillNumber;
			var pkgStates = receiveConsol.PopulatedConsignmentsForTesting[0].PackageStates;
			pkgStates.ForEach(ps => ps.WPS_WDC_TransitDispatchConsignment = dcn.PK);
			var shipmentNumber = Helper.CreateAdditionalReference(dcn, "S123", WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber).CE_EntryNum;
			Factory.SaveForTesting();

			// Prepare gate booking shipment
			var bookingReference = referenceNumberType == ReferenceNumberTypes.DispatchConsignment ? dcn.WDC_JobID : referenceNumberType == ReferenceNumberTypes.ForwardingShipmentNumber ? shipmentNumber : houseBillNumber;
			var gateBookingShipment = Data.HeaderDataObjectForGateBooking_ATW;
			var subShipment2 = Data.CreateSubShipmentForGateBookingHeaderObject("GMB001", bookingReference, isPickup: false, new ZString[] { "PKG1", "PKG2" });
			gateBookingShipment.SetSubShipmentCollection(() => new DataObjectList<Shipment>() { subShipment2 });
			Logger.TopLevelDataObject = gateBookingShipment;

			new WhsTransitDispatchConsolDataObjectReader(gateBookingShipment, Logger, Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			(WhsItemDispatchLoadList[] dlls, WhsItemDispatchConsignment[] dcns, WhsItemDispatchLoadListDTUPivot[] pivots, WhsItemDispatchTransportationUnit[] dtus) =
				AssertEntitiesCount(dllCount: 1, dcnCount: 1, dllDtuPivotCount: 1, dtuCount: 1);
			var dtuDll = pivots[0].WLD_WDL_TransitDispatchLoadList;
			var dtu = dtus[0];

			AssertEquals("Vehicle DLL should be active", true, dlls.FirstOrDefault(dll => dll.PK == dtuDll)?.WDL_IsActive);
			AssertEquals("Vehicle Registration Number should be updated.", "DEF-023", dtu.WDH_VehicleReference);
			AssertEquals("Driver should be updated.", "DRIVER1", dtu.WDH_SignedBy);
			var links = Factory.Load<StmUniversalJobLink>(new ZQuery());
			AssertUniversalJobLink(links, null, dtu.PK, WhsItemDispatchTransportationUnitSchema.Constants.Prefix, DataContextType.GateBooking, "GTB001", "EDI", "DAT", "EDI");
			AssertUniversalJobLink(links, null, dtu.PK, WhsItemDispatchTransportationUnitSchema.Constants.Prefix, DataContextType.GateMovementBooking, "GMB001", "EDI", "DAT", "EDI");
			AssertUniversalJobLinkShouldBeUnique(links, DataContextType.GateBooking, "GTB001");
			AssertUniversalJobLinkShouldBeUnique(links, DataContextType.GateMovementBooking, "GMB001");
			AssertGateRelatedBookingConfirmedLogs(dtu,
				expectedGateBookingConfirmedLogRef: $"{dtu.WDH_ReferenceNumber}|FAC=WHS|LOC=ThereVille|MST=GateBooking|RFN=GTB001|TYP=WhsItemDispatchTransportationUnit|WHS=TW2",
				expectedGateMovementBookingConfirmedLogRef: $"{dtu.WDH_ReferenceNumber}|FAC=WHS|LOC=ThereVille|MST=GateMovementBooking|RFN=GMB001|TYP=WhsItemDispatchTransportationUnit|WHS=TW2");

			TestGateBooking_UpdateBookingCore(referenceNumberType, dlls[0], bookingReference, expectedDLLCount: 2);
		}

		public void TestGateBookingWithABlindContainer_BookingReferenceIsMasterBill_ContainerASNFullMatch()
		{
			TestGateBookingWithABlindContainerCore(true, ReferenceNumberTypes.MasterBill, true);
		}

		public void TestGateBookingWithABlindContainer_BookingReferenceIsMasterBill_ContainerASNPartialMatch()
		{
			TestGateBookingWithABlindContainerCore(false, ReferenceNumberTypes.MasterBill, true);
		}

		public void TestGateBookingWithABlindContainer_BookingReferenceIsHouseBill_ContainerASNFullMatch()
		{
			TestGateBookingWithABlindContainerCore(true, ReferenceNumberTypes.HouseBill, true);
		}

		public void TestGateBookingWithABlindContainer_BookingReferenceIsHouseBill_ContainerASNPartialMatch()
		{
			TestGateBookingWithABlindContainerCore(false, ReferenceNumberTypes.HouseBill, true);
		}

		public void TestGateBookingWithABlindContainer_ContainerDCNFullMatch()
		{
			TestGateBookingWithABlindContainerCore(true, ReferenceNumberTypes.HouseBill, false);
		}

		public void TestGateBookingWithABlindContainer_ContainerDCNPartialMatch()
		{
			TestGateBookingWithABlindContainerCore(false, ReferenceNumberTypes.HouseBill, false);
		}

		public void TestGateBookingWithABlindContainer_BookingReferenceIsContainerNumber()
		{
			TestGateBookingWithABlindContainerCore(false, ReferenceNumberTypes.ContainerNumber, false);
		}

		public void TestGateBookingWithABlindContainerCore(bool containerFullMatch, ReferenceNumberTypes referenceNumberType, bool hasConsol)
		{
			// Prepare data
			Data.SetupForForwardingImport();
			var consol = new Shipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			consol.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.ATW, ServiceCode = ServiceCodeType.TWD } } });
			consol.DataContext.AddDataSource(DataContextType.ForwardingConsol, "C123");
			consol.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { Data.Orgs.Consignor_CRAHOLSYD, Data.Orgs.Consignee_INTHEMSYD, Data.Orgs.Warehouse_WUFSHIJNB, Data.Orgs.Warehouse_INTHEMSYD });
			consol.SetSubShipmentCollection(() => new DataObjectList<Shipment>());
			consol.WayBillNumber = "WAYBILLNO1";
			var subShipment1 = Data.CreateShipmentWithPackages("WAYBILLNO2", new string[] { "PKG1", "PKG2" });
			consol.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S123");
			consol.SubShipmentCollection.Add(subShipment1);
			Logger.TopLevelDataObject = consol;

			// Prepare receive consignments and packages
			Data.CreateReceiveConsolAndConsignmentsInDB(consol);

			// Dispatch consol
			if (hasConsol)
			{
				new WhsTransitDispatchConsolDataObjectReader(consol, Logger, Factory).ReadIntoBusinessObject();
			}
			else
			{
				new WhsTransitDispatchConsignmentDataObjectReader(subShipment1, Logger, Factory).ReadIntoBusinessObject();
			}

			Factory.SaveForTesting();

			(WhsItemDispatchLoadList[] dlls, WhsItemDispatchConsignment[] dcns, WhsItemDispatchLoadListDTUPivot[] pivots, WhsItemDispatchTransportationUnit[] dtus) =
				AssertEntitiesCount(dllCount: hasConsol ? 1 : 0, dcnCount: 1, dllDtuPivotCount: 0, dtuCount: 0);

			// Prepare gate booking shipment
			var gateBookingShipment = Data.HeaderDataObjectForGateBooking_ATW;
			var subShipment2 = Data.CreateSubShipmentForGateBookingHeaderObject("GMB001", referenceNumberType == ReferenceNumberTypes.ContainerNumber ? new ZString("CNT-1") : referenceNumberType == ReferenceNumberTypes.HouseBill ? subShipment1.WayBillNumber.Value : consol.WayBillNumber.Value, isPickup: false, new ZString[] { "PKG1", "PKG2" });
			subShipment2.PackingLineCollection[0].ContainerLink = 1;
			if (containerFullMatch)
			{
				subShipment2.PackingLineCollection[1].ContainerLink = 1;
			}

			subShipment2.SetContainerCollection(() => new DataObjectList<Container>()
			{
				new() { Link = 1, ContainerNumber = "CNT-1", ContainerType = DefaultContainerType },
			});
			gateBookingShipment.SetSubShipmentCollection(() => new DataObjectList<Shipment>() { subShipment2 });
			Logger.TopLevelDataObject = gateBookingShipment;

			if (referenceNumberType == ReferenceNumberTypes.ContainerNumber)
			{
				AssertExceptionThrown<DataObjectReadFailureException>("Blind Container matching with Container Number is not supported.",
				@"Could not find any package state using Container Number, House Bill and Master Bill.", () => new WhsTransitDispatchConsolDataObjectReader(gateBookingShipment, Logger, Factory).ReadIntoBusinessObject());
			}
			else
			{
				var newFactory = new UniversalObjectFactory();
				new WhsTransitDispatchConsolDataObjectReader(gateBookingShipment, Logger, newFactory).ReadIntoBusinessObject();
				newFactory.SaveForTesting();

				var dllCount = containerFullMatch ? 2 : 1;

				if (hasConsol)
				{
					dllCount++;
				}

				(dlls, dcns, pivots, dtus) = AssertEntitiesCount(dllCount: dllCount, dcnCount: 1, dllDtuPivotCount: 2, dtuCount: 2, universalObjectFactory: newFactory);
				var container1Dtu = dtus.FirstOrDefault(dtu => dtu.WDH_VehicleReference == "CNT-1");
				var container1Pivot = pivots.FirstOrDefault(pivot => pivot.WLD_WDH_TransitDispatchTransportationUnit == container1Dtu.PK);
				var dtu1PkgState = GetDtuPackageState(container1Dtu.PK);
				var vehicle1Dtu = dtus.FirstOrDefault(dtu => dtu.WDH_VehicleReference == "DEF-023");
				var vehicle1Pivot = pivots.FirstOrDefault(pivot => pivot.WLD_WDH_TransitDispatchTransportationUnit == vehicle1Dtu.PK);
				var vehicle1Dll = dlls.FirstOrDefault(dll => dll.PK == vehicle1Pivot.WLD_WDL_TransitDispatchLoadList);

				if (containerFullMatch)
				{
					var container1Dll = dlls.FirstOrDefault(dll => dll.PK == container1Pivot.WLD_WDL_TransitDispatchLoadList);
					Assert("Container DTU and DLL should be linked.", container1Pivot.WLD_WDH_TransitDispatchTransportationUnit == container1Dtu.PK && container1Pivot.WLD_WDL_TransitDispatchLoadList == container1Dll.PK);
					AssertEquals("Container DLL should be active.", true, container1Dll.WDL_IsActive);
					AssertPackageStatesUnderTheDCNsShouldLinkToASpecificDll(new[] { dcns[0].PK }, container1Dll.PK, newFactory);
					Assert("Vehicle DTU and DLL should be linked.", vehicle1Pivot.WLD_WDH_TransitDispatchTransportationUnit == vehicle1Dtu.PK && vehicle1Pivot.WLD_WDL_TransitDispatchLoadList == vehicle1Dll.PK);
					AssertEquals("Driver should be populated.", "DRIVER1", vehicle1Dtu.WDH_SignedBy);
					AssertEquals("Vehicle DLL should be active.", true, vehicle1Dll.WDL_IsActive);
					AssertEquals("Container DTU's PackageState should link to the vehicle DLL.", dtu1PkgState.WPS_WDL_LoadList, vehicle1Dll.PK);
				}
				else
				{
					var emptyDll = dlls.FirstOrDefault(dll => dll.PK != vehicle1Dll.PK);

					Assert("Vehicle DTU and DLL should be linked.", vehicle1Pivot.WLD_WDH_TransitDispatchTransportationUnit == vehicle1Dtu.PK && vehicle1Pivot.WLD_WDL_TransitDispatchLoadList == vehicle1Dll.PK);
					AssertEquals("Vehicle Registration Number should be populated.", "DEF-023", vehicle1Dtu.WDH_VehicleReference);
					AssertEquals("Driver should be populated.", "DRIVER1", vehicle1Dtu.WDH_SignedBy);
					AssertEquals("Vehicle DLL should be active.", true, vehicle1Dll.WDL_IsActive);
					Assert("Container DTU and Vehicle DLL should be linked.", container1Pivot.WLD_WDH_TransitDispatchTransportationUnit == container1Dtu.PK && container1Pivot.WLD_WDL_TransitDispatchLoadList == vehicle1Dll.PK);
					AssertPackageStatesUnderTheDCNsShouldLinkToASpecificDll(new[] { dcns[0].PK }, vehicle1Dll.PK, newFactory);
					var container1DtuPkgState = GetDtuPackageState(container1Dtu.PK);
					AssertNull("No package state should be created for the container", container1DtuPkgState);
				}

				if (hasConsol)
				{
					var emptyDll = dlls.FirstOrDefault(dll => dll.WDL_ReferenceNumber == "C123");
					AssertEquals("Empty DLL should be made inactive.", false, emptyDll.WDL_IsActive);
				}

				var links = Factory.Load<StmUniversalJobLink>(new ZQuery());
				AssertUniversalJobLink(links, null, vehicle1Dtu.PK, WhsItemDispatchTransportationUnitSchema.Constants.Prefix, DataContextType.GateBooking, "GTB001", "EDI", "DAT", "EDI");
				AssertUniversalJobLink(links, null, container1Dtu.PK, WhsItemDispatchTransportationUnitSchema.Constants.Prefix, DataContextType.GateMovementBooking, "GMB001", "EDI", "DAT", "EDI");
				AssertUniversalJobLinkShouldBeUnique(links, DataContextType.GateBooking, "GTB001");
				AssertUniversalJobLinkShouldBeUnique(links, DataContextType.GateMovementBooking, "GMB001");
				AssertGateRelatedBookingConfirmedLog(vehicle1Dtu, nameof(DataContextType.GateBooking), expectedLogReference: $"{vehicle1Dtu.WDH_ReferenceNumber}|FAC=WHS|LOC=ThereVille|MST=GateBooking|RFN=GTB001|TYP=WhsItemDispatchTransportationUnit|WHS=TW2");
				AssertGateRelatedBookingConfirmedLog(container1Dtu, nameof(DataContextType.GateMovementBooking), expectedLogReference: $"{container1Dtu.WDH_ReferenceNumber}|FAC=WHS|LOC=ThereVille|MST=GateMovementBooking|RFN=GMB001|TYP=WhsItemDispatchTransportationUnit|WHS=TW2");
			}
		}

		public void TestGateBookingWithNoMatchingContainers_BookingReferenceIsMasterBill()
		{
			TestGateBookingWithNoMatchingContainersCore(ReferenceNumberTypes.MasterBill);
		}

		public void TestGateBookingWithNoMatchingContainers_BookingReferenceIsHouseBill()
		{
			TestGateBookingWithNoMatchingContainersCore(ReferenceNumberTypes.HouseBill);
		}

		public void TestGateBookingWithNoMatchingContainers_BookingReferenceIsContainerNumber()
		{
			TestGateBookingWithNoMatchingContainersCore(ReferenceNumberTypes.ContainerNumber);
		}

		public void TestGateBookingWithNoMatchingContainersCore(ReferenceNumberTypes referenceNumberType)
		{
			// Prepare data
			Data.SetupForForwardingImport();
			var consol = new Shipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			consol.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.ATW, ServiceCode = ServiceCodeType.TWD } } });
			consol.DataContext.AddDataSource(DataContextType.ForwardingConsol, "C123");
			consol.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { Data.Orgs.Consignor_CRAHOLSYD, Data.Orgs.Consignee_INTHEMSYD, Data.Orgs.Warehouse_WUFSHIJNB, Data.Orgs.Warehouse_INTHEMSYD });
			consol.SetSubShipmentCollection(() => new DataObjectList<Shipment>());
			consol.WayBillNumber = "WAYBILLNO1";
			consol.SetContainerCollection(() => new DataObjectList<Container>()
			{
				new() { Link = 1, ContainerNumber = "CNT-1", ContainerType = DefaultContainerType }
			});

			var subShipment = Data.CreateShipmentWithPackages("WAYBILLNO2", new string[] { "PKG1", "PKG2" });
			subShipment.PackingLineCollection[0].ContainerLink = 1;
			subShipment.PackingLineCollection[1].ContainerLink = 1;
			consol.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S123");
			consol.SubShipmentCollection.Add(subShipment);
			Logger.TopLevelDataObject = consol;

			// Prepare receive consignments and packages
			var receiveConsol = Data.CreateReceiveConsolAndConsignmentsInDB(consol);
			AssertEquals("Precondition - One consignment is created.", 1, receiveConsol.PopulatedConsignmentsForTesting.Length);

			new WhsTransitDispatchConsolDataObjectReader(consol, Logger, Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			(WhsItemDispatchLoadList[] dlls, WhsItemDispatchConsignment[] dcns, WhsItemDispatchLoadListDTUPivot[] pivots, WhsItemDispatchTransportationUnit[] dtus) =
				AssertEntitiesCount(dllCount: 1, dcnCount: 1, dllDtuPivotCount: 1, dtuCount: 1);
			var originalDll = dlls[0];
			var originalDtu = dtus[0];
			var originalPivot = pivots[0];
			AssertPackageStatesUnderTheDCNsShouldLinkToASpecificDll(new[] { dcns[0].PK }, originalDll.PK);
			AssertNoExceptionThrown("Should not throw errors during the saving.", () => Factory.SaveForTesting());

			// Prepare first gate booking shipment
			var gateBookingShipment = Data.HeaderDataObjectForGateBooking_ATW;
			var gateSubShipment = Data.CreateSubShipmentForGateBookingHeaderObject("GMB001", referenceNumberType == ReferenceNumberTypes.ContainerNumber ? new ZString("CNT-2") : referenceNumberType == ReferenceNumberTypes.HouseBill ? subShipment.WayBillNumber.Value : consol.WayBillNumber.Value, isPickup: false);
			gateSubShipment.SetContainerCollection(() => new DataObjectList<Container>()
			{
				new() { Link = 1, ContainerNumber = "CNT-2", ContainerType = DefaultContainerType },
			});
			gateBookingShipment.SetSubShipmentCollection(() => new DataObjectList<Shipment>() { gateSubShipment });
			Logger.TopLevelDataObject = gateBookingShipment;

			var reader = new WhsTransitDispatchConsolDataObjectReader(gateBookingShipment, Logger, Factory);

			if (referenceNumberType == ReferenceNumberTypes.MasterBill || referenceNumberType == ReferenceNumberTypes.HouseBill)
			{
				AssertExceptionThrown<DataObjectReadFailureException>("Exception should throw as there is no matching container in TWH.",
				"Gate Booking has a container and failed to find a matching Dispatch Load List with the same container.",
				() => reader.ReadIntoBusinessObject());
			}
			else if (referenceNumberType == ReferenceNumberTypes.ContainerNumber)
			{
				AssertExceptionThrown<DataObjectReadFailureException>("Exception should throw as there is no matching container in TWH with the Booking Conifrmation Reference.",
					@"Could not find any package state using Container Number, House Bill and Master Bill.", () => reader.ReadIntoBusinessObject());
			}
		}

		public void TestGateBookingsWithMatchingContainers()
		{
			// Prepare data
			Data.SetupForForwardingImport();
			var consol = new Shipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			consol.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.ATW, ServiceCode = ServiceCodeType.TWD } } });
			consol.DataContext.AddDataSource(DataContextType.ForwardingConsol, "C123");
			consol.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { Data.Orgs.Consignor_CRAHOLSYD, Data.Orgs.Consignee_INTHEMSYD, Data.Orgs.Warehouse_WUFSHIJNB, Data.Orgs.Warehouse_INTHEMSYD });
			consol.SetSubShipmentCollection(() => new DataObjectList<Shipment>());
			consol.WayBillNumber = "WAYBILLNO1";
			consol.SetContainerCollection(() => new DataObjectList<Container>()
			{
				new() { Link = 1, ContainerNumber = "CNT-1", ContainerType = DefaultContainerType },
				new() { Link = 2, ContainerNumber = "CNT-2", ContainerType = DefaultContainerType }
			});

			var subShipment1 = Data.CreateShipmentWithPackages("WAYBILLNO2", new string[] { "PKG1", "PKG2", "PKG3" });
			subShipment1.PackingLineCollection[0].ContainerLink = 1;
			subShipment1.PackingLineCollection[1].ContainerLink = 1;
			subShipment1.PackingLineCollection[2].ContainerLink = 2;
			consol.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S123");
			consol.SubShipmentCollection.Add(subShipment1);
			Logger.TopLevelDataObject = consol;

			// Prepare receive consignments and packages
			var receiveConsol = Data.CreateReceiveConsolAndConsignmentsInDB(consol);
			AssertEquals("Precondition - One consignment is created.", 1, receiveConsol.PopulatedConsignmentsForTesting.Length);

			new WhsTransitDispatchConsolDataObjectReader(consol, Logger, Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			(WhsItemDispatchLoadList[] dlls, WhsItemDispatchConsignment[] dcns, WhsItemDispatchLoadListDTUPivot[] pivots, WhsItemDispatchTransportationUnit[] dtus) =
				AssertEntitiesCount(dllCount: 2, dcnCount: 1, dllDtuPivotCount: 2, dtuCount: 2);
			var originalDtu1 = dtus.FirstOrDefault(dtu => dtu.WDH_VehicleReference == "CNT-1");
			var originalDtu2 = dtus.FirstOrDefault(dtu => dtu.WDH_VehicleReference == "CNT-2");
			var originalPivot1 = pivots.FirstOrDefault(pivot => pivot.WLD_WDH_TransitDispatchTransportationUnit == originalDtu1.PK);
			var originalPivot2 = pivots.FirstOrDefault(pivot => pivot.WLD_WDH_TransitDispatchTransportationUnit == originalDtu2.PK);
			var originalDll1 = dlls.FirstOrDefault(dll => dll.PK == originalPivot1.WLD_WDL_TransitDispatchLoadList);
			var originalDll2 = dlls.FirstOrDefault(dll => dll.PK == originalPivot2.WLD_WDL_TransitDispatchLoadList);

			// Handle first gate booking shipment
			var gateBookingShipment = Data.HeaderDataObjectForGateBooking_ATW;
			var gateSubShipment = Data.CreateSubShipmentForGateBookingHeaderObject("GMB001", new ZString("CNT-1"), isPickup: false);
			gateSubShipment.SetContainerCollection(() => new DataObjectList<Container>()
			{
				new() { Link = 1, ContainerNumber = "CNT-1", ContainerType = DefaultContainerType },
			});
			gateBookingShipment.SetSubShipmentCollection(() => new DataObjectList<Shipment>() { gateSubShipment });
			Logger.TopLevelDataObject = gateBookingShipment;

			new WhsTransitDispatchConsolDataObjectReader(gateBookingShipment, Logger, Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			(dlls, dcns, pivots, dtus) = AssertEntitiesCount(dllCount: 3, dcnCount: 1, dllDtuPivotCount: 3, dtuCount: 3);
			var dtu1PkgState = GetDtuPackageState(originalDtu1.PK);
			var vehicle1Dtu = dtus.FirstOrDefault(dtu => dtu.PK != originalDtu1.PK && dtu.PK != originalDtu2.PK);
			var vehicle1Dll = dlls.FirstOrDefault(dll => dll.PK != originalDll1.PK && dll.PK != originalDll2.PK);
			var vehicle1Pivot = pivots.FirstOrDefault(pivot => pivot.PK != originalPivot1.PK && pivot.PK != originalPivot2.PK);
			Assert("Vehicle DTU and DLL should be linked.", vehicle1Pivot.WLD_WDH_TransitDispatchTransportationUnit == vehicle1Dtu.PK && vehicle1Pivot.WLD_WDL_TransitDispatchLoadList == vehicle1Dll.PK);
			AssertEquals("Vehicle Registration Number should be populated.", "DEF-023", vehicle1Dtu.WDH_VehicleReference);
			AssertEquals("Driver should be populated.", "DRIVER1", vehicle1Dtu.WDH_SignedBy);
			AssertEquals("Vehicle DLL should be active.", true, vehicle1Dll.WDL_IsActive);
			AssertEquals("Vehicle DTU's PackageState should link to the vehicle DLL.", dtu1PkgState.WPS_WDL_LoadList, vehicle1Dll.PK);
			var links = Factory.Load<StmUniversalJobLink>(new ZQuery());
			AssertUniversalJobLink(links, null, vehicle1Dtu.PK, WhsItemDispatchTransportationUnitSchema.Constants.Prefix, DataContextType.GateBooking, "GTB001", "EDI", "DAT", "EDI");
			AssertUniversalJobLink(links, null, originalDtu1.PK, WhsItemDispatchTransportationUnitSchema.Constants.Prefix, DataContextType.GateMovementBooking, "GMB001", "EDI", "DAT", "EDI");
			AssertGateRelatedBookingConfirmedLog(vehicle1Dtu, nameof(DataContextType.GateBooking), expectedLogReference: $"{vehicle1Dtu.WDH_ReferenceNumber}|FAC=WHS|LOC=ThereVille|MST=GateBooking|RFN=GTB001|TYP=WhsItemDispatchTransportationUnit|WHS=TW2");
			AssertGateRelatedBookingConfirmedLog(originalDtu1, nameof(DataContextType.GateMovementBooking), expectedLogReference: $"{originalDtu1.WDH_ReferenceNumber}|FAC=WHS|LOC=ThereVille|MST=GateMovementBooking|RFN=GMB001|TYP=WhsItemDispatchTransportationUnit|WHS=TW2");

			// Handle second gate booking shipment
			gateBookingShipment.DataContext.DataSourceCollection.FirstOrDefault(ds => ds.Type.Value == new ZString(nameof(DataContextType.GateBooking))).Key = "GTB002";
			gateBookingShipment.VehicleRun.CrewCollection.FirstOrDefault().FullName = "DRIVER2";
			gateBookingShipment.PreCarriageShipmentCollection.FirstOrDefault().VehicleRun.Vehicle.Registration.Number = "ABC-123";
			var gateSubShipment2 = Data.CreateSubShipmentForGateBookingHeaderObject("GMB002", new ZString("CNT-2"), isPickup: false);
			gateSubShipment2.SetContainerCollection(() => new DataObjectList<Container>()
			{
				new() { Link = 2, ContainerNumber = "CNT-2", ContainerType = DefaultContainerType },
			});
			gateBookingShipment.SetSubShipmentCollection(() => new DataObjectList<Shipment>() { gateSubShipment2 });
			Logger.TopLevelDataObject = gateBookingShipment;

			new WhsTransitDispatchConsolDataObjectReader(gateBookingShipment, Logger, Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			(dlls, dcns, pivots, dtus) = AssertEntitiesCount(dllCount: 4, dcnCount: 1, dllDtuPivotCount: 4, dtuCount: 4);
			var dtu2PkgState = GetDtuPackageState(originalDtu2.PK);
			var vehicle2Dtu = dtus.FirstOrDefault(dtu => dtu.PK != originalDtu1.PK && dtu.PK != originalDtu2.PK && dtu.PK != vehicle1Dtu.PK);
			var vehicle2Dll = dlls.FirstOrDefault(dll => dll.PK != originalDll1.PK && dll.PK != originalDll2.PK && dll.PK != vehicle1Dll.PK);
			var vehicle2Pivot = pivots.FirstOrDefault(pivot => pivot.PK != originalPivot1.PK && pivot.PK != originalPivot2.PK && pivot.PK != vehicle1Pivot.PK);
			Assert("Vehicle DTU and DLL should be linked.", vehicle2Pivot.WLD_WDH_TransitDispatchTransportationUnit == vehicle2Dtu.PK && vehicle2Pivot.WLD_WDL_TransitDispatchLoadList == vehicle2Dll.PK);
			AssertEquals("Vehicle Registration Number should be populated.", "ABC-123", vehicle2Dtu.WDH_VehicleReference);
			AssertEquals("Driver should be populated.", "DRIVER2", vehicle2Dtu.WDH_SignedBy);
			AssertEquals("Vehicle DLL should be active.", true, vehicle2Dll.WDL_IsActive);
			AssertEquals("Vehicle DTU's PackageState should link to the vehicle DLL.", dtu2PkgState.WPS_WDL_LoadList, vehicle2Dll.PK);
			links = Factory.Load<StmUniversalJobLink>(new ZQuery());
			AssertUniversalJobLink(links, null, vehicle2Dtu.PK, WhsItemDispatchTransportationUnitSchema.Constants.Prefix, DataContextType.GateBooking, "GTB002", "EDI", "DAT", "EDI");
			AssertUniversalJobLink(links, null, originalDtu2.PK, WhsItemDispatchTransportationUnitSchema.Constants.Prefix, DataContextType.GateMovementBooking, "GMB002", "EDI", "DAT", "EDI");
			AssertUniversalJobLinkShouldBeUnique(links, DataContextType.GateBooking, "GTB001");
			AssertUniversalJobLinkShouldBeUnique(links, DataContextType.GateBooking, "GTB002");
			AssertUniversalJobLinkShouldBeUnique(links, DataContextType.GateMovementBooking, "GMB001");
			AssertUniversalJobLinkShouldBeUnique(links, DataContextType.GateMovementBooking, "GMB002");
			AssertGateRelatedBookingConfirmedLog(vehicle2Dtu, nameof(DataContextType.GateBooking), expectedLogReference: $"{vehicle2Dtu.WDH_ReferenceNumber}|FAC=WHS|LOC=ThereVille|MST=GateBooking|RFN=GTB002|TYP=WhsItemDispatchTransportationUnit|WHS=TW2");
			AssertGateRelatedBookingConfirmedLog(originalDtu2, nameof(DataContextType.GateMovementBooking), expectedLogReference: $"{originalDtu2.WDH_ReferenceNumber}|FAC=WHS|LOC=ThereVille|MST=GateMovementBooking|RFN=GMB002|TYP=WhsItemDispatchTransportationUnit|WHS=TW2");
		}

		public void TestGateBookingsWithMatchingContainers_SameVehicle()
		{
			// Prepare data
			Data.SetupForForwardingImport();
			var consol = new Shipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			consol.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.ATW, ServiceCode = ServiceCodeType.TWD } } });
			consol.DataContext.AddDataSource(DataContextType.ForwardingConsol, "C123");
			consol.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { Data.Orgs.Consignor_CRAHOLSYD, Data.Orgs.Consignee_INTHEMSYD, Data.Orgs.Warehouse_WUFSHIJNB, Data.Orgs.Warehouse_INTHEMSYD });
			consol.SetSubShipmentCollection(() => new DataObjectList<Shipment>());
			consol.WayBillNumber = "WAYBILLNO1";
			consol.SetContainerCollection(() => new DataObjectList<Container>()
			{
				new() { Link = 1, ContainerNumber = "CNT-1", ContainerType = DefaultContainerType },
				new() { Link = 2, ContainerNumber = "CNT-2", ContainerType = DefaultContainerType }
			});

			var subShipment1 = Data.CreateShipmentWithPackages("WAYBILLNO2", new string[] { "PKG1", "PKG2", "PKG3" });
			subShipment1.PackingLineCollection[0].ContainerLink = 1;
			subShipment1.PackingLineCollection[1].ContainerLink = 1;
			subShipment1.PackingLineCollection[2].ContainerLink = 2;
			consol.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S123");
			consol.SubShipmentCollection.Add(subShipment1);
			Logger.TopLevelDataObject = consol;

			// Prepare receive consignments and packages
			var receiveConsol = Data.CreateReceiveConsolAndConsignmentsInDB(consol);
			AssertEquals("Precondition - One consignment is created.", 1, receiveConsol.PopulatedConsignmentsForTesting.Length);

			new WhsTransitDispatchConsolDataObjectReader(consol, Logger, Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			(WhsItemDispatchLoadList[] dlls, WhsItemDispatchConsignment[] dcns, WhsItemDispatchLoadListDTUPivot[] pivots, WhsItemDispatchTransportationUnit[] dtus) =
				AssertEntitiesCount(dllCount: 2, dcnCount: 1, dllDtuPivotCount: 2, dtuCount: 2);
			var originalDtu1 = dtus.FirstOrDefault(dtu => dtu.WDH_VehicleReference == "CNT-1");
			var originalDtu2 = dtus.FirstOrDefault(dtu => dtu.WDH_VehicleReference == "CNT-2");
			var originalPivot1 = pivots.FirstOrDefault(pivot => pivot.WLD_WDH_TransitDispatchTransportationUnit == originalDtu1.PK);
			var originalPivot2 = pivots.FirstOrDefault(pivot => pivot.WLD_WDH_TransitDispatchTransportationUnit == originalDtu2.PK);
			var originalDll1 = dlls.FirstOrDefault(dll => dll.PK == originalPivot1.WLD_WDL_TransitDispatchLoadList);
			var originalDll2 = dlls.FirstOrDefault(dll => dll.PK == originalPivot2.WLD_WDL_TransitDispatchLoadList);

			// Handle first gate booking shipment
			var gateBookingShipment = Data.HeaderDataObjectForGateBooking_ATW;
			var gateSubShipment = Data.CreateSubShipmentForGateBookingHeaderObject("GMB001", new ZString("CNT-1"), isPickup: false);
			gateSubShipment.SetContainerCollection(() => new DataObjectList<Container>()
			{
				new() { Link = 1, ContainerNumber = "CNT-1", ContainerType = DefaultContainerType },
			});
			gateBookingShipment.SetSubShipmentCollection(() => new DataObjectList<Shipment>() { gateSubShipment });
			Logger.TopLevelDataObject = gateBookingShipment;

			new WhsTransitDispatchConsolDataObjectReader(gateBookingShipment, Logger, Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			(dlls, dcns, pivots, dtus) = AssertEntitiesCount(dllCount: 3, dcnCount: 1, dllDtuPivotCount: 3, dtuCount: 3);
			var dtu1PkgState = GetDtuPackageState(originalDtu1.PK);
			var vehicleDtu = dtus.FirstOrDefault(dtu => dtu.PK != originalDtu1.PK && dtu.PK != originalDtu2.PK);
			var vehicleDll = dlls.FirstOrDefault(dll => dll.PK != originalDll1.PK && dll.PK != originalDll2.PK);
			var vehiclePivot = pivots.FirstOrDefault(pivot => pivot.PK != originalPivot1.PK && pivot.PK != originalPivot2.PK);
			Assert("Vehicle DTU and DLL should be linked.", vehiclePivot.WLD_WDH_TransitDispatchTransportationUnit == vehicleDtu.PK && vehiclePivot.WLD_WDL_TransitDispatchLoadList == vehicleDll.PK);
			AssertEquals("Vehicle Registration Number should be populated.", "DEF-023", vehicleDtu.WDH_VehicleReference);
			AssertEquals("Driver should be populated.", "DRIVER1", vehicleDtu.WDH_SignedBy);
			AssertEquals("Vehicle DLL should be active.", true, vehicleDll.WDL_IsActive);
			AssertEquals("Vehicle DTU's PackageState should link to the vehicle DLL.", dtu1PkgState.WPS_WDL_LoadList, vehicleDll.PK);
			var links = Factory.Load<StmUniversalJobLink>(new ZQuery());
			AssertUniversalJobLink(links, null, vehicleDtu.PK, WhsItemDispatchTransportationUnitSchema.Constants.Prefix, DataContextType.GateBooking, "GTB001", "EDI", "DAT", "EDI");
			AssertUniversalJobLink(links, null, originalDtu1.PK, WhsItemDispatchTransportationUnitSchema.Constants.Prefix, DataContextType.GateMovementBooking, "GMB001", "EDI", "DAT", "EDI");

			// Handle second gate booking shipment
			var gateSubShipment2 = Data.CreateSubShipmentForGateBookingHeaderObject("GMB002", new ZString("CNT-2"), isPickup: false);
			gateSubShipment2.SetContainerCollection(() => new DataObjectList<Container>()
			{
				new() { Link = 2, ContainerNumber = "CNT-2", ContainerType = DefaultContainerType },
			});
			gateBookingShipment.SetSubShipmentCollection(() => new DataObjectList<Shipment>() { gateSubShipment2 });
			Logger.TopLevelDataObject = gateBookingShipment;

			new WhsTransitDispatchConsolDataObjectReader(gateBookingShipment, Logger, Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			(dlls, dcns, pivots, dtus) = AssertEntitiesCount(dllCount: 3, dcnCount: 1, dllDtuPivotCount: 3, dtuCount: 3);
			var dtu2PkgState = GetDtuPackageState(originalDtu2.PK);
			vehicleDtu = dtus.FirstOrDefault(rtu => rtu.PK == vehicleDtu.PK);
			vehicleDll = dlls.FirstOrDefault(rtu => rtu.PK == vehicleDll.PK);
			vehiclePivot = pivots.FirstOrDefault(pivot => pivot.PK == vehiclePivot.PK);
			Assert("Vehicle DTU and DLL should be linked.", vehiclePivot.WLD_WDH_TransitDispatchTransportationUnit == vehicleDtu.PK && vehiclePivot.WLD_WDL_TransitDispatchLoadList == vehicleDll.PK);
			AssertEquals("Vehicle Registration Number should be populated.", "DEF-023", vehicleDtu.WDH_VehicleReference);
			AssertEquals("Driver should be populated.", "DRIVER1", vehicleDtu.WDH_SignedBy);
			AssertEquals("Vehicle DLL should be active.", true, vehicleDll.WDL_IsActive);
			AssertEquals("Vehicle DTU's PackageState should link to the vehicle DLL.", dtu2PkgState.WPS_WDL_LoadList, vehicleDll.PK);
			links = Factory.Load<StmUniversalJobLink>(new ZQuery());
			AssertUniversalJobLink(links, null, vehicleDtu.PK, WhsItemDispatchTransportationUnitSchema.Constants.Prefix, DataContextType.GateBooking, "GTB001", "EDI", "DAT", "EDI");
			AssertUniversalJobLink(links, null, originalDtu1.PK, WhsItemDispatchTransportationUnitSchema.Constants.Prefix, DataContextType.GateMovementBooking, "GMB001", "EDI", "DAT", "EDI");
			AssertUniversalJobLink(links, null, originalDtu2.PK, WhsItemDispatchTransportationUnitSchema.Constants.Prefix, DataContextType.GateMovementBooking, "GMB002", "EDI", "DAT", "EDI");
			AssertUniversalJobLinkShouldBeUnique(links, DataContextType.GateBooking, "GTB001");
			AssertUniversalJobLinkShouldBeUnique(links, DataContextType.GateMovementBooking, "GMB001");
			AssertUniversalJobLinkShouldBeUnique(links, DataContextType.GateMovementBooking, "GMB002");
		}

		public void TestGateBookingWithOneMatchingContainer()
		{
			// Prepare data
			Data.SetupForForwardingImport();
			var consol = new Shipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			consol.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.ATW, ServiceCode = ServiceCodeType.TWD } } });
			consol.DataContext.AddDataSource(DataContextType.ForwardingConsol, "C123");
			consol.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { Data.Orgs.Consignor_CRAHOLSYD, Data.Orgs.Consignee_INTHEMSYD, Data.Orgs.Warehouse_WUFSHIJNB, Data.Orgs.Warehouse_INTHEMSYD });
			consol.SetSubShipmentCollection(() => new DataObjectList<Shipment>());
			consol.WayBillNumber = "WAYBILLNO1";
			consol.SetContainerCollection(() => new DataObjectList<Container>()
			{
				new() { Link = 1, ContainerNumber = "CNT-1", ContainerType = DefaultContainerType }
			});

			var subShipment1 = Data.CreateShipmentWithPackages("WAYBILLNO2", new string[] { "PKG1", "PKG2" });
			subShipment1.PackingLineCollection[0].ContainerLink = 1;
			subShipment1.PackingLineCollection[1].ContainerLink = 1;
			consol.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S123");
			consol.SubShipmentCollection.Add(subShipment1);
			Logger.TopLevelDataObject = consol;

			// Prepare receive consignments and packages
			var receiveConsol = Data.CreateReceiveConsolAndConsignmentsInDB(consol);
			AssertEquals("Precondition - One consignment is created.", 1, receiveConsol.PopulatedConsignmentsForTesting.Length);

			new WhsTransitDispatchConsolDataObjectReader(consol, Logger, Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			(WhsItemDispatchLoadList[] dlls, WhsItemDispatchConsignment[] dcns, WhsItemDispatchLoadListDTUPivot[] pivots, WhsItemDispatchTransportationUnit[] dtus) =
				AssertEntitiesCount(dllCount: 1, dcnCount: 1, dllDtuPivotCount: 1, dtuCount: 1);
			var originalDll = dlls[0];
			var originalDtu = dtus[0];
			var originalPivot = pivots[0];
			AssertPackageStatesUnderTheDCNsShouldLinkToASpecificDll(new[] { dcns[0].PK }, originalDll.PK);
			AssertNoExceptionThrown("Should not throw errors during the saving.", () => Factory.SaveForTesting());

			// Prepare first gate booking shipment
			var gateBookingShipment = Data.HeaderDataObjectForGateBooking_ATW;
			var gateSubShipment = Data.CreateSubShipmentForGateBookingHeaderObject("GMB001", new ZString("CNT-1"), isPickup: false);
			gateSubShipment.SetContainerCollection(() => new DataObjectList<Container>()
			{
				new() { Link = 1, ContainerNumber = "CNT-1", ContainerType = DefaultContainerType },
			});
			gateBookingShipment.SetSubShipmentCollection(() => new DataObjectList<Shipment>() { gateSubShipment });
			Logger.TopLevelDataObject = gateBookingShipment;

			new WhsTransitDispatchConsolDataObjectReader(gateBookingShipment, Logger, Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			(dlls, dcns, pivots, dtus) = AssertEntitiesCount(dllCount: 2, dcnCount: 1, dllDtuPivotCount: 2, dtuCount: 2);

			var containerDtuPkgState = GetDtuPackageState(originalDtu.PK);
			var vehicleDtu = dtus.FirstOrDefault(dtu => dtu.PK != originalDtu.PK);
			var vehicleDll = dlls.FirstOrDefault(dll => dll.PK != originalDll.PK);
			var vehiclePivot = pivots.FirstOrDefault(pivot => pivot.PK != originalPivot.PK);
			Assert("Vehicle DTU and DLL should be linked.", vehiclePivot.WLD_WDH_TransitDispatchTransportationUnit == vehicleDtu.PK && vehiclePivot.WLD_WDL_TransitDispatchLoadList == vehicleDll.PK);
			AssertEquals("Vehicle Registration Number should be populated.", "DEF-023", vehicleDtu.WDH_VehicleReference);
			AssertEquals("Driver should be populated.", "DRIVER1", vehicleDtu.WDH_SignedBy);
			AssertEquals("Vehicle DLL should be active.", true, vehicleDll.WDL_IsActive);
			AssertEquals("Container DTU's PackageState should link to the vehicle DLL.", containerDtuPkgState.WPS_WDL_LoadList, vehicleDll.PK);
			var links = Factory.Load<StmUniversalJobLink>(new ZQuery());
			AssertUniversalJobLink(links, null, vehicleDtu.PK, WhsItemDispatchTransportationUnitSchema.Constants.Prefix, DataContextType.GateBooking, "GTB001", "EDI", "DAT", "EDI");
			AssertUniversalJobLink(links, null, originalDtu.PK, WhsItemDispatchTransportationUnitSchema.Constants.Prefix, DataContextType.GateMovementBooking, "GMB001", "EDI", "DAT", "EDI");
			AssertGateRelatedBookingConfirmedLog(vehicleDtu, nameof(DataContextType.GateBooking), expectedLogReference: $"{vehicleDtu.WDH_ReferenceNumber}|FAC=WHS|LOC=ThereVille|MST=GateBooking|RFN=GTB001|TYP=WhsItemDispatchTransportationUnit|WHS=TW2");
			AssertGateRelatedBookingConfirmedLog(originalDtu, nameof(DataContextType.GateMovementBooking), expectedLogReference: $"{originalDtu.WDH_ReferenceNumber}|FAC=WHS|LOC=ThereVille|MST=GateMovementBooking|RFN=GMB001|TYP=WhsItemDispatchTransportationUnit|WHS=TW2");

			// Prepare the second gate booking with the same container: CNT-1, updated vehicle and driver
			gateBookingShipment.VehicleRun.CrewCollection.FirstOrDefault().FullName = "DRIVER2";
			gateBookingShipment.PreCarriageShipmentCollection.FirstOrDefault().VehicleRun.Vehicle.Registration.Number = "ABC-123";
			Logger.TopLevelDataObject = gateBookingShipment;

			new WhsTransitDispatchConsolDataObjectReader(gateBookingShipment, Logger, Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			(dlls, dcns, pivots, dtus) = AssertEntitiesCount(dllCount: 3, dcnCount: 1, dllDtuPivotCount: 2, dtuCount: 2);
			var newVehicleDtu = dtus.FirstOrDefault(dtu => dtu.PK != originalDtu.PK);
			var newVehicleDll = dlls.FirstOrDefault(dll => dll.PK != originalDll.PK && dll.PK != vehicleDll.PK);
			var newVehiclePivot = pivots.FirstOrDefault(pivot => pivot.PK != originalPivot.PK);
			containerDtuPkgState = GetDtuPackageState(originalDtu.PK);
			vehicleDll = dlls.FirstOrDefault(dll => dll.PK == vehicleDll.PK);
			Assert("Vehicle DTU and DLL should be linked.", newVehiclePivot.WLD_WDH_TransitDispatchTransportationUnit == newVehicleDtu.PK && newVehiclePivot.WLD_WDL_TransitDispatchLoadList == newVehicleDll.PK);
			AssertEquals("Vehicle Registration Number should be populated.", "ABC-123", newVehicleDtu.WDH_VehicleReference);
			AssertEquals("Driver should be populated.", "DRIVER2", newVehicleDtu.WDH_SignedBy);
			AssertEquals("Original Vehicle DLL should be inactive.", false, vehicleDll.WDL_IsActive);
			AssertEquals("New Vehicle DLL should be active.", true, newVehicleDll.WDL_IsActive);
			AssertEquals("Container DTU's PackageState should link to the vehicle DLL.", containerDtuPkgState.WPS_WDL_LoadList, newVehicleDll.PK);
			links = Factory.Load<StmUniversalJobLink>(new ZQuery());
			AssertUniversalJobLink(links, null, newVehicleDtu.PK, WhsItemDispatchTransportationUnitSchema.Constants.Prefix, DataContextType.GateBooking, "GTB001", "EDI", "DAT", "EDI");
			AssertUniversalJobLink(links, null, originalDtu.PK, WhsItemDispatchTransportationUnitSchema.Constants.Prefix, DataContextType.GateMovementBooking, "GMB001", "EDI", "DAT", "EDI");
			AssertUniversalJobLinkShouldBeUnique(links, DataContextType.GateBooking, "GTB001");
			AssertUniversalJobLinkShouldBeUnique(links, DataContextType.GateMovementBooking, "GMB001");
			AssertGateRelatedBookingConfirmedLog(newVehicleDtu, nameof(DataContextType.GateBooking), expectedLogReference: $"{newVehicleDtu.WDH_ReferenceNumber}|FAC=WHS|LOC=ThereVille|MST=GateBooking|RFN=GTB001|TYP=WhsItemDispatchTransportationUnit|WHS=TW2");
			AssertGateRelatedBookingConfirmedLog(originalDtu, nameof(DataContextType.GateMovementBooking), expectedLogReference: $"{originalDtu.WDH_ReferenceNumber}|FAC=WHS|LOC=ThereVille|MST=GateMovementBooking|RFN=GMB001|TYP=WhsItemDispatchTransportationUnit|WHS=TW2", isLogUnique: false);
		}

		public void TestGateBookingWithContainer_DLLHavingDTUPivot_NoMatchingContainerFound()
		{
			// Prepare data
			Data.SetupForForwardingImport();
			var consol = new Shipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			consol.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.ATW, ServiceCode = ServiceCodeType.TWD } } });
			consol.DataContext.AddDataSource(DataContextType.ForwardingConsol, "C123");
			consol.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { Data.Orgs.Consignor_CRAHOLSYD, Data.Orgs.Consignee_INTHEMSYD, Data.Orgs.Warehouse_WUFSHIJNB, Data.Orgs.Warehouse_INTHEMSYD });
			consol.SetSubShipmentCollection(() => new DataObjectList<Shipment>());
			consol.WayBillNumber = "WAYBILLNO1";
			consol.SetContainerCollection(() => new DataObjectList<Container>()
			{
				new() { Link = 1, ContainerNumber = "CNT-1", ContainerType = DefaultContainerType }
			});

			var subShipment1 = Data.CreateShipmentWithPackages("WAYBILLNO2", new string[] { "PKG1", "PKG2" });
			subShipment1.PackingLineCollection[0].ContainerLink = 1;
			subShipment1.PackingLineCollection[1].ContainerLink = 1;
			consol.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S123");
			consol.SubShipmentCollection.Add(subShipment1);
			Logger.TopLevelDataObject = consol;

			// Prepare receive consignments and packages
			var receiveConsol = Data.CreateReceiveConsolAndConsignmentsInDB(consol);
			AssertEquals("Precondition - One consignment is created.", 1, receiveConsol.PopulatedConsignmentsForTesting.Length);

			new WhsTransitDispatchConsolDataObjectReader(consol, Logger, Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			(WhsItemDispatchLoadList[] dlls, WhsItemDispatchConsignment[] dcns, WhsItemDispatchLoadListDTUPivot[] pivots, WhsItemDispatchTransportationUnit[] dtus) =
				AssertEntitiesCount(dllCount: 1, dcnCount: 1, dllDtuPivotCount: 1, dtuCount: 1);
			var originalDll = dlls[0];
			var originalDtu = dtus[0];
			var originalPivot = pivots[0];
			AssertPackageStatesUnderTheDCNsShouldLinkToASpecificDll(new[] { dcns[0].PK }, originalDll.PK);
			AssertNoExceptionThrown("Should not throw errors during the saving.", () => Factory.SaveForTesting());

			// Prepare gate booking shipment
			var gateBookingShipment = Data.HeaderDataObjectForGateBooking_ATW;
			var gateSubShipment = Data.CreateSubShipmentForGateBookingHeaderObject("BRK001", consol.WayBillNumber.Value, isPickup: false);
			gateSubShipment.SetContainerCollection(() => new DataObjectList<Container>()
			{
				new() { Link = 1, ContainerNumber = "CNT-2", ContainerType = DefaultContainerType },
			});
			gateBookingShipment.SetSubShipmentCollection(() => new DataObjectList<Shipment>() { gateSubShipment });
			Logger.TopLevelDataObject = gateBookingShipment;

			var reader = new WhsTransitDispatchConsolDataObjectReader(gateBookingShipment, Logger, Factory);
			AssertExceptionThrown<DataObjectReadFailureException>("Exception should throw as there is no matching container in TWH.",
				"Gate Booking has a container and failed to find a matching Dispatch Load List with the same container.",
				() => reader.ReadIntoBusinessObject());
		}

		public void TestGateBookingWithContainer_DLLHasNoDTUPivot_FullMatchDTUWithContainer()
		{
			// Prepare data
			Data.SetupForForwardingImport();
			var consol = new Shipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			consol.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.ATW, ServiceCode = ServiceCodeType.TWD } } });
			consol.DataContext.AddDataSource(DataContextType.ForwardingConsol, "C123");
			consol.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { Data.Orgs.Consignor_CRAHOLSYD, Data.Orgs.Consignee_INTHEMSYD, Data.Orgs.Warehouse_WUFSHIJNB, Data.Orgs.Warehouse_INTHEMSYD });
			consol.SetSubShipmentCollection(() => new DataObjectList<Shipment>());
			consol.WayBillNumber = "WAYBILLNO1";
			consol.SetContainerCollection(() => new DataObjectList<Container>()
			{
				new() { Link = 1, ContainerNumber = "CNT-1", ContainerType = DefaultContainerType }
			});

			var subShipment1 = Data.CreateShipmentWithPackages("WAYBILLNO2", new string[] { "PKG1", "PKG2" });
			subShipment1.PackingLineCollection[0].ContainerLink = 1;
			subShipment1.PackingLineCollection[1].ContainerLink = 1;
			consol.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S123");
			consol.SubShipmentCollection.Add(subShipment1);
			Logger.TopLevelDataObject = consol;

			// Prepare receive consignments and packages
			var receiveConsol = Data.CreateReceiveConsolAndConsignmentsInDB(consol);
			AssertEquals("Precondition - One consignment is created.", 1, receiveConsol.PopulatedConsignmentsForTesting.Length);

			new WhsTransitDispatchConsolDataObjectReader(consol, Logger, Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			(WhsItemDispatchLoadList[] dlls, WhsItemDispatchConsignment[] dcns, WhsItemDispatchLoadListDTUPivot[] pivots, WhsItemDispatchTransportationUnit[] dtus) =
				AssertEntitiesCount(dllCount: 1, dcnCount: 1, dllDtuPivotCount: 1, dtuCount: 1);
			var originalDll = dlls[0];
			var originalDtu = dtus[0];
			var originalPivot = pivots[0];
			AssertPackageStatesUnderTheDCNsShouldLinkToASpecificDll(new[] { dcns[0].PK }, originalDll.PK);
			AssertNoExceptionThrown("Should not throw errors during the saving.", () => Factory.SaveForTesting());

			// Prepare gate booking shipment
			var gateBookingShipment = Data.HeaderDataObjectForGateBooking_ATW;
			var gateSubShipment = Data.CreateSubShipmentForGateBookingHeaderObject("BRK001", consol.WayBillNumber.Value, isPickup: false, new ZString[] { "PKG1", "PKG2" });
			gateSubShipment.SetContainerCollection(() => new DataObjectList<Container>()
			{
				new() { Link = 1, ContainerNumber = "CNT-2", ContainerType = DefaultContainerType },
			});
			gateSubShipment.PackingLineCollection.First().ContainerLink = 1;
			gateSubShipment.PackingLineCollection.First().PackQty = 2;
			gateSubShipment.PackingLineCollection.Last().ContainerLink = 1;
			gateSubShipment.PackingLineCollection.First().PackQty = 4;

			gateBookingShipment.SetSubShipmentCollection(() => new DataObjectList<Shipment>() { gateSubShipment });
			Logger.TopLevelDataObject = gateBookingShipment;

			var reader = new WhsTransitDispatchConsolDataObjectReader(gateBookingShipment, Logger, Factory);
			AssertExceptionThrown<DataObjectReadFailureException>("Exception should throw as there is no matching container in TWH.",
				"Gate Booking has a container and failed to find a matching Dispatch Load List with the same container.",
				() => reader.ReadIntoBusinessObject());
		}

		public void TestGateBookingWithContainer_DLLHasNoDTUPivot_PartialMatchDTUWithContainer()
		{
			// Prepare data
			Data.SetupForForwardingImport();
			var consol = new Shipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			consol.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.ATW, ServiceCode = ServiceCodeType.TWD } } });
			consol.DataContext.AddDataSource(DataContextType.ForwardingConsol, "C123");
			consol.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { Data.Orgs.Consignor_CRAHOLSYD, Data.Orgs.Consignee_INTHEMSYD, Data.Orgs.Warehouse_WUFSHIJNB, Data.Orgs.Warehouse_INTHEMSYD });
			consol.SetSubShipmentCollection(() => new DataObjectList<Shipment>());
			consol.WayBillNumber = "WAYBILLNO1";
			consol.SetContainerCollection(() => new DataObjectList<Container>()
			{
				new() { Link = 1, ContainerNumber = "CNT-1", ContainerType = DefaultContainerType }
			});

			var subShipment1 = Data.CreateShipmentWithPackages("WAYBILLNO2", new string[] { "PKG1", "PKG2" });
			subShipment1.PackingLineCollection[0].ContainerLink = 1;
			subShipment1.PackingLineCollection[1].ContainerLink = 1;
			consol.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S123");
			consol.SubShipmentCollection.Add(subShipment1);
			Logger.TopLevelDataObject = consol;

			// Prepare receive consignments and packages
			var receiveConsol = Data.CreateReceiveConsolAndConsignmentsInDB(consol);
			AssertEquals("Precondition - One consignment is created.", 1, receiveConsol.PopulatedConsignmentsForTesting.Length);

			new WhsTransitDispatchConsolDataObjectReader(consol, Logger, Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			(WhsItemDispatchLoadList[] dlls, WhsItemDispatchConsignment[] dcns, WhsItemDispatchLoadListDTUPivot[] pivots, WhsItemDispatchTransportationUnit[] dtus) =
				AssertEntitiesCount(dllCount: 1, dcnCount: 1, dllDtuPivotCount: 1, dtuCount: 1);
			var originalDll = dlls[0];
			var originalDtu = dtus[0];
			var originalPivot = pivots[0];
			AssertPackageStatesUnderTheDCNsShouldLinkToASpecificDll(new[] { dcns[0].PK }, originalDll.PK);
			AssertNoExceptionThrown("Should not throw errors during the saving.", () => Factory.SaveForTesting());

			// Prepare gate booking shipment
			var gateBookingShipment = Data.HeaderDataObjectForGateBooking_ATW;
			var gateSubShipment = Data.CreateSubShipmentForGateBookingHeaderObject("BRK001", consol.WayBillNumber.Value, isPickup: false);
			gateSubShipment.SetContainerCollection(() => new DataObjectList<Container>()
			{
				new() { Link = 1, ContainerNumber = "CNT-2", ContainerType = DefaultContainerType },
			});
			gateBookingShipment.SetSubShipmentCollection(() => new DataObjectList<Shipment>() { gateSubShipment });
			Logger.TopLevelDataObject = gateBookingShipment;

			var reader = new WhsTransitDispatchConsolDataObjectReader(gateBookingShipment, Logger, Factory);
			AssertExceptionThrown<DataObjectReadFailureException>("Exception should throw as there is no matching container in TWH.",
				"Gate Booking has a container and failed to find a matching Dispatch Load List with the same container.",
				() => reader.ReadIntoBusinessObject());
		}

		public void TestGateBookingWithContainer_DLLHasNoDTUPivot_NoDTUMatchWithContainer()
		{
			// Prepare data
			Data.SetupForForwardingImport();
			var consol = new Shipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			consol.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.ATW, ServiceCode = ServiceCodeType.TWD } } });
			consol.DataContext.AddDataSource(DataContextType.ForwardingConsol, "C123");
			consol.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { Data.Orgs.Consignor_CRAHOLSYD, Data.Orgs.Consignee_INTHEMSYD, Data.Orgs.Warehouse_WUFSHIJNB, Data.Orgs.Warehouse_INTHEMSYD });
			consol.SetSubShipmentCollection(() => new DataObjectList<Shipment>());
			consol.WayBillNumber = "WAYBILLNO1";
			consol.SetContainerCollection(() => new DataObjectList<Container>()
			{
				new() { Link = 1, ContainerNumber = "CNT-1", ContainerType = DefaultContainerType }
			});

			var subShipment1 = Data.CreateShipmentWithPackages("WAYBILLNO2", new string[] { "PKG1", "PKG2" });
			subShipment1.PackingLineCollection[0].ContainerLink = 1;
			subShipment1.PackingLineCollection[1].ContainerLink = 1;
			consol.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S123");
			consol.SubShipmentCollection.Add(subShipment1);
			Logger.TopLevelDataObject = consol;

			// Prepare receive consignments and packages
			var receiveConsol = Data.CreateReceiveConsolAndConsignmentsInDB(consol);
			AssertEquals("Precondition - One consignment is created.", 1, receiveConsol.PopulatedConsignmentsForTesting.Length);

			new WhsTransitDispatchConsolDataObjectReader(consol, Logger, Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			(WhsItemDispatchLoadList[] dlls, WhsItemDispatchConsignment[] dcns, WhsItemDispatchLoadListDTUPivot[] pivots, WhsItemDispatchTransportationUnit[] dtus) =
				AssertEntitiesCount(dllCount: 1, dcnCount: 1, dllDtuPivotCount: 1, dtuCount: 1);
			var originalDll = dlls[0];
			var originalDtu = dtus[0];
			var originalPivot = pivots[0];
			AssertPackageStatesUnderTheDCNsShouldLinkToASpecificDll(new[] { dcns[0].PK }, originalDll.PK);
			AssertNoExceptionThrown("Should not throw errors during the saving.", () => Factory.SaveForTesting());

			// Prepare gate booking shipment
			var gateBookingShipment = Data.HeaderDataObjectForGateBooking_ATW;
			var gateSubShipment = Data.CreateSubShipmentForGateBookingHeaderObject("GMB001", consol.WayBillNumber.Value, isPickup: false);
			gateSubShipment.SetContainerCollection(() => new DataObjectList<Container>()
			{
				new() { Link = 1, ContainerNumber = "CNT-2", ContainerType = DefaultContainerType },
			});
			gateBookingShipment.SetSubShipmentCollection(() => new DataObjectList<Shipment>() { gateSubShipment });
			Logger.TopLevelDataObject = gateBookingShipment;

			var reader = new WhsTransitDispatchConsolDataObjectReader(gateBookingShipment, Logger, Factory);
			AssertExceptionThrown<DataObjectReadFailureException>("Exception should throw as there is no matching container in TWH.",
				"Gate Booking has a container and failed to find a matching Dispatch Load List with the same container.",
				() => reader.ReadIntoBusinessObject());
		}

		public void TestGateBookingWithNoValidSubShipment()
		{
			// Prepare data
			Data.SetupForForwardingImport();
			var consol = new Shipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			consol.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.ATW, ServiceCode = ServiceCodeType.TWD } } });
			consol.DataContext.AddDataSource(DataContextType.ForwardingConsol, "C123");
			consol.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { Data.Orgs.Consignor_CRAHOLSYD, Data.Orgs.Consignee_INTHEMSYD, Data.Orgs.Warehouse_WUFSHIJNB, Data.Orgs.Warehouse_INTHEMSYD });
			consol.SetSubShipmentCollection(() => new DataObjectList<Shipment>());
			consol.WayBillNumber = "WAYBILLNO1";
			consol.SetContainerCollection(() => new DataObjectList<Container>()
			{
				new() { Link = 1, ContainerNumber = "CNT-1", ContainerType = DefaultContainerType }
			});

			var subShipment1 = Data.CreateShipmentWithPackages("WAYBILLNO2", new string[] { "PKG1", "PKG2" });
			subShipment1.PackingLineCollection[0].ContainerLink = 1;
			subShipment1.PackingLineCollection[1].ContainerLink = 1;
			consol.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S123");
			consol.SubShipmentCollection.Add(subShipment1);
			Logger.TopLevelDataObject = consol;

			// Prepare receive consignments and packages
			var receiveConsol = Data.CreateReceiveConsolAndConsignmentsInDB(consol);
			AssertEquals("Precondition - One consignment is created.", 1, receiveConsol.PopulatedConsignmentsForTesting.Length);

			new WhsTransitDispatchConsolDataObjectReader(consol, Logger, Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			// Prepare gate booking shipment
			var gateBookingShipment = Data.HeaderDataObjectForGateBooking_ATW;
			var gateSubShipment = Data.CreateSubShipmentForGateBookingHeaderObject("GMB001", consol.WayBillNumber.Value, isPickup: false);
			gateSubShipment.SetContainerCollection(() => new DataObjectList<Container>()
			{
				new() { Link = 1, ContainerNumber = "CNT-2", ContainerType = DefaultContainerType },
			});
			// Make sub-shipment as an invalid one
			gateSubShipment.DataContext.ClearDataSourceCollection();
			gateBookingShipment.SetSubShipmentCollection(() => new DataObjectList<Shipment>() { gateSubShipment });
			Logger.TopLevelDataObject = gateBookingShipment;

			var reader = new WhsTransitDispatchConsolDataObjectReader(gateBookingShipment, Logger, Factory);
			AssertExceptionThrown<DataObjectReadFailureException>("Exception should throw as there is no valid Sub-shipment in UXML.",
							"Gate Movement Booking is not provided in UXML.",
							() => reader.ReadIntoBusinessObject());
		}

		public void TestGateBookingWithNoContainerButTheMatchingLoadListHasAContainer()
		{
			// Prepare data
			Data.SetupForForwardingImport();
			var consol = new Shipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			consol.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.ATW, ServiceCode = ServiceCodeType.TWD } } });
			consol.DataContext.AddDataSource(DataContextType.ForwardingConsol, "C123");
			consol.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { Data.Orgs.Consignor_CRAHOLSYD, Data.Orgs.Consignee_INTHEMSYD, Data.Orgs.Warehouse_WUFSHIJNB, Data.Orgs.Warehouse_INTHEMSYD });
			consol.SetSubShipmentCollection(() => new DataObjectList<Shipment>());
			consol.WayBillNumber = "WAYBILLNO1";
			consol.SetContainerCollection(() => new DataObjectList<Container>()
			{
				new() { Link = 1, ContainerNumber = "CNT-1", ContainerType = DefaultContainerType }
			});

			var subShipment1 = Data.CreateShipmentWithPackages("WAYBILLNO2", new string[] { "PKG1", "PKG2" });
			subShipment1.PackingLineCollection[0].ContainerLink = 1;
			subShipment1.PackingLineCollection[1].ContainerLink = 1;
			consol.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S123");
			consol.SubShipmentCollection.Add(subShipment1);
			Logger.TopLevelDataObject = consol;

			// Prepare receive consignments and packages
			var receiveConsol = Data.CreateReceiveConsolAndConsignmentsInDB(consol);
			AssertEquals("Precondition - One consignment is created.", 1, receiveConsol.PopulatedConsignmentsForTesting.Length);

			new WhsTransitDispatchConsolDataObjectReader(consol, Logger, Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			(WhsItemDispatchLoadList[] dlls, WhsItemDispatchConsignment[] dcns, WhsItemDispatchLoadListDTUPivot[] pivots, WhsItemDispatchTransportationUnit[] dtus) =
				AssertEntitiesCount(dllCount: 1, dcnCount: 1, dllDtuPivotCount: 1, dtuCount: 1);
			var originalDll = dlls[0];
			var originalDtu = dtus[0];
			var originalPivot = pivots[0];
			AssertPackageStatesUnderTheDCNsShouldLinkToASpecificDll(new[] { dcns[0].PK }, originalDll.PK);
			AssertNoExceptionThrown("Should not throw errors during the saving.", () => Factory.SaveForTesting());

			// Prepare gate booking shipment
			var gateBookingShipment = Data.HeaderDataObjectForGateBooking_ATW;
			var gateSubShipment = Data.CreateSubShipmentForGateBookingHeaderObject("GMB001", consol.WayBillNumber.Value, isPickup: false);
			gateBookingShipment.SetSubShipmentCollection(() => new DataObjectList<Shipment>() { gateSubShipment });
			Logger.TopLevelDataObject = gateBookingShipment;

			var reader = new WhsTransitDispatchConsolDataObjectReader(gateBookingShipment, Logger, Factory);
			AssertExceptionThrown<DataObjectReadFailureException>("Exception should throw as there is no matching container in TWH.",
				"Gate Booking has no container and failed to find a matching Dispatch Load List without a container.",
				() => reader.ReadIntoBusinessObject());
		}

		void AssertUniversalJobLinkShouldBeUnique(StmUniversalJobLink[] links, DataContextType dataContextType, ZString sourceKey, string parentTableCode = "WDH")
		{
			var filteredLinks = links.Where(link => link.UCL_SourceType == dataContextType.ToString() && link.UCL_SourceKey == sourceKey && link.UCL_ParentTableCode == parentTableCode);
			AssertEquals($"Universal Job Link with type:'{dataContextType.ToString()} and source key:'{sourceKey}' should be unique.", 1, filteredLinks.Count());
		}

		WhsItemPackageState GetDtuPackageState(ZGuid vehicleDtuPK)
		{
			var pkgExtensionQuery = new ZQuery(PkgPackageExtensionSchema.KPN_ParentID, vehicleDtuPK);
			pkgExtensionQuery.AddToFilter(PkgPackageExtensionSchema.KPN_ParentTableCode, WhsItemDispatchTransportationUnitSchema.Constants.Prefix);
			var pkgExtensions = Factory.Load<PkgPackageExtension>(pkgExtensionQuery);
			var pkgStateQuery = new ZQuery(WhsItemPackageStateSchema.WPS_KP_Package, pkgExtensions.Select(pe => pe.GetValue(PkgPackageExtensionSchema.KPN_KP_Package)));
			pkgStateQuery.OrderBy = WhsItemPackageStateSchema.Constants.WPS_SystemCreateTimeUtc + OrderByClause.Descending;
			return Factory.Load<WhsItemPackageState>(pkgStateQuery).FirstOrDefault();
		}

		void AssertPackageStatesUnderTheDCNsShouldLinkToASpecificDll(ZGuid[] dcnPKs, ZGuid dllPK, UniversalObjectFactory universalObjectFactory = null)
		{
			var dispatchConsignments = (universalObjectFactory ?? Factory).Load<WhsItemDispatchConsignment>(new ZQuery(WhsItemDispatchConsignmentSchema.PK, dcnPKs));
			var allPackageStates = dispatchConsignments.SelectMany(rc => rc.PackageStates).ToArray();
			Assert("All package states must have the same load list.", allPackageStates.All(p => p.WPS_WDL_LoadList == dllPK));
		}

		(WhsItemDispatchLoadList[] dlls, WhsItemDispatchConsignment[] dcns, WhsItemDispatchLoadListDTUPivot[] pivots, WhsItemDispatchTransportationUnit[] dtus)
			AssertEntitiesCount(int? dllCount, int? dcnCount, int? dllDtuPivotCount, int? dtuCount, UniversalObjectFactory universalObjectFactory = null)
		{
			var dlls = AssertEntityCount<WhsItemDispatchLoadList>(dllCount, universalObjectFactory);
			var dcns = AssertEntityCount<WhsItemDispatchConsignment>(dcnCount, universalObjectFactory);
			var pivots = AssertEntityCount<WhsItemDispatchLoadListDTUPivot>(dllDtuPivotCount, universalObjectFactory);
			var dtus = AssertEntityCount<WhsItemDispatchTransportationUnit>(dtuCount, universalObjectFactory);
			return (dlls, dcns, pivots, dtus);
		}

		void AssertGateRelatedBookingConfirmedLogs(WhsItemDispatchTransportationUnit dtu, ZString expectedGateBookingConfirmedLogRef, ZString expectedGateMovementBookingConfirmedLogRef)
		{
			var bookingConfirmedLogQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.BookingConfirmed.Code);
			var allBookingConfirmedLogs = dtu.Logs.Find(bookingConfirmedLogQuery).ToList();
			var gateBookingConfirmedLog = allBookingConfirmedLogs.SingleOrDefault(log => log.SL_Reference.Contains(nameof(DataContextType.GateBooking)));
			var gateMovementBookingConfirmedLog = allBookingConfirmedLogs.SingleOrDefault(log => log.SL_Reference.Contains(nameof(DataContextType.GateMovementBooking)));

			AssertNotNull("Gate Booking Confirmed Log should exist", gateBookingConfirmedLog);
			AssertNotNull("Gate Movement Booking Confirmed Log should exist", gateMovementBookingConfirmedLog);
			AssertEquals(expectedGateBookingConfirmedLogRef, gateBookingConfirmedLog.SL_Reference);
			AssertEquals(expectedGateMovementBookingConfirmedLogRef, gateMovementBookingConfirmedLog.SL_Reference);
		}

		void AssertGateRelatedBookingConfirmedLog(WhsItemDispatchTransportationUnit dtu, ZString gateBookingReferenceType, ZString expectedLogReference, bool isLogUnique = true)
		{
			var bookingConfirmedLogQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.BookingConfirmed.Code);
			var allBookingConfirmedLogs = dtu.Logs.Find(bookingConfirmedLogQuery).ToList();
			StmALog gateRelatedBookingConfirmedLog = null;
			if (isLogUnique)
			{
				gateRelatedBookingConfirmedLog = allBookingConfirmedLogs.SingleOrDefault(log => log.SL_Reference.Contains(gateBookingReferenceType));
			}
			else
			{
				gateRelatedBookingConfirmedLog = allBookingConfirmedLogs.OrderByDescending(log => log.SL_EventTimeUtc).FirstOrDefault();
			}
			AssertNotNull("Gate Related Booking Confirmed Log should exist", gateRelatedBookingConfirmedLog);
			AssertEquals(expectedLogReference, gateRelatedBookingConfirmedLog.SL_Reference);
		}

		#endregion

		ContainerType DefaultContainerType => new ContainerType { Code = "20GP" };
		WhsTransitTestHelper Helper => new WhsTransitTestHelper(Factory.BOFactory);
	}
}
