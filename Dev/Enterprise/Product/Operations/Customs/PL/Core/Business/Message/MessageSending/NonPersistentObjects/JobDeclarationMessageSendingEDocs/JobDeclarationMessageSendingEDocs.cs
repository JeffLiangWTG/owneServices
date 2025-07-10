using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.PL.Business;

public class JobDeclarationMessageSendingEDocs : AutoJobDeclarationMessageSendingEDocs
{
	public JobDeclarationMessageSendingEDocs(BusinessObjectFactory factory, JobDeclarationMessageSendingEDocsCollection parentCollection)
		: base(factory)
	{
		this.parentCollection = Argument.NotNull(parentCollection, nameof(parentCollection));
	}

	readonly JobDeclarationMessageSendingEDocsCollection parentCollection;

	[List(nameof(StorageDocs), nameof(ICodeDescription.PK), nameof(ICodeDescription.Code), AllowOnlyTheseValues = true)]
	public override ZGuid EDoc
	{
		get => base.EDoc;
		set
		{
			var oldValue = EDoc;
			base.EDoc = value;
			if (oldValue != EDoc)
			{
				SetEDocNote(EDoc);
				SetEDocFilename(EDoc);
			}
		}
	}

	[ReadOnlyMember(nameof(HasSupportingDocument))]
	[List(nameof(AdditionalInformationList))]
	public override ZString AdditionalInformation
	{
		get => base.AdditionalInformation;
		set
		{
			if (value != base.AdditionalInformation)
			{
				base.AdditionalInformation = value;
				Validation.ValidateSupportingDocument();
			}
		}
	}

	[ReadOnlyMember(nameof(HasAdditionalInformation))]
	[List(nameof(SupportingDocumentList))]
	public override ZString SupportingDocument
	{
		get => base.SupportingDocument;
		set
		{
			if (value != base.SupportingDocument)
			{
				base.SupportingDocument = value;
				Validation.ValidateAdditionalInformation();
			}
		}
	}

	public ZZRefCusCodeListCombinedCollection SupportingDocumentList => Declaration.IsExport ?
		ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Declaration.GetDefaultDataGroupingCode(), Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, ZDate.Today)
		: ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Declaration.GetDefaultDataGroupingCode(), Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, ZDate.Today);

	public ZZRefCusCodeListCombinedCollection AdditionalInformationList => Declaration.IsExport ?
		ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Declaration.GetDefaultDataGroupingCode(), UniversalReferenceConstants.RefCusCodeListType.Codes.AdditionalInformationCodes, ZDate.Today)
		: ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Declaration.GetDefaultDataGroupingCode(), Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, ZDate.Today);

	bool HasAdditionalInformation => !AdditionalInformation.IsEmpty;

	bool HasSupportingDocument => !SupportingDocument.IsEmpty;

	Declaration.JobDeclaration Declaration => parentCollection.Parent.ParentDeclaration;

	void SetEDocNote(ZGuid pk)
	{
		DocumentDescription = parentCollection.DocumentNotes.TryGetValue(pk, out ZString notes) ? notes : ZString.Empty;
	}

	void SetEDocFilename(ZGuid pk)
	{
		if (pk.IsValid
			&& StorageDocs.Count > 0)
		{
			const int defaultDashAmountForFileName = 3;
			const char dash = '-';
			var storageDocFullName = StorageDocs[pk]?.Code ?? ZString.Empty;
			var lastSplitPartOfStorageDocName = storageDocFullName.Split(new[] { dash }, defaultDashAmountForFileName).LastOrDefault();
			Filename = lastSplitPartOfStorageDocName;
		}
	}

	public CodeDescriptionPairList StorageDocs => GetFilteredCodeDescriptionPairList(parentCollection.StorageDocs);

	CodeDescriptionPairList GetFilteredCodeDescriptionPairList(CodeDescriptionPairList list)
	{
		var storageDocsFilteredArray =
			list.ToArray().Where(x =>
			{
				var currentPk = new ZGuid(x.PK);
				return !parentCollection.EDocsInCollection().Contains(currentPk) || currentPk == EDoc;
			});
		var filteredCodeDescriptionPairList = new CodeDescriptionPairList();
		filteredCodeDescriptionPairList.AddPairsEvenIfThisWillCauseDuplicateEntries(storageDocsFilteredArray);
		return filteredCodeDescriptionPairList;
	}

	public ZString Filename { get; private set; }
}
