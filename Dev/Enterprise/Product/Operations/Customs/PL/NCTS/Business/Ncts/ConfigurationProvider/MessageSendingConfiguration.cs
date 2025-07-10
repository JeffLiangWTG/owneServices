using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.Core;
using EuNctsHeader = Enterprise.Customs.EU.NCTS.Business.NctsHeader;

namespace Enterprise.Customs.PL.NCTS.Business;

public class MessageSendingConfiguration : EU.NCTS.Business.MessageSendingConfiguration
{
	public override CodeDescriptionPairList MessageTypeList(EuNctsHeader header)
	{
		var factory = header.Factory;
		return header.IsDepartureMovement
			? factory.GetCachedValue<DepartureMessageSendingObjectTypeList>()
			: factory.GetCachedValue<ArrivalMessageSendingObjectTypeList>();
	}

	protected override NctsHeaderMessageSendingObjectParent GetNewNctsHeaderMessageSendingObjectParentCore(EuNctsHeader header)
		=> new MessageSendingObjectParent((NctsHeader)header);

	public override string ReleaseRequestCode => DepartureMessageSendingObjectTypeList.Codes.RRL;

	protected override bool ShowJustificationCore(EuNctsHeader header) => false;

	protected override INctsHeaderMessageSendingObjectValidationDecider GetValidationDeciderCore()
	{
		return new NctsHeaderMessageSendingObjectValidationDecider();
	}
}
