using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	public class ApprovalDocumentWrapper : ILPCODetail
	{
		public ApprovalDocumentWrapper(ZString lpcoExemptionCode, ZString lpcoID, ZString lpcoAuthorizedPartyID, ZString lpcoAuthorizedPartyTypeCode)
		{
			LPCOExemptionCode = lpcoExemptionCode;
			LPCOID = lpcoID;
			LPCOAuthorizedPartyID = lpcoAuthorizedPartyID;
			LPCOAuthorizedPartyTypeCode = lpcoAuthorizedPartyTypeCode;
		}

		public ZString LPCOExemptionCode { get; }

		public ZString LPCOID { get; }

		ZString LPCOAuthorizedPartyID { get; }

		ZString LPCOAuthorizedPartyTypeCode { get; }

		public ILPCOAuthorizedParty LPCOAuthorizedParty => new LPCOAuthorizedPartyWrapper(SharedHelper.GetIDStartWithNO(LPCOAuthorizedPartyID, LPCOAuthorizedPartyTypeCode), LPCOAuthorizedPartyTypeCode);
	}
}
