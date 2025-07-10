using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class PermitCountrySpecificInstructionTest : TestCaseWithFactory
	{
		public void TestGetTypeList()
		{
			AssertType<PermitTypeList>(countrySpecificInstruction.GetTypeList());
		}

		public void TestSubTypeList()
		{
			AssertType<Customs.Business.PermitSubTypeList>(countrySpecificInstruction.GetSubTypeList(""));
			AssertType<PermitSubTypeList>(countrySpecificInstruction.GetSubTypeList(PermitTypeList.Codes.RCC));
			AssertType<PermitSubTypeList>(countrySpecificInstruction.GetSubTypeList(PermitTypeList.Codes.PRC));
			AssertType<PermitSubTypeList>(countrySpecificInstruction.GetSubTypeList(PermitTypeList.Codes.VALA));
		}

		public void TestQtyValIndicatorList()
		{
			AssertType<Customs.Business.PermitQtyValIndicatorList>(countrySpecificInstruction.GetQtyValIndicatorList("", ""));
			CombineAssertions(() =>
			{
				var rccQtyValList = countrySpecificInstruction.GetQtyValIndicatorList(PermitTypeList.Codes.RCC, "");
				Assert("VAL", rccQtyValList.ContainsCode(Customs.Business.PermitQtyValIndicatorList.Codes.VAL));
				Assert("!QTY", !rccQtyValList.ContainsCode(Customs.Business.PermitQtyValIndicatorList.Codes.QTY));
				Assert("!BTH", !rccQtyValList.ContainsCode(Customs.Business.PermitQtyValIndicatorList.Codes.BTH));
				rccQtyValList = countrySpecificInstruction.GetQtyValIndicatorList(PermitTypeList.Codes.REB, "");
				Assert("the list is empty", rccQtyValList.Count == 0);
			});
		}

		public void TestGetRuleCodeList()
		{
			AssertType<PermitRuleCodeList>(countrySpecificInstruction.GetRuleCodeList("", ""));
		}

		[TestDate(2019, 04, 09)]
		public void TestGetPermitNumberCollection()
		{
			var factory = new BusinessObjectFactory();
			var helper = new ZAUniversalReferenceTestDataHelper(factory, false);
			var tariffType1P1 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1);
			var tariffType3P1 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "3P1");
			var tariffType4P2 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "4P2");
			factory.Save();
			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "1111111111", new ZDateTime(2019, 1, 1), new ZDateTime(2019, 12, 1));
			var tariff2 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType3P1.PK, "2222222222", new ZDateTime(2019, 1, 1), new ZDateTime(2019, 12, 1));
			var tariff3 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType4P2.PK, "3333333333", new ZDateTime(2019, 1, 1), new ZDateTime(2019, 12, 1));
			permitHeader.CPH_Type = PermitTypeList.Codes.REB;
			var collection = countrySpecificInstruction.GetPermitNumberCollection(permitHeader) as TariffViewCollection;
			collection.Load();
			AssertContainsExactElementsInAnyOrder(new ZString[] { tariff2.ZZ1_TariffCode, tariff3.ZZ1_TariffCode }, collection.Select(c => c.ZZ1_TariffCode));
			permitHeader.CPH_Type = PermitTypeList.Codes.IMP;
			AssertNull(countrySpecificInstruction.GetPermitNumberCollection(permitHeader));
		}

		protected override void SetUp()
		{
			base.SetUp();
			permitHeader = Factory.New<CusPermitHeader>();
			countrySpecificInstruction = (PermitCountrySpecificInstruction)permitHeader.CountrySpecificInstruction;
		}

		CusPermitHeader permitHeader;
		PermitCountrySpecificInstruction countrySpecificInstruction;
	}
}
