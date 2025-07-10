using CargoWise.Types;
using Enterprise.Customs.Business.BusinessObjects.CusPermit;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.Business.MessagingProcess
{
	public interface ISupportPermitProcessing
	{
		ICusPermitCusDecProcessor<EDIMessage> PermitProcessor { get; }
		ZString GetPermitAppIdForMessage(EDIMessage message);
	}
}
