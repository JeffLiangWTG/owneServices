using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(CusClassPartPivotLookups))]
sealed class CusClassPartPivotLookupsTest : TestCaseWithFactory
{
	public void TestCusClassPartPivotLookups() => CombineAssertions(() =>
	{
		AssertEquals("Codes from list", "HTB, HTE, HTI", lookups.ClassificationTypes.CodesAsString);
		AssertSame("Cached", lookups.ClassificationTypes, lookups.ClassificationTypes);
	});

	public void TestVATCodeListLookups() => CombineAssertions(() =>
	{
		var codeList = lookups.VATCodeList;
		AssertSame("Cached", codeList, lookups.VATCodeList);
		AssertEquals("Code from list", string.Empty, codeList.CodesAsString);
	});

	public void TestReducedCustomsFlagListLookups() => CombineAssertions(() =>
	{
		var codeList = lookups.ReducedCustomsFlagList;
		AssertSame("Cached", codeList, lookups.ReducedCustomsFlagList);
		AssertEquals("Code from list", string.Empty, codeList.CodesAsString);
	});

	public void TestCountyOfOriginListLookups() => CombineAssertions(() =>
	{
		var codeList = lookups.CountyOfOriginList;
		AssertSame("Cached", codeList, lookups.CountyOfOriginList);
		AssertEquals("Code from list", string.Empty, codeList.CodesAsString);
	});

	protected override void SetUp()
	{
		base.SetUp();
		pivot = Factory.New<CusClassPartPivot>();
		lookups = pivot.Lookups;
	}

	CusClassPartPivot pivot;
	CusClassPartPivotLookups lookups;
}
