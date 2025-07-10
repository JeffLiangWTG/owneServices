using System;

namespace Enterprise.MasterFiles.Business
{
	public interface IDocsAndCartageParent
	{
		IHaveRequiredDocuments RequiredDocumentsProvider { get; }
		Type DocsAndCartageParentType { get; }
		Type DocsAndCartageType { get; }
	}
}