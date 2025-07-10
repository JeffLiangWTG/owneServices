namespace Enterprise.Customs.Business
{
	public static class WarehouseConstants
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Exact Note Description for hidden data.")]
		public const string UniversalHoldShipmentNoteDescription = "Universal Shipment Warehouse Hold";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Exact Note Description for hidden data.")]
		public const string UniversalModificationShipmentNoteDescription = "Universal Shipment Warehouse Modification";

		public static class WarehouseType
		{
			public const string FreeTradeZone = "FTZ";
			public const string Product = "PRW";
		}
	}
}
