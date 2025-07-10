using System;
using System.Linq;
using System.Reflection;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Packing.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Integration.Forwarding;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	public class WhsItemReceiveConsignmentTest : TestCaseWithFactory
	{
		#region TestReadOnlyFields

		static readonly string[] ReadOnlyProperties =
		{
			"WRC_WW_IntendedWarehouse",
			"WRC_SystemCreateTimeUtc",
			"WRC_SystemCreateUser",
			"WRC_SystemLastEditTimeUtc",
			"WRC_SystemLastEditUser",
			"WRC_ParentID",
			"WRC_ParentTableCode",
			"WRC_CompleteTime",
			"WRC_JobID",
			"WRC_CustomsStatus",
			"WRC_CustomsStatusReason",
		};

		public void TestReadOnly()
		{
			var rcn = Factory.New<WhsItemReceiveConsignment>();
			foreach (var propertyName in ReadOnlyProperties)
			{
				var readOnly = ((ZPropertyInfo)typeof(WhsItemReceiveConsignment).GetProperty(propertyName + "Info").GetValue(rcn)).ReadOnly;
				var readOnlyAction = ActionFieldAttribute.Get(typeof(WhsItemReceiveConsignment).GetProperty(propertyName))?.ReadOnly;
				AssertEquals(propertyName, true, readOnly);
				AssertEquals(propertyName, true, readOnlyAction);
			}
		}

		#endregion

		#region TestAutoLog

		public void TestAutoLog()
		{
			var consignment = Factory.New<WhsItemReceiveConsignment>();
			AssertEquals(true, consignment.IsAutoAdminBusinessObjectLoggerEnabled);
		}

		#endregion

		#region TestSupportedAddressTypes

		public void TestSupportedAddressTypes()
		{
			var consignment = (IDocAddresses)Factory.New<WhsItemReceiveConsignment>();
			var expectedSupportedDocAddressTypes = new[] { DocAddressType.LocalCartageExporter, DocAddressType.ConsignorPickupDeliveryAddress, DocAddressType.ConsigneeDocumentaryAddress, DocAddressType.ClientRequestedBillingParty, DocAddressType.BookingPartyDocumentaryAddress, DocAddressType.ArrivalCTOAddress };
			AssertContainsExactElementsInAnyOrder(expectedSupportedDocAddressTypes, consignment.SupportedAddressTypes);
		}

		#endregion

		#region TestJobDocAddresses

		public void TestJobDocAddresses()
		{
			var consignment = Factory.New<WhsItemReceiveConsignment>();
			AssertEquals(DocAddressType.LocalCartageExporter, consignment.ConsignorDocAddress.DocAddressType);
			AssertEquals(DocAddressType.ConsignorPickupDeliveryAddress, consignment.ConsignorPickupDeliveryAddress.DocAddressType);
			AssertEquals(DocAddressType.ConsigneeDocumentaryAddress, consignment.ConsigneeDocAddress.DocAddressType);
			AssertEquals(DocAddressType.BookingPartyDocumentaryAddress, consignment.BookingPartyDocAddress.DocAddressType);
			AssertEquals(DocAddressType.ClientRequestedBillingParty, consignment.ClientRequestedBillToPartyDocAddress.DocAddressType);
			AssertEquals(DocAddressType.ArrivalCTOAddress, consignment.CTODocAddress.DocAddressType);
		}

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

		#region TestWarehouse

		public void TestWarehouse()
		{
			var warehouse = Factory.New<WhsWarehouse>();
			warehouse.WW_WarehouseName = "Wh1";
			var consignment = Factory.New<WhsItemReceiveConsignment>();
			consignment.WRC_WW_IntendedWarehouse = warehouse.PK;
			AssertEquals(warehouse, consignment.Warehouse);
		}

		#endregion

		#region TestNextDischargePort

		public void TestNextDischargePort()
		{
			var consignment = Factory.New<WhsItemReceiveConsignment>();
			consignment.WRC_RL_NKNextDischargePort = "AUSYD";
			AssertEquals("AUSYD", consignment.WRC_RL_NKNextDischargePort);
		}

		#endregion

		#region TestLinesThatDoNotImplementWorkflow_LineEventsFire

		public void TestLinesThatDoNotImplementWorkflow_LineEventsFire()
		{
			var bookingParty = Factory.NewWithValidTestData<OrgHeader>();
			var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			var receiveTransportationUnit = Factory.NewWithValidTestData<WhsItemReceiveTransportationUnit>();
			receiveTransportationUnit.WRH_WW_Warehouse = warehouse.PK;

			var consignment = Factory.NewWithValidTestData<WhsItemReceiveConsignment>();
			consignment.BookingPartyDocAddress.E2_OA_Address = bookingParty.MainAddress.PK;
			consignment.WRC_WW_IntendedWarehouse = warehouse.PK;
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(consignment);
			var package = packageJob.Packages.AddNew("PLT");
			var packageState = consignment.PackageStates.AddNew();
			packageState.WPS_WRH_TransitReceiveHeader = receiveTransportationUnit.PK;
			packageState.WPS_Status = "ARV";
			packageState.WPS_KP_Package = package.PK;
			packageState.WPS_WL_LastLocation = receiveTransportationUnit.WRH_WL_StagingLocation;
			packageState.WPS_WW_Warehouse = warehouse.PK;

			var trigger = consignment.WorkflowItems.Triggers.AddNew();
			trigger.P9_LineTriggerType = TriggerLineTypes.Codes.PkgPackage;
			trigger.TriggerConditions.TriggerEventCode = Events.ArrivalCode;
			AssertNoErrors(trigger.P9_LineTriggerTypeInfo);

			SetupEmailNotification(trigger.ProcessTaskNotifications.AddNew());
			Factory.Save();

			package.GetLogs().AddNew(Events.Arrival);
			AssertEquals(1, trigger.GetLogs().Find(l => l.SL_SE_NKEvent == Events.WorkflowTriggerEventCode).Count());
		}

		static void SetupEmailNotification(ProcessTaskNotification notification)
		{
			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
			notification.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.Email;
			notification.PQ_EmailAddr = "dilanka@cargowise.com";
		}

		#endregion

		#region TestAdditionalReferences

		public void TestAdditionalReferences()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var receiveConsignment = helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var additionalReferenceInReceive = receiveConsignment.AdditionalReferenceNumbers.AddNew();
			additionalReferenceInReceive.CE_EntryType = WarehouseAdditionalReferenceTypes.Codes.RunSheetInstructionReference;
			additionalReferenceInReceive.CE_EntryNum = "R1-2";

			AssertEquals("R1-2", receiveConsignment.AdditionalReferenceNumbers.GetAllReferenceNumbersByType(WarehouseAdditionalReferenceTypes.Codes.RunSheetInstructionReference).Single());
		}

		public void TestAdditionalReferenceNumbersSingleLine()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var receiveConsignment = helper.CreateReceiveConsignment("RCN1", warehouse.PK);

			var cusNumber1 = receiveConsignment.AdditionalReferenceNumbers.AddNew();
			cusNumber1.CE_EntryType = WarehouseAdditionalReferenceTypes.Codes.MasterBill;
			cusNumber1.CE_RN_NKCountryCode = "";
			cusNumber1.CE_EntryNum = "MB0000001";
			AssertEquals("MAB: MB0000001", receiveConsignment.AdditionalReferenceNumbersSingleLine);

			cusNumber1.CE_RN_NKCountryCode = "AU";
			AssertEquals("MAB: MB0000001/AU", receiveConsignment.AdditionalReferenceNumbersSingleLine);

			var cusNumber2 = receiveConsignment.AdditionalReferenceNumbers.AddNew();
			cusNumber2.CE_EntryType = WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber;
			cusNumber2.CE_EntryNum = "S0000001";
			cusNumber2.CE_RN_NKCountryCode = "CN";
			AssertEquals("MAB: MB0000001/AU, FSH: S0000001/CN", receiveConsignment.AdditionalReferenceNumbersSingleLine);
		}

		public void TestShipmentNumber()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var receiveConsignment = helper.CreateReceiveConsignment("RCN1", warehouse.PK);

			var cusNumber1 = receiveConsignment.AdditionalReferenceNumbers.AddNew();
			cusNumber1.CE_EntryType = WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber;
			cusNumber1.CE_EntryNum = "S0000001";
			AssertEquals("S0000001", receiveConsignment.ShipmentNumber);
		}

		#endregion

		#region TestHumanReadableName

		public void TestHumanReadableName()
		{
			var itemReceiveConsignment = Factory.New<WhsItemReceiveConsignment>();
			AssertEquals("Receive Consignment", itemReceiveConsignment.HumanReadableName);

			itemReceiveConsignment.WRC_JobID = "190";
			AssertEquals("Receive Consignment 190", itemReceiveConsignment.HumanReadableName);
		}

		#endregion

		#region TestIJobNumber

		public void TestIJobNumber()
		{
			var consignment = Factory.New<WhsItemReceiveConsignment>();
			consignment.WRC_JobID = "ThisIsValid";
			AssertEquals("ThisIsValid", ((IJobNumber)consignment).JobNumber);
		}

		#endregion

		#region TestPopulateJobHeaderParent

		public void TestPopulateJobHeaderParent()
		{
			var shipment = (IJobHeaderParent)Factory.New<IForwardingShipment>();
			var jobForShipment = new JobHeader.Loader(shipment).TryLoadOrCreate();

			var receiveConsignment = Factory.New<WhsItemReceiveConsignment>();
			receiveConsignment.WRC_ParentID = shipment.PK;
			receiveConsignment.WRC_ParentTableCode = ((BusinessObject)shipment).TablePrefix;
			new JobHeader.Loader(receiveConsignment).TryLoadOrCreate();
			AssertEquals(jobForShipment.PK, receiveConsignment.JobHeader.JH_JH_ParentJob);
		}

		#endregion

		#region TestCreateJobHeader

		public void TestCreateJobHeaderWithNoParent()
		{
			var shipment = (IJobHeaderParent)Factory.New<IForwardingShipment>();
			new JobHeader.Loader(shipment).TryLoadOrCreate();

			var receiveConsignment = Factory.New<WhsItemReceiveConsignment>();
			new JobHeader.Loader(receiveConsignment).TryLoadOrCreate();
			AssertEquals(ZGuid.Empty, receiveConsignment.JobHeader.JH_JH_ParentJob);
		}

		public void TestCreateJobHeaderWithNoParentJobOnParent()
		{
			var shipment = (IJobHeaderParent)Factory.New<IForwardingShipment>();

			var receiveConsignment = Factory.New<WhsItemReceiveConsignment>();
			receiveConsignment.WRC_ParentID = shipment.PK;
			receiveConsignment.WRC_ParentTableCode = ((BusinessObject)shipment).TablePrefix;
			new JobHeader.Loader(receiveConsignment).TryLoadOrCreate();
			AssertEquals(ZGuid.Empty, receiveConsignment.JobHeader.JH_JH_ParentJob);
		}

		#endregion

		#region TestIPackingParentDocumentOptions

		public void TestIPackingParentDocumentOptions()
		{
			var consignment = Factory.New<WhsItemReceiveConsignment>();
			AssertEquals("Transit Warehouse should only expose the Basic Label.", DocumentOptions.ShowBasicLabelOnly, ((IPackingParent)consignment).DocumentOptions);
		}

		#endregion

		#region TestIPackingParentJobNo

		public void TestIPackingParentJobNo()
		{
			var consignment = Factory.New<WhsItemReceiveConsignment>();
			consignment.WRC_ConsignmentID = "ThisIsValid";
			consignment.WRC_JobID = "RC00000001";
			AssertEquals("ThisIsValid", ((IPackingParent)consignment).JobNo);
		}

		#endregion

		#region TestIPackingParentConnoteNo

		public void TestIPackingParentConnoteNo()
		{
			var consignment = Factory.New<WhsItemReceiveConsignment>();
			AssertEquals(ZString.Empty, ((IPackingParent)consignment).ConnoteNo);
		}

		#endregion

		#region TestIPackingParentWithTransportCompany_GetTransportCompany

		public void TestIPackingParentWithTransportCompany_GetTransportCompany()
		{
			var consignment = Factory.New<WhsItemReceiveConsignment>();
			AssertEquals(null, ((IPackingParentWithTransportCompanyAndBookingParty)consignment).GetTransportCompany("1", MessageRecipientPartyTypeList.Codes.ArrivalCFS));

			var packageState = consignment.PackageStates.AddNew();
			var package = Factory.New<PkgPackage>();
			package.KP_PackageID = "1";
			packageState.WPS_KP_Package = package.PK;
			AssertEquals(null, ((IPackingParentWithTransportCompanyAndBookingParty)consignment).GetTransportCompany("1", MessageRecipientPartyTypeList.Codes.ArrivalCFS));
			AssertEquals(null, ((IPackingParentWithTransportCompanyAndBookingParty)consignment).GetTransportCompany("1", MessageRecipientPartyTypeList.Codes.DepartureCFS));
		}

		public void TestIPackingParentWithTransportCompany_GetTransportCompany_WithTransportationUnit()
		{
			var consignment = Factory.New<WhsItemReceiveConsignment>();
			var receiveTransportationUnit = Factory.New<WhsItemReceiveTransportationUnit>();
			receiveTransportationUnit.TransportCompany.OrganisationPK = Factory.New<OrgHeader>().PK;
			var dispatchTransportationUnit = Factory.New<WhsItemDispatchTransportationUnit>();
			dispatchTransportationUnit.TransportCompany.OrganisationPK = Factory.New<OrgHeader>().PK;

			var packageStateWithoutUnit = consignment.PackageStates.AddNew();
			var packageStateWithReceiveUnit = consignment.PackageStates.AddNew();
			packageStateWithReceiveUnit.WPS_WRH_TransitReceiveHeader = receiveTransportationUnit.PK;
			var packageStateWithDispatchUnit = consignment.PackageStates.AddNew();
			packageStateWithDispatchUnit.WPS_WRH_TransitReceiveHeader = receiveTransportationUnit.PK;
			packageStateWithDispatchUnit.WPS_WDH_TransitDispatchHeader = dispatchTransportationUnit.PK;

			CreatePackage(packageStateWithoutUnit, "1");
			CreatePackage(packageStateWithReceiveUnit, "2");
			CreatePackage(packageStateWithDispatchUnit, "3");

			AssertEquals(null, ((IPackingParentWithTransportCompanyAndBookingParty)consignment).GetTransportCompany("1", MessageRecipientPartyTypeList.Codes.DeliveryCartage));
			AssertEquals(null, ((IPackingParentWithTransportCompanyAndBookingParty)consignment).GetTransportCompany("1", MessageRecipientPartyTypeList.Codes.PickupCartage));

			AssertEquals(receiveTransportationUnit.TransportCompany.Organisation.MainAddress, ((IPackingParentWithTransportCompanyAndBookingParty)consignment).GetTransportCompany("2", MessageRecipientPartyTypeList.Codes.DeliveryCartage));
			AssertEquals(null, ((IPackingParentWithTransportCompanyAndBookingParty)consignment).GetTransportCompany("2", MessageRecipientPartyTypeList.Codes.PickupCartage));
			AssertEquals(receiveTransportationUnit.TransportCompany.Organisation.MainAddress, ((IPackingParentWithTransportCompanyAndBookingParty)consignment).GetTransportCompany("3", MessageRecipientPartyTypeList.Codes.DeliveryCartage));
			AssertEquals(dispatchTransportationUnit.TransportCompany.Organisation.MainAddress, ((IPackingParentWithTransportCompanyAndBookingParty)consignment).GetTransportCompany("3", MessageRecipientPartyTypeList.Codes.PickupCartage));
		}

		void CreatePackage(WhsItemPackageState packageState, string packageId)
		{
			var package = Factory.New<PkgPackage>();
			package.KP_PackageID = packageId;
			packageState.WPS_KP_Package = package.PK;
		}

		#endregion

		#region TestIPackingParentSupportConcreteLoosePackage

		public void TestIPackingParentSupportConcreteLoosePackage()
		{
			var consignment = Factory.New<WhsItemReceiveConsignment>();
			AssertNotNull("Receive Consignment support conversion of a Loose Package to a Concrete Package.", consignment);
		}

		#endregion

		#region TestIPackingParentSupportsImportingBookedDimensions

		public void TestIPackingParentSupportsImportingBookedDimensions()
		{
			var consignment = Factory.New<WhsItemReceiveConsignment>();
			AssertNotNull("Receive Consignment support importing Booked Dimensions.", consignment);
		}

		#endregion

		#region TestIPackingParentMembers

		public void TestIPackingParentMembers()
		{
			var consignment = Factory.New<WhsItemReceiveConsignment>();
			consignment.WRC_ConsignmentID = "RCN1";
			IPackingParent packingParent = consignment;
			AssertEquals(null, packingParent.ControllerID);
			AssertEquals(DocumentOptions.ShowBasicLabelOnly, packingParent.DocumentOptions);
			AssertEquals(false, packingParent.IsAutoPrintAllowed);
			AssertEquals(false, packingParent.IsParentJobFinalised);
			AssertEquals(false, packingParent.IsScanEventsVisible);
			AssertEquals("Receive Consignment", packingParent.JobDescription);
			AssertEquals("RCN1", packingParent.JobNo);
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
			var consignment = Factory.New<WhsItemReceiveConsignment>();
			IPackingParent packingParent = consignment;
			AssertEquals(true, packingParent.ShouldPackTrackedPackagesViaDivot);
		}

		#endregion

		#region TestIPackingParent_OnPackageDelete

		public void TestIPackingParent_OnPackageDelete()
		{
			var consignment = Factory.New<WhsItemReceiveConsignment>();
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

		#region TestIPackingParentWithOutturn Members

		public void TestIPackingParentWithOutturn_OutturnProvider_NoReceiveTransportationUnit()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var receiveConsignment = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var packingParentWithOutturn = (IPackingParentWithOutturn)receiveConsignment;
			var package = Helper.CreatePackageState(receiveConsignment, 1, "PLT", "P1", "BKD").Package;

			var outturnProvider = packingParentWithOutturn.GetOutturnProvider(package);

			AssertEquals("Transport reference is empty if there is no RTU.", string.Empty, outturnProvider.ActualTransportJobID);
		}

		public void TestIPackingParentWithOutturn_OutturnProvider_WithVehicle()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var receiveConsignment = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var asn = Helper.CreateReceiveASN("ASN1", warehouse.PK);
			var packingParentWithOutturn = (IPackingParentWithOutturn)receiveConsignment;
			var location = Helper.CreateRowAndGenerateLocations(warehouse, "DOCK");
			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			receiveTransportationUnit.WRH_VehicleReference = "VEH1";
			var package = Helper.CreatePackageState(receiveConsignment, 1, "PLT", "P1", "ARV", receiveUnit: receiveTransportationUnit, receiveASN: asn).Package;

			var outturnProvider = packingParentWithOutturn.GetOutturnProvider(package);

			AssertEquals("Actual Transport reference is container's RTU ID.", "RTU1", outturnProvider.ActualTransportJobID);
			AssertEquals("Actual Transport Job Type Code.", TWAdditionalReferenceTypesForUniversalXML.Codes.TransitWarehouseReceiveTransportationUnit, outturnProvider.ActualTransportJobTypeCode);
			AssertEquals("Actual Transport Job Type Description.", TWAdditionalReferenceTypesForUniversalXML.Descriptions.TransitWarehouseReceiveTransportationUnit, outturnProvider.ActualTransportJobTypeDescription);

			AssertEquals("Expected Transport reference is container's ASN ID.", "ASN1", outturnProvider.ExpectedTransportJobID);
			AssertEquals("Expected Transport Job Type Code.", TWAdditionalReferenceTypesForUniversalXML.Codes.TransitWarehouseReceiveASN, outturnProvider.ExpectedTransportJobTypeCode);
			AssertEquals("Expected Transport Job Type Description.", TWAdditionalReferenceTypesForUniversalXML.Descriptions.TransitWarehouseReceiveASN, outturnProvider.ExpectedTransportJobTypeDescription);

			var containerTuple = packingParentWithOutturn.GetParentContainer(package);
			AssertNull("Container is empty if the RTU is not a container.", containerTuple.Container);
			AssertNull("Container Number is empty if the RTU is not a container.", containerTuple.ContainerNumber);
		}

		public void TestIPackingParentWithOutturn_OutturnProvider_WithContainer()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var receiveConsignment = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var packingParentWithOutturn = (IPackingParentWithOutturn)receiveConsignment;
			var location = Helper.CreateRowAndGenerateLocations(warehouse, "DOCK");
			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnitWithContainerType("RTU1", warehouse.PK, location.PK);
			receiveTransportationUnit.WRH_VehicleReference = "VEH1";
			var package = Helper.CreatePackageState(receiveConsignment, 1, "PLT", "P1", "ARV", receiveUnit: receiveTransportationUnit).Package;
			var container = receiveTransportationUnit.Container;

			var outturnProvider = packingParentWithOutturn.GetOutturnProvider(package);

			AssertEquals("Transport reference is container's RTU ID.", "RTU1", outturnProvider.ActualTransportJobID);

			var containerTuple = packingParentWithOutturn.GetParentContainer(package);
			AssertEquals(container, containerTuple.Container);
			AssertEquals("VEH1", containerTuple.ContainerNumber);
		}

		public void TestIPackingParentWithOutturn_OutturnProvider_NoPackageState_DoesNotThrow()
		{
			var consignment = Factory.New<WhsItemReceiveConsignment>();
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
			var receiveConsignment = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var packingParentWithOutturn = (IPackingParentWithOutturn)receiveConsignment;
			var location = Helper.CreateRowAndGenerateLocations(warehouse, "DOCK");
			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnitWithContainerType("RTU1", warehouse.PK, location.PK);
			var currentTime = ZDateTimeOffset.Now;
			receiveTransportationUnit.WRH_UnloadCompleteTime = currentTime;
			receiveTransportationUnit.WRH_UnloadCompleteNotYetProcessedTime = receiveTransportationUnit.WRH_UnloadCompleteTime;
			var container = receiveTransportationUnit.Container;

			var containerView = packingParentWithOutturn.GetContainerView(container);

			AssertNotNull(containerView);
			AssertNull(containerView.PackCompleteDate);
			AssertEquals(currentTime.ToZDateTime(), containerView.UnpackCompleteDate.Value);
		}

		public void TestIPackingParentWithOutturn_ContainerView_NoDispatchTransportationUnit()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var receiveConsignment = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var packingParentWithOutturn = (IPackingParentWithOutturn)receiveConsignment;
			var package = receiveConsignment.PackageJob.Containers.AddNew();

			var containerView = packingParentWithOutturn.GetContainerView(package.Container);

			AssertNotNull(containerView);
			AssertNull(containerView.PackCompleteDate);
			AssertNull(containerView.UnpackCompleteDate);
		}

		public void TestIPackingParentWithOutturn_ContainerView_InvalidDate()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var receiveConsignment = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var packingParentWithOutturn = (IPackingParentWithOutturn)receiveConsignment;
			var location = Helper.CreateRowAndGenerateLocations(warehouse, "DOCK");
			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnitWithContainerType("RTU1", warehouse.PK, location.PK);
			receiveTransportationUnit.WRH_UnloadCompleteTime = ZDateTimeOffset.Invalid;
			receiveTransportationUnit.WRH_UnloadCompleteNotYetProcessedTime = receiveTransportationUnit.WRH_UnloadCompleteTime;
			var container = receiveTransportationUnit.Container;

			var containerView = packingParentWithOutturn.GetContainerView(container);

			AssertNotNull(containerView);
			AssertNull(containerView.PackCompleteDate);
			AssertNull(containerView.UnpackCompleteDate);
		}

		public void TestIPackingParentWithOutturn_ContainerView_NoDates()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var receiveConsignment = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var packingParentWithOutturn = (IPackingParentWithOutturn)receiveConsignment;
			var location = Helper.CreateRowAndGenerateLocations(warehouse, "DOCK");
			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnitWithContainerType("RTU1", warehouse.PK, location.PK);
			var container = receiveTransportationUnit.Container;

			var containerView = packingParentWithOutturn.GetContainerView(container);

			AssertNotNull(containerView);
			AssertNull(containerView.PackCompleteDate);
			AssertNull(containerView.UnpackCompleteDate);
		}

		#endregion

		#region TestIDocManagerSupport

		public void TestIDocManagerSupport()
		{
			var header = Factory.New<WhsItemReceiveConsignment>();
			AssertEquals(Constants.DocManagerCodes.TransitReceiveConsignment, ((IDocManagerSupport)header).DocManagerInfo.DocManagerCode);
		}

		#endregion

		#region TestIWorkflowProvider Members

		public void TestIWorkflowProviderMembers()
		{
			var receive = Factory.New<WhsItemReceiveConsignment>();
			var newWorkflowItem = receive.WorkflowItems.AddNew();
			var workflowProvider = (IWorkflowProvider)receive;

			AssertEquals(WorkflowDescriptors.TransitReceiveConsignment, workflowProvider.WorkflowType);
			AssertEquals(newWorkflowItem, workflowProvider.WorkflowItems.Single());
			AssertEquals(typeof(ColumnValueRanker), workflowProvider.GetTemplateSelectionCriteria().GetType());
			AssertNull(workflowProvider.GetWorkflowInformationProvider());
		}

		public void TestGetTemplateSelectionCriteria()
		{
			var bookingParty1 = Factory.NewWithValidTestData<OrgHeader>();
			var intendedWarehouse = Factory.NewWithValidTestData<WhsWarehouse>();

			var consignmentWithoutHeader = Factory.NewWithValidTestData<WhsItemReceiveConsignment>();
			consignmentWithoutHeader.BookingPartyDocAddress.E2_OA_Address = bookingParty1.MainAddress.PK;
			consignmentWithoutHeader.WRC_WW_IntendedWarehouse = intendedWarehouse.PK;
			Factory.Save();

			var criteriaForConsignmentWithoutHeader = (ColumnValueRanker)((IWorkflowProvider)consignmentWithoutHeader).GetTemplateSelectionCriteria();
			AssertContainsExactElementsInAnyOrder(new[] { bookingParty1.PK, ZGuid.Empty }, criteriaForConsignmentWithoutHeader.GetValues(ProcessTaskTemplateSchema.P0_OH_Client));
			AssertContainsExactElementsInAnyOrder(new[] { intendedWarehouse.PK, ZGuid.Empty }, criteriaForConsignmentWithoutHeader.GetValues(ProcessTaskTemplateSchema.P0_WW));
		}

		#endregion

		#region ITransportParentCommon Members

		public void TestTypeCode()
		{
			var receiveConsignment = Factory.New<WhsItemReceiveConsignment>();
			AssertEquals(Constants.TransportParentTypes.TransitReceiveConsignment, receiveConsignment.TypeCode);
		}

		#endregion

		#region TestIConsignment Members

		public void TestIConsignment()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var receiveConsignment = helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			receiveConsignment.WRC_WW_IntendedWarehouse = warehouse.PK;
			receiveConsignment.WRC_HouseBillNumber = "SSYD000000";
			receiveConsignment.WRC_JobID = "RC000000001";
			receiveConsignment.WRC_TransportMode = "SEA";
			receiveConsignment.WRC_Direction = "IMP";

			var consignment = (IConsignment)receiveConsignment;
			AssertEquals(consignment.Warehouse, warehouse);
			AssertEquals(consignment.BookingPartyDocAddress, receiveConsignment.BookingPartyDocAddress);
			AssertEquals(consignment.HouseBillNumber, receiveConsignment.WRC_HouseBillNumber);
			AssertEquals(consignment.JobID, receiveConsignment.WRC_JobID);
			AssertEquals(consignment.TransportMode, receiveConsignment.WRC_TransportMode);
			AssertEquals(consignment.Direction, receiveConsignment.WRC_Direction);
		}

		#region TestIConsignmentForMasterBillNumber

		public void TestIConsignmentForMasterBillNumber_HasAdditionalReference()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var receiveConsignment = helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var additionalReferenceInReceive = receiveConsignment.AdditionalReferenceNumbers.AddNew();
			additionalReferenceInReceive.CE_EntryType = WarehouseAdditionalReferenceTypes.Codes.MasterBill;
			additionalReferenceInReceive.CE_EntryNum = "MASTERBILL";

			var consignment = (IConsignment)receiveConsignment;
			AssertEquals(consignment.MasterBillNumber, additionalReferenceInReceive.CE_EntryNum);
		}

		public void TestIConsignmentForMasterBillNumber_HasEmptyAdditionalReference_HasASNMasterBillNumber()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var receiveConsignment = helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var additionalReferenceInReceive = receiveConsignment.AdditionalReferenceNumbers.AddNew();
			additionalReferenceInReceive.CE_EntryType = WarehouseAdditionalReferenceTypes.Codes.MasterBill;
			additionalReferenceInReceive.CE_EntryNum = ZString.Empty;

			var asn = Helper.CreateReceiveASN("ASN1", warehouse.PK);
			var additionalReferenceInASN = asn.AdditionalReferenceNumbers.AddNew();
			additionalReferenceInASN.CE_EntryType = WarehouseAdditionalReferenceTypes.Codes.MasterBill;
			additionalReferenceInASN.CE_EntryNum = "MASTERBILL";

			Helper.CreatePackageState(receiveConsignment, 1, "PLT", "P1", TransitWarehouseStatuses.Codes.Arrived, receiveASN: asn);

			var consignment = (IConsignment)receiveConsignment;
			AssertEquals(consignment.MasterBillNumber, additionalReferenceInASN.CE_EntryNum);
		}

		public void TestIConsignmentForMasterBillNumber_NoAdditionalReference_HasASNMasterBillNumber()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var receiveConsignment = helper.CreateReceiveConsignment("RCN1", warehouse.PK);

			var asn = Helper.CreateReceiveASN("ASN1", warehouse.PK);
			var additionalReferenceInASN = asn.AdditionalReferenceNumbers.AddNew();
			additionalReferenceInASN.CE_EntryType = WarehouseAdditionalReferenceTypes.Codes.MasterBill;
			additionalReferenceInASN.CE_EntryNum = "MASTERBILL";

			Helper.CreatePackageState(receiveConsignment, 1, "PLT", "P1", TransitWarehouseStatuses.Codes.Arrived, receiveASN: asn);

			var consignment = (IConsignment)receiveConsignment;
			AssertEquals(consignment.MasterBillNumber, additionalReferenceInASN.CE_EntryNum);
		}

		public void TestIConsignmentForMasterBillNumber_NoAdditionalReference_HasEmptyASNMasterBillNumber()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var receiveConsignment = helper.CreateReceiveConsignment("RCN1", warehouse.PK);

			var asn = Helper.CreateReceiveASN("ASN1", warehouse.PK);
			var additionalReferenceInASN = asn.AdditionalReferenceNumbers.AddNew();
			additionalReferenceInASN.CE_EntryType = WarehouseAdditionalReferenceTypes.Codes.MasterBill;
			additionalReferenceInASN.CE_EntryNum = ZString.Empty;

			var consignment = (IConsignment)receiveConsignment;
			AssertEquals(consignment.MasterBillNumber, ZString.Empty);
		}

		public void TestIConsignmentForMasterBillNumber_NoAdditionalReference_NoASNMasterBillNumber()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var receiveConsignment = helper.CreateReceiveConsignment("RCN1", warehouse.PK);

			var consignment = (IConsignment)receiveConsignment;
			AssertEquals(consignment.MasterBillNumber, ZString.Empty);
		}

		#endregion

		#endregion

		#region TestITransitConsignmentForRating Members

		public void TestITransitConsignmentForRating()
		{
			var now = ZDateTime.Now;
			var consignor = Helper.CreateClient("CNR1");
			var consignee = Helper.CreateClient("CNE1");
			var bookingParty = Helper.CreateClient("BKP1");
			var consignment = Factory.New<WhsItemReceiveConsignment>();
			consignment.WRC_RS_NKServiceLevel = "STD";
			consignment.WRC_ExpectedArrivalTime = now.AddDays(-2);
			consignment.WRC_ExpectedDispatchTime = now.AddDays(2);
			Helper.CreateJobDocAddressFromAddress(consignment, DocAddressTypes.Codes.LocalCartageExporter, consignor.MainAddress);
			Helper.CreateJobDocAddressFromAddress(consignment, DocAddressTypes.Codes.ConsigneeDocumentaryAddress, consignee.MainAddress);
			Helper.CreateJobDocAddressFromAddress(consignment, DocAddressTypes.Codes.BookingPartyDocumentaryAddress, bookingParty.MainAddress);

			var transitRating = (ITransitConsignmentForRating)consignment;
			AssertEquals("STD", transitRating.ServiceLevel);
			AssertEquals(ChargeCodeGroupList.Codes.TRWReceive, transitRating.ChargeCodeGroup);
			AssertEquals(consignment.WRC_ExpectedArrivalTime, transitRating.ExpectedArrivalDate);
			AssertEquals(consignment.WRC_ExpectedDispatchTime, transitRating.ExpectedDepartureDate);
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

		#region TestTransitPackagesForRating

		[TestDate(2022, 6, 10)]
		public void TestTransitPackagesForRating()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			Factory.Save();

			var undgSubstance = Helper.CreateUNDGSubstance("BCDE", "1.5D", "BCDEa");
			undgSubstance.DG_ExceptedQuantityCode = "E1";

			var now = ZDateTimeOffset.Now;
			var unloadedTime = now.AddDays(-5);
			var rcn = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			var dcn = Helper.CreateDispatchConsignment("DC1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			rtu.WRH_GateInTime = unloadedTime.AddHours(-1);
			rtu.WRH_UnloadCompleteTime = unloadedTime;
			rtu.WRH_UnloadCompleteNotYetProcessedTime = rtu.WRH_UnloadCompleteTime;
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			var arrivedPackageState = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, dispatchConsignment: dcn);
			arrivedPackageState.WPS_UnloadedTime = unloadedTime;
			arrivedPackageState.WPS_UnloadedNotYetProcessedTime = arrivedPackageState.WPS_UnloadedTime;
			arrivedPackageState.Package.KP_RH_NKCommodityCode = "HAZ";
			arrivedPackageState.Package.KP_Weight = 10m;
			arrivedPackageState.Package.KP_WeightUQ = Constants.Weight.Kilograms;
			arrivedPackageState.Package.KP_Volume = 5m;
			arrivedPackageState.Package.KP_VolumeUQ = Constants.Volume.CubicMetres;
			arrivedPackageState.Package.KP_RH_NKCommodityCode = "HAZ";
			var undgDataItem = Helper.CreateUNDGDataItem(arrivedPackageState.Package.PK, arrivedPackageState.Package.TablePrefix, undgSubstance, 2, 3);
			arrivedPackageState.Package.UNDGs.Add(undgDataItem);

			var loadedPackageState = Helper.CreatePackageState(rcn, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu);
			loadedPackageState.WPS_UnloadedTime = unloadedTime;
			loadedPackageState.WPS_UnloadedNotYetProcessedTime = loadedPackageState.WPS_UnloadedTime;
			loadedPackageState.WPS_LoadedTime = now;
			var bookedPackageState = Helper.CreatePackageState(rcn, 10, "PKG", "", TransitWarehouseStatuses.Codes.Booked, dispatchConsignment: dcn);
			Factory.Save();

			var transitPackagesForRating = ((ITransitJobForRating)rcn).TransitPackagesForRating;
			AssertEquals(2, transitPackagesForRating.Count());

			var transitPackageForRating1 = transitPackagesForRating.FirstOrDefault(p => p.PK == arrivedPackageState.PK);
			AssertTransitPackageForRating(transitPackageForRating1, arrivedPackageState, true, unloadedTime);

			var transitPackageForRating2 = transitPackagesForRating.FirstOrDefault(p => p.PK == loadedPackageState.PK);
			AssertTransitPackageForRating(transitPackageForRating2, loadedPackageState, false, unloadedTime);
		}

		[TestDate(2022, 6, 10)]
		public void TestTransitPackagesForRating_PackageIsAdjustOut()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			Factory.Save();

			var now = ZDateTimeOffset.Now;
			var completeTime = ZDateTimeOffset.Now.AddDays(-5);
			var rcn = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			rtu.WRH_GateInTime = completeTime.AddHours(-1);
			rtu.WRH_UnloadCompleteTime = completeTime;
			rtu.WRH_UnloadCompleteNotYetProcessedTime = rtu.WRH_UnloadCompleteTime;
			var arrivedPackageState = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			arrivedPackageState.WPS_UnloadedTime = completeTime;
			arrivedPackageState.WPS_UnloadedNotYetProcessedTime = arrivedPackageState.WPS_UnloadedTime;

			var adjustOutPackageState = Helper.CreatePackageState(rcn, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.AdjustedOut, receiveUnit: rtu);
			adjustOutPackageState.WPS_UnloadedTime = completeTime;
			adjustOutPackageState.WPS_UnloadedNotYetProcessedTime = adjustOutPackageState.WPS_UnloadedTime;
			adjustOutPackageState.WPS_AdjustedOut = "LCC";
			Factory.Save();

			var transitPackagesForRating = ((ITransitJobForRating)rcn).TransitPackagesForRating;
			AssertEquals(1, transitPackagesForRating.Count());

			var transitPackageForRating = transitPackagesForRating.FirstOrDefault();
			AssertTransitPackageForRating(transitPackageForRating, arrivedPackageState, false, completeTime);
		}

		[TestDate(2022, 6, 10)]
		public void TestTransitPackagesForRating_HandlingUnit()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			Factory.Save();

			var now = ZDateTimeOffset.Now;
			var completeTime = ZDateTimeOffset.Now.AddDays(-5);
			var rcn = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			rtu.WRH_GateInTime = completeTime.AddHours(-1);
			rtu.WRH_UnloadCompleteTime = completeTime;
			rtu.WRH_UnloadCompleteNotYetProcessedTime = rtu.WRH_UnloadCompleteTime;
			var arrivedPackageState = Helper.CreatePackageState(rcn, 1, "PKG", "PKGA", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			arrivedPackageState.WPS_UnloadedTime = completeTime;
			arrivedPackageState.WPS_UnloadedNotYetProcessedTime = arrivedPackageState.WPS_UnloadedTime;

			var handlingUnit = Helper.CreatePackageHandlingUnit();
			var handlingUnitPackage = Helper.CreateHandlingUnitPackage("HU-1", handlingUnit, rtu);
			var childPackage1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			childPackage1.WPS_UnloadedTime = completeTime;
			childPackage1.WPS_UnloadedNotYetProcessedTime = childPackage1.WPS_UnloadedTime;
			var childPackage2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			childPackage2.WPS_UnloadedTime = completeTime;
			childPackage2.WPS_UnloadedNotYetProcessedTime = childPackage2.WPS_UnloadedTime;
			Helper.DisableTopLevelHUFKForTest(TestConnection);
			Helper.PackPackageIntoHandlingUnit(handlingUnitPackage, childPackage1, ZDateTimeOffset.Now, "AAA", topHandlingUnit: handlingUnitPackage);
			Helper.PackPackageIntoHandlingUnit(handlingUnitPackage, childPackage2, ZDateTimeOffset.Now, "AAA", topHandlingUnit: handlingUnitPackage);
			Factory.Save();

			var transitPackagesForRating = ((ITransitJobForRating)rcn).TransitPackagesForRating;
			AssertEquals(3, transitPackagesForRating.Count());

			var transitPackageForRating1 = transitPackagesForRating.FirstOrDefault(p => p.PK == arrivedPackageState.PK);
			AssertTransitPackageForRating(transitPackageForRating1, arrivedPackageState, false, completeTime);

			var transitPackageForRating2 = transitPackagesForRating.FirstOrDefault(p => p.PK == childPackage1.PK);
			AssertTransitPackageForRating(transitPackageForRating2, childPackage1, false, completeTime);

			var transitPackageForRating3 = transitPackagesForRating.FirstOrDefault(p => p.PK == childPackage2.PK);
			AssertTransitPackageForRating(transitPackageForRating3, childPackage2, false, completeTime);
		}

		[TestDate(2022, 6, 10)]
		public void TestTransitPackagesForRating_OverpackageHandlingUnit()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			Factory.Save();

			var now = ZDateTimeOffset.Now;
			var completeTime = ZDateTimeOffset.Now.AddDays(-5);
			var rcn = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			rtu.WRH_GateInTime = completeTime.AddHours(-1);
			rtu.WRH_UnloadCompleteTime = completeTime;
			rtu.WRH_UnloadCompleteNotYetProcessedTime = rtu.WRH_UnloadCompleteTime;
			var arrivedPackageState = Helper.CreatePackageState(rcn, 1, "PKG", "PKGA", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			arrivedPackageState.WPS_UnloadedTime = completeTime;
			arrivedPackageState.WPS_UnloadedNotYetProcessedTime = arrivedPackageState.WPS_UnloadedTime;

			var overpackHandlingUnit = Helper.CreatePackageHandlingUnit();
			var handlingUnitPackage = Helper.CreateHandlingUnitPackage("HU-1", overpackHandlingUnit, rtu, TransitWarehouseStatuses.Codes.Arrived, unitType: PackageStateUnitType.Codes.Overpack, rcn: rcn);
			handlingUnitPackage.WPS_UnloadedTime = completeTime;
			handlingUnitPackage.WPS_UnloadedNotYetProcessedTime = handlingUnitPackage.WPS_UnloadedTime;
			var childPackage1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var childPackage2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			Helper.DisableTopLevelHUFKForTest(TestConnection);
			Helper.PackPackageIntoHandlingUnit(handlingUnitPackage, childPackage1, ZDateTimeOffset.Now, "AAA", topHandlingUnit: handlingUnitPackage);
			Helper.PackPackageIntoHandlingUnit(handlingUnitPackage, childPackage2, ZDateTimeOffset.Now, "AAA", topHandlingUnit: handlingUnitPackage);
			Factory.Save();

			var transitPackagesForRating = ((ITransitJobForRating)rcn).TransitPackagesForRating;
			AssertEquals(2, transitPackagesForRating.Count());

			var transitPackageForRating1 = transitPackagesForRating.FirstOrDefault(p => p.PK == arrivedPackageState.PK);
			AssertTransitPackageForRating(transitPackageForRating1, arrivedPackageState, false, completeTime);

			var transitPackageForRating2 = transitPackagesForRating.FirstOrDefault(p => p.PK == handlingUnitPackage.PK);
			AssertTransitPackageForRating(transitPackageForRating2, handlingUnitPackage, false, completeTime);
		}

		void AssertTransitPackageForRating(TransitPackageForRatingInfo transitPackageForRating, WhsItemPackageState packageState, bool hasDG,
			ZDateTimeOffset expectedUnloadCompleteTime)
		{
			CombineAssertions(() =>
			{
				AssertNotNull(transitPackageForRating);
				AssertEquals("Package State PK", packageState.PK, transitPackageForRating.PK);
				AssertEquals("Package Qty", packageState.Package.KP_PackageQty, transitPackageForRating.PackageQty);
				AssertEquals("Package Type", packageState.Package.KP_F3_NKPackType, transitPackageForRating.PackageType);
				AssertEquals("Commodity Code", packageState.Package.KP_RH_NKCommodityCode, transitPackageForRating.CommodityCode);
				AssertEquals("Weight", packageState.Package.KP_Weight, transitPackageForRating.Weight);
				AssertEquals("Weight UQ", packageState.Package.KP_WeightUQ, transitPackageForRating.WeightUQ);
				AssertEquals("Volume", packageState.Package.KP_Volume, transitPackageForRating.Volume);
				AssertEquals("Volume UQ", packageState.Package.KP_VolumeUQ, transitPackageForRating.VolumeUQ);
				AssertEquals("Unloaded Time", ZDateTimeOffset.Empty, transitPackageForRating.UnloadedTime);
				AssertEquals("Unload Complete Time", expectedUnloadCompleteTime, transitPackageForRating.UnloadCompleteTime);
				AssertEquals("Loaded Time", packageState.WPS_LoadedTime, transitPackageForRating.LoadedTime);
				AssertEquals("Has Dangerous Goods", hasDG, transitPackageForRating.HasDangerousGoods);
			});
		}

		#endregion

		#endregion

		#region ITransitJobForConsolCosting Members

		public void TestITransitJobForConsolCosting()
		{
			var consignment = Factory.New<WhsItemReceiveConsignment>();
			consignment.WRC_ConsignmentID = "RCN001";
			var consolCosting = (ITransitJobForConsolCosting)consignment;

			AssertEquals("RCN001", consolCosting.HouseBillNumber);

			consignment.WRC_HouseBillNumber = "HB0000001";

			AssertEquals("HB0000001", consolCosting.HouseBillNumber);

			consignment.WRC_RL_NKDestination = "CNNJG";

			AssertEquals("CNNJG", consolCosting.Destination.Code);
		}

		public void TestPackageStatesForConsolCosting()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;
			var rcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);

			var package1 = Helper.CreatePackageState(rcn, 1, "PLT", "P1", TransitWarehouseStatuses.Codes.Arrived, rtu);
			var package2 = Helper.CreatePackageState(rtu, 1, "PLT", "P3", TransitWarehouseStatuses.Codes.Arrived);
			var package3 = Helper.CreatePackageState(rcn, 1, "PLT", "P4", TransitWarehouseStatuses.Codes.Booked);

			var consolCosting = (ITransitJobForConsolCosting)rcn;
			AssertContainsExactElementsInAnyOrder(new WhsItemPackageState[] { package1 }, consolCosting.PackageStatesForConsolCosting);
		}

		#endregion

		#region IRoutingSupport Members

		public void TestRoutingSupport()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var consignment = helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			consignment.WRC_TransportMode = "Air";

			var routingSupport = (IRoutingSupport)consignment;
			AssertNotNull(routingSupport.TransportsIncludingRelated);
			AssertNotNull(routingSupport.Transports);
			AssertNullOrEmpty(routingSupport.AdditionalETAUpdateMsg);
			AssertNullOrEmpty(routingSupport.AdditionalETDUpdateMsg);
			AssertEquals("Air", routingSupport.TransportMode);
		}

		#endregion

		#region ITransportParent Members

		public void TestTransportParent()
		{
			var consignment = Factory.New<WhsItemReceiveConsignment>();

			var routingSupport = (ITransportParent)consignment;
			AssertNotNull(routingSupport.TransportSupporter);
			AssertNotNull(routingSupport.Transports);
			AssertEquals(Directions.Unknown, routingSupport.JobDirection);
		}

		#endregion

		#region IHaveServices Members

		public void TestNeedsServiceEvents()
		{
			var consignment = (IHaveServices)Factory.New<WhsItemReceiveConsignment>();
			AssertEquals(true, consignment.NeedsServiceEvents);
		}

		public void TestJobServiceDeleted()
		{
			var warehouse = Helper.CreateWarehouse("WHS", true);
			var rcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);

			var rtu = Helper.CreateReceiveTransportationUnit("RTU12", warehouse.PK, warehouse.DefaultInboundDockDoorLocation.PK);
			var jobService = Helper.CreateJobService(rcn.PK, "FUM");
			rcn.Services.Add(jobService);

			var unloadTask1 = Helper.CreateUnloadTask(rtu.PK, warehouse.PK, rcn.PK);
			unloadTask1.WUT_ES_ActiveJobService = jobService.PK;

			var unloadTask2 = Helper.CreateUnloadTask(rtu.PK, warehouse.PK, rcn.PK);
			unloadTask2.WUT_ES_ActiveJobService = jobService.PK;

			Factory.Save();

			AssertEquals("Active Job Service should be present on Unload Task", jobService.PK, unloadTask1.WUT_ES_ActiveJobService);
			AssertEquals("Active Job Service should be present on Unload Task", jobService.PK, unloadTask2.WUT_ES_ActiveJobService);

			var jobServicesParent = (IHaveServices)rcn;
			jobServicesParent.JobServiceDeleted(jobService.PK);

			AssertEquals("Active Job Service should be cleared on Unload Task", Guid.Empty, unloadTask1.WUT_ES_ActiveJobService);
			AssertEquals("Active Job Service should be cleared on Unload Task", Guid.Empty, unloadTask2.WUT_ES_ActiveJobService);
		}

		public void TestServiceBranch()
		{
			var consignment = Factory.New<WhsItemReceiveConsignment>();
			var iHaveServices = (IHaveServices)consignment;
			AssertNull("No service branch", iHaveServices.ServiceBranch);

			var warehouse = Helper.CreateWarehouse("WHS", true);
			AssertEquals(true, warehouse.WW_GB_RelatedCompanyBranch.IsValid);

			consignment.WRC_WW_IntendedWarehouse = warehouse.PK;
			AssertEquals("Service branch is warehouse branch", warehouse.WW_GB_RelatedCompanyBranch, iHaveServices.ServiceBranch.PK);
		}

		#endregion

		#region IHaveEventsFromServices Members

		public void TestReferenceParameters()
		{
			var warehouseWithAddress = Helper.CreateTransitWarehouseInCurrentBranch("WH1");
			warehouseWithAddress.WarehouseAddress.City = "SYD";
			var consignment = Factory.New<WhsItemReceiveConsignment>();
			consignment.WRC_WW_IntendedWarehouse = warehouseWithAddress.PK;

			var referenceParameters = ((IHaveEventsFromServices)consignment).ReferenceParameters.ToList();
			AssertEquals(2, referenceParameters.Count);
			AssertEquals(CargoWise.EventReference.Constants.Facilities.Code.Depot, referenceParameters.Find(r => r.Key == CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Facility).Value);
			AssertEquals("SYD", referenceParameters.Find(r => r.Key == CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Location).Value);
		}

		#endregion

		#region GetAdditionalReferenceNumberTypeList

		public void TestGetAdditionalReferenceNumberTypeList()
		{
			var receiveConsignment = Factory.New<WhsItemReceiveConsignment>();
			var warehouseRegistryReferenceNumbers = WarehouseDataRegistry.Instance.AdditionalReferenceType.GetFallBackValueAtAllLevels(Guid.Empty, Guid.Empty, Guid.Empty).GetCodeDescriptionPairList();

			var additionalReferenceNumberTypeList = ((IAdditionalReferenceNumberTypeProvider)receiveConsignment).GetAdditionalReferenceNumberTypeList
			(
				CusEntryNumber.Categories.AdditionalReferenceNumber,
				""
			);

			AssertContainsExactElementsInAnyOrder(warehouseRegistryReferenceNumbers.GetAllCodes(), additionalReferenceNumberTypeList.GetAllCodes());
		}

		public void TestGetAdditionalReferenceNumberTypeList_InvalidCategory()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var receiveConsignment = helper.CreateDispatchConsignment("RCN1", warehouse.PK);

			var additionalReferenceNumberTypeList = ((IAdditionalReferenceNumberTypeProvider)receiveConsignment).GetAdditionalReferenceNumberTypeList
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
			var receiveConsignment = Factory.New<WhsItemReceiveConsignment>();
			AssertContainsExactElementsInAnyOrder(new PredefinedNoteType[] {
				PredefinedNoteTypes.Instance.AutoRatingAuditLog,
				PredefinedNoteTypes.Instance.UnrecognisedAdditionalReferenceTypes,
				PredefinedNoteTypes.Instance.UnmatchedOrgDetails,
				PredefinedNoteTypes.Instance.DeliveryOrderReceiptNotes,
				PredefinedNoteTypes.Instance.CIN750MessageNotes,
				PredefinedNoteTypes.Instance.CRESAMessageNotes,
				PredefinedNoteTypes.Instance.UnloadLoadNotes },
				receiveConsignment.NoteTypes);
		}

		#endregion

		#region TestReceiveConsignmentHasVisualizableDocumentsSupportableAttribute

		public void TestReceiveConsignmentHasVisualizableDocumentsSupportableAttribute()
		{
			var consignment = Factory.New<WhsItemReceiveConsignment>();
			var attribute = consignment.GetType().GetCustomAttribute<VisualizableDocumentsSupportableAttribute>();
			AssertNotNull("Has VisualizableDocumentsSupportable attribute", attribute);
			AssertEquals("ReceiveConsignmentVisualizableDocumentSupporter", attribute.SupporterType.Name);
		}

		#endregion

		#region TestWRC_CompleteTime

		[TestDate(2021, 1, 1)]
		[TestUtcOffset(10, 0, 0)]
		public void TestWRC_CompleteTime()
		{
			var warehouse = Helper.CreateTRWWarehouse("ABC");
			warehouse.WarehouseAddress.OA_City = "SYD";
			var rcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);

			AssertEquals("Precondition: RCN is not closed.", true, rcn.WRC_CompleteTime.IsEmpty);

			rcn.WRC_CompleteTime = ZDateTimeOffset.Now;

			AssertEquals("RCN is closed.", false, rcn.WRC_CompleteTime.IsEmpty);

			var logQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.ItemDocumentJobFinalised.Code);
			var rcnLog = rcn.Logs.Find(logQuery).SingleOrDefault();
			AssertNotNull("Should create closed log for rcn", rcnLog);
			AssertEquals("RCN1|FAC=CFS|LOC=SYD|TYP=RCN Completed|WHS=ABC", rcnLog.SL_Reference);
			AssertEquals(ZDateTimeOffset.Now.ToDateTime(), rcnLog.SL_EventTime);
			AssertEquals(new ZDateTimeOffset(2021, 1, 1, 10, 0, 0, new TimeSpan(10, 0, 0)), rcn.WRC_CompleteTime);
		}

		public void TestWRC_CompleteTime_CannotUpdateAfterInsert()
		{
			var warehouse = Helper.CreateTRWWarehouse("ABC");
			warehouse.WarehouseAddress.OA_City = "SYD";
			var rcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);

			AssertEquals("Precondition: RCN is not closed.", true, rcn.WRC_CompleteTime.IsEmpty);

			var completeTime = ZDateTimeOffset.Now;
			rcn.WRC_CompleteTime = completeTime;

			AssertEquals("RCN is closed.", false, rcn.WRC_CompleteTime.IsEmpty);

			rcn.WRC_CompleteTime = new ZDateTimeOffset(2024, 1, 1);
			AssertEquals("RCN complete time should not be changed.", completeTime, rcn.WRC_CompleteTime);
		}

		#endregion

		#region TestBusinessObjectsWithRelatedEvents_ContainsVisualizerDocumentData

		public void TestBusinessObjectsWithRelatedEvents_ContainsVisualizerDocumentData()
		{
			var warehouse = Helper.CreateTRWWarehouse("ABC");
			warehouse.WarehouseAddress.OA_City = "SYD";
			var rcn1 = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var rcn2 = Helper.CreateReceiveConsignment("RCN2", warehouse.PK);

			var documentData1 = (BusinessObject)Factory.New<IVisualizerDocumentData>();
			documentData1[JobDocumentDataSchema.JDD_ParentTableCode] = rcn1.TablePrefix;
			documentData1[JobDocumentDataSchema.JDD_ParentID] = rcn1.PK;

			var documentData2 = (BusinessObject)Factory.New<IVisualizerDocumentData>();
			documentData2[JobDocumentDataSchema.JDD_ParentTableCode] = rcn2.TablePrefix;
			documentData2[JobDocumentDataSchema.JDD_ParentID] = rcn2.PK;

			AssertContainsExactElementsInAnyOrder("BusinessObjectsWithRelatedEvents contains sent document data",
				new[] { documentData1 },
				rcn1.BusinessObjectsWithRelatedEvents.OfType<IVisualizerDocumentData>());
		}

		#endregion

		#region TestGetAttachedJobNumber Member

		public void TestGetAttachedJobNumber()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var rcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var package = Helper.CreatePackageState(rcn, 1, "PKG", "P1", "BKD");
			Helper.CreateAdditionalReference(package, "S0000001", "BPR");
			Factory.Save();
			AssertEquals("", "S0000001", ((IPackingParentWithAttachedParent)rcn).GetAttachedJobNumber(package.Package));
		}

		#endregion

		#region TestIsBlind

		public void TestIsBlind_ParentIDEmpty()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var rcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			rcn.WRC_ParentID = ZGuid.Empty;

			AssertEquals(true, rcn.IsBlind);
		}

		public void TestIsBlind_ParentIDNotEmpty()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var rcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			rcn.WRC_ParentID = ZGuid.ParseSafe("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");

			AssertEquals(false, rcn.IsBlind);
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

			var bookedPackage = Helper.CreatePackageState(receiveConsignment, 1, TransitConstants.TransitPackageUnitType.Package, "PKG1", TransitWarehouseStatuses.Codes.Booked);
			var arrivedPackage = Helper.CreatePackageState(receiveConsignment, 1, TransitConstants.TransitPackageUnitType.Package, "PKG2", TransitWarehouseStatuses.Codes.Arrived, rtu);

			Factory.Save();

			var outerPackages = receiveConsignment.OuterPackages;
			AssertEquals(2, outerPackages.Count());
			AssertContainsExactElementsInAnyOrder(new[] { bookedPackage.PK, arrivedPackage.PK }, outerPackages.Select(p => p.PK));
		}

		public void TestOuterPackages_Container()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			var receiveConsignment = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var uld = Helper.CreateReceiveTransportationUnitWithContainerType("ULD1", warehouse.PK, location.PK);

			var bookedPackage = Helper.CreatePackageState(receiveConsignment, 1, TransitConstants.TransitPackageUnitType.Package, "PKG1", TransitWarehouseStatuses.Codes.Booked);
			var arrivedPackage = Helper.CreatePackageState(receiveConsignment, 1, TransitConstants.TransitPackageUnitType.Package, "PKG2", TransitWarehouseStatuses.Codes.Arrived, uld);

			Factory.Save();

			var outerPackages = receiveConsignment.OuterPackages;
			AssertEquals(2, outerPackages.Count());
			AssertContainsExactElementsInAnyOrder(new[] { bookedPackage.PK, arrivedPackage.PK }, outerPackages.Select(p => p.PK));
		}

		public void TestOuterPackages_OVP()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var receiveConsignment = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);

			var ovp = Helper.CreateOverpackPackage("OVP1", receiveConsignment, rtu, status: TransitWarehouseStatuses.Codes.Arrived, rcn: receiveConsignment);
			var innerPackage = Helper.CreatePackageState(receiveConsignment, 1, TransitConstants.TransitPackageUnitType.Package, "Inner1", TransitWarehouseStatuses.Codes.Arrived, rtu);
			var innerPackline = Helper.CreatePackageState(receiveConsignment, 10, TransitConstants.TransitPackageUnitType.Package, "", TransitWarehouseStatuses.Codes.Arrived, rtu);
			helper.PackPackageIntoHandlingUnit(ovp, innerPackage, ZDateTimeOffset.Now, "ABC", ovp);
			helper.PackPackageIntoHandlingUnit(ovp, innerPackline, ZDateTimeOffset.Now, "ABC", ovp);

			Factory.Save();

			var outerPackages = receiveConsignment.OuterPackages;
			AssertEquals(1, outerPackages.Count());
			AssertContainsExactElementsInAnyOrder(new[] { ovp.PK }, outerPackages.Select(p => p.PK));
		}

		public void TestOuterPackages_OVPInContainer()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var receiveConsignment = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var uld = Helper.CreateReceiveTransportationUnitWithContainerType("ULD1", warehouse.PK, location.PK);

			var ovp = Helper.CreateOverpackPackage("OVP1", receiveConsignment, uld, status: TransitWarehouseStatuses.Codes.Arrived, rcn: receiveConsignment);
			var innerPackage = Helper.CreatePackageState(receiveConsignment, 1, TransitConstants.TransitPackageUnitType.Package, "Inner1", TransitWarehouseStatuses.Codes.Arrived, uld);
			var innerPackline = Helper.CreatePackageState(receiveConsignment, 10, TransitConstants.TransitPackageUnitType.Package, "", TransitWarehouseStatuses.Codes.Arrived, uld);
			helper.PackPackageIntoHandlingUnit(ovp, innerPackage, ZDateTimeOffset.Now, "ABC", ovp);
			helper.PackPackageIntoHandlingUnit(ovp, innerPackline, ZDateTimeOffset.Now, "ABC", ovp);

			Factory.Save();

			var outerPackages = receiveConsignment.OuterPackages;
			AssertEquals(1, outerPackages.Count());
			AssertContainsExactElementsInAnyOrder(new[] { ovp.PK }, outerPackages.Select(p => p.PK));
		}

		public void TestOuterPackages_OVPInHU()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var receiveConsignment = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);

			var topHU = helper.CreateHandlingUnitPackage("TopHU1", helper.CreatePackageHandlingUnit(), rtu, TransitWarehouseStatuses.Codes.Arrived, unitType: PackageStateUnitType.Codes.HandlingUnit);
			var innerPackage1 = Helper.CreatePackageState(receiveConsignment, 1, TransitConstants.TransitPackageUnitType.Package, "Inner1", TransitWarehouseStatuses.Codes.Arrived, rtu);

			var ovp = Helper.CreateOverpackPackage("OVP1", receiveConsignment, rtu, status: TransitWarehouseStatuses.Codes.Arrived, rcn: receiveConsignment);
			var innerPackage2 = Helper.CreatePackageState(receiveConsignment, 1, TransitConstants.TransitPackageUnitType.Package, "Inner2", TransitWarehouseStatuses.Codes.Arrived, rtu);
			var innerPackline = Helper.CreatePackageState(receiveConsignment, 10, TransitConstants.TransitPackageUnitType.Package, "", TransitWarehouseStatuses.Codes.Arrived, rtu);
			helper.PackPackageIntoHandlingUnit(ovp, innerPackage2, ZDateTimeOffset.Now, "ABC", topHU);
			helper.PackPackageIntoHandlingUnit(ovp, innerPackline, ZDateTimeOffset.Now, "ABC", topHU);

			helper.PackPackageIntoHandlingUnit(topHU, innerPackage1, ZDateTimeOffset.Now, "ABC", topHU);
			helper.PackPackageIntoHandlingUnit(topHU, ovp, ZDateTimeOffset.Now, "ABC", topHU);
			Factory.Save();

			var outerPackages = receiveConsignment.OuterPackages;
			AssertEquals(2, outerPackages.Count());
			AssertContainsExactElementsInAnyOrder(new[] { ovp.PK, innerPackage1.PK }, outerPackages.Select(p => p.PK));
		}

		public void TestOuterPackages_HU()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var receiveConsignment = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);

			var hu = helper.CreateHandlingUnitPackage("HU1", helper.CreatePackageHandlingUnit(), rtu, TransitWarehouseStatuses.Codes.Arrived, unitType: PackageStateUnitType.Codes.HandlingUnit);
			var innerPackage = Helper.CreatePackageState(receiveConsignment, 1, TransitConstants.TransitPackageUnitType.Package, "Inner1", TransitWarehouseStatuses.Codes.Arrived, rtu);
			var innerPackline = Helper.CreatePackageState(receiveConsignment, 10, TransitConstants.TransitPackageUnitType.Package, "", TransitWarehouseStatuses.Codes.Arrived, rtu);
			helper.PackPackageIntoHandlingUnit(hu, innerPackage, ZDateTimeOffset.Now, "ABC", hu);
			helper.PackPackageIntoHandlingUnit(hu, innerPackline, ZDateTimeOffset.Now, "ABC", hu);

			Factory.Save();

			var outerPackages = receiveConsignment.OuterPackages;
			AssertEquals(2, outerPackages.Count());
			AssertContainsExactElementsInAnyOrder(new[] { innerPackage.PK, innerPackline.PK }, outerPackages.Select(p => p.PK));
		}

		public void TestOuterPackages_MultipleHU()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var receiveConsignment = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);

			var topHU = helper.CreateHandlingUnitPackage("TopHU1", helper.CreatePackageHandlingUnit(), rtu, TransitWarehouseStatuses.Codes.Arrived, unitType: PackageStateUnitType.Codes.HandlingUnit);
			var innerPackage1 = Helper.CreatePackageState(receiveConsignment, 1, TransitConstants.TransitPackageUnitType.Package, "Inner1", TransitWarehouseStatuses.Codes.Arrived, rtu);

			var hu = helper.CreateHandlingUnitPackage("HU1", helper.CreatePackageHandlingUnit(), rtu, TransitWarehouseStatuses.Codes.Arrived, unitType: PackageStateUnitType.Codes.HandlingUnit);
			var innerPackage2 = Helper.CreatePackageState(receiveConsignment, 1, TransitConstants.TransitPackageUnitType.Package, "Inner2", TransitWarehouseStatuses.Codes.Arrived, rtu);
			var innerPackline = Helper.CreatePackageState(receiveConsignment, 10, TransitConstants.TransitPackageUnitType.Package, "", TransitWarehouseStatuses.Codes.Arrived, rtu);
			helper.PackPackageIntoHandlingUnit(hu, innerPackage2, ZDateTimeOffset.Now, "ABC", topHU);
			helper.PackPackageIntoHandlingUnit(hu, innerPackline, ZDateTimeOffset.Now, "ABC", topHU);

			helper.PackPackageIntoHandlingUnit(topHU, innerPackage1, ZDateTimeOffset.Now, "ABC", topHU);
			helper.PackPackageIntoHandlingUnit(topHU, hu, ZDateTimeOffset.Now, "ABC", topHU);

			Factory.Save();

			var outerPackages = receiveConsignment.OuterPackages;
			AssertEquals(3, outerPackages.Count());
			AssertContainsExactElementsInAnyOrder(new[] { innerPackage1.PK, innerPackage2.PK, innerPackline.PK }, outerPackages.Select(p => p.PK));
		}

		public void TestOuterPackages_HUInContainer()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var receiveConsignment = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var uld = Helper.CreateReceiveTransportationUnitWithContainerType("ULD1", warehouse.PK, location.PK);

			var hu = helper.CreateHandlingUnitPackage("HU1", helper.CreatePackageHandlingUnit(), uld, TransitWarehouseStatuses.Codes.Arrived, unitType: PackageStateUnitType.Codes.HandlingUnit);
			var innerPackage = Helper.CreatePackageState(receiveConsignment, 1, TransitConstants.TransitPackageUnitType.Package, "Inner1", TransitWarehouseStatuses.Codes.Arrived, uld);
			var innerPackline = Helper.CreatePackageState(receiveConsignment, 10, TransitConstants.TransitPackageUnitType.Package, "", TransitWarehouseStatuses.Codes.Arrived, uld);
			helper.PackPackageIntoHandlingUnit(hu, innerPackage, ZDateTimeOffset.Now, "ABC", hu);
			helper.PackPackageIntoHandlingUnit(hu, innerPackline, ZDateTimeOffset.Now, "ABC", hu);

			Factory.Save();

			var outerPackages = receiveConsignment.OuterPackages;
			AssertEquals(2, outerPackages.Count());
			AssertContainsExactElementsInAnyOrder(new[] { innerPackage.PK, innerPackline.PK }, outerPackages.Select(p => p.PK));
		}

		#endregion

		#region TestOrderReferences

		public void TestOrderReferences()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var rcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var consignmentOrderReference1 = Helper.CreateWhsItemConsignmentOrderReference("REF1", rcn);
			var consignmentOrderReference2 = Helper.CreateWhsItemConsignmentOrderReference("REF2", rcn);
			Factory.Save();

			var consignmentOrderReferenceCollection = rcn.OrderReferences;
			AssertNotNull(consignmentOrderReferenceCollection);
			AssertEquals("Should have 2 order refs", 2, consignmentOrderReferenceCollection.Count);

			AssertEquals("Order Ref Should Be REF1", "REF1", consignmentOrderReferenceCollection.Single(s => s.PK == consignmentOrderReference1.PK).WOR_OrderReference);
			AssertEquals("Order Ref Should Be REF2", "REF2", consignmentOrderReferenceCollection.Single(s => s.PK == consignmentOrderReference2.PK).WOR_OrderReference);
		}

		#region TestDelete

		public void TestDeleteConsignmentDeletesOrderReferences()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var rcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var consignmentOrderReference = Helper.CreateWhsItemConsignmentOrderReference("REF1", rcn);
			Factory.Save();

			rcn.Delete();
			AssertEquals(true, consignmentOrderReference.IsDeleted);
			AssertNull(Factory.LoadTop1<WhsItemConsignmentOrderReference>(new ZQuery()));
		}

		#endregion

		#endregion

		#region TestInvoicingJob

		public void TestReleaseMutexesOnUnusedJobs_ConsolHasSubShipmentWithWarehouseRecieveConsignment_ShouldNotDeleteInternalJob()
		{
			// Arrange
			var accountingCreator = new TestObjectCreator(Factory);
			var consol = accountingCreator.CreateConsol();
			var leadShipment = accountingCreator.CreateShipment("MASTER", consol);
			leadShipment.JS_ShipmentType = ShipmentTypes.AssemblyMaster;
			var subShipment1 = accountingCreator.CreateShipment("SUB1", consol);
			subShipment1.JS_JS_ColoadMasterShipment = leadShipment.PK;
			subShipment1.OuterPackLines.AddNew().JL_PackageCount = 10;
			var subShipment2 = accountingCreator.CreateShipment("SUB2", consol);
			subShipment2.JS_JS_ColoadMasterShipment = leadShipment.PK;
			subShipment2.JS_OA_ExportReceivingDepot = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			subShipment2.OuterPackLines.AddNew().JL_PackageCount = 10;

			var warehouse = Helper.CreateTransitWarehouseInCurrentBranch();
			var receiveConsignment = Helper.CreateReceiveConsignment("WRC1", warehouse.PK);
			receiveConsignment.WRC_ParentID = subShipment2.PK;
			receiveConsignment.WRC_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			var receiveConsignmentJob = new JobHeader.Loader(receiveConsignment).TryLoadOrCreate();
			Factory.Save();

			var apportionments = consol.GetApportionments();
			apportionments.LoadChildShipmentsAndAcquireMutexesWhereRequired();

			var cost = accountingCreator.CreateConsolCost(consol, accountingCreator.FRT, 240m, apportionmentListing: apportionments);
			AssertEquals("Pre-condition: creating cost creates jobs for all shipments", 3, apportionments.JobsWithMutexes_ForTestOnly.Count);
			AssertEquals("Pre-condition: but should only apportion to the master shipment", 1, cost.ApportionmentCharges.Count);

			// Act
			Factory.Save();

			// Assert
			var message = "MASTER shipment job should remain because it's apportioned to by the consolcost. " +
				"SUB1 should remain as it's the parent job of the receiveConsignment so only SUB2 job should be deleted";
			var shipmentJobQuery = new ZQuery(JobHeaderSchema.JH_ParentTableCode, JobShipmentSchema.Constants.Prefix);
			var shipmentJobsInFactory = Factory.Load<JobHeader>(shipmentJobQuery).Select(x => x.Parent.JobNumber);
			AssertContainsExactElementsInAnyOrder(message, new[] { "MASTER", "SUB2" }, shipmentJobsInFactory);
		}

		#endregion

		WhsTransitTestHelper Helper => helper ?? (helper = new WhsTransitTestHelper(Factory));
		WhsTransitTestHelper helper;
	}

	#region WhsItemReceiveConsignmentWorkflowProviderTest

	[TestedType(typeof(WhsItemReceiveConsignment))]
	public class WhsItemReceiveConsignmentWorkflowProviderTest : WorkflowProviderTest<WhsItemReceiveConsignment, WhsItemReceiveConsignmentProcessTaskCollection>
	{
		protected override ZString ExpectedWorkflowType
		{
			get { return WorkflowDescriptors.TransitReceiveConsignment; }
		}
	}

	#endregion
}
