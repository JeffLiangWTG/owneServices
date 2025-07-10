using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.Business;

class OrgCusCodeValidation : MasterFiles.Business.OrgCusCodeValidation, Integration.Customs.PL.IOrgCusCodeValidation
{
	public OrgCusCodeValidation(AutoOrgCusCode parent) : base(parent)
	{
	}

	protected override void CheckOK_CustomsRegNo()
	{
		base.CheckOK_CustomsRegNo();

		switch (Parent.OK_CodeType)
		{
			case OrgCusCode.CodeTypes.EDISiteID:
				new EIDValidator().Validate(Parent.OK_CustomsRegNoInfo);
				break;
			case OrgCusCode.PolandCodeTypes.NIP:
				new NIPValidator().Validate(Parent.OK_CustomsRegNoInfo);
				break;
			case OrgCusCode.PolandCodeTypes.PES:
				new PESELValidator().Validate(Parent.OK_CustomsRegNoInfo);
				break;
			case OrgCusCode.PolandCodeTypes.PTU:
				new PTUValidator().Validate(Parent.OK_CustomsRegNoInfo);
				break;
			case OrgCusCode.CodeTypes.GovBusinessCode:
				new REGONValidator().Validate(Parent.OK_CustomsRegNoInfo);
				break;
			case OrgCusCode.PolandCodeTypes.TIN:
				new TINValidator().Validate(Parent.OK_CustomsRegNoInfo);
				break;
		}
	}
}
