using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.PL.Business.Declaration;

public class TranCircumstanceLookups : CusCodeDataLookups
{
	public TranCircumstanceLookups(TranCircumstance code) : base(code)
	{
	}

	public CodeDescriptionPairList TranCircumstancesList => Factory.GetCachedValue<TranCircumstancesList>();

	protected new TranCircumstance Parent => (TranCircumstance)base.Parent;
}
