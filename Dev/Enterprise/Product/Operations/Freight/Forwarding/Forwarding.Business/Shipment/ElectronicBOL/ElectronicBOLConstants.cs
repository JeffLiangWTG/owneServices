namespace Enterprise.Freight.Forwarding.Business
{
	public static class ElectronicBOLConstants
	{
		public const string DirectXTClientID = "ELECTRONIC_BILL_OF_LADING";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Electronic Bill Of Lading Event Departments")]
		public static class EHBLEventDepartments
		{
			public const string Carrier = "Carrier";
			public const string TitleRegistry = "Title Registry";
		}
	}
}
