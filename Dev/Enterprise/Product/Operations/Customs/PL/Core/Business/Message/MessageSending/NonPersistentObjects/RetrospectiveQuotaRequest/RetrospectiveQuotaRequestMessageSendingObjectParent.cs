using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.PL.Business.Declaration;
using static Enterprise.Customs.PL.Business.Constants;

namespace Enterprise.Customs.PL.Business;

public class RetrospectiveQuotaRequestMessageSendingObjectParent : BaseMessageSendingObjectParent
{
	public RetrospectiveQuotaRequestMessageSendingObjectParent(JobDeclaration declaration)
		: base(declaration)
	{
	}

	public bool AnyZCX05 => SendingObjectsCollection.Cast<RetrospectiveQuotaRequestMessageSendingObject>()
		.Any(x => x.Action == MessageSendingObjectActionCodes.ZCX05);

	[ChildEditable(true)]
	public JobDeclarationMessageSendingEntryLineCollection EntryLines
	{
		get
		{
			if (entryLines == null)
			{
				entryLines = CreateNewEntryLineCollection();
				RegisterEditableChildObject(entryLines);
			}
			return entryLines;
		}
	}

	JobDeclarationMessageSendingEntryLineCollection entryLines;

	protected JobDeclarationMessageSendingEntryLineCollection CreateNewEntryLineCollection()
	{
		var collection = new JobDeclarationMessageSendingEntryLineCollection(Factory, this);
		collection.Load();
		return collection;
	}

	protected override Customs.Business.JobDeclarationMessageSendingObject CreateNewJobDeclarationMessageSendingObject(Customs.Business.CusEntryHeader header)
	{
		return new RetrospectiveQuotaRequestMessageSendingObject((CusEntryHeader)header);
	}
}
