using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

sealed class NctsArrivalMovementHeaderValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckUnloadingRemarksFreeText()
	{
		var expectedMessage = "Characters '<', '>' and ';' are not allowed.";
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		var arrivalMovement = nctsHeader.ArrivalMovementHeader;
		var unloadingRemarksFreeTextInfo = arrivalMovement.UnloadingRemarksFreeTextInfo;
		CombineAssertions(() =>
		{
			arrivalMovement.UnloadingRemarksFreeText = "Use less than '<' sign";
			AssertHasError("Contains <", unloadingRemarksFreeTextInfo, expectedMessage);
			arrivalMovement.UnloadingRemarksFreeText = "Use larger than '>' sign";
			AssertHasError("Contains >", unloadingRemarksFreeTextInfo, expectedMessage);
			arrivalMovement.UnloadingRemarksFreeText = "Use larger than ';' sign";
			AssertHasError("Contains ;", unloadingRemarksFreeTextInfo, expectedMessage);
			arrivalMovement.UnloadingRemarksFreeText = "Free Text";
			AssertNoErrorContaining("Free Text", unloadingRemarksFreeTextInfo, expectedMessage);
		});
	}
}
