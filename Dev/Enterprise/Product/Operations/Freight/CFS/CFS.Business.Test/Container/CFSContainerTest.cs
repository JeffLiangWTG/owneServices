using System;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;
using EventConstants = CargoWise.EventReference.Constants;

namespace Enterprise.Freight.CFS.Business.Testing
{
	public class CFSContainerTest : CommonContainerTest2
	{
		#region TestNotIncludingChildEditableObjectsIfViewingFromPortTransportLegPlanner

		public void TestNotIncludingChildEditableObjectsIfViewingFromPortTransportLegPlanner()
		{
			var consol = (CFSLoadListConsol)GetNewConsol();
			var container = consol.Containers.AddNew();
			Factory.Save();

			var originConfirm = container.OriginConfirm;
			originConfirm.EU_Distance = 3;
			AssertEquals(true, container.HasChanges);
			Factory.Save();

			var newFactory2 = new BusinessObjectFactory();
			FactoryCacheHelper.SetIsViewingFromPortTransportLegPlanner(newFactory2);

			var container_NewFactory = newFactory2.Load<CFSContainer>(container.PK);

			var destinationConfirm = container.DestinationConfirm;
			destinationConfirm.EU_Distance = 3;
			AssertEquals(false, container_NewFactory.HasChanges);
		}

		#endregion

		public void TestConfirmEnumeration_ValidationShouldNotThrowError()
		{
			var consol = (CFSLoadListConsol)GetNewConsol();
			var container = consol.Containers.AddNew();

			container.JC_DepartureTime = ZDateTime.Now;
			var arrivalConfirm = container.DestinationCFSArrival;

			AssertEquals(arrivalConfirm.EU_PickupDeliveryType, Constants.PickupDeliveryConfirmTypes.DestinationCFSArrival);

			arrivalConfirm.Delete();

			consol.RunPreSaveValidation();

			CombineAssertions("Validation should not throw an error", () =>
			{
				Assert(ErrorReporter.LastKeyReported.IsNullOrEmpty());
				Assert(ErrorReporter.LastMessageReported.IsNullOrEmpty());
			});
		}

		public void TestContainerIsRegisteredEditableChildObjectByCFSArrival()
		{
			var container = Factory.New<CFSContainer>();
			Assert("Container is registered editable child object by CFSArrival", container.IsRegisteredEditableChildObject(container.CFSArrival));
		}

		public void TestJC_ArrivalTruckRegistration_MaxLength()
		{
			CFSLoadListConsol consol = (CFSLoadListConsol)GetNewConsol();
			CFSContainer container = consol.Containers.AddNew();
			AssertEquals(8, container.JC_ArrivalTruckRegistrationInfo.MaxLength);
		}

		public void TestJC_DepartureTruckRegistration_MaxLength()
		{
			CFSLoadListConsol consol = (CFSLoadListConsol)GetNewConsol();
			CFSContainer container = consol.Containers.AddNew();
			AssertEquals(8, container.JC_DepartureTruckRegistrationInfo.MaxLength);
		}

		public void TestJC_DepartureTruckDriversLicense_MaxLength()
		{
			CFSLoadListConsol consol = (CFSLoadListConsol)GetNewConsol();
			CFSContainer container = consol.Containers.AddNew();
			AssertEquals(15, container.JC_DepartureTruckDriversLicenseInfo.MaxLength);
		}

		public void TestJC_ArrivalTruckDriversLicense_MaxLength()
		{
			CFSLoadListConsol consol = (CFSLoadListConsol)GetNewConsol();
			CFSContainer container = consol.Containers.AddNew();
			AssertEquals(15, container.JC_ArrivalTruckDriversLicenseInfo.MaxLength);
		}

		public void TestGetEDocsProviderSupporter()
		{
			IEDocsProvider container = Factory.New<CFSContainer>();
			AssertType("GetEDocsProviderSupporter().GetType()", typeof(JobInvoicingEDocsProviderSupporter), container.GetEDocsProviderSupporter());
		}

		public void TestSetDefaultValues()
		{
			CFSContainer container = Factory.New<CFSContainer>();
			AssertEquals(ContainerPurposeTypeCodeDescriptionPairList.Codes.CFS, container.JC_Purpose);
		}

		public void TestJC_PurposeInfo()
		{
			AssertEquals(true, CFSContainer.JC_PurposeInfo.ReadOnly);
		}

		public void TestJC_Purpose_List()
		{
			AssertEquals(2, CFSContainer.JC_Purpose_List.Count);
			AssertEquals(ContainerPurposeTypeCodeDescriptionPairList.Codes.CFS, CFSContainer.JC_Purpose_List[0].Code);
			AssertEquals(ContainerPurposeTypeCodeDescriptionPairList.Codes.Storage, CFSContainer.JC_Purpose_List[1].Code);
		}

		public void TestJC_OH_ShippingLine()
		{
			OrgHeader shippingLine = Factory.New<OrgHeader>();
			shippingLine.OH_IsShippingProvider = true;

			CFSLoadListConsol consol = Factory.New<CFSLoadListConsol>();
			consol.JK_OA_ShippingLineAddress = shippingLine.MainAddress.PK;
			CFSContainer.JC_JK = consol.PK;

			AssertEquals(consol.ShippingLinePK, CFSContainer.JC_OH_ShippingLine);
		}

		public void TestJC_FCLAvailable()
		{
			var container = Factory.New<CFSContainer>();

			var loadList = Factory.New<PackUnpackLoadListConsol>();
			loadList.AutomaticallyUpdatePackLineContainers = true;
			loadList.JK_TransportMode = Constants.TransportModes.Sea;
			loadList.Containers.Add(container);
			Assert("Precondition", !container.JC_OverrideFCLAvailableStorage);
			AssertEquals("The last transport does not have Terminal Availability Date", ZDateTime.Empty, container.JC_FCLAvailable);
			Assert(container.JC_FCLAvailableInfo.ReadOnly);

			var transport = loadList.Transports[0];
			transport.JW_JX = ImportSailing1.PK;
			var tomorrow = ZDateTime.Today.AddDays(1);
			transport.JW_TerminalAvailabilityDate = tomorrow;
			loadList.JK_RL_NKLoadPort = transport.JW_RL_NKLoadPort;
			loadList.JK_RL_NKDischargePort = transport.JW_RL_NKDiscPort;

			var shipment = loadList.Shipments.AddNew() as CFSShipment;
			shipment.JS_RL_NKOrigin = OverseasPort2;
			shipment.JS_RL_NKDestination = HomePort;
			var pack = shipment.OuterPackLines.AddNew() as PackLine;
			loadList.Containers.Add(container);
			container.PackLines.Add(pack);

			Assert("Precondition", !container.JC_OverrideFCLAvailableStorage);
			AssertEquals("Return transport.JW_TerminalAvailabilityDate if JC_OverrideFCLAvailableStorage is false and the last transport is not null", tomorrow, container.JC_FCLAvailable);
			Assert(container.JC_FCLAvailableInfo.ReadOnly);

			container.JC_FCLAvailable = ZDateTime.Today;
			Assert("If FCLAvailable is updated, JC_OverrideFCLAvailableStorage is updated to true", container.JC_OverrideFCLAvailableStorage);
			Assert("If JC_OverrideFCLAvailableStorage is true, then FCLAavailable is editable", !container.JC_FCLAvailableInfo.ReadOnly);
			AssertEquals(ZDateTime.Today, container.JC_FCLAvailable);
		}

		public void TestJC_OH_ShippingLineInfo()
		{
			AssertEquals(true, CFSContainer.JC_OH_ShippingLineInfo.ReadOnly);
		}

		[TestDate(2006, 1, 1)]
		public void TestIsStorageJob_IsCFSJob()
		{
			CFSContainer cFSContainer = Factory.New<CFSContainer>();
			cFSContainer.JC_Purpose = ContainerPurposeTypeCodeDescriptionPairList.Codes.Storage;
			AssertEquals(true, cFSContainer.IsStorageJob);
			AssertEquals(false, cFSContainer.IsCFSJob);

			cFSContainer.JC_Purpose = ContainerPurposeTypeCodeDescriptionPairList.Codes.CFS;
			AssertEquals(false, cFSContainer.IsStorageJob);
			AssertEquals(true, cFSContainer.IsCFSJob);
		}

		[TestDate(2006, 1, 1)]
		public void TestCreateStorageFromPack()
		{
			AssertEquals(ContainerPurposeTypeCodeDescriptionPairList.Codes.CFS, CFSContainer.JC_Purpose);
			CFSContainer.JC_TransportMode = Constants.TransportModes.Sea;
			CFSContainer.JC_ContainerMode = Constants.ContainerModes.FCL;
			CFSContainer.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			CFSContainer.JC_ContainerNum = "12345";
			CFSContainer.JC_SealNum = "67890";
			CFSContainer.JC_SealParty = "CAR";
			CFSContainer.JC_AdditionalSealParty = "CRD";
			CFSContainer.JC_Additional2SealParty = "CUS";
			CFSContainer.JC_OH_CFSClient = Factory.New<OrgHeader>().PK;

			CFSContainer.JC_JK = CFSLoadListConsol.PK;
			CFSLoadListConsol.JK_RL_NKDischargePort = "AUSYD";
			CFSLoadListConsol.JK_RL_NKLoadPort = "SGSIN";
			CFSLoadListConsol.JK_MasterBillNum = "23211111111";
			CFSLoadListConsol.Transports.MostInterestingTransport.JW_JX = ZGuid.NewZGuid();

			CFSContainer.JC_FCLStorageModuleOnlyMaster = "08111111111";
			CFSContainer.JC_EmptyReturnedBy = ZDateTime.Now;
			CFSContainer.JC_OA_DepartureContainerYardAddress = Factory.New<OrgAddress>().PK;
			CFSContainer.JC_OA_ArrivalContainerYardAddress = Factory.New<OrgAddress>().PK;

			CFSContainer clonedCFSContainer = CFSContainer.CreateNewContainer();
			// close old job off
			AssertEquals(ZDateTime.Now, CFSContainer.JC_DepartureTime);

			// open new job
			AssertEquals(ContainerPurposeTypeCodeDescriptionPairList.Codes.Storage, clonedCFSContainer.JC_Purpose);
			AssertEquals(Constants.TransportModes.Sea, clonedCFSContainer.JC_TransportMode);
			AssertEquals(Constants.ContainerModes.FCL, clonedCFSContainer.JC_ContainerMode);
			AssertEquals(CFSContainer.JC_RC, clonedCFSContainer.JC_RC);
			AssertEquals("12345", clonedCFSContainer.JC_ContainerNum);
			AssertEquals("67890", clonedCFSContainer.JC_SealNum);
			AssertEquals("CAR", clonedCFSContainer.JC_SealParty);
			AssertEquals("CRD", clonedCFSContainer.JC_AdditionalSealParty);
			AssertEquals("CUS", clonedCFSContainer.JC_Additional2SealParty);
			AssertEquals(CFSContainer.JC_OH_CFSClient, clonedCFSContainer.JC_OH_CFSClient);
			AssertEquals(ZGuid.Empty, clonedCFSContainer.JC_JK);
			AssertEquals(CFSLoadListConsol.JK_JX_Sailing, clonedCFSContainer.JC_JX);
			AssertEquals("23211111111", clonedCFSContainer.JC_FCLStorageModuleOnlyMaster);
			AssertEquals(CFSContainer.JC_EmptyReturnedBy, clonedCFSContainer.JC_EmptyReturnedBy);
			AssertEquals(CFSContainer.JC_OA_DepartureContainerYardAddress, clonedCFSContainer.JC_OA_DepartureContainerYardAddress);
			AssertEquals(CFSContainer.JC_OA_ArrivalContainerYardAddress, clonedCFSContainer.JC_OA_ArrivalContainerYardAddress);
			AssertEquals(ZDateTime.Now, clonedCFSContainer.JC_ArrivalTime);
		}

