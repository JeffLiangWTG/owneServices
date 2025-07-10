using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using static Enterprise.Customs.ZA.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.ZA.Business.Testing
{
	public class ZAUniversalReferenceTestDataHelper : UniversalReferenceTestDataHelper
	{
		public ZAUniversalReferenceTestDataHelper(BusinessObjectFactory factory, bool setupBasicTariffData = true) : base(factory)
		{
			if (setupBasicTariffData)
			{
				SetupBasicTariffTypesForZATesting(factory, this);
			}
		}

		public static void SetupRefCusMapAndRefCusMapType(BusinessObjectFactory factory, ZAUniversalReferenceTestDataHelper helper = null)
		{
			if (helper == null)
			{
				helper = new ZAUniversalReferenceTestDataHelper(factory, false);
			}

			var zaCountry = Core.Constants.CountryCodes.SouthAfrica;
			var cusMapType1 = helper.CreateCusMapType("REL", "BTH", "Related Party Indicator", true);
			var cusMapType2 = helper.CreateCusMapType("ZADOC", "OUT", "ZA Supporting Document Types", false);
			var cusMapType3 = helper.CreateCusMapType("PREF", "OUT", "ZA Preference Codes", true);
			var cusMapType4 = helper.CreateCusMapType("BOL", "OUT", "House Bill Types", true);
			var cusMap1 = helper.CreateCusMap("REL", "Y", "R", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 06, 06), zaCountry);
			var cusMap2 = helper.CreateCusMap("REL", "N", "N", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 06, 06), zaCountry);
			var cusMap3 = helper.CreateCusMap("REL", "E", "E", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 06, 06), zaCountry);
			var cusMap4 = helper.CreateCusMap("ZADOC", "DGF", "DGS", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 06, 06), zaCountry);
			var cusMap5 = helper.CreateCusMap("ZADOC", "CIV", "INV", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 06, 06), zaCountry);
			var cusMap6 = helper.CreateCusMap("ZADOC", "MCD", "OTH", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 06, 06), zaCountry);
			var cusMap7 = helper.CreateCusMap("ZADOC", "MSC", "OTH", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 06, 06), zaCountry);
			var cusMap8 = helper.CreateCusMap("ZADOC", "MFD", "OTH", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 06, 06), zaCountry);
			var cusMap9 = helper.CreateCusMap("ZADOC", "EXV", "VEC", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 06, 06), zaCountry);
			var cusMap10 = helper.CreateCusMap("ZADOC", "WMR", "WBC", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 06, 06), zaCountry);
			var cusMap11 = helper.CreateCusMap("PREF", "STANDARD", "100", new ZDateTime(2016, 9, 15), new ZDateTime(2079, 06, 06), zaCountry);
			var cusMap12 = helper.CreateCusMap("PREF", "SADC", "200", new ZDateTime(2016, 9, 15), new ZDateTime(2079, 06, 06), zaCountry);
			var cusMap13 = helper.CreateCusMap("PREF", "EFTA", "200", new ZDateTime(2016, 9, 15), new ZDateTime(2079, 06, 06), zaCountry);
			var cusMap14 = helper.CreateCusMap("PREF", "MERCOSUR", "200", new ZDateTime(2016, 9, 15), new ZDateTime(2079, 06, 06), zaCountry);
			var cusMap15 = helper.CreateCusMap("PREF", "EUTRADE", "200", new ZDateTime(2016, 9, 15), new ZDateTime(2079, 06, 06), zaCountry);
			var cusMap16 = helper.CreateCusMap("PREF", "EUQUOTA", "400", new ZDateTime(2016, 9, 15), new ZDateTime(2079, 06, 06), zaCountry);
			var cusMap17 = helper.CreateCusMap("PREF", "EFTAQUOTA", "400", new ZDateTime(2016, 9, 15), new ZDateTime(2079, 06, 06), zaCountry);
			var cusMap18 = helper.CreateCusMap("BOL", "STD", "BOL", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 06, 06), zaCountry);
			var cusMap19 = helper.CreateCusMap("BOL", "CLD", "PBL", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 06, 06), zaCountry);
			factory.Save();
		}

		public static void SetupBasicTariffTypesForZATesting(BusinessObjectFactory factory, ZAUniversalReferenceTestDataHelper helper = null)
		{
			if (helper == null)
			{
				helper = new ZAUniversalReferenceTestDataHelper(factory, false);
			}

			var zaCountry = Core.Constants.CountryCodes.SouthAfrica;
			var addRateType = Constants.RateTypes.AntiDumping;
			var levyRateType = Constants.RateTypes.Levy;
			var rebateRateType = Constants.RateTypes.Rebate;
			var refundRateType = Constants.RateTypes.Refund;
			var rateType1 = helper.CreateNewOrGetExistingRateType(zaCountry, addRateType, "Anti-Dumping");
			rateType1.ZZR_IsPayable = true;
			rateType1.ZZR_CustomsValueFormula = "CV";
			var rateType2 = helper.CreateNewOrGetExistingRateType(zaCountry, Constants.RateTypes.Duty, "Duty");
			rateType2.ZZR_IsPayable = true;
			rateType2.ZZR_CustomsValueFormula = "CV";
			var rateType3 = helper.CreateNewOrGetExistingRateType(zaCountry, Constants.RateTypes.AdValoremExcise, "Ad-Valorem Excise");
			rateType3.ZZR_IsPayable = true;
			rateType3.ZZR_CustomsValueFormula = "(CV * 1.15) + 1P1 + 2P1 + 2P2 + 2P3 - 3P1 - 3P2 - 4P1 - 4P2 - 4P3 - 4P4 - 4P5 - 4P6";
			helper.CreateNewOrGetExistingRateType(zaCountry, Constants.RateTypes.Excise, "Excise").ZZR_IsPayable = true;
			helper.CreateNewOrGetExistingRateType(zaCountry, levyRateType, "Levy").ZZR_IsPayable = true;
			helper.CreateNewOrGetExistingRateType(zaCountry, rebateRateType, "Rebate").ZZR_IsPayable = false;
			helper.CreateNewOrGetExistingRateType(zaCountry, refundRateType, "Refund").ZZR_IsPayable = false;
			//Schedule 1
			RefCusTariffType type1;
			CreateRateViewWithTariffTypeAndRateType(helper, factory, zaCountry, "1P1", Constants.RateTypes.Duty, "1P1", "9999901", out type1);
			RefCusTariffType type2;
			CreateRateViewWithTariffTypeAndRateType(helper, factory, zaCountry, "12A", Constants.RateTypes.Excise, "12A", "9999902", out type2, tariffTypeDesc: "Schedule 1 Part 2 A");
			RefCusTariffType type3;
			CreateRateViewWithTariffTypeAndRateType(helper, factory, zaCountry, "12B", Constants.RateTypes.AdValoremExcise, "12B", "9999903", out type3);
			RefCusTariffType type4;
			CreateRateViewWithTariffTypeAndRateType(helper, factory, zaCountry, "13A", levyRateType, "13A", "9999904", out type4);
			RefCusTariffType type5;
			CreateRateViewWithTariffTypeAndRateType(helper, factory, zaCountry, "13B", levyRateType, "13B", "9999905", out type5);
			RefCusTariffType type6;
			CreateRateViewWithTariffTypeAndRateType(helper, factory, zaCountry, "13C", levyRateType, "13C", "9999906", out type6);
			RefCusTariffType type7;
			CreateRateViewWithTariffTypeAndRateType(helper, factory, zaCountry, "13D", levyRateType, "13D", "9999907", out type7);
			RefCusTariffType type8;
			CreateRateViewWithTariffTypeAndRateType(helper, factory, zaCountry, "15A", levyRateType, "15A", "9999908", out type8);
			RefCusTariffType type9;
			CreateRateViewWithTariffTypeAndRateType(helper, factory, zaCountry, "15B", levyRateType, "15B", "9999909", out type9);
			//Schedule 2
			RefCusTariffType type0;
			CreateRateViewWithTariffTypeAndRateType(helper, factory, zaCountry, "2P1", addRateType, "2P1", "9999900", out type0);
			RefCusTariffType typeA;
			CreateRateViewWithTariffTypeAndRateType(helper, factory, zaCountry, "2P2", addRateType, "2P2", "999990A", out typeA);
			RefCusTariffType typeB;
			CreateRateViewWithTariffTypeAndRateType(helper, factory, zaCountry, "2P3", addRateType, "2P3", "999990B", out typeB);
			//Schedule 3
			RefCusTariffType typeC;
			CreateRateViewWithTariffTypeAndRateType(helper, factory, zaCountry, "3P1", rebateRateType, "3P1", "999990C", out typeC, null);
			RefCusTariffType typeD;
			CreateRateViewWithTariffTypeAndRateType(helper, factory, zaCountry, "3P2", rebateRateType, "3P2", "999990D", out typeD, null);
			//Schedule 4
			RefCusTariffType typeE;
			RefCusTariffType typeF;
			RefCusTariffType typeG;
			RefCusTariffType typeH;
			RefCusTariffType typeI;
			RefCusTariffType typeJ;
			CreateRateViewWithTariffTypeAndRateType(helper, factory, zaCountry, "4P1", rebateRateType, "4P1", "999990E", out typeE, null);
			CreateRateViewWithTariffTypeAndRateType(helper, factory, zaCountry, "4P2", rebateRateType, "4P2", "999990F", out typeF, null);
			CreateRateViewWithTariffTypeAndRateType(helper, factory, zaCountry, "4P3", rebateRateType, "4P3", "999990G", out typeG, null);
			CreateRateViewWithTariffTypeAndRateType(helper, factory, zaCountry, "4P4", rebateRateType, "4P4", "999990H", out typeH, null);
			CreateRateViewWithTariffTypeAndRateType(helper, factory, zaCountry, "4P5", rebateRateType, "4P5", "999990I", out typeI, null);
			CreateRateViewWithTariffTypeAndRateType(helper, factory, zaCountry, "4P6", rebateRateType, "4P6", "999990J", out typeJ, null);
			//Schedule 5
			RefCusTariffType typeK;
			RefCusTariffType typeL;
			RefCusTariffType typeM;
			RefCusTariffType typeN;
			RefCusTariffType typeO;
			CreateRateViewWithTariffTypeAndRateType(helper, factory, zaCountry, "5P1", refundRateType, "5P1", "999990K", out typeK, null);
			CreateRateViewWithTariffTypeAndRateType(helper, factory, zaCountry, "5P2", refundRateType, "5P2", "999990L", out typeL, null);
			CreateRateViewWithTariffTypeAndRateType(helper, factory, zaCountry, "5P3", refundRateType, "5P3", "999990M", out typeM, null);
			CreateRateViewWithTariffTypeAndRateType(helper, factory, zaCountry, "5P4", refundRateType, "5P4", "999990N", out typeN, null);
			CreateRateViewWithTariffTypeAndRateType(helper, factory, zaCountry, "5P5", refundRateType, "5P5", "999990O", out typeO, null);
			//Schedule 6
			RefCusTariffType typeP;
			RefCusTariffType typeQ;
			RefCusTariffType typeR;
			RefCusTariffType typeS;
			CreateRateViewWithTariffTypeAndRateType(helper, factory, zaCountry, "6P1", refundRateType, "6P1", "999990P", out typeP, null);
			CreateRateViewWithTariffTypeAndRateType(helper, factory, zaCountry, "6P2", refundRateType, "6P2", "999990Q", out typeQ, null);
			CreateRateViewWithTariffTypeAndRateType(helper, factory, zaCountry, "6P3", refundRateType, "6P3", "999990R", out typeR, null);
			CreateRateViewWithTariffTypeAndRateType(helper, factory, zaCountry, "6P4", refundRateType, "6P4", "999990S", out typeS, null);
			factory.Save();
		}

		static TariffView CreateRateViewWithTariffTypeAndRateType(ZAUniversalReferenceTestDataHelper helper, BusinessObjectFactory factory, ZString zaCountry, ZString tariffType, ZString rateType, ZString rateCode, ZString tariffCode, out RefCusTariffType cusTariffType, string tariffTypeDesc = null, string taxOrFeeCode = null, string relatedTariffCode = null)
		{
			var startDate = ZDateTime.Today.AddDays(-1);
			var endDate = ZDateTime.Today.AddDays(1);
			cusTariffType = helper.CreateNewOrGetExistingTariffType(zaCountry, tariffType);
			cusTariffType.ZZI_Description = tariffTypeDesc ?? (tariffType + "DESC");
			var rateType1 = helper.CreateNewOrGetExistingRateType(zaCountry, rateType);
			var rateCode1 = helper.LoadOrCreateNewCusRateCode(factory, rateCode, rateType1.PK);
			factory.Save();
			var tariffView1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.SouthAfrica, cusTariffType.PK, tariffCode, startDate, endDate, taxOrFeeCode: taxOrFeeCode, relatedTariffCode: relatedTariffCode, isSystem: true, ensureDataGroupingExists: true);
			var rateView = helper.CreateRate(tariffView1, rateCode1.PK, startDate, endDate);
			factory.Save();
			return tariffView1;
		}

		/// <summary>
		/// Create a Merged Declaration with all Schedule Types; [TestDate(1990, 6, 1)] is required
		/// </summary>
		/// <returns>JobDeclaration</returns>
		public JobDeclaration SetupDeclarationWithAllSchedules()
		{
			var preference = CreatePreferenceForCountryAndGrouping("200", "Cus Preference", Core.Constants.CountryCodes.SouthAfrica, "ZA");
			factory.Save();
			var procedure = CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "1#", "2$", "4", "BOB's PROCEDURE", ZAJobMessageTypeList.Codes.Import, "");
			CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "11", ZString.Empty, ZString.Empty, "BOB's PROCEDURE", ZAJobMessageTypeList.Codes.Import, "");
			CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "11", "20", ZString.Empty, "BOB's PROCEDURE", ZAJobMessageTypeList.Codes.Import, "");
			var startDate = ZDate.Today.AddDays(-1);
			var endDate = ZDate.Today.AddDays(1);
			var zaCountryCode = Core.Constants.CountryCodes.SouthAfrica;
			var itCountryCode = Core.Constants.CountryCodes.Italy;
			var testTradeGroup1 = LoadOrCreateTradeGroup(zaCountryCode, "EUTRADE", startDate, endDate);
			AddCountry(testTradeGroup1, zaCountryCode, startDate, endDate);
			AddCountry(testTradeGroup1, itCountryCode, startDate, endDate);
			CreateTaxOrFee("VAT", 0.14, zaCountryCode, startDate, endDate, "VAT Normal");
			factory.Save();
			var formula2 = "(VFD+1P1)*0.1";
			var formula3 = "VFD*0.1";
			var formula4 = "(VFD+1P1+12A+12B)*0.1";
			var formula5 = "(VFD+1P1+12A+12B+13A)*0.1";
			var formula6 = "(VFD+1P1+12A+12B+13A+13B)*0.1";
			var formula7 = "(VFD+1P1+12A+12B+13A+13B+13C)*0.1";
			var formula8 = "(VFD+1P1+12A+12B+13A+13B+13C+13D)*0.1";
			var formula9 = "(VFD+1P1+12A+12B+13A+13B+13C+13D+15A)*0.1";
			var formula10 = "(VFD+1P1+12A+12B+13A+13B+13C+13D+15A+15B)*0.1";
			var formula11 = "(VFD+1P1+12A+12B+13A+13B+13C+13D+15A+15B+2P1)*0.1";
			var formula12 = "(VFD+1P1+12A+12B+13A+13B+13C+13D+15A+15B+2P1+2P2)*0.1";
			var formula13 = "(VFD+1P1+12A+12B+13A+13B+13C+13D+15A+15B+2P1+2P2+2P3)*0.1";
			var formula14 = "(VFD+1P1+12A+12B+13A+13B+13C+13D+15A+15B+2P1+2P2+2P3+3P1)*0.1";
			var formula15 = "(VFD+1P1+12A+12B+13A+13B+13C+13D+15A+15B+2P1+2P2+2P3+3P1+3P2)*0.1";
			var formula16 = "(VFD+1P1+12A+12B+13A+13B+13C+13D+15A+15B+2P1+2P2+2P3+3P1+3P2+4P1)*0.1";
			var formula17 = "(VFD+1P1+12A+12B+13A+13B+13C+13D+15A+15B+2P1+2P2+2P3+3P1+3P2+4P1+4P2)*0.1";
			var formula18 = "(VFD+1P1+12A+12B+13A+13B+13C+13D+15A+15B+2P1+2P2+2P3+3P1+3P2+4P1+4P2+4P3)*0.1";
			var formula19 = "(VFD+1P1+12A+12B+13A+13B+13C+13D+15A+15B+2P1+2P2+2P3+3P1+3P2+4P1+4P2+4P3+4P4)*0.1";
			var formula20 = "(VFD+1P1+12A+12B+13A+13B+13C+13D+15A+15B+2P1+2P2+2P3+3P1+3P2+4P1+4P2+4P3+4P4+4P5)*0.1";
			var formula21 = "(VFD+1P1+12A+12B+13A+13B+13C+13D+15A+15B+2P1+2P2+2P3+3P1+3P2+4P1+4P2+4P3+4P4+4P5+4P6)*0.1";
			var formula22 = "(VFD+1P1+12A+12B+13A+13B+13C+13D+15A+15B+2P1+2P2+2P3+3P1+3P2+4P1+4P2+4P3+4P4+4P5+4P6+5P1)*0.1";
			var formula23 = "(VFD+1P1+12A+12B+13A+13B+13C+13D+15A+15B+2P1+2P2+2P3+3P1+3P2+4P1+4P2+4P3+4P4+4P5+4P6+5P1+5P2)*0.1";
			var formula24 = "(VFD+1P1+12A+12B+13A+13B+13C+13D+15A+15B+2P1+2P2+2P3+3P1+3P2+4P1+4P2+4P3+4P4+4P5+4P6+5P1+5P2+5P3)*0.1";
			var formula25 = "(VFD+1P1+12A+12B+13A+13B+13C+13D+15A+15B+2P1+2P2+2P3+3P1+3P2+4P1+4P2+4P3+4P4+4P5+4P6+5P1+5P2+5P3+5P4)*0.1";
			var formula26 = "(VFD+1P1+12A+12B+13A+13B+13C+13D+15A+15B+2P1+2P2+2P3+3P1+3P2+4P1+4P2+4P3+4P4+4P5+4P6+5P1+5P2+5P3+5P4+5P5)*0.1";
			var formula27 = "(VFD+1P1+12A+12B+13A+13B+13C+13D+15A+15B+2P1+2P2+2P3+3P1+3P2+4P1+4P2+4P3+4P4+4P5+4P6+5P1+5P2+5P3+5P4+5P5+6P1)*0.1";
			var formula28 = @"(VFD+1P1+12A+12B+13A+13B+13C+13D+15A+15B+2P1+2P2+2P3+3P1+3P2+4P1+4P2+4P3+4P4+4P5+4P6+5P1+5P2+5P3+5P4+5P5+6P1+6P2+{DECIMAL(6,3):""User Enter Value""})*0.1";
			var formula29 = @"(VFD+1P1+12A+12B+13A+13B+13C+13D+15A+15B+2P1+2P2+2P3+3P1+3P2+4P1+4P2+4P3+4P4+4P5+4P6+5P1+5P2+5P3+5P4+5P5+6P1+6P2+6P3+{DECIMAL(6,3):""User Enter Value""})*0.1";
			var tariff1 = CreateTariffViewWithRate(zaCountryCode, preference.PK, "1P1", "1P1", "1010101011", "0.1 * VFD", taxOrFeeCode: "VAT");
			var tariff2 = CreateTariffViewWithRateType(zaCountryCode, ZGuid.Empty, "12A", "12A", "1010101012", itCountryCode, formula2, rateType: Constants.RateTypes.Excise, relatedTariffCode: tariff1.ZZ1_TariffCode);
			var tariff3 = CreateTariffViewWithRateType(zaCountryCode, ZGuid.Empty, "12B", "12B", "1010101013", itCountryCode, formula3, rateType: Constants.RateTypes.AdValoremExcise, relatedTariffCode: tariff1.ZZ1_TariffCode);
			tariff3.Rates.First().CusRateType.ZZR_CustomsValueFormula = "CV+1P1+12A";
			var tariff4 = CreateTariffViewWithRateType(zaCountryCode, ZGuid.Empty, "13A", "13A", "1010101014", itCountryCode, formula4, rateType: Constants.RateTypes.Levy, relatedTariffCode: tariff1.ZZ1_TariffCode);
			var tariff5 = CreateTariffViewWithRateType(zaCountryCode, ZGuid.Empty, "13B", "13B", "1010101015", itCountryCode, formula5, rateType: Constants.RateTypes.Levy, relatedTariffCode: tariff1.ZZ1_TariffCode);
			var tariff6 = CreateTariffViewWithRateType(zaCountryCode, ZGuid.Empty, "13C", "13C", "1010101016", itCountryCode, formula6, rateType: Constants.RateTypes.Levy, relatedTariffCode: tariff1.ZZ1_TariffCode);
			var tariff7 = CreateTariffViewWithRateType(zaCountryCode, ZGuid.Empty, "13D", "13D", "1010101017", itCountryCode, formula7, rateType: Constants.RateTypes.Levy, relatedTariffCode: tariff1.ZZ1_TariffCode);
			var tariff8 = CreateTariffViewWithRateType(zaCountryCode, ZGuid.Empty, "15A", "15A", "1010101018", itCountryCode, formula8, rateType: Constants.RateTypes.Levy, relatedTariffCode: tariff1.ZZ1_TariffCode);
			var tariff9 = CreateTariffViewWithRateType(zaCountryCode, ZGuid.Empty, "15B", "15B", "1010101019", itCountryCode, formula9, rateType: Constants.RateTypes.Levy, relatedTariffCode: tariff1.ZZ1_TariffCode);
			var tariff10 = CreateTariffViewWithRateType(zaCountryCode, ZGuid.Empty, "2P1", "2P1", "1010101020", itCountryCode, formula10, rateType: Constants.RateTypes.AntiDumping, relatedTariffCode: tariff1.ZZ1_TariffCode);
			var tariff11 = CreateTariffViewWithRateType(zaCountryCode, ZGuid.Empty, "2P2", "2P2", "1010101021", itCountryCode, formula11, rateType: Constants.RateTypes.AntiDumping, relatedTariffCode: tariff1.ZZ1_TariffCode);
			var tariff12 = CreateTariffViewWithRateType(zaCountryCode, ZGuid.Empty, "2P3", "2P3", "1010101022", itCountryCode, formula12, rateType: Constants.RateTypes.AntiDumping, relatedTariffCode: tariff1.ZZ1_TariffCode);
			var tariff13 = CreateTariffViewWithRateType(zaCountryCode, ZGuid.Empty, "3P1", "3P1", "1010101023", itCountryCode, formula13, rateType: Constants.RateTypes.Rebate, relatedTariffCode: tariff1.ZZ1_TariffCode);
			var tariff14 = CreateTariffViewWithRateType(zaCountryCode, ZGuid.Empty, "3P2", "3P2", "1010101024", itCountryCode, formula14, rateType: Constants.RateTypes.Rebate, relatedTariffCode: tariff1.ZZ1_TariffCode);
			var tariff15 = CreateTariffViewWithRateType(zaCountryCode, ZGuid.Empty, "4P1", "4P1", "1010101025", itCountryCode, formula15, rateType: Constants.RateTypes.Rebate, relatedTariffCode: tariff1.ZZ1_TariffCode);
			var tariff16 = CreateTariffViewWithRateType(zaCountryCode, ZGuid.Empty, "4P2", "4P2", "1010101026", itCountryCode, formula16, rateType: Constants.RateTypes.Rebate, relatedTariffCode: tariff1.ZZ1_TariffCode);
			var tariff17 = CreateTariffViewWithRateType(zaCountryCode, ZGuid.Empty, "4P3", "4P3", "1010101027", itCountryCode, formula17, rateType: Constants.RateTypes.Rebate, relatedTariffCode: tariff1.ZZ1_TariffCode);
			var tariff18 = CreateTariffViewWithRateType(zaCountryCode, ZGuid.Empty, "4P4", "4P4", "1010101028", itCountryCode, formula18, rateType: Constants.RateTypes.Rebate, relatedTariffCode: tariff1.ZZ1_TariffCode);
			var tariff19 = CreateTariffViewWithRateType(zaCountryCode, ZGuid.Empty, "4P5", "4P5", "1010101029", itCountryCode, formula19, rateType: Constants.RateTypes.Rebate, relatedTariffCode: tariff1.ZZ1_TariffCode);
			var tariff20 = CreateTariffViewWithRateType(zaCountryCode, ZGuid.Empty, "4P6", "4P6", "1010101030", itCountryCode, formula20, rateType: Constants.RateTypes.Rebate, relatedTariffCode: tariff1.ZZ1_TariffCode);
			var tariff21 = CreateTariffViewWithRateType(zaCountryCode, ZGuid.Empty, "5P1", "5P1", "1010101031", itCountryCode, formula21, rateType: Constants.RateTypes.Rebate, relatedTariffCode: tariff1.ZZ1_TariffCode);
			var tariff22 = CreateTariffViewWithRateType(zaCountryCode, ZGuid.Empty, "5P2", "5P2", "1010101032", itCountryCode, formula22, rateType: Constants.RateTypes.Rebate, relatedTariffCode: tariff1.ZZ1_TariffCode);
			var tariff23 = CreateTariffViewWithRateType(zaCountryCode, ZGuid.Empty, "5P3", "5P3", "1010101033", itCountryCode, formula23, rateType: Constants.RateTypes.Rebate, relatedTariffCode: tariff1.ZZ1_TariffCode);
			var tariff24 = CreateTariffViewWithRateType(zaCountryCode, ZGuid.Empty, "5P4", "5P4", "1010101034", itCountryCode, formula24, rateType: Constants.RateTypes.Rebate, relatedTariffCode: tariff1.ZZ1_TariffCode);
			var tariff25 = CreateTariffViewWithRateType(zaCountryCode, ZGuid.Empty, "5P5", "5P5", "1010101035", itCountryCode, formula25, rateType: Constants.RateTypes.Rebate, relatedTariffCode: tariff1.ZZ1_TariffCode);
			var tariff26 = CreateTariffViewWithRateType(zaCountryCode, ZGuid.Empty, "6P1", "6P1", "1010101036", itCountryCode, formula26, rateType: Constants.RateTypes.Rebate, relatedTariffCode: tariff1.ZZ1_TariffCode);
			var tariff27 = CreateTariffViewWithRateType(zaCountryCode, ZGuid.Empty, "6P2", "6P2", "1010101037", itCountryCode, formula27, rateType: Constants.RateTypes.Rebate, relatedTariffCode: tariff1.ZZ1_TariffCode);
			var tariff28 = CreateTariffViewWithRateType(zaCountryCode, ZGuid.Empty, "6P3", "6P3", "1010101038", itCountryCode, formula28, rateType: Constants.RateTypes.Rebate, relatedTariffCode: tariff1.ZZ1_TariffCode);
			var tariff29 = CreateTariffViewWithRateType(zaCountryCode, ZGuid.Empty, "6P4", "6P4", "1010101039", itCountryCode, formula29, rateType: Constants.RateTypes.Rebate, relatedTariffCode: tariff1.ZZ1_TariffCode);
			factory.Save();
			var testApplicability1 = CreateCusApplicability(tariff1.Rates.FirstOrDefault(), testTradeGroup1, new ZDateTime(1980, 01, 01), new ZDateTime(2070, 06, 06));
			var testApplicability2 = CreateCusApplicability(tariff2.Rates.FirstOrDefault(), null, new ZDateTime(1980, 01, 01), new ZDateTime(2070, 06, 06));
			var testApplicability3 = CreateCusApplicability(tariff3.Rates.FirstOrDefault(), null, new ZDateTime(1980, 01, 01), new ZDateTime(2070, 06, 06));
			var testApplicability4 = CreateCusApplicability(tariff4.Rates.FirstOrDefault(), null, new ZDateTime(1980, 01, 01), new ZDateTime(2070, 06, 06));
			var testApplicability5 = CreateCusApplicability(tariff5.Rates.FirstOrDefault(), null, new ZDateTime(1980, 01, 01), new ZDateTime(2070, 06, 06));
			var testApplicability6 = CreateCusApplicability(tariff6.Rates.FirstOrDefault(), null, new ZDateTime(1980, 01, 01), new ZDateTime(2070, 06, 06));
			var testApplicability7 = CreateCusApplicability(tariff7.Rates.FirstOrDefault(), null, new ZDateTime(1980, 01, 01), new ZDateTime(2070, 06, 06));
			var testApplicability8 = CreateCusApplicability(tariff8.Rates.FirstOrDefault(), null, new ZDateTime(1980, 01, 01), new ZDateTime(2070, 06, 06));
			var testApplicability9 = CreateCusApplicability(tariff9.Rates.FirstOrDefault(), null, new ZDateTime(1980, 01, 01), new ZDateTime(2070, 06, 06));
			var testApplicability10 = CreateCusApplicability(tariff10.Rates.FirstOrDefault(), null, new ZDateTime(1980, 01, 01), new ZDateTime(2070, 06, 06));
			var testApplicability11 = CreateCusApplicability(tariff11.Rates.FirstOrDefault(), null, new ZDateTime(1980, 01, 01), new ZDateTime(2070, 06, 06));
			var testApplicability12 = CreateCusApplicability(tariff12.Rates.FirstOrDefault(), null, new ZDateTime(1980, 01, 01), new ZDateTime(2070, 06, 06));
			var testApplicability13 = CreateCusApplicability(tariff13.Rates.FirstOrDefault(), null, new ZDateTime(1980, 01, 01), new ZDateTime(2070, 06, 06));
			var testApplicability14 = CreateCusApplicability(tariff14.Rates.FirstOrDefault(), null, new ZDateTime(1980, 01, 01), new ZDateTime(2070, 06, 06));
			var testApplicability15 = CreateCusApplicability(tariff15.Rates.FirstOrDefault(), null, new ZDateTime(1980, 01, 01), new ZDateTime(2070, 06, 06));
			var testApplicability16 = CreateCusApplicability(tariff16.Rates.FirstOrDefault(), null, new ZDateTime(1980, 01, 01), new ZDateTime(2070, 06, 06));
			var testApplicability17 = CreateCusApplicability(tariff17.Rates.FirstOrDefault(), null, new ZDateTime(1980, 01, 01), new ZDateTime(2070, 06, 06));
			var testApplicability18 = CreateCusApplicability(tariff18.Rates.FirstOrDefault(), null, new ZDateTime(1980, 01, 01), new ZDateTime(2070, 06, 06));
			var testApplicability19 = CreateCusApplicability(tariff19.Rates.FirstOrDefault(), null, new ZDateTime(1980, 01, 01), new ZDateTime(2070, 06, 06));
			var testApplicability20 = CreateCusApplicability(tariff20.Rates.FirstOrDefault(), null, new ZDateTime(1980, 01, 01), new ZDateTime(2070, 06, 06));
			var testApplicability21 = CreateCusApplicability(tariff21.Rates.FirstOrDefault(), null, new ZDateTime(1980, 01, 01), new ZDateTime(2070, 06, 06));
			var testApplicability22 = CreateCusApplicability(tariff22.Rates.FirstOrDefault(), null, new ZDateTime(1980, 01, 01), new ZDateTime(2070, 06, 06));
			var testApplicability23 = CreateCusApplicability(tariff23.Rates.FirstOrDefault(), null, new ZDateTime(1980, 01, 01), new ZDateTime(2070, 06, 06));
			var testApplicability24 = CreateCusApplicability(tariff24.Rates.FirstOrDefault(), null, new ZDateTime(1980, 01, 01), new ZDateTime(2070, 06, 06));
			var testApplicability25 = CreateCusApplicability(tariff25.Rates.FirstOrDefault(), null, new ZDateTime(1980, 01, 01), new ZDateTime(2070, 06, 06));
			var testApplicability26 = CreateCusApplicability(tariff26.Rates.FirstOrDefault(), null, new ZDateTime(1980, 01, 01), new ZDateTime(2070, 06, 06));
			var testApplicability27 = CreateCusApplicability(tariff27.Rates.FirstOrDefault(), null, new ZDateTime(1980, 01, 01), new ZDateTime(2070, 06, 06));
			var testApplicability28 = CreateCusApplicability(tariff28.Rates.FirstOrDefault(), null, new ZDateTime(1980, 01, 01), new ZDateTime(2070, 06, 06));
			var testApplicability29 = CreateCusApplicability(tariff29.Rates.FirstOrDefault(), null, new ZDateTime(1980, 01, 01), new ZDateTime(2070, 06, 06));
			factory.Save();
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = ProcedureCodes._11;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.SouthAfrica;
			invoice.JZ_InvoiceAmount = 1000m;
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 1000m;
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Italy;
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.JI_Procedure = invoiceLine.EntryInstruction.CEI_Style + ProcedureCodes._20;
			invoiceLine.JI_Tariff = "1010101011";
			invoiceLine.JI_PrimaryPreference = "STANDARD";
			invoiceLine.CusLineTariffDetails.RemoveAndDeleteAll();
			invoiceLine.CusLineTariffDetails.AddNew(tariff2.ZZ1_ZZI_TariffTypeCode, tariff2.ZZ1_TariffCode);
			invoiceLine.CusLineTariffDetails.AddNew(tariff3.ZZ1_ZZI_TariffTypeCode, tariff3.ZZ1_TariffCode);
			invoiceLine.CusLineTariffDetails.AddNew(tariff4.ZZ1_ZZI_TariffTypeCode, tariff4.ZZ1_TariffCode);
			invoiceLine.CusLineTariffDetails.AddNew(tariff5.ZZ1_ZZI_TariffTypeCode, tariff5.ZZ1_TariffCode);
			invoiceLine.CusLineTariffDetails.AddNew(tariff6.ZZ1_ZZI_TariffTypeCode, tariff6.ZZ1_TariffCode);
			invoiceLine.CusLineTariffDetails.AddNew(tariff7.ZZ1_ZZI_TariffTypeCode, tariff7.ZZ1_TariffCode);
			invoiceLine.CusLineTariffDetails.AddNew(tariff8.ZZ1_ZZI_TariffTypeCode, tariff8.ZZ1_TariffCode);
			invoiceLine.CusLineTariffDetails.AddNew(tariff9.ZZ1_ZZI_TariffTypeCode, tariff9.ZZ1_TariffCode);
			invoiceLine.CusLineTariffDetails.AddNew(tariff10.ZZ1_ZZI_TariffTypeCode, tariff10.ZZ1_TariffCode);
			invoiceLine.CusLineTariffDetails.AddNew(tariff11.ZZ1_ZZI_TariffTypeCode, tariff11.ZZ1_TariffCode);
			invoiceLine.CusLineTariffDetails.AddNew(tariff12.ZZ1_ZZI_TariffTypeCode, tariff12.ZZ1_TariffCode);
			invoiceLine.CusLineTariffDetails.AddNew(tariff13.ZZ1_ZZI_TariffTypeCode, tariff13.ZZ1_TariffCode);
			invoiceLine.CusLineTariffDetails.AddNew(tariff14.ZZ1_ZZI_TariffTypeCode, tariff14.ZZ1_TariffCode);
			invoiceLine.CusLineTariffDetails.AddNew(tariff15.ZZ1_ZZI_TariffTypeCode, tariff15.ZZ1_TariffCode);
			invoiceLine.CusLineTariffDetails.AddNew(tariff16.ZZ1_ZZI_TariffTypeCode, tariff16.ZZ1_TariffCode);
			invoiceLine.CusLineTariffDetails.AddNew(tariff17.ZZ1_ZZI_TariffTypeCode, tariff17.ZZ1_TariffCode);
			invoiceLine.CusLineTariffDetails.AddNew(tariff18.ZZ1_ZZI_TariffTypeCode, tariff18.ZZ1_TariffCode);
			invoiceLine.CusLineTariffDetails.AddNew(tariff19.ZZ1_ZZI_TariffTypeCode, tariff19.ZZ1_TariffCode);
			invoiceLine.CusLineTariffDetails.AddNew(tariff20.ZZ1_ZZI_TariffTypeCode, tariff20.ZZ1_TariffCode);
			invoiceLine.CusLineTariffDetails.AddNew(tariff21.ZZ1_ZZI_TariffTypeCode, tariff21.ZZ1_TariffCode);
			invoiceLine.CusLineTariffDetails.AddNew(tariff22.ZZ1_ZZI_TariffTypeCode, tariff22.ZZ1_TariffCode);
			invoiceLine.CusLineTariffDetails.AddNew(tariff23.ZZ1_ZZI_TariffTypeCode, tariff23.ZZ1_TariffCode);
			invoiceLine.CusLineTariffDetails.AddNew(tariff24.ZZ1_ZZI_TariffTypeCode, tariff24.ZZ1_TariffCode);
			invoiceLine.CusLineTariffDetails.AddNew(tariff25.ZZ1_ZZI_TariffTypeCode, tariff25.ZZ1_TariffCode);
			invoiceLine.CusLineTariffDetails.AddNew(tariff26.ZZ1_ZZI_TariffTypeCode, tariff26.ZZ1_TariffCode);
			invoiceLine.CusLineTariffDetails.AddNew(tariff27.ZZ1_ZZI_TariffTypeCode, tariff27.ZZ1_TariffCode);
			var tariffDetail28 = invoiceLine.CusLineTariffDetails.AddNew(tariff28.ZZ1_ZZI_TariffTypeCode, tariff28.ZZ1_TariffCode);
			if ("User Enter Value" != tariffDetail28.FormulaSpecificQuestion)
			{
				throw new System.Exception("Test setup failed. PreCondition:tariffDetail28.FormulaSpecificQuestion. Expected 'User Enter Value' but was: " + tariffDetail28.FormulaSpecificQuestion);
			}

			tariffDetail28.FormulaSpecificValue = "200,545";
			var tariffDetail29 = invoiceLine.CusLineTariffDetails.AddNew(tariff29.ZZ1_ZZI_TariffTypeCode, tariff29.ZZ1_TariffCode);
			if ("User Enter Value" != tariffDetail29.FormulaSpecificQuestion)
			{
				throw new System.Exception("Test setup failed. PreCondition:tariffDetail29.FormulaSpecificQuestion. Expected 'User Enter Value' but was: " + tariffDetail29.FormulaSpecificQuestion);
			}

			tariffDetail29.FormulaSpecificValue = "600";
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			return declaration;
		}

		public TariffView CreateRateViewWithTariffTypeAndRateType(ZString country, ZString typeCode, string rateCode, string tariffCode, string rateType = Constants.RateTypes.Duty, string tariffTypeDesc = null, string taxOrFeeCode = null, string relatedTariffCode = null)
		{
			RefCusTariffType type;
			return CreateRateViewWithTariffTypeAndRateType(this, factory, country, typeCode, rateType, rateCode, tariffCode, out type, tariffTypeDesc: tariffTypeDesc, taxOrFeeCode: taxOrFeeCode, relatedTariffCode: relatedTariffCode);
		}

		public TariffView CreateTariffViewWithRateType(ZString country, ZGuid preferencePk, ZString typeCode, string rateCode, string tariffCode, ZString cofOValue, string rateFormula = null, string rateType = Constants.RateTypes.Duty, string relatedTariffCode = null)
		{
			return CreateTariffViewWithRateType(this, factory, country, preferencePk, typeCode, rateType, rateCode, tariffCode, rateFormula: rateFormula, relatedTariffCode: relatedTariffCode);
		}

		static TariffView CreateTariffViewWithRateType(ZAUniversalReferenceTestDataHelper helper, BusinessObjectFactory factory, ZString zaCountry, ZGuid preferencePk, ZString tariffType, ZString rateType, ZString rateCode, ZString tariffCode, string rateFormula = null, string relatedTariffCode = null)
		{
			var startDate = ZDateTime.Today.AddDays(-1);
			var endDate = ZDateTime.Today.AddDays(1);
			var cusTariffType = helper.CreateNewOrGetExistingTariffType(zaCountry, tariffType);
			cusTariffType.ZZI_Description = tariffType + "DESC";
			var rateType1 = helper.CreateNewOrGetExistingRateType(zaCountry, rateType);
			var rateCode1 = helper.LoadOrCreateNewCusRateCode(factory, rateCode, rateType1.PK);
			factory.Save();
			var tariffView1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.SouthAfrica, cusTariffType.PK, tariffCode, startDate, endDate, relatedTariffCode: relatedTariffCode, isSystem: true, ensureDataGroupingExists: true);
			var rateView = helper.CreateRate(tariffView1, rateCode1.PK, startDate, endDate, rateFormula, preferencePk);
			factory.Save();
			return tariffView1;
		}

		public TariffView CreateTariffViewWithRate(ZString country, ZGuid preferencePk, ZString typeCode, string rateCode, string tariffCode, string rateFormula = null, string taxOrFeeCode = null, string rateType = Constants.RateTypes.Duty)
		{
			return CreateTariffViewWithRate(this, factory, country, preferencePk, typeCode, rateCode, tariffCode, rateFormula: rateFormula, rateType: rateType, taxOrFeeCode: taxOrFeeCode);
		}

		static TariffView CreateTariffViewWithRate(ZAUniversalReferenceTestDataHelper helper, BusinessObjectFactory factory, ZString country, ZGuid preferencePk, ZString typeCode, string rateCode, string tariffCode, string rateFormula = null, string rateType = Constants.RateTypes.Duty, string taxOrFeeCode = null)
		{
			var startDate = ZDateTime.Today.AddDays(-1);
			var endDate = ZDateTime.Today.AddDays(1);
			var cusTariffType = helper.CreateNewOrGetExistingTariffType(country, typeCode);
			cusTariffType.ZZI_Description = typeCode + "DESC";
			var rateType1 = helper.CreateNewOrGetExistingRateType(country, rateType);
			var rateCode1 = helper.LoadOrCreateNewCusRateCode(factory, rateCode, rateType1.PK);
			factory.Save();
			var tariffView1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.SouthAfrica, cusTariffType.PK, tariffCode, startDate, endDate, taxOrFeeCode: taxOrFeeCode, isSystem: true, ensureDataGroupingExists: true);
			var rateView = helper.CreateRate(tariffView1, rateCode1.PK, startDate, endDate, rateFormula, preferencePk);
			factory.Save();
			return tariffView1;
		}

		public RefCusCodeList CreateAdditionalInformationCusCodeEntry(ZString code, string description = null)
		{
			if (additionalInformationType == null)
			{
				additionalInformationType = CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, "Additional Information");
			}

			var cusCode = CreateZACusCodeListEntry(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, code, description);
			switch (code)
			{
				case "ADI":
					{
						cusCode.ZZD_Description = "Anti-Dumping Duty Item";
						AddAttributesAndNames(cusCode, new KeyValuePair<string, string>[] { new KeyValuePair<string, string>(RefCusCodeListAttributeTypes.Codes.Import, ZString.Empty), new KeyValuePair<string, string>(RefCusCodeListAttributeTypes.Codes.Schedule, "2P1"), });
						break;
					}

				case UniversalReferenceConstants.AdditionalInformation.AgentRepresentingAForeignHaulier:
					{
						cusCode.ZZD_Description = "Agent representing a foreign Haulier";
						AddAttributesAndNames(cusCode, new KeyValuePair<string, string>[] { new KeyValuePair<string, string>(RefCusCodeListAttributeTypes.Codes.AllowSpace, RefCusCodeListAttributes.Values.NotAllowSpace), new KeyValuePair<string, string>(RefCusCodeListAttributeTypes.Codes.PadWith, "0"), new KeyValuePair<string, string>(RefCusCodeListAttributeTypes.Codes.OnlyForLine1, ZString.Empty), });
						break;
					}

				case UniversalReferenceConstants.AdditionalInformation.AgentRepresentingAForeignImporterOrExporter:
					{
						cusCode.ZZD_Description = "Agent representing a foreign Importer or Exporter";
						AddAttributesAndNames(cusCode, new KeyValuePair<string, string>[] { new KeyValuePair<string, string>(RefCusCodeListAttributeTypes.Codes.AllowSpace, RefCusCodeListAttributes.Values.NotAllowSpace), new KeyValuePair<string, string>(RefCusCodeListAttributeTypes.Codes.PadWith, "0"), new KeyValuePair<string, string>(RefCusCodeListAttributeTypes.Codes.OnlyForLine1, ZString.Empty), });
						break;
					}

				case "AGO":
					{
						cusCode.ZZD_Description = "African Growth and Opportunities Act.";
						AddAttributesAndNames(cusCode, new KeyValuePair<string, string>[] { new KeyValuePair<string, string>(RefCusCodeListAttributeTypes.Codes.Export, ZString.Empty), new KeyValuePair<string, string>(RefCusCodeListAttributeTypes.Codes.ROOType, ZString.Empty), });
						break;
					}

				case UniversalReferenceConstants.AdditionalInformation.ApprovedExporter:
					{
						cusCode.ZZD_Description = "Approved Exporter";
						AddAttributesAndNames(cusCode, new KeyValuePair<string, string>[] { new KeyValuePair<string, string>(RefCusCodeListAttributeTypes.Codes.Export, ZString.Empty), new KeyValuePair<string, string>(RefCusCodeListAttributeTypes.Codes.ROOType, ZString.Empty), });
						break;
					}

				case UniversalReferenceConstants.AdditionalInformation.BondHolder:
					{
						cusCode.ZZD_Description = "Bond Holder";
						AddAttributesAndNames(cusCode, new KeyValuePair<string, string>(RefCusCodeListAttributeTypes.Codes.OnlyForLine1, ZString.Empty));
						break;
					}

				case UniversalReferenceConstants.AdditionalInformation.BondSuretyAmount:
					{
						cusCode.ZZD_Description = "Bond Surety Amount";
						AddAttributesAndNames(cusCode, new KeyValuePair<string, string>("Amount", "0"));
						break;
					}

				case UniversalReferenceConstants.AdditionalInformation.CountervailingDutyItem:
					{
						cusCode.ZZD_Description = "Countervailing Duty Item";
						AddAttributesAndNames(cusCode, new KeyValuePair<string, string>[] { new KeyValuePair<string, string>(RefCusCodeListAttributeTypes.Codes.AllowSpace, RefCusCodeListAttributes.Values.NotAllowSpace), new KeyValuePair<string, string>(RefCusCodeListAttributeTypes.Codes.PadWith, "0"), new KeyValuePair<string, string>(RefCusCodeListAttributeTypes.Codes.Import, ZString.Empty), new KeyValuePair<string, string>(RefCusCodeListAttributeTypes.Codes.Schedule, "2P2"), });
						break;
					}

				case UniversalReferenceConstants.AdditionalInformation.DiamondBeneficiaryLicense:
					{
						cusCode.ZZD_Description = "Diamond Beneficiary License";
						break;
					}

				case UniversalReferenceConstants.AdditionalInformation.DiamondDealerLicense:
					{
						cusCode.ZZD_Description = "Diamond Dealer License";
						break;
					}

				case UniversalReferenceConstants.AdditionalInformation.DiamondLevyValue:
					{
						cusCode.ZZD_Description = "Diamond Levy Value";
						AddAttributesAndNames(cusCode, new KeyValuePair<string, string>[] { new KeyValuePair<string, string>("Amount", "0"), new KeyValuePair<string, string>(RefCusCodeListAttributeTypes.Codes.Export, ZString.Empty), });
						break;
					}

				case UniversalReferenceConstants.AdditionalInformation.DiamondProducerExemption:
					{
						cusCode.ZZD_Description = "Diamond Producer Exemption";
						AddAttributesAndNames(cusCode, new KeyValuePair<string, string>(RefCusCodeListAttributeTypes.Codes.Export, ZString.Empty));
						break;
					}

				case UniversalReferenceConstants.AdditionalInformation.DiamondProducerRegistration:
					{
						cusCode.ZZD_Description = "Diamond Producer Registration";
						break;
					}

				case "DCC":
					{
						cusCode.ZZD_Description = "Duty Credit Certificate";
						AddAttributesAndNames(cusCode, new KeyValuePair<string, string>[] { new KeyValuePair<string, string>(RefCusCodeListAttributeTypes.Codes.Import, ZString.Empty), new KeyValuePair<string, string>(RefCusCodeListAttributeTypes.Codes.Pair, "DCV"), });
						break;
					}

				case UniversalReferenceConstants.AdditionalInformation.DutyCreditValue:
					{
						cusCode.ZZD_Description = "Duty Credit Value";
						AddAttributesAndNames(cusCode, new KeyValuePair<string, string>[] { new KeyValuePair<string, string>(RefCusCodeListAttributeTypes.Codes.Amount, "0"), new KeyValuePair<string, string>(RefCusCodeListAttributeTypes.Codes.Import, ZString.Empty), new KeyValuePair<string, string>(RefCusCodeListAttributeTypes.Codes.Pair, "DCC"), });
						break;
					}

				case UniversalReferenceConstants.AdditionalInformation.ElectionsExemptionsLevy:
					{
						cusCode.ZZD_Description = "Elections Exemptions Levy";
						break;
					}

				case "ETA":
					{
						cusCode.ZZD_Description = "EFTA European Free Trade Agreement";
						AddAttributesAndNames(cusCode, new KeyValuePair<string, string>[] { new KeyValuePair<string, string>("CusApprovedExporter", ZString.Empty), new KeyValuePair<string, string>(RefCusCodeListAttributeTypes.Codes.Export, ZString.Empty), new KeyValuePair<string, string>(RefCusCodeListAttributeTypes.Codes.ROOType, ZString.Empty), });
						break;
					}

				case "EUR":
					{
						cusCode.ZZD_Description = "EU Free Trade Agreement";
						AddAttributesAndNames(cusCode, new KeyValuePair<string, string>[] { new KeyValuePair<string, string>("CusApprovedExporter", ZString.Empty), new KeyValuePair<string, string>(RefCusCodeListAttributeTypes.Codes.Export, ZString.Empty), new KeyValuePair<string, string>(RefCusCodeListAttributeTypes.Codes.ROOType, ZString.Empty), });
						break;
					}

				case UniversalReferenceConstants.AdditionalInformation.ExportPermitControl:
					{
						cusCode.ZZD_Description = "Export Permit Control";
						AddAttributesAndNames(cusCode, new KeyValuePair<string, string>[] { new KeyValuePair<string, string>(RefCusCodeListAttributeTypes.Codes.Export, ZString.Empty), new KeyValuePair<string, string>("LicenceNumber", "LicenceNumber"), });
						break;
					}

				case "GSP":
					{
						cusCode.ZZD_Description = "General System Of Preferences";
						AddAttributesAndNames(cusCode, new KeyValuePair<string, string>[] { new KeyValuePair<string, string>(RefCusCodeListAttributeTypes.Codes.Export, ZString.Empty), new KeyValuePair<string, string>(RefCusCodeListAttributeTypes.Codes.ROOType, ZString.Empty), });
						break;
					}

				case UniversalReferenceConstants.AdditionalInformation.ImportPermitControl:
					{
						cusCode.ZZD_Description = "Import Permit Control";
						AddAttributesAndNames(cusCode, new KeyValuePair<string, string>(RefCusCodeListAttributeTypes.Codes.Import, ZString.Empty));
						break;
					}

				case UniversalReferenceConstants.AdditionalInformation.KimberleyCertificate:
					{
						cusCode.ZZD_Description = "Kimberley Certificate";
						break;
					}

				case "MER":
					{
						cusCode.ZZD_Description = "MERCOSUR Rules Of Origin";
						AddAttributesAndNames(cusCode, new KeyValuePair<string, string>[] { new KeyValuePair<string, string>(RefCusCodeListAttributeTypes.Codes.Export, ZString.Empty), new KeyValuePair<string, string>(RefCusCodeListAttributeTypes.Codes.ROOType, ZString.Empty), });
						break;
					}

				case UniversalReferenceConstants.AdditionalInformation.NewUsedIndicator:
					{
						cusCode.ZZD_Description = "New Used Indicator";
						break;
					}

				case UniversalReferenceConstants.AdditionalInformation.OrdinaryLevyItem:
					{
						cusCode.ZZD_Description = "Ordinary Levy Item";
						AddAttributesAndNames(cusCode, new KeyValuePair<string, string>(RefCusCodeListAttributeTypes.Codes.Import, ZString.Empty));
						break;
					}

				case "PGR":
					{
						cusCode.ZZD_Description = "Permit for various 4th Schedule General Rebates issued by ITAC";
						AddAttributesAndNames(cusCode, new KeyValuePair<string, string>[] { new KeyValuePair<string, string>(RefCusCodeListAttributeTypes.Codes.Import, ZString.Empty), new KeyValuePair<string, string>(RefCusCodeListAttributeTypes.Codes.LicenceNumber, "LicenceNumber"), });
						break;
					}

				case "PPR":
					{
						cusCode.ZZD_Description = "Provisional Payment Surety Reference";
						break;
					}

				case UniversalReferenceConstants.AdditionalInformation.ProvisionalPaymentSurety:
					{
						cusCode.ZZD_Description = "Provisional Payment Surety";
						AddAttributesAndNames(cusCode, new KeyValuePair<string, string>[] { new KeyValuePair<string, string>(RefCusCodeListAttributeTypes.Codes.AllowSpace, RefCusCodeListAttributes.Values.NotAllowSpace), new KeyValuePair<string, string>(RefCusCodeListAttributeTypes.Codes.PadWith, "0"), });
						break;
					}

				case UniversalReferenceConstants.AdditionalInformation.RebateCreditCertificate:
					{
						cusCode.ZZD_Description = "Rebate Credit Certificate";
						AddAttributesAndNames(cusCode, new KeyValuePair<string, string>[] { new KeyValuePair<string, string>(RefCusCodeListAttributeTypes.Codes.Import, ZString.Empty), new KeyValuePair<string, string>(RefCusCodeListAttributeTypes.Codes.Pair, "RCV"), });
						break;
					}

				case UniversalReferenceConstants.AdditionalInformation.RebateCreditValue:
					{
						cusCode.ZZD_Description = "Credit Rebate Value";
						AddAttributesAndNames(cusCode, new KeyValuePair<string, string>[] { new KeyValuePair<string, string>(RefCusCodeListAttributeTypes.Codes.Amount, "0"), new KeyValuePair<string, string>(RefCusCodeListAttributeTypes.Codes.Import, ZString.Empty), new KeyValuePair<string, string>(RefCusCodeListAttributeTypes.Codes.Pair, "RCC"), });
						break;
					}

				case UniversalReferenceConstants.AdditionalInformation.RulesOfOrigin:
					{
						cusCode.ZZD_Description = "Rules Of Origin";
						AddAttributesAndNames(cusCode, new KeyValuePair<string, string>[] { new KeyValuePair<string, string>(RefCusCodeListAttributeTypes.Codes.AllowEmpty, ZString.Empty), new KeyValuePair<string, string>(RefCusCodeListAttributeTypes.Codes.Import, ZString.Empty), });
						break;
					}

				case "SAD":
					{
						cusCode.ZZD_Description = "SADC Agreement";
						AddAttributesAndNames(cusCode, new KeyValuePair<string, string>[] { new KeyValuePair<string, string>(RefCusCodeListAttributeTypes.Codes.Export, ZString.Empty), new KeyValuePair<string, string>(RefCusCodeListAttributeTypes.Codes.ROOType, ZString.Empty), });
						break;
					}

				case UniversalReferenceConstants.AdditionalInformation.TemporaryBuyersPermit:
					{
						cusCode.ZZD_Description = "Temporary Buyers Permit";
						AddAttributesAndNames(cusCode, new KeyValuePair<string, string>(RefCusCodeListAttributeTypes.Codes.Export, ZString.Empty));
						break;
					}

				case UniversalReferenceConstants.AdditionalInformation.TemporaryExportExemption:
					{
						cusCode.ZZD_Description = "Temporary Export Exemption";
						AddAttributesAndNames(cusCode, new KeyValuePair<string, string>(RefCusCodeListAttributeTypes.Codes.Export, ZString.Empty));
						break;
					}

				case UniversalReferenceConstants.AdditionalInformation.VATTaxExemptions:
					{
						cusCode.ZZD_Description = "VAT Tax Exemptions";
						AddAttributesAndNames(cusCode, new KeyValuePair<string, string>[] { new KeyValuePair<string, string>(RefCusCodeListAttributeTypes.Codes.Import, ZString.Empty), new KeyValuePair<string, string>("AllowEmpty", ZString.Empty), });
						break;
					}

				case UniversalReferenceConstants.AdditionalInformation.SafeguardDutyItem:
					{
						cusCode.ZZD_Description = "VAT Tax Exemptions";
						AddAttributesAndNames(cusCode, new KeyValuePair<string, string>[] { new KeyValuePair<string, string>(RefCusCodeListAttributeTypes.Codes.AllowSpace, RefCusCodeListAttributes.Values.NotAllowSpace), new KeyValuePair<string, string>(RefCusCodeListAttributeTypes.Codes.PadWith, "0"), new KeyValuePair<string, string>(RefCusCodeListAttributeTypes.Codes.Import, ZString.Empty), new KeyValuePair<string, string>(RefCusCodeListAttributeTypes.Codes.Schedule, "2P3"), });
						break;
					}

				case UniversalReferenceConstants.AdditionalInformation.ValueDeterminationNumber:
					{
						cusCode.ZZD_Description = "Value Determination Number";
						break;
					}

				case UniversalReferenceConstants.AdditionalInformation.VehicleIdentificationNumber:
					{
						cusCode.ZZD_Description = "Vehicle Identification Number";
						break;
					}

				case "DT1":
					{
						cusCode.ZZD_Description = "Decimals Test 1";
						AddAttributesAndNames(cusCode, new KeyValuePair<string, string>("Amount", "1"));
						break;
					}

				case "DT2":
					{
						cusCode.ZZD_Description = "Decimals Test 2";
						AddAttributesAndNames(cusCode, new KeyValuePair<string, string>("Amount", "2"));
						break;
					}

				case UniversalReferenceConstants.AdditionalInformation.ProductionRebateCertificate:
					{
						cusCode.ZZD_Description = "Production Rebate Certificate";
						AddAttributesAndNames(cusCode, new KeyValuePair<string, string>[] { new KeyValuePair<string, string>(RefCusCodeListAttributeTypes.Codes.Import, ZString.Empty), new KeyValuePair<string, string>(RefCusCodeListAttributeTypes.Codes.Pair, "PRC"), });
						break;
					}

				case UniversalReferenceConstants.AdditionalInformation.ProductionRebateValue:
					{
						cusCode.ZZD_Description = "Production Rebate Value";
						AddAttributesAndNames(cusCode, new KeyValuePair<string, string>[] { new KeyValuePair<string, string>(RefCusCodeListAttributeTypes.Codes.Import, ZString.Empty), new KeyValuePair<string, string>(RefCusCodeListAttributeTypes.Codes.Pair, "PRV"), });
						break;
					}

				case "UK":
					{
						cusCode.ZZD_Description = "Two letter ROO code test";
						cusCode.ZZD_StartDate = ZDateTime.Today.AddDays(-2);
						cusCode.ZZD_EndDate = ZDateTime.Today.AddDays(2);
						AddAttributesAndNames(cusCode, new KeyValuePair<string, string>[] { new KeyValuePair<string, string>(RefCusCodeListAttributeTypes.Codes.Export, ZString.Empty), new KeyValuePair<string, string>(RefCusCodeListAttributeTypes.Codes.ROOType, ZString.Empty), });
						break;
					}
			}

			return cusCode;
		}

		public RefCusCodeList CreateCustomsStatusCusCodeEntry(ZString code, string description = null)
		{
			if (customsStatusType == null)
			{
				customsStatusType = CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "Customs Status");
			}

			var cusCode = CreateZACusCodeListEntry(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, code, description);
			switch (code)
			{
				case "1":
					{
						cusCode.ZZD_Description = "Release";
						AddAttributesAndNames(cusCode, new[] { new KeyValuePair<string, string>("IMarkEntryPayInfoAwaitingResp", "true"), new KeyValuePair<string, string>("CustomsCleared", "true"), new KeyValuePair<string, string>("IAddEntryDocsToEDocs", ZString.Empty), new KeyValuePair<string, string>("IAllowCancel", "true"), new KeyValuePair<string, string>("INotify", ZString.Empty), new KeyValuePair<string, string>("ISendEntryDocs", ZString.Empty), new KeyValuePair<string, string>("IUpdateBondedWhs", ZString.Empty), new KeyValuePair<string, string>("IUpdateCustomsStatus", ZString.Empty), new KeyValuePair<string, string>("IUpdateEntryNumber", ZString.Empty), new KeyValuePair<string, string>("IUpdateReleaseDate", ZString.Empty), new KeyValuePair<string, string>("IPostCustomsAPInvoice", "true"), });
						break;
					}

				case "2":
					{
						cusCode.ZZD_Description = "Stop/Detain";
						AddAttributesAndNames(cusCode, new[] { new KeyValuePair<string, string>("IMarkEntryPayInfoAwaitingResp", "true"), new KeyValuePair<string, string>("IAllowCancel", "true"), new KeyValuePair<string, string>("INotify", ZString.Empty), new KeyValuePair<string, string>("ISendEntryDocs", ZString.Empty), new KeyValuePair<string, string>("IUpdateCustomsStatus", ZString.Empty), new KeyValuePair<string, string>("IUpdateEntryNumber", ZString.Empty), });
						break;
					}

				case "3":
					{
						cusCode.ZZD_Description = "Conditional Release";
						AddAttributesAndNames(cusCode, new[] { new KeyValuePair<string, string>("IMarkEntryPayInfoAwaitingResp", "true"), new KeyValuePair<string, string>("CustomsCleared", "true"), new KeyValuePair<string, string>("IAddEntryDocsToEDocs", ZString.Empty), new KeyValuePair<string, string>("IAllowCancel", "true"), new KeyValuePair<string, string>("INotify", ZString.Empty), new KeyValuePair<string, string>("ISendEntryDocs", ZString.Empty), new KeyValuePair<string, string>("IUpdateBondedWhs", ZString.Empty), new KeyValuePair<string, string>("IUpdateCustomsStatus", ZString.Empty), new KeyValuePair<string, string>("IUpdateEntryNumber", ZString.Empty), new KeyValuePair<string, string>("IUpdateReleaseDate", ZString.Empty), new KeyValuePair<string, string>("IPostCustomsAPInvoice", "true"), });
						break;
					}

				case "4":
					{
						cusCode.ZZD_Description = "Detain Other (Other Government Agency - OGA)";
						AddAttributesAndNames(cusCode, new[] { new KeyValuePair<string, string>("IMarkEntryPayInfoAwaitingResp", "true"), new KeyValuePair<string, string>("CustomsCleared", "true"), new KeyValuePair<string, string>("IAddEntryDocsToEDocs", ZString.Empty), new KeyValuePair<string, string>("IAllowCancel", "true"), new KeyValuePair<string, string>("INotify", ZString.Empty), new KeyValuePair<string, string>("ISendEntryDocs", ZString.Empty), new KeyValuePair<string, string>("IUpdateBondedWhs", ZString.Empty), new KeyValuePair<string, string>("IUpdateCustomsStatus", ZString.Empty), new KeyValuePair<string, string>("IUpdateEntryNumber", ZString.Empty), new KeyValuePair<string, string>("IUpdateReleaseDate", ZString.Empty), new KeyValuePair<string, string>("IPostCustomsAPInvoice", "true"), });
						break;
					}

				case "5":
					{
						cusCode.ZZD_Description = "Conditional release - Detain Other (OGA)";
						AddAttributesAndNames(cusCode, new[] { new KeyValuePair<string, string>("IMarkEntryPayInfoAwaitingResp", "true"), new KeyValuePair<string, string>("CustomsCleared", "true"), new KeyValuePair<string, string>("IAddEntryDocsToEDocs", ZString.Empty), new KeyValuePair<string, string>("IAllowCancel", "true"), new KeyValuePair<string, string>("INotify", ZString.Empty), new KeyValuePair<string, string>("ISendEntryDocs", ZString.Empty), new KeyValuePair<string, string>("IUpdateBondedWhs", ZString.Empty), new KeyValuePair<string, string>("IUpdateCustomsStatus", ZString.Empty), new KeyValuePair<string, string>("IUpdateEntryNumber", ZString.Empty), new KeyValuePair<string, string>("IUpdateReleaseDate", ZString.Empty), new KeyValuePair<string, string>("IPostCustomsAPInvoice", "true"), });
						break;
					}

				case "6":
					{
						cusCode.ZZD_Description = "Reject To Clearer";
						AddAttributesAndNames(cusCode, new[] { new KeyValuePair<string, string>("IMarkEntryPayInfoAwaitingResp", "true"), new KeyValuePair<string, string>("CustomsRejected", "true"), new KeyValuePair<string, string>("ICancelBondedWhs", ZString.Empty), new KeyValuePair<string, string>("INotify", ZString.Empty), new KeyValuePair<string, string>("ISendEntryDocs", ZString.Empty), new KeyValuePair<string, string>("IUpdateCustomsStatus", ZString.Empty), });
						break;
					}

				case "7":
					{
						cusCode.ZZD_Description = "Ready For Cash Payment";
						AddAttributesAndNames(cusCode, new[] { new KeyValuePair<string, string>("IMarkEntryPayInfoAwaitingResp", "true"), new KeyValuePair<string, string>("IAllowCancel", "true"), new KeyValuePair<string, string>("INotify", ZString.Empty), new KeyValuePair<string, string>("ISendEntryDocs", ZString.Empty), new KeyValuePair<string, string>("IUpdateCustomsStatus", ZString.Empty), new KeyValuePair<string, string>("IUpdateEntryNumber", ZString.Empty), });
						break;
					}

				case "8":
					{
						cusCode.ZZD_Description = "Proceed to Border (SACU clearances)";
						AddAttributesAndNames(cusCode, new[] { new KeyValuePair<string, string>("IMarkEntryPayInfoAwaitingResp", "true"), new KeyValuePair<string, string>("IAddEntryDocsToEDocs", ZString.Empty), new KeyValuePair<string, string>("IAllowCancel", "true"), new KeyValuePair<string, string>("INotify", ZString.Empty), new KeyValuePair<string, string>("ISendEntryDocs", ZString.Empty), new KeyValuePair<string, string>("IUpdateCustomsStatus", ZString.Empty), new KeyValuePair<string, string>("IUpdateEntryNumber", ZString.Empty), });
						break;
					}

				case "9":
					{
						cusCode.ZZD_Description = "Already on Customs system";
						AddAttributesAndNames(cusCode, new[] { new KeyValuePair<string, string>("IMarkEntryPayInfoAwaitingResp", "true"), new KeyValuePair<string, string>("INotify", ZString.Empty), new KeyValuePair<string, string>("IUpdateEntryNumber", ZString.Empty), });
						break;
					}

				case "13":
					{
						cusCode.ZZD_Description = "Query - Supporting documents required";
						AddAttributesAndNames(cusCode, new[] { new KeyValuePair<string, string>("IMarkEntryPayInfoAwaitingResp", "true"), new KeyValuePair<string, string>("INotify", ZString.Empty), new KeyValuePair<string, string>("ISendEntryDocs", ZString.Empty), new KeyValuePair<string, string>("IUpdateCustomsStatus", ZString.Empty), new KeyValuePair<string, string>("IUpdateEntryNumber", ZString.Empty), });
						break;
					}

				case "14":
					{
						cusCode.ZZD_Description = "Notice To Upload Supporting Cargo Clearance / Customs Clearance Information";
						AddAttributesAndNames(cusCode, new[] { new KeyValuePair<string, string>("IMarkEntryPayInfoAwaitingResp", "true"), new KeyValuePair<string, string>("INotify", ZString.Empty), new KeyValuePair<string, string>("ISendEntryDocs", ZString.Empty), new KeyValuePair<string, string>("IUpdateCustomsStatus", ZString.Empty), new KeyValuePair<string, string>("IUpdateEntryNumber", ZString.Empty), });
						break;
					}

				case "26":
					{
						cusCode.ZZD_Description = "Amendment notification (Inspection report outcome)";
						AddAttributesAndNames(cusCode, new[] { new KeyValuePair<string, string>("IMarkEntryPayInfoAwaitingResp", "true"), new KeyValuePair<string, string>("IAllowCancel", "true"), new KeyValuePair<string, string>("INotify", ZString.Empty), new KeyValuePair<string, string>("ISendEntryDocs", ZString.Empty), new KeyValuePair<string, string>("IUpdateCustomsStatus", ZString.Empty), new KeyValuePair<string, string>("IUpdateEntryNumber", ZString.Empty), });
						break;
					}

				case "27":
					{
						cusCode.ZZD_Description = "Amendment granted";
						AddAttributesAndNames(cusCode, new[] { new KeyValuePair<string, string>("IMarkEntryPayInfoAwaitingResp", "true"), new KeyValuePair<string, string>("CustomsCleared", "true"), new KeyValuePair<string, string>("IAddEntryDocsToEDocs", ZString.Empty), new KeyValuePair<string, string>("IAllowCancel", "true"), new KeyValuePair<string, string>("INotify", ZString.Empty), new KeyValuePair<string, string>("ISendEntryDocs", ZString.Empty), new KeyValuePair<string, string>("IUpdateBondedWhs", ZString.Empty), new KeyValuePair<string, string>("IUpdateCustomsStatus", ZString.Empty), new KeyValuePair<string, string>("IUpdateEntryNumber", ZString.Empty), new KeyValuePair<string, string>("IUpdateReleaseDate", ZString.Empty), new KeyValuePair<string, string>("IPostCustomsAPInvoice", "true"), });
						break;
					}

				case "28":
					{
						cusCode.ZZD_Description = "Cancellation granted";
						AddAttributesAndNames(cusCode, new[] { new KeyValuePair<string, string>("IMarkEntryPayInfoAwaitingResp", "true"), new KeyValuePair<string, string>("ICustomsCancelled", ZString.Empty), new KeyValuePair<string, string>("INotify", ZString.Empty), new KeyValuePair<string, string>("ISendEntryDocs", ZString.Empty), new KeyValuePair<string, string>("IUpdateBondedWhs", ZString.Empty), new KeyValuePair<string, string>("IUpdateCustomsStatus", ZString.Empty), new KeyValuePair<string, string>("IUpdateEntryNumber", ZString.Empty), });
						break;
					}

				case "29":
					{
						cusCode.ZZD_Description = "Replacement granted";
						AddAttributesAndNames(cusCode, new[] { new KeyValuePair<string, string>("IMarkEntryPayInfoAwaitingResp", "true"), new KeyValuePair<string, string>("CustomsCleared", "true"), new KeyValuePair<string, string>("IAddEntryDocsToEDocs", ZString.Empty), new KeyValuePair<string, string>("IAllowCancel", "true"), new KeyValuePair<string, string>("INotify", ZString.Empty), new KeyValuePair<string, string>("ISendEntryDocs", ZString.Empty), new KeyValuePair<string, string>("IUpdateBondedWhs", ZString.Empty), new KeyValuePair<string, string>("IUpdateCustomsStatus", ZString.Empty), new KeyValuePair<string, string>("IUpdateEntryNumber", ZString.Empty), new KeyValuePair<string, string>("IUpdateReleaseDate", ZString.Empty), new KeyValuePair<string, string>("IPostCustomsAPInvoice", "true"), });
						break;
					}

				case "33":
					{
						cusCode.ZZD_Description = "Supporting documents received";
						AddAttributesAndNames(cusCode, new[] { new KeyValuePair<string, string>("IMarkEntryPayInfoAwaitingResp", "true"), new KeyValuePair<string, string>("INotify", ZString.Empty), new KeyValuePair<string, string>("IUpdateCustomsStatus", ZString.Empty), new KeyValuePair<string, string>("IUpdateEntryNumber", ZString.Empty), });
						break;
					}

				case "34":
					{
						cusCode.ZZD_Description = "Case Closed";
						AddAttributesAndNames(cusCode, new[] { new KeyValuePair<string, string>("IMarkEntryPayInfoAwaitingResp", "true") });
						break;
					}

				case "38":
					{
						cusCode.ZZD_Description = "Offline Release";
						AddAttributesAndNames(cusCode, new[] { new KeyValuePair<string, string>("IMarkEntryPayInfoAwaitingResp", "true"), new KeyValuePair<string, string>("CustomsCleared", "true"), new KeyValuePair<string, string>("IAddEntryDocsToEDocs", ZString.Empty), new KeyValuePair<string, string>("IAllowCancel", "true"), new KeyValuePair<string, string>("INotify", ZString.Empty), new KeyValuePair<string, string>("ISendEntryDocs", ZString.Empty), new KeyValuePair<string, string>("IUpdateBondedWhs", ZString.Empty), new KeyValuePair<string, string>("IUpdateCustomsStatus", ZString.Empty), new KeyValuePair<string, string>("IUpdateEntryNumber", ZString.Empty), new KeyValuePair<string, string>("IUpdateReleaseDate", ZString.Empty), new KeyValuePair<string, string>("IPostCustomsAPInvoice", "true"), });
						break;
					}

				case "40":
					{
						cusCode.ZZD_Description = "Entry Arrival Notification";
						AddAttributesAndNames(cusCode, new[] { new KeyValuePair<string, string>("IMarkEntryPayInfoAwaitingResp", "true"), new KeyValuePair<string, string>("INotify", ZString.Empty), new KeyValuePair<string, string>("ISendEntryDocs", ZString.Empty), new KeyValuePair<string, string>("IUpdateCustomsStatus", ZString.Empty), new KeyValuePair<string, string>("IUpdateEntryNumber", ZString.Empty), });
						break;
					}

				case "43":
					{
						cusCode.ZZD_Description = "Appeal Application Received";
						AddAttributesAndNames(cusCode, new[] { new KeyValuePair<string, string>("IMarkEntryPayInfoAwaitingResp", "true"), new KeyValuePair<string, string>("INotify", ZString.Empty), new KeyValuePair<string, string>("IUpdateProvisionalPaymentStatus", ZString.Empty) });
						break;
					}

				case "49":
					{
						cusCode.ZZD_Description = "Provisional Payment Liquidated";
						AddAttributesAndNames(cusCode, new[] { new KeyValuePair<string, string>("IMarkEntryPayInfoAwaitingResp", "true"), new KeyValuePair<string, string>("INotify", ZString.Empty), new KeyValuePair<string, string>("IUpdateProvisionalPaymentStatus", ZString.Empty), new KeyValuePair<string, string>("IsLiquidatedStatus", ZString.Empty) });
						break;
					}
			}

			return cusCode;
		}

		public RefCusCodeList CreateCustomsOfficeCusCodeEntry(ZString code, string description = null)
		{
			if (customsOfficeType == null)
			{
				customsOfficeType = CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			}

			var cusCode = CreateZACusCodeListEntry(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, code, description);
			switch (code)
			{
				case "BBR":
					{
						cusCode.ZZD_Description = "Beit Bridge";
						AddAttributesAndNames(cusCode, new KeyValuePair<string, string>("Code", "15"));
						break;
					}

				case "BFN":
					{
						cusCode.ZZD_Description = "BLOEMFONTEIN";
						AddAttributesAndNames(cusCode, new KeyValuePair<string, string>("Code", "60"));
						break;
					}

				case "CLP":
					{
						cusCode.ZZD_Description = "CALEDONSPOORT";
						AddAttributesAndNames(cusCode, new KeyValuePair<string, string>("Code", "62"));
						break;
					}

				case "CTN":
					{
						cusCode.ZZD_Description = "CAPE TOWN";
						AddAttributesAndNames(cusCode, new KeyValuePair<string, string>("Code", "30"));
						break;
					}

				case "DBN":
					{
						cusCode.ZZD_Description = "DURBAN";
						AddAttributesAndNames(cusCode, new KeyValuePair<string, string>("Code", "50"));
						break;
					}

				case "JHB":
					{
						cusCode.ZZD_Description = "JOHANNESBURG";
						AddAttributesAndNames(cusCode, new KeyValuePair<string, string>("Code", "10"));
						break;
					}

				case "JSA":
					{
						cusCode.ZZD_Description = "O.R. TAMBO INT AIRPORT";
						AddAttributesAndNames(cusCode, new KeyValuePair<string, string>("Code", "11"));
						break;
					}

				case "KFN":
					{
						cusCode.ZZD_Description = "KOPFONTEIN";
						AddAttributesAndNames(cusCode, new KeyValuePair<string, string>("Code", "55"));
						break;
					}
			}

			return cusCode;
		}

		public RefCusCodeList CreateZACusCodeListEntry(string codeType, ZString code, string description = null)
		{
			if (string.IsNullOrEmpty(description))
			{
				description = code;
			}

			return CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, codeType, code, description, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		}

		void AddAttributesAndNames(RefCusCodeList cusCodeList, params KeyValuePair<string, string>[] namesAndValues)
		{
			foreach (var nameValue in namesAndValues)
			{
				var name = nameValue.Key;
				var value = nameValue.Value;
				var key = string.Join("|", name.ToUpper(), cusCodeList.ZZD_ZZK_NKCodeType, cusCodeList.ZZD_ZZZ_NKDataGrouping);
				if (!ExistingNames.Contains(key))
				{
					ExistingNames.Add(key);
					CreateNewOrGetExistingRefCusCodeListAttributeName(name, value, cusCodeList.ZZD_ZZK_NKCodeType, cusCodeList.ZZD_ZZZ_NKDataGrouping);
				}

				cusCodeList.Attributes.AddNew(name, value);
			}
		}

		public void AddRefCusCodeListAttribute(BusinessObjectFactory factory, ZString name, ZString code, ZString dataGroupingCode, ZDateTime date)
		{
			var statusCode = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(factory, code, dataGroupingCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, date);
			CreateNewOrGetExistingRefCusCodeListAttributeName(name, "true", statusCode.ZZD_CodeType, statusCode.ZZD_CountryOrGrouping);
		}

		public void SetRefCusCodeListAttribute(BusinessObjectFactory factory, ZString attrName, ZString code, ZString dataGroupingCode, ZDateTime date, ZString value)
		{
			var statusCode = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(factory, code, dataGroupingCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, date);
			var attribute = statusCode?.Attributes?.Find(x => x.ZZE_ZXE_NKName.EqualsIgnoringCase(attrName))?.FirstOrDefault();
			if (attribute != null)
			{
				attribute.ZZE_Value = value;
			}
		}

		RefCusCodeType additionalInformationType;
		RefCusCodeType customsStatusType;
		RefCusCodeType customsOfficeType;
		HashSet<string> ExistingNames => existingNames ?? (existingNames = new HashSet<string>());
		HashSet<string> existingNames;
	}
}
