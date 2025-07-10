using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Module.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Integration.Customs;
using IForwardingShipmentModuleCustomColumnsAndFiltersProvider = Enterprise.Integration.Customs.IForwardingShipmentModuleCustomColumnsAndFiltersProvider;
using IGridControl = Enterprise.Integration.ZArchitecture.IGridControl;

namespace Enterprise.Freight.Forwarding.Module.Testing
{
	public class JobShipmentFilterControlBashFetchTest : FilterControlBashFetchHintTest<ForwardingModuleShipment>
	{
		#region BashFetchTest

		public void TestBashFetchForView_JS_ExportReceivingDepotReceiptRequested()
		{
			BashFetchForView(nameof(ForwardingShipment.JS_ExportReceivingDepotReceiptRequested), 0);
		}
		public void TestBashFetchForView_JS_ExportReceivingDepotDispatchRequested()
		{
			BashFetchForView(nameof(ForwardingShipment.JS_ExportReceivingDepotDispatchRequested), 0);
		}
		public void TestBashFetchForView_JS_ImportReleaseDepotReceiptRequested()
		{
			BashFetchForView(nameof(ForwardingShipment.JS_ImportReleaseDepotReceiptRequested), 0);
		}
		public void TestBashFetchForView_JS_ImportReleaseDepotDispatchRequested()
		{
			BashFetchForView(nameof(ForwardingShipment.JS_ImportReleaseDepotDispatchRequested), 0);
		}
		public void TestBashFetchForView_JS_A_RCV()
		{
			BashFetchForView(nameof(ForwardingShipment.JS_A_RCV), 0);
		}

		public void TestBashFetchForView_JS_CompanyTariffLevelOverride()
		{
			BashFetchForView(nameof(ForwardingShipment.JS_CompanyTariffLevelOverride), 0);
		}

		public void TestBashFetchForView_JS_FMCTariffID()
		{
			BashFetchForView(nameof(ForwardingShipment.JS_FMCTariffID), 0);
		}

		public void TestBashFetchForView_JS_RH_NKRateCommodity()
		{
			BashFetchForView(nameof(ForwardingShipment.JS_RH_NKRateCommodity), 0);
		}

		public void TestBashFetchForView_JS_SystemCreateUser()
		{
			BashFetchForView("JS_SystemCreateUser", 0);
		}

		public void TestBashFetchForView_JS_SystemCreateBranch()
		{
			BashFetchForView("JS_SystemCreateBranch", 0);
		}

		public void TestBashFetchForView_JS_SystemCreateDepartment()
		{
			BashFetchForView("JS_SystemCreateDepartment", 0);
		}

		public void TestBashFetchForView_JS_SystemCreateTimeUtc()
		{
			BashFetchForView("JS_SystemCreateTimeUtc", 0);
		}

		public void TestBashFetchForView_JS_SystemLastEditUser()
		{
			BashFetchForView("JS_SystemLastEditUser", 0);
		}

		public void TestBashFetchForView_JS_SystemLastEditTimeUtc()
		{
			BashFetchForView("JS_SystemLastEditTimeUtc", 0);
		}

		public void TestBashFetchForView_JS_UniqueConsignRef()
		{
			BashFetchForView("JS_UniqueConsignRef", 0);
		}

		public void TestBashFetchForView_JS_JK_ConsolID()
		{
			// JobConShipLink: 1
			// JobConsol: 1
			// JobDeclaration: 1
			// JobDocsAndCartage: 1

			BashFetchForView("JS_JK_ConsolID", 4);
		}

		public void TestBashFetchForView_CommodityCodes()
		{
			// JobPackLines: 1
			// RefCommodityCode: 1

			BashFetchForView("CommodityCodes", 2);
		}

		public void TestBashFetchForView_IsHazardous()
		{
			// JobPackLines: 1
			// RefCommodityCode: 1
			// UNDGDataItem: 1

			BashFetchForView("IsHazardous", 3);
		}

		public void TestBashFetchForView_IsPerishable()
		{
			// JobPackLines: 1
			// RefCommodityCode: 1

			BashFetchForView("IsPerishable", 2);
		}

		public void TestBashFetchForView_IsTimber()
		{
			// JobPackLines: 1
			// RefCommodityCode: 1

			BashFetchForView("IsTimber", 2);
		}

		public void TestBashFetchForView_IsFlammable()
		{
			// JobPackLines: 1
			// RefCommodityCode: 1

			BashFetchForView("IsFlammable", 2);
		}

		public void TestBashFetchForView_IsContainerVentRequired()
		{
			// JobPackLines: 1
			// RefCommodityCode: 1

			BashFetchForView("IsContainerVentRequired", 2);
		}

		public void TestBashFetchForView_NotesChecker_HasSpecialInstructions()
		{
			// JobContainerPackPivot: 2
			// JobCartage: 1
			// JobComInvoiceHeader: 1
			// JobConShipLink: 1
			// JobConsol: 1
			// JobContainer: 1
			// JobDeclaration: 1
			// JobDocAddress: 1
			// JobDocsAndCartage: 1
			// JobDocumentData: 1
			// JobOrderHeader: 1
			// JobPackLines: 1
			// OrgAddress: 1
			// OrgHeader: 1
			// StmNote: 1

			BashFetchForView("NotesChecker.HasSpecialInstructions", 16);
		}

		public void TestBashFetchForView_NotesChecker_HasGoodsHandlingInstructions()
		{
			// JobContainerPackPivot: 2
			// JobCartage: 1
			// JobComInvoiceHeader: 1
			// JobConShipLink: 1
			// JobConsol: 1
			// JobContainer: 1
			// JobDeclaration: 1
			// JobDocAddress: 1
			// JobDocsAndCartage: 1
			// JobDocumentData: 1
			// JobOrderHeader: 1
			// JobPackLines: 1
			// OrgAddress: 1
			// OrgHeader: 1
			// StmNote: 1

			BashFetchForView("NotesChecker.HasGoodsHandlingInstructions", 16);
		}

		public void TestBashFetchForView_JS_HBLContainerPackModeOverride()
		{
			// JobPackLines: 1

			BashFetchForView("JS_HBLContainerPackModeOverride", 1);
		}

		public void TestBashFetchForView_JS_IsForwardRegistered()
		{
			BashFetchForView("JS_IsForwardRegistered", 0);
		}

		public void TestBashFetchForView_JS_TransportMode()
		{
			BashFetchForView("JS_TransportMode", 0);
		}

		public void TestBashFetchForView_JS_PackingMode()
		{
			BashFetchForView("JS_PackingMode", 0);
		}

		public void TestBashFetchForView_JS_ShipmentType()
		{
			BashFetchForView("JS_ShipmentType", 0);
		}

		public void TestBashFetchForView_JS_Phase()
		{
			BashFetchForView("JS_Phase", 0);
		}

		public void TestBashFetchForView_JS_RL_NKOrigin()
		{
			BashFetchForView("JS_RL_NKOrigin", 0);
		}

		public void TestBashFetchForView_JS_RL_NKDestination()
		{
			BashFetchForView("JS_RL_NKDestination", 0);
		}

		public void TestBashFetchForView_ConsignorNameOrPK()
		{
			// JobDeclaration: 1
			// JobDocAddress: 1
			// OrgAddress: 1
			// OrgHeader: 1

			BashFetchForView("ConsignorNameOrPK", 4);
		}

		public void TestBashFetchForView_ConsignorDocumentaryAddress_E2_CompanyName()
		{
			// JobDeclaration: 1
			// JobDocAddress: 1
			// OrgAddress: 1
			// OrgHeader: 1

			BashFetchForView("ConsignorDocumentaryAddress+E2_CompanyName", 4);
		}

		public void TestBashFetchForView_ConsignorDocumentaryAddress_AddressAsASingleLine()
		{
			// OrgAddress: 2
			// JobDeclaration: 1
			// JobDocAddress: 1
			// OrgAddressAdditionalInfo: 1
			// OrgAddressCapability: 1
			// OrgHeader: 1

			BashFetchForView("ConsignorDocumentaryAddress+AddressAsASingleLine", 7);
		}

		public void TestBashFetchForView_ConsigneeNameOrPK()
		{
			// JobDeclaration: 1
			// JobDocAddress: 1
			// OrgAddress: 1
			// OrgHeader: 1

			BashFetchForView("ConsigneeNameOrPK", 4);
		}

		public void TestBashFetchForView_ConsigneeDocumentaryAddress_E2_CompanyName()
		{
			// JobDeclaration: 1
			// JobDocAddress: 1
			// OrgAddress: 1
			// OrgHeader: 1

			BashFetchForView("ConsigneeDocumentaryAddress+E2_CompanyName", 4);
		}

		public void TestBashFetchForView_ConsigneeDocumentaryAddress_AddressAsASingleLine()
		{
			// OrgAddress: 2
			// JobDeclaration: 1
			// JobDocAddress: 1
			// OrgAddressAdditionalInfo: 1
			// OrgAddressCapability: 1
			// OrgHeader: 1

			BashFetchForView("ConsigneeDocumentaryAddress+AddressAsASingleLine", 7);
		}

		public void TestBashFetchForView_JS_HouseBill()
		{
			BashFetchForView("JS_HouseBill", 0);
		}

		public void TestBashFetchForView_JS_JK_MasterBillNum()
		{
			// JobConShipLink: 1
			// JobConsol: 1
			// JobDeclaration: 1
			// JobDocsAndCartage: 1

			BashFetchForView("JS_JK_MasterBillNum", 4);
		}

		public void TestBashFetchForView_JS_ShipmentStatus()
		{
			BashFetchForView("JS_ShipmentStatus", 0);
		}

		public void TestBashFetchForView_JS_ScreeningStatus()
		{
			BashFetchForView("JS_ScreeningStatus", 0);
		}

		public void TestBashFetchForView_JS_E_DEP()
		{
			BashFetchForView("JS_E_DEP", 0);
		}

		public void TestBashFetchForView_JS_E_ARV()
		{
			BashFetchForView("JS_E_ARV", 0);
		}

		public void TestBashFetchForView_JS_JK_Vessel()
		{
			// JobSailing: 2
			// JobVoyage: 2
			// JobVoyDestination: 2
			// JobVoyOrigin: 2
			// JobConShipLink: 1
			// JobConsol: 1
			// JobConsolTransport: 1

			BashFetchForView("JS_JK_Vessel", 11);
		}

		public void TestBashFetchForView_JS_JK_VoyageFlight()
		{
			// JobSailing: 2
			// JobVoyage: 2
			// JobVoyDestination: 2
			// JobVoyOrigin: 2
			// JobConShipLink: 1
			// JobConsol: 1
			// JobConsolTransport: 1

			BashFetchForView("JS_JK_VoyageFlight", 11);
		}

		public void TestBashFetchForView_JS_GoodsDescription()
		{
			BashFetchForView("JS_GoodsDescription", 0);
		}

		public void TestBashFetchForView_JS_Calc_ActualVolumeWeight()
		{
			BashFetchForView("JS_Calc_ActualVolumeWeight", 0);
		}

