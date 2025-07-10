using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using static Enterprise.Integration.Customs.NL;

namespace Enterprise.Customs.NL.Business;

public class OrgCusCodeValidation : EU.Business.OrgCusCodeValidation, IOrgCusCodeValidation
{
	public OrgCusCodeValidation(OrgCusCode parent) : base(parent)
	{
	}

	protected override void CheckOK_CustomsRegNo()
	{
		base.CheckOK_CustomsRegNo();

		var cusRegCode = Parent.OK_CustomsRegNo;
		if (!cusRegCode.IsEmpty &&
			Parent.OK_CodeType == OrgCusCode.NetherlandsCodeTypes.LFRVATNumberCode
			&& Parent.OK_RN_NKCodeCountry == Core.Constants.CountryCodes.Netherlands)
		{
			if (ValidateControlledPremisesEnding(cusRegCode))
			{
				Parent.OK_CustomsRegNoInfo.AddError(Res.GetString("F4C085AE-5BE8-4B22-8987-2D62ED590A96", "LFR VAT number must end with B02"));
			}
		}
	}
	bool ValidateControlledPremisesEnding(ZString code)
	{
		return !code.EndsWith(LFRVatNumberPostFix, StringComparison.InvariantCultureIgnoreCase);
	}

	const string LFRVatNumberPostFix = "B02";
}
