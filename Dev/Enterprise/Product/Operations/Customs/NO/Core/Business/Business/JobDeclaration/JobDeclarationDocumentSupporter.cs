using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Integration;
using DataContext = Enterprise.Core.Constants.DataContext;

namespace Enterprise.Customs.NO.Business;

public class JobDeclarationDocumentSupporter : BaseJobDeclarationDocumentSupporter
{
	public JobDeclarationDocumentSupporter(JobDeclaration declaration)
		: base(declaration)
	{
		this.declaration = declaration;
	}
	readonly JobDeclaration declaration;

	protected override DocumentWrapper[] GetDocumentWrappersInternal(DataContext dataContext, IStmMenuItem commandBeingRun)
	{
		switch (dataContext)
		{
			case DataContext.SADH:
				List<DocumentWrapper> result = new List<DocumentWrapper>();
				foreach (CusEntryHeader entryHeader in declaration.CustomsEntryHeaders)
				{
					result.Add(DocumentWrapperFactory.CreateWrapperWithoutException(FullNamespaceForSADHWrapperClass, entryHeader, "Enterprise.Customs.NO.Business"));
				}
				return result.ToArray();
		}
		return base.GetDocumentWrappersInternal(dataContext, commandBeingRun);
	}

	protected override DataContext[] GetSupportedDataContexts()
	{
		return base.GetSupportedDataContexts()
			.Append(DataContext.SADH)
			.ToArray();
	}

	public override TitleCopyCountPair GetDocumentTitlesForPivot(ZString parentDocumentMenuName, IDocumentSupportable parentBusinessObject, IStmMenuTemplatePivot pivot)
	{
		if (parentBusinessObject is CusEntryHeader entryHeader)
		{
			var code = !entryHeader.MovementReferenceNumber.IsEmpty ? (entryHeader.MovementReferenceNumber + entryHeader.CH_EntryReleaseDate.ToString(DateFormat)) : ZDateTime.Now.ToString(DateFormat);
			return new TitleCopyCountPair($"{parentDocumentMenuName} - {code}");
		}
		return base.GetDocumentTitlesForPivot(parentDocumentMenuName, parentBusinessObject, pivot);
	}

	const string FullNamespaceForSADHWrapperClass = "Enterprise.Customs.NO.Business.NODocSADH";
	const string DateFormat = "yyyyMMdd";
}