		public void TestBashFetchForView_JS_Calc_ActualVolumeWeightUnit()
		{
			BashFetchForView("JS_Calc_ActualVolumeWeightUnit", 0);
		}

		public void TestBashFetchForView_JS_ActualWeight()
		{
			BashFetchForView("JS_ActualWeight", 0);
		}

		public void TestBashFetchForView_JS_UnitOfWeight()
		{
			BashFetchForView("JS_UnitOfWeight", 0);
		}

		public void TestBashFetchForView_TotalCO2e()
		{
			// JobCO2e: 1

			BashFetchForView("TotalCO2e", 1);
		}

		public void TestBashFetchForView_TotalCO2eForSorting()
		{
			// JobCO2e: 1

			BashFetchForView("TotalCO2eForSorting", 1);
		}

		public void TestBashFetchForView_CO2eStatus()
		{
			// JobCO2e: 1

			BashFetchForView("CO2eStatus", 1);
		}

		public void TestBashFetchForView_JS_ActualVolume()
		{
			BashFetchForView("JS_ActualVolume", 0);
		}

		public void TestBashFetchForView_JS_UnitOfVolume()
		{
			BashFetchForView("JS_UnitOfVolume", 0);
		}

		public void TestBashFetchForView_JS_LoadingMeters()
		{
			BashFetchForView("JS_LoadingMeters", 0);
		}

		public void TestBashFetchForView_JS_ActualChargeable()
		{
			BashFetchForView("JS_ActualChargeable", 0);
		}

		public void TestBashFetchForView_JS_ChargeableUnit()
		{
			BashFetchForView("JS_ChargeableUnit", 0);
		}

		public void TestBashFetchForView_JS_TotalPackageCount()
		{
			BashFetchForView("JS_TotalPackageCount", 0);
		}

		public void TestBashFetchForView_JS_F3_NKTotalCountPackType()
		{
			BashFetchForView("JS_F3_NKTotalCountPackType", 0);
		}

		public void TestBashFetchForView_JS_OuterPacks()
		{
			BashFetchForView("JS_OuterPacks", 0);
		}

		public void TestBashFetchForView_JS_F3_NKPackType()
		{
			BashFetchForView("JS_F3_NKPackType", 0);
		}

		public void TestBashFetchForView_NotifyPartyCompanyCode()
		{
			// JobDeclaration: 1
			// JobDocAddress: 1
			// OrgAddress: 1
			// OrgHeader: 1

			BashFetchForView("NotifyPartyCompanyCode", 4);
		}

		public void TestBashFetchForView_JS_HouseBillIssueDate()
		{
			BashFetchForView("JS_HouseBillIssueDate", 0);
		}

		public void TestBashFetchForView_JS_GoodsValue()
		{
			BashFetchForView("JS_GoodsValue", 0);
		}

		public void TestBashFetchForView_JS_RX_NKGoodsValueCurr()
		{
			BashFetchForView("JS_RX_NKGoodsValueCurr", 0);
		}

		public void TestBashFetchForView_JS_InsuranceValue()
		{
			BashFetchForView("JS_InsuranceValue", 0);
		}

		public void TestBashFetchForView_JS_RX_NKInsuranceCurrency()
		{
			BashFetchForView("JS_RX_NKInsuranceCurrency", 0);
		}

		public void TestBashFetchForView_JS_Calc_CoLoadMasterShipmentID()
		{
			BashFetchForView("JS_Calc_CoLoadMasterShipmentID", 0);
		}

		public void TestBashFetchForView_JS_JK_SendingAgent()
		{
			// JobConShipLink: 1
			// JobConsol: 1
			// JobDeclaration: 1
			// JobDocsAndCartage: 1

			BashFetchForView("JS_JK_SendingAgent", 4);
		}

		public void TestBashFetchForView_JS_JK_ReceivingAgent()
		{
			// JobConShipLink: 1
			// JobConsol: 1
			// JobDeclaration: 1
			// JobDocsAndCartage: 1

			BashFetchForView("JS_JK_ReceivingAgent", 4);
		}

		public void TestBashFetchForView_JS_JH_Branch()
		{
			// JobHeader: 1

			BashFetchForView("JS_JH_Branch", 1);
		}

		public void TestBashFetchForView_JS_JH_Dept()
		{
			// JobHeader: 1

			BashFetchForView("JS_JH_Dept", 1);
		}

		public void TestBashFetchForView_JS_GenericOrderNumbers()
		{
			// JobOrderHeader: 1
			// WhsDocketJobPivot: 1

			BashFetchForView("JS_GenericOrderNumbers", 2);
		}

		public void TestBashFetchForView_JS_Calc_ImportManifestStatus()
		{
			// CusHAWB: 12
			// CusEntryNum: 1

			BashFetchForView("JS_Calc_ImportManifestStatus", 13);
		}

		public void TestBashFetchForView_EntryNumberStatus()
		{
			BashFetchForView("EntryNumberStatus", 0);
		}

		public void TestBashFetchForView_JS_Calc_TEUCount()
		{
			// JobContainer: 12
			// JobContainerPackPivot: 12
			// JobPackLines: 1
			// RefContainer: 1

			BashFetchForView("JS_Calc_TEUCount", 26);
		}

		public void TestBashFetchForView_JS_Calc_ContainerCount()
		{
			// JobContainer: 12
			// JobContainerPackPivot: 12
			// JobPackLines: 1

			BashFetchForView("JS_Calc_ContainerCount", 25);
		}

		public void TestBashFetchForView_JS_Calc_20GPCount()
		{
			// JobContainer: 12
			// JobContainerPackPivot: 12
			// JobPackLines: 1
			// RefContainer: 1

			BashFetchForView("JS_Calc_20GPCount", 26);
		}

		public void TestBashFetchForView_JS_Calc_20RECount()
		{
			// JobContainer: 12
			// JobContainerPackPivot: 12
			// JobPackLines: 1
			// RefContainer: 1

			BashFetchForView("JS_Calc_20RECount", 26);
		}

		public void TestBashFetchForView_JS_Calc_40GPCount()
		{
			// JobContainer: 12
			// JobContainerPackPivot: 12
			// JobPackLines: 1
			// RefContainer: 1

			BashFetchForView("JS_Calc_40GPCount", 26);
		}

		public void TestBashFetchForView_JS_Calc_40RECount()
		{
			// JobContainer: 12
			// JobContainerPackPivot: 12
			// JobPackLines: 1
			// RefContainer: 1

			BashFetchForView("JS_Calc_40RECount", 26);
		}

		public void TestBashFetchForView_JS_Calc_OtherContainerCount()
		{
			// JobContainer: 12
			// JobContainerPackPivot: 12
			// JobPackLines: 1
			// RefContainer: 1

			BashFetchForView("JS_Calc_OtherContainerCount", 26);
		}

		public void TestBashFetchForView_JS_Calc_PossibleOversize()
		{
			// JobPackLines: 2
			// JobConShipLink: 1
			// JobConsol: 1
			// JobContainer: 1
			// JobContainerPackPivot: 1
			// JobDeclaration: 1
			// JobDocsAndCartage: 1

			BashFetchForView("JS_Calc_PossibleOversize", 8);
		}

		public void TestBashFetchForView_JS_BookingReference()
		{
			BashFetchForView("JS_BookingReference", 0);
		}

		public void TestBashFetchForView_JS_OrderReferences()
		{
			// JobOrderItem: 12
			// JobDocsAndCartage: 1

			BashFetchForView("JS_OrderReferences", 13);
		}

		public void TestBashFetchForView_JS_JK_MasterBillNumbers()
		{
			// JobConShipLink: 1
			// JobConsol: 12
			// JobDeclaration: 1
			// JobDocsAndCartage: 1

			BashFetchForView("JS_JK_MasterBillNumbers", 15);
		}

		public void TestBashFetchForView_JS_InterimReceipt()
		{
			BashFetchForView("JS_InterimReceipt", 0);
		}

		public void TestBashFetchForView_CustomsEntryNumber()
		{
			// CusEntryHeader: 12
			// CusInBondHeader: 1
			// CusEntryNum: 2
			// CusHAWB: 1
			// JobDeclaration: 1

			BashFetchForView("CustomsEntryNumber", 28);
		}

		public void TestBashFetchForView_CustomsEntryNumberType()
		{
			// CusEntryHeader: 12
			// CusInBondHeader: 2
			// CusEntryNum: 2
			// CusHAWB: 1
			// JobDeclaration: 1

			BashFetchForView("CustomsEntryNumberType", 28);
		}

		public void TestBashFetchForView_JS_INCO()
		{
			BashFetchForView("JS_INCO", 0);
		}

		public void TestBashFetchForView_JS_AdditionalTerms()
		{
			BashFetchForView("JS_AdditionalTerms", 0);
		}

		public void TestBashFetchForView_JS_OH_ExportBroker()
		{
			BashFetchForView("JS_OH_ExportBroker", 0);
		}

		public void TestBashFetchForView_JS_OH_ImportBroker()
		{
			BashFetchForView("JS_OH_ImportBroker", 0);
		}

		public void TestBashFetchForView_JS_OH_DeliveryAgent()
		{
			BashFetchForView("JS_OH_DeliveryAgent", 0);
		}

		public void TestBashFetchForView_DocsAndCartage_PickupCartageCoPK()
		{
			// JobDocsAndCartage: 1

			BashFetchForView("DocsAndCartage+PickupCartageCoPK", 1);
		}

		public void TestBashFetchForView_DocsAndCartage_DeliveryCartageCoPK()
		{
			// JobDocsAndCartage: 1

			BashFetchForView("DocsAndCartage+DeliveryCartageCoPK", 1);
		}

		public void TestBashFetchForView_DocsAndCartage_JP_FCLPickupEquipmentNeeded()
		{
			// JobDocsAndCartage: 1

			BashFetchForView("DocsAndCartage+JP_FCLPickupEquipmentNeeded", 1);
		}

		public void TestBashFetchForView_DocsAndCartage_JP_FCLDeliveryEquipmentNeeded()
		{
			// JobDocsAndCartage: 1

			BashFetchForView("DocsAndCartage+JP_FCLDeliveryEquipmentNeeded", 1);
		}

		public void TestBashFetchForView_JS_RS_NKServiceLevel()
		{
			BashFetchForView("JS_RS_NKServiceLevel", 0);
		}

		public void TestBashFetchForView_Job_JH_Status()
		{
			// JobHeader: 1

			BashFetchForView("Job+JH_Status", 1);
		}

		public void TestBashFetchForView_Job_JH_HoldReason()
		{
			// JobHeader: 1

			BashFetchForView("Job+JH_HoldReason", 1);
		}

		public void TestBashFetchForView_Job_JH_ProfitLossReasonCode()
		{
			// JobHeader: 1

			BashFetchForView("Job+JH_ProfitLossReasonCode", 1);
		}

