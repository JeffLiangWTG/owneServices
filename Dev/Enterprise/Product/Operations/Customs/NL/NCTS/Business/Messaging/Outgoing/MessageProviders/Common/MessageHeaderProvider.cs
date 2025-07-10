using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.NL.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NL.NCTS.Business;

public class MessageHeaderProvider : IMessageHeader
{
	protected readonly NctsHeader nctsHeader;

	public MessageHeaderProvider(NctsHeader nctsHeader)
	{
		this.nctsHeader = Argument.NotNull(nctsHeader, nameof(nctsHeader));
	}

	public string MessageSender => NLCustomsRegistry.Instance.SenderIDs.Value.Cast<SenderInfo>().FirstOrDefault(si => si.OrganizationPK == GlbCompany.CurrentCompany.GC_OH_OrgProxy)?.SenderID;

	public string MessageRecipient => NLCustomsRegistry.Instance.CustomsMessageVersion.GetTargetSystemName(MessageVersionRegistry.NCTSP5DomainCode);

	public DateTime PreparationDateTime => DateTime.ParseExact(ZDateTime.Now.ToBestReadableDateTimeString(), (NoResString)"dd MMM yyyy HH:mm", null);

	public string MessageIdentification => EDIMessage.MessageNumberPlaceHolder;

	public virtual string MessageType => ZString.Empty;

	public string CorrelationIdentifier => null;
}
