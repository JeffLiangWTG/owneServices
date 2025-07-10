using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Packing.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing
{
	class WhsTransitReceiveConsolDataObjectReaderTest : TransitUniversalTestCase
	{
		#region TestConsolImportsManyReceiveConsignments_WithArrivalTransitWarehouseReceive

		public void TestConsolImportsManyReceiveConsignments_WithArrivalTransitWarehouseReceive()
		{
			Data.SetupForForwardingImport();
			Data.HeaderDataObject.SetContainerCollection(() => new DataObjectList<Container>
			{
				new Container { Link = 1, ContainerNumber = "CNT-1", ContainerType = DefaultContainerType },
				new Container { Link = 2, ContainerNumber = "CNT-2", ContainerType = DefaultContainerType },
				new Container { Link = 3, ContainerNumber = "CNT-3", ContainerType = DefaultContainerType }
			});

			var shipment1 = Data.CreateShipmentWithPackages("A123", "Pack1", "Pack2");
			shipment1.PackingLineCollection[0].ContainerLink = 1;
			shipment1.PackingLineCollection[1].ContainerLink = 2;

			var shipment2 = Data.CreateShipmentWithPackages("B456", "Pack3", "Pack4", "Pack5");
			shipment2.PackingLineCollection[0].ContainerLink = 2;
			shipment2.PackingLineCollection[1].ContainerLink = 2;
			shipment2.PackingLineCollection[2].ContainerLink = 3;
			Data.HeaderDataObject.SetSubShipmentCollection(() => new DataObjectList<Shipment> { shipment1, shipment2 });

			// so there will be:
			// container CNT-1 with package Pack1
			// container CNT-2 with packages Pack2, Pack3, Pack4
			// container CNT-3 with package Pack5

			Logger.TopLevelDataObject = Data.HeaderDataObject;
			Data.HeaderDataObject.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.ATW } } });
			Data.HeaderDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			Data.HeaderDataObject.DataContext.AddDataTarget(DataContextType.ForwardingConsol, "C001");
			var consol = new WhsTransitReceiveConsolDataObjectReader(Data.HeaderDataObject, Logger, Factory).ReadIntoBusinessObject();

			AssertEquals(2, consol.PopulatedConsignmentsForTesting.Length);

			var packageJob1 = Factory.Load<PkgPackageJob>(new ZQuery(PkgPackageJobSchema.KJ_ParentID, consol.PopulatedConsignmentsForTesting[0].PK)).Single();
			AssertEquals("There should be 2 packages on the package Job.", 2, packageJob1.Packages.Count);
			AssertContainsExactElementsInAnyOrder(new ZString[] { "Pack1", "Pack2" }, packageJob1.Packages.Select(p => p.KP_PackageID));

			var packageJob2 = Factory.Load<PkgPackageJob>(new ZQuery(PkgPackageJobSchema.KJ_ParentID, consol.PopulatedConsignmentsForTesting[1].PK)).Single();
			AssertEquals("There should be 3 packages on the package Job.", 3, packageJob2.Packages.Count);
			AssertContainsExactElementsInAnyOrder(new ZString[] { "Pack3", "Pack4", "Pack5" }, packageJob2.Packages.Select(p => p.KP_PackageID));

			var asns = Factory.Load<WhsItemReceiveASN>(new ZQuery());
			AssertEquals(3, asns.Length);

			var asn1 = asns.SingleOrDefault(a => a.WRP_VehicleReference == "CNT-1" && a.WRP_WW_IntendedWarehouse == Data.WarehouseINTHEMSYD.PK);
			var asn2 = asns.SingleOrDefault(a => a.WRP_VehicleReference == "CNT-2" && a.WRP_WW_IntendedWarehouse == Data.WarehouseINTHEMSYD.PK);
			var asn3 = asns.SingleOrDefault(a => a.WRP_VehicleReference == "CNT-3" && a.WRP_WW_IntendedWarehouse == Data.WarehouseINTHEMSYD.PK);
			AssertNotNull(asn1);
			AssertNotNull(asn2);
			AssertNotNull(asn3);

			var rtus = Factory.Load<WhsItemReceiveTransportationUnit>(new ZQuery());
			AssertEquals("An RTU should be created for each container.", 3, rtus.Length);
			var rtu1 = rtus.SingleOrDefault(rtu => rtu.WRH_VehicleReference == "CNT-1" && rtu.WRH_WW_Warehouse == Data.WarehouseINTHEMSYD.PK);
			var rtu2 = rtus.SingleOrDefault(rtu => rtu.WRH_VehicleReference == "CNT-2" && rtu.WRH_WW_Warehouse == Data.WarehouseINTHEMSYD.PK);
			var rtu3 = rtus.SingleOrDefault(rtu => rtu.WRH_VehicleReference == "CNT-3" && rtu.WRH_WW_Warehouse == Data.WarehouseINTHEMSYD.PK);
			AssertNotNull(rtu1);
			AssertNotNull(rtu2);
			AssertNotNull(rtu3);

			var pivots = Factory.Load<WhsItemReceiveASNRTUPivot>(new ZQuery());
			AssertEquals("Each RTU should be linked to its ASN.", 3, pivots.Length);
			var pivot1 = pivots.SingleOrDefault(p => p.WAR_WRP_TransitReceiveASN == asn1.PK && p.WAR_WRH_TransitReceiveTransportationUnit == rtu1.PK);
			var pivot2 = pivots.SingleOrDefault(p => p.WAR_WRP_TransitReceiveASN == asn2.PK && p.WAR_WRH_TransitReceiveTransportationUnit == rtu2.PK);
			var pivot3 = pivots.SingleOrDefault(p => p.WAR_WRP_TransitReceiveASN == asn3.PK && p.WAR_WRH_TransitReceiveTransportationUnit == rtu3.PK);
			AssertNotNull(pivot1);
			AssertNotNull(pivot2);
			AssertNotNull(pivot3);

			var receiveConsignments = Factory.Load<WhsItemReceiveConsignment>(new ZQuery());
			AssertEquals(2, receiveConsignments.Length);

			var receiveConsignment1 = consol.PopulatedConsignmentsForTesting.SingleOrDefault(rc => rc.WRC_ConsignmentID == "A123");
			AssertNotNull(receiveConsignment1);
			AssertEquals(2, receiveConsignment1.PackageStates.Count);

			var pack1 = receiveConsignment1.PackageStates.SingleOrDefault(p => p.Package.KP_PackageID == "Pack1");
			AssertNotNull(pack1);
			AssertEquals(asn1.PK, pack1.WPS_WRP_ReceiveExpectedPacking);

			var pack2 = receiveConsignment1.PackageStates.SingleOrDefault(p => p.Package.KP_PackageID == "Pack2");
			AssertNotNull(pack2);
			AssertEquals(asn2.PK, pack2.WPS_WRP_ReceiveExpectedPacking);

			var receiveConsignment2 = consol.PopulatedConsignmentsForTesting.SingleOrDefault(rc => rc.WRC_ConsignmentID == "B456");
			AssertNotNull(receiveConsignment2);

			var pack3 = receiveConsignment2.PackageStates.SingleOrDefault(p => p.Package.KP_PackageID == "Pack3");
			AssertNotNull(pack3);
			AssertEquals(asn2.PK, pack3.WPS_WRP_ReceiveExpectedPacking);

			var pack4 = receiveConsignment2.PackageStates.SingleOrDefault(p => p.Package.KP_PackageID == "Pack4");
			AssertNotNull(pack4);
			AssertEquals(asn2.PK, pack4.WPS_WRP_ReceiveExpectedPacking);

			var pack5 = receiveConsignment2.PackageStates.SingleOrDefault(p => p.Package.KP_PackageID == "Pack5");
			AssertNotNull(pack5);
			AssertEquals(asn3.PK, pack5.WPS_WRP_ReceiveExpectedPacking);
		}

		#endregion

		#region TestConsolImportsManyReceiveConsignments_WithDepartureTransitWarehouseReceive

		public void TestConsolImportsManyReceiveConsignments_WithDepartureTransitWarehouseReceive()
		{
			Data.SetupForForwardingImport();
			Data.HeaderDataObject.SetContainerCollection(() => new DataObjectList<Container>
			{
				new Container { Link = 1, ContainerNumber = "CNT-1", ContainerType = DefaultContainerType },
				new Container { Link = 2, ContainerNumber = "CNT-2", ContainerType = DefaultContainerType },
				new Container { Link = 3, ContainerNumber = "CNT-3", ContainerType = DefaultContainerType }
			});

			var shipment1 = Data.CreateShipmentWithPackages("A123", "Pack1", "Pack2");
			shipment1.PackingLineCollection[0].ContainerLink = 1;
			shipment1.PackingLineCollection[1].ContainerLink = 2;

			var shipment2 = Data.CreateShipmentWithPackages("B456", "Pack3", "Pack4", "Pack5");
			shipment2.PackingLineCollection[0].ContainerLink = 2;
			shipment2.PackingLineCollection[1].ContainerLink = 2;
			shipment2.PackingLineCollection[2].ContainerLink = 3;
			Data.HeaderDataObject.SetSubShipmentCollection(() => new DataObjectList<Shipment> { shipment1, shipment2 });

			// so there will be:
			// container CNT-1 with package Pack1
			// container CNT-2 with packages Pack2, Pack3, Pack4
			// container CNT-3 with package Pack5

			Logger.TopLevelDataObject = Data.HeaderDataObject;
			Data.HeaderDataObject.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.DTW, ServiceCode = ServiceCodeType.TWR } } });
			Data.HeaderDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			Data.HeaderDataObject.DataContext.AddDataTarget(DataContextType.ForwardingConsol, "C001");
			var consol = new WhsTransitReceiveConsolDataObjectReader(Data.HeaderDataObject, Logger, Factory).ReadIntoBusinessObject();

			AssertEquals(2, consol.PopulatedConsignmentsForTesting.Length);

			var packageJob1 = Factory.Load<PkgPackageJob>(new ZQuery(PkgPackageJobSchema.KJ_ParentID, consol.PopulatedConsignmentsForTesting[0].PK)).Single();
			AssertEquals("There should be 2 packages on the package Job.", 2, packageJob1.Packages.Count);
			AssertContainsExactElementsInAnyOrder(new ZString[] { "Pack1", "Pack2" }, packageJob1.Packages.Select(p => p.KP_PackageID));

			var packageJob2 = Factory.Load<PkgPackageJob>(new ZQuery(PkgPackageJobSchema.KJ_ParentID, consol.PopulatedConsignmentsForTesting[1].PK)).Single();
			AssertEquals("There should be 3 packages on the package Job.", 3, packageJob2.Packages.Count);
			AssertContainsExactElementsInAnyOrder(new ZString[] { "Pack3", "Pack4", "Pack5" }, packageJob2.Packages.Select(p => p.KP_PackageID));

			var asns = Factory.Load<WhsItemReceiveASN>(new ZQuery());
			AssertEquals("Given the Recipient Role is DTR, then the entire consol should be imported into one Receive ASN.", 1, asns.Length);

			var asn = asns.SingleOrDefault(a => a.WRP_VehicleReference == "WaybillParent" && a.WRP_WW_IntendedWarehouse == Data.Warehouse.PK);
			AssertNotNull("ASN must be successfully created with the correct vehicle reference and intended warehouse.", asn);

			var rtus = Factory.Load<WhsItemReceiveTransportationUnit>(new ZQuery());
			AssertEquals("No RTUs should be created for a Departure Transit Warehouse.", 0, rtus.Length);
			var pivots = Factory.Load<WhsItemReceiveASNRTUPivot>(new ZQuery());
			AssertEquals("No Pivots should be created for a Departure Transit Warehouse.", 0, pivots.Length);

			var receiveConsignments = Factory.Load<WhsItemReceiveConsignment>(new ZQuery());
			AssertEquals(2, receiveConsignments.Length);

			var receiveConsignment1 = consol.PopulatedConsignmentsForTesting.SingleOrDefault(rc => rc.WRC_ConsignmentID == "A123");
			AssertNotNull(receiveConsignment1);
			AssertEquals(2, receiveConsignment1.PackageStates.Count);

			var pack1 = receiveConsignment1.PackageStates.SingleOrDefault(p => p.Package.KP_PackageID == "Pack1");
			AssertNotNull(pack1);
			AssertEquals(asn.PK, pack1.WPS_WRP_ReceiveExpectedPacking);

			var pack2 = receiveConsignment1.PackageStates.SingleOrDefault(p => p.Package.KP_PackageID == "Pack2");
			AssertNotNull(pack2);
			AssertEquals(asn.PK, pack2.WPS_WRP_ReceiveExpectedPacking);

			var receiveConsignment2 = consol.PopulatedConsignmentsForTesting.SingleOrDefault(rc => rc.WRC_ConsignmentID == "B456");
			AssertNotNull(receiveConsignment2);

			var pack3 = receiveConsignment2.PackageStates.SingleOrDefault(p => p.Package.KP_PackageID == "Pack3");
			AssertNotNull(pack3);
			AssertEquals(asn.PK, pack3.WPS_WRP_ReceiveExpectedPacking);

			var pack4 = receiveConsignment2.PackageStates.SingleOrDefault(p => p.Package.KP_PackageID == "Pack4");
			AssertNotNull(pack4);
			AssertEquals(asn.PK, pack4.WPS_WRP_ReceiveExpectedPacking);

			var pack5 = receiveConsignment2.PackageStates.SingleOrDefault(p => p.Package.KP_PackageID == "Pack5");
			AssertNotNull(pack5);
			AssertEquals(asn.PK, pack5.WPS_WRP_ReceiveExpectedPacking);
		}

		#endregion

		#region TestConsolImportWithLooseGoodsAndEmptyContainers

		public void TestConsolImportWithLooseGoodsAndEmptyContainers()
		{
			Data.SetupForForwardingImport();
			Data.HeaderDataObject.SetContainerCollection(() => new DataObjectList<Container>
			{
				new Container { Link = 1, ContainerNumber = "", ContainerType = DefaultContainerType },
				new Container { Link = 2, ContainerNumber = "", ContainerType = DefaultContainerType },
			});

			var shipment1 = Data.CreateShipmentWithPackages("A123", "Pack1", "Pack2");
			shipment1.PackingLineCollection[0].ContainerLink = 1; // Pack1
			shipment1.PackingLineCollection[1].ContainerLink = 2; // Pack2

			var shipment2 = Data.CreateShipmentWithPackages("B456", "Pack3", "Pack4");
			shipment2.PackingLineCollection[0].ContainerLink = 2; // Pack3
																  // Pack4 has no container link

			Data.HeaderDataObject.SetSubShipmentCollection(() => new DataObjectList<Shipment> { shipment1, shipment2 });

			Logger.TopLevelDataObject = Data.HeaderDataObject;
			Data.HeaderDataObject.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.ATW } } });
			Data.HeaderDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			Data.HeaderDataObject.DataContext.AddDataTarget(DataContextType.ForwardingConsol, "C001");
			var consol = new WhsTransitReceiveConsolDataObjectReader(Data.HeaderDataObject, Logger, Factory).ReadIntoBusinessObject();

			AssertEquals(2, consol.PopulatedConsignmentsForTesting.Length);

			var packageJob1 = Factory.Load<PkgPackageJob>(new ZQuery(PkgPackageJobSchema.KJ_ParentID, consol.PopulatedConsignmentsForTesting[0].PK)).Single();
			AssertEquals("There should be 2 packages on the package Job.", 2, packageJob1.Packages.Count);
			AssertContainsExactElementsInAnyOrder(new ZString[] { "Pack1", "Pack2" }, packageJob1.Packages.Select(p => p.KP_PackageID));

			var packageJob2 = Factory.Load<PkgPackageJob>(new ZQuery(PkgPackageJobSchema.KJ_ParentID, consol.PopulatedConsignmentsForTesting[1].PK)).Single();
			AssertEquals("There should be 2 packages on the package Job.", 2, packageJob2.Packages.Count);
			AssertContainsExactElementsInAnyOrder(new ZString[] { "Pack3", "Pack4" }, packageJob2.Packages.Select(p => p.KP_PackageID));

			var receiveConsignments = Factory.Load<WhsItemReceiveConsignment>(new ZQuery());
			AssertEquals(2, receiveConsignments.Length);
			AssertEquals(2, receiveConsignments[0].PackageStates.Count);
			AssertEquals(2, receiveConsignments[1].PackageStates.Count);

			var allPackageStates = receiveConsignments.SelectMany(rc => rc.PackageStates).ToArray();
			Assert("All packages must be attached to some Expected Packing.", allPackageStates.All(p => !p.WPS_WRP_ReceiveExpectedPacking.IsEmpty));

			var pack1 = allPackageStates.Single(p => p.Package.KP_PackageID == "Pack1");
			var pack2 = allPackageStates.Single(p => p.Package.KP_PackageID == "Pack2");
			var pack3 = allPackageStates.Single(p => p.Package.KP_PackageID == "Pack3");
			var pack4 = allPackageStates.Single(p => p.Package.KP_PackageID == "Pack4");

			AssertEquals("Pack2 and Pack3 will arrive in same container -> same expected packing.", pack2.WPS_WRP_ReceiveExpectedPacking, pack3.WPS_WRP_ReceiveExpectedPacking);
			AssertEquals("There must be 3 expected packings created: 2 for containers and 1 for loose.", 3, allPackageStates.Select(p => p.WPS_WRP_ReceiveExpectedPacking).Distinct().Count());

			var receiveASNs = Factory.Load<WhsItemReceiveASN>(new ZQuery());
			AssertEquals(3, receiveASNs.Length);
			Assert("Vehicle reference and ETA date for receive ASNs should be empty.", receiveASNs.All(ep => ep.WRP_ETA.IsEmpty && ep.WRP_VehicleReference == "WaybillParent")); // if the container has no number, it should not create an ASN per container!
																																												 // For now RTUs aren't created to store the container information, as this is not an expected business case.
			var rtus = Factory.Load<WhsItemReceiveTransportationUnit>(new ZQuery());
			AssertEquals("No RTUs should be created from a shipment with no container numbers.", 0, rtus.Length);
			var pivots = Factory.Load<WhsItemReceiveASNRTUPivot>(new ZQuery());
			AssertEquals("No RTUs should be created from a shipment with no container numbers.", 0, pivots.Length);
		}

		#endregion

		#region TestConsolImportWithPackagesInDifferentWarehouse

		public void TestConsolImportWithPackagesInDifferentWarehouse()
		{
			Data.SetupForForwardingImport();
			var helper = new WhsTransitTestHelper(Factory.BOFactory);
			var warehouse = Data.WarehouseCRAHOLSYD;
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;

			var rcn = helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var rtu = helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);

			var packageState1 = helper.CreatePackageState(rtu, 1, "PKG", "PKG-1", TransitWarehouseStatuses.Codes.Arrived);
			helper.CreateAdditionalReference(packageState1, "S1000000", WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference);
			var packageState2 = helper.CreatePackageState(rcn, 1, "PKG", "PKG-2", TransitWarehouseStatuses.Codes.Booked, entryNum: "S1000000");
			Factory.SaveForTesting();

			var shipment = Data.CreateShipmentWithPackages("S1000000", "PKG-1", "PKG-2");
			Data.ShipmentDataObject.SetSubShipmentCollection(() => new DataObjectList<Shipment> { shipment });
			Logger.TopLevelDataObject = Data.ShipmentDataObject;
			Data.ShipmentDataObject.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.ATW } } });
			var consol = new WhsTransitReceiveConsolDataObjectReader(Data.ShipmentDataObject, Logger, Factory).ReadIntoBusinessObject();

			var receiveConsignments = Factory.Load<WhsItemReceiveConsignment>(new ZQuery(WhsItemReceiveConsignmentSchema.WRC_WW_IntendedWarehouse, SQLComparisonOperator.NotEqual, warehouse.PK));
			AssertEquals(1, receiveConsignments.Length);
			AssertEquals(2, receiveConsignments[0].PackageStates.Count);
			var packageStates = receiveConsignments[0].PackageStates;
			AssertEquals(true, packageStates.Any(p => p.WPS_Status == TransitWarehouseStatuses.Codes.Booked && p.Package.KP_PackageID == "PKG-1"));
			AssertEquals(true, packageStates.Any(p => p.WPS_Status == TransitWarehouseStatuses.Codes.Booked && p.Package.KP_PackageID == "PKG-2"));
		}

		#endregion

		#region TestConsolImportFailedWithDuplicatedHouseBillNumber

		public void TestConsolImportFailedWithDuplicatedHouseBillNumber()
		{
			Data.SetupForForwardingImport();

			var shipment1 = Data.CreateShipmentWithPackages("A123", "Pack1", "Pack2");
			var shipment2 = Data.CreateShipmentWithPackages("A123", "Pack3", "Pack4");
			var shipment3 = Data.CreateShipmentWithPackages("B456", "Pack5", "Pack6");
			var shipment4 = Data.CreateShipmentWithPackages("B456", "Pack7", "Pack8");
			var shipment5 = Data.CreateShipmentWithPackages("ABCD", "Pack9", "Pack10");

			Data.HeaderDataObject.SetSubShipmentCollection(() => new DataObjectList<Shipment> { shipment1, shipment2, shipment3, shipment4, shipment5 });

			Logger.TopLevelDataObject = Data.HeaderDataObject;
			Data.HeaderDataObject.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.ATW } } });
			AssertExceptionThrown(typeof(DataObjectReadFailureException),
				@"Duplicate House Bill Number(s) found in the Receive Instruction: A123, B456.",
				() => new WhsTransitReceiveConsolDataObjectReader(Data.HeaderDataObject, Logger, Factory).ReadIntoBusinessObject());
		}

		#endregion

		#region TestCreateASNAndRTUForEmptyContainers

		public void TestCreateASNAndRTUForEmptyContainers()
		{
			Data.SetupForForwardingImport();
			Data.HeaderDataObject.SetContainerCollection(() => new DataObjectList<Container>
			{
				new Container { Link = 1, ContainerNumber = "XXX", ContainerType = DefaultContainerType },
				new Container { Link = 2, ContainerNumber = "YYY", ContainerType = DefaultContainerType },
			});

			var shipment = Data.CreateShipmentWithPackages("A123", "Pack1", "Pack2", "Pack3");
			shipment.PackingLineCollection[0].ContainerLink = 1;
			shipment.PackingLineCollection[1].ContainerLink = 1;
			shipment.PackingLineCollection[2].ContainerLink = null;

			Data.HeaderDataObject.SetSubShipmentCollection(() => new DataObjectList<Shipment> { shipment });
			Logger.TopLevelDataObject = Data.HeaderDataObject;
			Data.HeaderDataObject.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.ATW } } });
			Data.HeaderDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			var consol = new WhsTransitReceiveConsolDataObjectReader(Data.HeaderDataObject, Logger, Factory).ReadIntoBusinessObject();
			AssertNoExceptionThrown(() => Factory.SaveForTesting());

			var rtus = Factory.Load<WhsItemReceiveTransportationUnit>(new ZQuery());
			AssertEquals("Should create 2 RTUs", 2, rtus.Length);

			var rtuXXX = rtus.Where(rtu => rtu.WRH_VehicleReference == "XXX").FirstOrDefault();
			AssertNotNull("Should create 1 RTU for container XXX", rtuXXX);

			var rtuYYY = rtus.Where(rtu => rtu.WRH_VehicleReference == "YYY").FirstOrDefault();
			AssertNotNull("Should create 1 RTU for empty container YYY", rtuYYY);

			var asns = Factory.Load<WhsItemReceiveASN>(new ZQuery());
			AssertEquals("Should create 3 ASNs", 3, asns.Length);

			var asnXXX = asns.Where(asn => asn.WRP_VehicleReference == "XXX").FirstOrDefault();
			AssertNotNull("Should create ASN for container XXX", asnXXX);

			var asnYYY = asns.Where(asn => asn.WRP_VehicleReference == "YYY").FirstOrDefault();
			AssertNotNull("Should create ASN for empty container YYY", asnYYY);

			var asnCreatedByMasterBill = asns.Where(asn => asn.WRP_VehicleReference == "WaybillParent").FirstOrDefault();
			AssertNotNull("Should create ASN for packline without container", asnCreatedByMasterBill);

			var pivots = Factory.Load<WhsItemReceiveASNRTUPivot>(new ZQuery());
			AssertEquals("Should create 2 ASN-RTU pivots", 2, pivots.Length);
			AssertNotNull("Should create a pivot for container XXX", pivots.Where(p => p.WAR_WRH_TransitReceiveTransportationUnit == rtuXXX.PK && p.WAR_WRP_TransitReceiveASN == asnXXX.PK).FirstOrDefault());
			AssertNotNull("Should create a pivot for container YYY", pivots.Where(p => p.WAR_WRH_TransitReceiveTransportationUnit == rtuYYY.PK && p.WAR_WRP_TransitReceiveASN == asnYYY.PK).FirstOrDefault());
		}

		#endregion

		#region TestCreateSingleASNForMultipleContainers

		public void TestCreateSingleASNForAllContainers_Enabled() => TestCreateSingleASNForAllContainers(true);

		public void TestCreateSingleASNForAllContainers_Disabled() => TestCreateSingleASNForAllContainers(false);

		void TestCreateSingleASNForAllContainers(bool isRegistryEnabled)
		{
			Data.SetupForForwardingImport();
			using (WarehouseDataRegistry.Instance.CreateSingleASNForAllContainers.SetTemporaryValue(Guid.Empty, Data.Warehouse.WW_GB_RelatedCompanyBranch.ToGuid(), Guid.Empty, isRegistryEnabled))
			{
				Data.HeaderDataObject.SetContainerCollection(() => new DataObjectList<Container>
				{
					new Container { Link = 1, ContainerNumber = "XXX", ContainerType = DefaultContainerType },
					new Container { Link = 2, ContainerNumber = "YYY", ContainerType = DefaultContainerType },
					new Container { Link = 3, ContainerNumber = "ZZZ", ContainerType = DefaultContainerType },
				});

				var shipment1 = Data.CreateShipmentWithPackages("A123", "Pack1", "Pack2");
				shipment1.PackingLineCollection[0].ContainerLink = 1; // Pack1
				shipment1.PackingLineCollection[1].ContainerLink = 1; // Pack2

				var shipment2 = Data.CreateShipmentWithPackages("B456", "Pack3", "Pack4");
				shipment2.PackingLineCollection[0].ContainerLink = 1; // Pack3
				shipment2.PackingLineCollection[1].ContainerLink = 2; // Pack4

				Data.HeaderDataObject.SetSubShipmentCollection(() => new DataObjectList<Shipment> { shipment1, shipment2 });

				Logger.TopLevelDataObject = Data.HeaderDataObject;
				Data.HeaderDataObject.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.ATW } } });
				Data.HeaderDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
				var consol = new WhsTransitReceiveConsolDataObjectReader(Data.HeaderDataObject, Logger, Factory).ReadIntoBusinessObject();
				AssertEquals(2, consol.PopulatedConsignmentsForTesting.Length);

				Factory.SaveForTesting();

				var receiveConsignments = Factory.Load<WhsItemReceiveConsignment>(new ZQuery());
				AssertEquals(2, receiveConsignments.Length);

				var rtus = Factory.Load<WhsItemReceiveTransportationUnit>(new ZQuery());
				AssertEquals("Should create 3 RTUs", 3, rtus.Length);

				var rtuXXX = rtus.Single(rtu => rtu.WRH_VehicleReference == "XXX");
				var rtuYYY = rtus.Single(rtu => rtu.WRH_VehicleReference == "YYY");
				var rtuZZZ = rtus.Single(rtu => rtu.WRH_VehicleReference == "ZZZ");

				var pivots = Factory.Load<WhsItemReceiveASNRTUPivot>(new ZQuery());
				AssertEquals("Should create 3 ASN-RTU pivots", 3, pivots.Length);

				var pivotXXX = pivots.Single(p => p.WAR_WRH_TransitReceiveTransportationUnit == rtuXXX.PK);
				var pivotYYY = pivots.Single(p => p.WAR_WRH_TransitReceiveTransportationUnit == rtuYYY.PK);
				var pivotZZZ = pivots.Single(p => p.WAR_WRH_TransitReceiveTransportationUnit == rtuZZZ.PK);

				var receiveASNs = Factory.Load<WhsItemReceiveASN>(new ZQuery());
				if (isRegistryEnabled)
				{
					AssertEquals(1, receiveASNs.Length);

					var receiveASN = receiveASNs.Single();
					AssertEquals("ASN parent should be consol", "WaybillParent", receiveASN.WRP_VehicleReference);

					AssertEquals("ASN should be linked to RTU XXX", receiveASN.PK, pivotXXX.WAR_WRP_TransitReceiveASN);
					AssertEquals("ASN should be linked to RTU YYY", receiveASN.PK, pivotYYY.WAR_WRP_TransitReceiveASN);
					AssertEquals("ASN should be linked to RTU ZZZ", receiveASN.PK, pivotZZZ.WAR_WRP_TransitReceiveASN);
				}
				else
				{
					AssertEquals(3, receiveASNs.Length);

					var receiveASNXXX = receiveASNs.SingleOrDefault(ep => ep.WRP_VehicleReference == "XXX");
					AssertNotNull("Should create RCN for container XXX", receiveASNXXX);

					var receiveASNYYY = receiveASNs.SingleOrDefault(ep => ep.WRP_VehicleReference == "YYY");
					AssertNotNull("Should create RCN for container YYY", receiveASNYYY);

					var receiveASNZZZ = receiveASNs.SingleOrDefault(ep => ep.WRP_VehicleReference == "ZZZ");
					AssertNotNull("Should create RCN for container ZZZ", receiveASNZZZ);

					AssertEquals("There should be one pivot for ASN-XXX and RTU-XXX", receiveASNXXX.PK, pivotXXX.WAR_WRP_TransitReceiveASN);
					AssertEquals("There should be one pivot for ASN-YYY and RTU-YYY", receiveASNYYY.PK, pivotYYY.WAR_WRP_TransitReceiveASN);
					AssertEquals("There should be one pivot for ASN-ZZZ and RTU-ZZZ", receiveASNZZZ.PK, pivotZZZ.WAR_WRP_TransitReceiveASN);
				}
			}
		}

		#endregion

		#region TestSecondImportForSameConsol

		public void TestSecondImportForSameConsol_RemovedShipment()
		{
			Data.SetupForForwardingImport();
			Data.HeaderDataObject.SetContainerCollection(() => new DataObjectList<Container>
			{
				new Container { Link = 1, ContainerNumber = "XXX", ContainerType = DefaultContainerType },
				new Container { Link = 2, ContainerNumber = "YYY", ContainerType = DefaultContainerType },
			});

			var shipment1 = Data.CreateShipmentWithPackages("A123", "Pack1", "Pack2");
			shipment1.PackingLineCollection[0].ContainerLink = 1; // Pack1
			shipment1.PackingLineCollection[1].ContainerLink = 1; // Pack2

			var shipment2 = Data.CreateShipmentWithPackages("B456", "Pack3", "Pack4");
			shipment2.PackingLineCollection[0].ContainerLink = 1; // Pack3
			shipment2.PackingLineCollection[1].ContainerLink = 2; // Pack4

			Data.HeaderDataObject.SetSubShipmentCollection(() => new DataObjectList<Shipment> { shipment1, shipment2 });

			// so there will be:
			// container XXX with packages Pack1, Pack2, Pack3
			// container YYY with package Pack4

			Logger.TopLevelDataObject = Data.HeaderDataObject;
			Data.HeaderDataObject.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.ATW } } });
			Data.HeaderDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			Data.HeaderDataObject.DataContext.AddDataTarget(DataContextType.ForwardingConsol, "C001");
			var consol1 = new WhsTransitReceiveConsolDataObjectReader(Data.HeaderDataObject, Logger, Factory).ReadIntoBusinessObject();
			AssertEquals(2, consol1.PopulatedConsignmentsForTesting.Length);

			Factory.SaveForTesting();

			var receiveConsignments = Factory.Load<WhsItemReceiveConsignment>(new ZQuery());
			AssertEquals(2, receiveConsignments.Length);

			var allPackageStates = receiveConsignments.SelectMany(rc => rc.PackageStates).ToArray();
			AssertEquals(4, allPackageStates.Length);
			Assert("All packages must be attached to some Expected Packing.", allPackageStates.All(p => !p.WPS_WRP_ReceiveExpectedPacking.IsEmpty));

			var pack1 = allPackageStates.Single(p => p.Package.KP_PackageID == "Pack1");
			var pack2 = allPackageStates.Single(p => p.Package.KP_PackageID == "Pack2");
			var pack3 = allPackageStates.Single(p => p.Package.KP_PackageID == "Pack3");
			var pack4 = allPackageStates.Single(p => p.Package.KP_PackageID == "Pack4");

			var receiveASNs = Factory.Load<WhsItemReceiveASN>(new ZQuery());
			AssertEquals(2, receiveASNs.Length);

			var receiveASNXXX = receiveASNs.Single(ep => ep.WRP_VehicleReference == "XXX");
			var receiveASNYYY = receiveASNs.Single(ep => ep.WRP_VehicleReference == "YYY");
			AssertEquals(receiveASNXXX.PK, pack1.WPS_WRP_ReceiveExpectedPacking);
			AssertEquals(receiveASNXXX.PK, pack2.WPS_WRP_ReceiveExpectedPacking);
			AssertEquals(receiveASNXXX.PK, pack3.WPS_WRP_ReceiveExpectedPacking);
			AssertEquals(receiveASNYYY.PK, pack4.WPS_WRP_ReceiveExpectedPacking);

			var rtus = Factory.Load<WhsItemReceiveTransportationUnit>(new ZQuery());
			AssertEquals("An RTU should be created for each container.", 2, rtus.Length);
			var rtuXXX = rtus.SingleOrDefault(rtu => rtu.WRH_VehicleReference == "XXX");
			var rtuYYY = rtus.SingleOrDefault(rtu => rtu.WRH_VehicleReference == "YYY");
			AssertNotNull(rtuXXX);
			AssertNotNull(rtuYYY);

			var pivots = Factory.Load<WhsItemReceiveASNRTUPivot>(new ZQuery());
			AssertEquals("Each RTU should be linked to its ASN.", 2, pivots.Length);
			var pivotXXX = pivots.SingleOrDefault(p => p.WAR_WRP_TransitReceiveASN == receiveASNXXX.PK && p.WAR_WRH_TransitReceiveTransportationUnit == rtuXXX.PK);
			var pivotYYY = pivots.SingleOrDefault(p => p.WAR_WRP_TransitReceiveASN == receiveASNYYY.PK && p.WAR_WRH_TransitReceiveTransportationUnit == rtuYYY.PK);
			AssertNotNull(pivotXXX);
			AssertNotNull(pivotYYY);

			// now let's try to import consol without second shipment
			Data.ShipmentDataObject.SetSubShipmentCollection(() => new DataObjectList<Shipment> { shipment1 });
			var newFactory = new UniversalObjectFactory();
			var consol2 = new WhsTransitReceiveConsolDataObjectReader(Data.ShipmentDataObject, Logger, newFactory).ReadIntoBusinessObject();

			AssertEquals(1, consol2.PopulatedConsignmentsForTesting.Length);
			AssertEquals("Consignment should stay the same", consol1.PopulatedConsignmentsForTesting[0].PK, consol2.PopulatedConsignmentsForTesting[0].PK);

			var receiveConsignmentsReimport = newFactory.Load<WhsItemReceiveConsignment>(new ZQuery());
			AssertEquals(2, receiveConsignmentsReimport.Length);
			var receiveASNsReimport = newFactory.Load<WhsItemReceiveASN>(new ZQuery());
			AssertEquals("Expected packings should not be matched.", 2, receiveASNsReimport.Length);

			var expectedPackingXXXnew = receiveASNsReimport.Single(ep => ep.WRP_VehicleReference == "XXX");
			AssertEquals("Expected packings should be matched.", receiveASNXXX.PK, expectedPackingXXXnew.PK);
			var expectedPackingYYYnew = receiveASNsReimport.Single(ep => ep.WRP_VehicleReference == "YYY");
			AssertEquals("Expected packings should be matched.", receiveASNYYY.PK, expectedPackingYYYnew.PK);

			var packageStatesReimport = receiveConsignmentsReimport.SelectMany(rc => rc.PackageStates).ToArray();
			AssertEquals(4, packageStatesReimport.Length);

			var pack1Reimport = packageStatesReimport.Single(p => p.Package.KP_PackageID == "Pack1");
			var pack2Reimport = packageStatesReimport.Single(p => p.Package.KP_PackageID == "Pack2");
			var pack3Reimport = packageStatesReimport.Single(p => p.Package.KP_PackageID == "Pack3");
			var pack4Reimport = packageStatesReimport.Single(p => p.Package.KP_PackageID == "Pack4");

			// but expected packing should change
			AssertEquals(expectedPackingXXXnew.PK, pack1Reimport.WPS_WRP_ReceiveExpectedPacking);
			AssertEquals(expectedPackingXXXnew.PK, pack2Reimport.WPS_WRP_ReceiveExpectedPacking);
			Assert("Pack3 should be removed from the ASN as it was not included on the consol.", pack3Reimport.WPS_WRP_ReceiveExpectedPacking.IsEmpty);
			AssertEquals("As current consol has no information about packages in container YYY, we can't remove that expected packing.", expectedPackingYYYnew.PK, pack4Reimport.WPS_WRP_ReceiveExpectedPacking);

			var rtusReimport = newFactory.Load<WhsItemReceiveTransportationUnit>(new ZQuery());
			AssertEquals("RTU should not be duplicated.", 2, rtusReimport.Length);
			var rtuXXXnew = rtusReimport.SingleOrDefault(rtu => rtu.WRH_VehicleReference == "XXX");
			var rtuYYYnew = rtusReimport.SingleOrDefault(rtu => rtu.WRH_VehicleReference == "YYY");
			AssertEquals("RTU for XXX should not be recreated.", rtuXXX.PK, rtuXXXnew?.PK);
			AssertEquals("RTU for YYY should not be recreated.", rtuYYY.PK, rtuYYYnew?.PK);

			var pivotsReimport = newFactory.Load<WhsItemReceiveASNRTUPivot>(new ZQuery());
			AssertEquals("Each RTU should be linked to its ASN.", 2, pivotsReimport.Length);
			var pivotXXXnew = pivotsReimport.SingleOrDefault(p => p.WAR_WRP_TransitReceiveASN == expectedPackingXXXnew.PK && p.WAR_WRH_TransitReceiveTransportationUnit == rtuXXXnew.PK);
			var pivotYYYnew = pivotsReimport.SingleOrDefault(p => p.WAR_WRP_TransitReceiveASN == expectedPackingYYYnew.PK && p.WAR_WRH_TransitReceiveTransportationUnit == rtuYYYnew.PK);
			AssertNotNull(pivotXXXnew);
			AssertNotNull(pivotYYYnew);
			AssertNoExceptionThrown(() => newFactory.SaveForTesting());
		}

		public void TestSecondImportForSameConsol_RemoveContainer() => SecondImportForSameConsol_DetachPackageFromContainer(deleteContainer: true);

		public void TestSecondImportForSameConsol_RemovePackageFromContainer() => SecondImportForSameConsol_DetachPackageFromContainer(deleteContainer: false);

		public void TestSecondImportForSameConsol_RemoveContainer_WithUnloadTask() => SecondImportForSameConsol_DetachPackageFromContainer(deleteContainer: true, createUnloadTask: true);

		public void TestSecondImportForSameConsol_RemovePackageFromContainer_WithUnloadTask() => SecondImportForSameConsol_DetachPackageFromContainer(deleteContainer: false, createUnloadTask: true);

		void SecondImportForSameConsol_DetachPackageFromContainer(bool deleteContainer, bool createUnloadTask = false)
		{
			Data.SetupForForwardingImport();
			Data.HeaderDataObject.SetContainerCollection(() => new DataObjectList<Container>
			{
				new Container { Link = 1, ContainerNumber = "XXX", ContainerType = DefaultContainerType },
			});
			Data.HeaderDataObject.WayBillNumber = "MB1";

			var shipment1 = Data.CreateShipmentWithPackages("A123", "Pack1");
			shipment1.PackingLineCollection[0].ContainerLink = 1; // Pack1

			Data.HeaderDataObject.SetSubShipmentCollection(() => new DataObjectList<Shipment> { shipment1 });

			Logger.TopLevelDataObject = Data.HeaderDataObject;
			Data.HeaderDataObject.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.ATW } } });
			Data.HeaderDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			var consol1 = new WhsTransitReceiveConsolDataObjectReader(Data.HeaderDataObject, Logger, Factory).ReadIntoBusinessObject();
			AssertEquals(1, consol1.PopulatedConsignmentsForTesting.Length);

			var receiveConsignment = Factory.Load<WhsItemReceiveConsignment>(new ZQuery()).SingleOrDefault();
			AssertNotNull("Precondition: Receive Consignment should be created.", receiveConsignment);
			var packageState = receiveConsignment.PackageStates.SingleOrDefault();
			AssertNotNull("Precondition: Package attached to the RCN should be created.", packageState);
			var receiveASNForContainer = Factory.Load<WhsItemReceiveASN>(new ZQuery()).SingleOrDefault();
			AssertNotNull("Precondition: ASN should be created for Container.", receiveASNForContainer);
			AssertEquals("Precondition: The package should be attached to the Container ASN.", receiveASNForContainer.PK, packageState.WPS_WRP_ReceiveExpectedPacking);
			var rtu = Factory.Load<WhsItemReceiveTransportationUnit>(new ZQuery()).SingleOrDefault();
			AssertNotNull("Precondition: RTU should be created for Container.", rtu);
			var pivot = Factory.Load<WhsItemReceiveASNRTUPivot>(new ZQuery()).SingleOrDefault(p => p.WAR_WRP_TransitReceiveASN == receiveASNForContainer.PK && p.WAR_WRH_TransitReceiveTransportationUnit == rtu.PK);
			AssertNotNull("Precondition: RTU should be linked to the Container ASN.", pivot);

			if (createUnloadTask)
			{
				var unloadTask = Factory.New<WhsItemUnloadTask>();
				unloadTask.WUT_WRH_ActiveReceiveHeader = rtu.PK;
				unloadTask.WUT_GS_NKUnloader = "~BP";
				unloadTask.WUT_WW_Warehouse = rtu.WRH_WW_Warehouse;
			}

			Factory.SaveForTesting();

			if (deleteContainer)
			{
				Data.HeaderDataObject.SetContainerCollection(() => new DataObjectList<Container>());
			}

			shipment1.PackingLineCollection[0].ContainerLink = null;

			// now let's try to import consol with changes
			var newFactory = new UniversalObjectFactory();
			var consol2 = new WhsTransitReceiveConsolDataObjectReader(Data.HeaderDataObject, Logger, newFactory).ReadIntoBusinessObject();
			AssertNoExceptionThrown(() => newFactory.SaveForTesting());

			AssertEquals(1, consol2.PopulatedConsignmentsForTesting.Length);
			AssertEquals("Consignment should stay the same.", consol1.PopulatedConsignmentsForTesting[0].PK, consol2.PopulatedConsignmentsForTesting[0].PK);

			newFactory = new UniversalObjectFactory();
			var asnsReimport = newFactory.Load<WhsItemReceiveASN>(new ZQuery());
			if (deleteContainer)
			{
				AssertEquals("Container ASN should be replaced with a Consol ASN.", 1, asnsReimport.Length);
				var rtusReimport = newFactory.Load<WhsItemReceiveTransportationUnit>(new ZQuery());
				AssertEquals("RTU should be deleted.", 0, rtusReimport.Length);
				var pivotsReimport = newFactory.Load<WhsItemReceiveASNRTUPivot>(new ZQuery());
				AssertEquals("Pivot should be deleted.", 0, pivotsReimport.Length);

				var asnForConsolReimport = asnsReimport.SingleOrDefault(r => r.WRP_VehicleReference == "MB1" && r.PK != receiveASNForContainer.PK);
				AssertNotNull("Container ASN should be replaced with a Consol ASN.", asnForConsolReimport);
				var packageStatesReimport = newFactory.Load<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_UnitType, PackageStateUnitType.Codes.Package));
				AssertEquals("Package should be matched.", 1, packageStatesReimport.Length);
				AssertEquals("Package should be attached to the Consol ASN", asnForConsolReimport.PK, packageStatesReimport[0].WPS_WRP_ReceiveExpectedPacking);
			}
			else
			{
				AssertEquals("Consol ASN should be created.", 2, asnsReimport.Length);
				AssertEquals("Consol ASN should be created.", 1, asnsReimport.Where(asn => asn.WRP_VehicleReference == "MB1").Count());
				AssertEquals("Container ASN should not be deleted for empty container.", 1, asnsReimport.Where(asn => asn.WRP_VehicleReference == "XXX").Count());
				var rtusReimport = newFactory.Load<WhsItemReceiveTransportationUnit>(new ZQuery());
				AssertEquals("RTU should be not be deleted for empty container.", 1, rtusReimport.Length);
				var pivotsReimport = newFactory.Load<WhsItemReceiveASNRTUPivot>(new ZQuery());
				AssertEquals("Pivot should not be deleted.", 1, pivotsReimport.Length);

				var asnForConsolReimport = asnsReimport.SingleOrDefault(r => r.WRP_VehicleReference == "MB1" && r.PK != receiveASNForContainer.PK);
				AssertNotNull("Container ASN should be replaced with a Consol ASN.", asnForConsolReimport);
				var packageStatesReimport = newFactory.Load<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_UnitType, PackageStateUnitType.Codes.Package));
				AssertEquals("Package should be matched.", 1, packageStatesReimport.Length);
				AssertEquals("Package should be attached to the Consol ASN", asnForConsolReimport.PK, packageStatesReimport[0].WPS_WRP_ReceiveExpectedPacking);
			}
		}

		#endregion

		#region TestImportShipmentOnDifferentConsol

		public void TestImportShipmentOnDifferentConsol()
		{
			Data.SetupForForwardingImport();
			Data.HeaderDataObject.SetContainerCollection(() => new DataObjectList<Container>
			{
				new Container { Link = 1, ContainerNumber = "XXX", ContainerType = DefaultContainerType },
			});

			var shipment1 = Data.CreateShipmentWithPackages("A123", "Pack1");
			shipment1.PackingLineCollection[0].ContainerLink = 1; // Pack1

			Data.HeaderDataObject.SetSubShipmentCollection(() => new DataObjectList<Shipment> { shipment1 });

			shipment1.WayBillNumber = "HB1";
			Data.HeaderDataObject.WayBillNumber = "MB1";

			Logger.TopLevelDataObject = Data.HeaderDataObject;
			Data.HeaderDataObject.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.ATW } } });
			Data.HeaderDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			var consol1 = new WhsTransitReceiveConsolDataObjectReader(Data.HeaderDataObject, Logger, Factory).ReadIntoBusinessObject();
			AssertEquals(1, consol1.PopulatedConsignmentsForTesting.Length);

			var receiveConsignment = Factory.Load<WhsItemReceiveConsignment>(new ZQuery()).SingleOrDefault();
			AssertNotNull("Precondition: Receive Consignment should be created.", receiveConsignment);
			var receiveASN = Factory.Load<WhsItemReceiveASN>(new ZQuery()).SingleOrDefault(r => r.WRP_VehicleReference == "XXX");
			AssertNotNull("Precondition: ASN should be created.", receiveASN);
			var packageState = receiveConsignment.PackageStates.SingleOrDefault(p => p.Package.KP_PackageID == "Pack1");
			AssertNotNull("Precondition: Package should be attached to the RCN should be created.", packageState);
			AssertEquals("Precondition: The package should be attached to the ASN.", receiveASN.PK, packageState.WPS_WRP_ReceiveExpectedPacking);
			var rtu = Factory.Load<WhsItemReceiveTransportationUnit>(new ZQuery()).SingleOrDefault(r => r.WRH_VehicleReference == "XXX");
			AssertNotNull("Precondition: RTU should be created.", rtu);
			var pivot = Factory.Load<WhsItemReceiveASNRTUPivot>(new ZQuery()).SingleOrDefault(p => p.WAR_WRP_TransitReceiveASN == receiveASN.PK && p.WAR_WRH_TransitReceiveTransportationUnit == rtu.PK);
			AssertNotNull("Precondition: RTU should be linked to the ASN.", pivot);

			Factory.SaveForTesting();

			Data.HeaderDataObject.WayBillNumber = "MB2";

			// now let's try to import the shipment on another consol
			var newFactory = new UniversalObjectFactory();
			Logger.TopLevelDataObject = Data.HeaderDataObject;
			var consol2 = new WhsTransitReceiveConsolDataObjectReader(Data.HeaderDataObject, Logger, newFactory).ReadIntoBusinessObject();
			AssertNoExceptionThrown(() => newFactory.SaveForTesting());

			var receiveConsignmentsReimport = newFactory.Load<WhsItemReceiveConsignment>(new ZQuery());
			AssertEquals("The RCN should be matched.", 1, receiveConsignmentsReimport.Length);
			AssertEquals("The RCN should be matched.", receiveConsignment.PK, receiveConsignmentsReimport[0].PK);
			var receiveASNsReimport = newFactory.Load<WhsItemReceiveASN>(new ZQuery());
			AssertEquals("The ASN should be replaced.", 1, receiveASNsReimport.Length);
			AssertNotEquals("The ASN should be replaced.", receiveASN.PK, receiveASNsReimport[0].PK);
			var rtusReimport = newFactory.Load<WhsItemReceiveTransportationUnit>(new ZQuery());
			AssertEquals("The RTU should be replaced.", 1, rtusReimport.Length);
			AssertNotEquals("The RTU should be replaced.", rtu.PK, rtusReimport[0].PK);
			var pivotsReimport = newFactory.Load<WhsItemReceiveASNRTUPivot>(new ZQuery());
			AssertEquals("The pivot should be replaced.", 1, pivotsReimport.Length);
			AssertNotEquals("The pivot should be replaced.", pivot.PK, pivotsReimport[0].PK);

			AssertContains("Should add a log when deleting the ASN.", "Deleted ASNs 'TRT00000001' as they have no remaining packages.", Logger.Logs);
			AssertContains("Should add a log when deleting the RTU.", "Deleted RTUs 'TR00000001' as their planned ASNs were deleted.", Logger.Logs);

			AssertEquals("There should be a single package.", 1, newFactory.Load<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_UnitType, PackageStateUnitType.Codes.Package)).Length);
			var packageStateReimport = receiveConsignmentsReimport[0].PackageStates.SingleOrDefault(p => p.Package.KP_PackageID == "Pack1" && p.WPS_WRP_ReceiveExpectedPacking == receiveASNsReimport[0].PK);
			AssertEquals("Package should be matched and reassigned to the new ASN.", packageState.PK, packageStateReimport?.PK);
		}

		#endregion

		#region Test Delete ASN

		public void TestDeleteASN_WithUnimportedPackagesOnUnmatchedASN_WithCompletePackingLine_DeletesUnmatchedPackageAndASN()
			=> TestDeleteASN_WithUnimportedPackagesOnUnmatchedASN(CollectionContent.Complete);

		public void TestDeleteASN_WithUnimportedPackagesOnUnmatchedASN_WithPartialPackingLine_DoesNotReplaceUnmatchedASN()
			=> TestDeleteASN_WithUnimportedPackagesOnUnmatchedASN(CollectionContent.Partial);

		protected void TestDeleteASN_WithUnimportedPackagesOnUnmatchedASN(CollectionContent packlingLineContent)
		{
			// Setup booked package that will be matched on import. Attach them to (unmatched) ASN
			var warehouse = Data.WarehouseINTHEMSYD;
			var bookingPartyOrg = GlbCompany.CurrentCompany.OrgProxy;
			var matchedRCN = Helper.CreateReceiveConsignment("HB1", warehouse.PK, consignor: Data.Orgs.CRAHOLSYD);
			var unmatchedASN = Helper.CreateReceiveASN("XXX", warehouse.PK, bookingPartyOrg, bookingPartyOrg);
			var unmatchedPackage = Helper.CreatePackageState(matchedRCN, 1, "PLT", "Pack1", TransitWarehouseStatuses.Codes.Booked, receiveASN: unmatchedASN);

			Factory.SaveForTesting();

			Data.SetupForForwardingImport();
			var shipment1 = Data.CreateShipmentWithPackages("HB1", "Pack2");
			shipment1.OrganizationAddressCollection[2] = Data.Orgs.Consignor_CRAHOLSYD;
			shipment1.PackingLineCollection.Content = packlingLineContent;

			Data.HeaderDataObject.SetSubShipmentCollection(() => new DataObjectList<Shipment> { shipment1 });
			Data.HeaderDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);

			Logger.TopLevelDataObject = Data.HeaderDataObject;
			Data.HeaderDataObject.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.ATW } } });
			Data.HeaderDataObject.SetOrganizationAddressCollection(() => shipment1.OrganizationAddressCollection);
			Data.HeaderDataObject.WayBillNumber = "MB1";
			var newFactory = new UniversalObjectFactory();

			AssertNoExceptionThrown(() => new WhsTransitReceiveConsolDataObjectReader(Data.HeaderDataObject, Logger, newFactory).ReadIntoBusinessObject());

			var packages = newFactory.Load<WhsItemPackageState>(new ZQuery());

			var asns = newFactory.Load<WhsItemReceiveASN>(new ZQuery());
			var asnOld = asns.SingleOrDefault(a => a.PK == unmatchedASN.PK);
			var asnNew = asns.SingleOrDefault(a => a.PK != unmatchedASN.PK);

			if (packlingLineContent == CollectionContent.Complete)
			{
				AssertNull("Unmatched ASN should be deleted.", asnOld);
				AssertEquals("Unmatched ASN should be deleted.", asns.Length, 1);
				AssertEquals("The new ASN should be for the consol.", "MB1", asnNew.WRP_VehicleReference);
				AssertEquals("Pack1 should be deleted.", packages.Length, 1);
				AssertNotNull("Pack2 should be assigned to the new ASN.",
					asnNew.PackageStates.SingleOrDefault(p => p.Package.KP_PackageID == "Pack2"));
				AssertNoExceptionThrown(() => newFactory.SaveForTesting());
			}
			else if (packlingLineContent == CollectionContent.Partial)
			{
				AssertEquals("Unmatched ASN should not be deleted.", 2, asns.Length);
				AssertEquals("The new ASN should be for the consol.", "MB1", asnNew.WRP_VehicleReference);
				AssertEquals("Pack1 should not be deleted.", packages.Length, 2);
				AssertNotNull("Pack1 should be assigned to the old ASN.",
					asnOld.PackageStates.SingleOrDefault(p => p.Package.KP_PackageID == "Pack1"));
				AssertNotNull("Pack2 should be assigned to the new ASN.",
					asnNew.PackageStates.SingleOrDefault(p => p.Package.KP_PackageID == "Pack2"));
				AssertNoExceptionThrown(() => newFactory.SaveForTesting());
			}
		}

		public void TestDeleteASN_WithUnmatchedPackages_DoesNotDeleteASN()
		{
			// Setup booked package that will be matched on import. Attach them to (unmatched) ASN
			var warehouse = Data.WarehouseINTHEMSYD;
			var bookingPartyOrg = GlbCompany.CurrentCompany.OrgProxy;
			var matchedRCN = Helper.CreateReceiveConsignment("HB1", warehouse.PK, consignor: Data.Orgs.CRAHOLSYD);
			var unmatchedASN = Helper.CreateReceiveASN("XXX", warehouse.PK, bookingPartyOrg, bookingPartyOrg);
			unmatchedASN.WRP_VehicleReference = "XXX";
			Helper.CreatePackageState(matchedRCN, 1, "PLT", "Pack1", TransitWarehouseStatuses.Codes.Booked, receiveASN: unmatchedASN);
			var unmatchedRCN = Helper.CreateReceiveConsignment("HB2", warehouse.PK, consignor: Data.Orgs.CRAHOLSYD);
			Helper.CreatePackageState(unmatchedRCN, 1, "PLT", "Pack2", TransitWarehouseStatuses.Codes.Booked, receiveASN: unmatchedASN);

			Factory.SaveForTesting();

			Data.SetupForForwardingImport();
			var shipment1 = Data.CreateShipmentWithPackages("HB1", "Pack1");
			shipment1.OrganizationAddressCollection[2] = Data.Orgs.Consignor_CRAHOLSYD;
			Data.HeaderDataObject.SetSubShipmentCollection(() => new DataObjectList<Shipment> { shipment1 });
			Data.HeaderDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);

			Logger.TopLevelDataObject = Data.HeaderDataObject;
			Data.HeaderDataObject.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.ATW } } });
			Data.HeaderDataObject.SetOrganizationAddressCollection(() => shipment1.OrganizationAddressCollection);
			Data.HeaderDataObject.WayBillNumber = "MB1";
			var newFactory = new UniversalObjectFactory();

			AssertNoExceptionThrown(() => new WhsTransitReceiveConsolDataObjectReader(Data.HeaderDataObject, Logger, newFactory).ReadIntoBusinessObject());

			var receiveASNsReimport = newFactory.Load<WhsItemReceiveASN>(new ZQuery());
			AssertEquals("Unmatched ASN should be remain.", 2, receiveASNsReimport.Length);
			var unmatchedASNAfterImport = receiveASNsReimport.SingleOrDefault(asn => asn.WRP_VehicleReference == "XXX" && asn.PK == unmatchedASN.PK);
			var newASN = receiveASNsReimport.SingleOrDefault(asn => asn.WRP_VehicleReference == "MB1" && asn.PK != unmatchedASN.PK);
			AssertNotNull("Unmatched ASN should remain.", unmatchedASNAfterImport);
			AssertNotNull("The new ASN should be for the consol.", newASN);
			AssertNotNull("Pack1 should be assigned to the new ASN.",
				newASN.PackageStates.SingleOrDefault(p => p.Package.KP_PackageID == "Pack1"));
			AssertNotNull("Pack2 should be assigned to the old ASN.",
				unmatchedASNAfterImport.PackageStates.SingleOrDefault(p => p.Package.KP_PackageID == "Pack2"));
			AssertNoExceptionThrown(() => newFactory.SaveForTesting());
		}

		public void TestDeleteASN_WithUnmatchedPackagesOnRTU_DoesNotDeleteRTU()
		{
			// Setup booked package that will be matched on import. Attach them to (unmatched) ASN
			var warehouse = Data.WarehouseINTHEMSYD;
			var bookingPartyOrg = GlbCompany.CurrentCompany.OrgProxy;
			var matchedRCN = Helper.CreateReceiveConsignment("HB1", warehouse.PK, consignor: Data.Orgs.CRAHOLSYD);
			var unmatchedASN = Helper.CreateReceiveASN("XXX", warehouse.PK, bookingPartyOrg, bookingPartyOrg);
			Helper.CreatePackageState(matchedRCN, 1, "PLT", "Pack1", TransitWarehouseStatuses.Codes.Booked, receiveASN: unmatchedASN);
			var stagingLocation = warehouse.DefaultInboundDockDoorLocation;
			var unmatchedRTU = Helper.CreateReceiveTransportationUnit("XXX", warehouse.PK, stagingLocation.PK);
			Helper.CreatePackageState(unmatchedRTU, 1, "PLT", "Pack2", TransitWarehouseStatuses.Codes.Arrived);
			Helper.CreateReceiveASNRTUPivot(unmatchedRTU.PK, unmatchedASN.PK);

			Factory.SaveForTesting();

			Data.SetupForForwardingImport();
			var shipment1 = Data.CreateShipmentWithPackages("HB1", "Pack1");
			shipment1.OrganizationAddressCollection[2] = Data.Orgs.Consignor_CRAHOLSYD;
			Data.HeaderDataObject.SetSubShipmentCollection(() => new DataObjectList<Shipment> { shipment1 });
			Data.HeaderDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);

			Logger.TopLevelDataObject = Data.HeaderDataObject;
			Data.HeaderDataObject.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.ATW } } });
			Data.HeaderDataObject.SetOrganizationAddressCollection(() => shipment1.OrganizationAddressCollection);
			Data.HeaderDataObject.WayBillNumber = "MB1";
			var newFactory = new UniversalObjectFactory();

			AssertNoExceptionThrown(() => new WhsTransitReceiveConsolDataObjectReader(Data.HeaderDataObject, Logger, newFactory).ReadIntoBusinessObject());
			AssertNoExceptionThrown(() => newFactory.SaveForTesting());

			var receiveASNsReimport = newFactory.Load<WhsItemReceiveASN>(new ZQuery());
			AssertEquals("Unmatched ASN should be replaced.", 1, receiveASNsReimport.Length);
			AssertNotEquals("Unmatched ASN should be replaced.", unmatchedASN.PK, receiveASNsReimport[0].PK);
			AssertEquals("The new ASN should be for the consol.", "MB1", receiveASNsReimport[0].WRP_VehicleReference);
			AssertNotNull("Pack1 should be assigned to the new ASN.",
				receiveASNsReimport[0].PackageStates.SingleOrDefault(p => p.Package.KP_PackageID == "Pack1"));
			var rtusReimport = newFactory.Load<WhsItemReceiveTransportationUnit>(new ZQuery());
			AssertEquals("The Planned RTU with arrived packages should not be deleted.", 1, rtusReimport.Length);
			AssertEquals("The Planned RTU with arrived packages should not be deleted.", unmatchedRTU.PK, rtusReimport[0].PK);
			AssertNotNull("Pack2 should be assigned to the RTU.",
				rtusReimport[0].PackageStates.SingleOrDefault(p => p.Package.KP_PackageID == "Pack2"));
			var pivotsReimport = newFactory.Load<WhsItemReceiveASNRTUPivot>(new ZQuery());
			AssertEquals("Pivot should be deleted.", 0, pivotsReimport.Length);
		}

		[TestDate(2020, 9, 24)]
		public void TestDeleteASN_PlannedRTUHasOtherPivots_DoesNotDeleteRTU()
		{
			// Import a container with a single package
			Data.SetupForForwardingImport();
			Data.HeaderDataObject.SetContainerCollection(() => new DataObjectList<Container>
			{
				new Container { Link = 1, ContainerNumber = "XXX", ContainerType = DefaultContainerType },
			});

			var shipment1 = Data.CreateShipmentWithPackages("HB1", "Pack1");
			shipment1.PackingLineCollection[0].ContainerLink = 1; // Pack1

			Data.HeaderDataObject.SetSubShipmentCollection(() => new DataObjectList<Shipment> { shipment1 });
			Data.HeaderDataObject.WayBillNumber = "MB1";
			Logger.TopLevelDataObject = Data.HeaderDataObject;
			Data.HeaderDataObject.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.ATW } } });
			Data.HeaderDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			new WhsTransitReceiveConsolDataObjectReader(Data.HeaderDataObject, Logger, Factory).ReadIntoBusinessObject();

			var rtu = Factory.Load<WhsItemReceiveTransportationUnit>(new ZQuery()).SingleOrDefault();
			AssertNotNull("Precondition: An RTU is created.", rtu);
			AssertEquals("Precondition: ASN is created.", 1, Factory.Load<WhsItemReceiveASN>(new ZQuery()).Length);
			AssertEquals("Precondition: Pivot is created.", 1, Factory.Load<WhsItemReceiveASNRTUPivot>(new ZQuery()).Length);

			// Attach the RTU to an unrelated ASN
			var warehouse = Data.WarehouseINTHEMSYD;
			var unmatchedASN = Helper.CreateReceiveASN("YYY", warehouse.PK);
			unmatchedASN.WRP_VehicleReference = "YYY";
			var pivot = Helper.CreateReceiveASNRTUPivot(rtu.PK, unmatchedASN.PK);

			Factory.SaveForTesting();

			// Now replace the ASN for container XXX with an ASN for loose packages
			shipment1.PackingLineCollection[0].ContainerLink = null;

			Logger.TopLevelDataObject = Data.HeaderDataObject;
			var newFactory = new UniversalObjectFactory();
			new WhsTransitReceiveConsolDataObjectReader(Data.HeaderDataObject, Logger, newFactory).ReadIntoBusinessObject();

			var asns = newFactory.Load<WhsItemReceiveASN>(new ZQuery());
			AssertEquals("The ASN should be replaced.", 3, newFactory.Load<WhsItemReceiveASN>(new ZQuery()).Length);
			AssertContainsExactElementsInAnyOrder("The ASN for container XXX should not be deleted.", new[] { "MB1", "YYY", "XXX" }, asns.Select(asn => asn.WRP_VehicleReference));
			var rtusReimport = newFactory.Load<WhsItemReceiveTransportationUnit>(new ZQuery());
			AssertEquals("The RTU should remain.", 1, rtusReimport.Length);
			AssertEquals("The RTU should be unchanged.", rtu.PK, rtusReimport[0].PK);
			var pivotsReimport = newFactory.Load<WhsItemReceiveASNRTUPivot>(new ZQuery());
			AssertEquals("The RTU should have 2 pivots to ASN.", 2, pivotsReimport.Length);

			var emptyContainerASN = asns.Where(asn => asn.WRP_VehicleReference == "XXX").FirstOrDefault();
			AssertEquals("The RTU's pivot to ASN XXX should remain.", 1, pivotsReimport.Count(p => p.WAR_WRH_TransitReceiveTransportationUnit == rtu.PK && p.WAR_WRP_TransitReceiveASN == emptyContainerASN?.PK));
			AssertEquals("The RTU's pivot to ASN YYY should remain.", pivot.PK,
				pivotsReimport.SingleOrDefault(p => p.WAR_WRH_TransitReceiveTransportationUnit == rtu.PK && p.WAR_WRP_TransitReceiveASN == unmatchedASN.PK)?.PK);
			AssertNoExceptionThrown(() => newFactory.SaveForTesting());
		}

		#endregion

		#region TestConsolImport_RunSheet

		public void TestConsolImport_RunSheet()
		{
			Data.SetupForForwardingImport();
			Data.HeaderDataObject.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.ATW } } });
			Data.HeaderDataObject.DataContext.AddDataSource(DataContextType.TransportConsignmentRunSheet, "TR1");
			Data.HeaderDataObject.VoyageFlightNo = "V1";
			var shipment1 = Data.CreateShipmentWithPackages("A123", "Pack1");

			Data.HeaderDataObject.SetSubShipmentCollection(() => new DataObjectList<Shipment> { shipment1 });
			Data.HeaderDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);

			Logger.TopLevelDataObject = Data.HeaderDataObject;
			var consol = new WhsTransitReceiveConsolDataObjectReader(Data.HeaderDataObject, Logger, Factory).ReadIntoBusinessObject();

			var receiveASN1 = Factory.Load<WhsItemReceiveASN>(new ZQuery());
			AssertEquals("One expected packing should be created", 1, receiveASN1.Length);
			AssertEquals("WaybillParent", receiveASN1[0].WRP_VehicleReference);
			var rtus = Factory.Load<WhsItemReceiveTransportationUnit>(new ZQuery());
			AssertEquals("No RTUs should be created from a consol with no containers.", 0, rtus.Length);
			var pivots = Factory.Load<WhsItemReceiveASNRTUPivot>(new ZQuery());
			AssertEquals("No RTUs should be created from a consol with no containers.", 0, pivots.Length);
		}

		#endregion

		#region TestTransportCompany

		public void TestImportASN_ASNTransport_ViaConsol_WithoutDepartureTransportCompany()
		{
			Data.SetupForForwardingImport();
			var consolDO = Data.HeaderDataObject;

			var shipmentDO = Data.CreateShipmentWithPackages("1", "Pack1");
			var org = Data.Orgs.Warehouse_WUFSHIJNB;
			org.AddressType = AddressTypes.PickupLocalCartage;
			shipmentDO.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { org });
			consolDO.SetSubShipmentCollection(() => new DataObjectList<Shipment>() { shipmentDO });
			consolDO.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { Data.Orgs.Warehouse_WUFSHIJNB });

			Logger.TopLevelDataObject = consolDO;
			AssertEquals("Precondition: No receive ASNs in the system.", 0, Factory.Load<WhsItemReceiveASN>(new ZQuery()).Length);

			var expectedPackingsByContainer = new Dictionary<ZInt, IColumnIndexer>();
			var reader = new WhsTransitReceiveConsolDataObjectReader(consolDO, Logger, Factory);
			reader.ReadIntoBusinessObject();

			var receiveASNs = Factory.Load<WhsItemReceiveASN>(new ZQuery());
			AssertEquals("1 expected packing must be created.", 1, receiveASNs.Length);

			var receiveAsn = receiveASNs.Single();
			var addresses = Factory.Load<JobDocAddress>(new ZQuery(JobDocAddressSchema.E2_ParentID, receiveAsn.PK));
			var transportDocAddress = addresses.SingleOrDefault(doc => doc.E2_AddressType == DocAddressTypes.Codes.TransportCompanyDocumentaryAddress);

			AssertNull("ASN should not have Transport company for there is no PickupLocalCartage in Shipment", transportDocAddress);
		}

		public void TestImportASN_ASNTransport_ViaConsol()
		{
			Data.SetupForForwardingImport();
			var consolDO = Data.HeaderDataObject;
			var org = Data.Orgs.Warehouse_INTHEMSYD;
			org.AddressType = nameof(DocAddressType.DepartureCFSLocalTransportAddress);
			consolDO.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { org, Data.Orgs.Warehouse_WUFSHIJNB });

			var shipmentDO = Data.CreateShipmentWithPackages("1", "Pack1");
			consolDO.SetSubShipmentCollection(() => new DataObjectList<Shipment>() { shipmentDO });

			Logger.TopLevelDataObject = consolDO;
			AssertEquals("Precondition: No receive ASNs in the system.", 0, Factory.Load<WhsItemReceiveASN>(new ZQuery()).Length);

			var reader = new WhsTransitReceiveConsolDataObjectReader(consolDO, Logger, Factory);
			reader.ReadIntoBusinessObject();

			var receiveASNs = Factory.Load<WhsItemReceiveASN>(new ZQuery());
			AssertEquals("1 expected packing must be created.", 1, receiveASNs.Length);

			var receiveAsn = receiveASNs.Single();
			var addresses = Factory.Load<JobDocAddress>(new ZQuery(JobDocAddressSchema.E2_ParentID, receiveAsn.PK));
			var transportDocAddress = addresses.Single(doc => doc.E2_AddressType == DocAddressTypes.Codes.TransportCompanyDocumentaryAddress);

			AssertEquals("Transport company in ASN should be same as Consol's DepartureCFSLocalTransportAddress", Data.Orgs.INTHEMSYD.MainAddress.PK, transportDocAddress.E2_OA_Address);
		}

		#endregion

		#region TestCreateASN

		public void TestReceiveASNCreation_FromConsol()
		{
			Data.SetupForForwardingImport();

			var shipment = Data.CreateShipmentWithPackages("A123", "Pack1", "Pack2");
			shipment.OrganizationAddressCollection[2] = Data.Orgs.Warehouse_CRAHOLSYD;
			Data.HeaderDataObject.SetSubShipmentCollection(() => new DataObjectList<Shipment> { shipment });
			Data.HeaderDataObject.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.ATW, ServiceCode = ServiceCodeType.TWR } } });
			Data.HeaderDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			// The consol has an empty container which should be used for the ASN
			Data.HeaderDataObject.SetContainerCollection(() => new DataObjectList<Container> { new Container { Link = 1, ContainerNumber = "CNT-1", ContainerType = DefaultContainerType } });
			Data.HeaderDataObject.SetOrganizationAddressCollection(() => Data.ShipmentDataObject.OrganizationAddressCollection);
			var orgAddressForConsol = Data.HeaderDataObject.OrganizationAddressCollection.FirstOrDefault(nameof(DocAddressType.ArrivalCFSAddress));
			AssertEquals("Precondition: consol level arrival CFS is pointing to INTHEMSYD", Data.Orgs.Warehouse_INTHEMSYD.OrganizationCode, orgAddressForConsol.OrganizationCode);
			var orgAddressForShipment = shipment.OrganizationAddressCollection.FirstOrDefault(nameof(DocAddressType.ArrivalCFSAddress));
			AssertEquals("Precondition: shipment level arrival CFS is pointing to CRAHOLSYD", Data.Orgs.Warehouse_CRAHOLSYD.OrganizationCode, orgAddressForShipment.OrganizationCode);

			Logger.TopLevelDataObject = Data.HeaderDataObject;
			var consol = new WhsTransitReceiveConsolDataObjectReader(Data.HeaderDataObject, Logger, Factory).ReadIntoBusinessObject();
			AssertNoExceptionThrown("Assert Data can save properly", () => Factory.SaveForTesting());

			var receiveASNs = Factory.Load<WhsItemReceiveASN>(new ZQuery());
			AssertEquals("A Consol ASN and a Container ASN should be created", 2, receiveASNs.Length);

			var consolASN = receiveASNs.Where(asn => asn.WRP_VehicleReference == "WaybillParent").FirstOrDefault();
			AssertNotNull("ASN should be created for the consol", consolASN);

			var containerASN = receiveASNs.Where(asn => asn.WRP_VehicleReference == "CNT-1").FirstOrDefault();
			AssertNotNull("ASN should be created for the empty container", containerASN);

			AssertEquals("Should use Master Bill.", Data.HeaderDataObject.WayBillNumber, consolASN.WRP_VehicleReference);
			AssertEquals("Should have set the Booking Party.", GlbCompany.CurrentCompany.OrgProxy.PK, consolASN.BookingPartyDocAddress?.Organisation.PK);
			AssertEquals("ASN's intended warehouse should be taken from consol (INTHEMSYD).", Data.WarehouseINTHEMSYD.PK, consolASN.WRP_WW_IntendedWarehouse);

			var rtus = Factory.Load<WhsItemReceiveTransportationUnit>(new ZQuery());
			AssertEquals("A single RTU should be created.", 1, rtus.Length);

			var pivots = Factory.Load<WhsItemReceiveASNRTUPivot>(new ZQuery());
			AssertEquals("A RTU-ASN pivot should be created.", 1, pivots.Length);
			AssertEquals("A RTU-ASN pivot should be created.", containerASN.PK, pivots.Single().WAR_WRP_TransitReceiveASN);
			AssertEquals("A RTU-ASN pivot should be created.", rtus.Single().PK, pivots.Single().WAR_WRH_TransitReceiveTransportationUnit);
		}

		public void TestReceiveASNCreation_FromContainer()
		{
			Data.SetupForForwardingImport();
			var containerDO = new Container { Link = 1, ContainerNumber = "CNT-1", ContainerType = DefaultContainerType };
			var shipment = Data.CreateShipmentWithPackages("A123", "Pack1", "Pack2");
			shipment.PackingLineCollection[0].ContainerLink = 1; // Pack1
			shipment.PackingLineCollection[1].ContainerLink = 1; // Pack2
			shipment.OrganizationAddressCollection[2] = Data.Orgs.Warehouse_CRAHOLSYD;
			Data.HeaderDataObject.SetSubShipmentCollection(() => new DataObjectList<Shipment> { shipment });
			Data.HeaderDataObject.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.ATW, ServiceCode = ServiceCodeType.TWR } } });
			Data.HeaderDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			Data.HeaderDataObject.SetContainerCollection(() => new DataObjectList<Container> { containerDO });
			Data.HeaderDataObject.SetOrganizationAddressCollection(() => Data.ShipmentDataObject.OrganizationAddressCollection);
			var orgAddressForConsol = Data.HeaderDataObject.OrganizationAddressCollection.FirstOrDefault(nameof(DocAddressType.ArrivalCFSAddress));
			AssertEquals("Precondition: consol level arrival CFS is pointing to INTHEMSYD", Data.Orgs.Warehouse_INTHEMSYD.OrganizationCode, orgAddressForConsol.OrganizationCode);
			var orgAddressForShipment = shipment.OrganizationAddressCollection.FirstOrDefault(nameof(DocAddressType.ArrivalCFSAddress));
			AssertEquals("Precondition: shipment level arrival CFS is pointing to CRAHOLSYD", Data.Orgs.Warehouse_CRAHOLSYD.OrganizationCode, orgAddressForShipment.OrganizationCode);

			Logger.TopLevelDataObject = Data.HeaderDataObject;
			var consol = new WhsTransitReceiveConsolDataObjectReader(Data.HeaderDataObject, Logger, Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			var receiveASN = Factory.Load<WhsItemReceiveASN>(new ZQuery()).SingleOrDefault();
			AssertNotNull("A single ASN should be created for the container", receiveASN);
			AssertEquals("Should use Container Number", "CNT-1", receiveASN.WRP_VehicleReference);

			var rtu = Factory.Load<WhsItemReceiveTransportationUnit>(new ZQuery()).SingleOrDefault();
			AssertNotNull(rtu);
			AssertEquals("RTU should use Container Number", "CNT-1", rtu.WRH_VehicleReference);
			AssertEquals("RTU's warehouse should be taken from consol (INTHEMSYD)", Data.WarehouseINTHEMSYD.PK, rtu.WRH_WW_Warehouse);

			AssertNoExceptionThrown("Assert Data can save properly", () => Factory.SaveForTesting());
		}

		#endregion

		#region TestConsolWarehouseMatchingFallback

		public void TestConsolWarehouseMatchingFallback()
		{
			Data.SetupForForwardingImport();

			var shipment = Data.CreateShipmentWithPackages("A123", "Pack1", "Pack2");
			shipment.OrganizationAddressCollection[2] = Data.Orgs.Warehouse_CRAHOLSYD;
			Data.ShipmentDataObject.SetSubShipmentCollection(() => new DataObjectList<Shipment> { shipment });
			Data.ShipmentDataObject.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.DTW, ServiceCode = ServiceCodeType.TWR } } });
			var orgAddressForConsol = Data.ShipmentDataObject.OrganizationAddressCollection.FirstOrDefault(nameof(DocAddressType.DepartureCFSAddress));
			AssertEquals("Precondition: consol level departure CFS is pointing to WUFSHIJNB", orgAddressForConsol.OrganizationCode, Data.Orgs.Warehouse_WUFSHIJNB.OrganizationCode);
			var orgAddressForShipment = shipment.OrganizationAddressCollection.FirstOrDefault(nameof(DocAddressType.DepartureCFSAddress));
			AssertEquals("Precondition: shipment level departure CFS is pointing to CRAHOLSYD", orgAddressForShipment.OrganizationCode, Data.Orgs.Warehouse_CRAHOLSYD.OrganizationCode);

			Logger.TopLevelDataObject = Data.ShipmentDataObject;
			var whsTransitReceiveConsol1 = new WhsTransitReceiveConsolDataObjectReader(Data.ShipmentDataObject, Logger, Factory).ReadIntoBusinessObject();

			AssertEquals(1, whsTransitReceiveConsol1.PopulatedConsignmentsForTesting.Length);
			AssertEquals("Consignment's intended warehouse should be taken from consol (WUFSHIJNB)", Data.Warehouse.PK, whsTransitReceiveConsol1.PopulatedConsignmentsForTesting[0].WRC_WW_IntendedWarehouse);

			// removing consol-level Departure CFS address (which is used to find warehouse)
			Data.ShipmentDataObject.OrganizationAddressCollection.Remove(orgAddressForConsol);
			AssertExceptionThrown(typeof(DataObjectReadFailureException), () => new WhsTransitReceiveConsolDataObjectReader(Data.ShipmentDataObject, Logger, Factory).ReadIntoBusinessObject());
		}

		#endregion

		#region TestLogMessage

		public void TestLogMessage()
		{
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

			var consolDataObjectReader = new WhsTransitReceiveConsolDataObjectReader(consol, Logger, Factory);
			consolDataObjectReader.ReadIntoBusinessObject();
			AssertEquals("Log message must not have dispatch consol loading text.",
@"Information - Searching for Transit Warehouse matching Departure CFS Address 'WUFSHIJNB - Level 2, Building G'.
Information - Matching 'DepartureCFSAddress':- Matched to 'WUFSHIJNB' by code, address 'SC1' (only address).
Information - Found Warehouse TWH from Departure CFS Address.
Information - Recipient Role Service is 'DTW - Departure Transit Warehouse'.
Error - Could not find 'Booking Party':- Organization Code is empty in 'SendingForwarderAddress' for this Departure Transit Warehouse, and UXML does not originate from this system.
Information - Data Source is 'ForwardingShipment - S1000000'.
Information - Searching for Receive Consignment for 'ForwardingShipment - S1000000'.
Information - No matching Receive Consignment found, creating new Receive Consignment.
Information - Populating Receive Consignment...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Populating Packages for Receive Consignment...
Information - Successfully loaded matching PkgPackageJob.
Information - Populating PkgPackageJob...
Information - No matching PkgPackage found, creating new PkgPackage.
Information - Populating PkgPackage...
Information - No matching WhsItemPackageState found, creating new WhsItemPackageState.
Information - Populating WhsItemPackageState...
Information - Populating Addresses for Receive Consignment...
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Matching 'ConsignorPickupDeliveryAddress':- Matched to 'WUFSHIJNB' by code, address 'SC1' (only address).
Information - Matching 'ConsigneeDocumentaryAddress':- Matched to 'INTHEMSYD' by code, address '' (only address).
Information - Added Receive Consignment RC00000001 from UniversalShipment.
Error - Could not find 'Booking Party':- Organization Code is empty in 'SendingForwarderAddress' for this Departure Transit Warehouse, and UXML does not originate from this system.
Information - ASN matching for the Consolidation of Consignments failed - No ASN created within the last 30 days could be found with the provided Warehouse TWH, Master Bill , Consol Number C123, and reference C123.
Information - No matching Receive ASN found, creating new Receive ASN.
Information - Populating Receive ASN...
Information - ASN reference set to Consol Number C123.
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Successfully loaded matching WhsItemPackageState.
Information - Populating WhsItemPackageState...
Information - Attaching the following packages to Advanced Shipping Notice TRT00000001:
Package        RCN
PKG1           WAYBILLNO1 (RC00000001)
Information - Added Receive ASN TRT00000001 from UniversalShipment.
Information - Updated record from UniversalShipment.", Logger.Logs);
		}

		public void TestLogMessage_GivenMultipleShipmentErrors()
		{
			Data.SetupForForwardingImport();
			var consol = new Shipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			consol.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.DTW, ServiceCode = ServiceCodeType.TWD } } });
			consol.DataContext.AddDataSource(DataContextType.ForwardingConsol, "C123");
			consol.DataContext.AddDataTarget(DataContextType.ForwardingConsol, "C123");
			consol.ContainerMode = new ContainerMode { Code = "FCL" };
			consol.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { Data.Orgs.Consignor_CRAHOLSYD, Data.Orgs.Consignee_INTHEMSYD, Data.Orgs.Warehouse_WUFSHIJNB, Data.Orgs.Warehouse_INTHEMSYD });

			consol.SetContainerCollection(() => new DataObjectList<Container>());
			var container = Data.CreateContainer("A", 1);
			consol.ContainerCollection.Add(container);

			// shipment
			consol.SetSubShipmentCollection(() => new DataObjectList<Shipment>());
			var shipment1 = Data.CreateShipmentWithPackages("WAYBILLNO1", "PKG1");
			Data.SetupNewDataContextWithDataSource(shipment1, shipmentNumber: "S1");
			consol.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S1");
			consol.SubShipmentCollection.Add(shipment1);

			var shipment2 = Data.CreateShipmentWithPackages("WAYBILLNO2", "PKG2");
			Data.SetupNewDataContextWithDataSource(shipment2, shipmentNumber: "S2");
			shipment2.ShipmentType = new CodeDescriptionPair() { Code = Constants.ShipmentTypes.AssemblyMaster };
			consol.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S2");
			consol.SubShipmentCollection.Add(shipment2);
			Data.SetupForForwardingImport();

			var shipment3 = Data.CreateShipmentWithPackages("WAYBILLNO3", "PKG3", "PKG4");
			Data.SetupNewDataContextWithDataSource(shipment3, shipmentNumber: "S3");
			shipment3.PackingLineCollection[0].ReferenceNumber = "DUPLICATE_ID";
			shipment3.PackingLineCollection[1].ReferenceNumber = "DUPLICATE_ID";
			consol.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S3");
			consol.SubShipmentCollection.Add(shipment3);

			Logger.TopLevelDataObject = consol;

			var consolDataObjectReader = new WhsTransitReceiveConsolDataObjectReader(consol, Logger, Factory);
			AssertExceptionThrown<DataObjectReadFailureException>("Should throw an error listing the errors for each shipment",
@"The following Sub Shipments encountered errors when importing ForwardingConsol - C123:
Data Source
ForwardingShipment - S2
ForwardingShipment - S3
Error importing ForwardingShipment - S2:
Sub Shipments are mandatory for Master Shipments but none were specified for Master Shipment 'S2'.

Error importing ForwardingShipment - S3:
Reference number DUPLICATE_ID is used for more than one package on a shipment WAYBILLNO3. Please provide a unique reference number or leave it empty.
",
() => consolDataObjectReader.ReadIntoBusinessObject());

			AssertEquals("Log message must have the combined error messages.",
@"Information - Searching for Transit Warehouse matching Departure CFS Address 'WUFSHIJNB - Level 2, Building G'.
Information - Matching 'DepartureCFSAddress':- Matched to 'WUFSHIJNB' by code, address 'SC1' (only address).
Information - Found Warehouse TWH from Departure CFS Address.
Information - Recipient Role Service is 'DTW - Departure Transit Warehouse'.
Error - Could not find 'Booking Party':- Organization Code is empty in 'SendingForwarderAddress' for this Departure Transit Warehouse, and UXML does not originate from this system.
Information - Data Source is 'ForwardingShipment - S1'.
Information - Searching for Receive Consignment for 'ForwardingShipment - S1'.
Information - No matching Receive Consignment found, creating new Receive Consignment.
Information - Populating Receive Consignment...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Populating Packages for Receive Consignment...
Information - Successfully loaded matching PkgPackageJob.
Information - Populating PkgPackageJob...
Information - No matching PkgPackage found, creating new PkgPackage.
Information - Populating PkgPackage...
Information - No matching WhsItemPackageState found, creating new WhsItemPackageState.
Information - Populating WhsItemPackageState...
Information - Populating Addresses for Receive Consignment...
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Matching 'ConsignorPickupDeliveryAddress':- Matched to 'WUFSHIJNB' by code, address 'SC1' (only address).
Information - Matching 'ConsigneeDocumentaryAddress':- Matched to 'INTHEMSYD' by code, address '' (only address).
Information - Added Receive Consignment RC00000001 from UniversalShipment.
Error - Sub Shipments are mandatory for Master Shipments but none were specified for Master Shipment 'S2'.
Error - Could not find 'Booking Party':- Organization Code is empty in 'SendingForwarderAddress' for this Departure Transit Warehouse, and UXML does not originate from this system.
Information - Data Source is 'ForwardingShipment - S3'.
Information - Searching for Receive Consignment for 'ForwardingShipment - S3'.
Information - No matching Receive Consignment found, creating new Receive Consignment.
Information - Populating Receive Consignment...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Populating Packages for Receive Consignment...
Information - Successfully loaded matching PkgPackageJob.
Information - Populating PkgPackageJob...
Information - No matching PkgPackage found, creating new PkgPackage.
Information - Populating PkgPackage...
Information - Successfully loaded matching PkgPackage.
Information - Populating PkgPackage...
Error - Reference number DUPLICATE_ID is used for more than one package on a shipment WAYBILLNO3. Please provide a unique reference number or leave it empty.", Logger.Logs);
		}

		#endregion

		#region TestLogMessage_CombinedInstructions

		public void TestLogMessage_CombinedInstructions()
		{
			Data.SetupForForwardingImport();
			var consol = new Shipment(DefaultDataObjectWriterStrategy.TestInstance) { DataContext = DataContextFactory.New() };
			consol.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.DTW, ServiceCode = ServiceCodeType.TWX } } });
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

			var consolDataObjectReader = new WhsTransitReceiveConsolDataObjectReader(consol, Logger, Factory);
			consolDataObjectReader.ReadIntoBusinessObject();
			AssertEquals("Log message must not have dispatch consol loading text.",
@"Information - Searching for Transit Warehouse matching Departure CFS Address 'WUFSHIJNB - Level 2, Building G'.
Information - Matching 'DepartureCFSAddress':- Matched to 'WUFSHIJNB' by code, address 'SC1' (only address).
Information - Found Warehouse TWH from Departure CFS Address.
Information - ================================Processing Receive Instruction================================
Information - Recipient Role Service is 'DTW - Departure Transit Warehouse'.
Error - Could not find 'Booking Party':- Organization Code is empty in 'SendingForwarderAddress' for this Departure Transit Warehouse, and UXML does not originate from this system.
Information - Data Source is 'ForwardingShipment - S1000000'.
Information - Searching for Receive Consignment for 'ForwardingShipment - S1000000'.
Information - No matching Receive Consignment found, creating new Receive Consignment.
Information - Populating Receive Consignment...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Populating Packages for Receive Consignment...
Information - Successfully loaded matching PkgPackageJob.
Information - Populating PkgPackageJob...
Information - No matching PkgPackage found, creating new PkgPackage.
Information - Populating PkgPackage...
Information - No matching WhsItemPackageState found, creating new WhsItemPackageState.
Information - Populating WhsItemPackageState...
Information - Populating Addresses for Receive Consignment...
Information - Matching 'ConsignorDocumentaryAddress':- Matched to 'CRAHOLSYD' by code, address '' (only address).
Information - Matching 'ConsignorPickupDeliveryAddress':- Matched to 'WUFSHIJNB' by code, address 'SC1' (only address).
Information - Matching 'ConsigneeDocumentaryAddress':- Matched to 'INTHEMSYD' by code, address '' (only address).
Information - Added Receive Consignment RC00000001 from UniversalShipment.
Error - Could not find 'Booking Party':- Organization Code is empty in 'SendingForwarderAddress' for this Departure Transit Warehouse, and UXML does not originate from this system.
Information - ASN matching for the Consolidation of Consignments failed - No ASN created within the last 30 days could be found with the provided Warehouse TWH, Master Bill , Consol Number C123, and reference C123.
Information - No matching Receive ASN found, creating new Receive ASN.
Information - Populating Receive ASN...
Information - ASN reference set to Consol Number C123.
Information - No matching CusEntryNumber found, creating new CusEntryNumber.
Information - Populating CusEntryNumber...
Information - Successfully loaded matching WhsItemPackageState.
Information - Populating WhsItemPackageState...
Information - Attaching the following packages to Advanced Shipping Notice TRT00000001:
Package        RCN
PKG1           WAYBILLNO1 (RC00000001)
Information - Added Receive ASN TRT00000001 from UniversalShipment.
Information - Successfully saved Receive ASN TRT00000001 with 3 x CusEntryNumber, 1 x PkgPackage, 1 x PkgPackageJob, 1 x WhsItemPackageState, 1 x WhsItemReceiveConsignment.
Information - ================================Processing Dispatch Instruction================================
Information - Recipient Role Service is 'DTW - Departure Transit Warehouse'.
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
PKG1           WAYBILLNO1 (RC00000001)
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
Package        RCN                        DCN
PKG1           WAYBILLNO1 (RC00000001)    WAYBILLNO1 (DC00000001)
Information - Added Dispatch Load List DLL00000001 from UniversalShipment.
Information - No matching Dispatch Transportation Unit found, creating new Dispatch Transportation Unit.
Information - Populating Dispatch Transportation Unit...
Error - Could not find 'Booking Party':- Organization Code is empty in 'SendingForwarderAddress' for this Departure Transit Warehouse, and UXML does not originate from this system.
Information - No matching PkgPackage found, creating new PkgPackage.
Information - Populating PkgPackage...
Information - Successfully loaded matching Container Type.
Information - Updated record from UniversalShipment.
Information - Updated record from UniversalShipment.", Logger.Logs);
		}

		#endregion

		#region TestConsolImportsWithoutShipments

		public void TestConsolImportsWithoutShipments()
		{
			Data.SetupForForwardingImport();
			Data.HeaderDataObject.SetContainerCollection(() => new DataObjectList<Container>
			{
				new Container { Link = 1, ContainerNumber = "CNT-1", ContainerType = DefaultContainerType },
			});

			Data.HeaderDataObject.SetSubShipmentCollection(() => new DataObjectList<Shipment>());

			Logger.TopLevelDataObject = Data.HeaderDataObject;
			Data.HeaderDataObject.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.ATW, ServiceCode = ServiceCodeType.TWX } } });
			Data.HeaderDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);

			AssertExceptionThrown(typeof(DataObjectReadFailureException),
				@"No Receive Consignments (e.g. Forwarding Shipments) were included in the Receive Instruction.",
				() => new WhsTransitReceiveConsolDataObjectReader(Data.HeaderDataObject, Logger, Factory).ReadIntoBusinessObject());
		}

		public void TestConsolImportsWithSubShipmentCollectionIsNull()
		{
			Data.SetupForForwardingImport();
			Data.HeaderDataObject.SetContainerCollection(() => new DataObjectList<Container>
			{
				new Container { Link = 1, ContainerNumber = "CNT-1", ContainerType = DefaultContainerType },
			});

			Data.HeaderDataObject.SetSubShipmentCollection(() => null);

			Logger.TopLevelDataObject = Data.HeaderDataObject;
			Data.HeaderDataObject.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.ATW, ServiceCode = ServiceCodeType.TWX } } });
			Data.HeaderDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);

			AssertExceptionThrown(typeof(DataObjectReadFailureException),
				@"No Receive Consignments (e.g. Forwarding Shipments) were included in the Receive Instruction.",
				() => new WhsTransitReceiveConsolDataObjectReader(Data.HeaderDataObject, Logger, Factory).ReadIntoBusinessObject());
		}

		#endregion

		#region TestGateBooking

		public void TestGateBookingWithConsolImportWithLoosePackages()
		{
			Data.SetupForForwardingImport();
			var shipment = Data.HeaderDataObject;
			var subShipment = Data.CreateShipmentWithPackages("A123", "Pack1", "Pack2");
			shipment.SetSubShipmentCollection(() => new DataObjectList<Shipment> { subShipment });

			// Fill Gate Booking Information
			Data.SetupHeaderObjectForGateBooking(shipment);
			Data.SetupNewDataContextWithDataSource(shipment, gateBookingNumber: "GTB001", isArrival: true, keepExistingDataContext: true);
			var gateBookingSubShipment = Data.CreateSubShipmentForGateBookingHeaderObject("GMB001", subShipment.WayBillNumber.Value, true);
			Data.SetupNewDataContextWithDataSource(gateBookingSubShipment, gateBookingNumber: "GTB001", isArrival: true, keepExistingDataContext: true);
			shipment.SubShipmentCollection.Add(gateBookingSubShipment);
			shipment.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.ATW } } });
			shipment.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			Logger.TopLevelDataObject = shipment;

			var consol = new WhsTransitReceiveConsolDataObjectReader(shipment, Logger, Factory).ReadIntoBusinessObject();

			(WhsItemReceiveASN[] asns, WhsItemReceiveConsignment[] rcns, WhsItemReceiveASNRTUPivot[] pivots, WhsItemReceiveTransportationUnit[] rtus) =
				AssertEntitiesCount(asnCount: 1, rcnCount: 1, asnRtuPivotCount: 1, rtuCount: 1);

			var packageJob1 = Factory.Load<PkgPackageJob>(new ZQuery(PkgPackageJobSchema.KJ_ParentID, consol.PopulatedConsignmentsForTesting[0].PK)).Single();
			AssertEquals("There should be 2 packages on the package Job.", 2, packageJob1.Packages.Count);
			AssertContainsExactElementsInAnyOrder(new ZString[] { "Pack1", "Pack2" }, packageJob1.Packages.Select(p => p.KP_PackageID));

			var rtuASNPK = pivots[0].WAR_WRP_TransitReceiveASN;
			var allPackageStates = consol.PopulatedConsignmentsForTesting.SelectMany(rc => rc.PackageStates).ToArray();
			Assert("All packages should have the same Expected Packing.", allPackageStates.All(p => p.WPS_WRP_ReceiveExpectedPacking == rtuASNPK));

			var vehicleRtu = rtus[0];
			var links = Factory.Load<StmUniversalJobLink>(new ZQuery());
			AssertUniversalJobLink(links, null, vehicleRtu.PK, WhsItemReceiveTransportationUnitSchema.Constants.Prefix, DataContextType.GateBooking, "GTB001", "EDI", "DAT", "EDI");
			AssertUniversalJobLink(links, null, vehicleRtu.PK, WhsItemReceiveTransportationUnitSchema.Constants.Prefix, DataContextType.GateMovementBooking, "GMB001", "EDI", "DAT", "EDI");
			AssertUniversalJobLinkShouldBeUnique(links, DataContextType.GateBooking, "GTB001");
			AssertUniversalJobLinkShouldBeUnique(links, DataContextType.GateMovementBooking, "GMB001");
			AssertGateRelatedBookingConfirmedLogs(vehicleRtu,
				expectedGateBookingConfirmedLogRef: $"{vehicleRtu.WRH_ReferenceNumber}|FAC=WHS|LOC=ThereVille|MST=GateBooking|RFN=GTB001|TYP=WhsItemReceiveTransportationUnit|WHS=TW2",
				expectedGateMovementBookingConfirmedLogRef: $"{vehicleRtu.WRH_ReferenceNumber}|FAC=WHS|LOC=ThereVille|MST=GateMovementBooking|RFN=GMB001|TYP=WhsItemReceiveTransportationUnit|WHS=TW2");
		}

		public void TestGateBookingAfterConsolImportWithLoosePackagesCore_BookingReferenceIsAdvancedShippingNotice()
		{
			TestGateBookingAfterConsolImportWithLoosePackagesCore(ReferenceNumberTypes.AdvancedShippingNotice);
		}

		public void TestGateBookingAfterConsolImportWithLoosePackagesCore_BookingReferenceIsForwardingConsolNumber()
		{
			TestGateBookingAfterConsolImportWithLoosePackagesCore(ReferenceNumberTypes.ForwardingConsolNumber);
		}

		public void TestGateBookingAfterConsolImportWithLoosePackagesCore_BookingReferenceIsMasterBill()
		{
			TestGateBookingAfterConsolImportWithLoosePackagesCore(ReferenceNumberTypes.MasterBill);
		}

		void TestGateBookingAfterConsolImportWithLoosePackagesCore(ReferenceNumberTypes referenceNumberType)
		{
			Data.SetupForForwardingImport();
			var shipment = Data.HeaderDataObject;

			var subShipment = Data.CreateShipmentWithPackages("A123", "Pack1", "Pack2");
			shipment.SetSubShipmentCollection(() => new DataObjectList<Shipment> { subShipment });
			Logger.TopLevelDataObject = shipment;
			shipment.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.ATW } } });
			shipment.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);

			var consol = new WhsTransitReceiveConsolDataObjectReader(shipment, Logger, Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();
			(WhsItemReceiveASN[] asns, WhsItemReceiveConsignment[] rcns, WhsItemReceiveASNRTUPivot[] pivots, WhsItemReceiveTransportationUnit[] rtus) =
				AssertEntitiesCount(asnCount: 1, rcnCount: 1, asnRtuPivotCount: 0, rtuCount: 0);
			var asn = asns[0];
			AssertPackageStatesUnderTheRCNsShouldLinkToAnSpecificASN(consol.PopulatedConsignmentsForTesting.Select(c => c.PK).ToArray(), asn.PK);

			// Handle Gate booking
			var bookingReference = referenceNumberType == ReferenceNumberTypes.AdvancedShippingNotice ? asn.WRP_ReferenceNumber : referenceNumberType == ReferenceNumberTypes.ForwardingConsolNumber ? "C1000000" : shipment.WayBillNumber.Value;
			var gateShipment = Data.HeaderDataObjectForGateBooking_ATW;
			var gateSubShipment = Data.CreateSubShipmentForGateBookingHeaderObject("GMB001", bookingReference, true);
			gateShipment.SetSubShipmentCollection(() => new DataObjectList<Shipment>() { gateSubShipment });
			Logger.TopLevelDataObject = gateShipment;
			new WhsTransitReceiveConsolDataObjectReader(gateShipment, Logger, Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			(asns, rcns, pivots, rtus) = AssertEntitiesCount(asnCount: 1, rcnCount: 1, asnRtuPivotCount: 1, rtuCount: 1);
			var rtu1 = rtus[0];
			var vehicleASNPk = pivots.FirstOrDefault(p => p.WAR_WRH_TransitReceiveTransportationUnit == rtu1.PK).WAR_WRP_TransitReceiveASN;
			AssertPackageStatesUnderTheRCNsShouldLinkToAnSpecificASN(rcns.Select(c => c.PK).ToArray(), vehicleASNPk);
			var transportCompanyDocAddress = rtu1.DocAddresses.FindByDocAddressType(DocAddressType.TransportCompanyDocumentaryAddress);
			AssertEquals("Transport Company Address should be populated.", "INTHEMSYD", transportCompanyDocAddress.Organisation.OH_Code);
			AssertEquals("Vehicle Registration Number should be populated.", "DEF-023", rtu1.WRH_VehicleReference);
			AssertEquals("Driver should be populated.", "DRIVER1", rtu1.WRH_SignedBy);
			var links = Factory.Load<StmUniversalJobLink>(new ZQuery());
			AssertUniversalJobLink(links, null, rtu1.PK, WhsItemReceiveTransportationUnitSchema.Constants.Prefix, DataContextType.GateBooking, "GTB001", "EDI", "DAT", "EDI");
			AssertUniversalJobLink(links, null, rtu1.PK, WhsItemReceiveTransportationUnitSchema.Constants.Prefix, DataContextType.GateMovementBooking, "GMB001", "EDI", "DAT", "EDI");
			AssertGateRelatedBookingConfirmedLogs(rtu1,
				expectedGateBookingConfirmedLogRef: $"{rtu1.WRH_ReferenceNumber}|FAC=WHS|LOC=ThereVille|MST=GateBooking|RFN=GTB001|TYP=WhsItemReceiveTransportationUnit|WHS=TW2",
				expectedGateMovementBookingConfirmedLogRef: $"{rtu1.WRH_ReferenceNumber}|FAC=WHS|LOC=ThereVille|MST=GateMovementBooking|RFN=GMB001|TYP=WhsItemReceiveTransportationUnit|WHS=TW2");
			// Send the updated booking again with updated vehicle and driver info
			bookingReference = referenceNumberType == ReferenceNumberTypes.AdvancedShippingNotice ? asns[0].WRP_ReferenceNumber : referenceNumberType == ReferenceNumberTypes.ForwardingConsolNumber ? "C1000000" : shipment.WayBillNumber.Value;
			TestGateBooking_UpdateBookingCore(bookingReference, vehicleASNPk, rtu1.PK);
		}

		public void TestGateBookingAfterShipmentImportWithLoosePackages_MatchedASNHasArrivedPackage() => TestGateBookingAfterShipmentImportWithLoosePackages_MatchedASNCore(hasArrivedPackage: true);

		public void TestGateBookingAfterShipmentImportWithLoosePackages_MatchedASNPlannedRTUGatedIn() => TestGateBookingAfterShipmentImportWithLoosePackages_MatchedASNCore(plannedRTUGatedIn: true);

		public void TestGateBookingAfterShipmentImportWithLoosePackages_MatchedASN() => TestGateBookingAfterShipmentImportWithLoosePackages_MatchedASNCore();

		void TestGateBookingAfterShipmentImportWithLoosePackages_MatchedASNCore(bool hasArrivedPackage = false, bool plannedRTUGatedIn = false)
		{
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", Data.WarehouseINTHEMSYD.PK, "EXTREF1");
			var asnAlreadyInWarehouse = Helper.CreateReceiveASN("TRT0000001", Data.WarehouseINTHEMSYD.PK);
			asnAlreadyInWarehouse.WRP_VehicleReference = "DEF-023";
			Helper.CreatePackageState(rcn, 1, "PKG", "PKG-1", TransitWarehouseStatuses.Codes.Booked);

			if (plannedRTUGatedIn)
			{
				var plannedRTU = Helper.CreateReceiveTransportationUnit("RTU001", Data.WarehouseINTHEMSYD.PK, Data.WarehouseINTHEMSYD.DefaultLocation.PK, "RTU001");
				plannedRTU.WRH_GateInTime = new ZDateTimeOffset(2025, 2, 3, 1, 1, 0);
				Helper.CreateReceiveASNRTUPivot(plannedRTU.PK, asnAlreadyInWarehouse.PK);
			}

			if (hasArrivedPackage)
			{
				var receiveUnit = Helper.CreateReceiveTransportationUnit("RTU002", Data.WarehouseINTHEMSYD.PK, Data.WarehouseINTHEMSYD.DefaultLocation.PK, "RTU002");
				Helper.CreateReceiveASNRTUPivot(receiveUnit.PK, asnAlreadyInWarehouse.PK);
				receiveUnit.WRH_GateInTime = new ZDateTimeOffset(2025, 2, 3, 1, 1, 0);
				receiveUnit.WRH_UnloadStartTime = new ZDateTimeOffset(2025, 2, 3, 1, 2, 0);
				var packageState = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit, receiveASN: asnAlreadyInWarehouse);
			}

			Factory.SaveForTesting();

			var bookingReference = rcn.WRC_JobID;
			var gateShipment = Data.HeaderDataObjectForGateBooking_ATW;
			var gateSubShipment = Data.CreateSubShipmentForGateBookingHeaderObject("GMB001", bookingReference, true);
			gateShipment.SetSubShipmentCollection(() => new DataObjectList<Shipment>() { gateSubShipment });
			Logger.TopLevelDataObject = gateShipment;
			new WhsTransitReceiveConsolDataObjectReader(gateShipment, Logger, Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			var asns = Factory.Load<WhsItemReceiveASN>(new ZQuery());
			if (plannedRTUGatedIn || hasArrivedPackage)
			{
				AssertEquals("Cannot match existing ASN, so there should be two ASNs", 2, asns.Length);
				AssertEquals("The ASN should have same vehicle reference number as gate booking vehicle.", true, asns.All(a => a.WRP_VehicleReference == "DEF-023"));
			}
			else
			{
				AssertEquals("Cannot match existing ASN, so there should be two ASNs", 1, asns.Length);
			}
		}

		public void TestGateBookingAfterShipmentImportWithLoosePackages_BookingReferenceIsReceiveConsignment()
		{
			TestGateBookingAfterShipmentImportWithLoosePackagesCore(ReferenceNumberTypes.ReceiveConsignment);
		}

		public void TestGateBookingAfterShipmentImportWithLoosePackages_BookingReferenceIsForwardingShipmentNumber()
		{
			TestGateBookingAfterShipmentImportWithLoosePackagesCore(ReferenceNumberTypes.ForwardingShipmentNumber);
		}

		public void TestGateBookingAfterShipmentImportWithLoosePackages_BookingReferenceIsHouseBill()
		{
			TestGateBookingAfterShipmentImportWithLoosePackagesCore(ReferenceNumberTypes.HouseBill);
		}

		void TestGateBookingAfterShipmentImportWithLoosePackagesCore(ReferenceNumberTypes referenceNumberType)
		{
			Data.SetupForForwardingImport();
			var houseBillNumber = "HSB001";

			var warehouse = Data.WarehouseINTHEMSYD;
			var rcn1 = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			rcn1.WRC_HouseBillNumber = houseBillNumber;
			var packageState1 = Helper.CreatePackageState(rcn1, 1, "PKG", "PKG-1", TransitWarehouseStatuses.Codes.Booked);
			var shipmentNumber = Helper.CreateAdditionalReference(rcn1, "S123", WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber).CE_EntryNum;
			Factory.SaveForTesting();

			// Handle Gate booking
			var bookingReference = referenceNumberType == ReferenceNumberTypes.ReceiveConsignment ? rcn1.WRC_JobID : referenceNumberType == ReferenceNumberTypes.ForwardingShipmentNumber ? shipmentNumber : houseBillNumber;
			var gateShipment = Data.HeaderDataObjectForGateBooking_ATW;
			var gateSubShipment = Data.CreateSubShipmentForGateBookingHeaderObject("GMB001", bookingReference, true);
			gateShipment.SetSubShipmentCollection(() => new DataObjectList<Shipment>() { gateSubShipment });
			Logger.TopLevelDataObject = gateShipment;
			new WhsTransitReceiveConsolDataObjectReader(gateShipment, Logger, Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			(WhsItemReceiveASN[] asns, WhsItemReceiveConsignment[] rcns, WhsItemReceiveASNRTUPivot[] pivots, WhsItemReceiveTransportationUnit[] rtus) =
				AssertEntitiesCount(asnCount: 1, rcnCount: 1, asnRtuPivotCount: 1, rtuCount: 1);
			var rtu1 = rtus[0];
			var vehicleASNPk = pivots.FirstOrDefault(p => p.WAR_WRH_TransitReceiveTransportationUnit == rtu1.PK).WAR_WRP_TransitReceiveASN;
			AssertPackageStatesUnderTheRCNsShouldLinkToAnSpecificASN(rcns.Select(c => c.PK).ToArray(), vehicleASNPk);
			AssertEquals("Transport Company Address should be populated.", "INTHEMSYD", rtu1.DocAddresses.FindByDocAddressType(DocAddressType.TransportCompanyDocumentaryAddress).Organisation.OH_Code);
			AssertEquals("Vehicle Registration Number should be populated.", "DEF-023", rtu1.WRH_VehicleReference);
			AssertEquals("Driver should be populated.", "DRIVER1", rtu1.WRH_SignedBy);
			var links = Factory.Load<StmUniversalJobLink>(new ZQuery());
			AssertUniversalJobLink(links, null, rtu1.PK, WhsItemReceiveTransportationUnitSchema.Constants.Prefix, DataContextType.GateBooking, "GTB001", "EDI", "DAT", "EDI");
			AssertUniversalJobLink(links, null, rtu1.PK, WhsItemReceiveTransportationUnitSchema.Constants.Prefix, DataContextType.GateMovementBooking, "GMB001", "EDI", "DAT", "EDI");
			AssertUniversalJobLinkShouldBeUnique(links, DataContextType.GateBooking, "GTB001");
			AssertUniversalJobLinkShouldBeUnique(links, DataContextType.GateMovementBooking, "GMB001");
			AssertGateRelatedBookingConfirmedLogs(rtu1,
				expectedGateBookingConfirmedLogRef: $"{rtu1.WRH_ReferenceNumber}|FAC=WHS|LOC=ThereVille|MST=GateBooking|RFN=GTB001|TYP=WhsItemReceiveTransportationUnit|WHS=TW2",
				expectedGateMovementBookingConfirmedLogRef: $"{rtu1.WRH_ReferenceNumber}|FAC=WHS|LOC=ThereVille|MST=GateMovementBooking|RFN=GMB001|TYP=WhsItemReceiveTransportationUnit|WHS=TW2");
			TestGateBooking_UpdateBookingCore(bookingReference, vehicleASNPk, rtu1.PK);
		}

		void TestGateBooking_UpdateBookingCore(ZString bookingReference, ZGuid vehicleASNPk, ZGuid originalRTUPk)
		{
			// Send the updated booking again with updated vehicle and driver info
			var newTransportCompanyDocAddress = OrganizationAddressTestHelper.GetNewAddressData_CRAHOLSYD(DocAddressType.TransportCompanyDocumentaryAddress);
			var newUniversalObjectFactory = NewUniversalObjectFactory();
			var newGateShipment = Data.CreateHeaderObjectForGateBooking("ABC-123", "RTRK", "NewDriver", newTransportCompanyDocAddress);
			Data.SetupNewDataContextWithDataSource(newGateShipment, gateBookingNumber: "GTB001", isArrival: true);
			var newSubShipment = Data.CreateSubShipmentForGateBookingHeaderObject("GMB001", bookingReference, true);
			newGateShipment.SetSubShipmentCollection(() => new DataObjectList<Shipment>() { newSubShipment });
			Logger.TopLevelDataObject = newGateShipment;
			new WhsTransitReceiveConsolDataObjectReader(newGateShipment, Logger, newUniversalObjectFactory).ReadIntoBusinessObject();
			newUniversalObjectFactory.SaveForTesting();

			var (asns, rcns, pivots, rtus) = AssertEntitiesCount(asnCount: 1, rcnCount: 1, asnRtuPivotCount: 1, rtuCount: 1, newUniversalObjectFactory);
			var updatedRTU = rtus[0];
			var newVehicleASNPk = pivots.FirstOrDefault(p => p.WAR_WRH_TransitReceiveTransportationUnit == updatedRTU.PK).WAR_WRP_TransitReceiveASN;
			AssertPackageStatesUnderTheRCNsShouldLinkToAnSpecificASN(rcns.Select(c => c.PK).ToArray(), newVehicleASNPk, newUniversalObjectFactory);

			AssertEquals("Transport Company Address should be updated.", "CRAHOLSYD", updatedRTU.DocAddresses.FindByDocAddressType(DocAddressType.TransportCompanyDocumentaryAddress).Organisation.OH_Code);
			AssertEquals("Vehicle Registration Number should be updated.", "ABC-123", updatedRTU.WRH_VehicleReference);
			AssertEquals("Driver should be updated.", "NewDriver", updatedRTU.WRH_SignedBy);
			AssertNotEquals("New vehicle ASN should be different from the original vehicle ASN.", vehicleASNPk, newVehicleASNPk);
			AssertEquals("Vehicle RTU should remain the same.", originalRTUPk, updatedRTU.PK);
			var links = newUniversalObjectFactory.Load<StmUniversalJobLink>(new ZQuery());
			AssertUniversalJobLink(links, null, updatedRTU.PK, WhsItemReceiveTransportationUnitSchema.Constants.Prefix, DataContextType.GateBooking, "GTB001", "EDI", "DAT", "EDI");
			AssertUniversalJobLink(links, null, updatedRTU.PK, WhsItemReceiveTransportationUnitSchema.Constants.Prefix, DataContextType.GateMovementBooking, "GMB001", "EDI", "DAT", "EDI");
			AssertUniversalJobLinkShouldBeUnique(links, DataContextType.GateBooking, "GTB001");
			AssertUniversalJobLinkShouldBeUnique(links, DataContextType.GateMovementBooking, "GMB001");
			AssertGateRelatedBookingConfirmedLogs(updatedRTU,
				expectedGateBookingConfirmedLogRef: $"{updatedRTU.WRH_ReferenceNumber}|FAC=WHS|LOC=ThereVille|MST=GateBooking|RFN=GTB001|TYP=WhsItemReceiveTransportationUnit|WHS=TW2",
				expectedGateMovementBookingConfirmedLogRef: $"{updatedRTU.WRH_ReferenceNumber}|FAC=WHS|LOC=ThereVille|MST=GateMovementBooking|RFN=GMB001|TYP=WhsItemReceiveTransportationUnit|WHS=TW2");
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

		public void TestGateBookingWithABlindContainer_ContainerRCNFullMatch()
		{
			TestGateBookingWithABlindContainerCore(true, ReferenceNumberTypes.HouseBill, false);
		}

		public void TestGateBookingWithABlindContainer_ContainerRCNPartialMatch()
		{
			TestGateBookingWithABlindContainerCore(false, ReferenceNumberTypes.HouseBill, false);
		}

		public void TestGateBookingWithABlindContainer_BookingReferenceIsContainerNumber()
		{
			TestGateBookingWithABlindContainerCore(false, ReferenceNumberTypes.ContainerNumber, false);
		}

		void TestGateBookingWithABlindContainerCore(bool containerFullMatch, ReferenceNumberTypes referenceNumberType, bool hasConsol)
		{
			Data.SetupForForwardingImport();
			var shipment = Data.HeaderDataObject;
			var subShipment = Data.CreateShipmentWithPackages("A123", "Pack1", "Pack2");
			shipment.SetSubShipmentCollection(() => new DataObjectList<Shipment> { subShipment });
			Logger.TopLevelDataObject = shipment;
			shipment.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.ATW } } });
			shipment.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);

			if (hasConsol)
			{
				new WhsTransitReceiveConsolDataObjectReader(shipment, Logger, Factory).ReadIntoBusinessObject();
			}
			else
			{
				new WhsTransitReceiveConsignmentDataObjectReader(subShipment, Logger, Factory).ReadIntoBusinessObject();
			}

			Factory.SaveForTesting();

			(WhsItemReceiveASN[] asns, WhsItemReceiveConsignment[] rcns, WhsItemReceiveASNRTUPivot[] pivots, WhsItemReceiveTransportationUnit[] rtus) =
			AssertEntitiesCount(asnCount: hasConsol ? 1 : 0, rcnCount: 1, asnRtuPivotCount: 0, rtuCount: 0);

			// Handle Gate booking
			var gateShipment = Data.HeaderDataObjectForGateBooking_ATW;
			var gateSubShipment = Data.CreateSubShipmentForGateBookingHeaderObject("GMB001", referenceNumberType == ReferenceNumberTypes.ContainerNumber ? new ZString("CNT-1") : referenceNumberType == ReferenceNumberTypes.HouseBill ? subShipment.WayBillNumber.Value : shipment.WayBillNumber.Value, true, new ZString[] { "Pack1", "Pack2" });
			gateSubShipment.PackingLineCollection[0].ContainerLink = 1;

			if (containerFullMatch)
			{
				gateSubShipment.PackingLineCollection[1].ContainerLink = 1;
			}

			gateShipment.SetSubShipmentCollection(() => new DataObjectList<Shipment>() { gateSubShipment });
			gateSubShipment.SetContainerCollection(() => new DataObjectList<Container>()
			{
				new() { Link = 1, ContainerNumber = "CNT-1", ContainerType = DefaultContainerType },
			});

			Logger.TopLevelDataObject = gateShipment;
			if (referenceNumberType == ReferenceNumberTypes.ContainerNumber)
			{
				AssertExceptionThrown<DataObjectReadFailureException>("Blind Container matching with Container Number is not supported.",
					@"Could not find any package state using Container Number, House Bill and Master Bill.", () => new WhsTransitReceiveConsolDataObjectReader(gateShipment, Logger, Factory).ReadIntoBusinessObject());
			}
			else
			{
				new WhsTransitReceiveConsolDataObjectReader(gateShipment, Logger, Factory).ReadIntoBusinessObject();
				Factory.SaveForTesting();

				(asns, rcns, pivots, rtus) = AssertEntitiesCount(asnCount: containerFullMatch ? 2 : 1, rcnCount: 1, asnRtuPivotCount: 2, rtuCount: 2);
				var container1Rtu = rtus.FirstOrDefault(rtu => rtu.WRH_VehicleReference == "CNT-1");
				var container1AsnPivot = pivots.FirstOrDefault(pivot => pivot.WAR_WRH_TransitReceiveTransportationUnit == container1Rtu.PK);
				var vehicle1Rtu = rtus.FirstOrDefault(rtu => rtu.WRH_VehicleReference == "DEF-023");
				var vehicle1Asn = asns.FirstOrDefault(asn => asn.WRP_VehicleReference == "DEF-023");
				var vehicle1AsnPivot = pivots.FirstOrDefault(pivot => pivot.PK != container1AsnPivot.PK);

				if (containerFullMatch)
				{
					var container1Asn = asns.FirstOrDefault(asn => asn.WRP_VehicleReference == "CNT-1");

					AssertEquals("Container ASN and RTU should link to the container pivot", true,
					container1Asn.PK == container1AsnPivot.WAR_WRP_TransitReceiveASN &&
					container1Rtu.PK == container1AsnPivot.WAR_WRH_TransitReceiveTransportationUnit);
					AssertEquals("Vehicle ASN and RTU should link to the vehicle pivot", true,
						vehicle1Asn.PK == vehicle1AsnPivot.WAR_WRP_TransitReceiveASN &&
						vehicle1Rtu.PK == vehicle1AsnPivot.WAR_WRH_TransitReceiveTransportationUnit);
					AssertPackageStatesUnderTheRCNsShouldLinkToAnSpecificASN(rcns.Select(c => c.PK).ToArray(), container1Asn.PK);
					var container1RtuPkgState = GetRtuPackageState(container1Rtu.PK);
					AssertRtuPackageStatesShouldLinkToVehicleASN(container1RtuPkgState, vehicle1Asn.PK);
				}
				else
				{
					AssertEquals("Container RTU and Vehicle ASN should link to the container pivot", true,
					vehicle1Asn.PK == container1AsnPivot.WAR_WRP_TransitReceiveASN &&
					container1Rtu.PK == container1AsnPivot.WAR_WRH_TransitReceiveTransportationUnit);
					AssertEquals("Vehicle ASN and RTU should link to the vehicle pivot", true,
						vehicle1Asn.PK == vehicle1AsnPivot.WAR_WRP_TransitReceiveASN &&
						vehicle1Rtu.PK == vehicle1AsnPivot.WAR_WRH_TransitReceiveTransportationUnit);
					AssertPackageStatesUnderTheRCNsShouldLinkToAnSpecificASN(rcns.Select(c => c.PK).ToArray(), vehicle1Asn.PK);
					var container1RtuPkgState = GetRtuPackageState(container1Rtu.PK);
					AssertNull("No package state should be created for the container", container1RtuPkgState);
				}

				var links = Factory.Load<StmUniversalJobLink>(new ZQuery());
				AssertUniversalJobLink(links, null, vehicle1Rtu.PK, WhsItemReceiveTransportationUnitSchema.Constants.Prefix, DataContextType.GateBooking, "GTB001", "EDI", "DAT", "EDI");
				AssertUniversalJobLink(links, null, container1Rtu.PK, WhsItemReceiveTransportationUnitSchema.Constants.Prefix, DataContextType.GateMovementBooking, "GMB001", "EDI", "DAT", "EDI");
				AssertUniversalJobLinkShouldBeUnique(links, DataContextType.GateBooking, "GTB001");
				AssertUniversalJobLinkShouldBeUnique(links, DataContextType.GateMovementBooking, "GMB001");
				AssertGateRelatedBookingConfirmedLog(vehicle1Rtu, nameof(DataContextType.GateBooking), expectedLogReference: $"{vehicle1Rtu.WRH_ReferenceNumber}|FAC=WHS|LOC=ThereVille|MST=GateBooking|RFN=GTB001|TYP=WhsItemReceiveTransportationUnit|WHS=TW2");
				AssertGateRelatedBookingConfirmedLog(container1Rtu, nameof(DataContextType.GateMovementBooking), expectedLogReference: $"{container1Rtu.WRH_ReferenceNumber}|FAC=WHS|LOC=ThereVille|MST=GateMovementBooking|RFN=GMB001|TYP=WhsItemReceiveTransportationUnit|WHS=TW2");
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
			Data.SetupForForwardingImport();
			var shipment = Data.HeaderDataObject;
			shipment.SetContainerCollection(() => new DataObjectList<Container>()
			{
				new() { Link = 1, ContainerNumber = "CNT-1", ContainerType = DefaultContainerType },
			});

			var subShipment = Data.CreateShipmentWithPackages("A123", "Pack1", "Pack2");
			subShipment.PackingLineCollection[0].ContainerLink = 1;
			subShipment.PackingLineCollection[1].ContainerLink = 1;
			shipment.SetSubShipmentCollection(() => new DataObjectList<Shipment> { subShipment });
			Logger.TopLevelDataObject = shipment;
			shipment.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.ATW } } });
			shipment.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			shipment.DataContext.AddDataTarget(DataContextType.ForwardingConsol, "C001");
			new WhsTransitReceiveConsolDataObjectReader(shipment, Logger, Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			// Handle Gate booking with container: CNT-1
			var gateShipment = Data.HeaderDataObjectForGateBooking_ATW;
			var gateSubShipment = Data.CreateSubShipmentForGateBookingHeaderObject("GMB001", referenceNumberType == ReferenceNumberTypes.ContainerNumber ? new ZString("CNT-2") : referenceNumberType == ReferenceNumberTypes.HouseBill ? subShipment.WayBillNumber.Value : shipment.WayBillNumber.Value, true);
			gateShipment.SetSubShipmentCollection(() => new DataObjectList<Shipment>() { gateSubShipment });
			gateSubShipment.SetContainerCollection(() => new DataObjectList<Container>()
			{
				new() { Link = 1, ContainerNumber = "CNT-2", ContainerType = DefaultContainerType },
			});
			Logger.TopLevelDataObject = gateShipment;
			var reader = new WhsTransitReceiveConsolDataObjectReader(gateShipment, Logger, Factory);

			if (referenceNumberType == ReferenceNumberTypes.MasterBill || referenceNumberType == ReferenceNumberTypes.HouseBill)
			{
				AssertExceptionThrown<DataObjectReadFailureException>("Exception should throw as there is no matching container in TWH.",
				"Gate Booking has a container and failed to find a matching ASN with the same container.",
				() => reader.ReadIntoBusinessObject());
			}
			else if (referenceNumberType == ReferenceNumberTypes.ContainerNumber)
			{
				AssertExceptionThrown<DataObjectReadFailureException>("Exception should throw as there is no matching container in TWH with the Booking Conifrmation Reference.",
					@"Could not find any package state using Container Number, House Bill and Master Bill.", () => reader.ReadIntoBusinessObject());
			}
		}

		public void TestGateBookingWithNoValidSubShipment()
		{
			Data.SetupForForwardingImport();
			var shipment = Data.HeaderDataObject;
			shipment.SetContainerCollection(() => new DataObjectList<Container>()
			{
				new() { Link = 1, ContainerNumber = "CNT-1", ContainerType = DefaultContainerType },
			});

			var subShipment = Data.CreateShipmentWithPackages("A123", "Pack1", "Pack2");
			subShipment.PackingLineCollection[0].ContainerLink = 1;
			subShipment.PackingLineCollection[1].ContainerLink = 1;
			shipment.SetSubShipmentCollection(() => new DataObjectList<Shipment> { subShipment });
			Logger.TopLevelDataObject = shipment;
			shipment.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.ATW } } });
			shipment.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			new WhsTransitReceiveConsolDataObjectReader(shipment, Logger, Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			// Handle First Gate booking with container: CNT-1
			var gateShipment = Data.HeaderDataObjectForGateBooking_ATW;
			var gateSubShipment = Data.CreateSubShipmentForGateBookingHeaderObject("GMB001", shipment.WayBillNumber.Value, true);
			// Make sub-shipment as an invalid one
			gateSubShipment.DataContext.ClearDataSourceCollection();
			gateShipment.SetSubShipmentCollection(() => new DataObjectList<Shipment>() { gateSubShipment });
			gateSubShipment.SetContainerCollection(() => new DataObjectList<Container>()
			{
				new() { Link = 1, ContainerNumber = "CNT-2", ContainerType = DefaultContainerType },
			});
			Logger.TopLevelDataObject = gateShipment;
			var reader = new WhsTransitReceiveConsolDataObjectReader(gateShipment, Logger, Factory);
			AssertExceptionThrown<DataObjectReadFailureException>("Exception should throw as there is no valid Sub-shipment in UXML.",
				"Gate Movement Booking is not provided in UXML.",
				() => reader.ReadIntoBusinessObject());
		}

		public void TestGateBookingWithNoContainerButTheMatchingASNWithAContainer()
		{
			Data.SetupForForwardingImport();
			var shipment = Data.HeaderDataObject;
			shipment.SetContainerCollection(() => new DataObjectList<Container>()
			{
				new() { Link = 1, ContainerNumber = "CNT-1", ContainerType = DefaultContainerType },
			});

			var subShipment = Data.CreateShipmentWithPackages("A123", "Pack1", "Pack2");
			subShipment.PackingLineCollection[0].ContainerLink = 1;
			subShipment.PackingLineCollection[1].ContainerLink = 1;
			shipment.SetSubShipmentCollection(() => new DataObjectList<Shipment> { subShipment });
			Logger.TopLevelDataObject = shipment;
			shipment.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.ATW } } });
			shipment.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			new WhsTransitReceiveConsolDataObjectReader(shipment, Logger, Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			// Handle First Gate booking with container: CNT-1
			var gateShipment = Data.HeaderDataObjectForGateBooking_ATW;
			var gateSubShipment = Data.CreateSubShipmentForGateBookingHeaderObject("GMB001", shipment.WayBillNumber.Value, true);
			gateShipment.SetSubShipmentCollection(() => new DataObjectList<Shipment>() { gateSubShipment });
			Logger.TopLevelDataObject = gateShipment;
			var reader = new WhsTransitReceiveConsolDataObjectReader(gateShipment, Logger, Factory);
			AssertExceptionThrown<DataObjectReadFailureException>("Exception should throw as there is no matching container in TWH.",
				"Gate Booking has no container and failed to find a matching ASN without container.",
				() => reader.ReadIntoBusinessObject());
		}

		public void TestGateBookingsWithMatchingContainers()
		{
			Data.SetupForForwardingImport();
			var shipment = Data.HeaderDataObject;
			shipment.SetContainerCollection(() => new DataObjectList<Container>()
			{
				new() { Link = 1, ContainerNumber = "CNT-1", ContainerType = DefaultContainerType },
				new() { Link = 2, ContainerNumber = "CNT-2", ContainerType = DefaultContainerType }
			});

			var subShipment = Data.CreateShipmentWithPackages("A123", "Pack1", "Pack2", "Pack3");
			subShipment.PackingLineCollection[0].ContainerLink = 1;
			subShipment.PackingLineCollection[1].ContainerLink = 1;
			subShipment.PackingLineCollection[2].ContainerLink = 2;
			shipment.SetSubShipmentCollection(() => new DataObjectList<Shipment> { subShipment });
			Logger.TopLevelDataObject = shipment;
			shipment.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.ATW } } });
			shipment.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			new WhsTransitReceiveConsolDataObjectReader(shipment, Logger, Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			(WhsItemReceiveASN[] asns, WhsItemReceiveConsignment[] rcns, WhsItemReceiveASNRTUPivot[] pivots, WhsItemReceiveTransportationUnit[] rtus) =
				AssertEntitiesCount(asnCount: 2, rcnCount: 1, asnRtuPivotCount: 2, rtuCount: 2);
			var container1Asn = asns.FirstOrDefault(asn => asn.WRP_VehicleReference == "CNT-1");
			var container2Asn = asns.FirstOrDefault(asn => asn.WRP_VehicleReference == "CNT-2");
			var container1AsnPivot = pivots.FirstOrDefault(pivot => pivot.WAR_WRP_TransitReceiveASN == container1Asn.PK);
			var container2AsnPivot = pivots.FirstOrDefault(pivot => pivot.WAR_WRP_TransitReceiveASN == container2Asn.PK);
			var container1Rtu = rtus.FirstOrDefault(rtu => rtu.WRH_VehicleReference == "CNT-1");
			var container2Rtu = rtus.FirstOrDefault(rtu => rtu.WRH_VehicleReference == "CNT-2");

			// Handle First Gate booking with container: CNT-1
			var gateShipment = Data.HeaderDataObjectForGateBooking_ATW;
			var gateSubShipment = Data.CreateSubShipmentForGateBookingHeaderObject("GMB001", new ZString("CNT-1"), true);
			gateShipment.SetSubShipmentCollection(() => new DataObjectList<Shipment>() { gateSubShipment });
			gateSubShipment.SetContainerCollection(() => new DataObjectList<Container>()
			{
				new() { Link = 1, ContainerNumber = "CNT-1", ContainerType = DefaultContainerType },
			});
			Logger.TopLevelDataObject = gateShipment;
			new WhsTransitReceiveConsolDataObjectReader(gateShipment, Logger, Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			(asns, rcns, pivots, rtus) = AssertEntitiesCount(asnCount: 3, rcnCount: 1, asnRtuPivotCount: 3, rtuCount: 3);
			var vehicle1Rtu = rtus.FirstOrDefault(rtu => rtu.PK != container1Rtu.PK && rtu.PK != container2Rtu.PK);
			var vehicle1Asn = asns.FirstOrDefault(asn => asn.PK != container1Asn.PK && asn.PK != container2Asn.PK);
			var vehicle1AsnPivot = pivots.FirstOrDefault(pivot => pivot.PK != container1AsnPivot.PK && pivot.PK != container2AsnPivot.PK);
			AssertEquals("Vehicle ASN and RTU should link to the vehicle pivot", true,
				vehicle1Asn.PK == vehicle1AsnPivot.WAR_WRP_TransitReceiveASN &&
				vehicle1Rtu.PK == vehicle1AsnPivot.WAR_WRH_TransitReceiveTransportationUnit);
			var container1RtuPkgState = GetRtuPackageState(container1Rtu.PK);
			AssertRtuPackageStatesShouldLinkToVehicleASN(container1RtuPkgState, vehicle1Asn.PK);
			var links = Factory.Load<StmUniversalJobLink>(new ZQuery());
			AssertUniversalJobLink(links, null, vehicle1Rtu.PK, WhsItemReceiveTransportationUnitSchema.Constants.Prefix, DataContextType.GateBooking, "GTB001", "EDI", "DAT", "EDI");
			AssertUniversalJobLink(links, null, container1Rtu.PK, WhsItemReceiveTransportationUnitSchema.Constants.Prefix, DataContextType.GateMovementBooking, "GMB001", "EDI", "DAT", "EDI");
			AssertGateRelatedBookingConfirmedLog(vehicle1Rtu, nameof(DataContextType.GateBooking), expectedLogReference: $"{vehicle1Rtu.WRH_ReferenceNumber}|FAC=WHS|LOC=ThereVille|MST=GateBooking|RFN=GTB001|TYP=WhsItemReceiveTransportationUnit|WHS=TW2");
			AssertGateRelatedBookingConfirmedLog(container1Rtu, nameof(DataContextType.GateMovementBooking), expectedLogReference: $"{container1Rtu.WRH_ReferenceNumber}|FAC=WHS|LOC=ThereVille|MST=GateMovementBooking|RFN=GMB001|TYP=WhsItemReceiveTransportationUnit|WHS=TW2");

			// Handle Second Gate booking with container: CTN-2
			gateShipment.DataContext.DataSourceCollection.FirstOrDefault(ds => ds.Type.Value == new ZString(nameof(DataContextType.GateBooking))).Key = "GTB002";
			gateShipment.VehicleRun.CrewCollection.FirstOrDefault().FullName = "DRIVER2";
			gateShipment.PreCarriageShipmentCollection.FirstOrDefault().VehicleRun.Vehicle.Registration.Number = "ABC-123";
			var gateSubShipment2 = Data.CreateSubShipmentForGateBookingHeaderObject("GMB002", new ZString("CNT-2"), true);
			gateSubShipment2.SetContainerCollection(() => new DataObjectList<Container>()
			{
				new() { Link = 2, ContainerNumber = "CNT-2", ContainerType = DefaultContainerType },
			});
			gateShipment.SetSubShipmentCollection(() => new DataObjectList<Shipment>() { gateSubShipment2 });
			Logger.TopLevelDataObject = gateShipment;
			new WhsTransitReceiveConsolDataObjectReader(gateShipment, Logger, Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			(asns, rcns, pivots, rtus) = AssertEntitiesCount(asnCount: 4, rcnCount: 1, asnRtuPivotCount: 4, rtuCount: 4);
			var vehicle2Rtu = rtus.FirstOrDefault(rtu => rtu.PK != container1Rtu.PK && rtu.PK != container2Rtu.PK && rtu.PK != vehicle1Rtu.PK);
			var vehicle2Asn = asns.FirstOrDefault(asn => asn.PK != container1Asn.PK && asn.PK != container2Asn.PK && asn.PK != vehicle1Asn.PK);
			var vehicle2AsnPivot = pivots.FirstOrDefault(pivot => pivot.PK != container1AsnPivot.PK && pivot.PK != container2AsnPivot.PK && pivot.PK != vehicle1AsnPivot.PK);
			AssertEquals("Vehicle ASN and RTU should link to the vehicle pivot", true,
				vehicle2Asn.PK == vehicle2AsnPivot.WAR_WRP_TransitReceiveASN &&
				vehicle2Rtu.PK == vehicle2AsnPivot.WAR_WRH_TransitReceiveTransportationUnit);
			var container2RtuPkgState = GetRtuPackageState(container2Rtu.PK);
			AssertRtuPackageStatesShouldLinkToVehicleASN(container2RtuPkgState, vehicle2Asn.PK);
			links = Factory.Load<StmUniversalJobLink>(new ZQuery());
			AssertUniversalJobLink(links, null, vehicle2Rtu.PK, WhsItemReceiveTransportationUnitSchema.Constants.Prefix, DataContextType.GateBooking, "GTB002", "EDI", "DAT", "EDI");
			AssertUniversalJobLink(links, null, container2Rtu.PK, WhsItemReceiveTransportationUnitSchema.Constants.Prefix, DataContextType.GateMovementBooking, "GMB002", "EDI", "DAT", "EDI");
			AssertUniversalJobLinkShouldBeUnique(links, DataContextType.GateBooking, "GTB001");
			AssertUniversalJobLinkShouldBeUnique(links, DataContextType.GateBooking, "GTB002");
			AssertUniversalJobLinkShouldBeUnique(links, DataContextType.GateMovementBooking, "GMB001");
			AssertUniversalJobLinkShouldBeUnique(links, DataContextType.GateMovementBooking, "GMB002");
			AssertGateRelatedBookingConfirmedLog(vehicle2Rtu, nameof(DataContextType.GateBooking), expectedLogReference: $"{vehicle2Rtu.WRH_ReferenceNumber}|FAC=WHS|LOC=ThereVille|MST=GateBooking|RFN=GTB002|TYP=WhsItemReceiveTransportationUnit|WHS=TW2");
			AssertGateRelatedBookingConfirmedLog(container2Rtu, nameof(DataContextType.GateMovementBooking), expectedLogReference: $"{container2Rtu.WRH_ReferenceNumber}|FAC=WHS|LOC=ThereVille|MST=GateMovementBooking|RFN=GMB002|TYP=WhsItemReceiveTransportationUnit|WHS=TW2");
		}

		public void TestGateBookingsWithMatchingContainers_SameVehicle()
		{
			Data.SetupForForwardingImport();
			var shipment = Data.HeaderDataObject;
			shipment.OrganizationAddressCollection.Add(Data.Orgs.TransportCompanyDocAddress_INTHEMSYD);
			shipment.SetContainerCollection(() => new DataObjectList<Container>()
			{
				new() { Link = 1, ContainerNumber = "CNT-1", ContainerType = DefaultContainerType },
				new() { Link = 2, ContainerNumber = "CNT-2", ContainerType = DefaultContainerType }
			});

			var subShipment = Data.CreateShipmentWithPackages("A123", "Pack1", "Pack2", "Pack3");
			subShipment.PackingLineCollection[0].ContainerLink = 1;
			subShipment.PackingLineCollection[1].ContainerLink = 1;
			subShipment.PackingLineCollection[2].ContainerLink = 2;
			shipment.SetSubShipmentCollection(() => new DataObjectList<Shipment> { subShipment });
			Logger.TopLevelDataObject = shipment;
			shipment.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.ATW } } });
			shipment.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			new WhsTransitReceiveConsolDataObjectReader(shipment, Logger, Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			(WhsItemReceiveASN[] asns, WhsItemReceiveConsignment[] rcns, WhsItemReceiveASNRTUPivot[] pivots, WhsItemReceiveTransportationUnit[] rtus) =
				AssertEntitiesCount(asnCount: 2, rcnCount: 1, asnRtuPivotCount: 2, rtuCount: 2);
			var container1Asn = asns.FirstOrDefault(asn => asn.WRP_VehicleReference == "CNT-1");
			var container2Asn = asns.FirstOrDefault(asn => asn.WRP_VehicleReference == "CNT-2");
			var container1AsnPivot = pivots.FirstOrDefault(pivot => pivot.WAR_WRP_TransitReceiveASN == container1Asn.PK);
			var container2AsnPivot = pivots.FirstOrDefault(pivot => pivot.WAR_WRP_TransitReceiveASN == container2Asn.PK);
			var container1Rtu = rtus.FirstOrDefault(rtu => rtu.WRH_VehicleReference == "CNT-1");
			var container2Rtu = rtus.FirstOrDefault(rtu => rtu.WRH_VehicleReference == "CNT-2");

			// Handle First Gate booking with container: CNT-1
			var gateShipment = Data.HeaderDataObjectForGateBooking_ATW;
			var gateSubShipment = Data.CreateSubShipmentForGateBookingHeaderObject("GMB001", new ZString("CNT-1"), true);
			gateShipment.SetSubShipmentCollection(() => new DataObjectList<Shipment>() { gateSubShipment });
			gateSubShipment.SetContainerCollection(() => new DataObjectList<Container>()
			{
				new() { Link = 1, ContainerNumber = "CNT-1", ContainerType = DefaultContainerType },
			});
			Logger.TopLevelDataObject = gateShipment;
			new WhsTransitReceiveConsolDataObjectReader(gateShipment, Logger, Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			(asns, rcns, pivots, rtus) = AssertEntitiesCount(asnCount: 3, rcnCount: 1, asnRtuPivotCount: 3, rtuCount: 3);
			var vehicleRtu = rtus.FirstOrDefault(rtu => rtu.PK != container1Rtu.PK && rtu.PK != container2Rtu.PK);
			var vehicleAsn = asns.FirstOrDefault(asn => asn.PK != container1Asn.PK && asn.PK != container2Asn.PK);
			var vehicleAsnPivot = pivots.FirstOrDefault(pivot => pivot.PK != container1AsnPivot.PK && pivot.PK != container2AsnPivot.PK);
			AssertEquals("Vehicle ASN and RTU should link to the vehicle pivot", true,
				vehicleAsn.PK == vehicleAsnPivot.WAR_WRP_TransitReceiveASN &&
				vehicleRtu.PK == vehicleAsnPivot.WAR_WRH_TransitReceiveTransportationUnit);
			var container1RtuPkgState = GetRtuPackageState(container1Rtu.PK);
			AssertRtuPackageStatesShouldLinkToVehicleASN(container1RtuPkgState, vehicleAsn.PK);
			var links = Factory.Load<StmUniversalJobLink>(new ZQuery());
			AssertUniversalJobLink(links, null, vehicleRtu.PK, WhsItemReceiveTransportationUnitSchema.Constants.Prefix, DataContextType.GateBooking, "GTB001", "EDI", "DAT", "EDI");
			AssertUniversalJobLink(links, null, container1Rtu.PK, WhsItemReceiveTransportationUnitSchema.Constants.Prefix, DataContextType.GateMovementBooking, "GMB001", "EDI", "DAT", "EDI");

			// Handle Second Gate booking with container: CTN-2
			var gateSubShipment2 = Data.CreateSubShipmentForGateBookingHeaderObject("GMB002", new ZString("CNT-2"), true);
			gateSubShipment2.SetContainerCollection(() => new DataObjectList<Container>()
			{
				new() { Link = 2, ContainerNumber = "CNT-2", ContainerType = DefaultContainerType },
			});
			gateShipment.SetSubShipmentCollection(() => new DataObjectList<Shipment>() { gateSubShipment2 });
			Logger.TopLevelDataObject = gateShipment;
			new WhsTransitReceiveConsolDataObjectReader(gateShipment, Logger, Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			(asns, rcns, pivots, rtus) = AssertEntitiesCount(asnCount: 3, rcnCount: 1, asnRtuPivotCount: 3, rtuCount: 3);

			vehicleRtu = rtus.FirstOrDefault(rtu => rtu.PK == vehicleRtu.PK);
			vehicleAsn = asns.FirstOrDefault(asn => asn.PK == vehicleAsn.PK);
			vehicleAsnPivot = pivots.FirstOrDefault(pivot => pivot.PK == vehicleAsnPivot.PK);
			AssertEquals("Vehicle ASN and RTU should link to the vehicle pivot", true,
				vehicleAsn.PK == vehicleAsnPivot.WAR_WRP_TransitReceiveASN &&
				vehicleRtu.PK == vehicleAsnPivot.WAR_WRH_TransitReceiveTransportationUnit);
			var container2RtuPkgState = GetRtuPackageState(container2Rtu.PK);
			AssertRtuPackageStatesShouldLinkToVehicleASN(container1RtuPkgState, vehicleAsn.PK);
			AssertRtuPackageStatesShouldLinkToVehicleASN(container2RtuPkgState, vehicleAsn.PK);
			links = Factory.Load<StmUniversalJobLink>(new ZQuery());
			AssertUniversalJobLink(links, null, vehicleRtu.PK, WhsItemReceiveTransportationUnitSchema.Constants.Prefix, DataContextType.GateBooking, "GTB001", "EDI", "DAT", "EDI");
			AssertUniversalJobLink(links, null, container1Rtu.PK, WhsItemReceiveTransportationUnitSchema.Constants.Prefix, DataContextType.GateMovementBooking, "GMB001", "EDI", "DAT", "EDI");
			AssertUniversalJobLink(links, null, container2Rtu.PK, WhsItemReceiveTransportationUnitSchema.Constants.Prefix, DataContextType.GateMovementBooking, "GMB002", "EDI", "DAT", "EDI");
			AssertUniversalJobLinkShouldBeUnique(links, DataContextType.GateBooking, "GTB001");
			AssertUniversalJobLinkShouldBeUnique(links, DataContextType.GateMovementBooking, "GMB001");
			AssertUniversalJobLinkShouldBeUnique(links, DataContextType.GateMovementBooking, "GMB002");
		}

		public void TestGateBookingWithOneMatchingContainer()
		{
			Data.SetupForForwardingImport();
			var shipment = Data.HeaderDataObject;
			shipment.SetContainerCollection(() => new DataObjectList<Container>()
			{
				new() { Link = 1, ContainerNumber = "CNT-1", ContainerType = DefaultContainerType }
			});

			var subShipment = Data.CreateShipmentWithPackages("A123", "Pack1", "Pack2");
			subShipment.PackingLineCollection[0].ContainerLink = 1;
			subShipment.PackingLineCollection[1].ContainerLink = 1;
			shipment.SetSubShipmentCollection(() => new DataObjectList<Shipment> { subShipment });
			Logger.TopLevelDataObject = shipment;
			shipment.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.ATW } } });
			shipment.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			new WhsTransitReceiveConsolDataObjectReader(shipment, Logger, Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			(WhsItemReceiveASN[] asns, WhsItemReceiveConsignment[] rcns, WhsItemReceiveASNRTUPivot[] pivots, WhsItemReceiveTransportationUnit[] rtus) =
				AssertEntitiesCount(asnCount: 1, rcnCount: 1, asnRtuPivotCount: 1, rtuCount: 1);
			var container1Asn = asns.FirstOrDefault(asn => asn.WRP_VehicleReference == "CNT-1");
			var container1AsnPivot = pivots.FirstOrDefault(pivot => pivot.WAR_WRP_TransitReceiveASN == container1Asn.PK);
			var container1Rtu = rtus.FirstOrDefault(rtu => rtu.WRH_VehicleReference == "CNT-1");

			// Handle First Gate booking with container: CNT-1
			var gateShipment = Data.HeaderDataObjectForGateBooking_ATW;
			var gateSubShipment = Data.CreateSubShipmentForGateBookingHeaderObject("GMB001", new ZString("CNT-1"), true);
			gateShipment.SetSubShipmentCollection(() => new DataObjectList<Shipment>() { gateSubShipment });
			gateSubShipment.SetContainerCollection(() => new DataObjectList<Container>()
			{
				new() { Link = 1, ContainerNumber = "CNT-1", ContainerType = DefaultContainerType },
			});
			Logger.TopLevelDataObject = gateShipment;
			new WhsTransitReceiveConsolDataObjectReader(gateShipment, Logger, Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			(asns, rcns, pivots, rtus) = AssertEntitiesCount(asnCount: 2, rcnCount: 1, asnRtuPivotCount: 2, rtuCount: 2);
			var vehicle1Rtu = rtus.FirstOrDefault(rtu => rtu.PK != container1Rtu.PK);
			var vehicle1Asn = asns.FirstOrDefault(asn => asn.PK != container1Asn.PK);
			var vehicle1AsnPivot = pivots.FirstOrDefault(pivot => pivot.PK != container1AsnPivot.PK);

			AssertEquals("Vehicle ASN and RTU should link to the vehicle pivot", true,
				vehicle1Asn.PK == vehicle1AsnPivot.WAR_WRP_TransitReceiveASN &&
				vehicle1Rtu.PK == vehicle1AsnPivot.WAR_WRH_TransitReceiveTransportationUnit);
			var container1RtuPkgState = GetRtuPackageState(container1Rtu.PK);
			AssertRtuPackageStatesShouldLinkToVehicleASN(container1RtuPkgState, vehicle1Asn.PK);
			var links = Factory.Load<StmUniversalJobLink>(new ZQuery());
			AssertUniversalJobLink(links, null, vehicle1Rtu.PK, WhsItemReceiveTransportationUnitSchema.Constants.Prefix, DataContextType.GateBooking, "GTB001", "EDI", "DAT", "EDI");
			AssertUniversalJobLink(links, null, container1Rtu.PK, WhsItemReceiveTransportationUnitSchema.Constants.Prefix, DataContextType.GateMovementBooking, "GMB001", "EDI", "DAT", "EDI");
			AssertGateRelatedBookingConfirmedLog(vehicle1Rtu, nameof(DataContextType.GateBooking), expectedLogReference: $"{vehicle1Rtu.WRH_ReferenceNumber}|FAC=WHS|LOC=ThereVille|MST=GateBooking|RFN=GTB001|TYP=WhsItemReceiveTransportationUnit|WHS=TW2");
			AssertGateRelatedBookingConfirmedLog(container1Rtu, nameof(DataContextType.GateMovementBooking), expectedLogReference: $"{container1Rtu.WRH_ReferenceNumber}|FAC=WHS|LOC=ThereVille|MST=GateMovementBooking|RFN=GMB001|TYP=WhsItemReceiveTransportationUnit|WHS=TW2");

			// Handle Second Gate booking with the same container: CNT-1, updated vehicle and driver
			gateShipment.VehicleRun.CrewCollection.FirstOrDefault().FullName = "DRIVER2";
			gateShipment.PreCarriageShipmentCollection.FirstOrDefault().VehicleRun.Vehicle.Registration.Number = "ABC-123";
			Logger.TopLevelDataObject = gateShipment;
			new WhsTransitReceiveConsolDataObjectReader(gateShipment, Logger, Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			(asns, rcns, pivots, rtus) = AssertEntitiesCount(asnCount: 2, rcnCount: 1, asnRtuPivotCount: 2, rtuCount: 2);

			var vehicle2Rtu = rtus.FirstOrDefault(rtu => rtu.PK != container1Rtu.PK);
			var vehicle2Asn = asns.FirstOrDefault(asn => asn.PK != container1Asn.PK);
			var vehicle2AsnPivot = pivots.FirstOrDefault(pivot => pivot.PK != container1AsnPivot.PK);
			AssertEquals("Vehicle Registration Number should be updated.", "ABC-123", vehicle2Rtu.WRH_VehicleReference);
			AssertEquals("Driver should be updated.", "DRIVER2", vehicle2Rtu.WRH_SignedBy);
			AssertNotEquals("New vehicle ASN should be different from the original vehicle ASN.", vehicle1Asn.PK, vehicle2Asn.PK);
			AssertEquals("Vehicle RTU should remain the same.", vehicle1Rtu.PK, vehicle2Rtu.PK);
			AssertEquals("Vehicle ASN and RTU should link to the vehicle pivot", true,
				vehicle2Asn.PK == vehicle2AsnPivot.WAR_WRP_TransitReceiveASN &&
				vehicle2Rtu.PK == vehicle2AsnPivot.WAR_WRH_TransitReceiveTransportationUnit);
			container1RtuPkgState = GetRtuPackageState(container1Rtu.PK);
			AssertRtuPackageStatesShouldLinkToVehicleASN(container1RtuPkgState, vehicle2Asn.PK);
			links = Factory.Load<StmUniversalJobLink>(new ZQuery());
			AssertUniversalJobLink(links, null, vehicle2Rtu.PK, WhsItemReceiveTransportationUnitSchema.Constants.Prefix, DataContextType.GateBooking, "GTB001", "EDI", "DAT", "EDI");
			AssertUniversalJobLink(links, null, container1Rtu.PK, WhsItemReceiveTransportationUnitSchema.Constants.Prefix, DataContextType.GateMovementBooking, "GMB001", "EDI", "DAT", "EDI");
			AssertUniversalJobLinkShouldBeUnique(links, DataContextType.GateBooking, "GTB001");
			AssertUniversalJobLinkShouldBeUnique(links, DataContextType.GateMovementBooking, "GMB001");
			AssertGateRelatedBookingConfirmedLog(vehicle2Rtu, nameof(DataContextType.GateBooking), expectedLogReference: $"{vehicle2Rtu.WRH_ReferenceNumber}|FAC=WHS|LOC=ThereVille|MST=GateBooking|RFN=GTB001|TYP=WhsItemReceiveTransportationUnit|WHS=TW2");
			AssertGateRelatedBookingConfirmedLog(container1Rtu, nameof(DataContextType.GateMovementBooking), expectedLogReference: $"{container1Rtu.WRH_ReferenceNumber}|FAC=WHS|LOC=ThereVille|MST=GateMovementBooking|RFN=GMB001|TYP=WhsItemReceiveTransportationUnit|WHS=TW2", isLogUnique: false);
		}

		WhsItemPackageState GetRtuPackageState(ZGuid vehicleRtuPK)
		{
			var pkgExtensionQuery = new ZQuery(PkgPackageExtensionSchema.KPN_ParentID, vehicleRtuPK);
			pkgExtensionQuery.AddToFilter(PkgPackageExtensionSchema.KPN_ParentTableCode, WhsItemReceiveTransportationUnitSchema.Constants.Prefix);
			var pkgExtensions = Factory.Load<PkgPackageExtension>(pkgExtensionQuery);
			var pkgStateQuery = new ZQuery(WhsItemPackageStateSchema.WPS_KP_Package, pkgExtensions.Select(pe => pe.GetValue(PkgPackageExtensionSchema.KPN_KP_Package)));
			pkgStateQuery.OrderBy = WhsItemPackageStateSchema.Constants.WPS_SystemCreateTimeUtc + OrderByClause.Descending;
			return Factory.Load<WhsItemPackageState>(pkgStateQuery).FirstOrDefault();
		}

		IEnumerable<WhsItemPackageState> GetRcnsPackageStates(ZGuid[] rcnPKs, UniversalObjectFactory universalObjectFactory)
		{
			var receiveConsignments = (universalObjectFactory ?? Factory).Load<WhsItemReceiveConsignment>(new ZQuery(WhsItemReceiveConsignmentSchema.PK, rcnPKs));
			return receiveConsignments.SelectMany(rc => rc.PackageStates).ToArray();
		}

		void AssertUniversalJobLinkShouldBeUnique(StmUniversalJobLink[] links, DataContextType dataContextType, ZString sourceKey)
		{
			var filteredLinks = links.Where(link => link.UCL_SourceType == dataContextType.ToString() && link.UCL_SourceKey == sourceKey);
			AssertEquals($"Universal Job Link with type:'{dataContextType.ToString()} and source key:'{sourceKey}' should be unique.", 1, filteredLinks.Count());
		}

		void AssertRtuPackageStatesShouldLinkToVehicleASN(WhsItemPackageState rtuPkgState, ZGuid asnPK)
		{
			Assert("Rtu package states should link to the ASN.", rtuPkgState.WPS_WRP_ReceiveExpectedPacking == asnPK);
		}

		void AssertPackageStatesUnderTheRCNsShouldLinkToAnSpecificASN(ZGuid[] rcnPKs, ZGuid asnPK, UniversalObjectFactory universalObjectFactory = null)
		{
			var allPackageStates = GetRcnsPackageStates(rcnPKs, universalObjectFactory);
			Assert("All packages should link to the ASN.", allPackageStates.All(p => p.WPS_WRP_ReceiveExpectedPacking == asnPK));
		}

		void AssertGateRelatedBookingConfirmedLogs(WhsItemReceiveTransportationUnit rtu, ZString expectedGateBookingConfirmedLogRef, ZString expectedGateMovementBookingConfirmedLogRef)
		{
			var bookingConfirmedLogQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.BookingConfirmed.Code);
			var allBookingConfirmedLogs = rtu.Logs.Find(bookingConfirmedLogQuery).ToList();
			var gateBookingConfirmedLog = allBookingConfirmedLogs.SingleOrDefault(log => log.SL_Reference.Contains(nameof(DataContextType.GateBooking)));
			var gateMovementBookingConfirmedLog = allBookingConfirmedLogs.SingleOrDefault(log => log.SL_Reference.Contains(nameof(DataContextType.GateMovementBooking)));

			AssertNotNull("Gate Booking Confirmed Log should exist", gateBookingConfirmedLog);
			AssertNotNull("Gate Movement Booking Confirmed Log should exist", gateMovementBookingConfirmedLog);
			AssertEquals(expectedGateBookingConfirmedLogRef, gateBookingConfirmedLog.SL_Reference);
			AssertEquals(expectedGateMovementBookingConfirmedLogRef, gateMovementBookingConfirmedLog.SL_Reference);
		}

		void AssertGateRelatedBookingConfirmedLog(WhsItemReceiveTransportationUnit rtu, ZString gateBookingReferenceType, ZString expectedLogReference, bool isLogUnique = true)
		{
			var bookingConfirmedLogQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.BookingConfirmed.Code);
			var allBookingConfirmedLogs = rtu.Logs.Find(bookingConfirmedLogQuery).ToList();
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

		(WhsItemReceiveASN[] asns, WhsItemReceiveConsignment[] rcns, WhsItemReceiveASNRTUPivot[] pivots, WhsItemReceiveTransportationUnit[] rtus) AssertEntitiesCount(int? asnCount, int? rcnCount, int? asnRtuPivotCount, int? rtuCount, UniversalObjectFactory universalObjectFactory = null)
		{
			var asns = AssertEntityCount<WhsItemReceiveASN>(asnCount, universalObjectFactory);
			var rcns = AssertEntityCount<WhsItemReceiveConsignment>(rcnCount, universalObjectFactory);
			var pivots = AssertEntityCount<WhsItemReceiveASNRTUPivot>(asnRtuPivotCount, universalObjectFactory);
			var rtus = AssertEntityCount<WhsItemReceiveTransportationUnit>(rtuCount, universalObjectFactory);
			return (asns, rcns, pivots, rtus);
		}

		#endregion

		ContainerType DefaultContainerType => new ContainerType { Code = "20GP" };
		WhsTransitTestHelper Helper => helper ?? (helper = new WhsTransitTestHelper(Factory.BOFactory));
		WhsTransitTestHelper helper;
	}
}
