using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.PL.Business;

public class RetrospectiveQuotaRequestMessageSendingObjectValidation : BaseMessageSendingObjectValidation
{
	public RetrospectiveQuotaRequestMessageSendingObjectValidation(AutoJobDeclarationMessageSendingObject parent)
		: base(parent)
	{
	}

	protected override void CheckEntryNumber()
	{
		MandatoryValidation.CheckEntered(Parent.EntryNumberInfo);
	}
}
