using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.Customs.EU.Business.MessageBuilders;
using Enterprise.Customs.NL.Business.Declaration;

namespace Enterprise.Customs.NL.Business;

public class JobDeclarationMessageSender
{
	readonly JobDeclarationMessageSendingObjectParent decWrapper;
	readonly IEnumerable<JobDeclarationMessageSendingObject> objectsToSend;

	public JobDeclarationMessageSender(JobDeclarationMessageSendingObjectParent decWrapper)
	{
		this.decWrapper = decWrapper;
		objectsToSend = decWrapper.ObjectsToSend;
	}

	public void Send(ISendsMessagesToCustoms sendMessagesToCustoms)
	{
		var declaration = decWrapper.ParentDeclaration;
		SendCore(declaration, sendMessagesToCustoms);
	}

	void SendCore(JobDeclaration declaration, ISendsMessagesToCustoms sendMessagesToCustoms)
	{
		if (declaration != null)
		{
			var entriesToSend = objectsToSend.Select(x => x.Header);
			var generator = GetMessageGenerator();
			var manager = GetDeclarationManager(generator, declaration);
			declaration.SendMessageWithBondedWarehouseAutomation(() => DeclareDeclaration(manager, sendMessagesToCustoms), GetMessageAction(), saveFactory: declaration.Factory.Save, reportHasChanges: false);
		}
	}

	bool DeclareDeclaration(JobDeclarationMessageManager manager, ISendsMessagesToCustoms sendMessagesToCustoms)
	{
		return manager.DeclareDeclaration(sendMessagesToCustoms);
	}

	IMessageGenerator<EU.Business.Declaration.CusEntryHeader> GetMessageGenerator() => new MessageGenerator(decWrapper.ObjectsToSend);

	JobDeclarationMessageManager GetDeclarationManager(IMessageGenerator<EU.Business.Declaration.CusEntryHeader> transmissionGenerator, JobDeclaration declaration)
	{
		return new JobDeclarationMessageManager(declaration, transmissionGenerator);
	}

	MessageAction GetMessageAction() => MessageAction.Original;
}
