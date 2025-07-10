
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.Business;

static class PLOrgHeaderValidationHelper
{
	public static void ValidateHasCustomsRegNo(OrgCusCodeCollection codes, ZString codeType, ZString countryCode, ZPropertyInfo targetInfo)
	{
		if (codes == null || codes.GetCustomsRegNo(codeType, countryCode).IsEmpty)
		{
			if (codeType == OrgCusCode.CodeTypes.GovBusinessCode)
			{
				targetInfo.AddMessageError(Res.GetString("PLOrgHeaderValidationHelper|ValidateHasCustomsRegNoREGON", "Selected Organization doesn't have a valid REGON ({0}) number.", codeType));
			}
			else
			{
				targetInfo.AddMessageError(Res.GetString("PLOrgHeaderValidationHelper|ValidateHasCustomsRegNo", "Selected Organization doesn't have a valid {0} number.", codeType));
			}
		}
	}

	internal static void ValidateOrganizationName(OrgHeader orgHeader, ZPropertyInfo targetInfo)
	{
		const int fullNameMaxLengthForBUS = 70;
		const int lastNameMaxLengthForNAT = 30;

		if (orgHeader is OrgHeader)
		{
			var fullName = orgHeader.OH_FullName;
			var category = orgHeader.OH_Category;

			if (category == OrgConstants.Category.NaturalPersonIndividual)
			{
				var lastName = fullName.Substring(fullName.LastIndexOf(' ') + 1);

				if (lastName == fullName)
				{
					targetInfo.AddMessageError(Res.GetString("9cf938c1-16a8-4dd1-a50a-ceaf48030c67", "Surname is missing in Organization Full name."));
				}
				else if (lastName.Length > lastNameMaxLengthForNAT)
				{
					targetInfo.AddMessageError(Res.GetString("5ad648de-227c-4bff-80c2-eb4abbf9ff14", "The length of the Name/Surname exceeds a maximum of {0} characters and will be truncated in the XML message.", lastNameMaxLengthForNAT));
				}
			}
			else if (category == OrgConstants.Category.Business && fullName.Length > fullNameMaxLengthForBUS)
			{
				targetInfo.AddMessageError(Res.GetString("718cf842-b3d4-4a44-9b96-db7a8e498889", "The length of the organization name exceeds a maximum of {0} characters and will be truncated in the XML message.", fullNameMaxLengthForBUS));
			}
		}
	}
}