		public void TestCreateStorageFromPackWithNoConsol()
		{
			AssertEquals(ContainerPurposeTypeCodeDescriptionPairList.Codes.CFS, CFSContainer.JC_Purpose);
			CFSContainer.JC_TransportMode = Constants.TransportModes.Sea;
			CFSContainer.JC_ContainerMode = Constants.ContainerModes.FCL;
			CFSContainer.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			CFSContainer.JC_ContainerNum = "12345";
			CFSContainer.JC_SealNum = "67890";
			CFSContainer.JC_OH_CFSClient = Factory.New<OrgHeader>().PK;

			CFSContainer clonedCFSContainer = CFSContainer.CreateNewContainer();
			AssertEquals(true, clonedCFSContainer.JC_OverrideFCLAvailableStorage);
		}

		public override void TestValidateJC_DeliveryMode()
		{
			Assert("Delivery Mode Does not apply in CFS", true);
		}

		public override void TestValidateJC_ContainerNum()
		{
			Assert("Duplicate number validation Does not apply in CFS", true);
		}

		[TestDate(2006, 1, 1)]
		public void TestCreateUnpackFromStorage()
		{
			CFSContainer.JC_Purpose = ContainerPurposeTypeCodeDescriptionPairList.Codes.Storage;

			CFSContainer.JC_TransportMode = Constants.TransportModes.Sea;
			CFSContainer.JC_ContainerMode = Constants.ContainerModes.FCL;
			CFSContainer.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			CFSContainer.JC_ContainerNum = "12345";
			CFSContainer.JC_SealNum = "67890";
			CFSContainer.JC_OH_CFSClient = Factory.New<OrgHeader>().PK;
			CFSContainer.JC_JX = ZGuid.NewZGuid();
			CFSContainer.JC_FCLStorageModuleOnlyMaster = "08111111111";
			CFSContainer.JC_EmptyReturnedBy = ZDateTime.Now;
			CFSContainer.JC_OA_DepartureContainerYardAddress = Factory.New<OrgAddress>().PK;
			CFSContainer.JC_OA_ArrivalContainerYardAddress = Factory.New<OrgAddress>().PK;

			CFSContainer clonedCFSContainer = CFSContainer.CreateNewContainer();
			//close old job off
			AssertEquals(ZDateTime.Now, CFSContainer.JC_DepartureTime);

			//open new job
			AssertEquals(ContainerPurposeTypeCodeDescriptionPairList.Codes.CFS, clonedCFSContainer.JC_Purpose);
			AssertEquals(Constants.TransportModes.Sea, clonedCFSContainer.JC_TransportMode);
			AssertEquals(Constants.ContainerModes.FCL, clonedCFSContainer.JC_ContainerMode);
			AssertEquals(CFSContainer.JC_RC, clonedCFSContainer.JC_RC);
			AssertEquals("12345", clonedCFSContainer.JC_ContainerNum);
			AssertEquals("67890", clonedCFSContainer.JC_SealNum);
			AssertEquals(CFSContainer.JC_OH_CFSClient, clonedCFSContainer.JC_OH_CFSClient);
			AssertEquals(CFSContainer.JC_JX, clonedCFSContainer.JC_JX);
			AssertEquals(CFSContainer.JC_EmptyReturnedBy, clonedCFSContainer.JC_EmptyReturnedBy);
			AssertEquals(CFSContainer.JC_OA_DepartureContainerYardAddress, clonedCFSContainer.JC_OA_DepartureContainerYardAddress);
			AssertEquals(CFSContainer.JC_OA_ArrivalContainerYardAddress, clonedCFSContainer.JC_OA_ArrivalContainerYardAddress);
			AssertEquals("", clonedCFSContainer.JC_FCLStorageModuleOnlyMaster);
			AssertEquals(ZDateTime.Empty, clonedCFSContainer.JC_ArrivalCTOStorageStartDate);
			AssertEquals(ZDateTime.Now, clonedCFSContainer.JC_ArrivalTime);
		}

		#region IJobInvoicingPlugin Members

		public void TestEditSecurity()
		{
			IJobInvoicingPlugIn testJob = Factory.New<CFSContainer>();
			AssertEquals("EditSecurityCheckPoint should be Empty", Env.Security.None, testJob.InvoicingSupporter.EditSecurityCheckpoint);
			AssertEquals("EditSecuritytMessage should be Empty", ZString.Empty, testJob.InvoicingSupporter.EditSecurityMessage);
			AssertEquals("EditSecurityLock should be False", ZBool.False, testJob.InvoicingSupporter.EditSecurityLock);
		}

		public void TestIJobInvoicingPlugInDefaultChargeGroup()
		{
			IJobInvoicingPlugIn testJob = Factory.New<CFSContainer>();
			AssertEquals("DefaultChargeGroup should be Empty", ZString.Empty, testJob.InvoicingSupporter.DefaultChargeGroup);
		}

		#endregion

		#region IJobHeaderParent Members

		public void TestIJobHeaderParent_AllowInvoiceDeletion()
		{
			IJobHeaderParent cfsContainer = Factory.New<CFSContainer>();
			Assert(cfsContainer.AllowInvoiceDeletion);
		}

		#endregion

		#region Container Yard Address Tests

		public void TestJC_OA_ArrivalContainerYardAddressDefaultAddressType()
		{
			CFSContainer container = (CFSContainer)GetNewContainer();
			AssertEquals("Default Address Type", AddressType.DLV, container.JC_OA_ArrivalContainerYardAddress_ZAddress.DefaultAddressType);
		}

		public void TestJC_OA_DepartureContainerYardAddressDefaultAddressType()
		{
			CFSContainer container = (CFSContainer)GetNewContainer();
			AssertEquals("Default Address Type", AddressType.PIC, container.JC_OA_DepartureContainerYardAddress_ZAddress.DefaultAddressType);
		}

		public void TestContainerYardAddress()
		{
			var consol = (CFSLoadListConsol)GetNewConsol();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.Transports[0].JW_JX = ExportSailing1.PK;
			var container = consol.Containers.AddNew();
			AssertEquals("Export Container Container Yard Address Default Address Type", AddressType.PIC, container.ContainerYardAddress_ZAddress.DefaultAddressType);
			consol.Transports[0].JW_JX = ImportSailing1.PK;
			AssertEquals("Export Container Container Yard Address Default Address Type", AddressType.DLV, container.ContainerYardAddress_ZAddress.DefaultAddressType);
		}

		public void TestContainerLegsPackDepot()
		{
			GlbBranch.CurrentBranch.OrgProxy.OH_IsPackDepot = false;
			GlbBranch.CurrentBranch.OrgProxy.OH_IsUnpackDepot = true;
			CFSContainer container = (CFSContainer)GetNewContainer();
			AssertNotNull("Arrival Container Leg at Pack Depot", container.CFSArrival);
			AssertNotNull("Departure Container Leg at Pack Depot", container.CFSDispatch);
		}

		public void TestContainerLegsUnpackDepot()
		{
			GlbBranch.CurrentBranch.OrgProxy.OH_IsPackDepot = true;
			GlbBranch.CurrentBranch.OrgProxy.OH_IsUnpackDepot = false;
			CFSContainer container = (CFSContainer)GetNewContainer();
			AssertNotNull("Arrival Container Leg at Unpack Depot", container.CFSArrival);
			AssertNotNull("Departure Container Leg at Unpack Depot", container.CFSDispatch);
		}

		public void TestContainerLegsPackUnpackDepot()
		{
			GlbBranch.CurrentBranch.OrgProxy.OH_IsPackDepot = true;
			GlbBranch.CurrentBranch.OrgProxy.OH_IsUnpackDepot = true;
			CFSContainer container = (CFSContainer)GetNewContainer();
			AssertNotNull("Arrival Container Leg at Pack/Unpack Depot", container.CFSArrival);
			AssertNotNull("Departure Container Leg at Pack/Unpack Depot", container.CFSDispatch);
		}

		public void TestContainerLegsNonDepot()
		{
			GlbBranch.CurrentBranch.OrgProxy.OH_IsPackDepot = false;
			GlbBranch.CurrentBranch.OrgProxy.OH_IsUnpackDepot = false;
			CFSContainer container = (CFSContainer)GetNewContainer();
			AssertNotNull("Arrival Container Leg at non Depot", container.CFSArrival);
			AssertNotNull("Departure Container Leg at non Depot", container.CFSDispatch);
		}

		public void TestContainerYardAddressReadOnlyIfNoSailing()
		{
			CFSContainer container = (CFSContainer)GetNewContainer();
			AssertEquals("Container Yard Address Default Read-only Status", true, container.ContainerYardAddressInfo.ReadOnly);
		}

