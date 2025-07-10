using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.Packing.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.TransportConsignment.Business;
using Enterprise.TransportConsignment.Business.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Writing;
using Enterprise.UniversalDataBuss.Integration;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.TransportConsignment.DataTransfer.Universal.Testing
{
	class LTConsignmentConsolidationDataObjectReaderTest : DtbBookingConsignmentUniversalTestCase
	{
		#region TestDataContextType
		public void TestDataContextType()
		{
			AssertEquals(DataContextType.LandTransportConsignmentConsol, new LTConsignmentConsolidationDataObjectReader(new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance), Logger, Factory).DataContextType);
		}

		#endregion
		#region TestConsignments
		public void TestConsignments()
		{
			var consolidationDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var consignmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var pickupInstructionDataObject = UniversalTestHelper.CreateInstructionDataObject(InstructionTypes.Codes.PickUp);
			var pkgDataObject = UniversalTestHelper.CreatePackageDataObject("123", "PLT", 1);
			UniversalTestHelper.AddPackageLink(pickupInstructionDataObject, 1);
			consolidationDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine> { pkgDataObject });
			consolidationDataObject.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment> { consignmentDataObject });
			var reader = new LTConsignmentConsolidationDataObjectReader(consolidationDataObject, Logger, Factory);
			var consolidation = reader.ReadIntoBusinessObject();
			AssertEquals(1, consolidation.ConsignmentsCreatedDuringImport_ForTesting.Count);
		}

		#endregion
		#region TestDeliveryInformationIsNotImportedForPickUpConfirmedBookings
		public void TestDeliveryInformationIsNotImportedForPickUpConfirmedBookings()
		{
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2012_11))
			{
				var consolidationDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
				var consignmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
				{ DataContext = DataContextFactory.New() };
				consignmentDataObject.ShipmentStatus = new CodeDescriptionPair { Code = TransportStatuses.Codes.PickUpConfirmed };
				consignmentDataObject.SetParentShipmentCollection(() => new List<UniversalShipment> { consolidationDataObject });
				Logger.TopLevelDataObject = consignmentDataObject;
				Logger.TopLevelDataContext.AddDataSource(DataContextType.TransportBooking, "TB00000007");
				var pickUpAddress = GetNewAddressData_CRAHOLSYD(DocAddressType.LocalCartageExporter);
				var deliveryAddress1 = GetNewAddressData_INTHEMSYD(DocAddressType.LocalCartageImporter);
				var deliveryAddress2 = GetNewAddressData_WUFSHIJNB(DocAddressType.LocalCartageImporter);
				UniversalTestHelper.CreateAddressInDB(pickUpAddress, Factory);
				UniversalTestHelper.CreateAddressInDB(deliveryAddress1, Factory);
				UniversalTestHelper.CreateAddressInDB(deliveryAddress2, Factory);
				var pickUpInstruction = UniversalTestHelper.CreateInstructionDataObject(InstructionTypes.Codes.PickUp, pickUpAddress);
				var deliveryInstruction1 = UniversalTestHelper.CreateInstructionDataObject(InstructionTypes.Codes.Delivery, deliveryAddress1);
				var deliveryInstruction2 = UniversalTestHelper.CreateInstructionDataObject(InstructionTypes.Codes.Delivery, deliveryAddress2);
				consignmentDataObject.SetInstructionCollection(() => new DataObjectList<Instruction> { pickUpInstruction, deliveryInstruction1, deliveryInstruction2 });
				consolidationDataObject.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment> { consignmentDataObject });
				var pkgDataObject = UniversalTestHelper.CreatePackageDataObject("123", "PLT", 1);
				UniversalTestHelper.AddPackageLink(pickUpInstruction, 1);
				consignmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine> { pkgDataObject });
				var reader = new LTConsignmentConsolidationDataObjectReader(consignmentDataObject, Logger, Factory);
				var consolidation = reader.ReadIntoBusinessObject();
				AssertEquals("Only 1 Pick Up Consignment should be Created.", 1, consolidation.ConsignmentsCreatedDuringImport_ForTesting.Count);
				var consignment = consolidation.ConsignmentsCreatedDuringImport_ForTesting.Single();
				AssertAddressContentMatches_CRAHOLSYD(consignment.PickupAddress.Address.Address);
				AssertEquals("Delivery Instruction should have no Delivery Information imported.", true, consignment.DeliveryAddress.Address.IsEmpty);
			}
		}

		#endregion
		#region TestGetExistingBusinessObject_AttachedToParentBooking
		public void TestGetExistingBusinessObject_AttachedToParentBooking()
		{
			Logger.OutboundSessionTracker = new DataWritingManager(new DummyActionInfo());
			var pickupOrg = Helper.CreateOrganisation("PCK1", address1: "123 Test st");
			var deliveryOrg = Helper.CreateOrganisation("Dlb1", address1: "456 Test st");
			var consignment = HelperLT.CreateConsignment("LT001");
			var pickupAddress = HelperLT.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp, pickupOrg.MainAddress);
			var deliveryAddress = HelperLT.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.Delivery);
			HelperLT.CreateConsignmentAction(pickupAddress, ActionTypes.Codes.PickUp);
			HelperLT.CreateConsignmentAction(deliveryAddress, ActionTypes.Codes.Delivery);
			deliveryAddress.Address.E2_OA_Address = ZGuid.Empty;
			var pkg = HelperLT.CreatePackage(consignment, "123", "PLT");
			Factory.SaveForTesting();
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2012_11))
			{
				var consignmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
				SetupConsignmentDataObjectForUpdateAndSetupLogger(consignment, consignmentDataObject);
				var pickUpInstructionDataObject = GetInstructionByType(consignmentDataObject.InstructionCollection, InstructionTypes.Codes.PickUp);
				var deliveryInstructionDataObject = GetInstructionByType(consignmentDataObject.InstructionCollection, InstructionTypes.Codes.Delivery);
				CreateAndAssignPackage(pickUpInstructionDataObject, consignmentDataObject, deliveryInstructionDataObject);
				var reader = new LTConsignmentConsolidationDataObjectReader(consignmentDataObject, Logger, Factory);
				var consolidation = reader.ReadIntoBusinessObject();
				AssertContainsExactElementsInAnyOrder("Consolidation should load existing consignment.", new[] { consignment }, consolidation.ConsignmentsCreatedDuringImport_ForTesting);
				AssertEquals("Import should have no errors.", false, Logger.HasErrors);
			}
		}

		#endregion
		#region TestGetReasonForNotAbleToUpdateFromDataSourceOrTargetBO_NoExistingConsignment
		public void TestGetReasonForNotAbleToUpdateFromDataSourceOrTargetBO_NoExistingConsignment()
		{
			Logger.OutboundSessionTracker = new DataWritingManager(new DummyActionInfo());
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2012_11))
			{
				var booking = Helper.CreateBooking("TB123");
				Factory.SaveForTesting();
				var consignmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
				var deliveryAddress = GetNewAddressData_WUFSHIJNB(DocAddressType.LocalCartageImporter);
				UniversalTestHelper.CreateAddressInDB(deliveryAddress, Factory);
				var consolidationDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
				consignmentDataObject.DataContext = DataContextFactory.New();
				var pickupInstructionDataObject = UniversalTestHelper.CreateInstructionDataObject(InstructionTypes.Codes.PickUp, null);
				var deliveryInstructionDataObject = UniversalTestHelper.CreateInstructionDataObject(InstructionTypes.Codes.Delivery, deliveryAddress);
				consignmentDataObject.SetInstructionCollection(() => new DataObjectList<Instruction> { pickupInstructionDataObject, deliveryInstructionDataObject });
				CreateAndAssignPackage(pickupInstructionDataObject, consignmentDataObject, deliveryInstructionDataObject);
				consignmentDataObject.ShipmentStatus = new CodeDescriptionPair { Code = TransportStatuses.Codes.Available };
				consignmentDataObject.SetParentShipmentCollection(() => new List<UniversalShipment> { consolidationDataObject });
				Logger.OutboundSessionTracker = new DataWritingManager(new DummyActionInfo());
				Logger.TopLevelDataObject = consignmentDataObject;
				Logger.TopLevelDataContext.AddDataSource(DataContextType.TransportBooking, "TB123");
				Logger.TopLevelDataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
				var reader = new LTConsignmentConsolidationDataObjectReader(consignmentDataObject, Logger, Factory);
				var consolidation = reader.ReadIntoBusinessObject();
				AssertNotNull(consolidation.ConsignmentsCreatedDuringImport_ForTesting);
				AssertEquals("Import should have no errors.", false, Logger.HasErrors);
			}
		}

		#endregion
		#region TestWithOldNamespace
		public void TestWithOldNamespace()
		{
			var consolidationDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			consolidationDataObject.DataContext = DataContextFactory.New();
			consolidationDataObject.DataContext.AddDataSource(DataContextType.TransportBookingConsolidation, "C");
			consolidationDataObject.DataContext.AddDataSource(DataContextType.TransportBooking, "123");
			var consignmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			consignmentDataObject.DataContext = DataContextFactory.New();
			consignmentDataObject.DataContext.AddDataSource(DataContextType.TransportBooking, "123");
			var pickUpInstruction = UniversalTestHelper.CreateInstructionDataObject(InstructionTypes.Codes.PickUp);
			var deliveryInstruction1 = UniversalTestHelper.CreateInstructionDataObject(InstructionTypes.Codes.Delivery);
			var deliveryInstruction2 = UniversalTestHelper.CreateInstructionDataObject(InstructionTypes.Codes.Delivery);
			var pkgDataObject1 = UniversalTestHelper.CreatePackageDataObject("123", "PLT", 1);
			var pkgDataObject2 = UniversalTestHelper.CreatePackageDataObject("1234", "PLT", 2);
			UniversalTestHelper.AddPackageLink(pickUpInstruction, 1);
			UniversalTestHelper.AddPackageLink(pickUpInstruction, 2);
			UniversalTestHelper.AddPackageLink(deliveryInstruction1, 1);
			UniversalTestHelper.AddPackageLink(deliveryInstruction2, 2);
			consolidationDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine> { pkgDataObject1, pkgDataObject2 });
			consignmentDataObject.SetInstructionCollection(() => new DataObjectList<Instruction> { pickUpInstruction, deliveryInstruction1, deliveryInstruction2 });
			consolidationDataObject.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment> { consignmentDataObject });
			var reader = new LTConsignmentConsolidationDataObjectReader(consolidationDataObject, Logger, Factory);
			var consolidation = reader.ReadIntoBusinessObject();
			AssertEquals(2, consolidation.ConsignmentsCreatedDuringImport_ForTesting.Count);
		}

		#endregion
		#region TestMultipleDeliveryInstructionsCreatesMultipleConsignments
		public void TestMultipleDeliveryInstructionsCreatesMultipleConsignments()
		{
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2012_11))
			{
				var consolidationDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
				var consignmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
				{ DataContext = DataContextFactory.New() };
				consignmentDataObject.SetParentShipmentCollection(() => new List<UniversalShipment> { consolidationDataObject });
				Logger.TopLevelDataObject = consignmentDataObject;
				Logger.TopLevelDataContext.AddDataSource(DataContextType.TransportBooking, "TB00000007");
				var pickUpInstruction = UniversalTestHelper.CreateInstructionDataObject(InstructionTypes.Codes.PickUp);
				var deliveryInstruction1 = UniversalTestHelper.CreateInstructionDataObject(InstructionTypes.Codes.Delivery);
				var deliveryInstruction2 = UniversalTestHelper.CreateInstructionDataObject(InstructionTypes.Codes.Delivery);
				var pkgDataObject1 = UniversalTestHelper.CreatePackageDataObject("123", "PLT", 1);
				var pkgDataObject2 = UniversalTestHelper.CreatePackageDataObject("1234", "PLT", 2);
				UniversalTestHelper.AddPackageLink(pickUpInstruction, 1);
				UniversalTestHelper.AddPackageLink(pickUpInstruction, 2);
				UniversalTestHelper.AddPackageLink(deliveryInstruction1, 1);
				UniversalTestHelper.AddPackageLink(deliveryInstruction2, 2);
				consignmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine> { pkgDataObject1, pkgDataObject2 });
				consignmentDataObject.SetInstructionCollection(() => new DataObjectList<Instruction> { pickUpInstruction, deliveryInstruction1, deliveryInstruction2 });
				consolidationDataObject.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment> { consignmentDataObject });
				var reader = new LTConsignmentConsolidationDataObjectReader(consignmentDataObject, Logger, Factory);
				var consolidation = reader.ReadIntoBusinessObject();
				AssertEquals(2, consolidation.ConsignmentsCreatedDuringImport_ForTesting.Count);
			}
		}

		#endregion
		#region TestMultipleDeliveryInstructionsThrowsErrorWhenPackagesNotLinkedProperly
		public void TestMultipleDeliveryInstructionsThrowsError_WhenPackagesNotLinkedProperly()
		{
			var consolidationDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var consignmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{ DataContext = DataContextFactory.New() };
			consignmentDataObject.SetParentShipmentCollection(() => new List<UniversalShipment> { consolidationDataObject });
			Logger.TopLevelDataObject = consignmentDataObject;
			Logger.TopLevelDataContext.AddDataSource(DataContextType.TransportBooking, "TB00000007");
			var pickUpInstruction = UniversalTestHelper.CreateInstructionDataObject(InstructionTypes.Codes.PickUp);
			var deliveryInstruction1 = UniversalTestHelper.CreateInstructionDataObject(InstructionTypes.Codes.Delivery);
			var deliveryInstruction2 = UniversalTestHelper.CreateInstructionDataObject(InstructionTypes.Codes.Delivery);
			CreateAndAssignPackage(pickUpInstruction, consignmentDataObject, deliveryInstruction1);
			consignmentDataObject.SetInstructionCollection(() => new DataObjectList<Instruction> { pickUpInstruction, deliveryInstruction1, deliveryInstruction2 });
			consolidationDataObject.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment> { consignmentDataObject });
			var reader = new LTConsignmentConsolidationDataObjectReader(consignmentDataObject, Logger, Factory);
			AssertExceptionThrown(typeof(DataObjectReadFailureException), "The packages are not linked correctly.", () => reader.ReadIntoBusinessObject());
		}

		#endregion
		#region TestMultipleDeliveryInstructionsThrowsErrorWhenThereAreNoPackagesAssigned
		public void TestMultipleDeliveryInstructionsThrowsError_WhenThereAreNoPackagesAssigned()
		{
			var consolidationDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var consignmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{ DataContext = DataContextFactory.New() };
			consignmentDataObject.SetParentShipmentCollection(() => new List<UniversalShipment> { consolidationDataObject });
			Logger.TopLevelDataObject = consignmentDataObject;
			Logger.TopLevelDataContext.AddDataSource(DataContextType.TransportBooking, "TB00000007");
			var pickUpInstruction = UniversalTestHelper.CreateInstructionDataObject(InstructionTypes.Codes.PickUp);
			var deliveryInstruction1 = UniversalTestHelper.CreateInstructionDataObject(InstructionTypes.Codes.Delivery);
			var deliveryInstruction2 = UniversalTestHelper.CreateInstructionDataObject(InstructionTypes.Codes.Delivery);
			consignmentDataObject.SetInstructionCollection(() => new DataObjectList<Instruction> { pickUpInstruction, deliveryInstruction1, deliveryInstruction2 });
			consolidationDataObject.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment> { consignmentDataObject });
			var reader = new LTConsignmentConsolidationDataObjectReader(consignmentDataObject, Logger, Factory);
			AssertExceptionThrown(typeof(DataObjectReadFailureException), "Packages are not assigned to any instructions", () => reader.ReadIntoBusinessObject());
		}

		#endregion
		#region TestMultipleDeliveryInstructionsUpdatesAndCreatesMultipleConsignmentIfPickUpConsignmentExists
		public void TestMultipleDeliveryInstructionsUpdatesAndCreatesMultipleConsignmentIfPickUpConsignmentExists()
		{
			var pickupOrg = Helper.CreateOrganisation("PCK1", address1: "123 Test st");
			var deliveryOrg = Helper.CreateOrganisation("Dlb1", address1: "456 Test st");
			var depotOrg = Helper.CreateOrganisation("Dpt1", address1: "789 Test st");
			var pickUpConsignment = HelperLT.CreateConsignment("LT001");
			var pickupAddress = HelperLT.CreateConsignmentAddress(pickUpConsignment, ConsignmentAddressTypes.Codes.PickUp, pickupOrg.MainAddress);
			var depotAddress = HelperLT.CreateConsignmentAddress(pickUpConsignment, ConsignmentAddressTypes.Codes.Multi, depotOrg.MainAddress);
			depotAddress.DocAddresses[0].DocAddressType = DocAddressType.LocalCartageCFS; //To make it IsDepot true
			var deliveryAddress = HelperLT.CreateConsignmentAddress(pickUpConsignment, ConsignmentAddressTypes.Codes.Delivery);
			pickupAddress.LTS_Notes = "HANDLE WELL";
			var outerPallet = helper.CreatePackage(pickUpConsignment.PackageJob, "PLT", "PLT-1");
			var innerCarton1 = outerPallet.Packages.AddNew("CTN", "CTN-1");
			var innerCarton2 = outerPallet.Packages.AddNew("CTN", "CTN-2");
			var innerBox = outerPallet.Packages.AddNew("BOX", "BOX-1");
			var outerPalletID = Factory.Load<PkgPackageHeader>(outerPallet.KP_KPH_PackageHeader);
			var innerBoxID = Factory.Load<PkgPackageHeader>(innerBox.KP_KPH_PackageHeader);
			var pickUpAction = HelperLT.CreateConsignmentAction(pickupAddress, ActionTypes.Codes.PickUp);
			pickUpAction.LTA_ReferenceNumber = "PICKUPREF";
			var depotDeliveryAction = HelperLT.CreateConsignmentAction(depotAddress, ActionTypes.Codes.Delivery);
			var depotPickupAction = HelperLT.CreateConsignmentAction(depotAddress, ActionTypes.Codes.PickUp);
			var deliveryAction = HelperLT.CreateConsignmentAction(deliveryAddress, ActionTypes.Codes.Delivery);
			depotDeliveryAction.LTA_ReferenceNumber = "DLVREF";
			var runSheet = Helper.CreateRunSheet(GlbStaff.CurrentUser);
			var pickUpRunSheetInstruction = HelperLT.CreateRunSheetInstruction(pickUpAction, runSheet.PK); // allocate PickUp to runsheet
			var depotDeliveryRunSheetInstruction = HelperLT.CreateRunSheetInstruction(depotDeliveryAction, runSheet.PK); // manually need to do this, normally this should be done for us.
			Factory.SaveForTesting();
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2012_11))
			{
				var consignmentDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
				SetupConsignmentDataObjectForUpdateAndSetupLogger(pickUpConsignment, consignmentDataObject);
				int palletLink = 1;
				int carton1Link = 2;
				int carton2Link = 3;
				var boxDataObject = UniversalTestHelper.CreatePackageDataObject(1, "BOX-1", "BOX");
				var cartonDataObject1 = UniversalTestHelper.CreatePackageDataObject(1, "CTN-1", "CTN", carton1Link);
				var cartonDataObject2 = UniversalTestHelper.CreatePackageDataObject(1, "CTN-2", "CTN", carton2Link);
				var palletDataObject = UniversalTestHelper.CreatePackageDataObject(1, "PLT-1", "PLT", palletLink);
				palletDataObject.SetPackingLineCollection(() => new List<PackingLine> { boxDataObject, cartonDataObject1, cartonDataObject2 });
				consignmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine> { palletDataObject });
				var pickUpInstructionDataObject = GetInstructionByType(consignmentDataObject.InstructionCollection, InstructionTypes.Codes.PickUp);
				var deliveryInstructionDataObject = GetInstructionByType(consignmentDataObject.InstructionCollection, InstructionTypes.Codes.Delivery);
				UniversalTestHelper.AddPackageLink(pickUpInstructionDataObject, palletLink);
				UniversalTestHelper.AddPackageLink(deliveryInstructionDataObject, carton1Link);
				var secondDeliveryAddress = GetNewAddressData_CRAHOLSYD(DocAddressType.LocalCartageImporter);
				var secondDeliveryOrgAddress = UniversalTestHelper.CreateAddressInDB(secondDeliveryAddress, Factory);
				var secondDeliveryInstruction = UniversalTestHelper.CreateInstructionDataObject(InstructionTypes.Codes.Delivery, secondDeliveryAddress);
				UniversalTestHelper.AddPackageLink(secondDeliveryInstruction, carton2Link);
				consignmentDataObject.InstructionCollection.Add(secondDeliveryInstruction);
				var reader = new LTConsignmentConsolidationDataObjectReader(consignmentDataObject, Logger, Factory);
				var matchingConsolidation = reader.ReadIntoBusinessObject();
				AssertCollectionContains("Should update existing Consignment Consolidation.", pickUpConsignment, matchingConsolidation.ConsignmentsCreatedDuringImport_ForTesting);
				AssertEquals("Should update Pick Up Consignment and created 1 new Consignment.", 2, matchingConsolidation.ConsignmentsCreatedDuringImport_ForTesting.Count);
				var newConsignment = matchingConsolidation.ConsignmentsCreatedDuringImport_ForTesting.Single(c => !c.IsInDatabase);
				AssertContainsExactElementsInAnyOrder(new[] { pickUpConsignment, newConsignment }, matchingConsolidation.ConsignmentsCreatedDuringImport_ForTesting);
				AssertEquals("All Pick Up Allocations should be the same on each Consignment.", true, matchingConsolidation.ConsignmentsCreatedDuringImport_ForTesting.All(c => c.PickupAddress.PickupAction.LTA_K1_RunSheetInstruction == pickUpRunSheetInstruction.PK));
				// get depot address object manually
				var pickupDepotAddresses = matchingConsolidation.ConsignmentsCreatedDuringImport_ForTesting.SelectMany(c => c.Addresses).OrderBy(i => i.LTS_Sequence).Where(i => i.IsDepot && i.LTS_InstructionType.EqualsIgnoringCase(ConsignmentAddressTypes.Codes.Multi)).ToArray();
				AssertEquals("Should be at least one Depot Address.", true, pickupDepotAddresses.Any());
				AssertEquals("All Deliver to Depot Allocations should be the same on each Consignment.", true, pickupDepotAddresses.All(a => a.DeliveryAction.LTA_K1_RunSheetInstruction == depotDeliveryRunSheetInstruction.PK));
				AssertEquals("Pick Up Information should be the same on each Consignment.", true, matchingConsolidation.ConsignmentsCreatedDuringImport_ForTesting.All(c => c.PickupAddress.PickupAction.LTA_ReferenceNumber == "PICKUPREF" && c.PickupAddress.LTS_Notes == "HANDLE WELL"));
				var consignmentWithDeliveryAddress1 = matchingConsolidation.ConsignmentsCreatedDuringImport_ForTesting.Single(c => c.DeliveryAddress.Address.E2_OA_Address != secondDeliveryOrgAddress.PK);
				var consignmentWithDeliveryAddress2 = matchingConsolidation.ConsignmentsCreatedDuringImport_ForTesting.Single(c => c.DeliveryAddress.Address.E2_OA_Address == secondDeliveryOrgAddress.PK);
				AssertAddressContentMatches_WUFSHIJNB(consignmentWithDeliveryAddress1.DeliveryAddress.Address.Address);
				AssertAddressContentMatches_CRAHOLSYD(consignmentWithDeliveryAddress2.DeliveryAddress.Address.Address);
				AssertEquals("Original Outer Pallet should be deleted.", true, outerPallet.IsDeleted);
				AssertEquals("Original Outer Pallet should be deleted.", true, outerPalletID.IsDeleted);
				AssertEquals("Any other left over Packages should be deleted.", true, innerBox.IsDeleted);
				AssertEquals("Any other left over Packages should be deleted.", true, innerBoxID.IsDeleted);
				AssertEquals("Import should have no errors.", false, Logger.HasErrors);
			}
		}

		#endregion
		#region Implementation
		Instruction GetInstructionByType(IEnumerable<Instruction> instructions, string instructionType)
		{
			return instructions.SingleOrDefault(i => i.Type.GetCodeAsUpperCase() == instructionType);
		}

		void SetupConsignmentDataObjectForUpdateAndSetupLogger(DtbConsignment existingConsignment, UniversalShipment consignmentDataObject, string bookingStatus = TransportStatuses.Codes.Available, OrganizationAddress pickupAddress = null)
		{
			var booking = Helper.CreateBooking("TB123");
			var reference = existingConsignment.AdditionalReferenceNumbers.AddNew();
			reference.CE_EntryType = TransportCommonAdditionalReferenceTypes.Codes.BookingJobId;
			reference.CE_EntryNum = booking.KM_JobID.ToString();
			Factory.SaveForTesting();
			var deliveryAddress = GetNewAddressData_WUFSHIJNB(DocAddressType.LocalCartageImporter);
			UniversalTestHelper.CreateAddressInDB(deliveryAddress, Factory);
			var dataWritingManager = new DataWritingManager(new DummyActionInfo());
			OrganizationAddress pickUpOrganisationAddressDataObject = null;
			if (pickupAddress != null)
			{
				pickUpOrganisationAddressDataObject = pickupAddress;
			}
			else
			{
				var pickUpOrgAddress = existingConsignment.PickupAddress.Address.Address;
				if (pickUpOrgAddress != null)
				{
					var writer = new OrganizationDataObjectWriter(dataWritingManager, nameof(DocAddressType.LocalCartageExporter));
					pickUpOrganisationAddressDataObject = writer.GetDataObject(pickUpOrgAddress);
				}
			}

			var consolidationDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			consignmentDataObject.DataContext = DataContextFactory.New();
			var pickupInstructionDataObject = UniversalTestHelper.CreateInstructionDataObject(InstructionTypes.Codes.PickUp, pickUpOrganisationAddressDataObject);
			var deliveryInstructionDataObject = UniversalTestHelper.CreateInstructionDataObject(InstructionTypes.Codes.Delivery, deliveryAddress);
			consignmentDataObject.SetInstructionCollection(() => new DataObjectList<Instruction> { pickupInstructionDataObject, deliveryInstructionDataObject });
			consignmentDataObject.ShipmentStatus = new CodeDescriptionPair { Code = bookingStatus };
			consignmentDataObject.SetParentShipmentCollection(() => new List<UniversalShipment> { consolidationDataObject });
			Logger.OutboundSessionTracker = dataWritingManager;
			Logger.TopLevelDataObject = consignmentDataObject;
			Logger.TopLevelDataContext.AddDataSource(DataContextType.TransportBooking, "TB123");
			Logger.TopLevelDataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
		}

		#region CreateAndAssignPackageToInstruction
		PackingLine CreateAndAssignPackage(Instruction addressPIC, UniversalShipment consignmentDataObject, Instruction addressDLV = null)
		{
			var package1 = UniversalTestHelper.CreatePackageDataObject("123", "PLT", 1);
			AddPackageToInstruction(addressPIC, package1);
			if (addressDLV != null)
			{
				AddPackageToInstruction(addressDLV, package1);
			}

			consignmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine> { package1 });
			return package1;
		}

		void AddPackageToInstruction(Instruction instruction, PackingLine package)
		{
			UniversalTestHelper.AddPackageLink(instruction, PACKAGELINK);
			instruction.SetInstructionPackingLineLinkCollection(() => new List<InstructionPackingLineLink> { new InstructionPackingLineLink { PackingLineLink = PACKAGELINK, Quantity = 1 } });
		}

		const int PACKAGELINK = 1;
		#endregion
		#endregion
		#region Helper
		TransportConsignmentTestHelper HelperLT
		{
			get
			{
				return helper ?? (helper = new TransportConsignmentTestHelper(Factory.BOFactory));
			}
		}

		TransportConsignmentTestHelper helper;
		#endregion
	}
}
