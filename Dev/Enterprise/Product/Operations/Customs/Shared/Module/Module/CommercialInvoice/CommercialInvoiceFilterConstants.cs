using Enterprise.Customs.Business;

namespace Enterprise.Customs.Module
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter constant")]
	public static class CommercialInvoiceFilterConstants
	{
		public const string InvoiceNumber = "Invoice #";

		public const string Company = "Company";

		public const string ImporterSupplier = "Importer / Supplier";
		public const string Importer = "Importer";
		public const string Supplier = "Supplier";
		public const string ImporterName = "Importer Name";
		public const string SupplierName = "Supplier Name";
		public const string InvoiceDate = "Commercial Invoice Date";
		public const string InvoiceTotal = "Invoice Amount";
		public const string ShipmentType = "Shipment Type";
		public const string Branch = "Branch";
		public const string LineOrderNumber = "Inv. Line Order Number";
		public const string Vessel = "Vessel";
		public const string ETA = "ETA";
		public const string RoutingCarrier = "Routing Carrier";
		public const string LoadDischarge = "Routing Load/Discharge";
		public const string Load = "Load";
		public const string Discharge = "Discharge";
		public const string References = "References";
		public const string AttachedToDeclaration = InvoiceHeaderWithNoDeclarationCollection.AttachedToDeclarationFilterName;
	}
}
