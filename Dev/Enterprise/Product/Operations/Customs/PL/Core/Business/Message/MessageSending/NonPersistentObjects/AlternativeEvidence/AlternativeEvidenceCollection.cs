using CargoWise.EntityFramework;

namespace Enterprise.Customs.PL.Business;

public class AlternativeEvidenceCollection(BaseMessageSendingObjectParent messageSendingObjectParent) : NonPersistentBusinessObjectCollection<AlternativeEvidence>(messageSendingObjectParent.Factory)
{
	protected override BusinessObject CreateNonPersistentBusinessObject() => new AlternativeEvidence(messageSendingObjectParent);
}
