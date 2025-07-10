using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.SG.V4.Business
{
	public static class SGPlacesRefCusCodeList
	{
		public static ZZRefCusCodeListCombined GetCurrentOrMatchingPlace(BusinessObjectFactory factory, ZString placeCode) => factory.GetCachedValue("GetCurrentOrMatchingPlace" + placeCode, delegate
		{
			return ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(factory, placeCode, Core.Constants.CountryCodes.Singapore,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, ZDateTime.Now);
		});

		public static ZString GetSGCType(this ZZRefCusCodeListCombined place)
		{
			return place?.GetAttribute(UniversalReferenceConstants.RefCusCodeList.Attributes.SGCType) ?? ZString.Empty;
		}

		public static bool IsValidForGUIUse(this ZZRefCusCodeListCombined place)
		{
			var code = place?.ZZD_Code ?? string.Empty;
			return !(code == SGCPlaces.Constants.PremiseType.Others
					 || code == SGCPlaces.Constants.PremiseType.SailingClub
					 || code == SGCPlaces.Constants.PremiseType.ShipYard
					 || code == SGCPlaces.Constants.PremiseType.BondedWarehouse
					 || code == SGCPlaces.Constants.PremiseType.BondedWarehouseClass2Yard
					 || code == SGCPlaces.Constants.PremiseType.LicensedWarehouse);
		}

		public static bool IsMajorExporterSchemeExemptionCode(this ZZRefCusCodeListCombined place)
		{
			var code = place?.ZZD_Code ?? string.Empty;
			return code == SGCPlaces.Constants.MajorExporterScheme;
		}

		public static bool IsApprovedImportGSTSuspensionSchemeExemptionCode(this ZZRefCusCodeListCombined place)
		{
			var code = place?.ZZD_Code ?? string.Empty;
			return code == SGCPlaces.Constants.ApprovedImportGSTSuspensionScheme;
		}

		public static bool SupplierExemptPlaceOfReceipt(this ZZRefCusCodeListCombined place)
		{
			return place.IsRecoveryPayment()
				|| place.IsShortPayment()
				|| place.IsApprovedImportGSTSuspensionSchemeExemptionCode()
				|| place.IsImportGSTDefermentSchemeExemptionCode()
				|| place.IsApprovedImportSuspensionSchemeLocal();
		}

		public static bool IsSupplierExemptPlaceOfRelease(this ZZRefCusCodeListCombined place)
		{
			var sgcType = place.GetSGCType();
			return sgcType == SGCPlaces.Constants.SupplierExemptLocation.Embassy
					|| sgcType == SGCPlaces.Constants.SupplierExemptLocation.ExemptionOnMotorVehicle;
		}

		public static bool IsImportGSTDefermentSchemeExemptionCode(this ZZRefCusCodeListCombined place)
		{
			var code = place?.ZZD_Code ?? string.Empty;
			return code == SGCPlaces.Constants.ImportGSTDefermentScheme;
		}

		public static bool IsFTZ(this ZZRefCusCodeListCombined place)
		{
			var sgcType = place.GetSGCType();
			return sgcType == SGCPlaces.Constants.FreeTradeZones.ChangiFTZ ||
					sgcType == SGCPlaces.Constants.FreeTradeZones.JurongFTZ ||
					sgcType == SGCPlaces.Constants.FreeTradeZones.KeppelFTZ ||
					sgcType == SGCPlaces.Constants.FreeTradeZones.PasirPanjangFTZ ||
					sgcType == SGCPlaces.Constants.FreeTradeZones.SembawangFTZ;
		}

		public static bool ShouldUseFTZ(this ZZRefCusCodeListCombined place)
		{
			var sgcType = place.GetSGCType();
			return sgcType == SGCPlaces.Constants.NotToUseForReceiptRelease.ContainerWharves ||
					sgcType == SGCPlaces.Constants.NotToUseForReceiptRelease.MarinaWharves ||
					sgcType == SGCPlaces.Constants.NotToUseForReceiptRelease.KeppelWharves ||
					sgcType == SGCPlaces.Constants.NotToUseForReceiptRelease.JurongWharves ||
					sgcType == SGCPlaces.Constants.NotToUseForReceiptRelease.PasirPanjangWharves ||
					sgcType == SGCPlaces.Constants.NotToUseForReceiptRelease.SembawangWharves ||
					sgcType == SGCPlaces.Constants.NotToUseForReceiptRelease.AirCargoTransitBond;
		}

		public static bool IsBWCY(this ZZRefCusCodeListCombined place)
		{
			return place.GetSGCType() == SGCPlaces.Constants.PremiseType.BondedWarehouseClass2Yard;
		}

		public static bool IsCFW(this ZZRefCusCodeListCombined place)
		{
			return place.GetSGCType() == SGCPlaces.Constants.PremiseType.ContainerFreightWarehouse;
		}

		public static bool IsC2Y(this ZZRefCusCodeListCombined place)
		{
			return place.GetSGCType() == SGCPlaces.Constants.PremiseType.Class2Yard;
		}

		public static bool IsShortPayment(this ZZRefCusCodeListCombined place)
		{
			var sgcType = place.GetSGCType();
			return sgcType == SGCPlaces.Constants.ShortPayment.ShortPaymentInvolvingUpdates ||
					sgcType == SGCPlaces.Constants.ShortPayment.ShortPaymentNotInvolvingUpdates ||
					sgcType == SGCPlaces.Constants.ShortPayment.ShortPaymentImportGSTDefermentScheme;
		}

		public static bool IsLicencedPremise(this ZZRefCusCodeListCombined place)
		{
			return place.GetSGCType() == SGCPlaces.Constants.PremiseType.LicensedWarehouse || place.IsBondedWarehouse();
		}

		public static bool IsBondedWarehouse(this ZZRefCusCodeListCombined place)
		{
			var sgcType = place.GetSGCType();
			return sgcType == SGCPlaces.Constants.PremiseType.BondedWarehouse || sgcType == SGCPlaces.Constants.PremiseType.BondedWarehouseClass2Yard;
		}

		public static bool IsRecoveryPayment(this ZZRefCusCodeListCombined place)
		{
			return place.GetSGCType() == SGCPlaces.Constants.RecoveryPayment.RecoveryPaymentNotInvolvingUpdates;
		}

		public static bool IsExemptPlaceCodePresident(this ZZRefCusCodeListCombined place)
		{
			var code = place?.ZZD_Code ?? string.Empty;
			return code == SGCPlaces.Constants.ExemptPlaceCodePresident;
		}

		public static bool IsNonSystemNonLicenced(this ZZRefCusCodeListCombined place)
		{
			var sgcType = place.GetSGCType();
			return sgcType == SGCPlaces.Constants.PremiseType.ShipYard
					|| sgcType == SGCPlaces.Constants.PremiseType.SailingClub
					|| sgcType == SGCPlaces.Constants.PremiseType.Others;
		}

		static bool IsApprovedImportSuspensionSchemeLocal(this ZZRefCusCodeListCombined place)
		{
			return place.GetSGCType() == SGCPlaces.Constants.SupplierExemptLocation.ApprovedImportSuspensionSchemeLocal;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Will be replaced soon")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
		public static bool IsLocationAddressToBePrinted(this ZZRefCusCodeListCombined place)
		{
			var code = place?.ZZD_Code ?? string.Empty;
			switch (code)
			{
				case "AISS":
				case "AISSLOC":
				case "ALPS":
				case "ANCH":
				case "AT1B":
				case "AT2B":
				case "AT3B":
				case "ATB":
				case "ATC":
				case "AZ":
				case "BEERSP":
				case "BJ":
				case "BTB":
				case "CABE":
				case "CABW":
				case "CAP":
				case "CFT":
				case "CNB":
				case "CPFT":
				case "CTM":
				case "CW":
				case "CZ":
				case "DGL":
				case "DR":
				case "DUMP":
				case "EM":
				case "ETHYL":
				case "ETHYLD":
				case "EXEMPT":
				case "EXEMPTD":
				case "EXTCZ":
				case "EXTJZ":
				case "EXTKZ":
				case "EXTPPZ":
				case "EXTSZ":
				case "GASPL":
				case "IGDS":
				case "FUEL":
				case "GOVT":
				case "H":
				case "HORSE":
				case "INTL":
				case "INTCZ":
				case "INTJZ":
				case "INTKZ":
				case "INTPPZ":
				case "INTSZ":
				case "JFP":
				case "JI":
				case "JW":
				case "JZ":
				case "KW":
				case "KZ":
				case "LHQ":
				case "LOB":
				case "MC":
				case "MD":
				case "ME":
				case "MPC":
				case "MSP":
				case "MW":
				case "NAAFI":
				case "P":
				case "PA":
				case "PBB":
				case "PBN":
				case "PPW":
				case "PPZ":
				case "PSB":
				case "PT":
				case "PTAP":
				case "PTCX":
				case "PTGX":
				case "PTMO":
				case "PTPSH":
				case "PTSH":
				case "PTTPS":
				case "PUB":
				case "RCNOSTK":
				case "RELIEF":
				case "RECYCL":
				case "SA":
				case "SBAB":
				case "SC":
				case "SCC":
				case "SEMCO":
				case "SFP":
				case "SPBB":
				case "SPG":
				case "SPIGDS":
				case "SPNOSTK":
				case "SPSTK":
				case "SW":
				case "SZ":
				case "TA":
				case "TBJ":
				case "TCS":
				case "TEN":
				case "THQ":
				case "TMFT":
				case "TNB":
				case "TRADESP":
				case "TOBSP":
				case "TOMFR":
				case "TV12":
				case "USAF":
				case "VAR":
				case "VEHBIC":
				case "VEHCUT":
				case "VEHGOF":
				case "VEHMIL":
				case "VEHRAC":
				case "VEHSG":
				case "VEHSGD":
				case "VEHTOY":
				case "VEHVIN":
				case "VEHORU":
				case "WCP":
				case "WTCP":
				case "WWN":
					return false;
				default:
					return true;
			}
		}
	}
}
