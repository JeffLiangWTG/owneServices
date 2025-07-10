using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.SG.V4.Business.SGCPlaces.Constants;
using static Enterprise.Customs.SG.V4.Business.UniversalReferenceConstants.RefCusCodeList.Attributes;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public static class SGPlacesRefCusCodeListTestDataHelper
	{
		public static void CreateSGPlaces(BusinessObjectFactory factory)
		{
			CreateLCFACSGPlace(factory, PremiseType.BondedWarehouse, "BONDED WAREHOUSE");
			CreateLCFACSGPlace(factory, PremiseType.BondedWarehouseClass2Yard, "BONDED WAREHOUSE CLASS 2 YARD");
			CreateLCFACSGPlace(factory, PremiseType.Class2Yard, "CLASS 2 YARD");
			CreateLCFACSGPlace(factory, PremiseType.ContainerFreightWarehouse, "CONTAINER FREIGHT WAREHOUSE");
			CreateLCFACSGPlace(factory, PremiseType.LicensedWarehouse, "LICENSED WAREHOUSE");
			CreateLCFACSGPlace(factory, PremiseType.Others, "OTHERS");
			CreateLCFACSGPlace(factory, PremiseType.SailingClub, "SAILING CLUB");
			CreateLCFACSGPlace(factory, PremiseType.ShipYard, "SHIP YARD");

			CreateFACSGPlace(factory, FreeTradeZones.ChangiFTZ, FreeTradeZones.ChangiFTZ, "CHANGI FTZ");
			CreateFACSGPlace(factory, FreeTradeZones.JurongFTZ, FreeTradeZones.JurongFTZ, "JURONG FTZ");
			CreateFACSGPlace(factory, FreeTradeZones.KeppelFTZ, FreeTradeZones.KeppelFTZ, "KEPPEL FTZ");
			CreateFACSGPlace(factory, FreeTradeZones.PasirPanjangFTZ, FreeTradeZones.PasirPanjangFTZ, "PASIR PANJANG FTZ");
			CreateFACSGPlace(factory, FreeTradeZones.SembawangFTZ, FreeTradeZones.SembawangFTZ, "SEMBAWANG FTZ");

			CreateFACSGPlace(factory, NotToUseForReceiptRelease.ContainerWharves, NotToUseForReceiptRelease.ContainerWharves, "CONTAINER WHARVES");
			CreateFACSGPlace(factory, NotToUseForReceiptRelease.MarinaWharves, NotToUseForReceiptRelease.MarinaWharves, "MARINA WHARVES");
			CreateFACSGPlace(factory, NotToUseForReceiptRelease.KeppelWharves, NotToUseForReceiptRelease.KeppelWharves, "KEPPEL WHARVES");
			CreateFACSGPlace(factory, NotToUseForReceiptRelease.JurongWharves, NotToUseForReceiptRelease.JurongWharves, "JURONG WHARVES");
			CreateFACSGPlace(factory, NotToUseForReceiptRelease.PasirPanjangWharves, NotToUseForReceiptRelease.PasirPanjangWharves, "PASIR PANJANG WHARVES");
			CreateFACSGPlace(factory, NotToUseForReceiptRelease.SembawangWharves, NotToUseForReceiptRelease.SembawangWharves, "SEMBAWANG WHARVES");

			CreateFACSGPlace(factory, PremiseType.Others, PremiseType.Others, "OTHERS");
			CreateFACSGPlace(factory, PremiseType.SailingClub, PremiseType.SailingClub, "SAILING CLUB");
			CreateFACSGPlace(factory, PremiseType.ShipYard, PremiseType.ShipYard, "SHIPYARD");

			CreateFACSGPlace(factory, ShortPayment.ShortPaymentInvolvingUpdates, ShortPayment.ShortPaymentInvolvingUpdates, "SHORT PAYMENT INVOLVING UPDATES TO STOCK");
			CreateFACSGPlace(factory, ShortPayment.ShortPaymentNotInvolvingUpdates, ShortPayment.ShortPaymentNotInvolvingUpdates, "SHORT PAYMENT NOT INVOLVING UPDATES TO STOCK");
			CreateFACSGPlace(factory, ShortPayment.ShortPaymentImportGSTDefermentScheme, ShortPayment.ShortPaymentImportGSTDefermentScheme, "SUPPLEMENTARY PERMIT FOR IMPORT GST DEFERMENT SCHEME");

			CreateFACSGPlace(factory, RecoveryPayment.RecoveryPaymentNotInvolvingUpdates, RecoveryPayment.RecoveryPaymentNotInvolvingUpdates, "REVENUE RECOVERING NOT INVOLVING UPDATES  TO INVENTORY RECORDS");

			CreateFACSGPlace(factory, ApprovedImportGSTSuspensionScheme, ApprovedImportGSTSuspensionScheme, "APPROVED IMPORT GST SUSPENSION SCHEME (IRAS)");

			CreateFACSGPlace(factory, ImportGSTDefermentScheme, ImportGSTDefermentScheme, "IMPORT GST DEFERMENT SCHEME");

			CreateFACSGPlace(factory, SupplierExemptLocation.ApprovedImportSuspensionSchemeLocal, SupplierExemptLocation.ApprovedImportSuspensionSchemeLocal, "APPROVED IMPORT SUSPENSION SCHEME - LOCAL (IRAS)");

			CreateFACSGPlace(factory, SupplierExemptLocation.Embassy, SupplierExemptLocation.Embassy, "EMBASSY/HIGH COMMISSION");
			CreateFACSGPlace(factory, SupplierExemptLocation.ExemptionOnMotorVehicle, SupplierExemptLocation.ExemptionOnMotorVehicle, "EXEMPTION ON MOTOR VEHICLES");

			CreateFACSGPlace(factory, MajorExporterScheme, MajorExporterScheme, "MAJOR EXPORTER SCHEME");

			CreateFACSGPlace(factory, "AT1B", "AT1B", "AIRPORT TERMINAL 1 BOND");

			CreateFACSGPlace(factory, ExemptPlaceCodePresident, ExemptPlaceCodePresident, "PRESIDENT");
		}

		public static RefCusCodeList CreateFACSGPlace(BusinessObjectFactory factory, string code, string sgcTypeValue, string nameAddress, string[] additionalTypeNames = null, ZDateTime? effectiveDate = null, ZDateTime? expiryDate = null)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			var codeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities;
			var cusCodeType = factory.LoadTop1<RefCusCodeType>(new ZQuery(RefCusCodeTypeSchema.ZZK_CodeType, codeType));
			if (cusCodeType == null)
			{
				helper.CreateCusCodeType(codeType, "Facilities");
			}
			var sgPlace = helper.CreateCusCodeList(Core.Constants.CountryCodes.Singapore, codeType, code, nameAddress, effectiveDate ?? ZDateTime.MinSmallDateTimeValue, expiryDate ?? ZDateTime.MaxSmallDateTimeValue);

			if (!string.IsNullOrEmpty(sgcTypeValue))
			{
				helper.CreateCusCodeListAttribute(sgPlace.PK, SGCType, sgcTypeValue);
			}

			if (additionalTypeNames != null && additionalTypeNames.Any())
			{
				foreach (var type in additionalTypeNames)
				{
					helper.CreateCusCodeListAttribute(sgPlace.PK, type, "");
				}
			}

			return sgPlace;
		}

		public static RefCusCodeList CreateLCFACSGPlace(BusinessObjectFactory factory, string code, string nameAddress, string[] additionalTypeNames = null, ZDateTime? effectiveDate = null, ZDateTime? expiryDate = null)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			var codeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.LicensedFacilities;
			var cusCodeType = factory.LoadTop1<RefCusCodeType>(new ZQuery(RefCusCodeTypeSchema.ZZK_CodeType, codeType));
			if (cusCodeType == null)
			{
				helper.CreateCusCodeType(codeType, "Licensed Facilities");
			}
			var sgPlace = helper.CreateCusCodeList(Core.Constants.CountryCodes.Singapore, codeType, code, nameAddress, effectiveDate ?? ZDateTime.MinSmallDateTimeValue, expiryDate ?? ZDateTime.MaxSmallDateTimeValue);

			if (additionalTypeNames != null && additionalTypeNames.Any())
			{
				foreach (var type in additionalTypeNames)
				{
					helper.CreateCusCodeListAttribute(sgPlace.PK, type, "");
				}
			}

			return sgPlace;
		}
	}
}
