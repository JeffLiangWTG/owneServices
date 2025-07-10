using Enterprise.Messaging.Business;

namespace Enterprise.Customs.NO.NCTS.Business;

interface IMessageInformationUpdater
{
	void UpdateInformation(NctsHeader header, EDIMessage message);
}
