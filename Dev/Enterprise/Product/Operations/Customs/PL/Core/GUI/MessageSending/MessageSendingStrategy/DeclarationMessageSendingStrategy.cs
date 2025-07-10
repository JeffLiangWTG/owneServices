using CargoWise.EntityFramework;
using Enterprise.Customs.PL.Business;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.GUI;

abstract class DeclarationMessageSendingStrategy : BaseMessageSendingStrategy
{
	public DeclarationMessageSendingStrategy(BusinessObjectFactory factory, JobDeclaration declaration) : base(factory, declaration) { }

	public override AttachmentMessageSender GetAttachmentSender(BaseMessageSendingObjectParent parent) => new AttachmentMessageSender(SendingMessageFactory, parent);

	public override BaseMessageSendingObjectParent GetMessageParent() => new CustomsDeclarationMessageSendingObjectParent(Declaration);

	public override MessageSendingForm GetSendingForm(BaseMessageSendingObjectParent parent) => new CustomsDeclarationSendingForm((CustomsDeclarationMessageSendingObjectParent)parent);
}
