using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.MessageProcessors.UCMP
{
	public static class UCMPHelper
	{
		public static EDIInterchange GetOutgoingEdiInterchange(EDIInterchange incomingInterchange)
		{
			EDIInterchange result = null;
			if (incomingInterchange != null)
			{
				var outgoingInterchangeQuery = new ZQuery(EDIInterchangeSchema.EI_SessionGUID, incomingInterchange.EI_SessionGUID)
					.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Transmit)
					.AddToFilter(EDIInterchangeSchema.EI_ApplicationCode, incomingInterchange.EI_ApplicationCode);
				result = incomingInterchange.Factory.LoadTop1<EDIInterchange>(outgoingInterchangeQuery);
			}

			return result;
		}
	}
}
