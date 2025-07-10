using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	public class LPCODetailWrapper : ILPCODetail
	{
		public LPCODetailWrapper(ZString lpcoID, ZString lpcoAuthorizedPartyID, ZString lpcoAuthorizedPartyTypeCode)
		{
			LPCOID = lpcoID;
			LPCOAuthorizedParty = new LPCOAuthorizedPartyWrapper(SharedHelper.GetIDStartWithNO(lpcoAuthorizedPartyID, lpcoAuthorizedPartyTypeCode), lpcoAuthorizedPartyTypeCode);
		}

		public ZString LPCOExemptionCode { get; }

		public ZString LPCOID { get; }

		public ILPCOAuthorizedParty LPCOAuthorizedParty { get; }
	}
}
