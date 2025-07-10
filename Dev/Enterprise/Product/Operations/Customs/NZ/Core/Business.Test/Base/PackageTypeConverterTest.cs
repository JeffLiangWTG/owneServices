using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using static Enterprise.Core.Constants;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Customs.NZ.Business.Testing
{
	using CargoWise.EntityFramework.Testing;
	public class PackageTypeConverterTest : TestCaseWithFactory
	{
		public void TestGetCustomsPackageType()
		{
			var list = new PackageTypeList();
			var fields = typeof(PkgUnit).GetFields();
			var exceptList = new ZString[]
			{
				PkgUnit.BreakBulk,
				PkgUnit.Cradle,
				PkgUnit.Dozen,
				PkgUnit.Gross,
				PkgUnit.Mix,
				PkgUnit.RollOnRollOff,
			};

			foreach (var field in fields)
			{
				var unitType = (string)field.GetValue(null);
				if (exceptList.Contains(unitType))
				{
					Assert(string.Format("Can't map for Unit {0}", unitType), !list.ContainsCode(PackageTypeConverter.GetCustomsPackageType(unitType)));
				}
				else
				{
					Assert(string.Format("Unit {0} should convert as expected", unitType), list.ContainsCode(PackageTypeConverter.GetCustomsPackageType(unitType)));
				}
			}

			AssertEquals("Unknown code should return same code", "Unknown Code", PackageTypeConverter.GetCustomsPackageType("Unknown Code"));
			AssertEquals("Null string should not cause exception", "", PackageTypeConverter.GetCustomsPackageType(""));
		}

		public void TestGetFreightPackageType()
		{
			AssertEquals("Units should convert as expected", Constants.PkgUnit.Bag, PackageTypeConverter.GetFreightPackageType(PackageTypeList.Codes.Bag));
			AssertEquals("Units should convert as expected", Constants.PkgUnit.Keg, PackageTypeConverter.GetFreightPackageType(PackageTypeList.Codes.Keg));
			AssertEquals("Unknown code should return same code", "Unknown Code", PackageTypeConverter.GetFreightPackageType("Unknown Code"));
			AssertEquals("Null string should not cause exception", "", PackageTypeConverter.GetFreightPackageType(""));
		}

		public void TestGetCustomsTSWPackagingType()
		{
			var refPack1 = Factory.New<CusRefPacks>();
			refPack1.RP_CommercialPack = "CKG";
			refPack1.RP_CustomsPack = "CK";
			refPack1.RP_ConversionFactor = 1.0;
			refPack1.RP_CustomsCountry = "NZ";
			refPack1.RP_Type = RPTypeList.Codes.CommercialInvoice;
			refPack1.RP_IsSystem = true;

			var refPack2 = Factory.New<CusRefPacks>();
			refPack2.RP_CommercialPack = "PKG";
			refPack2.RP_CustomsPack = "PK";
			refPack2.RP_ConversionFactor = 1.0;
			refPack2.RP_CustomsCountry = "NZ";
			refPack2.RP_IsSystem = true;
			refPack2.RP_Type = RPTypeList.Codes.DeclarationTotal;
			Factory.Save();

			AssertEquals("CKG -> CK", "CK", PackageTypeConverter.GetCustomsTSWPackagingType(Factory, "CKG"));
			AssertEquals("PKG -> PK", "PK", PackageTypeConverter.GetCustomsTSWPackagingType(Factory, "PKG"));
			AssertEquals("Unknown code should return empty string as field max size would be exceeded if returning the code entered", ZString.Empty, PackageTypeConverter.GetCustomsTSWPackagingType(Factory, "Unknown Code"));
			AssertEquals("Null string should not cause exception", "", PackageTypeConverter.GetCustomsTSWPackagingType(Factory, ""));
		}

		public void TestGetCustomsTSWPackagingType_OriginalTestCase()
		{
			AssertEquals("Unknown code should return empty string as field max size would be exceeded if returning the code entered", ZString.Empty, PackageTypeConverter.GetCustomsTSWPackagingType(Factory, "Unknown Code"));
			AssertEquals("Null string should not cause exception", "", PackageTypeConverter.GetCustomsTSWPackagingType(Factory, ""));
			AssertEquals("NMB should convert to packages", "PK", PackageTypeConverter.GetCustomsTSWPackagingType(Factory, "NMB"));

			AssertEquals("BAG -> BG", "BG", PackageTypeConverter.GetCustomsTSWPackagingType(Factory, "BAG"));
			AssertEquals("BBG -> 43", "43", PackageTypeConverter.GetCustomsTSWPackagingType(Factory, "BBG"));
			AssertEquals("BBK -> NE", "NE", PackageTypeConverter.GetCustomsTSWPackagingType(Factory, "BBK"));
			AssertEquals("BLC -> BL", "BL", PackageTypeConverter.GetCustomsTSWPackagingType(Factory, "BLC"));
			AssertEquals("BND -> BE", "BE", PackageTypeConverter.GetCustomsTSWPackagingType(Factory, "BND"));
			AssertEquals("BOT -> BO", "BO", PackageTypeConverter.GetCustomsTSWPackagingType(Factory, "BOT"));
			AssertEquals("BOX -> BX", "BX", PackageTypeConverter.GetCustomsTSWPackagingType(Factory, "BOX"));
			AssertEquals("BSK -> BK", "BK", PackageTypeConverter.GetCustomsTSWPackagingType(Factory, "BSK"));
			AssertEquals("CAS -> CS", "CS", PackageTypeConverter.GetCustomsTSWPackagingType(Factory, "CAS"));
			AssertEquals("CNT -> CN", "CN", PackageTypeConverter.GetCustomsTSWPackagingType(Factory, "CNT"));
			AssertEquals("COI -> CL", "CL", PackageTypeConverter.GetCustomsTSWPackagingType(Factory, "COI"));
			AssertEquals("CS -> CS", "CS", PackageTypeConverter.GetCustomsTSWPackagingType(Factory, "CS"));
			AssertEquals("CTN -> CT", "CT", PackageTypeConverter.GetCustomsTSWPackagingType(Factory, "CTN"));
			AssertEquals("DRM -> DR", "DR", PackageTypeConverter.GetCustomsTSWPackagingType(Factory, "DRM"));
			AssertEquals("EA -> PK", "PK", PackageTypeConverter.GetCustomsTSWPackagingType(Factory, "EA"));
			AssertEquals("ENV -> EN", "EN", PackageTypeConverter.GetCustomsTSWPackagingType(Factory, "ENV"));
			AssertEquals("GOH -> RJ", "RJ", PackageTypeConverter.GetCustomsTSWPackagingType(Factory, "GOH"));
			AssertEquals("KEG -> KG", "KG", PackageTypeConverter.GetCustomsTSWPackagingType(Factory, "KEG"));
			AssertEquals("MIX -> NG", "NG", PackageTypeConverter.GetCustomsTSWPackagingType(Factory, "MIX"));
			AssertEquals("NMB -> PK", "PK", PackageTypeConverter.GetCustomsTSWPackagingType(Factory, "NMB"));
			AssertEquals("NO -> PK", "PK", PackageTypeConverter.GetCustomsTSWPackagingType(Factory, "NO"));
			AssertEquals("PAC -> PA", "PA", PackageTypeConverter.GetCustomsTSWPackagingType(Factory, "PAC"));
			AssertEquals("PAI -> PL", "PL", PackageTypeConverter.GetCustomsTSWPackagingType(Factory, "PAI"));
			AssertEquals("PCE -> PK", "PK", PackageTypeConverter.GetCustomsTSWPackagingType(Factory, "PCE"));
			AssertEquals("PCS -> PK", "PK", PackageTypeConverter.GetCustomsTSWPackagingType(Factory, "PCS"));
			AssertEquals("PLT -> PX", "PX", PackageTypeConverter.GetCustomsTSWPackagingType(Factory, "PLT"));
			AssertEquals("REL -> RL", "RL", PackageTypeConverter.GetCustomsTSWPackagingType(Factory, "REL"));
			AssertEquals("RLL -> RO", "RO", PackageTypeConverter.GetCustomsTSWPackagingType(Factory, "RLL"));
			AssertEquals("SHT -> ST", "ST", PackageTypeConverter.GetCustomsTSWPackagingType(Factory, "SHT"));
			AssertEquals("SKD -> SI", "SI", PackageTypeConverter.GetCustomsTSWPackagingType(Factory, "SKD"));
			AssertEquals("SPL -> SO", "SO", PackageTypeConverter.GetCustomsTSWPackagingType(Factory, "SPL"));
			AssertEquals("TE -> TC", "TC", PackageTypeConverter.GetCustomsTSWPackagingType(Factory, "TE"));
			AssertEquals("TUB -> TU", "TU", PackageTypeConverter.GetCustomsTSWPackagingType(Factory, "TUB"));
			AssertEquals("UNT -> UN", "UN", PackageTypeConverter.GetCustomsTSWPackagingType(Factory, "UNT"));
		}
	}
}
