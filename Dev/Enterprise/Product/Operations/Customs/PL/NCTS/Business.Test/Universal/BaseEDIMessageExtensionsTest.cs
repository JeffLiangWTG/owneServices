using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class BaseEDIMessageExtensionsTest : TestCaseWithFactory
{
	public void TestGetRelatedCommonMovementHeader() => CombineAssertions(() =>
	{
		var message = Factory.New<EDIMessage>();
		var nctsHeader = Factory.New<NctsHeader>();

		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		message.EM_LinkedObject = nctsHeader.MovementHeader;
		AssertSame("RelatedCommonMovementHeader for Departure movement", nctsHeader.MovementHeader, message.GetRelatedCommonMovementHeader());

		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(Common.EU.NctsMoveHeaderType.Codes.Arrival);
		message.EM_LinkedObject = nctsHeader;
		AssertSame("RelatedCommonMovementHeader for Arrival movement", nctsHeader.ArrivalMovementHeader, message.GetRelatedCommonMovementHeader());

		message.EM_LinkedObject = null;
		AssertNull("RelatedCommonMovementHeader for empty EM_LinkedObject", message.GetRelatedCommonMovementHeader());
	});

	public void TestGetRelatedNctsHeader() => CombineAssertions(() =>
	{
		var message = Factory.New<EDIMessage>();
		var nctsHeader = Factory.New<NctsHeader>();

		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		message.EM_LinkedObject = nctsHeader.MovementHeader;
		AssertSame("RelatedNctsHeader for Departure movement", nctsHeader, message.GetRelatedNctsHeader());

		nctsHeader.SetMovementType(Common.EU.NctsMoveHeaderType.Codes.Arrival);
		message.EM_LinkedObject = nctsHeader;
		AssertSame("RelatedNctsHeader for Arrival movement", nctsHeader, message.GetRelatedNctsHeader());

		message.EM_LinkedObject = null;
		AssertNull("RelatedNctsHeader for empty EM_LinkedObject", message.GetRelatedNctsHeader());
	});
}
