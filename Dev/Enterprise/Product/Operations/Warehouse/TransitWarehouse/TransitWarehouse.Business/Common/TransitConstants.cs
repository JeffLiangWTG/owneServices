using System;
using System.Collections.Generic;
using Enterprise.UniversalDataBuss.Integration;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Warehouse.Transit.Business
{
	public static class TransitConstants
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Event reference parameters")]
		public const string TransitDispatchJobType = "Transit Dispatch";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Event reference parameters")]
		public const string TransitReceiveJobType = "Transit Receive";

		[ThreadSafe]
		public static Lazy<Dictionary<string, int>> TransitDataContextPriority
			= new Lazy<Dictionary<string, int>>(() =>
			{
				var priorityDictionary = new Dictionary<string, int>();
				priorityDictionary.Add(nameof(DataContextType.ForwardingShipment), 3);
				priorityDictionary.Add(nameof(DataContextType.AirManifestLine), 2);
				priorityDictionary.Add(nameof(DataContextType.Outturn), 2);
				priorityDictionary.Add(nameof(DataContextType.LandTransportConsignment), 1);
				return priorityDictionary;
			});

		#region DGSource

		public static class DGSource
		{
			public const string CountryReference = "DCR";
			public const string UNDGClass = "CLS";
			public const string UNDGSubstance = "DG";
		}

		#endregion

		#region TransitPackageUnitType

		public static class TransitPackageUnitType
		{
			public const string Package = "PKG";
			public const string HandlingUnit = "HU";
			public const string SeaContainer = "CNT";
			public const string AirContainer = "ULD";
			public const string Packline = "PKL";
			public const string Overpack = "OVP";
		}

		#endregion

		#region TransportCertType

		public static class TransportCertType
		{
			public const string CertifiedHaulier = "CH";
			public const string ApprovedHaulier = "AH";
			public const string RegulatedAgentACENotice = "RA";
			public const string RegulatedAgentEACENotice = "RE";
			public const string AccreditedAgent = "AA";
			public const string KnownConsignor = "KC";
			public const string AccountConsignor = "AC";
			public const string RegularCustomer = "RC";
			public const string NoCertAssigned = "NO";
		}

		#endregion

		#region GateEventContextType

		public static class GateEventContextType
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Event context parameters")]
			public const string Direction = "Direction";
			public const string GateBookingNumber = "GateBookingNumber";
			public const string MovementBookingNumber = "MovementBookingNumber";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Event context parameters")]
			public const string Dock = "Dock";
		}

		#endregion

		#region TransitCycleCountLocationStatus

		public static class TransitCycleCountLocationStatus
		{
			public const string NotStarted = "NST";
			public const string ProcessingVariance = "PCV";
			public const string InProgress = "INP";
			public const string Completed = "CMP";
			public const string Error = "ERR";
		}

		#endregion

		#region TransportDirection

		public static class TransportDirection
		{
			public const string Delivery = "DLV";
			public const string Pickup = "PIC";
		}

		#endregion
	}
}
