using System;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Business
{
	public class ScheduleRelatedJobTypes : CodeDescriptionPairList
	{
		public const string AgencyBookingCode = "AGB";
		public const string AsycudaManifestCode = "ASY";
		public const string BillOfLadingCode = "ASH";
		public const string CartageCode = "TRN";
		public const string CFSContainerCode = "CNT";
		public const string CFSLoadListConsolCode = "CLL";
		public const string CFSShipmentCode = "CSH";
		public const string ConsolCode = "CON";
		public const string CusISFHeaderCode = "ISF";
		public const string DeclarationCode = "DEC";
		public const string DtbBookingCode = "DTB";
		public const string ForwardingShipmentCode = "SHP";
		public const string JobComInvoiceHeaderCode = "INV";
		public const string QuotedBookingCode = "QSH";
		public const string ShipmentPreAdviceCode = "SPA";
		public const string TransitReceiveConsignmentCode = "WRC";
		public const string TransitReceiveASNCode = "WRP";
		public const string TransitDispatchLoadListCode = "WDL";

		public static readonly ScheduleRelatedJobType AgencyBooking = new AgencyBookingScheduleRelatedJobType(AgencyBookingCode, ResString.GetMultilingualString("Freight|ScheduleRelatedJobTypes|AgencyBooking", "Agency Booking"));
		public static readonly ScheduleRelatedJobType AsycudaManifest = new AsycudaManifestScheduleRelatedJobType(AsycudaManifestCode, ResString.GetMultilingualString("Freight|ScheduleRelatedJobTypes|AsycudaManifest", "Global Manifest"));
		public static readonly ScheduleRelatedJobType BillOfLading = new BillOfLadingScheduleRelatedJobType(BillOfLadingCode, ResString.GetMultilingualString("Freight|ScheduleRelatedJobTypes|BillOfLading", "Bill of Lading"));
		public static readonly ScheduleRelatedJobType Cartage = new CartageScheduleRelatedJobType(CartageCode, ResString.GetMultilingualString("Freight|ScheduleRelatedJobTypes|Cartage", "Port Transport"));
		public static readonly ScheduleRelatedJobType CFSContainer = new CFSContainerScheduleRelatedJobType(CFSContainerCode, ResString.GetMultilingualString("Freight|ScheduleRelatedJobTypes|CFSContainer", "Container Registration"));
		public static readonly ScheduleRelatedJobType CFSLoadListConsol = new CFSLoadListConsolScheduleRelatedJobType(CFSLoadListConsolCode, ResString.GetMultilingualString("Freight|ScheduleRelatedJobTypes|CFSLoadList", "CFS Load List"));
		public static readonly ScheduleRelatedJobType CFSShipment = new CFSShipmentScheduleRelatedJobType(CFSShipmentCode, ResString.GetMultilingualString("Freight|ScheduleRelatedJobTypes|CFSShipment", "CFS Shipment"));
		public static readonly ScheduleRelatedJobType Consol = new ForwardingConsolScheduleRelatedJobType(ConsolCode, ResString.GetMultilingualString("Freight|ScheduleRelatedJobTypes|Consol", "Consol"));
		public static readonly ScheduleRelatedJobType CusISFHeader = new CusISFHeaderScheduleRelatedJobType(CusISFHeaderCode, ResString.GetMultilingualString("Freight|ScheduleRelatedJobTypes|CusISFHeader", "Importer Security Filing"));
		public static readonly ScheduleRelatedJobType Declaration = new DeclarationScheduleRelatedJobType(DeclarationCode, ResString.GetMultilingualString("Freight|ScheduleRelatedJobTypes|Declaration", "Declaration"));
		public static readonly ScheduleRelatedJobType DtbBooking = new DtbBookingScheduleRelatedJobType(DtbBookingCode, ResString.GetMultilingualString("Freight|ScheduleRelatedJobTypes|DtbBooking", "Transport Booking"));
		public static readonly ScheduleRelatedJobType ForwardingShipment = new ForwardingShipmentScheduleRelatedJobType(ForwardingShipmentCode, ResString.GetMultilingualString("Freight|ScheduleRelatedJobTypes|ForwardingShipment", "Forwarding Shipment"));
		public static readonly ScheduleRelatedJobType JobComInvoiceHeader = new JobComInvoiceHeaderScheduleRelatedJobType(JobComInvoiceHeaderCode, ResString.GetMultilingualString("Freight|ScheduleRelatedJobTypes|JobComInvoiceHeader", "Commercial Invoice"));
		public static readonly ScheduleRelatedJobType QuotedBooking = new QuotedBookingScheduleRelatedJobType(QuotedBookingCode, ResString.GetMultilingualString("Freight|ScheduleRelatedJobTypes|QuotedBooking", "Booking"));
		public static readonly ScheduleRelatedJobType ShipmentPreAdvice = new ShipmentPreAdviceSailingRelatedJobType(ShipmentPreAdviceCode, ResString.GetMultilingualString("Freight|ScheduleRelatedJobTypes|ShipmentPreAdvice", "Shipment Pre Advice"));
		public static readonly ScheduleRelatedJobType TransitReceiveConsignment = new ShipmentPreAdviceSailingRelatedJobType(TransitReceiveConsignmentCode, ResString.GetMultilingualString("Freight|ScheduleRelatedJobTypes|TransitReceiveConsignment", "Receive Consignment"));
		public static readonly ScheduleRelatedJobType TransitReceiveASN = new ShipmentPreAdviceSailingRelatedJobType(TransitReceiveASNCode, ResString.GetMultilingualString("Freight|ScheduleRelatedJobTypes|TransitReceiveASN", "Receive ASN"));
		public static readonly ScheduleRelatedJobType TransitDispatchLoadList = new ShipmentPreAdviceSailingRelatedJobType(TransitDispatchLoadListCode, ResString.GetMultilingualString("Freight|ScheduleRelatedJobTypes|TransitDispatchLoadList", "Dispatch Load List"));

		protected ScheduleRelatedJobTypes()
		{
			Add(AgencyBooking);
			Add(AsycudaManifest);
			Add(BillOfLading);
			Add(Cartage);
			Add(CFSContainer);
			Add(CFSLoadListConsol);
			Add(CFSShipment);
			Add(Consol);
			Add(CusISFHeader);
			Add(Declaration);
			Add(DtbBooking);
			Add(ForwardingShipment);
			Add(JobComInvoiceHeader);
			Add(QuotedBooking);
			Add(ShipmentPreAdvice);
			Add(TransitReceiveConsignment);
			Add(TransitReceiveASN);
			Add(TransitDispatchLoadList);
		}

		public static ScheduleRelatedJobTypes New() => new ScheduleRelatedJobTypes();

		public new ScheduleRelatedJobType this[string code] => (ScheduleRelatedJobType)base[code, StringComparison.Ordinal];
	}
}
