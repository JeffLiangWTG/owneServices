using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.Business;

public class JobDeclarationMessageSendingEntryLineCollection : NonPersistentBusinessObjectCollection<JobDeclarationMessageSendingEntryLine>
{
	public JobDeclarationMessageSendingEntryLineCollection(BusinessObjectFactory factory, RetrospectiveQuotaRequestMessageSendingObjectParent parent) : base(factory)
	{
		Parent = Argument.NotNull(parent, nameof(parent));
	}

	public bool AnyToSend()
	{
		return this.Cast<JobDeclarationMessageSendingEntryLine>().Any(x => x.Send);
	}

	public override void Load()
	{
		RemoveAll();
		var lines = Parent.ParentDeclaration?.CustomsEntryHeaders.SelectMany(x => x.AllEntryLines)
			.Cast<CusEntryLine>().Where(x => !x.QuotaOrderNumber.IsEmpty).Select(x => new JobDeclarationMessageSendingEntryLine(x, this)) ?? Enumerable.Empty<JobDeclarationMessageSendingEntryLine>();
		AddRange(lines);
	}

	protected override bool AllowNewCore => false;

	protected override BusinessObject CreateNonPersistentBusinessObject() => throw new NotImplementedException();

	RetrospectiveQuotaRequestMessageSendingObjectParent Parent { get; }
}