		public void TestBashFetchForView_Job_JH_TotalProfitRevenueMargin()
		{
			// JobHeader: 1
			// WhsDocketJobPivot: 1

			BashFetchForView("Job+JH_TotalProfitRevenueMargin", 2);
		}

		public void TestBashFetchForView_SystemCreateUser_GS_LoginName()
		{
			// GlbStaff: 1
			// GlbBranch: 0
			// GlbCompany: 0
			// GlbDepartment: 0
			// RefCountry: 0
			// RefCurrency: 0
			// RefPackType: 0
			// RefServiceLevel: 0
			// RefUNLOCO: 0

			BashFetchForView("SystemCreateUser+GS_LoginName", 1);
		}

		public void TestBashFetchForView_SystemCreateUser_GS_Code()
		{
			BashFetchForView("SystemCreateUser+GS_Code", 1);
		}

		public void TestBashFetchForView_CustomsBroker()
		{
			// JobDeclaration: 1

			BashFetchForView("CustomsBroker", 1);
		}

		public void TestBashFetchForView_WorkflowItems_MilestonesIncludingRelated_LastMilestone_P9_SE_NKMilestoneEvent()
		{
			// AccTransactionHeader: 2
			// JobCO2e: 2
			// JobPackLines: 2
			// ProcessTasks: 2
			// AsycudaManifestHeader: 1
			// CusEntryHeader: 1
			// CusEntryNum: 1
			// CusInBondHeader: 1
			// CusInBondMoveHeader: 1
			// CusSCAHouse: 1
			// DtbBookingConsolidation: 1
			// JobCartage: 1
			// JobComInvoiceHeader: 1
			// JobConShipLink: 1
			// JobConsol: 1
			// JobConsolTransport: 1
			// JobContainer: 1
			// JobContainerPackPivot: 1
			// JobDeclaration: 1
			// JobDocsAndCartage: 1
			// JobDocumentData: 1
			// JobHeader: 1
			// JobPickupDeliveryConfirm: 1
			// JobSailing: 1
			// JobVoyage: 1
			// JobVoyDestination: 1
			// JobVoyOrigin: 1
			// WhsDocket: 1
			// WhsDocketJobPivot: 1

			BashFetchForView("WorkflowItems+MilestonesIncludingRelated+LastMilestone+P9_SE_NKMilestoneEvent", 33);
		}

		public void TestBashFetchForView_WorkflowItems_MilestonesIncludingRelated_LastMilestone_DescriptionWithReference()
		{
			// AccTransactionHeader: 2
			// JobCO2e: 2
			// JobPackLines: 2
			// ProcessTasks: 2
			// AsycudaManifestHeader: 1
			// CusEntryHeader: 1
			// CusEntryNum: 1
			// CusInBondHeader: 1
			// CusInBondMoveHeader: 1
			// CusSCAHouse: 1
			// DtbBookingConsolidation: 1
			// JobCartage: 1
			// JobComInvoiceHeader: 1
			// JobConShipLink: 1
			// JobConsol: 1
			// JobConsolTransport: 1
			// JobContainer: 1
			// JobContainerPackPivot: 1
			// JobDeclaration: 1
			// JobDocsAndCartage: 1
			// JobDocumentData: 1
			// JobHeader: 1
			// JobPickupDeliveryConfirm: 1
			// JobSailing: 1
			// JobVoyage: 1
			// JobVoyDestination: 1
			// JobVoyOrigin: 1
			// WhsDocket: 1
			// WhsDocketJobPivot: 1

			BashFetchForView("WorkflowItems+MilestonesIncludingRelated+LastMilestone+DescriptionWithReference", 33);
		}

		public void TestBashFetchForView_WorkflowItems_MilestonesIncludingRelated_LastMilestone_P9_ActualDateForBinding()
		{
			// AccTransactionHeader: 2
			// JobCO2e: 2
			// JobPackLines: 2
			// ProcessTasks: 2
			// AsycudaManifestHeader: 1
			// CusEntryHeader: 1
			// CusEntryNum: 1
			// CusInBondHeader: 1
			// CusInBondMoveHeader: 1
			// CusSCAHouse: 1
			// DtbBookingConsolidation: 1
			// JobCartage: 1
			// JobComInvoiceHeader: 1
			// JobConShipLink: 1
			// JobConsol: 1
			// JobConsolTransport: 1
			// JobContainer: 1
			// JobContainerPackPivot: 1
			// JobDeclaration: 1
			// JobDocsAndCartage: 1
			// JobDocumentData: 1
			// JobHeader: 1
			// JobPickupDeliveryConfirm: 1
			// JobSailing: 1
			// JobVoyage: 1
			// JobVoyDestination: 1
			// JobVoyOrigin: 1
			// WhsDocket: 1
			// WhsDocketJobPivot: 1

			BashFetchForView("WorkflowItems+MilestonesIncludingRelated+LastMilestone+P9_ActualDateForBinding", 33);
		}

		public void TestBashFetchForView_WorkflowItems_MilestonesIncludingRelated_NextMilestone_P9_SE_NKMilestoneEvent()
		{
			// AccTransactionHeader: 2
			// JobCO2e: 2
			// JobPackLines: 2
			// ProcessTasks: 2
			// AsycudaManifestHeader: 1
			// CusEntryHeader: 1
			// CusEntryNum: 1
			// CusInBondHeader: 1
			// CusInBondMoveHeader: 1
			// CusSCAHouse: 1
			// DtbBookingConsolidation: 1
			// JobCartage: 1
			// JobComInvoiceHeader: 1
			// JobConShipLink: 1
			// JobConsol: 1
			// JobConsolTransport: 1
			// JobContainer: 1
			// JobContainerPackPivot: 1
			// JobDeclaration: 1
			// JobDocsAndCartage: 1
			// JobDocumentData: 1
			// JobHeader: 1
			// JobPickupDeliveryConfirm: 1
			// JobSailing: 1
			// JobVoyage: 1
			// JobVoyDestination: 1
			// JobVoyOrigin: 1
			// WhsDocket: 1
			// WhsDocketJobPivot: 1

			BashFetchForView("WorkflowItems+MilestonesIncludingRelated+NextMilestone+P9_SE_NKMilestoneEvent", 33);
		}

		public void TestBashFetchForView_WorkflowItems_MilestonesIncludingRelated_NextMilestone_DescriptionWithReference()
		{
			// AccTransactionHeader: 2
			// JobCO2e: 2
			// JobPackLines: 2
			// ProcessTasks: 2
			// AsycudaManifestHeader: 1
			// CusEntryHeader: 1
			// CusEntryNum: 1
			// CusInBondHeader: 1
			// CusInBondMoveHeader: 1
			// CusSCAHouse: 1
			// DtbBookingConsolidation: 1
			// JobCartage: 1
			// JobComInvoiceHeader: 1
			// JobConShipLink: 1
			// JobConsol: 1
			// JobConsolTransport: 1
			// JobContainer: 1
			// JobContainerPackPivot: 1
			// JobDeclaration: 1
			// JobDocsAndCartage: 1
			// JobDocumentData: 1
			// JobHeader: 1
			// JobPickupDeliveryConfirm: 1
			// JobSailing: 1
			// JobVoyage: 1
			// JobVoyDestination: 1
			// JobVoyOrigin: 1
			// WhsDocket: 1
			// WhsDocketJobPivot: 1

			BashFetchForView("WorkflowItems+MilestonesIncludingRelated+NextMilestone+DescriptionWithReference", 33);
		}

		public void TestBashFetchForView_WorkflowItems_MilestonesIncludingRelated_NextMilestone_P9_ScheduledDateForBinding()
		{
			// AccTransactionHeader: 2
			// JobCO2e: 2
			// JobPackLines: 2
			// ProcessTasks: 2
			// AsycudaManifestHeader: 1
			// CusEntryHeader: 1
			// CusEntryNum: 1
			// CusInBondHeader: 1
			// CusInBondMoveHeader: 1
			// CusSCAHouse: 1
			// DtbBookingConsolidation: 1
			// JobCartage: 1
			// JobComInvoiceHeader: 1
			// JobConShipLink: 1
			// JobConsol: 1
			// JobConsolTransport: 1
			// JobContainer: 1
			// JobContainerPackPivot: 1
			// JobDeclaration: 1
			// JobDocsAndCartage: 1
			// JobDocumentData: 1
			// JobHeader: 1
			// JobPickupDeliveryConfirm: 1
			// JobSailing: 1
			// JobVoyage: 1
			// JobVoyDestination: 1
			// JobVoyOrigin: 1
			// WhsDocket: 1
			// WhsDocketJobPivot: 1

			BashFetchForView("WorkflowItems+MilestonesIncludingRelated+NextMilestone+P9_ScheduledDateForBinding", 33);
		}

		public void TestBashFetchForView_WorkflowItems_Milestones_LastMilestone_P9_SE_NKMilestoneEvent()
		{
			// ProcessTasks: 1

			BashFetchForView("WorkflowItems+Milestones+LastMilestone+P9_SE_NKMilestoneEvent", 1);
		}

		public void TestBashFetchForView_WorkflowItems_Milestones_LastMilestone_DescriptionWithReference()
		{
			// ProcessTasks: 1

			BashFetchForView("WorkflowItems+Milestones+LastMilestone+DescriptionWithReference", 1);
		}

		public void TestBashFetchForView_WorkflowItems_Milestones_LastMilestone_P9_ActualDateForBinding()
		{
			// ProcessTasks: 1

			BashFetchForView("WorkflowItems+Milestones+LastMilestone+P9_ActualDateForBinding", 1);
		}

		public void TestBashFetchForView_WorkflowItems_Milestones_NextMilestone_P9_SE_NKMilestoneEvent()
		{
			// ProcessTasks: 1

			BashFetchForView("WorkflowItems+Milestones+NextMilestone+P9_SE_NKMilestoneEvent", 1);
		}

		public void TestBashFetchForView_WorkflowItems_Milestones_NextMilestone_DescriptionWithReference()
		{
			// ProcessTasks: 1

			BashFetchForView("WorkflowItems+Milestones+NextMilestone+DescriptionWithReference", 1);
		}

		public void TestBashFetchForView_WorkflowItems_Milestones_NextMilestone_P9_ScheduledDateForBinding()
		{
			// ProcessTasks: 1

			BashFetchForView("WorkflowItems+Milestones+NextMilestone+P9_ScheduledDateForBinding", 1);
		}

		public void TestBashFetchForView_WorkflowItems_Tasks_NextTask_P9_Description()
		{
			// ProcessTasks: 1

			BashFetchForView("WorkflowItems+Tasks+NextTask+P9_Description", 1);
		}

		public void TestBashFetchForView_StorageLocation()
		{
			BashFetchForView("StorageLocation", 0);
		}

		public void TestBashFetchForView_DocsAndCartage_JP_EstimatedPickup()
		{
			// JobDocsAndCartage: 1

			BashFetchForView("DocsAndCartage+JP_EstimatedPickup", 1);
		}

		public void TestBashFetchForView_DocsAndCartage_JP_PickupRequiredBy()
		{
			// JobDocsAndCartage: 1

			BashFetchForView("DocsAndCartage+JP_PickupRequiredBy", 1);
		}

