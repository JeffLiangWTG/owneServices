using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TR.Business.Declaration.Testing
{
	public class AddInfoJobDeclarationLookupsTest : EU.Business.Declaration.Testing.AddInfoJobDeclarationLookupsTest
	{
		public void TestCountryList()
		{
			AssertType<RefCountryCollection>(lookups.CountryList);
		}

		public void TestBankList()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			var yesterday = ZDateTime.Today.AddDays(-1);
			var tomorrow = ZDateTime.Today.AddDays(1);
			var eun = helper.CreateNewOrGetExistingDataGrouping(EconomicGroupList.Codes.EuropeanUnion, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Turkey, "Turkey", eun);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TRDeclarationBankCode, "Bank Code");

			helper.CreateCusCodeList(Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TRDeclarationBankCode, "BANK1", yesterday, tomorrow);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TRDeclarationBankCode, "BANK2", yesterday, tomorrow);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TRDeclarationBankCode, "BANK3", yesterday, tomorrow);

			Factory.Save();

			var bankList = lookups.BankCodeList;
			bankList.Load();

			AssertContainsExactElementsInAnyOrder(new[] { "BANK1", "BANK2", "BANK3" }, bankList.Select(x => x.ZZD_Code));
		}

		public void TestTradeTypeList()
		{
			var list = lookups.TradeTypeList;
			AssertEquals(true, list.ContainsCode("ET"));
			AssertEquals(true, list.ContainsCode("ETD"));
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			lookups = declaration.AddInfoLookups;
		}

		JobDeclaration declaration;
		JobDeclarationLookups lookups;
	}
}
