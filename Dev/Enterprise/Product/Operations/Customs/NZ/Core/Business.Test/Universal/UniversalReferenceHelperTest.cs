using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NZ.Business.Testing;

public class UniversalReferenceHelperTest : TestCaseWithFactory
{
	public void TestGetUNEPackageTypeList()
	{
		for (var i = 0; i < 10; i++)
		{
			helper.CreateCusCodeList("UNE", "UNPKG", "Code " + i, UniversalReferenceConstants.UNPackTypeStartDate.AddDays(-1), ZDateTime.Today.AddDays(1));
		}
		helper.CreateCusCodeList("UNE", "UNPKG", "Expired Code", UniversalReferenceConstants.UNPackTypeStartDate.AddDays(-3), ZDateTime.Today.AddDays(-1));
		helper.CreateCusCodeList("UNE", "UNPKG", "Future Code", UniversalReferenceConstants.UNPackTypeStartDate.AddDays(1), ZDateTime.Today.AddDays(3));
		helper.CreateCusCodeList("UNE", "DIFFF", "Different Type", UniversalReferenceConstants.UNPackTypeStartDate.AddDays(-1), ZDateTime.Today.AddDays(1));
		helper.CreateCusCodeList("DIF", "UNPKG", "Different Grouping", UniversalReferenceConstants.UNPackTypeStartDate.AddDays(-1), ZDateTime.Today.AddDays(1));
		Factory.Save();

		var list = UniversalReferenceHelper.GetUNEPackageTypeList(Factory);

		CombineAssertions(() =>
		{
			AssertEquals("Valid codes included", 10, list.GetAllCodes().Count(c => c.StartsWith("Code ")));
			Assert("Expired code not included", !list.ContainsCode("Expired Code"));
			Assert("Future code not included", !list.ContainsCode("Future Code"));
			Assert("Different type not included", !list.ContainsCode("Different Type"));
			Assert("Different grouping not included", !list.ContainsCode("Different Grouping"));
		});
	}

	public void TestGetUNEPackageTypeListCaching()
	{
		var list1 = UniversalReferenceHelper.GetUNEPackageTypeList(Factory);
		var list2 = UniversalReferenceHelper.GetUNEPackageTypeList(Factory);
		AssertSame(list1, list2);
	}

	public void TestGetUNEPackageTypeListOrderedByCode()
	{
		var random = new Random();
		foreach (char c in Enumerable.Range('A', 26).OrderBy(_ => random.Next()))
		{
			helper.CreateCusCodeList("UNE", "UNPKG", "CODE " + c, UniversalReferenceConstants.UNPackTypeStartDate.AddDays(-1), ZDateTime.Today.AddDays(1));
		}
		Factory.Save();

		var list = UniversalReferenceHelper.GetUNEPackageTypeList(Factory);

		for (var i = 0; i < list.Count - 1; i++)
		{
			AssertLessThan(list[i].Code, list[i + 1].Code);
		}
	}

	public void TestIsBulkType()
	{
		var notBulk = helper.CreateCusCodeList("UNE", "UNPKG", "Not bulk", UniversalReferenceConstants.UNPackTypeStartDate.AddMonths(-1), UniversalReferenceConstants.UNPackTypeStartDate.AddMonths(1));
		var isBulk = helper.CreateCusCodeListWithAttribute("UNE", "UNPKG", "Is bulk", "Description", UniversalReferenceConstants.UNPackTypeStartDate.AddMonths(-1), UniversalReferenceConstants.UNPackTypeStartDate.AddMonths(1), "BULK", "BULK");
		var hasWrongValue = helper.CreateCusCodeListWithAttribute("UNE", "UNPKG", "Has wrong value", "Description", UniversalReferenceConstants.UNPackTypeStartDate.AddMonths(-1), UniversalReferenceConstants.UNPackTypeStartDate.AddMonths(1), "BULK", "N");
		Factory.Save();

		AssertEquals(false, UniversalReferenceHelper.UNEPackageTypeIsBulk(Factory, notBulk.ZZD_Code));
		AssertEquals(true, UniversalReferenceHelper.UNEPackageTypeIsBulk(Factory, isBulk.ZZD_Code));
		AssertEquals(true, UniversalReferenceHelper.UNEPackageTypeIsBulk(Factory, hasWrongValue.ZZD_Code));
	}

	UniversalReferenceTestDataHelper helper;

	protected override void SetUp()
	{
		base.SetUp();
		helper = new UniversalReferenceTestDataHelper(Factory);
	}

	public static void InitialiseUNEPackageTypeList(BusinessObjectFactory factory, params string[] codes)
	{
		InitialiseUNEPackageTypeList(factory, codes.Select(code => new CodeDescriptionPair(code, code + " DESC")).ToArray());
	}

	public static void InitialiseUNEPackageTypeList(BusinessObjectFactory factory, params CodeDescriptionPair[] pairs)
	{
		var refDataHelper = new UniversalReferenceTestDataHelper(factory);
		refDataHelper.CreateNewOrGetExistingDataGrouping("UNE");
		refDataHelper.CreateNewOrGetExistingCusCodeType("UNPKG", "UNPKG", "UNE");
		foreach (var pair in pairs)
		{
			refDataHelper.CreateNewOrGetExistingCusCodeList("UNE", "UNPKG", pair.Code, pair.Description, UniversalReferenceConstants.UNPackTypeStartDate, ZDateTime.Today.AddDays(1));
		}
	}
}
