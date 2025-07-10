using CargoWise.EntityFramework;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class LocatorNCTSArrivalTest : LocatorNCTSTest
{
	protected override string HeaderType => EU.NCTS.Business.NctsMovementType.Codes.Arrival;

	protected override void AllocateLRN(BusinessObject linkedObject, string lrn) =>
		((NctsHeader)linkedObject).ArrivalMovementHeader.BM_PaperlessInbondNum = lrn;

	protected override BusinessObject GetMessageLinkedObject(BusinessObject linkedObject) => linkedObject;
}
