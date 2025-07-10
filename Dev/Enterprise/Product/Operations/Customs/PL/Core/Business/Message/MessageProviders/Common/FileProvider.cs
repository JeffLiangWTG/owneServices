using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.PL.Business;

public class FileProvider : IFile
{
	public FileProvider(JobDeclarationMessageSendingEDocs eDoc)
	{
		this.eDoc = Argument.NotNull(eDoc, nameof(eDoc));
	}

	readonly JobDeclarationMessageSendingEDocs eDoc;

	public string Name => eDoc.Filename;

	public string Code => CachedValueHelper.GetValue(ref code, () =>
	{
		var supportingDocument = eDoc.SupportingDocument;
		return supportingDocument.IsEmpty
			? eDoc.AdditionalInformation
			: supportingDocument;
	});
	CachedValue<string> code;

	public string Description => eDoc.DocumentDescription;
}