		public void TestContainerYardAddressEditableIfPacking()
		{
			CFSContainer container = (CFSContainer)GetNewContainer();
			container.JC_JX = ExportSailing.PK;
			Assert("PackUnpack Status is Packing", container.PackOrUnpackStatus == PackUnpackStatusHelper.PackUnpackStatus.Pack || container.PackOrUnpackStatus != PackUnpackStatusHelper.PackUnpackStatus.Unpack);
			AssertEquals("Container Yard Address Read-only Status if Packing", false, container.ContainerYardAddressInfo.ReadOnly);
		}

		public void TestContainerYardAddressReadOnlyIfNonpack()
		{
			CFSContainer container = (CFSContainer)GetNewContainer();
			container.JC_JX = ExportSailing.PK;
			ExportSailing.Origin.JA_RL_NKPortOfLoading = OverseasPort;
			ExportSailing.Destination.JB_RL_NKPortOfDischarge = OverseasPort2;
			Assert("PackUnpack Status is Non-packing", container.PackOrUnpackStatus != PackUnpackStatusHelper.PackUnpackStatus.Pack && container.PackOrUnpackStatus != PackUnpackStatusHelper.PackUnpackStatus.Pack);
			AssertEquals("Container Yard Address Read-only Status If Non-Pack", true, container.ContainerYardAddressInfo.ReadOnly);
		}

		public void TestContainerYardAddressUnset()
		{
			CFSContainer container = (CFSContainer)GetNewContainer();
			container.JC_JX = ExportSailing.PK;
			container.ContainerYardAddress = LocalContainerYard.MainAddress.PK;
			container.JC_JX = ZGuid.Empty;
			Assert("PackUnpack Status is Non-packing", container.PackOrUnpackStatus != PackUnpackStatusHelper.PackUnpackStatus.Pack && container.PackOrUnpackStatus != PackUnpackStatusHelper.PackUnpackStatus.Pack);
			AssertEquals("Container Yard Address Read-only when Packing status is changed", true, container.ContainerYardAddressInfo.ReadOnly);
			AssertEquals("Container Yard Address is Empty when Packing status is changed", ZGuid.Empty, container.ContainerYardAddress);
			AssertEquals("Container Yard Address Org is Empty when Packing status is changed", ZGuid.Empty, container.ContainerYardAddress_ZAddress.OrgPK);
		}

		public void TestContainerYardAddressBetweenBranches()
		{
			var homePort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;

			CFSContainer container = (CFSContainer)GetNewContainer();
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = ExportSailing.Origin.JA_RL_NKPortOfLoading;
			container.JC_JX = ExportSailing.PK;
			container.ContainerYardAddress = LocalContainerYard.MainAddress.PK;
			Factory.Save();
			AssertNotEquals("Container Yard Lookup from Origin", ZGuid.Empty, container.ContainerYardAddress);

			GlbBranch.CurrentBranch.GB_RL_NKHomePort = ExportSailing.Destination.JB_RL_NKPortOfDischarge;
			container.ContainerYardAddress = LocalContainerYard.MainAddress.PK;
			Factory.Save();
			AssertNotEquals("Container Yard Lookup from Destination", ZGuid.Empty, container.ContainerYardAddress);

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			CFSContainer loadedContainer = newFactory.Load<CFSContainer>(container.PK);
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = ExportSailing.Origin.JA_RL_NKPortOfLoading;
			AssertNotEquals("Container Yard Lookup from Origin", ZGuid.Empty, loadedContainer.ContainerYardAddress);

			GlbBranch.CurrentBranch.GB_RL_NKHomePort = homePort;
		}

		#endregion

		#region TestValidateJC_ArrivalTime

		public void TestValidateJC_ArrivalTime()
		{
			CFSContainer container = (CFSContainer)GetNewContainer();

			container.JC_ArrivalTime = ZDateTime.Empty;
			container.JC_DepartureTime = ZDateTime.Empty;
			Assert("both invalid, can't compare them", !container.JC_ArrivalTimeInfo.HasErrors());

			container.JC_ArrivalTime = ZDateTime.Today;
			Assert("one invalid, can't compare them", !container.JC_ArrivalTimeInfo.HasErrors());

			container.JC_ArrivalTime = ZDateTime.Empty;
			container.JC_DepartureTime = ZDateTime.Today;
			Assert("one invalid, can't compare them", !container.JC_ArrivalTimeInfo.HasErrors());

			container.JC_ArrivalTime = ZDateTime.Today;
			Assert("Equal, that's OK", !container.JC_ArrivalTimeInfo.HasErrors());
		}

		#endregion

		public void TestJC_GrossWeightUQ_Conversion()
		{
			var container = Factory.New<CFSContainer>();
			container.JC_GrossWeight = 33;
			container.JC_GrossWeightUQ = "KG";
			container.JC_GrossWeightUQ = "G";

			AssertEquals((ZDecimal)33000, container.JC_GrossWeight);
		}

		#region TestArrivalAndDepartureLegsAreDeletedOnDelete

		[ExpectNoExceptions]
		public void TestArrivalAndDepartureLegsAreDeletedOnDelete()
		{
			AssertNotNull("PreCondition: Not expecting current branch's org to be null", GlbBranch.CurrentBranch.OrgProxy);
			CFSLoadListConsol consol = Factory.New<CFSLoadListConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.Transports[0].JW_JX = ImportSailing.PK;
			CFSContainer container1 = consol.Containers.AddNew();
			AssertNotNull("Should have added a depart leg", container1.CFSDispatch);
			container1.Delete();
			AssertEquals("Collection shouldn't contain container", 0, consol.Containers.Count);

			Factory.Save();
		}

		#endregion

		#region TestValidateJC_OH_CFSClient

		public void TestValidateJC_OH_CFSClient()
		{
			CFSContainer cR = Factory.New<CFSContainer>();
			cR.JC_OH_CFSClient = ZGuid.Empty;
			cR.Validation.ValidateJC_OH_CFSClient();
			Assert("Expecting JC_OH_CFSClient to have errors, field is mandatory.", cR.JC_OH_CFSClientInfo.HasErrors());

			cR.JC_OH_CFSClient = ZGuid.NewZGuid();
			Assert("Expecting JC_OH_CFSClient to have errors, org is not a forwarder or shipping provider.", cR.JC_OH_CFSClientInfo.HasErrors());

			cR.JC_OH_CFSClient = LocalForwarder.PK;
			Assert("Not Expecting JC_OH_CFSClient to have errors, org is valid.", !cR.JC_OH_CFSClientInfo.HasErrors());
		}

		#endregion

		#region TestSetContainerModeSetsShipments

		public void TestSetContainerModeSetsShipments()
		{
			var container = Factory.New<CFSContainer>();
			var shipment = container.PackUnpackShipments.AddNew();

			container.JC_ContainerMode = Constants.ContainerModes.Groupage;
			AssertEquals("Expecting Shipment Packing Mode to be LCL.", Constants.ContainerModes.LCL, shipment.JS_PackingMode);
			AssertNoErrors("Not expecting Shipment Packing Mode to have errors.", shipment.JS_PackingModeInfo);

			container.JC_ContainerMode = Constants.ContainerModes.FCL;
			AssertEquals("Expecting Shipment Packing Mode to be FCL.", Constants.ContainerModes.FCL, shipment.JS_PackingMode);
			AssertNoErrors("Not expecting Shipment Packing Mode to have errors.", shipment.JS_PackingModeInfo);

			container.JC_ContainerMode = Constants.ContainerModes.BuyersConsol;
			AssertEquals("Expecting Shipment Packing Mode to be BCN.", Constants.ContainerModes.BuyersConsol, shipment.JS_PackingMode);
			AssertNoErrors("Not expecting Shipment Packing Mode to have errors.", shipment.JS_PackingModeInfo);

			container.JC_ContainerMode = Constants.ContainerModes.LCL;
			AssertEquals("Expecting Shipment Packing Mode to be LCL.", Constants.ContainerModes.LCL, shipment.JS_PackingMode);
			AssertNoErrors("Not expecting Shipment Packing Mode to have errors.", shipment.JS_PackingModeInfo);

			container.JC_ContainerMode = Constants.ContainerModes.Bulk;
			AssertEquals("Expecting Shipment Packing Mode to be BLK.", Constants.ContainerModes.Bulk, shipment.JS_PackingMode);
			AssertNoErrors("Not expecting Shipment Packing Mode to have errors.", shipment.JS_PackingModeInfo);

			container.JC_ContainerMode = Constants.ContainerModes.Liquid;
			AssertEquals("Expecting Shipment Packing Mode to be LQD.", Constants.ContainerModes.Liquid, shipment.JS_PackingMode);
			AssertNoErrors("Not expecting Shipment Packing Mode to have errors.", shipment.JS_PackingModeInfo);

			container.JC_ContainerMode = Constants.ContainerModes.BreakBulk;
			AssertEquals("Expecting Shipment Packing Mode to be BBK.", Constants.ContainerModes.BreakBulk, shipment.JS_PackingMode);
			AssertNoErrors("Not expecting Shipment Packing Mode to have errors.", shipment.JS_PackingModeInfo);

			container.JC_ContainerMode = Constants.ContainerModes.AIR;
			AssertEquals("Container: AIR. Expecting changing Shipment Packing Mode to LSE.", Constants.ContainerModes.Loose, shipment.JS_PackingMode);
			AssertNoErrors("Not expecting Shipment Packing Mode to have errors.", shipment.JS_PackingModeInfo);
		}

		#endregion

		#region TestSettingSailing

		public void TestSettingSailing()
		{
			CFSContainer container = (CFSContainer)GetNewContainer();
			AssertEquals("sailing empty by default", ZGuid.Empty, container.JC_JX);
			AssertEquals("loadlist empty by default", ZGuid.Empty, container.JC_JK);

			container.JC_JX = ExportSailing1.PK;
			AssertEquals("Container Rembers Sailing", ExportSailing1.PK, container.JC_JX);

			CFSLoadListConsol loadList = Factory.New<CFSLoadListConsol>();
			loadList.JK_TransportMode = Constants.TransportModes.Sea;
			loadList.Transports[0].JW_JX = ImportSailing1.PK;
			container.JC_JK = loadList.PK;

			ZArchitecture.Core.Testing.ExceptionReporterTestListener.Instance.Clear();
			container.JC_JX = ZGuid.NewZGuid();
			AssertEquals("Developer error when setting the sailing while attached to a loadlist", 1, ZArchitecture.Core.Testing.ExceptionReporterTestListener.Instance.Count);
			ZArchitecture.Core.Testing.ExceptionReporterTestListener.Instance.Clear();

			AssertEquals("Sailing defaults from LoadList.", loadList.Schedule.PK, container.JC_JX);

			container.JC_JX = ZGuid.Empty; // "Container sailing can be set to empty even when attached to a consol"

			AssertEquals("Sailing still defaults from LoadList.", loadList.Schedule.PK, container.JC_JX);

			container.JC_JK = ZGuid.Empty;
			AssertEquals("Sailing is empty", ZGuid.Empty, container.JC_JX);
		}

