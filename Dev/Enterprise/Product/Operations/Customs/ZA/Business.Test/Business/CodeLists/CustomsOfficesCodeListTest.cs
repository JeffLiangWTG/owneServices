using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class CustomsOfficesCodeListTest : TestCaseWithFactory
	{
		public void TestGetCodeDescriptionPairList()
		{
			TestCaseHelper.ClearTable(RefCusCodeList.Schema.TableName);
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory);
			testHelper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.BankCode, "BankCode");
			testHelper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			var today = ZDateTime.Today;
			testHelper.CreateCusCodeList(Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "XX1", today.AddDays(-1), today.AddDays(1));
			testHelper.CreateCusCodeList(Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "XX2", today.AddDays(-2), today.AddDays(-1));
			testHelper.CreateCusCodeList(Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "XX3", today.AddDays(-1), today.AddDays(1));
			testHelper.CreateCusCodeList(Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.BankCode, "XX4", today.AddDays(-1), today.AddDays(1));
			testHelper.CreateCusCodeList(Core.Constants.CountryCodes.Australia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "XX4", today.AddDays(-1), today.AddDays(1));
			Factory.Save();
			CombineAssertions(() =>
			{
				var tester = new CustomsOfficesCodeListProvider().GetCodeDescriptionPairList();
				AssertEquals(2, tester.Count);
				AssertContainsExactElementsInAnyOrder(new string[] { "XX1", "XX3" }, tester.GetAllCodes());
			});
		}
	}
}
