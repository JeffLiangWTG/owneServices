using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.PL.Business;

public class JobDeclarationMessageSendingEDocsCollection : NonPersistentBusinessObjectCollection<JobDeclarationMessageSendingEDocs>
{
	readonly Lazy<CodeDescriptionPairList> storageDocs;
	readonly Lazy<Dictionary<ZGuid, ZString>> documentNotes;

	public JobDeclarationMessageSendingEDocsCollection(BusinessObjectFactory factory, Func<CodeDescriptionPairList> getStorageDocs,
		BaseMessageSendingObjectParent parent, Func<Dictionary<ZGuid, ZString>> getDocumentNotes) : base(factory)
	{
		storageDocs = new Lazy<CodeDescriptionPairList>(Argument.NotNull(getStorageDocs, nameof(getStorageDocs)));
		documentNotes = new Lazy<Dictionary<ZGuid, ZString>>(Argument.NotNull(getDocumentNotes, nameof(getDocumentNotes)));
		Parent = Argument.NotNull(parent, nameof(parent));
	}

	public readonly BaseMessageSendingObjectParent Parent;

	public IReadOnlyDictionary<ZGuid, ZString> DocumentNotes => documentNotes.Value;

	public CodeDescriptionPairList StorageDocs => storageDocs.Value;

	public IEnumerable<ZGuid> EDocsInCollection() => this.Cast<JobDeclarationMessageSendingEDocs>().Select(x => x.EDoc);

	protected override BusinessObject CreateNonPersistentBusinessObject() => new JobDeclarationMessageSendingEDocs(Factory, this);
}
