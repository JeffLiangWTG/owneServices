using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.PL.Business.Declaration;

public class CusEntryLineFeeLookups : EU.Business.Declaration.CusEntryLineFeeLookups
{
	public CusEntryLineFeeLookups(EU.Business.Declaration.CusEntryLineFee parent) : base(parent)
	{
	}

	public override CodeDescriptionPairList MethodOfPaymentList => Factory.GetCachedValue<PLMethodOfPaymentList>();
}