		public void TestBashFetchForView_DocsAndCartage_JP_EstimatedDelivery()
		{
			// JobDocsAndCartage: 1

			BashFetchForView("DocsAndCartage+JP_EstimatedDelivery", 1);
		}

		public void TestBashFetchForView_DocsAndCartage_JP_DeliveryRequiredBy()
		{
			// JobDocsAndCartage: 1

			BashFetchForView("DocsAndCartage+JP_DeliveryRequiredBy", 1);
		}

		public void TestBashFetchForView_JS_CFSReference()
		{
			BashFetchForView("JS_CFSReference", 0);
		}

		public void TestBashFetchForView_DocsAndCartage_JP_DeliveryCartageCompleted()
		{
			// JobDocsAndCartage: 1

			BashFetchForView("DocsAndCartage+JP_DeliveryCartageCompleted", 1);
		}

		public void TestBashFetchForView_DeliveryGoodsSignedForBy()
		{
			// JobPackLines: 2
			// JobPickupDeliveryConfirm: 2
			// JobConShipLink: 1
			// JobConsol: 1
			// JobContainer: 1
			// JobContainerPackPivot: 1
			// JobDeclaration: 1
			// JobDocsAndCartage: 1

			BashFetchForView("DeliveryGoodsSignedForBy", 10);
		}

		public void TestBashFetchForView_PickupGoodsSignedForBy()
		{
			// JobPackLines: 2
			// JobPickupDeliveryConfirm: 2
			// JobConShipLink: 1
			// JobConsol: 1
			// JobContainer: 1
			// JobContainerPackPivot: 1
			// JobDeclaration: 1
			// JobDocsAndCartage: 1

			BashFetchForView("PickupGoodsSignedForBy", 10);
		}

		public void TestBashFetchForView_DocsAndCartage_JP_PickupCartageCompleted()
		{
			// JobDocsAndCartage: 1

			BashFetchForView("DocsAndCartage+JP_PickupCartageCompleted", 1);
		}

		public void TestBashFetchForView_ConsigneeDocumentaryAddress_E2_AddressOverride()
		{
			// JobDeclaration: 1
			// JobDocAddress: 1
			// OrgAddress: 1
			// OrgHeader: 1

			BashFetchForView("ConsigneeDocumentaryAddress+E2_AddressOverride", 4);
		}

		public void TestBashFetchForView_ConsigneeDocumentaryAddress_E2_Address1()
		{
			// JobDeclaration: 1
			// JobDocAddress: 1
			// OrgAddress: 1
			// OrgHeader: 1

			BashFetchForView("ConsigneeDocumentaryAddress+E2_Address1", 4);
		}

		public void TestBashFetchForView_ConsigneeDocumentaryAddress_E2_Address2()
		{
			// JobDeclaration: 1
			// JobDocAddress: 1
			// OrgAddress: 1
			// OrgHeader: 1

			BashFetchForView("ConsigneeDocumentaryAddress+E2_Address2", 4);
		}

		public void TestBashFetchForView_ConsigneeDocumentaryAddress_E2_City()
		{
			// JobDeclaration: 1
			// JobDocAddress: 1
			// OrgAddress: 1
			// OrgHeader: 1

			BashFetchForView("ConsigneeDocumentaryAddress+E2_City", 4);
		}

		public void TestBashFetchForView_ConsigneeDocumentaryAddress_E2_State()
		{
			// JobDeclaration: 1
			// JobDocAddress: 1
			// OrgAddress: 1
			// OrgHeader: 1

			BashFetchForView("ConsigneeDocumentaryAddress+E2_State", 4);
		}

		public void TestBashFetchForView_ConsigneeDocumentaryAddress_E2_RN_NKCountryCode()
		{
			// OrgAddress: 2
			// JobDeclaration: 1
			// JobDocAddress: 1
			// OrgAddressCapability: 1
			// OrgHeader: 1

			BashFetchForView("ConsigneeDocumentaryAddress+E2_RN_NKCountryCode", 6);
		}

		public void TestBashFetchForView_ConsignorDocumentaryAddress_E2_AddressOverride()
		{
			// JobDeclaration: 1
			// JobDocAddress: 1
			// OrgAddress: 1
			// OrgHeader: 1

			BashFetchForView("ConsignorDocumentaryAddress+E2_AddressOverride", 4);
		}

		public void TestBashFetchForView_ConsignorDocumentaryAddress_E2_Address1()
		{
			// JobDeclaration: 1
			// JobDocAddress: 1
			// OrgAddress: 1
			// OrgHeader: 1

			BashFetchForView("ConsignorDocumentaryAddress+E2_Address1", 4);
		}

		public void TestBashFetchForView_ConsignorDocumentaryAddress_E2_Address2()
		{
			// JobDeclaration: 1
			// JobDocAddress: 1
			// OrgAddress: 1
			// OrgHeader: 1

			BashFetchForView("ConsignorDocumentaryAddress+E2_Address2", 4);
		}

		public void TestBashFetchForView_ConsignorDocumentaryAddress_E2_City()
		{
			// JobDeclaration: 1
			// JobDocAddress: 1
			// OrgAddress: 1
			// OrgHeader: 1

			BashFetchForView("ConsignorDocumentaryAddress+E2_City", 4);
		}

		public void TestBashFetchForView_ConsignorDocumentaryAddress_E2_State()
		{
			// JobDeclaration: 1
			// JobDocAddress: 1
			// OrgAddress: 1
			// OrgHeader: 1

			BashFetchForView("ConsignorDocumentaryAddress+E2_State", 4);
		}

		public void TestBashFetchForView_ConsignorDocumentaryAddress_E2_RN_NKCountryCode()
		{
			// OrgAddress: 2
			// JobDeclaration: 1
			// JobDocAddress: 1
			// OrgAddressCapability: 1
			// OrgHeader: 1

			BashFetchForView("ConsignorDocumentaryAddress+E2_RN_NKCountryCode", 6);
		}

		public void TestBashFetchForView_Job_LocalCharges_OH_Code()
		{
			// JobHeader: 1
			// OrgAddress: 1
			// OrgHeader: 1

			BashFetchForView("Job+LocalCharges+OH_Code", 3);
		}

		public void TestBashFetchForView_Job_LocalCharges_OH_FullName()
		{
			// JobHeader: 1
			// OrgAddress: 1
			// OrgHeader: 1

			BashFetchForView("Job+LocalCharges+OH_FullName", 3);
		}

		public void TestBashFetchForView_Job_LocalChargesAddr_AddressAsASingleLine()
		{
			// OrgAddress: 2
			// JobHeader: 1
			// OrgAddressAdditionalInfo: 1
			// OrgAddressCapability: 1
			// OrgHeader: 1

			BashFetchForView("Job+LocalChargesAddr+AddressAsASingleLine", 6);
		}

		public void TestBashFetchForView_Job_LocalChargesAddr_OA_Address1()
		{
			// JobHeader: 1
			// OrgAddress: 1

			BashFetchForView("Job+LocalChargesAddr+OA_Address1", 2);
		}

		public void TestBashFetchForView_Job_LocalChargesAddr_OA_Address2()
		{
			// JobHeader: 1
			// OrgAddress: 1

			BashFetchForView("Job+LocalChargesAddr+OA_Address2", 2);
		}

		public void TestBashFetchForView_Job_LocalChargesAddr_CityFallback()
		{
			// JobHeader: 1
			// OrgAddress: 1
			// OrgHeader: 1

			BashFetchForView("Job+LocalChargesAddr+CityFallback", 3);
		}

		public void TestBashFetchForView_Job_LocalChargesAddr_OA_State()
		{
			// JobHeader: 1
			// OrgAddress: 1

			BashFetchForView("Job+LocalChargesAddr+OA_State", 2);
		}

		public void TestBashFetchForView_Job_LocalChargesAddr_RelatedCountry_RN_Code()
		{
			// JobHeader: 1
			// OrgAddress: 1
			// RefCountry: 1

			BashFetchForView("Job+LocalChargesAddr+RelatedCountry+RN_Code", 3);
		}

		public void TestBashFetchForView_Job_AgentCollect_OH_Code()
		{
			// JobHeader: 1
			// OrgAddress: 1
			// OrgHeader: 1

			BashFetchForView("Job+AgentCollect+OH_Code", 3);
		}

		public void TestBashFetchForView_Job_AgentCollect_OH_FullName()
		{
			// JobHeader: 1
			// OrgAddress: 1
			// OrgHeader: 1

			BashFetchForView("Job+AgentCollect+OH_FullName", 3);
		}

		public void TestBashFetchForView_Job_AgentCollectAddr_AddressAsASingleLine()
		{
			// OrgAddress: 2
			// JobHeader: 1
			// OrgAddressAdditionalInfo: 1
			// OrgAddressCapability: 1
			// OrgHeader: 1

			BashFetchForView("Job+AgentCollectAddr+AddressAsASingleLine", 6);
		}

		public void TestBashFetchForView_Job_AgentCollectAddr_OA_Address1()
		{
			// JobHeader: 1
			// OrgAddress: 1

			BashFetchForView("Job+AgentCollectAddr+OA_Address1", 2);
		}

		public void TestBashFetchForView_Job_AgentCollectAddr_OA_Address2()
		{
			// JobHeader: 1
			// OrgAddress: 1

			BashFetchForView("Job+AgentCollectAddr+OA_Address2", 2);
		}

		public void TestBashFetchForView_Job_AgentCollectAddr_CityFallback()
		{
			// JobHeader: 1
			// OrgAddress: 1
			// OrgHeader: 1

			BashFetchForView("Job+AgentCollectAddr+CityFallback", 3);
		}

		public void TestBashFetchForView_Job_AgentCollectAddr_OA_State()
		{
			// JobHeader: 1
			// OrgAddress: 1

			BashFetchForView("Job+AgentCollectAddr+OA_State", 2);
		}

		public void TestBashFetchForView_Job_AgentCollectAddr_RelatedCountry_RN_Code()
		{
			// JobHeader: 1
			// OrgAddress: 1
			// RefCountry: 1

			BashFetchForView("Job+AgentCollectAddr+RelatedCountry+RN_Code", 3);
		}

		public void TestBashFetchForView_ISFBillStatusDescription()
		{
			BashFetchForView("ISFBillStatusDescription", 0);
		}

		public void TestBashFetchForView_Job_JH_GS_NKRepSales()
		{
			// JobHeader: 1

			BashFetchForView("Job+JH_GS_NKRepSales", 1);
		}

		public void TestBashFetchForView_Job_JH_GS_NKRepOps()
		{
			// JobHeader: 1

			BashFetchForView("Job+JH_GS_NKRepOps", 1);
		}

		public void TestBashFetchForView_ExportReceivingDepot_Header_OH_Code()
		{
			// OrgAddress: 1
			// OrgHeader: 1

			BashFetchForView("ExportReceivingDepot+Header+OH_Code", 2);
		}

