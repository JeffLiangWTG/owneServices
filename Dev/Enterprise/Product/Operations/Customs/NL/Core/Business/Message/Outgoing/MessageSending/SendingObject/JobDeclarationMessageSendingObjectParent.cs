using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.NL.Business;

public class JobDeclarationMessageSendingObjectParent : JobDeclarationMessageSendingObjectParent<JobDeclarationMessageSendingObject>
{
	public JobDeclarationMessageSendingObjectParent(BaseJobDeclaration declaration) : base(declaration)
	{
	}

	public new Declaration.JobDeclaration ParentDeclaration => (Declaration.JobDeclaration)base.ParentDeclaration;

	protected override Customs.Business.JobDeclarationMessageSendingObject CreateNewJobDeclarationMessageSendingObject(CusEntryHeader header)
	{
		var nlHeader = header as EU.Business.Declaration.CusEntryHeader;
		if (nlHeader != null)
		{
			return new JobDeclarationMessageSendingObject(nlHeader);
		}
		else
		{
			return base.CreateNewJobDeclarationMessageSendingObject(header);
		}
	}

	public override IEnumerable<MessageSendingObjectProperty> MessageSendingObjectProperties => new List<MessageSendingObjectProperty>
	{
		new MessageSendingObjectProperty(JobDeclarationMessageSendingObject.NLSchema.Update, false, 50),
		new MessageSendingObjectProperty(JobDeclarationMessageSendingObject.Schema.MessageType, true, 70),
		new MessageSendingObjectProperty(JobDeclarationMessageSendingObject.Schema.EntryType, true, 70),
		new MessageSendingObjectProperty(JobDeclarationMessageSendingObject.NLSchema.SubStyle, true, 70),
		new MessageSendingObjectProperty(JobDeclarationMessageSendingObject.NLSchema.Description, true, 175),
		new MessageSendingObjectProperty(JobDeclarationMessageSendingObject.NLSchema.Date, true, 100),
		new MessageSendingObjectProperty(JobDeclarationMessageSendingObject.Schema.EntryStatus, true, 70)
	};

	protected NonPersistentBusinessObjectCollection<JobDeclarationMessageSendingObject> SendingObjectsCollectionCore
	{
		get
		{
			var result = new JobDeclarationMessageSendingObjectCollection<JobDeclarationMessageSendingObject>(Factory);
			foreach (Declaration.CusEntryHeader header in ParentDeclaration.ActiveEntryHeaders)
			{
				result.Add(new JobDeclarationMessageSendingObject(header));
			}
			return result;
		}
	}

	public IEnumerable<JobDeclarationMessageSendingObject> ObjectsToSend
	{
		get => SendingObjectsCollection.OfType<JobDeclarationMessageSendingObject>().Where(x => x.ShouldSend);
	}
}