		#endregion

		#region IHaveServices Members

		public void TestDependentJobServiceParents()
		{
			CFSContainer container = Factory.New<CFSContainer>();
			container.PackUnpackShipments.AddNew();
			AssertEquals("Precondition - one shipment in PackUnpackShipments", 1, container.PackUnpackShipments.Count);
			AssertEquals("Precondition - number of items in DependentJobServiceParents should match PackUnpackShipments", container.PackUnpackShipments.Count, container.DependentServiceParents.Length);
			AssertEquals("Item in DependentJobServiceParents should be the JobDocsAndCartage of the PackUnpackShipment", container.PackUnpackShipments[0].DocsAndCartage.PK, container.DependentServiceParents[0].PK);
		}

		public void TestJobServices()
		{
			CFSContainer container = Factory.New<CFSContainer>();
			AssertNotNull("Container should contain a job services collection", container.Services);

			CFSService fumigation = container.Services.AddNew();
			fumigation.ES_ServiceCode = Constants.FreightServiceType.Codes.Fumigation;
			Factory.Save();
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			CFSContainer loadedContainer = newFactory.Load<CFSContainer>(container.PK);
			AssertEquals("Job services of container were not loaded correctly", 1, loadedContainer.Services.Count);

			AssertEquals("Precondition - LoadedContainer.Services.ReadOnly should be false.", false, container.Services.ReadOnly);
			container.SetReadOnlyIncludingChildren(true);
			AssertEquals("LoadedContainer.Services should be true (JobServices is registered editable).", true, container.Services.ReadOnly);
		}

		#endregion

		public void TestJC_DepartureTimeAfterDeletingTheDepartureLeg()
		{
			CFSContainer container = Factory.New<CFSContainer>();
			container.CFSDispatch.Delete();
			AssertNotNull("Ensure JC_DepartureTime dont blow up", container.JC_DepartureTime);
		}

		public void TestJC_ArrivalTimeAfterDeletingTheArrivalLeg()
		{
			CFSContainer container = Factory.New<CFSContainer>();
			container.CFSArrival.Delete();
			AssertNotNull("Ensure JC_ArrivalTime dont blow up", container.JC_ArrivalTime);
		}

		public void TestValidateArrivalDetails()
		{
			CFSDataRegistry.Instance.RequireTransportDetails.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			GlbBranch.CurrentBranch.OrgProxy.OH_IsUnpackDepot = true;
			CFSLoadListConsol consol = (CFSLoadListConsol)GetNewConsol();
			CFSContainer container = consol.Containers.AddNew();
			container.JC_ArrivalTime = ZDateTime.Now;
			container.RunPreSaveValidation();
			AssertContainerArrivalTransportDetailsRequired(container, true);

			container.JC_ArrivalTruckRegistration = "VWS-526";
			container.JC_ArrivalTruckDriversLicense = "DL3993827139";
			container.JC_ArrivalTransportPK = GetTransportCompany();
			container.JC_ArrivalTime = ZDateTime.Now;
			AssertContainerArrivalTransportDetailsRequired(container, false);

			container.JC_ArrivalTruckRegistration = "";
			container.JC_ArrivalTruckDriversLicense = "";
			container.JC_ArrivalTransportPK = ZGuid.Empty;
			container.JC_ArrivalTime = ZDateTime.Empty;
			container.RunPreSaveValidation();
			AssertContainerArrivalTransportDetailsRequired(container, false);

			CFSDataRegistry.Instance.RequireTransportDetails.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			container.JC_ArrivalTransportPK = GetTransportCompany();
			container.RunPreSaveValidation();
			AssertContainerArrivalTransportDetailsRequired(container, false);

			CFSDataRegistry.Instance.RequireTransportDetails.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			container.JC_ArrivalTransportPK = ZGuid.Empty;
			container.JC_ArrivalTime = ZDateTime.Now;
			container.RunPreSaveValidation();
			AssertContainerArrivalTransportDetailsRequired(container, true);

			container.JC_ArrivalTruckRegistration = "GRT-845";
			container.JC_ArrivalTruckDriversLicense = "DL9875411559";
			container.JC_ArrivalTransportPK = GetTransportCompany();
			container.JC_ArrivalTime = ZDateTime.Now;
			AssertContainerArrivalTransportDetailsRequired(container, false);
		}

		public void TestValidateDepartureDetails()
		{
			CFSDataRegistry.Instance.RequireTransportDetails.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			GlbBranch.CurrentBranch.OrgProxy.OH_IsUnpackDepot = true;
			CFSLoadListConsol consol = (CFSLoadListConsol)GetNewConsol();
			CFSContainer container = consol.Containers.AddNew();
			container.JC_DepartureTime = ZDateTime.Now;
			container.RunPreSaveValidation();
			AssertContainerDepartureTransportDetailsRequired(container, true);

			container.JC_DepartureTruckRegistration = "LKC-784";
			container.JC_DepartureTruckDriversLicense = "DL8541542537";
			container.JC_DepartureTransportPK = GetTransportCompany();
			container.JC_DepartureTime = ZDateTime.Now;
			AssertContainerDepartureTransportDetailsRequired(container, false);

			container.JC_DepartureTruckRegistration = "";
			container.JC_DepartureTruckDriversLicense = "";
			container.JC_DepartureTransportPK = ZGuid.Empty;
			container.JC_DepartureTime = ZDateTime.Empty;
			container.RunPreSaveValidation();
			AssertContainerDepartureTransportDetailsRequired(container, false);

			CFSDataRegistry.Instance.RequireTransportDetails.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			container.RunPreSaveValidation();
			AssertContainerDepartureTransportDetailsRequired(container, false);
			container.JC_DepartureTime = ZDateTime.Now;
			AssertContainerDepartureTransportDetailsRequired(container, false);

			CFSDataRegistry.Instance.RequireTransportDetails.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			container.RunPreSaveValidation();
			AssertContainerDepartureTransportDetailsRequired(container, true);
		}

		protected void AssertContainerDepartureTransportDetailsRequired(CFSContainer container, bool required)
		{
			AssertEquals("JC_DepartureTruckRegistration", required, container.JC_DepartureTruckRegistrationInfo.HasErrors());
			AssertEquals("JC_DepartureTruckDriversLicense", required, container.JC_DepartureTruckDriversLicenseInfo.HasErrors());
			AssertEquals("JC_OH_DepartureTransport", required, container.JC_OA_DepartureTransportAddressInfo.HasErrors());
		}

		protected void AssertContainerArrivalTransportDetailsRequired(CFSContainer container, bool required)
		{
			AssertEquals("JC_ArrivalTruckRegistration", required, container.JC_ArrivalTruckRegistrationInfo.HasErrors());
			AssertEquals("JC_ArrivalTruckDriversLicense", required, container.JC_ArrivalTruckDriversLicenseInfo.HasErrors());
			AssertEquals("JC_OH_ArrivalTransport", required, container.JC_OA_ArrivalTransportAddressInfo.HasErrors());
		}

		ZGuid GetTransportCompany()
		{
			OrgHeader result = Factory.New<OrgHeader>();
			result.OH_FullName = "Transport Company";
			result.MainAddress.OA_Address1 = "Test Address One";
			result.OH_RL_NKClosestPort = HomePort;
			result.OH_IsMiscFreightServices = true;
			return result.PK;
		}

		public void TestNonStaffArrivalDriver()
		{
			CFSLoadListConsol consol = (CFSLoadListConsol)GetNewConsol();
			CFSContainer container = consol.Containers.AddNew();
			container.JC_ArrivalTruckDrivers = "Reza";
			AssertEquals("DriversName of Arrival Leg (FYD)", "Reza", container.CFSArrival.EU_DriversName);
		}

		public void TestNonStaffDepartureDriver()
		{
			CFSLoadListConsol consol = (CFSLoadListConsol)GetNewConsol();
			CFSContainer container = consol.Containers.AddNew();
			container.JC_DepartureTruckDrivers = "Reza";
			AssertEquals("DriversName of Arrival Leg (FDC)", "Reza", container.CFSDispatch.EU_DriversName);
		}

		public void TestJC_LCLStorageCommences_DoesntComeFromSailingSchedule()
		{
			TestJC_LCLAvailableStorage_DoesntComeFromSailingSchedule("JW_DepotStorageDate", JobContainerSchema.JC_LCLStorageCommences.Name);
		}

		public void TestJC_LCLAvailable_DoesntComeFromSailingSchedule()
		{
			TestJC_LCLAvailableStorage_DoesntComeFromSailingSchedule("JW_DepotAvailabilityDate", JobContainerSchema.JC_LCLAvailable.Name);
		}

		void TestJC_LCLAvailableStorage_DoesntComeFromSailingSchedule(string transportAvailableStorageProperty, string containerAvailableStorageProperty)
		{
			Transport arrivalTransport = CFSLoadListConsol.Transports.AddNew();
			arrivalTransport.JW_IsLinked = true;
			arrivalTransport.JW_RL_NKLoadPort = "MYPKG";
			arrivalTransport.JW_RL_NKDiscPort = "AUSYD";
			arrivalTransport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			arrivalTransport.JW_Vessel = "Vessel";
			arrivalTransport.JW_VoyageFlight = "Voyage";

			arrivalTransport[transportAvailableStorageProperty] = new ZDateTime(2005, 1, 1);
			CFSLoadListConsol.JK_RL_NKDischargePort = "AUSYD";
			AssertEquals("Consol.ArrivalTransport for the test", arrivalTransport, CFSLoadListConsol.Transports.ArrivalTransport);

			CFSContainer container = CFSLoadListConsol.Containers.AddNew();
			AssertEquals("CFS Available date must not come from sailing schedule", ZDateTime.Empty, container[containerAvailableStorageProperty]);
			container[containerAvailableStorageProperty] = new ZDateTime(2005, 2, 2);
			AssertEquals("CFS Available date must come from the container itself", new ZDateTime(2005, 2, 2), container[containerAvailableStorageProperty]);
		}