		public void TestBashFetchForView_ImportReleaseDepot_Header_OH_Code()
		{
			// OrgAddress: 1
			// OrgHeader: 1

			BashFetchForView("ImportReleaseDepot+Header+OH_Code", 2);
		}

		public void TestBashFetchForView_JS_InspectionTypeCode()
		{
			// CusEntryNum: 1

			BashFetchForView("JS_InspectionTypeCode", 1);
		}

		public void TestBashFetchForView_Job_LocalChargesAddr_OA_Code()
		{
			// JobHeader: 1
			// OrgAddress: 1

			BashFetchForView("Job+LocalChargesAddr+OA_Code", 2);
		}

		public void TestBashFetchForView_JS_ReleaseType()
		{
			BashFetchForView("JS_ReleaseType", 0);
		}

		public void TestBashFetchForView_JS_HBLAWBChargesDisplay()
		{
			BashFetchForView("JS_HBLAWBChargesDisplay", 0);
		}

		public void TestBashFetchForView_PickupAgentCompanyCode()
		{
			// JobDeclaration: 1
			// JobDocAddress: 1

			BashFetchForView("PickupAgentCompanyCode", 2);
		}

		public void TestBashFetchForView_ControllingAgent_OH_Code()
		{
			// JobDeclaration: 1
			// JobDocAddress: 1
			// OrgAddress: 1
			// OrgHeader: 1

			BashFetchForView("ControllingAgent+OH_Code", 4);
		}

		public void TestBashFetchForView_ControllingCustomer_OH_Code()
		{
			// JobDeclaration: 1
			// JobDocAddress: 1
			// OrgAddress: 1
			// OrgHeader: 1

			BashFetchForView("ControllingCustomer+OH_Code", 4);
		}

		public void TestBashFetchForView_JS_EFreightStatus()
		{
			BashFetchForView("JS_EFreightStatus", 0);
		}

		public void TestBashFetchForView_NumbersAsString()
		{
			// CusEntryNum: 1

			BashFetchForView("NumbersAsString", 1);
		}

		public void TestBashFetchForView_WorkflowItems_MilestonesIncludingRelated_CurrentCompanyLastMilestone_P9_SE_NKMilestoneEvent()
		{
			// AccTransactionHeader: 2
			// JobCO2e: 2
			// JobPackLines: 2
			// ProcessTasks: 2
			// AsycudaManifestHeader: 1
			// CusEntryHeader: 1
			// CusEntryNum: 1
			// CusInBondHeader: 1
			// CusInBondMoveHeader: 1
			// CusSCAHouse: 1
			// DtbBookingConsolidation: 1
			// JobCartage: 1
			// JobComInvoiceHeader: 1
			// JobConShipLink: 1
			// JobConsol: 1
			// JobConsolTransport: 1
			// JobContainer: 1
			// JobContainerPackPivot: 1
			// JobDeclaration: 1
			// JobDocsAndCartage: 1
			// JobDocumentData: 1
			// JobHeader: 1
			// JobPickupDeliveryConfirm: 1
			// JobSailing: 1
			// JobVoyage: 1
			// JobVoyDestination: 1
			// JobVoyOrigin: 1
			// WhsDocket: 1
			// WhsDocketJobPivot: 1

			BashFetchForView("WorkflowItems+MilestonesIncludingRelated+CurrentCompanyLastMilestone+P9_SE_NKMilestoneEvent", 33);
		}

		public void TestBashFetchForView_WorkflowItems_MilestonesIncludingRelated_CurrentCompanyLastMilestone_DescriptionWithReference()
		{
			// AccTransactionHeader: 2
			// JobCO2e: 2
			// JobPackLines: 2
			// ProcessTasks: 2
			// AsycudaManifestHeader: 1
			// CusEntryHeader: 1
			// CusEntryNum: 1
			// CusInBondHeader: 1
			// CusInBondMoveHeader: 1
			// CusSCAHouse: 1
			// DtbBookingConsolidation: 1
			// JobCartage: 1
			// JobComInvoiceHeader: 1
			// JobConShipLink: 1
			// JobConsol: 1
			// JobConsolTransport: 1
			// JobContainer: 1
			// JobContainerPackPivot: 1
			// JobDeclaration: 1
			// JobDocsAndCartage: 1
			// JobDocumentData: 1
			// JobHeader: 1
			// JobPickupDeliveryConfirm: 1
			// JobSailing: 1
			// JobVoyage: 1
			// JobVoyDestination: 1
			// JobVoyOrigin: 1
			// WhsDocket: 1
			// WhsDocketJobPivot: 1

			BashFetchForView("WorkflowItems+MilestonesIncludingRelated+CurrentCompanyLastMilestone+DescriptionWithReference", 33);
		}

		public void TestBashFetchForView_WorkflowItems_MilestonesIncludingRelated_CurrentCompanyLastMilestone_P9_ActualDateForBinding()
		{
			// AccTransactionHeader: 2
			// JobCO2e: 2
			// JobPackLines: 2
			// ProcessTasks: 2
			// AsycudaManifestHeader: 1
			// CusEntryHeader: 1
			// CusEntryNum: 1
			// CusInBondHeader: 1
			// CusInBondMoveHeader: 1
			// CusSCAHouse: 1
			// DtbBookingConsolidation: 1
			// JobCartage: 1
			// JobComInvoiceHeader: 1
			// JobConShipLink: 1
			// JobConsol: 1
			// JobConsolTransport: 1
			// JobContainer: 1
			// JobContainerPackPivot: 1
			// JobDeclaration: 1
			// JobDocsAndCartage: 1
			// JobDocumentData: 1
			// JobHeader: 1
			// JobPickupDeliveryConfirm: 1
			// JobSailing: 1
			// JobVoyage: 1
			// JobVoyDestination: 1
			// JobVoyOrigin: 1
			// WhsDocket: 1
			// WhsDocketJobPivot: 1

			BashFetchForView("WorkflowItems+MilestonesIncludingRelated+CurrentCompanyLastMilestone+P9_ActualDateForBinding", 33);
		}

		public void TestBashFetchForView_WorkflowItems_Milestones_CurrentCompanyLastMilestone_P9_SE_NKMilestoneEvent()
		{
			// ProcessTasks: 1

			BashFetchForView("WorkflowItems+Milestones+CurrentCompanyLastMilestone+P9_SE_NKMilestoneEvent", 1);
		}

		public void TestBashFetchForView_WorkflowItems_Milestones_CurrentCompanyLastMilestone_DescriptionWithReference()
		{
			// ProcessTasks: 1

			BashFetchForView("WorkflowItems+Milestones+CurrentCompanyLastMilestone+DescriptionWithReference", 1);
		}

		public void TestBashFetchForView_WorkflowItems_Milestones_CurrentCompanyLastMilestone_P9_ActualDateForBinding()
		{
			// ProcessTasks: 1

			BashFetchForView("WorkflowItems+Milestones+CurrentCompanyLastMilestone+P9_ActualDateForBinding", 1);
		}

		public void TestBashFetchForView_WorkflowItems_MilestonesIncludingRelated_CurrentCompanyNextMilestone_P9_SE_NKMilestoneEvent()
		{
			// AccTransactionHeader: 2
			// JobCO2e: 2
			// JobPackLines: 2
			// ProcessTasks: 2
			// AsycudaManifestHeader: 1
			// CusEntryHeader: 1
			// CusEntryNum: 1
			// CusInBondHeader: 1
			// CusInBondMoveHeader: 1
			// CusSCAHouse: 1
			// DtbBookingConsolidation: 1
			// JobCartage: 1
			// JobComInvoiceHeader: 1
			// JobConShipLink: 1
			// JobConsol: 1
			// JobConsolTransport: 1
			// JobContainer: 1
			// JobContainerPackPivot: 1
			// JobDeclaration: 1
			// JobDocsAndCartage: 1
			// JobHeader: 1
			// JobPickupDeliveryConfirm: 1
			// JobSailing: 1
			// JobVoyage: 1
			// JobVoyDestination: 1
			// JobVoyOrigin: 1
			// WhsDocket: 1
			// WhsDocketJobPivot: 1

			BashFetchForView("WorkflowItems+MilestonesIncludingRelated+CurrentCompanyNextMilestone+P9_SE_NKMilestoneEvent", 33);
		}

		public void TestBashFetchForView_WorkflowItems_MilestonesIncludingRelated_CurrentCompanyNextMilestone_DescriptionWithReference()
		{
			// AccTransactionHeader: 2
			// JobCO2e: 2
			// JobPackLines: 2
			// ProcessTasks: 2
			// AsycudaManifestHeader: 1
			// CusEntryHeader: 1
			// CusEntryNum: 1
			// CusInBondHeader: 1
			// CusInBondMoveHeader: 1
			// CusSCAHouse: 1
			// DtbBookingConsolidation: 1
			// JobCartage: 1
			// JobComInvoiceHeader: 1
			// JobConShipLink: 1
			// JobConsol: 1
			// JobConsolTransport: 1
			// JobContainer: 1
			// JobContainerPackPivot: 1
			// JobDeclaration: 1
			// JobDocsAndCartage: 1
			// JobDocumentData: 1
			// JobHeader: 1
			// JobPickupDeliveryConfirm: 1
			// JobSailing: 1
			// JobVoyage: 1
			// JobVoyDestination: 1
			// JobVoyOrigin: 1
			// WhsDocket: 1
			// WhsDocketJobPivot: 1

			BashFetchForView("WorkflowItems+MilestonesIncludingRelated+CurrentCompanyNextMilestone+DescriptionWithReference", 33);
		}

		public void TestBashFetchForView_WorkflowItems_MilestonesIncludingRelated_CurrentCompanyNextMilestone_P9_ScheduledDateForBinding()
		{
			// AccTransactionHeader: 2
			// JobCO2e: 2
			// JobPackLines: 2
			// ProcessTasks: 2
			// AsycudaManifestHeader: 1
			// CusEntryHeader: 1
			// CusEntryNum: 1
			// CusInBondHeader: 1
			// CusInBondMoveHeader: 1
			// CusSCAHouse: 1
			// DtbBookingConsolidation: 1
			// JobCartage: 1
			// JobComInvoiceHeader: 1
			// JobConShipLink: 1
			// JobConsol: 1
			// JobConsolTransport: 1
			// JobContainer: 1
			// JobContainerPackPivot: 1
			// JobDeclaration: 1
			// JobDocsAndCartage: 1
			// JobDocumentData: 1
			// JobHeader: 1
			// JobPickupDeliveryConfirm: 1
			// JobSailing: 1
			// JobVoyage: 1
			// JobVoyDestination: 1
			// JobVoyOrigin: 1
			// WhsDocket: 1
			// WhsDocketJobPivot: 1

			BashFetchForView("WorkflowItems+MilestonesIncludingRelated+CurrentCompanyNextMilestone+P9_ScheduledDateForBinding", 33);
		}

