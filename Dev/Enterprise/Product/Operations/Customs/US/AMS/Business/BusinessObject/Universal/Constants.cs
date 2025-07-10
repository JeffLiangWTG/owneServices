namespace Enterprise.Customs.US.AMS.Business.Universal
{
	public static class Constants
	{
		public static class Header
		{
			public static class AddInfo
			{
				public const string CarrierSCAC = "CarrierSCAC";
				public const string FIRMS = "FIRMS";
				public const string IsOutboundCargo = "IsOutboundCargo";
				public const string IsPaperlessMIBParticipant = "IsPaperlessMIBParticipant";
				public const string PortOfDischargeScheduleD = "PortOfDischargeScheduleD";
				public const string UniqueVoyageIdentifier = "UniqueVoyageIdentifier";
			}
		}

		public static class Bill
		{
			public static class AddInfo
			{
				public const string ShipmentType = "ShipmentType";
				public const string IssuerCode = "IssuerCode";
				public const string BillStatus = "BillStatus";
				public const string PortOfLading = "PortOfLading";
				public const string PortOfLadingScheduleK = "PortOfLadingScheduleK";
				public const string PlaceOfReceiptScheduleD = "PlaceOfReceiptScheduleD";
				public const string TransportModeToPortOfLading = "TransportModeToPortOfLading";
				public const string LastForeignPort = "LastForeignPort";
				public const string LastForeignPortScheduleK = "LastForeignPortScheduleK";
				public const string ForeignPortOfContract = "ForeignPortOfContract";
				public const string ForeignPortOfContractScheduleK = "ForeignPortOfContractScheduleK";
				public const string Weight = " Weight";
				public const string WeightUnit = "WeightUnit";
				public const string Volume = "Volume";
				public const string VolumeUnit = "VolumeUnit";
				public const string PortOfUnlading = "PortOfUnlading";
				public const string PortOfUnladingScheduleD = "PortOfUnladingScheduleD";
				public const string EstimatedUnloadDate = "EstimatedUnloadDate";
				public const string MasterInBondIndicator = "MasterInBondIndicator";
				public const string FIRMS = "FIRMS";
				public const string PaymentMethod = "PaymentMethod";
				public const string ActionCode = "ActionCode";
				public const string AmendmentCode = "AmendmentCode";
			}
		}

		public static class Container
		{
			public static class AddInfo
			{
				public const string ForeignPort = "ForeignPort";
				public const string ForeignPortScheduleK = "ForeignPortScheduleK";
				public const string TypeOfService = "TypeOfService";
			}
		}

		public static class PortArrivalDate
		{
			public const string GroupTypeCode = "ARD";
			public const string GroupTypeDescription = "Arrival Date";

			public static class AddInfo
			{
				public const string PortCode = "PortCode";
				public const string ArrivalDate = "ArrivalDate";
			}
		}

		public static class AdditionalReference
		{
			public const string Type = "ADR";
			public const string TypeDescription = "Additional Reference";
		}

		public static class SecondaryNotifyParty
		{
			public const string Type = "SNP";
			public const string TypeDescription = "Secondary Notify Party";
		}

		public static class VehicleReference
		{
			public const string Type = "VIN";
			public const string TypeDescription = "Vehicle";
		}
	}
}
