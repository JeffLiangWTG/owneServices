using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.PL.Business;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.GUI;

abstract class BaseMessageSendingStrategy
{
	protected BaseMessageSendingStrategy(BusinessObjectFactory factory, JobDeclaration declaration)
	{
		SendingMessageFactory = Argument.NotNull(factory, nameof(factory));
		Declaration = Argument.NotNull(declaration, nameof(declaration));
	}

	protected JobDeclaration Declaration { get; }

	protected BusinessObjectFactory SendingMessageFactory { get; }

	public abstract BaseMessageSendingObjectParent GetMessageParent();

	public abstract MessageSendingForm GetSendingForm(BaseMessageSendingObjectParent parent);

	public abstract PLMessageSender GetMessageSender(BaseMessageSendingObject sendingObject, BaseMessageSendingObjectParent parent);

	public abstract AttachmentMessageSender GetAttachmentSender(BaseMessageSendingObjectParent parent);
}
