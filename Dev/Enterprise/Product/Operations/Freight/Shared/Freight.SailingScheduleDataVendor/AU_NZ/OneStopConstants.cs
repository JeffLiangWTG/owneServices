using System;
using Enterprise.ZArchitecture.Core.Encryption;

namespace Enterprise.Freight.SailingDataVendor.Business
{
	internal static class OneStopConstants
	{
		public const string EDIABN = "41065894724";
		public const string UserID = "EDalerts";

		public static string PIN
		{
			get { return TwoWayEncoder.Decrypt("rGEY7v6JZnedoiWbCNRFsdUEFcOyOuhymZNJUkvtDDw="); } // eda1ert5
		}

		static TwoWayEncoder TwoWayEncoder
		{
			get { return new TwoWayEncoder(new Guid("{C499311E-3D5A-435a-BBC5-F1B2D627FFF7}")); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "hard-coded constant")]
		public const string ContainerEventResponseEmailSubjectPrefix = "1-STOP NOTIFY*";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "hard-coded constant")]
		public const string ContainerEventsExpiredResponseEmailSubjectPrefix = "1-STOP ALERT*";
		public const string ContainerEventResponseEmailAddress = "helpdesk@1-stop.biz";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "hard-coded constant, filter related")]
		public static class ContainerEventTypes
		{
			public const string GateIn = "Gate In";
			public const string GateOut = "Gate Out";
			public const string LoadOnVessel = "Load on Vessel";
			public const string ImportPreAdvised = "On Board Vessel";
			public const string DischargeOffVessel = "Discharge off Vessel";
			public const string ExportPreAdvised = "Export Pre-Advise";
			public const string Dehire = "Dehire";
			public const string StorageStart = "Storage Start";
			public const string ImportAvailable = "Import Available";
		}
	}
}
