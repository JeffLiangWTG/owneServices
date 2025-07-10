using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.TR.Manifest.Business
{
	public static class TRManifestMessageHelper
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public static ZBool IsForbidden(ZString fieldName, ZString manifestTypeCode, string transportMode = "")
		{
			switch (manifestTypeCode)
			{
				case TRManifestTypes.Codes.CIKONC:
					return IsForbidden(fieldName, new[] { "NumberofBillsInDeclaration", ManifestBase.AutoAsycudaBill.Schema.ABL_TransportValue, ManifestBase.AutoAsycudaBill.Schema.ABL_RX_NKTransportValueCurrency, AsycudaManifestHeader.Schema.TR_GM_PresentationCustomsOffice, nameof(VisitedCountryProvider.MovementDateTime) });

				case TRManifestTypes.Codes.TESLIM:
					return IsForbidden(fieldName, new[] { "NumberofBillsInDeclaration" });

				case TRManifestTypes.Codes.DEMIHR:
				case TRManifestTypes.Codes.DEMITH:
				case TRManifestTypes.Codes.DENIHR:
				case TRManifestTypes.Codes.TIRIHR:
				case TRManifestTypes.Codes.TIRITH:
				case TRManifestTypes.Codes.VARONC:
				case TRManifestTypes.Codes.EMANIF:
					return IsForbidden(fieldName, new[] { ManifestBase.AutoAsycudaBill.Schema.ABL_TransportValue, ManifestBase.AutoAsycudaBill.Schema.ABL_RX_NKTransportValueCurrency });

				case TRManifestTypes.Codes.DENITH:
					return IsForbidden(fieldName, new[] { ManifestBase.AutoAsycudaBill.Schema.ABL_TransportValue, ManifestBase.AutoAsycudaBill.Schema.ABL_RX_NKTransportValueCurrency, "Bill.CustomsEntryNumber" });

				case TRManifestTypes.Codes.DIGIHR:
				case TRManifestTypes.Codes.DIGITH:
					return IsForbidden(fieldName, new[] { ManifestBase.AutoAsycudaBill.Schema.ABL_RX_NKTransportValueCurrency });

				case TRManifestTypes.Codes.GRUPAJ:
					var forbiddenList = new List<string> {  ManifestBase.AutoAsycudaManifestHeader.Schema.AMA_Trailer1RegNo,
															ManifestBase.AutoAsycudaManifestHeader.Schema.AMA_RN_NKTrailer1RegCountry,
															ManifestBase.AutoAsycudaManifestHeader.Schema.AMA_Trailer2RegNo,
															ManifestBase.AutoAsycudaManifestHeader.Schema.AMA_RN_NKTrailer2RegCountry,
															ManifestBase.AutoAsycudaManifestHeader.Schema.AMA_AgentType,
															ManifestBase.AutoAsycudaManifestHeader.Schema.AMA_Voyage,
															ManifestBase.AutoAsycudaManifestHeader.Schema.AMA_RN_NKConveyanceNationality,
															CusEntryNumber.Schema.CE_EntryNum };
					if (transportMode != TransportTypeList.Codes.Air)
					{
						forbiddenList.Add(ManifestBase.AutoAsycudaBill.Schema.ABL_TransportValue);
						forbiddenList.Add(ManifestBase.AutoAsycudaBill.Schema.ABL_RX_NKTransportValueCurrency);
					}
					return IsForbidden(fieldName, forbiddenList.ToArray());

				default:
					return false;
			}
		}

		static bool IsForbidden(string fieldName, string[] fields)
		{
			return Array.IndexOf(fields, fieldName) >= 0;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Type strings")]
		public static ZString TRManifestType(ZString manifestTypeCode)
		{
			switch (manifestTypeCode)
			{
				case TRManifestTypes.Codes.ATAIHR:
					return "ATAİHR";
				case TRManifestTypes.Codes.ATAITH:
					return "ATAİTH";
				case TRManifestTypes.Codes.CIKONC:
					return "CIKONC";
				case TRManifestTypes.Codes.DEMIHR:
					return "DEMİHR";
				case TRManifestTypes.Codes.DEMITH:
					return "DEMİTH";
				case TRManifestTypes.Codes.DENIHR:
					return "DENİHR";
				case TRManifestTypes.Codes.DENITH:
					return "DENİTH";
				case TRManifestTypes.Codes.DIGIHR:
					return "DIGİHR";
				case TRManifestTypes.Codes.DIGITH:
					return "DIGİTH";
				case TRManifestTypes.Codes.HAVIHR:
					return "HAVİHR";
				case TRManifestTypes.Codes.HAVITH:
					return "HAVİTH";
				case TRManifestTypes.Codes.TESLIM:
					return "TESLİM";
				case TRManifestTypes.Codes.TIRIHR:
					return "TIRİHR";
				case TRManifestTypes.Codes.TIRITH:
					return "TIRİTH";
				case TRManifestTypes.Codes.GRUPAJ:
					return "GRUPAJ";
				case TRManifestTypes.Codes.VARONC:
					return "VARONC";
				case TRManifestTypes.Codes.EMANIF:
					return "EMANIF";
				default:
					return ZString.Empty;
			}
		}

		public static ZBool IsManifestContainer(AsycudaBill bill)
		{
			ZBool returnValue = false;
			if (bill.ContainersOnThisBill.Any())
			{
				var firstContainer = bill.ContainersOnThisBill.First();
				if (firstContainer != null && !firstContainer.ACN_ContainerNumber.IsEmpty)
				{
					returnValue = true;
				}
			}
			return returnValue;
		}
	}
}
