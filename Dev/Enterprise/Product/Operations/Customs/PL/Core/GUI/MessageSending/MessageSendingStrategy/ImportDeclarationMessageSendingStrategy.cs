using CargoWise.EntityFramework;
using Enterprise.Customs.PL.Business;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.GUI;

class ImportDeclarationMessageSendingStrategy : DeclarationMessageSendingStrategy
{
	public ImportDeclarationMessageSendingStrategy(BusinessObjectFactory factory, JobDeclaration declaration) : base(factory, declaration) { }

	public override PLMessageSender GetMessageSender(BaseMessageSendingObject sendingObject, BaseMessageSendingObjectParent parent) =>
		new AISMessageSender(SendingMessageFactory, sendingObject, parent);
}
