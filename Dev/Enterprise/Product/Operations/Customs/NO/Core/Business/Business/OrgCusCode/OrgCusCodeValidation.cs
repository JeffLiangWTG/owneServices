using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using static Enterprise.Integration.Customs.NO;

namespace Enterprise.Customs.NO.Business;

public sealed class OrgCusCodeValidation(AutoOrgCusCode parent) : MasterFiles.Business.OrgCusCodeValidation(parent), IOrgCusCodeValidation
{
	protected override void CheckOK_CustomsRegNo()
	{
		base.CheckOK_CustomsRegNo();
		var parent = this.parent;
		ValidateCustomsCode(new CustomsRegistrationNumberValidation(), parent.OK_RN_NKCodeCountry, parent.OK_CodeType, parent.OK_CustomsRegNo, parent.OK_CustomsRegNoInfo);
	}

	static void ValidateCustomsCode(CustomsRegistrationNumberValidation customsRegistrationNumberValidation, ZString codeCountry, ZString codeType, ZString customsRegNo, ZPropertyInfo customsRegNoInfo)
	{
		if (codeCountry != Core.Constants.CountryCodes.Norway)
		{
			return;
		}

		switch (codeType)
		{
			case OrgCusCode.CodeTypes.GovBusinessCode:
				customsRegistrationNumberValidation.CheckGBRCustomsRegistrationNumber(customsRegNoInfo, customsRegNo);
				break;
			case OrgCusCode.NorwayCodeTypes.MVA:
				customsRegistrationNumberValidation.CheckMVACustomsRegistrationNumber(customsRegNoInfo, customsRegNo);
				break;
			case OrgCusCode.NorwayCodeTypes.EMD:
				customsRegistrationNumberValidation.CheckEMDCustomsRegistrationNumber(customsRegNoInfo, customsRegNo);
				break;
			default:
				break;
		}
	}

	readonly OrgCusCode parent = (OrgCusCode)parent;
}
