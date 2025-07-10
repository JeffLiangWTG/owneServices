using CargoWise.EntityFramework;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.PL.Business;

public class BaseMessageSendingObjectLookups : ZLookups
{
	public BaseMessageSendingObjectLookups(BaseMessageSendingObject parent) : base(parent)
	{
	}

	public CodeDescriptionPairList SecurityList => Factory.GetCachedValue<EU.Business.ExportSecurityTypeList>();

	public CodeDescriptionPairList CorrectionAcceptanceList => Factory.GetCachedValue<MessageSendingObjectCorrectionAcceptanceList>();

	public CodeDescriptionPairList MessageNumList => GetMessagesForResponse();

	CodeDescriptionPairList GetMessagesForResponse()
	{
		var parent = (BaseMessageSendingObject)Parent;
		var result = new CodeDescriptionPairList();
		foreach(var item in parent.GetMessagesForResponse(parent.Action))
		{
			result.AddPair(item, string.Empty);
		}
		return result;
	}
}
