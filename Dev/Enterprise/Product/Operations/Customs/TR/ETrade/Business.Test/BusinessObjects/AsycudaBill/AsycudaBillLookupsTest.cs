using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Helper;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.TR.ETrade.Business.Testing
{
	public class AsycudaBillLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestProcedureCodeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var trImport = helper.CreateRefCusProcedure("TR", "A", "40", "10", "", "Import", "IMP");
			var trExport = helper.CreateRefCusProcedure("TR", "H", "81", "00", "", "Export", "EXP");

			Factory.Save();

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Turkey;
			header.AMA_Nature = ShipmentTypeList.Codes.Export22;
			var bill = header.Bills.AddNew();
			var lookup = bill.Lookups;
			AssertEquals(1, lookup.Procedures.Count);
			AssertEquals(true, lookup.Procedures.ContainsCode("8100"));

			header.AMA_Nature = ShipmentTypeList.Codes.Import23;
			AssertEquals(1, lookup.Procedures.Count);
			AssertEquals(true, lookup.Procedures.ContainsCode("4010"));
		}

		public void TestBondTypeList()
		{
			AssertEquals("BANAKIT, BANKA, DAC, DAC/R2, GAR, GDS, GLOBAL, GTR1, GTR2, GTRANT, NAKIT, RODER, UND", Bill.Lookups.BondTypeList.CodesAsString);

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Turkey;
			var bill = header.Bills.AddNew();
			AssertSame(bill.Lookups.BondTypeList, Bill.Lookups.BondTypeList);
		}

		public void TestPaymentMethodList()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Turkey;
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			var bill = header.Bills.AddNew();
			bill.ABL_ShipmentType = ShipmentTypeList.Codes.Import23;
			var list = bill.Lookups.PaymentMethodList;

			AssertEquals(true, list.ContainsCode("1"));
			AssertEquals(true, list.ContainsCode("2"));
			AssertEquals(true, list.ContainsCode("3"));
			AssertEquals(true, list.ContainsCode("4"));
			AssertEquals(false, list.ContainsCode("9"));
		}

		public void TestNatureOfBusinessList()
		{
			var list = Bill.Lookups.NatureOfBusinessList;

			AssertEquals(true, list.ContainsCode("11"));
			AssertEquals(true, list.ContainsCode("22"));
			AssertEquals(true, list.ContainsCode("33"));
			AssertEquals(true, list.ContainsCode("99"));
			AssertEquals(false, list.ContainsCode("XX"));
		}

		public void TestExemptionCodeList()
		{
			Bill.ExportCountry = Core.Constants.CountryCodes.Germany;
			Manifest.AMA_DateAtCustomsOffice = ZDateTime.Now;
			var list = Bill.Lookups.ExemptionCodeList;
			AssertEquals(true, list.ContainsCode("AFET"));
			AssertEquals(true, list.ContainsCode("ATOM"));
			AssertEquals(false, list.ContainsCode("ABCXY"));
			Bill.ExportCountry = Core.Constants.CountryCodes.SouthAfrica;
			list = Bill.Lookups.ExemptionCodeList;
			AssertEquals(true, list.ContainsCode("AFET"));
			AssertEquals(true, list.ContainsCode("TOHUM"));
			AssertEquals(false, list.ContainsCode("ABCXY"));
		}

		public void TestPackageTypeList()
		{
			var list = Bill.Lookups.PackageTypeList;
			AssertEquals(true, list.ContainsCode("PX"));
			AssertEquals(true, list.ContainsCode("BI"));
			AssertEquals(true, list.ContainsCode("BS"));
			AssertEquals(true, list.ContainsCode("DB"));
			AssertEquals(true, list.ContainsCode("DC"));
			AssertEquals(false, list.ContainsCode("xyz"));
		}

		public void TestETradeCargoStatusList()
		{
			var list = Bill.Lookups.CargoStatusList;
			AssertEquals(true, list.ContainsCode("APP"));
			AssertEquals(true, list.ContainsCode("NAP"));
		}

		public void TestBillStatusList()
		{
			var list = Bill.Lookups.CustomsStatusList;
			AssertEquals(true, list.ContainsCode("RED"));
			AssertEquals(true, list.ContainsCode("YEL"));
		}

		public void TestBillContainer()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill1 = header.Bills.AddNew();
			bill1.ContainerNumber = "XXX";
			Factory.Save();
			AssertEquals(bill1.Lookups.Containers.Count, 1);
			var bill2 = header.Bills.AddNew();
			AssertEquals(bill2.Lookups.Containers.Count, 1);
			bill2.ContainerNumber = "YYY";
			Factory.Save();
			AssertEquals(bill1.Lookups.Containers.Count, 2);
			bill2.ContainerNumber = "XXX";
			Factory.Save();
			AssertEquals(bill2.Lookups.Containers.Count, 1);
			bill1.ContainerNumber = ZString.Empty;
			Factory.Save();
			AssertEquals(bill2.Lookups.Containers.Count, 1);
		}

		public void TestSpecialCargoCodes()
		{
			var list = Bill.Lookups.SpecialCargoCodes;
			AssertEquals(true, list.ContainsCode("ET"));
			AssertEquals(true, list.ContainsCode("ETD"));
		}

		protected override void SetUp()
		{
			base.SetUp();

			var helper = new UniversalReferenceTestDataHelper(Factory);

			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NatureOfBusiness, "Nature Of Business");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NatureOfBusiness, "11", @"Eşyanın Mülkiyetinin Parasal veya Başka Bir Karşılıkla Devredildiği İşlemler - Doğrudan Satın Alma veya Satma", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NatureOfBusiness, "22", @"İade Edilen Eşyanın Değiştirilmesi", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NatureOfBusiness, "33", @"Eşyanın Mülkiyetinin Parasal veya Başka Bir Karşılık Alınmaksızın Devredildiği İşlemler-Diğer Yardım Programları (Özel hukuk tüzel kişileri, gerçek kişiler ve sivil toplum kuruluşları)", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NatureOfBusiness, "99", @"Diğer Ticari İşlemler", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));

			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExemptionCode, "Exemption Code");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExemptionCode, "AFET", @"27.3.1992-92/2879 BKK Afetler için tanınan muafiyet", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExemptionCode, "OHAL", @"285 Sayılı OHAL ilişkin KHK.", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExemptionCode, "ORMAN", @"2008/13262sayı.İthalat Rejimi Kararı", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));

			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Turkey, "ETR");
			Factory.Save();
			var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Turkey, tariffType.PK, "AFET", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1), @"27.3.1992-92/2879 BKK Afetler için tanınan muafiyet");
			var tariff2 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Turkey, tariffType.PK, "ATOM", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1), @"285 Sayılı OHAL ilişkin KHK.");
			var tariff3 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Turkey, tariffType.PK, "TOHUM", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1), @"2008/13262sayı.İthalat Rejimi Kararı");

			var tradegroupAll = helper.CreateTradeGroup(Core.Constants.CountryCodes.Turkey, "All Countries", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			var tradegroupEu = helper.CreateTradeGroup(Core.Constants.CountryCodes.Turkey, "EU", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			var tradegroupNonEu = helper.CreateTradeGroup(Core.Constants.CountryCodes.Turkey, "Non-EU", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			Factory.Save();

			var tradeGroupCountryDEAll = helper.AddCountry(tradegroupAll, Core.Constants.CountryCodes.Germany);
			var tradeGroupCountryDEEu = helper.AddCountry(tradegroupEu, Core.Constants.CountryCodes.Germany);
			var tradeGroupCountryZAAll = helper.AddCountry(tradegroupNonEu, Core.Constants.CountryCodes.SouthAfrica);
			var tradeGroupCountryZANeu = helper.AddCountry(tradegroupAll, Core.Constants.CountryCodes.SouthAfrica);
			Factory.Save();

			var rateType = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Turkey, "ETR");
			Factory.Save();
			var ratecode1 = helper.LoadOrCreateNewCusRateCode(Factory, "10", rateType.PK);
			Factory.Save();
			var ratedeall = helper.CreateRate(tariff1, ratecode1.PK, ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			var appAll = helper.CreateCusApplicability(ratedeall, tradegroupAll, ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));

			var ratezaall = helper.CreateRate(tariff1, ratecode1.PK, ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			helper.CreateCusApplicability(ratezaall, tradegroupAll, ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));

			var ratedeeu = helper.CreateRate(tariff2, ratecode1.PK, ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			helper.CreateCusApplicability(ratedeeu, tradegroupEu, ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));

			var ratezanoneu = helper.CreateRate(tariff3, ratecode1.PK, ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			helper.CreateCusApplicability(ratezanoneu, tradegroupNonEu, ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));

			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes, "PackageTypes");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes, "PX", @"PALLET", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes, "BI", @"BIN", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes, "BS", @"BOTTLE NON PROTECTED BULBOUS", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes, "DB", @"CRATE MULTIPLE PAYER WOODEN", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes, "DC", @"CRATE MULTIPLE LAYER CARDBOARD", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			Factory.Save();

			Manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			Manifest.AMA_RN_NKCountry = Core.Constants.CountryCodes.Turkey;
			Manifest.AMA_TransportMode = Core.Constants.TransportModes.Air;
			Manifest.AMA_ManifestType = ShipmentTypeList.Codes.Import23;
			Bill = Manifest.Bills.AddNew();
			Bill.ABL_ShipmentType = ShipmentTypeList.Codes.Import23;
			Factory.Save();
		}

		public AsycudaManifestHeader Manifest { get; set; }
		public AsycudaBill Bill { get; set; }
	}
}
