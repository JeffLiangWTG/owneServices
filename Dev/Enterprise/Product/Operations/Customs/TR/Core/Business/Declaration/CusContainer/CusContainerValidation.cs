using CargoWise.EntityFramework;

namespace Enterprise.Customs.TR.Business.Declaration;

public class CusContainerValidation : EU.Business.Declaration.CusContainerValidation
{
	public CusContainerValidation(CusContainer parent)
		: base(parent)
	{
	}

	protected override void CheckCO_RN_NKOwnerCountry()
	{
		base.CheckCO_RN_NKOwnerCountry();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.CO_RN_NKOwnerCountryInfo);
	}
}
