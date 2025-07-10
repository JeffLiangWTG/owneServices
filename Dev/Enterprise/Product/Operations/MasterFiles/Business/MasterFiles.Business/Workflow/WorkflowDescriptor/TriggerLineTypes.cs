using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	using System;
	using CargoWise.Application;
	using Enterprise.MasterFiles.Integration;
	using Enterprise.ZArchitecture.Core;

	public class TriggerLineTypes : CodeDescriptionPairList
	{
		public static class Codes
		{
			public const string Order = WorkflowDescriptors.OrderWorkflowDescriptorCode;
			public const string JobShipmentPreplanning = WorkflowDescriptors.JobShipmentPreplanningWorkflowDescriptorCode;
			public const string ForwardingShipment = WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode;
			public const string Container = WorkflowDescriptors.ContainerWorkflowDescriptorCode;
			public const string ContainerStockManager = WorkflowDescriptors.ContainerStockManagerWorkflowDescriptorCode;
			public const string ContainerMovement = WorkflowDescriptors.ContainerMovementWorkflowDescriptorCode;
			public const string CusInBondHeader = WorkflowDescriptors.CusInBondHeaderWorkflowDescriptorCode;
			public const string eManifest = WorkflowDescriptors.eManifestWorkflowDescriptorCode;
			public const string JobConsol = WorkflowDescriptors.JobConsolWorkflowDescriptorCode;
			public const string JobDeclaration = WorkflowDescriptors.JobDeclarationWorkflowDescriptorCode;
			public const string Quotation = WorkflowDescriptors.QuotationWorkflowDescriptorCode;
			public const string ClientRate = WorkflowDescriptors.ClientRateWorkflowDescriptorCode;
			public const string CusISFHeader = WorkflowDescriptors.CusISFHeaderWorkflowDescriptorCode;
			public const string QuotedBooking = WorkflowDescriptors.QuotedBookingWorkflowDescriptorCode;
			public const string AgencyBooking = WorkflowDescriptors.AgencyBookingWorkflowDescriptorCode;
			public const string BillOfLading = WorkflowDescriptors.BillOfLadingWorkflowDescriptorCode;
			public const string Campaign = WorkflowDescriptors.CampaignWorkflowDescriptorCode;
			public const string AgencyShipment = WorkflowDescriptors.AgencyShipmentWorkflowDescriptorCode;
			public const string AgencyContainer = WorkflowDescriptors.AgencyContainerWorkflowDescriptorCode;
			public const string CartageLeg = WorkflowDescriptors.CartageLegWorkflowDescriptorCode;
			public const string SailingSchedule = WorkflowDescriptors.SailingScheduleWorkflowDescriptorCode;
			public const string SalesEnquiry = WorkflowDescriptors.SalesEnquiryWorkflowDescriptorCode;
			public const string DtbBookingConsolidation = WorkflowDescriptors.DtbBookingConsolidationWorkflowDescriptorCode;
			public const string DtbBooking = WorkflowDescriptors.DtbBookingWorkflowDescriptorCode;
			public const string DtbBookingInstruction = WorkflowDescriptors.DtbBookingInstructionWorkflowDescriptorCode;
			public const string DtbBookingConfirmation = WorkflowDescriptors.DtbBookingConfirmationWorkflowDescriptorCode;
			public const string DtbConsignment = WorkflowDescriptors.DtbBookingConsignmentWorkflowDescriptorCode;
			public const string RunSheetInstruction = WorkflowDescriptors.DtbConsignmentRunSheetInstructionWorkflowDescriptorCode;
			public const string WhsAdjustment = WorkflowDescriptors.WhsAdjustmentWorkflowDescriptorCode;
			public const string WhsCartage = WorkflowDescriptors.WhsCartageWorkflowDescriptorCode;
			public const string WhsReceive = WorkflowDescriptors.WhsReceiveWorkflowDescriptorCode;
			public const string WhsStocktake = WorkflowDescriptors.WhsStocktakeWorkflowDescriptorCode;
			public const string WhsOrder = WorkflowDescriptors.WhsOrderWorkflowDescriptorCode;
			public const string WhsTransfer = WorkflowDescriptors.WhsTransferWorkflowDescriptorCode;
			public const string WhsWorkOrder = WorkflowDescriptors.WhsWorkOrderWorkflowDescriptorCode;
			public const string WhsPick = WorkflowDescriptors.WhsPickWorkflowDescriptorCode;
			public const string WhsVASOrder = WorkflowDescriptors.WhsVASOrderWorkflowDescriptorCode;
			public const string ARInvoice = WorkflowDescriptors.ARInvoiceCode;
			public const string APInvoice = WorkflowDescriptors.APInvoiceCode;
			public const string OrgPartRelation = WorkflowDescriptors.OrgPartRelationWorkflowDescriptorCode;
			public const string OrgSupplierPart = WorkflowDescriptors.OrgSupplierPartWorkflowDescriptorCode;
			public const string OrgHeader = WorkflowDescriptors.OrgHeaderWorkflowDescriptorCode;
			public const string Opportunity = WorkflowDescriptors.OpportunityWorkflowDescriptorCode;
			public const string CrmOpportunity = WorkflowDescriptors.CrmOpportunityWorkflowDescriptorCode;
			public const string Recon = WorkflowDescriptors.ReconWorkflowDescriptorCode;
			public const string Protest = WorkflowDescriptors.ProtestWorkflowDescriptorCode;
			public const string DrawBack = WorkflowDescriptors.DrawBackWorkflowDescriptorCode;
			public const string CustomsHouseAirCargo = WorkflowDescriptors.CustomsHouseAirCargoCode;
			public const string CusSCAOceanBill = WorkflowDescriptors.CusSCAOceanBillWorkflowDescriptorCode;
			public const string JPAFR = WorkflowDescriptors.JPAFRWorkflowDescriptorCode;
			public const string Communication = WorkflowDescriptors.CommunicationWorkflowDescriptorCode;
			public const string PkgPackage = WorkflowDescriptors.PkgPackageWorkflowDecriptorCode;
			public const string CusStatementHeader = WorkflowDescriptors.CusStatementHeaderWorkflowDescriptorCode;
			public const string CusEntryHeader = WorkflowDescriptors.CusEntryHeaderWorkflowDescriptorCode;
			public const string CusExitReport = WorkflowDescriptors.CusExitReportWorkflowDescriptorCode;
			public const string Service = WorkflowDescriptors.ServiceWorkflowDescriptorCode;
			public const string CusNOEmmaMessageGenerator = WorkflowDescriptors.CusNOEmmaMessageGeneratorWorkflowDescriptorCode;
			public const string CYDDelivery = WorkflowDescriptors.CYDDeliveryWorkflowDescriptorCode;
			public const string CYDPickup = WorkflowDescriptors.CYDPickupWorkflowDescriptorCode;
		}

		public TriggerLineTypes()
		{
			AddRange(ObjectFactory.Get<IFullWorkflowDescriptorList>());
		}

		public static TriggerLineTypes All
		{
			get
			{
#if DEBUG
				if (Globals.IsTest)
				{
					return new TriggerLineTypes();
				}
#endif
				return instance ?? (instance = new TriggerLineTypes());
			}
		}

		[ThreadStatic]
		static TriggerLineTypes instance;
	}
}
