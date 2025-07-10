using System.Collections.Generic;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.eTail.Integration
{
	public static class HVLVConstants
	{
		public static List<string> InvalidUsers = new List<string> { User.ServiceUserCode, User.SupportUserCode, User.UnKnownUserCode, User.InterchangeUserCode, User.WebUserCode };

		public static class ItemReferenceTypes
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "constant text")]
			public const string Barcode = "ITEM BARCODE";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "constant text")]
			public const string ShipperReference = "ITEM SHIPPER REFERENCE";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "constant text")]
			public const string ItemId = "ITEM ID";
		}

		public static class UsageCategories
		{
			public const string HighValueDeclaration = "HVD";
			public const string LowValueDeclaration = "LVD";
			public const string SecurityFiling = "SEC";
			public const string CargoWiseOneUsage = "CW1";
			public const string WebUsage = "WEB";

			public static Dictionary<string, string> LookupByUsageCode => new Dictionary<string, string>
			{
				{ UsageCodes.AUAirCargoReport, LowValueDeclaration },
				{ UsageCodes.AUSeaCargoReport, LowValueDeclaration },
				{ UsageCodes.EUICS2Manifest, SecurityFiling },
				{ UsageCodes.EUH7Declaration, LowValueDeclaration },
				{ UsageCodes.NZAirCRE, LowValueDeclaration },
				{ UsageCodes.NZAirICR, LowValueDeclaration },
				{ UsageCodes.NZSeaCRE, LowValueDeclaration },
				{ UsageCodes.NZSeaICR, LowValueDeclaration },
				{ UsageCodes.SGSingaporeACCESS, LowValueDeclaration },
				{ UsageCodes.TRETradeManifest, LowValueDeclaration },
				{ UsageCodes.TWBriefCustomsDeclaration, LowValueDeclaration },
				{ UsageCodes.TWForwarderManifest, SecurityFiling },
				{ UsageCodes.USAirAMS, SecurityFiling },
				{ UsageCodes.USImporterSecurityFiling, SecurityFiling },
				{ UsageCodes.USRoadEManifest, LowValueDeclaration },
				{ UsageCodes.USSeaAMS, SecurityFiling },
				{ UsageCodes.USLowValueEntries, LowValueDeclaration },
				// Global
				{ UsageCodes.ACAS, SecurityFiling },
				{ UsageCodes.DeniedPartyScreening, SecurityFiling },
				{ UsageCodes.FHLAirlineMessaging, SecurityFiling },
				{ UsageCodes.ImportStandAloneDeclaration, HighValueDeclaration },
				{ UsageCodes.ExportStandAloneDeclaration, HighValueDeclaration },
				{ UsageCodes.CargoWiseUsage, CargoWiseOneUsage },
				// Glow
				{ UsageCodes.GlowContactAddUsage, WebUsage },
				{ UsageCodes.GlowStaffAddUsage, WebUsage },
				{ UsageCodes.GlowContactEditUsage, WebUsage },
				{ UsageCodes.GlowStaffEditUsage, WebUsage },
			};
		}

		public static class UsageCodes
		{
			#region AU

			public const string AUAirCargoReport = "ACR";
			public const string AUSeaCargoReport = "SCR";

			#endregion

			#region EU

			public const string EUICS2Manifest = "IC2";
			public const string EUH7Declaration = "H7D";

			#endregion

			#region NZ

			public const string NZAirCRE = "ANC";
			public const string NZAirICR = "ANI";
			public const string NZSeaCRE = "SNC";
			public const string NZSeaICR = "SNI";

			#endregion

			#region SG

			public const string SGSingaporeACCESS = "SGA";

			#endregion

			#region TR

			public const string TRETradeManifest = "TET";

			#endregion

			#region TW

			public const string TWBriefCustomsDeclaration = "TCD";
			public const string TWForwarderManifest = "TWM";

			#endregion

			#region US

			public const string USAirAMS = "UAM";
			public const string USImporterSecurityFiling = "USF";
			public const string USRoadEManifest = "USR";
			public const string USSeaAMS = "USM";
			public const string USLowValueEntries = "USC";

			#endregion

			#region Global

			public const string ACAS = "ACA";
			public const string DeniedPartyScreening = "DPS";
			public const string FHLAirlineMessaging = "FHL";
			public const string ExportStandAloneDeclaration = "DEE";
			public const string ImportStandAloneDeclaration = "DEC";
			public const string CargoWiseUsage = "CWU";
			public const string CargoWiseUsageByOtherCompany = "CWE";

			#endregion

			#region Glow

			public const string GlowContactAddUsage = "GLC";
			public const string GlowStaffAddUsage = "GLS";
			public const string GlowContactEditUsage = "GCE";
			public const string GlowStaffEditUsage = "GSE";

			#endregion
		}

		public static class HVLVConsignmentHeaderUsageTypesCodes
		{
			public const string Plus = "P";
			public const string Standard = "S";
		}
	}
}
