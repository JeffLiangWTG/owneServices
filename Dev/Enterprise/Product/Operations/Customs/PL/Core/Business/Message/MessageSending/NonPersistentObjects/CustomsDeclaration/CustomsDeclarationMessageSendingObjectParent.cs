using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.Business;

public class CustomsDeclarationMessageSendingObjectParent : BaseMessageSendingObjectParent
{
	public CustomsDeclarationMessageSendingObjectParent(JobDeclaration declaration)
		: base(declaration)
	{
	}

	protected override Customs.Business.JobDeclarationMessageSendingObject CreateNewJobDeclarationMessageSendingObject(Customs.Business.CusEntryHeader header)
	{
		return new CustomsDeclarationMessageSendingObject((CusEntryHeader)header);
	}
}
