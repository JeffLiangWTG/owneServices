using CargoWise.EntityFramework;

namespace Enterprise.Customs.NL.Business;

public class AlternativeEvidenceCollection : NonPersistentBusinessObjectCollection<AlternativeEvidence>
{
	public AlternativeEvidenceCollection(JobDeclarationMessageSendingObject jobDeclarationMessageSendingObject) : base(jobDeclarationMessageSendingObject.Factory)
	{
		this.jobDeclarationMessageSendingObject = jobDeclarationMessageSendingObject;
	}

	readonly JobDeclarationMessageSendingObject jobDeclarationMessageSendingObject;

	protected override BusinessObject CreateNonPersistentBusinessObject()
	{
		return new AlternativeEvidence(jobDeclarationMessageSendingObject);
	}
}
