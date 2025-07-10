using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.MultiLineAddInfos
{
	[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	public sealed class CusAddInfoTypeAttribute : Attribute
	{
		public CusAddInfoTypeAttribute(string typeCode)
		{
			TypeCode = typeCode;
		}
		public readonly string TypeCode;

		/// <summary>
		/// returns the CusAddInfoTypeAttribute for the type being passed
		/// </summary>
		/// <param name="type"></param>
		/// <exception cref="ArgumentException"></exception>
		/// <returns></returns>
		public static CusAddInfoTypeAttribute Get(Type type)
		{
			object[] typeAttributes = type.GetCustomAttributes(typeof(CusAddInfoTypeAttribute), false);
			if (typeAttributes.Length > 0)
			{
				return (CusAddInfoTypeAttribute)typeAttributes[0];
			}
			else
			{
				throw new ArgumentException("CusAddInfoTypeAttribute must be implemented", FormattableString.Invariant($"type {type.FullName}"));
			}
		}

		public static string GetTypeCodeFromAttribute(Type type)
		{
			Type baseType = type;

			while (baseType != null && (!baseType.IsGenericType || baseType.GetGenericTypeDefinition() != typeof(CusAddInfo<>)))
			{
				baseType = baseType.BaseType;
			}

			if (baseType == null)
			{
				throw new InvalidOperationException(ZString.Format("Type '{0}' is not inherited from dbo.CusAddInfo<>.", type?.Name));
			}

			Type addInfoType = baseType.GetGenericArguments()[0];

			object[] typeAttributes = addInfoType.GetCustomAttributes(typeof(CusAddInfoTypeAttribute), true);
			if (typeAttributes.Length == 0)
			{
				throw new InvalidOperationException(ZString.Format("Type '{0}' does not have {1}.", addInfoType.Name, nameof(CusAddInfoTypeAttribute)));
			}
			if (typeAttributes.Length > 1)
			{
				throw new InvalidOperationException(ZString.Format("Type '{0}' has more than one {1}.", addInfoType.Name, nameof(CusAddInfoTypeAttribute)));
			}
			return ((CusAddInfoTypeAttribute)typeAttributes[0]).TypeCode;
		}

		public static class Codes
		{
#if DEBUG
			public const string TypeCodeForTesting = "_._";
#endif
			public const string AURFPNumber = "RFP";
			public const string AUREG = OrgHeader.CusAddInfoTypeAttribute.Codes.AUREG;
			public const string AUREGContact = "CON";
			public const string AUCLR = "CLR";
			public const string AUROLL = "ROL";
			public const string AURecommendationLetter = "ARL";

			public const string CADutyAndTax = "CDT";
			public const string CACCN = "CAC";
			public const string CACSAMessage = OrgHeader.CusAddInfoTypeAttribute.Codes.CACSAMessage;

			// ---- Canada PGAs ----------------------------------
			public const string CAHCPGAHeader = "CHC";
			public const string CAComponent = "CCP";
			public const string CAPHACPGAHeader = "CPH";
			public const string CANRCanPGAHeader = "CNR";
			public const string CADFOPGAHeader = "CFO";
			public const string CAGACPGAHeader = "CGA";
			public const string CACFIAPGAHeader = "CCF";
			public const string CACNSCPGAHeader = "CCN";
			public const string CAECCCPGAHeader = "CEC";
			public const string CATCPGAHeader = "CTC";

			public const string GBTax = "GTX";
			public const string GBAllSimpleProperties = "GBA";
			public const string GbMawbExport = "GBM";
			public const string GBMaritimeUCNThatIsHeld = "UCN";
			public const string GbCcsukSpecialHandling = "GSH";
			public const string EuNctsResultsOfControl = "ROC";
			public const string EuNctsUnloadingRemark = "ULR";

			public const string JPAFRNewVesselVoyage = "VV1";
			public const string JPAFRBLLFunction = "BLL";

			public const string NZMAFFiles = "NMF";
			public const string NZMAFData = "NMD"; // Used by MAFPlugInSupportConsolWrapper
			public const string NZTSWCommodityData = "TCD";
			public const string NZTSWCommodityConstituentData = "TCC";
			public const string NZTSWCommodityItineraryData = "TCI";
			public const string NZTSWCommodityProductData = "TCP";
			public const string NZItemPackaging = "IPK";

			public const string SGCustomsProcedureCode = "CPC";

			public const string TWShippingIdentification = "SID";

			public const string USATF = "ATF";
			public const string USFDA = "USA";
			public const string USDOT = "UST";
			public const string USDOTVIN = "USV";
			public const string USFCC = "USC";
			public const string USDrawbackNAFTA = "USD";
			public const string USDrawbackAdditionalImportTariffNumber = "DTI";
			public const string USDrawbackOtherFee = "DOF";
			public const string USAIILine = "USE";
			public const string USITDoc = "USI";
			public const string USDeliveryOrderHeader = "UDH";
			public const string USDeliveryOrderLine = "UDL";
			public const string USDeliveryOrderContainer = "UDC";
			public const string USDeliveryOrderHazmat = "UDZ";
			public const string USPPQForm368Data = "UPQ";
			public const string USPGA = "USP";
			public const string USSCI = "SCI";
			public const string USPGACommon = "PGA";
			public const string USDisposition = "UDP";
			public const string USOGADisposition = "DOG";
			public const string USOGADispositionDetail = "UOD";
			public const string USITNumber = "ITN";
			public const string US7501DocPrinting = "US7";
			public const string USLinkedEntry = "ULE";
			public const string USDeclarationFSISCertificate = "FDH";
			public const string USFSISCertificate = "FSH";
			public const string USPGAVehicle = "VEH";
			public const string USPGAVehicleDetails = "VDE";
			public const string USAMS = "AMS";
			public const string USAMSLine = "AML";

			public const string USFSISLot = "FSL";
			public const string USWHSPack = "WPK";
			public const string USWHSPackLine = "WPL";
			public const string USPesticide = "PST";
			public const string USPesticideLine = "PSL";

			public const string USNHTSAHeader = "NTH";
			public const string USNHTSADetails = "NTD";
			public const string USNHTSAAdditionalNumber = "NTA";
			public const string USNHTSAPermitAndLicense = "NTP";
			public const string USNHTSADocument = "NTC";

			public const string USNMFSLine = "NFL";
			public const string USNMFSHarvestingDetail = "NFH";
			public const string USNMFSHarvestingVessel = "NFV";

			public const string USOMCHeader = "OMC";
			public const string USOMCDetails = "OMD";

			public const string USACEFDA = "FDA";
			public const string USLot = "LOT";
			public const string USLaceyActLicense = "LSN";
			public const string USFDALicense = "FLS";
			public const string USLaceyCountries = "LAC";

			public const string USTTBCigar = "TBC";
			public const string USTTBLine = "TBL";
			public const string USTTBCOLAAndCertificate = "TBP";

			public const string USAPHISHeader = "APH";
			public const string USAPHISIdentityNumberRange = "API";
			public const string USAPHISInspection = "APN";
			public const string USAPHISLicense = "APL";
			public const string USAPHISProduct = "APP";
			public const string USAPHISRouting = "APR";
			public const string USAPHISSource = "APS";

			public const string USFWSHeader = "FWH";
			public const string USFWSLicense = "FWL";

			public const string USDEAHeader = "DEA";
			public const string USDEAConstituent = "DEC";

			public const string USCPSCHeader = "CPS";
			public const string USCPSCRule = "CPR";
			public const string USCPSCReport = "CPT";

			public const string USHFCHeader = "HFC";
			public const string USHFCDetail = "HFD";

			public const string USWarehouseDetail = "UWD";

			public const string WarehouseCustomsAddInfo = "WCA";
			public const string WarehouseAllocationInfo = "ALI";

			// ---- CN ----------------------------------
			public const string CIQRequiredDocument = "RQD";
			public const string CNVINData = "VID";
		}
	}
}
