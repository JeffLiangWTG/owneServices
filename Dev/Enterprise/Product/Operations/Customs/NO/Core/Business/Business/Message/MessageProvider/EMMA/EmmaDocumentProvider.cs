using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.NO.Business;

sealed class EmmaDocumentProvider(CusEntryHeader entryHeader)
{
	readonly CusEntryHeader entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
	readonly JobDeclaration declaration = Argument.NotNull(entryHeader.Declaration, nameof(entryHeader.Declaration));

	public IReadOnlyCollection<EmmaDocument> GetDocuments()
	{
		var attachedEDocs = declaration.DocManagerInfo()
			.EDocsView
			.Cast<IeDoc>()
			.Where(e => e.DocType == Core.Constants.RefDocTypes.Invoice);

		if (entryHeader.DocManagerInfo() is { EDocsView: { } entryHeaderEDocs }
			&& entryHeaderEDocs.GetMostRecentEDoc(Core.Constants.RefDocTypes.EntryPrint) is { } sadhEDoc)
		{
			attachedEDocs = attachedEDocs.Prepend(sadhEDoc);
		}

		return attachedEDocs
			.Select(d => new EmmaDocument(GenerateFileName(d), d))
			.ToArray();
	}

	string GenerateFileName(IeDoc eDoc)
	{
		return FormattableString.Invariant($"{eDoc.DocType}{(string.IsNullOrEmpty(entryReleaseNumber) ? string.Empty : "-" + entryReleaseNumber)}-{eDoc.FileName}");
	}

	readonly ZString entryReleaseNumber = entryHeader.EntryReleaseNumber;
}
