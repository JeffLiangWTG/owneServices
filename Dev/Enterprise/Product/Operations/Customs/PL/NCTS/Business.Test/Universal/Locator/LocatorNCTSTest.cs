using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.Customs.PL.Business.Testing;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

class LocatorNCTSTest : LocatorTestBase<LocatorNCTS, EDIMessage>
{
	protected override string ExpectedApplicationCodes => ApplicationCodeList.Codes.PLCustomsNCTS;

	protected override string MessageApplicationCode => ApplicationCodeList.Codes.PLCustomsNCTS;

	protected virtual string HeaderType => EU.NCTS.Business.NctsMovementType.Codes.Departure;

	protected override BusinessObject CreateLinkedObject()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_HeaderType = HeaderType;
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

		return nctsHeader;
	}

	protected override void AllocateLRN(BusinessObject linkedObject, string lrn) =>
		((NctsHeader)linkedObject).MovementHeader.BM_PaperlessInbondNum = lrn;

	protected override BusinessObject GetMessageLinkedObject(BusinessObject linkedObject) =>
		((NctsHeader)linkedObject).MovementHeader;
}