		public void TestBashFetchForView_WorkflowItems_Milestones_CurrentCompanyNextMilestone_P9_SE_NKMilestoneEvent()
		{
			// ProcessTasks: 1

			BashFetchForView("WorkflowItems+Milestones+CurrentCompanyNextMilestone+P9_SE_NKMilestoneEvent", 1);
		}

		public void TestBashFetchForView_WorkflowItems_Milestones_CurrentCompanyNextMilestone_DescriptionWithReference()
		{
			// ProcessTasks: 1

			BashFetchForView("WorkflowItems+Milestones+CurrentCompanyNextMilestone+DescriptionWithReference", 1);
		}

		public void TestBashFetchForView_WorkflowItems_Milestones_CurrentCompanyNextMilestone_P9_ScheduledDateForBinding()
		{
			// ProcessTasks: 1

			BashFetchForView("WorkflowItems+Milestones+CurrentCompanyNextMilestone+P9_ScheduledDateForBinding", 1);
		}

		public void TestBashFetchForView_ConsignorPickupAddress_E2_CompanyName()
		{
			// JobDeclaration: 1
			// JobDocAddress: 1
			// OrgAddress: 1
			// OrgHeader: 1

			BashFetchForView("ConsignorPickupAddress+E2_CompanyName", 4);
		}

		public void TestBashFetchForView_ConsignorPickupAddress_AddressAsASingleLine()
		{
			// OrgAddress: 2
			// JobDeclaration: 1
			// JobDocAddress: 1
			// OrgAddressAdditionalInfo: 1
			// OrgAddressCapability: 1
			// OrgHeader: 1

			BashFetchForView("ConsignorPickupAddress+AddressAsASingleLine", 7);
		}

		public void TestBashFetchForView_ConsignorPickupAddress_E2_Address1()
		{
			// JobDeclaration: 1
			// JobDocAddress: 1
			// OrgAddress: 1
			// OrgHeader: 1

			BashFetchForView("ConsignorPickupAddress+E2_Address1", 4);
		}

		public void TestBashFetchForView_ConsignorPickupAddress_E2_Address2()
		{
			// JobDeclaration: 1
			// JobDocAddress: 1
			// OrgAddress: 1
			// OrgHeader: 1

			BashFetchForView("ConsignorPickupAddress+E2_Address2", 4);
		}

		public void TestBashFetchForView_ConsignorPickupAddress_E2_City()
		{
			// JobDeclaration: 1
			// JobDocAddress: 1
			// OrgAddress: 1
			// OrgHeader: 1

			BashFetchForView("ConsignorPickupAddress+E2_City", 4);
		}

		public void TestBashFetchForView_ConsignorPickupAddress_E2_State()
		{
			// JobDeclaration: 1
			// JobDocAddress: 1
			// OrgAddress: 1
			// OrgHeader: 1

			BashFetchForView("ConsignorPickupAddress+E2_State", 4);
		}

		public void TestBashFetchForView_ConsignorPickupAddress_E2_RN_NKCountryCode()
		{
			// OrgAddress: 2
			// JobDeclaration: 1
			// JobDocAddress: 1
			// OrgAddressCapability: 1
			// OrgHeader: 1

			BashFetchForView("ConsignorPickupAddress+E2_RN_NKCountryCode", 6);
		}

		public void TestBashFetchForView_ConsignorPickupAddress_E2_Postcode()
		{
			// JobDeclaration: 1
			// JobDocAddress: 1
			// OrgAddress: 1
			// OrgHeader: 1

			BashFetchForView("ConsignorPickupAddress+E2_Postcode", 4);
		}

		public void TestBashFetchForView_ConsigneeDeliveryAddress_E2_CompanyName()
		{
			// JobDeclaration: 1
			// JobDocAddress: 1
			// OrgAddress: 1
			// OrgHeader: 1

			BashFetchForView("ConsigneeDeliveryAddress+E2_CompanyName", 4);
		}

		public void TestBashFetchForView_ConsigneeDeliveryAddress_AddressAsASingleLine()
		{
			// OrgAddress: 2
			// JobDeclaration: 1
			// JobDocAddress: 1
			// OrgAddressAdditionalInfo: 1
			// OrgAddressCapability: 1
			// OrgHeader: 1

			BashFetchForView("ConsigneeDeliveryAddress+AddressAsASingleLine", 7);
		}

		public void TestBashFetchForView_ConsigneeDeliveryAddress_E2_Address1()
		{
			// JobDeclaration: 1
			// JobDocAddress: 1
			// OrgAddress: 1
			// OrgHeader: 1

			BashFetchForView("ConsigneeDeliveryAddress+E2_Address1", 4);
		}

		public void TestBashFetchForView_ConsigneeDeliveryAddress_E2_Address2()
		{
			// JobDeclaration: 1
			// JobDocAddress: 1
			// OrgAddress: 1
			// OrgHeader: 1

			BashFetchForView("ConsigneeDeliveryAddress+E2_Address2", 4);
		}

		public void TestBashFetchForView_ConsigneeDeliveryAddress_E2_City()
		{
			// JobDeclaration: 1
			// JobDocAddress: 1
			// OrgAddress: 1
			// OrgHeader: 1

			BashFetchForView("ConsigneeDeliveryAddress+E2_City", 4);
		}

		public void TestBashFetchForView_ConsigneeDeliveryAddress_E2_State()
		{
			// JobDeclaration: 1
			// JobDocAddress: 1
			// OrgAddress: 1
			// OrgHeader: 1

			BashFetchForView("ConsigneeDeliveryAddress+E2_State", 4);
		}

		public void TestBashFetchForView_ConsigneeDeliveryAddress_E2_RN_NKCountryCode()
		{
			// OrgAddress: 2
			// JobDeclaration: 1
			// JobDocAddress: 1
			// OrgAddressAdditionalInfo: 1
			// OrgAddressCapability: 1
			// OrgHeader: 1

			BashFetchForView("ConsigneeDeliveryAddress+E2_RN_NKCountryCode", 7);
		}

		public void TestBashFetchForView_ConsigneeDeliveryAddress_E2_Postcode()
		{
			// JobDeclaration: 1
			// JobDocAddress: 1
			// OrgAddress: 1
			// OrgHeader: 1

			BashFetchForView("ConsigneeDeliveryAddress+E2_Postcode", 4);
		}

		public void TestBashFetchForView_Density_VolumeRatio()
		{
			BashFetchForView("Density+VolumeRatio", 0);
		}

		public void TestBashFetchForView_Density_DensityRemark()
		{
			BashFetchForView("Density+DensityRemark", 0);
		}

		public void TestBashFetchForView_Density_DensityFactor()
		{
			BashFetchForView("Density+DensityFactor", 0);
		}

		public void TestBashFetchForView_JS_Calc_ExcessActualVolumeWeight()
		{
			BashFetchForView("JS_Calc_ExcessActualVolumeWeight", 0);
		}

		public void TestBashFetchForView_JS_Calc_ExcessActualVolumeWeightUnit()
		{
			BashFetchForView("JS_Calc_ExcessActualVolumeWeightUnit", 0);
		}

		public void TestBashFetchForView_JS_RL_NKLoadPort()
		{
			BashFetchForView("JS_RL_NKLoadPort", 0);
		}

		public void TestBashFetchForView_JS_RL_NKDischargePort()
		{
			BashFetchForView("JS_RL_NKDischargePort", 0);
		}

		public void TestBashFetchForView_JS_RL_NKFreightRateOrigin()
		{
			BashFetchForView("JS_RL_NKFreightRateOrigin", 0);
		}

		public void TestBashFetchForView_JS_RL_NKFreightRateDestination()
		{
			BashFetchForView("JS_RL_NKFreightRateDestination", 0);
		}

		public void TestBashFetchForView_IsTemperatureControlled()
		{
			// JobPackLines: 1

			BashFetchForView("IsTemperatureControlled", 1);
		}

		public void TestBashFetchForView_JS_CommunityTransitStatus()
		{
			BashFetchForView("JS_CommunityTransitStatus", 1);
		}

		public void TestBashFetchForView_JS_Calc_DGClass()
		{
			// JobPackLines: 1
			// UNDGDataItems: 1

			BashFetchForView("JS_Calc_DGClass", 2);
		}

		public void TestBashFetchForView_JS_Calc_DGSubstance()
		{
			// JobPackLines: 1
			// UNDGDataItems: 1
			// UNDGSubstancePivot: 1
			// UNDGSubstance: 2

			BashFetchForView("JS_Calc_DGSubstance", 5);
		}

		public void TestBashFetchForView_JS_Calc_DIHazardousWasteCode()
		{
			//JobPackLines: 1
			//UNDGDataItem: 1
			BashFetchForView("JS_Calc_DIHazardousWasteCode", 2);
		}

		public void TestBashFetchForView_JS_Calc_DISpecialPermitIssueDate()
		{
			//JobPackLines: 1
			//UNDGDataItem: 1
			BashFetchForView("JS_Calc_DISpecialPermitIssueDate", 2);
		}

		public void TestBashFetchForView_JS_Calc_DISpecialPermitNumber()
		{
			//JobPackLines: 1
			//UNDGDataItem: 1
			BashFetchForView("JS_Calc_DISpecialPermitNumber", 2);
		}

		public void TestBashFetchForView_JS_Calc_DIIsSalvagePackaging()
		{
			//JobPackLines: 1
			//UNDGDataItem: 1
			BashFetchForView("JS_Calc_DIIsSalvagePackaging", 2);
		}

		public void TestBashFetchForView_JS_Calc_DIIsResidueLastContained()
		{
			//JobPackLines: 1
			//UNDGDataItem: 1
			BashFetchForView("JS_Calc_DIIsResidueLastContained", 2);
		}
		public void TestBashFetchForView_ShippingLineOrgCodeFallbackToBookedShippingLine()
		{
			// JobConsol: 12
			// JobConShipLink: 1
			// JobDeclaration: 1
			// JobDocsAndCartage: 1

			BashFetchForView("ShippingLineOrgCodeFallbackToBookedShippingLine", 15);
		}

		public void TestBashFetchForView_FirstLoadConsol_JK_DepotCutOff()
		{
			// JobConsol: 12
			// JobConsolTransport: 12
			// JobConShipLink: 1
			// JobDeclaration: 1
			// JobDocsAndCartage: 1

			BashFetchForView("FirstLoadConsol+JK_DepotCutOff", 27);
		}

		public void TestBashFetchForView_FirstLoadConsol_JK_DepotReceivalCommences()
		{
			// JobConsol: 12
			// JobConsolTransport: 12
			// JobConShipLink: 1
			// JobDeclaration: 1
			// JobDocsAndCartage: 1

			BashFetchForView("FirstLoadConsol+JK_DepotReceivalCommences", 27);
		}

		public void TestBashFetchForView_FirstLoadConsol_JK_ConsolCutOffDateLocal()
		{
			// JobConsol: 12
			// JobConShipLink: 1
			// JobDeclaration: 1
			// JobDocsAndCartage: 1

			BashFetchForView("FirstLoadConsol+JK_ConsolCutOffDateLocal", 15);
		}

