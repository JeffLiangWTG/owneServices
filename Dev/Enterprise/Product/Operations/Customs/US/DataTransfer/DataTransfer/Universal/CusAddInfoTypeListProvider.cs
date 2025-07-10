using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.DataTransfer.Universal
{
	static class CusAddInfoTypeListProvider
	{
		public static class AdditionalAddInfoType
		{
			public static class FDARelatedBill
			{
				public const string Type = "FRB";
				public const string MasterBill = "MasterBill";
				public const string MasterBillIssuerCode = "MasterBillIssuerCode";
				public const string HouseBill = "HouseBill";
				public const string HouseBillIssuerCode = "HouseBillIssuerCode";
			}

			public static class FDARelatedContainer
			{
				public const string Type = "FRC";
				public const string ContainerNumber = "ContainerNumber";
				public const string SealNumber = "SealNumber";
			}

			public static class InvoiceLinePackData
			{
				public const string PackedQty = "PackedQty";
				public const string PackageID = "PackageID";
				public const string PackageQty = "PackageQty";
				public const string IsSimplePackagingStyle = "IsSimplePackagingStyle";
				public const string CodeDescription = "WHS Pack Line";
			}
		}

		public static ICodeDescriptionPairList TableSpecificCusAddInfoTypeList(ZString tableCode)
		{
			ICodeDescriptionPairList result = null;
			switch (tableCode)
			{
				case JobDeclarationSchema.Constants.Prefix:
					result = GetListForJobDeclaration();
					break;
				case CusEntryHeaderSchema.Constants.Prefix:
					result = GetListForCusEntryHeader();
					break;
				case CusAddInfoSchema.Constants.Prefix:
					result = GetListForCusAddInfo();
					break;
				case CusDecHouseBillSchema.Constants.Prefix:
					result = GetListForBill();
					break;
				case JobComInvoiceLineSchema.Constants.Prefix:
					result = GetListForJobComInvoiceLine();
					break;
				case CusClassPartPivotSchema.Constants.Prefix:
					result = GetListForCusClassPartPivot();
					break;
				case CusContainerSchema.Constants.Prefix:
					result = GetListForCusContainer();
					break;
			}
			return result;
		}

		#region Implementation

		static CodeDescriptionPairList GetListForJobDeclaration()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(CusAddInfoTypeAttribute.Codes.USLinkedEntry, "Linked Entry");
			// Don't export CusAddInfoTypeAttribute.Codes.USDeliveryOrderHeader as it's internal printing data
			// Don't export CusAddInfoTypeAttribute.Codes.USFSISForm9540Header as it's internal printing data
			// Don't export CusAddInfoTypeAttribute.Codes.USPPQForm368Data as it's internal printing data
			result.AddPair(CusAddInfoTypeAttribute.Codes.USITDoc, "IT Doc");
			result.AddPair(CusAddInfoTypeAttribute.Codes.USOGADisposition, "OGA Disposition");
			result.AddPair(CusAddInfoTypeAttribute.Codes.USDisposition, "Disposition");
			result.AddPair(CusAddInfoTypeAttribute.Codes.USWHSPack, "Warehouse Pack");
			result.AddPair(CusAddInfoTypeAttribute.Codes.USDeclarationFSISCertificate, "Declaration FSIS Certificate");
			return result;
		}

		static CodeDescriptionPairList GetListForCusEntryHeader()
		{
			// Don't export CusAddInfoTypeAttribute.Codes.US7501DocPrinting as it's internal printing data
			// CusCodeDataTypeList.Codes.PSCReasonCodes is special handled
			return null;
		}

		static CodeDescriptionPairList GetListForCusAddInfo()
		{
			var result = new CodeDescriptionPairList();
			// Don't export CusAddInfoTypeAttribute.Codes.USDeliveryOrderContainer as it's internal printing data
			// Don't export CusAddInfoTypeAttribute.Codes.USDeliveryOrderHazmat as it's internal printing data
			// Don't export CusAddInfoTypeAttribute.Codes.USDeliveryOrderLine as it's internal printing data
			// Don't export CusAddInfoTypeAttribute.Codes.USFSISForm9540Line as it's internal printing data
			result.AddPair(CusAddInfoTypeAttribute.Codes.USDOTVIN, "DOT VIN");
			result.AddPair(CusAddInfoTypeAttribute.Codes.USPGA, "PGA");
			result.AddPair(CusAddInfoTypeAttribute.Codes.USSCI, "SCI");
			result.AddPair(CusAddInfoTypeAttribute.Codes.USFSISLot, "FSIS Lot");
			result.AddPair(CusAddInfoTypeAttribute.Codes.USPGAVehicleDetails, "PGA Vehicle Details");
			result.AddPair(CusAddInfoTypeAttribute.Codes.USAMSLine, "PGA AMS Line");
			result.AddPair(CusAddInfoTypeAttribute.Codes.USPesticideLine, "EPA Pesticide Line");
			result.AddPair(CusAddInfoTypeAttribute.Codes.USNHTSADetails, "PGA NHTSA Details");
			result.AddPair(CusAddInfoTypeAttribute.Codes.USNHTSAAdditionalNumber, "PGA NHTSA Additional Number");
			result.AddPair(CusAddInfoTypeAttribute.Codes.USNHTSAPermitAndLicense, "PGA NHTSA Permit And License");
			result.AddPair(CusAddInfoTypeAttribute.Codes.USNHTSADocument, "PGA NHTSA Document");
			result.AddPair(CusAddInfoTypeAttribute.Codes.USNMFSHarvestingDetail, "PGA NMFS Harvesting Detail");
			result.AddPair(CusAddInfoTypeAttribute.Codes.USNMFSHarvestingVessel, "PGA NMFS Vessel Detail");
			result.AddPair(CusAddInfoTypeAttribute.Codes.USLot, "PGA FDA LOT");
			result.AddPair(CusAddInfoTypeAttribute.Codes.USLaceyActLicense, "PGA Lacey Act License");
			result.AddPair(CusAddInfoTypeAttribute.Codes.USFDALicense, "PGA FDA License");
			result.AddPair(CusAddInfoTypeAttribute.Codes.USTTBCigar, "PGA TTB Cigar");
			result.AddPair(CusAddInfoTypeAttribute.Codes.USTTBCOLAAndCertificate, "PGA TTB COLA And Certificate");
			result.AddPair(CusAddInfoTypeAttribute.Codes.USAPHISIdentityNumberRange, "PGA APHIS Identity Number Range");
			result.AddPair(CusAddInfoTypeAttribute.Codes.USAPHISInspection, "PGA APHIS Inspection");
			result.AddPair(CusAddInfoTypeAttribute.Codes.USAPHISLicense, "PGA APHIS License");
			result.AddPair(CusAddInfoTypeAttribute.Codes.USAPHISProduct, "PGA APHIS Product");
			result.AddPair(CusAddInfoTypeAttribute.Codes.USAPHISRouting, "PGA APHIS Routing");
			result.AddPair(CusAddInfoTypeAttribute.Codes.USAPHISSource, "PGA APHIS Source");
			result.AddPair(CusAddInfoTypeAttribute.Codes.USOMCDetails, "PGA OMC Details");
			result.AddPair(CusAddInfoTypeAttribute.Codes.USFWSLicense, "PGA FWS License");
			result.AddPair(CusAddInfoTypeAttribute.Codes.USCPSCRule, "PGA CPSC Rule And Lab");
			result.AddPair(CusAddInfoTypeAttribute.Codes.USDEAConstituent, "PGA DEA Constituent");
			result.AddPair(CusAddInfoTypeAttribute.Codes.USHFCDetail, "PGA HFC Detail");
			return result;
		}

		static CodeDescriptionPairList GetListForBill()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(CusAddInfoTypeAttribute.Codes.USITDoc, "IT Doc");
			result.AddPair(CusAddInfoTypeAttribute.Codes.USITNumber, "IT Number");
			result.AddPair(CusAddInfoTypeAttribute.Codes.USDisposition, "Disposition");
			return result;
		}

		static CodeDescriptionPairList GetListForJobComInvoiceLine()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(CusAddInfoTypeAttribute.Codes.USAIILine, "AII Line");
			result.AddPair(CusAddInfoTypeAttribute.Codes.USFDA, "FDA");
			result.AddPair(CusAddInfoTypeAttribute.Codes.USDOT, "DOT");
			result.AddPair(CusAddInfoTypeAttribute.Codes.USFCC, "FCC");
			result.AddPair(CusAddInfoTypeAttribute.Codes.USPGACommon, "PGA Common");
			result.AddPair(CusAddInfoTypeAttribute.Codes.USDrawbackNAFTA, "Drawback NAFTA");
			result.AddPair(CusAddInfoTypeAttribute.Codes.USFSISCertificate, "FSIS Certificate");
			result.AddPair(CusAddInfoTypeAttribute.Codes.USATF, "PGA ATF");
			result.AddPair(CusAddInfoTypeAttribute.Codes.USPGAVehicle, "PGA Vehicle");
			result.AddPair(CusAddInfoTypeAttribute.Codes.USAMS, "PGA AMS");
			result.AddPair(CusAddInfoTypeAttribute.Codes.USPesticide, "EPA Pesticide");
			result.AddPair(CusAddInfoTypeAttribute.Codes.USNHTSAHeader, "PGA NHTSA Header");
			result.AddPair(CusAddInfoTypeAttribute.Codes.USNMFSLine, "PGA NMFS Line");
			result.AddPair(CusAddInfoTypeAttribute.Codes.USACEFDA, "PGA ACE FDA");
			result.AddPair(CusAddInfoTypeAttribute.Codes.USHFCHeader, "PGA HFC Header");
			result.AddPair(CusAddInfoTypeAttribute.Codes.USOMCHeader, "PGA OMC Header");
			result.AddPair(CusAddInfoTypeAttribute.Codes.USTTBLine, "PGA TTB Line");
			result.AddPair(CusAddInfoTypeAttribute.Codes.USAPHISHeader, "PGA APHIS Header");
			result.AddPair(CusAddInfoTypeAttribute.Codes.USFWSHeader, "PGA FWS Header");
			result.AddPair(CusAddInfoTypeAttribute.Codes.USCPSCHeader, "PGA CPSC Header");
			result.AddPair(CusAddInfoTypeAttribute.Codes.USDEAHeader, "PGA DEA Header");
			result.AddPair(CusAddInfoTypeAttribute.Codes.USDrawbackOtherFee, "Drawback Other Fee");
			result.AddPair(CusAddInfoTypeAttribute.Codes.USDrawbackAdditionalImportTariffNumber, "Drawback Additional Import Tariff");
			return result;
		}

		static CodeDescriptionPairList GetListForCusContainer()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(CusAddInfoTypeAttribute.Codes.USDisposition, "Disposition");
			return result;
		}

		static CodeDescriptionPairList GetListForCusClassPartPivot()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(CusAddInfoTypeAttribute.Codes.USFDA, "FDA");
			result.AddPair(CusAddInfoTypeAttribute.Codes.USDOT, "DOT");
			result.AddPair(CusAddInfoTypeAttribute.Codes.USFCC, "FCC");
			result.AddPair(CusAddInfoTypeAttribute.Codes.USPGACommon, "PGA Common");
			return result;
		}

		#endregion
	}
}
