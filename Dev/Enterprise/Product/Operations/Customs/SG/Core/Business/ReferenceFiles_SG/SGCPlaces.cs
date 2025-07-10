
namespace Enterprise.Customs.SG.V4.Business
{
	internal class SGCPlaces
	{
		#region Constants

		public static class Constants
		{
			public const string MajorExporterScheme = "ME";
			public const string ApprovedImportGSTSuspensionScheme = "AISS";
			public const string ImportGSTDefermentScheme = "IGDS";
			public const string ExemptPlaceCodePresident = "P";

			public static class SupplierExemptLocation
			{
				public const string ApprovedImportSuspensionSchemeLocal = "AISSLOC";
				public const string Embassy = "EM";
				public const string ExemptionOnMotorVehicle = "EXEMPT";
			}

			public static class PremiseType
			{
				public const string BondedWarehouse = "BW";
				public const string BondedWarehouseClass2Yard = "BWCY";
				public const string Class2Yard = "C2Y";
				public const string ContainerFreightWarehouse = "CFW";
				public const string LicensedWarehouse = "LW";
				public const string ShipYard = "SY";
				public const string SailingClub = "SC";
				public const string Others = "O";
			}

			public static class FreeTradeZones
			{
				public const string ChangiFTZ = "CZ";
				public const string JurongFTZ = "JZ";
				public const string KeppelFTZ = "KZ";
				public const string PasirPanjangFTZ = "PPZ";
				public const string SembawangFTZ = "SZ";
			}

			public static class NotToUseForReceiptRelease
			{
				public const string ContainerWharves = "CW";
				public const string MarinaWharves = "MW";
				public const string KeppelWharves = "KW";
				public const string JurongWharves = "JW";
				public const string PasirPanjangWharves = "PPW";
				public const string SembawangWharves = "SW";
				public const string AirCargoTransitBond = "ATB";
			}

			public static class ShortPayment
			{
				public const string ShortPaymentNotInvolvingUpdates = "SPNOSTK";
				public const string ShortPaymentInvolvingUpdates = "SPSTK";
				public const string ShortPaymentImportGSTDefermentScheme = "SPIGDS";
			}

			public static class RecoveryPayment
			{
				public const string RecoveryPaymentNotInvolvingUpdates = "RCNOSTK";
			}

			public static class ReceiptRelease
			{
				public const string UseFTZPlaceOfReceiptRelease = "The location code used should not be declared as a Place of Release and/or Receipt. For cargo clearance purposes, please declare the nearest FTZ as appropriate: \r\nContainer Wharves (CW), Marina Wharves (MW) or Keppel Wharves (KW), use Keppel Free Trade Zone (KZ) \r\nJurong Wharves (JW), use Jurong Free Trade Zone (JZ) \r\nPasir Panjang Wharves (PPW), use Pasir Panjang Free Trade Zone (PPZ) \r\nSembawang Wharves (SW), use Sembawang Free Trade Zone (SZ) \r\nAir Cargo Transit Bond (ATB), use Changi FTZ (CZ)";
				public const string AISSNotValidForRelease = "Approved Import GST Suspension Scheme (AISS) exemption code is not valid for Place of Release.";
				public const string AISSInvalidUseForReceipt = "Invalid use of Approved Import GST Suspension Scheme (AISS) exemption code for Place of Receipt. \r\nAISS can be used on the In-Non-Payment message type with Declaration Type Approved Premise/Scheme (INP/APS).";
				public const string IGDSCorrectUsageForINP = "Import GST Deferment Scheme (IGDS) is only valid on INP Permit applications for Approved Premises/Schemes (APS) or GST releif and/or duty exemption (GTR) Declaration Types.";
				public const string IGDSCorrectUsageForIPT = "Import GST Deferment Scheme (IGDS) is only valid on IPT Permit applications for Duty (DUT) Declaration Type.";
				public const string IGDSNotValidForRelease = "Import GST Deferment Scheme (IGDS) exemption code is not valid for Place of Release.";
				public const string IGDSInvalidUseForReceipt = "Invalid use of Import GST Deferment Scheme (IGDS) exemption code for Place of Receipt. \r\nIGDS can be used for the: \r\nImport of non-dutiable goods with GST under deferment (INP/APS); \r\nImport of dutiable goods with duty exempted and GST under deferment (INP/GTR); and \r\nImport of dutiable goods with duty payable and GST under deferment (IPT/DUT).";
			}
		}

		#endregion
	}
}