		#region TestLCLAvailableUpdatesShipments

		public void TestLCLAvailableUpdatesShipments()
		{
			ZDateTime now = ZDateTime.Now;

			CFSLoadListConsol consol = Factory.New<CFSLoadListConsol>();
			CFSContainer container = consol.Containers.AddNew();
			CFSShipment shipment = consol.Shipments.AddNew();
			CFSPackLine packline = shipment.OuterPackLines.AddNew();

			packline.SetContainer(container.PK);

			AssertEquals("precondition: LCL availability should not be set yet", ZDateTime.Empty, shipment.DocsAndCartage.JP_LCLAvailable);

			container.JC_LCLAvailable = now;
			AssertEquals("precondition: LCL availability should now be set", now, shipment.DocsAndCartage.JP_LCLAvailable);
		}

		#endregion

		#region TestAvailableDateDefaults

		public void TestAvailableDateDefaults()
		{
			CFSContainer.JC_LCLUnpack = ZDateTime.Now;
			AssertEquals("Available date should be 1 business day after today", CFSContainer.JC_LCLUnpack.AddDays(1), CFSContainer.JC_LCLAvailable);
		}

		#endregion

		#region TestStorageDateDefaults

		public void TestStorageDateDefaults()
		{
			Assert("Pre-condition: Expected registry to default to using Client Free Days", CFSDataRegistry.Instance.CFSSeaFreightUseClientFreeDays.Value);
			AssertEquals("Pre-condition: Expected registry to default to 3 for sea shipments", 3, EnvProxy.Instance.Registry.RawRegistry.CFSSeaFreightLCLStorageFreeDays.Value);

			ZDateTime now = ZDateTime.Now;
			CFSContainer.JC_LCLAvailable = now;
			AssertEquals("Storage date should be the current date client does not have free days", now.AddDays(3), CFSContainer.JC_LCLStorageCommences);

			var client = Factory.NewWithValidTestData<OrgHeader>();
			client.MiscServ.OM_IMSeaDepotFreeDays = 5;
			CFSContainer.JC_OH_CFSClient = client.PK;
			CFSContainer.JC_LCLStorageCommences = ZDateTime.Empty;
			CFSContainer.JC_LCLAvailable = now;

			AssertEquals("Storage date should be the current date client does not have free days", now.AddDays(5), CFSContainer.JC_LCLStorageCommences);

			CFSDataRegistry.Instance.CFSSeaFreightUseClientFreeDays.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			CFSContainer.JC_LCLStorageCommences = ZDateTime.Empty;
			CFSContainer.JC_LCLAvailable = now;

			AssertEquals("Storage date should be 3 business days after Available date", now.AddDays(3), CFSContainer.JC_LCLStorageCommences);
		}

		#endregion

		#region TestStorageDateDoesntDefaultWhenAlreadySet

		public void TestStorageDateDoesntDefaultWhenAlreadySet()
		{
			CFSContainer.JC_LCLStorageCommences = ZDateTime.Now.AddDays(10);
			CFSContainer.JC_LCLAvailable = ZDateTime.Now;
			AssertEquals("Storage date should be 10 days after NOW cause its already set", CFSContainer.JC_LCLStorageCommences, CFSContainer.JC_LCLStorageCommences);
		}

		#endregion

		#region TestDatesOnShipmentDefault

		public void TestDatesOnShipmentDefaultWithDangerousAndGeneralGoods()
		{
			CFSDataRegistry.Instance.CFSSeaFreightUseClientFreeDays.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

			CFSLoadListConsol consol = Factory.New<CFSLoadListConsol>();
			CFSShipment shipmentHaz = consol.Shipments.AddNew();
			CFSShipment shipmentGen = consol.Shipments.AddNew();

			PackLine packLineHaz = shipmentHaz.OuterPackLines.AddNew();
			PackLine packLineGen = shipmentGen.OuterPackLines.AddNew();

			UNDGSubstance dg = Factory.NewWithValidTestData<UNDGSubstance>();
			packLineHaz.UNDGs.AddNew().DI_DG = dg.PK;

			CFSContainer.PackUnpackShipments.Add(shipmentHaz);
			CFSContainer.PackUnpackShipments.Add(shipmentGen);

			ZDateTime unpackDate = ZDateTime.Now;

			CFSContainer.JC_LCLUnpack = unpackDate;
			AssertEquals("ShipmentHaz Available Date should be same date as unpack", unpackDate, shipmentHaz.DocsAndCartage.JP_LCLAvailable);
			AssertEquals("ShipmentGen Available Date should be following date", unpackDate.AddDays(1), shipmentGen.DocsAndCartage.JP_LCLAvailable);
			AssertEquals("ShipmentHaz Storage Commence Date should be same date as available", shipmentHaz.DocsAndCartage.JP_LCLAvailable, shipmentHaz.DocsAndCartage.JP_LCLStorageCommences);
			AssertEquals("ShipmentGen Storage Commence Date should default from the registry", shipmentGen.DocsAndCartage.JP_LCLAvailable.AddDays(3), shipmentGen.DocsAndCartage.JP_LCLStorageCommences);

			ZDateTime availableDate = unpackDate.AddDays(5);

			shipmentGen.DocsAndCartage.JP_LCLAvailable = ZDateTime.Empty;
			shipmentHaz.DocsAndCartage.JP_LCLAvailable = ZDateTime.Empty;
			CFSContainer.JC_LCLAvailable = availableDate;
			AssertEquals("Unpack date is untouched", unpackDate, CFSContainer.JC_LCLUnpack);
			AssertEquals("ShipmentHaz Available Date reset", availableDate, shipmentHaz.DocsAndCartage.JP_LCLAvailable);
			AssertEquals("ShipmentGen Available Date reset", availableDate, shipmentGen.DocsAndCartage.JP_LCLAvailable);
			AssertEquals("ShipmentHaz Storage Commence Date should be same date as available", availableDate, shipmentHaz.DocsAndCartage.JP_LCLStorageCommences);
			AssertEquals("ShipmentGen Storage Commence Date should default from the registry", availableDate.AddDays(3), shipmentGen.DocsAndCartage.JP_LCLStorageCommences);

			ZDateTime storageDate = unpackDate.AddDays(9);

			shipmentGen.DocsAndCartage.JP_LCLStorageCommences = ZDateTime.Empty;
			shipmentHaz.DocsAndCartage.JP_LCLStorageCommences = ZDateTime.Empty;
			CFSContainer.JC_LCLStorageCommences = storageDate;
			AssertEquals("Unpack date is untouched", unpackDate, CFSContainer.JC_LCLUnpack);
			AssertEquals("ShipmentHaz Available Date is untouched", availableDate, shipmentHaz.DocsAndCartage.JP_LCLAvailable);
			AssertEquals("ShipmentGen Available Date is untouched", availableDate, shipmentGen.DocsAndCartage.JP_LCLAvailable);
			AssertEquals("ShipmentHaz Storage Commence Date should be reset", storageDate, shipmentHaz.DocsAndCartage.JP_LCLStorageCommences);
			AssertEquals("ShipmentGen Storage Commence Date should be reset", storageDate, shipmentGen.DocsAndCartage.JP_LCLStorageCommences);
		}

		#endregion

		#region Test Defaulting Dates on Container and related Shipments

		public void TestDatesOnShipmentsAndContainerDefault()
		{
			CFSLoadListConsol consol = Factory.New<CFSLoadListConsol>();
			CFSShipment shipment1 = consol.Shipments.AddNew();
			CFSShipment shipment2 = consol.Shipments.AddNew();

			PackLine packLine1 = shipment1.OuterPackLines.AddNew();
			PackLine packLine2 = shipment2.OuterPackLines.AddNew();

			OrgHeader consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_IsConsignee = true;
			consignee.MiscServ.OM_IMSeaDepotFreeDays = 6;
			shipment2.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;

			CFSContainer.PackUnpackShipments.Add(shipment1);
			CFSContainer.PackUnpackShipments.Add(shipment2);

			ZDateTime unpackDate = new ZDateTime(2006, 08, 07);
			ZDateTime availableDate = new ZDateTime(2006, 08, 12);
			ZDateTime storageDate = new ZDateTime(2006, 08, 18);

			CFSContainer.JC_LCLUnpack = unpackDate;

			AssertUnpackDateSetsAllOtherDates(unpackDate, CFSContainer, shipment1, shipment2);

			shipment2.DocsAndCartage.JP_LCLStorageCommences = ZDateTime.Empty;
			shipment2.DocsAndCartage.JP_LCLAvailable = ZDateTime.Empty;
			CFSContainer.JC_LCLAvailable = availableDate;

			AssertSettingAvailableDate(unpackDate, availableDate, CFSContainer, shipment1, shipment2);

			AssertSettingStorageCommencesDate(unpackDate, availableDate, storageDate, CFSContainer, shipment1, shipment2);
		}

		void AssertUnpackDateSetsAllOtherDates(ZDateTime unpackDate, CFSContainer container, CFSShipment shipment1, CFSShipment shipment2)
		{
			AssertEquals("Pre-condition: CFS Sea registry should default 3 free days", 3, EnvProxy.Instance.Registry.RawRegistry.CFSSeaFreightLCLStorageFreeDays.Value);
			AssertEquals("Pre-condition: CFS Sea registry should default to client free days", true, CFSDataRegistry.Instance.CFSSeaFreightUseClientFreeDays.Value);

			AssertEquals("Container Available date should be unpack date + 1", unpackDate.AddDays(1), container.JC_LCLAvailable);
			AssertEquals("Shipment1 Available date should be unpack date + 1", unpackDate.AddDays(1), shipment1.DocsAndCartage.JP_LCLAvailable);
			AssertEquals("Shipment2 Available date should be unpack date + 1", unpackDate.AddDays(1), shipment2.DocsAndCartage.JP_LCLAvailable);
			AssertEquals("Container Storage date should be container available date + 3 from the registry", container.JC_LCLAvailable.AddDays(3), container.JC_LCLStorageCommences);
			AssertEquals("Shipment1 Storage date should be shipment available date + 3 from the registry", shipment1.DocsAndCartage.JP_LCLAvailable.AddDays(3), shipment1.DocsAndCartage.JP_LCLStorageCommences);
			AssertEquals("Shipment2 Storage date should be shipment available date + 6 from the client", shipment2.DocsAndCartage.JP_LCLAvailable.AddDays(6), shipment2.DocsAndCartage.JP_LCLStorageCommences);

			CFSDataRegistry.Instance.CFSSeaFreightUseClientFreeDays.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			container.JC_LCLUnpack = ZDateTime.Empty;
			container.JC_LCLUnpack = unpackDate;

			AssertEquals("Container Storage date should be container available date + 3 from the registry", container.JC_LCLAvailable.AddDays(3), container.JC_LCLStorageCommences);
			AssertEquals("Shipment1 Storage date should be shipment available date + 3 from the registry", shipment1.DocsAndCartage.JP_LCLAvailable.AddDays(3), shipment1.DocsAndCartage.JP_LCLStorageCommences);
			AssertEquals("Shipment2 Storage date should be shipment available date + 3 from the registry", shipment2.DocsAndCartage.JP_LCLAvailable.AddDays(3), shipment2.DocsAndCartage.JP_LCLStorageCommences);

			CFSDataRegistry.Instance.CFSSeaFreightUseClientFreeDays.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
		}

