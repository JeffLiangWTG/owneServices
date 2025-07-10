using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.PL.NCTS.Business;

public class MessageSendingObjectLookups : EU.NCTS.Business.NctsHeaderMessageSendingObjectLookups
{
	public MessageSendingObjectLookups(MessageSendingObject parent)
		: base(parent)
	{
	}

	public CodeDescriptionPairList AmendmentTypeList => Factory.GetCachedValue<AmendmentTypeList>();

	public CodeDescriptionPairList TirPageNumberTypeList => Factory.GetCachedValue<TirPageNumberTypeList>();

	public CodeDescriptionPairList TirUnloadingNumberTypeList => Factory.GetCachedValue<TirUnloadingNumberTypeList>();
}
