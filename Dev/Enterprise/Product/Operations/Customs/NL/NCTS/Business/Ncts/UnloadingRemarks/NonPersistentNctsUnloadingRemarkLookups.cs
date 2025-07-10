using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NL.NCTS.Business;

public class NonPersistentNctsUnloadingRemarkLookups : ZLookups
{
	public NonPersistentNctsUnloadingRemarkLookups(BusinessObject parent) : base(parent)
	{
	}

	public CodeDescriptionPairList CodeList => Factory.GetCachedValue<NctsUnloadingRemarkCodeList>();
}