		void AssertSettingAvailableDate(ZDateTime unpackDate, ZDateTime availableDate, CFSContainer container, CFSShipment shipment1, CFSShipment shipment2)
		{
			AssertEquals("Container Unpack date is untouched", unpackDate, container.JC_PackUnpackDate);
			AssertEquals("Container Available date should be reset", availableDate, container.JC_LCLAvailable);
			AssertEquals("Shipment1 Available date is untouched", unpackDate.AddDays(1), shipment1.DocsAndCartage.JP_LCLAvailable);
			AssertEquals("Shipment2 Available date should be reset as it is empty", availableDate, shipment2.DocsAndCartage.JP_LCLAvailable);
			AssertEquals("Container Storage date is untouched as it already has a value", unpackDate.AddDays(4), container.JC_LCLStorageCommences);
			AssertEquals("Shipment1 Storage date is untouched as it already has a value", unpackDate.AddDays(4), shipment1.DocsAndCartage.JP_LCLStorageCommences);
			AssertEquals("Shipment2 Storage date should be reset to available date + 6 as there is a valid consignee", availableDate.AddDays(6), shipment2.DocsAndCartage.JP_LCLStorageCommences);

			container.JC_LCLStorageCommences = ZDateTime.Empty;
			container.JC_LCLAvailable = ZDateTime.Empty;
			container.JC_LCLAvailable = availableDate;

			AssertEquals("Container Storage date default from the registry as there is no client date", availableDate.AddDays(3), container.JC_LCLStorageCommences);
			AssertEquals("Shipment1 Storage date is untouched", unpackDate.AddDays(4), shipment1.DocsAndCartage.JP_LCLStorageCommences);
			AssertEquals("Shipment2 Storage date is untouched", availableDate.AddDays(6), shipment2.DocsAndCartage.JP_LCLStorageCommences);

			CFSDataRegistry.Instance.CFSSeaFreightUseClientFreeDays.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			shipment1.DocsAndCartage.JP_LCLAvailable = availableDate;

			AssertEquals("Container Available date is untouched", availableDate, container.JC_LCLAvailable);
			AssertEquals("Container Storage date is untouched", availableDate.AddDays(3), container.JC_LCLStorageCommences);
			AssertEquals("Shipment1 Available date should be reset", availableDate, shipment1.DocsAndCartage.JP_LCLAvailable);
			AssertEquals("Shipment1 Storage date should be shipment available date + 3 from the registry", availableDate.AddDays(3), shipment1.DocsAndCartage.JP_LCLStorageCommences);

			CFSDataRegistry.Instance.CFSSeaFreightUseClientFreeDays.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			shipment2.DocsAndCartage.JP_LCLAvailable = availableDate;

			AssertEquals("Shipment1 Storage date should be shipment available date + 6 from the client", availableDate.AddDays(6), shipment2.DocsAndCartage.JP_LCLStorageCommences);
		}

		void AssertSettingStorageCommencesDate(ZDateTime unpackDate, ZDateTime availableDate, ZDateTime storageDate, CFSContainer container, CFSShipment shipment1, CFSShipment shipment2)
		{
			shipment2.DocsAndCartage.JP_LCLStorageCommences = ZDateTime.Empty;
			CFSContainer.JC_LCLStorageCommences = storageDate;

			AssertEquals("Container Unpack date is untouched", unpackDate, CFSContainer.JC_PackUnpackDate);
			AssertEquals("Container Available date is untouched", availableDate, CFSContainer.JC_LCLAvailable);
			AssertEquals("Shipment1 Available date is untouched", availableDate, shipment1.DocsAndCartage.JP_LCLAvailable);
			AssertEquals("Shipment2 Available date is untouched", availableDate, shipment2.DocsAndCartage.JP_LCLAvailable);
			AssertEquals("Container Storage date be set to storageDate", storageDate, CFSContainer.JC_LCLStorageCommences);
			AssertEquals("Shipment1 Storage date is untouched", availableDate.AddDays(3), shipment1.DocsAndCartage.JP_LCLStorageCommences);
			AssertEquals("Shipment2 Storage date should be reset as it is empty", storageDate, shipment2.DocsAndCartage.JP_LCLStorageCommences);

			shipment1.DocsAndCartage.JP_LCLStorageCommences = storageDate;

			AssertEquals("Shipment1 Available date is untouched", availableDate, shipment1.DocsAndCartage.JP_LCLAvailable);
			AssertEquals("Shipment1 Storage date should be reset", storageDate, shipment1.DocsAndCartage.JP_LCLStorageCommences);
		}

		#endregion

		#region TestDatesOnShipmentsAndContainerDefaultWithAClientForAir

		public void TestDatesOnShipmentsAndContainerDefaultWithAClientForAir()
		{
			CFSContainer.JC_TransportMode = Core.Constants.TransportModes.Air;
			CFSLoadListConsol consol = Factory.New<CFSLoadListConsol>();
			CFSShipment shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = CFSContainer.JC_TransportMode;
			PackLine packLine = shipment.OuterPackLines.AddNew();

			CFSContainer.PackUnpackShipments.Add(shipment);

			OrgHeader client = Factory.New<OrgHeader>();
			client.OH_Code = "CUKSQK";
			client.MiscServ.OM_IMAirDepotFreeDays = 4;
			CFSContainer.JC_OH_CFSClient = client.PK;
			Factory.Save();

			CFSContainer.JC_LCLUnpack = new ZDateTime(2006, 08, 07);
			AssertEquals(CFSContainer.JC_LCLUnpack.AddDays(1), shipment.DocsAndCartage.JP_LCLAvailable);
			AssertEquals(CFSContainer.JC_LCLAvailable.AddDays(4), shipment.DocsAndCartage.JP_LCLStorageCommences);
			AssertEquals(CFSContainer.JC_LCLUnpack.AddDays(1), CFSContainer.JC_LCLAvailable);
			AssertEquals(CFSContainer.JC_LCLAvailable.AddDays(4), CFSContainer.JC_LCLStorageCommences);
		}

		#endregion

		#region TestDatesOnShipmentsAndContainerDefaultWithAClientForSea

		public void TestDatesOnShipmentsAndContainerDefaultWithAClientForSea()
		{
			CFSContainer.JC_TransportMode = Core.Constants.TransportModes.Sea;
			CFSLoadListConsol consol = Factory.New<CFSLoadListConsol>();
			CFSShipment shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = CFSContainer.JC_TransportMode;
			PackLine packLine = shipment.OuterPackLines.AddNew();

			CFSContainer.PackUnpackShipments.Add(shipment);

			OrgHeader client = Factory.New<OrgHeader>();
			client.OH_Code = "CUKSQK";
			client.MiscServ.OM_IMSeaDepotFreeDays = 2;
			CFSContainer.JC_OH_CFSClient = client.PK;
			Factory.Save();

			CFSContainer.JC_LCLUnpack = new ZDateTime(2006, 08, 07);
			AssertEquals(CFSContainer.JC_LCLUnpack.AddDays(1), shipment.DocsAndCartage.JP_LCLAvailable);
			AssertEquals(CFSContainer.JC_LCLAvailable.AddDays(2), shipment.DocsAndCartage.JP_LCLStorageCommences);
			AssertEquals(CFSContainer.JC_LCLUnpack.AddDays(1), CFSContainer.JC_LCLAvailable);
			AssertEquals(CFSContainer.JC_LCLAvailable.AddDays(2), CFSContainer.JC_LCLStorageCommences);
		}

		#endregion

		#region Container Event Dates are updated from Gate In/Out/Dehire Events

		[TestDate(2014, 09, 09)]
		public void TestContainerFieldsChangesAddGateInEvents()
		{
			var today = ZDateTime.Today;
			var container = ContainerWithImportLoadList;
			container.JC_FCLWharfGateIn = today;

			var logs = container.Logs;
			var gateInLog = logs.MostRecentLogByEventTime(Events.GateIn);

			AssertNotNull("Expected event to have been created by updating the container date", gateInLog);
			AssertEquals(container.JC_FCLWharfGateIn, gateInLog.SL_EventTime);
			AssertEquals(EventConstants.Facilities.Code.Terminal, gateInLog.Parameters[EventConstants.EventReferenceParameters.Codes.Facility]);
			AssertEquals("CNSHA", gateInLog.Parameters[EventConstants.EventReferenceParameters.Codes.Location]);

			container.JC_ContainerYardEmptyReturnGateIn = today.AddDays(2);
			gateInLog = logs.MostRecentLogByEventTime(Events.GateIn);

			AssertEquals(container.JC_ContainerYardEmptyReturnGateIn, gateInLog.SL_EventTime);
			AssertEquals(EventConstants.Facilities.Code.ContainerYard, gateInLog.Parameters[EventConstants.EventReferenceParameters.Codes.Facility]);
			AssertEquals("Location should be consol's discharge port",
				"AUSYD", gateInLog.Parameters[EventConstants.EventReferenceParameters.Codes.Location]);

			var containerPark = Factory.NewWithValidTestData<OrgHeader>();
			containerPark.OH_IsContainerYard = true;
			containerPark.MainAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			container.JC_OA_ArrivalContainerYardAddress = containerPark.MainAddress.PK;

			container.JC_ContainerYardEmptyReturnGateIn = today.AddDays(3);
			gateInLog = logs.MostRecentLogByEventTime(Events.GateIn);

			AssertEquals(container.JC_ContainerYardEmptyReturnGateIn, gateInLog.SL_EventTime);
			AssertEquals("CY event should prefer the container park address",
				"AUBNE", gateInLog.Parameters[EventConstants.EventReferenceParameters.Codes.Location]);

			container.JC_FCLWharfGateIn = today.AddDays(4);
			gateInLog = logs.MostRecentLogByEventTime(Events.GateIn);

			AssertEquals(container.JC_FCLWharfGateIn, gateInLog.SL_EventTime);
			AssertEquals("Wharf Gate In event always match the consol's discharge load port",
				"CNSHA", gateInLog.Parameters[EventConstants.EventReferenceParameters.Codes.Location]);
		}

