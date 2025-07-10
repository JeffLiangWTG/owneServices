using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.Business;

public class AttachmentProvider : IAttachments
{
	public AttachmentProvider(BaseMessageSendingObjectParent messageSendingObjectParent)
	{
		this.messageSendingObjectParent = Argument.NotNull(messageSendingObjectParent, nameof(messageSendingObjectParent));
	}

	readonly BaseMessageSendingObjectParent messageSendingObjectParent;

	public string Number => CachedValueHelper.GetValue(ref number, () => new ZGuid().ToString());
	CachedValue<string> number;

	public string ReferenceNumber => messageSendingObjectParent.PurposeOfSending == MessageSendingPurposeOfSendingList.Codes._0
		? messageSendingObjectParent.RefNumber
		: ZString.Empty;

	public string ReferenceNumber2 => messageSendingObjectParent.PurposeOfSending == MessageSendingPurposeOfSendingList.Codes._1
		? messageSendingObjectParent.MrnNumber
		: ZString.Empty;

	public string Office => CachedValueHelper.GetValue(ref office, () => messageSendingObjectParent.CustomsOffice.SubstringSafe(2));
	CachedValue<string> office;

	public string System => CachedValueHelper.GetValue(ref system, () =>
	{
		var declaration = messageSendingObjectParent.ParentDeclaration;
		return declaration.IsExport ? Constants.ExportText : Constants.ImportText;
	});
	CachedValue<string> system;

	public string PurposeOfSending => messageSendingObjectParent.PurposeOfSending;

	public string Procedure => messageSendingObjectParent.Procedure;

	public string Comments => messageSendingObjectParent.Comments;

	public IReadOnlyCollection<IFile> Files => files
												?? (files = messageSendingObjectParent.EDocs.Cast<JobDeclarationMessageSendingEDocs>()
													.Select(x => new FileProvider(x))
													.ToArray());
	IReadOnlyCollection<IFile> files;
}
