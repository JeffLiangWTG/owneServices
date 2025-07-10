using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.Business;
using Enterprise.Packing.Business;
using Enterprise.TransportBookings.Business.Testing;
using Enterprise.UniversalDataBuss.Integration.DataObjects;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing
{
	public abstract class IntegrationTestCaseWithFactory : TestCaseWithFactory
	{
		#region Create

		#region Create AirCargoReport

		protected CusMAWB CreateAirCargoReport(WhsWarehouse warehouse, string reference)
		{
			var cargoReport = Factory.NewWithValidTestData<CusMAWB>();
			cargoReport.CM_OA_UnpackDepotAddress = warehouse.WarehouseAddress.PK;
			cargoReport.CM_MAWB = reference;

			return cargoReport;
		}

		#endregion

		#region Create AirCargoHouse

		protected CusHAWB CreateAirCargoHouse(CusMAWB cargoReport, string housebill, string masterbill, string origin, string destination, short pieces, OrgHeader consignor, OrgHeader consignee, string goodsDescription)
		{
			var cargoHouse = Factory.NewWithValidTestData<CusHAWB>();
			cargoHouse.CS_HAWB = housebill;
			cargoHouse.CS_MasterHouseBill = masterbill;
			cargoHouse.CS_RL_NKOrigin = origin;
			cargoHouse.CS_RL_NKDestination = destination;
			cargoHouse.CS_PiecesManifested = pieces;
			cargoHouse.CS_PiecesLanded = 0;
			cargoHouse.CS_CM = cargoReport.PK;
			cargoHouse.CS_GoodsDescription = goodsDescription;
			cargoHouse.CS_OH_Consignor = consignor.PK;
			cargoHouse.CS_OH_Consignee = consignee.PK;

			return cargoHouse;
		}

		#endregion

		#region CreateUnderbond

		protected CusUnderbond CreateUnderbond(CusMAWB mawb, string referenceNumber)
		{
			var underbond = Factory.NewWithValidTestData<CusUnderbond>();
			underbond.C4_ParentID = mawb.PK;
			underbond.C4_ParentTableCode = "CM";
			underbond.C4_SendersMessageReference = referenceNumber;
			underbond.C4_MovementReason = "DCL";
			underbond.C4_ModeOfMovement = "ROA";

			return underbond;
		}

		protected CusUnderbond CreateUnderbond(CusHAWB hawb, string referenceNumber)
		{
			var underbond = Factory.NewWithValidTestData<CusUnderbond>();
			underbond.C4_ParentID = hawb.PK;
			underbond.C4_ParentTableCode = "CS";
			underbond.C4_SendersMessageReference = referenceNumber;
			underbond.C4_MovementReason = "CTO";
			underbond.C4_ModeOfMovement = "ROA";

			return underbond;
		}

		protected CusUnderbond CreateUnderbond(string mawb, string referenceNumber)
		{
			var underbond = Factory.NewWithValidTestData<CusUnderbond>();
			underbond.C4_MAWB = mawb;
			underbond.C4_SendersMessageReference = referenceNumber;
			underbond.C4_MovementReason = "CTO";
			underbond.C4_ModeOfMovement = "ROA";

			return underbond;
		}

		#endregion

		#region Create Shipment

		protected ForwardingShipment CreateConsolidatedShipment(ForwardingConsol consol, ZString houseBill, ZString origin, ZString destination,
			ZDateTime etd, ZDateTime eta, OrgHeader consignor, OrgHeader consignee,
			ForwardingShipment[] childShipments, string shipmentType = Constants.ShipmentTypes.AssemblyMaster)
		{
			var shipment = CreateShipment(consol, houseBill, origin, destination, etd, eta, consignor, consignee, shipmentType);
			shipment.CoLoadShipments.AddRange(childShipments);

			return shipment;
		}

		protected ForwardingShipment CreateConsolidatedShipment(ZString houseBill, ZString origin, ZString destination,
	ZDateTime etd, ZDateTime eta, OrgHeader consignor, OrgHeader consignee,
	ForwardingShipment[] childShipments, string shipmentType = Constants.ShipmentTypes.AssemblyMaster)
		{
			var shipment = CreateShipment(houseBill, origin, destination, etd, eta, consignor, consignee, shipmentType);
			shipment.CoLoadShipments.AddRange(childShipments);

			return shipment;
		}

		protected ForwardingShipment CreateShipment(
			ForwardingConsol consol,
			ZString houseBill,
			ZString origin,
			ZString destination,
			ZDateTime etd,
			ZDateTime eta,
			OrgHeader consignor,
			OrgHeader consignee,
			string shipmentType = Constants.ShipmentTypes.StandardHouse,
			ZDateTime? estimatedPickup = null,
			ZDateTime? pickupRequiredFrom = null
		)
		{
			var shipment = consol.Shipments.AddNew();
			shipment.JS_ShipmentType = shipmentType;

			SetUpShipment(
				shipment,
				houseBill,
				origin,
				destination,
				etd,
				eta,
				consignor,
				consignee,
				estimatedPickup: estimatedPickup ?? etd,
				pickupRequiredFrom
			);

			return shipment;
		}

		protected ForwardingShipment CreateShipment(
			ZString houseBill,
			ZString origin,
			ZString destination,
			ZDateTime etd,
			ZDateTime eta,
			OrgHeader consignor,
			OrgHeader consignee,
			string shipmentType = Constants.ShipmentTypes.StandardHouse,
			ZDateTime? estimatedPickup = null,
			ZDateTime? pickupRequiredFrom = null,
			ZInt? totalPackageCount = null,
			ZString? totalCountPackType = null
		)
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = shipmentType;
			shipment.JS_TotalPackageCount = totalPackageCount ?? 0;
			shipment.JS_F3_NKTotalCountPackType = totalCountPackType ?? ZString.Empty;

			SetUpShipment(
				shipment,
				houseBill,
				origin,
				destination,
				etd,
				eta,
				consignor,
				consignee,
				estimatedPickup: estimatedPickup ?? etd,
				pickupRequiredFrom
			);

			return shipment;
		}

		protected void SetUpShipment(
			ForwardingShipment shipment,
			ZString houseBill,
			ZString origin,
			ZString destination,
			ZDateTime etd,
			ZDateTime eta,
			OrgHeader consignor,
			OrgHeader consignee,
			ZDateTime? estimatedPickup = null,
			ZDateTime? pickupRequiredFrom = null
		)
		{
			shipment.JS_HouseBillOfLadingType = "YUS";
			shipment.JS_HouseBill = houseBill;
			shipment.JS_RL_NKOrigin = origin;
			shipment.JS_RL_NKDestination = destination;
			shipment.JS_E_DEP = etd;
			shipment.JS_E_ARV = eta;
			shipment.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;
			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;
			shipment.DocsAndCartage.JP_EstimatedPickup = estimatedPickup ?? ZDateTime.Empty;
			shipment.DocsAndCartage.JP_PickupRequiredFrom = pickupRequiredFrom ?? ZDateTime.Empty;
		}

		#endregion

		#region CreateCargoPackLine

		protected DepotCusOutturn CreateCargoPackLine(CusOutturnHeader header, string cargoType, string containerNumber, string houseBill, string oceanBill,
			int manifested, string manifestedUnit, string goodsDescription, decimal grossWeight, string grossWeightUQ = "KG", string marksAndNumbers = "", string inlandMovementMode = "ROA",
			decimal netWeight = 0, string newtWeightUQ = "KG", decimal volume = 7, string volumeUQ = "CM", string consignee = "", string customsStatus = "")
		{
			var cargoPackLine = header.Outturns.AddNew();
			cargoPackLine.C5_CargoType = cargoType;
			cargoPackLine.C5_ContainerNumber = containerNumber;
			cargoPackLine.C5_HouseBill = houseBill;
			cargoPackLine.C5_MasterBill = oceanBill;
			cargoPackLine.C5_OuterPacks = manifested;
			cargoPackLine.C5_OuterPackUnits = manifestedUnit;
			cargoPackLine.C5_GoodsDescription = goodsDescription;
			cargoPackLine.C5_MarksAndNumbers = marksAndNumbers;
			cargoPackLine.C5_CustomsStatus = customsStatus;

			string sampleSEIMessage = "UNH+000001+CUSRES:D:99B:UN'BGM+961:::SEI+1G79 7IAF 71F9:1++11'DTM+9:20090317154645464587:ZZZ'TDT+20+6990++11++++7631456::11'" +
	$"TDT+1++{inlandMovementMode}'NAD+MR+AAA374M::95'NAD+VW+41065894724::95'RFF+ABO:O00000007/DAT8::8'DOC+1'PAC+++FCL:67:95'PAC+100++BX:185:95'RFF+MB:'RFF+AAQ:OCLU8911239'" +
	"FTX+AAA+++CONSOLIDATED CARGO'GIS+FFO:109:95'MEA+AAE+G+KG:0000000023000.00'MEA+AAE+AAL+KG:0000000020000.00'MEA+AAE+ABJ+CM:0000000000020.00'NAD+CN++LOCAL FORWARDER'" +

	"DOC+1'PAC+++LCL:67:95'PAC+20++BX:185:95'RFF+MB:OBLDPT001'RFF+BH:HBL001'RFF+AAQ:OCLU8911239'FTX+AAA+++STUFF TYPE 1'MEA+AAE+G+KG:0000000005000.00'" +
	$"MEA+AAE+AAL+KG:0000000005000.00'MEA+AAE+ABJ+CM:0000000000005.00'NAD+CN++CONSIGNEE'DOC+1'PAC+++LCL:67:95'PAC+30++BX:185:95'RFF+MB:{oceanBill}'RFF+BH:{houseBill}'" +
	$"RFF+AAQ:{containerNumber}'PCI+28+{marksAndNumbers}'FTX+AAA+++{goodsDescription}'MEA+AAE+G+{grossWeightUQ}:000000000{grossWeight}'" +
	$"MEA+AAE+AAL+{newtWeightUQ}:000000000{netWeight}'MEA+AAE+ABJ+{volumeUQ}:000000000000{volume}'NAD+CN++{consignee}'UNT+43+000001'";

			CMRSEIMessage seiMessage = (CMRSEIMessage)header.Messages.AddNew(typeof(CMRSEIMessage));
			seiMessage.EM_MessageText = sampleSEIMessage;
			seiMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			seiMessage.EM_MessageType = CMRMessage.CMRMessageTypes.SEI;

			return cargoPackLine;
		}

		#endregion

		#region Create Consol

		protected (OrgHeader cfs, ZDateTime today, WhsWarehouse warehouse, OrgHeader consignor, OrgHeader consignee, RefVessel vessel) CreateTestData(bool arrivalWarehouse, string cfsPortCode = "NZAKL", bool parentProcessIsConsol = true, bool createTemplate = true)
		{
			var parentProcessType = parentProcessIsConsol ? WorkflowDescriptors.JobConsolWorkflowDescriptorCode : WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode;
			// workflow template
			//   ASN Request triggered on event BookingRequested
			//   Dispatch Request triggered on event BookingConfirmed
			if (createTemplate)
			{
				if (arrivalWarehouse)
				{
					CreateWorkflowTemplateForForwardingToArrivalTransitWarehouse(parentProcessType);
				}
				else
				{
					CreateWorkflowTemplateForForwardingToDepartureTransitWarehouse(parentProcessType);
				}
			}

			// transit warehouse OrgProxy Location
			var cfs = GlbBranch.CurrentBranch.OrgProxy;
			cfs.MainAddress.OA_RL_NKRelatedPortCode = cfsPortCode;
			cfs.Factory.Save();

			// master data
			var now = ZDateTime.Now;
			var today = now.Date.ToZDateTime();
			var warehouse = CreateTransitWarehouse(cfs);
			var consignor = TestHelper.CreateOrganisation("CNR");
			var consignee = TestHelper.CreateOrganisation("CNE");
			var vessel = CreateVessel("VesselCode", "Veslloy");
			Factory.Save();

			return (cfs: cfs, today: today, warehouse: warehouse, consignor: consignor, consignee: consignee, vessel: vessel);
		}

		protected (OrgHeader cfs, ZDateTime today, WhsWarehouse warehouse, OrgHeader consignor, OrgHeader consignee, RefVessel vessel) CreateTestDataForCombined(string cfsPortCode = "NZAKL", bool parentProcessIsConsol = true, bool isArrival = false)
		{
			// transit warehouse OrgProxy Location
			var cfs = GlbBranch.CurrentBranch.OrgProxy;
			cfs.MainAddress.OA_RL_NKRelatedPortCode = cfsPortCode;
			cfs.Factory.Save();

			// master data
			var now = ZDateTime.Now;
			var today = now.Date.ToZDateTime();
			var warehouse = CreateTransitWarehouse(cfs);
			var consignor = TestHelper.CreateOrganisation("CNR");
			var consignee = TestHelper.CreateOrganisation("CNE");
			var vessel = CreateVessel("VesselCode", "Veslloy");
			Factory.Save();

			return (cfs: cfs, today: today, warehouse: warehouse, consignor: consignor, consignee: consignee, vessel: vessel);
		}

		public enum CFSType
		{
			DepartureMiniCFSOnShipment,
			DepartureCFSOnConsol,
			ArrivalCFSOnConsol,
			ArrivalMiniCFSOnShipment
		}

		protected (WhsWarehouse departureMiniCFSForShipment, WhsWarehouse departureCFS, WhsWarehouse arrivalCFS, WhsWarehouse arrivalMiniCFSForShipment, OrgHeader consignor, OrgHeader consignee)
			CreateTestDataToMatchRCN(CFSType cfsToSend, string departureCFSPortCode = "AUBNE", string arrivalCFSPortCode = "NZAKL")
		{
			switch (cfsToSend)
			{
				case CFSType.DepartureMiniCFSOnShipment:
					CreateWorkflowTemplateForForwardingToDepartureTransitWarehouse(WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode);
					break;
				case CFSType.DepartureCFSOnConsol:
					CreateWorkflowTemplateForForwardingToDepartureTransitWarehouse(WorkflowDescriptors.JobConsolWorkflowDescriptorCode);
					break;
				case CFSType.ArrivalCFSOnConsol:
					CreateWorkflowTemplateForForwardingToArrivalTransitWarehouse(WorkflowDescriptors.JobConsolWorkflowDescriptorCode);

					break;
				case CFSType.ArrivalMiniCFSOnShipment:
					CreateWorkflowTemplateForForwardingToArrivalTransitWarehouse(WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode);
					break;
			}

			var orgProxy = GlbBranch.CurrentBranch.OrgProxy;
			var nonOrgProxy1 = Helper.CreateClient("NP1");
			var nonOrgProxy2 = Helper.CreateClient("NP2");
			var nonOrgProxy3 = Helper.CreateClient("NP3");
			var nonOrgProxy4 = Helper.CreateClient("NP4");

			orgProxy.MainAddress.OA_RL_NKRelatedPortCode = (cfsToSend == CFSType.ArrivalCFSOnConsol || cfsToSend == CFSType.ArrivalMiniCFSOnShipment)
															? arrivalCFSPortCode
															: departureCFSPortCode;
			orgProxy.Factory.Save();

			// master data
			var departureMiniCFSForShipment = CreateTransitWarehouse(nonOrgProxy1, code: "TDM");
			var departureCFS = CreateTransitWarehouse(cfsToSend == CFSType.DepartureCFSOnConsol ? orgProxy : nonOrgProxy2, code: "TDC");
			var arrivalCFS = CreateTransitWarehouse(cfsToSend == CFSType.ArrivalCFSOnConsol ? orgProxy : nonOrgProxy3, code: "TAC");
			var arrivalMiniCFSForShipment = CreateTransitWarehouse(cfsToSend == CFSType.ArrivalMiniCFSOnShipment ? orgProxy : nonOrgProxy4, code: "TAM");
			var consignor = TestHelper.CreateOrganisation("CNR");
			var consignee = TestHelper.CreateOrganisation("CNE");
			Factory.Save();

			return (departureMiniCFSForShipment: departureMiniCFSForShipment, departureCFS: departureCFS, arrivalCFS: arrivalCFS, arrivalMiniCFSForShipment: arrivalMiniCFSForShipment, consignor: consignor, consignee: consignee);
		}

		protected ForwardingConsol CreateConsol(ZString masterBill, RefVessel vessel, ZString load, ZString discharge, string voyageFlight = "", string transportMode = Constants.TransportModes.Sea)
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = load;
			consol.JK_RL_NKDischargePort = discharge;
			consol.JK_TransportMode = transportMode;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol.JK_MasterBillNum = masterBill;
			if (vessel != null)
			{
				consol.MostInterestingTransportForBinding[0].JW_VoyageFlightForBinding = voyageFlight;
				consol.MostInterestingTransportForBinding[0].JW_VesselForBinding = vessel.RV_Code;
			}
			consol.Transports.RemoveAndDeleteAll();

			return consol;
		}

		#endregion

		#region Create Transport

		protected Transport CreateTransport(ForwardingConsol parent, ZByte sequence, ZString transportMode, ZString vessel, ZString voyageFlight, ZString load, ZString discharge, ZDateTime etd, ZDateTime eta)
		{
			var transport = parent.Transports.AddNew();
			transport.JW_IsLinked = false;
			transport.JW_LegOrder = sequence;
			transport.JW_Vessel = vessel;
			transport.JW_VoyageFlight = voyageFlight;
			transport.JW_RL_NKLoadPort = load;
			transport.JW_RL_NKDiscPort = discharge;
			transport.JW_ETD = etd;
			transport.JW_ETA = eta;
			return transport;
		}

		protected Transport CreateTransport(ForwardingShipment parent, ZByte sequence, ZString transportMode, ZString vessel, ZString voyageFlight, ZString load, ZString discharge, ZDateTime etd, ZDateTime eta)
		{
			var transport = parent.Transports.AddNew();
			transport.JW_IsLinked = false;
			transport.JW_LegOrder = sequence;
			transport.JW_Vessel = vessel;
			transport.JW_VoyageFlight = voyageFlight;
			transport.JW_RL_NKLoadPort = load;
			transport.JW_RL_NKDiscPort = discharge;
			transport.JW_ETD = etd;
			transport.JW_ETA = eta;
			return transport;
		}

		#endregion

		#region Create Vessel

		protected RefVessel CreateVessel(ZString code, ZString lloydsNumber)
		{
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Code = code;
			vessel.RV_LloydsNumber = lloydsNumber;

			return vessel;
		}

		#endregion

		#region Create CusUnderbond

		protected CusUnderbond CreateCusUnderbond(ZString mAWB, ZString originPremiseId, ZString destinationPremiseId, RefVessel aplVessel, CusOutturn[] outturns)
		{
			var underbond = Factory.New<CusUnderbond>();

			underbond.C4_MAWB = mAWB;
			underbond.C4_Status = CMRUnderbondStatuses.Codes.ExpectedCargoArrivalAdviceReceived;
			underbond.C4_MovementReason = CMRUnderbondRequestCodes.Codes.Transshipment;
			underbond.C4_ArrivalDate = new ZDateTime(2017, 10, 1);
			underbond.C4_Outurned = new ZDateTime(2017, 9, 1);
			underbond.UnderbondStatus.Code = CMRBaseStatuses.Codes.NotSent;

			underbond.C4_IsMoveFromDischarge = ZBool.True;
			underbond.C4_UnderbondBySeaVessel = aplVessel.RV_Code;

			underbond.C4_OriginPremiseID = originPremiseId;
			underbond.C4_DestinationPremiseID = destinationPremiseId;

			underbond.Outturns.AddRange(outturns);

			return underbond;
		}

		#endregion

		#region Create CusOutturn

		protected CusOutturn CreateCusOutturn(ZString houseBill, ZString outerPackUnitType, ZInt outerPacks)
		{
			var outturn1 = Factory.New<CusOutturn>();
			outturn1.C5_HouseBill = houseBill;
			outturn1.C5_OuterPackUnits = outerPackUnitType;
			outturn1.C5_OuterPacks = outerPacks;

			return outturn1;
		}

		#endregion

		#region Create Transit Warehouse

		protected WhsWarehouse CreateTransitWarehouse(OrgHeader cfs, string code = "TRW")
		{
			var warehouse = Helper.CreateTRWWarehouse(code);
			warehouse.WW_OA_WarehouseAddress = cfs.MainAddress.PK;
			warehouse.WarehouseAddress.OA_City = "Sydney";
			warehouse.RelatedCompanyBranch.GB_RL_NKHomePort = cfs.MainAddress.OA_RL_NKRelatedPortCode;

			return warehouse;
		}

		#endregion

		#region Create Outer Packline

		protected static ForwardingPackLine CreateOuterPackline(ForwardingShipment shipment, int qty, string packType, ForwardingContainer container = null, string reference = "", string packLineId = "", decimal weight = 0m, string weightUQ = "KG", decimal length = 0m, decimal width = 0m, decimal height = 0m, string dimUQ = "CM", string marksAndNumbers = "", bool isHighRisk = false)
		{
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_JC = container != null ? container.PK : ZGuid.Empty;
			packLine.JL_PackageCount = qty;
			packLine.JL_F3_NKPackType = packType;
			packLine.JL_RefNumber = reference;
			packLine.JL_PackLineId = packLineId;
			packLine.JL_ActualWeight = weight;
			packLine.JL_ActualWeightUQ = weightUQ;
			packLine.JL_Length = length;
			packLine.JL_Width = width;
			packLine.JL_Height = height;
			packLine.JL_UnitOfDimension = dimUQ;
			packLine.JL_MarksAndNumbers = marksAndNumbers;
			packLine.JL_IsHighRisk = isHighRisk;
			shipment.UpdateShipmentFromOuterPackLines();

			return packLine;
		}

		protected static ForwardingPackLine CreateInnerPackline(ForwardingShipment shipment, ForwardingPackLine outerPackline, int qty, string packType, string reference = "", string packLineId = "", decimal weight = 0m, string weightUQ = "KG", decimal length = 0m, decimal width = 0m, decimal height = 0m, string dimUQ = "CM", string marksAndNumbers = "")
		{
			var packLine = shipment.InnerPackLines.AddNew();
			packLine.JL_JL_OuterPackLine = outerPackline.PK;
			packLine.JL_JC = ZGuid.Empty;
			packLine.JL_PackageCount = qty;
			packLine.JL_F3_NKPackType = packType;
			packLine.JL_RefNumber = reference;
			packLine.JL_PackLineId = packLineId;
			packLine.JL_ActualWeight = weight;
			packLine.JL_ActualWeightUQ = weightUQ;
			packLine.JL_Length = length;
			packLine.JL_Width = width;
			packLine.JL_Height = height;
			packLine.JL_UnitOfDimension = dimUQ;
			packLine.JL_MarksAndNumbers = marksAndNumbers;
			outerPackline.Shipment.UpdateShipmentFromInnerPackLines();

			return (ForwardingPackLine)packLine;
		}

		#endregion

		#region Create Container

		protected static ForwardingContainer CreateContainer(ForwardingConsol consol, string containerNum)
		{
			return CreateContainer(consol, containerNum, 1);
		}

		protected static ForwardingContainer CreateContainer(ForwardingConsol consol, string containerNum, string containerWithShippingMode)
		{
			var refContainer = consol.Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_ShippingMode, containerWithShippingMode));
			return CreateContainer(consol, containerNum, 1, refContainer);
		}

		protected static ForwardingContainer CreateContainer(ForwardingConsol consol, string containerNum, ZShort containerCount, string containerTypeCode = "20GP")
		{
			var refContainer = consol.Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, containerTypeCode);
			return CreateContainer(consol, containerNum, containerCount, refContainer);
		}

		protected static ForwardingContainer CreateContainer(ForwardingConsol consol, string containerNum, ZShort containerCount, RefContainer refContainer)
		{
			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = containerNum;
			container.JC_RC = refContainer.PK;
			container.JC_ContainerCount = containerCount;
			return container;
		}

		#endregion

		#region Create Additional Service

		protected static JobService CreateAdditionalService(ForwardingShipment shipment, ZString serviceCode, ZGuid location, ZDateTime? bookedTime = null, ZDateTime? completedTime = null, ZGuid? contractor = null, ZDateTime? duration = null, ZString? references = null, int serviceCount = 1, ZString? serviceNote = null)
		{
			var service = shipment.Services.AddNew();
			service.ES_Booked = bookedTime ?? ZDateTime.Empty;
			service.ES_Completed = completedTime ?? ZDateTime.Empty;
			service.ES_OH_Contractor = contractor ?? ZGuid.Empty;
			service.ES_Duration = duration ?? ZDateTime.Empty;
			service.ES_OA_Location = location;
			service.ES_References = references ?? ZString.Empty;
			service.ES_ServiceCode = serviceCode;
			service.ES_ServiceCount = serviceCount;
			service.ES_ServiceNote = serviceNote ?? ZString.Empty;

			return service;
		}

		#endregion

		#region CreateTriggerForTransitWarehouseToCustomsOutturnSendUniversalShipment

		protected static void CreateTriggerForTransitWarehouseToCustomsOutturnSendUniversalShipment(WhsItemReceiveTransportationUnit header)
		{
			var task = header.WorkflowItems.AddNew();
			task.P9_Type = "TRG";
			task.P9_Description = "Test";
			task.TriggerConditions.TriggerEventCode = Events.BookingRequestedCode;

			var action = task.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;
			action.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.CustomsOutturnAgent;
		}

		#endregion

		#region CreateTriggerForTransitWarehouseToCustomsOutturnSendUniversalEvent

		protected static void CreateTriggerForTransitWarehouseToCustomsOutturnSendUniversalEvent(WhsItemReceiveTransportationUnit header)
		{
			var task = header.WorkflowItems.AddNew();
			task.P9_Type = "TRG";
			task.P9_Description = "Test";
			task.TriggerConditions.TriggerEventCode = Events.GateInCode;

			var action = task.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXML;
			action.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.CustomsOutturnAgent;
			action.PQ_TriggerPartyService = ServiceCodesList.Codes.TransitWarehouseReceive;
		}

		#endregion

		#region CreateWorkflowForForwardingToTransitWarehouse

		public static void CreateWorkflowTemplateForForwardingToDepartureTransitWarehouse(ZString parentProcessType, bool isTWD = true)
		{
			var newFactory = new BusinessObjectFactory();
			var template = newFactory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = parentProcessType;
			CreateWorkflow(template, "DTW", Events.BookingRequestedCode, "TWR");

			if (isTWD)
			{
				CreateWorkflow(template, "DTW", Events.BookingConfirmedCode, "TWD");
			}
			else
			{
				CreateWorkflow(template, "DTW", Events.BookingConfirmedCode, "TWP");
				CreateWorkflow(template, "DTW", Events.BookingConfirmedCode, "TWD");
			}

			CreateWorkflow(template, "DTW", Events.ServiceRequestedCode, triggerType: "XUE");
			CreateWorkflow(template, "DTW", Events.ServiceSuspendedCode, triggerType: "XUE");
			newFactory.Save();
		}

		public static void CreateWorkflowTemplateForForwardingToArrivalTransitWarehouse(ZString parentProcessType)
		{
			var newFactory = new BusinessObjectFactory();
			var template = newFactory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = parentProcessType;
			CreateWorkflow(template, "ATW", Events.BookingRequestedCode, "TWR");
			CreateWorkflow(template, "ATW", Events.BookingConfirmedCode, "TWD");
			CreateWorkflow(template, "ATW", Events.ServiceRequestedCode, triggerType: "XUE");
			CreateWorkflow(template, "ATW", Events.ServiceSuspendedCode, triggerType: "XUE");
			newFactory.Save();
		}

		public static void CreateWorkflowTemplateForForwardingToTransitWarehouse_CreateBothReceiveAndDispatch(ZString parentProcessType, bool isArrival)
		{
			var newFactory = new BusinessObjectFactory();
			var template = newFactory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = parentProcessType;
			CreateWorkflow(template, isArrival ? "ATW" : "DTW", Events.BookingRequestedCode, "TWX");
			CreateWorkflow(template, isArrival ? "ATW" : "DTW", Events.BookingConfirmedCode, "TWD");
			newFactory.Save();
		}

		protected static void CreateWorkflowTemplate(ZString parentProcessType, string relatedParty, string eventCode, string relatedPartyService)
		{
			var newFactory = new BusinessObjectFactory();
			var template = newFactory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = parentProcessType;
			CreateWorkflow(template, relatedParty, eventCode, relatedPartyService);
			newFactory.Save();
		}

		#endregion

		#region CreateWorkflowForTransitWarehouseToSendCIDEventToForwarder

		public static void CreateWorkflowTemplateForTransitWarehouseToSendCIDEventToForwarder(string p0_ProcessType)
		{
			var newFactory = new BusinessObjectFactory();
			var template = newFactory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = p0_ProcessType;
			CreateWorkflow(template, "FOR", Events.ChangeOfIdentifierCode, triggerType: "XUE");
			newFactory.Save();
		}

		#endregion

		#region CreateWorkflowForTransitWarehouseToSendATCEventToForwarder

		public static void CreateWorkflowTemplateForTransitWarehouseToSendATCEventToForwarder(string p0_ProcessType)
		{
			var newFactory = new BusinessObjectFactory();
			var template = newFactory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = p0_ProcessType;
			CreateWorkflow(template, "FOR", Events.AttachedCode, triggerType: "XUE");
			newFactory.Save();
		}

		#endregion

		#region CreateWorkflowTemplateForCustomsToArrivalTransitWarehouse

		protected void CreateWorkflowTemplateForCustomsToArrivalTransitWarehouse(CusOutturnHeader header)
		{
			var task = header.WorkflowItems.AddNew();
			task.P9_Type = "TRG";
			task.P9_Description = "Test";
			task.TriggerConditions.TriggerEventCode = Events.BookingConfirmedCode;

			var action = task.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = "XUS";
			action.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.ArrivalTransitWarehouse;
			action.PQ_TriggerPartyService = ServiceCodesList.Codes.TransitWarehouseReceive;
		}

		#endregion

		#region PopulateOutturnHeaderWithMatchingDataForImport

		protected void PopulateOutturnHeaderWithMatchingDataForImport(CusOutturnHeader outturnHeader, WhsWarehouse warehouse, string premiseId, string vesselLloyds, string voyageFlightNumber, string vesselName)
		{
			/* fields used for matching when TWH sends shipment back to SeaCargoOutturn
				- C6_LloydsIMO
				- C6_VoyageNum
				- C6_OutturningPremiseID
			*/
			outturnHeader.C6_OA_OutturningPremise = warehouse.WarehouseAddress.PK;
			outturnHeader.C6_VoyageNum = voyageFlightNumber;
			outturnHeader.C6_VesselName = vesselName;
			outturnHeader.C6_LloydsIMO = vesselLloyds;
			outturnHeader.C6_OutturningPremiseID = premiseId;
			CreateWorkflowTemplateForCustomsToArrivalTransitWarehouse(outturnHeader);
		}

		#endregion

		#region CreateWorkflowForAirCargoCustomsToTransitWarehouse

		public static void CreateWorkflowTemplateForAirCargoCustomsToArrivalTransitWarehouse(ZString parentProcessType)
		{
			var newFactory = new BusinessObjectFactory();
			var template = newFactory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = parentProcessType;
			CreateWorkflow(template, "ATW", Events.BookingConfirmedCode, "TWR", "XUS");
			newFactory.Save();
		}

		#endregion

		#region CreateWorkflowTemplateForAirCargoDepotOutturn

		public static void CreateWorkflowTemplateForAirCargoDepotOutturn()
		{
			var newFactory = new BusinessObjectFactory();
			var template = newFactory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = JobInvoicingConsumerTypes.CusUnderbond.Code;

			var task = template.WorkflowItems.AddNew();
			task.P9_Type = "TRG";
			task.P9_Description = "Test";
			task.TriggerConditions.TriggerEventCode = Events.BookingConfirmedCode;

			var action = task.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = "XUS";
			action.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.ArrivalTransitWarehouse;
			action.PQ_TriggerPartyService = ServiceCodesList.Codes.TransitWarehouseReceive;
			action.PQ_OH_Recipient = GlbBranch.CurrentBranch.OrgProxy.PK;

			newFactory.Save();
		}

		#endregion

		#region CreateWorkflow

		protected static void CreateWorkflow(ProcessTaskTemplate template, string relatedParty, string eventCode, string relatedPartyService = "", string triggerType = "XUS")
		{
			var templateTask = template.WorkflowItems.Tasks.AddNew();
			templateTask.P9_Description = "Test";
			templateTask.P9_Type = "TRG";
			templateTask.TriggerConditions.TriggerEventCode = eventCode;

			var action = templateTask.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = triggerType;
			action.PQ_Calc_TriggerParty = relatedParty;
			action.PQ_TriggerPartyService = relatedPartyService;
		}

		protected static void CreateWorkflowTemplate(string processType, string relatedParty)
		{
			var newFactory = new BusinessObjectFactory();
			var template = newFactory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = processType;
			CreateWorkflow(template, relatedParty, Events.BookingRequestedCode);
			newFactory.Save();
		}

		#endregion

		#region TriggerAndFireTransitRequestUsingBookingRequested

		protected static void TriggerAndFireTransitRequestUsingBookingRequested(ForwardingConsol consol)
		{
			consol.Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var consolInNewFactory = newFactory.Load<ForwardingConsol>(consol.PK);
			consolInNewFactory.WorkflowItems.TriggersIncludingRelated.Rebuild();
			consolInNewFactory.Logs.AddNew(new EventValue(Events.BookingRequested, eventTime: ZDateTimeOffset.Now));
			newFactory.Save();

			MasterFilesTestHelper.RunLogWalker();
		}

		protected static void TriggerAndFireTransitRequestUsingBookingRequested(ForwardingShipment shipment)
		{
			shipment.Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var shipmentInNewFactory = newFactory.Load<ForwardingShipment>(shipment.PK);
			shipmentInNewFactory.WorkflowItems.TriggersIncludingRelated.Rebuild();
			shipmentInNewFactory.Logs.AddNew(new EventValue(Events.BookingRequested, eventTime: ZDateTimeOffset.Now));
			newFactory.Save();

			MasterFilesTestHelper.RunLogWalker();
		}

		protected static void TriggerAndFireTransitRequestUsingBookingPending(ForwardingShipment shipment)
		{
			shipment.Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var shipmentInNewFactory = newFactory.Load<ForwardingShipment>(shipment.PK);
			shipmentInNewFactory.WorkflowItems.TriggersIncludingRelated.Rebuild();
			shipmentInNewFactory.Logs.AddNew(new EventValue(Events.BookingPending, eventTime: ZDateTimeOffset.Now));
			newFactory.Save();

			MasterFilesTestHelper.RunLogWalker();
		}

		#endregion

		#region TriggerAndFireTransitRequestUsingBookingConfirmed

		protected static void TriggerAndFireTransitRequestUsingBookingConfirmed_ToSendDispatchInstructions(ForwardingConsol consol)
		{
			consol.Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var consolInNewFactory = newFactory.Load<ForwardingConsol>(consol.PK);
			consolInNewFactory.WorkflowItems.TriggersIncludingRelated.Rebuild();
			consolInNewFactory.Logs.AddNew(new EventValue(Events.BookingConfirmed, eventTime: ZDateTimeOffset.Now));
			newFactory.Save();

			MasterFilesTestHelper.RunLogWalker();
		}

		protected static void TriggerAndFireTransitRequestUsingBookingConfirmed_ToSendDispatchInstructions(ForwardingShipment shipment)
		{
			shipment.Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var shipmentInNewFactory = newFactory.Load<ForwardingShipment>(shipment.PK);
			shipmentInNewFactory.WorkflowItems.TriggersIncludingRelated.Rebuild();
			shipmentInNewFactory.Logs.AddNew(new EventValue(Events.BookingConfirmed, eventTime: ZDateTimeOffset.Now));
			newFactory.Save();

			MasterFilesTestHelper.RunLogWalker();
		}

		#endregion

		#region TriggerAndFireTransitRequestForRelease

		protected static void TriggerAndFireTransitRequestForRelease(ForwardingConsol consol)
		{
			consol.Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var consolInNewFactory = newFactory.Load<ForwardingConsol>(consol.PK);
			consolInNewFactory.WorkflowItems.TriggersIncludingRelated.Rebuild();
			consolInNewFactory.Logs.AddNew(new EventValue(Events.BookingConfirmed, eventTime: ZDateTimeOffset.Now));
			newFactory.Save();

			MasterFilesTestHelper.RunLogWalker();
		}

		protected static void TriggerAndFireTransitRequestForRelease(ForwardingShipment shipment)
		{
			shipment.Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var shipmentInNewFactory = newFactory.Load<ForwardingShipment>(shipment.PK);
			shipmentInNewFactory.WorkflowItems.TriggersIncludingRelated.Rebuild();
			shipmentInNewFactory.Logs.AddNew(new EventValue(Events.BookingConfirmed, eventTime: ZDateTimeOffset.Now));
			newFactory.Save();

			MasterFilesTestHelper.RunLogWalker();
		}

		protected static void TriggerAndFireTransitRequestForRelease(CusMAWB airCargoReport)
		{
			airCargoReport.Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var airCargoInNewFactory = newFactory.Load<CusMAWB>(airCargoReport.PK);

			var workflowProvider = airCargoInNewFactory as IWorkflowProvider;
			workflowProvider.WorkflowItems.TriggersIncludingRelated.Rebuild();
			airCargoInNewFactory.Logs.AddNew(new EventValue(Events.BookingConfirmed, eventTime: ZDateTimeOffset.Now));
			newFactory.Save();

			MasterFilesTestHelper.RunLogWalker();
		}

		protected static void TriggerAndFireTransitRequestForRelease(CusUnderbond underbond)
		{
			underbond.Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var airCargoInNewFactory = newFactory.Load<CusUnderbond>(underbond.PK);

			var workflowProvider = airCargoInNewFactory as IWorkflowProvider;
			workflowProvider.WorkflowItems.TriggersIncludingRelated.Rebuild();
			airCargoInNewFactory.Logs.AddNew(new EventValue(Events.BookingConfirmed, eventTime: ZDateTimeOffset.Now));
			newFactory.Save();

			MasterFilesTestHelper.RunLogWalker();
		}

		#endregion

		#region TriggerAndFireUniversalEventFromShipment

		protected static void TriggerAndFireUniversalEventFromShipment(ForwardingShipment shipment, Event eventToSend, string reference)
		{
			shipment.Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var shipmentInNewFactory = newFactory.Load<ForwardingShipment>(shipment.PK);
			shipmentInNewFactory.WorkflowItems.TriggersIncludingRelated.Rebuild();
			shipmentInNewFactory.Logs.AddNew(new EventValue(eventToSend, reference: reference, eventTime: ZDateTimeOffset.Now));
			newFactory.Save();

			MasterFilesTestHelper.RunLogWalker();
		}

		#endregion

		#region TriggerAndFireUniversalEventFromConsol

		protected static void TriggerAndFireUniversalEventFromConsol(ForwardingConsol consol, Event eventToSend, string reference)
		{
			consol.Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var consolInNewFactory = newFactory.Load<ForwardingConsol>(consol.PK);
			consolInNewFactory.WorkflowItems.TriggersIncludingRelated.Rebuild();
			consolInNewFactory.Logs.AddNew(new EventValue(eventToSend, reference: reference, eventTime: ZDateTimeOffset.Now));
			newFactory.Save();

			MasterFilesTestHelper.RunLogWalker();
		}

		#endregion

		#region TriggerAndFireFreightUnloadedEventFromRCN

		protected static void TriggerAndFireFreightUnloadedEventFromRCN(WhsItemReceiveConsignment rcn)
		{
			rcn.Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var rcnInNewFactory = newFactory.Load<WhsItemReceiveConsignment>(rcn.PK);
			rcnInNewFactory.WorkflowItems.TriggersIncludingRelated.Rebuild();
			rcnInNewFactory.Logs.AddNew(new EventValue(Events.FreightUnloaded, eventTime: ZDateTimeOffset.Now));
			newFactory.Save();

			MasterFilesTestHelper.RunLogWalker();
		}

		#endregion

		#region TriggerAndFireFreightLoadedEventFromDCN

		protected static void TriggerAndFireFreightLoadedEventFromDCN(WhsItemDispatchConsignment dcn)
		{
			dcn.Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var dcnInNewFactory = newFactory.Load<WhsItemDispatchConsignment>(dcn.PK);
			dcnInNewFactory.WorkflowItems.TriggersIncludingRelated.Rebuild();
			dcnInNewFactory.Logs.AddNew(new EventValue(Events.FreightLoaded, eventTime: ZDateTimeOffset.Now));
			newFactory.Save();

			MasterFilesTestHelper.RunLogWalker();
		}

		#endregion

		#region TriggerAndFireToOutturn

		protected static void TriggerAndFireToOutturn(WhsItemReceiveTransportationUnit rtu)
		{
			rtu.Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var rtuInNewFactory = newFactory.Load<WhsItemReceiveTransportationUnit>(rtu.PK);
			rtuInNewFactory.WorkflowItems.TriggersIncludingRelated.Rebuild();
			rtuInNewFactory.Logs.AddNew(new EventValue(Events.BookingRequested, eventTime: ZDateTimeOffset.Now));
			newFactory.Save();

			MasterFilesTestHelper.RunLogWalker();
		}

		#endregion

		#region UnloadAndLabelPackage

		protected static WhsItemPackageState UnloadAndLabelPackage(WhsItemPackageState packageState, WhsItemReceiveTransportationUnit rtu1, string packageID, bool setDetails = false, decimal weight = 1, string weightUQ = "KG", decimal volume = 1, string volumeUQ = "D3", bool isDamaged = false, bool isPillaged = false)
		{
			var packageStateToUnload = packageState;

			if (packageState.Package.KP_PackageQty > 1)
			{
				var factory = packageState.Factory;

				var package = packageState.Package.PackageJob.Packages.AddNew();
				package.KP_F3_NKPackType = packageState.Package.KP_F3_NKPackType;
				package.KP_ExternalReference = packageState.Package.KP_ExternalReference;
				_ = package.BookedDimensions;

				packageStateToUnload = factory.NewWithValidTestData<WhsItemPackageState>();
				packageStateToUnload.WPS_KP_Package = package.PK;
				packageStateToUnload.WPS_WRC_TransitReceiveConsignment = packageState.WPS_WRC_TransitReceiveConsignment;
				packageStateToUnload.WPS_WW_Warehouse = rtu1.WRH_WW_Warehouse;

				packageState.Package.KP_PackageQty--;
			}

			packageStateToUnload.WPS_WRH_TransitReceiveHeader = rtu1.PK;
			packageStateToUnload.WPS_WW_Warehouse = rtu1.WRH_WW_Warehouse;
			packageStateToUnload.Package.KP_PackageQty = 1;
			packageStateToUnload.Package.KP_PackageID = packageID;
			if (setDetails)
			{
				packageStateToUnload.Package.KP_Weight = weight;
				packageStateToUnload.Package.KP_WeightUQ = weightUQ;
				packageStateToUnload.Package.KP_Volume = volume;
				packageStateToUnload.Package.KP_VolumeUQ = volumeUQ;
				packageStateToUnload.Package.KP_IsDamaged = isDamaged;
				packageStateToUnload.Package.KP_IsPillaged = isPillaged;
			}
			packageStateToUnload.WPS_Status = TransitWarehouseStatuses.Codes.Arrived;
			packageStateToUnload.WPS_WL_LastLocation = rtu1.WRH_WL_StagingLocation;

			return packageStateToUnload;
		}

		#endregion

		#endregion

		#region TriggerAndFireOutturn

		protected static void TriggerAndFireOutturn(WhsItemReceiveConsignment consignment)
		{
			consignment.Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var consignmentInNewFactory = newFactory.Load<WhsItemReceiveConsignment>(consignment.PK);
			consignmentInNewFactory.WorkflowItems.TriggersIncludingRelated.Rebuild();
			consignmentInNewFactory.Logs.AddNew(new EventValue(Events.BookingRequested, eventTime: ZDateTimeOffset.Now));
			newFactory.Save();

			MasterFilesTestHelper.RunLogWalker();
		}

		protected static void TriggerAndSendASN(WhsItemReceiveASN asn)
		{
			asn.Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var asnInNewFactory = newFactory.Load<WhsItemReceiveASN>(asn.PK);
			asnInNewFactory.WorkflowItems.TriggersIncludingRelated.Rebuild();
			asnInNewFactory.Logs.AddNew(new EventValue(Events.BookingRequested, eventTime: ZDateTimeOffset.Now));
			newFactory.Save();

			MasterFilesTestHelper.RunLogWalker();
		}

		protected static void TriggerAndFireOutturn(WhsItemDispatchConsignment consignment)
		{
			consignment.Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var consignmentInNewFactory = newFactory.Load<WhsItemDispatchConsignment>(consignment.PK);
			consignmentInNewFactory.WorkflowItems.TriggersIncludingRelated.Rebuild();
			consignmentInNewFactory.Logs.AddNew(new EventValue(Events.BookingConfirmed, eventTime: ZDateTimeOffset.Now));
			newFactory.Save();

			MasterFilesTestHelper.RunLogWalker();
		}

		protected static void CreateWorkflowTemplateForReceiveConsignmentToShipment()
		{
			var newFactory = new BusinessObjectFactory();
			var template = newFactory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptors.TransitReceiveConsignment;
			CreateWorkflow(template, "FOR", Events.BookingRequestedCode);
			newFactory.Save();
		}

		protected static void CreateWorkflowTemplateForReceiveConsignmentToAirCargoHouse()
		{
			var newFactory = new BusinessObjectFactory();
			var template = newFactory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptors.TransitReceiveConsignment;
			CreateWorkflow(template, "COA", Events.BookingRequestedCode);
			newFactory.Save();
		}

		protected static void CreateWorkflowTemplateForReceiveASNToAirCargo()
		{
			var newFactory = new BusinessObjectFactory();
			var template = newFactory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptors.TransitReceiveASN;
			CreateWorkflow(template, "ARP", Events.BookingRequestedCode);
			newFactory.Save();
		}

		protected static void CreateWorkflowTemplateForReceiveConsignment_SendBKCEventToForwarder()
		{
			var newFactory = new BusinessObjectFactory();
			var template = newFactory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptors.TransitReceiveConsignment;
			CreateWorkflow(template, "FOR", Events.BookingConfirmedCode, triggerType: "XUE");
			newFactory.Save();
		}

		protected static void CreateWorkflowTemplateForShipmentToSendCustomsEventsToForwarder()
		{
			var newFactory = new BusinessObjectFactory();
			var template = newFactory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode;
			CreateWorkflow(template, "DTW", Events.ClearanceCompletedCode, triggerType: "XUE");
			CreateWorkflow(template, "DTW", Events.HeldCode, triggerType: "XUE");
			CreateWorkflow(template, "DTW", Events.CustomsNumberEnteredCode, triggerType: "XUE");
			CreateWorkflow(template, "DTW", Events.CustomsReleaseNumberEnteredCode, triggerType: "XUE");

			newFactory.Save();
		}

		protected static void CreateWorkflowTemplateForDispatchConsignment_SendBKCEventToForwarder()
		{
			var newFactory = new BusinessObjectFactory();
			var template = newFactory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptors.TransitDispatchConsignment;
			CreateWorkflow(template, "FOR", Events.BookingConfirmedCode, triggerType: "XUS");
			newFactory.Save();
		}

		#endregion

		#region TriggerAndFireTransitHeaderCIDEvent

		protected static void TriggerAndFireTransitHeaderCIDEvent(WhsItemReceiveTransportationUnit rtu, string reference)
		{
			rtu.Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var rtuInNewFactory = newFactory.Load<WhsItemReceiveTransportationUnit>(rtu.PK);
			rtuInNewFactory.WorkflowItems.TriggersIncludingRelated.Rebuild();
			rtuInNewFactory.Logs.AddNew(new EventValue(Events.ChangeOfIdentifier, reference: reference, eventTime: ZDateTimeOffset.Now));
			newFactory.Save();

			MasterFilesTestHelper.RunLogWalker();
		}

		protected static void TriggerAndFireTransitHeaderCIDEvent(WhsItemDispatchTransportationUnit dtu, string reference)
		{
			dtu.Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var dtuInNewFactory = newFactory.Load<WhsItemDispatchTransportationUnit>(dtu.PK);
			dtuInNewFactory.WorkflowItems.TriggersIncludingRelated.Rebuild();
			dtuInNewFactory.Logs.AddNew(new EventValue(Events.ChangeOfIdentifier, reference: reference, eventTime: ZDateTimeOffset.Now));
			newFactory.Save();

			MasterFilesTestHelper.RunLogWalker();
		}

		#endregion

		#region TriggerAndFireTransitHeaderATCEvent

		protected static void TriggerAndFireTransitHeaderATCEvent(WhsItemReceiveTransportationUnit rtu)
		{
			new ProcessTask.Loader(rtu.Factory).CreateTasksAndMilestonesFromTemplateIfRequired(rtu, TemplateApplicationParameters.ApplyIgnoreHasChanges());
			rtu.Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var rtuInNewFactory = newFactory.Load<WhsItemReceiveTransportationUnit>(rtu.PK);
			var reference = "|NEW=ATTACH|REF=Containerised_RTU|RFN=TR001";
			rtuInNewFactory.WorkflowItems.TriggersIncludingRelated.Rebuild();
			rtuInNewFactory.Logs.AddNew(new EventValue(Events.Attached, reference: reference, eventTime: ZDateTimeOffset.Now));
			newFactory.Save();

			MasterFilesTestHelper.RunLogWalker();
		}

		protected static void TriggerAndFireTransitHeaderATCEvent(WhsItemDispatchTransportationUnit dtu)
		{
			new ProcessTask.Loader(dtu.Factory).CreateTasksAndMilestonesFromTemplateIfRequired(dtu, TemplateApplicationParameters.ApplyIgnoreHasChanges());
			dtu.Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var dtuInNewFactory = newFactory.Load<WhsItemDispatchTransportationUnit>(dtu.PK);
			var reference = "|NEW=ATTACH|REF=Containerised_DTU|RFN=TD001";
			dtuInNewFactory.WorkflowItems.TriggersIncludingRelated.Rebuild();
			dtuInNewFactory.Logs.AddNew(new EventValue(Events.Attached, reference: reference, eventTime: ZDateTimeOffset.Now));
			newFactory.Save();

			MasterFilesTestHelper.RunLogWalker();
		}

		#endregion

		#region CreateWorkflowTemplateForSendingCRESAMessage

		protected static void CreateWorkflowTemplateForSendingCRESAMessageFromRCN()
		{
			var newFactory = new BusinessObjectFactory();
			var template = newFactory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptors.TransitReceiveConsignment;
			CreateWorkflow(template, "DTW", Events.FreightUnloadedCode, triggerType: "SCM");
			newFactory.Save();
		}

		protected static void CreateWorkflowTemplateForSendingCRESAMessageFromDCN()
		{
			var newFactory = new BusinessObjectFactory();
			var template = newFactory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptors.TransitDispatchConsignment;
			CreateWorkflow(template, "DTW", Events.FreightLoadedCode, triggerType: "SCM");
			newFactory.Save();
		}

		#endregion

		#region Assertions

		protected void AssertJobDocAddress(JobDocAddress jobDocAddress, string companyName, string address, string city)
		{
			AssertEquals(companyName, jobDocAddress.CompanyName);
			AssertEquals(address, jobDocAddress.E2_Address1);
			AssertEquals(city, jobDocAddress.E2_City);
		}

		protected static void AssertPackageProperties(PkgPackage package, int qty, string packType, string goodsDescription, bool isUnknownQty = false)
		{
			AssertEquals(qty, package.KP_PackageQty);
			AssertEquals(packType, package.KP_F3_NKPackType);
			AssertEquals(goodsDescription, package.KP_GoodsDescription);
			AssertEquals(isUnknownQty, package.KP_IsUnknownQty);
		}

		protected static void AssertPackageOutturned(CusHAWB cusHAWB, int packageQty, int outturnedQty, decimal weightOutturned, string weightOutturnedUQ, decimal volumeOutturned, string volumeOutturnedUQ, bool isDamaged = false, bool isPillaged = false, string expectedOutturnResultType = "NIL", ZDateTime? unpackDate = null, ZDateTime? gateInTime = null, string packType = "", bool fromMAWB = false)
		{
			CusOutturn outturn;
			if (fromMAWB)
			{
				outturn = cusHAWB.MAWB.Underbonds.Cast<CusUnderbond>().Single().Outturns.Cast<CusOutturn>().Single(o => o.C5_HouseBill == cusHAWB.CS_HAWB);
			}
			else
			{
				outturn = cusHAWB.Underbonds.Cast<CusUnderbond>().Single().Outturns.Cast<CusOutturn>().Single();
			}
			AssertPackageOutturnedCore(outturn, cusHAWB, packageQty, outturnedQty, weightOutturned, weightOutturnedUQ, volumeOutturned, volumeOutturnedUQ, isDamaged, isPillaged, expectedOutturnResultType, unpackDate, gateInTime, packType, fromMAWB);
		}

		static void AssertPackageOutturnedCore(CusOutturn outturn, CusHAWB cusHAWB, int packageQty, int outturnedQty, decimal weightOutturned, string weightOutturnedUQ, decimal volumeOutturned, string volumeOutturnedUQ, bool isDamaged = false, bool isPillaged = false, string expectedOutturnResultType = "NIL", ZDateTime? unpackDate = null, ZDateTime? gateInTime = null, string packType = "", bool fromMAWB = false)
		{
			CombineAssertions(() =>
			{
				AssertEquals("Outturn package qty must be correct.", packageQty, outturn.C5_OuterPacks);
				AssertEquals("Outturn outturned qty must be correct.", outturnedQty, outturn.C5_PackagesOutturned);
				AssertEquals("Outturn pack type must be correct.", packType, outturn.C5_PackagesUnits);
				AssertEquals("Outturn result type must be correct.", expectedOutturnResultType, outturn.C5_OutturnResultType);
				AssertEquals("Outturn weight must be correct.", weightOutturned, outturn.C5_WeightOutturned);
				AssertEquals("Outturn weight unit must be correct.", weightOutturnedUQ, outturn.C5_WeightOutturnedUQ);
				AssertEquals("Outturn volume must be correct.", volumeOutturned, outturn.C5_VolumeOutturned);
				AssertEquals("Outturn volume unit must be correct.", volumeOutturnedUQ, outturn.C5_VolumeOutturnedUQ);
				AssertEquals("Outturn damaged indicator must be correct.", isDamaged, outturn.C5_DamageIndicator);
				AssertEquals("Outturn pillaged indicator must be correct.", isPillaged, outturn.C5_PillageIndicator);
				AssertEquals("Outturn unpack date must be correct.", unpackDate ?? ZDateTime.Empty, outturn.C5_CargoUnpackDate);
				AssertEquals("Outturn gate in date must be correct.", gateInTime ?? ZDateTime.Empty, outturn.C5_CargoReceiptDate);
				AssertEquals("Outturn parent code must be correct.", "CS", outturn.C5_ParentTableCode);
				AssertEquals("Outturn parent must be correct.", cusHAWB.PK, outturn.C5_ParentID);
			});
		}

		protected static WhsItemReceiveConsignment AssertConsignment(BusinessObjectFactory factory, ZString consignmentID, ZDateTime eta, ZDateTime etd, ZString nextDischarge, ZString serviceLevel, WhsWarehouse whs, ZGuid shipmentPK, Transport expectedTransport)
		{
			var receiveConsignments = factory.Load<WhsItemReceiveConsignment>(new ZQuery(WhsItemReceiveConsignmentSchema.WRC_ConsignmentID, consignmentID));
			var receiveConsignment = receiveConsignments.Single();

			CombineAssertions("Receive Consignment.", () =>
			{
				AssertEquals("ETA", eta, receiveConsignment.WRC_ExpectedArrivalTime);
				AssertEquals("ETD", etd, receiveConsignment.WRC_ExpectedDispatchTime);
				AssertEquals("Next Discharge Port", nextDischarge, receiveConsignment.WRC_RL_NKNextDischargePort);
				AssertEquals("Service Level", serviceLevel, receiveConsignment.WRC_RS_NKServiceLevel);
				AssertEquals("Warehouse", whs.PK, receiveConsignment.WRC_WW_IntendedWarehouse);
				AssertEquals("Parent Shipment", shipmentPK, receiveConsignment.WRC_ParentID);
				AssertEquals("Parent Shipment table code", "JS", receiveConsignment.WRC_ParentTableCode);

				var transports = new TransportCollection(receiveConsignment);
				transports.Load();
				var transport = (Transport)transports.Single();
				AssertEquals("Discharge port", expectedTransport.JW_RL_NKDiscPort, transport.JW_RL_NKDiscPort);
				AssertEquals("Load port", expectedTransport.JW_RL_NKLoadPort, transport.JW_RL_NKLoadPort);
				AssertEquals("Vessel", expectedTransport.JW_Vessel, transport.JW_Vessel);
				AssertEquals("Voyage Flight", expectedTransport.JW_VoyageFlight, transport.JW_VoyageFlight);
				AssertEquals("ETD", expectedTransport.JW_ETD, transport.JW_ETD);
				AssertEquals("ETA", expectedTransport.JW_ETA, transport.JW_ETA);
			});

			return receiveConsignment;
		}

		protected static WhsItemReceiveConsignment AssertAndReturnReceiveConsignment(BusinessObjectFactory factory, ZString consignmentID, WhsWarehouse whs, ZGuid shipmentPK, IEnumerable<CusEntryNumber> refNumbers = null, Dictionary<string, string> rcnCusEntryNumMapping = null, string refType = null)
		{
			var query = new ZQuery(WhsItemReceiveConsignmentSchema.WRC_ConsignmentID, consignmentID);
			query.AddToFilter(WhsItemReceiveConsignmentSchema.WRC_WW_IntendedWarehouse, whs.PK);
			query.AddToFilter(WhsItemReceiveConsignmentSchema.WRC_ParentID, shipmentPK);
			var receiveConsignments = factory.Load<WhsItemReceiveConsignment>(query);
			var receiveConsignment = receiveConsignments.Single();

			CombineAssertions("Receive Consignment.", () =>
			{
				AssertEquals("Warehouse", whs.PK, receiveConsignment.WRC_WW_IntendedWarehouse);
				AssertEquals("Parent Shipment", shipmentPK, receiveConsignment.WRC_ParentID);
				AssertEquals("Parent Shipment table code", "JS", receiveConsignment.WRC_ParentTableCode);
			});

			if (rcnCusEntryNumMapping != null)
			{
				AssertNotNull(refNumbers);
				AssertCollectionContains(receiveConsignment.PK, refNumbers.Select(r => r.CE_ParentID));

				var actualRefNumber = refNumbers.FirstOrDefault(r => r.CE_ParentID == receiveConsignment.PK);

				string expectedRefNumberValue;
				AssertEquals(true, rcnCusEntryNumMapping.TryGetValue(receiveConsignment.WRC_ConsignmentID, out expectedRefNumberValue));

				AssertAdditionalReference(receiveConsignment, [actualRefNumber], refType, expectedRefNumberValue);
			}

			return receiveConsignment;
		}

		protected static WhsItemDispatchConsignment AssertAndReturnDispatchConsignment(BusinessObjectFactory factory, ZString consignmentID, WhsWarehouse whs, ZGuid shipmentPK, IEnumerable<CusEntryNumber> refNumbers = null, Dictionary<string, string> dcnCusEntryNumMapping = null, string refType = null)
		{
			var query = new ZQuery(WhsItemDispatchConsignmentSchema.WDC_ConsignmentID, consignmentID);
			query.AddToFilter(WhsItemDispatchConsignmentSchema.WDC_WW_Warehouse, whs.PK);
			query.AddToFilter(WhsItemDispatchConsignmentSchema.WDC_ParentID, shipmentPK);
			var dispatchConsignments = factory.Load<WhsItemDispatchConsignment>(query);
			var dispatchConsignment = dispatchConsignments.Single();

			CombineAssertions("Dispatch Consignment.", () =>
			{
				AssertEquals("Warehouse", whs.PK, dispatchConsignment.WDC_WW_Warehouse);
				AssertEquals("Parent Shipment", shipmentPK, dispatchConsignment.WDC_ParentID);
				AssertEquals("Parent Shipment table code", "JS", dispatchConsignment.WDC_ParentTableCode);
			});

			if (dcnCusEntryNumMapping != null)
			{
				AssertNotNull(refNumbers);
				AssertCollectionContains(dispatchConsignment.PK, refNumbers.Select(r => r.CE_ParentID));

				var actualRefNumber = refNumbers.FirstOrDefault(r => r.CE_ParentID == dispatchConsignment.PK);

				string expectedRefNumberValue;
				AssertEquals(true, dcnCusEntryNumMapping.TryGetValue(dispatchConsignment.WDC_ConsignmentID, out expectedRefNumberValue));

				AssertAdditionalReference(dispatchConsignment, [actualRefNumber], refType, expectedRefNumberValue);
			}

			return dispatchConsignment;
		}

		protected static void AssertCargoPackLine(DepotCusOutturn outturn, int outturnQty, decimal weightOutturned, decimal volumeOutturned, bool isDamaged = false, bool isPillaged = false, string expectedOutturnResultType = "NIL", ZDateTime? unpackDate = null, string packType = "")
		{
			AssertEquals("Outturn qty must be correct.", outturnQty, outturn.C5_PackagesOutturned);
			AssertEquals("Outturn pack type must be correct.", packType, outturn.C5_PackagesUnits);
			AssertEquals("Outturn result type must be correct.", expectedOutturnResultType, outturn.C5_OutturnResultType);
			AssertEquals("Outturn weight must be correct.", weightOutturned, outturn.C5_WeightOutturned);
			AssertEquals("Outturn volume must be correct.", volumeOutturned, outturn.C5_VolumeOutturned);
			AssertEquals("Outturn damaged indicator must be correct.", isDamaged, outturn.C5_DamageIndicator);
			AssertEquals("Outturn pillaged indicator must be correct.", isPillaged, outturn.C5_PillageIndicator);
			AssertEquals("Outturn unpack date must be correct.", unpackDate ?? ZDateTime.Empty, outturn.C5_CargoUnpackDate);
		}

		protected static void AssertConsignmentPackages(WhsItemReceiveConsignment consignment, ZInt packageCount)
		{
			var packageStates = consignment.PackageStates;
			AssertEquals($"{packageCount} package states must be created in UXML.", packageCount, packageStates.Count);
		}

		protected static void AssertDispatchConsignmentPackageStates(IEnumerable<WhsItemPackageState> packageStates, WhsItemDispatchConsignment dispatchConsignment, ZGuid shipmentPK)
		{
			AssertEquals(packageStates.Count(), dispatchConsignment.PackageStates.Count);

			foreach (var packageState in packageStates)
			{
				AssertDispatchConsignment(packageState, dispatchConsignment, shipmentPK);
			}
		}

		protected static void AssertDispatchConsignment(WhsItemPackageState packageState, WhsItemDispatchConsignment dispatchConsignment, ZGuid shipmentPK)
		{
			packageState.Reload();
			AssertEquals("Dispatch Consignment list parent PK", shipmentPK, dispatchConsignment.WDC_ParentID);
			AssertEquals("Dispatch Consignment parent code", "JS", dispatchConsignment.WDC_ParentTableCode);
			AssertEquals(dispatchConsignment.PK, packageState.WPS_WDC_TransitDispatchConsignment);
		}

		protected static void AssertDispatchConsignment(WhsItemDispatchConsignment dispatchConsignment, ZGuid shipmentPK)
		{
			AssertEquals("Dispatch Consignment list parent PK", shipmentPK, dispatchConsignment.WDC_ParentID);
			AssertEquals("Dispatch Consignment parent code", "JS", dispatchConsignment.WDC_ParentTableCode);
		}

		protected static void AssertPackageWithDCN(WhsItemPackageState packageState, WhsItemDispatchConsignment dispatchConsignment, WhsItemDispatchLoadList loadList)
		{
			AssertEquals(dispatchConsignment.PK, packageState.WPS_WDC_TransitDispatchConsignment);
			AssertEquals(loadList.PK, packageState.WPS_WDL_LoadList);
		}

		protected static void AssertPackage(WhsItemPackageState packageState, string expectedPackageType, string packageId = "")
		{
			AssertEquals(packageId, packageState.Package.KP_PackageID);
			AssertEquals(expectedPackageType, packageState.Package.KP_F3_NKPackType);
		}

		protected static void AssertReceiveASN(WhsItemReceiveASN receiveASN, ZGuid parentPK, string parentTableCode, string expectedExternalReference, Transport expectedTransport = null, OrgAddress rcnBookingPartyAddress = null)
		{
			AssertEquals("Container PK", parentPK, receiveASN.WRP_ParentID);
			AssertEquals("Container Parent Code", parentTableCode, receiveASN.WRP_ParentTableCode);
			AssertEquals("External Reference", expectedExternalReference, receiveASN.WRP_VehicleReference);

			if (rcnBookingPartyAddress != null)
			{
				var asnBookingPartyAddress = receiveASN.BookingPartyDocAddress.Address;
				AssertEquals("Booking Party Address Should Match", asnBookingPartyAddress.PK, rcnBookingPartyAddress.PK);
			}

			if (expectedTransport != null)
			{
				var transports = new TransportCollection(receiveASN);
				transports.Load();
				var transport = (Transport)transports.Single();
				AssertTransport(expectedTransport, transport);
			}
			else
			{
				var transports = new TransportCollection(receiveASN);
				transports.Load();
				AssertEquals(0, transports.Count);
			}
		}

		protected static void AssertReceiveASNTransportCompany(BusinessObjectFactory factory, WhsItemReceiveASN asn, OrgAddress address)
		{
			var transportDocAddress = asn.TransportCompany;

			if (address != null)
			{
				AssertNotNull(transportDocAddress);
				AssertEquals("Transport company address type should be TRA", "TRA", transportDocAddress.E2_AddressType);
				AssertEquals("Transport company of ASN should be same as address.", address.PK, transportDocAddress.E2_OA_Address);
			}
			else
			{
				AssertEquals("Transport company of ASN should be empty.", ZGuid.Empty, transportDocAddress.E2_OA_Address);
			}
		}

		protected static void AssertReceiveTransportationUnitTransportCompany(BusinessObjectFactory factory, WhsItemReceiveTransportationUnit rtu, OrgAddress address)
		{
			var transportDocAddress = rtu.TransportCompany;

			if (address != null)
			{
				AssertNotNull(transportDocAddress);
				AssertEquals("Transport company address type should be TRA", "TRA", transportDocAddress.E2_AddressType);
				AssertEquals("Transport company of RTU should be same as address.", address.PK, transportDocAddress.E2_OA_Address);
			}
			else
			{
				AssertEquals("Transport company of RTU should be empty.", ZGuid.Empty, transportDocAddress.E2_OA_Address);
			}
		}

		protected static void AssertDispatchDTUTransportCompany(BusinessObjectFactory factory, WhsItemDispatchTransportationUnit dtu, OrgAddress address)
		{
			var addresses = factory.Load<JobDocAddress>(new ZQuery(JobDocAddressSchema.E2_ParentID, dtu.PK));
			var transportDocAddress = addresses.SingleOrDefault(doc => doc.E2_AddressType == DocAddressTypes.Codes.TransportCompanyDocumentaryAddress);

			if (address != null)
			{
				AssertNotNull(transportDocAddress);
				AssertEquals("Transport company address type should be TRA", "TRA", transportDocAddress.E2_AddressType);
				AssertEquals("Transport company of DTU should be same as address.", address.PK, transportDocAddress.E2_OA_Address);
			}
			else
			{
				AssertNull("Transport company of DTU should be null.", transportDocAddress);
			}
		}

		protected static void AssertTransport(Transport expected, Transport actual)
		{
			CombineAssertions("Transport Leg.", () =>
			{
				AssertEquals("Discharge port", expected.JW_RL_NKDiscPort, actual.JW_RL_NKDiscPort);
				AssertEquals("Load port", expected.JW_RL_NKLoadPort, actual.JW_RL_NKLoadPort);
				AssertEquals("Vessel", expected.JW_Vessel, actual.JW_Vessel);
				AssertEquals("Voyage Flight", expected.JW_VoyageFlight, actual.JW_VoyageFlight);
				AssertEquals("ETD", expected.JW_ETD, actual.JW_ETD);
				AssertEquals("ETA", expected.JW_ETA, actual.JW_ETA);
			});
		}

		protected static void AssertAdditionalReference<T>(T parentBO, CusEntryNumber[] additionalReferences, string referenceType, string expectedValue, string referenceCategory = TransitWarehouseReferenceCategories.Codes.AdditionalReference)
			where T : BusinessObject
		{
			var additionalReference = additionalReferences.SingleOrDefault(r => r.CE_EntryType == referenceType && r.CE_Category == referenceCategory);
			AssertEquals($"Expected value for {referenceCategory} reference: {referenceType} must be {expectedValue}", expectedValue, additionalReference?.CE_EntryNum ?? "");
			if (additionalReference != null)
			{
				AssertEquals($"Expected Parent Table Name must be: {parentBO.TableName}", parentBO.TableName, additionalReference.CE_ParentTable);
			}
		}

		protected static void AssertGoverningReference<T>(T parentBO, CusEntryNumber[] governingReferences, string referenceType, string expectedValue, string referenceCategory)
	where T : BusinessObject
		{
			var governingReference = governingReferences.SingleOrDefault(r => r.CE_EntryType == referenceType && r.CE_Category == referenceCategory);
			AssertEquals($"Expected value for {referenceCategory} reference: {referenceType} must be {expectedValue}", expectedValue, governingReference?.CE_EntryNum ?? "");
			if (governingReference != null)
			{
				AssertEquals($"Expected Parent Table Name must be: {parentBO.TableName}", parentBO.TableName, governingReference.CE_ParentTable);
			}
		}

		protected static void AssertAdditionalService<T>(T parentBO, JobService jobService)
		   where T : BusinessObject, IHaveServices
		{
			if (jobService != null)
			{
				var service = parentBO.Services.Cast<JobService>().FirstOrDefault(t => t.ES_ServiceCode == jobService.ES_ServiceCode && t.ES_ExternalServiceId == jobService.ES_ServiceId);
				AssertNotNull("Service is not null", service);
				AssertEquals($"Expected Booked:", service.ES_Booked, jobService.ES_Booked);
				AssertEquals($"Expected Completed:", service.ES_Completed, jobService.ES_Completed);
				AssertEquals($"Expected Contractor:", service.ES_OH_Contractor, jobService.ES_OH_Contractor);
				AssertEquals($"Expected Duration:", service.ES_Duration, jobService.ES_Duration);
				AssertEquals($"Expected Location:", service.ES_OA_Location, jobService.ES_OA_Location);
				AssertEquals($"Expected References:", service.ES_References, jobService.ES_References);
				AssertEquals($"Expected ServiceCode:", service.ES_ServiceCode, jobService.ES_ServiceCode);
				AssertEquals($"Expected ServiceCount:", service.ES_ServiceCount, jobService.ES_ServiceCount);
				AssertEquals($"Expected ServiceNote:", service.ES_ServiceNote, jobService.ES_ServiceNote);
				AssertNotNullOrEmpty($"Expected ServiceId not empty:", service.ES_ServiceId);
			}
		}

		protected static WhsItemDispatchLoadList AssertLoadList(BusinessObjectFactory factory, ZString destinationPort, ZString vessel, ZString voyageFlight, ZString etd, ZGuid parentPK, string parentTableCode, string masterBillNumber, string consolNumber, Transport expectedTransport = null)
		{
			var loadList = factory.Load<WhsItemDispatchLoadList>(new ZQuery()).Single();
			AssertEquals("Load list parent PK", parentPK, loadList.WDL_ParentID);
			AssertEquals("Load list parent code", parentTableCode, loadList.WDL_ParentTableCode);
			var additionalReferencesForDispatchLoadList = factory.Load<CusEntryNumber>(new ZQuery(CusEntryNumSchema.CE_ParentID, loadList.PK));
			AssertAdditionalReference(loadList, additionalReferencesForDispatchLoadList, WarehouseAdditionalReferenceTypes.Codes.DestinationPort, destinationPort);
			AssertAdditionalReference(loadList, additionalReferencesForDispatchLoadList, WarehouseAdditionalReferenceTypes.Codes.Vessel, vessel);
			AssertAdditionalReference(loadList, additionalReferencesForDispatchLoadList, WarehouseAdditionalReferenceTypes.Codes.VoyageFlightNumber, voyageFlight);
			AssertAdditionalReference(loadList, additionalReferencesForDispatchLoadList, WarehouseAdditionalReferenceTypes.Codes.ETDDate, etd);

			AssertAdditionalReference(loadList, additionalReferencesForDispatchLoadList, WarehouseAdditionalReferenceTypes.Codes.MasterBill, masterBillNumber);
			AssertAdditionalReference(loadList, additionalReferencesForDispatchLoadList, WarehouseAdditionalReferenceTypes.Codes.ForwardingConsolNumber, consolNumber);

			if (expectedTransport != null)
			{
				var transports = new TransportCollection(loadList);
				transports.Load();
				var transport = (Transport)transports.Single();
				AssertEquals("Discharge port", expectedTransport.JW_RL_NKDiscPort, transport.JW_RL_NKDiscPort);
				AssertEquals("Load port", expectedTransport.JW_RL_NKLoadPort, transport.JW_RL_NKLoadPort);
				AssertEquals("Vessel", expectedTransport.JW_Vessel, transport.JW_Vessel);
				AssertEquals("Voyage Flight", expectedTransport.JW_VoyageFlight, transport.JW_VoyageFlight);
				AssertEquals("ETD", expectedTransport.JW_ETD, transport.JW_ETD);
				AssertEquals("ETA", expectedTransport.JW_ETA, transport.JW_ETA);
			}

			return loadList;
		}

		// This function does not support duplicate container numbers
		protected static void AssertReceiveTransportationUnits(string message, BusinessObjectFactory factory, params ForwardingContainer[] containers)
		{
			var receiveTransportationUnits = factory.Load<WhsItemReceiveTransportationUnit>(new ZQuery());
			AssertEquals(message, containers.Length, receiveTransportationUnits.Length);
			var containerNumbers = containers.Select(c => c.JC_ContainerNum).ToArray();
			AssertContainsExactElementsInAnyOrder(containerNumbers, receiveTransportationUnits.Select(d => d.WRH_VehicleReference));

			var packageContainers = factory.Load<PkgPackageContainer>(new ZQuery());

			foreach (var container in containers)
			{
				var rtu = receiveTransportationUnits.First(r => r.WRH_VehicleReference == container.JC_ContainerNum);
				var rtuContainer = packageContainers.FirstOrDefault(c => c.Package.PackageJob.KJ_ParentID == rtu.PK);
				AssertNotEquals(message, null, rtuContainer);
				AssertEquals(message, container.JC_ContainerNum, rtuContainer.Package.GetPackageHeader().KPH_PackageID);
				AssertEquals(message, container.JC_ContainerMode, rtuContainer.K0_ContainerMode);
				AssertEquals(message, container.JC_RC, rtuContainer.K0_RC_ContainerType);
			}
		}

		protected static void AssertDispatchTransportationUnits(BusinessObjectFactory factory, string[] vehicleReferences)
		{
			var dispatchUnits = factory.Load<WhsItemDispatchTransportationUnit>(new ZQuery());
			AssertEquals($"{vehicleReferences} dispatch transport units should have been created.", vehicleReferences.Length, dispatchUnits.Length);
			AssertContainsExactElementsInAnyOrder(vehicleReferences, dispatchUnits.Select(d => d.WDH_VehicleReference));
		}

		protected static void AssertConsignmentAdditionalRefs<T>(T consignment, ZString houseBill, ZString shipmentID, ZString masterbill, string consolNumber = "", string voyageFlight = "", string etd = "") where T : BusinessObject
		{
			var additionalReferencesForReceiveConsignment = consignment.Factory.Load<CusEntryNumber>(new ZQuery(CusEntryNumSchema.CE_ParentID, consignment.PK));

			CombineAssertions("Consignment Additional References.", () =>
			{
				if (houseBill != ZString.Empty)
				{
					AssertEquals(houseBill, consignment.GetPropertyValue("HouseBillNumber"));
				}

				if (shipmentID != ZString.Empty)
				{
					AssertAdditionalReference(consignment, additionalReferencesForReceiveConsignment, WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber, shipmentID);
				}

				if (masterbill != ZString.Empty)
				{
					AssertAdditionalReference(consignment, additionalReferencesForReceiveConsignment, WarehouseAdditionalReferenceTypes.Codes.MasterBill, masterbill);
				}
				if (consolNumber != ZString.Empty)
				{
					AssertAdditionalReference(consignment, additionalReferencesForReceiveConsignment, WarehouseAdditionalReferenceTypes.Codes.ForwardingConsolNumber, consolNumber);
				}

				if (voyageFlight != ZString.Empty)
				{
					AssertAdditionalReference(consignment, additionalReferencesForReceiveConsignment, WarehouseAdditionalReferenceTypes.Codes.VoyageFlightNumber, voyageFlight);
				}

				if (etd != ZString.Empty)
				{
					AssertAdditionalReference(consignment, additionalReferencesForReceiveConsignment, WarehouseAdditionalReferenceTypes.Codes.ETDDate, etd);
				}
			});
		}

		protected static void AssertPackageState(WhsItemPackageState packageState, string unitType, bool isHandlingUnit, string packageType, int qty, ZGuid? topHandlingUnitPackage = null, bool isHighRisk = false)
		{
			packageState.Reload();
			AssertEquals(unitType, packageState.WPS_UnitType);
			AssertEquals(isHandlingUnit, packageState.WPS_IsHandlingUnit);
			AssertEquals(packageType, packageState.Package.KP_F3_NKPackType);
			AssertEquals(qty, packageState.Package.KP_PackageQty);
			AssertEquals(isHighRisk, packageState.WPS_IsHighRisk);
			if (topHandlingUnitPackage.HasValue)
			{
				AssertEquals(topHandlingUnitPackage, packageState.Package.KP_KP_TopHandlingUnitPackage);
			}
		}

		#endregion

		#region TestHelper

		protected TransportBookingTestHelper TestHelper
		{
			get { return testHelper ?? (testHelper = new TransportBookingTestHelper(Factory)); }
		}
		TransportBookingTestHelper testHelper;

		protected WhsTransitTestHelper Helper => new WhsTransitTestHelper(Factory);

		#endregion

		protected override void SetUp()
		{
			base.SetUp();
			FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		}

		protected override void TearDown()
		{
			base.TearDown();
			FreightConfigurationRegistry.Instance.EnableTransitWarehouseIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
		}
	}
}
