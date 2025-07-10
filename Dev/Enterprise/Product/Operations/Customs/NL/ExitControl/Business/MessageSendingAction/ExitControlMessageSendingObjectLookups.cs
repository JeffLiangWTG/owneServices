using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NL.ExitControl.Business;

public class ExitControlMessageSendingObjectLookups : ZLookups
{
	public ExitControlMessageSendingObjectLookups(BusinessObject parent) : base(parent)
	{
	}

	public CodeDescriptionPairList EntryTypeList => Factory.GetCachedValue<EntryTypeList>();
}