		public void TestBashFetchForView_LastDischargeConsol_JK_CTOAvailabilityDate()
		{
			// JobConsol: 12
			// JobConsolTransport: 12
			// JobSailing: 12
			// JobVoyage: 12
			// JobVoyDestination: 12
			// JobVoyOrigin: 12
			// JobConShipLink: 1
			// JobDeclaration: 1
			// JobDocsAndCartage: 1

			BashFetchForView("LastDischargeConsol+JK_CTOAvailabilityDate", 75);
		}

		public void TestBashFetchForView_FirstLoadConsol_JK_CTOCutOff()
		{
			// JobConsol: 12
			// JobConsolTransport: 12
			// JobConShipLink: 1
			// JobDeclaration: 1
			// JobDocsAndCartage: 1

			BashFetchForView("FirstLoadConsol+JK_CTOCutOff", 27);
		}

		public void TestBashFetchForView_FirstLoadConsol_JK_JX_JA_A_DEP()
		{
			// JobConsol: 12
			// JobConsolTransport: 12
			// JobConShipLink: 1
			// JobDeclaration: 1
			// JobDocsAndCartage: 1

			BashFetchForView("FirstLoadConsol+JK_JX_JA_A_DEP", 27);
		}

		public void TestBashFetchForView_LastDischargeConsol_JK_JX_JB_A_ARV()
		{
			// JobConsol: 12
			// JobConsolTransport: 12
			// JobConShipLink: 1
			// JobDeclaration: 1
			// JobDocsAndCartage: 1

			BashFetchForView("LastDischargeConsol+JK_JX_JB_A_ARV", 27);
		}

		public void TestBashFetchForView_FirstLoadConsol_JK_DocsCutOff()
		{
			// JobConsol: 12
			// JobConsolTransport: 12
			// JobConShipLink: 1
			// JobDeclaration: 1
			// JobDocsAndCartage: 1

			BashFetchForView("FirstLoadConsol+JK_DocsCutOff", 27);
		}

		public void TestBashFetchForView_FirstLoadConsol_JK_RL_NKLoadPort()
		{
			// JobConsol: 12
			// JobConsolTransport: 12
			// JobConShipLink: 1

			BashFetchForView("FirstLoadConsol+JK_RL_NKLoadPort", 25);
		}

		public void TestBashFetchForView_LastDischargeConsol_JK_RL_NKDischargePort()
		{
			// JobConsol: 12
			// JobConsolTransport: 12
			// JobConShipLink: 1

			BashFetchForView("LastDischargeConsol+JK_RL_NKDischargePort", 25);
		}

		public void TestBashFetchForView_FirstLoadConsol_Transports_DepartureTransport_JW_ATD()
		{
			// JobConsol: 12
			// JobConsolTransport: 12
			// JobConShipLink: 1
			// JobDeclaration: 1
			// JobDocsAndCartage: 1

			BashFetchForView("FirstLoadConsol+Transports+DepartureTransport+JW_ATD", 27);
		}

		public void TestBashFetchForView_FirstLoadConsol_Transports_DepartureTransport_JW_ETD()
		{
			// JobConsol: 12
			// JobConsolTransport: 12
			// JobConShipLink: 1
			// JobDeclaration: 1
			// JobDocsAndCartage: 1

			BashFetchForView("FirstLoadConsol+Transports+DepartureTransport+JW_ETD", 27);
		}

		public void TestBashFetchForView_LastDischargeConsol_Transports_ArrivalTransport_JW_ATA()
		{
			// JobConsol: 12
			// JobConsolTransport: 12
			// JobSailing: 12
			// JobVoyage: 12
			// JobVoyDestination: 12
			// JobVoyOrigin: 12
			// JobConShipLink: 1
			// JobDeclaration: 1
			// JobDocsAndCartage: 1

			BashFetchForView("LastDischargeConsol+Transports+ArrivalTransport+JW_ATA", 75);
		}

		public void TestBashFetchForView_LastDischargeConsol_Transports_ArrivalTransport_JW_ETA()
		{
			// JobConsol: 12
			// JobConsolTransport: 12
			// JobSailing: 12
			// JobVoyage: 12
			// JobVoyDestination: 12
			// JobVoyOrigin: 12
			// JobConShipLink: 1
			// JobDeclaration: 1
			// JobDocsAndCartage: 1

			BashFetchForView("LastDischargeConsol+Transports+ArrivalTransport+JW_ETA", 75);
		}

		public void TestBashFetchForView_DepartureConsol_Transports_DepartureTransport_JW_ATA()
		{
			// JobConsol: 12
			// JobConsolTransport: 12
			// JobConShipLink: 1
			// JobDeclaration: 1
			// JobDocsAndCartage: 1

			BashFetchForView("DepartureConsol+Transports+DepartureTransport+JW_ATA", 27);
		}

		public void TestBashFetchForView_DepartureConsol_Transports_DepartureTransport_JW_ETA()
		{
			// JobConsol: 12
			// JobConsolTransport: 12
			// JobConShipLink: 1
			// JobDeclaration: 1
			// JobDocsAndCartage: 1

			BashFetchForView("DepartureConsol+Transports+DepartureTransport+JW_ETA", 27);
		}

		public void TestBashFetchForView_LocalConsol_JK_PrepaidCollect()
		{
			// JobConsol: 12
			// JobConShipLink: 1
			// JobDeclaration: 1
			// JobDocsAndCartage: 1

			BashFetchForView("LocalConsol+JK_PrepaidCollect", 15);
		}

		public void TestBashFetchForView_FirstLoadConsol_Transports_DepartureTransport_JW_VGMCutOff()
		{
			// JobConsol: 12
			// JobConsolTransport: 12
			// JobConShipLink: 1
			// JobDeclaration: 1
			// JobDocsAndCartage: 1

			BashFetchForView("FirstLoadConsol+Transports+DepartureTransport+JW_VGMCutOff", 27);
		}

		public void TestBashFetchForView_JS_Calc_LastKnownTransitWarehouseStatus()
		{
			// JobConsol: 12
			// JobConsolTransport: 12
			// JobSailing: 12
			// JobVoyage: 12
			// JobVoyDestination: 12
			// JobVoyOrigin: 12
			// JobConShipLink: 1
			// JobDeclaration: 1
			// JobDocAddress: 1
			// JobDocsAndCartage: 1
			// JobPackLines: 1
			// OrgAddress: 1
			// OrgHeader: 1

			BashFetchForView("JS_Calc_LastKnownTransitWarehouseStatus", 79);
		}

		public void TestBashFetchForView_HasDamagedPackages()
		{
			// JobPackLines: 1

			BashFetchForView("HasDamagedPackages", 1);
		}

		public void TestBashFetchForView_HasPillagedPackages()
		{
			// JobPackLines: 1

			BashFetchForView("HasPillagedPackages", 1);
		}

		public void TestBashFetchForView_OriginTransitWarehouseStatuses()
		{
			// JobPackLines: 1

			BashFetchForView("OriginTransitWarehouseStatuses", 1);
		}

		#endregion

		public void TestNoExceptionWhenShipmentNotHasCusExitHeader()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var factory = new BusinessObjectFactory();

				var shipment = factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = "AUSDY";
				shipment.JS_RL_NKDestination = "DEWIB";

				factory.Save();

				var cusExitHeaderLoader = ObjectFactory.Get<EUExitControl.ICusExitHeaderLoader>("EUExitControl.ICusExitHeaderLoader", factory);
				var cusExitHeader = cusExitHeaderLoader.Load(!shipment.IsInDatabase, shipment.PK, JobShipmentSchema.Constants.Prefix).SingleOrDefault();

