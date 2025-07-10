using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using RefCusCodeListTypes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(CUSRESEDIMessageLookups))]
sealed class CUSRESEDIMessageLookupsTest : BusinessObjectLookupsTestCase
{
	[TestDate(2024, 10, 25)]
	[RunInExtraTransaction]
	public void TestErrorCodes() => CombineAssertions(() =>
	{
		var helper = new RefCusTariffTestHelper(Factory);
		_ = helper.GetOrCreateCodeList(RefCusCodeListTypes.Codes.ErrorCode, "420", "Yes");
		_ = helper.GetOrCreateCodeList(RefCusCodeListTypes.Codes.CustomsOffice, "666", "Nope");
		_ = helper.GetOrCreateCodeList(RefCusCodeListTypes.Codes.ErrorCode, "777", "Indeed");
		Factory.Save();
		var actualErrorCodes = CreateNewLookups().ErrorCodes;
		AssertEquals("420, Exists", expected: true, actualErrorCodes.ContainsCode("420"));
		AssertEquals("420, Description", "Yes", actualErrorCodes.GetDescriptionFromCode("420"));
		AssertEquals("666, Exists", expected: false, actualErrorCodes.ContainsCode("666"));
		AssertEquals("777, Exists", expected: true, actualErrorCodes.ContainsCode("777"));
		AssertEquals("777, Description", "Indeed", actualErrorCodes.GetDescriptionFromCode("777"));
	});

	CUSRESEDIMessageLookups CreateNewLookups() => new (Factory.New<CUSRESEDIMessage>());
}
