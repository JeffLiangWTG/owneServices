using CargoWise.Common;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NL.Business;

public class CommunicationMetaDataWrapper : ICommunicationMetaData
{
	public CommunicationMetaDataWrapper(CusEntryHeader entryHeader)
	{
		this.entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
	}
	protected readonly CusEntryHeader entryHeader;

	public string ApplicationReferenceId => entryHeader.CH_BGMReference;

	public string CommunicationsAgreementID => NLEDIMessage.MessageNumberPlaceHolder;

	public string PreparationDateTime => ZDateTime.Now.ToString("yyyyMMddHHmmssZ");

	public string RecipientID => NLCustomsRegistry.Instance.CustomsMessageVersion.GetTargetSystemName(MessageVersionRegistry.DMSDomainCode);

	public string SenderID => senderID ??= entryHeader.Declaration.GetSenderInfoCustomsAccount(GlbCompany.CurrentCompany.GC_OH_OrgProxy);
	string senderID;
}
