//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoNLOrgImpAddInfoValidation
//
//    This class should be used for overriding validation in AutoNLOrgImpAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;

namespace Enterprise.Customs.NL.Business;

public class NLOrgImpAddInfoValidation : AutoNLOrgImpAddInfoValidation
{
	public NLOrgImpAddInfoValidation(AutoNLOrgImpAddInfo parent) : base(parent)
	{
	}
	protected override void CheckZO_VATDeferment()
	{
		base.CheckZO_VATDeferment();
		ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ZO_VATDefermentInfo);
	}
}