				AssertNull(cusExitHeader);
				AssertNoExceptionThrown(() => GetNewCollection().FetchStrategy.FetchForView(
					new[] { factory.Load<ForwardingModuleShipment>(shipment.PK) },
					new TableColumn[] { new TableColumn("", "WorkflowItems+MilestonesIncludingRelated+LastMilestone+P9_SE_NKMilestoneEvent") }));
			}
		}

		protected override SchemaPKColumn PkColumn => JobShipmentSchema.PK;

		protected override ZGuid[] CreateKeysForTest()
		{
			var result = new List<ZGuid>();
			var factory = new BusinessObjectFactory();

			var consignee = factory.NewWithValidTestData<OrgHeader>();
			var consignor = factory.NewWithValidTestData<OrgHeader>();

			var refContainer = factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			var declarationType = ObjectFactory.GetType<IBaseJobDeclaration>();
			var inBondType = ObjectFactory.GetType<US.InBond.ICusInBondHeader>();

			var importReleaseDepotOrgHeader = factory.NewWithValidTestData<OrgHeader>();

			var importReleaseDepot = importReleaseDepotOrgHeader.Addresses.AddNew();
			importReleaseDepot.FillWithValidTestData();

			var exportReceivingDepotOrgHeader = factory.NewWithValidTestData<OrgHeader>();

			var exportReceivingDepot = exportReceivingDepotOrgHeader.Addresses.AddNew();
			exportReceivingDepot.FillWithValidTestData();

			var controllingAgent = factory.NewWithValidTestData<OrgHeader>();
			var controllingCustomer = factory.NewWithValidTestData<OrgHeader>();

			var controllingCustomerAddress = factory.NewWithValidTestData<OrgAddress>();
			controllingCustomerAddress.OA_OH = controllingCustomer.PK;
			controllingCustomerAddress.OA_Code = "TESTSCPADR";

			var localCharges = factory.NewWithValidTestData<OrgHeader>();

			var localChargesAddr = localCharges.MainAddress;
			localChargesAddr.FillWithValidTestData();
			localChargesAddr.OA_RN_NKCountryCode = "AU";

			var agentCollect = factory.NewWithValidTestData<OrgHeader>();

			var agentCollectAddr = agentCollect.MainAddress;
			agentCollectAddr.FillWithValidTestData();
			agentCollectAddr.OA_RN_NKCountryCode = "AU";

			var supplierBuyerLink = consignee.SupplierLinks.AddNew(consignor);
			supplierBuyerLink.OrgSupBuyLinkTrnModes[0].PF_TransportMode = "ALL";
			supplierBuyerLink.OrgSupBuyLinkTrnModes[0].PF_OH_ControllingCustomer = controllingCustomer.PK;

			localCharges.AllRelatedParties.AddNew();
			localCharges.AllRelatedParties[0].PR_OH_RelatedParty = controllingCustomer.PK;
			localCharges.AllRelatedParties[0].PR_PartyType = RelatedPartyTypeList.Codes.ControllingCustomer;
			localCharges.AllRelatedParties[0].PR_FreightDirection = RelatedPartyDirectionList.Codes.PickupAndDelivery;

			using (FreightDataRegistry.Instance.DefaultShipmentControllingCustomer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				for (var i = 0; i < 12; i++)
				{
					var vessel = Factory.NewWithValidTestData<RefVessel>();
					vessel.RV_Name = "vessel" + i;

					var voyage = factory.New<JobVoyage>();
					voyage.JV_RV_NKVessel = vessel.RV_FK;
					voyage.JV_VoyageFlight = "voyage" + i;
					voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
					voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";
					voyage.GenerateSailings();

					var consol = factory.New<ForwardingConsol>();

					var container = consol.Containers.AddNew();
					container.JC_ContainerNum = "TEST2017";
					container.JC_RC = refContainer.PK;

					var transport = consol.Transports.AddNew();
					transport.JW_IsLinked = true;
					transport.JW_JX = voyage.Sailings[0].PK;

					var shipment = consol.Shipments.AddNew();
					shipment.FillWithValidTestData();
					shipment.JS_TransportMode = "AIR";

					shipment.JS_UniqueConsignRef = "shipment" + i;

					shipment.ControllingAgentNameOrPK = controllingAgent.PK.ToString();
					shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
					shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
					shipment.JS_OA_ExportReceivingDepot = exportReceivingDepot.PK;
					shipment.JS_OA_ImportReleaseDepot = importReleaseDepot.PK;

					shipment.DefaultControllingCustomer(shipment.BuyerSupplierLinksHelper?.GetControllingCustomer(), false);

					var job = new JobHeader.Loader(shipment).TryCreate();
					job.JH_GE = GlbDepartment.CurrentDepartment.PK;
					job.JH_GB = GlbBranch.CurrentBranch.PK;
					job.LocalChargesPK = localCharges.PK;
					job.AgentCollectPK = agentCollect.PK;

					var outerPackline = shipment.OuterPackLines.AddNew();
					outerPackline.SetContainer(consol, container);

					var undg = outerPackline.UNDGs.AddNew();
					var subs = Factory.New<UNDGSubstance>();
					subs.DG_Code = "1001";
					subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
					undg.LinkDefault(subs);

					var milestone = shipment.WorkflowItems.Milestones.AddNew();
					milestone.SetMilestoneActualDateForTest(ZDateTime.Now);
					milestone.P9_GC = GlbCompany.CurrentCompany.PK;

					milestone = shipment.WorkflowItems.Milestones.AddNew();
					milestone.SetMilestoneActualDateForTest(ZDateTime.Invalid);
					milestone.P9_GC = GlbCompany.CurrentCompany.PK;

					milestone = shipment.WorkflowItems.MilestonesIncludingRelated.AddNew();
					milestone.SetMilestoneActualDateForTest(ZDateTime.Now);
					milestone.P9_GC = GlbCompany.CurrentCompany.PK;

					milestone = shipment.WorkflowItems.MilestonesIncludingRelated.AddNew();
					milestone.SetMilestoneActualDateForTest(ZDateTime.Invalid);
					milestone.P9_GC = GlbCompany.CurrentCompany.PK;

					var task = shipment.WorkflowItems.Tasks.AddNew();
					task.P9_Type = "INV";
					task.P9_Description = "Investigation";
					task.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

					var declaration = factory.NewWithValidTestData(declarationType);
					declaration[JobDeclarationSchema.JE_DeclarationReference] = "decl" + i;
					declaration[JobDeclarationSchema.JE_JS] = shipment.PK;

					var inbondHeader = factory.NewWithValidTestData(inBondType);
					inbondHeader[CusInBondHeaderSchema.BH_ParentID] = shipment.PK;
					inbondHeader[CusInBondHeaderSchema.BH_ParentTableCode] = shipment.TablePrefix;
					inbondHeader[CusInBondHeaderSchema.BH_ApplicationCode] = CusInBondApplicationCodeList.Codes.InBond;
					inbondHeader[CusInBondHeaderSchema.BH_GB] = GlbBranch.CurrentBranch.PK;

					result.Add(shipment.PK);
				}
			}
			factory.Save();
			return result.ToArray();
		}

		[RequiresSTA]
		public void TestWorksForRelatedShipmentCollection()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var relatedShipmentCollection = shipment.Lookups.CoLoadShipment_List as IBusinessObjectCollection;
			var filterBO = new JobShipmentFilterBusinessObject();
			using (var filterControl = new JobShipmentFilterControl(relatedShipmentCollection, filterBO))
			{
				AssertEquals(typeof(ForwardingShipment), filterControl.GridCollection.TypeOfElements);
			}
		}

		GlbStaff CreateStaff(BusinessObjectFactory factory, string staffCode, string loginName, bool isOperational = false)
		{
			var staff = factory.New<GlbStaff>();
			staff.GS_Code = staffCode;
			staff.GS_LoginName = loginName;
			staff.GS_IsOperational = isOperational;
			return staff;
		}

		void AddSecurityRight(GlbStaff staff, string securityRight, bool allowed = false)
		{
			var glbSecurity = staff.StaffSecurityPermissionsCollection.AddNew();
			glbSecurity.GU_SecurityRight = securityRight;
			glbSecurity.GU_SecurityItemIsAllowed = allowed;
		}

		[RequiresSTA]
		public void TestWorkflowPopupModulesSecurityOnJob()
		{
			var factory = new BusinessObjectFactory();

			var modules = new ModuleIdentifier[] {
				ModuleIDs.WorkflowExceptions,
				ModuleIDs.WorkflowMilestones,
				ModuleIDs.WorkflowTriggers,
				ModuleIDs.ProcessTasks,
			};

			var staff1 = CreateStaff(factory, "AAA", "AAA", true);
			AddSecurityRight(staff1, "MaintainShipmentWorkflowView Exceptions", true);
			AddSecurityRight(staff1, "MaintainShipmentWorkflowView Milestones", true);
			AddSecurityRight(staff1, "MaintainShipmentWorkflowView Triggers", true);
			AddSecurityRight(staff1, "MaintainShipmentWorkflowView Tasks", true);

			var staff2 = CreateStaff(factory, "BBB", "BBB", true);
			AddSecurityRight(staff2, "MaintainShipmentWorkflowView Exceptions", false);
			AddSecurityRight(staff2, "MaintainShipmentWorkflowView Milestones", false);
			AddSecurityRight(staff2, "MaintainShipmentWorkflowView Triggers", false);
			AddSecurityRight(staff2, "MaintainShipmentWorkflowView Tasks", false);

			factory.Save();

			foreach (var moduleIdentifier in modules)
			{
				AssertEquals(0, PopupBoxFilterSecurityChecker(moduleIdentifier, staff1));
				AssertEquals(1, PopupBoxFilterSecurityChecker(moduleIdentifier, staff2));
			}
		}

		int PopupBoxFilterSecurityChecker(ModuleIdentifier moduleIdentifier, GlbStaff staff)
		{
			using (Env.SetTemporaryUserContext(new UserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				using (var findBox = new TestPopupFindBox())
				{
					SecurityCheckpoint.CheckpointChecked += OnCheckpointCheckedForTest;
					var currentModuleID = moduleIdentifier;
					ZFilterModule filterModule = ZFilterModule.GetZFilterModule(currentModuleID, "");
					findBox.SetFilterModuleForTesting(filterModule);
					findBox.SetParentModuleIDForTest(ModuleIDs.JobShipment);
					findBox.ModuleShowing += (sender, e) => e.ModuleID = currentModuleID;
					using (var module = findBox.NewModuleFromModuleID_PublicForTest())
					{
						findBox.SelectFromPopupFormWithoutDisplaying();
					}
				}
			}

			var errorCount = ErrorReporter.LastExceptionsReported().Count(a => a.Contains("Denied"));
			ErrorReporter.Clear();
			SecurityCheckpoint.CheckpointChecked -= OnCheckpointCheckedForTest;
			return errorCount;
		}

		static string[] CheckpointsToIgnore =>
			new string[] { "Config.BufferManagementConfig.BMBoard.VisualBoards" };

		void OnCheckpointCheckedForTest(SecurityCheckpoint securityCheckpoint, bool isAllowed, bool wasCached)
		{
			if (CheckpointsToIgnore.Contains(securityCheckpoint.ToString()))
			{
				return;
			}

			if (!isAllowed)
			{
				ErrorReporter.ReportOnce("Denied");
			}
		}

		public class TestPopupFindBox : ZPopupFindBox
		{
			public new IFindBoxPopup PopupForm
			{
				get { return base.PopupForm; }
			}

			public void SetFilterModuleForTesting(ZFilterModule module)
			{
				filterModuleForTesting = module;
				ModuleID = module.ID;
			}
			ZFilterModule filterModuleForTesting;

			public void SetParentModuleIDForTest(ModuleIdentifier parentModuleID)
			{
				ParentModuleID = parentModuleID;
			}

			protected override ZFilterModule NewModuleFromModuleID()
			{
				var result = filterModuleForTesting ?? base.NewModuleFromModuleID();
				return result;
			}

			public ZFilterModule NewModuleFromModuleID_PublicForTest()
			{
				return NewModuleFromModuleID();
			}
		}

		protected override ZFilterStripControl GetNewFilterStripControl()
		{
			var shipments = new ShipmentCollection(Factory);
			var filterBusinessObject = new JobShipmentFilterBusinessObject();
			return new JobShipmentFilterControl(shipments, filterBusinessObject);
		}

		protected override string[] GetExcludedColumnNamesForTestFetchHint()
		{
			var columnNames = new List<string>();

			using (var filterControl = GetNewFilterStripControl())
			{
				filterControl.FilteredGrid.ColumnStyles.Clear();

				ObjectFactory.Get<IForwardingShipmentModuleCustomColumnsAndFiltersProvider>().AddColumns((IGridControl)filterControl);

				columnNames.AddRange(
					filterControl.FilteredGrid.ColumnStyles
						.Cast<ZGridColumnInfo>()
						.Select(c => c.ColumnName)
						.ToArray()
				);
			}

			columnNames.Add(nameof(ForwardingShipment.IsTemplate));
			columnNames.Add("TemplateRecord+STR_IsActive");
			columnNames.Add("TemplateRecord+STR_TemplateName");
			columnNames.Add("EarliestLastFreeDayImportDemurrageForBinding");
			columnNames.Add("EarliestLastFreeDayImportDetentionForBinding");
			columnNames.Add("EarliestLastFreeDayImportStorageForBinding");
			columnNames.Add("EarliestLastFreeDayExportDemurrageForBinding");
			columnNames.Add("EarliestLastFreeDayExportDetentionForBinding");
			columnNames.Add("EarliestLastFreeDayExportStorageForBinding");
			columnNames.Add("FirstSeaTransport");
			columnNames.Add("LastSeaTransport");
			columnNames.Add("FirstSeaLegLoadPortForBinding");
			columnNames.Add("FirstSeaLegLoadPortETDForBinding");
			columnNames.Add("FirstSeaLegLoadPortATDForBinding");
			columnNames.Add("LastSeaLegDischargePortForBinding");
			columnNames.Add("LastSeaLegDischargePortETAForBinding");
			columnNames.Add("LastSeaLegDischargePortATAForBinding");

			return columnNames.ToArray();
		}

		protected override IBusinessObjectCollection GetNewCollection()
		{
			return new ForwardingModuleShipmentCollection(Factory);
		}
	}
}
