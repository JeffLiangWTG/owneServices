using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.US.Business
{
	public static class ForwardingConsolExtensions
	{
		public static bool HasCoLoadData(this ForwardingConsol consol)
		{
			var result = false;
			if (consol != null && consol.JK_AgentType == Core.Constants.AgentType.CoLoad)
			{
				result = !consol.JK_CoLoadMasterBill.IsEmpty ||
					!consol.JK_CoLoadBookingReference.IsEmpty ||
					consol.JK_OA_CreditorAddress.IsValid;
			}
			return result;
		}
	}
}
