namespace Enterprise.Freight.Forwarding.DataTransfer
{
	static class ContainerAutomationEventContextType
	{
		public const string TransportLeg = "TransportLeg"; // Not translatable
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Not translatable")]
		public const string Carrier = "Carrier";
		public const string AcceptsOnlyProvidedContainers = "AcceptsOnlyProvidedContainers"; // Not translatable
		public const string CoLoadBookingReference = "CoLoadBookingReference";
		public const string CoLoadWithCode = "CoLoadWithCode";
		public const string CoLoadWithName = "CoLoadWithName";
		public const string CoLoadBillNumber = "CoLoadBillNumber";
		public const string CoLoadWithC1CCode = "CoLoadWithC1CCode";
		public const string ContainerMode = "ContainerMode";
		public const string TransportMode = "TransportMode";
	}
}
