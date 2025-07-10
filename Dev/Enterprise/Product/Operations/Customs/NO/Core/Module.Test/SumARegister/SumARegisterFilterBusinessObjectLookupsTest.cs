using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.NO.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Module.Testing;

[TestedType(typeof(SumARegisterFilterBusinessObjectLookups))]
sealed class SumARegisterFilterBusinessObjectLookupsTest : TestCaseWithFactory
{
	public void TestOwnerReferenceTypeList()
	{
		var lookups = new SumARegisterFilterBusinessObjectLookups(new SumARegisterFilterBusinessObject());
		var ownerReferenceTypeList = lookups.OwnerReferenceTypeList;

		CombineAssertions(() =>
		{
			AssertSame("Cached", ownerReferenceTypeList, lookups.OwnerReferenceTypeList);
			AssertContainsExactElementsInAnyOrder(new[]
			{
				OwnerReferenceTypeList.Codes._AWB,
				OwnerReferenceTypeList.Codes._ULD,
				OwnerReferenceTypeList.Codes._ZZZ
			}, ownerReferenceTypeList.GetAllCodes());
		});
	}
}