		[TestDate(2014, 09, 09)]
		public void TestContainerFieldsChangesAddGateOutEvents()
		{
			var today = ZDateTime.Today;
			var container = ContainerWithImportLoadList;
			container.JC_FCLWharfGateOut = today;

			var logs = container.Logs;
			var gateOutLog = logs.MostRecentLogByEventTime(Events.GateOut);

			AssertNotNull("Expected event to have been created by updating the container date", gateOutLog);
			AssertEquals(container.JC_FCLWharfGateOut, gateOutLog.SL_EventTime);
			AssertEquals(EventConstants.Facilities.Code.Terminal, gateOutLog.Parameters[EventConstants.EventReferenceParameters.Codes.Facility]);
			AssertEquals("AUSYD", gateOutLog.Parameters[EventConstants.EventReferenceParameters.Codes.Location]);

			container.JC_ContainerYardEmptyPickupGateOut = today.AddDays(2);
			gateOutLog = logs.MostRecentLogByEventTime(Events.GateOut);

			AssertEquals(container.JC_ContainerYardEmptyPickupGateOut, gateOutLog.SL_EventTime);
			AssertEquals(EventConstants.Facilities.Code.ContainerYard, gateOutLog.Parameters[EventConstants.EventReferenceParameters.Codes.Facility]);
			AssertEquals("Location should be consol's load port",
				"CNSHA", gateOutLog.Parameters[EventConstants.EventReferenceParameters.Codes.Location]);

			var containerPark = Factory.NewWithValidTestData<OrgHeader>();
			containerPark.OH_IsContainerYard = true;
			containerPark.MainAddress.OA_RL_NKRelatedPortCode = "CNCAN";
			container.JC_OA_DepartureContainerYardAddress = containerPark.MainAddress.PK;

			container.JC_ContainerYardEmptyPickupGateOut = today.AddDays(3);
			gateOutLog = logs.MostRecentLogByEventTime(Events.GateOut);

			AssertEquals(container.JC_ContainerYardEmptyPickupGateOut, gateOutLog.SL_EventTime);
			AssertEquals("CY event should prefer the container park address",
				"CNCAN", gateOutLog.Parameters[EventConstants.EventReferenceParameters.Codes.Location]);

			container.JC_FCLWharfGateOut = today.AddDays(4);
			gateOutLog = logs.MostRecentLogByEventTime(Events.GateOut);

			AssertEquals(container.JC_FCLWharfGateOut, gateOutLog.SL_EventTime);
			AssertEquals("Wharf Gate Out event always match the consol's first load port",
				"AUSYD", gateOutLog.Parameters[EventConstants.EventReferenceParameters.Codes.Location]);
		}

		[TestDate(2014, 09, 09)]
		public void TestContainerFieldsUpdatedFromGateInEvents()
		{
			var today = ZDateTime.Today;
			var container = ContainerWithImportLoadList;

			var log = container.Logs.AddNew(Events.GateIn);
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_EventTime = today;
				log.SL_Reference = "";
			}

			Assert("Expected no date change without relevant parameter information", container.JC_FCLWharfGateIn.IsEmpty);
			Assert("Expected no date change without relevant parameter information", container.JC_ContainerYardEmptyReturnGateIn.IsEmpty);

			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_EventTime = today.AddHours(4);
				log.Parameters[EventConstants.EventReferenceParameters.Codes.Facility] = EventConstants.Facilities.Code.Terminal;
				log.Parameters[EventConstants.EventReferenceParameters.Codes.Location] = "CNSHA";
			}

