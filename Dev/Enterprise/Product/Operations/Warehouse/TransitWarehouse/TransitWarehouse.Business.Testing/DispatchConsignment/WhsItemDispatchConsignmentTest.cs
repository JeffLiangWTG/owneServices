using System;
using System.Linq;
using System.Reflection;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.DataTransfer.Business;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.LandTransport;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Packing.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.TransportBookings.Shared;
using Enterprise.TransportBookings.Shared.Testing;
using Enterprise.TransportCommon.Shared;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Integration.Forwarding;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	public class WhsItemDispatchConsignmentTest : TestCaseWithFactory
	{
		#region TestReadOnlyFields

		static readonly string[] ReadOnlyProperties =
		{
			"WDC_ConsignmentID",
			"WDC_SystemCreateTimeUtc",
			"WDC_SystemCreateUser",
			"WDC_SystemLastEditTimeUtc",
			"WDC_SystemLastEditUser",
			"WDC_WW_Warehouse",
			"WDC_JobID",
			"WDC_ParentID",
			"WDC_ParentTableCode",
			"WDC_IsAuthorizedForDispatch",
		};

		public void TestReadOnly()
		{
			var dcn = Factory.New<WhsItemDispatchConsignment>();
			foreach (var propertyName in ReadOnlyProperties)
			{
				var readOnly = ((ZPropertyInfo)typeof(WhsItemDispatchConsignment).GetProperty(propertyName + "Info").GetValue(dcn)).ReadOnly;
				var readOnlyAction = ActionFieldAttribute.Get(typeof(WhsItemDispatchConsignment).GetProperty(propertyName))?.ReadOnly;
				AssertEquals(propertyName, true, readOnly);
				AssertEquals(propertyName, true, readOnlyAction);
			}
		}

		#endregion

		#region TestAdditionalReferences

		public void TestAdditionalReferences()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var dispatchConsignment = helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var additionalReferenceInDispatch = dispatchConsignment.AdditionalReferenceNumbers.AddNew();
			additionalReferenceInDispatch.CE_EntryType = WarehouseAdditionalReferenceTypes.Codes.RunSheetInstructionReference;
			additionalReferenceInDispatch.CE_EntryNum = "R1-2";

			AssertEquals("R1-2", dispatchConsignment.AdditionalReferenceNumbers.GetAllReferenceNumbersByType(WarehouseAdditionalReferenceTypes.Codes.RunSheetInstructionReference).Single());
		}

		public void TestAdditionalReferenceNumbersSingleLine()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var dispatchConsignment = helper.CreateDispatchConsignment("DCN1", warehouse.PK);

			var cusNumber1 = dispatchConsignment.AdditionalReferenceNumbers.AddNew();
			cusNumber1.CE_EntryType = WarehouseAdditionalReferenceTypes.Codes.MasterBill;
			cusNumber1.CE_RN_NKCountryCode = "";
			cusNumber1.CE_EntryNum = "MB0000001";
			AssertEquals("MAB: MB0000001", dispatchConsignment.AdditionalReferenceNumbersSingleLine);

			cusNumber1.CE_RN_NKCountryCode = "AU";
			AssertEquals("MAB: MB0000001/AU", dispatchConsignment.AdditionalReferenceNumbersSingleLine);

			var cusNumber2 = dispatchConsignment.AdditionalReferenceNumbers.AddNew();
			cusNumber2.CE_EntryType = WarehouseAdditionalReferenceTypes.Codes.OrderNumber;
			cusNumber2.CE_EntryNum = "ORD0000001";
			cusNumber2.CE_RN_NKCountryCode = "CN";

			AssertEquals("MAB: MB0000001/AU, ORD: ORD0000001/CN", dispatchConsignment.AdditionalReferenceNumbersSingleLine);
		}

		public void TestShipmentNumber()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var dispatchConsignment = helper.CreateDispatchConsignment("DCN1", warehouse.PK);

			var cusNumber1 = dispatchConsignment.AdditionalReferenceNumbers.AddNew();
			cusNumber1.CE_EntryType = WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber;
			cusNumber1.CE_EntryNum = "S0000001";
			AssertEquals("S0000001", dispatchConsignment.ShipmentNumber);
		}

		#endregion

		#region TestAutoLog

		public void TestAutoLog()
		{
			var consignment = Factory.New<WhsItemDispatchConsignment>();
			AssertEquals(true, consignment.IsAutoAdminBusinessObjectLoggerEnabled);
		}

		#endregion

		#region TestJobDocAddresses

		public void TestJobDocAddresses()
		{
			var consignment = Factory.New<WhsItemDispatchConsignment>();
			AssertEquals(DocAddressType.ConsigneeDocumentaryAddress, consignment.ConsigneeDocAddress.DocAddressType);
			AssertEquals(DocAddressType.ClientRequestedBillingParty, consignment.ClientRequestedBillToPartyDocAddress.DocAddressType);
			AssertEquals(DocAddressType.ConsigneePickupDeliveryAddress, consignment.DeliveryDocAddress.DocAddressType);
			AssertEquals(DocAddressType.TransportCompanyDocumentaryAddress, consignment.TransportCompanyDocAddress.DocAddressType);
			AssertEquals(DocAddressType.DepartureCTOAddress, consignment.CTODocAddress.DocAddressType);
		}

		#endregion

		#region TestClientRequestedBillToPartyDocAddress

		public void TestBillingPartyCompanyName()
		{
			var billing1 = Helper.CreateClient("CRB1");
			var billing2 = Helper.CreateClient("CRB2");

			var consignmentWithNoAddress = Factory.New<WhsItemDispatchConsignment>();
			var consignmentWithJobDocAddress = Factory.New<WhsItemDispatchConsignment>();
			var consignmentWithJobDocAddressOverride = Factory.New<WhsItemDispatchConsignment>();
			var consignmentWithJobHeader = Factory.New<WhsItemDispatchConsignment>();
			var consignmentWithLocalClient = Factory.New<WhsItemDispatchConsignment>();

			var job1 = new JobHeader.Loader(consignmentWithJobHeader).TryLoadOrCreate();
			var job2 = new JobHeader.Loader(consignmentWithLocalClient).TryLoadOrCreate();
			job1.JH_OA_LocalChargesAddr = ZGuid.Empty;
			job2.JH_OA_LocalChargesAddr = billing2.MainAddress.PK;

			Helper.CreateJobDocAddressFromAddress(consignmentWithJobDocAddress, DocAddressTypes.Codes.ClientRequestedBillingParty, billing1.MainAddress);
			Helper.CreateJobDocAddressFromAddress(consignmentWithJobHeader, DocAddressTypes.Codes.ClientRequestedBillingParty, billing1.MainAddress);
			Helper.CreateJobDocAddressFromAddress(consignmentWithLocalClient, DocAddressTypes.Codes.ClientRequestedBillingParty, billing1.MainAddress);
			consignmentWithJobDocAddressOverride.ClientRequestedBillToPartyDocAddress.E2_AddressOverride = true;
			consignmentWithJobDocAddressOverride.ClientRequestedBillToPartyDocAddress.E2_CompanyName = "CRB3";

			AssertEquals(ZString.Empty, consignmentWithNoAddress.BillingPartyCompanyName);
			AssertEquals("CRB1", consignmentWithJobDocAddress.BillingPartyCompanyName);
			AssertEquals("CRB3", consignmentWithJobDocAddressOverride.BillingPartyCompanyName);
			AssertEquals(ZString.Empty, consignmentWithJobHeader.BillingPartyCompanyName);
			AssertEquals("CRB2", consignmentWithLocalClient.BillingPartyCompanyName);
		}

		#endregion

		#region TestSupportedAddressTypes

		public void TestSupportedAddressTypes()
		{
			var consignment = (IDocAddresses)Factory.New<WhsItemDispatchConsignment>();
			var expectedSupportedDocAddressTypes = new[] { DocAddressType.LocalCartageExporter, DocAddressType.ConsigneeDocumentaryAddress, DocAddressType.ClientRequestedBillingParty, DocAddressType.BookingPartyDocumentaryAddress, DocAddressType.ConsigneePickupDeliveryAddress, DocAddressType.TransportCompanyDocumentaryAddress, DocAddressType.DepartureCTOAddress };
			AssertContainsExactElementsInAnyOrder(expectedSupportedDocAddressTypes, consignment.SupportedAddressTypes);
		}

		#endregion

		#region BusinessObject Overrides

		public void TestHumanReadableName()
		{
			var itemDispatchConsignment = Factory.New<WhsItemDispatchConsignment>();
			AssertEquals("Dispatch Consignment", itemDispatchConsignment.HumanReadableName);

			itemDispatchConsignment.WDC_JobID = "188";
			AssertEquals("Dispatch Consignment 188", itemDispatchConsignment.HumanReadableName);
		}

		#endregion

		#region TestIJobNumber

		public void TestIJobNumber()
		{
			var header = Factory.New<WhsItemDispatchConsignment>();
			header.WDC_JobID = "ThisIsValid";
			AssertEquals("ThisIsValid", ((IJobNumber)header).JobNumber);
		}

		#endregion

		#region TestPopulateJobHeaderParent

		public void TestPopulateJobHeaderParent()
		{
			var shipment = (IJobHeaderParent)Factory.New<IForwardingShipment>();
			var jobForShipment = new JobHeader.Loader(shipment).TryLoadOrCreate();

			var dispatchConsignment = Factory.New<WhsItemDispatchConsignment>();
			dispatchConsignment.WDC_ParentID = shipment.PK;
			dispatchConsignment.WDC_ParentTableCode = ((BusinessObject)shipment).TablePrefix;

			var job = new JobHeader.Loader(dispatchConsignment).TryLoadOrCreate();
			AssertEquals(jobForShipment.PK, dispatchConsignment.JobHeader.JH_JH_ParentJob);
		}

		#endregion

		#region TestCreateJobHeader

		public void TestCreateJobHeaderWithNoParent()
		{
			var shipment = (IJobHeaderParent)Factory.New<IForwardingShipment>();
			new JobHeader.Loader(shipment).TryLoadOrCreate();

			var dispatchConsignment = Factory.New<WhsItemDispatchConsignment>();
			new JobHeader.Loader(dispatchConsignment).TryLoadOrCreate();
			AssertEquals(ZGuid.Empty, dispatchConsignment.JobHeader.JH_JH_ParentJob);
		}

		public void TestCreateJobHeaderWithNoParentJobOnParent()
		{
			var shipment = (IJobHeaderParent)Factory.New<IForwardingShipment>();

			var dispatchConsignment = Factory.New<WhsItemDispatchConsignment>();
			dispatchConsignment.WDC_ParentID = shipment.PK;
			dispatchConsignment.WDC_ParentTableCode = ((BusinessObject)shipment).TablePrefix;
			new JobHeader.Loader(dispatchConsignment).TryLoadOrCreate();
			AssertEquals(ZGuid.Empty, dispatchConsignment.JobHeader.JH_JH_ParentJob);
		}

		#endregion

		#region TestIPackingParentJobNo

		public void TestIPackingParentJobNo()
		{
			var consignment = Factory.New<WhsItemDispatchConsignment>();
			consignment.WDC_JobID = "ThisIsValid";
			AssertEquals("ThisIsValid", ((IPackingParent)consignment).JobNo);
		}

		#endregion

		#region TestIPackingParentConnoteNo

		public void TestIPackingParentConnoteNo()
		{
			var consignment = Factory.New<WhsItemDispatchConsignment>();
			AssertEquals(ZString.Empty, ((IPackingParent)consignment).ConnoteNo);
		}

		#endregion

		#region TestIPackingParentMembers

		public void TestIPackingParentMembers()
		{
			var consignment = Factory.New<WhsItemDispatchConsignment>();
			consignment.WDC_JobID = "DCN1";
			IPackingParent packingParent = consignment;
			AssertEquals(null, packingParent.ControllerID);
			AssertEquals(DocumentOptions.ShowBasicLabelOnly, packingParent.DocumentOptions);
			AssertEquals(false, packingParent.IsAutoPrintAllowed);
			AssertEquals(false, packingParent.IsParentJobFinalised);
			AssertEquals(false, packingParent.IsScanEventsVisible);
			AssertEquals("", packingParent.JobDescription);
			AssertEquals("DCN1", packingParent.JobNo);
			AssertEquals(null, packingParent.CarrierBookingAgent);
			AssertEquals("", packingParent.TransportReference);
			AssertEquals(false, packingParent.IsLoosePackageIDsSupported);
			AssertEquals(ParentJobType.None, packingParent.ParentJobType);
			AssertEquals(PackageSequenceType.OuterWithLooseID, packingParent.PackageSequenceType);
			AssertEquals(false, packingParent.IsPackingJobReadOnly);
			AssertEquals("NotificationTypeForInvalidContainerNumber should be correct", NotificationTypes.None, packingParent.NotificationTypeForInvalidContainerNumber);
		}

		#endregion

		#region TestIPackingParent_ShouldPackTrackedPackagesViaDivot

		public void TestIPackingParent_ShouldPackTrackedPackagesViaDivot()
		{
			var consignment = Factory.New<WhsItemDispatchConsignment>();
			IPackingParent packingParent = consignment;
			AssertEquals(false, packingParent.ShouldPackTrackedPackagesViaDivot);
		}

		#endregion

		#region TestIPackingParentSupportsImportingBookedDimensions

		public void TestIPackingParentSupportsImportingBookedDimensions()
		{
			var consignment = Factory.New<WhsItemDispatchConsignment>();
			AssertNotNull("Dispatch Consignment support importing Booked Dimensions.", consignment);
		}

		#endregion

		#region TestIPackingParent_OnPackageDelete

		public void TestIPackingParent_OnPackageDelete()
		{
			var consignment = Factory.New<WhsItemDispatchConsignment>();
			var packageState = consignment.PackageStates.AddNew();
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(consignment);

			var package = packageJob.Packages.AddNew();
			package.KP_PackageID = "1";
			packageState.WPS_KP_Package = package.PK;
			AssertEquals("Precondition", false, packageState.IsDeleted);

			package.Delete();
			AssertEquals(true, packageState.IsDeleted);
		}

		#endregion

		#region TestIDocManagerSupport

		public void TestIDocManagerSupport()
		{
			var header = Factory.New<WhsItemDispatchConsignment>();
			AssertEquals(Constants.DocManagerCodes.TransitDispatchConsignment, ((IDocManagerSupport)header).DocManagerInfo.DocManagerCode);
		}

		#endregion

		#region TestITransitConsignmentForRating Members

		public void TestITransitConsignmentForRating()
		{
			var now = ZDateTime.Now;
			var consignor = Helper.CreateClient("CNR1");
			var consignee = Helper.CreateClient("CNE1");
			var bookingParty = Helper.CreateClient("BKP1");
			var consignment = Factory.New<WhsItemDispatchConsignment>();
			consignment.WDC_RS_NKServiceLevel = "STD";
			Helper.CreateJobDocAddressFromAddress(consignment, DocAddressTypes.Codes.LocalCartageExporter, consignor.MainAddress);
			Helper.CreateJobDocAddressFromAddress(consignment, DocAddressTypes.Codes.ConsigneeDocumentaryAddress, consignee.MainAddress);
			Helper.CreateJobDocAddressFromAddress(consignment, DocAddressTypes.Codes.BookingPartyDocumentaryAddress, bookingParty.MainAddress);

			var transitRating = (ITransitConsignmentForRating)consignment;
			AssertEquals("STD", transitRating.ServiceLevel);
			AssertEquals(ChargeCodeGroupList.Codes.TRWDispatch, transitRating.ChargeCodeGroup);
			AssertEquals(consignor.PK, transitRating.ConsignorDocAddress.OrganisationPK);
			AssertEquals(consignee.PK, transitRating.ConsigneeDocAddress.OrganisationPK);
			AssertEquals(bookingParty.PK, transitRating.BookingPartyDocAddress.OrganisationPK);
		}

		public void TestITransportJobForRating_FreightMode()
		{
			var consignment = Factory.New<WhsItemReceiveConsignment>();
			var transitRating = (ITransitJobForRating)consignment;
			AssertNull(transitRating.FreightMode);

			consignment.WRC_TransportMode = TransportModes.Air;
			AssertEquals(FreightMode.AIR, transitRating.FreightMode);

			consignment.WRC_TransportMode = TransportModes.AirSea;
			AssertEquals(FreightMode.AIR, transitRating.FreightMode);

			consignment.WRC_TransportMode = TransportModes.Sea;
			AssertEquals(FreightMode.SEA, transitRating.FreightMode);

			consignment.WRC_TransportMode = TransportModes.SeaAir;
			AssertEquals(FreightMode.SEA, transitRating.FreightMode);

			consignment.WRC_TransportMode = TransportModes.Road;
			AssertEquals(FreightMode.ROA, transitRating.FreightMode);

			consignment.WRC_TransportMode = TransportModes.Courier;
			AssertEquals(FreightMode.UKN, transitRating.FreightMode);
		}

		[TestDate(2019, 11, 15)]
		public void TestITransportConsignmentForRating_ExpectedArrivalDate()
		{
			var now = ZDateTime.Now;
			ITransitJobForRating noPackageStates = Factory.New<WhsItemDispatchConsignment>();
			AssertEquals(now, noPackageStates.ExpectedArrivalDate);
			AssertEquals(now, noPackageStates.ExpectedDepartureDate);

			var withoutReceiveConsignment = Factory.New<WhsItemDispatchConsignment>();
			withoutReceiveConsignment.PackageStates.AddNew();
			AssertEquals(now, ((ITransitJobForRating)withoutReceiveConsignment).ExpectedArrivalDate);
			AssertEquals(now, ((ITransitJobForRating)withoutReceiveConsignment).ExpectedDepartureDate);

			var withReceiveConsignmentNoDates = Factory.New<WhsItemDispatchConsignment>();
			var receiveConsignment1 = Factory.New<WhsItemReceiveConsignment>();
			var packageState1 = withReceiveConsignmentNoDates.PackageStates.AddNew();
			packageState1.WPS_WRC_TransitReceiveConsignment = receiveConsignment1.PK;
			AssertEquals(now, ((ITransitJobForRating)withReceiveConsignmentNoDates).ExpectedArrivalDate);
			AssertEquals(now, ((ITransitJobForRating)withReceiveConsignmentNoDates).ExpectedDepartureDate);

			var withReceiveConsignmentAndDates = Factory.New<WhsItemDispatchConsignment>();
			receiveConsignment1.WRC_ExpectedArrivalTime = now.AddDays(-2);
			receiveConsignment1.WRC_ExpectedDispatchTime = now.AddDays(2);

			var packageState2 = withReceiveConsignmentAndDates.PackageStates.AddNew();
			packageState2.WPS_WRC_TransitReceiveConsignment = receiveConsignment1.PK;
			AssertEquals(receiveConsignment1.WRC_ExpectedArrivalTime, ((ITransitJobForRating)withReceiveConsignmentAndDates).ExpectedArrivalDate);
			AssertEquals(receiveConsignment1.WRC_ExpectedDispatchTime, ((ITransitJobForRating)withReceiveConsignmentAndDates).ExpectedDepartureDate);

			var receiveConsignment3 = Factory.New<WhsItemReceiveConsignment>();
			receiveConsignment3.WRC_ExpectedArrivalTime = now.AddDays(-3);
			receiveConsignment3.WRC_ExpectedDispatchTime = now.AddDays(4);
			var packageState3 = withReceiveConsignmentAndDates.PackageStates.AddNew();
			packageState3.WPS_WRC_TransitReceiveConsignment = receiveConsignment3.PK;
			AssertEquals(receiveConsignment3.WRC_ExpectedArrivalTime, ((ITransitJobForRating)withReceiveConsignmentAndDates).ExpectedArrivalDate);
			AssertEquals(receiveConsignment3.WRC_ExpectedDispatchTime, ((ITransitJobForRating)withReceiveConsignmentAndDates).ExpectedDepartureDate);
		}

		#endregion

		#region ITransitJobForConsolCosting Members

		public void TestITransitJobForConsolCosting()
		{
			var consignment = Factory.New<WhsItemDispatchConsignment>();
			consignment.WDC_ConsignmentID = "DCN001";
			var consolCosting = (ITransitJobForConsolCosting)consignment;

			AssertEquals("DCN001", consolCosting.HouseBillNumber);

			consignment.WDC_HouseBillNumber = "HB0000001";

			AssertEquals("HB0000001", consolCosting.HouseBillNumber);

			consignment.WDC_RL_NKDestination = "CNNJG";

			AssertEquals("CNNJG", consolCosting.Destination.Code);
		}

		public void TestPackageStatesForConsolCosting()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;
			var rcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			var dcn = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);

			var package1 = Helper.CreatePackageState(rcn, 1, "PKG", "P1", TransitWarehouseStatuses.Codes.FreightLoaded, rtu, dcn, dtu, dll);
			package1.WPS_WL_LastLocation = stageLocation.PK;
			package1.WPS_WL_ReceiveLocation = stageLocation.PK;
			var package2 = Helper.CreatePackageState(rcn, 1, "PLT", "P2", TransitWarehouseStatuses.Codes.Arrived, rtu);

			var handlingUnit = Helper.CreatePackageHandlingUnit();
			var handlingUnitPackage = Helper.CreateHandlingUnitPackage("HU1", handlingUnit, rtu);
			var childPackageState = Helper.CreatePackageState(rcn, 1, "PKG", "PC1", TransitWarehouseStatuses.Codes.FreightLoaded, rtu, dcn, dtu, dll);
			childPackageState.Package.KP_KP_TopHandlingUnitPackage = handlingUnitPackage.WPS_KP_Package;
			childPackageState.WPS_WL_LastLocation = stageLocation.PK;
			childPackageState.WPS_WL_ReceiveLocation = stageLocation.PK;

			Helper.DisableTopLevelHUFKForTest(TestConnection);
			Helper.PackPackageIntoHandlingUnit(handlingUnitPackage, childPackageState, ZDateTimeOffset.Now, "ABC");

			var consolCosting = (ITransitJobForConsolCosting)dcn;
			AssertContainsExactElementsInAnyOrder(new WhsItemPackageState[] { package1, childPackageState }, consolCosting.PackageStatesForConsolCosting);
		}

		#endregion

		#region TestIWorkflowProvider Members

		public void TestIWorkflowProviderMembers()
		{
			var consignment = Factory.New<WhsItemDispatchConsignment>();
			var newWorkflowItem = consignment.WorkflowItems.AddNew();
			var workflowProvider = (IWorkflowProvider)consignment;

			AssertEquals(WorkflowDescriptors.TransitDispatchConsignment, workflowProvider.WorkflowType);
			AssertContainsExactElementsInAnyOrder(new[] { newWorkflowItem }, workflowProvider.WorkflowItems);
			AssertEquals(typeof(ColumnValueRanker), workflowProvider.GetTemplateSelectionCriteria().GetType());
			AssertNull(workflowProvider.GetWorkflowInformationProvider());
		}

		public void TestGetTemplateSelectionCriteria()
		{
			var consignee1 = Factory.NewWithValidTestData<OrgHeader>();
			var intendedWarehouse = Factory.NewWithValidTestData<WhsWarehouse>();

			var consignmentWithoutHeader = Factory.NewWithValidTestData<WhsItemDispatchConsignment>();
			consignmentWithoutHeader.ConsigneeDocAddress.E2_OA_Address = consignee1.MainAddress.PK;
			consignmentWithoutHeader.WDC_WW_Warehouse = intendedWarehouse.PK;
			Factory.Save();

			var criteriaForConsignmentWithoutHeader = (ColumnValueRanker)((IWorkflowProvider)consignmentWithoutHeader).GetTemplateSelectionCriteria();
			AssertContainsExactElementsInAnyOrder(new[] { consignee1.PK, ZGuid.Empty }, criteriaForConsignmentWithoutHeader.GetValues(ProcessTaskTemplateSchema.P0_OH_Client));
			AssertContainsExactElementsInAnyOrder(new[] { intendedWarehouse.PK, ZGuid.Empty }, criteriaForConsignmentWithoutHeader.GetValues(ProcessTaskTemplateSchema.P0_WW));
		}

		#endregion

		#region TestIPackingParentWithOutturn Members

		public void TestIPackingParentWithOutturn_OutturnProvider_NoDispatchTransportationUnit()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var receiveConsignment = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var dispatchConsignment = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var packingParentWithOutturn = (IPackingParentWithOutturn)dispatchConsignment;
			var package = Helper.CreatePackage(receiveConsignment.PackageJob, "P1", 1, "PLT");
			var packageState = Helper.CreatePackageState(package, "BKD", dispatchConsignment: dispatchConsignment);

			var outturnProvider = packingParentWithOutturn.GetOutturnProvider(package);

			AssertEquals("Transport reference is empty if there is no DTU.", string.Empty, outturnProvider.ActualTransportJobID);
		}

		public void TestIPackingParentWithOutturn_OutturnProvider_WithVehicle()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var receiveConsignment = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var dispatchConsignment = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var dispatchLoadList = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			var packingParentWithOutturn = (IPackingParentWithOutturn)dispatchConsignment;
			var dispatchTransportationUnit = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			dispatchTransportationUnit.WDH_VehicleReference = "VEH1";
			var package = Helper.CreatePackage(receiveConsignment.PackageJob, "P1", 1, "PLT");
			var packageState = Helper.CreatePackageState(package, "FLO", dispatchConsignment: dispatchConsignment, dispatchUnit: dispatchTransportationUnit, dispatchLoadList: dispatchLoadList);

			var outturnProvider = packingParentWithOutturn.GetOutturnProvider(package);

			AssertEquals("Actual Transport reference is the container number if the DTU is not a container.", "DTU1", outturnProvider.ActualTransportJobID);
			AssertEquals("Actual Transport Job Type Code.", TWAdditionalReferenceTypesForUniversalXML.Codes.TransitWarehouseDispatchTransportationUnit, outturnProvider.ActualTransportJobTypeCode);
			AssertEquals("Actual Transport Job Type Description.", TWAdditionalReferenceTypesForUniversalXML.Descriptions.TransitWarehouseDispatchTransportationUnit, outturnProvider.ActualTransportJobTypeDescription);

			AssertEquals("Expected Transport reference is the DLL number.", "DLL1", outturnProvider.ExpectedTransportJobID);
			AssertEquals("Expected Transport Job Type Code.", TWAdditionalReferenceTypesForUniversalXML.Codes.TransitWarehouseDispatchLoadList, outturnProvider.ExpectedTransportJobTypeCode);
			AssertEquals("Expected Transport Job Type Description.", TWAdditionalReferenceTypesForUniversalXML.Descriptions.TransitWarehouseDispatchLoadList, outturnProvider.ExpectedTransportJobTypeDescription);

			var containerTuple = packingParentWithOutturn.GetParentContainer(package);
			AssertNull("Container is empty if the DTU is not a container.", containerTuple.Container);
			AssertNull("Container Number is empty if the DTU is not a container.", containerTuple.ContainerNumber);
		}

		public void TestIPackingParentWithOutturn_OutturnProvider_WithContainer()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var receiveConsignment = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var dispatchConsignment = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var packingParentWithOutturn = (IPackingParentWithOutturn)dispatchConsignment;
			var dispatchTransportationUnit = Helper.CreateDispatchTransportationUnitWithContainerType("DTUREF", warehouse.PK);
			dispatchTransportationUnit.WDH_VehicleReference = "VEH1";
			var container = dispatchTransportationUnit.Container;
			var package = Helper.CreatePackage(receiveConsignment.PackageJob, "P1", 1, "PLT");
			var packageState = Helper.CreatePackageState(package, "FLO", dispatchConsignment: dispatchConsignment, dispatchUnit: dispatchTransportationUnit);

			var outturnProvider = packingParentWithOutturn.GetOutturnProvider(package);

			AssertEquals("Transport reference is Container's DTU ID.", "DTUREF", outturnProvider.ActualTransportJobID);

			var containerTuple = packingParentWithOutturn.GetParentContainer(package);
			AssertEquals(container, containerTuple.Container);
			AssertEquals("VEH1", containerTuple.ContainerNumber);
		}

		public void TestIPackingParentWithOutturn_OutturnProvider_NoPackageState_DoesNotThrow()
		{
			var consignment = Factory.New<WhsItemDispatchConsignment>();
			var packingParentWithOutturn = (IPackingParentWithOutturn)consignment;
			var package = Factory.New<PkgPackage>();

			IOutturnProvider outturnProvider = null;
			AssertNoExceptionThrown(() => packingParentWithOutturn.GetParentContainer(package));
			AssertNoExceptionThrown(() => outturnProvider = packingParentWithOutturn.GetOutturnProvider(package));
			AssertNotNull(outturnProvider);
			AssertEquals(string.Empty, outturnProvider.ActualTransportJobID);
		}

		[TestDate(2020, 08, 28, 04, 39, 50)]
		public void TestIPackingParentWithOutturn_ContainerView()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var dispatchConsignment = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var packingParentWithOutturn = (IPackingParentWithOutturn)dispatchConsignment;
			var dispatchTransportationUnit = Helper.CreateDispatchTransportationUnitWithContainerType("DTU1", warehouse.PK);
			var currentTime = ZDateTimeOffset.Now;
			dispatchTransportationUnit.WDH_LoadCompleteTime = currentTime;
			var container = dispatchTransportationUnit.Container;

			var containerView = packingParentWithOutturn.GetContainerView(container);

			AssertNotNull(containerView);
			AssertEquals(currentTime.ToZDateTime(), containerView.PackCompleteDate);
			AssertNull(containerView.UnpackCompleteDate);
		}

		public void TestIPackingParentWithOutturn_ContainerView_NoDispatchTransportationUnit()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var receiveConsignment = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var dispatchConsignment = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var packingParentWithOutturn = (IPackingParentWithOutturn)dispatchConsignment;
			var package = receiveConsignment.PackageJob.Containers.AddNew();

			var containerView = packingParentWithOutturn.GetContainerView(package.Container);

			AssertNotNull(containerView);
			AssertNull(containerView.PackCompleteDate);
			AssertNull(containerView.UnpackCompleteDate);
		}

		public void TestIPackingParentWithOutturn_ContainerView_InvalidDate()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var dispatchConsignment = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var packingParentWithOutturn = (IPackingParentWithOutturn)dispatchConsignment;
			var dispatchTransportationUnit = Helper.CreateDispatchTransportationUnitWithContainerType("DTU1", warehouse.PK);
			dispatchTransportationUnit.WDH_LoadCompleteTime = ZDateTimeOffset.Invalid;
			var container = dispatchTransportationUnit.Container;

			var containerView = packingParentWithOutturn.GetContainerView(container);

			AssertNotNull(containerView);
			AssertNull(containerView.PackCompleteDate);
			AssertNull(containerView.UnpackCompleteDate);
		}

		public void TestIPackingParentWithOutturn_ContainerView_NoDates()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var dispatchConsignment = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var packingParentWithOutturn = (IPackingParentWithOutturn)dispatchConsignment;
			var dispatchTransportationUnit = Helper.CreateDispatchTransportationUnitWithContainerType("DTU1", warehouse.PK);
			var container = dispatchTransportationUnit.Container;

			var containerView = packingParentWithOutturn.GetContainerView(container);

			AssertNotNull(containerView);
			AssertNull(containerView.PackCompleteDate);
			AssertNull(containerView.UnpackCompleteDate);
		}

		#endregion

		#region TestGetAttachedJobNumber Member

		public void TestGetAttachedJobNumber()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var rcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var dcn = helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var package = Helper.CreatePackageState(rcn, 1, "PKG", "P1", "BKD", dispatchConsignment: dcn);
			Helper.CreateAdditionalReference(package, "S0000001", "BPR");
			Factory.Save();
			AssertEquals("", "S0000001", ((IPackingParentWithAttachedParent)dcn).GetAttachedJobNumber(package.Package));
		}

		#endregion

		#region GetAdditionalReferenceNumberTypeList

		public void TestGetAdditionalReferenceNumberTypeList()
		{
			var dispatchConsignment = Factory.New<WhsItemDispatchConsignment>();
			var warehouseRegistryReferenceNumbers = WarehouseDataRegistry.Instance.AdditionalReferenceType.GetFallBackValueAtAllLevels(Guid.Empty, Guid.Empty, Guid.Empty).GetCodeDescriptionPairList();

			var additionalReferenceNumberTypeList = ((IAdditionalReferenceNumberTypeProvider)dispatchConsignment).GetAdditionalReferenceNumberTypeList
			(
				CusEntryNumber.Categories.AdditionalReferenceNumber,
				""
			);

			AssertContainsExactElementsInAnyOrder(warehouseRegistryReferenceNumbers.GetAllCodes(), additionalReferenceNumberTypeList.GetAllCodes());
		}

		public void TestGetAdditionalReferenceNumberTypeList_InvalidCategory()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var dispatchConsignment = helper.CreateDispatchConsignment("DCN1", warehouse.PK);

			var additionalReferenceNumberTypeList = ((IAdditionalReferenceNumberTypeProvider)dispatchConsignment).GetAdditionalReferenceNumberTypeList
			(
				CusEntryNumber.Categories.InspectionStatus,
				""
			);

			AssertEquals("No reference numbers returned for invalid category.", 0, additionalReferenceNumberTypeList.Count);
		}

		#endregion

		#region TestNoteTypes

		public void TestNoteTypes()
		{
			var consignment = Factory.New<WhsItemDispatchConsignment>();
			AssertContainsExactElementsInAnyOrder(new PredefinedNoteType[] {
				PredefinedNoteTypes.Instance.AutoRatingAuditLog,
				PredefinedNoteTypes.Instance.UnrecognisedAdditionalReferenceTypes,
				PredefinedNoteTypes.Instance.UnmatchedOrgDetails,
				PredefinedNoteTypes.Instance.CIN750MessageNotes,
				PredefinedNoteTypes.Instance.CRESAMessageNotes,
				PredefinedNoteTypes.Instance.UnloadLoadNotes }, consignment.NoteTypes);
		}

		#endregion

		#region TestReceiveConsignmentHasVisualizableDocumentsSupportableAttribute

		public void TestDispatchConsignmentHasVisualizableDocumentsSupportableAttribute()
		{
			var consignment = Factory.New<WhsItemReceiveConsignment>();
			var attribute = consignment.GetType().GetCustomAttribute<VisualizableDocumentsSupportableAttribute>();
			AssertNotNull("Has VisualizableDocumentsSupportable attribute", attribute);
			AssertEquals("ReceiveConsignmentVisualizableDocumentSupporter", attribute.SupporterType.Name);
		}

		#endregion

		#region TestIConsignment Members

		public void TestIConsignment()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var dispatchConsignment = helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			dispatchConsignment.WDC_WW_Warehouse = warehouse.PK;
			dispatchConsignment.WDC_HouseBillNumber = "SSYD000000";
			dispatchConsignment.WDC_JobID = "RC000000001";
			dispatchConsignment.WDC_TransportMode = "SEA";
			dispatchConsignment.WDC_Direction = "IMP";

			var consignment = (IConsignment)dispatchConsignment;
			AssertEquals(consignment.Warehouse, warehouse);
			AssertEquals(consignment.BookingPartyDocAddress, dispatchConsignment.BookingPartyDocAddress);
			AssertEquals(consignment.HouseBillNumber, dispatchConsignment.WDC_HouseBillNumber);
			AssertEquals(consignment.JobID, dispatchConsignment.WDC_JobID);
			AssertEquals(consignment.TransportMode, dispatchConsignment.WDC_TransportMode);
			AssertEquals(consignment.Direction, dispatchConsignment.WDC_Direction);
		}

		#region TestIConsignmentForMasterBillNumber

		public void TestIConsignmentForMasterBillNumber_HasAdditionalReference()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var dispatchConsignment = helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var additionalReferenceInDispatch = dispatchConsignment.AdditionalReferenceNumbers.AddNew();
			additionalReferenceInDispatch.CE_EntryType = WarehouseAdditionalReferenceTypes.Codes.MasterBill;
			additionalReferenceInDispatch.CE_EntryNum = "MASTERBILL";

			var consignment = (IConsignment)dispatchConsignment;
			AssertEquals(consignment.MasterBillNumber, additionalReferenceInDispatch.CE_EntryNum);
		}

		public void TestIConsignmentForMasterBillNumber_HasEmptyAdditionalReference_HasDLLMasterBillNumber()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var receiveConsignment = helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var dispatchConsignment = helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var additionalReferenceInDispatch = dispatchConsignment.AdditionalReferenceNumbers.AddNew();
			additionalReferenceInDispatch.CE_EntryType = WarehouseAdditionalReferenceTypes.Codes.MasterBill;
			additionalReferenceInDispatch.CE_EntryNum = ZString.Empty;

			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			var additionalReferenceInDLL = dll.AdditionalReferenceNumbers.AddNew();
			additionalReferenceInDLL.CE_EntryType = WarehouseAdditionalReferenceTypes.Codes.MasterBill;
			additionalReferenceInDLL.CE_EntryNum = "MASTERBILL";

			Helper.CreatePackageState(receiveConsignment, 1, "PLT", "P1", TransitWarehouseStatuses.Codes.Arrived, dispatchConsignment: dispatchConsignment, dispatchLoadList: dll);

			var consignment = (IConsignment)dispatchConsignment;
			AssertEquals(consignment.MasterBillNumber, additionalReferenceInDLL.CE_EntryNum);
		}

		public void TestIConsignmentForMasterBillNumber_NoAdditionalReference_HasDLLMasterBillNumber()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var receiveConsignment = helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var dispatchConsignment = helper.CreateDispatchConsignment("DCN1", warehouse.PK);

			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			var additionalReferenceInDLL = dll.AdditionalReferenceNumbers.AddNew();
			additionalReferenceInDLL.CE_EntryType = WarehouseAdditionalReferenceTypes.Codes.MasterBill;
			additionalReferenceInDLL.CE_EntryNum = "MASTERBILL";

			Helper.CreatePackageState(receiveConsignment, 1, "PLT", "P1", TransitWarehouseStatuses.Codes.Arrived, dispatchConsignment: dispatchConsignment, dispatchLoadList: dll);

			var consignment = (IConsignment)dispatchConsignment;
			AssertEquals(consignment.MasterBillNumber, additionalReferenceInDLL.CE_EntryNum);
		}

		public void TestIConsignmentForMasterBillNumber_NoAdditionalReference_HasEmptyDLLMasterBillNumber()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var dispatchConsignment = helper.CreateDispatchConsignment("DCN1", warehouse.PK);

			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			var additionalReferenceInDLL = dll.AdditionalReferenceNumbers.AddNew();
			additionalReferenceInDLL.CE_EntryType = WarehouseAdditionalReferenceTypes.Codes.MasterBill;
			additionalReferenceInDLL.CE_EntryNum = ZString.Empty;

			var consignment = (IConsignment)dispatchConsignment;
			AssertEquals(consignment.MasterBillNumber, ZString.Empty);
		}

		public void TestIConsignmentForMasterBillNumber_NoAdditionalReference_NoDLLMasterBillNumber()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var dispatchConsignment = helper.CreateDispatchConsignment("DCN1", warehouse.PK);

			var consignment = (IConsignment)dispatchConsignment;
			AssertEquals(consignment.MasterBillNumber, ZString.Empty);
		}

		#endregion

		#endregion

		#region IHaveServices Members

		public void TestNeedsServiceEvents()
		{
			var consignment = (IHaveServices)Factory.New<WhsItemDispatchConsignment>();
			AssertEquals(true, consignment.NeedsServiceEvents);
		}

		public void TestJobServiceDeleted()
		{
			var warehouse = Helper.CreateWarehouse("WHS", true);
			var rcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var dcn = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);

			var rtu = Helper.CreateReceiveTransportationUnit("RTU12", warehouse.PK, warehouse.DefaultInboundDockDoorLocation.PK);
			var jobService = Helper.CreateJobService(dcn.PK, dcn.TablePrefix, "FUM");
			dcn.Services.Add(jobService);

			var unloadTask1 = Helper.CreateUnloadTask(rtu.PK, warehouse.PK, rcn.PK);
			unloadTask1.WUT_ES_ActiveJobService = jobService.PK;

			var unloadTask2 = Helper.CreateUnloadTask(rtu.PK, warehouse.PK, rcn.PK);
			unloadTask2.WUT_ES_ActiveJobService = jobService.PK;

			Factory.Save();

			AssertEquals("Active Job Service should be present on Unload Task", jobService.PK, unloadTask1.WUT_ES_ActiveJobService);
			AssertEquals("Active Job Service should be present on Unload Task", jobService.PK, unloadTask2.WUT_ES_ActiveJobService);

			var jobServicesParent = (IHaveServices)dcn;
			jobServicesParent.JobServiceDeleted(jobService.PK);

			AssertEquals("Active Job Service should be cleared on Unload Task", Guid.Empty, unloadTask1.WUT_ES_ActiveJobService);
			AssertEquals("Active Job Service should be cleared on Unload Task", Guid.Empty, unloadTask2.WUT_ES_ActiveJobService);
		}

		public void TestServiceBranch()
		{
			var consignment = Factory.New<WhsItemDispatchConsignment>();
			var iHaveServices = (IHaveServices)consignment;
			AssertNull("No service branch", iHaveServices.ServiceBranch);

			var warehouse = Helper.CreateWarehouse("WHS", true);
			AssertEquals(true, warehouse.WW_GB_RelatedCompanyBranch.IsValid);

			consignment.WDC_WW_Warehouse = warehouse.PK;
			AssertEquals("Service branch is warehouse branch", warehouse.WW_GB_RelatedCompanyBranch, iHaveServices.ServiceBranch.PK);
		}

		#endregion

		#region IHaveEventsFromServices Members

		public void TestReferenceParameters()
		{
			var warehouseWithAddress = Helper.CreateTransitWarehouseInCurrentBranch("WH1");
			warehouseWithAddress.WarehouseAddress.City = "SYD";
			var consignment = Factory.New<WhsItemDispatchConsignment>();
			consignment.WDC_WW_Warehouse = warehouseWithAddress.PK;

			var referenceParameters = ((IHaveEventsFromServices)consignment).ReferenceParameters.ToList();
			AssertEquals(2, referenceParameters.Count);
			AssertEquals(CargoWise.EventReference.Constants.Facilities.Code.Depot, referenceParameters.Find(r => r.Key == CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Facility).Value);
			AssertEquals("SYD", referenceParameters.Find(r => r.Key == CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Location).Value);
		}

		#endregion

		#region IDtbBookingParent Members

		public void TestIDtbBookingParent()
		{
			var dcn = Factory.New<WhsItemDispatchConsignment>();
			var dtbBookingParent = dcn as IDtbBookingParent;

			dcn.WDC_ConsignmentID = "DCN01";
			dcn.WDC_JobID = "DC00000001";
			AssertEquals("Dispatch Consignment", dtbBookingParent.JobTypeDescription);
			AssertEquals("DC00000001", dtbBookingParent.JobNumber);
			AssertEquals(WorkflowDescriptors.TransitDispatchConsignment, dtbBookingParent.JobType);

			AssertContainsExactElementsInAnyOrder(new DtbBookingDirection[] { DtbBookingDirection.DLV }, dtbBookingParent.GetSupportedDirections());
		}

		public void TestIDtbBookingParent_ControllerID()
		{
			var dcn = Factory.New<WhsItemDispatchConsignment>();
			var dtbBookingParent = dcn as IDtbBookingParent;
			AssertEquals(ControllerIDs.WhsTransitDispatchConsignment, dtbBookingParent.ControllerID);
		}

		public void TestCanCreateTransportBooking()
		{
			var dcn = Factory.New<WhsItemDispatchConsignment>();
			IDtbBookingParent dtbBookingParent = dcn;
			AssertEquals("CanCreateTransportBooking should always return true", true, dtbBookingParent.CanCreateTransportBooking);
		}

		public void TestBookingParentPKAndBookingParentTablePrefix_WhenParentDoesNotExist()
		{
			var dcn = Factory.NewWithValidTestData<WhsItemDispatchConsignment>();
			dcn.WDC_ParentID = ZGuid.Empty;
			dcn.WDC_ParentTableCode = ZString.Empty;

			IDtbBookingParent dtbBookingParent = dcn;

			AssertEquals("BookingParentPK should be the Dispatch Consignment PK.", dcn.PK, dtbBookingParent.BookingParentPK);
			AssertEquals("BookingParentTablePrefix should be the Dispatch Consignment TablePrefix.", dcn.TablePrefix, dtbBookingParent.BookingParentTablePrefix);
		}

		public void TestBookingParentPKAndBookingParentTablePrefix_WhenParentIsNotAnIDtbBookingParent()
		{
			var parent = (BusinessObject)Factory.New<IDtbConsignment>();
			parent.FillWithValidTestData();
			AssertEquals("Precondition: The parent of the Consignment should not be an IDtbBookingParent", false, parent is IDtbBookingParent);

			var dcn = Factory.NewWithValidTestData<WhsItemDispatchConsignment>();
			dcn.WDC_JobID = "TestID123";
			dcn.WDC_ParentID = parent.PK;
			dcn.WDC_ParentTableCode = parent.TablePrefix;

			Factory.Save();

			IDtbBookingParent dtbBookingParent = dcn;

			AssertEquals("BookingParentPK should be the Dispatch Consignment PK.", dcn.PK, dtbBookingParent.BookingParentPK);
			AssertEquals("BookingParentTablePrefix should be the Dispatch Consignment TablePrefix.", dcn.TablePrefix, dtbBookingParent.BookingParentTablePrefix);
		}

		public void TestBookingParentPKAndBookingParentTablePrefix_WhenValidParentExists()
		{
			var parent = Factory.New<ForwardingShipment>();
			AssertEquals("Precondition: The parent of the Consignment should be an IDtbBookingParent", true, parent is IDtbBookingParent);

			var dcn = Factory.NewWithValidTestData<WhsItemDispatchConsignment>();
			dcn.WDC_JobID = "TestID123";
			dcn.WDC_ParentID = parent.PK;
			dcn.WDC_ParentTableCode = parent.TablePrefix;

			Factory.Save();

			IDtbBookingParent dtbBookingParent = dcn;

			AssertEquals("BookingParentPK should be the parent Shipment PK.", parent.PK, dtbBookingParent.BookingParentPK);
			AssertEquals("BookingParentTablePrefix should be the parent Shipment TablePrefix.", parent.TablePrefix, dtbBookingParent.BookingParentTablePrefix);
		}

		public void TestGetExtendingConfirmMessageBeforeCreateTransportBooking()
		{
			var dcn = Factory.New<WhsItemDispatchConsignment>();
			var dtbBookingParent = dcn as IDtbBookingParent;

			var (isShouldShow, caption, message, confirmation) = dtbBookingParent.GetExtendingConfirmMessageBeforeCreateTransportBooking();
			AssertEquals("IsShouldShow should return false.", false, isShouldShow);
			AssertNullOrEmpty("Caption should be null.", caption);
			AssertNullOrEmpty("Message should be null.", message);
			AssertNullOrEmpty("Confirmation should be null.", confirmation);
		}

		public void TestIDtbBookingParent_InvoicingJob()
		{
			var dcn = Factory.New<WhsItemDispatchConsignment>();
			var dtbBookingParent = dcn as IDtbBookingParent;
			AssertEquals("Dispatch consignment should be returned by InvoicingJob, so that the Billing tab on a Transport Booking of a dispatch consignment is available", dtbBookingParent, dtbBookingParent.InvoicingJob);
		}

		public void TestIDtbBookingParent_TransportBookingTemplateFilters()
		{
			var dcn = Factory.New<WhsItemDispatchConsignment>();
			var dtbBookingParent = dcn as IDtbBookingParent;
			AssertEquals("TransportBookingTemplateFilters should be default query", string.Empty, dtbBookingParent.TransportBookingTemplateFilters.LiteralTextSqlFormatted);
		}

		public void TestTransportBookingCreatedOrUpdated()
		{
			var dcn = Factory.NewWithValidTestData<WhsItemDispatchConsignment>();
			var consolidation = Factory.New<IDtbBookingConsolidation>();
			var transportBooking = Factory.New<IDtbBooking>();
			transportBooking.KM_KB_Booking = consolidation.PK;
			transportBooking.KM_JobID = "TB001";
			Factory.Save();

			var bookingParent = dcn as IDtbBookingParent;
			bookingParent.TransportBookingCreatedOrUpdated(new[] { transportBooking });

			var jobLinkForDCN = Factory.LoadTop1<StmUniversalJobLink>(new ZQuery(StmUniversalJobLinkSchema.UCL_ParentID, dcn.PK));
			var jobLinkForTB = Factory.LoadTop1<StmUniversalJobLink>(new ZQuery(StmUniversalJobLinkSchema.UCL_ParentID, transportBooking.PK));
			AssertNotNull("universal link from DCN to TB has created", jobLinkForDCN);
			AssertEquals("universal link of DCN has linked to TB", jobLinkForDCN.UCL_SourceKey, transportBooking.KM_JobID);
			AssertNotNull("universal link from TB to DCN has created", jobLinkForTB);
			AssertEquals("universal link of TB has linked to DCN", jobLinkForTB.UCL_SourceKey, dcn.WDC_JobID);
		}

		#endregion

		#region MasterBillNumber

		public void TesttMasterBillNumber_GetFromDLL()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			var dcn = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var dll1 = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			var dll2 = Helper.CreateDispatchLoadList("DLL2", warehouse.PK);
			Helper.CreateAdditionalReference(dll2, "MAB0001", TransportAdditionalReferenceTypes.Codes.MasterBill);
			Helper.CreatePackageState(rcn, 1, "PKG", "PKG-1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: dll1);
			Helper.CreatePackageState(rcn, 1, "PKG", "PKG-2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: dll2);
			Factory.Save();

			AssertEquals("MAB0001", dcn.MasterBillNumber);
		}

		public void TesttMasterBillNumber_GetNull()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			var dcn = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			Helper.CreatePackageState(rcn, 1, "PKG", "PKG-1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: dll);
			Factory.Save();

			AssertNullOrEmpty(dcn.MasterBillNumber);
		}

		#endregion

		#region ICancellable Member

		public void TestICancellableMembers()
		{
			var consignment = Factory.New<WhsItemDispatchConsignment>();

			AssertEquals(false, consignment.IsCancelled);
			AssertEquals(true, consignment.IsCancelledHasChanged);
			AssertNull(consignment.CanCancel());
			AssertNull(consignment.CanReactivate());
		}

		#endregion

		#region TestIsAnyRelatedRCNBlind

		public void TestIsAnyRelatedRCNBlind_IsBlind()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var rcn1 = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			rcn1.WRC_ParentID = ZGuid.Empty;
			var rcn2 = Helper.CreateReceiveConsignment("RCN2", warehouse.PK);
			rcn2.WRC_ParentID = ZGuid.ParseSafe("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");
			var dcn = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			Helper.CreatePackageState(rcn1, 1, "PKG", "P1", "ARV", dispatchConsignment: dcn);
			Helper.CreatePackageState(rcn2, 1, "PKG", "P2", "ARV", dispatchConsignment: dcn);

			AssertEquals(true, dcn.IsAnyRelatedRCNBlind);
		}

		public void TestIsAnyRelatedRCNBlind_NotBlind()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var rcn1 = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			rcn1.WRC_ParentID = ZGuid.ParseSafe("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");
			var dcn = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			Helper.CreatePackageState(rcn1, 1, "PKG", "P1", "ARV", dispatchConsignment: dcn);
			Helper.CreatePackageState(rcn1, 1, "PKG", "P2", "ARV", dispatchConsignment: dcn);

			AssertEquals(false, dcn.IsAnyRelatedRCNBlind);
		}

		public void TestIsAnyRelatedRCNBlind_NoPackages()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var rcn1 = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			rcn1.WRC_ParentID = ZGuid.ParseSafe("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");
			var dcn = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);

			AssertEquals(false, dcn.IsAnyRelatedRCNBlind);
		}

		#endregion

		#region Test OuterPackages

		public void TestOuterPackages()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			var receiveConsignment = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var dcn = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);

			var bookedPackage = Helper.CreatePackageState(receiveConsignment, 1, TransitConstants.TransitPackageUnitType.Package, "PKG1", TransitWarehouseStatuses.Codes.Booked, dispatchConsignment: dcn);
			var arrivedPackage = Helper.CreatePackageState(receiveConsignment, 1, TransitConstants.TransitPackageUnitType.Package, "PKG2", TransitWarehouseStatuses.Codes.Arrived, rtu, dcn);
			var arrivedPackage2 = Helper.CreatePackageState(receiveConsignment, 1, TransitConstants.TransitPackageUnitType.Package, "PKG2", TransitWarehouseStatuses.Codes.Arrived, rtu);

			Factory.Save();

			var outerPackages = dcn.OuterPackages;
			AssertEquals(2, outerPackages.Count());
			AssertContainsExactElementsInAnyOrder(new[] { bookedPackage.Package.PK, arrivedPackage.Package.PK }, outerPackages.Select(p => p.PK));

			var outerPackageStates = dcn.OuterPackageStates;
			AssertEquals(2, outerPackageStates.Count());
			AssertContainsExactElementsInAnyOrder(new[] { bookedPackage.PK, arrivedPackage.PK }, outerPackageStates.Select(p => p.PK));
		}

		public void TestOuterPackages_Container()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			var receiveConsignment = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var uld = Helper.CreateReceiveTransportationUnitWithContainerType("ULD1", warehouse.PK, location.PK);
			var dcn = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);

			var bookedPackage = Helper.CreatePackageState(receiveConsignment, 1, TransitConstants.TransitPackageUnitType.Package, "PKG1", TransitWarehouseStatuses.Codes.Booked, dispatchConsignment: dcn);
			var arrivedPackage = Helper.CreatePackageState(receiveConsignment, 1, TransitConstants.TransitPackageUnitType.Package, "PKG2", TransitWarehouseStatuses.Codes.Arrived, uld, dcn);
			var arrivedPackage2 = Helper.CreatePackageState(receiveConsignment, 1, TransitConstants.TransitPackageUnitType.Package, "PKG2", TransitWarehouseStatuses.Codes.Arrived, uld);

			Factory.Save();

			var outerPackages = dcn.OuterPackages;
			AssertEquals(2, outerPackages.Count());
			AssertContainsExactElementsInAnyOrder(new[] { bookedPackage.Package.PK, arrivedPackage.Package.PK }, outerPackages.Select(p => p.PK));

			var outerPackageStates = dcn.OuterPackageStates;
			AssertEquals(2, outerPackageStates.Count());
			AssertContainsExactElementsInAnyOrder(new[] { bookedPackage.PK, arrivedPackage.PK }, outerPackageStates.Select(p => p.PK));
		}

		public void TestOuterPackages_OVP()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			var receiveConsignment = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var dcn = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);

			var ovp = Helper.CreateOverpackPackage("OVP1", receiveConsignment, rtu, status: TransitWarehouseStatuses.Codes.Arrived, rcn: receiveConsignment, dcn: dcn);
			var innerPackage = Helper.CreatePackageState(receiveConsignment, 1, TransitConstants.TransitPackageUnitType.Package, "Inner1", TransitWarehouseStatuses.Codes.Arrived, rtu, dcn);
			var innerPackline = Helper.CreatePackageState(receiveConsignment, 10, TransitConstants.TransitPackageUnitType.Package, "", TransitWarehouseStatuses.Codes.Arrived, rtu, dcn);
			helper.PackPackageIntoHandlingUnit(ovp, innerPackage, ZDateTimeOffset.Now, "ABC", ovp);
			helper.PackPackageIntoHandlingUnit(ovp, innerPackline, ZDateTimeOffset.Now, "ABC", ovp);

			Factory.Save();

			var outerPackages = dcn.OuterPackages;
			AssertEquals(1, outerPackages.Count());
			AssertContainsExactElementsInAnyOrder(new[] { ovp.Package.PK }, outerPackages.Select(p => p.PK));

			var outerPackageStates = dcn.OuterPackageStates;
			AssertEquals(1, outerPackageStates.Count());
			AssertContainsExactElementsInAnyOrder(new[] { ovp.PK }, outerPackageStates.Select(p => p.PK));
		}

		public void TestOuterPackages_OVPInContainer()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			var receiveConsignment = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var uld = Helper.CreateReceiveTransportationUnitWithContainerType("ULD1", warehouse.PK, location.PK);
			var dcn = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);

			var ovp = Helper.CreateOverpackPackage("OVP1", receiveConsignment, uld, status: TransitWarehouseStatuses.Codes.Arrived, rcn: receiveConsignment, dcn: dcn);
			var innerPackage = Helper.CreatePackageState(receiveConsignment, 1, TransitConstants.TransitPackageUnitType.Package, "Inner1", TransitWarehouseStatuses.Codes.Arrived, uld, dcn);
			var innerPackline = Helper.CreatePackageState(receiveConsignment, 10, TransitConstants.TransitPackageUnitType.Package, "", TransitWarehouseStatuses.Codes.Arrived, uld, dcn);
			helper.PackPackageIntoHandlingUnit(ovp, innerPackage, ZDateTimeOffset.Now, "ABC", ovp);
			helper.PackPackageIntoHandlingUnit(ovp, innerPackline, ZDateTimeOffset.Now, "ABC", ovp);

			Factory.Save();

			var outerPackages = dcn.OuterPackages;
			AssertEquals(1, outerPackages.Count());
			AssertContainsExactElementsInAnyOrder(new[] { ovp.Package.PK }, outerPackages.Select(p => p.PK));

			var outerPackageStates = dcn.OuterPackageStates;
			AssertEquals(1, outerPackageStates.Count());
			AssertContainsExactElementsInAnyOrder(new[] { ovp.PK }, outerPackageStates.Select(p => p.PK));
		}

		public void TestOuterPackages_OVPInHU()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			var receiveConsignment = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var dcn = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);

			var topHU = helper.CreateHandlingUnitPackage("TopHU1", helper.CreatePackageHandlingUnit(), rtu, TransitWarehouseStatuses.Codes.Arrived, unitType: PackageStateUnitType.Codes.HandlingUnit);
			var innerPackage1 = Helper.CreatePackageState(receiveConsignment, 1, TransitConstants.TransitPackageUnitType.Package, "Inner1", TransitWarehouseStatuses.Codes.Arrived, rtu, dcn);

			var ovp = Helper.CreateOverpackPackage("OVP1", receiveConsignment, rtu, status: TransitWarehouseStatuses.Codes.Arrived, rcn: receiveConsignment, dcn: dcn);
			var innerPackage2 = Helper.CreatePackageState(receiveConsignment, 1, TransitConstants.TransitPackageUnitType.Package, "Inner2", TransitWarehouseStatuses.Codes.Arrived, rtu, dcn);
			var innerPackline = Helper.CreatePackageState(receiveConsignment, 10, TransitConstants.TransitPackageUnitType.Package, "", TransitWarehouseStatuses.Codes.Arrived, rtu, dcn);
			helper.PackPackageIntoHandlingUnit(ovp, innerPackage2, ZDateTimeOffset.Now, "ABC", topHU);
			helper.PackPackageIntoHandlingUnit(ovp, innerPackline, ZDateTimeOffset.Now, "ABC", topHU);

			helper.PackPackageIntoHandlingUnit(topHU, innerPackage1, ZDateTimeOffset.Now, "ABC", topHU);
			helper.PackPackageIntoHandlingUnit(topHU, ovp, ZDateTimeOffset.Now, "ABC", topHU);
			Factory.Save();

			var outerPackages = dcn.OuterPackages;
			AssertEquals(2, outerPackages.Count());
			AssertContainsExactElementsInAnyOrder(new[] { ovp.Package.PK, innerPackage1.Package.PK }, outerPackages.Select(p => p.PK));

			var outerPackageStates = dcn.OuterPackageStates;
			AssertEquals(2, outerPackageStates.Count());
			AssertContainsExactElementsInAnyOrder(new[] { ovp.PK, innerPackage1.PK }, outerPackageStates.Select(p => p.PK));
		}

		public void TestOuterPackages_HU()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			var receiveConsignment = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var dcn = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);

			var hu = helper.CreateHandlingUnitPackage("HU1", helper.CreatePackageHandlingUnit(), rtu, TransitWarehouseStatuses.Codes.Arrived, unitType: PackageStateUnitType.Codes.HandlingUnit);
			var innerPackage = Helper.CreatePackageState(receiveConsignment, 1, TransitConstants.TransitPackageUnitType.Package, "Inner1", TransitWarehouseStatuses.Codes.Arrived, rtu, dcn);
			var innerPackline = Helper.CreatePackageState(receiveConsignment, 10, TransitConstants.TransitPackageUnitType.Package, "", TransitWarehouseStatuses.Codes.Arrived, rtu, dcn);
			helper.PackPackageIntoHandlingUnit(hu, innerPackage, ZDateTimeOffset.Now, "ABC", hu);
			helper.PackPackageIntoHandlingUnit(hu, innerPackline, ZDateTimeOffset.Now, "ABC", hu);

			Factory.Save();

			var outerPackages = dcn.OuterPackages;
			AssertEquals(2, outerPackages.Count());
			AssertContainsExactElementsInAnyOrder(new[] { innerPackage.Package.PK, innerPackline.Package.PK }, outerPackages.Select(p => p.PK));

			var outerPackageStates = dcn.OuterPackageStates;
			AssertEquals(2, outerPackageStates.Count());
			AssertContainsExactElementsInAnyOrder(new[] { innerPackage.PK, innerPackline.PK }, outerPackageStates.Select(p => p.PK));
		}

		public void TestOuterPackages_MultipleHU()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			var receiveConsignment = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var dcn = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);

			var topHU = helper.CreateHandlingUnitPackage("TopHU1", helper.CreatePackageHandlingUnit(), rtu, TransitWarehouseStatuses.Codes.Arrived, unitType: PackageStateUnitType.Codes.HandlingUnit);
			var innerPackage1 = Helper.CreatePackageState(receiveConsignment, 1, TransitConstants.TransitPackageUnitType.Package, "Inner1", TransitWarehouseStatuses.Codes.Arrived, rtu, dcn);

			var hu = helper.CreateHandlingUnitPackage("HU1", helper.CreatePackageHandlingUnit(), rtu, TransitWarehouseStatuses.Codes.Arrived, unitType: PackageStateUnitType.Codes.HandlingUnit);
			var innerPackage2 = Helper.CreatePackageState(receiveConsignment, 1, TransitConstants.TransitPackageUnitType.Package, "Inner2", TransitWarehouseStatuses.Codes.Arrived, rtu, dcn);
			var innerPackline = Helper.CreatePackageState(receiveConsignment, 10, TransitConstants.TransitPackageUnitType.Package, "", TransitWarehouseStatuses.Codes.Arrived, rtu, dcn);
			helper.PackPackageIntoHandlingUnit(hu, innerPackage2, ZDateTimeOffset.Now, "ABC", topHU);
			helper.PackPackageIntoHandlingUnit(hu, innerPackline, ZDateTimeOffset.Now, "ABC", topHU);

			helper.PackPackageIntoHandlingUnit(topHU, innerPackage1, ZDateTimeOffset.Now, "ABC", topHU);
			helper.PackPackageIntoHandlingUnit(topHU, hu, ZDateTimeOffset.Now, "ABC", topHU);

			Factory.Save();

			var outerPackages = dcn.OuterPackages;
			AssertEquals(3, outerPackages.Count());
			AssertContainsExactElementsInAnyOrder(new[] { innerPackage1.Package.PK, innerPackage2.Package.PK, innerPackline.Package.PK }, outerPackages.Select(p => p.PK));

			var outerPackageStates = dcn.OuterPackageStates;
			AssertEquals(3, outerPackageStates.Count());
			AssertContainsExactElementsInAnyOrder(new[] { innerPackage1.PK, innerPackage2.PK, innerPackline.PK }, outerPackageStates.Select(p => p.PK));
		}

		public void TestOuterPackages_HUInContainer()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			var receiveConsignment = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var uld = Helper.CreateReceiveTransportationUnitWithContainerType("ULD1", warehouse.PK, location.PK);
			var dcn = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);

			var hu = helper.CreateHandlingUnitPackage("HU1", helper.CreatePackageHandlingUnit(), uld, TransitWarehouseStatuses.Codes.Arrived, unitType: PackageStateUnitType.Codes.HandlingUnit);
			var innerPackage = Helper.CreatePackageState(receiveConsignment, 1, TransitConstants.TransitPackageUnitType.Package, "Inner1", TransitWarehouseStatuses.Codes.Arrived, uld, dcn);
			var innerPackline = Helper.CreatePackageState(receiveConsignment, 10, TransitConstants.TransitPackageUnitType.Package, "", TransitWarehouseStatuses.Codes.Arrived, uld, dcn);
			helper.PackPackageIntoHandlingUnit(hu, innerPackage, ZDateTimeOffset.Now, "ABC", hu);
			helper.PackPackageIntoHandlingUnit(hu, innerPackline, ZDateTimeOffset.Now, "ABC", hu);

			Factory.Save();

			var outerPackages = dcn.OuterPackages;
			AssertEquals(2, outerPackages.Count());
			AssertContainsExactElementsInAnyOrder(new[] { innerPackage.Package.PK, innerPackline.Package.PK }, outerPackages.Select(p => p.PK));

			var outerPackageStates = dcn.OuterPackageStates;
			AssertEquals(2, outerPackageStates.Count());
			AssertContainsExactElementsInAnyOrder(new[] { innerPackage.PK, innerPackline.PK }, outerPackageStates.Select(p => p.PK));
		}

		#endregion

		#region Test IsSplit

		public void TestIsSplit()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var shipmentJobPK = ZGuid.NewZGuid();

			var dispatchConsignment1 = Helper.CreateDispatchConsignment("DCN1", warehouse.PK, jobID: "EXTREF1", parentPK: shipmentJobPK, parentCode: "JS");
			Helper.CreateDispatchConsignment("DCN2", warehouse.PK, jobID: "EXTREF2", parentPK: shipmentJobPK, parentCode: "JS");

			Factory.Save();

			Assert("should be true", dispatchConsignment1.IsSplit);
		}

		#endregion

		#region Test BusinessObjectsWithRelatedEvents

		public void TestBusinessObjectsWithRelatedEvents_ContainsVisualizerDocumentData()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			warehouse.WarehouseAddress.OA_City = "SYD";
			var dcn1 = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var dcn2 = Helper.CreateDispatchConsignment("DCN2", warehouse.PK);

			var documentData1 = (BusinessObject)Factory.New<IVisualizerDocumentData>();
			documentData1[JobDocumentDataSchema.JDD_ParentTableCode] = dcn1.TablePrefix;
			documentData1[JobDocumentDataSchema.JDD_ParentID] = dcn1.PK;

			var documentData2 = (BusinessObject)Factory.New<IVisualizerDocumentData>();
			documentData2[JobDocumentDataSchema.JDD_ParentTableCode] = dcn2.TablePrefix;
			documentData2[JobDocumentDataSchema.JDD_ParentID] = dcn2.PK;

			AssertContainsExactElementsInAnyOrder("BusinessObjectsWithRelatedEvents contains sent document data",
				new[] { documentData1 },
				dcn1.BusinessObjectsWithRelatedEvents.OfType<IVisualizerDocumentData>());
		}

		#endregion

		#region TestOrderReferences

		public void TestOrderReferences()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var dcn = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var consignmentOrderReference1 = Helper.CreateWhsItemConsignmentOrderReference("REF1", dcn);
			var consignmentOrderReference2 = Helper.CreateWhsItemConsignmentOrderReference("REF2", dcn);
			Factory.Save();

			var consignmentOrderReferenceCollection = dcn.OrderReferences;
			AssertNotNull(consignmentOrderReferenceCollection);
			AssertEquals("Should have 2 order refs", 2, consignmentOrderReferenceCollection.Count);

			AssertEquals("Order Ref Should Be REF2", "REF1", consignmentOrderReferenceCollection.Single(s => s.PK == consignmentOrderReference1.PK).WOR_OrderReference);
			AssertEquals("Order Ref Should Be REF2", "REF2", consignmentOrderReferenceCollection.Single(s => s.PK == consignmentOrderReference2.PK).WOR_OrderReference);
		}

		#endregion

		#region TestDelete

		public void TestDeleteConsignmentDeletesOrderReferences()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var dcn = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var consignmentOrderReference = Helper.CreateWhsItemConsignmentOrderReference("REF1", dcn);
			Factory.Save();

			dcn.Delete();
			AssertEquals(true, consignmentOrderReference.IsDeleted);
			AssertNull(Factory.LoadTop1<WhsItemConsignmentOrderReference>(new ZQuery()));
		}

		#endregion

		WhsTransitTestHelper Helper => helper ?? (helper = new WhsTransitTestHelper(Factory));
		WhsTransitTestHelper helper;
	}

	#region WhsItemDispatchConsignmentWorkflowProviderTest

	[TestedType(typeof(WhsItemDispatchConsignment))]
	public class WhsItemDispatchConsignmentWorkflowProviderTest : WorkflowProviderTest<WhsItemDispatchConsignment, WhsItemDispatchConsignmentProcessTaskCollection>
	{
		protected override ZString ExpectedWorkflowType
		{
			get { return WorkflowDescriptors.TransitDispatchConsignment; }
		}
	}

	#endregion

	#region WhsDispatchConsignmentIDtbBookingParentTestCase

	[TestedType(typeof(WhsItemDispatchConsignment))]
	public class WhsItemDispatchConsignmentIDtbBookingParentTestCase : IDtbBookingParentTestCase<WhsItemDispatchConsignment>
	{
		protected override WhsItemDispatchConsignment GetNewParent()
		{
			var helper = new WhsTransitTestHelper(Factory);
			var warehouse = helper.CreateTRWWarehouse();
			var dispatchConsignment = helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var additionalReferenceInDispatch = dispatchConsignment.AdditionalReferenceNumbers.AddNew();
			return dispatchConsignment;
		}

		protected override bool IsWorkflowDescriptorSupportsCreateTransportBooking() => false;

		protected override bool CanHaveDirectCartageChild => false;
	}

	#endregion
}
