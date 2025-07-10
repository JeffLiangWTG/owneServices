using CargoWise.EntityFramework;
using Enterprise.Customs.PL.Business;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.GUI;

class ExportDeclarationMessageSendingStrategy : DeclarationMessageSendingStrategy
{
	public ExportDeclarationMessageSendingStrategy(BusinessObjectFactory factory, JobDeclaration declaration) : base(factory, declaration) { }

	public override PLMessageSender GetMessageSender(BaseMessageSendingObject sendingObject, BaseMessageSendingObjectParent parent) =>
		new AESMessageSender(SendingMessageFactory, sendingObject, parent);
}