			AssertEquals("Expected GateIn event for a wharf to update the container event date", today.AddHours(4), container.JC_FCLWharfGateIn);
			Assert("Expected no change as the facility is not applicable", container.JC_ContainerYardEmptyReturnGateIn.IsEmpty);

			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_EventTime = today.AddDays(1);
				log.Parameters[EventConstants.EventReferenceParameters.Codes.Facility] = EventConstants.Facilities.Code.ContainerYard;
				log.Parameters[EventConstants.EventReferenceParameters.Codes.Location] = "AUSYD";
			}

			AssertEquals(today.AddDays(1), container.JC_ContainerYardEmptyReturnGateIn);
			AssertEquals("Wharf event should remain unchanged", today.AddHours(4), container.JC_FCLWharfGateIn);

			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_EventTime = today.AddDays(2);
				log.Parameters[EventConstants.EventReferenceParameters.Codes.Facility] = EventConstants.Facilities.Code.ContainerYard;
				log.Parameters[EventConstants.EventReferenceParameters.Codes.Location] = "";
			}

			AssertEquals("Container yard should be updatable with an empty location", today.AddDays(2), container.JC_ContainerYardEmptyReturnGateIn);

			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_EventTime = today.AddDays(3);
				log.Parameters[EventConstants.EventReferenceParameters.Codes.Facility] = EventConstants.Facilities.Code.Terminal;
				log.Parameters[EventConstants.EventReferenceParameters.Codes.Location] = "CNSHA";
			}

			AssertEquals(today.AddDays(3), container.JC_FCLWharfGateIn);
			AssertEquals(today.AddDays(2), container.JC_ContainerYardEmptyReturnGateIn);

			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_EventTime = today.AddDays(5);
				log.Parameters[EventConstants.EventReferenceParameters.Codes.Facility] = EventConstants.Facilities.Code.ContainerYard;
				log.Parameters[EventConstants.EventReferenceParameters.Codes.Location] = "GBLON";
			}

			AssertEquals("Expected no change for unrelated location", today.AddDays(2), container.JC_ContainerYardEmptyReturnGateIn);

			var gateOutLog = container.Logs.AddNew(Events.GateOut);
			using (gateOutLog.LockForUpdatingKeyFieldsForTesting())
			{
				gateOutLog.SL_EventTime = today.AddDays(5);
				gateOutLog.Parameters[EventConstants.EventReferenceParameters.Codes.Facility] = EventConstants.Facilities.Code.ContainerYard;
				gateOutLog.Parameters[EventConstants.EventReferenceParameters.Codes.Location] = "AUSYD";
			}

			using (gateOutLog.LockForUpdatingKeyFieldsForTesting())
			{
				gateOutLog.SL_EventTime = today.AddDays(5);
				gateOutLog.Parameters[EventConstants.EventReferenceParameters.Codes.Facility] = EventConstants.Facilities.Code.Terminal;
				gateOutLog.Parameters[EventConstants.EventReferenceParameters.Codes.Location] = "CNSHA";
			}

			AssertEquals("Expected Gate Out event not to change Gate In date", today.AddDays(3), container.JC_FCLWharfGateIn);
			AssertEquals("Expected Gate Out event not to change Gate In date", today.AddDays(2), container.JC_ContainerYardEmptyReturnGateIn);
		}

		[TestDate(2014, 09, 09)]
		public void TestContainerFieldsUpdatedFromGateOutEvents()
		{
			var today = ZDateTime.Today;
			var container = ContainerWithImportLoadList;

			var log = container.Logs.AddNew(Events.GateOut);
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_EventTime = today;
				log.SL_Reference = "";
			}

			Assert("Expected no date change without relevant parameter information", container.JC_FCLWharfGateOut.IsEmpty);
			Assert("Expected no date change without relevant parameter information", container.JC_ContainerYardEmptyPickupGateOut.IsEmpty);

			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_EventTime = today.AddHours(6);
				log.Parameters[EventConstants.EventReferenceParameters.Codes.Facility] = EventConstants.Facilities.Code.Terminal;
				log.Parameters[EventConstants.EventReferenceParameters.Codes.Location] = "AUSYD";
			}

			AssertEquals("Expected GateOut event for a wharf to update the container event date", today.AddHours(6), container.JC_FCLWharfGateOut);
			Assert("Expected no change as the facility is not applicable", container.JC_ContainerYardEmptyPickupGateOut.IsEmpty);

			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_EventTime = today.AddDays(1);
				log.Parameters[EventConstants.EventReferenceParameters.Codes.Facility] = EventConstants.Facilities.Code.ContainerYard;
				log.Parameters[EventConstants.EventReferenceParameters.Codes.Location] = "CNSHA";
			}

			AssertEquals("Expected date change to now apply", today.AddDays(1), container.JC_ContainerYardEmptyPickupGateOut);
			AssertEquals("Wharf event should remain unchanged", today.AddHours(6), container.JC_FCLWharfGateOut);

			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_EventTime = today.AddDays(2);
				log.Parameters[EventConstants.EventReferenceParameters.Codes.Facility] = EventConstants.Facilities.Code.ContainerYard;
				log.Parameters[EventConstants.EventReferenceParameters.Codes.Location] = "";
			}

			AssertEquals("Container yard should be updatable with an empty location", today.AddDays(2), container.JC_ContainerYardEmptyPickupGateOut);

			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_EventTime = today.AddDays(4);
				log.Parameters[EventConstants.EventReferenceParameters.Codes.Facility] = EventConstants.Facilities.Code.Terminal;
				log.Parameters[EventConstants.EventReferenceParameters.Codes.Location] = "AUSYD";
			}

			AssertEquals("Expected wharf date to be updated", today.AddDays(4), container.JC_FCLWharfGateOut);

			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_EventTime = today.AddDays(5);
				log.Parameters[EventConstants.EventReferenceParameters.Codes.Facility] = EventConstants.Facilities.Code.ContainerYard;
				log.Parameters[EventConstants.EventReferenceParameters.Codes.Location] = "USLAX";
			}

			AssertEquals("GOU event wouldn't check location deciding to update JC_ContainerYardEmptyPickupGateOut", today.AddDays(5), container.JC_ContainerYardEmptyPickupGateOut);

			var gateInLog = container.Logs.AddNew(Events.GateIn);
			using (gateInLog.LockForUpdatingKeyFieldsForTesting())
			{
				gateInLog.SL_EventTime = today.AddDays(10);
				gateInLog.Parameters[EventConstants.EventReferenceParameters.Codes.Facility] = EventConstants.Facilities.Code.ContainerYard;
				gateInLog.Parameters[EventConstants.EventReferenceParameters.Codes.Location] = "";
			}

			using (gateInLog.LockForUpdatingKeyFieldsForTesting())
			{
				gateInLog.SL_EventTime = today.AddDays(10);
				gateInLog.Parameters[EventConstants.EventReferenceParameters.Codes.Facility] = EventConstants.Facilities.Code.Terminal;
				gateInLog.Parameters[EventConstants.EventReferenceParameters.Codes.Location] = "CNSHA";
			}

			AssertEquals("Expected Gate In event not to change Gate Out date", today.AddDays(5), container.JC_ContainerYardEmptyPickupGateOut);
			AssertEquals("Expected Gate In event not to change Gate Out date", today.AddDays(4), container.JC_FCLWharfGateOut);
		}

		[TestDate(2014, 09, 09)]
		public void TestGateInContainerYardCreatesAndUpdatesDehireEvents()
		{
			var arrivalContainerYard = Factory.NewWithValidTestData<OrgHeader>();
			var arrivalAddress = arrivalContainerYard.MainAddress;
			arrivalAddress.OA_RL_NKRelatedPortCode = "CNCAN";

			var today = ZDateTime.Today;
			var container = ContainerWithImportLoadList;
			container.JC_OA_ArrivalContainerYardAddress = arrivalAddress.PK;
			container.JC_ContainerYardEmptyReturnGateIn = today;

			var logs = container.Logs;
			var dehireLog = logs.MostRecentLogByEventTime(Events.Dehire);

			AssertNotNull("logs should have been created", dehireLog);
			AssertEquals(container.JC_ContainerYardEmptyReturnGateIn, dehireLog.SL_EventTime);
			AssertEquals("Expected event to prefer container park address over consol dischange port",
				"CNCAN", dehireLog.Parameters[EventConstants.EventReferenceParameters.Codes.Location]);
			AssertEquals(EventConstants.Facilities.Code.ContainerYard,
				dehireLog.Parameters[EventConstants.EventReferenceParameters.Codes.Facility]);

			var gateInLog = container.Logs.AddNew(Events.GateIn);
			using (gateInLog.LockForUpdatingKeyFieldsForTesting())
			{
				gateInLog.SL_EventTime = today.AddDays(2);
				gateInLog.Parameters[EventConstants.EventReferenceParameters.Codes.Facility] = EventConstants.Facilities.Code.ContainerYard;
				gateInLog.Parameters[EventConstants.EventReferenceParameters.Codes.Location] = "";
			}

			AssertEquals("Field should be updated from Gate In event", today.AddDays(2), container.JC_ContainerYardEmptyReturnGateIn);

			dehireLog = logs.MostRecentLogByEventTime(Events.Dehire);

			AssertEquals("The dehire event should have been updated by Gate In CY changing the container field",
				container.JC_ContainerYardEmptyReturnGateIn,
				dehireLog.SL_EventTime);

			using (gateInLog.LockForUpdatingKeyFieldsForTesting())
			{
				gateInLog.SL_EventTime = today.AddDays(3);
				gateInLog.Parameters[EventConstants.EventReferenceParameters.Codes.Facility] = EventConstants.Facilities.Code.ContainerYard;
				gateInLog.Parameters[EventConstants.EventReferenceParameters.Codes.Location] = "";
			}

			AssertEquals(today.AddDays(3), container.JC_ContainerYardEmptyReturnGateIn);

			dehireLog = logs.MostRecentLogByEventTime(Events.Dehire);
			AssertEquals(container.JC_ContainerYardEmptyReturnGateIn, dehireLog.SL_EventTime);
		}

		[TestDate(2014, 09, 09)]
		public void TestDehireEvents()
		{
			var today = ZDateTime.Today;
			var container = ContainerWithImportLoadList;

			var dehireLog = container.Logs.AddNew(Events.Dehire);
			using (dehireLog.LockForUpdatingKeyFieldsForTesting())
			{
				dehireLog.SL_EventTime = today;
			}

			AssertEquals("Date should be updated from Dehire event without any parameters", today, container.JC_ContainerYardEmptyReturnGateIn);

			using (dehireLog.LockForUpdatingKeyFieldsForTesting())
			{
				dehireLog.SL_EventTime = today.AddDays(3);
				dehireLog.Parameters[EventConstants.EventReferenceParameters.Codes.Facility] = EventConstants.Facilities.Code.ContainerYard;
				dehireLog.Parameters[EventConstants.EventReferenceParameters.Codes.Location] = "GBLON";
			}

			AssertEquals("Date should be updated regardless of parameters", today.AddDays(3), container.JC_ContainerYardEmptyReturnGateIn);

			container.JC_ContainerYardEmptyReturnGateIn = today.AddDays(5);

			dehireLog = container.Logs.MostRecentLogByEventTime(Events.Dehire);

			AssertEquals("Log with a new event time should have been created", today.AddDays(5), dehireLog.SL_EventTime);
		}

		#endregion

		#region TestSetJY_FCLStorageCommences

		[TestDate(2006, 1, 1)]
		public void TestSetJY_FCLStorageCommences()
		{
			CFSContainer cFSContainer = Factory.New<CFSContainer>();
			cFSContainer.JC_Purpose = ContainerPurposeTypeCodeDescriptionPairList.Codes.CFS;
			cFSContainer.JC_ArrivalCTOStorageStartDate = ZDateTime.Now;
			AssertEquals(ZDateTime.Empty, cFSContainer.JC_ArrivalTime);
			cFSContainer.JC_Purpose = ContainerPurposeTypeCodeDescriptionPairList.Codes.Storage;
			cFSContainer.JC_ArrivalCTOStorageStartDate = ZDateTime.Now.AddDays(1);
			AssertEquals(new ZDateTime(2006, 1, 2), cFSContainer.JC_ArrivalTime);
		}

		#endregion

		#region TestJY_LCLUnpack

		public void TestJY_LCLUnpack()
		{
			CFSContainer container = Factory.New<CFSContainer>();

			ZDateTime validDateTime = new ZDateTime(2003, 11, 21);
			container.JC_LCLUnpack = validDateTime;
			AssertEquals("Valid ZDateTime on JY_LCLUnpack", validDateTime, container.JC_LCLUnpack);

			ZDateTime invalidDateTime = ZDateTime.Invalid;
			container.JC_LCLUnpack = invalidDateTime;
			AssertEquals("Valid ZDateTime on  JY_LCLUnpack", invalidDateTime, container.JC_LCLUnpack);

			ZDateTime maxSmallDateTime = ZDateTime.MaxSmallDateTime.AddMonths(-1);
			container.JC_LCLUnpack = maxSmallDateTime;
			AssertEquals("Valid ZDateTime on  JY_LCLUnpack", maxSmallDateTime, container.JC_LCLUnpack);

			ZDateTime minDateTime = new ZDateTime(DateTime.MinValue);
			container.JC_LCLUnpack = minDateTime;
			AssertEquals("Valid ZDateTime on  JY_LCLUnpack", minDateTime, container.JC_LCLUnpack);
			container.JC_LCLUnpack = new ZDateTime(2003, 11, 21);
			container.JC_LCLUnpack = new ZDateTime(2003, 11, 22);
			container.JC_LCLUnpack = ZDateTime.Empty;
			container.JC_LCLUnpack = ZDateTime.Empty;
		}

		#endregion

		#region TestSetJC_LCLUnpackDoesNotSetShipments

		public void TestSetJC_LCLUnpackDoesNotSetShipmentsJS_A_RCVDate()
		{
			CFSContainer container = Factory.New<CFSContainer>();

			PackUnpackLoadListConsol loadList = Factory.New<PackUnpackLoadListConsol>();
			loadList.AutomaticallyUpdatePackLineContainers = true;
			loadList.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;

			Transport transport = loadList.Transports[0];
			transport.JW_JX = ImportSailing1.PK;
			loadList.JK_RL_NKLoadPort = transport.JW_RL_NKLoadPort;
			loadList.JK_RL_NKDischargePort = transport.JW_RL_NKDiscPort;

			CFSShipment shipment = loadList.Shipments.AddNew();
			shipment.JS_RL_NKOrigin = OverseasPort2;
			shipment.JS_RL_NKDestination = HomePort;
			PackLine pack = shipment.OuterPackLines.AddNew();
			loadList.Containers.Add(container);
			container.PackLines.Add(pack);

			AssertEquals("Pre-requisite: shipment's JS_A_RCV should be empty", ZDateTime.Empty, shipment.JS_A_RCV);
			container.JC_LCLUnpack = new ZDateTime(2016, 07, 06);
			AssertEquals("The shipment's JS_A_RCV should not be auto-updated when container's unpack date is changed", ZDateTime.Empty, shipment.JS_A_RCV);
		}

		#endregion

		protected ZDateTime TestTime;

		protected override void SetUp()
		{
			base.SetUp();
			TestTime = ZDateTime.Now.AddDays(10);
			new ConstantsAndReusables(Factory).EnsureCurrentCompanyMatchesCurrentBranch();
		}

		#region Implementation

		CFSContainer CFSContainer
		{
			get
			{
				if (fCFSContainer == null)
				{
					fCFSContainer = (CFSContainer)GetNewContainer();
				}
				return fCFSContainer;
			}
		}
		CFSContainer fCFSContainer;

		CFSLoadListConsol CFSLoadListConsol
		{
			get
			{
				if (fCFSLoadListConsol == null)
				{
					fCFSLoadListConsol = (CFSLoadListConsol)GetNewConsol();
				}
				return fCFSLoadListConsol;
			}
		}
		CFSLoadListConsol fCFSLoadListConsol;

		protected override CommonConsol GetNewConsol()
		{
			return Factory.New<CFSLoadListConsol>();
		}

		protected override CommonContainer GetNewContainer()
		{
			return Factory.New<CFSContainer>();
		}

		protected override bool CouldLoadRelatedDeclaration
		{
			get { return false; }
		}

		CFSContainer ContainerWithImportLoadList
		{
			get
			{
				CFSLoadListConsol.JK_RL_NKLoadPort = "CNSHA";
				CFSLoadListConsol.JK_RL_NKDischargePort = "AUSYD";

				return CFSLoadListConsol.Containers.AddNew();
			}
		}

		#endregion
	}
}
