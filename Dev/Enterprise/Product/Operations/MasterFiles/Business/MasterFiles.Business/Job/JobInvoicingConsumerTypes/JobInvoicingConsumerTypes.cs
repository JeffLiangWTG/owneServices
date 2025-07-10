using CargoWise.Common;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	/// <summary>
	/// This list defines every consumer of Job Invoicing and AutoRating.
	///
	/// One of these defined types must be returned on each implementor of IJobInvoicingPlugIn or IAutoRating.
	/// If you are implementing this interface on a new business object (new type), please add a line
	/// to the list below.
	///
	/// The codes used must also be identical to the codes specified in ViewGenericJob.sql.
	///
	/// You must ALSO add the consumer type in the constructor of this class.
	///
	/// You gotta do waaaay more than just that. You even have to change the schema!  Look at a recent checkin to this file to see qite how much more you have to plumb together.
	/// </summary>
	public class JobInvoicingConsumerTypes : CodeDescriptionPairList, IJobInvoicingConsumerTypes
	{
		public const string ShipmentCode = "SHP";
		public const string QuotedBookingCode = "QSH";
		public const string ConsolCode = "CON";
		public const string ForwardingConsolCode = "FCN";
		public const string GatewayConsolCode = "GCN";
		public const string BrokerageCode = "BRK";
		public const string PostClearanceBrokerageCode = "PCB";
		public const string MasterAWBCode = "AWB";
		public const string CFSShipmentCode = "CSH";
		public const string CFSLoadListCode = "CLL";
		public const string FCLStorageCode = "CST";
		public const string LocalCartageCode = "TRN";
		public const string AgentBookingCode = "ABK";
		public const string TransportBookingCode = WorkflowDescriptors.DtbBookingWorkflowDescriptorCode;
		public const string TransportBookingConsignmentCode = WorkflowDescriptors.DtbBookingConsignmentWorkflowDescriptorCode;
		public const string TransportBookingWithAgentCode = "ATB";
		public const string TransportConsignmentCode = WorkflowDescriptors.DtbConsignmentWorkflowDescriptorCode;
		public const string WarehouseInwardsCode = "WIN";
		public const string WarehouseOutwardsCode = "WOU";
		public const string TransitReceiveCode = "TRC";
		public const string TransitReceiveTransportationUnitCode = "TRU";
		public const string TransitDispatchCode = "TDC";
		public const string TransitDispatchLoadListCode = "TDL";
		public const string TransitDispatchTransportationUnitCode = "TDU";
		public const string WarehouseStorageCode = "WST";
		public const string WarehouseStocktakeCode = "WSC";
		public const string WarehouseAdHocServiceJobCode = "WSJ";
		public const string WarehouseVASOrderCode = "WVO";
		public const string OneOffQuotationCode = "QTE";
		public const string CusMAWBCode = "ACR";
		public const string CusUnderbondCode = "UBR";
		public const string CTOCusMAWBCode = "CTO";
		public const string CTOCusImportHAWBCode = "AHW";
		public const string CTOCusExportHAWBCode = "AHE";
		public const string AgencyBillOfLadingCode = "AGS";
		public const string AgencyBookingCode = "AGB";
		public const string AgencyDetentionInvoiceCode = "ACD";
		public const string AgencyVoyageAccountingCode = "AVA";
		public const string AgencySundryChargesCode = "ASC";
		public const string ImporterSecurityFilingCode = "ISF";
		public const string OrganisationCode = "ORG";
		public const string eManifestCode = "MAN";
		public const string CAeManifestCode = "CAE";
		public const string WorkItemCode = "WKI";
		public const string ProjectCode = "WKP";
		public const string WorkRequestCode = "WKR";
		public const string CYDReceiveAdviceJobCode = "YRA";
		public const string CYDReleaseAdviceJobCode = "YRE";
		public const string CYDTransportationUnitJobCode = "YTU";
		public const string CYDAdHocServiceOrderJobCode = "YAO";
		public const string CYDPeriodicInvoicingJobCode = "YPI";
		public const string MNRWorkOrderHeaderJobCode = "MWO";
		public const string CustomsTransitNCTSCode = Enterprise.Messaging.Integration.ApplicationCodeList.Codes.EuNcts; // "NCT"
		public const string CustomsTemporaryStorageCode = "STO";
		public const string BRLPCOCode = "LPC";

		public static readonly JobInvoicingConsumerType Shipment = new ShipmentConsumerType(ShipmentCode, ResString.GetMultilingualString("MasterFiles|JobInvoicingConsumerTypes|Shipment", "Shipment"));
		public static readonly JobInvoicingConsumerType QuotedBooking = new QuotedBookingConsumerType(QuotedBookingCode, ResString.GetMultilingualString("MasterFiles|JobInvoicingConsumerTypes|QuotedBooking", "Quick Booking"));
		public static readonly JobInvoicingConsumerType Consol = new ConsolConsumerType(ConsolCode, ResString.GetMultilingualString("MasterFiles|JobInvoicingConsumerTypes|Consol", "Consol"));
		public static readonly JobInvoicingConsumerType ForwardingConsol = new ForwardingConsolConsumerType(ForwardingConsolCode, ResString.GetMultilingualString("MasterFiles|JobInvoicingConsumerTypes|ForwardingConsol", "Consol"));
		public static readonly JobInvoicingConsumerType GatewayConsol = new GatewayConsolConsumerType(GatewayConsolCode, ResString.GetMultilingualString("MasterFiles|JobInvoicingConsumerTypes|GatewayConsol", "Gateway Consol"));
		public static readonly JobInvoicingConsumerType Brokerage = new BrokerageConsumerType(BrokerageCode, ResString.GetMultilingualString("MasterFiles|JobInvoicingConsumerTypes|Brokerage", "Declaration Job"));
		public static readonly JobInvoicingConsumerType PostClearanceBrokerage = new PostClearanceBrokerageConsumerType(PostClearanceBrokerageCode, ResString.GetMultilingualString("MasterFiles|JobInvoicingConsumerTypes|PostClearanceBrokerage", "Post Clearance Declaration Job"));
		public static readonly JobInvoicingConsumerType MasterAWB = new MasterAWBConsumerType(MasterAWBCode, ResString.GetMultilingualString("MasterFiles|JobInvoicingConsumerTypes|MasterAWB", "Master Air Waybill"));
		public static readonly JobInvoicingConsumerType CFSShipment = new CFSShipmentConsumerType(CFSShipmentCode, ResString.GetMultilingualString("MasterFiles|JobInvoicingConsumerTypes|CFSShipment", "CFS Shipment"));
		public static readonly JobInvoicingConsumerType CFSLoadList = new CFSLoadListConsumerType(CFSLoadListCode, ResString.GetMultilingualString("MasterFiles|JobInvoicingConsumerTypes|CFSLoadList", "CFS Load List"));
		public static readonly JobInvoicingConsumerType FCLStorage = new FCLStorageConsumerType(FCLStorageCode, ResString.GetMultilingualString("MasterFiles|JobInvoicingConsumerTypes|FCLStorage", "FCL Container Storage"));
		public static readonly JobInvoicingConsumerType LocalCartage = new CartageConsumerType(LocalCartageCode, ResString.GetMultilingualString("MasterFiles|JobInvoicingConsumerTypes|LocalCartage", "Port Transport"));
		public static readonly JobInvoicingConsumerType AgentBooking = new AgentBookingConsumerType(AgentBookingCode, ResString.GetMultilingualString("MasterFiles|JobInvoicingConsumerTypes|AgentBooking", "Standalone Transport Booking Booked via CBA (obsolete)"));
		public static readonly JobInvoicingConsumerType TransportBooking = new TransportBookingConsumerType(TransportBookingCode, ResString.GetMultilingualString("MasterFiles|JobInvoicingConsumerTypes|TransportBooking", "Standalone Transport Booking"));
		public static readonly JobInvoicingConsumerType TransportBookingConsignment = new TransportBookingConsignmentConsumerType(TransportBookingConsignmentCode, ResString.GetMultilingualString("MasterFiles|JobInvoicingConsumerTypes|TransportBookingConsignment", "Transport Booking Consignment"));
		public static readonly JobInvoicingConsumerType TransportBookingWithAgent = new TransportBookingWithAgentConsumerType(TransportBookingWithAgentCode, ResString.GetMultilingualString("MasterFiles|JobInvoicingConsumerTypes|TransportBookingWithAgent", "Standalone Transport Booking Booked via CBA"));
		public static readonly JobInvoicingConsumerType TransportConsignment = new TransportLTConsignmentConsumerType(TransportConsignmentCode, ResString.GetMultilingualString("MasterFiles|JobInvoicingConsumerTypes|TransportConsignment", "Land Transport Consignment"));
		public static readonly JobInvoicingConsumerType WarehouseInwards = new WarehouseInwardsConsumerType(WarehouseInwardsCode, ResString.GetMultilingualString("MasterFiles|JobInvoicingConsumerTypes|WarehouseInwards", "Warehouse Receive"));
		public static readonly JobInvoicingConsumerType WarehouseOutwards = new WarehouseOrdersConsumerType(WarehouseOutwardsCode, ResString.GetMultilingualString("MasterFiles|JobInvoicingConsumerTypes|WarehouseOutwards", "Warehouse Release"));
		public static readonly JobInvoicingConsumerType WarehouseStorage = new WarehousePeriodicConsumerType(WarehouseStorageCode, ResString.GetMultilingualString("MasterFiles|JobInvoicingConsumerTypes|WarehouseStorage", "Warehouse Periodic"));
		public static readonly JobInvoicingConsumerType WarehouseStocktake = new WarehouseStocktakeConsumerType(WarehouseStocktakeCode, ResString.GetMultilingualString("MasterFiles|JobInvoicingConsumerTypes|WarehouseStocktake", "Warehouse Stocktake"));
		public static readonly JobInvoicingConsumerType WarehouseAdHocServiceJob = new WarehouseAdHocServiceJobConsumerType(WarehouseAdHocServiceJobCode, ResString.GetMultilingualString("MasterFiles|JobInvoicingConsumerTypes|WarehouseAdHocServiceJob", "Warehouse Ad Hoc Service Job"));
		public static readonly JobInvoicingConsumerType WarehouseVASOrder = new WarehouseVASOrderConsumerType(WarehouseVASOrderCode, ResString.GetMultilingualString("MasterFiles|JobInvoicingConsumerTypes|WarehouseVASOrder", "Warehouse VAS Order"));
		public static readonly JobInvoicingConsumerType OneOffQuotation = new OneOffQuoteConsumerType(OneOffQuotationCode, ResString.GetMultilingualString("MasterFiles|JobInvoicingConsumerTypes|OneOffQuote", "One Off Quote"));
		public static readonly JobInvoicingConsumerType CusMAWB = new CusMAWBConsumerType(CusMAWBCode, ResString.GetMultilingualString("MasterFiles|JobInvoicingConsumerTypes|CusMAWB", "Air Cargo"));
		public static readonly JobInvoicingConsumerType CusUnderbond = new CusUnderbondConsumerType(CusUnderbondCode, ResString.GetMultilingualString("MasterFiles|JobInvoicingConsumerTypes|CusUnderbond", "Air Cargo Outturn"));
		public static readonly JobInvoicingConsumerType CTOCusMAWB = new CTOCusMAWBConsumerType(CTOCusMAWBCode, ResString.GetMultilingualString("MasterFiles|JobInvoicingConsumerTypes|CTOCusMAWB", "Air Cargo CTO"));
		public static readonly JobInvoicingConsumerType CTOCusImportHAWB = new CTOCusImportHAWBConsumerType(CTOCusImportHAWBCode, ResString.GetMultilingualString("MasterFiles|JobInvoicingConsumerTypes|CTOCusImportHAWB", "Air Cargo CTO Import HAWB"));
		public static readonly JobInvoicingConsumerType CTOCusExportHAWB = new CTOCusExportHAWBConsumerType(CTOCusExportHAWBCode, ResString.GetMultilingualString("MasterFiles|JobInvoicingConsumerTypes|CTOCusExportHAWB", "Air Cargo CTO Export HAWB"));
		public static readonly JobInvoicingConsumerType AgencyBillOfLading = new AgencyBillOfLadingConsumerType(AgencyBillOfLadingCode, ResString.GetMultilingualString("MasterFiles|JobInvoicingConsumerTypes|LinerAgencyBillOfLading", "Liner & Agency Bill Of Lading"));
		public static readonly JobInvoicingConsumerType AgencyBooking = new AgencyBookingConsumerType(AgencyBookingCode, ResString.GetMultilingualString("MasterFiles|JobInvoicingConsumerTypes|LinerAgencyBooking", "Liner & Agency Booking"));
		public static readonly JobInvoicingConsumerType AgencyDetentionInvoice = new AgencyShipmentDetentionConsumerType(AgencyDetentionInvoiceCode, ResString.GetMultilingualString("MasterFiles|JobInvoicingConsumerTypes|LinerAgencyDetentionInvoice", "Liner & Agency Detention Invoice"));
		public static readonly JobInvoicingConsumerType AgencyVoyageAccounting = new AgencyShipmentVoyageAccountingConsumerType(AgencyVoyageAccountingCode, ResString.GetMultilingualString("MasterFiles|JobInvoicingConsumerTypes|LinerAgencyVoyageAccounting", "Liner & Agency Voyage Accounting"));
		public static readonly JobInvoicingConsumerType AgencySundryCharges = new AgencyShipmentSundryChargesConsumerType(AgencySundryChargesCode, ResString.GetMultilingualString("MasterFiles|JobInvoicingConsumerTypes|LinerAgencySundryCharges", "Liner & Agency Sundry Charges"));
		public static readonly JobInvoicingConsumerType ImporterSecurityFiling = new JobInvoicingImporterSecurityFilingConsumerType(ImporterSecurityFilingCode, ResString.GetMultilingualString("MasterFiles|JobInvoicingConsumerTypes|ImporterSecurityFiling", "Importer Security Filing"));
		public static readonly JobInvoicingConsumerType Organisation = new OrganisationConsumerType(OrganisationCode, ResString.GetMultilingualString("MasterFiles|JobInvoicingConsumerTypes|Organisation", "Organization"));
		public static readonly JobInvoicingConsumerType eManifest = new eManifestJobInvoicingConsumerType(eManifestCode, ResString.GetMultilingualString("MasterFiles|JobInvoicingConsumerTypes|eManifest", "e-Manifest"));
		public static readonly JobInvoicingConsumerType CAeManifest = new CusCAeMHJobInvoicingConsumerType(CAeManifestCode, ResString.GetMultilingualString("MasterFiles|JobInvoiceingConsumerTypes|CAeMH", "e-Manifest Forwarder (CA)"));
		public static readonly JobInvoicingConsumerType WorkItem = new WorkItemConsumerType(WorkItemCode, ResString.GetMultilingualString("MasterFiles|JobInvoicingConsumerTypes|Workitem", "Work Item"));
		public static readonly JobInvoicingConsumerType Project = new ProjectConsumerType(ProjectCode, ResString.GetMultilingualString("MasterFiles|JobInvoicingConsumerTypes|Project", "Project"));
		public static readonly JobInvoicingConsumerType WorkRequest = new WorkRequestConsumerType(WorkRequestCode, ResString.GetMultilingualString("MasterFiles|JobInvoicingConsumerTypes|WorkRequest", "Customer Service Ticket"));
		public static readonly JobInvoicingConsumerType CYDReceiveAdvice = new CYDReceiveAdviceConsumerType(CYDReceiveAdviceJobCode, ResString.GetMultilingualString("MasterFiles|JobInvoicingConsumerTypes|CYDReceiveAdvice", "Container Yard Pre-Arrival Instruction"));
		public static readonly JobInvoicingConsumerType CYDReleaseAdvice = new CYDReleaseAdviceConsumerType(CYDReleaseAdviceJobCode, ResString.GetMultilingualString("MasterFiles|JobInvoicingConsumerTypes|CYDReleaseAdvice", "Container Yard Release Order"));
		public static readonly JobInvoicingConsumerType CYDTransportationUnit = new CYDTransportationUnitConsumerType(CYDTransportationUnitJobCode, ResString.GetMultilingualString("MasterFiles|JobInvoicingConsumerTypes|CYDTransportationUnitConsumerType", "Container Yard Transportation Unit"));
		public static readonly JobInvoicingConsumerType CYDAdHocServiceOrder = new CYDAdHocServiceOrderConsumerType(CYDAdHocServiceOrderJobCode, ResString.GetMultilingualString("MasterFiles|JobInvoicingConsumerTypes|CYDAdHocServiceOrderConsumerType", "Container Yard Service Order"));
		public static readonly JobInvoicingConsumerType CYDPeriodicInvoicing = new CYDTransportationUnitConsumerType(CYDPeriodicInvoicingJobCode, ResString.GetMultilingualString("MasterFiles|JobInvoicingConsumerTypes|CYDPeriodicInvoicingConsumerType", "Container Yard Periodic Invoicing"));
		public static readonly JobInvoicingConsumerType MNRWorkOrderHeader = new MNRWorkOrderHeaderConsumerType(MNRWorkOrderHeaderJobCode, ResString.GetMultilingualString("MasterFiles|JobInvoicingConsumerTypes|MNRWorkOrderHeaderConsumerType", "Container Yard Work Order"));
		public static readonly JobInvoicingConsumerType TransitReceive = new TransitReceiveConsumerType(TransitReceiveCode, ResString.GetMultilingualString("MasterFiles|JobInvoicingConsumerTypes|TransitReceive", "Transit Receive"));
		public static readonly JobInvoicingConsumerType TransitReceiveTransportationUnit = new TransitReceiveTransportationUnitConsumerType(TransitReceiveTransportationUnitCode, ResString.GetMultilingualString("MasterFiles|JobInvoicingConsumerTypes|TransitReceiveTransportationUnit", "Transit Receive Transportation Unit"));
		public static readonly JobInvoicingConsumerType TransitDispatch = new TransitDispatchConsumerType(TransitDispatchCode, ResString.GetMultilingualString("MasterFiles|JobInvoicingConsumerTypes|TransitDispatch", "Transit Dispatch"));
		public static readonly JobInvoicingConsumerType TransitDispatchLoadList = new TransitDispatchLoadListConsumerType(TransitDispatchLoadListCode, ResString.GetMultilingualString("MasterFiles|JobInvoicingConsumerTypes|TransitDispatchLoadList", "Transit Dispatch Load List"));
		public static readonly JobInvoicingConsumerType TransitDispatchTransportationUnit = new TransitDispatchTransportationUnitConsumerType(TransitDispatchTransportationUnitCode, ResString.GetMultilingualString("MasterFiles|JobInvoicingConsumerTypes|TransitDispatchTransportationUnit", "Transit Dispatch Transportation Unit"));
		public static readonly JobInvoicingConsumerType CustomsTransitNCTS = new CustomsTransitNCTSConsumerType(CustomsTransitNCTSCode, ResString.GetMultilingualString("MasterFiles|JobInvoicingConsumerTypes|CustomsTransitNCTS", "Customs Transit (NCTS)"));
		public static readonly JobInvoicingConsumerType CustomsTemporaryStorage = new CustomsTemporaryStorageConsumerType(CustomsTemporaryStorageCode, ResString.GetMultilingualString("MasterFiles|JobInvoicingConsumerTypes|CustomsTemporaryStorage", "Customs Temporary Storage"));
		public static readonly JobInvoicingConsumerType BRLPCO = new BRLPCOConsumerType(BRLPCOCode, ResString.GetMultilingualString("MasterFiles|JobInvoicingConsumerTypes|BRLPCO", "BR LPCO"));

		protected JobInvoicingConsumerTypes()
		{
			Add(Shipment);
			Add(QuotedBooking);
			Add(ForwardingConsol);
			Add(GatewayConsol);
			Add(Brokerage);
			Add(PostClearanceBrokerage);
			Add(MasterAWB);
			Add(CFSShipment);
			Add(CFSLoadList);
			Add(FCLStorage);
			Add(LocalCartage);
			Add(AgentBooking);
			Add(TransportBooking);
			Add(TransportBookingConsignment);
			Add(TransportBookingWithAgent);
			Add(TransportConsignment);
			Add(WarehouseInwards);
			Add(WarehouseOutwards);
			Add(WarehouseStorage);
			Add(WarehouseStocktake);
			Add(WarehouseAdHocServiceJob);
			Add(WarehouseVASOrder);
			Add(CusMAWB);
			Add(CusUnderbond);
			Add(CTOCusMAWB);
			Add(CTOCusImportHAWB);
			Add(CTOCusExportHAWB);
			Add(AgencyBillOfLading);
			Add(AgencyBooking);
			Add(AgencyDetentionInvoice);
			Add(AgencyVoyageAccounting);
			Add(AgencySundryCharges);
			Add(ImporterSecurityFiling);
			Add(Organisation);
			Add(eManifest);
			Add(CAeManifest);
			Add(WorkItem);
			Add(Project);
			Add(WorkRequest);
			Add(CYDReceiveAdvice);
			Add(CYDReleaseAdvice);
			Add(CYDTransportationUnit);
			Add(CYDAdHocServiceOrder);
			Add(CYDPeriodicInvoicing);
			Add(MNRWorkOrderHeader);
			Add(TransitReceive);
			Add(TransitReceiveTransportationUnit);
			Add(TransitDispatchLoadList);
			Add(TransitDispatch);
			Add(TransitDispatchTransportationUnit);
			Add(CustomsTransitNCTS);
			Add(CustomsTemporaryStorage);
			Add(BRLPCO);
		}

		public static JobInvoicingConsumerTypes New()
		{
			var overridden = OverridableNewDelegate.Value;
			var result = overridden != null ? overridden() : new JobInvoicingConsumerTypes();

			return result;
		}

		public static JobInvoicingConsumerTypes NewOnlyJobInvoicingTypes()
		{
			var result = New();
			result.RemoveCode("ORG");

			return result;
		}

		protected delegate JobInvoicingConsumerTypes NewDelegate();

		protected static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();

		public new JobInvoicingConsumerType this[string code]
		{
			get { return (JobInvoicingConsumerType)base[code]; }
		}
	}
}
