using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.PL.Business;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.GUI;

class RetrospectiveQuotaRequestMessageSendingStrategy : BaseMessageSendingStrategy
{
	public RetrospectiveQuotaRequestMessageSendingStrategy(BusinessObjectFactory factory, JobDeclaration declaration) : base(factory, declaration) { }

	public override AttachmentMessageSender GetAttachmentSender(BaseMessageSendingObjectParent parent) => null;

	public override BaseMessageSendingObjectParent GetMessageParent() => new RetrospectiveQuotaRequestMessageSendingObjectParent(Declaration);

	public override PLMessageSender GetMessageSender(BaseMessageSendingObject sendingObject, BaseMessageSendingObjectParent parent)
	{
		var requestParent = (RetrospectiveQuotaRequestMessageSendingObjectParent)parent;
		var entryLines = requestParent.EntryLines.Cast<JobDeclarationMessageSendingEntryLine>().Where(x => x.Send);

		return new AISRetrospectiveQuotaMessageSender(SendingMessageFactory, sendingObject, parent,
			entryLines, sendingObject.LocalReferenceNumber, sendingObject.EntryNumber);
	}

	public override MessageSendingForm GetSendingForm(BaseMessageSendingObjectParent parent) =>
		new RetrospectiveQuotaRequestSendingForm((RetrospectiveQuotaRequestMessageSendingObjectParent)parent);
}
