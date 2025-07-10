using System.Diagnostics.CodeAnalysis;

namespace Enterprise.Tracking.Business
{
	[SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors")]
	[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter related")]
	public static class TrackingDeclarationFilterConstants
	{
		public const string DateAtOrigin = "Date at Origin";
		public const string ActiveStatus = "Active Status";
		public const string DomesticInternational = "Domestic / International";
		public const string ImporterCompanyName = "Importer Company Name";
		public const string SupplierCompanyName = "Supplier Company Name";
		public const string SendReceiveForwarders = "Shipment Send / Receive Forwarders";
		public const string Status = "Status";
		public const string LastEditTime = "Last Edit Time";
		public const string CreatedTime = "Created Time";
		public const string DeclarationCountry = "Declaration Country/Region (Not applicable to shipments)";

		[SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors")]
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter related")]
		public static class NoResultFilters
		{
			public const string FlagsFilter = "No result: flags filter";
			public const string GuidsFilter = "No result: guids filter";
			public const string GuidFilter = "No result: guid filter";
			public const string TextFilter = "No result: text filter";
			public const string NumberFilter = "No result: number filter";
			public const string DateFilter = "No result: date filter";
		}
	}
}
