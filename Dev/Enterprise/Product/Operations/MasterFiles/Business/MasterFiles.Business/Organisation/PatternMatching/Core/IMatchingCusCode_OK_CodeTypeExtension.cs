using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.OrgPatternMatching
{
	static class IMatchingCusCode_OK_CodeTypeExtension
	{
		internal static bool IsAcceptedForMatching(this ZString codeType)
		{
			switch (codeType.ToString())
			{
				case OrgCusCode.CodeTypes.GSTCode:
				case OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber:
				case OrgCusCode.CodeTypes.GovBusinessCode:
				case OrgCusCode.CodeTypes.CorporationCode:
				case OrgCusCode.CodeTypes.VATCode:
				case MalaysiaOrgCusCodeInfo.OrgCusCodes.RegistrarOfBusiness:
				case MalaysiaOrgCusCodeInfo.OrgCusCodes.RegistrarOfCompany:
					return true;
				default:
					return false;
			}
		}
	}
}
